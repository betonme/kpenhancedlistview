using System.Windows.Forms;

namespace KPEnhancedListview
{
    partial class CustomColumnsForm
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomColumnsForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.m_lblSeparator = new System.Windows.Forms.Label();
            this.m_bannerImage = new System.Windows.Forms.PictureBox();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnAccept = new System.Windows.Forms.Button();
            this.m_dgvCustomColumns = new System.Windows.Forms.DataGridView();
            this.m_dgvColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.m_dgvName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_dgvIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_dgvOrder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_dgvEnable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.m_dgvHide = new KPEnhancedListview.ThreeStateCheckBoxColumn();
            this.m_dgvProtect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.m_dgvReadOnly = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.m_dgvWidth = new CSUST.Data.TNumEditDataGridViewColumn();
            this.m_dgvSort = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.m_btnTools = new System.Windows.Forms.Button();
            this.m_cmsTools = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showGroupsViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showUserStringsViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showAllUserStringsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.m_tsmiHideKeePass = new System.Windows.Forms.ToolStripMenuItem();
            this.m_tsmiManageKPEntryTemplates = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tODOExpertViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.threeStateCheckBoxColumn1 = new KPEnhancedListview.ThreeStateCheckBoxColumn();
            this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewCheckBoxColumn3 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.tNumEditDataGridViewColumn1 = new CSUST.Data.TNumEditDataGridViewColumn();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvCustomColumns)).BeginInit();
            this.m_cmsTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_lblSeparator
            // 
            this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_lblSeparator.Location = new System.Drawing.Point(0, 422);
            this.m_lblSeparator.Name = "m_lblSeparator";
            this.m_lblSeparator.Size = new System.Drawing.Size(734, 2);
            this.m_lblSeparator.TabIndex = 8;
            // 
            // m_bannerImage
            // 
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(734, 60);
            this.m_bannerImage.TabIndex = 9;
            this.m_bannerImage.TabStop = false;
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(647, 431);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
            this.m_btnCancel.TabIndex = 5;
            this.m_btnCancel.Text = "&Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
            // 
            // m_btnAccept
            // 
            this.m_btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnAccept.Location = new System.Drawing.Point(566, 431);
            this.m_btnAccept.Name = "m_btnAccept";
            this.m_btnAccept.Size = new System.Drawing.Size(75, 23);
            this.m_btnAccept.TabIndex = 4;
            this.m_btnAccept.Text = "&OK";
            this.m_btnAccept.UseVisualStyleBackColor = true;
            this.m_btnAccept.Click += new System.EventHandler(this.OnBtnOK);
            // 
            // m_dgvCustomColumns
            // 
            this.m_dgvCustomColumns.AllowUserToResizeRows = false;
            this.m_dgvCustomColumns.BackgroundColor = System.Drawing.SystemColors.Window;
            this.m_dgvCustomColumns.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_dgvCustomColumns.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.m_dgvCustomColumns.ColumnHeadersHeight = 22;
            this.m_dgvCustomColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.m_dgvCustomColumns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.m_dgvColumn,
            this.m_dgvName,
            this.m_dgvIndex,
            this.m_dgvOrder,
            this.m_dgvEnable,
            this.m_dgvHide,
            this.m_dgvProtect,
            this.m_dgvReadOnly,
            this.m_dgvWidth,
            this.m_dgvSort});
            this.m_dgvCustomColumns.GridColor = System.Drawing.SystemColors.ControlLight;
            this.m_dgvCustomColumns.Location = new System.Drawing.Point(12, 66);
            this.m_dgvCustomColumns.Name = "m_dgvCustomColumns";
            this.m_dgvCustomColumns.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.m_dgvCustomColumns.RowHeadersWidth = 22;
            this.m_dgvCustomColumns.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.m_dgvCustomColumns.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.m_dgvCustomColumns.Size = new System.Drawing.Size(710, 341);
            this.m_dgvCustomColumns.TabIndex = 0;
            this.m_dgvCustomColumns.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.OnCellMouseClick);
            this.m_dgvCustomColumns.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.OnCellBeginEdit);
            this.m_dgvCustomColumns.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.OnRowsAdded);
            this.m_dgvCustomColumns.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnCellEndEdit);
            this.m_dgvCustomColumns.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.OnDefaultValuesNeeded);
            this.m_dgvCustomColumns.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.OnEditingControlShowing);
            this.m_dgvCustomColumns.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.OnRowHeaderMouseClick);
            // 
            // m_dgvColumn
            // 
            this.m_dgvColumn.DataPropertyName = "Column";
            this.m_dgvColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.m_dgvColumn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.m_dgvColumn.HeaderText = "Column";
            this.m_dgvColumn.MinimumWidth = 50;
            this.m_dgvColumn.Name = "m_dgvColumn";
            this.m_dgvColumn.Width = 160;
            // 
            // m_dgvName
            // 
            this.m_dgvName.DataPropertyName = "Name";
            this.m_dgvName.HeaderText = "Name";
            this.m_dgvName.Name = "m_dgvName";
            this.m_dgvName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // m_dgvIndex
            // 
            this.m_dgvIndex.DataPropertyName = "Index";
            this.m_dgvIndex.HeaderText = "Index";
            this.m_dgvIndex.Name = "m_dgvIndex";
            this.m_dgvIndex.ReadOnly = true;
            this.m_dgvIndex.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgvIndex.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.m_dgvIndex.ToolTipText = "Internal";
            this.m_dgvIndex.Width = 40;
            // 
            // m_dgvOrder
            // 
            this.m_dgvOrder.DataPropertyName = "Order";
            this.m_dgvOrder.HeaderText = "Order";
            this.m_dgvOrder.Name = "m_dgvOrder";
            this.m_dgvOrder.ReadOnly = true;
            this.m_dgvOrder.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgvOrder.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.m_dgvOrder.ToolTipText = "Internal";
            this.m_dgvOrder.Width = 40;
            // 
            // m_dgvEnable
            // 
            this.m_dgvEnable.DataPropertyName = "Enable";
            this.m_dgvEnable.HeaderText = "Enable";
            this.m_dgvEnable.Name = "m_dgvEnable";
            this.m_dgvEnable.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgvEnable.ToolTipText = "Show Columns";
            this.m_dgvEnable.Width = 50;
            // 
            // m_dgvHide
            // 
            this.m_dgvHide.DataPropertyName = "Hide";
            this.m_dgvHide.DefaultValue = System.Windows.Forms.CheckState.Unchecked;
            this.m_dgvHide.FalseValue = KPEnhancedListview.HideStatus.Unhidden;
            this.m_dgvHide.HeaderText = "Hide";
            this.m_dgvHide.IndeterminateValue = KPEnhancedListview.HideStatus.Lazy;
            this.m_dgvHide.Name = "m_dgvHide";
            this.m_dgvHide.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgvHide.ToolTipText = "Hide Fields (Asterisks)";
            this.m_dgvHide.TrueValue = KPEnhancedListview.HideStatus.Full;
            this.m_dgvHide.Width = 50;
            // 
            // m_dgvProtect
            // 
            this.m_dgvProtect.DataPropertyName = "Protect";
            this.m_dgvProtect.HeaderText = "Protect";
            this.m_dgvProtect.Name = "m_dgvProtect";
            this.m_dgvProtect.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgvProtect.ToolTipText = "Write Protection";
            this.m_dgvProtect.Width = 50;
            // 
            // m_dgvReadOnly
            // 
            this.m_dgvReadOnly.DataPropertyName = "ReadOnly";
            this.m_dgvReadOnly.HeaderText = "ReadOnly";
            this.m_dgvReadOnly.Name = "m_dgvReadOnly";
            this.m_dgvReadOnly.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgvReadOnly.Width = 60;
            // 
            // m_dgvWidth
            // 
            this.m_dgvWidth.DataPropertyName = "Width";
            dataGridViewCellStyle1.Format = "F0";
            this.m_dgvWidth.DefaultCellStyle = dataGridViewCellStyle1;
            this.m_dgvWidth.HeaderText = "Width";
            this.m_dgvWidth.MaxInputLength = 3;
            this.m_dgvWidth.Name = "m_dgvWidth";
            this.m_dgvWidth.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgvWidth.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.m_dgvWidth.Width = 50;
            // 
            // m_dgvSort
            // 
            this.m_dgvSort.DataPropertyName = "Sort";
            this.m_dgvSort.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.m_dgvSort.HeaderText = "Sort";
            this.m_dgvSort.Name = "m_dgvSort";
            this.m_dgvSort.Width = 80;
            // 
            // m_btnTools
            // 
            this.m_btnTools.Image = ((System.Drawing.Image)(resources.GetObject("m_btnTools.Image")));
            this.m_btnTools.Location = new System.Drawing.Point(12, 431);
            this.m_btnTools.Name = "m_btnTools";
            this.m_btnTools.Size = new System.Drawing.Size(80, 23);
            this.m_btnTools.TabIndex = 1;
            this.m_btnTools.Text = "&Tools";
            this.m_btnTools.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.m_btnTools.UseVisualStyleBackColor = true;
            this.m_btnTools.Click += new System.EventHandler(this.OnBtnTools);
            // 
            // m_cmsTools
            // 
            this.m_cmsTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showGroupsViewToolStripMenuItem,
            this.showUserStringsViewToolStripMenuItem,
            this.toolStripSeparator1,
            this.showAllUserStringsToolStripMenuItem,
            this.toolStripSeparator2,
            this.m_tsmiHideKeePass,
            this.m_tsmiManageKPEntryTemplates,
            this.toolStripSeparator3,
            this.tODOExpertViewToolStripMenuItem});
            this.m_cmsTools.Name = "m_cmsTools";
            this.m_cmsTools.Size = new System.Drawing.Size(256, 154);
            // 
            // showGroupsViewToolStripMenuItem
            // 
            this.showGroupsViewToolStripMenuItem.Name = "showGroupsViewToolStripMenuItem";
            this.showGroupsViewToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.showGroupsViewToolStripMenuItem.Text = "Show Groups Tree View";
            this.showGroupsViewToolStripMenuItem.Click += new System.EventHandler(this.showGroupsViewToolStripMenuItem_Click);
            // 
            // showUserStringsViewToolStripMenuItem
            // 
            this.showUserStringsViewToolStripMenuItem.Name = "showUserStringsViewToolStripMenuItem";
            this.showUserStringsViewToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.showUserStringsViewToolStripMenuItem.Text = "Show User Strings View";
            this.showUserStringsViewToolStripMenuItem.Click += new System.EventHandler(this.showUserStringsViewToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(252, 6);
            // 
            // showAllUserStringsToolStripMenuItem
            // 
            this.showAllUserStringsToolStripMenuItem.Name = "showAllUserStringsToolStripMenuItem";
            this.showAllUserStringsToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.showAllUserStringsToolStripMenuItem.Text = "Show All User Strings";
            this.showAllUserStringsToolStripMenuItem.Click += new System.EventHandler(this.showAllUserStringsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(252, 6);
            // 
            // m_tsmiHideKeePass
            // 
            this.m_tsmiHideKeePass.Name = "m_tsmiHideKeePass";
            this.m_tsmiHideKeePass.Size = new System.Drawing.Size(255, 22);
            this.m_tsmiHideKeePass.Text = "Hide KeePass Columns";
            this.m_tsmiHideKeePass.Click += new System.EventHandler(this.OnClickToolsHideKeePass);
            // 
            // m_tsmiManageKPEntryTemplates
            // 
            this.m_tsmiManageKPEntryTemplates.Name = "m_tsmiManageKPEntryTemplates";
            this.m_tsmiManageKPEntryTemplates.Size = new System.Drawing.Size(255, 22);
            this.m_tsmiManageKPEntryTemplates.Text = "Manage KPEntryTemplates Strings";
            this.m_tsmiManageKPEntryTemplates.Click += new System.EventHandler(this.OnClickToolsManageKPEntryTemplates);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(252, 6);
            // 
            // tODOExpertViewToolStripMenuItem
            // 
            this.tODOExpertViewToolStripMenuItem.Name = "tODOExpertViewToolStripMenuItem";
            this.tODOExpertViewToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.tODOExpertViewToolStripMenuItem.Text = "Expert View";
            this.tODOExpertViewToolStripMenuItem.Click += new System.EventHandler(this.tODOExpertViewToolStripMenuItem_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Name";
            this.dataGridViewTextBoxColumn1.HeaderText = "Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Index";
            this.dataGridViewTextBoxColumn2.HeaderText = "Index";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn2.ToolTipText = "Internal";
            this.dataGridViewTextBoxColumn2.Width = 40;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Order";
            this.dataGridViewTextBoxColumn3.HeaderText = "Order";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn3.ToolTipText = "Internal";
            this.dataGridViewTextBoxColumn3.Width = 40;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.DataPropertyName = "Show";
            this.dataGridViewCheckBoxColumn1.HeaderText = "Show";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            this.dataGridViewCheckBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn1.ToolTipText = "Show Columns";
            this.dataGridViewCheckBoxColumn1.Width = 40;
            // 
            // threeStateCheckBoxColumn1
            // 
            this.threeStateCheckBoxColumn1.DataPropertyName = "Hide";
            this.threeStateCheckBoxColumn1.DefaultValue = System.Windows.Forms.CheckState.Unchecked;
            this.threeStateCheckBoxColumn1.FalseValue = KPEnhancedListview.HideStatus.Unhidden;
            this.threeStateCheckBoxColumn1.HeaderText = "Hide";
            this.threeStateCheckBoxColumn1.IndeterminateValue = KPEnhancedListview.HideStatus.Lazy;
            this.threeStateCheckBoxColumn1.Name = "threeStateCheckBoxColumn1";
            this.threeStateCheckBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.threeStateCheckBoxColumn1.ToolTipText = "Hide Fields (Asterisks)";
            this.threeStateCheckBoxColumn1.TrueValue = KPEnhancedListview.HideStatus.Full;
            this.threeStateCheckBoxColumn1.Width = 40;
            // 
            // dataGridViewCheckBoxColumn2
            // 
            this.dataGridViewCheckBoxColumn2.DataPropertyName = "Protect";
            this.dataGridViewCheckBoxColumn2.HeaderText = "Protect";
            this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            this.dataGridViewCheckBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn2.ToolTipText = "Write Protection";
            this.dataGridViewCheckBoxColumn2.Width = 50;
            // 
            // dataGridViewCheckBoxColumn3
            // 
            this.dataGridViewCheckBoxColumn3.DataPropertyName = "ReadOnly";
            this.dataGridViewCheckBoxColumn3.HeaderText = "ReadOnly";
            this.dataGridViewCheckBoxColumn3.Name = "dataGridViewCheckBoxColumn3";
            this.dataGridViewCheckBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn3.Width = 50;
            // 
            // tNumEditDataGridViewColumn1
            // 
            this.tNumEditDataGridViewColumn1.DataPropertyName = "Width";
            dataGridViewCellStyle2.Format = "F0";
            this.tNumEditDataGridViewColumn1.DefaultCellStyle = dataGridViewCellStyle2;
            this.tNumEditDataGridViewColumn1.HeaderText = "Width";
            this.tNumEditDataGridViewColumn1.MaxInputLength = 3;
            this.tNumEditDataGridViewColumn1.Name = "tNumEditDataGridViewColumn1";
            this.tNumEditDataGridViewColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.tNumEditDataGridViewColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.tNumEditDataGridViewColumn1.Width = 50;
            // 
            // CustomColumnsForm
            // 
            this.AcceptButton = this.m_btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(734, 466);
            this.Controls.Add(this.m_btnTools);
            this.Controls.Add(this.m_dgvCustomColumns);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnAccept);
            this.Controls.Add(this.m_lblSeparator);
            this.Controls.Add(this.m_bannerImage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomColumnsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Custom Columns";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvCustomColumns)).EndInit();
            this.m_cmsTools.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.PictureBox m_bannerImage;
        //private GridView m_lvCustomColumns;
        private System.Windows.Forms.Label m_lblSeparator;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.Button m_btnAccept;
        private System.Windows.Forms.DataGridView m_dgvCustomColumns;
        private System.Windows.Forms.Button m_btnTools;
        private System.Windows.Forms.ContextMenuStrip m_cmsTools;
        private System.Windows.Forms.ToolStripMenuItem m_tsmiHideKeePass;
        private System.Windows.Forms.ToolStripMenuItem m_tsmiManageKPEntryTemplates;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem showGroupsViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showUserStringsViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showAllUserStringsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private ThreeStateCheckBoxColumn threeStateCheckBoxColumn1;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn3;
        private CSUST.Data.TNumEditDataGridViewColumn tNumEditDataGridViewColumn1;
        private DataGridViewComboBoxColumn m_dgvColumn;
        private DataGridViewTextBoxColumn m_dgvName;
        private DataGridViewTextBoxColumn m_dgvIndex;
        private DataGridViewTextBoxColumn m_dgvOrder;
        private DataGridViewCheckBoxColumn m_dgvEnable;
        private ThreeStateCheckBoxColumn m_dgvHide;
        private DataGridViewCheckBoxColumn m_dgvProtect;
        private DataGridViewCheckBoxColumn m_dgvReadOnly;
        private CSUST.Data.TNumEditDataGridViewColumn m_dgvWidth;
        private DataGridViewComboBoxColumn m_dgvSort;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem tODOExpertViewToolStripMenuItem;
	}
}