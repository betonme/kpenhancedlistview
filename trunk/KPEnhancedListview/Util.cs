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
        internal static List<string> GetListKeePassColumns(CustomListViewEx clve)
        {
            List<string> strl = new List<string>();

            foreach (ColumnHeader ch in clve.Columns)
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
                case AppDefs.ColumnId.Uuid:
                case AppDefs.ColumnId.Attachment:
                    // No changes
                    return Text;
                case AppDefs.ColumnId.Notes:
                default:
                    // Keepass specific
                    return Text.Replace("\r", string.Empty).Replace("\n", " ");
            }
        }

        // String to multiline string
        internal static string StringToMultiLine(string Text, int SubItem)
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
                case AppDefs.ColumnId.Uuid:
                case AppDefs.ColumnId.Attachment:
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

        // Adapted from KeePass
        /*
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
        }*/

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
    }
}
