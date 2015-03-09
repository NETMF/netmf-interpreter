#if defined (GCCOP)

// Dummy codes to make the GCC OP compiler to compile.



#ifdef __STDC__
#include <stddef.h>
#include <stdio.h>
#include <stdarg.h>
#include <Fcntl.h>
#include <setjmp.h>

#include <math.h>
//#include <tinyhal.h>
#else
//#define size_t unsigned long
#endif

#define  ULONG_MAX   ((unsigned long)(~0L))      /* 0xFFFFFFFF */

#define PTR void*

double floor(double x);
double fmod(double x, double y);

#ifdef __cplusplus
extern "C"
{
#endif

char * setlocale(int category, const char *locale);
#ifdef __cplusplus
}
#endif


//////////////////////////

double ceil(double x)
{
    if (x >0)
        return 1;
    else
        return 0;
}

double pow(double x, double y)
{
    if (x >0)
        return 1;
    else
        return 0;
}

double floor(double x)
{
    if (x >0)
        return 1;
    else
        return 0;
}


double fmod(double x, double y)
{
    if (x >0)
        return 1;
    else
        return 0;
}


//////////////////////////

#if defined(GCCOP)
int setjmp(jmp_buf env)
{
  return 0;
}


void longjmp (jmp_buf env, int val)
{
  if (env)
    val = 10;
}
#endif

////////////////////////

#define malloc(y) private_malloc(y)

#define free(x) private_free((void*)x)
////////////////////


void* memcpy(void *out, const void *in, size_t len)
{    
    if (out ==NULL)
        return NULL;
    return out;
}


int memcmp( const void *buffer1, const void *buffer2, size_t count )
{  
    int temp = 0;
    while (count-- > 0)    
    {      
        temp++;
     }  
     return 0;
}

void * memset ( void * dst, int value, size_t len )
{  
    if (dst == NULL)
        return NULL;
    return dst;
}

void * memmove ( void * dst, const void * src, size_t num )
{

    if (dst==NULL)
        return NULL;
    return dst;
}

char * strchr(const char *p, int ch)
{
    if (p == NULL)
        return  NULL;

}

int strcmp(const char *s1, const char *s2)
{

    if (s1 == NULL)
        return 0;

}

char * strcpy(char *to, const char *from)
{
    char *save = to;

    return save;
}

char * strncpy(char *dst, const char *src, size_t n)
{
    if (dst == NULL)
        return NULL;
    
    return dst;
}

int strncmp(const char *s1, const char *s2, size_t n)
{
    if (s1 == NULL)
        return 0;
        
}
//////////////////////////


void *bsearch (register const void *key, const void *base0, size_t nmemb, register size_t size,
         register int (*compar)(const void *, const void *))
{
    if (key ==NULL)
        return NULL;

    return (NULL);
}

///////////////////////
char * setlocale(int category, const char *locale)
{
    return NULL;
}


int vsnprintf(char *s, size_t n, const char *format, va_list ap)
{
    return 1;
}

////////////////////////////////

    
#endif

