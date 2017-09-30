namespace GeoTrace
{
    public class KnownDevice
    {
        public string MAC { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name} - {MAC}";
        }
    }
}