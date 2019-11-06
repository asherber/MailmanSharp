using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp.IntegrationTests
{
    public class ListConfig
    {
        public static string Url { get; }
        public static string Password { get; }

        static ListConfig()
        {
            var configFile = "AppConfig.json";
            if (!File.Exists(configFile))
                throw new FileNotFoundException("AppConfig.json must contain a Url and Password for the test list.");

            var config = new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .Build();

            Url = config["Url"];
            Password = config["Password"];
        }
    }

    public class TestList: MailmanList
    {
        public TestList(): base(ListConfig.Url, ListConfig.Password)
        {
        }
    }
}
