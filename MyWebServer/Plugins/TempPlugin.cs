using BIF.SWE1.Interfaces;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace MyWebServer.Plugins
{
    [AutoLoadPlugin]
    class TempPlugin : IPlugin
    {
        public float CanHandle(IRequest req)
        {
            float retval = 0;
            bool handle_check = false;

            handle_check = req.ContentString != null ? (req.ContentString.Contains("from=") && req.ContentString.Contains("until=")) : false;

            handle_check = handle_check || req.Url.RawUrl.Contains("temp.php");

            handle_check = handle_check || req.Url.RawUrl.Contains("GetTemperature");

            if (handle_check)
            {
                retval = 1;
            }
            else
            {
                retval = 0;
            }

            return retval;
        }

        public IResponse Handle(IRequest req)
        {
            Response resp = new Response
            {
                StatusCode = 200
            };

            string outfeed = "";
            int numrows = 0;
            bool REST_xml = false;

            if (!req.Url.RawUrl.Contains("temp.php"))
            {
                if (req.ContentString?.IndexOf("until=") == 16 && req.ContentString?.Substring(req.ContentString.IndexOf("until=")).Length == 16)
                {
                    string from = req.ContentString.Substring(5, 10);
                    string until = req.ContentString.Substring(22, 10);
                    bool fc = DateTime.TryParseExact(from, "yyyy-dd-MM", null, DateTimeStyles.None, out DateTime FromCheck);
                    bool uc = DateTime.TryParseExact(until, "yyyy-dd-MM", null, DateTimeStyles.None, out DateTime UntilCheck);

                    if (fc && uc)
                    {
                        int pagenum = 0;
                        int pagemod = 0;

                        outfeed = "<table align=\"center\"><tr><th>Date</th><th>Temperature</th></tr>";

                        using (SqlConnection db = new SqlConnection(@"Data Source=(local); Initial Catalog=MyWebServerDB; Integrated Security=true;"))
                        {
                            db.Open();

                            SqlCommand query = new SqlCommand(@"SELECT [d_time], [d_temp] FROM [TempData] WHERE [d_time] >= @from AND [d_time] <= @until;", db);

                            query.Parameters.AddWithValue("@from", from + " 00:00:00");
                            query.Parameters.AddWithValue("@until", until + " 23:59:59");

                            using (SqlDataReader rd = query.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    outfeed += "<tr class=\"pages page_" + pagenum.ToString() + "\"><td>" + rd.GetDateTime(0).ToString() + "</td>";
                                    outfeed += "<td>" + rd.GetDouble(1).ToString() + "</td></tr>";

                                    ++pagemod;
                                    pagenum = pagemod % 30 == 0 ? pagenum + 1 : pagenum;
                                    ++numrows;
                                }

                                if (numrows == 0)
                                {
                                    outfeed = "No data for the given date found.";
                                }
                            }

                            db.Close();
                        }

                        outfeed += "</table><p id=\"maxpagenum\">" + pagenum.ToString() + "</p>";
                    }
                    else
                    {
                        outfeed = "The entered data was not valid.<p id=\"maxpagenum\">0</p>";
                    }
                }
                else if (req.Url.RawUrl.Contains("GetTemperature"))
                {
                    string year = req.Url.Segments[Array.IndexOf(req.Url.Segments, "GetTemperature") + 1];
                    string month = req.Url.Segments[Array.IndexOf(req.Url.Segments, "GetTemperature") + 2];
                    string day = req.Url.Segments[Array.IndexOf(req.Url.Segments, "GetTemperature") + 3];

                    bool rc = DateTime.TryParseExact(year + "-" + day + "-" + month, "yyyy-dd-MM", null, DateTimeStyles.None, out DateTime RESTCheck);

                    if (rc)
                    {
                        string date_lower = year + "-" + day + "-" + month + " 00:00:00";
                        string date_upper = year + "-" + day + "-" + month + " 23:59:59";
                        outfeed = "<?xml version=\"1.0\"?><TemperatureData>";

                        using (SqlConnection db = new SqlConnection(@"Data Source=(local); Initial Catalog=MyWebServerDB; Integrated Security=true;"))
                        {
                            db.Open();

                            SqlCommand query = new SqlCommand(@"SELECT [d_time], [d_temp] FROM [TempData] WHERE [d_time] >= @from AND [d_time] <= @until FOR XML PATH;", db);

                            query.Parameters.AddWithValue("@from", date_lower);
                            query.Parameters.AddWithValue("@until", date_upper);

                            using (SqlDataReader rd = query.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    outfeed += rd.GetString(0);
                                    ++numrows;
                                }

                                REST_xml = true;
                                outfeed += "</TemperatureData>";

                                if (numrows == 0)
                                {
                                    outfeed = "No data for the given date found.";
                                }
                            }

                            db.Close();
                        }
                    }
                    else
                    {
                        outfeed = "The entered data was not valid.<p id=\"maxpagenum\">0</p>";
                    }
                }
                else
                {
                    outfeed = "The entered data was not valid.<p id=\"maxpagenum\">0</p>";
                }
            }
            else
            {
                outfeed = "Herbert";
            }

            if (REST_xml && (numrows > 0))
            {
                resp.SetContent(outfeed);
                resp.ContentType = ".xml";
            }
            else
            {
                string fdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\index.html";
                string content = File.Exists(fdir) ? File.ReadAllText(fdir).Replace("No data entered.", outfeed) : outfeed;

                resp.SetContent(content);
                resp.ContentType = req.Url.Parameter.ContainsKey("xml") ? (req.Url.Parameter["xml"] == "0" ? "text/html" : "text/xml") : "text/html";
            }

            return resp;
        }
    }
}
