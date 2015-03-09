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
 * @version CVS $Id: StringCharacterIterator.java 518156 2007-03-14 14:31:26Z vgritsenko $
 */
using System;
using System.Text;


namespace System.Text.RegularExpressions
{
    /// <summary>
    ///  Encapsulates String as CharacterIterator.
    /// </summary>
    internal class StringCharacterIterator : ICharacterIterator
    {
        /// <summary>
        /// encapsulated string
        /// </summary>
        private String src;
        int srcLength;

        /// <summary>
        /// Creates a StringCharacterIterator for the given string
        /// </summary>
        /// <param name="src">The string to encapsulate</param>
        public StringCharacterIterator(String src)
        {
            this.src = src;
            this.srcLength = src.Length;
        }

        /// <summary>
        /// Gets a range of characters from the encapsulated string
        /// </summary>
        /// <param name="beginIndex">The position to start the range</param>
        /// <param name="endIndex">The position to end the range</param>
        /// <returns>A new string with the characters within the range</returns>
        public String Range(int beginIndex, int endIndex)
        {
            return src.Substring(beginIndex, endIndex - beginIndex);
            //return StringExtensions.Range(src, beginIndex, endIndex);
        }

        /// <summary>
        /// Gets a range of characters from the encapsulated string
        /// </summary>
        /// <param name="beginIndex">The position to start the range</param>
        /// <returns>A new string with the characters within the range</returns>
        public String Range(int beginIndex)
        {
            return src.Substring(beginIndex);
        }

        /// <summary>
        /// Gets a character at the specified position in the encapsulated string.
        /// </summary>
        /// <param name="pos">The position to get the character at</param>
        /// <returns>The character at the specified position</returns>
        public char CharAt(int pos)
        {
            return src[pos];
        }

        /// <summary>
        /// Returns the encapsulated string
        /// </summary>
        /// <returns>The encapsulated string</returns>
        public override string ToString()
        {
            return src;
        }

        /// <summary>
        /// Indicates weather or not he given position is past the end of the encapsulated string
        /// </summary>
        /// <param name="pos"></param>
        /// <returns><tt>true</tt> iff if the specified index is past the end of the character stream</returns>
        public bool IsEnd(int pos)
        {
            return (pos >= srcLength);
        }
    }


} 
