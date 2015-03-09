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

    public class FileTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("The following tests are located in FileTests.cs");

            // delete the directory DOTNETMF_FS_EMULATION
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
        public MFTestResults FiletTest_CreateFile0()
        {
            bool testResult = true;

            try
            {
                if (File.Exists("fooclose_test0.txt"))
                {
                    Debug.Print( "hello " );
                }

                FileStream fooclose_test0 = File.Create("fooclose_test0.txt");

                fooclose_test0.Close();

                if (!File.Exists("fooclose_test0.txt"))
                {
                    throw new IOException("File not found");
                }

                FileStream foodispose_test0 = File.Create("foodispose_test0.txt");

                foodispose_test0.Dispose();

                if (!File.Exists("foodispose_test0.txt"))
                {
                    throw new IOException("File not found");
                }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Create twice.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTest_CreateFile1()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test1 = File.Create("FiletTest_CreateFile1.txt");

                try
                {
                    FileStream fooclose_test2 = File.Create("FiletTest_CreateFile1.txt");
                    testResult = false;
                }
                catch (IOException)
                {
                }

                fooclose_test1.Close();
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Close twice. This should not throw exception.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTest_CloseFile0()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test0 = File.Create("fooclose_test0.txt");

                fooclose_test0.Close();
                fooclose_test0.Close();
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Creat-close-delete many times.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTest_CloseFile1()
        {
            bool testResult = true;

            try
            {
                int i = 0;

                for (i = 0; i < 10; i++)
                {
                    FileStream fooclose_test0 = File.Create("fooclose_test0.txt");

                    fooclose_test0.Close();

                    File.Delete("fooclose_test0.txt");
                }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults FiletTest1_CreateAndDeleteFile()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test1 = File.Create("fooclose_test1.txt");

                fooclose_test1.Close();

                if (!File.Exists("fooclose_test1.txt"))
                {
                    throw new IOException("File not found");
                }

                File.Delete("fooclose_test1.txt");

                if (File.Exists("fooclose_test1.txt"))
                {
                    throw new IOException("File was not deleted");
                }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults FiletTest2_CreateAndDeleteFileInUse()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test2 = File.Create("fooclose_test2.txt");

                if (!File.Exists("fooclose_test2.txt"))
                {
                    throw new IOException("File not found");
                }

                /// This delete attempt should fail.

                try
                {
                    File.Delete("fooclose_test2.txt");
                    testResult = false;
                    Log.Exception("In use file was deleted");
                }
                catch (IOException)
                {
                    testResult = true;
                }

                fooclose_test2.Close();
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Delete non existent file.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTest_DeleteFile1()
        {
            bool testResult = true;

            try
            {
                Log.Comment("Delete non-existing file should not throw");
                File.Delete("FiletTest_DeleteFile1.txt");
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Delete file in non existent directory.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTest_DeleteFile2()
        {
            bool testResult = true;

            try
            {
                if (Directory.Exists("Del2"))
                    throw new IOException("Del2 dir not supposed to exist.");

                /// This delete attempt should fail.  Bug # 22012
                try
                {
                    File.Delete("Del2\\FiletTest_DeleteFile2.txt");
                    testResult = false;
                    Log.Exception("File deleted when parent directory does not exist");
                }
                catch (IOException)
                {
                    testResult = true;
                }

            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Gentle move file (valid source and destination).
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTest_MoveFile0()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test1 = File.Create("fooclose_test3-1.txt");

                fooclose_test1.Close();

                if (!File.Exists("fooclose_test3-1.txt"))
                {
                    throw new IOException("File not found");
                }

                File.Move("fooclose_test3-1.txt", "fooclose_test3-2.txt");

                if (!File.Exists("fooclose_test3-2.txt"))
                {
                    throw new IOException("Moved file not found");
                }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        /// <summary>
        /// Source file does not exist.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTest_MoveFile1()
        {
            bool testResult = true;

            try
            {
                if (File.Exists("fooclose_test4-1.txt"))
                    throw new IOException("fooclose_test4-1.txt not supposed to exist.");

                try
                {
                    File.Move("fooclose_test4-1.txt", "fooclose_test4-2.txt");

                    /// File.Move should fail above.
                }
                catch (IOException)
                {
                    testResult = true;
                }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }
        
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Destination path does not exist.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTest_MoveFile2()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test1 = File.Create("fooclose_test5-1.txt");

                fooclose_test1.Close();

                if (!File.Exists("fooclose_test5-1.txt"))
                {
                    throw new IOException("File not found");
                }

                try
                {
                    File.Move("fooclose_test5-1.txt", "dir5\\fooclose_test5-2.txt");

                    if (File.Exists("fooclose_test5-2.txt"))
                    {
                        testResult = false;
                        Log.Exception("Moved to non existent directory");
                    }
                }
                catch (IOException)
                {
                }                
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Source file is in use.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTes_MoveFile3()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test1 = File.Create("fooclose_test6-1.txt");

                if (!File.Exists("fooclose_test6-1.txt"))
                {
                    throw new IOException("File not found");
                }

                try
                {
                    File.Move("fooclose_test6-1.txt", "fooclose_test6-2.txt");
                }
                catch (IOException)
                {
                }

                if (!File.Exists("fooclose_test6-1.txt"))
                {
                    throw new IOException("File in use moved.");
                }

                fooclose_test1.Close();
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Destination file already exists.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTest_MoveFile4()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test1 = File.Create("fooclose_test7-1.txt");
                fooclose_test1.Close();

                FileStream fooclose_test2 = File.Create("fooclose_test7-2.txt");
                fooclose_test2.Close();

                if (!File.Exists("fooclose_test7-1.txt"))
                {
                    throw new IOException("File not found");
                }

                try
                {
                    File.Move("fooclose_test7-1.txt", "fooclose_test7-2.txt");
                }
                catch (IOException)
                {
                }

                if (!File.Exists("fooclose_test7-1.txt"))
                {
                    throw new IOException("File in use moved.");
                }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Create-Move-Delete-then create both again.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FiletTest_MoveFile6()
        {
            bool testResult = true;

            try
            {
                FileStream fooclose_test1 = File.Create("FiletTest_MoveFile6-1.txt");
                fooclose_test1.Close();

                File.Move("FiletTest_MoveFile6-1.txt", "FiletTest_MoveFile6-2.txt");
                File.Delete("FiletTest_MoveFile6-2.txt");

                FileStream fooclose_test2 = File.Create("FiletTest_MoveFile6-2.txt");
                fooclose_test2.Close();

                FileStream fooclose_test3 = File.Create("FiletTest_MoveFile6-1.txt");
                fooclose_test3.Close();

            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}