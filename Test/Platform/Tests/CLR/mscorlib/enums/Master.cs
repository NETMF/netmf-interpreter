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
            string[] args = { "EnumTests" };
            MFTestRunner runner = new MFTestRunner(args);
        }
    }
}
//Baseline Document for types
//..\conformance\4_types\value (val007
//..\conformance\4_types\value (val008
//..\conformance\4_types\value (val009
//..\conformance\4_types\value (multiple_assembly01
//..\conformance\4_types\value (multiple_assembly02
//..\conformance\4_types\value (multiple_assembly03
//..\conformance\4_types\value (multiple_assembly04
//..\conformance\4_types\value (multiple_assembly06
//..\conformance\4_types\value\default_const (defco001
//..\conformance\4_types\value\default_const (defco002
//..\conformance\4_types\value\default_const (defco003
//..\conformance\4_types\value\default_const (defco004
//..\conformance\4_types\value\default_const (defco005
//..\conformance\4_types\value\default_const (defco006
//..\conformance\4_types\value\default_const (defco007
//..\conformance\4_types\value\default_const (defco009
//..\conformance\4_types\value\default_const (defco011
//..\conformance\4_types\value\default_const (defco012
//..\conformance\4_types\value\default_const (defco014
//..\conformance\4_types\value\default_const (defco015
//..\conformance\4_types\value\default_const (defco016
//..\conformance\4_types\value\default_const (defco017
//..\conformance\4_types\value\simple (simp001
//..\conformance\4_types\value\simple (simp002
//..\conformance\4_types\value\simple (simp003
//..\conformance\4_types\value\simple (simp004
//..\conformance\4_types\value\simple (simp005
//..\conformance\4_types\value\simple (simp006
//..\conformance\4_types\value\simple (simp007
//..\conformance\4_types\value\simple (simp009
//..\conformance\4_types\value\simple (simp011
//..\conformance\4_types\value\simple (simp012
//..\conformance\4_types\value\simple (simp013
//..\conformance\4_types\value\simple (simp014
//..\conformance\4_types\value\simple (simp015
//..\conformance\4_types\value\simple (simp016
//..\conformance\4_types\value\integral (intg001
//..\conformance\4_types\value\integral (intg005
//..\conformance\4_types\value\integral (intg009
//..\conformance\4_types\value\integral (intg013
//..\conformance\4_types\value\integral (intg014
//..\conformance\4_types\value\integral (intg015
//..\conformance\4_types\value\integral (intg016
//..\conformance\4_types\value\integral (intg017
//..\conformance\4_types\value\integral (intg018
//..\conformance\4_types\value\integral (intg019
//..\conformance\4_types\value\integral (intg020
//..\conformance\4_types\value\integral (intg021
//..\conformance\4_types\value\integral (intg022
//..\conformance\4_types\value\integral (intg023
//..\conformance\4_types\value\integral (intg024
//..\conformance\4_types\value\integral (intg025
//..\conformance\4_types\value\integral (intg026
//..\conformance\4_types\value\integral (intg027
//..\conformance\4_types\value\integral (intg028
//..\conformance\4_types\value\integral (intg038
//..\conformance\4_types\value\integral (intg039
//..\conformance\4_types\value\integral (intg042
//..\conformance\4_types\value\integral (intg045
//..\conformance\4_types\value\integral (intg049
//..\conformance\4_types\value\integral (intg050
//..\conformance\4_types\value\integral (intg051
//..\conformance\4_types\value\integral (intg052
//..\conformance\4_types\value\integral (intg053
//..\conformance\4_types\value\integral (intg054
//..\conformance\4_types\value\integral (intg055
//..\conformance\4_types\value\integral (intg056
//..\conformance\4_types\value\integral (intg057
//..\conformance\4_types\value\integral (intg058
//..\conformance\4_types\value\integral (intg059
//..\conformance\4_types\value\integral (intg060
//..\conformance\4_types\value\integral (intg061
//..\conformance\4_types\value\integral (intg062
//..\conformance\4_types\value\integral (intg063
//..\conformance\4_types\value\integral (intg064
//..\conformance\4_types\value\integral (intg070
//..\conformance\4_types\value\float (float004
//..\conformance\4_types\value\float (float005
//..\conformance\4_types\value\float (float006
//..\conformance\4_types\value\float (float007
//..\conformance\4_types\value\float (float008
//..\conformance\4_types\value\float (float009
//..\conformance\4_types\value\float (float010
//..\conformance\4_types\value\float (float011
//..\conformance\4_types\value\float (float012
//..\conformance\4_types\value\float (float013
//..\conformance\4_types\value\float (float014
//..\conformance\4_types\value\float (float015
//..\conformance\4_types\value\float (float016
//..\conformance\4_types\value\float (float017
//..\conformance\4_types\value\float (float018
//..\conformance\4_types\value\float (float019
//..\conformance\4_types\value\float (float020
//..\conformance\4_types\value\float (float021
//..\conformance\4_types\value\float (float022
//..\conformance\4_types\value\float (float023
//..\conformance\4_types\value\float (float024
//..\conformance\4_types\value\float (float025
//..\conformance\4_types\value\float (float026
//..\conformance\4_types\value\float (float027
//..\conformance\4_types\value\float (float028
//..\conformance\4_types\value\float (float029
//..\conformance\4_types\value\float (float030
//..\conformance\4_types\reference\boxing (box001
//..\conformance\4_types\reference\boxing (box002
//..\conformance\4_types\reference\boxing (box003
//..\conformance\4_types\reference\boxing (box004
//..\conformance\4_types\reference\boxing (box005
//..\conformance\4_types\reference\boxing (box006
//..\conformance\4_types\reference\boxing (box007
//..\conformance\4_types\reference\boxing (box009
//..\conformance\4_types\reference\boxing (box011
//..\conformance\4_types\reference\boxing (box012
//..\conformance\4_types\reference\boxing (box013
//..\conformance\4_types\reference\boxing (box015
//..\conformance\4_types\reference\boxing (box016
//..\conformance\4_types\reference\boxing (box017
//..\conformance\4_types\reference\boxing (box018
//..\conformance\4_types\reference\boxing (box019
