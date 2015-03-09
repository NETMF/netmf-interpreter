using System;
using System.Diagnostics;

namespace Microsoft.SPOT.Debugger
{
    //Lots of redefinitions from TinyCLR_Types.h
    public class TinyCLR_TypeSystem
    {
        public enum CLR_TABLESENUM : uint
        {
            TBL_AssemblyRef   = 0x00000000,
            TBL_TypeRef       = 0x00000001,
            TBL_FieldRef      = 0x00000002,
            TBL_MethodRef     = 0x00000003,
            TBL_TypeDef       = 0x00000004,
            TBL_FieldDef      = 0x00000005,
            TBL_MethodDef     = 0x00000006,
            TBL_Attributes    = 0x00000007,
            TBL_TypeSpec      = 0x00000008,
            TBL_Resources     = 0x00000009,
            TBL_ResourcesData = 0x0000000A,
            TBL_Strings       = 0x0000000B,
            TBL_Signatures    = 0x0000000C,
            TBL_ByteCode      = 0x0000000D,
            TBL_EndOfAssembly = 0x0000000E,
            TBL_Max           = 0x0000000F,
        };

        public enum CorTokenType : uint
        {
            mdtModule = 0x00000000,       
            mdtTypeRef = 0x01000000,       
            mdtTypeDef = 0x02000000,       
            mdtFieldDef = 0x04000000,       
            mdtMethodDef = 0x06000000,       
            mdtParamDef = 0x08000000,       
            mdtInterfaceImpl = 0x09000000,       
            mdtMemberRef = 0x0a000000,       
            mdtCustomAttribute = 0x0c000000,       
            mdtPermission = 0x0e000000,       
            mdtSignature = 0x11000000,       
            mdtEvent = 0x14000000,       
            mdtProperty = 0x17000000,       
            mdtModuleRef = 0x1a000000,       
            mdtTypeSpec = 0x1b000000,       
            mdtAssembly = 0x20000000,       
            mdtAssemblyRef = 0x23000000,       
            mdtFile = 0x26000000,       
            mdtExportedType = 0x27000000,       
            mdtManifestResource = 0x28000000,       
            mdtString = 0x70000000,       
            mdtName = 0x71000000,       
            mdtBaseType = 0x72000000,       // Leave this on the high end value. This does not correspond to metadata table
        };

        public static uint IdxAssemblyFromIndex(uint idx)
        {
            return idx >> 16;
        }

        public static uint IdxFromIndex(uint idx)  
        {
            return idx & 0xffff;
        }

        public static uint IndexFromIdxAssemblyIdx(uint idxAssm, uint idx) 
        {
            return idxAssm << 16 | idx;
        }

        public static uint IndexFromIdxAssemblyIdx(uint idxAssm)
        {
            return idxAssm << 16;            
        }

        public static CLR_TABLESENUM CLR_TypeFromTk(uint tk)
        {
            return (CLR_TABLESENUM)(tk >> 24);
        }

        public static uint CLR_DataFromTk(uint tk)
        {
            return  tk & 0x00FFFFFF;
        }

        public static uint CLR_TkFromType( CLR_TABLESENUM tbl, uint data)
        {
            return ((((uint)tbl) << 24) & 0xFF000000) | (data & 0x00FFFFFF);
        }
        
        public static CorDebugAssembly AssemblyFromIndex (CorDebugAppDomain appDomain, uint index)
        {
            return appDomain.AssemblyFromIdx (TinyCLR_TypeSystem.IdxAssemblyFromIndex (index));
        }
       
        public static CorDebugClass CorDebugClassFromTypeIndex(uint typeIndex, CorDebugAppDomain appDomain)
        {
            CorDebugClass cls = null;

            CorDebugAssembly assembly = appDomain.AssemblyFromIdx(TinyCLR_TypeSystem.IdxAssemblyFromIndex(typeIndex));
            if (assembly != null)
            {
                uint typedef = TinyCLR_TypeSystem.CLR_TkFromType(TinyCLR_TypeSystem.CLR_TABLESENUM.TBL_TypeDef, TinyCLR_TypeSystem.IdxFromIndex(typeIndex));
                cls = assembly.GetClassFromTokenTinyCLR(typedef);
            }

            return cls;
        }

        public static CorDebugFunction CorDebugFunctionFromMethodIndex (uint methodIndex, CorDebugAppDomain appDomain)
        {
            CorDebugFunction function = null;
            CorDebugAssembly assembly = appDomain.AssemblyFromIdx (TinyCLR_TypeSystem.IdxAssemblyFromIndex (methodIndex));

            if (assembly != null)
            {
                uint tk = TinyCLR_TypeSystem.TinyCLRTokenFromMethodIndex (methodIndex);
                function = assembly.GetFunctionFromTokenTinyCLR (tk);                
            }

            return function;
        }

        public static uint ClassMemberIndexFromCLRToken(uint token, CorDebugAssembly assembly)
        {
            Pdbx.ClassMember cm = assembly.GetPdbxClassMemberFromTokenCLR(token);

            return ClassMemberIndexFromTinyCLRToken(cm.Token.TinyCLR, assembly);            
        }

        public static uint ClassMemberIndexFromTinyCLRToken(uint token, CorDebugAssembly assembly)
        {
            uint idx = TinyCLR_TypeSystem.CLR_DataFromTk(token);
            return TinyCLR_TypeSystem.IndexFromIdxAssemblyIdx(assembly.Idx, idx);
        }

        private static uint TinyCLRTokenFromIndex (TinyCLR_TypeSystem.CLR_TABLESENUM tbl, uint index)
        {
            uint idxAssembly = TinyCLR_TypeSystem.IdxAssemblyFromIndex (index);
            uint idxMethod = TinyCLR_TypeSystem.IdxFromIndex (index);

            return TinyCLR_TypeSystem.CLR_TkFromType (tbl, idxMethod);
        }

        public static uint TinyCLRTokenFromMethodIndex (uint index)
        {
            return TinyCLRTokenFromIndex (TinyCLR_TypeSystem.CLR_TABLESENUM.TBL_MethodDef, index);
        }

        public static uint TinyCLRTokenFromTypeIndex (uint index)
        {
            return TinyCLRTokenFromIndex (TinyCLR_TypeSystem.CLR_TABLESENUM.TBL_TypeDef, index);
        }

        public class SymbollessSupport
        {
            public static uint MethodDefTokenFromTinyCLRToken (uint token)
            {
                Debug.Assert (CLR_TypeFromTk (token) == CLR_TABLESENUM.TBL_MethodDef);
                return (uint)CorTokenType.mdtMethodDef | CLR_DataFromTk (token);
            }

            public static uint TinyCLRTokenFromMethodDefToken (uint token)
            {
                Debug.Assert ((token & (uint)CorTokenType.mdtMethodDef) != 0);
                return TinyCLR_TypeSystem.CLR_TkFromType (CLR_TABLESENUM.TBL_MethodDef, token & 0x00ffffff);
            }

            public static uint TypeDefTokenFromTinyCLRToken (uint token)
            {
                Debug.Assert (CLR_TypeFromTk (token) == CLR_TABLESENUM.TBL_TypeDef);
                return (uint)CorTokenType.mdtTypeDef | CLR_DataFromTk (token);
            }

            public static uint TinyCLRTokenFromTypeDefToken (uint token)
            {
                Debug.Assert ((token & (uint)CorTokenType.mdtTypeDef) != 0);
                return TinyCLR_TypeSystem.CLR_TkFromType (CLR_TABLESENUM.TBL_TypeDef, token & 0x00ffffff);
            }
        }
    }
}
