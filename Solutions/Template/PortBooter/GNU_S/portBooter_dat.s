@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .section    .text, "xa", %progbits

    .global     PortBooterLoader_Dat

@   .global     ARM_Vectors
@ 
@ FirstEntry.obj refers to ARM_Vectors to include Vector_Tramp.obj.
@ We don't need those vectors, to make the linker happy let's just declare a symbol (0 byte overhead).
@ 
@ARM_Vectors

    .extern     EntryPoint
    
PortBooterLoader_Dat:
    .incbin     "PortBooter_loader.dat"
PortBooterLoader_Dat_End:

    .end

