#ifndef _METADATATABLES_H_
#define _METADATATABLES_H_

////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files
// except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing permissions
// and limitations under the License.
// 
////////////////////////////////////////////////////////////////////////////////////////////////

#include <cstdint>
#include <type_traits>
#include <cassert>

#include "EnumFlags.h"
#include "Utilities.h"

namespace NETMF
{
    namespace Metadata
    {
        // macro to generate a compile time error if the given structure is not a proper C++ POD type
        // POD types are essential for ensuring binary compatibility with the memory layout generated
        // by the NETMF tools. Since this code is intended for use on the desktop and on devices it
        // must remain portable so that the raw PE images are accessible directly in memory as-is.
        #define ASSERT_METADATA_STRUCT_IS_POD( t ) \
        static_assert( std::is_pod<t>::value \
                     , "Metadata structure " #t " MUST always remain a POD structure for portability" \
                     );

        using Microsoft::Utilities::EnumFlags;

        constexpr uint16_t MakeOpcode( uint8_t msb, uint8_t lsb )
        {
            return ( msb << 8 ) + lsb;
        }

        enum class Opcode : uint16_t
        {
#define OPDEF( id, name, pop, push, params, kind, len, b1, b2, flow )\
            id = MakeOpcode( b1, b2 ),
#include <Opcode.def>
#undef OPDEF
        };

        // using a typedef for each of these helps in providing more
        // self documenting code. 
        typedef uint16_t StringTableIndex;
        typedef uint16_t TypeDefTableIndex;
        typedef uint16_t TypeRefTableIndex;
        typedef uint16_t FieldDefTableIndex;
        typedef uint16_t MethodDefTableIndex;
        typedef uint16_t SigTableIndex;
        typedef uint32_t MetadataToken;
        typedef uint16_t MetadataOffset;
        typedef uint8_t const* MetadataPtr;

        const uint16_t EmptyIndex = 0xFFFF;

        enum class TableKind : uint16_t
        {
               AssemblyRef = 0x0000,
                   TypeRef = 0x0001,
                  FieldRef = 0x0002,
                 MethodRef = 0x0003,
                   TypeDef = 0x0004,
                  FieldDef = 0x0005,
                 MethodDef = 0x0006,
                Attributes = 0x0007,
                  TypeSpec = 0x0008,
                 Resources = 0x0009,
             ResourcesData = 0x000A, // Blob
                   Strings = 0x000B, // Blob
                Signatures = 0x000C, // Blob
                  ByteCode = 0x000D, // Blob
            ResourcesFiles = 0x000E,
             EndOfAssembly = 0x000F,
                       Max = 0x0010,
        };


        static const uint32_t TableMax = static_cast< uint32_t >( TableKind::Max );
        
        // Traits style template specialized by DECLARE_TABLEKIND
        // macro to bind the table type with the enumeration index.
        // This allows other templates based on the type to determine
        // the correct index without repetitive and error prone manual
        // alternatives.
        template< typename T>
        struct table_kind;

        #define DECLARE_TABLEKIND( t, k )\
        ASSERT_METADATA_STRUCT_IS_POD( t ) \
        template<> \
        struct table_kind<t> \
        {\
            static const TableKind value = k;\
        };

        // some token types in the Binary form refer to 
        // one of two possible tables only, this template
        // provides support for defining such types and 
        // extracting the table and index values from them
        // 
        template< TableKind bitSetKind, TableKind bitUnsetKind>
        struct BinaryToken
        {
            uint16_t Value;

            uint16_t GetIndex( ) const
            {
                return Value &0x7FFF;
            }

            TableKind GetTableKind( ) const
            {
                return ( Value & 0x8000 ) ? bitSetKind : bitUnsetKind;
            }
        };

        typedef BinaryToken<TableKind::TypeRef, TableKind::AssemblyRef> TypeRefOrAssemblyRef;
        typedef BinaryToken<TableKind::TypeRef, TableKind::TypeDef> TypeRefOrTypeDef;
        typedef BinaryToken<TableKind::MethodRef, TableKind::MethodDef> MethodRefOrMethodDef;
        typedef BinaryToken<TableKind::FieldRef, TableKind::FieldDef> FieldRefOrFieldDef;

        static_assert( sizeof( TypeRefOrAssemblyRef ) == sizeof( uint16_t ), "structure template size mismatch!" );
        static_assert( sizeof( TypeRefOrTypeDef ) == sizeof( uint16_t ), "structure template size mismatch!" );

        // Tokens in NETMF PE tables take two forms
        // 1. Full token: 8 bit TableKind + 24 bit table index
        // 2. Binary token: 1 bit Table kind flag + 15 bit index
        // The meaning of the 1 bit flag depends on the usage but is always used to
        // select between one of two tables (i.e. TypeDef or TypeRef)
        constexpr uint32_t IndexFromToken( MetadataToken tk ) { return tk & 0x00FFFFFF; }
        constexpr TableKind TypeFromToken( MetadataToken tk ) { return static_cast<TableKind>( tk >> 24 ); }

        template< TableKind bitSetKind, TableKind bitUnsetKind>
        constexpr uint16_t IndexFromToken( BinaryToken<bitSetKind,bitUnsetKind> tk )
        { 
            return tk.GetIndex();
        }

        template< TableKind bitSetKind, TableKind bitUnsetKind>
        constexpr TableKind TypeFromToken( BinaryToken<bitSetKind,bitUnsetKind> tk )
        {
            return tk.GetTableKind();
        }

        // Constructs a full token from table and index values
        constexpr MetadataToken MakeToken( TableKind tbl, uint32_t data )
        { 
            return ( ( static_cast<uint32_t>( tbl) << 24 ) & 0xFF000000 ) | ( data & 0x00FFFFFF );
        }

        struct VersionInfo
        {
            uint16_t MajorVersion;
            uint16_t MinorVersion;
            uint16_t BuildNumber;
            uint16_t RevisionNumber;
        };
        ASSERT_METADATA_STRUCT_IS_POD( VersionInfo )

        struct AssemblyRefTableEntry
        {
            StringTableIndex Name;
            uint16_t pad;
            VersionInfo Version;
        };
        DECLARE_TABLEKIND( AssemblyRefTableEntry, TableKind::AssemblyRef )

        struct TypeRefTableEntry
        {
            StringTableIndex Name;
            StringTableIndex NameSpace;

            TypeRefOrAssemblyRef Scope;
            uint16_t Pad;
        };
        DECLARE_TABLEKIND( TypeRefTableEntry, TableKind::TypeRef )

        struct FieldRefTableEntry
        {
            StringTableIndex Name;
            TypeRefTableIndex Container;

            SigTableIndex Sig;
            uint16_t Pad;
        };
        DECLARE_TABLEKIND( FieldRefTableEntry, TableKind::FieldRef )

        struct MethodRefTableEntry
        {
            StringTableIndex Name;
            TypeRefTableIndex Container;
            SigTableIndex Sig;
            uint16_t Pad;
        };
        DECLARE_TABLEKIND( MethodRefTableEntry, TableKind::MethodRef )

        enum class TypeDefFlags : uint16_t
        {
                         None = 0,
                    ScopeMask = 0x0007,
                    NotPublic = 0x0000, // Class is not public scope.
                       Public = 0x0001, // Class is public scope.
                 NestedPublic = 0x0002, // Class is nested with public visibility.
                NestedPrivate = 0x0003, // Class is nested with private visibility.
                 NestedFamily = 0x0004, // Class is nested with family visibility.
               NestedAssembly = 0x0005, // Class is nested with assembly visibility.
            NestedFamANDAssem = 0x0006, // Class is nested with family and assembly visibility.
             NestedFamORAssem = 0x0007, // Class is nested with family or assembly visibility.
                 Serializable = 0x0008,
                SemanticsMask = 0x0030,
                        Class = 0x0000,
                    ValueType = 0x0010,
                    Interface = 0x0020,
                         Enum = 0x0030,
                     Abstract = 0x0040,
                       Sealed = 0x0080,
                  SpecialName = 0x0100,
                     Delegate = 0x0200,
            MulticastDelegate = 0x0400,
                      Patched = 0x0800,
              BeforeFieldInit = 0x1000,
                  HasSecurity = 0x2000,
                 HasFinalizer = 0x4000,
                HasAttributes = 0x8000,
        };
        ENUM_FLAGS( TypeDefFlags )

        struct TypeDefTableEntry
        {
            StringTableIndex Name;
            StringTableIndex NameSpace;

            TypeRefOrTypeDef Extends;
            TypeDefTableIndex EnclosingType; // nested type?

            SigTableIndex Interfaces;
            MethodDefTableIndex FirstMethod;

            uint8_t VirtualMethodCount;
            uint8_t InstanceMethodCount;
            uint8_t StaticMethodCount;
            uint8_t dataType;

            FieldDefTableIndex FirstStaticField;
            FieldDefTableIndex FirstInstanceField;

            uint8_t StaticFieldCount;
            uint8_t InstanceFieldCount;
            TypeDefFlags flags;

            bool IsEnum( ) const
            {
                return ( flags & ( TypeDefFlags::SemanticsMask ) ) == TypeDefFlags::Enum;
            }

            bool IsDelegate( ) const
            {
                return ( flags & ( TypeDefFlags::Delegate | TypeDefFlags::MulticastDelegate ) ) != TypeDefFlags::None;
            }
        };
        DECLARE_TABLEKIND( TypeDefTableEntry, TableKind::TypeDef )

        enum class FieldDefFlags : uint16_t
        {
                     None = 0x0000,
                ScopeMask = 0x0007,
             PrivateScope = 0x0000,     // Member not referenceable.
                  Private = 0x0001,     // Accessible only by the parent type.
              FamANDAssem = 0x0002,     // Accessible by sub-types only in this Assembly.
                 Assembly = 0x0003,     // Accessibly by anyone in the Assembly.
                   Family = 0x0004,     // Accessible only by type and sub-types.
               FamORAssem = 0x0005,     // Accessibly by sub-types anywhere, plus anyone in assembly.
                   Public = 0x0006,     // Accessibly by anyone who has visibility to this scope.
            NotSerialized = 0x0008,     // Field does not have to be serialized when type is remoted.
                   Static = 0x0010,     // Defined on type, else per instance.
                 InitOnly = 0x0020,     // Field may only be initialized, not written to after init.
                  Literal = 0x0040,     // Value is compile time constant.
              SpecialName = 0x0100,     // field is special.  Name describes how.
               HasDefault = 0x0200,     // Field has default.
              HasFieldRVA = 0x0400,     // Field has RVA.
             NoReflection = 0x0800,     // field does not allow reflection
            HasAttributes = 0x8000,
        };
        ENUM_FLAGS( FieldDefFlags )

        struct FieldDefTableEntry
        {
            StringTableIndex Name;
            SigTableIndex sig;

            SigTableIndex DefaultValue;
            FieldDefFlags Flags;
        };
        DECLARE_TABLEKIND( FieldDefTableEntry, TableKind::FieldDef )

        enum class MethodDefFlags : uint32_t
        {
                            None = 0x00000000,
                       ScopeMask = 0x00000007,
                    PrivateScope = 0x00000000,  // Member not referenceable.
                         Private = 0x00000001,  // Accessible only by the parent type.
                     FamANDAssem = 0x00000002,  // Accessible by sub-types only in this Assembly.
                           Assem = 0x00000003,  // Accessibly by anyone in the Assembly.
                          Family = 0x00000004,  // Accessible only by type and sub-types.
                      FamORAssem = 0x00000005,  // Accessibly by sub-types anywhere, plus anyone in assembly.
                          Public = 0x00000006,  // Accessibly by anyone who has visibility to this scope.
                          Static = 0x00000010,  // Defined on type, else per instance.
                           Final = 0x00000020,  // Method may not be overridden.
                         Virtual = 0x00000040,  // Method virtual.
                       HideBySig = 0x00000080,  // Method hides by name+sig, else just by name.
                VtableLayoutMask = 0x00000100,
                       ReuseSlot = 0x00000000,  // The default.
                         NewSlot = 0x00000100,  // Method always gets a new slot in the vtable.
                        Abstract = 0x00000200,  // Method does not provide an implementation.
                     SpecialName = 0x00000400,  // Method is special.  Name describes how.
                  NativeProfiled = 0x00000800,
                     Constructor = 0x00001000,
               StaticConstructor = 0x00002000,
                       Finalizer = 0x00004000,
             DelegateConstructor = 0x00010000,
                  DelegateInvoke = 0x00020000,
             DelegateBeginInvoke = 0x00040000,
               DelegateEndInvoke = 0x00080000,
                    Synchronized = 0x01000000,
            GloballySynchronized = 0x02000000,
                         Patched = 0x04000000,
                      EntryPoint = 0x08000000,
                RequireSecObject = 0x10000000,  // Method calls another method containing security code.
                     HasSecurity = 0x20000000,  // Method has security associate with it.
            HasExceptionHandlers = 0x40000000,
                   HasAttributes = 0x80000000,
        };
        ENUM_FLAGS( MethodDefFlags )

        struct MethodDefTableEntry
        {
            StringTableIndex Name;
            MetadataOffset RVA;

            MethodDefFlags Flags;

            uint8_t RetVal;
            uint8_t NumArgs;
            uint8_t NumLocals;
            uint8_t LengthEvalStack;

            SigTableIndex Locals;
            SigTableIndex Sig;
        };
        DECLARE_TABLEKIND( MethodDefTableEntry, TableKind::MethodDef )

        struct AttributeTableEntry
        {
            TableKind OwnerType;      // one of TableKind::TypeDef, TableKind::MethodDef, or TableKind::FieldDef.
            uint16_t OwnerIdx;        // index into the table specified by ownerType
            MethodRefOrMethodDef Constructor;
            SigTableIndex Data;
        };
        DECLARE_TABLEKIND( AttributeTableEntry, TableKind::Attributes )

        struct TypeSpecTableEntry
        {
            SigTableIndex Sig;
            uint16_t pad;
        };
        DECLARE_TABLEKIND( TypeSpecTableEntry, TableKind::TypeSpec )

        enum class ExceptionHandlerMode : uint16_t
        {
               Catch = 0x0000,
            CatchAll = 0x0001,
             Finally = 0x0002,
              Filter = 0x0003,
        };

        // if a method has the MethodDefFlags::HasExceptionHandlers flag
        // set then the last byte of the ByteCode table for the method
        // is the number of Exception handlers for the method.
        // The exception handlers precede the count in the byte code
        // stream (e.g. with a negative offset from the size). 
        // NOTE:
        // This means that instances of this structure stored in a PE
        // image may be stored at an address that is *NOT* correctly
        // aligned for the structure. Thus consumers should always copy
        // the data into a properly aligned buffer.
        // REVIEW:
        // In a future revision of the PE binary format this should be
        // managed by inserting padding into the ByteCode stream so that
        // copying the data isn't needed.
        // TODO:
        // VERIFY whether MetadataProcessor will align the table entries
        // or not. (Current NETMF CLR seems to assume it won't and does
        // a memcpy)
        struct ExceptionHandlerTableEntry
        {
            ExceptionHandlerMode Mode;
            union
            {
                TypeRefOrTypeDef ClassToken;
                MetadataOffset FilterStart;
            };
            MetadataOffset TryStart;
            MetadataOffset TryEnd;
            MetadataOffset HandlerStart;
            MetadataOffset HandlerEnd;
            
            bool IsCatchOrCatchAny( ) const
            {
                return Mode == ExceptionHandlerMode::Catch
                    || Mode == ExceptionHandlerMode::CatchAll;
            }

            bool InRange( MetadataOffset ilOffset, MetadataOffset start, MetadataOffset end ) const
            {
                return ( start < ilOffset ) && ( ilOffset <= end );
            }

            bool InTryBlock( MetadataOffset ilOffset ) const
            {
                return InRange( ilOffset, TryStart, TryEnd );
            }

            bool InHandlerBlock( MetadataOffset ilOffset ) const
            {
                return InRange( ilOffset, HandlerStart, HandlerEnd );
            }

            bool IsTryStart( MetadataOffset ilOffset ) const
            {
                return ilOffset == ( TryStart + 1 );
            }

            bool IsTryEnd( MetadataOffset ilOffset ) const
            {
                return ilOffset == TryEnd;
            }

            bool IsHandlerStart( MetadataOffset ilOffset ) const
            {
                return ilOffset == ( HandlerStart + 1 );
            }

            bool IsHandlerEnd( MetadataOffset ilOffset ) const
            {
                return ilOffset == HandlerEnd;
            }

            //bool InFilter( MetadataPtr pByteCodeStream, MetadataPtr p )

        };
        ASSERT_METADATA_STRUCT_IS_POD( ExceptionHandlerTableEntry )

        static_assert( sizeof( ExceptionHandlerTableEntry ) == 12, "Record size mismatch!" );

        struct ResourcesFilesTableEntry
        {
            static const uint32_t CurrentVersion = 2;

            uint32_t Version;        // Should be == CurrentVersion
            // sizeOfHeader Should be sizeof( ResourcesFilesTableEntry )
            // but isn't due to a minor bug in the original NETMF
            // MetadataProcessor code (in Linker.cpp)
            // where this field is set using:
            //     resourceFile->sizeOfHeader = (sizeof(resourceFile));
            // when it should have used:
            //     resourceFile->sizeOfHeader = (sizeof(*resourceFile));
            // Thus the value, for version 2 at least, is actually 4
            // (the size of a 32 bit pointer instead of the size of the structure itself)
            uint32_t SizeOfHeader;   
            uint32_t SizeOfResourceHeader; // should be sizeof( ResourcesTableEntry ) 
            uint32_t NumberOfResources;
            StringTableIndex Name;
            uint16_t Pad;
            uint32_t Offset;          // TableKind::Resource
        };
        DECLARE_TABLEKIND( ResourcesFilesTableEntry, TableKind::ResourcesFiles )

        // in future release of NETMF CLR the
        // bitmap and font enumeration values here
        // should be removed and treated as binary
        // blobs.
        // This requires that the PE generator
        // (or more specifically the NETMF resource generator)
        // understands the format and can perform the necessary
        // byte order conversions.
        enum class ResourceKind : uint8_t
        {
            Invalid = 0x00,
             Bitmap = 0x01, // binary image with CLR_GFX_BitmapDescription format
               Font = 0x02, // binary data with CLR_GFX_FontDescription format
             String = 0x03,
             Binary = 0x04,
        };

        struct ResourcesTableEntry
        {
            // The last entry in the Resources table 
            // will have:
            //     id = SentinelId
            //   kind = Invalid
            // offset = size of the ResourceData table
            //  flags = 0
            // This is used to ensure that the preceding
            // entry can use the offset of the last entry
            // to compute the size of its data in the
            // ResourceData blob table.
            static const uint16_t SentinelId = 0x7FFF;

            // lower two bits of the flags is the padding
            // applied to align this entries data in the
            // ResourcesData blob table. That is the
            // size of the previous entries data is the
            // offset of this entry - offset of the previous
            // entry - padding for this entry.
            static const uint8_t FlagsPaddingMask = 0x03;

            // Sorted on id
            uint16_t Id;
            ResourceKind Kind;
            uint8_t Flags;

            // offset into the ResourceData blob table for the
            // start of the resource data
            uint32_t Offset; // TableKind::ResourceData

            // Compute size of the resourceData blob for this entry
            // based on the start of the next entry
            // The size of the resource data blob is determined
            // by subtracting the offset of this entry from the
            // offset field of the ResourcesTableEntry immediately
            // following this one in the ResourecesTable. (See comments
            // on SentinelId above for details on how the last entry is
            // handled)
            // NOTE: 
            // The next parameter must actually be the immediate next
            // entry in the table or the resulting size is invalid and
            // unspecified.
            size_t SizeOfData( ResourcesTableEntry const& next ) const
            {
                return next.Offset - Offset + ( next.Flags & FlagsPaddingMask );
            }
        };
        DECLARE_TABLEKIND( ResourcesTableEntry, TableKind::Resources )

        enum class AssemblyHeaderFlags : uint32_t
        {
                  None = 0x00000000,
            NeedReboot = 0x00000001,
                 Patch = 0x00000002,
             BigEndian = 0x80000080
        };
        ENUM_FLAGS( AssemblyHeaderFlags )

        enum CallingConvention : uint8_t
        {
                 Default = 0x0,
                  VarArg = 0x5,
                   Field = 0x6,
                LocalSig = 0x7,
                Property = 0x8,
                   Unmgd = 0x9,
             GenericInst = 0xa,  // generic method instantiation
            NativeVarArg = 0xb,  // used ONLY for 64bit vararg PInvoke calls
                     Max = 0xc,  // first invalid calling convention
          
          // The high bits of the calling convention convey additional info
                    Mask = 0x0f, // Calling convention is bottom 4 bits
                 HasThis = 0x20, // Top bit indicates a 'this' parameter
            ExplicitThis = 0x40, // This parameter is explicitly in the signature
                 Generic = 0x10, // Generic method sig with explicit number of type arguments (precedes ordinary parameter count)
        };

        // NOTE: While this NETMF specific enum corresponds to the ECMA ELEMENT_TYPE_xxxx
        // the actual numeric values are not the same and it is a reduced set of values.
        enum class DataType : uint8_t
        {
            Void,      // 0 bytes
            Boolean,   // 1 byte
            I1,        // 1 byte
            U1,        // 1 byte
            CHAR,      // 2 bytes
            I2,        // 2 bytes
            U2,        // 2 bytes
            I4,        // 4 bytes
            U4,        // 4 bytes
            R4,        // 4 bytes
            I8,        // 8 bytes
            U8,        // 8 bytes
            R8,        // 8 bytes
            DateTime,  // 8 bytes     // Shortcut for System.DateTime
            TimeSpan,  // 8 bytes     // Shortcut for System.TimeSpan
            String,   
            Object,    // Shortcut for System.Object
            Class,     // CLASS <class Token>
            ValueType, // VALUETYPE <class Token>
            SZArray,   // Shortcut for single dimension zero lower bound array SZARRAY <type>
            ByRef,     // BYREF <type>
        };
        
        template<typename T> class AssemblyTable;

        // This structure defines an Assembly in the NETMF PE binary format
        // One, and only one, such header exists in a PE file and it is
        // located at offset 0 of the file. At run time, multiple assembly
        // headers are located in the well known memory regions used by the
        // CLR. The memory regions are scanned for valid headers (e.g. marker
        // and CRCs are valid )
        struct AssemblyHeader
        {
            uint8_t Marker[ 8 ];

            uint32_t HeaderCRC;
            uint32_t AssemblyCRC;
            AssemblyHeaderFlags Flags;

            uint32_t NativeMethodsChecksum;
            // offset into TableKind::ResourceData for native "patch" code exists.
            // At runtime if this is not 0xFFFFFFFF then the CLR will compute
            // a physical address of the start of the patch code and call the function
            // located there. The function must be position independent and must have 
            // the following signature void PatchEntry();
            // This is a very limited mechanism at present and ultimately requires deep
            // knowledge of the underlying platform HAL/PAL etc... to be of any real use
            // Of special importance is the location of the assembly in physical memory
            // as many micro controllers limit the memory addresses where executable
            // code can reside. (i.e. internal flash or RAM only ) thus, this is not a
            // generalized extensibility/ dynamically loaded native code mechanism.
            uint32_t PatchEntryOffset; 

            VersionInfo Version;

            StringTableIndex AssemblyName;
            uint16_t StringTableVersion;

            uint32_t StartOfTables[ TableMax ];
            uint32_t NumPatchedMethods;

            // For every table, a number of bytes that were padded to the end of the table
            // to align to a 32bit boundary. The start of each table is aligned to a 32bit
            // boundary, and ends at a 32bit boundary.  Some of these tables will, therefore,
            // have no padding, and all will have values in the range [0-3]. This isn't the
            // most compact form to hold this information, but it only costs 16 bytes/assembly.
            // Trying to only align some of the tables is just much more hassle than it's worth.
            // This field itself must also be aligned on a 32 bit boundary.
            //uint8_t PaddingOfTables[ ( ( TableMax - 1 ) + 3 ) / 4 * 4 ];
            uint8_t PaddingOfTables[ TableMax ];

            uint32_t SizeOfTable( TableKind index ) const
            {
                auto tbl = static_cast< uint32_t >( index );
                return StartOfTables[ tbl + 1 ] - StartOfTables[ tbl ] - PaddingOfTables[ tbl ];
            }

            int32_t TableElementCount( TableKind index ) const;

            uint32_t TotalSize( ) const
            {
                return StartOfTables[ static_cast< uint32_t >( TableKind::EndOfAssembly ) ];
            }

            template<typename T>
            AssemblyTable<T> GetTable( ) const;

            MetadataPtr GetTable( TableKind tbl ) const
            {
                return reinterpret_cast<MetadataPtr>( this ) + StartOfTables[ static_cast<uint8_t>( tbl ) ];
            }

            bool VerifyHeaderCRC( ) const
            {
                return HeaderCRC == ComputeCRC( this, sizeof( AssemblyHeader ), 0, offsetof( AssemblyHeader, HeaderCRC ), sizeof( AssemblyHeader::HeaderCRC ) );
            }

            bool VerifyAssemblyCRC( ) const
            {
                // Compute CRC of assembly data following this header
                return AssemblyCRC == ComputeCRC( &this[ 1 ], this->TotalSize( ) - sizeof( *this ), 0 );
            }

            bool VerifyCRCs( ) const
            {
                return VerifyHeaderCRC( ) && VerifyAssemblyCRC( );
            }

            bool FindMethodBoundaries( MethodDefTableIndex i, MetadataOffset& start, MetadataOffset& end ) const;

            bool FindMethodBoundaries( MethodDefTableEntry const& pMethodDef
                                     , MethodDefTableIndex i
                                     , MetadataOffset& start
                                     , MetadataOffset& end
                                     ) const;
            char const* LookupString( StringTableIndex i ) const;
            static uint32_t UncompressData( MetadataPtr& p );
        
        // NOTE: private static fields don't violate POD rules
        private:
            static size_t const StringTableIndexToBlobOffsetMapSize;
            static uint16_t const StringTableIndexToBlobOffsetMap[ ];
            static char const WellKnownStringTableBlob[ ];
        };
        ASSERT_METADATA_STRUCT_IS_POD( AssemblyHeader )
        static_assert( 0 == ( offsetof( AssemblyHeader, StartOfTables ) & 0x03 ), "StartOfTables not aligned on 32bit boundary" );
        static_assert( 0 == ( offsetof( AssemblyHeader, PaddingOfTables ) & 0x03 ), "PaddingOfTables not aligned on 32bit boundary" );

        // Extract a potentially compressed type token from
        // a metadata stream
        inline MetadataToken TokenFromStream( MetadataPtr& p )
        {
            static const TableKind lookup[ 4 ] = { TableKind::TypeDef
                                                 , TableKind::TypeRef
                                                 , TableKind::TypeSpec
                                                 , TableKind::Max
                                                 };
            uint32_t data = AssemblyHeader::UncompressData( p );
            return MakeToken( lookup[ data & 3 ], data >> 2 );
        }
        
        // parse an opcode from a stream
        // MSIL opcodes may be encoded as one or two bytes,
        // the Prefix 0xFE indicates a two byte code  
        inline Opcode OpcodeFromStream( MetadataPtr& p )
        {
            uint16_t retVal = *p++;
            if( retVal != 0xFE )
                retVal += 0xFF00;
            else
            {
                retVal <<= 8;
                retVal += *p++;
            }
            return static_cast<Opcode>( retVal );
        }
        
        // Container Facade for the collection of exception
        // handlers in a method definition
        class IterableExceptionHandlers final
        {
        public:
            IterableExceptionHandlers( )
                : pHandlers( nullptr )
                , NumHandlers( 0 )
            {
            }
            
            ~IterableExceptionHandlers( )
            {
                delete[] pHandlers;
                pHandlers = nullptr;
            }
            
            IterableExceptionHandlers( IterableExceptionHandlers const& ) = delete;
            IterableExceptionHandlers( IterableExceptionHandlers const&& other) = delete;

            ExceptionHandlerTableEntry const* begin( ) const
            {
                return pHandlers;
            }

            ExceptionHandlerTableEntry const* end( ) const
            {
                return pHandlers + NumHandlers;
            }

            size_t length( ) const
            {
                return NumHandlers;
            }

        private:
            friend class IterableByteCodeStream;
            void InitFromByteCode( MetadataPtr& pIlEnd );

            ExceptionHandlerTableEntry const* pHandlers;
            size_t NumHandlers;
        };

        // Container facade for the NETMF IL in a
        // method definition body
        class IterableByteCodeStream final
        {
        public:
            IterableByteCodeStream( AssemblyHeader const& header, MethodDefTableIndex i );

            MetadataPtr begin( ) const
            { 
                return IlStart;
            }

            MetadataPtr end( ) const
            {
                return IlEnd;
            }

            uint8_t operator[]( int index ) const
            {
                return *( begin( ) + index );
            }
            
            size_t length( ) const
            {
                return IlEnd - IlStart;
            }
            
            ExceptionHandlerTableEntry const* GetHandlerFor( MetadataOffset ilOffset )
            {
                ExceptionHandlerTableEntry const* retVal = nullptr;
                for( auto const& handler : EhHandlers )
                {
                    if( handler.InTryBlock( ilOffset )
                     || handler.InHandlerBlock( ilOffset )
                      )
                    {
                        retVal = &handler;
                    }
                }
                return retVal;
            }

            IterableExceptionHandlers const& GetExceptionHandlers()
            {
                return EhHandlers;
            }
        private:
            MetadataPtr IlStart;
            MetadataPtr IlEnd;
            IterableExceptionHandlers EhHandlers;
            AssemblyHeader const& Header;
            MethodDefTableEntry const& MethodDef;
        };

        // Collection facade class template for accessing an
        // AssmblyHeader table, with support for indexing,
        // use in ranged for loops, and other standard C++
        // contexts.
        template<typename T>
        class AssemblyTable
        {
            static const TableKind Kind = table_kind<T>::value;
        public:
            AssemblyTable( AssemblyHeader const& header )
                : Header( header )
            { }

            T const* begin( ) { return reinterpret_cast<T const*>( Header.GetTable( Kind ) ); }
            T const* end( ) { return begin() + Header.TableElementCount( Kind ); }

            T const& operator[]( uint16_t index )
            {
                return begin()[ index ];
            }

            size_t length( )
            {
                return Header.TableElementCount( Kind );
            }

            AssemblyHeader const& Header;
        };

        template<typename T>
        inline AssemblyTable<T> AssemblyHeader::GetTable( ) const
        {
            return AssemblyTable<T>( *this );
        }

        inline bool AssemblyHeader::FindMethodBoundaries( MethodDefTableIndex i, MetadataOffset& start, MetadataOffset& end ) const
        {
            MethodDefTableEntry const& p = GetTable<MethodDefTableEntry>( )[ i ];
            return FindMethodBoundaries( p, i, start, end );
        }

    }
}
#endif