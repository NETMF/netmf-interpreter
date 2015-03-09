#include "tinyhal.h" 

//--//

#ifndef _LOOPBACK_DRIVER_H_
#define _LOOPBACK_DRIVER_H_ 1


struct LOOPBACK_Driver
{
    int m_interfaceNumber;

    //--//
    
    static int  Open   (   void          );
    static BOOL Close  (   void          );
    static BOOL Bind   (   void          );
};

#endif /* _LOOPBACK_DRIVER_H_ */
