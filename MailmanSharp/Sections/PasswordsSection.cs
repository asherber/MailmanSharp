/**
 * Copyright 2014-2018 Aaron Sherber
 * 
 * This file is part of MailmanSharp.
 *
 * MailmanSharp is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * MailmanSharp is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with MailmanSharp. If not, see <http://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    [Path("passwords")]
    [Order(2)]
    public class PasswordsSection: SectionBase
    {
        public string Administrator { get; set; }
        public string Moderator { get; set; }
        public string Poster { get; set; }

        internal PasswordsSection(MailmanList list) : base(list) { }

        public override Task ReadAsync()
        {
            ResetProperties(_props);
            // Nothing to read
            return Task.CompletedTask;
        }

        public override Task WriteAsync()
        {
            var req = new RestRequest(Method.POST);
            SetParams("", this.Administrator, req);
            SetParams("mod", this.Moderator, req);
            SetParams("post", this.Poster, req);

            return this.GetClient().ExecuteAdminRequestAsync(_paths.Single(), req);
        }

        internal override JProperty GetCurrentConfigJProperty()
        {
            return null;
        }


        private void SetParams(string tag, string property, RestRequest req)
        {
            if (!String.IsNullOrWhiteSpace(property))
            {
                req.AddParameter(String.Format("new{0}pw", tag), property);
                req.AddParameter(String.Format("confirm{0}pw", tag), property);
            }
        }
    }
}
