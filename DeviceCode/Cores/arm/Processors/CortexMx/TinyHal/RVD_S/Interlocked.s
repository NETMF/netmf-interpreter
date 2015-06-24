;//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
;// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
;// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
;// 
;// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
;// You may obtain a copy of the License at:
;// 
;// http://www.apache.org/licenses/LICENSE-2.0
;// 
;// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
;// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
;// permissions and limitations under the License.
;// 
;//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    THUMB

;extern "C" INT32 InterlockedIncrement( volatile INT32* lpAddend );        
    AREA ||i.InterlockedIncrement||, CODE, READONLY, ALIGN=1
InterlockedIncrement PROC
        EXPORT InterlockedIncrement [WEAK]
        MOV      r1,r0
|ILIncLoop|
        LDREX    r0,[r1,#0]
        ADDS     r0,r0,#1
        STREX    r2,r0,[r1,#0]
        CMP      r2,#0
        BNE      |ILIncLoop|
        BX       lr
        
        ENDP

;extern "C" INT32 InterlockedDecrement( volatile INT32* lpAddend );
    AREA ||i.InterlockedDecrement||, CODE, READONLY, ALIGN=1
InterlockedDecrement PROC
        EXPORT InterlockedDecrement [WEAK]
        MOV      r1,r0
|IlDecLoop|
        LDREX    r0,[r1,#0]
        SUBS     r0,r0,#1
        STREX    r2,r0,[r1,#0]
        CMP      r2,#0
        BNE      |IlDecLoop|
        BX       lr

        ENDP

;extern "C" INT32 InterlockedAnd( volatile INT32* Destination, INT32 Flag );
    AREA ||i.InterlockedAnd||, CODE, READONLY, ALIGN=1
InterlockedAnd PROC
        EXPORT InterlockedAnd [WEAK]
        PUSH     {r4,lr}
        MOV      r2,r0
|L1.6|
        LDREX    r0,[r2,#0]
        AND      r3,r0,r1
        STREX    r4,r3,[r2,#0]
        CMP      r4,#0
        BNE      |L1.6|
        POP      {r4,pc}
        ENDP

;extern "C" INT32 InterlockedCompareExchange( INT32* Destination, INT32 Exchange, INT32 Comperand );
    AREA ||i.InterlockedCompareExchange||, CODE, READONLY, ALIGN=1
InterlockedCompareExchange PROC
        EXPORT InterlockedCompareExchange [WEAK]
        PUSH     {r4,lr}
        MOV      r3,r0
|L2.1|
        LDREX    r0,[r3,#0]
        CMP      r0,r2
        BNE      |L2.2|

        STREX    r4,r1,[r3,#0]
        CMP      r4,#0
        BNE      |L2.1|
        B        |L2.3|
|L2.2|
        CLREX
|L2.3|    
        POP      {r4,pc}
        ENDP

;extern "C" INT32 InterlockedExchange( volatile INT32* Target, INT32 Value );
    AREA ||i.InterlockedExchange||, CODE, READONLY, ALIGN=1
InterlockedExchange PROC
        EXPORT InterlockedExchange [WEAK]
        MOV      r2,r0
|L3.4|
        LDREX    r0,[r2,#0]
        STREX    r3,r1,[r2,#0]
        CMP      r3,#0
        BNE      |L3.4|
        BX       lr
        
        ENDP

;extern "C" INT32 InterlockedExchangeAdd( volatile INT32* Addend, INT32 Value );
    AREA ||i.InterlockedExchangeAdd||, CODE, READONLY, ALIGN=1
InterlockedExchangeAdd PROC
        EXPORT InterlockedExchangeAdd [WEAK]
        PUSH     {r4,lr}
        MOV      r2,r0
|L4.6|
        LDREX    r0,[r2,#0]
        ADDS     r4,r0,r1
        STREX    r3,r4,[r2,#0]
        CMP      r3,#0
        BNE      |L4.6|
        POP      {r4,pc}
    
        ENDP

;extern "C" INT32 InterlockedOr( volatile INT32* Destination, INT32 Flag );
    AREA ||i.InterlockedOr||, CODE, READONLY, ALIGN=1
InterlockedOr PROC
        EXPORT InterlockedOr [WEAK]
        PUSH     {r4,lr}
        MOV      r2,r0
|L5.6|
        LDREX    r0,[r2,#0]
        ORR      r3,r0,r1
        STREX    r4,r3,[r2,#0]
        CMP      r4,#0
        BNE      |L5.6|
        POP      {r4,pc}
        ENDP
            
    END