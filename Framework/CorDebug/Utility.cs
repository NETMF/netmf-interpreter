using System;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using CorDebugInterop;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Microsoft.SPOT.Debugger
{
    public class Utility
    {
        public class COM_HResults
        {
            public const int S_OK = 0x00000000;
            public const int S_FALSE = 0x00000001;

            public const int E_NOINTERFACE = unchecked((int) 0x80004002);
            public const int CONNECT_E_NOCONNECTION = unchecked((int) 0x80040200);
            public const int E_NOTIMPL = unchecked((int) 0x80004001);
            public const int ERR_XML_ATTRIBUTE_NOT_FOUND = unchecked((int) 0x8004C738);
            public const int E_FAIL = unchecked((int) 0x80004005);

            //from msdbg.idl
            public const int E_PROCESS_DESTROYED = unchecked((int)0x80040070);
            
            public static bool SUCCEEDED(int hr) { return hr >= 0; }
            public static bool FAILED(int hr) { return hr < 0; }
            public static int BOOL_TO_HRESULT( bool b, int hrFalse ) { return b ? S_OK : hrFalse; }
            public static int BOOL_TO_HRESULT_FAIL( bool b ) { return BOOL_TO_HRESULT( b, E_FAIL ); }
            public static int BOOL_TO_HRESULT_FALSE( bool b ) { return BOOL_TO_HRESULT( b, S_FALSE ); }

            public static void Throw( int hr )
            {
                Throw( hr, string.Empty );
            }

            public static void Throw( int hr, string message )
            {
                Debug.Assert( FAILED( hr ) );

                throw new COMException( message, hr );
            }

            public static void ThrowOnFailure( int hr )
            {
                ThrowOnFailure( hr, string.Empty );
            }

            public static void ThrowOnFailure( int hr, string message )
            {
                if(FAILED( hr ))
                {
                    Throw( hr, message );                    
                }
            }
        }

        public class Boolean
        {
            public const int FALSE = 0;
            public const int TRUE = 1;
            public static int BoolToInt(bool b) { return b ? TRUE : FALSE; }
            public static bool IntToBool(int i) { return i == 0 ? false : true; }
        }

        public static object XmlDeserialize(string filename, XmlSerializer xmls)
        {
            object o = null;

            /*
                          
             Using the XmlSerializers have a significant cost to building a temporary assembly to do the serialization.
             SGen can build a new assembly at build time, but we aren't changing our xml structures very often.  To 
             avoid the overhead of another assembly in the build/install, we can just use sgen to generate the deserialization
             code and build that into this assembly.
             
             1.  Run sgen.  Something like the command line below.  Point it at CorDebug, tell it about the type that
                needs to be serialized, and give it an output directory
             
             sgen /assembly:Microsoft.SPOT.Debugger.CorDebug.dll 
              /type:Microsoft.SPOT.Debugger.Pdbx+PdbxFile 
              /keep 
              /force 
              /compiler:/keyfile:%SPOCLIENT%\framework\key.snk 
              /out:d:\temp

             2.  Take the temporary file (given a cryptic filename *.cs in the output directory), and add it
                 to CorDebug.
             3.  Add a #pragma warning disable 0219 around the file, perhaps, to avoid compilation errors about
                 unused variables.                  
             4.  Create the Serializer and call this class.
             */

            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                o = xmls.Deserialize(stream);
            }

            return o;            
        }

        public static void MarshalInt(IntPtr ptr, int val)
        {
            if (ptr != IntPtr.Zero)
                Marshal.WriteInt32(ptr, val);
        }

        public static void MarshalString(string s, uint cchName, IntPtr pcchName, System.IntPtr szName, bool fIncludeNullTerminator)
        {
            if (s == null)
                s = "";

            if (fIncludeNullTerminator)
                s += '\0';

            int cch = s.Length;

            if (szName != IntPtr.Zero)
            {
                char[] chars = s.ToCharArray();

                cch = System.Math.Min((int) cchName, cch);
                if (fIncludeNullTerminator)
                    chars[cch - 1] = '\0';

                Marshal.Copy(chars, 0, szName, cch);
            }

            if (pcchName != IntPtr.Zero)
                Marshal.WriteInt32(pcchName, cch);
        }

        public static void MarshalString(string s, uint cchName, IntPtr pcchName, System.IntPtr szName)
        {
            MarshalString(s, cchName, pcchName, szName, true);
        }

        public static bool InRange(int i, int iLow, int iHigh)
        {
            return i >= iLow && i <= iHigh;
        }

        public static bool InRange(uint i, uint iLow, uint iHigh)
        {
            return i >= iLow && i <= iHigh;
        }

        public static bool FImplies(bool b1, bool b2)
        {
            return !b1 || b2;
        }

        public static void SideAssert(bool b)
        {
            Debug.Assert(b);
        }

        public static void SideAssert(bool b, string s)
        {            
            Debug.Assert(b, s);
        }

        public static object CreateInstance(Type t)
        {
            return t.GetConstructor(Type.EmptyTypes).Invoke(null);
        }

        public static void ShowMessageBox(string s)
        {
            MessageBox.Show(s, "TinyCLR Error");
        }

        //FIXME - where should the pe really be?  Embedded in the dll? Next to it?
        public static string FindPEForDll(string pathDll, bool fTargetIsBigEndian, string frameworkVersion)
        {
            string pathPE = pathDll;

            Microsoft.SPOT.Debugger.PlatformInfo pi = new Microsoft.SPOT.Debugger.PlatformInfo(frameworkVersion);
            ArrayList searchPaths = new ArrayList();

            searchPaths.Add(Path.GetDirectoryName(pathDll));
            searchPaths.AddRange(pi.AssemblyFolders);
            string ext = Path.GetExtension(pathPE).ToLower();

            if (ext != ".pe")
            {
                if (ext != ".dll" && ext != ".exe")
                {
                    pathPE += ".pe";
                }
                else
                {
                    pathPE = Path.ChangeExtension(pathDll, "pe");
                }
                if (!File.Exists(pathPE))
                {
                    // For user applications in v3.0 the pe file is found in the object directory 
                    pathPE = pathPE.ToLower().Replace("\\bin\\", "\\obj\\");
                }
                if(!File.Exists(pathPE))
                {
                    //this is a hack to find the pe....
                    //in our build system, back up one directory and look in the pe folder
                    string fileName = Path.GetFileName(pathPE);

                    foreach (string path in searchPaths)
                    {
                        pathPE = Path.Combine(path, fileName);
                        if (File.Exists(pathPE)) break;
                        
                        if (fTargetIsBigEndian)
                        {
                            pathPE = Path.Combine(Path.Combine(path, @"..\pe\be"), fileName);

                            if (!File.Exists(pathPE))
                            {
                                pathPE = Path.Combine(Path.Combine(path, "be"), fileName);
                            }
                        }
                        else
                        {
                            pathPE = Path.Combine(Path.Combine(path, @"..\pe\le"), fileName);

                            if (!File.Exists(pathPE))
                            {
                                pathPE = Path.Combine(Path.Combine(path, "le"), fileName);
                            }
                        }

                        if (File.Exists(pathPE)) break;
                    }
                }
            }

            //Debug.Assert(File.Exists(pathPE));
            return File.Exists(pathPE) ? pathPE : null;
        }

        public class PropertiesHelper
        {
            public static object GetPropertyValue(EnvDTE.Properties properties, string name)
            {
                return properties.Item(name).Value;
            }

            public static string GetPropertyValueString(EnvDTE.Properties properties, string name)
            {
                return (string) GetPropertyValue(properties, name);
            }
        }

        public class CLSIDList
        {
            private List<Guid> m_guids;

            public CLSIDList(string clsidList)
            {
                string[] guids = clsidList.Split(';');

                m_guids = new List<Guid>(guids.Length);

                for (int iGuid = 0; iGuid < guids.Length; iGuid++)
                {
                    string guid = guids[iGuid];

                    m_guids.Add(new Guid(guid));
                }                               
            }

            public CLSIDList(Guid[] clsidList)
            {
                m_guids = new List<Guid>(clsidList);
            }

            public void Append(Guid guid)
            {
                m_guids.Add(guid);
            }

            public void Prepend(Guid guid)
            {
                m_guids.Insert(0,guid);
            }

            public void Remove(Guid guid)
            {
                m_guids.Remove(guid);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                for (int iGuid = 0; iGuid < m_guids.Count; iGuid++)
                {
                    Guid guid = m_guids[iGuid];

                    sb.Append(guid.ToString("B"));

                    if (iGuid + 1 < m_guids.Count)
                    {
                        sb.Append(";");
                    }
                }

                return sb.ToString();
            }
        }
        
        public class Kernel32
        {
            public const int DUPLICATE_SAME_ACCESS = 0x00000002;
            public const uint CREATE_SUSPENDED = 0x00000004;

            public delegate void CreateThreadCallback(IntPtr lpParameter);

            [DllImport("kernel32.dll", EntryPoint = "CreateProcessW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool CreateProcessW(string appName, string cmdLine, IntPtr lpProcessAttrs, IntPtr lpThreadAttrs, int bInheritHandles, uint dwCreatingFlags, IntPtr lpEnvironment, string curDir, ref _STARTUPINFO info, ref _PROCESS_INFORMATION pinfo);
            [DllImport("kernel32.dll")]
            public static extern IntPtr GetCurrentThread();
            [DllImport("kernel32.dll")]
            public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, int dwDesiredAccess, bool bInheritHandle, int dwOptions);
            [DllImport("kernel32.dll")]
            public static extern bool CloseHandle(IntPtr hObject);
            [DllImport("kernel32.dll")]
            public static extern IntPtr GetCurrentProcess();
            [DllImport("kernel32.dll")]
            public static extern int SuspendThread(IntPtr hThread);
            [DllImport("kernel32.dll")]
            public static extern int ResumeThread(IntPtr hThread);
            [DllImport("kernel32.dll")]
            public static extern int GetCurrentThreadId();
            [DllImport("kernel32.dll")]
            public static extern int GetLastError();
            [DllImport("kernel32.dll")]
            public static extern IntPtr CreateThread(IntPtr lpsa, uint cbStack, CreateThreadCallback lpStartAddr, IntPtr lpvThreadParam, uint fdwCreate, out uint threadId);
        }

        public class ComCtl32
        {
            [DllImport("comctl32.dll")]
            public static extern IntPtr ImageList_Duplicate(IntPtr hIml);
            [DllImport("comctl32.dll")]
            public static extern int ImageList_ReplaceIcon(IntPtr hIml, int i, IntPtr hIcon);
            public static int ImageList_AddIcon(IntPtr hIml, IntPtr hIcon) { return ImageList_ReplaceIcon(hIml, -1, hIcon); }
            [DllImport("comctl32.dll")]
            public static extern bool ImageList_Destroy(IntPtr hIml);
            [DllImport("comctl32.dll")]
            public static extern bool ImageList_GetIconSize(IntPtr himl, out int cx, out int cy);
        }

        public class User32
        {
            [DllImport("user32.dll")]
            public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        }

        public class VsConstants
        {
            public class VisualStudioShell
            {
                public static readonly Guid BuildOutput = new Guid("1BD8A850-02D1-11d1-BEE7-00A0C913D1F8");
                public static readonly Guid DebugOutput = new Guid("FC076020-078A-11D1-A7DF-00A0C9110051");
            }

            public enum VSStd2KCmdID
            {
                ECMD_RUNFXCOPPROJCTX = 1648,
            }
        }

        private static int Floor0(int i)
        {
            return i < 0 ? 0 : i;
        }

        internal static Version CleanVersion(Version v)
        {
            return new Version(Floor0(v.Major), Floor0(v.Minor), Floor0(v.Build), Floor0(v.Revision));
        }

        internal static uint VersionToUint(System.Version v)
        {
            return (((uint)v.Major & 0xFFFF) << 16) | (((uint)v.Minor & 0xFFFF));
        }

        internal static Version VersionFromUint(uint u)
        {
            return new Version(
                (int)(u >> 16) & 0xFFFF,
                (int)(u) & 0xFFFF);
        }

        class VersionFormatException : Exception
        {
            public VersionFormatException(string vs) : base(String.Format("The version string \"{0}\" is not in the proper format", vs)) { }
        }

        internal static Version VersionFromPropertyString(string vs)
        {
            if (!vs.StartsWith("v"))
                throw new VersionFormatException(vs);

            string[] components = vs.Substring(1).Split('.');

            if (components.Length > 4)
                throw new VersionFormatException(vs);

            try
            {
                int major = 0;
                int minor = 0;
                int build = 0;
                int rev = 0;

                major = Int32.Parse(components[0]);
                if (components.Length > 1) minor = Int32.Parse(components[1]);
                if (components.Length > 2) minor = Int32.Parse(components[2]);
                if (components.Length > 3) minor = Int32.Parse(components[3]);

                return new Version(major, minor, build, rev);
            }
            catch (Exception)
            {
                throw new VersionFormatException(vs);
            }
        }

        internal static string MinimumVersionString(Version v)
        {
            string vstr = String.Format(@"v{0}.{1}", v.Major, v.Minor);
            if (v.Build > 0 || v.Revision > 0)
            {
                vstr += String.Format(@".{0}", Floor0(v.Build));
                if (v.Revision > 0)
                    vstr += String.Format(@".{0}", Floor0(v.Revision));
            }
            return vstr;
        }
    }
}
