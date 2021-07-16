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
        Logic logic;
        public Form1()
        {
            InitializeComponent();
            this.logic = new Logic(this);
            Application.DoEvents();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            try
            {
                Logic.StartMainProgram();
            }
            catch (Exception ex)
            {
                string message = "No executable was found!\nIs this your first time running the launcher without and internet connection?";
                string caption = "ERROR Executable not found!";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult Result = MessageBox.Show(message, caption, buttons);

                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            Environment.Exit(0);
        }
    }
}
