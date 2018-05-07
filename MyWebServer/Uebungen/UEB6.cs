using System;
using BIF.SWE1.Interfaces;
using MyWebServer;
using MyWebServer.Plugins;

namespace Uebungen
{
    /// <summary>
    /// Tests Uebung 6.
    /// </summary>
    public class UEB6 : IUEB6
    {
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
        /// Returns a valid url for the temperature plugin. The plugin should be able to return a page showing all cities where a posted street exists. The name of the posted field will be "street".
        /// </summary>
        /// <returns>A valid URL</returns>
        public string GetNaviUrl()
        {
            return "/navi.php";
        }

        /// <summary>
        /// Must return a navigation plugin implementation. This method must not fail.
        /// </summary>
        /// <returns>A IPlugin implementation</returns>
        public IPlugin GetNavigationPlugin()
        {
            return new NaviPlugin();
        }

        /// <summary>
        /// Must return a temperature plugin implementation. This method must not fail.
        /// </summary>
        /// <returns>A IPlugin implementation</returns>
        public IPlugin GetTemperaturePlugin()
        {
            return new TempPlugin();
        }

        /// <summary>
        /// Returns a valid url for the temperature plugin. The plugin should be able to return a xml document containing temperature data of the given date range.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="until"></param>
        /// <returns>A valid URL</returns>
        public string GetTemperatureRestUrl(DateTime from, DateTime until)
        {
            return "/temp.php?xml=1&from=" + from + "&until=" + until;
        }

        /// <summary>
        /// Returns a valid url for the temperature plugin. The plugin should be able to return a page showing temperature data of the given date range.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="until"></param>
        /// <returns>A valid URL</returns>
        public string GetTemperatureUrl(DateTime from, DateTime until)
        {
            return "/temp.php?xml=0&from=" + from + "&until=" + until;
        }

        /// <summary>
        /// Must return a ToLower plugin implementation. This method must not fail.
        /// </summary>
        /// <returns>A IPlugin implementation</returns>
        public IPlugin GetToLowerPlugin()
        {
            return new ToLowerPlugin();
        }

        /// <summary>
        /// Returns a valid url for the ToLower plugin. The plugin should be able to return a page showing a posted text lowercase. The name of the posted field will be "text".
        /// </summary>
        /// <returns>A valid URL</returns>
        public string GetToLowerUrl()
        {
            return "/tolower/";
        }
    }
}
