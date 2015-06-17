Option Strict On
Option Infer On
Option Explicit On
Option Compare Binary

<Assembly: Microsoft.VisualBasic.Embedded()> 
Namespace System.Runtime.CompilerServices
    <Microsoft.VisualBasic.Embedded()> _
    <System.AttributeUsage(System.AttributeTargets.Class, Inherited:=False)> _
    <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
Public Class CompilerGeneratedAttribute
        Inherits System.Attribute
    End Class
End Namespace

Namespace Microsoft.VisualBasic
    Namespace CompilerServices
        <Microsoft.VisualBasic.Embedded()> _
        <System.Diagnostics.DebuggerNonUserCode(), System.Runtime.CompilerServices.CompilerGenerated()> _
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Public Class Operators
            Private Sub New()
            End Sub
            Public Shared Function CompareString(ByVal Left As String, ByVal Right As String, ByVal TextCompare As Boolean) As Integer
                If Left Is Right Then
                    Return 0
                End If
                If Left Is Nothing Then
                    If Right.Length() = 0 Then
                        Return 0
                    End If
                    Return -1
                End If
                If Right Is Nothing Then
                    If Left.Length() = 0 Then
                        Return 0
                    End If
                    Return 1
                End If
                Dim Result As Integer
                If TextCompare Then
                    Throw New System.NotImplementedException("'Option Compare Text' is not supported.")
                Else
                    Result = String.Compare(Left, Right)
                End If
                If Result = 0 Then
                    Return 0
                ElseIf Result > 0 Then
                    Return 1
                Else
                    Return -1
                End If
            End Function
        End Class
        <Microsoft.VisualBasic.Embedded()> _
        <System.Diagnostics.DebuggerNonUserCode(), System.Runtime.CompilerServices.CompilerGenerated()> _
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Public Class Conversions
            Private Sub New()
            End Sub
            Public Shared Function ToBoolean(ByVal Value As String) As Boolean
                If Value Is Nothing Then
                    Value = ""
                End If
                Try
                    If String.Compare(Value, Boolean.FalseString) = 0 Then
                        Return False
                        'ElseIf System.String.Compare(Value, Boolean.TrueString, True, loc) = 0 Then
                    ElseIf String.Compare(Value, Boolean.TrueString) = 0 Then
                        Return True
                    End If

                    Dim i64Value As System.Int64
                    If IsHexOrOctValue(Value, i64Value) Then
                        Return CBool(i64Value)
                    End If
                    Return CBool(ParseDouble(Value))
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            Public Shared Function ToBoolean(ByVal Value As Object) As Boolean
                If Value Is Nothing Then
                    Return False
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CBool(DirectCast(Value, Boolean))
                    Case TypeOf Value Is System.SByte
                        Return CBool(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CBool(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CBool(DirectCast(Value, System.Int16))
                    Case TypeOf Value Is System.UInt16
                        Return CBool(DirectCast(Value, System.UInt16))
                    Case TypeOf Value Is System.Int32
                        Return CBool(DirectCast(Value, System.Int32))
                    Case TypeOf Value Is System.UInt32
                        Return CBool(DirectCast(Value, System.UInt32))
                    Case TypeOf Value Is System.Int64
                        Return CBool(DirectCast(Value, System.Int64))
                    Case TypeOf Value Is System.UInt64
                        Return CBool(DirectCast(Value, System.UInt64))
                    Case TypeOf Value Is System.Single
                        Return CBool(DirectCast(Value, Single))
                    Case TypeOf Value Is System.Double
                        Return CBool(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime
                        Throw New System.Exception("DOES NOT SEEM LOGICAL (SPOTTY)")
                    Case TypeOf Value Is System.Char, TypeOf Value Is System.String
                        Return ToBoolean(DirectCast(Value, String))
                    Case Else
                End Select
                Throw New System.Exception(".NET Micro Framework does not support VB implicit conversions")
            End Function
            Public Shared Function ToByte(ByVal Value As String) As Byte
                If Value Is Nothing Then
                    Return 0
                End If
                Try
                    Dim i64Value As System.Int64
                    If IsHexOrOctValue(Value, i64Value) Then
                        Return CByte(i64Value)
                    End If
                    Return CByte(ParseDouble(Value))
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            Public Shared Function ToByte(ByVal Value As Object) As Byte
                If Value Is Nothing Then
                    Return 0
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CByte(DirectCast(Value, Boolean))
                    Case TypeOf Value Is System.SByte
                        Return CByte(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CByte(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CByte(DirectCast(Value, System.Int16))

                    Case TypeOf Value Is System.UInt16
                        Return CByte(DirectCast(Value, System.UInt16))

                    Case TypeOf Value Is System.Int32
                        Return CByte(DirectCast(Value, System.Int32))
                    Case TypeOf Value Is System.UInt32
                        Return CByte(DirectCast(Value, System.UInt32))
                    Case TypeOf Value Is System.Int64
                        Return CByte(DirectCast(Value, System.Int64))
                    Case TypeOf Value Is System.UInt64
                        Return CByte(DirectCast(Value, System.UInt64))
                    Case TypeOf Value Is System.Single
                        Return CByte(DirectCast(Value, Single))
                    Case TypeOf Value Is System.Double
                        Return CByte(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime, TypeOf Value Is System.Char
                    Case TypeOf Value Is System.String
                        Dim StringValue As String = TryCast(Value, String)
                        Return Byte.Parse(StringValue)
                    Case Else
                End Select

                Throw New System.InvalidCastException(".NET Micro Framework does not support VB Implicit Conversion.  Try casting to explicit type.")
            End Function
            <System.CLSCompliant(False)> _
            Public Shared Function ToSByte(ByVal Value As String) As SByte
                If Value Is Nothing Then
                    Return 0
                End If
                Try
                    Dim i64Value As System.Int64
                    If IsHexOrOctValue(Value, i64Value) Then
                        Return CSByte(i64Value)
                    End If
                    Return CSByte(ParseDouble(Value))
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            <System.CLSCompliant(False)> _
            Public Shared Function ToSByte(ByVal Value As Object) As SByte
                If Value Is Nothing Then
                    Return 0
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CSByte(DirectCast(Value, Boolean))

                    Case TypeOf Value Is System.SByte
                        Return CSByte(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CSByte(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CSByte(DirectCast(Value, System.Int16))

                    Case TypeOf Value Is System.UInt16
                        Return CSByte(DirectCast(Value, System.UInt16))

                    Case TypeOf Value Is System.Int32
                        Return CSByte(DirectCast(Value, System.Int32))

                    Case TypeOf Value Is System.UInt32
                        Return CSByte(DirectCast(Value, System.UInt32))

                    Case TypeOf Value Is System.Int64
                        Return CSByte(DirectCast(Value, System.Int64))

                    Case TypeOf Value Is System.UInt64
                        Return CSByte(DirectCast(Value, System.UInt64))

                    Case TypeOf Value Is System.Single
                        Return CSByte(DirectCast(Value, Single))
                    Case TypeOf Value Is System.Double
                        Return CSByte(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime, _
                         TypeOf Value Is System.Char
                    Case TypeOf Value Is System.String
                        Dim StringValue As String = TryCast(Value, String)
                        Return SByte.Parse(StringValue)
                    Case Else
                End Select

                Throw New System.InvalidCastException()
            End Function
            Public Shared Function ToShort(ByVal Value As String) As Short
                If Value Is Nothing Then
                    Return 0
                End If
                Try
                    Dim i64Value As System.Int64
                    If IsHexOrOctValue(Value, i64Value) Then
                        Return CShort(i64Value)
                    End If
                    Return CShort(ParseDouble(Value))
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            Public Shared Function ToShort(ByVal Value As Object) As Short
                If Value Is Nothing Then
                    Return 0
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CShort(DirectCast(Value, Boolean))
                    Case TypeOf Value Is System.SByte
                        Return CShort(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CShort(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CShort(DirectCast(Value, System.Int16))
                    Case TypeOf Value Is System.UInt16
                        Return CShort(DirectCast(Value, System.UInt16))
                    Case TypeOf Value Is System.Int32
                        Return CShort(DirectCast(Value, System.Int32))
                    Case TypeOf Value Is System.UInt32
                        Return CShort(DirectCast(Value, System.UInt32))
                    Case TypeOf Value Is System.Int64
                        Return CShort(DirectCast(Value, System.Int64))
                    Case TypeOf Value Is System.UInt64
                        Return CShort(DirectCast(Value, System.UInt64))
                    Case TypeOf Value Is System.Single
                        Return CShort(DirectCast(Value, Single))
                    Case TypeOf Value Is System.Double
                        Return CShort(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime, _
                         TypeOf Value Is System.Char
                    Case TypeOf Value Is System.String
                        Dim StringValue As String = TryCast(Value, String)
                        Return Short.Parse(StringValue)
                    Case Else
                End Select
                Throw New System.InvalidCastException()
            End Function
            <System.CLSCompliant(False)> _
            Public Shared Function ToUShort(ByVal Value As String) As UShort
                If Value Is Nothing Then
                    Return 0
                End If
                Try
                    Dim i64Value As System.Int64
                    If IsHexOrOctValue(Value, i64Value) Then
                        Return CUShort(i64Value)
                    End If
                    Return CUShort(ParseDouble(Value))
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            <System.CLSCompliant(False)> _
            Public Shared Function ToUShort(ByVal Value As Object) As UShort
                If Value Is Nothing Then
                    Return 0
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CUShort(DirectCast(Value, Boolean))
                    Case TypeOf Value Is System.SByte
                        Return CUShort(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CUShort(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CUShort(DirectCast(Value, System.Int16))
                    Case TypeOf Value Is System.UInt16
                        Return CUShort(DirectCast(Value, System.UInt16))
                    Case TypeOf Value Is System.Int32
                        Return CUShort(DirectCast(Value, System.Int32))
                    Case TypeOf Value Is System.UInt32
                        Return CUShort(DirectCast(Value, System.UInt32))
                    Case TypeOf Value Is System.Int64
                        Return CUShort(DirectCast(Value, System.Int64))
                    Case TypeOf Value Is System.UInt64
                        Return CUShort(DirectCast(Value, System.UInt64))
                    Case TypeOf Value Is System.Single
                        Return CUShort(DirectCast(Value, Single))
                    Case TypeOf Value Is System.Double
                        Return CUShort(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime, _
                         TypeOf Value Is System.Char
                    Case TypeOf Value Is System.String
                        Dim StringValue As String = TryCast(Value, String)
                        Return UShort.Parse(StringValue)
                    Case Else
                End Select
                Throw New System.InvalidCastException()
            End Function
            Public Shared Function ToInteger(ByVal Value As String) As Integer
                If Value Is Nothing Then
                    Return 0
                End If
                Try
                    Dim i64Value As System.Int64
                    If IsHexOrOctValue(Value, i64Value) Then
                        Return CInt(i64Value)
                    End If
                    Return CInt(ParseDouble(Value))
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            Public Shared Function ToInteger(ByVal Value As Object) As Integer
                If Value Is Nothing Then
                    Return 0
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CInt(DirectCast(Value, Boolean))
                    Case TypeOf Value Is System.SByte
                        Return CInt(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CInt(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CInt(DirectCast(Value, System.Int16))
                    Case TypeOf Value Is System.UInt16
                        Return CInt(DirectCast(Value, System.UInt16))
                    Case TypeOf Value Is System.Int32
                        Return CInt(DirectCast(Value, System.Int32))
                    Case TypeOf Value Is System.UInt32
                        Return CInt(DirectCast(Value, System.UInt32))
                    Case TypeOf Value Is System.Int64
                        Return CInt(DirectCast(Value, System.Int64))
                    Case TypeOf Value Is System.UInt64
                        Return CInt(DirectCast(Value, System.UInt64))
                    Case TypeOf Value Is System.Single
                        Return CInt(DirectCast(Value, Single))
                    Case TypeOf Value Is System.Double
                        Return CInt(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime, _
                         TypeOf Value Is System.Char
                    Case TypeOf Value Is System.String
                        Dim StringValue As String = TryCast(Value, String)
                        Return Integer.Parse(StringValue)
                    Case Else
                End Select
                Throw New System.InvalidCastException()
            End Function
            <System.CLSCompliant(False)> _
            Public Shared Function ToUInteger(ByVal Value As String) As UInteger
                If Value Is Nothing Then
                    Return 0
                End If
                Try
                    Dim i64Value As System.Int64
                    If IsHexOrOctValue(Value, i64Value) Then
                        Return CUInt(i64Value)
                    End If
                    Return CUInt(ParseDouble(Value))
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            <System.CLSCompliant(False)> _
            Public Shared Function ToUInteger(ByVal Value As Object) As UInteger
                If Value Is Nothing Then
                    Return 0
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CUInt(DirectCast(Value, Boolean))
                    Case TypeOf Value Is System.SByte
                        Return CUInt(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CUInt(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CUInt(DirectCast(Value, System.Int16))
                    Case TypeOf Value Is System.UInt16
                        Return CUInt(DirectCast(Value, System.UInt16))
                    Case TypeOf Value Is System.Int32
                        Return CUInt(DirectCast(Value, System.Int32))
                    Case TypeOf Value Is System.UInt32
                        Return CUInt(DirectCast(Value, System.UInt32))
                    Case TypeOf Value Is System.Int64
                        Return CUInt(DirectCast(Value, System.Int64))
                    Case TypeOf Value Is System.UInt64
                        Return CUInt(DirectCast(Value, System.UInt64))
                    Case TypeOf Value Is System.Single
                        Return CUInt(DirectCast(Value, Single))
                    Case TypeOf Value Is System.Double
                        Return CUInt(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime, _
                         TypeOf Value Is System.Char

                    Case TypeOf Value Is System.String
                        Return ToUInteger(TryCast(Value, String))
                    Case Else
                End Select
                Throw New System.InvalidCastException()
            End Function
            Public Shared Function ToLong(ByVal Value As String) As Long
                If (Value Is Nothing) Then
                    Return 0
                End If
                Try
                    Dim i64Value As System.Int64
                    If IsHexOrOctValue(Value, i64Value) Then
                        Return CLng(i64Value)
                    End If
                    Return CLng(ParseDouble(Value))
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            Public Shared Function ToLong(ByVal Value As Object) As Long
                If Value Is Nothing Then
                    Return 0
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CLng(DirectCast(Value, Boolean))
                    Case TypeOf Value Is System.SByte
                        Return CLng(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CLng(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CLng(DirectCast(Value, System.Int16))
                    Case TypeOf Value Is System.UInt16
                        Return CLng(DirectCast(Value, System.UInt16))
                    Case TypeOf Value Is System.Int32
                        Return CLng(DirectCast(Value, System.Int32))
                    Case TypeOf Value Is System.UInt32
                        Return CLng(DirectCast(Value, System.UInt32))
                    Case TypeOf Value Is System.Int64
                        Return CLng(DirectCast(Value, System.Int64))
                    Case TypeOf Value Is System.UInt64
                        Return CLng(DirectCast(Value, System.UInt64))
                    Case TypeOf Value Is System.Single
                        Return CLng(DirectCast(Value, Single))
                    Case TypeOf Value Is System.Double
                        Return CLng(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime, TypeOf Value Is System.Char
                    Case TypeOf Value Is System.String
                        Dim StringValue As String = TryCast(Value, String)
                        Return Long.Parse(StringValue)
                    Case Else
                End Select
                Throw New System.InvalidCastException()
            End Function
            <System.CLSCompliant(False)> _
            Public Shared Function ToULong(ByVal Value As String) As ULong
                If (Value Is Nothing) Then
                    Return 0
                End If
                Try
                    Dim ui64Value As System.UInt64
                    If IsHexOrOctValue(Value, ui64Value) Then
                        Return CULng(ui64Value)
                    End If
                    Return CULng(ParseDouble(Value))
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            <System.CLSCompliant(False)> _
            Public Shared Function ToULong(ByVal Value As Object) As ULong
                If Value Is Nothing Then
                    Return 0
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CULng(DirectCast(Value, Boolean))
                    Case TypeOf Value Is System.SByte
                        Return CULng(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CULng(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CULng(DirectCast(Value, System.Int16))
                    Case TypeOf Value Is System.UInt16
                        Return CULng(DirectCast(Value, System.UInt16))
                    Case TypeOf Value Is System.Int32
                        Return CULng(DirectCast(Value, System.Int32))
                    Case TypeOf Value Is System.UInt32
                        Return CULng(DirectCast(Value, System.UInt32))
                    Case TypeOf Value Is System.Int64
                        Return CULng(DirectCast(Value, System.Int64))
                    Case TypeOf Value Is System.UInt64
                        Return CULng(DirectCast(Value, System.UInt64))
                    Case TypeOf Value Is System.Single
                        Return CULng(DirectCast(Value, Single))
                    Case TypeOf Value Is System.Double
                        Return CULng(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime, _
                         TypeOf Value Is System.Char
                    Case TypeOf Value Is System.String
                        Dim StringValue As String = TryCast(Value, String)
                        Return ULong.Parse(StringValue)
                    Case Else
                End Select
                Throw New System.InvalidCastException()
            End Function
            Private Shared Function GetNormalizedNumberFormat(ByVal InNumberFormat As System.Globalization.NumberFormatInfo) As System.Globalization.NumberFormatInfo
                With InNumberFormat
                    If (Not .NumberDecimalSeparator Is Nothing) AndAlso _
                    (Not .NumberGroupSeparator Is Nothing) AndAlso _
                       (.NumberDecimalSeparator.Length = 1) AndAlso _
                       (.NumberGroupSeparator.Length = 1) Then
                        Return InNumberFormat
                    End If
                End With
                Throw New System.Exception("Currency Format Not Supported")
            End Function

            Public Shared Function ToSingle(ByVal Value As String) As Single
                If Value Is Nothing Then
                    Return 0
                End If
                Try
                    Dim i64Value As System.Int64
                    If IsHexOrOctValue(Value, i64Value) Then
                        Return CSng(i64Value)
                    End If
                    Dim Result As Double = ParseDouble(Value)
                    If (Result < System.Single.MinValue OrElse Result > System.Single.MaxValue) Then
                        Throw New System.Exception
                    End If
                    Return CSng(Result)
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            Public Shared Function ToSingle(ByVal Value As Object) As Single
                If Value Is Nothing Then
                    Return 0
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CSng(DirectCast(Value, Boolean))
                    Case TypeOf Value Is System.SByte
                        Return CSng(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CSng(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CSng(DirectCast(Value, System.Int16))
                    Case TypeOf Value Is System.UInt16
                        Return CSng(DirectCast(Value, System.UInt16))
                    Case TypeOf Value Is System.Int32
                        Return CSng(DirectCast(Value, System.Int32))
                    Case TypeOf Value Is System.UInt32
                        Return CSng(DirectCast(Value, System.UInt32))
                    Case TypeOf Value Is System.Int64
                        Return CSng(DirectCast(Value, System.Int64))
                    Case TypeOf Value Is System.UInt64
                        Return CSng(DirectCast(Value, System.UInt64))
                    Case TypeOf Value Is System.Single
                        Return DirectCast(Value, Single)
                    Case TypeOf Value Is System.Double
                        Return CSng(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime, _
                         TypeOf Value Is System.Char
                    Case TypeOf Value Is System.String
                        Return ToSingle(TryCast(Value, String))
                    Case Else
                End Select
                Throw New System.InvalidCastException()
            End Function
            Public Shared Function ToDouble(ByVal Value As String) As Double
                If Value Is Nothing Then
                    Return 0
                End If
                Try
                    Dim i64Value As System.Int64
                    If IsHexOrOctValue(Value, i64Value) Then
                        Return CDbl(i64Value)
                    End If
                    Return ParseDouble(Value)
                Catch e As System.Exception
                    Throw New System.InvalidCastException(e.Message, e)
                End Try
            End Function
            Public Shared Function ToDouble(ByVal Value As Object) As Double
                If Value Is Nothing Then
                    Return 0
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return CDbl(DirectCast(Value, Boolean))
                    Case TypeOf Value Is System.SByte
                        Return CDbl(DirectCast(Value, SByte))
                    Case TypeOf Value Is System.Byte
                        Return CDbl(DirectCast(Value, Byte))
                    Case TypeOf Value Is System.Int16
                        Return CDbl(DirectCast(Value, System.Int16))
                    Case TypeOf Value Is System.UInt16
                        Return CDbl(DirectCast(Value, System.UInt16))
                    Case TypeOf Value Is System.Int32
                        Return CDbl(DirectCast(Value, System.Int32))
                    Case TypeOf Value Is System.UInt32
                        Return CDbl(DirectCast(Value, System.UInt32))
                    Case TypeOf Value Is System.Int64
                        Return CDbl(DirectCast(Value, System.Int64))
                    Case TypeOf Value Is System.UInt64
                        Return CDbl(DirectCast(Value, System.UInt64))
                    Case TypeOf Value Is System.Single
                        Return CDbl(DirectCast(Value, Single))
                    Case TypeOf Value Is System.Double
                        Return CDbl(DirectCast(Value, Double))
                    Case TypeOf Value Is System.DateTime, _
                         TypeOf Value Is System.Char
                    Case TypeOf Value Is System.String
                        Dim stringValue As String = TryCast(Value, String)
                        Return Double.Parse(StringValue)
                    Case Else
                End Select
                Throw New System.InvalidCastException()
            End Function
            Private Shared Function ParseDouble(ByVal Value As String) As Double
                Dim NormalizedNumberFormat As System.Globalization.NumberFormatInfo
                Dim culture As System.Globalization.CultureInfo = GetCultureInfo()
                Dim NumberFormat As System.Globalization.NumberFormatInfo = culture.NumberFormat
                NormalizedNumberFormat = GetNormalizedNumberFormat(NumberFormat)
                Value = ToHalfwidthNumbers(Value)
                Try
                    Return System.Double.Parse(Value)
                Catch Ex As System.Exception
                    Throw Ex
                End Try
            End Function

            Public Shared Function ToDate(ByVal Value As String) As Date
                Dim ParsedDate As System.DateTime
                Dim Culture As System.Globalization.CultureInfo = GetCultureInfo()
                Try
                    ParsedDate = ToDate(Value)
                    Return ParsedDate
                Catch ex As System.Exception
                    Throw New System.InvalidCastException()
                End Try
            End Function
            Public Shared Function ToDate(ByVal Value As Object) As Date
                If Value Is Nothing Then
                    Return Nothing
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean, _
                         TypeOf Value Is System.SByte, _
                         TypeOf Value Is System.Byte, _
                         TypeOf Value Is System.Int16, _
                         TypeOf Value Is System.UInt16, _
                         TypeOf Value Is System.Int32, _
                         TypeOf Value Is System.UInt32, _
                         TypeOf Value Is System.Int64, _
                         TypeOf Value Is System.UInt64, _
                         TypeOf Value Is System.Single, _
                         TypeOf Value Is System.Double, _
                         TypeOf Value Is System.Char
                    Case TypeOf Value Is System.DateTime
                        Return CDate(DirectCast(Value, System.DateTime))
                    Case TypeOf Value Is System.String
                        Dim StringValue As String = TryCast(Value, String)
                        Return ToDate(StringValue)
                    Case Else
                End Select
                Throw New System.InvalidCastException()
            End Function
            Public Shared Function ToChar(ByVal Value As String) As Char
                If (Value Is Nothing) OrElse (Value.Length = 0) Then
                    Return System.Convert.ToChar(0 And &HFFFFI)
                End If
                Return Value.Chars(0)
            End Function
            Public Shared Function ToChar(ByVal Value As Object) As Char
                If Value Is Nothing Then
                    Return System.Convert.ToChar(0 And &HFFFFI)
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean, _
                         TypeOf Value Is System.SByte, _
                         TypeOf Value Is System.Byte, _
                         TypeOf Value Is System.Int16, _
                         TypeOf Value Is System.UInt16, _
                         TypeOf Value Is System.Int32, _
                         TypeOf Value Is System.UInt32, _
                         TypeOf Value Is System.Int64, _
                         TypeOf Value Is System.UInt64, _
                         TypeOf Value Is System.Single, _
                         TypeOf Value Is System.Double, _
                         TypeOf Value Is System.DateTime
                    Case TypeOf Value Is System.Char
                        Return CChar(DirectCast(Value, Char))
                    Case TypeOf Value Is System.String
                        Return ToChar(TryCast(Value, String))
                    Case Else
                End Select
                Throw New System.InvalidCastException()
            End Function

            Public Shared Function ToCharArrayRankOne(ByVal Value As String) As Char()
                If Value Is Nothing Then Value = ""
                Return Value.ToCharArray()
            End Function

            Public Shared Function ToCharArrayRankOne(ByVal Value As Object) As Char()
                If Value Is Nothing Then
                    Dim c(-1) As Char
                    Return c
                End If

                Dim ArrayValue As Char() = TryCast(Value, Char())
                If ArrayValue IsNot Nothing Then
                    Return ArrayValue
                Else
                    If TypeOf Value Is String Then Return DirectCast(Value, String).ToCharArray()
                End If

                Throw New System.InvalidCastException()
            End Function
            Public Shared Shadows Function ToString(ByVal Value As Short) As String
                Return Value.ToString()
            End Function
            Public Shared Shadows Function ToString(ByVal Value As Integer) As String
                Return Value.ToString()
            End Function
            <System.CLSCompliant(False)> _
            Public Shared Shadows Function ToString(ByVal Value As UInteger) As String
                Return Value.ToString()
            End Function
            Public Shared Shadows Function ToString(ByVal Value As Long) As String
                Return Value.ToString()
            End Function
            <System.CLSCompliant(False)> _
            Public Shared Shadows Function ToString(ByVal Value As ULong) As String
                Return Value.ToString()
            End Function
            Public Shared Shadows Function ToString(ByVal Value As Single) As String
                Return Value.ToString()
            End Function
            Public Shared Shadows Function ToString(ByVal Value As Double) As String
                Return Value.ToString("G")
            End Function
            Public Shared Shadows Function ToString(ByVal Value As Date) As String
                Dim TimeTicks As Long = Value.TimeOfDay.Ticks
                If (TimeTicks = Value.Ticks) OrElse _
                    (Value.Year = 1899 AndAlso Value.Month = 12 AndAlso Value.Day = 30) Then
                    Return Value.ToString("T")
                ElseIf TimeTicks = 0 Then
                    Return Value.ToString("d")
                Else
                    Return Value.ToString("G")
                End If
            End Function
            Public Shared Shadows Function ToString(ByVal Value As Object) As String
                If Value Is Nothing Then
                    Return Nothing
                Else
                    Dim StringValue As String = TryCast(Value, String)
                    If StringValue IsNot Nothing Then
                        Return StringValue
                    End If
                End If
                Select Case True
                    Case TypeOf Value Is System.Boolean
                        Return DirectCast(Value, System.Boolean).ToString
                    Case TypeOf Value Is System.SByte
                        Return Cint(DirectCast(Value, System.SByte)).ToString
                    Case TypeOf Value Is System.Byte
                        Return DirectCast(Value, System.Byte).ToString
                    Case TypeOf Value Is System.Int16
                        Return Cint(DirectCast(Value, System.Int16)).ToString
                    Case TypeOf Value Is System.UInt16
                        Return DirectCast(Value, System.UInt16).ToString
                    Case TypeOf Value Is System.Int32
                        Return DirectCast(Value, System.Int32).ToString
                    Case TypeOf Value Is System.UInt32
                        Return DirectCast(Value, System.UInt32).ToString
                    Case TypeOf Value Is System.Int64
                        Return DirectCast(Value, System.Int64).ToString
                    Case TypeOf Value Is System.UInt64
                        Return DirectCast(Value, System.UInt64).ToString
                    Case TypeOf Value Is System.Single
                        Return DirectCast(Value, System.Single).ToString
                    Case TypeOf Value Is System.Double
                        Return DirectCast(Value, System.Double).ToString
                    Case TypeOf Value Is System.Char
                        Return DirectCast(Value, System.Char).ToString
                    Case TypeOf Value Is System.DateTime
                        Return DirectCast(Value, System.DateTime).ToString
                    Case TypeOf Value Is System.String
                        Return DirectCast(Value, String)
                    Case Else
                End Select
                Throw New System.InvalidCastException()
           End Function
            Public Shared Shadows Function ToString(ByVal Value As Boolean) As String
                If Value Then
                    Return System.Boolean.TrueString
                Else
                    Return System.Boolean.FalseString
                End If
            End Function
            Public Shared Shadows Function ToString(ByVal Value As Byte) As String
                Return Value.ToString()
            End Function
            Public Shared Shadows Function ToString(ByVal Value As Char) As String
                Return Value.ToString()
            End Function
            Public Shared Function GetCultureInfo() As System.Globalization.CultureInfo
                'TODO: CHANGED TO PICK UP CULTUREINFO FROM GLOBALIZATION
                Return System.Globalization.CultureInfo.CurrentUICulture 'System.Threading.Thread.CurrentThread.CurrentCulture
            End Function
            Public Shared Function ToHalfwidthNumbers(ByVal s As String) As String
                Return s
            End Function
            Public Shared Function IsHexOrOctValue(ByVal Value As String, ByRef i64Value As System.Int64) As Boolean
                Dim ch As Char
                Dim Length As Integer
                Dim FirstNonspace As Integer
                Dim TmpValue As String
                Length = Value.Length
                Do While (FirstNonspace < Length)
                    ch = Value.Chars(FirstNonspace)
                    If ch = "&"c AndAlso FirstNonspace + 2 < Length Then
                        GoTo GetSpecialValue
                    End If
                    If ch <> Strings.ChrW(32) AndAlso ch <> Strings.ChrW(&H3000) Then
                        Return False
                    End If
                    FirstNonspace += 1
                Loop
                Return False
GetSpecialValue:
                ch = CType(Value.Chars(FirstNonspace + 1), Char)

                TmpValue = ToHalfwidthNumbers(Value.Substring(FirstNonspace + 2))
                If ch = "h"c Then
                    i64Value = System.Convert.ToInt64(TmpValue)
                ElseIf ch = "o"c Then
                    i64Value = System.Convert.ToInt64(TmpValue)
                Else
                    Throw New System.Exception
                End If
                Return True
            End Function
            Public Shared Function IsHexOrOctValue(ByVal Value As String, ByRef ui64Value As System.UInt64) As Boolean
                Dim ch As Char
                Dim Length As Integer
                Dim FirstNonspace As Integer
                Dim TmpValue As String
                Length = Value.Length
                Do While (FirstNonspace < Length)
                    ch = Value.Chars(FirstNonspace)
                    If ch = "&"c AndAlso FirstNonspace + 2 < Length Then
                        GoTo GetSpecialValue
                    End If
                    If ch <> Strings.ChrW(32) AndAlso ch <> Strings.ChrW(&H3000) Then
                        Return False
                    End If
                    FirstNonspace += 1
                Loop
                Return False
GetSpecialValue:
                ch = CType(Value.Chars(FirstNonspace + 1), Char)

                TmpValue = ToHalfwidthNumbers(Value.Substring(FirstNonspace + 2))
                If ch = "h"c Then
                    ui64Value = System.Convert.ToUInt64(TmpValue)
                ElseIf ch = "o"c Then
                    ui64Value = System.Convert.ToUInt64(TmpValue)
                Else
                    Throw New System.Exception
                End If
                Return True
            End Function
        End Class
        <Microsoft.VisualBasic.Embedded()> _
        <System.Diagnostics.DebuggerNonUserCode(), System.Runtime.CompilerServices.CompilerGenerated()> _
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Public Class ProjectData
            Private Sub New()
            End Sub
            Public Overloads Shared Sub SetProjectError(ByVal ex As System.Exception)
            End Sub
            Public Overloads Shared Sub SetProjectError(ByVal ex As System.Exception, ByVal lErl As Integer)
            End Sub
            Public Shared Sub ClearProjectError()
            End Sub
        End Class
        <Microsoft.VisualBasic.Embedded()> _
        <System.Diagnostics.DebuggerNonUserCode(), System.Runtime.CompilerServices.CompilerGenerated()> _
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Public Class ObjectFlowControl
            Public Class ForLoopControl
                Public Shared Function ForNextCheckR4(ByVal count As Single, ByVal limit As Single, ByVal StepValue As Single) As Boolean
                    If StepValue >= 0 Then
                        Return count <= limit
                    Else
                        Return count >= limit
                    End If
                End Function
                Public Shared Function ForNextCheckR8(ByVal count As Double, ByVal limit As Double, ByVal StepValue As Double) As Boolean
                    If StepValue >= 0 Then
                        Return count <= limit
                    Else
                        Return count >= limit
                    End If
                End Function
            End Class
        End Class
        <Microsoft.VisualBasic.Embedded()> _
        <System.Diagnostics.DebuggerNonUserCode(), System.Runtime.CompilerServices.CompilerGenerated()> _
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Public Class StaticLocalInitFlag
            Public State As Short
        End Class
        <Microsoft.VisualBasic.Embedded()> _
        <System.Diagnostics.DebuggerNonUserCode(), System.Runtime.CompilerServices.CompilerGenerated()> _
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Public Class IncompleteInitialization
            Inherits System.Exception
            Public Sub New()
                MyBase.New()
            End Sub
        End Class
        <Microsoft.VisualBasic.Embedded()> _
        <System.AttributeUsage(System.AttributeTargets.Class, Inherited:=False)> _
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Public Class StandardModuleAttribute
            Inherits System.Attribute
        End Class
        <Microsoft.VisualBasic.Embedded()> _
        <System.AttributeUsage(System.AttributeTargets.Class, Inherited:=False)> _
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Public Class DesignerGeneratedAttribute
            Inherits System.Attribute
        End Class
        <Microsoft.VisualBasic.Embedded()> _
        <System.AttributeUsage(System.AttributeTargets.Parameter, Inherited:=False)> _
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Public Class OptionCompareAttribute
            Inherits System.Attribute
        End Class
        <Microsoft.VisualBasic.Embedded()> _
        <System.AttributeUsage(System.AttributeTargets.Class, Inherited:=False)> _
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
        Public Class OptionTextAttribute
            Inherits System.Attribute
        End Class
    End Namespace

    <System.AttributeUsage(System.AttributeTargets.Class Or System.AttributeTargets.Module Or System.AttributeTargets.Assembly, Inherited:=False)> _
    <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
    Public Class EmbeddedAttribute
        Inherits System.Attribute
    End Class
    <Microsoft.VisualBasic.Embedded()> _
    <System.AttributeUsage(System.AttributeTargets.Class, Inherited:=False)> _
    <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _
    Public Class HideModuleNameAttribute
        Inherits System.Attribute
    End Class

    <Microsoft.VisualBasic.Embedded(), System.Diagnostics.DebuggerNonUserCode()> _
    Public Class Strings
        Public Shared Function ChrW(ByVal CharCode As Integer) As Char
            If CharCode < -32768 OrElse CharCode > 65535 Then
                Throw New System.ArgumentException()
            End If
            Return System.Convert.ToChar(CType(CharCode And &HFFFFI, UShort))
        End Function
        Public Shared Function AscW(ByVal [String] As String) As Integer
            If ([String] Is Nothing) OrElse ([String].Length = 0) Then
                Throw New System.ArgumentException()
            End If
            Return AscW([String].Chars(0))
        End Function
        Public Shared Function AscW(ByVal [String] As Char) As Integer
            Return AscW([String])
        End Function
    End Class
    <Microsoft.VisualBasic.Embedded(), System.Diagnostics.DebuggerNonUserCode()> _
    Public Class Constants
        Public Const vbCrLf As String = Strings.ChrW(13) & Strings.ChrW(10)
        Public Const vbNewLine As String = Strings.ChrW(13) & Strings.ChrW(10)
        Public Const vbCr As String = Strings.ChrW(13)
        Public Const vbLf As String = Strings.ChrW(10)
        Public Const vbBack As String = Strings.ChrW(8)
        Public Const vbFormFeed As String = Strings.ChrW(12)
        Public Const vbTab As String = Strings.ChrW(9)
        Public Const vbVerticalTab As String = Strings.ChrW(11)
        Public Const vbNullChar As String = Strings.ChrW(0)
        Public Const vbNullString As String = Nothing
    End Class
End Namespace
