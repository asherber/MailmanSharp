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
        private class CustomHtmlDocument: HtmlDocument
        {
            public CustomHtmlDocument()
            {
                OptionFixNestedTags = true;
            }
        }

        [Fact]
        public void SafeSelectNodes_ValidPath_Should_Work()
        {
            var doc = new CustomHtmlDocument();
            doc.Load("html/general.html");

            var safeNodes = doc.DocumentNode.SafeSelectNodes("//td");
            var nodes = doc.DocumentNode.SelectNodes("//td");
            safeNodes.Should().BeEquivalentTo(nodes);
        }

        [Fact]
        public void SafeSelectNodes_BadPath_Should_Return_Empty()
        {
            var doc = new CustomHtmlDocument();
            doc.Load("html/general.html");

            var safeNodes = doc.DocumentNode.SafeSelectNodes("//tdxxx");
            safeNodes.Should().NotBeNull().And.BeEmpty();
        }

        [Theory]
        [InlineData("<html><body><h1>Hello!</h1></body></html>")]
        [InlineData("<html><BODY><H1>Hello!</H1></BODY></html>")]
        [InlineData(null)]
        [InlineData("   ")]
        public void GetHtmlDocument_CorrectContent(string input)
        {
            var output = input.GetHtmlDocument();

            output.Should().BeOfType<HtmlDocument>();
            output.Should().NotBeNull();
            output.ParsedText.Should().BeEquivalentTo(input);
        }


        private string _nodeTestHtml = 
            @"<html>
            <body>
            <input name=""string_prop"" value=""apple"">
            <input name=""int_prop"" value=""42"">
            <input name=""double_prop"" value=""8.7"">
            <textarea name=""list_prop"" >Line One
Line Two</textarea>
            <input name=""bool_prop"" checked value=""1"">
            <input name=""enum_prop"" checked value=""2"">
            <input name=""bad_enum_prop"" checked value=""90"">
            </body>
            </html>";

        private class NodeTestClass
        {
            public string StringProp { get; set; } = "apple";
            public ushort IntProp { get; set; } = 42;
            public double DoubleProp { get; set; } = 8.7;
            public List<string> ListProp { get; set; } = new List<string>() { "Line One", "Line Two" };
            public DigestVolumeFrequencyOption EnumProp { get; set; } = DigestVolumeFrequencyOption.Quarterly;
            public bool BoolProp { get; set; } = true;
        }

        [Theory]
        [InlineData("string_prop", "apple")]
        [InlineData("int_prop", "42")]
        [InlineData("double_prop", "8.7")]
        [InlineData("enum_prop", "2")]
        public void GetNodeValue_ByName(string name, object expected)
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);

            var output = doc.GetNodeValue(name);
            output.Should().Be(expected);
        }

        [Theory]
        [InlineData("StringProp", "apple")]
        [InlineData("IntProp", "42")]
        [InlineData("DoubleProp", "8.7")]
        [InlineData("EnumProp", "2")]
        public void GetNodeValue_ByProp(string name, object expected)
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);

            var prop = typeof(NodeTestClass).GetProperty(name);

            var output = doc.GetNodeValue(prop);
            output.Should().Be(expected);
        }

        [Fact]
        public void GetNodeStringValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("StringProp");

            var output = doc.GetNodeStringValue(prop);
            output.Should().Be(obj.StringProp);
        }

        [Fact]
        public void GetNodeIntValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("IntProp");

            var output = doc.GetNodeIntValue(prop);
            output.Should().Be(obj.IntProp);
        }

        [Fact]
        public void GetNodeDoubleValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("DoubleProp");

            var output = doc.GetNodeDoubleValue(prop);
            output.Should().Be(obj.DoubleProp);
        }

        [Fact]
        public void GetNodeBoolValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("BoolProp");

            var output = doc.GetNodeBoolValue(prop);
            output.Should().Be(obj.BoolProp);
        }

        [Fact]
        public void GetNodeListValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("ListProp");

            var output = doc.GetNodeListValue(prop);
            output.Should().BeEquivalentTo(obj.ListProp);
        }

        [Fact]
        public void GetNodeListValue_ByName()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            
            var output = doc.GetNodeListValue("list_prop");
            output.Should().BeEquivalentTo(obj.ListProp);
        }

        [Fact]
        public void GetNodeEnumValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("EnumProp");

            var output = doc.GetNodeEnumValue(prop);
            output.Should().Be(obj.EnumProp);
        }

        [Fact]
        public void GetNodeEnumValue_ByName()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();

            var output = doc.GetNodeEnumValue<DigestVolumeFrequencyOption>("enum_prop");
            output.Should().Be(obj.EnumProp);
        }

        [Fact]
        public void GetNodeEnumValue_WrongType_ArgumentException()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            
            Action act = () => doc.GetNodeEnumValue<int>("enum_prop");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetNodeEnumValue_WrongValue_ArgumentException()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);

            Action act = () => doc.GetNodeEnumValue<DigestVolumeFrequencyOption>("bad_enum_prop");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetNodeEnumValue_WrongName_OutOfRangeException()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);

            Action act = () => doc.GetNodeEnumValue<DigestVolumeFrequencyOption>("xyz");
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
