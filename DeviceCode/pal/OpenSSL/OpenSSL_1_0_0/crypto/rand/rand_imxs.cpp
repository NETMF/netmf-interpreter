#define USE_SOCKETS
#include "e_os.h"
#include "cryptlib.h"
#include <openssl/rand.h>
#include "rand_lcl.h"


#if !defined(OPENSSL_SYS_WINDOWS)
int RAND_poll(void)
	{
	unsigned long Time=(unsigned long)TINYCLR_SSL_TIME(NULL);

	RAND_add(&Time,sizeof(Time),ENTROPY_NEEDED);

	return 1;
	}
#endif
