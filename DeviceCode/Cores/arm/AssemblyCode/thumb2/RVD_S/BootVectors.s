;*****************************************************************************

    EXPORT  Boot_Vectors

;*****************************************************************************

    PRESERVE8
    THUMB

    IMPORT    StackTop
    IMPORT    EntryPoint
    IMPORT    HARD_Breakpoint
    IMPORT    NMI_Handler

    AREA    BOOT, DATA, READONLY

Boot_Vectors
    DCD     StackTop             ; Top of Stack
    DCD     EntryPoint           ; Reset Handler
    DCD     NMI_Handler          ; NMI Handler
    DCD     HARD_Breakpoint      ; Hard Fault Handler

    ; remaming vectors
    SPACE   460

END

