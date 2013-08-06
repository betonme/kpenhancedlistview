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
using KeePassLib.Collections;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KPEnhancedListview
{
    partial class KPEnhancedListviewExt
    {
        public static class Util
        {

            /// <summary>
            /// Finds a Control recursively. Note finds the first match and exists
            /// </summary>
            /// <param name="container">The container to search for the control passed. Remember
            /// all controls (Panel, GroupBox, Form, etc are all containsers for controls
            /// </param>
            /// <param name="name">Name of the control to look for</param>
            /// <returns>The Control we found</returns>
            internal static Control FindControlRecursive(Control container, string name)
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

            // Get all KeePass Columns
            internal static List<string> GetListKeePassColumns()
            {
                List<string> strl = new List<string>();

                foreach (ColumnHeader ch in m_lvEntries.Columns)
                {
                    strl.Add(ch.Text);
                }

                strl.Sort();
                return strl;
            }

            // Get all user defined strings
            internal static List<string> GetListEntriesUserStrings(PwGroup pwg)
            {
                List<string> strl = new List<string>();

                // Add all known pwentry strings
                foreach (PwEntry pe in pwg.GetEntries(true))
                {
                    foreach (KeyValuePair<string, ProtectedString> pstr in pe.Strings)
                    {
                        if (!strl.Contains(pstr.Key))
                        {
                            if (!PwDefs.IsStandardField(pstr.Key))
                            {
                                strl.Add(pstr.Key);
                            }
                        }
                    }
                }

                strl.Sort();

                return strl;
            }

            /// <summary>
            /// Find ListViewItem and SubItem Index at position (x,y)
            /// </summary>
            /// <param name="x">relative to ListView</param>
            /// <param name="y">relative to ListView</param>
            /// <param name="item">Item at position (x,y)</param>
            /// <returns>SubItem index</returns>
            internal static int GetSubItemAt(int x, int y, out ListViewItem item)
            {
                item = m_lvEntries.GetItemAt(x, y);

                if (item != null)
                {
                    int[] order = GetColumnOrder();
                    Rectangle lviBounds;
                    int subItemX;

                    lviBounds = item.GetBounds(ItemBoundsPortion.Entire);
                    subItemX = lviBounds.Left;
                    for (int i = 0; i < order.Length; i++)
                    {
                        ColumnHeader h = m_lvEntries.Columns[order[i]];
                        if (x < subItemX + h.Width)
                        {
                            return h.Index;
                        }
                        subItemX += h.Width;
                    }
                }

                return -1;
            }

/*
        private void OnPwListMouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListViewHitTestInfo lvHit = m_lvEntries.HitTest(e.Location);
			ListViewItem lvi = lvHit.Item;

			if(lvHit.SubItem != null)
			{
				int i = 0;
				foreach(ListViewItem.ListViewSubItem lvs in lvi.SubItems)
				{
					if(lvs == lvHit.SubItem)
					{
						PerformDefaultAction(sender, e, lvi.Tag as PwEntry, i);
						break;
					}

					++i;
				}
			}
			else PerformDefaultAction(sender, e, lvi.Tag as PwEntry, 0);
		}
*/


            // ListView messages constants
            private const int LVM_FIRST = 0x1000;
            private const int LVM_GETCOLUMNORDERARRAY = (LVM_FIRST + 59);

            // ListView send message
            [DllImport("user32.dll")]
            private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int[] lParam);

            /// <summary>
            /// Retrieve the order in which columns appear
            /// </summary>
            /// <returns>Current display order of column indices</returns>
            internal static int[] GetColumnOrder()
            {
                int count = m_lvEntries.Columns.Count;
                int[] order = new int[count];

                if (SendMessage(m_lvEntries.Handle, LVM_GETCOLUMNORDERARRAY, count, order) == 0)
                {
                    throw new ApplicationException("GetColumnOrder SendMessage exception");
                }

                return order;
            }

            /*
            // Get all user defined strings
            internal static Dictionary<string, string> GetDictEntriesUserStrings(PwGroup pwg)
            {
                Dictionary<string, string> strd = new Dictionary<string, string>();
                //SortedDictionary<string, string> strd = new SortedDictionary<string, string>();

                // Add all known pwentry strings
                foreach (PwEntry pe in pwg.GetEntries(true))
                {
                    foreach (KeyValuePair<string, ProtectedString> pstr in pe.Strings)
                    {
                        if (!strd.ContainsKey(pstr.Key))
                        {
                            if (!PwDefs.IsStandardField(pstr.Key))
                            {
                                strd.Add(pstr.Key, pstr.Value.ReadString());
                            }
                        }
                    }
                }

                return strd;
            }*/

            // Ported from KeePass Entry Dialog SaveEntry() and UpdateEntryStrings(...)
            internal static bool SaveEntry(PwDatabase pwStorage, ListViewItem Item, int SubItem, string Text)
            {
                PwListItem pli = (((ListViewItem)Item).Tag as PwListItem);
                if (pli == null) { Debug.Assert(false); return false; }
                PwEntry pe = pli.Entry;
                pe = pwStorage.RootGroup.FindEntry(pe.Uuid, true);

                PwEntry peInit = pe.CloneDeep();
                pe.CreateBackup(null);
                pe.Touch(true, false); // Touch *after* backup

                int colID = SubItem;
                AceColumn col = GetAceColumn(colID);
                AceColumnType colType = col.Type;
                switch (colType)
                {
                    case AceColumnType.Title:
                        //if(PwDefs.IsTanEntry(pe))
                        //TODO tan list	 TanTitle ???		    pe.Strings.Set(PwDefs.TanTitle, new ProtectedString(false, Text));
                        //else
                        pe.Strings.Set(PwDefs.TitleField, new ProtectedString(pwStorage.MemoryProtection.ProtectTitle, Text));
                        break;
                    case AceColumnType.UserName:
                        pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(pwStorage.MemoryProtection.ProtectUserName, Text));
                        break;
                    case AceColumnType.Password:
                        //byte[] pb = Text.ToUtf8();
                        //pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(pwStorage.MemoryProtection.ProtectPassword, pb));
                        //MemUtil.ZeroByteArray(pb);
                        pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(pwStorage.MemoryProtection.ProtectPassword, Text));
                        break;
                    case AceColumnType.Url:
                        pe.Strings.Set(PwDefs.UrlField, new ProtectedString(pwStorage.MemoryProtection.ProtectUrl, Text));
                        break;
                    case AceColumnType.Notes:
                        pe.Strings.Set(PwDefs.NotesField, new ProtectedString(pwStorage.MemoryProtection.ProtectNotes, Text));
                        break;
                    case AceColumnType.OverrideUrl:
                        pe.OverrideUrl = Text;
                        break;
                    case AceColumnType.Tags:
                        List<string> vNewTags = StrUtil.StringToTags(Text);
                        pe.Tags.Clear();
                        foreach (string strTag in vNewTags) pe.AddTag(strTag);
                        break;
                    case AceColumnType.CustomString:
                        pe.Strings.Set(col.CustomName, new ProtectedString(pe.Strings.GetSafe(col.CustomName).IsProtected, Text));
                        break;
                    default:
                        // Nothing todo
                        break;
                }

                PwCompareOptions cmpOpt = (PwCompareOptions.IgnoreLastMod | PwCompareOptions.IgnoreLastAccess | PwCompareOptions.IgnoreLastBackup);
                if (pe.EqualsEntry(peInit, cmpOpt, MemProtCmpMode.None))
                {
                    pe.LastModificationTime = peInit.LastModificationTime;

                    pe.History.Remove(pe.History.GetAt(pe.History.UCount - 1)); // Undo backup

                    return false;
                }
                else
                {
                    return true;
                }
            }

            // Adapted from KeePass because it is private
            internal static AceColumn GetAceColumn(int nColID)
            {
                List<AceColumn> v = Program.Config.MainWindow.EntryListColumns;
                if ((nColID < 0) || (nColID >= v.Count)) { Debug.Assert(false); return new AceColumn(); }

                return v[nColID];
            }

            // Adapted from KeePass because it is private
            internal static ListViewItem GuiFindEntry(PwUuid puSearch)
            {
                Debug.Assert(puSearch != null);
                if (puSearch == null) return null;

                foreach (ListViewItem lvi in m_lvEntries.Items)
                {
                    PwListItem pli = (((ListViewItem)lvi).Tag as PwListItem);
                    if (pli == null) { Debug.Assert(false); return null; }
                    if (pli.Entry.Uuid.EqualsValue(puSearch))
                        return lvi;
                }

                return null;
            }

            // Adapted from KeePass
            public static void SelectEntries(PwObjectList<PwEntry> lEntries, bool bDeselectOthers, bool bFocusFirst)
            {
                bool bFirst = true;
                for (int i = 0; i < m_lvEntries.Items.Count; ++i)
                {
                    PwEntry pe = ((PwListItem)m_lvEntries.Items[i].Tag).Entry;
                    if (pe == null) { Debug.Assert(false); continue; }

                    bool bFound = false;
                    foreach (PwEntry peFocus in lEntries)
                    {
                        if (pe == peFocus)
                        {
                            m_lvEntries.Items[i].Selected = true;

                            if (bFirst && bFocusFirst)
                            {
                                m_lvEntries.Items[i].Focused = true;
                                bFirst = false;
                            }

                            bFound = true;
                            break;
                        }
                    }

                    if (bDeselectOthers && !bFound)
                        m_lvEntries.Items[i].Selected = false;
                }
            }

            // Adapted from KeePass
            internal static void SelectEntry(PwEntry entry, bool bDeselectOthers, bool bFocusFirst)
            {
                bool bFirst = true;
                for (int i = 0; i < m_lvEntries.Items.Count; ++i)
                {
                    PwEntry pe = ((PwListItem)m_lvEntries.Items[i].Tag).Entry;
                    if (pe == null) { Debug.Assert(false); continue; }

                    bool bFound = false;

                    if (pe == entry)
                    {
                        m_lvEntries.Items[i].Selected = true;

                        if (bFirst && bFocusFirst)
                        {
                            m_lvEntries.Items[i].Focused = true;
                            bFirst = false;
                        }

                        bFound = true;
                    }

                    if (bDeselectOthers && !bFound)
                        m_lvEntries.Items[i].Selected = false;
                }
            }

            internal static void UpdateSaveState()
            {
                // Update toolbar icons
                PwDatabase pwDb = m_host.MainWindow.DocumentManager.ActiveDatabase;
                m_host.MainWindow.UpdateUI(false, null, pwDb.UINeedsIconUpdate, null, false, null, true);
                //m_host.MainWindow.UpdateUI(false, null, false, null, false, null, true);
            }

            // Adapted from KeePass because it is private
            internal static string GetEntryFieldEx(PwEntry pe, int iColumnID, bool bFormatForDisplay, out bool bRequestAsync)
            {
                bRequestAsync = false;
                return GetEntryFieldEx(pe, iColumnID, bFormatForDisplay);
            }
            internal static string GetEntryFieldEx(PwEntry pe, int iColumnID, bool bFormatForDisplay)
            {
                // Adapted variables
                string m_strNeverExpiresText = KPRes.NeverExpires;

                List<AceColumn> l = Program.Config.MainWindow.EntryListColumns;
                if ((iColumnID < 0) || (iColumnID >= l.Count)) { Debug.Assert(false); return string.Empty; }

                AceColumn col = l[iColumnID];
                // TODO Optionally PasswordChar *** during editing for protected strings
                if (bFormatForDisplay && col.HideWithAsterisks) return PwDefs.HiddenPassword;

                string str;
                switch (col.Type)
                {
                    case AceColumnType.Title: str = pe.Strings.ReadSafe(PwDefs.TitleField); break;
                    case AceColumnType.UserName: str = pe.Strings.ReadSafe(PwDefs.UserNameField); break;
                    case AceColumnType.Password: str = pe.Strings.ReadSafe(PwDefs.PasswordField); break;
                    case AceColumnType.Url: str = pe.Strings.ReadSafe(PwDefs.UrlField); break;
                    case AceColumnType.Notes:
                        if (!bFormatForDisplay) str = pe.Strings.ReadSafe(PwDefs.NotesField);
                        else str = StrUtil.MultiToSingleLine(pe.Strings.ReadSafe(PwDefs.NotesField));
                        break;
                    case AceColumnType.CreationTime: str = TimeUtil.ToDisplayString(pe.CreationTime); break;
                    case AceColumnType.LastAccessTime: str = TimeUtil.ToDisplayString(pe.LastAccessTime); break;
                    case AceColumnType.LastModificationTime: str = TimeUtil.ToDisplayString(pe.LastModificationTime); break;
                    case AceColumnType.ExpiryTime:
                        if (pe.Expires) str = TimeUtil.ToDisplayString(pe.ExpiryTime);
                        else str = m_strNeverExpiresText;
                        break;
                    case AceColumnType.Uuid: str = pe.Uuid.ToHexString(); break;
                    case AceColumnType.Attachment: str = pe.Binaries.KeysToString(); break;
                    case AceColumnType.CustomString:
                        if (!bFormatForDisplay) str = pe.Strings.ReadSafe(col.CustomName);
                        else str = StrUtil.MultiToSingleLine(pe.Strings.ReadSafe(col.CustomName));
                        break;
                    case AceColumnType.PluginExt:
                        if (!bFormatForDisplay) str = Program.ColumnProviderPool.GetCellData(col.CustomName, pe);
                        else str = StrUtil.MultiToSingleLine(Program.ColumnProviderPool.GetCellData(col.CustomName, pe));
                        break;
                    case AceColumnType.OverrideUrl: str = pe.OverrideUrl; break;
                    case AceColumnType.Tags:
                        str = StrUtil.TagsToString(pe.Tags, true);
                        break;
                    case AceColumnType.ExpiryTimeDateOnly:
                        if (pe.Expires) str = TimeUtil.ToDisplayStringDateOnly(pe.ExpiryTime);
                        else str = m_strNeverExpiresText;
                        break;
                    case AceColumnType.Size:
                        str = StrUtil.FormatDataSizeKB(pe.GetSize());
                        break;
                    case AceColumnType.HistoryCount:
                        str = pe.History.UCount.ToString();
                        break;
                    default: Debug.Assert(false); str = string.Empty; break;
                }

                return str;
            }
     
            // Ported from KeePass MainForm OnEntryAdd
            internal static PwEntry CreateEntry(PwGroup pg)
            {
                Debug.Assert(pg != null); if (pg == null) return null;

                if (pg.IsVirtual)
                {
                    MessageService.ShowWarning(KPRes.GroupCannotStoreEntries,
                        KPRes.SelectDifferentGroup);
                    return null;
                }

                PwDatabase pd = m_host.MainWindow.DocumentManager.ActiveDatabase;
                if (pd == null) { Debug.Assert(false); return null; }
                if (pd.IsOpen == false) { Debug.Assert(false); return null; }

                PwEntry pe = new PwEntry(true, true);
                pe.CreationTime = pe.LastModificationTime = pe.LastAccessTime = DateTime.Now;
                //pwe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
                //    pd.MemoryProtection.ProtectUserName,
                //    pd.DefaultUserName));

                //ProtectedString psAutoGen = new ProtectedString(pd.MemoryProtection.ProtectPassword);
                //PwGenerator.Generate(psAutoGen, Program.Config.PasswordGenerator.AutoGeneratedPasswordsProfile,
                //    null, Program.PwGeneratorPool);
                //pe.Strings.Set(PwDefs.PasswordField, psAutoGen);

                int nExpireDays = Program.Config.Defaults.NewEntryExpiresInDays;
                if (nExpireDays >= 0)
                {
                    pe.Expires = true;
                    pe.ExpiryTime = DateTime.Now.AddDays(nExpireDays);
                }

                if ((pg.IconId != PwIcon.Folder) && (pg.IconId != PwIcon.FolderOpen) &&
                    (pg.IconId != PwIcon.FolderPackage))
                {
                    pe.IconId = pg.IconId; // Inherit icon from group
                }
                pe.CustomIconUuid = pg.CustomIconUuid;

                // Add entry to group and inherit default auto-type sequence
                pg.AddEntry(pe, true);

                return pe;
            }

            // Ported from KeePass Mainform_functions AddEntriesToList
            internal static ListViewItem AddEntryToList(PwEntry pe)
            {
                if (pe == null) { Debug.Assert(false); return null; }

                // Adapted variables
                bool m_bEntryGrouping = m_lvEntries.ShowGroups;
                Color m_clrAlternateItemBgColor = UIUtil.GetAlternateColor(m_lvEntries.BackColor);
                ListViewGroup m_lvgLastEntryGroup = null;
                foreach (ListViewGroup lvg in m_lvEntries.Groups)
                {
                    if ((lvg.Tag as PwGroup) == pe.ParentGroup)
                        m_lvgLastEntryGroup = lvg;
                }

                //ListViewStateEx lvseCachedState = new ListViewStateEx(m_lvEntries);
                //foreach (PwEntry pe in vEntries)
                //if (pe == null) { Debug.Assert(false); continue; }

                if (m_bEntryGrouping)
                {
                    PwGroup pg = pe.ParentGroup;

                    foreach (ListViewGroup lvg in m_lvEntries.Groups)
                    {
                        PwGroup pgList = (lvg.Tag as PwGroup);
                        Debug.Assert(pgList != null);
                        if ((pgList != null) && (pg == pgList))
                        {
                            m_lvgLastEntryGroup = lvg;
                            break;
                        }
                    }
                }

                ListViewItem lvi = SetListEntry(pe, null);

                //Debug.Assert(lvseCachedState.CompareTo(m_lvEntries));

                m_lvEntries.Items.Add(lvi);
                //m_lvEntries.Items.Insert(iIndex, lvi);

                UIUtil.SetAlternatingBgColors(m_lvEntries, m_clrAlternateItemBgColor,
                    Program.Config.MainWindow.EntryListAlternatingBgColors);

                return lvi;
            }

            // Ported from KeePass Mainform_functions SetListEntry
            private static ListViewItem SetListEntry(PwEntry pe, ListViewItem lviTarget)
            {
                if (pe == null) { Debug.Assert(false); return null; }

                // Adapted variables
                bool m_bEntryGrouping = m_lvEntries.ShowGroups;
                DateTime m_dtCachedNow = DateTime.Now;
                Font m_fontExpired = FontUtil.CreateFont(m_lvEntries.Font, FontStyle.Strikeout);
                bool m_bShowTanIndices = Program.Config.MainWindow.TanView.ShowIndices;
                bool bSubEntries = Program.Config.MainWindow.ShowEntriesOfSubGroups;
                ListViewGroup m_lvgLastEntryGroup = null;
                foreach (ListViewGroup lvg in m_lvEntries.Groups)
                {
                    if ((lvg.Tag as PwGroup) == pe.ParentGroup)
                        m_lvgLastEntryGroup = lvg;
                }
                PwGroup pg = (Program.MainForm.GetSelectedGroup());
                PwObjectList<PwEntry> pwlSource = ((pg != null) ? pg.GetEntries(bSubEntries) : new PwObjectList<PwEntry>());
                bool m_bOnlyTans = ListContainsOnlyTans(pwlSource);

                ListViewItem lvi = (lviTarget ?? new ListViewItem());

                PwListItem pli = new PwListItem(pe);
                if (lviTarget == null) lvi.Tag = pli; // Lock below (when adding it)
                //else
                //{
                //    lock (m_asyncListUpdate.ListEditSyncObject) { lvi.Tag = pli; }
                //}

                int iIndexHint = ((lviTarget != null) ? lviTarget.Index :
                    m_lvEntries.Items.Count);

                if (pe.Expires && (pe.ExpiryTime <= m_dtCachedNow))
                {
                    lvi.ImageIndex = (int)PwIcon.Expired;
                    if (m_fontExpired != null) lvi.Font = m_fontExpired;
                }
                else // Not expired
                {
                    // Reset font, if item was expired previously (i.e. has expired font)
                    if ((lviTarget != null) && (lvi.ImageIndex == (int)PwIcon.Expired))
                        lvi.Font = m_lvEntries.Font;

                    if (pe.CustomIconUuid.EqualsValue(PwUuid.Zero))
                        lvi.ImageIndex = (int)pe.IconId;
                    else
                        lvi.ImageIndex = (int)PwIcon.Count +
                            m_host.MainWindow.DocumentManager.ActiveDatabase.GetCustomIconIndex(pe.CustomIconUuid);
                }

                if (m_bEntryGrouping && (lviTarget == null))
                {
                    PwGroup pgContainer = pe.ParentGroup;
                    PwGroup pgLast = ((m_lvgLastEntryGroup != null) ?
                        (PwGroup)m_lvgLastEntryGroup.Tag : null);

                    Debug.Assert(pgContainer != null);
                    if (pgContainer != null)
                    {
                        if (pgContainer != pgLast)
                        {
                            m_lvgLastEntryGroup = new ListViewGroup(
                                pgContainer.GetFullPath());
                            m_lvgLastEntryGroup.Tag = pgContainer;

                            m_lvEntries.Groups.Add(m_lvgLastEntryGroup);
                        }

                        lvi.Group = m_lvgLastEntryGroup;
                    }
                }

                if (!pe.ForegroundColor.IsEmpty)
                    lvi.ForeColor = pe.ForegroundColor;
                else if (lviTarget != null) lvi.ForeColor = m_lvEntries.ForeColor;
                else { Debug.Assert(UIUtil.ColorsEqual(lvi.ForeColor, m_lvEntries.ForeColor)); }

                if (!pe.BackgroundColor.IsEmpty)
                    lvi.BackColor = pe.BackgroundColor;
                // else if(Program.Config.MainWindow.EntryListAlternatingBgColors &&
                //	((m_lvEntries.Items.Count & 1) == 1))
                //	lvi.BackColor = m_clrAlternateItemBgColor;
                else if (lviTarget != null) lvi.BackColor = m_lvEntries.BackColor;
                else { Debug.Assert(UIUtil.ColorsEqual(lvi.BackColor, m_lvEntries.BackColor)); }

                bool bAsync;

                // m_bOnlyTans &= PwDefs.IsTanEntry(pe);
                if (m_bShowTanIndices && m_bOnlyTans)
                {
                    string strIndex = pe.Strings.ReadSafe(PwDefs.TanIndexField);

                    if (strIndex.Length > 0) lvi.Text = strIndex;
                    else lvi.Text = PwDefs.TanTitle;
                }
                else
                {
                    string strMain = GetEntryFieldEx(pe, 0, true, out bAsync);
                    lvi.Text = strMain;
                    //if (bAsync)
                    //    m_asyncListUpdate.Queue(strMain, pli, iIndexHint, 0,
                    //        AsyncPwListUpdate.SprCompileFn);
                }

                int nColumns = m_lvEntries.Columns.Count;
                if (lviTarget == null)
                {
                    for (int iColumn = 1; iColumn < nColumns; ++iColumn)
                    {
                        string strSub = GetEntryFieldEx(pe, iColumn, true, out bAsync);
                        lvi.SubItems.Add(strSub);
                        //if (bAsync)
                        //    m_asyncListUpdate.Queue(strSub, pli, iIndexHint, iColumn,
                        //        AsyncPwListUpdate.SprCompileFn);
                    }
                }
                else
                {
                    int nSubItems = lvi.SubItems.Count;
                    for (int iColumn = 1; iColumn < nColumns; ++iColumn)
                    {
                        string strSub = GetEntryFieldEx(pe, iColumn, true, out bAsync);

                        if (iColumn < nSubItems) lvi.SubItems[iColumn].Text = strSub;
                        else lvi.SubItems.Add(strSub);

                        //if (bAsync)
                        //    m_asyncListUpdate.Queue(strSub, pli, iIndexHint, iColumn,
                        //        AsyncPwListUpdate.SprCompileFn);
                    }

                    Debug.Assert(lvi.SubItems.Count == nColumns);
                }

                if (lviTarget == null)
                {
                    //lock (m_asyncListUpdate.ListEditSyncObject)
                    //{
                    //    m_lvEntries.Items.Add(lvi);
                    //}
                }
                return lvi;
            }

            // Ported from KeePass Mainform_functions SetListEntry
            internal static ListViewItem InsertListEntryOLD(PwEntry pe, int iIndex)
            {
                // Adapted variables
                DateTime m_dtCachedNow = DateTime.Now;
                Font m_fontExpired = FontUtil.CreateFont(m_lvEntries.Font, FontStyle.Strikeout);
                bool m_bEntryGrouping = m_lvEntries.ShowGroups;
                bool m_bShowTanIndices = Program.Config.MainWindow.TanView.ShowIndices;
                bool bSubEntries = Program.Config.MainWindow.ShowEntriesOfSubGroups;
                ListViewGroup m_lvgLastEntryGroup = null;
                foreach (ListViewGroup lvg in m_lvEntries.Groups)
                {
                    if ((lvg.Tag as PwGroup) == pe.ParentGroup)
                        m_lvgLastEntryGroup = lvg;
                }
                PwGroup pg = (Program.MainForm.GetSelectedGroup());
                PwObjectList<PwEntry> pwlSource = ((pg != null) ? pg.GetEntries(bSubEntries) : new PwObjectList<PwEntry>());
                bool m_bOnlyTans = ListContainsOnlyTans(pwlSource);

                ListViewItem lviTarget = null;
                Color m_clrAlternateItemBgColor = UIUtil.GetAlternateColor(m_lvEntries.BackColor);

                if (pe == null) { Debug.Assert(false); return null; }

                ListViewItem lvi = (lviTarget ?? new ListViewItem());
                PwListItem pli = new PwListItem(pe);
                lvi.Tag = pli;

                //lvi.BeginUpdate();

                if (pe.Expires && (pe.ExpiryTime <= m_dtCachedNow))
                {
                    lvi.ImageIndex = (int)PwIcon.Expired;
                    if (m_fontExpired != null) lvi.Font = m_fontExpired;
                }
                else // Not expired
                {
                    // Reset font, if item was expired previously (i.e. has expired font)
                    if ((lviTarget != null) && (lvi.ImageIndex == (int)PwIcon.Expired))
                        lvi.Font = m_lvEntries.Font;

                    if (pe.CustomIconUuid.EqualsValue(PwUuid.Zero))
                        lvi.ImageIndex = (int)pe.IconId;
                    else
                        lvi.ImageIndex = (int)PwIcon.Count +
                            m_host.MainWindow.DocumentManager.ActiveDatabase.GetCustomIconIndex(pe.CustomIconUuid);
                }

                if (m_bEntryGrouping && (lviTarget == null))
                {
                    PwGroup pgContainer = pe.ParentGroup;
                    PwGroup pgLast = ((m_lvgLastEntryGroup != null) ?
                        (PwGroup)m_lvgLastEntryGroup.Tag : null);

                    Debug.Assert(pgContainer != null);
                    if (pgContainer != null)
                    {
                        if (pgContainer != pgLast)
                        {
                            m_lvgLastEntryGroup = new ListViewGroup(
                                pgContainer.GetFullPath());
                            m_lvgLastEntryGroup.Tag = pgContainer;

                            m_lvEntries.Groups.Add(m_lvgLastEntryGroup);
                        }

                        lvi.Group = m_lvgLastEntryGroup;
                    }
                }

                if (!pe.ForegroundColor.IsEmpty)
                    lvi.ForeColor = pe.ForegroundColor;
                else if (lviTarget != null) lvi.ForeColor = m_lvEntries.ForeColor;
                else { Debug.Assert(UIUtil.ColorsEqual(lvi.ForeColor, m_lvEntries.ForeColor)); }

                if (!pe.BackgroundColor.IsEmpty)
                    lvi.BackColor = pe.BackgroundColor;
                // else if(Program.Config.MainWindow.EntryListAlternatingBgColors &&
                //	((m_lvEntries.Items.Count & 1) == 1))
                //	lvi.BackColor = m_clrAlternateItemBgColor;
                else if (lviTarget != null) lvi.BackColor = m_lvEntries.BackColor;
                else { Debug.Assert(UIUtil.ColorsEqual(lvi.BackColor, m_lvEntries.BackColor)); }

                // m_bOnlyTans &= PwDefs.IsTanEntry(pe);
                if (m_bShowTanIndices && m_bOnlyTans)
                {
                    string strIndex = pe.Strings.ReadSafe(PwDefs.TanIndexField);

                    if (strIndex.Length > 0) lvi.Text = strIndex;
                    else lvi.Text = PwDefs.TanTitle;
                }
                else lvi.Text = GetEntryFieldEx(pe, 0, true);

                int nColumns = m_lvEntries.Columns.Count;
                if (lviTarget == null)
                {
                    for (int iColumn = 1; iColumn < nColumns; ++iColumn)
                        lvi.SubItems.Add(GetEntryFieldEx(pe, iColumn, true));
                }
                else
                {
                    int nSubItems = lvi.SubItems.Count;
                    for (int iColumn = 1; iColumn < nColumns; ++iColumn)
                    {
                        string strSub = GetEntryFieldEx(pe, iColumn, true);
                        if (iColumn < nSubItems) lvi.SubItems[iColumn].Text = strSub;
                        else lvi.SubItems.Add(strSub);
                    }

                    Debug.Assert(lvi.SubItems.Count == nColumns);
                }

                //if (lviTarget == null) m_lvEntries.Items.Add(lvi);
                if (lviTarget == null) m_lvEntries.Items.Insert(iIndex, lvi);

                UIUtil.SetAlternatingBgColors(m_lvEntries, m_clrAlternateItemBgColor,
                    Program.Config.MainWindow.EntryListAlternatingBgColors);

                //lvi.EndUpdate();

                return lvi;
            }

            // Ported from KeePass because it is private
            private static bool ListContainsOnlyTans(PwObjectList<PwEntry> vEntries)
            {
                if (vEntries == null) { Debug.Assert(false); return true; }

                foreach (PwEntry pe in vEntries)
                {
                    if (!PwDefs.IsTanEntry(pe)) return false;
                }

                return true;
            }

            internal static int LviCompareByIndex(ListViewItem a, ListViewItem b)
            {
                return a.Index.CompareTo(b.Index);
            }
        }
    }
}

