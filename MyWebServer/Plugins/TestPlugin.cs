using System;
using BIF.SWE1.Interfaces;

namespace MyWebServer.Plugins
{
    [AutoLoadPlugin]
    class TestPlugin : IPlugin
    {
        public float CanHandle(IRequest req)
        {
            float retval = 0;
            bool handle_check = false;
            Predicate<string> test_check = (string s) => { return s == "test"; };

            handle_check = ("test" == Array.Find(req.Url.Segments, test_check)) || req.Url.Parameter.ContainsKey("test_plugin");

            if(handle_check)
            {
                retval = 1;
            }
            else
            {
                retval = req.Url.Path == "/" ? 0.1F : 0;
            }

            return retval;
        }

        public IResponse Handle(IRequest req)
        {
            Response resp = new Response
            {
                StatusCode = 200
            };

            string content = "<html><head></head><body><h1>What is this test-plugin sorcery?!</h1></body></html>";
            resp.SetContent(content);

            return resp;
        }
    }
}