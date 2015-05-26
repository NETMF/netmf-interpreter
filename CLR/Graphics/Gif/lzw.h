////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#ifndef _LZW_H_

#define _LZW_H_

#include <stdlib.h>
#include <string.h>
#include "lzwutil.h"

#define __int32 int
#define __int16 short
#define __int8  char

/*****************************************************************************
    A class which holds the information which is shared between LZW (GIF)
    compression and decompression.
******************************************************************* JohnBo **/
struct LZW
    {
protected:
    inline void LZWInit(unsigned int bcodeSize)
    {
        m_bcodeSize = LZW_B8(bcodeSize);
        
        LZW_ASSERT(bcodeSize >= 2 && bcodeSize <= 8);
    }
    
    /* METHODS */
    /* Standard GIF definitions. */
    inline unsigned __int16 WClear(void) const
        {
        return LZW_B16(1U<<m_bcodeSize);
        }
    inline unsigned __int16 WEOD(void) const
        {
        return LZW_B16(1U+(1U<<m_bcodeSize));
        }

    /* DATA TYPES */
    /* Basic types. */
    typedef unsigned __int32 TokenValue;
    typedef unsigned __int16 TokenIndex;

    /* Constants. */
    enum
        {
        ctokenBits = 12,
        ctokens = (1U<<ctokenBits),
        };

    /* DATA */
    unsigned char  m_bcodeSize;    // The LZW initial code size
    };


/*****************************************************************************
    A class which also holds the current token size.
******************************************************************* JohnBo **/
struct LZWState : protected LZW
    {
protected:
    inline void LZWStateInit(unsigned int bcodeSize)
    {
        m_ibitsToken = LZW_B8(bcodeSize+1);
        m_itokenLast = WEOD();

        LZWInit(bcodeSize);
    }

    unsigned char  m_ibitsToken; // The (current) number of bits in a token
    TokenIndex       m_itokenLast; // The last token to be allocated
    };

#endif
