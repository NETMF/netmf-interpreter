using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentDiagnostics;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.SPOT.Debugger
{
    [ComImport, Guid( "591E80E4-5F44-11d3-8BDC-00C04F8EC28C" ), InterfaceType( 1 ), ComVisible( true )]
    internal interface IVsMicrosoftInstalledProduct
    {
        [PreserveSig]
        int IdBmpSplash( out uint pIdBmp );
        [PreserveSig]
        int IdIcoLogoForAboutbox( out uint pIdIco );
        [PreserveSig]
        int OfficialName( out string pbstrName );
        [PreserveSig]
        int ProductDetails( out string pbstrProductDetails );
        [PreserveSig]
        int ProductID( out string pbstrPID );
        [PreserveSig]
        int ProductRegistryName( out string pbstrRegName );
    }

    /// <summary>
    /// Summary description for VsPackage.
    /// </summary>
    /// 
    ///ms-help://MS.VSCC/MS.VSIP/vsenvsdk/html/vsconOverviewOfVisualStudioSecurityVSPackageLoadKeys.htm
    /// and a satellite dll, etc...
    /// 
    [ComVisible( true ), Guid( "78581CBA-38EF-44C8-B0AF-D022375AFFE4" )]
    [ProvideProjectFactory( typeof( VsProjectCSharp.ProjectFactory )
                          , "MicroFrameworkVCSFactory"
                          , "Micro Framework Project Files (*.csproj);*.csproj"
                          , "csproj"
                          , "csproj"
                          , @"\.\NullPath"
                          , LanguageVsTemplate="CSharp"
                          , TemplateGroupIDsVsTemplate="MicroFramework"
                          , ProjectSubTypeVsTemplate="MicroFramework"
                          , ShowOnlySpecifiedTemplatesVsTemplate=true
                          , TemplateIDsVsTemplate=VsPackage.VbTemplateIds
                          )
    ]
    [ProvideProjectFactory( typeof( VsProjectVisualBasic.ProjectFactory )
                          , "MicroFrameworkVBFactory"
                          , "Micro Framework Project Files (*.vbproj);*.vbproj"
                          , "vbproj"
                          , "vbproj"
                          , @"\.\NullPath"
                          , LanguageVsTemplate = "VisualBasic"
                          , TemplateGroupIDsVsTemplate = "MicroFramework"
                          , ProjectSubTypeVsTemplate = "MicroFramework"
                          , ShowOnlySpecifiedTemplatesVsTemplate = true
                          , TemplateIDsVsTemplate=VsPackage.VbTemplateIds
                          )
    ]
    [ProvideObject( typeof( PropertyPageComObject ) )]
    [ProvideObject( typeof( CorDebug ) )]
    [ProvideDebugEngine]
    [ProvidePortSupplier(typeof( DebugPortSupplier), "{499dc3d0-66c1-42a2-a292-6ee253d7f708}")]
    public class VsPackage : Package, IVsInstalledProduct, IVsMicrosoftInstalledProduct
    {
        internal const string VcsTemplateIds = "Microsoft.CSharp.Bitmap,Microsoft.CSharp.ClassDiagram,Microsoft.CSharp.CodeFile,Microsoft.CSharp.HTMLPage,Microsoft.CSharp.Resource,Microsoft.CSharp.StyleSheet,Microsoft.CSharp.TextFile,Microsoft.CSharp.XmlFile,Microsoft.CSharp.XmlSchema,Microsoft.CSharp.XsltFile";
        internal const string VbTemplateIds = "Microsoft.VisualBasic.General.Bitmap,Microsoft.VisualBasic.General.ClassDiagram,Microsoft.VisualBasic.Code.CodeFile,Microsoft.VisualBasic.Web.HtmlPage,Microsoft.VisualBasic.General.Resource,Microsoft.WAP.VisualBasic.StyleSheet,Microsoft.VisualBasic.General.Text,Microsoft.VisualBasic.Data.XmlFile,Microsoft.VisualBasic.Data.XMLSchema,Microsoft.VisualBasic.Data.XSLTFile";

        public IVsSolution IVsSolution
        {
            get { return ( IVsSolution )GetService( typeof( IVsSolution ) ); }
        }

        private void RegisterProjectFactory( VsProjectFactory factory )
        {
            try
            {
                base.RegisterProjectFactory( factory );
            }
            catch( ArgumentException ae )
            {
                MessageCentre.InternalErrorMsg( false, String.Format( "Could not register {0} with the IDE: {1}", factory.GetType( ).Name, ae.Message ) );
            }
        }

        protected override void Initialize( )
        {
            base.Initialize( );
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            RegisterProjectFactory( new VsProjectCSharp.ProjectFactory( this ) );
            RegisterProjectFactory( new VsProjectVisualBasic.ProjectFactory( this ) );
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return null;    
        }

        public static MessageCentre MessageCentre
        {
            get
            {
                if( s_messageCentre == null )
                {
                    try
                    {
                        s_messageCentre = new MessageCentreDeployment( );
                    }
                    catch
                    {
                        return new NullMessageCentre( );
                    }
                }

                return s_messageCentre;
            }
        }

        //--//

        private static MessageCentre s_messageCentre = null;

        #region IVsInstalledProduct Members

        int IVsInstalledProduct.IdBmpSplash( out uint pIdBmp )
        {
            pIdBmp = 300;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.IdIcoLogoForAboutbox( out uint pIdIco )
        {
            pIdIco = 400;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.OfficialName( out string pbstrName )
        {
            Version version = Assembly.GetExecutingAssembly( ).GetName( ).Version;
            pbstrName = "Microsoft .NET Micro Framework v" + version.ToString( 2 );
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.ProductDetails( out string pbstrProductDetails )
        {
            Version version = Assembly.GetExecutingAssembly( ).GetName( ).Version;
            pbstrProductDetails = "Microsoft .NET Micro Framework SDK v" + version.ToString( 2 ) +
                "\r\nVersion " + version.ToString( 4 );
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.ProductID( out string pbstrPID )
        {
            //Visual Studio doesn't use this for Microsoft products; it looks for the registration info in
            //VS_REG_ROOT\Registration\%IVsMicrosoftInstalledProduct.ProductRegistryName%\ for
            //ProductID and UserName string values.
            //When we don't provide one it falls back to the Visual Studio product ID.
            pbstrPID = "";
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsMicrosoftInstalledProduct Members

        int IVsMicrosoftInstalledProduct.IdBmpSplash( out uint pIdBmp )
        {
            return ( ( IVsInstalledProduct )this ).IdBmpSplash( out pIdBmp );
        }

        int IVsMicrosoftInstalledProduct.IdIcoLogoForAboutbox( out uint pIdIco )
        {
            return ( ( IVsInstalledProduct )this ).IdIcoLogoForAboutbox( out pIdIco );
        }

        int IVsMicrosoftInstalledProduct.OfficialName( out string pbstrName )
        {
            return ( ( IVsInstalledProduct )this ).OfficialName( out pbstrName );
        }

        int IVsMicrosoftInstalledProduct.ProductDetails( out string pbstrProductDetails )
        {
            return ( ( IVsInstalledProduct )this ).ProductDetails( out pbstrProductDetails );
        }

        int IVsMicrosoftInstalledProduct.ProductID( out string pbstrPID )
        {
            return ( ( IVsInstalledProduct )this ).ProductID( out pbstrPID );
        }

        int IVsMicrosoftInstalledProduct.ProductRegistryName( out string pbstrRegName )
        {
            pbstrRegName = "Microsoft .NET Micro Framework";
            return VSConstants.S_OK;
        }

        #endregion

        internal const string NetMFPortSupplierKeyGuid = "{499dc3d0-66c1-42a2-a292-6ee253d7f708}";
    }

    internal class MSBuildProperties
    {
        public const string TargetFrameworkVersion = "TargetFrameworkVersion";
    }
}

