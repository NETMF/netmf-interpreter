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
* Exception thrown to indicate a syntax error in a regular expression.
* This is a non-checked exception because you should only have problems compiling
* a regular expression during development.
* If you are making regular expresion programs dynamically then you can catch it
* if you wish. But should not be forced to.
* Encapsulates different types of character sources - String, InputStream, ...
* Defines a set of common methods
*
* @author <a href="mailto:ales.novak@netbeans.com">Ales Novak</a>
* @version CVS $Id: CharacterIterator.java 518156 2007-03-14 14:31:26Z vgritsenko $
*/
using System;
using System.Text;

namespace System.Text.RegularExpressions
{    
    /// <summary>
    /// Encapsulates different types of character sources - String, InputStream, ...
    /// Defines a set of common methods 
    /// </summary>
    internal interface ICharacterIterator
    {        
        /// <summary>
        /// Gets a substring of the character stream
        /// </summary>
        /// <param name="beginIndex">The position to begin the substring</param>
        /// <param name="endIndex">The position in the origional string where the substring should end</param>
        /// <returns>A substring of the character stream starting at the given index</returns>
        String Range(int beginIndex, int endIndex);    
        
        /// <summary>
        /// Gets a substring of the character stream
        /// </summary>
        /// <param name="beginIndex">The position to begin the substring</param>
        /// <returns>A substring of the character stream starting at the given index</returns>
        String Range(int beginIndex);   
        
        /// <summary>
        /// Retrieves the character at the underlying position in the stream
        /// </summary>
        /// <param name="pos">The postion in the character stream</param>
        /// <returns>A character at the specified position.</returns>
        char CharAt(int pos);    
        
        /// <summary>
        /// Determines weather the given position is at the end of the stream
        /// </summary>
        /// <param name="pos">The position in the character stream</param>
        /// <returns>true iff if the specified index is after the end of the character stream</returns>
        bool IsEnd(int pos);
    }
}
