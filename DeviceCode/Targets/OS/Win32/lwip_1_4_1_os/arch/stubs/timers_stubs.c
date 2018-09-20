#include "lwip/timers.h"
#include "lwip/opt.h"

#if LWIP_TIMERS
#if LWIP_TCP
/**
* Called from TCP_REG when registering a new PCB:
* the reason is to have the TCP timer only running when
* there are active (or time-wait) PCBs.
*/
void tcp_timer_needed(void)
{
}
#endif /* LWIP_TCP */
/** Initialize this module */
void sys_timeouts_init(void)
{
}

#else /* LWIP_TIMERS */
/* Satisfy the TCP code which calls this function */
void
tcp_timer_needed(void)
{
}
#endif /* LWIP_TIMERS */
