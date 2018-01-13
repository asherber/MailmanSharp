using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using RestSharp;

namespace MailmanSharp.Tests
{
    public class RestSharpExtensionsTests
    {
        [Fact]
        public void AddOrSetParameter_NotExisting_Should_Add()
        {
            var req = new RestRequest();
            req.AddOrSetParameter("foo", "bar");

            req.Parameters.Should().HaveCount(1);
            req.Parameters.Single().Value.Should().Be("bar");
        }
        [Fact]
        public void AddOrSetParameter_Existing_Should_Set()
        {
            var req = new RestRequest();
            req.AddParameter("foo", "bar");
            req.AddOrSetParameter("foo", "baz");

            req.Parameters.Should().HaveCount(1);
            req.Parameters.Single().Value.Should().Be("baz");
        }


    }
}
