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
        private List<ColumnHeader> m_lCC = null;

        private ToolStripMenuItem m_tsmiCustomColumns = null;
        private ToolStripMenuItem m_tsmiAddCustomColumns = null;
        private ToolStripMenuItem m_tsmiRemoveCustomColumns = null;

        private List<CustomColumn> m_lCustomColumns = null;
        
        private void InitializeCustomColumns()
        {
            m_lCC = new List<ColumnHeader>();
            m_lCustomColumns = new List<CustomColumn>();

            // Add menu item
            m_tsmiCustomColumns = new ToolStripMenuItem();
            m_tsmiCustomColumns.Image = Properties.Resources.B16x16_CustomColumns;
            m_tsmiCustomColumns.Text = "Custom Columns";
            m_tsmiCustomColumns.Click += OnMenuCustomColumns;
            m_tsMenu.Add(m_tsmiCustomColumns);

            // Add menu item
            m_tsmiAddCustomColumns = new ToolStripMenuItem();
            m_tsmiAddCustomColumns.Text = "Add Custom Columns";
            //m_tsmiAddCustomColumns.Click += OnMenuAddCustomColumns;
            //m_tsMenu.Add(m_tsmiAddCustomColumns);

            // Add menu item
            m_tsmiRemoveCustomColumns = new ToolStripMenuItem();
            m_tsmiRemoveCustomColumns.Text = "Remove Custom Columns";
            //m_tsmiRemoveCustomColumns.Click += OnMenuRemoveCustomColumns;
            //m_tsMenu.Add(m_tsmiRemoveCustomColumns);

            AddHandlerCustomColumns();

            // Read CustomConfig
            //TODO
            // No config found - get actual listview columns when user calls custom config dialog
            // If we call here the GetListViewColumns KeePass will post an error message
            //Call OnFormLoad() or us try cath in fct
            //m_lCustomColumns = GetListViewColumns();
        }

        public void TerminateCustomColumns()
        {
            // Remove our menu items
            m_tsMenu.Remove(m_tsmiCustomColumns);
            m_tsMenu.Remove(m_tsmiAddCustomColumns);
            m_tsMenu.Remove(m_tsmiRemoveCustomColumns);

            foreach (ColumnHeader chd in m_lCC)
            {
                m_clveEntries.Columns.Remove(chd);
            }

            m_lCC = new List<ColumnHeader>();

            RemoveHandlerCustomColumns();
        }

        private void AddHandlerCustomColumns()
        {
            if (m_lCC.Count != 0)
            {
                m_evEntries.Suppress("OnPwListColumnWidthChanging");
                m_evEntries.Suppress("OnPwListColumnWidthChanged");

                m_clveEntries.ColumnWidthChanging += new ColumnWidthChangingEventHandler(this.OnPwListCustomColumnWidthChanging);
                m_clveEntries.ColumnWidthChanged += new ColumnWidthChangedEventHandler(this.OnPwListCustomColumnWidthChanged);

                m_clveEntries.ColumnClick += new ColumnClickEventHandler(this.OnPwListCustomColumnClick);
                m_host.MainWindow.UIStateUpdated += new EventHandler(this.OnPwListCustomColumnUpdate);

                m_clveEntries.MouseDoubleClick += new MouseEventHandler(this.OnEntryAction);

                // Not working KeePass doesn't 
                //  send CustomColumns
                //  send event on OwnerDraw is set to true
                //m_host.MainWindow.DefaultEntryAction += OnEntryAction;
            }
        }

        private void RemoveHandlerCustomColumns()
        {
            if (m_lCC.Count == 0)
            {
                m_evEntries.Resume("OnPwListColumnWidthChanging");
                m_evEntries.Resume("OnPwListColumnWidthChanged");

                m_clveEntries.ColumnWidthChanging -= new ColumnWidthChangingEventHandler(this.OnPwListCustomColumnWidthChanging);
                m_clveEntries.ColumnWidthChanged -= new ColumnWidthChangedEventHandler(this.OnPwListCustomColumnWidthChanged);

                m_clveEntries.ColumnClick -= new ColumnClickEventHandler(this.OnPwListCustomColumnClick);
                m_host.MainWindow.UIStateUpdated -= new EventHandler(this.OnPwListCustomColumnUpdate);

                m_clveEntries.MouseDoubleClick -= new MouseEventHandler(this.OnEntryAction);
            }
        }

        private void OnMenuCustomColumns(object sender, EventArgs e)
        {
            if (!m_host.Database.IsOpen)
            {
                MessageBox.Show("You first need to open a database!", "CustomColumn");
                return;
            }

            CustomColumnsForm ccf = new CustomColumnsForm();

            //TODO not necessary for 2.11
            m_lCustomColumns = GetListViewColumns();

            ccf.InitEx(m_host, m_clveEntries, m_lCustomColumns);

            //ccf.ShowDialog();
            if (ccf.ShowDialog() == DialogResult.OK)
            {
                // Get new CustomColumns only on result ok
                m_lCustomColumns = ccf.GetCustomColumns();

                // The number of visible entries has not changed, so we can call RefreshEntriesList
                m_host.MainWindow.RefreshEntriesList();

                UpdateListViewColumns();
                UpdateListView();
            }

            /*foreach (CustomColumn cl in m_lCustomColumns)
            {
                MessageBox.Show(cl.Column.ToString() + " " + cl.Hide.ToString());
            }*/

//TODO add remove columns
// refreshentrieslist
// updatelistview
        }

#if false   
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
                strl = KPELVUtil.GetListEntriesUserStrings(m_host.Database.RootGroup);
            }

            // Remove all listview columns
            foreach (ColumnHeader ch in m_clveEntries.Columns)
            {
                strl.Remove(ch.Text);
            }

            // Show dialog
            string[] strs = InputComboBox.Show(strl.ToArray(), "Add CustomColumn with name:", "Add CustomColumn");

            if (strs != null)
            {
                strl = new List<string>(strs);

                foreach (ColumnHeader ch in m_clveEntries.Columns)
                {
                    strl.Remove(ch.Text);
                }

                if (strl.Count != 0)
                {
                    //m_clveEntries.SuspendLayout();
                    m_clveEntries.BeginUpdate();
                    m_evEntries.Suppress("OnPwListColumnWidthChanged");

                    foreach (string str in strl)
                    {
                        ColumnHeader chd = m_clveEntries.Columns.Add(str);

                        m_lCC.Add(chd);
                    }

                    // Not necessary - DrawItem will handle it 
                    //UpdateListView();

                    m_evEntries.Resume("OnPwListColumnWidthChanged");
                    m_clveEntries.EndUpdate();
                    //m_clveEntries.ResumeLayout();
                    //m_clveEntries.Update();
                }
            }

            AddHandlerCustomColumns();
        }
     
        private void OnMenuRemoveCustomColumns(object sender, EventArgs e)
        {
            /*
            if (!m_host.Database.IsOpen)
            {
                MessageBox.Show("You first need to open a database!", "CustomColumn");
                return;
            }*/

            if (m_lCC.Count == 0)
            {
                MessageBox.Show("No Custom Culomns available!", "CustomColumn");
                return;
            }

            List<string> strl = new List<string>();
            strl.AddRange(m_lCC.ConvertAll<string>(ch => ch.Text));
            strl.Sort();

            // Show dialog
            string[] strs = InputListBox.Show(strl.ToArray(), "Remove CustomColumn with name:", "Remove CustomColumn");

            if (strs != null)
            {
                m_clveEntries.BeginUpdate();

                foreach (string str in strs)
                {
                    ColumnHeader chd = m_lCC.Find(ch => ch.Text.Equals(str));
                    int i = chd.Index;

                    if (chd != null)
                    {
                        m_lCC.Remove(chd);
                        m_clveEntries.Columns.Remove(chd);

                        // Removing SubItems not necessary 
                        // RefreshEntriesList will clear the entire listview
                        // After that UpdateListview will build up the whole listview subitems

                        // Remove function like UpdateListview()
                        // Also possible call RefreshEntriesList or UpdateEntryList
                        /*foreach (ListViewItem lvi in m_clveEntries.Items)
                        {
                            if (lvi.SubItems.Count > i)
                            {
                                try
                                {
                                    lvi.SubItems.RemoveAt(i);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    // nothing todo
                                }
                            }
                        }*/
                    }
                }

                // The number of visible entries has not changed, so we can call RefreshEntriesList
                m_host.MainWindow.RefreshEntriesList();

                m_clveEntries.EndUpdate();
                //m_clveEntries.Update();
            }

            RemoveHandlerCustomColumns();
        }
#endif
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
//TODO if column width == 0 then update m_lCustomColumns and config
            m_evEntries.Suppress("OnPwListColumnWidthChanged");
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
                    bool bCnt = false;
                    if (pe.Strings.Exists(col))
                    {
                        bCnt = ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(col), false, m_host.MainWindow, pe, null);
                    }
                    else
                    {
                        // SubItem does not exist
                        //bCnt = ClipboardUtil.CopyAndMinimize(string.Empty, false, m_host.MainWindow, pe, null);
                        MessageBox.Show("Item is empty");
                    }
                    if (bCnt) m_host.MainWindow.StartClipboardCountdown();
                }
            }
        }

        // Check after ColumnClick, if all CustomColums SubItems are available
        private void OnPwListCustomColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Is done within DrawItem
            if (m_clveEntries.Items.Count != 0)
            {
                if (m_clveEntries.TopItem.SubItems.Count != m_clveEntries.Columns.Count)
                {
                    UpdateListView();
                }
            }
        }

        // Check after UIStateUpdated if all CustomColums SubItems are available
        private void OnPwListCustomColumnUpdate(object sender, EventArgs e)
        {
            // DON'T - Is very slow and flickrs
            // Is done within DrawItem
            if (m_clveEntries.Items.Count != 0)
            {
                //TEST
                //if (m_clveEntries.TopItem.SubItems.Count != m_clveEntries.Columns.Count)
                {           
                    UpdateListView();
                }
            }
        }

        private List<CustomColumn> GetListViewColumns()
        {
            List<CustomColumn> lCustomColumns = new List<CustomColumn>();
            int[] iColumnOrder = GetColumnOrder();
            ToolStripItem[] tsi = null;
            ToolStripMenuItem tsmi = null;

            foreach (ColumnHeader ch in m_clveEntries.Columns)
            {
                CustomColumn cc = new CustomColumn();
                cc.Column = ch.Text;
                cc.Name = ch.Text;
                cc.Index = ch.Index;
                cc.Order = Array.IndexOf(iColumnOrder, ch.Index);
                if (ch.Width != 0) 
                    cc.Enable = true;
                else
                    cc.Enable = false;
                switch ((AppDefs.ColumnId)ch.Index)
                {
                    case AppDefs.ColumnId.Title:
                        tsi = m_host.MainWindow.MainMenu.Items.Find("m_menuViewHideTitles", true);
                        tsmi = tsi[0] as ToolStripMenuItem;
                    //TODO test always possible and uptodate    
                    //if (Program.Config.MainWindow.ColumnsDict[PwDefs.TitleField].HideWithAsterisks)
                        if (tsmi.Checked)
                            cc.Hide = HideStatus.Full;
                        else
                            cc.Hide = HideStatus.Unhidden;
                        break;
                    case AppDefs.ColumnId.UserName:
                        tsi = m_host.MainWindow.MainMenu.Items.Find("m_menuViewHideUserNames", true);
                        tsmi = tsi[0] as ToolStripMenuItem;
                        if (tsmi.Checked)
                            cc.Hide = HideStatus.Full;
                        else
                            cc.Hide = HideStatus.Unhidden;
                        break;
                    case AppDefs.ColumnId.Password:
                        tsi = m_host.MainWindow.MainMenu.Items.Find("m_menuViewHidePasswords", true);
                        tsmi = tsi[0] as ToolStripMenuItem;
                        if (tsmi.Checked)
                            cc.Hide = HideStatus.Full;
                        else
                            cc.Hide = HideStatus.Unhidden;
                        break;
                    case AppDefs.ColumnId.Url:
                        tsi = m_host.MainWindow.MainMenu.Items.Find("m_menuViewHideURLs", true);
                        tsmi = tsi[0] as ToolStripMenuItem;
                        if (tsmi.Checked)
                            cc.Hide = HideStatus.Full;
                        else
                            cc.Hide = HideStatus.Unhidden;
                        break;
                    case AppDefs.ColumnId.Notes:
                        tsi = m_host.MainWindow.MainMenu.Items.Find("m_menuViewHideNotes", true);
                        tsmi = tsi[0] as ToolStripMenuItem;
                        if (tsmi.Checked)
                            cc.Hide = HideStatus.Full;
                        else
                            cc.Hide = HideStatus.Unhidden;
                        break;
                    case AppDefs.ColumnId.CreationTime:
                    case AppDefs.ColumnId.LastAccessTime:
                    case AppDefs.ColumnId.LastModificationTime:
                    case AppDefs.ColumnId.ExpiryTime:
                    case AppDefs.ColumnId.Uuid:
                    case AppDefs.ColumnId.Attachment:
                        cc.Hide = HideStatus.Unhidden;
                        break;
                    default:
                        if (ch.Index < m_lCustomColumns.Count)
                        {
                            // Read Hide Status from m_lCustomColumns
                            cc.Hide = m_lCustomColumns[ch.Index].Hide;
                        }
                        else
                        {
                            // We should never reach this state 
                            // There are columns, which are not in defined in our struct
                            cc.Hide = HideStatus.Unhidden;
                            Debug.Assert(true, "Undefined columns found", cc.Column.ToString());
                        }
                        break;
                }
                switch ((AppDefs.ColumnId)ch.Index)
                {
                    case AppDefs.ColumnId.Title:
                        cc.Protect = m_host.Database.MemoryProtection.ProtectTitle;
                        break;
                    case AppDefs.ColumnId.UserName:
                        cc.Protect = m_host.Database.MemoryProtection.ProtectUserName;
                        break;
                    case AppDefs.ColumnId.Password:
                        cc.Protect = m_host.Database.MemoryProtection.ProtectPassword;
                        break;
                    case AppDefs.ColumnId.Url:
                        cc.Protect = m_host.Database.MemoryProtection.ProtectUrl;
                        break;
                    case AppDefs.ColumnId.Notes:
                        cc.Protect = m_host.Database.MemoryProtection.ProtectNotes;
                        break;
                    case AppDefs.ColumnId.CreationTime:
                    case AppDefs.ColumnId.LastAccessTime:
                    case AppDefs.ColumnId.LastModificationTime:
                    case AppDefs.ColumnId.ExpiryTime:
                    case AppDefs.ColumnId.Uuid:
                    case AppDefs.ColumnId.Attachment:
                        cc.Protect = false;
                        break;
                    default:
                        if (ch.Index < m_lCustomColumns.Count)
                        {
                            // Read Protect Status from m_lCustomColumns
                            cc.Protect = m_lCustomColumns[ch.Index].Protect;
                        }
                        else
                        {
                            // We should never reach this state 
                            // There are columns, which are not in defined in our struct
                            cc.Protect = false;
                            Debug.Assert(true, "Undefined columns found", cc.Column.ToString());
                        }
                        break;
                }
                switch ((AppDefs.ColumnId)ch.Index)
                {
                    case AppDefs.ColumnId.CreationTime:
                    case AppDefs.ColumnId.LastAccessTime:
                    case AppDefs.ColumnId.LastModificationTime:
                    case AppDefs.ColumnId.ExpiryTime:
                    case AppDefs.ColumnId.Uuid:
                    case AppDefs.ColumnId.Attachment:
                        cc.ReadOnly = true;
                        break;
                    default:
                        cc.ReadOnly = false;
                        break;
                }
                cc.Width = ch.Width;
                cc.Sort = SortOrder.None; 
                lCustomColumns.Add(cc);
            }

            ListSorter ls = (ListSorter)m_clveEntries.ListViewItemSorter;
            if (ls != null)
            {
                if (ls.Column < lCustomColumns.Count)
                {
                    lCustomColumns[ls.Column].Sort = ls.Order;
                }
            }

            // Custom sorting column order
            lCustomColumns.Sort();

            return lCustomColumns;
        }


        /*
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
                        lvi.SubItems.Add(KPELVUtil.StringToOneLine(sit, i));
                    }
                    catch (NullReferenceException)
                    {
                        lvi.SubItems.Add(string.Empty);
                    }
                }
                //TODO else ...
            }

            //SortListView();
            
            m_clveEntries.EndUpdate();
            //m_clveEntries.Update();

            return true;
        }*/

        private void UpdateListView()
        {
            m_clveEntries.BeginUpdate();

            //UpdateListViewColumns();

            UpdateListViewSubItems();

            //SortListView();

            //Select...

            m_clveEntries.EndUpdate();
        }

        private void UpdateListViewColumns()
        { 
//TODO
            m_evEntries.Suppress("OnPwListColumnWidthChanged");

            //MessageBox.Show(m_lCustomColumns.Count.ToString());
            foreach (CustomColumn cc in m_lCustomColumns)
            {
                if (cc.Index < (int)AppDefs.ColumnId.Count)
                {
                    continue;
                }

                // Rename column
                try
                {
                    m_clveEntries.Columns[cc.Index].Text = cc.Name;
                }
                catch
                {
                    m_clveEntries.Columns.Add(cc.Name);
                }

                m_clveEntries.Columns[cc.Index].Width = cc.Width;
            }

            //m_evEntries.Resume("OnPwListColumnWidthChanged");
        }

        private void UpdateListViewSubItems()
        {
            /*
            if (_editingControl != null)
            {
                return;
            }*/

            if (m_clveEntries.Items.Count == 0)
            {
                return;
            }

            string sit;

            // Check subitems
            foreach (ListViewItem lvi in m_clveEntries.Items)
            {
                if (lvi.SubItems.Count == m_clveEntries.Columns.Count)
                    continue;

                // Check custom columns
                foreach (CustomColumn cc in m_lCustomColumns)
                {
                    if (cc.Index < (int)AppDefs.ColumnId.Count)
                    {
                        continue;
                    }

                    if (cc.Hide == HideStatus.Unhidden)
                    {
                        //MessageBox.Show(cc.Column.ToString()); return;
                        try
                        {
                            // Get entry
                            sit = ((PwEntry)lvi.Tag).Strings.Get(cc.Column).ReadString();
                            sit = Util.StringToOneLine(sit, cc.Index);
                        }
                        catch (NullReferenceException)
                        {
                            // No entry found set emtpy
                            sit = string.Empty;
                        }
                    }
                    else if (cc.Hide == HideStatus.Full)
                    {
                        // Hide all entries
                        sit = PwDefs.HiddenPassword;
                    }
                    else //if (cc.Hide == HideStatus.Lazy)
                    {
                        try
                        {
                            // Check if there is an entry
                            if (((PwEntry)lvi.Tag).Strings.Get(cc.Column).IsProtected)
                            {
                                // Set entry hidden
                                sit = PwDefs.HiddenPassword;
                            }
                            else
                            {
                                // Get entry
                                sit = ((PwEntry)lvi.Tag).Strings.Get(cc.Column).ReadString();
                                sit = Util.StringToOneLine(sit, cc.Index); 
                            }
                        }
                        catch (NullReferenceException)
                        {
                            // No entry found set emtpy
                            sit = string.Empty;
                        }
                    }

                    lvi.SubItems.Add(sit);
                }
            }

            SortListView();

            //m_clveEntries.EndUpdate();
            //m_clveEntries.Update();

            return;
        }

        private void UpdateListViewSubItemsCHD()
        {
            if (_editingControl != null)
            {
                return;
            }

            if (m_clveEntries.Items.Count == 0)
            {
                return;
            }

            //notworking
            /*
            if ((m_clveEntries.Items[0].SubItems.Count == m_clveEntries.Columns.Count) && (m_clveEntries.Items[m_clveEntries.Items.Count - 1].SubItems.Count == m_clveEntries.Columns.Count))
            {
                return;
            }*/

            /*
            PwEntry[] vSelected = m_host.MainWindow.GetSelectedEntries();
            if (vSelected == null) vSelected = new PwEntry[0];
            if (vSelected[0].SubItems.Count == m_clveEntries.Columns.Count)
            {
                return;
            }*/
            
            // Check selected item
            /*if (m_clveEntries.SelectedIndices.Count > 0)
            {
                ListViewItem Item = m_clveEntries.SelectedItems[0];
                if (Item != null)
                {
                    if (Item.SubItems.Count == m_clveEntries.Columns.Count)
                    {
                        return;
                    }
                }
            }*/

            

            //int i;
            //string str;
            string sit;

            // Check subitems
            foreach (ListViewItem lvi in m_clveEntries.Items)
            {
                if (lvi.SubItems.Count == m_clveEntries.Columns.Count)
                    continue;

                // Check custom columns
                foreach (ColumnHeader chd in m_lCC)
                {
                    //i = chd.Index;
                    //str = chd.Text;

                    try
                    {
                        //sit = ((PwEntry)lvi.Tag).Strings.Get(str).ReadString();
                        //sit = KPELVUtil.StringToOneLine(sit, i);
                        sit = ((PwEntry)lvi.Tag).Strings.Get(chd.Text).ReadString();
                        sit = Util.StringToOneLine(sit, chd.Index);
                    }
                    catch (NullReferenceException)
                    {
                        sit = string.Empty;
                    }

                   // if (lvi.SubItems.Count == i)
                    {
                        // Subitem has not been added to the listview - add subitem
                        lvi.SubItems.Add(sit);
                    }
                    /*
                    else if (lvi.SubItems.Count < i)
                    {
                        // There are fewer items as expected
                        // This should never happen
                        do
                        {
                            lvi.SubItems.Add(string.Empty);
                        }
                        while (lvi.SubItems.Count < i);
                        
                        Debug.Assert(true, "Missing subitems");
                     
                        //empty = new String[i - lvi.SubItems.Count];
                        //for (int e = 0; e < empty.Length; e++) { empty[e] = string.Empty; };
                        //lvi.SubItems.AddRange(empty);

                        lvi.SubItems.Add(sit);
                    }
                    else
                    {
                        // Subitem already added to listview - replace item
                        //continue;
                        lvi.SubItems[i].Text = sit;
                    }*/

                    //if (Array.IndexOf(vSelected, lvi.Tag) >= 0) lvi.Selected = true;
                }
            }

            SortListView();
            
            //m_clveEntries.EndUpdate();
            //m_clveEntries.Update();

            return;
        }

        private void SortListView()
        {
            ListSorter ls = (ListSorter)m_clveEntries.ListViewItemSorter;
            if (ls != null)
            {
                if (ls.Column >= (int)AppDefs.ColumnId.Count)
                {
                    if (ls.Order != SortOrder.None)
                    {
                        m_clveEntries.ListViewItemSorter = new ListSorter(ls.Column, ls.Order, true, false);

                        m_clveEntries.Sort();

                        UIUtil.SetAlternatingBgColors(m_clveEntries, UIUtil.GetAlternateColor(m_clveEntries.BackColor), Program.Config.MainWindow.EntryListAlternatingBgColors);
                    }
                }
            }
        }
    }
}
