using System.ComponentModel;
using System.Runtime.CompilerServices;
using GeoTrace.Annotations;
using MaxMind.GeoIP2.Responses;

namespace GeoTrace
{
    public class TraceRouteInfo : INotifyPropertyChanged
    {
        private string _protocol;
        private string _sourceIpAddress;
        private string _sourcePort;
        private string _destinationIpAddress;
        private string _destinationPort;
        private string _status;
        private string _sourceName;
        private string _sourceMac;
        private string _destinationName;
        private CityResponse _destinationCity;

        public string Protocol
        {
            get { return _protocol; }
            set
            {
                if (value == _protocol) return;
                _protocol = value;
                OnPropertyChanged();
            }
        }

        public string SourceIpAddress
        {
            get { return _sourceIpAddress; }
            set
            {
                if (value == _sourceIpAddress) return;
                _sourceIpAddress = value;
                OnPropertyChanged();
            }
        }

        public string SourcePort
        {
            get { return _sourcePort; }
            set
            {
                if (value == _sourcePort) return;
                _sourcePort = value;
                OnPropertyChanged();
            }
        }

        public string DestinationIpAddress
        {
            get { return _destinationIpAddress; }
            set
            {
                if (value == _destinationIpAddress) return;
                _destinationIpAddress = value;
                OnPropertyChanged();
            }
        }

        public string DestinationPort
        {
            get { return _destinationPort; }
            set
            {
                if (value == _destinationPort) return;
                _destinationPort = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public string SourceName
        {
            get { return _sourceName; }
            set
            {
                if (value == _sourceName) return;
                _sourceName = value;
                OnPropertyChanged();
            }
        }

        public string SourceMAC
        {
            get { return _sourceMac; }
            set
            {
                if (value == _sourceMac) return;
                _sourceMac = value;
                OnPropertyChanged();
            }
        }

        public string DestinationName
        {
            get { return _destinationName; }
            set
            {
                if (value == _destinationName) return;
                _destinationName = value;
                OnPropertyChanged();
            }
        }

        public CityResponse DestinationCity
        {
            get { return _destinationCity; }
            set
            {
                if (Equals(value, _destinationCity)) return;
                _destinationCity = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}