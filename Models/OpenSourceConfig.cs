using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TchOpenSource.Models
{
    public static class OpenSourceConfig
    {
        public static string CloudStorageContainerReference
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("CloudStorageContainerReference");
            }
        }
        public static string CloudStorageConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            }
        }
    }
}
