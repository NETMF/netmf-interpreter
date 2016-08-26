# Constraints of the original design of the runtime
The .NET Micro Framework was originally developed pretty much as a means to developing
a product, not as a platform in and of itself. As a consequence, most of the design
decisions were forced by the limited resources present in the product. Ironically,
very close to shipping the product, external events altered the set of resources,
too late for a redesign. The runtime still carries the shortcomings of having to
target a product different from what ultimately shipped and carried that into shipping
the runtime as a platform.

The original product was supposed to have 512KB of ROM and 32KB of serial EEPROM.
This meant that fixing any problem in the code couldn’t be done in place. Every
single function call had to go through an explicit indirection table because:
1. The HAL was written in C, which meant no classes and thus no virtual tables.
2. The compiler had to generate code compatible across position-dependent and  
position-independent blocks, leading to virtual tables based on relative offsets,  
not actual method pointers.
3. The other big limiting factor was the need to store in RAM read-only data structures
used to cross-reference the assemblies in the type system. Because the RAM was a scarce 
resource, the data structures had to be pared down and the lost information had to be
rebuilt at runtime. This led to lots of special casing, which translated into insufficient
code coverage that shows up even today as bugs under new configurations.

Additional constraints based on the micro-controller space at the time included:
1. Limited or no C++ compiler language support
    1. Little or no support for exceptions
    2. Standard C++ Runtime Libraries not optimized for embedded use increased bloat
2. No common cross architecture or cross tool startup code
    1. There was no standard BIOS/UEFI/CMSIS or similar type of basic startup.
    everything was "bare-metal".
 


