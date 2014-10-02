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
            var mm = new MailmanList("http://lists.sherber.com/admin.cgi/test-sherber.com", "***REMOVED***");
            //mm.Login();
            var st = Stopwatch.StartNew();
            await mm.ReadAsync();
            st.Stop();
            MessageBox.Show(st.Elapsed.ToString());
            //mm.Write();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var mm = new MailmanList("http://lists.sherber.com/admin.cgi/test-sherber.com", "***REMOVED***");
            await mm.ReadAsync();
            var xml = mm.CurrentConfig;
            mm.LoadConfig(xml);
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var mm = new MailmanList("http://lists.mountwashingtonschool.org/admin.cgi/announce-mountwashingtonschool.org", "***REMOVED***");
            //var mm = new MailmanList("http://lists.sherber.com/admin.cgi/test-sherber.com", "***REMOVED***");

            var members = await mm.Membership.GetMembersAsync();
            //var foo = mm.Membership.GetType().GetCustomAttributes(false);
            
        }
        private void button4_Click(object sender, EventArgs e)
        {
            
        }
    }

    
}
