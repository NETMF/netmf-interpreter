////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_LARGEBUFFER_DECL_H_
#define _DRIVERS_LARGEBUFFER_DECL_H_ 1

///
///  Gets the size (in bytes) of the LargeBuffer data available in native code for the specified bufferId.  This method is called by the CLR interop in order
///  to allocate the appropriate size buffer for the call to LargeBuffer_NativeToManaged call.
///
///  returns the size in bytes of the current native LargeBuffer
///
INT32 LargeBuffer_GetNativeBufferSize( UINT16 bufferId );

///
///  Copies the native LargeBuffer data for the specified bufferId to the managed code buffer represented by parameter pData.  The data pointed to by 
///  pData should not be used outside the scope of this method as it may be reclaimed by managed code).
///
///  bufferId - The ID number of the LargeBuffer (this can be used to send and recieve different buffers to different parts of the system).
///  pData    - The managed code buffer
///  size       - The length in bytes of pData
///
void LargeBuffer_NativeToManaged( UINT16 bufferId, BYTE* pData, size_t size );

///
///  Copies the managed LargeBuffer data to native code for the specified bufferId.  This method is used by a managed application to send
///  large amounts of data to the native level.  The data pointed to by pData should not be used outside the scope of this method as it may
///  be reclaimed by managed code).
///
///  bufferId - The ID number of the LargeBuffer (this can be used to send and recieve different buffers to different parts of the system).
///  pData    - The managed code buffer - readonly
///  size       - The length in bytes of pData
///
void LargeBuffer_ManagedToNative( UINT16 bufferId, const BYTE* pData, size_t size );

#endif //_DRIVERS_LARGEBUFFER_DECL_H_

