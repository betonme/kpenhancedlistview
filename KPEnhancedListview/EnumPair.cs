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
    /// Represents a <see cref="EnumPair"/> consisting of an value 
    /// of an enum T and a string represantion of the value.
    /// </summary>
    /// <remarks>
    /// With this generic class every <see cref="Enum"/> can be
    /// dynamically enhanced by additional values, such as an empty
    /// entry, which is usefull in beeing used with 
    /// <see cref="ComboBox"/>es.
    /// </remarks>
    /// <typeparam name="T">The type of the <see cref="Enum"/> to represent.</typeparam>
    public class EnumPair<T>
    {
        #region Constants

        public const string ValueMember = "EnumValue";
        public const string DisplayMember = "EnumStringValue";

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumPair"/> class.
        /// </summary>
        public EnumPair()
        {
            Type t = typeof(T);
            if (!t.IsEnum)
            {
                throw new ArgumentException("Class EnumPair<T> can only be instantiated with Enum-Types!");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumPair"/> class.
        /// </summary>
        /// <param name="value">The value of the enum.</param>
        /// <param name="stringValue">The <see cref="string"/> value of the enum.</param>
        public EnumPair(T value, string stringValue)
        {
            Type t = typeof(T);
            if (!t.IsEnum)
            {
                throw new ArgumentException("Class EnumPair<T> can only be instantiated with Enum-Types!");
            }

            this.EnumValue = value;
            this.EnumStringValue = stringValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value part of the <see cref="EnumPair"/>.
        /// </summary>
        public T EnumValue { get; set; }

        /// <summary>
        /// Gets or sets the string value of the <see cref="EnumPair"/>.
        /// </summary>
        public string EnumStringValue { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="EnumPair"/>.
        /// </summary>
        public override string ToString()
        {
            return this.EnumStringValue;
        }

        /// <summary>
        /// Generates a <see cref="List<T>"/> of the values
        /// of the <see cref="Enum"/> T.
        /// </summary>
        public static List<EnumPair<T>> GetValuePairList()
        {
            List<EnumPair<T>> list = new List<EnumPair<T>>();
            EnumPair<T> pair = new EnumPair<T>();

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                pair = new EnumPair<T>();
                pair.EnumValue = (T)item;
                pair.EnumStringValue = ((T)item).ToString();
                list.Add(pair);
            }

            return list;
        }

        /// <summary>
        /// Implicit conversion from enum value to <see cref="EnumPair<>"/> from that enum.
        /// </summary>
        /// <param name="e">The enum value to convert to.</param>
        /// <returns>A <see cref="EnumPair<>"/> to the enum value.</returns>
        public static implicit operator EnumPair<T>(T e)
        {
            Type t = typeof(EnumPair<>).MakeGenericType(e.GetType());
            return new EnumPair<T>((T)e, ((T)e).ToString());
        }

        #endregion
    }

}
