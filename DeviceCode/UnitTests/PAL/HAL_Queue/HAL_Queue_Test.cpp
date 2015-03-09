////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include <tinyhal.h>

int* TestPopLen(Hal_Queue_UnknownSize<int>& queue, int len, int expectedSize, int overloaded=0)
{
    size_t tmp = len;
    int* data;

    if(len ==1 )
    {
        data = queue.Pop();
    }
    else
    {
        data = queue.Pop(tmp);
    }

    if(overloaded > 0)
    {
        if(overloaded != (len-tmp))
        {
            hal_printf( "FAILURE_POP_UNDERFLOW %d=>%d\r\n", overloaded, len-tmp );
            lcd_printf( "FAILURE_POP_UNDERFLOW %d=>%d\r\n", overloaded, len-tmp );        
            while(true);
        }
    }
    else if(tmp != len)
    {
        hal_printf( "FAILURE_POP %d=>%d\r\n", len, tmp );
        lcd_printf( "FAILURE_POP %d=>%d\r\n", len, tmp );        
        while(true);
    }

    if(queue.NumberOfElements() != expectedSize)
    {
        hal_printf( "FAILURE_PUSH\r\n" );
        lcd_printf( "FAILURE_PUSH %d=>%d\r\n", expectedSize, queue.NumberOfElements());
        while(true);
    }
    
    return data;
}

int* TestPushLen(Hal_Queue_UnknownSize<int>& queue, int len, int expectedSize, int overloaded=0)
{
    size_t tmp = len;
    int* data;

//    lcd_printf("\f\n\np%d: ", len);

    if(len == 1)
    {
        data = queue.Push();
    }
    else
    {
        data = queue.Push(tmp);
    }

    if(overloaded > 0)
    {
        if(overloaded != (len-tmp))
        {
            hal_printf( "FAILURE_PUSH_UNDERFLOW %d=>%d\r\n", overloaded, len-tmp );
            lcd_printf( "FAILURE_PUSH_UNDERFLOW %d=>%d\r\n", overloaded, len-tmp );        
            while(true);
        }
    }
    else if(tmp != len)
    {
        hal_printf( "FAILURE_PUSH %d=>%d\r\n", len, tmp );
        lcd_printf( "FAILURE_PUSH %d=>%d\r\n", len, tmp );        
        while(true);
    }

    if(queue.NumberOfElements() != expectedSize)
    {
        hal_printf( "FAILURE_PUSH\r\n" );
        lcd_printf( "FAILURE_PUSH %d=>%d\r\n", expectedSize, queue.NumberOfElements());
        while(true);
    }
    
    return data;
}

void HalQueueTest()
{
    Hal_Queue_UnknownSize<int> queue;
    int array[100];
    int tmp[25];
    int *data;
    int len;

    LCD_Clear();
    lcd_printf( "Hal_Queue_UnknownSize Test\r\n");

    queue.Initialize(array, 100);

    for(int i=50; i<50+25; i++)
    {
        tmp[i-50] = i;
    }
    

    for(int j = 0; j<10; j++)
    {
        lcd_printf( "\f\r\nIteration %d\r\n", j);
        if(!queue.IsEmpty()) 
        {
            hal_printf( "FAILURE_EMPTY\r\n" );
            lcd_printf( "FAILURE_EMPTY %d\r\n", queue.NumberOfElements());
            while(true);
        }

        for(int i=0; i<50; i++)
        {
            data = TestPushLen(queue, 1, i+1 );
            *data = i;
        }
        if(queue.NumberOfElements() != 50)
        {
            hal_printf( "FAILURE_PUSH\r\n" );
            lcd_printf( "FAILURE_PUSH %d=>%d\r\n", 50, queue.NumberOfElements());
            while(true);
        }

        data = TestPushLen(queue,25,75);
        memcpy(data, tmp, sizeof(tmp));

        for(int i=75; i<95; i++)
        {
            data = TestPushLen(queue,1,i+1);
            *data = i;
        }

        data = TestPushLen(queue,10, 100, 5);
        for(int i=95; i<100; i++)
        {
            data[i] = i;
        }

        if(!queue.IsFull()) 
        {
            hal_printf( "FAILURE_FULL\r\n" );
            lcd_printf( "FAILURE_FULL %d\r\n", queue.NumberOfElements());
            while(true);
        }

        data = TestPopLen(queue,50, 50);

        for(int i=0; i<50; i++)
        {
            if(data[i] != i)
            {
                hal_printf( "FAILURE_1: %d\r\n", i );
                lcd_printf( "FAILURE_1: %d=>%d\r\n", i, data[i] );
                while(true);
            }
        }
        for(int i=50; i<50+25; i++)
        {
            data = TestPopLen(queue,1,100-i-1);
            if(data[i-50] != tmp[i-50])
            {
                hal_printf( "FAILURE_2: %d\r\n", tmp[i-50] );
                lcd_printf( "FAILURE_2: %d=>%d\r\n", tmp[i-50], data[i-50] );
                while(true);
            }
        }
        data = TestPopLen(queue,20, 5);
        for(int i=75; i<95; i++)
        {
            if(data[i-75] != i)
            {
                hal_printf( "FAILURE_3: %d\r\n", i );
                lcd_printf( "FAILURE_3: %d=>%d\r\n", i, data[i-75] );
                while(true);
            }
        }

        for(int i=95; i<100; i++)
        {
            data = TestPopLen(queue,1,100-i-1);
            if(data[i-95] != i)
            {
                hal_printf( "FAILURE_3: %d\r\n", i );
                lcd_printf( "FAILURE_3: %d=>%d\r\n", i, data[i-95] );
                while(true);
            }
        }
        if((j%3) == 0)
        {
            TestPopLen(queue, 3, 0, 3);
        }
    }
}
