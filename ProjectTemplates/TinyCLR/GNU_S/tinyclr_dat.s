@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

     .align 4
     .section   tinyclr_metadata, "a", %progbits

    .global  TinyClr_Dat_Start
    .global  TinyClr_Dat_End

TinyClr_Dat_Start:
    .incbin "tinyclr.dat"
TinyClr_Dat_End:

    .end

