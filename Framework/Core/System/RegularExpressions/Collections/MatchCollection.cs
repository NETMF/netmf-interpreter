using System;
using System.Collections;

namespace System.Text.RegularExpressions
{
    [Serializable]
    public class MatchCollection : ICollection, IEnumerable
    {
        #region Fields

        internal string _input;
        /// <summary>
        /// the last index to match against
        /// </summary>
        internal int _lastIndex;
        internal ArrayList _matches;
        internal int _previousIndex;
        internal Regex _regex;
        internal int _startIndex;
        internal bool _done;
        internal const int infinite = 0x7fffffff;

        #endregion

        #region Constructor

        internal MatchCollection(Regex regex, string input, int length, int startat)
        {
            if ((startat < 0) || (startat > input.Length)) throw new ArgumentOutOfRangeException("startat");
            if (length < 0) length = input.Length;
            this._regex = regex;
            this._input = input;
            this._lastIndex = length;
            this._startIndex = startat;
            this._previousIndex = 0;
            this._matches = new ArrayList();
        }

        #endregion

        #region Methods

        public void CopyTo(Array array, int arrayIndex)
        {
            int count = this.Count;
            try
            {
                this._matches.CopyTo(array, arrayIndex);
            }
            catch
            {
                throw new ArgumentException("Arg_InvalidArrayType");
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new MatchEnumerator(this);
        }

        internal Match GetMatch(int i)
        {
            Match match;            
            if (this._matches.Count > i) return (Match)this._matches[i];
            if (i < 0 || this._done) return null;
            do
            {
                //match = this._regex.Run(false, this._prevlen, this._input, this._beginning, this._length, this._startat);
                match = this._regex.Match(this._input, this._previousIndex, this._lastIndex);
                if (!match.Success)
                {
                    this._done = true;
                    return null;
                }
                this._matches.Add(match);
                this._previousIndex = match._textend;
            }
            while (this._matches.Count <= i);
            return match;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of matches. 
        /// </summary>
        public int Count
        {
            get
            {
                if (!this._done)
                {
                    this.GetMatch(infinite);
                }
                return this._matches.Count;
            }
        }
 


        /// <summary>
        /// Gets a value that indicates whether the collection is read only. 
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the collection is synchronized (thread-safe). 
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an individual member of the collection. 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual Match this[int i]
        {
            get
            {
                if (i > Count) throw new ArgumentOutOfRangeException("i");
                return this.GetMatch(i);
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the collection. 
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        #endregion
    }
}
