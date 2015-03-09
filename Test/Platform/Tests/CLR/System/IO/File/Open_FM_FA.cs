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
    public class Open_FM_FA : IMFTestInterface
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

                Directory.CreateDirectory(testDir);
                Directory.SetCurrentDirectory(testDir);
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
        private const string fileName = "file1.tmp";
        private const string file2Name = "file2.txt";
        private const string testDir = "Open_FM_FA";

        #endregion Local vars

        #region Helper Methods
        private MFTestResults TestMethod(FileMode fm, FileAccess fa)
        {
            Log.Comment("Starting tests in FileMode: " + fm.ToString() + " with FileAccess: " + fa.ToString());
            int iCountErrors = 0;

            String fileName = "TestFile";
            StreamWriter sw2;
            FileStream fs2;
            String str2;

            if (File.Exists(fileName))
                File.Delete(fileName);

            Log.Comment("File does not exist");
            //------------------------------------------------------------------

            switch (fm)
            {
                case FileMode.CreateNew:
                case FileMode.Create:
                case FileMode.OpenOrCreate:
                    try
                    {
                        Log.Comment("null path");
                        fs2 = File.Open(null, fm, fa);
                        if (!File.Exists(fileName))
                        {
                            iCountErrors++;
                            Log.Exception("File not created, FileMode==" + fm.ToString());
                        }
                        fs2.Close();
                    }
                    catch (ArgumentException aexc)
                    {
                        Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc);
                    }
                    try
                    {
                        Log.Comment("string empty path");
                        fs2 = File.Open("", fm, fa);
                        if (!File.Exists(fileName))
                        {
                            iCountErrors++;
                            Log.Exception("File not created, FileMode==" + fm.ToString());
                        }
                        fs2.Close();
                    }
                    catch (ArgumentException aexc)
                    {
                        Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc);
                    }
                    try
                    {
                        Log.Comment("string:" + fileName);
                        fs2 = File.Open(fileName, fm, fa);
                        if (!File.Exists(fileName))
                        {
                            iCountErrors++;
                            Log.Exception("File not created, FileMode==" + fm.ToString());
                        }
                        fs2.Close();
                    }
                    catch (ArgumentException aexc)
                    {
                        if ((fm == FileMode.Create && fa == FileAccess.Read) || (fm == FileMode.CreateNew && fa == FileAccess.Read))
                        {
                            Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                        }
                        else
                        {
                            iCountErrors++;
                            Log.Exception("Unexpected exception, aexc==" + aexc);
                        }
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc);
                    }
                    break;
                case FileMode.Open:
                case FileMode.Truncate:
                    try
                    {
                        Log.Comment("null path");
                        fs2 = File.Open(null, fm, fa);
                        iCountErrors++;
                        Log.Exception("Expected exception not thrown");
                        fs2.Close();
                    }
                    catch (IOException fexc)
                    {
                        Log.Comment("Caught expected exception, fexc==" + fexc.Message);
                    }
                    catch (ArgumentException aexc)
                    {
                        Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }

                    try
                    {
                        Log.Comment("string empty path");
                        fs2 = File.Open("", fm, fa);
                        iCountErrors++;
                        Log.Exception("Expected exception not thrown");
                        fs2.Close();
                    }
                    catch (IOException fexc)
                    {
                        Log.Comment("Caught expected exception, fexc==" + fexc.Message);
                    }
                    catch (ArgumentException aexc)
                    {
                        Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }

                    try
                    {
                        fs2 = File.Open(fileName, fm, fa);
                        iCountErrors++;
                        Log.Exception("Expected exception not thrown");
                        fs2.Close();
                    }
                    catch (IOException fexc)
                    {
                        Log.Comment("Caught expected exception, fexc==" + fexc.Message);
                    }
                    catch (ArgumentException aexc)
                    {
                        if (fa == FileAccess.Read)
                            Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                        else
                        {
                            iCountErrors++;
                            Log.Exception("Unexpected exception thrown, aexc==" + aexc);
                        }
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }
                    break;
                case FileMode.Append:
                    if (fa == FileAccess.Write)
                    {
                        fs2 = File.Open(fileName, fm, fa);
                        if (!File.Exists(fileName))
                        {
                            iCountErrors++;
                            Log.Exception("File not created");
                        }
                        fs2.Close();
                    }
                    else
                    {
                        try
                        {
                            fs2 = File.Open(fileName, fm, fa);
                            iCountErrors++;
                            Log.Exception("Expected exception not thrown");
                            fs2.Close();
                        }
                        catch (ArgumentException aexc)
                        {
                            Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                        }
                        catch (Exception exc)
                        {
                            iCountErrors++;
                            Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                        }
                    }
                    break;
                default:
                    iCountErrors++;
                    Log.Exception("Invalid FileMode.");
                    break;
            }
            if (File.Exists(fileName))
                File.Delete(fileName);

            //------------------------------------------------------------------


            Log.Comment("File already exists");
            //------------------------------------------------------------------

            sw2 = new StreamWriter(fileName);
            str2 = "Du er en ape";
            sw2.Write(str2);
            sw2.Close();
            switch (fm)
            {
                case FileMode.CreateNew:
                    try
                    {
                        fs2 = File.Open(null, fm, fa);
                        iCountErrors++;
                        Log.Exception("Expected exception not thrown");
                        fs2.Close();
                    }
                    catch (ArgumentException aexc)
                    {
                        Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                    }
                    catch (IOException aexc)
                    {
                        Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }

                    try
                    {
                        fs2 = File.Open("", fm, fa);
                        iCountErrors++;
                        Log.Exception("Expected exception not thrown");
                        fs2.Close();
                    }
                    catch (ArgumentException aexc)
                    {
                        Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                    }
                    catch (IOException aexc)
                    {
                        Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }

                    try
                    {
                        fs2 = File.Open(fileName, fm, fa);
                        iCountErrors++;
                        Log.Exception("Expected exception not thrown");
                        fs2.Close();
                    }
                    catch (ArgumentException aexc)
                    {
                        if (fa == FileAccess.Read)
                        {
                            Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                        }
                        else
                        {
                            iCountErrors++;
                            Log.Exception("Unexpected exception, aexc==" + aexc);
                        }

                    }
                    catch (IOException aexc)
                    {
                        Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }
                    break;
                case FileMode.Create:
                    try
                    {
                        fs2 = File.Open(fileName, fm, fa);
                        if (fs2.Length != 0)
                        {
                            iCountErrors++;
                            Log.Exception("Incorrect length of file==" + fs2.Length);
                        }
                        fs2.Close();
                    }
                    catch (ArgumentException aexc)
                    {
                        if (fa == FileAccess.Read)
                        {
                            Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                        }
                        else
                        {
                            iCountErrors++;
                            Log.Exception("Unexpected exception, aexc==" + aexc);
                        }
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }
                    break;
                case FileMode.OpenOrCreate:
                case FileMode.Open:
                    fs2 = File.Open(fileName, fm, fa);
                    if (fs2.Length != str2.Length)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect length on file==" + fs2.Length);
                    }
                    fs2.Close();
                    break;
                case FileMode.Truncate:
                    if (fa == FileAccess.Read)
                    {
                        try
                        {
                            fs2 = File.Open(fileName, fm, fa);
                            iCountErrors++;
                            Log.Exception("Expected exception not thrown");
                        }
                        catch (ArgumentException iexc)
                        {
                            Log.Comment("Caught expected exception, iexc==" + iexc.Message);
                        }
                        catch (Exception exc)
                        {
                            iCountErrors++;
                            Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                        }
                    }
                    else
                    {
                        fs2 = File.Open(fileName, fm, fa);
                        if (fs2.Length != 0)
                        {
                            iCountErrors++;
                            Log.Exception("Incorrect length on file==" + fs2.Length);
                        }
                        fs2.Close();
                    }
                    break;
                case FileMode.Append:
                    if (fa == FileAccess.Write)
                    {
                        fs2 = File.Open(fileName, fm, fa);
                        if (!File.Exists(fileName))
                        {
                            iCountErrors++;
                            Log.Exception("File not created");
                        }
                        fs2.Close();
                    }
                    else
                    {
                        try
                        {
                            fs2 = File.Open(fileName, fm, fa);
                            iCountErrors++;
                            Log.Exception("Expected exception not thrown");
                            fs2.Close();
                        }
                        catch (ArgumentException aexc)
                        {
                            Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                        }
                        catch (Exception exc)
                        {
                            iCountErrors++;
                            Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                        }
                    }
                    break;
                default:
                    iCountErrors++;
                    Log.Exception("Invalid file mode");
                    break;
            }
            return iCountErrors == 0 ? MFTestResults.Pass : MFTestResults.Fail;
        }
        #endregion Helper Methods

        #region Test Cases
        [TestMethod]
        public MFTestResults FileMode_CreateNew_FileAccess_Read()
        {
            return TestMethod(FileMode.CreateNew, FileAccess.Read);
        }

        [TestMethod]
        public MFTestResults FileMode_CreateNew_FileAccess_Write()
        {
            return TestMethod(FileMode.CreateNew, FileAccess.Write);
        }

        [TestMethod]
        public MFTestResults FileMode_CreateNew_FileAccess_ReadWrite()
        {
            return TestMethod(FileMode.CreateNew, FileAccess.ReadWrite);
        }

        [TestMethod]
        public MFTestResults FileMode_Create_FileAccess_Read()
        {
            return TestMethod(FileMode.Create, FileAccess.Read);
        }

        [TestMethod]
        public MFTestResults FileMode_Create_FileAccess_Write()
        {
            return TestMethod(FileMode.Create, FileAccess.Write);
        }

        [TestMethod]
        public MFTestResults FileMode_Create_FileAccess_ReadWrite()
        {
            return TestMethod(FileMode.Create, FileAccess.ReadWrite);
        }

        [TestMethod]
        public MFTestResults FileMode_Open_FileAccess_Read()
        {
            return TestMethod(FileMode.Open, FileAccess.Read);
        }

        [TestMethod]
        public MFTestResults FileMode_Open_FileAccess_Write()
        {
            return TestMethod(FileMode.Open, FileAccess.Write);
        }

        [TestMethod]
        public MFTestResults FileMode_Open_FileAccess_ReadWrite()
        {
            return TestMethod(FileMode.Open, FileAccess.ReadWrite);
        }

        [TestMethod]
        public MFTestResults FileMode_OpenOrCreate_FileAccess_Read()
        {
            return TestMethod(FileMode.OpenOrCreate, FileAccess.Read);
        }

        [TestMethod]
        public MFTestResults FileMode_OpenOrCreate_FileAccess_Write()
        {
            return TestMethod(FileMode.OpenOrCreate, FileAccess.Write);
        }

        [TestMethod]
        public MFTestResults FileMode_OpenOrCreate_FileAccess_ReadWrite()
        {
            return TestMethod(FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        [TestMethod]
        public MFTestResults FileMode_Truncate_FileAccess_Read()
        {
            return TestMethod(FileMode.Truncate, FileAccess.Read);
        }

        [TestMethod]
        public MFTestResults FileMode_Truncate_FileAccess_Write()
        {
            return TestMethod(FileMode.Truncate, FileAccess.Write);
        }

        [TestMethod]
        public MFTestResults FileMode_Truncate_FileAccess_ReadWrite()
        {
            return TestMethod(FileMode.Truncate, FileAccess.ReadWrite);
        }

        [TestMethod]
        public MFTestResults FileMode_Append_FileAccess_Read()
        {
            return TestMethod(FileMode.Append, FileAccess.Read);
        }

        [TestMethod]
        public MFTestResults FileMode_Append_FileAccess_Write()
        {
            return TestMethod(FileMode.Append, FileAccess.Write);
        }

        [TestMethod]
        public MFTestResults FileMode_Append_FileAccess_ReadWrite()
        {
            return TestMethod(FileMode.Append, FileAccess.ReadWrite);
        }
        #endregion Test Cases
    }
}
