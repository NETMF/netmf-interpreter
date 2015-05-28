class SmartPtr_FIQ
{
    static const UINT32 DISABLED_MASK = 0x40;

    UINT32 m_state;

public:
    SmartPtr_FIQ()  { Disable(); }
    ~SmartPtr_FIQ() { Restore(); }

    BOOL WasDisabled()
    {
        return (m_state & DISABLED_MASK) == DISABLED_MASK;
    }

    void Acquire()
    {
        UINT32 Cp = m_state;

        if((Cp & DISABLED_MASK) == DISABLED_MASK)
        {
            Disable();
        }
    }

    void Release()
    {
        UINT32 Cp = m_state;

        if((Cp & DISABLED_MASK) == 0)
        {
            UINT32 Cs;

#ifdef __GNUC__
			asm("MRS %0, CPSR" : "=r" (Cp));
			asm("BIC %0, %1, #0x40" : "=r" (Cs) : "r" (Cp));
			asm("MSR CPSR_c %0" :: "r" (Cs));
#else
            __asm
            {
                MRS     Cp, CPSR
                BIC     Cs, Cp, #0x40
                MSR     CPSR_c, Cs
            }
#endif

            m_state = Cp;
        }
    }

    void Probe()
    {
        UINT32 Cp = m_state;

        if((Cp & DISABLED_MASK) == 0)
        {
            UINT32 Cs;

#ifdef __GNUC__
			asm("MRS %0, CPSR" : "=r" (Cp));
			asm("BIC %0, %1, #0x40" : "=r" (Cs) : "r" (Cp));
			asm("MSR CPSR_c %0" :: "r" (Cs));
			asm("MSR CPSR_c %0" :: "r" (Cp));
#else
            __asm
            {
                MRS     Cp, CPSR
                BIC     Cs, Cp, #0x40
                MSR     CPSR_c, Cs
                MSR     CPSR_c, Cp
            }
#endif
        }
    }

    static BOOL GetState()
    {
        UINT32 Cp;

#ifdef __GNUC__
		asm("MRS %0, CPSR" : "=r" (Cp));
		asm("MVN %0, %1, LSR #6" : "=r" (Cp) : "r" (Cp));
		asm("AND %0, %1, #0x01" : "=r" (Cp) : "r" (Cp));
#else
        __asm
        {
            MRS     Cp, CPSR
            MVN     Cp, Cp, LSR #6
            AND     Cp, Cp, #0x01
        }
#endif

        return Cp;
    }

    static BOOL ForceDisabled()
    {
        UINT32 Cp;
        UINT32 Cs;

#ifdef __GNUC__
		asm("MRS %0, CPSR" : "=r" (Cp));
		asm("ORR %0, %1, #0x40" : "=r" (Cs) : "r" (Cp));
		asm("MSR CPSR_c, %0" :: "r" (Cs));
		asm("MVN %0, %1, LSR #6" : "=r" (Cp) : "r" (Cp));
		asm("AND %0, %1, #0x01" : "=r" (Cp) : "r" (Cp));
#else
        __asm
        {
            MRS     Cp, CPSR
            ORR     Cs, Cp, #0x40
            MSR     CPSR_c, Cs
            MVN     Cp, Cp, LSR #6
            AND     Cp, Cp, #0x01
        }
#endif

        return Cp;
    }

    static BOOL ForceEnabled()
    {
        UINT32 Cp;
        UINT32 Cs;

#ifdef __GNUC__
		asm("MRS %0, CPSR" : "=r" (Cp));
		asm("BIC %0, %1, #0x40" : "=r" (Cs) : "r" (Cp));
		asm("MSR CPSR_c, %0" :: "r" (Cs));
		asm("MVN %0, %1, LSR #6" : "=r" (Cp) : "r" (Cp));
		asm("AND %0, %1, #0x01" : "=r" (Cp) : "r" (Cp));
#else
        __asm
        {
            MRS     Cp, CPSR
            BIC     Cs, Cp, #0x40
            MSR     CPSR_c, Cs
            MVN     Cp, Cp, LSR #6
            AND     Cp, Cp, #0x01
        }
#endif

        return Cp;
    }

private:
    void Disable()
    {
        UINT32 Cp;
        UINT32 Cs;

#ifdef __GNUC__
		asm("MRS %0, CPSR" : "=r" (Cp));
		asm("ORR %0, %1, #0x40" : "=r" (Cs) : "r" (Cp));
		asm("MSR CPSR_c, %0" :: "r" (Cs));
#else
        __asm
        {
            MRS     Cp, CPSR
            ORR     Cs, Cp, #0x40
            MSR     CPSR_c, Cs
        }
#endif

        m_state = Cp;
    }

    void Restore()
    {
        UINT32 Cp = m_state;

        if((Cp & DISABLED_MASK) == 0)
        {
#ifdef __GNUC__
			asm("MRS %0, CPSR" : "=r" (Cp));
			asm("BIC %0, %1, #0x40" : "=r" (Cp) : "r" (Cp));
			asm("BSR CPSR_c, %0" :: "r" (Cp));
#else
            __asm
            {
                MRS     Cp, CPSR
                BIC     Cp, Cp, #0x40
                MSR     CPSR_c, Cp
            }
#endif
        }
    }
};
