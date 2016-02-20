using System;
using System.IO;
using System.Linq;
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
using EnvDTE80;
using EnvDTE;
using VSLangProj;
using VSLangProj2;
using VSLangProj80;
using VslangProj90;
using VSLangProj110;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.SPOT.Debugger
{
    [ComVisible( true )]
    [ClassInterface( ClassInterfaceType.None )]
    public class VsProject : FlavoredProjectBase,
        IVsBuildPropertyStorage,
        IVsProjectFlavorCfgProvider,
        IVsHierarchy,
        IVsSingleFileGeneratorFactory,
        IVsComponentUser,
        IVsProject,
        IOleCommandTarget,
        IInternalExtenderProvider,
        IVsProjectSpecialFiles
    {
        private VsPackage m_package;
        protected IVsProject m_innerIVsProject;
        protected IVsProjectBuildSystem m_innerIVsProjectBuildSystem;
        protected IVsBuildPropertyStorage m_innerIVsBuildPropertyStorage;
        protected IVsHierarchy m_innerIVsHierarchy;
        protected IVsUIHierarchy m_innerIVsUIHierarchy;
        protected IVsSingleFileGeneratorFactory m_innerSingleFileGeneratorFactory;
        protected IVsProjectSpecialFiles m_innerProjectSpecialFiles;

        /*Due to lots of problems encountered with the icons, we are duplicating the base ImageList and keeping a copy
         * of our own.  This only presents a problem if the base project will change images in the ImageList after we get
         * a copy.  However, I don't think that is the case*/
        private IntPtr m_hImgList;
        private int m_iconIndex;
        private VSLangProj.ProjectProperties m_projectProperties;
        private BuildHost m_buildHost;
        private AddReferenceState m_addReferenceState;
        private string m_cachedTargetFrameworkVersion;

        private string m_CATIDProjectBrowseObject;
        private string m_CATIDProjectConfigBrowseObject;

        private VsProjectExtender m_projectExtender;
        private Dictionary<object, VsProjectConfigExtender> m_projectConfigExtenderMap;

        #region HelperClasses

        private class AddReferenceState
        {
            public uint xDlgSize = 0;
            public uint yDlgSize = 0;
            public Guid lastActiveTab = Guid.Empty;
            public string startBrowse = null;
            public bool fDialogActive = false;
        }

        public class BuildHost : Microsoft.Build.Framework.ITaskHost
        {
            public const string c_RESOLVE_RUNTIME_DEPENDENCIES_TARGET = "ResolveRuntimeDependencies";
            public const string c_RESOLVE_RUNTIME_DEPENDENCIES_TARGETLE = "ResolveRuntimeDependenciesLE";
            public const string c_RESOLVE_RUNTIME_DEPENDENCIES_TARGETBE = "ResolveRuntimeDependenciesBE";
            public const string c_RESOLVE_RUNTIME_DEPENDENCIES_TASK = "ResolveRuntimeDependencies";
            public const string c_RESOLVE_REFERENCES_TARGET = "ResolveReferences";

            private string[] m_assemblyReferences;
            private string m_assembly;
            private string m_startProgam;
            private string[] m_startProgramReferences;

            private void AddDependencies( Hashtable table, string[] paths )
            {
                if(paths != null)
                {
                    foreach(string path in paths)
                    {
                        if(path != null)
                        {
                            string filename = Path.GetFileName( path );

                            // When changing target frameworks in VS2010, sometimes VS tells us we are dependent
                            // on a GAC version of mscorlib (in addition to our own mscorlib), so ignore it.
                            if (path.ToLower().Contains("\\gac\\") || path.ToLower().Contains("\\gac_64\\"))
                            {
                                continue;
                            }

                            if(table[filename] != null)
                            {
                                //nop for now....otherwise need to check and make sure everything is kosher,
                                //need to check for versioning here?
                            }
                            else
                            {
                                table[filename] = path;
                            }
                        }
                    }
                }
            }

            public bool HasBeenBuilt
            {
                get { return this.m_assembly != null; }
            }

            public string[] GetDependencies( bool fIncludeStartProgram, bool fPE, bool fTargetIsBigEndian, string frameworkVersion )
            {
                Hashtable table = new Hashtable();

                //Make sure this runs even for RunWithoutBuilding....how to do that???
                AddDependencies( table, new string[] { this.m_assembly } );
                AddDependencies( table, this.m_assemblyReferences );

                if(fIncludeStartProgram)
                {
                    AddDependencies( table, new string[] { this.m_startProgam } );
                    AddDependencies( table, this.m_startProgramReferences );
                }
                // if mscorlib isn't in the list of assemblies, add it as it's always required
                // the build system in some versions of VS+MSBuild will assume that, while other
                // versions will explicitly reference it with the InProject item property set to
                // false so it won't show in the IDE. By testing for it and adding it if not found
                // both cases are covered.
                var q = from path in table.OfType<string>( )
                        where string.Equals( Path.GetFileName( path ), "mscorlib.dll", StringComparison.OrdinalIgnoreCase )
                        select path;
                if( !q.Any())
                {
                    PlatformInfo pi = new PlatformInfo(frameworkVersion);
                    AddDependencies( table, new string[] { Path.Combine( pi.FrameworkAssembliesPath, "mscorlib.dll" ) } );
                }

                ArrayList deps = new ArrayList( table.Values );

                string[] depsArray = (string[])deps.ToArray( typeof( string ) );

                if(fPE)
                {
                    for(int iAssembly = 0; iAssembly < depsArray.Length; iAssembly++)
                    {
                        depsArray[iAssembly] = Utility.FindPEForDll(depsArray[iAssembly], fTargetIsBigEndian, frameworkVersion);
                    }
                }

                return depsArray;
            }

            public string Assembly
            {
                set { this.m_assembly = value; }
            }

            public string[] AssemblyReferences
            {
                set { this.m_assemblyReferences = value; }
            }

            public string StartProgram
            {
                set { this.m_startProgam= value; }
            }

            public string[] StartProgramReferences
            {
                set { this.m_startProgramReferences = value; }
            }
        }

        public class SiteServiceProvider : IOleServiceProvider
        {
            IOleServiceProvider m_spInner;
            VsProject m_project;

            public SiteServiceProvider(VsProject project, IOleServiceProvider spInner)
            {
                m_project = project;
                m_spInner = spInner;
            }

            #region IOleServiceProvider Members

            int IOleServiceProvider.QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject)
            {
                int hr = Utility.COM_HResults.S_OK;

                if (guidService.Equals(typeof(SVsTargetFrameworkAssemblies).GUID))
                {
                    IVsTargetFrameworkAssemblies frameworkAssemblies = new TargetFrameworkAssemblies(m_project);
                    ppvObject = Marshal.GetComInterfaceForObject(frameworkAssemblies, typeof(IVsTargetFrameworkAssemblies));
                }
                if (guidService.Equals(typeof(SVsFrameworkMultiTargeting).GUID))
                {
                    IVsFrameworkMultiTargeting multiTargetting = new NetMFMultiTargeting(m_project);
                    ppvObject = Marshal.GetComInterfaceForObject(multiTargetting, typeof(IVsFrameworkMultiTargeting));
                }
                else
                {
                    hr = m_spInner.QueryService(ref guidService, ref riid, out ppvObject);
                }

                return hr;
            }

            #endregion
        }

        public class ContextServiceProvider : IOleServiceProvider, IVsBrowseObject
        {
            IOleServiceProvider m_spInner;
            IVsBrowseObject m_boInner;
            VsProject m_project;

            public ContextServiceProvider( VsProject project, IOleServiceProvider spInner )
            {
                m_project = project;
                m_spInner = spInner;
                m_boInner = spInner as IVsBrowseObject;

                Debug.Assert( m_boInner != null );
            }

            #region IOleServiceProvider Members

            int IOleServiceProvider.QueryService( ref Guid guidService, ref Guid riid, out IntPtr ppvObject )
            {
                int hr = Utility.COM_HResults.S_OK;

                if(guidService.Equals( typeof( IVsContextualIntellisenseFilter ).GUID ))
                {
                    IVsContextualIntellisenseFilter filter = new IntellisenseFilter( m_project );
                    ppvObject = Marshal.GetComInterfaceForObject( filter, typeof( IVsContextualIntellisenseFilter ) );
                }
                else
                {
                    hr = m_spInner.QueryService( ref guidService, ref riid, out ppvObject );
                }

                return hr;
            }

            #endregion

            #region IVsBrowseObject Members

            int IVsBrowseObject.GetProjectItem( out IVsHierarchy pHier, out uint pItemid )
            {
                return m_boInner.GetProjectItem( out pHier, out pItemid );
            }

            #endregion
}

        #endregion HelperClasses

        internal VsPackage Package
        {
            get { return m_package; }
            set { m_package = value; }
        }

        public VsProject()
        {
            m_cachedTargetFrameworkVersion = Utility.MinimumVersionString(Assembly.GetExecutingAssembly().GetName().Version);
            m_projectConfigExtenderMap = new Dictionary<object, VsProjectConfigExtender>();
        }

        ~VsProject()
        {
            if(m_hImgList != IntPtr.Zero)
                Utility.ComCtl32.ImageList_Destroy( m_hImgList );
            m_hImgList = IntPtr.Zero;
        }

        void InitBuildHost()
        {
            if(m_buildHost == null)
            {
                m_buildHost = new BuildHost();
            }
        }

        public VSLangProj.ProjectProperties ProjectProperties
        {
            get
            {
                if(m_projectProperties == null)
                {
                    m_projectProperties = (VSLangProj.ProjectProperties)GetProperty( VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_BrowseObject );
                }

                return m_projectProperties;
            }
        }

        [DebuggerHidden]
        public object GetPropertyInner( uint itemid, int propid )
        {
            object ret;
            int hr = base.GetProperty( itemid, propid, out ret );
            Debug.Assert( Utility.COM_HResults.SUCCEEDED( hr ) );
            return ret;
        }

        [DebuggerHidden]
        public object GetProperty( uint itemid, int propid )
        {
            object ret;
            int hr = GetProperty( itemid, propid, out ret );
            Debug.Assert( Utility.COM_HResults.SUCCEEDED( hr ) );
            return ret;
        }

        protected virtual string ProjectNodeIconResource
        {
            get { return null; }
        }

        private IntPtr EnsureImageList()
        {
            if(m_hImgList == IntPtr.Zero)
            {
                IntPtr hImgList = (IntPtr)(int)GetPropertyInner( VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_IconImgList );

                m_hImgList = Utility.ComCtl32.ImageList_Duplicate( hImgList );

                string iconResource = this.ProjectNodeIconResource;

                if (!string.IsNullOrEmpty(iconResource))
                {
                    int cx, cy;

                    if (Utility.ComCtl32.ImageList_GetIconSize(hImgList, out cx, out cy))
                    {
                        Stream stream = GetType().Module.Assembly.GetManifestResourceStream(GetType(), iconResource);

                        if (stream != null)
                        {
                            Icon icon = new Icon(stream, cx, cy);

                            if (icon != null)
                            {
                                m_iconIndex = Utility.ComCtl32.ImageList_AddIcon(m_hImgList, icon.Handle);
                            }

                            //Make sure icon.Handle isn't released before adding it to the image list.
                            GC.KeepAlive(icon);
                        }
                    }
                }
            }

            Debug.Assert( m_hImgList != IntPtr.Zero );
            return m_hImgList;
        }

        public bool CanLaunch
        {
            get
            {
                bool fCanLaunch = false;

                switch(this.ProjectProperties.OutputType)
                {
                    case VSLangProj.prjOutputType.prjOutputTypeWinExe:
                    case VSLangProj.prjOutputType.prjOutputTypeExe:
                        fCanLaunch = true;
                        break;
                    case VSLangProj.prjOutputType.prjOutputTypeLibrary:
                        fCanLaunch = (this.ProjectProperties.ActiveConfigurationSettings.StartAction == VSLangProj.prjStartAction.prjStartActionProgram && File.Exists( this.ProjectProperties.ActiveConfigurationSettings.StartProgram ));
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }

                return fCanLaunch;
            }
        }

        public string[] GetDependencies(bool fIncludeStartProgram, bool fPE, bool fTargetIsBigEndian)
        {
            string frameworkVersion = GetTargetFrameworkProperty();

            if (m_buildHost == null)
            {
                m_buildHost = new BuildHost();
            }

            if (!m_buildHost.HasBeenBuilt)
            {
                bool fSuccess;
                int err;
                Version ver = new Version(frameworkVersion.TrimStart('v'));

                err = m_innerIVsProjectBuildSystem.SetHostObject(
                        ((ver.Major == 4 && ver.Minor >= 1) || ver.Major > 4) ? fTargetIsBigEndian ? BuildHost.c_RESOLVE_RUNTIME_DEPENDENCIES_TARGETBE : BuildHost.c_RESOLVE_RUNTIME_DEPENDENCIES_TARGETLE : BuildHost.c_RESOLVE_RUNTIME_DEPENDENCIES_TARGET,
                        BuildHost.c_RESOLVE_RUNTIME_DEPENDENCIES_TASK,
                        m_buildHost);
                err = m_innerIVsProjectBuildSystem.BuildTarget(BuildHost.c_RESOLVE_REFERENCES_TARGET, out fSuccess);
            }

            return m_buildHost.GetDependencies(fIncludeStartProgram, fPE, fTargetIsBigEndian, frameworkVersion);
        }

        private enum PropPageSubset
        {
            Config_agnostic,
            Config_specific,
            Preferred
        }

        private string FilterProjectPropertyPages(string property, PropPageSubset subset)
        {
            Utility.CLSIDList clsidList = new Utility.CLSIDList((string)property);

            //Signing tab does not work for two reasons.  One, our cfg does not implement IVsPublishableProjectCfg
            //The designer fails in this case.
            //Secondly, we are not currently signing our assemblies.  So trying to sign an applicaiton assembly
            //that references the unsigned mscorlib fails. So this property page doesn't work for the MicroFramework.
            clsidList.Remove(new Guid("F8D6553F-F752-4DBF-ACB6-F291B744A792")); //Signing
            clsidList.Remove(new Guid("DF8F7042-0BB1-47D1-8E6D-DEB3D07698BD")); //Security
            clsidList.Remove(new Guid("CC4014F5-B18D-439C-9352-F99D984CCA85")); //Publish
            clsidList.Remove(new Guid("984AE51A-4B21-44E7-822C-DD5E046893EF")); //Code Analysis
            clsidList.Remove(new Guid("43E38D2E-43B8-4204-8225-9357316137A4")); //Services
            clsidList.Remove(new Guid("6D2695F9-5365-4A78-89ED-F205C045BFE6")); //Settings

            switch (subset)
            {
                case PropPageSubset.Config_specific:
                    clsidList.Append(typeof(PropertyPageComObject).GUID);
                    break;
                case PropPageSubset.Config_agnostic:
                    break;
                default:
                    clsidList.Append(typeof(PropertyPageComObject).GUID);
                    break;
            }

            return clsidList.ToString();
        }

        protected virtual Guid AddItemTemplatesGuid
        {
            get { return Guid.Empty; }
        }

        protected override int GetProperty( uint itemId, int propId, out object property )
        {
            property = null;

            int hr = base.GetProperty( itemId, propId, out property );
            bool fHandled = false;

            if(itemId == VSConstants.VSITEMID_ROOT)
            {
                switch(propId)
                {
                    case (int)__VSHPROPID.VSHPROPID_DefaultEnableDeployProjectCfg:
                        property = this.CanLaunch;
                        fHandled = true;
                        break;
                    case (int)__VSHPROPID.VSHPROPID_IconImgList:
                        property = EnsureImageList().ToInt32();
                        fHandled = true;
                        break;
                    case (int)__VSHPROPID.VSHPROPID_IconIndex:
                    case (int)__VSHPROPID.VSHPROPID_OpenFolderIconIndex:
                        EnsureImageList();
                        if (m_iconIndex != 0)
                        {
                            property = m_iconIndex;
                            fHandled = false;
                        }
                        break;
                    case (int)__VSHPROPID.VSHPROPID_ShowProjInSolutionPage:
                        property = (m_addReferenceState == null || !m_addReferenceState.fDialogActive);
                        fHandled = true;
                        break;
                    case (int)__VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList:
                        property = FilterProjectPropertyPages((string)property, PropPageSubset.Config_specific);
                        fHandled = true;
                        break;
                    case (int)__VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList:
                        property = FilterProjectPropertyPages((string)property, PropPageSubset.Config_agnostic);
                        fHandled = true;
                        break;
                    case (int)__VSHPROPID2.VSHPROPID_PriorityPropertyPagesCLSIDList:
                        property = FilterProjectPropertyPages((string)property, PropPageSubset.Preferred);
                        fHandled = true;
                        break;

                    case (int)__VSHPROPID4.VSHPROPID_TargetFrameworkMoniker:
                        property = ".NETMicroFramework,Version=" + GetTargetFrameworkProperty();
                        fHandled = true;
                        break;
                }
            }

            if (fHandled)
            {
                hr = Utility.COM_HResults.S_OK;
            }

            return hr;
        }

        protected override int SetProperty(uint itemId, int propId, object property)
        {
            int hr = Utility.COM_HResults.S_OK;
            try
            {
                hr = base.SetProperty(itemId, propId, property);
            }
            catch
            {
            }

            return hr;
        }

        protected override int QueryStatusCommand( uint itemid, ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText )
        {
            if(QueryStatusHelper( itemid, ref pguidCmdGroup, cCmds, prgCmds, pCmdText ))
                return Utility.COM_HResults.S_OK;
            else
                return base.QueryStatusCommand( itemid, ref pguidCmdGroup, cCmds, prgCmds, pCmdText );
        }

        private VsProjectFlavorCfg ActiveCfg
        {
            get
            {
                IVsSolutionBuildManager buildMgr = (IVsSolutionBuildManager)ServiceProvider.GlobalProvider.GetService( typeof( IVsSolutionBuildManager ) );
                IVsProjectCfg[] cfg = new IVsProjectCfg[1];

                buildMgr.FindActiveProjectCfg( IntPtr.Zero, IntPtr.Zero, this, cfg );

                IntPtr pCfg;
                Guid iidCfg = typeof( IVsDebuggableProjectCfg ).GUID;
                ((IVsProjectCfg2)cfg[0]).get_CfgType( ref iidCfg, out pCfg );

                return Marshal.GetObjectForIUnknown( pCfg ) as VsProjectFlavorCfg;
            }
        }

        private VSCOMPONENTSELECTORTABINIT CreateComponentSelectorTab( string guid, object initInfo )
        {
            VSCOMPONENTSELECTORTABINIT tab = new VSCOMPONENTSELECTORTABINIT();
            tab.dwSize = (uint)Marshal.SizeOf( tab );
            tab.guidTab = new Guid( guid );
            tab.varTabInitInfo = initInfo;

            return tab;
        }

        private int ExecCommandAddReference( uint itemid, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut )
        {
            int hr = Utility.COM_HResults.S_OK;

            VSCOMPONENTSELECTORTABINIT[] tabinit = new VSCOMPONENTSELECTORTABINIT[]
            {
                CreateComponentSelectorTab(ComponentSelectorGuids80.COMPlusPage, this.ActiveCfg.PlatformInfo.AssemblyFoldersList),
                CreateComponentSelectorTab(ComponentSelectorGuids80.SolutionPage,(int)__VSHPROPID.VSHPROPID_ShowProjInSolutionPage),
                CreateComponentSelectorTab(ComponentSelectorGuids80.MRUPage, null),
                CreateComponentSelectorTab(ComponentSelectorGuids80.BrowseFilesPage, null),
            };

            IVsComponentSelectorDlg2 compSelDlg = ( IVsComponentSelectorDlg2 )ServiceProvider.GlobalProvider.GetService( typeof( IVsComponentSelectorDlg ) );

            if(m_addReferenceState == null)
            {
                m_addReferenceState = new AddReferenceState();
                GetCanonicalName( VSConstants.VSITEMID_ROOT, out m_addReferenceState.startBrowse);
                m_addReferenceState.lastActiveTab = new Guid( ComponentSelectorGuids80.COMPlusPage );
            }

            m_addReferenceState.fDialogActive = true;
            hr = compSelDlg.ComponentSelectorDlg2( (uint)__VSCOMPSELFLAGS2.VSCOMSEL2_MultiSelectMode, this, 0, null, "Add Reference" /*get VS localized string?*/, "VS.AddReference",
                ref m_addReferenceState.xDlgSize, ref m_addReferenceState.yDlgSize, (uint)tabinit.Length, tabinit, ref m_addReferenceState.lastActiveTab, "Component Files (*.exe *.dll)\0*.exe;*.dll\0", ref m_addReferenceState.startBrowse );
            m_addReferenceState.fDialogActive = false;

            return hr;
        }

        protected override int ExecCommand(uint itemid, ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
           if (Guid.Equals(pguidCmdGroup, VSConstants.VSStd2K))
            {
                switch (nCmdID)
                {
                    case (uint)VSConstants.VSStd2KCmdID.ADDREFERENCE:
                        return ExecCommandAddReference(itemid, nCmdexecopt, pvaIn, pvaOut);
                }
            }

            return base.ExecCommand(itemid, ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        protected override void SetInnerProject(IntPtr innerIUnknown)
        {
            //
            // This was changed to support VS2012 - The FlavoredProject base class we were using
            // was throwing a COM exception, so we switched to use the newer FlavoredProjectBase class
            // which uses an IntPtr instead of a managed object for this class.  The following method
            // converts the IUnknown IntPtr into a managed object from which we can convert into the
            // following interfaces.
            //
            object inner = Marshal.GetUniqueObjectForIUnknown(innerIUnknown);

            m_innerIVsProject = inner as IVsProject;
            m_innerIVsProjectBuildSystem = inner as IVsProjectBuildSystem;
            m_innerIVsHierarchy = inner as IVsHierarchy;
            m_innerIVsUIHierarchy = inner as IVsUIHierarchy;
            m_innerSingleFileGeneratorFactory = inner as IVsSingleFileGeneratorFactory;
            m_innerIVsBuildPropertyStorage = inner as IVsBuildPropertyStorage;
            m_innerProjectSpecialFiles = inner as IVsProjectSpecialFiles;

            Debug.Assert(m_innerIVsProject                 != null);
            Debug.Assert(m_innerIVsProjectBuildSystem      != null);
            Debug.Assert(m_innerIVsHierarchy               != null);
            Debug.Assert(m_innerIVsUIHierarchy             != null);
            Debug.Assert(m_innerSingleFileGeneratorFactory != null);
            Debug.Assert(m_innerIVsBuildPropertyStorage    != null);
            Debug.Assert(m_innerProjectSpecialFiles        != null);

            if (this.serviceProvider == null)
            {
                this.serviceProvider = (System.IServiceProvider)this.m_package;
            }

            base.SetInnerProject(innerIUnknown);
        }

        protected override void InitializeForOuter(string fileName, string location, string name, uint flags, ref Guid guidProject, out bool cancel)
        {
            int hr;
            base.InitializeForOuter(fileName, location, name, flags, ref guidProject, out cancel);

            VsPackage.MessageCentre.InternalErrorMsg(m_innerIVsUIHierarchy != null, "Inner project has no IVsUIHierarchy");

            object obj = null;
            hr = m_innerIVsUIHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID2.VSHPROPID_BrowseObjectCATID, out obj);
            VsPackage.MessageCentre.InternalErrorMsg(ErrorHandler.Succeeded(hr), "Project unable to get the BrowseObject CATID.");
            m_CATIDProjectBrowseObject = obj as string;
            VsPackage.MessageCentre.InternalErrorMsg(!string.IsNullOrEmpty(m_CATIDProjectBrowseObject),
                                                     "Project BrowseObject CATID value was unexpected.");

            obj = null;
            hr = m_innerIVsUIHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID2.VSHPROPID_CfgBrowseObjectCATID, out obj);
            VsPackage.MessageCentre.InternalErrorMsg(ErrorHandler.Succeeded(hr), "Project unable to get the CfgBrowseObject CATID.");
            m_CATIDProjectConfigBrowseObject = obj as string;
            VsPackage.MessageCentre.InternalErrorMsg(!string.IsNullOrEmpty(m_CATIDProjectConfigBrowseObject),
                                                     "Project CfgBrowseObject CATID value was unexpected.");
        }

        private EnvDTE.Project GetProjectFromHierarchy( IVsHierarchy hier )
        {
            object extObject;
            hier.GetProperty( (uint)VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject );

            return extObject as EnvDTE.Project;
        }

        internal EnvDTE.Project DTE_Project
        {
            get
            {
                return GetProjectFromHierarchy( this );
            }
        }

        internal VSLangProj.VSProject VSLangProj_VSProject
        {
            get
            {
                return this.DTE_Project.Object as VSLangProj.VSProject;
            }
        }

        internal VSLangProj.References VSLangProj_References
        {
            get
            {
                return this.VSLangProj_VSProject.References;
            }
        }

        internal IVsSolution IVsSolution
        {
            get
            {
                return ServiceProvider.GlobalProvider.GetService( typeof( IVsSolution ) ) as IVsSolution;
            }
        }

        internal IVsBuildPropertyStorage InnerBuildPropertyStorage
        {
            get { return m_innerIVsBuildPropertyStorage; }
        }

        private bool QueryStatusHelper( uint itemid, ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText )
        {
            Debug.Assert( cCmds == 1 );

            bool fHandled = false;
            uint cmdID = prgCmds[0].cmdID;

            if(pguidCmdGroup.Equals( VSConstants.VSStd2K ))
            {
                switch((VSConstants.VSStd2KCmdID)cmdID)
                {
                    case VSConstants.VSStd2KCmdID.ADDWFCFORM:
                    case VSConstants.VSStd2KCmdID.ADDTBXCOMPONENT:
                    case VSConstants.VSStd2KCmdID.UPDATEWEBREFERENCE:
                    case VSConstants.VSStd2KCmdID.ADDWEBREFERENCE:
                    case VSConstants.VSStd2KCmdID.ADDWEBREFERENCECTX:
                    case VSConstants.VSStd2KCmdID.ADDUSERCONTROL:
                    case VSConstants.VSStd2KCmdID.StaticAnalysisOnlyProject:
                    case VSConstants.VSStd2KCmdID.ECMD_RUNFXCOPSEL:
                    case (VSConstants.VSStd2KCmdID)Utility.VsConstants.VSStd2KCmdID.ECMD_RUNFXCOPPROJCTX:
                        prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE);
                        fHandled = true;
                        break;
                }
            }
            else if(pguidCmdGroup.Equals( DebuggerInterop.Constants.guidVSDebugCommand ))
            {
                switch((DebuggerInterop.Constants.VSDebugCmdID)cmdID)
                {
                    case DebuggerInterop.Constants.VSDebugCmdID.GoToDisassembly:
                    case DebuggerInterop.Constants.VSDebugCmdID.ToggleDisassembly:
                    case DebuggerInterop.Constants.VSDebugCmdID.BreakpointsWindowGoToDisassembly:
                    case DebuggerInterop.Constants.VSDebugCmdID.ListDisassembly:
                    case DebuggerInterop.Constants.VSDebugCmdID.DisasmWindowShow:
                    case DebuggerInterop.Constants.VSDebugCmdID.RegisterWindowShow:
                    case DebuggerInterop.Constants.VSDebugCmdID.ListRegisters:
                    case DebuggerInterop.Constants.VSDebugCmdID.RegWinGroupFirst:
                        {
                            prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE);
                            fHandled = true;
                            break;
                        }
                }
            }

            return fHandled;
        }

        private NameValueCollection ParseGeneratorProgID( ref string progID )
        {
            NameValueCollection coll = null;

            progID = progID.Trim();

            if (progID.Length > 1 && progID[progID.Length - 1] == ')')
            {
                int iLeftParen = progID.IndexOf( '(' );
                if(iLeftParen >= 0)
                {
                    coll = new NameValueCollection();

                    string args = progID.Substring( iLeftParen + 1, progID.Length - iLeftParen - 2 );
                    progID = progID.Substring( 0, iLeftParen );

                    string[] nvpairs = args.Split( ',' );

                    for(int i = 0; i < nvpairs.Length; i++)
                    {
                        string nvpair = nvpairs[i];

                        string[] nv = nvpair.Split( '=' );

                        if(nv.Length > 2)
                        {
                            throw new ArgumentException();
                        }

                        string name = nv[0];
                        string value = string.Empty;

                        if(nv.Length > 1)
                        {
                            value = nv[1];
                        }

                        coll.Add( name, value );
                    }
                }
            }

            return coll;
        }

        internal string GetTargetFrameworkProperty()
        {
            int err = Utility.COM_HResults.S_OK;

            try
            {
                string fxVer = null;
                err = this.GetTargetFrameworkProperty(out fxVer);
                if (err == Utility.COM_HResults.S_OK)
                {
                    return fxVer;
                }
                else
                {
                    VsPackage.MessageCentre.InternalErrorMsg(false, String.Format("Error while getting target framework property (error code was {0:X}).", err));
                    return m_cachedTargetFrameworkVersion;
                }
            }
            catch (Exception ex)
            {
                VsPackage.MessageCentre.InternalErrorMsg(false, String.Format("Error while getting target framework property (Exception was: \"{0}\").", ex.Message));
                return m_cachedTargetFrameworkVersion;
            }
        }

        private int GetTargetFrameworkProperty(out uint targetFxVer)
        {
            string tgtFxVerStr = null;
            targetFxVer = 0;

            try
            {
                int err = this.GetTargetFrameworkProperty(out tgtFxVerStr);
                if (err != Utility.COM_HResults.S_OK)
                    return err;

                targetFxVer = Utility.VersionToUint(Utility.VersionFromPropertyString(tgtFxVerStr));
                return Utility.COM_HResults.S_OK;
            }
            catch (Exception)
            {
                return Utility.COM_HResults.E_FAIL;
            }
        }

        private int GetTargetFrameworkProperty(out string targetFxVer)
        {
            int err = Utility.COM_HResults.S_OK;
            targetFxVer = null;

            try
            {
                IVsBuildPropertyStorage bps = this as IVsBuildPropertyStorage;

                err = bps.GetPropertyValue(@"TargetFrameworkVersion",
                           String.Empty,
                           (uint)_PersistStorageType.PST_PROJECT_FILE, out targetFxVer);
            }
            catch (Exception ex)
            {
                VsPackage.MessageCentre.InternalErrorMsg(false, String.Format("Error while getting target framework property (Exception was: \"{0}\").", ex.Message));
                err = Utility.COM_HResults.E_FAIL;
            }

            return err;
        }


        private int SetTargetFrameworkProperty(uint targetFxVerUint)
        {
            try
            {
                string s = "v" + Utility.VersionFromUint(targetFxVerUint).ToString(2);
                m_buildHost = new BuildHost();
                return this.SetTargetFrameworkProperty(s);
            }
            catch (Exception ex)
            {
                VsPackage.MessageCentre.InternalErrorMsg(false, String.Format("Error while setting target framework property to int \"{0:X}\" (Exception was: \"{1}\").", targetFxVerUint, ex.Message));
                return Utility.COM_HResults.E_FAIL;
            }
        }

        private int SetTargetFrameworkProperty(string targetFxVersion)
        {
            int err = Utility.COM_HResults.S_OK;

            // We don't seem to need to do as desktop VC# does, unloading and reloading the project when the tgtFx property changes
            try
            {
                IVsBuildPropertyStorage bps = this as IVsBuildPropertyStorage;

                err = m_innerIVsBuildPropertyStorage.SetPropertyValue(@"TargetFrameworkVersion",
                    String.Empty,
                    (uint)_PersistStorageType.PST_PROJECT_FILE, targetFxVersion);
            }
            catch (Exception ex)
            {
                VsPackage.MessageCentre.InternalErrorMsg(false, String.Format("Error while setting target framework property to \"{0}\" (Exception was: \"{1}\").", targetFxVersion, ex.Message));

                err = Utility.COM_HResults.E_FAIL;
            }

            return err;
        }

        internal static bool TargetFrameworkExactMatch( Debugger.WireProtocol.Commands.Debugging_Resolve_Assembly.Version left, Version right)
        {
            if(left.iMajorVersion   == right.Major         &&
               left.iMinorVersion   == right.Minor         &&
               left.iBuildNumber    == right.Build         &&
               left.iRevisionNumber == right.Revision
               )
            {
                return true;
            }

            return false;
        }

        internal string TargetFrameworkVersion
        {
            get
            {
                return this.GetTargetFrameworkProperty();
            }

            set
            {
                if (Utility.COM_HResults.S_OK != this.SetTargetFrameworkProperty(value))
                    throw new Exception(String.Format("Cannot set target framework to \"{0}\"", value));
            }
        }


        #region IVsProjectFlavorCfgProvider Members

        public int CreateProjectFlavorCfg( IVsCfg pBaseProjectCfg, out IVsProjectFlavorCfg ppFlavorCfg )
        {
            InitBuildHost();

            ppFlavorCfg = new VsProjectFlavorCfg( this, pBaseProjectCfg );

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #region IVsHierarchy Members

        int IVsHierarchy.AdviseHierarchyEvents( IVsHierarchyEvents pEventSink, out uint pdwCookie )
        {
            return m_innerIVsHierarchy.AdviseHierarchyEvents( pEventSink, out pdwCookie );
        }

        int IVsHierarchy.Close()
        {
            return m_innerIVsHierarchy.Close();
        }

        int IVsHierarchy.GetCanonicalName( uint itemid, out string pbstrName )
        {
            return m_innerIVsHierarchy.GetCanonicalName( itemid, out pbstrName );
        }

        int IVsHierarchy.GetGuidProperty( uint itemid, int propid, out Guid pguid )
        {
            pguid = Guid.Empty;

            bool fHandled = false;
            int hr = Utility.COM_HResults.S_OK;

            if (itemid == VSConstants.VSITEMID_ROOT)
            {
                switch (propid)
                {
                    case (int)__VSHPROPID2.VSHPROPID_AddItemTemplatesGuid:
                        pguid = this.AddItemTemplatesGuid;
                        fHandled = !Guid.Empty.Equals(pguid);
                        break;
                }
            }

            if (!fHandled)
            {
                hr = m_innerIVsHierarchy.GetGuidProperty(itemid, propid, out pguid);
            }

            return hr;
        }

        int IVsHierarchy.GetNestedHierarchy( uint itemid, ref Guid iidHierarchyNested, out IntPtr ppHierarchyNested, out uint pitemidNested )
        {
            return m_innerIVsHierarchy.GetNestedHierarchy( itemid, ref iidHierarchyNested, out ppHierarchyNested, out pitemidNested );
        }

        int IVsHierarchy.GetProperty( uint itemid, int propid, out object pvar )
        {
            return this.GetProperty( itemid, propid, out pvar );
        }

        int IVsHierarchy.GetSite( out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP )
        {
            int hr = m_innerIVsHierarchy.GetSite( out ppSP );
            ppSP = new SiteServiceProvider(this, ppSP);
            return hr;
        }

        int IVsHierarchy.ParseCanonicalName( string pszName, out uint pitemid )
        {
            return m_innerIVsHierarchy.ParseCanonicalName( pszName, out pitemid );
        }

        int IVsHierarchy.QueryClose( out int pfCanClose )
        {
            return m_innerIVsHierarchy.QueryClose( out pfCanClose );
        }

        int IVsHierarchy.SetGuidProperty( uint itemid, int propid, ref Guid rguid )
        {
            return m_innerIVsHierarchy.SetGuidProperty( itemid, propid, ref rguid );
        }

        int IVsHierarchy.SetProperty( uint itemid, int propid, object var )
        {
            return this.SetProperty(itemid, propid, var);
        }

        int IVsHierarchy.SetSite( Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp )
        {
            return m_innerIVsHierarchy.SetSite( psp );
        }

        int IVsHierarchy.UnadviseHierarchyEvents( uint dwCookie )
        {
            return m_innerIVsHierarchy.UnadviseHierarchyEvents( dwCookie );
        }

        int IVsHierarchy.Unused0()
        {
            return m_innerIVsHierarchy.Unused0();
        }

        int IVsHierarchy.Unused1()
        {
            return m_innerIVsHierarchy.Unused1();
        }

        int IVsHierarchy.Unused2()
        {
            return m_innerIVsHierarchy.Unused2();
        }

        int IVsHierarchy.Unused3()
        {
            return m_innerIVsHierarchy.Unused3();
        }

        int IVsHierarchy.Unused4()
        {
            return m_innerIVsHierarchy.Unused4();
        }

        #endregion

        #region IVsSingleFileGeneratorFactory Members

        int IVsSingleFileGeneratorFactory.CreateGeneratorInstance( string wszProgId, out int pbGeneratesDesignTimeSource, out int pbGeneratesSharedDesignTimeSource, out int pbUseTempPEFlag, out IVsSingleFileGenerator ppGenerate )
        {
            int hr = Utility.COM_HResults.S_OK;

            //what are these used for?
            pbGeneratesDesignTimeSource = Utility.Boolean.TRUE;
            pbGeneratesSharedDesignTimeSource = Utility.Boolean.FALSE;
            pbUseTempPEFlag = Utility.Boolean.FALSE;
            ppGenerate = null;

            NameValueCollection coll = ParseGeneratorProgID( ref wszProgId );

            switch(wszProgId)
            {
                case ResXFileCodeGenerator.c_Name:
                    ResXFileCodeGenerator resxGenerator = new ResXFileCodeGenerator(this);
                    ppGenerate = resxGenerator;

                    if(coll != null)
                    {
                        foreach(string name in coll.AllKeys)
                        {
                            string value = coll[name];
                            bool fValue;

                            switch(name.Trim().ToLower())
                            {
                                case "nestedenums":
                                    if(bool.TryParse( value, out fValue ))
                                        resxGenerator.m_fNestedEnums = fValue;
                                    break;
                                case "internal":
                                    if(bool.TryParse( value, out fValue ))
                                        resxGenerator.m_fInternal = fValue;
                                    break;
                                case "mscorlib":
                                    if(bool.TryParse( value, out fValue ))
                                        resxGenerator.m_fMscorlib = fValue;
                                    break;
                            }
                        }
                    }
                    break;
                default:
                    hr = Utility.COM_HResults.E_FAIL;

                    if(m_innerSingleFileGeneratorFactory != null)
                    {
                        hr = m_innerSingleFileGeneratorFactory.CreateGeneratorInstance( wszProgId, out pbGeneratesDesignTimeSource, out pbGeneratesSharedDesignTimeSource, out pbUseTempPEFlag, out ppGenerate );
                    }
                    break;
            }

            return hr;
        }

        int IVsSingleFileGeneratorFactory.GetDefaultGenerator( string wszFilename, out string pbstrGenProgID )
        {
            int hr = Utility.COM_HResults.S_OK;
            string ext = Path.GetExtension( wszFilename );

            pbstrGenProgID = string.Empty;

            switch(ext)
            {
                case ".resx":
                    //base project allows for overriding the progid of the SFG.
                    //if the base project has some overridden SFG, we do not want to pick it up
                    //however, we may? want to allow overriding this behavior in our own project generator regkey?
                    break;
                default:
                    hr = Utility.COM_HResults.E_FAIL;
                    if(m_innerSingleFileGeneratorFactory != null)
                    {
                        hr = m_innerSingleFileGeneratorFactory.GetDefaultGenerator( wszFilename, out pbstrGenProgID );
                    }
                    break;
            }

            return hr;
        }

        int IVsSingleFileGeneratorFactory.GetGeneratorInformation( string wszProgId, out int pbGeneratesDesignTimeSource, out int pbGeneratesSharedDesignTimeSource, out int pbUseTempPEFlag, out Guid pguidGenerator )
        {
            int hr = Utility.COM_HResults.S_OK;

            //what are these used for?
            pbGeneratesDesignTimeSource = Utility.Boolean.TRUE;
            pbGeneratesSharedDesignTimeSource = Utility.Boolean.FALSE;
            pbUseTempPEFlag = Utility.Boolean.FALSE;
            pguidGenerator = Guid.Empty;

            NameValueCollection coll = ParseGeneratorProgID( ref wszProgId );

            switch(wszProgId)
            {
                case ResXFileCodeGenerator.c_Name:
                    pguidGenerator = typeof( ResXFileCodeGenerator ).GUID;
                    break;
                default:
                    hr = Utility.COM_HResults.E_FAIL;
                    if(m_innerSingleFileGeneratorFactory != null)
                    {
                        hr = m_innerSingleFileGeneratorFactory.GetGeneratorInformation( wszProgId, out pbGeneratesDesignTimeSource, out pbGeneratesSharedDesignTimeSource, out pbUseTempPEFlag, out pguidGenerator );
                    }
                    break;
            }

            return hr;
        }

        #endregion

        #region IVsComponentUser Members

        int IVsComponentUser.AddComponent( VSADDCOMPOPERATION dwAddCompOperation, uint cComponents, IntPtr[] rgpcsdComponents, IntPtr hwndPickerDlg, VSADDCOMPRESULT[] pResult )
        {
            int hr = Utility.COM_HResults.S_OK;
            pResult[0] = VSADDCOMPRESULT.ADDCOMPRESULT_Success;

            //Do some validation checking here.  Make sure the component is TinyCLR compatible.

            VSLangProj.References references = this.VSLangProj_References;
            VSLangProj.Reference reference = null;

            for(int iComponent = 0; iComponent < cComponents; iComponent++)
            {
                VSCOMPONENTSELECTORDATA compData = (VSCOMPONENTSELECTORDATA)Marshal.PtrToStructure( rgpcsdComponents[iComponent], typeof( VSCOMPONENTSELECTORDATA ) );

                switch(compData.type)
                {
                    case VSCOMPONENTTYPE.VSCOMPONENTTYPE_ComPlus:
                        string fileName = Path.GetFileName(compData.bstrFile);
                        reference = references.Add(fileName);
                        break;
                    case VSCOMPONENTTYPE.VSCOMPONENTTYPE_File:
                    case VSCOMPONENTTYPE.VSCOMPONENTTYPE_Path:
                        reference = references.Add( compData.bstrFile );
                        break;
                    case VSCOMPONENTTYPE.VSCOMPONENTTYPE_Project:
                        string strNull = null;
                        IVsSolution solution = this.IVsSolution;
                        IVsHierarchy hier;

                        hr = solution.GetProjectOfProjref( compData.bstrProjRef, out hier, out strNull, null );
                        if(Utility.COM_HResults.FAILED( hr )) break;

                        EnvDTE.Project proj = this.GetProjectFromHierarchy( hier );
                        reference = references.AddProject( proj );

                        break;
                    default:
                        hr = Utility.COM_HResults.E_FAIL;
                        break;
                }

                if(Utility.COM_HResults.FAILED( hr )) break;
            }

            return hr;
        }

        #endregion

        #region IVsProject Members

        int IVsProject.AddItem( uint itemidLoc, VSADDITEMOPERATION dwAddItemOperation, string pszItemName, uint cFilesToOpen, string[] rgpszFilesToOpen, IntPtr hwndDlgOwner, VSADDRESULT[] pResult )
        {
            return m_innerIVsProject.AddItem( itemidLoc, dwAddItemOperation, pszItemName, cFilesToOpen, rgpszFilesToOpen, hwndDlgOwner, pResult );
        }

        int IVsProject.GenerateUniqueItemName( uint itemidLoc, string pszExt, string pszSuggestedRoot, out string pbstrItemName )
        {
            return m_innerIVsProject.GenerateUniqueItemName( itemidLoc, pszExt, pszSuggestedRoot, out pbstrItemName );
        }

        int IVsProject.GetItemContext( uint itemid, out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP )
        {
            int hr = Utility.COM_HResults.S_OK;

            {
                hr = m_innerIVsProject.GetItemContext( itemid, out ppSP );
            }

            if(Utility.COM_HResults.SUCCEEDED( hr ))
            {
                ppSP = new ContextServiceProvider( this, ppSP );
            }

            return hr;
        }

        int IVsProject.GetMkDocument( uint itemid, out string pbstrMkDocument )
        {
            return m_innerIVsProject.GetMkDocument( itemid, out pbstrMkDocument );
        }

        int IVsProject.IsDocumentInProject( string pszMkDocument, out int pfFound, VSDOCUMENTPRIORITY[] pdwPriority, out uint pitemid )
        {
            return m_innerIVsProject.IsDocumentInProject( pszMkDocument, out pfFound, pdwPriority, out pitemid );
        }

        int IVsProject.OpenItem( uint itemid, ref Guid rguidLogicalView, IntPtr punkDocDataExisting, out IVsWindowFrame ppWindowFrame )
        {
            return m_innerIVsProject.OpenItem( itemid, ref rguidLogicalView, punkDocDataExisting, out ppWindowFrame );
        }

        #endregion

        #region IOleCommandTarget Members

        int IOleCommandTarget.Exec( ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut )
        {
            return base._innerOleCommandTarget.Exec( ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut );
        }

        int IOleCommandTarget.QueryStatus( ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText )
        {
            if(QueryStatusHelper( VSConstants.VSITEMID_ROOT, ref pguidCmdGroup, cCmds, prgCmds, pCmdText ))
                return Utility.COM_HResults.S_OK;
            else
                return base._innerOleCommandTarget.QueryStatus( ref pguidCmdGroup, cCmds, prgCmds, pCmdText );
        }

        #endregion

        #region IInternalExtenderProvider Members

        bool IInternalExtenderProvider.CanExtend(string ExtenderCATID, string ExtenderName, object ExtendeeObject)
        {
            bool retVal = false;
            if (ExtenderName == VsProjectExtender.Name &&
               (ExtenderCATID == VSLangProj.PrjCATID.prjCATIDProject || ExtenderCATID == m_CATIDProjectBrowseObject))
            {
                retVal = true;
            }
            else if (ExtenderName == VsProjectConfigExtender.Name && ExtenderCATID == m_CATIDProjectConfigBrowseObject)
            {
                retVal = true;
            }
            else
            {
                // either we couldn't initialize the browse object CATID's or this isn't
                //   an object that we can extend.

                // delegate to aggregated project's IInternalExtenderProvider? Smart Device projects don't.
            }
            return retVal;
        }

        object IInternalExtenderProvider.GetExtender(string ExtenderCATID, string ExtenderName, object ExtendeeObject, EnvDTE.IExtenderSite ExtenderSite, int Cookie)
        {
            if (ExtenderName == VsProjectExtender.Name &&
               (ExtenderCATID == VSLangProj.PrjCATID.prjCATIDProject || ExtenderCATID == m_CATIDProjectBrowseObject))
            {
                if (m_projectExtender == null)
                {
                    m_projectExtender = new VsProjectExtender(ExtendeeObject, ExtenderSite, Cookie);
                }
                return m_projectExtender;
            }
            else if (ExtenderName == VsProjectConfigExtender.Name && ExtenderCATID == m_CATIDProjectConfigBrowseObject)
            {
                VsProjectConfigExtender configExtender = null;
                if (m_projectConfigExtenderMap.TryGetValue(ExtendeeObject, out configExtender) == false)
                {
                    configExtender = new VsProjectConfigExtender(ExtendeeObject, ExtenderSite, Cookie);
                    m_projectConfigExtenderMap.Add(ExtendeeObject, configExtender);
                }
                return configExtender;
            }
            else
            {
                // delegate to aggregated projects? Smart Device projects don't.
                Marshal.ThrowExceptionForHR(VSConstants.E_UNEXPECTED);
                return null;
            }
        }

        object IInternalExtenderProvider.GetExtenderNames(string ExtenderCATID, object ExtendeeObject)
        {
            // delegate into inner project's IInternalExternalProvider? SmartDevice projects don't.
            string[] extenderNames = null;

            if (ExtenderCATID == VSLangProj.PrjCATID.prjCATIDProject || ExtenderCATID == m_CATIDProjectBrowseObject)
            {
                extenderNames = new string[1];
                extenderNames[0] = VsProjectExtender.Name;

            }
            else if (ExtenderCATID == m_CATIDProjectConfigBrowseObject)
            {
                extenderNames = new string[1];
                extenderNames[0] = VsProjectConfigExtender.Name;
            }
            return extenderNames;
        }

        #endregion

        #region IVsProjectSpecialFiles Members

        int IVsProjectSpecialFiles.GetFile(int fileID, uint grfFlags, out uint pitemid, out string pbstrFilename)
        {
            pitemid = 0;
            pbstrFilename = null;
            if (fileID == (int)__PSFFILEID2.PSFFILEID_AppSettings)
            {
                //Hide the "Settings" tab in the project properties AppDesigner
                return VSConstants.E_FAIL;
            }

            return m_innerProjectSpecialFiles.GetFile(fileID, grfFlags, out pitemid, out pbstrFilename);
        }

        #endregion


        #region IVsBuildPropertyStorage Members

        /// <summary>
        /// Get the property of an item
        /// </summary>
        /// <param name="item">ItemID</param>
        /// <param name="attributeName">Name of the property</param>
        /// <param name="attributeValue">Value of the property (out parameter)</param>
        /// <returns>HRESULT</returns>
        int IVsBuildPropertyStorage.GetItemAttribute(uint item, string attributeName, out string attributeValue)
        {
            return this.m_innerIVsBuildPropertyStorage.GetItemAttribute(item, attributeName, out attributeValue);
        }

        /// <summary>
        /// Get the value of the property in the project file
        /// </summary>
        /// <param name="propertyName">Name of the property to remove</param>
        /// <param name="configName">Configuration for which to remove the property</param>
        /// <param name="storage">Project or user file (_PersistStorageType)</param>
        /// <param name="propertyValue">Value of the property (out parameter)</param>
        /// <returns>HRESULT</returns>
        int IVsBuildPropertyStorage.GetPropertyValue(string propertyName, string configName, uint storage, out string propertyValue)
        {
            return this.m_innerIVsBuildPropertyStorage.GetPropertyValue(propertyName, configName, storage, out propertyValue);
        }

        /// <summary>
        /// Delete a property
        /// In our case this simply mean defining it as null
        /// </summary>
        /// <param name="propertyName">Name of the property to remove</param>
        /// <param name="configName">Configuration for which to remove the property</param>
        /// <param name="storage">Project or user file (_PersistStorageType)</param>
        /// <returns>HRESULT</returns>
        int IVsBuildPropertyStorage.RemoveProperty(string propertyName, string configName, uint storage)
        {
            return this.m_innerIVsBuildPropertyStorage.RemoveProperty(propertyName, configName, storage);
        }

        /// <summary>
        /// Set a property on an item
        /// </summary>
        /// <param name="item">ItemID</param>
        /// <param name="attributeName">Name of the property</param>
        /// <param name="attributeValue">New value for the property</param>
        /// <returns>HRESULT</returns>
        int IVsBuildPropertyStorage.SetItemAttribute(uint item, string attributeName, string attributeValue)
        {
            return this.m_innerIVsBuildPropertyStorage.SetItemAttribute(item, attributeName, attributeValue);
        }

        /// <summary>
        /// Set a project property
        /// </summary>
        /// <param name="propertyName">Name of the property to set</param>
        /// <param name="configName">Configuration for which to set the property</param>
        /// <param name="storage">Project file or user file (_PersistStorageType)</param>
        /// <param name="propertyValue">New value for that property</param>
        /// <returns>HRESULT</returns>
        int IVsBuildPropertyStorage.SetPropertyValue(string propertyName, string configName, uint storage, string propertyValue)
        {
            return this.m_innerIVsBuildPropertyStorage.SetPropertyValue(propertyName, configName, storage, propertyValue);
        }

        #endregion

    }
}
