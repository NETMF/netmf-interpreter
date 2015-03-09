////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Microsoft.SPOT.Debugger
{
    public class CommandLineBuilder
    {
        private ArrayList m_args = new ArrayList();

        public CommandLineBuilder()
        {
        }

        public CommandLineBuilder(string commandLine)
        {
            //not support escaped quotes now
            Debug.Assert(commandLine.IndexOf("\\\"") < 0);
            string[] args = commandLine.Split('"');
            bool fQuoted = false;

            foreach (string arg in args)
            {
                if (fQuoted)
                {
                    m_args.Add(arg);
                }
                else
                {
                    string argTrim = arg.Trim();
                    if (argTrim.Length > 0)
                        m_args.AddRange(argTrim.Split(' '));
                }

                fQuoted = !fQuoted;
            }
        }

        public string[] Arguments
        {
            get { return (string[])m_args.ToArray(typeof(string)); }
        }

        public void AddArguments(params object[] args)
        {
            InsertArguments(m_args.Count, args);
        }

        public void AddArgumentsIfTrue(bool f, params object[] args)
        {
            if (f)
                AddArguments(args);
        }

        public void AddArgumentLoop(string command, params object[] args)
        {
            object[] argsT = null;

            foreach (object arg in args)
            {
                Array arr = arg as Array;

                if (arr == null)
                {
                    argsT = new object[2];
                    argsT[1] = arg;
                }
                else
                {
                    argsT = new object[arr.Length + 1];
                    arr.CopyTo(argsT, 1);
                }

                argsT[0] = command;
                AddArguments(argsT);
            }
        }

        public void AddArgumentLoopIfTrue(bool f, string command, params object[] args)
        {
            if (f)
                AddArgumentLoop(command, args);
        }

        public void InsertArguments(int index, params object[] args)
        {
            foreach (object o in args)
            {
                m_args.Insert(index, o.ToString());
                index++;
            }
        }

        public void RemoveArguments(int index, int count)
        {
            while (count-- > 0)
            {
                m_args.RemoveAt(index);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(m_args.Count * 32);

            foreach (string arg in m_args)
            {
                //not support escaped quotes now
                sb.Append(" \"" + arg + "\"");
            }

            return sb.ToString().Trim();
        }
    }
}
