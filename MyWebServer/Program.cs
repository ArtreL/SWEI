using BIF.SWE1.Interfaces;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MyWebServer
{
    class Program
    {
        public static MutexMan MutexCollection = new MutexMan();

        static void Main(string[] args)
        {
            PluginManager PlugMan = new PluginManager();

            IPAddress IpAddress = IPAddress.Parse("127.0.0.1");
            TcpListener Listener = new TcpListener(IpAddress, 8080);

            Console.WriteLine("Waiting for connections...");

            Listener.Start();

            var TempSensorThread = new Thread(() =>
            {
                string time;
                double temp;

                while (true)
                {
                    time = DateTime.Now.ToString("yyyy-dd-MM HH:mm:ss");
                    temp = (new Random().Next(-40000, 40000)) / 1000F;

                    using (SqlConnection db = new SqlConnection(@"Data Source=(local); Initial Catalog=MyWebServerDB; Integrated Security=true;"))
                    {
                        db.Open();

                        SqlCommand query = new SqlCommand(@"INSERT INTO [TempData] ([d_time], [d_temp]) VALUES (@time, @temp)", db);

                        query.Parameters.AddWithValue("@time", time);
                        query.Parameters.AddWithValue("@temp", temp);

                        query.ExecuteNonQuery();
                        Console.WriteLine("TempSensor: Inserted Data");

                        db.Close();
                    }

                    Thread.Sleep(10000);
                }
            });

            TempSensorThread.Start();

            while (true)
            {
                TcpClient Client = Listener.AcceptTcpClient();
                Console.WriteLine("Connection accepted.");

                PlugMan.UpdatePlugins();

                try
                {
                    var ChildSocketThread = new Thread(() =>
                    {
                        bool do_this = true;
                        NetworkStream BrowserNetworkStream = Client.GetStream();
                        StreamWriter NetWriter = new StreamWriter(BrowserNetworkStream);

                        Request BrowserRequest = new Request(BrowserNetworkStream);

                        try
                        {
                            if (BrowserRequest.Url.RawUrl.Contains("favicon"))
                            {
                                throw new FUFaviconException();
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Favicon Request Eliminated");
                            do_this = false;
                        }

                        if (do_this)
                        {
                            IPlugin HandlePlugin = PlugMan.GetPlugin(BrowserRequest);

                            IResponse PluginResponse = HandlePlugin.Handle(BrowserRequest);
                            PluginResponse.Send(BrowserNetworkStream);
                        }

                        Client.Close();
                    });

                    ChildSocketThread.Start();
                }
                catch
                {
                    Console.WriteLine("Client done goofed!");
                }
            }
        }
    }
}
