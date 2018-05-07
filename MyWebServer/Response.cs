using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using System.Web;

namespace MyWebServer
{
    /// <summary>
    /// Implements a Response object, containing information to send to a client of the WebServer.
    /// </summary>
    public class Response : IResponse
    {
        private IDictionary<string, string> _Headers = new Dictionary<string, string>();
        private int _ContentLength = 0;
        private string _ContentType = "";
        private int _StatusCode = 0;
        private string _Status = null;
        private string _ServerHeader = "BIF-SWE1-Server";
        private byte[] _Content = null;

        /// <summary>
        /// Adds the static Header "ServerHeader" to the Dictionary.
        /// </summary>
        public Response()
        {
            _Headers.Add("Server", _ServerHeader);
        }

        /// <summary>
        /// Adds or replaces a response header in the headers dictionary.
        /// </summary>
        /// <param name="header">
        /// Key of the Dictionary, represented by the name of the header.
        /// </param>
        /// <param name="value">
        /// Value of the Dictionary, represented by the value of the header.
        /// </param>
        public void AddHeader(string header, string value)
        {
            CheckAndInsertHeader(header, value);
        }

        /// <summary>
        /// Sends the response to the network stream.
        /// </summary>
        /// <param name="network">
        /// The network stream of the client on which to write on.
        /// </param>
        public void Send(Stream network)
        {
            byte[] outbyte = null;
            string outfeed = "HTTP/1.1 " + _Status + "\r\n";

            if ((_ContentType != "") && (_Content == null))
            {
                throw new NoContentSetException();
            }

            foreach (var header in _Headers)
            {
                outfeed += header.Key + ": " + header.Value + "\r\n";
            }

            outfeed += "Connection: closed\r\n\n";

            outbyte = Encoding.UTF8.GetBytes(outfeed);

            outbyte = _Content != null ? outbyte.Concat(_Content).ToArray() : outbyte;

            try
            {
                network.Write(outbyte, 0, outbyte.Count());
            }
            catch
            {
                Console.WriteLine("Client aborted request.");
            }
        }

        /// <summary>
        /// Sets a string content. The content will be encoded in UTF-8.
        /// </summary>
        /// <param name="content">
        /// The string, representing the content.
        /// </param>
        public void SetContent(string content)
        {
            _Content = Encoding.UTF8.GetBytes(content);
            _ContentLength = _Content.Count();
            CheckAndInsertHeader("Content-Length", _ContentLength.ToString());
        }

        /// <summary>
        /// Sets a byte[] as content.
        /// </summary>
        /// <param name="content">
        /// The byte[], representing the content.
        /// </param>
        public void SetContent(byte[] content)
        {
            _Content = content;
            _ContentLength = _Content.Count();
            CheckAndInsertHeader("Content-Length", _ContentLength.ToString());
        }

        /// <summary>
        /// Sets the stream as content.
        /// </summary>
        /// <param name="stream">
        /// The stream, representing the content.
        /// </param>
        public void SetContent(Stream stream)
        {
            if (stream != null)
            {
                StreamReader ContentReader = new StreamReader(stream);
                string content = ContentReader.ReadToEnd();
                _Content = Encoding.UTF8.GetBytes(content);
                _ContentLength = _Content.Count();
                CheckAndInsertHeader("Content-Length", _ContentLength.ToString());
            }
        }

        private void CheckAndInsertHeader(string h_key, string h_value)
        {
            _Headers.TryGetValue(h_key, out string temp);

            if (temp == "0")
            {
                _Headers.Add(h_key, h_value);
            }
            else
            {
                _Headers[h_key] = h_value;
            }
        }

        #region Getter/Setter
        /// <summary>
        /// Returns a writable dictionary of the response headers. Never returns null.
        /// </summary>
        public IDictionary<string, string> Headers
        {
            get { return _Headers; }
        }

        /// <summary>
        /// Returns the content length or 0 if no content is set yet.
        /// </summary>
        public int ContentLength
        {
            get { return _ContentLength; }
        }

        /// <summary>
        /// Gets or sets the content type of the response.
        /// </summary>
        public string ContentType
        {
            get { return _ContentType; }
            set
            {
                if(value.Contains('/'))
                {
                    _ContentType = value;
                }
                else
                {
                    _ContentType = MimeMapping.GetMimeMapping(value);
                }

                CheckAndInsertHeader("Content-Type", _ContentType);
            }
        }

        /// <summary>
        /// Gets or sets the current status code. An Exceptions is thrown, if no status code was set.
        /// </summary>
        public int StatusCode
        {
            get
            {
                if (_StatusCode == 0)
                {
                    throw new StatusCodeNotSetException();
                }
                else
                {
                    return _StatusCode;
                }
            }

            set
            {
                _StatusCode = value;
                _Status = _StatusCode == 200 ? "200 OK" : _Status;
                _Status = _StatusCode == 404 ? "404 Not Found" : _Status;
                _Status = _StatusCode == 500 ? "500 Internal Server Error" : _Status;
            }
        }

        /// <summary>
        /// Returns the status code as string. (200 OK)
        /// </summary>
        public string Status
        {
            get { return _Status; }
        }

        /// <summary>
        /// Gets or sets the Server response header. Defaults to "BIF-SWE1-Server".
        /// </summary>
        public string ServerHeader
        {
            get { return _ServerHeader; }
            set
            {
                _ServerHeader = value;
                CheckAndInsertHeader("Server", value);
            }
        }
        #endregion
    }
}
