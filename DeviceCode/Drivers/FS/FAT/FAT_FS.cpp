////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"
#include "FAT_FS.h"
#include "FAT_FS_Utility.h"
#include "TinyCLR_Interop.h"

//--//

////////////////////////////////////////////////////////////////////////////////////////////////////
//
// FAT_MBR member function
//
////////////////////////////////////////////////////////////////////////////////////////////////////
BOOL FAT_MBR::IsValid()
{
    return (Get_EndingFlag() == 0xaa55) &&
        (Partitions[0].BootIndicator == 0x00 || Partitions[0].BootIndicator == 0x80) &&
        (Partitions[0].Get_RelativeSector() > 0) &&
        (Partitions[0].Get_TotalSector() > 0);
}


////////////////////////////////////////////////////////////////////////////////////////////////////
//
// FAT_DBR member function
//
////////////////////////////////////////////////////////////////////////////////////////////////////
BOOL FAT_DBR::IsValid( BOOL* isFAT16 )
{
    if(!(//check DBR end flag--0x55 AA
        (Get_EndingFlag() == 0xaa55) &&
        //check jump instruction at DBR beginning
        (BS_JmpBoot[0] == 0xeb || BS_JmpBoot[0] == 0xe9) &&
        //now only support FAT FS with 512 bytes per sector
        (Get_BPB_BytsPerSec() == 512) &&
        // Check for SecPerClus, legal values are 1, 2, 4, 8, 16, 32, 64, and 128
        (BPB_SecPerClus != 0 && ((BPB_SecPerClus - 1) & BPB_SecPerClus) == 0) &&
        // Rerserved sector count must be at least 1 (32 typically)
        (Get_BPB_RsvdSecCnt() > 0) &&
        // now only supprt FAT FS with 2 FATs
        (BPB_NumFATs == 2)))
    {
        return FALSE;
    }

    UINT32 countOfClusters = GetCountOfClusters();

    if(countOfClusters < 4085)
    {
        // FAT12, not supported
        return FALSE;
    }
    else if(countOfClusters < 65525)
    {
        // FAT16
        *isFAT16 = TRUE;

        return 
        (
            // now only support FAT16 FS with 512 RootEntCnt
            (Get_BPB_RootEntCnt() == 512) &&
            // FATSz16 needs to be greater than 0 for FAT16
            (Get_BPB_FATSz16() > 0) &&
            // One of TotSec16 or TotSec32 needs to be 0 for FAT16
            (Get_BPB_TotSec16() == 0 || Get_BPB_TotSec32() == 0)
        );
    }
    else
    {
        // FAT32
        *isFAT16 = FALSE;

        return 
        (



            // RootEntCnt needs to be 0 for FAT32
            (Get_BPB_RootEntCnt() == 0) &&
            // TotSec16 needs to be 0 for FAT32
            (Get_BPB_TotSec16() == 0) &&
            // FATSz16 needs to be 0 for FAT32
            (Get_BPB_FATSz16() == 0) &&
            // TotSec32 must be greater than 0
            (Get_BPB_TotSec32() > 0) &&
            // FATSz32 must be greater than 0
            (DBRUnion.FAT32.Get_BPB_FATSz32() > 0)
        );
    }
    

}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
// FAT_FSINFO member function
//
////////////////////////////////////////////////////////////////////////////////////////////////////
void FAT_FSINFO::Initialize( UINT32 freeCount, UINT32 nxtFree )
{
    memset( this, 0, sizeof(FAT_FSINFO) );

    Set_FSI_LeadSig   ((UINT32) 0x41615252);
    Set_FSI_StrucSig  ((UINT32) 0x61417272);
    Set_FSI_Free_Count((UINT32) freeCount);
    Set_FSI_Nxt_free  ((UINT32) nxtFree);
    Set_FSI_TrailSig  ((UINT32) 0xAA550000);
}

BOOL FAT_FSINFO::IsValid()
{
    return(
        (Get_FSI_LeadSig()  == 0x41615252) &&
        (Get_FSI_StrucSig() == 0x61417272) &&
        (Get_FSI_TrailSig() == 0xAA550000)
        );
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
// FAT_Directory member function
//
////////////////////////////////////////////////////////////////////////////////////////////////////
void FAT_Directory::Initialize()
{
    memset( this, 0, sizeof(FAT_Directory) );
}

void FAT_Directory::SetName( LPCWSTR name, UINT32 nameLen )
{
    char c;
    int j;

    // Copy the base name
    for(j = 0; j < SHORTNAME_SIZE && nameLen > 0; j++)
    {
        c = (char)(*name);
        name++;
        nameLen--;

        // stop if we see the "."
        if(c == '.') break;

        if(j == 0 && c == 0xE5) c = 0x05;
    
        DIR_Name[j] = c;
    }

    // Fill the rest of the base name with blanks
    for(; j < SHORTNAME_SIZE; j++)
    {
        DIR_Name[j] = WHITESPACE_CHAR;
    }

    // Only copy the extension if there is one
    if(nameLen > 0)
    {
        // skip the '.' if we haven't already
        if(*name == '.') 
        {
            name++;
            nameLen--;
        }

        // Copy the extension
        for(; j < SHORTNAME_FULL_SIZE && nameLen > 0; j++)
        {
            c = (char)(*name);
            name++;
            nameLen--;
                        
            DIR_Name[j] = c;
        }
    }

    // Fill the rest of the extension with blanks
    for(; j < SHORTNAME_FULL_SIZE; j++)
    {
        DIR_Name[j] = WHITESPACE_CHAR;
    }
}

BOOL FAT_Directory::IsName( LPCWSTR name, UINT32 nameLen )
{
    char c;
    int j;


    // Compare the base name
    for(j = 0; j < SHORTNAME_SIZE && nameLen > 0; j++)
    {
        c = (char)(*name);
        name++;
        nameLen--;


        // stop if we see the "."
        if(c == '.') break;
        
        if(j == 0 && c == 0xE5) c = 0x05;

        if(MF_towupper(DIR_Name[j]) != MF_towupper(c)) return FALSE;
        
    }

    // the rest of the base name has to be blanks
    for(; j < 8; j++)
    {
        if(DIR_Name[j] != WHITESPACE_CHAR) return FALSE;
    }

    // Only compare the extension if there is one
    if(nameLen > 0)
    {
        // skip the '.' if we haven't already
        
        if(*name == '.')
        {
            name++;
            nameLen--;
        }

        // Compare the extension
        for(; j < SHORTNAME_FULL_SIZE && nameLen > 0; j++)
        {
        
            c = (char)(*name);
            name++;
            nameLen--;

            if(MF_towupper(DIR_Name[j]) != MF_towupper(c)) return FALSE;
            
        }
    }

    // the rest of the extension has to be blanks
    for(; j < 11; j++)
    {
    
        if(DIR_Name[j] != WHITESPACE_CHAR) return FALSE;
    }

    return TRUE;
}

void FAT_Directory::CopyName( WCHAR* name )
{
    int j;

    // Copy the base name
    for(j = 0; j < 8; j++)
    {
        if(DIR_Name[j] == WHITESPACE_CHAR) break;

        *name = (j == 0 && DIR_Name[j] == 0x05) ? 0xE5 : DIR_Name[j];
        
        name++;
    }

    // Only add the dot if there's an extension
    if(DIR_Name[8] != WHITESPACE_CHAR)
    {
        *name = '.';
        name++;
    }

    // Copy the extension
    for(j = 8; j < 11; j++)
    {        
        if(DIR_Name[j] == WHITESPACE_CHAR) break;
        
        *name = DIR_Name[j];
        name++;
    }

    // Terminate the string
    *name = 0;
}

UINT32 FAT_Directory::GetNameLength()
{
    int j;
    UINT32 len = 0;

    // Count the base name
    for(j = 0; j < 8; j++)
    {
        if(DIR_Name[j] == WHITESPACE_CHAR) break;
        len++;
    }

    // Only add the dot if there's an extension
    if(DIR_Name[8] != WHITESPACE_CHAR)
    {
        len++;
    }

    // Count the extension
    for(j = 8; j < 11; j++)
    {        
        if(DIR_Name[j] == WHITESPACE_CHAR) break;
        len++;
    }

    return len;
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
// FAT_LONG_Directory member function
//
////////////////////////////////////////////////////////////////////////////////////////////////////
void FAT_LONG_Directory::Initialize( BYTE ord, LPCWSTR name, UINT32 nameLen, BYTE chksum )
{
    LDIR_Ord = ord;
    LDIR_Chksum = chksum;
    LDIR_Attr = ATTR_LONG_NAME;
    LDIR_Type = 0;
    Set_LDIR_FstClusLO((UINT16) 0);

    SetName( name, nameLen );
}

void FAT_LONG_Directory::SetName( LPCWSTR name, UINT32 nameLen )
{
    WCHAR tempName[13];
    
    if(nameLen < 13)
    {
        UINT32 i;
        for(i = 0; i < nameLen; i++)
        {
            tempName[i] = name[i];
        }
        
        tempName[i] = 0;
        i++;
        
        for(; i < 13; i++)
        {
            tempName[i] = 0xFFFF;
        }

        name = tempName;
    }
    
    int j;
        
    for(j = 0; j < LDIR_Name1__size; j +=2)
    {
        // LDIR_Name1 is in BYTEs because it's not UINT16 aligned
        LDIR_Name1[j  ] = (BYTE)(*name &  0xFF);
        LDIR_Name1[j+1] = (BYTE)(*name >>    8);

        name++;
    }

    for(j = 0; j < LDIR_Name2__size; j+=2)
    {
        LDIR_Name2[j  ] = (BYTE)(*name &  0xFF);
        LDIR_Name2[j+1] = (BYTE)(*name >>    8);
        name++;
    }


    for(j = 0; j < LDIR_Name3__size ; j+=2)
    {
        LDIR_Name3[j  ] = (BYTE)(*name &  0xFF);
        LDIR_Name3[j+1] = (BYTE)(*name >>    8);
        
        name++;
    }
}

BOOL FAT_LONG_Directory::IsName( LPCWSTR name, UINT32 nameLen )
{
    UINT32 j;
    WCHAR c;
    
    for(j = 0; j < LDIR_Name1__size; j+=2)
    {
        // LDIR_Name1 is in BYTEs because it's not UINT16 aligned
        c = ((UINT16)LDIR_Name1[j]) | ((UINT16)LDIR_Name1[j+1]) << 8;
        
        if(c == 0 && nameLen == 0)
        {
            return TRUE;
        }
        else if(nameLen == 0 || MF_towupper( c ) != MF_towupper( *name ))
        {
            return FALSE;
        }

        name++;
        nameLen--;
    }

    for(j = 0; j < LDIR_Name2__size; j+=2)
    {
        c = ((UINT16)LDIR_Name2[j]) | ((UINT16)LDIR_Name2[j+1]) << 8;
    
        if(c == 0 && nameLen == 0)
        {
            return TRUE;
        }
        else if(nameLen == 0 || MF_towupper( c ) != MF_towupper( *name ))
        {
            return FALSE;
        }
        
        name++;
        nameLen--;
    }


    for(j = 0; j < LDIR_Name3__size; j+=2)
    {
        c = ((UINT16)LDIR_Name3[j]) | ((UINT16)LDIR_Name3[j+1]) << 8;

        if(c == 0 && nameLen == 0)
        {
            return TRUE;
        }
        else if(nameLen == 0 || MF_towupper( c ) != MF_towupper( *name ))
        {
            return FALSE;
        }
        
        name++;
        nameLen--;
    }

    return TRUE;
}

void FAT_LONG_Directory::CopyName( WCHAR* name )
{
    int j;
        

    for(j = 0; j < LDIR_Name1__size; j+=2)        
    {
        // LDIR_Name1 is in BYTEs because it's not UINT16 aligned

        *name = ((UINT16)LDIR_Name1[j]) | (((UINT16)LDIR_Name1[j+1]) << 8);
        
        if(*name == 0)
        {
            return;
        }

        name++;
    }

    for(j = 0; j < LDIR_Name2__size; j+=2)
    {
        *name = ((UINT16)LDIR_Name2[j]) | (((UINT16)LDIR_Name2[j+1]) << 8);

        if(*name == 0)
        {
            return;
        }
        
        name++;
    }

    for(j = 0; j < LDIR_Name3__size; j+=2)
    {
        *name = ((UINT16)LDIR_Name3[j]) | (((UINT16)LDIR_Name3[j+1]) << 8);

        if(*name == 0)
        {
            return;
        }
        
        name++;
    }
}

UINT32 FAT_LONG_Directory::GetNameLength()
{    
    int j;
    
    UINT32 len = 0;
    
    for(j = 0; j < LDIR_Name1__size; j+=2)
    {
        if(LDIR_Name1[j] == 0 && LDIR_Name1[j+1] == 0)
        {
            return len;
        }
        len++;
    }

    for(j = 0; j < LDIR_Name2__size; j+=2)
    {
        if(LDIR_Name2[j] == 0 && LDIR_Name2[j+1] == 0)
        {
            return len;
        }
        len++;
    }

    for(j = 0; j < LDIR_Name3__size; j+=2)
    {
        if(LDIR_Name3[j] == 0 && LDIR_Name3[j+1] == 0)
        {
            return len;
        }
        len++;
    }

    return len;
}


//--//

FAT_FINDFILES* FAT_FINDFILES::FindOpen( FAT_LogicDisk* logicDisk, UINT32 clusIndex )
{
    FAT_FINDFILES* findFiles = (FAT_FINDFILES*)FAT_MemoryManager::AllocateHandle();

    if(findFiles == NULL)
        return NULL;

    findFiles->m_entryEnum.Initialize( logicDisk, logicDisk->ClusToSect( clusIndex ), 0 );

    findFiles->m_logicDisk = logicDisk;

    return findFiles;
}

HRESULT FAT_FINDFILES::FindNext( FS_FILEINFO *fi, BOOL *fileFound )
{
    TINYCLR_HEADER();
    
    FAT_FILE fileInfo;
    FAT_Directory* dirEntry;
    
    if(!fi || !fileFound)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
    }

    do
    {
        if(fileInfo.Parse( m_logicDisk, &m_entryEnum ) != S_OK)
        {
            *fileFound = FALSE;
            TINYCLR_SET_AND_LEAVE(S_OK);
        }
        
        dirEntry = fileInfo.GetDirectoryEntry( FALSE );

        if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
    }
    while(dirEntry->DIR_Name[0] == '.' || dirEntry->DIR_Attr == ATTR_VOLUME_ID); // Skip the "." and ".." and the Volume_ID entries

    fi->Attributes     = dirEntry->DIR_Attr;
    fi->Size           = dirEntry->Get_DIR_FileSize();
    fi->CreationTime   = FAT_Utility::FATTimeToTicks( dirEntry->Get_DIR_CrtDate()   , dirEntry->Get_DIR_CrtTime(), dirEntry->DIR_CrtTimeTenth );
    fi->LastAccessTime = FAT_Utility::FATTimeToTicks( dirEntry->Get_DIR_LstAccDate(), 0                    , 0                          );
    fi->LastWriteTime  = FAT_Utility::FATTimeToTicks( dirEntry->Get_DIR_WrtDate()   , dirEntry->Get_DIR_WrtTime(), 0                          );

    TINYCLR_CHECK_HRESULT(fileInfo.CopyFileName( (LPWSTR)(fi->FileName), fi->FileNameSize ));

    *fileFound = TRUE;

    TINYCLR_NOCLEANUP();
}

HRESULT FAT_FINDFILES::FindClose()
{
    FAT_MemoryManager::FreeHandle( this );
    return S_OK;
}

//--//

void FAT_EntryEnumerator::Initialize( FAT_LogicDisk* logicDisk, UINT32 sectIndex, UINT32 dataIndex, BOOL extend )
{
    m_logicDisk = logicDisk;
    m_clusIndex = logicDisk->SectToClus( sectIndex );
    m_sectIndex = sectIndex;
    m_dataIndex = dataIndex;

    m_flag = Flag_First;

    if(extend) m_flag |= Flag_Extend;
}

FAT_Directory* FAT_EntryEnumerator::GetNext( BOOL forWrite )
{
    bool fFirst = false;

    if((m_flag & Flag_First) == Flag_First)
    {
        fFirst  = true;
        m_flag &= ~Flag_First;

    }
    else if((m_flag & Flag_Done) == Flag_Done)
    {
        return NULL;
    }
    else
    {
        //move to next dir_entry position
        m_dataIndex += sizeof(FAT_Directory);

        if(m_dataIndex >= m_logicDisk->m_bytesPerSector)
        {
            UINT32 flags = (m_flag & Flag_Extend) ? FAT_LogicDisk::GetNextSect__CREATE | FAT_LogicDisk::GetNextSect__CLEAR : FAT_LogicDisk::GetNextSect__NONE;
            if(m_logicDisk->GetNextSect( &m_clusIndex, &m_sectIndex, flags ) != S_OK)
            {

                m_flag |= Flag_Done;

                return NULL;

            }

            m_dataIndex = 0;
        }
    }

    return (FAT_Directory*)(&(m_logicDisk->SectorCache.GetSector( m_sectIndex, fFirst || forWrite, forWrite ))[m_dataIndex]);
}

void FAT_EntryEnumerator::GetIndices( UINT32* sectIndex, UINT32* dataIndex )
{
    *sectIndex = m_sectIndex;
    *dataIndex = m_dataIndex;
}

