using System;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;

using KeePass;
using KeePass.Plugins;

namespace KPEnhancedListview
{
    partial class KPEnhancedListviewExt
    {
        public abstract class KPEnhancedListviewBase
        {
            private ToolStripMenuItem m_tbItem = null;
            private string m_cfgString = "";

            ~ KPEnhancedListviewBase()
            {
                RemoveMenu();
            }

            protected void AddMenu(string cfgString, string tbText, string tbToolTip)
            {
                // Config identifier
                m_cfgString = cfgString;

                // Add menu item
                m_tbItem = new ToolStripMenuItem();
                m_tbItem.Text = tbText;
                m_tbItem.ToolTipText = tbToolTip;
                m_tbItem.Click += OnMenuItemClick;
                m_tsPopup.DropDownItems.Add(m_tbItem);

                // Check custom config
                if (m_host.CustomConfig.GetBool(cfgString, false))
                {
                    // Function enabled
                    m_tbItem.Checked = true;
                    AddHandler();
                }
                else
                {
                    // Function disabled
                    m_tbItem.Checked = false;
                    RemoveHandler();
                }
            }

            protected void RemoveMenu()
            {
                // Remove our menu items
                m_tsPopup.DropDownItems.Remove(m_tbItem);
                
                // Disable function
                RemoveHandler();
            }

            private void OnMenuItemClick(object sender, EventArgs e)
            {
                if (!m_host.Database.IsOpen)
                {
                    // Doesn't matter
                }

                // Toggle menu item
                ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;

                // Save toggle state
                m_host.CustomConfig.SetBool(m_cfgString, m_tbItem.Checked);

                if (((ToolStripMenuItem)sender).Checked)
                {
                    // Enable function
                    AddHandler();
                }
                else
                {
                    // Disable function
                    RemoveHandler();
                }
            }

            protected abstract void AddHandler();

            protected abstract void RemoveHandler();
        }
    }
}
