;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;
;  Licensed under the Apache License, Version 2.0 (the "License");
;  you may not use this file except in compliance with the License.
;  You may obtain a copy of the License at http:;www.apache.org/licenses/LICENSE-2.0
;
;  Copyright (c) Microsoft Corporation. All rights reserved.
;  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
;
;  Delay Loop
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    EXPORT  IDelayLoop

    AREA ||i.IDelayLoop||, CODE, READONLY ; void IDelayLoop(UINT32 count)

IDelayLoop
    subs    r0, r0, #4          ;; 1 cycle
    bgt     IDelayLoop          ;; 3 cycles taken, 1 cycle not taken.
    bx      lr                  ;; 3 cycles


    END
