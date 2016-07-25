////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "spot_hardware_onewire_native.h"
#include "spot_hardware_onewire_native_Microsoft_SPOT_Hardware_OneWire.h"
#include "ownet.h"

using namespace Microsoft::SPOT::Hardware;

INT32 OneWire::TouchReset( CLR_RT_HeapBlock* pMngObj, HRESULT &hr )
{
  INT32 retVal = 0; 
 	UINT32 pin = Get__logicalPort(pMngObj); 
	retVal = owTouchReset(pin);
  return retVal;
}

INT32 OneWire::TouchBit( CLR_RT_HeapBlock* pMngObj, INT32 param0, HRESULT &hr )
{
  INT32 retVal = 0; 
	UINT32 pin = Get__logicalPort(pMngObj); 
	retVal = owTouchBit(pin, param0);
  return retVal;
}

INT32 OneWire::TouchByte( CLR_RT_HeapBlock* pMngObj, INT32 param0, HRESULT &hr )
{
  INT32 retVal = 0; 
	UINT32 pin = Get__logicalPort(pMngObj); 
	retVal = owTouchByte(pin, param0);
  return retVal;
}

INT32 OneWire::WriteByte( CLR_RT_HeapBlock* pMngObj, INT32 param0, HRESULT &hr )
{
  INT32 retVal = 0; 
	UINT32 pin = Get__logicalPort(pMngObj); 
	retVal = owWriteByte(pin, param0);
  return retVal;
}

INT32 OneWire::ReadByte( CLR_RT_HeapBlock* pMngObj, HRESULT &hr )
{
  INT32 retVal = 0; 
	UINT32 pin = Get__logicalPort(pMngObj); 
	retVal = owReadByte(pin);
  return retVal;
}

INT32 OneWire::AcquireEx( CLR_RT_HeapBlock* pMngObj, HRESULT &hr )
{
  INT32 retVal = 0; 
	UINT32 pin = Get__pin(pMngObj); 
	UINT32 logicalPort = Get__logicalPort(pMngObj);
  retVal = owAcquire(logicalPort, pin);
  return retVal;
}

INT32 OneWire::Release( CLR_RT_HeapBlock* pMngObj, HRESULT &hr )
{
  INT32 retVal = 0; 
	UINT32 pin = Get__logicalPort(pMngObj); 
	owRelease(pin);
  return retVal;
}

INT32 OneWire::First( CLR_RT_HeapBlock* pMngObj, INT8 param0, INT8 param1, HRESULT &hr )
{
  INT32 retVal = 0; 
	UINT32 pin = Get__logicalPort(pMngObj); 
	retVal = owFirst(pin, param0, param1);
  return retVal;
}

INT32 OneWire::Next( CLR_RT_HeapBlock* pMngObj, INT8 param0, INT8 param1, HRESULT &hr )
{
  INT32 retVal = 0; 
	UINT32 pin = Get__logicalPort(pMngObj); 
	retVal = owNext(pin, param0, param1);
  return retVal;
}

INT32 OneWire::SerialNum( CLR_RT_HeapBlock* pMngObj, CLR_RT_TypedArray_UINT8 param0, INT8 param1, HRESULT &hr )
{
  INT32 retVal = 0; 
	UINT32 pin = Get__logicalPort(pMngObj); 
	owSerialNum(pin, param0.GetBuffer(), param1);
  return retVal;
}

