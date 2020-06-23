using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MW5LOMLauncherV2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Logic logic = new Logic(this);
            Application.DoEvents();
            this.timer1.Enabled = true;
            this.timer1.Start();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Logic.StartMainProgram();
            Environment.Exit(0);
        }
    }
}
