using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using MyWebServer;

namespace Uebungen
{
    /// <summary>
    /// Tests Uebung 2.
    /// </summary>
    public class UEB2 : IUEB2
    {
        /// <summary>
        /// This method is only called to prove the unit test setup.
        /// </summary>
        public void HelloWorld()
        {
        }

        /// <summary>
        /// Must return a IUrl implementation. A valid, invalid, empty or null url may be passed. This method must not fail in any case.
        /// </summary>
        /// <param name="path">A url. May be null.</param>
        /// <returns>A IUrl implementation.</returns>
        public IUrl GetUrl(string path)
        {
            return new Url(path);
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
