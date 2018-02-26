using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json;

namespace MailmanSharp.Tests
{
    public class LoadConfigTests
    {
        [Theory]
        [InlineData("asd")]
        [InlineData("\"Foo\": 34")]
        public void MalformedJson_To_List_Should_Throw(string input)
        {
            var list = new MailmanList();
            Action act = () => list.LoadConfig(input);
            act.Should().Throw<JsonException>();
        }
      
        [Fact]
        public void Wrong_Section_Should_Throw()
        {
            var list = new MailmanList();
            var json = "{ \"WrongSection\": { \"Description\": \"foobar\" } }";
            Action act = () => list.General.LoadConfig(json);
            act.Should().Throw<JsonException>().WithMessage("Incorrect root property name.");
        }

        [Fact]
        public void General_Should_Ignore_RealName()
        {
            var list = new MailmanList();
            var guid = Guid.NewGuid().ToString();
            list.General.RealName = guid;
            var json = "{ \"General\": { \"Description\": \"foobar\", \"RealName\": \"sdkjfnjdknf\" } }";

            list.General.LoadConfig(json);
            list.General.Description.Should().Be("foobar");
            list.General.RealName.Should().Be(guid);
        }
    }
}
