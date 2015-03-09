using System;
using System.Collections;

namespace System.Text.RegularExpressions
{
    [Serializable]
    internal class MatchEnumerator : IEnumerator
    {
        #region Fields
        
        internal int _curindex;
        internal bool _done;
        internal Match _match;
        internal MatchCollection _matchcoll;
        
        #endregion

        #region Methods

        internal MatchEnumerator(MatchCollection matchcoll)
        {
            this._matchcoll = matchcoll;
        }

        public bool MoveNext()
        {
            if (this._done)
            {
                return false;
            }
            this._match = this._matchcoll.GetMatch(this._curindex++);
            if (this._match == null)
            {
                this._done = true;
                return false;
            }
            return true;
        }

        public void Reset()
        {
            this._curindex = 0;
            this._done = false;
            this._match = null;
        }

        #endregion

        #region Properties

        public object Current
        {
            get
            {
                if (this._match == null)
                {
                    throw new InvalidOperationException("EnumNotStarted");
                }
                return this._match;
            }
        }

        #endregion
    }
}