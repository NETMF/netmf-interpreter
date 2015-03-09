//-----------------------------------------------------------------------------
//
//  <No description>
//
//  Microsoft dotNetMF Project
//  Copyright ©2001,2002 Microsoft Corporation
//  One Microsoft Way, Redmond, Washington 98052-6399 U.S.A.
//  All rights reserved.
//  MICROSOFT CONFIDENTIAL
//
//-----------------------------------------------------------------------------

#ifndef __MICROBOOTER_DECL_H__
#define __MICROBOOTER_DECL_H__

/////////////////////////////////////////////////////////////////////////////

#include <Tinyhal.h>
#include <MFUpdate_decl.h>

extern BOOL EnterMicroBooter(INT32& timeout);

extern UINT32 MicroBooter_ProgramMarker();

extern UINT32 MicroBooter_PrepareForExecution(UINT32 physicalEntryPointAddress);

extern const IUpdateStorageProvider** g_MicroBooter_UpdateStorageList;
extern UINT32                         g_MicroBooter_UpdateStorageListCount;

/////////////////////////////////////////////////////////////////////////////

#endif /* __MICROBOOTER_DECL_H__ */
