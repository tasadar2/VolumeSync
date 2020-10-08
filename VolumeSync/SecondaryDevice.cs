using AudioSwitcher.AudioApi;

namespace VolumeSync
{
    public class SecondaryDevice
    {
        public IDevice Device { get; set; }
        public double Multiplier { get; set; }
        public double RateConstant { get; set; }
        public double Exponent { get; set; }
        public double Offset { get; set; }
    }
}