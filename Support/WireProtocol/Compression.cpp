////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include <TinySupport.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#define WINSIZE  4096
#define MAXLEN   18

#define LENGTH(x) ((((x) & 0x0F)) + 3)
#define OFFSET(x1, x2) ((((((x2 & 0xF0) >> 4) * 0x0100) + x1) & 0x0FFF) + 0x0010)

#define FAKE2REAL_POS(x)   ((x) & (WINSIZE - 1))
#define BITSET(byte, bit)  (((byte) & (1<<bit)) > 0)

#define COMP_CODE(x,y)      ((((x-3) & 0x0F) << 8) + (((y - 0x10) & 0x0F00) << 4) + ((y - 0x10) & 0x00FF))
#define COMP_CODE_LOBYTE(x) ((unsigned char)(x))
#define COMP_CODE_HIBYTE(x) ((unsigned char)(((unsigned short)(x) >> 8) & 0xFF))

#define DROP_INDEX(x) (x == 0) ? (WINSIZE - 1) : (x - 1)
#define ADD_INDEX(x)  ((x + 1) == WINSIZE) ? 0 : (x + 1)

//--//

int LZ77_Decompress( UINT8* inBuf, int inSize, UINT8* outBuf, int outSize, WRITE_MEMORY_FUNCT writeMem, READ_MEMORY_FUNCT readMem)
{
    UINT8* inBufEnd  = inBuf  + inSize;
    UINT8* outBufEnd = outBuf + outSize;
    int    counter;
    int    currPos = 0;
    UINT8  window[ WINSIZE ];

    for(counter = 0; counter < WINSIZE; counter ++)
    {
        window[ counter ] = ' ';
    }

//#define CHECKEDREAD(x)  if(inBuf  >= inBufEnd ) { DebugBreak(); return -1; } x
//#define CHECKEDWRITE(x) if(outBuf >= outBufEnd) { DebugBreak(); return -1; } x
#define CHECKEDREAD(x,y)  if(inBuf  >= inBufEnd ) return -1; if(readMem  != NULL) readMem ( (UINT32)y, 1, &x ); else  x = *y
#define CHECKEDWRITE(x,y) if(outBuf >= outBufEnd) return -1; if(writeMem != NULL) writeMem( (UINT32)x, 1, &y ); else *x =  y

    while(inBuf < inBufEnd)
    {
        //
        // Get BitMap and data following it.
        //
        UINT8 bitMap;
        
        CHECKEDREAD(bitMap, inBuf++);

        //
        // Go through and decode data.
        //
        for(counter = 0; counter < 8 && outBuf < outBufEnd; counter++)
        {
            //
            // It's a code, so decode it and copy the data.
            //
            if(!BITSET(bitMap, counter))
            {
                UINT8 byte1;
                UINT8 byte2;
                
                CHECKEDREAD(byte1, inBuf++);
                CHECKEDREAD(byte2, inBuf++);

                int length = LENGTH(byte2);
                int offset = OFFSET(byte1, byte2);

                //
                // Copy data from window.
                //
                while(length)
                {
                    byte1 = window[ FAKE2REAL_POS(offset) ];

                    window[ FAKE2REAL_POS(currPos) ] = byte1;

                    CHECKEDWRITE(outBuf++, byte1);

                    currPos++;
                    offset++; 
                    length--;
                }
            }
            else
            {
                UINT8 byte1;
                
                CHECKEDREAD(byte1, inBuf++);

                window[ FAKE2REAL_POS(currPos) ] = byte1;

                CHECKEDWRITE(outBuf++, byte1);

                currPos++;
            }
        }
    }

#undef CHECKEDREAD
#undef CHECKEDWRITE

    return 1;
}


int LZ77_Decompress( UINT8* inBuf, int inSize, UINT8* outBuf, int outSize)
{
    return LZ77_Decompress(inBuf, inSize, outBuf, outSize, NULL, NULL);
}

//--//

#if defined(_WIN32) || defined(WIN32) || defined(_WIN32_WCE)

#include <vector>

struct LZ77_Compressor
{
    typedef std::vector< UINT8 > Buffer;

    struct Header
    {
        int m_compressed;
        int m_decompressed;
    };

    UINT8   m_window[ WINSIZE ]; // Compression Window.
    int     m_currPos;

    UINT8   m_flagByte;          // Byte at the start of each block of compressed data.
    UINT8   m_dataBytes[ 17 ];   // Block of compressed data.
    int     m_dataCount;         // Number of chars in current m_dataBytes[].
    int     m_flagCount;         // Number of items in m_flagByte.

    Buffer& m_infile;
    size_t  m_infilePos;
    size_t  m_infileSize;

    Buffer& m_outfile;

    //--//

    LZ77_Compressor( Buffer& infile, Buffer& outfile ) : m_infile(infile), m_outfile(outfile)
    {
        m_currPos    = 0;

        m_flagByte   = 0;
        m_dataCount  = 0;
        m_flagCount  = 0;

        m_infilePos  = 0;
        m_infileSize = infile.size();

        m_outfile.reserve( m_infileSize );
    }

    //--//

    static bool LoadFile( LPCWSTR szFile, Buffer& vec )
    {
        bool  fRes   = false;
        FILE* stream;
#if defined _WIN32_WCE
        if(NULL != (stream =_wfopen(szFile,L"rb")))        
#else
        if(_wfopen_s(&stream, szFile, L"rb" ) == 0)
#endif
        {
            /*********/ fseek( stream, 0, SEEK_END );
            long size = ftell( stream              );
            /*********/ fseek( stream, 0, SEEK_SET );

            vec.resize( size );

            if(fread( &vec[0], vec.size(), 1, stream ) == 1)
            {
                fRes = true;
            }

            fclose( stream );
        }

        return fRes;
    }

    static bool SaveFile( LPCWSTR szFile, const Buffer& vec, UINT8* prefix, size_t prefixLength )
    {
        bool  fRes   = false;
        FILE* stream;
#if defined _WIN32_WCE
        if(NULL != (stream =_wfopen(szFile,L"rb")))
#else
        if(_wfopen_s(&stream, szFile, L"wb" ) == 0)
#endif
        {
            INT32 mod = (prefixLength + vec.size()) % 4;
            fRes = true;

            if(prefix && fwrite( prefix , prefixLength, 1, stream ) != 1) fRes = false;
            if(          fwrite( &vec[0], vec.size()  , 1, stream ) != 1) fRes = false;

            if(mod != 0)
            {
                byte tmp[4] = {0x00, 0x00, 0x00, 0x00};

                fwrite( tmp, 1, 4 - mod, stream );
            }

            fclose( stream );
        }

        return fRes;
    }

    //--//

    void LeaveSpaceForHeader()
    {
        for(int pos=0; pos<sizeof(Header); pos++)
        {
            WriteByte( 0 );
        }
    }

    bool Execute()
    {
        int  count;
        int  shifter;
        int  offset    = 0;
        int  newPos    = 0;
        int  ch;
        char oldchars[ 3 ];

        for(count = 0; count < WINSIZE; count ++)
        {
            m_window[ count ] = ' ';
        }

        //
        // Go through input file until we're done.
        //
        m_currPos = DROP_INDEX(m_currPos);
        ch = GetNextChar();
        while(ch != EOF)
        {
            //
            // If less than 3 chars from end, just write out remainder.
            //
            if((m_infileSize - m_infilePos) < 2)
            {
                SaveUncompByte( ch );
                ch = GetNextChar();
                continue;
            } 

            //
            // Find previous occurrence of character in window.
            //
            count   = 1;
            shifter = DROP_INDEX(m_currPos);
            
            while((m_window[ shifter ] != m_window[ m_currPos ]) && (count < WINSIZE))
            {
                count++;
                shifter = DROP_INDEX(shifter);
            }

            //
            // Check if char is unique so far in input file.
            //
            if(count == WINSIZE)
            {
                SaveUncompByte( ch );
                ch = GetNextChar();
                continue;
            }
            else
            {
                //
                // Find out how many characters match.
                //
                int savePos = m_currPos;

                oldchars[ 2 ] = oldchars[ 0 ] = m_window[ ADD_INDEX(m_currPos) ];

                count   = 1;
                offset  = shifter;
                shifter = ADD_INDEX(shifter);

                while((ch = GetNextChar()) != EOF && (m_window[ shifter ] == ch) && (count < MAXLEN))
                {
                    ++count;

                    if(count == 2) oldchars[ 1 ] = m_window[ ADD_INDEX(m_currPos) ];
                    /************/ oldchars[ 2 ] = m_window[ ADD_INDEX(m_currPos) ];

                    shifter = ADD_INDEX(shifter); 
                }

                //
                // Since this is the first match, save it as the best so far.
                //
                int bestcount  = count;
                int bestoffset = offset;

                if(ch != EOF && ((m_window[ shifter ] != ch) || (count == MAXLEN)))
                {
                    UnreadChar( oldchars[ 2 ] );
                }

                //
                // Now find the best match for the string in the window.
                //
                shifter = DROP_INDEX( offset );

                while((shifter != m_currPos) && (bestcount < MAXLEN) && (!InBetween( savePos, m_currPos, shifter )))
                {
                    while((shifter != m_currPos) && (m_window[ shifter ] != m_window[ savePos ]))
                    {
                        shifter = DROP_INDEX(shifter);
                    }

                    if(shifter == m_currPos) break;

                    count  = 0;
                    offset = shifter;
                    newPos = savePos;

                    while(ch != EOF && (m_window[ shifter ] == m_window[ newPos ]) && (count < MAXLEN))
                    {
                        if(count >= (bestcount - 1))
                        {
                            if(count == 1) oldchars[ 1 ] = m_window[ ADD_INDEX(m_currPos) ];
                            /************/ oldchars[ 2 ] = m_window[ ADD_INDEX(m_currPos) ];

                            ch = GetNextChar();
                        }

                        shifter = ADD_INDEX(shifter);
                        newPos  = ADD_INDEX(newPos );
                        count++;
                    }

                    if(ch != EOF && ((count >= MAXLEN) || ((m_window[ shifter ] != m_window[ newPos ]) && (count >= bestcount))))
                    {
                        UnreadChar( oldchars[ 2 ] );
                    }

                    if(count > bestcount)
                    {
                        bestcount  = count;
                        bestoffset = offset;
                    }

                    shifter = DROP_INDEX(offset);
                }

                if(ch != EOF)
                {
                    ch = GetNextChar();
                }

                count  = bestcount;
                offset = bestoffset;

                //
                // If count < 3, then not enough chars to compress.
                //
                if(count < 3)
                {
                    SaveUncompByte( m_window[ savePos ] );

                    m_infilePos -= count;

                    m_window[ savePos = ADD_INDEX(savePos) ] = oldchars[ 0 ];

                    m_currPos = DROP_INDEX(m_currPos);

                    if(count == 2)
                    {
                        m_window[ ADD_INDEX(savePos) ] = oldchars[ 1 ];

                        m_currPos = DROP_INDEX(m_currPos);
                    }
                }
                else
                {
                    int iCompCode = COMP_CODE(count, offset);

                    m_dataBytes[   m_dataCount ] = COMP_CODE_LOBYTE( iCompCode );
                    m_dataBytes[ ++m_dataCount ] = COMP_CODE_HIBYTE( iCompCode );

                    CheckFlagByte();

                    if(ch != EOF)
                    {
                        UnreadChar( oldchars[ 2 ] );
                    }
                }

                if(ch != EOF && (count <= MAXLEN))
                {
                    ch = GetNextChar();
                }
            }
        }

        WriteFlagByte();

        return true;
    }

    void EmitHeader()
    {
        Header* ptr = (Header*)&m_outfile[ 0 ];

        ptr->m_compressed   = (int)(m_outfile.size() - sizeof(*ptr));
        ptr->m_decompressed = (int) m_infile .size();
    }

private:
    //
    // InBetween - detects wrap around of indices.
    //
    bool InBetween( int lower, int higher, int target )
    {
        if(higher < lower) higher += WINSIZE;

        return (lower <= target) && (target <= higher);
    }

    void WriteByte( UINT8 ch )
    {
        m_outfile.push_back( ch );
    }

    //
    // Flush data buffers.
    //
    void WriteFlagByte()
    {
        if(m_flagCount > 0)
        {
            int index;

            m_dataBytes[ m_dataCount ] = '\0';

            WriteByte( m_flagByte );

            for(index = 0; index < m_dataCount; ++index)
            {
                WriteByte( m_dataBytes[ index ] );
            }

            m_dataCount = 0;
            m_flagCount = 0;
            m_flagByte  = 0;

            for(index = 0; index < sizeof(m_dataBytes); ++index)
            {
                m_dataBytes[ index ] = ' ';
            }
        }
    }

    //
    // Check if the flag byte is full and should be printed out.
    //
    void CheckFlagByte()
    {
        m_dataCount++;

        if(++m_flagCount == 8)
        {
            WriteFlagByte();
        }
    }

    //
    // Saves an uncompressed data byte.
    //
    int GetNextChar()
    {
        int ch;

        if(m_infilePos < m_infileSize)
        {
            ch = m_infile[ m_infilePos++ ];

            m_currPos = ADD_INDEX(m_currPos);

            m_window[ m_currPos ] = ch;
        }
        else
        {
            ch = EOF;
        }

        return ch;
    }

    //
    // Unreads the last character read.
    //
    void UnreadChar( UINT8 ch )
    {
        m_infilePos--;

        m_window[ m_currPos ] = ch;

        m_currPos = DROP_INDEX(m_currPos);
    }

    //
    // Saves an uncompressed data byte.
    //
    void SaveUncompByte( UINT8 ch )
    {
        m_flagByte |= 1 << m_flagCount;

        m_dataBytes[ m_dataCount ] = ch;

        CheckFlagByte();
    }
};

bool LZ77_Compress( LPCWSTR inFileText, LPCWSTR outFileText, UINT8* prefix, size_t prefixLength )
{
    LZ77_Compressor::Buffer inBuf;
    LZ77_Compressor::Buffer outBuf;
    bool                    fRes = false;

    if(LZ77_Compressor::LoadFile( inFileText, inBuf ) == false)
    {
        wprintf( L"%s does not exist\n", inFileText );
    }
    else
    {
        LZ77_Compressor hlp( inBuf, outBuf );

        hlp.LeaveSpaceForHeader();

        fRes = hlp.Execute();

        hlp.EmitHeader();

        if(fRes)
        {
            if(LZ77_Compressor::SaveFile( outFileText, outBuf, prefix, prefixLength ) == false)
            {
                wprintf( L"%s cannot be created\n", outFileText );
            }
            else
            {
                UINT8* ptr    =        &outBuf[0];
                int    len    =  (int ) outBuf.size();
                int    lenIn  = *(int*)ptr; ptr += sizeof(int);
                int    lenOut = *(int*)ptr; ptr += sizeof(int);

                if(lenIn != (len - sizeof(LZ77_Compressor::Header)) || lenOut != inBuf.size())
                {
                    wprintf( L"Invalid header!\n" );
                }
                else
                {
                    LZ77_Compressor::Buffer verBuf; verBuf.resize( lenOut );

                    if(LZ77_Decompress( ptr, lenIn, &verBuf[ 0 ], lenOut ) == false)
                    {
                        wprintf( L"Decompression failed!\n" );
                    }
                    else if(memcmp( &verBuf[ 0 ], &inBuf[ 0 ], (int)verBuf.size() ) != 0)
                    {
                        wprintf( L"Verification failed!\n" );
                    }
                    else
                    {
                        fRes = true;
                    }
                }
            }
        }
    }

    return fRes;
}

#endif
