#include "SrecProcessor.h"

extern BOOL MemStreamSeekBlockAddress( BlockStorageStream &stream, UINT32 address );
typedef void (*ApplicationStartAddress)();


void SREC_Handler::Initialize()
{
    m_Pos = 0;
    m_Failures = 0;
    m_StartAddress = 0xFFFFFFFF;
    m_ImageStart = 0;
    m_ImageCRC = 0;
    m_ImageLength = 0;
    m_BootMarkerAddress = 0;
    m_isRamBuild = FALSE;

    m_pMemCfg = NULL;

    memset(&m_Stream, 0, sizeof(m_Stream));

    m_Stream.SetReadModifyWrite();
}

BOOL SREC_Handler::Process( char c )
{
    switch(c)
    {
    case XON:
    case XOFF:
        // swallow these and go on
        break;

    case '\r':
    case '\n':
        {
            // terminate string!
            m_LineBuffer[m_Pos] = 0;

            if(ParseLine( m_LineBuffer, FALSE ))
            {
                if(m_StartAddress != 0xFFFFFFFF)
                {
                    UINT32 dstExec = m_ImageStart;

                    if(!m_isRamBuild)
                    {
                        // add the boot marker since we completed the entire download without errors
                        if(m_BootMarkerAddress != 0 && MemStreamSeekBlockAddress(m_Stream, m_BootMarkerAddress))
                        {
                            UINT32 marker = MicroBooter_ProgramMarker();
                            
                            m_Stream.Write( (UINT8*)&marker, sizeof(UINT32) );
                        }

                        if(!m_Stream.IsXIP())
                        {
                            dstExec = (UINT32)private_malloc(m_ImageLength);
                        
                            MemStreamSeekBlockAddress( m_Stream, m_ImageStart );
                                
                            m_Stream.ReadIntoBuffer( (UINT8*)dstExec, m_ImageLength );
                        }
                    }

                    if(m_ImageCRC == 0 || (m_ImageCRC == SUPPORT_ComputeCRC((UINT32*)dstExec, m_ImageLength, 0)))
                    {
                        SignalSuccess();
                        //SignalSuccess();

                        // give response some time to process
                        Events_WaitForEvents(0, 200);

                        if(m_isRamBuild)
                        {
                            ApplicationStartAddress str = (ApplicationStartAddress)m_ImageStart;

                            LCD_Clear();
                            
                            DebuggerPort_Uninitialize( HalSystemConfig.DebuggerPorts[0] );
                            
                            DISABLE_INTERRUPTS();
                            
                            LCD_Uninitialize();
                            
                            CPU_DisableCaches();

                            (*str)();
                        }
                        else
                        {
                            CPU_Reset();
                        }
                    }

                    if(m_BootMarkerAddress != 0 && MemStreamSeekBlockAddress(m_Stream, m_BootMarkerAddress))
                    {
                        m_Stream.Erase(m_Stream.BlockLength);
                    }

                    // after getting a failed complete download, clear the error and start anew
                    m_StartAddress = 0xFFFFFFFF;
                    m_Failures     = FALSE;

                    SignalFailure();
                }
                else
                {
                    SignalSuccess();
                }
            }
            else if(m_Pos != 0)
            {
                // bad characters! realign and don't exec
                m_Failures = TRUE;

                SignalFailure();
            }

            m_Pos = 0;

            return FALSE; // Always align again after getting a full record.
        }
        break;

    default:
        {
            // if we have a non whitespace character, store it
            m_LineBuffer[m_Pos++] = c;

            // don't overrun stuff in case of bizarre failures that appear OK char by char
            if(m_Pos >= sizeof(m_LineBuffer))
            {
                m_Failures  = TRUE;
                m_Pos       = 0;
                return FALSE; // Realign.
            }
        }
        break;
    }

    return TRUE;
}

void SREC_Handler::SignalFailure()
{
    DebuggerPort_Write( HalSystemConfig.DebuggerPorts[0], " <MB>ERROR</MB><MB>ERROR</MB> ", 30 );
    //DebuggerPort_Flush( HalSystemConfig.DebuggerPorts[0] );
    Events_WaitForEvents(0, 1);
}

static char s_buf[20] = "<MB>        </MB>";

void SREC_Handler::SignalSuccess()
{
    //m_LineBuffer[12] = 0;


    memcpy(&s_buf[4], &m_LineBuffer[4], 8);
    
    DebuggerPort_Write( HalSystemConfig.DebuggerPorts[0], s_buf, 17 );
    Events_WaitForEvents(0, 1);
}

BOOL SREC_Handler::ParseLine( const char* SRECLine, BOOL readModWrite )
{
    UINT32               Address;
    int                  i;
    UINT32               BytesRemaining = 0;
    UINT32               Temp           = 0;
    UINT32               Data32;
    UINT32               RecordType;
    UINT32               CheckSum;
    UINT32               ByteCount = 0;
    UINT8                ByteData[256];                           // we support 16 bytes of data per record max, 4 words, 8 shorts
    UINT32               m_size;

    // last record before jump record should be "<CRC>IMAGE_CRC,IMAGE_LENGTH</CRC>\n"
    if('<' == SRECLine[0] && 'C' == SRECLine[1] && 'R' == SRECLine[2] && 'C' == SRECLine[3] && '>' == SRECLine[4])
    {
        if((NULL == htoi(&SRECLine[ 5], 8, m_ImageStart  )) ||
           (NULL == htoi(&SRECLine[14], 8, m_ImageLength )) ||
           (NULL == htoi(&SRECLine[23], 8, m_ImageCRC    )) ||
           (NULL == htoi(&SRECLine[32], 8, m_StartAddress)) )
        { 
           m_ImageStart   = 0; 
           m_ImageLength  = 0;
           m_ImageCRC     = 0;
           m_StartAddress = 0xFFFFFFFF;

           return FALSE;
        }

        return TRUE;
    }

    if('S' != *SRECLine && 's' != *SRECLine) return FALSE;

    SRECLine++;            
        
    RecordType = *SRECLine++;

    SRECLine = htoi( SRECLine, 2, BytesRemaining ); if(!SRECLine) return FALSE;

    // start the checksum with this byte
    CheckSum = BytesRemaining;

    // get the destination address
    // do bytes only to make checksum calc easier
    Data32 = 0;
    for(i = 0; i < 4; i++)
    {
        SRECLine = htoi( SRECLine, 2, Temp ); if(!SRECLine) return FALSE;

        CheckSum += Temp;

        Data32 <<= 8;
        Data32  |= Temp;

        BytesRemaining -= 1;
    }
    Address = Data32;

    // take off one byte for CRC;
    m_size = BytesRemaining -1;

    switch(RecordType)
    {
    case '3':
        {

            while(BytesRemaining > 1)
            {
                // get bytes into words, and checksum
                Data32 = 0;
                for(i = 0; i < sizeof(FLASH_WORD); i++)
                {
                    SRECLine = htoi( SRECLine, 2, Temp ); if(!SRECLine) return FALSE;

                    CheckSum += Temp;

                    Data32 |= Temp << (i*8);    // little endian format

                    ByteData[ByteCount++] = Temp;

                    BytesRemaining -= 1;

                    // leave the checksum in place
                    if(1 == BytesRemaining) break;
                }

                ASSERT(ByteCount < sizeof(ByteData));

                //WordData[WordCount++] = Data32;
            }
        }
        break;

    case '7':
        // just a return address (starting location)
        m_StartAddress = (UINT32)Address;
        break;

    default:
        // we only support S3 and S7 records, for now
        return FALSE;
    }

    // get the checksum
    SRECLine = htoi( SRECLine, 2, Temp ); if(!SRECLine) return FALSE;

    CheckSum += Temp;

    BytesRemaining -= 1;

    ASSERT(0 == BytesRemaining);

    if(0xff != (CheckSum & 0xff))
    {
        return FALSE;
    }
    else
    {
        // only write if we succeeded the checksum entirely for whole line
        if(m_size > 0)
        {
            ByteAddress sectAddress;
            BlockStorageDevice *device;
            
            if(BlockStorageList::FindDeviceForPhysicalAddress( &device, Address, sectAddress )) 
            {
                m_isRamBuild = FALSE;
                while(m_Stream.CurrentAddress() != Address)
                {
                    if(!m_Stream.Seek(Address - m_Stream.CurrentAddress(), BlockStorageStream::SeekCurrent))
                    {
                        if(!m_Stream.NextStream())
                        {
                            if(!MemStreamSeekBlockAddress(m_Stream, Address)) return FALSE;
                        }
                    }
                }

                if(m_Stream.Usage == BlockUsage::BOOTSTRAP)
                {
                    return FALSE;
                }

                if(!readModWrite)
                {
                    if(0 == (m_Stream.CurrentIndex % m_Stream.BlockLength))
                    {
                        m_Stream.Erase( m_Stream.BlockLength );
                        
                        if(m_BootMarkerAddress == 0 &&  Address != 0 && *(UINT32*)ByteData == MicroBooter_ProgramMarker())
                        {
                            m_BootMarkerAddress = Address;

                            // make sure we don't put the bootmarker until we verify the full image
                            ByteData[0] = 0xFF;
                            ByteData[1] = 0xFF;
                            ByteData[2] = 0xFF;
                            ByteData[3] = 0xFF;
                        }
                    }
                }
                
                m_Stream.Write( ByteData, ByteCount );
            }
            else
            {
                m_isRamBuild = TRUE;

                memcpy((void*)Address, ByteData, ByteCount);
            }
        }
    }

    return TRUE;
}

//--//

const char* SREC_Handler::htoi( const char* hexstring, UINT32 length, UINT32& value )
{
    // internal limitation of result size right now, big enough for debug software
    ASSERT(length <= 8);

    UINT32 result = 0;

    while(length--)
    {
        char c = *hexstring++;

        if     (c >= '0' && c <= '9') c -= '0';
        else if(c >= 'a' && c <= 'f') c -= 'a' - 10;
        else if(c >= 'A' && c <= 'F') c -= 'A' - 10;
        else                          return NULL;

        result = (result << 4) + c;
    }

    value = result;

    return hexstring;
}


