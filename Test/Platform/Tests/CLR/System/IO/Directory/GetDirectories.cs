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
    public class GetDirectories : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("The following tests are located in FileTests.cs");
            
            try
            {
                IOTests.IntializeVolume();
                AddDir(IOTests.Volume.RootDirectory + "\\" + TestDir + "\\" + Mid1 + "\\" + Tail1);
                AddDir(IOTests.Volume.RootDirectory + "\\" + TestDir + "\\" + Mid1 + "\\" + Tail2);
                AddDir(IOTests.Volume.RootDirectory + "\\" + TestDir + "\\" + Mid2 + "\\" + Tail1);
            }
            catch (Exception ex)
            {
                Log.Exception("Skipping: Unable to initialize file system.", ex); 

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
        private char[] special = new char[] { '!', '#', '$', '%', '\'', '(', ')', '+', '-', '.', '@', '[', ']', '_', '`', '{', '}', '~' };
        private const string TestDir = "GetDirectory";
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
                    while (!validDirs.Contains(path) && path != "\\" && path != null)
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

        private string StrArrayToStr(string[] list)
        {
            string result = "";
            foreach (string item in list)
            {
                result += item + ", ";
            }
            return result.TrimEnd(',', ' ');
        }

        private bool TestGetDirectoryEnum(int expected, params string[] nodes)
        {
            return VerifyEnum(expected, GetPath(nodes), Directory.EnumerateDirectories(GetPath(nodes)));
        }

        private bool VerifyEnum(int expected, string path, IEnumerable result)
        {
            bool success = true;
            int cnt = 0;
            foreach (string dir in result)
            {
                if (!validDirs.Contains(dir))
                {
                    Log.Exception("Unexpected directory found: " + dir);
                    success = false;
                }
                cnt++;
            }
            if (cnt != expected)
            {
                Log.Exception("Expected " + expected + " directories, got " + cnt);
                success = false;
            }
            return success;
        }

        private bool TestGetDirectories(int expected, params string[] nodes)
        {
            return Verify(expected, Directory.GetDirectories(GetPath(nodes)));
        }
        
        private bool Verify(int expected, string[] result)
        {
            bool success = true;
            if (result.Length != expected)
            {
                Log.Exception("Expected " + expected + " directories, got " + result.Length);
                success = false;
            }
            foreach (string dir in result)
            {
                if (!validDirs.Contains(dir))
                {
                    Log.Exception("Unexpected directory found: " + dir);
                    success = false;
                }
            }
            return success;
        }

        private string GetPath(string[] nodes)
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
            Log.Comment("Path: " + path);
            return path;
        }

        private string RandDirName(int length)
        {
            char[] chars = new char[length];
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                switch (random.Next(4))
                {
                    // 0 = specal chars
                    case 0:
                        int x = random.Next(special.Length);
                        chars[i] = special[x];
                        break;
                    // 1 = numbers
                    case 1:
                        chars[i] = (char)((int)'0' + random.Next(10));
                        break;
                    // 2 = upper case
                    case 2:
                        chars[i] = (char)((int)'A' + random.Next(26));
                        break;
                    // 3 = lower case
                    case 3:
                        chars[i] = (char)((int)'a' + random.Next(26));
                        break;
                    default:
                        throw new ArgumentException("Expected 0-3");
                }
            }
            return new string(chars);
        }
        #endregion Helper functions

        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            string[] dirs;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                // null cases
                try
                {
                    Log.Comment("Null");
                    dirs = Directory.GetDirectories(null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentNullException, but got " + StrArrayToStr(dirs));
                }
                catch (ArgumentNullException) { /* pass case */ }

                // string.empty cases
                try
                {
                    Log.Comment("String.Empty");
                    dirs = Directory.GetDirectories(String.Empty);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + StrArrayToStr(dirs));
                }
                catch (ArgumentException) { /* pass case */ }

                // whitespace cases
                try
                {
                    Log.Comment("White Space");
                    dirs = Directory.GetDirectories("       ");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + StrArrayToStr(dirs));
                }
                catch (ArgumentException) { /* pass case */ }

                // New lines
                try
                {
                    Log.Comment("\\n");
                    dirs = Directory.GetDirectories("\n");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + StrArrayToStr(dirs));
                }
                catch (ArgumentException) { /* pass case */ }

                // Filter as path
                try
                {
                    Log.Comment("*");
                    dirs = Directory.GetDirectories("*");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + StrArrayToStr(dirs));
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
        public MFTestResults ValidCasesNoFilter()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                // relative
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);
                if (!TestGetDirectories(0, TestDir, Mid1, Tail2))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(0, TestDir, Mid2, Tail1))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(2, TestDir, Mid1))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(1, TestDir, Mid2))
                    result = MFTestResults.Fail; 
                
                if (!TestGetDirectories(2, TestDir))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(1, "."))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(0, TestDir, Mid1, "." , Tail1, "..", Tail1, "..", "..", Mid2, Tail1))
                    result = MFTestResults.Fail;

                // Move up tree where there is more directories
                Directory.SetCurrentDirectory(TestDir);
                if (!TestGetDirectories(2, "."))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(1, ".."))
                    result = MFTestResults.Fail;

                // absolute
                if (!TestGetDirectories(0, IOTests.Volume.RootDirectory, TestDir, Mid1, Tail1))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(0, IOTests.Volume.RootDirectory, TestDir, Mid2, Tail1))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(1, IOTests.Volume.RootDirectory, TestDir, Mid2))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(2, IOTests.Volume.RootDirectory, TestDir, Mid1))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(2, IOTests.Volume.RootDirectory, TestDir))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(1, IOTests.Volume.RootDirectory))
                    result = MFTestResults.Fail;

                if (!TestGetDirectories(0, IOTests.Volume.RootDirectory, TestDir, Mid1, ".", Tail1, "..", Tail1, "..", "..", Mid2, Tail1, "."))
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
        public MFTestResults DirectoryEnumTest()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                // relative
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);
                if (!TestGetDirectoryEnum(0, TestDir, Mid1, Tail2))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(0, TestDir, Mid2, Tail1))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(2, TestDir, Mid1))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(1, TestDir, Mid2))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(2, TestDir))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(1, "."))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(0, TestDir, Mid1, ".", Tail1, "..", Tail1, "..", "..", Mid2, Tail1))
                    result = MFTestResults.Fail;

                // Move up tree where there is more directories
                Directory.SetCurrentDirectory(TestDir);
                if (!TestGetDirectoryEnum(2, "."))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(1, ".."))
                    result = MFTestResults.Fail;

                // absolute
                if (!TestGetDirectoryEnum(0, IOTests.Volume.RootDirectory, TestDir, Mid1, Tail1))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(0, IOTests.Volume.RootDirectory, TestDir, Mid2, Tail1))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(1, IOTests.Volume.RootDirectory, TestDir, Mid2))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(2, IOTests.Volume.RootDirectory, TestDir, Mid1))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(2, IOTests.Volume.RootDirectory, TestDir))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(1, IOTests.Volume.RootDirectory))
                    result = MFTestResults.Fail;

                if (!TestGetDirectoryEnum(0, IOTests.Volume.RootDirectory, TestDir, Mid1, ".", Tail1, "..", Tail1, "..", "..", Mid2, Tail1, "."))
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
        public MFTestResults SpecialDirNames()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Find file each with special char file names");
                string specialdir = IOTests.Volume.RootDirectory + "\\" + TestDir + "\\SpecialFileNames";
                AddDir(specialdir);

                for (int i = 0; i < special.Length; i++)
                {
                    string dir = i + "_" + new string(new char[] { special[i] }) + "_zDirectory";
                    AddDir(specialdir + "\\" + dir);
                }
                if (!TestGetDirectories(special.Length, specialdir))
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
        public MFTestResults RandomDirectoryNames()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Light stress with 100 random directory names");
                Random random = new Random();
                string randdir = IOTests.Volume.RootDirectory + "\\" + TestDir + "\\RandomFileNames";
                AddDir(randdir);

                for (int i = 0; i < 100; i++)
                {
                    string dir = i + "_" + RandDirName(random.Next(50) + 1);
                    // Don't end with period.
                    if (dir[dir.Length - 1] == '.')
                    {
                        dir += "c";
                    }
                    AddDir(randdir + "\\" +  dir);
                }
                if (!TestGetDirectories(100, randdir))
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
        public MFTestResults GetDirectoriesBadPath()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                Directory.GetDirectories(IOTests.Volume.RootDirectory + "\\_DIR_NOT_EXIST");
                result = MFTestResults.Fail;
            }
            catch (IOException e1)
            {
                if (e1.ErrorCode != IOException.IOExceptionErrorCode.DirectoryNotFound)
                    result = MFTestResults.Fail;
            }

            // make sure the directory was not created
            try
            {
                Directory.GetDirectories(IOTests.Volume.RootDirectory + "\\_DIR_NOT_EXIST");
                result = MFTestResults.Fail;
            }
            catch (IOException e1)
            {
                if (e1.ErrorCode != IOException.IOExceptionErrorCode.DirectoryNotFound)
                    result = MFTestResults.Fail;
            }


            try
            {
                Directory.GetFiles(IOTests.Volume.RootDirectory + "\\_DIR_NOT_EXIST");
                result = MFTestResults.Fail;
            }
            catch (IOException e1)
            {
                if (e1.ErrorCode != IOException.IOExceptionErrorCode.DirectoryNotFound)
                    result = MFTestResults.Fail;
            }

            // make sure the path was not created
            try
            {
                Directory.GetFiles(IOTests.Volume.RootDirectory + "\\_DIR_NOT_EXIST");
                result = MFTestResults.Fail;
            }
            catch (IOException e1)
            {
                if (e1.ErrorCode != IOException.IOExceptionErrorCode.DirectoryNotFound)
                    result = MFTestResults.Fail;
            }

            try
            {
                foreach(string file in Directory.EnumerateFileSystemEntries(IOTests.Volume.RootDirectory + "\\_DIR_NOT_EXIST"))
                {
                    Log.Comment(file);
                }
                result = MFTestResults.Fail;
            }
            catch (IOException e1)
            {
                if (e1.ErrorCode != IOException.IOExceptionErrorCode.DirectoryNotFound)
                    result = MFTestResults.Fail;
            }

            // make sure the directory was not created
            try
            {
                foreach (string file in Directory.EnumerateFileSystemEntries(IOTests.Volume.RootDirectory + "\\_DIR_NOT_EXIST"))
                {
                    Log.Comment(file);
                }
                result = MFTestResults.Fail;
            }
            catch (IOException e1)
            {
                if (e1.ErrorCode != IOException.IOExceptionErrorCode.DirectoryNotFound)
                    result = MFTestResults.Fail;
            }
           

            return result;
        }

        #endregion Test Cases
    }
}
