using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CorDebugInterop;
using System.Collections;
using System.Reflection;

namespace Microsoft.SPOT.Debugger
{    
    public abstract class CorDebugValue : ICorDebugHeapValue, ICorDebugValue2
    {
        protected RuntimeValue m_rtv;
        protected CorDebugAppDomain m_appDomain;

        public static CorDebugValue CreateValue(RuntimeValue rtv, CorDebugAppDomain appDomain)
        {
            CorDebugValue val = null;
            bool fIsReference;
            
            if (rtv.IsBoxed)
            {
                val = new CorDebugValueBoxedObject (rtv, appDomain);
                fIsReference = true;
            }
            else if (rtv.IsPrimitive)
            {
                CorDebugClass c = ClassFromRuntimeValue (rtv, appDomain);
    
                if (c.IsEnum)
                {
                    val = new CorDebugValueObject (rtv, appDomain);
                    fIsReference = false;
                }
                else
                {
                    val = new CorDebugValuePrimitive (rtv, appDomain);
                    fIsReference = false;
                }
            }
            else if (rtv.IsArray)
            {
                val = new CorDebugValueArray (rtv, appDomain);
                fIsReference = true;
            }
            else if (rtv.CorElementType == CorElementType.ELEMENT_TYPE_STRING)
            {
                val = new CorDebugValueString (rtv, appDomain);
                fIsReference = true;
            }
            else
            {
                val = new CorDebugValueObject (rtv, appDomain);
                fIsReference = !rtv.IsValueType;
            }
            
            if (fIsReference)
            {
                val = new CorDebugValueReference(val, val.m_rtv, val.m_appDomain);
            }

            if (rtv.IsReference)    //CorElementType.ELEMENT_TYPE_BYREF
            {
                val = new CorDebugValueReferenceByRef(val, val.m_rtv, val.m_appDomain);
            }

            return val;        
        }

        public static CorDebugValue[] CreateValues(RuntimeValue[] rtv, CorDebugAppDomain appDomain)
        {
            CorDebugValue [] values = new CorDebugValue[rtv.Length];
            for (int i = 0; i < rtv.Length; i++)
            {
                values[i] = CorDebugValue.CreateValue(rtv[i], appDomain);
            }

            return values;
        }

        public static CorDebugClass ClassFromRuntimeValue(RuntimeValue rtv, CorDebugAppDomain appDomain)
        {
            RuntimeValue_Reflection rtvf = rtv as RuntimeValue_Reflection;
            CorDebugClass cls = null;
            object objBuiltInKey = null;
            Debug.Assert( !rtv.IsNull );

            if (rtvf != null)
            {
                objBuiltInKey = rtvf.ReflectionType;
            }
            else if(rtv.DataType == RuntimeDataType.DATATYPE_TRANSPARENT_PROXY)
            {
                objBuiltInKey = RuntimeDataType.DATATYPE_TRANSPARENT_PROXY;
            }
            else
            {
                cls = TinyCLR_TypeSystem.CorDebugClassFromTypeIndex( rtv.Type, appDomain ); ;
            }

            if(objBuiltInKey != null)
            {                
                CorDebugProcess.BuiltinType builtInType = appDomain.Process.ResolveBuiltInType( objBuiltInKey );             
                
                cls = builtInType.GetClass( appDomain );

                if(cls == null)
                {
                    cls = new CorDebugClass( builtInType.GetAssembly( appDomain ), builtInType.TokenCLR );
                }                
            }

            return cls;
        }

        public CorDebugValue(RuntimeValue rtv, CorDebugAppDomain appDomain)
        {
            m_rtv = rtv;                                  
            m_appDomain = appDomain;
        }

        public virtual RuntimeValue RuntimeValue
        {
            get { return m_rtv; }

            set
            {
                //This should only be used if the underlying RuntimeValue changes, but not the data
                //For example, if we ever support compaction.  For now, this is only used when the scratch
                //pad needs resizing, the RuntimeValues, and there associated heapblock*, will be relocated
                Debug.Assert (m_rtv.GetType () == value.GetType ());
                Debug.Assert(m_rtv.CorElementType == value.CorElementType || value.IsNull || m_rtv.IsNull);
                //other debug checks here...
                m_rtv = value;
            }
        }

        public CorDebugAppDomain AppDomain
        {
            get { return m_appDomain; }
        }

        protected Engine Engine
        {
            [System.Diagnostics.DebuggerHidden]
            get {return m_appDomain.Engine;}
        }        

        protected CorDebugValue CreateValue(RuntimeValue rtv)
        {
            return CorDebugValue.CreateValue(rtv, m_appDomain);
        }

        protected virtual CorElementType ElementTypeProtected
        {
            get { return m_rtv.CorElementType; }
        }

        public virtual uint Size
        {
            get { return 8; }
        }

        public virtual CorElementType Type
        {
            get { return this.ElementTypeProtected; }
        }
    
        public ICorDebugValue ICorDebugValue
        {
            get { return (ICorDebugValue)this; }
        }

        public ICorDebugHeapValue ICorDebugHeapValue
        {
            get { return (ICorDebugHeapValue)this; }
        }

        #region ICorDebugValue Members

        int ICorDebugValue.GetType( out CorElementType pType )
        {
            pType = this.Type;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugValue.GetSize( out uint pSize )
        {
            pSize = this.Size;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugValue.GetAddress( out ulong pAddress )
        {
            pAddress = m_rtv.ReferenceIdDirect;

            return Utility.COM_HResults.S_OK; 
        }

        int ICorDebugValue.CreateBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            ppBreakpoint = null;

            return Utility.COM_HResults.E_NOTIMPL;            
        }

        #endregion

        #region ICorDebugHeapValue Members

        #region ICorDebugValue

        int ICorDebugHeapValue.GetType( out CorElementType pType )
        {
            return this.ICorDebugValue.GetType( out pType );            
        }

        int ICorDebugHeapValue.GetSize( out uint pSize )
        {
            return this.ICorDebugValue.GetSize( out pSize );            
        }

        int ICorDebugHeapValue.GetAddress( out ulong pAddress )
        {
            return this.ICorDebugValue.GetAddress( out pAddress );            
        }

        int ICorDebugHeapValue.CreateBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );            
        }

        #endregion

        #region ICorDebugHeapValue

        int ICorDebugHeapValue.IsValid( out int pbValid )
        {
            pbValid = Utility.Boolean.TRUE;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugHeapValue.CreateRelocBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            ppBreakpoint = null;

            return Utility.COM_HResults.E_NOTIMPL;            
        }

        #endregion

        #endregion

        #region ICorDebugValue2 Members

        int ICorDebugValue2.GetExactType(out ICorDebugType ppType)
        {
            ppType = new CorDebugGenericType(RuntimeValue.CorElementType, m_rtv, m_appDomain);
            return Utility.COM_HResults.S_OK;
        }

        #endregion
    }

    public class CorDebugValuePrimitive : CorDebugValue, ICorDebugGenericValue
    {
        public CorDebugValuePrimitive(RuntimeValue rtv, CorDebugAppDomain appDomain) : base(rtv, appDomain)
        {
        }
        
        protected virtual object ValueProtected
        {
            get { return m_rtv.Value; }
            set { m_rtv.Value = value; }
        }

        public override uint Size
        {
            get 
            {
                object o = this.ValueProtected;
                return (uint)Marshal.SizeOf( o );
            }
        }

        public ICorDebugGenericValue ICorDebugGenericValue
        {
            get {return (ICorDebugGenericValue)this;}
        }

        #region ICorDebugGenericValue Members

        #region ICorDebugValue

        int ICorDebugGenericValue.GetType( out CorElementType pType )
        {
            return this.ICorDebugValue.GetType( out pType );            
        }

        int ICorDebugGenericValue.GetSize( out uint pSize )
        {
            return this.ICorDebugValue.GetSize( out pSize );         
        }

        int ICorDebugGenericValue.GetAddress( out ulong pAddress )
        {
            return this.ICorDebugValue.GetAddress( out pAddress );            
        }

        int ICorDebugGenericValue.CreateBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );            
        }

        #endregion

        int ICorDebugGenericValue.GetValue( IntPtr pTo )
        {
            byte[] data = null;
            object val = this.ValueProtected;

            switch(this.ElementTypeProtected)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    data = BitConverter.GetBytes( (bool)val );
                    break;

                case CorElementType.ELEMENT_TYPE_I1:
                    data = new byte[] { (byte)(sbyte)val };
                    break;

                case CorElementType.ELEMENT_TYPE_U1:
                    data = new byte[] { (byte)val };
                    break;

                case CorElementType.ELEMENT_TYPE_CHAR:
                    data = BitConverter.GetBytes( (char)val );
                    break;

                case CorElementType.ELEMENT_TYPE_I2:
                    data = BitConverter.GetBytes( (short)val );
                    break;

                case CorElementType.ELEMENT_TYPE_U2:
                    data = BitConverter.GetBytes( (ushort)val );
                    break;

                case CorElementType.ELEMENT_TYPE_I4:
                    data = BitConverter.GetBytes( (int)val );
                    break;

                case CorElementType.ELEMENT_TYPE_U4:
                    data = BitConverter.GetBytes( (uint)val );
                    break;

                case CorElementType.ELEMENT_TYPE_R4:
                    data = BitConverter.GetBytes( (float)val );
                    break;

                case CorElementType.ELEMENT_TYPE_I8:
                    data = BitConverter.GetBytes( (long)val );
                    break;

                case CorElementType.ELEMENT_TYPE_U8:
                    data = BitConverter.GetBytes( (ulong)val );
                    break;

                case CorElementType.ELEMENT_TYPE_R8:
                    data = BitConverter.GetBytes( (double)val );
                    break;

                default:
                    Debug.Assert( false );
                    break;
            }
            Marshal.Copy( data, 0, pTo, data.Length );

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugGenericValue.SetValue( IntPtr pFrom )
        {
            object val = null;
            uint cByte = this.Size;            

            byte[] data = new byte[cByte];

            Marshal.Copy( pFrom, data, 0, (int)cByte );
            switch(this.ElementTypeProtected)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    val = BitConverter.ToBoolean( data, 0 );
                    break;

                case CorElementType.ELEMENT_TYPE_I1:
                    val = (sbyte)data[0];
                    break;

                case CorElementType.ELEMENT_TYPE_U1:
                    val = data[0];
                    break;

                case CorElementType.ELEMENT_TYPE_CHAR:
                    val = BitConverter.ToChar( data, 0 );
                    break;

                case CorElementType.ELEMENT_TYPE_I2:
                    val = BitConverter.ToInt16( data, 0 );
                    break;

                case CorElementType.ELEMENT_TYPE_U2:
                    val = BitConverter.ToUInt16( data, 0 );
                    break;

                case CorElementType.ELEMENT_TYPE_I4:
                    val = BitConverter.ToInt32( data, 0 );
                    break;

                case CorElementType.ELEMENT_TYPE_U4:
                    val = BitConverter.ToUInt32( data, 0 );
                    break;

                case CorElementType.ELEMENT_TYPE_R4:
                    val = BitConverter.ToSingle( data, 0 );
                    break;

                case CorElementType.ELEMENT_TYPE_I8:
                    val = BitConverter.ToInt64( data, 0 );
                    break;

                case CorElementType.ELEMENT_TYPE_U8:
                    val = BitConverter.ToUInt64( data, 0 );
                    break;

                case CorElementType.ELEMENT_TYPE_R8:
                    val = BitConverter.ToDouble( data, 0 );
                    break;
            }

            this.ValueProtected = val;

            return Utility.COM_HResults.S_OK;
        }

        #endregion
}
    
    public class CorDebugValueBoxedObject : CorDebugValue, ICorDebugBoxValue
    {
        CorDebugValueObject m_value;

        public CorDebugValueBoxedObject(RuntimeValue rtv, CorDebugAppDomain appDomain) : base (rtv, appDomain)
        {
            m_value = new CorDebugValueObject (rtv, appDomain);  
        }

        public override RuntimeValue RuntimeValue
        {
            set
            {
                m_value.RuntimeValue = value;
                base.RuntimeValue = value;
            }
        }

        public override CorElementType Type
        {
            get { return CorElementType.ELEMENT_TYPE_CLASS; }
        }

        #region ICorDebugBoxValue Members

        #region ICorDebugValue

        int ICorDebugBoxValue.GetType( out CorElementType pType )
        {
            return this.ICorDebugValue.GetType( out pType );            
        }

        int ICorDebugBoxValue.GetSize( out uint pSize )
        {
            return this.ICorDebugValue.GetSize( out pSize );            
        }

        int ICorDebugBoxValue.GetAddress( out ulong pAddress )
        {
            return this.ICorDebugValue.GetAddress( out pAddress );            
        }

        int ICorDebugBoxValue.CreateBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );            
        }

        #endregion

        #region ICorDebugHeapValue

        int ICorDebugBoxValue.IsValid( out int pbValid )
        {
            return this.ICorDebugHeapValue.IsValid( out pbValid );            
        }

        int ICorDebugBoxValue.CreateRelocBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );            
        }

        #endregion

        #region ICorDebugBoxValue

        int ICorDebugBoxValue.GetObject( out ICorDebugObjectValue ppObject )
        {
            ppObject = m_value;

            return Utility.COM_HResults.S_OK;            
        }

        #endregion

        #endregion
    }

    public class CorDebugValueReference : CorDebugValue, ICorDebugHandleValue, ICorDebugValue2
    {
        private CorDebugValue m_value;

        public CorDebugValueReference( CorDebugValue val, RuntimeValue rtv, CorDebugAppDomain appDomain )
            : base( rtv, appDomain )
        {
            m_value = val;
        }

        public override RuntimeValue RuntimeValue
        {
            set
            {
                m_value.RuntimeValue = value;
                base.RuntimeValue = value;
            }
        }

        public override CorElementType Type
        {
            get 
            {
                return m_value.Type;                
            }
        }

        public ICorDebugReferenceValue ICorDebugReferenceValue
        {
            get { return (ICorDebugReferenceValue)this; }
        }

        #region ICorDebugReferenceValue Members

        #region ICorDebugValue2 Members

        int ICorDebugValue2.GetExactType(out ICorDebugType ppType)
        {
            return ((ICorDebugValue2)m_value).GetExactType( out ppType);
        }

        #endregion

        #region ICorDebugValue

        int ICorDebugReferenceValue.GetType( out CorElementType pType )
        {
            return this.ICorDebugValue.GetType( out pType );            
        }

        int ICorDebugReferenceValue.GetSize( out uint pSize )
        {
            return this.ICorDebugValue.GetSize( out pSize );            
        }

        int ICorDebugReferenceValue.GetAddress( out ulong pAddress )
        {
            return this.ICorDebugValue.GetAddress( out pAddress );            
        }

        int ICorDebugReferenceValue.CreateBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );
        }

        #endregion

        #region ICorDebugReferenceValue

        int ICorDebugReferenceValue.IsNull( out int pbNull )
        {
            pbNull = Utility.Boolean.BoolToInt( m_rtv.IsNull );

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugReferenceValue.GetValue( out ulong pValue )
        {
            pValue = (ulong)m_rtv.ReferenceIdDirect;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugReferenceValue.SetValue( ulong value )
        {
            Debug.Assert( value <= uint.MaxValue );
            RuntimeValue rtvNew = m_rtv.Assign( (uint)value );

            this.RuntimeValue = rtvNew;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugReferenceValue.Dereference( out ICorDebugValue ppValue )
        {
            ppValue = m_value;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugReferenceValue.DereferenceStrong( out ICorDebugValue ppValue )
        {
            return this.ICorDebugReferenceValue.Dereference( out ppValue );            
        }

        #endregion

        #endregion

        #region ICorDebugHandleValue Members

        #region ICorDebugValue

        int ICorDebugHandleValue.GetType( out CorElementType pType )
        {
            return this.ICorDebugValue.GetType( out pType );            
        }

        int ICorDebugHandleValue.GetSize( out uint pSize )
        {
            return this.ICorDebugValue.GetSize( out pSize );            
        }

        int ICorDebugHandleValue.GetAddress( out ulong pAddress )
        {
            return this.ICorDebugValue.GetAddress( out pAddress );            
        }

        int ICorDebugHandleValue.CreateBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );            
        }

        #endregion

        #region ICorDebugReferenceValue

        int ICorDebugHandleValue.IsNull( out int pbNull )
        {
            return this.ICorDebugReferenceValue.IsNull( out pbNull );            
        }

        int ICorDebugHandleValue.GetValue( out ulong pValue )
        {
            return this.ICorDebugReferenceValue.GetValue( out pValue );            
        }

        int ICorDebugHandleValue.SetValue( ulong value )
        {
            return this.ICorDebugReferenceValue.SetValue( value );            
        }

        int ICorDebugHandleValue.Dereference( out ICorDebugValue ppValue )
        {
            return this.ICorDebugReferenceValue.Dereference( out ppValue );            
        }

        int ICorDebugHandleValue.DereferenceStrong( out ICorDebugValue ppValue )
        {
            return this.ICorDebugReferenceValue.DereferenceStrong( out ppValue );            
        }

        #endregion

        #region ICorDebugHandleValue

        int ICorDebugHandleValue.GetHandleType( out CorDebugHandleType pType )
        {
            pType = CorDebugHandleType.HANDLE_STRONG;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugHandleValue.Dispose()
        {
            return Utility.COM_HResults.S_OK;            
        }

        #endregion

        #endregion
    }
    
    public class CorDebugValueReferenceByRef : CorDebugValueReference
    {
        public CorDebugValueReferenceByRef(CorDebugValue val, RuntimeValue rtv, CorDebugAppDomain appDomain) : base(val, rtv, appDomain)
        {                    
        }

        public override CorElementType Type
        {
            get { return CorElementType.ELEMENT_TYPE_BYREF; }
        }
    }

    public class CorDebugValueArray : CorDebugValue, ICorDebugArrayValue, ICorDebugValue2
    {
        public CorDebugValueArray(RuntimeValue rtv, CorDebugAppDomain appDomain) : base(rtv, appDomain)
        {
        }

        public static CorElementType typeValue = CorElementType.ELEMENT_TYPE_I4;

        public uint Count
        {
            get { return m_rtv.Length; }
        }

        public ICorDebugArrayValue ICorDebugArrayValue
        {
            get { return (ICorDebugArrayValue)this; }
        }

        #region ICorDebugArrayValue Members

        #region ICorDebugValue

        int ICorDebugArrayValue.GetType( out CorElementType pType )
        {
            return this.ICorDebugValue.GetType( out pType );            
        }

        int ICorDebugArrayValue.GetSize( out uint pSize )
        {
            return this.ICorDebugValue.GetSize( out pSize );            
        }

        int ICorDebugArrayValue.GetAddress( out ulong pAddress )
        {
            return this.ICorDebugValue.GetAddress( out pAddress );            
        }

        int ICorDebugArrayValue.CreateBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );            
        }

        #endregion

        #region ICorDebugValue2 Members

        int ICorDebugValue2.GetExactType(out ICorDebugType ppType)

        {
            ppType = new CorDebugTypeArray( this );
            return Utility.COM_HResults.S_OK;
        }
        
        #endregion

        #region ICorDebugHeapValue

        int ICorDebugArrayValue.IsValid( out int pbValid )
        {
            return this.ICorDebugHeapValue.IsValid( out pbValid );            
        }

        int ICorDebugArrayValue.CreateRelocBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );            
        }

        #endregion

        #region ICorDebugArrayValue


        /* With implementation of ICorDebugValue2.GetExactType this function
         * ICorDebugArrayValue.GetElementType is not called.
         * It is left to support VS 2005.
         * We cannot remove this function since it is part of ICorDebugArrayValue interface.
         */
        
        int ICorDebugArrayValue.GetElementType( out CorElementType pType )
        {
            if (this.Count != 0)
            {
                pType = m_rtv.GetElement(0).CorElementType;
            }
            else
            {
                pType = CorElementType.ELEMENT_TYPE_CLASS;
            }
            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugArrayValue.GetRank( out uint pnRank )
        {
            pnRank = 1;

            return Utility.COM_HResults.S_OK;                 
        }

        int ICorDebugArrayValue.GetCount( out uint pnCount )
        {
            pnCount = this.Count;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugArrayValue.GetDimensions( uint cdim, uint[] dims )
        {
            Debug.Assert( cdim == 1 );
            
            dims[0] = this.Count;

            return Utility.COM_HResults.S_OK; 
        }

        int ICorDebugArrayValue.HasBaseIndicies( out int pbHasBaseIndicies )
        {
            pbHasBaseIndicies = Utility.Boolean.FALSE;

            return Utility.COM_HResults.S_OK;             
        }

        int ICorDebugArrayValue.GetBaseIndicies( uint cdim, uint[] indicies )
        {
            Debug.Assert( cdim == 1 );
            
            indicies[0] = 0;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugArrayValue.GetElement( uint cdim, uint[] indices, out ICorDebugValue ppValue )
        {
            //ask for several at once and cache?

            Debug.Assert( cdim == 1 );
            
            ppValue = CreateValue( m_rtv.GetElement( indices[0] ) );

            return Utility.COM_HResults.S_OK;   
        }

        int ICorDebugArrayValue.GetElementAtPosition( uint nPosition, out ICorDebugValue ppValue )
        {
            //Cache values?
            ppValue = CreateValue( m_rtv.GetElement( nPosition ) );

            return Utility.COM_HResults.S_OK;
        }

        #endregion

        #endregion
}

    public class CorDebugValueObject : CorDebugValue, ICorDebugObjectValue, ICorDebugGenericValue /*, ICorDebugObjectValue2*/
    {
        CorDebugClass      m_class = null;        
        CorDebugValuePrimitive      m_valuePrimitive = null;     //for boxed primitives, or enums
        bool               m_fIsEnum;
        bool               m_fIsBoxed;

        //Object or CLASS, or VALUETYPE
        public CorDebugValueObject(RuntimeValue rtv, CorDebugAppDomain appDomain) : base(rtv, appDomain)
        {
            if(!rtv.IsNull)
            {
                m_class = CorDebugValue.ClassFromRuntimeValue(rtv, appDomain);            
                m_fIsEnum = m_class.IsEnum;
                m_fIsBoxed = rtv.IsBoxed;                
            }
        }

        private bool IsValuePrimitive()
        {
            if (m_fIsBoxed || m_fIsEnum)
            {
                if (m_valuePrimitive == null)
                {
                    if (m_rtv.IsBoxed)
                    {
                        RuntimeValue rtv = m_rtv.GetField(1, 0);

                        Debug.Assert(rtv.IsPrimitive);

                        //Assert that m_class really points to a primitive
                        m_valuePrimitive = (CorDebugValuePrimitive)CreateValue(rtv);
                    }
                    else
                    {
                        Debug.Assert(m_fIsEnum);
                        m_valuePrimitive = new CorDebugValuePrimitive(m_rtv, m_appDomain);
                        Debug.Assert(m_rtv.IsPrimitive);
                    }
                }
            }

            return m_valuePrimitive != null;
        }
        
        public override uint Size
        {
            get 
            {
                if (this.IsValuePrimitive())
                {
                    return m_valuePrimitive.Size;
                }
                else
                {
                    return 4;
                }
            }
        }

        public override CorElementType Type
        {
            get 
            {
                if(m_fIsEnum)
                {
                    return CorElementType.ELEMENT_TYPE_VALUETYPE;
                }
                else
                {
                    return base.Type;                    
                }
            }
        }


        #region ICorDebugObjectValue Members

        #region ICorDebugValue

        int ICorDebugObjectValue.GetType( out CorElementType pType )
        {
            return this.ICorDebugValue.GetType( out pType );            
        }

        int ICorDebugObjectValue.GetSize( out uint pSize )
        {
            return this.ICorDebugValue.GetSize( out pSize );        
        }

        int ICorDebugObjectValue.GetAddress( out ulong pAddress )
        {
            return this.ICorDebugValue.GetAddress( out pAddress );            
        }

        int ICorDebugObjectValue.CreateBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );            
        }

        #endregion

        #region ICorDebugObjectValue

        int ICorDebugObjectValue.GetClass( out ICorDebugClass ppClass )
        {
            ppClass = m_class;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugObjectValue.GetFieldValue( ICorDebugClass pClass, uint fieldDef, out ICorDebugValue ppValue )
        {
            //cache fields?
            RuntimeValue rtv = m_rtv.GetField( 0, TinyCLR_TypeSystem.ClassMemberIndexFromCLRToken( fieldDef, ((CorDebugClass)pClass).Assembly ) );

            ppValue = CreateValue( rtv );

            return Utility.COM_HResults.S_OK;                 
        }

        int ICorDebugObjectValue.GetVirtualMethod( uint memberRef, out ICorDebugFunction ppFunction )
        {
            uint mdVirtual = Engine.GetVirtualMethod( TinyCLR_TypeSystem.ClassMemberIndexFromCLRToken( memberRef, this.m_class.Assembly ), this.m_rtv );

            ppFunction = TinyCLR_TypeSystem.CorDebugFunctionFromMethodIndex( mdVirtual, this.m_appDomain );

            return Utility.COM_HResults.S_OK;                             
        }

        int ICorDebugObjectValue.GetContext( out ICorDebugContext ppContext )
        {
            ppContext = null;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugObjectValue.IsValueClass( out int pbIsValueClass )
        {
            pbIsValueClass = Utility.Boolean.BoolToInt( m_rtv.IsValueType );

            return Utility.COM_HResults.S_OK;                      
        }

        int ICorDebugObjectValue.GetManagedCopy( out object ppObject )
        {
            ppObject = null;

            Debug.Assert( false );

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugObjectValue.SetFromManagedCopy( object pObject )
        {
            return Utility.COM_HResults.S_OK;              
        }

        #endregion

        #endregion

        #region ICorDebugGenericValue Members

        #region ICorDebugValue

        int ICorDebugGenericValue.GetType( out CorElementType pType )
        {
            return this.ICorDebugValue.GetType( out pType );             
        }

        int ICorDebugGenericValue.GetSize( out uint pSize )
        {
            return this.ICorDebugValue.GetSize( out pSize );            
        }

        int ICorDebugGenericValue.GetAddress( out ulong pAddress )
        {
            return this.ICorDebugValue.GetAddress( out pAddress );            
        }

        int ICorDebugGenericValue.CreateBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );            
        }

        #endregion

        #region ICorDebugGenericValue

        int ICorDebugGenericValue.GetValue( IntPtr pTo )
        {
            int hr = Utility.COM_HResults.S_OK;

            if (this.IsValuePrimitive())
            {
                hr = m_valuePrimitive.ICorDebugGenericValue.GetValue(pTo);
            }
            else
            {
                ulong addr;

                hr = ((ICorDebugGenericValue)this).GetAddress(out addr);

                Marshal.WriteInt32(pTo, (int)addr);
            }

            return hr;            
        }

        int ICorDebugGenericValue.SetValue( IntPtr pFrom )
        {
            int hr = Utility.COM_HResults.S_OK;

            if (this.IsValuePrimitive())
            {
                hr = m_valuePrimitive.ICorDebugGenericValue.SetValue(pFrom);
            }
            else
            {
                Debug.Assert(false);
            }

            return hr;
        }

        #endregion

        #endregion
    }

    public class CorDebugValueString : CorDebugValue, ICorDebugStringValue
    {
        public CorDebugValueString( RuntimeValue rtv, CorDebugAppDomain appDomain )
            : base( rtv, appDomain )
        {
        }

        private string Value
        {
            get
            {
                string ret = m_rtv.Value as string;
                return (ret == null) ? "" : ret;
            }
        }


        #region ICorDebugStringValue Members

        #region ICorDebugValue

        int ICorDebugStringValue.GetType( out CorElementType pType )
        {
            return this.ICorDebugValue.GetType( out pType );
        }

        int ICorDebugStringValue.GetSize( out uint pSize )
        {
            return this.ICorDebugValue.GetSize( out pSize );   
        }

        int ICorDebugStringValue.GetAddress( out ulong pAddress )
        {
            return this.ICorDebugValue.GetAddress( out pAddress );
        }

        int ICorDebugStringValue.CreateBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugValue.CreateBreakpoint( out ppBreakpoint );
        }

        #endregion

        #region ICorDebugHeapValue

        int ICorDebugStringValue.IsValid( out int pbValid )
        {
            return this.ICorDebugHeapValue.IsValid( out pbValid );
        }

        int ICorDebugStringValue.CreateRelocBreakpoint( out ICorDebugValueBreakpoint ppBreakpoint )
        {
            return this.ICorDebugHeapValue.CreateRelocBreakpoint( out ppBreakpoint );
        }

        #endregion

        #region ICorDebugStringValue

        int ICorDebugStringValue.GetLength( out uint pcchString )
        {
            pcchString = (uint)Value.Length;

            return Utility.COM_HResults.S_OK;            
        }

        int ICorDebugStringValue.GetString( uint cchString, IntPtr pcchString, IntPtr szString )
        {
            Utility.MarshalString( Value, cchString, pcchString, szString, false );

            return Utility.COM_HResults.S_OK;            
        }

        #endregion

        #endregion

    }
}
