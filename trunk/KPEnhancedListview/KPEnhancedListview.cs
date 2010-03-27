/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
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
#if !USE_NET20
  using System.Linq;
#endif

using KeePass;
using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Plugins;
using KeePass.Forms;
using KeePass.UI;
using KeePass.Util;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Utility;

// The namespace name must be the same as the filename of the
// plugin without its extension.
namespace KPEnhancedListview
{
    /// <summary>
    /// This is the main plugin class. It must be named exactly
    /// like the namespace and must be derived from
    /// <c>KeePassPlugin</c>.
    /// </summary>
    public sealed class KPEnhancedListviewExt : Plugin
    {
        // The plugin remembers its host in this variable.
        private IPluginHost m_host = null;

        private ToolStripSeparator m_tsSeparator = null;
        private ToolStripMenuItem m_tsmiInlineEditing = null;
        private ToolStripMenuItem m_tsmiAddCustomColumns = null;
        private ToolStripMenuItem m_tsmiRemoveCustomColumns = null;

        private const string m_ctseName = "m_toolMain";
        private const string m_clveName = "m_lvEntries";
        private const string m_ctveName = "m_tvGroups";
        private const string m_csceName = "m_splitVertical";

        private CustomToolStripEx m_ctseToolMain = null;
        private CustomListViewEx m_clveEntries = null;
        private CustomTreeViewEx m_ctveGroups = null;
        private CustomSplitContainerEx m_csceSplitVertical = null;

        private EventSuppressor m_evEntries = null;
        private EventSuppressor m_evMainWindow = null;

        private List<ColumnHeader> m_lCustomColums = null;

        // Mouse handler helper
        private const int m_mouseTimeMin = 3000000;
        private const int m_mouseTimeMax = 10000000;
        private DateTime m_mouseDownAt = DateTime.MinValue;
        private ListViewItem m_previousClickedListViewItem = null;

        // ListView send messages
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wPar, IntPtr lPar);
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int len, ref	int[] order);

        [DllImport("user32.dll", EntryPoint = "LockWindowUpdate", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern long LockWindow(long Handle);
        // Lock listview - prevent scolling
        //LockWindow(m_clveEntries.Handle.ToInt64());
        // Unlock listview
        //LockWindow(0);

        // ListView messages
        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETCOLUMNORDERARRAY = (LVM_FIRST + 59);

        //TODO
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const long LVM_GETHEADER = (LVM_FIRST + 31);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessage(IntPtr hwnd, long wMsg, long wParam, long lParam);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hwnd, out RECT lpRect);
        //END


        // InlineEditing control
        private PaddedTextBox m_textBoxComment;
        private int m_headerBottom = 0;

        // The control performing the actual editing
        private PaddedTextBox _editingControl;
        // The LVI being edited
        private ListViewItem _editItem;
        // The SubItem being edited
        //TODO private ListViewItem.ListViewSubItem _editSubItem;
        private int _editSubItem;

        private static Mutex mutEdit = new Mutex();

        /// <summary>
        /// The <c>Initialize</c> function is called by KeePass when
        /// you should initialize your plugin (create menu items, etc.).
        /// </summary>
        /// <param name="host">Plugin host interface. By using this
        /// interface, you can access the KeePass main window and the
        /// currently opened database.</param>
        /// <returns>You must return <c>true</c> in order to signal
        /// successful initialization. If you return <c>false</c>,
        /// KeePass unloads your plugin (without calling the
        /// <c>Terminate</c> function of your plugin).</returns>
        public override bool Initialize(IPluginHost host)
        {
            Debug.Assert(host != null);
            if (host == null) return false;
            m_host = host;

            // Get a reference to the 'Tools' menu item container
            ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;

            // Add a separator at the bottom
            m_tsSeparator = new ToolStripSeparator();
            tsMenu.Add(m_tsSeparator);

            // Add menu item
            m_tsmiInlineEditing = new ToolStripMenuItem();
            m_tsmiInlineEditing.Text = "Inline Editing";
            m_tsmiInlineEditing.Click += OnMenuInlineEditing;
            tsMenu.Add(m_tsmiInlineEditing);

            // Add menu item
            m_tsmiAddCustomColumns = new ToolStripMenuItem();
            m_tsmiAddCustomColumns.Text = "Add Custom Columns";
            m_tsmiAddCustomColumns.Click += OnMenuAddCustomColumns;
            tsMenu.Add(m_tsmiAddCustomColumns);

            // Add menu item
            m_tsmiRemoveCustomColumns = new ToolStripMenuItem();
            m_tsmiRemoveCustomColumns.Text = "Remove Custom Columns";
            m_tsmiRemoveCustomColumns.Click += OnMenuRemoveCustomColumns;
            tsMenu.Add(m_tsmiRemoveCustomColumns);

            // We want a notification when the user tried to save the current database
            m_host.MainWindow.FileSaved += OnFileSaved;

            // Find the listview control 
            m_clveEntries = (CustomListViewEx)FindControlRecursive(m_host.MainWindow, m_clveName);
            m_ctveGroups = (CustomTreeViewEx)FindControlRecursive(m_host.MainWindow, m_ctveName);
            m_ctseToolMain = (CustomToolStripEx)FindControlRecursive(m_host.MainWindow, m_ctseName);
            m_csceSplitVertical = (CustomSplitContainerEx)FindControlRecursive(m_host.MainWindow, m_csceName);

            // Initialize EventSuppressor
            m_evEntries = new EventSuppressor(m_clveEntries);
            m_evMainWindow = new EventSuppressor(m_host.MainWindow); //(Control)this);//(Control)m_host.MainWindow);

            // Initialize Inline Editing
            m_textBoxComment = new PaddedTextBox();
            m_textBoxComment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_textBoxComment.Location = new System.Drawing.Point(32, 104);
            m_textBoxComment.Name = "m_textBoxComment";
            m_textBoxComment.Size = new System.Drawing.Size(80, 16);
            m_textBoxComment.TabIndex = 3;
            m_textBoxComment.Text = "";
            m_textBoxComment.AutoSize = false;
            m_textBoxComment.Padding = new Padding(6, 1, 1, 0);
            m_textBoxComment.Visible = false;

            m_clveEntries.Controls.Add(m_textBoxComment);    // Add control to listview


            //TODO
            /*
            RECT rc = new RECT();
            IntPtr hwnd = SendMessage(m_clveEntries.Handle, LVM_GETHEADER, 0, 0);
            if (hwnd != null)
            {
                if (GetWindowRect(new HandleRef(null, hwnd), out rc))
                {
                    headerHeight = rc.Bottom - rc.Top;
                }
            }*/



            // Initialize Custom columns
            m_lCustomColums = new List<ColumnHeader>();

            m_evEntries.Suppress("OnPwListColumnWidthChanging");
            m_evEntries.Suppress("OnPwListColumnWidthChanged");

            m_clveEntries.ColumnWidthChanging += new ColumnWidthChangingEventHandler(this.OnPwListCustomColumnWidthChanging);
            m_clveEntries.ColumnWidthChanged += new ColumnWidthChangedEventHandler(this.OnPwListCustomColumnWidthChanged);

            //TODO sorting is not working after leaving pwentry form
            //m_evEntries.Suppress("OnPwListColumnClick");
            m_clveEntries.ColumnClick += new ColumnClickEventHandler(this.OnPwListCustomColumnClick);
            //m_evEntries.Resume("OnPwListColumnClick");
            m_host.MainWindow.UIStateUpdated += new EventHandler(this.OnPwListCustomColumnUpdate);

            m_clveEntries.MouseDoubleClick += new MouseEventHandler(this.OnItemDoubleClick);

            // Tell windows we are interested in drawing items in ListBox on our own
            m_clveEntries.OwnerDraw = true;
            m_clveEntries.DrawItem += new DrawListViewItemEventHandler(this.DrawItemHandler);
            m_clveEntries.DrawSubItem += new DrawListViewSubItemEventHandler(this.DrawSubItemHandler);
            m_clveEntries.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(this.DrawColumnHeaderHandler);

            return true; // Initialization successful
        }

        /// <summary>
        /// Finds a Control recursively. Note finds the first match and exists
        /// </summary>
        /// <param name="container">The container to search for the control passed. Remember
        /// all controls (Panel, GroupBox, Form, etc are all containsers for controls
        /// </param>
        /// <param name="name">Name of the control to look for</param>
        /// <returns>The Control we found</returns>
        public Control FindControlRecursive(Control container, string name)
        {
            if (container.Name == name)
                return container;

            foreach (Control ctrl in container.Controls)
            {
                Control foundCtrl = FindControlRecursive(ctrl, name);
                if (foundCtrl != null)
                {
                    return foundCtrl;
                }
            }
            return null;
        }

        private void OnFileSaved(object sender, FileSavedEventArgs e)
        {
            // nothing todo
            //m_evMainWindow.Suppress("OnFileSaved");
            //MessageBox.Show("ofs");
        }

        /// <summary>
        /// The <c>Terminate</c> function is called by KeePass when
        /// you should free all resources, close open files/streams,
        /// etc. It is also recommended that you remove all your
        /// plugin menu items from the KeePass menu.
        /// </summary>
        public override void Terminate()
        {
            // Remove all of our menu items
            ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;
            tsMenu.Remove(m_tsSeparator);
            tsMenu.Remove(m_tsmiInlineEditing);
            tsMenu.Remove(m_tsmiAddCustomColumns);
            tsMenu.Remove(m_tsmiRemoveCustomColumns);

            // Important! Remove event handlers!
            m_host.MainWindow.FileSaved -= OnFileSaved;

            // Undo Initialize
            m_evEntries.Resume("OnPwListColumnWidthChanging");
            m_evEntries.Resume("OnPwListColumnWidthChanged");

            m_clveEntries.ColumnWidthChanging -= new ColumnWidthChangingEventHandler(this.OnPwListCustomColumnWidthChanging);
            m_clveEntries.ColumnWidthChanged -= new ColumnWidthChangedEventHandler(this.OnPwListCustomColumnWidthChanged);

            m_clveEntries.ColumnClick -= new ColumnClickEventHandler(this.OnPwListCustomColumnClick);
            m_host.MainWindow.UIStateUpdated -= new EventHandler(this.OnPwListCustomColumnUpdate);

            m_clveEntries.MouseDoubleClick -= new MouseEventHandler(this.OnItemDoubleClick);

            m_clveEntries.DrawItem -= new DrawListViewItemEventHandler(this.DrawItemHandler);
            m_clveEntries.DrawSubItem -= new DrawListViewSubItemEventHandler(this.DrawSubItemHandler);
            m_clveEntries.DrawColumnHeader -= new DrawListViewColumnHeaderEventHandler(this.DrawColumnHeaderHandler);

            // Undo OnMenuInlineEditing
            m_clveEntries.KeyDown -= new KeyEventHandler(this.OnItemKeyDown);
            m_clveEntries.MouseUp -= new MouseEventHandler(this.OnItemMouseUp);

            m_clveEntries.MouseDown -= new MouseEventHandler(this.OnItemCancel);
            m_clveEntries.MouseCaptureChanged -= new EventHandler(this.OnItemCancel);

            m_clveEntries.Resize -= new EventHandler(this.OnItemCancel);
            m_clveEntries.Invalidated -= new InvalidateEventHandler(this.OnItemCancel);
            m_clveEntries.ColumnReordered -= new ColumnReorderedEventHandler(this.OnItemCancel); // Todo change position of textbox and redraw

            m_host.MainWindow.Move -= new EventHandler(this.OnItemCancel);
            m_host.MainWindow.Resize -= new EventHandler(this.OnItemCancel);
            m_host.MainWindow.MouseDown -= new MouseEventHandler(this.OnItemCancel);
            m_host.MainWindow.MouseCaptureChanged -= new EventHandler(this.OnItemCancel);

            m_host.MainWindow.MainMenu.MouseDown -= new MouseEventHandler(this.OnItemCancel);
            m_host.MainWindow.MainMenu.MenuActivate -= new EventHandler(this.OnItemCancel);

            m_ctseToolMain.MouseDown -= new MouseEventHandler(this.OnItemCancel);
            m_ctseToolMain.ItemClicked -= new ToolStripItemClickedEventHandler(this.OnItemCancel);
        }

        private void DrawItemHandler(object sender, DrawListViewItemEventArgs e)
        {
            // CustomColumns
            //TODO combine all additem algorithm to one function
            foreach (ColumnHeader chd in m_lCustomColums)
            {
                int i = chd.Index;
                string str = chd.Text;

                // Check subitems
                if (e.Item.SubItems.Count > i)
                {
                    break;
                }
                else
                {
                    PwEntry pe = (PwEntry)e.Item.Tag;
                    if (pe.Strings.Exists(str))
                    {
                        string sit = pe.Strings.Get(str).ReadString();
                        e.Item.SubItems.Add((new ListViewItem.ListViewSubItem()).Text = sit);
                    }
                    else
                    {
                        e.Item.SubItems.Add((new ListViewItem.ListViewSubItem()).Text = "");
                    }
                }
            }

            // Inline Editing
            if (_editingControl != null)
            {
                // TODO is ther an alternative
                //if(e.Item.Tag.Equals(_editItem.Tag))
                // Check if item is visible - below ColumnHeader
                //if (m_clveEntries.TopItem.Bounds.Top <= m_clveEntries.Items[_editItem.Index].Bounds.Top)
                //if (m_clveEntries.PointToClient(new Point(0, headerHeight)).Y <= m_clveEntries.Items[_editItem.Index].Bounds.Top)
                //if ( ( m_clveEntries.TopItem.Bounds.Top <= m_clveEntries.Items[_editItem.Index].Bounds.Top ) && ( m_clveEntries.PointToClient(new Point(0, headerHeight)).Y <= m_clveEntries.Items[_editItem.Index].Bounds.Top ) )
                //Point pt = new Point(0, m_clveEntries.ClientRectangle.Top); pt.Y += headerHeight;
                //if (m_clveEntries.PointToClient(pt).Y <= m_clveEntries.Items[_editItem.Index].Bounds.Top)
                //if ((e.Bounds.Top+headerHeight) <= m_clveEntries.Items[_editItem.Index].Bounds.Top)               
                if (m_headerBottom <= m_clveEntries.Items[_editItem.Index].Bounds.Top)
                {
                    //_editingControl.Text = m_clveEntries.TopItem.Bounds.Top.ToString();
                    SetEditBox(_editingControl, GetSubItemBounds(m_clveEntries.Items[_editItem.Index], _editSubItem), _editSubItem);
                }
                else
                {
                    // Workaround to avoid Editbox is drawn on ColumnHeader
                    Rectangle rc = GetSubItemBounds(m_clveEntries.Items[_editItem.Index], _editSubItem);
                    rc.Height = 0;
                    SetEditBox(_editingControl, rc, _editSubItem);
                }
            }

            e.DrawDefault = true;
        }

        private void DrawSubItemHandler(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void DrawColumnHeaderHandler(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            //headerHeight = e.Header.Height;
            m_headerBottom = e.Bounds.Bottom;

            e.DrawDefault = true;
        }

        private void OnMenuInlineEditing(object sender, EventArgs e)
        {
            if (!m_host.Database.IsOpen)
            {
                // doesn't matter
            }

            m_tsmiInlineEditing.Checked = !m_tsmiInlineEditing.Checked;

            if (m_tsmiInlineEditing.Checked)
            {
                m_clveEntries.FullRowSelect = true;
                m_clveEntries.View = View.Details;
                m_clveEntries.AllowColumnReorder = true;

                m_clveEntries.KeyDown += new KeyEventHandler(this.OnItemKeyDown);
                m_clveEntries.MouseUp += new MouseEventHandler(this.OnItemMouseUp);

                m_clveEntries.MouseDown += new MouseEventHandler(this.OnItemCancel);
                m_clveEntries.MouseCaptureChanged += new EventHandler(this.OnItemCancel);

                m_clveEntries.Resize += new EventHandler(this.OnItemCancel);
                m_clveEntries.Invalidated += new InvalidateEventHandler(this.OnItemCancel);
                m_clveEntries.ColumnReordered += new ColumnReorderedEventHandler(this.OnItemCancel);

                m_host.MainWindow.Move += new EventHandler(this.OnItemCancel);
                m_host.MainWindow.Resize += new EventHandler(this.OnItemCancel);
                m_host.MainWindow.MouseDown += new MouseEventHandler(this.OnItemCancel);
                m_host.MainWindow.MouseCaptureChanged += new EventHandler(this.OnItemCancel);

                m_host.MainWindow.MainMenu.MouseDown += new MouseEventHandler(this.OnItemCancel);
                m_host.MainWindow.MainMenu.MenuActivate += new EventHandler(this.OnItemCancel);

                m_ctseToolMain.MouseDown += new MouseEventHandler(this.OnItemCancel);
                m_ctseToolMain.ItemClicked += new ToolStripItemClickedEventHandler(this.OnItemCancel);
            }
            else
            {
                // disable plugin function
                m_clveEntries.KeyDown -= new KeyEventHandler(this.OnItemKeyDown);
                m_clveEntries.MouseUp -= new MouseEventHandler(this.OnItemMouseUp);

                m_clveEntries.MouseDown -= new MouseEventHandler(this.OnItemCancel);
                m_clveEntries.MouseCaptureChanged -= new EventHandler(this.OnItemCancel);

                m_clveEntries.Resize -= new EventHandler(this.OnItemCancel);
                m_clveEntries.Invalidated -= new InvalidateEventHandler(this.OnItemCancel);
                m_clveEntries.ColumnReordered -= new ColumnReorderedEventHandler(this.OnItemCancel); // Todo change position of textbox and redraw

                m_host.MainWindow.Move -= new EventHandler(this.OnItemCancel);
                m_host.MainWindow.Resize -= new EventHandler(this.OnItemCancel);
                m_host.MainWindow.MouseDown -= new MouseEventHandler(this.OnItemCancel);
                m_host.MainWindow.MouseCaptureChanged -= new EventHandler(this.OnItemCancel);

                m_host.MainWindow.MainMenu.MouseDown -= new MouseEventHandler(this.OnItemCancel);
                m_host.MainWindow.MainMenu.MenuActivate -= new EventHandler(this.OnItemCancel);

                m_ctseToolMain.MouseDown -= new MouseEventHandler(this.OnItemCancel);
                m_ctseToolMain.ItemClicked -= new ToolStripItemClickedEventHandler(this.OnItemCancel);
            }
        }

        private void OnItemKeyDown(object sender, System.Windows.Forms.KeyEventArgs k)
        {
            if (k.KeyCode == Keys.F3)
            {
                // edit selected item
                if (m_clveEntries.SelectedIndices.Count > 0)
                {
                    ListViewItem Item = m_clveEntries.SelectedItems[0];
                    if (Item != null)
                    {
                        StartEditing(m_textBoxComment, Item, 0);
                    }
                }
            }
        }

        private void OnItemMouseUp(object sender, MouseEventArgs e)
        {
            ListViewItem item;
            int idx = GetSubItemAt(e.X, e.Y, out item);
            if (item != null)
            {
                if ((m_previousClickedListViewItem != null) && (item == m_previousClickedListViewItem) && (e.Button == MouseButtons.Left))
                {
                    long datNow = DateTime.Now.Ticks;
                    long datMouseDown = m_mouseDownAt.Ticks;
                    if ((datNow - datMouseDown > m_mouseTimeMin) && (datNow - datMouseDown < m_mouseTimeMax))
                    {
                        Point pt = m_clveEntries.PointToClient(Cursor.Position);
                        EditSubitemAt(pt);
                    }
                }
                m_mouseDownAt = DateTime.Now;
            }
            m_previousClickedListViewItem = item;
        }

        private void OnItemCancel(object sender, EventArgs e)
        {
            if (_editingControl != null)
            {
                // Close and cancel
                EndEditing(false);
            }
        }

        private void OnItemDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item;
            int subitem = GetSubItemAt(e.X, e.Y, out item);

            if (subitem > (int)AppDefs.ColumnId.Uuid)
            {
                // Copy CustomColumn subitem text to clipboard
                bool bCnt = ClipboardUtil.CopyAndMinimize(item.SubItems[subitem].Text, false, m_host.MainWindow, null, null);
                if (bCnt) m_host.MainWindow.StartClipboardCountdown();
            }
        }

        ///<summary>
        /// Fire SubItemClicked
        ///</summary>
        ///<param name="p">Point of click/doubleclick</param>
        private void EditSubitemAt(Point p)
        {
            ListViewItem item;
            int subitem = GetSubItemAt(p.X, p.Y, out item);

            if (subitem >= 0)
            {
                //TODO avoid editing of 
                /*
                case AppDefs.ColumnId.CreationTime:
				case AppDefs.ColumnId.LastAccessTime:
				case AppDefs.ColumnId.LastModificationTime:
				case AppDefs.ColumnId.ExpiryTime:
				case AppDefs.ColumnId.Attachment:
				case AppDefs.ColumnId.Uuid:
                 */
                /*
                foreach(int i in m_clveEntries.)
                switch ((AppDefs.ColumnId)subitem)
                {
                    case AppDefs.ColumnId.CreationTime:
                    case AppDefs.ColumnId.LastAccessTime:
                    case AppDefs.ColumnId.LastModificationTime:
                    case AppDefs.ColumnId.ExpiryTime:
                    case AppDefs.ColumnId.Attachment:
                    case AppDefs.ColumnId.Uuid:
                        // No editing allowed
                        subitem = GetNextSubItemFor(subitem);
                    default:
                        StartEditing(m_textBoxComment, item, subitem);
                        break;
                }*/

                //StartEditing(Editors[e.SubItem], e.Item, e.SubItem);
                StartEditing(m_textBoxComment, item, subitem);
            }
        }

        /// <summary>
        /// Find ListViewItem and SubItem Index at position (x,y)
        /// </summary>
        /// <param name="x">relative to ListView</param>
        /// <param name="y">relative to ListView</param>
        /// <param name="item">Item at position (x,y)</param>
        /// <returns>SubItem index</returns>
        public int GetSubItemAt(int x, int y, out ListViewItem item)
        {
            item = m_clveEntries.GetItemAt(x, y);

            if (item != null)
            {
                int[] order = GetColumnOrder();
                Rectangle lviBounds;
                int subItemX;

                lviBounds = item.GetBounds(ItemBoundsPortion.Entire);
                subItemX = lviBounds.Left;
                for (int i = 0; i < order.Length; i++)
                {
                    ColumnHeader h = m_clveEntries.Columns[order[i]];
                    if (x < subItemX + h.Width)
                    {
                        return h.Index;
                    }
                    subItemX += h.Width;
                }
            }

            return -1;
        }

        /// <summary>
        /// Find Next ListView SubItem
        /// </summary>
        /// <param name="SubItem">SubItem to find next for</param>
        /// <returns>SubItem index</returns>
        public int GetNextSubItemFor(int SubItem)
        {
            int[] order = GetColumnOrder();

            int nextSubItem = 0;

            for (int i = m_clveEntries.Columns[SubItem].DisplayIndex; i < order.Length - 1; i++)
            {
                if (order[i] == SubItem)
                {
                    if (m_clveEntries.Columns[order[i + 1]].Width > 0)
                    {
                        nextSubItem = i + 1;
                        break;
                    }
                    else
                    {

                        SubItem = order[i + 1];
                    }
                }
            }

            return order[nextSubItem];
        }

        /// <summary>
        /// Retrieve the order in which columns appear
        /// </summary>
        /// <returns>Current display order of column indices</returns>
        public int[] GetColumnOrder()
        {
            IntPtr lPar = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * m_clveEntries.Columns.Count);

            IntPtr res = SendMessage(m_clveEntries.Handle, LVM_GETCOLUMNORDERARRAY, new IntPtr(m_clveEntries.Columns.Count), lPar);
            if (res.ToInt32() == 0)	// Something went wrong
            {
                Marshal.FreeHGlobal(lPar);
                return null;
            }

            int[] order = new int[m_clveEntries.Columns.Count];
            Marshal.Copy(lPar, order, 0, m_clveEntries.Columns.Count);
            Marshal.FreeHGlobal(lPar);

            return order;
        }

        /// <summary>
        /// Get bounds for a SubItem
        /// </summary>
        /// <param name="Item">Target ListViewItem</param>
        /// <param name="SubItem">Target SubItem index</param>
        /// <returns>Bounds of SubItem (relative to ListView)</returns>
        public Rectangle GetSubItemBounds(ListViewItem Item, int SubItem)
        {
            int[] order = GetColumnOrder();

            Rectangle subItemRect = Rectangle.Empty;
            if (SubItem >= order.Length)
                throw new IndexOutOfRangeException("SubItem " + SubItem + " out of range");

            if (Item == null)
                throw new ArgumentNullException("Item");

            Rectangle lviBounds = Item.GetBounds(ItemBoundsPortion.Entire);
            int subItemX = lviBounds.Left;
            switch (m_clveEntries.BorderStyle)
            {
                case BorderStyle.Fixed3D:
                    subItemX += SystemInformation.Border3DSize.Width;
                    break;
                case BorderStyle.FixedSingle:
                    subItemX += SystemInformation.BorderSize.Width;
                    break;
            }

            ColumnHeader col;
            int i;
            for (i = 0; i < order.Length; i++)
            {
                col = m_clveEntries.Columns[order[i]];
                if (col.Index == SubItem)
                    break;
                subItemX += col.Width;
            }

            if (m_clveEntries.RightToLeftLayout)
            {
                subItemX = m_clveEntries.Width - subItemX - m_clveEntries.Columns[order[i]].Width;
            }

            // Adapt the bounds
            subItemX = subItemX - 1;
            int subItemT = lviBounds.Top + 2;
            int subItemW = m_clveEntries.Columns[order[i]].Width + 1;
            int subItemH = lviBounds.Height;

            if (SubItem == 0)
            {
                // Text indention because of the entry icon
                subItemX += 21;
                subItemW -= 21;
            }

            subItemRect = new Rectangle(subItemX, subItemT, subItemW, subItemH);
            return subItemRect;
        }

        /// <summary>
        /// Begin in-place editing of given cell
        /// </summary>
        /// <param name="c">Control used as cell editor</param>
        /// <param name="Item">ListViewItem to edit</param>
        /// <param name="SubItem">SubItem index to edit</param>
        public void StartEditing(PaddedTextBox c, ListViewItem Item, int SubItem)
        {
            mutEdit.WaitOne();

            if (_editingControl != null)
            {
                mutEdit.ReleaseMutex();
                return;
            }

            // Remove/Suppress UIStateUpdated Event Handler
            // UIStateUpdated is called on entering InlineEditing 
            //TODO m_evMainWindow Suppress not working
            //m_evMainWindow.Suppress("OnPwListCustomColumnUpdate");
            m_host.MainWindow.UIStateUpdated -= new EventHandler(OnPwListCustomColumnUpdate);

            m_clveEntries.EnsureVisible(Item.Index);

            // Set MultiLine property
            switch ((AppDefs.ColumnId)SubItem)
            {
                case AppDefs.ColumnId.Title:
                case AppDefs.ColumnId.UserName:
                case AppDefs.ColumnId.Password:
                case AppDefs.ColumnId.Url:
                case AppDefs.ColumnId.CreationTime:
                case AppDefs.ColumnId.LastAccessTime:
                case AppDefs.ColumnId.LastModificationTime:
                case AppDefs.ColumnId.ExpiryTime:
                case AppDefs.ColumnId.Attachment:
                case AppDefs.ColumnId.Uuid:
                    c.Multiline = false;
                    //c.LineSpacing(20); // Single spacing
                    break;
                case AppDefs.ColumnId.Notes:
                default:
                    c.Multiline = true;
                    //c.LineSpacing(27); //26 ok); // Spacing equal listview item height
                    break;
            }

            // Set editing allowed
            switch ((AppDefs.ColumnId)SubItem)
            {
                case AppDefs.ColumnId.CreationTime:
                case AppDefs.ColumnId.LastAccessTime:
                case AppDefs.ColumnId.LastModificationTime:
                case AppDefs.ColumnId.ExpiryTime:
                case AppDefs.ColumnId.Attachment:
                case AppDefs.ColumnId.Uuid:
                    // No editing allowed
                    c.ReadOnly = true;
                    break;
                default:
                    // Editing allowed
                    c.ReadOnly = false;
                    break;
            }

            // Read SubItem text and set textbox property
            c.Text = StringNormalizeToMultiLine(ReadEntry(Item, SubItem), SubItem);
            c.ScrollToTop();
            c.SelectAll();

            // Set control location, bounding, padding
            SetEditBox(c, GetSubItemBounds(Item, SubItem), SubItem);

            c.Visible = true;
            c.BringToFront();
            c.Select();
            c.Focus();

            _editingControl = c;
            _editingControl.Leave += new EventHandler(_editControl_Leave);
            _editingControl.LostFocus += new EventHandler(_editControl_LostFocus);
            _editingControl.KeyPress += new KeyPressEventHandler(_editControl_KeyPress);

            _editItem = Item;
            _editSubItem = SubItem;

            mutEdit.ReleaseMutex();
        }

        private void SetEditBox(PaddedTextBox c, Rectangle rcSubItem, int SubItem)
        {
            if (rcSubItem.X < 0)
            {
                // Left edge of SubItem not visible - adjust rectangle position and width
                rcSubItem.Width += rcSubItem.X;
                rcSubItem.X = 0;
            }

            if (rcSubItem.X + rcSubItem.Width > m_clveEntries.ClientRectangle.Width)
            {
                // Right edge of SubItem not visible - adjust rectangle width
                rcSubItem.Width = m_clveEntries.ClientRectangle.Width - rcSubItem.Left;
            }

            // Calculate editbox height
#if USE_NET20
            if (c.Lines.Length > 1)
#else
              if (c.Lines.Count() > 1)
#endif
            {
                // For linespacing equal listview
                //rcSubItem.Height *= c.Lines.Count();

                // For linespacing equal singlespacing
                //rcSubItem.Height = rcSubItem.Height + c.Lines.Count() * /*~*/ 8;

                // Always display 3 lines
                rcSubItem.Height *= 3;

                c.ScrollBars(true);
            }
            else
            {
                c.ScrollBars(false);
            }

            // Subitem bounds are relative to the location of the ListView!
            rcSubItem.Offset(m_clveEntries.Left, m_clveEntries.Top);

            // In case the editing control and the listview are on different parents,
            // account for different origins
            Point origin = new Point(0, 0);
            Point lvOrigin = m_clveEntries.Parent.PointToScreen(origin);
            Point ctlOrigin = c.Parent.PointToScreen(origin);

            rcSubItem.Offset(lvOrigin.X - ctlOrigin.X, lvOrigin.Y - ctlOrigin.Y);

            // Padding
            Padding pdSubItem = new Padding(6, 1, 1, 0);
            if (SubItem == 0)
            {
                pdSubItem.Left = 1;
            }
#if USE_RTB
            else if (c.Multiline)
            {
                pdSubItem.Left = 5;
            }
#endif

            // Position, padding and show editor
            if (!c.Bounds.Equals(rcSubItem))
                c.Bounds = rcSubItem;

            if (!c.Padding.Equals(pdSubItem))
                c.Padding = pdSubItem;
        }

        private string ReadEntry(ListViewItem Item, int SubItem)
        {
            string str = null;

            PwEntry pe = (PwEntry)Item.Tag;
            pe = m_host.Database.RootGroup.FindEntry(pe.Uuid, true);

            if (pe == null)
            {
                return "";
            }

            switch ((AppDefs.ColumnId)SubItem)
            {
                case AppDefs.ColumnId.Title:
                    //if(PwDefs.IsTanEntry(pe))
                    //TODO tan list	 TanTitle ???		    pe.Strings.Set(PwDefs.TanTitle, new ProtectedString(false, Text));
                    //else
                    str = pe.Strings.Get(PwDefs.TitleField).ReadString();
                    break;
                case AppDefs.ColumnId.UserName:
                    str = pe.Strings.Get(PwDefs.UserNameField).ReadString();
                    break;
                case AppDefs.ColumnId.Password:
                    str = pe.Strings.Get(PwDefs.PasswordField).ReadString();
                    break;
                case AppDefs.ColumnId.Url:
                    str = pe.Strings.Get(PwDefs.UrlField).ReadString();
                    break;
                case AppDefs.ColumnId.Notes:
                    str = pe.Strings.Get(PwDefs.NotesField).ReadString();
                    break;
                case AppDefs.ColumnId.CreationTime:
                    str = TimeUtil.ToDisplayString(pe.CreationTime);
                    break;
                case AppDefs.ColumnId.LastAccessTime:
                    str = TimeUtil.ToDisplayString(pe.LastAccessTime);
                    break;
                case AppDefs.ColumnId.LastModificationTime:
                    str = TimeUtil.ToDisplayString(pe.LastModificationTime);
                    break;
                case AppDefs.ColumnId.ExpiryTime:
                    str = TimeUtil.ToDisplayString(pe.ExpiryTime);
                    break;
                case AppDefs.ColumnId.Attachment:
                    str = pe.Binaries.KeysToString();
                    break;
                case AppDefs.ColumnId.Uuid:
                    str = pe.Uuid.ToHexString();
                    break;
                default:
                    if (pe.Strings.Exists(m_clveEntries.Columns[SubItem].Text))
                    {
                        str = pe.Strings.Get(m_clveEntries.Columns[SubItem].Text).ReadString();
                    }
                    else
                    {
                        // SubItem does not exist
                        str = Item.SubItems[SubItem].Text;
                    }

                    break;
            }
            return str;
        }

        public void _editControl_Leave(object sender, EventArgs e)
        {
            // input edit leaves cell editor focus
            // not necessary - see _editControl_LostFocus
        }

        public void _editControl_LostFocus(object sender, EventArgs e)
        {
            // cell editor losing focus
            if (m_clveEntries.Focused)
            {
                // list view gets focus - save
                EndEditing(true);
            }
            else
            {
                // focus is gone away - cancel
                EndEditing(false);
            }
        }

        public void _editControl_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)(int)Keys.Escape:
                    {
                        // Close and cancel
                        EndEditing(false);
                        break;
                    }

                case (char)(int)Keys.Enter:
                    {
                        // Close and save
                        EndEditing(true);
                        break;
                    }

                case (char)(int)Keys.Tab:
                    {
                        //TODO add new function 
                        // Save and edit next SubItem
                        ListViewItem Item = _editItem;
                        int SubItem = GetNextSubItemFor(_editSubItem);

                        EndEditing(true);
                        StartEditing(m_textBoxComment, Item, SubItem);

                        break;
                    }
                case (char)(int)Keys.ControlKey:
                    {
                        MessageBox.Show("Ck");
                        break;
                    }
                //TODO cusrsor keys not working
                case (char)(int)Keys.Right:
                    {
                        MessageBox.Show("R");
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            MessageBox.Show("1");
                            // Save and edit next SubItem
                            ListViewItem Item = _editItem;
                            int SubItem = GetNextSubItemFor(_editSubItem);

                            EndEditing(true);

                            StartEditing(m_textBoxComment, Item, SubItem);
                        }

                        if (Control.ModifierKeys == Keys.ControlKey)
                        {
                            MessageBox.Show("2");
                            // Save and edit next SubItem
                            ListViewItem Item = _editItem;
                            int SubItem = GetNextSubItemFor(_editSubItem);

                            EndEditing(true);

                            StartEditing(m_textBoxComment, Item, SubItem);
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Accept or discard current value of cell editor control
        /// </summary>
        /// <param name="AcceptChanges">Use the _editingControl's Text as new SubItem text or discard changes?</param>
        public void EndEditing(bool AcceptChanges)
        {
            mutEdit.WaitOne();

            if (_editingControl == null)
            {
                mutEdit.ReleaseMutex();
                return;
            }

            if (AcceptChanges == true)
            {
                // Check if item and textbox contain different text
                if (_editItem.SubItems[_editSubItem].Text != _editingControl.Text)
                {
                    // Save changes
                    SaveEntry(_editItem, _editSubItem, _editingControl.Text);

                    // If Item is protected
                    if (_editItem.SubItems[_editSubItem].Text.Equals(PwDefs.HiddenPassword))
                    {
                        // Set Text to hidden
                        _editingControl.Text = PwDefs.HiddenPassword;
                    }

                    // Updating the listview item
                    _editItem.SubItems[_editSubItem].Text = StringNormalizeToOneLine(_editingControl.Text, _editSubItem);

                    m_clveEntries.EnsureVisible(_editItem.Index);
                }
                else
                {
                    // No changes detected
                    AcceptChanges = false;
                }
            }
            else
            {
                // Cancel 
                // Nothing todo
            }

            _editingControl.Leave -= new EventHandler(_editControl_Leave);
            _editingControl.LostFocus -= new EventHandler(_editControl_LostFocus);
            _editingControl.KeyPress -= new KeyPressEventHandler(_editControl_KeyPress);

            _editingControl.Visible = false;

            _editingControl = null;
            _editItem = null;
            _editSubItem = -1;

            m_clveEntries.Update();
            m_clveEntries.Select();

            // Add/Resume UIStateUpdated Event Handler
            //m_evMainWindow.Resume("OnPwListCustomColumnUpdate");
            m_host.MainWindow.UIStateUpdated += new EventHandler(OnPwListCustomColumnUpdate);

            mutEdit.ReleaseMutex();
        }

        // Normalize Multiline String to OneLine 
        private string StringNormalizeToOneLine(string Text, int SubItem)
        {
            switch ((AppDefs.ColumnId)SubItem)
            {
                case AppDefs.ColumnId.Title:
                case AppDefs.ColumnId.UserName:
                case AppDefs.ColumnId.Password:
                case AppDefs.ColumnId.Url:
                case AppDefs.ColumnId.CreationTime:
                case AppDefs.ColumnId.LastAccessTime:
                case AppDefs.ColumnId.LastModificationTime:
                case AppDefs.ColumnId.ExpiryTime:
                case AppDefs.ColumnId.Attachment:
                case AppDefs.ColumnId.Uuid:
                    // No changes
                    return Text;
                case AppDefs.ColumnId.Notes:
                default:
                    // Keepass specific
                    return Text.Replace("\r", string.Empty).Replace("\n", " ");
            }
        }

        // Normalize Multiline String to MultiLine 
        private string StringNormalizeToMultiLine(string Text, int SubItem)
        {
            switch ((AppDefs.ColumnId)SubItem)
            {
                case AppDefs.ColumnId.Title:
                case AppDefs.ColumnId.UserName:
                case AppDefs.ColumnId.Password:
                case AppDefs.ColumnId.Url:
                case AppDefs.ColumnId.CreationTime:
                case AppDefs.ColumnId.LastAccessTime:
                case AppDefs.ColumnId.LastModificationTime:
                case AppDefs.ColumnId.ExpiryTime:
                case AppDefs.ColumnId.Attachment:
                case AppDefs.ColumnId.Uuid:
                    // No changes
                    return Text;
                case AppDefs.ColumnId.Notes:
                default:
                    // Keepass specific
                    // Use Environment.NewLine
#if USE_RTB   // RichTextBox \r \n \r\n
                    return Text;
#else         // TextBox needs \r\n
                      return Text.Replace("\r", "\r\n").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n");
#endif
            }
        }

        private void SaveEntry(ListViewItem Item, int SubItem, string Text)
        {
            PwEntry pe = (PwEntry)Item.Tag;
            pe = m_host.Database.RootGroup.FindEntry(pe.Uuid, true);

            PwEntry peInit = pe.CloneDeep();

            pe.CreateBackup();

            pe.Touch(true, false); // Touch *after* backup

#if !USE_RTB
              Text = Text.Replace("\r", string.Empty);
#endif

            switch ((AppDefs.ColumnId)SubItem)
            {
                case AppDefs.ColumnId.Title:
                    //if(PwDefs.IsTanEntry(pe))
                    //TODO tan list	 TanTitle ???		    pe.Strings.Set(PwDefs.TanTitle, new ProtectedString(false, Text));
                    //else
                    pe.Strings.Set(PwDefs.TitleField, new ProtectedString(m_host.Database.MemoryProtection.ProtectTitle, Text));
                    break;
                case AppDefs.ColumnId.UserName:
                    pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(m_host.Database.MemoryProtection.ProtectUserName, Text));
                    break;
                case AppDefs.ColumnId.Password:
                    pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(m_host.Database.MemoryProtection.ProtectPassword, Text));
                    break;
                case AppDefs.ColumnId.Url:
                    pe.Strings.Set(PwDefs.UrlField, new ProtectedString(m_host.Database.MemoryProtection.ProtectUrl, Text));
                    break;
                case AppDefs.ColumnId.Notes:
                    pe.Strings.Set(PwDefs.NotesField, new ProtectedString(m_host.Database.MemoryProtection.ProtectNotes, Text));
                    break;
                case AppDefs.ColumnId.CreationTime:
                case AppDefs.ColumnId.LastAccessTime:
                case AppDefs.ColumnId.LastModificationTime:
                case AppDefs.ColumnId.ExpiryTime:
                case AppDefs.ColumnId.Attachment:
                case AppDefs.ColumnId.Uuid:
                    // Nothing todo
                    break;
                default:
                    if (pe.Strings.Exists(m_clveEntries.Columns[SubItem].Text))
                    {
                        pe.Strings.Set(m_clveEntries.Columns[SubItem].Text, new ProtectedString(pe.Strings.Get(m_clveEntries.Columns[SubItem].Text).IsProtected, Text));
                    }
                    else
                    {
                        // SubItem does not exist
                        pe.Strings.Set(m_clveEntries.Columns[SubItem].Text, new ProtectedString(false, Text));
                    }
                    break;
            }

            if (pe.EqualsEntry(peInit, false, true, true, false, true))
            {
                pe.LastModificationTime = peInit.LastModificationTime;

                pe.History.Remove(pe.History.GetAt(pe.History.UCount - 1)); // Undo backup
            }
            else
            {
                // Update toolbar save icon
                m_host.MainWindow.UpdateUI(false, null, false, null, false, null, true);
            }
        }

        //TODO test m_clveEntries.BeginInvoke(OnMenuAddCustomColumns)
        private void OnMenuAddCustomColumns(object sender, EventArgs e)
        {
            if (!m_host.Database.IsOpen)
            {
                MessageBox.Show("You first need to open a database!", "CustomColumn");
                return;
            }

            List<string> strl = new List<string>();

            // Add all known pwentry strings
            foreach (PwEntry pe in m_host.Database.RootGroup.GetEntries(true))
            {
                foreach (KeyValuePair<string, ProtectedString> pstr in pe.Strings)
                {
                    if (!strl.Contains(pstr.Key))
                    {
                        if (!Enum.IsDefined(typeof(AppDefs.ColumnId), pstr.Key))
                        {
                            strl.Add(pstr.Key);
                        }
                    }
                }
            }

            // Remove all listview columns
            foreach (ColumnHeader ch in m_clveEntries.Columns)
            {
                strl.Remove(ch.Text);
            }

            string[] strs = InputComboBox.Show(strl.ToArray(), "Add CustomColumn with name:", "Add CustomColumn");

            if (strs != null)
            {
                foreach (string str in strs)
                {
#if USE_NET20
                    bool b = false;
                    string[] st = m_lCustomColums.ConvertAll<string>(ch => ch.Text).ToArray();
                    foreach (string s in st)
                    {
                        if (s.Equals(str))
                        {
                            b = true;
                        }
                    }
                    if (!b)
#else
                      if(!m_lCustomColums.ConvertAll<string>(ch => ch.Text).ToArray().Contains(str))
#endif
                    {
                        m_evEntries.Suppress("OnPwListColumnWidthChanged");
                        ColumnHeader chd = m_clveEntries.Columns.Add(str);
                        m_evEntries.Resume("OnPwListColumnWidthChanged");

                        m_lCustomColums.Add(chd);

                        //TODO make one function add subitems
                        m_clveEntries.BeginUpdate();
                        foreach (ListViewItem lvi in m_clveEntries.Items)
                        {
                            //Test protected strings ?!?
                            //lvi.SubItems[1].Text = ((PwEntry)lvi.Tag).Strings.Get(str).ReadString();
                            //ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem();
                            PwEntry pe = (PwEntry)lvi.Tag;
                            if (pe.Strings.Exists(str))
                            {
                                string sit = pe.Strings.Get(str).ReadString();
                                lvi.SubItems.Add((new ListViewItem.ListViewSubItem()).Text = StringNormalizeToOneLine(sit, chd.Index));
                            }
                            else
                            {
                                lvi.SubItems.Add((new ListViewItem.ListViewSubItem()).Text = "");
                            }
                        }
                        m_clveEntries.EndUpdate();
                        m_clveEntries.Update();
                    }
                }
            }
        }

        private void OnMenuRemoveCustomColumns(object sender, EventArgs e)
        {
            if (!m_host.Database.IsOpen)
            {
                MessageBox.Show("You first need to open a database!", "CustomColumn");
                return;
            }

            if (m_lCustomColums.Count == 0)
            {
                MessageBox.Show("No Custom Culomns available!", "CustomColumn");
                return;
            }

            string[] strs = m_lCustomColums.ConvertAll<string>(ch => ch.Text).ToArray();
            strs = InputListBox.Show(strs, "Remove CustomColumn with name:", "Remove CustomColumn");

            if (strs != null)
            {
                foreach (string str in strs)
                {
                    ColumnHeader chd = m_lCustomColums.Find(ch => ch.Text.Equals(str));
                    int i = chd.Index;

                    if (chd != null)
                    {
                        m_lCustomColums.Remove(chd);
                        m_clveEntries.Columns.Remove(chd);
                        m_clveEntries.BeginUpdate();
                        foreach (ListViewItem lvi in m_clveEntries.Items)
                        {
                            lvi.SubItems.RemoveAt(i);
                        }
                        m_clveEntries.EndUpdate();
                        m_clveEntries.Update();
                    }
                }
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
            OnPwListCustomColumnUpdate(sender, e);

            //TODO sort custom columns correct
            //Reimplement Listsort function
            //and supress keepass sort fct
            //m_clveEntries.Columns[e.Column].ListView.Sort();
        }

        // Check after UIStateUpdated if all CustomColums SubItems are available
        private void OnPwListCustomColumnUpdate(object sender, EventArgs e)
        {
            bool upd = false;

            // Check custom columns
            foreach (ColumnHeader chd in m_lCustomColums)
            {
                int i = chd.Index;
                string str = chd.Text;

                //TODO make one function add subitems
                // Check subitems
                foreach (ListViewItem lvi in m_clveEntries.Items)
                {
                    if (lvi.SubItems.Count > i)
                    {
                        break;
                    }
                    else
                    {
                        if (!upd)
                        {
                            upd = true;
                            m_clveEntries.BeginUpdate();
                        }

                        PwEntry pe = (PwEntry)lvi.Tag;
                        if (pe.Strings.Exists(str))
                        {
                            string sit = pe.Strings.Get(str).ReadString();
                            lvi.SubItems.Add((new ListViewItem.ListViewSubItem()).Text = StringNormalizeToOneLine(sit, chd.Index));
                        }
                        else
                        {
                            lvi.SubItems.Add((new ListViewItem.ListViewSubItem()).Text = "");
                        }
                    }
                }
            }
            if (upd)
            {
                m_clveEntries.EndUpdate();
                m_clveEntries.Update();

                //TODO sort list
                /*
                foreach (ColumnHeader chd in m_clveEntries.Columns)
                {
                    if (chd.ListView.Sorting != SortOrder.None)
                    {
                        m_clveEntries.ListViewItemSorter = new ListSorter(0, chd.ListView.Sorting, true, false);
                        chd.ListView.Sort();

                        m_clveEntries.ListViewItemSorter = new ListSorter(chd.Index, chd.ListView.Sorting, true, false);
                        chd.ListView.Sort();

                        break;
                    }
                }
                */

                /*
                foreach (ColumnHeader chd in m_lCustomColums)
                {
                    if (chd.ListView.Sorting != SortOrder.None)
                    {
                        chd.ListView.Sort();
                        m_clveEntries.Update();
                        break;
                    }
                }*/

                /*
                    m_pListSorter = new ListSorter(nColumn, sortOrder,
						bSortNaturally, bSortTimes);
					m_lvEntries.ListViewItemSorter = m_pListSorter;
                 */
            }
        }
    }
}
