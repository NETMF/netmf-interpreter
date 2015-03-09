;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    AREA |.text|, CODE, READONLY

    ; has to keep it as ARM code, otherwise the the label PortBooterLoader_Dat and PortBooterLoader_Dat_End are 1 word shift.
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF

    EXPORT  PortBooterLoader_Dat

;	EXPORT  ARM_Vectors
; 
; FirstEntry.obj refers to ARM_Vectors to include Vector_Tramp.obj.
; We don't need those vectors, to make the linker happy let's just declare a symbol (0 byte overhead).
; 
;ARM_Vectors
		
PortBooterLoader_Dat
    INCBIN PortBooter_loader.dat
PortBooterLoader_Dat_End

    END
