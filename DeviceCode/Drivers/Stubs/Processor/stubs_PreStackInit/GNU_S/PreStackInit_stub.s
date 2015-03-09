@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .global  PreStackInit

    .extern  PreStackInit_Exit_Pointer

    .section SectionForBootstrapOperations, "xa", %progbits

PreStackInit:


    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    @
    @ TODO: Enter your pre stack initialization code here (if needed)
    @       e.g. SDRAM initialization if you don't have/use SRAM for the stack
    @
    
    @ << ADD CODE HERE >>

    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    @ DO NOT CHANGE THE FOLLOWING CODE! we can not use pop to return because we 
    @ loaded the PC register to get here (since the stack has not been initialized).  
    @ Make sure the PreStackInit_Exit_Pointer is within range and 
    @ in the SectionForBootstrapOperations
    @ go back to the firstentry(_loader) code 
    @


PreStackEnd:
    B     PreStackInit_Exit_Pointer

    
    @
    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .end


