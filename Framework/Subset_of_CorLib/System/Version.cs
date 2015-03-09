////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System.Globalization;

    // A Version object contains four hierarchical numeric components: major, minor,
    // revision and build.  Revision and build may be unspecified, which is represented
    // internally as a -1.  By definition, an unspecified component matches anything
    // (both unspecified and specified), and an unspecified component is "less than" any
    // specified component.

    public sealed class Version // : ICloneable, IComparable, IComparable<Version>, IEquatable<Version>
    {
        // AssemblyName depends on the order staying the same
        private int _Major;
        private int _Minor;
        private int _Build;// = -1;
        private int _Revision;// = -1;

        public Version(int major, int minor, int build, int revision)
        {
            if (major < 0 || minor < 0 || revision < 0 || build < 0)
                throw new ArgumentOutOfRangeException();

            _Major = major;
            _Minor = minor;
            _Revision = revision;
            _Build = build;
        }

        public Version(int major, int minor)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException();

            if (minor < 0)
                throw new ArgumentOutOfRangeException();

            _Major = major;
            _Minor = minor;

            // Other 2 initialize to -1 as it done on desktop and CE
            _Build = -1;
            _Revision = -1;
        }

        // Properties for setting and getting version numbers
        public int Major
        {
            get { return _Major; }
        }

        public int Minor
        {
            get { return _Minor; }
        }

        public int Revision
        {
            get { return _Revision; }
        }

        public int Build
        {
            get { return _Build; }
        }

        public override bool Equals(Object obj)
        {
            if (((Object)obj == null) ||
                (!(obj is Version)))
                return false;

            Version v = (Version)obj;
            // check that major, minor, build & revision numbers match
            if ((this._Major != v._Major) ||
                (this._Minor != v._Minor) ||
                (this._Build != v._Build) ||
                (this._Revision != v._Revision))
                return false;

            return true;
        }

        public override String ToString()
        {
            string retStr = _Major + "." + _Minor;

            // Adds _Build and then _Revision if they are positive. They could be -1 in this case not added.
            if (_Build >= 0)
            {
                retStr += "." + _Build;
                if (_Revision >= 0)
                {
                    retStr += "." + _Revision;
                }
            }

            return retStr;
        }
    }
}


