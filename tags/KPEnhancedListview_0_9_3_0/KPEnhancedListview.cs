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
//#if !USE_NET20
//  using System.Linq;
//#endif

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
        //public static DocumentManagerEx m_docMgr = null;

        public static ToolStripItemCollection m_tsMenu = null;
        public static ToolStripMenuItem m_tsPopup = null;

        //private const string m_ctseName = "m_toolMain";
        private const string m_clveName = "m_lvEntries";
        //private const string m_ctveName = "m_tvGroups";
        //private const string m_csceName = "m_splitVertical";

        //public static CustomToolStripEx m_toolMain = null;
        public static CustomListViewEx m_lvEntries = null;
        //public static CustomTreeViewEx m_tvGroups = null;
        //private CustomSplitContainerEx m_csceSplitVertical = null;

        // Declaration Sub Plugins
        public static KPEnhancedListviewInlineEditing KPELInlineEditing = null;
        public static KPEnhancedListviewAddEntry KPELAddEntry = null;
        public static KPEnhancedListviewOpenGroup KPELOpenDirecotory = null;
		public static KPEnhancedListviewEditableNotes KPELEditableNotes = null;

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
            //m_docMgr = host.MainWindow.DocumentManager;

            // Get a reference to the 'Tools' menu item container
            m_tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;

            // Add a separator at the bottom
            ToolStripSeparator tsSeparator = new ToolStripSeparator();
            m_tsMenu.Add(tsSeparator);

            m_tsPopup = new ToolStripMenuItem();
            m_tsPopup.Text = "KPEnhancedListview";
            //m_tsMenu.ToolTipText = tbToolTip;
            m_tsMenu.Add(m_tsPopup);
            
            // We want a notification when the user tried to save the current database
            m_host.MainWindow.FileSaved += OnFileSaved;

            // Find the listview control 
            //m_toolMain = (CustomToolStripEx)Util.FindControlRecursive(m_host.MainWindow, m_ctseName);
            m_lvEntries = (CustomListViewEx)Util.FindControlRecursive(m_host.MainWindow, m_clveName);
            //m_tsmiMenuView = (ToolStripMenuItem)Util.FindControlRecursive(m_host.MainWindow, m_tsmiName);
            //m_tvGroups = (CustomTreeViewEx)Util.FindControlRecursive(m_host.MainWindow, m_ctveName);
            //m_csceSplitVertical = (CustomSplitContainerEx)Util.FindControlRecursive(m_host.MainWindow, m_csceName);

            // Initialize Sub Plugins
            KPELInlineEditing = new KPEnhancedListviewInlineEditing();
            KPELAddEntry = new KPEnhancedListviewAddEntry();
            KPELOpenDirecotory = new KPEnhancedListviewOpenGroup();
			KPELEditableNotes = new KPEnhancedListviewEditableNotes(); 
            
            // Add About dialog

            // Add About dialog
            ToolStripMenuItem m_tbItem = null;
            m_tbItem = new ToolStripMenuItem();
            m_tbItem.Text = "About";
            m_tbItem.Image = Properties.Resources.B16x16_Help;
            m_tbItem.Click += OpenAbout;
            m_tsPopup.DropDownItems.Add(m_tbItem);

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
            m_tsMenu.Clear();

            // Important! Remove event handlers!
            m_host.MainWindow.FileSaved -= OnFileSaved;

            // Delete Sub plugins
            KPELInlineEditing = null;
            KPELAddEntry = null;
            KPELOpenDirecotory = null;
        }

        public override string UpdateUrl
        {
            get { return "http://kpenhancedlistview.googlecode.com/svn/trunk/KPEnhancedListview/VersionInformation.txt"; }
        }

        private void OpenAbout(object sender, EventArgs e)
        {
            AboutForm abf = new AboutForm();
            UIUtil.ShowDialogAndDestroy(abf);
        }
    }
}
