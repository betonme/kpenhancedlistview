/*
    KPEnhancedListview - Extend the KeePass Listview for inline editing.
    Copyright (C) 2010 - 2012  Frank Glaser  <glaserfrank(at)gmail.com>
    https://github.com/betonme/kpenhancedlistview/
    
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
// Workaround to allow listview scrolling during inline editing
// Alternative maybe: http://www.codeproject.com/Tips/143450/How-to-scroll-a-parent-control-with-mouse-wheel-wh
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
            this.hasMouse = true;
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            this.hasMouse = false;
        }

        protected override void WndProc(ref Message m)
        {
            // Mouse over TextBox
            if (!this.hasMouse)
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
}
