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
    #region ThreeStateCheckBoxCell
    public class ThreeStateCheckBoxCell : DataGridViewCheckBoxCell
    {
        private CheckState  m_DefaultValue;

        public ThreeStateCheckBoxCell()
            : base()
        {
        }

        [DefaultValue(CheckState.Indeterminate)]
        public CheckState DefaultValue
        {
            get
            {
                // Use false as the default value.
                // A default threestate checkbox uses null.
                return this.m_DefaultValue;
            }
            set
            {
                this.m_DefaultValue = value;
            }
        }

        public override object DefaultNewRowValue
        {
            get
            {
                // Use false as the default value.
                // A default threestate checkbox uses null.                
                return this.m_DefaultValue;
            }
        }

        public override object Clone()
        {
            ThreeStateCheckBoxCell dataGridViewCell = base.Clone() as ThreeStateCheckBoxCell;
            if (dataGridViewCell != null)
            {
                dataGridViewCell.DefaultValue = this.DefaultValue;
            }
            return dataGridViewCell;
        }
    }
    #endregion

    #region ThreeStateCheckBoxColumn
    public class ThreeStateCheckBoxColumn : DataGridViewCheckBoxColumn
    {
        public ThreeStateCheckBoxColumn()
            : base(true)
        {
            CellTemplate = new ThreeStateCheckBoxCell();
            base.ThreeState = true;
        }

        private ThreeStateCheckBoxCell ThreeStateCellTemplate
        {
            get
            {
                ThreeStateCheckBoxCell cell = this.CellTemplate as ThreeStateCheckBoxCell;
                if (cell == null)
                {
                    throw new InvalidOperationException("ThreeStateCheckBoxCell does not have a CellTemplate.");
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
                ThreeStateCheckBoxCell cell = value as ThreeStateCheckBoxCell;
                if (value != null && cell == null)
                {
                    throw new InvalidCastException("Value provided for CellTemplate must be of type ThreeStateCheckBoxCell or derive from it.");
                }
                base.CellTemplate = value;
            }
        }

        [
            Category("Data"),
            Description("Default value for a new row."),
            Browsable(true),
            DefaultValue(CheckState.Indeterminate),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)
        ]
        public CheckState DefaultValue
        {
            get 
            { 
                return this.ThreeStateCellTemplate.DefaultValue; 
            }
            set
            {
                this.ThreeStateCellTemplate.DefaultValue = value;
            }
        }

        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public new bool ThreeState
        {
            get 
            { 
                return base.ThreeState; 
            }
            set
            {
                throw new InvalidOperationException("ThreeStateCheckBoxColumn only allows ThreeState mode.");
            }
        }
    }
    #endregion
}