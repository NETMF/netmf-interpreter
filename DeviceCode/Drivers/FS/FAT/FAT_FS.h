////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _FAT_FS_H_
#define _FAT_FS_H_ 1

#include "tinyhal.h"
#include "FS_decl.h"

#ifdef _DEBUG
#ifndef FAT_FS__VALIDATE_READONLY_CACHELINE
#define FAT_FS__VALIDATE_READONLY_CACHELINE 1
#endif
#endif

#ifndef FAT_FS__CACHE_FLUSH_TIMEOUT_USEC
#define FAT_FS__CACHE_FLUSH_TIMEOUT_USEC (60*1000*1000)
#endif


#define ATTR_READONLY  0x01                       // file is readonly
#define ATTR_HIDDEN    0x02                       // file is hidden
#define ATTR_SYSTEM    0x04                       // file is a system file
#define ATTR_VOLUME_ID 0x08                       // entry is a volume label
#define ATTR_DIRECTORY 0x10                       // entry is a directory name
#define ATTR_ARCHIVE   0x20                       // file is new or modified
// this is a long filename entry
#define ATTR_LONG_NAME      (ATTR_READONLY | ATTR_HIDDEN | ATTR_SYSTEM |ATTR_VOLUME_ID)          
#define ATTR_LONG_NAME_MASK (ATTR_LONG_NAME | ATTR_DIRECTORY | ATTR_ARCHIVE)
#define ATTR_SET_MASK       (ATTR_READONLY | ATTR_HIDDEN | ATTR_SYSTEM | ATTR_ARCHIVE)

#define CLUST_NONE          0x0    
#define CLUST_RSRVD         0x0ffffff6                 // reserved cluster range
#define CLUST_FAT16_BAD     0xfff7                     // a cluster with a defect
#define CLUST_FAT16_EOFS    0xfff8                     // start of eof cluster range
#define CLUST_FAT16_EOFE    0xffff                     // end of eof cluster range
#define CLUST_FAT32_BAD     0x0ffffff7                 // a cluster with a defect
#define CLUST_FAT32_EOFS    0x0ffffff8                 // start of eof cluster range
#define CLUST_FAT32_EOFE    0x0fffffff                 // end of eof cluster range
#define CLUST_EOF           0x0fffffff                 // 

#define CLUST_ERROR    0xFFFFFFFF                 // error while reading / writing cluster
#define FAT32_MASK     0x0fffffff                 //mask for FAT32 cluster numbers

#define SLOT_EMPTY     0x00                       // slot has never been used
#define SLOT_E5        0x05                       // the real value is 0xe5
#define SLOT_DELETED   0xE5                       // file in this slot deleted

#define LAST_LONG_ENTRY 0x40                      // end flag of long directory count

#define MINIMUM_SECTOR_SIZE 512
#define DEFAULT_SECTOR_SIZE MINIMUM_SECTOR_SIZE

#define SHORTNAME_MAXLENGTH 13                    //short name 8.3 format + one end flag'\0'
#define MAX_LONGENTRY_COUNT 20                    // MAX long directory count one file has: 20*13>255

#define SHORTNAME_CONVERSION_MAP_SIZE 512
#define SHORTNAME_SIZE                8
#define SHORTNAME_EXT_SIZE            3
#define SHORTNAME_FULL_SIZE           SHORTNAME_SIZE + SHORTNAME_EXT_SIZE

#define WHITESPACE_CHAR   0x20

#define CLUSTYPE_DATA     0                       //return value of GetClusType function
#define CLUSTYPE_EOS      1
#define CLUSTYPE_BAD      2
#define CLUSTYPE_ERROR    3

#define FORMAT_PARAMETER_DEFAULT       0x0
#define FORMAT_PARAMETER_FORCE_FAT16   0x1
#define FORMAT_PARAMETER_FORCE_FAT32   0x2

#define CLUSTER_NOT_A_CLUSTER 0
#define CLUSTER_START         2

#ifndef PLATFORM_DEPENDENT_FATFS_SECTORCACHE_MAXSIZE
#define SECTORCACHE_MAXSIZE     8
#else
#define SECTORCACHE_MAXSIZE     PLATFORM_DEPENDENT_FATFS_SECTORCACHE_MAXSIZE
#endif

#ifndef PLATFORM_DEPENDENT_FATFS_SECTORCACHE_LINESIZE
#define SECTORCACHE_LINESIZE    2048
#else
#define SECTORCACHE_LINESIZE    PLATFORM_DEPENDENT_FATFS_SECTORCACHE_LINESIZE
#endif

#ifndef PLATFORM_DEPENDENT_FATFS_MAX_OPEN_HANDLES
#define MAX_OPEN_HANDLES        8
#else
#define MAX_OPEN_HANDLES        PLATFORM_DEPENDENT_FATFS_MAX_OPEN_HANDLES
#endif

#ifndef PLATFORM_DEPENDENT_FATFS_MAX_VOLUMES
#define MAX_VOLUMES             2
#else
#define MAX_VOLUMES             PLATFORM_DEPENDENT_FATFS_MAX_VOLUMES
#endif


//--//

//------------------------------------------------
#define GET_DATAT16(a)   (UINT16)(a[0] | (((UINT16)a[1]) << 8))

#define SET_DATA16(a, data16)   \
            a[0] = (BYTE)(data16 & 0xFF);  \
            a[1] = (BYTE)(data16 >> 8)


#define GET_DATAT32(a)   (UINT32)(a[0] | (((UINT32)a[1]) << 8) | (((UINT32)a[2]) << 16) |  (((UINT32)a[3]) << 24))


#define SET_DATA32(a, data32)   \
            a[0] = (BYTE)(data32 & 0xFF);  \
            a[1] = (BYTE)(data32 >> 8);    \
            a[2] = (BYTE)(data32 >> 16);   \
            a[3] = (BYTE)(data32 >> 24)


//MBR
struct GNU_PACKED FAT_MBR
{
    BYTE CodeArea[446];

    struct Partition
    {
        BYTE BootIndicator;
        BYTE StartingCHS[3];
        BYTE PartitionType;
        BYTE EndingCHS[3];
        BYTE RelativeSector[4];
        BYTE TotalSector[4];

        
        UINT32 Get_RelativeSector() 
        { 
            return GET_DATAT32(RelativeSector);
        }

        UINT32 Get_TotalSector()
        {
            return GET_DATAT32(TotalSector);
        }
        
    } Partitions[4];

    UINT8 EndingFlag[2];
    UINT16 Get_EndingFlag() 
    { 
        return GET_DATAT16(EndingFlag);
    }
    void Set_EndingFlag( UINT16 endFlag )
    {
        SET_DATA16(EndingFlag,endFlag);
    }


    //--//
    
    BOOL IsValid();
};

CT_ASSERT( sizeof(FAT_MBR) == 512 );

struct FAT_LogicDisk;
struct FAT_EntryEnumerator;




//DBR
struct FAT_DBR
{
    BYTE BS_JmpBoot[3]; //ofs:0.   0xEB,0x3E,0x90

    BYTE BS_OEMName[8]; //ofs:3.   "MSDOS5.0"

    BYTE BPB_BytsPerSec[2]; //ofs:11.  512  

    UINT16 Get_BPB_BytsPerSec() 
    { 
        return GET_DATAT16(BPB_BytsPerSec);
    }
    void Set_BPB_BytsPerSec( UINT16 bytsPerSec )
    {
        SET_DATA16(BPB_BytsPerSec,bytsPerSec);
    }

    BYTE BPB_SecPerClus; //ofs:13.   

    UINT8 BPB_RsvdSecCnt[2]; //ofs:14.  from DBR to FAT
    UINT16 Get_BPB_RsvdSecCnt() 
    { 
        return GET_DATAT16(BPB_RsvdSecCnt);
    }
    void Set_BPB_RsvdSecCnt( UINT16 rsvdSecCnt )
    {
        SET_DATA16(BPB_RsvdSecCnt,rsvdSecCnt);
    }


    BYTE BPB_NumFATs; //ofs:16.  2 

    BYTE BPB_RootEntCnt[2]; //ofs:17.  FAT16=512  FAT32=0 
    UINT16 Get_BPB_RootEntCnt() 
    { 
        return GET_DATAT16(BPB_RootEntCnt);
    }
    void Set_BPB_RootEntCnt( UINT16 rootEntCnt )
    {
        SET_DATA16(BPB_RootEntCnt,rootEntCnt);
    }

    BYTE BPB_TotSec16[2]; //ofs:19.  <32M  FAT32=0
    UINT16 Get_BPB_TotSec16() 
    { 
        return GET_DATAT16(BPB_TotSec16);
    }
    void Set_BPB_TotSec16( UINT16 totSec16 )
    {
        SET_DATA16(BPB_TotSec16,totSec16);
    }

    BYTE BPB_Media; //ofs:21.  0xF8

    UINT8 BPB_FATSz16[2]; //ofs:22.  
    UINT16 Get_BPB_FATSz16() 
    { 
        return GET_DATAT16(BPB_FATSz16);
    }
    void Set_BPB_FATSz16( UINT16 FATSz16)
    {
        SET_DATA16(BPB_FATSz16,FATSz16);
    }


    UINT8 BPB_SecPerTrk[2];  //ofs:24
    UINT16 Get_BPB_SecPerTrk() 
    { 
        return GET_DATAT16(BPB_SecPerTrk);
    }
    void Set_BPB_SecPerTrk( UINT16 secPerTrk)
    {
        SET_DATA16(BPB_SecPerTrk,secPerTrk);
    }


    UINT8 BPB_NumHeads[2]; //ofs:26.
    UINT16 Get_BPB_NumHeads() 
    { 
        return GET_DATAT16(BPB_NumHeads);
    }
    void Set_BPB_NumHeads( UINT16 numHeads)
    {
        SET_DATA16(BPB_NumHeads,numHeads);
    }


    UINT8  BPB_HiddSec[4]; //ofs:28.  from MBR to DBR
    UINT32 Get_BPB_HiddSec() 
    { 
        return GET_DATAT32(BPB_HiddSec);
    }
    void Set_BPB_HiddSec( UINT32 hiddSec)
    {
        SET_DATA32(BPB_HiddSec,hiddSec);
    }


    UINT8  BPB_TotSec32[4]; //ofs:32.  >=32M
    UINT32 Get_BPB_TotSec32() 
    { 
        return GET_DATAT32(BPB_TotSec32);
    }
    void Set_BPB_TotSec32( UINT32 totSec32)
    {
        SET_DATA32(BPB_TotSec32,totSec32);
    }




    struct FAT16_DBR
    {
        BYTE BS_DrvNum; //ofs:36.floppy disk:0x00, hard disk:0x80

        BYTE BS_Reserved1; //ofs:37.

        BYTE BS_BootSig; //ofs:38. 0x29

        BYTE BS_VolID[4]; //ofs:39. Disk ID

        BYTE BS_VolLab[11]; //ofs:43. "MFDisk  "

        BYTE BS_FilSysType[8]; //ofs:54. "FAT16    "
    };
    
    struct FAT32_DBR
    {
        //FAT32
        UINT8 BPB_FATSz32[4]; //ofs:36. 
        UINT32 Get_BPB_FATSz32() 
        { 
            return GET_DATAT32(BPB_FATSz32);
        }
        void Set_BPB_FATSz32( UINT32 FATSz32)
        {
            SET_DATA32(BPB_FATSz32,FATSz32);
        }

        

        UINT8 BPB_ExtFlags[2]; //ofs:40.
        UINT16 Get_BPB_ExtFlags() 
        { 
            return GET_DATAT16(BPB_ExtFlags);
        }
        void Set_BPB_ExtFlags( UINT16 extFlags)
        {
            SET_DATA16(BPB_ExtFlags,extFlags);
        }        
        static const UINT32 BPB_ExtFlags__offset = 40;

        UINT8 BPB_FSVer[2]; //ofs:42. 
        UINT16 Get_BPB_FSVer() 
        { 
            return GET_DATAT16(BPB_FSVer);
        }
        void Set_BPB_FSVer( UINT16 FSVer)
        {
            SET_DATA16(BPB_FSVer,FSVer);
        }      

        UINT8 BPB_RootClus[4]; //ofs:44. 2
        UINT32 Get_BPB_RootClus() 
        { 
            return GET_DATAT32(BPB_RootClus);
        }
        void Set_BPB_RootClus( UINT32 RootClus)
        {
            SET_DATA32(BPB_RootClus,RootClus);
        }



        UINT8 BPB_FSInfo[2]; //ofs:48. 1
        UINT16 Get_BPB_FSInfo() 
        { 
            return GET_DATAT16(BPB_FSInfo);
        }
        void Set_BPB_FSInfo( UINT16 FSInfo)
        {
            SET_DATA16(BPB_FSInfo,FSInfo);
        }  

        UINT8 BPB_BkBootSec[2]; //ofs:50. 6
        UINT16 Get_BPB_BkBootSec() 
        { 
            return GET_DATAT16(BPB_BkBootSec);
        }
        void Set_BPB_BkBootSec( UINT16 bkBootSec)
        {
            SET_DATA16(BPB_BkBootSec,bkBootSec);
        }  


        BYTE BPB_Reserved[12]; //ofs:52.

        //---------------------
        BYTE BS_DrvNum; //ofs:64.floppy disk:0x00, hard disk:0x80

        BYTE BS_Reserved1; //ofs:65.

        BYTE BS_BootSig; //ofs:66. 0x29

        BYTE BS_VolID[4]; //ofs:67. Disk ID

        BYTE BS_VolLab[11]; //ofs:71. "MFDisk  "

        BYTE BS_FilSysType[8]; //ofs:82. "FAT32   "/"FAT16    "/"FAT12   "

        BYTE ExecutableCode[2];
    };

    union DBR_Union
    {
        FAT16_DBR FAT16;
        FAT32_DBR FAT32;
    };

    DBR_Union DBRUnion; //ofs:36

    BYTE ExecutableCode[418]; //ofs:90  FAT32=420

    UINT8 EndingFlag[2]; //ofs:510. 0xAA55
    UINT16 Get_EndingFlag() 
    { 
        return GET_DATAT16(EndingFlag);
    }
    void Set_EndingFlag( UINT16 endFlag )
    {
        SET_DATA16(EndingFlag,endFlag);
    }
    //--//
    BOOL IsValid( BOOL* isFAT16 );

    UINT32 GetCountOfClusters()
    {
        UINT32 FATSz  = (Get_BPB_FATSz16()  != 0) ? Get_BPB_FATSz16()  : DBRUnion.FAT32.Get_BPB_FATSz32();
        UINT32 totSec = (Get_BPB_TotSec16() != 0) ? Get_BPB_TotSec16() : Get_BPB_TotSec32()        ;

        UINT32 rootDirSectors = ((Get_BPB_RootEntCnt() * 32) + (Get_BPB_BytsPerSec() - 1)) / Get_BPB_BytsPerSec();
        
        return (totSec - (Get_BPB_RsvdSecCnt() + (BPB_NumFATs * FATSz) + rootDirSectors)) / BPB_SecPerClus;
    }
};

CT_ASSERT( sizeof(FAT_DBR) == 512 );

//FS_INFO  - FAT32
struct FAT_FSINFO
{
    UINT8 FSI_LeadSig[4]; //ofs:0. 0x41615252
    UINT32 Get_FSI_LeadSig() 
    { 
        return GET_DATAT32(FSI_LeadSig);
    }
    void Set_FSI_LeadSig( UINT32 leadSig)
    {
        SET_DATA32(FSI_LeadSig,leadSig);
    }

    static const UINT32 FSI_LeadSig__offset = 0;



    BYTE FSI_Reserved1[480]; //ofs:4. 
    static const UINT32 FSI_Reserved1__offset = 4;
    static const UINT32 FSI_Reserved1__size = 480;

    UINT8 FSI_StrucSig[4]; //ofs:484. 0x61417272
    UINT32 Get_FSI_StrucSig() 
    { 
        return GET_DATAT32(FSI_StrucSig);
    }
    void Set_FSI_StrucSig( UINT32 strucSig)
    {
        SET_DATA32(FSI_StrucSig,strucSig);
    }
    static const UINT32 FSI_StrucSig__offset = 484;

    UINT8 FSI_Free_Count[4]; //ofs:488.
    UINT32 Get_FSI_Free_Count() 
    { 
        return GET_DATAT32(FSI_Free_Count);
    }
    void Set_FSI_Free_Count( UINT32 free_Count)
    {
        SET_DATA32(FSI_Free_Count,free_Count);
    }    
    static const UINT32 FSI_Free_Count__offset = 488;

    UINT8 FSI_Nxt_free[4]; //ofs:492.
    UINT32 Get_FSI_Nxt_free() 
    { 
        return GET_DATAT32(FSI_Nxt_free);
    }
    void Set_FSI_Nxt_free( UINT32 nxt_free)
    {
        SET_DATA32(FSI_Nxt_free,nxt_free);
    }    
    static const UINT32 FSI_Nxt_free__offset = 492;

    BYTE FSI_Reserved2[12]; //ofs:496.
    static const UINT32 FSI_Reserved2__offset = 496;
    static const UINT32 FSI_Reserved2__size = 12;

    UINT8 FSI_TrailSig[4]; //ofs:508. 0xAA550000
    UINT32 Get_FSI_TrailSig() 
    { 
        return GET_DATAT32(FSI_TrailSig);
    }
    void Set_FSI_TrailSig( UINT32 trailSig)
    {
        SET_DATA32(FSI_TrailSig,trailSig);
    }       
    static const UINT32 FSI_TrailSig__offset = 508;

    //--//
    void Initialize( UINT32 freeCount, UINT32 nxtFree );

    BOOL IsValid();
};

CT_ASSERT( sizeof(FAT_FSINFO) == 512 );


//Directory 
struct FAT_Directory
{
    BYTE DIR_Name[11]; //ofs:0.  Short name  
    static const UINT32 DIR_Name__offset = 0;
    static const UINT32 DIR_Name__size = 11;

    BYTE DIR_Attr; //ofs:11. File attributes
    static const UINT32 DIR_Attr__offset = 11;

    BYTE DIR_NTRes; //0fs:12. Reserved for use by Windows NT
    static const UINT32 DIR_NTRes__offset = 12;

    BYTE DIR_CrtTimeTenth; //0fs:13. Millisecond stamp at file creation time
    static const UINT32 DIR_CrtTimeTenth__offset = 13;

    UINT8 DIR_CrtTime[2]; //0fs:14. Time file was created
    UINT16 Get_DIR_CrtTime() 
    { 
        return GET_DATAT16(DIR_CrtTime);
    }
    void Set_DIR_CrtTime( UINT16 crtTime)
    {
        SET_DATA16(DIR_CrtTime,crtTime);
    }
    static const UINT32 DIR_CrtTime__offset = 14;

    UINT8 DIR_CrtDate[2]; //0fs:16. Date file was created
    UINT16 Get_DIR_CrtDate() 
    { 
        return GET_DATAT16(DIR_CrtDate);
    }
    void Set_DIR_CrtDate( UINT16 crtDate)
    {
        SET_DATA16(DIR_CrtDate,crtDate);
    }
    static const UINT32 DIR_CrtDate__offset = 16;

    UINT8 DIR_LstAccDate[2]; //0fs:18. Last access date
    UINT16 Get_DIR_LstAccDate() 
    { 
        return GET_DATAT16(DIR_LstAccDate);
    }
    void Set_DIR_LstAccDate( UINT16 lstAccDate)
    {
        SET_DATA16(DIR_LstAccDate,lstAccDate);
    }
    static const UINT32 DIR_LstAccDate__offset = 18;

    UINT8 DIR_FstClusHI[2]; //0fs:20.High word of this entry's first cluster number
    UINT16 Get_DIR_FstClusHI() 
    { 
        return GET_DATAT16(DIR_FstClusHI);
    }
    void Set_DIR_FstClusHI( UINT16 fstClusHI)
    {
        SET_DATA16(DIR_FstClusHI,fstClusHI);
    }    
    static const UINT32 DIR_FstClusHI__offset = 20;

    UINT8 DIR_WrtTime[2]; //0fs:22. Time of last write
    UINT16 Get_DIR_WrtTime() 
    { 
        return GET_DATAT16(DIR_WrtTime);
    }
    void Set_DIR_WrtTime( UINT16 wrtTime)
    {
        SET_DATA16(DIR_WrtTime,wrtTime);
    }
    static const UINT32 DIR_WrtTime__offset = 22;

    UINT8 DIR_WrtDate[2]; //0fs:24. Date of last write
    UINT16 Get_DIR_WrtDate() 
    { 
        return GET_DATAT16(DIR_WrtDate);
    }
    void Set_DIR_WrtDate( UINT16 wrtDate)
    {
        SET_DATA16(DIR_WrtDate,wrtDate);
    }
    static const UINT32 DIR_WrtDate__offset = 24;

    UINT8 DIR_FstClusLO[2]; //0fs:26. Low  word of this entry's first cluster number
    UINT16 Get_DIR_FstClusLO() 
    { 
        return GET_DATAT16(DIR_FstClusLO);
    }
    void Set_DIR_FstClusLO( UINT16 fstClusLO)
    {
        SET_DATA16(DIR_FstClusLO,fstClusLO);
    }
    static const UINT32 DIR_FstClusLO__offset = 26;

    UINT8 DIR_FileSize[4]; //0fs:28. 32-bit DWORD holding this file's size in bytes
    UINT32 Get_DIR_FileSize() 
    { 
        return GET_DATAT32(DIR_FileSize);
    }
    void Set_DIR_FileSize( UINT32 fileSize)
    {
        SET_DATA32(DIR_FileSize,fileSize);
    }
    static const UINT32 DIR_FileSize__offset = 28;

    //--//

    UINT32 GetFstClus()
    { 
        return (((UINT32)Get_DIR_FstClusHI()) << 16) | ((UINT32)Get_DIR_FstClusLO());
    }
    
    void SetFstClus( UINT32 value )
    {
        Set_DIR_FstClusHI ((UINT16)(value >> 16)    );
        Set_DIR_FstClusLO ((UINT16)(value & 0xFFFF) );
    }

    //--//
    void Initialize();

    void SetName ( LPCWSTR name, UINT32 nameLen );
    BOOL IsName  ( LPCWSTR name, UINT32 nameLen );
    void CopyName( WCHAR*  name                 );
    
    UINT32 GetNameLength();
};

CT_ASSERT( sizeof(FAT_Directory) == 32 );


//LONG_Directory
struct FAT_LONG_Directory
{
    BYTE LDIR_Ord; //ofs:0. (LAST_LONG_ENTRY)|orderNum
    static const UINT32 LDIR_Ord__offset = 0;

    // 5 UTF-16char
    static const UINT32 LDIR_Name1__size = 5*2;    
    BYTE LDIR_Name1[LDIR_Name1__size]; //0fs:1.
    static const UINT32 LDIR_Name1__offset = 1;


    BYTE LDIR_Attr; //0fs:11. should be ATTR_LONG_NAME
    static const UINT32 LDIR_Attr__offset = 11;

    BYTE LDIR_Type; //0fs:12. should be zero
    static const UINT32 LDIR_Type__offset = 12;

    BYTE LDIR_Chksum; //0fs:13. get from short file name
    static const UINT32 LDIR_Chksum__offset = 13;

    // 6 UTF-16char
    BYTE LDIR_Name2[6*2]; //0fs:14. 6-11
    static const UINT32 LDIR_Name2__offset = 14;
    static const UINT32 LDIR_Name2__size = 12;

    BYTE LDIR_FstClusLO[2]; //0fs:26. should be zero
    UINT16 Get_LDIR_FstClusLO() 
    { 
        return GET_DATAT16(LDIR_FstClusLO);
    }
    void Set_LDIR_FstClusLO( UINT16 fstClusLO)
    {
        SET_DATA16(LDIR_FstClusLO,fstClusLO);
    }
    
    static const UINT32 LDIR_FstClusLO__offset = 26;

    // 2 UTF-16char
    BYTE LDIR_Name3[2*2]; //0fs:28. 12-13
    static const UINT32 LDIR_Name3__offset = 28;
    static const UINT32 LDIR_Name3__size = 4;

    //--//

    void Initialize( BYTE ord, LPCWSTR name, UINT32 nameLength, BYTE chksum );

    void SetName ( LPCWSTR name, UINT32 nameLen );
    BOOL IsName  ( LPCWSTR name, UINT32 nameLen );
    void CopyName( WCHAR*  name                 );
    
    UINT32 GetNameLength();
};

CT_ASSERT( sizeof(FAT_LONG_Directory) == 32 );


//FILE
struct FAT_FILE
{
private:
    FAT_LogicDisk* m_logicDisk;
    
    UINT32 m_dirSectIndex; //index of sector store short directory
    UINT32 m_dirDataIndex; //short directory store byte-offset in one sector

    UINT32 m_lDirSectIndex; //index of sector store long directory
    UINT32 m_lDirDataIndex; //long directory store byte-offset in one sector
    UINT32 m_lDirCount;     //real used long directory count for this file

    UINT32 m_fileNameLength;

    //--//
    
public:
    HRESULT Create( FAT_LogicDisk* logicDisk, UINT32 clusIndex, LPCWSTR fileName, UINT32 fileNameLen, BYTE attributes );
    HRESULT Parse ( FAT_LogicDisk* logicDisk, FAT_EntryEnumerator* entryEnum );

    FAT_Directory* GetDirectoryEntry( BOOL forWrite );
    void MarkDirectoryEntryForWrite();

    HRESULT DeleteDirectoryEntry();

    BOOL IsFileName( LPCWSTR name, UINT32 nameLen );
    HRESULT CopyFileName( LPWSTR name, UINT32 nameLen );

private:
    BOOL IsFileNameValid( LPCWSTR fileName, UINT32 fileNameLen );
    BOOL IsFileLongName( LPCWSTR fileName, UINT32 fileNameLen );
    BOOL GenerateBasisName( LPCWSTR fileName, UINT32 fileNameLen, LPSTR basisName );
    BOOL SetBasisNameChar( LPSTR basis, WCHAR c, BOOL* needsTrail );    
    BOOL CollectNumTail( LPCSTR fileName, LPCSTR basisName, UINT32 *availableMap );
    HRESULT LongToShortName( UINT32 clusIndex, LPCWSTR longName, UINT32 longNameLength, LPSTR shortName );
    void AttachNumTail( LPSTR shortName, UINT32 num );
    UINT32 CreateNewDirectory( UINT32 parentClusIndex );
    LPCWSTR GetFileNameExt( LPCWSTR path, UINT32 pathLen, UINT32* retLen );
    BYTE GetShortNameChksum( LPCSTR shortName );
};

struct FAT_FileHandle
{
private:
    INT64 m_position; //File IO position

    UINT32 m_clusIndex; //temp cluster index
    UINT32 m_sectIndex; //temp sector index
    UINT32 m_dataIndex; //temp data offset in one sector

    //File read/write flag, to update file time when CloseFile()
    UINT32 m_readWriteState;

    static const UINT32 ReadWriteState__NONE  = 0; 
    static const UINT32 ReadWriteState__READ  = 0x1;
    static const UINT32 ReadWriteState__WRITE = 0x2;

    FAT_FILE m_file;

    FAT_LogicDisk* m_logicDisk;

    //--//

public:
    static FAT_FileHandle* Open( FAT_LogicDisk* logicDisk, FAT_FILE* fileInfo, UINT32 clusIndex );

    HRESULT Close();
    HRESULT Read( BYTE *buffer, int size, int *bytesRead );
    HRESULT Write( BYTE *buffer, int size, int *bytesWritten );
    HRESULT Flush();
    HRESULT Seek( INT64 offset, UINT32 origin, INT64* position );
    HRESULT GetLength( INT64 *size );
    HRESULT SetLength( INT64 size );

    
private:
    static const int Helper_Read  = 0;
    static const int Helper_Write = 1;
    static const int Helper_Seek  = 2;
    static const int Helper_Flush = 3;
    
    HRESULT ReadWriteSeekHelper( int type, BYTE* buffer, int size, int* bytesDone );
};

struct FAT_EntryEnumerator
{
private:
    
    FAT_LogicDisk* m_logicDisk;

    UINT32 m_clusIndex;
    UINT32 m_sectIndex;
    UINT32 m_dataIndex;

    UINT32 m_flag;

    static const UINT32 Flag_None   = 0x0;
    static const UINT32 Flag_First  = 0x1;
    static const UINT32 Flag_Done   = 0x2;
    static const UINT32 Flag_Extend = 0x4;
    //--//

public:
    void Initialize( FAT_LogicDisk* logicDisk, UINT32 sectIndex, UINT32 dataIndex, BOOL extend = FALSE );

    FAT_Directory* GetNext( BOOL forWrite = false );

    void GetIndices( UINT32* sectIndex, UINT32* dataIndex );
};

struct FAT_FINDFILES
{
private:
    FAT_EntryEnumerator m_entryEnum;
    FAT_LogicDisk*      m_logicDisk;

    //--//
public:
    static FAT_FINDFILES* FindOpen( FAT_LogicDisk* logicDisk, UINT32 clusIndex );

    HRESULT FindNext( FS_FILEINFO *fi, BOOL *fileFound );
    HRESULT FindClose();
};


struct FAT_SectorCache
{
private:    
    struct FAT_CacheLine
    {
        BYTE*  m_buffer;
        UINT32 m_begin;
        UINT32 m_bsByteAddress;
        UINT32 m_flags;
#ifdef FAT_FS__VALIDATE_READONLY_CACHELINE
        UINT32 m_crc;
#endif

        static const UINT32 CacheLine__Dirty           = 0x80000000;
        static const UINT32 CacheLine__IsReadWrite     = 0x40000000;
        static const UINT32 CacheLine__LRUCounter_Mask = 0x3FFFFFFF;

        BOOL IsDirty() { return 0 != (m_flags & CacheLine__Dirty); }
        void SetDirty( BOOL dirty )
        {
            if(dirty) m_flags |=  CacheLine__Dirty | CacheLine__IsReadWrite;
            else      m_flags &= ~CacheLine__Dirty;
        }

        BOOL IsReadWrite() { return 0 != (m_flags & CacheLine__IsReadWrite); }
        void SetReadWrite( BOOL readWrite )
        {
            if(readWrite) m_flags |=  CacheLine__IsReadWrite;
            else          m_flags &= ~CacheLine__IsReadWrite;
        }

        UINT32 GetLRUCounter() { return m_flags & CacheLine__LRUCounter_Mask; }
        void SetLRUCOunter( UINT32 counter )
        {
            m_flags &= ~CacheLine__LRUCounter_Mask;
            m_flags |= counter & CacheLine__LRUCounter_Mask;
        }
    };

    FAT_CacheLine m_cacheLines[SECTORCACHE_MAXSIZE];

    UINT32    m_LRUCounter;

    UINT32    m_baseByteAddress;
    UINT32    m_sectorsPerLine;

    BlockStorageDevice* m_blockStorageDevice;
    UINT32              m_bytesPerSector;
    UINT32              m_sectorCount;
    HAL_COMPLETION      m_flushCompletion;

    FAT_CacheLine* GetCacheLine( UINT32 sectorIndex );
    FAT_CacheLine* GetUnusedCacheLine(BOOL useLRU);
    void FlushSector( FAT_CacheLine* cacheLine, BOOL fClearDirtyBit=TRUE );

    static void OnFlushCallback(void* arg);

public:
    void Initialize( BlockStorageDevice* blockStorageDevice, UINT32 bytesPerSector, UINT32 baseAddress, UINT32 sectorCount );
    void Uninitialize();

    BYTE* GetSector( UINT32 sectorIndex, BOOL forWrite );
    BYTE* GetSector( UINT32 sectorIndex, BOOL useLRU, BOOL forWrite );
    void MarkSectorDirty( UINT32 sectorIndex );

    void FlushSector( UINT32 sectorIndex );
    void FlushAll();
    void EraseSector( UINT32 sectorIndex );
};

//LogicDisk 
struct FAT_LogicDisk
{
private:
    UINT64 m_diskSize;      //when initialize, get disk size by all BLOCKTYPE_FILESYSTEM blocks in block map
    UINT32 m_baseAddress;   //BLOCKTYPE_FILESYSTEM blocks begin address, set base address for readsector() & writesector()
    UINT32 m_sectorCount;   //all sectors count

    /////////////////////////////////////////////////////////
    //	  IO buffer scenario
    //			buffer size: one block size in hardware device
    //			when block size is changed(For different region can has different block size) ,
    //			delete old buffer and re-allocate new buffer
    //
    //	   *Fs_IOBuffer--IO buffer pointer
    //	  Buffer_Sector_Begin--FAT FS sector index at the beginning of IO buffer
    //	  Buffer_Sector_End--FAT FS sector index at the end of IO buffer
    //	  Buffer_Dirty--IO buffer pointer has been rewrited and need to flush
    ////////////////////////////////////////////////////////////////

    //--//

    UINT32 m_FATBaseSector[2];
    UINT32 m_firstDataSector;

    UINT32 m_totalClusterCount;
    UINT32 m_totalSectorCount;
    UINT32 m_entriesPerSector;

    UINT32 m_sectorFSInfo;
    UINT32 m_freeCount;
    UINT32 m_nextFree;
    
    BOOL   m_isFAT16;

public:
    FAT_SectorCache SectorCache;
    
    UINT32 m_rootSectorStart;
    UINT32 m_bytesPerSector;
    UINT32 m_sectorsPerCluster;

    UINT32 m_volumeId;
    BlockStorageDevice* m_blockStorageDevice;

    //--//
public:


    //--//
    UINT32 ReadFAT ( UINT32 clusIndex               );
    UINT32 WriteFAT( UINT32 clusIndex, UINT32 value );
    
    void EraseCluster( UINT32 clusIndex );

    //--//
    BOOL MountDisk();
    HRESULT FormatHelper( LPCSTR volumeLabel, UINT32 parameters );
    HRESULT GetDiskVolLab( LPSTR label );
    UINT64 GetDiskTotalSize();
    UINT64 GetDiskFreeSize();

    //--//
    UINT32 ClusToSect( UINT32 clusIndex );
    UINT32 SectToClus( UINT32 sectIndex );
    UINT32 GetNextFreeClus( BOOL clear );
    UINT32 GetNextFreeClusHelper( UINT32 fromClus, UINT32 toClus );

    HRESULT GetNextSect( UINT32 *clusIndex, UINT32 *sectIndex, UINT32 flag );
    static const UINT32 GetNextSect__NONE   = 0;
    static const UINT32 GetNextSect__CREATE = 0x1;
    static const UINT32 GetNextSect__CLEAR  = 0x2;

    HRESULT GetNextFreeEntry( UINT32* sectIndex, UINT32* dataIndex, BYTE count );

    UINT32 GetClusType( UINT32 dwData );
    
    BOOL SearchCurDir( UINT32 clusIndex, LPCWSTR fileName, UINT32 fileNameLen, FAT_FILE* fileInfo );

    static const UINT32 GetFile__NONE               = 0;
    static const UINT32 GetFile__CREATE_FILE        = 0x01;
    static const UINT32 GetFile__CREATE_DIRECTORY   = 0x02;
    static const UINT32 GetFile__CREATE_PATH        = 0x04;
    static const UINT32 GetFile__FAIL_IF_EXISTS     = 0x08;
    static const UINT32 GetFile__GET_DIRECTORY_ONLY = 0x16;

    FAT_Directory* GetFile( LPCWSTR path, UINT32 pathLen, FAT_FILE* fileInfo, UINT32 flags = GetFile__NONE );
    
    BOOL PopulateDiskSize();
    void PopulateFreeCount();

    HRESULT DeleteClusterChain( UINT32 clusIndex );
    HRESULT DeleteAll( UINT32 clusIndex );

    //--//
    
    static FAT_LogicDisk* Initialize( const VOLUME_ID *volume );    
    BOOL Uninitialize();
    static HRESULT Format( const VOLUME_ID *volume, LPCSTR volumeLabel, UINT32 parameters );
    static BOOL IsLoadableMedia( BlockStorageDevice *driverInterface, UINT32 *numVolumes );

    
    HRESULT GetSizeInfo( INT64* totalSize, INT64* totalFreeSpace );
    HRESULT Open( LPCWSTR path, UINT32 *handle );
    
    HRESULT FindOpen( LPCWSTR fileSpec, UINT32 *findHandle );
    
    HRESULT GetFileInfo( LPCWSTR path, FS_FILEINFO* fileInfo, BOOL* found );
    HRESULT GetAttributes( LPCWSTR path, UINT32* attributes );
    HRESULT SetAttributes( LPCWSTR path, UINT32  attributes );
    HRESULT CreateDirectory( LPCWSTR path );
    HRESULT Move( LPCWSTR oldPath, LPCWSTR newPath );
    HRESULT Delete( LPCWSTR path );

    //--//

    BOOL InitDisk  ( const VOLUME_ID* volume );
    void UninitDisk(                         );

    
    void InitMount( FAT_DBR* dbr, BOOL isFAT16 );
};

struct FAT_MemoryManager
{
private:
    union FAT_Handle
    {
        FAT_FileHandle fileHandle;
        FAT_FINDFILES  findHandle;
    };

    struct FAT_LogicDiskBuffer
    {
        BOOL          inUse;
        FAT_LogicDisk logicDisk;
    };


    struct FAT_HandleBuffer
    {
        BOOL       inUse;
        FAT_Handle handle;
    };
    
    static FAT_LogicDiskBuffer s_logicDisks[MAX_VOLUMES     ];
    static FAT_HandleBuffer    s_handles   [MAX_OPEN_HANDLES];

public:
    static void Initialize();
        
    static FAT_LogicDisk* AllocateLogicDisk( const VOLUME_ID*     volume    );
    static void           FreeLogicDisk    (       FAT_LogicDisk* logicDisk );    
    static FAT_LogicDisk* GetLogicDisk     ( const VOLUME_ID*     volume    );

    static void*          AllocateHandle   (                          );
    static void           FreeHandle       ( void* handle );
};

//------------------------------------------------

struct FAT_FS_Driver
{
    static void Initialize();
    static BOOL InitializeVolume( const VOLUME_ID *volume );
    static BOOL UnInitializeVolume( const VOLUME_ID *volume );
    static HRESULT Format( const VOLUME_ID *volume, LPCSTR volumeLabel, UINT32 parameters );
    static HRESULT GetSizeInfo( const VOLUME_ID *volume, INT64* totalSize, INT64* totalFreeSpace );
    static HRESULT FlushAll( const VOLUME_ID* volume );
    static HRESULT GetVolumeLabel( const VOLUME_ID* volume, LPSTR volumeLabel, INT32 volumeLabelLen );

    //--//
    static BOOL IsLoadableMedia( BlockStorageDevice *driverInterface, UINT32 *numVolumes );
    static STREAM_DRIVER_DETAILS *DriverDetails( const VOLUME_ID *volume );
    
    static HRESULT Open( const VOLUME_ID *volume, LPCWSTR path, UINT32 *handle );
    static HRESULT Close( UINT32 handle );
    static HRESULT Read( UINT32 handle, BYTE *buffer, int size, int *readsize );
    static HRESULT Write( UINT32 handle, BYTE *buffer, int size, int *writesize );
    static HRESULT Flush( UINT32 handle );
    static HRESULT Seek( UINT32 handle, INT64 offset, UINT32 Flag, INT64 *position );
    static HRESULT GetLength( UINT32 handle, INT64 *size );
    static HRESULT SetLength( UINT32 handle, INT64 size );
    
    //--//
    static HRESULT FindOpen(const VOLUME_ID *volume, LPCWSTR fileSpec, UINT32 *findHandle);
    static HRESULT FindNext(UINT32 handle, FS_FILEINFO *fi, BOOL *fileFound);
    static HRESULT FindClose(UINT32 handle);
    static HRESULT GetFileInfo( const VOLUME_ID* volume, LPCWSTR path, FS_FILEINFO* fileInfo, BOOL* found );
    static HRESULT GetAttributes(const VOLUME_ID *volume, LPCWSTR path, UINT32 *attributes);
    static HRESULT SetAttributes(const VOLUME_ID *volume, LPCWSTR path, UINT32 attributes);
    static HRESULT CreateDirectory(const VOLUME_ID *volume, LPCWSTR path);
    static HRESULT Move(const VOLUME_ID *volume, LPCWSTR oldPath, LPCWSTR newPath);
    static HRESULT Delete(const VOLUME_ID *volume, LPCWSTR path);

    //--//
};

#endif //_FAT_FS_H_
