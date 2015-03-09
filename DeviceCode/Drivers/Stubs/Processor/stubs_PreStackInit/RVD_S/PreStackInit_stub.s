;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    EXPORT  PreStackInit

    IMPORT  PreStackInit_Exit_Pointer

    PRESERVE8

    AREA SectionForBootstrapOperations, CODE, READONLY

    ENTRY

    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB  
    ARM
    ENDIF    

PreStackInit


    ;*************************************************************************
    ;
    ; TODO: Enter your pre stack initialization code here (if needed)
    ;       e.g. SDRAM initialization if you don't have/use SRAM for the stack
    ;
    
    ; << ADD CODE HERE >>

    ;*************************************************************************
    ; DO NOT CHANGE THE FOLLOWING CODE! we can not use pop to return because we 
    ; loaded the PC register to get here (since the stack has not been initialized).  
    ; Make sure the PreStackInit_Exit_Pointer is within range and 
    ; in the SectionForBootstrapOperations
    ; go back to the firstentry(_loader) code 
    ;
    ;
PreStackEnd
    B     PreStackInit_Exit_Pointer

    ;
    ;**************************************************************************
    IF :DEF:COMPILE_THUMB  
    THUMB
    ENDIF   


    END


