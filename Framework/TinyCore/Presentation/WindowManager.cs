////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;

using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Hardware;

using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation
{
    public delegate void PostRenderEventHandler(DrawingContext dc);

    public class WindowManager : Controls.Canvas
    {
        private WindowManager()
        {
            //
            // initially measure and arrange ourselves.
            //
            Instance = this;

            //
            // WindowManagers have no parents but they are Visible.
            //
            _flags = _flags | Flags.IsVisibleCache;

            Measure(Media.Constants.MaxExtent, Media.Constants.MaxExtent);

            int desiredWidth, desiredHeight;

            GetDesiredSize(out desiredWidth, out desiredHeight);

            Arrange(0, 0, desiredWidth, desiredHeight);
        }

        internal static WindowManager EnsureInstance()
        {
            if (Instance == null)
            {
                WindowManager wm = new WindowManager();
                // implicitly the window manager is responsible for posting renders
                wm._flags |= Flags.ShouldPostRender;
            }

            return Instance;
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            base.MeasureOverride(availableWidth, availableHeight, out desiredWidth, out desiredHeight);
            desiredWidth = SystemMetrics.ScreenWidth;
            desiredHeight = SystemMetrics.ScreenHeight;
        }

        internal void SetTopMost(Window window)
        {
            UIElementCollection children = LogicalChildren;

            if (!IsTopMost(window))
            {
                children.Remove(window);
                children.Add(window);
            }
        }

        internal bool IsTopMost(Window window)
        {
            int index = LogicalChildren.IndexOf(window);
            return (index >= 0 && index == LogicalChildren.Count - 1);
        }

        //
        // this was added for aux, behavior needs to change for watch.
        //
        protected internal override void OnChildrenChanged(UIElement added, UIElement removed, int indexAffected)
        {
            base.OnChildrenChanged(added, removed, indexAffected);

            UIElementCollection children = LogicalChildren;
            int last = children.Count - 1;

            // something was added, and it's the topmost. Make sure it is visible before setting focus
            if (added != null && indexAffected == last && Visibility.Visible == added.Visibility)
            {
                Input.Buttons.Focus(added);
                Input.TouchCapture.Capture(added);
            }

            // something was removed and it lost focus to us.
            if (removed != null && this.IsFocused)
            {
                // we still have a window left, so make it focused.
                if (last >= 0)
                {
                    Input.Buttons.Focus(children[last]);
                    Input.TouchCapture.Capture(children[last]);
                }
            }
        }

        //--//

        public static WindowManager Instance;

        //--//

        private PostRenderEventHandler _postRenderHandler;

        public event PostRenderEventHandler PostRender
        {
            add
            {
                _postRenderHandler += value;
            }

            remove
            {
                _postRenderHandler -= value;
            }
        }

        protected internal override void RenderRecursive(DrawingContext dc)
        {
            base.RenderRecursive(dc);

            if (_postRenderHandler != null)
            {
                _postRenderHandler(dc);
            }
        }
    }

}


