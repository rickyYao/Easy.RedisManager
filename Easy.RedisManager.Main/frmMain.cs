﻿using Easy.Common.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Easy.RedisManager.Main
{
    public partial class frmMain : Form
    {
        /// <summary>
        /// 日志记录
        /// </summary>
        private ILogger m_Logger = null;

        public frmMain()
        {
            InitializeComponent();

            // 初始化日志记录
            m_Logger = LogFactory.CreateLogger(typeof(frmMain)); 
        }

        /// <summary>
        /// Manage Connections Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnManageConn_Click(object sender, EventArgs e)
        {
            m_Logger.Debug("btnManageConn_Click Start.");

            Point pScreen = this.btnManageConn.PointToScreen(this.btnManageConn.Location);
            Point point = new Point();
            point.X = pScreen.X;
            point.Y = pScreen.Y + this.btnManageConn.Size.Height;

            // The height of the WorkingArea
            int iActulaHeight = SystemInformation.WorkingArea.Height;
            if (point.Y + this.conMSManageConn.Size.Height > iActulaHeight)
            {
                point.X = pScreen.X - this.btnManageConn.Size.Width + 50;
                point.Y = pScreen.Y - this.btnManageConn.Size.Height - 30;
            }

            this.conMSManageConn.Show(point);
            m_Logger.Debug("btnManageConn_Click End.");
        }
    }
}
