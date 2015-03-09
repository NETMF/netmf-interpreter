////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemGCTests : IMFTestInterface
    {
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

        //SystemGC Test methods
        class FinalizeObject
        {
            public static FinalizeObject m_currentInstance = null;

            ~FinalizeObject()
            {
                if (m_hasFinalized1 == false)
                {
                    Log.Comment("First finalization");

                    // Put this object back into a root by creating
                    // a reference to it.
                    FinalizeObject.m_currentInstance = this;

                    // Indicate that this instance has finalized once.
                    m_hasFinalized1 = true;

                    // Place a reference to this object back in the
                    // finalization queue.
                    GC.ReRegisterForFinalize(this);
                }
                else
                {
                    Log.Comment("Second finalization"); 
                    m_hasFinalized2 = true;
                }
            }
        }

        static bool m_hasFinalized1 = false;
        static bool m_hasFinalized2 = false;
        static bool m_Test1Result = false;
 
        [TestMethod]
        public MFTestResults SystemGC1_Test()
        {
            /// <summary>
            /// 1. Create a FinalizeObject.
            /// 2. Release the reference
            /// 3. Allow for GC
            /// 4. Run ReRegisterForFinalize
            /// 5. Allow for GC
            /// 6. Verify that object has been collected
            /// </summary>
            ///
            Log.Comment("Tests ReRegisterForFinalize");
            Log.Comment("Create a FinalizeObject.");
            FinalizeObject mfo = new FinalizeObject();
            m_hasFinalized1 = false;
            m_hasFinalized2 = false;

            Log.Comment("Release reference");
            mfo = null;

            Log.Comment("Allow GC");
            SPOT.Debug.GC(true); 
            int sleepTime = 1000;
            int slept = 0;
            while (m_hasFinalized1 == false && slept < sleepTime)
            {
                System.Threading.Thread.Sleep(10);
                slept += 10;
            }
            Log.Comment("GC took " + slept); 

            // At this point mfo will have gone through the first Finalize.
            // There should now be a reference to mfo in the static
            // FinalizeObject.m_currentInstance field.  Setting this value
            // to null and forcing another garbage collection will now
            // cause the object to Finalize permanently.
            Log.Comment("Reregister and allow for GC");
            FinalizeObject.m_currentInstance = null;
            SPOT.Debug.GC(true);
            sleepTime = 1000;
            slept = 0;
            while (m_hasFinalized2 == false && slept < sleepTime)
            {
                System.Threading.Thread.Sleep(10);
                slept += 10;
            }
            Log.Comment("GC took " + slept);

            m_Test1Result = m_hasFinalized2;
            return (m_hasFinalized2 ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemGC2_Test()
        {
            /// <summary>
            /// 1. Create a FinalizeObject.
            /// 2. Release the reference
            /// 3. SupressFinalize
            /// 3. Allow for GC
            /// 6. Verify that object has not been collected
            /// </summary>
            ///
            Log.Comment("Tests SuppressFinalize");
            Log.Comment("Create a FinalizeObject.");
            FinalizeObject mfo = new FinalizeObject();
            m_hasFinalized1 = false;
            m_hasFinalized2 = false;

            Log.Comment("Releasing");
            System.GC.SuppressFinalize(mfo);
            mfo = null;

            Log.Comment("Allow GC");
            SPOT.Debug.GC(true);
            int sleepTime = 1000;
            int slept = 0;
            while (m_hasFinalized1 == false && slept < sleepTime)
            {
                System.Threading.Thread.Sleep(10);
                slept += 10;
            }
            Log.Comment("GC took " + slept); 

            return (!m_hasFinalized1 ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemGC3_Test()
        {
            /// <summary>
            /// 1. Create a FinalizeObject.
            /// 2. Release the reference
            /// 3. SupressFinalize
            /// 3. Allow for GC
            /// 6. Verify that object has not been collected
            /// </summary>
            ///
            Log.Comment("Tests WaitForPendingFinalizers, dependant on test 1");
            Log.Comment("will auto-fail if test 1 fails."); 
            if (m_Test1Result)
            {
                Log.Comment("Create a FinalizeObject.");
                FinalizeObject mfo = new FinalizeObject();
                m_hasFinalized1 = false;
                m_hasFinalized2 = false;

                Log.Comment("Releasing");
                mfo = null;

                Log.Comment("Wait for GC");
                SPOT.Debug.GC(true);
                System.GC.WaitForPendingFinalizers();
               
                Log.Comment("Releasing again");
                FinalizeObject.m_currentInstance = null;

                Log.Comment("Wait for GC");
                SPOT.Debug.GC(true);
                System.GC.WaitForPendingFinalizers();

                return (m_hasFinalized2 ? MFTestResults.Pass : MFTestResults.Fail);
            }
            return MFTestResults.Fail;
        }
    }
}
