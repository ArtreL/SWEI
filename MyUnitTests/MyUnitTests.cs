using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyWebServer;
using System.Linq;
using System.IO;
using BIF.SWE1.Interfaces;
using System.Reflection;
using System.Threading;

namespace MyUnitTests
{
    [TestClass]
    public class MyUnitTests
    {
        #region Properties and Methods
        private static MyRequestHelper ReqHelper = new MyRequestHelper();
        private static readonly string valid_url = "/Homepage/index.html?Page=1&Parameter=2#Fragment";
        private static PluginManager PlugManager = new PluginManager();

        private static string GetHandledResponseStringFromPlugin(Request req)
        {
            Stream resp_stream = new MemoryStream();
            StreamReader stream_reader = new StreamReader(resp_stream);

            IPlugin HandlePlugin = PlugManager.GetPlugin(req);
            IResponse resp = HandlePlugin.Handle(req);

            resp.Send(resp_stream);
            resp_stream.Position = 0;

            return stream_reader.ReadToEnd();
        }
        #endregion

        #region Basic Classes 3/20
        [TestMethod]
        public void Url_should_contain_EVERYTHING()
        {
            bool assertflag = false;
            Url url = new Url(valid_url);

            assertflag = url.Segments.Count() == 2
                         && url.ParameterCount == 2
                         && url.Parameter["Parameter"] == "2"
                         && url.Fragment == "Fragment"
                         && url.FileName == "index"
                         && url.Extension == ".html";

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void Request_should_contain_EVERYTHING()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "This is content."));

            assertflag = req.IsValid
                         && req.Method == "GET"
                         && req.UserAgent == "Unit-Test-Agent/1.0 (The OS)"
                         && req.HeaderCount == 8
                         && req.Headers["host"] == "localhost"
                         && req.ContentLength == 16
                         && req.ContentType == "application/x-www-form-urlencoded"
                         && req.ContentString == "This is content.";

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void Response_should_contain_EVERYTHING()
        {
            bool assertflag = false;
            Response resp = new Response();
            Stream resp_stream = new MemoryStream();
            StreamReader stream_reader = new StreamReader(resp_stream);

            resp.StatusCode = 200;
            resp.SetContent("This is content.");
            resp.ContentType = ".txt";
            resp.Send(resp_stream);

            resp_stream.Position = 0;
            string sent_response = stream_reader.ReadToEnd();

            assertflag = sent_response.Contains("Content-Length: 16")
                         && sent_response.Contains("Content-Type: text/plain")
                         && sent_response.Contains("200 OK")
                         && sent_response.Contains("This is content.");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }
        #endregion

        #region ToLower Plugin 4/20
        [TestMethod]
        public void ToLower_plugin_should_handle_numb3rs()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "text=12POLIZEI"));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("Content-Length:")
                         && sent_response.Contains("Content-Type: text/html")
                         && sent_response.Contains("200 OK")
                         && sent_response.Contains("12polizei");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void ToLower_plugin_should_handle_ümläüts()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "text=ÖLÜBERSCHUSSLÄNDER"));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("Content-Length: 24")
                         && sent_response.Contains("Content-Type: text/html")
                         && sent_response.Contains("200 OK")
                         && sent_response.Contains("ölüberschussländer");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void ToLower_plugin_should_handle_special_characters()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "text=$PE(|AL (HARA(TER$"));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("Content-Length: 18")
                         && sent_response.Contains("Content-Type: text/html")
                         && sent_response.Contains("200 OK")
                         && sent_response.Contains("$pe(|al (hara(ter$");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void ToLower_plugin_should_handle_chinese_characters()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "text=測試"));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("Content-Length: 10")
                         && sent_response.Contains("Content-Type: text/html")
                         && sent_response.Contains("200 OK")
                         && sent_response.Contains("測試");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }
        #endregion

        #region StaticFile Plugin 2/20
        [TestMethod]
        public void StaticFile_plugin_should_read_file_from_subdirectory()
        {
            bool assertflag = false;
            string static_file_url = valid_url.Replace("Homepage/index.html", "static-files/subdir-files/unittestfile.txt");

            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\static-files";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            folder += "\\subdir-files";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using (var fs = File.OpenWrite(folder + "\\unittestfile.txt"))
            using (var sw = new StreamWriter(fs))
            {
                fs.SetLength(0);
                sw.Write("This is a file within a subdirectory of static-files.");
            }

            Request req = new Request(ReqHelper.GetValidRequestStream(static_file_url));
            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("This is a file within a subdirectory of static-files.")
                         && sent_response.Contains("Content-Type: text/plain");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void StaticFile_plugin_should_read_file_content_type()
        {
            bool assertflag = false;
            string static_file_url = valid_url.Replace("Homepage/index.html", "static-files/tiny.png");

            Request req = new Request(ReqHelper.GetValidRequestStream(static_file_url));
            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("Content-Type: image/png");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }
        #endregion

        #region Navi Plugin 3/20
        [TestMethod]
        public void Navi_plugin_should_return_error_if_file_already_in_use()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "street=Kunzgasse"));
            string user2_retval = "";

            var FirstUserThread = new Thread(() =>
            {
                GetHandledResponseStringFromPlugin(req);
            });

            var SecondUserThread = new Thread(() =>
            {
                Thread.Sleep(10);

                user2_retval = GetHandledResponseStringFromPlugin(req);
            });

            FirstUserThread.Start();
            SecondUserThread.Start();

            FirstUserThread.Join();
            SecondUserThread.Join();

            assertflag = user2_retval.Contains("Map file already in use.");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void Navi_plugin_should_find_Reumannplatz()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "street=Reumannplatz"));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("Wien");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void Navi_plugin_should_not_find_Buxtehudegasse()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "street=Buxtehudegasse"));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("0 Orte gefunden");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }
        #endregion

        #region MutexMan 2/20
        [TestMethod]
        public void MutexMan_should_lock_file()
        {
            bool assertflag = false;
            MutexMan FileMut = new MutexMan();

            FileMut.TryLockFile("File1");

            assertflag = !FileMut.TryLockFile("File1");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void MutexMan_should_release_file()
        {
            bool assertflag = false;
            MutexMan FileMut = new MutexMan();

            FileMut.TryLockFile("File1");
            FileMut.ReleaseFile("File1");

            assertflag = FileMut.TryLockFile("File1");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }
        #endregion

        #region Temp Plugin 6/20
        [TestMethod]
        public void Temp_plugin_should_handle_false_input()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "from=9999-99-99&until=9999-99-99"));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("The entered data was not valid.");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void Temp_plugin_should_handle_empty_input()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "from=&until="));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("The entered data was not valid.");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void Temp_plugin_should_return_data_for_REST_request()
        {
            bool assertflag = false;
            string temp_rest_url = valid_url.Replace("Homepage/index.html", "GetTemperature/2017/12/17/oops");
            Request req = new Request(ReqHelper.GetValidRequestStream(temp_rest_url));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("<?xml");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void Temp_plugin_should_return_correct_tags_for_REST_request()
        {
            bool assertflag = false;
            string temp_rest_url = valid_url.Replace("Homepage/index.html", "GetTemperature/2017/12/17/dummy.html");
            Request req = new Request(ReqHelper.GetValidRequestStream(temp_rest_url));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("<d_time>")
                         && sent_response.Contains("</d_time>")
                         && sent_response.Contains("<d_temp>")
                         && sent_response.Contains("</d_temp>");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void Temp_plugin_should_only_return_given_day()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "from=2017-17-12&until=2017-17-12"));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("17/12/2017")
                         && !sent_response.Contains("18/12/2017")
                         && !sent_response.Contains("16/12/2017");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }

        [TestMethod]
        public void Temp_plugin_should_return_range_of_days()
        {
            bool assertflag = false;
            Request req = new Request(ReqHelper.GetValidRequestStream(valid_url, body: "from=2017-12-12&until=2017-17-12"));

            string sent_response = GetHandledResponseStringFromPlugin(req);

            assertflag = sent_response.Contains("17/12/2017")
                         && sent_response.Contains("15/12/2017")
                         && sent_response.Contains("12/12/2017");

            if (!assertflag)
            {
                throw new Exception("Test did not succeed.");
            }
        }
        #endregion
    }
}
