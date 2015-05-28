@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .global IDelayLoop
    .global IDelayLoop2

    .cpu cortex-m3
    .code 16

    .section   SectionForFlashOperations,"xa", %progbits       @  void IDelayLoop(UINT32 count)

    .thumb_func
IDelayLoop:
IDelayLoop__Fi_b:
 
    sub    r0, r0, #4          @@ 1 cycle
    bgt     IDelayLoop__Fi_b    @@ 3 cycles, expect the last round, which is 1 cycle.
	
    mov     pc, lr              @@ 3 cycles, expect the last round, which is 1 cycle.

        @@ Total cost of execution: 8 + 4 * count.
        @@
        @@ From RAM  : 1 +  3 + 9 + 4 * count (The extra   cycle   is to move the delay value into the R0 register).
        @@ From FLASH: 3 + 22 + 9 + 4 * count (The extra 3 cycles are to move the delay value into the R0 register).

        @@ wait states = 0, 4 clocks per iteration
        @@  SUBS    FDE FDE FDE FDE
        @@  BGT      FDEE*DEE*DEE*DEE
        @@            FD  FD  FD  FD
        @@             FD  FD  FD  FD
        @@
        @@ wait states = 1, 8 clocks per iteration
        @@  SUBS    FFD E   FFD E   FFD E   FFD E
        @@  BGT       FFD E E *FD E E *FD E E *FD E
        @@              FFD     FFD     FFD     FFD
        @@                FFD     FFD     FFD     FD
        @@
        @@ wait states = 2, 12 clocks per iteration
        @@  SUBS    FFFD  E     FFFD  E     FFFD  E     FFFD  E
        @@  BGT        FFFD  E  E  *FFD  E  E  *FFD  E  E  *FFD  E
        @@                FFFD        FFFD        FFFD        FFFD
        @@                   FFFD        FFFD        FFFD        F


@@ The following loop was implemented to handle the XScale processors which feature branch
@@ prediction.  Because of this, the branch when taken requires only 1 cycle.  Note that
@@ this routine only works if both I cache and branch prediction are enabled.

    .thumb_func
IDelayLoop2:

    sub    r0,r0, #2          @@ 1 cycle
    bgt    IDelayLoop2        @@ 1 cycle
                              @@ Effectively 4 additional cycles here due to branch prediction failure on the last loop
    mov    pc, lr             @@ 5 cycles

    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
