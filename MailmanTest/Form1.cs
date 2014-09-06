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
using MailmanSharp.Sections;

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
            var mm = new MailmanList();
            mm.BaseAdminUrl = "http://lists.sherber.com/admin.cgi";
            mm.ListName = "test-sherber.com";
            mm.AdminPassword = "***REMOVED***";
            //mm.Login();
            var st = Stopwatch.StartNew();
            mm.Read();
            st.Stop();
            MessageBox.Show(st.Elapsed.ToString());
            //mm.Write();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var mm = new MailmanList();
            mm.BaseAdminUrl = "http://lists.sherber.com/admin.cgi/";
            mm.ListName = "test-sherber.com";
            mm.AdminPassword = "***REMOVED***";

            mm.Membership.Unsubscribe("bob@dole.com", "djfnksfn");
            //mm.Membership.Subscribe("bob@dole.com", "aaron@sherber.com", "jsdfn");
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            /*
            var list = new MailmanList()
            {
                ListName = "calhoun-parents-mountwashingtonschool.org",
                Password = "***REMOVED***",
                BaseUrl = "http://lists.mountwashingtonschool.org/admin.cgi"
            };  //*/

            //*
            var list = new MailmanList()
            {
                ListName = "test-sherber.com",
                AdminPassword = "***REMOVED***",
                BaseAdminUrl = "http://lists.sherber.com"
            };  //*/

            list.Membership.Unsubscribe("jeremy@sherber.com", "bob@example.com", "jill@example.com");


        }
    }

    
}
