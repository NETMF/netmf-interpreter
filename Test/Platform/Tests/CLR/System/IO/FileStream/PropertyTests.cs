////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class PropertyTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            // These tests rely on underlying file system so we need to make
            // sure we can format it before we start the tests.  If we can't
            // format it, then we assume there is no FS to test on this platform.

            // delete the directory DOTNETMF_FS_EMULATION
            try
            {
                IOTests.IntializeVolume();
                Directory.CreateDirectory(TestDir);
                Directory.SetCurrentDirectory(TestDir);
            }
            catch (Exception ex)
            {
                Log.Exception("Skipping: Unable to initialize file system " + ex.StackTrace);
                return InitializeResult.Skip;
            }
            return InitializeResult.ReadyToGo;
        }


        [TearDown]
        public void CleanUp()
        {
        }

        #region local vars
        private const string TestDir = "Length";
        private const string fileName = "test.tmp";
        #endregion local vars

        #region Helper methods
        private bool TestLength(FileStream fs, long expectedLength)
        {
            if (fs.Length != expectedLength)
            {
                Log.Exception("Expected length " + expectedLength + " but got, " + fs.Length);
                return false;
            }
            return true;
        }

        private bool TestPosition(FileStream fs, long expectedPosition)
        {
            if (fs.Position != expectedPosition)
            {
                Log.Exception("Expected position " + expectedPosition + " but got, " + fs.Position);
                return false;
            }
            return true;
        }
        #endregion Helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults ObjectDisposed()
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            FileStream fs = new FileStream(fileName, FileMode.Create);
            fs.Close(); 
            
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    long length = fs.Length;
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ObjectDisposedException, but got length " + length);
                }
                catch (ObjectDisposedException) { /*Pass Case */ }

                try
                {
                    long position = fs.Position;
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ObjectDisposedException, but got position " + position);
                }
                catch (ObjectDisposedException) { /*Pass Case */ }

                try
                {
                    fs.Position = 0;
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ObjectDisposedException, but set position");
                }
                catch (ObjectDisposedException) { /*Pass Case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults LengthTests()
        {
            MFTestResults result = MFTestResults.Pass;
            if (File.Exists(fileName))
                File.Delete(fileName);

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    Log.Comment("Set initial length to 50, and position to 50");
                    fs.SetLength(50);
                    fs.Position = 50;
                    if (!TestLength(fs, 50))
                        result = MFTestResults.Fail;

                    Log.Comment("Write 'foo bar'");
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write("foo bar");
                    sw.Flush();
                    if (!TestLength(fs, 57))
                        result = MFTestResults.Fail;

                    Log.Comment("Shorten Length to 30");
                    fs.SetLength(30);
                    if (!TestLength(fs, 30))
                        result = MFTestResults.Fail;

                    Log.Comment("Verify position was adjusted");
                    if (!TestPosition(fs, 30))
                        result = MFTestResults.Fail;

                    Log.Comment("Extend length to 100");
                    fs.SetLength(100);
                    if (!TestLength(fs, 100))
                        result = MFTestResults.Fail;
                }

                Log.Comment("Verify file is 100 bytes after close");
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    if (!TestLength(fs, 100))
                        result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults InvalidSetLength()
        {
            MFTestResults result = MFTestResults.Pass;
            if (File.Exists(fileName))
                File.Delete(fileName);

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    try
                    {
                        Log.Comment("-1");
                        fs.SetLength(-1);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException, but set length");
                    }
                    catch (ArgumentOutOfRangeException) { /* Pass Case */ }

                    try
                    {
                        Log.Comment("-10000");
                        fs.SetLength(-10000);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException, but set length");
                    }
                    catch (ArgumentOutOfRangeException) { /* Pass Case */ }

                    try
                    {
                        Log.Comment("long.MinValue");
                        fs.SetLength(long.MinValue);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException, but set length");
                    }
                    catch (ArgumentOutOfRangeException) { /* Pass Case */ }

                    try
                    {
                        Log.Comment("long.MaxValue");
                        fs.SetLength(long.MaxValue);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected IOException, but set length");
                    }
                    catch (ArgumentOutOfRangeException) { /* Pass Case */ }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            return result;
        }

        [TestMethod]
        public MFTestResults NameTests()
        {
            MFTestResults result = MFTestResults.Pass;
            if (File.Exists(fileName))
                File.Delete(fileName);

            try
            {
                FileStream fs = new FileStream(fileName, FileMode.CreateNew);
                Log.Comment("Verify name while open");
                if (fs.Name != Directory.GetCurrentDirectory() + @"\" + fileName)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Unexpected File name: " + fs.Name);
                }

                Log.Comment("Verify name after close");
                fs.Close();
                if (fs.Name != Directory.GetCurrentDirectory() + @"\" + fileName)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Unexpected File name: " + fs.Name);
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            return result;
        }
        #endregion Test Cases
    }
}
