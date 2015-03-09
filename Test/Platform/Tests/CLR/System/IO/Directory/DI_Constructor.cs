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
    public class DI_Constructor : IMFTestInterface
    {
        #region internal vars
        private const string DIRA = @"DirA";
        private const string DIRB = @"DirB";
        private const string DIR1 = @"Dir1";
        private const string DIR2 = @"Dir2";
        #endregion

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

                Directory.CreateDirectory(GetDir(DIRA, DIR1));
                Directory.CreateDirectory(GetDir(DIRA, DIR2));
                Directory.CreateDirectory(GetDir(DIRB, DIR1));
                Directory.CreateDirectory(GetDir(DIRB, DIR2));
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

        #region Helper Functions
        private string GetDir(params string[] args)
        {
            string dir = IOTests.Volume.RootDirectory;
            for (int i = 0; i < args.Length; i++)
            {
                dir += @"\" + args[i];
            }
            return dir;
        }
        private bool TestDirectoryInfo(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            path = RelativePath(path);
            Log.Comment("Path: '" + path + "'");
            if (dir.FullName != path)
            {
                Log.Exception("Got: '" + dir.FullName + "'");
                return false;
            }
            return true;
        }

        private string RelativePath(string path)
        {
            // rooted
            if (path.Substring(0,1) == "\\")
                return path;
            return Directory.GetCurrentDirectory() + "\\" + path;
        }
        #endregion

        #region Test Cases

        [TestMethod]
        public MFTestResults NullArguments()
        {
            DirectoryInfo dir;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Null Constructor");
                    dir = new DirectoryInfo(null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected Argument exception, but got " + dir.FullName);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("String.Empty Constructor");
                    dir = new DirectoryInfo(String.Empty);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected Argument exception, but got " + dir.FullName);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("White Space Constructor");
                    dir = new DirectoryInfo("       ");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected Argument exception, but got " + dir.FullName);
                }
                catch (ArgumentException) { /* pass case */ }

                // Try above root
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);
                try
                {
                    Log.Comment(".. above root Constructor");
                    /// .. is a valid location, while ..\\.. is not.
                    dir = new DirectoryInfo("..\\..");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected Argument exception, but got " + dir.FullName);
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
        public MFTestResults ValidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("root");
                if (!TestDirectoryInfo(Directory.GetCurrentDirectory()))
                {
                    result = MFTestResults.Fail;
                }
                Log.Comment("test created dirs");
                if (!TestDirectoryInfo(GetDir(DIRA)))
                {
                    result = MFTestResults.Fail;
                } 
                if (!TestDirectoryInfo(GetDir(DIRB)))
                {
                    result = MFTestResults.Fail;
                } 
                if (!TestDirectoryInfo(GetDir(DIRA,DIR1)))
                {
                    result = MFTestResults.Fail;
                } 
                if (!TestDirectoryInfo(GetDir(DIRA, DIR2)))
                {
                    result = MFTestResults.Fail;
                } 
                if (!TestDirectoryInfo(GetDir(DIRB, DIR1)))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestDirectoryInfo(GetDir(DIRB, DIR2)))
                {
                    result = MFTestResults.Fail;
                }
                Log.Comment("Case insensitive");
                if (!TestDirectoryInfo(GetDir(DIRA.ToLower())))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestDirectoryInfo(GetDir(DIRB.ToUpper())))
                {
                    result = MFTestResults.Fail;
                }
                Log.Comment("Relative - set current dir to DirB");
                Directory.SetCurrentDirectory(GetDir(DIRB));
                if (!TestDirectoryInfo(DIR1))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestDirectoryInfo(DIR2))
                {
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
        public MFTestResults ArgumentExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            DirectoryInfo dir;
            try
            {
                foreach (char invalidChar in Path.GetInvalidPathChars())
                {
                    try
                    {
                        Log.Comment("Invalid char ascii val = " + (int)invalidChar);
                        string path = new string(new char[] { 'b', 'a', 'd', '\\', invalidChar, 'p', 'a', 't', 'h', invalidChar, '.', 't', 'x', 't' });
                        dir = Directory.CreateDirectory(path);
                        if (invalidChar == 0)
                        {
                            Log.Exception("[Known issue for '\\0' char] Expected Argument exception for for '" + path + "' but got: '" + dir + "'");
                        }
                        else
                        {
                            Log.Exception("Expected Argument exception for '" + path + "' but got: '" + dir.FullName + "'");
                            result = MFTestResults.Fail;
                        }
                    }
                    catch (ArgumentException) { /* Pass Case */ }
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
