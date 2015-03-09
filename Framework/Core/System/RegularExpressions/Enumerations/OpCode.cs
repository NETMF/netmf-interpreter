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
* @author <a href="mailto:jonl@muppetlabs.com">Jonathan Locke</a>
* @version $Id: RE.java,v 1.1.1.1 2002/01/31 03:14:36 rcm Exp $
*/
using System;

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// OpCodes used by the Regex Engine
    /// </summary>
    internal sealed class OpCode
    {
        /************************************************
         *                                              *
         * The format of a node in a program is:        *
         *                                              *
         * [ OPCODE ] [ OPDATA ] [ OPNEXT ] [ OPERAND ] *
         *                                              *
         * char OPCODE - instruction                    *
         * char OPDATA - modifying data                 *
         * char OPNEXT - next node (relative offset)    *
         *                                              *
         ************************************************/

        //   Opcode              Char       Opdata/Operand  Meaning
        //   ----------          ---------- --------------- --------------------------------------------------
        internal const char EndProgram = 'E';  //                 end of program
        internal const char BeginOfLine = '^';  //                 match only if at beginning of line
        internal const char EndOfLine = '$';  //                 match only if at end of line
        internal const char Any = '.';  //                 match any single character except newline
        internal const char AnyOf = '[';  // count/ranges    match any char in the list of ranges
        internal const char Branch = '|';  // node            match this alternative or the next one
        internal const char Atom = 'A';  // Length/string   Length of string followed by string itself
        internal const char Star = '*';  // node            kleene closure
        internal const char Plus = '+';  // node            positive closure
        internal const char Maybe = '?';  // node            optional closure
        internal const char Escape = '\\'; // escape          special escape code char class (escape is E_* code)
        internal const char Open = '(';  // number          nth opening paren
        internal const char OpenCluster = '<';  //                 opening cluster
        internal const char Close = ')';  // number          nth closing paren
        internal const char CloseCluster = '>';  //                 closing cluster
        internal const char BackRef = '#';  // number          reference nth already matched parenthesized string
        internal const char GoTo = 'G';  //                 nothing but a (back-)pointer
        internal const char Nothing = 'N';  //                 match null string such as in '(a|)'
        internal const char Continue = 'C';  //                 continue to the following command (ignore next)
        internal const char ReluctantStar = '8';  // none/expr       reluctant '*' (mnemonic for char is unshifted '*')
        internal const char ReluctantPlus = '=';  // none/expr       reluctant '+' (mnemonic for char is unshifted '+')
        internal const char ReluctantMaybe = '/';  // none/expr       reluctant '?' (mnemonic for char is unshifted '?')
        internal const char PosixClass = 'P';  // classid         one of the posix character classes
    }
}
