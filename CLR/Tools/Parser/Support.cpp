////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

_COM_SMRT_PTR(ISymUnmanagedMethod);


////////////////////////////////////////////////////////////////////////////////////////////////////

// Following two functions lifted from NT sources, imagedir.c
static PIMAGE_SECTION_HEADER Cor_RtlImageRvaToSection( IN PIMAGE_NT_HEADERS NtHeaders ,
                                                       IN PVOID             Base      ,
                                                       IN ULONG             Rva       )
{
    PIMAGE_SECTION_HEADER NtSection;
    ULONG                 i;

    NtSection = IMAGE_FIRST_SECTION( NtHeaders );

    for(i=0; i<NtHeaders->FileHeader.NumberOfSections; i++, NtSection++)
    {
        if(Rva >= NtSection->VirtualAddress && Rva < NtSection->VirtualAddress + NtSection->SizeOfRawData)
        {
            return NtSection;
        }
    }

    return NULL;
}

static PVOID Cor_RtlImageRvaToVa( IN     PIMAGE_NT_HEADERS      NtHeaders               ,
                                  IN     PVOID                  Base                    ,
                                  IN     ULONG                  Rva                     ,
                                  IN OUT PIMAGE_SECTION_HEADER *LastRvaSection OPTIONAL )
{
    PIMAGE_SECTION_HEADER NtSection;

    if(!LastRvaSection                                              ||
        (NtSection = *LastRvaSection) == NULL                       ||
        Rva  < NtSection->VirtualAddress                            ||
        Rva >= NtSection->VirtualAddress + NtSection->SizeOfRawData  )
    {
        NtSection = Cor_RtlImageRvaToSection( NtHeaders, Base, Rva );
    }

    if(NtSection != NULL)
    {
        if(LastRvaSection != NULL)
        {
            *LastRvaSection = NtSection;
        }

        return (PVOID)((PCHAR)Base + (Rva - NtSection->VirtualAddress) + NtSection->PointerToRawData);
    }
    else
    {
        return NULL;
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

PELoader::PELoader()
{
    InitToZero();
}

PELoader::~PELoader()
{
    Close();

    m_hMod = NULL;
    m_pNT  = NULL;
}

PELoader::PELoader( const PELoader& pe )
{
    InitToZero();
}

PELoader& PELoader::operator= ( const PELoader& pe )
{
    InitToZero();

    return *this;
}

void PELoader::InitToZero()
{
    m_hFile    = INVALID_HANDLE_VALUE;
    m_hMod     = NULL;
    m_hMapFile = NULL;
    m_pNT      = NULL;
}

void PELoader::Close()
{
    if(m_hFile != INVALID_HANDLE_VALUE)
    {
        if(m_hMod)
        {
            UnmapViewOfFile( (void*)m_hMod );

            m_hMod = NULL;
        }

        if(m_hMapFile)
        {
            CloseHandle( m_hMapFile );

            m_hMapFile = NULL;
        }

        CloseHandle( m_hFile );

        m_hFile = INVALID_HANDLE_VALUE;
    }
}

HRESULT PELoader::OpenAndMapToMemory( LPCWSTR moduleName )
{
    TINYCLR_HEADER();

    if(!moduleName)
    {
        TINYCLR_SET_AND_LEAVE(E_INVALIDARG);
    }

    m_hFile = ::CreateFileW( moduleName, GENERIC_READ, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
    if(m_hFile == INVALID_HANDLE_VALUE)
    {
        wprintf( L"Cannot open '%s'!\n", moduleName );

        TINYCLR_SET_AND_LEAVE(HRESULT_FROM_WIN32(::GetLastError()));
    }

    m_hMapFile = CreateFileMapping( m_hFile, NULL, PAGE_READONLY, 0, 0, NULL );
    if(m_hMapFile == NULL)
    {
        TINYCLR_SET_AND_LEAVE(HRESULT_FROM_WIN32(::GetLastError()));
    }

    m_hMod = (HMODULE)MapViewOfFile( m_hMapFile, FILE_MAP_READ, 0, 0, 0 );
    if(m_hMod == NULL)
    {
        TINYCLR_SET_AND_LEAVE(HRESULT_FROM_WIN32(::GetLastError()));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT PELoader::OpenAndDecode( LPCWSTR moduleName )
{
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(OpenAndMapToMemory( moduleName ));

    TINYCLR_CHECK_HRESULT(Initialize());

    TINYCLR_NOCLEANUP();
}

HRESULT PELoader::Initialize()
{
    TINYCLR_HEADER();

    IMAGE_DOS_HEADER* pdosHeader = (IMAGE_DOS_HEADER*)m_hMod; // get the dos header...

    if(pdosHeader->e_magic == IMAGE_DOS_SIGNATURE && 0 < pdosHeader->e_lfanew && pdosHeader->e_lfanew < 0xFF0) // has to start on first page
    {
        m_pNT = (IMAGE_NT_HEADERS*)(pdosHeader->e_lfanew + (DWORD_PTR)m_hMod);

        if((m_pNT->Signature                       != IMAGE_NT_SIGNATURE             ) ||
           (m_pNT->OptionalHeader.Magic            != IMAGE_NT_OPTIONAL_HDR_MAGIC    )  )
        {
            // @TODO - add some SetLastError info? Not sure that in this case this could happen...But!
            // Make this appear uninitalized because for some reason this file is toast...
            // Not sure that this could ever happen because this file has already been loaded
            // bye the system loader unless someone gave us garbage as the hmod
            m_pNT  = NULL;
            m_hMod = NULL;

            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
    }
    else
    {
        m_hMod = NULL;

        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_NOCLEANUP();
}

bool PELoader::GetCOMHeader( IMAGE_COR20_HEADER*& pCorHeader )
{
    // Get the image header from the image, then get the directory location
    // of the COM+ header which may or may not be filled out.
    DWORD                 pCOMHeader     = m_pNT->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_COMHEADER].VirtualAddress;
    PIMAGE_SECTION_HEADER pSectionHeader = (PIMAGE_SECTION_HEADER)Cor_RtlImageRvaToVa( m_pNT, m_hMod, pCOMHeader, NULL );

    // If the section header exists, then return ok and the address.
    if(pSectionHeader)
    {
        pCorHeader = (IMAGE_COR20_HEADER*)pSectionHeader;

        return true;
    }
    // If there is no COM+ Data in this image, return false.
    else
    {
        pCorHeader = NULL;

        return false;
    }
}

bool PELoader::GetResource( DWORD dwOffset, BYTE*& pResource, DWORD& dwSize )
{
    IMAGE_COR20_HEADER *pCorHeader;

    if(GetCOMHeader( pCorHeader ))
    {
        pResource = (BYTE *)Cor_RtlImageRvaToVa( m_pNT, m_hMod, pCorHeader->Resources.VirtualAddress, NULL );

        if(pResource && dwOffset < pCorHeader->Resources.Size)
        {
            pResource += dwOffset;

            memcpy( &dwSize, pResource, sizeof(dwSize) );

            pResource += sizeof(dwSize);

            return true;
        }
    }

    return false;
}

bool PELoader::GetVAforRVA( DWORD rva, void*& va )
{
    PIMAGE_SECTION_HEADER pSectionHeader = (PIMAGE_SECTION_HEADER)Cor_RtlImageRvaToVa( m_pNT, m_hMod, rva, NULL );

    // If the section header exists, then return ok and the address.
    if(pSectionHeader)
    {
        va = pSectionHeader;

        return true;
    }
    // If there is no COM+ Data in this image, return false.
    else
    {
        va = NULL;

        return false;
    }
}

//      Main.cs(17,20):Command line warning CS0168: The variable 'foo' is declared but never used
//      -------------- ------------ ------- ------  ----------------------------------------------
//      Origin         SubCategory  Cat.    Code    Text

/**      (line)
     *      (line-line)
     *      (line,col)
     *      (line,col-col)
     *      (line,col,len)
     *      (line,col,line,col)
*/

void ErrorReporting::Print( LPCWSTR szOrigin, LPCWSTR szSubCategory, BOOL fError, int code, LPCWSTR szTextFormat, ... )
{
    va_list arg;

    va_start( arg, szTextFormat );

                       wprintf( L"%s: ", szOrigin ? szOrigin : L"MMP"                 );
    if(szSubCategory)  wprintf( L"%s " , szSubCategory                                );
    /***************/  wprintf( L"%s MMP%04d: ", fError ? L"error" : L"warning", code );
    if(szTextFormat )  vwprintf( szTextFormat, arg                                    );
    /***************/  wprintf( L"\n"                                                 );

    va_end( arg );

}

//VS#299537 will define this constant in corsym header files
#define NO_SOURCE_AVAILABLE 0x00FeeFee

HRESULT ErrorReporting::ConstructErrorOrigin( std::wstring &str, ISymUnmanagedReader* pSymReader, mdMethodDef md, ULONG32 ipOffset )
{
    TINYCLR_HEADER();

    ISymUnmanagedMethodPtr pMethod = NULL;
    ULONG32 cSeqPoints             = 0;
    ULONG32* pOffsets              = NULL;
    ULONG32* pLines                = NULL;
    ULONG32* pCols                 = NULL;
    ULONG32* pEndCols              = NULL;
    ULONG32* pEndLines             = NULL;
    ISymUnmanagedDocument** pDocs  = NULL;
    WCHAR szName[512];
    WCHAR buf[512];

    if(!pSymReader) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    TINYCLR_CHECK_HRESULT(pSymReader->GetMethod( md, &pMethod ));

    TINYCLR_CHECK_HRESULT(pMethod->GetSequencePointCount( &cSeqPoints ));

    pOffsets  = new ULONG32[cSeqPoints];
    pDocs     = new ISymUnmanagedDocument*[cSeqPoints];
    pLines    = new ULONG32[cSeqPoints];
    pCols     = new ULONG32[cSeqPoints];
    pEndLines = new ULONG32[cSeqPoints];
    pEndCols  = new ULONG32[cSeqPoints];

    TINYCLR_CHECK_HRESULT(pMethod->GetSequencePoints( cSeqPoints, &cSeqPoints, pOffsets, pDocs, pLines, pCols, pEndLines, pEndCols ));

    for(ULONG32 i = 0; i < cSeqPoints; i++)
    {
        if(i == cSeqPoints || ipOffset < pOffsets[i+1])
        {
            //We found the correct offset.  Now find the closest line of code

            while(i > 0              && pLines[i] == NO_SOURCE_AVAILABLE) i--;
            while(i < cSeqPoints - 1 && pLines[i] == NO_SOURCE_AVAILABLE) i++;

            if(pLines[i] == NO_SOURCE_AVAILABLE) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

            ISymUnmanagedDocument *pDoc = pDocs[i];
            TINYCLR_CHECK_HRESULT(pDoc->GetURL( ARRAYSIZE(szName), NULL, szName ));

            _snwprintf_s( buf, ARRAYSIZE(buf), L"%s(%d,%d,%d,%d)", szName, pLines[i], pCols[i], pEndLines[i], pEndCols[i] );

            str = buf;
            break;
        }
    }

    TINYCLR_CLEANUP();

    if(pDocs)
    {
        for(ULONG32 i = 0; i < cSeqPoints; i++)
        {
            if(pDocs[i])
            {
                pDocs[i]->Release();
            }
        }
    }

    delete[] pOffsets;
    delete[] pDocs;
    delete[] pLines;
    delete[] pCols;
    delete[] pEndLines;
    delete[] pEndCols;

    TINYCLR_CLEANUP_END();
}
