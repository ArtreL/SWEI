using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using MyWebServer;
using MyWebServer.Plugins;

namespace Uebungen
{
    /// <summary>
    /// Tests Uebung 5.
    /// </summary>
    public class UEB5 : IUEB5
    {
        private string _Folder = null;

        /// <summary>
        /// This method is only called to prove the unit test setup.
        /// </summary>
        public void HelloWorld()
        {
        }

        /// <summary>
        /// Must return a IPluginManager implementation. This method must not fail.
        /// </summary>
        /// <returns>A IPluginManager implementation</returns>
        public IPluginManager GetPluginManager()
        {
            return new PluginManager();
        }

        /// <summary>
        /// Must return a IRequest implementation. A valid, invalid or empty (containing one empty line) stream may be passed. This method must not fail in any case.
        /// </summary>
        /// <param name="network">A stream simulating the network.</param>
        /// <returns>A IRequest implementation</returns>
        public IRequest GetRequest(System.IO.Stream network)
        {
            return new Request(network);
        }

        /// <summary>
        /// Must return a static file plugin implementation. This method must not fail.
        /// </summary>
        /// <returns>A IPlugin implementation</returns>
        public IPlugin GetStaticFilePlugin()
        {
            return new StaticFilePlugin();
        }

        /// <summary>
        /// Returns a valid url for the static file plugin. The plugin should be able to return the given file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>A valid URL</returns>
        public string GetStaticFileUrl(string fileName)
        {
            return "/static-files/" + fileName;
        }

        /// <summary>
        /// Sets the folder path relative to the current working directory where test files are located. These files should be handles by the static file plugin.
        /// </summary>
        /// <param name="folder"></param>
        public void SetStatiFileFolder(string folder)
        {
            _Folder = folder;
        }
    }
}
