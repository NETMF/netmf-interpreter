////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_OSTASK_DECL_H_
#define _DRIVERS_OSTASK_DECL_H_ 1

//--//

#include "ASyncProcCalls_decl.h"

//--//

typedef HAL_CALLBACK_FPN OSTASK_CALLBACK_FPN;
typedef HAL_CALLBACK     OSTASK_CALLBACK; 

//--//

struct OSTASK 
{

private:
    OSTASK_CALLBACK Callback;
    BOOL            Completed;

public:
    void Initialize( OSTASK_CALLBACK_FPN EntryPoint, void* Argument = NULL )
    {
        Completed = FALSE;
        Callback.Initialize( EntryPoint, Argument);
    }

    void SetArgument( void* Argument )
    {
        Callback.SetArgument( Argument );
    }

    OSTASK_CALLBACK_FPN GetEntryPoint() const { return Callback.GetEntryPoint(); }
    void*               GetArgument  () const { return Callback.GetArgument  (); }
    void                Execute      () const {        Callback.Execute      (); }
    void                SetCompleted ()       {        Completed = TRUE        ; }
    BOOL                HasCompleted () const { return Completed               ; }
};


//--//

BOOL OSTASK_Initialize     ( HAL_CALLBACK_FPN completed ); 
void OSTASK_Uninitialize   (                            ); 
BOOL OSTASK_Post           ( OSTASK* task               ); 
void OSTASK_SignalCompleted( OSTASK* task               ); 

//--//

#endif // _DRIVERS_OSTASK_DECL_H_
