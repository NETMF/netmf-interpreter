#include "e_os.h"
#ifdef OPENSSL_SYS_WINDOWS
#include <string.h>
#else
#include <tinyhal.h>
#endif

OPENSSL_TYPE__FILE fake_cert;
OPENSSL_TYPE__FILE SSL_STDERR;
OPENSSL_TYPE__FILE SSL_STDOUT;
OPENSSL_TYPE__FILE SSL_STDIN;


OPENSSL_TYPE__FILE* tinyclr_fopen(const char * filename, const char * mode) 
{
	
    fake_cert.buffer = filename;
    fake_cert.read = 0;
	return &fake_cert;
}
int tinyclr_fclose ( OPENSSL_TYPE__FILE * stream )
{
	/*
	Closes the file associated with the stream and disassociates it.
	All internal buffers associated with the stream are flushed: the content 
	of any unwritten buffer is written and the content of any unread buffer is discarded.
	Even if the call fails, the stream passed as parameter will no longer be associated with the file.
	*/
	return 0; //for success matches std library
}

size_t tinyclr_fread ( void * ptr, size_t size, size_t count, OPENSSL_TYPE__FILE * stream )
{
	/*
	
	Reads an array of count elements, each one with a size of size bytes, from the stream 
	and stores them in the block of memory specified by ptr.
	The postion indicator of the stream is advanced by the total amount of bytes read.
	The total amount of bytes read if successful is (size * count).
	
	*/
	return 0; //should return number of bytes read per std library
}

int tinyclr_feof ( OPENSSL_TYPE__FILE * stream )
{
	return 0;
}
int tinyclr_ferror ( OPENSSL_TYPE__FILE * stream )
{
	return 0;
}
int tinyclr_fseek ( OPENSSL_TYPE__FILE * stream, long int offset, int origin )
{
	return 0;
}
long int tinyclr_ftell ( OPENSSL_TYPE__FILE * stream )
{
	return 0;
}
int tinyclr_fflush ( OPENSSL_TYPE__FILE * stream )
{
	return 0;
}

char* tinyclr_fgets(char * str, int num, OPENSSL_TYPE__FILE * stream) 
{
    int count = num;
	int cursor = 0;
    char* ptr = (char*)&stream->buffer[stream->read];
    while(*ptr != '\n' && *ptr != '\0' && count > 1 ) 
    {
        count--; ptr++; cursor++;
    };

    TINYCLR_SSL_MEMCPY(str,	&stream->buffer[stream->read], num-count+1);
	str[num-count+1] = '\0';
    stream->read += cursor+1;
	TINYCLR_SSL_PRINTF(str);
	return str;
}

long tinyclr_ssl_lseek(int a, int b, int c)
{
	return 0L;
};

int tinyclr_fputs ( const char * str, OPENSSL_TYPE__FILE * stream )
{
	int size = 0;
    char* ptr = (char*)&stream->buffer[stream->read];
    while(*ptr != '\0' ) 
    {
        ptr++; size++;
    };
	
	TINYCLR_SSL_MEMCPY((void*)&stream->buffer[stream->read], str, size);

	stream->read += size+1;
	
	return 1; // non-negative is success
}

int tinyclr_fputc ( int character, OPENSSL_TYPE__FILE * stream )
{

	TINYCLR_SSL_MEMCPY((void*)&stream->buffer[stream->read], (void*)&character, 1);
	stream->read++;
	
	return character;
}

int tinyclr_fgetc ( OPENSSL_TYPE__FILE * stream )
{
	char ch = stream->buffer[stream->read]; 
	if (ch != (char)EOF) stream->read++;
	return ch;
}

int tinyclr_sprintf ( char * str, const char * format, ... )
{
	return 0; //total number of characters written
}

int tinyclr_fileno( OPENSSL_TYPE__FILE *stream )
{
	return (int)stream->buffer;
}

		
