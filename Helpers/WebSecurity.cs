using System;
using System.Linq;
using System.Security.Claims;
using Dapper;
using Faysal.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Faysal.Helpers
{
    public static class WebSecurity
    {
        private static IHttpContextAccessor? _http;
        private static IConfiguration? _config;

        public static void Configure(IHttpContextAccessor http, IConfiguration config)
        {
            _http = http;
            _config = config;
            Initialize();
        }

        public static void Initialize()
        {
            var db = Database.Open("faysal");

            db.Execute(@"
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserProfile')
        BEGIN
            CREATE TABLE UserProfile (
                UserId INT IDENTITY(1,1) PRIMARY KEY,
                UserName NVARCHAR(56) NOT NULL
            );
        END;

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'webpages_Membership')
        BEGIN
            CREATE TABLE webpages_Membership (
                UserId INT PRIMARY KEY,
                CreateDate DATETIME DEFAULT GETDATE(),
                ConfirmationToken NVARCHAR(128),
                IsConfirmed BIT DEFAULT 0,
                LastPasswordFailureDate DATETIME,
                PasswordFailuresSinceLastSuccess INT DEFAULT 0,
                Password NVARCHAR(128) NOT NULL,
                PasswordSalt NVARCHAR(128),
                PasswordChangedDate DATETIME,
                PasswordVerificationToken NVARCHAR(128),
                PasswordVerificationTokenExpirationDate DATETIME
            );
        END;

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'webpages_Roles')
        BEGIN
            CREATE TABLE webpages_Roles (
                RoleId INT IDENTITY(1,1) PRIMARY KEY,
                RoleName NVARCHAR(256) NOT NULL
            );
        END;

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'webpages_UsersInRoles')
        BEGIN
            CREATE TABLE webpages_UsersInRoles (
                UserId INT,
                RoleId INT,
                PRIMARY KEY (UserId, RoleId)
            );
        END;

        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'webpages_OAuthMembership')
        BEGIN
            CREATE TABLE webpages_OAuthMembership (
                Provider NVARCHAR(30) NOT NULL,
                ProviderUserId NVARCHAR(100) NOT NULL,
                UserId INT NOT NULL,
                PRIMARY KEY (Provider, ProviderUserId)
            );
        END;
    "
);

            // ✅ Ensure default admin exists
            DefaultAdminSeeder.Ensure(db);
        }

        public static bool Login(string username, string password, bool rememberMe = false)
        {
            var db = Database.Open("faysal");

            var user = db.QuerySingleOrDefault(@"
        SELECT u.UserId, u.UserName, u.SessionVersion, u.IsBlocked, u.BlockUntilUtc,
               m.Password
        FROM UserProfile u
        INNER JOIN webpages_Membership m ON u.UserId = m.UserId
        WHERE u.UserName = @0", username);

            if (user == null) return false;

            // 🔒 Block check (permanent or temporary)
            bool isBlocked = false; try { isBlocked = Convert.ToBoolean(user.IsBlocked); } catch { }
            DateTime? blockUntil = null; try { blockUntil = (DateTime?)user.BlockUntilUtc; } catch { }
            if (isBlocked || (blockUntil.HasValue && DateTime.UtcNow < blockUntil.Value))
                return false;

            // Password check (your current simple compare)
            string storedPassword = Convert.ToString(user.Password)?.Trim() ?? "";
            if (storedPassword != password) return false;

            int u_id = (int)user.UserId;
            int sessionVersion = 0; try { sessionVersion = (int)user.SessionVersion; } catch { }

            var identity = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, u_id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName.ToString()),
        new Claim("sv", sessionVersion.ToString())
    }, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties { IsPersistent = rememberMe };
            if (rememberMe) authProps.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);

            _http!.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps
            ).GetAwaiter().GetResult();

            _http.HttpContext.User = principal; // so CurrentUserId works immediately
            return true;
        }

        public static void Logout()
        {
            _http?.HttpContext?.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
        }

        public static bool IsAuthenticated => _http?.HttpContext?.User?.Identity?.IsAuthenticated == true;

        public static string CurrentUserName => _http?.HttpContext?.User?.Identity?.Name ?? "";

        public static int CurrentUserId => int.TryParse(_http?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

        private static class DefaultAdminSeeder
        {
            public static void Ensure(dynamic db)
            {
                var existing = db.QuerySingleOrDefault("SELECT * FROM UserProfile WHERE UserName=@0", "Admin");
                if (existing != null)
                    return;

                db.Execute("INSERT INTO UserProfile (UserName) VALUES (@0)", "Admin");

                var row = db.QuerySingle("SELECT UserId FROM UserProfile WHERE UserName=@0", "Admin");
                int newId = row.UserId;

                db.Execute(@"
    INSERT INTO webpages_Membership (UserId, Password, PasswordSalt)
    VALUES (@UserId, @Password, @PasswordSalt)",
                    new
                    {
                        UserId = newId,
                        Password = "123456",
                        PasswordSalt = "staticSalt"
                    });

                var roleId = db.QuerySingleOrDefault<int?>("SELECT RoleId FROM webpages_Roles WHERE RoleName=@0", "Admin");
                if (roleId == null)
                {
                    db.Execute("INSERT INTO webpages_Roles (RoleName) VALUES (@0)", "Admin");
                    roleId = db.QuerySingle("SELECT RoleId FROM webpages_Roles WHERE RoleName=@0", "Admin");
                }

                db.Execute("INSERT INTO webpages_UsersInRoles (UserId, RoleId) VALUES (@0, @1)", newId, roleId);
            }
        }
    }
}
