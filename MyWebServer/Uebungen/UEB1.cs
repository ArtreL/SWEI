using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using MyWebServer;

namespace Uebungen
{
    /// <summary>
    /// Tests Uebung 1.
    /// </summary>
    public class UEB1 : IUEB1
    {
        /// <summary>
        /// This method is only called to prove the unit test setup.
        /// </summary>
        public void HelloWorld()
        {
            // I'm fine
        }

        /// <summary>
        /// Must return a IUrl implementation. A valid, invalid, empty or null url may be passed. This method must not fail in any case.
        /// </summary>
        /// <param name="path">A url. May be valid, invalid, empty or null.</param>
        /// <returns>A IUrl implementation.</returns>
        public IUrl GetUrl(string path)
        {
            return new Url(path);
        }
    }
}
