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
#if USE_RTB
        //public RichTextBox tb;
        public MultilineTextBox tb;
#else
        //public TextBox tb;
        public MultilineTextBox tb;
#endif

#if USE_RTB
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
#endif

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
#if USE_RTB
            //this.tb = new RichTextBox();
            this.tb = new MultilineTextBox();
#else
            //this.tb = new TextBox();
            this.tb = new MultilineTextBox();
#endif

            this.SuspendLayout();
            // 
            // tb
            // 
            this.tb.Multiline = false;
#if USE_RTB
            this.tb.ScrollBars = RichTextBoxScrollBars.None;
#else
            this.tb.ScrollBars = System.Windows.Forms.ScrollBars.None;
#endif
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
            get { return tb.Text; }
            set { tb.Text = value; }
        }

        /// <summary>
        /// The ToString() value of the text box. 
        /// </summary>
        public override string ToString()
        {
            return tb.ToString();
        }

        /// <summary>
        /// This property allows the user the ability to set the Multiline property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public bool Multiline
        {
            get { return tb.Multiline; }
            set { tb.Multiline = value; }
        }

        /// <summary>
        /// This property allows the user the ability to set the Lines property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public string[] Lines
        {
            get { return tb.Lines; }
            set { tb.Lines = value; }
        }

        /// <summary>
        /// This property allows the user the ability to set the ReadOnly property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public bool ReadOnly
        {
            get { return tb.ReadOnly; }
            set { tb.ReadOnly = value; }
        }

        /// <summary>
        /// This property allows the user the ability to set the Lines property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public void ScrollBars(bool VerticalScrollBar)
        {
            if (VerticalScrollBar)
            {
#if USE_RTB
                this.tb.ScrollBars = RichTextBoxScrollBars.Vertical;
#else
                this.tb.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
#endif
            }
            else
            {
#if USE_RTB
                this.tb.ScrollBars = RichTextBoxScrollBars.None;
#else
                this.tb.ScrollBars = System.Windows.Forms.ScrollBars.None;
#endif
            }
        }

        /// <summary>
        /// This property allows the user the ability to set the SelectAll property
        /// of the textbox directly without having to use the "tb" property.
        /// </summary>
        public void SelectAll()
        {
            tb.SelectAll();
        }

        /// <summary>
        /// This property allows the user the ability to scroll
        /// the textbox to top directly without having to use the "tb" property.
        /// </summary>
        public void ScrollToTop()
        {
            tb.SelectionStart = 0;
            tb.SelectionLength = 0;
            tb.ScrollToCaret();
        }

        /// <summary>
        /// Set the line spacing of the rich text box
        /// </summary>
#if USE_RTB
#if USE_LS
        public void LineSpacing(int ls)
        {
            PARAFORMAT2 fmt = new PARAFORMAT2();
            fmt.cbSize = Marshal.SizeOf(fmt);
            fmt.dwMask |= PFM_LINESPACING | PFM_SPACEAFTER;

            fmt.dyLineSpacing = ls; 

            // specify exact line spacing
            // 0 Single spacing. The dyLineSpacing member is ignored.
            // 1 One-and-a-half spacing. The dyLineSpacing member is ignored.
            // 2 Double spacing. The dyLineSpacing member is ignored.
            // 3 The dyLineSpacing member specifies the spacingfrom one line to the next, in twips. However, if dyLineSpacing specifies a value that is less than single spacing, the control displays single-spaced text.
            // 4 The dyLineSpacing member specifies the spacing from one line to the next, in twips. The control uses the exact spacing specified, even if dyLineSpacing specifies a value that is less than single spacing.
            // 5 The value of dyLineSpacing / 20 is the spacing, in lines, from one line to the next. Thus, setting dyLineSpacing to 20 produces single-spaced text, 40 is double spaced, 60 is triple spaced, and so on.
            fmt.bLineSpacingRule = Convert.ToByte(5);//3);//4);

            //0 (SCF_DEFAULT) or SCF_ALL(4) - formatting will be applied to all text.
            //1 (SCF_SELECTION) - formatting will be applied only to selected text.
            SendMessage(this.tb.Handle, EM_SETPARAFORMAT, 4, ref fmt);
        }
#endif
#endif

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

        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Tab:
                case Keys.Enter:
                case Keys.Escape:
                //TODO check keys
                // add function to navigate within text
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.Right:
                    this.OnKeyPress(new KeyPressEventArgs((char)keyData));
                    return true;
                default:
                    // nothing to do
                    break;
            }

            return base.ProcessDialogKey(keyData);
        }
    }
}
