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
using KeePass.UI;
using KeePassLib;

namespace KPEnhancedListview
{
    partial class KPEnhancedListviewExt
    {
        public class KPEnhancedListviewOpenGroup : SubPluginBase
        {
            //////////////////////////////////////////////////////////////
            // Sub Plugin setup
            private const string m_tbText = "Open Group";
            private const string m_tbToolTip = "Opens the group on double click a group header in the listview area";
            protected const string m_cfgString = "KPEnhancedListview_OpenGroup";

            public KPEnhancedListviewOpenGroup()
            {
                AddMenu(m_cfgString, m_tbText, m_tbToolTip);
            }

            //////////////////////////////////////////////////////////////
            // Sub Plugin handler registration
            protected override void AddHandler()
            {
                // The Listview is designed not to fire the MouseDoubleClick event except when doubleclicking on an item
                // So we use the MouseDown event
                // But if an item of the group which should be opened was selected, the edit item dialog will be opened
                // So we use the MouseUp event
                // But this won't provide the clicks counter
                m_lvEntries.MouseDown += new MouseEventHandler(this.OnMouseDown);
                m_lvEntries.MouseUp += new MouseEventHandler(this.OnMouseUp);
            }

            protected override void RemoveHandler()
            {
                m_lvEntries.MouseDown -= new MouseEventHandler(this.OnMouseDown);
                m_lvEntries.MouseUp -= new MouseEventHandler(this.OnMouseUp);
            }

            //////////////////////////////////////////////////////////////
            // Sub Plugin functionality

            private bool m_bDoubleClick = false;

            private void OnMouseDown(object sender, MouseEventArgs e)
            {
                if ((e.Button == MouseButtons.Left)
                    && (e.Clicks == 2))
                    m_bDoubleClick = true;
            }

            private void OnMouseUp(object sender, MouseEventArgs e)
            {
                // Only if left double click was detected
                if (m_bDoubleClick)
                {
                    m_bDoubleClick = false;

                    if (m_lvEntries.Items.Count > 0)
                    {
                        // Horizontal click position was inside the item area
                        ListViewItem open = null;

                        // FindNearestItem is supported only when the ListView is in SMALLICON or LARGEICON view.
                        // Check right bound of the list item area
                        // We can't check the bottom bound directly because the items are unsorted
                        //if (m_lvEntries.Items[0].Bounds.Right > e.X)
                        {
                            //TODO only with one for loop ??????
                            //use ListView.Groups ?

                            // We have to find the first item below the click position
                            foreach (ListViewItem item in m_lvEntries.Items)
                            {
                                // Check vertical click position
                                if (item.Bounds.Bottom >= e.Y)
                                {
                                    if (item.Bounds.Top <= e.Y)
                                    {
                                        // Item match
                                        open = null;
                                        break;
                                    }

                                    // Found item below the click position
                                    // Group header double click detected
                                    if ( (open==null) 
                                        || (item.Bounds.Top < open.Bounds.Top) )
                                    {
                                        // Found first item or
                                        // Found item which is nearer to the group header
                                        open = item;
                                    }
                                    // We have to go through all items because the items are not sorted
                                }
                            }
                            
                            if (open != null)
                            {
                                // Open list group
                                PwListItem pli = (((ListViewItem)open).Tag as PwListItem);
                                if (pli == null) { Debug.Assert(false); return; }
                                PwGroup pg = pli.Entry.ParentGroup;
                                OpenGroup(pg);
                                return;
                            }
                        }
                    }
                }
            }

            private void OpenGroup(PwGroup pg)
            {
                // Open and select the group which was clicked by the user
                bool modified = m_host.MainWindow.DocumentManager.ActiveDatabase.Modified;
                m_host.MainWindow.UpdateUI(false, null, true, pg, true, pg, modified, m_lvEntries);
            }
        }
    }
}
