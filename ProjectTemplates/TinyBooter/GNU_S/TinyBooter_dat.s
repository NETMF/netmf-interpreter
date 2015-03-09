
    .section    .text, "xa", %progbits

    .extern     EntryPoint

    .global     TinyBooterCompressed_Dat

TinyBooterCompressed_Dat:
    .incbin     "TinyBooter_Compressed.dat"
TinyBooterCompressed_Dat_End:

    .end

