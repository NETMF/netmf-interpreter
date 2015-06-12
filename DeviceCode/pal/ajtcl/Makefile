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

#
# This makefile is meant to be used solely for building WSL-enabled applications.
# It is intended to be invoked from IDEs that do not support building using SCons.
# All paths in this file are relative to the makefile location
#
# Some care has to be taken when passing string values into the makefile from the command-line.
# An extra level of backslash escaping required to get a string presented to the compiler's preprocessor.
# For example, passing the string "bar" to use as a character string initializer in a .c file:
#    const char foo_var[] = FOO_VAR;
#
# Is passed on the command line:
#    make target FOO_VAR=\\\"bar\\\"
#
# Which results in compiler arguments such as:
#    -DFOO_VAR=\"bar\"
#
# settings that can be defined:
#   AJ_CONFIGURE_WIFI_UPON_START: (boolean) should wifi network connections be started when the AllJoyn
#            application starts, 1 means yes, 0 or undef means no
#            example: AJ_CONFIGURE_WIFI_UPON_START=1
#   WIFI_DEVICE_NAME: (quoted string) the hostname to assign to your device when connecting to the network
#            example: WIFI_DEVICE_NAME=\\\"WSLNode001\\\"
#   WIFI_SCAN: (boolean) should a scan of access points be performed when the device starts up,
#            1 means yes, 0 or undef means no
#            example: WIFI_SCAN=1
#   WIFI_SSID: (quoted string) the SSID of the network to connect to
#            example: WIFI_SSID=\\\"AP_SSID01\\\"
#   WIFI_PASSPHRASE: (quoted string) the passphrase used to connect to WIFI_SSID
#            example: WIFI_PASSPHRASE=\\\"PassPhrase\\\"
#   SOFTAP_SSID: (quoted string) the SSID to use when the device is put into Access Point mode
#            example: SOFTAP_SSID=\\\"SoftAP_SSID01\\\"
#   SOFTAP_PASSPHRASE: (quoted string) the passphrase needed by clients to connect to SOFTAP_SSID
#            example: SOFTAP_PASSPHRASE=\\\"My_PassPhrase\\\"

ALLJOYN_DEFINES =

# Check which options are defined on the command-line
ifneq ($(AJ_DEBUG_RESTRICT), )
ALLJOYN_DEFINES += -DAJ_DEBUG_RESTRICT=${AJ_DEBUG_RESTRICT}
endif

ifneq ($(AJ_CONFIGURE_WIFI_UPON_START), )
ALLJOYN_DEFINES += -DAJ_CONFIGURE_WIFI_UPON_START=${AJ_CONFIGURE_WIFI_UPON_START}
endif

ifneq ($(WIFI_DEVICE_NAME),)
ALLJOYN_DEFINES += -DWIFI_DEVICE_NAME=${WIFI_DEVICE_NAME}
endif

ifneq ($(WIFI_SCAN),)
ALLJOYN_DEFINES += -DWIFI_SCAN=${WIFI_SCAN}
endif

ifneq ($(WIFI_SSID),)
ALLJOYN_DEFINES += -DWIFI_SSID=${WIFI_SSID}
endif

ifneq ($(WIFI_PASSPHRASE),)
ALLJOYN_DEFINES += -DWIFI_PASSPHRASE=${WIFI_PASSPHRASE}
endif

ifneq ($(SOFTAP_SSID),)
ALLJOYN_DEFINES += -DSOFTAP_SSID=${SOFTAP_SSID}
endif

ifneq ($(SOFTAP_PASSPHRASE),)
ALLJOYN_DEFINES += -DSOFTAP_PASSPHRASE=${SOFTAP_PASSPHRASE}
endif


DEFINES = 	-D__SAM3X8E__ \
			-DARM_MATH_CM3=true \
			-DBOARD=ARDUINO_DUE_X \
			-Dprintf=iprintf 

DEFINES += $(ALLJOYN_DEFINES)



RELEASE_DEFINES = 	-DNDEBUG \


# Linker flags
LDFLAGS = 	-mthumb \
			-Wl,-Map=$@.map \
			-Wl,--start-group \
			-larm_cortexM3l_math \
			-lm \
			-Wl,--end-group \
			-L"$(ATMEL_DIR)/thirdparty/CMSIS/Lib/GCC" \
			-Wl,--gc-sections \
			-mcpu=cortex-m3 \
			-Wl,--entry=Reset_Handler \
			-Wl,--cref \
			-mthumb \
			-T$(ATMEL_DIR)/sam/utils/linker_scripts/sam3x/sam3x8/gcc/flash.ld 

RELEASE_LDFLAGS = \
			-Os \


# Include Paths
INCLUDES = 	-I. \
			-Iconfig \
			-I$(FREE_RTOS_DIR)/Source/include \
			-I$(FREE_RTOS_DIR)/Source/portable/GCC/ARM_CM3 \
			-I$(ATMEL_DIR)/common/boards \
			-I$(ATMEL_DIR)/common/services/clock \
			-I$(ATMEL_DIR)/common/services/clock/sam3x \
			-I$(ATMEL_DIR)/common/services/gpio \
			-I$(ATMEL_DIR)/common/services/ioport \
			-I$(ATMEL_DIR)/common/services/freertos/sam \
			-I$(ATMEL_DIR)/common/services/serial/sam_uart \
			-I$(ATMEL_DIR)/common/services/serial \
			-I$(ATMEL_DIR)/common/services/spi \
			-I$(ATMEL_DIR)/common/services/sam_spi \
			-I$(ATMEL_DIR)/common/services/spi/sam_spi/module_config \
			-I$(ATMEL_DIR)/common/utils \
			-I$(ATMEL_DIR)/common/utils/stdio/stdio_serial \
			-I$(ATMEL_DIR)/common/drivers/nvm \
			-I$(ATMEL_DIR)/common/drivers/nvm/sam/module_config \
			-I$(ATMEL_DIR)/sam/boards \
			-I$(ATMEL_DIR)/sam/boards/arduino_due_x \
			-I$(ATMEL_DIR)/sam/drivers/dmac \
			-I$(ATMEL_DIR)/sam/drivers/pio \
			-I$(ATMEL_DIR)/sam/drivers/pmc \
			-I$(ATMEL_DIR)/sam/drivers/pdc \
			-I$(ATMEL_DIR)/sam/drivers/uart \
			-I$(ATMEL_DIR)/sam/drivers/usart \
			-I$(ATMEL_DIR)/sam/drivers/spi \
			-I$(ATMEL_DIR)/sam/drivers/efc \
			-I$(ATMEL_DIR)/sam/drivers/trng \
			-I$(ATMEL_DIR)/sam/utils \
			-I$(ATMEL_DIR)/sam/utils/cmsis/sam3x/include \
			-I$(ATMEL_DIR)/sam/utils/cmsis/sam3x/source/templates \
			-I$(ATMEL_DIR)/sam/utils/cmsis/sam3x/include/component \
			-I$(ATMEL_DIR)/sam/utils/header_files \
			-I$(ATMEL_DIR)/sam/utils/preprocessor \
			-I$(ATMEL_DIR)/sam/services/flash_efc \
			-I$(ATMEL_DIR)/thirdparty/CMSIS/Include \
			-I$(ATMEL_DIR)/thirdparty/CMSIS/Lib/GCC \
			-I$(ATMEL_DIR)/sam/boards/arduino_due_x/board_config \
			-I$(ATMEL_DIR)/common/services/clock/sam3x/module_config \
			-I$(ATMEL_DIR)/config \
			-I$(ATMEL_DIR)/common/services/clock/sam3x \
			-Iinc \
			-IRTOS \
			-IRTOS/FreeRTOS \
			-Ibsp \
			-Ibsp/due \
			-Ibsp/due/config \
			-Imalloc \
			-Iexternal\sha2 \
			-Icrypto \
			-Icrypto/ecc \
			-IWSL
	
# Compiler flags	
FLAGS = 	-mthumb \
			-fdata-sections \
			-ffunction-sections \
			-mlong-calls \
			-g3 \
			-Wall \
			-mcpu=cortex-m3 \
			-c \
			-pipe \
			-fno-strict-aliasing \
			-Wmissing-prototypes \
			-Wpointer-arith \
			-std=gnu99 \
			-Wchar-subscripts \
			-Wcomment \
			-Wformat=2 \
			-Wimplicit-int \
			-Wmain \
			-Wparentheses \
			-Wsequence-point \
			-Wreturn-type \
			-Wswitch \
			-Wtrigraphs \
			-Wunused \
			-Wuninitialized \
			-Wfloat-equal \
			-Wundef \
			-Wshadow \
			-Wbad-function-cast \
			-Wwrite-strings \
			-Wsign-compare \
			-Waggregate-return \
			-Wmissing-declarations \
			-Wformat \
			-Wmissing-format-attribute \
			-Wno-deprecated-declarations \
			-Wpacked \
			-Wlong-long \
			-Wunreachable-code \
			-Wcast-align \
			--param max-inline-insns-single=500 \
			-MD \
			-MP \
			-MF $(@:%.o=%.d)


			
COMPILER = arm-none-eabi-gcc.exe

SOURCE_FILES :=	$(FREE_RTOS_DIR)/Source/croutine.c \
				$(FREE_RTOS_DIR)/Source/list.c \
				$(FREE_RTOS_DIR)/Source/queue.c \
				$(FREE_RTOS_DIR)/Source/tasks.c \
				$(FREE_RTOS_DIR)/Source/timers.c \
				$(FREE_RTOS_DIR)/Source/portable/MemMang/heap_3.c \
				$(FREE_RTOS_DIR)/Source/portable/GCC/ARM_CM3/port.c \
				$(ATMEL_DIR)/common/services/clock/sam3x/sysclk.c \
				$(ATMEL_DIR)/common/services/spi/sam_spi/spi_master.c \
				$(ATMEL_DIR)/common/services/freertos/sam/freertos_peripheral_control.c \
				$(ATMEL_DIR)/common/services/freertos/sam/freertos_usart_serial.c \
				$(ATMEL_DIR)/common/utils/interrupt/interrupt_sam_nvic.c \
				$(ATMEL_DIR)/common/utils/stdio/read.c \
				$(ATMEL_DIR)/common/utils/stdio/write.c \
				$(ATMEL_DIR)/common/drivers/nvm/sam/sam_nvm.c \
				$(ATMEL_DIR)/sam/boards/arduino_due_x/init.c \
				$(ATMEL_DIR)/sam/boards/arduino_due_x/led.c \
				$(ATMEL_DIR)/sam/drivers/dmac/dmac.c \
				$(ATMEL_DIR)/sam/drivers/pdc/pdc.c \
				$(ATMEL_DIR)/sam/drivers/pio/pio.c \
				$(ATMEL_DIR)/sam/drivers/pio/pio_handler.c \
				$(ATMEL_DIR)/sam/drivers/pmc/pmc.c \
				$(ATMEL_DIR)/sam/drivers/pmc/sleep.c \
				$(ATMEL_DIR)/sam/drivers/uart/uart.c \
				$(ATMEL_DIR)/sam/drivers/usart/usart.c \
				$(ATMEL_DIR)/sam/drivers/spi/spi.c \
				$(ATMEL_DIR)/sam/drivers/efc/efc.c \
				$(ATMEL_DIR)/sam/drivers/trng/trng.c \
				$(ATMEL_DIR)/sam/utils/cmsis/sam3x/source/templates/exceptions.c \
				$(ATMEL_DIR)/sam/utils/cmsis/sam3x/source/templates/system_sam3x.c \
				$(ATMEL_DIR)/sam/utils/cmsis/sam3x/source/templates/gcc/startup_sam3x.c \
				$(ATMEL_DIR)/sam/services/flash_efc/flash_efc.c \
				$(ATMEL_DIR)/sam/utils/syscalls/gcc/syscalls.c \
				crypto/aj_sw_crypto.c \
				crypto/ecc/aj_crypto_ecc.c \
				crypto/ecc/aj_crypto_sha2.c \
				src/aj_about.c \
				src/aj_bufio.c \
				src/aj_bus.c \
				src/aj_connect.c \
				src/aj_cert.c \
				src/aj_crc16.c \
				src/aj_creds.c \
				src/aj_crypto.c \
				src/aj_debug.c \
				src/aj_disco.c \
				src/aj_guid.c \
				src/aj_helper.c \
				src/aj_init.c \
				src/aj_introspect.c \
				src/aj_keyauthentication.c \
				src/aj_keyexchange.c \
				src/aj_link_timeout.c \
				src/aj_msg.c \
				src/aj_nvram.c \
				src/aj_peer.c \
				src/aj_serial.c \
				src/aj_serial_rx.c \
				src/aj_serial_tx.c \
				src/aj_std.c \
				src/aj_util.c \
				malloc/aj_malloc.c \
				external/sha2/sha2.c \
				WSL/aj_buf.c \
				WSL/aj_wsl_htc.c \
				WSL/aj_wsl_net.c \
				WSL/aj_wsl_spi_mbox.c \
				WSL/aj_wsl_unmarshal.c \
				WSL/aj_wsl_wmi.c \
				WSL/aj_wsl_marshal.c \
				WSL/aj_wsl_tasks.c \
				RTOS/main.c \
				RTOS/aj_net.c \
				RTOS/aj_wifi_ctrl.c \
				RTOS/Alljoyn.c \
				RTOS/FreeRTOS/aj_target_rtos.c \
				bsp/due/aj_spi.c \
				bsp/due/aj_trng.c \
				bsp/due/aj_target_platform.c


TEST_FILES = 	test/aestest.c
TEST_FILES +=	test/mutter.c
TEST_FILES +=	test/svclite.c
TEST_FILES +=	test/nvramtest.c
TEST_FILES += 	test/clientlite.c
TEST_FILES += 	test/sessionslite.c

		
SAMPLE_FILES =  	samples/basic/basic_client.c
SAMPLE_FILES += 	samples/basic/basic_service.c
SAMPLE_FILES += 	samples/basic/nameChange_client.c
SAMPLE_FILES += 	samples/basic/signalConsumer_client.c
SAMPLE_FILES += 	samples/basic/signal_service.c
SAMPLE_FILES += 	samples/secure/SecureClient.c

OBJECTS = $(SOURCE_FILES:.c=.o)
TEST_OBJECTS = $(TEST_FILES:.c=.o)
SAMPLE_OBJECTS = $(SAMPLE_FILES:.c=.o)
DFILES = $(SOURCE_FILES:.c=.d)
EXECUTABLES = $(TEST_FILES:.c=.elf)

test: $(TEST_FILES) $(TEST_OBJECTS) $(OBJECTS)
sample: $(SAMPLE_FILES) $(SAMPLE_OBJECTS) $(OBJECTS)




Release: release
#release: FLAGS += -O3
release: DEFINES += -DNDEBUG
release: all

Debug: debug
#debug: FLAGS += -g3
debug: all


# All the test elf's are linked here. Add more by using the same format as the others
# $(COMPILER) -o <name_of_elf>.elf $(OBJECTS) <any_extra_files_with_main()>.o $(LDFLAGS)
all: test \
	sample \
	due_mutter \
	due_svclite \
	due_nvram_test \
	due_clientlite \
	due_sessionslite \
	due_basic_client \
	due_basic_service \

due_aestest:
	$(COMPILER) -o due_aestest.elf $(OBJECTS) test/aestest.o $(LDFLAGS)

due_mutter:
	$(COMPILER) -o due_mutter.elf $(OBJECTS) test/mutter.o $(LDFLAGS)

due_svclite:
	$(COMPILER) -o due_svclite.elf $(OBJECTS) test/svclite.o $(LDFLAGS)
	
due_nvram_test:
	$(COMPILER) -o due_nvram_test.elf $(OBJECTS) test/nvramtest.o $(LDFLAGS)

due_clientlite:
	$(COMPILER) -o due_clientlite.elf $(OBJECTS) test/clientlite.o $(LDFLAGS)

due_sessionslite:
	$(COMPILER) -o due_sessionslite.elf $(OBJECTS) test/sessionslite.o $(LDFLAGS)

due_samplesecureclient:
	$(COMPILER) -o due_samplesecureclient.elf $(OBJECTS) samples/secure/secureclient.o $(LDFLAGS)

due_samplesecureservice:
	$(COMPILER) -o due_samplesecureservice.elf $(OBJECTS) samples/secure/secureservice.o $(LDFLAGS)


due_basic_client:
	$(COMPILER) -o due_basic_client.elf $(OBJECTS) samples/basic/basic_client.o $(LDFLAGS)

due_basic_service:
	$(COMPILER) -o due_basic_service.elf $(OBJECTS) samples/basic/basic_service.o $(LDFLAGS)

due_nameChange_client:
	$(COMPILER) -o due_nameChange_client.elf $(OBJECTS) samples/basic/nameChange_client.o $(LDFLAGS)

due_signalConsumer_client:
	$(COMPILER) -o due_signalConsumer_client.elf $(OBJECTS) samples/basic/signalConsumer_client.o $(LDFLAGS)

due_signal_service:
	$(COMPILER) -o due_signal_service.elf $(OBJECTS) samples/basic/signal_service.o $(LDFLAGS)

%.o : %.c
	$(COMPILER) $(DEFINES) $(INCLUDES) $(FLAGS) -o $@ $<

clean:
	cmd /C del /S *.elf
	cmd /C for /r %i in (*.o, *.d, *.map) do del %i

clean_c: 
	cmd /C del /S *.o
	cmd /C del /S *.d
	cd test; cmd /C del /S *.o
	cd test; cmd /C del /S *.d
	cd ajtcl; cmd /C del /S *.o
	cd ajtcl; cmd /C del /S *.d

