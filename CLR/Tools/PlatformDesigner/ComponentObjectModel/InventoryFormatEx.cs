using System.Reflection;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using ComponentObjectModel;
using Microsoft.Build.Execution;
using Microsoft.Build.Construction;

namespace XsdInventoryFormatObject
{
    internal static class CopyHelper
    {
        internal static void CopyTo(object from, object to)
        {
            CopyTo(from, to, null);
        }

        internal static string ReplaceText(string text, string old, string replace)
        {
            string ret = text;

            if (string.IsNullOrEmpty(text)) return text;

            if (string.Compare(text, old, true) == 0)
            {
                return replace;
            }

            Regex ex = new Regex(old, RegexOptions.IgnoreCase);

            ret = ex.Replace(ret, replace);

            return ret;
        }

        internal static string ReplaceText(string text, Dictionary<string,string> nameChanges)
        {
            if (nameChanges != null)
            {
                foreach (string key in nameChanges.Keys)
                {
                    text = ReplaceText(text, key, (string)nameChanges[key]);
                }
            }

            return text;
        }

        internal static void CopyTo(object from, object to, Dictionary<string,string> nameChanges)
        {
            Type toType = to.GetType();            

            foreach(PropertyInfo fi in from.GetType().GetProperties())
            {
                object val = fi.GetValue(from, null);

                if (val != null)
                {
                    if (fi.Name.ToUpper().Contains("GUID") && !(from is MFComponent) && !(from is BuildToolRef))
                    {
                        fi.SetValue(to, System.Guid.NewGuid().ToString("B"), null);
                    }
                    else if (fi.PropertyType.FullName.ToUpper().Contains("SYSTEM.COLLECTIONS.GENERIC.LIST" ))
                    {
                        System.Collections.IList list = (System.Collections.IList)fi.PropertyType.GetConstructor(new Type[] { }).Invoke(null);
                        System.Collections.IList listFrom = (System.Collections.IList)val;

                        foreach (object o in listFrom)
                        {
                            if (o.GetType() == typeof(string))
                            {
                                list.Add(ReplaceText((string)o, nameChanges));
                            }
                            else if (o.GetType().IsValueType)
                            {
                                list.Add(o);
                            }
                            else if(o.GetType() == typeof(ProjectTargetElement))
                            {
                                list.Add(o);
                            }
                            else
                            {
                                object dstObj = o.GetType().GetConstructor(new Type[] { }).Invoke(null);

                                CopyTo(o, dstObj, nameChanges);

                                if (dstObj is MFProject)
                                {
                                    MFProject old = (MFProject)o;
                                    if (old.m_cloneProj != null)
                                    {
                                        ((MFProject)dstObj).m_cloneProj = old.m_cloneProj;
                                    }
                                    else
                                    {
                                        ((MFProject)dstObj).m_cloneProj = old;
                                    }
                                }

                                list.Add(dstObj);
                            }
                        }

                        toType.GetProperty(fi.Name).SetValue(to, list, null);
                    }
                    else if (fi.PropertyType == typeof(string))
                    {
                        toType.GetProperty(fi.Name).SetValue(to, ReplaceText((string)val, nameChanges), null);
                    }
                    else if (fi.PropertyType.IsValueType)
                    {
                        toType.GetProperty(fi.Name).SetValue(to, val, null);
                    }
                    else if(fi.Name != "m_cloneSolution")
                    {
                        object o = fi.PropertyType.GetConstructor(new Type[] { }).Invoke(null);

                        CopyTo(val, o, nameChanges);

                        toType.GetProperty(fi.Name).SetValue(to, o, null);
                    }
                }
            }
        }
        internal static void Rename(object targ, string oldName, string newName)
        {
            Type oType = targ.GetType();

            foreach (PropertyInfo fi in oType.GetProperties())
            {
                object val = fi.GetValue(targ, null);

                if (val != null)
                {
                    if (fi.PropertyType.FullName.ToUpper().Contains("SYSTEM.COLLECTIONS.GENERIC.LIST"))
                    {
                        System.Collections.IList list = (System.Collections.IList)val;
                        ArrayList rem = new ArrayList();
                        ArrayList add = new ArrayList();

                        foreach (object o in list)
                        {
                            if (o.GetType() == typeof(string))
                            {
                                string txt = ReplaceText((string)o, oldName, newName);

                                if (txt != (string)o)
                                {
                                    rem.Add(o);
                                    add.Add(txt);
                                }
                            }
                            else
                            {
                                Rename(o, oldName, newName);
                            }
                        }
                        foreach (object o in rem)
                        {
                            list.Remove(o);
                        }
                        foreach (object o in add)
                        {
                            list.Add(o);
                        }
                    }
                    else if (fi.PropertyType == typeof(string))
                    {
                        fi.SetValue(targ, (object)ReplaceText((string)val, oldName, newName), null);
                    }
                    else if (fi.PropertyType.IsValueType)
                    {
                    }
                    else if (fi.PropertyType.Assembly == typeof(CopyHelper).Assembly)
                    {
                        Rename(val, oldName, newName);
                    }
                    else
                    {
                        Console.WriteLine("");
                    }
                }
            }
        }
    }

    public partial class Inventory : object
    {
        public Inventory()
        {
            //this.bSPsField = new System.Collections.Generic.List<BSP>();
            this.buildParametersField = new System.Collections.Generic.List<BuildParameter>();
            this.buildToolsField = new System.Collections.Generic.List<BuildTool>();
            this.featuresField = new System.Collections.Generic.List<Feature>();
            this.fileField = "";
            this.librariesField = new System.Collections.Generic.List<Library>();
            this.libraryCategoriesField = new System.Collections.Generic.List<LibraryCategory>();
            this.nameField = "";
            this.solutionsField = new System.Collections.Generic.List<MFSolution>();
            this.processorsField = new System.Collections.Generic.List<Processor>();
            this.versionField = new Version();
            this.assembliesField = new System.Collections.Generic.List<MFAssembly>();
            this.projectTemplatesField = new System.Collections.Generic.List<MFProject>();
        }
        public void CopyTo(Inventory dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }

    public partial class MFComponent : object
    {
        public MFComponent() : this(MFComponentType.Unknown) { }
        public MFComponent(MFComponentType type) 
        { 
            componentTypeField = type; 
            refCountField = 0; 
            conditionalField = "";
            projectPathField = "";
            conditionalField = "";
        }
        public MFComponent(MFComponentType type, string name) : this(type, name, "", "", "") { }
        public MFComponent(MFComponentType type, string name, string guid) : this(type, name, guid, "", "") { }
        public MFComponent(MFComponentType type, string name, string guid, string projectPath) : this(type, name, guid, projectPath, "") { }
        public MFComponent(MFComponentType type, string name, string guid, string projectPath, string conditional)
        {
            this.nameField = name;
            this.projectPathField = projectPath;
            this.conditionalField = conditional;
            this.guidField = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString("B"): guid.ToUpper();
            this.componentTypeField = type;
            this.refCountField = 0;
        }
        public void CopyTo(MFComponent dest)
        {
            CopyHelper.CopyTo(this, dest);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            MFComponent rhs = obj as MFComponent;

            if (rhs != null)
            {
                return (string.Compare(this.Guid, rhs.Guid, true) == 0);
            }

            return false;
        }
    }

    public partial class MFAssembly : object
    {
        public MFAssembly()
        {
            this.assemblyFileField = "";
            this.descriptionField = "";
            this.groupsField = "";
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.nameField = "";
            this.referencesField = new System.Collections.Generic.List<MFComponent>();
            this.isSolutionWizardVisibleField = true;
        }
        public void CopyTo(MFAssembly dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }

    public partial class Feature : object
    {
        public Feature()
        {
            this.componentDependenciesField = new System.Collections.Generic.List<MFComponent>();
            this.descriptionField = "";
            this.documentationField = "";
            this.featureDependenciesField = new System.Collections.Generic.List<MFComponent>();
            this.groupsField = "";
            this.nameField = "";
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.projectPathField = "";
            this.assembliesField = new System.Collections.Generic.List<MFComponent>();
            this.IsSolutionWizardVisible = true;
        }
        public void CopyTo(Feature dest)
        {
            CopyHelper.CopyTo(this, dest);
        }


        public Dictionary<string, MFComponent> GetDependants(InventoryHelper helper)
        {
            Dictionary<string, MFComponent> deps = new Dictionary<string, MFComponent>();

            foreach (Feature f in helper.Features)
            {
                foreach (MFComponent fDep in f.featureDependenciesField)
                {
                    if (0 == string.Compare(fDep.Guid, this.guidField, true))
                    {
                        deps[f.Guid.ToLower()] = new MFComponent(MFComponentType.Feature, f.Name, f.Guid, f.ProjectPath);
                        break;
                    }
                }
            }

            return deps;
        }
    }
    public partial class MFProject : object
    {
        internal MFProject m_cloneProj = null;
        System.Collections.Generic.List<ProjectTargetElement> targetsField;

        public MFProject()
        {
            this.targetsField = new List<ProjectTargetElement>();
            this.buildToolField = null;
            this.descriptionField = "";
            this.documentationField = "";
            this.directoryField = "";
            this.featuresField = new System.Collections.Generic.List<MFComponent>();
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.librariesField = new System.Collections.Generic.List<MFComponent>();
            this.memoryMapField = new MemoryMap();
            this.nameField = "";
            this.versionField = new Version();
            this.propertiesField = new System.Collections.Generic.List<MFProperty>();
            this.interopFeaturesField = new System.Collections.Generic.List<string>();
            this.extraAssembliesField = new System.Collections.Generic.List<string>();
            this.sourceFilesField = new System.Collections.Generic.List<MFBuildFile>();
            this.headerFilesField = new System.Collections.Generic.List<MFBuildFile>();
            this.includePathsField = new System.Collections.Generic.List<MFBuildFile>();
            this.otherFilesField = new System.Collections.Generic.List<MFBuildFile>();
            this.fastCompileFilesField = new System.Collections.Generic.List<MFBuildFile>();
            this.libraryCategoriesField = new System.Collections.Generic.List<MFComponent>();
            this.isClrProjectField = false;
            this.isSolutionWizardVisibleField = true;
        }

        public List<ProjectTargetElement> Targets
        {
            get
            {
                return targetsField;
            }
            set
            {
                targetsField = value;
            }
        }

        public void CopyTo(MFProject dest, string solutionName)
        {
            if (dest.m_cloneProj == null)
            {
                dest.m_cloneProj = this;
            }
            Dictionary<string, string> hash = new Dictionary<string, string>();
            hash[@"\$\(PLATFORM\)"] = solutionName;
            hash[@"\$\(TARGETPLATFORM\)"] = solutionName;

            CopyHelper.CopyTo(this, dest, hash);
        }

        public void CopyTo(MFProject dest, string newName, string solutionName)
        {
            if (dest.m_cloneProj == null)
            {
                dest.m_cloneProj = this;
            }

            Dictionary<string, string> hash = new Dictionary<string, string>();

            hash[this.nameField] = newName;
            hash[@"\$\(PLATFORM\)"] = solutionName;
            hash[@"\$\(TARGETPLATFORM\)"] = solutionName;

            CopyHelper.CopyTo(this, dest, hash);
        }

        public List<MFComponent> ResolveLibraries()
        {
            List<MFComponent> list = new List<MFComponent>();

            return list;
        }

        public bool IsBootloaderProject()
        {
            foreach (MFProperty prop in propertiesField)
            {
                if (0 == string.Compare(prop.Name, "ReduceSize", true))
                {
                    if (0 == string.Compare(prop.Value, "true", true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsAssocFeatureChecked(LibraryCategory libCat, Dictionary<string, MFComponent> featureList)
        {
            bool isFeatureChecked = false;

            foreach (MFComponent feat in libCat.FeatureAssociations)
            {
                if (featureList.ContainsKey(feat.Guid.ToLower()))
                {
                    isFeatureChecked = true;
                }
            }

            return isFeatureChecked;
        }


        private void RecurseFeatureDeps(MFComponent cmpFeat, List<MFComponent> features, InventoryHelper helper)
        {
            if (features.Contains(cmpFeat)) return;

            Feature feat = helper.FindFeature(cmpFeat.Guid);

            if(feat != null)
            {
                foreach (MFComponent depFeat in feat.FeatureDependencies)
                {
                    RecurseFeatureDeps(depFeat, features, helper);
                }

                features.Add(cmpFeat);
            }
            else
            {
                Console.WriteLine("Error: Feature not found: " + cmpFeat.Name);
            }
        }

        public void AnalyzeFeatures(InventoryHelper helper)
        {
            List<MFComponent> features = new List<MFComponent>();

            foreach (Feature feat in helper.GetRequiredFeatures())
            {
                MFComponent cmp = new MFComponent(MFComponentType.Feature, feat.Name, feat.Guid, feat.ProjectPath);

                if (!features.Contains(cmp))
                {
                    features.Add(cmp);
                }
            }

            foreach (MFComponent cmpFeat in featuresField)
            {
                RecurseFeatureDeps(cmpFeat, features, helper);
            }

            this.featuresField.Clear();
            foreach (MFComponent cmpFeat in features)
            {
                this.featuresField.Add(cmpFeat);
            }
        }

        public bool ValidateLibrary(Library lib, MFSolution solution, Processor proc, InventoryHelper helper)
        {
            try
            {
                if (!lib.IsSolutionWizardVisible)
                {
                    return false;
                }

                // don't show processor specific libraries
                if ((lib.ProcessorSpecific != null) && !string.IsNullOrEmpty(lib.ProcessorSpecific.Guid) &&
                    (0 != string.Compare(lib.ProcessorSpecific.Guid, solution.Processor.Guid, true)))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(lib.CustomFilter))
                {
                    bool OK = false;

                    // don't show custom specific libraries
                    foreach (string libFilter in lib.CustomFilter.Split(';'))
                    {
                        string[] customAttribs = proc.CustomFilter.Split(';');

                        foreach (string attrib in customAttribs)
                        {
                            if (string.Compare(attrib, libFilter, true) == 0)
                            {
                                OK = true;
                                break;
                            }
                        }
                        if (!OK)
                        {
                            foreach (string attrib in solution.CustomFilter.Split(';'))
                            {
                                if (0 == string.Compare(attrib, libFilter, true))
                                {
                                    OK = true;
                                    break;
                                }
                            }
                        }

                        /// 
                        /// Now lets check to see if one of the selected features contains this filter.
                        /// This is used for Network (LWIP) to enable the libraries to be auto selected
                        /// based on which Network feature was choosen
                        /// 
                        if (!OK)
                        {
                            MFProject defProj = null;
                            foreach(MFProject proj in solution.Projects)
                            {
                                if(proj.IsClrProject)
                                {
                                    defProj = proj; 
                                    break;
                                }
                            }

                            foreach (MFComponent feat in defProj.Features)
                            {
                                Feature f = helper.FindFeature(feat.Guid);
                                
                                if (f != null && 0 == string.Compare(f.Filter, libFilter, true))
                                {
                                    OK = true;
                                    break;
                                }
                            }
                        }

                        if (!OK)
                        {
                            OK = (0 == string.Compare(lib.CustomFilter, solution.Name, true));
                        }
                        if (OK) break;
                    }

                    if (!OK) return false;
                }

                if (!IsBootloaderProject() && lib.IsBootloaderLibrary())
                {
                    return false;
                }

                // only add CLR libraries to a CLR project
                if (lib.Level == LibraryLevel.CLR && !this.IsClrProject)
                {
                    return false;
                }

                string projPath = lib.ProjectPath.ToLower();
                if (projPath.Contains(@"\devicecode\drivers\sample\"))
                {
                    return false;
                }

                if (projPath.Contains("\\solutions\\") && !projPath.Contains("\\solutions\\" + solution.Name.ToLower() + "\\"))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception validating solution libraries: " + e.ToString());
                return false;
            }

            return true;
        }

        // only to be called for LibraryCategories that are not dependencies of other libraries
        private bool ValidateLibraryCategory(LibraryCategory libc, Dictionary<string, MFComponent> selectedFeatures)
        {
            if (libc.Required) return true;

            if (libc.FeatureAssociations.Count > 0)
            {
                bool fOK = false;

                foreach (MFComponent feat in libc.FeatureAssociations)
                {
                    if (selectedFeatures.ContainsKey(feat.Guid.ToLower()))
                    {
                        fOK = true;
                        break;
                    }
                }

                if (!fOK) return false;
            }

            return true;
        }

        private void RecurseLibCatDeps(
                    MFComponent cmp,
                    Dictionary<string, MFComponent> unresolvedCatList,
                    InventoryHelper helper,
                    bool fDependency)
        {
            string key = cmp.Guid.ToLower();

            if (unresolvedCatList.ContainsKey(key)) return;

            if (cmp.ComponentType == MFComponentType.Library)
            {
                Library lib = helper.FindLibrary(cmp);

                if (lib != null)
                {
                    foreach (MFComponent dep in lib.Dependencies)
                    {
                        RecurseLibCatDeps(dep, unresolvedCatList, helper, true);
                    }
                }
            }
            else if (cmp.ComponentType == MFComponentType.LibraryCategory)
            {
                if (!unresolvedCatList.ContainsKey(key))
                {
                    unresolvedCatList[key] = cmp;
                }
            }
        }
        

        public void AnalyzeLibraries(InventoryHelper helper, MFSolution solution, List<LibraryCategory> unresolved, List<LibraryCategory> removed)
        {
            Dictionary<string, MFComponent> reqLibList = new Dictionary<string, MFComponent>();
            Dictionary<string, MFComponent> unresolvedTypes = new Dictionary<string, MFComponent>();
            List<MFComponent> features = new List<MFComponent>();
            Dictionary<string, MFComponent> preferredLibrary = new Dictionary<string, MFComponent>();
            
            Processor proc = helper.FindProcessor(solution.Processor.Guid);

            libraryCategoriesField.Clear();

            ///
            /// First add all the features and there dependencies
            ///
            if (this.IsClrProject)
            {
                foreach (MFComponent cmpFeat in featuresField)
                {
                    RecurseFeatureDeps(cmpFeat, features, helper);
                }

                ///
                /// get debug transport
                ///
                if (solution.TransportType != null)
                {
                    string key = solution.TransportType.Guid.ToLower();

                    foreach (MFComponent transportFeat in solution.TransportType.FeatureAssociations)
                    {
                        if (!features.Contains(transportFeat))
                        {
                            features.Add(transportFeat);
                        }
                    }
                }
            }

            this.featuresField.Clear();
            foreach (MFComponent cmpFeat in features)
            {
                Feature feat = helper.FindFeature(cmpFeat.Guid);

                this.featuresField.Add(cmpFeat);

                foreach (MFComponent cmp in feat.ComponentDependencies)
                {
                    string key = cmp.Guid.ToLower();

                    if (cmp.ComponentType == MFComponentType.LibraryCategory)
                    {
                        if (!unresolvedTypes.ContainsKey(key))
                        {
                            unresolvedTypes[key] = cmp;
                        }
                    }
                    else if (cmp.ComponentType == MFComponentType.Library)
                    {
                        reqLibList[key] = cmp;
                        RecurseLibCatDeps(cmp, unresolvedTypes, helper, false);
                    }
                }
            }

            ///
            /// Add any required libraries before analyzing all project libraries
            ///
            foreach (Library lib in helper.GetRequiredLibraries())
            {
                // Only add CLR libraries to CLR projects
                if (lib.Level == LibraryLevel.CLR && !this.IsClrProject)
                    continue;

                string key = lib.Guid.ToLower();

                if (!reqLibList.ContainsKey(key))
                {
                    MFComponent cmpNew = new MFComponent(MFComponentType.Library, lib.Name, lib.Guid, lib.ProjectPath);

                    reqLibList[key] = cmpNew;

                    RecurseLibCatDeps(cmpNew, unresolvedTypes, helper, false);
                }
            }

            ///
            /// Now add all required library categories
            ///
            foreach (LibraryCategory lc in helper.GetRequiredLibraryCategories())
            {
                if (lc.Level == LibraryLevel.CLR && !this.isClrProjectField) continue;

                string key = lc.Guid.ToLower();

                if (!unresolvedTypes.ContainsKey(key))
                {
                    MFComponent cmp = new MFComponent(MFComponentType.LibraryCategory, lc.Name, lc.Guid, lc.ProjectPath);

                    unresolvedTypes[lc.Guid.ToLower()] = cmp;
                }
            }

            ///
            /// Add cloned solution's solution-dependent projects
            /// 
            if (solution.m_cloneSolution != null && m_cloneProj != null)
            {
                foreach (MFComponent lib in m_cloneProj.librariesField)
                {
                    // If we have already added a library for the current solution, then just add a new component to the project
                    if (solution.m_clonedLibraryMap.ContainsKey(lib.Guid.ToUpper()))
                    {
                        Library newLib = solution.m_clonedLibraryMap[lib.Guid.ToUpper()];

                        librariesField.Add(new MFComponent(lib.ComponentType, newLib.Name, newLib.Guid, newLib.ProjectPath, lib.Conditional));
                    }
                    // otherwise, create a new library based on the cloned solution's library
                    else if (lib.ProjectPath.ToUpper().Contains("\\SOLUTIONS\\" + solution.m_cloneSolution.Name.ToUpper() + "\\"))
                    {
                        string name = CopyHelper.ReplaceText(lib.Name, solution.m_cloneSolution.Name, solution.Name);
                        string path = CopyHelper.ReplaceText(lib.ProjectPath, solution.m_cloneSolution.Name, solution.Name);
                        string guid = System.Guid.NewGuid().ToString("B").ToUpper();

                        // find cloned solution's library in the intventory
                        Library l = helper.FindLibrary(lib);
                        if (l != null)
                        {
                            Library l2 = new Library();

                            // copy and rename 
                            l.CopyTo(l2);

                            CopyHelper.Rename(l2, solution.m_cloneSolution.Name, solution.Name);

                            l2.Name = name;
                            l2.Guid = guid;
                            l2.ProjectPath = path;

                            // add library to inventory
                            helper.AddLibraryToInventory(l2, false, helper.DefaultInventory);

                            // hash to used so that we don't add multiple libraries for each project
                            solution.m_clonedLibraryMap[lib.Guid.ToUpper()] = l2;
                            // Add the component to this projects library list
                            MFComponent newCmp = new MFComponent(lib.ComponentType, name, guid, path, lib.Conditional);
                            librariesField.Add(newCmp);

                            if (l.HasLibraryCategory)
                            {
                                preferredLibrary[l.LibraryCategory.Guid.ToLower()] = newCmp;
                            }
                        }
                    }
                }
            }

            ///
            /// HACK - fix this to make it data driven (add a field on library categories that allows them to be required by a solution)
            /// 
            if (this.isClrProjectField)
            {
                LibraryCategory lc = helper.FindLibraryCategoryByName("WearLeveling_HAL");

                if (lc != null)
                {
                    string key = lc.Guid.ToLower();
                    if (unresolvedTypes.ContainsKey(key))
                    {
                        solution.m_solRequiredLibCats[key] = unresolvedTypes[key];
                    }
                }
            }
            else if(solution.m_solRequiredLibCats != null)
            {
                foreach (MFComponent cmp in solution.m_solRequiredLibCats.Values)
                {
                    string key = cmp.Guid.ToLower();

                    if (!unresolvedTypes.ContainsKey(key))
                    {
                        unresolvedTypes[key] = cmp;
                    }
                }
            }

            /// 
            /// Use a copy of the libraries, because the libraryField may be updated inside the loop
            /// 
            MFComponent[] __libs = new MFComponent[librariesField.Count];
            librariesField.CopyTo(__libs);

            List<MFComponent> libs = new List<MFComponent>(__libs);
            Dictionary<string, MFComponent> resolvedTypes = new Dictionary<string, MFComponent>();
            List<string> duplicateLibList = new List<string>();

            while (true)
            {
                List<MFComponent> remList = new List<MFComponent>();
                Dictionary<string, MFComponent> newUnresolvedTypes = new Dictionary<string, MFComponent>();

                foreach (MFComponent cmpLib in libs)
                {
                    Library lib = helper.FindLibrary(cmpLib);

                    bool fKeepingLib = false;

                    if (duplicateLibList.Contains(cmpLib.Guid.ToLower()) || lib == null || !ValidateLibrary(lib, solution, proc, helper))
                    {
                        librariesField.Remove(cmpLib);
                        remList.Add(cmpLib);
                        continue;
                    }

                    if (lib.HasLibraryCategory)
                    {
                        if (preferredLibrary.ContainsKey(lib.LibraryCategory.Guid.ToLower()) &&
                            string.Compare(cmpLib.Guid, preferredLibrary[lib.LibraryCategory.Guid.ToLower()].Guid, true) != 0)
                        {
                            fKeepingLib = true;
                        }
                        ///
                        /// Make sure the library selection matches the feature selection
                        ///
                        else if (this.isClrProjectField)
                        {
                            LibraryCategory lc = helper.FindLibraryCategory(lib.LibraryCategory.Guid);

                            if (lc != null)
                            {
                                if (lc.FeatureAssociations.Count > 0)
                                {
                                    bool fFeatureSelected = false;

                                    foreach (MFComponent feat in lc.FeatureAssociations)
                                    {
                                        if (features.Contains(feat))
                                        {
                                            fFeatureSelected = true;
                                            break;
                                        }
                                    }

                                    if (((!fFeatureSelected && !lib.IsStub) || (fFeatureSelected && lib.IsStub)) &&
                                        string.IsNullOrEmpty(cmpLib.Conditional))
                                    {
                                        librariesField.Remove(cmpLib);
                                        remList.Add(cmpLib);
                                        continue;
                                    }
                                }
                            }
                        }

                        if (!fKeepingLib)
                        {
                            string key = lib.LibraryCategory.Guid.ToLower();

                            if (unresolvedTypes.ContainsKey(key) || (!string.IsNullOrEmpty(cmpLib.Conditional) && resolvedTypes.ContainsKey(key)))
                            {
                                unresolvedTypes.Remove(key);
                                resolvedTypes[key] = lib.LibraryCategory;
                                fKeepingLib = true;
                                RecurseLibCatDeps(cmpLib, newUnresolvedTypes, helper, true);
                            }
                        }
                    }
                    else
                    {
                        fKeepingLib = true;
                    }

                    if (fKeepingLib)
                    {
                        remList.Add(cmpLib);

                        duplicateLibList.Add(cmpLib.Guid.ToLower());

                        foreach (MFComponent dep in lib.Dependencies)
                        {
                            if (dep.ComponentType == MFComponentType.Library)
                            {
                                if (duplicateLibList.Contains(dep.Guid.ToLower()) || librariesField.Contains(dep))
                                {
                                    continue;
                                }

                                Library libDep = helper.FindLibrary(dep);

                                if (libDep != null && libDep.HasLibraryCategory)
                                {
                                    string key = libDep.LibraryCategory.Guid.ToLower();
                                    if(!unresolvedTypes.ContainsKey(key) && !resolvedTypes.ContainsKey(key) && !newUnresolvedTypes.ContainsKey(key))
                                    {
                                        newUnresolvedTypes[key] = libDep.LibraryCategory;
                                    }
                                }
                                else
                                {
                                    remList.Add(dep);
                                    librariesField.Add(dep);
                                }
                            }
                        }
                    }
                }

                foreach (MFComponent cmp in remList)
                {
                    if (libs.Contains(cmp))
                    {
                        libs.Remove(cmp);
                    }
                }

                if (newUnresolvedTypes.Count == 0) break;

                foreach (string key in newUnresolvedTypes.Keys)
                {
                    if (!unresolvedTypes.ContainsKey(key) && !resolvedTypes.ContainsKey(key))
                    {
                        unresolvedTypes[key] = newUnresolvedTypes[key];
                    }
                }
            }

            foreach (MFComponent cmp in resolvedTypes.Values)
            {
                libraryCategoriesField.Add(cmp);
            }

            foreach (MFComponent cmp in libs)
            {
                librariesField.Remove(cmp);
            }

            foreach (MFComponent cmp in reqLibList.Values)
            {
                if (!librariesField.Contains(cmp))
                {
                    Library lib = helper.FindLibrary(cmp);
                    if (lib != null && ValidateLibrary(lib, solution, proc, helper))
                    {
                        librariesField.Add(cmp);
                    }
                }
            }

            foreach (MFComponent cmp in unresolvedTypes.Values)
            {
                LibraryCategory lc = helper.FindLibraryCategory(cmp.Guid);

                unresolved.Add(lc);
            }
        }

        public bool ContainsFeature(MFComponent compFeature)
        {
            foreach (MFComponent cmp in featuresField)
            {
                if (0 == string.Compare(cmp.Guid, compFeature.Guid, true))
                {
                    return true;
                }
            }

            return false;
        }

        public bool RemoveLibrary(MFComponent compLibrary)
        {
            if (compLibrary.ComponentType != MFComponentType.Library) return false;

            return librariesField.Remove(FindLibrary(compLibrary.Guid));
        }

        public MFComponent FindLibrary(string libGuid)
        {
            MFComponent cmpFeature = null;

            foreach (MFComponent lib in librariesField)
            {
                if (0 == string.Compare(lib.Guid, libGuid, true))
                {
                    return lib;
                }
            }

            return cmpFeature;
        }
    }

    public partial class MFSolution : object
    {
        internal MFSolution m_cloneSolution;
        internal Dictionary<string, Library> m_clonedLibraryMap;
        internal Dictionary<string, MFComponent> m_solRequiredLibCats;
        internal LibraryCategory m_transportType;

        public MFSolution()
        {
            this.m_cloneSolution = null;
            this.m_solRequiredLibCats = new Dictionary<string,MFComponent>();
            this.m_clonedLibraryMap = new Dictionary<string, Library>();
            this.descriptionField = "";
            this.documentationField = "";
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.nameField = "";
            this.projectsField = new System.Collections.Generic.List<MFProject>(); 
            this.versionField = new Version();
            this.propertiesField = new System.Collections.Generic.List<MFProperty>();
            this.isSolutionWizardVisibleField = true;
            this.authorField = "";
            this.itemsField = new List<MFBuildFile>();
            this.customFilterField = "";
            this.m_transportType = null;
            this.ENDIANNESS = "le";
        }

        public LibraryCategory TransportType
        {
            set { m_transportType = value; }
            get { return m_transportType; }
        }

        public void CopyTo(MFSolution dest, string newName)
        {
            Dictionary<string, string> hash = new Dictionary<string, string>();

            dest.m_cloneSolution = this;
            
            hash[this.nameField] = newName;
            hash[@"\$\(PLATFORM\)"] = newName;
            hash[@"\$\(TARGETPLATFORM\)"] = newName;

            CopyHelper.CopyTo(this, dest, hash);
        }

        public void Rename(string newName)
        {
            if (m_cloneSolution == null)
            {
                MFSolution sol = new MFSolution();
                
                CopyHelper.CopyTo(this, sol);

                m_cloneSolution = sol;
            }

            CopyHelper.Rename(this, this.nameField, newName);
        }
    }
    public partial class BuildTool : object
    {
        public BuildTool()
        {
            this.archiverField = new BuildToolDefine();
            this.asmCompilerField = new BuildToolDefine();
            this.binExtField = "bin";
            this.buildOptionsField = new ToolChainOptions();
            this.buildToolWrapperField = "";
            this.cCompilerField = new BuildToolDefine();
            this.cppCompilerField = new BuildToolDefine();
            this.dbgExtField = "axf";
            this.documentationField = "";
            this.fromELFField = new BuildToolDefine();
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.libExtField = "lib";
            this.linkerField = new BuildToolDefine();
            this.miscToolsField = new System.Collections.Generic.List<MiscBuildTool>();
            this.nameField = "";
            this.objExtField = "obj";
            this.supportedISAsField = new System.Collections.Generic.List<ISA>();
            this.toolPathField = "";
            this.versionField = new Version();
            this.supportedCpuNamesField = new System.Collections.Generic.List<string>();
            this.propertiesField = new System.Collections.Generic.List<MFProperty>();
            this.itemsField = new System.Collections.Generic.List<MFProperty>();
            this.isSolutionWizardVisibleField = true;
        }
        public void CopyTo(BuildTool dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    /*
    public partial class ProcessorType : object
    {
        public ProcessorType()
        {
            this.nameField = "";
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.versionField = new Version();
            this.buildToolOptionsField = new System.Collections.Generic.List<ToolChainOptions>();
            this.descriptionField = "";
            this.documentationField = "";
        }
        public void CopyTo(ProcessorType dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    */

    public partial class ISA : object
    {
        public ISA()
        {
            this.buildToolOptionsField = new ToolChainOptions();
            this.descriptionField = "";
            this.documentationField = "";
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.nameField = "";
            this.versionField = new Version();
        }
        public void CopyTo(ISA dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class Processor : object
    {
        public Processor()
        {
            this.buildToolOptionsField = new System.Collections.Generic.List<BuildToolRef>();
            this.descriptionField = "";
            this.documentationField = "";
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.nameField = "";
            this.supportedISAsField = new System.Collections.Generic.List<MFComponent>();
            this.versionField = new Version();
            this.isSolutionWizardVisibleField = true;
            this.customFilterField = "";
        }
        public void CopyTo(Processor dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class ToolChainOptions : object
    {
        public ToolChainOptions()
        {
            this.archiverFlagsField = new ToolOptions();
            this.asmFlagsField = new ToolOptions();
            this.c_CppFlagsField = new System.Collections.Generic.List<ToolFlag>();
            this.cFlagsField = new ToolOptions();
            this.commonFlagsField = new System.Collections.Generic.List<ToolFlag>();
            this.cppFlagsField = new ToolOptions();
            this.environmentVariablesField = new System.Collections.Generic.List<EnvVar>();
            this.linkerFlagsField = new ToolOptions();
        }
        public void CopyTo(ToolChainOptions dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class MemoryMap : object
    {
        public MemoryMap()
        {
            this.conditionalField = "";
            this.descriptionField = "";
            this.environmentVariablesField = new EnvVars();
            this.nameField = "";
            this.regionsField = new System.Collections.Generic.List<MemoryRegion>();
        }
        public void CopyTo(MemoryMap dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class MemorySymbol : object
    {
        public MemorySymbol()
        {
            this.conditionalField = "";
            this.descriptionField = "";
            this.nameField = "";
            this.optionsField = "";
            this.orderField = 0;
        }
        public void CopyTo(MemorySymbol dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class MemoryRegion : object
    {
        public MemoryRegion()
        {
            this.addressField = "";
            this.conditionalField = "";
            this.descriptionField = "";
            this.nameField = "";
            this.optionsField = "";
            this.sectionsField = new System.Collections.Generic.List<MemorySection>();
            this.sizeField = "";
        }
        public void CopyTo(MemoryRegion dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class MemorySection : object
    {
        public MemorySection()
        {
            this.addressField = "";
            this.conditionalField = "";
            this.descriptionField = "";
            this.nameField = "";
            this.optionsField = "";
            this.orderField = 0;
            this.sizeField = "";
            this.symbolsField = new System.Collections.Generic.List<MemorySymbol>();
        }
        public void CopyTo(MemorySection dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class EnvVars : object
    {
        public EnvVars()
        {
            this.conditionalField = "";
            this.envVarCollectionField = new System.Collections.Generic.List<EnvVar>();
            this.envVarsCollectionField = new System.Collections.Generic.List<EnvVars>();
            this.nameField = "";
        }
        public void CopyTo(EnvVars dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class EnvVar : object
    {
        public EnvVar()
        {
            this.conditionalField = "";
            this.nameField = "";
            this.valueField = "";
        }
        public void CopyTo(EnvVar dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class MFProperty : object
    {
        public MFProperty()
        {
            this.conditionField = "";
            this.nameField = "";
            this.valueField = "";
        }
        public void CopyTo(MFProperty dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class Library : object
    {
        System.Collections.Generic.List<ProjectTargetElement> targetsField;

        public Library()
        {
            this.targetsField = new System.Collections.Generic.List<ProjectTargetElement>();
            this.dependenciesField = new System.Collections.Generic.List<MFComponent>();
            this.descriptionField = "";
            this.documentationField = "";
            this.groupsField = "";
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.ignoreDefaultLibPathField = false;
            this.iSASpecificField = new MFComponent(MFComponentType.ISA);
            this.isStubField = false;
            this.levelField = LibraryLevel.HAL;
            this.libraryCategoryField = new MFComponent(MFComponentType.LibraryCategory);
            this.libraryFileField = "";
            this.nameField = "";
            this.platformIndependentField = false;
            this.processorSpecificField = new MFComponent(MFComponentType.Processor);
            this.requiredField = false;
            this.sizeField = "";
            //this.softDependenciesField = new System.Collections.Generic.List<MFComponent>();
            this.versionField = new Version();
            this.propertiesField = new System.Collections.Generic.List<MFProperty>();
            this.sourceFilesField = new System.Collections.Generic.List<MFBuildFile>();
            this.headerFilesField = new System.Collections.Generic.List<MFBuildFile>();
            this.fastCompileFilesField = new System.Collections.Generic.List<MFBuildFile>();
            this.otherFilesField = new System.Collections.Generic.List<MFBuildFile>();
            this.includePathsField = new System.Collections.Generic.List<MFBuildFile>();
            this.manifestFileField = "";
            this.projectPathField = "";
            this.isSolutionWizardVisibleField = true;
        }

        public bool HasLibraryCategory
        {
            get { return !string.IsNullOrEmpty(libraryCategoryField.Guid); }
            set { }
        }

        public System.Collections.Generic.List<ProjectTargetElement> Targets
        {
            get
            {
                return targetsField;
            }
            set
            {
                targetsField = value;
            }
        }

        public void CopyTo(Library dest)
        {
            CopyHelper.CopyTo(this, dest);
        }

        public bool IsBootloaderLibrary()
        {
            foreach (MFProperty prop in propertiesField)
            {
                if (0 == string.Compare(prop.Name, "ReduceSize", true))
                {
                    if (0 == string.Compare(prop.Value, "true", true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public partial class LibraryCategory : object
    {
        public LibraryCategory()
        {
            this.descriptionField = "";
            this.documentationField = "";
            this.groupsField = "";
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.ignoreDefaultLibPathField = false;
            this.levelField = LibraryLevel.CLR;
            this.nameField = "";
            this.requiredField = false;
            this.stubLibraryField = null;
            this.templatesField = new System.Collections.Generic.List<ApiTemplate>();
            this.versionField = new Version();
            this.projectPathField = "";
            this.libraryProjCacheField = new System.Collections.Generic.List<string>();
            this.featureAssociationsField = new List<MFComponent>();
            this.IsTransport = false;
        }
        public void CopyTo(LibraryCategory dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            LibraryCategory rhs = obj as LibraryCategory;

            if (rhs != null)
            {
                return (string.Compare(this.Guid, rhs.Guid, true) == 0);
            }

            return false;
        }
    }
    public partial class BuildToolParameters : object
    {
        public BuildToolParameters()
        {
            this.parametersField = new System.Collections.Generic.List<BuildScript>();
            this.postBuildField = new System.Collections.Generic.List<BuildScript>();
            this.preBuildField = new System.Collections.Generic.List<BuildScript>();
        }
        public void CopyTo(BuildToolParameters dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class ToolOptions : object
    {
        public ToolOptions()
        {
            this.buildToolParametersField = new BuildToolParameters();
            this.toolFlagsField = new System.Collections.Generic.List<ToolFlag>();
        }
        public void CopyTo(ToolOptions dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class MiscBuildTool : object
    {
        public MiscBuildTool()
        {
            this.buildToolField = new BuildToolDefine();
            this.buildToolOptionsField = new ToolOptions();
            this.buildToolWrapperField = "";
            this.environmentVariablesField = new System.Collections.Generic.List<EnvVar>();
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.nameField = "";
            this.toolPathField = "";
        }
        public void CopyTo(MiscBuildTool dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    /*
    public partial class BSP : object
    {
        public BSP()
        {
            this.buildToolOptionsField = new System.Collections.Generic.List<BuildToolRef>();
            this.descriptionField = "";
            this.guidField = System.Guid.NewGuid().ToString("B");
            this.nameField = "";
            this.processorField = null;
            this.versionField = new Version();
        }
    }
    */
    public partial class InventoryCollection : object
    {
        public InventoryCollection()
        {
            this.inventoriesField = new System.Collections.Generic.List<Inventory>();
        }
    }
    public partial class Version : object
    {
        public Version()
        {
            this.buildField = "0";
            this.dateField = System.DateTime.Now;
            this.extraField = "";
            this.majorField = "4";
            this.minorField = "0";
            this.revisionField = "0";
            this.authorField = "";
        }
        public void CopyTo(Version dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class MFBuildFile : object
    {
        public MFBuildFile()
        {
            this.conditionField = "";
            this.fileField = "";
            this.itemNameField = "";
        }
        public void CopyTo(MFBuildFile dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
    public partial class BuildToolRef : object
    {
        public BuildToolRef()
        {
            this.buildOptionsField = new ToolChainOptions();
            this.guidField = "";
            this.nameField = "";
        }
        public void CopyTo(BuildToolRef dest)
        {
            CopyHelper.CopyTo(this, dest);
        }
    }
}