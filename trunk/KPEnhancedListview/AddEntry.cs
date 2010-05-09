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

namespace KPEnhancedListview
{
    partial class KPEnhancedListviewExt
    {
        private const string m_cfgAddEntry = "KPEnhancedListview_AddEntry";

        private DateTime m_mouseDownForAeAt = DateTime.MinValue;

        private ToolStripMenuItem m_tsmiAddEntry = null;

        private void InitializeAddEntry()
        {
            // Add menu item
            m_tsmiAddEntry = new ToolStripMenuItem();
            m_tsmiAddEntry.Text = "Double Click add an Entry";
            m_tsmiAddEntry.Click += OnMenuAddEntry;
            m_tsMenu.Add(m_tsmiAddEntry);

            // Check custom config
            if (m_host.CustomConfig.GetBool(m_cfgAddEntry, false))
            {
                m_tsmiAddEntry.Checked = true;
                AddHandlerAddEntry();
            }
            else
            {
                m_tsmiAddEntry.Checked = false;
                RemoveHandlerAddEntry();
            }
        }

        public void TerminateAddEntry()
        {
            // Remove our menu items
            m_tsMenu.Remove(m_tsmiAddEntry);

            RemoveHandlerAddEntry();
        }

        private void AddHandlerAddEntry()
        {
            m_clveEntries.MouseUp += new MouseEventHandler(this.OnMouseUp);
        }

        private void RemoveHandlerAddEntry()
        {
            m_clveEntries.MouseUp -= new MouseEventHandler(this.OnMouseUp);
        }

        private void OnMenuAddEntry(object sender, EventArgs e)
        {
            if (!m_host.Database.IsOpen)
            {
                // doesn't matter
            }

            m_tsmiAddEntry.Checked = !m_tsmiAddEntry.Checked;

            // save config
            m_host.CustomConfig.SetBool(m_cfgInlineEditing, m_tsmiAddEntry.Checked);

            if (m_tsmiAddEntry.Checked)
            {
                // enable function
                AddHandlerAddEntry();
            }
            else
            {
                // disable function
                RemoveHandlerAddEntry();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            // Only allow left mouse button
            if (e.Button == MouseButtons.Left)
            {
                ListViewItem item;
                int idx = GetSubItemAt(e.X, e.Y, out item);
                if (idx == -1)
                {
                    // No item was clicked
                    long datNow = DateTime.Now.Ticks;
                    long datMouseDown = m_mouseDownForAeAt.Ticks;

                    // Fast double clicking with the left moaus button
                    if (datNow - datMouseDown < m_mouseTimeMin)
                    {
                        // KeePass has no define or constant for the add entry keystroke
                        SendKeys.Send("{INSERT}");
                    }
                    m_mouseDownForAeAt = DateTime.Now;
                }
            }
        }
    }
}
