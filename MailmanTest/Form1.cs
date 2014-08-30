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
            var mm = new MailmanList();
            mm.BaseUrl = "http://lists.sherber.com";
            mm.ListName = "test-sherber.com";
            mm.Password = "***REMOVED***";
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
            mm.BaseUrl = "http://lists.sherber.com";
            mm.ListName = "test-sherber.com";
            mm.Password = "***REMOVED***";
            mm.Read();
            var foo = mm.Serialize();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var mm = new MailmanList();
            mm.BaseUrl = "http://lists.sherber.com";
            mm.ListName = "test-sherber.com";
            mm.Password = "***REMOVED***";
            mm.GetCurrentSubscribers();
        }
    }

    
}
