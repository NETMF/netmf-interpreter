## STM32 Cube embedded software libraries ##
This folder is a placeholder for the SMT32 Cube embedded software libraries source code. STM32 Cube embedded software libraries include: 
- The HAL hardware abstraction layer, enabling portability between different STM32 devices via standardized API calls
- A collection of Middleware components, like RTOS, USB library, file system, TCP/IP stack, Touch sensing library or Graphic Library (depending on the MCU series)

The SMTCube is available from [ST web site](http://www.st.com/web/en/catalog/tools/FM146/CL2167/SC2004)

There are software packages for each STM32 device familly. Each one should go into its own folder. E.g. F0 folder for STM32 F0 series, F4 for STM32 F4 series, etc. The build system will look the code and header files there.

### Download and install ###
To download and install the SMT32Cube source use the respective Powershell command available at the root folder. E.g. `Install-STM32Cube_F4.ps1` for F4 familly.

### Supported and tested versions ###
**_This code base was validated and tested with the following versions:_**

- F0: not implmented/ not tested
- F1: not implmented/ not tested
- F2: not implmented/ not tested
- F3: not implmented/ not tested
- F4: v1.11.0
- F7: not implmented/ not tested
- L0: not implmented/ not tested
- L1: not implmented/ not tested
- L4: not implmented/ not tested
