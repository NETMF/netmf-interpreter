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
// owerr.c - Library functions for error handling with 1-Wire library
//
// Version: 1.00
//

#include <string.h>
#ifndef _WIN32_WCE
#include <stdio.h>
#endif
#ifdef _WIN64
#include <stdio.h>
#endif
#include "ownet.h"

#ifndef SIZE_OWERROR_STACK
   #ifdef SMALL_MEMORY_TARGET
      //for small memory, only hold 1 error
      #define SIZE_OWERROR_STACK 1
   #else
      #define SIZE_OWERROR_STACK 10
   #endif
#endif

//---------------------------------------------------------------------------
// Variables
//---------------------------------------------------------------------------

// Error Struct for holding error information.
// In DEBUG, this will also hold the line number and filename.
typedef struct
{
   int owErrorNum;
#ifdef DEBUG
   int lineno;
   char *filename;
#endif
} owErrorStruct;

// Ring-buffer used for stack.
// In case of overflow, deepest error is over-written.
static owErrorStruct owErrorStack[SIZE_OWERROR_STACK];

// Stack pointer to top-most error.
static int owErrorPointer = 0;


//---------------------------------------------------------------------------
// Functions Definitions
//---------------------------------------------------------------------------
int owGetErrorNum(void);
void owClearError(void);
int owHasErrors(void);
#ifdef DEBUG
   void owRaiseError(int,int,char*);
#else
   void owRaiseError(int);
#endif
#ifndef SMALL_MEMORY_TARGET
   void owPrintErrorMsg(FILE *);
   void owPrintErrorMsgStd();
   char *owGetErrorMsg(int);
#endif


//--------------------------------------------------------------------------
// The 'owGetErroNum' returns the error code of the top-most error on the
// error stack.  NOTE: This function has the side effect of popping the
// current error off the stack.  All successive calls to 'owGetErrorNum'
// will further clear the error stack.
//
// For list of error codes, see 'ownet.h'
//
// Returns:   int :  The error code of the top-most error on the stack
//
int owGetErrorNum(void)
{
   int i = owErrorStack[ owErrorPointer ].owErrorNum;
   owErrorStack[ owErrorPointer ].owErrorNum = 0;
   if(!owErrorPointer)
      owErrorPointer = SIZE_OWERROR_STACK - 1;
   else
      owErrorPointer = (owErrorPointer - 1);
   return i;
}

//--------------------------------------------------------------------------
// The 'owClearError' clears all the errors.
//
void owClearError(void)
{
   owErrorStack[ owErrorPointer ].owErrorNum = 0;
}

//--------------------------------------------------------------------------
// The 'owHasErrors' is a boolean test function which tests whether or not
// a valid error is waiting on the stack.
//
// Returns:   TRUE (1) : When there are errors on the stack.
//            FALSE (0): When stack's errors are set to 0, or NO_ERROR_SET.
//
int owHasErrors(void)
{
   if(owErrorStack[ owErrorPointer ].owErrorNum)
      return 1; //TRUE
   else
      return 0; //FALSE
}

#ifdef DEBUG
   //--------------------------------------------------------------------------
   // The 'owRaiseError' is the method for raising an error onto the error
   // stack.
   //
   // Arguments:  int err - the error code you wish to raise.
   //             int lineno - DEBUG only - the line number where it was raised
   //             char* filename - DEBUG only - the file name where it occured.
   //
   void owRaiseError(int err, int lineno, char* filename)
   {
      owErrorPointer = (owErrorPointer + 1) % SIZE_OWERROR_STACK;
      owErrorStack[ owErrorPointer ].owErrorNum = err;
      owErrorStack[ owErrorPointer ].lineno = lineno;
      owErrorStack[ owErrorPointer ].filename = filename;
   }
#else
   //--------------------------------------------------------------------------
   // The 'owRaiseError' is the method for raising an error onto the error
   // stack.
   //
   // Arguments:  int err - the error code you wish to raise.
   //
   void owRaiseError(int err)
   {
      owErrorPointer = (owErrorPointer + 1) % SIZE_OWERROR_STACK;
      owErrorStack[ owErrorPointer ].owErrorNum = err;
   }
#endif


// SMALL_MEMORY_TARGET - embedded microcontrollers, where these
// messaging functions might not make any sense.
#ifndef SMALL_MEMORY_TARGET

// To stop GCC complaining about deprciated automatic typecast
#define _TYPE_CAST_STRING_ (char *)


   //Array of meaningful error messages to associate with codes.
   //Not used on targets with low memory (i.e. PIC).
   static char *owErrorMsg[125] =
   {
   /*000*/ _TYPE_CAST_STRING_"No Error Was Set",
   /*001*/ _TYPE_CAST_STRING_"No Devices found on 1-Wire Network",
   /*002*/ _TYPE_CAST_STRING_"1-Wire Net Reset Failed",
   /*003*/ _TYPE_CAST_STRING_"Search ROM Error: Couldn't locate next device on 1-Wire",
   /*004*/ _TYPE_CAST_STRING_"Access Failed: Could not select device",
   /*005*/ _TYPE_CAST_STRING_"DS2480B Adapter Not Detected",
   /*006*/ _TYPE_CAST_STRING_"DS2480B: Wrong Baud",
   /*007*/ _TYPE_CAST_STRING_"DS2480B: Bad Response",
   /*008*/ _TYPE_CAST_STRING_"Open COM Failed",
   /*009*/ _TYPE_CAST_STRING_"Write COM Failed",
   /*010*/ _TYPE_CAST_STRING_"Read COM Failed",
   /*011*/ _TYPE_CAST_STRING_"Data Block Too Large",
   /*012*/ _TYPE_CAST_STRING_"Block Transfer failed",
   /*013*/ _TYPE_CAST_STRING_"Program Pulse Failed",
   /*014*/ _TYPE_CAST_STRING_"Program Byte Failed",
   /*015*/ _TYPE_CAST_STRING_"Write Byte Failed",
   /*016*/ _TYPE_CAST_STRING_"Read Byte Failed",
   /*017*/ _TYPE_CAST_STRING_"Write Verify Failed",
   /*018*/ _TYPE_CAST_STRING_"Read Verify Failed",
   /*019*/ _TYPE_CAST_STRING_"Write Scratchpad Failed",
   /*020*/ _TYPE_CAST_STRING_"Copy Scratchpad Failed",
   /*021*/ _TYPE_CAST_STRING_"Incorrect CRC Length",
   /*022*/ _TYPE_CAST_STRING_"CRC Failed",
   /*023*/ _TYPE_CAST_STRING_"Failed to acquire a necessary system resource",
   /*024*/ _TYPE_CAST_STRING_"Failed to initialize system resource",
   /*025*/ _TYPE_CAST_STRING_"Data too long to fit on specified device.",
   /*026*/ _TYPE_CAST_STRING_"Read exceeds memory bank end.",
   /*027*/ _TYPE_CAST_STRING_"Write exceeds memory bank end.",
   /*028*/ _TYPE_CAST_STRING_"Device select failed",
   /*029*/ _TYPE_CAST_STRING_"Read Scratch Pad verify failed.",
   /*030*/ _TYPE_CAST_STRING_"Copy scratchpad complete not found",
   /*031*/ _TYPE_CAST_STRING_"Erase scratchpad complete not found",
   /*032*/ _TYPE_CAST_STRING_"Address read back from scrachpad was incorrect",
   /*033*/ _TYPE_CAST_STRING_"Read page with extra-info not supported by this memory bank",
   /*034*/ _TYPE_CAST_STRING_"Read page packet with extra-info not supported by this memory bank",
   /*035*/ _TYPE_CAST_STRING_"Length of packet requested exceeds page size",
   /*036*/ _TYPE_CAST_STRING_"Invalid length in packet",
   /*037*/ _TYPE_CAST_STRING_"Program pulse required but not available",
   /*038*/ _TYPE_CAST_STRING_"Trying to access a read-only memory bank",
   /*039*/ _TYPE_CAST_STRING_"Current bank is not general purpose memory",
   /*040*/ _TYPE_CAST_STRING_"Read back from write compare is incorrect, page may be locked",
   /*041*/ _TYPE_CAST_STRING_"Invalid page number for this memory bank",
   /*042*/ _TYPE_CAST_STRING_"Read page with CRC not supported by this memory bank",
   /*043*/ _TYPE_CAST_STRING_"Read page with CRC and extra-info not supported by this memory bank",
   /*044*/ _TYPE_CAST_STRING_"Read back from write incorrect, could not lock page",
   /*045*/ _TYPE_CAST_STRING_"Read back from write incorrect, could not lock redirect byte",
   /*046*/ _TYPE_CAST_STRING_"The read of the status was not completed.",
   /*047*/ _TYPE_CAST_STRING_"Page redirection not supported by this memory bank",
   /*048*/ _TYPE_CAST_STRING_"Lock Page redirection not supported by this memory bank",
   /*049*/ _TYPE_CAST_STRING_"Read back byte on EPROM programming did not match.",
   /*050*/ _TYPE_CAST_STRING_"Can not write to a page that is locked.",
   /*051*/ _TYPE_CAST_STRING_"Can not lock a redirected page that has already been locked.",
   /*052*/ _TYPE_CAST_STRING_"Trying to redirect a locked redirected page.",
   /*053*/ _TYPE_CAST_STRING_"Trying to lock a page that is already locked.",
   /*054*/ _TYPE_CAST_STRING_"Trying to write to a memory bank that is write protected.",
   /*055*/ _TYPE_CAST_STRING_"Error due to not matching MAC.",
   /*056*/ _TYPE_CAST_STRING_"Memory Bank is write protected.",
   /*057*/ _TYPE_CAST_STRING_"Secret is write protected, can not Load First Secret.",
   /*058*/ _TYPE_CAST_STRING_"Error in Reading Scratchpad after Computing Next Secret.",
   /*059*/ _TYPE_CAST_STRING_"Load Error from Loading First Secret.",
   /*060*/ _TYPE_CAST_STRING_"Power delivery required but not available",
   /*061*/ _TYPE_CAST_STRING_"Not a valid file name.",
   /*062*/ _TYPE_CAST_STRING_"Unable to Create a Directory in this part.",
   /*063*/ _TYPE_CAST_STRING_"That file already exists.",
   /*064*/ _TYPE_CAST_STRING_"The directory is not empty.",
   /*065*/ _TYPE_CAST_STRING_"The wrong type of part for this operation.",
   /*066*/ _TYPE_CAST_STRING_"The max len for this file is too small.",
   /*067*/ _TYPE_CAST_STRING_"This is not a write once bank.",
   /*068*/ _TYPE_CAST_STRING_"The file can not be found.",
   /*069*/ _TYPE_CAST_STRING_"There is not enough space available.",
   /*070*/ _TYPE_CAST_STRING_"There is not a page to match that bit in the bitmap.",
   /*071*/ _TYPE_CAST_STRING_"There are no jobs for EPROM parts.",
   /*072*/ _TYPE_CAST_STRING_"Function not supported to modify attributes.",
   /*073*/ _TYPE_CAST_STRING_"Handle is not in use.",
   /*074*/ _TYPE_CAST_STRING_"Tring to read a write only file.",
   /*075*/ _TYPE_CAST_STRING_"There is no handle available for use.",
   /*076*/ _TYPE_CAST_STRING_"The directory provided is an invalid directory.",
   /*077*/ _TYPE_CAST_STRING_"Handle does not exist.",
   /*078*/ _TYPE_CAST_STRING_"Serial Number did not match with current job.",
   /*079*/ _TYPE_CAST_STRING_"Can not program EPROM because a non-EPROM part on the network.",
   /*080*/ _TYPE_CAST_STRING_"Write protect redirection byte is set.",
   /*081*/ _TYPE_CAST_STRING_"There is an inappropriate directory length.",
   /*082*/ _TYPE_CAST_STRING_"The file has already been terminated.",
   /*083*/ _TYPE_CAST_STRING_"Failed to read memory page of iButton part.",
   /*084*/ _TYPE_CAST_STRING_"Failed to match scratchpad of iButton part.",
   /*085*/ _TYPE_CAST_STRING_"Failed to erase scratchpad of iButton part.",
   /*086*/ _TYPE_CAST_STRING_"Failed to read scratchpad of iButton part.",
   /*087*/ _TYPE_CAST_STRING_"Failed to execute SHA function on SHA iButton.",
   /*088*/ _TYPE_CAST_STRING_"SHA iButton did not return a status completion byte.",
   /*089*/ _TYPE_CAST_STRING_"Write data page failed.",
   /*090*/ _TYPE_CAST_STRING_"Copy secret into secret memory pages failed.",
   /*091*/ _TYPE_CAST_STRING_"Bind unique secret to iButton failed.",
   /*092*/ _TYPE_CAST_STRING_"Could not install secret into user token.",
   /*093*/ _TYPE_CAST_STRING_"Transaction Incomplete: signature did not match.",
   /*094*/ _TYPE_CAST_STRING_"Transaction Incomplete: could not sign service data.",
   /*095*/ _TYPE_CAST_STRING_"User token did not provide a valid authentication response.",
   /*096*/ _TYPE_CAST_STRING_"Failed to answer a challenge on the user token.",
   /*097*/ _TYPE_CAST_STRING_"Failed to create a challenge on the coprocessor.",
   /*098*/ _TYPE_CAST_STRING_"Transaction Incomplete: service data was not valid.",
   /*099*/ _TYPE_CAST_STRING_"Transaction Incomplete: service data was not updated.",
   /*100*/ _TYPE_CAST_STRING_"Unrecoverable, catastrophic service failure occured.",
   /*101*/ _TYPE_CAST_STRING_"Load First Secret from scratchpad data failed.",
   /*102*/ _TYPE_CAST_STRING_"Failed to match signature of user's service data.",
   /*103*/ _TYPE_CAST_STRING_"Subkey out of range for the DS1991.",
   /*104*/ _TYPE_CAST_STRING_"Block ID out of range for the DS1991",
   /*105*/ _TYPE_CAST_STRING_"Password is enabled",
   /*106*/ _TYPE_CAST_STRING_"Password is invalid",
   /*107*/ _TYPE_CAST_STRING_"This memory bank has no read only password",
   /*108*/ _TYPE_CAST_STRING_"This memory bank has no read/write password",
   /*109*/ _TYPE_CAST_STRING_"1-Wire is shorted",
   /*110*/ _TYPE_CAST_STRING_"Error communicating with 1-Wire adapter",
   /*111*/ _TYPE_CAST_STRING_"CopyScratchpad failed: Ending Offset must go to end of page",
   /*112*/ _TYPE_CAST_STRING_"WriteScratchpad failed: Ending Offset must go to end of page",
   /*113*/ _TYPE_CAST_STRING_"Mission can not be stopped while one is not in progress",
   /*114*/ _TYPE_CAST_STRING_"Error stopping the mission",
   /*115*/ _TYPE_CAST_STRING_"Port number is outside (0,MAX_PORTNUM) interval",
   /*116*/ _TYPE_CAST_STRING_"Level of the 1-Wire was not changed",
   /*117*/ _TYPE_CAST_STRING_"Both the Read Only and Read Write Passwords must be set",
   /*118*/ _TYPE_CAST_STRING_"Failure to change latch state.",
   /*119*/ _TYPE_CAST_STRING_"Could not open usb port through libusb",
   /*120*/ _TYPE_CAST_STRING_"Libusb DS2490 port already opened",
   /*121*/ _TYPE_CAST_STRING_"Failed to set libusb configuration",
   /*122*/ _TYPE_CAST_STRING_"Failed to claim libusb interface",
   /*123*/ _TYPE_CAST_STRING_"Failed to set libusb altinterface",
   /*124*/ _TYPE_CAST_STRING_"No adapter found at this port number"
   };

   char *owGetErrorMsg(int err)
   {
#ifndef SMALL_MEMORY_TARGET
      return owErrorMsg[err];
#else

#define ERROR_STRING_SIZE 100
       static char[ERROR_STRING_SIZE] owErrorMsg;
       hal_snprintf(owErrorMsg, ERROR_STRING_SIZE, "Onewire Error %d", err);
       return owErrorMsg;
#endif
   }

#ifndef __C51__
   //--------------------------------------------------------------------------
   // The 'owPrintErrorMsg' is the method for printing an error from the stack.
   // The destination for the print is specified by the argument, fileno, which
   // can be stderr, stdout, or a log file.  In non-debug mode, the output is
   // of the form:
   // Error num: Error msg
   //
   // In debug-mode, the output is of the form:
   // Error num: filename line#: Error msg
   //
   // NOTE: This function has the side-effect of popping the error off the stack.
   //
   // Arguments:  FILE*: the destination for printing.
   //
   void owPrintErrorMsg(FILE *filenum)
   {
   #ifdef DEBUG
      int l = owErrorStack[ owErrorPointer ].lineno;
      char *f = owErrorStack[ owErrorPointer ].filename;
      int err = owGetErrorNum();
      hal_printf(/*filenum,*/"Error %d: %s line %d: %s\r\n",err,f,l,owErrorMsg[err]);
   #else
      int err = owGetErrorNum();
      hal_printf(/*filenum,*/"Error %d: %s\r\n",err,owErrorMsg[err]);
   #endif
   }
#endif //__C51__

   // Same as above, except uses default printf output
   void owPrintErrorMsgStd()
   {
   #ifdef DEBUG
      int l = owErrorStack[ owErrorPointer ].lineno;
      char *f = owErrorStack[ owErrorPointer ].filename;
      int err = owGetErrorNum();
      hal_printf("Error %d: %s line %d: %s\r\n",err,f,l,owErrorMsg[err]);
   #else
      int err = owGetErrorNum();
      hal_printf("Error %d: %s\r\n",err,owErrorMsg[err]);
   #endif
   }
#endif

