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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
        public void CheckObjectName_MatchingName_Should_Work()
        {
            string name = "foo";
            var obj = new JObject(new JProperty(name, 123));
            
            obj.CheckObjectName(name);
        }

        [Fact]
        public void CheckElementName_NoMatch_Should_Throw()
        {
            var obj = new JObject(new JProperty("foo", 123));

            Action act = () => obj.CheckObjectName("Bar");
            act.Should().Throw<JsonException>();
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
            var output = type.GetProperties().Unignored();
            output.Should().HaveCount(expectedCount);
        }
    
        private class NoUnignored
        {
            [MailmanIgnoreAttribute]
            public int Foo { get; set; }
        }

        private class OneUnignored
        {
            [MailmanIgnoreAttribute]
            public string Foo { get; set; }
            public int Bar { get; set; }
        }

        private class TwoUnignored
        {
            [MailmanIgnoreAttribute]
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

        [Theory]
        [InlineData("apple", 1)]
        [InlineData("banana", 2)]
        [InlineData("carrot", 0)]
        public void Props_GetForPath(string path, int expectedCount)
        {
            var props = typeof(PathClass).GetProperties();
            var propsForPath = props.ForPath(path);
            propsForPath.Should().HaveCount(expectedCount);
        }

        private class PathClass
        {
            [Path("banana")]
            public int B { get; set; }
            [Path("apple")]
            public int A { get; set; }
            [Path("banana")]
            public int C { get; set; }            
        }

        private class GetSimpleValueClass
        {
            public bool? BoolProp { get; set; } = true;
            public List<string> ListProp { get; set; } = new List<string>() { "apple", "banana" };
            public string StringProp { get; set; } = "string";
            public ushort? UshortProp { get; set; } = 42;
            public double? DoubleProp { get; set; } = 4.5;
            public SubscribePolicyOption? SubscribePolicy { get; set; } = SubscribePolicyOption.RequireApproval;
        }

        [Theory]
        [InlineData("BoolProp", typeof(Int32), 1)]
        [InlineData("ListProp", typeof(string), "apple\nbanana")]
        [InlineData("StringProp", typeof(string), "string")]
        [InlineData("UshortProp", typeof(ushort), (ushort)42)]
        [InlineData("DoubleProp", typeof(double), 4.5)]
        public void Prop_GetSimpleValue(string propName, Type expectedType, object expectedValue)
        {
            var prop = typeof(GetSimpleValueClass).GetProperty(propName);
            var output = prop.GetSimpleValue(new GetSimpleValueClass());

            output.GetType().Should().Be(expectedType);
            output.Should().Be(expectedValue);
        }

        private class EnumValuesClass
        {
            public SubscribePolicyOption? SubscribePolicy { get; set; } = SubscribePolicyOption.RequireApproval;
            public ChangeNotificationOptions? ChangeNotification { get; set; } = ChangeNotificationOptions.SendToNewAddress | ChangeNotificationOptions.SendToOldAddress;
            public SubscribePolicyOption? SubscribePolicyNull { get; set; } = null;
            public ChangeNotificationOptions? ChangeNotificationNull { get; set; } = null;
        }

        [Theory]
        [InlineData("SubscribePolicy", new object[] { SubscribePolicyOption.RequireApproval })]
        [InlineData("ChangeNotification", new object[] { "sendtonewaddress", "sendtooldaddress"})]
        [InlineData("SubscribePolicyNull", new object[0])]
        [InlineData("ChangeNotificationNull", new object[0])]
        public void Prop_GetEnumValues(string propName, object[] expectedValue)
        {
            var prop = typeof(EnumValuesClass).GetProperty(propName);
            var output = prop.GetEnumValues(new EnumValuesClass());

            output.Should().BeEquivalentTo(expectedValue);        
        }


        private class Foo
        {
            public string Name { get; set; } = "John";
        }

    }
}
