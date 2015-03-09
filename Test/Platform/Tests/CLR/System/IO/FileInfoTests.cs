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

    public class FileInfoTests : IMFTestInterface
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
        /// Try with a file that does not exist.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Constructor0()
        {
            bool testResult = false;

            try
            {
                FileInfo fi = new FileInfo("FileThatNeverExists.txt");
                testResult = true;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Bad file name.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Constructor1()
        {
            bool testResult = true;

            try
            {
                FileInfo fi = new FileInfo("%$^%^%&**?");
                testResult = false;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Validate Name property.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Name()
        {
            bool testResult = false;

            try
            {
                FileInfo fi = new FileInfo(IOTests.Volume.RootDirectory + "\\dir\\FileName.txt");
                testResult = String.Equals(fi.Name, "FileName.txt");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Validate a zero lengthed file.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Length0()
        {
            bool testResult = false;

            try
            {
                FileStream file = File.Create("FileInfoTest_Length0.txt");
                file.Close();

                FileInfo fi = new FileInfo("FileInfoTest_Length0.txt");
                testResult = (fi.Length == 0);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Validate a non-zero lengthed file.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Length1()
        {
            bool testResult = false;

            try
            {
                FileStream file = File.Create("FileInfoTest_Length1.txt");
                file.Close();

                /// FUTURE: This will be done once Read/Write starts working.

                FileInfo fi = new FileInfo("FileInfoTest_Length1.txt");
                testResult = (fi.Length == 0);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Get length of a file that does not exist.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Length2()
        {
            bool testResult = false;

            try
            {
                FileInfo fi = new FileInfo("FileInfoTest_Length2.txt");
                testResult = (fi.Length == -1);
            }
            catch (IOException)
            {
                testResult = true;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Validate DirectoryName property.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_DirectoryName()
        {
            bool testResult = false;

            try
            {
                FileInfo fi = new FileInfo(IOTests.Volume.RootDirectory + "\\dir\\FileName.txt");
                testResult = String.Equals(fi.DirectoryName, IOTests.Volume.RootDirectory + "\\dir");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Validate Directory property is non-null.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Directory()
        {
            bool testResult = false;

            try
            {
                FileInfo fi = new FileInfo(IOTests.Volume.RootDirectory + "\\dir\\FileName.txt");
                DirectoryInfo di = fi.Directory;
                testResult = (di != null);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Validate Directory property is points to valid structure.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Directory1()
        {
            bool testResult = false;

            try
            {
                FileInfo fi = new FileInfo(IOTests.Volume.RootDirectory + "\\dir\\FileName.txt");
                DirectoryInfo di = fi.Directory;
                testResult = String.Equals(di.FullName, IOTests.Volume.RootDirectory + "\\dir");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        
        /// <summary>
        /// Validate file is created.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Create0()
        {
            bool testResult = false;

            try
            {
                string fullPath = Path.GetFullPath("FileInfoTest_Create0.txt");
                Log.Exception("FullPath = " + fullPath);
                FileInfo fi = new FileInfo("FileInfoTest_Create0.txt");
                FileStream fs = fi.Create();
                fs.Close();

                testResult = true;
                testResult = File.Exists("FileInfoTest_Create0.txt");                
                
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Validate file created file is deleted.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Delete0()
        {
            bool testResult = false;

            try
            {
                FileInfo fi = new FileInfo("FileInfoTest_Delete0.txt");
                FileStream fs = fi.Create();
                fs.Close();

                testResult = File.Exists("FileInfoTest_Delete0.txt");
                if (!testResult)
                    throw new IOException();

                fi.Delete();
                testResult = !File.Exists("FileInfoTest_Delete0.txt");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
         

        /// <summary>
        /// Validate existence check.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Exists0()
        {
            bool testResult = false;

            try
            {
                FileStream file = File.Create("FileInfoTest_Exists0.txt");
                file.Close();

                FileInfo fi = new FileInfo("FileInfoTest_Exists0.txt");
                testResult = fi.Exists;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
         

        /// <summary>
        /// Validate not existence check.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults FileInfoTest_Exists1()
        {
            bool testResult = false;

            try
            {
                FileInfo fi = new FileInfo("FileInfoTest_Exists1.txt");
                testResult = !fi.Exists;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults FileInfoTest_Attributes0()
        {
            bool testResult = false;

            try
            {
                FileStream file = File.Create("FileInfoTest_Attributes0.txt");
                file.Close();

                FileInfo fi = new FileInfo("FileInfoTest_Attributes0.txt");
                testResult = ((fi.Attributes & FileAttributes.Directory) == 0);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults FileInfoTest_CreationTime0()
        {
            // do not run on Windows
            if (Directory.GetCurrentDirectory().IndexOf("WINFS") != -1)
            {
                return MFTestResults.Skip;
            }

            bool testResult = false;

            try
            {
                string name = "FileInfoTest_CreationTime" + Guid.NewGuid().ToString() + "_0.txt";

                DateTime now = DateTime.Now;

                // now wil have more precisin, therefore potentially larger
                // Creation time is limited to seconds
                System.Threading.Thread.Sleep(5000);

                FileStream file = File.Create(name);
                file.Close();

                FileInfo fi = new FileInfo(name);
                testResult = (fi.CreationTime >= now);

                Log.Comment("Time at creation: " + now.ToString());
                Log.Comment("Creation time : " + fi.CreationTime.ToString());
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults FileInfoTest_LastAccessTime0()
        {
            // do not run on Windows
            if (Directory.GetCurrentDirectory().IndexOf("WINFS") != -1)
            {
                return MFTestResults.Skip;
            }

            bool testResult = false;
 
            try
            {
                FileInfo fi1, fi2;

                string name = "FileInfoTest_LastAccessTime" + Guid.NewGuid().ToString() + "_0.txt";

                using (FileStream file = File.Create(name))
                {
                    file.Close();
                    fi1 = new FileInfo(name);
                    testResult = (fi1.LastAccessTime.Ticks != 0);
                }
                
                // add one day 
                Microsoft.SPOT.Hardware.Utility.SetLocalTime(DateTime.Now.AddDays(1));

                using (FileStream again = File.Create(name))
                {
                    again.Close();
                    fi2 = new FileInfo(name);
                    testResult = (fi2.LastAccessTime - fi1.LastAccessTime >= new TimeSpan(1,0,0,0));
                }
                Log.Comment("First access : " + fi1.LastAccessTime.ToString());
                Log.Comment("Second access: " + fi2.LastAccessTime.ToString());
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults FileInfoTest_LastWriteTime0()
        {
            bool testResult = false;

            int sleep = 2000; // 2 secs

            try
            {
                FileInfo fi1, fi2;

                string name = "FileInfoTest_LastWriteTime" + Guid.NewGuid().ToString() + "_0.txt";

                using (FileStream file = File.Create(name))
                {
                    file.WriteByte( 0x33 );
                    file.WriteByte( 0x34 );
                    file.WriteByte( 0x35 );
                    file.Close();

                    fi1 = new FileInfo(name);
                    testResult = (fi1.LastWriteTime.Ticks != 0);
                }
                System.Threading.Thread.Sleep(sleep);
                using (FileStream again = File.Open(name, FileMode.Append))
                {
                    again.WriteByte(0x33);
                    again.Close();

                    fi2 = new FileInfo(name);
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
    }
}