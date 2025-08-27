using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;

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

        public static string AddToCart(HttpContext context)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            string sessionId = context.Session.Id;
            int member_id = 0; // TODO: Replace with logged-in user ID
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
                sqlSelect = "INSERT INTO cart(ts,m_id,session_id,quanOfPackages,quanInPackage,quan_title,quan_labor_cost,quan_labor_multiplier,quan_labor_charge,strain_title,strain_gram_cost,strain_multiplier,strain_gram_charge,paper_title,paper_gram_in_unit,paper_nick,paper_unit_cost,mixture_title,mixture_gram_cost,mixture_multiplier,mixture_gram_charge,potency_title,potency,potency_nick,total_cost,total_price,unit_cost,unit_price,total_gram,total_potent_gram,mixture_price,potent_price,sale_price,unit_sale_price,last_activity)";
                sqlSelect += " VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,@24,@25,@26,@27,@28,@29,@30,@31,@32,@33,@34)";
                db.Execute(sqlSelect, local_time, member_id, sessionId, quanOfPackages, requested_quan, quan_option_title, quan_labor_cost, quan_multiplier, quan_labor_charge,
                    strain_option_title, strain_gram_cost, strain_multiplier, gram_charge,
                    paper_option_title, gram_in_unit, paper_nick, paper_unit_cost,
                    mixture_option_title, mixture_gram_cost, mixture_multiplier, mixture_gram_charge,
                    mixture_potency_option_title, potency, mixture_potency_option_nick,
                    total_cost, total_price, unit_cost, unit_price,
                    total_gram, total_potent_gram, mixture_price, potent_price,
                    sale_price, unit_sale_price, local_time);

                theHtmlOutput = "הכל טוב";
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
            theHtmlOutput = "!!!";
            db.Close();
            return theHtmlOutput;
        }

        public static string ClearCart(HttpContext context)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("he-IL");
            string sessionId = context.Session.Id;
            int member_id = 0;
            DateTime local_time = AppFunctions.LocalTime();
            string theHtmlOutput = "";
            var db = Database.Open("faysal");
            string sqlSelect = "DELETE FROM cart WHERE session_id=@0";
            db.Execute(sqlSelect, sessionId);
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

            int order_status = 1;
            int member_id = 0;
            decimal order_total = 0;
            decimal shipping_charge = 0;
            decimal grand_total = 0;
            int new_order_id = 0;
            int item_status = 0;
            int item_type = 0;

            sqlSelect = "SELECT * FROM cart WHERE session_id=@0 order by ts";
            var cart = db.Query(sqlSelect, sessionId);

            if (cart != null)
            {
                sqlSelect = "INSERT INTO orders(session_id,ts,order_status,m_id,full_name,wanum,telegram_nick,address,city,order_total,shipping_charge,grand_total,order_notes) ";
                sqlSelect += " VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12)";
                db.Execute(sqlSelect, session_id, local_time, order_status, member_id, full_name, wanum, telegram_nick, address, city, order_total, shipping_charge, grand_total, order_notes);

                sqlSelect = "SELECT order_id FROM orders WHERE ts=@0 AND session_id=@1";
                var order = db.QuerySingle(sqlSelect, local_time, session_id);

                if (order != null)
                {
                    new_order_id = order.order_id;
                    foreach (var item in cart)
                    {
                        sqlSelect = "INSERT INTO orderitems(ts,order_id,m_id,item_status,item_type,quanOfPackages,quanInPackage,quan_title,quan_labor_cost,quan_labor_multiplier,quan_labor_charge,strain_title,strain_gram_cost,strain_multiplier,strain_gram_charge,paper_title,paper_gram_in_unit,paper_nick,paper_unit_cost,mixture_title,mixture_gram_cost,mixture_multiplier,mixture_gram_charge,potency_title,potency,potency_nick,total_cost,total_price,unit_cost,unit_price,total_gram,total_potent_gram,mixture_price,potent_price,sale_price,unit_sale_price) ";
                        sqlSelect += " VALUES(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,@19,@20,@21,@22,@23,@24,@25,@26,@27,@28,@29,@30,@31,@32,@33,@34,@35)";

                        db.Execute(sqlSelect, local_time, new_order_id, member_id, item_status, item_type,
                            item.quanOfPackages, item.quanInPackage, item.quan_title, item.quan_labor_cost, item.quan_labor_multiplier, item.quan_labor_charge,
                            item.strain_title, item.strain_gram_cost, item.strain_multiplier, item.strain_gram_charge,
                            item.paper_title, item.paper_gram_in_unit, item.paper_nick, item.paper_unit_cost,
                            item.mixture_title, item.mixture_gram_cost, item.mixture_multiplier, item.mixture_gram_charge,
                            item.potency_title, item.potency, item.potency_nick,
                            item.total_cost, item.total_price, item.unit_cost, item.unit_price,
                            item.total_gram, item.total_potent_gram, item.mixture_price, item.potent_price, item.sale_price, item.unit_sale_price);
                    }
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

    }
}
