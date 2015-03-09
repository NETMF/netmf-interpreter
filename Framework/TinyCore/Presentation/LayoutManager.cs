////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation
{
    // LayoutManager is responsible for all layout operations. It maintains the Arrange,
    // Invalidate and Measure queues.
    //
    internal class LayoutManager : DispatcherObject
    {
        public class LayoutQueue
        {
            public LayoutQueue(LayoutManager layoutManager)
            {
                _layoutManager = layoutManager;
                _elements      = new ArrayList();
            }

            public bool IsEmpty
            {
                get
                {
                    return _elements.Count == 0;
                }
            }

            public void Add(UIElement e)
            {
                if (!_elements.Contains(e))
                {
                    RemoveOrphans(e);
                    _elements.Add(e);
                }

                _layoutManager.NeedsRecalc();
            }

            public UIElement GetTopMost()
            {
                UIElement found = null;
                int treeLevel = int.MaxValue;

                int count = _elements.Count;
                for (int index = 0; index < count; index++)
                {
                    UIElement currentElement = (UIElement)_elements[index];
                    UIElement parent = currentElement._parent;

                    int cnt = 0;
                    while (parent != null && cnt < treeLevel)
                    {
                        cnt++;
                        parent = parent._parent;
                    }

                    if (cnt < treeLevel)
                    {
                        treeLevel = cnt;
                        found = currentElement;
                    }
                }

                return found;
            }

            public void Remove(UIElement e)
            {
                _elements.Remove(e);
            }

            public void RemoveOrphans(UIElement parent)
            {
                int count = _elements.Count;
                for (int index = count - 1; index >= 0; index--)
                {
                    UIElement child = (UIElement)_elements[index];
                    if (child._parent == parent)
                    {
                        _elements.RemoveAt(index);
                    }
                }
            }

            private LayoutManager _layoutManager;

            private ArrayList _elements;
        }

        private class SingletonLock
        {
        }

        private LayoutManager()
        {
            // This constructor exists to prevent instantiation of a LayoutManager by any
            // means other than through LayoutManager.CurrentLayoutManager.
            _updateLayoutBackground = new DispatcherOperationCallback(UpdateLayoutBackground);
            _updateCallback = new DispatcherOperationCallback(UpdateLayoutCallback);
        }

        // posts a layout update
        private void NeedsRecalc()
        {
            if (!_layoutRequestPosted && !_isUpdating)
            {
                _layoutRequestPosted = true;
                MediaContext.From(Dispatcher).BeginInvokeOnRender(_updateCallback, this);
            }
        }

        private object UpdateLayoutBackground(object arg)
        {
            this.NeedsRecalc();
            return null;
        }

        private object UpdateLayoutCallback(object arg)
        {
            this.UpdateLayout();
            return null;
        }

        public LayoutQueue ArrangeQueue
        {
            get
            {
                if (_arrangeQueue == null)
                {
                    lock (typeof(SingletonLock))
                    {
                        if (_arrangeQueue == null)
                        {
                            _arrangeQueue = new LayoutQueue(this);
                        }
                    }
                }

                return _arrangeQueue;
            }
        }

        // Returns the LayoutManager singleton
        //
        public static LayoutManager CurrentLayoutManager
        {
            get
            {
                return LayoutManager.From(Dispatcher.CurrentDispatcher);
            }
        }

        public static LayoutManager From(Dispatcher dispatcher)
        {
            if (dispatcher == null) throw new ArgumentException();

            if (dispatcher._layoutManager == null)
            {
                lock (typeof(SingletonLock))
                {
                    if (dispatcher._layoutManager == null)
                    {
                        dispatcher._layoutManager = new LayoutManager();
                    }
                }
            }

            return dispatcher._layoutManager;
        }

        public LayoutQueue MeasureQueue
        {
            get
            {
                if (_measureQueue == null)
                {
                    lock (typeof(SingletonLock))
                    {
                        if (_measureQueue == null)
                        {
                            _measureQueue = new LayoutQueue(this);
                        }
                    }
                }

                return _measureQueue;
            }
        }

        public void UpdateLayout()
        {
            VerifyAccess();

            //make UpdateLayout to be a NOP if called during UpdateLayout.
            if (_isUpdating) return;

            _isUpdating = true;

            WindowManager.Instance.Invalidate();

            LayoutQueue measureQueue = MeasureQueue;
            LayoutQueue arrangeQueue = ArrangeQueue;

            int cnt = 0;
            bool gotException = true;
            UIElement currentElement = null;

            //NOTE:
            //
            //There are a bunch of checks here that break out of and re-queue layout if
            //it looks like things are taking too long or we have somehow gotten into an
            //infinite loop.   In the TinyCLR we will probably have better ways of
            //dealing with a bad app through app domain separation, but keeping this
            //robustness can't hurt.  In a single app domain scenario, it could
            //give the opportunity to get out to the system if something is misbehaving,
            //we like this kind of reliability in embedded systems.
            //

            try
            {
                invalidateTreeIfRecovering();

                while ((!MeasureQueue.IsEmpty) || (!ArrangeQueue.IsEmpty))
                {
                    if (++cnt > 153)
                    {
                        //loop detected. Lets re-queue and let input/user to correct the situation.
                        //
                        Dispatcher.BeginInvoke(_updateLayoutBackground, this);
                        currentElement = null;
                        gotException = false;
                        return;
                    }

                    //loop for Measure
                    //We limit the number of loops here by time - normally, all layout
                    //calculations should be done by this time, this limit is here for
                    //emergency, "infinite loop" scenarios - yielding in this case will
                    //provide user with ability to continue to interact with the app, even though
                    //it will be sluggish. If we don't yield here, the loop is goign to be a deadly one

                    int loopCounter = 0;
                    TimeSpan loopStartTime = TimeSpan.Zero;

                    while (true)
                    {
                        if (++loopCounter > 153)
                        {
                            loopCounter = 0;
                            if (LimitExecution(ref loopStartTime))
                            {
                                currentElement = null;
                                gotException = false;
                                return;
                            }
                        }

                        currentElement = measureQueue.GetTopMost();

                        if (currentElement == null) break; //exit if no more Measure candidates

                        currentElement.Measure(
                            currentElement._previousAvailableWidth,
                            currentElement._previousAvailableHeight
                            );

                        measureQueue.RemoveOrphans(currentElement);
                    }

                    //loop for Arrange
                    //if Arrange dirtied the tree go clean it again

                    //We limit the number of loops here by time - normally, all layout
                    //calculations should be done by this time, this limit is here for
                    //emergency, "infinite loop" scenarios - yielding in this case will
                    //provide user with ability to continue to interact with the app, even though
                    //it will be sluggish. If we don't yield here, the loop is goign to be a deadly one
                    loopCounter = 0;
                    loopStartTime = TimeSpan.Zero;

                    while (true)
                    {
                        if (++loopCounter > 153)
                        {
                            loopCounter = 0;
                            if (LimitExecution(ref loopStartTime))
                            {
                                currentElement = null;
                                gotException = false;
                                return;
                            }
                        }

                        currentElement = arrangeQueue.GetTopMost();

                        if (currentElement == null) break; //exit if no more Arrange candidates

                        int arrangeX, arrangeY, arrangeWidth, arrangeHeight;

                        getProperArrangeRect(currentElement, out arrangeX, out arrangeY, out arrangeWidth, out arrangeHeight);

#if TINYCLR_DEBUG_LAYOUT
                        Trace.Print("arrangeWidth = " + arrangeWidth);
                        Trace.Print("arrangeHeight = " + arrangeWidth);
#endif

                        currentElement.Arrange(arrangeX, arrangeY, arrangeWidth, arrangeHeight);
                        arrangeQueue.RemoveOrphans(currentElement);
                    }

                    /* REFACTOR -- do we need Layout events and Size changed events?

                                        //let LayoutUpdated handlers to call UpdateLayout
                                        //note that it means we can get reentrancy into UpdateLayout past this point,
                                        //if any of event handlers call UpdateLayout sync. Need to protect from reentrancy
                                        //in the firing methods below.

                                        fireSizeChangedEvents();
                                        if ((!MeasureQueue.IsEmpty) || (!ArrangeQueue.IsEmpty)) continue;
                                        fireLayoutUpdateEvent();
                    */
                }

                currentElement = null;
                gotException = false;
            }
            finally
            {
                _isUpdating = false;
                _layoutRequestPosted = false;

                if (gotException)
                {
                    //set indicator
                    _gotException = true;
                    _forceLayoutElement = currentElement;

                    //make attempt to request the subsequent layout calc
                    Dispatcher.BeginInvoke(_updateLayoutBackground, this);
                }
            }
        }

        //
        // ensures we don't spend all day doing layout, and
        // give the system the chance to do something else.
        private bool LimitExecution(ref TimeSpan loopStartTime)
        {
            if (loopStartTime.Ticks == 0)
            {
                loopStartTime = Microsoft.SPOT.Hardware.Utility.GetMachineTime();
            }
            else
            {
                if ((Microsoft.SPOT.Hardware.Utility.GetMachineTime() - loopStartTime).Ticks > 153 * 2 * TimeSpan.TicksPerMillisecond) // 153*2 = magic*science
                {
                    //loop detected. Lets go over to background to let input work.
                    Dispatcher.BeginInvoke(_updateLayoutBackground, this);
                    return true;
                }
            }

            return false;
        }

        private void getProperArrangeRect(UIElement element, out int x, out int y, out int width, out int height)
        {
            x = element._finalX;
            y = element._finalY;
            width = element._finalWidth;
            height = element._finalHeight;

            // ELements without a parent (top level) get Arrange at DesiredSize
            // if they were measured "to content" (as Constants.MaxExtent indicates).
            // If we arrange the element that is temporarily disconnected
            // so it is not a top-level one, the assumption is that it will be
            // layout-invalidated and/or recomputed by the parent when reconnected.
            if (element.Parent == null)
            {
                int desiredWidth, desiredHeight;
                x = y = 0;
                element.GetDesiredSize(out desiredWidth, out desiredHeight);

                if (element._previousAvailableWidth == Media.Constants.MaxExtent)
                    width = desiredWidth;

                if (element._previousAvailableHeight == Media.Constants.MaxExtent)
                    height = desiredHeight;
            }
        }

        private void invalidateTreeIfRecovering()
        {
            if ((_forceLayoutElement != null) || _gotException)
            {
                if (_forceLayoutElement != null)
                {
                    UIElement e = _forceLayoutElement.RootUIElement;

                    markTreeDirtyHelper(e);
                    MeasureQueue.Add(e);
                }

                _forceLayoutElement = null;
                _gotException = false;
            }
        }

        private void markTreeDirtyHelper(UIElement e)
        {
            //now walk down and mark all UIElements dirty
            if (e != null)
            {
                e._flags |= (UIElement.Flags.InvalidMeasure | UIElement.Flags.InvalidArrange);

                UIElementCollection uiec = e._logicalChildren;

                if (uiec != null)
                {
                    for (int i = uiec.Count; i-- > 0; )
                    {
                        markTreeDirtyHelper(uiec[i]);
                    }
                }
            }
        }

        private bool _isUpdating;
        private bool _gotException; //true if UpdateLayout exited with exception
        private bool _layoutRequestPosted;

        private UIElement _forceLayoutElement; //set in extreme situations, forces the update of the whole tree containing the element

        // measure & arrange queues.
        private LayoutQueue _arrangeQueue;
        private LayoutQueue _measureQueue;

        private DispatcherOperationCallback _updateLayoutBackground;
        private DispatcherOperationCallback _updateCallback;

    }

}


