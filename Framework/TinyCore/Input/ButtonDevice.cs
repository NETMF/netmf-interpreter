////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define TRACK_BUTTON_STATE

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;

namespace Microsoft.SPOT.Input
{
    /// <summary>
    ///     The ButtonDevice class represents the button device to the
    ///     members of a context.
    /// </summary>
    public sealed class ButtonDevice : InputDevice
    {
        internal ButtonDevice(InputManager inputManager)
        {
            _inputManager = inputManager;

            _inputManager.InputDeviceEvents[(int)InputManager.InputDeviceType.Button].PreNotifyInput += new NotifyInputEventHandler(PreNotifyInput);
            _inputManager.InputDeviceEvents[(int)InputManager.InputDeviceType.Button].PostProcessInput += new ProcessInputEventHandler(PostProcessInput);

            _isEnabledOrVisibleChangedEventHandler = new PropertyChangedEventHandler(OnIsEnabledOrVisibleChanged);
        }

        /// <summary>
        ///     Returns the element that input from this device is sent to.
        /// </summary>
        public override UIElement Target
        {
            get
            {
                return _focus;
            }
        }

        public override InputManager.InputDeviceType DeviceType
        {
            get
            {
                return InputManager.InputDeviceType.Button;
            }
        }

        /// <summary>
        ///     Focuses the button input on a particular element.
        /// </summary>
        /// <param name="element">
        ///     The element to focus the button pad on.
        /// </param>
        /// <returns>Element focused to</returns>
        public UIElement Focus(UIElement obj)
        {
            VerifyAccess();

            bool forceToNullIfFailed = false;

            // Make sure that the element is enabled.  This includes all parents.
            bool enabled = true;
            bool visible = true;
            if (obj != null)
            {
                enabled = obj.IsEnabled;
                visible = obj.IsVisible;

                if ((enabled && visible) && forceToNullIfFailed)
                {
                    obj = null;
                    enabled = true;
                    visible = true;
                }
            }

            if ((enabled && visible) && obj != _focus)
            {
                // go ahead and change our internal sense of focus to the desired element.
                ChangeFocus(obj, DateTime.UtcNow);
            }

            return _focus;
        }

        /// <summary>
        ///     Returns whether or not the specified button is down.
        /// </summary>
        public bool IsButtonDown(Button button)
        {
            return GetButtonState(button) == ButtonState.Down;
        }

        /// <summary>
        ///     Returns whether or not the specified button is up.
        /// </summary>
        public bool IsButtonUp(Button button)
        {
            return GetButtonState(button) == ButtonState.None;
        }

        /// <summary>
        ///     Returns whether or not the specified button is held.
        /// </summary>
        public bool IsButtonHeld(Button button)
        {
            return GetButtonState(button) == ButtonState.Held;
        }

        /// <summary>
        ///     Returns the state of the specified button.
        /// </summary>
        public ButtonState GetButtonState(Button button)
        {
#if TRACK_BUTTON_STATE

            if ((int)Button.LastSystemDefinedButton <= (int)button || (int)button <= 0)
                throw new ArgumentOutOfRangeException("button", "invalid enum");

            int index = (int)button / 4;
            int state = (_buttonState[index] >> ((int)button % 4)) & 0x3;

            return (ButtonState)state;
#else
            return ButtonState.None;
#endif
        }

#if TRACK_BUTTON_STATE
        internal void SetButtonState(Button button, ButtonState state)
        {
            //If the PreNotifyInput event sent by the InputManager is always sent by the
            //correct thread, this is redundant. Also, why is this function 'internal'
            //when we only access it from inside this class?
            VerifyAccess();

            if ((int)Button.LastSystemDefinedButton <= (int)button || (int)button <= 0)
                throw new ArgumentOutOfRangeException("button", "invalid enum");

            int index = (int)button / 4;
            int shift = ((int)button % 4);

            byte newState = _buttonState[index];

            newState &= (byte)(~((byte)(0x3 << shift)));
            newState |= (byte)((int)state << shift);

            _buttonState[index] = newState;
        }

#endif

        private void ChangeFocus(UIElement focus, DateTime timestamp)
        {
            if (focus != _focus)
            {
                // Update the critical pieces of data.
                UIElement oldFocus = _focus;
                _focus = focus;
                _focusRootUIElement = focus != null ? focus.RootUIElement : null;

                // Adjust the handlers we use to track everything.
                if (oldFocus != null)
                {
                    oldFocus.IsEnabledChanged -= _isEnabledOrVisibleChangedEventHandler;
                    oldFocus.IsVisibleChanged -= _isEnabledOrVisibleChangedEventHandler;
                }

                if (focus != null)
                {
                    focus.IsEnabledChanged += _isEnabledOrVisibleChangedEventHandler;
                    focus.IsVisibleChanged += _isEnabledOrVisibleChangedEventHandler;
                }

                // Send the LostFocus and GotFocus events.
                if (oldFocus != null)
                {
                    FocusChangedEventArgs lostFocus = new FocusChangedEventArgs(this, timestamp, oldFocus, focus);
                    lostFocus.RoutedEvent = Buttons.LostFocusEvent;
                    lostFocus.Source = oldFocus;

                    _inputManager.ProcessInput(lostFocus);
                }

                if (focus != null)
                {
                    FocusChangedEventArgs gotFocus = new FocusChangedEventArgs(this, timestamp, oldFocus, focus);
                    gotFocus.RoutedEvent = Buttons.GotFocusEvent;
                    gotFocus.Source = focus;

                    _inputManager.ProcessInput(gotFocus);
                }
            }
        }

        private void OnIsEnabledOrVisibleChanged(object sender, PropertyChangedEventArgs e)
        {
            // The element with focus just became disabled or non-visible
            //
            // We can't leave focus on a disabled element, so move it.
            //
            // Will need to change this for watch, but this solution is for aux now.

            Focus(_focus.Parent);
        }

        private void PreNotifyInput(object sender, NotifyInputEventArgs e)
        {
            RawButtonInputReport buttonInput = ExtractRawButtonInputReport(e, InputManager.PreviewInputReportEvent);
            if (buttonInput != null)
            {
                CheckForDisconnectedFocus();
                /*

REFACTOR --

                the keyboard device is only active per app domain basis -- so like if your app domain doesn't have
                focus your keyboard device is not going to give you the real state of the keyboard.

                When it gets focus, it needs to know about this somehow.   We could use this keyboard action
                type stuff to do so.  Though this design really seem to be influenced a lot from living in
                the windows world.

                Essentially the input stack is being used like a message pump to say, hey dude you can
                use the keyboard now -- it's not real input, it's more or less a message.

                It could be interesting for elements to know about this -- since I think
                they will probalby still have focus (or do they get a Got and Lost Focus when the keyboard activates -- I don't think so,
                we need to know what we were focused on when the window gets focus again.

                So maybe elements want to stop some animation or something when input focus moves away from the activesource, and
                start them again later.  Could be interesting.
*/

                if ((buttonInput.Actions & RawButtonActions.Activate) == RawButtonActions.Activate)
                {
                    //System.Console.WriteLine("Initializing the button state.");

#if TRACK_BUTTON_STATE
                    // Clear out our key state storage.
                    for (int i = 0; i < _buttonState.Length; i++)
                    {
                        _buttonState[i] = 0;
                    }

#endif
                    // we are now active.
                    // we should track which source is active so we don't confuse deactivations.
                    _isActive = true;
                }

                // Generally, we need to check against redundant actions.
                // We never prevet the raw event from going through, but we
                // will only generate the high-level events for non-redundant
                // actions.  We store the set of non-redundant actions in
                // the dictionary of this event.

                // If the input is reporting a button down, the action is never
                // considered redundant.
                if ((buttonInput.Actions & RawButtonActions.ButtonDown) == RawButtonActions.ButtonDown)
                {
                    RawButtonActions actions = GetNonRedundantActions(e);
                    actions |= RawButtonActions.ButtonDown;
                    e.StagingItem.SetData(_tagNonRedundantActions, actions);

                    // Pass along the button that was pressed, and update our state.
                    e.StagingItem.SetData(_tagButton, buttonInput.Button);

#if TRACK_BUTTON_STATE
                    ButtonState buttonState = GetButtonState(buttonInput.Button);

                    if ((buttonState & ButtonState.Down) == ButtonState.Down)
                    {
                        buttonState = ButtonState.Down | ButtonState.Held;
                    }
                    else
                    {
                        buttonState |= ButtonState.Down;
                    }

                    SetButtonState(buttonInput.Button, buttonState);
#endif

                    // Tell the InputManager that the MostRecentDevice is us.
                    if (_inputManager != null && _inputManager.MostRecentInputDevice != this)
                    {
                        _inputManager.MostRecentInputDevice = (InputDevice)this;
                    }
                }

                // need to detect redundant ups.
                if ((buttonInput.Actions & RawButtonActions.ButtonUp) == RawButtonActions.ButtonUp)
                {
                    RawButtonActions actions = GetNonRedundantActions(e);
                    actions |= RawButtonActions.ButtonUp;
                    e.StagingItem.SetData(_tagNonRedundantActions, actions);

                    // Pass along the button that was pressed, and update our state.
                    e.StagingItem.SetData(_tagButton, buttonInput.Button);

#if TRACK_BUTTON_STATE
                    ButtonState buttonState = GetButtonState(buttonInput.Button);

                    if ((buttonState & ButtonState.Down) == ButtonState.Down)
                    {
                        buttonState &= (~ButtonState.Down) & (ButtonState.Down | ButtonState.Held);
                    }
                    else
                    {
                        buttonState |= ButtonState.Held;
                    }

                    SetButtonState(buttonInput.Button, buttonState);
#endif

                    // Tell the InputManager that the MostRecentDevice is us.
                    if (_inputManager != null && _inputManager.MostRecentInputDevice != this)
                    {
                        _inputManager.MostRecentInputDevice = (InputDevice)this;
                    }
                }
            }

            // On ButtonDown, we might need to set the Repeat flag

            if (e.StagingItem.Input.RoutedEvent == Buttons.PreviewButtonDownEvent)
            {
                CheckForDisconnectedFocus();

                ButtonEventArgs args = (ButtonEventArgs)e.StagingItem.Input;

                // Is this the same as the previous button?
                if (_previousButton == args.Button)
                {
                    // Yes, this is a repeat (we got the buttondown for it twice, with no ButtonUp in between)
                    // what about chording?
                    args._isRepeat = true;
                }

                // Otherwise, keep this button to check against next time.
                else
                {
                    _previousButton = args.Button;
                    args._isRepeat = false;
                }

            }

            // On ButtonUp, we clear Repeat flag
            else if (e.StagingItem.Input.RoutedEvent == Buttons.PreviewButtonUpEvent)
            {
                CheckForDisconnectedFocus();

                ButtonEventArgs args = (ButtonEventArgs)e.StagingItem.Input;
                args._isRepeat = false;

                // Clear _previousButton, so that down/up/down/up doesn't look like a repeat
                _previousButton = Button.None;

            }
        }

        private void PostProcessInput(object sender, ProcessInputEventArgs e)
        {
            // PreviewButtonDown --> ButtonDown
            if (e.StagingItem.Input.RoutedEvent == Buttons.PreviewButtonDownEvent)
            {
                CheckForDisconnectedFocus();

                if (!e.StagingItem.Input.Handled)
                {
                    ButtonEventArgs previewButtonDown = (ButtonEventArgs)e.StagingItem.Input;
                    ButtonEventArgs buttonDown = new ButtonEventArgs(this, previewButtonDown.InputSource, previewButtonDown.Timestamp, previewButtonDown.Button);

                    buttonDown._isRepeat = previewButtonDown.IsRepeat;
                    buttonDown.RoutedEvent = Buttons.ButtonDownEvent;

                    e.PushInput(buttonDown, e.StagingItem);
                }
            }

            // PreviewButtonUp --> ButtonUp
            if (e.StagingItem.Input.RoutedEvent == Buttons.PreviewButtonUpEvent)
            {
                CheckForDisconnectedFocus();

                if (!e.StagingItem.Input.Handled)
                {
                    ButtonEventArgs previewButtonUp = (ButtonEventArgs)e.StagingItem.Input;

                    ButtonEventArgs buttonUp = new ButtonEventArgs(this, previewButtonUp.InputSource, previewButtonUp.Timestamp, previewButtonUp.Button);

                    buttonUp.RoutedEvent = Buttons.ButtonUpEvent;

                    e.PushInput(buttonUp, e.StagingItem);
                }
            }

            RawButtonInputReport buttonInput = ExtractRawButtonInputReport(e, InputManager.InputReportEvent);
            if (buttonInput != null)
            {
                CheckForDisconnectedFocus();

                if (!e.StagingItem.Input.Handled)
                {
                    // In general, this is where we promote the non-redundant
                    // reported actions to our premier events.
                    RawButtonActions actions = GetNonRedundantActions(e);

                    // Raw --> PreviewButtonDown
                    if ((actions & RawButtonActions.ButtonDown) == RawButtonActions.ButtonDown)
                    {
                        Button button = (Button)e.StagingItem.GetData(_tagButton);
                        if (button != Button.None)
                        {
                            ButtonEventArgs previewButtonDown = new ButtonEventArgs(this, buttonInput.InputSource, buttonInput.Timestamp, button);
                            previewButtonDown.RoutedEvent = Buttons.PreviewButtonDownEvent;
                            e.PushInput(previewButtonDown, e.StagingItem);
                        }
                    }

                    // Raw --> PreviewButtonUp
                    if ((actions & RawButtonActions.ButtonUp) == RawButtonActions.ButtonUp)
                    {
                        Button button = (Button)e.StagingItem.GetData(_tagButton);
                        if (button != Button.None)
                        {
                            ButtonEventArgs previewButtonUp = new ButtonEventArgs(this, buttonInput.InputSource, buttonInput.Timestamp, button);
                            previewButtonUp.RoutedEvent = Buttons.PreviewButtonUpEvent;
                            e.PushInput(previewButtonUp, e.StagingItem);
                        }
                    }
                }

                // Deactivate
                if ((buttonInput.Actions & RawButtonActions.Deactivate) == RawButtonActions.Deactivate)
                {
                    if (_isActive)
                    {
                        _isActive = false;

                        // Even if handled, a button deactivate results in a lost focus.
                        ChangeFocus(null, e.StagingItem.Input.Timestamp);
                    }
                }
            }
        }

        private RawButtonActions GetNonRedundantActions(NotifyInputEventArgs e)
        {
            RawButtonActions actions;

            // The CLR throws a null-ref exception if it tries to unbox a
            // null.  So we have to special case that.
            object o = e.StagingItem.GetData(_tagNonRedundantActions);
            if (o != null)
            {
                actions = (RawButtonActions)o;
            }
            else
            {
                actions = new RawButtonActions();
            }

            return actions;
        }

        // at the moment we don't have a good way of detecting when an
        // element gets deleted from the tree (logical or visual).  The
        // best we can do right now is clear out the focus if we detect
        // that the tree containing the focus was disconnected.
        private bool CheckForDisconnectedFocus()
        {
            bool wasDisconnected = false;

            if (_focus != null && _focus.RootUIElement != _focusRootUIElement)
            {
                wasDisconnected = true;

                // need to remove this for the watch, placed here for aux now.
                Focus(_focusRootUIElement);
            }

            return wasDisconnected;
        }

        private RawButtonInputReport ExtractRawButtonInputReport(NotifyInputEventArgs e, RoutedEvent Event)
        {
            RawButtonInputReport buttonInput = null;

            InputReportEventArgs input = e.StagingItem.Input as InputReportEventArgs;
            if (input != null)
            {
                if (input.Report is RawButtonInputReport && input.RoutedEvent == Event)
                {
                    buttonInput = (RawButtonInputReport)input.Report;
                }
            }

            return buttonInput;
        }

        private InputManager _inputManager;
        private bool _isActive;

        private UIElement _focus;
        private UIElement _focusRootUIElement;
        private Button _previousButton;

        private PropertyChangedEventHandler _isEnabledOrVisibleChangedEventHandler;

#if TRACK_BUTTON_STATE
        // Device state we track
        private byte[] _buttonState = new byte[(int)Button.LastSystemDefinedButton / 4];
#endif

        // Data tags for information we pass around the staging area.
        private object _tagNonRedundantActions = new object();
        private object _tagButton = new object();
    }
}


