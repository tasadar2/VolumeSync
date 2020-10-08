using System.Collections.Generic;

namespace VolumeSync.Configuration
{
    public class VolumeSyncOptions
    {
        public string Primary { get; set; }
        public List<SecondaryDeviceOptions> SecondaryDevices { get; set; }
    }
}