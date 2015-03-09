#region Using directives

using System;
using System.Collections;
using System.Resources;
using System.Reflection;
using System.Threading;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Configuration;
using System.Security;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace Microsoft.SPOT.Tasks
{
    #region RippedOffMsbuildCode
    static internal class ExceptionHandling
    {
        /// <summary>
        /// If the given exception is file IO related then return.
        /// Otherwise, rethrow the exception.
        /// </summary>
        /// <param name="e">The exception to check.</param>
        internal static void RethrowUnlessFileIO(Exception e)
        {
            if
            (
                e is UnauthorizedAccessException
                || e is ArgumentNullException
                || e is PathTooLongException
                || e is DirectoryNotFoundException
                || e is NotSupportedException
                || e is ArgumentException
                || e is SecurityException
                || e is IOException
            )
            {
                return;
            }

            Debug.Assert(false, "Exception unexpected for this File IO case. Please open a bug that we need to add a 'catch' block for this exception. Look at the build log for more details including a stack trace.");
            throw e;
        }
    }


    /// <remarks>
    /// Represents a cache of inputs to a compilation-style task.
    /// </remarks>
    [Serializable()]
    internal class Dependencies
    {
        /// <summary>
        /// Hashtable of other dependency files.
        /// Key is filename and value is DependencyFile.
        /// </summary>
        private Hashtable dependencies = new Hashtable();

        /// <summary>
        /// Look up a dependency file. Return null if its not there.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal DependencyFile GetDependencyFile( string filename )
        {
            return (DependencyFile)dependencies[filename];
        }


        /// <summary>
        /// Add a new dependency file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal void AddDependencyFile( string filename, DependencyFile file )
        {
            dependencies[filename] = file;
        }

        /// <summary>
        /// Remove new dependency file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal void RemoveDependencyFile( string filename )
        {
            dependencies.Remove( filename );
        }

        /// <summary>
        /// Remove all entries from the dependency table.
        /// </summary>
        internal void Clear()
        {
            dependencies.Clear();
        }
    }


    /// <remarks>
    /// Represents a single input to a compilation-style task.
    /// Keeps track of timestamp for later comparison.
    /// </remarks>
    [Serializable]
    internal class DependencyFile
    {
        // Filename
        private string filename;

        // Date and time the file was last modified
        private DateTime lastModified;

        // Whether the file exists or not.
        bool exists = false;

        /// <summary>
        /// The name of the file.
        /// </summary>
        /// <value></value>
        internal string FileName
        {
            get { return filename; }
        }

        /// <summary>
        /// The last-modified timestamp when the class was instantiated.
        /// </summary>
        /// <value></value>
        internal DateTime LastModified
        {
            get { return lastModified; }
        }

        /// <summary>
        /// Returns true if the file existed when this class was instantiated.
        /// </summary>
        /// <value></value>
        internal bool Exists
        {
            get { return exists; }
        }

        /// <summary>
        /// Construct.
        /// </summary>
        /// <param name="filename">The file name.</param>
        internal DependencyFile( string filename )
        {
            this.filename = filename;

            if(File.Exists( FileName ))
            {
                this.lastModified = File.GetLastWriteTime( FileName );
                this.exists = true;
            }
            else
            {
                this.exists = false;
            }
        }

        /// <summary>
        /// Checks whether the file has changed since the last time a timestamp was recorded.
        /// </summary>
        /// <returns></returns>
        internal bool HasFileChanged()
        {
            // Obviously if the file no longer exists then we are not up to date.
            if(!File.Exists( filename ))
            {
                return true;
            }

            // Check the saved timestamp against the current timestamp.
            // If they are different then obviously we are out of date.
            DateTime curLastModified = File.GetLastWriteTime( filename );
            if(curLastModified != lastModified)
            {
                return true;
            }

            // All checks passed -- the info should still be up to date.
            return false;
        }
    }

    /// <remarks>
    /// Base class for task state files.
    /// </remarks>
    [Serializable()]
    internal class StateFileBase
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        internal StateFileBase()
        {
            // do nothing
        }

        /// <summary>
        /// Writes the contents of this object out to the specified file.
        /// </summary>
        /// <param name="stateFile"></param>
        virtual internal void SerializeCache( string stateFile, TaskLoggingHelper log )
        {
            try
            {
                if(stateFile != null && stateFile.Length > 0)
                {
                    if(File.Exists( stateFile ))
                    {
                        File.Delete( stateFile );
                    }

                    using(FileStream s = new FileStream( stateFile, FileMode.CreateNew ))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize( s, this );
                    }
                }
            }
            catch(Exception e)
            {
                // If there was a problem writing the file (like it's read-only or locked on disk, for
                // example), then eat the exception and log a warning.  Otherwise, rethrow.
                ExceptionHandling.RethrowUnlessFileIO( e );

                // Not being able to serialize the cache is not an error, but we let the user know anyway.
                // Don't want to hold up processing just because we couldn't read the file.
                log.LogWarning( "Could not write state file {0} ({1})", stateFile, e.Message );
            }
        }

        /// <summary>
        /// Reads the specified file from disk into a StateFileBase derived object.
        /// </summary>
        /// <param name="stateFile"></param>
        /// <returns></returns>
        static internal StateFileBase DeserializeCache( string stateFile, TaskLoggingHelper log, Type requiredReturnType )
        {
            StateFileBase retVal = null;

            // First, we read the cache from disk if one exists, or if one does not exist
            // then we create one.
            try
            {
                if(stateFile != null && stateFile.Length > 0 && File.Exists( stateFile ))
                {
                    using(FileStream s = new FileStream( stateFile, FileMode.Open ))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        retVal = (StateFileBase)formatter.Deserialize( s );

                        if((retVal != null) && (!requiredReturnType.IsInstanceOfType( retVal )))
                        {
                            log.LogWarning( "Could not write state file {0} (Incompatible state file type)", stateFile);
                            retVal = null;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                // The deserialization process seems like it can throw just about
                // any exception imaginable.  Catch them all here.

                // Not being able to deserialize the cache is not an error, but we let the user know anyway.
                // Don't want to hold up processing just because we couldn't read the file.
                log.LogWarning( "Could not read state file {0} ({1})", stateFile, e.Message );
            }

            return retVal;
        }

        /// <summary>
        /// Deletes the state file from disk
        /// </summary>
        /// <param name="stateFile"></param>
        /// <param name="log"></param>
        static internal void DeleteFile( string stateFile, TaskLoggingHelper log )
        {
            try
            {
                if(stateFile != null && stateFile.Length > 0)
                {
                    if(File.Exists( stateFile ))
                    {
                        File.Delete( stateFile );
                    }
                }
            }
            catch(Exception e)
            {
                // If there was a problem deleting the file (like it's read-only or locked on disk, for
                // example), then eat the exception and log a warning.  Otherwise, rethrow.
                ExceptionHandling.RethrowUnlessFileIO( e );

                log.LogWarning( "Could not delete state file {0} ({1})", stateFile, e.Message );
            }
        }
    }


    /// <remarks>
    /// This class is a caching mechanism for the resgen task to keep track of linked
    /// files within processed .resx files.
    /// </remarks>
    [Serializable()]
    internal sealed class ResGenDependencies : StateFileBase
    {
        /// <summary>
        /// The list of resx files.
        /// </summary>
        private Dependencies resXFiles = new Dependencies();

        /// <summary>
        /// A newly-created ResGenDependencies is not dirty.
        /// What would be the point in saving the default?
        /// </summary>
        [NonSerialized]
        private bool isDirty = false;

        /// <summary>
        ///  This is the directory that will be used for resolution of files linked within a .resx.
        ///  If this is NULL then we use the directory in which the .resx is in (that should always
        ///  be the default!)
        /// </summary>
        private string baseLinkedFileDirectory;

        /// <summary>
        /// Construct.
        /// </summary>
        internal ResGenDependencies()
        {
        }

        internal string BaseLinkedFileDirectory
        {
            get
            {
                return baseLinkedFileDirectory;
            }
            set
            {
                if(value == null && baseLinkedFileDirectory == null)
                {
                    // No change
                    return;
                }
                else if((value == null && baseLinkedFileDirectory != null) ||
                         (value != null && baseLinkedFileDirectory == null) ||
                         (String.Compare( baseLinkedFileDirectory, value, true, CultureInfo.InvariantCulture ) != 0))
                {
                    // Ok, this is slightly complicated.  Changing the base directory in any manner may
                    // result in changes to how we find .resx files.  Therefore, we must clear our out
                    // cache whenever the base directory changes.
                    resXFiles.Clear();
                    isDirty = true;
                    baseLinkedFileDirectory = value;
                }
            }
        }

        internal bool UseSourcePath
        {
            set
            {
                // Ensure that the cache is properly initialized with respect to how resgen will
                // resolve linked files within .resx files.  ResGen has two different
                // ways for resolving relative file-paths in linked files. The way
                // that ResGen resolved relative paths before Whidbey was always to
                // resolve from the current working directory. In Whidbey a new command-line
                // switch "/useSourcePath" instructs ResGen to use the folder that
                // contains the .resx file as the path from which it should resolve
                // relative paths. So we should base our timestamp/existence checking
                // on the same switch & resolve in the same manner as ResGen.
                BaseLinkedFileDirectory = value ? null : Environment.CurrentDirectory;
            }
        }

        internal ResXFile GetResXFileInfo( string resxFile )
        {
            // First, try to retrieve the resx information from our hashtable.
            ResXFile retVal = (ResXFile)resXFiles.GetDependencyFile( resxFile );

            if(retVal == null)
            {
                // Ok, the file wasn't there.  Add it to our cache and return it to the caller.
                retVal = AddResxFile( resxFile );
            }
            else
            {
                // The file was there.  Is it up to date?  If not, then we'll have to refresh the file
                // by removing it from the hashtable and readding it.
                if(retVal.HasFileChanged())
                {
                    resXFiles.RemoveDependencyFile( resxFile );
                    isDirty = true;
                    retVal = AddResxFile( resxFile );
                }
            }

            return retVal;
        }

        private ResXFile AddResxFile( string file )
        {
            // This method adds a .resx file "file" to our .resx cache.  The method causes the file
            // to be cracked for contained files.

            ResXFile resxFile = new ResXFile( file, BaseLinkedFileDirectory );
            resXFiles.AddDependencyFile( file, resxFile );
            isDirty = true;
            return resxFile;
        }
        /// <summary>
        /// Writes the contents of this object out to the specified file.
        /// </summary>
        /// <param name="stateFile"></param>
        override internal void SerializeCache( string stateFile, TaskLoggingHelper log )
        {
            base.SerializeCache( stateFile, log );
            isDirty = false;
        }

        /// <summary>
        /// Reads the .cache file from disk into a ResGenDependencies object.
        /// </summary>
        /// <param name="stateFile"></param>
        /// <param name="useSourcePath"></param>
        /// <returns></returns>
        internal static ResGenDependencies DeserializeCache( string stateFile, bool useSourcePath, TaskLoggingHelper log )
        {
            ResGenDependencies retVal = (ResGenDependencies)StateFileBase.DeserializeCache( stateFile, log, typeof( ResGenDependencies ) );

            if(retVal == null)
            {
                retVal = new ResGenDependencies();
            }

            // Ensure that the cache is properly initialized with respect to how resgen will
            // resolve linked files within .resx files.  ResGen has two different
            // ways for resolving relative file-paths in linked files. The way
            // that ResGen resolved relative paths before Whidbey was always to
            // resolve from the current working directory. In Whidbey a new command-line
            // switch "/useSourcePath" instructs ResGen to use the folder that
            // contains the .resx file as the path from which it should resolve
            // relative paths. So we should base our timestamp/existence checking
            // on the same switch & resolve in the same manner as ResGen.
            retVal.UseSourcePath = useSourcePath;

            return retVal;
        }

        /// <remarks>
        /// Represents a single .resx file in the dependency cache.
        /// </remarks>
        [Serializable()]
        internal sealed class ResXFile : DependencyFile
        {
            // Files contained within this resx file.
            private string[] linkedFiles;

            internal string[] LinkedFiles
            {
                get { return linkedFiles; }
            }

            internal ResXFile( string filename, string baseLinkedFileDirectory )
                : base( filename )
            {
                // Creates a new ResXFile object and populates the class member variables
                // by computing a list of linked files within the .resx that was passed in.
                //
                // filename is the filename of the .resx file that is to be examined.

                if(File.Exists( FileName ))
                {
                    linkedFiles = ResXFile.GetLinkedFiles( filename, baseLinkedFileDirectory );
                }
            }

            /// <summary>
            /// Given a .RESX file, returns all the linked files that are referenced within that .RESX.
            /// </summary>
            /// <param name="filename"></param>
            /// <param name="baseLinkedFileDirectory"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException">May be thrown if Resx is invalid. May contain XmlException.</exception>
            /// <exception cref="XmlException">May be thrown if Resx is invalid</exception>
            internal static string[] GetLinkedFiles( string filename, string baseLinkedFileDirectory )
            {
                // This method finds all linked .resx files for the .resx file that is passed in.
                // filename is the filename of the .resx file that is to be examined.

                // Construct the return array
                ArrayList retVal = new ArrayList();

#if !EVERETT_BUILD
                using(ResXResourceReader resxReader = new ResXResourceReader( filename ))
                {
                    // Tell the reader to return ResXDataNode's instead of the object type
                    // the resource becomes at runtime so we can figure out which files
                    // the .resx references
                    resxReader.UseResXDataNodes = true;

                    // First we need to figure out where the linked file resides in order
                    // to see if it exists & compare its timestamp, and we need to do that
                    // comparison in the same way ResGen does it. ResGen has two different
                    // ways for resolving relative file-paths in linked files. The way
                    // that ResGen resolved relative paths before Whidbey was always to
                    // resolve from the current working directory. In Whidbey a new command-line
                    // switch "/useSourcePath" instructs ResGen to use the folder that
                    // contains the .resx file as the path from which it should resolve
                    // relative paths. So we should base our timestamp/existence checking
                    // on the same switch & resolve in the same manner as ResGen.
                    resxReader.BasePath = (baseLinkedFileDirectory == null) ? Path.GetDirectoryName( filename ) : baseLinkedFileDirectory;

                    foreach(DictionaryEntry dictEntry in resxReader)
                    {
                        if((dictEntry.Value != null) && (dictEntry.Value is ResXDataNode))
                        {
                            ResXFileRef resxFileRef = ((ResXDataNode)dictEntry.Value).FileRef;
                            if(resxFileRef != null)
                                retVal.Add( resxFileRef.FileName );
                        }
                    }
                }
#endif
                return (string[])retVal.ToArray( typeof( string ) );
            }
        }

        /// <summary>
        /// Whether this cache is dirty or not.
        /// </summary>
        internal bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
        }
    }
    #endregion //RippedOffMsBuildCode

    public class GenerateTinyResource : Task
    {
        //See MSBuild sources sources\vsproject\xmake\XMakeTasks\GenerateResource.cs
        //Can we override MSbuild's class???

       #region Fields
        // This cache helps us track the linked resource files listed inside of a resx resource file
        private ResGenDependencies cache;

        // This is where we store the list of input files/sources
        private ITaskItem[] sources = null;

        // Indicates whether the resource reader should use the source file's
        // directory to resolve relative file paths.
        private bool useSourcePath = false;

        // This is needed for the actual items from the project
        private ITaskItem[] references = null;

        // This is the path/name of the dependency cache file
        private ITaskItem stateFile = null;

        // This list is all of the resource file(s) generated by the task
        private ITaskItem[] outputResources = null;

        // List of those output resources that were not actually created, due to an error
        private ArrayList unsuccessfullyCreatedOutFiles = new ArrayList();

        // Storage for names of *all files* written to disk.
        private ArrayList filesWritten = new ArrayList();

        // When true, a separate AppDomain is always created.
        private bool neverLockTypeAssemblies = false;

        #endregion  // fields
        #region Properties

        /// <summary>
        /// The names of the items to be converted. The extension must be one of the
        //  following: .txt, .resx or .resources.
        /// </summary>
        [Required]
        public ITaskItem[] Sources
        {
            set { sources = value; }
            get { return sources; }
        }

        /// <summary>
        /// Indicates whether the resource reader should use the source file's directory to
        /// resolve relative file paths.
        /// </summary>
        public bool UseSourcePath
        {
            set { useSourcePath = value; }
            get { return useSourcePath; }
        }

        /// <summary>
        /// Resolves types in ResX files (XML resources) for Strongly Typed Resources
        /// </summary>
        public ITaskItem[] References
        {
            set { references = value; }
            get { return references; }
        }

        /// <summary>
        /// This is the path/name of the file containint the dependency cache
        /// </summary>
        public ITaskItem StateFile
        {
            set { stateFile = value; }
            get { return stateFile; }
        }

        /// <summary>
        /// The name(s) of the resource file to create. If the user does not specify this
        /// attribute, the task will append a .resources extension to each input filename
        /// argument and write the file to the directory that contains the input file.
        /// Includes any output files that were already up to date, but not any output files
        /// that failed to be written due to an error.
        /// </summary>
        [Output]
        public ITaskItem[] OutputResources
        {
            set { outputResources = value; }
            get { return outputResources; }
        }

        /// <summary>
        /// Storage for names of *all files* written to disk.  This is part of the implementation
        /// for Clean, and contains the OutputResources items and the StateFile item.
        /// Includes any output files that were already up to date, but not any output files
        /// that failed to be written due to an error.
        /// </summary>
        [Output]
        public ITaskItem[] FilesWritten
        {
            set { /*Do Nothing, Inputs not Allowed*/  }
            get
            {
                return (ITaskItem[])filesWritten.ToArray(typeof(ITaskItem));
            }
        }

        /// <summary>
        /// (default = false)
        /// When true, a new AppDomain is always created to evaluate the .resx files.
        /// When false, a new AppDomain is created only when it looks like a user's
        ///  assembly is referenced by the .resx.
        /// </summary>
        public bool NeverLockTypeAssemblies
        {
            set { neverLockTypeAssemblies = value; }
            get { return neverLockTypeAssemblies; }
        }

#endregion // properties
      /// <summary>
        /// This is the main entry point for  the GenerateResource task.
        /// </summary>
        /// <owner>ChadR</owner>
        /// <returns>true, if task executes successfully</returns>
        public override bool Execute()
        {
            // If there are no sources to process, just return (with success) and report the condition.
            if ((Sources == null) || (Sources.Length == 0))
            {
                Log.LogMessage(MessageImportance.Low, "GenerateResource.NoSources");
                // Indicate we generated nothing
                OutputResources = null;
                return true;
            }

            if (!ValidateParameters())
            {
                // Indicate we generated nothing
                OutputResources = null;
                return false;
            }

            // In the case that OutputResources wasn't set, build up the outputs by transforming the Sources
            if (!CreateOutputResourcesNames())
            {
                // Indicate we generated nothing
                OutputResources = null;
                return false;
            }

            // First we look to see if we have a resgen linked files cache.  If so, then we can use that
            // cache to speed up processing.
            ReadStateFile();

            bool nothingOutOfDate = true;

            ArrayList inputsToProcess = new ArrayList();
            ArrayList outputsToProcess = new ArrayList();

            // decide what sources we need to build
            for (int i = 0; i < Sources.Length; ++i)
            {
                // Attributes from input items are forwarded to output items.
                //Sources[i].CopyMetadataTo(OutputResources[i]);

                if (!File.Exists(Sources[i].ItemSpec))
                {
                    // Error but continue with the files that do exist
                    Log.LogError("GenerateResource.ResourceNotFound", Sources[i].ItemSpec);
                    unsuccessfullyCreatedOutFiles.Add(OutputResources[i].ItemSpec);
                }
                else
                {
                    // check to see if the output resources file (and, if it is a .resx, any linked files)
                    // is up to date compared to the input file
                    if (ShouldRebuildResgenOutputFile(Sources[i].ItemSpec, OutputResources[i].ItemSpec))
                    {
                        nothingOutOfDate = false;

                        inputsToProcess.Add( Sources[i] );
                        outputsToProcess.Add( OutputResources[i] );
                    }
                }
            }

            if (nothingOutOfDate)
            {
                Log.LogMessage("GenerateResource.NothingOutOfDate");
            }
            else
            {
                // Prepare list of referenced assemblies
                AssemblyName[] assemblyList;
                try
                { //only load system.drawing, mscorlib.  no parameters needed here?!!
                    assemblyList = LoadReferences();
                }
                catch(ArgumentException e)
                {
                    Log.LogError("GenerateResource.ReferencedAssemblyNotFound - {0}: {1}", e.ParamName, e.Message);
                    OutputResources = null;
                    return false;
                }

                // Figure out whether a separate AppDomain is required because an assembly would be locked.
                bool needSeparateAppDomain = NeedSeparateAppDomain();

                AppDomain appDomain = null;
                ProcessResourceFiles process = null;

                try
                {
                    if (needSeparateAppDomain)
                    {
                        appDomain = AppDomain.CreateDomain
                        (
                            "generateResourceAppDomain",
                            null,
                            AppDomain.CurrentDomain.SetupInformation
                        );

                        object obj = appDomain.CreateInstanceFromAndUnwrap
                           (
                               typeof(ProcessResourceFiles).Module.FullyQualifiedName,
                               typeof(ProcessResourceFiles).FullName
                           );

                        Type processType = obj.GetType();
                        //ErrorUtilities.VerifyThrow(processType == typeof(ProcessResourceFiles), "Somehow got a wrong and possibly incompatible type for ProcessResourceFiles.");

                        process = (ProcessResourceFiles)obj;
                    }
                    else
                    {
                        process = new ProcessResourceFiles();
                    }

                    //setup strongly typed class name??

                    process.Run( Log, assemblyList, (ITaskItem[])inputsToProcess.ToArray( typeof( ITaskItem ) ), (ITaskItem[])outputsToProcess.ToArray( typeof( ITaskItem ) ),
                        UseSourcePath );

                    if (null != process.UnsuccessfullyCreatedOutFiles)
                    {
                        foreach (string item in process.UnsuccessfullyCreatedOutFiles)
                        {
                            this.unsuccessfullyCreatedOutFiles.Add(item);
                        }
                    }
                    process = null;
                }
                finally
                {
                    if (needSeparateAppDomain && appDomain != null)
                    {
                        AppDomain.Unload(appDomain);
                        process = null;
                        appDomain = null;
                    }
                }
            }

            // And now we serialize the cache to save our resgen linked file resolution for later use.
            WriteStateFile();

            RemoveUnsuccessfullyCreatedResourcesFromOutputResources();

            RecordFilesWritten();

            return true; //!Log.HasLoggedErrors;
        }

        /// <summary>
        /// Create the AssemblyName array that ProcessResources will need.
        /// </summary>
        /// <returns>AssemblyName array</returns>
        /// <owner>danmose</owner>
        /// <throws>ArgumentException</throws>
        private AssemblyName[] LoadReferences()
        {
            if (References == null)
            {
                return null;
            }

            AssemblyName[] assemblyList = new AssemblyName[References.Length];

            for (int i = 0; i < References.Length; i++)
            {
                try
                {
                    assemblyList[i] = AssemblyName.GetAssemblyName(References[i].ItemSpec);
                }
                // We should never get passed in references we can't load. In the VS build process, for example,
                // we're passed in @(ReferencePath), which only contains resolved references.
                catch (ArgumentNullException e)
                {
                    throw new ArgumentException(e.Message, References[i].ItemSpec);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException(e.Message, References[i].ItemSpec);
                }
                catch (FileNotFoundException e)
                {
                    throw new ArgumentException(e.Message, References[i].ItemSpec);
                }
                /*catch (SecurityException e)
                {
                    throw new ArgumentException(e.Message, References[i].ItemSpec);
                }
                */
                catch (BadImageFormatException e)
                {
                    throw new ArgumentException(e.Message, References[i].ItemSpec);
                }
                catch (FileLoadException e)
                {
                    throw new ArgumentException(e.Message, References[i].ItemSpec);
                }
            }

            return assemblyList;
        }

        /// <summary>
        /// Check for parameter errors.
        /// </summary>
        /// <returns>true if parameters are valid</returns>
        /// <owner>danmose</owner>
        private bool ValidateParameters()
        {
            // make sure that if the output resources were set, they exactly match the number of input sources
            if ((OutputResources != null) && (OutputResources.Length != Sources.Length))
            {
                Log.LogError("General.TwoVectorsMustHaveSameLength", Sources.Length, OutputResources.Length, "Sources", "OutputResources");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Make the decision about whether a separate AppDomain is needed.
        /// If this algorithm is unsure about whether a separate AppDomain is
        /// needed, it should always err on the side of returning 'true'. This
        /// is because a separate AppDomain, while slow to create, is always safe.
        /// </summary>
        /// <param name="sources">The list of .resx files.</param>
        /// <returns></returns>
        private bool NeedSeparateAppDomain()
        {
            /*
            if (NeverLockTypeAssemblies)
            {
                Log.LogMessage(MessageImportance.Low, "GenerateResource.SeparateAppDomainBecauseNeverLockTypeAssembliesTrue");
                return true;
            }

            foreach (ITaskItem source in sources)
            {
                string extension = Path.GetExtension(source.ItemSpec);
                if (String.Compare(extension, ".resx", true, CultureInfo.InvariantCulture) == 0)
                {
                    XmlTextReader reader = null;
                    try
                    {
                        reader = new XmlTextReader(source.ItemSpec);

                        while (reader.Read())
                        {
                            // Look for the <data> section
                            if (reader.NodeType == XmlNodeType.Element && String.Compare(reader.Name, "data", true, CultureInfo.InvariantCulture) == 0)
                            {
                                // Is there an attribute called type?
                                string type = reader.GetAttribute("type");

                                if (type != null)
                                {
                                    // The assembly referenced in the type will be locked.
                                    // We don't care if this is a system assembly.
                                    if (!type.StartsWith("System."))
                                    {
                                        // The type didn't start with "System." so return true.
                                        Log.LogMessage
                                        (
                                            MessageImportance.Low,
                                            "GenerateResource.SeparateAppDomainBecauseOfType",
                                            type,
                                            source.ItemSpec,
                                            ((IXmlLineInfo)reader).LineNumber
                                        );

                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException e)
                    {
                        // If there was any problem parsing the .resx then log a message and
                        // fall back to using a separate AppDomain.
                        Log.LogMessage
                                    (
                                        MessageImportance.Low,
                                        "GenerateResource.SeparateAppDomainBecauseOfException",
                                        source.ItemSpec,
                                        e.Message
                                    );
                        return true;
                    }
                    catch (Exception e)
                    {
                        ExceptionHandling.RethrowUnlessFileIO(e);

                        // If there was any problem parsing the .resx then log a message and
                        // fall back to using a separate AppDomain.
                        Log.LogMessage
                                    (
                                        MessageImportance.Low,
                                        "GenerateResource.SeparateAppDomainBecauseOfException",
                                        source.ItemSpec,
                                        e.Message
                                    );
                        return true;
                    }
                    catch
                    {
                        // For non-CLS exception,
                        Log.LogMessage
                        (
                            MessageImportance.Low,
                            "GenerateResource.SeparateAppDomainBecauseOfException",
                            source.ItemSpec,
                            "Non CLS-Compliant Exception"
                        );
                        return true;
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }

            */
            return false;
        }

        /// <summary>
        /// Make sure that OutputResources has 1 file name for each name in Sources.
        /// </summary>
        private bool CreateOutputResourcesNames()
        {
            if (OutputResources == null)
            {
                OutputResources = new ITaskItem[Sources.Length];

                int i = 0;
                try
                {
                    for (i = 0; i < Sources.Length; ++i)
                    {
                        OutputResources[i] = new TaskItem(Path.ChangeExtension(Sources[i].ItemSpec, ".tinyresources"));
                    }
                }
                catch (ArgumentException e)
                {
                    Log.LogError("GenerateResource.InvalidFilename", Sources[i].ItemSpec, e.Message);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Remove any output resources that we didn't successfully create (due to error) from the
        /// OutputResources list. Keeps the ordering of OutputResources the same.
        /// </summary>
        /// <remarks>
        /// Q: Why didn't we keep a "successfully created" list instead, like in the Copy task does, which
        /// would save us doing the removal algorithm below?
        /// A: Because we want the ordering of OutputResources to be the same as the ordering passed in.
        /// Some items (the up to date ones) would be added to the successful output list first, and the other items
        /// are added during processing, so the ordering would change. We could fix that up, but it's better to do
        /// the fix up only in the rarer error case. If there were no errors, the algorithm below skips.</remarks>
        /// <owner>danmose</owner>
        private void RemoveUnsuccessfullyCreatedResourcesFromOutputResources()
        {
            // Normally, there aren't any unsuccessful conversions.
            if (unsuccessfullyCreatedOutFiles == null ||
                unsuccessfullyCreatedOutFiles.Count == 0)
            {
                return;
            }

            Debug.Assert(OutputResources != null && OutputResources.Length != 0);

            // We only get here if there was at least one resource generation error.
            ITaskItem[] temp = new ITaskItem[OutputResources.Length - unsuccessfullyCreatedOutFiles.Count];
            int copied = 0;
            int removed = 0;
            foreach (ITaskItem item in OutputResources)
            {
                // Check whether this one is in the bad list.
                if (removed < unsuccessfullyCreatedOutFiles.Count &&
                    unsuccessfullyCreatedOutFiles.Contains(item.ItemSpec))
                {
                    removed++;
                }
                else
                {
                    // Copy it to the okay list.
                    temp[copied] = item;
                    copied++;
                }
            }
            OutputResources = temp;
        }

        /// <summary>
        /// Record the list of file that will be written to disk.
        /// </summary>
        private void RecordFilesWritten()
        {
            // Add any output resources that were successfully created,
            // or would have been if they weren't already up to date (important for Clean)
            foreach (ITaskItem item in this.OutputResources)
            {
                Debug.Assert(File.Exists(item.ItemSpec), item.ItemSpec + " doesn't exist but we're adding to FilesWritten");
                filesWritten.Add(item);
            }

            // Add any state file
            if (StateFile != null && StateFile.ItemSpec.Length > 0)
            {
                // It's possible the file wasn't actually written (eg the path was invalid)
                // We can't easily tell whether that happened here, and I think it's fine to add it anyway.
                filesWritten.Add(StateFile);
            }
        }

        /// <summary>
        /// Read the state file if able.
        /// </summary>
        private void ReadStateFile()
        {
            // First we look to see if we have a resgen linked files cache.  If so, then we can use that
            // cache to speed up processing.  If there's a problem reading the cache file (or it
            // just doesn't exist, then this method will return a brand new cache object.

            // This method eats IO Exceptions

            cache = ResGenDependencies.DeserializeCache((StateFile == null) ? null : StateFile.ItemSpec, UseSourcePath, Log);

            //RWOLFF -- throw here?
            //ErrorUtilities.VerifyThrow(cache != null, "We did not create a cache!");
        }

        /// <summary>
        /// Write the state file if there is one to be written.
        /// </summary>
        private void WriteStateFile()
        {
            if (cache.IsDirty)
            {
                // And now we serialize the cache to save our resgen linked file resolution for later use.
                cache.SerializeCache((StateFile == null) ? null : StateFile.ItemSpec, Log);
            }
        }

        /// <summary>
        /// Determines if the given output file is up to date with respect to the
        /// the given input file by comparing timestamps of the two files as well as
        /// (if the source is a .resx) the linked files inside the .resx file itself
        /// <param name="sourceFilePath"></param>
        /// <param name="outputFilePath"></param>
        /// <returns></returns>
        private bool ShouldRebuildResgenOutputFile(string sourceFilePath, string outputFilePath)
        {
/*#if !(MSBUILD_SOURCES)
            return true;
#else
 */
            bool sourceFileExists = File.Exists(sourceFilePath);
            bool destinationFileExists = File.Exists(outputFilePath);

            // PERF: Regardless of whether the outputFile exists, if the source file is a .resx
            // go ahead and retrieve it from the cache. This is because we want the cache
            // to be populated so that incremental builds can be fast.
            // Note that this is a trade-off: clean builds will be slightly slower. However,
            // for clean builds we're about to read in this very same .resx file so reading
            // it now will page it in. The second read should be cheap.
            ResGenDependencies.ResXFile resxFileInfo = null;
            if (String.Compare(Path.GetExtension(sourceFilePath), ".resx", true, CultureInfo.InvariantCulture) == 0)
            {
                try
                {
                    resxFileInfo = cache.GetResXFileInfo(sourceFilePath);
                }
                catch (ArgumentException)
                {
                    // Return true, so that resource processing will display the error
                    // No point logging a duplicate error here as well
                    return true;
                }
                catch (XmlException)
                {
                    // Return true, so that resource processing will display the error
                    // No point logging a duplicate error here as well
                    return true;
                }
                catch (Exception e)  // Catching Exception, but rethrowing unless it's a well-known exception.
                {
                    ExceptionHandling.RethrowUnlessFileIO(e);
                    // Return true, so that resource processing will display the error
                    // No point logging a duplicate error here as well
                    return true;
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////
            // If the output file does not exist, then we should rebuild it.
            //  Also, if the input file does not exist, we will also return saying that the
            //  the output file needs to be rebuilt, so that this pair of files will
            //  get added to the command-line which will let resgen output whatever error
            //  it normally outputs in the case when users call the tool with bad params
            bool    shouldRebuildOutputFile = (!destinationFileExists || !sourceFileExists);

            // if both files do exist, then we need to do some timestamp comparisons
            if (!shouldRebuildOutputFile)
            {
                Debug.Assert(destinationFileExists && sourceFileExists, "GenerateResource task should not check timestamps if neither the .resx nor the .resources files exist");

                // cache the output file timestamps
                DateTime outputFileTimeStamp = File.GetLastWriteTime(outputFilePath);

                // If source file is NOT a .resx, timestamp checking is simple
                if (resxFileInfo == null)
                {
                    // We have a non .resx file. Don't attempt to parse it.

                    // cache the source file timestamp
                    DateTime sourceFileTimeStamp = File.GetLastWriteTime(sourceFilePath);

                    // we need to rebuild this output file if the source file has a
                    //  more recent timestamp than the output file
                    shouldRebuildOutputFile = (sourceFileTimeStamp > outputFileTimeStamp);

                    return shouldRebuildOutputFile;
                }

                // Source file IS a .resx file so we need to do deep dependency analysis
                Debug.Assert(resxFileInfo != null, "Why didn't we get resx file information?");

                // cache the .resx file timestamps
                DateTime resxTimeStamp = resxFileInfo.LastModified;

                // we need to rebuild this .resources file if the .resx file has a
                //  more recent timestamp than the .resources file
                shouldRebuildOutputFile = (resxTimeStamp > outputFileTimeStamp);

                // Check the timestamp of each of the passed-in references against the .RESOURCES file.
                if (!shouldRebuildOutputFile && (this.References != null))
                {
                    foreach (ITaskItem reference in this.References)
                    {
                        // If the reference doesn't exist, then we want to rebuild this
                        // .resources file so the user sees an error from ResGen.exe
                        shouldRebuildOutputFile = !File.Exists(reference.ItemSpec);

                        // If the reference exists, then we need to compare the timestamp
                        // for the linked resource to see if it is more recent than the
                        // .resources file
                        if (!shouldRebuildOutputFile)
                        {
                            DateTime referenceTimeStamp = File.GetLastWriteTime(reference.ItemSpec);
                            shouldRebuildOutputFile = referenceTimeStamp > outputFileTimeStamp;
                        }

                        // If we found an instance where a reference is in a state
                        // that we should rebuild the .resources file, then we should
                        // bail from this loop & just return since the first file that
                        // forces a rebuild is enough
                        if (shouldRebuildOutputFile)
                        {
                            break;
                        }
                    }
                }

                // if the .resources is up to date with respect to the .resx file
                //  then we need to compare timestamps for each linked file inside
                //  the .resx file itself
                if (!shouldRebuildOutputFile && resxFileInfo.LinkedFiles != null)
                {
                    foreach (string linkedFilePath in resxFileInfo.LinkedFiles)
                    {
                        // If the linked file doesn't exist, then we want to rebuild this
                        // .resources file so the user sees an error from ResGen.exe
                        shouldRebuildOutputFile = !File.Exists(linkedFilePath);

                        // If the linked file exists, then we need to compare the timestamp
                        // for the linked resource to see if it is more recent than the
                        // .resources file
                        if (!shouldRebuildOutputFile)
                        {
                            DateTime linkedFileTimeStamp = File.GetLastWriteTime(linkedFilePath);
                            shouldRebuildOutputFile = linkedFileTimeStamp > outputFileTimeStamp;
                        }

                        // If we found an instance where a linked file is in a state
                        // that we should rebuild the .resources file, then we should
                        // bail from this loop & just return since the first file that
                        // forces a rebuild is enough
                        if (shouldRebuildOutputFile)
                        {
                            break;
                        }
                    }
                }
            }

            return shouldRebuildOutputFile;
//#endif
        }

        internal static string GetStronglyTypedFileFromTaskItem( ITaskItem inTaskItem, string stronglyTypedLanguage )
        {
            string name = inTaskItem.GetMetadata( "AutoGenerateFileName" );

            if(string.IsNullOrEmpty( name ))
            {
                CodeDomProvider provider = CodeDomProvider.CreateProvider( stronglyTypedLanguage );
                name = string.Format( "{0}{1}.designer.{2}", inTaskItem.GetMetadata("RelativeDir"),inTaskItem.GetMetadata("FileName"), provider.FileExtension );
                inTaskItem.SetMetadata( "AutoGenerateFileName", name );
            }

            return name;
        }
    }

    /// <summary>
    /// This class handles the processing of source resource files into compiled resource files.
    /// Its designed to be called from a separate AppDomain so that any files locked by ResXResourceReader
    /// can be released.
    /// </summary>
    public sealed class ProcessResourceFiles : MarshalByRefObject
    {

#region fields
        /// <summary>
        /// Resource list (used to preserve resource ordering, primarily for easier testing)
        /// </summary>
        private ArrayList resources = new ArrayList();

        /// <summary>
        /// Mirror resource list, used to check for duplicates
        /// </summary>
        private Hashtable resourcesHashTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Logger for any messages or errors
        /// </summary>
        private TaskLoggingHelper logger = null;

        /// <summary>
        /// Language for the strongly typed resources.
        /// </summary>
        //private string stronglyTypedLanguage;

        /// <summary>
        /// Filename for the strongly typed resources.
        /// Getter provided since the processor may choose a default.
        /// </summary>

        /// <summary>
        /// Namespace for the strongly typed resources.
        /// </summary>
        private string stronglyTypedNamespace;

        public string StronglyTypedNamespace
        {
            get { return this.stronglyTypedNamespace; }
            set { this.stronglyTypedNamespace = value; }
        }

        /// <summary>
        /// Class name for the strongly typed resources.
        /// Getter provided since the processor may choose a default.
        /// </summary>
        public string StronglyTypedClassName
        {
            get
            {
                return this.stronglyTypedClassName;
            }
            set
            {
                this.stronglyTypedClassName = value;
            }
         }
        private string stronglyTypedClassName;

        private bool useNestedClassForEnums = true;
        private bool useInternalClass = true;

        /// <summary>
        /// List of assemblies to use for type resolution within resx files
        /// </summary>
        private AssemblyName[] assemblyList;

        /// <summary>
        /// List of input files to process.
        /// </summary>
        private ITaskItem[] inFiles;

        /// <summary>
        /// List of output files to process.
        /// </summary>
        private ITaskItem[] outFiles;

        public bool GenerateNestedEnums
        {
            get { return this.useNestedClassForEnums; }
            set { this.useNestedClassForEnums = value; }
        }

        public bool GenerateInternalClass
        {
            get { return this.useInternalClass; }
            set {this.useInternalClass = value;}
        }

        public bool IsMscorlib
        {
            get {return isMscorlib; }
            set { this.isMscorlib = value; }
        }


        /// <summary>
        /// List of output files that we failed to create due to an error.
        /// See note in RemoveUnsuccessfullyCreatedResourcesFromOutputResources()
        /// </summary>
        internal ArrayList UnsuccessfullyCreatedOutFiles
        {
            get
            {
                if (null == unsuccessfullyCreatedOutFiles)
                {
                    unsuccessfullyCreatedOutFiles = new ArrayList();
                }
                return unsuccessfullyCreatedOutFiles;
            }
        }
        private ArrayList unsuccessfullyCreatedOutFiles;

        /// <summary>
        /// Whether we successfully created the STR class
        /// </summary>
        internal bool StronglyTypedResourceSuccessfullyCreated
        {
            get
            {
                return stronglyTypedResourceSuccessfullyCreated;
            }
        }
        private bool stronglyTypedResourceSuccessfullyCreated = false;

        private bool isMscorlib = false;

        /// <summary>
        /// Indicates whether the resource reader should use the source file's
        /// directory to resolve relative file paths.
        /// </summary>
        private bool useSourcePath = false;

        private ITaskItem m_inTaskItem;
        private ITaskItem m_outTaskItem;

#endregion

        /// <summary>
        /// Process all files.
        /// </summary>
        internal void Run( TaskLoggingHelper log, AssemblyName[] assemblies, ITaskItem[] inputs, ITaskItem[] outputs, bool sourcePath)
        {
            logger = log;
            assemblyList = assemblies;
            inFiles = inputs;
            outFiles = outputs;
            useSourcePath = sourcePath;

            for (int i = 0; i < inFiles.Length; ++i)
            {
                if (!ProcessFile(inFiles[i], outFiles[i]))
                {
                    UnsuccessfullyCreatedOutFiles.Add(outFiles[i]);
                }
            }
        }

        private void Init()
        {
            this.resources.Clear();
            this.resourcesHashTable.Clear();
        }

        private void InitFileProcessing( ITaskItem inTaskItem, ITaskItem outTaskItem )
        {
            Init();

            m_inTaskItem = inTaskItem;
            m_outTaskItem = outTaskItem;
        }

        #region Code from ResGen.EXE
        /// <summary>
        /// Read all resources from a file and write to a new file in the chosen format
        /// </summary>
        /// <remarks>Uses the input and output file extensions to determine their format</remarks>
        /// <param name="inFile">Input resources file</param>
        /// <param name="outFile">Output resources file</param>
        /// <returns>True if conversion was successful, otherwise false</returns>
        private bool ProcessFile( ITaskItem inTaskItem, ITaskItem outTaskItem )
        {
            InitFileProcessing( inTaskItem, outTaskItem );

            string inFile = inTaskItem.ItemSpec;
            string outFile = outTaskItem.ItemSpec;

            if (GetFormat(inFile) == Format.Error)
            {
                logger.LogError("GenerateResource.UnknownFileExtension", Path.GetExtension(inFile), inFile);
                return false;
            }
            if (GetFormat(outFile) == Format.Error)
            {
                logger.LogError("GenerateResource.UnknownFileExtension", Path.GetExtension(outFile), outFile);
                return false;
            }

//            logger.LogMessage("GenerateResource.ProcessingFile", inFile, outFile);

            try
            {
                ReadResources(inFile, useSourcePath);
            }
            catch (ArgumentException ae)
            {
                if (ae.InnerException is XmlException)
                {
                    XmlException xe = (XmlException)ae.InnerException;
                    logger.LogError(null, inFile, xe.LineNumber, xe.LinePosition, 0, 0, "General.InvalidResxFile", xe.Message);
                }
                else
                {
                    logger.LogError(null, inFile, 0, 0, 0, 0, "General.InvalidResxFile", ae.Message);
                }
                return false;
            }
            catch (XmlException xe)
            {
                logger.LogError(null, inFile, xe.LineNumber, xe.LinePosition, 0, 0, "General.InvalidResxFile", xe.Message);
                return false;
            }
#if MSBUILD_SOURCES
            catch (Exception e)
            {
                ExceptionHandling.RethrowUnlessFileIO(e);
                logger.LogError(null, inFile, 0, 0, 0, 0, "General.InvalidResxFile", e.Message);

                // We need to give meaningful error messages to the user.
                // Note that ResXResourceReader wraps any exception it gets
                // in an ArgumentException with the message "Invalid ResX input."
                // If you don't look at the InnerException, you have to attach
                // a debugger to find the problem.
                if (e.InnerException != null)
                {
                    Exception inner = e.InnerException;
                    StringBuilder sb = new StringBuilder(200);
                    sb.Append(e.Message);
                    while (inner != null)
                    {
                        sb.Append(" ---> ");
                        sb.Append(inner.GetType().Name);
                        sb.Append(": ");
                        sb.Append(inner.Message);
                        inner = inner.InnerException;
                    }
                    logger.LogError(null, inFile, 0, 0, 0, 0, "General.InvalidResxFile", sb.ToString());
                }
                return false;
            }
#endif
            catch
            {
                throw;
            }

            try
            {
                WriteResources(outFile);
            }
            catch (IOException io)
            {
                logger.LogError("GenerateResource.CannotWriteOutput", outFile, io.Message);

                if (File.Exists(outFile))
                {
                    logger.LogError("GenerateResource.CorruptOutput", outFile);
                    try
                    {
                        File.Delete(outFile);
                    }
                    catch (Exception e)
                    {
                        logger.LogWarning("GenerateResource.DeleteCorruptOutputFailed", outFile, e.Message);
                    }
                }
                return false;
            }
#if MSBUILD_SOURCES
            catch (Exception e)
            {
                ExceptionHandling.RethrowUnlessFileIO(e);
                logger.LogError("GenerateResource.CannotWriteOutput", outFile, e.Message);
                return false;
            }
#endif
            catch
            {
                throw;
            }

            return true;
        }

        /// <summary>
        /// Figure out the format of an input resources file from the extension
        /// </summary>
        /// <param name="filename">Input resources file</param>
        /// <returns>Resources format</returns>
        private Format GetFormat(string filename)
        {
            string extension = Path.GetExtension(filename);
            if (String.Compare(extension, ".txt", true, CultureInfo.InvariantCulture) == 0 ||
                String.Compare(extension, ".restext", true, CultureInfo.InvariantCulture) == 0)
            {
                return Format.Text;
            }
            else if (String.Compare(extension, ".resx", true, CultureInfo.InvariantCulture) == 0)
            {
                return Format.XML;
            }
            else if (String.Compare(extension, ".resources", true, CultureInfo.InvariantCulture) == 0)
            {
                return Format.Binary;
            }
            else if (String.Compare(extension, ".tinyresources", true, CultureInfo.InvariantCulture) == 0)
            {
                return Format.TinyResources;
            }
            else
            {
                return Format.Error;
            }
        }

        /// <summary>
        /// Text files are just name/value pairs.  ResText is the same format
        /// with a unique extension to work around some ambiguities with MSBuild
        /// ResX is our existing XML format from V1.
        /// </summary>
        private enum Format
        {
            Text, // .txt or .restext
            XML, // .resx
            Binary, // .resources
            TinyResources, //.tinyresources
            Error, // anything else
        }

        /// <summary>
        /// Reads the resources out of the specified file and populates the
        /// resources hashtable.
        /// </summary>
        /// <param name="filename">Filename to load</param>
        /// <param name="shouldUseSourcePath">Whether to resolve paths in the
        /// resources file relative to the resources file location</param>
        public void ReadResources(String filename, bool shouldUseSourcePath)
        {
            // Reset state
//            resources.Clear();
//            resourcesHashTable.Clear();

            Format format = GetFormat(filename);
            switch (format)
            {
                case Format.Text:
                    ReadTextResources(filename);
                    break;

                case Format.XML:
                    ResXResourceReader resXReader = null;
                    if (assemblyList != null)
                    {
                        resXReader = new ResXResourceReader(filename, assemblyList);
                    }
                    else
                    {
                        resXReader = new ResXResourceReader(filename);
                    }
                    if (shouldUseSourcePath)
                    {
                        String fullPath = Path.GetFullPath(filename);
                        resXReader.BasePath = Path.GetDirectoryName(fullPath);
                    }
                    // ReadResources closes the reader for us
                    ReadResources(resXReader, filename);
                    break;

                case Format.Binary:
                    ReadResources(new ResourceReader(filename), filename); // closes reader for us
                    break;
                case Format.TinyResources:
                    Debug.Fail("Unknown format " + format.ToString());
                    break;

                default:
                    // We should never get here, we've already checked the format
                    Debug.Fail("Unknown format " + format.ToString());
                    return;
            }
            //logger.LogMessage(BuildEventImportance.Low/*MessageImportance.Low*/, "GenerateResource.ReadResourceMessage", resources.Count, filename);
        }

        /// <summary>
        /// Write resources from the resources ArrayList to the specified output file
        /// </summary>
        /// <param name="filename">Output resources file</param>
        public void WriteResources(String filename)
        {
            Format format = GetFormat(filename);
            switch (format)
            {
                case Format.Text:
                    WriteTextResources(filename);
                    break;

                case Format.XML:
                    WriteResources(new ResXResourceWriter(filename)); // closes writer for us
                    break;

                case Format.Binary:
                    WriteResources(new ResourceWriter(filename)); // closes writer for us
                    break;

                case Format.TinyResources:
                    WriteResources( new TinyResourceWriter( filename ) ); // closes writer for us
                    break;
                default:
                    // We should never get here, we've already checked the format
                    Debug.Fail("Unknown format " + format.ToString());
                    break;
            }
        }

        private void CreateStronglyTypedResources( CodeDomProvider provider, TextWriter writer, string resourceName, out string[] errors )
        {
            CodeCompileUnit ccu = CreateStronglyTypedResourceFile( resourceName, resources,
                stronglyTypedClassName, stronglyTypedNamespace, provider, out errors );
            CodeGeneratorOptions codeGenOptions = new CodeGeneratorOptions();
            codeGenOptions.BlankLinesBetweenMembers = false;
            codeGenOptions.BracingStyle = "C";

            provider.GenerateCodeFromCompileUnit( ccu, writer, codeGenOptions );
            writer.Flush();
        }

        public void CreateStronglyTypedResources( string inputFileName, CodeDomProvider provider, TextWriter writer, string resourceName )
        {
            Init();

            ReadResources( inputFileName, true );

            string[] errors = null;

            CreateStronglyTypedResources( provider, writer, resourceName, out errors );

            if(errors != null && errors.Length > 0)
            {
                throw new ApplicationException( errors[0] );
            }
        }

        private CodeNamespace CreateNamespace( CodeCompileUnit ccu, string ns, Hashtable tableNamespaces )
        {
            CodeNamespace codeNamespace = (CodeNamespace)tableNamespaces[ns];

            if(codeNamespace == null)
            {
                codeNamespace = new CodeNamespace( ns );
                ccu.Namespaces.Add( codeNamespace );
                tableNamespaces[ns] = codeNamespace;
            }

            return codeNamespace;
        }

        private CodeTypeDeclaration CreateTypeDeclaration( CodeNamespace codeNamespace, string type, Hashtable tableTypes)
        {
            CodeTypeDeclaration codeTypeDeclaration = (CodeTypeDeclaration)tableTypes[type];

            if(codeTypeDeclaration == null)
            {
                int iPlus = type.LastIndexOf( '+' );

                if(iPlus < 0)
                {
                    codeTypeDeclaration = new CodeTypeDeclaration( type );
                    codeTypeDeclaration.IsPartial = true;

                    codeNamespace.Types.Add( codeTypeDeclaration );
                }
                else
                {
                    string typeBase = type.Substring( 0, iPlus );
                    string typeNested = type.Substring( iPlus + 1 );

                    CodeTypeDeclaration codeTypeDeclarationBase = CreateTypeDeclaration( codeNamespace, typeBase, tableTypes );
                    codeTypeDeclaration = new CodeTypeDeclaration( typeNested );

                    codeTypeDeclarationBase.Members.Add(codeTypeDeclaration);
                }

                MakeInternalIfNecessary( codeTypeDeclaration );
                tableTypes[type] = codeTypeDeclaration;
            }

            return codeTypeDeclaration;
        }

        private void CreateHelperMethod(CodeTypeDeclaration codeTypeDeclaration, Entry.ResourceTypeDescription typeDesciption, string parameterType)
        {

            /*
                    public static <typeDescription.runtimeType> <typeDescription.helperName> (<parameterType> id)
                    {
                        return (<typeDescription.runtimeType>)<getObjectClass>.GetObject( <codeTypeDeclaration>.ResourceManager, id );
                    }

             for example

                    public static Font GetFont( FontTag id )
                    {
                        return (Font)Microsoft.SPOT.ResourcesUtility.GetObject( MyResources.ResourceManager, FontTag id );
                    }

            */

            CodeMemberMethod method = new CodeMemberMethod();
            CodeParameterDeclarationExpression parameterIdDeclaration = new CodeParameterDeclarationExpression( parameterType, "id" );
            CodeVariableReferenceExpression parameterIdReference = new CodeVariableReferenceExpression( parameterIdDeclaration.Name );
            codeTypeDeclaration.Members.Add( method );

            method.Name = typeDesciption.helperName;
            method.Parameters.Add( parameterIdDeclaration );
            method.ReturnType = new CodeTypeReference(typeDesciption.runtimeType);

            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            MakeInternalIfNecessary( method );

            string getObjectClass = this.isMscorlib ? "System.Resources.ResourceManager" : "Microsoft.SPOT.ResourceUtility";

            CodeVariableReferenceExpression expresionId = new CodeVariableReferenceExpression( "id" );
            CodeTypeReferenceExpression spotResourcesReference = new CodeTypeReferenceExpression( getObjectClass );
            CodePropertyReferenceExpression resourceManagerReference = new CodePropertyReferenceExpression( null, "ResourceManager" );
            CodeExpression expressionGetObject = new CodeMethodInvokeExpression( spotResourcesReference, "GetObject", resourceManagerReference, expresionId );
            CodeExpression expressionValue = new CodeCastExpression( typeDesciption.runtimeType, expressionGetObject );
            CodeMethodReturnStatement statementReturn = new CodeMethodReturnStatement( expressionValue );
            method.Statements.Add( statementReturn );
        }

        private void MakeInternalIfNecessary( CodeTypeDeclaration codeTypeDeclaration )
        {
            if(this.GenerateInternalClass)
            {
                codeTypeDeclaration.TypeAttributes &= ~TypeAttributes.VisibilityMask;
                codeTypeDeclaration.TypeAttributes |= TypeAttributes.NestedAssembly;
            }
        }

        private void MakeInternalIfNecessary( CodeTypeMember codeTypeMember )
        {
            if(this.GenerateInternalClass)
            {
                codeTypeMember.Attributes &= ~MemberAttributes.AccessMask;
                codeTypeMember.Attributes |= MemberAttributes.Assembly;
            }
        }


        private CodeCompileUnit CreateStronglyTypedResourceFile( string resourceName, ArrayList resources, string className, string ns, CodeDomProvider provider, out string[] errors )
        {
            //create list of classes needed to be emitted.
            CodeCompileUnit ccu = new CodeCompileUnit();

            Hashtable tableNamespaces = new Hashtable();
            Hashtable tableTypes = new Hashtable();
            Hashtable tableHelperFunctionsNeeded = new Hashtable();
            ArrayList[] resourceTypesUsed = new ArrayList[TinyResourceFile.ResourceHeader.RESOURCE_Max+1];

            CodeNamespace codeNamespace;
            CodeTypeDeclaration codeTypeDeclaration;

            //break down resources by enum
            for(int iEntry = 0; iEntry < resources.Count; iEntry++)
            {
                Entry entry = (Entry)resources[iEntry];

                if(resourceTypesUsed[entry.ResourceType] == null)
                {
                    resourceTypesUsed[entry.ResourceType] = new ArrayList();
                }

                if(!resourceTypesUsed[entry.ResourceType].Contains( entry.ClassName ))
                {
                    resourceTypesUsed[entry.ResourceType].Add( entry.ClassName );
                }

                codeNamespace = CreateNamespace( ccu, entry.Namespace, tableNamespaces );
                codeTypeDeclaration = CreateTypeDeclaration( codeNamespace, entry.ClassName, tableTypes );

                if(!codeTypeDeclaration.IsEnum)
                {
                    //only initialize once
                    codeTypeDeclaration.IsEnum = true;
                    codeTypeDeclaration.CustomAttributes.Add( new CodeAttributeDeclaration( "System.SerializableAttribute" ) );
                    codeTypeDeclaration.BaseTypes.Add( new CodeTypeReference( typeof( short ) ) );
                }

                CodeMemberField codeMemberField = new CodeMemberField( entry.ClassName, entry.Field );
                codeMemberField.Attributes = MemberAttributes.Const | MemberAttributes.Static;

                CodePrimitiveExpression codeExpression = new CodePrimitiveExpression( entry.Id );
                codeMemberField.InitExpression = codeExpression;
                //set constant value??!!
                int iField = codeTypeDeclaration.Members.Add( codeMemberField );

            }

            //emit helper functions

            codeNamespace = CreateNamespace( ccu, ns, tableNamespaces );
            codeTypeDeclaration = CreateTypeDeclaration( codeNamespace, className, tableTypes );
            MakeInternalIfNecessary( codeTypeDeclaration );

            CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression( codeTypeDeclaration.Name );

            //private static System.Resources.ResourceManager manager;
            CodeMemberField fieldManager = new CodeMemberField( "System.Resources.ResourceManager", "manager" );
            CodeFieldReferenceExpression fieldManagerExpression = new CodeFieldReferenceExpression( codeTypeReferenceExpression, fieldManager.Name );

            fieldManager.Attributes = MemberAttributes.Static | MemberAttributes.Private;
            codeTypeDeclaration.Members.Add( fieldManager );

            /*
                    [private|internal] static System.Resources.ResourceManager ResourceManager
                    {
                    get
                    {
                        if(manager == null)
                        {
                            manager = new System.Resources.ResourceManager( "Resources", typeof( Resources ).Assembly );
                        }

                        return manager;
                    }
                }
            */

            CodeBinaryOperatorExpression getResourceManagerExpressionIfNull = new CodeBinaryOperatorExpression( fieldManagerExpression, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression( null ) );
            CodeObjectCreateExpression getResourceManagerExpressionNewManager = new CodeObjectCreateExpression(
                "System.Resources.ResourceManager",
                new CodePrimitiveExpression( resourceName ),
                new CodePropertyReferenceExpression( new CodeTypeOfExpression( codeTypeReferenceExpression.Type ), "Assembly" )
                );
            CodeAssignStatement getResourceManagerExpressionInitializeManager = new CodeAssignStatement( fieldManagerExpression, getResourceManagerExpressionNewManager );

            CodeStatementCollection getResourceManagerExpression = new CodeStatementCollection();

            getResourceManagerExpression.AddRange( new CodeStatement[] {
                new CodeConditionStatement( getResourceManagerExpressionIfNull, getResourceManagerExpressionInitializeManager ),
                new CodeMethodReturnStatement( fieldManagerExpression )
                }
            );

            CodeMemberProperty propertyResourceManager = new CodeMemberProperty();
            propertyResourceManager.Name = "ResourceManager";
            propertyResourceManager.Type = new CodeTypeReference( "System.Resources.ResourceManager" );
            propertyResourceManager.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            MakeInternalIfNecessary( propertyResourceManager );
            propertyResourceManager.GetStatements.AddRange( getResourceManagerExpression );
            codeTypeDeclaration.Members.Add( propertyResourceManager );

            for(int i = 0; i < resourceTypesUsed.Length; i++)
            {
                ArrayList list = resourceTypesUsed[i];

                if(list != null)
                {
                    Entry.ResourceTypeDescription typeDescription = Entry.ResourceTypeDescriptionFromResourceType( (byte)i );

                    for(int iClassName = 0; iClassName < list.Count; iClassName++)
                    {
                        CreateHelperMethod( codeTypeDeclaration, typeDescription, (string)list[iClassName] );
                    }
                }
            }

            errors = new string[0];
            return ccu;
        }
        /// <summary>
        /// If no strongly typed resource class filename was specified, we come up with a default based on the
        /// input file name and the default language extension. Broken out here so it can be called from GenerateResource class.
        /// </summary>
        /// <param name="provider">A CodeDomProvider for the language</param>
        /// <param name="outputResourcesFile">Name of the output resources file</param>
        /// <returns>Filename for strongly typed resource class</returns>
        public static string GenerateDefaultStronglyTypedFilename(CodeDomProvider provider, string outputResourcesFile)
        {
            return Path.ChangeExtension(outputResourcesFile, provider.FileExtension);
        }

        /// <summary>
        /// Read resources from an XML or binary format file
        /// </summary>
        /// <param name="reader">Appropriate IResourceReader</param>
        /// <param name="fileName">Filename, for error messages</param>
        private void ReadResources(IResourceReader reader, String fileName)
        {
            using (reader)
            {
                IDictionaryEnumerator resEnum = reader.GetEnumerator();
                while (resEnum.MoveNext())
                {
                    string name = (string)resEnum.Key;
                    // Replace dot in the name with underscore. 
                    // 1. First reason  - this is what desktop resource generator does.
                    // 2. Second reason - Extra dots causes resource generator to create name space and enumerations.
                    //    This complicates the syntax and finally create invalid code if 2 or more dots are present.
                    //    So we just make longer name.
                    name = name.Replace('.', '_');
                    object value = resEnum.Value;
                    AddResource(name, value, fileName);
                }
            }

            EnsureResourcesIds( this.resources );
        }

        private static short GenerateIdFromResourceName( string s )
        {
            //adapted from BCL implementation

            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            char[] chars = s.ToCharArray();

            int len = s.Length;

            for(int i = 0; i < len; i++)
            {
                char c = s[i];
                if(i % 2 == 0)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ c;
                }
                else
                {
                    hash2 = ((hash2 << 5) + hash2) ^ c;
                }
            }

            int hash = hash1 + (hash2 * 1566083941);

            short ret = (short)((short)(hash >> 16) ^ (short)hash);

            return ret;
        }

        internal static void EnsureResourcesIds(ArrayList resources)
        {
            int iResource;
            Hashtable table = new Hashtable();

            if(resources.Count > UInt16.MaxValue)
            {
                throw new ApplicationException( "Too many resources.  Maximum number of resources per ResourceSet is 65535" );
            }

            for(iResource = 0; iResource < resources.Count; iResource++)
            {
                Entry entry = (Entry)resources[iResource];

                short id = entry.Id;

                if(table.ContainsKey( id ))
                {
                    //rwolff -- check regarding boxed objects....

                    //duplicate id detected.
                    Entry entryDup = (Entry)table[id];

                    throw new ApplicationException( string.Format("Duplicate id detected.  Resources '{0}' and '{1}' are generating the same id=0x{2}", entry.Name, entryDup.Name, id ));
                }

                table[id] = entry;
            }

            resources.Sort();
        }

        /// <summary>
        /// Read resources from a text format file
        /// </summary>
        /// <param name="fileName">Input resources filename</param>
        private void ReadTextResources(String fileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write resources to an XML or binary format resources file.
        /// </summary>
        /// <remarks>Closes writer automatically</remarks>
        /// <param name="writer">Appropriate IResourceWriter</param>
        private void WriteResources(IResourceWriter writer)
        {
            try
            {
                foreach (Entry entry in resources)
                {
                    string key = entry.RawName;
                    object value = entry.Value;

                    if(writer is TinyResourceWriter)
                    {
                        ((TinyResourceWriter)writer).AddResource( entry );
                    }
                    else
                    {
                        writer.AddResource(key, value);
                    }
                }

                writer.Generate();
            }
            finally
            {
                writer.Close();
            }
        }

        /// <summary>
        /// Write resources to a text format resources file
        /// </summary>
        /// <param name="fileName">Output resources file</param>
        private void WriteTextResources(String fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                foreach (Entry entry in resources)
                {
                    String key = entry.Name;
                    Object v = entry.Value;
                    String value = v as String;
                    if (value == null)
                    {
                        logger.LogError(null, fileName, 0, 0, 0, 0, "GenerateResource.OnlyStringsSupported", key, v.GetType().FullName);
                    }
                    else
                    {
                        // Escape any special characters in the String.
                        value = value.Replace("\\", "\\\\");
                        value = value.Replace("\n", "\\n");
                        value = value.Replace("\r", "\\r");
                        value = value.Replace("\t", "\\t");

                        writer.WriteLine("{0}={1}", key, value);
                    }
                }
            }
        }

        /// <summary>
        /// Add a resource from a text file to the internal data structures
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="value">Resource value</param>
        /// <param name="inputFileName">Input file for messages</param>
        /// <param name="lineNumber">Line number for messages</param>
        /// <param name="linePosition">Column number for messages</param>
        private void AddResource(string name, object value, String inputFileName, int lineNumber, int linePosition)
        {
            Entry entry = Entry.CreateEntry(name, value, this.stronglyTypedNamespace, this.useNestedClassForEnums ? this.stronglyTypedClassName : string.Empty);

            Debug.Assert( entry.ClassName.Length > 0 );

            resources.Add(entry);
        }

        private void AddResource( string name, object value, String inputFileName )
        {
            AddResource( name, value, inputFileName, 0, 0 );
        }

        /// <summary>
        /// Name value resource pair to go in resources list
        /// </summary>
        internal abstract class Entry : IComparable
        {
            public class ResourceTypeDescription
            {
                public byte resourceType;
                public string helperName;
                public string runtimeType;
                public string defaultEnum;

                public ResourceTypeDescription( byte resourceType, string helperName, string runtimeType, string defaultEnum )
                {
                    this.resourceType = resourceType;
                    this.helperName = helperName;
                    this.runtimeType = runtimeType;
                    this.defaultEnum = defaultEnum;
                }
            }

            private static ResourceTypeDescription[] typeDescriptions = new ResourceTypeDescription[]
                {
                    null, //RESOURCE_Invalid
                    new ResourceTypeDescription(TinyResourceFile.ResourceHeader.RESOURCE_Bitmap, "GetBitmap", "Microsoft.SPOT.Bitmap", "BitmapResources"),
                    new ResourceTypeDescription(TinyResourceFile.ResourceHeader.RESOURCE_Font, "GetFont", "Microsoft.SPOT.Font", "FontResources"),
                    new ResourceTypeDescription(TinyResourceFile.ResourceHeader.RESOURCE_String, "GetString", "System.String", "StringResources"),
                    new ResourceTypeDescription(TinyResourceFile.ResourceHeader.RESOURCE_Binary, "GetBytes", "System.Byte[]", "BinaryResources"),
                    };

            public static ResourceTypeDescription ResourceTypeDescriptionFromResourceType( byte type )
            {
                if(type < TinyResourceFile.ResourceHeader.RESOURCE_Bitmap || type > TinyResourceFile.ResourceHeader.RESOURCE_Binary)
                {
                    throw new ArgumentException();
                }

                return typeDescriptions[type];
            }

            public static Entry CreateEntry( string name, object value, string defaultNamespace, string defaultDeclaringClass )
            {
                string stringValue = value as string;
                System.Drawing.Bitmap bitmapValue = value as System.Drawing.Bitmap;
                byte[] rawValue = value as byte[];
                Entry entry = null;

                if(stringValue != null)
                {
                    entry = new StringEntry( name, stringValue );
                }
                else if(bitmapValue != null)
                {
                    entry = new BitmapEntry( name, bitmapValue );
                }
                if(rawValue != null)
                {
                    entry = TinyResourcesEntry.TryCreateTinyResourcesEntry( name, rawValue );

                    if(entry == null)
                    {
                        entry = new BinaryEntry( name, rawValue );
                    }
                }

                if(entry == null)
                {
                    throw new Exception();
                }

                if(entry.Namespace.Length == 0)
                {
                    entry.Namespace = defaultNamespace;
                }

                if(entry.ClassName.Length == 0)
                {
                    ResourceTypeDescription typeDescription = ResourceTypeDescriptionFromResourceType( entry.ResourceType );
                    entry.ClassName = typeDescription.defaultEnum;

                    if(!string.IsNullOrEmpty( defaultDeclaringClass ))
                    {
                        entry.ClassName = string.Format( "{0}+{1}", defaultDeclaringClass, entry.ClassName );
                    }
                }

                return entry;
            }

            private bool ParseId( string val, out short id )
            {
                val = val.Trim();
                bool fSuccess = false;

                if(val.StartsWith( "0x", true, CultureInfo.InvariantCulture ))
                {
                    ushort us;

                    fSuccess = ushort.TryParse( val.Substring( 2 ), NumberStyles.AllowHexSpecifier, null, out us );

                    id = (short)us;
                }
                else
                {
                    fSuccess = short.TryParse( val, out id );
                }

                return fSuccess;
            }

            public Entry(string name, object value)
            {
                this.value = value;
                this.ns = string.Empty;
                this.className = string.Empty;
                this.field = string.Empty;
                this.rawName = name;

                //parse name

                string[] tokens = name.Split( ';' );
                string idValue;
                short idT;

                switch(tokens.Length)
                {
                    case 1:
                        name = tokens[0];
                        idValue = string.Empty;
                        break;
                    case 2:
                        name = tokens[0];
                        idValue = tokens[1];
                        break;
                    default:
                        throw new ArgumentException();
                }

                idValue = idValue.Trim();

                if(idValue.Length > 0)
                {
                    if(!ParseId( idValue, out idT ))
                    {
                        throw new ApplicationException( string.Format( "Cannot parse id '{0}' from resource '{1}'", idValue, name ) );
                    }

                    this.Id = idT;
                }
                else
                {
                    this.Id = GenerateIdFromResourceName( name );
                }

                int iDotLast = name.LastIndexOf( '.' );

                this.field = name;

                if(iDotLast >= 0)
                {
                    this.field = name.Substring( iDotLast + 1 );

                    name = name.Substring( 0, iDotLast );

                    iDotLast = name.LastIndexOf( '.' );
                    //iDotLast = name.LastIndexOfAny( new char[] { '.', '+' } );

                    this.className = name.Trim();

                    if(iDotLast >= 0)
                    {
                        this.className = name.Substring( iDotLast + 1 ).Trim();
                        this.ns = name.Substring( 0, iDotLast ).Trim();
                    }
                }
            }

            private string ns;
            private string className;
            private string field;
            private short id;
            private object value;
            private string rawName;

            public string Name
            {
                get { return string.Format( "{0}.{1}.{2};0x{3}", ns, className, field, id.ToString( "X4" ) ); }
            }

            public string RawName
            {
                get { return rawName; }
            }

            public object Value
            {
                get { return value; }
            }

            public short Id
            {
                get { return id; }
                set { id = value; }
            }

            public string Namespace
            {
                get { return ns; }
                set { ns = value; }
            }

            public string ClassName
            {
                get { return className; }
                set { className = value; }
            }

            public string Field
            {
                get { return field; }
                set { field = value; }
            }

            #region IComparable Members

            int IComparable.CompareTo( object obj )
            {
                Entry entry = obj as Entry;

                if(entry == null)
                {
                    return -1;
                }

                return this.id.CompareTo( entry.id );
            }

            #endregion

            public virtual byte ResourceType
            {
                get
                {
                    if(value.GetType() == typeof( string )) return TinyResourceFile.ResourceHeader.RESOURCE_String;

                    return TinyResourceFile.ResourceHeader.RESOURCE_Invalid;
                }
            }

            public virtual byte[] GenerateResourceData()
            {
                return null;
            }
        }

        private class StringEntry : Entry
        {
            public StringEntry(string name, string value) : base(name, value)
            {
            }

            private string StringValue
            {
                get { return this.Value as string; }
            }

            public override byte ResourceType
            {
                get
                {
                    return TinyResourceFile.ResourceHeader.RESOURCE_String;
                }
            }

            public override byte[] GenerateResourceData()
            {
                string val = this.StringValue + '\0';

                byte[] data = Encoding.UTF8.GetBytes( val );

                return data;
            }
        }

        private class BitmapEntry : Entry
        {
            public BitmapEntry(string name, System.Drawing.Bitmap value) : base(name, value)
            {
            }

            private System.Drawing.Bitmap BitmapValue
            {
                get { return this.Value as System.Drawing.Bitmap; }
            }

            public override byte ResourceType
            {
                get
                {
                    return TinyResourceFile.ResourceHeader.RESOURCE_Bitmap;
                }
            }


            private void Adjust1bppOrientation( byte[] buf )
            {
                //CLR_GFX_Bitmap::AdjustBitOrientation
                //The TinyCLR treats 1bpp bitmaps reversed from Windows
                //And most likely every other 1bpp format as well
                byte[] reverseTable = new byte[]
            {
                0x00, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0,
                0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0,
                0x08, 0x88, 0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8,
                0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8,
                0x04, 0x84, 0x44, 0xC4, 0x24, 0xA4, 0x64, 0xE4,
                0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4,
                0x0C, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC, 0x6C, 0xEC,
                0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC,
                0x02, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2,
                0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2,
                0x0A, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA,
                0x1A, 0x9A, 0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA,
                0x06, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6,
                0x16, 0x96, 0x56, 0xD6, 0x36, 0xB6, 0x76, 0xF6,
                0x0E, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE,
                0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE, 0x7E, 0xFE,
                0x01, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1,
                0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
                0x09, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9,
                0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9,
                0x05, 0x85, 0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5,
                0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5,
                0x0D, 0x8D, 0x4D, 0xCD, 0x2D, 0xAD, 0x6D, 0xED,
                0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD,
                0x03, 0x83, 0x43, 0xC3, 0x23, 0xA3, 0x63, 0xE3,
                0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3,
                0x0B, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB,
                0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB,
                0x07, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7,
                0x17, 0x97, 0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7,
                0x0F, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF,
                0x1F, 0x9F, 0x5F, 0xDF, 0x3F, 0xBF, 0x7F,0xFF,
                };

                for(int i = buf.Length - 1; i >= 0; i--)
                {
                    buf[i] = reverseTable[buf[i]];
                }
            }

            private void Compress1bpp( TinyResourceFile.CLR_GFX_BitmapDescription bitmapDescription, ref byte[] buf )
            {
                MemoryStream ms = new MemoryStream( buf.Length );

                //adapted from CLR_GFX_Bitmap::Compress
                //CLR_RT_Buffer   buffer;
                int count = 0;
                bool fSetSav = false;
                bool fSet = false;
                bool fFirst = true;
                bool fRun = true;
                byte data = 0;
                bool fEmit = false;
                int widthInWords = (int)((bitmapDescription.m_width + 31) / 32);
                int iByte;
                byte iPixelMask;

                iByte = 0;
                for(int y = 0; y < bitmapDescription.m_height; y++)
                {
                    iPixelMask = 0x1;
                    iByte = y * (widthInWords * 4);

                    for(int x = 0; x < bitmapDescription.m_width; x++)
                    {
                        fSetSav = fSet;

                        fSet = (buf[iByte] & iPixelMask) != 0;
                        if(fFirst)
                        {
                            fFirst = false;
                        }
                        else
                        {
                            if(fRun)
                            {
                                fRun = (fSetSav == fSet);

                                if((count == 0x3f + TinyResourceFile.CLR_GFX_BitmapDescription.c_CompressedRunOffset) ||
                                (!fRun && count >= TinyResourceFile.CLR_GFX_BitmapDescription.c_CompressedRunOffset))
                                {
                                    data = TinyResourceFile.CLR_GFX_BitmapDescription.c_CompressedRun;
                                    data |= (fSetSav ? TinyResourceFile.CLR_GFX_BitmapDescription.c_CompressedRunSet : (byte)0x0);
                                    data |= (byte)(count - TinyResourceFile.CLR_GFX_BitmapDescription.c_CompressedRunOffset);
                                    fEmit = true;
                                }
                            }

                            if(!fRun && count == TinyResourceFile.CLR_GFX_BitmapDescription.c_UncompressedRunLength)
                            {
                                fEmit = true;
                            }

                            if(fEmit)
                            {
                                ms.WriteByte( data );

                                data = 0;
                                count = 0;
                                fEmit = false;
                                fRun = true;
                            }
                        }

                        data |= (byte)((0x1 << count) & (fSet ? 0xff : 0x0));

                        iPixelMask <<= 1;
                        if(iPixelMask == 0)
                        {
                            iPixelMask = 0x1;
                            iByte++;
                        }

                        count++;
                    }
                }

                if(fRun && count >= TinyResourceFile.CLR_GFX_BitmapDescription.c_CompressedRunOffset)
                {
                    data = TinyResourceFile.CLR_GFX_BitmapDescription.c_CompressedRun;
                    data |= (fSetSav ? TinyResourceFile.CLR_GFX_BitmapDescription.c_CompressedRunSet : (byte)0x0);
                    data |= (byte)(count - TinyResourceFile.CLR_GFX_BitmapDescription.c_CompressedRunOffset);
                }

                ms.WriteByte( data );

                if(ms.Length < buf.Length)
                {
                    ms.Capacity = (int)ms.Length;
                    buf = ms.GetBuffer();

                    bitmapDescription.m_flags |= TinyResourceFile.CLR_GFX_BitmapDescription.c_Compressed;
                }
            }

            private byte[] GetBitmapDataBmp( Bitmap bitmap, out TinyResourceFile.CLR_GFX_BitmapDescription bitmapDescription )
            {
                //issue warning for formats that we lose information?
                //other formats that we need to support??

                byte bitsPerPixel = 24;
                BitmapData bitmapData = null;
                Rectangle rect = new Rectangle( 0, 0, bitmap.Width, bitmap.Height );
                PixelFormat formatDst = bitmap.PixelFormat;
                byte[] data = null;

                switch(bitmap.PixelFormat)
                {
                    case PixelFormat.Format1bppIndexed:
                        bitsPerPixel = 1;
                        formatDst = PixelFormat.Format1bppIndexed;
                        break;
                    // Anything more than 16bpp will fall through to 16bpp
                    case PixelFormat.Format8bppIndexed:
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                    case PixelFormat.Format48bppRgb:
                    case PixelFormat.Format16bppRgb555:
                    case PixelFormat.Format16bppRgb565:
                        bitsPerPixel = 16;
                        formatDst = PixelFormat.Format16bppRgb565;
                        break;
                    default:
                        throw new NotSupportedException( string.Format( "PixelFormat of '{0}' resource not supported", this.Name ) );
                }

                //turn bitmap data into a form we can use.

                if(formatDst != bitmap.PixelFormat)
                {
                    bitmap = bitmap.Clone( rect, formatDst );
                }

                try
                {
                    bitmapData = bitmap.LockBits( rect, ImageLockMode.ReadOnly, formatDst );

                    IntPtr p = bitmapData.Scan0;
                    data = new byte[bitmapData.Stride * bitmap.Height];

                    System.Runtime.InteropServices.Marshal.Copy( bitmapData.Scan0, data, 0, data.Length );

                    if(bitsPerPixel == 1)
                    {
                        //special case for 1pp with index 0 equals white???!!!???
                        if(bitmap.Palette.Entries[0].GetBrightness() < 0.5)
                        {
                            for(int i = 0; i < data.Length; i++)
                            {
                                data[i] = (byte)~data[i];
                            }
                        }

                        //special case for 1pp need to flip orientation??
                        //for some stupid reason, 1bpp is flipped compared to windows!!
                        Adjust1bppOrientation( data );
                    }
                }
                finally
                {
                    if(bitmapData != null)
                    {
                        bitmap.UnlockBits( bitmapData );
                    }
                }

                bitmapDescription = new TinyResourceFile.CLR_GFX_BitmapDescription( (ushort)bitmap.Width, (ushort)bitmap.Height, 0, bitsPerPixel, TinyResourceFile.CLR_GFX_BitmapDescription.c_TypeBitmap );

                if(bitsPerPixel == 1)
                {
                    //test compression;
                    Compress1bpp( bitmapDescription, ref data );
                }

                return data;
            }

            private byte[] GetBitmapDataRaw( Bitmap bitmap, out TinyResourceFile.CLR_GFX_BitmapDescription bitmapDescription, byte type )
            {
                bitmapDescription = new TinyResourceFile.CLR_GFX_BitmapDescription( (ushort)bitmap.Width, (ushort)bitmap.Height, 0, 1, type);

                MemoryStream stream = new MemoryStream();
                bitmap.Save( stream, bitmap.RawFormat );

                stream.Capacity = (int)stream.Length;
                return stream.GetBuffer();
            }

            private byte[] GetBitmapDataJpeg( Bitmap bitmap, out TinyResourceFile.CLR_GFX_BitmapDescription bitmapDescription )
            {
                return GetBitmapDataRaw( bitmap, out bitmapDescription, TinyResourceFile.CLR_GFX_BitmapDescription.c_TypeJpeg );
            }

            private byte[] GetBitmapDataGif( Bitmap bitmap, out TinyResourceFile.CLR_GFX_BitmapDescription bitmapDescription )
            {
                return GetBitmapDataRaw( bitmap, out bitmapDescription, TinyResourceFile.CLR_GFX_BitmapDescription.c_TypeGif);
            }

            private byte[] GetBitmapData( Bitmap bitmap, out TinyResourceFile.CLR_GFX_BitmapDescription bitmapDescription )
            {
                byte[] data = null;

                if(bitmap.Width > 0xFFFF || bitmap.Height > 0xFFFF)
                {
                    throw new ArgumentException( "bitmap dimensions out of range" );
                }

                if(bitmap.RawFormat.Equals(ImageFormat.Jpeg))
                {
                    data = GetBitmapDataJpeg( bitmap, out bitmapDescription );
                }
                else if(bitmap.RawFormat.Equals(ImageFormat.Gif))
                {
                    data = GetBitmapDataGif( bitmap, out bitmapDescription );
                }
                else if(bitmap.RawFormat.Equals( ImageFormat.Bmp ))
                {
                    data = GetBitmapDataBmp( bitmap, out bitmapDescription );
                }
                else
                {
                    throw new NotSupportedException( string.Format("Bitmap imageFormat not supported '{0}'", bitmap.RawFormat.Guid.ToString()) );
                }

                return data;
            }

            public override byte[] GenerateResourceData()
            {
                Bitmap bitmap = this.BitmapValue;

                TinyResourceFile.CLR_GFX_BitmapDescription bitmapDescription;

                byte[] data = GetBitmapData( bitmap, out bitmapDescription );

                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter( stream );

                bitmapDescription.Serialize( writer );
                writer.Write( data );

                stream.Capacity = (int)stream.Length;
                return stream.GetBuffer();
            }

            /*
            public override byte[] GenerateResourceData()
            {
                byte[] data = null;

                ushort flags = 0;
                byte type = 0;
                byte bitsPerPixel = 24;
                BitmapData bitmapData = null;
                Bitmap bitmap = this.BitmapValue;
                PixelFormat formatDst = bitmap.PixelFormat;
                ushort width = (ushort)bitmap.Width;
                ushort height = (ushort)bitmap.Height;
                TinyResourceFile.CLR_GFX_BitmapDescription bitmapDescription;

                Rectangle rect = new Rectangle( 0, 0, this.BitmapValue.Width, this.BitmapValue.Height );

                if(bitmap.RawFormat.Equals(ImageFormat.Jpeg))
                {
                    type = TinyResourceFile.CLR_GFX_BitmapDescription.c_TypeJpeg;
                }
                else if(bitmap.RawFormat.Equals(ImageFormat.Gif))
                {
                    type = TinyResourceFile.CLR_GFX_BitmapDescription.c_TypeGif;
                }
                else if(bitmap.RawFormat.Equals(ImageFormat.Bmp))
                {
                    type = TinyResourceFile.CLR_GFX_BitmapDescription.c_TypeBitmap;

                    //issue warning for formats that we lose information?
                    //other formats that we need to support??

                    switch(bitmap.PixelFormat)
                    {
                        case PixelFormat.Format1bppIndexed:
                            bitsPerPixel = 1;
                            formatDst = PixelFormat.Format1bppIndexed;
                            break;
                        case PixelFormat.Format24bppRgb:
                        case PixelFormat.Format32bppRgb:
                        case PixelFormat.Format48bppRgb:
                        //currently don't support more than 16bp...fall through..
                        case PixelFormat.Format16bppRgb555:
                        case PixelFormat.Format16bppRgb565:
                            bitsPerPixel = 16;
                            formatDst = PixelFormat.Format16bppRgb565;
                            break;
                        default:
                            throw new NotSupportedException( string.Format( "PixelFormat of '{0}' resource not supported", this.Name ) );
                    }

                    //turn bitmap data into a form we can use.

                    if(formatDst != bitmap.PixelFormat)
                    {
                        bitmap = bitmap.Clone( rect, formatDst );
                    }
                }
                else
                {
                    throw new NotSupportedException( string.Format("Bitmap imageFormat not supported '{0}'", bitmap.RawFormat.Guid.ToString()) );
                }

                try
                {
                    bitmapData = bitmap.LockBits( rect, ImageLockMode.ReadOnly, formatDst );

                    IntPtr p = bitmapData.Scan0;
                    data = new byte[bitmapData.Stride * height];

                    System.Runtime.InteropServices.Marshal.Copy( bitmapData.Scan0, data, 0, data.Length );

                    if(bitsPerPixel == 1)
                    {
                        //special case for 1pp with index 0 equals white???!!!???
                        if(bitmap.Palette.Entries[0].GetBrightness() < 0.5)
                        {
                            for(int i = 0; i < data.Length; i++)
                            {
                                data[i] = (byte)~data[i];
                            }
                        }

                        //special case for 1pp need to flip orientation??
                        //for some stupid reason, 1bpp is flipped compared to windows!!
                        Adjust1bppOrientation( data );
                    }
                }
                finally
                {
                    if(bitmapData != null)
                    {
                        bitmap.UnlockBits( bitmapData );
                    }
                }

                TinyResourceFile.CLR_GFX_BitmapDescription bitmapDescription = new TinyResourceFile.CLR_GFX_BitmapDescription( width, height, flags, bitsPerPixel, type );

                if(bitsPerPixel == 1 && type == TinyResourceFile.CLR_GFX_BitmapDescription.c_TypeBitmap)
                {
                    //test compression;
                    Compress1bpp( bitmapDescription, ref data );
                }

                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter( stream );

                bitmapDescription.Serialize( writer );
                writer.Write( data );

                byte[] buf = new byte[stream.Length];

                Array.Copy( stream.GetBuffer(), buf, stream.Length );

                return buf;
            }
            */
        }

        private class TinyResourcesEntry : Entry
        {
            TinyResourceFile.ResourceHeader resource;

            public TinyResourcesEntry(string name, byte[] value) : base(name, value)
            {
            }

            public static TinyResourcesEntry TryCreateTinyResourcesEntry( string name, byte[] value )
            {
                TinyResourcesEntry entry = null;

                try
                {
                    MemoryStream stream = new MemoryStream( value );

                    BinaryReader reader = new BinaryReader( stream );
                    uint magicNumber = reader.ReadUInt32();

                    stream.Position = 0;

                    if(magicNumber == TinyResourceFile.Header.MAGIC_NUMBER)
                    {
                        TinyResourceFile file = new TinyResourceFile();

                        file.Deserialize( reader );

                        if(file.resources.Length == 1)
                        {
                            TinyResourceFile.Resource resource = file.resources[0];

                            entry = new TinyResourcesEntry( name, resource.data );
                            entry.resource = resource.header;
                        }
                    }
                }
                catch
                {
                }

                return entry;
            }

            private byte[] RawValue
            {
                get { return this.Value as byte[]; }
            }

            public override byte ResourceType
            {
                get
                {
                    return resource.kind;
                }
            }

            public override byte[] GenerateResourceData()
            {
                return this.RawValue;
            }

        }

        private class BinaryEntry : Entry
        {
            public BinaryEntry(string name, byte[] value) : base(name, value)
            {

            }

            private byte[] BinaryValue
            {
                get { return this.Value as byte[]; }
            }

            public override byte ResourceType
            {
                get
                {
                    return TinyResourceFile.ResourceHeader.RESOURCE_Binary;
                }
            }

            public override byte[] GenerateResourceData()
            {
                return this.BinaryValue;
            }
        }

        #endregion // Code from ResGen.EXE

        internal class TinyResourceFile
        {
            /*
                    .tinyresources file format.  The Header is shared with the Metadataprocessor.  Everything else
                    is shared with the TinyCLR>

                    -------
                    Header
                    -------

                    uint MagicNumber = 0xf995b0a8;
                    uint Version;
                    uint SizeOfHeader;
                    uint SizeOfResourceHeader;          //size of all CLR_RECORD_RESOURCE structures to follow
                    uint NumberOfResources;             //number of resources

                    ---------------------
                    Resource Headers
                    ---------------------

                    Starting at Header.SizeOfHeader in the stream,
                    Header.NumberOfResources resourceHeader structures



            */

            public Header header;
            public Resource[] resources;

            public class Resource
            {
                public ResourceHeader header;
                public byte[] data;

                public Resource( ResourceHeader header, byte[] data )
                {
                    this.header = header;
                    this.data = data;
                }
            }

            public TinyResourceFile()
            {
                resources = new Resource[0];
            }

            public TinyResourceFile( Header header ) : this()
            {
                this.header = header;
            }

            public void AddResource(Resource resource)
            {
                int cResource = resources.Length;

                Resource[] resourcesNew = new Resource[cResource + 1];
                resources.CopyTo( resourcesNew, 0 );
                resourcesNew[cResource] = resource;
                resources = resourcesNew;
            }

            public void Serialize( BinaryWriter writer )
            {
                this.header.Serialize( writer );

                for(int iResource = 0; iResource < this.resources.Length; iResource++)
                {
                    Resource resource = resources[iResource];

                    resource.header.Serialize( writer );
                    writer.Write( resource.data );
                }
            }

            public void Deserialize( BinaryReader reader )
            {
                header = new Header();
                header.Deserialize( reader );

                if(header.NumberOfResources == 0)
                {
                    throw new SerializationException("No resources found");
                }

                resources = new Resource[header.NumberOfResources];

                for(int iResource = 0; iResource < resources.Length; iResource++)
                {
                    Resource resource = new Resource(new ResourceHeader(), new byte[0]);

                    resources[iResource] = resource;
                    resource.header.Deserialize( reader );
                    resource.data = reader.ReadBytes( (int)resource.header.size );
                }
            }

            #region Records

            public class Header
            {
                public const uint VERSION = 2;
                public const uint MAGIC_NUMBER = 0xf995b0a8;
                public const uint SIZE_FILE_HEADER = 5 * 4;
                public const uint SIZE_RESOURCE_HEADER = 2 * 4;

                public uint MagicNumber;
                public uint Version;
                public uint SizeOfHeader;
                public uint SizeOfResourceHeader;
                public uint NumberOfResources;

                public Header( uint numResources )
                {
                    this.MagicNumber = MAGIC_NUMBER;
                    this.Version = VERSION;
                    this.SizeOfHeader = SIZE_FILE_HEADER;
                    this.SizeOfResourceHeader = SIZE_RESOURCE_HEADER;
                    this.NumberOfResources = numResources;
                }

                public Header()
                {
                }

                public void Serialize( BinaryWriter writer )
                {
                    writer.Write( MagicNumber );
                    writer.Write( Version );
                    writer.Write( SizeOfHeader );
                    writer.Write( SizeOfResourceHeader );
                    writer.Write( NumberOfResources );
                }

                public void Deserialize( BinaryReader reader )
                {
                    this.MagicNumber = reader.ReadUInt32();
                    this.Version = reader.ReadUInt32();
                    this.SizeOfHeader = reader.ReadUInt32();
                    this.SizeOfResourceHeader = reader.ReadUInt32();
                    this.NumberOfResources = reader.ReadUInt32();

                    reader.BaseStream.Position = this.SizeOfHeader;

                    if( this.MagicNumber != MAGIC_NUMBER ||
                         this.SizeOfHeader < SIZE_FILE_HEADER  ||
                         this.SizeOfResourceHeader < SIZE_RESOURCE_HEADER
                    )
                    {
                        throw new SerializationException();
                    }
                    else if(this.Version != VERSION)
                    {
                        throw new SerializationException( string.Format( "Incompatible version (version {0}) found, expecting version {1}.", this.Version, VERSION ) );
                    }
                }

                public uint OffsetOfResourceData
                {
                    get
                    {
                        return this.SizeOfHeader + this.SizeOfResourceHeader * this.NumberOfResources;
                    }
                }
            }

            public class ResourceHeader
            {
                public const byte RESOURCE_Invalid = 0x00;
                public const byte RESOURCE_Bitmap = 0x01;
                public const byte RESOURCE_Font = 0x02;
                public const byte RESOURCE_String = 0x03;
                public const byte RESOURCE_Binary = 0x04;
                public const byte RESOURCE_Max = RESOURCE_Binary;

                public short id;
                public byte kind;
                public byte pad;
                public uint size;

                public ResourceHeader( short id, byte kind, uint size )
                {
                    this.id = id;
                    this.kind = kind;
                    this.pad = 0;
                    this.size = size;
                }

                public ResourceHeader()
                {
                }

                public void Serialize( BinaryWriter writer )
                {
                    writer.Write( this.id );
                    writer.Write( this.kind );
                    writer.Write( this.pad );
                    writer.Write( this.size );
                }

                public void Deserialize( BinaryReader reader )
                {
                    this.id   = reader.ReadInt16();
                    this.kind = reader.ReadByte();
                    this.pad  = reader.ReadByte();
                    this.size = reader.ReadUInt32();
                }
            }

            public class CLR_GFX_BitmapDescription
            {
                public const ushort c_ReadOnly = 0x0001;
                public const ushort c_Compressed = 0x0002;

                public const ushort c_Rotation0 = 0x0000;
                public const ushort c_Rotation90 = 0x0004;
                public const ushort c_Rotation180 = 0x0008;
                public const ushort c_Rotation270 = 0x000b;
                public const ushort c_RotationMask = 0x000b;

                public const byte c_CompressedRun = 0x80;
                public const byte c_CompressedRunSet = 0x40;
                public const byte c_CompressedRunLengthMask = 0x3f;
                public const byte c_UncompressedRunLength = 7;
                public const byte c_CompressedRunOffset = c_UncompressedRunLength + 1;

                // Note that these type definitions has to match the ones defined in Bitmap.BitmapImageType enum defined in Graphics.cs
                public const byte c_TypeBitmap = 0;
                public const byte c_TypeGif = 1;
                public const byte c_TypeJpeg = 2;

                // !!!!WARNING!!!!
                // These fields should correspond to CLR_GFX_BitmapDescription in TinyCLR_Graphics.h
                // and should be 4-byte aligned in size. When these fields are changed, the version number
                // of the tinyresource file should be incremented, the tinyfnts should be updated (buildhelper -convertfont ...)
                // and the MMP should also be updated as well. (Consult rwolff before touching this.)
                public uint m_width;
                public uint m_height;
                public ushort m_flags;
                public byte m_bitsPerPixel;
                public byte m_type;

                public CLR_GFX_BitmapDescription( ushort width, ushort height, ushort flags, byte bitsPerPixel, byte type )
                {
                    this.m_width = width;
                    this.m_height = height;
                    this.m_flags = flags;
                    this.m_bitsPerPixel = bitsPerPixel;
                    this.m_type = type;
                }

                public CLR_GFX_BitmapDescription()
                {
                }

                public void Serialize( BinaryWriter writer )
                {
                    writer.Write( m_width );
                    writer.Write( m_height );
                    writer.Write( m_flags );
                    writer.Write( m_bitsPerPixel );
                    writer.Write( m_type );
                }

                public void Deserialize( BinaryReader reader )
                {
                    this.m_width = reader.ReadUInt16();
                    this.m_height = reader.ReadUInt16();
                    this.m_flags = reader.ReadUInt16();
                    this.m_bitsPerPixel = reader.ReadByte();
                    this.m_type = reader.ReadByte();
                }
            }
            #endregion
        }

        internal class TinyResourceWriter : IResourceWriter
        {
            string fileName;
            ArrayList resources;

            public TinyResourceWriter( string fileName )
            {
                this.fileName = fileName;
                this.resources = new ArrayList();
            }

            public void AddResource( Entry entry )
            {
                resources.Add( entry );
            }

            private void Add( string name, object value )
            {
                Entry entry = Entry.CreateEntry( name, value, string.Empty, string.Empty );
                resources.Add( entry );
            }

            #region IResourceWriter Members

            void IResourceWriter.AddResource( string name, byte[] value )
            {
                Add( name, value );
            }

            void IResourceWriter.AddResource( string name, object value )
            {
                Add( name, value );
            }

            void IResourceWriter.AddResource( string name, string value )
            {
                Add( name, value );
            }

            void IResourceWriter.Close()
            {
                ((IDisposable)this).Dispose();
            }

            void IResourceWriter.Generate()
            {
                //PrepareToGenerate();
                ProcessResourceFiles.EnsureResourcesIds( this.resources );

                TinyResourceFile.Header header = new TinyResourceFile.Header( (uint)resources.Count );
                TinyResourceFile file = new TinyResourceFile( header );

                for(int iResource = 0; iResource < resources.Count; iResource++)
                {
                    Entry entry = (Entry)resources[iResource];

                    byte[] data = entry.GenerateResourceData();
                    TinyResourceFile.ResourceHeader resource = new TinyResourceFile.ResourceHeader(entry.Id, entry.ResourceType, (uint)data.Length);

                    file.AddResource( new TinyResourceFile.Resource(resource, data) );
                }

                using(FileStream fileStream = File.Open( fileName, FileMode.OpenOrCreate ))
                {
                    BinaryWriter writer = new BinaryWriter( fileStream );
                    file.Serialize( writer );
                    fileStream.Flush();
                }
            }

            #endregion

            #region IDisposable Members

            void IDisposable.Dispose()
            {
            }

            #endregion
        }

        public class TinyResourceReader : IResourceReader
        {

            #region IResourceReader Members

            void IResourceReader.Close()
            {
                throw new NotImplementedException();
            }

            IDictionaryEnumerator IResourceReader.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IDisposable Members

            void IDisposable.Dispose()
            {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
}
