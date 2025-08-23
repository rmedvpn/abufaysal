using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Faysal.Helpers;              // ← for AppFunctions
using Microsoft.AspNetCore.Http;     // ← for IHttpContextAccessor

var builder = WebApplication.CreateBuilder(args);

// 1) Register services
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 🔧 Hook up our Database shim:
Faysal.Helpers.Database.Configure(builder.Configuration);

var app = builder.Build();

// 2) Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// 3) Initialize AppFunctions so _connectionString is set
AppFunctions.Configure(
    builder.Configuration,
    app.Services.GetRequiredService<IHttpContextAccessor>()
);

// 4) Map endpoints
app.MapRazorPages();
app.Run();
