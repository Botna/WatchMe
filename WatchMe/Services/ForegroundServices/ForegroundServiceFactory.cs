using WatchMe.Helpers;

namespace WatchMe.Services.ForegroundServices
{
    public static class ForegroundServiceFactory
    {
        public static IForegroundService GetServiceFromEnumBundle(string bundle)
        {
            if (Enum.TryParse<ForegroundServiceEnum>(bundle, out var service))
            {
                switch (service)
                {
                    case ForegroundServiceEnum.VUFS:
                        return CurrentServiceProvider.Services.GetService<VideoUploadForegroundService>();
                    default: throw new Exception("unable to parse foregroundserviceenum to appropriate type");
                }
            }
            else
            {
                throw new Exception("unable to parse foregroundserviceenum to appropriate type");
            }
        }
    }
}
