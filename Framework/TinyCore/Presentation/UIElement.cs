////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;

using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation
{
    public abstract class UIElement : DispatcherObject
    {
        protected UIElement()
        {
            EnsureClassHandlers();

            _flags = Flags.NeverMeasured | Flags.NeverArranged | Flags.Enabled;

            _visibility = Visibility.Visible;

            _verticalAlignment = VerticalAlignment.Stretch;
            _horizontalAlignment = HorizontalAlignment.Stretch;
        }

        #region Class Handlers

        private static void AddRoutedEventHandler(
            Hashtable table,
            RoutedEvent routedEvent,
            RoutedEventHandler handler,
            bool handledEventsToo)
        {
            if (routedEvent == null || handler == null)
            {
                throw new ArgumentNullException();
            }

            // Create a new RoutedEventHandler
            RoutedEventHandlerInfo routedEventHandlerInfo =
                new RoutedEventHandlerInfo(handler, handledEventsToo);

            // Get the entry corresponding to the given RoutedEvent
            ArrayList handlers = (ArrayList)table[routedEvent];
            if (handlers == null)
            {
                table[routedEvent] = (handlers = new ArrayList());
            }

            // Add the RoutedEventHandlerInfo to the list
            handlers.Add(routedEventHandlerInfo);
        }

        private static void EnsureClassHandlers()
        {
            // since we can't rely on the order of static constructors,
            // we will do this the first time an element is created.
            // we have a single dispatcher, so we don't have to worry about
            // race conditions here..

            if (_classEventHandlersStore == null)
            {
                _classEventHandlersStore = new Hashtable();

                // buttons
                AddRoutedEventHandler(_classEventHandlersStore, Buttons.PreviewButtonDownEvent, new RoutedEventHandler(UIElement.OnPreviewButtonDownThunk), false);
                AddRoutedEventHandler(_classEventHandlersStore, Buttons.ButtonDownEvent       , new RoutedEventHandler(UIElement.OnButtonDownThunk)       , false);
                AddRoutedEventHandler(_classEventHandlersStore, Buttons.PreviewButtonUpEvent  , new RoutedEventHandler(UIElement.OnPreviewButtonUpThunk)  , false);
                AddRoutedEventHandler(_classEventHandlersStore, Buttons.ButtonUpEvent         , new RoutedEventHandler(UIElement.OnButtonUpThunk)         , false);

                // focus
                AddRoutedEventHandler(_classEventHandlersStore, Buttons.GotFocusEvent         , new RoutedEventHandler(UIElement.OnGotFocusThunk)          , true);
                AddRoutedEventHandler(_classEventHandlersStore, Buttons.LostFocusEvent        , new RoutedEventHandler(UIElement.OnLostFocusThunk)         , true);

                AddRoutedEventHandler(_classEventHandlersStore, TouchEvents.TouchDownEvent    , new RoutedEventHandler(UIElement.OnTouchDownThunk)         , false);
                AddRoutedEventHandler(_classEventHandlersStore, TouchEvents.TouchUpEvent      , new RoutedEventHandler(UIElement.OnTouchUpThunk)           , false);
                AddRoutedEventHandler(_classEventHandlersStore, TouchEvents.TouchMoveEvent    , new RoutedEventHandler(UIElement.OnTouchMoveThunk)         , false);

                AddRoutedEventHandler(_classEventHandlersStore, GenericEvents.GenericStandardEvent, new RoutedEventHandler(UIElement.OnGenericEventThunk)  , true);

            }
        }

        //
        // We have this thunking layer because the delegates for class handlers are static,
        // but we need to call instance methods.  An alternative would be to either not have class handlers (inconsistent),
        // or to use per-instance storage for every class handler (wasteful).  So we will trade off some code size here
        // to get class handlers.

        private static void OnPreviewButtonDownThunk(object sender, RoutedEventArgs e) { ((UIElement)sender).OnPreviewButtonDown((ButtonEventArgs)e); }

        private static void OnButtonDownThunk(object sender, RoutedEventArgs e) { ((UIElement)sender).OnButtonDown((ButtonEventArgs)e); }

        private static void OnPreviewButtonUpThunk(object sender, RoutedEventArgs e) { ((UIElement)sender).OnPreviewButtonUp((ButtonEventArgs)e); }

        private static void OnButtonUpThunk(object sender, RoutedEventArgs e) { ((UIElement)sender).OnButtonUp((ButtonEventArgs)e); }

        private static void OnGotFocusThunk(object sender, RoutedEventArgs e) { ((UIElement)sender).OnGotFocus((FocusChangedEventArgs)e); }

        private static void OnLostFocusThunk(object sender, RoutedEventArgs e) { ((UIElement)sender).OnLostFocus((FocusChangedEventArgs)e); }

        private static void OnGenericEventThunk(object sender, RoutedEventArgs e) { ((UIElement)sender).OnGenericEvent((GenericEventArgs)e); }

        protected virtual void OnGenericEvent(GenericEventArgs e)
        {
            GenericEvent genericEvent = e.InternalEvent;
            switch (genericEvent.EventCategory)
            {
                case (byte)EventCategory.Gesture:
                    {
                        TouchGestureEventArgs ge = new TouchGestureEventArgs();

                        ge.Gesture = (TouchGesture)genericEvent.EventMessage;
                        ge.X = genericEvent.X;
                        ge.Y = genericEvent.Y;
                        ge.Arguments = (ushort)genericEvent.EventData;

                        if (ge.Gesture == TouchGesture.Begin)
                        {
                            OnTouchGestureStarted(ge);
                        }
                        else if (ge.Gesture == TouchGesture.End)
                        {
                            OnTouchGestureEnded(ge);
                        }
                        else
                        {
                            OnTouchGestureChanged(ge);
                        }

                        break;
                    }
                default:
                    {
                        break;
                    }
            }

        }

        private static void OnTouchDownThunk(object sender, RoutedEventArgs e)
        {
            ((UIElement)sender).OnTouchDown((TouchEventArgs)e);
        }

        private static void OnTouchUpThunk(object sender, RoutedEventArgs e)
        {
            ((UIElement)sender).OnTouchUp((TouchEventArgs)e);
        }

        private static void OnTouchMoveThunk(object sender, RoutedEventArgs e)
        {
            ((UIElement)sender).OnTouchMove((TouchEventArgs)e);
        }

        protected virtual void OnTouchDown(TouchEventArgs e)
        {
            if (TouchDown != null)
            {
                TouchDown(this, e);
            }
        }

        protected virtual void OnTouchUp(TouchEventArgs e)
        {
            if (TouchUp != null)
            {
                TouchUp(this, e);
            }
        }

        protected virtual void OnTouchMove(TouchEventArgs e)
        {
            if (TouchMove != null)
            {
                TouchMove(this, e);
            }
        }

        protected virtual void OnTouchGestureStarted(TouchGestureEventArgs e)
        {
            if (TouchGestureStart != null)
            {
                TouchGestureStart(this, e);
            }
        }

        protected virtual void OnTouchGestureChanged(TouchGestureEventArgs e)
        {
            if (TouchGestureChanged != null)
            {
                TouchGestureChanged(this, e);
            }
        }

        protected virtual void OnTouchGestureEnded(TouchGestureEventArgs e)
        {
            if (TouchGestureEnd != null)
            {
                TouchGestureEnd(this, e);
            }
        }

        /// <summary>
        ///     An event reporting a button was pressed.
        /// </summary>
        protected virtual void OnPreviewButtonDown(ButtonEventArgs e) { }

        /// <summary>
        ///     An event reporting a button was pressed.
        /// </summary>
        protected virtual void OnButtonDown(ButtonEventArgs e) { }

        /// <summary>
        ///     An event reporting a button was released.
        /// </summary>
        protected virtual void OnPreviewButtonUp(ButtonEventArgs e) { }

        /// <summary>
        ///     An event reporting a button was released.
        /// </summary>
        protected virtual void OnButtonUp(ButtonEventArgs e) { }

        /// <summary>
        ///     An event announcing that the buttons are focused on this element.
        /// </summary>
        protected virtual void OnGotFocus(FocusChangedEventArgs e) { }

        /// <summary>
        ///     An event announcing that the buttons is no longer focused on this element
        /// </summary>
        protected virtual void OnLostFocus(FocusChangedEventArgs e) { }

        #endregion Class Handlers

        public event TouchEventHandler TouchDown;
        public event TouchEventHandler TouchUp;
        public event TouchEventHandler TouchMove;

        public event TouchGestureEventHandler TouchGestureStart;
        public event TouchGestureEventHandler TouchGestureChanged;
        public event TouchGestureEventHandler TouchGestureEnd;

        public void GetDesiredSize(out int width, out int height)
        {
            if (this.Visibility == Visibility.Collapsed)
            {
                width = 0;
                height = 0;
            }
            else
            {
                width = _desiredWidth;
                height = _desiredHeight;
            }
        }

        public void GetMargin(out int left, out int top, out int right, out int bottom)
        {
            left = _marginLeft;
            top = _marginTop;
            right = _marginRight;
            bottom = _marginBottom;
        }

        public void SetMargin(int length)
        {
            VerifyAccess();

            SetMargin(length, length, length, length);
        }

        public void SetMargin(int left, int top, int right, int bottom)
        {
            VerifyAccess();

            _marginLeft = left;
            _marginTop = top;
            _marginRight = right;
            _marginBottom = bottom;
            InvalidateMeasure();
        }

        public int ActualWidth
        {
            get
            {
                return _renderWidth;
            }
        }

        public int ActualHeight
        {
            get
            {
                return _renderHeight;
            }
        }

        public int Height
        {
            get
            {
                int height;
                if (IsHeightSet(out height))
                {
                    return height;
                }
                else
                {
                    throw new InvalidOperationException("height not set");
                }
            }

            set
            {
                VerifyAccess();
                
                if(value < 0) throw new ArgumentException();

                if (_requestedSize == null)
                {
                    _requestedSize = new Pair();
                }

                _requestedSize._second = value;
                _requestedSize._status |= Pair.Flags_Second;
                InvalidateMeasure();
            }
        }

        public int Width
        {
            get
            {
                int width;
                if (IsWidthSet(out width))
                {
                    return width;
                }
                else
                {
                    throw new InvalidOperationException("width not set");
                }
            }

            set
            {
                VerifyAccess();

                if(value < 0) throw new ArgumentException();

                if (_requestedSize == null) 
                {
                    _requestedSize = new Pair();
                }

                _requestedSize._first = value;
                _requestedSize._status |= Pair.Flags_First;
                InvalidateMeasure();
            }
        }

        private bool IsHeightSet(out int height)
        {
            Pair size = _requestedSize;
            if (size != null && (size._status & Pair.Flags_Second) != 0)
            {
                height = size._second;
                return true;
            }

            height = 0;
            return false;
        }

        private bool IsWidthSet(out int width)
        {
            Pair size = _requestedSize;
            if (size != null && (size._status & Pair.Flags_First) != 0)
            {
                width = size._first;
                return true;
            }

            width = 0;
            return false;
        }

        public void GetLayoutOffset(out int x, out int y)
        {
            x = this._offsetX;
            y = this._offsetY;
        }

        public void GetRenderSize(out int width, out int height)
        {
            width = this._renderWidth;
            height = this._renderHeight;
        }

        protected UIElementCollection LogicalChildren
        {
            get
            {
                VerifyAccess();

                if (_logicalChildren == null)
                {
                    _logicalChildren = new UIElementCollection(this);
                }

                return _logicalChildren;
            }
        }

        /// <summary>
        /// OnChildrenChanged is called when the UIElementCollection of the UIElement is edited.
        /// </summary>
        protected internal virtual void OnChildrenChanged(
            UIElement added,
            UIElement removed,
            int indexAffected)
        {
            //Child visibility can't change if parent isn't visible
            if ((this._flags & Flags.IsVisibleCache) != 0)
            {
                if (removed != null && removed._visibility == Visibility.Visible)
                {
                    removed._flags &= ~Flags.IsVisibleCache;
                    removed.OnIsVisibleChanged(true);
                }

                if (added != null && added._visibility == Visibility.Visible)
                {
                    added._flags |= Flags.IsVisibleCache;
                    added.OnIsVisibleChanged(false);
                }
            }
        }

        /// <summary>
        ///     A property indicating if the button is focused on this
        ///     element or not.
        /// </summary>
        public bool IsFocused
        {
            get
            {
                // avalon also has a dependencyproperty for this I think, but I'm not sure we need it.
                return (Buttons.FocusedElement == this);
            }
        }

        private void ComputeAlignmentOffset(int clientWidth, int clientHeight,
                                            int arrangeWidth, int arrangeHeight,
                                            out int dx, out int dy)
        {
            HorizontalAlignment ha = HorizontalAlignment;
            VerticalAlignment va = VerticalAlignment;

            //this is to degenerate Stretch to Top-Left in case when clipping is about to occur
            //if we need it to be Center instead, simply remove these 2 ifs
            if (ha == HorizontalAlignment.Stretch
                && arrangeWidth > clientWidth)
            {
                ha = HorizontalAlignment.Left;
            }

            if (va == VerticalAlignment.Stretch
                && arrangeHeight > clientHeight)
            {
                va = VerticalAlignment.Top;
            }

            //end of degeneration of Stretch to Top-Left

            if (ha == HorizontalAlignment.Center
                || ha == HorizontalAlignment.Stretch)
            {
                dx = (clientWidth - arrangeWidth) / 2;
            }
            else if (ha == HorizontalAlignment.Right)
            {
                dx = clientWidth - arrangeWidth;
            }
            else
            {
                dx = 0;
            }

            if (va == VerticalAlignment.Center
                || va == VerticalAlignment.Stretch)
            {
                dy = (clientHeight - arrangeHeight) / 2;
            }
            else if (va == VerticalAlignment.Bottom)
            {
                dy = clientHeight - arrangeHeight;
            }
            else
            {
                dy = 0;
            }
        }

        /// <summary>
        /// Recursively propagates IsLayoutSuspended flag down to the whole v's sub tree.
        /// </summary>
        internal static void PropagateSuspendLayout(UIElement v)
        {
            if ((v._flags & Flags.IsLayoutSuspended) == 0)
            {
                v._flags |= Flags.IsLayoutSuspended;

                UIElementCollection children = v._logicalChildren;
                if (children != null)
                {
                    int count = children.Count;
                    for (int i = 0; i < count; i++)
                    {
                        UIElement child = children[i];
                        if (child != null)
                        {
                            PropagateSuspendLayout(child);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursively resets IsLayoutSuspended flag on all visuals of the whole v's sub tree.
        /// For UIElements also re-inserts the UIElement into Measure and / or Arrange update queues
        /// if necessary.
        /// </summary>
        internal static void PropagateResumeLayout(UIElement e)
        {
            if ((e._flags & Flags.IsLayoutSuspended) != 0)
            {
                e._flags &= ~Flags.IsLayoutSuspended;

                Debug.Assert((e._flags & (Flags.MeasureInProgress | Flags.ArrangeInProgress)) == 0);

                bool requireMeasureUpdate = ((e._flags & Flags.InvalidMeasure) != 0) && ((e._flags & Flags.NeverMeasured) == 0);
                bool requireArrangeUpdate = ((e._flags & Flags.InvalidArrange) != 0) && ((e._flags & Flags.NeverArranged) == 0);
                LayoutManager layoutManager = (requireMeasureUpdate || requireArrangeUpdate) ? LayoutManager.From(e.Dispatcher) : null;
                if (requireMeasureUpdate)
                {
                    layoutManager.MeasureQueue.Add(e);
                }

                if (requireArrangeUpdate)
                {
                    layoutManager.ArrangeQueue.Add(e);
                }

                UIElementCollection children = e._logicalChildren;
                if (children != null)
                {
                    int count = children.Count;
                    for (int i = 0; i < count; i++)
                    {
                        UIElement child = children[i];
                        if (child != null)
                        {
                            PropagateResumeLayout(child);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates DesiredSize of the UIElement. Must be called by parents from their MeasureOverride, to form recursive update.
        /// This is first pass of layout update.
        /// </summary>
        /// <remarks>
        /// Measure is called by parents on their children. Internally, Measure calls MeasureOverride override on the same object,
        /// giving it opportunity to compute its DesiredSize.<para/>
        /// This method will return immediately if child is not Dirty, previously measured
        /// and availableSize is the same as cached. <para/>
        /// This method also resets the IsMeasureinvalid bit on the child.<para/>
        /// In case when "unbounded measure to content" is needed, parent can use availableSize
        /// as double.PositiveInfinity. Any returned size is OK in this case.
        /// </remarks>
        /// <param name="availableWidth">Available width that parent can give to the child. May be MaxValue (when parent wants to
        /// measure to content). This is soft constraint. Child can return bigger size to indicate that it wants bigger space and hope
        /// that parent can throw in scrolling...</param>
        /// <param name="availableHeight">Available height that parent can give to the child. May be MaxValue (when parent wants to
        /// measure to content). This is soft constraint. Child can return bigger size to indicate that it wants bigger space and hope
        /// that parent can throw in scrolling...</param>
        public void Measure(int availableWidth, int availableHeight)
        {
            VerifyAccess();

#if TINYCLR_DEBUG_LAYOUT
            Trace.Print(this.ToString() + ".Measure " + PrintSize(availableWidth, availableHeight));
#endif

            // cache here, since we read this a few times before writing.
            Flags flags = _flags;

            //if Collapsed, we should not Measure, keep dirty bit but remove request
            if (this.Visibility == Visibility.Collapsed || ((flags & Flags.IsLayoutSuspended) != 0))
            {
                //reset measure request.

                LayoutManager.CurrentLayoutManager.MeasureQueue.Remove(this);

                //  remember though that parent tried to measure at this size
                //  in case when later this element is called to measure incrementally
                //  it has up-to-date information stored in _previousAvailableSize
                _previousAvailableWidth = availableWidth;
                _previousAvailableHeight = availableHeight;

                return;
            }

#if TINYCLR_DEBUG_LAYOUT
            if (true)
            {
                bool neverMeasured = ((_flags & Flags.NeverMeasured) != 0);

                String traceMessage = this.GetType().FullName + ": ";

                if (_flags&Flags.InvalidMeasure == 0)
                {
                    traceMessage += "dirty ";
                }
                else
                {
                    traceMessage += "not dirty ";
                }

                if (neverMeasured == false)
                {
                    traceMessage += "measured never";
                }

            else
                {
                    traceMessage += "measured before ";
                }

                traceMessage += " old-h= " + Convert.ToString(_previousAvailableHeight);
                traceMessage += " old-w= " + Convert.ToString(_previousAvailableWidth);
                traceMessage += " cur-h= " + Convert.ToString(availableHeight);
                traceMessage += " cur-w= " + Convert.ToString(availableWidth);

                Trace.Print(traceMessage);
            }

#endif

            // Bypass if possible
            //
            if (((flags & Flags.InvalidMeasure) == 0) &&
                ((flags & Flags.NeverMeasured) == 0) &&
                availableWidth == _previousAvailableWidth &&
                availableHeight == _previousAvailableHeight)
            {
#if TINYCLR_DEBUG_LAYOUT
                String traceMessage = this.GetType().FullName + ": measure by passed";

                Trace.Print(traceMessage);
#endif
                return;
            }

            _flags &= ~Flags.NeverMeasured;

            int previousWidth = _desiredWidth;
            int previousHeight = _desiredHeight;

            //we always want to be arranged, ensure arrange request
            //doing it before OnMeasure prevents unneeded requests from children in the queue
            InvalidateArrange();

            // MeasureInProgress prevents OnChildDesiredSizeChange to cause the elements be put
            // into the queue.
            _flags |= Flags.MeasureInProgress;

            int desiredWidth;
            int desiredHeight;
            try
            {
                // Compute available size for call into MeasureOverride
                //
                int marginWidth = _marginLeft + _marginRight;
                int marginHeight = _marginTop + _marginBottom;
                int frameworkAvailableWidth = availableWidth - marginWidth;
                int frameworkAvailableHeight = availableHeight - marginHeight;
                if (_requestedSize != null)
                {
                    // Respect requested size
                    //
                    bool haveRequestedWidth = (_requestedSize._status & Pair.Flags_First) != 0;
                    bool haveRequestedHeight = (_requestedSize._status & Pair.Flags_Second) != 0;
                    if (haveRequestedWidth)
                    {
                        frameworkAvailableWidth = System.Math.Min(_requestedSize._first, frameworkAvailableWidth);
                    }

                    if (haveRequestedHeight)
                    {
                        frameworkAvailableHeight = System.Math.Min(_requestedSize._second, frameworkAvailableHeight);
                    }

                    MeasureOverride(frameworkAvailableWidth, frameworkAvailableHeight, out desiredWidth, out desiredHeight);

                    if (haveRequestedWidth)
                    {
                        desiredWidth = _requestedSize._first;
                    }

                    if (haveRequestedHeight)
                    {
                        desiredHeight = _requestedSize._second;
                    }
                }
                else
                {
                    // No requested size specified
                    MeasureOverride(frameworkAvailableWidth, frameworkAvailableHeight, out desiredWidth, out desiredHeight);
                }

                // Restrict the desired size to the available size
                _unclippedWidth = desiredWidth;
                _unclippedHeight = desiredHeight;
                desiredWidth = System.Math.Min(desiredWidth, frameworkAvailableWidth);
                desiredHeight = System.Math.Min(desiredHeight, frameworkAvailableHeight);

                // Add margins
                desiredWidth += marginWidth;
                desiredHeight += marginHeight;
            }
            finally
            {
                _flags &= ~Flags.MeasureInProgress;
                _previousAvailableWidth = availableWidth;
                _previousAvailableHeight = availableHeight;
            }

            _flags &= ~Flags.InvalidMeasure;

            LayoutManager.CurrentLayoutManager.MeasureQueue.Remove(this);

            _desiredWidth = desiredWidth;
            _desiredHeight = desiredHeight;

            //notify parent if our desired size changed (waterfall effect)
            if ((_flags & Flags.MeasureDuringArrange) == 0 && !(previousWidth == desiredWidth && previousHeight == desiredHeight))
            {
                UIElement parent = _parent;
                if (parent != null && (parent._flags & Flags.MeasureInProgress) == 0)
                {
                    parent.OnChildDesiredSizeChanged(this);
                }
            }

#if TINYCLR_DEBUG_LAYOUT
            if (true)
            {
                String traceMessage = this.GetType().FullName + ": measured";

                Trace.Print(traceMessage);
            }

#endif
        }

        /// <summary>
        /// Parents or system call this method to arrange the internals of children on a second pass of layout update.
        /// </summary>
        /// <remarks>
        /// This method internally calls ArrangeOverride override, giving the derived class opportunity
        /// to arrange its children and/or content using final computed size.
        /// In their ArrangeOverride overrides, derived class is supposed to create its visual structure and
        /// prepare itself for rendering. Arrange is called by parents
        /// from their implementation of ArrangeOverride or by system when needed.
        /// This method sets Bounds=finalSize before calling ArrangeOverride.
        /// </remarks>
        /// <param name="finalRectX">This is the final X location that parent or system wants this UIElement to assume.</param>
        /// <param name="finalRectY">This is the final Y location that parent or system wants this UIElement to assume.</param>
        /// <param name="finalRectWidth">This is the Width that parent or system wants this UIElement to assume.</param>
        /// <param name="finalRectHeight">This is the height that parent or system wants this UIElement to assume.</param>

        public void Arrange(int finalRectX, int finalRectY, int finalRectWidth, int finalRectHeight)
        {
            VerifyAccess();

            //in case parent did not call Measure on a child, we call it now.
            //parent can skip calling Measure on a child if it does not care about child's size
            //passing finalSize practically means "set size" because that's what Measure(sz)/Arrange(same_sz) means
            if ((_flags & (Flags.InvalidMeasure | Flags.NeverMeasured)) != 0)
            {
                try
                {
                    _flags |= Flags.MeasureDuringArrange;
                    Measure(finalRectWidth, finalRectHeight);
                }
                finally
                {
                    _flags &= ~Flags.MeasureDuringArrange;
                }
            }

            //if Collapsed, we should not Arrange, keep dirty bit but remove request
            //
            if (this.Visibility == Visibility.Collapsed
                || ((_flags & Flags.IsLayoutSuspended) != 0)

                )
            {
                //reset arrange request.
                LayoutManager.CurrentLayoutManager.ArrangeQueue.Remove(this);

                //  remember though that parent tried to arrange at this rect
                //  in case when later this element is called to arrange incrementally
                //  it has up-to-date information stored in _finalRect
                _finalX = finalRectX;
                _finalY = finalRectY;
                _finalWidth = finalRectWidth;
                _finalHeight = finalRectHeight;
                return;
            }

            //your basic bypass. No reason to calc the same thing.
            if (((_flags & Flags.InvalidArrange) == 0) &&
                ((_flags & Flags.NeverArranged) == 0) &&
                finalRectWidth == _finalWidth &&
                finalRectHeight == _finalHeight)
            {
                if (finalRectX != _finalX || finalRectY != _finalY)
                {
                    _offsetX = _offsetX - _finalX + finalRectX;
                    _offsetY = _offsetY - _finalY + finalRectY;

                    // Cache final position
                    _finalX = finalRectX;
                    _finalY = finalRectY;

                    if (IsRenderable())
                    {
                        PropagateFlags(this, Flags.IsSubtreeDirtyForRender);
                    }
                }

                return;
            }

            _flags = (_flags & ~Flags.NeverArranged) | Flags.ArrangeInProgress;

            int marginWidth = _marginLeft + _marginRight;
            int marginHeight = _marginTop + _marginBottom;

            // Alignment==Stretch --> arrange at the slot size minus margins
            // Alignment!=Stretch --> arrange at the desiredSize minus margins
            int arrangeWidth = (HorizontalAlignment == HorizontalAlignment.Stretch) ? finalRectWidth : _desiredWidth;
            int arrangeHeight = (VerticalAlignment == VerticalAlignment.Stretch) ? finalRectHeight : _desiredHeight;

            arrangeWidth -= marginWidth;
            arrangeHeight -= marginHeight;

            // If a particular size has been requested, and that size is less than the available size,
            // honor the requested size.
            if (_requestedSize != null)
            {
                if ((_requestedSize._status & Pair.Flags_First) != 0)
                {
                    int width = _requestedSize._first;
                    if (width < arrangeWidth)
                    {
                        arrangeWidth = width;
                    }
                }

                if ((_requestedSize._status & Pair.Flags_Second) != 0)
                {
                    int height = _requestedSize._second;
                    if (height < arrangeHeight)
                    {
                        arrangeHeight = height;
                    }
                }
            }

            try
            {
                ArrangeOverride(arrangeWidth, arrangeHeight);
            }
            finally
            {
                _flags &= ~Flags.ArrangeInProgress;
            }

            // Account for alignment
            int offsetX, offsetY;
            int clientWidth = System.Math.Max(0, finalRectWidth - marginWidth);
            int clientHeight = System.Math.Max(0, finalRectHeight - marginHeight);

            if (clientWidth != arrangeWidth || clientHeight != arrangeHeight)
            {
                ComputeAlignmentOffset(clientWidth, clientHeight, arrangeWidth, arrangeHeight, out offsetX, out offsetY);
#if TINYCLR_DEBUG_LAYOUT
                Trace.Print(this.ToString() + ": ComputeAlignmentOffset: " + PrintSize(clientWidth, clientHeight) + ", " + PrintSize(arrangeWidth, arrangeHeight) + " returned " + offsetX.ToString() + ", " + offsetY.ToString());
#endif
            }
            else
            {
                offsetX = offsetY = 0;
            }

            offsetX += finalRectX + _marginLeft;
            offsetY += finalRectY + _marginTop;

            _offsetX = offsetX;
            _offsetY = offsetY;
            _renderWidth = arrangeWidth;
            _renderHeight = arrangeHeight;

            // Cache final rect
            _finalX = finalRectX;
            _finalY = finalRectY;
            _finalWidth = finalRectWidth;
            _finalHeight = finalRectHeight;

#if TINYCLR_DEBUG_LAYOUT
            Trace.Print(this.ToString() + ": Layout offset = " + PrintSize(_offsetX, _offsetY));
#endif

            // Reset dirtiness
            //
            _flags &= ~Flags.InvalidArrange;

            LayoutManager.CurrentLayoutManager.ArrangeQueue.Remove(this);

            if (IsRenderable())
            {
                PropagateFlags(this, Flags.IsSubtreeDirtyForRender);
            }
        }

        /// <summary>
        /// Measurement override. Implement your size-to-content logic here.
        /// </summary>
        /// <remarks>
        /// MeasureOverride is designed to be the main customizability point for size control of layout.
        /// UIElement authors should override this method, call Measure on each child UIElement,
        /// and compute their desired size based upon the measurement of the children.
        /// The return value should be the desired size.<para/>
        /// Note: It is required that a parent UIElement calls Measure on each child or they won't be sized/arranged.
        /// Typical override follows a pattern roughly like this (pseudo-code):
        /// <example>
        ///     <code lang="C#">
        /// <![CDATA[
        ///
        /// protected override void MeasureOverride(int avialableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        /// {
        ///     foreach (UIElement child in VisualChildren)
        ///     {
        ///         child.Measure(availableSize);
        ///         availableSize.Deflate(child.DesiredSize);
        ///         _cache.StoreInfoAboutChild(child);
        ///     }
        ///
        ///     Size desired = CalculateBasedOnCache(_cache);
        ///     return desired;
        /// }
        /// ]]>
        ///     </code>
        /// </example>
        /// The key aspects of this snippet are:
        ///     <list type="bullet">
        /// <item>You must call Measure on each child UIElement</item>
        /// <item>It is common to cache measurement information between the MeasureOverride and ArrangeOverride method calls</item>
        /// <item>Calling base.MeasureOverride is not required.</item>
        /// <item>Calls to Measure on children are passing either the same availableSize as the parent, or a subset of the area depending
        /// on the type of layout the parent will perform (for example, it would be valid to remove the area
        /// for some border or padding).</item>
        ///     </list>
        /// </remarks>
        /// <param name="availableWidth">Available size that parent can give to the child. May be MaxValue(when parent wants to
        /// measure to content). This is soft constraint. Child can return bigger size to indicate that it wants bigger space and hope
        /// that parent can throw in scrolling...</param>
        /// <param name="availableHeight"></param>
        /// <param name="desiredWidth"></param>
        /// <param name="desiredHeight"></param>
        protected virtual void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            desiredHeight = desiredWidth = 0;
        }

        /// <summary>
        /// ArrangeOverride allows for the customization of the final sizing and positioning of children.
        /// </summary>
        /// <remarks>
        /// UIElement authors should override this method, call Arrange on each visible child UIElement,
        /// to size and position each child UIElement by passing a rectangle reserved for the child within parent space.
        /// Note: It is required that a parent UIElement calls Arrange on each child or they won't be rendered.
        /// Typical override follows a pattern roughly like this (pseudo-code):
        /// <example>
        ///     <code lang="C#">
        /// <![CDATA[
        ///
        /// protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight)
        /// {
        ///
        ///     foreach (UIElement child in VisualChildren)
        ///     {
        ///         child.Arrange(new Rect(childX, childY, childWidth, childHeight);
        ///     }
        /// }
        /// ]]>
        ///     </code>
        /// </example>
        /// </remarks>
        /// <param name="arrangeWidth">Final width</param>
        /// <param name="arrangeHeight">Final height</param>
        protected virtual void ArrangeOverride(int arrangeWidth, int arrangeHeight)
        {
            UIElementCollection children = _logicalChildren;
            if (children != null)
            {
                int count = children.Count;
                for (int i = 0; i < count; i++)
                {
                    UIElement child = children[i];
                    child.Arrange(0, 0, arrangeWidth, arrangeHeight);
                }
            }
        }

        /// <summary>
        /// Call this method to ensure that the whoel subtree of elements that includes this UIElement
        /// is properly updated.
        /// </summary>
        /// <remarks>
        /// This ensures that UIElements with IsMeasureInvalid or IsArrangeInvalid will
        /// get call to their MeasureOverride and ArrangeOverride, and all computed sizes will be validated.
        /// This method does nothing if layout is clean but it does work if layout is not clean so avoid calling
        /// it after each change in the UIElement tree. It makes sense to either never call it (system will do this
        /// in a deferred manner) or only call it if you absolutely need updated sizes and positions after you do all changes.
        /// </remarks>
        public void UpdateLayout()
        {
            VerifyAccess();

            LayoutManager.CurrentLayoutManager.UpdateLayout();
        }

        /// <summary>
        /// Determines if the DesiredSize is valid.
        /// </summary>
        /// <remarks>
        /// A developer can force arrangement to be invalidated by calling InvalidateMeasure.
        /// IsArrangeValid and IsMeasureValid are related,
        /// in that arrangement cannot be valid without measurement first being valid.
        /// </remarks>
        public bool IsMeasureValid
        {
            get
            {
                return (_flags & Flags.InvalidMeasure) == 0;
            }
        }

        /// <summary>
        /// Determines if the RenderSize and position of child elements is valid.
        /// </summary>
        /// <remarks>
        /// A developer can force arrangement to be invalidated by calling InvalidateArrange.
        /// IsArrangeValid and IsMeasureValid are related, in that arrangement cannot be valid without measurement first
        /// being valid.
        /// </remarks>
        public bool IsArrangeValid
        {
            get
            {
                return (_flags & Flags.InvalidArrange) == 0;
            }
        }

        /// <summary>
        /// Given x, y co-ordinates of the parent UIElement,
        /// find the child control that is directly underneath that point.
        /// If there are multiple such controls, the one that was created/inserted
        /// into the list last wins. This is because we don't have explicit z-ordering
        /// right now.
        ///

        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public UIElement ChildElementFromPoint(int x, int y)
        {
            UIElement targetElement = null;

            /// Translate.
            x -= _offsetX;
            y -= _offsetY;

            if ((x >= 0) && (y >= 0) && (x <= _renderWidth) && (y <= _renderHeight))
            {
                targetElement = this;

                UIElementCollection children = _logicalChildren;
                if (children != null) //children appears to be null, instead of empty array in some cases.
                {
                    if (children.Count > 0)
                    {
                        int count = children.Count;
                        for (int i = count - 1; i >= 0; i--)
                        {
                            UIElement child = children[i];
                            UIElement target = child.ChildElementFromPoint(x, y);
                            if (target != null)
                            {
                                targetElement = target;
                                break;
                            }
                        }
                    }
                }
            }

            return targetElement;
        }

        public void GetUnclippedSize(out int width, out int height)
        {
            width = _unclippedWidth;
            height = _unclippedHeight;
        }

        public bool ContainsPoint(int x, int y)
        {
            return (x >= _offsetX && x < (_offsetX + _renderWidth) && y >= _offsetY && y < (_offsetY + _renderHeight));
        }

        public UIElement GetPointerTarget(int x, int y)
        {
            UIElement target = null;

            UIElementCollection children = _logicalChildren;

            while (children != null)
            {
                int i = children.Count;
                while (--i >= 0)
                {
                    UIElement element = children[i];
                    if (element != null && element.Visibility == Visibility.Visible && element.ContainsPoint(x, y))
                    {
                        target = element;
                        children = element._logicalChildren;

                        x -= target._offsetX;
                        y -= target._offsetY;
                        break;
                    }
                }

                if (i < 0)
                {
                    break;
                }
            }

            return target;
        }

        /// <summary>
        /// We are deviating little from their desktop counter parts, mostly for simplicity and perf.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void PointToScreen(ref int x, ref int y)
        {
            UIElement client = this;
            while (client != null)
            {
                x += client._offsetX;
                y += client._offsetY;

                client = client._parent;
            }
        }

        public void PointToClient(ref int x, ref int y)
        {
            UIElement client = this;
            //need to cache this value on first call after relayout.
            while (client != null)
            {
                x -= client._offsetX;
                y -= client._offsetY;

                client = client._parent;
            }
        }

        // This is needed to prevent dirty elements from drawing and crashing while doing so.
        private bool IsRenderable()
        {
            // VerifyAccess(); - we're only called by Arrange() which already did verification

            //elements that were created but never invalidated/measured are clean
            //from layout perspective, but we still don't want to render them
            //because they don't have state build up enough for that.
            if ((_flags & (Flags.NeverMeasured | Flags.NeverArranged)) != 0)
                return false;

            //if UIElement is collapsed, no rendering is needed
            //it is not only perf optimization, but also protection from
            //UIElement to break itself since RenderSize is reported as (0,0)
            //when UIElement is Collapsed
            //
            if (_visibility == Visibility.Collapsed || _visibility == Visibility.Hidden)
                return false;

            return (_flags & (Flags.InvalidArrange | Flags.InvalidMeasure)) == 0;
        }

        /// <summary>
        /// Invalidates the measurement state for the UIElement.
        /// This has the effect of also invalidating the arrange state for the UIElement.
        /// The UIElement will be queued for an update layout that will occur asynchronously.
        /// </summary>
        public void InvalidateMeasure()
        {
            VerifyAccess();

            Flags flags = _flags;

            if ((flags & (Flags.InvalidMeasure | Flags.MeasureInProgress)) == 0)
            {
                _flags |= Flags.InvalidMeasure;

                if ((flags & Flags.NeverMeasured) == 0)
                {
                    LayoutManager.CurrentLayoutManager.MeasureQueue.Add(this);
                }
            }
        }

        /// <summary>
        /// Invalidates the arrange state for the UIElement.
        /// The UIElement will be queued for an update layout that will occur asynchronously.
        /// MeasureOverride will not be called unless InvalidateMeasure is also called - or that something
        /// else caused the measure state to be invalidated.
        /// </summary>
        public void InvalidateArrange()
        {
            VerifyAccess();

            Flags flags = _flags;

            if ((flags & (Flags.InvalidArrange | Flags.ArrangeInProgress)) == 0)
            {
                _flags |= Flags.InvalidArrange;

                if ((flags & Flags.NeverArranged) == 0)
                {
                    LayoutManager.CurrentLayoutManager.ArrangeQueue.Add(this);
                }
            }
        }

        public UIElement Parent
        {
            get
            {
                return _parent;
            }
        }

        public UIElement RootUIElement
        {
            get
            {
                // we use two pointers to atomically check / iterate
                // through parents
                UIElement p = null;
                UIElement pp = this;
                do
                {
                    p = pp;
                    pp = p._parent;
                } while (pp != null);

                return p;
            }
        }

        /// <summary>
        /// The CompositionTarget marks the root element. The root element is responsible
        /// for posting renders. This method is also used to ensure that the Visual is not
        /// used in multiple CompositionTargets.
        /// </summary>
        internal bool GetIsRootElement()
        {
            return ((_flags & Flags.ShouldPostRender) != 0);
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return _horizontalAlignment;
            }

            set
            {
                VerifyAccess();

                _horizontalAlignment = value;
                InvalidateArrange();
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return _verticalAlignment;
            }

            set
            {
                VerifyAccess();

                _verticalAlignment = value;
                InvalidateArrange();
            }
        }

        /// <summary>
        /// Notification that is called by Measure of a child when
        /// it ends up with different desired size for the child.
        /// </summary>
        /// <remarks>
        /// Default implementation simply calls invalidateMeasure(), assuming that layout of a
        /// parent should be updated after child changed its size.<para/>
        /// Finer point: this method can only be called in the scenario when the system calls Measure on a child,
        /// not when parent calls it since if parent calls it, it means parent has dirty layout and is recalculating already.
        /// </remarks>
        protected virtual void OnChildDesiredSizeChanged(UIElement child)
        {
            if (IsMeasureValid)
            {
                InvalidateMeasure();
            }
        }

        public virtual void OnRender(DrawingContext dc)
        {
        }

        /// <summary>
        ///     Visibility accessor
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return _visibility;
            }

            set
            {
                VerifyAccess();

                if (_visibility != value)
                {
                    bool wasVisible = (_flags & Flags.IsVisibleCache) != 0;

                    bool invalidateMeasure = (_visibility == Visibility.Collapsed || value == Visibility.Collapsed);

                    _visibility = value;

                    bool isVisible = false;

                    bool parentVisible = (_parent == null) ? false : (_parent._flags & Flags.IsVisibleCache) != 0;
                    if (parentVisible && value == Visibility.Visible)
                    {
                        _flags = _flags | Flags.IsVisibleCache;
                        isVisible = true;
                    }
                    else
                    {
                        _flags = _flags & ~Flags.IsVisibleCache;
                    }

                    if (invalidateMeasure && _parent != null)
                    {
                        _parent.InvalidateMeasure();
                    }

                    if (wasVisible != isVisible)
                    {
                        OnIsVisibleChanged(wasVisible);
                    }
                }
            }
        }

        private void OnIsVisibleChanged(bool wasVisible)
        {
            //Desktop WPF sends the event to the parent first.
            if (_isVisibleChanged != null)
            {
                _isVisibleChanged(this, new PropertyChangedEventArgs("IsVisible", wasVisible, !wasVisible));
            }

            // Loop through all children and fire events for only those that need it.
            UIElementCollection children = _logicalChildren;
            if (children != null)
            {
                int n = children.Count;
                for (int i = 0; i < n; i++)
                {
                    UIElement child = children[i];

                    if (child._visibility == Visibility.Visible)
                    {
                        /* The IsVisbile property on a child can only change if it wants to be visible.
                         * If the parent is transitioning from visible to not, it only affects visible children.
                         * If the parent is transitioning from invisible to visible, it only affects children who want to be visible.
                         */
                        if (!wasVisible)
                        {
                            child._flags = child._flags | Flags.IsVisibleCache;
                        }
                        else
                        {
                            child._flags = child._flags & ~Flags.IsVisibleCache;
                        }

                        child.OnIsVisibleChanged(wasVisible);
                    }
                }
            }
        }

        /// <summary>
        ///     A property indicating if this element is Visible or not.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return (_flags & Flags.IsVisibleCache) != 0;
            }
        }

        public event PropertyChangedEventHandler IsVisibleChanged
        {
            add
            {
                VerifyAccess();

                _isVisibleChanged += value;
            }

            remove
            {
                VerifyAccess();

                _isVisibleChanged -= value;
            }
        }

        /// <summary>
        ///     Fetches the value of the IsEnabled property
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                // REFACTOR
                // Implement this similar to IsVisibleChanged and fire
                // IsEnabledChanged events on child elements?

                // If our parent is not enabled, then we cannot be either.
                UIElement parent = _parent;

                bool isEnabled = parent == null ? true : parent.IsEnabled;

                if (isEnabled)
                {
                    // If our parent was enabled, then we may or may not be.
                    isEnabled = ((_flags & Flags.Enabled) != 0);
                }

                return isEnabled;
            }

            set
            {
                VerifyAccess();

                bool wasEnabled = IsEnabled;

                if (value)
                {
                    _flags |= Flags.Enabled;
                }
                else
                {
                    _flags &= ~Flags.Enabled;
                }

                if (_isEnabledChanged != null && wasEnabled != IsEnabled)
                {
                    _isEnabledChanged(this, new PropertyChangedEventArgs("IsEnabled", wasEnabled, !wasEnabled));
                }
            }
        }

        public event PropertyChangedEventHandler IsEnabledChanged
        {
            add
            {
                VerifyAccess();

                _isEnabledChanged += value;
            }

            remove
            {
                VerifyAccess();

                _isEnabledChanged -= value;
            }
        }

        protected internal virtual void RenderRecursive(DrawingContext dc)
        {
            Debug.Assert(this.IsMeasureValid && this.IsArrangeValid);

            dc.Translate(_offsetX, _offsetY);
            dc.PushClippingRectangle(0, 0, _renderWidth, _renderHeight);
            try
            {
                Debug.Assert(this.Visibility == Visibility.Visible);

                if (!dc.EmptyClipRect)
                {
                    OnRender(dc);
                    UIElementCollection children = _logicalChildren;
                    if (children != null)
                    {
                        int n = children.Count;
                        for (int i = 0; i < n; i++)
                        {
                            UIElement child = children[i];
                            if (child.IsRenderable())
                            {
                                child.RenderRecursive(dc);
                            }
                        }
                    }
                }
            }
            finally
            {
                dc.PopClippingRectangle();
                dc.Translate(-_offsetX, -_offsetY);

                //-------------------------------------------------------------------------------
                // Reset the render flags.

                _flags &= ~(Flags.IsSubtreeDirtyForRender | Flags.IsDirtyForRender);
            }
        }

        internal static void PropagateFlags(UIElement e, Flags flags)
        {
            while ((e != null) && ((e._flags & flags) == 0))
            {
                e._flags |= flags;

                if ((e._flags & Flags.ShouldPostRender) != 0)
                {
                    MediaContext.From(e.Dispatcher).PostRender();
                }

                e = e._parent;
            }
        }

        private void MarkDirtyRect(int x, int y, int w, int h)
        {
            PointToScreen(ref x, ref y);
            MediaContext.From(Dispatcher).AddDirtyArea(x, y, w, h);

            PropagateFlags(
                this,
                Flags.IsSubtreeDirtyForRender);
        }

        public void InvalidateRect(int x, int y, int w, int h)
        {
            VerifyAccess();

            MarkDirtyRect(x, y, w, h);
        }

        public void Invalidate()
        {
            VerifyAccess();

            MarkDirtyRect(0, 0, _renderWidth, _renderHeight);
        }

        #region Eventing

        /// <summary>
        ///     Raise the events specified by
        ///     <see cref="RoutedEventArgs.RoutedEvent"/>
        /// </summary>
        /// <remarks>
        ///     This method is a shorthand for
        ///     This method walks up the visual tree, calling
        ///     <see cref="UIElement.BuildRouteCore"/>
        ///     on every <see cref="UIElement"/> <para/>
        ///     <para/>
        ///
        ///     NOTE: The RoutedEvent in RoutedEventArgs
        ///     and EventRoute must be matched
        ///
        ///     Once the route is built, it calls InvokeHandlers()
        /// </remarks>
        /// <param name="args">
        ///     <see cref="RoutedEventArgs"/> for the event to
        ///     be raised
        /// </param>
        public void RaiseEvent(RoutedEventArgs args)
        {
            // Verify Context Access
            VerifyAccess();

            if (args == null)
            {
                throw new ArgumentNullException();
            }

            EventRoute route = new EventRoute(args._routedEvent);

            // Set Source
            args.Source = this;

            // direct.
            if (args._routedEvent._routingStrategy == RoutingStrategy.Direct)
            {
                this.AddToEventRouteImpl(route, args);
            }
            else
            {
                int cElements = 0;

                UIElement uiElement = this;

                do
                {
                    // Protect against infinite loops by limiting the number of elements
                    // that we will process.
                    if (cElements++ > MAX_ELEMENTS_IN_ROUTE)
                    {
                        throw new InvalidOperationException(/*SR.Get(SRID.TreeLoop) */);
                    }

                    uiElement.AddToEventRouteImpl(route, args);

                    uiElement = uiElement._parent;

                } while (uiElement != null);
            }

            route.InvokeHandlers(this, args);

            // Reset Source to OriginalSource
            args.Source = args.OriginalSource;
        }

        /// <summary>
        ///     Add the event handlers for this element to the route.
        /// </summary>
        // REFACTOR -- do we need this to be public?
        public void AddToEventRoute(EventRoute route, RoutedEventArgs args)
        {
            VerifyAccess();

            if (route == null || args == null)
            {
                throw new ArgumentNullException();
            }

            AddToEventRouteImpl(route, args);
        }

        private void AddToEventRouteImpl(EventRoute route, RoutedEventArgs args)
        {
            //
            // add class listeners then instance listeners.
            //
            Hashtable store = _classEventHandlersStore;
            RoutedEvent evt = args._routedEvent;
            for (int repeat = 0; repeat < 2; repeat++)
            {
                if (store != null)
                {
                    ArrayList eventListeners = (ArrayList)store[evt];

                    // Add all listeners for this UIElement
                    if (eventListeners != null)
                    {
                        for (int i = 0, count = eventListeners.Count; i < count; i++)
                        {
                            RoutedEventHandlerInfo eventListener = (RoutedEventHandlerInfo)eventListeners[i];
                            route.Add(this, eventListener._handler, eventListener._handledEventsToo);
                        }
                    }
                }

                store = _instanceEventHandlersStore;
            }
        }

        #endregion

        #region Instance Handlers

        /// <summary>
        /// Ensure the store has been created.
        /// </summary>
        protected Hashtable InstanceEventHandlersStore
        {
            get
            {
                if (_instanceEventHandlersStore == null)
                {
                    _instanceEventHandlersStore = new Hashtable();
                }

                return _instanceEventHandlersStore;
            }
        }

        /// <summary>
        ///     Adds a routed event handler for the particular
        ///     <see cref="RoutedEvent"/>
        /// </summary>
        /// <remarks>
        ///     The handler added thus is also known as
        ///     an instance handler <para/>
        ///     <para/>
        ///
        ///     NOTE: It is not an error to add a handler twice
        ///     (handler will simply be called twice) <para/>
        ///     <para/>
        ///
        ///     Input parameters <see cref="RoutedEvent"/>
        ///     and handler cannot be null <para/>
        ///     handledEventsToo input parameter when false means
        ///     that listener does not care about already handled events.
        ///     Hence the handler will not be invoked on the target if
        ///     the RoutedEvent has already been
        ///     <see cref="RoutedEventArgs.Handled"/> <para/>
        ///     handledEventsToo input parameter when true means
        ///     that the listener wants to hear about all events even if
        ///     they have already been handled. Hence the handler will
        ///     be invoked irrespective of the event being
        ///     <see cref="RoutedEventArgs.Handled"/>
        /// </remarks>
        /// <param name="routedEvent">
        ///     <see cref="RoutedEvent"/> for which the handler
        ///     is attached
        /// </param>
        /// <param name="handler">
        ///     The handler that will be invoked on this object
        ///     when the RoutedEvent is raised
        /// </param>
        /// <param name="handledEventsToo">
        ///     Flag indicating whether or not the listener wants to
        ///     hear about events that have already been handled
        /// </param>
        public void AddHandler(
            RoutedEvent routedEvent,
            RoutedEventHandler handler,
            bool handledEventsToo)
        {
            // Verify Context Access
            this.VerifyAccess();

            if (routedEvent == null || handler == null)
            {
                throw new ArgumentNullException();
            }

            AddRoutedEventHandler(InstanceEventHandlersStore, routedEvent, handler, handledEventsToo);
        }

        #endregion

#if TINYCLR_DEBUG_LAYOUT
        public static string PrintRect(int x, int y, int width, int height)
        {
            return "[" + x + ", " + y + ", " + width + ", " + height + "]";
        }

        public static string PrintSize(int x, int y)
        {
            return "[" + x + ", " + y + "]";
        }

#endif

        internal const int MAX_ELEMENTS_IN_ROUTE = 256;

        [Flags]
        internal enum Flags : uint
        {
            None = 0x00000000,
            // IsSubtreeDirtyForRender indicates that at least one element in the sub-graph of this element needs to
            // be re-rendered.
            IsSubtreeDirtyForRender = 0x00000002,
            // IsDirtyForRender indicates that the element has changed in a way that all it's children
            // need to be updated. E.g. more/less children clipped, children themselves
            // changed, clip changed => more/less children clipped
            //
            IsDirtyForRender = 0x00000004,

            Enabled = 0x00000020,
            InvalidMeasure = 0x00000040,
            InvalidArrange = 0x00000080,
            MeasureInProgress = 0x00000100,
            ArrangeInProgress = 0x00000200,
            MeasureDuringArrange = 0x00000400,
            NeverMeasured = 0x00000800,
            NeverArranged = 0x00001000,
            // Should post render indicates that this is a root element and therefore we need to indicate that this
            // element tree needs to be re-rendered. Today we are doing this by posting a render queue item.
            ShouldPostRender = 0x00002000,
            IsLayoutSuspended = 0x00004000,

            IsVisibleCache = 0x00008000,
        }

        //--//

        internal UIElement _parent;
        internal UIElementCollection _logicalChildren;

        //
        internal Flags _flags;
        private Visibility _visibility;

        // Layout
        //
        internal class Pair
        {
            public const int Flags_First = 0x1;  // Can be (optionally) used with _status
            public const int Flags_Second = 0x2;

            public int _first;
            public int _second;
            public int _status;
        }

        internal Pair _requestedSize;            // Used when Width/Height properties are set
        internal Pair _anchorInfo;               // Used if the parent is a Canvas

        private int _marginLeft;
        private int _marginTop;
        private int _marginRight;
        private int _marginBottom;

        protected HorizontalAlignment _horizontalAlignment;
        protected VerticalAlignment _verticalAlignment;

        // Cached layout information
        //
        internal int _finalX;
        internal int _finalY;
        internal int _finalWidth;
        internal int _finalHeight;

        internal int _offsetX;
        internal int _offsetY;
        internal int _renderWidth;
        internal int _renderHeight;

        internal int _previousAvailableWidth;
        internal int _previousAvailableHeight;

        private int _desiredWidth;
        private int _desiredHeight;

        internal int _unclippedWidth;
        internal int _unclippedHeight;

        // Routed Event Handling

        private static Hashtable _classEventHandlersStore;
        private Hashtable _instanceEventHandlersStore;

        // Regular Events
        PropertyChangedEventHandler _isEnabledChanged;
        PropertyChangedEventHandler _isVisibleChanged;
    }
}


