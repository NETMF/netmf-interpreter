using System;
using System.Collections;

namespace System.Text.RegularExpressions
{
    [Serializable]
    public class Group : Capture
    {
        #region Fields

        internal CaptureCollection _capcoll;
        internal int _capcount;
        internal int[] _caps;
        internal static Group _emptygroup = new Group(string.Empty, new int[0], 0);

        #endregion

        #region Methods 

        internal Group(string text, int[] caps, int capcount)
            : base(text, (capcount == 0) ? 0 : caps[(capcount - 1) * 2], (capcount == 0) ? 0 : caps[(capcount * 2) - 1])
        {
            this._caps = caps;
            this._capcount = capcount;
        }

        //[System.Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        public static Group Synchronized(Group inner)
        {
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            CaptureCollection captures = inner.Captures;
            if (inner._capcount > 0)
            {
                Capture capture1 = captures[0];
            }
            return inner;
        }

        #endregion

        #region Properties
        public CaptureCollection Captures
        {
            get
            {
                if (this._capcoll == null)
                {
                    this._capcoll = new CaptureCollection(this);
                }
                return this._capcoll;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the match was successful. 
        /// </summary>
        public bool Success
        {
            get
            {
                return (this._capcount != 0);
            }
        }

        #endregion

    }
}
