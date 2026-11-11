using Microsoft.Extensions.Configuration;

namespace TchOpenSource.Models
{
    public static class OpenSourceConfig
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string CloudStorageContainerReference
        {
            get
            {
                return _configuration["CloudStorageContainerReference"];
            }
        }
        public static string CloudStorageConnectionString
        {
            get
            {
                return _configuration.GetConnectionString("StorageConnectionString");
            }
        }
    }
}
