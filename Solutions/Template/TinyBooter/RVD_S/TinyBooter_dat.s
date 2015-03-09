;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    AREA |.text|, CODE, READONLY

    ; has to keep it as ARM code, otherwise the the label TinyBooterCompressed_Dat and TinyBooterCompressed_Dat_End are 1 word shift.
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF

    EXPORT  TinyBooterCompressed_Dat
    IMPORT  EntryPoint


TinyBooterCompressed_Dat
    INCBIN TinyBooter_Compressed.dat
TinyBooterCompressed_Dat_End

    END
