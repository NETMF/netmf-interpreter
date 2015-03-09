// SPOT_USB_TEST.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"            


#define ONE_MEGABIT			1048576

typedef struct _OPERATION_CONTEXT
{
    _OPERATION_CONTEXT( )                          { _OPERATION_CONTEXT( true, -1);          }
    _OPERATION_CONTEXT( bool async )               { _OPERATION_CONTEXT( async, -1);         }
    _OPERATION_CONTEXT( bool async, int maxBytes ) { m_async = async; m_maxBytes = maxBytes; }

    bool              m_async;
    StringType        m_fileName;
    std::vector<char> m_pattern;
    unsigned int      m_maxBytes;
    unsigned int      m_bufferSize;
    unsigned int      m_cancellation;
} OPERATION_CONTEXT, *POPERATION_CONTEXT;

typedef void (*operation)(void*);

static GUID SpotUsbGuid = { 
    0x09343630L, 
    0xa794, 
    0x10ef, 
    0x33,
    0x4f, 
    0x82, 
    0xea, 
    0x33, 
    0x2c, 
    0x49, 
    0xf3
};


HANDLE enumerate( DWORD mode )
{
    HDEVINFO hDevInfo;
    SP_DEVICE_INTERFACE_DATA sp;
    char buffer[1024];
    PSP_DEVICE_INTERFACE_DETAIL_DATA detail;
    DWORD index;
    HANDLE h = NULL;

    hDevInfo = SetupDiGetClassDevs(&SpotUsbGuid, 
                                   NULL,
                                   NULL,
                                   DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);
     
    detail = (SP_DEVICE_INTERFACE_DETAIL_DATA *)&buffer[0];

    sp.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
    detail->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);

    index = 0;

    while(SetupDiEnumDeviceInterfaces(hDevInfo,
                                      NULL,
                                      &SpotUsbGuid,
                                      index++,
                                      &sp)){
          
        if (SetupDiGetDeviceInterfaceDetail(hDevInfo,
                                            &sp,
                                            detail,
                                            1024,
                                            NULL,
                                            NULL)){

            h = CreateFile(detail->DevicePath,
                           mode,
                           0,
                           NULL,
                           OPEN_EXISTING,
                           0,
                           NULL);
            break;
        }
    }
               
    SetupDiDestroyDeviceInfoList(hDevInfo);
     
    return h;
}

DWORD HandleError( DWORD error, HANDLE device, LPOVERLAPPED ov )
{
    DWORD res;

    switch( error ) 
    { 
        case ERROR_HANDLE_EOF: 
            res = 0;
            printf("ERROR_HANDLE_EOF\n");
            break;
    
        case ERROR_IO_PENDING: 
            if(ov)
            {
                if(!GetOverlappedResult( device, ov, &res, TRUE )) 
                {
                    res = 0;
                }
            }
            else
            {
                res = 0;
            }
            break;

        default:
            printf("Unspecified error: %08x\n", error);
            res = 0;
            break;
    }

    return res;
}


void write( void* context )
{
    HANDLE device, file;
    DWORD tick, bytes, count;

    POPERATION_CONTEXT opc = (POPERATION_CONTEXT)context;

    std::vector<char> buffer(opc->m_bufferSize);

    DWORD flags   = 0;
    HANDLE event  = NULL;
    OVERLAPPED ov;
        
    if(opc->m_async) 
    {
        flags |= FILE_FLAG_OVERLAPPED;
        event = CreateEvent( NULL, TRUE, FALSE, NULL );
        if(!event) throw ApplicationException();
        ZeroMemory( &ov, sizeof(OVERLAPPED) );
        ov.hEvent = event;
    }

    device = enumerate( GENERIC_WRITE );

    if(device == INVALID_HANDLE_VALUE) throw ApplicationException();

    bytes = 0;

    bool readFromFile = opc->m_fileName.compare(TEXT("")) != 0;

    if(readFromFile)
    {
        file = CreateFile(
                            opc->m_fileName.c_str(),
                            GENERIC_READ,
                            0,
                            NULL, 
                            OPEN_EXISTING, 
                            0, 
                            NULL
                        );
        if(file == INVALID_HANDLE_VALUE)
        {
            CloseHandle( device );
            throw ApplicationException();
        }
    }
    else
    {
        size_t skew = 0;

        size_t patternLength = opc->m_pattern.size();

        for(;;)
        {
            for(unsigned int i = 0; i < patternLength; ++i)
            {
                if(buffer.size() > i + skew)
                {
                    buffer[i + skew] = (char)opc->m_pattern[i];
                }
                else
                {
                    break;
                }
            }
            if(buffer.size() <= i + skew) break;

            skew += patternLength;
        }

        count = (DWORD)buffer.size();
    }


    DWORD adjust;
    DWORD totalAdjust = 0;

    tick  = GetTickCount();

    for(;;)
    {
        if(readFromFile)
        {
            adjust = GetTickCount();

            if (!ReadFile(file, &buffer[0], (DWORD)buffer.size(), &count, NULL)) 
            {
                DWORD err = GetLastError();

                switch(err)
                {
                case ERROR_HANDLE_EOF:
                    printf("End of input file reached\n");
                    break;
                default:
                    printf("Unpecified error when reading input file: %d\n", err);
                    break;
                }
                break;
            }

            if(count == 0) break;

            totalAdjust += GetTickCount() - adjust;
        }

        if(!WriteFile( device, &buffer[0],  count, &count, opc->m_async ? &ov : NULL ))
        {
            if(opc->m_cancellation)
            {
                CancelIo( device );
            }
            else 
            {
                if(0 == (count = HandleError( GetLastError(), device, &ov ))) break;
            }
        }

        bytes += count;

        if( bytes >= opc->m_maxBytes) break;
    }

    tick = (GetTickCount() - tick) - totalAdjust;

    printf("bytes tranferred: %d; tranfer time: %g s; tranfer rate: %g Mbs", bytes, (double)(tick ? tick : 1 )/1000, (((double)(bytes * 8)/(double)( tick ? tick : 1 ))*1000)/ONE_MEGABIT );

    CloseHandle( device );
    if(readFromFile)
    {
        CloseHandle( file );
    }
}


void read( void* context )
{
    HANDLE device;
    DWORD tick, bytes, count;

    POPERATION_CONTEXT opc = (POPERATION_CONTEXT)context;

    std::vector<char> buffer(opc->m_bufferSize + 1);

    DWORD flags   = 0;
    HANDLE event  = NULL;
    OVERLAPPED ov;

        
    if(opc->m_async) 
    {
        flags |= FILE_FLAG_OVERLAPPED;
        event = CreateEvent( NULL, TRUE , FALSE, NULL );
        if(!event) throw ApplicationException();
        ZeroMemory( &ov, sizeof(OVERLAPPED) );
        ov.hEvent = event;
    }

    device = enumerate( GENERIC_READ );

    if (device == INVALID_HANDLE_VALUE) throw ApplicationException();


    bytes = 0;

    tick = GetTickCount();

    for(;;)
    {
        if (!ReadFile(device, &buffer[0], opc->m_bufferSize, &count, opc->m_async ? &ov : NULL ))
        {   
            if(0 == ( count = HandleError( GetLastError(), device, &ov ))) break;
        }
        
        buffer[count] = '\0';
        printf("read: <%s>\n", &buffer[0]);

        bytes += count;

        if(bytes >= opc->m_maxBytes) break;
    }

    tick = GetTickCount() - tick;

    printf("bytes tranferred: %d; tranfer time: %g s; tranfer rate: %g Mbs", bytes, (double)(tick ? tick : 1 )/1000, (((double)(bytes * 8)/(double)( tick ? tick : 1 ))*1000)/ONE_MEGABIT );

    CloseHandle(device);
}




 


void read_write( void* context )
{
    HANDLE device, file;
    DWORD tick, bytes, count;

    POPERATION_CONTEXT opc = (POPERATION_CONTEXT)context;

    std::vector<char> rdbuffer(opc->m_bufferSize + 1);
    std::vector<char> wrbuffer(opc->m_bufferSize + 1);

    DWORD flags   = 0;
    HANDLE event  = NULL;
    OVERLAPPED ov;
        
    if(opc->m_async) 
    {
        flags |= FILE_FLAG_OVERLAPPED;
        event = CreateEvent( NULL, TRUE, FALSE, NULL );
        if(!event) throw ApplicationException();
        ZeroMemory( &ov, sizeof(OVERLAPPED) );
        ov.hEvent = event;
    }

    device = enumerate( GENERIC_READ | GENERIC_WRITE );

    if(device == INVALID_HANDLE_VALUE) throw ApplicationException();

    bytes = 0;

    bool readFromFile = opc->m_fileName.compare(TEXT("")) != 0;

    if(readFromFile)
    {
        file = CreateFile(
                            opc->m_fileName.c_str(),
                            GENERIC_READ,
                            0,
                            NULL, 
                            OPEN_EXISTING, 
                            0, 
                            NULL
                        );
        if(file == INVALID_HANDLE_VALUE)
        {
            CloseHandle( device );
            throw ApplicationException();
        }
    }
    else
    {
        size_t skew = 0;

        size_t patternLength = opc->m_pattern.size();

        for(;;)
        {
            for(unsigned int i = 0; i < patternLength; ++i)
            {
                if(wrbuffer.size() > i + skew)
                {
                    wrbuffer[i + skew] = (char)opc->m_pattern[i];
                }
                else
                {
                    break;
                }
            }
            if(wrbuffer.size() <= i + skew) break;

            skew += patternLength;
        }

        count = (DWORD)wrbuffer.size()-1;
    }


    DWORD adjust;
    DWORD totalAdjust = 0;

    tick  = GetTickCount();

    for(;;)
    {
        if(readFromFile)
        {
            adjust = GetTickCount();

            if (!ReadFile(file, &wrbuffer[0], opc->m_bufferSize, &count, NULL)) 
            {
                DWORD err = GetLastError();

                switch(err)
                {
                case ERROR_HANDLE_EOF:
                    printf("End of input file reached\n");
                    break;
                default:
                    printf("Unpecified error when reading input file: %d\n", err);
                    break;
                }
                break;
            }

            if(count == 0) break;

            totalAdjust += GetTickCount() - adjust;
        }

        if(!WriteFile( device, &wrbuffer[0],  count, &count, opc->m_async ? &ov : NULL ))
        {
            if(opc->m_cancellation)
            {
                CancelIo( device );
            }
            else 
            {
                if(0 == (count = HandleError( GetLastError(), device, &ov ))) break;
            }
        }
printf("finsihed write-count %d\n ",count);
        for(int i=0; i<=opc->m_bufferSize;i++)
            rdbuffer[i]=0;
printf("read: <%s>\n", &rdbuffer[0]);
        bytes += count;

        if (!ReadFile(device, &rdbuffer[0], opc->m_bufferSize, &count, opc->m_async ? &ov : NULL ))
        {   
            if(0 == ( count = HandleError( GetLastError(), device, &ov ))) break;
        }
        
        rdbuffer[count] = '\0';
        printf(" read : <");
        for (i = 0; i< count; i++)
            printf("%02x", rdbuffer[i]);
        printf(">\n");
   //     bytes += count;



        if( bytes >= opc->m_maxBytes) break;
    }

    tick = (GetTickCount() - tick) - totalAdjust;

    printf("bytes tranferred: %d; tranfer time: %g s; tranfer rate: %g Mbs", bytes, (double)(tick ? tick : 1 )/1000, (((double)(bytes * 8)/(double)( tick ? tick : 1 ))*1000)/ONE_MEGABIT );

    CloseHandle( device );
    if(readFromFile)
    {
        CloseHandle( file );
    }
}

void auto_read_write( void* context )
{
    HANDLE device, file;
    DWORD tick, bytes, count, Rdcount,tmpcount;
    char WrCmd[20];
    char BufLen[5];
    unsigned int testcount;

    POPERATION_CONTEXT opc = (POPERATION_CONTEXT)context;

    std::vector<char> rdbuffer(opc->m_bufferSize + 1);
    std::vector<char> wrbuffer(opc->m_bufferSize + 1);

    DWORD flags   = 0;
    HANDLE event  = NULL;
    OVERLAPPED ov;

    

    if(opc->m_async) 
    {
        flags |= FILE_FLAG_OVERLAPPED;
        event = CreateEvent( NULL, TRUE, FALSE, NULL );
        if(!event) throw ApplicationException();
        ZeroMemory( &ov, sizeof(OVERLAPPED) );
        ov.hEvent = event;
    }

    device = enumerate( GENERIC_READ | GENERIC_WRITE );

    if(device == INVALID_HANDLE_VALUE) throw ApplicationException();

    bytes = 0;

    bool readFromFile = opc->m_fileName.compare(TEXT("")) != 0;

    if(readFromFile)
    {
        file = CreateFile(
                            opc->m_fileName.c_str(),
                            GENERIC_READ,
                            0,
                            NULL, 
                            OPEN_EXISTING, 
                            0, 
                            NULL
                        );
        if(file == INVALID_HANDLE_VALUE)
        {
            CloseHandle( device );
            throw ApplicationException();
        }
        if (!ReadFile(file, &wrbuffer[0], opc->m_bufferSize, &count, NULL)) 
        {
            DWORD err = GetLastError();

            switch(err)
            {
            case ERROR_HANDLE_EOF:
                printf("End of input file reached\n");
                break;
            default:
                printf("Unpecified error when reading input file: %d\n", err);
                break;
            }
         }
    }
    else
    {
        size_t skew = 0;

        size_t patternLength = opc->m_pattern.size();

        for(;;)
        {
            for(unsigned int i = 0; i < patternLength; ++i)
            {
                if(wrbuffer.size() > i + skew)
                {
                    wrbuffer[i + skew] = (char)opc->m_pattern[i];
                }
                else
                {
                    break;
                }
            }
            if(wrbuffer.size() <= i + skew) break;

            skew += patternLength;
        }

        count = (DWORD)wrbuffer.size()-1;
    }


    DWORD adjust;
    DWORD totalAdjust = 0;

    tick  = GetTickCount();
    testcount=0;
    if( count >1024)
        count = 1024;
    _itoa(count,BufLen,10);
    sprintf(WrCmd,"%s%s%s","READ_EPBS",BufLen,"**\0");

    for(;;)
    {

        // Device side buffer size = 1024

        // write command to test_usb to read data with size of "count"
        WriteFile( device, &WrCmd[0],  strlen(WrCmd), &tmpcount, opc->m_async ? &ov : NULL );

        wrbuffer[0]++;
        if (wrbuffer[0] >'9'){
            wrbuffer[0]='0';
        }
        wrbuffer[1]=wrbuffer[0]+1;
        
   //     printf(" after WrCmd \n ");

        if(!WriteFile( device, &wrbuffer[0],  count, &count, opc->m_async ? &ov : NULL ))
        {
            if(opc->m_cancellation)
            {
                CancelIo( device );
            }
            else 
            {
                if(0 == (count = HandleError( GetLastError(), device, &ov ))) break;
            }
        }
   //     printf(" after Wrdata \n");
  

        if (!ReadFile(device, &rdbuffer[0], count, &Rdcount, opc->m_async ? &ov : NULL ))
        {   
            if(0 == ( count = HandleError( GetLastError(), device, &ov ))) break;
        }
        
        rdbuffer[count] = '\0';
        //printf("read: <%s>\n", &rdbuffer[0]);

        // check the return data
        if ( count != Rdcount)
            printf(" error reading data back expect%d, return\n",count,Rdcount);

        for (unsigned int i= 0; i <count; i++){
            if (wrbuffer[i] !=rdbuffer[i])
                printf(" error of return(%d) value exp %c, ret%c\n",i,wrbuffer[i],rdbuffer[i]);
        }
        testcount++;
        if ((testcount%1001) == 0)
            printf("TestCnt %d rd(%d)<%s>\r",testcount,testcount,&rdbuffer[0]);
        else
            printf("TestCnt %d \r",testcount);
        
            
       
    }

    tick = (GetTickCount() - tick) - totalAdjust;

    printf("bytes tranferred: %d; tranfer time: %g s; tranfer rate: %g Mbs", bytes, (double)(tick ? tick : 1 )/1000, (((double)(bytes * 8)/(double)( tick ? tick : 1 ))*1000)/ONE_MEGABIT );

    CloseHandle( device );
    if(readFromFile)
    {
        CloseHandle( file );
    }
}









void usage()
{
    printf("<spot_usb_test /operation:<r|w> [/mode:<a|s>] [/buffersize:<number>] [/file:<file_name> | /pattern:<pattern>] [/maxbytes:<number>] /canc");
}


int _tmain(int argc, _TCHAR* argv[])
{
    try
    {
    operation op;
    OPERATION_CONTEXT context;

    TCHAR commandLine[1024];
    commandLine[0] = '\0';

    for(int i = 1; i < argc; ++i)
    {
        _tcscat( commandLine, argv[i] );
    }    

    ParamParser p( commandLine, L'/', L':' );

    if(!p.Contains(TEXT("operation"))) throw InvalidParameterException();
    
    StringType operation = p.GetString( TEXT("operation"), TEXT("r") );
    if(!operation.compare(TEXT("r")))
    {
        op = &read;
    }
    else if(!operation.compare(TEXT("w")) || !operation.compare(TEXT("wr"))|| !operation.compare(TEXT("a")))
    {
        if(!operation.compare(TEXT("w")))
        {
            op = &write;
        }
        else if(!operation.compare(TEXT("a")))
        {
           op = &auto_read_write;
        }

        else
        {
            op = &read_write;
        }

        if     (p.Contains( TEXT("file")))
        {
            context.m_fileName = p.GetString( TEXT("file"), TEXT("data.txt") );
        }
        else 
        {
            StringType pattern = p.GetString( TEXT("pattern"), TEXT("aaaaa55555") );
            for(size_t i = 0; i < pattern.length(); ++i)
            {
                context.m_pattern.push_back( (char)pattern[i] );
            }
        }
    }
    

    StringType mode = p.GetString( TEXT("mode"), TEXT("s") );
    if(!mode.compare( TEXT("s") ))
    {
        context.m_async = false;
    }
    else
    {
        context.m_async = true;
    }

    context.m_maxBytes   = p.GetInteger( TEXT("maxbytes")  , 1024 );
    context.m_bufferSize = p.GetInteger( TEXT("buffersize"), 1024 );

    if(p.Contains(TEXT("canc")))
    {
        if(context.m_async == false)
        {
            _tprintf(TEXT("random cancellation switch ignored in synchronous mode\n"));
        }
        context.m_cancellation = true;
    }
    else
    {
        context.m_cancellation = false;
    }

    op( (void*)&context );

    }
    catch(...)
    {
        usage();
        exit(1);
    }

    return 0;
}

