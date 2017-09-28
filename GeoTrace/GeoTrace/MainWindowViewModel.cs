using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.Maps.MapControl.WPF;
using PrimS.Telnet;

namespace GeoTrace
{
    public class MainWindowViewModel
    {
        private readonly Map _map;
        private MapLayer _pinLayer;
        private string _routerUri;

        public MainWindowViewModel(Map map)
        {
            _routerUri = ConfigurationManager.AppSettings["Router"];
            _map = map;
            _pinLayer = new MapLayer();
            _map.Children.Add(_pinLayer);

        }

        public async void Run()
        {

            var connections = await GetConnectionInfo();
            PlotConnections(connections);

        }


        public class TraceRouteInfo
        {
            public string Protocol { get; set; }
            public string SourceIpAddress { get; set; }
            public string SourcePort { get; set; }
            public string DestinationIpAddress { get; set; }
            public string DestinationPort { get; set; }
            public string Status { get; set; }
        }
        public async Task<List<TraceRouteInfo>> GetConnectionInfo()
        {

            using (Client client = new Client(_routerUri, 23, new System.Threading.CancellationToken()))
            {
                var r = await client.TryLoginAsync(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"], 5000);


                var connectionList= await GetIpConnections(client);


                var client_info_tmp = await ExecuteCommandAsync(client, "nvram get client_info_tmp");
                var client_info_tmp_clean = client_info_tmp.Split(',');
                var custom_clientlist = await ExecuteCommandAsync(client, "nvram get custom_clientlist");
              

                return connectionList;
            }


        }

        private static async Task<string> ExecuteCommandAsync(Client client,string command)
        {
            await client.WriteLine(command);
            return await client.ReadAsync(TimeSpan.FromSeconds(20));
        }

        private static async Task<List<TraceRouteInfo>> GetIpConnections(Client client)
        {
           
            string s = await ExecuteCommandAsync(client, "netstat-nat -no");
            var lines = s.Split('\r');

            var connections = new List<TraceRouteInfo>();
            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                var lineParts = line.Trim().Split(' ').Where(x => x != string.Empty).ToList();
                var sourceAddressParts = lineParts[1].Split(':');
                var destinationAddressParts = lineParts[2].Split(':');
                var connection = new TraceRouteInfo()
                {
                    Protocol = lineParts[0],
                    SourceIpAddress = sourceAddressParts[0],
                    SourcePort = sourceAddressParts[1],
                    DestinationIpAddress = destinationAddressParts[0],
                    DestinationPort = destinationAddressParts[1],
                    Status = lineParts[3]
                };
                connections.Add(connection);
            }

            return connections;
        }


        private void PlotConnections(List<TraceRouteInfo> connections)
        {
            CityResponse city = null;
            // This creates the DatabaseReader object, which should be reused across
            // lookups.
            using (var reader = new DatabaseReader(@"C:\Users\johns\Downloads\GeoLite2-City_20170905\GeoLite2-City_20170905\GeoLite2-City.mmdb"))
            {
                foreach (var connection in connections)
                {


                    // Replace "City" with the appropriate method for your database, e.g.,
                    // "Country".
                    city = reader.City(connection.DestinationIpAddress);

                    var cityLocation = city.Location;
                    var cityName = city.City.Name;

                    if (cityName != null && cityLocation != null && cityLocation.HasCoordinates)
                    {

                        Pushpin pin = new Pushpin
                        {
                            Location = new Location(cityLocation.Latitude.Value, cityLocation.Longitude.Value)
                        };
                        _pinLayer.AddChild(new TextBox() { Text = cityName }, pin.Location);
                        // Adds the pushpin to the map.
                        _map.Children.Add(pin);
                    }
                    else
                    {
                        //unknown city
                    }
                }
            }



        }

    }
}
