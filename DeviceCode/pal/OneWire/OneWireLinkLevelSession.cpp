//---------------------------------------------------------------------------
// Copyright (C) 1999 Dallas Semiconductor Corporation, All Rights Reserved.
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
//  DS520SES.C - Acquire and release a Session for general 1-Wire Net
//              library
//
//  Version: 2.00
//           1.03 -> 2.00  Changed 'MLan' to 'ow'. Added support for
//                         multiple ports.
//

#include "ownet.h"

// defined in link file
//extern void usDelay(int);

// local function prototypes
//SMALLINT owAcquire(int,int);
//void     owRelease(int);

//---------------------------------------------------------------------------
// Attempt to acquire a 1-Wire net
//
// 'portnum'    - number 0 to MAX_PORTNUM-1.  This number is provided to
//                indicate the symbolic port number.
// 'port_zstr'  - zero terminated port name.
//
// Returns: TRUE - success, port opened
//
SMALLINT owAcquire(int portnum, int pin)
{
  if((portnum >= MAX_PORTNUM) || (portnum < 0) || (owPortPin[portnum] != 0)) return FALSE; // already in use

  //portnum = 0;
  //UINT32 pin = (UINT32)atoi(port_zstr); 
   
  // drive bus high.
  CPU_GPIO_EnableOutputPin( pin, true );     
  
  // give time for line to settle
  usDelay(500);                              

   // checks to make sure the line is idling high.
  CPU_GPIO_EnableInputPin( pin, false, NULL, GPIO_INT_EDGE_HIGH, RESISTOR_PULLUP );
  
  if (CPU_GPIO_GetPinState(pin)==1)
  {
	owPortPin[portnum] = pin;
	return TRUE;
  }
  else
  {
	return FALSE;
  }
}
//---------------------------------------------------------------------------
// Release the previously acquired a 1-Wire net.
//
// 'portnum'    - number 0 to MAX_PORTNUM-1.  This number is provided to
//                indicate the symbolic port number.
//
void owRelease(int portnum)
{
   if((portnum < MAX_PORTNUM) && (portnum >= 0) ) 
   {
		UINT32 pin = (UINT32)owPortPin[portnum]; 
				
		CPU_GPIO_EnableInputPin( pin, false, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );
		
		owPortPin[portnum] = 0;
   }
}


