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
    public class MailNewsGatewaysTests : BaseTests
    {
        private static MailNewsGatewaysSection _saved;
        private static string _config;

        private MailNewsGatewaysSection Section => _list.MailNewsGateways;

        public override async Task P10_ReadValues()
        {
            await Section.ReadAsync();

            Section.LinkedNewsgroup.Should().NotBeNull();
            Section.GatewayToNews.Should().NotBeNull();
            Section.NewsModeration.Should().NotBeNull();
            Section.NewsPrefixSubjectToo.Should().NotBeNull();

            _saved = Section;
        }

        public override async Task P20_ChangeAndSave()
        {
            _saved.NntpHost = Guid.NewGuid().ToString();
            _saved.LinkedNewsgroup = Guid.NewGuid().ToString();
            _saved.GatewayToNews = !_saved.GatewayToNews;
            _saved.NewsModeration = Inc(_saved.NewsModeration);
            _saved.NewsPrefixSubjectToo = !_saved.NewsPrefixSubjectToo;
            
            await _saved.WriteAsync();
        }

        public override async Task P30_ReadAndVerifyChanges()
        {
            await Section.ReadAsync();
            Section.Should().BeEquivalentTo(_saved);
        }

        public override async Task P40_LoadJsonAndSave()
        {
            _config = SampleConfig("MailNewsGateways");
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
        public async Task Gatewaying_Requires_Fields()
        {
            await Section.ReadAsync();
            Section.NntpHost = "";
            Section.LinkedNewsgroup = "";
            Section.GatewayToMail = true;

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"You cannot enable gatewaying unless both the news server field and the linked newsgroup fields are filled in.");
        }
    }
}
