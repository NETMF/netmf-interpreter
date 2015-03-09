////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network
{
    /// <summary>
    /// This class stores the context of a message we've sent to Mac and awaiting response.
    /// </summary>
    public class MessageContext
    {
        // Mac sdu handle is a byte, so we limit ourself to 256 pending messages
        internal const int cHandleDontCare = 255; // used for messages without send context
        private const int cMaxNrHandles = 32; // 0..31
        private object _lock;
        private byte _nextHandle; // id of the next mac sdu handle
        private ushort _usedHandles; // number of handles in use
        private Context[] _context;

        public MessageContext()
        {
            _lock = new object();
            _nextHandle = 0;
            _usedHandles = 0;
            _context = new Context[cMaxNrHandles];
            for (int i = 0; i < cMaxNrHandles; i++)
                _context[i] = new Context((byte)i);
        }

        /// <summary>
        /// Get free context from pool
        /// </summary>
        /// <returns>Allocated context or null on failure</returns>
        public Context GetFreeContext()
        {
            lock (_lock)
            {
                if (_usedHandles == cMaxNrHandles)
                    return null;

                // get next free context. normally messages are processed in-order
                // and there is no iteration. needed to handle out-of-order events
                int i = _nextHandle;
                int n = 0;
                while (_context[i].inUse && n < cMaxNrHandles)
                {
                    n++;
                    i = (i + 1) % cMaxNrHandles;
                }

                if (n == cMaxNrHandles)
                    throw new SystemException("corrupted state: all contexts in use");

                Context context = _context[i];

                context.inUse = true;
                _usedHandles++;
                // in case of out-of-order event, let's try the expected free value again next time
                if (i == _nextHandle)
                    _nextHandle = (byte)((_nextHandle + 1) % cMaxNrHandles);

                return context;
            }
        }

        /// <summary>
        /// Get specific context from pool
        /// </summary>
        /// <param name="msduHandle"></param>
        /// <returns>Requested context or null on failure</returns>
        public Context GetContext(byte msduHandle)
        {
            if (msduHandle > cMaxNrHandles - 1)
                return null;

            lock (_lock)
            {
                Context context = _context[msduHandle];
                if (context.inUse)
                    return context;
                return null;
            }
        }

        /// <summary>
        /// Release a previously allocated context and return to pool
        /// </summary>
        /// <param name="context">context to release</param>
        public void ReleaseContext(Context context)
        {
            lock (_lock)
            {
                if (context != _context[context.macSduHandle])
                    throw new System.ArgumentException("invalid context");

                if (!context.inUse || _usedHandles == 0)
                    throw new SystemException("corrupted state");

                context.inUse = false;
                context.useExtAddr = false;
                context.nextHopExt = 0;
                context.nextHopShort = 0;
                context.dataSduHandle = 0;
                context.dataHandler = null; // clear reference
                _usedHandles--;
            }
        }

        /// <summary>
        /// This class represents a single context for a Mac data request
        /// </summary>
        public class Context // reference-type, so array references only physical copy
        {
            public Context(byte _macSduHandle)
            {
                macSduHandle = _macSduHandle;
            }

            // set by MessageContext:
            public readonly byte macSduHandle; // the corresponding macSduHandle
            public bool inUse; // true if this context is in use, e.g. has outstanding mac confirmation. set by MessageContext

            // set by "user":
            public bool useExtAddr;
            public UInt16 nextHopShort; // can be broadcast
            public UInt64 nextHopExt;

            // for sdu only:
            public Byte dataSduHandle;
            public DataConfirmHandler dataHandler;
        }

    }
}


