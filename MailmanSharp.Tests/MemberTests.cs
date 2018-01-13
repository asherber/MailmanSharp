using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using HtmlAgilityPack;
using RestSharp;
using MailmanSharp.Extensions;

namespace MailmanSharp.Tests
{
    public class MemberTests
    {
        private HtmlDocument _doc;

        public MemberTests()
        {
            _doc = new HtmlDocument()
            {
                OptionFixNestedTags = true
            };
            _doc.Load("html/members.html");
        }

        [Fact]
        public void NewMember_ShouldRead_FromPage()
        {
            var members = GetMembersFromHtml();
            members.Should().HaveCount(2);
            members.Select(m => m.Email).Should().BeEquivalentTo(new[] { "aaron@sherber.com", "aaron.sherber@sterlingts.com" });
            VerifyMember(members.Single(r => r.Email == "aaron@sherber.com"));
            VerifyMember(members.Single(r => r.Email == "aaron.sherber@sterlingts.com"));
        }

        [Fact] 
        public void ToParameters_Should_Work()
        {            
            var members = GetMembersFromHtml();
            var member = members.Single(r => r.Email == "aaron@sherber.com");
            var request = new RestRequest();
            var encName = "aaron%40sherber.com";
            request.AddParameter("user", "aaron%40sherber.com")
                .AddParameter($"{encName}_realname", "")
                .AddParameter($"{encName}_hide", 1)
                .AddParameter($"{encName}_notmetoo", 1)
                .AddParameter($"{encName}_plain", 1)
                .AddParameter($"{encName}_language", "en");
            var expected = request.Parameters;

            var output = member.ToParameters().ToList();
            output.Should().Equal(expected, (p1, p2) => p1.Name == p2.Name && p1.Value.ToString() == p2.Value.ToString());
        }

        private IEnumerable<Member> GetMembersFromHtml()
        {
            var result = new List<Member>();
            var emails = new[] { "aaron%40sherber.com", "aaron.sherber%40sterlingts.com" };
            foreach (var email in emails)
            {
                var xpath = String.Format("//input[contains(@name, '{0}')]", email);
                var nodes = _doc.DocumentNode.SafeSelectNodes(xpath);
                if (nodes.Any())
                    result.Add(new Member(nodes));
            }

            return result;
        }

        private void VerifyMember(Member member)
        {
            switch (member.Email)
            {
                case "aaron@sherber.com":
                    member.RealName.Should().Be("");
                    member.Mod.Should().BeFalse();
                    member.Hide.Should().BeTrue();
                    member.NoMail.Should().BeFalse();
                    member.NoMailReason.Should().Be(NoMailReason.None);
                    member.Ack.Should().BeFalse();
                    member.NotMeToo.Should().BeTrue();
                    member.NoDupes.Should().BeFalse();
                    member.Digest.Should().BeFalse();
                    member.Plain.Should().BeTrue();
                    break;
                case "aaron.sherber@sterlingts.com":
                    member.RealName.Should().Be("");
                    member.Mod.Should().BeTrue();
                    member.Hide.Should().BeFalse();
                    member.NoMail.Should().BeTrue();
                    member.NoMailReason.Should().Be(NoMailReason.Administrator);
                    member.Ack.Should().BeTrue();
                    member.NotMeToo.Should().BeFalse();
                    member.NoDupes.Should().BeTrue();
                    member.Digest.Should().BeFalse();
                    member.Plain.Should().BeTrue();
                    break;
                default:
                    break;
            }
        }
       
    }
}
