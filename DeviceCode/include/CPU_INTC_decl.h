////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_INTC_DECL_H_
#define _DRIVERS_INTC_DECL_H_ 1

//--//

extern "C"
{
    void __irq IRQ_Handler();
}

//--//

void CPU_INTC_Initialize          (                                                         );
BOOL CPU_INTC_ActivateInterrupt   ( UINT32 Irq_Index, HAL_CALLBACK_FPN ISR, void* ISR_Param );
BOOL CPU_INTC_DeactivateInterrupt ( UINT32 Irq_Index                                        );
BOOL CPU_INTC_InterruptEnable     ( UINT32 Irq_Index                                        );
BOOL CPU_INTC_InterruptDisable    ( UINT32 Irq_Index                                        );
BOOL CPU_INTC_InterruptEnableState( UINT32 Irq_Index                                        );
BOOL CPU_INTC_InterruptState      ( UINT32 Irq_Index                                        );

//--//

#endif // _DRIVERS_INTC_DECL_H_

