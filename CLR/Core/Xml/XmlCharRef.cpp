////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

CLR_INT32 CLR_RT_XmlCharRef::ParseCharRef( LPSTR buffer, CLR_UINT32 count, CLR_UINT32 index, CLR_RT_XmlShiftHelper* shiftHelper )
{
    _ASSERTE(buffer[ 0 ] == '&');

    // need at least 4 char (i.e., &gt; to begin)
    if(count < 4) return 0;

    CLR_INT32 retCount = 0;

    CLR_INT32 charSize = 1;

    switch(buffer[ 1 ])
    {
    case 'a':
        if(buffer[ 2 ] == 'm')
        {
            if(count < 5) return 0;

            if(buffer[ 3 ] == 'p' && buffer[ 4 ] == ';')
            {
                buffer[ 0 ] = '&';
                buffer[ 3 ] = 4;
                retCount    = 5;
            }
            else
            {
                return -1;
            }
        }
        else if(buffer[ 2 ] == 'p')
        {
            if(count < 6) return 0;

            if(buffer[ 3 ] == 'o' && buffer[ 4 ] == 's' && buffer[ 5 ] == ';')
            {
                buffer[ 0 ] = '\'';
                buffer[ 3 ] = 5;
                retCount    = 6;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            return -1;
        }
        break;
        
    case 'g':
    case 'l':
        if(buffer[ 2 ] == 't' && buffer[ 3 ] == ';')
        {
            buffer[ 0 ] = (buffer[ 1 ] == 'g') ? '>' : '<';
            buffer[ 3 ] = 3;
            retCount    = 4;
        }
        else
        {
            return -1;
        }
        break;
        
    case 'q':
        if(count < 6) return 0;
        
        if(buffer[ 2 ] == 'u' && buffer[ 3 ] == 'o' && buffer[ 4 ] == 't' && buffer[ 5 ] == ';')
        {
            buffer[ 0 ] = '"';
            buffer[ 3 ] = 5;
            retCount    = 6;
        }
        else
        {
            return -1;
        }
        break;
        
    case '#':
        {
            CLR_UINT32 ch = 0;
            CLR_UINT8  digit;

            if(buffer[ 2 ] == 'x')
            {
                retCount = 3;
                
                while(true)
                {
                    digit = buffer[ retCount++ ];

                    if(digit == ';') break;

                    ch *= 16;

                    if(digit >= '0' && digit <= '9')
                    {
                        ch += (digit - '0');
                    }
                    else if(digit >= 'a' && digit <= 'f')
                    {
                        ch += (digit - 'a') + 10;
                    }
                    else if(digit >= 'A' && digit <= 'F')
                    {
                        ch += (digit - 'A') + 10;
                    }
                    else
                    {
                        return -1;
                    }

                    if(retCount == count)
                    {
                        return 0;
                    }
                    
                    if(retCount > 8) // the max would be &#xffff;
                    {
                        return -1;
                    }
                }
            }
            else
            {
                retCount = 2;
                
                while(true)
                {
                    digit = buffer[ retCount++ ];

                    if(digit == ';') break;

                    ch *= 10;

                    if(digit >= '0' && digit <= '9')
                    {
                        ch += (digit - '0');
                    }
                    else
                    {
                        return -1;
                    }

                    if(retCount == count)
                    {
                        return 0;
                    }
                    
                    if(retCount > 7) // the max would be &#65535;
                    {
                        return -1;
                    }
                }
            }
            
            if(ch < 0x80)
            {
                if(ch >= 0x20 || (c_XmlCharType[ ch ] & XmlCharType_Space))
                {
                    buffer[ 0 ] = ch;
                    buffer[ 3 ] = retCount - 1;
                }
                else
                {
                    return -1;
                }
            }
            else if(ch < 0x800)
            {
                buffer[ 4 ] = retCount - 2;
                
                buffer[ 1 ] = 0x80 | (ch & 0x3F); ch >>= 6;
                buffer[ 0 ] = 0xC0 |  ch        ;

                charSize = 2;
            }
            else if(ch < 0x10000)
            {
                buffer[ 5 ] = retCount - 3;
                
                buffer[ 2 ] = 0x80 | (ch & 0x3F); ch >>= 6;
                buffer[ 1 ] = 0x80 | (ch & 0x3F); ch >>= 6;
                buffer[ 0 ] = 0xE0 |  ch        ;

                charSize = 3;
            }
            else
            {
                return -1;
            }
        }
        break;
    }

    shiftHelper->SetNextCharRef( index + charSize, (CLR_UINT8*)&buffer[ charSize ] );

    return retCount;
}

