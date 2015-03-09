////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////using namespace std;
using namespace std;

/******************************************************************************
**  CLR_RT_ManagedElementType, CLR_RT_ManagedElementTypeByRef, CLR_RT_ManagedElementTypeArray
**  are intoduced for generation of the marshaling code and stub functions.
**  Instance of each class represent one parameter of the managed method and 
**  support generation of different strings for that parameter in marshaling code. 
**
*******************************************************************************/


class CLR_RT_ManagedElementType
{   
public :  
    
    // Strutcture that associates type of managed parameter
    // and strings we use for it.
    struct CLR_RT_TypeNamesRecord
    {   CLR_DataType  type;
        LPCSTR        lpszTypeName;
        LPCSTR        lpszNativeType;
    };
protected :    
    // Data type of paramenter.
    CLR_DataType           m_dataType;
    
    // Pointer to static entry in TypeNamesTable.
    const CLR_RT_TypeNamesRecord *m_pTypeData;
    // 
    CLR_RT_TypeNamesRecord m_InvalidType;

public :  
    // Finds the entry in the table that correspond to type and set m_pTypeData
    CLR_RT_ManagedElementType( CLR_DataType dataType );
    // No need to delete m_pTypeData, it points to static data.
    // Need it as virtual since object of derived classes deleted through pointer to CLR_RT_ManagedElementType
    virtual ~CLR_RT_ManagedElementType() {}
    
    // Return the name of type like I1, I2, I4, U1 and so on.
    string GetTypeName()   { return m_pTypeData->lpszTypeName;   }
    
    // Returns the name of variable like INT8, INT16, INT32, UINT8, float and so on.
    string GetNativeType() { return m_pTypeData->lpszNativeType; }
    
    // Declaration of variable in marshaling code.
    virtual string GetVariableDecl() { return GetNativeType(); }
    
    // For value type there is no prefix. Return empty string
    virtual string GetTypeNamePrefix()  { return "\0"; }
    
    // Generate declaration of variable and marshaling call for basic type.
    virtual string GetMarshalCodeBeforeNativeCall( bool bStaticMethod, UINT32 paramIndex );
    
    // Function that generates code after native function call. 
    // This required only for reference parameters. Return empty string for basic type 
    virtual string GetMashalCodeAfterNativeCall( UINT32 paramIndex, bool bStaticMethod )  { return "\0"; }
    
    // Static function that converts unsigned integer to string. 
    static string GetParamIndexAsText( UINT32 index );

    // Return true if type represented by element is "void".
    // The void type needs special hanling since expression like "void retVal = ..." is illegal.
    bool IsVoidType() { return GetNativeType() == "void"; }

};

class CLR_RT_ManagedElementTypeByRef : public CLR_RT_ManagedElementType

{
public :  
    CLR_RT_ManagedElementTypeByRef( CLR_DataType dataType ) : CLR_RT_ManagedElementType( dataType ) {}
    virtual ~CLR_RT_ManagedElementTypeByRef() {}

    // Varialble declaration - pointer to native type;
    virtual string GetVariableDecl() { return CLR_RT_ManagedElementType::GetVariableDecl() + " *"; }
    
    // Reference types are prefixed by BYREF_ in function names.
    virtual string GetTypeNamePrefix() { return "BYREF_"; }

    // Generate declaration of variable and marshaling call for refence to basic  type.
    virtual string GetMarshalCodeBeforeNativeCall( bool bStaticMethod, UINT32 paramIndex );

    // Generates code that stores reference variable.
    virtual string GetMashalCodeAfterNativeCall( UINT32 paramIndex, bool bStaticMethod );
};

class CLR_RT_ManagedElementTypeArray : public CLR_RT_ManagedElementType
{
public :  
    CLR_RT_ManagedElementTypeArray( CLR_DataType dataType ) : CLR_RT_ManagedElementType( dataType ) {}
    virtual ~CLR_RT_ManagedElementTypeArray() {}

    // Varialble declaration - "CLR_RT_TypedArray_" of specified type;
    virtual string GetVariableDecl() { return "CLR_RT_TypedArray_" + CLR_RT_ManagedElementType::GetVariableDecl(); }
    
    // Reference types are prefixed by BYREF_ in function names.
    virtual string GetTypeNamePrefix() { return "SZARRAY_"; }

    // Generate declaration of array and marshaling call for array of basic types.
    virtual string GetMarshalCodeBeforeNativeCall( bool bStaticMethod, UINT32 paramIndex );
};

/******************************************************************************
**  CLR_RT_VectorOfManagedElements keeps array of pointers to 
**  CLR_RT_ManagedElementType or derived class.
**  The reason to introduce this class - its destructor automatically deletes 
**  all contained objects.
*******************************************************************************/

class CLR_RT_VectorOfManagedElements : public vector<CLR_RT_ManagedElementType *>
{ 
public :    
    CLR_RT_VectorOfManagedElements() {}
    
    // Destructor deletes all objects whose pointers are stored in the array
    ~CLR_RT_VectorOfManagedElements(); 
};
