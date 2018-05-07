using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;

namespace MyWebServer
{
    /// <summary>
    /// Implements a Url object, containing parsed information about a given URL.
    /// </summary>
    public class Url : IUrl
    {
        private IDictionary<string, string> _Parameter = new Dictionary<string, string>();
        private int _ParameterCount = 0;
        private string _Path = null;
        private string _RawUrl = null;
        private string _Extension = "";
        private string _FileName = "";
        private string _Fragment = "";
        private string[] _Segments = new string[] { };
        
        /// <summary>
        /// Initializes an empty Url object to work with.
        /// </summary>
        public Url()
        {
            // Nothing happens here
        }
        
        /// <summary>
        /// Initializes a Url object with parsed information from the given string.
        /// </summary>
        /// <param name="raw">
        /// The string, that is supposed to be parsed for information.
        /// </param>
        public Url(string raw)
        {
            if (!string.IsNullOrWhiteSpace(raw))
            {
                _RawUrl = raw; // == null ? null : "//test.jpg#fragment?one=bla&two=blub"; // Debug

                // check if there is either '?' or '#' in the raw URL
                // and set the lower one of them to 'lower_index'
                int q_index = _RawUrl.IndexOf('?');
                int h_index = _RawUrl.IndexOf('#');
                int lower_index = h_index < q_index ? (h_index == -1 ? q_index : h_index) : (q_index == -1 ? h_index : q_index);

                // if we found a '?' or '#', we take the substring up until the first of those characters
                _Path = lower_index != -1 ? _RawUrl.Substring(0, lower_index) : _RawUrl;

                // segments are all parts between '/'
                // the first element is empty though => '//'
                _Segments = _Path.Split('/').Skip(1).ToArray();

                // get filename
                _FileName = _Segments.Last().Contains('.') ? _Segments.Last().Substring(0, _Segments.Last().LastIndexOf('.')) : null;
                // get file extension
                _Extension = _Segments.Last().Contains('.') ? _Segments.Last().Split('?', '#').First().Substring(_Segments.Last().LastIndexOf('.')) : null;

                // if we found a '?', we take the string from the '?' to the end, otherwise the substring will be null
                string ParamSubRaw = q_index != -1 ? _RawUrl.Substring(q_index + 1) : null;
                // set characters to split the string at
                char[] delimiters = new char[] { '=', '&', '#' }; 
                // Alternative => var delimiters = new [] { '=', '&', '#' };
                // if the substring isn't null, it's split at the specified delimiters, otherwise it'll be an empty string
                string[] ParamSplit = ParamSubRaw?.Split(delimiters) ?? new string[] { };

                // calculate the number of parameters by halving the number of elements in the split array
                // if there was a fragment afterwards, it'll be ignored by the integer
                _ParameterCount = ParamSplit.Length / 2;

                // declare variables for the upcoming loop
                string ParameterKey = "";
                string ParameterValue = "";

                for (int i = 0; i < (_ParameterCount * 2); ++i)
                {
                    // set key and value every odd and even loop
                    if ((i % 2) == 0)
                    {
                        ParameterKey = ParamSplit[i];
                    }
                    else
                    {
                        ParameterValue = ParamSplit[i];
                        // at the end of the odd loop, put both variables into the dictionary
                        _Parameter.Add(ParameterKey, ParameterValue);
                    }
                }

                if (h_index != -1)
                {
                    // if we found a '#', we extract the fragment
                    delimiters = new char[] { '#', '?' };
                    // if the '#' is at the lower_index, either there are no parameters or it is in front of them
                    // in either case it'll be at the second element in the split array
                    // if the '#' is not at the lower_index, it'll be the last element of the split array
                    _Fragment = h_index == lower_index ? _RawUrl.Split(delimiters).ElementAt(1) : _RawUrl.Split(delimiters).Last();
                }
            }
        }

        #region Getter
        /// <summary>
        /// Returns a dictionary with the parameter of the url. Never returns null.
        /// </summary>
        public IDictionary<string, string> Parameter
        {
            get { return _Parameter; }
        }

        /// <summary>
        /// Returns the number of parameter of the url. Returns 0 if there are no parameter.
        /// </summary>
        public int ParameterCount
        {
            get { return _ParameterCount; }
        }

        /// <summary>
        /// Returns the path of the url, without parameter.
        /// </summary>
        public string Path
        {
            get { return _Path; }
        }

        /// <summary>
        /// Returns the raw url.
        /// </summary>
        public string RawUrl
        {
            get { return _RawUrl; }
        }

        /// <summary>
        /// Returns the extension of the url filename, including the leading dot. If the url contains no filename, a empty string is returned. Never returns null.
        /// </summary>
        public string Extension
        {
            get { return _Extension; }
        }

        /// <summary>
        /// Returns the filename (with extension) of the url path. If the url contains no filename, a empty string is returned. Never returns null. A filename is present in the url, if the last segment contains a name with at least one dot.
        /// </summary>
        public string FileName
        {
            get { return _FileName; }
        }

        /// <summary>
        /// Returns the url fragment. A fragment is the part after a '#' char at the end of the url. If the url contains no fragment, an empty string is returned. Never returns null.
        /// </summary>
        public string Fragment
        {
            get { return _Fragment; }
        }

        /// <summary>
        /// Returns the segments of the url path. A segment is divided by '/' chars. Never returns null.
        /// </summary>
        public string[] Segments
        {
            get { return _Segments; }
        }
        #endregion
    }
}