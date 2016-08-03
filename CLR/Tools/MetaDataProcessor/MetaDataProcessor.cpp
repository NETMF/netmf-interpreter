////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "keygen.h"
#include <TinyCLR_Endian.h>
#include <TinyCLR_Types.h>

#pragma comment(lib, "Comdlg32")
#include <Commdlg.h>
////////////////////////////////////////////////////////////////////////////////////////////////////

struct Settings : CLR_RT_ParseOptions
{
    PELoader                       m_pe;
    MetaData::Collection           m_col;
    MetaData::Parser*              m_pr;
    bool                           m_fEE;
    CLR_RT_Assembly*               m_assm;
    CLR_RT_ParseOptions::BufferMap m_assemblies;

    bool                           m_fDumpStatistics;

    WatchAssemblyBuilder::Linker   m_lkForStrings;

    bool                           m_patch_fReboot;
    bool                           m_patch_fSign;
    std::wstring                   m_patch_szNative;

    bool                           m_fFromAssembly;
    bool                           m_fFromImage;
    bool                           m_fNoByteCode;
    bool                           m_fBigEndian;
    bool                           m_fLittleEndian;

    CLR_RT_StringSet                m_resources;

    //--//

    struct Command_Call : CLR_RT_ParseOptions::Command
    {
        typedef HRESULT (Settings::*FPN)( CLR_RT_ParseOptions::ParameterList* params );

        Settings& m_parent;
        FPN       m_call;

        Command_Call( Settings& parent, FPN call, LPCWSTR szName, LPCWSTR szDescription ) : CLR_RT_ParseOptions::Command( szName, szDescription ), m_parent(parent), m_call(call)
        {
        }

        virtual HRESULT Execute()
        {
            return (m_parent.*m_call)( &m_params );
        }
    };


    Settings()
    {
        m_fEE             = false;

        m_fDumpStatistics = false;

        m_patch_fReboot   = false;
        m_patch_fSign     = false;

        m_fFromAssembly   = false;
        m_fFromImage      = false;
        m_fNoByteCode     = false;
        m_fBigEndian      = false;
        m_fLittleEndian   = false;
        RevertToDefaults();

        BuildOptions();
    }

    ~Settings()
    {
        Cmd_Reset();
    }

    //--//

    void RevertToDefaults()
    {
        for(CLR_RT_ParseOptions::BufferMapIter it = m_assemblies.begin(); it != m_assemblies.end(); it++)
        {
            delete it->second;
        }

        m_pe .Close();                                   // PELoader                       m_pe;
        m_col.Clear( false );                            // MetaData::Collection           m_col;
        m_pr               = NULL;                       // MetaData::Parser*              m_pr;
                                                         // bool                           m_fEE;
        m_assm             = NULL;                       // CLR_RT_Assembly*               m_assm;
        m_assemblies.clear();                            // CLR_RT_ParseOptions::BufferMap m_assemblies;
                                                         //
        m_fDumpStatistics  = false;                      // bool                           m_fDumpStatistics;
                                                         //
                                                         // WatchAssemblyBuilder::Linker   m_lkForStrings;
                                                         //
                                                         // bool                           m_patch_fReboot;
                                                         // bool                           m_patch_fSign;
                                                         // std::wstring                   m_patch_szNative;
                                                         //
        m_fFromAssembly    = false;                      // bool                           m_fFromAssembly;
        m_fFromImage       = false;                      // bool                           m_fFromImage;
                                                         // bool                           m_fNoByteCode;
    }

    //--//

    HRESULT AllocateSystem()
    {
        TINYCLR_HEADER();

        if(m_fEE == false)
        {
            TINYCLR_CHECK_HRESULT(CLR_RT_ExecutionEngine::CreateInstance());

            m_fEE = true;
        }

        TINYCLR_NOCLEANUP();
    }

    void ReleaseSystem()
    {
        if(m_fEE)
        {
            CLR_RT_ExecutionEngine::DeleteInstance();

            m_fEE = false;
        }
    }

    HRESULT CheckAssemblyFormat( CLR_RECORD_ASSEMBLY* header, LPCWSTR src )
    {
        TINYCLR_HEADER();

        if(header->GoodAssembly() == false)
        {
            wprintf( L"Invalid assembly format for '%s': ", src );
            for(int i=0; i<sizeof(header->marker); i++)
            {
                wprintf( L"%02x", header->marker[i] );
            }
            wprintf( L"\n" );

            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        TINYCLR_NOCLEANUP();
    }

    //--//

#define PARAM_GENERIC(parm1Name,parm1Desc)     param = new CLR_RT_ParseOptions::Parameter_Generic(      parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_STRING(val,parm1Name,parm1Desc)  param = new CLR_RT_ParseOptions::Parameter_String ( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_BOOLEAN(val,parm1Name,parm1Desc) param = new CLR_RT_ParseOptions::Parameter_Boolean( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_INTEGER(val,parm1Name,parm1Desc) param = new CLR_RT_ParseOptions::Parameter_Integer( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )
#define PARAM_FLOAT(val,parm1Name,parm1Desc)   param = new CLR_RT_ParseOptions::Parameter_Float  ( val, parm1Name, parm1Desc ); cmd->m_params.push_back( param )

#define PARAM_EXTRACT_STRING(lst,idx)    ((CLR_RT_ParseOptions::Parameter_Generic*)(*lst)[idx])->m_data.c_str()
#define PARAM_EXTRACT_BOOLEAN(lst,idx) *(((CLR_RT_ParseOptions::Parameter_Boolean*)(*lst)[idx])->m_dataPtr)


#define OPTION_GENERIC(optName,optDesc) cmd = new CLR_RT_ParseOptions::Command        (      optName, optDesc ); m_commands.push_back( cmd )
#define OPTION_SET(val,optName,optDesc) cmd = new CLR_RT_ParseOptions::Command_SetFlag( val, optName, optDesc ); m_commands.push_back( cmd )
#define OPTION_CALL(fpn,optName,optDesc) cmd = new Command_Call( *this, &Settings::fpn, optName, optDesc ); m_commands.push_back( cmd )

#define OPTION_STRING(val,optName,optDesc,parm1Name,parm1Desc)  OPTION_GENERIC(optName,optDesc); PARAM_STRING(val,parm1Name,parm1Desc)
#define OPTION_BOOLEAN(val,optName,optDesc,parm1Name,parm1Desc) OPTION_GENERIC(optName,optDesc); PARAM_BOOLEAN(val,parm1Name,parm1Desc)
#define OPTION_INTEGER(val,optName,optDesc,parm1Name,parm1Desc) OPTION_GENERIC(optName,optDesc); PARAM_INTEGER(val,parm1Name,parm1Desc)
#define OPTION_FLOAT(val,optName,optDesc,parm1Name,parm1Desc)   OPTION_GENERIC(optName,optDesc); PARAM_FLOAT(val,parm1Name,parm1Desc)

    HRESULT GenerateOtherEndianPE( CLR_RT_Buffer &bufferLE, const wchar_t* outFile )
    {        
        TINYCLR_HEADER();
        TINYCLR_CHECK_HRESULT(AllocateSystem());
        {
            CLR_RT_Buffer        bufferBE;
            CLR_RECORD_ASSEMBLY* headerLE;
            CLR_RECORD_ASSEMBLY* headerEndLE;
            UINT32               bufferLEBase;
            std::wstring         strName;

            //TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( lePeFile, bufferLE ));

            // ExtractEhFromByteCode needs this to be 1 field larger - the extra entry will never be accessed
            // however it it used in some pointer arithmetic.
            bufferBE.resize( bufferLE.size()+1 );
            bufferBE.assign( bufferLE.begin(), bufferLE.end() );
            bufferBE.push_back( 0 ); // the extra, unused, entry

            bufferLEBase = (UINT32)&bufferLE[0];
            headerLE    = (CLR_RECORD_ASSEMBLY*)&bufferLE[0              ];
            headerEndLE = (CLR_RECORD_ASSEMBLY*)&bufferLE[bufferLE.size()-1];
       
            if ( headerLE->GoodAssembly() )
            {
                CLR_RT_Assembly* assm = 0;
                TINYCLR_CHECK_HRESULT(CLR_RT_Assembly::CreateInstance( headerLE, assm ));
                
                //CLR_RECORD_ASSEMBLY
                {
                    CLR_RECORD_ASSEMBLY* assmrec = (CLR_RECORD_ASSEMBLY*)( &bufferBE[0] );
                    assmrec->headerCRC=SwapEndian(assmrec->headerCRC);
                    assmrec->assemblyCRC=SwapEndian(assmrec->assemblyCRC);

                    

                    if ( assmrec->flags & CLR_RECORD_ASSEMBLY::c_Flags_BigEndian )
                    {
                        // Mark the assembly as a Litte-Endian assembly.
                        assmrec->flags &= ~CLR_RECORD_ASSEMBLY::c_Flags_BigEndian;
                    }
                    else
                    {
                        // Mark the assembly as a Big-Endian assembly.
                        assmrec->flags |= CLR_RECORD_ASSEMBLY::c_Flags_BigEndian;
                    }
                    assmrec->flags=SwapEndian(assmrec->flags);
                    assmrec->nativeMethodsChecksum=SwapEndian(assmrec->nativeMethodsChecksum);
                    assmrec->patchEntryOffset=SwapEndian(assmrec->patchEntryOffset);
                    
                    assmrec->version.iMajorVersion=SwapEndian(assmrec->version.iMajorVersion);
                    assmrec->version.iMinorVersion=SwapEndian(assmrec->version.iMinorVersion);
                    assmrec->version.iBuildNumber=SwapEndian(assmrec->version.iBuildNumber);
                    assmrec->version.iRevisionNumber=SwapEndian(assmrec->version.iRevisionNumber);

                    assmrec->assemblyName=SwapEndian(assmrec->assemblyName);
                    assmrec->stringTableVersion=SwapEndian(assmrec->stringTableVersion);
                    for(int k=0; k!=TBL_Max; k++)
                    {
                        assmrec->startOfTables[k]=SwapEndian(assmrec->startOfTables[k]);
                    }
                    assmrec->numOfPatchedMethods=SwapEndian(assmrec->numOfPatchedMethods); 
                }
                
                //CLR_RECORD_ASSEMBLYREF
                {                
                    for(int i=0; i!=assm->m_pTablesSize[TBL_AssemblyRef]; i++)
                    {
                        CLR_RECORD_ASSEMBLYREF* rec = (CLR_RECORD_ASSEMBLYREF*)(assm->GetAssemblyRef(i));
                        rec = (CLR_RECORD_ASSEMBLYREF*)(&bufferBE[(UINT32)rec-bufferLEBase] );
                        rec->name=SwapEndian(rec->name);
                        rec->pad=SwapEndian(rec->pad);
                        rec->version.iMajorVersion=SwapEndian(rec->version.iMajorVersion);
                        rec->version.iMinorVersion=SwapEndian(rec->version.iMinorVersion);
                        rec->version.iBuildNumber=SwapEndian(rec->version.iBuildNumber);
                        rec->version.iRevisionNumber=SwapEndian(rec->version.iRevisionNumber);
                    }
                }

                //CLR_RECORD_TYPEREF
                {            
                    for(int i=0; i!=assm->m_pTablesSize[TBL_TypeRef]; i++)
                    {
                        CLR_RECORD_TYPEREF* rec = (CLR_RECORD_TYPEREF*)assm->GetTypeRef(i);
                        rec = (CLR_RECORD_TYPEREF*)(&bufferBE[(UINT32)rec-bufferLEBase] );
                        rec->name=SwapEndian(rec->name);
                        rec->nameSpace=SwapEndian(rec->nameSpace);
                        rec->scope=SwapEndian(rec->scope);
                        rec->pad=SwapEndian(rec->pad);
                    }
                }
                //CLR_RECORD_FIELDREF
                {            
                    for(int i=0; i!=assm->m_pTablesSize[TBL_FieldRef]; i++)
                    {
                        CLR_RECORD_FIELDREF* rec = (CLR_RECORD_FIELDREF*)assm->GetFieldRef(i);
                        rec = (CLR_RECORD_FIELDREF*)(&bufferBE[(UINT32)rec-bufferLEBase] );
                        rec->name=SwapEndian(rec->name);
                        rec->container=SwapEndian(rec->container);
                        rec->sig=SwapEndian(rec->sig);
                        rec->pad=SwapEndian(rec->pad);
                    }
                }
                //CLR_RECORD_METHODREF
                {               
                    for(int i=0; i!=assm->m_pTablesSize[TBL_MethodRef]; i++)
                    {
                        CLR_RECORD_METHODREF* rec = (CLR_RECORD_METHODREF*)assm->GetMethodRef(i);
                        rec = (CLR_RECORD_METHODREF*)(&bufferBE[(UINT32)rec-bufferLEBase] );
                        rec->name=SwapEndian(rec->name);
                        rec->container=SwapEndian(rec->container);
                        rec->sig=SwapEndian(rec->sig);
                        rec->pad=SwapEndian(rec->pad);
                    }
                }

                //CLR_RECORD_TYPEDEF
                {             
                    for(int i=0; i!=assm->m_pTablesSize[TBL_TypeDef]; i++)
                    {
                        CLR_RECORD_TYPEDEF* rec = (CLR_RECORD_TYPEDEF*)assm->GetTypeDef(i);
                        rec = (CLR_RECORD_TYPEDEF*)(&bufferBE[ (UINT32)rec-bufferLEBase ] );
                        rec->name=SwapEndian(rec->name);
                        rec->nameSpace=SwapEndian(rec->nameSpace);
                        rec->extends=SwapEndian(rec->extends);
                        rec->enclosingType=SwapEndian(rec->enclosingType);
                        rec->interfaces=SwapEndian(rec->interfaces);
                        rec->methods_First=SwapEndian(rec->methods_First);
                        rec->sFields_First=SwapEndian(rec->sFields_First);
                        rec->iFields_First=SwapEndian(rec->iFields_First);
                        rec->flags=SwapEndian(rec->flags);
                    }
                }

                //CLR_RECORD_FIELDDEF
                {
                    for(int i=0; i!=assm->m_pTablesSize[TBL_FieldDef]; i++)
                    {
                        CLR_RECORD_FIELDDEF* rec = (CLR_RECORD_FIELDDEF*)assm->GetFieldDef(i);
                        rec = (CLR_RECORD_FIELDDEF*)(&bufferBE[ (UINT32)rec-bufferLEBase ] );
                        rec->name=SwapEndian(rec->name);
                        rec->sig=SwapEndian(rec->sig);
                        rec->defaultValue=SwapEndian(rec->defaultValue);                    
                        rec->flags=SwapEndian(rec->flags);
                    }
                }

                 //CLR_RECORD_METHODDEF
                {
                    for(int i=0; i!=assm->m_pTablesSize[TBL_MethodDef]; i++)
                    {
                        CLR_RECORD_METHODDEF* rec = (CLR_RECORD_METHODDEF*)assm->GetMethodDef(i);
                        CLR_RECORD_METHODDEF* origRec = rec;
                        rec = (CLR_RECORD_METHODDEF*)(&bufferBE[ (UINT32)rec-bufferLEBase ] );
                        rec->name=SwapEndian(rec->name);
                        rec->RVA=SwapEndian(rec->RVA);
                        rec->flags=SwapEndian(rec->flags);                    
                        rec->locals=SwapEndian(rec->locals);
                        rec->sig=SwapEndian(rec->sig);

                        //obtain CLR_RECORD_EH from the methoddef and take care of it.
                        CLR_OFFSET start;
                        CLR_OFFSET end;

                        if(assm->FindMethodBoundaries( i, start, end ))
                        {                        
                            const CLR_UINT8* IP      = &bufferBE[ (UINT32)assm->GetByteCode( start ) - bufferLEBase ];
                            const CLR_UINT8* IPStart = IP;
                            const UINT32 IPendOffset   =  (UINT32)assm->GetByteCode( end ) - bufferLEBase;
                               

                            const CLR_RECORD_EH* ptrEh = NULL;
                            CLR_UINT32           numEh = 0;

                            //
                            // Extract EH data.  Use the original record to check for the EH Flags since the current one has already 
                            // been big-endianized.
                            //
                            if(origRec->flags & CLR_RECORD_METHODDEF::MD_HasExceptionHandlers)
                            {
                                const CLR_UINT8* IPend = &bufferBE[IPendOffset];
                                IPend = CLR_RECORD_EH::ExtractEhFromByteCode( IPend, ptrEh, numEh );
                            }

                            CLR_RECORD_EH* beEh = const_cast<CLR_RECORD_EH*>(ptrEh);
                            for(CLR_UINT32 j=0; j<numEh; j++)
                            {
                                beEh->mode=SwapEndian(beEh->mode);
                                beEh->classToken=SwapEndian(beEh->classToken);                            
                                beEh->tryStart=SwapEndian(beEh->tryStart);
                                beEh->tryEnd=SwapEndian(beEh->tryEnd);
                                beEh->handlerStart=SwapEndian(beEh->handlerStart);
                                beEh->handlerEnd=SwapEndian(beEh->handlerEnd);
                                beEh++;
                            }
                        }
                    }
                }
                //CLR_RECORD_ATTRIBUTE
                {
                    for(int i=0; i!=assm->m_pTablesSize[TBL_Attributes];++i)
                    {
                        CLR_RECORD_ATTRIBUTE* rec = (CLR_RECORD_ATTRIBUTE*)assm->GetAttribute(i);
                        rec = (CLR_RECORD_ATTRIBUTE*)(&bufferBE[ (UINT32)rec-bufferLEBase ] );
                        rec->constructor=SwapEndian(rec->constructor);
                        rec->data=SwapEndian(rec->data);
                        rec->ownerIdx=SwapEndian(rec->ownerIdx);
                        rec->ownerType=SwapEndian(rec->ownerType);
                    }
                }

                //CLR_RECORD_TYPESPEC
                {
                    for(int i=0; i!=assm->m_pTablesSize[TBL_TypeSpec];++i)
                    {
                        CLR_RECORD_TYPESPEC* rec = (CLR_RECORD_TYPESPEC*)assm->GetTypeSpec(i);
                        rec = (CLR_RECORD_TYPESPEC*)(&bufferBE[ (UINT32)rec-bufferLEBase ] );
                        rec->pad=SwapEndian(rec->pad);
                        rec->sig=SwapEndian(rec->sig);                    
                    }
                }
                //CLR_RECORD_EH
                {
                    //See CLR_RECORD_METHODDEF
                }

                //CLR_RECORD_RESOURCE
                {
                    // Switching to using accessor to get this info in the runtime,
                    // only swap the actual values here

                    UINT32 curSize = 0;
                    for(int i=0; i!=assm->m_pTablesSize[TBL_Resources];++i)
                    {
                        CLR_RECORD_RESOURCE* rec = (CLR_RECORD_RESOURCE*)assm->GetResource(i);
                        CLR_RECORD_RESOURCE* recNext = (CLR_RECORD_RESOURCE*)assm->GetResource(i+1);

                        UINT32 p=(UINT32)rec;
                        rec = (CLR_RECORD_RESOURCE*)(&bufferBE[ (UINT32)rec-bufferLEBase ] );
                        UINT8* pByte;

                        
                        curSize = recNext->offset - rec->offset ;
                        curSize = (recNext->flags & CLR_RECORD_RESOURCE::FLAGS_PaddingMask);
                        
                        switch (rec->kind)
                        {
                            case CLR_RECORD_RESOURCE::RESOURCE_Font:
                            {   
                                pByte = (UINT8*)assm->GetResourceData(rec->offset);
                                        
                                // LE fd
                                CLR_GFX_FontDescription *le_fd = (CLR_GFX_FontDescription* )pByte;  
                                //BE fd
                                pByte = (&bufferBE[ (UINT32)pByte-bufferLEBase ] );     

                                CLR_GFX_FontDescription *fd = (CLR_GFX_FontDescription* )pByte; 
                                
                                fd->m_metrics.m_height =    SwapEndian (fd->m_metrics.m_height );
                                fd->m_metrics.m_offset =    SwapEndian (fd->m_metrics.m_offset );
                                fd->m_metrics.m_ascent =    SwapEndian (fd->m_metrics.m_ascent );
                                fd->m_metrics.m_descent=    SwapEndian (fd->m_metrics.m_descent);
                                fd->m_metrics.m_internalLeading =   SwapEndian (fd->m_metrics.m_internalLeading );
                                fd->m_metrics.m_externalLeading =   SwapEndian (fd->m_metrics.m_externalLeading);
                                fd->m_metrics.m_aveCharWidth =  SwapEndian (fd->m_metrics.m_aveCharWidth);
                                fd->m_metrics.m_maxCharWidth =  SwapEndian (fd->m_metrics.m_maxCharWidth);

                                fd->m_ranges =  SwapEndian (fd->m_ranges );
                                fd->m_characters =  SwapEndian (fd->m_characters );
                                fd->m_flags =   SwapEndian (fd->m_flags );
                                fd->m_pad = SwapEndian (fd->m_pad );
                                        
                                pByte += sizeof(CLR_GFX_FontDescription  );
                                CLR_GFX_BitmapDescription* bm;
                                CLR_GFX_BitmapDescription* le_bm = (CLR_GFX_BitmapDescription* )((UINT32)le_fd + sizeof(CLR_GFX_FontDescription));
                                bm         = (CLR_GFX_BitmapDescription*)pByte;
    
                                bm->m_width  = SwapEndian(bm->m_width);
                                bm->m_height = SwapEndian( bm->m_height);
                                bm->m_flags  = SwapEndian( bm->m_flags);
                                //bm->m_bitsPerPixel = SwapEndian(bm->m_bitsPerPixel);
                                //bm->m_type   = SwapEndian( bm->m_type );
                                pByte += sizeof(CLR_GFX_BitmapDescription);

                                CLR_GFX_FontCharacterRange * fontCharRange = (CLR_GFX_FontCharacterRange * )pByte;
                                for(int count=0; count<le_fd->m_ranges+1;count++)
                                {
                                    fontCharRange->m_indexOfFirstFontCharacter = SwapEndian(fontCharRange->m_indexOfFirstFontCharacter );
                                    fontCharRange->m_firstChar = SwapEndian(fontCharRange->m_firstChar);
                                    fontCharRange->m_lastChar = SwapEndian(fontCharRange->m_lastChar );
                                    fontCharRange->m_rangeOffset = SwapEndian(fontCharRange->m_rangeOffset );
                                    fontCharRange ++;
                                }               
                                pByte += le_fd->GetRangeSize();
                
                                CLR_GFX_FontCharacter* fontChar = (CLR_GFX_FontCharacter*)pByte; 
                                for (int count=0;count<le_fd->m_characters+1; count++)
                                {
                                    fontChar->m_offset= SwapEndian(fontChar->m_offset);
                                    //fontChar->m_marginLeft = SwapEndian(fontChar->m_marginLeft);
                                    //fontChar->m_marginRight= SwapEndian(fontChar->m_marginRight);
                                    fontChar ++;
                                }
                                pByte += le_fd->GetCharacterSize();

                                CLR_UINT32* bmData = (CLR_UINT32*)pByte;
                                for (CLR_UINT32 count=0;count<le_bm->GetTotalSize()/sizeof(CLR_UINT32);count ++)
                                {
                                    *bmData = SwapEndian(*bmData);
                                    bmData++;
                                }
                                pByte += le_bm->GetTotalSize();

                                if(le_fd->m_flags & CLR_GFX_FontDescription::c_FontEx)
                                {       
                                    CLR_GFX_FontDescriptionEx *pfdEx = (CLR_GFX_FontDescriptionEx *)pByte;
                                    pfdEx->m_antiAliasSize = SwapEndian(pfdEx->m_antiAliasSize );
                                    pByte += sizeof(CLR_GFX_FontDescriptionEx);


                                    CLR_GFX_FontCharacterRangeEx * ftCharRangeEx =  (CLR_GFX_FontCharacterRangeEx*)pByte;
                                    for(CLR_UINT32 count=0; count <le_fd->GetRangeExSize()/sizeof(CLR_GFX_FontCharacterRangeEx); count++)  
                                    {
                                        ftCharRangeEx ->m_offsetAntiAlias = SwapEndian(ftCharRangeEx ->m_offsetAntiAlias );
                                        ftCharRangeEx++;
                                    }
                                    pByte += le_fd->GetRangeExSize();

                                    CLR_GFX_FontCharacterEx* fdCharEx=  (CLR_GFX_FontCharacterEx*)pByte; 
                                    for(CLR_UINT32 count=0; count <le_fd->GetCharacterExSize()/sizeof(CLR_GFX_FontCharacterEx); count++)   
                                    {
                                        fdCharEx->m_offsetAntiAlias = SwapEndian(fdCharEx->m_offsetAntiAlias );
                                        fdCharEx++;
                                    }
                                    pByte += le_fd->GetCharacterExSize();

                                    //CLR_UINT8* byteData = (CLR_UINT8*)pByte;
                                    //font->m_antiAliasingData =  (CLR_UINT8*)data; 
                                    //data += font->m_fontEx.GetAntiAliasSize();
                                }
                                /*
                                pByte = (UINT8*)assm->GetResourceData(rec->offset);
                                UINT8 *tmp =(UINT8*)fd;
                                for (int j=0;j<0x10;j++)
                                    wprintf(L"buf(%d) %x(le), %x  ",j,pByte[j],tmp[j] );
                                wprintf(L"\r\n");
                                */
                                break;
                            }
                            case CLR_RECORD_RESOURCE::RESOURCE_Binary:
                            {   
                                //wprintf(L"binary\r\n");
                                break;
                            }
                            case CLR_RECORD_RESOURCE::RESOURCE_Bitmap:
                            {   

                                
                                UINT8 * le_pByte;
                                pByte = (UINT8*)assm->GetResourceData(rec->offset);
                                        
                                // LE bm
                                CLR_GFX_BitmapDescription* le_bm = (CLR_GFX_BitmapDescription* )pByte; 
                                le_pByte = pByte;
                                //BE bm
                                pByte = (&bufferBE[ (UINT32)pByte-bufferLEBase ] );     

                                CLR_GFX_BitmapDescription* bm = (CLR_GFX_BitmapDescription* )pByte; 

                                curSize -= sizeof(    CLR_GFX_BitmapDescription);
    
                                bm->m_width  = SwapEndian(bm->m_width);
                                bm->m_height = SwapEndian( bm->m_height);
                                bm->m_flags  = SwapEndian( bm->m_flags);
                                //bm->m_bitsPerPixel = SwapEndian(bm->m_bitsPerPixel);
                                //bm->m_type   = SwapEndian( bm->m_type );
                                pByte += sizeof(CLR_GFX_BitmapDescription);
                                le_pByte +=sizeof(CLR_GFX_BitmapDescription);

                                if (le_bm->m_type == CLR_GFX_BitmapDescription::c_TypeJpeg )
                                {
                                }
                                else if (le_bm->m_type == CLR_GFX_BitmapDescription::c_TypeGif)
                                {
                                    GifFileHeader * pGifHd = (GifFileHeader *)pByte;



                                    pGifHd->LogicScreenWidth = SwapEndian(pGifHd->LogicScreenWidth);
                                    pGifHd->LogicScreenHeight = SwapEndian(pGifHd->LogicScreenHeight);                                    

                                    UINT32 colorTableSize = 1 << ((pGifHd->globalcolortablesize) + 1);
                                    pByte +=sizeof(GifFileHeader);

                                    pByte = pByte + (UINT32)colorTableSize* sizeof(GifPaletteEntry);

                                    bool inLoop = TRUE;

                                    while (inLoop)
                                    {
                                        UINT8 chunkType = *pByte;
                                        pByte ++;

                                        switch(chunkType)
                                        {
                                        case 0x2C:  //Image Chunk
                                        {

                                            GifImageDescriptor *pgifDes = (GifImageDescriptor *)pByte;

                                            pgifDes->left = SwapEndian(pgifDes->left );

                                            pgifDes->top = SwapEndian(pgifDes->top );
                                            pgifDes->width = SwapEndian(pgifDes->width );
                                            pgifDes->height = SwapEndian(pgifDes->height );

                                             inLoop= false;       
                
                                             break;
                                        }            
                                        case 0x3B:  //Terminator Chunk
                                            inLoop= false; 
                                            break;

                                        case 0x21:  //Extension
                                        {
                                        //Read in the extension chunk type.
                                            UINT8 chunkType2 = *pByte;
                                            pByte ++;

                                            switch(chunkType2)
                                            {
                                                case 0xF9:
                                                {
                                                    GifGraphicControlExtension *gce = (GifGraphicControlExtension *)pByte;

                                                    gce->delaytime = SwapEndian(gce->delaytime );
                                                    pByte += sizeof(GifGraphicControlExtension);
                                                    break;
                                                }            
                                                case 0xFE: // Comment Chunk
                                                case 0x01: // Plain Text Chunk
                                                case 0xFF: // APplication Extension Chunk
                                                {
                                                    UINT8 skipUnwanteddata = *pByte;
                                                    pByte +=skipUnwanteddata ;
                                                }
                                                default:
                                                    // Unknown chunk type
                                                    break;
                                            }
                                            break;
                                        }
                                        }
                                    }
                                }

                                if((le_bm->m_flags & CLR_GFX_BitmapDescription::c_Compressed) != 0)
                                {
                                   //if data is compressed, no need to swapendian, as it is 1 byte data and will be converted back to BE/LE at the CLR.
                                }
                                else if (le_bm->m_type == CLR_GFX_BitmapDescription::c_TypeBmp)
                                {
                                    BITMAPFILEHEADER *pbmfh = (BITMAPFILEHEADER *)pByte;

                                    pbmfh->bfType= SwapEndian(pbmfh->bfType);
                                    pbmfh->bfSize= SwapEndian((UINT32)pbmfh->bfSize);
                                    pbmfh->bfReserved1= SwapEndian(pbmfh->bfReserved1);
                                    pbmfh->bfReserved2= SwapEndian(pbmfh->bfReserved2);
                                    pbmfh->bfOffBits= SwapEndian((UINT32)pbmfh->bfOffBits);

                                    pByte += sizeof(BITMAPFILEHEADER);
                                    le_pByte += sizeof(BITMAPFILEHEADER);    

                                    BITMAPINFOHEADER *pbmih = (BITMAPINFOHEADER *)pByte;
                                    BITMAPINFOHEADER *le_pbmih = (BITMAPINFOHEADER *)le_pByte;

                                    pbmih->biSize =SwapEndian((UINT32)pbmih->biSize);
                                    
                                    pbmih->biWidth =SwapEndian((INT32)pbmih->biWidth);
                                    pbmih->biHeight =SwapEndian((INT32)pbmih->biHeight);

                                    pbmih->biPlanes =SwapEndian(pbmih->biPlanes);
                                    pbmih->biBitCount =SwapEndian(pbmih->biBitCount);
                                    pbmih->biCompression =SwapEndian((UINT32)pbmih->biCompression);

                                    pbmih->biSizeImage =SwapEndian((UINT32)pbmih->biSizeImage);
                                    pbmih->biXPelsPerMeter =SwapEndian((INT32)pbmih->biXPelsPerMeter);
                                    pbmih->biYPelsPerMeter =SwapEndian((INT32)pbmih->biYPelsPerMeter);

                                    pbmih->biClrUsed =SwapEndian((UINT32)pbmih->biClrUsed);
                                    pbmih->biClrImportant =SwapEndian((UINT32)pbmih->biClrImportant);

                    
                                    pByte += le_pbmih->biSize;

                                    

                                    if (le_pbmih->biBitCount == 16)
                                    {
                                        if (le_pbmih->biCompression == BI_BITFIELDS)
                                        {
                                            COLORREF *pColor = (COLORREF *)pByte ;
                                            pColor[0] = SwapEndian((UINT32)pColor[0]);
                                            pColor[1] = SwapEndian((UINT32)pColor[1]);
                                            pColor[2] = SwapEndian((UINT32)pColor[2]);
                                            pByte += 3*sizeof(COLORREF);
                                        }

                                    }
                                    else if (le_pbmih->biBitCount == 24)
                                    {
                                    }
                                    else if (le_pbmih->biBitCount == 8)
                                    {
                                    }

                                    UINT32 *pConvertData = (UINT32 *)pByte;
                                    *pConvertData = SwapEndian(*pConvertData);
                                    pByte+=sizeof(UINT32);

                                }
                                else if (le_bm->m_type == CLR_GFX_BitmapDescription::c_TypeTinyCLRBitmap)
                                {


                                    UINT32 sizeColor= (le_bm->m_width) *(le_bm->m_height);

                                    if (le_bm->m_bitsPerPixel ==1)
                                    {
                                        sizeColor /=32;
                                        UINT32 *pConvertData32 ;
                                        for(CLR_UINT32 count=0; count<sizeColor; count++)
                                        {
                                            pConvertData32 = (UINT32 *)pByte;
                                            *pConvertData32 = (UINT32)SwapEndian((UINT32)*pConvertData32);
                                            pByte+=sizeof(UINT32 );
                                        }
                                    
                                    }
                                    else if(le_bm->m_bitsPerPixel ==16)
                                    {
                                        UINT16 *pConvertData16;
                                        for(CLR_UINT32 count=0; count<sizeColor; count++)
                                        {
                                            pConvertData16 = (UINT16 *)pByte;
                                            *pConvertData16 = SwapEndian(*pConvertData16);
                                            pByte+=sizeof(UINT16 );
                                        }
                                    }

                                    
                                }

/*                                UINT32 *pData = (UINT32 *)pByte;
                                for (int count=0; count<curSize ; count -=4)
                                {
                                    *pData = SwapEndian(*pData);
                                    pData++;
                                }
                                 
  */                              


                                break;
                            }

                        }
                        rec->id = SwapEndian((UINT16)rec->id);
                        rec->offset=SwapEndian((UINT32)rec->offset);
                        
                    }
                }

                // Byte width tables:
                //TBL_ResourcesData
                // TBL_Strings
                // TBL_Signatures
                // TBL_ByteCode

                // TBL_ResourcesFiles
                //CLR_RECORD_RESOURCE_FILE - Leave these alone

                ////compute the new CRC.   
                CLR_RECORD_ASSEMBLY* modifiedHeader = (CLR_RECORD_ASSEMBLY*)&bufferBE[ (UINT32)headerLE-bufferLEBase ];
                CLR_UINT32 modifiedSize = modifiedHeader->TotalSize();
                modifiedSize=SwapEndian(modifiedSize);
                modifiedHeader->nativeMethodsChecksum = assm->GenerateSignatureForNativeMethods();
                modifiedHeader->nativeMethodsChecksum = SwapEndian(modifiedHeader->nativeMethodsChecksum);
                
                modifiedHeader->assemblyCRC = SUPPORT_ComputeCRC( &modifiedHeader[1], modifiedSize - sizeof(*modifiedHeader), 0 );

                modifiedHeader->assemblyCRC=SwapEndian(modifiedHeader->assemblyCRC);
                modifiedHeader->headerCRC = 0;
                modifiedHeader->headerCRC   = SUPPORT_ComputeCRC(  modifiedHeader   , sizeof(*modifiedHeader), 0 );
                modifiedHeader->headerCRC=SwapEndian(modifiedHeader->headerCRC);
            }
            bufferBE.resize( bufferLE.size() );
            TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::SaveFile( outFile, bufferBE ));
        }



TinyCLR_Cleanup:

        TINYCLR_RETURN();
    }

    //--//

    HRESULT Cmd_Cfg( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        TINYCLR_CHECK_HRESULT(ExtractOptionsFromFile( PARAM_EXTRACT_STRING( params, 0 ) ));

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_Reset( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        RevertToDefaults();

        TINYCLR_NOCLEANUP_NOLABEL();
    }

    HRESULT Cmd_ResetHints( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        m_col.Clear( true );

        TINYCLR_NOCLEANUP_NOLABEL();
    }

    HRESULT Cmd_LoadHints( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        TINYCLR_CHECK_HRESULT(m_col.LoadHints( PARAM_EXTRACT_STRING( params, 0 ), PARAM_EXTRACT_STRING( params, 1 ) ));

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_IgnoreAssembly( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        TINYCLR_CHECK_HRESULT(m_col.IgnoreAssembly( PARAM_EXTRACT_STRING( params, 0 ) ));

        TINYCLR_NOCLEANUP();
    }

    //--//

    HRESULT Cmd_Parse( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        m_fFromAssembly = true ;
        m_fFromImage    = false;

        if(!m_pr) TINYCLR_CHECK_HRESULT(m_col.CreateAssembly( m_pr ));

        TINYCLR_CHECK_HRESULT(m_pr->Analyze( PARAM_EXTRACT_STRING( params, 0 ) ));

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_VerboseMinimize( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        if(!m_pr) TINYCLR_CHECK_HRESULT(m_col.CreateAssembly( m_pr ));

        m_pr->m_fVerboseMinimize = true;

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_NoByteCode( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        if(!m_pr) TINYCLR_CHECK_HRESULT(m_col.CreateAssembly( m_pr ));

        m_pr->m_fNoByteCode = true;
        m_fNoByteCode       = true;

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_NoAttributes( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        if(!m_pr) TINYCLR_CHECK_HRESULT(m_col.CreateAssembly( m_pr ));

        m_pr->m_fNoAttributes = true;

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_ExcludeClassByName( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        if(!m_pr) TINYCLR_CHECK_HRESULT(m_col.CreateAssembly( m_pr ));

        m_pr->m_setFilter_ExcludeClassByName.insert( PARAM_EXTRACT_STRING( params, 0 ) );

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_Minimize( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        if(!m_pr) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

        TINYCLR_CHECK_HRESULT(m_pr->RemoveUnused());

        TINYCLR_CHECK_HRESULT(m_pr->VerifyConsistency());

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_SaveStrings( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        if(!m_pr) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

        {
            MetaData::Parser prCopy = *m_pr;

            m_lkForStrings.SetBigEndianTarget( m_fBigEndian );
            TINYCLR_CHECK_HRESULT(m_lkForStrings.Process( prCopy ));

            TINYCLR_CHECK_HRESULT(m_lkForStrings.SaveUniqueStrings( PARAM_EXTRACT_STRING( params, 0 ) ));
        }

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_LoadStrings( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        TINYCLR_CHECK_HRESULT(m_lkForStrings.LoadUniqueStrings( PARAM_EXTRACT_STRING( params, 0 ) ));

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_GenerateStringsTable( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        TINYCLR_CHECK_HRESULT(m_lkForStrings.DumpUniqueStrings( PARAM_EXTRACT_STRING( params, 0 ) ));

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_ImportResource( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        m_resources.insert( PARAM_EXTRACT_STRING( params, 0 ) );

        TINYCLR_NOCLEANUP_NOLABEL();
    }

    HRESULT Cmd_Compile( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        if(!m_pr) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

        if(!m_pr) TINYCLR_SET_AND_LEAVE(E_FAIL);
        
        m_pr->m_resources = m_resources; m_resources.clear();

        {
            WatchAssemblyBuilder::Linker             lk;
            WatchAssemblyBuilder::CQuickRecord<BYTE> buf;
            MetaData::Parser                         prCopy = *m_pr;

            std::wstring                             szFile = PARAM_EXTRACT_STRING( params, 0 );


            lk.SetBigEndianTarget( m_fBigEndian );
            lk.LoadGlobalStrings();

            TINYCLR_CHECK_HRESULT(lk.Process( prCopy ));

            TINYCLR_CHECK_HRESULT(lk.Generate( buf, m_patch_fReboot, m_patch_fSign, m_patch_szNative.size() ? &m_patch_szNative : NULL ));

            if(m_fDumpStatistics)
            {
                MetaData::ByteCode::DumpDistributionStats();
            }

            
            if ( m_fLittleEndian )
            {
                TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::SaveFile( szFile.c_str(), (CLR_UINT8*)buf.Ptr(), (DWORD)buf.Size() ));
            }

            if ( m_fBigEndian )
            {
                UINT32 len = buf.Size();
                CLR_RT_Buffer bufferLE;
                CLR_UINT8 *p = (CLR_UINT8*)buf.Ptr();

                for (UINT32 i=0;i<len;i++)
                {
                    bufferLE.push_back( p[i] );
                }
                TINYCLR_CHECK_HRESULT(GenerateOtherEndianPE( bufferLE, szFile.c_str() ));
//                TINYCLR_CHECK_HRESULT(lk.DumpPdbx     ( szFile.c_str() ));
            }
            //TINYCLR_CHECK_HRESULT(lk.DumpDownloads( szFile ));
            TINYCLR_CHECK_HRESULT(lk.DumpPdbx     ( szFile.c_str() ));
        }

        TINYCLR_NOCLEANUP();
    }
    void AppendString( std::string& str, LPCSTR format, ... )
    {
        char    rgBuffer[512];
        LPSTR   szBuffer =           rgBuffer;
        size_t  iBuffer  = MAXSTRLEN(rgBuffer);
        va_list arg;

        va_start( arg, format );

        CLR_SafeSprintfV( szBuffer, iBuffer, format, arg );

        str.append( rgBuffer );
    }

    HRESULT Cmd_Load( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        m_fFromAssembly = false;
        m_fFromImage    = true ;

        TINYCLR_CHECK_HRESULT(AllocateSystem());

        {
            LPCWSTR              szName = PARAM_EXTRACT_STRING( params, 0 );
            CLR_RT_Buffer*       buffer = new CLR_RT_Buffer(); m_assemblies[szName] = buffer;
            CLR_RECORD_ASSEMBLY* header;
            CLR_RT_Assembly*     assm;

            TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( szName, *buffer ));

            header = (CLR_RECORD_ASSEMBLY*)&(*buffer)[0]; TINYCLR_CHECK_HRESULT(CheckAssemblyFormat( header, szName ));

            TINYCLR_CHECK_HRESULT(CLR_RT_Assembly::CreateInstance( header, assm ));

            g_CLR_RT_TypeSystem.Link( assm );
        }

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_LoadDatabase( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        m_fFromAssembly = false;
        m_fFromImage    = true ;

        TINYCLR_CHECK_HRESULT(AllocateSystem());

        {
            LPCWSTR              szFile = PARAM_EXTRACT_STRING( params, 0 );
            CLR_RT_Buffer        buffer;
            CLR_RECORD_ASSEMBLY* header;
            CLR_RECORD_ASSEMBLY* headerEnd;
            std::wstring         strName;

            TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( szFile, buffer ));

            header    = (CLR_RECORD_ASSEMBLY*)&buffer[0              ];
            headerEnd = (CLR_RECORD_ASSEMBLY*)&buffer[buffer.size()-1];

            while(header + 1 <= headerEnd && header->GoodAssembly())
            {
                CLR_RT_Buffer*       bufferSub = new CLR_RT_Buffer();
                CLR_RECORD_ASSEMBLY* headerSub;
                CLR_RT_Assembly*     assm;

                bufferSub->resize( header->TotalSize() );

                headerSub = (CLR_RECORD_ASSEMBLY*)&(*bufferSub)[0];

                if((CLR_UINT8*)header + header->TotalSize() > (CLR_UINT8*)headerEnd)
                {
                    //checksum passed, but not enough data in assembly
                    _ASSERTE(FALSE);
                    break;
                }
                memcpy( headerSub, header, header->TotalSize() );

                TINYCLR_CHECK_HRESULT(CLR_RT_Assembly::CreateInstance( headerSub, assm ));

                g_CLR_RT_TypeSystem.Link( assm );

                CLR_RT_UnicodeHelper::ConvertFromUTF8( assm->m_szName, strName ); m_assemblies[strName] = bufferSub;

                header = (CLR_RECORD_ASSEMBLY*)ROUNDTOMULTIPLE( (size_t)header + header->TotalSize(), CLR_UINT32 );
            }
        }

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_DumpAll( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR szName = PARAM_EXTRACT_STRING( params, 0 );

        if(szName[0] == 0) szName = NULL;

        if(m_fFromAssembly && m_pr)
        {
            m_pr->DumpSchema( szName, m_fNoByteCode );
        }
        else
        {
            TINYCLR_CHECK_HRESULT(AllocateSystem());

            g_CLR_RT_TypeSystem.Dump( szName, m_fNoByteCode );
        }

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_DumpDat( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        m_fFromAssembly = false;
        m_fFromImage    = true ;

        TINYCLR_CHECK_HRESULT(AllocateSystem());

        {
            LPCWSTR              szFile = PARAM_EXTRACT_STRING( params, 0 );
            CLR_RT_Buffer        buffer;
            CLR_RECORD_ASSEMBLY* header;
            CLR_RECORD_ASSEMBLY* headerEnd;
            std::wstring         strName;

            TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( szFile, buffer ));

            header    = (CLR_RECORD_ASSEMBLY*)&buffer[0              ];
            headerEnd = (CLR_RECORD_ASSEMBLY*)&buffer[buffer.size()-1];

            int number = 0;

            while(header + 1 <= headerEnd && header->GoodAssembly())
            {
                CLR_RT_Buffer*       bufferSub = new CLR_RT_Buffer();
                CLR_RECORD_ASSEMBLY* headerSub;
                CLR_RT_Assembly*     assm;

                bufferSub->resize( header->TotalSize() );

                headerSub = (CLR_RECORD_ASSEMBLY*)&(*bufferSub)[0];

                if((CLR_UINT8*)header + header->TotalSize() > (CLR_UINT8*)headerEnd)
                {
                    //checksum passed, but not enough data in assembly
                    _ASSERTE(FALSE);
                    break;
                }
                memcpy( headerSub, header, header->TotalSize() );

                TINYCLR_CHECK_HRESULT(CLR_RT_Assembly::CreateInstance( headerSub, assm ));

                //CLR_RT_UnicodeHelper::ConvertFromUTF8( assm->m_szName, strName ); m_assemblies[strName] = bufferSub;

                printf( "Assembly %d: %s (%d.%d.%d.%d), size: %d\r\n", ++number, assm->m_szName, header->version.iMajorVersion, header->version.iMinorVersion, header->version.iBuildNumber, header->version.iRevisionNumber, header->TotalSize() );

                // jump to next assembly
                header = (CLR_RECORD_ASSEMBLY*)ROUNDTOMULTIPLE( (size_t)header + header->TotalSize(), CLR_UINT32 );
            }
        }

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_DumpExports( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR szName = PARAM_EXTRACT_STRING( params, 0 );

        if(szName[0] == 0) szName = NULL;

        if(m_fFromAssembly && m_pr)
        {
            m_pr->DumpCompact( szName );
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_GenerateSkeleton( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR     szFile = PARAM_EXTRACT_STRING( params, 0 );
        LPCWSTR     szName = PARAM_EXTRACT_STRING( params, 1 );
        LPCWSTR     szProj = PARAM_EXTRACT_STRING( params, 2 );
        LPCWSTR     szLeg  = PARAM_EXTRACT_STRING( params, 3 );


        BOOL fUseOldCodeGen =  _wcsicmp(L"TRUE", szLeg) == 0;
        
        std::string name;

        TINYCLR_CHECK_HRESULT(AllocateSystem());

        if(szFile[0] == 0) szFile = NULL;

        CLR_RT_UnicodeHelper::ConvertToUTF8( szName, name );

        m_assm = g_CLR_RT_TypeSystem.FindAssembly( name.c_str(), NULL, false );
        if(m_assm)
        {
            if(fUseOldCodeGen)
            {
                m_assm->GenerateSkeleton_Legacy( szFile, szProj );
            }
            else
            {
                m_assm->GenerateSkeleton( szFile, szProj );
            }
        }

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_RefreshAssembly( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR     szName = PARAM_EXTRACT_STRING( params, 0 );
        LPCWSTR     szFile = PARAM_EXTRACT_STRING( params, 1 );
        std::string name;

        CLR_RT_UnicodeHelper::ConvertToUTF8( szName, name );

        TINYCLR_CHECK_HRESULT(AllocateSystem());

        m_assm = g_CLR_RT_TypeSystem.FindAssembly( name.c_str(), NULL, false );
        if(m_assm)
        {
            CLR_UINT32 len = m_assm->m_header->TotalSize();

            if(len % sizeof(CLR_UINT32))
            {
                len += sizeof(CLR_UINT32) - (len % sizeof(CLR_UINT32));
            }

            TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::SaveFile( szFile, (CLR_UINT8*)m_assm->m_header, (DWORD)len ));
        }

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_SetEndianness( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();
        int cmpResult;
        LPCWSTR     szEndian = PARAM_EXTRACT_STRING( params, 0 );
        
        cmpResult=wcscmp( szEndian, L"be" );
        if ( 0==cmpResult) 
        {
            m_fBigEndian=true;
        }
        cmpResult=wcscmp( szEndian, L"le" );
        if ( 0==cmpResult) 
        {
            m_fLittleEndian=true;
        }
        cmpResult=wcscmp( szEndian, L"both" );
        if ( 0==cmpResult) 
        {
            m_fLittleEndian=true;
            m_fBigEndian=true;
        }
        TINYCLR_NOCLEANUP_NOLABEL();
    }

#if defined(TINYCLR_JITTER)
    HRESULT Cmd_Jit( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR     szName = PARAM_EXTRACT_STRING( params, 0 );
        std::string name;

        CLR_RT_UnicodeHelper::ConvertToUTF8( szName, name );

        TINYCLR_CHECK_HRESULT(AllocateSystem());

        m_assm = g_CLR_RT_TypeSystem.FindAssembly( name.c_str(), NULL, false );
        if(m_assm)
        {
            for(int i=0; i<m_assm->m_pTablesSize[TBL_MethodDef]; i++)
            {
                CLR_RT_MethodDef_Index md;

                md.Set( m_assm->m_idx, i );

                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.Compile( md, CLR_RT_ExecutionEngine::c_Compile_CPP ));
            }
        }

        TINYCLR_NOCLEANUP();
    }
#endif

    HRESULT Cmd_Resolve( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        bool fError = false;

        TINYCLR_CHECK_HRESULT(AllocateSystem());

        TINYCLR_FOREACH_ASSEMBLY(g_CLR_RT_TypeSystem)
        {
            const CLR_RECORD_ASSEMBLYREF* src = (const CLR_RECORD_ASSEMBLYREF*)pASSM->GetTable( TBL_AssemblyRef );
            for(int i=0; i<pASSM->m_pTablesSize[TBL_AssemblyRef]; i++, src++)
            {
                LPCSTR szName = pASSM->GetString( src->name );

                if(g_CLR_RT_TypeSystem.FindAssembly( szName, &src->version, true ) == NULL)
                {
                    printf( "Missing assembly: %s (%d.%d.%d.%d)\r\n", szName, src->version.iMajorVersion, src->version.iMinorVersion, src->version.iBuildNumber, src->version.iRevisionNumber );

                    fError = true;
                }
            }
        }
        TINYCLR_FOREACH_ASSEMBLY_END();

        if(fError) TINYCLR_SET_AND_LEAVE(CLR_E_ENTRY_NOT_FOUND);

        TINYCLR_CHECK_HRESULT(g_CLR_RT_TypeSystem.ResolveAll());

        TINYCLR_NOCLEANUP();
    }

    //--//

    HRESULT Cmd_GenerateDependency__OutputAssembly( CLR_XmlUtil& xml, IXMLDOMNode* node, IXMLDOMNodePtr& assmNode, LPCWSTR szTag, CLR_RT_Assembly* assm )
    {
        TINYCLR_HEADER();

        std::wstring name;
        WCHAR        rgBuffer[1024];
        bool         fFound;

        CLR_RT_UnicodeHelper::ConvertFromUTF8( assm->m_szName, name );
        swprintf( rgBuffer, ARRAYSIZE(rgBuffer), L"%d.%d.%d.%d", assm->m_header->version.iMajorVersion, assm->m_header->version.iMinorVersion, assm->m_header->version.iBuildNumber, assm->m_header->version.iRevisionNumber );

        TINYCLR_CHECK_HRESULT(xml.CreateNode( szTag, &assmNode, node ));

        TINYCLR_CHECK_HRESULT(xml.PutAttribute( NULL, L"Name"   , name                                                      , fFound, assmNode ));
        TINYCLR_CHECK_HRESULT(xml.PutAttribute( NULL, L"Version", rgBuffer                                                  , fFound, assmNode ));
        TINYCLR_CHECK_HRESULT(xml.PutAttribute( NULL, L"Hash"   , WatchAssemblyBuilder::ToHex( assm->ComputeAssemblyHash() ), fFound, assmNode ));
        TINYCLR_CHECK_HRESULT(xml.PutAttribute( NULL, L"Flags"  , WatchAssemblyBuilder::ToHex( assm->m_header->flags       ), fFound, assmNode ));

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_GenerateDependency( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        LPCWSTR     szFile = PARAM_EXTRACT_STRING( params, 0 );
        CLR_XmlUtil xml;

        TINYCLR_CHECK_HRESULT(xml.New( L"AssemblyGraph" ));

        TINYCLR_FOREACH_ASSEMBLY(g_CLR_RT_TypeSystem)
        {
            IXMLDOMNodePtr assmNode;

            TINYCLR_CHECK_HRESULT(Cmd_GenerateDependency__OutputAssembly( xml, NULL, assmNode, L"Assembly", pASSM ));

            {
                const CLR_RECORD_ASSEMBLYREF* src = (const CLR_RECORD_ASSEMBLYREF*)pASSM->GetTable( TBL_AssemblyRef );
                for(int i=0; i<pASSM->m_pTablesSize[TBL_AssemblyRef]; i++, src++)
                {
                    IXMLDOMNodePtr   assmRefNode;
                    CLR_RT_Assembly* assmRef = g_CLR_RT_TypeSystem.FindAssembly( pASSM->GetString( src->name ), &src->version, true ); if(!assmRef) TINYCLR_SET_AND_LEAVE(CLR_E_NULL_REFERENCE);

                    TINYCLR_CHECK_HRESULT(Cmd_GenerateDependency__OutputAssembly( xml, assmNode, assmRefNode, L"AssemblyRef", assmRef ));
                }
            }

            {
                const CLR_RECORD_TYPEDEF*      src = pASSM->GetTypeDef( 0 );
                CLR_RT_TypeDef_CrossReference* dst = pASSM->m_pCrossReference_TypeDef;

                for(int i=0; i<pASSM->m_pTablesSize[TBL_TypeDef]; i++, src++, dst++)
                {
                    IXMLDOMNodePtr       typeNode;
                    CLR_RT_TypeDef_Index td; td.Set( pASSM->m_idx, i );
                    char                 rgBuffer[512];
                    LPSTR                szBuffer = rgBuffer;
                    size_t               iBuffer  = MAXSTRLEN(rgBuffer);
                    std::wstring         name;
                    bool                 fFound;

                    g_CLR_RT_TypeSystem.BuildTypeName( td, szBuffer, iBuffer );

                    //
                    // Skip types used by the runtime.
                    //
                    if(strchr( rgBuffer, '<' )) continue;
                    if(strchr( rgBuffer, '>' )) continue;
                    if(strchr( rgBuffer, '$' )) continue;

                    CLR_RT_UnicodeHelper::ConvertFromUTF8( rgBuffer, name );

                    TINYCLR_CHECK_HRESULT(xml.CreateNode( L"Type", &typeNode, assmNode ));

                    TINYCLR_CHECK_HRESULT(xml.PutAttribute( NULL, L"Name", name                                      , fFound, typeNode ));
                    TINYCLR_CHECK_HRESULT(xml.PutAttribute( NULL, L"Hash", WatchAssemblyBuilder::ToHex( dst->m_hash ), fFound, typeNode ));
                }
            }
        }
        TINYCLR_FOREACH_ASSEMBLY_END();

        TINYCLR_CHECK_HRESULT(xml.Save( szFile ));

        TINYCLR_NOCLEANUP();
    }

    //--//

    HRESULT Cmd_CreateDatabase( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        CLR_RT_StringVector vec;
        CLR_RT_StringVectorIter iter;
        CLR_RT_StringVectorIter current;
        CLR_RT_Buffer       database;
        size_t              pos;

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::ExtractTokensFromFile( PARAM_EXTRACT_STRING( params, 0 ), vec ));

        // Delete duplicate assemblies
        current = vec.begin();
        while ( current != vec.end() )
        {
            iter = current;
            iter++;
            while ( iter != vec.end() )
            {
                if ( 0 == (*current).compare(*iter) )
                {
                    iter = vec.erase( iter );
                }
                else
                {
                    iter++;
                }
            }
            if (current != vec.end()) current++;
        }



        for(size_t j=0; j<vec.size(); j++)
        {
            CLR_RT_Buffer buffer;

            TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( vec[j].c_str(), buffer ));

            pos = ROUNDTOMULTIPLE(database.size(),CLR_UINT32);

            database.resize( pos + buffer.size() );

            memcpy( &database[pos], &buffer[0], buffer.size() );
        }

        //
        // Add a group of zeros at the end, the device will stop at that point.
        //
        pos = ROUNDTOMULTIPLE(database.size(),CLR_UINT32);
        database.resize( pos + sizeof(CLR_UINT32) );
        {
            TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::SaveFile( PARAM_EXTRACT_STRING( params, 1 ), database ));
        }
        TINYCLR_NOCLEANUP();
    }


    HRESULT Cmd_GenerateKeyPair( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        std::wstring privateKeyFile = PARAM_EXTRACT_STRING( params, 0 );
        std::wstring publicKeyFile  = PARAM_EXTRACT_STRING( params, 1 );

        RSAKey privateKey, publicKey;

        int retries = 100;
        // this call can fail becasuse of crypto API
        // try 100 times
        while(--retries)
        {
            if(GenerateKeyPair( privateKey, publicKey )) break;
        }

        TINYCLR_CHECK_HRESULT(SaveKeyToFile( privateKeyFile, privateKey ));
        TINYCLR_CHECK_HRESULT(SaveKeyToFile( publicKeyFile , publicKey  ));

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_DumpKey( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        unsigned int i;

        std::wstring keyFile = PARAM_EXTRACT_STRING( params, 0 );

        RSAKey key;

        TINYCLR_CHECK_HRESULT(LoadKeyFromFile( keyFile, key ));

        printf( "//typedef struct tagRSAKey\r\n" );
        printf( "//{\r\n" );
        printf( "//   DWORD exponent_len;\r\n" );
        printf( "//   RSABuffer module;\r\n" );
        printf( "//   RSABuffer exponent;\r\n" );
        printf( "//} RSAKey, *PRSAKey;\r\n" );

        printf( "\r\n" );
        printf( "\r\n" );


        printf( "RSAKey myKey =\r\n" );
        printf( "{\r\n" );
        // exponenent length
        printf( "   0x%08x,\r\n", key.exponent_len );

        // module
        printf( "{\r\n" );
        for(i = 0; i < RSA_BLOCK_SIZE_BYTES; ++i)
        {
            printf( "   0x%02x,", key.module[i] );
        }
        printf( "\r\n},\r\n" );

        // exponenent
        printf( "{\r\n" );
        for(i = 0; i < RSA_BLOCK_SIZE_BYTES; ++i)
        {
            printf( "   0x%02x,", key.exponent[i] );
        }
        printf( "\r\n},\r\n" );


        printf( "};\r\n" );

        TINYCLR_NOCLEANUP();
    }


    HRESULT Cmd_SignFile( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        CLR_RT_Buffer buf(RSA_BLOCK_SIZE_BYTES);
        RSAKey        privateKey;
        CLR_RT_Buffer signature(RSA_BLOCK_SIZE_BYTES);

        std::wstring dataFile       = PARAM_EXTRACT_STRING( params, 0 );
        std::wstring privateKeyFile = PARAM_EXTRACT_STRING( params, 1 );
        std::wstring signatureFile  = PARAM_EXTRACT_STRING( params, 2 );

        TINYCLR_CHECK_HRESULT(LoadKeyFromFile( privateKeyFile, privateKey ));

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( dataFile.c_str(), buf ));

        if(!SignData( &buf[0], buf.size(), privateKey, &signature[0], signature.size() ))
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::SaveFile( signatureFile.c_str(), signature ));

        TINYCLR_NOCLEANUP();
    }

    HRESULT Cmd_VerifySignature( CLR_RT_ParseOptions::ParameterList* params = NULL )
    {
        TINYCLR_HEADER();

        CLR_RT_Buffer buf;
        RSAKey        publicKey;
        CLR_RT_Buffer signature;

        std::wstring dataFile      = PARAM_EXTRACT_STRING( params, 0 );
        std::wstring publicKeyFile = PARAM_EXTRACT_STRING( params, 1 );
        std::wstring signatureFile = PARAM_EXTRACT_STRING( params, 2 );

        TINYCLR_CHECK_HRESULT(LoadKeyFromFile( publicKeyFile, publicKey ));

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( dataFile.c_str(), buf ));

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( signatureFile.c_str(), signature ));

        if(!VerifySignature( &buf[0], buf.size(), publicKey, &signature[0], signature.size() ))
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        TINYCLR_NOCLEANUP();
    }


    void Usage()
    {
        wprintf( L"\nMetaDataProcessor.exe\n"            );
        wprintf( L"Available command line switches:\n\n" );

        CLR_RT_ParseOptions::Usage();
    }

    void BuildOptions()
    {
        CLR_RT_ParseOptions::Command*   cmd;
        CLR_RT_ParseOptions::Parameter* param;

        OPTION_SET( &m_fVerbose, L"-verbose", L"Outputs each command before executing it" );

        OPTION_INTEGER( &s_CLR_RT_fTrace_RedirectLinesPerFile, L"-Trace_RedirectLinesPerFile", L"", L"<lines>", L"Lines per File" );
        OPTION_STRING ( &s_CLR_RT_fTrace_RedirectOutput      , L"-Trace_RedirectOutput"      , L"", L"<file>" , L"Output file"    );

        OPTION_SET( &m_fDumpStatistics, L"-ILstats", L"Dumps statistics about IL code" );

        //--//

        OPTION_CALL( Cmd_Reset, L"-reset", L"Clears all previous configuration" );

        //--//

        OPTION_CALL( Cmd_ResetHints, L"-resetHints", L"Clears all previous DLL hints" );

        OPTION_CALL( Cmd_LoadHints, L"-loadHints", L"Loads a specific file as a dependency" );
        PARAM_GENERIC( L"<assembly>", L"Name of the assembly to process"                    );
        PARAM_GENERIC( L"<file>"    , L"File for the assembly"                              );

        //--//

        OPTION_CALL( Cmd_IgnoreAssembly, L"-ignoreAssembly", L"Doesn't include an assembly in the dependencies" );
        PARAM_GENERIC( L"<assembly>", L"Assembly to ignore"                                                     );

        //--//

        OPTION_CALL( Cmd_Parse, L"-parse", L"Analyzes .NET assembly" );
        PARAM_GENERIC( L"<file>", L"File to analyze"                 );

        //--//

        OPTION_SET(    &m_patch_fReboot , L"-patchReboot", L"Marks the patch as needing a reboot"                               );
        OPTION_SET(    &m_patch_fSign   , L"-patchSign"  , L"Sign the patch"                                                    );
        OPTION_STRING( &m_patch_szNative, L"-patchNative", L"ARM code to include in the patch", L"<file>" , L"Native code file" );

        //--//

        OPTION_CALL( Cmd_Cfg, L"-cfg", L"Loads configuration from a file" );
        PARAM_GENERIC( L"<file>", L"Config file to load"                  );

        OPTION_CALL( Cmd_VerboseMinimize, L"-verboseMinimize", L"Turns on verbose level for the minimization phase" );

        OPTION_CALL( Cmd_NoByteCode, L"-noByteCode", L"Skips any ByteCode present in the assembly" );

        OPTION_CALL( Cmd_NoAttributes, L"-noAttributes", L"Skips any attribute present in the assembly" );

        OPTION_CALL( Cmd_ExcludeClassByName, L"-excludeClassByName", L"Removes a class from an assembly" );
        PARAM_GENERIC( L"<class>", L"Class to exclude"                                                   );

        OPTION_CALL( Cmd_Minimize, L"-minimize", L"Minimizes the assembly, removing unwanted elements" );

        OPTION_CALL( Cmd_SaveStrings, L"-saveStrings", L"Saves strings table to a file" );
        PARAM_GENERIC( L"<file>", L"Output file"                                        );

        OPTION_CALL( Cmd_LoadStrings, L"-loadStrings", L"Loads strings table from file" );
        PARAM_GENERIC( L"<file>", L"Input file"                                         );

        OPTION_CALL( Cmd_GenerateStringsTable, L"-generateStringsTable", L"Outputs the collected database of strings" );
        PARAM_GENERIC( L"<file>", L"Output file"                                                                      );

        OPTION_CALL( Cmd_ImportResource, L"-importResource", L"Imports .tinyresources file"   );
        PARAM_GENERIC( L"<file>", L"File to load"                                             );

        OPTION_CALL( Cmd_Compile, L"-compile", L"Compiles an assembly into the TinyCLR format"     );
        PARAM_GENERIC( L"<file>", L"Generated filename"                                            );

        OPTION_CALL( Cmd_Load, L"-load", L"Loads an assembly formatted for TinyCLR" );
        PARAM_GENERIC( L"<file>", L"File to load"                                   );

        OPTION_CALL( Cmd_LoadDatabase, L"-loadDatabase", L"Loads a set of assemblies" );
        PARAM_GENERIC( L"<file>", L"Image to load"                                    );

        OPTION_CALL( Cmd_DumpAll, L"-dump_all", L"Generates a report of an assembly's metadata" );
        PARAM_GENERIC( L"<file>", L"Report file"                                                );

        OPTION_CALL( Cmd_DumpDat, L"-dump_dat", L"dumps the pe files in a dat file together with their size" );
        PARAM_GENERIC( L"<file>", L"Dat file"                                                                );

        OPTION_CALL( Cmd_DumpExports, L"-dump_exports", L"Generates a report of an assembly's metadata, more readable format" );
        PARAM_GENERIC( L"<file>", L"Report file"                                                                          );

        OPTION_CALL( Cmd_GenerateSkeleton, L"-generate_skeleton", L"Generates a skeleton for the methods implemented in native code" );
        PARAM_GENERIC( L"<file>"      , L"Prefix name for the files"                                                                 );
        PARAM_GENERIC( L"<name>"      , L"Name of the assembly"                                                                      );
        PARAM_GENERIC( L"<project>"   , L"Identifier for the library"                                                                );
        PARAM_GENERIC( L"<true|false>", L"Use legacy interop method signature"                                                       );

        OPTION_CALL( Cmd_RefreshAssembly, L"-refresh_assembly", L"Recomputes CRCs for an assembly" );
        PARAM_GENERIC( L"<name>"  , L"Name of the assembly"                                        );
        PARAM_GENERIC( L"<output>", L"Output file"                                                 );

        OPTION_CALL( Cmd_Resolve, L"-resolve", L"Tries to resolve cross-assembly references"       );

        OPTION_CALL( Cmd_SetEndianness, L"-endian", L"Set assembly endian type"                    );
        PARAM_GENERIC( L"<le|be>"  , L"le:little endian (default) be:big endian"                );


#if defined(TINYCLR_JITTER)
        OPTION_CALL( Cmd_Jit, L"-jit", L"Generate JIT code" );
        PARAM_GENERIC( L"<name>", L"Name of the assembly"   );

        OPTION_INTEGER( &s_CLR_RT_fJitter_Trace_Statistics, L"-Jitter_Trace_Statistics", L"", L"<level>", L"Level of verbosity" );
        OPTION_INTEGER( &s_CLR_RT_fJitter_Trace_Compile   , L"-Jitter_Trace_Compile"   , L"", L"<level>", L"Level of verbosity" );
#endif

        OPTION_CALL( Cmd_GenerateDependency, L"-generate_dependency", L"Generate an XML file with the relationship between assemblies" );
        PARAM_GENERIC( L"<file>", L"Output file"                                                                                       );

        //--//

        OPTION_CALL( Cmd_CreateDatabase, L"-create_database", L"Creates file database for a device" );
        PARAM_GENERIC( L"<config>", L"File containing the Bill of Materials"                        );
        PARAM_GENERIC( L"<file>"  , L"Output file"                                                  );

        OPTION_CALL( Cmd_GenerateKeyPair, L"-create_key_pair", L"Creates a pair of private and public RSA keys" );
        PARAM_GENERIC( L"<private key file>", L"Output file containing the private key"                         );
        PARAM_GENERIC( L"<public key file>" , L"Output file containing the public key"                          );

        OPTION_CALL( Cmd_DumpKey, L"-dump_key", L"Dumps the key in the input file in readable format" );
        PARAM_GENERIC( L"<key file>", L"Input file containing the key"                                );

        OPTION_CALL( Cmd_SignFile, L"-sign_file", L"Signs a file with a rivate RSA key" );
        PARAM_GENERIC( L"<file to sign>"    , L"Input File to be signed"                );
        PARAM_GENERIC( L"<private key file>", L"Input file containing the private key"  );
        PARAM_GENERIC( L"<signature file>"  , L"Output file containing the signature"   );

        OPTION_CALL( Cmd_VerifySignature, L"-verify_signature", L"Verifies the signature of a file"   );
        PARAM_GENERIC( L"<signed file>"    , L"Input file for which the signature has been generated" );
        PARAM_GENERIC( L"<public key file>", L"Input file containing the public key"                  );
        PARAM_GENERIC( L"<signature file>" , L"Input file containing the signature"                   );
    }
};

//--//

extern int s_CLR_RT_fTrace_AssemblyOverhead;

//--//

int _tmain(int argc, _TCHAR* argv[])
{
    TINYCLR_HEADER();

    CLR_RT_Assembly::InitString();

    CLR_RT_StringVector vec;
    Settings            st;

    ::CoInitialize( 0 );

    TINYCLR_CHECK_HRESULT(HAL_Windows::Memory_Resize( 64 * 1024 * 1024 ));
    HAL_Init_Custom_Heap();

    // do not display assembly load information
    s_CLR_RT_fTrace_AssemblyOverhead = 0;

    CLR_RT_Memory::Reset         ();    

    st.PushArguments( argc-1, argv+1, vec );

    TINYCLR_CHECK_HRESULT(st.ProcessOptions( vec ));

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        ErrorReporting::Print( NULL, NULL, TRUE, 0, L"%S", CLR_RT_DUMP::GETERRORMESSAGE( hr ) );
        fflush( stdout );
    }

    ::CoUninitialize();

    return FAILED(hr) ? 10 : 0;
}
