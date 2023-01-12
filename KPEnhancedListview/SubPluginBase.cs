/*
    KPEnhancedListview - Extend the KeePass Listview for inline editing.
    Copyright (C) 2010 - 2012  Frank Glaser  <glaserfrank(at)gmail.com>
    https://github.com/betonme/kpenhancedlistview/
    
    KPEnhancedListview is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
        public abstract class SubPluginBase
        {
            protected bool m_bEnabled = false;
            private ToolStripMenuItem m_tbItem = null;
            private string m_cfgString = "";

            ~ SubPluginBase()
            {
                CleanUp();
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

                // Check custom config and set sub plugin state
                SetEnable( m_host.CustomConfig.GetBool(cfgString, false) );
            }

            protected void CleanUp()
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
                SetEnable( !((ToolStripMenuItem)sender).Checked );


                // Save toggle state
                m_host.CustomConfig.SetBool( m_cfgString, m_bEnabled );
            }

            private void SetEnable(bool bEnable)
            {
                m_tbItem.Checked = bEnable;
                m_bEnabled = bEnable;

                if (bEnable)
                {
                    // Function enabled
                    AddHandler();
                }
                else
                {
                    // Function disabled
                    RemoveHandler();
                }
            }

            protected bool GetEnable()
            {
                return m_bEnabled;
            }

            protected abstract void AddHandler();

            protected abstract void RemoveHandler();
        }
    }
}
