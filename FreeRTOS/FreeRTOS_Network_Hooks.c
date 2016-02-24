/*
*/

/* FreeRTOS includes. */
#include "FreeRTOS.h"
#include "task.h"

/* FreeRTOS+TCP includes. */
#include "FreeRTOS_IP.h"

/* Define names that will be used for DNS, LLMNR and NBNS searches.  This allows
mainHOST_NAME to be used when the IP address is not known.  For example
"ping RTOSDemo" to resolve RTOSDemo to an IP address then send a ping request. */
#define mainHOST_NAME					"RTOSDemo"
#define mainDEVICE_NICK_NAME			"stm32"

/*-----------------------------------------------------------*/

const char *pcApplicationHostnameHook( void )
{
	/* Assign the name "rtosdemo" to this network node.  This function will be
	called during the DHCP: the machine will be registered with an IP address
	plus this name. */
	return mainHOST_NAME;
}
/*-----------------------------------------------------------*/

BaseType_t xApplicationDNSQueryHook( const char *pcName )
{
BaseType_t xReturn;

	/* Determine if a name lookup is for this node.  Two names are given
	to this node: that returned by pcApplicationHostnameHook() and that set
	by mainDEVICE_NICK_NAME. */
	if( strcasecmp( pcName, pcApplicationHostnameHook() ) == 0 )
	{
		xReturn = pdPASS;
	}
	else if( strcasecmp( pcName, mainDEVICE_NICK_NAME ) == 0 )
	{
		xReturn = pdPASS;
	}
	else
	{
		xReturn = pdFAIL;
	}


	return xReturn;
}
/*-----------------------------------------------------------*/

void vApplicationPingReplyHook( ePingReplyStatus_t eStatus, uint16_t usIdentifier )
{
}
/*-----------------------------------------------------------*/

