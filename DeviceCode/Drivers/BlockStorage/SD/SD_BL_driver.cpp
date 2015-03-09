////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "SD_BL.h"

//--//

extern struct SD_BL_CONFIGURATION g_SD_BL_Config;
extern struct SD_DEVICE_REGISTERS g_SD_DeviceRegisters;

#define SD_PHYISCAL_BASE_ADDRESS 0

static BYTE s_sectorBuff[SD_DATA_SIZE];
static BYTE s_cmdBuff[SD_CMD_SIZE];

#if defined(SD_DEBUG)
void DumpSector(BYTE* pSectorBuff, UINT32 BytesPerSector, UINT32 Offset);
#endif



//--//

static UINT8 CRC7Encode(UINT8 crc, UINT8 *input, int length)
{
    for(int j = 0; j < length; j++)
    {
        UINT8 byte = input[j];
        
        for(int i = 8; i > 0; i--)
        {
            crc = (crc << 1) | ((byte &0x80) ? 1 : 0);
            if(crc &0x80)
            {
                crc ^= 9; // polynomial
            } 
            byte <<= 1;
        }
        crc &= 0x7f;
    }
    
    for(int i = 7; i > 0; i--)
    {
        crc = (crc << 1);
        if(crc &0x80)
        {
            crc ^= 9; // polynomial
        }
    }
    
    crc &= 0x7f;
    
    return crc;
}

#define SPI_DELAY 50

BYTE SD_BS_Driver::SPISendByte(BYTE data)
{
    SPI_XACTION_8 config;
    BYTE ReadByte = 0;
    
    config.Read8 = &ReadByte;
    config.ReadCount = 1;
    config.ReadStartOffset = 0;
    config.SPI_mod = g_SD_BL_Config.SPI.SPI_mod;
    config.Write8 = &data;
    config.WriteCount = 1;
    config.BusyPin.Pin = GPIO_PIN_NONE;
    
    HAL_Time_Sleep_MicroSeconds(SPI_DELAY);
    CPU_SPI_Xaction_nWrite8_nRead8(config);
    HAL_Time_Sleep_MicroSeconds(SPI_DELAY);
    
    return ReadByte;
} 

void SD_BS_Driver::SPISendCount(BYTE *pWrite, UINT32 WriteCount)
{
    SPI_XACTION_8 config;

    ASSERT((pWrite != NULL) && (WriteCount != 0));

    config.Read8 = NULL;
    config.ReadCount = 0;
    config.ReadStartOffset = 0;
    config.SPI_mod = g_SD_BL_Config.SPI.SPI_mod;
    config.Write8 = pWrite;
    config.WriteCount = WriteCount;
    config.BusyPin.Pin = GPIO_PIN_NONE;

    HAL_Time_Sleep_MicroSeconds(SPI_DELAY);
    CPU_SPI_Xaction_nWrite8_nRead8(config);
	HAL_Time_Sleep_MicroSeconds(SPI_DELAY);
}

void SD_BS_Driver::SPIRecvCount(BYTE *pRead, UINT32 ReadCount, UINT32 Offset)
{
    SPI_XACTION_8 config;

    BYTE dummy = DUMMY;

    ASSERT((pRead != NULL) && (ReadCount != 0));

    config.Read8 = pRead;
    config.ReadCount = ReadCount;
    config.ReadStartOffset = Offset;
    config.SPI_mod = g_SD_BL_Config.SPI.SPI_mod;
    config.Write8 = &dummy;
    config.WriteCount = 1;
    config.BusyPin.Pin = GPIO_PIN_NONE;

	HAL_Time_Sleep_MicroSeconds(SPI_DELAY);
    CPU_SPI_Xaction_nWrite8_nRead8(config);
	HAL_Time_Sleep_MicroSeconds(SPI_DELAY);
}

//Sets SD CS to INACTIVE state
void SD_BS_Driver::SD_CsSetHigh()
{
    HAL_Time_Sleep_MicroSeconds(SPI_DELAY);
    CPU_GPIO_EnableOutputPin(g_SD_BL_Config.SPI.DeviceCS, !g_SD_BL_Config.SPI.CS_Active);

    // to force SDC/MMC to release the bus after CS goes inactive, do one dummy write.
    HAL_Time_Sleep_MicroSeconds(SPI_DELAY);
    SPISendByte(DUMMY);
}

//Sets SD CS to ACTIVE state
void SD_BS_Driver::SD_CsSetLow()
{
    CPU_GPIO_EnableOutputPin(g_SD_BL_Config.SPI.DeviceCS, g_SD_BL_Config.SPI.CS_Active);
    HAL_Time_Sleep_MicroSeconds(SPI_DELAY);
}

BYTE SD_BS_Driver::SD_CheckBusy(void)
{
    BYTE response;
    BYTE rvalue = 0xFF;

    for(int i=10000; i!=0; i--)
    {
        response = SPISendByte(0xff);

        if(response != 0xFF && rvalue == 0xFF)
        {
            response &= 0x1f;
            switch(response) /* 7 6 5 4 3    1 0  */
            {
                /* data response  x x x 0 status 1 */
                case 0x05:
                    rvalue = SD_SUCCESS;
                    break;

                case 0x0b:
                    return (SD_CRC_ERROR);

                case 0x0d:
                    return (SD_WRITE_ERROR);

                default:
                    rvalue = SD_OTHER_ERROR;
                    break;
            }
        }
        else if(response != 0x00 && rvalue != 0xFF)
        {
            break;
        }

        HAL_Time_Sleep_MicroSeconds(SPI_DELAY); 
    }

    return rvalue;
}

//return Card status in R1 response
BYTE SD_BS_Driver::SD_SendCmdWithR1Resp(BYTE cmd, UINT32 arg, BYTE crc, BYTE expectedToken, INT32 iterations)
{
    BYTE response;
    BYTE retVal = 0xFF;

    s_cmdBuff[0] = (0x40 | cmd); // command
    s_cmdBuff[1] = ((BYTE)((arg >> 24) &0xff)); // parameter
    s_cmdBuff[2] = ((BYTE)((arg >> 16) &0xff));
    s_cmdBuff[3] = ((BYTE)((arg >> 8) &0xff));
    s_cmdBuff[4] = ((BYTE)((arg >> 0) &0xff));
    s_cmdBuff[5] = (crc); // CRC check code

    SPISendByte(DUMMY);

    SPISendCount(s_cmdBuff, SD_CMD_SIZE);

    for(int i = 0; i < iterations; i++)
    {
        response = SPISendByte(DUMMY);
		
        if(response == expectedToken)
        {
            return expectedToken;
        }
        else if((response != 0xFF) && (retVal == 0xFF))
        {
            retVal = response;
        }
    }

    return retVal;
}

//return support voltage range in R3response
BYTE SD_BS_Driver::SD_SendCmdWithR7Resp(BYTE cmd, UINT32 arg, BYTE *outVoltage)
{
    BYTE response;
    BYTE i, echo_back;

    ASSERT(outVoltage != NULL);

    *outVoltage = 0xFF;

    s_cmdBuff[0] = (0x40 | cmd); // tansmition bit | command
    s_cmdBuff[1] = ((BYTE)((arg >> 24) &0xff)); // parameter
    s_cmdBuff[2] = ((BYTE)((arg >> 16) &0xff));
    s_cmdBuff[3] = ((BYTE)((arg >> 8) &0xff));
    s_cmdBuff[4] = ((BYTE)((arg >> 0) &0xff));
    s_cmdBuff[5] = (CRC7Encode(0, s_cmdBuff, 5) << 1) | 1; // CRC | ENDBIT

    SPISendByte(DUMMY);

    SPISendCount(s_cmdBuff, SD_CMD_SIZE);

    for(i = 0; i < 100; i++)
    {
        response = SPISendByte(DUMMY);
        if(response != 0xFF)
        // begin token of R7 response
            break;
    }

    if(i == 100)
        return FALSE;

    //recieve voltage content in R7 response
    SPISendByte(DUMMY);
    SPISendByte(DUMMY);
    *outVoltage = SPISendByte(DUMMY);
    echo_back = SPISendByte(DUMMY);

	return response;

}

//return support voltage range in R3response
BYTE SD_BS_Driver::SD_SendCmdWithR3Resp(BYTE cmd, UINT32 arg, UINT32 *pOcr)
{
    BYTE response;
    BYTE i;

    ASSERT(pOcr != NULL);

    *pOcr = 0;

    s_cmdBuff[0] = (0x40 | cmd); // tansmition bit | command
    s_cmdBuff[1] = ((BYTE)((arg >> 24) &0xff)); // parameter
    s_cmdBuff[2] = ((BYTE)((arg >> 16) &0xff));
    s_cmdBuff[3] = ((BYTE)((arg >> 8) &0xff));
    s_cmdBuff[4] = ((BYTE)((arg >> 0) &0xff));
    s_cmdBuff[5] = (CRC7Encode(0, s_cmdBuff, 5) << 1) | 1; // CRC | ENDBIT

    SPISendByte(DUMMY);

    SPISendCount(s_cmdBuff, SD_CMD_SIZE);

    for(i = 0; i < 10; i++)
    {
	    // R1 response ...
        response = SPISendByte(DUMMY);

        if(response != 0xFF)
        // begin token of R3 response
            break;
    }

    if(i == 10)
        return FALSE;

    //recieve voltage content in R7 response
    BYTE temp=0;
    for(int j=24; j>=0; j-=8)
    {
        temp=SPISendByte(DUMMY); 
       *pOcr |= (temp << j);
    }


     return response;

}

BOOL SD_BS_Driver::ChipInitialize(void *context)
{
    SD_BLOCK_CONFIG *config = (SD_BLOCK_CONFIG*)context;

	BOOL isVersion1 = TRUE;
    BOOL isInitialised = FALSE;
	
    if(!config || !config->BlockDeviceInformation)
    {
        return FALSE;
    }

    BlockDeviceInfo* pDevInfo = config->BlockDeviceInformation;
    
    UINT32 clkNormal = g_SD_BL_Config.SPI.Clock_RateKHz;
    
    g_SD_BL_Config.SPI.Clock_RateKHz = 400; // initialization clock speed
    
    //one test for insert \ eject ISR
    if(g_SD_BL_Config.InsertIsrPin != GPIO_PIN_NONE)
    {
        CPU_GPIO_EnableInputPin(g_SD_BL_Config.InsertIsrPin, TRUE, InsertISR, GPIO_INT_EDGE_LOW, RESISTOR_PULLUP);
    }
    if(g_SD_BL_Config.EjectIsrPin != GPIO_PIN_NONE)
    {
        CPU_GPIO_EnableInputPin(g_SD_BL_Config.EjectIsrPin, TRUE, EjectISR, GPIO_INT_EDGE_LOW, RESISTOR_PULLUP);
    }

    CPU_SPI_Initialize();
    GLOBAL_LOCK(irq);

    CPU_SPI_Xaction_Start(g_SD_BL_Config.SPI);

    BYTE response;

    do
    {
        // select SD card and set to IDLE
        response = SD_Cmd_GO_IDLE_STATE();
        if(response != R1_IN_IDLE_STATUS)
        {
            break;
        }

        // check if voltages in interface ok. Side effect is to detect version 2.0+ cards.
        if (!SD_Cmd_SEND_IF_COND(g_SD_BL_Config.Low_Voltage_Flag, &isVersion1))
        {
            break;
        }
       
        // read OCR. 
        // TODO: Check if supported voltage ok.
        response = ReadOCR_R3(&g_SD_DeviceRegisters.OCR);
        
        // Set SD to READY STATUS
        if (!SD_Set_In_READY_STATUS(!isVersion1))
        {
             break;  // failed, could not get card into ready state.
        }

        // read OCR and check CCS. Lets us know if the SD card is SC or HC/XC. 
        response = ReadOCR_R3(&g_SD_DeviceRegisters.OCR);
        
        // send CMD16, set block length to 512
#ifdef SD_DEBUG
        debug_printf" SD SendCmdWithR1Resp: SD_SET_BLOCKLEN[512] -> R1_IN_READY_STATUS\r\n");
#endif

        response = SD_SendCmdWithR1Resp(SD_SET_BLOCKLEN, 512, 0xFF, R1_IN_READY_STATUS);

        if(response != R1_IN_READY_STATUS)
        {
            break;
        }    

        //send CMD9  to get CSD
        BYTE regCSD[CSD_CID_LENGTH];

        BYTE C_SIZE_MULT = 0;

        BYTE TAAC, NSAC, MAX_TRAN_SPEED, READ_BL_LEN, SECTOR_SIZE;

        BOOL ERASE_BL_EN;

        UINT32 C_SIZE;

        UINT64 MemCapacity = 0; //total memory size, in unit of byte

        UINT32 Max_Trans_Speed = 0; //in unit of Hz
        

#ifdef SD_DEBUG
	    debug_printf(" SD SendCmdWithR1Resp: SD_SEND_CSD[0] -> SD_START_DATA_BLOCK_TOKEN=%02X\r\n",SD_START_DATA_BLOCK_TOKEN);
#endif

        response = SD_SendCmdWithR1Resp(SD_SEND_CSD, 0, 0xFF, SD_START_DATA_BLOCK_TOKEN);
    
        if(response != SD_START_DATA_BLOCK_TOKEN)
        {
            break;
        }
        else
        {
            // receive one sector data
            SPIRecvCount(regCSD, CSD_CID_LENGTH, 0);
    
            // receive 16 bit CRC
        SPISendByte(DUMMY);
        SPISendByte(DUMMY);
    
    
            //Table 5-5: TAAC Access Time Definition	
            //TAAC bit position code
            //2:0 time unit   0=1ns, 1=10ns, 2=100ns, 3=1µs, 4=10µs,5=100µs, 6=1ms, 7=10ms
            //6:3 time value  0=reserved, 1=1.0, 2=1.2, 3=1.3, 4=1.5, 5=2.0, 6=2.5, 7=3.0, 
            //                8=3.5,      9=4.0, A=4.5, B=5.0, C=5.5, D=6.0, E=7.0, F=8.0
            //  7 reserved
            TAAC = regCSD[1];
            
            // NSAC
            // Defines the worst case for the clock-dependant factor of the data access time. The unit for NSAC is 100
            // clock cycles. Therefore, the maximal value for the clock-dependent part of the data access time is 25.5
            // k clock cycles.
            //
            // The total access time NAC is the sum of TAAC and NSAC. It should be computed by the host for the
            // actual clock rate. The read access time should be interpreted as a typical delay for the first data bit of a
            // data block or stream.
            NSAC = regCSD[2];
            
            //TRAN_SPEED
            // The following table defines the maximum data transfer rate per one data line 
            // Table 5-6: Maximum Data Transfer Rate Definition
            //   2:0 transfer rate unit 0=100kbit/s, 1=1Mbit/s, 2=10Mbit/s, 3=100Mbit/s, 4... 7=reserved
            //   6:3 time value         0=reserved, 1=1.0, 2=1.2, 3=1.3, 4=1.5, 5=2.0, 6=2.5, 7=3.0, 
            //                          8=3.5, 9=4.0, A=4.5, B=5.0, C=5.5, D=6.0, E=7.0, F=8.0
            //   7   reserved
            // Notes:
            //  For current SD Memory Cards, this field shall be always 0_0110_010b (032h) which is equal to
            //  25 MHz - the mandatory maximum operating frequency of SD Memory Card.
            //  In High-Speed mode, this field shall be always 0_1011_010b (05Ah) which is equal to 50 MHz, and
            //  when the timing mode returns to the default by CMD6 or CMD0 command, its value will be 032h.
            MAX_TRAN_SPEED = regCSD[3];
            
            if(MAX_TRAN_SPEED == 0x32)
                Max_Trans_Speed = 25000000;   //normal mode
            else if(MAX_TRAN_SPEED == 0x5A)
                Max_Trans_Speed = 50000000;   //High-Speed mode
           
            
            // The maximum read data block length: 2 ^ READ_BL_LEN 
            READ_BL_LEN = regCSD[5] &0x0F;
            
            ERASE_BL_EN = ((regCSD[10] &0x40) == 0x00) ? FALSE : TRUE;
            SECTOR_SIZE = ((regCSD[10] &0x3F) << 1) | ((regCSD[11] &0x80) >> 7); //erase sector size
    
    
            if(regCSD[0] == 0x00)
            //SD spec version1.0
            {
    
#ifdef SD_DEBUG
		        debug_printf(" SD spec version 1.0\r\n");
#endif
                //UINT32 CCC = (regCSD[4]<<4) | ((regCSD[5] >> 4)  &0x0F);// Card Command Classes 01x110110101
                
                // This parameter is used to compute the user's data card capacity (not include the security protected area).
                C_SIZE = ((regCSD[6] &0x3) << 10) | (regCSD[7] << 2) | ((regCSD[8] &0xC0) >> 6);

                // C_SIZE_MULT : Table 5-12: Multiply Factor for the Device Size 
                // This parameter is used for coding a factor MULT for computing the total device size (see 'C_SIZE'). The
                // factor MULT is defined as 2^(C_SIZE_MULT+2).
                // C_SIZE_MULT MULT
                // 0 2^2 = 4
                // 1 2^3 = 8
                // 2 2^4 = 16
                // 3 2^5 = 32
                // 4 2^6 = 64
                // 5 2^7 = 128
                // 6 2^8 = 256
                // 7 2^9 = 512
                C_SIZE_MULT = ((regCSD[9] &0x03) << 1) | ((regCSD[10] &0x80) >> 7);

                // memory capacity = BLOCKNR * BLOCK_LEN
                // Where: 
                //  BLOCK_LEN = 2 ^ READ_BL_LEN where (READ_BL_LEN < 12)
                //  MULT = 2 ^ (C_SIZE_MULT+2) * (C_SIZE_MULT < 8)
                //  BLOCKNR = (C_SIZE+1) * MULT
                MemCapacity = (C_SIZE + 1)*(0x1 << (C_SIZE_MULT + 2))*(0x1 << READ_BL_LEN);
            }
            else
            //SD spec version2.0
            {
#ifdef SD_DEBUG
                debug_printf(" SD spec version 2.0\r\n");
#endif
                C_SIZE = ((regCSD[7] &0x3F) << 16) | (regCSD[8] << 8) | regCSD[9];
                MemCapacity = (((UINT64)(C_SIZE + 1)) << 19);// * (UINT64)512 * (UINT64)1024;
            }

#ifdef SD_DEBUG
            debug_printf(" SD MemCapacity=%llu\r\n",  MemCapacity);
#endif
            if (MemCapacity > MAX_SD_CARD_SIZE )
            {
#ifdef SD_DEBUG
                debug_printf(" SD: ##### LIMIT SIZE to %llu (HACK!)\r\n",MAX_SD_CARD_SIZE);
#endif
                MemCapacity = MAX_SD_CARD_SIZE;
            }
#ifdef SD_DEBUG
            debug_printf(" SD C_SIZE=%ld (%08lX) C_SIZE_MULT=%d\r\n",  C_SIZE,C_SIZE,C_SIZE_MULT);
            debug_printf(" SD ERASE_BL_EN=%d SECTOR_SIZE=%d Max_Trans_Speed=%d READ_BL_LEN=%d\r\n",
	                        ERASE_BL_EN,SECTOR_SIZE,Max_Trans_Speed,READ_BL_LEN);
#endif	
		
#if 0
            UINT8 crc = (CRC7Encode(0, regCSD, 15) << 1) | 1;

            if(crc != regCSD[15])
            {
#ifdef SD_DEBUG
                debug_printf(" Wrong CRC for CSD register!\r\n");
#endif
            }
#endif

            //Update SD config according to CSD register
            UINT32 SectorsPerBlock    = (ERASE_BL_EN == TRUE) ? 1 : (SECTOR_SIZE + 1);
            pDevInfo->BytesPerSector  = 512; // data bytes per sector is always 512
            pDevInfo->Size            = MemCapacity;
    
            BlockRegionInfo* pRegions = (BlockRegionInfo*)&pDevInfo->Regions[0];
            pRegions[0].BytesPerBlock = SectorsPerBlock * pDevInfo->BytesPerSector;
            pRegions[0].NumBlocks     = MemCapacity / pRegions[0].BytesPerBlock;
            
            BlockRange* pRanges   = (BlockRange*)&pRegions[0].BlockRanges[0];
    
            pRanges[0].StartBlock = 0;
            pRanges[0].EndBlock   = pRegions[0].NumBlocks-1;
        }

        //CMD55+ACMD51 to get SCR register
        BYTE regSCR[8];
    
        SD_SendCmdWithR1Resp(SD_APP_CMD, 0, 0xFF, R1_IN_READY_STATUS);
        response = SD_SendCmdWithR1Resp(SD_SEND_SCR, 0, 0xFF, SD_START_DATA_BLOCK_TOKEN);
    
        if(response != SD_START_DATA_BLOCK_TOKEN)
        {
            break;
        }
        else
        {
            // receive one sector data
            SPIRecvCount(regSCR, SCR_LENGTH, 0);
    
#ifndef SD_DEBUG
            SPISendByte(DUMMY);
            SPISendByte(DUMMY);
#else
            // receive 16 bit CRC
            BYTE crc1 = SPISendByte(DUMMY);
            BYTE crc2 = SPISendByte(DUMMY);
    
	        debug_printf(" SD SCR: %02X %02X %02X %02X %02X %02X %02X %02X - %02X %02X\r\n",
						   regSCR[0],regSCR[1],regSCR[2],regSCR[3],
						   regSCR[4],regSCR[5],regSCR[6],regSCR[7],
						   crc1,crc2						   
						   );
#endif
            g_SD_BL_Config.State_After_Erase = ((regSCR[1] &0x80) != 0x0);
        }

        //CMD10 to get CID

        BYTE regCID[16];

        BYTE ManufacturerCode;

        UINT16 OEMID;

        BYTE ProductName[5];
	       
        response = SD_SendCmdWithR1Resp(SD_SEND_CID, 0, 0xFF, SD_START_DATA_BLOCK_TOKEN);

        if(response != SD_START_DATA_BLOCK_TOKEN)
        {
            break;
        }
        else
        {
            // receive one sector data
            SPIRecvCount(regCID, CSD_CID_LENGTH, 0);

#ifndef SD_DEBUG
            SPISendByte(DUMMY);
            SPISendByte(DUMMY);
#else
            // receive 16 bit CRC
            BYTE crc1 = SPISendByte(DUMMY);
            BYTE crc2 = SPISendByte(DUMMY);


	        debug_printf(" SD CID: %02X %02X %02X %02X %02X %02X %02X %02X - " 
	                      "%02X %02X %02X %02X %02X %02X %02X %02X - %02X %02X\r\n",
						   regCID[0],regCID[1],regCID[2],regCID[3],
						   regCID[4],regCID[5],regCID[6],regCID[7],
						   regCID[8],regCID[9],regCID[10],regCID[11],
						   regCID[12],regCID[13],regCID[14],regCID[15],
						   crc1,crc2						   
						   );
#endif
        }

        ManufacturerCode = regCID[0];

        memcpy(&OEMID, &regCID[1], 2);

        memcpy(&ProductName, &regCID[3], 5);
	    
        isInitialised = true;
  
    } while (FALSE);


    // Clean up and deselect SD card.
    
    SD_CsSetHigh();
    CPU_SPI_Xaction_Stop(g_SD_BL_Config.SPI);
    g_SD_BL_Config.SPI.Clock_RateKHz = clkNormal;
    
    return isInitialised;
}

BYTE SD_BS_Driver::SD_Cmd_GO_IDLE_STATE()
{
    BYTE response;
    int i;
    
    SD_CsSetHigh(); // SET SD Chip Select (CS) inactive
    
    //need 74 clock for initialize
    for(i = 0; i < 10; i++)
    {
        SPISendByte(DUMMY);
    }
    
    // need CS low to enter SPI mode
    SD_CsSetLow();
    
    // send CMD0, card should enter IDLE state
    for(i = 0; i < 10; i++)
    {
        response = SD_SendCmdWithR1Resp(SD_GO_IDLE_STATE, 0, 0x95, R1_IN_IDLE_STATUS);
        if(response == R1_IN_IDLE_STATUS)
            break;
    }
#if defined(SD_DEBUG)
    if(response != R1_IN_IDLE_STATUS)
    {
        debug_printf(" SD Error. NOT Reset to Idle mode.\r\n");
    }
    else
    {
        debug_printf(" SD in Idle mode\r\n");
    }
#endif

    return response;
}


// OCR BUSY bit is active LOW.
BOOL SD_BS_Driver::Get_OCR_BUSY()
{
    return (g_SD_DeviceRegisters.OCR & OCR_BUSY_BIT) == 0;
}

BOOL SD_BS_Driver::Get_OCR_CCS()
{
    return (g_SD_DeviceRegisters.OCR & OCR_CCS_BIT) != 0;
}

BYTE SD_BS_Driver::ReadOCR_R3(UINT32* pOCR)
{
    *pOCR=0;
     
    // SD_SEND_OCR
    BYTE R3response = SD_SendCmdWithR3Resp(SD_READ_OCR, 0, pOCR);
    
#ifdef SD_DEBUG
    debug_printf(" SD %s OCR=%08X : BUSY=%d CCS=%d\r\n",(Get_OCR_CCS() ? "HC/XC" : "SC"),
                   g_SD_DeviceRegisters.OCR, Get_OCR_BUSY(), Get_OCR_CCS()); 
#endif                   
    return R3response;
}

// Purpose:
//  Check if the SD card supports this command. i.e. its a version 2.0+ card.
//  Check if the signal voltage levels on interface match card supported voltages
// 
// Version: 
//  This command is added in spec version 2.00
//
// Notes: 
//  Sends SD Memory Card interface condition that includes host supply voltage information and asks the
//  accessed card whether card can operate in supplied voltage range.
//  Reserved bits shall be set to '0'.
//    [31:12]Reserved bits
//    [11:8]supply voltage(VHS)
//    [7:0]check pattern

BOOL SD_BS_Driver::SD_Cmd_SEND_IF_COND(BOOL isLowVoltageRequired, BOOL *pIs_SD_version1)
{
    //send CMD8, check voltage range support and V1.0 vs. V2.0 API
    UINT32 CMD8_Arg = CMD8_CHECK_PATTERN;
    BYTE support_voltage = 0;
    UINT32 supply_voltage;
   
   
    if(isLowVoltageRequired == TRUE)
        supply_voltage = 2;
    else
        supply_voltage = 1;
   
    CMD8_Arg |= (supply_voltage << 8);
    BYTE R7response = SD_SendCmdWithR7Resp(SD_SEND_IF_COND, CMD8_Arg, &support_voltage);
    // check if command was successful?
    if(R7response == R7_ILLEGAL_COOMMAND)
    {
        // V1.0 cards return illegal command response.
   
#ifdef SD_DEBUG
	    debug_printf(" SD spec version 1.0 [Assume SD or MMC 3.0V/3.3V]\r\n");
#endif
	 
        support_voltage = 1; // Assume SD or MMC 3.0 V or 3.3 V 
        *pIs_SD_version1 = TRUE;
    }
    else if (R7response == R7_IN_IDLE_STATUS)
    {	
        // as it responded to CMD8, must be type 2 card!

#ifdef SD_DEBUG
	    debug_printf(" SD spec version 2.0\r\n");
#endif

        *pIs_SD_version1 = FALSE;
    }
  
    if(support_voltage != supply_voltage) 
    {
	    return FALSE;
	}
  
    return TRUE;
}
  
BOOL SD_BS_Driver::SD_Set_In_READY_STATUS(BOOL isHC_XC_Supported)
{  
    int i;
    BYTE response; 
    
    for(i=0; i<0x7fff; i++)
    {
        //send CMD55 + ACMD41 until return 0x00 for type 1 cards
        SD_SendCmdWithR1Resp(SD_APP_CMD, 0, 0xFF, R1_IN_IDLE_STATUS);

        response = SD_SendCmdWithR1Resp(SD_SEND_OP_COND, (isHC_XC_Supported ? CMD41_HCS_PATTERN : 0), 0xFF, R1_IN_READY_STATUS);

        if(response == R1_IN_READY_STATUS)
        {
            return TRUE;
        }
        
        // If HC or XC supported, try the V2 command
        if (isHC_XC_Supported)
        {
            // use v2.0 command
            response = SD_SendCmdWithR1Resp(SD_V2_SEND_OP_COND, 0, 0xFF, R1_IN_READY_STATUS);
            if(response == R1_IN_READY_STATUS)
            {
                return TRUE;
            }
        }
    }
  

#if defined(SD_DEBUG)
    debug_printf(" SD: Card Not in READY_STATUS %d\r\n",response);
#endif
  
    return FALSE;
}


void SD_BS_Driver::InsertISR(GPIO_PIN Pin, BOOL PinState, void* Param)
{
    FS_MountVolume("SD1", 0, 0, g_SD_BL_Config.Device);
}

void SD_BS_Driver::EjectISR(GPIO_PIN Pin, BOOL PinState, void* Param)
{
    FS_UnmountVolume(g_SD_BL_Config.Device);
}

BOOL SD_BS_Driver::ChipUnInitialize(void *context)
{
    return TRUE;
}

BOOL SD_BS_Driver::ReadProductID(void *context, BYTE *ManufacturerCode, BYTE *OEMID, BYTE *ProductName)
{
    BYTE regCID[16];
    BYTE response;
   
  	GLOBAL_LOCK(irq);
    CPU_SPI_Xaction_Start(g_SD_BL_Config.SPI);

    // enable SD card
    SD_CsSetLow();

    //CMD10 to CID
    response = SD_SendCmdWithR1Resp(SD_SEND_CID, 0, 0xFF, SD_START_DATA_BLOCK_TOKEN);

    if(response != SD_START_DATA_BLOCK_TOKEN)
    {
        SD_CsSetHigh();
        CPU_SPI_Xaction_Stop(g_SD_BL_Config.SPI);
        return FALSE;
    }
    else
    {
        // receive one sector data
        SPIRecvCount(regCID, CSD_CID_LENGTH, 0);

#ifndef SD_DEBUG
        SPISendByte(DUMMY);
        SPISendByte(DUMMY);
#else
        // receive 16 bit CRC
        BYTE crc1 = SPISendByte(DUMMY);
        BYTE crc2 = SPISendByte(DUMMY);

	    debug_printf(" SD ReadProductID CID: %02X %02X %02X %02X %02X %02X %02X %02X - " 
	                      "%02X %02X %02X %02X %02X %02X %02X %02X - %02X %02X\r\n",
						   regCID[0],regCID[1],regCID[2],regCID[3],
						   regCID[4],regCID[5],regCID[6],regCID[7],
						   regCID[8],regCID[9],regCID[10],regCID[11],
						   regCID[12],regCID[13],regCID[14],regCID[15],
						   crc1,crc2						   
						   );
#endif
    }

    *ManufacturerCode = regCID[0];

    memcpy(OEMID, &regCID[1], 2);

    memcpy(ProductName, &regCID[3], 5);

    CPU_SPI_Xaction_Stop(g_SD_BL_Config.SPI);

    // disable SD card
    SD_CsSetHigh();

    return TRUE;
}

const BlockDeviceInfo *SD_BS_Driver::GetDeviceInfo(void *context)
{
    SD_BLOCK_CONFIG *config = (SD_BLOCK_CONFIG*)context;

    return config->BlockDeviceInformation;
}

BOOL SD_BS_Driver::ReadSector(SectorAddress sectorAddress, UINT32 Offset, UINT32 NumBytes, BYTE* pSectorBuff, UINT32 BytesPerSector)
{
    BYTE response;
    UINT32 leftOver;
    bool   fResponse = false;

    NumBytes = (NumBytes + Offset > BytesPerSector ? BytesPerSector - Offset : NumBytes);
    leftOver = BytesPerSector - (Offset + NumBytes);

    for(int i=0; i<10; i++)
    {
        // enable SD card
        SD_CsSetLow();

        // send CMD17 and wait for DATA_BLOCK_TOKEN
        response = SD_SendCmdWithR1Resp(SD_READ_SINGLE_BLOCK, sectorAddress << 9, 0xff, SD_START_DATA_BLOCK_TOKEN, 10000);

        if(response == SD_START_DATA_BLOCK_TOKEN)
        {
            fResponse = true;
            break;
        }

        SD_CsSetHigh();
    }

    if(fResponse)
    {
        // receive one sector data
        SPIRecvCount(pSectorBuff, BytesPerSector, Offset);

        while(leftOver--)
        {
            SPISendByte(DUMMY);
        }

        // receive 16 bit CRC
        SPISendByte(DUMMY);
        SPISendByte(DUMMY);		
#if defined(SD_DEBUG)		
		DumpSector(pSectorBuff, BytesPerSector, Offset);
#endif
    }
    else
    //can't get valid response after CMD17
    {
        SD_CsSetHigh();
        return FALSE;
    }

    //disable select SD card
    SD_CsSetHigh();
    return TRUE;
}
#define DUMP_BUFFER_SIZE 16384

#if defined(SD_DEBUG)	
void DumpSector(BYTE* pSectorBuff, UINT32 BytesPerSector, UINT32 Offset)
{
	debug_printf("DumpSector: %p %d %d\r\n",pSectorBuff,BytesPerSector,Offset);

    char pszBuffer[DUMP_BUFFER_SIZE];
	BYTE *stackbytes = pSectorBuff+Offset;


	
	for(UINT32 i = 0; i < (BytesPerSector / 16); i++)
	{	
		size_t len = 0;
		memset(pszBuffer,0,sizeof(pszBuffer));
		
		len+=hal_snprintf(&pszBuffer[len],DUMP_BUFFER_SIZE-len,"[0x%08x] :", (UINT32)&stackbytes[i*16]);
		for(UINT32 j = 0; j < 16; j++)
		{
			// don't cause a data abort here!
			if((UINT32) (/*&stackbytes[*/i*16 + j/*]*/) < (UINT32) BytesPerSector)
			{
				len+=hal_snprintf(&pszBuffer[len],DUMP_BUFFER_SIZE-len," %02x", stackbytes[i*16 + j]);
			}
			else
			{
				len+=hal_snprintf(&pszBuffer[len],DUMP_BUFFER_SIZE-len,"   ");
			}
		}
		len+=hal_snprintf(&pszBuffer[len],DUMP_BUFFER_SIZE-len," ");

		for(UINT32 j = 0; j < 16; j++)
		{
			if((UINT32) (/*&stackbytes[*/i*16 + j/*]*/) < (UINT32) BytesPerSector)
			{
				len+=hal_snprintf(&pszBuffer[len],DUMP_BUFFER_SIZE-len,"%c", (stackbytes[i*16 + j] >= ' ') ? stackbytes[i*16 + j] : '.');
			}
		}
		len+=hal_snprintf(&pszBuffer[len],DUMP_BUFFER_SIZE-len,"\r\n");
		
		debug_printf(pszBuffer);
	}
	
}
#endif

BOOL SD_BS_Driver::Read(void *context, ByteAddress phyAddress, UINT32 NumBytes, BYTE *pSectorBuff)
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();
    UINT32 RangeIndex;
    UINT32 RegionIndex;
    UINT32 BytesPerSector;

    BLOCK_CONFIG* pConfig = (BLOCK_CONFIG*)context;

    if(pConfig->BlockDeviceInformation->FindRegionFromAddress(phyAddress, RegionIndex, RangeIndex))
    {
        ByteAddress StartSector = pConfig->BlockDeviceInformation->PhysicalToSectorAddress( &pConfig->BlockDeviceInformation->Regions[RegionIndex], phyAddress);

        BytesPerSector = pConfig->BlockDeviceInformation->BytesPerSector;

        CHIP_WORD *pBuf = (CHIP_WORD*)pSectorBuff;

        UINT32 offset = phyAddress - (StartSector * pConfig->BlockDeviceInformation->BytesPerSector);

        UINT32 bytes  = (NumBytes + offset > BytesPerSector ? BytesPerSector - offset : NumBytes);

		    GLOBAL_LOCK(irq);

        CPU_SPI_Xaction_Start(g_SD_BL_Config.SPI);

        while(NumBytes > 0)
        {
            if(!ReadSector(StartSector, offset, bytes, pBuf, BytesPerSector))
            {
                CPU_SPI_Xaction_Stop(g_SD_BL_Config.SPI);

                return FALSE;
            }
            
            offset    = 0;
            pBuf      = (CHIP_WORD*)((UINT32)pBuf + bytes);
            NumBytes -= bytes;
            StartSector++;

            bytes = __min(BytesPerSector, NumBytes);
        }

        CPU_SPI_Xaction_Stop(g_SD_BL_Config.SPI);
        
        return TRUE;
    }
    else
    {
        return FALSE;
    }
}

BOOL SD_BS_Driver::Write(void *context, ByteAddress phyAddr, UINT32 NumBytes, BYTE *pSectorBuff, BOOL ReadModifyWrite )
{
    NATIVE_PROFILE_PAL_FLASH();

    return WriteX( context, phyAddr, NumBytes, pSectorBuff, ReadModifyWrite, TRUE );
}

BOOL SD_BS_Driver::Memset(void *context, ByteAddress phyAddr, UINT8 Data, UINT32 NumBytes )
{
    NATIVE_PROFILE_PAL_FLASH();

    return WriteX( context, phyAddr, NumBytes, &Data, TRUE, FALSE );
}

BOOL SD_BS_Driver::WriteX(void *context, ByteAddress phyAddr, UINT32 NumBytes, BYTE *pSectorBuff, BOOL ReadModifyWrite, BOOL fIncrementDataPtr )
{
    NATIVE_PROFILE_PAL_FLASH();

    UINT32 RangeIndex;
    UINT32 RegionIndex;
    UINT32 BytesPerSector;
    UINT32 offset;
    UINT32 bytes;
    BYTE response;

    BLOCK_CONFIG* pConfig = (BLOCK_CONFIG*)context;

    CHIP_WORD *pData, *pWrite;

    // find the corresponding region     
    if(!pConfig->BlockDeviceInformation->FindRegionFromAddress(phyAddr, RegionIndex, RangeIndex))
        return FALSE;

    ByteAddress StartSector = pConfig->BlockDeviceInformation->PhysicalToSectorAddress( &pConfig->BlockDeviceInformation->Regions[RegionIndex], phyAddr);

    pData = (CHIP_WORD*)pSectorBuff;
    BytesPerSector = pConfig->BlockDeviceInformation->BytesPerSector;

    GLOBAL_LOCK(irq);

    offset = phyAddr - (StartSector * BytesPerSector);

    bytes = (NumBytes + offset > BytesPerSector ? BytesPerSector - offset : NumBytes);

    CPU_SPI_Xaction_Start(g_SD_BL_Config.SPI);

    while(NumBytes > 0)
    {
        // if we are using memset, or if the bytes written are less than the BytesPerSector then do read/modify/write
        if(!fIncrementDataPtr || (bytes != BytesPerSector))
        {   
            if(bytes != BytesPerSector)
            {
                if(!ReadSector(StartSector, 0, BytesPerSector, s_sectorBuff, BytesPerSector))
                {
                    CPU_SPI_Xaction_Stop(g_SD_BL_Config.SPI);
                    return FALSE;
                }

            }
            
            pWrite = (CHIP_WORD*)&s_sectorBuff[0];

            if(fIncrementDataPtr)
            {
                memcpy(&pWrite[offset], pData, bytes);
            }
            else
            {
                memset(&pWrite[offset], *pData, bytes);
            }
        }
        else
        {
            pWrite = pData;
        }

        // select SD CS
        SD_CsSetLow();
        
        // send CMD24 --read single block data
        response = SD_SendCmdWithR1Resp(SD_WRITE_SINGLE_BLOCK, StartSector << 9, 0xff, R1_IN_READY_STATUS);

        if(response == R1_IN_READY_STATUS)
        {
            SPISendByte(SD_START_DATA_BLOCK_TOKEN); // send DATA_BLOCK_TOKEN

            // send data
            SPISendCount(pWrite, BytesPerSector);

            // send CRC
            SPISendByte(0xff);
            SPISendByte(0xff);

            // wait for end of write busy
            response = SD_CheckBusy();
        }

        //disable SD card CS
        SD_CsSetHigh();

        if(fIncrementDataPtr) pData = (CHIP_WORD*)((UINT32)pData + bytes);

        NumBytes   -= bytes;
        offset      = 0;
        StartSector++;
        bytes = __min(BytesPerSector, NumBytes);        
    }

    CPU_SPI_Xaction_Stop(g_SD_BL_Config.SPI);

    return TRUE;

}

BOOL SD_BS_Driver::GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    return TRUE;
}

BOOL SD_BS_Driver::SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    return TRUE;
}

BOOL SD_BS_Driver::IsBlockErased(void *context, ByteAddress phyAddress, UINT32 BlockLength)
{

    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    UINT32 RegionIndex;
    UINT32 RangeIndex;
    UINT32 SectorsPerBlock;
    UINT32 BytesPerSector;

    BLOCK_CONFIG* pConfig = (BLOCK_CONFIG*)context;

    // this is static buffer, as the driver is current tailor for SD, a page size is 2048 bytes.
    BYTE *pSectorBuff = s_sectorBuff;


    BYTE state_After_Erase = g_SD_BL_Config.State_After_Erase ? 0xFF : 0x00;

    if(!pConfig->BlockDeviceInformation->FindRegionFromAddress(phyAddress, RegionIndex, RangeIndex))
        return FALSE;

    ByteAddress StartSector = pConfig->BlockDeviceInformation->PhysicalToSectorAddress( &pConfig->BlockDeviceInformation->Regions[RegionIndex], phyAddress);

    const BlockRegionInfo* pRegion = &pConfig->BlockDeviceInformation->Regions[RegionIndex];

    // as the input arg Sector may not be the startSector address of a block,
    // we need to recalculate it.
    BytesPerSector  = pConfig->BlockDeviceInformation->BytesPerSector;
    SectorsPerBlock = (pRegion->BytesPerBlock / BytesPerSector);

    StartSector = (StartSector / SectorsPerBlock) * SectorsPerBlock;
    
    for(UINT32 i = 0; i < SectorsPerBlock; i++)
    {
        SD_BS_Driver::Read(context, StartSector, BytesPerSector, pSectorBuff);
        for(UINT32 j = 0; j < BytesPerSector; j++)
        {
            if(pSectorBuff[j] != state_After_Erase)
            {
                return FALSE;
            }
        }
    }
    return TRUE;
}


BOOL SD_BS_Driver::EraseSectors(SectorAddress Address, INT32 SectorCount)
{
    BYTE response;

    SD_CsSetLow(); // cs low

    //send ERASE_WR_BLK_START command
    response = SD_SendCmdWithR1Resp(SD_ERASE_WR_BLK_START, Address << 9, 0xff, R1_IN_READY_STATUS);

    if(response != R1_IN_READY_STATUS)
    {
        SD_CsSetHigh();
        return FALSE;
    }

    //send ERASE_WR_BLK_END command
    response = SD_SendCmdWithR1Resp(SD_ERASE_WR_BLK_END, (Address + SectorCount - 1) << 9, 0xff, R1_IN_READY_STATUS);

    if(response != R1_IN_READY_STATUS)
    {
        SD_CsSetHigh();
        return FALSE;
    }

    // send erase command
    response = SD_SendCmdWithR1Resp(SD_ERASE, 0xffffffff, 0xff, R1_IN_READY_STATUS);

    if(response != R1_IN_READY_STATUS)
    {
        SD_CsSetHigh();
        return FALSE;
    }

    // wait for IDLE
    SD_CheckBusy();

    SD_CsSetHigh();

    return TRUE;

}

BOOL SD_BS_Driver::EraseBlock(void *context, ByteAddress phyAddr)
{
    NATIVE_PROFILE_HAL_DRIVERS_FLASH();

    UINT32 RangeIndex;
    UINT32 RegionIndex;

    BLOCK_CONFIG* pConfig = (BLOCK_CONFIG*)context;

    if(!pConfig->BlockDeviceInformation->FindRegionFromAddress(phyAddr, RegionIndex, RangeIndex))
        return FALSE;

    const BlockRegionInfo* pRegion = &pConfig->BlockDeviceInformation->Regions[RegionIndex];

    ByteAddress StartSector = pConfig->BlockDeviceInformation->PhysicalToSectorAddress( pRegion, phyAddr );

    UINT32 SectorsPerBlock = pRegion->BytesPerBlock / pConfig->BlockDeviceInformation->BytesPerSector;

    SectorAddress SectorAddress = (StartSector / SectorsPerBlock) * SectorsPerBlock;

	GLOBAL_LOCK(irq);

    CPU_SPI_Xaction_Start(g_SD_BL_Config.SPI);

    EraseSectors(SectorAddress, SectorsPerBlock);

    CPU_SPI_Xaction_Stop(g_SD_BL_Config.SPI);

    return TRUE;

}

void SD_BS_Driver::SetPowerState(void *context, UINT32 State)
{
    // our flash driver is always Power ON
    return ;
}

UINT32 SD_BS_Driver::MaxSectorWrite_uSec(void *context)
{
    NATIVE_PROFILE_PAL_FLASH();

    SD_BLOCK_CONFIG *config = (SD_BLOCK_CONFIG*)context;

    return config->BlockDeviceInformation->MaxSectorWrite_uSec;
}

UINT32 SD_BS_Driver::MaxBlockErase_uSec(void *context)
{
    NATIVE_PROFILE_PAL_FLASH();

    SD_BLOCK_CONFIG *config = (SD_BLOCK_CONFIG*)context;

    return config->BlockDeviceInformation->MaxBlockErase_uSec;
}

