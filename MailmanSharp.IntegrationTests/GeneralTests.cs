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
            _saved.SendReminders = !_saved.SendReminders;
            _saved.MaxMessageSize = Inc(_saved.MaxMessageSize);
            _saved.FromIsList = Inc(_saved.FromIsList);
            _saved.Moderator = GuidEmailArray(2);
            _saved.Info = Guid.NewGuid().ToString();
            _saved.NewMemberOptions = NewMemberOptions.None;

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

        [Fact]
        public async Task Bad_Moderator_Should_Throw()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.Moderator.Add(guid);

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option moderator: {guid}");
        }

        [Fact]
        public async Task Bad_Owner_Should_Throw()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.Owner.Add(guid);

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option owner: {guid}");
        }

        [Fact]
        public async Task Bad_ReplyTo_Should_Throw()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.ReplyToAddress = guid;

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option reply_to_address: {guid}");
        }

        [Fact]
        public async Task ReplyToList_Needs_ReplyToAddress()
        {
            await Section.ReadAsync();
            Section.ReplyGoesToList = ReplyGoesToListOption.ExplicitAddress;
            Section.ReplyToAddress = "";
            
            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage("You cannot add a Reply-To: to an explicit address if that address is blank. Resetting these values.");
        }
    }
}
