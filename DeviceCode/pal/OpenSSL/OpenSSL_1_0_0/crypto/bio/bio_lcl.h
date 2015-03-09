#include <openssl/bio.h>

#if BIO_FLAGS_UPLINK==0
/* Shortcut UPLINK calls on most platforms... */
#define	UP_stdin	OPENSSL_TYPE__FILE_STDIN
#define	UP_stdout	OPENSSL_TYPE__FILE_STDOUT
#define	UP_stderr	OPENSSL_TYPE__FILE_STDERR
#define	UP_fprintf	TINYCLR_SSL_FPRINTF
#define	UP_fgets	TINYCLR_SSL_FGETS
#define	UP_fread	TINYCLR_SSL_FREAD
#define	UP_fwrite	TINYCLR_SSL_FWRITE
#undef	UP_fsetmod
#define	UP_feof		TINYCLR_SSL_FEOF
#define	UP_fclose	TINYCLR_SSL_FCLOSE

#define	UP_fopen	TINYCLR_SSL_FOPEN
#define	UP_fseek	TINYCLR_SSL_FSEEK
#define	UP_ftell	TINYCLR_SSL_FTELL
#define	UP_fflush	TINYCLR_SSL_FFLUSH
#define	UP_ferror	TINYCLR_SSL_FERROR
#ifdef _WIN32
#define	UP_fileno	_fileno
#define	UP_open		_open
#define	UP_read		_read
#define	UP_write	_write
#define	UP_lseek	_lseek
#define	UP_close	_close
#else
#define	UP_fileno	fileno
#define	UP_open		open
#define	UP_read		read
#define	UP_write	write
#define	UP_lseek	TINYCLR_SSL_LSEEK
#define	UP_close	close
#endif
#endif
