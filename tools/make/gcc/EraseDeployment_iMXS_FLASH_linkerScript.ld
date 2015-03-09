/* EraseDeployment_iMXS_FLASH_linkerScript.ld
 * gnu linker script for FLASH version of EraseDeployment */

MEMORY
{
    LR_FLASH  : ORIGIN = 0x10300000, LENGTH = 1M
    HEAP      : ORIGIN = 0x08100000, LENGTH = 4M
    STACK     : ORIGIN = 0x08500000, LENGTH = 8K
    RELOC_RAM : ORIGIN = 0x00000000, LENGTH = 1M
}

ENTRY(EntryPoint)

SECTIONS
{
    ER_BOOT :
    {
        *(i.EntryPoint)
        *(SectionForBootstrapOperations)
    } >LR_FLASH
    ER_RAM_RO :
    {
        KEEP(*(.text.VectorsTrampolines))
        *(.text*)
        *(SectionForFlashOperations)
        *(i.*)
        *(.rodata*)
        *(.constdata*)
        *(.data*)
        *(SectionForConfig)

        /* libc thinks that it needs these routines, but they are never called */
        . = ALIGN(4);
        PROVIDE(_sbrk = .);
        PROVIDE(_write = .);
        PROVIDE(_close = .);
        PROVIDE(_fstat = .);
        PROVIDE(_lseek = .);
        PROVIDE(_read = .);
        PROVIDE(abort = .);
        PROVIDE(__errno = .);
        PROVIDE(isatty = .);
        LONG(0xE12FFF1E);       /* This is a "bx lr" instruction, but it will never be executed */
    } >RELOC_RAM AT>LR_FLASH
    .bss :
    {
        *(.bss*)

        /* libgcc thinks it needs this table, but exceptions are disabled, so it doesn't */
        PROVIDE(__exidx_start = .);
        PROVIDE(__exidx_end = .);
    } >RELOC_RAM
    /DISCARD/ :
    {
        *(.glue*)
        *(.vfp11*)
        *(.ARM.exidx*)
        *(.ARM.extab*)
    }
    Load$$ER_FLASH$$Base         = ADDR(ER_BOOT);
    Image$$ER_FLASH$$Length      = SIZEOF(ER_BOOT);
    Image$$ER_RAM_RO$$Base       = ADDR(ER_RAM_RO);
    Image$$ER_RAM_RO$$Length     = SIZEOF(ER_RAM_RO);
    Load$$ER_RAM_RO$$Base        = LOADADDR(ER_RAM_RO);
    Image$$ER_RAM_RW$$Base       = ADDR(.bss);
    Image$$ER_RAM_RW$$Length     = SIZEOF(.bss);
    Load$$ER_RAM_RW$$Base        = LOADADDR(ER_RAM_RO) + SIZEOF(ER_RAM_RO);
    Image$$ER_RAM_RW$$ZI$$Base   = ADDR(.bss);
    Image$$ER_RAM_RW$$ZI$$Length = SIZEOF(.bss);
    ER_HEAP_BEGIN ORIGIN(HEAP) :
    {
        *(SectionForHeapBegin)
    }
    ER_HEAP_END ORIGIN(HEAP) + LENGTH(HEAP) - 4 :
    {
        *(SectionForHeapEnd)
    }
    ER_STACK_BOTTOM ORIGIN(STACK) :
    {
        *(SectionForStackBottom)
    }
    ER_STACK_TOP ORIGIN(STACK) + LENGTH(STACK) :
    {
        *(SectionForStackTop)
    }
    /* This makes RealView generated code happy */
    __use_no_semihosting_swi = 0;
}
