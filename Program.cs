using Faysal.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using static Faysal.Helpers.Faysal;

var builder = WebApplication.CreateBuilder(args);

// Persist Data Protection keys (stable cookie encryption)
var keysDir = Path.Combine(builder.Environment.ContentRootPath, "dpkeys");
Directory.CreateDirectory(keysDir);
builder.Services.AddDataProtection()
    .SetApplicationName("FaysalAuth")
    .PersistKeysToFileSystem(new DirectoryInfo(keysDir));

// Services
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Auth (cookie)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = ".Faysal.Auth";
    // Keep SameAsRequest to mirror your current behavior (set Always in prod if desired)
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

builder.Services.AddAuthorization();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1); // match parity with first app
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Build
var app = builder.Build();

// Env-specific pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Honor proxy headers (so external HTTPS is recognized)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
    // Optionally set KnownProxies/KnownNetworks if you want to restrict sources.
});

// Match first app’s stance on HTTPS redirection (disabled if TLS is terminated upstream)
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication(); // authenticate BEFORE custom middleware & authorization

// Force-logout when SessionVersion changed
app.Use(async (ctx, next) =>
{
    try
    {
        var uid = Faysal.Helpers.WebSecurity.CurrentUserId;
        if (uid > 0)
        {
            var claimSv = ctx.User.FindFirst("sv")?.Value;
            int claimVersion = int.TryParse(claimSv, out var v) ? v : -1;

            var db = Faysal.Helpers.Database.Open("faysal"); // use faysal for this app
            var row = db.QuerySingleOrDefault("SELECT SessionVersion FROM UserProfile WHERE UserId=@0", uid);
            int dbVersion = (int)(row?.SessionVersion ?? 0);

            if (claimVersion != dbVersion)
            {
                await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                ctx.Response.Redirect("/Login?reason=revoked");
                return;
            }
        }
    }
    catch (Exception ex)
    {
        var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("RevocationMiddleware");
        logger.LogError(ex, "Revocation check failed; continuing without enforcement.");
        // continue
    }
    if (!SessionBootstrapper.IsInitialized(ctx))           // or allow anon cart/session
    {
        await SessionBootstrapper.InitializeAsync(ctx);
    }

    await next();
});

app.UseAuthorization();

// Configure helpers after DI is available
var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
var configuration = app.Services.GetRequiredService<IConfiguration>();
Database.Configure(configuration);
WebSecurity.Configure(httpContextAccessor, configuration);
AppFunctions.Configure(configuration, httpContextAccessor);

// Map endpoints
app.MapRazorPages();
app.Run();
