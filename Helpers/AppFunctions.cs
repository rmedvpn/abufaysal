using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Faysal.Helpers
{
    /// <summary>
    /// Copy your .cshtml helper logic into a real C# class.
    /// Call AppFunctions.Configure(...) once at startup.
    /// </summary>
    public static class AppFunctions
    {
        private static string _connectionString;
        private static IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initialize the helper with DI-provided services.
        /// </summary>
        public static void Configure(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("faysal");
            _httpContextAccessor = httpContextAccessor;
        }
        public static string Clean(string s)
        {
            s = (s ?? "").Trim();
            // remove common zero-width marks (LRM/RLM)
            return s.Replace("\u200E", "").Replace("\u200F", "");
        }


        public static void DoLogin(int u_id)
        {
            var db = Database.Open("faysal");
            DateTime local_time = LocalTime();
            var sqlSelect = "UPDATE users SET last_login=@0,last_online=@0 WHERE u_id=@1";
            db.Execute(sqlSelect, local_time, u_id);
            db.Close();
        }
        public static void DoLogout(int u_id)
        {
            var db = Database.Open("faysal");
            DateTime local_time = LocalTime();
            var sqlSelect = "UPDATE users SET last_logout=@0 WHERE u_id=@1";
            db.Execute(sqlSelect, local_time, u_id);
            sqlSelect = "DELETE FROM cartsettings WHERE u_id=@0";
           // db.Execute(sqlSelect, u_id);
            sqlSelect = "DELETE FROM Cart WHERE u_id=@0";
            db.Execute(sqlSelect, u_id);

            db.Close();
        }


        public static bool ValidateEmail(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var email = input.Trim();

            // Quick shape check to reject obvious bad inputs
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
                return false;

            // Must have exactly one '@'
            int at = email.IndexOf('@');
            if (at <= 0 || at != email.LastIndexOf('@') || at == email.Length - 1)
                return false;

            var local = email[..at];
            var domain = email[(at + 1)..];

            // Local part: no leading/trailing dot, no consecutive dots
            if (local.StartsWith('.') || local.EndsWith('.') || local.Contains(".."))
                return false;

            // Domain: no leading/trailing dot, no consecutive dots
            if (domain.StartsWith('.') || domain.EndsWith('.') || domain.Contains(".."))
                return false;

            // Normalize Unicode domain to ASCII (Punycode). Fails if domain is invalid.
            string asciiDomain;
            try
            {
                asciiDomain = new IdnMapping().GetAscii(domain);
            }
            catch
            {
                return false;
            }

            // Label-level checks (lengths, hyphens)
            var labels = asciiDomain.Split('.');
            if (labels.Any(l => l.Length == 0 || l.Length > 63 || l.StartsWith('-') || l.EndsWith('-')))
                return false;

            // TLD length >= 2 (loose but practical)
            if (labels[^1].Length < 2)
                return false;

            // Common total length guard
            if ((local.Length + 1 + asciiDomain.Length) > 254)
                return false;

            // Final parse (uses ASCII domain to avoid false negatives with Unicode)
            try
            {
                var _ = new MailAddress(local + "@" + asciiDomain);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Must be at least 6 characters long And no longer than 20 characters
            if (username.Length < 6 || username.Length > 20)
                return false;

            // First character must be a letter
            if (!char.IsLetter(username[0]))
                return false;

            // All characters must be letters or digits
            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c))
                    return false;
            }

            return true;
        }


        public static string GetAppProperty(string property)
        {
            var db = Database.Open("faysal");
            var sql = "SELECT propertyValue FROM AppProperties WHERE propertyName=@0";
            var value = db.QuerySingleOrDefault<string>(sql, property);
            return value ?? string.Empty;
        }

        public static void SetAppProperty(string property, string theValue)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                db.Execute(
                    "UPDATE AppProperties SET propertyValue=@Value WHERE propertyName=@Property",
                    new { Value = theValue, Property = property }
                );
            }
        }

        public static int HowManyDaysAgoWas(DateTime theDate)
        {
            return (DateTime.Now - theDate).Days;
        }

        public static int HowManyYearsAgoWas(DateTime theDate)
        {
            return Convert.ToInt32((DateTime.Now - theDate).Days / 365m);
        }

        public static DateTime LocalTime(string localCode = "IL")
        {
            var utcNow = DateTime.UtcNow;
            var propName = $"UTCDIFF-{localCode}".ToUpperInvariant();
            if (!int.TryParse(GetAppProperty(propName), out var utcDiff))
                utcDiff = 0;
            return utcNow.AddHours(utcDiff);
        }

        public static void WriteWebStats(string rec_action, int site_index, int ref_id = 0, int ref_type = 0, int aff_id = 0)
        {
            var ts = LocalTime();
            var session = _httpContextAccessor?.HttpContext?.Session;
            var sessionId = session?.Id ?? string.Empty;
            session?.SetString("tmp", ts.ToString());

            using (var db = new SqlConnection(_connectionString))
            {
                var exists = db.QuerySingleOrDefault<int?>(
                    "SELECT serial FROM joinstats WHERE session_id=@SessionId AND rec_action=@Action",
                    new { SessionId = sessionId, Action = rec_action }
                );
                if (exists == null)
                {
                    db.Execute(
                        "INSERT INTO joinStats(ts,ref_id,ref_type,rec_action,session_id,site_index) VALUES(@Ts,@RefId,@RefType,@Action,@SessionId,@SiteIndex)",
                        new { Ts = ts, RefId = ref_id, RefType = ref_type, Action = rec_action, SessionId = sessionId, SiteIndex = site_index }
                    );
                }
            }
        }

        public static string Wanumize(string wanum)
        {
            var wanumized = new string(wanum.Where(char.IsDigit).ToArray());
            if (wanumized.StartsWith("0"))
                wanumized = "972" + wanumized.Substring(1);

            var prefixes = new[] { "50", "51", "52", "53", "54", "55", "56", "57", "58", "59" };
            if (prefixes.Any(p => wanumized.StartsWith(p)))
                wanumized = "972" + wanumized;

            return wanumized;
        }

        public static void WriteDebugLine(string txt)
        {
            // For now, we’ll hard-code user ID = 0
            const int userId = 0;

            using (var db = new SqlConnection(_connectionString))
            {
                db.Open();
                db.Execute(
                    "INSERT INTO debugTbl(ts, txt, u_id) VALUES(@Ts, @Txt, @Uid)",
                    new
                    {
                        Ts = LocalTime(),
                        Txt = txt,
                        Uid = userId
                    }
                );
            }
        }


    }
}
