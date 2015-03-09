using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace MFDpwsTestCaseGenerator
{
    class MFSvcUtil
    {
        private TypedFileInfo m_Exe;

        public MFSvcUtil(string buildTreeRoot)
        {
            ILocator locator = LocatorFactory.GetLocator(buildTreeRoot);

            m_Exe = new TypedFileInfo(locator.Tools + "mfsvcutil.exe", ".exe");
        }

        private WsdlFileX m_Wsdl;
        private CodeFile m_Interface;
        private CodeFile m_ClientProxy;
        private CodeFile m_HostedService;

        public WsdlFileX WSDL
        {
            get { return m_Wsdl; }
            set { m_Wsdl = value; }
        }

        public CodeFile Interface
        {
            get { return m_Interface; }
        }

        public CodeFile ClientProxy
        {
            get { return m_ClientProxy; }
        }

        public CodeFile HostedService
        {
            get { return m_HostedService; }
        }

        public bool GenerateCode()
        {
            if (m_Wsdl == null)
            {
                throw new ArgumentException("WSDL property can not be null.  Must point to a valid WSDL file");
            }

            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo();
            proc.StartInfo.FileName = m_Exe.FileInfo.FullName;
            proc.StartInfo.Arguments = "\"" + m_Wsdl.FileInfo.FullName + "\"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = false;

            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            proc.Start();

            proc.WaitForExit();

            Console.ForegroundColor = currentColor;
            
            if (proc.ExitCode != 0) return false;

            try
            {
                m_Interface = new CodeFile(Environment.CurrentDirectory + "\\" + m_Wsdl.ToInterfaceCS());
                m_ClientProxy = new CodeFile(Environment.CurrentDirectory + "\\" + m_Wsdl.ToClientProxyCS());
                m_HostedService = new CodeFile(Environment.CurrentDirectory + "\\" + m_Wsdl.ToHostedServiceCS());
            }
            catch
            {
                // One of the files does not exist.
                return false;
            }
            return true;
        }
    }
}
