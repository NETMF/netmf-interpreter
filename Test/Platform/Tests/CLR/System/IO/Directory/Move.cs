////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Platform.Test;

using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Move : IMFTestInterface
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
                AddDir(IOTests.Volume.RootDirectory + "\\" + TestDir1 + "\\" + Mid1 + "\\" + Tail1);
                AddDir(IOTests.Volume.RootDirectory + "\\" + TestDir1 + "\\" + Mid1 + "\\" + Tail2);
                AddDir(IOTests.Volume.RootDirectory + "\\" + TestDir1 + "\\" + Mid2 + "\\" + Tail1);
                AddDir(IOTests.Volume.RootDirectory + "\\" + TestDir2);
            }
            catch (Exception ex)
            {
                Log.Exception("Skipping: Unable to initialize file system", ex);

                return InitializeResult.Skip;
            }
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        #region local vars
        private ArrayList validDirs = new ArrayList();
        private const string TestDir1 = "MoveDirectory1";
        private const string TestDir2 = "MoveDirectory2";
        private const string TestDir3 = "MoveDirectory3";
        private const string Mid1 = "Mid1";
        private const string Mid2 = "Mid2";
        private const string Tail1 = "Tail1";
        private const string Tail2 = "Tail2";
        #endregion local vars

        #region Helper functions

        private void AddDir(string path)
        {
            Log.Comment("Adding Directory: " + path);
            try
            {
                path = Path.GetFullPath(path);

                if (!validDirs.Contains(path))
                {
                    // create full path
                    Directory.CreateDirectory(path);
                    while (!validDirs.Contains(path) & path != @"\")
                    {
                        validDirs.Add(path);
                        path = Path.GetFullPath(path + @"\..");
                    }
                }
            }
            catch
            {
                Log.Exception("Failed to create directory");
            }
        }

        private string GetPath(params string[] nodes)
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
            return path;
        }

        private string StrArrayToStr(string[] list)
        {
            string result = "";
            foreach (string item in list)
            {
                result += item + ", ";
            }
            return result.TrimEnd(',', ' ');
        }

        private int CountChildern(string path)
        {
            int count = 0;
            string[] childern = Directory.GetDirectories(Path.GetFullPath(path));
            count += childern.Length;
            foreach (string child in childern)
            {
                count += CountChildern(child);
            }
            return count;
        }

        private bool TestMove(string source, string destination)
        {
            bool success = true;
            int sourceChildren = CountChildern(source);
            Log.Comment("Source: " + source);
            Log.Comment("Destination: " + destination);
            Log.Comment("Children: " + sourceChildren);

            // Move
            Directory.Move(source, destination);

            if (Directory.Exists(source))
            {
                Log.Exception("Old directory not gone - " + source);
                success = false;
            }

            if (!Directory.Exists(destination))
            {
                Log.Exception("Source not found at destination - " + destination);
                return false;
            }

            // verify children
            int destChildren = CountChildern(destination);
            if (sourceChildren != destChildren)
            {
                Log.Exception("Lost children!  Destination Children: " + destChildren);
                return false;
            }

            return success;
        }
        #endregion Helper functions

        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                // null cases
                string localdir = Directory.GetCurrentDirectory();

                try
                {
                    Log.Comment("Null, local");
                    Directory.Move(null, localdir);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentNullException");
                }
                catch (ArgumentNullException) { /* pass case */ }
                try
                {
                    Log.Comment("local, Null");
                    Directory.Move(localdir, null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentNullException");
                }
                catch (ArgumentNullException) { /* pass case */ }

                // string.empty cases
                try
                {
                    Log.Comment("String.Empty, local");
                    Directory.Move(String.Empty, localdir);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("local, String.Empty");
                    Directory.Move(localdir, String.Empty);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                // whitespace cases
                try
                {
                    Log.Comment("White Space, Local");
                    Directory.Move("       ", localdir);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("local, White Space");
                    Directory.Move(localdir, "       ");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                // Non-existant dirs
                try
                {
                    Log.Comment("Nonexistent, Local");
                    Directory.Move("Nonexistent", localdir);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (IOException) { /* pass case - Directory not found */ }
                try
                {
                    Log.Comment("local, Nonexistent");
                    Directory.Move(localdir, "Nonexistent");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (IOException) { /* pass case - Directory not found  */ }

                // wildchar in directory
                try
                {
                    Log.Comment("TestDir, *");
                    Directory.Move(TestDir1, "*");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("*, TestDir");
                    Directory.Move("*", TestDir1);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                // Move parent Dir
                try
                {
                    Log.Comment(".., TestDir");
                    Directory.Move("..", TestDir1);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (IOException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults TailRename()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("rename Tail1 to Tail2");
                if (!TestMove(GetPath(IOTests.Volume.RootDirectory, TestDir1, Mid2, Tail1), GetPath(IOTests.Volume.RootDirectory, TestDir1, Mid2, Tail2)))
                    result = MFTestResults.Fail;

                Log.Comment("move Back");
                if (!TestMove(GetPath(IOTests.Volume.RootDirectory, TestDir1, Mid2, Tail2), GetPath(IOTests.Volume.RootDirectory, TestDir1, Mid2, Tail1)))
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
        public MFTestResults TopRename()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("rename TestDir1 to TestDir3");
                if (!TestMove(GetPath(IOTests.Volume.RootDirectory, TestDir1), GetPath(IOTests.Volume.RootDirectory, TestDir3)))
                    result = MFTestResults.Fail;

                Log.Comment("move Back");
                if (!TestMove(GetPath(IOTests.Volume.RootDirectory, TestDir3), GetPath(IOTests.Volume.RootDirectory, TestDir1)))
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
        public MFTestResults TailMoveRoot()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("move root Tail1 to TestDir2");
                if (!TestMove(GetPath(IOTests.Volume.RootDirectory, TestDir1, Mid1, Tail1), GetPath(IOTests.Volume.RootDirectory, TestDir2, Tail1)))
                    result = MFTestResults.Fail;

                Log.Comment("move Back");
                if (!TestMove(GetPath(IOTests.Volume.RootDirectory, TestDir2, Tail1), GetPath(IOTests.Volume.RootDirectory, TestDir1, Mid1, Tail1)))
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
        public MFTestResults TopMoveRoot()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("move root TestDir1 to TestDir2");
                if (!TestMove(GetPath(IOTests.Volume.RootDirectory, TestDir1), GetPath(IOTests.Volume.RootDirectory, TestDir2, TestDir1)))
                    result = MFTestResults.Fail;

                Log.Comment("move back");
                if (!TestMove(GetPath(IOTests.Volume.RootDirectory, TestDir2, TestDir1), GetPath(IOTests.Volume.RootDirectory, TestDir1)))
                    result = MFTestResults.Fail;
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
