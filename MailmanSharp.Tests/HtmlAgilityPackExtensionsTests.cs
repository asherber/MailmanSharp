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
            doc.Load("general.html");

            var safeNodes = doc.DocumentNode.SafeSelectNodes("//td");
            var nodes = doc.DocumentNode.SelectNodes("//td");
            safeNodes.Should().BeEquivalentTo(nodes);
        }

        [Fact]
        public void SafeSelectNodes_BadPath_Should_Return_Empty()
        {
            var doc = new CustomHtmlDocument();
            doc.Load("general.html");

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
            public string StringProp => "apple";
            public ushort IntProp => 42;
            public double DoubleProp => 8.7;
            public List<string> ListProp => new List<string>() { "Line One", "Line Two" };
            public string StringProp2 => "Line One\r\nLine Two";
            public DigestVolumeFrequencyOption EnumProp => DigestVolumeFrequencyOption.Quarterly;
            public bool BoolProp => true;
        }

        [Theory]
        [InlineData("string_prop", "apple")]
        [InlineData("int_prop", "42")]
        [InlineData("double_prop", "8.7")]
        [InlineData("enum_prop", "2")]
        public void GetInputValue_ByName(string name, object expected)
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);

            var output = doc.GetInputValue(name);
            output.Should().Be(expected);
        }

        [Theory]
        [InlineData("StringProp", "apple")]
        [InlineData("IntProp", "42")]
        [InlineData("DoubleProp", "8.7")]
        [InlineData("EnumProp", "2")]
        public void GetInputValue_ByProp(string name, object expected)
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);

            var prop = typeof(NodeTestClass).GetProperty(name);

            var output = doc.GetInputValue(prop);
            output.Should().Be(expected);
        }

        [Fact]
        public void GetInputStringValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("StringProp");

            var output = doc.GetInputStringValue(prop);
            output.Should().Be(obj.StringProp);
        }

        [Fact]
        public void GetInputIntValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("IntProp");

            var output = doc.GetInputIntValue(prop);
            output.Should().Be(obj.IntProp);
        }

        [Fact]
        public void GetInputDoubleValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("DoubleProp");

            var output = doc.GetInputDoubleValue(prop);
            output.Should().Be(obj.DoubleProp);
        }

        [Fact]
        public void GetInputBoolValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("BoolProp");

            var output = doc.GetInputBoolValue(prop);
            output.Should().Be(obj.BoolProp);
        }

        [Fact]
        public void GetTextareaListValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("ListProp");

            var output = doc.GetTextAreaListValue(prop);
            output.Should().BeEquivalentTo(obj.ListProp);
        }

        [Fact]
        public void GetTextareaListValue_ByName()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            
            var output = doc.GetTextAreaListValue("list_prop");
            output.Should().BeEquivalentTo(obj.ListProp);
        }

        [Fact]
        public void GetTextAreaStringValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("ListProp");

            var output = doc.GetTextAreaStringValue(prop);
            output.Should().Be(obj.StringProp2);
        }

        [Fact]
        public void GetTextareaStringValue_ByName()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();

            var output = doc.GetTextAreaStringValue("list_prop");
            output.Should().Be(obj.StringProp2);
        }

        [Fact]
        public void GetInputEnumValue()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();
            var prop = typeof(NodeTestClass).GetProperty("EnumProp");

            var output = doc.GetInputEnumValue(prop);
            output.Should().Be(obj.EnumProp);
        }

        [Fact]
        public void GetInputEnumValue_ByName()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            var obj = new NodeTestClass();

            var output = doc.GetInputEnumValue<DigestVolumeFrequencyOption>("enum_prop");
            output.Should().Be(obj.EnumProp);
        }

        [Fact]
        public void GetInputEnumValue_WrongType_ArgumentException()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);
            
            Action act = () => doc.GetInputEnumValue<int>("enum_prop");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetInputEnumValue_WrongValue_ArgumentException()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);

            Action act = () => doc.GetInputEnumValue<DigestVolumeFrequencyOption>("bad_enum_prop");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetInputEnumValue_WrongName_OutOfRangeException()
        {
            var doc = new CustomHtmlDocument();
            doc.LoadHtml(_nodeTestHtml);

            Action act = () => doc.GetInputEnumValue<DigestVolumeFrequencyOption>("xyz");
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
