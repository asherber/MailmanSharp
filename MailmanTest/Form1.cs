using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MailmanSharp;

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
            mm.ServerUrl = "http://lists.sherber.com";
            mm.ListName = "test-sherber.com";
            mm.Password = "***REMOVED***";
            //mm.Login();
            mm.Read();

        }
    }
}
