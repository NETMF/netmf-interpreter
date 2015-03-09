
class SmartPtr_IRQ
{
    UINT32 m_state;
    void*  m_context;

public:

#if defined(ARM_V1_2)
// for ads1.2 compiler, if put at .cpp file, the NULL_HEAP codes will compile failed.
    SmartPtr_IRQ(void* context=NULL)  { m_context = context; Disable(); }
    ~SmartPtr_IRQ() { Restore(); }

#else
    SmartPtr_IRQ(void* context=NULL);
    ~SmartPtr_IRQ();
#endif



    BOOL WasDisabled();

    void Acquire();

    void Release();

    void Probe();

    static BOOL GetState(void* context=NULL);

    static BOOL ForceDisabled(void* context=NULL);

    static BOOL ForceEnabled(void* context=NULL);

private:
    void Disable();

    void Restore();
};

