////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemReflectionAssembly: IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        [TestMethod]
        public MFTestResults AssemblyName_Test1()
        {
            MFTestResults res = MFTestResults.Pass;

            try
            {

                Assembly assm = Assembly.GetExecutingAssembly();

                // we will check that the Name and FullName from the AssemblyName class do go along
                string name = assm.GetName().Name;
                Log.Comment("Assembly name from AssemblyName type is: \"" + name + "\"");
                string fullNameFromAssemblyName = assm.GetName().FullName;
                Log.Comment("Assembly FullNme from AssemblyName type is: \"" + fullNameFromAssemblyName + "\"");

                string nameParsedFromFullName = fullNameFromAssemblyName.Substring(0, fullNameFromAssemblyName.IndexOf(','));
                if (nameParsedFromFullName != name)
                {
                    Log.Comment("The name parsed from the FullName is different than the Name");
                    res = MFTestResults.Fail;
                }

                // we will check that the FullName from Assembly and FullName from the AssemblyName class do match
                string fullName = assm.FullName;
                Log.Comment("Assembly FullName from Assembly type: \"" + fullName + "\"");

                if (fullName != (name + ", Version=" + assm.GetName().Version.ToString()))
                {
                    Log.Comment("The FullName from the Assembly is different than the Name from the AssemblyName");
                    res = MFTestResults.Fail;
                }
            }
            catch(Exception e)
            {
                res = MFTestResults.Fail;
                Log.Exception("Exception caught: ", e);
            }

            return res;
        }

        [TestMethod]
        public MFTestResults AssemblyVersion_Test2()
        {
            MFTestResults res = MFTestResults.Pass;

            try
            {
                // get the version
                Version ver = Assembly.GetExecutingAssembly().GetName().Version;

                if (ver == null)
                {
                    Log.Comment("Assebly Version could not be retrieved");
                    res = MFTestResults.Fail;
                }
            }
            catch (Exception e)
            {
                res = MFTestResults.Fail;
                Log.Exception("Exception caught: ", e);
            }

            return res;
        }

        [TestMethod]
        public MFTestResults AssemblyVersion_Test3()
        {
            MFTestResults res = MFTestResults.Pass;

            try
            {

                Type myType3 = Type.GetType("System.Int32");

                // get the version
                Assembly assm = Assembly.Load("mscorlib");
                if (assm == null) res = MFTestResults.Fail;

                string v = assm.GetName().Version.ToString();

                Assembly assm1 = Assembly.Load("mscorlib, Version=" + v);
                if(assm1 == null) res = MFTestResults.Fail;

                try
                {
                    Assembly assm2 = Assembly.Load("mscorlib, <THIS SHOULD NOT BE HERE>,Version=" + v);
                    res = MFTestResults.Fail;
                }
                catch (ArgumentException)
                {
                }

                // Test for extra parameters after assembly version.  The assembly version parser needs to handle this
                // because the VS debugger will identify in CultureInfo and PublicKeyToken when debugging.
                assm = Assembly.Load("mscorlib, Version=" + v + ", CultureInfo=en, PublicKeyToken=null");
                if (assm == null) res = MFTestResults.Fail;
            }
            catch (Exception e)
            {
                Log.Exception("Exception caught: ", e);
                res = MFTestResults.Fail;
            }

            return res;
        }

        [TestMethod]
        public MFTestResults Assembly_GetAssemblies_Satellite_Test4()
        {
            bool fRes = true;

            Assembly asm = typeof(int).Assembly;

            // Make sure satellite assembly can be retrieved
            Assembly res = asm.GetSatelliteAssembly(new System.Globalization.CultureInfo("en"));
            fRes &= res != null;

            // Make sure we can get a known type from the target assembly
            Type t = asm.GetType("System.Int32");
            fRes &= t.IsValueType;

            // make sure all types from the assembly have proper
            // assembly property
            Type[] ts = asm.GetTypes();
            for (int i = 0; i < ts.Length; i++)
            {
                fRes &= ts[i].Assembly.FullName == asm.FullName;
            }

            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }
    }
}
