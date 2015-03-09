using System;
using System.Collections;

namespace System.Text.RegularExpressions
{
    /* Previous Implemetnation
    [Serializable]
    public class GroupCollection : ICollection, IEnumerable
    {
        #region Fields

        internal Hashtable _captureMap;
        internal Group[] _groups;
        internal Match _match;

        #endregion

        #region Constructor

        internal GroupCollection(Match match, Hashtable captures)
        {
            this._match = match;
            this._captureMap = captures;
        }

        #endregion

        #region Methods

        public void CopyTo(Array array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            IList list = array as IList;
            for (int i = 0, index = arrayIndex, e = this.Count; i < e; i++, index++) list[i] = index;
        }

        public IEnumerator GetEnumerator()
        {
            return new GroupEnumerator(this);
        }

        internal Group GetGroup(int groupnum)
        {
            if (this._captureMap != null)
            {
                object obj2 = this._captureMap[groupnum];
                if (obj2 == null)
                {
                    return Group._emptyGroup;
                }
                return this.GetGroupImpl((int)obj2);
            }
            if ((groupnum < this._match._captures.Count) && (groupnum >= 0)) return this.GetGroupImpl(groupnum);
            return Group._emptyGroup;
        }

        internal Group GetGroupImpl(int groupnum)
        {
            if (groupnum == 0) return this._match;
            if (this._groups == null)
            {
                this._groups = new Group[this._match._captures.Count - 1];
                for (int i = 0; i < this._groups.Length; i++)
                {
                    this._groups[i] = new Group(this._match._origionalString, (int[])this._match._captures[i]);
                }
            }
            return this._groups[groupnum - 1];
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the number of groups in the collection. 
        /// </summary>
        public int Count
        {
            get
            {
                return this._match._captures.Count;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the collection is read-only. 
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether access to the GroupCollection is synchronized (thread-safe). 
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }
        
        /// <summary>
        /// Enables access to a member of the collection by integer index. 
        /// </summary>
        /// <param name="groupnum"></param>
        /// <returns></returns>
        public Group this[int groupnum]
        {
            get
            {
                return this.GetGroup(groupnum);
            }
        }

        /// <summary>
        /// Enables access to a member of the collection by string index. 
        /// </summary>
        /// <param name="groupname"></param>
        /// <returns></returns>
        public Group this[string groupname]
        {
            get
            {
                if (this._match._regex == null) return Group._emptyGroup;
                //return this.GetGroup(this._match._regex.GroupNumberFromName(groupname));
                return Group._emptyGroup;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the GroupCollection
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return this._match;
            }
        }
        
        #endregion
    }
    */

    [Serializable]
    public class GroupCollection : ICollection, IEnumerable
    {
        // Fields
        internal Hashtable _captureMap;
        internal Group[] _groups;
        internal Match _match;

        // Methods
        internal GroupCollection(Match match, Hashtable caps)
        {
            this._match = match;
            this._captureMap = caps;
        }

        public void CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            IList list = array as IList;
            for (int i = 0; i < this.Count; ++i)
            {
                //array.SetValue(this[i], index);
                //index++;
                list[i] = ++arrayIndex;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new GroupEnumerator(this);
        }

        internal Group GetGroup(int groupnum)
        {
            if (this._captureMap != null)
            {
                object obj2 = this._captureMap[groupnum];
                if (obj2 == null)
                {
                    return Group._emptygroup;
                }
                return this.GetGroupImpl((int)obj2);
            }
            if ((groupnum < this._match._matchcount.Length) && (groupnum >= 0))
            {
                return this.GetGroupImpl(groupnum);
            }
            return Group._emptygroup;
        }

        internal Group GetGroupImpl(int groupnum)
        {
            if (groupnum == 0)
            {
                return this._match;
            }
            if (this._groups == null)
            {
                int groupLen = this._match._matchcount.Length - 1;
                this._groups = new Group[groupLen];
                //int capcount = this._match._regex.matchCount * 2;
                //int[] caps = new int[capcount];
                //this._match._regex.starts.CopyTo(caps, 0);
                //this._match._regex.ends.CopyTo(caps, this._match._regex.matchCount);


                for (int i = 0; i < groupLen; ++i)
                {
                    //This goes out of range because matches i + 1 is longer then the array
                    //The reason for this is because matches array is not formatted correctly, it should be in the format int[][], where the index in the first dimension corresponds to the capture map which is a int[] formatted like so {start, end}
                    //A quick fix would be to catch the out of range exception and utilize the look ahead which is already calculated and guaranteed to exist from this._match._regex.Group(groupnum) then add it using GroupStart and GroupEnd
                    //string groupValue =  this._match._regex.Group(groupnum);
                    //return new Group(groupValue, new int[] { this._match._regex.GroupStart(groupnum), this._match._regex.GroupStart(groupnum) }, this._match._regex.matchCount);
                    

                    //this._groups[i] = new Group(this._match._text, caps, capcount);

                    this._groups[i] = new Group(this._match._text, this._match._matches[i + 1], this._match._matchcount[i + 1]);                    
                }
            }
            return this._groups[groupnum - 1];
        }

        // Properties
        public int Count
        {
            get
            {
                return this._match._matchcount.Length;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public Group this[int groupnum]
        {
            get
            {
                return this.GetGroup(groupnum);
            }
        }

        public Group this[string groupname]
        {
            get
            {
                //if (this._match._regex == null)
                //{
                //    return Group._emptygroup;
                //}
                //return this.GetGroup(this._match._regex.GroupNumberFromName(groupname));

                throw new NotImplementedException();

            }
        }

        public object SyncRoot
        {
            get
            {
                return this._match;
            }
        }
    }


}
