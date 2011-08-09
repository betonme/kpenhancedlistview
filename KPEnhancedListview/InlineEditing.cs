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
        public class KPEnhancedListviewInlineEditing : KPEnhancedListviewBase
        {
            //////////////////////////////////////////////////////////////
            // Sub Plugin setup
            private const string m_tbText = "Inline Editing";
            private const string m_tbToolTip = "Allows entry editing within the listview";
            protected const string m_cfgString = "KPEnhancedListview_InlineEditing";

            public KPEnhancedListviewInlineEditing()
            {
                AddMenu(m_cfgString, m_tbText, m_tbToolTip);
                InitializeInlineEditing();
            }

            //////////////////////////////////////////////////////////////
            // Sub Plugin handler registration
            protected override void AddHandler()
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

                // Tell windows we are interested in drawing items in ListBox on our own
                m_clveEntries.OwnerDraw = true;
                m_clveEntries.DrawItem += new DrawListViewItemEventHandler(this.DrawItemHandler);
                m_clveEntries.DrawSubItem += new DrawListViewSubItemEventHandler(this.DrawSubItemHandler);
                m_clveEntries.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(this.DrawColumnHeaderHandler);
            }

            protected override void RemoveHandler()
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

                m_clveEntries.DrawItem -= new DrawListViewItemEventHandler(this.DrawItemHandler);
                m_clveEntries.DrawSubItem -= new DrawListViewSubItemEventHandler(this.DrawSubItemHandler);
                m_clveEntries.DrawColumnHeader -= new DrawListViewColumnHeaderEventHandler(this.DrawColumnHeaderHandler);
            }

            //////////////////////////////////////////////////////////////
            // Sub Plugin functionality
            private DateTime m_mouseDownForIeAt = DateTime.MinValue;

            private ListViewItem m_previousClickedListViewItem = null;
            private int m_previousClickedListViewSubItem = 0;

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
                // Add control to listview
                m_textBoxComment = new PaddedTextBox();
                m_textBoxComment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                m_textBoxComment.Location = new System.Drawing.Point(32, 104);
                m_textBoxComment.Name = "m_textBoxComment";
                m_textBoxComment.Size = new System.Drawing.Size(80, 16);
                m_textBoxComment.TabIndex = 3;
                m_textBoxComment.Text = string.Empty;
                m_textBoxComment.AutoSize = false;
                m_textBoxComment.Padding = new Padding(6, 1, 1, 0);
                m_textBoxComment.Visible = false;
                m_clveEntries.Controls.Add(m_textBoxComment);
            }

            private void OnItemKeyDown(object sender, System.Windows.Forms.KeyEventArgs k)
            {
                //TODO Ctrl+F2  InlineEditing     // configurable and save
                //TODO Alt+F2   Edit Icon         // configurable and save
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
                    int subitem = Util.GetSubItemAt(m_clveEntries, e.X, e.Y, out item);
                    if (item != null)
                    {
                        if ((m_previousClickedListViewItem != null) && (item == m_previousClickedListViewItem))
                        {
                            if (m_previousClickedListViewSubItem == subitem)
                            {
                                long datNow = DateTime.Now.Ticks;
                                long datMouseDown = m_mouseDownForIeAt.Ticks;

                                // Slow double clicking with the left moaus button
                                if ((datNow - datMouseDown > m_mouseTimeMin) && (datNow - datMouseDown < m_mouseTimeMax))
                                {
                                    Point pt = m_clveEntries.PointToClient(Cursor.Position);
                                    EditSubitemAt(pt);
                                    return;
                                }
                            }
                        }
                        m_mouseDownForIeAt = DateTime.Now;
                    }
                    m_previousClickedListViewItem = item;
                    m_previousClickedListViewSubItem = subitem;
                }
            }

            private void OnItemCancel(object sender, EventArgs e)
            {
                if (_editingControl != null)
                {
                    // Close and cancel
                    EndEditing(_editingControl, _editItem, _editSubItem, false);
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

                if (Item.Index == -1)
                {
                    mutEdit.ReleaseMutex();
                    return;
                }

                // Remove/Suppress UIStateUpdated Event Handler
                // UIStateUpdated is called on entering InlineEditing 
                //TODO m_evMainWindow Suppress not working
                //m_evMainWindow.Suppress("OnPwListCustomColumnUpdate");
                //m_host.MainWindow.UIStateUpdated -= new EventHandler(OnPwListCustomColumnUpdate);

                Util.EnsureVisibleEntry(m_clveEntries, ((PwEntry)Item.Tag).Uuid);

                int colID = SubItem;
                AceColumn col = Util.GetAceColumn(colID);
                AceColumnType colType = col.Type;

                // Set Multiline property
                switch (colType)
                {
                    case AceColumnType.Notes:
                    case AceColumnType.CustomString:
                        c.Multiline = true;
#if USE_LS
                        c.LineSpacing(27); //26 ok); // Spacing equal listview item height
#endif
                        break;
                    case AceColumnType.PluginExt:
                        //TODO
                        c.Multiline = false;
#if USE_LS
                        c.LineSpacing(20); // Single spacing
#endif
                        break;
                    default:
                        c.Multiline = false;
#if USE_LS
                        c.LineSpacing(20); // Single spacing
#endif
                        break;
                }

                // Set editing allowed
                switch (colType)
                {
                    case AceColumnType.CreationTime:
                    case AceColumnType.LastAccessTime:
                    case AceColumnType.LastModificationTime:
                    case AceColumnType.ExpiryTime:
                    case AceColumnType.Uuid:
                    case AceColumnType.Attachment:
                    case AceColumnType.ExpiryTimeDateOnly:
                    case AceColumnType.Size:
                    case AceColumnType.HistoryCount:
                        // No editing allowed
                        c.ReadOnly = true;
                        break;
                    case AceColumnType.PluginExt:
                        //TODO No editing allowed
                        c.ReadOnly = true;
                        break;
                    default:
                        // Editing allowed
                        c.ReadOnly = false;
                        break;
                }

                // Read SubItem text and set textbox property
                // TODO PasswordChar *** during editing for protected strings ???

                PwEntry pe = (((ListViewItem)Item).Tag as PwEntry);

#if USE_RTB
                // Read entry
                c.Text = Util.GetEntryFieldEx(pe, SubItem);
#else
                c.Text = Util.StringToMultiLine(Util.GetEntryFieldEx(pe, SubItem), SubItem);
#endif
                c.ScrollToTop();
                c.SelectAll();

                // Set control location, bounding, padding
                SetEditBox(c, GetSubItemBounds(Item, SubItem), SubItem);

                c.Visible = true;
                c.BringToFront();
                c.Select();
                c.Focus();

                c.Leave += new EventHandler(_editControl_Leave);
                c.LostFocus += new EventHandler(_editControl_LostFocus);
                c.KeyPress += new KeyPressEventHandler(_editControl_KeyPress);

                _editingControl = c;
                _editItem = Item;
                _editSubItem = SubItem;

                mutEdit.ReleaseMutex();
            }

            /// <summary>
            /// Start Image Editing
            /// </summary>
            /// <param name="Item">ListViewItem to edit</param>
            private void StartImageEditing(ListViewItem item)
            {
                IconPickerForm ipf = new IconPickerForm();
                PwEntry pe = ((PwEntry)item.Tag);
                ipf.InitEx(m_host.MainWindow.ClientIcons, (uint)PwIcon.Count, m_host.Database, (uint)pe.IconId, pe.CustomIconUuid);

                if (ipf.ShowDialog() == DialogResult.OK)
                {
                    if (ipf.ChosenCustomIconUuid != PwUuid.Zero)
                        pe.CustomIconUuid = ipf.ChosenCustomIconUuid;
                    else
                    {
                        pe.IconId = (PwIcon)ipf.ChosenIconId;
                        pe.CustomIconUuid = PwUuid.Zero;
                    }

                    //bool bUpdImg = m_host.Database.UINeedsIconUpdate;
                    m_host.MainWindow.RefreshEntriesList();
                    Util.UpdateSaveIcon();
                }
            }

            /// <summary>
            /// Accept or discard current value of cell editor control
            /// </summary>
            /// <param name="AcceptChanges">Use the _editingControl's Text as new SubItem text or discard changes?</param>
            private void EndEditing(PaddedTextBox c, ListViewItem Item, int SubItem, bool AcceptChanges)
            {
                mutEdit.WaitOne();

                if (_editingControl == null)
                {
                    mutEdit.ReleaseMutex();
                    return;
                }

                c.Leave -= new EventHandler(_editControl_Leave);
                c.LostFocus -= new EventHandler(_editControl_LostFocus);
                c.KeyPress -= new KeyPressEventHandler(_editControl_KeyPress);

                c.Visible = false;

                if (AcceptChanges == true)
                {
                    // Check if item and textbox contain different text
                    if (_editItem.SubItems[SubItem].Text != c.Text)
                    {
                        // Save changes
                        AcceptChanges = Util.SaveEntry(m_host.Database, Item, SubItem, c.Text);

                        // Avoid flickering
                        // Set item text manually before calling RefreshEntriesList
                        // If Item is protected
                        if (!_editItem.SubItems[_editSubItem].Text.Equals(PwDefs.HiddenPassword))
                        {
                            // Updating the listview item
#if USE_RTB
                            _editItem.SubItems[_editSubItem].Text = _editingControl.Text;
#else
                            _editItem.SubItems[_editSubItem].Text = Util.StringToOneLine(_editingControl.Text, _editSubItem);
#endif
                        }
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
                    // AcceptChanges is false
                }

                _editingControl = null;
                _editItem = null;
                _editSubItem = -1;

                if (AcceptChanges == true)
                {
                    // The number of visible entries has not changed, so we can call RefreshEntriesList
                    Util.UpdateSaveIcon();
                    m_host.MainWindow.RefreshEntriesList();
                    Util.EnsureVisibleEntry(m_clveEntries, ((PwEntry)Item.Tag).Uuid);
                    Util.SelectEntry(m_clveEntries, (PwEntry)Item.Tag, true);
                }

                //m_clveEntries.Update();
                m_clveEntries.Select();

                // Add/Resume UIStateUpdated Event Handler
                //m_evMainWindow.Resume("OnPwListCustomColumnUpdate");
                //m_host.MainWindow.UIStateUpdated += new EventHandler(OnPwListCustomColumnUpdate);

                mutEdit.ReleaseMutex();
            }

            ///<summary>
            /// Fire SubItemClicked
            ///</summary>
            ///<param name="p">Point of click/doubleclick</param>
            private void EditSubitemAt(Point p)
            {
                ListViewItem item;
                int subitem = Util.GetSubItemAt(m_clveEntries, p.X, p.Y, out item);

                // Image Editing
                if (subitem == 0)
                {
                    if (HitImageTestAt(p, item))
                    {
                        StartImageEditing(item);
                        return;
                    }
                }

                // Inline Editing
                if (subitem >= 0)
                {
                    StartEditing(m_textBoxComment, item, subitem);
                    return;
                }
            }

            private bool HitImageTestAt(Point p, ListViewItem item)
            {
                Rectangle rcItem = item.GetBounds(ItemBoundsPortion.Entire);
                GraphicsUnit units = GraphicsUnit.Point;
                if (item.ImageList != null)
                {
                    Image img = item.ImageList.Images[0];
                    RectangleF rcImageF = img.GetBounds(ref units);
                    Rectangle rcImage = Rectangle.Round(rcImageF);

                    rcImage.Width += item.IndentCount + item.Position.X;
                    p.Offset(rcItem.Left, -rcItem.Top);

                    if (rcImage.Contains(p))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }

            /// <summary>
            /// Get bounds for a SubItem
            /// </summary>
            /// <param name="Item">Target ListViewItem</param>
            /// <param name="SubItem">Target SubItem index</param>
            /// <returns>Bounds of SubItem (relative to ListView)</returns>
            private Rectangle GetSubItemBounds(ListViewItem Item, int SubItem)
            {
                int[] order = Util.GetColumnOrder(m_clveEntries);

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
            /// Find Next ListView SubItem
            /// </summary>
            /// <param name="SubItem">SubItem to find next for</param>
            /// <returns>SubItem index</returns>
            private int GetNextSubItemFor(int SubItem)
            {
                int[] order = Util.GetColumnOrder(m_clveEntries);

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
                if (c.Lines.Length > 1)
                {
#if USE_LS
                                // For linespacing equal listview
                rcSubItem.Height *= c.Lines.Count();

                // For linespacing equal singlespacing
                //rcSubItem.Height = rcSubItem.Height + c.Lines.Count() * /*~*/ 8;
#endif
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
                    EndEditing(_editingControl, _editItem, _editSubItem, true);
                }
                else
                {
                    // focus is gone away - cancel
                    EndEditing(_editingControl, _editItem, _editSubItem, false);
                }
            }

            private void _editControl_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
            {
                // TODO PasswordChar *** during editing for protected strings ???
                switch (e.KeyChar)
                {
                    case (char)(int)Keys.Escape:
                        {
                            // Close and cancel
                            EndEditing(_editingControl, _editItem, _editSubItem, false);
                            break;
                        }

                    case (char)(int)Keys.Enter:
                        {
                            // Close and save
                            EndEditing(_editingControl, _editItem, _editSubItem, true);
                            break;
                        }

                    case (char)(int)Keys.Tab:
                        {
                            //TODO add new function 
                            // Save and edit next SubItem
                            ListViewItem Item = _editItem;
                            int SubItem = GetNextSubItemFor(_editSubItem);

                            EndEditing(_editingControl, _editItem, _editSubItem, true);

                            if (m_clveEntries.SelectedIndices.Count > 0)
                            {
                                StartEditing(m_textBoxComment, m_clveEntries.SelectedItems[0], SubItem);
                            }

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

                                EndEditing(_editingControl, _editItem, _editSubItem, true);

                                StartEditing(m_textBoxComment, Item, SubItem);
                            }

                            if (Control.ModifierKeys == Keys.ControlKey)
                            {
                                MessageBox.Show("2");
                                // Save and edit next SubItem
                                ListViewItem Item = _editItem;
                                int SubItem = GetNextSubItemFor(_editSubItem);

                                EndEditing(_editingControl, _editItem, _editSubItem, true);

                                StartEditing(m_textBoxComment, Item, SubItem);
                            }

                            break;
                        }
                }
            }


            private void DrawItemHandler(object sender, DrawListViewItemEventArgs e)
            {
                // Inline Editing
                if (_editingControl != null)
                {
                    //if (_editItem.Equals(e.Item)) // textbox does not move outside the listview clientarea
                    {
                        // Check if item is visible - below ColumnHeader     
                        if (m_headerBottom <= m_clveEntries.Items[_editItem.Index].Bounds.Top)
                        {
                            Rectangle rect = GetSubItemBounds(m_clveEntries.Items[_editItem.Index], _editSubItem);
                            //if (!_editingControl.Bounds.Equals(rect))
                            {
                                SetEditBox(_editingControl, rect, _editSubItem);
                            }
                        }
                        else
                        {
                            //if (_editingControl.Bounds.Height != 0)
                            {
                                // Workaround to avoid Editbox is drawn on ColumnHeader
                                Rectangle rc = GetSubItemBounds(m_clveEntries.Items[_editItem.Index], _editSubItem);
                                rc.Height = 0;
                                SetEditBox(_editingControl, rc, _editSubItem);
                            }
                        }
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
                m_headerBottom = e.Bounds.Bottom;

                e.DrawDefault = true;
            }

        }
    }
}
