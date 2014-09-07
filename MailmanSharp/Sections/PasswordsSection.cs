using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    [Path("passwords")]
    [Order(2)]
    public class PasswordsSection: SectionBase
    {
        public string Administrator { get; set; }
        public string Moderator { get; set; }
        public string Poster { get; set; }

        public PasswordsSection(MailmanList list) : base(list) { }

        public override void Read()
        {
            // Nothing to read
        }

        public override void Write()
        {
            var req = new RestRequest();
            SetParams("", this.Administrator, req);
            SetParams("mod", this.Moderator, req);
            SetParams("post", this.Poster, req);

            this.Client.ExecuteAdminRequest(_paths.Single(), req);
        }

        internal override string GetConfig()
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
