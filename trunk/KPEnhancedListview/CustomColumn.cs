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
    public enum HideStatus
    {
        Lazy,
        Full,
        Unhidden
    };

    [Serializable]
    public class CustomColumn : IComparable
    {
        string column;
        string name;
        int index;
        int order;
        bool enable;
        HideStatus hide; //threestate
        bool protect;
        bool readOnly;
        int width;
        SortOrder sort;
        //int sortType;  //enum num nat
        //int sortOrder; //enum asc des none

        public CustomColumn()
        {
            enable = true;
            hide = HideStatus.Unhidden;
//TODO set default width  int nDefaultWidth = m_lvEntries.ClientRectangle.Width / 5;
            width = 100;
            sort = SortOrder.None;
//TODO set index
//TODO set order
        }

        public CustomColumn(string Column, string Name, int Index, int Order, bool Enable, HideStatus Hide, bool Protect, bool ReadOnly, int Width, SortOrder Sort)
        {
            column = Column;
            name = Name;
            index = Index;
            order = Order;
            enable = Enable;
            hide = Hide;
            protect = Protect;
            readOnly = ReadOnly;
            width = Width;
            sort = Sort;
        }

        public string Column
        {
            get { return column; }
            set { column = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public int Index
        {
            get { return index; }
            set { index = value; }
        }
        public int Order
        {
            get { return order; }
            set { order = value; }
        }
        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }
        public HideStatus Hide
        {
            get { return hide; }
            set { hide = value; }
        }
        public bool Protect
        {
            get { return protect; }
            set { protect = value; }
        }
        public bool ReadOnly
        {
            get { return readOnly; }
            set { readOnly = value; }
        }
        public int Width
        {
            get { return width; }
            set { width = value; }
        }
        public SortOrder Sort
        {
            get { return sort; }
            set { sort = value; }
        }

        public int CompareTo(Object o)
        {
            if (o is CustomColumn)
            {
                return this.Order.CompareTo(((CustomColumn)o).Order);
            }
            return 0;
        }
    };
}
