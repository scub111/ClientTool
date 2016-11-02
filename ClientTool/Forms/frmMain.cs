using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ClientTool
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();

            LocalIPAddress = "none";
            String strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            string ip;
            Regex ipReg = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            for (int i = 0; i < addr.Length; i++)
            {
                ip = addr[i].ToString();
                if (ipReg.IsMatch(ip))
                {
                    LocalIPAddress = ip;
                    break;
                }
            }
        }

        /// <summary>
        /// Локальный IP-адрес.
        /// </summary>
        string LocalIPAddress;

        Thread background { get; set; }

        private void btnSetInfoWallper_Click(object sender, EventArgs e)
        {
            //background = new Thread(SetInfoWallper);

            //background.Start();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Text += string.Format(" [{0}]", LocalIPAddress);
        }
    }
}
