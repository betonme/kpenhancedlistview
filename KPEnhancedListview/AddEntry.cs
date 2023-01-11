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
using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Plugins;
using KeePass.Forms;
using KeePass.UI;
using KeePass.Util;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KPEnhancedListview
{
    partial class KPEnhancedListviewExt
    {
        public class KPEnhancedListviewAddEntry : SubPluginBase
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
                // The Listview is designed not to fire the MouseDoubleClick event except when doubleclicking on an item.
                m_lvEntries.MouseDown += new MouseEventHandler(this.OnMouseDown);
            }

            protected override void RemoveHandler()
            {
                m_lvEntries.MouseDown -= new MouseEventHandler(this.OnMouseDown);
            }

            //////////////////////////////////////////////////////////////
            // Sub Plugin functionality

            private void OnMouseDown(object sender, MouseEventArgs e)
            {
                // Only allow left mouse button and double clicks
                if ( (e.Button == MouseButtons.Left) 
                    && (e.Clicks == 2) )
                {
                    ListViewItem add = null;
                    ListViewItem above = null;      
                    
                    //TODO only with one for loop and one listviewitem ??????
                    //use ListView.Groups ?

                    if (m_lvEntries.Items.Count > 0)
                    {
                        // Check right bound of the list item area
                        // We can't check the bottom bound directly because the items are unsorted
                        if (m_lvEntries.Items[0].Bounds.Right < e.X)
                        {
                            // Horizontal click position was beside the item area

                            // We have to find the matching item or first item below the click position or the last item in the list
                            bool belowlast = true;
                            foreach (ListViewItem item in m_lvEntries.Items)
                            {
                                // Check vertical click position
                                if (item.Bounds.Bottom >= e.Y)
                                {
                                    belowlast = false;
                                    if (item.Bounds.Top <= e.Y)
                                    {
                                        // Item match
                                        add = item;
                                        break;
                                    }

                                    // Found item below the click position
                                    // Group header double click detected
                                    if ((add == null)
                                        || (item.Bounds.Top < add.Bounds.Top))
                                    {
                                        // Found first item or
                                        // Found item which is nearer to the group header
                                        //add = item;
                                    }
                                    // We have to go through all items because the items are not sorted
                                }
                                else
                                {
                                    // Item is above the click position

                                    // Found item below the click position
                                    // Group header double click detected
                                    if ((above == null)
                                        || (item.Bounds.Top > above.Bounds.Top))
                                    {
                                        // Found first item or
                                        // Found item which is nearer to the group header
                                        above = item;
                                    }
                                    // We have to go through all items because the items are not sorted
                                }
                            }

                            if (add == null)
                            {
                                if (belowlast)
                                {
                                    add = above;
                                }
                            }
                        }
                        else
                        {
                            // Horizontal click position was inside the item area

                            // Check if the click position is below the item area
                            foreach (ListViewItem item in m_lvEntries.Items)
                            {
                                // Check vertical click position
                                if (item.Bounds.Bottom >= e.Y)
                                {
                                    // There are items below the click position
                                    // Group Header was clicked
                                    add = null;
                                    break;
                                }
                                else
                                {
                                    // Item is above the click position

                                    // Found item below the click position
                                    if ((add == null)
                                        || (item.Bounds.Top > add.Bounds.Top))
                                    {
                                        // Found first item or
                                        // Found item which is nearer to the group header
                                        add = item;
                                    }
                                    // We have to go through all items because the items are not sorted
                                }
                            }
                        }
                    }
                    else
                    {
                        // No item in listview
                        PwGroup pg = Program.MainForm.GetSelectedGroup();
                        AddEntry(pg, 0);
                        return;
                    }

                    if (add != null)
                    {
                        // Empty area double click detected
                        PwListItem pli = (((ListViewItem)add).Tag as PwListItem);
                        if (pli == null) { Debug.Assert(false); return; }
                        PwGroup pg = pli.Entry.ParentGroup;
                        int iIndex = m_lvEntries.Items.IndexOf(add);
                        AddEntry(pg, iIndex);
                    }
                }
            }

            private void AddEntry(PwGroup pg, int iIndex)
            {
                // Create new entry which belongs to given group
                PwEntry pwe = Util.CreateEntry(pg);

                if (pwe == null) { return; }

                // Insort is only working in not sorted and not grouped listviews
                //ListViewItem lviFocus = Util.InsertListEntry(pwe, iIndex);
                ListViewItem lviFocus = Util.AddEntryToList(pwe);
                                
                lviFocus = Util.GuiFindEntry(pwe.Uuid);
                if (lviFocus != null) m_lvEntries.FocusedItem = lviFocus;
                
                m_host.MainWindow.EnsureVisibleEntry(pwe.Uuid);
                m_host.MainWindow.RefreshEntriesList();
                Util.UpdateSaveState();

                //PwObjectList<PwEntry> vSelect = new PwObjectList<PwEntry>();
                //vSelect.Add(pwe);
                //Util.SelectEntries(vSelect, true, true);

                m_lvEntries.Select();
                m_lvEntries.HideSelection = false;
                m_lvEntries.Focus();
                //MAYBE problem: ListView row is only focused but not highlighted (selected?)

                // Start inline editing with new item
                if (lviFocus != null)
                {
                    KPELInlineEditing.StartEditing(lviFocus);
                }
                
                //IDEA: On esc press remove item, undo database changes and update save icon
            }
        }
    }
}
