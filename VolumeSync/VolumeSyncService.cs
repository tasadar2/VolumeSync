using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VolumeSync.Configuration;

namespace VolumeSync
{
    public class VolumeSyncService : IHostedService, IObserver<DeviceVolumeChangedArgs>, IObserver<DeviceMuteChangedArgs>
    {
        private readonly IAudioController<CoreAudioDevice> _audioController;
        private readonly ILogger _logger;
        private readonly IOptions<VolumeSyncOptions> _options;
        private readonly List<SecondaryDevice> _secondaries = new List<SecondaryDevice>();
        private IDevice _primary;

        public VolumeSyncService(IAudioController<CoreAudioDevice> audioController, IOptions<VolumeSyncOptions> options, ILogger<VolumeSyncService> logger)
        {
            _audioController = audioController;
            _options = options;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            IDevice defaultDevice = null;
            var secondaryOptions = new List<SecondaryDeviceOptions>();
            if (_options.Value.SecondaryDevices != null)
            {
                secondaryOptions.AddRange(_options.Value.SecondaryDevices);
            }

            foreach (var device in _audioController.GetDevices(DeviceType.All, DeviceState.All))
            {
                if (_primary == null && IsDevice(device, _options.Value.Primary))
                {
                    _primary = device;
                }

                if (device.IsDefaultDevice && device.IsPlaybackDevice)
                {
                    defaultDevice = device;
                }

                var secondary = secondaryOptions.FirstOrDefault(sd => IsDevice(device, sd.Name));
                if (secondary != null)
                {
                    secondaryOptions.Remove(secondary);
                    _secondaries.Add(new SecondaryDevice
                    {
                        Device = device,
                        Multiplier = secondary.Multiplier ?? VolumeSyncDefaults.Multiplier,
                        RateConstant = secondary.RateConstant ?? VolumeSyncDefaults.RateConstant,
                        Exponent = secondary.Exponent ?? VolumeSyncDefaults.Exponent,
                        Offset = secondary.Offset ?? VolumeSyncDefaults.Offset,
                    });
                    device.VolumeChanged.Subscribe(this);
                    device.MuteChanged.Subscribe(this);
                    _logger?.LogInformation($"Found secondary {device.FullName} ({device.Id:D})");
                }
            }

            if (_primary == null && defaultDevice != null)
            {
                _primary = defaultDevice;
            }

            if (_primary != null && _secondaries.Any())
            {
                _primary.VolumeChanged.Subscribe(this);
                _primary.MuteChanged.Subscribe(this);
                _logger?.LogInformation($"Found primary {_primary.FullName} ({_primary.Id:D})");
                _logger?.LogInformation("VolumeSync started successfully.");
            }
            else
            {
                if (_primary == null)
                {
                    _logger?.LogWarning("Primary audio device not found.");
                }

                if (!_secondaries.Any())
                {
                    _logger?.LogWarning("No secondary audio devices found.");
                }
            }

            foreach (var secondaryOption in secondaryOptions) _logger?.LogWarning($"Secondary audio device {secondaryOption.Name} was not found.");

            SyncVolume();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void OnNext(DeviceVolumeChangedArgs value)
        {
            if (value.Device.Id == _primary.Id)
            {
                SyncSecondaryVolume(value.Volume);
            }
        }

        public void OnNext(DeviceMuteChangedArgs value)
        {
            if (value.Device.Id == _primary.Id)
            {
                SyncSecondaryMute(value.IsMuted);
            }
        }

        public void OnError(Exception error)
        { }

        public void OnCompleted()
        { }

        private static bool IsDevice(IDevice device, string name)
        {
            return Guid.TryParse(name, out var id) && device.Id == id || device.FullName == name;
        }

        private void SyncVolume()
        {
            if (_primary != null)
            {
                SyncSecondaryVolume(_primary.Volume);
            }
        }

        private void SyncSecondaryVolume(double volume)
        {
            foreach (var secondary in _secondaries)
            {
                var result = Math.Pow(volume + secondary.RateConstant, secondary.Exponent) * secondary.Multiplier + secondary.Offset;
                _logger?.LogDebug($"Setting secondary volume (({volume} + {secondary.RateConstant}) ^ {secondary.Exponent} * {secondary.Multiplier} + {secondary.Offset}) = {result}");
                secondary.Device.Volume = result;
            }
        }

        private void SyncSecondaryMute(bool mute)
        {
            foreach (var secondary in _secondaries)
            {
                _logger?.LogDebug($"Setting secondary mute {mute}");
                secondary.Device.Mute(mute);
            }
        }
    }
}