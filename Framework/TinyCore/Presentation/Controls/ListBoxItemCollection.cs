using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class ListBoxItemCollection : ICollection
    {
        UIElementCollection _items;

        public ListBoxItemCollection(ListBox listBox, UIElementCollection items)
        {
            _listBox = listBox;
            _items = items;
        }

        public int Add(ListBoxItem item)
        {
            int pos = _items.Add(item);
            item.SetListBox(_listBox);
            return pos;
        }

        public int Add(UIElement element)
        {
            ListBoxItem item = new ListBoxItem();
            item.Child = element;
            return Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(ListBoxItem item)
        {
            return _items.Contains(item);
        }

        public ListBoxItem this[int index]
        {
            get { return (ListBoxItem)_items[index]; }
            set { _items[index] = value; value.SetListBox(_listBox); }
        }

        public int IndexOf(ListBoxItem item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, ListBoxItem item)
        {
            _items.Insert(index, item);
            item.SetListBox(_listBox);
        }

        public void Remove(ListBoxItem item)
        {
            _items.Remove(item);
            item.SetListBox(null);
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                this[index].SetListBox(null);
            }

            _items.RemoveAt(index);
        }

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            _items.CopyTo(array, index);
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public bool IsSynchronized
        {
            get { return _items.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return _items.SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        #endregion

        private ListBox _listBox;
    }
}


