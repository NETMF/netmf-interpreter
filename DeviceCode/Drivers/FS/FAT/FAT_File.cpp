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
// FAT_FILE member function
//
////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT FAT_FILE::Create( FAT_LogicDisk* logicDisk, UINT32 clusIndex, LPCWSTR fileName, UINT32 fileNameLen, BYTE attributes )
{
    TINYCLR_HEADER();
    FAT_Directory*      dirEntry;
    FAT_LONG_Directory* lDirEntry;
    
    UINT32 sectIndex ;
    UINT32 dataIndex = 0;

    char    shortName[11];
    int     entryCount = 1;
    BOOL    needsLongName;
    UINT32  fstClus = 0;
    BYTE    chksum = 0;
    LPCWSTR namePortion = NULL;
    FAT_EntryEnumerator entryEnum;

    
    if(!IsFileNameValid( fileName, fileNameLen ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
    }
    
    
    sectIndex = logicDisk->ClusToSect( clusIndex );
    

    m_logicDisk      = logicDisk;
    m_fileNameLength = fileNameLen;


    needsLongName = IsFileLongName( fileName, fileNameLen );


    if(needsLongName)
    {
        // Prep the info needed for long file name
        entryCount += (m_fileNameLength + 12) / 13;
        namePortion = fileName + 13 * (entryCount - 2);

        TINYCLR_CHECK_HRESULT(LongToShortName( clusIndex, fileName, fileNameLen, shortName ));
        chksum = GetShortNameChksum( shortName );
    }
    
    TINYCLR_CHECK_HRESULT(m_logicDisk->GetNextFreeEntry( &sectIndex, &dataIndex, entryCount ));

    // Once we get the entries, we should allocate the cluster for directory(if it's one)
    if((attributes & ATTR_DIRECTORY) == ATTR_DIRECTORY)
    {
        fstClus = CreateNewDirectory( clusIndex );

        if(fstClus == CLUST_ERROR) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
    }


    entryEnum.Initialize( m_logicDisk, sectIndex, dataIndex );

    m_lDirSectIndex = sectIndex;
    m_lDirDataIndex = dataIndex;
    m_lDirCount     = entryCount - 1;

    // Fill in the long directory entries first
    for(int i = m_lDirCount; i >= 1; i--, namePortion -= 13)
    {
        lDirEntry = (FAT_LONG_Directory*)entryEnum.GetNext( TRUE );

        if(!lDirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

        if(i == m_lDirCount) // the first time
        {
            lDirEntry->Initialize( i | LAST_LONG_ENTRY, namePortion, ((m_fileNameLength - 1) % 13) + 1, chksum );
        }
        else
        {
            lDirEntry->Initialize( i, namePortion, 13, chksum );
        }
    }


    // Fill out the short directory entry
    dirEntry = entryEnum.GetNext( TRUE );

    if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

    entryEnum.GetIndices( &m_dirSectIndex, &m_dirDataIndex );

    dirEntry->Initialize();
    dirEntry->DIR_Attr = attributes;
    dirEntry->SetFstClus( fstClus );

    if(needsLongName)
    {
        memcpy( dirEntry->DIR_Name, shortName, 11 );
    }
    else
    {
        dirEntry->SetName( fileName, fileNameLen );
    }

    UINT16 date,time;
    UINT8 timeTenth;

    FAT_Utility::GetCurrentFATTime( &date, &time, &timeTenth );

    dirEntry->Set_DIR_CrtDate ( date );
    dirEntry->Set_DIR_CrtTime ( time );
    dirEntry->DIR_CrtTimeTenth = timeTenth;
    dirEntry->Set_DIR_WrtDate ( date );
    dirEntry->Set_DIR_WrtTime ( time );
    dirEntry->Set_DIR_LstAccDate(date);

    TINYCLR_NOCLEANUP();
}

HRESULT FAT_FILE::Parse( FAT_LogicDisk* logicDisk, FAT_EntryEnumerator* entryEnum )
{
    TINYCLR_HEADER();
    
    FAT_Directory* dirEntry;
    FAT_LONG_Directory* lDirEntry;
    BYTE checksum = 0;


    // Skip through all the deleted entries
    do
    {
        dirEntry = entryEnum->GetNext();
    }
    while(dirEntry && dirEntry->DIR_Name[0] == SLOT_DELETED);
    
    //this dir_entry is empty
    if(!dirEntry || dirEntry->DIR_Name[0] == SLOT_EMPTY)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
    }

    //long directory
    lDirEntry = (FAT_LONG_Directory*)dirEntry;


    // Regardless of having long directory entries, lDirSectIndex and lDirDataIndex is where the dirEntries begin
    entryEnum->GetIndices( &m_lDirSectIndex, &m_lDirDataIndex );


    if(((lDirEntry->LDIR_Ord & LAST_LONG_ENTRY) == LAST_LONG_ENTRY) && ((lDirEntry->LDIR_Attr & ATTR_LONG_NAME_MASK) == ATTR_LONG_NAME))
    {
        m_lDirCount = lDirEntry->LDIR_Ord & 0x3F;

        if(m_lDirCount > MAX_LONGENTRY_COUNT)
        {

            TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);
        }

        checksum = lDirEntry->LDIR_Chksum;

        m_fileNameLength = lDirEntry->GetNameLength();


        for(int i = m_lDirCount - 1; i >= 1; i--)
        {
            lDirEntry = (FAT_LONG_Directory*)entryEnum->GetNext();

            if(!lDirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

            if(lDirEntry->LDIR_Chksum != checksum || lDirEntry->LDIR_Ord != i)
            {
                // The checksum or order is incorrect, so ignore all the long directory entries
                m_lDirCount = 0;
            }
        }

        m_fileNameLength += (m_lDirCount - 1) * 13;


        dirEntry = entryEnum->GetNext();   // dirEntry / entryEnum is now at the short directory entry


        if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

    }
    else
    {
        m_lDirCount = 0;
        m_fileNameLength = 0;
    }

    //short directory

    entryEnum->GetIndices( &m_dirSectIndex, &m_dirDataIndex );

    if(m_lDirCount > 0)
    {        
    
        if(checksum != GetShortNameChksum( (LPCSTR)dirEntry->DIR_Name ))
        {
            m_lDirCount = 0;
        }
    }


    if(m_lDirCount == 0)
    {
        m_fileNameLength = dirEntry->GetNameLength();
    }

    m_logicDisk = logicDisk;


    TINYCLR_NOCLEANUP();
}

FAT_Directory* FAT_FILE::GetDirectoryEntry( BOOL forWrite )
{
    BYTE* sector = m_logicDisk->SectorCache.GetSector( m_dirSectIndex, forWrite );

    if(sector)
    {
        return (FAT_Directory*)&sector[m_dirDataIndex];
    }

    return NULL;
}

void FAT_FILE::MarkDirectoryEntryForWrite()
{
    m_logicDisk->SectorCache.MarkSectorDirty( m_dirSectIndex );
}

HRESULT FAT_FILE::DeleteDirectoryEntry()
{
    FAT_EntryEnumerator entryEnum;
    FAT_Directory* dirEntry;
    entryEnum.Initialize( m_logicDisk, m_lDirSectIndex, m_lDirDataIndex );

    for(int i = m_lDirCount + 1; i > 0; i--)
    {
        dirEntry = entryEnum.GetNext( TRUE );

        if(!dirEntry) return CLR_E_FILE_IO;

        dirEntry->DIR_Name[0] = SLOT_DELETED;
    }

    return S_OK;
}

BOOL FAT_FILE::IsFileName( LPCWSTR name, UINT32 nameLen )
{
    // If there is a long name and the lengths are the same
    if(m_lDirCount > 0 && nameLen == m_fileNameLength)
    {        
        FAT_EntryEnumerator entryEnum;
        
        LPCWSTR namePortion = name + 13 * (m_lDirCount - 1);
        UINT32  portionLen  = ((nameLen - 1) % 13) + 1; // portionlen will be from 1 - 13
        FAT_LONG_Directory* lDirEntry;

        entryEnum.Initialize( m_logicDisk, m_lDirSectIndex, m_lDirDataIndex );

        for(UINT32 i = m_lDirCount; i > 0; i--)
        {
            lDirEntry = (FAT_LONG_Directory*)entryEnum.GetNext();


            if(!lDirEntry) return FALSE;

            if(!lDirEntry->IsName( namePortion, portionLen ))
            {                    

                goto COMPARE_SHORT_NAME;
            }
            
            namePortion -= 13;
            portionLen   = 13;
        }


        // If we reach here, it means all the IsName passed and the long name is a match
        return TRUE;
    }

COMPARE_SHORT_NAME:
    
    if(nameLen <= 12 && nameLen == m_fileNameLength) // 8.3 (with the dot)
    {
        FAT_Directory* dirEntry = GetDirectoryEntry( FALSE );

        if(!dirEntry) return FALSE;

        return dirEntry->IsName( name, nameLen );
    }

    return FALSE;
}

HRESULT FAT_FILE::CopyFileName( LPWSTR name, UINT32 nameLen )
{
    TINYCLR_HEADER();
    
    if(nameLen <= m_fileNameLength) // len needs to be at least m_fileNameLength + 1 (null terminator)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_PATH_TOO_LONG);
    }
    
    if(m_lDirCount > 0) // copy the long name if there's one
    {        
        FAT_EntryEnumerator entryEnum;
        
        WCHAR* namePortion = name + 13 * (m_lDirCount - 1);
        FAT_LONG_Directory* lDirEntry;
        
        entryEnum.Initialize( m_logicDisk, m_lDirSectIndex, m_lDirDataIndex );

        for(UINT32 i = m_lDirCount; i > 0; i--)
        {
            lDirEntry = (FAT_LONG_Directory*)entryEnum.GetNext();

            if(!lDirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

            lDirEntry->CopyName( namePortion );
            
            namePortion -= 13;
        }

        if(m_fileNameLength % 13 == 0)
        {
            // If the name fits exactly into an entry, it won't be terminated, so we do it here.
            name[m_fileNameLength] = 0;
        }
    }
    else // copy the short name
    {
        FAT_Directory* dirEntry = GetDirectoryEntry( FALSE );

        if(!dirEntry) TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

        dirEntry->CopyName( name );
    }

    TINYCLR_NOCLEANUP();
}


/////////////////////////////////////////////////////////////////////
// Description:
//     verify whether FileName contains invalid char
// 
// Input:
//     FileName-- 
//
// output:
//   
// Remarks:
//       
// Returns:   
//      
BOOL FAT_FILE::IsFileNameValid( LPCWSTR fileName, UINT32 fileNameLen )
{
    const WCHAR InvalidSymbols[] = {'*', '|', '\\', ':', '\"', '<', '>', '?', '/' };    
    const int InvalidSymbolsSize = ARRAYSIZE(InvalidSymbols);

    if(fileName[0] == '\0')
        return FALSE;

    WCHAR c;
    UINT32   i;
    int   j;

    for(i = 0; i < fileNameLen; i++)
    {
        c = fileName[i];

        if(c < 0x20) return FALSE;

        for(j = 0; j < InvalidSymbolsSize; j++)
        {
            if(c == InvalidSymbols[j])
                return FALSE;
        }
    }
    return TRUE;
}


/////////////////////////////////////////////////////////////////////
// Description:
//     verify whether need long directory for file entry
// 
// Input:
//     FileName-- 
//
// output:
//   
// Remarks:
//       
// Returns:   
//      
BOOL FAT_FILE::IsFileLongName( LPCWSTR fileName, UINT32 fileNameLen )
{
    const WCHAR InvalidSymbols[] = { ' ', '+', ',', ';', '=', '[', ']' };
    const int InvalidSymbolsSize = ARRAYSIZE(InvalidSymbols);

    UINT32  extLen;
    LPCWSTR ext         = GetFileNameExt( fileName, fileNameLen, &extLen );
    UINT32  baseNameLen = (extLen > 0) ? fileNameLen - extLen - 1 : fileNameLen;

    // if length is more than 8.3, we need long name
    if(baseNameLen > 8 || extLen > 3)
        return TRUE;
    
    //--//

    WCHAR c;
    UINT32   i;
    int   j;

    int dotCount = 0;

    for(i = 0; i < fileNameLen; i++)
    {
        c = fileName[i];

        // Keep track of the number of '.' we've seen
        if(c == '.')
        {
            dotCount++;
            continue;
        }

        // if at any point we discovered lower case letters -- to preserve casing we need long names 
        // We also check for any non-ANSI characters
        if((c >= 'a' && c <= 'z') || c > 255) return TRUE;

        for(j = 0; j < InvalidSymbolsSize; j++)
        {
            if(c == InvalidSymbols[j])
                return TRUE;
        }
    }

    // In shortname we're only allowed one '.' (between basename and ext), and never in the first char
    if(dotCount > 1 || fileName[0] == '.')
    {
        return TRUE;
    }
    
    return FALSE;
}

BOOL FAT_FILE::GenerateBasisName( LPCWSTR fileName, UINT32 fileNameLen, LPSTR basisName )
{
    UINT32 baseLen;
    UINT32 extLen;
    
    LPCWSTR fileNameExt = GetFileNameExt( fileName, fileNameLen, &extLen );

    baseLen = (extLen > 0) ? fileNameLen - extLen - 1 : fileNameLen;
    
    BOOL needsTrail = FALSE;

    if(baseLen == 0)
    {
        // if the filename is ".something" we'll make the "extension" the actual name
        fileName++;
        baseLen = extLen;
        extLen = 0;
    }

    UINT32 i;
    UINT32 j = 0;
    
    for(i = 0; i < baseLen && j < 8; i++)
    {
        if(SetBasisNameChar( &basisName[j], fileName[i], &needsTrail ))
        {
            j++;
        }
    }

    for(; j < 8; j++)
    {
        basisName[j] = WHITESPACE_CHAR;
    }

    for(i = 0; i < extLen && j < 11; i++)
    {
        if(SetBasisNameChar( &basisName[j], fileNameExt[i], &needsTrail ))
        {
            j++;
        }
    }

    for(; j < 11; j++)
    {
        basisName[j] = WHITESPACE_CHAR;
    }

    // If the name is longer than standard 8.3, set needsTrail to TRUE
    if(!needsTrail && (baseLen > 8 || extLen > 3))
    {
        needsTrail = TRUE;
    }

    return needsTrail;
}

BOOL FAT_FILE::SetBasisNameChar( LPSTR basis, WCHAR c, BOOL* needsTrail )
{
    if(c >= 'a' && c <= 'z')
    {
        *basis = c - ('a' - 'A');
        return TRUE;
    }
    else if(c > 255 || c == '+' || c == ',' || c == ';' || c == '=' || c == '[' || c == ']')
    {
        *basis = '_';
        *needsTrail = TRUE;
        return TRUE;
    }
    else if(c != WHITESPACE_CHAR && c != '.')
    {            
        *basis = (char)c;        
        return TRUE;
    }

    *needsTrail = TRUE;
    return FALSE;
}

/////////////////////////////////////////////////////////////////////
// Description:
//     get numberic tail in one short name
// 
// Input:
//     FileName-- 
// output:
//   
// Remarks:
//       
// Returns:   
//      
BOOL FAT_FILE::CollectNumTail( LPCSTR fileName, LPCSTR basisName, UINT32 *availableMap )
{
    int i;

    // Compare the extension first, that's the easiest
    for(i = 8; i < 11; i++)
    {
        if(fileName[i] != basisName[i]) // if it doesn't match, we don't need to consider this file
        {
            return FALSE;
        }
    }

    int numStart = 0;

    // Everything before '~' has to match exactly
    for(i = 0; i < 8; i++)
    {        
        if(fileName[i] != basisName[i]) // if it doesn't match
        {
            if(fileName[i] == '~') // is it because we hit the '~' ?
            {
                if(basisName[i] == WHITESPACE_CHAR || fileName[7] != 0x20) // make sure we didn't find it too early
                {
                    numStart = i;
                    break;
                }
            }
            return FALSE; // if not, we're done
        }
    }

    // if we don't find a '~', 
    if(numStart == 0)
    {
        return (i == 8); // if i == 8 it means that fileName == basisName
    }

    numStart++;

    UINT32 num = 0;
    char c;
    
    for(i = numStart; i < 8; i++)
    {
        c = fileName[i];
        
        if(c == WHITESPACE_CHAR) // the filename is done when we see a space
            break;
        
        if(c < '0' || c > '9') // the trail is not a number
            return FALSE;

        num *= 10;
        num += c - '0';
    }

    if(num <= SHORTNAME_CONVERSION_MAP_SIZE)
    {
        num--;

        availableMap[num / 32] |= 1 << (num % 32);
    }

    return FALSE;
}




/////////////////////////////////////////////////////////////////////
// Description:
//     transfer long file name to short name
// 
// Input:
//   
// output:
//   
// Remarks:
//       count existed same short name count(ex. abcdef~3.txt)
// Returns:   
//      
HRESULT FAT_FILE::LongToShortName( UINT32 clusIndex, LPCWSTR longName, UINT32 longNameLength, LPSTR shortName )
{
    BOOL needsTail = GenerateBasisName( longName, longNameLength, shortName );

    FAT_EntryEnumerator entryEnum;
    FAT_Directory*      dirEntry;
    FAT_FILE            fileInfo;

    
    UINT32 availableMap[SHORTNAME_CONVERSION_MAP_SIZE / 32];
    int i;

    for(i = 0; i < SHORTNAME_CONVERSION_MAP_SIZE / 32; i++)
    {
        availableMap[i] = 0;
    }
    
    entryEnum.Initialize( m_logicDisk, m_logicDisk->ClusToSect( clusIndex ), 0 );

    // Go through all the files in the directory, and collect all the numeric tails
    while(fileInfo.Parse( m_logicDisk, &entryEnum ) == S_OK)
    {
        dirEntry = fileInfo.GetDirectoryEntry( FALSE );
        
        if(!dirEntry) return CLR_E_FILE_IO;

        // Skip over the volume label file
        if(dirEntry->DIR_Attr == ATTR_VOLUME_ID)
        {
            continue;
        }

        // this will returns TRUE if it's an exact match, which means we'll definitly needs trail
        needsTail = CollectNumTail( (LPCSTR)dirEntry->DIR_Name, shortName, availableMap ) || needsTail;
    }

    if(needsTail)
    {
        BYTE map;
        UINT32 num = 1;

        // Go through the map and find the first bit that's not set.
        for(i = 0; i < SHORTNAME_CONVERSION_MAP_SIZE / 8; i++)
        {
            map = ((BYTE*)availableMap)[i];
            if(map != 0xFF)
            {
                while(map & 0x1)
                {
                    map = map >> 1;
                    num++;
                }
                break;
            }
            num += 8;
        }

        if(num > SHORTNAME_CONVERSION_MAP_SIZE) // Unfortunately all the number up to SHORTNAME_CONVERSION_MAP_SIZE is taken
        {
            // Brute force method!!
            char candidate[SHORTNAME_FULL_SIZE];

            while(num <= 999999)
            {
                // Start with the short name
                memcpy( candidate, shortName, SHORTNAME_FULL_SIZE );

                // tack on the numeric tail
                AttachNumTail( candidate, num++ );

                // and see if there's another file with the same name
                entryEnum.Initialize( m_logicDisk, m_logicDisk->ClusToSect( clusIndex ), 0 );
                
                while(fileInfo.Parse( m_logicDisk, &entryEnum ) == S_OK)
                {
                    dirEntry = fileInfo.GetDirectoryEntry( FALSE );
                    
                    if(!dirEntry) return CLR_E_FILE_IO;

                    // Skip over the volume label file
                    if(dirEntry->DIR_Attr == ATTR_VOLUME_ID)
                    {
                        continue;
                    }

                    if(hal_strncmp_s( (LPCSTR)dirEntry->DIR_Name, shortName, SHORTNAME_FULL_SIZE ) == 0)
                    {
                        continue;
                    }
                }

                // found it!! 
                memcpy( shortName, candidate, SHORTNAME_FULL_SIZE );
                return S_OK;
            }
            
        }
        else
        {
            AttachNumTail( shortName, num );
        }
    }

    return S_OK;
}

void FAT_FILE::AttachNumTail( LPSTR shortName, UINT32 num )
{
    int i;
    int shift;

    // convert num to string -- Start from the end of shortName and move back
    for(i = SHORTNAME_SIZE - 1; num > 0; i--)
    {
        shortName[i] = (num % 10) + '0';
        num /= 10;
    }

    // add the "~"
    shortName[i] = '~';
    i--;

    // If there are spaces between the numeric tail and the base name, figure out the shift amount
    for(shift = 0; shortName[i] == WHITESPACE_CHAR; i--, shift++);

    if(shift > 0)
    {
        i++; // i is now at the first white space

        // shift the numeric tail over
        for(; i + shift < SHORTNAME_SIZE; i++)
        {
            shortName[i] = shortName[i + shift];
        }

        // fill the rest with blanks
        for(; i < SHORTNAME_SIZE; i++)
        {
            shortName[i] = WHITESPACE_CHAR;
        }
    }
}

UINT32 FAT_FILE::CreateNewDirectory( UINT32 parentClusIndex )
{
    UINT32 clusIndex = m_logicDisk->GetNextFreeClus( TRUE );
    
    if(clusIndex == CLUST_ERROR) return CLUST_ERROR;
    
    m_logicDisk->WriteFAT( clusIndex, CLUST_EOF );
    
    // Create the "." and ".." directories
    FAT_Directory* dirEntry = (FAT_Directory*)m_logicDisk->SectorCache.GetSector( m_logicDisk->ClusToSect( clusIndex ), TRUE );
    
    if(!dirEntry) return CLUST_ERROR;
    
    for(int i = 0; i < 2; i++)
    {
        dirEntry[i].DIR_Name[0] = '.';
        dirEntry[i].DIR_Name[1] = (i == 0) ? WHITESPACE_CHAR : '.';
        memset( &(dirEntry[i].DIR_Name[2]), WHITESPACE_CHAR, 9 );
        
        dirEntry[i].DIR_Attr = ATTR_DIRECTORY;
        
        dirEntry[i].SetFstClus( (i == 0) ? clusIndex : parentClusIndex );
    }

    return clusIndex;
}

/////////////////////////////////////////////////////////////////////
// Description:
//     get flie extension name from one STR
// 
// Input:
//     path-- 
//
// output:
//      str -- 
// Remarks:
//       
// Returns:   
//  
LPCWSTR FAT_FILE::GetFileNameExt( LPCWSTR path, UINT32 pathLen, UINT32* retLen )
{
    LPCWSTR ext = FAT_Utility::FindCharReverse( path, pathLen, '.', retLen );

    if(*retLen > 0)
    {
        (*retLen)--;
        return ext + 1;
    }

    return ext;
}

/////////////////////////////////////////////////////////////////////
// Description:
//     calc short name check sum for long directory
// 
// Input:
//     ShortName-- 
//
// output:
//   
// Remarks:
//       
// Returns:   
//      check sum
BYTE FAT_FILE::GetShortNameChksum( LPCSTR shortName )
{
    UINT16 FcbNameLen;
    BYTE Sum;

    Sum = 0;
    for(FcbNameLen = 11; FcbNameLen != 0; FcbNameLen--)
    {
        Sum = ((Sum & 1) ? 0x80 : 0) + (Sum >> 1) + *shortName++;
    }

    return Sum;
}
