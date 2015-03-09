using Microsoft.Win32;
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Timers;
using System.Data.SqlClient;
using Microsoft.SPOT.Profiler;
using System.Threading;
using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using Microsoft.NetMicroFramework.Tools.MFProfilerTool;

namespace Microsoft.SPOT.Platform.Test
{
    // 1. Start MF Profiler.
    // 2. Update the device information.
    // 3. Specify the log file.
    // 4. Connect to the device and start profiling.
    // 5. Parse the log file.
    // 6. Generate profile reports.
    internal class Profile
    {   
        private Engine m_engine;
        private ProfilerSession m_session;
        private ManualResetEvent m_done;
#if DEBUG
        private Exporter m_exporter;
#endif        
        
        public Profile()
        {
            m_done = new ManualResetEvent(false);
        }

        internal void StartProfiler(string device, string logFile, string transport, 
            string exePath, string buildPath, ArrayList referenceList, bool isDevEnvironment, string assemblyName )
        {
            try
            {
                PortDefinition port = Utils.GetPort(device, transport, exePath);

                m_engine = new Engine(port);
                m_session = new ProfilerSession(m_engine);

#if DEBUG
                m_exporter = new Exporter_OffProf(m_session, logFile);
#endif

                lock (m_engine)
                {
                    m_engine.StopDebuggerOnConnect = true;
                    m_engine.Start();

                    bool connected = false;

                    connected = m_engine.TryToConnect(20, 500, true, ConnectionSource.TinyCLR);

                    if (connected)
                    {
                        if (m_engine.Capabilities.Profiling == false)
                        {
                            throw new ApplicationException("This device is not running a version of TinyCLR that supports profiling.");
                        }

                        // Deploy the test files to the device.                    
                        Utils.DeployToDevice(buildPath, referenceList, m_engine, transport, isDevEnvironment, assemblyName);

                        // Move IsDeviceInInitializeState(), IsDeviceInExitedState(), 
                        // GetDeviceState(),EnsureProcessIsInInitializedState() to Debugger.dll?

                        m_engine.RebootDevice(Engine.RebootOption.RebootClrWaitForDebugger);

                        if (!m_engine.TryToConnect(100, 500))
                        {
                            throw new ApplicationException("Connection Failed");
                        }

                        m_engine.ThrowOnCommunicationFailure = true;
                        m_session.EnableProfiling();

                        m_session.SetProfilingOptions(true, false);
                        m_engine.OnCommand += new CommandEventHandler(OnWPCommand);
                        m_engine.ResumeExecution();
                    }
                    else
                    {
                        throw new ApplicationException("Connection failed");
                    }
                }
            }
            catch (Exception ex)
            {
                SoftDisconnectDone(null, null);
                throw ex;
            }            
        }

        public ManualResetEvent Done
        {
            get
            {
                return m_done;
            }
        }

        private void OnWPCommand(IncomingMessage msg, bool fReply)
        {
            switch (msg.Header.m_cmd)
            {
                case Commands.c_Monitor_ProgramExit:
                    if (m_session != null)
                    {
                        m_session.OnDisconnect += SoftDisconnectDone;
                        m_session.Disconnect();
                    }
                    break;
            }
        }

        private void SoftDisconnectDone(object sender, EventArgs args)
        {
            string prOutput = String.Concat("Profiling Session Length: ",
                (m_session != null) ? m_session.BitsReceived : 0, " bits.");

            Utils.WriteToEventLog(string.Format("Profile: {0}", prOutput));
            Console.WriteLine(prOutput);

            if (m_session != null)
            {
                m_session.Disconnect();
            }

#if DEBUG
            if (m_exporter != null)
            {
                m_exporter.Close();
            }
#endif

            try
            {
                lock (m_engine)
                {
                    m_engine.Stop();
                }
            }
            // Depending on when we get called, stopping the engine 
            // throws anything from NullReferenceException, ArgumentNullException, IOException, etc.
            catch { }  
            m_engine = null;
            m_done.Set();
        }
    }
}