using System;
using BIF.SWE1.Interfaces;
using System.IO;
using System.Reflection;

namespace MyWebServer.Plugins
{
    [AutoLoadPlugin]
    class StaticFilePlugin : IPlugin
    {
        public float CanHandle(IRequest req)
        {
            float retval = 0;
            bool handle_check = false;
            Predicate<string> static_check = (string s) => { return s == "static-files"; };

            handle_check = ("static-files" == Array.Find(req.Url.Segments, static_check)) || req.Url.Parameter.ContainsKey("static-file_plugin");

            if (handle_check)
            {
                retval = 1;
            }
            else
            {
                retval = req.Url.Path == "/" ? 0.6F : 0.1F;
            }

            return retval;
        }

        public IResponse Handle(IRequest req)
        {
            Response resp = new Response
            {
                StatusCode = 200
            };

            string postfix = req.Url.RawUrl != "/" ? req.Url.Path : "\\index.html";
            string fdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + postfix;

            if(File.Exists(fdir))
            {
                string fn = req.Url.RawUrl != "/" ? req.Url.FileName + req.Url.Extension : "index.html";

                if(Program.MutexCollection.TryLockFile(fn))
                {
                    byte[] content = File.ReadAllBytes(fdir);
                    resp.SetContent(content);
                    resp.ContentType = req.Url.RawUrl != "/" ? req.Url.Extension ?? ".txt" : ".html";
                    Program.MutexCollection.ReleaseFile(fn);
                }
                else
                {
                    resp.StatusCode = 404;
                }
            }
            else if(req.Url.RawUrl == "/")
            {
                string content = "Danke Jenkins";
                resp.SetContent(content);
            }
            else
            {
                resp.StatusCode = 404;
            }

            return resp;
        }
    }
}