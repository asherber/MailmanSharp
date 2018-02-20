using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using RestSharp;
using System.Net;

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

        public static IEnumerable<object[]> HttpStatuses = new []
        {
            new object[] { HttpStatusCode.OK, true },
            new object[] { HttpStatusCode.Accepted, true },
            new object[] { HttpStatusCode.Created, true },
            new object[] { HttpStatusCode.Unauthorized, false },
            new object[] { HttpStatusCode.BadRequest, false },
            new object[] { HttpStatusCode.InternalServerError, false },
            new object[] { HttpStatusCode.MovedPermanently, false },
            new object[] { HttpStatusCode.BadGateway, false },
        };

        [Theory]
        [MemberData(nameof(HttpStatuses))]
        public void IsSuccessStatusCode_Should_Work(HttpStatusCode statusCode, bool expected)
        {
            var output = statusCode.IsSuccessStatusCode();
            output.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(HttpStatuses))]
        public void EnsureSuccessStatusCode_Should_Work(HttpStatusCode statusCode, bool success)
        {
            var response = new RestResponse()
            {
                StatusCode = statusCode
            };

            Action act = () => response.EnsureSuccessStatusCode();
            if (success)
                act.Should().NotThrow();
            else
                act.Should().Throw<MailmanHttpException>();
        }

        [Theory]
        [MemberData(nameof(HttpStatuses))]
        public void CheckAndThrow_Should_Work(HttpStatusCode statusCode, bool success)
        {
            var response = new RestResponse()
            {
                StatusCode = statusCode
            };

            Action act = () => response.CheckResponseAndThrowIfNeeded();
            if (success)
                act.Should().NotThrow();
            else
                act.Should().Throw<MailmanHttpException>();
        }

        [Fact]
        public void CheckAndThrow_Should_Raise_ErrorException()
        {
            var response = new RestResponse()
            {
                StatusCode = HttpStatusCode.OK,
                ErrorException = new Exception("foobar")
            };

            Action act = () => response.CheckResponseAndThrowIfNeeded();
            act.Should().Throw<Exception>().WithMessage("foobar");
        }
    }
}
