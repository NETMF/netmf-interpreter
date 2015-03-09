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
* 'recompile' is a command line tool that pre-compiles one or more regular expressions
* for use with the regular expression matcher class 'RE'.  For example, the command
* <code>java org.apache.regexp.recompile re1 "a*b"</code> produces output like this:
*
* <pre>
*
*    // Pre-compiled regular expression 'a*b'
*    private static final char[] re1Instructions =
*    {
*        0x002a, 0x0000, 0x0007, 0x0041, 0x0001, 0xfffd, 0x0061,
*        0x0041, 0x0001, 0x0004, 0x0062, 0x0045, 0x0000, 0x0000,
*    };
*
*    private static final REProgram re1 = new REProgram(re1Instructions);
*
* </pre>
*
* By pasting this output into your code, you can construct a regular expression matcher
* (RE) object directly from the pre-compiled data (the character array re1), thus avoiding
* the overhead of compiling the expression at runtime.  For example:
*
* <pre>
*  RE r = new RE(re1);
* </pre>
*
* @see RE
* @see RECompiler
*
* @author <a href="mailto:jonl@muppetlabs.com">Jonathan Locke</a>
* @version $Id: recompile.java 518156 2007-03-14 14:31:26Z vgritsenko $
*/

using System;
using Microsoft.SPOT;

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// Class for precompiling regular expressions for later use
    /// </summary>
    public class RegexPrecompiler
    {
        /// <summary>
        /// Main application entrypoint.
        /// Might make this have methods and be a class rathern the a program...
        /// Then the class can Serialise and Deserialiaze the Regexps
        /// </summary>
        /// <param name="arg">Command line arguments</param>
        static public void Main(String[] arg)
        {
            // Create a compiler object
            RegexCompiler r = new RegexCompiler();

            // Print usage if arguments are incorrect
            if (arg.Length <= 0 || arg.Length % 2 != 0)
            {
                Debug.Print("Usage: recompile <patternname> <pattern>");
                return;
            }

            // Loop through arguments, compiling each
            for (int i = 0, end = arg.Length; i < end; i += 2)
            {
                try
                {
                    // Compile regular expression
                    String name = arg[i];
                    String pattern = arg[i + 1];
                    String instructions = name + "Instructions";

                    // Output program as a nice, formatted char array
                    Debug.Print("\n    // Pre-compiled regular expression '" + pattern + "'\n"
                                     + "    private static  char[] " + instructions + " = \n    {");

                    // Compile program for pattern
                    RegexProgram program = r.Compile(pattern);

                    // Number of columns in output
                    int numColumns = 7;

                    // Loop through program
                    char[] p = program.Instructions;
                    for (int j = 0; j < p.Length; j++)
                    {
                        // End of column?
                        if ((j % numColumns) == 0) Debug.Print("\n        ");

                        // Print char as padded hex number                    
                        String hex = (0).ToHexString();
                        
                        while (hex.Length < 4) hex = "0" + hex;
                        
                        Debug.Print("0x" + hex + ", ");
                    }

                    // End of program block
                    Debug.Print("\n    };");
                    Debug.Print("\n    private static  REProgram " + name + " = new REProgram(" + instructions + ");");
                }
                catch (RegexpSyntaxException e)
                {
                    Debug.Print("Syntax error in expression \"" + arg[i] + "\": " + e.ToString());
                }
                catch (Exception e)
                {
                    Debug.Print("Unexpected exception: " + e.ToString());
                }
            }
        }
    }
}
