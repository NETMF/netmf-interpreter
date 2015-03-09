
namespace System.Security.Cryptography
{
    /// <summary>
    /// Specifies the block cipher mode to use for encryption.
    /// </summary>
    public enum CipherMode
    {
        CBC = 1,
        ECB = 2,
        OFB = 3,
        CFB = 4,
        CTS = 5
    }

    /// <summary>
    /// Specifies the type of padding to apply when the message data block is shorter than the full number of bytes needed for a cryptographic operation.
    /// </summary>
    public enum PaddingMode
    {

        None = 1,
        PKCS7 = 2,
        Zeros = 3,
        ANSIX923 = 4,
        ISO10126 = 5
    }

    /// <summary>
    /// The exception that is thrown when an error occurs during a cryptographic operation.
    /// </summary>
    public class CryptographicException : Exception
    {
        private int m_errorCode;

        /// <summary>
        /// Initializes a new instance of the CryptographicException class with default properties.
        /// </summary>
        public CryptographicException()
        {
            m_errorCode = 0;
        }

        /// <summary>
        /// Initializes a new instance of the CryptographicException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public CryptographicException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Gets the internal error code that caused the excpetion.
        /// </summary>
        public int ErrorCode
        {
            get
            {
                return m_errorCode;
            }
        }
    }
}

namespace Microsoft.SPOT.Cryptoki
{
    using System;
    using System.Security.Cryptography;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Defines a utility class for converting values to bytes and vice versa.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        ///  Convert from an int to bytes.
        /// </summary>
        /// <param name="value">Integer value to be converted.</param>
        /// <returns>Byte array representation.</returns>
        public static byte[] ConvertToBytes(Int32 value)
        {
            return new byte[] { (byte)(value >> 0), (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) };
        }

        /// <summary>
        /// Convert from an int to bytes
        /// </summary>
        /// <param name="value">Integer value to be converted.</param>
        /// <param name="dest">Destination byte array.</param>
        /// <param name="offset">Offset destination index for the output bytes.</param>
        public static void ConvertToBytes(Int32 value, byte[] dest, int offset)
        {
            dest[offset++] = (byte)(value >>  0);
            dest[offset++] = (byte)(value >>  8);
            dest[offset++] = (byte)(value >> 16);
            dest[offset++] = (byte)(value >> 24);
        }

        /// <summary>
        /// Converts from a byte array to a 64 bit integer.
        /// </summary>
        /// <param name="data">Input data bytes to be converted to a 64 bit integer.</param>
        /// <returns>Resulting 64 bit conversion value.</returns>
        public static Int64 ConvertToInt64(byte[] data)
        {
            return (Int64)((UInt64)data[0] << 0  | (UInt64)data[1] <<  8 | (UInt64)data[2] << 16 | (UInt64)data[3] << 24 |
                           (UInt64)data[4] << 32 | (UInt64)data[5] << 40 | (UInt64)data[6] << 48 | (UInt64)data[7] << 56);
        }

        /// <summary>
        /// Converts from a byte array to an integer value.
        /// </summary>
        /// <param name="data">Input data bytes to be converted.</param>
        /// <returns>Resulting 32 bit conversion value</returns>
        public static Int32 ConvertToInt32(byte[] data)
        {
            return (Int32)((UInt32)data[0] << 0 | (UInt32)data[1] << 8 | (UInt32)data[2] << 16 | (UInt32)data[3] << 24);
        }

        /// <summary>
        /// Converts from a byte array to an integer value.
        /// </summary>
        /// <param name="data">Input data bytes to be converted.</param>
        /// <param name="index">Index number in data to begin converting for the integer value.</param>
        /// <returns>Resulting 32 bit conversion value</returns>
        public static Int32 ConvertToInt32(byte[] data, int index)
        {
            return (Int32)((UInt32)data[index] << 0 | (UInt32)data[index + 1] << 8 | (UInt32)data[index + 2] << 16 | (UInt32)data[index + 3] << 24);
        }
    }

    /// <summary>
    /// Defines the security level for storage of Cryptoki objects.
    /// </summary>
    public enum SecureStorageLevel
    {
        HighestAvailable = 0,
        TamperResistent,
        ServerPublicKeyEncrypted,
        Wrapped,
        PlainText,
    }

    /// <summary>
    /// Defines a Cryptoki attribute (or property) for a Cryptoki object.
    /// </summary>
    public class CryptokiAttribute
    {
        private const uint c_ArrayAttribute = 0x40000000;

        /// <summary>
        /// Defines a Cryptoki object class.
        /// </summary>
        public enum CryptokiType : uint
        {
            Class                    = 0x00000000, 
            Token                    = 0x00000001,
            Private                  = 0x00000002,
            Label                    = 0x00000003,
            Application              = 0x00000010,
            Value                    = 0x00000011,
            ObjectID                 = 0x00000012,
            CertificateType          = 0x00000080,
            Issuer                   = 0x00000081,
            SerialNumber             = 0x00000082,
            AC_Issuer                = 0x00000083,
            Owner                    = 0x00000084,
            AttrTypes                = 0x00000085,
            Trusted                  = 0x00000086,
            CertficateCategory       = 0x00000087,
            JavaMidpSecurityDomain   = 0x00000088,
            URL                      = 0x00000089,
            HashOfSubjectPublicKey   = 0x0000008A,
            HashOfIssuerPublicKey    = 0x0000008B,
            NameHashAlgorithm        = 0x0000008C,
            CheckValue               = 0x00000090,
            KeyType                  = 0x00000100,
            Subject                  = 0x00000101,
            ID                       = 0x00000102,
            Sensitive                = 0x00000103,
            Encrypt                  = 0x00000104,
            Decrypt                  = 0x00000105,
            Wrap                     = 0x00000106,
            Unwrap                   = 0x00000107,
            Sign                     = 0x00000108,
            SignRecover              = 0x00000109,
            Verify                   = 0x0000010A,
            VerifyRecover            = 0x0000010B,
            Derive                   = 0x0000010C,
            StartDate                = 0x00000110,
            EndDate                  = 0x00000111,
            Modulus                  = 0x00000120,
            ModulusBits              = 0x00000121,
            PublicExponent           = 0x00000122,
            PrivateExponent          = 0x00000123,
            Prime1                   = 0x00000124,
            Prime2                   = 0x00000125,
            Exponent1                = 0x00000126,
            Exponent2                = 0x00000127,
            Coefficent               = 0x00000128,
            Prime                    = 0x00000130,
            Subprime                 = 0x00000131,
            Base                     = 0x00000132,
            PrimeBits                = 0x00000133,
            SubprimeBits             = 0x00000134,
            ValueBits                = 0x00000160,
            ValueLen                 = 0x00000161,
            Extractable              = 0x00000162,
            Local                    = 0x00000163,
            NeverExtractable         = 0x00000164,
            AlwaysSensitive          = 0x00000165,
            KeyGenMecahnism          = 0x00000166,
            Modifiable               = 0x00000170,
            Copyable                 = 0x00000171,
            EcdsaParams              = 0x00000180,
            EcParams                 = 0x00000180,
            EcPoint                  = 0x00000181,
            SecondaryAuth            = 0x00000200, /* Deprecated */
            AuthPinFlags             = 0x00000201, /* Deprecated */
            AlwaysAuthenticate       = 0x00000202,
            WrapWithTrusted          = 0x00000210,
            WrapTemplate             = (c_ArrayAttribute|0x00000211),
            UnwrapTemplate           = (c_ArrayAttribute|0x00000212),
            HardwareFeatureType      = 0x00000300,
            ResetOnInit              = 0x00000301,
            HasReset                 = 0x00000302,
            PixelX                   = 0x00000400,
            PixelY                   = 0x00000401,
            Resolution               = 0x00000402,
            CharRows                 = 0x00000403,
            CharColumns              = 0x00000404,
            Color                    = 0x00000405,
            BitsPerPixel             = 0x00000406,
            CharSets                 = 0x00000480,
            EncodingMethods          = 0x00000481,
            MimeTypes                = 0x00000482,
            MechanismType            = 0x00000500,
            RequiredCmsAttributes    = 0x00000501,
            DefaultCmsAttributes     = 0x00000502,
            SupportedCmsAttributes   = 0x00000503,
            AllowedMechanisms        = (c_ArrayAttribute|0x00000600),
            VendorDefined            = 0x80000000,
            Persist                  = (VendorDefined | 0x00000001),
            Password                 = (VendorDefined | 0x00000002),
        }

        ///<summary>
        /// Creates a Cryptoki attribute from the specified Cryptoki type and the attribute value.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public CryptokiAttribute(CryptokiType type, byte[] value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// The type of the attribute.
        /// </summary>
        public CryptokiType Type;
        /// <summary>
        /// The value of the attribute.
        /// </summary>
        public byte[] Value;
    }
    
    /// <summary>
    /// Defines a Cryptoki object class.
    /// </summary>
    [Flags]
    public enum CryptokiClass : uint
    {
        DATA = 0,
        CERTIFICATE,
        PUBLIC_KEY,
        PRIVATE_KEY,
        SECRET_KEY,
        HW_FEATURE,
        DOMAIN_PARAMETERS,
        MECHANISM,
        OTP_KEY,

        VENDOR_DEFINED = 0x80000000,
    }

    /// <summary>
    /// Defines the cryptoki version.
    /// </summary>
    public class CryptokiVersion
    {
        public byte Major;
        public byte Minor;
    }

    /// <summary>
    /// Defines the topmost Cryptoki object for accessing the slots and tokens for the device.
    /// </summary>
    public static class Cryptoki
    {
        private static Slot[] m_slotList;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static extern Cryptoki();

        /// <summary>
        /// Gets the Cryptoki slot list for the device.
        /// </summary>
        public static Slot[] SlotList
        {
            get
            {
                if (m_slotList == null)
                {
                    m_slotList = GetSlotsInternal();
                }

                return m_slotList;
            }
        }

        /// <summary>
        /// Finds the slots that match all of the specified mechanism criteria.
        /// </summary>
        /// <param name="mechanisms">The mechanism search criteria.</param>
        /// <returns>The matching slots.</returns>
        public static Slot[] FindSlots(params Mechanism[] mechanisms) 
        {
            return FindSlots("", mechanisms);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static Slot[] GetSlotsInternal();

        /// <summary>
        /// Find the slots that match the specified service provider and mechanism criteria.
        /// </summary>
        /// <param name="serviceProviderName"></param>
        /// <param name="mechanisms"></param>
        /// <returns></returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static Slot[] FindSlots(string serviceProviderName, params Mechanism[] mechanisms);
    }
}