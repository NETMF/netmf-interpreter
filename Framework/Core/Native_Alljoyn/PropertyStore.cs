//
// PropertyStore.cs
//
// Implements a property store which can be used by
// alljoyn thin core applications.
//
// TODO 1: This is 1-to-1 port (pretty much) of the C
// property store implementation. Thus the C idioms
// such as multidimensional arrays, arrays of structs,
// etc. rather than higher level data structures for
// dealing with key-value pairs. So need to reorganize
// to leverage C# data structures.
//
// TODO 2: This implemention does not take advantage
// of non-volatile storage. Everything is stored in
// ram and must be initialized upon program start. So
// need to tie into the C-level NV functions
//


using System;
using System.Text;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.AllJoyn
{
    // Indices for referencing keys in property store array
    
    public enum PropertyStoreFieldIndices {
        //Start of keys
        AJSVC_PROPERTY_STORE_DEVICE_ID = 0,
        AJSVC_PROPERTY_STORE_APP_ID,
        AJSVC_PROPERTY_STORE_DEVICE_NAME,
    #if !CONFIG_SERVICE
        AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS,
        //End of runtime keys
        AJSVC_PROPERTY_STORE_DEFAULT_LANGUAGE = AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS,
        AJSVC_PROPERTY_STORE_APP_NAME,
    #else
        AJSVC_PROPERTY_STORE_DEFAULT_LANGUAGE,
        AJSVC_PROPERTY_STORE_PASSCODE,
        AJSVC_PROPERTY_STORE_REALM_NAME,
        AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS,
        //End of runtime keys
        AJSVC_PROPERTY_STORE_APP_NAME = AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS,
    #endif
        AJSVC_PROPERTY_STORE_DESCRIPTION,
        AJSVC_PROPERTY_STORE_MANUFACTURER,
        AJSVC_PROPERTY_STORE_MODEL_NUMBER,
        AJSVC_PROPERTY_STORE_DATE_OF_MANUFACTURE,
        AJSVC_PROPERTY_STORE_SOFTWARE_VERSION,
        AJSVC_PROPERTY_STORE_AJ_SOFTWARE_VERSION,
    #if CONFIG_SERVICE
        AJSVC_PROPERTY_STORE_MAX_LENGTH,
    #endif
        AJSVC_PROPERTY_STORE_NUMBER_OF_MANDATORY_KEYS,
        //End of mandatory keys
        AJSVC_PROPERTY_STORE_HARDWARE_VERSION = AJSVC_PROPERTY_STORE_NUMBER_OF_MANDATORY_KEYS,
        AJSVC_PROPERTY_STORE_SUPPORT_URL,
        AJSVC_PROPERTY_STORE_NUMBER_OF_KEYS,
        //End of About keys
    };
    
    public struct PropertyStoreEntry {
    
        public PropertyStoreEntry(string a, int b, int c, int d, int e, int f, int g, int h, int i)
        {
            keyName = a;
            mode0Write = b;
            mode1Announce = c;
            mode2MultiLng = d;
            mode3Init = e;
            mode4 = f;
            mode5 = g;
            mode6 = h;
            mode7Public = i;
        }
    
        public string keyName;          // The property key name as shown in About and Config documentation

        public int mode0Write;          // is writable
        public int mode1Announce;       // is announcable
        public int mode2MultiLng;       // multilang property
        public int mode3Init;           // initialize once
        public int mode4;
        public int mode5;
        public int mode6;
        public int mode7Public;         // publicly accessible
    };      
    
    public struct PropertyStoreConfigEntry {
        public PropertyStoreConfigEntry(string [] a)
        {
            value = a;
        }
        public string [] value;     // An array of size 1
                                    //  - or -
                                    // NumberOfLanguages if the property is multilingual
    };
    
    public struct AJSVC_PropertyStoreCategoryFilter {
        public bool bit0About;
    #if CONFIG_SERVICE
        public bool bit1Config;
    #endif
        public bool bit2Announce;
    };

    public class PropertyStore
    {
        public const int ERROR_LANGUAGE_INDEX = -1;
        public const int ERROR_FIELD_INDEX = -1;
        public const int NO_LANGUAGE_INDEX    =  0;

        public const int DEVICE_NAME_VALUE_LENGTH = 32;
        public const int LANG_VALUE_LENGTH = 7;
        public const int KEY_VALUE_LENGTH = 10;
        
        public const int UUID_LENGTH = 16;
        public const int MACHINE_ID_LENGTH = UUID_LENGTH * 2;                                                

        public Hashtable props = null;
    
        public PropertyStoreEntry [] Properties =
        {
        //  { "Key Name            ",                       W, A, M, I .. . . ., P },
            new PropertyStoreEntry( "DeviceId",             0, 1, 0, 1, 0, 0, 0, 1 ),
            new PropertyStoreEntry( "AppId",                0, 1, 0, 1, 0, 0, 0, 1 ),
        #if !CONFIG_SERVICE
            new PropertyStoreEntry( "DeviceName",           0, 1, 1, 1, 0, 0, 0, 1 ),
        // Add other runtime keys above this line
            new PropertyStoreEntry( "DefaultLanguage",      0, 1, 0, 0, 0, 0, 0, 1 ),
        #else
            new PropertyStoreEntry( "DeviceName",           1, 1, 1, 1, 0, 0, 0, 1 ),
            new PropertyStoreEntry( "DefaultLanguage",      1, 1, 0, 0, 0, 0, 0, 1 ),
            new PropertyStoreEntry( "Passcode",             1, 0, 0, 0, 0, 0, 0, 0 ),
            new PropertyStoreEntry( "RealmName",            1, 0, 0, 0, 0, 0, 0, 0 ),
        // Add other runtime keys above this line
        #endif
            new PropertyStoreEntry( "AppName",              0, 1, 0, 0, 0, 0, 0, 1 ),
            new PropertyStoreEntry( "Description",          0, 0, 1, 0, 0, 0, 0, 1 ),
            new PropertyStoreEntry( "Manufacturer",         0, 1, 1, 0, 0, 0, 0, 1 ),
            new PropertyStoreEntry( "ModelNumber",          0, 1, 0, 0, 0, 0, 0, 1 ),
            new PropertyStoreEntry( "DateOfManufacture",    0, 0, 0, 0, 0, 0, 0, 1 ),
            new PropertyStoreEntry( "SoftwareVersion",      0, 0, 0, 0, 0, 0, 0, 1 ),
            new PropertyStoreEntry( "AJSoftwareVersion",    0, 0, 0, 0, 0, 0, 0, 1 ),
        #if CONFIG_SERVICE
            new PropertyStoreEntry( "MaxLength",            0, 1, 0, 0, 0, 0, 0, 1 ),
        #endif
        // Add other mandatory about keys above this line
            new PropertyStoreEntry( "HardwareVersion",      0, 0, 0, 0, 0, 0, 0, 1 ),
            new PropertyStoreEntry( "SupportUrl",           0, 0, 1, 0, 0, 0, 0, 1 ),
        // Add other optional about keys above this line
        };
        
        public PropertyStoreConfigEntry [] RuntimeValues =
        {
        //  {"Buffers for Values per language"},                                    "Key Name"
            new PropertyStoreConfigEntry( new string[1]{String.Empty} ),         // DeviceId
            new PropertyStoreConfigEntry( new string[1]{String.Empty} ),         // AppId
            new PropertyStoreConfigEntry( new string[1]{String.Empty} ),         // DeviceName
        // Add other persisted keys above this line
            new PropertyStoreConfigEntry( new string[1]{String.Empty} ),
            new PropertyStoreConfigEntry( new string[1]{String.Empty} ),
            new PropertyStoreConfigEntry( new string[1]{String.Empty} ),
            new PropertyStoreConfigEntry( new string[1]{String.Empty} ),
        };              
        
        public string [][] DefaultValues = 
        {
        // "Default Values per language",                    "Key Name"
            new string[1]{String.Empty},                   // DeviceId
            new string[1]{String.Empty},                   // AppId
            new string[1]{"Defname"},                      // DeviceName
            new string[1]{"en"},                           // DefaultLanguage
        // Add other runtime keys above this line
            new string[1]{"Microsoft"},                    // AppName
            new string[1]{"Toaster"},                      // Description
            new string[1]{"MSFT"},                         // Manufacturer
            new string[1]{"123"},                          // ModelNumber
            new string[1]{"1-1-01"},                       // DateOfManufacture
            new string[1]{"1.1.1"},                        // SoftwareVersion
            new string[1]{String.Empty},                   // AJSoftwareVersion
        // Add other mandatory about keys above this line
            new string[1]{"1.1.1"},                        // HardwareVersion
            new string[1]{String.Empty},                   // SupportUrl
        // Add other optional about keys above this line
        };
        
        public string DefaultLanguagesKeyName = "SupportedLanguages";
        public static string DefaultLanguage = "en";
        public string[] SupportedLanguages = { DefaultLanguage };
        public int NumberOfLanguages = 1;        
        public string AppId = String.Empty;
                
        
        // set single language properties (accept a string)
                        
        public void SetDefaultLanguage(string lang)
        {
            DefaultValues[(int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_DEFAULT_LANGUAGE] = new string[1]{lang};            
        }                                
        
        public void SetDefaultSoftwareVersion(string softwareVersion)
        {
            DefaultValues[(int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_SOFTWARE_VERSION] = new string[1]{softwareVersion};
        }
        
        public void SetDefaultModelNumber(string modelNumber)
        {
            DefaultValues[(int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_MODEL_NUMBER] = new string[1]{modelNumber};
        }
        
        // set multi-language properties (accept an array of strings)
        
        public void SetSupportedLanguages(string [] langs)
        {
            SupportedLanguages = langs;
            NumberOfLanguages = langs.Length;
        }        
        
        public void SetDefaultSupportUrls(string [] urls)
        {
            DefaultValues[(int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_SUPPORT_URL] = urls;
        }
        
        public void SetDefaultDeviceNames(string [] devNames)
        {
            DefaultValues[(int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_DEVICE_NAME] = devNames;
        }
        
        public void SetDefaultDescriptions(string [] descriptions)
        {
            DefaultValues[(int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_DESCRIPTION] = descriptions;
        }
        
        public void SetDefaultManufacturers(string [] manufacturers)
        {
            DefaultValues[(int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_MANUFACTURER] = manufacturers;
        }
        
        public void InitMandatoryFields()
        {
            if (AppId == String.Empty)
            {
                AppId = AJ.GetGUID();
            }    
            DefaultValues[(int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_APP_ID] = new string[1]{AppId};
            DefaultValues[(int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_DEVICE_ID] = new string[1]{AppId};
        }
                        
        public byte GetMaxValueLength(byte fieldIndex)
        {
            switch (fieldIndex) {
            case (int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_DEVICE_NAME:
                return DEVICE_NAME_VALUE_LENGTH;

            case (int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_DEFAULT_LANGUAGE:
                return LANG_VALUE_LENGTH;

        #if CONFIG_SERVICE
            case AJSVC_PROPERTY_STORE_PASSCODE:
                return PASSWORD_VALUE_LENGTH;
        #endif

            default:
                return KEY_VALUE_LENGTH;
            }
        }
        
        public int GetLanguageIndexForProperty(int langIndex, int fieldIndex)
        {
            if (Properties[fieldIndex].mode2MultiLng != 0) {
                return langIndex;
            }
            return NO_LANGUAGE_INDEX;
        }
        
        public string GetValue(int fieldIndex)
        {
            return GetValueForLang(fieldIndex, NO_LANGUAGE_INDEX);
        }
        
        public string GetValueForLang(int fieldIndex, int langIndex)
        {
            if (fieldIndex >= DefaultValues.Length)
            {
                return null;
            }
        
            if (fieldIndex <= ERROR_FIELD_INDEX || fieldIndex >= (int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_NUMBER_OF_KEYS) {
                return null;
            }
            
            langIndex = GetLanguageIndexForProperty(langIndex, fieldIndex);
            if (langIndex <= ERROR_LANGUAGE_INDEX || langIndex >= NumberOfLanguages) {
                return null;
            }
            if (fieldIndex < (byte)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS &&
                (Properties[fieldIndex].mode0Write ==1 || Properties[fieldIndex].mode3Init == 1) &&
                RuntimeValues[fieldIndex].value[langIndex] != String.Empty) {
                return RuntimeValues[fieldIndex].value[langIndex];
            } else if (DefaultValues[fieldIndex][langIndex] != String.Empty) {
                return DefaultValues[fieldIndex][langIndex];
            }
                            
            return String.Empty;
        }
    
        public int GetLanguageIndex(string language)
        {
            int langIndex = 0;
            string search = language;

            if (search == String.Empty) {
                search = GetValue((int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_DEFAULT_LANGUAGE);
                if (search == String.Empty) {
                    return ERROR_LANGUAGE_INDEX;
                }
            }
            for (langIndex = 0; langIndex < NumberOfLanguages; langIndex++) {
                if (search == SupportedLanguages[langIndex]) {
                    return langIndex;
                }
            }
            return ERROR_LANGUAGE_INDEX;
        }                
    }
    
    
}