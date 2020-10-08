namespace VolumeSync.Configuration
{
    public class SecondaryDeviceOptions
    {
        public string Name { get; set; }
        public double? Multiplier { get; set; }
        public double? RateConstant { get; set; }
        public double? Exponent { get; set; }
        public double? Offset { get; set; }
    }
}