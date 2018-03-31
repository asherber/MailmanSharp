using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions.Json;

namespace MailmanSharp.IntegrationTests
{
    public class AutoResponderTests : BaseTests
    {
        private static AutoResponderSection _saved;
        private static string _config;

        private AutoResponderSection Section => _list.AutoResponder;

        public override async Task P10_ReadValues()
        {
            await Section.ReadAsync();

            Section.AutorespondPostings.Should().NotBeNull();
            Section.AutoresponseAdminText.Should().NotBeNull();
            Section.AutorespondRequests.Should().NotBeNull();
            Section.AutoresponseGraceperiod.Should().NotBeNull();

            _saved = Section;
        }

        public override async Task P20_ChangeAndSave()
        {
            _saved.AutorespondPostings = !_saved.AutorespondPostings;
            _saved.AutoresponseAdminText = Guid.NewGuid().ToString();
            _saved.AutorespondRequests = Inc(_saved.AutorespondRequests);
            _saved.AutoresponseGraceperiod = Inc(_saved.AutoresponseGraceperiod);
            
            await _saved.WriteAsync();
        }

        public override async Task P30_ReadAndVerifyChanges()
        {
            await Section.ReadAsync();
            Section.Should().BeEquivalentTo(_saved);
        }

        public override async Task P40_LoadJsonAndSave()
        {
            _config = SampleConfig("AutoResponder");
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
