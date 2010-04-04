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
        // ListView messages
        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETCOLUMNORDERARRAY = (LVM_FIRST + 59);

        private DateTime m_mouseDownForIeAt = DateTime.MinValue;

        private ListViewItem m_previousClickedListViewItem = null;

        private static Mutex mutEdit = new Mutex();

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

        // ListView send messages
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wPar, IntPtr lPar);
        
        //[DllImport("user32.dll", CharSet = CharSet.Ansi)]
        //private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int len, ref	int[] order);

        //[DllImport("user32.dll", EntryPoint = "LockWindowUpdate", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //private static extern long LockWindow(long Handle);
        // Lock listview - prevent scolling
        //LockWindow(m_clveEntries.Handle.ToInt64());
        // Unlock listview
        //LockWindow(0);

        private void InitializeInlineEditing()
        {
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
            
            // Add control to listview
            m_clveEntries.Controls.Add(m_textBoxComment);   
        }

        public void TerminateInlineEditing()
        {
            RemoveHandlerInlineEditing();
        }

        private void AddHandlerInlineEditing()
        {
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

        private void RemoveHandlerInlineEditing()
        {
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

        private void OnMenuInlineEditing(object sender, EventArgs e)
        {
            if (!m_host.Database.IsOpen)
            {
                // doesn't matter
            }

            m_tsmiInlineEditing.Checked = !m_tsmiInlineEditing.Checked;

            if (m_tsmiInlineEditing.Checked)
            {
                // enable function
                m_clveEntries.FullRowSelect = true;
                m_clveEntries.View = View.Details;
                m_clveEntries.AllowColumnReorder = true;

                AddHandlerInlineEditing();                
            }
            else
            {
                // disable function
                RemoveHandlerInlineEditing();
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
            // Only allow left mouse button
            if (e.Button == MouseButtons.Left)
            {
                ListViewItem item;
                int idx = GetSubItemAt(e.X, e.Y, out item);
                if (item != null)
                {
                    if ((m_previousClickedListViewItem != null) && (item == m_previousClickedListViewItem))
                    {
                        long datNow = DateTime.Now.Ticks;
                        long datMouseDown = m_mouseDownForIeAt.Ticks;

                        // Slow double clicking with the left moaus button
                        if ((datNow - datMouseDown > m_mouseTimeMin) && (datNow - datMouseDown < m_mouseTimeMax))
                        {
                            Point pt = m_clveEntries.PointToClient(Cursor.Position);
                            EditSubitemAt(pt);
                        }
                    }
                    m_mouseDownForIeAt = DateTime.Now;
                }
                m_previousClickedListViewItem = item;
            }
        }

        private void OnItemCancel(object sender, EventArgs e)
        {
            if (_editingControl != null)
            {
                // Close and cancel
                EndEditing(false);
            }
        }

        /// <summary>
        /// Begin in-place editing of given cell
        /// </summary>
        /// <param name="c">Control used as cell editor</param>
        /// <param name="Item">ListViewItem to edit</param>
        /// <param name="SubItem">SubItem index to edit</param>
        private void StartEditing(PaddedTextBox c, ListViewItem Item, int SubItem)
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
            c.Text = StringToMultiLine(ReadEntry(Item, SubItem), SubItem);
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

        /// <summary>
        /// Accept or discard current value of cell editor control
        /// </summary>
        /// <param name="AcceptChanges">Use the _editingControl's Text as new SubItem text or discard changes?</param>
        private void EndEditing(bool AcceptChanges)
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
                    _editItem.SubItems[_editSubItem].Text = StringToOneLine(_editingControl.Text, _editSubItem);

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
        private int GetSubItemAt(int x, int y, out ListViewItem item)
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
        private int GetNextSubItemFor(int SubItem)
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
        private int[] GetColumnOrder()
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
        private Rectangle GetSubItemBounds(ListViewItem Item, int SubItem)
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

        private void _editControl_Leave(object sender, EventArgs e)
        {
            // input edit leaves cell editor focus
            // not necessary - see _editControl_LostFocus
        }

        private void _editControl_LostFocus(object sender, EventArgs e)
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

        private void _editControl_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
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
    }
}
