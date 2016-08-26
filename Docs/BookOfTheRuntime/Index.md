# Inside the .NET Micro Framework (v4.4)
This document covers the internal design of the .NET Micro Framework as it exist in V4.4 and earlier.
This is to help document what already exists as well as to serve as a baseline reference for what is
different in later releases. It covers some historical constraints and factors influencing the design
of the framework, paying special attention to those factors that no longer exist.

## Table of Contents
1. [Major constraints on the original design](OriginalConstraints.md)
2. Tools and File Formats
    1. [Metadata Processor](MetadataProcessor.md)
        1. [Commands](MetadataProcessor.md#command-line-options)
        2. [Parsing](MetadataProcessor.md#parsing)
    2. [Common Data types and enumerations](Common-PE-Types-and-Enumerations.md)
    3. [PE File Format](PeFileFormat.md)
        1. [CLR_RECORD_ASSEMBLY](AssemblyHeader.md)
        2. [CLR_RECORD_ASSEMBLYREF](AssemblyRefTableEntry.md)
        3. [CLR_RECORD_TYPEREF](TypeRefTableEntry.md)
        4. [CLR_RECORD_TYPEDEF](TypeDefTableEntry.md)
        4. [CLR_RECORD_FILEDREF](FieldRefTableEntry.md)
        5. [CLR_RECORD_METHODREF](MethodRefTableEntry.md)
        6. [CLR_RECORD_METHODDEF](MethodDefTableEntry.md)
        6. [CLR_RECORD_ATTRIBUTE](AttributeTableEntry.md)
        7. [CLR_RECORD_TYPESPEC](TypeSpecTableEntry.md)
        8. [CLR_RECORD_EH](ExceptionHandlerTableEntry.md)
        9. [CLR_RECORD_RESOURCE](ResourcesTableEntry.md)
3. Internal Design of the Interpreter
    1. Heaps and the Heap data structures
        1. Uses of the heaps
        2. Heap Data Structures
            1. CLR_RT_HeapCluster
            2. CLR_RT_HeapBlock
            3. CLR_RT_HeapBlock_Node
            4. CLR_RT_DblLinkedList
            5. CLR_RT_HeapBlock_String
            6. CLR_RT_HeapBlock_Array
            7. CLR_RT_HeapBlock_Delegate_List
            8. CLR_RT_HeapBlock_BinaryBlob
            9. CLR_RT_HeapBlock_WeakReference
            10. CLR_RT_HeapBlock_WeakReference_Identity
            11. CLR_RT_ObjectToEvent_Source
            12. CLR_RT_ObjectToEvent_Destination
            13. CLR_RT_HeapBlock_Timer
            14. CLR_RT_HeapBlock_EndPoint
        3. Garbage Collection
            1. CLR_RT_ProtectFromGC
        4. Compaction
        5. Persistent Storage 
            1. CLR_RT_Persistence_Manager
    2. Execution Engine