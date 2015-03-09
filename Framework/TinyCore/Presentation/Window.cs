////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Presentation
{
    public class Window : ContentControl
    {
        //---------------------------------------------------
        //
        // Constructors
        //
        //---------------------------------------------------
        #region Constructors

        /// <summary>
        ///     Constructs a window object
        /// </summary>
        /// <remarks>
        ///     Automatic determination of current Dispatcher. Use alternative constructor
        ///     that accepts a Dispatcher for best performance.
        /// REFACTOR -- consider specifying app default window sizes to cover Aux case for default window size.
        /// </remarks>
        ///     Initializes the Width/Height, Top/Left properties to use windows
        ///     default. Updates Application object properties if inside app.
        public Window()
        {
            //There is only one WindowManager.  All Windows currently are forced to be created
            //and to live on the same thread.
            _windowManager = WindowManager.EnsureInstance();

            _background = new SolidColorBrush(Colors.White);
            //
            // dependency property initialization.
            // we don't have them, so we just update the properties on the base class,
            // like normal *bleep* fearing developers.
            //
            // Visibility HAS to be set to Collapsed prior to adding this child to the
            // window manager, otherwise the window manager sets the focus to this window
            this.Visibility = Visibility.Collapsed;

            // register us with the window manager, like a good little boy
            _windowManager.Children.Add(this);

            Application app = Microsoft.SPOT.Application.Current;

            // check if within an app && on the same thread
            if (app != null)
            {
                if (app.Dispatcher.Thread == Dispatcher.CurrentDispatcher.Thread)
                {
                    // add to window collection
                    // use internal version since we want to update the underlying collection
                    app.WindowsInternal.Add(this);
                    if (app.MainWindow == null)
                    {
                        app.MainWindow = this;
                    }
                }
                else
                {
                    app.NonAppWindowsInternal.Add(this);
                }
            }
        }

        #endregion Constructors

        #region Public Methods

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        public void Close()
        {
            Application app = Microsoft.SPOT.Application.Current;
            if (app != null)
            {
                app.WindowsInternal.Remove(this);
                app.NonAppWindowsInternal.Remove(this);
            }

            if(_windowManager != null)
            {
                _windowManager.Children.Remove(this);
                _windowManager = null;
            }
        }

        #endregion Public Methods

        #region Public Properties

        /// <summary>
        /// Auto size Window to its content's size
        /// </summary>
        /// <remarks>
        /// 1. SizeToContent can be applied to Width Height independently
        /// 2. After SizeToContent is set, setting Width/Height does not take affect if that
        ///    dimension is sizing to content.
        /// </remarks>
        /// <value>
        /// Default value is SizeToContent.Manual
        /// </value>
        public SizeToContent SizeToContent
        {
            get
            {
                return _sizeToContent;
            }

            set
            {
                VerifyAccess();
                _sizeToContent = value;
            }
        }

        /// <summary>
        ///     Position for Top of the host window
        /// </summary>
        /// <value></value>
        public int Top
        {
            get
            {
                return Canvas.GetTop(this);
            }

            set
            {
                VerifyAccess();
                Canvas.SetTop(this, value);
            }
        }

        public int Left
        {
            get
            {
                return Canvas.GetLeft(this);
            }

            set
            {
                VerifyAccess();

                Canvas.SetLeft(this, value);
            }
        }

        /// <summary>
        ///     Determines if this window is always on the top.
        /// </summary>
        public bool Topmost
        {
            get
            {
                return _windowManager.IsTopMost(this);
            }

            set
            {
                VerifyAccess();

                _windowManager.SetTopMost(this);
            }
        }

        #endregion Public Properties

        //---------------------------------------------------
        //
        // Public Events
        //
        //---------------------------------------------------
        #region Public Events

        #endregion Public Events

        //---------------------------------------------------
        //
        // Protected Methods
        //
        //---------------------------------------------------
        #region Protected Methods

        // REFACTOR -- need to track if our parent changes.

        /// <summary>
        ///     Measurement override. Implements content sizing logic.
        /// </summary>
        /// <remarks>
        ///     Deducts the frame size from the constraint and then passes it on
        ///     to it's child.  Only supports one Visual child (just like control)
        /// </remarks>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            UIElementCollection children = this.LogicalChildren;

            if (children.Count > 0)
            {
                UIElement child = (UIElement)children[0];
                if (child != null)
                {
                    // REFACTOR --we need to subtract the frame & chrome around the visual child.
                    child.Measure(availableWidth, availableHeight);
                    child.GetDesiredSize(out desiredWidth, out desiredHeight);

                    return;
                }
            }

            desiredWidth = availableWidth;
            desiredHeight = availableHeight;
        }

        /// <summary>
        ///     ArrangeOverride allows for the customization of the positioning of children.
        /// </summary>
        /// <remarks>
        ///     Deducts the frame size of the window from the constraint and then
        ///     arranges it's child.  Supports only one child.
        /// </remarks>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight)
        {
            UIElementCollection children = this.LogicalChildren;

            if (children.Count > 0)
            {
                UIElement child = children[0] as UIElement;
                if (child != null)
                {
                    child.Arrange(0, 0, arrangeWidth, arrangeHeight);
                }
            }
        }

        #endregion Protected Methods

        //---------------------------------------------------
        //
        // Internal Methods
        //
        //---------------------------------------------------
        #region Internal Methods

        #endregion Internal Methods

        //----------------------------------------------
        //
        // Internal Properties
        //
        //----------------------------------------------
        #region Internal Properties

        #endregion Internal Properties

        //----------------------------------------------
        //
        // Private Methods
        //
        //----------------------------------------------
        #region Private Methods

        //
        // These are the callbacks used by the windowSource to notify the window
        // about what the window manager wants it to do.
        //
        #region WindowManager internal methods

        #endregion WindowManager internal methods

        #endregion Private Methods

        //----------------------------------------------
        //
        // Private Properties
        //
        //----------------------------------------------
        #region Private Properties

        #endregion Private Properties

        //----------------------------------------------
        //
        // Private Fields
        //
        //----------------------------------------------
        #region Private Fields

        private SizeToContent _sizeToContent;
        private WindowManager _windowManager;

        #endregion Private Fields
    }
}


