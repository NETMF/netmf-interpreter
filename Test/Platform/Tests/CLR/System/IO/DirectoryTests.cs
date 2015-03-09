/*---------------------------------------------------------------------
* FileTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 9/12/2007 4:51:10 PM 
* 
* Tests the basic functionality of Socket objects 
* ---------------------------------------------------------------------*/

using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.IO;
using Microsoft.SPOT.IO;

namespace Microsoft.SPOT.Platform.Tests
{

    public class DirectoryTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.                
            try 
            {
                IOTests.IntializeVolume();
            }
            catch 
            { 
                Log.Comment("Unable to format media, skipping class tests."); 
                return InitializeResult.Skip; 
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        [TestMethod]
        public MFTestResults DirectoryTest0_CreateDirectory()
        {
            bool testResult = false;

            try
            {
                DirectoryInfo di = Directory.CreateDirectory("DirectoryTest0_CreateDirectory");

                if (!Directory.Exists("DirectoryTest0_CreateDirectory"))
                {
                    throw new IOException("Directory not found");
                }

                testResult = true;
                
                Directory.Delete("DirectoryTest0_CreateDirectory");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectorytTest1_CreateAndDeleteDirectory()
        {
            bool testResult = false;

            try
            {
                DirectoryInfo di = Directory.CreateDirectory("DirectorytTest1_CreateAndDeleteDirectory");

                if (!Directory.Exists("DirectorytTest1_CreateAndDeleteDirectory"))
                {
                    throw new IOException("Directory not found");
                }

                Directory.Delete("DirectorytTest1_CreateAndDeleteDirectory");

                if (File.Exists("DirectorytTest1_CreateAndDeleteDirectory.txt"))
                {
                    throw new IOException("Directory was not deleted");
                }

                testResult = true;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryTest2_CreateExistingDirectory()
        {
            bool testResult = false;

            try
            {
                DirectoryInfo di = Directory.CreateDirectory("DirectoryTest2_CreateExistingDirectory");

                if (!Directory.Exists("DirectoryTest2_CreateExistingDirectory"))
                {
                    throw new IOException("Directory not found");
                }

                try
                {
                    DirectoryInfo di1 = Directory.CreateDirectory("DirectoryTest2_CreateExistingDirectory");
                    testResult = true;
                }
                finally
                {
                    Directory.Delete("DirectoryTest2_CreateExistingDirectory");
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Attempt creating a directory when a file with same name exists.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_CreateDirectory1()
        {
            bool testResult = false;

            try
            {
                FileStream fooclose_test1 = File.Create("DirectoryTest_CreateDirectory1");
                fooclose_test1.Close();
                
                try
                {
                    Directory.CreateDirectory("DirectoryTest_CreateDirectory1");
                    throw new Exception("Non empty directory deleted.");
                }
                catch (Exception)
                {
                    testResult = true;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Attempt deleting non empty directory.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_DeleteDirectory1()
        {
            bool testResult = false;

            try
            {
                DirectoryInfo di = Directory.CreateDirectory("DirectoryTest_DeleteDirectory1");

                FileStream fooclose_test1 = File.Create("DirectoryTest_DeleteDirectory1\\file-1.txt");
                fooclose_test1.Close();

                try
                {
                    Directory.Delete("DirectoryTest_DeleteDirectory1");
                    throw new Exception("Non empty directory deleted.");
                }
                catch (Exception)
                {
                    testResult = true;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Attempt deleting directory with sub directory.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_DeleteDirectory2()
        {
            bool testResult = false;

            try
            {
                Directory.CreateDirectory("DirectoryTest_DeleteDirectory2");
                Directory.CreateDirectory("DirectoryTest_DeleteDirectory2\\SubDir");

                try
                {
                    Directory.Delete("DirectoryTest_DeleteDirectory2");
                    throw new Exception("Non empty directory deleted.");
                }
                catch (Exception)
                {
                    testResult = true;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryTest_DeleteDirectory3()
        {
            bool testResult = false;

            try
            {
                Directory.CreateDirectory("DirectoryTest_DeleteDirectory3");
                Directory.CreateDirectory("DirectoryTest_DeleteDirectory3\\SubDir1");
                Directory.CreateDirectory("DirectoryTest_DeleteDirectory3\\SubDir2");

                FileStream fooclose_test1 = File.Create("DirectoryTest_DeleteDirectory3\\SubDir1\\file-1.txt");
                fooclose_test1.Close();

                FileStream fooclose_test2 = File.Create("DirectoryTest_DeleteDirectory3\\SubDir2\\file-2.txt");
                fooclose_test2.Close();

                Directory.Delete("DirectoryTest_DeleteDirectory3", true);

                testResult = true;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Delete a directory that does not exist.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_DeleteDirectory4()
        {
            bool testResult = true;

            try
            {
                Directory.Delete("DirectoryTest_DeleteDirectory4");
                testResult = false;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Simple move empty directory.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_MoveDirectory0()
        {
            bool testResult = true;

            try
            {
                Directory.CreateDirectory("DirectoryTest_MoveDirectory0");
                Directory.Move("DirectoryTest_MoveDirectory0", "DirectoryTest_MoveDirectory0-1");

                if (Directory.Exists("DirectoryTest_MoveDirectory0"))
                    throw new IOException("Source not moved");

                if (!Directory.Exists("DirectoryTest_MoveDirectory0-1"))
                    throw new IOException("Destination not found");
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Simple move directory with files.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_MoveDirectory1()
        {
            bool testResult = true;

            try
            {
                Directory.CreateDirectory("DirectoryTest_MoveDirectory1");

                FileStream fooclose_test1 = File.Create("DirectoryTest_MoveDirectory1\\file-1.txt");
                fooclose_test1.Close();

                Directory.Move("DirectoryTest_MoveDirectory1", "DirectoryTest_MoveDirectory1-1");

                if (Directory.Exists("DirectoryTest_MoveDirectory1"))
                    throw new IOException("Source not moved");

                if (!Directory.Exists("DirectoryTest_MoveDirectory1-1"))
                    throw new IOException("Destination not found");
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Simple move directory with directory substructure.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_MoveDirectory2()
        {
            bool testResult = true;

            try
            {
                Directory.CreateDirectory("DirectoryTest_MoveDirectory2");
                Directory.CreateDirectory("DirectoryTest_MoveDirectory2\\sub1");
                Directory.CreateDirectory("DirectoryTest_MoveDirectory2\\sub2");

                FileStream fooclose_test1 = File.Create("DirectoryTest_MoveDirectory2\\sub1\\file-1.txt");
                fooclose_test1.Close();

                Directory.Move("DirectoryTest_MoveDirectory2", "DirectoryTest_MoveDirectory2-1");

                if (Directory.Exists("DirectoryTest_MoveDirectory2"))
                    throw new IOException("Source not moved");

                if (!Directory.Exists("DirectoryTest_MoveDirectory2-1"))
                    throw new IOException("Destination not found");
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        /// <summary>
        /// Create-Move-ReCreate directory.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_MoveDirectory3()
        {
            bool testResult = true;

            try
            {
                Directory.CreateDirectory("DirectoryTest_MoveDirectory3");
                Directory.Move("DirectoryTest_MoveDirectory3", "DirectoryTest_MoveDirectory3-1");
                Directory.CreateDirectory("DirectoryTest_MoveDirectory3");
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Move dir when target exists.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_MoveDirectory4()
        {
            bool testResult = true;

            try
            {
                Directory.CreateDirectory("DirectoryTest_MoveDirectory4");
                Directory.CreateDirectory("DirectoryTest_MoveDirectory4-1");

                try
                {
                    Directory.Move("DirectoryTest_MoveDirectory4", "DirectoryTest_MoveDirectory4-1");
                    testResult = false;
                }
                catch (IOException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Move dir when source does not exist.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_MoveDirectory5()
        {
            bool testResult = true;

            try
            {
                try
                {
                    Directory.Move("DirectoryTest_MoveDirectory5", "DirectoryTest_MoveDirectory5-1");

                    testResult = false;
                }
                catch (IOException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Move dir when source exists as a file.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_MoveDirectory6()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test1 = File.Create("DirectoryTest_MoveDirectory6");
                fooclose_test1.Close();

                try
                {
                    Directory.Move("DirectoryTest_MoveDirectory6", "DirectoryTest_MoveDirectory6-1");

                    /// ISSUE: This appears to succeed, need to verify whether this is by design.
                    testResult = false;
                }
                catch (IOException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Move dir when destination exists as a file.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_MoveDirectory7()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test1 = File.Create("DirectoryTest_MoveDirectory7-1");
                fooclose_test1.Close();

                try
                {
                    Directory.Move("DirectoryTest_MoveDirectory7", "DirectoryTest_MoveDirectory7-1");
                    testResult = false;
                }
                catch (IOException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults DirectoryTest_Exist1()
        {
            bool testResult = false;

            try
            {
                testResult = !Directory.Exists("DirectoryTest_Exist1");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryTest_Exist2()
        {
            bool testResult = false;

            try
            {
                testResult = Directory.Exists("\\");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryTest_Exist3()
        {
            bool testResult = false;

            try
            {
                testResult = Directory.Exists(IOTests.Volume.RootDirectory);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryTest_GetFiles()
        {
            bool testResult = false;

            try
            {
                Directory.CreateDirectory("DirectoryTest_GetFiles");

                FileStream fooclose_test1 = File.Create("DirectoryTest_GetFiles\\file-1.txt");
                fooclose_test1.Close();

                FileStream fooclose_test2 = File.Create("DirectoryTest_GetFiles\\file-2.txt");
                fooclose_test2.Close();

                FileStream fooclose_test3 = File.Create("DirectoryTest_GetFiles\\file-3.txt");
                fooclose_test3.Close();

                string[] files = Directory.GetFiles("DirectoryTest_GetFiles");
                if (files.Length != 3)
                    throw new Exception("Incorrect number of files");

                testResult = true;

                testResult = testResult && (String.Equals(files[0], IOTests.Volume.RootDirectory + "\\DirectoryTest_GetFiles\\file-1.txt"));
                testResult = testResult && (String.Equals(files[1], IOTests.Volume.RootDirectory + "\\DirectoryTest_GetFiles\\file-2.txt"));
                testResult = testResult && (String.Equals(files[2], IOTests.Volume.RootDirectory + "\\DirectoryTest_GetFiles\\file-3.txt"));
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryTest_GetCurrentDirectory()
        {
            bool testResult = false;

            try
            {
                string curDir = Directory.GetCurrentDirectory();
                testResult = String.Equals(curDir, IOTests.Volume.RootDirectory);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryTest_SetCurrentDirectory0()
        {
            bool testResult = false;

            try
            {
                Directory.CreateDirectory(IOTests.Volume.RootDirectory + "\\DirectoryTest_SetCurrentDirectory0");
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory + "\\DirectoryTest_SetCurrentDirectory0");
                string curDir = Directory.GetCurrentDirectory();
                testResult = String.Equals(curDir, IOTests.Volume.RootDirectory + "\\DirectoryTest_SetCurrentDirectory0");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Set current directory to a non existent one.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryTest_SetCurrentDirectory1()
        {
            bool testResult = true;

            try
            {
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory + "\\DirectoryTest_SetCurrentDirectory1");
                testResult = false;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}