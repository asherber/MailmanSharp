using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using MailmanSharp;
using HtmlAgilityPack;
using System.Xml.Linq;
using System.Xml;
using RestSharp;

namespace MailmanSharp.Tests
{
    public class MiscellaneousExtensionsTests
    {
        public static IEnumerable<object[]> CamelData =>
            new List<object[]>()
            {
                new object[] { "SomeFieldName", "some_field_name" },
                new object[] { "Single", "single" },
                new object[] { "", "" }
            };
        

        [Theory]
        [MemberData(nameof(CamelData))]
        public void Decamel_Should_Work(string input, string expected)
        {
            var output = input.Decamel();
            output.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(CamelData))]
        public void Encamel_Should_Work(string expected, string input)
        {
            var output = input.Encamel();
            output.Should().Be(expected);
        }

        

        [Fact]
        public void CheckElementName_MatchingName_Should_Work()
        {
            string name = "foo";
            var el = new XElement(name);

            el.CheckElementName(name);
        }

        [Fact]
        public void CheckElementName_NoMatch_Should_Throw()
        {
            var el = new XElement("foo");

            Action act = () => el.CheckElementName("Bar");
            act.Should().Throw<XmlException>();
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public void ToInt_Should_Work(bool input, int expected)
        {
            var output = input.ToInt();
            output.Should().Be(expected);
        }

        [Theory]
        [InlineData(typeof(NoUnignored), 0)]
        [InlineData(typeof(OneUnignored), 1)]
        [InlineData(typeof(TwoUnignored), 2)]
        public void GetUnignoredProperties_Should_Work(Type type, int expectedCount)
        {
            var output = type.GetProperties().GetUnignored();
            output.Should().HaveCount(expectedCount);
        }
    
        private class NoUnignored
        {
            [Ignore]
            public int Foo { get; set; }
        }

        private class OneUnignored
        {
            [Ignore]
            public string Foo { get; set; }
            public int Bar { get; set; }
        }

        private class TwoUnignored
        {
            [Ignore]
            public int Foor { get; set; }
            public int Bar { get; set; }
            public int Baz { get; set; }
        }

        [Theory]
        [InlineData(new [] { "foo" }, "foo")]
        [InlineData(new[] { "foo", "bar" }, "foo\nbar")]
        [InlineData(new[] { "foo", "bar", "baz" }, "foo\nbar\nbaz")]
        public void Cat_Should_Work(IEnumerable<string> input, string expected)
        {
            var output = input.Cat();
            output.Should().Be(expected);
        }
    }
}
