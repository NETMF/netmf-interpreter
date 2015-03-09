////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master
    {
        public static void Main()
        {
            string[] args = {  "ValueArrayTests", "ReferenceBoxingTests", "ValueDefault_ConstTests", "ValueFloatTests",
                "ValueIntegralTests", "ValueSimpleTests", "ValueTests" };
            MFTestRunner runner = new MFTestRunner(args);
        }
    }
}

//Baseline Document for types
//..\conformance\4_types\value (val007.cs) -- passed
//..\conformance\4_types\value (val008.cs) -- passed
//..\conformance\4_types\value (val009.cs) -- passed
//..\conformance\4_types\value (multiple_assembly01.cs) -- failed
//..\conformance\4_types\value (multiple_assembly02.cs) -- failed
//..\conformance\4_types\value (multiple_assembly03.cs) -- failed
//..\conformance\4_types\value (multiple_assembly04.cs) -- failed
//..\conformance\4_types\value (multiple_assembly06.cs) -- failed
//..\conformance\4_types\value\default_const (defco001.cs) -- passed
//..\conformance\4_types\value\default_const (defco002.cs) -- passed
//..\conformance\4_types\value\default_const (defco003.cs) -- passed
//..\conformance\4_types\value\default_const (defco004.cs) -- passed
//..\conformance\4_types\value\default_const (defco005.cs) -- passed
//..\conformance\4_types\value\default_const (defco006.cs) -- passed
//..\conformance\4_types\value\default_const (defco007.cs) -- passed
//..\conformance\4_types\value\default_const (defco009.cs) -- passed
//..\conformance\4_types\value\default_const (defco011.cs) -- passed
//..\conformance\4_types\value\default_const (defco012.cs) -- passed
//..\conformance\4_types\value\default_const (defco014.cs) -- passed
//..\conformance\4_types\value\default_const (defco015.cs) -- passed
//..\conformance\4_types\value\default_const (defco016.cs) -- passed
//..\conformance\4_types\value\default_const (defco017.cs) -- passed
//..\conformance\4_types\value\simple (simp001.cs) -- passed
//..\conformance\4_types\value\simple (simp002.cs) -- passed
//..\conformance\4_types\value\simple (simp003.cs) -- passed
//..\conformance\4_types\value\simple (simp004.cs) -- passed
//..\conformance\4_types\value\simple (simp005.cs) -- passed
//..\conformance\4_types\value\simple (simp006.cs) -- passed
//..\conformance\4_types\value\simple (simp007.cs) -- passed
//..\conformance\4_types\value\simple (simp009.cs) -- passed
//..\conformance\4_types\value\simple (simp011.cs) -- passed
//..\conformance\4_types\value\simple (simp012.cs) -- failed
//..\conformance\4_types\value\simple (simp013.cs) -- passed
//..\conformance\4_types\value\simple (simp014.cs) -- passed
//..\conformance\4_types\value\simple (simp015.cs) -- passed
//..\conformance\4_types\value\simple (simp016.cs) -- passed
//..\conformance\4_types\value\integral (intg001.cs) -- passed
//..\conformance\4_types\value\integral (intg005.cs) -- passed
//..\conformance\4_types\value\integral (intg009.cs) -- passed
//..\conformance\4_types\value\integral (intg013.cs) -- passed
//..\conformance\4_types\value\integral (intg014.cs) -- passed
//..\conformance\4_types\value\integral (intg015.cs) -- passed
//..\conformance\4_types\value\integral (intg016.cs) -- passed
//..\conformance\4_types\value\integral (intg017.cs) -- passed
//..\conformance\4_types\value\integral (intg018.cs) -- passed
//..\conformance\4_types\value\integral (intg019.cs) -- passed
//..\conformance\4_types\value\integral (intg020.cs) -- passed
//..\conformance\4_types\value\integral (intg021.cs) -- passed
//..\conformance\4_types\value\integral (intg022.cs) -- passed
//..\conformance\4_types\value\integral (intg023.cs) -- passed
//..\conformance\4_types\value\integral (intg024.cs) -- passed
//..\conformance\4_types\value\integral (intg025.cs) -- passed
//..\conformance\4_types\value\integral (intg026.cs) -- passed
//..\conformance\4_types\value\integral (intg027.cs) -- passed
//..\conformance\4_types\value\integral (intg028.cs) -- passed
//..\conformance\4_types\value\integral (intg038.cs) -- passed
//..\conformance\4_types\value\integral (intg039.cs) -- passed
//..\conformance\4_types\value\integral (intg042.cs) -- passed
//..\conformance\4_types\value\integral (intg045.cs) -- passed
//..\conformance\4_types\value\integral (intg049.cs) -- passed
//..\conformance\4_types\value\integral (intg050.cs) -- passed
//..\conformance\4_types\value\integral (intg051.cs) -- passed
//..\conformance\4_types\value\integral (intg052.cs) -- passed
//..\conformance\4_types\value\integral (intg053.cs) -- passed
//..\conformance\4_types\value\integral (intg054.cs) -- passed
//..\conformance\4_types\value\integral (intg055.cs) -- passed
//..\conformance\4_types\value\integral (intg056.cs) -- passed
//..\conformance\4_types\value\integral (intg057.cs) -- passed
//..\conformance\4_types\value\integral (intg058.cs) -- passed
//..\conformance\4_types\value\integral (intg059.cs) -- passed
//..\conformance\4_types\value\integral (intg060.cs) -- passed
//..\conformance\4_types\value\integral (intg061.cs) -- passed
//..\conformance\4_types\value\integral (intg062.cs) -- passed
//..\conformance\4_types\value\integral (intg063.cs) -- passed
//..\conformance\4_types\value\integral (intg064.cs) -- passed
//..\conformance\4_types\value\integral (intg070.cs) -- passed
//..\conformance\4_types\value\float (float001.cs) -- skipped
//..\conformance\4_types\value\float (float002.cs) -- skipped
//..\conformance\4_types\value\float (float003.cs) -- skipped
//..\conformance\4_types\value\float (float004.cs) -- passed
//..\conformance\4_types\value\float (float005.cs) -- passed
//..\conformance\4_types\value\float (float006.cs) -- passed
//..\conformance\4_types\value\float (float007.cs) -- passed
//..\conformance\4_types\value\float (float008.cs) -- passed
//..\conformance\4_types\value\float (float009.cs) -- passed
//..\conformance\4_types\value\float (float010.cs) -- passed
//..\conformance\4_types\value\float (float011.cs) -- passed
//..\conformance\4_types\value\float (float012.cs) -- passed
//..\conformance\4_types\value\float (float013.cs) -- passed
//..\conformance\4_types\value\float (float014.cs) -- passed
//..\conformance\4_types\value\float (float015.cs) -- passed
//..\conformance\4_types\value\float (float016.cs) -- passed
//..\conformance\4_types\value\float (float017.cs) -- passed
//..\conformance\4_types\value\float (float018.cs) -- passed
//..\conformance\4_types\value\float (float019.cs) -- passed
//..\conformance\4_types\value\float (float020.cs) -- passed
//..\conformance\4_types\value\float (float021.cs) -- passed
//..\conformance\4_types\value\float (float022.cs) -- passed
//..\conformance\4_types\value\float (float023.cs) -- passed
//..\conformance\4_types\value\float (float024.cs) -- passed
//..\conformance\4_types\value\float (float025.cs) -- passed
//..\conformance\4_types\value\float (float026.cs) -- passed
//..\conformance\4_types\value\float (float027.cs) -- passed
//..\conformance\4_types\value\float (float028.cs) -- passed
//..\conformance\4_types\value\float (float029.cs) -- passed
//..\conformance\4_types\value\float (float030.cs) -- passed
//..\conformance\4_types\value\float (float031.cs) -- skipped
//..\conformance\4_types\value\float (float032.cs) -- skipped
//..\conformance\4_types\value\float (float033.cs) -- skipped
//..\conformance\4_types\value\float (float034.cs) -- skipped
//..\conformance\4_types\value\float (float035.cs) -- skipped
//..\conformance\4_types\value\float (float036.cs) -- skipped
//..\conformance\4_types\reference\boxing (box001.cs) -- passed
//..\conformance\4_types\reference\boxing (box002.cs) -- passed
//..\conformance\4_types\reference\boxing (box003.cs) -- passed
//..\conformance\4_types\reference\boxing (box004.cs) -- passed
//..\conformance\4_types\reference\boxing (box005.cs) -- passed
//..\conformance\4_types\reference\boxing (box006.cs) -- passed
//..\conformance\4_types\reference\boxing (box007.cs) -- passed
//..\conformance\4_types\reference\boxing (box009.cs) -- passed
//..\conformance\4_types\reference\boxing (box011.cs) -- passed
//..\conformance\4_types\reference\boxing (box012.cs) -- passed
//..\conformance\4_types\reference\boxing (box013.cs) -- passed
//..\conformance\4_types\reference\boxing (box015.cs) -- passed
//..\conformance\4_types\reference\boxing (box016.cs) -- passed
//..\conformance\4_types\reference\boxing (box017.cs) -- passed
//..\conformance\4_types\reference\boxing (box018.cs) -- passed
//..\conformance\4_types\reference\boxing (box019.cs) -- passed
