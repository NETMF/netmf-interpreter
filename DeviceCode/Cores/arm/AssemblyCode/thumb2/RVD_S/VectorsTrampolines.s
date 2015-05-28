;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;*****************************************************************************

    EXPORT  ARM_Vectors

;*****************************************************************************

    IMPORT    StackTop
    IMPORT    EntryPoint
    IMPORT    HARD_Breakpoint
    IMPORT    NMI_Handler

    AREA    RESET, DATA, READONLY
ARM_Vectors
    DCD     StackTop 		; Top of Stack
    DCD     EntryPoint  	; Reset Handler
    DCD     NMI_Handler  	; NMI Handler
    DCD     HARD_Breakpoint ; Hard Fault Handler

    DCD     0  ; Bus Fault Handler
    DCD     0  ; Usage Fault Handler
    DCD     0  ; Reserved
    DCD     0  ; Reserved
    DCD     0  ; Reserved
    DCD     0  ; Reserved
    DCD     0  ; SVCall Handler
    DCD     0  ; Debug Monitor Handler
    DCD     0  ; Reserved
    DCD     0  ; PendSV Handler
    DCD     0  ; SysTick Handler

    ; External Interrupts
    DCD     0  ; DMA Channel 0 Transfer Complete
    DCD     0  ; DMA Channel 1 Transfer Complete
    DCD     0  ; DMA Channel 2 Transfer Complete
    DCD     0  ; DMA Channel 3 Transfer Complete
    DCD     0  ; DMA Channel 4 Transfer Complete
    DCD     0  ; DMA Channel 5 Transfer Complete
    DCD     0  ; DMA Channel 6 Transfer Complete
    DCD     0  ; DMA Channel 7 Transfer Complete
    DCD     0  ; DMA Channel 8 Transfer Complete
    DCD     0  ; DMA Channel 9 Transfer Complete
    DCD     0  ; DMA Channel 10 Transfer Complete
    DCD     0  ; DMA Channel 11 Transfer Complete
    DCD     0  ; DMA Channel 12 Transfer Complete
    DCD     0  ; DMA Channel 13 Transfer Complete
    DCD     0  ; DMA Channel 14 Transfer Complete
    DCD     0  ; DMA Channel 15 Transfer Complete
    DCD     0  ; DMA Error Interrupt
    DCD     0  ; Normal Interrupt
    DCD     0  ; FTFL Interrupt
    DCD     0  ; Read Collision Interrupt
    DCD     0  ; Low Voltage Detect, Low Voltage Warning
    DCD     0  ; Low Leakage Wakeup
    DCD     0  ; WDOG Interrupt
    DCD     0  ; RNGB Interrupt
    DCD     0  ; I2C0 interrupt
    DCD     0  ; I2C1 interrupt
    DCD     0  ; SPI0 Interrupt
    DCD     0  ; SPI1 Interrupt
    DCD     0  ; SPI2 Interrupt
    DCD     0  ; CAN0 OR'd Message Buffers Interrupt
    DCD     0  ; CAN0 Bus Off Interrupt
    DCD     0  ; CAN0 Error Interrupt
    DCD     0  ; CAN0 Tx Warning Interrupt
    DCD     0  ; CAN0 Rx Warning Interrupt
    DCD     0  ; CAN0 Wake Up Interrupt
    DCD     0  ; Reserved interrupt 51
    DCD     0  ; Reserved interrupt 52
    DCD     0  ; CAN1 OR'd Message Buffers Interrupt
    DCD     0  ; CAN1 Bus Off Interrupt
    DCD     0  ; CAN1 Error Interrupt
    DCD     0  ; CAN1 Tx Warning Interrupt
    DCD     0  ; CAN1 Rx Warning Interrupt
    DCD     0  ; CAN1 Wake Up Interrupt
    DCD     0  ; Reserved interrupt 59
    DCD     0  ; Reserved interrupt 60
    DCD     0  ; UART0 Receive/Transmit interrupt
    DCD     0  ; UART0 Error interrupt
    DCD     0  ; UART1 Receive/Transmit interrupt
    DCD     0  ; UART1 Error interrupt
    DCD     0  ; UART2 Receive/Transmit interrupt
    DCD     0  ; UART2 Error interrupt
    DCD     0  ; UART3 Receive/Transmit interrupt
    DCD     0  ; UART3 Error interrupt
    DCD     0  ; UART4 Receive/Transmit interrupt
    DCD     0  ; UART4 Error interrupt
    DCD     0  ; UART5 Receive/Transmit interrupt
    DCD     0  ; UART5 Error interrupt
    DCD     0  ; ADC0 interrupt
    DCD     0  ; ADC1 interrupt
    DCD     0  ; CMP0 interrupt
    DCD     0  ; CMP1 interrupt
    DCD     0  ; CMP2 interrupt
    DCD     0  ; FTM0 fault, overflow and channels interrupt
    DCD     0  ; FTM1 fault, overflow and channels interrupt
    DCD     0  ; FTM2 fault, overflow and channels interrupt
    DCD     0  ; CMT interrupt
    DCD     0  ; RTC interrupt
    DCD     0  ; Reserved interrupt 83
    DCD     0  ; PIT timer channel 0 interrupt
    DCD     0  ; PIT timer channel 1 interrupt
    DCD     0  ; PIT timer channel 2 interrupt
    DCD     0  ; PIT timer channel 3 interrupt
    DCD     0  ; PDB0 Interrupt
    DCD     0  ; USB0 interrupt
    DCD     0  ; USBDCD Interrupt
    DCD     0  ; Ethernet MAC IEEE 1588 Timer Interrupt
    DCD     0  ; Ethernet MAC Transmit Interrupt
    DCD     0  ; Ethernet MAC Receive Interrupt
    DCD     0  ; Ethernet MAC Error and miscelaneous Interrupt
    DCD     0  ; I2S0 Interrupt
    DCD     0  ; SDHC Interrupt
    DCD     0  ; DAC0 interrupt
    DCD     0  ; DAC1 interrupt
    DCD     0  ; TSI0 Interrupt
    DCD     0  ; MCG Interrupt
    DCD     0  ; LPTimer interrupt
    DCD     0  ; Reserved interrupt 102
    DCD     0  ; Port A interrupt
    DCD     0  ; Port B interrupt
    DCD     0  ; Port C interrupt
    DCD     0  ; Port D interrupt
    DCD     0  ; Port E interrupt
    DCD     0  ; Reserved interrupt 108
    DCD     0  ; Reserved interrupt 109
    DCD     0  ; Reserved interrupt 110
    DCD     0  ; Reserved interrupt 111
    DCD     0  ; Reserved interrupt 112
    DCD     0  ; Reserved interrupt 113
    DCD     0  ; Reserved interrupt 114
    DCD     0  ; Reserved interrupt 115
    DCD     0  ; Reserved interrupt 116
    DCD     0  ; Reserved interrupt 117
    DCD     0  ; Reserved interrupt 118
    DCD     0  ; Reserved interrupt 119
    END
