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
    /// This is the main plugin class. It must be named exactly
    /// like the namespace and must be derived from
    /// <c>KeePassPlugin</c>.
    /// </summary>
    public sealed partial class KPEnhancedListviewExt : Plugin
    {
        // The plugin remembers its host in this variable.
        private IPluginHost m_host = null;
         
        private ToolStripSeparator m_tsSeparator = null;
        private ToolStripMenuItem m_tsmiAddEntry = null;
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
            ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;

            // Add a separator at the bottom
            m_tsSeparator = new ToolStripSeparator();
            tsMenu.Add(m_tsSeparator);

            // Add menu item
            m_tsmiAddEntry = new ToolStripMenuItem();
            m_tsmiAddEntry.Text = "Double Click add an Entry";
            m_tsmiAddEntry.Click += OnMenuAddEntry;
            tsMenu.Add(m_tsmiAddEntry);
            
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
            InitializeInlineEditing();

            // Initialize Custom columns
            InitializeCustomColumns();

            // Initialize add new entry on double click function
            InitializeAddEntry();

            // Tell windows we are interested in drawing items in ListBox on our own
            m_clveEntries.OwnerDraw = true;
            m_clveEntries.DrawItem += new DrawListViewItemEventHandler(this.DrawItemHandler);
            m_clveEntries.DrawSubItem += new DrawListViewSubItemEventHandler(this.DrawSubItemHandler);
            m_clveEntries.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(this.DrawColumnHeaderHandler);

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
            ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;
            tsMenu.Remove(m_tsSeparator);
            tsMenu.Remove(m_tsmiAddEntry);
            tsMenu.Remove(m_tsmiInlineEditing);
            tsMenu.Remove(m_tsmiAddCustomColumns);
            tsMenu.Remove(m_tsmiRemoveCustomColumns);

            // Important! Remove event handlers!
            m_host.MainWindow.FileSaved -= OnFileSaved;

            m_clveEntries.DrawItem -= new DrawListViewItemEventHandler(this.DrawItemHandler);
            m_clveEntries.DrawSubItem -= new DrawListViewSubItemEventHandler(this.DrawSubItemHandler);
            m_clveEntries.DrawColumnHeader -= new DrawListViewColumnHeaderEventHandler(this.DrawColumnHeaderHandler);
            
            // Undo Initialize
            TerminateCustomColumns();

            // Undo OnMenuInlineEditing
            TerminateInlineEditing();
            
            // AddEntry
            TerminateAddEntry();
        }

        //
        // Shared functions
        //
        private void DrawItemHandler(object sender, DrawListViewItemEventArgs e)
        {
            // CustomColumns
            // Should be not necessary only to avoid issues
            if (m_lCustomColums.Count != 0)
            {
                if (m_clveEntries.Items.Count != 0)
                {
//TODO update and sort very slow ???
                    //if (m_clveEntries.Items[0].SubItems.Count != m_clveEntries.Columns.Count)
                    //if (m_clveEntries.Items[m_clveEntries.Items.Count-1].SubItems.Count != m_clveEntries.Columns.Count)
                    {
// TODO cancel criterias ?
/*
                        // Stable but slow
                        UpdateListView();

                        // Faster but it flickrs
                        //UpdateListViewItem(e.Item);

                        SortListView();
*/ 
                    }
                }
            }
            
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
