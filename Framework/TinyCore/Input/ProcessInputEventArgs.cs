
using System;
using System.Collections;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Input
{

    /* REFACTOR

        Consider if we can wipe this wrapper, and just allow access to the input manager..
        this enforces a contract when processing input, which is nice, but I hate wrappers.
    */

    /// <summary>
    ///     Provides access to the input manager's staging area.
    /// </summary>
    /// <remarks>
    ///     An instance of this class, or a derived class, is passed to the
    ///     handlers of the following events:
    ///     <list>
    ///         <item>
    ///             <see cref="InputManager.PreProcessInput"/>
    ///         </item>
    ///         <item>
    ///             <see cref="InputManager.PostProcessInput"/>
    ///         </item>
    ///     </list>
    /// </remarks>
    public class ProcessInputEventArgs : NotifyInputEventArgs
    {
        // Only we can make these.
        internal ProcessInputEventArgs(StagingAreaInputItem input)
            : base(input)
        {
        }

        /// <summary>
        ///     Pushes an input event onto the top of the staging area.
        /// </summary>
        /// <param name="input">
        ///     The input event to place on the staging area.  This may not
        ///     be null, and may not already exist in the staging area.
        /// </param>
        /// <param name="promote">
        ///     An existing staging area item to promote the state from.
        /// </param>
        /// <returns>
        ///     The staging area input item that wraps the specified input.
        /// </returns>
        public StagingAreaInputItem PushInput(InputEventArgs input,
                                              StagingAreaInputItem promote) // Note: this should be a bool, and always use the InputItem available on these args.
        {
            StagingAreaInputItem item = new StagingAreaInputItem(input, promote);

            InputManager.CurrentInputManager._currentStagingStack.Push(item);
            
            return item;
        }

        /// <summary>
        ///     Pushes an input event onto the top of the staging area.
        /// </summary>
        /// <param name="input">
        ///     The input event to place on the staging area.  This may not
        ///     be null, and may not already exist in the staging area.
        /// </param>
        /// <returns>
        ///     The specified staging area input item.
        /// </returns>
        public StagingAreaInputItem PushInput(StagingAreaInputItem input)
        {
            InputManager.CurrentInputManager._currentStagingStack.Push(input);
            
            return input;
        }

        /// <summary>
        ///     Pops off the input event on the top of the staging area.
        /// </summary>
        /// <returns>
        ///     The input event that was on the top of the staging area.
        ///     This can be null, if the staging area was empty.
        /// </returns>
        public StagingAreaInputItem PopInput()
        {
            Stack stagingArea = InputManager.CurrentInputManager._currentStagingStack;
            
            if (stagingArea.Count > 0)
            {
                return (StagingAreaInputItem)stagingArea.Pop();
            }

            return null;
        }

        /// <summary>
        ///     Returns the input event on the top of the staging area.
        /// </summary>
        /// <returns>
        ///     The input event that is on the top of the staging area.
        ///     This can be null, if the staging area is empty.
        /// </returns>
        public StagingAreaInputItem PeekInput()
        {
            Stack stagingArea = InputManager.CurrentInputManager._currentStagingStack;
            
            if (stagingArea.Count > 0)
            {
                return (StagingAreaInputItem)stagingArea.Peek();
            }

            return null;
        }
    }
}


