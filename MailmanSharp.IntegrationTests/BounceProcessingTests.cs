﻿using FluentAssertions;
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
    public class BounceProcessingTests : BaseTests
    {
        private static BounceProcessingSection _saved;
        private static string _config;

        private BounceProcessingSection Section => _list.BounceProcessing;

        public override async Task P10_ReadValues()
        {
            await Section.ReadAsync();

            Section.BounceProcessing.Should().NotBeNull();
            Section.BounceScoreThreshold.Should().NotBeNull();
            Section.BounceYouAreDisabledWarningsInterval.Should().NotBeNull();

            _saved = Section;
        }

        public override async Task P20_ChangeAndSave()
        {
            _saved.BounceProcessing = !_saved.BounceProcessing;
            _saved.BounceScoreThreshold = Inc(_saved.BounceScoreThreshold);
            _saved.BounceYouAreDisabledWarningsInterval = Inc(_saved.BounceYouAreDisabledWarningsInterval);
            
            await _saved.WriteAsync();
        }

        public override async Task P30_ReadAndVerifyChanges()
        {
            await Section.ReadAsync();
            Section.Should().BeEquivalentTo(_saved);
        }

        public override async Task P40_LoadJsonAndSave()
        {
            _config = SampleConfig("BounceProcessing");
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
