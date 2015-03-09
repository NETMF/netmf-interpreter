using System;
using System.Collections;

namespace System.Text.RegularExpressions
{
    internal class GroupEnumerator : IEnumerator
    {
        #region Fields

        internal int _curindex = -1;
        internal GroupCollection _rgc;

        #endregion

        #region Constructor

        internal GroupEnumerator(GroupCollection rgc)
        {
            this._rgc = rgc;
        }

        #endregion

        #region Methods

        public bool MoveNext()
        {
            int count = this._rgc.Count;
            if (this._curindex >= count)
            {
                return false;
            }
            return (++this._curindex < count);
        }

        public void Reset()
        {
            this._curindex = -1;
        }

        #endregion

        #region Properties

        public Capture Capture
        {
            get
            {
                if ((this._curindex < 0) || (this._curindex >= this._rgc.Count))
                {
                    throw new InvalidOperationException("EnumNotStarted");
                }
                return this._rgc[this._curindex];
            }
        }

        public object Current
        {
            get
            {
                return this.Capture;
            }
        }
        
        #endregion
    }
}
