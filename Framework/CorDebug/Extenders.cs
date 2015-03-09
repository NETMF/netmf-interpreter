using System;
using System.IO;
using System.Diagnostics;
using CorDebugInterop;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.Win32;
using System.Collections.Specialized;
using System.Xml;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.BuildEngine;
using EnvDTE;
using EnvDTE80;
using VSLangProj;
using VSLangProj2;
using VSLangProj80;
using VslangProj90;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.SPOT.Debugger
{
    //classes must be public to be COM-visible
    public class VsProjectExtender
        : IFilterProperties
    {
        public const string Name = "SPOTProjectExtender";

        public VsProjectExtender(object ExtendeeObject, EnvDTE.IExtenderSite ExtenderSite, int Cookie)
        {
        }

        #region IFilterProperties Members

        vsFilterProperties IFilterProperties.IsPropertyHidden(string PropertyName)
        {
            vsFilterProperties retVal = vsFilterProperties.vsFilterPropertiesNone;

            if (PropertyName == @"ApplicationIcon" ||
                PropertyName == @"AssemblyType" ||
                PropertyName == @"Win32ResourceFile" ||
                PropertyName == @"ComVisible" ||
                PropertyName == @"AssemblyGuid" ||
                PropertyName == @"ApplicationManifest" ||
                PropertyName == @"NoStdLib" ||
                PropertyName == @"MyApplication" ||
                PropertyName == @"EnableVisualStyles")
            {
                retVal = vsFilterProperties.vsFilterPropertiesAll;
            }
            else if (
                PropertyName == @"TargetFrameworkSubset")
            {
                retVal = vsFilterProperties.vsFilterPropertiesSet;
            }

            return retVal;
        }

        #endregion
    }

    //classes must be public to be COM-visible
    public class VsProjectConfigExtender
        : IFilterProperties

    {
        public const string Name = "SPOTProjectConfigExtender";

        public VsProjectConfigExtender(object ExtendeeObject, EnvDTE.IExtenderSite ExtenderSite, int Cookie)
        {
        }

        #region IFilterProperties Members

        vsFilterProperties IFilterProperties.IsPropertyHidden(string PropertyName)
        {
            vsFilterProperties retVal = vsFilterProperties.vsFilterPropertiesNone;

            if (PropertyName == @"CheckForOverflowUnderflow" ||
                PropertyName == @"StartProgram" ||
                PropertyName == @"StartWorkingDirectory" ||
                PropertyName == @"StartArguments" ||
                PropertyName == @"EnableUnmanagedDebugging" ||
                PropertyName == @"EnableSQLServerDebugging" ||
                PropertyName == @"FileAlignment" ||
                PropertyName == @"RegisterForComInterop" ||
                PropertyName == @"RemoteDebugEnabled" ||
                PropertyName == @"RemoteDebugMachine" ||
                PropertyName == @"NoStdLib" ||
                PropertyName == @"UseVSHostingProcess" ||
                PropertyName == @"GenerateSerializationAssemblies" ||
                PropertyName == @"TargetFrameworkSubset")
            {
                retVal = vsFilterProperties.vsFilterPropertiesAll;
            }
            else if
                (PropertyName == @"PlatformTarget")
            {
                retVal = vsFilterProperties.vsFilterPropertiesSet;
            }

            return retVal;
        }

        #endregion
    }

}