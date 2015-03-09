using System;
using System.Globalization;
using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.SPOT.Debugger;

    ///     This attribute registers a package load key for your package.  
    ///     Package load keys are used by Visual Studio to validate that 
    ///     a package can be loaded.    
    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    public sealed class ProvideExpressLoadKeyAttribute : RegistrationAttribute
    {

        private string _minimumEdition;
        private readonly string _productVersion;
        private readonly string _productName;
        private readonly string _companyName;

        public ProvideExpressLoadKeyAttribute( string productVersion, string productName, string companyName )
        {
            Validate.IsNotNull( productVersion, "productVersion" );
            Validate.IsNotNull( productName, "productName" );
            Validate.IsNotNull( companyName, "companyName" );

            _productVersion = productVersion;
            _productName = productName;
            _companyName = companyName;
        }

        ///  "Standard" for all express skus.
        public string MinimumEdition
        {
            get
            {
                return ( string.IsNullOrWhiteSpace( _minimumEdition ) ? "Standard" : _minimumEdition );
            }

            set
            {
                _minimumEdition = value;
            }
        }

        ///     Version of the product that this VSPackage implements.
        public string ProductVersion
        {
            get
            {
                return _productVersion;
            }
        }

        ///     Name of the product that this VSPackage delivers.
        public string ProductName
        {
            get
            {
                return _productName;
            }
        }

        ///     Creator of the VSPackage. The literal name (case-sensitive) provided 
        ///     to Microsoft when registering for a PLK.
        public string CompanyName
        {
            get
            {
                return _companyName;
            }
        }

        public short VPDExpressId { get; set; }

        public short VsWinExpressId { get; set; }

        public short VWDExpressId { get; set; }

        public short WDExpressId { get; set; }

        /// <summary>
        /// Registry Key name for this package's load key information.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string RegKeyName( RegistrationContext context )
        {
            return string.Format( CultureInfo.InvariantCulture, "Packages\\{0}", context.ComponentType.GUID.ToString( "B" ) );
        }

        /// <include file='doc\ProvideLoadKeyAttribute.uex' path='docs/doc[@for="Register"]' />
        /// <devdoc>
        ///     Called to register this attribute with the given context.  The context
        ///     contains the location where the registration information should be placed.
        ///     it also contains such as the type being registered, and path information.
        ///
        ///     This method is called both for registration and unregistration.  The difference is
        ///     that unregistering just uses a hive that reverses the changes applied to it.
        /// </devdoc>
        public override void Register( RegistrationContext context )
        {
            using( Key packageKey = context.CreateKey( RegKeyName( context ) ) )
            {
                if( VPDExpressId != 0 )
                {
                    packageKey.SetValue( "ID", VPDExpressId );
                }

                if( VsWinExpressId != 0 )
                {
                    packageKey.SetValue( "ID", VsWinExpressId );
                }

                if( VWDExpressId != 0 )
                {
                    packageKey.SetValue( "ID", VWDExpressId );
                }

                if( WDExpressId != 0 )
                {
                    packageKey.SetValue( "ID", WDExpressId );
                }

                packageKey.SetValue( "MinEdition", MinimumEdition );
                packageKey.SetValue( "ProductVersion", ProductVersion );
                packageKey.SetValue( "ProductName", ProductName );
                packageKey.SetValue( "CompanyName", CompanyName );
            }
        }

        /// <summary>
        /// Unregisters this package's load key information
        /// </summary>
        /// <param name="context"></param>
        public override void Unregister( RegistrationContext context )
        {
            context.RemoveKey( RegKeyName( context ) );
        }

    }

    // not particularly generalized but does the job without needing an MSI installer
    // values hard-coded here were extracted from the old WixVsCommon.wxs
    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    public abstract class ProvideGenericRegistrationAttribute : RegistrationAttribute
    {
        protected ProvideGenericRegistrationAttribute( string relativePath )
        {
            relativeKeyPath = relativePath;
        }

        public override void Register( RegistrationContext context )
        {
            using( var key = context.CreateKey( RelativeKeyPath ) )
            {
                foreach( var kvp in Values )
                    key.SetValue( kvp.Key, kvp.Value );
            }
        }

        public override void Unregister( RegistrationContext context )
        {
            foreach( var kvp in Values )
                context.RemoveValue( RelativeKeyPath, kvp.Key );

            context.RemoveKeyIfEmpty( RelativeKeyPath );
        }

        string RelativeKeyPath { get { return relativeKeyPath; } }
        private readonly string relativeKeyPath;
        protected abstract IReadOnlyDictionary<string,object> Values { get; }
    }

    public sealed class ProvidePortSupplierAttribute : ProvideGenericRegistrationAttribute
    {
        private readonly Type supplierType;
        
        // since the ProvideObjectAttribute class is sealed, use containment to get it's functionality
        private readonly ProvideObjectAttribute objectAttribute;

        public ProvidePortSupplierAttribute( Type portSupplier, string supplierKeyGuid)
            : base( string.Format( @"AD7Metrics\PortSupplier\{0}", supplierKeyGuid ) )
        {
            supplierType = portSupplier;
            objectAttribute = new ProvideObjectAttribute( portSupplier );
        }

        public override void Register( RegistrationContext context )
        {
            base.Register( context );
            objectAttribute.Register( context );
        }

        public override void Unregister( RegistrationContext context )
        {
            objectAttribute.Unregister( context );
            base.Unregister( context );
        }

        protected override IReadOnlyDictionary<string, object> Values
        {
            get
            {
                return new Dictionary<string, object>
                { { "CLSID", supplierType.GUID.ToString( "B" ) }
                , { "Name", "TinyCLR" }
                };
            }
        }
    }

    public sealed class ProvideDebugEngineAttribute : ProvideGenericRegistrationAttribute
    {
        public ProvideDebugEngineAttribute( )
            : base( string.Format(@"AD7Metrics\Engine\{0}", CorDebug.s_guidDebugEngine.ToString("B")) )
        {
        }

        protected override IReadOnlyDictionary<string, object> Values
        {
            get { return Values_; }
        }

        const string COMPlusOnlyDebugEngineCLSID = "{7386310B-D5CB-4369-9BDD-609B3F103914}";

        static readonly IReadOnlyDictionary<string, object> Values_ = new Dictionary<string, object>
        { { string.Empty, ".NET Micro Framework" }
        , { "Attach", 0 }
        , { "CallStackBP", 1 }
        , { "CLSID", COMPlusOnlyDebugEngineCLSID }
        , { "ConditionalBP", 1 }
        , { "DataBP", 0 }
        , { "Disassembly", 0 }
        , { "Embedded", 0 }
        , { "ENC", 0 }
        , { "EngineCanWatchProcess", 0 }
        , { "Exceptions", 1 }
        , { "FunctionBP", 1 }
        , { "HitCountBP", 1 }
        , { "InterceptCurrentException", 0 }
        , { "Interop", 0 }
        , { "JITDebug", 0 }
        , { "Name", "Managed" }
        , { "NativeInteropOK", 0 }
        , { "PortSupplier", VsPackage.NetMFPortSupplierKeyGuid }
        , { "ProgramProvider", COMPlusOnlyDebugEngineCLSID }
        , { "Registers", 0 }
        , { "RemoteDebugging", 0 }
        , { "Runtime", typeof(CorDebug).GUID.ToString("B") }
        , { "SetNextStatement", 1 }
        , { "SqlCLR", 0 }
        , { "SuspendThread", 1 }
        , { "UseShimAPI", 0 }
        };
    }
}
