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
*
* @author <a href="mailto:juliusfriedman@gmail.com">Julius Friedman</a>
*/
using System;

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// This will be thrown if you have RegexOption.Timed set and the Matching is taking longer then MaxTicks per MatchNode
    /// </summary>
    internal class RegexExecutionTimeException : Exception
    {
        int index;

        /// <summary>
        /// You can use MatchAt again with this index to resume matching after cathing this exception
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        public RegexExecutionTimeException(int index)
            :base("This Regex is taking a long time")
        {
            this.index = index;
        }
    }
}
