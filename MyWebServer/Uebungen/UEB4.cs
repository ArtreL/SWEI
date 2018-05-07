using BIF.SWE1.Interfaces;
using MyWebServer;

namespace Uebungen
{
    /// <summary>
    /// Tests Uebung 4.
    /// </summary>
    public class UEB4 : IUEB4
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
        /// Must return a empty IResponse implementation. This method must not fail.
        /// </summary>
        /// <returns>A IResponse implementation</returns>
        public IResponse GetResponse()
        {
            return new Response();
        }
    }
}
