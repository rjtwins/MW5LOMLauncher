using PostSharp.Community.Packer;
using System;
using Application = System.Windows.Forms.Application;

[assembly: Packer]
namespace MW5LOMLauncherV2
{
    internal static class Program
    {
        public static OutputForm Form1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Program.Form1 = new OutputForm();
            Application.Run(Program.Form1);
        }
    }
}