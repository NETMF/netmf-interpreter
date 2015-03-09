namespace System.Collections
{
    using System;
    public interface IEqualityComparer
    {
        bool Equals(Object x,Object y);
        int GetHashCode(Object obj);
    }
}