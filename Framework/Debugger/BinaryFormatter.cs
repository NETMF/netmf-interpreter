//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.SPOT.Debugger;

namespace Microsoft.SPOT.Debugger
{
    [Flags()]
    public enum SerializationFlags
    {
        //
        // Keep in sync with Microsoft.SPOT.SerializationHints!!!!
        //

        Encrypted = 0x00000001,
        Compressed = 0x00000002, // Value uses range compression (max 2^30 values).
        Optional = 0x00000004, // If the value cannot be deserialized, skip it.
        PointerNeverNull = 0x00000010,
        ElementsNeverNull = 0x00000020,
        FixedType = 0x00000100,
        DemandTrusted = 0x00010000,
    }

    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Class, Inherited = true )]
    public class SerializationHintsAttribute : Attribute
    {
        //
        // Keep in sync with Microsoft.SPOT.SerializationHintsAttribute!!!!
        //

        public SerializationFlags Flags;
        public int ArraySize; // -1 == extend to the end of the stream.
        public int BitPacked;     // In bits.
        public long RangeBias;
        public ulong Scale;         // For time, it's in ticks.
    }

    internal enum ElementType : byte // KEEP IN SYNC WITH CLR_CorElementType!!
    {
        //
        // This is based on CorElementType, but adds a few types for support of boxing/unboxing.
        //
        PELEMENT_TYPE_END = 0x0,
        PELEMENT_TYPE_VOID = 0x1,
        PELEMENT_TYPE_BOOLEAN = 0x2,
        PELEMENT_TYPE_CHAR = 0x3,
        PELEMENT_TYPE_I1 = 0x4,
        PELEMENT_TYPE_U1 = 0x5,
        PELEMENT_TYPE_I2 = 0x6,
        PELEMENT_TYPE_U2 = 0x7,
        PELEMENT_TYPE_I4 = 0x8,
        PELEMENT_TYPE_U4 = 0x9,
        PELEMENT_TYPE_I8 = 0xa,
        PELEMENT_TYPE_U8 = 0xb,
        PELEMENT_TYPE_R4 = 0xc,
        PELEMENT_TYPE_R8 = 0xd,
        PELEMENT_TYPE_STRING = 0xe,

        // every type above PTR will be simple type
        PELEMENT_TYPE_PTR = 0xf,      // PTR <type>
        PELEMENT_TYPE_BYREF = 0x10,     // BYREF <type>

        // Please use ELEMENT_TYPE_VALUETYPE. ELEMENT_TYPE_VALUECLASS is deprecated.
        PELEMENT_TYPE_VALUETYPE = 0x11,     // VALUETYPE <class Token>
        PELEMENT_TYPE_CLASS = 0x12,     // CLASS <class Token>

        PELEMENT_TYPE_ARRAY = 0x14,     // MDARRAY <type> <rank> <bcount> <bound1> ... <lbcount> <lb1> ...

        PELEMENT_TYPE_TYPEDBYREF = 0x16,     // This is a simple type.

        PELEMENT_TYPE_I = 0x18,     // native integer size
        PELEMENT_TYPE_U = 0x19,     // native unsigned integer size
        PELEMENT_TYPE_FNPTR = 0x1B,     // FNPTR <complete sig for the function including calling convention>
        PELEMENT_TYPE_OBJECT = 0x1C,     // Shortcut for System.Object
        PELEMENT_TYPE_SZARRAY = 0x1D,     // Shortcut for single dimension zero lower bound array
        // SZARRAY <type>

        // This is only for binding
        PELEMENT_TYPE_CMOD_REQD = 0x1F,     // required C modifier : E_T_CMOD_REQD <mdTypeRef/mdTypeDef>
        PELEMENT_TYPE_CMOD_OPT = 0x20,     // optional C modifier : E_T_CMOD_OPT <mdTypeRef/mdTypeDef>

        // This is for signatures generated internally (which will not be persisted in any way).
        PELEMENT_TYPE_INTERNAL = 0x21,     // INTERNAL <typehandle>

        // Note that this is the max of base type excluding modifiers
        PELEMENT_TYPE_MAX = 0x22,     // first invalid element type
        //
        // End of overlap with CorElementType.
        //
    };

    public class BinaryFormatter
    {
        internal const int TE_L1 = 2;
        internal const uint TE_L1_Null = 0x00000000;
        internal const uint TE_L1_Duplicate = 0x00000001; // N bits for the duplicate id.
        internal const uint TE_L1_Reference = 0x00000002; // 32 bits for the type.
        internal const uint TE_L1_Other = 0x00000003;

        internal const int TE_L2 = 2;
        internal const uint TE_L2_Primitive = 0x00000000; // 4 bits for the type.
        internal const uint TE_L2_Array = 0x00000001;
        internal const uint TE_L2_ArrayList = 0x00000002;
        internal const uint TE_L2_Other = 0x00000003;

        internal const int TE_L3 = 4;
        internal const uint TE_L3_Type = 0x00000000; // 32 bits for the type.
        internal const uint TE_L3_Method = 0x00000001; // 32 bits for the type, N bits for the signature??
        internal const uint TE_L3_Field = 0x00000002; // 32 bits for the type, N bits for the signature??

        internal const int TE_ElementType = 4;
        internal const int TE_ArrayDepth = 4;

        internal const uint INVALID_HASH_ENTRY = 0xffffffff;

        struct TypeDescriptorBasic
        {
            internal const int c_Primitive = 0x00000001;
            internal const int c_Interface = 0x00000002;
            internal const int c_Class = 0x00000004;
            internal const int c_ValueType = 0x00000008;
            internal const int c_Enum = 0x00000010;
            internal const int c_SemanticMask = 0x0000001F;

            internal const int c_Array = 0x00010000;
            internal const int c_ArrayList = 0x00020000;

            internal Type m_referenceType;
            internal ElementType m_et;
            internal int m_flags;

            internal void SetType( Type t )
            {
                m_referenceType = t;
                m_et = ElementType.PELEMENT_TYPE_VOID;
                m_flags = 0;

                //
                // Not supported for now.
                //
                if(t == typeof( System.RuntimeFieldHandle )) goto Failure;
                if(t == typeof( System.RuntimeMethodHandle )) goto Failure;

                if(t == typeof( System.Boolean )) m_et = ElementType.PELEMENT_TYPE_BOOLEAN;
                else if(t == typeof( System.Char )) m_et = ElementType.PELEMENT_TYPE_CHAR;
                else if(t == typeof( System.SByte )) m_et = ElementType.PELEMENT_TYPE_I1;
                else if(t == typeof( System.Byte )) m_et = ElementType.PELEMENT_TYPE_U1;
                else if(t == typeof( System.Int16 )) m_et = ElementType.PELEMENT_TYPE_I2;
                else if(t == typeof( System.UInt16 )) m_et = ElementType.PELEMENT_TYPE_U2;
                else if(t == typeof( System.Int32 )) m_et = ElementType.PELEMENT_TYPE_I4;
                else if(t == typeof( System.UInt32 )) m_et = ElementType.PELEMENT_TYPE_U4;
                else if(t == typeof( System.Int64 )) m_et = ElementType.PELEMENT_TYPE_I8;
                else if(t == typeof( System.UInt64 )) m_et = ElementType.PELEMENT_TYPE_U8;
                else if(t == typeof( System.Single )) m_et = ElementType.PELEMENT_TYPE_R4;
                else if(t == typeof( System.Double )) m_et = ElementType.PELEMENT_TYPE_R8;
                else if(t == typeof( System.String )) m_et = ElementType.PELEMENT_TYPE_STRING;

                if(m_et == ElementType.PELEMENT_TYPE_VOID)
                {
                    if(t == typeof( System.Object )) { m_et = ElementType.PELEMENT_TYPE_OBJECT; }
                    else if(t.IsEnum) { m_et = ElementType.PELEMENT_TYPE_VALUETYPE; m_flags |= c_Enum; }
                    else if(t.IsValueType) { m_et = ElementType.PELEMENT_TYPE_VALUETYPE; m_flags |= c_ValueType; }
                    else if(t.IsInterface) { m_et = ElementType.PELEMENT_TYPE_CLASS; m_flags |= c_Interface; }
                    else if(t.IsClass) { m_et = ElementType.PELEMENT_TYPE_CLASS; m_flags |= c_Class; }
                    else
                    {
                        goto Failure;
                    }

                    if(t.IsArray)
                    {
                        m_flags |= c_Array;
                    }

                    if(t == typeof( System.Collections.ArrayList ))
                    {
                        m_flags |= c_ArrayList;
                    }
                }
                else
                {
                    m_flags |= c_Primitive;
                }

                return;


            Failure:
                throw Error( String.Format( "Unsupported type: {0}", t.FullName ) );
            }

            internal void SetType( ElementType et )
            {
                m_et = et;
                m_flags = TypeDescriptorBasic.c_Primitive;

                switch(m_et)
                {
                    case ElementType.PELEMENT_TYPE_BOOLEAN: m_referenceType = typeof( System.Boolean ); break;
                    case ElementType.PELEMENT_TYPE_CHAR: m_referenceType = typeof( System.Char ); break;
                    case ElementType.PELEMENT_TYPE_I1: m_referenceType = typeof( System.SByte ); break;
                    case ElementType.PELEMENT_TYPE_U1: m_referenceType = typeof( System.Byte ); break;
                    case ElementType.PELEMENT_TYPE_I2: m_referenceType = typeof( System.Int16 ); break;
                    case ElementType.PELEMENT_TYPE_U2: m_referenceType = typeof( System.UInt16 ); break;
                    case ElementType.PELEMENT_TYPE_I4: m_referenceType = typeof( System.Int32 ); break;
                    case ElementType.PELEMENT_TYPE_U4: m_referenceType = typeof( System.UInt32 ); break;
                    case ElementType.PELEMENT_TYPE_I8: m_referenceType = typeof( System.Int64 ); break;
                    case ElementType.PELEMENT_TYPE_U8: m_referenceType = typeof( System.UInt64 ); break;
                    case ElementType.PELEMENT_TYPE_R4: m_referenceType = typeof( System.Single ); break;
                    case ElementType.PELEMENT_TYPE_R8: m_referenceType = typeof( System.Double ); break;
                    case ElementType.PELEMENT_TYPE_STRING: m_referenceType = typeof( System.String ); break;

                    default: throw Error( String.Format( "Invalid primitive type: {0}", m_et ) );
                }
            }

            static internal int NumberOfBits( ElementType et )
            {
                switch(et)
                {
                    case ElementType.PELEMENT_TYPE_BOOLEAN: return 1;
                    case ElementType.PELEMENT_TYPE_CHAR: return 16;
                    case ElementType.PELEMENT_TYPE_I1: return 8;
                    case ElementType.PELEMENT_TYPE_U1: return 8;
                    case ElementType.PELEMENT_TYPE_I2: return 16;
                    case ElementType.PELEMENT_TYPE_U2: return 16;
                    case ElementType.PELEMENT_TYPE_I4: return 32;
                    case ElementType.PELEMENT_TYPE_U4: return 32;
                    case ElementType.PELEMENT_TYPE_I8: return 64;
                    case ElementType.PELEMENT_TYPE_U8: return 64;
                    case ElementType.PELEMENT_TYPE_R4: return 32;
                    case ElementType.PELEMENT_TYPE_R8: return 64;
                    case ElementType.PELEMENT_TYPE_STRING: return -1;
                    default: return -2;
                }
            }

            static internal bool IsSigned( ElementType et )
            {
                switch(et)
                {
                    case ElementType.PELEMENT_TYPE_BOOLEAN: return false;
                    case ElementType.PELEMENT_TYPE_CHAR: return false;
                    case ElementType.PELEMENT_TYPE_I1: return true;
                    case ElementType.PELEMENT_TYPE_U1: return false;
                    case ElementType.PELEMENT_TYPE_I2: return true;
                    case ElementType.PELEMENT_TYPE_U2: return false;
                    case ElementType.PELEMENT_TYPE_I4: return true;
                    case ElementType.PELEMENT_TYPE_U4: return false;
                    case ElementType.PELEMENT_TYPE_I8: return true;
                    case ElementType.PELEMENT_TYPE_U8: return false;
                    case ElementType.PELEMENT_TYPE_R4: return true;
                    case ElementType.PELEMENT_TYPE_R8: return true;
                    default: return false;
                }
            }

            internal Type Type { get { return m_referenceType; } }

            internal bool IsPrimitive { get { return (m_flags & TypeDescriptorBasic.c_Primitive) != 0; } }
            internal bool IsInterface { get { return (m_flags & TypeDescriptorBasic.c_Interface) != 0; } }
            internal bool IsClass { get { return (m_flags & TypeDescriptorBasic.c_Class) != 0; } }
            internal bool IsValueType { get { return (m_flags & TypeDescriptorBasic.c_ValueType) != 0; } }
            internal bool IsEnum { get { return (m_flags & TypeDescriptorBasic.c_Enum) != 0; } }

            internal bool NeedsSignature
            {
                get
                {
                    return IsPrimitive == false && IsEnum == false && IsValueType == false;
                }
            }

            internal bool IsArray { get { return (m_flags & TypeDescriptorBasic.c_Array) != 0; } }
            internal bool IsArrayList { get { return (m_flags & TypeDescriptorBasic.c_ArrayList) != 0; } }

            internal bool IsSealed { get { return m_referenceType != null && m_referenceType.IsSealed; } }

            static internal Exception Error( string msg )
            {
                return new System.Runtime.Serialization.SerializationException( msg );
            }
        }

        class TypeDescriptor
        {
            internal TypeDescriptorBasic m_base;
            internal int m_arrayDepth = 0;
            internal TypeDescriptorBasic m_arrayElement = new TypeDescriptorBasic();

            internal TypeDescriptor( Type t )
            {
                if(t == null)
                {
                    throw new ArgumentException( "Unknown type" );
                }

                m_base.SetType( t );

                if(m_base.IsArray)
                {
                    Type sub = t;

                    while(sub.IsArray)
                    {
                        m_arrayDepth++;

                        sub = sub.GetElementType();

                        if(sub == null)
                        {
                            throw new ApplicationException( "cannot load type" );
                        }
                    }

                    m_arrayElement.SetType( sub );
                    return;
                }
            }

            internal TypeDescriptor( ElementType et )
            {
                m_base.SetType( et );
            }

            internal TypeDescriptor( ElementType et, int depth )
            {
                m_base.m_et = et;
                m_base.m_flags = TypeDescriptorBasic.c_Array;
                m_arrayDepth = depth;
            }

            internal Type Type
            {
                get
                {
                    if(m_base.IsArray)
                    {
                        Type res = m_arrayElement.Type;
                        int depth = m_arrayDepth;

                        while(depth-- > 0)
                        {
                            res = Array.CreateInstance( res, 1 ).GetType();
                        }

                        return res;
                    }

                    return m_base.Type;
                }
            }
        }

        class TypeHandler
        {
            //
            // Type of signatures:
            //
            // 1) NULL
            //
            //      Invalid for NeverNull
            //
            // 2) DUPLICATE <num>
            //
            //      Invalid for Sealed/FixedType + NeverNull
            //
            // 3) PRIMITIVE <et>
            //
            //      <et>      optional for FixedType
            //      PRIMITIVE optional for FixedType + NeverNull
            //
            // 4) REFERENCE <type>
            //
            //      <type>    optional for Sealed/FixedType
            //      REFERENCE optional for Sealed/FixedType + NeverNull
            //
            // 5) ARRAYLIST <length>
            //
            //      <length>  optional for FixedSize
            //      ARRAYLIST optional for FixedType + NeverNull
            //
            // 6) ARRAY <depth> <baseType> <length>
            //
            //      <length>           optional for FixedSize
            //      <depth> <baseType> optional for FixedType
            //      ARRAY              optional for FixedType + NeverNull
            //
            // Always match type if FixedTyped is specified.
            //
            internal const int c_Signature_Header = 0x01;
            internal const int c_Signature_Type = 0x02;
            internal const int c_Signature_Length = 0x04;

            internal const int c_Action_None = 0;
            internal const int c_Action_ObjectData = 1;
            internal const int c_Action_ObjectFields = 2;
            internal const int c_Action_ObjectElements = 3;

            BinaryFormatter m_bf;

            internal object m_value;
            internal TypeDescriptor m_type;
            internal SerializationHintsAttribute m_hints;

            internal TypeDescriptor m_typeExpected;
            internal Type m_typeForced;

            internal TypeHandler( BinaryFormatter bf, SerializationHintsAttribute hints, TypeDescriptor expected )
            {
                m_bf = bf;

                m_value = null;
                m_type = null;
                m_hints = hints;

                m_typeExpected = expected;
                m_typeForced = null;
            }

            internal void SetValue( object v )
            {
                if(v != null)
                {
                    Type t = v.GetType();

                    if(t.IsEnum || t.IsPrimitive || (t.Attributes & TypeAttributes.Serializable) != 0)
                    {
                        m_value = v;
                        m_type = new TypeDescriptor( v.GetType() );
                    }
                }
            }

            internal int SignatureRequirements()
            {
                int res = c_Signature_Header | c_Signature_Type | c_Signature_Length;

                if(this.Hints_ArraySize != 0)
                {
                    res &= ~c_Signature_Length;
                }

                m_typeForced = null;

                if(m_typeExpected != null)
                {
                    if(m_typeExpected.m_base.NeedsSignature == false)
                    {
                        res = 0;
                    }
                    else
                    {
                        if(this.Hints_FixedType)
                        {
                            res &= ~c_Signature_Type;
                        }
                        else
                        {
                            TypeDescriptorBasic td;

                            if(m_typeExpected.m_base.IsArray)
                            {
                                td = m_typeExpected.m_arrayElement;
                            }
                            else
                            {
                                td = m_typeExpected.m_base;
                            }

                            if(td.IsSealed)
                            {
                                res &= ~c_Signature_Type;
                            }
                        }
                    }
                }

                if((res & c_Signature_Type) == 0)
                {
                    m_typeForced = m_typeExpected.Type;

                    if(this.Hints_PointerNeverNull)
                    {
                        res &= ~c_Signature_Header;
                    }
                }

                return res;
            }

            internal bool Hints_Encrypted { get { return (m_hints != null && (m_hints.Flags & SerializationFlags.Encrypted) != 0); } }
            internal bool Hints_Compressed { get { return (m_hints != null && (m_hints.Flags & SerializationFlags.Compressed) != 0); } }
            internal bool Hints_Optional { get { return (m_hints != null && (m_hints.Flags & SerializationFlags.Optional) != 0); } }

            internal bool Hints_PointerNeverNull { get { return (m_hints != null && (m_hints.Flags & SerializationFlags.PointerNeverNull) != 0); } }
            internal bool Hints_ElementsNeverNull { get { return (m_hints != null && (m_hints.Flags & SerializationFlags.ElementsNeverNull) != 0); } }

            internal bool Hints_FixedType { get { return (m_hints != null && (m_hints.Flags & SerializationFlags.FixedType) != 0); } }

            internal int Hints_ArraySize { get { return m_hints == null ? 0 : m_hints.ArraySize; } }

            internal int Hints_BitPacked { get { return m_hints == null ? 0 : m_hints.BitPacked; } }
            internal long Hints_RangeBias { get { return m_hints == null ? 0 : m_hints.RangeBias; } }
            internal ulong Hints_Scale { get { return m_hints == null ? 0 : m_hints.Scale; } }

            internal int EmitSignature()
            {
                int mask = SignatureRequirements();

                if((mask & c_Signature_Type) == 0)
                {
                    if(m_value != null && m_value.GetType() != m_typeForced)
                    {
                        if(m_typeForced == typeof( Type ) && m_value.GetType().IsSubclassOf( m_typeForced ))
                        {
                        }
                        else
                        {
                            throw TypeDescriptorBasic.Error( "FixedType violation" );
                        }
                    }
                }

                if(m_value == null)
                {
                    if(mask == 0)
                    {
                        //
                        // Special case for null strings (strings don't emit an hash): send a string of length -1.
                        //
                        if(m_typeExpected != null && m_typeExpected.m_base.m_et == ElementType.PELEMENT_TYPE_STRING)
                        {
                            BinaryFormatter.WriteLine( "NULL STRING" );

                            m_bf.WriteCompressedUnsigned( 0xFFFFFFFF );
                            return c_Action_None;
                        }

                        throw TypeDescriptorBasic.Error( "NoSignature violation" );
                    }

                    if(this.Hints_PointerNeverNull)
                    {
                        throw TypeDescriptorBasic.Error( "PointerNeverNull violation" );
                    }

                    BinaryFormatter.WriteLine( "NULL Pointer" );

                    m_bf.WriteBits( TE_L1_Null, TE_L1 );
                    return c_Action_None;
                }

                int idx = m_bf.SearchDuplicate( m_value );
                if(idx != -1)
                {
                    //
                    // No duplicates allowed for fixed-type objects.
                    //
                    if((mask & c_Signature_Header) == 0)
                    {
                        throw TypeDescriptorBasic.Error( "No duplicates for FixedType+PointerNeverNull!" );
                    }

                    BinaryFormatter.WriteLine( "Duplicate: {0}", idx );

                    m_bf.WriteBits( TE_L1_Duplicate, TE_L1 );
                    m_bf.WriteCompressedUnsigned( (uint)idx );
                    return c_Action_None;
                }

                EmitSignature_Inner( mask, m_type.m_base, m_type, m_value );

                return c_Action_ObjectData;
            }

            private void EmitSignature_Inner( int mask, TypeDescriptorBasic td, TypeDescriptor tdBase, object v )
            {
                int sizeReal = -1;

                if(td.Type.IsSubclassOf( typeof( Type ) ))
                {
                    Type t = (Type)v;

                    BinaryFormatter.WriteLine( "Reference: {0} {1}", mask, t.FullName );

                    if((mask & c_Signature_Header) != 0)
                    {
                        m_bf.WriteBits( TE_L1_Other, TE_L1 );
                        m_bf.WriteBits( TE_L2_Other, TE_L2 );
                        m_bf.WriteBits( TE_L3_Type, TE_L3 );
                    }
                }
                else if(td.IsPrimitive)
                {
                    BinaryFormatter.WriteLine( "Primitive: {0} {1}", mask, td.m_et );

                    if((mask & c_Signature_Header) != 0)
                    {
                        m_bf.WriteBits( TE_L1_Other, TE_L1 );
                        m_bf.WriteBits( TE_L2_Primitive, TE_L2 );
                    }

                    if((mask & c_Signature_Type) != 0)
                    {
                        m_bf.WriteBits( (uint)td.m_et, TE_ElementType );
                    }
                }
                else if(td.IsArray)
                {
                    Array arr = (Array)v;

                    BinaryFormatter.WriteLine( "Array: Depth {0} {1}", mask, tdBase.m_arrayDepth );

                    if((mask & c_Signature_Header) != 0)
                    {
                        m_bf.WriteBits( TE_L1_Other, TE_L1 );
                        m_bf.WriteBits( TE_L2_Array, TE_L2 );
                    }

                    if((mask & c_Signature_Type) != 0)
                    {
                        m_bf.WriteBits( (uint)tdBase.m_arrayDepth, TE_ArrayDepth );

                        EmitSignature_Inner( c_Signature_Header | c_Signature_Type, tdBase.m_arrayElement, null, null );
                    }

                    BinaryFormatter.WriteLine( "Array: Size {0}", arr.Length );

                    sizeReal = arr.Length;
                }
                else if(td.IsArrayList && v != null)
                {
                    ArrayList lst = (ArrayList)v;

                    BinaryFormatter.WriteLine( "ArrayList: Size {0}", lst.Count );

                    if((mask & c_Signature_Header) != 0)
                    {
                        m_bf.WriteBits( TE_L1_Other, TE_L1 );
                        m_bf.WriteBits( TE_L2_ArrayList, TE_L2 );
                    }

                    sizeReal = lst.Count;
                }
                else
                {
                    Type t;

                    if(v != null) t = v.GetType();
                    else t = td.Type;

                    BinaryFormatter.WriteLine( "Reference: {0} {1}", mask, t.FullName );

                    if((mask & c_Signature_Header) != 0)
                    {
                        m_bf.WriteBits( TE_L1_Reference, TE_L1 );
                    }

                    if((mask & c_Signature_Type) != 0)
                    {
                        m_bf.WriteType( t );
                    }
                }

                if(sizeReal != -1)
                {
                    if((mask & c_Signature_Length) != 0)
                    {
                        int bitsMax = this.Hints_BitPacked;

                        if(bitsMax != 0)
                        {
                            if(sizeReal >= (1 << bitsMax))
                            {
                                throw TypeDescriptorBasic.Error( String.Format( "Array size outside range: Bits={0}", bitsMax ) );
                            }

                            m_bf.WriteBits( (uint)sizeReal, bitsMax );
                        }
                        else
                        {
                            m_bf.WriteCompressedUnsigned( (uint)sizeReal );
                        }
                    }
                    else
                    {
                        int sizeExpected = this.Hints_ArraySize;

                        if(sizeExpected > 0 && sizeExpected != sizeReal)
                        {
                            throw TypeDescriptorBasic.Error( String.Format( "ArraySize violation: (Expected: {0} Got:{1})", sizeReal, sizeExpected ) );
                        }
                    }
                }
            }

            internal int ReadSignature()
            {
                int mask = SignatureRequirements();

                m_value = null;
                m_type = (m_typeForced == null) ? null : new TypeDescriptor( m_typeForced );

                if((mask & c_Signature_Header) != 0)
                {
                    uint levelOne;
                    uint levelTwo;
                    uint levelThree;


                    levelOne = m_bf.ReadBits( TE_L1 );
                    if(levelOne == TE_L1_Null)
                    {
                        if(this.Hints_PointerNeverNull)
                        {
                            throw TypeDescriptorBasic.Error( "PointerNeverNull violation" );
                        }

                        BinaryFormatter.WriteLine( "NULL Pointer" );

                        return c_Action_None;
                    }
                    else if(levelOne == TE_L1_Duplicate)
                    {
                        int idx = (int)m_bf.ReadCompressedUnsigned();

                        m_value = m_bf.GetDuplicate( idx );
                        m_type = new TypeDescriptor( m_value.GetType() );

                        BinaryFormatter.WriteLine( "Duplicate: {0}", idx );

                        return c_Action_None;
                    }
                    else if(levelOne == TE_L1_Reference)
                    {
                        if((mask & c_Signature_Type) != 0)
                        {
                            m_type = new TypeDescriptor( m_bf.ReadType() );
                        }

                        BinaryFormatter.WriteLine( "Reference: {0}", m_type.Type.FullName );
                    }
                    else
                    {
                        levelTwo = m_bf.ReadBits( TE_L2 );
                        if(levelTwo == TE_L2_Primitive)
                        {
                            if((mask & c_Signature_Type) != 0)
                            {
                                m_type = new TypeDescriptor( (ElementType)m_bf.ReadBits( TE_ElementType ) );
                            }

                            BinaryFormatter.WriteLine( "Primitive: {0}", m_type.m_base.m_et );
                        }
                        else if(levelTwo == TE_L2_Array)
                        {
                            if((mask & c_Signature_Type) != 0)
                            {
                                m_type = new TypeDescriptor( ElementType.PELEMENT_TYPE_CLASS, (int)m_bf.ReadBits( TE_ArrayDepth ) );

                                BinaryFormatter.WriteLine( "Array: Depth {0}", m_type.m_arrayDepth );

                                levelOne = m_bf.ReadBits( TE_L1 );
                                if(levelOne == TE_L1_Reference)
                                {
                                    m_type.m_arrayElement.SetType( m_bf.ReadType() );
                                }
                                else if(levelOne == TE_L1_Other)
                                {
                                    levelTwo = m_bf.ReadBits( TE_L2 );

                                    if(levelTwo == TE_L2_Primitive)
                                    {
                                        m_type.m_arrayElement.SetType( (ElementType)m_bf.ReadBits( TE_ElementType ) );
                                    }
                                    else
                                    {
                                        throw TypeDescriptorBasic.Error( String.Format( "Unexpected Level2 value: {0}", levelTwo ) );
                                    }
                                }
                                else
                                {
                                    throw TypeDescriptorBasic.Error( String.Format( "Unexpected Level1 value: {0}", levelOne ) );
                                }
                            }
                        }
                        else if(levelTwo == TE_L2_ArrayList)
                        {
                            if((mask & c_Signature_Type) != 0)
                            {
                                m_type = new TypeDescriptor( typeof( ArrayList ) );
                            }
                        }
                        else if(levelTwo == TE_L2_Other)
                        {
                            levelThree = m_bf.ReadBits( TE_L3 );
                            if(levelThree == TE_L3_Type)
                            {
                                m_type = new TypeDescriptor( typeof( Type ) );
                            }
                            else
                            {
                                throw TypeDescriptorBasic.Error( String.Format( "Unexpected Level3 value: {0}", levelThree ) );
                            }
                        }
                    }
                }

                if(m_type.m_base.IsArray || m_type.m_base.IsArrayList)
                {
                    int len;

                    if((mask & c_Signature_Length) != 0)
                    {
                        int bitsMax = this.Hints_BitPacked;

                        if(bitsMax != 0)
                        {
                            len = (int)m_bf.ReadBits( bitsMax );
                        }
                        else
                        {
                            len = (int)m_bf.ReadCompressedUnsigned();
                        }
                    }
                    else
                    {
                        len = this.Hints_ArraySize;

                        if(len == -1)
                        {
                            int bits = TypeDescriptorBasic.NumberOfBits( m_type.m_arrayElement.m_et );

                            if(bits < 0)
                            {
                                throw TypeDescriptorBasic.Error( "Only primitive types allowed for ArraySize = -1" );
                            }

                            len = m_bf.BitsAvailable() / bits;
                        }
                    }

                    if(m_type.m_base.IsArrayList)
                    {
                        m_value = new ArrayList( len );
                    }
                    else
                    {
                        m_value = Array.CreateInstance( m_type.Type.GetElementType(), len );

                        BinaryFormatter.WriteLine( "Array: Size {0}", ((Array)m_value).Length );
                    }
                }
                else
                {
                    if(m_type.Type == typeof( Type ))
                    {
                        m_value = m_bf.ReadType();

                        return c_Action_None;
                    }

                    if(m_type.Type == typeof( string ))
                    {
                        m_value = null;
                    }
                    else
                    {
                        m_value = System.Runtime.Serialization.FormatterServices.GetUninitializedObject( m_type.Type );
                    }
                }

                return c_Action_ObjectData;
            }

            private int TrackObject()
            {
                if(m_type.m_base.IsArray || m_type.m_base.IsArrayList)
                {
                    m_bf.TrackDuplicate( m_value );

                    return c_Action_ObjectElements;
                }
                else
                {
                    if(m_typeExpected == null || m_typeExpected.m_base.NeedsSignature)
                    {
                        m_bf.TrackDuplicate( m_value );
                    }

                    return c_Action_ObjectFields;
                }
            }

            internal int EmitValue()
            {
                if(m_type.Type.IsSubclassOf( typeof( Type ) ))
                {
                    m_bf.WriteType( (Type)m_value );

                    return c_Action_None;
                }

                ulong val;
                int bits;
                bool fSigned;

                if(m_type.m_base.IsPrimitive)
                {
                    switch(m_type.m_base.m_et)
                    {
                        case ElementType.PELEMENT_TYPE_BOOLEAN: val = (bool)m_value ? 1UL : 0UL; break;
                        case ElementType.PELEMENT_TYPE_CHAR: val = (ulong)(char)m_value; break;
                        case ElementType.PELEMENT_TYPE_I1: val = (ulong)(sbyte)m_value; break;
                        case ElementType.PELEMENT_TYPE_U1: val = (ulong)(byte)m_value; break;
                        case ElementType.PELEMENT_TYPE_I2: val = (ulong)(short)m_value; break;
                        case ElementType.PELEMENT_TYPE_U2: val = (ulong)(ushort)m_value; break;
                        case ElementType.PELEMENT_TYPE_I4: val = (ulong)(int)m_value; break;
                        case ElementType.PELEMENT_TYPE_U4: val = (ulong)(uint)m_value; break;
                        case ElementType.PELEMENT_TYPE_I8: val = (ulong)(long)m_value; break;
                        case ElementType.PELEMENT_TYPE_U8: val = (ulong)(ulong)m_value; break;
                        case ElementType.PELEMENT_TYPE_R4: val = BytesFromFloat( (float)m_value ); break;
                        case ElementType.PELEMENT_TYPE_R8: val = BytesFromDouble( (double)m_value ); break;

                        case ElementType.PELEMENT_TYPE_STRING:
                            {
                                byte[] buf = Encoding.UTF8.GetBytes( (string)m_value );

                                m_bf.WriteCompressedUnsigned( (uint)buf.Length );
                                m_bf.WriteArray( buf, 0, buf.Length );

                                BinaryFormatter.WriteLine( "Value: STRING {0}", m_value );

                                return c_Action_None;
                            }

                        default:
                            throw TypeDescriptorBasic.Error( "Bad primitive" );
                    }

                    bits = TypeDescriptorBasic.NumberOfBits( m_type.m_base.m_et );
                    fSigned = TypeDescriptorBasic.IsSigned( m_type.m_base.m_et );
                }
                else if(m_value is DateTime)
                {
                    val = (ulong)((DateTime)m_value).Ticks;
                    bits = 64;
                    fSigned = false;
                }
                else if(m_value is TimeSpan)
                {
                    val = (ulong)((TimeSpan)m_value).Ticks;
                    bits = 64;
                    fSigned = true;
                }
                else
                {
                    return TrackObject();
                }

                BinaryFormatter.WriteLine( "Value: NUM {0}", val );

                if(this.Hints_BitPacked != 0) bits = this.Hints_BitPacked;

                bool fValid = true;

                val -= (ulong)this.Hints_RangeBias;

                if(fSigned)
                {
                    long valS = (long)val;

                    if(this.Hints_Scale != 0) valS /= (long)this.Hints_Scale;

                    if(bits != 64)
                    {
                        long maxVal = (1L << (bits - 1)) - 1;

                        fValid = (valS <= maxVal) && (valS >= -maxVal - 1);
                    }

                    val = (ulong)valS;
                }
                else
                {
                    ulong valU = (ulong)val;

                    if(this.Hints_Scale != 0) valU /= (ulong)this.Hints_Scale;

                    if(bits != 64)
                    {
                        ulong maxVal = (1UL << bits) - 1;

                        fValid = (valU <= maxVal);
                    }

                    val = (ulong)valU;
                }

                if(!fValid)
                {
                    throw TypeDescriptorBasic.Error( String.Format( "Value outside range: Bits={0} Bias={1} Scale={2}", bits, this.Hints_RangeBias, this.Hints_Scale ) );
                }

                m_bf.WriteBitsLong( val, bits );

                return c_Action_None;
            }

            internal int ReadValue()
            {
                ulong val;
                int bits;
                bool fSigned;

                if(m_type.m_base.IsPrimitive)
                {
                    if(m_type.m_base.m_et == ElementType.PELEMENT_TYPE_STRING)
                    {
                        uint len = m_bf.ReadCompressedUnsigned();

                        if(len == 0xFFFFFFFF)
                        {
                            m_value = null;
                        }
                        else
                        {
                            byte[] buf = new byte[len];

                            m_bf.ReadArray( buf, 0, (int)len );

                            m_value = Encoding.UTF8.GetString( buf );
                        }

                        BinaryFormatter.WriteLine( "Value: STRING {0}", m_value );

                        return c_Action_None;
                    }

                    bits = TypeDescriptorBasic.NumberOfBits( m_type.m_base.m_et );
                    fSigned = TypeDescriptorBasic.IsSigned( m_type.m_base.m_et );

                    if(bits < 0)
                    {
                        throw TypeDescriptorBasic.Error( "Bad primitive" );
                    }
                }
                else if(m_type.Type == typeof( DateTime ))
                {
                    bits = 64;
                    fSigned = false;
                }
                else if(m_type.Type == typeof( TimeSpan ))
                {
                    bits = 64;
                    fSigned = true;
                }
                else
                {
                    return TrackObject();
                }

                if(this.Hints_BitPacked != 0) bits = this.Hints_BitPacked;
                val = m_bf.ReadBitsLong( bits );

                if(fSigned)
                {
                    long valS;

                    if(bits != 64)
                    {
                        valS = (long)(val << (64 - bits));
                        val = (ulong)(valS >> (64 - bits));
                    }

                    valS = (long)val;

                    if(this.Hints_Scale != 0) valS *= (long)this.Hints_Scale;

                    val = (ulong)valS;
                }
                else
                {
                    ulong valU;

                    if(bits != 64)
                    {
                        valU = (ulong)(val << (64 - bits));
                        val = (ulong)(valU >> (64 - bits));
                    }

                    valU = (ulong)val;

                    if(this.Hints_Scale != 0) valU *= (ulong)this.Hints_Scale;

                    val = (ulong)valU;
                }

                val += (ulong)this.Hints_RangeBias;

                BinaryFormatter.WriteLine( "Value: NUM {0}", val );

                if(m_type.Type == typeof( DateTime ))
                {
                    m_value = new DateTime( (long)val );
                }
                else if(m_type.Type == typeof( TimeSpan ))
                {
                    m_value = new TimeSpan( (long)val );
                }
                else
                {
                    switch(m_type.m_base.m_et)
                    {
                        case ElementType.PELEMENT_TYPE_BOOLEAN: m_value = val != 0; break;
                        case ElementType.PELEMENT_TYPE_CHAR: m_value = (char)val; break;
                        case ElementType.PELEMENT_TYPE_I1: m_value = (sbyte)val; break;
                        case ElementType.PELEMENT_TYPE_U1: m_value = (byte)val; break;
                        case ElementType.PELEMENT_TYPE_I2: m_value = (short)val; break;
                        case ElementType.PELEMENT_TYPE_U2: m_value = (ushort)val; break;
                        case ElementType.PELEMENT_TYPE_I4: m_value = (int)val; break;
                        case ElementType.PELEMENT_TYPE_U4: m_value = (uint)val; break;
                        case ElementType.PELEMENT_TYPE_I8: m_value = (long)val; break;
                        case ElementType.PELEMENT_TYPE_U8: m_value = (ulong)val; break;
                        case ElementType.PELEMENT_TYPE_R4: m_value = FloatFromBytes( val ); break;
                        case ElementType.PELEMENT_TYPE_R8: m_value = DoubleFromBytes( val ); break;
                    }
                }

                return c_Action_None;
            }

            unsafe float FloatFromBytes( ulong val )
            {
                float ret = 0.0f;

                if(m_bf.m_capabilities.FloatingPoint)
                {
                    uint val2 = (uint)val;

                    uint* ptr = &val2;

                    ret = *(float*)ptr;
                }
                else
                {
                    ret = ((float)val) / 1024;
                }

                return ret;
            }

            unsafe double DoubleFromBytes( ulong val )
            {
                double ret = 0.0;

                if(m_bf.m_capabilities.FloatingPoint)
                {
                    ulong* ptr = &val;

                    ret = *(double*)ptr;
                }
                else
                {
                    ret = ((double)val) / 65536;
                }

                return ret;
            }

            unsafe ulong BytesFromFloat( float val )
            {
                ulong ret = 0;

                if(m_bf.m_capabilities.FloatingPoint)
                {
                    float* ptr = &val;

                    ret = (ulong)*(uint*)ptr;
                }
                else
                {
                    ret = (ulong)(long)((float)val * 1024);
                }

                return ret;
            }

            unsafe ulong BytesFromDouble( double val )
            {
                ulong ret = 0;

                if(m_bf.m_capabilities.FloatingPoint)
                {
                    double* ptr = &val;

                    ret = *(ulong*)ptr;
                }
                else
                {
                    ret = (ulong)(long)((double)val * 65536);
                }

                return ret;
            }
        }

        class State
        {
            BinaryFormatter m_parent;
            State m_next = null;
            State m_prev = null;

            bool m_value_NeedProcessing = true;
            TypeHandler m_value;

            bool m_fields_NeedProcessing = false;
            Type m_fields_CurrentClass = null;
            FieldInfo[] m_fields_Fields = null;
            int m_fields_CurrentField = 0;

            bool m_array_NeedProcessing = false;
            Type m_array_ExpectedType = null;
            int m_array_CurrentPos = 0;
            int m_array_LastPos = 0;

            internal State( BinaryFormatter parent, SerializationHintsAttribute hints, Type t )
            {
                BinaryFormatter.WriteLine( "New State: {0}", t != null ? t.FullName : "<null>" );

                TypeDescriptor td = (t != null) ? new TypeDescriptor( t ) : null;

                m_parent = parent;
                m_value = new TypeHandler( parent, hints, td );
            }

            State CreateInstance( SerializationHintsAttribute hints, Type t )
            {
                State next = new State( m_parent, hints, t );

                m_next = next;
                next.m_prev = this;

                return next;
            }

            void GetValue()
            {
                State prev = m_prev;
                object o = null;

                if(prev == null)
                {
                    o = m_parent.m_value;
                }
                else
                {
                    if(prev.m_fields_NeedProcessing)
                    {
                        o = prev.m_fields_Fields[prev.m_fields_CurrentField - 1].GetValue( prev.m_value.m_value );
                    }

                    if(prev.m_array_NeedProcessing)
                    {
                        if(prev.m_value.m_type.m_base.IsArrayList)
                        {
                            ArrayList lst = (ArrayList)prev.m_value.m_value;

                            o = lst[prev.m_array_CurrentPos - 1];
                        }
                        else
                        {
                            Array arr = (Array)prev.m_value.m_value;

                            o = arr.GetValue( prev.m_array_CurrentPos - 1 );
                        }
                    }
                }

                BinaryFormatter.WriteLine( "New Object: {0}", o != null ? o.GetType().FullName : "<null>" );

                m_value.SetValue( o );
            }

            State SetValueAndDestroyInstance()
            {
                State prev = m_prev;

                if(prev == null)
                {
                    if(m_parent.m_fDeserialize)
                    {
                        m_parent.m_value = m_value.m_value;
                    }
                }
                else
                {
                    if(m_parent.m_fDeserialize)
                    {
                        object o = m_value.m_value;

                        if(prev.m_fields_NeedProcessing)
                        {
                            prev.m_fields_Fields[prev.m_fields_CurrentField - 1].SetValue( prev.m_value.m_value, o );
                        }

                        if(prev.m_array_NeedProcessing)
                        {
                            if(prev.m_value.m_type.m_base.IsArrayList)
                            {
                                ArrayList lst = (ArrayList)prev.m_value.m_value;

                                lst.Add( o );
                            }
                            else
                            {
                                Array arr = (Array)prev.m_value.m_value;

                                arr.SetValue( o, prev.m_array_CurrentPos - 1 );
                            }
                        }
                    }

                    prev.m_next = null;
                }

                return prev;
            }

            internal State Advance()
            {
                if(m_value_NeedProcessing)
                {
                    int res;

                    m_value_NeedProcessing = false;

                    if(m_parent.m_fDeserialize)
                    {
                        res = m_value.ReadSignature();
                    }
                    else
                    {
                        GetValue();

                        res = m_value.EmitSignature();
                    }

                    if(res != TypeHandler.c_Action_None)
                    {
                        object o;

                        if(m_parent.m_fDeserialize)
                        {
                            res = m_value.ReadValue();
                        }
                        else
                        {
                            res = m_value.EmitValue();
                        }

                        o = m_value.m_value;

                        switch(res)
                        {
                            case TypeHandler.c_Action_None:
                                break;

                            case TypeHandler.c_Action_ObjectFields:
                                {
                                    m_fields_NeedProcessing = true;
                                    m_fields_CurrentClass = o.GetType();
                                    m_fields_Fields = null;
                                    break;
                                }

                            case TypeHandler.c_Action_ObjectElements:
                                {
                                    m_array_NeedProcessing = true;
                                    m_array_CurrentPos = 0;

                                    if(o is ArrayList)
                                    {
                                        ArrayList lst = (ArrayList)o;

                                        m_array_ExpectedType = null;
                                        m_array_LastPos = m_parent.m_fDeserialize ? lst.Capacity : lst.Count;
                                    }
                                    else
                                    {
                                        Array arr = (Array)o;

                                        m_array_ExpectedType = arr.GetType().GetElementType();
                                        m_array_LastPos = arr.Length;
                                    }

                                    break;
                                }

                            default:
                                throw new System.Runtime.Serialization.SerializationException();
                        }
                    }
                }

                if(m_fields_NeedProcessing)
                {
                    return AdvanceToTheNextField();
                }

                if(m_array_NeedProcessing)
                {
                    return AdvanceToTheNextElement();
                }

                return SetValueAndDestroyInstance();
            }

            private State AdvanceToTheNextField()
            {
                while(m_fields_CurrentClass != null)
                {
                    if(m_fields_Fields == null)
                    {
                        m_fields_Fields = m_fields_CurrentClass.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
                        m_fields_CurrentField = 0;
                    }

                    if(m_fields_CurrentField < m_fields_Fields.Length)
                    {
                        FieldInfo f = m_fields_Fields[m_fields_CurrentField++];

                        if((f.Attributes & FieldAttributes.NotSerialized) == 0)
                        {
                            SerializationHintsAttribute hints;

                            if(m_value.m_type.m_base.IsEnum)
                            {
                                hints = m_value.m_hints;
                            }
                            else
                            {
                                hints = GetHints( f );
                            }

                            BinaryFormatter.WriteLine( "Field: {0} {1}", f.Name, f.FieldType );

                            return CreateInstance( hints, f.FieldType );
                        }
                    }
                    else
                    {
                        m_fields_CurrentClass = m_fields_CurrentClass.BaseType;
                        m_fields_Fields = null;
                    }
                }

                return SetValueAndDestroyInstance();
            }

            private State AdvanceToTheNextElement()
            {
                if(m_array_CurrentPos++ < m_array_LastPos)
                {
                    SerializationHintsAttribute hints;

                    if(m_value.m_hints != null && (m_value.m_hints.Flags & (SerializationFlags.FixedType | SerializationFlags.PointerNeverNull)) != 0)
                    {
                        hints = new SerializationHintsAttribute();

                        hints.BitPacked = 0;
                        hints.Flags = m_value.m_hints.Flags & (SerializationFlags.FixedType | SerializationFlags.PointerNeverNull);
                    }
                    else
                    {
                        hints = null;
                    }

                    return CreateInstance( hints, m_array_ExpectedType );
                }

                return SetValueAndDestroyInstance();
            }
        }

        static private bool s_fOnlySerializableObjects = false;
        static private Hashtable s_htLookupType;
        static private Hashtable s_htLookupHash;
        static private ArrayList s_lstProcessedAssemblies;

        BitStream m_stream;
        State m_first;
        object m_value;
        bool m_fDeserialize;
        readonly CLRCapabilities m_capabilities;

        Hashtable m_htDuplicates = new Hashtable();
        ArrayList m_lstObjects = new ArrayList();

        public BinaryFormatter( CLRCapabilities capabilities )
        {
            if(capabilities == null) capabilities = new CLRCapabilities();

            m_capabilities = capabilities;
        }

        public BinaryFormatter() : this(null)
        {
        }

        private void Initialize(Type t, bool fDeserialize)
        {
            m_first = new State( this, BuildHints( t ), t );
            m_fDeserialize = fDeserialize;            
            m_htDuplicates.Clear();
            m_lstObjects.Clear();
        }

        private void InitializeForSerialization( Type t, object o )
        {
            m_stream = new BitStream();
            m_value = o;
            Initialize( t, false );
        }

        private void InitializeForDeserialization( Type t, byte[] data, int pos, int len )
        {
            m_stream = new BitStream( data, pos, len );            
            m_value = null;
            Initialize( t, true );
        }

        private SerializationHintsAttribute BuildHints( Type t )
        {
            if(t == null) return null;

            SerializationHintsAttribute hints = new SerializationHintsAttribute();

            hints.Flags = SerializationFlags.FixedType | SerializationFlags.PointerNeverNull;

            return hints;
        }

        static BinaryFormatter()
        {
            Reset();            
        }

        static public void Initialize()
        {
            Initialize( false );
        }

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        static public void Initialize( bool fOnlySerializableObjects )
        {
            s_fOnlySerializableObjects = fOnlySerializableObjects;

            Reset();
            MonitorAssemblies = true;
            LoadTypesFromAppDomain();
        }
        
        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        static public void Reset()
        {
            s_htLookupType = new Hashtable();
            s_htLookupHash = new Hashtable();
            s_lstProcessedAssemblies = new ArrayList();

            AddType( typeof(Microsoft.SPOT.Messaging.RemotedException) );
        }       

        static public bool MonitorAssemblies
        {
            [MethodImplAttribute( MethodImplOptions.Synchronized )]
            set
            {
                AssemblyLoadEventHandler handler = new AssemblyLoadEventHandler( LoadTypesFromAssembly );

                if(value)
                {
                    AppDomain.CurrentDomain.AssemblyLoad += handler;
                }
                else
                {
                    AppDomain.CurrentDomain.AssemblyLoad -= handler;
                }
            }
        }

        static private void LoadTypesFromAssembly( object sender, AssemblyLoadEventArgs args )
        {
            LoadTypeFromAssembly( args.LoadedAssembly );
        }

        static public void LoadTypesFromAppDomain()
        {
            foreach(Assembly assm in AppDomain.CurrentDomain.GetAssemblies())
            {
                LoadTypeFromAssembly( assm );

                foreach(AssemblyName assmName in assm.GetReferencedAssemblies())
                {
                    LoadTypeFromAssembly( Assembly.Load( assmName ) );
                }
            }
        }


        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        static public void LoadTypeFromAssembly( Assembly assm )
        {
            if(s_lstProcessedAssemblies.Contains( assm ) == false)
            {
                s_lstProcessedAssemblies.Add( assm );

                foreach(Type t in assm.GetTypes())
                {
                    if(s_fOnlySerializableObjects && t.IsSerializable == false) continue;

                    AddType( t );
                }
            }
        }

        static public void LoadAllAssembliesFromDirectory( string dir )
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo( dir );

                foreach(FileInfo fi in di.GetFiles())
                {
                    try
                    {
                        if(fi.Name.EndsWith( ".dll" ) && fi.Name != "mscorlib.dll")
                        {
                            Assembly.LoadFrom( fi.FullName );
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        static internal void PopulateFromTypeForce( Type t )
        {
            LookupHash( t );
        }

        static public void PopulateFromType( Type t )
        {
            LookupHash( t );
        }

        static public void PopulateFromAssembly( Assembly assm )
        {
            foreach(Type t in assm.GetTypes())
            {
                LookupHash( t );
            }
        }

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        static public Hashtable GetAllTypeHashes()
        {
            return (Hashtable)s_htLookupHash.Clone();
        }
        
        static private void ConvertObject( object dst, object src )
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fi = src.GetType().GetFields( bindingFlags );
            for(int iField = 0; iField < fi.Length; iField++)
            {
                FieldInfo fieldSrc = fi[iField];

                FieldInfo fieldDst = dst.GetType().GetField( fieldSrc.Name, bindingFlags );

                if(fieldSrc.FieldType == typeof( System.Type ))
                {
                    continue;
                }

                if(fieldDst == null)
                {
                    Debug.Assert( false, "ConvertObject Incompatibility" );
                    continue;
                }

                object valSrc = fieldSrc.GetValue( src );
                object valDst = null;

                if((fieldSrc.FieldType.IsValueType || fieldSrc.FieldType.IsEnum) && (!fieldSrc.FieldType.IsPrimitive))
                {
                    valDst = fieldDst.GetValue( dst );

                    ConvertObject( valDst, valSrc );
                }
                else
                {
                    valDst = valSrc;
                }

                fieldDst.SetValue( dst, valDst );
            }
        }        

        static public SerializationHintsAttribute GetHints( MemberInfo mi )
        {
            SerializationHintsAttribute hintsDst = null;

            if(mi != null)
            {
                //In order to break the dependency between Microsoft.SPOT.Debugger and Microsoft.SPOT.Native
                //SerializationHintsAttribute class is defined in both places. 
                //Microsoft.SPOT.Debugger.SerializationHintsAttribute needs to remain backwards compatible with 
                //Microsoft.SPOT.SerializationHintsAttribute.  This method converts the attributes applied to a 
                //client assembly with Native class to the debugger class.


                object[] lst = mi.GetCustomAttributes( false );
                bool fFoundSerializationHints = false;

                for(int iAttr = 0; iAttr < lst.Length; iAttr++)
                {
                    object hintsSrc = lst[iAttr];

                    if(hintsSrc.GetType().FullName == "Microsoft.SPOT.SerializationHintsAttribute")
                    {
                        if(fFoundSerializationHints)
                        {
                            throw new NotSupportedException( "Only one attribute per type!" );
                        }

                        fFoundSerializationHints = true;

                        hintsDst = new SerializationHintsAttribute();

                        ConvertObject( hintsDst, hintsSrc );
                    }
                }
            }

            return hintsDst;
        }

        public byte[] Serialize( object o )
        {
            return Serialize( null, o );
        }

        public object Deserialize( byte[] v )
        {
            return Deserialize( null, v, 0, v.Length );
        }

        public object Deserialize( Type t, byte[] v )
        {
            return Deserialize( t, v, 0, v.Length );
        }

        public byte[] Serialize( Type t, object o )
        {
            InitializeForSerialization( t, o );

            State current = m_first;

            while(current != null)
            {
                current = current.Advance();
            }

            return m_stream.ToArray();
        }

        private object Deserialize( Type t, byte[] v, int pos, int len )
        {
            InitializeForDeserialization( t, v, pos, len );

            State current = m_first;

            while(current != null)
            {
                current = current.Advance();
            }

            return m_value;
        }

        void TrackDuplicate( object o )
        {
            if(o is Type) return;

            m_htDuplicates[o] = m_htDuplicates.Count;
            m_lstObjects.Add( o );
        }

        int SearchDuplicate( object o )
        {
            if(o is Type) return -1;

            if(m_htDuplicates.Contains( o ) == false) return -1;

            return (int)m_htDuplicates[o];
        }

        object GetDuplicate( int idx )
        {
            return m_lstObjects[idx];
        }

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        static internal void AddType( Type t )
        {
            if(t == null) return;

            if(s_htLookupHash.Contains( t )) return;

            //Ok, this is really evil. But it's this or 800 exceptions trying to load assemblies through BF.Initialize            
            //This is for dealing with generics                        
            if(t.FullName.IndexOf( '`' ) >= 0) return;
            //Lots of autogenerated code to ignore
            if(t.FullName.StartsWith( "Microsoft.Internal.Deployment" )) return;
            if(t.FullName == "ThisAssembly") return;
            if(t.FullName == "AssemblyRef") return;
            if(t.FullName.StartsWith( "<PrivateImplementationDetails>" )) return;

            AddTypeCore( t );

            AddType( t.GetElementType() );
            AddType( t.BaseType );

            foreach(FieldInfo f in t.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ))
            {
                if((f.Attributes & FieldAttributes.NotSerialized) != 0) continue;

                AddType( f.FieldType );
            }
        }
        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        static private void AddTypeCore( Type t )
        {
            try
            {
                uint hash = 0;

                //
                // Handling of special cases.
                //
                if(t == typeof( ArrayList ))
                {
                    hash = 0xEDDD427F;
                }
                else
                {
                    hash = ComputeHashForType( t, 0 );
                }

                Type tExists = s_htLookupType[hash] as Type;
                if(tExists != null)
                {
                    string error = string.Format( "Hash conflict: 0x{0:x8} {1}", hash, t.AssemblyQualifiedName, tExists.AssemblyQualifiedName );
                    BinaryFormatter.WriteLine( error );

                    throw new ApplicationException( error );
                }

                BinaryFormatter.WriteLine( "Hash: 0x{0:x8} {1}", hash, t.FullName );

                s_htLookupType[hash] = t;
                s_htLookupHash[t] = hash;
            }
            catch
            {
                BinaryFormatter.WriteLine( "AddType failed: {0}", t.FullName );
                s_htLookupHash[t] = INVALID_HASH_ENTRY;
            }
        }

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        static public Type LookupType( uint hash )
        {
            Type t = s_htLookupType[hash] as Type;

            return t;
        }

        [MethodImplAttribute( MethodImplOptions.Synchronized )]
        static public uint LookupHash( Type t )
        {
            object o = s_htLookupHash[t];
            if(o == null)
            {
                AddType( t );

                o = s_htLookupHash[t];
            }

            if(o == null)
            {
                throw new System.Runtime.Serialization.SerializationException();
            }

            return (uint)o;
        }

        static public uint LookupAssemblyHash( Assembly assm )
        {
            AssemblyName name = assm.GetName();

            return LookupAssemblyHash( name.Name, name.Version );
        }

        static public uint LookupAssemblyHash( string assemblyName, System.Version version )
        {
            uint hash;

            hash = ComputeHashForName( assemblyName, 0 );
            hash = ComputeHashForUShort( (ushort)version.Major, hash );
            hash = ComputeHashForUShort( (ushort)version.Minor, hash );
            hash = ComputeHashForUShort( (ushort)version.Build, hash );
            hash = ComputeHashForUShort( (ushort)version.Revision, hash );

            return hash;
        }

        private static uint ComputeHashForType( Type t, uint hash )
        {
            hash = ComputeHashForName( t.FullName, hash );

            while(t != null)
            {
                foreach(FieldInfo f in t.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ))
                {
                    if((f.Attributes & FieldAttributes.NotSerialized) != 0) continue;

                    hash = ComputeHashForField( f, hash );
                }

                t = t.BaseType;
            }

            return hash;
        }

        private static uint ComputeHashForField( FieldInfo f, uint hash )
        {
            Type t = f.FieldType;
            TypeDescriptor td;
            TypeDescriptorBasic tdSub;
            string name = f.Name;

            //
            // Special case for core types that have different representation on client and server.
            //
            if(f.DeclaringType == typeof( System.Reflection.MemberInfo ))
            {
                if(name == "m_cachedData") return hash;
            }

            if(f.DeclaringType == typeof( System.DateTime ))
            {
                if(name == "ticks") name = "m_ticks";

                if(name == "dateData")
                {
                    name = "m_ticks";
                    t = typeof( System.Int64 );
                }
            }

            if(f.DeclaringType == typeof( System.TimeSpan ))
            {
                if(name == "_ticks") name = "m_ticks";
            }

            td = new TypeDescriptor( t );

            if(td.m_base.IsArray)
            {
                int depth = td.m_arrayDepth;
                while(depth-- > 0)
                {
                    hash = ComputeHashForType( ElementType.PELEMENT_TYPE_SZARRAY, hash );
                }

                tdSub = td.m_arrayElement;
            }
            else
            {
                tdSub = td.m_base;
            }

            hash = ComputeHashForType( tdSub.m_et, hash );

            switch(tdSub.m_et)
            {
                case ElementType.PELEMENT_TYPE_CLASS:
                case ElementType.PELEMENT_TYPE_VALUETYPE:
                    hash = ComputeHashForName( tdSub.Type.FullName, hash );
                    break;
            }

            hash = ComputeHashForName( name, hash );

            return hash;
        }

        private static uint ComputeHashForType( ElementType et, uint hash )
        {
            uint hashPost = CRC.ComputeCRC( (byte)et, hash );

            return hashPost;
        }

        private static uint ComputeHashForName( string s, uint hash )
        {
            uint hashPost = CRC.ComputeCRC( Encoding.UTF8.GetBytes( s ), hash );

            return hashPost;
        }

        private static uint ComputeHashForUShort( ushort val, uint hash )
        {
            hash = CRC.ComputeCRC( (byte)(val >> 0), hash );
            hash = CRC.ComputeCRC( (byte)(val >> 8), hash );

            return hash;
        }

        [Conditional( "TRACE_SERIALIZATION" )]
        static public void WriteLine( string fmt, params object[] lst )
        {
            Console.WriteLine( fmt, lst );
        }

        internal int BitsAvailable()
        {
            return m_stream.BitsAvailable;
        }

        internal void WriteBits( uint val, int bits )
        {
            m_stream.WriteBits( val, bits );
        }

        internal uint ReadBits( int bits )
        {
            return m_stream.ReadBits( bits );
        }

        internal void WriteBitsLong( ulong val, int bits )
        {
            int extra = bits - 32;
            if(extra > 0)
            {
                m_stream.WriteBits( (uint)(val >> 32), extra );

                bits = 32;
            }

            m_stream.WriteBits( (uint)val, bits );
        }

        internal ulong ReadBitsLong( int bits )
        {
            ulong val;

            int extra = bits - 32;
            if(extra > 0)
            {
                val = (ulong)m_stream.ReadBits( extra ) << 32;
                bits = 32;
            }
            else
            {
                val = 0;
            }

            val |= (ulong)m_stream.ReadBits( bits );

            return val;
        }

        internal void WriteArray( byte[] data, int pos, int len )
        {
            m_stream.WriteArray( data, pos, len );
        }

        internal void ReadArray( byte[] data, int pos, int len )
        {
            m_stream.ReadArray( data, pos, len );
        }

        internal void WriteCompressedUnsigned( uint val )
        {
            if(val == 0xFFFFFFFF)
            {
                m_stream.WriteBits( 0xFF, 8 );
            }
            else if(val < 0x80)
            {
                m_stream.WriteBits( val, 8 );
            }
            else
            {
                if(val < 0x3F00)
                {
                    m_stream.WriteBits( 0x8000 | val, 16 );
                }
                else if(val < 0x3F000000)
                {
                    m_stream.WriteBits( 0xC0000000 | val, 32 );
                }
                else
                {
                    throw new ArgumentException( "Max value is 0x3F000000" );
                }
            }
        }

        internal uint ReadCompressedUnsigned()
        {
            uint val = m_stream.ReadBits( 8 );

            if(val == 0xFF) return 0xFFFFFFFF;

            switch(val & 0xC0)
            {
                case 0x00: break;
                case 0x40: break;
                case 0x80: val = ((val & ~0xC0U) << 8) | m_stream.ReadBits( 8 ); break;
                case 0xC0: val = ((val & ~0xC0U) << 24) | m_stream.ReadBits( 24 ); break;
            }

            return val;
        }

        internal void WriteType( Type t )
        {
            m_stream.WriteBits( LookupHash( t ), 32 );
        }

        internal Type ReadType()
        {
            uint hash = m_stream.ReadBits( 32 );
            Type t = LookupType( hash );

            if(t == null)
            {
                throw new System.Runtime.Serialization.SerializationException( String.Format( "Cannot find type for hash {0:X8}", hash ) );
            }

            return t;
        }

#if DEBUG

        public byte[] DebugSerialize( object o, string pathPrefix )
        {
            return DebugSerialize( o, null, pathPrefix );
        }

        public byte[] DebugSerialize( object o, Type type, string pathPrefix )
        {
            byte[] buf;

            try
            {
                buf = Serialize( type, o );
            }
            catch
            {
                DateTime dt = DateTime.Now;

                TraceDumpOriginalObject( o, pathPrefix, dt );

                throw;
            }

            if(buf != null)
            {
                object o2;

                try
                {
                    o2 = Deserialize( type, buf );
                }
                catch
                {
                    DateTime dt = DateTime.Now;

                    TraceDumpOriginalObject( o, pathPrefix, dt );
                    TraceDumpSerializedObject( buf, pathPrefix, dt );

                    throw;
                }
            }

            return buf;
        }

        void TraceDumpOriginalObject( object o, string pathPrefix, DateTime dt )
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                bf.Serialize( stream, o );

                SaveToFile( stream.ToArray(), pathPrefix, "orig", dt );
            }
            catch
            {
            }
        }

        void TraceDumpSerializedObject( byte[] buf, string pathPrefix, DateTime dt )
        {
            try
            {
                SaveToFile( buf, pathPrefix, "ser", dt );
            }
            catch
            {
            }
        }

        void SaveToFile( byte[] buf, string pathPrefix, string pathSuffix, DateTime dt )
        {
            string file = String.Format( "{0}_{2:yyyyMMdd}_{2:Hmmss}.{1}", pathPrefix, pathSuffix, dt );

            using(FileStream s = new FileStream( file, FileMode.Create, FileAccess.Write ))
            {
                s.Write( buf, 0, buf.Length );
                s.Close();
            }
        }

#endif //DEBUG

    }
}
