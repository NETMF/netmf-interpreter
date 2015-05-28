@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .global Prot_Bytes

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


    BackDoorK0      =     0xaa
    BackDoorK1      =     0x55
    BackDoorK2      =     0x11
    BackDoorK3      =     0x88
    BackDoorK4      =     0xFF
    BackDoorK5      =     0xFF
    BackDoorK6      =     0xFF
    BackDoorK7      =     0xFF

    nFPROT0         =     0x00
    FPROT0          =     nFPROT0 ^ 0xFF

    nFPROT1         =     0x00
    FPROT1          =     nFPROT1 ^ 0xFF

    nFPROT2         =     0x00
    FPROT2          =     nFPROT2 ^ 0xFF

    nFPROT3         =     0x00
    FPROT3          =     nFPROT3 ^ 0xFF

    nFDPROT         =     0x00
    FDPROT          =     nFDPROT ^ 0xFF

    nFEPROT         =     0x00
    FEPROT          =     nFEPROT ^ 0xFF

    FOPT            =     0xFF
    FSEC            =     0xFE

    .section ProtectionArea, "xa", %progbits
    .align
Prot_Bytes:
    .byte     BackDoorK0
    .byte     BackDoorK1 
    .byte     BackDoorK2
    .byte     BackDoorK3
    .byte     BackDoorK4
    .byte     BackDoorK5
    .byte     BackDoorK6
    .byte     BackDoorK7
    .byte     FPROT0
    .byte     FPROT1
    .byte     FPROT2
    .byte     FPROT3
    .byte     FSEC
    .byte     FOPT
    .byte     FEPROT
    .byte     FDPROT
            
    .end
