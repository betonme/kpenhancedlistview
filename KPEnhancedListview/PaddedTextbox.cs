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

namespace KPEnhancedListview
{
    /// <summary>
    /// Padded TextBox
    /// </summary>
    internal class PaddedTextBox : UserControl
    {
        //public RichTextBox tb;
        public MultilineTextBox tb;

        // RichTextBox Line spacing
        private const int PFM_SPACEAFTER = 128;
        private const int PFM_LINESPACING = 256;
        private const int EM_SETPARAFORMAT = 1095;

        [StructLayout(LayoutKind.Sequential)]
        public struct PARAFORMAT2
        {
            public int cbSize;
            public uint dwMask;
            public short wNumbering;
            public short wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public short wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;
            // PARAFORMAT2 from here onwards.
            public int dySpaceBefore;
            public int dySpaceAfter;
            public int dyLineSpacing;
            public short sStyle;
            public byte bLineSpacingRule;
            public byte bOutlineLevel;
            public short wShadingWeight;
            public short wShadingStyle;
            public short wNumberingStart;
            public short wNumberingStyle;
            public short wNumberingTab;
            public short wBorderSpace;
            public short wBorderWidth;
            public short wBorders;
        }

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref PARAFORMAT2 lParam);

        /// <summary>
        /// PaddedTextBox is a simple user control which provides the missing "Padding"
        /// property to a text box.  This allows the user to define a border
        /// around the text.  This border can be the same color as the text box
        /// in which case it is simply inner padding or it can be a true border
        /// of a different color.
        /// <remarks>
        /// The two properties of import are the padding size and the background
        /// color of the control which acts as the border color for the text box.
        /// The text box itself is exposed via the "tb" control which is
        /// public.
        /// </remarks>
        /// </summary>
        public PaddedTextBox()
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //this.tb = new RichTextBox();
            this.tb = new MultilineTextBox();
            this.SuspendLayout();
            // 
            // tb
            // 
            this.tb.Multiline = false;
            this.tb.ScrollBars = RichTextBoxScrollBars.None;
            this.tb.Enabled = true;
            this.tb.ReadOnly = false;
            this.tb.BackColor = System.Drawing.SystemColors.Window;
            this.tb.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb.ForeColor = System.Drawing.SystemColors.WindowText;
            this.tb.Location = new System.Drawing.Point(0, 0);
            this.tb.Name = "tb";
            this.tb.Size = new System.Drawing.Size(148, 13);
            this.tb.TabIndex = 0;

            this.tb.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._tb_KeyPress);
            this.tb.Leave += new System.EventHandler(this._tb_Leave);
            this.tb.LostFocus += new System.EventHandler(this._tb_LostFocus);
            this.tb.MouseCaptureChanged += new System.EventHandler(this._tb_MouseCaptureChanged);
            this.tb.TextChanged += new System.EventHandler(this._tb_TextChanged);
            // 
            // PaddedTextBox
            // 
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tb);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ForeColor = System.Drawing.Color.Black;
            this.Name = "PaddedTextBox";
            this.Size = new System.Drawing.Size(148, 148);
            //
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        /// <summary>
        /// This property allows the user the ability to set the text property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public override string Text
        {
            get { return this.tb.Text; }
            set { this.tb.Text = value; }
        }

        /// <summary>
        /// The ToString() value of the text box. 
        /// </summary>
        public override string ToString()
        {
            return this.tb.ToString();
        }

        /// <summary>
        /// This property allows the user the ability to set the Multiline property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public bool Multiline
        {
            get { return this.tb.Multiline; }
            set { this.tb.Multiline = value; }
        }

        /// <summary>
        /// This property allows the user the ability to set the Lines property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public string[] Lines
        {
            get { return this.tb.Lines; }
            set { this.tb.Lines = value; }
        }

        /// <summary>
        /// This property allows the user the ability to set the ReadOnly property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public bool ReadOnly
        {
            get { return this.tb.ReadOnly; }
            set { this.tb.ReadOnly = value; }
        }

        /// <summary>
        /// This property allows the user the ability to set the Lines property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public void ScrollBars(bool VerticalScrollBar)
        {
            if (VerticalScrollBar)
            {
                this.tb.ScrollBars = RichTextBoxScrollBars.Vertical;
            }
            else
            {
                this.tb.ScrollBars = RichTextBoxScrollBars.None;
            }
        }

        /// <summary>
        /// This property allows the user the ability to set the SelectAll property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public void SelectAll()
        {
            //this.tb.SelectAll(); // Focus is at the end
            //SendKeys.Send("^{END}"); SendKeys.Send("^+{HOME}"); // Sound will be played if textbox is empty
            SendKeys.Send("^a");
        }

        /// <summary>
        /// This property allows the user the ability to scroll
        /// the textbox to top directly without having to use the "tb" property.
        /// </summary>
        public void ScrollToTop()
        {
            this.tb.SelectionStart = 0;
            this.tb.SelectionLength = 0;
            if (this.tb.Lines.Length > 0 && this.tb.TextLength > 0)
            {
                this.tb.ScrollToCaret();
            }
        }

        public int SelectionLineStart
        {
            get { return this.tb.GetLineFromCharIndex(this.tb.SelectionStart); }
        }

        public int SelectionLineEnd
        {
            get { return this.tb.GetLineFromCharIndex(this.tb.SelectionStart + this.tb.SelectionLength); }
        }

        public int LineCount
        {
            get { return this.tb.Lines.Length; }
        }

        public int LastLine
        {
            get { return Math.Max(this.tb.Lines.Length - 1, 0); }
        }

        public int SelectionStart
        {
            get { return this.tb.SelectionStart; }
        }
        
        public int SelectionEnd
        {
            get { return this.tb.SelectionStart + this.tb.SelectionLength; }
        }

        public int TextLength
        {
            get { return this.tb.TextLength; }
        }

        public int LastCharIndex
        {
            get { return Math.Max(this.tb.TextLength, 0); }
        }

        public int CalculateHeight
        {
            //get { return this.tb.PreferredHeight + this.Padding.Vertical; }
            //get { return Math.Max(this.tb.Lines.Length, 1) * this.tb.Font.Height + this.tb.Margin.Vertical + this.Padding.Vertical; } // to heigh
            //get { return Math.Max(this.tb.Lines.Length, 1) * this.tb.Font.Height + this.tb.Margin.Vertical + 1/*Border*/; } // to heigh
            get { return Math.Max(this.tb.Lines.Length, 1) * this.tb.Font.Height + this.Padding.Vertical + 2; }
        }

        private void _tb_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        private void _tb_Leave(object sender, EventArgs e)
        {
            this.OnLeave(e);
        }

        private void _tb_LostFocus(object sender, EventArgs e)
        {
            this.OnLostFocus(e);
        }

        private void _tb_MouseCaptureChanged(object sender, EventArgs e)
        {
            this.OnMouseCaptureChanged(e);
        }
        
        private void _tb_TextChanged(object sender, EventArgs e)
        {
            if (this.Multiline)
            {
                this.Height = this.CalculateHeight;
                // Save
                int start = this.tb.SelectionStart;
                int length = this.tb.SelectionLength;
                // Reset
                this.tb.SelectionStart = 0;
                this.tb.SelectionLength = 0;
                // Scroll
                this.tb.ScrollToCaret();
                // Restore
                this.tb.SelectionStart = start;
                this.tb.SelectionLength = length;
            }
        }
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Enter:
                case Keys.Escape:
                case Keys.Tab:
                case Keys.Shift | Keys.Tab:
                case Keys.Control | Keys.Up:
                case Keys.Control | Keys.Down:
                case Keys.Control | Keys.Left:
                case Keys.Control | Keys.Right:
                    this.OnKeyPress(new KeyPressEventArgs((char)keyData));
                    return true;
                case Keys.Up:
                    if (this.SelectionLineStart == 0)
                    {
                        this.OnKeyPress(new KeyPressEventArgs((char)keyData));
                        return true;
                    }
                    break;
                case Keys.Down:
                    if (this.SelectionLineEnd == this.LastLine)
                    {
                        this.OnKeyPress(new KeyPressEventArgs((char)keyData));
                        return true;
                    }
                    break;
                case Keys.Left:
                    if (this.SelectionStart == 0)
                    {
                        this.OnKeyPress(new KeyPressEventArgs((char)keyData));
                        return true;
                    }
                    break;
                case Keys.Right:
                    if (this.SelectionEnd == this.LastCharIndex)
                    {
                        this.OnKeyPress(new KeyPressEventArgs((char)keyData));
                        return true;
                    }
                    break;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
