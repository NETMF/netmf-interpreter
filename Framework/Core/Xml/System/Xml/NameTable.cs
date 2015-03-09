////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace System.Xml
{

    // <devdoc>
    //    <para>
    //       XmlNameTable implemented as a simple hash table.
    //    </para>
    // </devdoc>
    public class NameTable : XmlNameTable
    {
        //
        // Private types
        //
        class Entry
        {
            internal string str;
            internal int hashCode;
            internal Entry next;

            internal Entry(string str, int hashCode, Entry next)
            {
                this.str = str;
                this.hashCode = hashCode;
                this.next = next;
            }
        }

        //
        // Fields
        //
        Entry[] entries;
        int count;
        int mask;

        //
        // Constructor
        //
        // <devdoc>
        //      Public constructor.
        // </devdoc>
        public NameTable()
        {
            mask = 31;
            entries = new Entry[mask + 1];
        }

        //
        // XmlNameTable public methods
        //
        // <devdoc>
        //      Add the given string to the NameTable or return
        //      the existing string if it is already in the NameTable.
        // </devdoc>
        public override string Add(string array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("key");
            }

            if (array.Length == 0)
            {
                return "";
            }

            int len = array.Length;
            int hashCode = len;
            // use array.Length to eliminate the rangecheck
            for (int i = 0; i < array.Length; i++)
            {
                hashCode += (hashCode << 7) ^ array[i];
            }

            // mix it a bit more
            hashCode -= hashCode >> 17;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;

            for (Entry e = entries[hashCode & mask]; e != null; e = e.next)
            {
                if (e.hashCode == hashCode && String.Compare(e.str, array) == 0)
                {
                    return e.str;
                }
            }

            return AddEntry(array, hashCode);
        }

        // <devdoc>
        //      Add the given string to the NameTable or return
        //      the existing string if it is already in the NameTable.
        // </devdoc>
        public override string Add(char[] array, int offset, int length)
        {
            if (length == 0)
            {
                return "";
            }

            int hashCode = length;
            hashCode += (hashCode << 7) ^ array[offset];   // this will throw IndexOutOfRangeException in case the start index is invalid
            int end = offset + length;
            for (int i = offset + 1; i < end; i++)
            {
                hashCode += (hashCode << 7) ^ array[i];
            }

            // mix it a bit more
            hashCode -= hashCode >> 17;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;

            for (Entry e = entries[hashCode & mask]; e != null; e = e.next)
            {
                if (e.hashCode == hashCode && TextEquals(e.str, array, offset))
                {
                    return e.str;
                }
            }

            return AddEntry(new string(array, offset, length), hashCode);
        }

        // <devdoc>
        //      Find the matching string in the NameTable.
        // </devdoc>
        public override string Get(string array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("value");
            }

            if (array.Length == 0)
            {
                return "";
            }

            int len = array.Length;
            int hashCode = len;
            // use array.Length to eliminate the rangecheck
            for (int i = 0; i < array.Length; i++)
            {
                hashCode += (hashCode << 7) ^ array[i];
            }

            // mix it a bit more
            hashCode -= hashCode >> 17;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;

            for (Entry e = entries[hashCode & mask]; e != null; e = e.next)
            {
                if (e.hashCode == hashCode && String.Compare(e.str, array) == 0)
                {
                    return e.str;
                }
            }

            return null;
        }

        // <devdoc>
        //      Find the matching string atom given a range of
        //      characters.
        // </devdoc>
        public override string Get(char[] array, int offset, int length)
        {
            if (length == 0)
            {
                return "";
            }

            int hashCode = length;
            hashCode += (hashCode << 7) ^ array[offset];   // this will throw IndexOutOfRangeException in case the start index is invalid
            int end = offset + length;
            for (int i = offset + 1; i < end; i++)
            {
                hashCode += (hashCode << 7) ^ array[i];
            }

            // mix it a bit more
            hashCode -= hashCode >> 17;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;

            for (Entry e = entries[hashCode & mask]; e != null; e = e.next)
            {
                if (e.hashCode == hashCode && TextEquals(e.str, array, offset))
                {
                    return e.str;
                }
            }

            return null;
        }

        //
        // Private methods
        //

        private string AddEntry(string str, int hashCode)
        {
            int index = hashCode & mask;
            Entry e = new Entry(str, hashCode, entries[index]);
            entries[index] = e;
            if (count++ == mask)
            {
                Grow();
            }

            return e.str;
        }

        private void Grow()
        {
            int newMask = mask * 2 + 1;
            Entry[] oldEntries = entries;
            Entry[] newEntries = new Entry[newMask + 1];

            // use oldEntries.Length to eliminate the rangecheck
            for (int i = 0; i < oldEntries.Length; i++)
            {
                Entry e = oldEntries[i];
                while (e != null)
                {
                    int newIndex = e.hashCode & newMask;
                    Entry tmp = e.next;
                    e.next = newEntries[newIndex];
                    newEntries[newIndex] = e;
                    e = tmp;
                }
            }

            entries = newEntries;
            mask = newMask;
        }

        private static bool TextEquals(string array, char[] text, int start)
        {
            // use array.Length to eliminate the rangecheck
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != text[start + i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}


