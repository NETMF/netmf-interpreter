using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using _BE=Microsoft.Build.Evaluation;

namespace Microsoft.SPOT.Tasks.Internal
{
    internal static class TransmorgificationUtilities
    {
        static internal bool IsInRestrictedList(string fileName)
        {
            string fileExt = Path.GetExtension(fileName);

            string[] badExtensions = new string[] { ".suo", ".user", ".ncb", ".incr", ".projdata", ".tlb", ".olb", ".resources", ".old", ".exp", ".lib", ".obj", ".pch", ".idb" };
            foreach (string str in badExtensions)
            {
                if (string.Compare(str, fileExt, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        #region Template Parameter Replacements
        static internal void MakeTemplateReplacements(bool isAProject, bool creatingProject,
            string rootNamespace, string sourceFileName, ref string fileContents)
        {
            if (isAProject)
            {
                if (!string.IsNullOrEmpty(rootNamespace))
                {
                    fileContents = fileContents.Replace("<StartupObject>" + rootNamespace + ".", "<StartupObject>$safeprojectname$.");
                    fileContents = fileContents.Replace("<DocumentationFile>" + rootNamespace + ".", "<DocumentationFile>$safeprojectname$.");
                    fileContents = fileContents.Replace("<RootNamespace>" + rootNamespace + "</RootNamespace>", "<RootNamespace>$safeprojectname$</RootNamespace>");
                    fileContents = fileContents.Replace("<AssemblyName>" + rootNamespace + "</AssemblyName>", "<AssemblyName>$safeprojectname$</AssemblyName>");
                }
            }
            else
            {
                if (creatingProject == true)
                {
                    //Exporting the entire project ... replace projectname only; this may not work as desired for Web projects,
                    //we don't have any so we don't care.
                    fileContents = WordReplace(fileContents, rootNamespace, "$safeprojectname$");
                }
                else
                {
                    //Exporting a single item from a project. Replace root name space (if given)
                    //and item name
                    if (!string.IsNullOrEmpty(rootNamespace))
                    {
                        fileContents = WordReplace(fileContents, rootNamespace, "$rootnamespace$");
                    }

                    string itemName = Path.GetFileNameWithoutExtension(sourceFileName);
                    int iDotIndex = itemName.LastIndexOf(".");
                    if (iDotIndex != -1)
                    {
                        //This is intended to fix the foo.designer.vb case (GetFileNameWithoutExtension()
                        //returns foo.designer, and we want to replace just foo
                        itemName = itemName.Substring(0, iDotIndex);
                    }

                    fileContents = WordReplace(fileContents, itemName, "$safeitemname$");
                }
            }
        }


        /// <summary>
        /// Replace all delimited occurances of search with replace in string source
        /// An occurance is delimited if it begins and ends with something in the delimiter list.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string WordReplace(string source, string search, string replace)
        {
            const string delimiters = "\\/.,;-()<>{}\'\"~!+=`@#$%^&*[]|:?";

            int iSearchFrom = 0;
            while (true)
            {
                int iStart = source.IndexOf(search, iSearchFrom, StringComparison.Ordinal);
                if (iStart == -1)
                {
                    //All done
                    break;
                }
                //By default, the search resumes one character past the end of the last hit
                iSearchFrom = iStart + search.Length;

                //Delimiter at the start?
                if ((iStart == 0) ||
                    Char.IsWhiteSpace(source[iStart - 1]) ||
                    (delimiters.IndexOf(source[iStart - 1]) != -1))
                {
                    //Delimiter at the end?
                    if ((source.Length == iStart + search.Length) ||
                        Char.IsWhiteSpace(source[iStart + search.Length]) ||
                        (delimiters.IndexOf(source[iStart + search.Length]) != -1))
                    {
                        //Delimiters at both ends ... do the replacement.
                        source = (source.Remove(iStart, search.Length)).Insert(iStart, replace);

                        //Start searching just after the replaced string
                        iSearchFrom = iStart + replace.Length;
                    }
                }
            }

            return source;
        }

        static internal void MakeTemplateReplacements(bool isAProject, bool creatingProject,
            string rootNamespace, string sourceFileName, _BE.Project proj)
        {
            if (isAProject)
            {
                if (!string.IsNullOrEmpty(rootNamespace))
                {
                    _BE.ProjectProperty prop;
                    string name;

                    name = "StartupObject";
                    prop = proj.GetProperty(name);
                    if (prop != null) proj.SetProperty(name, prop.UnevaluatedValue.Replace(rootNamespace + ".", "$safeprojectname$."));

                    name = "DocumentationFile";
                    prop = proj.GetProperty(name);
                    if (prop != null) proj.SetProperty(name, prop.UnevaluatedValue.Replace(rootNamespace + ".", "$safeprojectname$."));

                    name = "RootNamespace";
                    prop = proj.GetProperty(name);
                    if (prop != null) proj.SetProperty(name, prop.UnevaluatedValue.Replace(rootNamespace, "$safeprojectname$"));

                    name = "AssemblyName";
                    prop = proj.GetProperty(name);
                    if (prop != null) proj.SetProperty(name, prop.UnevaluatedValue.Replace(rootNamespace, "$safeprojectname$"));
                }
            }
            else
            {
                if (creatingProject == true)
                {
                    //Exporting the entire project ... replace projectname only; this may not work as desired for Web projects,
                    //we don't have any so we don't care.
                    WordReplace(proj, rootNamespace, "$safeprojectname$");
                }
                else
                {
                    //Exporting a single item from a project. Replace root name space (if given)
                    //and item name
                    if (!string.IsNullOrEmpty(rootNamespace))
                    {
                        WordReplace(proj, rootNamespace, "$rootnamespace$");
                    }

                    string itemName = Path.GetFileNameWithoutExtension(sourceFileName);
                    int iDotIndex = itemName.LastIndexOf(".");
                    if (iDotIndex != -1)
                    {
                        //This is intended to fix the foo.designer.vb case (GetFileNameWithoutExtension()
                        //returns foo.designer, and we want to replace just foo
                        itemName = itemName.Substring(0, iDotIndex);
                    }

                    WordReplace(proj, itemName, "$safeitemname$");
                }
            }
        }

        
        /// <summary>
        /// Replace all delimited occurances of search with replace in string source
        /// An occurance is delimited if it begins and ends with something in the delimiter list.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static void WordReplace(_BE.Project proj, string search, string replace)
        {
            const string delimiters = "\\/.,;-()<>{}\'\"~!+=`@#$%^&*[]|:?";

            foreach (_BE.ProjectProperty prop in proj.Properties)
            {
                string source = prop.UnevaluatedValue;
                int iStart = source.IndexOf(search, 0, StringComparison.Ordinal);
                if (iStart == -1)
                {
                    //All done
                    continue;
                }

                //Delimiter at the start?
                if ((iStart == 0) ||
                    Char.IsWhiteSpace(source[iStart - 1]) ||
                    (delimiters.IndexOf(source[iStart - 1]) != -1))
                {
                    //Delimiter at the end?
                    if ((source.Length == iStart + search.Length) ||
                        Char.IsWhiteSpace(source[iStart + search.Length]) ||
                        (delimiters.IndexOf(source[iStart + search.Length]) != -1))
                    {
                        //Delimiters at both ends ... do the replacement.
                        prop.UnevaluatedValue = (source.Remove(iStart, search.Length)).Insert(iStart, replace);
                    }
                }
            }
        }
        #endregion

        #region Mime Type Detection from File Extension
        private static string GetExtensionMimeType(string ext)
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(ext, false);
            if (key == null)
                return "";
            return ((string)key.GetValue("Content Type", "")).ToUpperInvariant();
        }

        internal static bool ValidMimeTypeForReplacements(string fileName)
        {
            string ext = Path.GetExtension(fileName);

            string mimeType = GetExtensionMimeType(ext);
            if (mimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
                return true;

            //Some file extensions below will have the 'text' mimetype already set.  So listing it here is redundant.
            //Here is a list of extensions intentionally left OFF of the list: ".bat"
            foreach (string str in new string[] { ".cpp", ".h", ".cxx", ".hxx", ".cs", ".txt", ".vb", ".resx", ".aspx", ".ascx", ".asax", ".css", ".master", ".skin", ".cpp", ".h", ".jsl", ".csproj", ".vbproj", ".vjsproj", ".vcproj", ".il", ".settings", ".myapp", ".config", ".reg", ".rgs", ".vstemplate", ".vscontent", ".xsd"
                                                , ".ashx" , ".datasource" , ".generictest" , ".java" , ".loadtest" , ".map", ".htm" , ".html" , ".mht"
                                                , ".mtx" , ".orderedtest" , ".settings" , ".sql" , ".testrunconfig" , ".webtest" , ".wsdl" , ".wsf"
                                                , ".xml" , ".xslt" , ".inf" , ".ini" , ".xaml" , ".mcml" , ".js" , ".vbs" , ".c" , ".inl"})
            {
                if (string.Compare(ext, str, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
            //text/plain
            //text/xml
            //text/css
            //text/x-component
            //text/html
            //text/webviewhtml
            //text/java
        }
        #endregion


        #region Zip Filename Handling functions
        /// <summary>
        /// Return false if any characters in the file name are outside the range [0...127]
        /// (which can cause problems for zip files).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool IsLegalZipFileName(string name)
        {
            foreach (char c in name)
            {
                if (c >= 128)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
