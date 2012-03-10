/*
    KPEnhancedListview - Extend the KeePass Listview for inline editing.
    Copyright (C) 2010 - 2012  Frank Glaser  <glaserfrank(at)gmail.com>
    http://code.google.com/p/kpenhancedlistview
    
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
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KPEnhancedListview
{
    partial class KPEnhancedListviewExt
    {
        public class KPEnhancedListviewInlineEditing : SubPluginBase
        {
            //////////////////////////////////////////////////////////////
            // Sub Plugin setup
            private const string m_tbText = "Inline Editing";
            private const string m_tbToolTip = "Allows entry editing within the listview";
            protected const string m_cfgString = "KPEnhancedListview_InlineEditing";

            //TODO Add submenu button list
            //Display them greyed if subplugin is disabled
            //private const string m_tbTextCN = "Cursor Navigation";
            //private const string m_tbToolTipCN = "Allows entry editing within the listview";
            //protected const string m_cfgStringCN = "KPEnhancedListview_InlineEditing";

            public KPEnhancedListviewInlineEditing()
            {
                AddMenu(m_cfgString, m_tbText, m_tbToolTip);
                //AddMenu(m_cfgStringCN, m_tbTextCN, m_tbToolTipCN);
                InitializeInlineEditing();
            }

            //////////////////////////////////////////////////////////////
            // Sub Plugin handler registration
            protected override void AddHandler()
            {
                m_lvEntries.KeyDown += new KeyEventHandler(this.OnItemKeyDown);
                m_lvEntries.MouseUp += new MouseEventHandler(this.OnItemMouseUp);

                m_lvEntries.Invalidated += new InvalidateEventHandler(this.OnItemCancel);
                m_lvEntries.ColumnReordered += new ColumnReorderedEventHandler(this.OnItemCancel);

                // Tell windows we are interested in drawing items in ListBox on our own
                m_lvEntries.OwnerDraw = true;
                m_lvEntries.DrawItem += new DrawListViewItemEventHandler(this.DrawItemHandler);
                m_lvEntries.DrawSubItem += new DrawListViewSubItemEventHandler(this.DrawSubItemHandler);
                m_lvEntries.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(this.DrawColumnHeaderHandler);
            }

            protected override void RemoveHandler()
            {
                m_lvEntries.KeyDown -= new KeyEventHandler(this.OnItemKeyDown);
                m_lvEntries.MouseUp -= new MouseEventHandler(this.OnItemMouseUp);

                m_lvEntries.Invalidated -= new InvalidateEventHandler(this.OnItemCancel);
                m_lvEntries.ColumnReordered -= new ColumnReorderedEventHandler(this.OnItemCancel); // Todo change position of textbox and redraw

                m_lvEntries.OwnerDraw = false;
                m_lvEntries.DrawItem -= new DrawListViewItemEventHandler(this.DrawItemHandler);
                m_lvEntries.DrawSubItem -= new DrawListViewSubItemEventHandler(this.DrawSubItemHandler);
                m_lvEntries.DrawColumnHeader -= new DrawListViewColumnHeaderEventHandler(this.DrawColumnHeaderHandler);
            }

            //////////////////////////////////////////////////////////////
            // Sub Plugin functionality
            private DateTime m_mouseDownForIeAt = DateTime.MinValue;

            // Mouse handler helper for detecting InlineEditing
            private const int m_mouseTimeMin = 3000000;
            private const int m_mouseTimeMax = 10000000;

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
            private int _editSubItem;

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
                m_lvEntries.Controls.Add(m_textBoxComment);
            }

            private void OnItemKeyDown(object sender, System.Windows.Forms.KeyEventArgs k)
            {
                //TODO Ctrl+F2  InlineEditing     // configurable and save
                //TODO Alt+F2   Edit Icon         // configurable and save
                if ( (k.Control && k.KeyCode == Keys.F2) || (k.KeyCode == Keys.F3) )
                {
                    // edit selected item
                    if (m_lvEntries.FocusedItem != null)
                    {
                        ListViewItem Item = m_lvEntries.FocusedItem;
                        if (Item != null)
                        {
                            StartEditing(Item, 0);
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
                    int subitem = Util.GetSubItemAt(e.X, e.Y, out item);
                    if (item != null)
                    {
                        if ((m_previousClickedListViewItem != null) && (item == m_previousClickedListViewItem))
                        {
                            if (m_previousClickedListViewSubItem == subitem)
                            {
                                long datNow = DateTime.Now.Ticks;
                                long datMouseDown = m_mouseDownForIeAt.Ticks;

                                // Slow double clicking with the left mouse button
                                if ((datNow - datMouseDown > m_mouseTimeMin) && (datNow - datMouseDown < m_mouseTimeMax))
                                {
                                    Point pt = m_lvEntries.PointToClient(Cursor.Position);
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
                    EndEditing(false);
                }
            }

            public void StartEditing(ListViewItem Item)
            {
                if (Item != null)
                {
                    PwListItem pli = (((ListViewItem)Item).Tag as PwListItem);
                    if (pli == null) { Debug.Assert(false); return; }
                    PwEntry pe = pli.Entry;
                    StartEditing(pe, Item, 0, false);
                }
            }

            public void StartEditing(ListViewItem Item, int SubItem)
            {
                if (Item != null)
                {
                    PwListItem pli = (((ListViewItem)Item).Tag as PwListItem);
                    if (pli == null) { Debug.Assert(false); return; }
                    PwEntry pe = pli.Entry;
                    StartEditing(pe, Item, SubItem, false);
                }
            }

            public void StartEditing(ListViewItem Item, int SubItem, bool ContinueEdit)
            {
                if (Item != null)
                {
                    PwListItem pli = (((ListViewItem)Item).Tag as PwListItem);
                    if (pli == null) { Debug.Assert(false); return; }
                    PwEntry pe = pli.Entry;
                    StartEditing(pe, Item, SubItem, ContinueEdit);
                }
            }

            public void StartEditingOLD(PwEntry pe, int SubItem)
            {
                if (pe != null)
                {
                    ListViewItem Item = Util.GuiFindEntry(pe.Uuid);
                    if (Item != null)
                    {
                        StartEditing(pe, Item, SubItem, false);
                    }
                }
            }

            /// <summary>
            /// Begin in-place editing of given cell
            /// </summary>
            /// <param name="c">Control used as cell editor</param>
            /// <param name="Item">ListViewItem to edit</param>
            /// <param name="SubItem">SubItem index to edit</param>
            private void StartEditing(PwEntry pe, ListViewItem Item, int SubItem, bool ContinueEdit)
            {
                if (!this.GetEnable())
                {
                    return;
                }

                mutEdit.WaitOne();

                if (Item.Index == -1)
                {
                    mutEdit.ReleaseMutex();
                    return;
                }

                //if (_editingControl != null)
                //{
                //    mutEdit.ReleaseMutex();
                //    return;
                //}

                m_host.MainWindow.EnsureVisibleEntry(pe.Uuid);
                Util.SelectEntry(pe, true, true);

                int colID = SubItem;
                AceColumn col = Util.GetAceColumn(colID);
                AceColumnType colType = col.Type;
                PaddedTextBox c = m_textBoxComment;

                // Set Multiline property
                //TODO separate function
                switch (colType)
                {
                    case AceColumnType.Notes:
                    case AceColumnType.CustomString:
                        c.Multiline = true;
                        break;
                    case AceColumnType.PluginExt:
                        //TODO
                        c.Multiline = false;
                        break;
                    default:
                        c.Multiline = false;
                        break;
                }

                // Set editing allowed
                //TODO separate function
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

                // TODO Optionally PasswordChar *** during editing for protected strings

                // Read entry
                c.Text = Util.GetEntryFieldEx(pe, SubItem, false);

                // Set control location, bounding, padding
                SetEditBox(c, GetSubItemBounds(Item, SubItem), SubItem);
                
                //c.ScrollToTop();
                c.SelectAll();

                _editingControl = c;
                _editItem = Item;
                _editSubItem = SubItem;

                if (ContinueEdit == false)
                {
                    //c.Invalidate();
                    c.Visible = true;
                    c.BringToFront();
                    c.Select();
                    c.Focus();

                    m_host.MainWindow.EnsureVisibleEntry(pe.Uuid);

                    // Check sub plugin state
                    if (!m_bEnabled)
                    {
                        // Function has to be enabled
                        AddHandler();
                    }
                }

                // Should be in the textbox
                c.Leave += new EventHandler(_editControl_Leave);
                c.LostFocus += new EventHandler(_editControl_LostFocus);
                c.KeyPress += new KeyPressEventHandler(_editControl_KeyPress);

                mutEdit.ReleaseMutex();
            }

            /// <summary>
            /// Start Image Editing
            /// </summary>
            /// <param name="Item">ListViewItem to edit</param>
            private void StartImageEditing(ListViewItem item)
            {
                IconPickerForm ipf = new IconPickerForm();

                PwListItem pli = (((ListViewItem)item).Tag as PwListItem);
                if (pli == null) { Debug.Assert(false); return; }
                PwEntry pe = pli.Entry;

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

                    //m_host.MainWindow.RefreshEntriesList();
                    //Util.UpdateSaveIcon();
                }
            }

            private void EndEditing(bool AcceptChanges)
            {
                EndEditing(AcceptChanges, false);
            }

            /// <summary>
            /// Accept or discard current value of cell editor control
            /// </summary>
            /// <param name="AcceptChanges">Use the _editingControl's Text as new SubItem text or discard changes?</param>
            private void EndEditing(bool AcceptChanges, bool ContinueEdit)
            {
                mutEdit.WaitOne();

                //if (_editingControl == null)
                //{
                //    mutEdit.ReleaseMutex();
                //    return;
                //}

                ListViewItem Item = _editItem;
                int SubItem = _editSubItem;
                PaddedTextBox c = _editingControl;

                PwListItem pli = (((ListViewItem)Item).Tag as PwListItem);
                if (pli == null) { Debug.Assert(false); return; }
                PwEntry pe = pli.Entry;

                if (AcceptChanges == true)
                {
                    // Check if item and textbox contain different text
                    if (Util.GetEntryFieldEx(pe, SubItem, false) != c.Text)
                    {
                        // Save changes
                        //MAYBE save only if editing is stopped or another item will be edited next
                        //if (ContinueEdit == false)
                        AcceptChanges = Util.SaveEntry(m_host.Database, Item, SubItem, c.Text);

                        //TODO TEST maybe it wont flickr if we set visible false later
                        // Avoid flickering
                        // Set item text manually before calling RefreshEntriesList
                        // If Item is protected
                        if (!_editItem.SubItems[_editSubItem].Text.Equals(PwDefs.HiddenPassword))
                        {
                            // Updating the listview item
                            _editItem.SubItems[_editSubItem].Text = Util.GetEntryFieldEx(pe, SubItem, true);
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

                //TODO Should be part of the textbox and inlineedit should be notified by event
                c.Leave -= new EventHandler(_editControl_Leave);
                c.LostFocus -= new EventHandler(_editControl_LostFocus);
                c.KeyPress -= new KeyPressEventHandler(_editControl_KeyPress);

                if (ContinueEdit == false)
                {
                    // Check sub plugin state
                    if (!m_bEnabled)
                    {
                        // Function has to be disabled
                        RemoveHandler();
                    }

                    _editingControl = null;
                    _editItem = null;
                    _editSubItem = -1;

                    c.Visible = false;

                    //Util.SelectEntry((PwEntry)Item.Tag, true);            
                    //m_lvEntries.Update();
                    m_lvEntries.Select();
                    m_lvEntries.HideSelection = false;
                    m_lvEntries.Focus();

                    if (AcceptChanges == true)
                    {
                        // The number of visible entries has not changed, so we can call RefreshEntriesList
                        Util.UpdateSaveState();
                        m_host.MainWindow.EnsureVisibleEntry(pe.Uuid);
                        //TEST m_host.MainWindow.RefreshEntriesList();
                    }
                }
                
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
                int subitem = Util.GetSubItemAt(p.X, p.Y, out item);
                if (item != null)
                {
                    // Image Editing
                    if (subitem == 0)
                    {
                        // The Icon is part of the SubItem 0 maybe it was clicked
                        if (HitImageTestAt(p, item))
                        {
                            StartImageEditing(item);
                            return;
                        }
                    }

                    // Inline Editing
                    if (subitem >= 0)
                    {
                        StartEditing(item, subitem);
                        return;
                    }
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
                int[] order = Util.GetColumnOrder();

                Rectangle subItemRect = Rectangle.Empty;
                if (SubItem >= order.Length)
                    throw new IndexOutOfRangeException("SubItem " + SubItem + " out of range");

                if (Item == null)
                    throw new ArgumentNullException("Item");

                Rectangle lviBounds = Item.GetBounds(ItemBoundsPortion.Entire);
                int subItemX = lviBounds.Left;
                switch (m_lvEntries.BorderStyle)
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
                    col = m_lvEntries.Columns[order[i]];
                    if (col.Index == SubItem)
                        break;
                    subItemX += col.Width;
                }

                if (m_lvEntries.RightToLeftLayout)
                {
                    subItemX = m_lvEntries.Width - subItemX - m_lvEntries.Columns[order[i]].Width;
                }

                // Adapt the bounds
                subItemX = subItemX - 1;
                int subItemT = lviBounds.Top + 2;
                int subItemW = m_lvEntries.Columns[order[i]].Width + 1;
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
            /// Find Next ListView Item
            /// </summary>
            /// <param name="Item">Item to find next for</param>
            /// <returns>Item</returns>
            private ListViewItem GetNextItemFor(ListViewItem Item)
            {
                bool m_bEntryGrouping = m_lvEntries.ShowGroups;

                ListViewItem nextItem = null;

                if (m_lvEntries.Items.Count > 0)
                {
                    if ( m_lvEntries.Groups.Count > 0 )
                    {
                        // Grouping is enabled
                        // Within the group the items are not in display order,
                        // but the order can be derived from the item indices
                        PwListItem pli = (((ListViewItem)Item).Tag as PwListItem);
                        PwGroup pg = pli.Entry.ParentGroup;
                        
                        foreach (ListViewGroup lvg in m_lvEntries.Groups)
                        {
                            if ((lvg.Tag as PwGroup) == pli.Entry.ParentGroup)
                            {
                                List<ListViewItem> lItems = new List<ListViewItem>();
                                foreach (ListViewItem lviEnum in lvg.Items)
                                    lItems.Add(lviEnum);
                                lItems.Sort(Util.LviCompareByIndex);

                                int nextidx = lItems.IndexOf(Item) + 1;
                                if (nextidx < lItems.Count)
                                {
                                    // Next item is within the group
                                    nextItem = lItems[nextidx];
                                    break;
                                }
                                else
                                {
                                    // Next item is in next group
                                    int nextgrp = m_lvEntries.Groups.IndexOf(lvg) + 1;
                                    if (nextgrp < m_lvEntries.Groups.Count)
                                    {
                                        // Go to first item in next group
                                        lItems = new List<ListViewItem>();
                                        foreach (ListViewItem lviEnum in m_lvEntries.Groups[nextgrp].Items)
                                            lItems.Add(lviEnum);
                                        lItems.Sort(Util.LviCompareByIndex);
                                        nextItem = lItems[0];
                                    }
                                    else
                                    {
                                        // Go to first item in first group
                                        lItems = new List<ListViewItem>();
                                        foreach (ListViewItem lviEnum in m_lvEntries.Groups[0].Items)
                                            lItems.Add(lviEnum);
                                        lItems.Sort(Util.LviCompareByIndex);
                                        nextItem = lItems[0];
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Grouping is disabled
                        // All items in list are in their correct order
                        int idx = m_lvEntries.Items.IndexOf(Item);
                        int cnt = m_lvEntries.Items.Count;
                        nextItem = m_lvEntries.Items[(idx + 1) % cnt];
                    }
                }

                return nextItem;
            }

            private ListViewItem GetPreviousItemFor(ListViewItem Item)
            {
                bool m_bEntryGrouping = m_lvEntries.ShowGroups;

                ListViewItem prevItem = null;

                if (m_lvEntries.Items.Count > 0)
                {
                    if (m_lvEntries.Groups.Count > 0)
                    {
                        // Grouping is enabled
                        // Within the group the items are not in display order,
                        // but the order can be derived from the item indices
                        PwListItem pli = (((ListViewItem)Item).Tag as PwListItem);
                        PwGroup pg = pli.Entry.ParentGroup;

                        foreach (ListViewGroup lvg in m_lvEntries.Groups)
                        {
                            if ((lvg.Tag as PwGroup) == pli.Entry.ParentGroup)
                            {
                                List<ListViewItem> lItems = new List<ListViewItem>();
                                foreach (ListViewItem lviEnum in lvg.Items)
                                    lItems.Add(lviEnum);
                                lItems.Sort(Util.LviCompareByIndex);

                                int previdx = lItems.IndexOf(Item) - 1;
                                if (previdx >= 0)
                                {
                                    // Previous item is within the group
                                    prevItem = lItems[previdx];
                                    break;
                                }
                                else
                                {
                                    // Previous item is in previous group
                                    int prevgrp = m_lvEntries.Groups.IndexOf(lvg) - 1;
                                    if (prevgrp >= 0)
                                    {
                                        // Go to last item in previous group
                                        lItems = new List<ListViewItem>();
                                        foreach (ListViewItem lviEnum in m_lvEntries.Groups[prevgrp].Items)
                                            lItems.Add(lviEnum);
                                        lItems.Sort(Util.LviCompareByIndex);
                                        prevItem = lItems[lItems.Count - 1];
                                    }
                                    else
                                    {
                                        // Go to last item in last group
                                        lItems = new List<ListViewItem>();
                                        foreach (ListViewItem lviEnum in m_lvEntries.Groups[m_lvEntries.Groups.Count - 1].Items)
                                            lItems.Add(lviEnum);
                                        lItems.Sort(Util.LviCompareByIndex);
                                        prevItem = lItems[lItems.Count - 1];
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Grouping is disabled
                        // All items in list are in their correct order
                        int idx = m_lvEntries.Items.IndexOf(Item);
                        int cnt = m_lvEntries.Items.Count;
                        prevItem = m_lvEntries.Items[(idx - 1 + cnt) % cnt];
                    }
                }

                return prevItem;
            }

            /// <summary>
            /// Find next ListView SubItem
            /// </summary>
            /// <param name="SubItem">SubItem to find next for</param>
            /// <returns>SubItem index</returns>
            private int GetNextSubItemFor(int SubItem)
            {
                int[] order = Util.GetColumnOrder();

                int start = m_lvEntries.Columns[SubItem].DisplayIndex;
                //TEST shift order start 
                //go through all order and check width
                int length = order.Length;
                int nextSubItem = 0;

                for (int i = 0; i < length; i++)
                {
                    int idx = (i + start) % length;
                    {
                        int newidx = (idx + 1) % length;
                        // Make sure column is visible
                        if (m_lvEntries.Columns[order[newidx]].Width > 1)
                        {
                            nextSubItem = newidx;
                            break;
                        }
                    }
                }
                return order[nextSubItem];
            }

            /// <summary>
            /// Find previous ListView SubItem
            /// </summary>
            /// <param name="SubItem">SubItem to find previous for</param>
            /// <returns>SubItem index</returns>
            private int GetPreviousSubItemFor(int SubItem)
            {
                int[] order = Util.GetColumnOrder();

                int start = m_lvEntries.Columns[SubItem].DisplayIndex;
                int length = order.Length;
                int prevSubItem = length - 1;

                for (int i = length; i >= 0; i--)
                {
                    int idx = (i + start) % length;
                    {
                        int newidx = (idx - 1 + length) % length;
                        // Make sure column is visible
                        if (m_lvEntries.Columns[order[newidx]].Width > 1)
                        {
                            prevSubItem = newidx;
                            break;
                        }
                    }
                }
                return order[prevSubItem];
            }

            private void SetEditBox(PaddedTextBox c, Rectangle rcSubItem, int SubItem)
            {
                if (rcSubItem.X < 0)
                {
                    // Left edge of SubItem not visible - adjust rectangle position and width
                    rcSubItem.Width += rcSubItem.X;
                    rcSubItem.X = 0;
                }

                if (rcSubItem.X + rcSubItem.Width > m_lvEntries.ClientRectangle.Width)
                {
                    // Right edge of SubItem not visible - adjust rectangle width
                    rcSubItem.Width = m_lvEntries.ClientRectangle.Width - rcSubItem.Left;
                }

                // Calculate editbox height
                //if (c.Lines.Length > 1)
                if (c.Multiline)
                {
                    // Always display only 1 line
                    //rcSubItem.Height *= 1;
                    
                    // Set height depending on lines
                    //rcSubItem.Height *= c.LineCount;
                    rcSubItem.Height = c.CalculateHeight;

                    //c.ScrollBars(true);
                }
                else
                {
                    c.ScrollBars(false);
                }

                // Subitem bounds are relative to the location of the ListView!
                rcSubItem.Offset(m_lvEntries.Left, m_lvEntries.Top);

                // In case the editing control and the listview are on different parents,
                // account for different origins
                Point origin = new Point(0, 0);
                Point lvOrigin = m_lvEntries.Parent.PointToScreen(origin);
                Point ctlOrigin = c.Parent.PointToScreen(origin);

                rcSubItem.Offset(lvOrigin.X - ctlOrigin.X, lvOrigin.Y - ctlOrigin.Y);

                // Padding
                Padding pdSubItem = new Padding(6, 1, 1, 0);
                if (SubItem == 0)
                {
                    pdSubItem.Left = 1;
                }
                else if (c.Multiline)
                {
                    pdSubItem.Left = 5;
                }

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
                if (m_lvEntries.Focused)
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
                //MAYBE Optionally PasswordChar *** during editing for protected strings
                switch (e.KeyChar)
                {
                    case (char)(int)Keys.Escape:
                        {
                            // TODO if addentry remove it 
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

                    case (char)(int)Keys.Right:
                        {
                            EndEditing(true, true);
                            ListViewItem Item = _editItem;
                            int SubItem = GetNextSubItemFor(_editSubItem);
                            StartEditing(Item, SubItem, true);

                            break;
                        }

                    case (char)(int)Keys.Left:
                        {
                            EndEditing(true, true);
                            ListViewItem Item = _editItem;
                            int SubItem = GetPreviousSubItemFor(_editSubItem);
                            StartEditing(Item, SubItem, true);

                            break;
                        }

                    case (char)(int)Keys.Up:
                        {
                            EndEditing(true, true);
                            ListViewItem Item = GetPreviousItemFor(_editItem);
                            int SubItem = _editSubItem;
                            StartEditing(Item, SubItem, true);

                            break;
                        }

                    case (char)(int)Keys.Down:
                        {
                            EndEditing(true, true);
                            ListViewItem Item = GetNextItemFor(_editItem);
                            int SubItem = _editSubItem;
                            StartEditing(Item, SubItem, true);

                            break;
                        }

                    case (char)(int)Keys.Tab:
                        {
                            //TODO add new function NextEditing
                            //NextEditing(_editItem, SubItem);
                            // Save and edit next SubItem
                            ListViewItem Item = _editItem;
                            int SubItem = 0;

                            EndEditing(true, true);
                            if (Control.ModifierKeys == Keys.Shift)
                            {
                                SubItem = GetPreviousSubItemFor(_editSubItem);
                            }
                            else
                            {
                                SubItem = GetNextSubItemFor(_editSubItem);
                            }
                            StartEditing(Item, SubItem, true);

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
                        if (m_headerBottom <= m_lvEntries.Items[_editItem.Index].Bounds.Top)
                        {
                            Rectangle rect = GetSubItemBounds(m_lvEntries.Items[_editItem.Index], _editSubItem);
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
                                Rectangle rc = GetSubItemBounds(m_lvEntries.Items[_editItem.Index], _editSubItem);
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
