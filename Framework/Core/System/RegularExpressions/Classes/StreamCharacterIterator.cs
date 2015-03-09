/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * Ported to C# for the .Net Micro Framework by <a href="mailto:juliusfriedman@gmail.com">Julius Friedman</a>
 * http://netmf.codeplex.com/
 * 
 * Encapsulates String as CharacterIterator.
 *
 * @author <a href="mailto:ales.novak@netbeans.com">Ales Novak</a>
 * @version CVS $Id: StreamCharacterIterator.java 518156 2007-03-14 14:31:26Z vgritsenko $
 */
using System;
using System.IO;

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// Encapsulates Stream as CharacterIterator.
    /// </summary>
    internal class StreamCharacterIterator : ICharacterIterator
    {
        #region Fields

        /// <summary>
        /// Underlying Stream
        /// </summary>
        private MemoryStream inputStream;

        /// <summary>
        /// Buffer of read chars
        /// </summary>
        private StringBuilder buff;

        /// <summary>
        /// at end?
        /// </summary>
        private bool closed;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a StreamCharacterIterator
        /// </summary>
        /// <param name="inputStream">inputStream a MemoryStream, from which input is parsed</param>
        public StreamCharacterIterator(MemoryStream inputStream)
        {
            this.inputStream = inputStream;
            buff = new StringBuilder();
            closed = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a range of characters from the source stream
        /// </summary>
        /// <param name="beginIndex">the beginIndex of the range</param>
        /// <param name="endIndex">the endIndex of the range</param>
        /// <returns>A Substring of the stream from beingIndex to endIndex</returns>
        public String Range(int beginIndex, int endIndex)
        {
            try
            {
                EnsureCapacity(endIndex);
                return buff.ToString().Substring(beginIndex, endIndex - beginIndex);
            }
            catch (IOException e)
            {
                throw new ArgumentOutOfRangeException(e.Message);
            }
        }

        /// <summary>
        /// Gets a range of characers from the underlying stream
        /// </summary>
        /// <param name="beginIndex">the start index</param>
        /// <returns>A range of characters from beginIndex to the end of the stream</returns>
        public String Range(int beginIndex)
        {
            try
            {
                ReadToEnd();
                return buff.ToString().Substring(beginIndex);
            }
            catch (IOException e)
            {
                throw new ArgumentOutOfRangeException(e.Message);
            }
        }

        /// <summary>
        /// gets the character at the specified position
        /// </summary>
        /// <param name="pos">the position in the underlying stream</param>
        /// <returns>the character at the given position</returns>
        public char CharAt(int pos)
        {
            try
            {
                EnsureCapacity(pos);
                return buff.ToString()[pos];
            }
            catch (IOException e)
            {
                throw new ArgumentOutOfRangeException(e.Message);
            }
        }

        /// <summary>
        /// Indicates <tt>true</tt> iff if the specified index is after the end of the character stream
        /// </summary>
        /// <param name="pos">The position to check is within the character stream</param>
        /// <returns></returns>
        public bool IsEnd(int pos)
        {
            if (buff.Length > pos)
            {
                return false;
            }
            else
            {
                try
                {
                    EnsureCapacity(pos);
                    return (buff.Length <= pos);
                }
                catch (IOException e)
                {
                    throw new ArgumentOutOfRangeException(e.Message);
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Reads n characters from the stream and appends them to the buffer
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private int Read(int n)
        {
            if (closed)
            {
                return 0;
            }

            int c;
            int i = n;
            while (--i >= 0)
            {
                c = inputStream.ReadByte();
                if (c < 0) // EOF
                {
                    closed = true;
                    break;
                }
                buff.Append((char)c);
            }
            return n - i;
        }

        /// <summary>
        /// Reads rest of the stream.
        /// </summary>
        private void ReadToEnd()
        {
            while (!closed)
            {
                Read(1000);
            }
        }

        /// <summary>
        /// Reads chars up to the idx
        /// </summary>
        /// <param name="idx"></param>
        private void EnsureCapacity(int idx)
        {
            if (closed)
            {
                return;
            }

            if (idx < buff.Length)
            {
                return;
            }

            Read(++idx - buff.Length);
        }

        #endregion
    }
}
