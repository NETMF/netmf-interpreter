using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using XsdInventoryFormatObject;

namespace ComponentObjectModel
{
    public class InventoryHelper
    {
        public enum ComponentType
        {
            Solution,
            Feature,
            Library,
            LibraryCategory,
            Processor,
            ISA,
            BuildTool,
            MiscBuildTool,

            NotFound,
        };

        List<Inventory> m_invs;
        Dictionary<string, object> m_guidToObjectHash = new Dictionary<string, object>();
        Dictionary<IList, Dictionary<string, object>> m_nameToObjectHash = new Dictionary<IList, Dictionary<string, object>>();
        Dictionary<string, object> m_fileToObjectHash = new Dictionary<string, object>();
        Dictionary<string, object> m_projToObjectHash = new Dictionary<string, object>();
        Dictionary<string, BuildParameter> m_nameToBuildParam = new Dictionary<string, BuildParameter>();
        static bool s_exhaustiveLibrarySearch = false;

        public static bool ExhaustiveLibrarySearch
        {
            get { return s_exhaustiveLibrarySearch; }
            set { s_exhaustiveLibrarySearch = value; }
        }

        public InventoryHelper(params Inventory[] inventories) : this(new List<Inventory>(inventories))
        {
        }

        public InventoryHelper(List<Inventory> inventories)
        {
            m_invs = new List<Inventory>(inventories);
        }

        public ICollection Inventories
        {
            get { return m_invs; }
        }

        public Inventory DefaultInventory
        {
            get { return m_invs[0]; }
        }

        private object FindObjectByProject(string file, IList list)
        {
            string key = MsBuildWrapper.ExpandEnvVars(file, "").ToLower();

            if (m_projToObjectHash.ContainsKey(key))
            {
                object o = m_projToObjectHash[key];

                if (!list.Contains(o))
                {
                    Console.WriteLine("Warning the list did not contain the project: " + file);
                }

                return o;
            }

            if (s_exhaustiveLibrarySearch || !(list is IList<Library>))
            {
                foreach (object o in list)
                {
                    try
                    {
                        string g = o.GetType().GetProperty("ProjectPath").GetValue(o, null) as string;

                        g = MsBuildWrapper.ExpandEnvVars(g, "").ToLower();

                        if (0 == string.Compare(g, key, true))
                        {
                            m_projToObjectHash[g] = o;
                            return o;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }


        private object FindObjectByFile(string file, IList list)
        {
            string key = Path.GetFileNameWithoutExtension(file).ToLower();

            if (m_fileToObjectHash.ContainsKey(key))
            {
                object o = m_fileToObjectHash[key];

                if (!list.Contains(o))
                {
                    Console.WriteLine("Warning, the list does not contai the file: " + file);
                }

                return o;
            }

            {
                foreach (object o in list)
                {
                    try
                    {
                        string g = o.GetType().GetProperty("LibraryFile").GetValue(o, null) as string;

                        g = Path.GetFileNameWithoutExtension(g).ToLower();

                        if (0 == string.Compare(g, key, true))
                        {
                            m_fileToObjectHash[g] = o;
                            return o;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }

        private object FindObjectByName(string name, IList list)
        {
            Dictionary<string, object> nameToObjectHash = null;

            if (m_nameToObjectHash.ContainsKey(list))
            {
                nameToObjectHash = m_nameToObjectHash[list];
            }
            else
            {
                nameToObjectHash = new Dictionary<string, object>();

                m_nameToObjectHash[list] = nameToObjectHash;
            }

            if (nameToObjectHash.ContainsKey(name.ToLower())) return nameToObjectHash[name.ToLower()];

            foreach (object o in list)
            {
                try
                {
                    string g = (o.GetType().GetProperty("Name").GetValue(o, null) as string).ToLower();

                    if (0 == string.Compare(g, name, true))
                    {
                        nameToObjectHash[g] = o;
                        return o;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private object FindObjectByGuid(string guid, IList list)
        {
            string key = guid.ToLower();
            if (m_guidToObjectHash.ContainsKey(key))
            {
                object o = m_guidToObjectHash[key];

                if (!list.Contains(o))
                {
                    Console.WriteLine("Warning the list does not contain a global libary");
                }

                return o;
            }

            if (s_exhaustiveLibrarySearch || !(list is List<Library>))
            {
                foreach (object o in list)
                {
                    try
                    {
                        string g = (o.GetType().GetProperty("Guid").GetValue(o, null) as string).ToLower();

                        if (0 == string.Compare(g, key, true))
                        {
                            m_guidToObjectHash[key] = o;
                            return o;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }

        private bool ValidateComponent(object component)
        {
            bool bRet = true;
            object o = null;

            if (component == null) return true;

            PropertyInfo pi = component.GetType().GetProperty("Guid");

            if (pi == null) return true;

            string guid = pi.GetValue(component, null) as string;

            if (string.IsNullOrEmpty(guid)) return true;

            if (ComponentType.NotFound != GetComponentTypeByGuid(guid, out o) && o != null)
            {
                PropertyInfo piNew = o.GetType().GetProperty("Name");
                pi = component.GetType().GetProperty("Name");
                string newName = piNew.GetValue(o, null) as string;
                string oldName = pi.GetValue(component, null) as string;
                if (pi != null && piNew != null)
                {
                    if (0 != string.Compare(newName, oldName))
                    {
                        pi.SetValue(component, piNew.GetValue(o, null) as string, null);
                    }
                }
                else
                {
                    bRet = false;
                }
            }
            else
            {
                bRet = false;
            }

            return bRet;
        }

        public void ValidateData()
        {
            foreach (Inventory inv in m_invs)
            {
                /*
                foreach (ISA procType in inv.ISAs)
                {
                    ArrayList removeList = new ArrayList();
                    foreach (BuildToolRef bt in procType.BuildToolOptions)
                    {
                        if (!ValidateComponent(bt))
                        {
                            removeList.Add(bt);
                        }
                    }
                    foreach (object o in removeList)
                    {
                        procType.BuildToolOptions.Remove(o as BuildToolRef);
                    }
                }
                */
                foreach (Processor proc in inv.Processors)
                {
                    List<MFComponent> removeISAs = new List<MFComponent>();

                    foreach (MFComponent isa in proc.SupportedISAs)
                    {
                        if (!ValidateComponent(isa))
                        {
                            removeISAs.Add(isa);
                        }
                    }
                    foreach (MFComponent isa in removeISAs)
                    {
                        proc.SupportedISAs.Remove(isa);
                    }

                    ArrayList removeList = new ArrayList();
                    foreach (BuildToolRef bt in proc.BuildToolOptions)
                    {
                        if (!ValidateComponent(bt))
                        {
                            removeList.Add(bt);
                        }
                    }
                    foreach (object o in removeList)
                    {
                        proc.BuildToolOptions.Remove(o as BuildToolRef);
                    }
                }

                foreach (Library lib in inv.Libraries)
                {
                    if (!ValidateComponent(lib.ProcessorSpecific))
                    {
                        lib.ProcessorSpecific = null;
                    }
                    if (!ValidateComponent(lib.LibraryCategory))
                    {
                        lib.LibraryCategory = null;
                    }
                    ArrayList removeList = new ArrayList();
                    foreach (MFComponent comp in lib.Dependencies)
                    {
                        if (!ValidateComponent(comp))
                        {
                            removeList.Add(comp);
                        }
                    }
                    foreach (object o in removeList)
                    {
                        lib.Dependencies.Remove(o as MFComponent);
                    }
                    /*
                                        removeList.Clear();
                                        foreach (MFComponent comp in lib.SoftDependencies)
                                        {
                                            if (!ValidateComponent(comp))
                                            {
                                                removeList.Add(comp);
                                            }
                                        }
                                        foreach (object o in removeList)
                                        {
                                            lib.SoftDependencies.Remove(o as MFComponent);
                                        }
                    */
                }

                foreach (Feature feat in inv.Features)
                {
                    ArrayList removeList = new ArrayList();

                    foreach (MFComponent comp in feat.FeatureDependencies)
                    {
                        if (!ValidateComponent(comp))
                        {
                            removeList.Add(comp);
                        }
                    }
                    foreach (MFComponent c in removeList)
                    {
                        feat.FeatureDependencies.Remove(c);
                    }

                    removeList.Clear();
                    foreach (MFComponent comp in feat.ComponentDependencies)
                    {
                        if (!ValidateComponent(comp))
                        {
                            removeList.Add(comp);
                        }
                    }

                    foreach (MFComponent c in removeList)
                    {
                        feat.ComponentDependencies.Remove(c);
                    }
                }
                foreach (MFSolution solution in inv.Solutions)
                {
                    if (!ValidateComponent(solution.Processor))
                    {
                        solution.Processor = null;
                    }
                    if (!ValidateComponent(solution.BuildTool))
                    {
                        solution.BuildTool = null;
                    }
                    ArrayList removeList = new ArrayList();
                    foreach (MFProject comp in solution.Projects)
                    {
                        if (!ValidateComponent(comp))
                        {
                            removeList.Add(comp);
                        }
                    }
                    foreach (object o in removeList)
                    {
                        solution.Projects.Remove(o as MFProject);
                    }
                }

            }
        }

        public ComponentType GetComponentTypeByGuid(string guid, out object component)
        {
            component = null;

            foreach (Inventory inv in m_invs)
            {
                if (null != (component = FindObjectByGuid(guid, inv.Solutions)))
                {
                    return ComponentType.Solution;
                }
                else if (null != (component = FindObjectByGuid(guid, inv.Features)))
                {
                    return ComponentType.Feature;
                }
                else if (null != (component = FindObjectByGuid(guid, inv.Libraries)))
                {
                    return ComponentType.Library;
                }
                else if (null != (component = FindObjectByGuid(guid, inv.LibraryCategories)))
                {
                    return ComponentType.LibraryCategory;
                }
                else if (null != (component = FindObjectByGuid(guid, inv.Processors)))
                {
                    return ComponentType.Processor;
                }
                /*
                else if (null != (component = FindObjectByGuid(guid, inv.ISAs)))
                {
                    return ComponentType.ISA;
                }
                */
                else if (null != (component = FindObjectByGuid(guid, inv.BuildTools)))
                {
                    return (component is BuildTool ? ComponentType.BuildTool : component is MiscBuildTool ? ComponentType.MiscBuildTool : ComponentType.NotFound);
                }
            }
            return ComponentType.NotFound;
        }

        public BuildParameter FindBuildParameter(string name)
        {
            if (m_nameToBuildParam.ContainsKey(name.ToLower())) return m_nameToBuildParam[name] as BuildParameter;

            foreach (Inventory inv in m_invs)
            {
                foreach (BuildParameter bp in inv.BuildParameters)
                {
                    if (0 == string.Compare(bp.Parameter, name, true))
                    {
                        m_nameToBuildParam[bp.Parameter.ToLower()] = bp;
                        return bp;
                    }
                }
            }
            return null;
        }

        public BuildTool FindBuildTool(string buildToolGuid)
        {
            foreach (Inventory inv in m_invs)
            {
                BuildTool bt = FindObjectByGuid(buildToolGuid, inv.BuildTools) as BuildTool;

                if (bt != null) return bt;
            }

            return null;
        }

        public BuildTool FindBuildToolByName(string buildToolName)
        {
            foreach (Inventory inv in m_invs)
            {
                BuildTool buildToo = FindObjectByName(buildToolName, inv.BuildTools) as BuildTool;

                if (buildToo != null) return buildToo;
            }

            return null;
        }

        public Feature FindFeature(string featureGuid)
        {
            foreach (Inventory inv in m_invs)
            {
                Feature f = FindObjectByGuid(featureGuid, inv.Features) as Feature;

                if (f != null) return f;
            }

            return null;
        }

        public Feature FindFeatureByProject(string featProject)
        {
            foreach (Inventory inv in m_invs)
            {
                Feature feat = FindObjectByProject(featProject, inv.Features) as Feature;

                if (feat != null) return feat;
            }

            return null;
        }

        public Feature FindFeatureByName(string name)
        {
            foreach (Inventory inv in m_invs)
            {
                Feature feat = FindObjectByName(name, inv.Features) as Feature;

                if (feat != null) return feat;
            }

            return null;
        }

        public MFSolution FindSolution(string solGuid)
        {
            foreach (Inventory inv in m_invs)
            {
                MFSolution p = FindObjectByGuid(solGuid, inv.Solutions) as MFSolution;

                if (p != null) return p;
            }

            return null;
        }

        public void AddLibraryToInventory(Library library, bool isManifest, Inventory inv)
        {
            if (isManifest)
            {
                // only add manifests if we don't have the project loaded
                if (m_guidToObjectHash.ContainsKey(library.Guid.ToLower()))
                {
                    return;
                }
            }

            string path = MsBuildWrapper.ExpandEnvVars(library.ProjectPath, "").ToLower();
            if (m_projToObjectHash.ContainsKey(path))
            {
                return;

                //Console.WriteLine("Warning: project path alread in inventory: " + library.ProjectPath);
            }
            m_projToObjectHash[path] = library;

            string libFile = Path.GetFileNameWithoutExtension(library.LibraryFile);
            if (!string.IsNullOrEmpty(libFile))
            {
                libFile = libFile.ToLower();

                if (m_fileToObjectHash.ContainsKey(libFile))
                {
                    if (!isManifest)
                    {
                        m_fileToObjectHash[libFile] = library;
                    }
                }
                else
                {
                    m_fileToObjectHash[libFile] = library;
                }
            }
            string guid = library.Guid;

            if (string.IsNullOrEmpty(guid))
            {
                if (!isManifest)
                {
                    Console.WriteLine("WARNING: project without guid found: " + library.ProjectPath);
                }
                library.Guid = Guid.NewGuid().ToString("B");
                guid = library.Guid;
            }
            guid = guid.ToLower();

            if (m_guidToObjectHash.ContainsKey(guid))
            {
                Console.WriteLine("Warning: GUID alread in inventory: " + guid);
            }
            m_guidToObjectHash[guid] = library;

            inv.Libraries.Add(library);
        }

        /*
        public BSP FindBSP(string bspGuid)
        {
            foreach (Inventory inv in m_invs)
            {
                BSP p = FindObjectByGuid(bspGuid, inv.BSPs) as BSP;

                if (p != null) return p;
            }

            return null;
        }
        */

        public Processor FindProcessor(string processorGuid)
        {
            foreach (Inventory inv in m_invs)
            {
                Processor p = FindObjectByGuid(processorGuid, inv.Processors) as Processor;

                if (p != null) return p;
            }

            return null;
        }

        public ISA FindISAByName(string ISAName)
        {
            foreach (Inventory inv in m_invs)
            {
                foreach (BuildTool bt in inv.BuildTools)
                {
                    ISA pt = FindObjectByName(ISAName, bt.SupportedISAs) as ISA;

                    if (pt != null) return pt;
                }
            }

            return null;
        }

        public ISA FindISA(string ISAGuid)
        {
            foreach (Inventory inv in m_invs)
            {
                foreach (BuildTool bt in inv.BuildTools)
                {
                    ISA pt = FindObjectByGuid(ISAGuid, bt.SupportedISAs) as ISA;

                    if (pt != null) return pt;
                }
            }

            return null;
        }

        public MFSolution FindSolutionByName(string solName)
        {
            foreach (Inventory inv in m_invs)
            {
                MFSolution p = FindObjectByName(solName, inv.Solutions) as MFSolution;

                if (p != null) return p;
            }

            return null;
        }

        public Processor FindProcessorByName(string procName)
        {
            foreach (Inventory inv in m_invs)
            {
                Processor p = FindObjectByName(procName, inv.Processors) as Processor;

                if (p != null) return p;
            }

            return null;
        }

        public MFSolution FindPlatformByName(string solutionName)
        {
            foreach (Inventory inv in m_invs)
            {
                MFSolution p = FindObjectByName(solutionName, inv.Solutions) as MFSolution;

                if (p != null) return p;
            }

            return null;
        }

        /*
        public MFSolution FindPlatform(string solutionGuid)
        {
            foreach (Inventory inv in m_invs)
            {
                MFSolution p = FindObjectByGuid(solutionGuid, inv.Solutions) as MFSolution;

                if (p != null) return p;
            }

            return null;
        }
        */

        public Library FindLibraryByFile(string libraryFile)
        {
            if (string.IsNullOrEmpty(libraryFile)) return null;

            foreach (Inventory inv in m_invs)
            {
                Library lib = FindObjectByFile(libraryFile, inv.Libraries) as Library;

                if (lib != null) return lib;
            }

            return null;
        }

        public Library FindLibraryByProject(string libraryProj)
        {
            if (string.IsNullOrEmpty(libraryProj)) return null;

            foreach (Inventory inv in m_invs)
            {
                Library lib = FindObjectByProject(libraryProj, inv.Libraries) as Library;

                if (lib != null) return lib;
            }

            return null;
        }


        public Library FindLibraryByName(string libraryName)
        {
            if (string.IsNullOrEmpty(libraryName)) return null;

            foreach (Inventory inv in m_invs)
            {
                Library lib = FindObjectByName(libraryName, inv.Libraries) as Library;

                if (lib != null) return lib;
            }

            return null;
        }

        public Library FindLibrary(string libraryGuid)
        {
            if (string.IsNullOrEmpty(libraryGuid)) return null;

            foreach (Inventory inv in m_invs)
            {
                Library l = FindObjectByGuid(libraryGuid, inv.Libraries) as Library;

                if (l != null) return l;
            }

            return null;
        }

        public Library FindLibrary(MFComponent comp)
        {
            if (comp.ComponentType != MFComponentType.Library) return null;

            Library l = FindLibrary(comp.Guid);

            if (l == null) l = FindLibraryByProject(comp.ProjectPath);
            if (l == null) l = FindLibraryByFile(comp.Name + ".$(LIB_EXT)");

            // some components are delay loaded, so fill in the data if need be
            if (l != null && (0 != string.Compare(comp.Guid, l.Guid, true)))
            {
                comp.Guid = l.Guid;
                comp.Name = l.Name;
                comp.ProjectPath = l.ProjectPath;
            }

            return l;
        }

        public MFAssembly FindAssemblyByProject(string asmProject)
        {
            foreach (Inventory inv in m_invs)
            {
                MFAssembly asm = FindObjectByProject(asmProject, inv.Assemblies) as MFAssembly;

                if (asm != null) return asm;
            }

            return null;
        }

        public MFAssembly FindAssembly(string asmGuid)
        {
            foreach (Inventory inv in m_invs)
            {
                MFAssembly asm = FindObjectByGuid(asmGuid, inv.Assemblies) as MFAssembly;

                if (asm != null) return asm;
            }

            return null;
        }


        public MFAssembly FindAssemblyByName(string asmName)
        {
            foreach (Inventory inv in m_invs)
            {
                MFAssembly asm = FindObjectByName(asmName, inv.Assemblies) as MFAssembly;

                if (asm != null) return asm;
            }

            return null;
        }

        public LibraryCategory FindLibraryCategoryByName(string LibraryCategoryName)
        {
            foreach (Inventory inv in m_invs)
            {
                LibraryCategory lt = FindObjectByName(LibraryCategoryName, inv.LibraryCategories) as LibraryCategory;

                if (lt != null) return lt;
            }

            return null;
        }
        public LibraryCategory FindLibraryCategory(string LibraryCategoryGuid)
        {
            foreach (Inventory inv in m_invs)
            {
                LibraryCategory lt = FindObjectByGuid(LibraryCategoryGuid, inv.LibraryCategories) as LibraryCategory;

                if (lt != null) return lt;
            }

            return null;
        }

        public MFSolution[] Solutions
        {
            get
            {
                List<MFSolution> ret = new List<MFSolution>();

                foreach (Inventory inv in m_invs)
                {
                    ret.AddRange(inv.Solutions);
                }

                return ret.ToArray();
            }
        }
        public Processor[] Processors
        {
            get
            {
                List<Processor> ret = new List<Processor>();

                foreach (Inventory inv in m_invs)
                {
                    ret.AddRange(inv.Processors);
                }

                return ret.ToArray();
            }
        }
        public ISA[] ISAs
        {
            get
            {
                List<ISA> ret = new List<ISA>();

                foreach (Inventory inv in m_invs)
                {
                    foreach (BuildTool bt in inv.BuildTools)
                    {
                        ret.AddRange(bt.SupportedISAs);
                    }
                }

                return ret.ToArray();
            }
        }
        public Feature[] Features
        {
            get
            {
                List<Feature> ret = new List<Feature>();

                foreach (Inventory inv in m_invs)
                {
                    ret.AddRange(inv.Features);
                }

                return ret.ToArray();
            }
        }
        public LibraryCategory[] LibraryCategories
        {
            get
            {
                List<LibraryCategory> ret = new List<LibraryCategory>();

                foreach (Inventory inv in m_invs)
                {
                    ret.AddRange(inv.LibraryCategories);
                }

                return ret.ToArray();
            }
        }
        public Library[] Libraries
        {
            get
            {
                List<Library> ret = new List<Library>();

                foreach (Inventory inv in m_invs)
                {
                    ret.AddRange(inv.Libraries);
                }

                return ret.ToArray();
            }
        }
        public BuildTool[] BuildTools
        {
            get
            {
                List<BuildTool> ret = new List<BuildTool>();

                foreach (Inventory inv in m_invs)
                {
                    ret.AddRange(inv.BuildTools);
                }

                return ret.ToArray();
            }
        }
        public BuildParameter[] BuildParameters
        {
            get
            {
                List<BuildParameter> ret = new List<BuildParameter>();

                foreach (Inventory inv in m_invs)
                {
                    ret.AddRange(inv.BuildParameters);
                }

                return ret.ToArray();
            }
        }

        public MFAssembly[] Assemblies
        {
            get
            {
                List<MFAssembly> ret = new List<MFAssembly>();

                foreach (Inventory inv in m_invs)
                {
                    ret.AddRange(inv.Assemblies);
                }

                return ret.ToArray();
            }
        }

        public List<LibraryCategory> GetTransports()
        {
            List<LibraryCategory> ret = new List<LibraryCategory>();

            foreach (Inventory inv in m_invs)
            {
                foreach (LibraryCategory lc in inv.LibraryCategories)
                {
                    if (lc != null)
                    {
                        if (lc.IsTransport)
                        {
                            ret.Add(lc);
                        }
                    }
                }
            }
            return ret;
        }

        public Library[] GetLibrariesOfType(LibraryCategory LibraryCategory)
        {
            return GetLibrariesOfType(LibraryCategory, null, null);
        }

        public Library[] GetLibrariesOfType(LibraryCategory LibraryCategory, MFProject proj, MFSolution solution)
        {
            List<Library> ret = new List<Library>();

            foreach (Inventory inv in m_invs)
            {
                foreach (Library lib in inv.Libraries)
                {
                    if (lib.LibraryCategory != null)
                    {
                        if (string.IsNullOrEmpty(lib.LibraryCategory.Guid)) continue;

                        if (0 == string.Compare(lib.LibraryCategory.Guid, LibraryCategory.Guid, true))
                        {
                            ret.Add(lib);
                        }
                    }
                }
            }

            return ret.ToArray();
        }

        public LibraryCategory[] GetRequiredLibraryCategories()
        {
            List<LibraryCategory> ret = new List<LibraryCategory>();

            foreach (Inventory inv in m_invs)
            {
                foreach (LibraryCategory libType in inv.LibraryCategories)
                {
                    if (libType.Required)
                    {
                        ret.Add(libType);
                    }
                }
            }

            return ret.ToArray();
        }
        public Library[] GetRequiredLibraries()
        {
            List<Library> ret = new List<Library>();

            foreach (Inventory inv in m_invs)
            {
                foreach (Library lib in inv.Libraries)
                {
                    if (lib.Required)
                    {
                        ret.Add(lib);
                    }
                }
            }

            return ret.ToArray();
        }

        public Feature[] GetRequiredFeatures()
        {
            List<Feature> ret = new List<Feature>();

            foreach (Inventory inv in m_invs)
            {
                foreach (Feature feat in inv.Features)
                {
                    if (feat.Required)
                    {
                        ret.Add(feat);
                    }
                }
            }

            return ret.ToArray();
        }

        public static bool CollectionContainsItem(ICollection collection, string itemGuid)
        {
            bool ret = false;

            foreach (object o in collection)
            {
                PropertyInfo pi = o.GetType().GetProperty("Guid");

                if (pi != null && 0 == string.Compare(pi.GetValue(o, null) as string, itemGuid, true))
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        public MFSolution CloneSolution(MFSolution solution, string name)
        {
            MFSolution solNew = new MFSolution();

            solution.CopyTo(solNew, name);

            return solNew;
        }
    }
}
