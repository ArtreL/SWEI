using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    /// <summary>
    /// Implements a Request object, containing all parsed information from the clients networkstream.
    /// </summary>
    public class Request : IRequest
    {
        private bool _IsValid = false;
        private string _Method = "";
        private IUrl _Url = new Url();
        private IDictionary<string, string> _Headers = new Dictionary<string, string>();
        private string _UserAgent = "";
        private int _HeaderCount = 0;
        private int _ContentLength = 0;
        private string _ContentType = "";
        private Stream _ContentStream = null;
        private string _ContentString = null;
        private byte[] _ContentBytes = null;
        private static readonly string[] ValidMethods = { "GET", "POST" };

        /// <summary>
        /// Parses given network stream and initializes the Request object.
        /// </summary>
        /// <param name="NetworkStream">
        /// The network stream from the client, that is supposed to be parsed.
        /// </param>
        public Request(Stream NetworkStream)
        {
            StreamReader ContentReader = new StreamReader(NetworkStream);
            string infeed = "";
            string[] splitfeed = { };
            string headerKey = "";
            string headerVal = "";
            bool check_content = false;
            bool break_flag = false;

            infeed = ContentReader.ReadLine();
            splitfeed = infeed.Split(' ');

            if (splitfeed.Length >= 2)
            {
                _Method = splitfeed[0].ToUpper();
                _Url = new Url(splitfeed[1]);
            }

            // For POST requests no "EndOfStream"
            while (!break_flag)
            {

                infeed = !check_content ? ContentReader.ReadLine() : "";
                infeed = infeed ?? "";
                splitfeed = infeed.Contains(':') ? new string[] { infeed.Substring(0, infeed.IndexOf(':')), infeed.Substring(infeed.IndexOf(':') + 2) } : null;

                if ((splitfeed != null) && !check_content)
                {
                    headerKey = splitfeed[0].ToLower();
                    headerVal = splitfeed[1];

                    ++_HeaderCount;
                    _Headers.Add(headerKey, headerVal);
                }

                if(string.IsNullOrWhiteSpace(infeed))
                {
                    break_flag = check_content ? true : false;
                    check_content = true;
                }
            }

            _UserAgent = _Headers.ContainsKey("user-agent") ? _Headers["user-agent"] : _UserAgent;
            _ContentType = _Headers.ContainsKey("content-type") ? _Headers["content-type"] : _ContentType;
            _ContentLength = _Headers.ContainsKey("content-length") ? Int32.Parse(_Headers["content-length"]) : 0;

            if(_ContentLength > 0)
            {
                char[] net_content = new char[_ContentLength];
                int size = ContentReader.ReadBlock(net_content, 0, _ContentLength);

                _ContentString = new string(net_content);

                _ContentBytes = Encoding.UTF8.GetBytes(_ContentString);

                _ContentStream = new MemoryStream();
                StreamWriter ContentWriter = new StreamWriter(_ContentStream);
                ContentWriter.Write(_ContentString);
                ContentWriter.Flush();
                _ContentStream.Position = 0;
            }

            _IsValid = ValidMethods.Contains(_Method);
        }

        #region Getter
        /// <summary>
        /// Returns true if the request is valid. A request is valid, if method and url could be parsed. A header is not necessary.
        /// </summary>
        public bool IsValid
        {
            get { return _IsValid; }
        }

        /// <summary>
        /// Returns the request method in UPPERCASE. get -> GET.
        /// </summary>
        public string Method
        {
            get { return _Method; }
        }

        /// <summary>
        /// Returns a URL object of the request. Never returns null.
        /// </summary>
        public IUrl Url
        {
            get { return _Url; }
        }

        /// <summary>
        /// Returns the request header. Never returns null. All keys must be lower case.
        /// </summary>
        public IDictionary<string, string> Headers
        {
            get { return _Headers; }
        }

        /// <summary>
        /// Returns the user agent from the request header
        /// </summary>
        public string UserAgent
        {
            get { return _UserAgent; }
        }

        /// <summary>
        /// Returns the number of header or 0, if no header where found.
        /// </summary>
        public int HeaderCount
        {
            get { return _HeaderCount; }
        }

        /// <summary>
        /// Returns the parsed content length request header.
        /// </summary>
        public int ContentLength
        {
            get { return _ContentLength; }
        }

        /// <summary>
        /// Returns the parsed content type request header. Never returns null.
        /// </summary>
        public string ContentType
        {
            get { return _ContentType; }
        }

        /// <summary>
        /// Returns the request content (body) stream or null if there is no content stream.
        /// </summary>
        public Stream ContentStream
        {
            get { return _ContentStream; }
        }

        /// <summary>
        /// Returns the request content (body) as string or null if there is no content.
        /// </summary>
        public string ContentString
        {
            get { return _ContentString; }
        }

        /// <summary>
        /// Returns the request content (body) as byte[] or null if there is no content.
        /// </summary>
        public byte[] ContentBytes
        {
            get { return _ContentBytes; }
        }
        #endregion
    }
}
