////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

void monitor_debug_printf( const char* format, ... );

//--//
#if defined(PLATFORM_ARM_MC9328MXS) && defined(FIQ_SAMPLING_PROFILER)
extern int ProfilerBufferBegin;
extern int ProfilerBufferEnd;

void FIQ_Profiler_Init()
{
    UINT32* DumpArea = (UINT32*)&ProfilerBufferBegin;    
   
    // clear the data area for profiler
    
    memset((void*)DumpArea, 0, (&ProfilerBufferEnd - &ProfilerBufferBegin + sizeof(ProfilerBufferEnd)));
    
    {
        GLOBAL_LOCK(irq);
        SmartPtr_FIQ fiq;

        Profiler_FIQ_Initialize();          
        

        MC9328MXL_AITC& AITC = MC9328MXL::AITC();

        // disable this interrupt while we change it
        AITC.INTDISNUM = MC9328MXL_AITC::c_IRQ_INDEX_TIMER2_INT;

        // set the correct type
        AITC.SetType( MC9328MXL_AITC::c_IRQ_INDEX_TIMER2_INT, TRUE );
        
        // enable the interrupt if we have a vector
        AITC.INTENNUM = MC9328MXL_AITC::c_IRQ_INDEX_TIMER2_INT;

        UINT32 mode = MC9328MXL_TIMER::TCTL_TEN;
        
        mode |= ( MC9328MXL_TIMER::TCTL_CLKSOURCE__PERCLK1 << MC9328MXL_TIMER::TCTL_CLKSOURCE_shift) & MC9328MXL_TIMER::TCTL_CLKSOURCE_mask;
        MC9328MXL_TIMER& TIMER = MC9328MXL::TIMER( MC9328MXL_Driver::c_PerformanceCounter_Timer );
        
        TIMER.TCTL = mode; 

        MC9328MXL_TIMER_Driver::SetCompare( MC9328MXL_Driver::c_PerformanceCounter_Timer, 150);

        MC9328MXL_TIMER_Driver::EnableCompareInterrupt( MC9328MXL_Driver::c_PerformanceCounter_Timer );

        g_MC9328MXL_TIMER_Driver.m_dump.InitializeForISR((HAL_CALLBACK_FPN )FIQ_Profiler_Dump);
        g_MC9328MXL_TIMER_Driver.m_dump.EnqueueDelta    (  2200000 );   

        
        
    }

    SmartPtr_FIQ::ForceEnabled();
  
}
void FIQ_Profiler_Dump()
{
    UINT32* DumpArea = (UINT32*)&ProfilerBufferBegin;    
    
    lcd_printf ("%s","Dump if the buffer is full\r\n");     
    // check the last location whether the allocated buffer is full or not
    //0x2000 location used as buffer full/empty indication
    if(*((UINT32*)&ProfilerBufferEnd)==0x1)
    {
        lcd_printf ("%-15s","Dumping...\r\n");
        
        {
            SmartPtr_FIQ fiq;
            
            USART_Initialize( ConvertCOM_ComPort(USART_DEFAULT_PORT), USART_DEFAULT_BAUDRATE, FALSE );
            monitor_debug_printf("0x%08x\r\n", 0xffffffff);//indicates buffer was full
            for(UINT32 i = 0; i < (&ProfilerBufferEnd - &ProfilerBufferBegin); i += sizeof(UINT32))
            {
                if(*DumpArea)
                {
                    monitor_debug_printf("0x%08x\r\n", *DumpArea);
                }               
                DumpArea++;
            }

        }       
        *((UINT32*)&ProfilerBufferEnd)=0;
        monitor_debug_printf("0x%08x\r\n", 0xeeeeeeee);//indicates buffer is empty
        g_MC9328MXL_TIMER_Driver.m_dump.EnqueueDelta    (  2200000 );        
        MC9328MXL_AITC& AITC = MC9328MXL::AITC();
        // set the correct type
        AITC.INTENNUM = MC9328MXL_AITC::c_IRQ_INDEX_TIMER2_INT; 

    }
    else
    {
        g_MC9328MXL_TIMER_Driver.m_dump.EnqueueDelta    (  1000 );
    }
    //clear the last location    
    
}
#endif  // defined(FIQ_SAMPLING_PROFILER)
//--//

