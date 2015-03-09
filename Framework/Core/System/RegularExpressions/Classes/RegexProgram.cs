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
 * A class that holds compiled regular expressions.  This is exposed mainly
 * for use by the recompile utility (which helps you produce precompiled
 * REProgram objects). You should not otherwise need to work directly with
 * this class.
 *
 * @see RE -> (Regex)
 * @see RECompiler -> (RegexCompiler)
 *
 * @author <a href="mailto:jonl@muppetlabs.com">Jonathan Locke</a>
 * @version $Id: REProgram.java 518156 2007-03-14 14:31:26Z vgritsenko $
*/
using System;
using System.Text;

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// A class that holds compiled regular expressions.  You should not need to work directly with this class. 
    /// Fields are marked internal so that they can be accesed from Capture, Group and Match as well as other classes in this assembly.    
    /// </summary>
    internal sealed class RegexProgram
    {
        #region Fields

        /// <summary>
        /// The compiled regular expression 'program' which can be thought of as opcodes and read as char's
        /// </summary>
        char[] instruction;         
        /// <summary>
        /// The amount of the instruction buffer in use out of all allocated instructions
        /// </summary>
        int lenInstruction;         
        /// <summary>
        /// Prefix string optimization, (only performed if present)
        /// </summary>
        char[] prefix;              
        /// <summary>
        /// Execution flags for the program, certain matches can be optomized by the use or lack there of certain flags. 
        /// Set when the Instructions property is set or when setInstructions is called.
        /// Can be changed by the Regex when Match is executed for advanced operations if necessary as well.
        /// </summary>
        ProgramOptions flags;
        /// <summary>
        /// The maximum amount of matches to perform, changed by the Regex when Match is executed 
        /// </summary>
        int maxMatches = -1;       
        /// <summary>
        /// The pattern we are matching against, it cannot be changed unless this program is Recompiled
        /// </summary>
        internal readonly string pattern;            

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a program object from a character array.
        /// Internal use only.
        /// </summary>
        /// <param name="instruction">Instruction array with RegexProgram opcode instructions in it</param>
        internal RegexProgram(char[] instruction)
            : this(instruction, instruction.Length, string.Empty) { }

        /// <summary>
        /// Constructs a program object from a character array
        /// </summary>
        /// <param name="parens">Count of parens in the program</param>
        /// <param name="instruction">Character array with RE opcode instructions in it</param>
        /// <param name="pattern">The pattern to match against</param>
        public RegexProgram(int parens, char[] instruction, string pattern)
            : this(instruction, instruction.Length, pattern)
        {
            this.maxMatches = parens;
        }

       /// <summary>
        /// Constructs a RegexProgram object from a char array which can be though of as RegexOpcode's
       /// </summary>
       /// <param name="instruction">Character array with RE opcode instructions in it</param>
       /// <param name="lenInstruction"> Amount of instruction array in use</param>
        /// <param name="pattern">The pattern this RegexProgram matches against</param>
        public RegexProgram(char[] instruction, int lenInstruction, string pattern)
        {
            this.pattern = pattern;
            setInstructions(instruction, lenInstruction);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The string representation of the pattern this RegexProgram matches against
        /// </summary>
        public string Pattern
        {
            get { return pattern; }
        }

        /// <summary>
        /// The char array of instuctions which this RegexProgram matches against. It can be though of and read as Regex.Opcode's
        /// </summary>
        internal char[] Instructions
        {
            get
            {
                return instruction;
            }
            set
            {
                setInstructions(instruction, instruction.Length);
            }
        }

        /// <summary>
        /// Optomization and Matching flags
        /// </summary>
        internal ProgramOptions Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        /// <summary>
        /// The maximum number of matches to match against
        /// </summary>
        internal int MaximumMatches
        {
            get
            {
                return maxMatches;
            }
            set { maxMatches = value; }
        }

        /// <summary>
        /// Returns a copy of the prefix of current regular expression program
        /// in a character array.  If there is no prefix, or there is no program
        /// compiled yet, <code>getPrefix</code> will return null.
        /// return A copy of the prefix of current compiled RE program
        /// </summary>
        internal char[] Prefix
        {
            get { return prefix; }
        }

        #endregion

        /// <summary>
        /// Sets a new regular expression program to run.  It is this method which
        /// performs any special compile-time search optimizations.  Currently only
        /// two optimizations are in place - one which checks for backreferences
        /// (so that they can be lazily allocated) and another which attempts to
        /// find an prefix anchor string so that substantial amounts of input can
        /// potentially be skipped without running the actual program.
        /// </summary>
        /// <param name="instruction">Program instruction buffer</param>
        /// <param name="lenInstruction">Length of instruction buffer in use</param>
        void setInstructions(char[] instruction, int lenInstruction)
        {
            // Save reference to instruction array
            this.instruction = instruction;
            this.lenInstruction = lenInstruction;

            // Initialize other program-related variables
            this.flags = 0;
            this.prefix = null;

            // Try various compile-time optimizations if there's a program
            if (instruction != null && lenInstruction != 0)
            {
                // If the first node is a branch
                if (lenInstruction >= Regex.nodeSize && instruction[0 + Regex.offsetOpcode] == OpCode.Branch)
                {
                    // to the end node
                    int next = (short)instruction[0 + Regex.offsetNext];
                    if (instruction[next + Regex.offsetOpcode] == OpCode.EndProgram && lenInstruction >= (Regex.nodeSize * 2))
                    {
                        char nextOp = instruction[Regex.nodeSize + Regex.offsetOpcode];
                        // the branch starts with an atom
                        if (nextOp == OpCode.Atom)
                        {
                            // then get that atom as an prefix because there's no other choice
                            int lenAtom = instruction[Regex.nodeSize + Regex.offsetOpdata];
                            this.prefix = new char[lenAtom];
                            System.Array.Copy(instruction, Regex.nodeSize * 2, prefix, 0, lenAtom);
                        }
                        // the branch starts with a BOL
                        else if (nextOp == OpCode.BeginOfLine)
                        {
                            // then set the flag indicating that BOL is present
                            this.flags |= ProgramOptions.HasBeginOfLine;
                        }
                    }
                }

                // Check for backreferences
                for (int i = 0; i < lenInstruction; i += Regex.nodeSize)
                {
                    switch (instruction[i + Regex.offsetOpcode])
                    {
                        case OpCode.AnyOf:
                            i += (instruction[i + Regex.offsetOpdata] * 2);
                            break;

                        case OpCode.Atom:
                            i += instruction[i + Regex.offsetOpdata];
                            break;

                        case OpCode.BackRef:
                            flags |= ProgramOptions.HasBackrefrence;
                            return;
                    }
                }

            }
        }
    }
}
