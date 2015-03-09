;============================== FILE HEADER ==========================================================
;   File Name           : Cortexm3.s
;   Description         : Cortexm3 assembly functions for corresponding inline assembly .
;=====================================================================================================

NVIC_ICER     EQU 0xE000E180
NVIC_ISER     EQU 0xE000E100
NVIC_ABR      EQU 0xE000E300
DISABLED_MASK EQU 0x00000003


     EXPORT IRQ_LOCK_Release_asm
     AREA |IRQ_LOCK_Release_asm$$CODE|, CODE, READONLY
IRQ_LOCK_Release_asm
     LDR     r1,=NVIC_ICER
     LDR     r0,[r1]
     LDR     r1,=NVIC_ISER
     LDR     r2,=DISABLED_MASK
     STR     r2,[r1]     
     BX      LR                        ; Return from subroutine.
     
     ALIGN
     AREA  |IRQ_LOCK_GetState_asm$$CODE|, CODE ,READONLY 
     EXPORT IRQ_LOCK_GetState_asm
IRQ_LOCK_GetState_asm
     LDR     r1,=NVIC_ABR
     LDR     r0,[r1]
     AND     r0,r0,#0x3
     BX      LR                        ; Return from subroutine.                  

     ALIGN
     AREA  |IRQ_LOCK_Probe_asm$$CODE|, CODE ,READONLY 
     EXPORT IRQ_LOCK_Probe_asm
IRQ_LOCK_Probe_asm
     LDR     r1,=NVIC_ISER
     LDR     r0,[r1]
     LDR     r2,=DISABLED_MASK
     STR     r2,[r1] 
     STR     r0,[r1]
     BX      LR                        ; Return from subroutine.
     
     ALIGN
     AREA  |IRQ_LOCK_ForceDisabled_asm$$CODE|, CODE ,READONLY 
     EXPORT IRQ_LOCK_ForceDisabled_asm
IRQ_LOCK_ForceDisabled_asm
     LDR     r1,=NVIC_ICER
     LDR     r0,[r1]
     AND     r0,r0,#0x3
     LDR     r2,=DISABLED_MASK
     STR     r2,[r1]     
     BX      LR                        ; Return from subroutine.
     
     AREA  |IRQ_LOCK_ForceEnabled_asm$$CODE|, CODE ,READONLY 
     EXPORT IRQ_LOCK_ForceEnabled_asm
IRQ_LOCK_ForceEnabled_asm
     LDR     r1,=NVIC_ICER
     LDR     r0,[r1]
     AND     r0,r0,#0x3
     LDR     r1,=NVIC_ISER
     LDR     r2,=DISABLED_MASK
     STR     r2,[r1]     
     BX      LR                        ; Return from subroutine.
     
     ALIGN
     AREA  |IRQ_LOCK_Disable_asm$$CODE|, CODE ,READONLY 
     EXPORT IRQ_LOCK_Disable_asm
IRQ_LOCK_Disable_asm
     LDR     r0,=NVIC_ICER
     LDR     r1,=DISABLED_MASK
     STR     r1,[r0]
     BX      LR                        ; Return from subroutine.

     ALIGN
     AREA  |IRQ_LOCK_Restore_asm$$CODE|, CODE ,READONLY 
     EXPORT IRQ_LOCK_Restore_asm
IRQ_LOCK_Restore_asm
     LDR     r0,=NVIC_ISER
     LDR     r1,=DISABLED_MASK
     STR     r1,[r0]
     BX      LR                        ; Return from subroutine.

     ALIGN
     AREA  |CPU_InvalidateTLBs_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_InvalidateTLBs_asm
CPU_InvalidateTLBs_asm
     
     BX      lr                        ; Return from subroutine.

     AREA  |CPU_EnableMMU_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_EnableMMU_asm
CPU_EnableMMU_asm
     
     BX      lr                        ; Return from subroutine.

     AREA  |CPU_DisableMMU_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_DisableMMU_asm
CPU_DisableMMU_asm
          
     BX      lr                        ; Return from subroutine.

     AREA  |CPU_IsMMUEnabled_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_IsMMUEnabled_asm
CPU_IsMMUEnabled_asm
         
     BX      lr                        ; Return from subroutine.

     AREA  |CPU_FlushCaches1_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_FlushCaches1_asm
CPU_FlushCaches1_asm
     
     BX      lr                        ; Return from subroutine.

     AREA  |CPU_FlushCaches2_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_FlushCaches2_asm
CPU_FlushCaches2_asm
          
     BX      lr                        ; Return from subroutine.

     AREA  |CPU_DrainWriteBuffers_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_DrainWriteBuffers_asm
CPU_DrainWriteBuffers_asm
     
     BX      lr                        ; Return from subroutine.


     AREA  |CPU_InvalidateCaches_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_InvalidateCaches_asm
CPU_InvalidateCaches_asm
     
     BX      lr                        ; Return from subroutine.
 
     AREA  |CPU_EnableCaches_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_EnableCaches_asm
CPU_EnableCaches_asm
     
     BX      lr                        ; Return from subroutine.

     AREA  |CPU_DisableCaches_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_DisableCaches_asm
CPU_DisableCaches_asm
     
     BX      lr                        ; Return from subroutine.

     AREA  |CPU_InvalidateAddress_asm$$CODE|, CODE ,READONLY
     EXPORT CPU_InvalidateAddress_asm
CPU_InvalidateAddress_asm
     
     BX      lr                        ; Return from subroutine.

     AREA  |MC9328MXL_TranslateAddress_asm$$CODE|, CODE ,READONLY
     EXPORT MC9328MXL_TranslateAddress_asm
MC9328MXL_TranslateAddress_asm
          
     BX      lr                        ; Return from subroutine.

     AREA  |MC9328MXL_TurnOnAsyncMode_asm$$CODE|, CODE ,READONLY
     EXPORT MC9328MXL_TurnOnAsyncMode_asm
MC9328MXL_TurnOnAsyncMode_asm
     BX      lr                        ; Return from subroutine.


     
     AREA  |MC9328MXL_Bootstrap_Delay_asm$$CODE|, CODE ,READONLY
     EXPORT MC9328MXL_Bootstrap_Delay_asm
MC9328MXL_Bootstrap_Delay_asm
Loop     
     BX      lr                        ; Return from subroutine.

     AREA  |MC9328MXL_BootstrapCode_ARM1_asm$$CODE|, CODE ,READONLY
     EXPORT MC9328MXL_BootstrapCode_ARM1_asm
MC9328MXL_BootstrapCode_ARM1_asm
     
     BX      lr                        ; Return from subroutine.

     AREA  |MC9328MXL_BootstrapCode_ARM2_asm$$CODE|, CODE ,READONLY
     EXPORT MC9328MXL_BootstrapCode_ARM2_asm
MC9328MXL_BootstrapCode_ARM2_asm
     
     BX      lr                        ; Return from subroutine.

     AREA  |MC9328MXL_BootstrapCode_ARM3_asm$$CODE|, CODE ,READONLY
     EXPORT MC9328MXL_BootstrapCode_ARM3_asm
MC9328MXL_BootstrapCode_ARM3_asm
     
     BX      lr                        ; Return from subroutine.

     AREA  |MC9328MXL_BootstrapCode_ARM4_asm$$CODE|, CODE ,READONLY
     EXPORT MC9328MXL_BootstrapCode_ARM4_asm
MC9328MXL_BootstrapCode_ARM4_asm
          
     BX      lr                        ; Return from subroutine.

     
     AREA  |MC9328MXL_LowPower_asm$$CODE|, CODE ,READONLY
     EXPORT MC9328MXL_LowPower_asm
MC9328MXL_LowPower_asm
     
     BX      lr                        ; Return from subroutine.

     AREA  |MC9328MXL_Delayloop_asm$$CODE|, CODE ,READONLY
     EXPORT MC9328MXL_Delayloop_asm
MC9328MXL_Delayloop_asm    
     
     BX      lr                        ; Return from subroutine.     

     AREA  |NOP_asm$$CODE|, CODE ,READONLY
     EXPORT NOP_asm
NOP_asm
     nop
     bx  lr                            ; Return from subroutine.
         
  
     END
   
