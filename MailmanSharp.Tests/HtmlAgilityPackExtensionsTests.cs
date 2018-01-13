using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using HtmlAgilityPack;

namespace MailmanSharp.Tests
{
    public class HtmlAgilityPackExtensionsTests
    {
        [Fact]
        public void SafeSelectNodes_ValidPath_Should_Work()
        {
            var doc = new HtmlDocument()
            {
                OptionFixNestedTags = true
            };
            doc.Load("html/general.html");

            var safeNodes = doc.DocumentNode.SafeSelectNodes("//td");
            var nodes = doc.DocumentNode.SelectNodes("//td");
            safeNodes.Should().BeEquivalentTo(nodes);
        }

        [Fact]
        public void SafeSelectNodes_BadPath_Should_Return_Empty()
        {
            var doc = new HtmlDocument()
            {
                OptionFixNestedTags = true
            };
            doc.Load("html/general.html");

            var safeNodes = doc.DocumentNode.SafeSelectNodes("//tdxxx");
            safeNodes.Should().NotBeNull().And.BeEmpty();
        }
    }
}
