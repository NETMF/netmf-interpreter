////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Collections;

namespace Microsoft.SPOT.Emulator
{
    public abstract class EmulatorComponentCollection : EmulatorComponent, ICollection 
    {
        CollectionChangeEventHandler _evtChanged;
        Type _collectionType;

        protected EmulatorComponentCollection( Type collectionType )
        {
            _collectionType = collectionType;
        }

        public Type CollectionType
        {
            get { return _collectionType; }
        }

        public event CollectionChangeEventHandler CollectionChanged
        {
            add { _evtChanged += value; }
            remove { _evtChanged -= value; }
        }

        [System.Obsolete("Use Emulator.RegisterComponent() Instead.")]
        public void Register(EmulatorComponent ec)
        {
            this.Emulator.RegisterComponent(ec);
        }

        [System.Obsolete("Use Emulator.UnregisterComponent() Instead.")]
        public void Unregister(EmulatorComponent ec)
        {
            this.Emulator.UnregisterComponent(ec);
        }

        internal virtual void RegisterInternal(EmulatorComponent ec)
        {
            this.Emulator.RegisterComponent(ec);

            CollectionChangeEventHandler evt = _evtChanged;
            CollectionChangeEventArgs args = new CollectionChangeEventArgs(CollectionChangeAction.Add, ec);

            if (evt != null)
            {
                evt(this, args);
            }
        }

        internal virtual void UnregisterInternal(EmulatorComponent ec)
        {
            CollectionChangeEventHandler evt = _evtChanged;
            CollectionChangeEventArgs args = new CollectionChangeEventArgs(CollectionChangeAction.Remove, ec);

            if (evt != null)
            {
                evt(this, args);
            }

            this.Emulator.UnregisterComponent(ec);
        }

        public override bool IsReplaceableBy( EmulatorComponent ec )
        {
            if (ec is EmulatorComponentCollection)
            {
                return CollectionType == ((EmulatorComponentCollection)ec).CollectionType;
            }
            else
            {
                return false;
            }
        }

        #region ICollection Members

        public abstract void CopyTo(Array array, int index);

        public abstract int Count
        {
            get;
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region IEnumerable Members

        public abstract IEnumerator GetEnumerator();

        #endregion
    }
}