using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp.IntegrationTests
{
    public class ListConfig
    {
        public static string Url => ConfigurationManager.AppSettings["Url"];
        public static string Password => ConfigurationManager.AppSettings["Password"];
    }

    public class TestList: MailmanList
    {
        public TestList(): base(ListConfig.Url, ListConfig.Password)
        {
        }
    }
}
