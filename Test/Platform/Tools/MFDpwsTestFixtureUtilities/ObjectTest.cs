using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

namespace MFDpwsTestFixtureUtilities
{
    public class ObjectTest
    {
        public static bool IsNullEquivalant(object o)
        {
            if (o == null) return true;

            if (o.GetType().IsArray)
            {
                Array a = (Array)o;

                if (a.Length == 0) return true;
            }

            if (o.GetType() == typeof(string))
            {
                string s = (string)o;
                return s.Length == 0;
            }

            return false;
        }

        public static void TestEqualEx(object o1, object o2)
        {
            if (!TestEqual(o1, o2))
            {
                throw new ArgumentException("Object not equivalent");
            }
        }

        public static bool TestEqual(object o1, object o2)
        {
            // Both objects are either null or an empty array
            if (IsNullEquivalant(o1) && IsNullEquivalant(o2)) return true;

            // One of the objects is null or an empty array, but
            // the other one is not.
            if (IsNullEquivalant(o1) || IsNullEquivalant(o2))
            {
                LogNotEqual(
                    IsNullEquivalant(o1) ? "null" : "not null",
                    IsNullEquivalant(o2) ? "null" : "not null"); 
                return false;
            }

            // Both objects should have the same type.
            if (o1.GetType() != o2.GetType())
            {
                LogNotEqual(
                    "type of " + o1.GetType().FullName,
                    "type of " + o2.GetType().FullName);
                return false;
            }

            // One object is an array and the other one is not.
            if ((o1.GetType().IsArray && !o2.GetType().IsArray) ||
                (!o1.GetType().IsArray && o2.GetType().IsArray))
            {
                LogNotEqual(
                    o1.GetType().IsArray ? "an array" : "not an array",
                    o2.GetType().IsArray ? "an array" : "not an array");
                return false;
            }

            Log.Comment("Testing objects of type " + o1.GetType().FullName + " for equivalence");

            bool retVal = false;
            switch (o1.GetType().FullName)
            {
                case "System.String":
                    retVal = (((string)o1) == ((string)o2));
                    break;
                case "System.Int16":
                    retVal = (((Int16)o1) == ((Int16)o2));
                    break;
                case "System.Int32":
                    retVal = (((Int32)o1) == ((Int32)o2));
                    break;
                case "System.Int64":
                    retVal = (((Int64)o1) == ((Int64)o2));
                    break;
                case "System.UInt16":
                    retVal = (((UInt16)o1) == ((UInt16)o2));
                    break;
                case "System.UInt32":
                    retVal = (((UInt32)o1) == ((UInt32)o2));
                    break;
                case "System.UInt64":
                    retVal = (((UInt64)o1) == ((UInt64)o2));
                    break;
                case "System.Double":
                    retVal = (((Double)o1) == ((Double)o2));
                    break;
                case "System.Single":
                    retVal = (((Single)o1) == ((Single)o2));
                    break;
                case "System.Boolean":
                    retVal = (((Boolean)o1) == ((Boolean)o2));
                    break;
                case "System.Byte":
                    retVal = (((Byte)o1) == ((Byte)o2));
                    break;
                case "System.SByte":
                    retVal = (((SByte)o1) == ((SByte)o2));
                    break;
                case "System.TimeSpan":
                    var t1 = (TimeSpan)o1;
                    var t2 = (TimeSpan)o2;

                    retVal = TimeSpansEqual(t1, t2);
                         
                    break;
                case "System.DateTime":
                    var d1 = (DateTime)o1;
                    var d2 = (DateTime)o2;

                    retVal = DateTimesEqual(d1, d2);
                    break;
                case "System.Array":
                    Array a1 = (Array)o1;
                    Array a2 = (Array)o2;

                    if ((a1 == null || a1.Length == 0) && (a2 == null || a2.Length == 0)) return true;

                    if (a1 == null || a2 == null)
                    {
                        LogNotEqual(
                            a1 == null ? "null" : "not null",
                            a2 == null ? "null" : "not null");
                        return false;
                    }

                    if (o1.GetType() != typeof(Ws.Services.Xml.WsXmlAttribute[]))
                    {
                        if (a1.Length != a2.Length)
                        {
                            LogNotEqual(
                                "an array of length " + a1.Length,
                                "an array of length " + a2.Length);
                            return false;
                        }
                    }
                    else
                    {
                        if (a1.Length > a2.Length)
                        {
                            LogNotEqual(
                                "an array of length >= to " + a1.Length,
                                "an array of length " + a2.Length);
                        }
                    }

                    if (o1.GetType() != typeof(Ws.Services.Xml.WsXmlAttribute[]))
                    {
                        for (int i = 0; i < a1.Length; i++)
                        {
                            Log.Comment("Testing array elements " + i);
                            if (!TestEqual(a1.GetValue(i), a2.GetValue(i)))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                default:
                    if (o1.GetType().IsArray)
                    {
                        goto case "System.Array";
                    }

                    // User defined type.  Compare public fields.
                    foreach (FieldInfo fi in o1.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                    {
                        Log.Comment("Test fields " + fi.Name);
                        if (!TestEqual(fi.GetValue(o1), fi.GetValue(o2)))
                        {
                            return false;
                        }
                    }

                    return true;
            }

            if (!retVal)
            {
                LogNotEqual(o1.ToString(), o2.ToString());
            }
            return retVal;
        }

        public static bool DateTimesEqual(DateTime d1, DateTime d2)
        {
            return ((d1.Date == d2.Date) && 
                TicksEqual(
                    d1.TimeOfDay.Ticks, 
                    d2.TimeOfDay.Ticks, 
                    TimeSpan.TicksPerSecond));
        }

        public static bool TimeSpansEqual(TimeSpan t1, TimeSpan t2)
        {
            return TicksEqual(t1.Ticks, t2.Ticks);
        }

        public static bool TicksEqual(long t1, long t2)
        {
            return TicksEqual(t1, t2, TimeSpan.TicksPerMillisecond);
        }

        public static bool TicksEqual(long t1, long t2, long p)
        {
            long delta = t1 - t2;

            return ((delta > -p) && (delta < p));
        }

        public static void LogNotEqual(string expected, string actual)
        {
            Log.Comment("Objects are not equal.  Expected is " +
                    expected + ".  " +
                    "Actual is " + actual + "."); 
        }
    }
}
