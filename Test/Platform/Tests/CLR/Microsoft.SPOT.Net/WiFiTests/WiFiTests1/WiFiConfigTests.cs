////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Net;
using Microsoft.SPOT;
using Microsoft.SPOT.Net;
using Microsoft.SPOT.Net.NetworkInformation;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    /// <summary>
    /// NOTE: WiFiTests1 must be run prior to running WiFiTests2.
    /// WiFiTests1 test setting the wireless properties. 
    /// </summary>
    public class WiFiTests1 : IMFTestInterface
    {
        private NetworkInterface[] m_interfaces;
        private Wireless80211 m_wireless;
        private string[] m_authenticationType = 
            new string[]{ "None", "EAP", "PEAP", "WCN", "Open", "Shared" };
        private string[] m_encryptionType =
            new string[] { "None", "WEP", "WPA", "WPAPSK", "Certificate" };
        private const int MAX_NK_LENGTH = 256;
        private const int MAX_REKEY_LENGTH = 32;

        [SetUp]
        public InitializeResult Initialize()
        {            
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
            {
                Log.Comment("Wireless tests run only on Device");
                return InitializeResult.Skip;
            }

            Log.Comment("Getting all the network interfaces.");
            m_interfaces = NetworkInterface.GetAllNetworkInterfaces();
            m_wireless = this.FindWireless80211();

            // if the device has no wireless configs, there is nothing we can do
            if(m_wireless == null)
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
        public MFTestResults AuthenticationType()
        {
            try
            {
                // Set.
                Log.Comment("**********************************************");
                Log.Comment("************AuthenticationType=None************");
                Log.Comment("**********************************************");
                SetAuthenticationType(Wireless80211.AuthenticationType.None);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyAuthenticationType(Wireless80211.AuthenticationType.None))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**********************************************");
                Log.Comment("************AuthenticationType=EAP************");
                Log.Comment("**********************************************");
                SetAuthenticationType(Wireless80211.AuthenticationType.EAP);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyAuthenticationType(Wireless80211.AuthenticationType.EAP))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**********************************************");
                Log.Comment("************AuthenticationType=WCN************");
                Log.Comment("**********************************************");
                SetAuthenticationType(Wireless80211.AuthenticationType.WCN);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyAuthenticationType(Wireless80211.AuthenticationType.WCN))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**********************************************");
                Log.Comment("************AuthenticationType=PEAP************");
                Log.Comment("**********************************************");
                SetAuthenticationType(Wireless80211.AuthenticationType.PEAP);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyAuthenticationType(Wireless80211.AuthenticationType.PEAP))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**********************************************");
                Log.Comment("************AuthenticationType=Open************");
                Log.Comment("**********************************************");
                SetAuthenticationType(Wireless80211.AuthenticationType.Open);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyAuthenticationType(Wireless80211.AuthenticationType.Open))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**********************************************");
                Log.Comment("************AuthenticationType=Shared************");
                Log.Comment("**********************************************");
                SetAuthenticationType(Wireless80211.AuthenticationType.Shared);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyAuthenticationType(Wireless80211.AuthenticationType.Shared))
                {
                    return MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("An exception was thrown: " + ex.Message);
                return MFTestResults.Fail;
            }

            Log.Comment("Successfully saved/retrieved the authentication property");
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults EncryptionType()
        {
            try
            {
                // Set.
                Log.Comment("**************************************************");
                Log.Comment("***************EncryptionType=None****************");
                Log.Comment("**************************************************");
                SetEncryptionType(Wireless80211.EncryptionType.None);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyEncryptionType(Wireless80211.EncryptionType.None))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**************************************************");
                Log.Comment("****************EncryptionType=WPAPSK****************");
                Log.Comment("**************************************************");
                SetEncryptionType(Wireless80211.EncryptionType.WPAPSK);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyEncryptionType(Wireless80211.EncryptionType.WPAPSK))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**************************************************");
                Log.Comment("****************EncryptionType=WPA****************");
                Log.Comment("**************************************************");
                SetEncryptionType(Wireless80211.EncryptionType.WPA);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyEncryptionType(Wireless80211.EncryptionType.WPA))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**************************************************");
                Log.Comment("***************EncryptionType=WEP**************");
                Log.Comment("**************************************************");
                SetEncryptionType(Wireless80211.EncryptionType.WEP);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyEncryptionType(Wireless80211.EncryptionType.WEP))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**************************************************");
                Log.Comment("************EncryptionType=Certificate************");
                Log.Comment("**************************************************");
                SetEncryptionType(Wireless80211.EncryptionType.Certificate);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyEncryptionType(Wireless80211.EncryptionType.Certificate))
                {
                    return MFTestResults.Fail;
                }

            }
            catch (Exception ex)
            {
                Log.Comment("An exception was thrown: " + ex.Message);
                return MFTestResults.Fail;
            }

            Log.Comment("Successfully saved/retrieved the encryption property");
            return MFTestResults.Pass;
        }

        public MFTestResults RadioType()
        {
            try
            {
                // Set.
                Log.Comment("**************************************************");
                Log.Comment("*******************RadioType=a********************");
                Log.Comment("**************************************************");
                SetRadioType(Wireless80211.RadioType.a);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyRadioType(Wireless80211.RadioType.a))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**************************************************");
                Log.Comment("*******************RadioType=b********************");
                Log.Comment("**************************************************");
                SetRadioType(Wireless80211.RadioType.b);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyRadioType(Wireless80211.RadioType.b))
                {
                    return MFTestResults.Fail;
                }

                Log.Comment("**************************************************");
                Log.Comment("*******************RadioType=n********************");
                Log.Comment("**************************************************");
                SetRadioType(Wireless80211.RadioType.n);


                // Verify.
                if (MFTestResults.Fail ==
                    VerifyRadioType(Wireless80211.RadioType.n))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**************************************************");
                Log.Comment("*******************RadioType=g********************");
                Log.Comment("**************************************************");
                SetRadioType(Wireless80211.RadioType.g);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyRadioType(Wireless80211.RadioType.g))
                {
                    return MFTestResults.Fail;
                }

                // Set.
                Log.Comment("**************************************************");
                Log.Comment("*******************RadioType=bgn********************");
                Log.Comment("**************************************************");
                SetRadioType(Wireless80211.RadioType.g | Wireless80211.RadioType.b | Wireless80211.RadioType.n);

                // Verify.
                if (MFTestResults.Fail ==
                    VerifyRadioType(Wireless80211.RadioType.g | Wireless80211.RadioType.b | Wireless80211.RadioType.n))
                {
                    return MFTestResults.Fail;
                }

            }
            catch (Exception ex)
            {
                Log.Comment("An exception was thrown: " + ex.Message);
                return MFTestResults.Fail;
            }

            Log.Comment("Successfully saved/retrieved the radio property");
            return MFTestResults.Pass;
        }

        public MFTestResults PassPhrase()
        {
            int MAX_PASSPHRASE_LENGTH = 63;

            char[] specialChars = { '~', '/', '`', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', 
                                      '-', '+', '=', '{','[','}',']', '|', '\\', ':', ';', '"', '\'', 
                                      '<', ',', '>', '.', '?', '/' };
            string[] testStrings = {                   
                                       " ",                    
                                       MFUtilities.GetRandomSafeString(1),
                                       MFUtilities.GetRandomSafeString(MAX_PASSPHRASE_LENGTH),
                                       MFUtilities.GetRandomString(1),                                       
                                       MFUtilities.GetRandomString(MAX_PASSPHRASE_LENGTH),
                                       MFUtilities.GetRandomHighByteString(1),
                                       MFUtilities.GetRandomHighByteString(8),
                                       "?˜???",
                                       "??????",
                                       "0x0001fd94",
                                       new string(specialChars),
                                   };
            string[] invalidStrings = {
                                          MFUtilities.GetRandomSafeString(MAX_PASSPHRASE_LENGTH + 1),
                                          MFUtilities.GetRandomString(MAX_PASSPHRASE_LENGTH + 1), 
                                          MFUtilities.GetRandomString(MAX_PASSPHRASE_LENGTH + 1 + new Random().Next(100)), 
                                      };

            foreach (string testString in testStrings)
            {
                try
                {
                    SetPassPhrase(testString);

                    // Verify.
                    if (MFTestResults.Fail == VerifyPassPhrase(testString))
                    {
                        return MFTestResults.Fail;
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(ex.Message);
                    return MFTestResults.Fail;
                }
            }

            foreach (string invalidString in invalidStrings)
            {
                try
                {
                    SetPassPhrase(invalidString);
                    Log.Comment("Setting pass phrase to " + invalidString + " succeeded. " +
                        " Expected an exception to be thrown. Returning a failure.");
                    return MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    Log.Comment("Test passed - A valid exception was thrown: " + ex.Message);
                }
            }

            SetPassPhrase("PASSPHRASE");

            return MFTestResults.Pass;
        }

        public MFTestResults NetworkKey()
        {            
            bool result = true;

            int[] validLengths = new int[] { 8, 16, 32, 64, 128, 256 }; // 64-bit -> 2048-bit

            // Valid cases.                
            foreach (int len in validLengths)
            {
                if (MFTestResults.Fail == TestNetworkKey(GetByteArray(len)))
                {
                    result &= false;
                }
            }

            // Invalid cases.            

            foreach (int len in validLengths)
            {
                if (MFTestResults.Pass == TestNetworkKey(GetByteArray(len-1)))
                {
                    result &= false;
                }
            }
            
            if (MFTestResults.Pass == TestNetworkKey(GetByteArray(MAX_NK_LENGTH + 1)))
            {
                result &= false;
            }

            SetNetworkKey(new byte[] {});

            // Return test result.
            return result ? MFTestResults.Pass : MFTestResults.Fail;
        }

        public MFTestResults ReKeyInternal()
        {
            bool result = true;

            // Valid cases.                
            if (MFTestResults.Fail == TestReKeyInternal(GetByteArray(1)))
            {
                result &= false;
            }

            if (MFTestResults.Fail == TestReKeyInternal(GetByteArray(new Random().Next(MAX_REKEY_LENGTH))))
            {
                result &= false;
            }
            if (MFTestResults.Fail == TestReKeyInternal(GetByteArray(MAX_REKEY_LENGTH)))
            {
                result &= false;
            }

            // Invalid cases.            
            if (MFTestResults.Pass == TestReKeyInternal(GetByteArray(MAX_REKEY_LENGTH + 1)))
            {
                result &= false;
            }

            SetReKeyInternal(new byte[] { });

            // Return test result.
            return result ? MFTestResults.Pass : MFTestResults.Fail;
        }

        public MFTestResults Ssid()
        {
            int MAX_SSID_LENGTH = 31;
            
            char[] specialChars = { '~', '/', '`', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', 
                                      '-', '+', '=', '{','[','}',']', '|', '\\', ':', ';', '"', '\'', };

            char[] specialChars2 = { '<', ',', '>', '.', '?', '/' };

            string[] testStrings = {                   
                                       " ",                    
                                       MFUtilities.GetRandomSafeString(1),
                                       MFUtilities.GetRandomSafeString(MAX_SSID_LENGTH),
                                       MFUtilities.GetRandomString(1),                                       
                                       MFUtilities.GetRandomString(MAX_SSID_LENGTH),
                                       MFUtilities.GetRandomHighByteString(1),
                                       MFUtilities.GetRandomHighByteString(8),
                                       "?˜???",
                                       "??????",
                                       "0x0001fd94",
                                       new string(specialChars),
                                       new string(specialChars2),
                                   };
            string[] invalidStrings = {
                                          MFUtilities.GetRandomSafeString(MAX_SSID_LENGTH + 1),
                                          MFUtilities.GetRandomString(MAX_SSID_LENGTH + 1), 
                                          MFUtilities.GetRandomString(MAX_SSID_LENGTH + 1 + new Random().Next(100)), 
                                      };

            foreach (string testString in testStrings)
            {
                try
                {
                    SetSsid(testString);

                    // Verify.
                    if (MFTestResults.Fail == VerifySsid(testString))
                    {
                        return MFTestResults.Fail;
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(ex.Message);
                    return MFTestResults.Fail;
                }
            }

            foreach (string invalidString in invalidStrings)
            {
                try
                {
                    SetSsid(invalidString);
                    Log.Comment("Setting ssid to " + invalidString + " succeeded. " +
                        " Expected an exception to be thrown. Returning a failure.");
                    return MFTestResults.Fail;
                }
                catch (Exception ex)
                {
                    Log.Comment("Test passed - A valid exception was thrown: " + ex.Message);
                }
            }

            SetSsid("SSID_1");

            // Return test result.
            return MFTestResults.Pass;
        }

        #region Private Methods

        private void SetAuthenticationType(Wireless80211.AuthenticationType authType)
        {
            Log.Comment("Setting authentication type to " + m_authenticationType[(int)authType]);
            m_wireless.Authentication = authType;
            Log.Comment("Calling SaveConfiguration to save the authentication type");
            Wireless80211.SaveConfiguration(new Wireless80211[] { m_wireless }, true);
        }

        private void SetEncryptionType(Wireless80211.EncryptionType encryptionType)
        {
            Log.Comment("Setting encryption type to " + m_encryptionType[(int)encryptionType]);
            m_wireless.Encryption = encryptionType;
            Log.Comment("Calling SaveConfiguration to save the encryption type");
            Wireless80211.SaveConfiguration(new Wireless80211[] { m_wireless }, true);
        }

        private string RadioTypeToString(Wireless80211.RadioType radioType)
        {
            string type = "";

            if (0 != (radioType & Wireless80211.RadioType.a)) type += "A";
            if (0 != (radioType & Wireless80211.RadioType.b)) type += "B";
            if (0 != (radioType & Wireless80211.RadioType.g)) type += "G";
            if (0 != (radioType & Wireless80211.RadioType.n)) type += "N";

            return type;
        }

        private void SetRadioType(Wireless80211.RadioType radioType)
        {

            Log.Comment("Setting radio type to " + RadioTypeToString(radioType));
            m_wireless.Radio = radioType;
            Log.Comment("Calling SaveConfiguration to save the radio type");
            Wireless80211.SaveConfiguration(new Wireless80211[] { m_wireless }, true);
        }

        private void SetPassPhrase(string passPhrase)
        {
            if (null != passPhrase)
            {
                Log.Comment("Setting Pass Phrase = " + passPhrase);                
            }
            else
            {
                Log.Comment("Setting Pass Phrase = null");
            }
            m_wireless.PassPhrase = passPhrase;
            Log.Comment("Calling SaveConfiguration to save the pass phrase");
            Wireless80211.SaveConfiguration(new Wireless80211[] { m_wireless }, false);
        }

        private void SetNetworkKey(byte[] key)
        {
            Log.Comment("Setting network key(byte array) = " + ByteArrayPrinter(key));
            m_wireless.NetworkKey = key;
            Log.Comment("Calling SaveConfiguration to save the network key");
            Wireless80211.SaveConfiguration(new Wireless80211[] { m_wireless }, false);
        }

        private void SetReKeyInternal(byte[] key)
        {
            Log.Comment("Setting rekey internal to " + ByteArrayPrinter(key));
            m_wireless.ReKeyInternal = key;
            Log.Comment("Calling SaveConfiguration to save rekeyinternal");
            Wireless80211.SaveConfiguration(new Wireless80211[] { m_wireless }, false);
        }

        private void SetSsid(string key)
        {
            Log.Comment("Setting the Ssid to " + key);
            m_wireless.Ssid = key;
            Log.Comment("Calling SaveConfiguration to save the Ssid");
            Wireless80211.SaveConfiguration(new Wireless80211[] { m_wireless }, false);
        }

        private MFTestResults VerifyAuthenticationType(Wireless80211.AuthenticationType authType)
        {
            m_wireless = FindWireless80211();
            Log.Comment("Verifying authentication type is set to " + m_authenticationType[(int)authType]);
            Wireless80211.AuthenticationType currentValue = m_wireless.Authentication;
            Log.Comment("Authentication type we get is " + m_authenticationType[(int)currentValue]);
            if (currentValue == authType)
            {
                Log.Comment("Returning a pass since " + m_authenticationType[(int)authType]
                    + " == " + m_authenticationType[(int)currentValue]);
                return MFTestResults.Pass;
            }

            Log.Comment("Returning a fail since " + m_authenticationType[(int)authType]
                + " != " + m_authenticationType[(int)currentValue]);
            return MFTestResults.Fail;
        }

        private MFTestResults VerifyEncryptionType(Wireless80211.EncryptionType encryptionType)
        {
            m_wireless = FindWireless80211();
            Log.Comment("Verifying encryption type is set to " + m_encryptionType[(int)encryptionType]);
            Wireless80211.EncryptionType currentValue = m_wireless.Encryption;
            Log.Comment("Encryption type we get is " + m_encryptionType[(int)currentValue]);
            if (currentValue == encryptionType)
            {
                Log.Comment("Returning a pass since " + m_encryptionType[(int)encryptionType]
                    + " == " + m_encryptionType[(int)currentValue]);
                return MFTestResults.Pass;
            }

            Log.Comment("Returning a fail since " + m_encryptionType[(int)encryptionType]
                + " != " + m_encryptionType[(int)currentValue]);
            return MFTestResults.Fail;
        }

        private MFTestResults VerifyRadioType(Wireless80211.RadioType radioType)
        {
            m_wireless = FindWireless80211();
            Log.Comment("Verifying radio type is set to " + RadioTypeToString(radioType));
            Wireless80211.RadioType currentValue = m_wireless.Radio;
            Log.Comment("Radio type we get is " + RadioTypeToString(currentValue));
            if (currentValue == radioType)
            {
                Log.Comment("Returning a pass since " + RadioTypeToString(radioType)
                    + " == " + RadioTypeToString(currentValue));
                return MFTestResults.Pass;
            }

            Log.Comment("Returning a fail since " + RadioTypeToString(radioType)
                + " != " + RadioTypeToString(currentValue));
            return MFTestResults.Fail;
        }

        private MFTestResults VerifyPassPhrase(string passPhrase)
        {
            m_wireless = FindWireless80211();
            Log.Comment("Verifying the pass phrase is " + passPhrase);
            string currentValue = m_wireless.PassPhrase;
            Log.Comment("The pass phrase we get is " + currentValue);
            if (string.Equals(passPhrase, currentValue))
            {
                Log.Comment("Returning a pass since " + passPhrase + " == " + currentValue);                
                return MFTestResults.Pass; 
            }
            
            Log.Comment("Returning a fail since " + passPhrase + " != " + currentValue);                
            return MFTestResults.Fail;
        }

        private MFTestResults VerifyNetworkKey(byte[] key)
        {
            m_wireless = FindWireless80211();
            Log.Comment("Verifying the network key is " + ByteArrayPrinter(key));
            byte[] currentValue = m_wireless.NetworkKey;
            return ReturnResult(key, currentValue);
        }

        private MFTestResults VerifyReKeyInternal(byte[] key)
        {
            m_wireless = FindWireless80211();
            Log.Comment("Verifying ReKeyInternal is " + ByteArrayPrinter(key));
            byte[] currentValue = m_wireless.ReKeyInternal;
            return ReturnResult(key, currentValue);
        }

        private MFTestResults VerifySsid(string key)
        {
            m_wireless = FindWireless80211();
            string currentValue = m_wireless.Ssid;
            Log.Comment("Verifying the ssid is " + key);
            if (string.Equals(key, currentValue))
            {
                Log.Comment("Returning a pass since " + key + " == " + currentValue);                
                return MFTestResults.Pass; 
            }
            
            Log.Comment("Returning a fail since " + key + " != " + currentValue);                
            return MFTestResults.Fail;
        }

        private MFTestResults ReturnResult(byte[] key, byte[] currentValue)
        {
            Log.Comment("The current value we get is " + currentValue);
            if (currentValue == key)
            {
                Log.Comment("Returning a pass since " + key + " == " + currentValue);
                return MFTestResults.Pass;
            }

            Log.Comment("Returning a fail since " + key + " != " + currentValue);
            return MFTestResults.Fail;
        }

        private MFTestResults TestNetworkKey(byte[] key)
        {
            try
            {                
                SetNetworkKey(key);
                if (MFTestResults.Fail == VerifyNetworkKey(key))
                {
                    return MFTestResults.Fail;
                }
                return MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Comment("An exception was thrown in TestNetworkKey(): " + ex.Message);
                return MFTestResults.Fail;
            }
        }

        private MFTestResults TestReKeyInternal(byte[] key)
        {
            try
            {
                SetReKeyInternal(key);
                if (MFTestResults.Fail == VerifyReKeyInternal(key))
                {
                    return MFTestResults.Fail;
                }
                return MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Comment("An exception was thrown in TestReKeyInternal(): " + ex.Message);
                return MFTestResults.Fail;
            }
        }

        private MFTestResults TestSsid(string key)
        {
            try
            {
                SetSsid(key);
                if (MFTestResults.Fail == VerifySsid(key))
                {
                    return MFTestResults.Fail;
                }
                return MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Comment("An exception was thrown in TestSsid(): " + ex.Message);
                return MFTestResults.Fail;
            }
        }

        private string ByteArrayPrinter(byte[] key)
        {
            string s = "{ ";
            if (null != key)
            {
                for (int i = 0; i < key.Length; i++)
                {
                    s += key[i].ToString() + " ";
                }
                s += " }";

                return s;
            }
            else
            {
                return "null";
            }
        }

        private Wireless80211 FindWireless80211()
        {
            Log.Comment("Finding the wireless interface");
            foreach (NetworkInterface net in m_interfaces)
            {
                if (net is Wireless80211)
                {
                    Log.Comment("Wireless interface found");
                    return (Wireless80211)net;                    
                }
            }
            Log.Comment("Unable to find a wireless interface");
            return null;
        }

        private byte[] GetByteArray(int length)
        {
            return MFUtilities.GetRandomBytes(length);
        }

        #endregion
    }
}
