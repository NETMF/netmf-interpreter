@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@ This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
@@ Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
@@ 
@@ Licensed under the Apache License, Version 2.0 (the "License")@ you may not use these files except in compliance with the License.
@@ You may obtain a copy of the License at:
@@ 
@@ http://www.apache.org/licenses/LICENSE-2.0
@@ 
@@ Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
@@ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
@@ permissions and limitations under the License.
@@ 
@@ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@@ NOTE:
@@ This is only an example for use as a template in creating SoC specific versions and is not intended to be used directly
@@ for any particular SoC. Each SoC must provide a version of these handlers (and the corresponding VectorTables) that is
@@ specific to the SoC.
@@ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .syntax unified
    .arch armv7-m
    .thumb

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                
@ Dummy Exception Handlers (infinite loops which can be overloaded since they are exported with weak linkage )

    .section SectionForBootsrtapOperations, "ax", %progbits

    .global  FAULT_SubHandler
    .extern  FAULT_Handler             @ void FAULT_Handler(UINT32*, UINT32)

    .global  HARD_Breakpoint
    .extern  HARD_Breakpoint_Handler   @ HARD_Breakpoint_Handler(UINT32*)

@ This serves as an adapter from the Cortex-M exception signature
@ to map back to the original CLR API designed around the older ARM
@ mode exception architecture.
    .section i.FAULT_SubHandler, "ax", %progbits
    .thumb_func

FAULT_SubHandler:
        @ on entry, we have an exception frame on the stack:
        @ SP+00: R0
        @ SP+04: R1
        @ SP+08: R2
        @ SP+12: R3
        @ SP+16: R12
        @ SP+20: LR
        @ SP+24: PC
        @ SP+28: PSR
        @ R0-R12 are not overwritten yet
            add      sp,sp,#16             @ remove R0-R3
            push     {r0-r11}              @ store R0-R11
            mov      r0,sp
        @ R0+00: R0-R12
        @ R0+52: LR
        @ R0+56: PC
        @ R0+60: PSR
            mrs      r1,IPSR               @ exception number
            b        FAULT_Handler
        @ never expect to return

    .section    i.HARD_Breakpoint, "ax", %progbits
    HARD_Breakpoint:
    @ on entry, were are being called from C/C++ in Thread mode
        add      sp,sp,#-4            @ space for PSR
        push     {r14}                @ store original PC
        push     {r0-r12,r14}         @ store R0 - R12, LR
        mov      r0,sp
        mrs      r1,XPSR
        str      r1,[r0,#60]          @ store PSR
    @ R0+00: R0-R12
    @ R0+52: LR
    @ R0+56: PC
    @ R0+60: PSR
        b        HARD_Breakpoint_Handler
    @ never expect to return

.end
