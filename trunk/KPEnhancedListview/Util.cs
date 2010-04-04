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

        /// <summary>
        /// Finds a Control recursively. Note finds the first match and exists
        /// </summary>
        /// <param name="container">The container to search for the control passed. Remember
        /// all controls (Panel, GroupBox, Form, etc are all containsers for controls
        /// </param>
        /// <param name="name">Name of the control to look for</param>
        /// <returns>The Control we found</returns>
        private Control FindControlRecursive(Control container, string name)
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

        // Multiline string to oneline string
        private string StringToOneLine(string Text, int SubItem)
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

        // String to multiline string
        private string StringToMultiLine(string Text, int SubItem)
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

        /*
        // Adapted from KeePass
        private void EnsureVisibleEntry(PwUuid uuid)
        {
            ListViewItem lvi = GuiFindEntry(uuid);
            if (lvi == null) { Debug.Assert(false); return; }

            m_clveEntries.EnsureVisible(lvi.Index);
        }

        // Adapted from KeePass
        private ListViewItem GuiFindEntry(PwUuid puSearch)
        {
            Debug.Assert(puSearch != null);
            if (puSearch == null) return null;

            foreach (ListViewItem lvi in m_clveEntries.Items)
            {
                if (((PwEntry)lvi.Tag).Uuid.EqualsValue(puSearch))
                    return lvi;
            }

            return null;
        }
        */
    }
}
