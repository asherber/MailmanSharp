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
    public class ContentFilteringTests : BaseTests
    {
        private static ContentFilteringSection _saved;
        private static string _config;

        private ContentFilteringSection Section => _list.ContentFiltering;

        public override async Task P10_ReadValues()
        {
            await Section.ReadAsync();

            Section.FilterContent.Should().NotBeNull();
            Section.PassMimeTypes.Should().NotBeNull();
            Section.CollapseAlternatives.Should().NotBeNull();
            Section.FilterAction.Should().NotBeNull();

            _saved = Section;
        }

        public override async Task P20_ChangeAndSave()
        {
            _saved.FilterContent = !_saved.FilterContent;
            _saved.PassMimeTypes = new List<string>() { Guid.NewGuid().ToString() };
            _saved.CollapseAlternatives = !_saved.CollapseAlternatives;
            _saved.FilterAction = Inc(_saved.FilterAction);
            
            await _saved.WriteAsync();
        }

        public override async Task P30_ReadAndVerifyChanges()
        {
            await Section.ReadAsync();
            Section.Should().BeEquivalentTo(_saved);
        }

        public override async Task P40_LoadJsonAndSave()
        {
            _config = SampleConfig("ContentFiltering");
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
