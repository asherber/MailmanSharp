﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.IO;
using Xunit.Priority;

namespace MailmanSharp.IntegrationTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class MembershipTests
    {
        private static readonly MembershipSection _membership;
        private static readonly List<string> _emails;

        static MembershipTests()
        {
            _emails = File.ReadAllLines("emails.txt").ToList();
            var list = new TestList();
            _membership = list.Membership;
            var deleteTask = _membership.UnsubscribeAllAsync();
            Task.WaitAll(deleteTask);
        }

        private static char RandomLetter => (char)((DateTime.Now.Millisecond % 26) + (int)'a');

        private Task EnsureMembers()
        {
            if (!_membership.EmailList.Any())
                return _membership.SubscribeAsync(_emails);
            else
                return Task.CompletedTask;
        }

        [Fact, Priority(10)]
        public async Task List_Should_Be_Empty()
        {
            await _membership.ReadAsync();
            _membership.EmailList.Should().BeEmpty();
        }

        [Fact, Priority(20)]
        public async Task Subscribe_All_Should_Work()
        {
            var resp = await _membership.SubscribeAsync(_emails);
            resp.AlreadyMembers.Should().BeEmpty();
            resp.BadEmails.Should().BeEmpty();
            resp.Subscribed.Should().BeEquivalentTo(_emails);
            _membership.EmailList.Should().BeEquivalentTo(_emails);
        }

        [Fact, Priority(25)]
        public async Task Unsubscribe_All_Should_Work()
        {
            var resp = await _membership.UnsubscribeAllAsync();
            resp.NonMembers.Should().BeEmpty();
            resp.Unsubscribed.Should().BeEquivalentTo(_emails);
            _membership.EmailList.Should().BeEmpty();
        }


        [Theory, Priority(30)]
        [InlineData("^a", 66)]
        [InlineData("com", 193)]
        [InlineData("example.com", 189)]
        [InlineData("st", 52)]
        public async Task Search_Should_Return_Correct_Number(string search, int expected)
        {
            await EnsureMembers();
            var output = await _membership.SearchMembersAsync(search);
            output.Should().HaveCount(expected);
        }

        [Fact, Priority(40)]
        public async Task Moderate_All_Should_Work()
        {
            await EnsureMembers();
            await _membership.ModerateAllAsync();
            var output = await _membership.SearchMembersAsync($"^{RandomLetter}");
            output.Select(m => m.Mod).Should().AllBeEquivalentTo(true);
        }

        [Fact, Priority(50)]
        public async Task Unmoderate_All_Should_Work()
        {
            await EnsureMembers();
            await _membership.UnmoderateAllAsync();
            var output = await _membership.SearchMembersAsync($"^{RandomLetter}");
            output.Select(m => m.Mod).Should().AllBeEquivalentTo(false);
        }

        [Fact, Priority(60)]
        public async Task Get_All_Should_Work()
        {
            await EnsureMembers();
            var members = await _membership.GetAllMembersAsync();
            
            members.Should().HaveSameCount(_emails);
            members.Select(m => m.Email).Should().BeEquivalentTo(_emails);
        }

        [Fact, Priority(70)]
        public async Task Save_Should_Work()
        {
            var email = Guid.NewGuid().ToString() + "@example.com";
            var subscribeResponse = await _membership.SubscribeAsync(email);
            subscribeResponse.Subscribed.Should().ContainSingle(email);

            var found = await _membership.SearchMembersAsync(email);
            found.Should().ContainSingle();
            var member = found.Single();

            member.RealName = Guid.NewGuid().ToString();
            member.Ack = !member.Ack;
            member.NotMeToo = !member.NotMeToo;
            member.NoDupes = !member.NoDupes;
            member.Digest = !member.Digest;
            member.Plain = !member.Plain;
            member.Mod = !member.Mod;
            member.Hide = !member.Hide;
            member.NoMail = !member.NoMail;
            await _membership.SaveMemberAsync(member);

            var output = (await _membership.SearchMembersAsync(email)).Single();
            output.Should().BeEquivalentTo(member, opt => opt.Excluding(o => o.NoMailReason));
            output.NoMailReason.Should().Be(output.NoMail ? NoMailReason.Administrator : NoMailReason.None);
        }

        [Fact, Priority(80)]
        public async Task Change_Address_Should_Work()
        {
            var email1 = Guid.NewGuid().ToString() + "@example.com";
            var email2 = Guid.NewGuid().ToString() + "@example.com";
            await _membership.SubscribeAsync(email1);

            Func<Task> act = async () => await _membership.ChangeMemberAddressAsync(email1, email2);
            act.Should().NotThrow();
        }

        [Fact, Priority(80)]
        public async Task Change_Address_Bad_Address()
        {
            var email1 = Guid.NewGuid().ToString() + "@example.com";
            var email2 = Guid.NewGuid().ToString();
            await _membership.SubscribeAsync(email1);

            Func<Task> act = async () => await _membership.ChangeMemberAddressAsync(email1, email2);
            act.Should().Throw<MailmanException>().WithMessage($"{email2} is not a valid email address.");
        }

        [Fact, Priority(80)]
        public async Task Change_Address_Not_A_Member()
        {
            var email = Guid.NewGuid().ToString() + "@example.com";
            var guid = Guid.NewGuid().ToString();
            await _membership.SubscribeAsync(email);

            Func<Task> act = async () => await _membership.ChangeMemberAddressAsync(guid, "foo@example.com");
            act.Should().Throw<MailmanException>().WithMessage($"{guid} is not a member");
        }

        [Fact, Priority(90)]
        public async Task Subscribe_Successful()
        {
            await _membership.UnsubscribeAllAsync();
            var email = Guid.NewGuid().ToString() + "@example.com";
            var output = await _membership.SubscribeAsync(email);

            output.Subscribed.Should().ContainSingle(email);
            output.BadEmails.Should().BeEmpty();
            output.AlreadyMembers.Should().BeEmpty();
            _membership.Emails.Should().Be(email);
        }

        [Fact, Priority(90)]
        public async Task Subscribe_Already_Member()
        {
            var email = Guid.NewGuid().ToString() + "@example.com";
            await _membership.SubscribeAsync(email);
            var output = await _membership.SubscribeAsync(email);

            output.Subscribed.Should().BeEmpty();
            output.BadEmails.Should().BeEmpty();
            output.AlreadyMembers.Should().ContainSingle(email);
        }

        [Fact, Priority(90)]
        public async Task Subscribe_Bad_Email()
        {
            var guid = Guid.NewGuid().ToString();
            var output = await _membership.SubscribeAsync(guid);

            output.Subscribed.Should().BeEmpty();
            output.AlreadyMembers.Should().BeEmpty();
            output.BadEmails.Should().ContainSingle(guid);
        }

        [Fact, Priority(90)]
        public async Task Unsubscribe_Successful()
        {
            await _membership.UnsubscribeAllAsync();
            var email = Guid.NewGuid().ToString() + "@example.com";
            await _membership.SubscribeAsync(email);
            var output = await _membership.UnsubscribeAsync(email);

            output.Unsubscribed.Should().ContainSingle(email);
            output.NonMembers.Should().BeEmpty();
            _membership.Emails.Should().BeEmpty();
        }

        [Fact, Priority(90)]
        public async Task Unsubscribe_Non_Member()
        {
            var email = Guid.NewGuid().ToString() + "@example.com";
            var output = await _membership.UnsubscribeAsync(email);
            
            output.Unsubscribed.Should().BeEmpty();
            output.NonMembers.Should().ContainSingle(email);
        }

        [Fact, Priority(90)]
        public async Task Unsubscribe_Enumerable()
        {
            await _membership.UnsubscribeAllAsync();

            var email1 = Guid.NewGuid().ToString() + "@example.com";
            var email2 = Guid.NewGuid().ToString() + "@example.com";
            await _membership.SubscribeAsync(new[] { email1, email2 });

            var members = await _membership.SearchMembersAsync("example");
            var output = await _membership.UnsubscribeAsync(members);

            output.Unsubscribed.Should().HaveCount(2);
            output.Unsubscribed.Should().Contain(email1).And.Contain(email2);
            output.NonMembers.Should().BeEmpty();
            _membership.Emails.Should().BeEmpty();
        }
    }
}
