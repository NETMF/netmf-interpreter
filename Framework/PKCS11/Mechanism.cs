using System.Runtime.CompilerServices;
using System;
using System.Security.Cryptography;

namespace Microsoft.SPOT.Cryptoki
{
    /// <summary>
    /// Enumeration of most of the Cryptoki mechanisms.
    /// </summary>
    [Flags]
    public enum MechanismType : uint
    {
        RSA_PKCS_KEY_PAIR_GEN = 0x00000000,
        RSA_PKCS = 0x00000001,
        RSA_9796 = 0x00000002,
        RSA_X_509 = 0x00000003,

        MD2_RSA_PKCS = 0x00000004,
        MD5_RSA_PKCS = 0x00000005,
        SHA1_RSA_PKCS = 0x00000006,

        RIPEMD128_RSA_PKCS = 0x00000007,
        RIPEMD160_RSA_PKCS = 0x00000008,
        RSA_PKCS_OAEP = 0x00000009,

        RSA_X9_31_KEY_PAIR_GEN = 0x0000000A,
        RSA_X9_31 = 0x0000000B,
        SHA1_RSA_X9_31 = 0x0000000C,
        RSA_PKCS_PSS = 0x0000000D,
        SHA1_RSA_PKCS_PSS = 0x0000000E,

        DSA_KEY_PAIR_GEN = 0x00000010,
        DSA = 0x00000011,
        DSA_SHA1 = 0x00000012,
        DH_PKCS_KEY_PAIR_GEN = 0x00000020,
        DH_PKCS_DERIVE = 0x00000021,

        X9_42_DH_KEY_PAIR_GEN = 0x00000030,
        X9_42_DH_DERIVE = 0x00000031,
        X9_42_DH_HYBRID_DERIVE = 0x00000032,
        X9_42_MQV_DERIVE = 0x00000033,

        SHA256_RSA_PKCS = 0x00000040,
        SHA384_RSA_PKCS = 0x00000041,
        SHA512_RSA_PKCS = 0x00000042,
        SHA256_RSA_PKCS_PSS = 0x00000043,
        SHA384_RSA_PKCS_PSS = 0x00000044,
        SHA512_RSA_PKCS_PSS = 0x00000045,

        SHA224_RSA_PKCS = 0x00000046,
        SHA224_RSA_PKCS_PSS = 0x00000047,

        RC2_KEY_GEN = 0x00000100,
        RC2_ECB = 0x00000101,
        RC2_CBC = 0x00000102,
        RC2_MAC = 0x00000103,

        RC2_MAC_GENERAL = 0x00000104,
        RC2_CBC_PAD = 0x00000105,

        RC4_KEY_GEN = 0x00000110,
        RC4 = 0x00000111,
        DES_KEY_GEN = 0x00000120,
        DES_ECB = 0x00000121,
        DES_CBC = 0x00000122,
        DES_MAC = 0x00000123,

        DES_MAC_GENERAL = 0x00000124,
        DES_CBC_PAD = 0x00000125,

        DES2_KEY_GEN = 0x00000130,
        DES3_KEY_GEN = 0x00000131,
        DES3_ECB = 0x00000132,
        DES3_CBC = 0x00000133,
        DES3_MAC = 0x00000134,

        DES3_MAC_GENERAL = 0x00000135,
        DES3_CBC_PAD = 0x00000136,
        CDMF_KEY_GEN = 0x00000140,
        CDMF_ECB = 0x00000141,
        CDMF_CBC = 0x00000142,
        CDMF_MAC = 0x00000143,
        CDMF_MAC_GENERAL = 0x00000144,
        CDMF_CBC_PAD = 0x00000145,

        DES_OFB64 = 0x00000150,
        DES_OFB8 = 0x00000151,
        DES_CFB64 = 0x00000152,
        DES_CFB8 = 0x00000153,

        MD2 = 0x00000200,

        MD2_HMAC = 0x00000201,
        MD2_HMAC_GENERAL = 0x00000202,

        MD5 = 0x00000210,

        MD5_HMAC = 0x00000211,
        MD5_HMAC_GENERAL = 0x00000212,

        SHA_1 = 0x00000220,

        SHA_1_HMAC = 0x00000221,
        SHA_1_HMAC_GENERAL = 0x00000222,

        SHA256 = 0x00000250,
        SHA256_HMAC = 0x00000251,
        SHA256_HMAC_GENERAL = 0x00000252,

        SHA224 = 0x00000255,
        SHA224_HMAC = 0x00000256,
        SHA224_HMAC_GENERAL = 0x00000257,

        SHA384 = 0x00000260,
        SHA384_HMAC = 0x00000261,
        SHA384_HMAC_GENERAL = 0x00000262,
        SHA512 = 0x00000270,
        SHA512_HMAC = 0x00000271,
        SHA512_HMAC_GENERAL = 0x00000272,

        RIPEMD160               = 0x00000240,
        RIPEMD160_HMAC          = 0x00000241,

        SSL3_PRE_MASTER_KEY_GEN = 0x00000370,
        SSL3_MASTER_KEY_DERIVE = 0x00000371,
        SSL3_KEY_AND_MAC_DERIVE = 0x00000372,

        SSL3_MASTER_KEY_DERIVE_DH = 0x00000373,
        TLS_PRE_MASTER_KEY_GEN = 0x00000374,
        TLS_MASTER_KEY_DERIVE = 0x00000375,
        TLS_KEY_AND_MAC_DERIVE = 0x00000376,
        TLS_MASTER_KEY_DERIVE_DH = 0x00000377,

        TLS_PRF = 0x00000378,

        SSL3_MD5_MAC = 0x00000380,
        SSL3_SHA1_MAC = 0x00000381,
        MD5_KEY_DERIVATION = 0x00000390,
        MD2_KEY_DERIVATION = 0x00000391,
        SHA1_KEY_DERIVATION = 0x00000392,

        SHA256_KEY_DERIVATION = 0x00000393,
        SHA384_KEY_DERIVATION = 0x00000394,
        SHA512_KEY_DERIVATION = 0x00000395,

        SHA224_KEY_DERIVATION = 0x00000396,

        WTLS_PRE_MASTER_KEY_GEN = 0x000003D0,
        WTLS_MASTER_KEY_DERIVE = 0x000003D1,
        WTLS_MASTER_KEY_DERIVE_DH_ECC = 0x000003D2,
        WTLS_PRF = 0x000003D3,
        WTLS_SERVER_KEY_AND_MAC_DERIVE = 0x000003D4,
        WTLS_CLIENT_KEY_AND_MAC_DERIVE = 0x000003D5,

        ECDSA_KEY_PAIR_GEN = 0x00001040,
        EC_KEY_PAIR_GEN = 0x00001040,

        ECDSA = 0x00001041,
        ECDSA_SHA1 = 0x00001042,

        ECDH1_DERIVE = 0x00001050,
        ECDH1_COFACTOR_DERIVE = 0x00001051,
        ECMQV_DERIVE = 0x00001052,

        AES_KEY_GEN = 0x00001080,
        AES_ECB = 0x00001081,
        AES_CBC = 0x00001082,
        AES_MAC = 0x00001083,
        AES_MAC_GENERAL = 0x00001084,
        AES_CBC_PAD = 0x00001085,
        AES_CTR = 0x00001086,
        AES_GCM = 0x00001087,
        AES_CCM = 0x00001088,
        AES_CTS = 0x00001089,
        AES_CMAC_GENERAL = 0x00001089,
        AES_CMAC = 0x0000108A,
        AES_OFB = 0x00002104,
        AES_CFB64 = 0x00002105,
        AES_CFB8 = 0x00002106,
        AES_CFB128 = 0x00002107,

        DES_ECB_ENCRYPT_DATA = 0x00001100,
        DES_CBC_ENCRYPT_DATA = 0x00001101,
        DES3_ECB_ENCRYPT_DATA = 0x00001102,
        DES3_CBC_ENCRYPT_DATA = 0x00001103,
        AES_ECB_ENCRYPT_DATA = 0x00001104,
        AES_CBC_ENCRYPT_DATA = 0x00001105,

        DSA_PARAMETER_GEN = 0x00002000,
        DH_PKCS_PARAMETER_GEN = 0x00002001,
        X9_42_DH_PARAMETER_GEN = 0x00002002,

        GENERIC_SECRET_KEY_GEN = 0x00000350,

        VENDOR_DEFINED = 0x80000000,
        SIGN_NO_NODIGEST_FLAG = 0x40000000,

        VENDOR_RNG   = (VENDOR_DEFINED | 0x00000001),
        ECDH_KEY_PAIR_GEN = (VENDOR_DEFINED | 0x00000002),
        AES_ECB_PAD = (VENDOR_DEFINED | 0x00000003),
        NULL_KEY_DERIVATION = (VENDOR_DEFINED | 0x00000004),
    }

    /// <summary>
    /// The Mechanism class for defining the crypto algorithm and parameters.
    /// </summary>
    public class Mechanism
    {
        /// <summary>
        /// Creates a Mechanism object with given mechanism type or algorithm
        /// </summary>
        /// <param name="type">The mechanism type.</param>
        public Mechanism(MechanismType type)
        {
            Type = type;
        }

        /// <summary>
        /// Creates a Mechansim object with specified mechanism type and parameter bytes.
        /// </summary>
        /// <param name="type">The mechanism type.</param>
        /// <param name="parameter">The parameter data for the mechanism.</param>
        public Mechanism(MechanismType type, byte[] parameter)
        {
            Type = type;
            Parameter = parameter;
        }

        public MechanismType Type;
        public byte[] Parameter;
    }

    /// <summary>
    /// Defines the properties of a particular Cryptoki mechanism.
    /// </summary>
    public class MechanismInfo
    {
        /// <summary>
        /// Cryptoki mechanism flags.
        /// </summary>
        [FlagsAttribute]
        public enum MechanismInfoFlags : ulong
        {
            Hardware        = 0x00000001,
            Encrypt         = 0x00000100,
            Decrypt         = 0x00000200,
            Digest          = 0x00000400,
            Sign            = 0x00000800,
            SignRecover     = 0x00001000,
            Verify          = 0x00002000,
            VerifyRecover   = 0x00004000,
            Generate        = 0x00008000,
            GenerateKeyPair = 0x00010000,
            Wrap            = 0x00020000,
            Unwrap          = 0x00040000,
            Derive          = 0x00080000,
            Extension       = 0x80000000,
        };

        /// <summary>
        /// Defines the minimum key size supported by the mechansim.
        /// </summary>
        public ulong MinKeySize;
        /// <summary>
        /// Defines the maximum key size supported by the mechansim.
        /// </summary>
        public ulong MaxKeySize;
        /// <summary>
        /// Defines the properties of the mechanism.
        /// </summary>
        public MechanismInfoFlags Flags;
    }
}
