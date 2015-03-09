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

    public class DirectoryInfoTests : IMFTestInterface
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

        /// <summary>
        /// Make sure DirectoryInfo could be constructed even when directory itself does not exist.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryInfoTest_Constructor0()
        {
            bool testResult = false;

            try
            {
                DirectoryInfo di = new DirectoryInfo("DirectoryDoesntExist");
                testResult = true;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Try creating with a bad path.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryInfoTest_Constructor1()
        {
            bool testResult = true;

            try
            {
                DirectoryInfo di = new DirectoryInfo("*2%&^$%#^%?");
                testResult = false;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Validate Name property
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryInfoTest_Name0()
        {
            MFTestResults testResult = MFTestResults.Pass;

            try
            {
                DirectoryInfo di = new DirectoryInfo(IOTests.Volume.RootDirectory + "\\dir1\\dir2");
                if (!String.Equals(di.Name, "dir2"))
                    testResult = MFTestResults.Fail;

                if (!String.Equals(di.FullName, IOTests.Volume.RootDirectory + "\\dir1\\dir2"))
                    testResult = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return testResult;
        }

        /// <summary>
        /// Validate Parent property
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryInfoTest_Parent0()
        {
            bool testResult = true;

            try
            {
                DirectoryInfo di = new DirectoryInfo(IOTests.Volume.RootDirectory + "\\dir1\\dir2");
                testResult = (di.Parent != null);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Validate Name property
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults DirectoryInfoTest_Parent1()
        {
            bool testResult = true;

            try
            {
                DirectoryInfo di = new DirectoryInfo(IOTests.Volume.RootDirectory + "\\dir1\\dir2");
                testResult = String.Equals(di.Parent.FullName, IOTests.Volume.RootDirectory + "\\dir1");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_SubDirectory0()
        {
            bool testResult = true;

            try
            {
                DirectoryInfo di = new DirectoryInfo(IOTests.Volume.RootDirectory);
                di.CreateSubdirectory("DirectoryInfoTest_SubDirectory0");
                testResult = Directory.Exists(IOTests.Volume.RootDirectory + "\\DirectoryInfoTest_SubDirectory0");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_Create0()
        {
            bool testResult = true;

            try
            {
                DirectoryInfo di = new DirectoryInfo(IOTests.Volume.RootDirectory + "\\DirectoryInfoTest_Create0");
                di.Create();
                testResult = Directory.Exists(IOTests.Volume.RootDirectory + "\\DirectoryInfoTest_Create0");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_Exists0()
        {
            bool testResult = true;

            try
            {
                Directory.CreateDirectory("DirectoryInfoTest_Exists0");
                DirectoryInfo di = new DirectoryInfo("DirectoryInfoTest_Exists0");
                testResult = di.Exists;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_Exists1()
        {
            bool testResult = true;

            try
            {
                DirectoryInfo di = new DirectoryInfo("DirectoryInfoTest_Exists1");
                testResult = !di.Exists;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_Delete0()
        {
            bool testResult = true;

            try
            {
                Directory.CreateDirectory("DirectoryInfoTest_Delete0");
                DirectoryInfo di = new DirectoryInfo("DirectoryInfoTest_Delete0");
                testResult = di.Exists;

                di.Delete();
                testResult = !di.Exists;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_Delete1()
        {
            bool testResult = true;

            try
            {
                Directory.CreateDirectory("DirectoryInfoTest_Delete1");
                FileStream file = File.Create("DirectoryInfoTest_Delete1\\file-1.txt");
                file.Close();

                DirectoryInfo di = new DirectoryInfo("DirectoryInfoTest_Delete1");
                testResult = di.Exists;

                di.Delete(true);
                testResult = !di.Exists;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_Attribute0()
        {
            bool testResult = true;

            try
            {
                Directory.CreateDirectory("DirectoryInfoTest_Attribute0");

                DirectoryInfo di = new DirectoryInfo("DirectoryInfoTest_Attribute0");
                testResult = ((di.Attributes & FileAttributes.Directory) != 0);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_CreationTime0()
        {
            bool testResult = false;

            try
            {
                Directory.CreateDirectory("DirectoryInfoTest_CreationTime0");

                DirectoryInfo fi = new DirectoryInfo("DirectoryInfoTest_CreationTime0");
                testResult = (fi.CreationTime.Ticks != 0);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_LastAccessTime0()
        {
            // do not run on Windows
            if (Directory.GetCurrentDirectory().IndexOf("WINFS") != -1)
            {
                return MFTestResults.Skip;
            }

            bool testResult = false;

            DirectoryInfo fi1, fi2;

            string dirName = "DirectoryInfoTest_LastAccessTime" + Guid.NewGuid().ToString() + "_0";

            fi1 = Directory.CreateDirectory(dirName);

            testResult = (fi1.LastAccessTime.Ticks != 0);

            // add one day 
            Microsoft.SPOT.Hardware.Utility.SetLocalTime(DateTime.Now.AddDays(1));

            fi1.CreateSubdirectory("AnotherDir" + Guid.NewGuid().ToString());

            fi2 = new DirectoryInfo(dirName);
            testResult = (fi2.LastAccessTime - fi1.LastAccessTime >= new TimeSpan(1,0,0,0));                
            
            Log.Comment("First access : " + fi1.LastAccessTime.ToString());
            Log.Comment("Second access: " + fi2.LastAccessTime.ToString());

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_LastWriteTime0()
        {
            bool testResult = false;

            DirectoryInfo di, fi1, fi2;
            int sleep = 2000;
            try
            {
                {
                    di = Directory.CreateDirectory("DirectoryInfoTest_LastWriteTime0");

                    fi1 = new DirectoryInfo("DirectoryInfoTest_LastWriteTime0");
                    testResult = (fi1.LastWriteTime.Ticks != 0);
                }

                System.Threading.Thread.Sleep(sleep);

                {
                    di.CreateSubdirectory("AnotherDirectory" + Guid.NewGuid().ToString());
                    
                    fi2 = new DirectoryInfo("DirectoryInfoTest_LastWriteTime0");

                    testResult = (fi2.LastWriteTime - fi1.LastWriteTime >= new TimeSpan(0, 0, 0, 0, 2000));
                }
                Log.Comment("First write : " + fi1.LastWriteTime.ToString());
                Log.Comment("Second write: " + fi2.LastWriteTime.ToString());
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults DirectoryInfoTest_GetFiles0()
        {
            bool testResult = false;

            try
            {
                Directory.CreateDirectory("DirectoryInfoTest_GetFiles0");

                FileStream fooclose_test1 = File.Create("DirectoryInfoTest_GetFiles0\\file-1.txt");
                fooclose_test1.Close();

                FileStream fooclose_test2 = File.Create("DirectoryInfoTest_GetFiles0\\file-2.txt");
                fooclose_test2.Close();

                FileStream fooclose_test3 = File.Create("DirectoryInfoTest_GetFiles0\\file-3.txt");
                fooclose_test3.Close();

                DirectoryInfo di = new DirectoryInfo("DirectoryInfoTest_GetFiles0");

                FileInfo[] files = di.GetFiles();
                if (files.Length != 3)
                    throw new Exception("Incorrect number of files");

                testResult = true;

                testResult = testResult && (String.Equals(files[0].FullName, IOTests.Volume.RootDirectory + "\\DirectoryInfoTest_GetFiles0\\file-1.txt"));
                testResult = testResult && (String.Equals(files[1].FullName, IOTests.Volume.RootDirectory + "\\DirectoryInfoTest_GetFiles0\\file-2.txt"));
                testResult = testResult && (String.Equals(files[2].FullName, IOTests.Volume.RootDirectory + "\\DirectoryInfoTest_GetFiles0\\file-3.txt"));
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}