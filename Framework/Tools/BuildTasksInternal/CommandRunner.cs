using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;


namespace Microsoft.SPOT.Tasks.Internal
{
    public abstract class MiniLogger
    {
        public abstract void Say(String msg);
        public abstract void Warn(String msg);
        public abstract void Complain(String msg);
        public abstract void Complain(Exception e);
        public abstract void CommandLine(String cmdLine);
    }


    public class CommandRunner
    {
        private class LogFilter
        {
            string[] _warningsToSuppress;
            string _errorPrefix;
            MiniLogger _logger;

            public LogFilter(string[] warningsToSuppress, string prefix, MiniLogger logger)
            {
                _warningsToSuppress = warningsToSuppress;
                _errorPrefix = prefix;
                _logger = logger;
            }


            public void Log(string msg, bool error)
            {
                if ( !String.IsNullOrEmpty(msg) )
                {
                    // Don't bother to add it if it's in the list of ignorable warnings
                    if ( _warningsToSuppress != null )
                    {
                        foreach (string ignorableWarning in _warningsToSuppress)
                        {
                            if ( msg.StartsWith(ignorableWarning + ":") )
                            return;
                        }
                    }

                    if ( error )
                        _logger.Complain(msg);
                    else
                        _logger.Say(msg);
                }
            }


            public void OutputFilter(object sendingProcess, DataReceivedEventArgs dreArgs)
            {
                if ( !String.IsNullOrEmpty(dreArgs.Data) )
                {
                    if ( _errorPrefix != null && dreArgs.Data.StartsWith(_errorPrefix) )
                    {
                        Log(dreArgs.Data, true);
                    }
                    else
                    {
                        Log(dreArgs.Data, false);
                    }
                }
            }

            public void ErrorHandler(object sendingProcess, DataReceivedEventArgs dreArgs)
            {
                Log(dreArgs.Data, true);
            }

        }


        public static bool Execute(MiniLogger logger, String cmd, String args, string[] warningsToSuppress, string errorPrefix)
        {
            Process commandProcess = new Process();

            commandProcess.StartInfo.FileName = cmd;
            commandProcess.StartInfo.Arguments = args;

            logger.CommandLine(cmd + " " + args);

            // Set UseShellExecute to false for redirection.
            commandProcess.StartInfo.UseShellExecute = false;

            // Redirect the standard output and error streams.
            // They are read asynchronously using an event handler.
            commandProcess.StartInfo.RedirectStandardOutput = true;
            commandProcess.StartInfo.RedirectStandardError = true;

            LogFilter logFilter = new LogFilter(warningsToSuppress, errorPrefix, logger);
            commandProcess.OutputDataReceived += new DataReceivedEventHandler(logFilter.OutputFilter);
            commandProcess.ErrorDataReceived += new DataReceivedEventHandler(logFilter.ErrorHandler);

            // Let's go do the real work
            commandProcess.Start();
            commandProcess.BeginOutputReadLine();
            commandProcess.BeginErrorReadLine();
            commandProcess.WaitForExit();

            // How'd it go?
            bool bOut = commandProcess.ExitCode == 0;

            commandProcess.Close();

            return bOut;
        }

    }
}

