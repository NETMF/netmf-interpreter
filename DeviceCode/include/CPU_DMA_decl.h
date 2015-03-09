////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_DMA_DECL_H_
#define _DRIVERS_DMA_DECL_H_ 1

//--//

BOOL DMA_Initialize   (                                                                                );
BOOL DMA_Uninitialize (                                                                                );
void DMA_MemCpy       ( void* dst, void*src, UINT32 size , BOOL async                                  );
void DMA_MemCpy2D     ( void* dst, void*src, UINT32 width, UINT32 height, UINT32 dataWidth, BOOL async );
void DMA_StartDummyDMA(                                                                                );

//--//

#endif // _DRIVERS_CMU_DECL_H_

