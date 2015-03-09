//---------------------------------------------------------------------------
// Copyright (C) 2000 Dallas Semiconductor Corporation, All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY,  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL DALLAS SEMICONDUCTOR BE LIABLE FOR ANY CLAIM, DAMAGES
// OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// Except as contained in this notice, the name of Dallas Semiconductor
// shall not be used except as stated in the Dallas Semiconductor
// Branding Policy.
//---------------------------------------------------------------------------
//
//  ownet.C - Network functions for 1-Wire net devices.
//
//  Version: 3.00
//
//  History: 1.00 -> 1.01  Change to owFamilySearchSetup, LastDiscrepancy[portnum]
//                         was set to 64 instead of 8 to enable devices with
//                         early contention to go in the '0' direction first.
//           1.02 -> 1.03  Initialized goodbits in  owVerify
//           1.03 -> 2.00  Changed 'MLan' to 'ow'. Added support for
//                         multiple ports.
//           2.00 -> 2.01  Added support for owError library
//           2.01 -> 3.00  Make search functions consistent with AN187
//

#include <stdio.h>
#include "ownet.h"

//--------------------------------------------------------------------------
// The 'owFirst' finds the first device on the 1-Wire Net  This function
// contains one parameter 'alarm_only'.  When
// 'alarm_only' is TRUE (1) the find alarm command 0xEC is
// sent instead of the normal search command 0xF0.
// Using the find alarm command 0xEC will limit the search to only
// 1-Wire devices that are in an 'alarm' state.
//
// 'portnum'    - number 0 to MAX_PORTNUM-1.  This number is provided to
//                indicate the symbolic port number.
// 'do_reset'   - TRUE (1) perform reset before search, FALSE (0) do not
//                perform reset before search.
// 'alarm_only' - TRUE (1) the find alarm command 0xEC is
//                sent instead of the normal search command 0xF0
//
// Returns:   TRUE (1) : when a 1-Wire device was found and it's
//                        Serial Number placed in the global SerialNum[portnum]
//            FALSE (0): There are no devices on the 1-Wire Net.
//
SMALLINT owFirst(int portnum, SMALLINT do_reset, SMALLINT alarm_only)
{

  return FALSE;
}

//--------------------------------------------------------------------------
// The 'owNext' function does a general search.  This function
// continues from the previos search state. The search state
// can be reset by using the 'owFirst' function.
// This function contains one parameter 'alarm_only'.
// When 'alarm_only' is TRUE (1) the find alarm command
// 0xEC is sent instead of the normal search command 0xF0.
// Using the find alarm command 0xEC will limit the search to only
// 1-Wire devices that are in an 'alarm' state.
//
// 'portnum'    - number 0 to MAX_PORTNUM-1.  This number is provided to
//                indicate the symbolic port number.
// 'do_reset'   - TRUE (1) perform reset before search, FALSE (0) do not
//                perform reset before search.
// 'alarm_only' - TRUE (1) the find alarm command 0xEC is
//                sent instead of the normal search command 0xF0
//
// Returns:   TRUE (1) : when a 1-Wire device was found and it's
//                       Serial Number placed in the global SerialNum[portnum]
//            FALSE (0): when no new device was found.  Either the
//                       last search was the last device or there
//                       are no devices on the 1-Wire Net.
//
SMALLINT owNext(int portnum, SMALLINT do_reset, SMALLINT alarm_only)
{ 
  return FALSE;
}

//--------------------------------------------------------------------------
// The 'owSerialNum' function either reads or sets the SerialNum buffer
// that is used in the search functions 'owFirst' and 'owNext'.
// This function contains two parameters, 'serialnum_buf' is a pointer
// to a buffer provided by the caller.  'serialnum_buf' should point to
// an array of 8 unsigned chars.  The second parameter is a flag called
// 'do_read' that is TRUE (1) if the operation is to read and FALSE
// (0) if the operation is to set the internal SerialNum buffer from
// the data in the provided buffer.
//
// 'portnum'       - number 0 to MAX_PORTNUM-1.  This number is provided to
//                   indicate the symbolic port number.
// 'serialnum_buf' - buffer to that contains the serial number to set
//                   when do_read = FALSE (0) and buffer to get the serial
//                   number when do_read = TRUE (1).
// 'do_read'       - flag to indicate reading (1) or setting (0) the current
//                   serial number.
//
void owSerialNum(int portnum, uchar *serialnum_buf, SMALLINT do_read)
{
}
