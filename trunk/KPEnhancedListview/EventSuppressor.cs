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
    public class EventSuppressor
    {
        Control _source;
        EventHandlerList _sourceEventHandlerList;
        FieldInfo _headFI;

        Dictionary<object, Delegate[]> _suppressedHandlers = new Dictionary<object, Delegate[]>();

        PropertyInfo _sourceEventsInfo;
        Type _eventHandlerListType;
        Type _sourceType;

        //BindingFlags _bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
        BindingFlags _bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase;

        public EventSuppressor(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control", "An instance of a control must be provided.");

            this._source = control;
            this._sourceType = this._source.GetType();
            this._sourceEventsInfo = this._sourceType.GetProperty("Events", this._bindingAttr);
            this._sourceEventHandlerList = (EventHandlerList)this._sourceEventsInfo.GetValue(_source, null);
            this._eventHandlerListType = this._sourceEventHandlerList.GetType();
            this._headFI = _eventHandlerListType.GetField("head", this._bindingAttr);
        }

        private Dictionary<object, Delegate[]> BuildList()
        {
            Dictionary<object, Delegate[]> retval = new Dictionary<object, Delegate[]>();
            object head = this._headFI.GetValue(_sourceEventHandlerList);
            if (head != null)
            {
                Type listEntryType = head.GetType();
                FieldInfo delegateFI = listEntryType.GetField("handler", this._bindingAttr);
                FieldInfo keyFI = listEntryType.GetField("key", this._bindingAttr);
                FieldInfo nextFI = listEntryType.GetField("next", this._bindingAttr);
                retval = BuildListWalk(retval, head, delegateFI, keyFI, nextFI);
            }
            return retval;
        }

        private Dictionary<object, Delegate[]> BuildListWalk(Dictionary<object, Delegate[]> dict,
                                    object entry, FieldInfo delegateFI, FieldInfo keyFI, FieldInfo nextFI)
        {
            if (entry != null)
            {
                Delegate dele = (Delegate)delegateFI.GetValue(entry);
                object key = keyFI.GetValue(entry);
                object next = nextFI.GetValue(entry);

                if (dele != null)
                {
                    Delegate[] listeners = dele.GetInvocationList();
                    if (listeners != null && listeners.Length > 0)
                    {
                        dict.Add(key, listeners);
                    }
                }
                if (next != null)
                {
                    dict = BuildListWalk(dict, next, delegateFI, keyFI, nextFI);
                }
            }
            return dict;
        }

        public void Resume()
        {
            Resume(null);
        }

        public void Resume(string pMethodName)
        {
            //if (_handlers == null)
            //    throw new ApplicationException("Events have not been suppressed.");
            Dictionary<object, Delegate[]> toRemove = new Dictionary<object, Delegate[]>();

            // goes through all handlers which have been suppressed.  If we are resuming,
            // all handlers, or if we find the matching handler, add it back to the
            // control's event handlers
            foreach (KeyValuePair<object, Delegate[]> pair in _suppressedHandlers)
            {

                for (int x = 0; x < pair.Value.Length; x++)
                {

                    string methodName = pair.Value[x].Method.Name;
                    if (pMethodName == null || methodName.Equals(pMethodName))
                    {
                        this._sourceEventHandlerList.AddHandler(pair.Key, pair.Value[x]);
                        toRemove.Add(pair.Key, pair.Value);
                    }
                }
            }
            // remove all un-suppressed handlers from the list of suppressed handlers
            foreach (KeyValuePair<object, Delegate[]> pair in toRemove)
            {
                for (int x = 0; x < pair.Value.Length; x++)
                {
                    this._suppressedHandlers.Remove(pair.Key);
                }
            }
            //_handlers = null;
        }

        public void Suppress()
        {
            Suppress(null);
        }

        public void Suppress(string pMethodName)
        {
            //if (_handlers != null)
            //    throw new ApplicationException("Events are already being suppressed.");

            Dictionary<object, Delegate[]> dict = BuildList();

            foreach (KeyValuePair<object, Delegate[]> pair in dict)
            {
                for (int x = pair.Value.Length - 1; x >= 0; x--)
                {
                    //MethodInfo mi = pair.Value[x].Method;
                    //string s1 = mi.Name; // name of the method
                    //object o = pair.Value[x].Target;
                    // can use this to invoke method    pair.Value[x].DynamicInvoke
                    string methodName = pair.Value[x].Method.Name;

                    if (pMethodName == null || methodName.Equals(pMethodName))
                    {
                        this._sourceEventHandlerList.RemoveHandler(pair.Key, pair.Value[x]);
                        this._suppressedHandlers.Add(pair.Key, pair.Value);
                    }
                }
            }
        }
    }
}
