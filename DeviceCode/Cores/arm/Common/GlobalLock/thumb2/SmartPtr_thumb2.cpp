#include <tinyhal.h>

//--//

#define DISABLED_MASK 0x1

//--//

SmartPtr_IRQ::SmartPtr_IRQ(void* context)
{
    m_context = context;
    Disable();
}

SmartPtr_IRQ::~SmartPtr_IRQ()
{
    Restore();
}

BOOL SmartPtr_IRQ::WasDisabled()
{
    return (m_state & DISABLED_MASK) == DISABLED_MASK;
}

void SmartPtr_IRQ::Acquire()
{
    UINT32 Cp = m_state;

    if ((Cp & DISABLED_MASK) == DISABLED_MASK)
    {
        Disable();
    }
}

void SmartPtr_IRQ::Release()
{
    UINT32 Cp = m_state;

    if ((Cp & DISABLED_MASK) == 0)
    {
        register UINT32 Cs __asm("primask");

        m_state = Cs;

        __enable_irq();
    }
}

void SmartPtr_IRQ::Probe()
{
    UINT32 Cp = m_state;

    if ((Cp & DISABLED_MASK) == 0)
    {
        register UINT32 Cs __asm("primask");

        UINT32 s = Cs;

        __enable_irq();

        // just to allow an interupt to an occur
        __NOP();

        // restore irq state
        Cs = s;
    }
}

BOOL SmartPtr_IRQ::GetState(void* context)
{
    register UINT32 Cp __asm("primask");
    return (0 == (Cp & DISABLED_MASK));
}

BOOL SmartPtr_IRQ::ForceDisabled(void* context)
{
    __disable_irq();
    return true;
}

BOOL SmartPtr_IRQ::ForceEnabled(void* context)
{
    __enable_irq();
    return true;
}

void SmartPtr_IRQ::Disable()
{
    register UINT32 Cp __asm("primask");

    m_state = Cp;

    __disable_irq();
}

void SmartPtr_IRQ::Restore()
{
    UINT32 Cp = m_state;

    if ((Cp & DISABLED_MASK) == 0)
    {
        __enable_irq();
    }
}

