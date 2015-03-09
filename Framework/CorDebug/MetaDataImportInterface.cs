using System;
using System.Runtime.InteropServices;
using CorDebugInterop;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Debugger.MetaData
{
    public enum CorTokenType
    {
        mdtModule = 0x00000000,       //          
        mdtTypeRef = 0x01000000,       //          
        mdtTypeDef = 0x02000000,       //          
        mdtFieldDef = 0x04000000,       //           
        mdtMethodDef = 0x06000000,       //       
        mdtParamDef = 0x08000000,       //           
        mdtInterfaceImpl = 0x09000000,       //  
        mdtMemberRef = 0x0a000000,       //       
        mdtCustomAttribute = 0x0c000000,       //      
        mdtPermission = 0x0e000000,       //       
        mdtSignature = 0x11000000,       //       
        mdtEvent = 0x14000000,       //           
        mdtProperty = 0x17000000,       //           
        mdtModuleRef = 0x1a000000,       //       
        mdtTypeSpec = 0x1b000000,       //           
        mdtAssembly = 0x20000000,       //
        mdtAssemblyRef = 0x23000000,       //
        mdtFile = 0x26000000,       //
        mdtExportedType = 0x27000000,       //
        mdtManifestResource = 0x28000000,       //
        mdtGenericParam = 0x2a000000,       //
        mdtMethodSpec = 0x2b000000,       //
        mdtGenericParamConstraint = 0x2c000000,
        mdtString = 0x70000000,       //          
        mdtName = 0x71000000,       //
        mdtBaseType = 0x72000000,       // Leave this on the high end value. This does not correspond to metadata table
    }

    [Flags]
    public enum CorTypeAttr : uint
    {
        // Use this mask to retrieve the type visibility information.
        tdVisibilityMask = 0x00000007,
        tdNotPublic = 0x00000000,     // Class is not public scope.
        tdPublic = 0x00000001,     // Class is public scope.
        tdNestedPublic = 0x00000002,     // Class is nested with public visibility.
        tdNestedPrivate = 0x00000003,     // Class is nested with private visibility.
        tdNestedFamily = 0x00000004,     // Class is nested with family visibility.
        tdNestedAssembly = 0x00000005,     // Class is nested with assembly visibility.
        tdNestedFamANDAssem = 0x00000006,     // Class is nested with family and assembly visibility.
        tdNestedFamORAssem = 0x00000007,     // Class is nested with family or assembly visibility.
        // Use this mask to retrieve class layout information
        tdLayoutMask = 0x00000018,
        tdAutoLayout = 0x00000000,     // Class fields are auto-laid out
        tdSequentialLayout = 0x00000008,     // Class fields are laid out sequentially
        tdExplicitLayout = 0x00000010,     // Layout is supplied explicitly
        // end layout mask
        // Use this mask to retrieve class semantics information.
        tdClassSemanticsMask = 0x00000020,
        tdClass = 0x00000000,     // Type is a class.
        tdInterface = 0x00000020,     // Type is an interface.
        // end semantics mask
        // Special semantics in addition to class semantics.
        tdAbstract = 0x00000080,     // Class is abstract
        tdSealed = 0x00000100,     // Class is concrete and may not be extended
        tdSpecialName = 0x00000400,     // Class name is special.  Name describes how.
        // Implementation attributes.
        tdImport = 0x00001000,     // Class / interface is imported
        tdSerializable = 0x00002000,     // The class is Serializable.
        // Use tdStringFormatMask to retrieve string information for native interop
        tdStringFormatMask = 0x00030000,
        tdAnsiClass = 0x00000000,     // LPTSTR is interpreted as ANSI in this class
        tdUnicodeClass = 0x00010000,     // LPTSTR is interpreted as UNICODE
        tdAutoClass = 0x00020000,     // LPTSTR is interpreted automatically
        // end string format mask
        tdBeforeFieldInit = 0x00100000,     // Initialize the class any time before first static field access.
        // Flags reserved for runtime use.
        tdReservedMask = 0x00040800,
        tdRTSpecialName = 0x00000800,     // Runtime should check name encoding.
        tdHasSecurity = 0x00040000,     // Class has security associate with it.
    }

    [Flags]
    public enum CorFieldAttr : int
    {
        // member access mask - Use this mask to retrieve accessibility information.
        fdFieldAccessMask = 0x0007,
        fdPrivateScope = 0x0000,     // Member not referenceable.
        fdPrivate = 0x0001,     // Accessible only by the parent type.  
        fdFamANDAssem = 0x0002,     // Accessible by sub-types only in this Assembly.
        fdAssembly = 0x0003,     // Accessibly by anyone in the Assembly.
        fdFamily = 0x0004,     // Accessible only by type and sub-types.    
        fdFamORAssem = 0x0005,     // Accessibly by sub-types anywhere, plus anyone in assembly.
        fdPublic = 0x0006,     // Accessibly by anyone who has visibility to this scope.    
        // end member access mask
        // field contract attributes.
        fdStatic = 0x0010,     // Defined on type, else per instance.
        fdInitOnly = 0x0020,     // Field may only be initialized, not written to after init.
        fdLiteral = 0x0040,     // Value is compile time constant.
        fdNotSerialized = 0x0080,     // Field does not have to be serialized when type is remoted.
        fdSpecialName = 0x0200,     // field is special.  Name describes how.
        // interop attributes
        fdPinvokeImpl = 0x2000,     // Implementation is forwarded through pinvoke.
        // Reserved flags for runtime use only.
        fdReservedMask = 0x9500,
        fdRTSpecialName = 0x0400,     // Runtime(metadata internal APIs) should check name encoding.
        fdHasFieldMarshal = 0x1000,     // Field has marshalling information.
        fdHasDefault = 0x8000,     // Field has default.
        fdHasFieldRVA = 0x0100,     // Field has RVA.
    }

    // MethodImpl attr bits, used by DefineMethodImpl.
    [Flags]
    public enum CorMethodImpl : int
    {
        // code impl mask
        miCodeTypeMask = 0x0003,   // Flags about code type.   
        miIL = 0x0000,   // Method impl is IL.   
        miNative = 0x0001,   // Method impl is native.     
        miOPTIL = 0x0002,   // Method impl is OPTIL 
        miRuntime = 0x0003,   // Method impl is provided by the runtime.
        // end code impl mask
        // managed mask
        miManagedMask = 0x0004,   // Flags specifying whether the code is managed or unmanaged.
        miUnmanaged = 0x0004,   // Method impl is unmanaged, otherwise managed.
        miManaged = 0x0000,   // Method impl is managed.
        // end managed mask
        // implementation info and interop
        miForwardRef = 0x0010,   // Indicates method is defined; used primarily in merge scenarios.
        miPreserveSig = 0x0080,   // Indicates method sig is not to be mangled to do HRESULT conversion.
        miInternalCall = 0x1000,   // Reserved for internal use.
        miSynchronized = 0x0020,   // Method is single threaded through the body.
        miNoInlining = 0x0008,   // Method may not be inlined.                                      
        miMustRun = 0x0040,   // Method is prepared such that execution will not fail at the infrastructue level.
        miMaxMethodImplVal = 0xffff,   // Range check value    
    }

    //
    // Open bits.
    //
    [Flags]
    public enum CorOpenFlags : uint
    {
        ofRead = 0x00000000,     // Open scope for read
        ofWrite = 0x00000001,     // Open scope for write.
        ofReadWriteMask = 0x00000001,     // Mask for read/write bit.
        ofCopyMemory = 0x00000002,     // Open scope with memory. Ask metadata to maintain its own copy of memory.
        // These are obsolete and are ignored.
        ofCacheImage = 0x00000004,     // EE maps but does not do relocations or verify image
        ofNoTypeLib = 0x00000080,     // Don't OpenScope on a typelib.
        // Internal bits
        ofReserved1 = 0x00000100,     // Reserved for internal use.
        ofReserved2 = 0x00000200,     // Reserved for internal use.
        ofReserved = 0xffffff78      // All the reserved bits.
    }

    [Flags]
    public enum CorCallingConvention : byte
    {
        IMAGE_CEE_CS_CALLCONV_DEFAULT = 0x0,
        IMAGE_CEE_CS_CALLCONV_VARARG = 0x5,
        IMAGE_CEE_CS_CALLCONV_FIELD = 0x6,
        IMAGE_CEE_CS_CALLCONV_LOCAL_SIG = 0x7,
        IMAGE_CEE_CS_CALLCONV_PROPERTY = 0x8,
        IMAGE_CEE_CS_CALLCONV_UNMGD = 0x9,
        IMAGE_CEE_CS_CALLCONV_GENERICINST = 0x0a,  // generic method instantiation
        IMAGE_CEE_CS_CALLCONV_MAX = 0x0b,  // first invalid calling convention    
        // The high bits of the calling convention convey additional info   
        IMAGE_CEE_CS_CALLCONV_MASK = 0x0f,  // Calling convention is bottom 4 bits 
        IMAGE_CEE_CS_CALLCONV_HASTHIS = 0x20,  // Top bit indicates a 'this' parameter    
        IMAGE_CEE_CS_CALLCONV_EXPLICITTHIS = 0x40,  // This parameter is explicitly in the signature
        IMAGE_CEE_CS_CALLCONV_GENERIC = 0x10,  // Generic method sig with explicit number of type arguments (precedes ordinary parameter count)
    }

    // MethodDef attr bits, Used by DefineMethod.
    [Flags]
    public enum CorMethodAttr : uint
    {
        // member access mask - Use this mask to retrieve accessibility information.
        mdMemberAccessMask = 0x0007,
        mdPrivateScope = 0x0000,     // Member not referenceable.
        mdPrivate = 0x0001,     // Accessible only by the parent type.  
        mdFamANDAssem = 0x0002,     // Accessible by sub-types only in this Assembly.
        mdAssem = 0x0003,     // Accessibly by anyone in the Assembly.
        mdFamily = 0x0004,     // Accessible only by type and sub-types.    
        mdFamORAssem = 0x0005,     // Accessibly by sub-types anywhere, plus anyone in assembly.
        mdPublic = 0x0006,     // Accessibly by anyone who has visibility to this scope.    
        // end member access mask
        // method contract attributes.
        mdStatic = 0x0010,     // Defined on type, else per instance.
        mdFinal = 0x0020,     // Method may not be overridden.
        mdVirtual = 0x0040,     // Method virtual.
        mdHideBySig = 0x0080,     // Method hides by name+sig, else just by name.
        // vtable layout mask - Use this mask to retrieve vtable attributes.
        mdVtableLayoutMask = 0x0100,
        mdReuseSlot = 0x0000,     // The default.
        mdNewSlot = 0x0100,     // Method always gets a new slot in the vtable.
        // end vtable layout mask
        // method implementation attributes.
        mdCheckAccessOnOverride = 0x0200,     // Overridability is the same as the visibility.
        mdAbstract = 0x0400,     // Method does not provide an implementation.
        mdSpecialName = 0x0800,     // Method is special.  Name describes how.
        // interop attributes
        mdPinvokeImpl = 0x2000,     // Implementation is forwarded through pinvoke.
        mdUnmanagedExport = 0x0008,     // Managed method exported via thunk to unmanaged code.
        // Reserved flags for runtime use only.
        mdReservedMask = 0xd000,
        mdRTSpecialName = 0x1000,     // Runtime should check name encoding.
        mdHasSecurity = 0x4000,     // Method has security associate with it.
        mdRequireSecObject = 0x8000,     // Method calls another method containing security code.
    }

    [ComImport()]
    [Guid("E5CB7A31-7512-11d2-89CE-0080C792E5D8")]
    public class CorMetaDataDispenser
    {
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3")] //really IMetaDataDispenserEx
    public interface IMetaDataDispenser
    {
        void DefineScope(ref Guid rclsid, int dwCreateFlags, ref Guid riid, out object ppUnk);
        void OpenScope([MarshalAs(UnmanagedType.LPWStr)]/* [ComAliasName ("Microsoft.VisualStudio.OLE.Interop.LPCWSTR")]*/string szScope, int dwOpenFlags, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppIUnk);
        void OpenScopeOnMemory(object pData, uint cbData, int dwOpenFlags, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppIUnk);

        //IMetaDataDispenserEx...
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
    [ComVisible(true)]
    public interface IMetaDataImport
    {
        //I make no claims on this interop
        //ref v. out params
        //how they are used
        //arrays into IntPtr
        //which are optional ref parameters???
        //STDMETHOD_(void, CloseEnum)
        [MethodImpl(MethodImplOptions.PreserveSig)]
        void CloseEnum(IntPtr hEnum
                        );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int CountEnum(IntPtr hEnum,
                       IntPtr pulCount
                       );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int ResetEnum(IntPtr hEnum,
                       uint ulPos
                       );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumTypeDefs(IntPtr phEnum,
                          IntPtr rTypeDefs,
                          uint cMax,
                          IntPtr pcTypeDefs
                          );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumInterfaceImpls(IntPtr phEnum,
                                uint td,
                                IntPtr rImpls,
                                uint cMax,
                                IntPtr pcImpls
                                );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumTypeRefs(IntPtr phEnum,
                          IntPtr rTypeRefs,
                          uint cMax,
                          IntPtr pcTypeRefs
                          );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int FindTypeDefByName(
                              [MarshalAs(UnmanagedType.LPWStr)]
                               string szTypeDef,      // [IN] Name of the Type.
                               uint tkEnclosingClass, // [IN] TypeDef/TypeRef for Enclosing class.
                               IntPtr mdTypeDef       // [OUT] Put the TypeDef token here.
                               );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetScopeProps(IntPtr szName,            // [OUT] Put the name here.
                           uint cchName,            // [IN] Size of name buffer in wide chars.
                           IntPtr pchName,          // [OUT] Put size of name (wide chars) here.
                           IntPtr pmvid             // [OUT, OPTIONAL] Put MVID here.        
                           );

        // S_OK.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetModuleFromScope(
                            IntPtr pmd             // [OUT] Put mdModule token here.        
                                );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetTypeDefProps(
                             uint td,                       // [IN] TypeDef token for inquiry.      
                             IntPtr szTypeDef,              // [OUT] Put name here.        
                             uint cchTypeDef,               // [IN] size of name buffer in wide chars.        
                             IntPtr pchTypeDef,             // [OUT] put size of name (wide chars) here.
                             IntPtr pdwTypeDefFlags,        // [OUT] Put flags here.
                             IntPtr ptkExtends              // [OUT] Put base class TypeDef/TypeRef here.
                             );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetInterfaceImplProps(uint iiImpl,                 // [IN] InterfaceImpl token.
                                   IntPtr pClass,              // [OUT] Put implementing class token here.
                                   IntPtr ptkIface             // [OUT] Put implemented interface token here.
                                   );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetTypeRefProps(uint tr,                         // [IN] TypeRef token.
                             IntPtr ptkResolutionScope,      // [OUT] Resolution scope, ModuleRef or AssemblyRef.
                             IntPtr szName,                  // [OUT] Name of the TypeRef.
                             uint cchName,                   // [IN] Size of buffer.
                             IntPtr pchName                  // [OUT] Size of Name.
                             );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int ResolveTypeRef(uint tr,
                            IntPtr riid,
                            [MarshalAs(UnmanagedType.IUnknown)] 
                            ref object ppIScope,
                            IntPtr ptd
                            );

        // S_OK, S_FALSE, or error.        
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumMembers(IntPtr phEnum,             // [IN|OUT] Pointer to the enum.
                         uint cl,                  // [IN] TypeDef to scope the enumeration.
                         IntPtr rMembers,          // [OUT] Put MemberDefs here.
                         uint cMax,                // [IN] Max MemberDefs to put.
                         IntPtr pcTokens           // [OUT] Put # put here.
                         );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumMembersWithName(IntPtr phEnum,                      // [IN|OUT] Pointer to the enum.
                                 uint cl,                           // [IN] TypeDef to scope the enumeration.
                                 [MarshalAs(UnmanagedType.LPWStr)]
                                 string szName,                     // [IN] Limit results to those with this name.
                                 IntPtr rMembers,                   // [OUT] Put MemberDefs here.
                                 uint cMax,                         // [IN] Max MemberDefs to put.
                                 IntPtr pcTokens                    // [OUT] Put # put here.
                                 );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumMethods(IntPtr phEnum,               // [IN|OUT] Pointer to the enum.
                         uint cl,                    // [IN] TypeDef to scope the enumeration.
                         IntPtr rMethods,            // [OUT] Put MethodDefs here.
                         uint cMax,                  // [IN] Max MethodDefs to put.
                         IntPtr pcTokens             // [OUT] Put # put here.
                         );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumMethodsWithName(IntPtr phEnum,                // [IN|OUT] Pointer to the enum.
                                 uint cl,                     // [IN] TypeDef to scope the enumeration.
                                 [MarshalAs(UnmanagedType.LPWStr)]
                                 string szName,               // [IN] Limit results to those with this name.
                                 IntPtr rMethods,             // [OU] Put MethodDefs here.
                                 uint cMax,                   // [IN] Max MethodDefs to put.
                                 IntPtr pcTokens              // [OUT] Put # put here.
                                 );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumFields(IntPtr phEnum,             // [IN|OUT] Pointer to the enum.
                        uint cl,                  // [IN] TypeDef to scope the enumeration.
                        IntPtr rFields,           // [OUT] Put FieldDefs here.
                        uint cMax,                // [IN] Max FieldDefs to put.
                        IntPtr pcTokens           // [OUT] Put # put here.
                        );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumFieldsWithName(IntPtr phEnum,                 // [IN|OUT] Pointer to the enum.
                                uint cl,                      // [IN] TypeDef to scope the enumeration.
                                [MarshalAs(UnmanagedType.LPWStr)]
                                string szName,                // [IN] Limit results to those with this name.
                                IntPtr rFields,               // [OUT] Put MemberDefs here.
                                uint cMax,                    // [IN] Max MemberDefs to put.
                                IntPtr pcTokens               // [OUT] Put # put here.
                                );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumParams(IntPtr phEnum,           // [IN|OUT] Pointer to the enum.
                        uint mb,                // [IN] MethodDef to scope the enumeration.
                        IntPtr rParams,         // [OUT] Put ParamDefs here.
                        uint cMax,              // [IN] Max ParamDefs to put.
                        IntPtr pcTokens         // [OUT] Put # put here.
                        );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumMemberRefs(IntPtr phEnum,                // [IN|OUT] Pointer to the enum.
                            uint tkParent,               // [IN] Parent token to scope the enumeration.
                            IntPtr rMemberRefs,          // [OUT] Put MemberRefs here.
                            uint cMax,                   // [IN] Max MemberRefs to put.
                            IntPtr pcTokens              // [OUT] Put # put here.
                            );

        // S_OK, S_FALSE, or error
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumMethodImpls(IntPtr phEnum,             // [IN|OUT] Pointer to the enum.
                             uint td,                  // [IN] TypeDef to scope the enumeration.
                             IntPtr rMethodBody,       // [OUT] Put Method Body tokens here.
                             IntPtr rMethodDecl,       // [OUT] Put Method Declaration tokens here.
                             uint cMax,                // [IN] Max tokens to put.
                             IntPtr pcTokens           // [OUT] Put # put here.
                             );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumPermissionSets(IntPtr phEnum,              // [IN|OUT] Pointer to the enum.
                                uint tk,                   // [IN] if !NIL, token to scope the enumeration.
                                int dwActions,             // [IN] if !0, return only these actions.
                                IntPtr rPermission,        // [OUT] Put Permissions here.
                                uint cMax,                 // [IN] Max Permissions to put.
                                IntPtr pcTokens            // [OUT] Put # put here.
                                );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int FindMember(uint td,                     // [IN] given typedef    
                        [MarshalAs(UnmanagedType.LPWStr)]
                        string szName,              // [IN] member name
                        IntPtr pvSigBlob,           // [IN] point to a blob value of CLR signature
                        uint cbSigBlob,             // [IN] count of bytes in the signature blob
                        IntPtr pmb                  // [OUT] matching memberdef
                        );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int FindMethod(uint td,                    // [IN] given typedef
                        [MarshalAs(UnmanagedType.LPWStr)]
                        string szName,             // [IN] member name
                        IntPtr pvSigBlog,          // [IN] point to a blob value of CLR signature
                        uint cbSigBlob,            // [IN] count of bytes in the signature blob
                        IntPtr pmb                 // [OUT] matching memberdef
                        );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int FindField(uint td,                     // [IN] given typedef
                       [MarshalAs(UnmanagedType.LPWStr)]
                       string szName,              // [IN] member name
                       IntPtr pvSigBlog,           // [IN] point to a blob value of CLR signature
                       uint cbSigBlob,             // [IN] count of bytes in the signature blob
                       IntPtr pmb                  // [OUT] matching memberdef
                       );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int FindMemberRef(uint td,                   // [IN] given typeRef
                           [MarshalAs(UnmanagedType.LPWStr)]
                           string szName,            // [IN] member name
                           IntPtr pvSigBlog,         // [IN] point to a blob value of CLR signature
                           uint cbSigBlob,           // [IN] count of bytes in the signature blob
                           IntPtr pmr                // [OUT] matching memberref
                           );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetMethodProps(uint mb,                 // The method for which to get props.
                            IntPtr pClass,          // Put method's class here.
                            IntPtr szMethod,        // Put method's name here.
                            uint cchMethod,         // Size of szMethod buffer in wide chars.
                            IntPtr pchMethod,       // Put actual size here
                            IntPtr pdwAttr,         // Put flags here.
                            IntPtr ppvSigBlob,      // [OUT] point to the blob value of meta data
                            IntPtr pcbSigBlob,      // [OUT] actual size of signature blob
                            IntPtr pulCodeRVA,      // [OUT] codeRVA
                            IntPtr pdwImplFlags     // [OUT] Impl. Flags
                            );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetMemberRefProps(uint mr,                   // [IN] given memberref
                               IntPtr ptk,               // [OUT] Put classref or classdef here.
                               IntPtr szMember,          // [OUT] buffer to fill for member's name
                               uint cchMember,           // [IN] the count of char of szMember
                               IntPtr pchMember,         // [OUT] actual count of char in member name
                               IntPtr ppvSigBlob,        // [OUT] point to meta data blob value
                               IntPtr pcbSigBlob         // [OUT] actual size of signature blob
                               );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumProperties(IntPtr phEnum,                // [IN|OUT] Pointer to the enum.
                            uint td,                     // [IN] TypeDef to scope the enumeration.
                            IntPtr rProperties,          // [OUT] Put Properties here.
                            uint cMax,                   // [IN] Max properties to put.
                            IntPtr pcProperties          // [OUT] Put # put here.
                            );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumEvents(IntPtr phEnum,             // [IN|OUT] Pointer to the enum.    
                        uint td,                  // [IN] TypeDef to scope the enumeration.
                        IntPtr rEvents,           // [OUT] Put events here.
                        uint cMax,                // [IN] Max events to put.
                        IntPtr pcEvents           // [OUT] Put # put here.
                        );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetEventProps(uint ev,                       // [IN] event token
                           IntPtr pClass,                // [OUT] typedef containing the event declarion.
                           IntPtr szEvent,               // [OUT] Event name
                           uint cchEvent,                // [IN] the count of wchar of szEvent
                           IntPtr pchEvent,              // [OUT] actual count of wchar for event's name
                           IntPtr pdwEventFlags,         // [OUT] Event flags. 
                           IntPtr ptkEventType,          // [OUT] EventType class
                           IntPtr pmdAddOn,              // [OUT] AddOn method of the event
                           IntPtr pmdRemoveOn,           // [OUT] RemoveOn method of the event
                           IntPtr pmdFire,               // [OUT] Fire method of the event
                           IntPtr rmdOtherMethod,        // [OUT] other method of the event
                           uint cMax,                    // [IN] size of rmdOtherMethod
                           IntPtr pcOtherMethod          // [OUT] total number of other method of this event
                           );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumMethodSemantics(IntPtr phEnum,              // [IN|OUT] Pointer to the enum.
                                 uint mb,                   // [IN] MethodDef to scope the enumeration.
                                 IntPtr rEventProp,         // [OUT] Put Event/Property here.
                                 uint cMax,                 // [IN] Max properties to put.
                                 IntPtr pcEventProp         // [OUT] Put # put here.
                                 );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetMethodSemantics(uint mb,                       // [IN] method token
                                uint tkEventProp,             // [IN] event/property token.
                                IntPtr pdwSemanticFlags       // [OUT] the role flags for the method/propevent pair
                                );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetClassLayout(uint td,                     // [IN] give typedef
                            IntPtr pdwPackSize,         // [OUT] 1, 2, 4, 8, or 16
                            IntPtr rFieldOffset,        // [OUT] field offset array
                            uint cMax,                  // [IN] size of the array
                            IntPtr pcFieldOffset,       // [OUT] needed array size
                            IntPtr pulClassSize         // [OUT] the size of the class
                            );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetFieldMarshal(uint tk,                    // [IN] given a field's memberdef
                             IntPtr ppvNativeType,      // [OUT] native type of this field
                             IntPtr pcbNativeType       // [OUT] the count of bytes of *ppvNativeType
                             );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetRVA(uint tk,                       // Member for which to set offset
                    IntPtr pulCodeRVA,            // The offset
                    IntPtr pdwImplFlags           // the implementation flags
        );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetPermissionSetProps(uint pm,                        // [IN] the permission token.
                                   IntPtr pdwAction,              // [OUT] CorDeclSecurity.
                                   IntPtr ppvPermission,          // [OUT] permission blob.
                                   IntPtr pcbPermission           // [OUT] count of bytes of pvPermission.
                                   );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetSigFromToken(uint mdSig,             // [IN] Signature token.
                             IntPtr ppvSig,         // [OUT] return pointer to token.
                             IntPtr pcbSig          // [OUT] return size of signature.
                             );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetModuleRefProps(uint mur,                  // [IN] moduleref token.
                               IntPtr szName,            // [OUT] buffer to fill with the moduleref name.
                               uint cchName,             // [IN] size of szName in wide characters.
                               IntPtr pchName            // [OUT] actual count of characters in the name.
                               );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumModuleRefs(IntPtr phEnum,               // [IN|OUT] pointer to the enum.
                            IntPtr rModuleRefs,         // [OUT] put modulerefs here.
                            uint cmax,                  // [IN] max memberrefs to put.
                            IntPtr pcModuleRefs         // [OUT] put # put here.
                            );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetTypeSpecFromToken(uint typespec,         // [IN] TypeSpec token.
                                  IntPtr ppvSig,        // [OUT] return pointer to TypeSpec signature
                                  IntPtr pcbSig         // [OUT] return size of signature.
                                  );

        // Not Recommended! May be removed!
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetNameFromToken(uint tk,                    // [IN] Token to get name from.  Must have a name.
                              IntPtr pszUtf8NamePtr      // [OUT] Return pointer to UTF8 name in heap.
                              );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumUnresolvedMethods(IntPtr phEnum,               // [IN|OUT] Pointer to the enum.
                                   IntPtr rMethods,            // [OUT] Put MemberDefs here.
                                   uint cMax,                  // [IN] Max MemberDefs to put.
                                   IntPtr pcTokens             // [OUT] Put # put here.
                                   );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetUserString(uint stk,                    // [IN] String token.
                           IntPtr szString,            // [OUT] Copy of string.
                           uint cchString,             // [IN] Max chars of room in szString.
                           IntPtr pchString            // [OUT] How many chars in actual string.
                           );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetPinvokeMap(uint tk,                         // [IN] FieldDef or MethodDef.
                           IntPtr pdwMappingFlags,         // [OUT] Flags used for mapping.
                           IntPtr szImportName,            // [OUT] Import name.
                           uint cchImportName,             // [IN] Size of the name buffer.
                           IntPtr pchImportName,           // [OUT] Actual number of characters stored.
                           IntPtr pmrImportDLL             // [OUT] ModuleRef token for the target DLL.
                           );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumSignatures(IntPtr phEnum,                    // [IN|OUT] pointer to the enum.
                            IntPtr rSignatures,              // [OUT] put signatures here.
                            uint cmax,                       // [IN] max signatures to put.
                            IntPtr pcSignatures              // [OUT] put # put here.
                            );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumTypeSpecs(IntPtr phEnum,                    // [IN|OUT] pointer to the enum.
                           IntPtr rTypeSpecs,               // [OUT] put TypeSpecs here.
                           uint cmax,                       // [IN] max TypeSpecs to put.
                           IntPtr pcTypeSpecs               // [OUT] put # put here.
                           );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumUserStrings(IntPtr phEnum,                 // [IN/OUT] pointer to the enum.
                             IntPtr rStrings,              // [OUT] put Strings here.
                             uint cmax,                    // [IN] max Strings to put.
                             IntPtr pcStrings              // [OUT] put # put here.
                             );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetParamForMethodIndex(uint md,                     // [IN] Method token.
                                    uint ulParamSeq,            // [IN] Parameter sequence.
                                    IntPtr ppd                  // [IN] Put Param token here.
                                    );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumUserStrings(IntPtr phEnum,                   // [IN, OUT] COR enumerator.
                             uint tk,                        // [IN] Token to scope the enumeration, 0 for all.
                             uint tkType,                    // [IN] Type of interest, 0 for all.
                             IntPtr rCustomAttributes,       // [OUT] Put custom attribute tokens here.
                             uint cMax,                      // [IN] Size of rCustomAttributes.
                             IntPtr pcCustomAttributes       // [OUT, OPTIONAL] Put count of token values here.
                             );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetCustomAttributeProps(uint cv,                       // [IN] CustomAttribute token.
                                     IntPtr ptkObj,                // [OUT, OPTIONAL] Put object token here.
                                     IntPtr ptkType,               // [OUT, OPTIONAL] Put AttrType token here.
                                     IntPtr ppBlob,                // [OUT, OPTIONAL] Put pointer to data here.
                                     IntPtr pcbSize                // [OUT, OPTIONAL] Put size of date here.
                                     );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int FindTypeRef(uint tkResolutionScope,                      // [IN] ModuleRef, AssemblyRef or TypeRef.
                         [MarshalAs(UnmanagedType.LPWStr)]
                         string szName,                              // [IN] TypeRef Name.
                         IntPtr ptr                                  // [OUT] matching TypeRef.
                         );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetMemberProps(uint mb,                           // The member for which to get props.
                            IntPtr pClass,                    // Put member's class here.
                            IntPtr szMember,                  // Put member's name here.
                            uint cchMember,                   // Size of szMember buffer in wide chars.
                            IntPtr pchMember,                 // Put actual size here
                            IntPtr pdwAttr,                   // Put flags here.
                            IntPtr ppvSigBlob,                // [OUT] point to the blob value of meta data
                            IntPtr pcbSigBlob,                // [OUT] actual size of signature blob
                            IntPtr pulCodeRVA,                // [OUT] codeRVA
                            IntPtr pdwImplFlags,              // [OUT] Impl. Flags
                            IntPtr pdwCPlusTypeFlag,          // [OUT] flag for value type. selected ELEMENT_TYPE_*
                            IntPtr ppValue,                   // [OUT] constant value
                            IntPtr pcchValue                  // [OUT] size of constant string in chars, 0 for non-strings.
                            );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetFieldProps(uint mb,                       // The field for which to get props.
                           IntPtr pClass,                // Put field's class here.
                           IntPtr szField,               // Put field's name here.
                           uint cchField,                // Size of szField buffer in wide chars.
                           IntPtr pchField,              // Put actual size here
                           IntPtr pdwAttr,               // Put flags here.
                           IntPtr ppvSigBlob,            // [OUT] point to the blob value of meta data
                           IntPtr pcbSigBlob,            // [OUT] actual size of signature blob
                           IntPtr pdwCPlusTypeFlag,      // [OUT] flag for value type. selected ELEMENT_TYPE_*
                           IntPtr ppValue,               // [OUT] constant value
                           IntPtr pcchValue              // [OUT] size of constant string in chars, 0 for non-strings.
                           );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetPropertyProps(uint prop,                            // [IN] property token
                              IntPtr pClass,                       // [OUT] typedef containing the property declarion.
                              IntPtr szProperty,                   // [OUT] Property name
                              uint cchProperty,                    // [IN] the count of wchar of szProperty
                              IntPtr pchProperty,                  // [OUT] actual count of wchar for property name
                              IntPtr pdwPropFlags,                 // [OUT] property flags.
                              IntPtr ppvSig,                       // [OUT] property type. pointing to meta data internal blob
                              IntPtr pbSig,                        // [OUT] count of bytes in *ppvSig
                              IntPtr pdwCPlusTypeFlag,             // [OUT] flag for value type. selected ELEMENT_TYPE_*
                              IntPtr ppDefaultValue,               // [OUT] constant value
                              IntPtr pcchDefaultValue,             // [OUT] size of constant string in chars, 0 for non-strings.
                              IntPtr pmdSetter,                    // [OUT] setter method of the property
                              IntPtr pmdGetter,                    // [OUT] getter method of the property
                              IntPtr rmdOtherMethod,               // [OUT] other method of the property
                              uint cMax,                           // [IN] size of rmdOtherMethod
                              IntPtr pcOtherMethod                 // [OUT] total number of other method of this property
                              );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetParamProps(uint tk,                          // [IN]The Parameter.
                           IntPtr pmd,                      // [OUT] Parent Method token.
                           IntPtr pulSequence,              // [OUT] Parameter sequence.
                           IntPtr szName,                   // [OUT] Put name here.
                           uint cchName,                    // [OUT] Size of name buffer.
                           IntPtr pchName,                  // [OUT] Put actual size of name here.
                           IntPtr pdwAttr,                  // [OUT] Put flags here.
                           IntPtr pdwCPlusTypeFlag,         // [OUT] Flag for value type. selected ELEMENT_TYPE_*.
                           IntPtr ppValue,                  // [OUT] Constant value.
                           IntPtr pcchValue                 // [OUT] size of constant string in chars, 0 for non-strings.
                           );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetCustomAttributeByName(uint tkObj,               // [IN] Object with Custom Attribute.
                                      [MarshalAs(UnmanagedType.LPWStr)]
            string szName,                                     // [IN] Name of desired Custom Attribute.
                                      IntPtr ppData,           // [OUT] Put pointer to data here.
                                      IntPtr pcbData           // [OUT] Put size of data here.
                                      );

        // True or False.   
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int IsValidToken(uint tk                               // [IN] Given token.
                          );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetNestedClassProps(uint tdNestedClass,            // [IN] NestedClass token.
                                 IntPtr ptdEnclosingClass      // [OUT] EnclosingClass token.
                                 );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetNativeCallConvFromSig(IntPtr pvSig,                     // [IN] Pointer to signature.
                                      uint cbSig,                      // [IN] Count of signature bytes.
                                      IntPtr pCallConv                 // [OUT] Put calling conv here (see CorPinvokemap).
                                      );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int IsGlobal(uint pd,                             // [IN] Type, Field, or Method token.
                      IntPtr pbGlobal                     // [OUT] Put 1 if global, 0 otherwise.
                      );
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("FCE5EFA0-8BBA-4f8e-A036-8F2022B08466")]
    [ComVisible(true)]
    public interface IMetaDataImport2 : IMetaDataImport
    {
        #region IMetaDataImport
        //I make no claims on this interop
        //ref v. out params
        //how they are used
        //arrays into IntPtr
        //which are optional ref parameters???
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new void CloseEnum(IntPtr hEnum
                        );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int CountEnum(IntPtr hEnum,
                           IntPtr pulCount
                       );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int ResetEnum(IntPtr hEnum,
                           uint ulPos
                       );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumTypeDefs(IntPtr phEnum,
                              IntPtr rTypeDefs,
                          uint cMax,
                          IntPtr pcTypeDefs
                          );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumInterfaceImpls(IntPtr phEnum,
                                    uint td,
                                IntPtr rImpls,
                                uint cMax,
                                IntPtr pcImpls
                                );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumTypeRefs(IntPtr phEnum,
                              IntPtr rTypeRefs,
                          uint cMax,
                          IntPtr pcTypeRefs
                          );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int FindTypeDefByName([MarshalAs(UnmanagedType.LPWStr)]
                               string szTypeDef,          // [IN] Name of the Type.
                              uint tkEnclosingClass,      // [IN] TypeDef/TypeRef for Enclosing class.
                              IntPtr mdTypeDef            // [OUT] Put the TypeDef token here.
                              );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetScopeProps(IntPtr szName,               // [OUT] Put the name here.
                               uint cchName,               // [IN] Size of name buffer in wide chars.
                           IntPtr pchName,                 // [OUT] Put size of name (wide chars) here.
                           IntPtr pmvid                    // [OUT, OPTIONAL] Put MVID here.        
                           );

        // S_OK.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetModuleFromScope(IntPtr pmd              // [OUT] Put mdModule token here.        
                                    );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetTypeDefProps(uint td,                        // [IN] TypeDef token for inquiry.      
                                 IntPtr szTypeDef,              // [OUT] Put name here.        
                             uint cchTypeDef,                   // [IN] size of name buffer in wide chars.        
                             IntPtr pchTypeDef,                 // [OUT] put size of name (wide chars) here.
                             IntPtr pdwTypeDefFlags,            // [OUT] Put flags here.
                             IntPtr ptkExtends                  // [OUT] Put base class TypeDef/TypeRef here.
                             );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetInterfaceImplProps(uint iiImpl,               // [IN] InterfaceImpl token.
                                       IntPtr pClass,            // [OUT] Put implementing class token here.
                                   IntPtr ptkIface               // [OUT] Put implemented interface token here.
                                   );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetTypeRefProps(uint tr,                           // [IN] TypeRef token.
                                 IntPtr ptkResolutionScope,        // [OUT] Resolution scope, ModuleRef or AssemblyRef.
                             IntPtr szName,                        // [OUT] Name of the TypeRef.
                             uint cchName,                         // [IN] Size of buffer.
                             IntPtr pchName                        // [OUT] Size of Name.
                             );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int ResolveTypeRef(uint tr,
                                IntPtr riid,
                            [MarshalAs(UnmanagedType.IUnknown)] 
                            ref object ppIScope,
                            IntPtr ptd
                            );

        // S_OK, S_FALSE, or error.        
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumMembers(IntPtr phEnum,              // [IN|OUT] Pointer to the enum.
                             uint cl,                   // [IN] TypeDef to scope the enumeration.
                         IntPtr rMembers,               // [OUT] Put MemberDefs here.
                         uint cMax,                     // [IN] Max MemberDefs to put.
                         IntPtr pcTokens                // [OUT] Put # put here.
                         );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumMembersWithName(IntPtr phEnum,              // [IN|OUT] Pointer to the enum.
                                     uint cl,                   // [IN] TypeDef to scope the enumeration.
                                 [MarshalAs(UnmanagedType.LPWStr)]
                                 string szName,                  // [IN] Limit results to those with this name.
                                 IntPtr rMembers,                // [OUT] Put MemberDefs here.
                                 uint cMax,                      // [IN] Max MemberDefs to put.
                                 IntPtr pcTokens                 // [OUT] Put # put here.
                                 );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumMethods(IntPtr phEnum,         // [IN|OUT] Pointer to the enum.
                             uint cl,              // [IN] TypeDef to scope the enumeration.
                         IntPtr rMethods,          // [OUT] Put MethodDefs here.
                         uint cMax,                // [IN] Max MethodDefs to put.
                         IntPtr pcTokens           // [OUT] Put # put here.
                         );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumMethodsWithName(IntPtr phEnum,                    // [IN|OUT] Pointer to the enum.
                                     uint cl,                         // [IN] TypeDef to scope the enumeration.
                                 [MarshalAs(UnmanagedType.LPWStr)]
                                 string szName,                       // [IN] Limit results to those with this name.
                                 IntPtr rMethods,                     // [OU] Put MethodDefs here.
                                 uint cMax,                           // [IN] Max MethodDefs to put.
                                 IntPtr pcTokens                      // [OUT] Put # put here.
                                 );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumFields(IntPtr phEnum,             // [IN|OUT] Pointer to the enum.
                            uint cl,                  // [IN] TypeDef to scope the enumeration.
                        IntPtr rFields,               // [OUT] Put FieldDefs here.
                        uint cMax,                    // [IN] Max FieldDefs to put.
                        IntPtr pcTokens               // [OUT] Put # put here.
                        );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumFieldsWithName(IntPtr phEnum,          // [IN|OUT] Pointer to the enum.
                                    uint cl,               // [IN] TypeDef to scope the enumeration.
                                [MarshalAs(UnmanagedType.LPWStr)]
                                string szName,             // [IN] Limit results to those with this name.
                                IntPtr rFields,            // [OUT] Put MemberDefs here.
                                uint cMax,                 // [IN] Max MemberDefs to put.
                                IntPtr pcTokens            // [OUT] Put # put here.
                                );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumParams(IntPtr phEnum,            // [IN|OUT] Pointer to the enum.
                            uint mb,                 // [IN] MethodDef to scope the enumeration.
                        IntPtr rParams,              // [OUT] Put ParamDefs here.
                        uint cMax,                   // [IN] Max ParamDefs to put.
                        IntPtr pcTokens              // [OUT] Put # put here.
                        );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumMemberRefs(IntPtr phEnum,           // [IN|OUT] Pointer to the enum.
                                uint tkParent,          // [IN] Parent token to scope the enumeration.
                            IntPtr rMemberRefs,         // [OUT] Put MemberRefs here.
                            uint cMax,                  // [IN] Max MemberRefs to put.
                            IntPtr pcTokens             // [OUT] Put # put here.
                            );

        // S_OK, S_FALSE, or error
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumMethodImpls(IntPtr phEnum,             // [IN|OUT] Pointer to the enum.
                                 uint td,                  // [IN] TypeDef to scope the enumeration.
                             IntPtr rMethodBody,           // [OUT] Put Method Body tokens here.
                             IntPtr rMethodDecl,           // [OUT] Put Method Declaration tokens here.
                             uint cMax,                    // [IN] Max tokens to put.
                             IntPtr pcTokens               // [OUT] Put # put here.
                             );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumPermissionSets(IntPtr phEnum,            // [IN|OUT] Pointer to the enum.
                                    uint tk,                 // [IN] if !NIL, token to scope the enumeration.
                                int dwActions,               // [IN] if !0, return only these actions.
                                IntPtr rPermission,          // [OUT] Put Permissions here.
                                uint cMax,                   // [IN] Max Permissions to put.
                                IntPtr pcTokens              // [OUT] Put # put here.
                                );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int FindMember(uint td,               // [IN] given typedef    
                            [MarshalAs(UnmanagedType.LPWStr)]
                        string szName,            // [IN] member name
                        IntPtr pvSigBlob,         // [IN] point to a blob value of CLR signature
                        uint cbSigBlob,           // [IN] count of bytes in the signature blob
                        IntPtr pmb                // [OUT] matching memberdef
                        );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int FindMethod(uint td,               // [IN] given typedef
                            [MarshalAs(UnmanagedType.LPWStr)]
                        string szName,            // [IN] member name
                        IntPtr pvSigBlog,         // [IN] point to a blob value of CLR signature
                        uint cbSigBlob,           // [IN] count of bytes in the signature blob
                        IntPtr pmb                // [OUT] matching memberdef
                        );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int FindField(uint td,               // [IN] given typedef
                           [MarshalAs(UnmanagedType.LPWStr)]
                       string szName,            // [IN] member name
                       IntPtr pvSigBlog,         // [IN] point to a blob value of CLR signature
                       uint cbSigBlob,           // [IN] count of bytes in the signature blob
                       IntPtr pmb                // [OUT] matching memberdef
                       );


        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int FindMemberRef(uint td,                // [IN] given typeRef
                               [MarshalAs(UnmanagedType.LPWStr)]
                           string szName,             // [IN] member name
                           IntPtr pvSigBlog,          // [IN] point to a blob value of CLR signature
                           uint cbSigBlob,            // [IN] count of bytes in the signature blob
                           IntPtr pmr                 // [OUT] matching memberref
                           );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetMethodProps(uint mb,              // The method for which to get props.
                                IntPtr pClass,       // Put method's class here.
                            IntPtr szMethod,         // Put method's name here.
                            uint cchMethod,          // Size of szMethod buffer in wide chars.
                            IntPtr pchMethod,        // Put actual size here
                            IntPtr pdwAttr,          // Put flags here.
                            IntPtr ppvSigBlob,       // [OUT] point to the blob value of meta data
                            IntPtr pcbSigBlob,       // [OUT] actual size of signature blob
                            IntPtr pulCodeRVA,       // [OUT] codeRVA
                            IntPtr pdwImplFlags      // [OUT] Impl. Flags
                            );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetMemberRefProps(uint mr,            // [IN] given memberref
                                   IntPtr ptk,        // [OUT] Put classref or classdef here.
                               IntPtr szMember,       // [OUT] buffer to fill for member's name
                               uint cchMember,        // [IN] the count of char of szMember
                               IntPtr pchMember,      // [OUT] actual count of char in member name
                               IntPtr ppvSigBlob,     // [OUT] point to meta data blob value
                               IntPtr pcbSigBlob      // [OUT] actual size of signature blob
                               );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumProperties(IntPtr phEnum,             // [IN|OUT] Pointer to the enum.
                                uint td,                  // [IN] TypeDef to scope the enumeration.
                            IntPtr rProperties,           // [OUT] Put Properties here.
                            uint cMax,                    // [IN] Max properties to put.
                            IntPtr pcProperties           // [OUT] Put # put here.
                            );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumEvents(IntPtr phEnum,          // [IN|OUT] Pointer to the enum.    
                            uint td,               // [IN] TypeDef to scope the enumeration.
                        IntPtr rEvents,            // [OUT] Put events here.
                        uint cMax,                 // [IN] Max events to put.
                        IntPtr pcEvents            // [OUT] Put # put here.
                        );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetEventProps(uint ev,                   // [IN] event token
                               IntPtr pClass,            // [OUT] typedef containing the event declarion.
                           IntPtr szEvent,               // [OUT] Event name
                           uint cchEvent,                // [IN] the count of wchar of szEvent
                           IntPtr pchEvent,              // [OUT] actual count of wchar for event's name
                           IntPtr pdwEventFlags,         // [OUT] Event flags. 
                           IntPtr ptkEventType,          // [OUT] EventType class
                           IntPtr pmdAddOn,              // [OUT] AddOn method of the event
                           IntPtr pmdRemoveOn,           // [OUT] RemoveOn method of the event
                           IntPtr pmdFire,               // [OUT] Fire method of the event
                           IntPtr rmdOtherMethod,        // [OUT] other method of the event
                           uint cMax,                    // [IN] size of rmdOtherMethod
                           IntPtr pcOtherMethod          // [OUT] total number of other method of this event
                           );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumMethodSemantics(IntPtr phEnum,       // [IN|OUT] Pointer to the enum.
                                     uint mb,            // [IN] MethodDef to scope the enumeration.
                                 IntPtr rEventProp,      // [OUT] Put Event/Property here.
                                 uint cMax,              // [IN] Max properties to put.
                                 IntPtr pcEventProp      // [OUT] Put # put here.
                                 );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetMethodSemantics(uint mb,               // [IN] method token
                                    uint tkEventProp,     // [IN] event/property token.
                                IntPtr pdwSemanticFlags   // [OUT] the role flags for the method/propevent pair
                                );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetClassLayout(uint td,                    // [IN] give typedef
                                IntPtr pdwPackSize,        // [OUT] 1, 2, 4, 8, or 16
                            IntPtr rFieldOffset,           // [OUT] field offset array
                            uint cMax,                     // [IN] size of the array
                            IntPtr pcFieldOffset,          // [OUT] needed array size
                            IntPtr pulClassSize            // [OUT] the size of the class
                            );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetFieldMarshal(uint tk,                    // [IN] given a field's memberdef
                                 IntPtr ppvNativeType,      // [OUT] native type of this field
                             IntPtr pcbNativeType           // [OUT] the count of bytes of *ppvNativeType
                             );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetRVA(uint tk,                    // Member for which to set offset
                        IntPtr pulCodeRVA,         // The offset
                    IntPtr pdwImplFlags            // the implementation flags
                    );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetPermissionSetProps(uint pm,                  // [IN] the permission token.
                                       IntPtr pdwAction,        // [OUT] CorDeclSecurity.
                                   IntPtr ppvPermission,        // [OUT] permission blob.
                                   IntPtr pcbPermission         // [OUT] count of bytes of pvPermission.
                                   );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetSigFromToken(uint mdSig,                     // [IN] Signature token.
                                 IntPtr ppvSig,                 // [OUT] return pointer to token.
                             IntPtr pcbSig                      // [OUT] return size of signature.
                             );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetModuleRefProps(uint mur,               // [IN] moduleref token.
                                   IntPtr szName,         // [OUT] buffer to fill with the moduleref name.
                               uint cchName,              // [IN] size of szName in wide characters.
                               IntPtr pchName             // [OUT] actual count of characters in the name.
                               );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumModuleRefs(IntPtr phEnum,           // [IN|OUT] pointer to the enum.
                                IntPtr rModuleRefs,     // [OUT] put modulerefs here.
                            uint cmax,                  // [IN] max memberrefs to put.
                            IntPtr pcModuleRefs         // [OUT] put # put here.
                            );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetTypeSpecFromToken(uint typespec,           // [IN] TypeSpec token.
                                      IntPtr ppvSig,          // [OUT] return pointer to TypeSpec signature
                                  IntPtr pcbSig               // [OUT] return size of signature.
                                  );

        // Not Recommended! May be removed!
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetNameFromToken(uint tk,                           // [IN] Token to get name from.  Must have a name.
                                  IntPtr pszUtf8NamePtr             // [OUT] Return pointer to UTF8 name in heap.
                              );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumUnresolvedMethods(IntPtr phEnum,               // [IN|OUT] Pointer to the enum.
                                       IntPtr rMethods,            // [OUT] Put MemberDefs here.
                                   uint cMax,                      // [IN] Max MemberDefs to put.
                                   IntPtr pcTokens                 // [OUT] Put # put here.
                                   );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetUserString(uint stk,                       // [IN] String token.
                               IntPtr szString,               // [OUT] Copy of string.
                           uint cchString,                    // [IN] Max chars of room in szString.
                           IntPtr pchString                   // [OUT] How many chars in actual string.
                           );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetPinvokeMap(uint tk,                    // [IN] FieldDef or MethodDef.
                               IntPtr pdwMappingFlags,    // [OUT] Flags used for mapping.
                           IntPtr szImportName,           // [OUT] Import name.
                           uint cchImportName,            // [IN] Size of the name buffer.
                           IntPtr pchImportName,          // [OUT] Actual number of characters stored.
                           IntPtr pmrImportDLL            // [OUT] ModuleRef token for the target DLL.
                           );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumSignatures(IntPtr phEnum,               // [IN|OUT] pointer to the enum.
                                IntPtr rSignatures,         // [OUT] put signatures here.
                            uint cmax,                      // [IN] max signatures to put.
                            IntPtr pcSignatures             // [OUT] put # put here.
                            );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumTypeSpecs(IntPtr phEnum,              // [IN|OUT] pointer to the enum.
                               IntPtr rTypeSpecs,         // [OUT] put TypeSpecs here.
                           uint cmax,                     // [IN] max TypeSpecs to put.
                           IntPtr pcTypeSpecs             // [OUT] put # put here.
                           );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumUserStrings(IntPtr phEnum,               // [IN/OUT] pointer to the enum.
                                 IntPtr rStrings,            // [OUT] put Strings here.
                             uint cmax,                      // [IN] max Strings to put.
                             IntPtr pcStrings                // [OUT] put # put here.
                             );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetParamForMethodIndex(uint md,                   // [IN] Method token.
                                        uint ulParamSeq,          // [IN] Parameter sequence.
                                    IntPtr ppd                    // [IN] Put Param token here.
                                    );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EnumUserStrings(IntPtr phEnum,                // [IN, OUT] COR enumerator.
                                 uint tk,                     // [IN] Token to scope the enumeration, 0 for all.
                             uint tkType,                     // [IN] Type of interest, 0 for all.
                             IntPtr rCustomAttributes,        // [OUT] Put custom attribute tokens here.
                             uint cMax,                       // [IN] Size of rCustomAttributes.
                             IntPtr pcCustomAttributes        // [OUT, OPTIONAL] Put count of token values here.
                             );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetCustomAttributeProps(uint cv,                 // [IN] CustomAttribute token.
                                         IntPtr ptkObj,          // [OUT, OPTIONAL] Put object token here.
                                     IntPtr ptkType,             // [OUT, OPTIONAL] Put AttrType token here.
                                     IntPtr ppBlob,              // [OUT, OPTIONAL] Put pointer to data here.
                                     IntPtr pcbSize              // [OUT, OPTIONAL] Put size of date here.
                                     );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int FindTypeRef(uint tkResolutionScope,            // [IN] ModuleRef, AssemblyRef or TypeRef.
                             [MarshalAs(UnmanagedType.LPWStr)]
                         string szName,                        // [IN] TypeRef Name.
                         IntPtr ptr                            // [OUT] matching TypeRef.
                         );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetMemberProps(uint mb,                       // The member for which to get props.
                                IntPtr pClass,                // Put member's class here.
                            IntPtr szMember,                  // Put member's name here.
                            uint cchMember,                   // Size of szMember buffer in wide chars.
                            IntPtr pchMember,                 // Put actual size here
                            IntPtr pdwAttr,                   // Put flags here.
                            IntPtr ppvSigBlob,                // [OUT] point to the blob value of meta data
                            IntPtr pcbSigBlob,                // [OUT] actual size of signature blob
                            IntPtr pulCodeRVA,                // [OUT] codeRVA
                            IntPtr pdwImplFlags,              // [OUT] Impl. Flags
                            IntPtr pdwCPlusTypeFlag,          // [OUT] flag for value type. selected ELEMENT_TYPE_*
                            IntPtr ppValue,                   // [OUT] constant value
                            IntPtr pcchValue                  // [OUT] size of constant string in chars, 0 for non-strings.
                            );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetFieldProps(uint mb,                    // The field for which to get props.
                               IntPtr pClass,             // Put field's class here.
                           IntPtr szField,                // Put field's name here.
                           uint cchField,                 // Size of szField buffer in wide chars.
                           IntPtr pchField,               // Put actual size here
                           IntPtr pdwAttr,                // Put flags here.
                           IntPtr ppvSigBlob,             // [OUT] point to the blob value of meta data
                           IntPtr pcbSigBlob,             // [OUT] actual size of signature blob
                           IntPtr pdwCPlusTypeFlag,       // [OUT] flag for value type. selected ELEMENT_TYPE_*
                           IntPtr ppValue,                // [OUT] constant value
                           IntPtr pcchValue               // [OUT] size of constant string in chars, 0 for non-strings.
                           );

        // S_OK, S_FALSE, or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetPropertyProps(uint prop,                    // [IN] property token
                                  IntPtr pClass,               // [OUT] typedef containing the property declarion.
                              IntPtr szProperty,               // [OUT] Property name
                              uint cchProperty,                // [IN] the count of wchar of szProperty
                              IntPtr pchProperty,              // [OUT] actual count of wchar for property name
                              IntPtr pdwPropFlags,             // [OUT] property flags.
                              IntPtr ppvSig,                   // [OUT] property type. pointing to meta data internal blob
                              IntPtr pbSig,                    // [OUT] count of bytes in *ppvSig
                              IntPtr pdwCPlusTypeFlag,         // [OUT] flag for value type. selected ELEMENT_TYPE_*
                              IntPtr ppDefaultValue,           // [OUT] constant value
                              IntPtr pcchDefaultValue,         // [OUT] size of constant string in chars, 0 for non-strings.
                              IntPtr pmdSetter,                // [OUT] setter method of the property
                              IntPtr pmdGetter,                // [OUT] getter method of the property
                              IntPtr rmdOtherMethod,           // [OUT] other method of the property
                              uint cMax,                       // [IN] size of rmdOtherMethod
                              IntPtr pcOtherMethod             // [OUT] total number of other method of this property
                              );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetParamProps(uint tk,                       // [IN]The Parameter.
                               IntPtr pmd,                   // [OUT] Parent Method token.
                           IntPtr pulSequence,               // [OUT] Parameter sequence.
                           IntPtr szName,                    // [OUT] Put name here.
                           uint cchName,                     // [OUT] Size of name buffer.
                           IntPtr pchName,                   // [OUT] Put actual size of name here.
                           IntPtr pdwAttr,                   // [OUT] Put flags here.
                           IntPtr pdwCPlusTypeFlag,          // [OUT] Flag for value type. selected ELEMENT_TYPE_*.
                           IntPtr ppValue,                   // [OUT] Constant value.
                           IntPtr pcchValue                  // [OUT] size of constant string in chars, 0 for non-strings.
                           );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetCustomAttributeByName(uint tkObj,             // [IN] Object with Custom Attribute.
                                          [MarshalAs(UnmanagedType.LPWStr)]
                                      string szName,             // [IN] Name of desired Custom Attribute.
                                      IntPtr ppData,             // [OUT] Put pointer to data here.
                                      IntPtr pcbData             // [OUT] Put size of data here.
                                      );

        // True or False.   
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int IsValidToken(uint tk                            // [IN] Given token.
                              );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetNestedClassProps(uint tdNestedClass,                  // [IN] NestedClass token.
                                     IntPtr ptdEnclosingClass            // [OUT] EnclosingClass token.
                                 );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetNativeCallConvFromSig(IntPtr pvSig,                     // [IN] Pointer to signature.
                                          uint cbSig,                      // [IN] Count of signature bytes.
                                      IntPtr pCallConv                     // [OUT] Put calling conv here (see CorPinvokemap).
                                      );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int IsGlobal(uint pd,                     // [IN] Type, Field, or Method token.
                      IntPtr pbGlobal                 // [OUT] Put 1 if global, 0 otherwise.
                      );
        #endregion

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumGenericParams(IntPtr phEnum,                   // [IN|OUT] Pointer to the enum.    
                               uint tk,                        // [IN] TypeDef or MethodDef whose generic parameters are requested
                               IntPtr rGenericParams,          // [OUT] Put GenericParams here.   
                               uint cMax,                      // [IN] Max GenericParams to put.  
                               IntPtr pcGenericParams          // [OUT] Put # put here.    
                               );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetGenericParamProps(uint gp,                       // [IN] GenericParam
                                  IntPtr pulParamSeq,           // [OUT] Index of the type parameter
                                  IntPtr pdwParamFlags,         // [OUT] Flags, for future use (e.g. variance)
                                  IntPtr ptOwner,               // [OUT] Owner (TypeDef or MethodDef)
                                  IntPtr ptkKind,               // [OUT] For future use (e.g. non-type parameters)
                                  IntPtr wzName,                // [OUT] Put name here
                                  uint cchName,                 // [IN] Size of buffer
                                  IntPtr pchName                // [OUT] Put size of name (wide chars) here.
                                  );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetMethodSpecProps(uint mi,                   // [IN] The method instantiation
                                IntPtr tkParent,          // [OUT] MethodDef or MemberRef
                                IntPtr ppvSigBlob,        // [OUT] point to the blob value of meta data   
                                IntPtr pcbSigBlob         // [OUT] actual size of signature blob  
                                );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumGenericParamConstraints(IntPtr phEnum,                       // [IN|OUT] Pointer to the enum.    
                                         uint tk,                            // [IN] GenericParam whose constraints are requested
                                         IntPtr rGenericParamConstraints,    // [OUT] Put GenericParamConstraints here.   
                                         uint cMax,                          // [IN] Max GenericParamConstraints to put.  
                                         IntPtr pcGenericParamConstraints    // [OUT] Put # put here.
                                         );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetGenericParamConstraintProps(uint gpc,                     // [IN] GenericParamConstraint
                                            IntPtr ptGenericParam,       // [OUT] GenericParam that is constrained
                                            IntPtr ptkConstraintType     // [OUT] TypeDef/Ref/Spec constraint
                                            );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetPEKind(IntPtr pdwPEKind,        // [OUT] The kind of PE (0 - not a PE)
                       IntPtr pdwMAchine       // [OUT] Machine as defined in NT header
                       );

        // S_OK or error.
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetVersionString(IntPtr pwzBuf,             // [OUT[ Put version string here.
                              int ccBufSize,            // [IN] size of the buffer, in wide chars
                              IntPtr pccBufSize         // [OUT] Size of the version string, wide chars, including terminating nul.
                              );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int EnumMethodSpecs(IntPtr phEnum,                         // [IN|OUT] Pointer to the enum.    
                            uint tk,                               // [IN] MethodDef or MemberRef whose MethodSpecs are requested
                            IntPtr rMethodSpecs,                   // [OUT] Put MethodSpecs here.   
                            uint cMax,                             // [IN] Max tokens to put.  
                            IntPtr pcMethodSpecs);                 // [OUT] Put actual count here.   
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EE62470B-E94B-424e-9B7C-2F00C9249F93")]
    [ComVisible(true)]
    public interface IMetaDataAssemblyImport
    {
        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error.
        int GetAssemblyProps(uint mda,                   // [IN] The Assembly for which to get the properties.
                              IntPtr ppbPublicKey,       // [OUT] Pointer to the public key.
                              IntPtr pcbPublicKey,       // [OUT] Count of bytes in the public key.
                              IntPtr pulHashAlgId,       // [OUT] Hash Algorithm.
                              IntPtr szName,             // [OUT] Buffer to fill with name.
                              uint cchName,              // [IN] Size of buffer in wide chars.
                              IntPtr pchName,            // [OUT] Actual # of wide chars in name.
                              IntPtr pMetaData,          // [OUT] Assembly MetaData.
                              IntPtr pdwAssemblyFlags    // [OUT] Flags.        
                              );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error.
        int GetAssemblyRefProps(uint mdar,                     // [IN] The AssemblyRef for which to get the properties.
                                 IntPtr ppbPublicKeyOrToken,   // [OUT] Pointer to the public key or token.
                                 IntPtr pcbPublicKeyOrToken,   // [OUT] Count of bytes in the public key or token.
                                 IntPtr szName,                // [OUT] Buffer to fill with name.
                                 uint cchName,                 // [IN] Size of buffer in wide chars.
                                 IntPtr pchName,               // [OUT] Actual # of wide chars in name.
                                 IntPtr pMetaData,             // [OUT] Assembly MetaData.
                                 IntPtr ppbHashValue,          // [OUT] Hash blob.
                                 IntPtr pcbHashValue,          // [OUT] Count of bytes in the hash blob.
                                 IntPtr pdwAssemblyRefFlags    // [OUT] Flags.
                                 );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error.
        int GetFileProps(uint mdf,                     // [IN] The File for which to get the properties.
                          IntPtr szName,               // [OUT] Buffer to fill with name.
                          uint cchName,                // [IN] Size of buffer in wide chars.
                          IntPtr pchName,              // [OUT] Actual # of wide chars in name.
                          IntPtr ppbHashValue,         // [OUT] Pointer to the Hash Value Blob.
                          IntPtr pcbHashValue,         // [OUT] Count of bytes in the Hash Value Blob.
                          IntPtr pdwFileFlags          // [OUT] Flags.
                          );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error.
        int GetExportedTypeProps(uint mdct,                     // [IN] The ExportedType for which to get the properties.
                                  IntPtr szName,                // [OUT] Buffer to fill with name.
                                  uint cchName,                 // [IN] Size of buffer in wide chars.
                                  IntPtr pchName,               // [OUT] Actual # of wide chars in name.
                                  IntPtr ptkImplementation,     // [OUT] mdFile or mdAssemblyRef or mdExportedType.
                                  IntPtr ptkTypeDef,            // [OUT] TypeDef token within the file.
                                  IntPtr pdwExportedTypeFlags   // [OUT] Flags.
                                  );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error.
        int GetManifestResourceProps(uint mdmr,                    // [IN] The ManifestResource for which to get the properties.
                                      IntPtr szName,               // [OUT] Buffer to fill with name.
                                      uint cchName,                // [IN] Size of buffer in wide chars.
                                      IntPtr pchName,              // [OUT] Actual # of wide chars in name.
                                      IntPtr ptkImplementation,    // [OUT] mdFile or mdAssemblyRef that provides the ManifestResource.
                                      IntPtr pdwOffset,            // [OUT] Offset to the beginning of the resource within the file.
                                      IntPtr pdwResourceFlags      // [OUT] Flags.
                                      );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error
        int EnumAssemblyRefs(IntPtr phEnum,             // [IN|OUT] Pointer to the enum.
                              IntPtr rAssemblyRefs,     // [OUT] Put AssemblyRefs here.
                              uint cMax,                // [IN] Max AssemblyRefs to put.
                              IntPtr pcTokens           // [OUT] Put # put here.
                              );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error
        int EnumFiles(IntPtr phEnum,              // [IN|OUT] Pointer to the enum.
                       IntPtr rFiles,             // [OUT] Put Files here.
                       uint cMax,                 // [IN] Max Files to put.
                       IntPtr pcTokens            // [OUT] Put # put here.
                       );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error
        int EnumExportedTypes(IntPtr phEnum,           // [IN|OUT] Pointer to the enum.
                               IntPtr rExportedTypes,  // [OUT] Put ExportedTypes here.
                               uint cMax,              // [IN] Max ExportedTypes to put.
                               IntPtr pcTokens         // [OUT] Put # put here.
                               );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error
        int EnumManifestResources(IntPtr phEnum,                // [IN|OUT] Pointer to the enum.
                                   IntPtr rManifestResources,   // [OUT] Put ManifestResources here.
                                   uint cMax,                   // [IN] Max Resources to put.
                                   IntPtr pcTokens              // [OUT] Put # put here.
                                   );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error
        int GetAssemblyFromScope(IntPtr ptkAssembly   // [OUT] Put token here.
                                  );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error
        int FindExportedTypeByName([MarshalAs(UnmanagedType.LPWStr)]
                                   string szName,                         // [IN] Name of the ExportedType.
                                   uint mdtExportedType,                  // [IN] ExportedType for the enclosing class.
                                   IntPtr ptkExportedType                 // [OUT] Put the ExportedType token here.
                                   );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error
        int FindManifestResourceByName([MarshalAs(UnmanagedType.LPWStr)]
                                       string szName,               // [IN] Name of the ManifestResource.
                                       IntPtr ptkManifestResource   // [OUT] Put the ManifestResource token here.
                                       );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        void CloseEnum(IntPtr hEnum    // Enum to be closed.
                        );

        [MethodImpl(MethodImplOptions.PreserveSig)]
        // S_OK or error
        int FindAssembliesByName([MarshalAs(UnmanagedType.LPWStr)]
                string szAppBase,                                    // [IN] optional - can be NULL
                                 [MarshalAs(UnmanagedType.LPWStr)]
                string szPrivateBin,                                 // [IN] optional - can be NULL
                                 [MarshalAs(UnmanagedType.LPWStr)]
                string szAssemblyName,                               // [IN] required - this is the assembly you are requesting
                                 IntPtr ppIUnk,                      // [OUT] put IMetaDataAssemblyImport pointers here
                                 uint cMax,                          // [IN] The max number to put
                                 IntPtr pcAssemblies                 // [OUT] The number of assemblies returned.
                                 );
    }

    public unsafe class Helper
    {
        private static uint TypeFromToken(uint tk)
        {
            return tk & 0xff000000;
        }

        private static bool IsTokenNil(uint tk)
        {
            return tk == 0;
        }

        private static uint CorSigUncompressData(ref byte* pData)
        {
            uint res;

            if ((*pData & 0x80) == 0x00)        // 0??? ????    
                return *pData++;

            // 1 byte data is handled in CorSigUncompressData   
            //  _ASSERTE(*pData & 0x80);    
            // Medium.  
            if ((*pData & 0xC0) == 0x80)  // 10?? ????  
            {
                res = (uint)((*pData++ & 0x3f) << 8);
                res |= *pData++;
            }
            else // 110? ???? 
            {
                res = (uint)((*pData++ & 0x1f) << 24);
                res |= (uint)(*pData++ << 16);
                res |= (uint)(*pData++ << 8);
                res |= *pData++;
            }

            return res;
        }

        public static string AssemblyGetName(IMetaDataImport mdi)
        {
            uint tk;
            uint chName;
            IMetaDataAssemblyImport mdai = mdi as IMetaDataAssemblyImport;
            Debug.Assert(mdai != null);
            mdai.GetAssemblyFromScope((IntPtr)(&tk));

            mdai.GetAssemblyProps(tk, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, (IntPtr)(&chName), IntPtr.Zero, IntPtr.Zero);

            char* szName = stackalloc char[(int)chName];

            mdai.GetAssemblyProps(tk, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (IntPtr)szName, chName, (IntPtr)(&chName), IntPtr.Zero, IntPtr.Zero);

            return new string(szName);
        }

        private static string ClassDerivesFrom(IMetaDataImport mdi, uint tk)
        {
            uint tkExtends;
            string name = null;

            mdi.GetTypeDefProps(tk, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, (IntPtr)(&tkExtends));
            if (!IsTokenNil(tkExtends))
            {
                if (TypeFromToken(tkExtends) == (uint)CorTokenType.mdtTypeDef)
                {
                    name = ClassGetName(mdi, tkExtends);
                }
                else
                {
                    uint chName;

                    mdi.GetTypeRefProps(tkExtends, IntPtr.Zero, IntPtr.Zero, 0, (IntPtr)(&chName));

                    char* szName = stackalloc char[(int)chName];

                    mdi.GetTypeRefProps(tkExtends, IntPtr.Zero, (IntPtr)szName, chName, (IntPtr)(&chName));
                    name = new string(szName);
                }
            }

            return name;
        }

        //This we should keep in the pdbx?  Kind of annoying to get from here
        //need to keep going up the parent class, looking for "System.Enum"
        public static bool ClassIsEnum(IMetaDataImport mdi, uint tk)
        {
            if ((ClassGetProps(mdi, tk) & (uint)MetaData.CorTypeAttr.tdSealed) == 0)
                return false;

            string nameParent = ClassDerivesFrom(mdi, tk);

            return nameParent == "System.Enum";
        }

        //This we should keep in the pdbx?  Kind of annoying to get from here
        //Don't think this is needed?
        public static bool ClassIsValueClass(IMetaDataImport mdi, uint tk)
        {
            if ((ClassGetProps(mdi, tk) & (uint)MetaData.CorTypeAttr.tdSealed) == 0)
                return false;

            string nameParent = ClassDerivesFrom(mdi, tk);

            return nameParent == "System.ValueType" || nameParent == "System.Enum";
        }

        public static string ClassGetName(IMetaDataImport mdi, uint tk)
        {
            uint chName;

            mdi.GetTypeDefProps(tk, IntPtr.Zero, 0, (IntPtr)(&chName), IntPtr.Zero, IntPtr.Zero);

            char* szName = stackalloc char[(int)chName];

            mdi.GetTypeDefProps(tk, (IntPtr)szName, chName, (IntPtr)(&chName), IntPtr.Zero, IntPtr.Zero);
            return new string(szName);
        }

        public static uint ClassTokenFromName(IMetaDataImport mdi, string name)
        {
            uint tk;
            mdi.FindTypeDefByName(name, 0, (IntPtr)(&tk));
            return tk;
        }

        public static uint ClassGetProps(IMetaDataImport mdi, uint tk)
        {
            uint dwFlags;

            mdi.GetTypeDefProps(tk, IntPtr.Zero, 0, IntPtr.Zero, (IntPtr)(&dwFlags), IntPtr.Zero);

            return dwFlags;
        }

        //Don't think this is needed?
        public static uint MethodGetMaxStack(IMetaDataImport mdi, uint tk)
        {
            return 0;
        }

        public static int MethodGetImplFlags(IMetaDataImport mdi, uint tk)
        {
            int dwImplFlags;

            mdi.GetMethodProps(tk, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, new IntPtr(&dwImplFlags));
            return dwImplFlags;
        }

        public static int MethodGetAttributes(IMetaDataImport mdi, uint tk)
        {
            int dwAttributes;

            mdi.GetMethodProps(tk, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, new IntPtr(&dwAttributes), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            return dwAttributes;
        }

        public static bool MethodIsInternal(IMetaDataImport mdi, uint tk)
        {
            return (MethodGetImplFlags(mdi, tk) & (int)MetaData.CorMethodImpl.miInternalCall) != 0;
        }

        public static string MethodGetName(IMetaDataImport mdi, uint tk)
        {
            uint chName;

            mdi.GetMethodProps(tk, IntPtr.Zero, IntPtr.Zero, 0, (IntPtr)(&chName), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            char* szName = stackalloc char[(int)chName];

            mdi.GetMethodProps(tk, IntPtr.Zero, (IntPtr)szName, chName, (IntPtr)(&chName), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            return new string(szName);
        }

        public static bool MethodIsInstance(IMetaDataImport mdi, uint tk)
        {
            return (MethodGetAttributes(mdi, tk) & (int)CorMethodAttr.mdStatic) == 0;
        }

        public static bool MethodIsVirtual(IMetaDataImport mdi, uint tk)
        {
            return (MethodGetAttributes(mdi, tk) & (int)CorMethodAttr.mdVirtual) != 0;
        }

        //need to parse methodsig
        public static uint MethodGetNumArg(IMetaDataImport mdi, uint tk)
        {
            uint cbSig;
            byte* pSig;
            mdi.GetMethodProps(tk, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, (IntPtr)(&pSig), (IntPtr)(&cbSig), IntPtr.Zero, IntPtr.Zero);

            byte flags = *pSig++;
            uint cArgs = CorSigUncompressData(ref pSig);

            if ((flags & (byte)CorCallingConvention.IMAGE_CEE_CS_CALLCONV_HASTHIS) != 0)
                cArgs++;

            return cArgs;
        }

        //need to parse localsig -- need to parse IL to get locals sig??!
        public static uint MethodGetNumLocal(IMetaDataImport mdi, uint tk)
        {
            return 0;
        }

        public static string FieldGetName(IMetaDataImport mdi, uint tk)
        {
            uint chName;

            mdi.GetFieldProps(tk, IntPtr.Zero, IntPtr.Zero, 0, (IntPtr)(&chName), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            char* szName = stackalloc char[(int)chName];

            mdi.GetFieldProps(tk, IntPtr.Zero, (IntPtr)szName, chName, (IntPtr)(&chName), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            return new string(szName);
        }

        public static bool FieldIsStatic(IMetaDataImport mdi, uint tk)
        {
            int dwAttr;
            mdi.GetFieldProps(tk, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, (IntPtr)(&dwAttr), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            return (dwAttr & (int)CorFieldAttr.fdStatic) != 0;
        }
    }
}
