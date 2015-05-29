/* -----------------------------------------------------------------------------
 * Copyright (c) 2013 - 2014 ARM Ltd.
 *
 * This software is provided 'as-is', without any express or implied warranty. 
 * In no event will the authors be held liable for any damages arising from 
 * the use of this software. Permission is granted to anyone to use this 
 * software for any purpose, including commercial applications, and to alter 
 * it and redistribute it freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not 
 *    claim that you wrote the original software. If you use this software in
 *    a product, an acknowledgment in the product documentation would be 
 *    appreciated but is not required. 
 * 
 * 2. Altered source versions must be plainly marked as such, and must not be 
 *    misrepresented as being the original software. 
 * 
 * 3. This notice may not be removed or altered from any source distribution.
 *
 *
 * $Date:        31. March 2014
 * $Revision:    V1.00
 *  
 * Driver:       Driver_Flash# (default: Driver_Flash0)
 * Project:      Flash Device Driver for M29W640FB (16-bit Bus)
 * ---------------------------------------------------------------------- 
 * Use the following configuration settings in the middleware component
 * to connect to this driver.
 * 
 *   Configuration Setting                   Value
 *   ---------------------                   -----
 *   Connect to hardware via Driver_Flash# = n (default: 0)
 * -------------------------------------------------------------------- */

#include "Driver_Flash.h"
#include "M29W640FB.h"

#define ARM_FLASH_DRV_VERSION ARM_DRIVER_VERSION_MAJOR_MINOR(1,00) /* driver version */


#ifndef DRIVER_FLASH_NUM
#define DRIVER_FLASH_NUM        0       /* Default driver number */
#endif

#ifndef FLASH_ADDR
#define FLASH_ADDR              0x60000000  /* Flash base address */
#endif


#define M16(addr) (*((volatile uint16_t *) (addr)))


/* Flash Commands */
#define CMD_RESET               0xF0
#define CMD_ERASE               0x80
#define CMD_ERASE_CHIP          0x10
#define CMD_ERASE_SECTOR        0x30
#define CMD_PROGRAM             0xA0

/* Flash Status Register bits */
#define DQ7                     (1<<7)
#define DQ6                     (1<<6)
#define DQ5                     (1<<5)
#define DQ3                     (1<<3)


/* Sector Information */
#ifdef FLASH_SECTORS
static ARM_FLASH_SECTOR FLASH_SECTOR_INFO[FLASH_SECTOR_COUNT] = {
  FLASH_SECTORS
};
#else
#define FLASH_SECTOR_INFO NULL
#endif

/* Flash Information */
static ARM_FLASH_INFO FlashInfo = {
  FLASH_SECTOR_INFO,
  FLASH_SECTOR_COUNT,
  FLASH_SECTOR_SIZE,
  FLASH_PAGE_SIZE,
  FLASH_PROGRAM_UNIT,
  FLASH_ERASED_VALUE
};

/* Flash Status */
static ARM_FLASH_STATUS FlashStatus;


/* Driver Version */
static const ARM_DRIVER_VERSION DriverVersion = {
  ARM_FLASH_API_VERSION,
  ARM_FLASH_DRV_VERSION
};

/* Driver Capabilities */
static const ARM_FLASH_CAPABILITIES DriverCapabilities = {
  0,    /* event_ready */
  1,    /* data_width = 0:8-bit, 1:16-bit, 2:32-bit */
  1     /* erase_chip */
};


/* Check if Program/Erase completed */
static bool DQ6_Polling (uint32_t addr) {
  uint32_t fsreg;
  uint32_t dqold;

  fsreg = M16(addr);
  do {
    dqold = fsreg & DQ6;
    fsreg = M16(addr);
    if ((fsreg & DQ6) == dqold) {
      return true;              /* Done */
    }
  } while ((fsreg & DQ5) != DQ5);
  fsreg = M16(addr);
  dqold = fsreg & DQ6;
  fsreg = M16(addr);
  if ((fsreg & DQ6) == dqold) {
    return true;                /* Done */
  }
  M16(addr) = CMD_RESET;        /* Reset Flash Device */
  return false;                 /* Error */
}


/**
  \fn          ARM_DRIVER_VERSION ARM_Flash_GetVersion (void)
  \brief       Get driver version.
  \return      \ref ARM_DRIVER_VERSION
*/
static ARM_DRIVER_VERSION GetVersion (void) {
  return DriverVersion;
}

/**
  \fn          ARM_FLASH_CAPABILITIES ARM_Flash_GetCapabilities (void)
  \brief       Get driver capabilities.
  \return      \ref ARM_FLASH_CAPABILITIES
*/
static ARM_FLASH_CAPABILITIES GetCapabilities (void) {
  return DriverCapabilities;
}

/**
  \fn          int32_t ARM_Flash_Initialize (ARM_Flash_SignalEvent_t cb_event)
  \brief       Initialize the Flash Interface.
  \param[in]   cb_event  Pointer to \ref ARM_Flash_SignalEvent
  \return      \ref execution_status
*/
static int32_t Initialize (ARM_Flash_SignalEvent_t cb_event) {
  FlashStatus.busy  = 0;
  FlashStatus.error = 0;
  return ARM_DRIVER_OK;
}

/**
  \fn          int32_t ARM_Flash_Uninitialize (void)
  \brief       De-initialize the Flash Interface.
  \return      \ref execution_status
*/
static int32_t Uninitialize (void) {
  return ARM_DRIVER_OK;
} 

/**
  \fn          int32_t ARM_Flash_PowerControl (ARM_POWER_STATE state)
  \brief       Control the Flash interface power.
  \param[in]   state  Power state
  \return      \ref execution_status
*/
static int32_t PowerControl (ARM_POWER_STATE state) {
  return ARM_DRIVER_OK;
}

/**
  \fn          int32_t ARM_Flash_ReadData (uint32_t addr, void *data, uint32_t cnt)
  \brief       Read data from Flash.
  \param[in]   addr  Data address.
  \param[out]  data  Pointer to a buffer storing the data read from Flash.
  \param[in]   cnt   Number of data items to read.
  \return      number of data items read or \ref execution_status
*/
static int32_t ReadData (uint32_t addr, void *data, uint32_t cnt) {
  uint16_t *mem;
  uint32_t  n;

  if ((addr & 1) || (data == NULL)) {
    return ARM_DRIVER_ERROR_PARAMETER;
  }

  if (FlashStatus.busy) {
    return ARM_DRIVER_ERROR_BUSY;
  }
  FlashStatus.error = 0;

  //addr += FLASH_ADDR;
  mem = data;
  for (n = cnt; n; n--) {
    *mem++ = M16(addr);
    addr  += 2;
  }

  return cnt;
}

/**
  \fn          int32_t ARM_Flash_ProgramData (uint32_t addr, const void *data, uint32_t cnt)
  \brief       Program data to Flash.
  \param[in]   addr  Data address.
  \param[in]   data  Pointer to a buffer containing the data to be programmed to Flash.
  \param[in]   cnt   Number of data items to program.
  \return      number of data items programmed or \ref execution_status
*/
static int32_t ProgramData (uint32_t addr, const void *data, uint32_t cnt) {
  const uint16_t *mem;
        uint32_t  n;

  if ((addr & 1) || (data == NULL)) {
    return ARM_DRIVER_ERROR_PARAMETER;
  }

  if (FlashStatus.busy) {
    return ARM_DRIVER_ERROR_BUSY;
  }
  FlashStatus.busy  = 1;
  FlashStatus.error = 0;

  if (cnt == 0) return 0;

  //addr += FLASH_ADDR;
  mem = data;
  for (n = cnt; n; n--) {
    M16(FLASH_ADDR + (0x555 << 1)) = 0xAA;
    M16(FLASH_ADDR + (0x2AA << 1)) = 0x55;
    M16(FLASH_ADDR + (0x555 << 1)) = CMD_PROGRAM;
    M16(addr) = *mem++;
    addr += 2;
    if (n > 1) {
      if (!DQ6_Polling(FLASH_ADDR)) {
        FlashStatus.busy  = 0;
        FlashStatus.error = 1;
        return ARM_DRIVER_ERROR;
      }
    }
  }

  return cnt;
}

/**
  \fn          int32_t ARM_Flash_EraseSector (uint32_t addr)
  \brief       Erase Flash Sector.
  \param[in]   addr  Sector address
  \return      \ref execution_status
*/
static int32_t EraseSector (uint32_t addr) {

  if (FlashStatus.busy) {
    return ARM_DRIVER_ERROR_BUSY;
  }
  FlashStatus.busy  = 1;
  FlashStatus.error = 0;

  M16(FLASH_ADDR + (0x555 << 1)) = 0xAA;
  M16(FLASH_ADDR + (0x2AA << 1)) = 0x55;
  M16(FLASH_ADDR + (0x555 << 1)) = CMD_ERASE;
  M16(FLASH_ADDR + (0x555 << 1)) = 0xAA;
  M16(FLASH_ADDR + (0x2AA << 1)) = 0x55;
  M16(/*FLASH_ADDR +*/  addr)        = CMD_ERASE_SECTOR;

  return ARM_DRIVER_OK;
}

/**
  \fn          int32_t ARM_Flash_EraseChip (void)
  \brief       Erase complete Flash.
               Optional function for faster full chip erase.
  \return      \ref execution_status
*/
static int32_t EraseChip (void) {

  if (FlashStatus.busy) {
    return ARM_DRIVER_ERROR_BUSY;
  }
  FlashStatus.busy  = 1;
  FlashStatus.error = 0;

  M16(FLASH_ADDR + (0x555 << 1)) = 0xAA;
  M16(FLASH_ADDR + (0x2AA << 1)) = 0x55;
  M16(FLASH_ADDR + (0x555 << 1)) = CMD_ERASE;
  M16(FLASH_ADDR + (0x555 << 1)) = 0xAA;
  M16(FLASH_ADDR + (0x2AA << 1)) = 0x55;
  M16(FLASH_ADDR + (0x555 << 1)) = CMD_ERASE_CHIP;

  return ARM_DRIVER_OK;
}

/**
  \fn          ARM_FLASH_STATUS ARM_Flash_GetStatus (void)
  \brief       Get Flash status.
  \return      Flash status \ref ARM_FLASH_STATUS
*/
static ARM_FLASH_STATUS GetStatus (void) {
  uint32_t fsreg;
  uint32_t dqold;

  if (FlashStatus.busy) {
    fsreg = M16(FLASH_ADDR);
    dqold = fsreg & DQ6;
    fsreg = M16(FLASH_ADDR);
    if ((fsreg & DQ6) == dqold) {
      FlashStatus.busy = 0;
    } else if (fsreg & DQ5) {
      M16(FLASH_ADDR) = CMD_RESET;
      FlashStatus.busy = 0;
      FlashStatus.error = 1;
    }
  }
  return FlashStatus;
}

/**
  \fn          ARM_FLASH_INFO * ARM_Flash_GetInfo (void)
  \brief       Get Flash information.
  \return      Pointer to Flash information \ref ARM_FLASH_INFO
*/
ARM_FLASH_INFO * GetInfo (void) {
  return &FlashInfo;
}


/* Flash Driver Control Block */
ARM_DRIVER_FLASH ARM_Driver_Flash_(DRIVER_FLASH_NUM) = {
  GetVersion,
  GetCapabilities,
  Initialize,
  Uninitialize,
  PowerControl,
  ReadData,
  ProgramData,
  EraseSector,
  EraseChip,
  GetStatus,
  GetInfo
};
