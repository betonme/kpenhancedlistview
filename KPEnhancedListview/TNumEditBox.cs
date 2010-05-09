using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace CSUST.Data
{
    /// <summary>
    /// design by: csust.hulihui(mailto: ehulh@163.com, home at: http://blog.csdn.net/hulihui)
    /// creation date: 2007-9-17
    /// modified date: 2008-9-29
    /// 1) ContextMenu delete operator is not captueed
    /// 2) add property AllowNegative
    /// 3) call base.OnTextChanged when mouse Paste/Cut/Clear
    /// 4) bug when cut negative number
    /// 5) bug when ReadOnly changed, override property ReadOnly.(2008-9-29)
    /// 6) bug when input 1 0 and 1 but displays 11.(2008-10-23)
    /// 7) use culture number format. (2008-10-23)
    /// 8) do not clear value when input minus. (2008-11-14)
    /// </summary>

    #region TTextBoxEx

    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(TextBox))]
    public class TTextBoxEx : TextBox
    {
        private string m_version;
        private bool m_keepBackColorWhenReadOnly = true;
        private Color m_backColor;
        private Color m_backColorWhenReadOnly;

        public TTextBoxEx()
        {
            this.m_version = "1.0";
            this.ForeColor = Color.Blue;

            m_backColor = base.BackColor;
            base.ReadOnly = true;
            m_backColorWhenReadOnly = base.BackColor;
            base.ReadOnly = false;
        }

        [DefaultValue(typeof(Color), "Blue")]
        public new Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        [DefaultValue(typeof(Color), "Window")]
        public new Color BackColor
        {
            get { return m_backColor; }
            set
            {
                if (m_backColor != value)
                {
                    m_backColor = value;
                    this.OnBackColorChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Custom")]
        [Description("Set/Get if keep backcolor when textbox is readonly.")]
        [DefaultValue(true)]
        public bool KeepBackColorWhenReadOnly
        {
            get { return m_keepBackColorWhenReadOnly; }
            set
            {
                if (m_keepBackColorWhenReadOnly != value)
                {
                    m_keepBackColorWhenReadOnly = value;
                    this.OnBackColorChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Custom")]
        [Description("Get TNumEditBox Version.")]
        public string Version
        {
            get { return m_version; }
            protected set { m_version = value; }
        }

        public new bool ReadOnly
        {
            get { return base.ReadOnly; }
            set
            {
                if (base.ReadOnly != value)
                {
                    base.ReadOnly = value;
                    this.OnBackColorChanged(EventArgs.Empty);
                }
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            if (this.ReadOnly)
            {
                if (m_keepBackColorWhenReadOnly)
                {
                    base.BackColor = m_backColor;
                }
                else
                {
                    base.BackColor = m_backColorWhenReadOnly;
                }
            }
            else
            {
                base.BackColor = m_backColor;
            }
            this.Invalidate();
        }
    }

    #endregion

    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(TextBox))]
    public class TNumEditBox : TTextBoxEx
    {
        #region  member fields

        private const int m_MaxDecimalLength = 10;  // max dot length
        private const int m_MaxValueLength = 27; // decimal can be 28 bits.

        private const int WM_CHAR = 0x0102; // char key message
        private const int WM_CUT = 0x0300;  // mouse message in ContextMenu
        private const int WM_COPY = 0x0301;
        private const int WM_PASTE = 0x0302;
        private const int WM_CLEAR = 0x0303;

        private int m_decimalLength = 0;
        private bool m_allowNegative = true;
        private string m_valueFormatStr = string.Empty;
        
        private char m_decimalSeparator = '.';
        private char m_negativeSign = '-';

        #endregion

        #region  class constructor

        public TNumEditBox()
        {
            base.Version = "1.3";  // 2008-11-24

            base.Multiline = false;
            base.TextAlign = HorizontalAlignment.Right;
            base.Text = "0";

            System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            m_decimalSeparator = ci.NumberFormat.CurrencyDecimalSeparator[0];
            m_negativeSign = ci.NumberFormat.NegativeSign[0];

            this.SetValueFormatStr();
        }

        #endregion

        #region  disabled public properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DefaultValue(typeof(HorizontalAlignment), "Right")]
        public new HorizontalAlignment TextAlign
        {
            get { return base.TextAlign; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string[] Lines
        {
            get { return base.Lines; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new ImeMode ImeMode
        {
            get { return base.ImeMode; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new char PasswordChar
        {
            get { return base.PasswordChar; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool UseSystemPasswordChar
        {
            get { return base.UseSystemPasswordChar; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool Multiline
        {
            get { return base.Multiline; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get { return base.AutoCompleteCustomSource; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new AutoCompleteMode AutoCompleteMode
        {
            get { return base.AutoCompleteMode; }
            set { base.AutoCompleteMode = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new AutoCompleteSource AutoCompleteSource
        {
            get { return base.AutoCompleteSource; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new CharacterCasing CharacterCasing
        {
            get { return base.CharacterCasing; }
        }

        #endregion

        #region override public properties

        [DefaultValue("0")]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                decimal val;
                if (decimal.TryParse(value, out val))
                {
                    base.Text = val.ToString(m_valueFormatStr);
                }
                else
                {
                    base.Text = 0.ToString(m_valueFormatStr);
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override int MaxLength
        {
            get
            {
                return base.MaxLength;
            }
        }

        #endregion

        #region  custom public properties

        [Category("Custom")]
        [Description("Set/Get dot length(0 is integer, 10 is maximum).")]
        [DefaultValue(0)]
        public int DecimalLength
        {
            get
            {
                return m_decimalLength;
            }
            set
            {
                if (m_decimalLength != value)
                {
                    if (value < 0 || value > m_MaxDecimalLength)
                    {
                        m_decimalLength = 0;
                    }
                    else
                    {
                        m_decimalLength = value;
                    }
                    this.SetValueFormatStr();
                    base.Text = this.Value.ToString(m_valueFormatStr);
                }
            }
        }

        [Category("Custom")]
        [Description("Get decimal value of textbox.")]
        public decimal Value
        {
            get
            {
                decimal val;
                if (decimal.TryParse(base.Text, out val))
                {
                    return val;
                }
                return 0;
            }
        }

        [Category("Custom")]
        [Description("Get integer value of textbox.")]
        public int IntValue
        {
            get
            {
                decimal val = this.Value;
                return (int)val;
            }
        }

        [Category("Custom")]
        [Description("Number can be negative or not.")]
        [DefaultValue(true)]
        public bool AllowNegative
        {
            get { return m_allowNegative; }
            set
            {
                if (m_allowNegative != value)
                {
                    m_allowNegative = value;
                }
            }
        }

        #endregion

        #region  override events or methods

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_PASTE)  // mouse paste
            {
                this.ClearSelection();
                SendKeys.Send(Clipboard.GetText());
                base.OnTextChanged(EventArgs.Empty);
            }
            else if (m.Msg == WM_COPY)  // mouse copy
            {
                Clipboard.SetText(this.SelectedText);
            }
            else if (m.Msg == WM_CUT)  // mouse cut or ctrl+x shortcut
            {
                Clipboard.SetText(this.SelectedText);
                this.ClearSelection();
                base.OnTextChanged(EventArgs.Empty);
            }
            else if (m.Msg == WM_CLEAR)
            {
                this.ClearSelection();
                base.OnTextChanged(EventArgs.Empty);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys)Shortcut.CtrlV)
            {
                this.ClearSelection();

                string text = Clipboard.GetText();
                //                SendKeys.Send(text);

                for (int k = 0; k < text.Length; k++) // can not use SendKeys.Send
                {
                    SendCharKey(text[k]);
                }
                return true;
            }
            else if (keyData == (Keys)Shortcut.CtrlC)
            {
                Clipboard.SetText(this.SelectedText);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!this.ReadOnly)
            {
                if (e.KeyData == Keys.Delete || e.KeyData == Keys.Back)
                {
                    if (this.SelectionLength > 0)
                    {
                        this.ClearSelection();
                    }
                    else
                    {
                        this.DeleteText(e.KeyData);
                    }
                    e.SuppressKeyPress = true;  // does not transform event to KeyPress, but to KeyUp
                }
            }

        }

        /// <summary>
        /// repostion SelectionStart, recalculate SelectedLength
        /// </summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (this.ReadOnly)
            {
                return;
            }

            if (e.KeyChar == (char)13 || e.KeyChar == (char)3 || e.KeyChar == (char)22 || e.KeyChar == (char)24)
            {
                return;
            }

            if (m_decimalLength == 0 && e.KeyChar == m_decimalSeparator)
            {
                e.Handled = true;
                return;
            }

            if (!m_allowNegative && e.KeyChar == m_negativeSign && base.Text.IndexOf(m_negativeSign) < 0)
            {
                e.Handled = true;
                return;
            }

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != m_negativeSign && e.KeyChar != m_decimalSeparator)
            {
                e.Handled = true;
                return;
            }

            if (base.Text.Length >= m_MaxValueLength && e.KeyChar != m_negativeSign)
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == m_decimalSeparator || e.KeyChar == m_negativeSign)  // will position after dot(.) or first
            {
                this.SelectionLength = 0;
            }
            else
            {
                this.ClearSelection();
            }

            bool isNegative = (base.Text[0] == m_negativeSign) ? true : false;

            if (isNegative && this.SelectionStart == 0)
            {
                this.SelectionStart = 1;
            }

            if (e.KeyChar == m_negativeSign)
            {
                int selStart = this.SelectionStart;

                if (!isNegative)
                {
                    base.Text = m_negativeSign + base.Text;
                    this.SelectionStart = selStart + 1;
                }
                else
                {
                    base.Text = base.Text.Substring(1, base.Text.Length - 1);
                    if (selStart >= 1)
                    {
                        this.SelectionStart = selStart - 1;
                    }
                    else
                    {
                        this.SelectionStart = 0;
                    }
                }
                e.Handled = true;  // minus(-) has been handled
                return;
            }

            int dotPos = base.Text.IndexOf(m_decimalSeparator) + 1;

            if (e.KeyChar == m_decimalSeparator)
            {
                if (dotPos > 0)
                {
                    this.SelectionStart = dotPos;
                }
                e.Handled = true;  // dot has been handled 
                return;
            }

            if (base.Text == "0")
            {
                this.SelectionStart = 0;
                this.SelectionLength = 1;  // replace thre first char, ie. 0
            }
            else if (base.Text == m_negativeSign + "0")
            {
                this.SelectionStart = 1;
                this.SelectionLength = 1;  // replace thre first char, ie. 0
            }
            else if (m_decimalLength > 0)
            {
                if (base.Text[0] == '0' && dotPos == 2 && this.SelectionStart <= 1)
                {
                    this.SelectionStart = 0;
                    this.SelectionLength = 1;  // replace thre first char, ie. 0
                }
                else if (base.Text.Substring(0, 2) == m_negativeSign + "0" && dotPos == 3 && this.SelectionStart <= 2)
                {
                    this.SelectionStart = 1;
                    this.SelectionLength = 1;  // replace thre first char, ie. 0
                }
                else if (this.SelectionStart == dotPos + m_decimalLength)
                {
                    e.Handled = true;  // last position after text
                }
                else if (this.SelectionStart >= dotPos)
                {
                    this.SelectionLength = 1;
                }
                else if (this.SelectionStart < dotPos - 1)
                {
                    this.SelectionLength = 0;
                }
            }
        }

        /// <summary>
        /// reformat the base.Text
        /// </summary>
        protected override void OnLeave(EventArgs e)
        {
            if (string.IsNullOrEmpty(base.Text))
            {
                base.Text = 0.ToString(m_valueFormatStr);
            }
            else
            {
                base.Text = this.Value.ToString(m_valueFormatStr);
            }
            base.OnLeave(e);
        }

        #endregion

        #region  custom private methods

        private void SetValueFormatStr()
        {
            m_valueFormatStr = "F" + m_decimalLength.ToString();
        }

        private void SendCharKey(char c)
        {
            Message msg = new Message();

            msg.HWnd = this.Handle;
            msg.Msg = WM_CHAR;
            msg.WParam = (IntPtr)c;
            msg.LParam = IntPtr.Zero;

            base.WndProc(ref msg);
        }

        /// <summary>
        /// Delete operator will be changed to BackSpace in order to
        /// uniformly handle the position of deleted digit.
        /// </summary>
        private void DeleteText(Keys key)
        {
            int selStart = this.SelectionStart;  // base.Text will be delete at selStart - 1

            if (key == Keys.Delete)  // Delete key change to BackSpace key, adjust selStart value
            {
                selStart += 1;  // adjust position for BackSpace
                if (selStart > base.Text.Length)  // text end
                {
                    return;
                }

                if (this.IsSeparator(selStart - 1))  // next if delete dot(.) or thousands(;)
                {
                    selStart++;
                }
            }
            else  // BackSpace key
            {
                if (selStart == 0)  // first position
                {
                    return;
                }

                if (this.IsSeparator(selStart - 1)) // char which will be delete is separator
                {
                    selStart--;
                }
            }

            if (selStart == 0 || selStart > base.Text.Length)  // selStart - 1 no digig
            {
                return;
            }

            int dotPos = base.Text.IndexOf(m_decimalSeparator);
            bool isNegative = (base.Text.IndexOf(m_negativeSign) >= 0) ? true : false;

            if (selStart > dotPos && dotPos >= 0)  // delete digit after dot(.)
            {
                base.Text = base.Text.Substring(0, selStart - 1) + base.Text.Substring(selStart, base.Text.Length - selStart) + "0";
                base.SelectionStart = selStart - 1;  // SelectionStart is unchanged
            }
            else // delete digit before dot(.)
            {
                if (selStart == 1 && isNegative)  // delete 1st digit and Text is negative,ie. delete minus(-)
                {
                    if (base.Text.Length == 1)  // ie. base.Text is '-'
                    {
                        base.Text = "0";
                    }
                    else if (dotPos == 1)  // -.* format
                    {
                        base.Text = "0" + base.Text.Substring(1, base.Text.Length - 1);
                    }
                    else
                    {
                        base.Text = base.Text.Substring(1, base.Text.Length - 1);
                    }
                    base.SelectionStart = 0;
                }
                else if (selStart == 1 && (dotPos == 1 || base.Text.Length == 1))  // delete 1st digit before dot(.) or Text.Length = 1
                {
                    base.Text = "0" + base.Text.Substring(1, base.Text.Length - 1);
                    base.SelectionStart = 1;
                }
                else if (isNegative && selStart == 2 && base.Text.Length == 2)  // -* format
                {
                    base.Text = m_negativeSign + "0";
                    base.SelectionStart = 1;
                }
                else if (isNegative && selStart == 2 && dotPos == 2)  // -*.* format
                {
                    base.Text = m_negativeSign + "0" + base.Text.Substring(2, base.Text.Length - 2);
                    base.SelectionStart = 1;
                }
                else  // selStart > 0
                {
                    base.Text = base.Text.Substring(0, selStart - 1) + base.Text.Substring(selStart, base.Text.Length - selStart);
                    base.SelectionStart = selStart - 1;
                }
            }
        }

        /// <summary>
        /// clear base.SelectedText
        /// </summary>
        private void ClearSelection()
        {
            if (this.SelectionLength == 0)
            {
                return;
            }

            if (this.SelectedText.Length == base.Text.Length)
            {
                base.Text = 0.ToString(m_valueFormatStr);
                return;
            }

            int selLength = this.SelectedText.Length;
            if (this.SelectedText.IndexOf(m_decimalSeparator) >= 0)
            {
                selLength--; // selected text contains dot(.), selected length minus 1
            }

            this.SelectionStart += this.SelectedText.Length;  // after selected text
            this.SelectionLength = 0;

            for (int k = 1; k <= selLength; k++)
            {
                this.DeleteText(Keys.Back);
            }
        }

        private bool IsSeparator(int index)
        {
            return this.IsSeparator(base.Text[index]);
        }

        private bool IsSeparator(char c)
        {
            if (c == m_decimalSeparator)
            {
                return true;
            }
            return false;
        }

        #endregion

    }
}

