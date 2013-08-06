/*
    EditableNotes sub-plugin that enables directly editing Notes in the main window.
	Developed by Jure Vrhovnik <jure.vrhovnik(at)gmail.com>
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
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePassLib.Utility;


namespace KPEnhancedListview
{
	partial class KPEnhancedListviewExt
	{
		public class KPEnhancedListviewEditableNotes : SubPluginBase
		{

			//////////////////////////////////////////////////////////////
			// Sub Plugin setup
			private const string m_tbText = "Editable notes";
			private const string m_tbToolTip = "Enable directly editing Notes in the main window.";
			protected const string m_cfgString = "KPEnhancedListview_EditableNotes";
			public KPEnhancedListviewEditableNotes()
			{
				AddMenu(m_cfgString, m_tbText, m_tbToolTip);

				InitializeEditableNotes();
			}

            //////////////////////////////////////////////////////////////
            // Sub Plugin handler registration
            protected override void AddHandler()
            {
				// Find splitter containing original Entry View that will be replaced by editable notes control
				m_splitHorizontal = (CustomSplitContainerEx)Util.FindControlRecursive(m_host.MainWindow, m_splitHorizontalName);

				if (m_splitHorizontal == null)
				{
					ShowPluginError();
				}
				else
				{
					// Find original Entry View control
					Control[] ctls = m_splitHorizontal.Panel2.Controls.Find(m_richEntryViewName, true);
					if (ctls == null || ctls.Length == 0)
					{
						ShowPluginError();
					}
					else
					{
						m_richEntryView = ctls[0];

						// Replace the original entry view with editable notes textbox
						m_richEntryView.Visible = false;
						m_splitHorizontal.Panel2.Controls.Add(txtNotes); // Add new textbox control
						txtNotes.Visible = true;
						
						//m_splitHorizontal.Panel2.Controls.RemoveAt(0); // Remove original control

						// Add events
						m_lvEntries.SelectedIndexChanged += m_lvEntries_SelectedIndexChanged;
						txtNotes.Enter += txtNotes_Enter;
						txtNotes.Leave += txtNotes_Leave;
						txtNotes.LinkClicked += txtNotes_LinkClicked;
						m_host.MainWindow.UIStateUpdated += MainWindow_UIStateUpdated;

						// Refresh editable notes content
						RefreshEditableNotes();
					}
				}
            }

            protected override void RemoveHandler()
            {
				m_splitHorizontal = (CustomSplitContainerEx)Util.FindControlRecursive(m_host.MainWindow, m_splitHorizontalName);

				if (m_splitHorizontal != null)
				{
					Control[] ctls = m_splitHorizontal.Panel2.Controls.Find(m_richEntryViewName, true);
					if (ctls != null && ctls.Length > 0)
					{
						m_richEntryView = ctls[0];
						// Replace editable notes textbox with the original entry view
						//m_splitHorizontal.Panel2.Controls.Remove(txtNotes);
						//m_splitHorizontal.Panel2.Controls.Add(m_richEntryView); // Add new textbox control
						txtNotes.Visible = false;
						m_richEntryView.Visible = true;
					}
				}

				// Remove events
				m_lvEntries.SelectedIndexChanged -= m_lvEntries_SelectedIndexChanged;
				txtNotes.Enter -= txtNotes_Enter;
				txtNotes.Leave -= txtNotes_Leave;
				txtNotes.LinkClicked -= txtNotes_LinkClicked;
				m_host.MainWindow.UIStateUpdated -= MainWindow_UIStateUpdated;
            }

            //////////////////////////////////////////////////////////////
            // Sub Plugin functionality


			private const string m_splitHorizontalName = "m_splitHorizontal";
			private const string m_richEntryViewName = "m_richEntryView";
			private const string m_txtNotesName = "m_txtNotesName";

			public CustomSplitContainerEx m_splitHorizontal = null;
			public Control m_richEntryView;
			public CustomRichTextBoxEx txtNotes = new CustomRichTextBoxEx();

			private void InitializeEditableNotes()
			{
				// Initialize editable notes textbox
				txtNotes.Name = m_txtNotesName;
				txtNotes.Enabled = false;
				txtNotes.Dock = DockStyle.Fill;
				txtNotes.BorderStyle = BorderStyle.FixedSingle;
				txtNotes.Font = Program.Config.UI.StandardFont.ToFont();
				txtNotes.AcceptsTab = true;
			}

			private void RefreshEditableNotes()
			{
				if (m_lvEntries.SelectedIndices.Count == 1)
				{
					PwEntry pe = m_host.MainWindow.GetSelectedEntry(true);
					if (pe != null)
					{
						txtNotes.Enabled = true;
						txtNotes.Text = pe.Strings.ReadSafe(PwDefs.NotesField);
					}
				}
				else
				{
					txtNotes.Text = "";
					txtNotes.Enabled = false;
				}
			}

			private void ShowPluginError()
			{
				MessageBox.Show("Error in KPEnhancedListView plugin: Editable Notes feature is not working. Maybe is not compatible with another custom plugin.");
			}

			private void txtNotes_Enter(object sender, EventArgs e)
			{
				txtNotes.TextChanged += txtNotes_TextChanged;
			}

			private void txtNotes_Leave(object sender, EventArgs e)
			{
				if (txtNotes.Tag != null) // Textbox has been marked as changed
				{
					PwDatabase pwStorage = m_host.Database;
					PwEntry pe = m_host.MainWindow.GetSelectedEntry(true);
					PwEntry peInit = (PwEntry)txtNotes.Tag;
					
					PwCompareOptions cmpOpt = (PwCompareOptions.IgnoreLastMod | PwCompareOptions.IgnoreLastAccess | PwCompareOptions.IgnoreLastBackup);
					if (pe.EqualsEntry(peInit, cmpOpt, MemProtCmpMode.None))
					{
						// Text contents has not been changed, undo last backup from history
						pe.LastModificationTime = peInit.LastModificationTime;
						pe.History.Remove(pe.History.GetAt(pe.History.UCount - 1)); // Undo backup
					}
					else
					{
						// Text has been changed - enable save icon
						Util.UpdateSaveState();
						m_host.MainWindow.EnsureVisibleEntry(pe.Uuid);
					}

					txtNotes.Tag = null;
				}

				txtNotes.TextChanged -= txtNotes_TextChanged;
			}

			private void txtNotes_TextChanged(object sender, EventArgs e)
			{
				PwDatabase pwStorage = m_host.Database;
				PwEntry pe = m_host.MainWindow.GetSelectedEntry(true);

				if (txtNotes.Tag == null)
				{
					// Text has just started changing, backup the original entry and save it in Tag property, which we'll use later do determine whether the entry is modified or not
					pe.CreateBackup(null);
					pe.Touch(true, false); // Touch *after* backup
					
					txtNotes.Tag = pe.CloneDeep();
				}

				pe.Strings.Set(PwDefs.NotesField, new ProtectedString(pwStorage.MemoryProtection.ProtectNotes, txtNotes.Text));
				Util.UpdateSaveState(); // Make save icon enabled
			}

			private void txtNotes_LinkClicked(object sender, LinkClickedEventArgs e)
			{
				System.Diagnostics.Process.Start(e.LinkText);
			}

			private void m_lvEntries_SelectedIndexChanged(object sender, EventArgs e)
			{
				RefreshEditableNotes();
			}

			private void MainWindow_UIStateUpdated(object sender, EventArgs e)
			{
				if (txtNotes.Tag == null)
				{
					PwEntry pe = m_host.MainWindow.GetSelectedEntry(true);
					if (pe != null && m_lvEntries.SelectedIndices.Count == 1)
					{
						txtNotes.Text = pe.Strings.ReadSafe(PwDefs.NotesField);
					}
				}
			}


		}
	}
}
