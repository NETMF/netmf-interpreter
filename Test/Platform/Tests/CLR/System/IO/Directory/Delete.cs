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
                Directory.CreateDirectory("CreateDirectory");
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

        #region Local vars
        private const string DIR = @"DelDir";
        private bool createFile = false;

        #endregion Local vars

        #region Helper functions
        private bool TestDelete(string delete)
        {
            return TestDelete(delete, delete);
        }
        private bool TestDelete(string delete, bool recursive)
        {
            return TestDelete(delete, delete, recursive);
        }

        private bool TestDelete(string delete, string create)
        {
            Log.Comment("Delete dir: " + delete); 
            
            if (!CreateDir(create))
                return false;

            Directory.Delete(delete);
            return VerifyDelete(delete);
        }
        private bool TestDelete(string delete, string create, bool recursive)
        {
            Log.Comment("Delete dir: " + delete);
            
            if (!CreateDir(create))
                return false;
            try
            {
                Directory.Delete(delete, recursive);
            }
            catch (IOException ex)
            {
                if (recursive && IOTests.Volume.FileSystem == "WINFS")
                {
                    Log.Exception("WINFS has bug where recurisve may fail if indexer or Virus has handle open.  Wait and try again.");
                    for (int i = 0; i < 10; i++)
                    {
                        System.Threading.Thread.Sleep(1000);
                        try { Directory.Delete(DIR, true); }
                        catch { }
                        if (VerifyDelete(DIR))
                        {
                            return true;
                        }
                    }
                }
                throw ex;
            }
            return VerifyDelete(delete);
        }
        private bool VerifyDelete(string path)
        {
            if (Directory.Exists(path))
            {
                Log.Exception("Failed to Delete " + path);
                return false;
            }
            return true;
        }
        private bool CreateDir(string path)
        {
            DirectoryInfo dir2 = Directory.CreateDirectory(path);
            if (!Directory.Exists(path))
            {
                Log.Exception("Failed to Create " + dir2.FullName);
                return false;
            }
            if (createFile)
            {
                using (FileStream fs = new FileStream(dir2.FullName + @"\temp.txt", FileMode.Create)) { }
                if (!File.Exists(dir2.FullName + @"\temp.txt"))
                {
                    Log.Exception("Failed to create file");
                    return false;
                }
            }
            return true;
        }
        #endregion Helper functions

        #region Test Cases
        [TestMethod]
        public MFTestResults DelCurrentDir()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    //Currently this causes hang
                    Directory.Delete(".", false);
                    Log.Exception("Should not be able to delete current directory");
                    result = MFTestResults.Fail;
                }
                catch (IOException) {/* pass case */}
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults DelNonExistent()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Directory.Delete("ThisDoesNotExist");
                    Log.Exception("Should not be able to delete nonexistent directory");
                    result = MFTestResults.Fail;
                }
                catch (IOException) {/* pass case */}
                try
                {
                    Directory.Delete("ThisDoesNotExist", false);
                    Log.Exception("Should not be able to delete nonexistent directory");
                    result = MFTestResults.Fail;
                }
                catch (IOException) {/* pass case */}
                try
                {
                    Directory.Delete("ThisDoesNotExist", true);
                    Log.Exception("Should not be able to delete nonexistent directory");
                    result = MFTestResults.Fail;
                }
                catch (IOException) {/* pass case */}
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults DeleteDirNoSub()
        {
            MFTestResults result = MFTestResults.Pass;
            createFile = false;
            try
            {
                Log.Comment("Delete");
                if (!TestDelete(DIR))
                    result = MFTestResults.Fail;

                Log.Comment("Delete non-recursive");
                if (!TestDelete(DIR, false))
                    result = MFTestResults.Fail;

                Log.Comment("Delete recursive");
                if (!TestDelete(DIR, true))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults DeleteDirSub()
        {
            MFTestResults result = MFTestResults.Pass;
            createFile = false;
            try
            {
                Log.Comment("Delete - should throw IOException");
                try
                {
                    TestDelete(DIR, DIR + @"\SubTest1");
                    Log.Comment("Expected IO Exception because of subdir");
                    result = MFTestResults.Fail;
                }
                catch (IOException) {/* pass case */}

                Log.Comment("Delete non-recursive - should throw IOException");
                try
                {
                    TestDelete(DIR, DIR + @"\SubTest2", false);
                    Log.Comment("Expected IO Exception because of subdir");
                    result = MFTestResults.Fail;
                }
                catch (IOException) {/* pass case */}

                Log.Comment("Delete recursive");
                if (!TestDelete(DIR, DIR + @"\SubTest3", true))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults DeleteDirNoSubWithFile()
        {
            MFTestResults result = MFTestResults.Pass;
            createFile = true;
            try
            {
                try
                {
                    Log.Comment("Delete");
                    TestDelete(DIR);
                    Log.Comment("Expected IO Exception because of subdir");
                    result = MFTestResults.Fail;
                }
                catch (IOException) {/* pass case */}

                try
                {
                    Log.Comment("Delete non-recursive");
                    TestDelete(DIR, false);
                    Log.Comment("Expected IO Exception because of subdir");
                    result = MFTestResults.Fail;
                }
                catch (IOException) {/* pass case */}

                Log.Comment("Delete recursive");
                if (!TestDelete(DIR, true))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults DeleteDirSubWithFiles()
        {
            MFTestResults result = MFTestResults.Pass;
            createFile = true;
            try
            {
                Log.Comment("Delete - should throw IOException");
                try
                {
                    TestDelete(DIR, DIR + @"\SubFileTest1");
                    Log.Comment("Expected IO Exception because of subdir");
                    result = MFTestResults.Fail;
                }
                catch (IOException) {/* pass case */}

                Log.Comment("Delete non-recursive - should throw IOException");
                try
                {
                    TestDelete(DIR, DIR + @"\SubFileTest2", false);
                    Log.Comment("Expected IO Exception because of subdir");
                    result = MFTestResults.Fail;
                }
                catch (IOException) {/* pass case */}

                Log.Comment("Delete recursive");
                if (!TestDelete(DIR, DIR + @"\SubFileTest3", true))
                    result = MFTestResults.Fail;

                Log.Comment("Delete deep recursive");
                CreateDir(DIR + @"\SubFileTest4");
                CreateDir(DIR + @"\SubFileTest4\Level1a");
                CreateDir(DIR + @"\SubFileTest4\Level1b");
                CreateDir(DIR + @"\SubFileTest4\Level1a\sub1\deep1");
                if (!TestDelete(DIR, DIR + @"\SubFileTest4\Level1a\sub1\deep2", true))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults DeleteFileInUse()
        {
            MFTestResults result = MFTestResults.Pass;
            createFile = false;
            Log.Comment("Try to delete dir with file in use");
            string dir = IOTests.Volume.RootDirectory + @"\DeleteFileInUse";
            if (!CreateDir(dir))
                result = MFTestResults.Fail;

            FileStream fs = new FileStream(dir + @"\temp.txt", FileMode.Create);
            try
            {
                Directory.Delete(dir, true);
                Log.Exception("Expected IOException because file was in use");
            }
            catch (IOException) { /*pass case*/ }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            Log.Comment("Close file and delete dir");
            fs.Close();
            try
            {
                Directory.Delete(dir, true);
                if (!VerifyDelete(dir))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }
            return result;
        }
        // Need attribute support to finish
        [TestMethod]
        public MFTestResults DeleteReadOnlyDir()
        {
            MFTestResults result = MFTestResults.Pass;
            createFile = false;
            string dir = IOTests.Volume.RootDirectory + @"\DeleteReadOnlyDir";
            DirectoryInfo dir2 = Directory.CreateDirectory(dir);
            File.SetAttributes(dir, FileAttributes.ReadOnly);
            try
            {
                try
                {
                    Directory.Delete(dir);
                    Log.Exception("Shouldn't be able to delete ReadOnly directory");
                    result = MFTestResults.Fail;
                }
                catch (IOException) { /*pass case*/ }
                try
                {
                    Directory.Delete(dir, true);
                    Log.Exception("Shouldn't be able to delete ReadOnly directory");
                    result = MFTestResults.Fail;
                }
                catch (IOException) { /*pass case*/ }
                try
                {
                    Directory.Delete(dir, false);
                    Log.Exception("Shouldn't be able to delete ReadOnly directory");
                    result = MFTestResults.Fail;
                }
                catch (IOException) { /*pass case*/ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                File.SetAttributes(dir, FileAttributes.Normal);
            }
            return result;
        }

        [TestMethod]
        public MFTestResults DeleteHiddenDir()
        {
            MFTestResults result = MFTestResults.Pass;
            createFile = false;
            DirectoryInfo dir2;
            try
            {
                string dirName = DIR + @"\HiddenDir1";
                dir2 = Directory.CreateDirectory(dirName);
                File.SetAttributes(dirName, FileAttributes.Hidden);
                Directory.Delete(dirName);
                if (!VerifyDelete(dirName))
                    result = MFTestResults.Fail;

                dir2 = Directory.CreateDirectory(dirName);
                File.SetAttributes(dirName, FileAttributes.Hidden);
                Directory.Delete(dirName, true);
                if (!VerifyDelete(dirName))
                    result = MFTestResults.Fail;

                dir2 = Directory.CreateDirectory(dirName);
                File.SetAttributes(dirName, FileAttributes.Hidden);
                Directory.Delete(dirName, false);
                if (!VerifyDelete(dirName))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }
            return result;
        }

        #endregion Test Cases
    }
}
