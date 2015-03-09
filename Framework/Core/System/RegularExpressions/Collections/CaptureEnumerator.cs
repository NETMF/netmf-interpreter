using System;
using System.Collections;

namespace System.Text.RegularExpressions
{
    internal class CaptureEnumerator : IEnumerator
    {
        #region Fields

        internal int _curindex = -1;
        internal CaptureCollection _rcc;

        #endregion

        #region Constructor 

        internal CaptureEnumerator(CaptureCollection rcc)
        {
            this._rcc = rcc;
        }

        #endregion

        #region Methods

        public bool MoveNext()
        {
            int count = this._rcc.Count;
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
                if ((this._curindex < 0) || (this._curindex >= this._rcc.Count))
                {
                    throw new InvalidOperationException("EnumNotStarted");
                }
                return this._rcc[this._curindex];
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
