////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Delete : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.                
            try
            {
                IOTests.IntializeVolume();

                Directory.CreateDirectory(sourceDir);
                Directory.CreateDirectory("Test " + sourceDir);
                Directory.SetCurrentDirectory(sourceDir);
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

        #region Local vars
        private const string file1Name = "file1.tmp";
        private const string file2Name = "file2.txt";
        private const string sourceDir = "source";
        #endregion Local vars

        #region Helper methods

        private bool TestDelete(string file)
        {
            bool success = true;
            Log.Comment("Deleting " + file);
            if (!File.Exists(file))
            {
                Log.Comment("Create " + file);
                File.Create(file).Close();
                if (!File.Exists(file))
                {
                    Log.Exception("Could not find file after creation!");
                    success = false;
                }
            }
            File.Delete(file);
            if (File.Exists(file))
            {
                Log.Exception("File still exists after delete!");
                success = false;
            }

            return success;
        }
        #endregion Helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults ArgumentExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Current Directory: " + Directory.GetCurrentDirectory());
                try
                {
                    Log.Comment("Null argument");
                    File.Delete(null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentNullException) { /* pass case */ }

                try
                {
                    Log.Comment("String.Empty argument");
                    File.Delete(string.Empty);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Whitespace argument");
                    File.Delete("       ");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("*.* argument");
                    File.Delete("*.*");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Current dir '.' argument");
                    File.Delete(".");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (IOException) { /* pass case */ } // UnauthorizedAccess 

                try
                {
                    Log.Comment("parent dir '..' argument");
                    File.Delete("..");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (IOException) { /* pass case */ } // UnauthorizedAccess 
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults IOExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            FileStream fs = null;
            try
            {
                Log.Comment("Current Directory: " + Directory.GetCurrentDirectory());
                try
                {
                    Log.Comment("non-existent file");
                    File.Delete("non-existent.file");
                    /// No exception is thrown for non existent file.
                }
                catch (IOException) 
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Unexpected IOException");
                }

                try
                {
                    Log.Comment("Read only file");
                    File.Create(file1Name).Close();
                    File.SetAttributes(file1Name, FileAttributes.ReadOnly);
                    File.Delete(file1Name);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (IOException) { /* pass case */ } // UnauthorizedAccess 
                finally
                {
                    if (File.Exists(file1Name))
                    {
                        Log.Comment("Clean up read only file");
                        File.SetAttributes(file1Name, FileAttributes.Normal);
                        File.Delete(file1Name);
                    }
                }

                try
                {
                    Log.Comment("file in use");
                    fs = File.Create(file1Name);
                    File.Delete(file1Name);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (IOException) { /* pass case */ }
                finally
                {
                    if (fs != null)
                    {
                        Log.Comment("Clean up file in use");
                        fs.Close();
                        File.Delete(file1Name);
                    }
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
        public MFTestResults ValidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            FileStream fs = null;
            try
            {
                Log.Comment("Current Directory: " + Directory.GetCurrentDirectory());

                Log.Comment("relative delete");
                if (!TestDelete(file1Name))
                    result = MFTestResults.Fail;

                Log.Comment("absolute delete");
                if (!TestDelete(Directory.GetCurrentDirectory() + "\\" + file1Name))
                    result = MFTestResults.Fail;

                Log.Comment("Case insensitive lower delete");
                File.Create(file1Name).Close();
                if (!TestDelete(file1Name.ToLower()))
                    result = MFTestResults.Fail;

                Log.Comment("Case insensitive UPPER delete");
                File.Create(file2Name).Close();
                if (!TestDelete(file2Name.ToUpper()))
                    result = MFTestResults.Fail; 
                
                Log.Comment("Write content to file");
                byte[] hello = UTF8Encoding.UTF8.GetBytes("Hello world!");
                fs = File.Create(file1Name);
                fs.Write(hello, 0, hello.Length);
                fs.Close();
                if (!TestDelete(file1Name))
                    result = MFTestResults.Fail;

                Log.Comment("relative . delete");
                File.Create(file2Name).Close();
                if (!TestDelete(@".\" + file2Name))
                    result = MFTestResults.Fail;

                Log.Comment("relative .. delete");
                File.Create(Path.Combine(IOTests.Volume.RootDirectory, file2Name)).Close();
                if (!TestDelete(@"..\" + file2Name))
                    result = MFTestResults.Fail;

                Log.Comment("hidden file delete");
                File.Create(file1Name).Close();
                File.SetAttributes(file1Name, FileAttributes.Hidden);
                if (!TestDelete(file1Name))
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
        public MFTestResults SpecialFileNames()
        {
            MFTestResults result = MFTestResults.Pass;
            char[] special = new char[] { '!', '#', '$', '%', '\'', '(', ')', '+', '-', '.', '@', '[', ']', '_', '`', '{', '}', '~' };

            try
            {
                Log.Comment("Create file each with special char file names");
                for (int i = 0; i < special.Length; i++)
                {
                    string file = i + "_" + new string(new char[] { special[i] }) + "_z.file";
                    if (!TestDelete(file))
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

        #endregion Test Cases
    }
}
