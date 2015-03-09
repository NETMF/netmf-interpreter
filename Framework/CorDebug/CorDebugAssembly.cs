using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using CorDebugInterop;
using Microsoft.SPOT.Debugger.MetaData;

namespace Microsoft.SPOT.Debugger
{
    public class CorDebugAssembly : ICorDebugAssembly, ICorDebugModule, ICorDebugModule2, IDisposable
    {
        CorDebugAppDomain m_appDomain;
        CorDebugProcess m_process;
        Hashtable m_htTokenCLRToPdbx;
        Hashtable m_htTokenTinyCLRToPdbx;
        Pdbx.PdbxFile m_pdbxFile;
        Pdbx.Assembly m_pdbxAssembly;
        IMetaDataImport m_iMetaDataImport;
        uint m_idx;
        string m_name;
        string m_path;
        ulong m_dummyBaseAddress;
        FileStream m_fileStream;
        CorDebugAssembly m_primaryAssembly;
        bool m_isFrameworkAssembly;

        public CorDebugAssembly( CorDebugProcess process, string name, Pdbx.PdbxFile pdbxFile, uint idx )
        {
            m_process = process;
            m_appDomain = null;
            m_name = name;
            m_pdbxFile = pdbxFile;
            m_pdbxAssembly = (pdbxFile != null) ? pdbxFile.Assembly : null;
            m_htTokenCLRToPdbx = new Hashtable();
            m_htTokenTinyCLRToPdbx = new Hashtable();
            m_idx = idx;
            m_primaryAssembly = null;
            m_isFrameworkAssembly = false;

            if(m_pdbxAssembly != null)
            {
                if (!string.IsNullOrEmpty(pdbxFile.PdbxPath))
                {
                    string pth = pdbxFile.PdbxPath.ToLower();

                    if (pth.Contains(@"\buildoutput\"))
                    {
#region V4_1_FRAMEWORK_ASSEMBLIES
                        List<string> frameworkAssemblies = new List<string> {
                                "mfdpwsclient",
                                "mfdpwsdevice",
                                "mfdpwsextensions",
                                "mfwsstack",
                                "microsoft.spot.graphics",
                                "microsoft.spot.hardware",
                                "microsoft.spot.hardware.serialport",
                                "microsoft.spot.hardware.usb",
                                "microsoft.spot.ink",
                                "microsoft.spot.io",
                                "microsoft.spot.native",
                                "microsoft.spot.net",
                                "microsoft.spot.net.security",
                                "microsoft.spot.time",
                                "microsoft.spot.tinycore",
                                "microsoft.spot.touch",
                                "mscorlib",
                                "system.http",
                                "system.io",
                                "system.net.security",
                                "system",
                                "system.xml.legacy",
                                "system.xml", 
                                                             };
#endregion // V4_1_FRAMEWORK_ASSEMBLIES

                        m_isFrameworkAssembly = (frameworkAssemblies.Contains(name.ToLower()));
                    }
                    else
                    {
                        m_isFrameworkAssembly = pdbxFile.PdbxPath.ToLower().Contains(@"\microsoft .net micro framework\");
                    }
                }

                m_pdbxAssembly.CorDebugAssembly = this;
                foreach(Pdbx.Class c in m_pdbxAssembly.Classes)
                {
                    AddTokenToHashtables( c.Token, c );
                    foreach(Pdbx.Field field in c.Fields)
                    {
                        AddTokenToHashtables( field.Token, field );
                    }

                    foreach(Pdbx.Method method in c.Methods)
                    {
                        AddTokenToHashtables( method.Token, method );
                    }
                }
            }
        }

        public ICorDebugAssembly ICorDebugAssembly
        {
            get { return ((ICorDebugAssembly)this); }
        }

        public ICorDebugModule ICorDebugModule
        {
            get { return ((ICorDebugModule)this); }
        }

        private bool IsPrimaryAssembly
        {
            get { return m_primaryAssembly == null; }
        }

        public bool IsFrameworkAssembly
        {
            get { return m_isFrameworkAssembly; }
        }

        private FileStream EnsureFileStream()
        {
            if(this.IsPrimaryAssembly)
            {
                if(m_path != null && m_fileStream == null)
                {
                    m_fileStream = File.OpenRead( m_path );

                    m_dummyBaseAddress = m_process.FakeLoadAssemblyIntoMemory( this );
                }

                return m_fileStream;
            }
            else
            {
                FileStream fileStream = m_primaryAssembly.EnsureFileStream();
                m_dummyBaseAddress = m_primaryAssembly.m_dummyBaseAddress;

                return fileStream;
            }
        }
        
        public CorDebugAssembly CreateAssemblyInstance( CorDebugAppDomain appDomain )
        {
            //Ensure the metadata import is created.  
            IMetaDataImport iMetaDataImport = this.MetaDataImport;

            CorDebugAssembly assm = (CorDebugAssembly)MemberwiseClone();
            assm.m_appDomain = appDomain;
            assm.m_primaryAssembly = this;

            return assm;
        }

        internal void ReadMemory( ulong address, uint size, byte[] buffer, out uint read )
        {
            FileStream fileStream = EnsureFileStream();

            read = 0;

            if(fileStream != null)
            {
                lock(fileStream)
                {
                    fileStream.Position = (long)address;
                    read = (uint)fileStream.Read( buffer, 0, (int)size );
                }
            }
        }

        public static CorDebugAssembly AssemblyFromIdx( uint idx, ArrayList assemblies )
        {
            foreach(CorDebugAssembly assembly in assemblies)
            {
                if(assembly.Idx == idx)
                    return assembly;
            }
            return null;
        }

        public static CorDebugAssembly AssemblyFromIndex( uint index, ArrayList assemblies )
        {
            return AssemblyFromIdx( TinyCLR_TypeSystem.IdxAssemblyFromIndex( index ), assemblies );
        }

        public string Name
        {
            get { return m_name; }
        }

        public bool HasSymbols
        {
            get { return m_pdbxAssembly != null; }
        }

        private void AddTokenToHashtables( Pdbx.Token token, object o )
        {
            m_htTokenCLRToPdbx[token.CLR] = o;
            m_htTokenTinyCLRToPdbx[token.TinyCLR] = o;
        }

        private string FindAssemblyOnDisk()
        {
            if (m_path == null && m_pdbxAssembly != null)
            {
                string[] pathsToTry = new string[]
            {
                // Look next to pdbx file
                Path.Combine( Path.GetDirectoryName( m_pdbxFile.PdbxPath ), m_pdbxAssembly.FileName ),
                // look next to the dll for the SDK (C:\Program Files\Microsoft .NET Micro Framework\<version>\Assemblies\le|be)
                Path.Combine( Path.GetDirectoryName( m_pdbxFile.PdbxPath ), @"..\" + m_pdbxAssembly.FileName ),
                // look next to the dll for the PK (SPOCLIENT\buildoutput\public\<buildtype>\client\pe\le|be)
                Path.Combine( Path.GetDirectoryName( m_pdbxFile.PdbxPath ), @"..\..\dll\" + m_pdbxAssembly.FileName ),
            };

                for (int iPath = 0; iPath < pathsToTry.Length; iPath++)
                {
                    string path = pathsToTry[iPath];

                    if (File.Exists(path))
                    {
                        //is this the right file?
                        m_path = path;
                        break;
                    }
                }
            }

            return m_path;
        }

        private IMetaDataImport FindMetadataImport()
        {
            Debug.Assert( m_iMetaDataImport == null );

            IMetaDataDispenser mdd = new CorMetaDataDispenser() as IMetaDataDispenser;
            object pImport = null;
            Guid iid = typeof( IMetaDataImport ).GUID;
            IMetaDataImport metaDataImport = null;

            try
            {
                string path = FindAssemblyOnDisk();

                if(path != null)
                {
                    mdd.OpenScope( path, (int)MetaData.CorOpenFlags.ofRead, ref iid, out pImport );
                    metaDataImport = pImport as IMetaDataImport;
                }
            }
            catch
            {
            }

            //check the version?
            return metaDataImport;
        }

        public IMetaDataImport MetaDataImport
        {
            get
            {
                if(m_iMetaDataImport == null)
                {
                    if(HasSymbols)
                    {
                        m_iMetaDataImport = FindMetadataImport();
                    }

                    if(m_iMetaDataImport == null)
                    {
                        m_pdbxFile = null;
                        m_pdbxAssembly = null;
                        m_iMetaDataImport = new MetaDataImport( this );
                    }
                }

                return m_iMetaDataImport;
            }
        }

        public CorDebugProcess Process
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_process; }
        }

        public CorDebugAppDomain AppDomain
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_appDomain; }
        }

        public uint Idx
        {
            [System.Diagnostics.DebuggerHidden]
            get { return m_idx; }
        }

        private CorDebugFunction GetFunctionFromToken( uint tk, Hashtable ht )
        {
            CorDebugFunction function = null;
            Pdbx.Method method = ht[tk] as Pdbx.Method;
            if(method != null)
            {
                CorDebugClass c = new CorDebugClass( this, method.Class );
                function = new CorDebugFunction( c, method );
            }

            Debug.Assert( function != null );
            return function;
        }

        public CorDebugFunction GetFunctionFromTokenCLR( uint tk )
        {
            return GetFunctionFromToken( tk, m_htTokenCLRToPdbx );
        }

        public CorDebugFunction GetFunctionFromTokenTinyCLR( uint tk )
        {
            if(HasSymbols)
            {
                return GetFunctionFromToken( tk, m_htTokenTinyCLRToPdbx );
            }
            else
            {
                uint index = TinyCLR_TypeSystem.ClassMemberIndexFromTinyCLRToken( tk, this );

                WireProtocol.Commands.Debugging_Resolve_Method.Result resolvedMethod = this.Process.Engine.ResolveMethod( index );
                Debug.Assert( TinyCLR_TypeSystem.IdxAssemblyFromIndex( resolvedMethod.m_td ) == this.Idx );

                uint tkMethod = TinyCLR_TypeSystem.SymbollessSupport.MethodDefTokenFromTinyCLRToken( tk );
                uint tkClass = TinyCLR_TypeSystem.TinyCLRTokenFromTypeIndex( resolvedMethod.m_td );

                CorDebugClass c = GetClassFromTokenTinyCLR( tkClass );

                return new CorDebugFunction( c, tkMethod );
            }
        }

        public Pdbx.ClassMember GetPdbxClassMemberFromTokenCLR( uint tk )
        {
            return m_htTokenCLRToPdbx[tk] as Pdbx.ClassMember;
        }

        private CorDebugClass GetClassFromToken( uint tk, Hashtable ht )
        {
            CorDebugClass cls = null;
            Pdbx.Class c = ht[tk] as Pdbx.Class;
            if(c != null)
            {
                cls = new CorDebugClass( this, c );
            }

            return cls;
        }

        public CorDebugClass GetClassFromTokenCLR( uint tk )
        {
            return GetClassFromToken( tk, m_htTokenCLRToPdbx );
        }

        public CorDebugClass GetClassFromTokenTinyCLR( uint tk )
        {
            if(HasSymbols)
                return GetClassFromToken( tk, m_htTokenTinyCLRToPdbx );
            else
                return new CorDebugClass( this, TinyCLR_TypeSystem.SymbollessSupport.TypeDefTokenFromTinyCLRToken( tk ) );
        }

        ~CorDebugAssembly()
        {
            try
            {
                ((IDisposable)this).Dispose();
            }
            catch(Exception)
            {
            }
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if(IsPrimaryAssembly)
            {
                if(m_iMetaDataImport != null && !(m_iMetaDataImport is MetaDataImport))
                {
                    Marshal.ReleaseComObject( m_iMetaDataImport );
                }

                m_iMetaDataImport = null;

                if(m_fileStream != null)
                {
                    ((IDisposable)m_fileStream).Dispose();
                    m_fileStream = null;
                }
            }

            GC.SuppressFinalize( this );
        }

        #endregion

        #region ICorDebugAssembly Members

        int ICorDebugAssembly.GetProcess( out ICorDebugProcess ppProcess )
        {
            ppProcess = this.Process;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugAssembly.GetAppDomain( out ICorDebugAppDomain ppAppDomain )
        {
            ppAppDomain = m_appDomain;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugAssembly.EnumerateModules( out ICorDebugModuleEnum ppModules )
        {
            ppModules = new CorDebugEnum( this, typeof( ICorDebugModule ), typeof( ICorDebugModuleEnum ) );

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugAssembly.GetCodeBase( uint cchName, IntPtr pcchName, IntPtr szName )
        {
            Utility.MarshalString( "", cchName, pcchName, szName );

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugAssembly.GetName( uint cchName, IntPtr pcchName, IntPtr szName )
        {
            string name = m_path != null ? m_path : m_name;

            Utility.MarshalString( name, cchName, pcchName, szName );

            return Utility.COM_HResults.S_OK;         
        }

        #endregion

        #region ICorDebugModule Members

        int ICorDebugModule.GetProcess( out ICorDebugProcess ppProcess )
        {
            ppProcess = this.Process;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugModule.GetBaseAddress( out ulong pAddress )
        {
            EnsureFileStream();
            pAddress = m_dummyBaseAddress;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugModule.GetAssembly( out ICorDebugAssembly ppAssembly )
        {
            ppAssembly = this;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.GetName( uint cchName, IntPtr pcchName, IntPtr szName )
        {
            return this.ICorDebugAssembly.GetName( cchName, pcchName, szName );            
        }

        int ICorDebugModule.EnableJITDebugging( int bTrackJITInfo, int bAllowJitOpts )
        {
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.EnableClassLoadCallbacks( int bClassLoadCallbacks )
        {
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.GetFunctionFromToken( uint methodDef, out ICorDebugFunction ppFunction )
        {
            ppFunction = GetFunctionFromTokenCLR( methodDef );

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.GetFunctionFromRVA( ulong rva, out ICorDebugFunction ppFunction )
        {
            ppFunction = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.GetClassFromToken( uint typeDef, out ICorDebugClass ppClass )
        {
            ppClass = GetClassFromTokenCLR( typeDef );

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.CreateBreakpoint( out ICorDebugModuleBreakpoint ppBreakpoint )
        {
            ppBreakpoint = null;

            return Utility.COM_HResults.E_NOTIMPL;
        }

        int ICorDebugModule.GetEditAndContinueSnapshot( out ICorDebugEditAndContinueSnapshot ppEditAndContinueSnapshot )
        {
            ppEditAndContinueSnapshot = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.GetMetaDataInterface( ref Guid riid, out IntPtr ppObj )
        {
            IntPtr pMetaDataImport = Marshal.GetIUnknownForObject( this.MetaDataImport );

            Marshal.QueryInterface( pMetaDataImport, ref riid, out ppObj );
            int cRef = Marshal.Release( pMetaDataImport );

            Debug.Assert( riid == typeof( IMetaDataImport ).GUID || riid == typeof( IMetaDataImport2 ).GUID || riid == typeof( IMetaDataAssemblyImport ).GUID );
            Debug.Assert( this.MetaDataImport != null && ppObj != IntPtr.Zero );

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.GetToken( out uint pToken )
        {
            pToken = m_pdbxAssembly.Token.CLR;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.IsDynamic( out int pDynamic )
        {
            pDynamic = Utility.Boolean.FALSE;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.GetGlobalVariableValue( uint fieldDef, out ICorDebugValue ppValue )
        {
            ppValue = null;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.GetSize( out uint pcBytes )
        {
            pcBytes = 0x1000;

            FileStream fileStream = EnsureFileStream();

            if(fileStream != null)
            {
                pcBytes = (uint)fileStream.Length;
            }

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule.IsInMemory( out int pInMemory )
        {
            pInMemory = Utility.Boolean.BoolToInt( !HasSymbols );// Utility.Boolean.FALSE;

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region ICorDebugModule2 Members

        int ICorDebugModule2.SetJMCStatus( int bIsJustMyCode, uint cTokens, ref uint pTokens )
        {
            Debug.Assert(cTokens == 0);

            bool fJMC = Utility.Boolean.IntToBool( bIsJustMyCode );

            int hres = fJMC ? Utility.COM_HResults.E_FAIL : Utility.COM_HResults.S_OK;

            Debug.Assert( Utility.FImplies( fJMC, this.HasSymbols ) );

            if (this.HasSymbols)
            {
                if (this.Process.Engine.Info_SetJMC(fJMC, ReflectionDefinition.Kind.REFLECTION_ASSEMBLY, TinyCLR_TypeSystem.IndexFromIdxAssemblyIdx(this.Idx)))
                {
                    if(!this.m_isFrameworkAssembly)
                    {
                        //now update the debugger JMC state...
                        foreach (Pdbx.Class c in this.m_pdbxAssembly.Classes)
                        {
                            foreach (Pdbx.Method m in c.Methods)
                            {
                                m.IsJMC = fJMC;
                            }
                        }
                    }
                    hres = Utility.COM_HResults.S_OK;
                }
            }

            return hres;
        }

        int ICorDebugModule2.ApplyChanges( uint cbMetadata, byte[] pbMetadata, uint cbIL, byte[] pbIL )
        {
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule2.SetJITCompilerFlags( uint dwFlags )
        {
            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule2.GetJITCompilerFlags( out uint pdwFlags )
        {
            pdwFlags = (uint)CorDebugJITCompilerFlags.CORDEBUG_JIT_DISABLE_OPTIMIZATION;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugModule2.ResolveAssembly( uint tkAssemblyRef, out ICorDebugAssembly ppAssembly )
        {
            ppAssembly = null;
            return Utility.COM_HResults.S_OK;
        }

        #endregion
    }
}
