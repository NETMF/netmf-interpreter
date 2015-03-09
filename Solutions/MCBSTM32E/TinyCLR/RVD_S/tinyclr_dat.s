;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;
;  Licensed under the Apache License, Version 2.0 (the "License");
;  you may not use this file except in compliance with the License.
;  You may obtain a copy of the License at http:;www.apache.org/licenses/LICENSE-2.0
;
;  Copyright (c) Microsoft Corporation. All rights reserved.
;  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
    AREA |.text|, CODE, READONLY

    ; has to keep it as ARM code, otherwise the the label TinyClr_Dat_Start and TinyClr_Dat_End are 1 word shift.

    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF
    
    EXPORT  TinyClr_Dat_Start
    EXPORT  TinyClr_Dat_End

TinyClr_Dat_Start  DATA
    INCBIN tinyclr.dat
TinyClr_Dat_End

    END
