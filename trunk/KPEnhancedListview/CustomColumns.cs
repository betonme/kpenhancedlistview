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
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KPEnhancedListview
{
    partial class KPEnhancedListviewExt
    {
        private List<ColumnHeader> m_lCustomColums = null;

        private void InitializeCustomColumns()
        {
            m_lCustomColums = new List<ColumnHeader>();

            m_evEntries.Suppress("OnPwListColumnWidthChanging");
            m_evEntries.Suppress("OnPwListColumnWidthChanged");

            //TODO sorting is not working after leaving pwentry form
            m_evEntries.Suppress("OnPwListColumnClick");

            AddHandlerCustomColumns();
        }

        public void TerminateCustomColumns()
        {
            m_evEntries.Resume("OnPwListColumnWidthChanging");
            m_evEntries.Resume("OnPwListColumnWidthChanged");

            //TODO sorting is not working after leaving pwentry form
            m_evEntries.Resume("OnPwListColumnClick");

            RemoveHandlerCustomColumns();
        }

        private void AddHandlerCustomColumns()
        {
            m_clveEntries.ColumnWidthChanging += new ColumnWidthChangingEventHandler(this.OnPwListCustomColumnWidthChanging);
            m_clveEntries.ColumnWidthChanged += new ColumnWidthChangedEventHandler(this.OnPwListCustomColumnWidthChanged);

            m_clveEntries.ColumnClick += new ColumnClickEventHandler(this.OnPwListCustomColumnClick);
            m_host.MainWindow.UIStateUpdated += new EventHandler(this.OnPwListCustomColumnUpdate);

            //m_clveEntries.GotFocus
            m_clveEntries.SelectedIndexChanged += new EventHandler(this.OnPwListCustomColumnUpdate);

            m_clveEntries.MouseDoubleClick += new MouseEventHandler(this.OnEntryAction);

            // Not working KeePass doesn't 
            //  send CustomColumns
            //  send event on OwnerDraw is set to true
            //m_host.MainWindow.DefaultEntryAction += OnEntryAction;
        }

        private void RemoveHandlerCustomColumns()
        {
            m_clveEntries.ColumnWidthChanging -= new ColumnWidthChangingEventHandler(this.OnPwListCustomColumnWidthChanging);
            m_clveEntries.ColumnWidthChanged -= new ColumnWidthChangedEventHandler(this.OnPwListCustomColumnWidthChanged);

            m_clveEntries.ColumnClick -= new ColumnClickEventHandler(this.OnPwListCustomColumnClick);
            m_host.MainWindow.UIStateUpdated -= new EventHandler(this.OnPwListCustomColumnUpdate);

            m_clveEntries.MouseDoubleClick -= new MouseEventHandler(this.OnEntryAction);
        }

        private void OnEntryAction(object sender, MouseEventArgs e)
        {
            // KeePass does not differ between mouse buttons
            //if (e.Button == MouseButtons.Left)
            {
                ListViewItem item;
                int subitem = GetSubItemAt(e.X, e.Y, out item);

                if (subitem >= (int)AppDefs.ColumnId.Count)
                {
                    string col = m_clveEntries.Columns[subitem].Text;
                    PwEntry pe = (PwEntry)item.Tag;
                    pe = m_host.Database.RootGroup.FindEntry(pe.Uuid, true);

                    // Copy CustomColumn subitem text to clipboard
                    bool bCnt;
                    if (pe.Strings.Exists(col))
                    {
                        bCnt = ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(col), false, m_host.MainWindow, pe, null);
                    }
                    else
                    {
                        // SubItem does not exist
                        bCnt = ClipboardUtil.CopyAndMinimize("", false, m_host.MainWindow, pe, null);
                    }
                    if (bCnt) m_host.MainWindow.StartClipboardCountdown();
                }
            }
        }

        private void OnMenuAddCustomColumns(object sender, EventArgs e)
        {
            /*
            if (!m_host.Database.IsOpen)
            {
                MessageBox.Show("You first need to open a database!", "CustomColumn");
                return;
            }*/

            List<string> strl = new List<string>();

            if (m_host.Database.IsOpen)
            {
                // Add all known pwentry strings
                foreach (PwEntry pe in m_host.Database.RootGroup.GetEntries(true))
                {
                    foreach (KeyValuePair<string, ProtectedString> pstr in pe.Strings)
                    {
                        if (!strl.Contains(pstr.Key))
                        {
                            //if (!Enum.IsDefined(typeof(AppDefs.ColumnId), pstr.Key))
                            if (!PwDefs.IsStandardField(pstr.Key))
                            {
                                strl.Add(pstr.Key);
                            }
                        }
                    }
                }
            }

            // Remove all listview columns
            foreach (ColumnHeader ch in m_clveEntries.Columns)
            {
                strl.Remove(ch.Text);
            }

            strl.Sort();

            // Show dialog
            string[] strs = InputComboBox.Show(strl.ToArray(), "Add CustomColumn with name:", "Add CustomColumn");

            if (strs != null)
            {
                m_clveEntries.SuspendLayout();
                //m_clveEntries.BeginUpdate();
                m_evEntries.Suppress("OnPwListColumnWidthChanged");
#if USE_NET20
                bool b = false;
                string[] st = m_lCustomColums.ConvertAll<string>(ch => ch.Text).ToArray();
#endif
                foreach (string str in strs)
                {
#if USE_NET20
                    foreach (string s in st)
                    {
                        if (s.Equals(str))
                        {
                            b = true;
                            break;
                        }
                    }
                    if (!b)
#else
                      if(!m_lCustomColums.ConvertAll<string>(ch => ch.Text).ToArray().Contains(str))
#endif
                    {
                        ColumnHeader chd = m_clveEntries.Columns.Add(str);

                        m_lCustomColums.Add(chd);
                    }
                }

                //TODO not necessary - draw item will handle it 
                UpdateListView();

                m_evEntries.Resume("OnPwListColumnWidthChanged");
                //m_clveEntries.EndUpdate();
                m_clveEntries.ResumeLayout();
                //m_clveEntries.Update();
            }
        }

        private void OnMenuRemoveCustomColumns(object sender, EventArgs e)
        {
            /*
            if (!m_host.Database.IsOpen)
            {
                MessageBox.Show("You first need to open a database!", "CustomColumn");
                return;
            }*/

            if (m_lCustomColums.Count == 0)
            {
                MessageBox.Show("No Custom Culomns available!", "CustomColumn");
                return;
            }

            List<string> strl = new List<string>();
            strl.AddRange(m_lCustomColums.ConvertAll<string>(ch => ch.Text));
            strl.Sort();
            string[] strs = strl.ToArray();

            // Show dialog
            strs = InputListBox.Show(strs, "Remove CustomColumn with name:", "Remove CustomColumn");

            if (strs != null)
            {
                m_clveEntries.BeginUpdate();
//TODO Improve Performance
                foreach (string str in strs)
                {
                    ColumnHeader chd = m_lCustomColums.Find(ch => ch.Text.Equals(str));
                    int i = chd.Index;

                    if (chd != null)
                    {
                        m_lCustomColums.Remove(chd);
                        m_clveEntries.Columns.Remove(chd);

                        //TODO remove function like UpdateListview()
                        foreach (ListViewItem lvi in m_clveEntries.Items)
                        {
                            if (lvi.SubItems.Count > i)
                            {
                                //TODO try catch
                                try
                                {
                                    lvi.SubItems.RemoveAt(i);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
//TODO remove
                                    MessageBox.Show("exception out of range");
                                }
                            }
                        }
                    }
                }

                m_clveEntries.EndUpdate();
                //m_clveEntries.Update();
            }
        }

        // Check if a CustomColumn width is changing
        private void OnPwListCustomColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            // Native KeePass OnPwListColumnWidthChanging Event is always suppressed
            // Reimplement KeePass OnPwListColumnWidthChanging EventHandler
            if (m_clveEntries.Columns[e.ColumnIndex].Width == 0)
            {
                e.Cancel = true;
            }

            // Allow event OnPwListColumnWidthChanged only for native KeePass columns
            if (e.ColumnIndex >= (int)AppDefs.ColumnId.Count)
            {
                m_evEntries.Suppress("OnPwListColumnWidthChanged");
            }
            else
            {
                m_evEntries.Resume("OnPwListColumnWidthChanged");
            }
        }

        // Suppress KeePass EventHandler after Keepass OnPwListColumnWidthChanged Event
        private void OnPwListCustomColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            m_evEntries.Suppress("OnPwListColumnWidthChanged");
        }

        // Check after ColumnClick, if all CustomColums SubItems are available
        private void OnPwListCustomColumnClick(object sender, ColumnClickEventArgs e)
        {
            //OnPwListCustomColumnUpdate(sender, e);

            //TODO not necessary - draw item will handle it 
            //TODO only on custom column ?
            UpdateListView();

            //TODO sort custom columns correct
            //Reimplement Listsort function
            //and supress keepass sort fct
            //m_clveEntries.Columns[e.Column].ListView.Sort();

            //SortPasswordList(true, e.Column, true);
            SortListView();
        }

        // Check after UIStateUpdated if all CustomColums SubItems are available
        private void OnPwListCustomColumnUpdate(object sender, EventArgs e)
        {
            //TODO not necessary - draw item will handle it 
            UpdateListView();

            SortListView();
        }

        private bool UpdateListViewItem(ListViewItem lvi)
        {
            m_clveEntries.BeginUpdate();

            int i;
            string str;
            string sit;

            // Check custom columns
            foreach (ColumnHeader chd in m_lCustomColums)
            {
                i = chd.Index;
                str = chd.Text;

                if (lvi.SubItems.Count == i)
                {
                    try
                    {
                        sit = ((PwEntry)lvi.Tag).Strings.Get(str).ReadString();
                        lvi.SubItems.Add(StringToOneLine(sit, i));
                    }
                    catch (NullReferenceException)
                    {
                        lvi.SubItems.Add("");
                    }
                }
            }

            //SortListView();
            
            m_clveEntries.EndUpdate();
            //m_clveEntries.Update();

            return true;
        }

        private bool UpdateListView()
        {
            //if (m_clveEntries.Items.Count == 0)
            {
                //return false;
            }

            //if (m_clveEntries.Items[0].SubItems.Count == m_clveEntries.Columns.Count)
            //if (m_clveEntries.Items[m_clveEntries.Items.Count - 1].SubItems.Count == m_clveEntries.Columns.Count)
            {
                //return false;
            }

            m_clveEntries.BeginUpdate();

            int i;
            string str;
            string sit;

            // Check subitems
            foreach (ListViewItem lvi in m_clveEntries.Items)
            {
//TODO continue if item has already all subitems ???

                // Check custom columns
                foreach (ColumnHeader chd in m_lCustomColums)
                {
                    i = chd.Index;
                    str = chd.Text;
                    
//test0
#if test0
                    try
                    {
                        sit = ((PwEntry)lvi.Tag).Strings.Get(str).ReadString();
                    }
                    catch (NullReferenceException)
                    {
                        //sit = "";
                        continue;
                    }

                    if (lvi.SubItems.Count == i)
                    {
                        // Subitem has not been added to the listview - add subitem
                        lvi.SubItems.Add(StringToOneLine(sit, i));
                    }
                    else if (lvi.SubItems.Count < i)
                    {
                        // There are fewer items as expected
                        // This should never happen
                        int j = i-lvi.SubItems.Count;
                        do
                        {
                            lvi.SubItems.Add("");
                            j++;
                        }
                        while ( j < i);

                        lvi.SubItems.Add(StringToOneLine(sit, i));
                    }
/*                    else
                    {
                        //TODO replace subitem
                        // Subitem already added to listview
                        continue;
                    }
*/
#endif

//#if test1
                    if (lvi.SubItems.Count == i)
                    {
                        // Subitem has not been added to the listview - add subitem
                        //1
                        /*
                        PwEntry pe = (PwEntry)lvi.Tag;
                        if (pe.Strings.Exists(str))
                        {
                            string sit = pe.Strings.Get(str).ReadString();
                        */

                        //2
                        /*
                        if (((PwEntry)lvi.Tag).Strings.Exists(str))
                        {
                            sit = ((PwEntry)lvi.Tag).Strings.Get(str).ReadString();
                            lvi.SubItems.Add(StringToOneLine(sit, i));
                        }
                        else
                        {
                            lvi.SubItems.Add("");
                        }*/

                        try
                        {
                            sit = ((PwEntry)lvi.Tag).Strings.Get(str).ReadString();
                            lvi.SubItems.Add(StringToOneLine(sit, i));
                        }
                        catch (NullReferenceException)
                        {
                            lvi.SubItems.Add("");
                        }
                    }

                    else if (lvi.SubItems.Count < i)
                    {
                        // There are fewer items as expected
                        // This should never happen
                        do
                        {
                            lvi.SubItems.Add("");
                        }
                        while (lvi.SubItems.Count < i);
//TODO add item
                    }
                    /*                    else
                                        {
                    //TODO replace subitem
                                            // Subitem already added to listview
                                            continue;
                                        }
                    */
//#endif
                }
            }

            //SortListView();
            
            m_clveEntries.EndUpdate();
            //m_clveEntries.Update();

            return true;
        }

        private void SortListView()
        {
            ListSorter ls = (ListSorter)m_clveEntries.ListViewItemSorter;
            if (ls != null)
            {
                if (ls.Column >= (int)AppDefs.ColumnId.Count)
                {
                    //Thread.Sleep(500);
                    m_clveEntries.ListViewItemSorter = new ListSorter(ls.Column, ls.Order, true, false);
                    
                    m_clveEntries.Sort();
                }
            }
        }
    }
}
