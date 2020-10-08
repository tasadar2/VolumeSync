using System.Threading.Tasks;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VolumeSync.Configuration;

namespace VolumeSync
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                      .ConfigureServices(ConfigureServices)
                      .UseWindowsService()
                      .Build()
                      .RunAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<VolumeSyncOptions>(context.Configuration);

            services.AddSingleton<IAudioController<CoreAudioDevice>, CoreAudioController>();

            services.AddHostedService<VolumeSyncService>();
        }
    }
}