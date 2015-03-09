//-----------------------------------------------------------------------------
//
//                   ** WARNING! ** 
//    This file was generated automatically by a tool.
//    Re-running the tool will overwrite this file.
//    You should copy this file to a custom location
//    before adding any customization in the copy to
//    prevent loss of your changes when the tool is
//    re-run.
//
//-----------------------------------------------------------------------------


#ifndef _CORLIB_NATIVE_H_
#define _CORLIB_NATIVE_H_

struct Library_corlib_native_System_Object
{
    TINYCLR_NATIVE_DECLARE(Equals___BOOLEAN__OBJECT);
    TINYCLR_NATIVE_DECLARE(GetHashCode___I4);
    TINYCLR_NATIVE_DECLARE(GetType___SystemType);
    TINYCLR_NATIVE_DECLARE(MemberwiseClone___OBJECT);
    TINYCLR_NATIVE_DECLARE(ReferenceEquals___STATIC__BOOLEAN__OBJECT__OBJECT);

    //--//

};

/* // THIS CLASS IS BUILT INTO mscorlib and we need to offset static fields accordingly
struct Library_corlib_native__<PrivateImplementationDetails>{232B98F2-8B22-4CE9-8AB0-12C746226FA4}
{
    static const int FIELD_STATIC__$$method0x60003e6-1 = 0;
    static const int FIELD_STATIC__$$method0x60003e6-2 = 1;
    static const int FIELD_STATIC__$$method0x60003e6-3 = 2;


    //--//

};
*/

struct Library_corlib_native_System_ValueType
{
    TINYCLR_NATIVE_DECLARE(Equals___BOOLEAN__OBJECT);

    //--//

};

struct Library_corlib_native_System_Collections_Hashtable
{
    static const int FIELD___buckets         = 1;
    static const int FIELD___numberOfBuckets = 2;
    static const int FIELD___count           = 3;
    static const int FIELD___loadFactor      = 4;
    static const int FIELD___maxLoadFactor   = 5;
    static const int FIELD___growthFactor    = 6;


    //--//

};

struct Library_corlib_native_System_Collections_Hashtable__Entry
{
    static const int FIELD__key   = 1;
    static const int FIELD__value = 2;
    static const int FIELD__next  = 3;


    //--//

};

struct Library_corlib_native_System_Collections_Hashtable__HashtableEnumerator
{
    static const int FIELD__ht         = 1;
    static const int FIELD__temp       = 2;
    static const int FIELD__index      = 3;
    static const int FIELD__returnType = 4;


    //--//

};

struct Library_corlib_native_System_Exception
{
    static const int FIELD___message         = 1;
    static const int FIELD__m_innerException = 2;
    static const int FIELD__m_stackTrace     = 3;
    static const int FIELD__m_HResult        = 4;

    TINYCLR_NATIVE_DECLARE(get_StackTrace___STRING);

    //--//

    struct StackTrace
    {
        CLR_RT_MethodDef_Index m_md;
        CLR_UINT32             m_IP;
    };

    static HRESULT CreateInstance( CLR_RT_HeapBlock& ref,                                    HRESULT hr, CLR_RT_StackFrame* stack );
    static HRESULT CreateInstance( CLR_RT_HeapBlock& ref, const CLR_RT_TypeDef_Index& cls  , HRESULT hr, CLR_RT_StackFrame* stack );
    static HRESULT SetStackTrace ( CLR_RT_HeapBlock& ref,                                                CLR_RT_StackFrame* stack );

    static CLR_RT_HeapBlock* GetTarget    ( CLR_RT_HeapBlock& ref                    );
    static StackTrace*       GetStackTrace( CLR_RT_HeapBlock* obj, CLR_UINT32& depth );

    //--//

    static LPCSTR     GetMessage( CLR_RT_HeapBlock* obj ) { return obj[ Library_corlib_native_System_Exception::FIELD___message  ].RecoverString()  ; }
    static CLR_UINT32 GetHResult( CLR_RT_HeapBlock* obj ) { return obj[ Library_corlib_native_System_Exception::FIELD__m_HResult ].NumericByRef().u4; }
};


struct Library_corlib_native_System_Collections_Hashtable__KeyCollection
{
    static const int FIELD__ht = 1;


    //--//

};

struct Library_corlib_native_System_Array
{
    TINYCLR_NATIVE_DECLARE(System_Collections_IList_get_Item___OBJECT__I4);
    TINYCLR_NATIVE_DECLARE(System_Collections_IList_set_Item___VOID__I4__OBJECT);
    TINYCLR_NATIVE_DECLARE(get_Length___I4);
    TINYCLR_NATIVE_DECLARE(CreateInstance___STATIC__SystemArray__SystemType__I4);
    TINYCLR_NATIVE_DECLARE(Copy___STATIC__VOID__SystemArray__I4__SystemArray__I4__I4);
    TINYCLR_NATIVE_DECLARE(Clear___STATIC__VOID__SystemArray__I4__I4);
    TINYCLR_NATIVE_DECLARE(TrySZIndexOf___STATIC__BOOLEAN__SystemArray__I4__I4__OBJECT__BYREF_I4);

    //--//

    static HRESULT Clear       ( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& arg   , int index   ,                                         int length                                            );
    static HRESULT Copy        ( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& argSrc, int indexSrc, CLR_RT_HeapBlock& argDst, int indexDst, int length                                            );
    static HRESULT TrySZIndexOf( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& argSrc,               CLR_RT_HeapBlock& match , int start   , int stop  , bool fForward, CLR_RT_HeapBlock& retValue );
};

struct Library_corlib_native_System_Array__SZArrayEnumerator
{
    static const int FIELD___array       = 1;
    static const int FIELD___index       = 2;
    static const int FIELD___endIndex    = 3;
    static const int FIELD___startIndex  = 4;
    static const int FIELD___arrayLength = 5;


    //--//

};

struct Library_corlib_native_System_Globalization_Resources_CultureInfo
{
    static const int FIELD_STATIC__manager = 3;


    //--//

};

struct Library_corlib_native_System_AppDomain
{
    static const int FIELD__m_appDomain    = 1;
    static const int FIELD__m_friendlyName = 2;

    TINYCLR_NATIVE_DECLARE(GetAssemblies___SZARRAY_SystemReflectionAssembly);
    TINYCLR_NATIVE_DECLARE(LoadInternal___SystemReflectionAssembly__STRING__BOOLEAN__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(CreateDomain___STATIC__SystemAppDomain__STRING);
    TINYCLR_NATIVE_DECLARE(Unload___STATIC__VOID__SystemAppDomain);

    //--//
#if defined(TINYCLR_APPDOMAINS)
    static HRESULT GetAppDomain( CLR_RT_HeapBlock& ref, CLR_RT_AppDomain*& appDomain, CLR_RT_AppDomain*& appDomainSav, bool fCheckForUnloadingAppDomain );    
#endif
};

struct Library_corlib_native_System_ArgumentException
{
    static const int FIELD__m_paramName = 5;


    //--//

};

struct Library_corlib_native_System_Delegate
{
    TINYCLR_NATIVE_DECLARE(Equals___BOOLEAN__OBJECT);
    TINYCLR_NATIVE_DECLARE(get_Method___SystemReflectionMethodInfo);
    TINYCLR_NATIVE_DECLARE(get_Target___OBJECT);
    TINYCLR_NATIVE_DECLARE(Combine___STATIC__SystemDelegate__SystemDelegate__SystemDelegate);
    TINYCLR_NATIVE_DECLARE(Remove___STATIC__SystemDelegate__SystemDelegate__SystemDelegate);
    TINYCLR_NATIVE_DECLARE(op_Equality___STATIC__BOOLEAN__SystemDelegate__SystemDelegate);
    TINYCLR_NATIVE_DECLARE(op_Inequality___STATIC__BOOLEAN__SystemDelegate__SystemDelegate);

    //--//

    static CLR_RT_HeapBlock_Delegate* GetLastDelegate( CLR_RT_HeapBlock_Delegate* dlg );
};

struct Library_corlib_native_System_MulticastDelegate
{
    TINYCLR_NATIVE_DECLARE(op_Equality___STATIC__BOOLEAN__SystemMulticastDelegate__SystemMulticastDelegate);
    TINYCLR_NATIVE_DECLARE(op_Inequality___STATIC__BOOLEAN__SystemMulticastDelegate__SystemMulticastDelegate);

    //--//

};

struct Library_corlib_native_System_BitConverter
{
    TINYCLR_NATIVE_DECLARE(get_IsLittleEndian___STATIC__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(DoubleToInt64Bits___STATIC__I8__R8);
    TINYCLR_NATIVE_DECLARE(GetBytes___STATIC__SZARRAY_U1__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(GetBytes___STATIC__SZARRAY_U1__CHAR);
    TINYCLR_NATIVE_DECLARE(GetBytes___STATIC__SZARRAY_U1__R8);
    TINYCLR_NATIVE_DECLARE(GetBytes___STATIC__SZARRAY_U1__R4);
    TINYCLR_NATIVE_DECLARE(GetBytes___STATIC__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(GetBytes___STATIC__SZARRAY_U1__I8);
    TINYCLR_NATIVE_DECLARE(GetBytes___STATIC__SZARRAY_U1__I2);
    TINYCLR_NATIVE_DECLARE(GetBytes___STATIC__SZARRAY_U1__U4);
    TINYCLR_NATIVE_DECLARE(GetBytes___STATIC__SZARRAY_U1__U8);
    TINYCLR_NATIVE_DECLARE(GetBytes___STATIC__SZARRAY_U1__U2);
    TINYCLR_NATIVE_DECLARE(Int64BitsToDouble___STATIC__R8__I8);
    TINYCLR_NATIVE_DECLARE(ToBoolean___STATIC__BOOLEAN__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(ToChar___STATIC__CHAR__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(ToDouble___STATIC__R8__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(ToInt16___STATIC__I2__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(ToInt32___STATIC__I4__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(ToInt64___STATIC__I8__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(ToSingle___STATIC__R4__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(ToString___STATIC__STRING__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(ToString___STATIC__STRING__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(ToString___STATIC__STRING__SZARRAY_U1__I4__I4);
    TINYCLR_NATIVE_DECLARE(ToUInt16___STATIC__U2__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(ToUInt32___STATIC__U4__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(ToUInt64___STATIC__U8__SZARRAY_U1__I4);

    //--//

};

struct Library_corlib_native_System_Boolean
{
    static const int FIELD_STATIC__FalseString = 4;
    static const int FIELD_STATIC__TrueString  = 5;

    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_Byte
{
    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_Char
{
    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_Collections_ArrayList
{
    static const int FIELD___items = 1;
    static const int FIELD___size  = 2;

    TINYCLR_NATIVE_DECLARE(get_Item___OBJECT__I4);
    TINYCLR_NATIVE_DECLARE(set_Item___VOID__I4__OBJECT);
    TINYCLR_NATIVE_DECLARE(Add___I4__OBJECT);
    TINYCLR_NATIVE_DECLARE(Clear___VOID);
    TINYCLR_NATIVE_DECLARE(Insert___VOID__I4__OBJECT);
    TINYCLR_NATIVE_DECLARE(RemoveAt___VOID__I4);
    TINYCLR_NATIVE_DECLARE(SetCapacity___VOID__I4);

    //--//

};

struct Library_corlib_native_System_Collections_DictionaryEntry
{
    static const int FIELD__Key   = 1;
    static const int FIELD__Value = 2;


    //--//

};

struct Library_corlib_native_System_Collections_Queue
{
    static const int FIELD___array = 1;
    static const int FIELD___head  = 2;
    static const int FIELD___tail  = 3;
    static const int FIELD___size  = 4;

    TINYCLR_NATIVE_DECLARE(Clear___VOID);
    TINYCLR_NATIVE_DECLARE(CopyTo___VOID__SystemArray__I4);
    TINYCLR_NATIVE_DECLARE(Enqueue___VOID__OBJECT);
    TINYCLR_NATIVE_DECLARE(Dequeue___OBJECT);
    TINYCLR_NATIVE_DECLARE(Peek___OBJECT);

    //--//

};

struct Library_corlib_native_System_Collections_Stack
{
    static const int FIELD___array = 1;
    static const int FIELD___size  = 2;

    TINYCLR_NATIVE_DECLARE(Clear___VOID);
    TINYCLR_NATIVE_DECLARE(Peek___OBJECT);
    TINYCLR_NATIVE_DECLARE(Pop___OBJECT);
    TINYCLR_NATIVE_DECLARE(Push___VOID__OBJECT);

    //--//

};

struct Library_corlib_native_System_Convert
{
    static const int FIELD_STATIC__s_rgchBase64EncodingDefault = 6;
    static const int FIELD_STATIC__s_rgchBase64EncodingRFC4648 = 7;
    static const int FIELD_STATIC__s_rgchBase64Encoding        = 8;
    static const int FIELD_STATIC__s_rgbBase64Decode           = 9;


    //--//

};

struct Library_corlib_native_System_TimeZone
{
    static const int FIELD__m_id = 1;

    TINYCLR_NATIVE_DECLARE(GetTimeZoneOffset___STATIC__I8);

    //--//

};

struct Library_corlib_native_System_CurrentSystemTimeZone
{
    static const int FIELD__m_ticksOffset = 2;


    //--//

};

struct Library_corlib_native_System_DateTime
{
    static const int FIELD_STATIC__MinValue      = 10;
    static const int FIELD_STATIC__MaxValue      = 11;
    static const int FIELD_STATIC__ticksAtOrigin = 12;

    static const int FIELD__m_ticks = 1;

    TINYCLR_NATIVE_DECLARE(_ctor___VOID__I4__I4__I4__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(get_Day___I4);
    TINYCLR_NATIVE_DECLARE(get_DayOfWeek___SystemDayOfWeek);
    TINYCLR_NATIVE_DECLARE(get_DayOfYear___I4);
    TINYCLR_NATIVE_DECLARE(get_Hour___I4);
    TINYCLR_NATIVE_DECLARE(get_Millisecond___I4);
    TINYCLR_NATIVE_DECLARE(get_Minute___I4);
    TINYCLR_NATIVE_DECLARE(get_Month___I4);
    TINYCLR_NATIVE_DECLARE(get_Second___I4);
    TINYCLR_NATIVE_DECLARE(get_Year___I4);
    TINYCLR_NATIVE_DECLARE(ToLocalTime___SystemDateTime);
    TINYCLR_NATIVE_DECLARE(ToUniversalTime___SystemDateTime);
    TINYCLR_NATIVE_DECLARE(DaysInMonth___STATIC__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(get_Now___STATIC__SystemDateTime);
    TINYCLR_NATIVE_DECLARE(get_UtcNow___STATIC__SystemDateTime);
    TINYCLR_NATIVE_DECLARE(get_Today___STATIC__SystemDateTime);

    //--//

    static CLR_INT64* NewObject  ( CLR_RT_StackFrame& stack );
    static CLR_INT64* GetValuePtr( CLR_RT_StackFrame& stack );
    static CLR_INT64* GetValuePtr( CLR_RT_HeapBlock&  ref   );

    static void Expand  ( CLR_RT_StackFrame& stack,       SYSTEMTIME& st );
    static void Compress( CLR_RT_StackFrame& stack, const SYSTEMTIME& st );
};

struct Library_corlib_native_System_Diagnostics_Debugger
{
    TINYCLR_NATIVE_DECLARE(get_IsAttached___STATIC__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(Break___STATIC__VOID);

    //--//

};

struct Library_corlib_native_System_Double
{
    static const int FIELD__m_value = 1;

    TINYCLR_NATIVE_DECLARE(CompareTo___STATIC__I4__R8__R8);
    TINYCLR_NATIVE_DECLARE(IsInfinity___STATIC__BOOLEAN__R8);
    TINYCLR_NATIVE_DECLARE(IsNaN___STATIC__BOOLEAN__R8);
    TINYCLR_NATIVE_DECLARE(IsNegativeInfinity___STATIC__BOOLEAN__R8);
    TINYCLR_NATIVE_DECLARE(IsPositiveInfinity___STATIC__BOOLEAN__R8);

    //--//

};

struct Library_corlib_native_System_GC
{
    TINYCLR_NATIVE_DECLARE(AnyPendingFinalizers___STATIC__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(SuppressFinalize___STATIC__VOID__OBJECT);
    TINYCLR_NATIVE_DECLARE(ReRegisterForFinalize___STATIC__VOID__OBJECT);

    //--//

};

struct Library_corlib_native_System_Globalization_CultureInfo
{
    static const int FIELD__numInfo      = 1;
    static const int FIELD__dateTimeInfo = 2;
    static const int FIELD__m_name       = 3;
    static const int FIELD__m_rm         = 4;
    static const int FIELD__m_parent     = 5;
   
    TINYCLR_NATIVE_DECLARE(get_CurrentUICultureInternal___STATIC__SystemGlobalizationCultureInfo);
    TINYCLR_NATIVE_DECLARE(set_CurrentUICultureInternal___STATIC__VOID__SystemGlobalizationCultureInfo);

    //--//

};

struct Library_corlib_native_System_Globalization_DateTimeFormat
{
    TINYCLR_NATIVE_DECLARE(FormatDigits___STATIC__STRING__I4__I4);

    //--//

};

struct Library_corlib_native_System_Globalization_DateTimeFormatInfo
{
    static const int FIELD__amDesignator            = 1;
    static const int FIELD__pmDesignator            = 2;
    static const int FIELD__dateSeparator           = 3;
    static const int FIELD__longTimePattern         = 4;
    static const int FIELD__shortTimePattern        = 5;
    static const int FIELD__generalShortTimePattern = 6;
    static const int FIELD__generalLongTimePattern  = 7;
    static const int FIELD__timeSeparator           = 8;
    static const int FIELD__monthDayPattern         = 9;
    static const int FIELD__fullDateTimePattern     = 10;
    static const int FIELD__longDatePattern         = 11;
    static const int FIELD__shortDatePattern        = 12;
    static const int FIELD__yearMonthPattern        = 13;
    static const int FIELD__abbreviatedDayNames     = 14;
    static const int FIELD__dayNames                = 15;
    static const int FIELD__abbreviatedMonthNames   = 16;
    static const int FIELD__monthNames              = 17;
    static const int FIELD__m_cultureInfo           = 18;


    //--//

};

struct Library_corlib_native_System_Globalization_DaylightTime
{
    static const int FIELD__m_start = 1;
    static const int FIELD__m_end   = 2;
    static const int FIELD__m_delta = 3;


    //--//

};

struct Library_corlib_native_System_Globalization_NumberFormatInfo
{
    static const int FIELD__numberGroupSizes       = 1;
    static const int FIELD__positiveSign           = 2;
    static const int FIELD__negativeSign           = 3;
    static const int FIELD__numberDecimalSeparator = 4;
    static const int FIELD__numberGroupSeparator   = 5;
    static const int FIELD__m_cultureInfo          = 6;


    //--//

};

struct Library_corlib_native_System_Guid
{
    static const int FIELD_STATIC__m_rand = 13;
    static const int FIELD_STATIC__Empty  = 14;

    static const int FIELD__m_data        = 1;


    //--//

};

struct Library_corlib_native_System_Int16
{
    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_Int32
{
    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_Int64
{
    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_Math
{
    TINYCLR_NATIVE_DECLARE(Acos___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Asin___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Atan___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Atan2___STATIC__R8__R8__R8);
    TINYCLR_NATIVE_DECLARE(Ceiling___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Cos___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Cosh___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(IEEERemainder___STATIC__R8__R8__R8);
    TINYCLR_NATIVE_DECLARE(Exp___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Floor___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Log___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Log10___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Pow___STATIC__R8__R8__R8);
    TINYCLR_NATIVE_DECLARE(Round___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Sign___STATIC__I4__R8);
    TINYCLR_NATIVE_DECLARE(Sin___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Sinh___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Sqrt___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Tan___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Tanh___STATIC__R8__R8);
    TINYCLR_NATIVE_DECLARE(Truncate___STATIC__R8__R8);

    //--//

};

struct Library_corlib_native_System_Number
{
    TINYCLR_NATIVE_DECLARE(FormatNative___STATIC__STRING__OBJECT__CHAR__I4);

    //--//

};

struct Library_corlib_native_System_Random
{
    static const int FIELD___random = 1;

    TINYCLR_NATIVE_DECLARE(Next___I4);
    TINYCLR_NATIVE_DECLARE(Next___I4__I4);
    TINYCLR_NATIVE_DECLARE(NextDouble___R8);
    TINYCLR_NATIVE_DECLARE(NextBytes___VOID__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__I4);

    //--//

    static HRESULT GetRandom( CLR_RT_StackFrame& stack, CLR_RT_Random*& rand, bool create = false );
};

struct Library_corlib_native_System_Reflection_Assembly
{
    TINYCLR_NATIVE_DECLARE(get_FullName___STRING);
    TINYCLR_NATIVE_DECLARE(GetType___SystemType__STRING);
    TINYCLR_NATIVE_DECLARE(GetTypes___SZARRAY_SystemType);
    TINYCLR_NATIVE_DECLARE(GetVersion___VOID__BYREF_I4__BYREF_I4__BYREF_I4__BYREF_I4);
    TINYCLR_NATIVE_DECLARE(GetManifestResourceNames___SZARRAY_STRING);
    TINYCLR_NATIVE_DECLARE(GetExecutingAssembly___STATIC__SystemReflectionAssembly);
    TINYCLR_NATIVE_DECLARE(LoadInternal___STATIC__SystemReflectionAssembly__STRING__BOOLEAN__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(Load___STATIC__SystemReflectionAssembly__SZARRAY_U1);

    //--//

    static HRESULT GetTypeDescriptor( CLR_RT_HeapBlock& arg, CLR_RT_Assembly_Instance& inst );
};

struct Library_corlib_native_System_Reflection_AssemblyName
{
    static const int FIELD___assembly = 1;


    //--//

};

struct Library_corlib_native_System_Reflection_MethodBase
{
    TINYCLR_NATIVE_DECLARE(get_Name___STRING);
    TINYCLR_NATIVE_DECLARE(get_DeclaringType___SystemType);
    TINYCLR_NATIVE_DECLARE(get_IsPublic___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsStatic___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsFinal___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsVirtual___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsAbstract___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(Invoke___OBJECT__OBJECT__SZARRAY_OBJECT);

    //--//

    static HRESULT GetMethodDescriptor( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& arg, CLR_RT_MethodDef_Instance& inst );

    static HRESULT CheckFlags( CLR_RT_StackFrame& stack, CLR_UINT32 mask, CLR_UINT32 flag );
};

struct Library_corlib_native_System_Reflection_ConstructorInfo
{
    TINYCLR_NATIVE_DECLARE(Invoke___OBJECT__SZARRAY_OBJECT);

    //--//

};

struct Library_corlib_native_System_Reflection_FieldInfo
{
    TINYCLR_NATIVE_DECLARE(SetValue___VOID__OBJECT__OBJECT);

    //--//

    static HRESULT Initialize( CLR_RT_StackFrame& stack, CLR_RT_FieldDef_Instance& instFD, CLR_RT_TypeDef_Instance& instTD, CLR_RT_HeapBlock*& obj );
};

struct Library_corlib_native_System_Reflection_PropertyInfo
{
    TINYCLR_NATIVE_DECLARE(GetValue___OBJECT__OBJECT__SZARRAY_OBJECT);
    TINYCLR_NATIVE_DECLARE(SetValue___VOID__OBJECT__OBJECT__SZARRAY_OBJECT);

    //--//

};

struct Library_corlib_native_System_Reflection_RuntimeFieldInfo
{
    TINYCLR_NATIVE_DECLARE(get_Name___STRING);
    TINYCLR_NATIVE_DECLARE(get_DeclaringType___SystemType);
    TINYCLR_NATIVE_DECLARE(get_FieldType___SystemType);
    TINYCLR_NATIVE_DECLARE(GetValue___OBJECT__OBJECT);

    //--//

    static bool GetFieldDescriptor( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& arg, CLR_RT_FieldDef_Instance& inst );
};

struct Library_corlib_native_System_Reflection_RuntimeMethodInfo
{
    TINYCLR_NATIVE_DECLARE(get_ReturnType___SystemType);

    //--//

};

struct Library_corlib_native_System_Resources_ResourceManager
{
    static const int FIELD__m_resourceFileId = 1;
    static const int FIELD__m_assembly       = 2;
    static const int FIELD__m_baseAssembly   = 3;
    static const int FIELD__m_baseName       = 4;
    static const int FIELD__m_cultureName    = 5;
    static const int FIELD__m_rmFallback     = 6;

    TINYCLR_NATIVE_DECLARE(GetObjectInternal___OBJECT__I2);
    TINYCLR_NATIVE_DECLARE(GetObjectInternal___OBJECT__I2__I4__I4);
    TINYCLR_NATIVE_DECLARE(FindResource___STATIC__I4__STRING__SystemReflectionAssembly);
    TINYCLR_NATIVE_DECLARE(GetObject___STATIC__OBJECT__SystemResourcesResourceManager__SystemEnum);

    //--//

    TINYCLR_NATIVE_DECLARE(GetObject___STATIC__OBJECT__SystemResourcesResourceManager__SystemEnum__I4__I4);    

};

struct Library_corlib_native_System_Runtime_CompilerServices_AccessedThroughPropertyAttribute
{
    static const int FIELD__propertyName = 1;


    //--//

};

struct Library_corlib_native_System_Runtime_CompilerServices_RuntimeHelpers
{
    TINYCLR_NATIVE_DECLARE(InitializeArray___STATIC__VOID__SystemArray__SystemRuntimeFieldHandle);
    TINYCLR_NATIVE_DECLARE(GetObjectValue___STATIC__OBJECT__OBJECT);
    TINYCLR_NATIVE_DECLARE(RunClassConstructor___STATIC__VOID__SystemRuntimeTypeHandle);
    TINYCLR_NATIVE_DECLARE(get_OffsetToStringData___STATIC__I4);

    //--//

};

struct Library_corlib_native_System_Runtime_Remoting_RemotingServices
{
    TINYCLR_NATIVE_DECLARE(IsTransparentProxy___STATIC__BOOLEAN__OBJECT);

    //--//

};

struct Library_corlib_native_System_Runtime_Versioning_TargetFrameworkAttribute
{
    static const int FIELD___frameworkName = 1;
    static const int FIELD___frameworkDisplayName = 2;


    //--//

};

struct Library_corlib_native_System_Type
{
    TINYCLR_NATIVE_DECLARE(get_DeclaringType___SystemType);
    TINYCLR_NATIVE_DECLARE(GetMethod___SystemReflectionMethodInfo__STRING__SystemReflectionBindingFlags);
    TINYCLR_NATIVE_DECLARE(IsInstanceOfType___BOOLEAN__OBJECT);
    TINYCLR_NATIVE_DECLARE(InvokeMember___OBJECT__STRING__SystemReflectionBindingFlags__SystemReflectionBinder__OBJECT__SZARRAY_OBJECT);
    TINYCLR_NATIVE_DECLARE(GetConstructor___SystemReflectionConstructorInfo__SZARRAY_SystemType);
    TINYCLR_NATIVE_DECLARE(GetMethod___SystemReflectionMethodInfo__STRING__SZARRAY_SystemType);
    TINYCLR_NATIVE_DECLARE(GetMethod___SystemReflectionMethodInfo__STRING);
    TINYCLR_NATIVE_DECLARE(get_IsNotPublic___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsPublic___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsClass___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsInterface___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsValueType___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsAbstract___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsEnum___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsSerializable___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_IsArray___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(GetTypeInternal___STATIC__SystemType__STRING__STRING__BOOLEAN__SZARRAY_I4);
    TINYCLR_NATIVE_DECLARE(GetTypeFromHandle___STATIC__SystemType__SystemRuntimeTypeHandle);

    //--//

    //
    static const int c_BindingFlags_Default              = 0x00000000; // a place holder for no flag specifed

    static const int c_BindingFlags_IgnoreCase           = 0x00000001; // Ignore the case of Names while searching
    static const int c_BindingFlags_DeclaredOnly         = 0x00000002; // Only look at the members declared on the Type
    static const int c_BindingFlags_Instance             = 0x00000004; // Include Instance members in search
    static const int c_BindingFlags_Static               = 0x00000008; // Include Static members in search
    static const int c_BindingFlags_Public               = 0x00000010; // Include Public members in search
    static const int c_BindingFlags_NonPublic            = 0x00000020; // Include Non-Public members in search
    static const int c_BindingFlags_FlattenHierarchy     = 0x00000040; // Rollup the statics into the class.

    static const int c_BindingFlags_InvokeMethod         = 0x00000100;
    static const int c_BindingFlags_CreateInstance       = 0x00000200;
    static const int c_BindingFlags_GetField             = 0x00000400;
    static const int c_BindingFlags_SetField             = 0x00000800;
    static const int c_BindingFlags_GetProperty          = 0x00001000;
    static const int c_BindingFlags_SetProperty          = 0x00002000;

    static const int c_BindingFlags_PutDispProperty      = 0x00004000;
    static const int c_BindingFlags_PutRefDispProperty   = 0x00008000;

    static const int c_BindingFlags_ExactBinding         = 0x00010000; // Bind with Exact Type matching, No Change type
    static const int c_BindingFlags_SuppressChangeType   = 0x00020000;

    static const int c_BindingFlags_OptionalParamBinding = 0x00040000;
    static const int c_BindingFlags_IgnoreReturn         = 0x01000000;  // This is used in COM Interop

    //--//

    static const int c_BindingFlags_DefaultLookup = c_BindingFlags_Instance | c_BindingFlags_Static | c_BindingFlags_Public;

    //--//

    static HRESULT CheckFlags( CLR_RT_StackFrame& stack, CLR_UINT32 mask, CLR_UINT32 flag );

    static HRESULT GetFields ( CLR_RT_StackFrame& stack, LPCSTR szText, CLR_UINT32 attr                                        , bool fAllMatches );
    static HRESULT GetMethods( CLR_RT_StackFrame& stack, LPCSTR szText, CLR_UINT32 attr, CLR_RT_HeapBlock* pParams, int iParams, bool fAllMatches );
};

struct Library_corlib_native_System_RuntimeType
{
    TINYCLR_NATIVE_DECLARE(get_Assembly___SystemReflectionAssembly);
    TINYCLR_NATIVE_DECLARE(get_Name___STRING);
    TINYCLR_NATIVE_DECLARE(get_FullName___STRING);
    TINYCLR_NATIVE_DECLARE(get_BaseType___SystemType);
    TINYCLR_NATIVE_DECLARE(GetMethods___SZARRAY_SystemReflectionMethodInfo__SystemReflectionBindingFlags);
    TINYCLR_NATIVE_DECLARE(GetField___SystemReflectionFieldInfo__STRING__SystemReflectionBindingFlags);
    TINYCLR_NATIVE_DECLARE(GetFields___SZARRAY_SystemReflectionFieldInfo__SystemReflectionBindingFlags);
    TINYCLR_NATIVE_DECLARE(GetInterfaces___SZARRAY_SystemType);
    TINYCLR_NATIVE_DECLARE(GetElementType___SystemType);

    //--//

    static HRESULT GetTypeDescriptor( CLR_RT_HeapBlock& arg, CLR_RT_TypeDef_Instance& inst, CLR_UINT32* levels );
    static HRESULT GetTypeDescriptor( CLR_RT_HeapBlock& arg, CLR_RT_TypeDef_Instance& inst );
    static HRESULT GetName( CLR_RT_HeapBlock& arg, bool fFullName, CLR_RT_HeapBlock& res );
};

struct Library_corlib_native_System_SByte
{
    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_Single
{
    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_String
{
    static const int FIELD_STATIC__Empty = 15;

    TINYCLR_NATIVE_DECLARE(CompareTo___I4__OBJECT);
    TINYCLR_NATIVE_DECLARE(get_Chars___CHAR__I4);
    TINYCLR_NATIVE_DECLARE(ToCharArray___SZARRAY_CHAR);
    TINYCLR_NATIVE_DECLARE(ToCharArray___SZARRAY_CHAR__I4__I4);
    TINYCLR_NATIVE_DECLARE(get_Length___I4);
    TINYCLR_NATIVE_DECLARE(Split___SZARRAY_STRING__SZARRAY_CHAR);
    TINYCLR_NATIVE_DECLARE(Split___SZARRAY_STRING__SZARRAY_CHAR__I4);
    TINYCLR_NATIVE_DECLARE(Substring___STRING__I4);
    TINYCLR_NATIVE_DECLARE(Substring___STRING__I4__I4);
    TINYCLR_NATIVE_DECLARE(Trim___STRING__SZARRAY_CHAR);
    TINYCLR_NATIVE_DECLARE(TrimStart___STRING__SZARRAY_CHAR);
    TINYCLR_NATIVE_DECLARE(TrimEnd___STRING__SZARRAY_CHAR);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__SZARRAY_CHAR__I4__I4);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__SZARRAY_CHAR);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__CHAR__I4);
    TINYCLR_NATIVE_DECLARE(CompareTo___I4__STRING);
    TINYCLR_NATIVE_DECLARE(IndexOf___I4__CHAR);
    TINYCLR_NATIVE_DECLARE(IndexOf___I4__CHAR__I4);
    TINYCLR_NATIVE_DECLARE(IndexOf___I4__CHAR__I4__I4);
    TINYCLR_NATIVE_DECLARE(IndexOfAny___I4__SZARRAY_CHAR);
    TINYCLR_NATIVE_DECLARE(IndexOfAny___I4__SZARRAY_CHAR__I4);
    TINYCLR_NATIVE_DECLARE(IndexOfAny___I4__SZARRAY_CHAR__I4__I4);
    TINYCLR_NATIVE_DECLARE(IndexOf___I4__STRING);
    TINYCLR_NATIVE_DECLARE(IndexOf___I4__STRING__I4);
    TINYCLR_NATIVE_DECLARE(IndexOf___I4__STRING__I4__I4);
    TINYCLR_NATIVE_DECLARE(LastIndexOf___I4__CHAR);
    TINYCLR_NATIVE_DECLARE(LastIndexOf___I4__CHAR__I4);
    TINYCLR_NATIVE_DECLARE(LastIndexOf___I4__CHAR__I4__I4);
    TINYCLR_NATIVE_DECLARE(LastIndexOfAny___I4__SZARRAY_CHAR);
    TINYCLR_NATIVE_DECLARE(LastIndexOfAny___I4__SZARRAY_CHAR__I4);
    TINYCLR_NATIVE_DECLARE(LastIndexOfAny___I4__SZARRAY_CHAR__I4__I4);
    TINYCLR_NATIVE_DECLARE(LastIndexOf___I4__STRING);
    TINYCLR_NATIVE_DECLARE(LastIndexOf___I4__STRING__I4);
    TINYCLR_NATIVE_DECLARE(LastIndexOf___I4__STRING__I4__I4);
    TINYCLR_NATIVE_DECLARE(ToLower___STRING);
    TINYCLR_NATIVE_DECLARE(ToUpper___STRING);
    TINYCLR_NATIVE_DECLARE(Trim___STRING);
    TINYCLR_NATIVE_DECLARE(Equals___STATIC__BOOLEAN__STRING__STRING);
    TINYCLR_NATIVE_DECLARE(op_Equality___STATIC__BOOLEAN__STRING__STRING);
    TINYCLR_NATIVE_DECLARE(op_Inequality___STATIC__BOOLEAN__STRING__STRING);
    TINYCLR_NATIVE_DECLARE(Compare___STATIC__I4__STRING__STRING);
    TINYCLR_NATIVE_DECLARE(Concat___STATIC__STRING__STRING__STRING);
    TINYCLR_NATIVE_DECLARE(Concat___STATIC__STRING__STRING__STRING__STRING);
    TINYCLR_NATIVE_DECLARE(Concat___STATIC__STRING__STRING__STRING__STRING__STRING);
    TINYCLR_NATIVE_DECLARE(Concat___STATIC__STRING__SZARRAY_STRING);

    //--//

    static const int c_IndexOf__SingleChar    = 0x00000001;
    static const int c_IndexOf__MultipleChars = 0x00000002;
    static const int c_IndexOf__String        = 0x00000004;
    static const int c_IndexOf__StartIndex    = 0x00000010;
    static const int c_IndexOf__Count         = 0x00000020;
    static const int c_IndexOf__Last          = 0x00000040;

    static HRESULT FromCharArray( CLR_RT_StackFrame& stack, int startIndex, int count );
    static HRESULT ToCharArray  ( CLR_RT_StackFrame& stack, int startIndex, int count );
    static HRESULT IndexOf      ( CLR_RT_StackFrame& stack, int mode                  );
    static HRESULT ChangeCase   ( CLR_RT_StackFrame& stack, bool fToUpper             );
    static HRESULT Substring    ( CLR_RT_StackFrame& stack, int startIndex, int count );


    static HRESULT Trim( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock_Array* arrayTrimChars, bool fStart, bool fEnd );

    static HRESULT Split( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& chars, int maxStrings );

    //--//

    static HRESULT Concat( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock* array, int num );

    static HRESULT ConvertToCharArray( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& ref, CLR_RT_HeapBlock_Array*& array, int startIndex, int length );
    static HRESULT ConvertToCharArray( LPCSTR szText           , CLR_RT_HeapBlock& ref, CLR_RT_HeapBlock_Array*& array, int startIndex, int length );
};

struct Library_corlib_native_System_Text_StringBuilder
{
    static const int FIELD__m_MaxCapacity = 1;
    static const int FIELD__m_ChunkChars = 2;
    static const int FIELD__m_ChunkLength = 3;
    static const int FIELD__m_ChunkPrevious = 4;
    static const int FIELD__m_ChunkOffset = 5;


    //--//

};

struct Library_corlib_native_System_Text_UTF8Decoder
{
    TINYCLR_NATIVE_DECLARE(Convert___VOID__SZARRAY_U1__I4__I4__SZARRAY_CHAR__I4__I4__BOOLEAN__BYREF_I4__BYREF_I4__BYREF_BOOLEAN);

    //--//

};

struct Library_corlib_native_System_Text_UTF8Encoding
{
    TINYCLR_NATIVE_DECLARE(GetBytes___SZARRAY_U1__STRING);
    TINYCLR_NATIVE_DECLARE(GetBytes___I4__STRING__I4__I4__SZARRAY_U1__I4);
    TINYCLR_NATIVE_DECLARE(GetChars___SZARRAY_CHAR__SZARRAY_U1);
    TINYCLR_NATIVE_DECLARE(GetChars___SZARRAY_CHAR__SZARRAY_U1__I4__I4);

    //--//

    static HRESULT Helper__GetChars(CLR_RT_StackFrame& stack, bool fIndexed);

};

struct Library_corlib_native_System_Threading_WaitHandle
{
    TINYCLR_NATIVE_DECLARE(WaitOne___BOOLEAN__I4__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(WaitMultiple___STATIC__I4__SZARRAY_SystemThreadingWaitHandle__I4__BOOLEAN__BOOLEAN);

    //--//

    static void Set  ( CLR_RT_StackFrame& stack );
    static void Reset( CLR_RT_StackFrame& stack );

    static HRESULT Wait( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock& blkTimeout, CLR_RT_HeapBlock& blkExitContext, CLR_RT_HeapBlock* objects, int cObjects, bool fWaitAll );
};

struct Library_corlib_native_System_Threading_AutoResetEvent
{
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(Reset___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(Set___BOOLEAN);

    //--//

};

struct Library_corlib_native_System_Threading_Interlocked
{
    TINYCLR_NATIVE_DECLARE(Increment___STATIC__I4__BYREF_I4);
    TINYCLR_NATIVE_DECLARE(Decrement___STATIC__I4__BYREF_I4);
    TINYCLR_NATIVE_DECLARE(Exchange___STATIC__I4__BYREF_I4__I4);
    TINYCLR_NATIVE_DECLARE(CompareExchange___STATIC__I4__BYREF_I4__I4__I4);

    //--//

};

struct Library_corlib_native_System_Threading_ManualResetEvent
{
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(Reset___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(Set___BOOLEAN);

    //--//

};

struct Library_corlib_native_System_Threading_Monitor
{
    TINYCLR_NATIVE_DECLARE(Enter___STATIC__VOID__OBJECT);
    TINYCLR_NATIVE_DECLARE(Exit___STATIC__VOID__OBJECT);

    //--//

};

struct Library_corlib_native_System_Threading_Thread
{
    static const int FIELD__m_Delegate  = 1;
    static const int FIELD__m_Priority  = 2;
    static const int FIELD__m_Thread    = 3;
    static const int FIELD__m_AppDomain = 4;
    static const int FIELD__m_Id        = 5;

    TINYCLR_NATIVE_DECLARE(_ctor___VOID__SystemThreadingThreadStart);
    TINYCLR_NATIVE_DECLARE(Start___VOID);
    TINYCLR_NATIVE_DECLARE(Abort___VOID);
    TINYCLR_NATIVE_DECLARE(Suspend___VOID);
    TINYCLR_NATIVE_DECLARE(Resume___VOID);
    TINYCLR_NATIVE_DECLARE(get_Priority___SystemThreadingThreadPriority);
    TINYCLR_NATIVE_DECLARE(set_Priority___VOID__SystemThreadingThreadPriority);
    TINYCLR_NATIVE_DECLARE(get_ManagedThreadId___I4);
    TINYCLR_NATIVE_DECLARE(get_IsAlive___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(Join___VOID);
    TINYCLR_NATIVE_DECLARE(Join___BOOLEAN__I4);
    TINYCLR_NATIVE_DECLARE(Join___BOOLEAN__SystemTimeSpan);
    TINYCLR_NATIVE_DECLARE(get_ThreadState___SystemThreadingThreadState);
    TINYCLR_NATIVE_DECLARE(Sleep___STATIC__VOID__I4);
    TINYCLR_NATIVE_DECLARE(get_CurrentThread___STATIC__SystemThreadingThread);
    TINYCLR_NATIVE_DECLARE(GetDomain___STATIC__SystemAppDomain);

    //--//

    static CLR_RT_ObjectToEvent_Source* GetThreadReference  ( CLR_RT_StackFrame& stack                                                               );
    static void                         ResetThreadReference( CLR_RT_StackFrame& stack                                                               );
    static HRESULT                      SetThread           ( CLR_RT_StackFrame& stack, CLR_RT_Thread*  th                                           );
    static HRESULT                      GetThread           ( CLR_RT_StackFrame& stack, CLR_RT_Thread*& th, bool mustBeStarted, bool noSystemThreads );

    static HRESULT Join( CLR_RT_StackFrame& stack, const CLR_INT64& timeExpire );
};

struct Library_corlib_native_System_Threading_Timer
{
    static const int FIELD__m_timer    = 1;
    static const int FIELD__m_state    = 2;
    static const int FIELD__m_callback = 3;

    TINYCLR_NATIVE_DECLARE(Dispose___VOID);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__SystemThreadingTimerCallback__OBJECT__I4__I4);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__SystemThreadingTimerCallback__OBJECT__SystemTimeSpan__SystemTimeSpan);
    TINYCLR_NATIVE_DECLARE(Change___BOOLEAN__I4__I4);
    TINYCLR_NATIVE_DECLARE(Change___BOOLEAN__SystemTimeSpan__SystemTimeSpan);

    //--//

    static HRESULT SetValues    ( CLR_RT_StackFrame& stack, CLR_UINT32 flags );
    static bool    CheckDisposed( CLR_RT_StackFrame& stack                   );
};

struct Library_corlib_native_System_TimeSpan
{
    static const int FIELD_STATIC__Zero     = 16;
    static const int FIELD_STATIC__MaxValue = 17;
    static const int FIELD_STATIC__MinValue = 18;

    static const int FIELD__m_ticks = 1;

    TINYCLR_NATIVE_DECLARE(Equals___BOOLEAN__OBJECT);
    TINYCLR_NATIVE_DECLARE(ToString___STRING);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__I4__I4__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(CompareTo___I4__OBJECT);
    TINYCLR_NATIVE_DECLARE(Compare___STATIC__I4__SystemTimeSpan__SystemTimeSpan);
    TINYCLR_NATIVE_DECLARE(Equals___STATIC__BOOLEAN__SystemTimeSpan__SystemTimeSpan);

    //--//

    static CLR_INT64* NewObject  ( CLR_RT_StackFrame& stack );
    static CLR_INT64* GetValuePtr( CLR_RT_StackFrame& stack );
    static CLR_INT64* GetValuePtr( CLR_RT_HeapBlock&  ref   );
    
    static void ConstructTimeSpan( CLR_INT64* val, CLR_INT32 days, CLR_INT32 hours, CLR_INT32 minutes, CLR_INT32 seconds, CLR_INT32 ms );
};

struct Library_corlib_native_System_UInt16
{
    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_UInt32
{
    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_UInt64
{
    static const int FIELD__m_value = 1;


    //--//

};

struct Library_corlib_native_System_Version
{
    static const int FIELD___Major    = 1;
    static const int FIELD___Minor    = 2;
    static const int FIELD___Build    = 3;
    static const int FIELD___Revision = 4;


    //--//

};

struct Library_corlib_native_System_WeakReference
{
    TINYCLR_NATIVE_DECLARE(get_IsAlive___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_Target___OBJECT);
    TINYCLR_NATIVE_DECLARE(set_Target___VOID__OBJECT);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__OBJECT);

    //--//

};

struct Library_corlib_native_System_Collections_Hashtable__ValueCollection
{
    static const int FIELD__ht = 1;


    //--//

};

extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_mscorlib;

#endif // _CORLIB_NATIVE_H_
