////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

using _Debugger=System.Diagnostics.Debugger;

namespace Microsoft.SPOT.ConnectionManager
{        
    internal class ConnectionManagerHost 
    {        
        TimeSpan m_tsSleep;
        
        private ConnectionManagerHost()
        {
            m_tsSleep    = TimeSpan.FromMinutes(1);            
        }

        [Conditional("DEBUG")]
        private void ParseArgs(string[] args)
        {
            for(int iArg = 0; iArg < args.Length; iArg++)
            {
                string arg = args[iArg];
                string val = null;
                
                if(arg.Length > 0 && arg[0] == '/')
                {
                    arg = arg.Substring(1, arg.Length-1);
                }

                string[] sArr = arg.Split(':');

                arg = sArr[0];

                if(sArr.Length > 1)
                {
                    val = sArr[1];
                }
                
                switch(arg)
                {
                    case "Sleep":                                                   
                        m_tsSleep = TimeSpan.FromSeconds(int.Parse(val));                        
                        break;                                       
                }
            }
        }

        private void Initialize()
        {
            ChannelRegistrationHelper.RegisterChannel( ManagerFactory.Port );

            RemotingConfiguration.RegisterWellKnownServiceType( typeof(ManagerFactory),
                ManagerFactory.ServiceName,                                                                 
                WellKnownObjectMode.SingleCall );   
        }

        private void Run()
        {
            while(true)
            {                              
                Thread.Sleep(m_tsSleep);

                ManagerRemote managerRemote = new ManagerFactory().ManagerRemote;

                if(managerRemote == null || managerRemote.CanShutdown)
                {
                    break; 
                }
            }       
        }

        static void Main(string[] args)
        {            
            try
            {   
                ConnectionManagerHost cmhost = new ConnectionManagerHost();
                      
                cmhost.ParseArgs( args );
                cmhost.Initialize();            
                cmhost.Run();
            }
            catch(Exception e)
            {
                System.Console.WriteLine("ConnectionManagerHost is shutting down due to an exception: " + e.Message);
            }
        }
    }
}
