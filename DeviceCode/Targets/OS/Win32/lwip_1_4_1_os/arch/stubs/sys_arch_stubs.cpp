#include "lwip/timers.h"
#include "lwip/opt.h"

extern "C"
{
	void sys_signal_sock_event()
	{
	}


void sys_init_timing()
{
}


u32_t sys_jiffies()
{
	return 0;
}

u32_t sys_now()
{
	return 0;
}

void InitSysArchProtect()
{
}

u32_t sys_arch_protect()
{
	return 0;
}

void sys_arch_unprotect(u32_t pval)
{
}

void sys_init()
{
}

void do_sleep(int ms)
{
}

err_t sys_sem_new(sys_sem_t *sem, u8_t count)
{
	return 0;
}

void sys_sem_free(sys_sem_t *sem)
{
}

u32_t sys_arch_sem_wait(sys_sem_t *sem, u32_t timeout)
{
	return 0;
}

void sys_sem_signal(sys_sem_t *sem)
{
}

sys_thread_t sys_thread_new(const char *name, lwip_thread_fn function, void *arg, int stacksize, int prio)
{
	return 0;
}

err_t sys_mbox_new(sys_mbox_t *mbox, int size)
{
	return 0;
}

void sys_mbox_free(sys_mbox_t *mbox)
{
}

void sys_mbox_post(sys_mbox_t *q, void *msg)
{
}

err_t sys_mbox_trypost(sys_mbox_t *q, void *msg)
{
	return 0;
}

u32_t sys_arch_mbox_fetch(sys_mbox_t *q, void **msg, u32_t timeout)
{
	return 0;
}

u32_t sys_arch_mbox_tryfetch(sys_mbox_t *q, void **msg)
{
	return 0;
}

//------------------------------------------------------------------------
// Hoisted out of original timers.c

/**
* Create a one-shot timer (aka timeout). Timeouts are processed in the
* following cases:
* - while waiting for a message using sys_timeouts_mbox_fetch()
* - by calling sys_check_timeouts() (NO_SYS==1 only)
*
* @param msecs time in milliseconds after that the timer should expire
* @param handler callback function to call when msecs have elapsed
* @param arg argument to pass to the callback function
*/
#if LWIP_DEBUG_TIMERNAMES
void sys_timeout_debug(u32_t msecs, sys_timeout_handler handler, void *arg, const char* handler_name)
#else /* LWIP_DEBUG_TIMERNAMES */
void sys_timeout(u32_t msecs, sys_timeout_handler handler, void *arg)
#endif /* LWIP_DEBUG_TIMERNAMES */
{
}

// create a periodic timer that triggers on a fixed interval (period)
void sys_periodic_timeout(u32_t msecs, sys_timeout_handler handler, void *arg)
{
}

/**
* Go through timeout list (for this task only) and remove the first matching
* entry, even though the timeout has not triggered yet.
*
* @note This function only works as expected if there is only one timeout
* calling 'handler' in the list of timeouts.
*
* @param handler callback function that would be called by the timeout
* @param arg callback argument that would be passed to handler
*/
void sys_untimeout(sys_timeout_handler handler, void *arg)
{
}

/**
* Wait (forever) for a message to arrive in an mbox.
* While waiting, timeouts are processed.
*
* @param mbox the mbox to fetch the message from
* @param msg the place to store the message
*/
void sys_timeouts_mbox_fetch(sys_mbox_t *mbox, void **msg)
{
}
}