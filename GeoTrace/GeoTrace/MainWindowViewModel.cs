using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Mvvm;
using GeoTrace.Annotations;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.Maps.MapControl.WPF;
using PrimS.Telnet;

namespace GeoTrace
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly Map _map;
        private MapLayer _pinLayer;
        private string _routerUri;
        private List<KnownDevice> _knownDevices = new List<KnownDevice>();
        private Dictionary<string, KnownDevice> _ipToMac = new Dictionary<string, KnownDevice>();
        private List<TraceRouteInfo> _currentConnections;
        private string _message;


        public MainWindowViewModel(Map map)
        {
            _routerUri = ConfigurationManager.AppSettings["Router"];
            _map = map;
            _pinLayer = new MapLayer();
            _map.Children.Add(_pinLayer);
            RefreshCommand = new DelegateCommand(Run);

        }

        public async void Run()
        {

            var connections = await GetConnectionInfo();
            AddSourceNameFromKnownDevices(connections);
            AddCityInfo(connections);
            PlotConnections(connections);
           
        
            Task.Run(() => AddHostInfo(connections));


        }

        private void AddSourceNameFromKnownDevices(List<TraceRouteInfo> connections)
        {
            string sourceText = "Unknown Source";

            foreach (var connection in connections)
            {
                KnownDevice sourceDevice = null;
                if (_ipToMac.TryGetValue(connection.SourceIpAddress, out sourceDevice))
                {
                    connection.SourceName = sourceDevice.Name;
                }
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (value == _message) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        private void AddHostInfo(List<TraceRouteInfo> connections)
        {
            foreach (var connection in connections)
            {


                try
                {
                    var ipDestination = Dns.GetHostEntry(connection.DestinationIpAddress);
                    connection.DestinationName = ipDestination.HostName;

                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error on resolving IP address {connection.DestinationIpAddress}" + e);

                }

                if (connection.SourceName == null)
                {
                    try
                    {
                        var ipSource = Dns.GetHostEntry(connection.SourceIpAddress);
                        connection.SourceName = ipSource.HostName;

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error on resolving IP address {connection.SourceIpAddress}" + e);

                    }
                }


            }

        }

        private void AddCityInfo(List<TraceRouteInfo> connections)
        {
            CityResponse city = null;
            // This creates the DatabaseReader object, which should be reused across
            // lookups.
            using (var reader =
                new DatabaseReader(
                    @"C:\Users\johns\Downloads\GeoLite2-City_20170905\GeoLite2-City_20170905\GeoLite2-City.mmdb"))
            {
                foreach (var connection in connections)
                {

                    try
                    {

                        city = reader.City(connection.DestinationIpAddress);
                        connection.DestinationCity = city;



                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error on lookup IP address {connection.SourceIpAddress}" + e);

                    }
                }
            }
        }

        public List<TraceRouteInfo> CurrentConnections
        {
            get { return _currentConnections; }
            set
            {
                if (Equals(value, _currentConnections)) return;
                _currentConnections = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand { get; set; }


        private void PlotConnections(List<TraceRouteInfo> connections)
        {
            _map.Children.Clear();
            CurrentConnections = connections;


            foreach (var connection in connections)
            {





                if (connection.DestinationCity != null && connection.DestinationCity.Location != null && connection.DestinationCity.Location.HasCoordinates)
                {

                    Pushpin pin = new Pushpin
                    {
                        Location = new Location(connection.DestinationCity.Location.Latitude.Value, connection.DestinationCity.Location.Longitude.Value)
                    };







                    _pinLayer.AddChild(new TextBox() { Text = $"'{connection.SourceName}'\r\nto\r\n'{connection.DestinationCity.City}' ({connection.DestinationName})\r\n[{connection.Status}]" }, pin.Location);
                    // Adds the pushpin to the map.
                    _map.Children.Add(pin);
                }
                else
                {
                    //unknown city
                    Debug.WriteLine($"Unknown location for IP address {connection.SourceIpAddress}");
                }



            }



        }

        public async Task<List<TraceRouteInfo>> GetConnectionInfo()
        {

            using (Client client = new Client(_routerUri, 23, new System.Threading.CancellationToken()))
            {
                var r = await client.TryLoginAsync(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"], 5000);


                var connectionList = await GetIpConnections(client);



                var customList = (await ExecuteCommandAsync(client, "nvram get custom_clientlist")).Split('<');
                var asusDevice = (await ExecuteCommandAsync(client, "nvram get asus_device_list")).Split('<');
                var fromDHCPLease = (await ExecuteCommandAsync(client, "cat /var/lib/misc/dnsmasq.leases")).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var client_info_tmp = (await ExecuteCommandAsync(client, "nvram get client_info_tmp")).Split(',');
                //nmp_custom_clientlist give list of all clients that have been connected?
                var nmp_custom_clientlist = (await ExecuteCommandAsync(client, "cat /jffs/nmp_client_list")).Split('<');

                foreach (var item in fromDHCPLease)
                {
                    var parts = item.Split(' ');
                    var mac = parts[1].ToLower().Replace(":", "");
                    var ip = parts[2];
                    var name = parts[3];

                    var knownDevice = _knownDevices.FirstOrDefault(x => x.MAC == mac);
                    if (knownDevice == null)
                    {
                        knownDevice = new KnownDevice { MAC = mac, Name = name };
                        _knownDevices.Add(knownDevice);
                    }
                    AddIpToMac(ip, knownDevice);
                }

                foreach (var item in client_info_tmp.Where(x => x.Trim() != ""))
                {
                    var parts = item.Split('>');
                    var name = parts[1];
                    var ip = parts[2];
                    var mac = parts[3].ToLower().Replace(":", "");

                    var knownDevice = _knownDevices.FirstOrDefault(x => x.MAC == mac);
                    if (knownDevice == null)
                    {
                        knownDevice = new KnownDevice { MAC = mac, Name = name };

                        _knownDevices.Add(knownDevice);
                    }

                    AddIpToMac(ip, knownDevice);

                }

                foreach (var item in customList.Where(x => x.Trim() != ""))
                {
                    var parts = item.Split('>');
                    var name = parts[0];
                    var mac = parts[1].ToLower().Replace(":", "");

                    var knownDevice = _knownDevices.FirstOrDefault(x => x.MAC == mac);
                    if (knownDevice == null)
                    {
                        knownDevice = new KnownDevice { MAC = mac, Name = name };
                        _knownDevices.Add(knownDevice);
                    }

                }

                foreach (var item in nmp_custom_clientlist.Where(x => x.Trim() != ""))
                {
                    var parts = item.Split('>');
                    var name = parts[2];
                    var mac = parts[0].ToLower().Replace(":", "");

                    var knownDevice = _knownDevices.FirstOrDefault(x => x.MAC == mac);
                    if (knownDevice == null)
                    {
                        knownDevice = new KnownDevice { MAC = mac, Name = name };
                        _knownDevices.Add(knownDevice);
                    }

                }




                return connectionList;
            }


        }

        private void AddIpToMac(string ip, KnownDevice device)
        {
            if (_ipToMac.ContainsKey(ip))
                _ipToMac.Remove(ip);

            _ipToMac.Add(ip, device);
        }

        private static async Task<string> ExecuteCommandAsync(Client client, string command)
        {
            await client.WriteLine(command);
            var result = await client.ReadAsync(TimeSpan.FromSeconds(20));
            //remove first line
            var firstNewLineIndex = result.IndexOf("\r\n");
            result = result.Remove(0, firstNewLineIndex + 2);
            //var lines = result.Split(new []{"\r\n"},StringSplitOptions.RemoveEmptyEntries);
            //return lines.Skip(1).Take(lines.Length - 2).ToList();
            result = result.Replace("admin@RT-N66U:/tmp/home/root#", "").Trim();


            return result;
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
                    SourcePort = sourceAddressParts.Length > 1 ? sourceAddressParts[1] : null,
                    DestinationIpAddress = destinationAddressParts[0],
                    DestinationPort = destinationAddressParts.Length > 1 ? destinationAddressParts[1] : null,
                    Status = lineParts[3]
                };


                connections.Add(connection);
            }

            return connections;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
