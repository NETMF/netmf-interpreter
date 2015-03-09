////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Platform.Test;
using System.Collections;

namespace Microsoft.SPOT.Platform.Tests
{
    public class HashtableTests : IMFTestInterface
    {
        private const int c_MinimumEntries = 10;
        private const int c_BareMinimum    = 1;

        //--//

        internal class MyClassTypeEntry
        {
            public MyClassTypeEntry() 
            {
                m_structValue = Guid.NewGuid();
                m_stringValue = "string" + m_structValue.ToString();
                m_integralValue = 42;
            }

            public MyClassTypeEntry(string s, int i, Guid g) 
            {
                m_stringValue = s;
                m_integralValue = i;
                m_structValue = g;
            }

            // override Object.GetHashCode
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            // override Object.Equals
            public override bool Equals(object obj)
            {
                try
                {
                    MyClassTypeEntry a = (MyClassTypeEntry)obj;

                    if (m_stringValue != a.StringValue)
                    {
                        return false;
                    }
                    if (m_integralValue != a.IntegerValue)
                    {
                        return false;
                    }
                    if (!m_structValue.Equals(a.GuidValue))
                    {
                        return false;
                    }

                    return true;
                }
                catch(Exception e)
                {
                    Log.Exception("Unexpected exception when comparing items", e);
                    return false;
                }
            }
            public string StringValue
            {
                get
                {
                    return m_stringValue;
                }
            }
            public int IntegerValue
            {
                get
                {
                    return m_integralValue;
                }
            }
            public Guid GuidValue
            {
                get
                {
                    return m_structValue;
                }
            }
            //--//
            public static string GetKey(int i, Guid g)
            {
                return "key_" + i.ToString() + "__" + g.ToString();
            }
            //--//
            private readonly string m_stringValue;
            private readonly int m_integralValue;
            private readonly Guid m_structValue;

        }

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //Test Case Calls 
        [TestMethod]
        public MFTestResults Hashtable_Add_Contains_Remove_Test1()
        {
            // Enter 5 values with 5 unique keys
            // 1) check that the keys are in the table
            // 2) check that all keys are present
            // 3) check that the items are what they are expected to be through all possible interfaces
            // 4) check that we can successfully remove the items 
            // 5) check that a removed item is no longer in the table, and so its key it is no longer in the table as well

            try
            {
                string key1 = "key1";
                string key2 = "key2";
                string key3 = "key3";
                string key4 = "key4";
                string key5 = "key5";

                MyClassTypeEntry entry1 = new MyClassTypeEntry("1 (one)", 1, Guid.NewGuid());
                MyClassTypeEntry entry2 = new MyClassTypeEntry("2 (two)", 2, Guid.NewGuid());
                MyClassTypeEntry entry3 = new MyClassTypeEntry("3 (three)", 3, Guid.NewGuid());
                MyClassTypeEntry entry4 = new MyClassTypeEntry("4 (four)", 4, Guid.NewGuid());
                MyClassTypeEntry entry5 = new MyClassTypeEntry("5 (five)", 5, Guid.NewGuid());

                string[] keys = new string[] { key1, key2, key3, key4, key5 };
                MyClassTypeEntry[] entries = new MyClassTypeEntry[] { entry1, entry2, entry3, entry4, entry5 };

                Hashtable t = new Hashtable();

                // 1) add 5 items with 5 unique keys
                t.Add(key1, entry1);
                t.Add(key2, entry2);
                t.Add(key3, entry3);
                t.Add(key4, entry4);
                t.Add(key5, entry5);

                // 2) check all added keys are present
                if (
                   !t.Contains(key1) ||
                   !t.Contains(key2) ||
                   !t.Contains(key3) ||
                   !t.Contains(key4) ||
                   !t.Contains(key5)
                  )
                {
                    return MFTestResults.Fail;
                }

                // 3) check that the items are what they are expected to be
                // check the items reference and value first...
                int index = 0;
                foreach (String k in keys)
                {
                    // test indexer
                    MyClassTypeEntry entry = (MyClassTypeEntry)t[k];
                    // check that the refernce is the same 
                    if (!Object.ReferenceEquals(entry, (entries[index])))
                    {
                        return MFTestResults.Fail;
                    }
                    // check that the values are the same
                    if (!entry.Equals(entries[index]))
                    {
                        return MFTestResults.Fail;
                    }
                    index++;
                }
                // ... then check the keys                
                foreach (String k in keys)
                {
                    bool found = false;
                    ICollection keysCollection = t.Keys;
                    foreach (string key in keysCollection)
                    {
                        if (k == key)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) return MFTestResults.Fail;
                }
                // 4) checked that we can remove the items
                // ... then check the keys                
                foreach (String k in keys)
                {
                    t.Remove(k);
                }
                // 4) checked that we can remove the items
                // ... then check the keys                
                foreach (String k in keys)
                {
                    t.Remove(k);
                }
                // 5) check that a removed item is no longer in the table, and so its key it is no longer in the table as well           
                // check the items reference and value first...
                // test nothing is left in teh Hashtable 
                if (t.Count != 0)
                {
                    return MFTestResults.Fail;
                }
                int indexR = 0;
                foreach (String k in keys)
                {
                    // test Contains
                    if (t.Contains(k))
                    {
                        return MFTestResults.Fail;
                    }
                    // test indexer
                    MyClassTypeEntry entry = (MyClassTypeEntry)t[k];
                    if (entry != null)
                    {
                        return MFTestResults.Fail;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }
        
        [TestMethod]
        public MFTestResults Hashtable_Add_Clear()
        {
            try
            {
                Hashtable t = new Hashtable();

                MyClassTypeEntry[] vals = InsertRandomValues(t, c_MinimumEntries);

                if (t.Count != vals.Length)
                {
                    return MFTestResults.Fail;
                }

                t.Clear();

                if (t.Count != 0)
                {
                    return MFTestResults.Fail;
                }

                ICollection keys = t.Keys;
                ICollection values = t.Values;
                if (keys.Count != 0)
                {
                    return MFTestResults.Fail;
                }
                if (values.Count != 0)
                {
                    return MFTestResults.Fail;
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }
        
        [TestMethod]
        public MFTestResults Hashtable_CheckKeys()
        {
            try
            {
                Hashtable t = new Hashtable();

                MyClassTypeEntry[] vals = InsertRandomValues(t, c_MinimumEntries);

                // check that the hastable contains the keys
                foreach (MyClassTypeEntry k in vals)
                {
                    if (!t.Contains(k.StringValue))
                    {
                        return MFTestResults.Fail;
                    }
                }

                ICollection keys = t.Keys;

                foreach(MyClassTypeEntry m in vals)
                {
                    // check that the key collection contains the key
                    bool found = false;
                    foreach(string s in keys)
                    {
                        if(m.StringValue.Equals(s))
                        {
                            found = true; break;
                        }
                    }
                    if (!found)
                    {
                        return MFTestResults.Fail;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }


        [TestMethod]
        public MFTestResults Hashtable_CheckValues()
        {
            try
            {
                Hashtable t = new Hashtable();

                MyClassTypeEntry[] vals = InsertRandomValues(t, c_MinimumEntries);

                // check that the hastable contains the keys
                foreach (MyClassTypeEntry k in vals)
                {
                    if (!t.Contains(k.StringValue))
                    {
                        return MFTestResults.Fail;
                    }
                }

                ICollection values = t.Values;

                foreach (MyClassTypeEntry m in vals)
                {
                    // check that the key collection contains the key
                    bool verified = false;
                    foreach (MyClassTypeEntry mm in values)
                    {
                        if (m.Equals(mm))
                        {
                            verified = true; break;
                        }
                    }
                    if (!verified)
                    {
                        return MFTestResults.Fail;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Hashtable_Count()
        {
            try
            {
                Hashtable t = new Hashtable();

                MyClassTypeEntry[] vals = InsertRandomValues(t, c_MinimumEntries);

                int count = t.Count;

                if (vals.Length != count)
                {
                    return MFTestResults.Fail;
                }

                t.Add("a new key without a guid, can't exist", new MyClassTypeEntry());
                t.Add("a new key without a guid, can't exist again", new MyClassTypeEntry());
                t.Add("a new key without a guid, can't exist another time", new MyClassTypeEntry());

                if ((count + 3) != t.Count)
                {
                    return MFTestResults.Fail;
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults Hashtable_Duplicate()
        {
            try
            {
                Hashtable t = new Hashtable();

                MyClassTypeEntry[] vals = InsertRandomValues(t, c_MinimumEntries);

                //
                // find a key and insert a duplicate: must fail
                //
                MyClassTypeEntry entry = vals[vals.Length / 2];
                string key = MyClassTypeEntry.GetKey(entry.IntegerValue, entry.GuidValue);

                bool exceptionThrown = false;
                try
                {
                    t.Add(key, new MyClassTypeEntry());
                }
                catch(Exception e)
                {
                    Log.Exception("EXpected exception -- duplicate in Hashtable", e);
                    exceptionThrown = true;
                }
                if (!exceptionThrown)
                {
                    return MFTestResults.Fail;
                }

                // remove the item 
                t.Remove(key);

                // try insert again: must succeeed
                exceptionThrown = false;
                try
                {
                    t.Add(key, new MyClassTypeEntry());
                }
                catch
                {
                    exceptionThrown = true;
                }
                if (exceptionThrown)
                {
                    return MFTestResults.Fail;
                }
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

         [TestMethod]
         public MFTestResults Hashtable_CopyTo()
         {
             try
             {
                 // TODO TODO TODO 
             }
             catch (Exception e)
             {
                 Log.Exception("Unexpected exception", e);
                 return MFTestResults.Fail;
             }

             return MFTestResults.Pass;
         }

         [TestMethod]
         public MFTestResults Hashtable_Clone()
         {
             try
             {
                 // TODO TODO TODO 
             }
             catch (Exception e)
             {
                 Log.Exception("Unexpected exception", e);
                 return MFTestResults.Fail;
             }

             return MFTestResults.Pass;
         }

        //--//

        /// <summary>
        /// Creates a MyClassEntry type whose string member with the prefix is the key item in the the hashtable
        /// </summary>
        private MyClassTypeEntry[] InsertRandomValues(Hashtable t, int max)
        {
            int count = (new Random().Next() % max) + c_BareMinimum; // at least 1

            MyClassTypeEntry[] vals = new MyClassTypeEntry[count];

            for (int i = 0; i < count; ++i)
            {
                Guid g = Guid.NewGuid();
                string key = MyClassTypeEntry.GetKey(i, g);
                vals[i] = new MyClassTypeEntry(key, i, g);
                t.Add(key, vals[i]);
            }

            return vals;
        }
    }
}
