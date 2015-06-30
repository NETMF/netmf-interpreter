;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    AREA |.text|, CODE, READONLY

    ; has to keep it as ARM code, otherwise the the label TinyClr_Dat_Start and TinyClr_Dat_End are 1 word shift.

    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF
    
    EXPORT  TinyClr_Dat_Start
    EXPORT  TinyClr_Dat_End

TinyClr_Dat_Start  DATA
    INCBIN tinyclr_nonet.dat
TinyClr_Dat_End

    END
