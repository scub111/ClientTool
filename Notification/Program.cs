using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Notification
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            frmMain frmMain = new frmMain();
            frmMain.TopMost = true;
            Application.Run(frmMain);
        }
    }
}
