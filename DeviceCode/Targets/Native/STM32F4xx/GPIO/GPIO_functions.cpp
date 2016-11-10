////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for CMSIS: Copyright (c) Eclo Solutions Lda. Based in original code from Oberon microsystems, Inc.
//
//  *** GPIO Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

// struct for pair GPIO and PIN
struct GPIOPortPin
{
    GPIO_TypeDef* port;
    uint16_t pin;
} ;

const GPIOPortPin gpioPortPin[] = GPIO_PORT_PINS;
#define GPIO_PIN_COUNT ARRAYSIZE_CONST_EXPR(gpioPortPin)

struct Int_State
{
    HAL_COMPLETION completion; // debounce completion
    BYTE pin;      // gpio port pin index
    BYTE mode;     // edge mode
    BYTE debounce; // debounce flag
    BYTE expected; // expected pin state
    GPIO_INTERRUPT_SERVICE_ROUTINE ISR; // interrupt handler
    void* param;   // interrupt handler parameter
    UINT32 debounceTicks;
};

static Int_State g_int_state[GPIO_PIN_COUNT]; // interrupt state

static UINT32 g_debounceTicks;
static UINT16 g_pinReserved[GPIO_PIN_COUNT]; //  1 bit per pin

/*
 * Debounce Completion Handler
 */
void GPIO_DebounceHandler(void* arg)
{
    Int_State* state = (Int_State*)arg;
    if(state->ISR)
    {
        UINT32 actual = CPU_GPIO_GetPinState(state->pin); // get actual pin state
        if(actual == state->expected)
        {
            state->ISR(state->pin, actual, state->param);
            if(state->mode == GPIO_INT_EDGE_BOTH)
            { // both edges
                state->expected ^= 1; // update expected state
            }
        }
    }
}

/*
 * Interrupt Handler
 */
void GlobalGPIOHandler(int pin)  // 0 <= num <= 15
{
    Int_State* state = &g_int_state[pin];
    state->completion.Abort();
    UINT32 actual;
    
    HAL_GPIO_EXTI_IRQHandler(gpioPortPin[pin].pin);
    
    actual = CPU_GPIO_GetPinState(state->pin); // get actual pin state

    if(state->ISR)
    {
        if(state->debounce)
        {   
            // debounce enabled
            // for back compat treat state.debounceTicks == 0 as indication to use global debounce setting
            UINT32 debounceDeltaTicks = state->debounceTicks == 0 ? g_debounceTicks : state->debounceTicks;
            state->completion.EnqueueTicks(HAL_Time_CurrentTicks() + debounceDeltaTicks);
        }
        else
        {
            state->ISR(state->pin, state->expected, state->param);
            if(state->mode == GPIO_INT_EDGE_BOTH)
            { 
                // both edges
                if(actual != state->expected)
                { // fire another isr to keep in synch
                    state->ISR(state->pin, actual, state->param);
                }
                else
                {
                    state->expected ^= 1; // update expected state
                }
            }
        }
    }

}

// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void EXTI0_IRQHandler(void) // EXTI0
{
    GlobalGPIOHandler(0);
}

// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void EXTI1_IRQHandler(void) // EXTI1
{
    GlobalGPIOHandler(1);
}

// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void EXTI2_IRQHandler(void) // EXTI2
{
    GlobalGPIOHandler(2);
}

// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void EXTI3_IRQHandler(void) // EXTI3
{
    GlobalGPIOHandler(3);
}

// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void EXTI4_IRQHandler(void) // EXTI4
{
    GlobalGPIOHandler(4);
}

// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void EXTI9_5_IRQHandler(void) // EXTI5 - EXTI9
{
    UINT32 pending = EXTI->PR & EXTI->IMR & 0x03E0; // pending bits 5..9
    int num = 5; 
    pending >>= 5;
    do
    {
        if(pending & 1) 
        {
            GlobalGPIOHandler(num);
        }
        num++;
        pending >>= 1;
    } 
    while(pending);
}

// must declare this as extern C funtion otherwise it won't be recognized by the linker as a valid interrupt handler  
extern "C" void EXTI15_10_IRQHandler(void) // EXTI10 - EXTI15
{
    UINT32 pending = EXTI->PR & EXTI->IMR & 0xFC00; // pending bits 10..15
    int num = 10; 
    pending >>= 10;
    do
    {
        if(pending & 1) 
        {
            GlobalGPIOHandler(num);
        }
        num++; 
        pending >>= 1;
    } 
    while(pending);
}

void EnableIRQ(uint16_t gpio_pin)
{
    if(gpio_pin == GPIO_PIN_0)
    {
        HAL_NVIC_EnableIRQ(EXTI0_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_1)
    {
        HAL_NVIC_EnableIRQ(EXTI1_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_2)
    {
        HAL_NVIC_EnableIRQ(EXTI2_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_3)
    {
        HAL_NVIC_EnableIRQ(EXTI3_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_4)
    {
        HAL_NVIC_EnableIRQ(EXTI4_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_5)
    {
        HAL_NVIC_EnableIRQ(EXTI9_5_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_6)
    {
        HAL_NVIC_EnableIRQ(EXTI9_5_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_7)
    {
        HAL_NVIC_EnableIRQ(EXTI9_5_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_8)
    {
        HAL_NVIC_EnableIRQ(EXTI9_5_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_9)
    {
        HAL_NVIC_EnableIRQ(EXTI9_5_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_10)
    {
        HAL_NVIC_EnableIRQ(EXTI15_10_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_11)
    {
        HAL_NVIC_EnableIRQ(EXTI15_10_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_12)
    {
        HAL_NVIC_EnableIRQ(EXTI15_10_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_13)
    {
        HAL_NVIC_EnableIRQ(EXTI15_10_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_14)
    {
        HAL_NVIC_EnableIRQ(EXTI15_10_IRQn);
    }
    else if(gpio_pin == GPIO_PIN_15)
    {
        HAL_NVIC_EnableIRQ(EXTI15_10_IRQn);
    }
}

void GPIO_Pin_Config(GPIO_PIN pin
                    , UINT32 mode
                    , GPIO_RESISTOR resistor 
                    , GPIO_INT_EDGE edge
                    , GPIO_INTERRUPT_SERVICE_ROUTINE ISR)
{
    GPIO_InitTypeDef GPIO_InitStructure;

    GPIO_InitStructure.Pin = gpioPortPin[pin].pin;

    // Configure pin mode
    GPIO_InitStructure.Mode = mode;
    
    // interrupt is defined?
    if(ISR != NULL)
    {
        if(edge == GPIO_INT_EDGE_BOTH)
        {
            GPIO_InitStructure.Mode |= GPIO_MODE_IT_RISING_FALLING;
        }
        // falling edge or both
        else if(edge == GPIO_INT_EDGE_LOW || edge == GPIO_INT_LEVEL_LOW)
        {
            GPIO_InitStructure.Mode |= GPIO_MODE_IT_FALLING;            
        }
        // rising or both 
        else if(edge == GPIO_INT_EDGE_HIGH || edge == GPIO_INT_LEVEL_HIGH)
        {
            GPIO_InitStructure.Mode |= GPIO_MODE_IT_RISING;            
        }
    }

    if(resistor == RESISTOR_PULLUP)
    {
        GPIO_InitStructure.Pull = GPIO_PUPDR_PUPDR0_0;
    }
    else if(resistor == RESISTOR_PULLDOWN)
    {
        GPIO_InitStructure.Pull = GPIO_PUPDR_PUPDR0_1;
    }

    // Init GPIO 
    HAL_GPIO_Init(gpioPortPin[pin].port, &GPIO_InitStructure);
}

BOOL CPU_GPIO_Initialize()
{
    GPIO_InitTypeDef GPIO_InitStruct;

    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    CPU_GPIO_SetDebounce(20); // ???

    // Configure all GPIO as analog to reduce current consumption on non used IOs
    GPIO_InitStruct.Mode = GPIO_MODE_ANALOG;
    GPIO_InitStruct.Speed = GPIO_SPEED_HIGH;
    GPIO_InitStruct.Pull = GPIO_NOPULL;
    GPIO_InitStruct.Pin = GPIO_PIN_All;

    #if defined (RCC_AHB1ENR_GPIOAEN)
    
    #if !defined(BUILD_RTM)
    // don't change PA13 and PA14 as they maybe used in JTAG
    GPIO_InitStruct.Pin = GPIO_PIN_All & (~GPIO_PIN_13) & (~GPIO_PIN_14);
    #endif 
    
    __HAL_RCC_GPIOA_CLK_ENABLE();
    HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);
    
    // disable Port A clock only if this not RTM, otherwise keep it active for JTAG and debug
    #if defined(BUILD_RTM)
    __HAL_RCC_GPIOA_CLK_DISABLE();
    #endif
    
    #endif

    #if !defined(BUILD_RTM) && (TOTAL_GENERIC_PORTS == 1)
    // don't change PB3 if it's being used as ITM port for SWO
    GPIO_InitStruct.Pin = GPIO_PIN_All & (~GPIO_PIN_3);
    #else
    // reset pin structure to ALL
    GPIO_InitStruct.Pin = GPIO_PIN_All & (~GPIO_PIN_13) & (~GPIO_PIN_14);
    #endif 

    #if defined (RCC_AHB1ENR_GPIOBEN)
    __HAL_RCC_GPIOB_CLK_ENABLE();
    HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

    // turn off clock only for Port B only if ITM port for SWO it's not being used
    #if !(TOTAL_GENERIC_PORTS == 1)
        __HAL_RCC_GPIOB_CLK_DISABLE();
    #endif

    #endif

    #if !defined(BUILD_RTM)
    // reset pin structure to ALL
    GPIO_InitStruct.Pin = GPIO_PIN_All & (~GPIO_PIN_13) & (~GPIO_PIN_14);
    #endif 

    #if defined (RCC_AHB1ENR_GPIOCEN)
    __HAL_RCC_GPIOC_CLK_ENABLE();
    HAL_GPIO_Init(GPIOC, &GPIO_InitStruct);
    __HAL_RCC_GPIOC_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIODEN)
    __HAL_RCC_GPIOD_CLK_ENABLE();
    HAL_GPIO_Init(GPIOD, &GPIO_InitStruct);
    __HAL_RCC_GPIOD_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOEEN)
    __HAL_RCC_GPIOE_CLK_ENABLE();
    HAL_GPIO_Init(GPIOE, &GPIO_InitStruct);
    __HAL_RCC_GPIOE_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOFEN)
    __HAL_RCC_GPIOF_CLK_ENABLE();
    HAL_GPIO_Init(GPIOF, &GPIO_InitStruct);
    __HAL_RCC_GPIOF_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOGEN)
    __HAL_RCC_GPIOG_CLK_ENABLE(); 
    HAL_GPIO_Init(GPIOG, &GPIO_InitStruct);
    __HAL_RCC_GPIOG_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOHEN)
    __HAL_RCC_GPIOH_CLK_ENABLE(); 
    HAL_GPIO_Init(GPIOH, &GPIO_InitStruct); 
    __HAL_RCC_GPIOH_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOIEN)
    __HAL_RCC_GPIOI_CLK_ENABLE(); 
    HAL_GPIO_Init(GPIOI, &GPIO_InitStruct);
    __HAL_RCC_GPIOI_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOJEN)
    __HAL_RCC_GPIOJ_CLK_ENABLE(); 
    HAL_GPIO_Init(GPIOJ, &GPIO_InitStruct);
    __HAL_RCC_GPIOJ_CLK_DISABLE();
    #endif

    for(int i = 0; i < GPIO_PIN_COUNT; i++)
    {
        g_pinReserved[i] = FALSE;
        g_int_state[i].completion.InitializeForISR(&GPIO_DebounceHandler);
          
        // Enable GPIO clock
        if(gpioPortPin[i].port == GPIOA)
        {
            __HAL_RCC_GPIOA_CLK_ENABLE();
        }
        #if defined (RCC_AHB1ENR_GPIOBEN)
        else if(gpioPortPin[i].port == GPIOB)
        {
            __HAL_RCC_GPIOB_CLK_ENABLE();
        }
        #endif
        #if defined (RCC_AHB1ENR_GPIOCEN)
        else if(gpioPortPin[i].port == GPIOC)
        {
            __HAL_RCC_GPIOC_CLK_ENABLE();
        }
        #endif
        #if defined (RCC_AHB1ENR_GPIODEN)
        else if(gpioPortPin[i].port == GPIOD)
        {
            __HAL_RCC_GPIOD_CLK_ENABLE();
        }
        #endif
        #if defined (RCC_AHB1ENR_GPIOEEN)
        else if(gpioPortPin[i].port == GPIOE)
        {
            __HAL_RCC_GPIOE_CLK_ENABLE();
        }
        #endif
        #if defined (RCC_AHB1ENR_GPIOFEN)
        else if(gpioPortPin[i].port == GPIOF)
        {
            __HAL_RCC_GPIOF_CLK_ENABLE();
        }
        #endif
        #if defined (RCC_AHB1ENR_GPIOGEN)
        else if(gpioPortPin[i].port == GPIOG)
        {
            __HAL_RCC_GPIOG_CLK_ENABLE();
        }
        #endif
        #if defined (RCC_AHB1ENR_GPIOHEN)
        else if(gpioPortPin[i].port == GPIOH)
        {
            __HAL_RCC_GPIOH_CLK_ENABLE();
        }
        #endif
        #if defined (RCC_AHB1ENR_GPIOIEN)
        else if(gpioPortPin[i].port == GPIOI)
        {
            __HAL_RCC_GPIOI_CLK_ENABLE();
        }
        #endif
        #if defined (RCC_AHB1ENR_GPIOJEN)
        else if(gpioPortPin[i].port == GPIOJ)
        {
            __HAL_RCC_GPIOJ_CLK_ENABLE();
        }
        #endif
    }

    return TRUE;
}

BOOL CPU_GPIO_Uninitialize()
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    for(int i = 0; i < GPIO_PIN_COUNT; i++)
    {
        g_int_state[i].completion.Abort();
    }

    // disable all GPIO external interrups and clock;
    #if defined (RCC_AHB1ENR_GPIOAEN)
    HAL_GPIO_DeInit(GPIOA, GPIO_PIN_All);
    __HAL_RCC_GPIOA_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOBEN)
    HAL_GPIO_DeInit(GPIOB, GPIO_PIN_All);
    __HAL_RCC_GPIOB_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOCEN)
    HAL_GPIO_DeInit(GPIOC, GPIO_PIN_All);
    __HAL_RCC_GPIOC_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIODEN)
    HAL_GPIO_DeInit(GPIOD, GPIO_PIN_All);
    __HAL_RCC_GPIOD_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOEEN)
    HAL_GPIO_DeInit(GPIOE, GPIO_PIN_All);
    __HAL_RCC_GPIOE_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOFEN)
    HAL_GPIO_DeInit(GPIOF, GPIO_PIN_All);
    __HAL_RCC_GPIOF_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOGEN)
    HAL_GPIO_DeInit(GPIOG, GPIO_PIN_All);
    __HAL_RCC_GPIOG_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOHEN)
    HAL_GPIO_DeInit(GPIOH, GPIO_PIN_All);
    __HAL_RCC_GPIOH_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOIEN)
    HAL_GPIO_DeInit(GPIOI, GPIO_PIN_All);
    __HAL_RCC_GPIOI_CLK_DISABLE();
    #endif
    #if defined (RCC_AHB1ENR_GPIOJEN)
    HAL_GPIO_DeInit(GPIOJ, GPIO_PIN_All);
    __HAL_RCC_GPIOJ_CLK_DISABLE();
    #endif

    return TRUE;
}

UINT32 CPU_GPIO_Attributes(GPIO_PIN pin)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    if(pin < GPIO_PIN_COUNT)
    {
        return GPIO_ATTRIBUTE_INPUT | GPIO_ATTRIBUTE_OUTPUT;
    }
    return GPIO_ATTRIBUTE_NONE;
}

/*
 * alternate:
 * GPIO_ALT_PRIMARY: GPIO
 * GPIO_ALT_MODE_1: Analog
 * GPIO_ALT_MODE_2 | AF << 4 | speed << 8: Alternate Function
 * GPIO_ALT_MODE_3 | AF << 4 | speed << 8: Alternate Function with open drain
 * speed: 0: 2MHZ, 1: 25MHz, 2: 50MHz, 3: 100MHz
 */
void CPU_GPIO_DisablePin(GPIO_PIN pin, GPIO_RESISTOR resistor, UINT32 output, GPIO_ALT_MODE alternate)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    if(pin < GPIO_PIN_COUNT)
    {
        // disable INT state for this pin
        Int_State* state = &g_int_state[pin];
       
        state->ISR = NULL;
        state->completion.Abort();
        
        // release pin
        g_pinReserved[pin] = FALSE;
        
        // de-init this pin and configure GPIOit as analog to reduce current consumption on non used IOs 
        GPIO_InitTypeDef GPIO_InitStruct;

        GPIO_InitStruct.Mode = GPIO_MODE_ANALOG;
        GPIO_InitStruct.Speed = GPIO_SPEED_HIGH;
        GPIO_InitStruct.Pull = GPIO_NOPULL;
        GPIO_InitStruct.Pin = gpioPortPin[pin].pin;

        HAL_GPIO_Init(gpioPortPin[pin].port, &GPIO_InitStruct);
    }
}

void CPU_GPIO_EnableOutputPin(GPIO_PIN pin, BOOL initialState)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    if(pin < GPIO_PIN_COUNT)
    {
        // general purpose output, any edge, does matter
        GPIO_Pin_Config(pin, GPIO_MODE_OUTPUT_PP, RESISTOR_DISABLED, GPIO_INT_EDGE_BOTH, NULL);

        CPU_GPIO_SetPinState(pin, initialState);
        
        // disable INT state for this pin
        Int_State* state = &g_int_state[pin];
       
        state->ISR = NULL;
        state->completion.Abort();
    }
}

BOOL CPU_GPIO_EnableInputPin(GPIO_PIN pin
                            , BOOL GlitchFilterEnable
                            , GPIO_INTERRUPT_SERVICE_ROUTINE ISR
                            , GPIO_INT_EDGE edge
                            , GPIO_RESISTOR resistor
                           )
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    return CPU_GPIO_EnableInputPin2(pin, GlitchFilterEnable, ISR, 0, edge, resistor);
}

BOOL CPU_GPIO_EnableInputPin2(GPIO_PIN pin
                             , BOOL GlitchFilterEnable
                             , GPIO_INTERRUPT_SERVICE_ROUTINE ISR
                             , void* ISR_Param
                             , GPIO_INT_EDGE edge
                             , GPIO_RESISTOR resistor
                            )
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    if(pin >= GPIO_PIN_COUNT)
        return FALSE;

    GPIO_Pin_Config(pin, GPIO_MODE_INPUT, resistor, edge, ISR); // input
    
    Int_State* state = &g_int_state[pin];

    if(ISR)
    {
        state->pin = (BYTE)pin;
        state->mode = (BYTE)edge;
        state->debounce = (BYTE)GlitchFilterEnable;
        state->param = ISR_Param;
        state->ISR = ISR;
        state->completion.Abort();
        state->completion.SetArgument(state);

        switch(edge)
        {
        case GPIO_INT_EDGE_LOW:
        case GPIO_INT_LEVEL_LOW:
            state->expected = FALSE;
            break;

        case GPIO_INT_EDGE_HIGH:
        case GPIO_INT_LEVEL_HIGH:
            state->expected = TRUE;
            break;

        case GPIO_INT_EDGE_BOTH:
            UINT32 actual;
            do
            {
                // clear pending interrupt
                __HAL_GPIO_EXTI_CLEAR_FLAG(gpioPortPin[pin].pin);
                actual = CPU_GPIO_GetPinState(pin); // get actual pin state
            } while(__HAL_GPIO_EXTI_GET_IT(gpioPortPin[pin].pin)); // repeat if pending again
            state->expected = (BYTE)(actual ^ 1);
        }
                    
        // Enable and set EXTI Line Interrupt
        EnableIRQ(gpioPortPin[pin].pin);
        
        // check for level interrupts
        if(edge == GPIO_INT_LEVEL_HIGH && CPU_GPIO_GetPinState(pin)
            || edge == GPIO_INT_LEVEL_LOW && !CPU_GPIO_GetPinState(pin))
        {
            // force interrupt
            __HAL_GPIO_EXTI_GENERATE_SWIT(gpioPortPin[pin].pin);
        }
    }
    else
    {
        // clear state, just in case
        state->ISR = NULL;
        state->completion.Abort();        
    }
    
    return TRUE;
}

BOOL CPU_GPIO_GetPinState(GPIO_PIN pin)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    if(pin >= GPIO_PIN_COUNT)
        return FALSE;

    return (BOOL)HAL_GPIO_ReadPin(gpioPortPin[pin].port, gpioPortPin[pin].pin);
}

void CPU_GPIO_SetPinState(GPIO_PIN pin, BOOL pinState)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    if(pin < GPIO_PIN_COUNT)
    {
        HAL_GPIO_WritePin(gpioPortPin[pin].port, gpioPortPin[pin].pin, (GPIO_PinState)pinState);
    }
}

BOOL CPU_GPIO_PinIsBusy(GPIO_PIN pin)  // busy == reserved
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    if(pin >= GPIO_PIN_COUNT)
        return FALSE;

    return g_pinReserved[pin];
}

BOOL CPU_GPIO_ReservePin(GPIO_PIN pin, BOOL fReserve)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    if(pin >= GPIO_PIN_COUNT)
        return FALSE;

    GLOBAL_LOCK(irq);
    if(fReserve)
    {
        if(g_pinReserved[pin])
        {
            // already reserved
            return FALSE; 
        }
        else
        {
            // OK to reserve pin
            g_pinReserved[pin] = TRUE;
        }
    }
    else
    {
        // release pin
        g_pinReserved[pin] = FALSE;
    }

    return TRUE;
}

UINT32 CPU_GPIO_GetDebounce()
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    return g_debounceTicks / (SLOW_CLOCKS_PER_SECOND / 1000); // ticks -> ms
}

BOOL CPU_GPIO_SetDebounce(INT64 debounceTimeMilliseconds)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    if(debounceTimeMilliseconds > 0 && debounceTimeMilliseconds < 10000)
    {
        g_debounceTicks = CPU_MillisecondsToTicks((UINT32)debounceTimeMilliseconds);
        return TRUE;
    }
    return FALSE;
}

INT32 CPU_GPIO_GetPinCount()
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    return GPIO_PIN_COUNT;
}

void CPU_GPIO_GetPinsMap(UINT8* pins, size_t size)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    for(int i = 0; i < size && i < GPIO_PIN_COUNT; i++)
    {
        pins[i] = GPIO_ATTRIBUTE_INPUT | GPIO_ATTRIBUTE_OUTPUT;
    }
}

UINT8 CPU_GPIO_GetSupportedResistorModes(GPIO_PIN pin)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();
    return (1 << RESISTOR_DISABLED) | (1 << RESISTOR_PULLUP) | (1 << RESISTOR_PULLDOWN);
}

UINT8 CPU_GPIO_GetSupportedInterruptModes(GPIO_PIN pin)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();
    return (1 << GPIO_INT_EDGE_LOW) | (1 << GPIO_INT_EDGE_HIGH) | (1 << GPIO_INT_EDGE_BOTH)
        | (1 << GPIO_INT_LEVEL_LOW) | (1 << GPIO_INT_LEVEL_HIGH);
}

UINT32 CPU_GPIO_GetPinDebounce(GPIO_PIN pin)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    Int_State& state = g_int_state[gpioPortPin[pin].pin];

    return state.debounceTicks / (SLOW_CLOCKS_PER_SECOND / 1000); // ticks -> ms
}

BOOL CPU_GPIO_SetPinDebounce(GPIO_PIN pin, INT64 debounceTimeMilliseconds)
{
    NATIVE_PROFILE_HAL_PROCESSOR_GPIO();

    Int_State& state = g_int_state[gpioPortPin[pin].pin];

    if(debounceTimeMilliseconds > 0 && debounceTimeMilliseconds < 10000)
    {
        state.debounceTicks = CPU_MillisecondsToTicks((UINT32)debounceTimeMilliseconds);
        return TRUE;
    }
    return FALSE;
}
