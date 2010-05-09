using System;
using System.Drawing;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;


namespace CSUST.Data
{
    /// <summary>
    /// design by: csust.hulihui(mailto: ehulh@163.com, home at: http://blog.csdn.net/hulihui)
    /// creation date: 2008-11-01
    /// modified date: 2008-11-25
    /// </summary>

    #region TNumEditDataGridViewEditingControl

    class TNumEditDataGridViewEditingControl : TNumEditBox, IDataGridViewEditingControl
    {
        private DataGridView dataGridView;  // grid owning this editing control
        private bool valueChanged;  // editing control's value has changed or not
        private int rowIndex;  //  row index in which the editing control resides

        public TNumEditDataGridViewEditingControl()
        {
            this.TabStop = false;  // control must not be part of the tabbing loop
        }

        public virtual DataGridView EditingControlDataGridView
        {
            get { return this.dataGridView; }
            set { this.dataGridView = value; }
        }

        public virtual object EditingControlFormattedValue
        {
            get
            {
                return GetEditingControlFormattedValue(DataGridViewDataErrorContexts.Formatting);
            }
            set
            {
                this.Text = (string)value;
            }
        }

        public virtual int EditingControlRowIndex
        {
            get { return this.rowIndex; }
            set { this.rowIndex = value; }
        }

        public virtual bool EditingControlValueChanged
        {
            get { return this.valueChanged; }
            set { this.valueChanged = value; }
        }

        /// <summary>
        /// Property which determines which cursor must be used for the editing panel,
        /// i.e. the parent of the editing control.
        /// </summary>
        public virtual Cursor EditingPanelCursor
        {
            get { return Cursors.Default; }
        }

        /// <summary>
        /// Property which indicates whether the editing control needs to be repositioned 
        /// when its value changes.
        /// </summary>
        public virtual bool RepositionEditingControlOnValueChange
        {
            get { return false; }
        }

        /// <summary>
        /// Method called by the grid before the editing control is shown so it can adapt to the 
        /// provided cell style.
        /// </summary>
        public virtual void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            this.Font = dataGridViewCellStyle.Font;
            if (dataGridViewCellStyle.BackColor.A < 255)
            {
                // The NumericUpDown control does not support transparent back colors
                Color opaqueBackColor = Color.FromArgb(255, dataGridViewCellStyle.BackColor);
                this.BackColor = opaqueBackColor;
                this.dataGridView.EditingPanel.BackColor = opaqueBackColor;
            }
            else
            {
                this.BackColor = dataGridViewCellStyle.BackColor;
            }
            this.ForeColor = dataGridViewCellStyle.ForeColor;
        }

        /// <summary>
        /// Method called by the grid on keystrokes to determine if the editing control is
        /// interested in the key or not.
        /// </summary>
        public virtual bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Down:
                case Keys.Up:
                case Keys.Home:
                case Keys.End:
                case Keys.Delete:
                    return true;
            }
            return !dataGridViewWantsInputKey;
        }

        /// <summary>
        /// Returns the current value of the editing control.
        /// </summary>
        public virtual object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return this.Text;
        }

        /// <summary>
        /// Called by the grid to give the editing control a chance to prepare itself for
        /// the editing session.
        /// </summary>
        public virtual void PrepareEditingControlForEdit(bool selectAll)
        {
            if (selectAll)
            {
                this.SelectAll();
            }
            else
            {
                this.SelectionStart = this.Text.Length;
            }
        }

        // End of the IDataGridViewEditingControl interface implementation

        /// <summary>
        /// Small utility function that updates the local dirty state and 
        /// notifies the grid of the value change.
        /// </summary>
        private void NotifyDataGridViewOfValueChange()
        {
            if (!this.valueChanged)
            {
                this.valueChanged = true;
                this.dataGridView.NotifyCurrentCellDirty(true);
            }
        }

        /// <summary>
        /// Listen to the KeyPress notification to know when the value changed, and 
        /// notify the grid of the change.
        /// </summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            
            // The value changes when a digit, the decimal separator or the negative sign is pressed.
            bool notifyValueChange = false;

            if (char.IsDigit(e.KeyChar))
            {
                notifyValueChange = true;
            }
            else
            {
                System.Globalization.NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
                string decimalSeparatorStr = numberFormatInfo.NumberDecimalSeparator;
                string groupSeparatorStr = numberFormatInfo.NumberGroupSeparator;
                string negativeSignStr = numberFormatInfo.NegativeSign;
                if (!string.IsNullOrEmpty(decimalSeparatorStr) && decimalSeparatorStr.Length == 1)
                {
                    notifyValueChange = decimalSeparatorStr[0] == e.KeyChar;
                }
                if (!notifyValueChange && !string.IsNullOrEmpty(groupSeparatorStr) && groupSeparatorStr.Length == 1)
                {
                    notifyValueChange = groupSeparatorStr[0] == e.KeyChar;
                }
                if (!notifyValueChange && !string.IsNullOrEmpty(negativeSignStr) && negativeSignStr.Length == 1)
                {
                    notifyValueChange = negativeSignStr[0] == e.KeyChar;
                }
            }

            if (notifyValueChange)
            {
                // Let the DataGridView know about the value change
                NotifyDataGridViewOfValueChange();
            }
        }

        /// <summary>
        /// Listen to the ValueChanged notification to forward the change to the grid.
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (this.Focused)
            {
                // Let the DataGridView know about the value change
                NotifyDataGridViewOfValueChange();
            }
        }
    }

    #endregion

    #region TNumEditDataGridViewColumn
    public class TNumEditDataGridViewColumn : DataGridViewTextBoxColumn
    {
        private const int m_allowMaxDecimalLength = 6;

        public TNumEditDataGridViewColumn()
        {
            TNumEditDataGridViewCell cell = new TNumEditDataGridViewCell();
            base.CellTemplate = cell;

            base.SortMode = DataGridViewColumnSortMode.Automatic;
            base.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            base.DefaultCellStyle.Format = "F" + this.DecimalLength.ToString();
        }

        private TNumEditDataGridViewCell NumEditCellTemplate
        {
            get
            {
                TNumEditDataGridViewCell cell = this.CellTemplate as TNumEditDataGridViewCell;
                if (cell == null)
                {
                    throw new InvalidOperationException("TNumEditDataGridViewColumn does not have a CellTemplate.");
                }
                return cell;
            }
        }

        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override DataGridViewCell CellTemplate
        {
            get { return base.CellTemplate; }
            set
            {
                TNumEditDataGridViewCell cell = value as TNumEditDataGridViewCell;
                if (value != null && cell == null)
                {
                    throw new InvalidCastException("Value provided for CellTemplate must be of type TEditNumDataGridViewCell or derive from it.");
                }
                base.CellTemplate = value;
            }
        }

        [
            Category("Appearance"),
            DefaultValue(0),
            Description("The decimal length of cell value.")
        ]
        public int DecimalLength
        {
            get { return this.NumEditCellTemplate.DecimalLength; }
            set
            {
                if (value < 0 || value > m_allowMaxDecimalLength)
                {
                    throw new ArgumentOutOfRangeException("The DecimalLength must be between 0 and " + m_allowMaxDecimalLength.ToString() + ".");
                }

                base.DefaultCellStyle.Format = "F" + value.ToString();

                this.NumEditCellTemplate.DecimalLength = value;
                if (this.DataGridView != null)
                {
                    // Update all the existing DataGridViewNumericUpDownCell cells in the column accordingly.
                    DataGridViewRowCollection dataGridViewRows = this.DataGridView.Rows;
                    int rowCount = dataGridViewRows.Count;
                    for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                    {
                        // Be careful not to unshare rows unnecessarily. 
                        // This could have severe performance repercussions.
                        DataGridViewRow dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
                        TNumEditDataGridViewCell dataGridViewCell = dataGridViewRow.Cells[this.Index] as TNumEditDataGridViewCell;
                        if (dataGridViewCell != null)
                        {
                            // Call the internal SetDecimalPlaces method instead of the property to avoid invalidation 
                            // of each cell. The whole column is invalidated later in a single operation for better performance.
                            dataGridViewCell.SetDecimalLength(rowIndex, value);
                        }
                    }
                    this.DataGridView.InvalidateColumn(this.Index);
                    // TODO: Call the grid's autosizing methods to autosize the column, rows, column headers / row headers as needed.
                }
            }
        }

        [
            Category("Appearance"),
            DefaultValue(true),
            Description("Whether negative sign is allowed or not.")
        ]
        public bool AllowNegative
        {
            get { return this.NumEditCellTemplate.AllowNegative; }
            set
            {
                this.NumEditCellTemplate.AllowNegative = value;
                if (this.DataGridView != null)
                {
                    DataGridViewRowCollection dataGridViewRows = this.DataGridView.Rows;
                    int rowCount = dataGridViewRows.Count;
                    for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                    {
                        DataGridViewRow dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
                        TNumEditDataGridViewCell dataGridViewCell = dataGridViewRow.Cells[this.Index] as TNumEditDataGridViewCell;
                        if (dataGridViewCell != null)
                        {
                            dataGridViewCell.EnableNegative(rowIndex, value);
                        }
                    }
                }
            }
        }

        [
            Category("Appearance"),
            DefaultValue(false),
            Description("Whether show null when value is zero or not.")
        ]
        public bool ShowNullWhenZero
        {
            get { return this.NumEditCellTemplate.ShowNullWhenZero; }
            set
            {
                this.NumEditCellTemplate.ShowNullWhenZero = value;
                if (this.DataGridView != null)
                {
                    DataGridViewRowCollection dataGridViewRows = this.DataGridView.Rows;
                    int rowCount = dataGridViewRows.Count;
                    for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                    {
                        DataGridViewRow dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
                        TNumEditDataGridViewCell dataGridViewCell = dataGridViewRow.Cells[this.Index] as TNumEditDataGridViewCell;
                        if (dataGridViewCell != null)
                        {
                            dataGridViewCell.EnableShowNullWhenZero(rowIndex, value);
                        }
                    }
                    this.DataGridView.InvalidateColumn(this.Index);
                }
            }
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(100);
            sb.Append("TNumEditDataGridViewColumn { Name=");
            sb.Append(this.Name);
            sb.Append(", Index=");
            sb.Append(this.Index.ToString(CultureInfo.CurrentCulture));
            sb.Append(" }");
            return sb.ToString();
        }
    }

    #endregion

    #region TNumEditDataGridViewCell

    public class TNumEditDataGridViewCell : DataGridViewTextBoxCell
    {
        private bool m_showNullWhenZero = false;
        private bool m_allowNegative = true;
        private int m_decimalLength = 0;

        private static Type defaultEditType = typeof(TNumEditDataGridViewEditingControl);
        private static Type defaultValueType = typeof(System.Decimal);

        public TNumEditDataGridViewCell(){}

        [DefaultValue(0)]
        public int DecimalLength
        {
            get { return m_decimalLength; }
            set
            {
                if (m_decimalLength != value)
                {
                    SetDecimalLength(this.RowIndex, value);
                    OnCommonChange();  // Assure that the cell/column gets repainted and autosized if needed
                }
            }
        }

        [DefaultValue(true)]
        public bool AllowNegative
        {
            get { return m_allowNegative; }
            set
            {
                if (m_allowNegative != value)
                {
                    EnableNegative(this.RowIndex, value);
                }
            }
        }

        [DefaultValue(false)]
        public bool ShowNullWhenZero
        {
            get { return m_showNullWhenZero; }
            set
            {
                if (m_showNullWhenZero != value)
                {
                    EnableShowNullWhenZero(this.RowIndex, value);
                    OnCommonChange();
                }
            }
        }

        private TNumEditDataGridViewEditingControl EditingTNumEditBox
        {
            get
            {
                return this.DataGridView.EditingControl as TNumEditDataGridViewEditingControl;
            }
        }

        public override Type EditType
        {
            get { return defaultEditType; }
        }

        public override Type ValueType
        {
            get
            {
                Type valueType = base.ValueType;
                if (valueType != null)
                {
                    return valueType;
                }
                return defaultValueType;
            }
        }

        /// <summary>
        /// If set 0/1.23 to two cells, it will throw Exception when sort by clicking column header.
        /// Override this method to ensure the type of value.
        /// </summary>
        protected override bool SetValue(int rowIndex, object value)
        {
            decimal val = 0.0m;
            try
            {
                val = Math.Round(System.Convert.ToDecimal(value), m_decimalLength);
            }
            catch { }
            return base.SetValue(rowIndex, val);  // if set 0 and 1.23, it will throw exception when sort
        }

        public override object Clone()
        {
            TNumEditDataGridViewCell dataGridViewCell = base.Clone() as TNumEditDataGridViewCell;
            if (dataGridViewCell != null)
            {
                dataGridViewCell.DecimalLength = this.DecimalLength;
                dataGridViewCell.AllowNegative = this.AllowNegative;
                dataGridViewCell.ShowNullWhenZero = this.ShowNullWhenZero;
            }
            return dataGridViewCell;
        }

        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            TNumEditBox numEditBox = this.DataGridView.EditingControl as TNumEditBox;
            if (numEditBox != null)
            {
                numEditBox.BorderStyle = BorderStyle.None;
                numEditBox.DecimalLength = this.DecimalLength;
                numEditBox.AllowNegative = this.AllowNegative;

                string initialFormattedValueStr = initialFormattedValue as string;

                if (string.IsNullOrEmpty(initialFormattedValueStr))
                {
                    numEditBox.Text = "0";
                }
                else
                {
                    numEditBox.Text = initialFormattedValueStr;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override void DetachEditingControl()
        {
            DataGridView dataGridView = this.DataGridView;
            if (dataGridView == null || dataGridView.EditingControl == null)
            {
                throw new InvalidOperationException("Cell is detached or its grid has no editing control.");
            }

            TNumEditBox numEditBox = dataGridView.EditingControl as TNumEditBox;
            if (numEditBox != null)
            {
                numEditBox.ClearUndo();  // avoid interferences between the editing sessions
            }

            base.DetachEditingControl();
        }

        /// <summary>
        /// Consider the decimal in the formatted representation of the cell value.
        /// </summary>
        protected override object GetFormattedValue(object value,
                                                    int rowIndex,
                                                    ref DataGridViewCellStyle cellStyle,
                                                    TypeConverter valueTypeConverter,
                                                    TypeConverter formattedValueTypeConverter,
                                                    DataGridViewDataErrorContexts context)
        {
            object baseFormattedValue = base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
            string formattedText = baseFormattedValue as string;

            if (value == null || string.IsNullOrEmpty(formattedText))
            {
                return baseFormattedValue;
            }

            Decimal unformattedDecimal = System.Convert.ToDecimal(value); // 123.1 to "123.1"
            Decimal formattedDecimal = System.Convert.ToDecimal(formattedText); // 123.1 to "123.12" if DecimalLength is 2

            if (unformattedDecimal == 0.0m && m_showNullWhenZero)
            {
                return base.GetFormattedValue(null, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
            }

            if (unformattedDecimal == formattedDecimal)
            {
                return formattedDecimal.ToString("F" + m_decimalLength.ToString());
            }
            return formattedText;
        }

        private void OnCommonChange()
        {
            if (this.DataGridView != null && !this.DataGridView.IsDisposed && !this.DataGridView.Disposing)
            {
                if (this.RowIndex == -1)
                {
                    this.DataGridView.InvalidateColumn(this.ColumnIndex);
                }
                else
                {
                    this.DataGridView.UpdateCellValue(this.ColumnIndex, this.RowIndex);
                }
            }
        }

        private bool OwnsEditingControl(int rowIndex)
        {
            if (rowIndex == -1 || this.DataGridView == null)
            {
                return false;
            }
            TNumEditDataGridViewEditingControl editingControl = this.DataGridView.EditingControl as TNumEditDataGridViewEditingControl;
            return editingControl != null && rowIndex == ((IDataGridViewEditingControl)editingControl).EditingControlRowIndex;
        }

        internal void SetDecimalLength(int rowIndex, int value)
        {
            m_decimalLength = value;
            if (OwnsEditingControl(rowIndex))
            {
                this.EditingTNumEditBox.DecimalLength = value;
            }
        }

        internal void EnableNegative(int rowIndex, bool value)
        {
            m_allowNegative = value;
            if (OwnsEditingControl(rowIndex))
            {
                this.EditingTNumEditBox.AllowNegative = value;
            }
        }

        internal void EnableShowNullWhenZero(int rowIndex, bool value)
        {
            m_showNullWhenZero = value;
        }

        public override string ToString()
        {
            return "TNumEditDataGridViewCell { ColIndex=" + ColumnIndex.ToString(CultureInfo.CurrentCulture) + 
                ", RowIndex=" + RowIndex.ToString(CultureInfo.CurrentCulture) + " }";
        }
    }

    #endregion
}
