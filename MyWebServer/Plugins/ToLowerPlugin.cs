using BIF.SWE1.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;

namespace MyWebServer.Plugins
{
    [AutoLoadPlugin]
    class ToLowerPlugin : IPlugin
    {
        public float CanHandle(IRequest req)
        {
            float retval = 0;
            bool handle_check = false;

            handle_check = req.ContentString != null ? req.ContentString.Contains("text=") : false;

            if (handle_check)
            {
                retval = 0.9F;
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

            string content = null;

            if(req.ContentString.Length > 5)
            {
                string tolowertext = HttpUtility.UrlDecode(req.ContentString.Substring(5)).ToLower();
                string fdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\index.html";
                content = File.Exists(fdir) ? File.ReadAllText(fdir).Replace("No text entered.", tolowertext) : tolowertext;
            }
            else
            {
                string tolowertext = "Bitte geben Sie einen Text ein";
                string fdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\index.html";
                content = File.Exists(fdir) ? File.ReadAllText(fdir).Replace("No text entered.", tolowertext) : tolowertext;
            }

            resp.ContentType = ".html";
            resp.SetContent(content);

            return resp;
        }
    }
}
