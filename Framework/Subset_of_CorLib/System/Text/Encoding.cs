////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Text
namespace System.Text
{
    // This abstract base class represents a character encoding. The class provides
    // methods to convert arrays and strings of Unicode characters to and from
    // arrays of bytes. A number of Encoding implementations are provided in
    // the System.Text package, including:
    //
    // ASCIIEncoding, which encodes Unicode characters as single 7-bit
    // ASCII characters. This encoding only supports character values between 0x00
    //     and 0x7F.
    // BaseCodePageEncoding, which encapsulates a Windows code page. Any
    //     installed code page can be accessed through this encoding, and conversions
    //     are performed using the WideCharToMultiByte and
    //     MultiByteToWideChar Windows API functions.
    // UnicodeEncoding, which encodes each Unicode character as two
    //    consecutive bytes. Both little-endian (code page 1200) and big-endian (code
    //    page 1201) encodings are recognized.
    // UTF7Encoding, which encodes Unicode characters using the UTF-7
    //     encoding (UTF-7 stands for UCS Transformation Format, 7-bit form). This
    //     encoding supports all Unicode character values, and can also be accessed
    //     as code page 65000.
    // UTF8Encoding, which encodes Unicode characters using the UTF-8
    //     encoding (UTF-8 stands for UCS Transformation Format, 8-bit form). This
    //     encoding supports all Unicode character values, and can also be accessed
    //     as code page 65001.
    // UTF32Encoding, both 12000 (little endian) & 12001 (big endian)
    //
    // In addition to directly instantiating Encoding objects, an
    // application can use the ForCodePage, GetASCII,
    // GetDefault, GetUnicode, GetUTF7, and GetUTF8
    // methods in this class to obtain encodings.
    //
    // Through an encoding, the GetBytes method is used to convert arrays
    // of characters to arrays of bytes, and the GetChars method is used to
    // convert arrays of bytes to arrays of characters. The GetBytes and
    // GetChars methods maintain no state between conversions, and are
    // generally intended for conversions of complete blocks of bytes and
    // characters in one operation. When the data to be converted is only available
    // in sequential blocks (such as data read from a stream) or when the amount of
    // data is so large that it needs to be divided into smaller blocks, an
    // application may choose to use a Decoder or an Encoder to
    // perform the conversion. Decoders and encoders allow sequential blocks of
    // data to be converted and they maintain the state required to support
    // conversions of data that spans adjacent blocks. Decoders and encoders are
    // obtained using the GetDecoder and GetEncoder methods.
    //
    // The core GetBytes and GetChars methods require the caller
    // to provide the destination buffer and ensure that the buffer is large enough
    // to hold the entire result of the conversion. When using these methods,
    // either directly on an Encoding object or on an associated
    // Decoder or Encoder, an application can use one of two methods
    // to allocate destination buffers.
    //
    // The GetByteCount and GetCharCount methods can be used to
    // compute the exact size of the result of a particular conversion, and an
    // appropriately sized buffer for that conversion can then be allocated.
    // The GetMaxByteCount and GetMaxCharCount methods can be
    // be used to compute the maximum possible size of a conversion of a given
    // number of bytes or characters, and a buffer of that size can then be reused
    // for multiple conversions.
    //
    // The first method generally uses less memory, whereas the second method
    // generally executes faster.
    //

    [Serializable()]
    public abstract class Encoding
    {
        public virtual byte[] GetBytes(String s)
        {
            return null;
        }

        public virtual int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            throw new NotImplementedException();
        }

        public virtual char[] GetChars(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public virtual char[] GetChars(byte[] bytes, int byteIndex, int byteCount)
        {
            throw new NotImplementedException();
        }

        public abstract Decoder GetDecoder();
        public static Encoding UTF8
        {
            get
            {
                return new UTF8Encoding();
            }
        }
    }
}


