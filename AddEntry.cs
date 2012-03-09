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
        public class KPEnhancedListviewAddEntry : KPEnhancedListviewBase
        {
            //////////////////////////////////////////////////////////////
            // Sub Plugin setup
            private const string m_tbText = "Add Entry";
            private const string m_tbToolTip = "Opens the new entry dialog on double click in empty listview area";
            protected const string m_cfgString = "KPEnhancedListview_AddEntry";

            public KPEnhancedListviewAddEntry()
            {
                AddMenu(m_cfgString, m_tbText, m_tbToolTip);
            }

            //////////////////////////////////////////////////////////////
            // Sub Plugin handler registration
            protected override void AddHandler()
            {
                m_clveEntries.MouseUp += new MouseEventHandler(this.OnMouseUp);
            }

            protected override void RemoveHandler()
            {
                m_clveEntries.MouseUp -= new MouseEventHandler(this.OnMouseUp);
            }

            //////////////////////////////////////////////////////////////
            // Sub Plugin functionality
            private DateTime m_mouseDownForAeAt = DateTime.MinValue;

            private void OnMouseUp(object sender, MouseEventArgs e)
            {
                // Only allow left mouse button
                if (e.Button == MouseButtons.Left)
                {
                    ListViewItem item;
                    int idx = Util.GetSubItemAt(m_clveEntries, e.X, e.Y, out item);
                    if (idx == -1)
                    {
                        for (int y = e.Y + 1; y < 100; y++)
                        {
                            idx = Util.GetSubItemAt(m_clveEntries, e.X, y, out item);
                            if (idx != -1)
                            {
                                break;
                            }
                        }
                        if (idx == -1)
                        {
                            // No item was clicked
                            long datNow = DateTime.Now.Ticks;
                            long datMouseDown = m_mouseDownForAeAt.Ticks;

                            // Detect fast double clicking with the left mouse button
                            if (datNow - datMouseDown < m_mouseTimeMin)
                            {
                                // Double click detected
                                // KeePass has no define or constant for the add entry keystroke
                                SendKeys.Send("{INSERT}");
                            }
                            m_mouseDownForAeAt = DateTime.Now;
                        }
                    }
                }
            }
        }
    }
}
