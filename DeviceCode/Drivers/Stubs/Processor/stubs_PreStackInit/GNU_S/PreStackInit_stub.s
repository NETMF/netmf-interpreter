@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .global  PreStackInit

    .section SectionForBootstrapOperations, "xa", %progbits

PreStackInit:


    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    @
    @ TODO: Enter your pre stack initialization code here (if needed)
    @       e.g. SDRAM initialization if you don't have/use SRAM for the stack
    @
    @ For "M" Profiles of the ARM Architecture (e.g. Cortex-M ) a stack is
    @ provided at power on reset so additional calls are allowed here. For
    @ other Architectures there may not be any valid stack at this point so
    @ no other method calls are allowed and the link register (LR) must be
    @ retained for the return.
    
    @ << ADD CODE HERE >>

    BX     lr

    
    @
    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .end


