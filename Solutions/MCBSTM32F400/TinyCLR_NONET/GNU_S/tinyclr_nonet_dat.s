@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

     .section   tinyclr_metadata, "a", %progbits
     .align 4

    .global  TinyClr_Dat_Start
    .global  TinyClr_Dat_End

TinyClr_Dat_Start:
    .incbin "tinyclr_nonet.dat"
TinyClr_Dat_End:

    .end

