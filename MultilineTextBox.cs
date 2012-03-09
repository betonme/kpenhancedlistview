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
#if USE_RTB
    internal class MultilineTextBox : RichTextBox
    {
        private bool hasMouse = false;

        public MultilineTextBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.MouseHover += new EventHandler(this.OnMouseHover);
            this.MouseLeave += new EventHandler(this.OnMouseLeave);
        }

        private void OnMouseHover(object sender, EventArgs e)
        {
            hasMouse = true;
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            hasMouse = false;
        }

        protected override void WndProc(ref Message m)
        {
            // Mouse over TextBox
            if (!hasMouse)
            {
                // Pass WM_MOUSEWHEEL to parent
                if (m.Msg == 0x020a)
                {
                    SendMessage(this.Parent.Handle, m.Msg, m.WParam, m.LParam);
                    m.Result = (IntPtr)0;
                }
                else base.WndProc(ref m);
            }
            else base.WndProc(ref m);
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }
#else
    internal class MultilineTextBox : TextBox
    {
        private bool hasMouse = false;

        public MultilineTextBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.MouseHover += new EventHandler(this.OnMouseHover);
            this.MouseLeave += new EventHandler(this.OnMouseLeave);
        }

        private void OnMouseHover(object sender, EventArgs e)
        {
            hasMouse = true;
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            hasMouse = false;
        }

        protected override void WndProc(ref Message m)
        {
            // Mouse over TextBox
            if (!hasMouse)
            {
                // Pass WM_MOUSEWHEEL to parent
                if (m.Msg == 0x020a)
                {
                    SendMessage(this.Parent.Handle, m.Msg, m.WParam, m.LParam);
                    m.Result = (IntPtr)0;
                }
                else base.WndProc(ref m);
            }
            else base.WndProc(ref m);
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }
#endif
}
