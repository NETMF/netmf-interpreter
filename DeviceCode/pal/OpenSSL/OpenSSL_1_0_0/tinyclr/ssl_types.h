


#ifndef SSL_TYPES_H
#define SSL_TYPES_H
#include <crypto/o_str.h>
struct OPENSSL_TYPE__FILE
{
    const char *buffer;
    int read;
};
#ifndef OPENSSL_SYS_WINDOWS
//#include <errno.h>
extern OPENSSL_TYPE__FILE SSL_STDERR;
extern OPENSSL_TYPE__FILE SSL_STDOUT;
extern OPENSSL_TYPE__FILE SSL_STDIN;
#if defined(__RENESAS__) || defined(__CC_ARM)
extern "C" volatile int errno;
#else
extern "C" int errno;
#endif
#endif

#ifdef OPENSSL_SYS_ARM
#undef _AEABI_PORTABILITY_LEVEL
#undef __DEFAULT_AEABI_PORTABILITY_LEVEL
#endif

#ifdef  __cplusplus
extern "C" {
#endif

#define EXT_COPY_NONE               0
#define EXT_COPY_ADD                1
#define EXT_COPY_ALL                2

#define NETSCAPE_CERT_HDR           "certificate"

#define APP_PASS_LEN                1024

#define SERIAL_RAND_BITS            64
// Ed


#define ENOENT                      2      /* No such file or directory */
#ifdef EAGAIN
#undef EAGAIN
#endif
#define EAGAIN                      11     /* Try again (defined for LWIP) */
#ifdef EINVAL
#undef EINVAL
#endif
#define EINVAL                      22     /* Invalid argument */

typedef int pid_t;          // for rand library
#ifdef OPENSSL_SYS_SH
typedef unsigned long size_t;
#else
typedef unsigned int size_t;
#endif
typedef long int off_t; // for ccgost library

#if !defined(OPENSSL_SYS_WINDOWS) && !defined(OPENSSL_SYS_WIN32)
typedef int  _ssize_t; 
typedef int  ssize_t;
#endif

#if defined(NETMF_TARGET_LITTLE_ENDIAN)
#define SSL_LONG_LITTLE_ENDIAN(x) (x)
#define SSL_ntohl(x) SOCK_htonl(x)
#else
#define SSL_LONG_LITTLE_ENDIAN(x) ( (((x) & 0x000000FFUL) << 24) | (((x) & 0x0000FF00UL) << 8) | (((x) & 0x00FF0000UL) >> 8) | (((x) & 0xFF000000UL) >> 24) )
#define SSL_ntohl(x) ((UINT64)(x))
#endif

#ifndef OPENSSL_SYS_WINDOWS
//From limits.h
#ifdef INT_MAX
#undef INT_MAX
#endif
#define INT_MAX                     2147483647L
#ifdef LONG_MAX
#undef LONG_MAX
#endif
#define LONG_MAX                    2147483647L
#ifdef ULONG_MAX
#undef ULONG_MAX
#endif
#define ULONG_MAX                   4294967295UL

//For sockets
#define AF_INET                     2       /* internetwork: UDP, TCP, etc. */
#define SOCK_STREAM                 1
#define SOCK_DGRAM                  2
#define SOCK_RAW                    3
#define SOCK_RDM                    4
#define SOCK_SEQPACKET              5
#define IPPROTO_TCP                 6
#define SOL_SOCKET                  0xfff     /* options for socket level (from LWIP) */ 
#define SO_ERROR                    0x1007    /* get error status and clear */
#ifndef INADDR_ANY
#define INADDR_ANY                  ((UINT32)0x00000000UL)
#endif
#ifndef INVALID_SOCKET
#define INVALID_SOCKET              -1
#endif /* INVALID_SOCKET */

#ifndef EWOULDBLOCK
#define EWOULDBLOCK                 EAGAIN
#endif

#define SIGINT                      4 // attention request from user from signal.h 

#ifdef BUFSIZ
#undef BUFSIZ
#endif
#define BUFSIZ                      512

typedef int sig_atomic_t;           // from signal.h
void (*signal(int sig, void (*func)(int param)))(int);
typedef long int time_t;            /* date/time in millisecs past 1601 */

typedef struct tm {
    int tm_sec;   /* seconds after the minute, 0 to 60
                     (0 - 60 allows for the occasional leap second) */
    int tm_min;   /* minutes after the hour, 0 to 59 */
    int tm_hour;  /* hours since midnight, 0 to 23 */
    int tm_mday;  /* day of the month, 1 to 31 */
    int tm_mon;   /* months since January, 0 to 11 */
    int tm_year;  /* years since 1900 */
    int tm_wday;  /* days since Sunday, 0 to 6 */
    int tm_yday;  /* days since January 1, 0 to 365 */
    int tm_isdst; /* Daylight Savings Time flag */
}tm;

typedef struct stat {
    unsigned long   st_dev;
    unsigned long   st_ino;
    unsigned short  st_mode;
    unsigned short  st_nlink;
    unsigned short  st_uid;
    unsigned short  st_gid;
    unsigned long   st_rdev;
    unsigned long   st_size;
    unsigned long   st_blksize;
    unsigned long   st_blocks;
    unsigned long   st_atime;
    unsigned long   st_atime_nsec;
    unsigned long   st_mtime;
    unsigned long   st_mtime_nsec;
    unsigned long   st_ctime;
    unsigned long   st_ctime_nsec;
    unsigned long   __unused4;
    unsigned long   __unused5;
}stat; 
typedef struct stat STRUCTSTAT;

// temp fix for switching between rtip & lwip
#ifndef hostent
typedef struct hostent {
  char          *h_name;
  char          **h_aliases;
  int           h_addrtype;
  int           h_length;
  char          **h_addr_list;
} hostent;
typedef struct servent {
        char    *s_name;        /* official service name */
        char    **s_aliases;    /* alias list */
        int     s_port;         /* port number */
        char    *s_proto;       /* protocol to use */
}servent;
//#define s_addr  S_un.S_addr           /* can be used for most tcp & ip code */
//#define s_host  S_un.S_un_b.s_b2        /* host on imp */
//#define s_net   S_un.S_un_b.s_b1        /* network */
//#define s_imp   S_un.S_un_w.s_w2        /* imp */
//#define s_impno S_un.S_un_b.s_b4        /* imp # */
//#define s_lh    S_un.S_un_b.s_b3        /* logical host */

struct hostent *gethostbyname(const char *name);
struct hostent *gethostbyaddr(const char *addr, int length, int type);
struct servent *getservbyname(const char *name, const char *proto);
#endif

// located in cc.h in lwip
// Define basic types used in lwIP
typedef unsigned   char    u8_t;
typedef signed     char    s8_t;
typedef unsigned   short   u16_t;
typedef signed     short   s16_t;
typedef unsigned   long    u32_t;
typedef signed     long    s32_t;
typedef int                pid_t;

//these are located in LWIP\src\core\ipv4\inet.c
//Maybe these should go into a public header?
#ifndef in_range
#define in_range(c, lo, up)  ((u8_t)c >= lo && (u8_t)c <= up)
#endif
#ifndef isprint
#define isprint(c)           in_range(c, 0x20, 0x7f)
#endif
#ifndef isdigit
#define isdigit(c)           in_range(c, '0', '9')
#endif
#ifndef isxdigit
#define isxdigit(c)          (isdigit(c) || in_range(c, 'a', 'f') || in_range(c, 'A', 'F'))
#endif
#ifndef islower
#define islower(c)           in_range(c, 'a', 'z')
#endif
#ifndef isupper
#define isupper(c)           in_range(c, 'A', 'Z')
#endif
#ifndef isspace
#define isspace(c)           (c == ' ' || c == '\f' || c == '\n' || c == '\r' || c == '\t' || c == '\v')
#endif
#ifndef isalnum
#define isalnum(c)           isdigit(c)
#endif

#else
#include <time.h>
#endif // ndef(OPENSSL_SYS_WINDOWS)

int tinyclr_ssl_toupper(int c);
int tinyclr_ssl_tolower(int c);
int tinyclr_strcmp ( const char * str1, const char * str2 );


//MS:TODO find implementation for getservbyname
struct servent*tinyclr_ssl_getservbyname(const char *name, const char *proto);
struct hostent *tinyclr_ssl_gethostbyname(const char *name);
struct hostent *tinyclr_ssl_gethostbyaddr(const char *addr, int length, int type);
//MS: TODO find impleation for lseek
long tinyclr_ssl_lseek(int a,int b,int c);
int tinyclr_ssl_chmod(const char * filename,int pmode);
size_t tinyclr_ssl_fwrite(const void * ptr,size_t size,size_t count,void * stream);
int tinyclr_ssl_stat( const char *path, struct stat *buffer);
int tinyclr_ssl_gettimeofday(void * tp,void * tzp);

pid_t tinyclr_ssl_getpid();

time_t tinyclr_time ( time_t * timer );
struct tm * tinyclr_localtime ( const time_t * timer );
struct tm * tinyclr_gmtime ( const time_t * timer );
time_t tinyclr_mktime ( struct tm * timeptr );;
extern void tinyclr_qsort ( void * base, size_t num, size_t size, int ( * comparator ) ( const void *, const void * ) );
//struct tm * tinyclr_localtime ( const time_t * timer );


extern int hal_fprintf_ssl(OPENSSL_TYPE__FILE* x, const char* format, ... );

//--//
// uncomment this to test on windows what will happen on the device
//#define ARM_EMULATION_MODE
//--//

#if !defined(OPENSSL_SYS_WINDOWS)
#define TINYCLR_SSL_STRCAT              strcat
#define TINYCLR_SSL_STRCPY(a,b)         hal_strcpy_s(a,hal_strlen_s(b)+1,b)
#define TINYCLR_SSL_STRLEN              hal_strlen_s
#define TINYCLR_SSL_STRNCPY(a,b,c)      hal_strncpy_s(a,c+1,b,c)
#define TINYCLR_SSL_STRNCMP             OPENSSL_strncasecmp
#define TINYCLR_SSL_STRCMP              tinyclr_strcmp
#define TINYCLR_SSL_STRNCASECMP         OPENSSL_strncasecmp
#define TINYCLR_SSL_STRCASECMP          OPENSSL_strcasecmp
#define TINYCLR_SSL_FPRINTF             hal_fprintf_ssl
#define TINYCLR_SSL_SNPRINTF            hal_snprintf
#define TINYCLR_SSL_PRINTF              hal_printf
#define TINYCLR_SSL_PERROR              TINYCLR_SSL_PRINTF

#define TINYCLR_SSL_MEMCPY              memcpy
#define TINYCLR_SSL_MEMSET              memset
#define TINYCLR_SSL_MEMCMP              memcmp
#define TINYCLR_SSL_MEMCHR              memchr
#define TINYCLR_SSL_MEMMOVE             memmove

#define TINYCLR_SSL_TIME                tinyclr_time
#define TINYCLR_SSL_LOCALTIME           tinyclr_localtime
#define TINYCLR_SSL_GMTIME              tinyclr_gmtime
#define TINYCLR_SSL_MKTIME              tinyclr_mktime

#define TINYCLR_SSL_TOUPPER             tinyclr_ssl_toupper
#define TINYCLR_SSL_TOLOWER             tinyclr_ssl_tolower
#define TINYCLR_SSL_LSEEK               tinyclr_ssl_lseek
#define TINYCLR_SSL_CHMOD               tinyclr_ssl_chmod
#define TINYCLR_SSL_FWRITE              tinyclr_ssl_fwrite
#define TINYCLR_SSL_STAT                tinyclr_ssl_stat
#define TINYCLR_SSL_GETTIMEOFDAY        tinyclr_ssl_gettimeofday
#define TINYCLR_SSL_GETPID              tinyclr_ssl_getpid
#define OPENSSL_TYPE__FILE_STDERR       &SSL_STDERR //MS: TODOsomething else later?
#define OPENSSL_TYPE__FILE_STDOUT       &SSL_STDERR //MS: TODOsomething else later? 
#define OPENSSL_TYPE__FILE_STDIN        &SSL_STDIN //MS: TODOsomething else later?
#define TINYCLR_SSL_FILE                OPENSSL_TYPE__FILE

#define TINYCLR_SSL_ASSERT(x)           ASSERT(x)

#define TINYCLR_SSL_QSORT               qsort
#define TINYCLR_SSL_EXIT(x)             if (x==0) return else TINYCLR_SSL_ASSERT(x)
#define TINYCLR_SSL_ABORT()             {TINYCLR_SSL_PRINTF("%s:%d",__FILE__,__LINE__);TINYCLR_SSL_ASSERT(1);}
#define TINYCLR_SSL_MALLOC              private_malloc 
#define TINYCLR_SSL_FREE                private_free
#define TINYCLR_SSL_REALLOC             private_realloc
#define TINYCLR_SSL_GETENV(x)           (NULL)
#else
#define TINYCLR_SSL_STRCAT              strcat
#define TINYCLR_SSL_STRCPY              strcpy
#define TINYCLR_SSL_STRLEN              strlen
#define TINYCLR_SSL_STRNCPY             strncpy
#define TINYCLR_SSL_STRNCMP             strncmp
#define TINYCLR_SSL_STRCMP              strcmp
#define TINYCLR_SSL_STRNCASECMP         strncasecmp
#define TINYCLR_SSL_STRCASECMP          strcasecmp
#define TINYCLR_SSL_FPRINTF             fprintf
#define TINYCLR_SSL_SNPRINTF            _snprintf
#define TINYCLR_SSL_PRINTF              printf
#define TINYCLR_SSL_STAT                stat 
#define TINYCLR_SSL_PERROR              perror

#define TINYCLR_SSL_MEMCPY              memcpy
#define TINYCLR_SSL_MEMSET              memset
#define TINYCLR_SSL_MEMCMP              memcmp
#define TINYCLR_SSL_MEMCHR              memchr
#define TINYCLR_SSL_MEMMOVE             memmove

#define TINYCLR_SSL_TIME                time
#define TINYCLR_SSL_LOCALTIME           localtime
#define TINYCLR_SSL_GMTIME              gmtime
#define TINYCLR_SSL_MKTIME              mktime

#define OPENSSL_TYPE__FILE_STDERR       stderr
#define OPENSSL_TYPE__FILE_STDOUT       stdout
#define OPENSSL_TYPE__FILE_STDIN        stdin

#define TINYCLR_SSL_TOUPPER             toupper
#define TINYCLR_SSL_TOLOWER             tolower
#define TINYCLR_SSL_LSEEK               lseek
#define TINYCLR_SSL_CHMOD               chmod
#define TINYCLR_SSL_FWRITE              fwrite
#define TINYCLR_SSL_GETTIMEOFDAY        gettimeofday
#define TINYCLR_SSL_GETPID              getpid
#define TINYCLR_SSL_FILE                FILE

#define TINYCLR_SSL_ASSERT(x)           ASSERT(x)
#define TINYCLR_SSL_QSORT               qsort
#define TINYCLR_SSL_EXIT                exit 
#define TINYCLR_SSL_ABORT               abort
#define TINYCLR_SSL_MALLOC              malloc 
#define TINYCLR_SSL_FREE                free
#define TINYCLR_SSL_REALLOC             realloc
#define TINYCLR_SSL_GETENV              getenv
#endif

OPENSSL_TYPE__FILE* tinyclr_fopen(const char * filename, const char * mode);
char* tinyclr_fgets(char * str, int num, OPENSSL_TYPE__FILE * stream);

int tinyclr_fclose ( OPENSSL_TYPE__FILE * stream );
size_t tinyclr_fread ( void * ptr, size_t size, size_t count, OPENSSL_TYPE__FILE * stream );
int tinyclr_feof ( OPENSSL_TYPE__FILE * stream );
int tinyclr_ferror ( OPENSSL_TYPE__FILE * stream );
int tinyclr_fseek ( OPENSSL_TYPE__FILE * stream, long int offset, int origin );
long int tinyclr_ftell ( OPENSSL_TYPE__FILE * stream );
int tinyclr_fflush ( OPENSSL_TYPE__FILE * stream );
int tinyclr_fputs ( const char * str, OPENSSL_TYPE__FILE * stream );
int tinyclr_sprintf ( char * str, const char * format, ... );
int tinyclr_fputc ( int character, OPENSSL_TYPE__FILE * stream );
int tinyclr_fgetc ( OPENSSL_TYPE__FILE * stream );
int tinyclr_fileno( OPENSSL_TYPE__FILE *stream );

#if defined(OPENSSL_SYS_WIN32) && defined(ARM_EMULATION_MODE)
#define TINYCLR_SSL_FOPEN               tinyclr_fopen
#define TINYCLR_SSL_FGETS               tinyclr_fgets
#define TINYCLR_SSL_FCLOSE              tinyclr_fclose
#define TINYCLR_SSL_FREAD               tinyclr_fread
#define TINYCLR_SSL_FEOF                tinyclr_feof
#define TINYCLR_SSL_FERROR              tinyclr_ferror
#define TINYCLR_SSL_FSEEK               tinyclr_fseek
#define TINYCLR_SSL_FTELL               tinyclr_ftell
#define TINYCLR_SSL_FFLUSH              tinyclr_fflush
#define TINYCLR_SSL_FPUTS               tinyclr_fputs
#define TINYCLR_SSL_FPUTC               tinyclr_fputc
#define TINYCLR_SSL_FGETC               tinyclr_fgetc
#define TINYCLR_SSL_SPRINTF             tinyclr_sprintf
#define TINYCLR_SSL_FILENO              tinyclr_fileno
#elif !defined(OPENSSL_SYS_WINDOWS)
#define TINYCLR_SSL_FOPEN               tinyclr_fopen
#define TINYCLR_SSL_FGETS               tinyclr_fgets
#define TINYCLR_SSL_FCLOSE              tinyclr_fclose
#define TINYCLR_SSL_FREAD               tinyclr_fread
#define TINYCLR_SSL_FEOF                tinyclr_feof
#define TINYCLR_SSL_FERROR              tinyclr_ferror
#define TINYCLR_SSL_FSEEK               tinyclr_fseek
#define TINYCLR_SSL_FTELL               tinyclr_ftell
#define TINYCLR_SSL_FFLUSH              tinyclr_fflush
#define TINYCLR_SSL_FPUTS               tinyclr_fputs
#define TINYCLR_SSL_FPUTC               tinyclr_fputc
#define TINYCLR_SSL_FGETC               tinyclr_fgetc
#define TINYCLR_SSL_SPRINTF(a,b,c)      hal_snprintf(a,sizeof(a),b,c)
#define TINYCLR_SSL_FILENO              tinyclr_fileno
#else
#define TINYCLR_SSL_FOPEN               fopen
#define TINYCLR_SSL_FGETS               fgets
#define TINYCLR_SSL_FCLOSE              fclose
#define TINYCLR_SSL_FREAD               fread
#define TINYCLR_SSL_FEOF                feof
#define TINYCLR_SSL_FERROR              ferror
#define TINYCLR_SSL_FSEEK               fseek
#define TINYCLR_SSL_FTELL               ftell
#define TINYCLR_SSL_FFLUSH              fflush
#define TINYCLR_SSL_FPUTS               fputs
#define TINYCLR_SSL_FPUTC               fputc
#define TINYCLR_SSL_FILENO              fileno

#define TINYCLR_SSL_FGETC               fgetc
#define TINYCLR_SSL_SPRINTF             sprintf
#endif


#ifndef OPENSSL_SYS_WINDOWS
#define TINYCLR_SSL_SELECT              SOCK_select
#define TINYCLR_SSL_ACCEPT              SOCK_accept
#define TINYCLR_SSL_CONNECT             SOCK_connect
#define TINYCLR_SSL_BIND                SOCK_bind
#define TINYCLR_SSL_LISTEN              SOCK_listen
#define TINYCLR_SSL_CLOSESOCKET         SOCK_close
#define TINYCLR_SSL_SOCKET              SOCK_socket
#define TINYCLR_SSL_SHUTDOWN            SOCK_shutdown
#define TINYCLR_SSL_RECV(a,b,c)         SOCK_recv(a,b,c,0)
#define TINYCLR_SSL_SEND(a,b,c)         SOCK_send(a,b,c,0)
#define TINYCLR_SSL_SOCKADDR            SOCK_sockaddr
#define TINYCLR_SSL_SOCKADDR_IN         SOCK_sockaddr_in
#define TINYCLR_SSL_TIMEVAL             SOCK_timeval
#define TINYCLR_SSL_GETSOCKOPT          SOCK_getsockopt
#define TINYCLR_SSL_SETSOCKOPT          SOCK_setsockopt
#define TINYCLR_SSL_RECVFROM            SOCK_recvfrom
#define TINYCLR_SSL_SENDTO              SOCK_sendto
#define TINYCLR_SSL_HTONS               SOCK_htons
#define TINYCLR_SSL_HTONL               SOCK_htonl 
#define TINYCLR_SSL_NTOHS               SOCK_ntohs
#define TINYCLR_SSL_NTOHL               SSL_ntohl
#define TINYCLR_SSL_GETHOSTBYNAME       tinyclr_ssl_gethostbyname
#define TINYCLR_SSL_GETHOSTBYADDR       tinyclr_ssl_gethostbyaddr
#define TINYCLR_SSL_GETSERVBYNAME       tinyclr_ssl_getservbyname
#define TINYCLR_SSL_GETLASTSOCKETERROR()errno
#define TINYCLR_SSL_IOCTL               SOCK_ioctl
#else
#define TINYCLR_SSL_SELECT              select
#define TINYCLR_SSL_ACCEPT              accept
#define TINYCLR_SSL_CONNECT             connect
#define TINYCLR_SSL_BIND                bind
#define TINYCLR_SSL_LISTEN              listen
#define TINYCLR_SSL_CLOSESOCKET         closesocket
#define TINYCLR_SSL_SOCKET              socket
#define TINYCLR_SSL_SHUTDOWN            shutdown
#define TINYCLR_SSL_RECV                recv
#define TINYCLR_SSL_SEND                send
#define TINYCLR_SSL_SOCKADDR            sockaddr
#define TINYCLR_SSL_SOCKADDR_IN         sockaddr_in
#define TINYCLR_SSL_TIMEVAL             timeval
#define TINYCLR_SSL_GETSOCKOPT          getsockopt
#define TINYCLR_SSL_SETSOCKOPT          setsockopt
#define TINYCLR_SSL_RECVFROM            recvfrom
#define TINYCLR_SSL_SENDTO              sendto
#define TINYCLR_SSL_HTONS               htons
#define TINYCLR_SSL_HTONL               htonl
#define TINYCLR_SSL_NTOHS               ntohs
#define TINYCLR_SSL_NTOHL               ntohl
#define TINYCLR_SSL_GETHOSTBYNAME       gethostbyname
#define TINYCLR_SSL_GETHOSTBYADDR       gethostbyaddr
#define TINYCLR_SSL_GETSERVBYNAME       getservbyname
#ifdef OPENSSL_SYS_WIN32
#define TINYCLR_SSL_GETLASTSOCKETERROR  WSAGetLastError
#else
#define TINYCLR_SSL_GETLASTSOCKETERROR() errno
#endif
#define TINYCLR_SSL_IOCTL               ioctl
#endif

#ifdef  __cplusplus
}
#endif

#endif //End: SSL_TYPES_H


