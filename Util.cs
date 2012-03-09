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

                foreach (ColumnHeader ch in m_clveEntries.Columns)
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
            internal static int GetSubItemAt(CustomListViewEx clve, int x, int y, out ListViewItem item)
            {
                item = clve.GetItemAt(x, y);

                if (item != null)
                {
                    int[] order = GetColumnOrder(clve);
                    Rectangle lviBounds;
                    int subItemX;

                    lviBounds = item.GetBounds(ItemBoundsPortion.Entire);
                    subItemX = lviBounds.Left;
                    for (int i = 0; i < order.Length; i++)
                    {
                        ColumnHeader h = clve.Columns[order[i]];
                        if (x < subItemX + h.Width)
                        {
                            return h.Index;
                        }
                        subItemX += h.Width;
                    }
                }

                return -1;
            }

            // ListView messages constants
            private const int LVM_FIRST = 0x1000;
            private const int LVM_GETCOLUMNORDERARRAY = (LVM_FIRST + 59);

            // ListView send message
            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wPar, IntPtr lPar);

            /// <summary>
            /// Retrieve the order in which columns appear
            /// </summary>
            /// <returns>Current display order of column indices</returns>
            internal static int[] GetColumnOrder(CustomListViewEx clve)
            {
                IntPtr lPar = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * clve.Columns.Count);

                IntPtr res = SendMessage(clve.Handle, LVM_GETCOLUMNORDERARRAY, new IntPtr(clve.Columns.Count), lPar);
                if (res.ToInt32() == 0)	// Something went wrong
                {
                    Marshal.FreeHGlobal(lPar);
                    return null;
                }

                int[] order = new int[clve.Columns.Count];
                Marshal.Copy(lPar, order, 0, clve.Columns.Count);
                Marshal.FreeHGlobal(lPar);

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

            // Multiline string to oneline string
            internal static string StringToOneLine(string Text, int SubItem)
            {
                int colID = SubItem;
                AceColumn col = Util.GetAceColumn(colID);
                AceColumnType colType = col.Type;
                switch (colType)
                {
                    case AceColumnType.Notes:
                    case AceColumnType.CustomString:
                        // Keepass specific
#if USE_RTB   // RichTextBox \r \n \r\n
                        // No changes
#else         // TextBox needs \r\n
                        Text = Text.Replace("\r", string.Empty).Replace("\n", " ");
#endif
                        break;
                    default:
                        // No changes
                        break;
                }
                return Text;
            }

            // String to multiline string
            internal static string StringToMultiLine(string Text, int SubItem)
            {
                int colID = SubItem;
                AceColumn col = Util.GetAceColumn(colID);
                AceColumnType colType = col.Type;
                switch (colType)
                {
                    case AceColumnType.Notes:
                    case AceColumnType.CustomString:
                        // Keepass specific
                        // Use Environment.NewLine
#if USE_RTB   // RichTextBox \r \n \r\n
                        // No changes
#else         // TextBox needs \r\n
                        Text = Text.Replace("\r", "\r\n").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n");
#endif
                        break;
                    default:
                        // No changes
                        break;
                }
                return Text;
            }

            // Adapted from KeePass because it is private
            internal static string GetEntryFieldEx(PwEntry pe, int iColumnID)
            {
                List<AceColumn> l = Program.Config.MainWindow.EntryListColumns;
                if ((iColumnID < 0) || (iColumnID >= l.Count)) { Debug.Assert(false); return string.Empty; }

                AceColumn col = l[iColumnID];
                if (col.HideWithAsterisks) return PwDefs.HiddenPassword;

                string str = string.Empty;
                switch (col.Type)
                {
                    case AceColumnType.Title: str = pe.Strings.ReadSafe(PwDefs.TitleField); break;
                    case AceColumnType.UserName: str = pe.Strings.ReadSafe(PwDefs.UserNameField); break;
                    case AceColumnType.Password: str = pe.Strings.ReadSafe(PwDefs.PasswordField); break;
                    case AceColumnType.Url: str = pe.Strings.ReadSafe(PwDefs.UrlField); break;
                    case AceColumnType.Notes: str = pe.Strings.ReadSafe(PwDefs.NotesField); break;
                    case AceColumnType.CreationTime: str = TimeUtil.ToDisplayString(pe.CreationTime); break;
                    case AceColumnType.LastAccessTime: str = TimeUtil.ToDisplayString(pe.LastAccessTime); break;
                    case AceColumnType.LastModificationTime: str = TimeUtil.ToDisplayString(pe.LastModificationTime); break;
                    case AceColumnType.ExpiryTime:
                        if (pe.Expires) str = TimeUtil.ToDisplayString(pe.ExpiryTime);
                        else str = KPRes.NeverExpires; //m_strNeverExpiresText;
                        break;
                    case AceColumnType.Uuid: str = pe.Uuid.ToHexString(); break;
                    case AceColumnType.Attachment: str = pe.Binaries.KeysToString(); break;
                    case AceColumnType.CustomString:
                        str = pe.Strings.ReadSafe(col.CustomName);
                        break;
                    case AceColumnType.PluginExt:
                        str = Program.ColumnProviderPool.GetCellData(col.CustomName, pe);
                        break;
                    case AceColumnType.OverrideUrl: str = pe.OverrideUrl; break;
                    case AceColumnType.Tags:
                        str = StrUtil.TagsToString(pe.Tags, true);
                        break;
                    case AceColumnType.ExpiryTimeDateOnly:
                        if (pe.Expires) str = TimeUtil.ToDisplayStringDateOnly(pe.ExpiryTime);
                        else str = KPRes.NeverExpires; //m_strNeverExpiresText;
                        break;
                    case AceColumnType.Size:
                        str = StrUtil.FormatDataSizeKB(pe.GetSize());
                        break;
                    case AceColumnType.HistoryCount:
                        str = pe.History.UCount.ToString();
                        break;
                    default: Debug.Assert(false); break;
                }

                return str;
            }

            // Ported from KeePass Entry Dialog SaveEntry() and UpdateEntryStrings(...)
            internal static bool SaveEntry(PwDatabase pwStorage, ListViewItem Item, int SubItem, string Text)
            {
                PwEntry pe = (PwEntry)Item.Tag;
                pe = pwStorage.RootGroup.FindEntry(pe.Uuid, true);

                PwEntry peInit = pe.CloneDeep();
                pe.CreateBackup();
                pe.Touch(true, false); // Touch *after* backup

#if !USE_RTB
                Text = Util.StringToOneLine(Text, SubItem);
#endif

                int colID = SubItem;
                AceColumn col = Util.GetAceColumn(colID);
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

                if (pe.EqualsEntry(peInit, false, true, true, false, true))
                {
                    pe.LastModificationTime = peInit.LastModificationTime;

                    pe.History.Remove(pe.History.GetAt(pe.History.UCount - 1)); // Undo backup

                    return false;
                }
                else
                {
                    //UpdateSaveIcon();

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

            // Adapted from KeePass
            internal static void EnsureVisibleEntry(CustomListViewEx clve, PwUuid uuid)
            {
                ListViewItem lvi = GuiFindEntry(clve, uuid);
                if (lvi == null) { Debug.Assert(false); return; }

                clve.EnsureVisible(lvi.Index);
            }

            // Adapted from KeePass
            internal static ListViewItem GuiFindEntry(CustomListViewEx clve, PwUuid puSearch)
            {
                Debug.Assert(puSearch != null);
                if (puSearch == null) return null;

                foreach (ListViewItem lvi in clve.Items)
                {
                    if (((PwEntry)lvi.Tag).Uuid.EqualsValue(puSearch))
                        return lvi;
                }

                return null;
            }

            // Adapted from KeePass - actually not used
            public static void SelectEntries(CustomListViewEx clve, PwObjectList<PwEntry> lEntries, bool bDeselectOthers)
            {
                for (int i = 0; i < clve.Items.Count; ++i)
                {
                    PwEntry pe = (clve.Items[i].Tag as PwEntry);
                    if (pe == null) { Debug.Assert(false); continue; }

                    bool bFound = false;
                    foreach (PwEntry peFocus in lEntries)
                    {
                        if (pe == peFocus)
                        {
                            clve.Items[i].Selected = true;
                            bFound = true;
                            break;
                        }
                    }

                    if (bDeselectOthers && !bFound)
                        clve.Items[i].Selected = false;
                }
            }

            // Adapted from KeePass
            internal static void SelectEntry(CustomListViewEx clve, PwEntry entry, bool bDeselectOthers)
            {
                for (int i = 0; i < clve.Items.Count; ++i)
                {
                    PwEntry pe = (clve.Items[i].Tag as PwEntry);
                    if (pe == null) { Debug.Assert(false); continue; }

                    bool bFound = false;

                    if (pe == entry)
                    {
                        clve.Items[i].Selected = true;
                        bFound = true;
                    }

                    if (bDeselectOthers && !bFound)
                        clve.Items[i].Selected = false;
                }
            }

            internal static void UpdateSaveIcon()
            {
                // Update toolbar save icon
                m_host.MainWindow.UpdateUI(false, null, false, null, false, null, true);
            }
        }
    }
}
