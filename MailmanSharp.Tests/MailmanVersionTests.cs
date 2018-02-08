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
    public class MailmanVersionTests
    {
        [Fact]
        public void Null_Input_Should_Throw()
        {
            Action act = () => new MailmanVersion(null);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("1.2.3", 1, 2, 3, "")]
        [InlineData("13.5.7 beta1", 13, 5, 7, "beta1")]
        [InlineData("2.1.18-1", 2, 1, 18, "-1")]
        [InlineData("foobar", 0, 0, 0, "")]
        public void Valid_Inputs_Should_Parse(string input, int major, int minor, int build, string patch)
        {
            var v = new MailmanVersion(input);

            v.Major.Should().Be(major);
            v.Minor.Should().Be(minor);
            v.Build.Should().Be(build);
            v.Patch.Should().Be(patch);
        }

        [Fact]
        public void Equals_Should_Work()
        {
            var v1 = new MailmanVersion("7.8.9");
            var v2 = new MailmanVersion("7.8.9");

            v1.Should().Equals(v2);
        }

        [Fact]
        public void opEquals_Should_Work()
        {
            var v1 = new MailmanVersion("7.8.9");
            var v2 = new MailmanVersion("7.8.9");

            var equals = v1 == v2;
            equals.Should().BeTrue();
        }

        [Fact]
        public void opEquals_Should_Be_False_If_One_Is_Null()
        {
            var v1 = new MailmanVersion("7.8.9");
            MailmanVersion v2 = null;

            var equals = v1 == v2;
            equals.Should().BeFalse();
        }

        [Fact]
        public void opNotEquals_Should_Work()
        {
            var v1 = new MailmanVersion("7.8.9");
            var v2 = new MailmanVersion("4.8.9");

            var equals = v1 == v2;
            equals.Should().BeFalse();
        }

        [Theory]
        [InlineData("1.2.3", "2.2.3", -1)]
        [InlineData("5.7.7", "5.6.7", 1)]
        [InlineData("6.5.4", "6.5.4", 0)]
        [InlineData("3.3.4", "3.3.5", -1)]
        [InlineData("2.2.2 beta1", "2.2.2 beta2", -1)]
        public void CompareTo_Should_Work(string v1, string v2, int expected)
        {
            var output = new MailmanVersion(v1).CompareTo(new MailmanVersion(v2));
            output.Should().Be(expected);
        }

        [Fact]
        public void opGreaterThan_Should__Handle_Null()
        {
            MailmanVersion v1 = null;
            MailmanVersion v2 = new MailmanVersion();

            var output = v1 > v2;
            output.Should().BeFalse();
        }

        [Fact]
        public void opLessThan_Should__Handle_Null()
        {
            MailmanVersion v1 = null;
            MailmanVersion v2 = new MailmanVersion();

            var output = v1 < v2;
            output.Should().BeTrue();
        }

        [Theory]
        [InlineData("1.2.4", "1.2.3")]
        [InlineData("1.2.3", "1.2.3")]
        public void opGTE_Should_Work(string v1, string v2)
        {
            var output = new MailmanVersion(v1) >= new MailmanVersion(v2);
            output.Should().BeTrue();
        }
    }
}
