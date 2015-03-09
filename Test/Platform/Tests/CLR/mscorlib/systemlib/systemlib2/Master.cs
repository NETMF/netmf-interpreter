/*---------------------------------------------------------------------
* Master.cs - file description
* Main class, responsible for running all of the other *Tests.cs files
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 8/23/2007 10:07:28 AM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_system
    {
        public static void Main()
        {
            string[] args = {  
                                "Utf8EncodingTests",
                                "MicrosoftSpotReflectionTests",
                                "SystemReflectionAssembly", 
                                "SystemReflectionTypeTests",
                                "SystemReflectionMemberTests",
                                "InitLocalsTests",
                                "SystemTypeTests",
                                "SystemAppDomainTests", 
                                "SystemStringTests", 
                                "SystemDateTimeTests", 
                                "SystemMathTests", 
                                "SystemGCTests", 
                                "SystemGlobalizationTests", 
                                "SystemWeakReferenceTests" 
                            };
            MFTestRunner runner = new MFTestRunner(args);
        }
    }
}