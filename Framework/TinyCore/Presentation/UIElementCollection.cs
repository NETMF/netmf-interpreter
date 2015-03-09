using System;
using System.Collections;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Presentation
{
    /// <summary>
    /// A UIElementCollection is a ordered collection of UIElements.
    /// </summary>
    /// <remarks>
    /// A UIElementCollection has implied context affinity. It is a violation to access
    /// the collection from a different context than that of the owning Element
    ///
    /// This collection is an amalgam of UIElementCollection and UIElementCollection from Avalon
    ///
    /// </remarks>
    public class UIElementCollection : ICollection
    {

        public UIElementCollection(UIElement owner)
        {
            Debug.Assert(owner != null);
            _owner = owner;
        }

        public virtual int Count
        {
            get
            {
                return _size;
            }
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual object SyncRoot
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Copies the UIElement collection to the specified array starting at the specified index.
        /// </summary>
        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if ((index < 0) ||
                (array.Length - index < _size))
            {
                throw new ArgumentOutOfRangeException("index");
            }

            Array.Copy(_items, 0, array, index, _size);
        }

        /// <summary>
        /// Strongly typed version of CopyTo
        /// Copies the collection into the Array.
        /// </summary>
        public virtual void CopyTo(UIElement[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        // ----------------------------------------------------------------
        // ArrayList like operations for the UIElementCollection
        // ----------------------------------------------------------------

        /// <summary>
        /// Ensures that the capacity of this list is at least the given minimum
        /// value. If the currect capacity of the list is less than min, the
        /// capacity is increased to min.
        /// </summary>
        private void EnsureCapacity(int min)
        {
            if (Capacity < min)
            {
                Capacity = System.Math.Max(min, (int)(Capacity * c_growFactor));
            }
        }

        /// <summary>
        /// Gets or sets the number of elements that the UIElementCollection can contain.
        /// </summary>
        /// <value>
        /// The number of elements that the UIElementCollection can contain.
        /// </value>
        /// <remarks>
        /// Capacity is the number of elements that the UIElementCollection is capable of storing.
        /// Count is the number of UIElements that are actually in the UIElementCollection.
        ///
        /// Capacity is always greater than or equal to Count. If Count exceeds
        /// Capacity while adding elements, the capacity of the UIElementCollection is increased.
        ///
        /// By default the capacity is 0.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Capacity is set to a value that is less than Count.</exception>
        public virtual int Capacity
        {
            get
            {
                return _items != null ? _items.Length : 0;
            }

            set
            {
                int currentCapacity = _items != null ? _items.Length : 0;
                if (value != currentCapacity)
                {
                    if (value < _size)
                    {
                        throw new ArgumentOutOfRangeException("value", "not enough capacity");
                    }

                    if (value > 0)
                    {
                        UIElement[] newItems = new UIElement[value];
                        if (_size > 0)
                        {
                            Debug.Assert(_items != null);
                            Array.Copy(_items, 0, newItems, 0, _size);
                        }

                        _items = newItems;
                    }
                    else
                    {
                        Debug.Assert(value == 0, "There shouldn't be a case where value != 0.");
                        Debug.Assert(_size == 0, "Size must be 0 here.");
                        _items = null;
                    }
                }
            }
        }

        /// <summary>
        /// Indexer for the UIElementCollection. Gets or sets the UIElement stored at the
        /// zero-based index of the UIElementCollection.
        /// </summary>
        /// <remarks>This property provides the ability to access a specific UIElement in the
        /// UIElementCollection by using the following systax: <c>myUIElementCollection[index]</c>.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><c>index</c> is less than zero -or- <c>index</c> is equal to or greater than Count.</exception>
        /// <exception cref="ArgumentException">If the new child has already a parent or if the slot a the specified index is not null.</exception>
        public UIElement this[int index]
        {
            get
            {
                if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException("index");
                return _items[index];
            }

            set
            {
                _owner.VerifyAccess();

                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (index < 0 || index >= _size) throw new ArgumentOutOfRangeException("index");

                UIElement child = _items[index];

                if (child != value)
                {
                    if ((value == null) && (child != null))
                    {
                        DisconnectChild(index);
                    }
                    else if (value != null)
                    {
                        if ((value._parent != null) || // Only a visual that isn't a visual parent or
                            (value.GetIsRootElement())) // are a root node of a visual target can be set into the collection.
                        {
                            throw new System.ArgumentException("element has parent");
                        }

                        ConnectChild(index, value);
                    }

                    _owner.InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Sets the specified element at the specified index into the child
        /// collection. It also corrects the parent.
        /// Note that the function requires that _item[index] == null and it
        /// also requires that the passed in child is not connected to another UIElement.
        /// </summary>
        /// <exception cref="ArgumentException">If the new child has already a parent or if the slot a the specified index is not null.</exception>
        private void ConnectChild(int index, UIElement value)
        {
            // Every function that calls this function needs to call VerifyAccess to prevent
            // foreign threads from changing the tree.
            //
            // We also need to ensure that the tree is homogenous with respect
            // to the dispatchers that the elements belong to.
            //
            value.VerifyAccess();

            Debug.Assert(_items[index] == null);

            value._parent = _owner;

            // The child might be dirty. Hence we need to propagate dirty information
            // from the parent and from the child.
            UIElement.PropagateFlags(_owner, UIElement.Flags.IsSubtreeDirtyForRender);
            UIElement.PropagateFlags(value, UIElement.Flags.IsSubtreeDirtyForRender);
            value._flags |= UIElement.Flags.IsDirtyForRender;
            _items[index] = value;
            _version++;

            UIElement.PropagateResumeLayout(value);

            // Fire notifications
            _owner.OnChildrenChanged(value, null /* no removed child */, index);
        }

        /// <summary>
        /// Disconnects a child.
        /// </summary>
        private void DisconnectChild(int index)
        {
            Debug.Assert(_items[index] != null);

            UIElement child = _items[index];

            // Every function that calls this function needs to call VerifyAccess to prevent
            // foreign threads from changing the tree.

            UIElement oldParent = child._parent;

            _items[index] = null;

            child._parent = null;
            UIElement.PropagateFlags(_owner, UIElement.Flags.IsSubtreeDirtyForRender);
            _version++;

            UIElement.PropagateSuspendLayout(child);

            oldParent.OnChildrenChanged(null /* no child added */, child, index);
        }

        /// <summary>
        /// Appends a UIElement to the end of the UIElementCollection.
        /// </summary>
        /// <param name="visual">The UIElement to be added to the end of the UIElementCollection.</param>
        /// <returns>The UIElementCollection index at which the UIElement has been added.</returns>
        /// <remarks>Adding a null is allowed.</remarks>
        /// <exception cref="ArgumentException">If the new child has already a parent.</exception>
        public int Add(UIElement element)
        {
            _owner.VerifyAccess();

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (((element._parent != null) ||   // Only visuals that are not connected to another tree
                (element.GetIsRootElement())))  // or a visual target can be added.
            {
                throw new System.ArgumentException("element has parent");
            }

            if ((_items == null) || (_size == _items.Length))
            {
                EnsureCapacity(_size + 1);
            }

            int addedPosition = _size++;
            Debug.Assert(_items[addedPosition] == null);

            ConnectChild(addedPosition, element);
            _version++;

            // invalidate measure on parent
            _owner.InvalidateMeasure();

            return addedPosition;
        }

        /// <summary>
        /// Returns the zero-based index of the UIElement. If the UIElement is not
        /// in the UIElementCollection -1 is returned. If null is passed to the method, the index
        /// of the first entry with null is returned. If there is no null entry -1 is returned.
        /// </summary>
        /// <param name="UIElement">The UIElement to locate in the UIElementCollection.</param>
        public int IndexOf(UIElement element)
        {
            if (element == null || element._parent == _owner)
            {
                for (int i = 0; i < _size; i++)
                {
                    if (_items[i] == element)
                    {
                        return i;
                    }
                }

                // not found, return -1
                return -1;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Removes the specified element from the UIElementCollection.
        /// </summary>
        /// <param name="element">The UIElement to remove from the UIElementCollection.</param>
        /// <remarks>
        /// The UIElements that follow the removed UIElements move up to occupy
        /// the vacated spot. The indexes of the UIElements that are moved are
        /// also updated.
        ///
        /// If element is null then the first null entry is removed. Note that removing
        /// a null entry is linear in the size of the collection.
        /// </remarks>
        public void Remove(UIElement element)
        {
            int indexToRemove = -1;

            _owner.VerifyAccess();

            if (element != null)
            {
                if (element._parent != _owner)
                {
                    // If the UIElement is not in this collection we silently return without
                    // failing. This is the same behavior that ArrayList implements.
                    return;
                }

                Debug.Assert(element._parent != null);

                DisconnectChild(indexToRemove = IndexOf(element));
            }
            else
            {
                // This is the case where element == null. We then remove the first null
                // entry.
                for (int i = 0; i < _size; i++)
                {
                    if (_items[i] == null)
                    {
                        indexToRemove = i;
                        break;
                    }
                }
            }

            if (indexToRemove != -1)
            {
                --_size;

                for (int i = indexToRemove; i < _size; i++)
                {
                    UIElement child = _items[i + 1];
                    _items[i] = child;
                }

                _items[_size] = null;
            }

            _owner.InvalidateMeasure();
        }

        /// <summary>
        /// Determines whether a element is in the UIElementCollection.
        /// </summary>
        public bool Contains(UIElement element)
        {
            if (element == null)
            {
                for (int i = 0; i < _size; i++)
                {
                    if (_items[i] == null)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return (element._parent == _owner);
            }
        }

        /// <summary>
        /// Removes all elements from the UIElementCollection.
        /// </summary>
        /// <remarks>
        /// Count is set to zero. Capacity remains unchanged.
        /// To reset the capacity of the UIElementCollection, call TrimToSize
        /// or set the Capacity property directly.
        /// </remarks>
        public void Clear()
        {
            _owner.VerifyAccess();

            for (int i = 0; i < _size; i++)
            {
                if (_items[i] != null)
                {
                    Debug.Assert(_items[i]._parent == _owner);
                    DisconnectChild(i);
                }

                _items[i] = null;
            }

            _size = 0;
            _version++;

            _owner.InvalidateMeasure();
        }

        /// <summary>
        /// Inserts an element into the UIElementCollection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="element">The UIElement to insert. </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// index is less than zero.
        ///
        /// -or-
        ///
        /// index is greater than Count.
        /// </exception>
        /// <remarks>
        /// If Count already equals Capacity, the capacity of the
        /// UIElementCollection is increased before the new UIElement
        /// is inserted.
        ///
        /// If index is equal to Count, value is added to the
        /// end of UIElementCollection.
        ///
        /// The UIElements that follow the insertion point move down to
        /// accommodate the new UIElement. The indexes of the UIElements that are
        /// moved are also updated.
        /// </remarks>
        public void Insert(int index, UIElement element)
        {
            _owner.VerifyAccess();

            if (index < 0 || index > _size)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (((element._parent != null) ||   // Only elements that are not connected to another tree
                (element.GetIsRootElement())))  // or a element target can be added.
            {
                throw new System.ArgumentException("element has parent");
            }

            if ((_items == null) || (_size == _items.Length))
            {
                EnsureCapacity(_size + 1);
            }

            for (int i = _size - 1; i >= index; i--)
            {
                UIElement child = _items[i];
                _items[i + 1] = child;
            }

            _items[index] = null;

            _size++;
            ConnectChild(index, element);
            // Note SetUIElement that increments the version to ensure proper enumerator
            // functionality.
            _owner.InvalidateMeasure();
        }

        /// <summary>
        /// Removes the UIElement at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than zero
        /// - or - index is equal or greater than count.</exception>
        /// <remarks>
        /// The UIElements that follow the removed UIElements move up to occupy
        /// the vacated spot. The indexes of the UIElements that are moved are
        /// also updated.
        /// </remarks>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _size)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            Remove(_items[index]);
        }

        /// <summary>
        /// Removes a range of UIElements from the UIElementCollection.
        /// </summary>
        /// <param name="index">The zero-based index of the range
        /// of elements to remove</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// index is less than zero.
        /// -or-
        /// count is less than zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// index and count do not denote a valid range of elements in the UIElementCollection.
        /// </exception>
        /// <remarks>
        /// The UIElements that follow the removed UIElements move up to occupy
        /// the vacated spot. The indexes of the UIElements that are moved are
        /// also updated.
        /// </remarks>
        public void RemoveRange(int index, int count)
        {
            _owner.VerifyAccess();

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (_size - index < count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (count > 0)
            {
                for (int i = index; i < index + count; i++)
                {
                    if (_items[i] != null)
                    {
                        DisconnectChild(i);
                        _items[i] = null;
                    }
                }

                _size -= count;
                for (int i = index; i < _size; i++)
                {
                    UIElement child = _items[i + count];
                    _items[i] = child;
                    _items[i + count] = null;
                }

                _version++; // Incrementing version number here to be consistent with the ArrayList
                // implementation.
            }
        }

        // ----------------------------------------------------------------
        // IEnumerable Interface
        // ----------------------------------------------------------------

        /// <summary>
        /// Returns an enumerator that can iterate through the UIElementCollection.
        /// </summary>
        /// <returns>Enumerator that enumerates the UIElementCollection in order.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// This is a simple UIElementCollection enumerator that is based on
        /// the ArrayListEnumeratorSimple that is used for ArrayLists.
        ///
        /// The following comment is from the CLR people:
        ///   For a straightforward enumeration of the entire ArrayList,
        ///   this is faster, because it's smaller.  Benchmarks showed
        ///   this.
        /// </summary>
        public struct Enumerator : IEnumerator, ICloneable
        {
            private UIElementCollection _collection;
            private int _index; // -1 means not started. -2 means that it reached the end.
            private int _version;
            private UIElement _currentElement;

            internal Enumerator(UIElementCollection collection)
            {
                _collection = collection;
                _index = -1; // not started.
                _version = _collection._version;
                _currentElement = null;
            }

            /// <summary>
            /// Creates a new object that is a copy of the current instance.
            /// </summary>
            public Object Clone()
            {
                return MemberwiseClone();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            public bool MoveNext()
            {
                if (_version == _collection._version)
                {
                    if ((_index > -2) && (_index < (_collection._size - 1)))
                    {
                        _index++;
                        _currentElement = _collection[_index];
                        return true;
                    }
                    else
                    {
                        _currentElement = null;
                        _index = -2; // -2 <=> reached the end.
                        return false;
                    }
                }
                else
                {
                    throw new InvalidOperationException("collection changed");
                }
            }

            /// <summary>
            /// Gets the current UIElement.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            /// <summary>
            /// Gets the current UIElement.
            /// </summary>
            public UIElement Current
            {
                get
                {
                    // Disable PREsharp warning about throwing exceptions in property
                    // get methods; see Windows OS Bugs #1035349 for an explanation.

                    if (_index < 0)
                    {
                        if (_index == -1)
                        {
                            // Not started.
                            throw new InvalidOperationException("not started");
                        }
                        else
                        {
                            // Reached the end.
                            Debug.Assert(_index == -2);
                            throw new InvalidOperationException("reached end");
                        }
                    }

                    return _currentElement;
                }
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                if (_version != _collection._version)
                    throw new InvalidOperationException("collection changed");
                _index = -1; // not started.
            }
        }

        internal UIElement[] _items;
        internal int _size;
        private int _version;
        private UIElement _owner;

        private const int c_defaultCapacity = 2;
        private const int c_growFactor = 2;

    }
}


