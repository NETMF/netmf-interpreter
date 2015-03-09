using System.Collections;
using System;

namespace Microsoft.SPOT.Net
{
    /// <summary>
    /// A list to store prefixes
    /// </summary>
    public class PrefixList : ArrayList
    {
        /// <summary>
        /// Add a prefix to the list
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override int Add(object value)
        {
            string uri = value as string;
            int result = -1;
            if (CheckValid(uri))
            {
                result = base.Add(value);
            }
            else
            {
                throw new ArgumentException("Prefix is not valid");
            }
            return result;
        }

        /// <summary>
        /// Check whether the prefix is valid
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private bool CheckValid(string uri)
        {
            if (uri == null)
            {
                return false;
            }
            char[] cArray = uri.ToCharArray();
            if (cArray[0] != '/' || cArray[cArray.Length - 1] != '/')
            {
                return false;
            }
            return true;
        }
    }
}
