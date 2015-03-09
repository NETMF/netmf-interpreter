using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;
using System.Collections;

namespace Microsoft.SPOT.Platform.Tests
{

    public class CryptokiTests : IMFTestInterface
    {
        bool m_isEmulator;

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            // Add your functionality here.                
            try
            {
                m_isEmulator = (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3);
                InitMechs();
            }
            catch
            {
                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        [TestMethod]
        public MFTestResults CryptokiTest_SlotList()
        {
            // this test will throw an exception if the slot list or slot info is incorrect.
            MFTestResults res = MFTestResults.Pass;

            try
            {
                Slot[] list = Cryptoki.Cryptoki.SlotList;

                int cnt = list.Length;

                for (int i = 0; i < cnt; i++)
                {
                    Slot slot = list[i];
                    bool fDigestExists = false;

                    slot.SlotEvent += new Slot.OnSlotEvent(slot_SlotEvent);

                    Debug.Print("Slot " + i.ToString());
                    Debug.Print("  Desc: " + slot.Info.Description);
                    Debug.Print("  Manu: " + slot.Info.ManufactureID);
                    Debug.Print("  Flgs: " + slot.Info.Flags);
                    Debug.Print("  HVer: " + slot.Info.HardwareVersion.Major.ToString() + "." + list[i].Info.HardwareVersion.Minor.ToString());
                    Debug.Print("  FVer: " + slot.Info.FirmwareVersion.Major.ToString() + "." + list[i].Info.FirmwareVersion.Minor.ToString());

                    MechanismType[] mechs = slot.SupportedMechanisms;

                    Debug.Print("  Mechs: " + slot.Info.Description);
                    for (int j = 0; j < mechs.Length; j++)
                    {
                        Debug.Print("    " + GetMechString(mechs[j]));

                        if (mechs[j] == MechanismType.SHA256)
                            fDigestExists = true;
                    }

                    if(0 != (slot.Info.Flags & Slot.SlotFlag.TokenPresent))
                    {
                        TokenInfo info = new TokenInfo();

                        slot.GetTokenInfo(ref info);


                        Debug.Print("Token Info: ");
                        Debug.Print("  Labl  : " + info.Label);
                        Debug.Print("  Model : " + info.Model);
                        Debug.Print("  S/N   : " + info.SerialNumber);
                        Debug.Print("  Manu  : " + info.Manufacturer);
                        Debug.Print("  Flgs  : " + info.Flags);
                        Debug.Print("  PriMem: " + info.FreePrivateMemory);
                        Debug.Print("  PubMem: " + info.FreePublicMemory);
                        Debug.Print("  MaxPin: " + info.MaxPinLen);
                        Debug.Print("  MinPin: " + info.MinPinLen);
                        Debug.Print("  MaxRW : " + info.MaxRwSessionCount);
                        Debug.Print("  MaxRO : " + info.MaxSessionCount);
                        Debug.Print("  SesCnt: " + info.SessionCount);
                        Debug.Print("  TPrMem: " + info.TotalPrivateMemory);
                        Debug.Print("  TPuMem: " + info.TotalPublicMemory);
                        Debug.Print("  Utc   : " + info.UtcTime.ToString());
                        Debug.Print("  HVer  : " + info.HardwareVersion.Major.ToString() + "." + info.HardwareVersion.Minor.ToString());
                        Debug.Print("  FVer  : " + info.FirmwareVersion.Major.ToString() + "." + info.FirmwareVersion.Minor.ToString());

                        try
                        {
                            slot.InitializeToken("TokenPin", "label");
                        }
                        catch
                        {
                            Debug.Print("Warning: InitializeToken Failed");
                        }

                    }

                    using (Session sess = slot.OpenSession(Session.SessionFlag.ReadOnly))
                    {
                        if (fDigestExists)
                        {
                            HashAlgorithm sha = new HashAlgorithm(HashAlgorithmType.SHA256, sess);

                            byte[] data1 = sha.ComputeHash(System.Text.UTF8Encoding.UTF8.GetBytes("Hello World"));
                            byte[] data2 = sha.ComputeHash(System.Text.UTF8Encoding.UTF8.GetBytes("Hello World "));

                            bool success = false;
                            for (int k = 0; k < data1.Length; k++)
                            {
                                if (data1[k] != data2[k])
                                {
                                    success = true;
                                    break;
                                }
                            }
                            if (!success) res = MFTestResults.Fail;
                        }
                    }

                    slot.SlotEvent -= new Slot.OnSlotEvent(slot_SlotEvent);
                }
            }
            catch(Exception ex)
            {
                Log.Exception("Unexpected", ex);
                res = MFTestResults.Fail;
            }

            return res;
        }

        private bool SessionLoginTest(bool fromSlot)
        {
            bool res = true;
            Session sess;

            if (fromSlot)
            {
                Slot[] slots = Cryptoki.Cryptoki.SlotList;

                sess = slots[0].OpenSession(Session.SessionFlag.ReadWrite);
            }
            else
            {
                sess = new Session("");
            }

            try
            {
                sess.Login(Session.UserType.User, "MyPin");

                sess.Logout();
            }
            finally
            {
                sess.Dispose();
            }

            return res;
        }

        [TestMethod]
        public MFTestResults CryptokiTest_SessionLogin()
        {
            try
            {
                bool res = SessionLoginTest(true);

                res &= SessionLoginTest(false);

                return res ? MFTestResults.Pass : MFTestResults.Fail;
            }
            catch
            {
                return MFTestResults.Skip;
            }
        }

        private bool SessionInfoTest(bool fromSlot)
        {
            bool res = true;
            Session sess;

            if (fromSlot)
            {
                Slot[] slots = Cryptoki.Cryptoki.SlotList;

                sess = slots[0].OpenSession(Session.SessionFlag.ReadWrite);
            }
            else
            {
                sess = new Session("");
            }

            try
            {
                Session.SessionInfo info = new Session.SessionInfo();

                sess.GetSessionInfo(ref info);

                Debug.Print("Session Info:");
                Debug.Print("  ID   : " + info.SlotID);
                Debug.Print("  State: " + info.State);
                Debug.Print("  Flags: " + info.Flag);
                Debug.Print("  Error: " + info.DeviceError);
            }
            catch
            {
                res = false;
            }
            finally
            {
                sess.Dispose();
            }

            return res;
        }

        [TestMethod]
        public MFTestResults CryptokiTest_SessionInfo()
        {
            bool res = SessionInfoTest(true);

            res &= SessionInfoTest(false);

            return res ? MFTestResults.Pass : MFTestResults.Fail;
        }


        private bool SessionInitPINTest(bool fromSlot)
        {
            bool res = true;
            Session sess;

            if (fromSlot)
            {
                Slot[] slots = Cryptoki.Cryptoki.SlotList;

                sess = slots[0].OpenSession(Session.SessionFlag.ReadWrite);
            }
            else
            {
                sess = new Session("");
            }

            try
            {
                sess.InitializePin("InitPin");
            }
            finally
            {
                sess.Dispose();
            }

            return res;
        }

        [TestMethod]
        public MFTestResults CryptokiTest_InitPIN()
        {
            try
            {
                bool res = SessionInitPINTest(true);

                res &= SessionInitPINTest(false);

                return res ? MFTestResults.Pass : MFTestResults.Fail;
            }
            catch
            {
                return MFTestResults.Skip;
            }
        }


        private bool SessionSetPinTest(bool fromSlot)
        {
            bool res = true;
            Session sess;

            if (fromSlot)
            {
                Slot[] slots = Cryptoki.Cryptoki.SlotList;

                sess = slots[0].OpenSession(Session.SessionFlag.ReadWrite);
            }
            else
            {
                sess = new Session("");
            }

            try
            {
                sess.Login(Session.UserType.User, "MyPin");

                sess.SetPin("MyPin", "MyNewPin");

                sess.Logout();

                sess.Login(Session.UserType.User, "MyNewPin");

                sess.SetPin("MyNewPin", "MyPin");

                sess.Logout();
            }
            finally
            {
                sess.Dispose();
            }

            return res;
        }

        [TestMethod]
        public MFTestResults CryptokiTest_SessionSetPin()
        {
            try
            {
                bool res = SessionSetPinTest(true);

                res &= SessionSetPinTest(false);

                return res ? MFTestResults.Pass : MFTestResults.Fail;
            }
            catch
            {
                return MFTestResults.Skip;
            }
        }

        void slot_SlotEvent(Slot.SlotEventType evt)
        {
            if (evt == Slot.SlotEventType.Inserted)
            {
                Debug.Print("Inserted token");
            }
            else
            {
                Debug.Print("Removed token");
            }
        }

        static Hashtable s_MechLookup = new Hashtable();

        private string GetMechString(MechanismType mechType)
        {
            string str = (string)s_MechLookup[mechType];

            if (str == null)
            {
                str = "UNKNOWN";
            }
            return str;
        }

        private void InitMechs()
        {
            if (s_MechLookup.Count == 0)
            {
                s_MechLookup[(MechanismType)0x00000000] = "RSA_PKCS_KEY_PAIR_GEN                ";
                s_MechLookup[(MechanismType)0x00000001] = "RSA_PKCS                             ";
                s_MechLookup[(MechanismType)0x00000002] = "RSA_9796                             ";
                s_MechLookup[(MechanismType)0x00000003] = "RSA_X_509                            ";
                s_MechLookup[(MechanismType)0x00000004] = "MD2_RSA_PKCS                         ";
                s_MechLookup[(MechanismType)0x00000005] = "MD5_RSA_PKCS                         ";
                s_MechLookup[(MechanismType)0x00000006] = "SHA1_RSA_PKCS                        ";
                s_MechLookup[(MechanismType)0x00000007] = "RIPEMD128_RSA_PKCS                   ";
                s_MechLookup[(MechanismType)0x00000008] = "RIPEMD160_RSA_PKCS                   ";
                s_MechLookup[(MechanismType)0x00000009] = "RSA_PKCS_OAEP                        ";
                s_MechLookup[(MechanismType)0x0000000A] = "RSA_X9_31_KEY_PAIR_GEN               ";
                s_MechLookup[(MechanismType)0x0000000B] = "RSA_X9_31                            ";
                s_MechLookup[(MechanismType)0x0000000C] = "SHA1_RSA_X9_31                       ";
                s_MechLookup[(MechanismType)0x0000000D] = "RSA_PKCS_PSS                         ";
                s_MechLookup[(MechanismType)0x0000000E] = "SHA1_RSA_PKCS_PSS                    ";
                s_MechLookup[(MechanismType)0x00000010] = "DSA_KEY_PAIR_GEN                     ";
                s_MechLookup[(MechanismType)0x00000011] = "DSA                                  ";
                s_MechLookup[(MechanismType)0x00000012] = "DSA_SHA1                             ";
                s_MechLookup[(MechanismType)0x00000020] = "DH_PKCS_KEY_PAIR_GEN                 ";
                s_MechLookup[(MechanismType)0x00000021] = "DH_PKCS_DERIVE                       ";
                s_MechLookup[(MechanismType)0x00000030] = "X9_42_DH_KEY_PAIR_GEN                ";
                s_MechLookup[(MechanismType)0x00000031] = "X9_42_DH_DERIVE                      ";
                s_MechLookup[(MechanismType)0x00000032] = "X9_42_DH_HYBRID_DERIVE               ";
                s_MechLookup[(MechanismType)0x00000033] = "X9_42_MQV_DERIVE                     ";
                s_MechLookup[(MechanismType)0x00000040] = "SHA256_RSA_PKCS                      ";
                s_MechLookup[(MechanismType)0x00000041] = "SHA384_RSA_PKCS                      ";
                s_MechLookup[(MechanismType)0x00000042] = "SHA512_RSA_PKCS                      ";
                s_MechLookup[(MechanismType)0x00000043] = "SHA256_RSA_PKCS_PSS                  ";
                s_MechLookup[(MechanismType)0x00000044] = "SHA384_RSA_PKCS_PSS                  ";
                s_MechLookup[(MechanismType)0x00000045] = "SHA512_RSA_PKCS_PSS                  ";
                s_MechLookup[(MechanismType)0x00000046] = "SHA224_RSA_PKCS                      ";
                s_MechLookup[(MechanismType)0x00000047] = "SHA224_RSA_PKCS_PSS                  ";
                s_MechLookup[(MechanismType)0x00000100] = "RC2_KEY_GEN                          ";
                s_MechLookup[(MechanismType)0x00000101] = "RC2_ECB                              ";
                s_MechLookup[(MechanismType)0x00000102] = "RC2_CBC                              ";
                s_MechLookup[(MechanismType)0x00000103] = "RC2_MAC                              ";
                s_MechLookup[(MechanismType)0x00000104] = "RC2_MAC_GENERAL                      ";
                s_MechLookup[(MechanismType)0x00000105] = "RC2_CBC_PAD                          ";
                s_MechLookup[(MechanismType)0x00000110] = "RC4_KEY_GEN                          ";
                s_MechLookup[(MechanismType)0x00000111] = "RC4                                  ";
                s_MechLookup[(MechanismType)0x00000120] = "DES_KEY_GEN                          ";
                s_MechLookup[(MechanismType)0x00000121] = "DES_ECB                              ";
                s_MechLookup[(MechanismType)0x00000122] = "DES_CBC                              ";
                s_MechLookup[(MechanismType)0x00000123] = "DES_MAC                              ";
                s_MechLookup[(MechanismType)0x00000124] = "DES_MAC_GENERAL                      ";
                s_MechLookup[(MechanismType)0x00000125] = "DES_CBC_PAD                          ";
                s_MechLookup[(MechanismType)0x00000130] = "DES2_KEY_GEN                         ";
                s_MechLookup[(MechanismType)0x00000131] = "DES3_KEY_GEN                         ";
                s_MechLookup[(MechanismType)0x00000132] = "DES3_ECB                             ";
                s_MechLookup[(MechanismType)0x00000133] = "DES3_CBC                             ";
                s_MechLookup[(MechanismType)0x00000134] = "DES3_MAC                             ";
                s_MechLookup[(MechanismType)0x00000135] = "DES3_MAC_GENERAL                     ";
                s_MechLookup[(MechanismType)0x00000136] = "DES3_CBC_PAD                         ";
                s_MechLookup[(MechanismType)0x00000140] = "CDMF_KEY_GEN                         ";
                s_MechLookup[(MechanismType)0x00000141] = "CDMF_ECB                             ";
                s_MechLookup[(MechanismType)0x00000142] = "CDMF_CBC                             ";
                s_MechLookup[(MechanismType)0x00000143] = "CDMF_MAC                             ";
                s_MechLookup[(MechanismType)0x00000144] = "CDMF_MAC_GENERAL                     ";
                s_MechLookup[(MechanismType)0x00000145] = "CDMF_CBC_PAD                         ";
                s_MechLookup[(MechanismType)0x00000150] = "DES_OFB64                            ";
                s_MechLookup[(MechanismType)0x00000151] = "DES_OFB8                             ";
                s_MechLookup[(MechanismType)0x00000152] = "DES_CFB64                            ";
                s_MechLookup[(MechanismType)0x00000153] = "DES_CFB8                             ";
                s_MechLookup[(MechanismType)0x00000200] = "MD2                                  ";
                s_MechLookup[(MechanismType)0x00000201] = "MD2_HMAC                             ";
                s_MechLookup[(MechanismType)0x00000202] = "MD2_HMAC_GENERAL                     ";
                s_MechLookup[(MechanismType)0x00000210] = "MD5                                  ";
                s_MechLookup[(MechanismType)0x00000211] = "MD5_HMAC                             ";
                s_MechLookup[(MechanismType)0x00000212] = "MD5_HMAC_GENERAL                     ";
                s_MechLookup[(MechanismType)0x00000220] = "SHA_1                                ";
                s_MechLookup[(MechanismType)0x00000221] = "SHA_1_HMAC                           ";
                s_MechLookup[(MechanismType)0x00000222] = "SHA_1_HMAC_GENERAL                   ";
                s_MechLookup[(MechanismType)0x00000250] = "SHA256                               ";
                s_MechLookup[(MechanismType)0x00000251] = "SHA256_HMAC                          ";
                s_MechLookup[(MechanismType)0x00000252] = "SHA256_HMAC_GENERAL                  ";
                s_MechLookup[(MechanismType)0x00000255] = "SHA224                               ";
                s_MechLookup[(MechanismType)0x00000256] = "SHA224_HMAC                          ";
                s_MechLookup[(MechanismType)0x00000257] = "SHA224_HMAC_GENERAL                  ";
                s_MechLookup[(MechanismType)0x00000260] = "SHA384                               ";
                s_MechLookup[(MechanismType)0x00000261] = "SHA384_HMAC                          ";
                s_MechLookup[(MechanismType)0x00000262] = "SHA384_HMAC_GENERAL                  ";
                s_MechLookup[(MechanismType)0x00000270] = "SHA512                               ";
                s_MechLookup[(MechanismType)0x00000271] = "SHA512_HMAC                          ";
                s_MechLookup[(MechanismType)0x00000272] = "SHA512_HMAC_GENERAL                  ";
                s_MechLookup[(MechanismType)0x00000370] = "SSL3_PRE_MASTER_KEY_GEN              ";
                s_MechLookup[(MechanismType)0x00000371] = "SSL3_MASTER_KEY_DERIVE               ";
                s_MechLookup[(MechanismType)0x00000372] = "SSL3_KEY_AND_MAC_DERIVE              ";
                s_MechLookup[(MechanismType)0x00000373] = "SSL3_MASTER_KEY_DERIVE_DH            ";
                s_MechLookup[(MechanismType)0x00000374] = "TLS_PRE_MASTER_KEY_GEN               ";
                s_MechLookup[(MechanismType)0x00000375] = "TLS_MASTER_KEY_DERIVE                ";
                s_MechLookup[(MechanismType)0x00000376] = "TLS_KEY_AND_MAC_DERIVE               ";
                s_MechLookup[(MechanismType)0x00000377] = "TLS_MASTER_KEY_DERIVE_DH             ";
                s_MechLookup[(MechanismType)0x00000378] = "TLS_PRF                              ";
                s_MechLookup[(MechanismType)0x00000380] = "SSL3_MD5_MAC                         ";
                s_MechLookup[(MechanismType)0x00000381] = "SSL3_SHA1_MAC                        ";
                s_MechLookup[(MechanismType)0x00000390] = "MD5_KEY_DERIVATION                   ";
                s_MechLookup[(MechanismType)0x00000391] = "MD2_KEY_DERIVATION                   ";
                s_MechLookup[(MechanismType)0x00000392] = "SHA1_KEY_DERIVATION                  ";
                s_MechLookup[(MechanismType)0x00000393] = "SHA256_KEY_DERIVATION                ";
                s_MechLookup[(MechanismType)0x00000394] = "SHA384_KEY_DERIVATION                ";
                s_MechLookup[(MechanismType)0x00000395] = "SHA512_KEY_DERIVATION                ";
                s_MechLookup[(MechanismType)0x00000396] = "SHA224_KEY_DERIVATION                ";
                s_MechLookup[(MechanismType)0x000003D0] = "WTLS_PRE_MASTER_KEY_GEN              ";
                s_MechLookup[(MechanismType)0x000003D1] = "WTLS_MASTER_KEY_DERIVE               ";
                s_MechLookup[(MechanismType)0x000003D2] = "WTLS_MASTER_KEY_DERIVE_DH_ECC        ";
                s_MechLookup[(MechanismType)0x000003D3] = "WTLS_PRF                             ";
                s_MechLookup[(MechanismType)0x000003D4] = "WTLS_SERVER_KEY_AND_MAC_DERIVE       ";
                s_MechLookup[(MechanismType)0x000003D5] = "WTLS_CLIENT_KEY_AND_MAC_DERIVE       ";
                s_MechLookup[(MechanismType)0x00001040] = "ECDSA_KEY_PAIR_GEN                   ";
                s_MechLookup[(MechanismType)0x00001040] = "EC_KEY_PAIR_GEN                      ";
                s_MechLookup[(MechanismType)0x00001041] = "ECDSA                                ";
                s_MechLookup[(MechanismType)0x00001042] = "ECDSA_SHA1                           ";
                s_MechLookup[(MechanismType)0x00001050] = "ECDH1_DERIVE                         ";
                s_MechLookup[(MechanismType)0x00001051] = "ECDH1_COFACTOR_DERIVE                ";
                s_MechLookup[(MechanismType)0x00001052] = "ECMQV_DERIVE                         ";
                s_MechLookup[(MechanismType)0x00001080] = "AES_KEY_GEN                          ";
                s_MechLookup[(MechanismType)0x00001081] = "AES_ECB                              ";
                s_MechLookup[(MechanismType)0x00001082] = "AES_CBC                              ";
                s_MechLookup[(MechanismType)0x00001083] = "AES_MAC                              ";
                s_MechLookup[(MechanismType)0x00001084] = "AES_MAC_GENERAL                      ";
                s_MechLookup[(MechanismType)0x00001085] = "AES_CBC_PAD                          ";
                s_MechLookup[(MechanismType)0x00001086] = "AES_CTR                              ";
                s_MechLookup[(MechanismType)0x00001087] = "AES_GCM                              ";
                s_MechLookup[(MechanismType)0x00001088] = "AES_CCM                              ";
                s_MechLookup[(MechanismType)0x00001089] = "AES_CTS                              ";
                s_MechLookup[(MechanismType)0x00001089] = "AES_CMAC_GENERAL                     ";
                s_MechLookup[(MechanismType)0x0000108A] = "AES_CMAC                             ";
                s_MechLookup[(MechanismType)0x00002104] = "AES_OFB                              ";
                s_MechLookup[(MechanismType)0x00002105] = "AES_CFB64                            ";
                s_MechLookup[(MechanismType)0x00002106] = "AES_CFB8                             ";
                s_MechLookup[(MechanismType)0x00002107] = "AES_CFB128                           ";
                s_MechLookup[(MechanismType)0x00001100] = "DES_ECB_ENCRYPT_DATA                 ";
                s_MechLookup[(MechanismType)0x00001101] = "DES_CBC_ENCRYPT_DATA                 ";
                s_MechLookup[(MechanismType)0x00001102] = "DES3_ECB_ENCRYPT_DATA                ";
                s_MechLookup[(MechanismType)0x00001103] = "DES3_CBC_ENCRYPT_DATA                ";
                s_MechLookup[(MechanismType)0x00001104] = "AES_ECB_ENCRYPT_DATA                 ";
                s_MechLookup[(MechanismType)0x00001105] = "AES_CBC_ENCRYPT_DATA                 ";
                s_MechLookup[(MechanismType)0x00002000] = "DSA_PARAMETER_GEN                    ";
                s_MechLookup[(MechanismType)0x00002001] = "DH_PKCS_PARAMETER_GEN                ";
                s_MechLookup[(MechanismType)0x00002002] = "X9_42_DH_PARAMETER_GEN               ";
                s_MechLookup[(MechanismType)0x00000350] = "GENERIC_SECRET_KEY_GEN               ";
                s_MechLookup[(MechanismType)0x80000000] = "VENDOR_DEFINED                       ";
                s_MechLookup[MechanismType.VENDOR_DEFINED | (MechanismType)0x00000001] = "VENDOR_RNG                           ";
            }
        }

    }
}