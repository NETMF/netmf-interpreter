/*---------------------------------------------------------------------
* FaultsTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 11/14/2007 10:27:20 AM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Ws.Services.Mtom;

namespace Microsoft.SPOT.Platform.Tests
{
    public class MtomTests : IMFTestInterface
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
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults MtomTest1_WsMtomBodyPart()
        {            
            /// <summary>
            /// 1. Gets and verifies each of the properties of a WsMtomBodyPart object
            /// 2. Sets and re-verifies all properties
            /// </summary>
            ///
            bool testResult = true;
            try
            {
                WsMtomBodyPart testWMBP = new WsMtomBodyPart();

                Log.Comment("ContentID");
                if (testWMBP.ContentID != null)
                    if (testWMBP.ContentID.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("ContentID wrong type");

                testWMBP.ContentID = "test datum 1";
                if (testWMBP.ContentID.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("ContentID wrong type after set to new");

                if (testWMBP.ContentID != "test datum 1")
                    throw new Exception("ContentID wrong data after set to new");


                Log.Comment("ContentTransferEncoding");
                if (testWMBP.ContentTransferEncoding != null)
                    if (testWMBP.ContentTransferEncoding.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("ContentTransferEncoding wrong type");

                testWMBP.ContentTransferEncoding = "test datum 2";
                if (testWMBP.ContentTransferEncoding.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("ContentTransferEncoding wrong type after set to new");

                if (testWMBP.ContentTransferEncoding != "test datum 2")
                    throw new Exception("ContentTransferEncoding wrong data after set to new");

                Log.Comment("ContentType");
                if (testWMBP.ContentType != null)
                    if (testWMBP.ContentType.GetType() !=
                        Type.GetType("System.String"))
                        throw new Exception("ContentType wrong type");

                testWMBP.ContentType = "test datum 3";
                if (testWMBP.ContentType.GetType() !=
                    Type.GetType("System.String"))
                    throw new Exception("ContentType wrong type after set to new");

                if (testWMBP.ContentType != "test datum 3")
                    throw new Exception("ContentType wrong data after set to new");

                Log.Comment("Content");
                if (testWMBP.Content != null)
                    if (testWMBP.Content.GetType() !=
                        Type.GetType("System.IO.MemoryStream"))
                        throw new Exception("Content wrong type");

                testWMBP.Content = new System.IO.MemoryStream();
                if (testWMBP.Content.GetType() !=
                    Type.GetType("System.IO.MemoryStream"))
                    throw new Exception("Content wrong type");
            }
            catch (Exception e)
            {
                testResult = false;
                Log.Comment("Incorrect exception caught: " + e.Message);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}
