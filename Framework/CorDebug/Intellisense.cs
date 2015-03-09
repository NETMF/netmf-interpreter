using System;
using System.IO;
using System.Diagnostics;
using CorDebugInterop;
using System.Collections;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.SPOT.Debugger
{
    public class IntellisenseFilter : IVsContextualIntellisenseFilter
    {
        VsProject m_project;

        public IntellisenseFilter( VsProject project )
        {
            m_project = project;
        }

        private bool HasGenerics( string name )
        {
            if(name.IndexOf( '`' ) >= 0)
            {
                return true;
            }

            return false;
        }

        #region IVsContextualIntellisenseFilter Members

        int IVsContextualIntellisenseFilter.Close()
        {
            return Utility.COM_HResults.S_OK;
        }

        int IVsContextualIntellisenseFilter.Initialize( IVsHierarchy pHierarchy )
        {
            return Utility.COM_HResults.S_OK;
        }

        int IVsContextualIntellisenseFilter.IsMemberVisible( string szMemberSignature, out int pfVisible )
        {
            bool fVisible = true;

            if(HasGenerics( szMemberSignature ))
            {
                fVisible = false;
            }

            pfVisible = Utility.Boolean.BoolToInt( fVisible );
            return Utility.COM_HResults.S_OK;
        }

        int IVsContextualIntellisenseFilter.IsTypeVisible( string szTypeName, out int pfVisible )
        {
            bool fVisible = true;

            if(HasGenerics( szTypeName ))
            {
                fVisible = false;
            }

            pfVisible = Utility.Boolean.BoolToInt( fVisible );
            return Utility.COM_HResults.S_OK;
        }

        #endregion
    }
}