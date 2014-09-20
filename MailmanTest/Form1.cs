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

        private void button1_Click(object sender, EventArgs e)
        {
            var mm = new MailmanList("http://lists.sherber.com/admin.cgi/test-sherber.com", "***REMOVED***");
            //mm.Login();
            var st = Stopwatch.StartNew();
            mm.Read();
            st.Stop();
            MessageBox.Show(st.Elapsed.ToString());
            //mm.Write();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var mm = new MailmanList("http://lists.sherber.com/admin.cgi/test-sherber.com", "***REMOVED***");
            mm.Membership.Read();
            mm.Membership.Read();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var mm = new MailmanList("http://lists.mountwashingtonschool.org/admin.cgi/announce-mountwashingtonschool.org", "***REMOVED***");
            //var mm = new MailmanList("http://lists.sherber.com/admin.cgi/test-sherber.com", "***REMOVED***");

            var st = Stopwatch.StartNew();
            mm.Membership.GetMembers("^a.*$");
            //mm.Membership.Read();
            //var members = mm.Membership.GetMembers();
            st.Stop();
            MessageBox.Show(st.Elapsed.ToString());
            //var foo = mm.Membership.GetMembers();
            //var nomail = foo.Where(m => m.NoMail);
            
        }
        private void button4_Click(object sender, EventArgs e)
        {
            
        }
    }

    
}
