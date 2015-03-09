////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

#if SCHEMA_VALIDATION
using System.Xml.Schema;
#endif //SCHEMA_VALIDATION

namespace System.Xml
{

    // XmlReaderSettings class specifies features of an XmlReader.
    public class XmlReaderSettings
    {
        //
        // Fields
        //
        // Nametable
        XmlNameTable nameTable;

        // Text settings
        int lineNumberOffset;
        int linePositionOffset;

        // Conformance settings
        ConformanceLevel conformanceLevel;
        bool checkCharacters;

        // Validation settings
        ValidationType validationType;

        // Filtering settings
        bool ignoreWhitespace;
        bool ignorePIs;
        bool ignoreComments;
        // other settings
        bool closeInput;

        // read-only flag
        bool isReadOnly;

        //
        // Constructor
        //
        public XmlReaderSettings()
        {
            Reset();
        }

        //
        // Properties
        //
        // Nametable
        public XmlNameTable NameTable
        {
            get
            {
                return nameTable;
            }

            set
            {
                CheckReadOnly("NameTable");
                nameTable = value;
            }
        }

        // Text settings
        public int LineNumberOffset
        {
            get
            {
                return lineNumberOffset;
            }

            set
            {
                CheckReadOnly("LineNumberOffset");
                if (lineNumberOffset < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                lineNumberOffset = value;
            }
        }

        public int LinePositionOffset
        {
            get
            {
                return linePositionOffset;
            }

            set
            {
                CheckReadOnly("LinePositionOffset");
                if (linePositionOffset < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                linePositionOffset = value;
            }
        }

        // Conformance settings
        public ConformanceLevel ConformanceLevel
        {
            get
            {
                return conformanceLevel;
            }

            set
            {
                CheckReadOnly("ConformanceLevel");

                if ((uint)value > (uint)ConformanceLevel.Document)
                {
                    throw new XmlException(Res.Xml_ConformanceLevel, "");
                }

                conformanceLevel = value;
            }
        }

        public bool CheckCharacters
        {
            get
            {
                return checkCharacters;
            }

            set
            {
                CheckReadOnly("CheckCharacters");
                checkCharacters = value;
            }
        }

        // Validation settings
        [Obsolete("Use ValidationType property set to ValidationType.Schema")]
        public bool XsdValidate
        {
            get
            {
                return validationType == ValidationType.Schema;
            }

            set
            {
                CheckReadOnly("XsdValidate");
                if (value)
                {
                    validationType = ValidationType.Schema;
                }
                else
                {
                    validationType = ValidationType.None;
                }
            }
        }

        public ValidationType ValidationType
        {
            get
            {
                return validationType;
            }

            set
            {
                CheckReadOnly("ValidationType");
                validationType = value;
            }
        }

        // Filtering settings
        public bool IgnoreWhitespace
        {
            get
            {
                return ignoreWhitespace;
            }

            set
            {
                CheckReadOnly("IgnoreWhitespace");
                ignoreWhitespace = value;
            }
        }

        public bool IgnoreProcessingInstructions
        {
            get
            {
                return ignorePIs;
            }

            set
            {
                CheckReadOnly("IgnoreProcessingInstructions");
                ignorePIs = value;
            }
        }

        public bool IgnoreComments
        {
            get
            {
                return ignoreComments;
            }

            set
            {
                CheckReadOnly("IgnoreComments");
                ignoreComments = value;
            }
        }

        public bool CloseInput
        {
            get
            {
                return closeInput;
            }

            set
            {
                CheckReadOnly("CloseInput");
                closeInput = value;
            }
        }

        //
        // Public methods
        //
        public void Reset()
        {
            nameTable = null;
            lineNumberOffset = 0;
            linePositionOffset = 0;
            checkCharacters = true;
            conformanceLevel = ConformanceLevel.Document;
            ignoreWhitespace = false;
            ignorePIs = false;
            ignoreComments = false;
            closeInput = false;
            isReadOnly = false;
        }

        public XmlReaderSettings Clone()
        {
            XmlReaderSettings clonedSettings = MemberwiseClone() as XmlReaderSettings;
            clonedSettings.isReadOnly = false;
            return clonedSettings;
        }

        //
        // Internal and private methods
        //
        internal bool ReadOnly
        {
            get
            {
                return isReadOnly;
            }

            set
            {
                isReadOnly = value;
            }
        }

        private void CheckReadOnly(string propertyName)
        {
            if (isReadOnly)
            {
                throw new XmlException(Res.Xml_ReadOnlyProperty, "XmlReaderSettings." + propertyName);
            }
        }

        internal bool CanResolveExternals
        {
            get
            {
                return false;
            }
        }

    }
}


