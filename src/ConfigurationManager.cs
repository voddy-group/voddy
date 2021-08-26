using System.IO;
using Microsoft.Extensions.Configuration;

namespace voddy {
    public class ConfigurationManager {
        public static IConfiguration Configuration { get; set; }

        static ConfigurationManager() {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }
}