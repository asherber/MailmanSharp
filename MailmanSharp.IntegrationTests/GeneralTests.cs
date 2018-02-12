using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MailmanSharp.IntegrationTests
{
    public class GeneralTests : BaseTests
    {
        private static GeneralSection _saved;
        private static string _config;

        private GeneralSection Section => _list.General;

        public override async Task P10_ReadValues()
        {
            await Section.ReadAsync();

            Section.Description.Should().NotBeNull();
            Section.SendReminders.Should().NotBeNull();
            Section.MaxMessageSize.Should().NotBeNull();

            _saved = Section;
        }

        public override async Task P20_ChangeAndSave()
        {
            _saved.Description = Guid.NewGuid().ToString();
            _saved.SendReminders = !_saved.SendReminders.Value;
            _saved.MaxMessageSize = Inc(_saved.MaxMessageSize);
            _saved.FromIsList = Inc(_saved.FromIsList.Value);
            _saved.Moderator = new List<string>() { GuidEmail(), GuidEmail() };
            _saved.Info = Guid.NewGuid().ToString();

            await _saved.WriteAsync();
        }

        public override async Task P30_ReadAndVerifyChanges()
        {
            await Section.ReadAsync();
            Section.Should().BeEquivalentTo(_saved);
        }

        public override async Task P40_LoadJsonAndSave()
        {
            _config = SampleConfig("General");
            Section.LoadConfig(_config);
            await Section.WriteAsync();
        }

        public override async Task P50_ReadAndVerifyJson()
        {
            var expected = JObject.Parse(_config);

            await Section.ReadAsync();
            var output = JObject.Parse(Section.CurrentConfig);
            output.Should().BeEquivalentTo(expected);
        }
    }
}
