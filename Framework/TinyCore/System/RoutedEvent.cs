using System;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT
{
    /// <summary>
    ///     RoutedEvent is a unique identifier for
    ///     any registered RoutedEvent
    /// </summary>
    /// <remarks>
    ///     RoutedEvent constitutes the <para/>
    ///     <see cref="RoutedEvent.Name"/>, <para/>
    ///     <see cref="RoutedEvent.RoutingStrategy"/>, <para/>
    ///     <see cref="RoutedEvent.HandlerType"/> and <para/>
    ///     <see cref="RoutedEvent.OwnerType"/> <para/>
    ///     <para/>
    ///
    ///     NOTE: None of the members can be null
    /// </remarks>
    /// <ExternalAPI/>
    public sealed class RoutedEvent
    {
        #region External API

        /// <summary>
        ///     Returns the Name of the RoutedEvent
        /// </summary>
        /// <remarks>
        ///     RoutedEvent Name is unique within the
        ///     OwnerType (super class types not considered
        ///     when talking about uniqueness)
        /// </remarks>
        /// <ExternalAPI/>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Returns the <see cref="RoutingStrategy"/>
        ///     of the RoutedEvent
        /// </summary>
        /// <ExternalAPI/>
        public RoutingStrategy RoutingStrategy
        {
            get { return _routingStrategy; }
        }

        /// <summary>
        ///     Returns Type of Handler for the RoutedEvent
        /// </summary>
        /// <remarks>
        ///     HandlerType is a type of delegate
        /// </remarks>
        /// <ExternalAPI/>
        public Type HandlerType
        {
            get { return _handlerType; }
        }

        /// <summary>
        ///    String representation
        /// </summary>
        public override string ToString()
        {
            return _name;
        }

        #endregion External API

        #region Construction

        /// <summary>
        /// Create a new routed event.
        ///
        /// You have to promise not to duplicate another event name in the system,
        /// or you will be sorry.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="routingStrategy"></param>
        /// <param name="handlerType"></param>
        public RoutedEvent(
            string name,
            RoutingStrategy routingStrategy,
            Type handlerType
            )
        {
            _name = name;
            _routingStrategy = routingStrategy;
            _handlerType = handlerType;
            lock (typeof(GlobalLock))
            {
                if (_eventCount >= Int32.MaxValue)
                {
                    throw new InvalidOperationException("too many events");
                }

                _globalIndex = _eventCount++;
            }
        }

        /// <summary>
        ///    Index for this event
        /// </summary>
        internal int GlobalIndex
        {
            get { return _globalIndex; }
        }

        #endregion Construction

        #region Data

        private string _name;      // do we need this ? we will incur some dumb strings.
        internal RoutingStrategy _routingStrategy;
        private Type _handlerType;
        private int _globalIndex;
        static int _eventCount;

        private class GlobalLock { }

        #endregion Data
    }
}


