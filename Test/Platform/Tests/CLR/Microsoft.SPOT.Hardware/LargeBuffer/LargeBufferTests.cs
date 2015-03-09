////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Platform.Tests
{
    using System;
    using Microsoft.SPOT;
    using System.Threading;
    using Microsoft.SPOT.Hardware;



    public class LargeBufferTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("LargeBufferTest Init");

            LargeBufferMarshaller m = new LargeBufferMarshaller(33);
            LargeBuffer b = null;

            try
            {
                b = new LargeBuffer(10);
                m.UnMarshalBuffer(ref b);
            }
            catch
            {
                // the test driver is not included
                return InitializeResult.Skip;
            }
            finally
            {
                if (b != null)
                {
                    b.Dispose();
                }
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("LargeBufferTest clean");           
        }

        AutoResetEvent m_evtLargeBuffer = new AutoResetEvent(false);
        ushort m_eventId = 0;

        private void OnLargeBufferEvent(ushort id)
        {
            m_eventId = id;
            m_evtLargeBuffer.Set();
        }

        [TestMethod]
        public MFTestResults LargeBufferTest_TestEvent()
        {
            MFTestResults res = MFTestResults.Pass;
            try
            {
                LargeBufferMarshaller lbm = new LargeBufferMarshaller(321);
                LargeBuffer lb = new LargeBuffer(100);

                LargeBufferMarshaller.OnLargeBufferRequest += new LargeBufferMarshaller.LargeBufferEventHandler(OnLargeBufferEvent);

                m_evtLargeBuffer.Reset();

                lbm.MarshalBuffer(lb);

                if (!m_evtLargeBuffer.WaitOne( 5000, true ))
                {
                    Log.Comment("Did not receive LargeBufferMarshaller event");
                    res = MFTestResults.Fail;
                }
                else if(m_eventId != 321)
                {
                    Log.Comment("Wrong marshallerID in event");
                    res = MFTestResults.Fail;
                }
                lb.Dispose();
                LargeBufferMarshaller.OnLargeBufferRequest -= new LargeBufferMarshaller.LargeBufferEventHandler(OnLargeBufferEvent);
            }
            catch(Exception e)
            {
                Log.Exception("Exception", e);
                res = MFTestResults.Fail;
            }

            return res;
        }

        [TestMethod]
        public MFTestResults LargeBufferTest_LargeData()
        {
            MFTestResults res = MFTestResults.Pass;
            LargeBuffer lb = null;

            try
            {
                LargeBufferMarshaller lbm = new LargeBufferMarshaller(321);
                lb = new LargeBuffer(1024 * 1024);
                int len = lb.Bytes.Length - 1;

                for (int i = 0; i < 1000; i++)
                {
                    lb.Bytes[len-i] = (byte)i;
                }

                lbm.MarshalBuffer(lb);

                lbm.UnMarshalBuffer(ref lb);

                for (int i = 0; i < 1000; i++)
                {
                    if ((byte)i != lb.Bytes[i])
                    {
                        Log.Comment("UnMarshalBuffer should have swapped the bytes");
                        res = MFTestResults.Fail;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception("Exception", e);
                res = MFTestResults.Fail;
            }
            finally
            {
                if (lb != null) lb.Dispose();
            }

            return res;
        }

        [TestMethod]
        public MFTestResults LargeBufferTest_DifferentSizeBuff()
        {
            MFTestResults res = MFTestResults.Pass;
            LargeBuffer lb = null;
            LargeBuffer lb2 = null;

            try
            {
                lb = new LargeBuffer(1000);
                lb2 = new LargeBuffer(100);

                LargeBufferMarshaller lbm = new LargeBufferMarshaller(123);

                lbm.MarshalBuffer(lb);

                lbm.UnMarshalBuffer(ref lb2);

                if (lb.Bytes.Length != lb2.Bytes.Length)
                {
                    Log.Comment("The UnMarshalBuffer call should have changed the byte length of lb2");
                    res = MFTestResults.Fail;
                }
            }
            catch (Exception e)
            {
                Log.Exception("Exception", e);
                res = MFTestResults.Fail;
            }
            finally
            {
                if (lb != null) lb.Dispose();
                if (lb2 != null) lb2.Dispose();
            }

            return res;
        }
    } 
}

