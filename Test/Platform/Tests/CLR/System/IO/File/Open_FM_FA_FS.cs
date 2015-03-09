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
    public class Open_FM_FA_FS : IMFTestInterface
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
        private const string testDir = "Open_FM_FA_FS";

        #endregion Local vars

        #region Test Cases

        [TestMethod]
        public MFTestResults FilesShare_None()
        {
            MFTestResults result = MFTestResults.Pass;

            FileStream fs1 = null;
            FileStream fs2 = null;

            try
            {
                // Clean up
                if (File.Exists(file2Name))
                    File.Delete(file2Name);

                fs1 = File.Open(file2Name, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                try
                {
                    fs2 = File.Open(file2Name, FileMode.Open);
                    result = MFTestResults.Fail;
                    Log.Exception("Should not be able to open file with FileShare.None");
                }
                catch (IOException) {/* pass case */}

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                Log.Exception("Stack: " + ex.StackTrace);
                result = MFTestResults.Fail;
            }
            finally
            {
                if (fs1 != null)
                    fs1.Close();
                if (fs2 != null)
                    fs2.Close();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults FilesShare_Read()
        {
            MFTestResults result = MFTestResults.Pass;

            FileStream fs1 = null;
            FileStream fs2 = null;

            try
            {
                byte[] bufferWrite = new byte[] { 10, 98 };
                byte[] bufferRead = new byte[2];

                // Clean up
                if (File.Exists(file2Name))
                    File.Delete(file2Name);

                fs1 = File.Open(file2Name, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                Log.Comment("open 2nd filestream read");
                fs2 = File.Open(file2Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                Log.Comment("write to 1st filestream");
                fs1.Write(bufferWrite, 0, 2);
                fs1.Flush();
                Log.Comment("read from 2nd filestream");
                fs2.Read(bufferRead, 0, 2);

                // Verify content
                if (bufferRead[0] != bufferWrite[0] || bufferRead[1] != bufferWrite[1])
                {
                    result = MFTestResults.Fail;
                }
                try
                {
                    Log.Comment("verify 2nd filestream can't write");
                    fs2.Write(new byte[] { 10 }, 0, 1);
                    result = MFTestResults.Fail;
                    Log.Exception("Should not be able to open file with FileShare.None");
                }
                catch (NotSupportedException) {/* pass case */}

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                Log.Exception("Stack: " + ex.StackTrace);
                result = MFTestResults.Fail;
            }
            finally
            {
                if (fs1 != null)
                    fs1.Close();
                if (fs2 != null)
                    fs2.Close();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults FilesShare_Write()
        {
            MFTestResults result = MFTestResults.Pass;

            FileStream fs1 = null;
            FileStream fs2 = null;

            try
            {
                // Clean up
                if (File.Exists(file2Name))
                    File.Delete(file2Name);

                fs1 = File.Open(file2Name, FileMode.Create, FileAccess.ReadWrite, FileShare.Write);
                fs2 = File.Open(file2Name, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                fs2.Write(new byte[] { 1, 2 }, 0, 2);
                fs1.Write(new byte[] { 3, 4 }, 0, 2);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                Log.Exception("Stack: " + ex.StackTrace);
                result = MFTestResults.Fail;
            }
            finally
            {
                if (fs1 != null)
                    fs1.Close();
                if (fs2 != null)
                    fs2.Close();
            }

            try
            {
                byte[] buffer = File.ReadAllBytes(file2Name);

                if (buffer.Length != 2 || buffer[0] != 3 || buffer[1] != 4)
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                Log.Exception("Stack: " + ex.StackTrace);
                result = MFTestResults.Fail;
            }

            return result;
        }

        public MFTestResults FilesShare_ReadWrite()
        {
            MFTestResults result = MFTestResults.Pass;

            FileStream fs1 = null;
            FileStream fs2 = null;
            FileStream fs3 = null;

            try
            {
                // Clean up
                if (File.Exists(file2Name))
                    File.Delete(file2Name);

                fs1 = File.Open(file2Name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs2 = File.Open(file2Name, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                fs1.Write(new byte[] { 1 }, 0, 1);
                fs3 = File.Open(file2Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fs2.Write(new byte[] { 2 }, 0, 1);
                fs1.Write(new byte[] { 3 }, 0, 1);
                fs2.Flush();
                fs1.Flush();
                fs3.Read(new byte[3], 0, 3);
                fs1.Read(new byte[3], 0, 3);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                Log.Exception("Stack: " + ex.StackTrace);
                result = MFTestResults.Fail;
            }
            finally
            {
                if (fs1 != null)
                    fs1.Close();
                if (fs2 != null)
                    fs2.Close();
                if (fs3 != null)
                    fs3.Close();
            }

            return result;
        }
        #endregion Test Cases
    }
}
