@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .cpu cortex-m3
    .code 16

    .global  EntryPoint

    .ifdef HAL_REDUCESIZE
    .extern BootEntryLoader
    .else
    .extern BootEntry
    .endif

    .extern  BootstrapCode
    .extern  Boot_Vectors         @ Even if we don't use this symbol, it's required by the linker to properly include the Vector trampolines.
	.extern  ARM_Vectors
	.extern  Prot_Bytes


@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .section SectionForStackBottom,       "a", %progbits
StackBottom:
    .word   0
    .section SectionForStackTop,          "a", %progbits
StackTop:
    .word   0
    .section SectionForHeapBegin,         "a", %progbits
HeapBegin:
    .word   0
    .section SectionForHeapEnd,           "a", %progbits
HeapEnd:
    .word   0
    .section SectionForCustomHeapBegin,   "a", %progbits
CustomHeapBegin:
    .word   0
    .section SectionForCustomHeapEnd,     "a", %progbits
CustomHeapEnd:
    .word   0

    .global StackBottom
    .global StackTop
    .global HeapBegin
    .global HeapEnd
    .global CustomHeapBegin
    .global CustomHeapEnd

    .section i.EntryPoint, "xa", %progbits

    @ signature & mini vector table
    .word   0xDEADBEEF
    .word   EntryPoint

    @ have to reference them otherwise they dont get linked in
    .word   Boot_Vectors
    .word   ARM_Vectors
    .word   Prot_Bytes

    .thumb_func
EntryPoint:
    bl BootstrapCode
    bl BootEntry

	.end
