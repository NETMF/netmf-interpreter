# Copyright AllSeen Alliance. All rights reserved.
#
#    Permission to use, copy, modify, and/or distribute this software for any
#    purpose with or without fee is hereby granted, provided that the above
#    copyright notice and this permission notice appear in all copies.
#
#    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
#    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
#    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
#    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
#    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
#    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
#    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

import os
import shutil
import platform

if platform.system() == 'Linux':
    default_target = 'linux'
    default_msvc_version = None
elif platform.system() == 'Windows':
    default_target = 'win32'
    default_msvc_version = '11.0'
elif platform.system() == 'Darwin':
    default_target = 'darwin'
    default_msvc_version = None

vars = Variables()

# Common build variables
vars.Add(EnumVariable('TARG', 'Target platform variant', default_target, allowed_values=('win32', 'linux', 'arduino', 'bsp', 'darwin')))
vars.Add(EnumVariable('VARIANT', 'Build variant', 'debug', allowed_values=('debug', 'release')))
vars.Add(PathVariable('GTEST_DIR', 'The path to googletest sources', os.environ.get('GTEST_DIR'), PathVariable.PathIsDir))
vars.Add(EnumVariable('WS', 'Whitespace Policy Checker', 'check', allowed_values=('check', 'detail', 'fix', 'off')))
vars.Add(EnumVariable('FORCE32', 'Force building 32 bit on 64 bit architecture', 'false', allowed_values=('false', 'true')))
vars.Add(EnumVariable('NO_AUTH', 'Compile in authentication mechanism\'s to the code base', 'no', allowed_values=('no', 'yes')))
vars.Add(EnumVariable('AJWSL', 'Compile driver for the QCA4004 for a specific platform', 'off', allowed_values=('due', 'stm32', 'frdm', 'off')))
vars.Add(PathVariable('ATMEL_DIR', 'Directory for ATMEL source code', os.environ.get('ATMEL_DIR'), PathVariable.PathIsDir))
vars.Add(PathVariable('FREE_RTOS_DIR','Directory to FreeRTOS source code', os.environ.get('FREE_RTOS_DIR'), PathVariable.PathIsDir))
vars.Add(PathVariable('ARM_TOOLCHAIN_DIR', 'Path to the GNU ARM toolchain bin folder', os.environ.get('ARM_TOOLCHAIN_DIR'), PathVariable.PathIsDir))
vars.Add(PathVariable('STM_SRC_DIR', 'Path to the source code for the STM32 microcontroller', os.environ.get('STM_SRC_DIR'), PathVariable.PathIsDir))
vars.Add(PathVariable('MBED_DIR', 'Path to the mbed source code repository', os.environ.get('MBED_DIR'), PathVariable))
vars.Add(EnumVariable('RTOS', 'RTOS your using', 'mbed', allowed_values=('mbed', 'freeRTOS')))

if default_msvc_version:
    vars.Add(EnumVariable('MSVC_VERSION', 'MSVC compiler version - Windows', default_msvc_version, allowed_values=('8.0', '9.0', '10.0', '11.0', '11.0Exp', '12.0', '12.0Exp')))

if ARGUMENTS.get('TARG', default_target) in ['linux', 'win32']:
    vars.Add(EnumVariable('ARDP', 'Enable support for ARDP?', 'off', allowed_values=('on', 'off')))

if ARGUMENTS.get('TARG', default_target) == 'win32':
    msvc_version = ARGUMENTS.get('MSVC_VERSION')
    env = Environment(variables = vars, MSVC_VERSION=msvc_version, TARGET_ARCH='x86')
else:
    env = Environment(variables = vars)
Help(vars.GenerateHelpText(env))

# Allows specification of preprocessor defines on the command line
#
# DEFINE="FOO=1" DEFINE="BAR=2"
#
cppdefines = []
for key, value in ARGLIST:
   if key == 'define':
       cppdefines.append(value)

env.Append(CPPDEFINES=cppdefines)

# Define if compiling to use authentication
if env['NO_AUTH'] == 'no':
    auth = ''
else:
    auth = 'NO_SECURITY'

# Set AJ_DEBUG_RESTRICT level
restrict = ARGUMENTS.get('DEBUG_RESTRICT', '')
if restrict != '':
    if restrict in ['0', '1', '2', '3', '4', '5', 'AJ_DEBUG_OFF', 'AJ_DEBUG_ERROR', 'AJ_DEBUG_WARN', 'AJ_DEBUG_INFO', 'AJ_DEBUG_DUMP', 'AJ_DEBUG_ALL']:
        env.Append(CPPDEFINES=['AJ_DEBUG_RESTRICT' + '=' + restrict])
    else:
        print 'Invalid value for DEBUG_RESTRICT'
        Exit(0)


# Define compile/link options only for win32/linux.
# In case of target platforms, the compilation/linking does not take place
# using SCons files.
if env['TARG'] == 'win32':
    env['libs'] = ['wsock32', 'advapi32']
    env.Append(CFLAGS=['/J', '/W3'])
    env.Append(CPPDEFINES=['_CRT_SECURE_NO_WARNINGS'])
    env.Append(LINKFLAGS=['/NODEFAULTLIB:libcmt.lib'])
    
    if env['ARDP'] == 'on':
        env.Append(CPPDEFINES=['AJ_ARDP'])
    
    if env['VARIANT'] == 'debug':
        # With a modern Microsoft compiler it is typical to use a pdb file i.e. the /Zi
        # or/ZI CCPDBFLAGS.  However in SCons a pdb file is created for each .obj file.
        # To be able to use the debug information we would have to copy all of the
        # pdb files (one for each C++ file) into the dist. SCons documentation recommends
        # using the /Z7 option to solve this problem.  Since another more acceptable
        # solution has not yet been found we are going with the recommendation from the
        # SCons documentation.
        env['CCPDBFLAGS'] = '/Z7'
        env.Append(CFLAGS=['/MD', '/Od'])
        env.Append(LINKFLAGS=['/debug'])
    else:
        env.Append(CPPDEFINES = ['NDEBUG'])
        env.Append(CFLAGS=['/MD', '/Gy', '/O1', '/GF'])
        env.Append(LINKFLAGS=['/opt:ref'])
    # Include paths
    env['includes'] = [ os.getcwd() + '/inc', os.getcwd() + '/target/${TARG}']
    # Target-specific headers and sources
    env['aj_targ_headers'] = [Glob('target/' + env['TARG'] + '/*.h')]
    env['aj_targ_srcs'] = [Glob('target/' + env['TARG'] + '/*.c')]
elif env['TARG'] in [ 'linux' ]:
    if env['ARDP'] == 'on':
        env.Append(CPPDEFINES=['AJ_ARDP'])

    if os.environ.has_key('CROSS_PREFIX'):
        env.Replace(CC = os.environ['CROSS_PREFIX'] + 'gcc')
        env.Replace(CXX = os.environ['CROSS_PREFIX'] + 'g++')
        env.Replace(LINK = os.environ['CROSS_PREFIX'] + 'gcc')
        env.Replace(AR = os.environ['CROSS_PREFIX'] + 'ar')
        env['ENV']['STAGING_DIR'] = os.environ.get('STAGING_DIR', '')

    if os.environ.has_key('CROSS_PATH'):
        env['ENV']['PATH'] = ':'.join([ os.environ['CROSS_PATH'], env['ENV']['PATH'] ] )

    if os.environ.has_key('CROSS_CFLAGS'):
        env.Append(CFLAGS=os.environ['CROSS_CFLAGS'].split())

    if os.environ.has_key('CROSS_LINKFLAGS'):
        env.Append(LINKFLAGS=os.environ['CROSS_LINKFLAGS'].split())

    env['libs'] = ['rt', 'crypto', 'pthread']
    env.Append(CFLAGS=['-Wall',
                       '-pipe',
                       '-static',
                       '-funsigned-char',
                       '-Wpointer-sign',
                       '-Wimplicit-function-declaration',
                       '-fno-strict-aliasing'])
    if env['VARIANT'] == 'debug':
        env.Append(CFLAGS='-g')
    else:
        env.Append(CPPDEFINES=['NDEBUG'])
        env.Append(CFLAGS='-Os')
        env.Append(LINKFLAGS='-s')

    if env['FORCE32'] == 'true':
        env.Append(CFLAGS='-m32')
        env.Append(LINKFLAGS='-m32')
    # Include paths
    env['includes'] = [ os.getcwd() + '/inc', os.getcwd() + '/target/${TARG}']
    # Target-specific headers and sources
    env['aj_targ_headers'] = [Glob('target/' + env['TARG'] + '/*.h')]
    env['aj_targ_srcs'] = [Glob('target/' + env['TARG'] + '/*.c')]

elif env['TARG'] == 'arduino':
    # Include paths
    env['includes'] = [ os.getcwd() + '/inc', os.getcwd() + '/target/${TARG}']

    # Target-specific headers and sources
    env['aj_targ_headers'] = [Glob('target/' + env['TARG'] + '/*.h')]
    env['aj_targ_srcs'] = [Glob('target/' + env['TARG'] + '/*.c')]

# Board support package target
elif env['TARG'] == 'bsp':
    print "You are building for the bsp target"
    if env['AJWSL'] == 'off':
        print "AJWSL must be set to a platform to build for the bsp target"
        Exit(2)

    # Set the compiler
    env.Replace(CC = env.File('${ARM_TOOLCHAIN_DIR}/arm-none-eabi-gcc'))
    env.Replace(CXX = env.File('${ARM_TOOLCHAIN_DIR}/arm-none-eabi-g++'))
    env.Replace(LINK = env.File('${ARM_TOOLCHAIN_DIR}/arm-none-eabi-gcc'))
    env.Replace(AR = env.File('${ARM_TOOLCHAIN_DIR}/arm-none-eabi-ar'))


    env['CPPDEFPREFIX']     = '-D'
    env['OBJSUFFIX']        = '.o'
    env['SHOBJSUFFIX']      = '.os'
    env['CCCOM']            = '$CC -o $TARGET -c $CFLAGS $CCFLAGS $_CCCOMCOM $SOURCES'
    env['CXXCOM']           = '$CXX $CFLAGS $CCFLAGS $CXXFLAGS -std=gnu++98 $_CCCOMCOM -o $TARGET $SOURCES'
    env['INCPREFIX']        = '-I'
    env['CCFLAGS']          = ''
    env['CPPFLAGS']         = ''
    env['PROGSUFFIX']       = '.elf'
    env['PROGPREFIX']       = ''
    env['LDMODULEPREFIX']   = ''
    env['LDMODULESUFFIX']   = ''
    env['LIBLINKPREFIX']    = ''
    env['LIBPREFIX']        = ''
    env['PRELINKFLAGS']     = ''
    env['OBJPREFIX']        = ''
    env['LINKSCRIPT']       = ''
    env['LIBPATHS']         = ''
    env['LINKFLAGS']        = '-Xlinker -Map -Xlinker '
    env['LINKCOM']          = '$LINK $PRELINKFLAGS $LINKSCRIPT $LIBPATHS -o $TARGET $__RPATH $SOURCES $_LIBDIRFLAGS $_LIBFLAGS $LINKFLAGS'
    env['LINK']             = '$CC'
    env['ASFLAGS']          = ''
    env['ASPPCOM']          = '$AS'
    env['ARFLAGS']          = 'rc'
    env['ARCOM']            = '$AR $ARFLAGS $TARGET $SOURCES'
    env['LIBDIRPREFIX']     = ''
    env['ASCOM']            = '$CC -o $TARGET -c $CFLAGS $CCFLAGS $_CCCOMCOM $SOURCES'
    env['LIBPREFIX'] = 'lib'
    env['LIBSUFFIX'] = '.a'
    env['RANLIB'] = 'ranlib'
    env['RANLIBFLAGS'] =    ''
    env['RANLIBCOM'] = '$RANLIB $RANLIBFLAGS $TARGET'
    env['SHLIBPREFIX'] = ''
    # This was done because scons creates a link file to feed into the linker
    # and arm-none-eabi removes '\' when interpreting a linker file. This
    # prevents scons from creating a link file and just feeding the command line
    # options directly to the compiler/linker
    env['MAXLINELENGTH'] = 10000

    # Debug/Release Variants
    if env['VARIANT'] == 'debug':
        env.Append(CFLAGS=['-g3'])
        env.Append(CFLAGS=['-ggdb'])
        env.Append(CFLAGS=['-O0'])
    else:
        env.Append(CPPDEFINES=['NDEBUG'])
        env.Append(CFLAGS=['-Os'])
        env.Append(LINKFLAGS=['-s'])

    if env['AJWSL'] != 'frdm':
        # Set the compiler flags
        env['CFLAGS'] = ['-mthumb', '-fdata-sections', '-ffunction-sections', '-mlong-calls',
                        '-g3', '-Wall', '-mcpu=cortex-m3', '-c', '-pipe', '-fno-strict-aliasing',
                        '-Wmissing-prototypes', '-Wpointer-arith', '-std=gnu99', '-Wchar-subscripts',
                        '-Wcomment', '-Wformat=2', '-Wimplicit-int', '-Wmain', '-Wparentheses',
                        '-Wsequence-point', '-Wreturn-type', '-Wswitch', '-Wtrigraphs', '-Wunused',
                        '-Wuninitialized', '-Wfloat-equal', '-Wundef', '-Wshadow', '-Wbad-function-cast',
                        '-Wwrite-strings', '-Wsign-compare', '-Waggregate-return', '-Wmissing-declarations',
                        '-Wformat', '-Wmissing-format-attribute', '-Wno-deprecated-declarations',
                        '-Wpacked', '-Wlong-long', '-Wunreachable-code', '-Wcast-align', '-MD', '-MP']

        # Add platform independent source files
        rtos_src = [Glob('RTOS/*.c') + Glob('RTOS/FreeRTOS/*.c') + Glob(env['FREE_RTOS_DIR'] + '/Source/*.c') +
                    [env['FREE_RTOS_DIR'] + '/Source/portable/GCC/ARM_CM3/port.c']]

    if env['AJWSL'] == 'due':
        rtos_src += [env['FREE_RTOS_DIR'] + '/Source/portable/MemMang/heap_4.c']

        # Add platform dependent sources
        due_src = [Glob('bsp/due/*.c') + [env['ATMEL_DIR'] + '/common/services/clock/sam3x/sysclk.c',
                                          env['ATMEL_DIR'] + '/common/services/spi/sam_spi/spi_master.c',
                                          env['ATMEL_DIR'] + '/common/services/freertos/sam/freertos_peripheral_control.c',
                                          env['ATMEL_DIR'] + '/common/services/freertos/sam/freertos_usart_serial.c',
                                          env['ATMEL_DIR'] + '/common/utils/interrupt/interrupt_sam_nvic.c',
                                          env['ATMEL_DIR'] + '/common/utils/stdio/read.c',
                                          env['ATMEL_DIR'] + '/common/utils/stdio/write.c',
                                          env['ATMEL_DIR'] + '/common/drivers/nvm/sam/sam_nvm.c',
                                          env['ATMEL_DIR'] + '/sam/boards/arduino_due_x/init.c',
                                          env['ATMEL_DIR'] + '/sam/boards/arduino_due_x/led.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/pdc/pdc.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/pio/pio.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/pio/pio_handler.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/pmc/pmc.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/pmc/sleep.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/uart/uart.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/usart/usart.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/spi/spi.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/efc/efc.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/tc/tc.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/trng/trng.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/rstc/rstc.c',
                                          env['ATMEL_DIR'] + '/sam/utils/cmsis/sam3x/source/templates/exceptions.c',
                                          env['ATMEL_DIR'] + '/sam/utils/cmsis/sam3x/source/templates/system_sam3x.c',
                                          env['ATMEL_DIR'] + '/sam/utils/cmsis/sam3x/source/templates/gcc/startup_sam3x.c',
                                          env['ATMEL_DIR'] + '/sam/services/flash_efc/flash_efc.c',
                                          env['ATMEL_DIR'] + '/sam/utils/syscalls/gcc/syscalls.c',
                                          env['ATMEL_DIR'] + '/sam/drivers/dmac/dmac.c']]

        # Add platform dependent linker flags
        env['LINKFLAGS'] = ['-mthumb', '-Wl,--start-group', '-larm_cortexM3l_math', '-lm',
                            '-Wl,--end-group', '-L"' + env['ATMEL_DIR'] + '/thirdparty/CMSIS/Lib/GCC"', '-Wl,--gc-sections', '-Wl,-Map,${TARGET.base}.map',
                            '-mcpu=cortex-m3', '-Wl,--entry=Reset_Handler', '-T' + env['ATMEL_DIR'] + '/sam/utils/linker_scripts/sam3x/sam3x8/gcc/flash.ld']
        # Add platform dependent defines
        env.Append(CPPDEFINES = ['__SAM3X8E__', 'ARM_MATH_CM3=true', 'BOARD=ARDUINO_DUE_X', 'printf=iprintf', 'AJ_HEAP4'])

        # Add platform dependent include paths
        env['CPPPATH'] = [os.getcwd() + '/bsp', os.getcwd() + '/bsp/due', os.getcwd() + '/bsp/due/config',           env['FREE_RTOS_DIR'] + '/Source/include', os.getcwd() + '/RTOS/FreeRTOS',
                          env['FREE_RTOS_DIR'] + '/Source/portable/GCC/ARM_CM3',  env['ATMEL_DIR'] + '/common/boards',
                          env['ATMEL_DIR'] + '/common/services/clock',          env['ATMEL_DIR'] + '/common/services/clock/sam3x',
                          env['ATMEL_DIR'] + '/common/services/gpio',           env['ATMEL_DIR'] + '/common/services/ioport',
                          env['ATMEL_DIR'] + '/common/services/freertos/sam',   env['ATMEL_DIR'] + '/common/services/serial/sam_uart',
                          env['ATMEL_DIR'] + '/common/services/serial',         env['ATMEL_DIR'] + '/common/services/spi',
                          env['ATMEL_DIR'] + '/common/services/sam_spi',        env['ATMEL_DIR'] + '/common/services/spi/sam_spi/module_config',
                          env['ATMEL_DIR'] + '/common/utils',                   env['ATMEL_DIR'] + '/common/utils/stdio/stdio_serial',
                          env['ATMEL_DIR'] + '/common/drivers/nvm',             env['ATMEL_DIR'] + '/common/nvm/sam/module_config',
                          env['ATMEL_DIR'] + '/sam/boards',                     env['ATMEL_DIR'] + '/sam/boards/arduino_due_x',
                          env['ATMEL_DIR'] + '/sam/drivers/pio',                env['ATMEL_DIR'] + '/sam/drivers/pmc',
                          env['ATMEL_DIR'] + '/sam/drivers/tc',                 env['ATMEL_DIR'] + '/sam/drivers/trng',
                          env['ATMEL_DIR'] + '/sam/drivers/pdc',                env['ATMEL_DIR'] + '/sam/drivers/uart',
                          env['ATMEL_DIR'] + '/sam/drivers/usart',              env['ATMEL_DIR'] + '/sam/drivers/spi',
                          env['ATMEL_DIR'] + '/sam/drivers/efc',                env['ATMEL_DIR'] + '/sam/utils',
                          env['ATMEL_DIR'] + '/sam/utils/cmsis/sam3x/include',  env['ATMEL_DIR'] + '/sam/utils/cmsis/sam3x/source/templates',
                          env['ATMEL_DIR'] + '/sam/utils/cmsis/sam3x/include/component', env['ATMEL_DIR'] + '/sam/utils/header_files',
                          env['ATMEL_DIR'] + '/sam/utils/preprocessor',         env['ATMEL_DIR'] + '/sam/services/flash_efc',
                          env['ATMEL_DIR'] + '/thirdparty/CMSIS/Include',       env['ATMEL_DIR'] + '/thirdparty/CMSIS/Lib/GCC',
                          env['ATMEL_DIR'] + '/sam/boards/arduino_due_x/board_config', env['ATMEL_DIR'] + '/config',
                          env['ATMEL_DIR'] + '/common/services/clock/sam3x/module_config', env['ATMEL_DIR'] + '/common/services/clock/sam3x',
                          env['ATMEL_DIR'] + '/sam/drivers/dmac', env['ATMEL_DIR'] + '/sam/drivers/rstc',
                          os.getcwd() + '/RTOS', os.getcwd() + '/crypto', os.getcwd() + '/crypto/ecc', os.getcwd() + '/external/sha2', os.getcwd() + '/malloc', os.getcwd() + '/inc', os.getcwd() + '/WSL']
    elif env['AJWSL'] == 'stm32':
        env['LINKCOM']          = '$LINK -o $TARGET $LINKFLAGS $__RPATH $SOURCES $_LIBDIRFLAGS $_LIBFLAGS'
        rtos_src += [env['FREE_RTOS_DIR'] + '/Source/portable/MemMang/heap_4.c']

        # Add platform dependent sources
        stm_src = [env['STM_SRC_DIR'] + 'Libraries/CMSIS/ST/STM32F4xx/Source/Templates/gcc_ride7/startup_stm32f4xx.s',
                   env['STM_SRC_DIR'] + 'Libraries/CMSIS/ST/STM32F4xx/Source/Templates/system_stm32f4xx.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/misc.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/stm32f4xx_rcc.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/stm32f4xx_usart.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/stm32f4xx_gpio.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/stm32f4xx_dma.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/stm32f4xx_wwdg.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/stm32f4xx_spi.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/stm32f4xx_flash.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/stm32f4xx_rng.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/stm32f4xx_exti.c',
                   env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/src/stm32f4xx_syscfg.c',
                   'bsp/stm32/aj_target_platform.c',
                   'bsp/stm32/aj_spi.c',
                   'bsp/stm32/syscalls.c'
                   ]

        # Add platform dependent linker flags
        env['LINKFLAGS'] = ['-mthumb', '-Wl,--start-group', '-lm', '-lc',
                            '-Wl,--end-group', '-Wl,--gc-sections', '-Wl,-Map,${TARGET.base}.map',
                            '-mcpu=cortex-m3', '-T' + env['STM_SRC_DIR'] + 'Project/Peripheral_Examples/SysTick/TrueSTUDIO/SysTick/stm32_flash.ld',
                            '-Wl,--entry=Reset_Handler']

        # Add platform dependent defines
        env.Append(CPPDEFINES = ['STM32F407xx', 'USE_STDPERIPH_DRIVER','HAL_UART_MODULE_ENABLED', 'HAL_RCC_MODULE_ENABLED',
                                 'HAL_GPIO_MODULE_ENABLED', 'HAL_USART_MODULE_ENABLED', 'HAL_FLASH_MODULE_ENABLED'])

        # Add platform dependent include paths
        env['CPPPATH'] = [os.getcwd() + '/bsp', os.getcwd() + '/bsp/stm32', env['FREE_RTOS_DIR'] + '/Source/include', os.getcwd() + '/RTOS/FreeRTOS',
                          os.getcwd() + '/bsp/stm32/config',
                          os.getcwd() + '/RTOS', os.getcwd() + '/crypto', os.getcwd() + '/crypto/ecc', os.getcwd() + '/external/sha2',
                          os.getcwd() + '/malloc', os.getcwd() + '/inc', os.getcwd() + '/WSL', env['FREE_RTOS_DIR'] + '/Source/portable/GCC/ARM_CM3',
                          env['STM_SRC_DIR'] + 'Utilities/STM32F4-Discovery',
                          env['STM_SRC_DIR'] + 'Libraries/CMSIS/ST/STM32F4xx/Include',
                          env['STM_SRC_DIR'] + 'Libraries/CMSIS/Include',
                          env['STM_SRC_DIR'] + 'Libraries/STM32F4xx_StdPeriph_Driver/inc']

    elif env['AJWSL'] == 'frdm':
        if env['RTOS'] == 'freeRTOS':
            headers = ['RTOS/aj_target.h', 'RTOS/RTOS.h', 'RTOS/freeRTOS/aj_target_rtos.h', 'bsp/frdm/aj_target_platform.h', 'bsp/frdm/aj_target_mbed.h']
            target = [Glob('bsp/frdm/*.c'), Glob('RTOS/*.c'), Glob('RTOS/FreeRTOS/*.c'), Glob('bsp/frdm/*.cpp')]
            env.Append(CPPPATH = [os.getcwd() + '/RTOS/FreeRTOS',
                                  os.getcwd() + '/RTOS'])
        elif env['RTOS'] == 'mbed':
            headers = ['RTOS/aj_target.h', 'RTOS/RTOS.h', 'RTOS/mbedRTOS/aj_target_rtos.h', 'bsp/frdm/aj_target_platform.h', 'bsp/frdm/aj_target_mbed.h']
            target = [Glob(env['MBED_DIR'] + '/mbed-rtos/rtx/TARGET_M3/TOOLCHAIN_GCC/*.s'),
                      Glob('bsp/frdm/*.c'),
                      Glob('RTOS/mbedRTOS/*.cpp'),
                      Glob('bsp/frdm/*.cpp'),
                      Glob(env['MBED_DIR'] + '/mbed-rtos/rtx/*.c'),
                      Glob(env['MBED_DIR'] + '/mbed-rtos/rtos/*.cpp'),
                      Glob(env['MBED_DIR'] + '/FATFileSystem/*.cpp'),
                      Glob(env['MBED_DIR'] + '/FATFileSystem/ChaN/*.cpp'),
                      Glob('RTOS/*.c'),
                      [env['MBED_DIR'] + '/SDFileSystem/SDFileSystem.cpp']]

            env.Append(CPPPATH = [env['MBED_DIR'] + '/mbed-rtos',
                                  env['MBED_DIR'] + '/mbed-rtos/rtx',
                                  env['MBED_DIR'] + '/mbed-rtos/rtx/TARGET_M4',
                                  env['MBED_DIR'] + '/mbed-rtos/rtx/TARGET_M4/TOOLCHAIN_GCC',
                                  env['MBED_DIR'] + '/mbed-rtos/rtos'])
        objects = [ env['MBED_DIR'] + '/mbed/TARGET_K64F/TOOLCHAIN_GCC_ARM/startup_MK64F12.o',
                    env['MBED_DIR'] + '/mbed/TARGET_K64F/TOOLCHAIN_GCC_ARM/cmsis_nvic.o',
                    env['MBED_DIR'] + '/mbed/TARGET_K64F/TOOLCHAIN_GCC_ARM/system_MK64F12.o',
                    env['MBED_DIR'] + '/mbed/TARGET_K64F/TOOLCHAIN_GCC_ARM/mbed_overrides.o',
                    env['MBED_DIR'] + '/mbed/TARGET_K64F/TOOLCHAIN_GCC_ARM/board.o',
                    env['MBED_DIR'] + '/mbed/TARGET_K64F/TOOLCHAIN_GCC_ARM/retarget.o']

        env['CCCOM'] = '$CC $CFLAGS $CCFLAGS $_CCCOMCOM -o $TARGET $SOURCES'

        if env['RTOS'] == 'freeRTOS':
            env.Append(CPPPATH  = [env['FREE_RTOS_DIR'] + '/Source/include', env['FREE_RTOS_DIR'] + '/Source/portable/GCC/ARM_CM3'])
        elif env['RTOS'] == 'mbed':
            env.Append(CPPPATH  = ['RTOS/mbedRTOS'])
        env.Append(CPPPATH      = [os.getcwd() + '/bsp' ,
                              os.getcwd() + '/bsp/frdm',
                              os.getcwd() + '/bsp/frdm/config',
                              os.getcwd() + '/malloc',
                              os.getcwd() + '/WSL',
                              os.getcwd() + '/RTOS',
                              os.getcwd() + '/inc',
                              os.getcwd() + '/external/sha2',
                              os.getcwd() + '/bsp/config',
                              env['MBED_DIR'] + '/SDFileSystem',
                              env['MBED_DIR'] + '/FATFileSystem',
                              env['MBED_DIR'] + '/FATFileSystem/ChaN',
                              env['MBED_DIR'] + '/mbed',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F_Freescale',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/can',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/edma',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/sdhc',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/pit',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/flextimer',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/smc',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/uart',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/pmc',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/mcg',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/port',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/wdog',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/adc',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/osc',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/enet',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/i2c',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/gpio',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/rtc',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/dmamux',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/sim',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/sai',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/hal/dspi',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/utilities',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/drivers',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/drivers/pit',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/drivers/clock',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/drivers/clock/src',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/drivers/enet',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/drivers/interrupt',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/common',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_KPSDK_CODE/common/phyksz8081',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_MCU_K64F',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_MCU_K64F/device',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_MCU_K64F/device/MK64F12',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TARGET_Freescale/TARGET_KPSDK_MCUS/TARGET_MCU_K64F/TARGET_FRDM',
                              env['MBED_DIR'] + '/mbed/TARGET_K64F/TOOLCHAIN_GCC_ARM'])

        env['CCFLAGS'] = ['-mcpu=cortex-m4',
                          '-mthumb',
                          '-mfpu=fpv4-sp-d16',
                          '-c',
                          '-g',
                          '-fno-common',
                          '-fmessage-length=0',
                          '-Wall',
                          '-fno-exceptions',
                          '-ffunction-sections',
                          '-fdata-sections']
        env['CXXFLAGS'] = ['-fno-rtti']
        env['PRELINKFLAGS'] = ['-mcpu=cortex-m4', '-mthumb', '-Wl,--gc-sections', '--specs=nano.specs', '-u', '_printf_float', '-u', '_scanf_float']
        env['LINKSCRIPT'] = ['-T' + env['MBED_DIR'] + '/mbed/TARGET_K64F/TOOLCHAIN_GCC_ARM/K64FN1M0xxx12.ld']
        env['LIBPATHS'] = ['-L' + env['MBED_DIR'] + '/mbed/TARGET_K64F/TOOLCHAIN_GCC_ARM']
        env['LINKFLAGS'] = ['-lstdc++',
                            '-lgcc',
                            '-lmbed',
                            '-lm',
                            '-lc',
                            '-Wl,-Map,${TARGET.base}.map']
        env.Append(CPPDEFINES = ['TARGET_K64F', 'TARGET_M4', 'TARGET_Freescale', 'TARGET_KPSDK_MCUS', 'TARGET_KPSDK_CODE',
                                 'TARGET_MCU_K64F', 'TARGET_FRDM', 'TOOLCHAIN_GCC_ARM', 'TOOLCHAIN_GCC', '__CORTEX_M4',
                                 'ARM_MATH_CM4', '__FPU_PRESENT=1', 'MBED_BUILD_TIMESTAMP=1411582835.22', '__MBED__=1',
                                 'CPU_MK64FN1M0VMD12', 'FSL_RTOS_MBED', 'TARGET_FF_ARDUINO', '__CORTEX_M4', 'FSL_RTOS_MBED'
                                 'AJ_CONFIGURE_WIFI_UPON_START',
                                 'WIFI_SSID=\\"SEAQUIC-AP3\\"',
                                 'WIFI_PASSPHRASE=\\"123456789\\"'
                                 ])

elif env['TARG'] in [ 'darwin' ]:
    if os.environ.has_key('CROSS_PREFIX'):
        env.Replace(CC = os.environ['CROSS_PREFIX'] + 'gcc')
        env.Replace(CXX = os.environ['CROSS_PREFIX'] + 'g++')
        env.Replace(LINK = os.environ['CROSS_PREFIX'] + 'gcc')
        env.Replace(AR = os.environ['CROSS_PREFIX'] + 'ar')
        env['ENV']['STAGING_DIR'] = os.environ.get('STAGING_DIR', '')

    if os.environ.has_key('CROSS_PATH'):
        env['ENV']['PATH'] = ':'.join([ os.environ['CROSS_PATH'], env['ENV']['PATH'] ] )

    if os.environ.has_key('CROSS_CFLAGS'):
        env.Append(CFLAGS=os.environ['CROSS_CFLAGS'].split())

    if os.environ.has_key('CROSS_LINKFLAGS'):
        env.Append(LINKFLAGS=os.environ['CROSS_LINKFLAGS'].split())

    env['libs'] = ['crypto', 'pthread']
    env.Append(CFLAGS=['-Wall',
                       '-pipe',
                       '-static',
                       '-funsigned-char',
                       '-Wpointer-sign',
                       '-Wimplicit-function-declaration',
                       '-fno-strict-aliasing'])
    if env['VARIANT'] == 'debug':
        env.Append(CFLAGS='-g')
    else:
        env.Append(CPPDEFINES=['NDEBUG'])
        env.Append(CFLAGS='-Os')
        env.Append(LINKFLAGS='-s')

    if env['FORCE32'] == 'true':
        env.Append(CFLAGS='-m32')
        env.Append(LINKFLAGS='-m32')

# Include paths
env['includes'] = [ os.getcwd() + '/inc', os.getcwd() + '/target/${TARG}', os.getcwd() + '/crypto/ecc', os.getcwd() + '/external/sha2']

# Target-specific headers and sources
env['aj_targ_headers'] = [Glob('target/' + env['TARG'] + '/*.h')]
env['aj_targ_srcs'] = [Glob('target/' + env['TARG'] + '/*.c')]


# AllJoyn Thin Client headers and sources (target independent)
env['aj_headers'] = [Glob('inc/*.h') + Glob('external/*/*.h')]
env['aj_srcs'] = [Glob('src/*.c')]
env['aj_sw_crypto'] = [Glob('crypto/*.c')]
env['aj_malloc'] = [Glob('malloc/*.c')]
env['aj_crypto_ecc'] = [Glob('crypto/ecc/*.c')]
env['aj_external_sha2'] = [Glob('external/sha2/*.c')]
wsl = [Glob('WSL/*.c')]
wsl_headers = [Glob('WSL/*.h')]

# Set-up the environment for Win/Linux
if env['TARG'] in [ 'win32', 'linux', 'darwin' ]:
    # To compile, sources need access to include files
    env.Append(CPPPATH = [env['includes']])

    # Win/Linux programs need libs to link
    env.Append(LIBS = [env['libs']])

    # Win/Linux programs need their own 'main' function
    env.Append(CPPDEFINES = ['AJ_MAIN', auth])

    # We have more memory on these platforms
    env.Append(CPPDEFINES = ['AJ_NVRAM_SIZE=64000'])
    env.Append(CPPDEFINES = ['AJ_NUM_REPLY_CONTEXTS=8'])

    # Produce shared libraries for these platforms
    srcs = env['aj_srcs'] + env['aj_targ_srcs'] + env['aj_crypto_ecc'] + env['aj_malloc'] + env['aj_external_sha2']
    if env['TARG'] == 'win32':
        srcs += env['aj_sw_crypto']

    env.SharedLibrary('ajtcl', srcs)
    env.StaticLibrary('ajtcl_st', srcs)
    env['aj_obj'] = env.StaticObject(srcs)
    env['aj_shobj'] = env.SharedObject(srcs)

if env['AJWSL'] == 'due':
    env['aj_obj'] = env.Object(env['aj_srcs'] + env['aj_sw_crypto'] + env['aj_malloc'] + env['aj_crypto_ecc'] + env['aj_external_sha2'])
    env['aj_obj'] += env.Object(wsl)
    env['aj_obj'] += env.Object(due_src)
    env['aj_obj'] += env.Object(rtos_src)

    # Build standard ajtcl test programs
    env.Program('test/svclite', ['test/svclite.c'] + env['aj_obj'])
    env.Program('test/clientlite', ['test/clientlite.c'] + env['aj_obj'])
    env.Program('test/siglite', ['test/siglite.c'] + env['aj_obj'])
    env.Program('test/nvramtest', ['test/nvramtest.c'] + env['aj_obj'])
    env.Program('test/sessionslite', ['test/sessionslite.c'] + env['aj_obj'])
    env.Program('test/codisco', ['test/codisco.c'] + env['aj_obj'])
    env.Program('test/scan-n-con', ['test/scan-n-con.c'] + env['aj_obj'])

elif env['AJWSL'] == 'stm32':

    srcs = env.Object(env['aj_srcs'] + env['aj_sw_crypto'] + env['aj_malloc'] + env['aj_crypto_ecc'] + env['aj_external_sha2'])
    srcs += env.Object(wsl)
    srcs += env.Object(rtos_src)
    srcs += env.Object(stm_src)

    stm_lib = env.StaticLibrary('ajtcl_stm32', srcs)

    # Build standard ajtcl test programs
    env.Program('test/svclite', ['test/svclite.c'], LIBS=[stm_lib])
    env.Program('test/nvramtest', ['test/nvramtest.c'], LIBS=[stm_lib])
    env.Program('test/clientlite', ['test/clientlite.c'], LIBS=[stm_lib])
    env.Program('test/codisco', ['test/codisco.c'], LIBS=[stm_lib])
    env.Program('test/scan-n-con', ['test/scan-n-con.c'], LIBS=[stm_lib])

elif env['AJWSL'] == 'frdm':
    srcs = []
    # The freedom board has its own NVRAM implementation so remove src/aj_nvram.c
    for h in env['aj_srcs']:
        h.remove(env.File('src/aj_nvram.c'))

    if env['RTOS'] == 'freeRTOS':
        freertos = [Glob(env['FREE_RTOS_DIR'] + '/Source/*.c') + [env['FREE_RTOS_DIR'] + '/Source/portable/GCC/ARM_CM3/port.c',
                                                              env['FREE_RTOS_DIR'] + '/Source/portable/MemMang/heap_4.c']]
        srcs += env.Object(freertos)

    aj_obj = env.Object(env['aj_srcs'] + env['aj_sw_crypto'] + env['aj_malloc'] + env['aj_crypto_ecc'] + env['aj_external_sha2'] + target + wsl)

    env.StaticLibrary('ajtcl_frdm', [objects, srcs,  aj_obj])
    #test_src = env.Object(['bsp/frdm/test_mbed.cpp'])

    env.Program('test/svclite', ['test/svclite.c'] + [objects, srcs,  aj_obj])
    env.Program('test/nvramtest', ['test/nvramtest.c'] + [objects, srcs,  aj_obj])
    env.Program('test/codisco', ['test/codisco.c'] + [objects, srcs,  aj_obj])
    env.Program('test/scan-n-con', ['test/scan-n-con.c'] + [objects, srcs,  aj_obj])

Export('env')

if env['WS'] != 'off' and not env.GetOption('clean') and not env.GetOption('help'):
    import sys
    bin_dir = os.getcwd() + '/tools'
    sys.path.append(bin_dir)
    import whitespace
    env['uncrustify_cfg_root'] = os.getcwd();

    def wsbuild(target, source, env):
        print "Evaluating whitespace compliance..."
        print "Note: enter 'scons -h' to see whitespace (WS) options"
        curdir = os.path.abspath(os.path.dirname(wsbuild.func_code.co_filename))
        version = whitespace.get_uncrustify_version()
        if (version == "0.57"):
            config = env['uncrustify_cfg_root'] + '/ajuncrustify.0.57.cfg'
        else: #use latest known version
            config = env['uncrustify_cfg_root'] + '/ajuncrustify.0.61.cfg'
        print "Config:", config
        print "Note: enter 'scons -h' to see whitespace (WS) options"
        return whitespace.main([env['WS'], config])

    env.Command('#/ws_ajtcl', Dir('$DISTDIR'), wsbuild)

# In case of Arduino target, package the 'SDK' suitable for development
# on Arduino IDE
if env['TARG'] == 'arduino':
    arduinoLibDir = 'build/arduino_due/libraries/AllJoyn/'

    # Arduino sketches need the corresponding platform-independent sources
    tests = [ ]
    tests.append('svclite')
    tests.append('clientlite')
    tests.append('siglite')
    tests.append('bastress2')
    tests.append('mutter')
    tests.append('sessions')
    tests.append('aestest')
    testInputs = [ ]
    testOutputs = [ ]

    # Install the generic .c files from the test directory into their
    # destination while changing the extension
    # Also install the .ino file for the test sketch
    for test in Flatten(tests):
        in_path = File('test/' + test + '.c')
        out_path = File('target/arduino/tests/AJ_' + test + '/' + test + '.cpp')

        env.Install(Dir(arduinoLibDir + 'tests/AJ_' + test + '/').abspath, File('target/arduino/tests/AJ_' + test + '/AJ_' + test + '.ino'))
        env.InstallAs(File(arduinoLibDir + 'tests/AJ_' + test + '/' + test + '.cpp').abspath, in_path.abspath)

    replaced_names = []
    for x in Flatten([env['aj_srcs'], env['aj_targ_srcs'], env['aj_sw_crypto'], env['aj_external_sha2']]):
        replaced_names.append( File(arduinoLibDir + x.name.replace('.c', '.cpp') ) )

    # change the extension
    install_renamed_files = env.InstallAs(Flatten(replaced_names), Flatten([env['aj_srcs'], env['aj_targ_srcs'], env['aj_sw_crypto'], env['aj_external_sha2']]))
    install_host_headers = env.Install(arduinoLibDir, env['aj_targ_headers'])
    install_headers = env.Install(arduinoLibDir, env['aj_headers'])

    # install the examples into their source
    env.Install(Dir(arduinoLibDir).abspath, 'target/arduino/examples/')

    # Install basic samples
    basicsamples = [ ]
    basicsamples.append('basic_service')
    basicsamples.append('basic_client')
    basicsamples.append('signal_service')
    basicsamples.append('signalConsumer_client')

    securesamples = [ ]
    securesamples.append('SecureClient')
    securesamples.append('SecureService')

    for sample in Flatten(basicsamples):
        in_path = File('samples/basic/' + sample + '.c')
        out_path = File('target/arduino/samples/AJ_' + sample + '/' + sample + '.cpp')
        env.Install(Dir(arduinoLibDir + 'samples/AJ_' + sample + '/').abspath, File('target/arduino/samples/AJ_' + sample + '/AJ_' + sample + '.ino'))
        env.InstallAs(File(arduinoLibDir + 'samples/AJ_' + sample + '/' + sample + '.cpp').abspath, in_path.abspath)

    for sample in Flatten(securesamples):
        in_path = File('samples/secure/' + sample + '.c')
        out_path = File('target/arduino/samples/AJ_' + sample + '/' + sample + '.cpp')
        env.Install(Dir(arduinoLibDir + 'samples/AJ_' + sample + '/').abspath, File('target/arduino/samples/AJ_' + sample + '/AJ_' + sample + '.ino'))
        env.InstallAs(File(arduinoLibDir + 'samples/AJ_' + sample + '/' + sample + '.cpp').abspath, in_path.abspath)

Return('env')
