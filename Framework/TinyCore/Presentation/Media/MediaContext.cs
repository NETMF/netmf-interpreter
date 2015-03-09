////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Presentation.Media
{
    /// <summary>
    /// The MediaContext class controls the rendering
    /// </summary>
    internal class MediaContext : DispatcherObject , IDisposable
    {
        /// <summary>
        /// The MediaContext lives in the Dispatcher and is the MediaSystem's class that keeps
        /// per Dispatcher state.
        /// </summary>
        internal MediaContext()
        {
            _renderMessage = new DispatcherOperationCallback(RenderMessageHandler);

            // we have one render target, the window manager
            _target = WindowManager.Instance;
            _screen = new Bitmap(SystemMetrics.ScreenWidth, SystemMetrics.ScreenHeight);
        }

        /// <summary>
        /// Gets the MediaContext from the context passed in as argument.
        /// </summary>
        internal static MediaContext From(Dispatcher dispatcher)
        {
            Debug.Assert(dispatcher != null, "Dispatcher required");

            MediaContext cm = dispatcher._mediaContext;

            if (cm == null)
            {
                lock (typeof(GlobalLock))
                {
                    cm = dispatcher._mediaContext;

                    if (cm == null)
                    {
                        cm = new MediaContext();
                        dispatcher._mediaContext = cm;
                    }
                }
            }

            return cm;
        }

        private class InvokeOnRenderCallback
        {
            private DispatcherOperationCallback _callback;
            private object _arg;

            public InvokeOnRenderCallback(
                DispatcherOperationCallback callback,
                object arg)
            {
                _callback = callback;
                _arg = arg;
            }

            public void DoWork()
            {
                _callback(_arg);
            }
        }

        internal void BeginInvokeOnRender(DispatcherOperationCallback callback, object arg)
        {
            Debug.Assert(callback != null);

            // While technically it could be OK for the arg to be null, for now
            // I know that arg represents the this reference for the layout
            // process and should never be null.

            Debug.Assert(arg != null);

            if (_invokeOnRenderCallbacks == null)
            {
                lock (this)
                {
                    if (_invokeOnRenderCallbacks == null)
                    {
                        _invokeOnRenderCallbacks = new ArrayList();
                    }
                }
            }

            lock (_invokeOnRenderCallbacks)
            {
                _invokeOnRenderCallbacks.Add(new InvokeOnRenderCallback(callback, arg));
            }

            PostRender();
        }

        /// <summary>
        /// If there is already a render operation in the Dispatcher queue, this
        /// method will do nothing.  If not, it will add a
        /// render operation.
        /// </summary>
        /// <remarks>
        /// This method should only be called when a render is necessary "right
        /// now."  Events such as a change to the visual tree would result in
        /// this method being called.
        /// </remarks>
        internal void PostRender()
        {
            VerifyAccess();

            if (!_isRendering)
            {
                if (_currentRenderOp == null)
                {
                    // If we don't have a render operation in the queue, add one
                    _currentRenderOp = Dispatcher.BeginInvoke(_renderMessage, null);
                }
            }
        }

        internal void AddDirtyArea(int x, int y, int w, int h)
        {
            if (x < 0) x = 0;
            if (x + w > _screenW) w = _screenW - x;
            if (w <= 0) return;

            if (y < 0) y = 0;
            if (y + h > _screenH) h = _screenH - y;
            if (h <= 0) return;

            int x1 = x + w;
            int y1 = y + h;

            if (x < _dirtyX0) _dirtyX0 = x;
            if (y < _dirtyY0) _dirtyY0 = y;
            if (x1 > _dirtyX1) _dirtyX1 = x1;
            if (y1 > _dirtyY1) _dirtyY1 = y1;
        }

        private int _screenW = SystemMetrics.ScreenWidth, _screenH = SystemMetrics.ScreenHeight;
        private int _dirtyX0 = SystemMetrics.ScreenWidth, _dirtyY0 = SystemMetrics.ScreenHeight, _dirtyX1 = 0, _dirtyY1 = 0;

        /// <summary>
        /// This is the standard RenderMessageHandler callback, posted via PostRender()
        /// and Resize().  This wraps RenderMessageHandlerCore and emits an ETW events
        /// to trace its execution.
        /// </summary>
        internal object RenderMessageHandler(object arg)
        {
            try
            {
                _isRendering = true;

                //_screen.Clear();

                if (_invokeOnRenderCallbacks != null)
                {
                    int callbackLoopCount = 0;
                    int count = _invokeOnRenderCallbacks.Count;

                    while (count > 0)
                    {
                        callbackLoopCount++;
                        if (callbackLoopCount > 153)
                        {
                            throw new InvalidOperationException("infinite loop");
                        }

                        InvokeOnRenderCallback[] callbacks;

                        lock (_invokeOnRenderCallbacks)
                        {
                            count = _invokeOnRenderCallbacks.Count;
                            callbacks = new InvokeOnRenderCallback[count];

                            _invokeOnRenderCallbacks.CopyTo(callbacks);
                            _invokeOnRenderCallbacks.Clear();
                        }

                        for (int i = 0; i < count; i++)
                        {
                            callbacks[i].DoWork();
                        }

                        count = _invokeOnRenderCallbacks.Count;
                    }
                }

                DrawingContext dc = new DrawingContext(_screen);

                /* The dirty rectange MUST be read after the InvokeOnRender callbacks are
                 * complete, as they can trigger layout changes or invalidate controls
                 * which are expected to be redrawn. */
                int x = _dirtyX0;
                int y = _dirtyY0;
                int w = _dirtyX1 - _dirtyX0;
                int h = _dirtyY1 - _dirtyY0;
                _dirtyX0 = _screenW; _dirtyY0 = _screenH;
                _dirtyX1 = _dirtyY1 = 0;

                try
                {
                    if (w > 0 && h > 0)
                    {
                        //
                        // This is the big Render!
                        //
                        // We've now updated layout and the updated scene will be
                        // rendered.
                        dc.PushClippingRectangle(x, y, w, h);
                        _target.RenderRecursive(dc);
                        dc.PopClippingRectangle();
                    }
                }
                finally
                {
                    dc.Close();
                    if (w > 0 && h > 0)
                    {
                        _screen.Flush(x, y, w, h);
                    }
                }
            }
            finally
            {
                _currentRenderOp = null;
                _isRendering = false;
            }

            return null;
        }

        /// <summary>
        /// Message delegate.
        /// </summary>
        private DispatcherOperation _currentRenderOp;
        private DispatcherOperationCallback _renderMessage;

        /// <summary>
        /// Indicates that we are in the middle of processing a render message.
        /// </summary>
        private bool _isRendering;

        private ArrayList _invokeOnRenderCallbacks;

        private UIElement _target;
        private Bitmap _screen;

        private class GlobalLock { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _screen.Dispose();    
        }

    }
}


