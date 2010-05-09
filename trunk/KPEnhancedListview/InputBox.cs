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

namespace KPEnhancedListview
{
    /// 
    /// AddCustomColumns NewString
    /// This static class contains methods named Show() to display a dialog box 
    /// with an input field, similar in appearance to the one in Visual Basic.
    /// The Show() method returns null if the user clicks Cancel, and non-null
    /// if the user clicks OK.
    /// 
    public class InputBox
    {
        static public string Show(string Prompt)
        { return Show(Prompt, null, null); }

        static public string Show(string Prompt, string Title)
        { return Show(Prompt, Title, null); }

        static public string Show(string Prompt, string Title, string Default)    
        {
            if (Title == null)
                Title = Application.ProductName;
            InputBoxDialog dlg = new InputBoxDialog(Prompt, Title);
            if (Default != null)
                dlg.txtInput.Text = Default;
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.Cancel)
                return null;
            else
                return dlg.txtInput.Text;
        }

        internal class InputBoxDialog : Form
        {
            private System.Windows.Forms.Label lblPrompt;
            public System.Windows.Forms.TextBox txtInput;
            private System.Windows.Forms.Button btnOK;
            private System.Windows.Forms.Button btnCancel;

            public InputBoxDialog(string prompt, string title)
            {
                InitializeComponent();

                lblPrompt.Text = prompt;
                this.Text = title;

                Graphics g = this.CreateGraphics();
                SizeF size = g.MeasureString(prompt, lblPrompt.Font, lblPrompt.Width);
                if (size.Height > lblPrompt.Height)
                    this.Height += (int)size.Height - lblPrompt.Height;

                txtInput.SelectionStart = 0;
                txtInput.SelectionLength = txtInput.Text.Length;
                txtInput.Focus();
            }

            /// 
            /// Required method for Designer support - do not modify
            /// the contents of this method with the code editor.
            /// 
            private void InitializeComponent()
            {
                this.lblPrompt = new System.Windows.Forms.Label();
                this.txtInput = new System.Windows.Forms.TextBox();
                this.btnOK = new System.Windows.Forms.Button();
                this.btnCancel = new System.Windows.Forms.Button();
                this.SuspendLayout();
                // 
                // lblPrompt
                // 
                this.lblPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)));
                this.lblPrompt.BackColor = System.Drawing.SystemColors.Control;
                this.lblPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
                this.lblPrompt.Location = new System.Drawing.Point(12, 9);
                this.lblPrompt.Name = "lblPrompt";
                this.lblPrompt.Size = new System.Drawing.Size(302, 71);
                this.lblPrompt.TabIndex = 3;
                // 
                // txtInput
                // 
                this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                this.txtInput.Location = new System.Drawing.Point(8, 88);
                this.txtInput.Name = "txtInput";
                this.txtInput.Size = new System.Drawing.Size(381, 20);
                this.txtInput.TabIndex = 0;
                this.txtInput.Text = string.Empty;
                // 
                // btnOK
                // 
                this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.btnOK.Location = new System.Drawing.Point(326, 8);
                this.btnOK.Name = "btnOK";
                this.btnOK.Size = new System.Drawing.Size(64, 24);
                this.btnOK.TabIndex = 1;
                this.btnOK.Text = "&OK";
                // 
                // btnCancel
                // 
                this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.btnCancel.Location = new System.Drawing.Point(326, 40);
                this.btnCancel.Name = "btnCancel";
                this.btnCancel.Size = new System.Drawing.Size(64, 24);
                this.btnCancel.TabIndex = 2;
                this.btnCancel.Text = "&Cancel";
                // 
                // Dialog
                // 
                this.AcceptButton = this.btnOK;
                this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
                this.CancelButton = this.btnCancel;
                this.ClientSize = new System.Drawing.Size(398, 117);
                this.Controls.Add(this.txtInput);
                this.Controls.Add(this.btnCancel);
                this.Controls.Add(this.btnOK);
                this.Controls.Add(this.lblPrompt);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.Name = "Dialog";
                this.ResumeLayout(false);
                this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            }
        }
    }

    /// 
    /// RemoveCustomColumns
    /// This static class contains methods named Show() to display a dialog box 
    /// with an listbox field.
    /// The Show() method returns null if the user clicks Cancel, and non-null
    /// if the user clicks OK.
    /// 
    public class InputListBox
    {
        static public string[] Show(string[] Options, string Prompt)
        { return Show(Options, Prompt, null, 0); }

        static public string[] Show(string[] Options, string Prompt, string Title)
        { return Show(Options, Prompt, Title, 0); }

        static public string[] Show(string[] Options, string Prompt, string Title, int Selected)
        {
            if (Title == null)
                Title = Application.ProductName;

            #if USE_NET20
              if (Options.Length == 0)
            #else
              if (Options.Count<string>() == 0)
            #endif
            {
                return null;
            }

            ListBoxDialog dlg = new ListBoxDialog(Options, Prompt, Title, Selected);

            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.Cancel)
                return null;
            else
            {
                if (dlg.lbInput.SelectedItems == null)
                {
                    return null;
                }
                else
                {
                    #if USE_NET20
                      string[] str = new string[dlg.lbInput.SelectedItems.Count];
                      dlg.lbInput.SelectedItems.CopyTo(str, 0);
                      return str;
                    #else
                      return dlg.lbInput.SelectedItems.Cast<string>().ToArray();
                    #endif
                }

            }
        }

        internal class ListBoxDialog : Form
        {
            private System.Windows.Forms.Label lblPrompt;        
            public System.Windows.Forms.ListBox lbInput;
            private System.Windows.Forms.Button btnOK;
            private System.Windows.Forms.Button btnCancel;

            public ListBoxDialog(string[] options, string prompt, string title, int selected)
            {
                InitializeComponent();

                lblPrompt.Text = prompt;
                this.Text = title;

                #if USE_NET20
                  if (options.Length > 0)
                #else
                  if (options.Count<string>() > 0)
                #endif
                {
                    this.lbInput.Items.AddRange(options);
                    this.lbInput.SetSelected(selected, true);
                }

                Graphics g = this.CreateGraphics();
                SizeF size = g.MeasureString(prompt, lblPrompt.Font, lblPrompt.Width);
                if (size.Height > lblPrompt.Height)
                    this.Height += (int)size.Height - lblPrompt.Height;

                lbInput.Focus();
            }

            /// 
            /// Required method for Designer support - do not modify
            /// the contents of this method with the code editor.
            /// 
            private void InitializeComponent()
            {
                this.SuspendLayout();
                // 
                // lblPrompt
                // 
                this.lblPrompt = new System.Windows.Forms.Label();
                this.lblPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)));
                this.lblPrompt.BackColor = System.Drawing.SystemColors.Control;
                this.lblPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
                this.lblPrompt.Location = new System.Drawing.Point(12, 9);
                this.lblPrompt.Name = "lblPrompt";
                this.lblPrompt.Size = new System.Drawing.Size(302, 71);
                this.lblPrompt.TabIndex = 3;
                // 
                // lbInput
                // 
                this.lbInput = new System.Windows.Forms.ListBox();
                this.lbInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                this.lbInput.Location = new System.Drawing.Point(8, 32);
                this.lbInput.Name = "lbInput";
                this.lbInput.Size = new System.Drawing.Size(302, 80);
                this.lbInput.TabIndex = 0;
                this.lbInput.Text = string.Empty;
                this.lbInput.SelectionMode = SelectionMode.MultiExtended;
                // 
                // btnOK
                // 
                this.btnOK = new System.Windows.Forms.Button();
                this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.btnOK.Location = new System.Drawing.Point(326, 8);
                this.btnOK.Name = "btnOK";
                this.btnOK.Size = new System.Drawing.Size(64, 24);
                this.btnOK.TabIndex = 1;
                this.btnOK.Text = "&OK";
                // 
                // btnCancel
                // 
                this.btnCancel = new System.Windows.Forms.Button();
                this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.btnCancel.Location = new System.Drawing.Point(326, 40);
                this.btnCancel.Name = "btnCancel";
                this.btnCancel.Size = new System.Drawing.Size(64, 24);
                this.btnCancel.TabIndex = 2;
                this.btnCancel.Text = "&Cancel";
                // 
                // Dialog
                // 
                this.AcceptButton = this.btnOK;
                this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
                this.CancelButton = this.btnCancel;
                this.ClientSize = new System.Drawing.Size(398, 117);
                this.Controls.Add(this.lbInput);
                this.Controls.Add(this.btnCancel);
                this.Controls.Add(this.btnOK);
                this.Controls.Add(this.lblPrompt);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.Name = "Dialog";
                this.ResumeLayout(false);
                this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            }
        }
    }

    /// 
    /// AddCustomColumns
    /// This static class contains methods named Show() to display a dialog box 
    /// with an listbox field.
    /// The Show() method returns null if the user clicks Cancel, and non-null
    /// if the user clicks OK.
    /// 
    public class InputComboBox
    {
        static public string[] Show(string[] Options, string Prompt)
        { return Show(Options, Prompt, null, 0); }

        static public string[] Show(string[] Options, string Prompt, string Title)
        { return Show(Options, Prompt, Title, 0); }

        static public string[] Show(string[] Options, string Prompt, string Title, int Selected)
        {
            if (Title == null)
                Title = Application.ProductName;

            ComboBoxDialog dlg = new ComboBoxDialog(Options, Prompt, Title, Selected);
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.Cancel)
                return null;
            else
            {
                if (dlg.lbInput.SelectedItems == null)
                {
                    return null;
                }
                else
                {
                    #if USE_NET20
                      string[] str = new string[dlg.lbInput.SelectedItems.Count];
                      dlg.lbInput.SelectedItems.CopyTo(str, 0);
                      return str;
                    #else
                      return dlg.lbInput.SelectedItems.Cast<string>().ToArray();
                    #endif
                }

            }
        }

        internal class ComboBoxDialog : Form
        {
            private System.Windows.Forms.Label lblPrompt;
            public System.Windows.Forms.ListBox lbInput;
            private System.Windows.Forms.Button btnOK;
            private System.Windows.Forms.Button btnCancel;
            private System.Windows.Forms.Button btnNew;

            public ComboBoxDialog(string[] options, string prompt, string title, int selected)
            {
                InitializeComponent();

                lblPrompt.Text = prompt;
                this.Text = title;

                #if USE_NET20
                  if (options.Length > 0)
                #else
                  if (options.Count<string>() > 0)
                #endif
                {
                    this.lbInput.Items.AddRange(options);
                    this.lbInput.SetSelected(selected, true);
                }

                Graphics g = this.CreateGraphics();
                SizeF size = g.MeasureString(prompt, lblPrompt.Font, lblPrompt.Width);
                if (size.Height > lblPrompt.Height)
                    this.Height += (int)size.Height - lblPrompt.Height;
//TODO
                //this.Height *= 2;

                lbInput.Focus();
            }

            /// 
            /// Required method for Designer support - do not modify
            /// the contents of this method with the code editor.
            /// 
            private void InitializeComponent()
            {
                this.SuspendLayout();
                // 
                // lblPrompt
                // 
                this.lblPrompt = new System.Windows.Forms.Label();
                this.lblPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)));
                this.lblPrompt.BackColor = System.Drawing.SystemColors.Control;
                this.lblPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
                this.lblPrompt.Location = new System.Drawing.Point(12, 9);
                this.lblPrompt.Name = "lblPrompt";
                this.lblPrompt.Size = new System.Drawing.Size(302, 71);
                this.lblPrompt.TabIndex = 3;
                // 
                // lbInput
                // 
                this.lbInput = new System.Windows.Forms.ListBox();
                this.lbInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                this.lbInput.Location = new System.Drawing.Point(8, 32);
                this.lbInput.Name = "lbInput";
                this.lbInput.Size = new System.Drawing.Size(302, 80);
                this.lbInput.TabIndex = 0;
                this.lbInput.Text = string.Empty;
                this.lbInput.SelectionMode = SelectionMode.MultiExtended;
                // 
                // btnOK
                // 
                this.btnOK = new System.Windows.Forms.Button();
                this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.btnOK.Location = new System.Drawing.Point(326, 8);
                this.btnOK.Name = "btnOK";
                this.btnOK.Size = new System.Drawing.Size(64, 24);
                this.btnOK.TabIndex = 1;
                this.btnOK.Text = "&OK";
                // 
                // btnCancel
                // 
                this.btnCancel = new System.Windows.Forms.Button();
                this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.btnCancel.Location = new System.Drawing.Point(326, 40);
                this.btnCancel.Name = "btnCancel";
                this.btnCancel.Size = new System.Drawing.Size(64, 24);
                this.btnCancel.TabIndex = 2;
                this.btnCancel.Text = "&Cancel";
                // 
                // btnNew
                // 
                this.btnNew = new System.Windows.Forms.Button();
                this.btnNew.Location = new System.Drawing.Point(326, 72);
                this.btnNew.Name = "btnNew";
                this.btnNew.Size = new System.Drawing.Size(64, 24);
                this.btnNew.TabIndex = 3;
                this.btnNew.Text = "&New";
                this.btnNew.Click += new EventHandler(OnButtonClickNew);
                // 
                // Dialog
                // 
                this.AcceptButton = this.btnOK;
                this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
                this.CancelButton = this.btnCancel;
                this.ClientSize = new System.Drawing.Size(398, 117);
                this.Controls.Add(this.lbInput);
                this.Controls.Add(this.btnCancel);
                this.Controls.Add(this.btnOK);
                this.Controls.Add(this.btnNew);
                this.Controls.Add(this.lblPrompt);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.Name = "Dialog";
                this.ResumeLayout(false);
                this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            }
            
            private void OnButtonClickNew(object sender, EventArgs e)
            {
                string str = InputBox.Show("Add new column", "Add new column", "CustomColumn");
                if (str != null)
                {
                    if (!this.lbInput.Items.Contains(str))
                    {
                        int i = this.lbInput.Items.Add(str);
                        this.lbInput.ClearSelected();
                        this.lbInput.SetSelected(i, true);
                    }
                    else
                    {
                        this.lbInput.ClearSelected();
                        this.lbInput.SetSelected(this.lbInput.Items.IndexOf(str), true);
                    }
                }
            }
        }
    }
}
