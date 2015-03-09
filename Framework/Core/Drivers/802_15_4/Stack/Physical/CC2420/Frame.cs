////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#if MICROFRAMEWORK
#define MICROFRAMEWORK

#if MICROFRAMEWORK
#else
#define USE_POOL
#endif

using System;
using System.Collections;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4
{

    public sealed class Frame
    {
        #region Frame: core
        public byte[] buf;     // the actual memory buffer
        private int lenOffset;  // start of actual data in buffer (==size of reserved head room)
        public int lenData;    // current amount of data
        private bool released; // used to ensure that a released frame is not longer used.

        /// <summary>
        /// DO NOT USE! Use GetFrame instead! Public for special tests
        /// </summary>
        /// <param name="size">size of internal buffer</param>
        public Frame(int size)
        {
            buf = new byte[size];
            released = false;
        }

        /// <summary>
        /// total size of buffer in bytes
        /// </summary>
        public int LengthBuffer
        {
            get { return buf.Length; }
        }

        /// <summary>
        /// amount of allocated buffer in bytes
        /// </summary>
        public int LengthDataUsed
        {
            get { return lenData; }
        }

        /// <summary>
        /// amount of available buffer at back of frame
        /// </summary>
        public int LengthDataAvail
        {
            get { return buf.Length - lenOffset - lenData; }
        }

        /// <summary>
        /// amount of available buffer at front of frame
        /// </summary>
        public int LengthHeaderAvail
        {
            get { return lenOffset; }
        }

        #endregion

        #region Frame: accessors
        /// <summary>
        /// Clear buffer and reserve some header space
        /// </summary>
        /// <param name="len">amount of bytes to reserve for the header</param>
        public void ReserveHeader(int len)
        {
            lenOffset = len;
            lenData = 0;
        }

        /// <summary>
        /// Append to front of this frame
        /// </summary>
        /// <param name="byteArray">source buffer</param>
        /// <param name="index">index into source buffer, starting at 0</param>
        /// <param name="nbBytes">amount of data</param>
        public void AppendToFront(byte[] byteArray, int index, int nbBytes)
        {
            SanityCheck(false);

            if (nbBytes > lenOffset)
                throw new System.ArgumentOutOfRangeException();

            lenData += nbBytes;
            lenOffset -= nbBytes;

            System.Array.Copy(byteArray, index, buf, lenOffset, nbBytes);
        }

        private void SanityCheck(bool expectedReleasedValue)
        {
            if (released != expectedReleasedValue)
                throw new System.ApplicationException("Frame release issue.");

        }

        /// <summary>
        /// Append to back of this frame
        /// </summary>
        /// <param name="byteArray">source buffer</param>
        /// <param name="index">index into source buffer, starting at 0</param>
        /// <param name="nbBytes">amount of data</param>
        public void AppendToBack(byte[] byteArray, int index, int nbBytes)
        {
            SanityCheck(false);

            int avail = buf.Length - lenOffset - lenData;
            if (nbBytes > avail)
                throw new System.ArgumentOutOfRangeException();

            System.Array.Copy(byteArray, index, buf, lenOffset + lenData, nbBytes);
            lenData += nbBytes;
        }

        /// <summary>
        /// Delete bytes at the front of the frame
        /// </summary>
        /// <param name="nbBytes">amount of bytes to delete</param>
        public void DeleteFromFront(int nbBytes)
        {
            SanityCheck(false);

            if (nbBytes > lenData)
                throw new System.ArgumentOutOfRangeException();
            lenData -= nbBytes;
            lenOffset += nbBytes;
        }

        /// <summary>
        /// Delete bytes at the back of the frame
        /// </summary>
        /// <param name="nbBytes">amount of bytes to delete</param>
        public void DeleteFromBack(int nbBytes)
        {
            SanityCheck(false);

            if (nbBytes > lenData)
                throw new System.ArgumentOutOfRangeException();
            lenData -= nbBytes;
        }

        /// <summary>
        /// Read bytes from array and copy into given buffer
        /// </summary>
        /// <param name="byteArray">buffer to copy data to</param>
        /// <param name="indexArray">write index into byteArray</param>
        /// <param name="indexFrame">read index into frame</param>
        /// <param name="nbBytes">amount of bytes to copy</param>
        public void ReadBytes(byte[] byteArray, int indexArray, int indexFrame, int nbBytes)
        {
            SanityCheck(false);

            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();
            System.Array.Copy(buf, lenOffset + indexFrame, byteArray, indexArray, nbBytes);
        }

        #endregion

        #region Frame: auxiliary functions
        /// <summary>
        /// Increase data area by 'nbBytes' byte at the front
        /// </summary>
        public void AllocFront(int nbBytes)
        {
            SanityCheck(false);

            if (nbBytes > lenOffset)
                throw new System.ArgumentOutOfRangeException();

            lenData += nbBytes;
            lenOffset -= nbBytes;
        }

        /// <summary>
        /// Increase data area at the back
        /// </summary>
        /// <param name="nbBytes">amount of bytes to add</param>
        public void AllocBack(int nbBytes)
        {
            SanityCheck(false);

            int avail = buf.Length - lenOffset - lenData;
            if (nbBytes > avail)
                throw new System.ArgumentOutOfRangeException();
            lenData += nbBytes;
        }

        /// <summary>
        /// Read a byte
        /// </summary>
        /// <param name="indexFrame">index</param>
        /// <returns>the byte read</returns>
        public byte ReadByte(int indexFrame)
        {
            SanityCheck(false);

            int nbBytes = 1;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();
            return buf[lenOffset + indexFrame];
        }

        /// <summary>
        /// Read a UInt16 in network order (big-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <returns>the UInt16 read</returns>
        public UInt16 ReadUInt16(int indexFrame)
        {
            SanityCheck(false);

            int nbBytes = 2;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();
            indexFrame += lenOffset;
            return (UInt16)(((buf[indexFrame]) << 8) | (buf[indexFrame + 1]));
        }

        /// <summary>
        /// Read a UInt32 in network order (big-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <returns>the UInt32 read</returns>
        public UInt32 ReadUInt32(int indexFrame)
        {
            SanityCheck(false);

            int nbBytes = 4;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();
            indexFrame += lenOffset;
            UInt32 res = 0;
            for (int i = 0; i < nbBytes; i++)
                res = (res << 8) | (UInt32)(buf[indexFrame + i]);
            return res;
        }

        /// <summary>
        /// Read a UInt64 in network order (big-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <returns>the UInt64 read</returns>
        public UInt64 ReadUInt64(int indexFrame)
        {
            SanityCheck(false);

            int nbBytes = 8;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();
            indexFrame += lenOffset;
            UInt64 res = 0;
            for (int i = 0; i < nbBytes; i++)
                res = (res << 8) | (UInt64)(buf[indexFrame + i]);
            return res;
        }

        /// <summary>
        /// Read a UInt16 in canonical order (little-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <returns>the UInt16 read</returns>
        public UInt16 ReadUInt16Canonical(int indexFrame)
        {
            SanityCheck(false);

            int nbBytes = 2;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();
            indexFrame += lenOffset;
            return (UInt16)(((buf[indexFrame])) | (buf[indexFrame + 1] << 8));
        }

        /// <summary>
        /// Read a UInt32 in canonical order (little-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <returns>the UInt32 read</returns>
        public UInt32 ReadUInt32Canonical(int indexFrame)
        {
            SanityCheck(false);

            int nbBytes = 4;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();
            indexFrame += lenOffset;
            UInt32 res = 0;
            for (int i = nbBytes; --i >= 0; )
                res = (res << 8) | (UInt32)(buf[indexFrame + i]);
            return res;
        }

        /// <summary>
        /// Read a UInt64 in canonical order (little-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <returns>the UInt64 read</returns>
        public UInt64 ReadUInt64Canonical(int indexFrame)
        {
            SanityCheck(false);

            int nbBytes = 8;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();
            indexFrame += lenOffset;
            UInt64 res = 0;
            for (int i = nbBytes; --i >= 0; )
                res = (res << 8) | (UInt64)(buf[indexFrame + i]);
            return res;
        }

        /// <summary>
        /// Write a byte into the frame
        /// </summary>
        /// <param name="indexFrame">index</param>
        /// <param name="val">the byte to write</param>
        public void Write(int indexFrame, byte val)
        {
            SanityCheck(false);

            int nbBytes = 1;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();
            buf[lenOffset + indexFrame] = val;
        }

        /// <summary>
        /// Write a UInt16 into the frame in network order (big-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <param name="val">the UInt16 to write</param>
        public void Write(int indexFrame, UInt16 val)
        {
            SanityCheck(false);

            int nbBytes = 2;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();

            indexFrame += lenOffset;
            buf[indexFrame] = (byte)(val >> 8);
            buf[indexFrame + 1] = (byte)(val & 0xFF);
        }

        /// <summary>
        /// Write a UInt32 into the frame in network order (big-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <param name="val">the UInt32 to write</param>
        public void Write(int indexFrame, UInt32 val)
        {
            SanityCheck(false);

            int nbBytes = 4;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();

            indexFrame += lenOffset;
            for (int i = nbBytes; --i >= 0; )
            {
                buf[indexFrame + i] = (byte)(val & 0xFF);
                val = val >> 8;
            }
        }

        /// <summary>
        /// Write a UInt64 into the frame in network order (big-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <param name="val">the UInt64 to write</param>
        public void Write(int indexFrame, UInt64 val)
        {
            SanityCheck(false);

            int nbBytes = 8;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();

            indexFrame += lenOffset;
            for (int i = nbBytes; --i >= 0; )
            {
                buf[indexFrame + i] = (byte)(val & 0xFF);
                val = val >> 8;
            }
        }

        /// <summary>
        /// Write a UInt16 into the frame in canonical order (little-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <param name="val">the UInt16 to write</param>
        public void WriteCanonical(int indexFrame, UInt16 val)
        {
            SanityCheck(false);

            int nbBytes = 2;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();

            indexFrame += lenOffset;
            buf[indexFrame] = (byte)(val & 0xFF);
            buf[indexFrame + 1] = (byte)(val >> 8);
        }

        /// <summary>
        /// Write a UInt32 into the frame in canonical order (little-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <param name="val">the UInt32 to write</param>
        public void WriteCanonical(int indexFrame, UInt32 val)
        {
            SanityCheck(false);

            int nbBytes = 4;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();

            indexFrame += lenOffset;
            for (int i = 0; i < nbBytes; i++)
            {
                buf[indexFrame + i] = (byte)(val & 0xFF);
                val = val >> 8;
            }
        }

        /// <summary>
        /// Write a UInt64 into the frame in canonical order (little-endian)
        /// </summary>
        /// <param name="indexFrame">start index</param>
        /// <param name="val">the UInt64 to write</param>
        public void WriteCanonical(int indexFrame, UInt64 val)
        {
            SanityCheck(false);

            int nbBytes = 8;
            if (nbBytes + indexFrame > lenData)
                throw new System.ArgumentOutOfRangeException();

            indexFrame += lenOffset;
            for (int i = 0; i < nbBytes; i++)
            {
                buf[indexFrame + i] = (byte)(val & 0xFF);
                val = val >> 8;
            }
        }

        /// <summary>
        /// Append part of frame to front of this frame
        /// </summary>
        /// <param name="srcFrame">the frame to append</param>
        /// <param name="index">index into srcFrame</param>
        /// <param name="nbBytes">amount of bytes</param>
        public void AppendToFront(Frame srcFrame, int index, int nbBytes)
        {
            AppendToFront(srcFrame.buf, srcFrame.lenOffset + index, nbBytes);
        }

        /// <summary>
        /// Append part of frame to back of this frame
        /// </summary>
        /// <param name="srcFrame">the frame to append</param>
        /// <param name="index">index into srcFrame</param>
        /// <param name="nbBytes">amount of bytes</param>
        public void AppendToBack(Frame srcFrame, int index, int nbBytes)
        {
            AppendToBack(srcFrame.buf, srcFrame.lenOffset + index, nbBytes);
        }

        /// <summary>
        /// Write a byte array into this frame
        /// </summary>
        /// <param name="indexFrame">index in frame</param>
        /// <param name="b">byte array to write</param>
        public void Write(int indexFrame, byte[] b)
        {
            SanityCheck(false);

            if (indexFrame + b.Length > buf.Length)
                throw new System.ArgumentOutOfRangeException();

            System.Array.Copy(b, 0, buf, lenOffset + indexFrame, b.Length);
        }

        /// <summary>
        /// Copy part of frame into another frame
        /// </summary>
        /// <param name="toFrame">frame to write to</param>
        /// <param name="indexToFrame">index into toFrame</param>
        /// <param name="indexFrame">index into this frame</param>
        /// <param name="nbBytes">amount of bytes</param>
        public void ReadBytes(Frame toFrame, int indexToFrame, int indexFrame, int nbBytes)
        {
            SanityCheck(false);

            if (toFrame.lenOffset + nbBytes > toFrame.LengthDataUsed)
                throw new System.ArgumentOutOfRangeException();
            ReadBytes(toFrame.buf, indexToFrame, indexFrame, nbBytes);
        }

        /// <summary>
        /// Append to front of this frame
        /// </summary>
        /// <param name="byteArray">source buffer</param>
        public void WriteToFront(byte[] byteArray)
        {
            AppendToFront(byteArray, 0, byteArray.Length);
        }

        /// <summary>
        /// Append to back of this frame
        /// </summary>
        /// <param name="byteArray">source frame</param>
        public void WriteToBack(byte[] byteArray)
        {
            AppendToBack(byteArray, 0, byteArray.Length);
        }

        /// <summary>
        /// Append to front of this frame
        /// </summary>
        /// <param name="frame">source frame</param>
        public void WriteToFront(Frame frame)
        {
            AppendToFront(frame, 0, frame.LengthDataUsed);
        }

        /// <summary>
        /// Append to back of this frame
        /// </summary>
        /// <param name="frame">source frame</param>
        public void WriteToBack(Frame frame)
        {
            AppendToBack(frame, 0, frame.LengthDataUsed);
        }

        #endregion

        #region Queue
        /// <summary>
        /// Queue of frames. Not thread-safe by itself. No dynamic allocations for queueing needed.
        /// </summary>
        public class Queue
        {
            private Frame[] _frames;
            private int _frameCount;
            private int _head; // begin of queue
            private int _tail; // end of queue
            private bool _fifo; // read order
            private int _attribute; // creator defined purpose

            /// <summary>
            /// Creates a fixed-size queue of frames
            /// </summary>
            /// <param name="size">max amount of frames to store</param>
            /// <param name="fifo">true: FIFO policy, false: LIFO policy</param>
            /// <param name="attribute">creator-defined attribute</param>
            public Queue(int size, bool fifo, int attribute)
            {
                _frames = new Frame[size];
                _frameCount = 0;
                _head = 0;
                _tail = 0;
                _fifo = fifo;
                _attribute = attribute;
            }

            /// <summary>
            /// Get the attribute defined at the constructor
            /// </summary>
            public int attribute
            {
                get { return _attribute; }
            }

            /// <summary>
            /// add frame to queue. return true on success
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool AddFrame(ref Frame item)
            {
                if (_frameCount == _frames.Length)
                    return false;
                _frameCount++;
                _frames[_tail] = item;
                item = null;
                _tail = (_tail + 1) % _frames.Length;
                return true;
            }

            /// <summary>
            /// get frame from queue. returns null of failure
            /// </summary>
            /// <returns></returns>
            public Frame GetFrame()
            {
                if (_frameCount == 0)
                    return null;
                _frameCount--;

                int idx;
                if (_fifo)
                {
                    idx = _head;
                    _head = (_head + 1) % _frames.Length;
                }
                else
                {
                    _tail = (_tail + _frames.Length - 1) % _frames.Length;
                    idx = _tail;
                }

                Frame item = _frames[idx];
                _frames[idx] = null;
                return item;
            }
        }

        #endregion

        #region Pool
        /// <summary>
        /// This class implements a memory pool. The goal is to have a pool of frames that used all the time,
        /// so the load on memory management is reduced.
        /// </summary>
        private class Pool
        {
            private static int c_minSize = 128; // min size of buffer in pool
            private static int c_maxSize = 4096; // max size of buffer in pool

            private ArrayList _framePool; // list of queues of buffers, ordered by buffer size, ascending
            private Object _lock;

            public Pool()
            {
                _framePool = new ArrayList();
                _lock = new Object();
            }

            /// <summary>
            /// Rounds frame size to next power of 2 up to max size.
            /// </summary>
            /// <param name="size"></param>
            /// <returns>Destination size, -1 if size it too large</returns>
            private int GetNormalizedSize(int size)
            {
                if (size > c_maxSize)
                    return -1;
                int res = c_minSize;
                while (res < size)
                    res <<= 1;
                return res;
            }

            /// <summary>
            /// Gets index for frames of specified frameSize in pool.
            /// Caller has to hold _lock.
            /// </summary>
            /// <param name="frameSize"></param>
            /// <param name="create">true: create new entry if it doesnt exist yet</param>
            /// <returns>Zero-based index or -1 for not-found</returns>
            private int GetPoolIndex(int frameSize, bool create)
            {
                int idx = 0;
                while (idx < _framePool.Count)
                {
                    Queue queue = _framePool[idx] as Queue;
                    if (queue == null)
                        throw new ApplicationException();
                    if (queue.attribute == frameSize)
                        return idx;
                    if (queue.attribute > frameSize)
                        break;
                    idx++;
                }

                if (create)
                {
                    Queue queueNew = new Queue(100, true, frameSize); // 100 entries, FIFO
                    // FIFO order helps detecting usage errors using the "released" flag
                    _framePool.Insert(idx, queueNew);
                    return idx;
                }

                return -1;
            }

            /// <summary>
            /// Allocates a new frame of specified size.
            /// </summary>
            /// <param name="frameSize"></param>
            /// <returns></returns>
            public Frame GetFrame(int reqFrameSize)
            {
                int frameSize = GetNormalizedSize(reqFrameSize);
                if (frameSize == -1) // too large for frame pool
                    return new Frame(reqFrameSize);

                // try to get frame from pool
                lock (_lock)
                {
                    int idx = GetPoolIndex(frameSize, false);
                    if (idx != -1)
                    {
                        Queue queue = _framePool[idx] as Queue;
                        if (queue == null)
                            throw new System.ApplicationException();
                        Frame frame = queue.GetFrame();
                        if (frame != null)
                        {
                            frame.SanityCheck(true);

                            if ((frame.lenOffset != 0) || (frame.lenData != 0))
                            {
                                throw new ApplicationException("frame was not properly initialized.");
                            }

                            frame.released = false;
                            return frame;
                        }
                    }
                }

                // create a new frame
                return new Frame(frameSize);
            }

            /// <summary>
            /// Release a frame, moving it into memory pool
            /// </summary>
            /// <param name="frame">The frame to release, will be set to null</param>
            public void Release(ref Frame frame)
            {
                if (frame == null)
                    return;
                int frameSize = GetNormalizedSize(frame.LengthBuffer);
                if (frameSize == -1 || frameSize != frame.LengthBuffer)
                    return; // if frame is too large for pool or length is non-standard, dispose

                lock (_lock)
                {
                    int idx = GetPoolIndex(frameSize, true);
                    Queue queue = _framePool[idx] as Queue;
                    if (queue == null)
                        throw new System.ApplicationException();
                    frame.ReserveHeader(0); // clear frame
                    frame.SanityCheck(false);
                    frame.released = true;
                    bool ignore = queue.AddFrame(ref frame);
                }

                frame = null;
            }
        }

        #endregion

        #region Singleton
        private static readonly Pool instance = new Pool();

        /// <summary>
        /// Get a frame from the memory pool
        /// </summary>
        /// <param name="frameSize">the required size of the frame</param>
        /// <returns></returns>
        public static Frame GetFrame(int frameSize)
        {
#if USE_POOL
            return instance.GetFrame(frameSize);
#else
            return new Frame(frameSize);
#endif
        }

        /// <summary>
        /// Get a frame from the memory pool
        /// </summary>
        /// <param name="headerSize">the required header size</param>
        /// <param name="dataSize">the required data size</param>
        /// <returns></returns>
        public static Frame GetFrame(int headerSize, int dataSize)
        {
            Frame frame = instance.GetFrame(headerSize + dataSize);
            frame.ReserveHeader(headerSize);
            return frame;
        }

        /// <summary>
        /// Return a frame to the memory pool
        /// </summary>
        /// <param name="frame">The frame to release. Will be set to null.</param>
        public static void Release(ref Frame frame)
        {
#if USE_POOL
            instance.Release(ref frame);
#else
            frame = null;
#endif
        }

        #endregion Singleton
    }
}


