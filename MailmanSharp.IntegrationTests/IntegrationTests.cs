using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MailmanSharp.IntegrationTests
{
    public class IntegrationTests
    {
        [Fact]
        public async void Foo()
        {
            var list = new MailmanList(ListConfig.Url, ListConfig.Password);
            var resp = await list.Membership.ChangeMemberAddressAsync("aaron+test@sherber.com", "aaron@sherber.com");
        }
    }
}
