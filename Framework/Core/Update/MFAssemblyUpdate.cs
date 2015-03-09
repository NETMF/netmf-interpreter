////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Microsoft.SPOT.MFUpdate
{
    using System;
    using System.Collections;
    using System.Reflection;

    /// <summary>
    /// Represents an assembly name and version to be used by an application to 
    /// determine which assemblies may need to be updated.
    /// </summary>
    public class MFAssemblyNames
    {
        public MFAssemblyNames(string assemblyFullName, Version assemblyVersion)
        {
            FullName = assemblyFullName;
            Version = assemblyVersion;
        }

        /// <summary>
        /// The full name of the assembly.
        /// </summary>
        public readonly string FullName;
        /// <summary>
        /// The version of the assembly.
        /// </summary>
        public readonly Version Version;
    }

    /// <summary>
    /// Represents an assembly update package.
    /// </summary>
    public class MFAssemblyUpdate : MFUpdate
    {
        private static MFAssemblyNames[] s_InstalledAsms;

        /// <summary>
        /// Creates an assembly update package.
        /// </summary>
        /// <param name="provider">The update provider name that will process this update.</param>
        /// <param name="updateID">The unique update identification number.</param>
        /// <param name="version">The assembly update version.</param>
        /// <param name="assemblyType">The assembly update subtype.</param>
        /// <param name="updateSize">The size (in bytes) of the entire update.</param>
        /// <param name="pktSize">The size (in bytes) of each packet.</param>
        public MFAssemblyUpdate(
            string provider,
            uint updateID,
            Version version,
            MFUpdateSubType assemblyType,
            int updateSize,
            int pktSize
            )
            : base(provider, updateID, version, MFUpdateType.AssemblyUpdate, assemblyType, updateSize, pktSize)
        {
        }

        /// <summary>
        /// Gets the list of installed assemblies on the device for the current AppDomain.
        /// </summary>
        /// <returns></returns>
        public static MFAssemblyNames[] GetInstalledAssemblies()
        {
            if (s_InstalledAsms == null)
            {
                lock (typeof(MFAssemblyUpdate))
                {
                    if (s_InstalledAsms == null)
                    {
                        Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
                        s_InstalledAsms = new MFAssemblyNames[asms.Length];

                        for (int i = 0, c = asms.Length; i < c; i++)
                        {
                            AssemblyName asmName = asms[i].GetName();

                            s_InstalledAsms[i] = new MFAssemblyNames(asmName.FullName, asmName.Version);
                        }
                    }
                }
            }

            return s_InstalledAsms;
        }
    }
}