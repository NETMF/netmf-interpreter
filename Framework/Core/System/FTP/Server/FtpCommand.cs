using System;
using System.Collections;

namespace Microsoft.SPOT.Net.Ftp
{
    /// <summary>
    /// FTP Command Parser
    ///     generate an FtpCommand object from a string
    /// </summary>
    internal class FtpCommandCreator
    {
        private FtpCommandCreator() { }

        /// <summary>
        /// Create an ftp command from an input string
        /// </summary>
        /// <param name="rawCmd"></param>
        /// <returns></returns>
        public static FtpCommand Create(string rawCmd)
        {
            FtpCommand cmd = null;
            string[] sArray = null;
            string input = rawCmd.Trim();
            int firstSpace = input.IndexOf(' ');
            if (firstSpace == -1)               // there is no space
            {
                sArray = new string[1];
                sArray[0] = input.ToUpper();
            }
            else
            {
                sArray = new string[2];
                sArray[0] = input.Substring(0, firstSpace).ToUpper();
                sArray[1] = input.Substring(firstSpace + 1);
            } 
            if (sArray[0] == "USER")
            {
                cmd = Creater2(FtpCommandType.User, sArray);
            }
            else if (sArray[0] == "PASS")
            {
                cmd = Creater2(FtpCommandType.Pass, sArray);
            }
            else if (sArray[0] == "CWD" || sArray[0] == "XCWD")
            {
                if (sArray.Length == 2)
                {
                    string s = sArray[1];
                    if (s[s.Length - 1] != '/')
                    {
                        sArray[1] += "/";
                    }
                }
                cmd = Creater2(FtpCommandType.Cwd, sArray);
            }
            else if (sArray[0] == "PASV")
            {
                cmd = Creater1(FtpCommandType.Pasv, sArray);
            }
            else if (sArray[0] == "QUIT")
            {
                cmd = Creater1(FtpCommandType.Quit, sArray);
            }
            else if (sArray[0] == "TYPE")
            {
                cmd = Creater2(FtpCommandType.Type, sArray);
            }
            else if (sArray[0] == "LIST")
            {
                cmd = Creater1or2(FtpCommandType.List, sArray);
            }
            else if (sArray[0] == "NLST")
            {
                cmd = Creater1or2(FtpCommandType.NList, sArray);
            }
            else if (sArray[0] == "PORT")
            {
                cmd = Creater2(FtpCommandType.Port, sArray);
            }
            else if (sArray[0] == "SYST")
            {
                cmd = Creater1(FtpCommandType.Sys, sArray);
            }
            else if (sArray[0] == "FEAT")
            {
                cmd = Creater1(FtpCommandType.Feature, sArray);
            }
            else if (sArray[0] == "PWD" || sArray[0] == "XPWD")
            {
                cmd = Creater1(FtpCommandType.Pwd, sArray);
            }
            else if (sArray[0] == "RETR")
            {
                cmd = Creater2(FtpCommandType.Retr, sArray);
            }
            else if (sArray[0] == "MDTM")
            {
                cmd = Creater2(FtpCommandType.Mdtm, sArray);
            }
            else if (sArray[0] == "OPTS")
            {
                cmd = Creater2(FtpCommandType.Opts, sArray);
            }
            else if (sArray[0] == "SIZE")
            {
                cmd = Creater2(FtpCommandType.Size, sArray);
            }
            else if (sArray[0] == "STOR")
            {
                cmd = Creater2(FtpCommandType.Store, sArray);
            }
            else if (sArray[0] == "NOOP")
            {
                cmd = Creater1(FtpCommandType.Noop, sArray);
            }
            else if (sArray[0] == "DELE")
            {
                cmd = Creater2(FtpCommandType.Delete, sArray);
            }
            else if (sArray[0] == "MKD" || sArray[0] == "XMKD")
            {
                cmd = Creater2(FtpCommandType.MkDir, sArray);
            }
            else if (sArray[0] == "RMD" || sArray[0] == "XRMD")
            {
                cmd = Creater2(FtpCommandType.Rmd, sArray);
            }
            else if (sArray[0] == "RNFR")
            {
                cmd = Creater2(FtpCommandType.Rnfr, sArray);
            }
            else if (sArray[0] == "RNTO")
            {
                cmd = Creater2(FtpCommandType.Rnto, sArray);
            }
            else
            {
                cmd = new FtpCommand();
                cmd.m_Type = FtpCommandType.Unknown;
            }

            return cmd;
        }

        /// <summary>
        /// Command creator for commands which do not contains parameters
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sArray"></param>
        /// <returns></returns>
        private static FtpCommand Creater1(FtpCommandType type, string[] sArray)
        {
            FtpCommand command = null;
            if (sArray.Length == 1)
            {
                command = new FtpCommand();
                command.m_Type = type;
            }
            return command;
        }

        /// <summary>
        /// Command creator for commands which must contain a paramter
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sArray"></param>
        /// <returns></returns>
        private static FtpCommand Creater2(FtpCommandType type, string[] sArray)
        {
            FtpCommand command = null;
            if (sArray.Length == 2)
            {
                command = new FtpCommand();
                command.m_Type = type;
                command.m_Content = sArray[1];
            }
            return command;
        }

        /// <summary>
        /// Command creator for commands which may or may not have parameter
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sArray"></param>
        /// <returns></returns>
        private static FtpCommand Creater1or2(FtpCommandType type, string[] sArray)
        {
            FtpCommand command = null;
            if (sArray.Length == 2)
            {
                command = new FtpCommand();
                command.m_Content = sArray[1];
                command.m_Type = type;
            }
            else if (sArray.Length == 1)
            {
                command = new FtpCommand();
                command.m_Type = type;
            }
            return command;
        }
    }

    /// <summary>
    /// The ftp command operator which contains type info and a string to store its parameter
    /// </summary>
    internal class FtpCommand
    {
        // Fields
        internal FtpCommandType m_Type = FtpCommandType.Unknown;
        internal string m_Content = null;

        // Properties
        /// <summary>
        /// Type of the command
        /// </summary>
        public FtpCommandType Type
        {
            get { return m_Type; }
        }

        /// <summary>
        /// The parameter of the command
        ///     null if the command do not have parameters
        /// </summary>
        public string Content
        {
            get { return m_Content; }
        }
    }
}
