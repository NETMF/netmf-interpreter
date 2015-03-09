using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CorDebugInterop;
using Microsoft.Win32;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Designer.Interfaces;
using System.CodeDom;
using System.CodeDom.Compiler;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.SPOT.Debugger
{
    internal abstract class BaseCodeGenerator : IVsSingleFileGenerator
    {
        private string codeFileNameSpace = string.Empty;
        private string codeFilePath = string.Empty;

        private IVsGeneratorProgress codeGeneratorProgress;

        // **************************** PROPERTIES ****************************
        protected string FileNameSpace
        {
            get
            {
                return codeFileNameSpace;
            }
        }

        protected string InputFilePath
        {
            get
            {
                return codeFilePath;
            }
        }

        internal IVsGeneratorProgress CodeGeneratorProgress
        {
            get
            {
                return codeGeneratorProgress;
            }
        }

        // **************************** METHODS **************************
        public abstract int DefaultExtension( out string ext );

        // MUST implement this abstract method.
        protected abstract byte[] GenerateCode( string inputFileName, string inputFileContent );


        protected virtual void GeneratorErrorCallback( int warning, uint level, string message, uint line, uint column )
        {
            IVsGeneratorProgress progress = CodeGeneratorProgress;
            if(progress != null)
            {
                Utility.COM_HResults.ThrowOnFailure( progress.GeneratorError( warning, level, message, line, column ) );
                
            }
        }

        public int Generate( string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace,
                             IntPtr[] pbstrOutputFileContents, out uint pbstrOutputFileContentSize, IVsGeneratorProgress pGenerateProgress )
        {

            if(bstrInputFileContents == null)
            {
                throw new ArgumentNullException( bstrInputFileContents );
            }
            codeFilePath = wszInputFilePath;
            codeFileNameSpace = wszDefaultNamespace;
            codeGeneratorProgress = pGenerateProgress;

            byte[] bytes = GenerateCode( wszInputFilePath, bstrInputFileContents );
            if(bytes == null)
            {
                pbstrOutputFileContents[0] = IntPtr.Zero;
                pbstrOutputFileContentSize = 0;
            }
            else
            {
                pbstrOutputFileContents[0] = Marshal.AllocCoTaskMem( bytes.Length );
                Marshal.Copy( bytes, 0, pbstrOutputFileContents[0], bytes.Length );
                pbstrOutputFileContentSize = (uint)bytes.Length;
            }
            return Utility.COM_HResults.S_OK;
        }

        protected byte[] StreamToBytes( Stream stream )
        {
            if(stream.Length == 0)
                return new byte[] { };

            long position = stream.Position;
            stream.Position = 0;
            byte[] bytes = new byte[(int)stream.Length];
            stream.Read( bytes, 0, bytes.Length );
            stream.Position = position;

            return bytes;
        }
    }

    internal abstract class BaseCodeGeneratorWithSite : BaseCodeGenerator, IObjectWithSite
    {

        private Object site = null;
        private CodeDomProvider codeDomProvider = null;
        private static Guid CodeDomInterfaceGuid = new Guid( "{73E59688-C7C4-4a85-AF64-A538754784C5}" );
        private static Guid CodeDomServiceGuid = CodeDomInterfaceGuid;
        private ServiceProvider serviceProvider = null;

        protected virtual CodeDomProvider CodeProvider
        {
            get
            {
                if(codeDomProvider == null)
                {
                    IVSMDCodeDomProvider vsmdCodeDomProvider = (IVSMDCodeDomProvider)GetService( CodeDomServiceGuid );
                    if(vsmdCodeDomProvider != null)
                    {
                        codeDomProvider = (CodeDomProvider)vsmdCodeDomProvider.CodeDomProvider;
                    }
                    Debug.Assert( codeDomProvider != null, "Get CodeDomProvider Interface failed.  GetService(QueryService(CodeDomProvider) returned Null." );
                }
                return codeDomProvider;
            }
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException();
                }

                codeDomProvider = value;
            }
        }

        private ServiceProvider SiteServiceProvider
        {
            get
            {
                if(serviceProvider == null)
                {
                    IOleServiceProvider oleServiceProvider = site as IOleServiceProvider;
                    Debug.Assert( oleServiceProvider != null, "Unable to get IOleServiceProvider from site object." );

                    serviceProvider = new ServiceProvider( oleServiceProvider );
                }
                return serviceProvider;
            }
        }

        protected Object GetService( Guid serviceGuid )
        {
            return SiteServiceProvider.GetService( serviceGuid );
        }

        protected object GetService( Type serviceType )
        {
            return SiteServiceProvider.GetService( serviceType );
        }


        public override int DefaultExtension( out string ext )
        {
            CodeDomProvider codeDom = CodeProvider;
            Debug.Assert( codeDom != null, "CodeDomProvider is NULL." );
            string extension = codeDom.FileExtension;
            if(extension != null && extension.Length > 0)
            {
                if(extension[0] != '.')
                {
                    extension = "." + extension;
                }
            }

            ext = extension;
            return Utility.COM_HResults.S_OK;
        }

        protected virtual ICodeGenerator GetCodeWriter()
        {
            CodeDomProvider codeDom = CodeProvider;
            if(codeDom != null)
            {
#pragma warning disable 618 //backwards compat
                return codeDom.CreateGenerator();
#pragma warning restore 618
            }

            return null;
        }

        // ******************* Implement IObjectWithSite *****************
        //
        public virtual void SetSite( object pUnkSite )
        {
            site = pUnkSite;
            codeDomProvider = null;
            serviceProvider = null;
        }

        // Does anyone rely on this method?
        public virtual void GetSite( ref Guid riid, out IntPtr ppvSite )
        {
            if(site == null)
            {
                Utility.COM_HResults.Throw( Utility.COM_HResults.E_FAIL );
            }

            IntPtr pUnknownPointer = Marshal.GetIUnknownForObject( site );
            try
            {
                Marshal.QueryInterface( pUnknownPointer, ref riid, out ppvSite );

                if(ppvSite == IntPtr.Zero)
                {
                    Utility.COM_HResults.Throw( Utility.COM_HResults.E_NOINTERFACE );
                }
            }
            finally
            {
                if(pUnknownPointer != IntPtr.Zero)
                {
                    Marshal.Release( pUnknownPointer );
                    pUnknownPointer = IntPtr.Zero;
                }
            }
        }
    }

    internal class ResXFileCodeGenerator : BaseCodeGeneratorWithSite, IObjectWithSite
    {
        internal const string c_Name   = "ResXFileCodeGenerator";
        private const string DesignerExtension = ".Designer";

        internal bool m_fInternal = true;
        internal bool m_fNestedEnums = true;
        internal bool m_fMscorlib = false;
        VsProject _project;

        public ResXFileCodeGenerator(VsProject vsProject)
        {
            _project = vsProject;
        }

        protected string GetResourcesNamespace()
        {
            string resourcesNamespace = null;
            try
            {
                IntPtr punkVsBrowseObject;
                Guid vsBrowseObjectGuid = typeof( IVsBrowseObject ).GUID;

                GetSite( ref vsBrowseObjectGuid, out punkVsBrowseObject );

                if(punkVsBrowseObject != IntPtr.Zero)
                {

                    IVsBrowseObject vsBrowseObject = Marshal.GetObjectForIUnknown( punkVsBrowseObject ) as IVsBrowseObject;
                    Debug.Assert( vsBrowseObject != null, "Generator invoked by Site that is not IVsBrowseObject?" );

                    Marshal.Release( punkVsBrowseObject );

                    if(vsBrowseObject != null)
                    {
                        IVsHierarchy vsHierarchy;
                        uint vsitemid;

                        vsBrowseObject.GetProjectItem( out vsHierarchy, out vsitemid );

                        Debug.Assert( vsHierarchy != null, "GetProjectItem should have thrown or returned a valid IVsHierarchy" );
                        Debug.Assert( vsitemid != 0, "GetProjectItem should have thrown or returned a valid VSITEMID" );


                        if(vsHierarchy != null)
                        {
                            object obj;

                            vsHierarchy.GetProperty( vsitemid, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out obj );
                            string objStr = obj as string;
                            if(objStr != null)
                            {
                                resourcesNamespace = objStr;
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine( e.ToString() );
                Debug.Fail( "These methods should succeed..." );
            }

            return resourcesNamespace;
        }

        public override int DefaultExtension( out string ext )
        {
            //copied from ResXFileCodeGenerator
            string baseExtension;
            ext = String.Empty;

            int hResult = base.DefaultExtension( out baseExtension );

            if(!Utility.COM_HResults.SUCCEEDED( hResult ))
            {
                Debug.Fail( "Invalid hresult returned by the base DefaultExtension" );
                return hResult;
            }

            if(!String.IsNullOrEmpty( baseExtension ))
            {
                ext = DesignerExtension + baseExtension;
            }

            return Utility.COM_HResults.S_OK;
        }

        protected override byte[] GenerateCode( string inputFileName, string inputFileContent )
        {
            VSLangProj.Reference reference = _project.VSLangProj_References.Find("System.Drawing");
            if (reference != null)
            {
                reference.Remove();
            }

            string spotGraphics = "Microsoft.SPOT.Graphics";
            reference = _project.VSLangProj_References.Find(spotGraphics);
            if (reference == null)
            {
                _project.VSLangProj_References.Add(spotGraphics);                
            }

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter( stream );
            string inputFileNameWithoutExtension = Path.GetFileNameWithoutExtension( inputFileName );            

            Assembly tasks = null;

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Compare(asm.GetName().Name, "Microsoft.SPOT.Tasks", true) == 0)
                {
                    if(asm.GetName().Version.ToString(2) == new Version(_project.GetTargetFrameworkProperty().TrimStart('v')).ToString(2))
                    {
                        tasks = asm;
                        break;
                    }
                }
            }

            if (tasks == null) throw new Exception("Microsoft.SPOT.Tasks.dll is not loaded!");

            Type typ = tasks.GetType("Microsoft.SPOT.Tasks.ProcessResourceFiles");

            if (typ != null)
            {
                object processResourceFiles = typ.GetConstructor(new Type[]{}).Invoke(null);

                typ.GetProperty("StronglyTypedClassName").SetValue(processResourceFiles, inputFileNameWithoutExtension, null);
                typ.GetProperty("StronglyTypedNamespace").SetValue(processResourceFiles, GetResourcesNamespace(), null);
                typ.GetProperty("GenerateNestedEnums").SetValue(processResourceFiles, m_fNestedEnums, null);
                typ.GetProperty("GenerateInternalClass").SetValue(processResourceFiles, m_fInternal, null);
                typ.GetProperty("IsMscorlib").SetValue(processResourceFiles, m_fMscorlib, null);

                string resourceName = (string)typ.GetProperty("StronglyTypedNamespace").GetValue(processResourceFiles, null);

                if (string.IsNullOrEmpty(resourceName))
                {
                    resourceName = inputFileNameWithoutExtension;
                }
                else
                {
                    resourceName = string.Format("{0}.{1}", resourceName, inputFileNameWithoutExtension);
                }

                typ.GetMethod("CreateStronglyTypedResources").Invoke(processResourceFiles, new object[]{ inputFileName, this.CodeProvider, writer, resourceName });
            }

            return base.StreamToBytes( stream );
        }
    }
}
