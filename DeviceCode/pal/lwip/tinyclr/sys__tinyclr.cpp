////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"
#include "mem.h"
#include "sys.h"
#include "tcpip.h"

//--//
//--// System Initailization, call before anything else
//--//

sys_timeo timeout_list;

//--//

void sys_init(void)
{
    memset(&timeout_list, 0, sizeof(timeout_list));
}

//--// 
//--// Timeout functions
//--//
struct sys_timeo *sys_arch_timeouts(void)
{
    return &timeout_list;
}

//--//
//--// Semaphore functions. 
//--//

void AcquireSemaphore(volatile UINT32* semaphore)
{
    *semaphore = *semaphore - 1;
}

void ReleaseSemaphore(volatile UINT32* semaphore)
{
    *semaphore = *semaphore + 1;
}

bool IsSemaphoreGreen(volatile UINT32* semaphore)
{
    return *semaphore != 0;
}

err_t sys_sem_new(sys_sem_t *sem, u8_t count)
{
    if(sem == NULL)
    {
        return ERR_ARG;
    }

    *sem = count;

    return ERR_OK;
}


void sys_sem_signal(sys_sem_t *sem)
{
    volatile UINT32* semaphore = (volatile UINT32*)sem;
    ReleaseSemaphore(semaphore);

    if(IsSemaphoreGreen(semaphore))
    {
        Events_Set(SYSTEM_EVENT_FLAG_NETWORK);
    }
}

u32_t sys_arch_sem_wait(sys_sem_t *sem, u32_t timeout)
{
    volatile UINT32* semaphore = (volatile UINT32*)sem;

    if(timeout != 0) 
    {
        INT64 now = ::HAL_Time_CurrentTime();
        INT64 elapsed = now + (timeout * 10000);
        
        while(elapsed > ::HAL_Time_CurrentTime()) 
        {
            if(IsSemaphoreGreen(semaphore)) 
            {
                AcquireSemaphore(semaphore); 
                break;
            }

            if(INTERRUPTS_ENABLED_STATE())
            {
                if(Events_WaitForEvents(SYSTEM_EVENT_FLAG_NETWORK, timeout))
                {
                    Events_Clear(SYSTEM_EVENT_FLAG_NETWORK);

                    INT64 curTime = ::HAL_Time_CurrentTime();

                    if(elapsed > curTime)
                    {
                        timeout -= (elapsed - HAL_Time_CurrentTime()) / 10000;
                    }
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }
    else
    {
        while(1) 
        {
            // wait and call the continuation for tcpip_thread
            if(IsSemaphoreGreen(semaphore)) 
            {
                AcquireSemaphore(semaphore);
                break;
            }
            
            if(INTERRUPTS_ENABLED_STATE())
            {
                Events_WaitForEvents(SYSTEM_EVENT_FLAG_NETWORK, EVENTS_TIMEOUT_INFINITE);
                Events_Clear(SYSTEM_EVENT_FLAG_NETWORK);
            }
            else
            {
                break;
            }
        }
    }

    Events_Set(SYSTEM_EVENT_FLAG_NETWORK);
    
    return *semaphore;
}

void sys_sem_free(sys_sem_t *sem)
{
    if(sem != NULL)
    {
        *sem = 1;
    }
}

int sys_sem_valid(sys_sem_t *sem)
{
    return (sem != NULL && *sem != 0xEFFFFFFFUL);
}

void sys_sem_set_invalid(sys_sem_t *sem)
{
    if(sem != NULL)
    {
        *sem = 0xEFFFFFFFUL;
    }
}


//--//
//--// Time jiffies (since power up) 
//--//
u32_t sys_jiffies(void)
{
    // IMPLEMENTED AS sys_now
    UINT64 currentTime = HAL_Time_CurrentTime() / 10000;

    return (u32_t)(currentTime);
}

void sys_signal_sock_event()
{
    Events_Set(SYSTEM_EVENT_FLAG_SOCKET);
}


//--//
//--// Mailbox functions. 
//--//

err_t sys_mbox_new(sys_mbox_t *mbox, int size)
{    
    if(mbox == NULL) { ASSERT(FALSE); return ERR_ARG; }

    Hal_Queue_UnknownSize<OpaqueQueueNode>* queue = (Hal_Queue_UnknownSize<OpaqueQueueNode>*)mem_malloc(sizeof(Hal_Queue_UnknownSize<OpaqueQueueNode>));
    OpaqueQueueNode* memory = (OpaqueQueueNode*)mem_malloc(sizeof(OpaqueQueueNode) * size);

    if(memory == NULL || queue == NULL)
    {
        if(queue != NULL) mem_free(queue);
            
        return ERR_MEM;
    }
    
    memset(memory, 0, sizeof(OpaqueQueueNode) * size);
    queue->Initialize(memory, size);

    *mbox = (sys_mbox_t)queue;
    
    return ERR_OK;
}

void sys_mbox_post(sys_mbox_t* mbox, void *msg)
{
    if(mbox == NULL || *mbox == NULL) { ASSERT(FALSE); return; }

    Hal_Queue_UnknownSize<OpaqueQueueNode>* queue = (Hal_Queue_UnknownSize<OpaqueQueueNode>*)*mbox;

    OpaqueQueueNode* node = NULL;
    
    if((node = queue->Push()) != NULL) 
    {
        node->payload = msg;
    }

    SOCKETS_RestartTcpIpProcessor(0);
}

err_t sys_mbox_trypost(sys_mbox_t* mbox, void *msg)
{
    if(mbox == NULL || *mbox == NULL) { ASSERT(FALSE); return ERR_ARG; }

    Hal_Queue_UnknownSize<OpaqueQueueNode>* queue = (Hal_Queue_UnknownSize<OpaqueQueueNode>*)*mbox;

    OpaqueQueueNode* node = queue->Push();

    if(node != NULL) 
    {
        node->payload = msg;

        SOCKETS_RestartTcpIpProcessor(0);

        return ERR_OK;    
    }

    return ERR_MEM;
}

u32_t sys_arch_mbox_fetch(sys_mbox_t* mbox, void **msg, u32_t timeout)
{
    if(mbox == NULL || *mbox == NULL) { ASSERT(FALSE); return SYS_ARCH_TIMEOUT; }

    Hal_Queue_UnknownSize<OpaqueQueueNode>* queue = (Hal_Queue_UnknownSize<OpaqueQueueNode>*)*mbox;
    bool didTimeout = false;

    if(timeout == 0) 
    {
        timeout = 0xFFFFFFFF;
    }

    INT64 now = ::HAL_Time_CurrentTime();
    INT64 elapsed = now + (timeout * 10000);
    
    while(elapsed > ::HAL_Time_CurrentTime() || timeout == 1) 
    {
        OpaqueQueueNode* node = queue->Pop();
        
        if(node) 
        {
            *msg = node->payload;

            Events_Set(SYSTEM_EVENT_FLAG_NETWORK);            

            SOCKETS_RestartTcpIpProcessor(0);
            
            return 0;
        }
        else if(timeout == 1)
        {
            break;
        }
        
        if(INTERRUPTS_ENABLED_STATE())
        {
            if(Events_WaitForEvents(SYSTEM_EVENT_FLAG_NETWORK, timeout))
            {
                Events_Clear(SYSTEM_EVENT_FLAG_NETWORK);
                
                INT64 curTime = ::HAL_Time_CurrentTime();

                if(elapsed > curTime)
                {
                    timeout -= (elapsed - HAL_Time_CurrentTime()) / 10000;
                }
            }
            else
            {
                break;
            }
        }
        else
        {
            break;
        }
    }

    if(timeout != 1)
    {
        Events_Set(SYSTEM_EVENT_FLAG_NETWORK);            
    }
    else
    {
        didTimeout = true;
    }

    *msg = NULL; 
    return didTimeout ? SYS_ARCH_TIMEOUT : SYS_MBOX_EMPTY;    
}

u32_t sys_arch_mbox_tryfetch(sys_mbox_t* mbox, void **msg)
{
    if(mbox == NULL || *mbox == NULL) { *msg = NULL; ASSERT(FALSE); return SYS_MBOX_EMPTY; }

    Hal_Queue_UnknownSize<OpaqueQueueNode>* queue = (Hal_Queue_UnknownSize<OpaqueQueueNode>*)*mbox;

    OpaqueQueueNode* node = queue->Pop();

    if(node)
    {
        *msg = node->payload;
        
        SOCKETS_RestartTcpIpProcessor(0);
    }
    else
    {
        *msg = NULL;
        return SYS_MBOX_EMPTY;
    }

    return 0;    
}

void sys_mbox_free(sys_mbox_t* mbox)
{
    if(mbox == NULL || *mbox == NULL) { ASSERT(FALSE); return; }

    Hal_Queue_UnknownSize<OpaqueQueueNode>* queue = (Hal_Queue_UnknownSize<OpaqueQueueNode>*)*mbox;
    
    mem_free(queue->Storage());
    mem_free(queue);
}

int sys_mbox_valid(sys_mbox_t *mbox)
{
    if(mbox == NULL) { ASSERT(FALSE); return 0; }

    return (*mbox != NULL);
}

void sys_mbox_set_invalid(sys_mbox_t *mbox)
{
    if(mbox == NULL) { ASSERT(FALSE); return; }

    *mbox = NULL;
}



//--//
//--// Thread functions. 
//--// 

sys_thread_t sys_thread_new(const char *name, lwip_thread_fn thread, void *arg, int stacksize, int prio)
{
    if(strcmp(name,TCPIP_THREAD_NAME) == 0)
    {
        SOCKETS_CreateTcpIpProcessor((HAL_CALLBACK_FPN)thread, arg);
    }

    return NULL;
}

void sys_thread_free(char *name)
{
    if(strcmp(name,TCPIP_THREAD_NAME) == 0)
    {
        // the tcip thread will be aborted later
        SOCKETS_RestartTcpIpProcessor(0xFFFFFFFF);
    }
}

//--//
//--// Time functions. 
//--// 

// Returns the current time in milliseconds.
u32_t sys_now(void) 
{
    UINT64 currentTime = HAL_Time_CurrentTime() / 10000;

    return (u32_t)(currentTime);
}

//--//
//--// Critical Region Protection. 
//--// 

#if SYS_LIGHTWEIGHT_PROT
sys_prot_t sys_arch_protect(void)
{
    if(INTERRUPTS_ENABLED_STATE())
    {
        DISABLE_INTERRUPTS();
        return 1;
    }

    return 0;
}

void sys_arch_unprotect(sys_prot_t pval)
{
    if(pval == 1)
    {
        ENABLE_INTERRUPTS();
    }
}
#endif

//--//
