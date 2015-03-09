////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYHAL_RELEASEINFO_H_
#define _TINYHAL_RELEASEINFO_H_ 1


struct MFVersion
{
    unsigned short usMajor;
    unsigned short usMinor;
    unsigned short usBuild;
    unsigned short usRevision;
    
    // Version & MfReleaseInfo participate in a union in the debugging support,
    // and therefore cannot have real constructors, though that would be better
    // style otherwise.
    static void Init(MFVersion& version, unsigned short major=0, unsigned short minor=0, unsigned short build=0, unsigned short revision=0)
    {
        version.usMajor = major;
        version.usMinor = minor;
        version.usBuild = build;
        version.usRevision = revision;
    }
};

struct MfReleaseInfo
{
    MFVersion version;
    unsigned char infoString[64-sizeof(MFVersion)];
    
    static void Init(MfReleaseInfo& releaseInfo, unsigned short major=0, unsigned short minor=0, unsigned short build=0, unsigned short revision=0, const char *info=(const char *)NULL, size_t infoLen=0);
};

struct OEM_MODEL_SKU
{
    unsigned char  OEM;
    unsigned char  Model;
    unsigned short SKU;
};

struct OEM_SERIAL_NUMBERS
{
    unsigned char module_serial_number[32];
    unsigned char system_serial_number[16];
};

struct HalSystemInfo
{
    MfReleaseInfo       m_releaseInfo;
    OEM_MODEL_SKU       m_OemModelInfo;
    OEM_SERIAL_NUMBERS  m_OemSerialNumbers;
};

//--//

// GetHalSystemInfo() is defined in DeviceCode\pal\configuration\, and is provided
// to allow convenient access to configuration and build information to the debugging
// system and to managed code.
unsigned int GetHalSystemInfo(HalSystemInfo& systemInfo);

// Solution-specific function; see TinyBooterEntry.cpp in your solution's TinyBooter directory for implementation.
unsigned int TinyBooter_GetReleaseInfo(MfReleaseInfo& releaseInfo);

// Solution-specific function, provide this to allow access to a vendor-provided
// informative string and build-time version information.
unsigned int Solution_GetReleaseInfo(MfReleaseInfo&);

//--//

#endif // _TINYHAL_RELEASEINFO_H_
