////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

#define UTF8_BAD_LOWPART(ch) (ch < 0x80 || ch > 0xBF)

#define UTF8_CHECK_LOWPART(ch,src) ch = (CLR_UINT32)*src++; if(UTF8_BAD_LOWPART(ch)) return -1

#define UTF8_LOAD_LOWPART(ch,ch2,src) ch = (CLR_UINT32)*src++; ch2 <<= 6; ch2 |=  (ch & 0x3F)

//--//

int CLR_RT_UnicodeHelper::CountNumberOfCharacters( int max )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_UINT8* pSrc = m_inputUTF8;
    int              num  = 0;

    while(true)
    {
        CLR_UINT32 ch = (CLR_UINT32)*pSrc++; if(!ch) break;

        if(max-- == 0) break; // This works even if you pass -1 as argument (it will walk through the whole string).

        switch(ch & 0xF0)
        {
        case 0x00:
        case 0x10:
        case 0x20:
        case 0x30:
        case 0x40:
        case 0x50:
        case 0x60:
        case 0x70:
            num += 1;
            break;

        case 0x80:
        case 0x90:
        case 0xA0:
        case 0xB0:
            return -1; // Illegal characters.

        case 0xC0:
        case 0xD0:
            UTF8_CHECK_LOWPART(ch,pSrc);

            num += 1;
            break;

        case 0xE0:
            UTF8_CHECK_LOWPART(ch,pSrc);
            UTF8_CHECK_LOWPART(ch,pSrc);

            num += 1;
            break;

        case 0xF0:
            UTF8_CHECK_LOWPART(ch,pSrc);
            UTF8_CHECK_LOWPART(ch,pSrc);
            UTF8_CHECK_LOWPART(ch,pSrc);

            num += 2;
            break;
        }
    }

    return num;
}

int CLR_RT_UnicodeHelper::CountNumberOfBytes( int max )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_UINT16* pSrc = m_inputUTF16;
    int               num  = 0;

    while(true)
    {
        CLR_UINT16 ch = *pSrc++; if(!ch) break;

        if(max-- == 0) break; // This works even if you pass -1 as argument (it will walk through the whole string).

        if(ch < 0x0080)
        {
            num += 1;
        }
        else if(ch < 0x0800)
        {
            num += 2;
        }
        else if(ch >= LOW_SURROGATE_START && ch <= LOW_SURROGATE_END)
        {
            return -1; // Invalid string: Low surrogate should only follow a high surrogate.
        }
        else if(ch >= HIGH_SURROGATE_START && ch <= HIGH_SURROGATE_END)
        {
            ch = *pSrc++;

            if(ch >= LOW_SURROGATE_START && ch <= LOW_SURROGATE_END)
            {
                num += 4;
                max--;
            }
            else
            {
                return -1; // Invalid string: Low surrogate should follow a high surrogate.
            }
        }
        else
        {
            num += 3;
        }
    }

    return num;
}

//--//

bool CLR_RT_UnicodeHelper::ConvertFromUTF8( int iMaxChars, bool fJustMove, int iMaxBytes )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_UINT8* inputUTF8        = m_inputUTF8;
    CLR_UINT16*      outputUTF16      = m_outputUTF16;
    int              outputUTF16_size = m_outputUTF16_size;
    CLR_UINT32       ch;
    CLR_UINT32       ch2;
    bool             res;

    if (iMaxBytes == -1)
    {
        iMaxBytes = iMaxChars * 3; // the max number of bytes it can have based on iMaxChars -- if all characters are of 3 bytes. 
    }

    while(iMaxChars > 0 && iMaxBytes > 0)
    {
        ch = (CLR_UINT32)*inputUTF8++;

        switch(ch & 0xF0)
        {
        case 0x00: if(ch == 0) { inputUTF8--; goto ExitFalse; }
        case 0x10:
        case 0x20:
        case 0x30:
        case 0x40:
        case 0x50:
        case 0x60:
        case 0x70:
            if(fJustMove == false)
            {
                if(outputUTF16_size < 1)
                {
                    // un-read the byte before exiting
                    inputUTF8--;
                    goto ExitFalse;
                }

                outputUTF16[ 0 ] = ch;

                outputUTF16      += 1;
                outputUTF16_size -= 1;
            }

            iMaxChars -= 1;
            iMaxBytes -= 1;
            break;

        case 0x80:
        case 0x90:
        case 0xA0:
        case 0xB0:
            goto ExitFalse; // Illegal characters.

        case 0xC0:
        case 0xD0:
            if((ch & 0xFF) < 0xC2) { inputUTF8--; goto ExitFalse; } // illegal - overlong encoding

            if(iMaxBytes >= 2)
            {
                if(fJustMove)
                {
                    inputUTF8++;
                }
                else
                {
                    if(outputUTF16_size < 1)
                    {
                        // un-read the byte before exiting
                        inputUTF8--;
                        goto ExitFalse;
                    }
                    ch2 = (ch & 0x1F);
                    UTF8_LOAD_LOWPART(ch,ch2,inputUTF8);

                    outputUTF16[ 0 ] = ch2;

                    outputUTF16      += 1;
                    outputUTF16_size -= 1;
                }

                iMaxChars -= 1;
                iMaxBytes -= 2;
            }
            else
            {
                // un-read the byte before exiting
                inputUTF8--;
                goto ExitFalse;
            }
            break;

        case 0xE0:
            if(iMaxBytes >= 3)
            {
                if(fJustMove)
                {
                    inputUTF8 += 2;
                }
                else
                {
                    if(outputUTF16_size < 1)
                    {
                        // un-read the byte before exiting
                        inputUTF8--;
                        goto ExitFalse;
                    }
                    ch2 = (ch & 0x0F);
                    UTF8_LOAD_LOWPART(ch,ch2,inputUTF8);
                    UTF8_LOAD_LOWPART(ch,ch2,inputUTF8);

                    outputUTF16[ 0 ] = ch2;

                    outputUTF16      += 1;
                    outputUTF16_size -= 1;
                }

                iMaxChars -= 1;
                iMaxBytes -= 3;
            }
            else
            {
                // un-read the byte before exiting
                inputUTF8--;
                goto ExitFalse;
            }
            break;

        case 0xF0:
            if((ch & 0xFF) >= 0xF5) { inputUTF8--; goto ExitFalse; } // restricted by RFC 3629

            if(iMaxBytes >= 4)
            {
                if(fJustMove)
                {
                    inputUTF8 += 3;
                }
                else
                {
                    if(outputUTF16_size < 2)
                    {
                        // un-read the byte before exiting
                        inputUTF8--;
                        goto ExitFalse;
                    }

                    ch2 = (ch & 0x07);
                    UTF8_LOAD_LOWPART(ch,ch2,inputUTF8);
                    UTF8_LOAD_LOWPART(ch,ch2,inputUTF8);
                    UTF8_LOAD_LOWPART(ch,ch2,inputUTF8);

                    outputUTF16[ 0 ] = (ch2 >> SURROGATE_HALFSHIFT) + HIGH_SURROGATE_START;
                    outputUTF16[ 1 ] = (ch2 &  SURROGATE_HALFMASK ) + LOW_SURROGATE_START ;

                    outputUTF16      += 2;
                    outputUTF16_size -= 2;
                }

                iMaxChars -= 2;
                iMaxBytes -= 4;
            }
            else
            {
                // un-read the byte before exiting
                inputUTF8--;
                goto ExitFalse;
            }
            break;
        }
    }

    if(fJustMove == false)
    {
        if(outputUTF16_size < 1) goto ExitFalse;

        outputUTF16[ 0 ] = 0;
    }

    res = true;
    goto Exit;

ExitFalse:
    res = false;

Exit:
    m_inputUTF8        = inputUTF8;
    m_outputUTF16      = outputUTF16;
    m_outputUTF16_size = outputUTF16_size;

    return res;
}

bool CLR_RT_UnicodeHelper::ConvertToUTF8( int iMaxChars, bool fJustMove )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_UINT16* inputUTF16      = m_inputUTF16;
    CLR_UINT8*        outputUTF8      = m_outputUTF8;
    int               outputUTF8_size = m_outputUTF8_size;
    CLR_UINT32        ch;
    bool              res;

    while(iMaxChars > 0)
    {
        ch = (CLR_UINT32)*inputUTF16++;

        if(ch < 0x0080)
        {
            if(ch == 0)
            {
                break;
            }

            if(fJustMove == false)
            {
                if(outputUTF8_size < 1) goto ExitFalse;

                outputUTF8[ 0 ] = ch;

                outputUTF8      += 1;
                outputUTF8_size -= 1;
            }

            iMaxChars -= 1;
        }
        else if(ch < 0x0800)
        {
            if(fJustMove == false)
            {
                if(outputUTF8_size < 2) goto ExitFalse;

                outputUTF8[ 1 ] = 0x80 | (ch & 0x3F); ch >>= 6;
                outputUTF8[ 0 ] = 0xC0 | (ch & 0x1F);

                outputUTF8      += 2;
                outputUTF8_size -= 2;
            }

            iMaxChars -= 1;
        }
        else if(ch >= HIGH_SURROGATE_START && ch <= HIGH_SURROGATE_END)
        {
            ch = SURROGATE_HALFBASE + ((ch - HIGH_SURROGATE_START) << SURROGATE_HALFSHIFT) + (*inputUTF16++ - LOW_SURROGATE_START);

            if(fJustMove == false)
            {
                if(outputUTF8_size < 4) goto ExitFalse;

                outputUTF8[ 3 ] = 0x80 | (ch & 0x3F); ch >>= 6;
                outputUTF8[ 2 ] = 0x80 | (ch & 0x3F); ch >>= 6;
                outputUTF8[ 1 ] = 0x80 | (ch & 0x3F); ch >>= 6;
                outputUTF8[ 0 ] = 0xF0 | (ch & 0x07);

                outputUTF8      += 4;
                outputUTF8_size -= 4;
            }

            iMaxChars -= 2;
        }
        else
        {
            if(fJustMove == false)
            {
                if(outputUTF8_size < 3) goto ExitFalse;

                outputUTF8[ 2 ] = 0x80 | (ch & 0x3F); ch >>= 6;
                outputUTF8[ 1 ] = 0x80 | (ch & 0x3F); ch >>= 6;
                outputUTF8[ 0 ] = 0xE0 | (ch & 0x0F);

                outputUTF8      += 3;
                outputUTF8_size -= 3;
            }

            iMaxChars -= 1;
        }
    }

    if(fJustMove == false)
    {
        if(outputUTF8_size < 1) goto ExitFalse;

        outputUTF8[ 0 ] = 0;
    }

    res = true;
    goto Exit;

ExitFalse:
    res = false;

Exit:
    m_inputUTF16      = inputUTF16;
    m_outputUTF8      = outputUTF8;
    m_outputUTF8_size = outputUTF8_size;

    return res;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(_WIN32)

void CLR_RT_UnicodeHelper::ConvertToUTF8( const std::wstring& src ,
                                          std::string&        dst )
{
    if(src.size())
    {
        LPSTR szBuf;
        int   iSize;

        iSize = ::WideCharToMultiByte( CP_UTF8, 0, src.c_str(), -1, NULL, 0, NULL, NULL );

        if(iSize > 0)
        {
            szBuf = new CHAR[ iSize ];

            iSize = ::WideCharToMultiByte( CP_UTF8, 0, src.c_str(), -1, szBuf, iSize, NULL, NULL );
            if(iSize > 0)
            {
                dst = szBuf;
            }

            delete [] szBuf;
        }
    }
    else
    {
        dst.erase();
    }
}

void CLR_RT_UnicodeHelper::ConvertFromUTF8( const std::string& src ,
                                            std::wstring&      dst )
{
    if(src.size())
    {
        LPWSTR szBuf;
        int    iSize;

        iSize = ::MultiByteToWideChar( CP_UTF8, 0, src.c_str(), -1, NULL, 0 );

        if(iSize > 0)
        {
            szBuf = new WCHAR[ iSize ];

            iSize = ::MultiByteToWideChar( CP_UTF8, 0, src.c_str(), -1, szBuf, iSize );
            if(iSize > 0)
            {
                dst = szBuf;
            }

            delete [] szBuf;
        }
    }
    else
    {
        dst.erase();
    }
}


#endif

UnicodeString::UnicodeString()
{
    m_wCharArray = NULL;
    m_length = 0;
}

UnicodeString::~UnicodeString()
{
    Release();
}

HRESULT UnicodeString::Assign( LPCSTR string )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    int byteLength = 0;

    /// Before we start assigning remove existing stuff.
    Release();

    m_unicodeHelper.SetInputUTF8( string );

    m_length = m_unicodeHelper.CountNumberOfCharacters(); 
   
    if(m_length < 0) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER); 
    /// We have m_length >=0 now.

    byteLength = (m_length + 1) * sizeof (CLR_UINT16);

    m_wCharArray = (CLR_UINT16 *)CLR_RT_Memory::Allocate( byteLength );  CHECK_ALLOCATION(m_wCharArray);

    m_unicodeHelper.m_outputUTF16      = m_wCharArray;
    m_unicodeHelper.m_outputUTF16_size = m_length + 1;

    m_unicodeHelper.ConvertFromUTF8( m_length , false );

    /// Note m_length > 0 already (see above), hence m_length >= 0 or a valid index.
    m_wCharArray[ m_length ] = 0;

    TINYCLR_NOCLEANUP();
}
    
void UnicodeString::Release()
{
    if (m_wCharArray != NULL)
    {
        CLR_RT_Memory::Release( m_wCharArray );
    }

    m_wCharArray = NULL;
}

