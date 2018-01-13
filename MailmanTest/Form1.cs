using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MailmanSharp;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;


namespace MailmanTest
{
    public partial class Form1: Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var mm = new MailmanList("http://lists.sherber.com/admin.cgi/test-sherber.com", "");
            mm.Membership.ReadAsync();
            
            //mm.Membership.Unsubscribe("aaron@sherber.com");
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var mm = new MailmanList("http://lists.sherber.com/admin.cgi/test-sherber.com", "");
            mm.ReadAsync();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var mm = new MailmanList("http://lists.mountwashingtonschool.org/admin.cgi/announce-mountwashingtonschool.org", "");
            //var mm = new MailmanList("http://lists.sherber.com/admin.cgi/test-sherber.com", "");

            var members = await mm.Membership.GetMembersAsync();
            //var foo = mm.Membership.GetType().GetCustomAttributes(false);
            
        }
        private void button4_Click(object sender, EventArgs e)
        {
            
        }
    }

    
}
