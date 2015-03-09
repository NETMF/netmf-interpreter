////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation
{

    internal class TimeoutEventArgs
    {
        FragmentationMessage _message;

        internal FragmentationMessage Message { get { return _message; } }

        internal TimeoutEventArgs(FragmentationMessage message)
        {
            _message = message;
        }
    }

    delegate void TimeoutEventHandler(Object sender, TimeoutEventArgs args);

    internal class FragmentationMessageTimer : IDisposable
    {
        private class TimerItem
        {
            static uint _nextId = 0;
            static uint GetNextId() { return _nextId++; }

            internal long TimeoutTicks;
            internal uint Id;
            internal FragmentationMessage Message;

            internal TimerItem(long timeoutTicks, FragmentationMessage message)
            {
                Id = GetNextId();
                TimeoutTicks = timeoutTicks;
                Message = message;
            }
        }

        ArrayList _timerItems;
        Object _lock;
        Timer _timer;
        int _maxItems;

        public virtual void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                }
            }
        }

        internal FragmentationMessageTimer(int maxItems)
        {
            _timerItems = new ArrayList();
            _lock = new object();
            _maxItems = maxItems;
            // create timer. initially, not active.
            _timer = new Timer(new TimerCallback(HandleTimeout), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        internal uint RegisterItem(long timeoutTick, FragmentationMessage message)
        {
            lock (_lock)
            {
                TimerItem item = new TimerItem(timeoutTick, message);
                AddItemToList(item);
                return item.Id;
            }
        }

        internal void UnregisterItem(uint id)
        {
            lock (_lock)
            {
                for (int i = 0; i < _timerItems.Count; i++)
                {
                    TimerItem item = (TimerItem)_timerItems[i];
                    if (item.Id == id)
                    {

                        RemoveItemFromList(i);
                        return;
                    }
                }
            }
        }

        internal void UnregisterItem(FragmentationMessage message)
        {
            lock (_lock)
            {
                for (int i = 0; i < _timerItems.Count; i++)
                {
                    TimerItem item = (TimerItem)_timerItems[i];
                    if (item.Message == message)
                    {
                        RemoveItemFromList(i);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// note: needs to have acquired the lock
        /// </summary>
        /// <param name="index"></param>
        private void RemoveItemFromList(int index)
        {
            _timerItems.RemoveAt(index);
            if (_timerItems.Count == 0)
            {
                _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }
            else if (index == 0)
            {
                // removed the first item. Timer should be adjust to new item
                TimerItem item = (TimerItem)_timerItems[0];
                int dueTime = (int)((item.TimeoutTicks - DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond);
                _timer.Change(dueTime, 0);
            }

            return;
        }

        /// <summary>
        /// method handling timer timeout
        /// </summary>
        /// <param name="state"></param>
        private void HandleTimeout(Object state)
        {
            TimerItem item = null;
            bool foundItem = false;

            lock (_lock)
            {
                // check whether the first element has expired.
                if (_timerItems.Count == 0)
                {
#if PRINTALL
                    Trace.Print("Timeout in message timer: no item. Do nothing.");
#endif
                    // nothing to do
                    return;
                }

                long currentTicks = DateTime.Now.Ticks;
                item = (TimerItem)_timerItems[0];
                if (item.TimeoutTicks <= currentTicks)
                {
                    // timer expired
#if PRINTALL
                    Trace.Print("Timeout in message timer: item expired.");
#endif
                    _timerItems.RemoveAt(0);
                    if (_timerItems.Count == 0)
                    {
                        // since no more timer item, remove timer schedule
                        _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    }

                    foundItem = true;

                }
                else
                {
#if PRINTALL
                    Trace.Print("Timeout in message timer: item did not expire yet.");
#endif

                }

            }

            // need to release the lock before calling handler to avoid deadlock
            // since FragmentationMessageTimer methods are called by the active messages.
            if (foundItem)
            {
                item.Message.HandleTimeout(this, new TimeoutEventArgs(item.Message));
                // re-call the handler in case more than one item expired.
                HandleTimeout(null);
            }
        }

        /// <summary>
        /// note: need to have acquired the lock
        /// </summary>
        /// <param name="item"></param>
        private void AddItemToList(TimerItem item)
        {
            int index = 0;
            int itemsCount = _timerItems.Count;
            if (itemsCount >= _maxItems)
                throw new ArgumentException("Already reached max number of timer elements.");

            for (int i = 0; i < itemsCount; i++)
            {
                TimerItem currentItem = (TimerItem)_timerItems[i];
                if (currentItem.TimeoutTicks > item.TimeoutTicks)
                {
                    index = i;
                    break;
                }
            }

            _timerItems.Insert(index, item);
            if (index == 0)
            {
                int dueTime = (int)((item.TimeoutTicks - DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond);
                if (dueTime < 0) { dueTime = 0; }
                _timer.Change(dueTime, System.Threading.Timeout.Infinite);
            }
        }

    }
}


