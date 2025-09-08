using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Globalization;

namespace Faysal.Helpers
{
    public static class Faysal
    {
        private static string Param(HttpContext context, string key)
        {
            var request = context.Request;
            return request.HasFormContentType && request.Form.ContainsKey(key)
                ? request.Form[key].ToString()
                : request.Query.ContainsKey(key)
                    ? request.Query[key].ToString()
                    : "";
        }

        public static class SessionBootstrapper
        {
            private const string InitFlag = "Session_InitDone";
            private static readonly CultureInfo HeIl = new CultureInfo("he-IL");

            public static bool IsInitialized(HttpContext ctx)
                => ctx?.Session?.GetString(InitFlag) == "1";

            public static async Task InitializeAsync(HttpContext ctx)
            {
                // Guard: no session? bail.
                if (ctx?.Session == null) return;

                // Already initialized? bail.
                if (IsInitialized(ctx)) return;

                // ---- Pull data you need ONCE per session ----
                // App-wide settings you use frequently
                var defaultShippingCharge = GetDecimalAppProp("DefaultShippingCharge");
                var freeShippingMarker = GetDecimalAppProp("FreeShippingMarker");

                // Current user (if logged in)
                int currentUserId = 0;
                try { currentUserId = WebSecurity.CurrentUserId; } catch { }

                // Minimal user profile payload (customize these fields to your schema)
                int? sessionVersion = null;
                bool? isBlocked = null;
                DateTime? blockUntilUtc = null;

                if (currentUserId > 0)
                {
                    using var db = Database.Open("faysal");
                    try
                    {
                        // Pull only what you need often
                        var row = db.QuerySingleOrDefault(
                            "SELECT user_id, SessionVersion, IsBlocked, BlockUntilUtc FROM UserProfile WHERE user_id=@0",
                            currentUserId
                        );
                        if (row != null)
                        {
                            try { sessionVersion = (int?)row.SessionVersion; } catch { }
                            try { isBlocked = (bool?)row.IsBlocked; } catch { }
                            try { blockUntilUtc = (DateTime?)row.BlockUntilUtc; } catch { }
                        }
                    }
                    finally { db.Close(); }
                }

                // Optionally: prime the cart totals now (or skip and compute lazily on first cart view)
                CartReCalculate(ctx); 

                // ---- Store into session ----
                // App settings
                ctx.Session.SetString("Default_Shipping_Charge", defaultShippingCharge.ToString(HeIl));
                ctx.Session.SetString("Default_Free_Shipping_Marker", freeShippingMarker.ToString(HeIl));

                // User info
                ctx.Session.SetInt32("User_Id", currentUserId);
                if (sessionVersion.HasValue) ctx.Session.SetInt32("User_SessionVersion", sessionVersion.Value);
                if (isBlocked.HasValue) ctx.Session.SetString("User_IsBlocked", isBlocked.Value ? "1" : "0");
                if (blockUntilUtc.HasValue) ctx.Session.SetString("User_BlockUntilUtc", blockUntilUtc.Value.ToString("o"));

                // Mark as initialized
                ctx.Session.SetString(InitFlag, "1");

                await Task.CompletedTask;
            }

            private static decimal GetDecimalAppProp(string key)
            {
                try
                {
                    var s = AppFunctions.GetAppProperty(key);
                    if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)) return d;
                    if (decimal.TryParse(s, NumberStyles.Number, new CultureInfo("he-IL"), out d)) return d;
                }
                catch { }
                return 0m;
            }

        }


        public static string GoTmp(HttpContext context)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            string sessionId = context.Session.Id;
            int member_id = 0;
            DateTime local_time = AppFunctions.LocalTime();
            string theHtmlOutput = "";
            var db = Database.Open("faysal");
            var old_faysal = Database.Open("old_faysal");
            string sqlSelect = "SELECT * FROM StrainImages order by file_id";
            var srcData = old_faysal.Query(sqlSelect);

            db.Execute("DELETE FROM StrainImages");
            db.Execute("SET IDENTITY_INSERT StrainImages ON");

            foreach (var row in srcData)
            {
                sqlSelect = "INSERT INTO StrainImages(file_id,strain_id,full_path,fileName,timestamp,clerk_id,is_deleted,fileContent,MimeType,is_tn,ImageThumb,is_primary,is_show) ";
                sqlSelect += "VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12)";
                db.Execute(sqlSelect, row.file_id,row.strain_id, row.full_path, row.fileName, row.timestamp, row.clerk_id, row.is_deleted, row.fileContent, row.MimeType, row.is_tn, row.ImageThumb, row.is_primary, row.is_show);
            }


            db.Execute("SET IDENTITY_INSERT StrainImages OFF");
            theHtmlOutput = "OK!";
            db.Close();
            old_faysal.Close();
            return theHtmlOutput;
        }


        public static int RoundUpAmount(decimal amount)
        {
            int rounded5 = (int)Math.Round(amount / 5m) * 5;
            int rounded10 = (int)Math.Round(amount / 10m) * 10;
            decimal diff5 = Math.Abs(amount - rounded5);
            decimal diff10 = Math.Abs(amount - rounded10);
            return (diff5 == diff10) ? Math.Max(rounded5, rounded10) : (diff5 < diff10 ? rounded5 : rounded10);
        }


        public static string RegisterProspect(HttpContext context)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            DateTime local_time = AppFunctions.LocalTime();

            int u_id = WebSecurity.CurrentUserId;
            string sessionId = context.Session.Id;

            string theHtmlOutput = "";
            var db = Database.Open("faysal");
            var sqlSelect = "";

            bool is_valid_entry = true;
            string validationFailureMsg = "!$!";
            string prospect_name = Param(context, "prospect_name");
            DateTime birth_date = Convert.ToDateTime("1/1/1900");
            int gender = 0; try { gender = Convert.ToInt32(Param(context, "gender")); } catch { }
            string city = Param(context, "city");
            string wanum = Param(context, "wanum");
            string email = Param(context, "email");
            string tlg_nick = Param(context, "tlg_nick");
            string fb = Param(context, "fb");
            string instagram = Param(context, "instagram");
            string ref_source = Param(context, "ref_source");
            string occupation = Param(context, "occupation");
            string more_info = Param(context, "more_info");
            string involvement = Param(context, "involvement");
            string inv_other_desc = Param(context, "inv_other_desc");
            string ref_source_desc = Param(context, "ref_source_desc");
            string ref_source_other = Param(context, "ref_source_other");
            int ref_id = 0; try { ref_id = Convert.ToInt32(Param(context, "ref_id")); } catch { }
            int aff_id = 0; try { aff_id = Convert.ToInt32(Param(context, "aff_id")); } catch { }
            string usage_habits = ""; usage_habits = Param(context, "usage_habits");
            string ref_txt = "";
            bool is_license = false; if (Param(context, "is_license") == "yes") { is_license = true; }
            string license_info = Param(context, "license_info");
            bool involvement_volunteer = false; if (string.IsNullOrWhiteSpace(Param(context, "involvement_volunteer"))) { involvement_volunteer = true; }
            bool involvement_paid = false; if (string.IsNullOrWhiteSpace(Param(context, "involvement_paid"))) { involvement_paid = true; }
            bool involvement_social = false; if (string.IsNullOrWhiteSpace(Param(context, "involvement_social"))) { involvement_social = true; }
            bool involvement_other = false; if (string.IsNullOrWhiteSpace(Param(context, "involvement_other"))) { involvement_other = true; }
            bool is_terms = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_terms"))) { is_terms = true; }
            string involvement_other_info = Param(context, "involvement_other_info");
            string lang = Param(context, "lang");
            try { birth_date = Convert.ToDateTime(Param(context, "birth_date")); } catch { birth_date = Convert.ToDateTime("1/1/1900"); }

            int age = 0;



            if (birth_date != Convert.ToDateTime("1/1/1900"))
            {
                DateTime zeroTime = new DateTime(1, 1, 1);
                TimeSpan span = local_time - birth_date;
                age = (zeroTime + span).Year - 1;
            }

            if (lang == "en")
            {
                if (prospect_name == "") { is_valid_entry = false; validationFailureMsg += "Full name is a required field!<br>"; }
                if (city == "") { is_valid_entry = false; validationFailureMsg += "City is a required field!<br>"; }
                if (birth_date == Convert.ToDateTime("1/1/1900") || age < 0) { is_valid_entry = false; validationFailureMsg += "Please enter a valid birth date!<BR>"; }
                if (age < 18 && age > 0) { is_valid_entry = false; validationFailureMsg += "18 is the minimum age to join us!<br>"; }

                if (wanum == "") { is_valid_entry = false; validationFailureMsg += "WhatsApp# is a required field!<br>"; }
                if (email == "" || !AppFunctions.ValidateEmail(email)) { is_valid_entry = false; validationFailureMsg += "Please enter a valid email address!<br>"; }

                if (ref_source == "") { is_valid_entry = false; validationFailureMsg += "Please specify how did you find us!<br>"; }
                if (ref_source == "other" && ref_source_other == "") { is_valid_entry = false; validationFailureMsg += "Please specify how did you find us!<br>"; }
                //if (usage_habits == "") { is_valid_entry = false; validationFailureMsg += "Usage habbits is a required field!<br>"; }
                if (ref_source == "other") { ref_source_desc = ref_source_other; }
                if (!is_terms) { is_valid_entry = false; validationFailureMsg += "You are required to consent to the terms of use!<br>"; }
            }
            else
            {
                if (prospect_name == "") { is_valid_entry = false; validationFailureMsg += "שם הוא שדה חובה!<br>"; }
                if (city == "") { is_valid_entry = false; validationFailureMsg += "עיר הוא שדה חובה!<br>"; }
                if (birth_date == Convert.ToDateTime("1/1/1900") || age < 0) { is_valid_entry = false; validationFailureMsg += "תאריך לידה לא תקין!<BR>"; }
                if (age < 18 && age > 0) { is_valid_entry = false; validationFailureMsg += "ההצטרפות מגיל 18 בלבד!<br>"; }

                if (wanum == "") { is_valid_entry = false; validationFailureMsg += "מספר ווטסאפ הוא שדה חובה!<br>"; }
                if (email == "" || !AppFunctions.ValidateEmail(email)) { is_valid_entry = false; validationFailureMsg += "חובה להזין כתובת מייל תקינה!<br>"; }

                if (ref_source == "") { is_valid_entry = false; validationFailureMsg += "חובה לציין איך הגעת אלינו!<br>"; }
                if (ref_source == "other" && ref_source_other == "") { is_valid_entry = false; validationFailureMsg += "חובה לפרט איך הגעת אלינו!<br>"; }
                //if (usage_habits == "") { is_valid_entry = false; validationFailureMsg += "חובה לציין הרגלי צריכה!<br>"; }
                if (ref_source == "other") { ref_source_desc = ref_source_other; }
                if (!is_terms) { is_valid_entry = false; validationFailureMsg += "חייבים להסכים לתנאי השימוש!<br>"; }
            }


            if (is_valid_entry)
            {
                try { wanum = AppFunctions.Wanumize(wanum); } catch { }
                sqlSelect = "SELECT serial FROM webprospects WHERE wanum=@0";
                var checkDup = db.QuerySingle(sqlSelect, wanum);
                if (checkDup == null)
                {
                    sqlSelect = "INSERT INTO WebProspects (ts,prospect_name,birth_date,gender,city,wanum,email,fb,instagram,tlg_nick,ref_txt,occupation,refer_id,ref_source,usage_habits,ref_source_desc,more_info,is_license,license_info,involvement_volunteer,involvement_paid,involvement_social,involvement_other,involvement_other_info) VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23)";
                    db.Execute(sqlSelect, local_time, prospect_name, birth_date, gender, city, wanum, email, fb, instagram, tlg_nick, ref_txt, occupation, ref_id, ref_source, usage_habits, ref_source_desc, more_info, is_license, license_info, involvement_volunteer, involvement_paid, involvement_social, involvement_other, involvement_other_info);
                    // AppFunctions.WriteWebStats("LEAD", 1, ref_id);
                    theHtmlOutput = "הכל תקין!";
                }
                else
                {
                    if (lang == "en") { validationFailureMsg += "The WhatsApp# you are trying to register already exists in our system!<br>"; }
                    else { validationFailureMsg += "מספר הטלפון שהוזן כבר קיים במערכת!<br>"; }

                    theHtmlOutput = validationFailureMsg;
                }


            }
            else
            {
                theHtmlOutput = validationFailureMsg;

            }

            db.Close();


            return theHtmlOutput;
        }


        public static string CheckRegistrationCode(HttpContext context)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            DateTime local_time = AppFunctions.LocalTime();

            int u_id = WebSecurity.CurrentUserId;
            string sessionId = context.Session.Id;
            string pa_code = Param(context, "pa_code");
            bool is_found = false;

            string theHtmlOutput = "";
            if (pa_code != "")
            {
                var db = Database.Open("faysal");
                var sqlSelect = "SELECT * FROM UserPreApprovals WHERE pa_code=@0 AND is_registered=0";
                var preapproval = db.QuerySingle(sqlSelect, pa_code);
                if (preapproval != null)
                {
                    is_found = true;
                }
                db.Close();

            }

            if (is_found) { theHtmlOutput = pa_code; }
            else { theHtmlOutput = "!$!קוד שגוי - נא לפנות למוקד!"; }




            return theHtmlOutput;
        }


        public static string UserSignUp(HttpContext context)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            DateTime local_time = AppFunctions.LocalTime();
            DateTime defDate = Convert.ToDateTime("1/1/1900");

            int u_id = WebSecurity.CurrentUserId;
            string sessionId = context.Session.Id;

            string theHtmlOutput = "";
            bool is_valid_entry = true;
            string validationFailureMsg = "!$!";

            string UserName = Param(context, "UserName");
            string member_nick = "";
            string member_name = Param(context, "member_name");
            string wanum = Param(context, "wanum");
            string email = Param(context, "email");
            string tlg_nick = Param(context, "telegram_nick");
            string password = Param(context, "password");
            string password_confirm = Param(context, "password_confirm");
            string pa_code = Param(context, "pa_code");
            string lang = Param(context, "lang");
            bool is_terms = !string.IsNullOrEmpty(Param(context, "is_terms"));
            bool is_statement = !string.IsNullOrEmpty(Param(context, "is_statement"));
            int member_id = 0; try { member_id = Convert.ToInt32(Param(context, "member_id")); } catch { }
            int gender = 0; try { gender = Convert.ToInt32(Param(context, "gender")); } catch { }
            int PreApprovalId = 0; try { PreApprovalId = Convert.ToInt32(Param(context, "PreApprovalId")); } catch { }
            DateTime birth_date = defDate; try { birth_date = Convert.ToDateTime(Param(context, "birth_date")); } catch { }
            int age = 0;
            if (birth_date != defDate)
            {
                DateTime zeroTime = new DateTime(1, 1, 1);
                TimeSpan span = local_time - birth_date;
                age = (zeroTime + span).Year - 1;
            }
            lang = "he";

            if (lang == "en")
            {
                if (member_name == "") { is_valid_entry = false; validationFailureMsg += "Member Name is a required field!<br>"; }

                if (wanum == "") { is_valid_entry = false; validationFailureMsg += "WhatsApp# is a required field!<br>"; }
                if (birth_date == defDate) { is_valid_entry = false; validationFailureMsg += "Birth Date is a required field!<br>"; }
                if (age < 18 && age > 0) { is_valid_entry = false; validationFailureMsg += "18 is the minimum age to join us!<br>"; }

                if (email == "" || !AppFunctions.ValidateEmail(email)) { is_valid_entry = false; validationFailureMsg += "Email is a required field!<br>"; }
                if (UserName == "") { is_valid_entry = false; validationFailureMsg += "User Name is a required field!<br>"; }
                else { if (!AppFunctions.IsValidUsername(UserName)) { is_valid_entry = false; validationFailureMsg += "User Name Must contain at least 6 alphanumeric charachters and start with a letter!<br>"; } }
                if (password == "")
                {
                    is_valid_entry = false;
                    validationFailureMsg += "Password is a required field!<br>";
                }
                else
                {
                    if (password_confirm != password) { is_valid_entry = false; validationFailureMsg += "Passwords do not match!<br>"; }
                }
             //   if (!is_statement) { is_valid_entry = false; validationFailureMsg += "You must agree to the personal statement!<br>"; }

                if (!is_terms) { is_valid_entry = false; validationFailureMsg += "You must accept the terms of use!<br>"; }
            }
            if (lang == "he")
            {
                if (member_name == "") { is_valid_entry = false; validationFailureMsg += "שם מלא הוא שדה חובה!<br>"; }
                if (wanum == "") { is_valid_entry = false; validationFailureMsg += "מספר ווטסאפ הוא שדה חובה!<br>"; }
                if (birth_date == defDate) { is_valid_entry = false; validationFailureMsg += "תאריך לידה הוא שדה חובה!<br>"; }
                if (age < 18 && age > 0) { is_valid_entry = false; validationFailureMsg += "ההרשמה מגיל 18 בלבד!<br>"; }

                if (email == "" || !AppFunctions.ValidateEmail(email)) { is_valid_entry = false; validationFailureMsg += "כתובת מייל הוא שדה חובה!<br>"; }
                if (UserName == "") { is_valid_entry = false; validationFailureMsg += "שם משתמש הוא שדה חובה!<br>"; }
                else { if (!AppFunctions.IsValidUsername(UserName)) { is_valid_entry = false; validationFailureMsg += "שם משתמש חייב להיות בין 6-20 תווים, לא לכלול תווים מיוחדים, ולהתחיל באות ולא במספר!<br>"; } }
                if (password == "")
                {
                    is_valid_entry = false;
                    validationFailureMsg += "סיסמה היא שדה חובה!<br>";
                }
                else
                {
                    if (password_confirm != password) { is_valid_entry = false; validationFailureMsg += "הסיסמאות אינן תואמות!<br>"; }
                }
              //  if (!is_statement) { is_valid_entry = false; validationFailureMsg += "חובה להסכים להצהרה האישית!<br>"; }
                if (!is_terms) { is_valid_entry = false; validationFailureMsg += "חובה לקבל את תנאי השימוש!<br>"; }

            }


            if (is_valid_entry)
            {

                var db = Database.Open("Faysal");
                var sqlSelect = "";

                UserName = AppFunctions.Clean(UserName);
                // Check if username already exists



                sqlSelect = "SELECT UserName FROM UserProfile WHERE UserName=@0";
                var exists = db.QuerySingle(sqlSelect, UserName);
                if (exists != null) { is_valid_entry = false; if (lang == "en") { validationFailureMsg += "User Name already exists!<br>"; } else { validationFailureMsg += "שם המשתמש כבר קיים!<br>"; } }

                wanum = AppFunctions.Wanumize(wanum);





                if (is_valid_entry)
                {
                    try { member_nick=member_name.Split(' ')[0]; } catch { }
                    string wanum_mask = ""; try { wanum_mask = Faysal.MaskString(wanum); } catch { }
                    string email_mask = ""; try { email_mask = Faysal.MaskString(email); } catch { }
                    string tlg_mask = ""; try { tlg_mask = Faysal.MaskString(tlg_nick); } catch { }



                    // Insert user
                    db.Execute("INSERT INTO UserProfile (UserName,ts,member_id) VALUES (@0, @1,@2)", UserName, local_time, member_id);
                    int userId = db.QueryValue<int>("SELECT UserId FROM UserProfile WHERE UserName=@0", UserName);
                    db.Execute("INSERT INTO webpages_Membership (UserId, Password) VALUES (@0, @1)", userId, password);

                    sqlSelect = "INSERT INTO users (User_Name, email, wanum,ts,u_id,member_id,last_login,last_online,member_nick,telegram_nick,wanum_mask,email_mask,tlg_mask,full_name,gender,birth_date) VALUES (@0, @1, @2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15)";
                    db.Execute(sqlSelect, UserName, email, wanum, local_time, userId, member_id, local_time, local_time, member_nick, tlg_nick, wanum_mask, email_mask, tlg_mask,member_name, gender, birth_date);

                    sqlSelect = "UPDATE UserPreApprovals SET is_registered=1,register_ts=@0,is_update=1,member_name='',wanum='',email='',telegram_nick='' WHERE serial=@1";
                    db.Execute(sqlSelect, local_time, PreApprovalId);
                  //  db.Execute("INSERT INTO members (UserName, email, wanum,phone,create_ts,u_id) VALUES (@0, @1, @2,@2,@3,@4)", UserName, email, wanum, local_time, userId);
                    // Optionally login
                    WebSecurity.Login(UserName, password, true);
                    //    AppFunctions.DoLogin(userId);

                }
                else
                {
                    theHtmlOutput = validationFailureMsg;

                }


                /*
                try { wanum = AppFunctions.Wanumize(wanum); } catch { }
                var m_join = Database.Open("zion");
                sqlSelect = "SELECT serial FROM webprospects WHERE wanum=@0";
                var checkDup = m_join.QuerySingle(sqlSelect, wanum);
                if (checkDup == null)
                {
                    sqlSelect = "INSERT INTO WebProspects (ts,prospect_name,birth_date,gender,city,wanum,email,fb,instagram,tlg_nick,ref_txt,occupation,refer_id,ref_source,usage_habits,ref_source_desc,more_info,is_license,license_info,involvement_volunteer,involvement_paid,involvement_social,involvement_other,involvement_other_info) VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23)";
              //      m_join.Execute(sqlSelect, local_time, prospect_name, birth_date, gender, city, wanum, email, fb, instagram, tlg_nick, ref_txt, occupation, ref_id, ref_source, usage_habits, ref_source_desc, more_info, is_license, license_info, involvement_volunteer, involvement_paid, involvement_social, involvement_other, involvement_other_info);
                   // AppFunctions.WriteWebStats("LEAD", 1, ref_id);
                    theHtmlOutput = "!$!הכל תקין!";
                }
                else
                {
                    if (lang == "en") { validationFailureMsg += "The WhatsApp# you are trying to register already exists in our system, we will contact you shortly!<br>"; }
                    else { validationFailureMsg += "מספר הטלפון שהוזן כבר קיים, בקרוב ניצור איתך קשר!<br>"; }

                    theHtmlOutput = validationFailureMsg;
                }

                m_join.Close();
                */
            }
            else
            {
                theHtmlOutput = validationFailureMsg;

            }



            return theHtmlOutput;
        }



        public static string UpdateSettings(HttpContext context)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            string theHtmlOutput = "";
            var db = Database.Open("faysal");
            var sqlSelect = "";

            string theValue = ""; theValue = Param(context, "value");
            int rec_id = 0; try { rec_id = Convert.ToInt32(Param(context, "rec_id")); } catch { }

            switch (theValue)
            {
                case "QUAN":
                    {
                        int quan = 0; try { quan = Convert.ToInt32(Param(context, "quan")); } catch { }
                        string title = Param(context, "title");
                        string title_e = Param(context, "title_e");
                        decimal labor_cost = 0; try { labor_cost = Convert.ToDecimal(Param(context, "labor_cost")); } catch { }
                        decimal multiplier = 0; try { multiplier = Convert.ToDecimal(Param(context, "multiplier")); } catch { }
                        decimal labor_charge = 0;
                        bool is_active = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_active"))) { is_active = true; }
                        bool is_add = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_add"))) { is_add = true; }
                        int labor_min = 0;
                        int quanOptionId = rec_id;
                        if (quan <= 0) { theHtmlOutput += "חובה להזין כמות תקינה!<br>"; }
                        if (string.IsNullOrWhiteSpace(title)) { theHtmlOutput += "חובה להזין כותרת תקינה!<br>"; }
                        if (string.IsNullOrWhiteSpace(title_e)) { theHtmlOutput += "חובה להזין כותרת תקינה באנגלית!<br>"; }
                        if (labor_cost <= 0) { theHtmlOutput += "חובה להזין עלות הכנה תקינה!<br>"; }
                        if (multiplier <= 0) { theHtmlOutput += "חובה להזין מכפיל תקין!<br>"; }

                        if (theHtmlOutput == "")
                        {
                            labor_charge = labor_cost * multiplier;

                            if (is_add)
                            {
                                sqlSelect = "INSERT INTO quanOptions(quan,title,title_e,labor_cost,multiplier,labor_charge,labor_min,is_active) VALUES(@0,@1,@2,@3,@4,@5,@6,@7)";
                                db.Execute(sqlSelect, quan, title, title_e, labor_cost, multiplier, labor_charge, labor_min, 1);
                            }
                            else
                            {
                                sqlSelect = "UPDATE quanOptions SET quan=@0,title=@1,title_e=@2,labor_cost=@3,multiplier=@4,labor_charge=@5,labor_min=@6,is_active=@7 WHERE id=@8";
                                db.Execute(sqlSelect, quan, title, title_e, labor_cost, multiplier, labor_charge, labor_min, is_active, quanOptionId);
                            }
                        }
                    }
                    break;

                case "Strain":
                    {
                        int quan = 0; try { quan = Convert.ToInt32(Param(context, "quan")); } catch { }
                        string title = Param(context, "title");
                        string title_e = Param(context, "title_e");
                        decimal gram_cost = 0; try { gram_cost = Convert.ToDecimal(Param(context, "gram_cost")); } catch { }
                        decimal multiplier = 0; try { multiplier = Convert.ToDecimal(Param(context, "multiplier")); } catch { }
                        decimal gram_charge = 0;
                        bool is_active = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_active"))) { is_active = true; }
                        bool is_add = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_add"))) { is_add = true; }
                        int strainOptionId = rec_id;
                        if (string.IsNullOrWhiteSpace(title)) { theHtmlOutput += "חובה להזין כותרת תקינה!<br>"; }
                        if (string.IsNullOrWhiteSpace(title_e)) { theHtmlOutput += "חובה להזין כותרת תקינה באנגלית!<br>"; }
                        if (gram_cost <= 0) { theHtmlOutput += "חובה להזין עלות גרם תקינה!<br>"; }
                        if (multiplier <= 0) { theHtmlOutput += "חובה להזין מכפיל תקין!<br>"; }

                        if (theHtmlOutput == "")
                        {
                            gram_charge = gram_cost * multiplier;

                            if (is_add)
                            {
                                sqlSelect = "INSERT INTO strainOptions(title,title_e,gram_cost,multiplier,gram_charge,is_active) VALUES(@0,@1,@2,@3,@4,@5)";
                                db.Execute(sqlSelect, title, title_e, gram_cost, multiplier, gram_charge, 1);
                            }
                            else
                            {
                                sqlSelect = "UPDATE strainOptions SET title=@0,title_e=@1,gram_cost=@2,multiplier=@3,gram_charge=@4,is_active=@5 WHERE id=@6";
                                db.Execute(sqlSelect, title, title_e, gram_cost, multiplier, gram_charge, is_active, strainOptionId);
                            }
                        }
                    }
                    break;

                case "mixture":
                    {
                        string title = Param(context, "title");
                        string title_e = Param(context, "title_e");
                        decimal gram_cost = 0; try { gram_cost = Convert.ToDecimal(Param(context, "gram_cost")); } catch { }
                        decimal multiplier = 0; try { multiplier = Convert.ToDecimal(Param(context, "multiplier")); } catch { }
                        decimal gram_charge = 0;
                        bool is_active = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_active"))) { is_active = true; }
                        bool is_add = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_add"))) { is_add = true; }
                        int mixtureOptionId = rec_id;
                        if (string.IsNullOrWhiteSpace(title)) { theHtmlOutput += "חובה להזין כותרת תקינה!<br>"; }
                        if (string.IsNullOrWhiteSpace(title_e)) { theHtmlOutput += "חובה להזין כותרת תקינה באנגלית!<br>"; }
                        if (gram_cost <= 0) { theHtmlOutput += "חובה להזין עלות גרם תקינה!<br>"; }
                        if (multiplier <= 0) { theHtmlOutput += "חובה להזין מכפיל תקין!<br>"; }

                        if (theHtmlOutput == "")
                        {
                            gram_charge = gram_cost * multiplier;

                            if (is_add)
                            {
                                sqlSelect = "INSERT INTO mixtureOptions(title,title_e,gram_cost,multiplier,gram_charge,is_active) VALUES(@0,@1,@2,@3,@4,@5)";
                                db.Execute(sqlSelect, title, title_e, gram_cost, multiplier, gram_charge, 1);
                            }
                            else
                            {
                                sqlSelect = "UPDATE mixtureOptions SET title=@0,title_e=@1,gram_cost=@2,multiplier=@3,gram_charge=@4,is_active=@5 WHERE id=@6";
                                db.Execute(sqlSelect, title, title_e, gram_cost, multiplier, gram_charge, is_active, mixtureOptionId);
                            }
                        }
                    }
                    break;

                case "mixturePotency":
                    {
                        string title = Param(context, "title");
                        string title_e = Param(context, "title_e");
                        decimal potency = 0; try { potency = Convert.ToDecimal(Param(context, "potency")); } catch { }
                        string nick = Param(context, "nick");
                        bool is_active = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_active"))) { is_active = true; }
                        bool is_add = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_add"))) { is_add = true; }
                        int mixturePotencyOptionId = rec_id;
                        if (string.IsNullOrWhiteSpace(title)) { theHtmlOutput += "חובה להזין כותרת תקינה!<br>"; }
                        if (string.IsNullOrWhiteSpace(title_e)) { theHtmlOutput += "חובה להזין כותרת תקינה באנגלית!<br>"; }
                        if (potency <= 0) { theHtmlOutput += "חובה להזין חוזק תקין באחוזים!<br>"; }
                        if (string.IsNullOrWhiteSpace(nick)) { theHtmlOutput += "חובה להזין כינוי תקין!<br>"; }

                        if (theHtmlOutput == "")
                        {
                            if (is_add)
                            {
                                sqlSelect = "INSERT INTO mixturePotencyOptions(title,title_e,potency,nick,is_active) VALUES(@0,@1,@2,@3,@4)";
                                db.Execute(sqlSelect, title, title_e, potency, nick, 1);
                            }
                            else
                            {
                                sqlSelect = "UPDATE mixturePotencyOptions SET title=@0,title_e=@1,potency=@2,nick=@3,is_active=@4 WHERE id=@5";
                                db.Execute(sqlSelect, title, title_e, potency, nick, is_active, mixturePotencyOptionId);
                            }
                        }
                    }
                    break;

                case "Paper":
                    {
                        string title = Param(context, "title");
                        string title_e = Param(context, "title_e");
                        decimal gram_in_unit = 0; try { gram_in_unit = Convert.ToDecimal(Param(context, "gram_in_unit")); } catch { }
                        string nick = Param(context, "nick");
                        bool is_active = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_active"))) { is_active = true; }
                        bool is_add = false; if (!string.IsNullOrWhiteSpace(Param(context, "is_add"))) { is_add = true; }
                        int paperOptionsOptionId = rec_id;
                        if (string.IsNullOrWhiteSpace(title)) { theHtmlOutput += "חובה להזין כותרת תקינה!<br>"; }
                        if (string.IsNullOrWhiteSpace(title_e)) { theHtmlOutput += "חובה להזין כותרת תקינה באנגלית!<br>"; }
                        if (gram_in_unit <= 0) { theHtmlOutput += "חובה להזין ערך גרם ליחידה תקין!<br>"; }
                        if (string.IsNullOrWhiteSpace(nick)) { theHtmlOutput += "חובה להזין כינוי תקין!<br>"; }

                        if (theHtmlOutput == "")
                        {
                            if (is_add)
                            {
                                sqlSelect = "INSERT INTO paperOptions(title,title_e,gram_in_unit,nick,is_active) VALUES(@0,@1,@2,@3,@4)";
                                db.Execute(sqlSelect, title, title_e, gram_in_unit, nick, 1);
                            }
                            else
                            {
                                sqlSelect = "UPDATE paperOptions SET title=@0,title_e=@1,gram_in_unit=@2,nick=@3,is_active=@4 WHERE id=@5";
                                db.Execute(sqlSelect, title, title_e, gram_in_unit, nick, is_active, paperOptionsOptionId);
                            }
                        }
                    }
                    break;
            }

            if (theHtmlOutput != "")
            {
                theHtmlOutput = "!$!" + theHtmlOutput;
            }
            else
            {
                theHtmlOutput = " הפעולה בוצעה!";
            }

            db.Close();
            return theHtmlOutput;
        }


        public static int InitCart()
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            int u_id = WebSecurity.CurrentUserId;
            int cart_id = 0;
            DateTime local_time = AppFunctions.LocalTime();
            var db = Database.Open("faysal");
            string sqlSelect = "SELECT id FROM CartSettings WHERE u_id=@0";
            var cart=db.QuerySingle(sqlSelect, u_id);
            if (cart == null)
            {
                db.Execute("INSERT INTO CartSettings(ts,last_active,u_id) VALUES(@0,@0,@1)",local_time,u_id);
                cart = db.QuerySingle("SELECT id FROM CartSettings WHERE ts=@0 AND u_id=@1", local_time, u_id);
                try { cart_id = cart.id; } catch { }
            }
            else
            {
                cart_id = cart.id;
            }

            db.Close();
            return cart_id;
        }


        public static void CartReCalculate(HttpContext context,int cart_id = 0)
        {
            var Request = context.Request;
            var culture = new System.Globalization.CultureInfo("he-IL");

            int u_id = WebSecurity.CurrentUserId;
            DateTime local_time = AppFunctions.LocalTime();

            var db = Database.Open("faysal");
            var sqlSelect = "";
            dynamic cart = null;
            if (cart_id > 0)
            {
                sqlSelect = "SELECT u_id FROM CartSettings WHERE id=@0";
                cart = db.QuerySingle(sqlSelect, cart_id);
                try { u_id = cart.u_id; } catch { }
            }
            else
            {
                sqlSelect = "SELECT id FROM CartSettings WHERE u_id=@0";
                cart = db.QuerySingle(sqlSelect, u_id);
                try { cart_id = cart.id; } catch { }
            }
            sqlSelect = "SELECT * FROM cart WHERE cart_id=@0 order by ts";
            cart = db.Query(sqlSelect, cart_id);

            db.Close();

            int itemCounter = 0;
            decimal order_total = 0;
            decimal order_total_including_shipping = 0;
            decimal shipping_charge = 0;
            decimal free_shipping_marker = 0;
            decimal free_shipping_deficit = 0;

            try { shipping_charge = Convert.ToInt32(AppFunctions.GetAppProperty("DefaultShippingCharge")); } catch { }
            try { free_shipping_marker = Convert.ToInt32(AppFunctions.GetAppProperty("FreeShippingMarker")); } catch { }

            if (cart.Count>0)
            {
                
                foreach (var item in cart)
                {
                    if (cart_id == 0)
                    {
                        cart_id = item.cart_id;
                    }
                    
                    itemCounter++;
                    decimal item_total = item.sale_price * item.quanOfPackages;
                    order_total += item_total;
                }

                if (order_total > free_shipping_marker)
                {
                    shipping_charge = 0;
                }
                else
                {
                    free_shipping_deficit = free_shipping_marker - order_total;
                }

                order_total_including_shipping = order_total + shipping_charge;

                sqlSelect = "UPDATE CartSettings SET last_active=@0,order_total=@1,shipping_charge=@2,order_total_with_sh=@3,free_shipping_deficit=@4 WHERE id=@5";
                db.Execute(sqlSelect, local_time, order_total, shipping_charge, order_total_including_shipping, free_shipping_deficit, cart_id);

                if (context != null)
                {
                    context.Session.SetString("Cart_OrderTotal", order_total.ToString(culture));
                    context.Session.SetString("Cart_ShippingCharge", shipping_charge.ToString(culture));
                    context.Session.SetString("Cart_OrderTotalWithShipping", order_total_including_shipping.ToString(culture));
                    context.Session.SetString("Cart_FreeShippingMarker", free_shipping_marker.ToString(culture));
                    context.Session.SetString("Cart_FreeShippingDeficit", free_shipping_deficit.ToString(culture));
                    context.Session.SetInt32("Cart_ItemCounter", itemCounter);
                }
            }
            else
            {
                sqlSelect = "DELETE FROM CartSettings WHERE id=@0";
                db.Execute(sqlSelect, cart_id);
                if (context != null)
                {
                    context.Session.SetString("Cart_OrderTotal", "");
                    context.Session.SetString("Cart_ShippingCharge", "");
                    context.Session.SetString("Cart_OrderTotalWithShipping", "");
                    context.Session.SetString("Cart_FreeShippingMarker", "");
                    context.Session.SetString("Cart_FreeShippingDeficit", "");
                    context.Session.SetInt32("Cart_ItemCounter", 0);
                }
            }

            db.Close();
         
        }


        public static string AddToCart(HttpContext context)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            string sessionId = context.Session.Id;
            int u_id = WebSecurity.CurrentUserId;
            DateTime local_time = AppFunctions.LocalTime();
            string theHtmlOutput = "";
            var db = Database.Open("faysal");
            string sqlSelect = "";

            int quan = 0; try { quan = Convert.ToInt32(Param(context, "quan")); } catch { }
            int strain = 0; try { strain = Convert.ToInt32(Param(context, "strain")); } catch { }
            int paper = 0; try { paper = Convert.ToInt32(Param(context, "paper")); } catch { }
            int MixturePotency = 0; try { MixturePotency = Convert.ToInt32(Param(context, "MixturePotency")); } catch { }
            int Mixture = 0; try { Mixture = Convert.ToInt32(Param(context, "Mixture")); } catch { }
            int quanOfPackages = 1; try { quanOfPackages = Convert.ToInt32(Param(context, "quanOfPackages")); } catch { }

            string retTxtHtml = "";
            if (quan == 0) retTxtHtml += "נא להזין כמות מבוקשת!<br>";
            if (strain == 0) retTxtHtml += "נא לבחור את הזן המבוקש!<br>";
            if (paper == 0) retTxtHtml += "נא לבחור את הגודל המבוקש!<br>";
            if (MixturePotency == 0) retTxtHtml += "נא לבחור את המינון המבוקש!<br>";
            if (Mixture == 0) retTxtHtml += "נא לבחור את התערובת המבוקשת!<br>";

            int requested_quan = 0;
            decimal quan_labor_charge = 0, quan_labor_cost = 0, quan_multiplier = 0;
            string quan_option_title = "", paper_option_title = "", paper_nick = "", paper_nick_e = "";
            string strain_option_title = "", mixture_potency_option_title = "", mixture_potency_option_nick = "", mixture_option_title = "";
            string quan_option_title_e = "", paper_option_title_e = "", strain_option_title_e = "", mixture_potency_option_title_e = "", mixture_potency_option_nick_e = "", mixture_option_title_e = "";

            decimal gram_in_unit = 0, unit_cost = 0, paper_unit_cost = 0, gram_charge = 0;
            decimal strain_gram_cost = 0, strain_multiplier = 0, potency = 0;
            decimal mixture_gram_charge = 0, mixture_gram_cost = 0, mixture_multiplier = 0;
            decimal total_potent_gram = 0, total_gram = 0, mixture_price = 0, mixture_cost = 0;
            decimal potent_price = 0, potent_cost = 0, total_price = 0, total_cost = 0;
            decimal sale_price = 0, unit_sale_price = 0, unit_price = 0;
            int potent_percent = 0, mixture_percent = 0;


            if (retTxtHtml == "")
            {

                sqlSelect = "SELECT * FROM quanOptions WHERE id=@0";
                var quanOptions = db.QuerySingle(sqlSelect, quan);
                if (quanOptions != null)
                {
                    quan_labor_cost = quanOptions.labor_cost;
                    quan_multiplier = quanOptions.multiplier;
                    quan_labor_charge = quan_labor_cost * quan_multiplier;
                    requested_quan = quanOptions.quan;
                    quan_option_title = quanOptions.title;
                    quan_option_title_e = quanOptions.title_e;
                }

                sqlSelect = "SELECT * FROM paperOptions WHERE id=@0";
                var paperOptions = db.QuerySingle(sqlSelect, paper);
                if (paperOptions != null)
                {
                    gram_in_unit = paperOptions.gram_in_unit;
                    paper_unit_cost = paperOptions.unit_cost;
                    paper_nick = paperOptions.nick;
                    paper_nick_e = paperOptions.nick_e;
                    paper_option_title = paperOptions.title;
                    paper_option_title_e = paperOptions.title_e;
                }

                sqlSelect = "SELECT * FROM strainOptions WHERE id=@0";
                var strainOptions = db.QuerySingle(sqlSelect, strain);
                if (strainOptions != null)
                {
                    strain_gram_cost = strainOptions.gram_cost;
                    strain_multiplier = strainOptions.multiplier;
                    gram_charge = strain_gram_cost * strain_multiplier;
                    strain_option_title = strainOptions.title;
                    strain_option_title_e = strainOptions.title_e;
                }

                sqlSelect = "SELECT * FROM mixturePotencyOptions WHERE id=@0";
                var mixturePotencyOptions = db.QuerySingle(sqlSelect, MixturePotency);
                if (mixturePotencyOptions != null)
                {
                    potency = mixturePotencyOptions.potency;
                    mixture_potency_option_title = mixturePotencyOptions.title;
                    mixture_potency_option_title_e = mixturePotencyOptions.title_e;
                    mixture_potency_option_nick = mixturePotencyOptions.nick;
                    mixture_potency_option_nick_e = mixturePotencyOptions.nick_e;
                }

                sqlSelect = "SELECT * FROM mixtureOptions WHERE id=@0";
                var mixtureOptions = db.QuerySingle(sqlSelect, Mixture);
                if (mixtureOptions != null)
                {
                    mixture_gram_cost = mixtureOptions.gram_cost;
                    mixture_multiplier = mixtureOptions.multiplier;
                    mixture_gram_charge = mixture_gram_cost * mixture_multiplier;
                    mixture_option_title = mixtureOptions.title;
                    mixture_option_title_e = mixtureOptions.title_e;
                }

                total_gram = requested_quan * gram_in_unit;
                total_potent_gram = total_gram * potency;
                mixture_price = (total_gram - total_potent_gram) * mixture_gram_charge;
                mixture_cost = (total_gram - total_potent_gram) * mixture_gram_cost;
                potent_price = total_potent_gram * gram_charge;
                potent_cost = total_potent_gram * strain_gram_cost;
                total_price = potent_price + mixture_price + quan_labor_charge;
                total_cost = potent_cost + mixture_cost + quan_labor_cost;
                unit_cost = total_cost / requested_quan;
                unit_price = total_price / requested_quan;
                potent_percent = Convert.ToInt32(potency * 100);
                mixture_percent = 100 - potent_percent;
                sale_price = RoundUpAmount(total_price);
                unit_sale_price = sale_price / requested_quan;
            }

            if (retTxtHtml != "")
            {
                theHtmlOutput = "!$!" + retTxtHtml;
            }
            else
            {
                int cart_id = 0; try { cart_id = InitCart(); } catch { }

                sqlSelect = "INSERT INTO cart(ts,u_id,session_id,quanOfPackages,quanInPackage,quan_title,quan_labor_cost,quan_labor_multiplier,quan_labor_charge,strain_title,strain_gram_cost,strain_multiplier,strain_gram_charge,paper_title,paper_gram_in_unit,paper_nick,paper_unit_cost,mixture_title,mixture_gram_cost,mixture_multiplier,mixture_gram_charge,potency_title,potency,potency_nick,total_cost,total_price,unit_cost,unit_price,total_gram,total_potent_gram,mixture_price,potent_price,sale_price,unit_sale_price,last_activity,cart_id)";
                sqlSelect += " VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,@24,@25,@26,@27,@28,@29,@30,@31,@32,@33,@34,@35)";
                db.Execute(sqlSelect, local_time, u_id, sessionId, quanOfPackages, requested_quan, quan_option_title, quan_labor_cost, quan_multiplier, quan_labor_charge,
                    strain_option_title, strain_gram_cost, strain_multiplier, gram_charge,
                    paper_option_title, gram_in_unit, paper_nick, paper_unit_cost,
                    mixture_option_title, mixture_gram_cost, mixture_multiplier, mixture_gram_charge,
                    mixture_potency_option_title, potency, mixture_potency_option_nick,
                    total_cost, total_price, unit_cost, unit_price,
                    total_gram, total_potent_gram, mixture_price, potent_price,
                    sale_price, unit_sale_price, local_time,cart_id);

                CartReCalculate(context);
                theHtmlOutput = "OK";
            }

            db.Close();
            return theHtmlOutput;
        }

        public static string CartAction(HttpContext context)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            string sessionId = context.Session.Id;
            int member_id = 0;
            int item_serial = 0; try { item_serial = Convert.ToInt32(Param(context, "param1")); } catch { }
            string theValue = Param(context, "value");
            DateTime local_time = AppFunctions.LocalTime();
            string theHtmlOutput = "";
            var db = Database.Open("faysal");
            string sqlSelect = "";

            switch (theValue)
            {
                case "REMOVE":
                    sqlSelect = "DELETE FROM CART WHERE serial=@0";
                    break;
                case "ADDONE":
                    sqlSelect = "UPDATE cart SET quanOfPackages=quanOfPackages+1 WHERE serial=@0";
                    break;
                case "SUBONE":
                    sqlSelect = "UPDATE cart SET quanOfPackages=quanOfPackages-1 WHERE serial=@0";
                    break;
            }

            db.Execute(sqlSelect, item_serial);
            CartReCalculate(context);
            theHtmlOutput = "!!!";
            db.Close();
            return theHtmlOutput;
        }

        public static string ClearCart(HttpContext context)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            
            int u_id = WebSecurity.CurrentUserId;
            DateTime local_time = AppFunctions.LocalTime();
            string theHtmlOutput = "";
            var db = Database.Open("faysal");
            string sqlSelect = "DELETE FROM cart WHERE u_id=@0;DELETE FROM CartSettings WHERE u_id=@0";
            db.Execute(sqlSelect, u_id);
            CartReCalculate(context);
            theHtmlOutput = "ההזמנה אופסה!";
            db.Close();
            return theHtmlOutput;
        }



        public static string ReviewOrder(HttpContext context)
        {
            CultureInfo culture = new CultureInfo("he-IL");
            DateTime local_time = AppFunctions.LocalTime();
            string theHtmlOutput = "";
            var db = Database.Open("faysal");
            string sqlSelect = "";

            string sessionId = context.Session.Id;
            string session_id = Param(context, "value");
            string full_name = Param(context, "full_name");
            string wanum = Param(context, "wanum");
            string address = Param(context, "address");
            string city = Param(context, "city");
            string telegram_nick = Param(context, "telegram_nick");
            string order_notes = Param(context, "order_notes");

            if (string.IsNullOrWhiteSpace(full_name)) { theHtmlOutput += "חובה להזין שם!<br>"; }
            if (string.IsNullOrWhiteSpace(wanum)) { theHtmlOutput += "חובה להזין מספר ווטסאפ!<br>"; }
            if (string.IsNullOrWhiteSpace(address)) { theHtmlOutput += "חובה להזין כתובת למשלוח!<br>"; }
            if (string.IsNullOrWhiteSpace(city)) { theHtmlOutput += "חובה להזין עיר למשלוח!<br>"; }

            sqlSelect = "SELECT * FROM cart WHERE session_id=@0 order by ts";
            var cart = db.Query(sqlSelect, sessionId);
            if (cart == null) { theHtmlOutput += "לא קיימים פריטים בהזמנה!<br>"; }

            db.Close();
            if (theHtmlOutput != "")
            {
                theHtmlOutput = "!$!" + theHtmlOutput;
            }
            return theHtmlOutput;
        }

        public static string PlaceOrder(HttpContext context)
        {
            CultureInfo culture = new CultureInfo("he-IL");
            DateTime local_time = AppFunctions.LocalTime();
            int u_id = WebSecurity.CurrentUserId;
            string theHtmlOutput = "";
            var db = Database.Open("faysal");
            string sqlSelect = "";

            string sessionId = context.Session.Id;
            string session_id = Param(context, "value");
            string full_name = Param(context, "full_name");
            string wanum = Param(context, "wanum");
            string address = Param(context, "address");
            string city = Param(context, "city");
            string telegram_nick = Param(context, "telegram_nick");
            string order_notes = Param(context, "order_notes");
            
            int address_id = 0; try { address_id = Convert.ToInt32(Param(context, "address_id")); } catch { }
            string ship_phone = Param(context, "ship_phone");
            string ship_name = Param(context, "ship_name");
            string ship_notes = Param(context, "ship_notes");
            string address_nick = Param(context, "address_nick");
            string member_nick = Param(context, "member_nick");
            bool address_remember = !string.IsNullOrEmpty(Param(context, "Address_remember"));
            bool send_to_other = !string.IsNullOrEmpty(Param(context, "send_to_other"));

            int order_status = 1;
            int member_id = 0;
            decimal order_total = 0;
            decimal shipping_charge = 0;
            decimal free_shipping_marker = 0;
            decimal free_shipping_deficit = 0;

            decimal grand_total = 0;
            decimal round_up_amount = 0;
            decimal after_round_up_grand_total = 0;
            
            int new_order_id = 0;
            int item_status = 0;
            int item_type = 0;
            int itemCounter = 0;

            string disp_address = MaskString(address + " " + city);

            sqlSelect = "SELECT * FROM cart WHERE session_id=@0 order by ts";
            var cart = db.Query(sqlSelect, sessionId);

            if (cart != null)
            {
                try { shipping_charge = Convert.ToInt32(AppFunctions.GetAppProperty("DefaultShippingCharge")); } catch { }
                try { free_shipping_marker = Convert.ToInt32(AppFunctions.GetAppProperty("FreeShippingMarker")); } catch { }

                foreach (var item in cart)
                {
                    itemCounter++;
                    decimal item_total = item.sale_price * item.quanOfPackages;
                    order_total += item_total;
                }
                if (order_total > free_shipping_marker)
                {
                    shipping_charge = 0;
                }
                else
                {
                    free_shipping_deficit = free_shipping_marker - order_total;
                }
                grand_total = order_total + shipping_charge;


                if (!send_to_other)
                {
                    ship_name = member_nick;
                }

                if (address_id > 0)
                {

                }
                else
                {
                    if (address_remember)
                    {
                        sqlSelect = "UPDATE Addresses SET is_default=0 WHERE u_id=@0";
                        db.Execute(sqlSelect, u_id);


                        sqlSelect = "INSERT INTO Addresses(address,city,last_active,ts,u_id,address_notes,address_nick,disp_address,is_default) ";
                        sqlSelect += " VALUES (@0,@1,@2,@3,@4,@5,@6,@7,@8)";
                        db.Execute(sqlSelect, address, city, local_time, local_time, u_id, ship_notes, address_nick, disp_address, 1);

                        sqlSelect = "SELECT TOP(1) id FROM addresses WHERE is_default=1 AND u_id=@0 ORDER BY id DESC";
                        var new_address = db.QuerySingle(sqlSelect, u_id);
                        try { address_id = new_address.id; } catch { }
                    }
         
                }

                sqlSelect = "INSERT INTO orders(session_id,ts,order_status,u_id,full_name,wanum,telegram_nick,address,city,order_total,shipping_charge,grand_total,order_notes,ship_name,ship_phone ,address_id ,ship_notes) ";
                sqlSelect += " VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16)";
                db.Execute(sqlSelect, session_id, local_time, order_status, u_id, full_name, wanum, telegram_nick, address, city, order_total, shipping_charge, grand_total, order_notes, ship_name, ship_phone, address_id, ship_notes);

                sqlSelect = "SELECT order_id FROM orders WHERE ts=@0 AND session_id=@1";
                var order = db.QuerySingle(sqlSelect, local_time, session_id);

                if (order != null)
                {
                    new_order_id = order.order_id;
                    foreach (var item in cart)
                    {
                        sqlSelect = "INSERT INTO orderitems(ts,order_id,u_id,item_status,item_type,quanOfPackages,quanInPackage,quan_title,quan_labor_cost,quan_labor_multiplier,quan_labor_charge,strain_title,strain_gram_cost,strain_multiplier,strain_gram_charge,paper_title,paper_gram_in_unit,paper_nick,paper_unit_cost,mixture_title,mixture_gram_cost,mixture_multiplier,mixture_gram_charge,potency_title,potency,potency_nick,total_cost,total_price,unit_cost,unit_price,total_gram,total_potent_gram,mixture_price,potent_price,sale_price,unit_sale_price) ";
                        sqlSelect += " VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,@24,@25,@26,@27,@28,@29,@30,@31,@32,@33,@34,@35)";

                        db.Execute(sqlSelect, local_time, new_order_id, u_id, item_status, item_type,
                            item.quanOfPackages, item.quanInPackage, item.quan_title, item.quan_labor_cost, item.quan_labor_multiplier, item.quan_labor_charge,
                            item.strain_title, item.strain_gram_cost, item.strain_multiplier, item.strain_gram_charge,
                            item.paper_title, item.paper_gram_in_unit, item.paper_nick, item.paper_unit_cost,
                            item.mixture_title, item.mixture_gram_cost, item.mixture_multiplier, item.mixture_gram_charge,
                            item.potency_title, item.potency, item.potency_nick,
                            item.total_cost, item.total_price, item.unit_cost, item.unit_price,
                            item.total_gram, item.total_potent_gram, item.mixture_price, item.potent_price, item.sale_price, item.unit_sale_price);
                    }

                    string wanum_mask = ""; try { wanum_mask = MaskString(wanum); } catch { }

                    //string address_mask = ""; try { address_mask = "***" + address.Substring(address.Length - 3) ; } catch { }
                    

                    sqlSelect = "SET IDENTITY_INSERT OrderTracking ON;INSERT INTO OrderTracking (order_id,ts,u_id,order_status,wanum,address,city,order_total,shipping_charge,grand_total,round_up_amount,after_round_up_grand_total,order_notes) ";
                    sqlSelect += " VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12);";
                    sqlSelect += "SET IDENTITY_INSERT OrderTracking OFF;";
                    db.Execute(sqlSelect, new_order_id, local_time, u_id, order_status, wanum_mask, disp_address, city, order_total, shipping_charge, grand_total, round_up_amount, after_round_up_grand_total, order_notes);
                }
                theHtmlOutput += "ההזמנה התקבלה בהצלחה!<br>";
            }
            else
            {
                theHtmlOutput = "!$!היתה בעיה וההזמנה לא נקלטה!<br>";
            }

            db.Close();
            return theHtmlOutput;
        }


        public static string MaskString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            char[] result = input.ToCharArray();

            for (int i = 0; i < result.Length; i++)
            {
                if (i % 2 == 1) // replace every other char (1,3,5,…)
                {
                    result[i] = '*';
                }
            }

            return new string(result);
        }


    }
}
