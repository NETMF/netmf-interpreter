////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Collections;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Platform.Tests
{
    public class MicrosoftSpotReflectionTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   

            Log.Comment("Testing the Microsoft.SPOT.Reflection class...");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        [TestMethod]
        public MFTestResults MicrosoftSpotReflection_GetTypesImplementingInterfaces_Test0()
        {
            bool fRes = true;

            ///
            /// GetTypesImplementingInterface uses reflection to get all types implementing a certain 
            /// interface.
            /// 
            Type[] typs = Reflection.GetTypesImplementingInterface(typeof(ITestReflectionInterface));

            fRes &= typs.Length == 4;

            foreach (Type t in typs)
            {
                fRes &= t.Assembly.FullName == typeof(MicrosoftSpotReflectionTests).Assembly.FullName;
            }

            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults MicrosoftSpotReflection_IsTypeLoaded_Test1()
        {
            bool fRes = true;

            /// 
            /// IsTypeLoaded was originally used for over the air transmition of
            /// types/assemblies.  It can not really be tested properly without
            /// sending an unloaded serialized type from the PC.  But we will make
            /// sure that it can recognize loaded types and doesn't cause exceptions.
            /// 
            fRes &= Reflection.IsTypeLoaded(typeof(System.Int32));
            fRes &= Reflection.IsTypeLoaded(typeof(TestClass1));

            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults MicrosoftSpotReflection_Hashes_Test2()
        {
            bool fRes = true;

            /// 
            /// This is a basic check to assure the GetxxxHash and GetxxxFromHash
            /// work properly after boxing the reflection types.  This is not intended
            /// to validate the hash value for uniqueness.
            /// 
            Assembly asm = typeof(int).Assembly;
            uint hash = Reflection.GetAssemblyHash(asm);
            fRes &= asm == Reflection.GetAssemblyFromHash(hash);

            hash = Reflection.GetTypeHash(typeof(TestClass1));
            fRes &= typeof(TestClass1) == Reflection.GetTypeFromHash(hash);
            
            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults MicrosoftSpotReflection_Assemblies_Test3()
        {
            bool fRes = true;

            ///
            /// Basic testing Reflection.GetAssemblies and GetAssemblyMemoryInfo. This test
            /// is only intended to assure these methods return propery types and do
            /// not crash.  It is not intended to check for complete correctness.
            /// 
            
            Assembly[] asms = Reflection.GetAssemblies();
            bool fThisAsmFound = false;
            foreach (Assembly a in asms)
            {
                // Make sure we find our assembly in the list
                if (a == typeof(MicrosoftSpotReflectionTests).Assembly)
                {
                    fThisAsmFound = true;
                }
                
                Reflection.AssemblyMemoryInfo mi = new Reflection.AssemblyMemoryInfo();

                // Get the memory info for the assembly and get a property to make sure
                // the value returned is accessible.
                if (Reflection.GetAssemblyMemoryInfo(a, mi))
                {
                    fRes &= (mi.RamSize > 0);
                }
                else
                {
                    fRes = false;
                }
            }
            fRes &= fThisAsmFound;

            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }
        
        //----- classes/Intefaces used by these tests -----//

        public class TestClass1 : ITestReflectionInterface
        {
        }

        public class TestSubclass : TestClass1
        {
        }

        internal protected class TestSubclass2 : TestSubclass
        {
        }
    }

    class TestClass2 : ITestReflectionInterface
    {
    }

    interface ITestReflectionInterface
    {
    }
}