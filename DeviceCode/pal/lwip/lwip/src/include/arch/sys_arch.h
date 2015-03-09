//
// sys_arch.h
//
// Contains defintions and typedefs specific to the interface between
// generic lwIP code and the ADFSP-BF535 architecture layer.
//
#ifndef _SYS_ARCH_H_
#define _SYS_ARCH_H_

#include <stdlib.h>
#include <limits.h>

// typedefs to tie lwIP to our porting (sys_arch) layer
typedef u32_t sys_prot_t;
typedef u32_t sys_sem_t;
typedef void* sys_mbox_t;
typedef void* sys_thread_t;

#define SYS_MBOX_NULL NULL
#define SYS_SEM_NULL  ((sys_sem_t)UINT_MAX)

struct sys_arch_thread* sys_arch_current_thread(void);

extern void sys_mbox_ISR_post(sys_mbox_t mbox, void* msg);

#endif // _SYS_ARCH_H_

