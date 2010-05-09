/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
#if !USE_NET20
    using System.Linq;
#endif

using KeePass;
using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

namespace KPEnhancedListview
{
    public partial class CustomColumnsForm : Form
    {
        private const string m_cfgCCHideKeePass           = "KPEnhancedListview_CustomColumns_HideKeePass";
        private const string m_cfgCCHideKPEntryTemplates  = "KPEnhancedListview_CustomColumns_HideKPEntryTemplates";

        private IPluginHost m_host = null;

        private CustomListViewEx m_clveEntries = null;

        private string m_sColumnPrev = null;

        private Control m_ccControl = null;

        private BindingSource m_bsCustomColumns = null;
        private BindingList<CustomColumn> m_blCustomColumns = null;
        //private BindingListView<CustomColumn> m_blCustomColumns = null;
        
        public CustomColumnsForm()
        {
            InitializeComponent();
            
            //Program.Translation.ApplyTo(this);
        }

        internal void InitEx(IPluginHost host, CustomListViewEx clve, List<CustomColumn> lcc)
        {
            m_host = host;
            m_clveEntries = clve;

            // ComboBox column item
            /*List<String> ls = new List<string>();
            ls.AddRange(KPELVUtil.GetKeePassColumns(m_clveEntries));
            ls.AddRange(KPELVUtil.GetEntriesUserStrings(m_host.Database.RootGroup));
            //foreach (string s in lcc.ConvertAll<string>(cc => cc.Column))
            // Add all already defined columns
            foreach (CustomColumn cc in lcc)
            {
                //if (!ls.Contains(s))
                ls.Add(cc.Column);
            }
            MessageBox.Show(ls.Count.ToString());
            m_dgvColumn.Items.AddRange(ls);
            */
            m_dgvColumn.Items.Clear();
            m_dgvColumn.Items.AddRange(Util.GetListKeePassColumns(m_clveEntries).ToArray());
            m_dgvColumn.Items.AddRange(Util.GetListEntriesUserStrings(m_host.Database.RootGroup).ToArray());

            // ComboBox sort item
            m_dgvSort.DataSource = EnumPair<SortOrder>.GetValuePairList();
            m_dgvSort.ValueMember = EnumPair<SortOrder>.ValueMember;
            m_dgvSort.DisplayMember = EnumPair<SortOrder>.DisplayMember;

//TODO Default row template instead of class constructor

//TODO Autosize columns or Calculate column width for column name and sort    

            // Local copy of the custom columns settings
            // ToList ist necessary to create a local copy
            m_blCustomColumns = new BindingList<CustomColumn>(lcc.ToList<CustomColumn>());
            m_bsCustomColumns = new BindingSource();

            m_bsCustomColumns.DataSource = m_blCustomColumns;
            m_dgvCustomColumns.DataSource = m_bsCustomColumns;

            m_dgvCustomColumns.AllowUserToAddRows = true;
            m_dgvCustomColumns.AllowUserToDeleteRows = true;
        }

        /*
        protected override DialogResult ShowDialog(object MyCustomParameterObject)
        {
            //this.CustomParam = MyCustomParameterObject;
            return this.ShowDialog();
        }*/

        internal List<CustomColumn> GetCustomColumns()
        {
            return m_blCustomColumns.ToList<CustomColumn>();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            GlobalWindowManager.AddWindow(this);

            m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
                m_bannerImage.Height, BannerStyle.Default,
                Properties.Resources.B48x48_CustomColumns,
                "Custom Columns",
                "Here you can manage the KeePass Custom Columns.");
            
            //m_dgvCustomColumns.RowTemplate = 
            //this.Icon = Properties.Resources.KeePass;

            // Check custom config
            m_tsmiHideKeePass.Checked = m_host.CustomConfig.GetBool(m_cfgCCHideKeePass, false);
            m_tsmiManageKPEntryTemplates.Checked = m_host.CustomConfig.GetBool(m_cfgCCHideKPEntryTemplates, false);

            UpdateKeePassColumns();
            UpdateKPEntryTemplates();

            // Todo search for KPEntryTemplates PlugIn
            //KeePass.Plugins.

            // Set Hide checkbox to unhidden
            //((System.Windows.Forms.DataGridViewCheckBoxCell)m_dgvCustomColumns.Rows[0].Cells[m_dgvHide.Index]).Value = HideStatus.Unhidden;
//m_dgvCustomColumns.Rows.Clear();

            //TODO
            //SetAlternatingBgColors((ListView)m_dgvCustomColumns, UIUtil.GetAlternateColor(m_clveEntries.BackColor), Program.Config.MainWindow.EntryListAlternatingBgColors);
            // Set the background color for all rows and for alternating rows. 
            // The value for alternating rows overrides the value for all rows. 
            //dataGridView1.RowsDefaultCellStyle.BackColor = Color.LightGray;
            //dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.DarkGray;
        }
        
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            //MessageBox.Show(TypeDescriptor.GetProperties(m_dgvCustomColumns.CurrentCell.GetType()).ToString());
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }

        private void OnBtnOK(object sender, EventArgs e)
        {
            // TODO return new struct
        }

        private void OnBtnCancel(object sender, EventArgs e)
        {
            // TODO return backup 
        }

        private void OnCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //TODO edit in designer: Cell edit begins with EditOnKeystrokeOrF2 or another way ???

            // If user string combobox editing control is shown
            if (e.ColumnIndex == m_dgvColumn.Index)
            {
                // Update combobox items
                UpdateDGVCellColumn((DataGridViewComboBoxCell)m_dgvCustomColumns.CurrentCell);
            }
        }

        private void OnEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // If name field combobox editing control is shown
            if (m_dgvCustomColumns.CurrentCell.ColumnIndex == m_dgvColumn.Index)
            {
                // If control is DataGridViewComboBoxEditingControl
                if (e.Control.GetType() == typeof(System.Windows.Forms.DataGridViewComboBoxEditingControl))
                {
                    m_ccControl = e.Control;
                    ((ComboBox)m_ccControl).SelectedValueChanged += new EventHandler(OnControlSelectedValueChanged);

                    // Save for next update
                    m_sColumnPrev = m_ccControl.Text;

                    // Workaround DataGridViewComboBoxCell.MaxDropDownItems not working
                    // Set ComboBox.DropDownHeight
                    ((ComboBox)m_ccControl).DropDownHeight = 240;
                }
            }
        }

        private void OnControlSelectedValueChanged(object sender, EventArgs e)
        {
            // If name field is actually edited
            if (m_dgvCustomColumns.CurrentCell.ColumnIndex == m_dgvColumn.Index)
            {
                // Update name field
                UpdateDGVCellName(m_dgvCustomColumns.CurrentCell.RowIndex);
            }
        }
        
        private void OnCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // If column is user string combobox
            if (e.ColumnIndex == m_dgvColumn.Index)
            {
                if (m_ccControl != null)
                {
                    ((ComboBox)m_ccControl).SelectedValueChanged -= new EventHandler(OnControlSelectedValueChanged);
                    m_ccControl = null;
                    m_sColumnPrev = string.Empty;
                }
            }
        }

        private void OnBtnTools(object sender, EventArgs e)
        {
            m_cmsTools.Show(m_btnTools, 0, m_btnTools.Height);
        }
  
        private void OnClickToolsHideKeePass(object sender, EventArgs e)
        {       
            m_tsmiHideKeePass.Checked = !m_tsmiHideKeePass.Checked;

            m_host.CustomConfig.SetBool(m_cfgCCHideKeePass, m_tsmiHideKeePass.Checked);

            /*
            BindingSource.Filter RemoveFilter column or index
            source1.Filter = "artist = 'Dave Matthews' OR cd = 'Tigerlily'";
            source1.Filter = "index < AppDefs.ColumnId.Count'";
            Is fillcomboboxcolumn working correct??
            */
            UpdateKeePassColumns();
        }

        private void OnClickToolsManageKPEntryTemplates(object sender, EventArgs e)
        {
            m_tsmiManageKPEntryTemplates.Checked = !m_tsmiManageKPEntryTemplates.Checked;

            m_host.CustomConfig.SetBool(m_cfgCCHideKPEntryTemplates, m_tsmiManageKPEntryTemplates.Checked);
        }

        private void OnRowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            m_dgvCustomColumns.ClearSelection();
            m_dgvCustomColumns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            m_dgvCustomColumns.Rows[e.RowIndex].Selected = true;
            m_blCustomColumns.AllowRemove = true;
        }

        private void OnCellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                m_dgvCustomColumns.ClearSelection();
                m_dgvCustomColumns.SelectionMode = DataGridViewSelectionMode.CellSelect;
                m_dgvCustomColumns.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
                m_blCustomColumns.AllowRemove = false;
            }
        }

        private void OnDefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            //e.Row.Cells[m_dgvIndex.Index].Value = m_dgvCustomColumns.Rows.Count.ToString();
            m_blCustomColumns[e.Row.Index].Index = e.Row.Index; //m_blCustomColumns.Count;
        }

        private void OnRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //m_blCustomColumns[e.RowIndex].Hide = HideStatus.Unhidden;
            //m_blCustomColumns[e.RowIndex].Index = m_blCustomColumns.Count;
            
            //m_dgvCustomColumns.Rows[e.RowIndex].Cells[m_dgvIndex.Index].Value = m_dgvCustomColumns.Rows.Count.ToString();
        }

        private void UpdateKeePassColumns()
        {
            foreach (DataGridViewRow row in m_dgvCustomColumns.Rows)
            {
                /*switch ((AppDefs.ColumnId)row.Index)
                {
                    case AppDefs.ColumnId.CreationTime:
                    case AppDefs.ColumnId.LastAccessTime:
                    case AppDefs.ColumnId.LastModificationTime:
                    case AppDefs.ColumnId.ExpiryTime:
                    case AppDefs.ColumnId.Uuid:
                    case AppDefs.ColumnId.Attachment:
                        //cc.Protect = true;
                        break;
                    default:
                        //cc.Protect = false;
                        break;
                }*/
                //TODO
                switch ((AppDefs.ColumnId)row.Index)
                {
                    case AppDefs.ColumnId.Title:
                        //((CSUST.Data.ThreeStateCheckBoxCell)(row.Cells[m_dgvHide.Index])).Enabled = true;
                        break;
                    case AppDefs.ColumnId.UserName:
                    case AppDefs.ColumnId.Password:
                    case AppDefs.ColumnId.Url:
                    case AppDefs.ColumnId.CreationTime:
                    case AppDefs.ColumnId.LastAccessTime:
                    case AppDefs.ColumnId.LastModificationTime:
                    case AppDefs.ColumnId.ExpiryTime:
                    case AppDefs.ColumnId.Uuid:
                    case AppDefs.ColumnId.Attachment:
                        break;
                    case AppDefs.ColumnId.Notes:
                    default:
                        break;
                }

            }

            if (m_tsmiHideKeePass.Checked)
            {
                //m_bsCustomColumns.Filter = "Index = < 5"; //< AppDefs.ColumnId.Count";  //"Column = 'test'"; //"Index = 0"; // 

                //BLW SourceFourge
                //m_blCustomColumns.ApplyFilter(delegate(CustomColumn cc) { return cc.Index > 5; }); 

                // working for BindingList List but AllowUserToAddRows not working 
                //m_bsCustomColumns.DataSource = m_blCustomColumns.Where(cc => cc.Index < 5); //.ToList(); // New Row is allowed but it will never returns to the original bindinglist
                //m_bsCustomColumns.AllowNew = true;

                //m_bsCustomColumns.AllowRemove = true;
                //m_bsCustomColumns.SyncRoot 

                // working
                m_dgvCustomColumns.CurrentCell = null;
                //m_dgvCustomColumns.Rows[0].Visible = false;

                foreach (DataGridViewRow row in m_dgvCustomColumns.Rows)
                {
                    if (((int)row.Cells[m_dgvIndex.Index].FormattedValue) < (int)AppDefs.ColumnId.Count)
                    {
                        row.Visible = false;
                    }
                }

                //m_blCustomColumns.AllowNew = true;
                //m_dgvCustomColumns.AllowUserToAddRows = true;
            }
            else
            {
                //m_bsCustomColumns.RemoveFilter();

                //BLW SourceFourge
                //m_blCustomColumns.RemoveFilter();

                //m_blCustomColumns.AddRange(m_dgvCustomColumns.DataSource as List<CustomColumn>); // NOT

                //m_bsCustomColumns.DataSource = m_blCustomColumns; //.Concat(m_bsCustomColumns.DataSource);

                m_dgvCustomColumns.CurrentCell = null;
                //m_dgvCustomColumns.Rows[0].Visible = false;

                foreach (DataGridViewRow row in m_dgvCustomColumns.Rows)
                {
                    if (row.Index < (int)AppDefs.ColumnId.Count)
                    {
                        row.Visible = true;
                    }
                }
            }
            m_dgvCustomColumns.Refresh();
        }

        private void UpdateKPEntryTemplates()
        {
            // TODO manage defined columns
        }

        private void UpdateDGVCellColumn(DataGridViewComboBoxCell dgvcbc)
        {
            // Clear previous items
            dgvcbc.Items.Clear();

            // Add all user strings
            dgvcbc.Items.AddRange(UpdateUserStrings(dgvcbc.RowIndex).ToArray());

            // Add new column item
            dgvcbc.Items.Add("New Column");
        }

        private void UpdateDGVCellName(int row)
        {
            // Update if name is empty or if name value is equal previous column value
            //if ((m_dgvCustomColumns.Rows[row].Cells[m_dgvName.Index].Value == null) || (m_dgvCustomColumns.Rows[row].Cells[m_dgvName.Index].Value.ToString() == m_sColumnPrev))

            if ((m_blCustomColumns[row].Name == null) || (m_blCustomColumns[row].Name == m_sColumnPrev))
            {
                // Copy user string to name field
                m_blCustomColumns[row].Name = m_ccControl.Text;
                
                // Save for next update
                m_sColumnPrev = m_ccControl.Text;
                
                // Refresh datagridview
                m_dgvCustomColumns.Refresh();
            }
        }

        private List<string> UpdateUserStrings(int index)
        {
            List<string> strl = new List<string>();

            // Add all keepass strings
            strl.AddRange(Util.GetListKeePassColumns(m_clveEntries));

            // All user strings
            strl.AddRange(Util.GetListEntriesUserStrings(m_host.Database.RootGroup));

            // Check state of hide KPEntryTemplates
            if (m_tsmiManageKPEntryTemplates.Checked == true)
            {
                /*
                Dictionary<string, string> strd = KPELVUtil.GetDictEntriesUserStrings(m_host.Database.RootGroup);

                foreach (KeyValuePair<string, string> data in strd)
                {
                    if (data.Key.StartsWith("_etm_title"))
                    {
                        //string s = data.Key.Substring(
                        strl.Add(data.Value);
                    }
                }
                */
                // Remove all _etm entries
                strl.RemoveAll(item => item.StartsWith("_etm"));
            }

            // Remove all already defined columns
            foreach (DataGridViewRow dgvr in m_dgvCustomColumns.Rows)
            {
                // If row is not the actually selected
                if (dgvr.Index != index)
                {
                    strl.Remove(dgvr.Cells[m_dgvColumn.Index].FormattedValue.ToString());
                }
            }

            strl.Sort();

            return strl;
        }

        private void showGroupsViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO
        }

        private void showUserStringsViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO
        }

        private void tODOExpertViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO
        }

        private void showAllUserStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO
        }


        /*  // move to kpeutil
        // Adapted from KeePass
        public static void SetAlternatingBgColors(DataGridView lv, Color clrAlternate, bool bAlternate)
        {
            if (lv == null) throw new ArgumentNullException("lv");

            Color clrBg = lv.BackColor;

            for (int i = 0; i < lv.Items.Count; ++i)
            {
                ListViewItem lvi = lv.Items[i];
                Debug.Assert(lvi.Index == i);
                Debug.Assert(lvi.UseItemStyleForSubItems);

                if (!bAlternate)
                {
                    if (ColorsEqual(lvi.BackColor, clrAlternate))
                        lvi.BackColor = clrBg;
                }
                else if (((i & 1) == 0) && ColorsEqual(lvi.BackColor, clrAlternate))
                    lvi.BackColor = clrBg;
                else if (((i & 1) == 1) && ColorsEqual(lvi.BackColor, clrBg))
                    lvi.BackColor = clrAlternate;
            }
        }*/
    }
    /*
    /// <summary>
    /// Provides a static utility object of methods and properties to interact
    /// with enumerated types.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Gets the <see cref="DescriptionAttribute" /> of an <see cref="Enum" /> 
        /// type value.
        /// </summary>
        /// <param name="value">The <see cref="Enum" /> type value.</param>
        /// <returns>A string containing the text of the
        /// <see cref="DescriptionAttribute"/>.</returns>
        public static string GetDescription(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            string description = value.ToString();
            FieldInfo fieldInfo = value.GetType().GetField(description);
            EnumDescriptionAttribute[] attributes =
               (EnumDescriptionAttribute[])
             fieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                description = attributes[0].Description;
            }
            return description;
        }

        /// <summary>
        /// Converts the <see cref="Enum" /> type to an <see cref="IList" /> 
        /// compatible object.
        /// </summary>
        /// <param name="type">The <see cref="Enum"/> type.</param>
        /// <returns>An <see cref="IList"/> containing the enumerated
        /// type value and description.</returns>
        public static IList ToList(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            ArrayList list = new ArrayList();
            Array enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
                list.Add(new KeyValuePair<Enum, string>(value, GetDescription(value)));
            }

            return list;
        }
    }*/
}
