using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;
using FluentAssertions.Json;

namespace MailmanSharp.IntegrationTests
{
    public class DigestTests : BaseTests
    {
        private static DigestSection _saved;
        private static string _config;

        private DigestSection Section => _list.Digest;

        public override async Task P10_ReadValues()
        {
            await Section.ReadAsync();

            Section.Digestable.Should().NotBeNull();
            Section.DigestIsDefault.Should().NotBeNull();
            Section.DigestFooter.Should().NotBeNull();

            _saved = Section;
        }

        public override async Task P20_ChangeAndSave()
        {
            _saved.Digestable = !_saved.Digestable;
            _saved.DigestIsDefault = Inc(_saved.DigestIsDefault);
            _saved.DigestFooter = Guid.NewGuid().ToString();

            await _saved.WriteAsync();
        }

        public override async Task P30_ReadAndVerifyChanges()
        {
            await Section.ReadAsync();
            Section.Should().BeEquivalentTo(_saved);
        }

        public override async Task P40_LoadJsonAndSave()
        {
            _config = SampleConfig("Digest");
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

        [Fact]
        public void NewVolume_Should_Work()
        {
            Func<Task> act = async () => await Section.NewVolumeAsync();
            act.Should().NotThrow();
        }

        [Fact]
        public void SendDigest_Should_Work()
        {
            Func<Task> act = async () => await Section.SendDigestNowAsync();
            act.Should().NotThrow();
        }
    }
}
