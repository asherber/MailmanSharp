using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace MailmanSharp.IntegrationTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public abstract class BaseTests
    {
        protected IMailmanList _list = new TestList();

        private static JObject _configObj;
        
        static BaseTests()
        {
            var rand = new Random();
            var randInt = rand.Next(20, 120);
            var randDouble = (rand.Next(100, 999) / 10.0);

            var configString = File.ReadAllText("SampleConfig.json");
            configString = configString.Replace("guid", Guid.NewGuid().ToString());
            configString = Regex.Replace(configString, "(?<=: )99.9", randDouble.ToString());
            configString = Regex.Replace(configString, "(?<=: )99", randInt.ToString());

            _configObj = JObject.Parse(configString);
        }


        [Fact]
        public abstract Task P10_ReadValues();

        [Fact]
        public abstract Task P20_ChangeAndSave();

        [Fact]
        public abstract Task P30_ReadAndVerifyChanges();

        [Fact]
        public abstract Task P40_LoadJsonAndSave();

        [Fact]
        public abstract Task P50_ReadAndVerifyJson();

        protected ushort Inc(ushort? value) => (ushort)((value.Value + 1) % 10);
        protected double Inc(double? value) => ((Math.Truncate(value.Value) + 1) % 10) + 0.5;

        protected T Inc<T>(T? input) where T: struct, IConvertible
        {
            var count = Enum.GetValues(typeof(T)).Length;
            var value = Convert.ToInt32(input);
            var newValue = ++value % count;
            return (T)Enum.ToObject(typeof(T), newValue);
        }

        protected string GuidEmail() => $"{Guid.NewGuid()}@example.com";
        protected List<string> GuidEmailArray(int count) => Enumerable.Range(0, count).Select(i => GuidEmail()).ToList();

        protected string SampleConfig(string section)
        {
            return new JObject(_configObj.Property(section)).ToString();
        }
    }
}
