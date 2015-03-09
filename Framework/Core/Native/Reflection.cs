////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//
// Uncomment to debug the serialization.
//

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT
{
    [Flags()]
    public enum SerializationFlags
    {
        //
        // Keep in sync with Microsoft.SPOT.Debugger.SerializationHints!!!!
        //

        Encrypted = 0x00000001,
        Compressed = 0x00000002, // Value uses range compression (max 2^30 values).
        Optional = 0x00000004, // If the value cannot be deserialized, skip it.

        PointerNeverNull = 0x00000010,
        ElementsNeverNull = 0x00000020,

        FixedType = 0x00000100,

        DemandTrusted = 0x00010000,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = true)]
    public class SerializationHintsAttribute : Attribute
    {
        //
        // Keep in sync with Microsoft.SPOT.Debugger.SerializationHintsAttribute!!!!
        //

        public SerializationFlags Flags;

        public int ArraySize; // -1 == extend to the end of the stream.

        public int BitPacked;     // In bits.
        public long RangeBias;
        public ulong Scale;         // For time, it's in ticks.
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    sealed public class FieldNoReflectionAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
    sealed public class GloballySynchronizedAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    sealed public class PublishInApplicationDirectoryAttribute : Attribute
    {
    }

    public class UnknownTypeException : Exception
    {
        public Type m_type;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    public static class Reflection
    {
#if TINYCLR_TRACE_DOWNLOAD
        [Serializable] // Used in some logging.
#endif
        public class AssemblyInfo
        {
            public const uint c_Flags_NeedReboot = 0x00000001;

            public string m_name;
            public uint m_flags;
            public int m_size;
            public uint m_hash;
            public uint[] m_refs;
        }

        /// <summary>
        /// Represents the runtime memory usage information for the type system
        /// </summary>
        public class AssemblyMemoryInfo
        {
            /// <summary>
            /// Runtime memory overhead of the application due to the type system. This does not include any memory allocated by the application
            /// but only the memory that the application needs to be loaded in the type system. 
            /// </summary>
            public uint RamSize;
            /// <summary>
            /// ROM memory size in bytes of the application.  This is the size in bytes of the applicaiton in the block storage device. 
            /// </summary>
            public uint RomSize;
            /// <summary>
            /// The size in bytes of the metadata.  It includes the methods implementation as well
            /// </summary>
            public uint MetadataSize;
            /// <summary>
            /// The assembly references size in bytes
            /// </summary>
            public uint AssemblyRef;
            /// <summary>
            /// The number of assembly references elements
            /// </summary>
            public uint AssemblyRefElements;
            /// <summary>
            /// The type references size in bytes 
            /// </summary>
            public uint TypeRef;
            /// <summary>
            /// The number of type references elements
            /// </summary>
            public uint TypeRefElements;
            /// <summary>
            /// The field references size in bytes 
            /// </summary>
            public uint FieldRef;
            /// <summary>
            /// The number of field references elements
            /// </summary>
            public uint FieldRefElements;
            /// <summary>
            /// The method references size in bytes 
            /// </summary>
            public uint MethodRef;
            /// <summary>
            /// The number of method references elements
            /// </summary>
            public uint MethodRefElements;
            /// <summary>
            /// The type definition size in bytes 
            /// </summary>
            public uint TypeDef;
            /// <summary>
            /// The number of type definition elements
            /// </summary>
            public uint TypeDefElements;
            /// <summary>
            /// The field definition size in bytes 
            /// </summary>
            public uint FieldDef;
            /// <summary>
            /// The number of field definition elements
            /// </summary>
            public uint FieldDefElements;
            /// <summary>
            /// The method definition size in bytes 
            /// </summary>
            public uint MethodDef;
            /// <summary>
            /// The number of method definition elements
            /// </summary>
            public uint MethodDefElements;
            /// <summary>
            /// The static fields size in bytes
            /// </summary>
            public uint StaticFields;
            /// <summary>
            /// The attributes elements size in bytes 
            /// </summary>
            public uint Attributes;
            /// <summary>
            /// The number of attributes elements
            /// </summary>
            public uint AttributesElements;
            /// <summary>
            /// The type specifiers size in bytes 
            /// </summary>
            public uint TypeSpec;
            /// <summary>
            /// The number of type specifier elements
            /// </summary>
            public uint TypeSpecElements;
            /// <summary>
            /// The resources size in bytes 
            /// </summary>
            public uint Resources;
            /// <summary>
            /// The number of resources elements 
            /// </summary>
            public uint ResourcesElements;
            /// <summary>
            /// The resources files size in bytes 
            /// </summary>
            public uint ResourcesFiles;
            /// <summary>
            /// The number of resources elements
            /// </summary>
            public uint ResourcesFilesElements;
            /// <summary>
            /// The resources data size in bytes
            /// </summary>
            public uint ResourcesData;
            /// <summary>
            /// The strings size in bytes
            /// </summary>
            public uint Strings;
            /// <summary>
            /// The signatures size in bytes
            /// </summary>
            public uint Signatures;
            /// <summary>
            /// The bytecode size in bytes
            /// </summary>
            public uint ByteCode;          
        }

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public Type[] GetTypesImplementingInterface(Type itf);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool IsTypeLoaded(Type t);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public uint GetTypeHash(Type t);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public uint GetAssemblyHash(Assembly assm);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public Assembly[] GetAssemblies();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool GetAssemblyInfo(byte[] assm, AssemblyInfo ai);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool GetAssemblyMemoryInfo(Assembly assm, AssemblyMemoryInfo ami);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public Type GetTypeFromHash(uint hash);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public Assembly GetAssemblyFromHash(uint hash);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public byte[] Serialize(object o, Type t);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public object Deserialize(byte[] v, Type t);
    }
}


