using BIF.SWE1.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Xml;

namespace MyWebServer.Plugins
{
    [AutoLoadPlugin]
    class NaviPlugin : IPlugin
    {
        public float CanHandle(IRequest req)
        {
            float retval = 0;
            bool handle_check = false;

            handle_check = req.ContentString != null ? (req.ContentString.Contains("street=") || req.ContentString.Contains("map=")) : false;

            if (handle_check)
            {
                retval = 1;
            }
            else
            {
                retval = 0;
            }

            return retval;
        }

        private static IDictionary<string, string> _PrepMap = new Dictionary<string, string>();
        private static IDictionary<string, string> _TempMap = new Dictionary<string, string>();
        private static string _OsmCity = null;
        private static string _OsmStreet = null;
        private static string _PostStreet = null;
        private static bool _Search = false;

        public IResponse Handle(IRequest req)
        {
            Response resp = new Response
            {
                StatusCode = 200
            };

            string content = null;

            bool legit_input = (req.ContentString.Contains("street=") && req.ContentString.Length > 7) || ((req.ContentString.Contains("map=") && req.ContentString.Length > 4));
            bool map_prepare = req.ContentString.Contains("map=");

            if (legit_input && req.Url.RawUrl != "/navi.php")
            {
                if (map_prepare)
                {
                    string status_text = "";

                    if (Program.MutexCollection.TryLockOsm())
                    {
                        var MapUpdate = new Thread(() => {
                            Update();
                            _PrepMap = new Dictionary<string, string>(_TempMap);
                            _TempMap.Clear();
                            Program.MutexCollection.ReleaseOsm();
                        });

                        MapUpdate.Start();

                        status_text = "The map is preparing for future use.";
                    }
                    else
                    {
                        status_text = "Map file already in use.";
                    }

                    //try
                    //{
                    //    Update();
                    //    _PrepMap = new Dictionary<string, string>(_TempMap);
                    //    _TempMap.Clear();
                    //    status_text = "The map has been prepared for future use.";
                    //}
                    //catch
                    //{
                    //    status_text = "Map file already in use.";
                    //}

                    string fdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\index.html";
                    content = File.Exists(fdir) ? File.ReadAllText(fdir).Replace("No street entered.", status_text) : status_text;
                }
                else
                {
                    _PostStreet = HttpUtility.UrlDecode(req.ContentString.Substring(7));
                    int citycounter = 0;
                    string navitext = "";

                    _PrepMap.TryGetValue(_PostStreet.ToLower(), out string temp);

                    if (temp != null)
                    {
                        citycounter = temp.Split('\n').Count() - 1;
                        navitext = temp;

                        navitext = citycounter.ToString() + " Orte gefunden\n" + navitext;
                    }
                    else
                    {
                        _Search = true;


                        if (Program.MutexCollection.TryLockOsm())
                        {
                            Update();
                            _PrepMap = new Dictionary<string, string>(_TempMap);
                            _TempMap.Clear();
                            Program.MutexCollection.ReleaseOsm();

                            _Search = false;

                            _PrepMap.TryGetValue(_PostStreet.ToLower(), out temp);

                            if (temp != null)
                            {
                                citycounter = temp.Split('\n').Count() - 1;
                                navitext = temp;
                            }

                            navitext = citycounter.ToString() + " Orte gefunden\n" + navitext;
                        }
                        else
                        {
                            navitext = "Map file already in use.";
                        }
                    }

                    string fdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\index.html";
                    content = File.Exists(fdir) ? File.ReadAllText(fdir).Replace("No street entered.", navitext) : navitext;
                }
            }
            else
            {
                content = "Bitte geben Sie eine Anfrage ein";
                content += req.Url.RawUrl == "/navi.php" ? " Orte gefunden" : "";
            }

            resp.SetContent(content);

            return resp;
        }

        private static void CheckAndInsertMapFeatures(string street, string city)
        {
            _TempMap.TryGetValue(street, out string temp);

            if (temp == null)
            {
                _TempMap.Add(street, city + "\n");
            }
            else
            {
                if (!_TempMap[street].Contains(city + "\n"))
                {
                    _TempMap[street] += city + "\n";
                }
            }
        }

        public static void Update()
        {
            string fdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\navi.osm";
            using (var fs = new FileStream(fdir, FileMode.Open))
            using (var xml = new XmlTextReader(fs))
            {
                while (xml.Read())
                {
                    if (xml.NodeType == XmlNodeType.Element
                    && xml.Name == "osm")
                    {
                        ReadOsm(xml);
                    }
                }
            }
        }

        private static void ReadOsm(XmlTextReader xml)
        {
            using (var osm = xml.ReadSubtree())
            {
                while (osm.Read())
                {
                    if (osm.NodeType == XmlNodeType.Element
                    && (osm.Name == "node" || osm.Name == "way"))
                    {
                        ReadAnyOsmElement(osm);
                    }
                }
            }
        }

        private static void ReadAnyOsmElement(XmlReader osm)
        {
            using (var element = osm.ReadSubtree())
            {
                while (element.Read())
                {
                    if (element.NodeType == XmlNodeType.Element
                    && element.Name == "tag")
                    {
                        ReadTag(element);
                    }
                }
            }
        }

        private static void ReadTag(XmlReader element)
        {
            string tagType = element.GetAttribute("k");
            string value = element.GetAttribute("v");
            switch (tagType)
            {
                case "addr:city":
                    _OsmCity = value;
                    break;
                case "addr:postcode":
                    if (_OsmStreet != "")
                    {
                        if (_Search)
                        {
                            if (_OsmStreet == _PostStreet.ToLower())
                            {
                                CheckAndInsertMapFeatures(_OsmStreet, _OsmCity + " / " + value);
                            }
                        }
                        else
                        {
                            CheckAndInsertMapFeatures(_OsmStreet, _OsmCity + " / " + value);
                        }
                    }
                    break;
                case "addr:street":
                    _OsmStreet = value.ToLower();
                    break;
            }
        }
    }
}

