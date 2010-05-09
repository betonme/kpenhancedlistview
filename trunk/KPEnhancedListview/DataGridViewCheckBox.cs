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
    #region DataGridViewDisableCheckBoxCell
    public class DataGridViewDisableCheckBoxCell : DataGridViewCheckBoxCell
    {
        private bool enabledValue;
        /// <summary>
        /// This property decides whether the checkbox should be shown checked or unchecked.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return enabledValue;
            }
            set
            {
                enabledValue = value;
            }
        }
        /// Override the Clone method so that the Enabled property is copied.
        public override object Clone()
        {
            DataGridViewDisableCheckBoxCell cell =
                (DataGridViewDisableCheckBoxCell)base.Clone();
            cell.Enabled = this.Enabled;
            return cell;
        }


        public DataGridViewDisableCheckBoxCell()
        {

        }
        /// <summary>
        /// Override the Paint method to show the disabled checked/unchecked datagridviewcheckboxcell.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="clipBounds"></param>
        /// <param name="cellBounds"></param>
        /// <param name="rowIndex"></param>
        /// <param name="elementState"></param>
        /// <param name="value"></param>
        /// <param name="formattedValue"></param>
        /// <param name="errorText"></param>
        /// <param name="cellStyle"></param>
        /// <param name="advancedBorderStyle"></param>
        /// <param name="paintParts"></param>
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
            int rowIndex, DataGridViewElementStates elementState, object value,
            object formattedValue, string errorText, DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            //base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            SolidBrush cellBackground = new SolidBrush(cellStyle.BackColor);
            graphics.FillRectangle(cellBackground, cellBounds);
            cellBackground.Dispose();
            PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            Rectangle checkBoxArea = cellBounds;
            Rectangle buttonAdjustment = this.BorderWidths(advancedBorderStyle);
            checkBoxArea.X += buttonAdjustment.X;
            checkBoxArea.Y += buttonAdjustment.Y;

            checkBoxArea.Height -= buttonAdjustment.Height;
            checkBoxArea.Width -= buttonAdjustment.Width;
            Point drawInPoint = new Point(cellBounds.X + cellBounds.Width / 2 - 7, cellBounds.Y + cellBounds.Height / 2 - 7);

            if (this.enabledValue)
                CheckBoxRenderer.DrawCheckBox(graphics, drawInPoint, System.Windows.Forms.VisualStyles.CheckBoxState.CheckedDisabled);
            else
                CheckBoxRenderer.DrawCheckBox(graphics, drawInPoint, System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedDisabled);


        }
    }
    #endregion

    #region DataGridViewDisableCheckBoxColumn
    public class DataGridViewDisableCheckBoxColumn : DataGridViewCheckBoxColumn
    {
        public DataGridViewDisableCheckBoxColumn()
        {
            this.CellTemplate = new DataGridViewDisableCheckBoxCell();
        }
    }   
    #endregion
}