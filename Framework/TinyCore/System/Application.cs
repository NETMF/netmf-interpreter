////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT
{

    /*
    /// <summary>+
    /// Delegate for the Exit event.
    /// </summary>
    public delegate void ExitEventHandler(Object sender, ExitEventArgs e);

    /// <summary>
    /// Delegate for SessionEnding event
    /// </summary>
    public delegate void SessionEndingCancelEventHandler(Object sender, SessionEndingCancelEventArgs e);

*/

    #region Application Class

    /// <summary>
    /// Application base class
    /// </summary>
    public class Application : DispatcherObject, IEventListener
    {

        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors

        /// <summary>
        ///     Application constructor
        /// </summary>
        public Application()
        {
            /* TRACING
                        if (EventTrace.IsEnabled(EventTrace.Flags.performance, EventTrace.Level.normal))
                        {
                            EventTrace.EventProvider.TraceEvent(EventTrace.APPGUID, MS.Utility.EventType.Info,
                                                                EventTrace.APPCTOR);
                        }

            */
            lock (typeof(GlobalLock))
            {
                // set the default statics
                // DO NOT move this from the begining of this constructor
                if (_appCreatedInThisAppDomain == false)
                {
                    Debug.Assert(_appInstance == null, "_appInstance must be null here.");
                    _appInstance = this;
                    IsShuttingDown = false;
                    _appCreatedInThisAppDomain = true;
                }
                else
                {
                    //lock will be released, so no worries about throwing an exception inside the lock
                    throw new InvalidOperationException("application is a singleton");
                }
            }

            Dispatcher.SetFinalDispatcherExceptionHandler(new DispatcherExceptionEventHandler(DefaultContextExceptionHandler));

            //
            // post item to do startup work
            // posting it here so that this is the first item in the queue. Devs
            // could post items before calling run and then those will be serviced
            // before if we don't post this one here.
            //
            // Also, doing startup (firing OnStartup etc.) once our dispatcher
            // is run ensures that we run before any external code is run in the
            // application's Dispatcher.
            Dispatcher.BeginInvoke(new DispatcherOperationCallback(this.StartupCallback), null);
        }

        private object StartupCallback(object unused)
        {
            // Event handler exception continuality: if exception occurs in Startup event handler,
            // our state would not be corrupted because it is fired by posting the item in the queue.
            // Please check Event handler exception continuality if the logic changes.
            OnStartup(new EventArgs());

            return null;
        }

        #endregion Constructors

        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        ///<summary>
        ///     Run is called to start an application.
        ///
        ///     Typically a developer will do some setting of properties/attaching to events after instantiating an application object,
        ///     and then call Run() to start the application.
        ///</summary>
        ///<remarks>
        ///     Once run has been called - an application's OnStartup override and Startup event is called
        ///     immediately afterwards.
        ///</remarks>
        /// <returns>ExitCode of the application</returns>
        public void Run()
        {
            /* TRACING
                        if (EventTrace.IsEnabled(EventTrace.Flags.performance, EventTrace.Level.normal))
                        {
                            EventTrace.EventProvider.TraceEvent(EventTrace.APPGUID, MS.Utility.EventType.Info,
                                                                EventTrace.APPRUN);
                        }

            */
            this.Run(null);
        }

        ///<summary>
        ///     Run is called to start an application.
        ///
        ///     Typically a developer will do some setting of properties/attaching to events after instantiating an application object,
        ///     and then call Run() to start the application.
        ///</summary>
        ///<remarks>
        ///     Once run has been called - an application's OnStartup override and Startup event is called
        ///     immediately afterwards.
        ///</remarks>
        /// <param name="window">Window that will be added to the Windows property and made the MainWindow of the Applcation.
        /// The passed Window must be created on the same thread as the Application object.  Furthermore, this Window is
        /// shown once the Application is run.</param>
        public void Run(Window window)
        {
            VerifyAccess();
            // In this case, we should throw an exception when Run is called for the second time.
            // When app is shutdown, _appIsShutdown is set to true.  If it is true here, then we
            // throw an exception
            if (_appIsShutdown == true)
            {
                throw new InvalidOperationException("cannot call Run multiple times");
            }

            //This is the only UI thread.  Make sure the WindowManager is created on this thread.
            WindowManager.EnsureInstance();

            if (window != null)
            {
                if (window.CheckAccess() == false)
                {
                    throw new ArgumentException("window must be on same dispatcher");
                }

                if (WindowsInternal.HasItem(window) == false)
                {
                    WindowsInternal.Add(window);
                }

                if (MainWindow == null)
                {
                    MainWindow = window;
                }

                if (window.Visibility != Visibility.Visible)
                {
                    Dispatcher.BeginInvoke(new DispatcherOperationCallback(this.ShowWindow), window);
                }
            }

            /*
        REFACTOR

        In Avalon at this point, the application hooks up a message loop
        so that it can detect when the application is activated and
        de-activated.

        In our case this communication will happen with the main application domain.

        We will need to pass it a stub that is an MBRO so that we can
        get callbacks when various things that are important to the app happen.

            */

            //Even if the subclass app cancels the event we still want to create and run the dispatcher
            //so that when the app explicitly calls Shutdown, we have a dispatcher to service the posted
            //Shutdown DispatcherOperationCallback

            // run the dispatcher - this method will not return
            // until the dispatcher is killed
            _ownDispatcherStarted = true;
            Dispatcher.Run();
        }

        /// <summary>
        ///     Shutdown is called to programmatically shutdown an application.
        ///
        ///     Once shutdown() is called, the application gets called with the
        ///     OnShutdown method to raise the Shutdown event.
        /// </summary>
        public void Shutdown()
        {
            VerifyAccess();

            //Already called once??
            if (IsShuttingDown == true)
            {
                return;
            }

            IsShuttingDown = true;

            Dispatcher.BeginInvoke(new DispatcherOperationCallback(ShutdownCallback), null);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void InitializeForEventSource()
        {
            if (_inputManager == null)
            {
                _inputManager = InputManager.CurrentInputManager;

                _inputProviderSite =
                    _inputManager.RegisterInputProvider(this);

                _reportInputMethod =
                    new DispatcherOperationCallback(delegate(object o)
                            {
                                InputReportArgs args = (InputReportArgs)o;
                                return _inputProviderSite.ReportInput(args.Device, args.Report);
                            });
            }
        }

        public bool OnEvent(BaseEvent ev)
        {
            InputReport ir = null;
            InputDevice dev = null;

            /// Process known events, otherwise forward as generic to MainWindow.
            ///

            TouchEvent touchEvent = ev as TouchEvent;
            if (touchEvent != null)
            {
                Microsoft.SPOT.Presentation.UIElement targetWindow = TouchCapture.Captured;

                ///
                ///  Make sure the current event's coordinates are contained in the current
                ///  stylus/touch window, if not then search for the appropriate window
                ///
                if (targetWindow != null && touchEvent.EventMessage == (byte)TouchMessages.Down)
                {
                    int x = 0, y = 0, w, h;
                    int xSrc = touchEvent.Touches[0].X;
                    int ySrc = touchEvent.Touches[0].Y;

                    targetWindow.PointToScreen(ref x, ref y);
                    targetWindow.GetRenderSize(out w, out h);

                    if (!(x <= xSrc && xSrc <= (x + w) &&
                        y <= ySrc && ySrc <= (y + h)))
                    {
                        // only look for different target window if the touch point is inside 
                        // the system metrics, otherwise, it may be a touch calibration point
                        // which is translated in the application layer.
                        if(xSrc <= SystemMetrics.ScreenWidth &&
                           ySrc <= SystemMetrics.ScreenHeight)
                        {
                            targetWindow = null;
                        }
                    }
                }

                if (targetWindow == null)
                {
                    //If we can enforce that the first event in the array is always the primary touch, we don't have to
                    //search.
                    targetWindow = WindowManager.Instance.GetPointerTarget(touchEvent.Touches[0].X, touchEvent.Touches[0].Y);
                }

                if (targetWindow != null)
                {
                    _inputManager.TouchDevice.SetTarget(targetWindow);
                }
                else
                {
                    _inputManager.TouchDevice.SetTarget(MainWindow);
                }

                ir =
                   new RawTouchInputReport(
                       null,
                       touchEvent.Time,
                       touchEvent.EventMessage,
                       touchEvent.Touches
                       );

                dev = _inputManager._touchDevice;

            }
            else if (ev is GenericEvent)
            {
                GenericEvent genericEvent = (GenericEvent)ev;

                    Microsoft.SPOT.Presentation.UIElement targetWindow = TouchCapture.Captured;

                    if (targetWindow == null)
                    {
                        targetWindow = WindowManager.Instance.GetPointerTarget(genericEvent.X, genericEvent.Y);
                    }

                    if (targetWindow != null)
                    {
                        _inputManager.GenericDevice.SetTarget(targetWindow);
                    }
                    else
                    {
                        _inputManager.GenericDevice.SetTarget(MainWindow);
                    }

                    ir = new RawGenericInputReport(
                           null,
                           genericEvent
                           );

                    dev = _inputManager._genericDevice;

            }
            else
            {
                /// Unkown event.
            }

            this.Dispatcher.BeginInvoke(_reportInputMethod, new InputReportArgs(dev, ir));

            return true;
        }

        #endregion Public Methods

        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        #region Public Properties

        /// <summary>
        ///     The Current property enables the developer to always get to the application in
        ///     AppDomain in which they are running.
        /// </summary>
        static public Application Current
        {
            get
            {
                lock (typeof(GlobalLock))
                {
                    return _appInstance;
                }
            }
        }

        /// <summary>
        ///     The Windows property exposes a WindowCollection object, from which a developer
        ///     can iterate over all the windows that have been opened in the current application.
        /// </summary>
        // DO-NOT USE THIS PROPERY IF YOU MEAN TO MODIFY THE UNDERLYING COLLECTION.  USE
        // WindowsInternal PROPERTY FOR MODIFYING THE UNDERLYING DATASTRUCTURE.
        public WindowCollection Windows
        {
            get
            {
                return WindowsInternal.Clone();
            }
        }

        /// <summary>
        ///     The MainWindow property indicates the primary window of the application.
        /// </summary>
        /// <remarks>
        ///     By default - MainWindow will be set to the first window opened in the application.
        ///     However the MainWindow may be set programmatically to indicate "this is my main window".
        ///     It is a recommended programming style to refer to MainWindow in code instead of Windows[0].
        ///
        /// </remarks>
        public Window MainWindow
        {
            get
            {
                /// We do not need VerifyAccess here, MainWindow property should be accessible
                /// from any thread, since it could be needed for non-UI related operations.
                /// This is also in-line with desktop CLR behavior, where you can access Form object
                /// itself from any thread, although many actionable items are blocked from
                /// non-UI ones.
                return _mainWindow;
            }

            set
            {
                VerifyAccess();

                if (value != _mainWindow)
                {
                    _mainWindow = value;
                }
            }
        }

        /// <summary>
        ///     The ShutdownMode property is called to set the shutdown specific mode of
        ///     the application. Setting this property controls the way in which an application
        ///     will shutdown.
        ///         The three values for the ShutdownMode enum are :
        ///                 OnLastWindowClose
        ///                 OnMainWindowClose
        ///                 OnExplicitShutdown
        ///
        ///         OnLastWindowClose - this mode will shutdown the application when  the
        ///                             last window is closed, or an explicit call is made
        ///                             to Application.Shutdown(). This is the default mode.
        ///
        ///         OnMainWindowClose - this mode will shutdown the application when the main
        ///                             window has been closed, or Application.Shutdown() is
        ///                             called. Note that if the MainWindow property has not
        ///                             been set - this mode is equivalent to OnExplicitOnly.
        ///
        ///         OnExplicitShutdown- this mode will shutdown the application only when an
        ///                             explicit call to OnShutdown() has been made.
        /// </summary>
        public ShutdownMode ShutdownMode
        {
            get
            {
                return _shutdownMode;
            }

            set
            {
                VerifyAccess();
                if (!IsValidShutdownMode(value))
                {
                    throw new ArgumentOutOfRangeException("value", "enum");
                }

                if (IsShuttingDown == true || _appIsShutdown == true)
                {
                    throw new InvalidOperationException();
                }

                _shutdownMode = value;
            }
        }

        #endregion Public Properties

        //------------------------------------------------------
        //
        //  Public Events
        //
        //------------------------------------------------------

        #region Public Events

        /// <summary>
        ///     The Startup event is fired when an application is starting.
        ///     This event is raised by the OnStartup method.
        /// </summary>
        public event EventHandler Startup
        {
            add { VerifyAccess(); _startupEventHandler += value; }
            remove { VerifyAccess(); _startupEventHandler -= value; }
        }

        /// <summary>
        /// The Exit event is fired when an application is shutting down.
        /// This event is raised by the OnExit method.
        /// </summary>
        public event EventHandler Exit
        {
            add { VerifyAccess(); _exitEventHandler += value; }
            remove { VerifyAccess(); _exitEventHandler -= value; }
        }

#if(FALSE)
        /// <summary>
        /// The activate event is fired when an applications window has been activated from
        /// the OS ( alt-tab, or changing application from taskbar, or clicking on a winodw).
        /// This event is raised by the OnActivate method.
        /// </summary>
        public event EventHandler Activate
        {
            add { VerifyAccess(); }
            remove { VerifyAccess(); }
        }

        /// <summary>
        /// The deactivate event is fired when an applications window has been de-activated
        /// from the OS ( alt-tab, or changing application from taskbar, or clicking away
        /// from an applications window). This event is raised by the OnDeactivate method.
        /// </summary>
        public event EventHandler Deactivate
        {
            add { VerifyAccess();  }
            remove { VerifyAccess(); }
        }

        /// <summary>
        /// The SessionEnding event is fired when windows is ending, either due to a shutdown,
        /// or loggoff from the start menu ( or calling the ExitWindows function).  The
        /// ReasonSessionEnding enum on the  SessionEndingEventArgs indicates whether the session
        /// is ending in response to a shutdown of the OS, or if the user is logging off.
        /// </summary>
        /// <SecurityNote>
        /// By setting cancel to true on the SessionEndingCancelEventArgs, the app can prevent
        /// the user from logging off.  Hence attempting to cancel this is a high trust
        /// operation and this is enforced when the event is handled.  No listener can cancel
        /// this event if the app is partial trust.
        /// </SecurityNote>
        public event SessionEndingCancelEventHandler SessionEnding
        {
            add { VerifyAccess(); }
            remove { VerifyAccess();  }
        }

#endif

        #endregion Public Events

        //------------------------------------------------------
        //
        //  Protected Methods
        //
        //------------------------------------------------------

        #region Protected Methods

        /// <summary>
        ///     OnStartup is called to raise the Startup event. The developer will typically override this method
        ///     if they want to take action at startup time ( or they may choose to attach an event).
        ///     This method will be called once when the application begins, once that application's Run() method
        ///     has been called.
        /// </summary>
        /// <remarks>
        ///     This method follows the .Net programming guideline of having a protected virtual method
        ///     that raises an event, to provide a convenience for developers that subclass the event.
        ///     If you override this method - you need to call Base.OnStartup(...) for the corresponding event
        ///     to be raised.
        /// </remarks>
        /// <param name="e">The event args that will be passed to the Startup event</param>
        protected virtual void OnStartup(EventArgs e)
        {
            // VerifyAccess(); //we're only called via Invoke so this is unnecessary?

            if (_startupEventHandler != null)
            {
                _startupEventHandler(this, e);
            }
        }

        /// <summary>
        ///     OnExit is called to raise the Exit event.
        ///     The developer will typically override this method if they want to take
        ///     action when the application exits  ( or they may choose to attach an event).
        /// </summary>
        /// <remarks>
        ///     This method follows the .Net programming guideline of having a protected virtual method
        ///     that raises an event, to provide a convenience for developers that subclass the event.
        ///     If you override this method - you need to call Base.OnExit(...) for the
        ///     corresponding event to be raised.
        /// </remarks>
        /// <param name="e">The event args that will be passed to the Exit event</param>
        protected virtual void OnExit(EventArgs e)
        {
            // VerifyAccess(); //- Only called via an Invoked frame, so unnecessary?

            if (_exitEventHandler != null)
            {
                _exitEventHandler(this, e);
            }
        }

#if(false)
        // REFACTOR

        /// <summary>
        ///     OnActivate is called to raise the Activate event.
        ///     The developer will typically override this method if they want to take action
        ///     when the application gets activated ( or they may choose to attach an event).
        ///     This method will be called when one of the current applications windows gets
        ///     activated on the desktop. ( This corresponds to Users WM_ACTIVATEAPP message).
        /// </summary>
        /// <remarks>
        ///     This method follows the .Net programming guideline of having a protected
        ///     virtual method that raises an event, to provide a convenience for developers
        ///     that subclass the event.
        /// </remarks>
        /// <param name="e"></param>
        protected virtual void OnActivate(EventArgs e)
        {
            VerifyAccess();

            EventHandler handler = (EventHandler)Events[EVENT_ACTIVATE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///     OnDeactivate is called to raise the Deactivate event. The developer will
        ///     typically override this method if they want to take action when the application
        ///     gets deactivated ( or they may choose to attach an event).
        ///     This method will be called when one of the current applications windows gets
        ///     activated on the desktop. ( This corresponds to Users WM_ACTIVATEAPP message,
        ///     with an wparam indicating the app is being deactivated).
        /// </summary>
        /// <remarks>
        ///     This method follows the .Net programming guideline of having a protected virtual
        ///     method that raises an event, to provide a convenience for developers that
        ///     subclass the event.
        /// </remarks>
        /// <param name="e"></param>
        protected virtual void OnDeactivate(EventArgs e)
        {
            VerifyAccess();

            EventHandler handler = (EventHandler)Events[EVENT_DEACTIVATE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///     OnSessionEnding is called to raise the SessionEnding event. The developer will
        ///     typically override this method if they want to take action when the OS is ending
        ///     a session ( or they may choose to attach an event). This method will be called when
        ///     the user has chosen to either logoff or shutdown. These events are equivalent
        ///     to receiving a WM_QUERYSESSION window event. Windows will send it when user is
        ///     logging out/shutting down. ( See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/wm_queryendsession.asp ).
        ///     By default if this event is not cancelled - Avalon will then call Application.Shutdown.
        /// </summary>
        /// <remarks>
        ///     This method follows the .Net programming guideline of having a protected virtual
        ///     method that raises an event, to provide a convenience for developers that subclass
        ///     the event.
        /// </remarks>
        /// <param name="e"></param>
        protected virtual void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            VerifyAccess();

            SessionEndingCancelEventHandler handler = (SessionEndingCancelEventHandler)Events[EVENT_SESSIONENDING];
            if (handler != null)
            {
                handler(this, e);
            }
        }

#endif

        #endregion Protected Methods

        //------------------------------------------------------
        //
        //  Internal Methods
        //
        //------------------------------------------------------

        #region Internal Methods

        /// <summary>
        /// DO NOT USE - internal method
        /// </summary>
        internal virtual void DoShutdown()
        {
            // VerifyAccess(); - is this really necessary when we're only called via an Invoke?

            // We need to know if we have been shut down already.
            // We cannot check the IsShuttingDown variable because it is set true
            // in the function that calls us.

            lock (typeof(GlobalLock))
            {
                _appWindowList = null;
            }

            EventArgs e = new EventArgs();

            // Event handler exception continuality: if exception occurs in ShuttingDown event handler,
            // our cleanup action is to finish Shuttingdown.  Since Shuttingdown cannot be cancelled.
            // We don't want user to use throw exception and catch it to cancel Shuttingdown.
            try
            {
                // fire Applicaiton Exit event
                OnExit(e);
            }
            finally
            {

                lock (typeof(GlobalLock))
                {
                    _appInstance = null;
                    _nonAppWindowList = null;
                }

                _mainWindow = null;

                // REFACTOR -- disconnect from managed system

                _appIsShutdown = true; // mark app as shutdown
            }
        }

        private object ShowWindow(object obj)
        {
            Window win = obj as Window;
            win.Visibility = Visibility.Visible;
            return null;
        }

        #endregion Internal methods

        //------------------------------------------------------
        //
        //  Internal Properties
        //
        //------------------------------------------------------

        #region Internal Properties

        // The public Windows property returns a copy of the underlying
        // WindowCollection.  This property is used internally to enable
        // modyfying the underlying collection.
        internal WindowCollection WindowsInternal
        {
            get
            {
                lock (typeof(GlobalLock))
                {
                    if (_appWindowList == null)
                    {
                        _appWindowList = new WindowCollection();
                    }

                    return _appWindowList;
                }
            }
        }

        internal WindowCollection NonAppWindowsInternal
        {
            get
            {
                lock (typeof(GlobalLock))
                {
                    if (_nonAppWindowList == null)
                    {
                        _nonAppWindowList = new WindowCollection();
                    }

                    return _nonAppWindowList;
                }
            }

        }

        internal static bool IsShuttingDown
        {
            get
            {
                lock (typeof(GlobalLock))
                {
                    return _isShuttingDown;
                }
            }

            set
            {
                lock (typeof(GlobalLock))
                {
                    _isShuttingDown = value;
                }
            }
        }

        #endregion Internal Properties

        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------
        #region Private Methods

        /*

        /// <summary>
        ///     Creates hwndsource so that we can listen to some window msgs.
        /// </summary>
        private void EnsureHwndSource()
        {
            // We don't support Activate, Deactivate, and SessionEnding
            // events for browser hosted scenarios thus don't create
            // this HwndSource if BrowserCallbackServices is valid
            if (BrowserCallbackServices == null && _parkingHwnd == null)
            {
                // We need these asserts b/c some PTT drts run as standalone
                // apps under limited security permissions.
                //
                HwndSourceParameters param = new HwndSourceParameters("", 0 , 0);
                param.HwndSourceHook = new HwndSourceHook(AppFilterMessage);
                param.WindowStyle = 0;
                param.ExtendedWindowStyle = 0;
                _parkingHwnd = new HwndSource(param);
            }
        }

        private IntPtr AppFilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            IntPtr retInt = IntPtr.Zero;
            switch (msg)
            {
                case NativeMethods.WM_ACTIVATEAPP:
                    handled = WmActivateApp(NativeMethods.IntPtrToInt32(wParam));
                    break;
                case NativeMethods.WM_QUERYENDSESSION :
                    handled = WmQueryEndSession(lParam, ref retInt);
                    break;
                default:
                    handled = false;
                    break;
            }

            return retInt;
        }

        private bool WmActivateApp(Int32 wParam)
        {
            int temp = wParam;
            bool isActivated = (temp == 0? false : true);

            // Event handler exception continuality: if exception occurs in Activate/Deactivate event handlers, our state would not
            // be corrupted because no internal state are affected by Activate/Deactivate. Please check Event handler exception continuality
            // if a state depending on those events is added.
            if (isActivated == true)
            {
                OnActivate( EventArgs.Empty );
            }
            else
            {
                OnDeactivate( EventArgs.Empty );
            }

            return false;
        }

        private bool WmQueryEndSession(IntPtr lParam, ref IntPtr refInt)
        {
            int reason = NativeMethods.IntPtrToInt32(lParam);
            bool retVal = false;

            // Event handler exception continuality: if exception occurs in SessionEnding event handlers, our state would not
            // be corrupted because no internal state are affected by SessionEnding. Please check Event handler exception continuality
            // if a state depending on this event is added.
            SessionEndingCancelEventArgs secEventArgs = new SessionEndingCancelEventArgs( (reason & NativeMethods.ENDSESSION_LOGOFF) != 0? ReasonSessionEnding.Logoff : ReasonSessionEnding.Shutdown );
            OnSessionEnding( secEventArgs );

            // we need this check to decided whether we have handled the WM_QUERYENDSESSION or not
            SessionEndingCancelEventHandler handler = (SessionEndingCancelEventHandler)Events[EVENT_SESSIONENDING];
            if (handler != null)
            {
                // we have handled the event DefWndProc will not be called for this msg
                retVal = true;

                // shut down the app if not cancelled
                if ( secEventArgs.Cancel == false )
                {
                    Shutdown();
                    // return true to the wnd proc to signal that we can terminate properly
                    refInt = new IntPtr(1);
                }
                else
                {
                    // <SecurityNote>
                    // This'll stop a user from Logging off and hence is a high trust operation.
                    // Demand high level of trust.
                    // </SecurityNote>
                    SecurityHelper.DemandUnmanagedCode();
                    refInt = IntPtr.Zero;
                }
            }

            return retVal;
        }

*/

        /// <summary>
        /// This method gets called on dispatch of the Shutdown DispatcherOperationCallback
        /// </summary>

        private object ShutdownCallback(object arg)
        {
            // Event handler exception continuality: if exception occurs in Exit event handler,
            // our cleanup action is to finish Shutdown since Exit cannot be cancelled. We don't
            // want user to use throw exception and catch it to cancel Shutdown.
            try
            {
                DoShutdown();
            }
            finally
            {
                // Quit the dispatcher if we ran our own.
                if (_ownDispatcherStarted == true)
                {
                    Dispatcher.InvokeShutdown();
                }

            }

            return null;
        }

        /// <summary>
        /// This DispatcherException event handler creates the default UI
        /// </summary>
        static private bool DefaultContextExceptionHandler(object sender, Exception e)
        {
            Trace.Print("[Default DispatcherException Handler] Exception caught: " + e.GetType().FullName);

            // what do we want to do when we get an exception? throw up a dialog?

            return true;
        }

        private static bool IsValidShutdownMode(ShutdownMode value)
        {
            return value == ShutdownMode.OnExplicitShutdown
                || value == ShutdownMode.OnLastWindowClose
                || value == ShutdownMode.OnMainWindowClose;
        }

        #endregion Private Methods

        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields
        private class GlobalLock { }
        static private bool _isShuttingDown;
        static private bool _appCreatedInThisAppDomain;
        static private Application _appInstance;

        private WindowCollection _appWindowList;
        private WindowCollection _nonAppWindowList;
        private Window _mainWindow;

        private bool _ownDispatcherStarted;
        private bool _appIsShutdown;
        private ShutdownMode _shutdownMode = ShutdownMode.OnLastWindowClose;

        private EventHandler _startupEventHandler;
        private EventHandler _exitEventHandler;

        private static DispatcherOperationCallback _reportInputMethod;
        private static InputManager _inputManager = null;
        private InputProviderSite _inputProviderSite = null;

        private static int _stylusMaxX = SystemMetrics.ScreenWidth;
        private static int _stylusMaxY = SystemMetrics.ScreenHeight;

        /*REFACTOR        private EventHandlerList            _events;

                private static readonly object EVENT_STARTUP        = new object();
                private static readonly object EVENT_EXIT           = new object();
                private static readonly object EVENT_ACTIVATE       = new object();
                private static readonly object EVENT_DEACTIVATE     = new object();
                private static readonly object EVENT_SESSIONENDING  = new object();
        */
        #endregion Private Fields
    }

    #endregion Application Class

    #region enum ShutdownMode

    /// <summary>
    ///     Enum for ShutdownMode
    /// </summary>
    public enum ShutdownMode : byte
    {
        /// <summary>
        ///
        /// </summary>
        OnLastWindowClose = 0,

        /// <summary>
        ///
        /// </summary>
        OnMainWindowClose = 1,

        /// <summary>
        ///
        /// </summary>
        OnExplicitShutdown

        // NOTE: if you add or remove any values in this enum, be sure to update Application.IsValidShutdownMode()
    }

    #endregion enum ShutdownMode

    #region enum ReasonSessionEnding

    /// <summary>
    ///     Enum for ReasonSessionEnding
    /// </summary>
    public enum ReasonSessionEnding : byte
    {
        /// <summary>
        ///
        /// </summary>
        Logoff = 0,
        /// <summary>
        ///
        /// </summary>
        Shutdown
    }

    #endregion enum ReasonSessionEnding
}


