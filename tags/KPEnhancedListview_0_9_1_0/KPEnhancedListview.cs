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
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Security;
using KeePassLib.Utility;

// The namespace name must be the same as the filename of the
// plugin without its extension.
namespace KPEnhancedListview
{
    /// <summary>
    /// This is the main plugin class. 
    /// It should be named something like the namespace+Ext and 
    /// must be derived from
    /// <c>KeePass.Plugin</c>.
    /// </summary>
    public sealed partial class KPEnhancedListviewExt : Plugin
    {
        // The plugin remembers its host in this variable.
        public static IPluginHost m_host = null;

        public static ToolStripItemCollection m_tsMenu = null;
        public static ToolStripMenuItem m_tsPopup = null;

        private ToolStripSeparator m_tsSeparator = null;

        private const string m_ctseName = "m_toolMain";
        private const string m_clveName = "m_lvEntries";
        //private const string m_ctveName = "m_tvGroups";
        //private const string m_csceName = "m_splitVertical";

        public static CustomToolStripEx m_ctseToolMain = null;
        public static CustomListViewEx m_clveEntries = null;
        //private CustomTreeViewEx m_ctveGroups = null;
        //private CustomSplitContainerEx m_csceSplitVertical = null;

        private EventSuppressor m_evEntries = null;
        //private EventSuppressor m_evMainWindow = null;

        // Mouse handler helper for detecting InlineEditing and AddEntry
        private const int m_mouseTimeMin = 3000000;
        private const int m_mouseTimeMax = 10000000;

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
            m_tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;

            // Add a separator at the bottom
            m_tsSeparator = new ToolStripSeparator();
            m_tsMenu.Add(m_tsSeparator);

            m_tsPopup = new ToolStripMenuItem();
            m_tsPopup.Text = "KPEnhancedListview";
            //m_tsMenu.ToolTipText = tbToolTip;
            m_tsMenu.Add(m_tsPopup);
            
            // We want a notification when the user tried to save the current database
            m_host.MainWindow.FileSaved += OnFileSaved;

            // Find the listview control 
            m_ctseToolMain = (CustomToolStripEx)Util.FindControlRecursive(m_host.MainWindow, m_ctseName);
            m_clveEntries = (CustomListViewEx)Util.FindControlRecursive(m_host.MainWindow, m_clveName);
            //m_tsmiMenuView = (ToolStripMenuItem)Util.FindControlRecursive(m_host.MainWindow, m_tsmiName);
            //m_ctveGroups = (CustomTreeViewEx)Util.FindControlRecursive(m_host.MainWindow, m_ctveName);
            //m_csceSplitVertical = (CustomSplitContainerEx)Util.FindControlRecursive(m_host.MainWindow, m_csceName);

            // Initialize EventSuppressor
            m_evEntries = new EventSuppressor(m_clveEntries);
            //m_evMainWindow = new EventSuppressor(m_host.MainWindow); //(Control)this);//(Control)m_host.MainWindow);

            // Initialize Inline Editing
            //InitializeInlineEditing();
            new KPEnhancedListviewInlineEditing();

            // Initialize add new entry on double click function
            //InitializeAddEntry();
            new KPEnhancedListviewAddEntry();

            new KPEnhancedListviewOpenDirectory();

            return true; // Initialization successful
        }

        private void OnFileSaved(object sender, FileSavedEventArgs e)
        {
            // nothing todo
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
            m_tsMenu.Remove(m_tsSeparator);

            // Important! Remove event handlers!
            m_host.MainWindow.FileSaved -= OnFileSaved;

            // Undo OnMenuInlineEditing
            //TerminateInlineEditing();
            //KPEnhancedListviewInlineEditing

            // AddEntry
            //TerminateAddEntry();
            //KPEnhancedListviewAddEntry

            //KPEnhancedListviewOpenDirectory
        } 
    }
}
