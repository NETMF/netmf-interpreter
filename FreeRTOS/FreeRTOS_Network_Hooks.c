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

/* Set the following constants to 1 or 0 to define which tasks to include and
exclude.  A WIDER RANGE OF EXAMPLES ARE AVAILABLE IN THE WIN32 DEMO:
http://www.freertos.org/FreeRTOS-Plus/FreeRTOS_Plus_TCP/examples_FreeRTOS_simulator.html

mainCREATE_FTP_SERVER:  When set to 1 the TCP server task will include an FTP
server.
See http://www.freertos.org/FreeRTOS-Plus/FreeRTOS_Plus_TCP/FTP_Server.html

mainCREATE_HTTP_SERVER:  When set to 1 the TCP server task will include a basic
HTTP server.
See http://www.freertos.org/FreeRTOS-Plus/FreeRTOS_Plus_TCP/HTTP_web_Server.html

mainCREATE_UDP_CLI_TASKS:  When set to 1 a command console that uses a UDP port
for input and output is created using FreeRTOS+CLI.  The port number used is set
by the mainUDP_CLI_PORT_NUMBER constant above.  A dumb UDP terminal such as YAT
can be used to connect.
See http://www.freertos.org/FreeRTOS-Plus/FreeRTOS_Plus_TCP/UDP_CLI.html

mainCREATE_TCP_ECHO_TASKS_SINGLE:  When set to 1 a set of tasks are created that
send TCP echo requests to the standard echo port (port 7), then wait for and
verify the echo reply, from within the same task (Tx and Rx are performed in the
same RTOS task).  The IP address of the echo server must be configured using the
configECHO_SERVER_ADDR0 to configECHO_SERVER_ADDR3 constants in
FreeRTOSConfig.h.
See http://www.freertos.org/FreeRTOS-Plus/FreeRTOS_Plus_TCP/TCP_Echo_Clients.html

mainCREATE_SIMPLE_TCP_ECHO_SERVER:  When set to 1 FreeRTOS tasks are used with
FreeRTOS+TCP to create a TCP echo server.  Echo requests can be sent to the
FreeRTOS+TCP using a tool such as EchoTool https://github.com/PavelBansky/EchoTool
(a pre-built binary is available).
See http://www.freertos.org/FreeRTOS-Plus/FreeRTOS_Plus_TCP/TCP_Echo_Server.html
*/
#define mainCREATE_FTP_SERVER					0
#define mainCREATE_HTTP_SERVER 					0
#define mainCREATE_UDP_CLI_TASKS				0
#define mainCREATE_TCP_ECHO_CLIENT_TASKS_SINGLE	0
#define mainCREATE_SIMPLE_TCP_ECHO_SERVER		1
#define mainCREATE_UDP_LOGGING_TASK 			0

/* Simple echo server task parameters ---------------------------------------
See http://www.freertos.org/FreeRTOS-Plus/FreeRTOS_Plus_TCP/TCP_Echo_Server.html */
#define mainECHO_SERVER_TASK_PRIORITY	( tskIDLE_PRIORITY + 2 )
#define	mainECHO_SERVER_STACK_SIZE		( configMINIMAL_STACK_SIZE * 4 )

extern void vStartSimpleTCPServerTasks( uint16_t usStackSize, UBaseType_t uxPriority );

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

/* Called by FreeRTOS+TCP when the network connects or disconnects.  Disconnect
events are only received if implemented in the MAC driver. */
void vApplicationIPNetworkEventHook( eIPCallbackEvent_t eNetworkEvent )
{
    uint32_t ulIPAddress, ulNetMask, ulGatewayAddress, ulDNSServerAddress;
    char cBuffer[ 16 ];
    static BaseType_t xTasksAlreadyCreated = pdFALSE;

//	FreeRTOS_printf( ( "vApplicationIPNetworkEventHook: event %ld\n", eNetworkEvent ) );

	/* If the network has just come up...*/
	if( eNetworkEvent == eNetworkUp )
	{
		/* Create the tasks that use the IP stack if they have not already been
		created. */
		if( xTasksAlreadyCreated == pdFALSE )
		{
			/* Tasks that use the TCP/IP stack can be created here. */

			/* Start a new task to fetch logging lines and send them out. */
			#if( mainCREATE_UDP_LOGGING_TASK == 1 )
			{
				vUDPLoggingTaskCreate();
			}
			#endif

			#if( ( mainCREATE_FTP_SERVER == 1 ) || ( mainCREATE_HTTP_SERVER == 1 ) )
			{
				/* Let the server work task now it can now create the servers. */
				xTaskNotifyGive( xServerWorkTaskHandle );
			}
			#endif

			#if( mainCREATE_TCP_ECHO_CLIENT_TASKS_SINGLE == 1 )
			{
				vStartTCPEchoClientTasks_SingleTasks( mainECHO_CLIENT_TASK_STACK_SIZE, mainECHO_CLIENT_TASK_PRIORITY );
			}
			#endif

			#if( mainCREATE_SIMPLE_TCP_ECHO_SERVER == 1 )
			{
				vStartSimpleTCPServerTasks( mainECHO_SERVER_STACK_SIZE, mainECHO_SERVER_TASK_PRIORITY );
			}
			#endif

			/* Register example commands with the FreeRTOS+CLI command
			interpreter via the UDP port specified by the
			mainUDP_CLI_PORT_NUMBER constant.  If the HTTP or FTP servers are
			being created then file system related commands will also be
			registered from the TCP server task. */
			#if( mainCREATE_UDP_CLI_TASKS == 1 )
			{
				vRegisterSampleCLICommands();
				vRegisterTCPCLICommands();
				vStartUDPCommandInterpreterTask( mainUDP_CLI_TASK_STACK_SIZE, mainUDP_CLI_PORT_NUMBER, mainUDP_CLI_TASK_PRIORITY );
			}
			#endif

			xTasksAlreadyCreated = pdTRUE;
		}

		/* Print out the network configuration, which may have come from a DHCP
		server. */
		FreeRTOS_GetAddressConfiguration( &ulIPAddress, &ulNetMask, &ulGatewayAddress, &ulDNSServerAddress );
		FreeRTOS_inet_ntoa( ulIPAddress, cBuffer );
//		FreeRTOS_printf( ( "IP Address: %s\n", cBuffer ) );

		FreeRTOS_inet_ntoa( ulNetMask, cBuffer );
//		FreeRTOS_printf( ( "Subnet Mask: %s\n", cBuffer ) );

		FreeRTOS_inet_ntoa( ulGatewayAddress, cBuffer );
//		FreeRTOS_printf( ( "Gateway Address: %s\n", cBuffer ) );

		FreeRTOS_inet_ntoa( ulDNSServerAddress, cBuffer );
//		FreeRTOS_printf( ( "DNS Server Address: %s\n", cBuffer ) );
	}
}


