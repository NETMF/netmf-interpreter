// CRT I/O redirection support
#include <stdio.h>
#include <time.h>
#include <rt_sys.h>
#include "stm32f4xx.h"

// disable semi hosting as NETMF doesn't require it and it adds to code size
#pragma import(__use_no_semihosting_swi)
#pragma import(__use_no_semihosting)
#pragma import(__use_no_heap_region)
#pragma import(__use_no_heap)

// dummy FILE struct
struct __FILE { int handle; /* Add whatever you need here */ };
FILE *__aeabi_stdin, *__aeabi_stdout, *__aeabi_stderr;

const char __stdin_name[] = { 0 };
const char __stdout_name[] = { 0 };
const char __stderr_name[] = { 0 };

// standard file handles
// __stdout redirects to ITM channel 0 for deeply integrated Debugging printf/tracing
FILE __stdin, __stdout, __stderr;

int fputc(int ch, FILE *f)
{
  if( f == &__stdout )
      ITM_SendChar( ch );
  
  return ch;
}

//FILEHANDLE _sys_open(const char *name, int openmode)
//{
//    return 0; 
//}

//int _sys_close(FILEHANDLE fh)
//{
//    return 0;
//}

//int _sys_write(FILEHANDLE fh, const unsigned char *buf,
//               unsigned len, int mode)
//{
//    //your_device_write(buf, len);
//    return 0;
//}

//int _sys_read( FILEHANDLE fh
//             , unsigned char *buf
//             , unsigned len
//             , int mode
//             )
//{
//    return -1; /* not supported */
//}

//void _ttywrch(int ch)
//{
//    ITM_SendChar(ch);
//}

//int _sys_istty(FILEHANDLE fh)
//{
//    return 0; /* buffered output */
//}

//int _sys_seek(FILEHANDLE fh, long pos)
//{
//    return -1; /* not supported */
//}

//long _sys_flen(FILEHANDLE fh)
//{
//    return -1; /* not supported */
//}

//void _sys_exit(int returncode)
//{
//}

//int __backspace(FILE *stream)
//{
//    return 0;
//}
