using System;
using System.Runtime.InteropServices;
using CorDebugInterop;
using System.Diagnostics;
using System.Collections;

namespace Microsoft.SPOT.Debugger.MetaData
{
    public class MetaDataImport : IMetaDataImport2, IMetaDataAssemblyImport
    {
        private readonly CorDebugAssembly m_assembly;
        private readonly Engine m_engine;
        private readonly Guid m_guidModule;
        private IntPtr m_fakeSig;
        private readonly int m_cbFakeSig;
        private ArrayList m_alEnums;
        private int m_handleEnumNext = 1;
        private bool m_fAlert = false;

        public MetaDataImport (CorDebugAssembly assembly)
        {
            m_assembly = assembly;
            m_engine = m_assembly.Process.Engine;
            m_guidModule = Guid.NewGuid ();
            m_alEnums = new ArrayList();

            byte[] fakeSig = new byte[] { (byte)CorCallingConvention.IMAGE_CEE_CS_CALLCONV_DEFAULT, 0x0 /*count of params*/, (byte)CorElementType.ELEMENT_TYPE_VOID};
            m_cbFakeSig = fakeSig.Length;
            m_fakeSig = Marshal.AllocCoTaskMem (m_cbFakeSig);
            Marshal.Copy (fakeSig, 0, m_fakeSig, m_cbFakeSig);
        }

        ~MetaDataImport ()
        {
            Marshal.FreeCoTaskMem (m_fakeSig);            
        }                            

        [Conditional("DEBUG")]
        private void NotImpl()
        {
            Debug.Assert(!m_fAlert, "IMDI NotImpl");
        }

        private class HCorEnum
        {
            public int m_handle;
            public int[] m_tokens;
            public int m_iToken;

            public HCorEnum(int handle, int[] tokens)
            {
                m_handle = handle;
                m_tokens = tokens;
                m_iToken = 0;
            }

            public int Reset(uint uPos)
            {
                m_iToken = (int)uPos;
                return Utility.COM_HResults.S_OK;
            }

            public int Count
            {
                get { return m_tokens.Length; }
            }

            public int Enum(IntPtr dest, uint cMax, IntPtr pct)
            {
                int cItems = Math.Min((int)cMax, this.Count - m_iToken);
                
                Marshal.WriteInt32(pct, cItems);
                Marshal.Copy(m_tokens, m_iToken, pct, cItems);
                
                m_iToken += cItems;
                
                return Utility.COM_HResults.S_OK;
            }
        }

        private HCorEnum CreateEnum(int[] tokens)
        {
            HCorEnum hce = new HCorEnum(m_handleEnumNext, tokens);
            m_alEnums.Add(hce);
            m_handleEnumNext++;

            return hce;
        }

        private HCorEnum HCorEnumFromHandle(int handle)
        {
            foreach (HCorEnum hce in m_alEnums)
            {
                if (hce.m_handle == handle)
                    return hce;
            }

            return null;
        }

        private int EnumNoTokens( IntPtr phEnum, IntPtr pcTokens )
        {
            int[] tokens = new int[0];        
            HCorEnum hce = CreateEnum( tokens );

            if( phEnum != IntPtr.Zero )
                Marshal.WriteInt32( phEnum, hce.m_handle );
            
            if( pcTokens != IntPtr.Zero )
                Marshal.WriteInt32( pcTokens, 0 );

            return Utility.COM_HResults.S_OK;
        }

        #region IMetaDataImport2 Members

        public int EnumGenericParams (IntPtr phEnum, uint tk, IntPtr rGenericParams, uint cMax, IntPtr pcGenericParams)
        {
            return EnumNoTokens( phEnum, pcGenericParams );
        }

        public int GetGenericParamProps (uint gp, IntPtr pulParamSeq, IntPtr pdwParamFlags, IntPtr ptOwner, IntPtr ptkKind, IntPtr wzName, uint cchName, IntPtr pchName)
        {
            // MetaDataImport.GetGenericParamProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetMethodSpecProps (uint mi, IntPtr tkParent, IntPtr ppvSigBlob, IntPtr pcbSigBlob)
        {
            // MetaDataImport.GetMethodSpecProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int EnumGenericParamConstraints (IntPtr phEnum, uint tk, IntPtr rGenericParamConstraints, uint cMax, IntPtr pcGenericParamConstraints)
        {
            return EnumNoTokens( phEnum, pcGenericParamConstraints );
        }

        public int GetGenericParamConstraintProps (uint gpc, IntPtr ptGenericParam, IntPtr ptkConstraintType)
        {
            // MetaDataImport.GetGenericParamConstraintProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetPEKind (IntPtr pdwPEKind, IntPtr pdwMAchine)
        {
            // MetaDataImport.GetPEKind is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetVersionString (IntPtr pwzBuf, int ccBufSize, IntPtr pccBufSize)
        {
            // MetaDataImport.GetVersionString is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int EnumMethodSpecs(IntPtr phEnum, uint tk, IntPtr rMethodSpecs, uint cMax, IntPtr pcMethodSpecs) 
        {
            return EnumNoTokens( phEnum, pcMethodSpecs );
        }

        #endregion

        #region IMetaDataImport/IMetaDataAssemblyImport Members
        public void CloseEnum (IntPtr hEnum)
        {
            HCorEnum hce = HCorEnumFromHandle(hEnum.ToInt32());
            if (hce != null)
                m_alEnums.Remove(hce);
        }        
        #endregion

        #region IMetaDataImport Members

        public int CountEnum (IntPtr hEnum, IntPtr pulCount)
        {            
            HCorEnum hce = HCorEnumFromHandle(hEnum.ToInt32());
            Marshal.WriteInt32(pulCount, hce.Count);
            return Utility.COM_HResults.S_OK;
        }

        public int ResetEnum (IntPtr hEnum, uint ulPos)
        {            
            HCorEnum hce = HCorEnumFromHandle(hEnum.ToInt32());
            return hce.Reset(ulPos);            
        }

        public int EnumTypeDefs (IntPtr phEnum, IntPtr rTypeDefs, uint cMax, IntPtr pcTypeDefs)
        {
            return EnumNoTokens( phEnum, pcTypeDefs );
        }

        public int EnumInterfaceImpls (IntPtr phEnum, uint td, IntPtr rImpls, uint cMax, IntPtr pcImpls)
        {
            return EnumNoTokens( phEnum, pcImpls );
        }

        public int EnumTypeRefs (IntPtr phEnum, IntPtr rTypeRefs, uint cMax, IntPtr pcTypeRefs)
        {
            return EnumNoTokens( phEnum, pcTypeRefs );
        }

        public int FindTypeDefByName (string szTypeDef, uint tkEnclosingClass, IntPtr mdTypeDef)
        {
            // MetaDataImport.FindTypeDefByName is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetScopeProps (IntPtr szName, uint cchName, IntPtr pchName, IntPtr pmvid)
        {
            // MetaDataImport.GetScopeProps is not implemented
            m_assembly.ICorDebugAssembly.GetName (cchName, pchName, szName);

            if (pmvid != IntPtr.Zero)
            {
                byte [] guidModule = m_guidModule.ToByteArray();
                Marshal.Copy (guidModule, 0, pmvid, guidModule.Length);
            }

            return Utility.COM_HResults.S_OK;
        }

        public int GetModuleFromScope (IntPtr pmd)
        {
            // MetaDataImport.GetModuleFromScope is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetTypeDefProps (uint td, IntPtr szTypeDef, uint cchTypeDef, IntPtr pchTypeDef, IntPtr pdwTypeDefFlags, IntPtr ptkExtends)
        {
            uint tk     = TinyCLR_TypeSystem.SymbollessSupport.TinyCLRTokenFromTypeDefToken (td);
            uint index  = TinyCLR_TypeSystem.ClassMemberIndexFromTinyCLRToken (td, m_assembly);
            string name = m_engine.GetTypeName (index);

            Utility.MarshalString (name, cchTypeDef, pchTypeDef, szTypeDef);
            Utility.MarshalInt (pchTypeDef, 0);
            Utility.MarshalInt (pdwTypeDefFlags, 0);
            Utility.MarshalInt (ptkExtends, 0);            
            return Utility.COM_HResults.S_OK;
        }

        public int GetInterfaceImplProps (uint iiImpl, IntPtr pClass, IntPtr ptkIface)
        {
            // MetaDataImport.GetInterfaceImplProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetTypeRefProps (uint tr, IntPtr ptkResolutionScope, IntPtr szName, uint cchName, IntPtr pchName)
        {
            // MetaDataImport.GetTypeRefProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int ResolveTypeRef (uint tr, IntPtr riid, ref object ppIScope, IntPtr ptd)
        {
            // MetaDataImport.ResolveTypeRef is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int EnumMembers (IntPtr phEnum, uint cl, IntPtr rMembers, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumMembersWithName (IntPtr phEnum, uint cl, string szName, IntPtr rMembers, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumMethods (IntPtr phEnum, uint cl, IntPtr rMethods, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumMethodsWithName (IntPtr phEnum, uint cl, string szName, IntPtr rMethods, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumFields (IntPtr phEnum, uint cl, IntPtr rFields, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumFieldsWithName (IntPtr phEnum, uint cl, string szName, IntPtr rFields, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumParams (IntPtr phEnum, uint mb, IntPtr rParams, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumMemberRefs (IntPtr phEnum, uint tkParent, IntPtr rMemberRefs, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumMethodImpls (IntPtr phEnum, uint td, IntPtr rMethodBody, IntPtr rMethodDecl, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumPermissionSets (IntPtr phEnum, uint tk, int dwActions, IntPtr rPermission, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int FindMember (uint td, string szName, IntPtr pvSigBlob, uint cbSigBlob, IntPtr pmb)
        {
            // MetaDataImport.FindMember is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int FindMethod (uint td, string szName, IntPtr pvSigBlog, uint cbSigBlob, IntPtr pmb)
        {
            // MetaDataImport.FindMethod is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int FindField (uint td, string szName, IntPtr pvSigBlog, uint cbSigBlob, IntPtr pmb)
        {
            // MetaDataImport.FindField is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int FindMemberRef (uint td, string szName, IntPtr pvSigBlog, uint cbSigBlob, IntPtr pmr)
        {
            // MetaDataImport.FindMemberRef is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetMethodProps (uint mb, IntPtr pClass, IntPtr szMethod, uint cchMethod, IntPtr pchMethod, IntPtr pdwAttr, IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pulCodeRVA, IntPtr pdwImplFlags)
        {
            uint tk = TinyCLR_TypeSystem.SymbollessSupport.TinyCLRTokenFromMethodDefToken (mb);

            uint md = TinyCLR_TypeSystem.ClassMemberIndexFromTinyCLRToken (tk, m_assembly);                           
            WireProtocol.Commands.Debugging_Resolve_Method.Result resolvedMethod = m_engine.ResolveMethod( md );
            
            string name = null;
            uint tkClass = 0;

            if (resolvedMethod != null)
            {
                name = resolvedMethod.m_name;                
                uint tkType = TinyCLR_TypeSystem.TinyCLRTokenFromTypeIndex (resolvedMethod.m_td);
                tkClass = TinyCLR_TypeSystem.SymbollessSupport.TypeDefTokenFromTinyCLRToken (tkType);
            }

            Utility.MarshalString (name, cchMethod, pchMethod, szMethod);
            Utility.MarshalInt (pClass, (int)tkClass);
            Utility.MarshalInt(pdwAttr, (int)CorMethodAttr.mdStatic);
            Utility.MarshalInt(pulCodeRVA, 0);
            Utility.MarshalInt(pdwImplFlags, 0);
            Utility.MarshalInt(pcbSigBlob, m_cbFakeSig);
            Utility.MarshalInt(ppvSigBlob, m_fakeSig.ToInt32 ());

            return Utility.COM_HResults.S_OK;
        }

        public int GetMemberRefProps (uint mr, IntPtr ptk, IntPtr szMember, uint cchMember, IntPtr pchMember, IntPtr ppvSigBlob, IntPtr pcbSigBlob)
        {
            // MetaDataImport.GetMemberRefProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int EnumProperties (IntPtr phEnum, uint td, IntPtr rProperties, uint cMax, IntPtr pcProperties)
        {
            return EnumNoTokens( phEnum, pcProperties );
        }

        public int EnumEvents (IntPtr phEnum, uint td, IntPtr rEvents, uint cMax, IntPtr pcEvents)
        {
            return EnumNoTokens( phEnum, pcEvents );            
        }

        public int GetEventProps (uint ev, IntPtr pClass, IntPtr szEvent, uint cchEvent, IntPtr pchEvent, IntPtr pdwEventFlags, IntPtr ptkEventType, IntPtr pmdAddOn, IntPtr pmdRemoveOn, IntPtr pmdFire, IntPtr rmdOtherMethod, uint cMax, IntPtr pcOtherMethod)
        {
            // MetaDataImport.GetEventProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int EnumMethodSemantics (IntPtr phEnum, uint mb, IntPtr rEventProp, uint cMax, IntPtr pcEventProp)
        {
            return EnumNoTokens( phEnum, pcEventProp );            
        }

        public int GetMethodSemantics (uint mb, uint tkEventProp, IntPtr pdwSemanticFlags)
        {
            // MetaDataImport.GetMethodSemantics is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetClassLayout (uint td, IntPtr pdwPackSize, IntPtr rFieldOffset, uint cMax, IntPtr pcFieldOffset, IntPtr pulClassSize)
        {
            // MetaDataImport.GetClassLayour is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetFieldMarshal (uint tk, IntPtr ppvNativeType, IntPtr pcbNativeType)
        {
            // MetaDataImport.GetFieldMarshal is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetRVA (uint tk, IntPtr pulCodeRVA, IntPtr pdwImplFlags)
        {
            // MetaDataImport.GetRVA is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetPermissionSetProps (uint pm, IntPtr pdwAction, IntPtr ppvPermission, IntPtr pcbPermission)
        {
            // MetaDataImport.GetPermissionSetProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetSigFromToken (uint mdSig, IntPtr ppvSig, IntPtr pcbSig)
        {
            // MetaDataImport.GetSigFromToken is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetModuleRefProps (uint mur, IntPtr szName, uint cchName, IntPtr pchName)
        {
            // MetaDataImport.GetModuleRefProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int EnumModuleRefs (IntPtr phEnum, IntPtr rModuleRefs, uint cmax, IntPtr pcModuleRefs)
        {
            return EnumNoTokens( phEnum, pcModuleRefs );
        }

        public int GetTypeSpecFromToken (uint typespec, IntPtr ppvSig, IntPtr pcbSig)
        {
            // MetaDataImport.GetTypeSpecFromToken is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetNameFromToken (uint tk, IntPtr pszUtf8NamePtr)
        {
            // MetaDataImport.GetNameFromToken is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int EnumUnresolvedMethods (IntPtr phEnum, IntPtr rMethods, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int GetUserString (uint stk, IntPtr szString, uint cchString, IntPtr pchString)
        {
            // MetaDataImport.GetUserString is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetPinvokeMap (uint tk, IntPtr pdwMappingFlags, IntPtr szImportName, uint cchImportName, IntPtr pchImportName, IntPtr pmrImportDLL)
        {
            // MetaDataImport.GetPinvokeMap is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int EnumSignatures (IntPtr phEnum, IntPtr rSignatures, uint cmax, IntPtr pcSignatures)
        {
            return EnumNoTokens( phEnum, pcSignatures );
        }

        public int EnumTypeSpecs (IntPtr phEnum, IntPtr rTypeSpecs, uint cmax, IntPtr pcTypeSpecs)
        {
            return EnumNoTokens( phEnum, pcTypeSpecs );
        }

        public int EnumUserStrings (IntPtr phEnum, IntPtr rStrings, uint cmax, IntPtr pcStrings)
        {
            return EnumNoTokens( phEnum, pcStrings );
        }

        public int GetParamForMethodIndex (uint md, uint ulParamSeq, IntPtr ppd)
        {
            // MetaDataImport.GetParamForMethodIndex is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int EnumUserStrings (IntPtr phEnum, uint tk, uint tkType, IntPtr rCustomAttributes, uint cMax, IntPtr pcCustomAttributes)
        {
            return EnumNoTokens( phEnum, pcCustomAttributes );
        }

        public int GetCustomAttributeProps (uint cv, IntPtr ptkObj, IntPtr ptkType, IntPtr ppBlob, IntPtr pcbSize)
        {
            return Utility.COM_HResults.S_FALSE;
        }

        public int FindTypeRef (uint tkResolutionScope, string szName, IntPtr ptr)
        {
            // MetaDataImport.FindTypeRef is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetMemberProps (uint mb, IntPtr pClass, IntPtr szMember, uint cchMember, IntPtr pchMember, IntPtr pdwAttr, IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pulCodeRVA, IntPtr pdwImplFlags, IntPtr pdwCPlusTypeFlag, IntPtr ppValue, IntPtr pcchValue)
        {
            // MetaDataImport.GetMemberProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetFieldProps (uint mb, IntPtr pClass, IntPtr szField, uint cchField, IntPtr pchField, IntPtr pdwAttr, IntPtr ppvSigBlob, IntPtr pcbSigBlob, IntPtr pdwCPlusTypeFlag, IntPtr ppValue, IntPtr pcchValue)
        {
            // MetaDataImport.GetFieldProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetPropertyProps (uint prop, IntPtr pClass, IntPtr szProperty, uint cchProperty, IntPtr pchProperty, IntPtr pdwPropFlags, IntPtr ppvSig, IntPtr pbSig, IntPtr pdwCPlusTypeFlag, IntPtr ppDefaultValue, IntPtr pcchDefaultValue, IntPtr pmdSetter, IntPtr pmdGetter, IntPtr rmdOtherMethod, uint cMax, IntPtr pcOtherMethod)
        {
            // MetaDataImport.GetPropertyProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetParamProps (uint tk, IntPtr pmd, IntPtr pulSequence, IntPtr szName, uint cchName, IntPtr pchName, IntPtr pdwAttr, IntPtr pdwCPlusTypeFlag, IntPtr ppValue, IntPtr pcchValue)
        {
            // MetaDataImport.GetParamProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetCustomAttributeByName (uint tkObj, string szName, IntPtr ppData, IntPtr pcbData)
        {
            // MetaDataImport.GetCustomAttributeByName is not implemented
            
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int IsValidToken (uint tk)
        {
            // MetaDataImport.IsValidToken is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetNestedClassProps (uint tdNestedClass, IntPtr ptdEnclosingClass)
        {
            // MetaDataImport.GetNestedClassProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetNativeCallConvFromSig (IntPtr pvSig, uint cbSig, IntPtr pCallConv)
        {
            // MetaDataImport.GetNativeCallConvFromSig is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int IsGlobal (uint pd, IntPtr pbGlobal)
        {
            // MetaDataImport.IsGlobal is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        #endregion

        #region IMetaDataAssemblyImport Members

        public int GetAssemblyProps (uint mda, IntPtr ppbPublicKey, IntPtr pcbPublicKey, IntPtr pulHashAlgId, IntPtr szName, uint cchName, IntPtr pchName, IntPtr pMetaData, IntPtr pdwAssemblyFlags)
        {
            m_assembly.ICorDebugAssembly.GetName(cchName, pchName, szName);
            return Utility.COM_HResults.S_OK;
        }

        public int GetAssemblyRefProps (uint mdar, IntPtr ppbPublicKeyOrToken, IntPtr pcbPublicKeyOrToken, IntPtr szName, uint cchName, IntPtr pchName, IntPtr pMetaData, IntPtr ppbHashValue, IntPtr pcbHashValue, IntPtr pdwAssemblyRefFlags)
        {
            // MetaDataImport.GetAssemblyRefProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetFileProps (uint mdf, IntPtr szName, uint cchName, IntPtr pchName, IntPtr ppbHashValue, IntPtr pcbHashValue, IntPtr pdwFileFlags)
        {
            // MetaDataImport.GetFileProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetExportedTypeProps (uint mdct, IntPtr szName, uint cchName, IntPtr pchName, IntPtr ptkImplementation, IntPtr ptkTypeDef, IntPtr pdwExportedTypeFlags)
        {
            // MetaDataImport.GetExportedTypeProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetManifestResourceProps (uint mdmr, IntPtr szName, uint cchName, IntPtr pchName, IntPtr ptkImplementation, IntPtr pdwOffset, IntPtr pdwResourceFlags)
        {
            // MetaDataImport.GetManifestResourceProps is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int EnumAssemblyRefs (IntPtr phEnum, IntPtr rAssemblyRefs, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumFiles (IntPtr phEnum, IntPtr rFiles, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumExportedTypes (IntPtr phEnum, IntPtr rExportedTypes, uint cMax, IntPtr pcTokens)
        {
            return EnumNoTokens( phEnum, pcTokens );
        }

        public int EnumManifestResources (IntPtr phEnum, IntPtr rManifestResources, uint cMax, IntPtr pcTokens)
        {
            // MetaDataImport.EnumManifestResources is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int GetAssemblyFromScope (IntPtr ptkAssembly)
        {            
            //Only one assembly per MetaDataImport, doesn't matter what token we give them back            
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int FindExportedTypeByName (string szName, uint mdtExportedType, IntPtr ptkExportedType)
        {
            // MetaDataImport.FindExportedTypeByName is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int FindManifestResourceByName (string szName, IntPtr ptkManifestResource)
        {
            // MetaDataImport.FindManifestResourceByName is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        public int FindAssembliesByName (string szAppBase, string szPrivateBin, string szAssemblyName, IntPtr ppIUnk, uint cMax, IntPtr pcAssemblies)
        {
            // MetaDataImport.FindAssembliesByName is not implemented
            NotImpl();
            return Utility.COM_HResults.E_NOTIMPL;
        }

        #endregion
    }
}
