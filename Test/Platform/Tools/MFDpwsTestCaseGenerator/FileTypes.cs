using System;
using System.IO;

namespace MFDpwsTestCaseGenerator
{
    internal class TypedFileInfo
    {
        private FileInfo m_FileInfo;

        internal TypedFileInfo(string path, string extension)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found", path);
            }

            m_FileInfo = new FileInfo(path);

            if (m_FileInfo.Extension.ToLower() != extension.ToLower())
            {
                throw new ArgumentException("File does not have the correct extension.  Expected " + extension, path);
            }
        }

        internal FileInfo FileInfo { get { return m_FileInfo; } }
    }

    public class WsdlFileX
    {
        private TypedFileInfo m_FileInfo;

        public WsdlFileX(string path)
        {
            m_FileInfo = new TypedFileInfo(path, ".wsdl");
        }

        public FileInfo FileInfo
        {
            get { return m_FileInfo.FileInfo; }
        }

        public string ToInterfaceCS()
        {
            return Path.GetFileNameWithoutExtension(m_FileInfo.FileInfo.Name) + ".cs";
        }

        public string ToClientProxyCS()
        {
            return Path.GetFileNameWithoutExtension(m_FileInfo.FileInfo.Name) + "ClientProxy.cs";
        }

        public string ToHostedServiceCS()
        {
            return Path.GetFileNameWithoutExtension(m_FileInfo.FileInfo.Name) + "HostedService.cs";
        }
    }

    public class CodeFile
    {
        private TypedFileInfo m_FileInfo;

        public CodeFile(string path)
        {
            m_FileInfo = new TypedFileInfo(path, ".cs");
        }

        public FileInfo FileInfo
        {
            get { return m_FileInfo.FileInfo; }
        }
    }

    public class ExeFile
    {
        private TypedFileInfo m_FileInfo;

        public ExeFile(string path)
        {
            m_FileInfo = new TypedFileInfo(path, ".exe");
        }

        public FileInfo FileInfo
        {
            get { return m_FileInfo.FileInfo; }
        }
    }

    public class DllFile
    {
        private TypedFileInfo m_FileInfo;

        public DllFile(string path)
        {
            m_FileInfo = new TypedFileInfo(path, ".dll");
        }

        public FileInfo FileInfo
        {
            get { return m_FileInfo.FileInfo; }
        }
    }
}
