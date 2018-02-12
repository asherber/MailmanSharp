using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace MailmanSharp.IntegrationTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public abstract class BaseTests
    {
        protected IMailmanList _list = new TestList();

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

        protected ushort Inc(ushort? value) => (ushort)(++value % 10);
        
        protected T Inc<T>(T input) where T: struct, IConvertible
        {
            var count = Enum.GetValues(typeof(T)).Length;
            var value = Convert.ToInt32(input);
            var newValue = ++value % count;
            return (T)Enum.ToObject(typeof(T), newValue);
        }

        protected string GuidEmail() => $"{Guid.NewGuid()}@example.com";

        protected string SampleConfig(string section)
        {
            var obj = JObject.Parse(File.ReadAllText("SampleConfig.json"));
            return new JObject(obj.Property(section)).ToString();
        }
    }
}
