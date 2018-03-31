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
    public class TopicsTests : BaseTests
    {
        private static TopicsSection _saved;
        private static string _config;

        private TopicsSection Section => _list.Topics;

        public override async Task P10_ReadValues()
        {
            await Section.ReadAsync();

            Section.TopicsEnabled.Should().NotBeNull();
            Section.TopicsBodylinesLimit.Should().NotBeNull();
            Section.Topics.Should().NotBeNull()
                .And.HaveCountGreaterOrEqualTo(1);

            _saved = Section;
        }

        public override async Task P20_ChangeAndSave()
        {
            _saved.TopicsEnabled = !_saved.TopicsEnabled;
            _saved.TopicsBodylinesLimit = Inc(_saved.TopicsBodylinesLimit);
            _saved.Topics = new List<Topic>()
            {
                new Topic(Guid.NewGuid().ToString(), new List<string>() { Guid.NewGuid().ToString() }, Guid.NewGuid().ToString())
            };

            await _saved.WriteAsync();
        }

        public override async Task P30_ReadAndVerifyChanges()
        {
            await Section.ReadAsync();
            Section.Should().BeEquivalentTo(_saved);
        }

        public override async Task P40_LoadJsonAndSave()
        {
            _config = SampleConfig("Topics");
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
