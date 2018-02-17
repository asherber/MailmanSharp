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
    public class ArchivingTests : BaseTests
    {
        private static ArchivingSection _saved;
        private static string _config;

        private ArchivingSection Section => _list.Archiving;

        public override async Task P10_ReadValues()
        {
            await Section.ReadAsync();

            Section.Archive.Should().NotBeNull();
            Section.ArchivePrivate.Should().NotBeNull();
            Section.ArchiveVolumeFrequency.Should().NotBeNull();

            _saved = Section;
        }

        public override async Task P20_ChangeAndSave()
        {
            _saved.Archive = !_saved.Archive;
            _saved.ArchivePrivate = Inc(_saved.ArchivePrivate);
            _saved.ArchiveVolumeFrequency = Inc(_saved.ArchiveVolumeFrequency);

            await _saved.WriteAsync();
        }

        public override async Task P30_ReadAndVerifyChanges()
        {
            await Section.ReadAsync();
            Section.Should().BeEquivalentTo(_saved);
        }

        public override async Task P40_LoadJsonAndSave()
        {
            _config = SampleConfig("Archiving");
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
