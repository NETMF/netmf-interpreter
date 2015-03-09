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
 * A subclass of RECompiler which can dump a regular expression program
 * for debugging purposes.
 *
 * @author <a href="mailto:jonl@muppetlabs.com">Jonathan Locke</a>
 * @version $Id: REDebugCompiler.java 518169 2007-03-14 15:03:35Z vgritsenko $
*/

using System;
using System.Text;
using System.Collections;

namespace System.Text.RegularExpressions
{
#if DEBUG

    /// <summary>
    /// A subclass of RECompiler which can dump a regular expression program for debugging purposes.
    /// </summary>
    public class RegexDebugCompiler : RegexCompiler
    {
        /// <summary>
        /// Mapping from opcodes to descriptive strings
        /// </summary>
        static Hashtable hashOpcode = new Hashtable()
    {
        {OpCode.ReluctantStar,    "Star"},
        {OpCode.ReluctantPlus,    "ReluctantPlus"},
        {OpCode.ReluctantMaybe,   "ReluctantMaybe"},
        {OpCode.EndProgram,              "EndProgram"},
        {OpCode.BeginOfLine,              "BeginOfLine"},
        {OpCode.EndOfLine,              "EndOfLine"},
        {OpCode.Any,              "Any"},
        {OpCode.AnyOf,            "AnyOf"},
        {OpCode.Branch,           "Branch"},
        {OpCode.Atom,             "Atom"},
        {OpCode.Star,             "Star"},
        {OpCode.Plus,             "Plus"},
        {OpCode.Maybe,            "Maybe"},
        {OpCode.Nothing,          "Nothing"},
        {OpCode.GoTo,             "GoTo"},
        {OpCode.Continue,         "Continue"},
        {OpCode.Escape,           "Escape"},
        {OpCode.Open,             "Open"},
        {OpCode.Close,            "Close"},
        {OpCode.BackRef,          "BackRef"},
        {OpCode.PosixClass,       "PosixClass"},
        {OpCode.OpenCluster,     "OpenCluster"},
        {OpCode.CloseCluster,    "CloseCluster"}
    };

        /// <summary>
        /// Returns a descriptive string for an opcode.
        /// </summary>
        /// <param name="opcode">Opcode to convert to a string</param>
        /// <returns> Description of opcode</returns>
        String OpcodeToString(char opcode)
        {
            // Get string for opcode
            String ret = (String)hashOpcode[opcode];

            // Just in case we have a corrupt program
            if (ret == null)
            {
                ret = "UNKNOWN_OPCODE";
            }
            return ret;
        }

        /// <summary>
        /// Return a string describing a (possibly unprintable) character.
        /// </summary>
        /// <param name="c">Character to convert to a printable representation</param>
        /// <returns>String representation of character</returns>
        String CharToString(char c)
        {
            // If it's unprintable, convert to '\###'
            if (c < ' ' || c > 127)
            {
                return "\\" + (int)c;
            }

            // Return the character as a string
            return c.ToString();
        }

        /// <summary>
        /// Returns a descriptive string for a node in a regular expression program.
        /// </summary>
        /// <param name="node">Node to describe</param>
        /// <returns>Description of node</returns>
        String NodeToString(int node)
        {
            // Get opcode and opdata for node
            char opcode = Instructions[node /* + RE.offsetOpcode */];
            int opdata = (int)Instructions[node + Regex.offsetOpdata];

            // Return opcode as a string and opdata value
            return OpcodeToString(opcode) + ", opdata = " + opdata;
        }


        /// <summary>
        /// Dumps the current program to a {@link PrintStream}.
        /// </summary>
        /// <param name="p">PrintStream for program dump output</param>
        public void DumpProgram(System.IO.TextWriter p)
        {
            // Loop through the whole program
            for (int i = 0, e = Instructions.Length; i < e; )
            {
                // Get opcode, opdata and next fields of current program node
                char opcode = Instructions[i /* + RE.offsetOpcode */];
                char opdata = Instructions[i + Regex.offsetOpdata];
                int next = (short)Instructions[i + Regex.offsetNext];

                // Display the current program node
                p.Write(i + ". " + NodeToString(i) + ", next = ");

                // If there's no next, say 'none', otherwise give absolute index of next node
                if (next == 0)
                {
                    p.Write("none");
                }
                else
                {
                    p.Write(i + next);
                }

                // Move past node
                i += Regex.nodeSize;

                // If character class
                if (opcode == OpCode.AnyOf)
                {
                    // Opening bracket for start of char class
                    p.Write(", [");

                    // Show each range in the char class
                    // int rangeCount = opdata;
                    for (int r = 0; r < opdata; r++)
                    {
                        // Get first and last chars in range
                        char charFirst = Instructions[i++];
                        char charLast = Instructions[i++];

                        // Print range as X-Y, unless range encompasses only one char
                        if (charFirst == charLast)
                        {
                            p.Write(CharToString(charFirst));
                        }
                        else
                        {
                            p.Write(CharToString(charFirst) + "-" + CharToString(charLast));
                        }
                    }

                    // Annotate the end of the char class
                    p.Write("]");
                }

                // If atom
                if (opcode == OpCode.Atom)
                {
                    // Open quote
                    p.Write(", \"");

                    // Print each character in the atom
                    for (int len = opdata; len-- != 0; )
                    {
                        p.Write(CharToString(Instructions[i++]));
                    }

                    // Close quote
                    p.Write("\"");
                }

                // Print a newline
                p.WriteLine("");
            }
        }

        /// <summary>
        /// Dumps the current program to a <code>System.out</code>.
        /// </summary>
        public void dumpProgram()
        {
            //PrintStream w = new PrintStream(System.out);
            //dumpProgram(w);
            //w.flush();
        }
    }

#endif
}
