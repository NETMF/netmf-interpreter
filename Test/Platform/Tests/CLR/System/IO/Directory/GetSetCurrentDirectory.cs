////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Platform.Test;

using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class GetSetCurrentDirectory : IMFTestInterface
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
                Directory.CreateDirectory(TestDir + "\\" + Mid1 + "\\" + Tail1);
                Directory.CreateDirectory(TestDir + "\\" + Mid1 + "\\" + Tail2);
                Directory.CreateDirectory(TestDir + "\\" + Mid2 + "\\" + Tail1);
                Directory.CreateDirectory(TestDir + "\\" + Mid2 + "\\" + Tail2);
            }
            catch (Exception ex)
            {
                Log.Comment("Skipping: Unable to initialize file system" + ex.StackTrace);

                return InitializeResult.Skip;
            }
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        #region local vars
        private const string TestDir = "ExistsDirectory";
        private const string Mid1 = "Mid1";
        private const string Mid2 = "Mid2";
        private const string Tail1 = "Tail1";
        private const string Tail2 = "Tail2";
        #endregion local vars

        #region Helper functions
        private bool TestGetSet(params string[] nodes)
        {
            string path = "";
            if (nodes.Length > 0)
            {
                path = nodes[0];
                for (int i = 1; i < nodes.Length; i++)
                {
                    path += "\\" + nodes[i];
                }
            }
            string expected = NormalizePath(path);
            Log.Comment("Changing path to " + path);
            Directory.SetCurrentDirectory(path);
            string result = Directory.GetCurrentDirectory();
            
            if (result != expected)
            {
                Log.Exception("Set failed.  Current directory is " + result);
                Log.Exception("Expected " + expected);
                return false;
            }
            return true;
        }

        private string NormalizePath(string newpath)
        {
            // Not rooted - get full path from Relative
            if (!(newpath.Substring(0, 1) == @"\"))
            {
                newpath = Directory.GetCurrentDirectory() + @"\" + newpath;
            }

            string path = "";
            int skipCount = 0;
            string[] nodes = newpath.Split('\\');
            // first node is always empty since we are rooted, so stop at 2nd node (i=1)
            for (int i = nodes.Length - 1; i > 0; i--)
            {
                // Drop . nodes (current)
                if (!(nodes[i] == "."))
                {
                    if (nodes[i] == "..")
                    {
                        skipCount++;
                    }
                    else
                    {
                        if (skipCount == 0)
                            //append node
                            path = @"\" + nodes[i] + path;
                        else
                            //skip node
                            skipCount--;
                    }
                }
            }

            return path;
        }

        #endregion Helper functions
        #region Test Cases

        [TestMethod]
        public MFTestResults GetSetRelative()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);

                Log.Comment("Walk tree using relative path");
                if (!TestGetSet(TestDir))
                    result = MFTestResults.Fail;

                if (!TestGetSet(Mid1))
                    result = MFTestResults.Fail;

                if (!TestGetSet(Tail1))
                    result = MFTestResults.Fail;

                if (!TestGetSet("."))
                    result = MFTestResults.Fail; 
                
                if (!TestGetSet(".."))
                    result = MFTestResults.Fail;

                if (!TestGetSet(Tail2))
                    result = MFTestResults.Fail;

                if (!TestGetSet(@"..\.."))
                    result = MFTestResults.Fail;

                if (!TestGetSet(Mid2, Tail2))
                    result = MFTestResults.Fail;

                if (!TestGetSet(@"..\..\.."))
                    result = MFTestResults.Fail;

                // Complex path, should result in path to Tail2
                if (!TestGetSet(TestDir, ".", Mid2, "..", Mid2, ".", Tail2))
                    result = MFTestResults.Fail;

                if (!TestGetSet("..", "..", Mid1, Tail1))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults GetSetAboslute()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Walk tree using absolute path");
                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir))
                    result = MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, Mid1))
                    result = MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, Mid2, Tail2))
                    result = MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, "."))
                    result = MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, ".."))
                    result = MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, Mid2, Tail2, @"..\..", Mid2, Tail2))
                    result = MFTestResults.Fail;

                // Complex path, should result in path to Tail2
                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, ".", Mid2, "..", Mid2, ".", Tail2))
                    result = MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults InvalidArgs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("null");
                    Directory.SetCurrentDirectory(null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentNullException got " + Directory.GetCurrentDirectory());
                }
                catch (ArgumentNullException) { /* pass case */ }

                try
                {
                    Log.Comment("String.Empty");
                    Directory.SetCurrentDirectory(string.Empty);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException got " + Directory.GetCurrentDirectory());
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Whitespace");
                    Directory.SetCurrentDirectory("    ");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException got " + Directory.GetCurrentDirectory());
                }
                catch (ArgumentException) { /* pass case */ }

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults InvalidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("NonExistentDir");
                    Directory.SetCurrentDirectory("NonExistentDir");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException got " + Directory.GetCurrentDirectory());
                }
                catch (IOException) { /* pass case, DirectoryNotFound */ }

                try
                {
                    Log.Comment(@"non exist mount \foo");
                    Directory.SetCurrentDirectory(@"\foo");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException got " + Directory.GetCurrentDirectory());
                }
                catch (IOException) { /* pass case */ }

                try
                {
                    Log.Comment(@"Move before root - ..\..\..\..\..");
                    Directory.SetCurrentDirectory(@"..\..\..\..\..");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException got " + Directory.GetCurrentDirectory());
                }
                catch (ArgumentException) { /* pass case */ }
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
