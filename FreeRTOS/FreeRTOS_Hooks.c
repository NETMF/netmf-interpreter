/*
    These hooks are defined here as weak to be able to override them in solution
*/

/* FreeRTOS includes. */
#include "FreeRTOS.h"
#include "task.h"

/* The trace macros are used to keep a count of seconds. */
uint32_t ulSeconds, ulMsec;

// prototypes
void vApplicationTickHook( void ) __attribute__ (( weak ));
void vApplicationMallocFailedHook( void ) __attribute__ (( weak ));
void vApplicationIdleHook( void ) __attribute__ (( weak ));
void vApplicationStackOverflowHook( TaskHandle_t pxTask, char *pcTaskName ) __attribute__ (( weak ));
void vAssertCalled( const char *pcFile, uint32_t ulLine ) __attribute__ (( weak ));
uint32_t xGetRunTimeCounterValue( void ) __attribute__ (( weak ));

void vApplicationTickHook( void )
{
// 	#if ( mainCREATE_SIMPLE_LED_FLASHER_DEMO_ONLY == 0 )
// 	{
// 		/* Just to verify that the interrupt nesting behaves as expected,
// 		increment ulFPUInterruptNesting on entry, and decrement it on exit. */
// 		ulFPUInterruptNesting++;
// 
// 		/* Fill the FPU registers with 0. */
// 		vRegTestClearFlopRegistersToParameterValue( 0UL );
// 
// 		/* Trigger a timer 2 interrupt, which will fill the registers with a
// 		different value and itself trigger a timer 3 interrupt.  Note that the
// 		timers are not actually used.  The timer 2 and 3 interrupt vectors are
// 		just used for convenience. */
// 		NVIC_SetPendingIRQ( TIM2_IRQn );
// 
// 		/* Ensure that, after returning from the nested interrupts, all the FPU
// 		registers contain the value to which they were set by the tick hook
// 		function. */
// 		configASSERT( ulRegTestCheckFlopRegistersContainParameterValue( 0UL ) );
// 
// 		ulFPUInterruptNesting--;
// 	}
// 	#endif
}

void vApplicationMallocFailedHook( void )
{
	/* vApplicationMallocFailedHook() will only be called if
	configUSE_MALLOC_FAILED_HOOK is set to 1 in FreeRTOSConfig.h.  It is a hook
	function that will get called if a call to pvPortMalloc() fails.
	pvPortMalloc() is called internally by the kernel whenever a task, queue,
	timer or semaphore is created.  It is also called by various parts of the
	demo application.  If heap_1.c or heap_2.c are used, then the size of the
	heap available to pvPortMalloc() is defined by configTOTAL_HEAP_SIZE in
	FreeRTOSConfig.h, and the xPortGetFreeHeapSize() API function can be used
	to query the size of free heap space that remains (although it does not
	provide information on how the remaining heap might be fragmented). */
	//taskDISABLE_INTERRUPTS();
	for( ;; );
}
/*-----------------------------------------------------------*/

void vApplicationIdleHook( void )
{
	/* vApplicationIdleHook() will only be called if configUSE_IDLE_HOOK is set
	to 1 in FreeRTOSConfig.h.  It will be called on each iteration of the idle
	task.  It is essential that code added to this hook function never attempts
	to block in any way (for example, call xQueueReceive() with a block time
	specified, or call vTaskDelay()).  If the application makes use of the
	vTaskDelete() API function (as this demo application does) then it is also
	important that vApplicationIdleHook() is permitted to return to its calling
	function, because it is the responsibility of the idle task to clean up
	memory allocated by the kernel to any task that has since been deleted. */
}
/*-----------------------------------------------------------*/

void vApplicationStackOverflowHook( TaskHandle_t pxTask, char *pcTaskName )
{
	( void ) pcTaskName;
	( void ) pxTask;

	/* Run time stack overflow checking is performed if
	configCHECK_FOR_STACK_OVERFLOW is defined to 1 or 2.  This hook
	function is called if a stack overflow is detected. */
	//taskDISABLE_INTERRUPTS();
	for( ;; );
}

void vAssertCalled( const char *pcFile, uint32_t ulLine )
{
    volatile uint32_t ulBlockVariable = 0UL;
    volatile char *pcFileName = ( volatile char *  ) pcFile;
    volatile uint32_t ulLineNumber = ulLine;

	( void ) pcFileName;
	( void ) ulLineNumber;

	//FreeRTOS_debug_printf( ( "vAssertCalled( %s, %ld\n", pcFile, ulLine ) );

	/* Setting ulBlockVariable to a non-zero value in the debugger will allow
	this function to be exited. */
	taskDISABLE_INTERRUPTS();
	{
		while( ulBlockVariable == 0UL )
		{
			__asm volatile( "NOP" );
		}
	}
	taskENABLE_INTERRUPTS();
}

uint32_t xGetRunTimeCounterValue( void )
{
    static uint64_t ullHiresTime = 0; /* Is always 0? */

	return ( uint32_t ) ( HAL_Time_CurrentTicks() - ullHiresTime );
}
