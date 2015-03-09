/*---------------------------------------------------------------------
* Master.cs - file description
* Main class, responsible for running all of the other *Tests.cs files
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 9/4/2007 10:20:51 AM 
* ---------------------------------------------------------------------*/
using System;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.IO;
using System.IO;
using System.Collections;


namespace Microsoft.SPOT.Platform.Tests
{
    public class FormatParameters
    {
        public String VolumeName = "";
        public uint Parameter = 0;
        public String Comment = "";
    }

    public class FileSystemSwitch : IMFTestInterface
    {
        public InitializeResult Initialize()
        {
            if (IOTestsBase.NextVolume() == null)
            {
                return InitializeResult.Skip;
            }
            else
            {
                return InitializeResult.ReadyToGo;
            }
        }

        public void CleanUp()
        {
        }

        public MFTestResults DummyTest()
        {
            return MFTestResults.Pass;
        }
    }

    public class IOTestsBase
    {
        static VolumeInfo _volumeInfo = null;
        static FormatParameters[] _volumes = null;
        static String[] _tests;
        static int _currentVolume;

        static IOTestsBase()
        {
            ArrayList deviceVolumes = new ArrayList();

            try
            {
                // Get Volumes from device
                foreach (VolumeInfo volume in VolumeInfo.GetVolumes())
                {
                    if (volume.Name == "WINFS")
                    {
                        deviceVolumes.Add(new FormatParameters { VolumeName = "WINFS", Parameter = 0, Comment = "Emulator" });
                    }
                    else
                    {
                        // Do one pass formating FAT16 and one pass formating FAT32
                        deviceVolumes.Add(new FormatParameters { VolumeName = volume.Name, Parameter = 1, Comment = "FAT16" });
                        deviceVolumes.Add(new FormatParameters { VolumeName = volume.Name, Parameter = 2, Comment = "FAT32" });
                    }
                }
            }
            catch
            {
            }

            _volumes = (FormatParameters[])deviceVolumes.ToArray(typeof(FormatParameters));
        }

        public static VolumeInfo Volume
        {
            get
            {
                return _volumeInfo;
            }
        }

        public static String[] Tests
        {
            set
            {
                if (_volumes.Length > 0)
                {
                    _tests = new String[_volumes.Length * (value.Length + 1)];

                    for (int i = 0; i < _volumes.Length; i++)
                    {
                        _tests[i * (value.Length + 1)] = "FileSystemSwitch";

                        Array.Copy(value, 0, _tests, i * (value.Length + 1) + 1, value.Length);
                    }
                }
                else
                {
                    // No volumes - still run tests that don't require FS
                    _tests = value;
                }
                _currentVolume = -1;
            }
            get
            {
                return _tests;
            }
        }

        public static VolumeInfo NextVolume()
        {
            _currentVolume++;

            try
            {
                _volumeInfo = new VolumeInfo(_volumes[_currentVolume].VolumeName);
                Log.Comment("The following tests are running on volume " + _volumeInfo.Name + " [" + _volumes[_currentVolume].Comment + "]");
            }
            catch
            {
                _volumeInfo = null;
            }

            return _volumeInfo;
        }
         

        public static void IntializeVolume()
        {
            Log.Comment("Formatting " + Volume.Name + " in " + Volume.FileSystem + " [" + _volumes[_currentVolume].Comment + "]");
            Volume.Format(Volume.FileSystem, _volumes[_currentVolume].Parameter, "TEST_VOL", true);
            Directory.SetCurrentDirectory(Volume.RootDirectory);

            Log.Comment("TestVolumeLabel: " + Volume.VolumeLabel);
        }
    }
}
