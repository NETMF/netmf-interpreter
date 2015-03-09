using System;

namespace Microsoft.SPOT.Net.Ftp
{
    internal static class Utility
    {
        /// <summary>
        /// Replace '\' with '/'
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static string ReplaceBackSlash(string s)
        {
            char[] cArray = s.ToCharArray();
            for (int i = 0; i < cArray.Length; i++)
            {
                if (cArray[i] == '\\')
                    cArray[i] = '/';
            }
            return new string(cArray);
        }

        /// <summary>
        /// Print unix-like date
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        internal static string PrintDate(DateTime dt)
        {
            string result = "";
            switch (dt.Month)
            {
                case 1: result += "Jan  "; break;
                case 2: result += "Feb  "; break;
                case 3: result += "Mar  "; break;
                case 4: result += "Apr  "; break;
                case 5: result += "May  "; break;
                case 6: result += "Jun  "; break;
                case 7: result += "Jul  "; break;
                case 8: result += "Aug  "; break;
                case 9: result += "Sep  "; break;
                case 10: result += "Oct  "; break;
                case 11: result += "Nov  "; break;
                case 12: result += "Dec  "; break;
                default: break;
            }
            result += dt.Day.ToString() + "  " + dt.Year.ToString() + " ";
            return result;
        }

        /// <summary>
        /// ToString method for long integers
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal static string MyToString(long l)
        {
            string s = "";
            int i = (int)(l / 1000000000);
            if (i > 0)
                s += i.ToString();
            i = (int)(l % 1000000000);
            s = s + i.ToString();
            return s;
        }
    }
}