////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class OtherTests1 : IMFTestInterface
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

        //Other Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Other
        //ident001,ident002,ident003,ident004,ident007,ident008,ident009,ident010,ident011,ident012,ident013,ident014,ident015,ident016,ident017,ident018,ident019,ident020,ident021,ident022,ident023,ident024,ident025,ident026,ident027,ident028,ident029,ident030,ident031,ident032,ident033,ident034,ident035,ident038,ident039,ident040,mem001,mem002,mem003,mem004,mem005,mem006,mem007,mem008,mem011,mem012,mem013,mem014,mem015,mem016,mem017,mem018,mem019,mem021,mem022,mem023,mem025,mem026,mem027,mem028,mem029,mem031,mem032,mem034,mem035,mem036,mem038,mem040,mem042,mem043,mem044,mem045,mem046,mem047,mem048,mem050,mem052,mem054,mem055,mem056,mem058,lit001,lit004,lit005,lit006,lit007,lit008,lit009,lit010,lit011,base006,base007,base009,base010,base011,base012,base013,base014,base015,base016,base017,base018,base019,base020,Shift_001,Shift_002,Shift_003,Shift_004,Shift_005,Shift_006,Shift_007,Shift_008,Shift_009,Shift_015,Shift_016,Shift_017,Shift_018,Shift_019,Shift_020,Shift_021,Shift_022,Shift_023,shift_029,shift_030,Shift_037,Shift_038,Shift_041,Shift_042,Shift_043,Shift_044,assign001,assign002,assign003,assign004,assign005,assign006,assign007,assign008,assign009,assign010,assign011,unary001,unary002,unary003,unary004,unary005,unary006,unary007,unary008,unary009,unary012,unary013,unary014,unary015,unary017,unary018,unary019,unary020,unary021,unary023,unary024,unary025,unary026,unary027,unary028,unary029,unary030,unary031,unary036,unary037,unary038,unary039,unary040,unary041,unary042,unary043,unary044,unary045,unary046,unary048,unary049,unary050,unary051,unary052,unary053,unary054,unary055,unary056,unary057,unary058,unary068,relat001,relat002,relat003,relat004,relat005,relat006,relat007,relat008,relat009,relat010,relat011,relat012,relat013,relat014,relat015,relat016,relat017,relat018,relat019,relat020,relat021,relat022,relat023,relat024,relat025,relat026,relat027,relat028,relat029,relat030,relat031,relat032,relat033,relat034,relat035,relat036,relat037,relat038,relat039,relat040,relat041,relat042,relat043,relat044,relat045,relat046,relat047,relat048,relat055,relat056,relat057,relat058,relat059,relat060,relat061,relat062,relat063,relat064,relat065,relat066,relat069,relat072,relat073,relat074,relat075,relat076,relat077,relat078,relat079,relat080,relat081,relat083,relat084,relat086,relat087,relat088,relat089,operators_logic001,operators_logic002,operators_logic003,operators_logic004,operators_logic005,operators_logic006,operators_logic007,operators_logic008,operators_logic009,operators_logic010,operators_logic011,operators_logic012,operators_logic013,operators_logic014,operators_logic015,operators_logic016,operators_logic017,operators_logic018,operators_logic019,operators_logic022,operators_logic023,operators_logic032,operators_logic033,cond001,cond002,cond003,cond004,cond005,cond006,cond008,cond010,cond011,cond014,cond015,is005,is006,is007,is008,is009,is010,is011,is012,is013,is014,is015,is016,is017,is018,is019,is020,is021,is022,is023,is024,is025,is026,is027,is028,is029,is030,is031,is032,is033,is034,as001,as002,as003,as004,as007,as008,as011,as012,add001,add002,add003,add004,add005,add006,logic001,logic002,logic003,logic004,logic005,logic006,logic007,logic008,logic009,logic010,logic011,logic012,logic013,logic014,logic015,logic016,logic017,logic018,logic019,logic020,logic021,logic022,logic023,logic024,logic025,logic026,logic027,logic028,logic029,logic030,logic032


        //Test Case Calls 

        
        [TestMethod]
        public MFTestResults Other_unary068_Test()
        {
            Log.Comment(" 7.6.5 Operator Overloading - Unary operator");
            if (Other_TestClass_unary068.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat001_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat002_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat003_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat004_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat005_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat006_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat007_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat008_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat009_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat010_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat011_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat011.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat012_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat012.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat013_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat014_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat015_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat016_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat016.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat017_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat017.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat018_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat018.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat019_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat019.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat020_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat020.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat021_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat021.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat022_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat022.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat023_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat024_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat024.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat025_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat025.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat026_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat026.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat027_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat027.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat028_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat028.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat029_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat029.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat030_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat030.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat031_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat031.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat032_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat032.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat033_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat033.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat034_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat034.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat035_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat035.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat036_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat036.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }


        /*
        [TestMethod]
        public MFTestResults Other_relat037_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("If either operand is NaN, the result is false");
            Log.Comment("for all operators except !=, and true for !=");
            Log.Comment("operator.");
            if (Other_TestClass_relat037.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat038_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("If either operand is NaN, the result is false");
            Log.Comment("for all operators except !=, and true for !=");
            Log.Comment("operator.");
            if (Other_TestClass_relat038.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat039_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("If either operand is NaN, the result is false");
            Log.Comment("for all operators except !=, and true for !=");
            Log.Comment("operator.");
            if (Other_TestClass_relat039.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat040_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("If either operand is NaN, the result is false");
            Log.Comment("for all operators except !=, and true for !=");
            Log.Comment("operator.");
            if (Other_TestClass_relat040.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat041_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("If either operand is NaN, the result is false");
            Log.Comment("for all operators except !=, and true for !=");
            Log.Comment("operator.");
            if (Other_TestClass_relat041.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat042_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("If either operand is NaN, the result is false");
            Log.Comment("for all operators except !=, and true for !=");
            Log.Comment("operator.");
            if (Other_TestClass_relat042.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat043_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("Negative and positive zero is considered");
            Log.Comment("equal.");
            if (Other_TestClass_relat043.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat044_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("Negative and positive zero is considered");
            Log.Comment("equal.");
            if (Other_TestClass_relat044.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat045_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("Other_TestClass_?_A negative infinity is considered less than");
            Log.Comment("all other values, but equal to another negative");
            Log.Comment("infinity.");
            if (Other_TestClass_relat045.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat046_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("Other_TestClass_?_A negative infinity is considered less than");
            Log.Comment("all other values, but equal to another negative");
            Log.Comment("infinity.");
            if (Other_TestClass_relat046.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat047_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("Other_TestClass_?_A positive infinity is considered greater than all");
            Log.Comment("other values, but equal to positive infinity.");
            if (Other_TestClass_relat047.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat048_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("Other_TestClass_?_A positive infinity is considered greater than all");
            Log.Comment("other values, but equal to positive infinity.");
            if (Other_TestClass_relat048.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        */
        [TestMethod]
        public MFTestResults Other_relat055_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat055.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat056_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat056.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat057_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat057.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat058_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat058.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat059_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat059.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat060_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat060.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat061_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat061.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat062_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat062.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat063_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("since the predefined reference type equality");
            Log.Comment("operators accept operands of type object, they");
            Log.Comment("apply to all types that do not declare applicable");
            Log.Comment("operator == and != members.  Conversely, any ");
            Log.Comment("applicable user-defined equality operators");
            Log.Comment("effectively hide the predefined reference type");
            Log.Comment("equality operators.");
            if (Other_TestClass_relat063.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat064_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("since the predefined reference type equality");
            Log.Comment("operators accept operands of type object, they");
            Log.Comment("apply to all types that do not declare applicable");
            Log.Comment("operator == and != members.  Conversely, any ");
            Log.Comment("applicable user-defined equality operators");
            Log.Comment("effectively hide the predefined reference type");
            Log.Comment("equality operators.");
            if (Other_TestClass_relat064.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat065_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("since the predefined reference type equality");
            Log.Comment("operators accept operands of type object, they");
            Log.Comment("apply to all types that do not declare applicable");
            Log.Comment("operator == and != members.  Conversely, any ");
            Log.Comment("applicable user-defined equality operators");
            Log.Comment("effectively hide the predefined reference type");
            Log.Comment("equality operators.");
            if (Other_TestClass_relat065.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat066_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("since the predefined reference type equality");
            Log.Comment("operators accept operands of type object, they");
            Log.Comment("apply to all types that do not declare applicable");
            Log.Comment("operator == and != members.  Conversely, any ");
            Log.Comment("applicable user-defined equality operators");
            Log.Comment("effectively hide the predefined reference type");
            Log.Comment("equality operators.");
            if (Other_TestClass_relat066.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat069_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("It is an error to use the predefined reference");
            Log.Comment("type equality operators to compare two references");
            Log.Comment("that are known to be different at compile-time.");
            if (Other_TestClass_relat069.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat072_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("The predefined reference type equality operators");
            Log.Comment("do not permit value type operands to be compared.");
            Log.Comment("Therefore, unless a struct type declares its own");
            Log.Comment("equality operators, it is not possible to compare");
            Log.Comment("values of that struct type.");
            if (Other_TestClass_relat072.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat073_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("For an operation of the form x==y or x!=y, ");
            Log.Comment("if any appicable operator == or != exists, the");
            Log.Comment("operator overload resolution rules will select");
            Log.Comment("that operator instead of the predefined reference");
            Log.Comment("type equality operator.  However, it is always");
            Log.Comment("possible to select the reference type equality");
            Log.Comment("operator by explicitly casting one or both of");
            Log.Comment("the operands to type object.");
            if (Other_TestClass_relat073.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat074_Test()
        {
            Log.Comment("Section 7.9");
            Log.Comment("For an operation of the form x==y or x!=y, ");
            Log.Comment("if any appicable operator == or != exists, the");
            Log.Comment("operator overload resolution rules will select");
            Log.Comment("that operator instead of the predefined reference");
            Log.Comment("type equality operator.  However, it is always");
            Log.Comment("possible to select the reference type equality");
            Log.Comment("operator by explicitly casting one or both of");
            Log.Comment("the operands to type object.");
            if (Other_TestClass_relat074.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat075_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat075.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat076_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat076.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat077_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat077.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat078_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat078.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        /*
        [TestMethod]
        public MFTestResults Other_relat079_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat079.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        */
        [TestMethod]
        public MFTestResults Other_relat080_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat080.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat081_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat081.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat083_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat083.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat084_Test()
        {
            Log.Comment("Section 7.9");
            if (Other_TestClass_relat084.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat086_Test()
        {
            Log.Comment("Making sure floating point relational operators are working correctly for negative");
            Log.Comment("vs. positive values.");
            if (Other_TestClass_relat086.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat087_Test()
        {
            Log.Comment("Making sure floating point relational operators are working correctly for negative");
            Log.Comment("vs. positive values.");
            if (Other_TestClass_relat087.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat088_Test()
        {
            Log.Comment("Making sure floating point relational operators are working correctly for negative");
            Log.Comment("vs. positive values.");
            if (Other_TestClass_relat088.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_relat089_Test()
        {
            Log.Comment("Making sure floating point relational operators are working correctly for negative");
            Log.Comment("vs. positive values.");
            if (Other_TestClass_relat089.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic001_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic002_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic003_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic004_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic005_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic006_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic007_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic008_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic009_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic010_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic011_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic011.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic012_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic012.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic013_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic014_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic015_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_operators_logic015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic016_Test()
        {
            Log.Comment("Section 7.11");
            Log.Comment("The operation x (double amp) y corresponds to the");
            Log.Comment("operation x (amp) y except that y is evaluated only");
            Log.Comment("if x is true.");
            if (Other_TestClass_operators_logic016.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic017_Test()
        {
            Log.Comment("Section 7.11");
            Log.Comment("The operation x (double amp) y corresponds to the");
            Log.Comment("operation x (amp) y except that y is evaluated only");
            Log.Comment("if x is true.");
            if (Other_TestClass_operators_logic017.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic018_Test()
        {
            Log.Comment("Section 7.11");
            Log.Comment("The operation x || y corresponds to the");
            Log.Comment("operation x (amp) y except that y is evaluated only");
            Log.Comment("if x is false.");
            if (Other_TestClass_operators_logic018.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic019_Test()
        {
            Log.Comment("Section 7.11");
            Log.Comment("The operation x || y corresponds to the");
            Log.Comment("operation x (amp) y except that y is evaluated only");
            Log.Comment("if x is false.");
            if (Other_TestClass_operators_logic019.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        /*
        [TestMethod]
        public MFTestResults Other_operators_logic022_Test()
        {
            Log.Comment("Section 7.11");
            Log.Comment("When the operands of && or || are of types");
            Log.Comment("that declare an applicable user-defined operator &");
            Log.Comment("or operator |, both of the following must be true,");
            Log.Comment("where T is the type in which the selected operand");
            Log.Comment("is declared:");
                        Log.Comment("The return type and the type of each parameter of ");
            Log.Comment("the selected operator must be T.  In other words, ");
            Log.Comment("the operator must compute the logical AND or the");
            Log.Comment("logical OR of the two operands of type T, and must");
            Log.Comment("return a result of type T.");
                        Log.Comment("T must contain declarations of operator true and");
            Log.Comment("operator false.");
                        if (Other_TestClass_operators_logic022.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic023_Test()
        {
            Log.Comment("Section 7.11");
            Log.Comment("When the operands of && or || are of types");
            Log.Comment("that declare an applicable user-defined operator &");
            Log.Comment("or operator |, both of the following must be true,");
            Log.Comment("where T is the type in which the selected operand");
            Log.Comment("is declared:");
                        Log.Comment("The return type and the type of each parameter of ");
            Log.Comment("the selected operator must be T.  In other words, ");
            Log.Comment("the operator must compute the logical AND or the");
            Log.Comment("logical OR of the two operands of type T, and must");
            Log.Comment("return a result of type T.");
                        Log.Comment("T must contain declarations of operator true and");
            Log.Comment("operator false.");
                        if (Other_TestClass_operators_logic023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_operators_logic032_Test()
        {
            Log.Comment("Section 7.11");
            if (Other_TestClass_operators_logic032.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        */
        [TestMethod]
        public MFTestResults Other_operators_logic033_Test()
        {
            Log.Comment("Section 7.11");
            if (Other_TestClass_operators_logic033.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond001_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("Other_TestClass_?_A conditional expression of the form b(question) x(colon)y");
            Log.Comment("first evaluates the condition b.  Then, if b");
            Log.Comment("is true, x is evaluated and becomes the result");
            Log.Comment("of the operation.  Otherwise, y is evaluated");
            Log.Comment("and becomes the result of the operation.  Other_TestClass_?_A");
            Log.Comment("conditional expression never evaluates both x");
            Log.Comment("and y.");
            if (Other_TestClass_cond001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond002_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("Other_TestClass_?_A conditional expression of the form b(question) x(colon)y");
            Log.Comment("first evaluates the condition b.  Then, if b");
            Log.Comment("is true, x is evaluated and becomes the result");
            Log.Comment("of the operation.  Otherwise, y is evaluated");
            Log.Comment("and becomes the result of the operation.  Other_TestClass_?_A");
            Log.Comment("conditional expression never evaluates both x");
            Log.Comment("and y.");
            if (Other_TestClass_cond002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond003_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("Other_TestClass_?_A conditional expression of the form b(question) x(colon)y");
            Log.Comment("first evaluates the condition b.  Then, if b");
            Log.Comment("is true, x is evaluated and becomes the result");
            Log.Comment("of the operation.  Otherwise, y is evaluated");
            Log.Comment("and becomes the result of the operation.  Other_TestClass_?_A");
            Log.Comment("conditional expression never evaluates both x");
            Log.Comment("and y.");
            if (Other_TestClass_cond003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond004_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("Other_TestClass_?_A conditional expression of the form b(question) x(colon)y");
            Log.Comment("first evaluates the condition b.  Then, if b");
            Log.Comment("is true, x is evaluated and becomes the result");
            Log.Comment("of the operation.  Otherwise, y is evaluated");
            Log.Comment("and becomes the result of the operation.  Other_TestClass_?_A");
            Log.Comment("conditional expression never evaluates both x");
            Log.Comment("and y.");
            if (Other_TestClass_cond004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond005_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("The conditional operator is right-associative, meaning");
            Log.Comment("that operations are grouped from right to left.  For example,");
            Log.Comment("an expression of the form a?b:c?d:e is evaluated as a?b:(c?d:e).");
            if (Other_TestClass_cond005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond006_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("The first operand of the ?: operator must");
            Log.Comment("be an expression of a type that can be ");
            Log.Comment("implicitly converted to bool, or an expression");
            Log.Comment("of a type that implements operator true.  If neither");
            Log.Comment("of these requirements are satisfied, a compile-time");
            Log.Comment("error occurs.");
            if (Other_TestClass_cond006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond008_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("The first operand of the ?: operator must");
            Log.Comment("be an expression of a type that can be ");
            Log.Comment("implicitly converted to bool, or an expression");
            Log.Comment("of a type that implements operator true.  If neither");
            Log.Comment("of these requirements are satisfied, a compile-time");
            Log.Comment("error occurs.");
            if (Other_TestClass_cond008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond010_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("Let X and Y be the types of the second and");
            Log.Comment("third operands. Then,");
            Log.Comment("If X and Y are the same types, then this is");
            Log.Comment("the type of the conditional expression.");
            Log.Comment("Otherwise, if an implicit conversion exists");
            Log.Comment("from X to Y, but not from Y to X, then Y");
            Log.Comment("is the type of the conditional expression.");
            Log.Comment("Otherwise, if an implicit conversion exists");
            Log.Comment("from Y to X, but not from X to Y, then X");
            Log.Comment("is the type of the conditional expression.");
            Log.Comment("Otherwise, no expression type can be ");
            Log.Comment("determined, and a compile time error occurs. ");
            if (Other_TestClass_cond010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond011_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("Let X and Y be the types of the second and");
            Log.Comment("third operands. Then,");
            Log.Comment("If X and Y are the same types, then this is");
            Log.Comment("the type of the conditional expression.");
            Log.Comment("Otherwise, if an implicit conversion exists");
            Log.Comment("from X to Y, but not from Y to X, then Y");
            Log.Comment("is the type of the conditional expression.");
            Log.Comment("Otherwise, if an implicit conversion exists");
            Log.Comment("from Y to X, but not from X to Y, then X");
            Log.Comment("is the type of the conditional expression.");
            Log.Comment("Otherwise, no expression type can be ");
            Log.Comment("determined, and a compile time error occurs. ");
            if (Other_TestClass_cond011.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond014_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("The run-time processing of a conditional expression of");
            Log.Comment("the form b? x: y consists of the following steps:");
            Log.Comment("If an implicit conversion from type b to bool exists,");
            Log.Comment("then this implicit conversion is performed to produce");
            Log.Comment("a bool value.");
            Log.Comment("Otherwise, the operator true defined by the type of");
            Log.Comment("b is invoked to produce a bool value.");
            if (Other_TestClass_cond014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_cond015_Test()
        {
            Log.Comment("Section 7.12");
            Log.Comment("The run-time processing of a conditional expression of");
            Log.Comment("the form b? x: y consists of the following steps:");
            Log.Comment("If an implicit conversion from type b to bool exists,");
            Log.Comment("then this implicit conversion is performed to produce");
            Log.Comment("a bool value.");
            Log.Comment("Otherwise, the operator true defined by the type of");
            Log.Comment("b is invoked to produce a bool value.");
            if (Other_TestClass_cond015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is005_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The is operator is used to check whether the run-time type");
            Log.Comment("of an ojbect is compatible with a given type.  In an operation");
            Log.Comment("of the form e is T, e must be an expression of a reference-type");
            Log.Comment("and T must be a reference-type.  If this is not the case, a compile");
            Log.Comment("time error occurs.");
            if (Other_TestClass_is005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is006_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The is operator is used to check whether the run-time type");
            Log.Comment("of an ojbect is compatible with a given type.  In an operation");
            Log.Comment("of the form e is T, e must be an expression of a reference-type");
            Log.Comment("and T must be a reference-type.  If this is not the case, a compile");
            Log.Comment("time error occurs.");
            if (Other_TestClass_is006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is007_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is008_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is009_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is010_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is011_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is011.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is012_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is012.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is013_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is014_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is015_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is016_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is016.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is017_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is017.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is018_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is018.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is019_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is019.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is020_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is020.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is021_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is021.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is022_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("The operation e is T returns true if not null");
            Log.Comment("and if an implicit reference conversion from the ");
            Log.Comment("run-time type of the instance referenced by e to ");
            Log.Comment("the type given by T exists.  In other words, e is T");
            Log.Comment("checks that e is not null and that a cast-expression");
            Log.Comment("of the form (T)(e) will complete without throwing an ");
            Log.Comment("System.Exception.");
            if (Other_TestClass_is022.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is023_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is024_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is024.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is025_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is025.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is026_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is026.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is027_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is027.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is028_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is028.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is029_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is029.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is030_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is030.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is031_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is031.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is032_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is032.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is033_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is033.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_is034_Test()
        {
            Log.Comment("Section 7.9.9");
            Log.Comment("If e is T is known at compile-time to always be");
            Log.Comment("true or always be false, a compile-time error");
            Log.Comment("occurs.  The operation is known to always be ");
            Log.Comment("true if an implicit reference conversion exists from");
            Log.Comment("the compile-time type of e to T.  The operation is known");
            Log.Comment("to always be false if no implicit or explicit reference");
            Log.Comment("conversion exists from the compile-time type of e to t.");
            if (Other_TestClass_is034.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_as001_Test()
        {
            Log.Comment("Section 7.9.10");
            if (Other_TestClass_as001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_as002_Test()
        {
            Log.Comment("Section 7.9.10");
            if (Other_TestClass_as002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_as003_Test()
        {
            Log.Comment("Section 7.9.10");
            Log.Comment("string->object->array  ");
            if (Other_TestClass_as003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_as004_Test()
        {
            Log.Comment("Section 7.9.10");
            Log.Comment("string->object->array  ");
            if (Other_TestClass_as004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_as007_Test()
        {
            Log.Comment("Section 7.9.10");
            Log.Comment("string->object->array  ");
            if (Other_TestClass_as007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_as008_Test()
        {
            Log.Comment("Section 7.9.10");
            Log.Comment("exp as object  ");
            if (Other_TestClass_as008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_as011_Test()
        {
            Log.Comment("Section 7.9.10");
            Log.Comment("expression as for a deep inheritance");
            if (Other_TestClass_as011.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_as012_Test()
        {
            Log.Comment("Section 7.9.10");
            Log.Comment("expression as non-type through interface (check at runtime)");
            if (Other_TestClass_as012.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_add001_Test()
        {
            Log.Comment("Section 7.7.4");
            Log.Comment("When one or both operands are of type string, the");
            Log.Comment("predefined addition operators concatenate the string");
            Log.Comment("representation of the operands.");
            if (Other_TestClass_add001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_add002_Test()
        {
            Log.Comment("Section 7.7.4");
            Log.Comment("When one or both operands are of type string, the");
            Log.Comment("predefined addition operators concatenate the string");
            Log.Comment("representation of the operands.");
            if (Other_TestClass_add002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_add003_Test()
        {
            Log.Comment("Section 7.7.4");
            Log.Comment("When one or both operands are of type string, the");
            Log.Comment("predefined addition operators concatenate the string");
            Log.Comment("representation of the operands.");
            if (Other_TestClass_add003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_add004_Test()
        {
            Log.Comment("Section 7.7.4");
            Log.Comment("When one or both operands are of type string, the");
            Log.Comment("predefined addition operators concatenate the string");
            Log.Comment("representation of the operands.");
            if (Other_TestClass_add004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_add005_Test()
        {
            Log.Comment("Section 7.7.4");
            Log.Comment("When one or both operands are of type string, the");
            Log.Comment("predefined addition operators concatenate the string");
            Log.Comment("representation of the operands.");
            if (Other_TestClass_add005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_add006_Test()
        {
            Log.Comment("Section 7.7.4");
            Log.Comment("When one or both operands are of type string, the");
            Log.Comment("predefined addition operators concatenate the string");
            Log.Comment("representation of the operands.");
            if (Other_TestClass_add006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic001_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic002_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic003_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic004_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic005_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic005.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic006_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic006.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic007_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic008_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic009_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic010_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic011_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic011.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic012_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic012.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic013_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic014_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic015_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic016_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic016.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic017_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic017.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic018_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic018.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic019_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic019.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic020_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic020.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic021_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic021.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic022_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic022.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic023_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic024_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic024.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic025_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic025.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic026_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic026.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic027_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic027.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic028_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic028.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic029_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic029.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic030_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic030.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Other_logic032_Test()
        {
            Log.Comment("Section 7.10");
            if (Other_TestClass_logic032.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        //Compiled Test Cases 
        
        public class Other_TestClass_Shift_001
        {
            public static int Main_old(string[] args)
            {
                byte bits = 8;
                bits = (byte)(bits << 1);
                if (bits != 16)
                    return (1);
                bits = (byte)(bits >> 1);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_002
        {
            public static int Main_old(string[] args)
            {
                sbyte bits = 8;
                bits = (sbyte)(bits << 1);
                if (bits != 16)
                    return (1);
                bits = (sbyte)(bits >> 1);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_003
        {
            public static int Main_old(string[] args)
            {
                short bits = 8;
                bits = (short)(bits << 1);
                if (bits != 16)
                    return (1);
                bits = (short)(bits >> 1);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_004
        {
            public static int Main_old(string[] args)
            {
                ushort bits = 8;
                bits = (ushort)(bits << 1);
                if (bits != 16)
                    return (1);
                bits = (ushort)(bits >> 1);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_005
        {
            public static int Main_old(string[] args)
            {
                int bits = 8;
                bits = (int)(bits << 1);
                if (bits != 16)
                    return (1);
                bits = (int)(bits >> 1);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_006
        {
            public static int Main_old(string[] args)
            {
                uint bits = 8;
                bits = (uint)(bits << 1);
                if (bits != 16)
                    return (1);
                bits = (uint)(bits >> 1);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_007
        {
            public static int Main_old(string[] args)
            {
                long bits = 8;
                bits = (long)(bits << 1);
                if (bits != 16)
                    return (1);
                bits = (long)(bits >> 1);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_008
        {
            public static int Main_old(string[] args)
            {
                ulong bits = 8;
                bits = (ulong)(bits << 1);
                if (bits != 16)
                    return (1);
                bits = (ulong)(bits >> 1);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_009
        {
            public static int Main_old(string[] args)
            {
                char bits = (char)8;
                bits = (char)(bits << 1);
                if (bits != 16)
                    return (1);
                bits = (char)(bits >> 1);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_015
        {
            public static int Main_old(string[] args)
            {
                int shifter = 1;
                byte bits = 8;
                bits = (byte)(bits << shifter);
                if (bits != 16)
                    return (1);
                bits = (byte)(bits >> shifter);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_016
        {
            public static int Main_old(string[] args)
            {
                int shifter = 1;
                sbyte bits = 8;
                bits = (sbyte)(bits << shifter);
                if (bits != 16)
                    return (1);
                bits = (sbyte)(bits >> shifter);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_017
        {
            public static int Main_old(string[] args)
            {
                int shifter = 1;
                short bits = 8;
                bits = (short)(bits << shifter);
                if (bits != 16)
                    return (1);
                bits = (short)(bits >> shifter);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_018
        {
            public static int Main_old(string[] args)
            {
                int shifter = 1;
                ushort bits = 8;
                bits = (ushort)(bits << shifter);
                if (bits != 16)
                    return (1);
                bits = (ushort)(bits >> shifter);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_019
        {
            public static int Main_old(string[] args)
            {
                int shifter = 1;
                int bits = 8;
                bits = (int)(bits << shifter);
                if (bits != 16)
                    return (1);
                bits = (int)(bits >> shifter);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_020
        {
            public static int Main_old(string[] args)
            {
                int shifter = 1;
                uint bits = 8;
                bits = (uint)(bits << shifter);
                if (bits != 16)
                    return (1);
                bits = (uint)(bits >> shifter);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_021
        {
            public static int Main_old(string[] args)
            {
                int shifter = 1;
                long bits = 8;
                bits = (long)(bits << shifter);
                if (bits != 16)
                    return (1);
                bits = (long)(bits >> shifter);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_022
        {
            public static int Main_old(string[] args)
            {
                int shifter = 1;
                ulong bits = 8;
                bits = (ulong)(bits << shifter);
                if (bits != 16)
                    return (1);
                bits = (ulong)(bits >> shifter);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_023
        {
            public static int Main_old(string[] args)
            {
                int shifter = 1;
                char bits = (char)8;
                bits = (char)(bits << shifter);
                if (bits != 16)
                    return (1);
                bits = (char)(bits >> shifter);
                if (bits != 8)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_shift_029
        {
            public static int Main_old(string[] args)
            {
                sbyte sb = 0x01;
                sb = (sbyte)(sb << 7);
                if (sb != unchecked((sbyte)0xFFFFFF80))
                    return (1);
                sb = (sbyte)(sb >> 7);
                if (sb != -1)
                    return (1);
                short sh = 0x01;
                sh = (short)(sh << 15);
                if (sh != unchecked((short)0xFFFF8000))
                    return (1);
                sh = (short)(sh >> 15);
                if (sh != -1)
                    return (1);
                int in1 = 0x01;
                in1 = in1 << 31;
                if (in1 != unchecked((int)0x80000000))
                    return (1);
                in1 = in1 >> 31;
                if (in1 != -1)
                    return (1);
                long lo = 0x01;
                lo = lo << 63;
                unchecked
                {
                    if (lo != (long)0x8000000000000000)
                        return (1);
                }
                lo = lo >> 63;
                if (lo != -1)
                {
                    return (1);
                }
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_shift_030
        {
            public static int Main_old(string[] args)
            {
                byte ub = 0x01;
                ub = (byte)(ub << 7);
                if (ub != 0x80)
                    return (1);
                ub = (byte)(ub >> 7);
                if (ub != 0x01)
                    return (1);
                ushort ush = 0x01;
                ush = (ushort)(ush << 15);
                if (ush != 0x8000)
                    return (1);
                ush = (ushort)(ush >> 15);
                if (ush != 0x01)
                    return (1);
                uint uin = 0x01;
                uin = uin << 31;
                if (uin != 0x80000000)
                    return (1);
                uin = uin >> 31;
                if (uin != 0x01)
                    return (1);
                ulong ulo = 0x01;
                ulo = ulo << 63;
                if (ulo != 0x8000000000000000)
                    return (1);
                ulo = ulo >> 63;
                if (ulo != 0x01)
                {
                    return (1);
                }
                char ch = (char)0x01;
                ch = (char)(ch << 15);
                if (ch != 0x8000)
                    return (1);
                ch = (char)(ch >> 15);
                if (ch != 0x01)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_037
        {
            public static int Main_old()
            {

                int intI = 8 >> 3;
                if (intI == 1)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_Shift_038
        {
            public static int Main_old()
            {

                int intI = 1 << 3;
                if (intI == 8)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_Shift_041
        {
            public byte ub;
            public ushort ush;
            public uint uin;
            public ulong ulo;
            public char ch;

            public static int Main_old(string[] args)
            {
                Other_TestClass_Shift_041 T = new Other_TestClass_Shift_041();

                T.ub = 0x01;
                T.ub = (byte)(T.ub << 7);
                if (T.ub != 0x80)
                    return (1);
                T.ub = (byte)(T.ub >> 7);
                if (T.ub != 0x01)
                    return (1);
                T.ush = 0x01;
                T.ush = (ushort)(T.ush << 15);
                if (T.ush != 0x8000)
                    return (1);
                T.ush = (ushort)(T.ush >> 15);
                if (T.ush != 0x01)
                    return (1);
                T.uin = 0x01;
                T.uin = T.uin << 31;
                if (T.uin != 0x80000000)
                    return (1);
                T.uin = T.uin >> 31;
                if (T.uin != 0x01)
                    return (1);
                T.ulo = 0x01;
                T.ulo = T.ulo << 63;
                if (T.ulo != 0x8000000000000000)
                    return (1);
                T.ulo = T.ulo >> 63;
                if (T.ulo != 0x01)
                {
                    return (1);
                }
                T.ch = (char)0x01;
                T.ch = (char)(T.ch << 15);
                if (T.ch != 0x8000)
                    return (1);
                T.ch = (char)(T.ch >> 15);
                if (T.ch != 0x01)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_042
        {
            public sbyte sb;
            public short sh;
            public int in1;
            public long lo;

            public static int Main_old(string[] args)
            {
                Other_TestClass_Shift_042 T = new Other_TestClass_Shift_042();
                T.sb = 0x01;
                T.sb = (sbyte)(T.sb << 7);
                if (T.sb != unchecked((sbyte)0xFFFFFF80))
                    return (1);
                T.sb = (sbyte)(T.sb >> 7);
                if (T.sb != -1)
                    return (1);
                T.sh = 0x01;
                T.sh = (short)(T.sh << 15);
                if (T.sh != unchecked((short)0xFFFF8000))
                    return (1);
                T.sh = (short)(T.sh >> 15);
                if (T.sh != -1)
                    return (1);
                T.in1 = 0x01;
                T.in1 = T.in1 << 31;
                if (T.in1 != unchecked((int)0x80000000))
                    return (1);
                T.in1 = T.in1 >> 31;
                if (T.in1 != -1)
                    return (1);
                T.lo = 0x01;
                T.lo = T.lo << 63;
                unchecked
                {
                    if (T.lo != (long)0x8000000000000000)
                        return (1);
                }
                T.lo = T.lo >> 63;
                if (T.lo != -1)
                {
                    return (1);
                }
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_043
        {
            public byte ub;
            public ushort ush;
            public uint uin;
            public ulong ulo;
            public char ch;

            public static int Main_old(string[] args)
            {
                Other_TestClass_Shift_043 T = new Other_TestClass_Shift_043();

                T.ub = 0x01;
                T.ub <<= 7;
                if (T.ub != 0x80)
                    return (1);
                T.ub >>= 7;
                if (T.ub != 0x01)
                    return (1);
                T.ush = 0x01;
                T.ush <<= 15;
                if (T.ush != 0x8000)
                    return (1);
                T.ush >>= 15;
                if (T.ush != 0x01)
                    return (1);
                T.uin = 0x01;
                T.uin <<= 31;
                if (T.uin != 0x80000000)
                    return (1);
                T.uin >>= 31;
                if (T.uin != 0x01)
                    return (1);
                T.ulo = 0x01;
                T.ulo <<= 63;
                if (T.ulo != 0x8000000000000000)
                    return (1);
                T.ulo >>= 63;
                if (T.ulo != 0x01)
                {
                    return (1);
                }
                T.ch = (char)0x01;
                T.ch <<= 15;
                if (T.ch != 0x8000)
                    return (1);
                T.ch >>= 15;
                if (T.ch != 0x01)
                    return (1);
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        public class Other_TestClass_Shift_044
        {
            public sbyte sb;
            public short sh;
            public int in1;
            public long lo;

            public static int Main_old(string[] args)
            {
                Other_TestClass_Shift_044 T = new Other_TestClass_Shift_044();
                T.sb = 0x01;
                T.sb <<= 7;
                if (T.sb != unchecked((sbyte)0xFFFFFF80))
                    return (1);
                T.sb >>= 7;
                if (T.sb != -1)
                    return (1);
                T.sh = 0x01;
                T.sh <<= 15;
                if (T.sh != unchecked((short)0xFFFF8000))
                    return (1);
                T.sh >>= 15;
                if (T.sh != -1)
                    return (1);
                T.in1 = 0x01;
                T.in1 <<= 31;
                if (T.in1 != unchecked((int)0x80000000))
                    return (1);
                T.in1 >>= 31;
                if (T.in1 != -1)
                    return (1);
                T.lo = 0x01;
                T.lo <<= 63;
                unchecked
                {
                    if (T.lo != (long)0x8000000000000000)
                        return (1);
                }
                T.lo >>= 63;
                if (T.lo != -1)
                {
                    return (1);
                }
                return (0);
            }
            public static bool testMethod()
            {
                if (Main_old(null) != 0)
                    return false;
                else
                    return true;
            }
        }
        class Other_TestClass_assign001
        {
            public static int Main_old()
            {
                int Other_TestClass_assign001;
                Other_TestClass_assign001 = 1;
                if (Other_TestClass_assign001 == 1)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_assign002
        {
            public static int Main_old()
            {
                int Other_TestClass_assign002 = 1;
                Other_TestClass_assign002 += 1;
                if (Other_TestClass_assign002 == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_assign003
        {
            public static int Main_old()
            {
                int Other_TestClass_assign003 = 3;
                Other_TestClass_assign003 -= 1;
                if (Other_TestClass_assign003 == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_assign004
        {
            public static int Main_old()
            {
                int Other_TestClass_assign004 = 3;
                Other_TestClass_assign004 *= 4;
                if (Other_TestClass_assign004 == 12)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_assign005
        {
            public static int Main_old()
            {
                int Other_TestClass_assign005 = 12;
                Other_TestClass_assign005 /= 4;
                if (Other_TestClass_assign005 == 3)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_assign006
        {
            public static int Main_old()
            {
                int Other_TestClass_assign006 = 15;
                Other_TestClass_assign006 %= 4;
                if (Other_TestClass_assign006 == 3)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_assign007
        {
            public static int Main_old()
            {
                int Other_TestClass_assign007 = 5;
                Other_TestClass_assign007 &= 4;
                if (Other_TestClass_assign007 == 4)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_assign008
        {
            public static int Main_old()
            {
                int Other_TestClass_assign008 = 3;
                Other_TestClass_assign008 |= 4;
                if (Other_TestClass_assign008 == 7)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_assign009
        {
            public static int Main_old()
            {
                int Other_TestClass_assign009 = 5;
                Other_TestClass_assign009 ^= 4;
                if (Other_TestClass_assign009 == 1)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_assign010
        {
            public static int Main_old()
            {
                int Other_TestClass_assign010 = 4;
                Other_TestClass_assign010 <<= 2;
                if (Other_TestClass_assign010 == 16)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_assign011
        {
            public static int Main_old()
            {
                int Other_TestClass_assign011 = 4;
                Other_TestClass_assign011 >>= 2;
                if (Other_TestClass_assign011 == 1)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary001
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = +test1;
                if (test2 == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary002
        {
            public static int Main_old()
            {
                uint test1 = 2;
                uint test2 = +test1;
                if (test2 == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary003
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = +test1;
                if (test2 == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary004
        {
            public static int Main_old()
            {
                ulong test1 = 2;
                ulong test2 = +test1;
                if (test2 == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary005
        {
            public static int Main_old()
            {
                float test1 = 2;
                float test2 = +test1;
                if (test2 == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary006
        {
            public static int Main_old()
            {
                double test1 = 2;
                double test2 = +test1;
                if (test2 == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary007
        {
            public static int Main_old()
            {
                double test1 = 2;
                double test2 = +test1;
                if (test2 == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary008
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = -test1;
                if (test2 == -2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary009
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = -test1;
                if (test2 == -2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary012
        {
            public static int Main_old()
            {
                int test1 = int.MinValue;
                int test2 = 0;
                unchecked
                {
                    test2 = -test1;
                }

                if ((test1 == int.MinValue) && (test2 == int.MinValue))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary013
        {
            public static int Main_old()
            {
                long test1 = long.MinValue;
                long test2 = 0;
                unchecked
                {
                    test2 = -test1;
                }

                if ((test1 == long.MinValue) && (test2 == long.MinValue))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary014
        {
            public static int Main_old()
            {
                uint test1 = 2;
                long test2 = 0;
                if (((-test1).GetType() == test2.GetType()) && ((-test1) == -2))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary015
        {
            public static int Main_old()
            {
                int intI = 0;
                if ((-2147483648).GetType() == intI.GetType())
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary017
        {
            public static int Main_old()
            {
                long test1 = -9223372036854775808;
                if (test1 == -9223372036854775808)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary018
        {
            public static int Main_old()
            {
                float test1 = 2.0f;
                float test2 = -test1;
                if (test2 == -2.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary019
        {
            public static int Main_old()
            {
                double test1 = 2.0;
                double test2 = -test1;
                if (test2 == -2.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }

        class Other_TestClass_unary023
        {
            public static int Main_old()
            {
                bool test1 = true;
                bool test2 = !test1;
                if (test2 == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary024
        {
            public static int Main_old()
            {
                bool test1 = false;
                bool test2 = !test1;
                if (test2 == true)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary025
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = ~test1;
                if (test2 == -3)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary026
        {
            public static int Main_old()
            {
                uint test1 = 2;
                uint test2 = ~test1;
                if (test2 == 0xFFFFFFFD)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary027
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = ~test1;
                if (test2 == -3)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary028
        {
            public static int Main_old()
            {
                ulong test1 = 2;
                ulong test2 = ~test1;
                if (test2 == 0xFFFFFFFFFFFFFFFD)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary029
        {
            public static int Main_old()
            {
                int intI = 2;
                int intJ = 2;
                ++intI;
                --intJ;
                if ((intI == 3) && (intJ == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary030
        {
            int intI;
            public int MyInt
            {
                get
                {
                    return intI;
                }
                set
                {
                    intI = value;
                }
            }
            public static int Main_old()
            {
                Other_TestClass_unary030 MC1 = new Other_TestClass_unary030();
                Other_TestClass_unary030 MC2 = new Other_TestClass_unary030();
                MC1.MyInt = 2;
                MC2.MyInt = 2;
                ++MC1.MyInt;
                --MC2.MyInt;
                if ((MC1.MyInt == 3) && (MC2.MyInt == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary031
        {
            int intI = 2;
            public int this[int intJ]
            {
                get
                {
                    return intI;
                }
                set
                {
                    intI = value;
                }
            }
            public static int Main_old()
            {
                Other_TestClass_unary031 MC1 = new Other_TestClass_unary031();
                Other_TestClass_unary031 MC2 = new Other_TestClass_unary031();
                ++MC1[0];
                --MC2[0];
                if ((MC1[0] == 3) && (MC2[0] == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary036
        {
            public static int Main_old()
            {
                sbyte test1 = 2;
                sbyte test2 = 2;
                ++test1;
                --test2;
                if ((test1 == 3) && (test2 == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary037
        {
            public static int Main_old()
            {
                byte test1 = 2;
                byte test2 = 2;
                ++test1;
                --test2;
                if ((test1 == 3) && (test2 == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary038
        {
            public static int Main_old()
            {
                short test1 = 2;
                short test2 = 2;
                ++test1;
                --test2;
                if ((test1 == 3) && (test2 == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary039
        {
            public static int Main_old()
            {
                ushort test1 = 2;
                ushort test2 = 2;
                ++test1;
                --test2;
                if ((test1 == 3) && (test2 == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary040
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = 2;
                ++test1;
                --test2;
                if ((test1 == 3) && (test2 == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary041
        {
            public static int Main_old()
            {
                uint test1 = 2;
                uint test2 = 2;
                ++test1;
                --test2;
                if ((test1 == 3) && (test2 == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary042
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = 2;
                ++test1;
                --test2;
                if ((test1 == 3) && (test2 == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary043
        {
            public static int Main_old()
            {
                ulong test1 = 2;
                ulong test2 = 2;
                ++test1;
                --test2;
                if ((test1 == 3) && (test2 == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary044
        {
            public static int Main_old()
            {
                char test1 = (char)2;
                char test2 = (char)2;
                ++test1;
                --test2;
                if ((test1 == 3) && (test2 == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary045
        {
            public static int Main_old()
            {
                float test1 = 2.0f;
                float test2 = 2.0f;
                ++test1;
                --test2;
                if ((test1 == 3.0f) && (test2 == 1.0f))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary046
        {
            public static int Main_old()
            {
                double test1 = 2.0;
                double test2 = 2.0;
                ++test1;
                --test2;
                if ((test1 == 3.0) && (test2 == 1.0))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary048
        {
            public static int Main_old()
            {
                sbyte test1 = 2;
                sbyte test2 = test1++;
                sbyte test3 = ++test1;
                if ((test2 == 2) && (test3 == 4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary049
        {
            public static int Main_old()
            {
                byte test1 = 2;
                byte test2 = test1++;
                byte test3 = ++test1;
                if ((test2 == 2) && (test3 == 4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary050
        {
            public static int Main_old()
            {
                short test1 = 2;
                short test2 = test1++;
                short test3 = ++test1;
                if ((test2 == 2) && (test3 == 4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary051
        {
            public static int Main_old()
            {
                ushort test1 = 2;
                ushort test2 = test1++;
                ushort test3 = ++test1;
                if ((test2 == 2) && (test3 == 4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary052
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = test1++;
                int test3 = ++test1;
                if ((test2 == 2) && (test3 == 4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary053
        {
            public static int Main_old()
            {
                uint test1 = 2;
                uint test2 = test1++;
                uint test3 = ++test1;
                if ((test2 == 2) && (test3 == 4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary054
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = test1++;
                long test3 = ++test1;
                if ((test2 == 2) && (test3 == 4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary055
        {
            public static int Main_old()
            {
                ulong test1 = 2;
                ulong test2 = test1++;
                ulong test3 = ++test1;
                if ((test2 == 2) && (test3 == 4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary056
        {
            public static int Main_old()
            {
                char test1 = (char)2;
                char test2 = test1++;
                char test3 = ++test1;
                if ((test2 == 2) && (test3 == 4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary057
        {
            public static int Main_old()
            {
                float test1 = 2.0f;
                float test2 = test1++;
                float test3 = ++test1;
                if ((test2 == 2.0f) && (test3 == 4.0f))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_unary058
        {
            public static int Main_old()
            {
                double test1 = 2.0;
                double test2 = test1++;
                double test3 = ++test1;
                if ((test2 == 2.0) && (test3 == 4.0))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_unary068
        {
            private int i;
            public int I
            {
                get { return i; }
                set { i = value; }
            }
            public static Other_TestClass_unary068 operator ++(Other_TestClass_unary068 a)
            {
                a.i++;
                return a;
            }
            public static Other_TestClass_unary068 operator --(Other_TestClass_unary068 a)
            {
                a.i--;
                return a;
            }
            public static int Main_old()
            {
                Other_TestClass_unary068 a = new Other_TestClass_unary068();

                for (a.I = 0; a.I < 10; a++) ;
                for (; a.I > -1; a--) ;
                for (; a.I < 10; ++a) ;
                for (; a.I > 0; --a) ;
                return a.I;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat001
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = 2;
                int test3 = 3;
                if ((test1 == test2) && !(test1 == test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat002
        {
            public static int Main_old()
            {
                uint test1 = 2;
                uint test2 = 2;
                uint test3 = 3;
                if ((test1 == test2) && !(test1 == test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat003
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = 2;
                long test3 = 3;
                if ((test1 == test2) && !(test1 == test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat004
        {
            public static int Main_old()
            {
                ulong test1 = 2;
                ulong test2 = 2;
                ulong test3 = 3;
                if ((test1 == test2) && !(test1 == test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat005
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = 2;
                int test3 = 3;
                if (!(test1 != test2) && (test1 != test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat006
        {
            public static int Main_old()
            {
                uint test1 = 2;
                uint test2 = 2;
                uint test3 = 3;
                if (!(test1 != test2) && (test1 != test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat007
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = 2;
                long test3 = 3;
                if (!(test1 != test2) && (test1 != test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat008
        {
            public static int Main_old()
            {
                ulong test1 = 2;
                ulong test2 = 2;
                ulong test3 = 3;
                if (!(test1 != test2) && (test1 != test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat009
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = 3;
                if ((test1 < test2) && !(test2 < test1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat010
        {
            public static int Main_old()
            {
                uint test1 = 2;
                uint test2 = 3;
                if ((test1 < test2) && !(test2 < test1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat011
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = 3;
                if ((test1 < test2) && !(test2 < test1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat012
        {
            public static int Main_old()
            {
                ulong test1 = 2;
                ulong test2 = 3;
                if ((test1 < test2) && !(test2 < test1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat013
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = 3;
                if (!(test1 > test2) && (test2 > test1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat014
        {
            public static int Main_old()
            {
                uint test1 = 2;
                uint test2 = 3;
                if (!(test1 > test2) && (test2 > test1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat015
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = 3;
                if (!(test1 > test2) && (test2 > test1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat016
        {
            public static int Main_old()
            {
                ulong test1 = 2;
                ulong test2 = 3;
                if (!(test1 > test2) && (test2 > test1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat017
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = 3;
                int test3 = 2;
                if ((test1 <= test2) && !(test2 <= test1) && (test1 <= test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat018
        {
            public static int Main_old()
            {
                uint test1 = 2;
                uint test2 = 3;
                uint test3 = 2;
                if ((test1 <= test2) && !(test2 <= test1) && (test1 <= test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat019
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = 3;
                long test3 = 2;
                if ((test1 <= test2) && !(test2 <= test1) && (test1 <= test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat020
        {
            public static int Main_old()
            {
                ulong test1 = 2;
                ulong test2 = 3;
                ulong test3 = 2;
                if ((test1 <= test2) && !(test2 <= test1) && (test1 <= test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat021
        {
            public static int Main_old()
            {
                int test1 = 2;
                int test2 = 3;
                int test3 = 2;
                if (!(test1 >= test2) && (test2 >= test1) && (test1 >= test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat022
        {
            public static int Main_old()
            {
                uint test1 = 2;
                uint test2 = 3;
                uint test3 = 2;
                if (!(test1 >= test2) && (test2 >= test1) && (test1 >= test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat023
        {
            public static int Main_old()
            {
                long test1 = 2;
                long test2 = 3;
                long test3 = 2;
                if (!(test1 >= test2) && (test2 >= test1) && (test1 >= test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat024
        {
            public static int Main_old()
            {
                ulong test1 = 2;
                ulong test2 = 3;
                ulong test3 = 2;
                if (!(test1 >= test2) && (test2 >= test1) && (test1 >= test3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat025
        {
            public static int Main_old()
            {
                float f1 = 1.0f;
                float f2 = 1.0f;
                float f3 = 2.0f;
                if ((f1 == f2) && !(f1 == f3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat026
        {
            public static int Main_old()
            {
                double d1 = 1.0;
                double d2 = 1.0;
                double d3 = 2.0;
                if ((d1 == d2) && !(d1 == d3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat027
        {
            public static int Main_old()
            {
                float f1 = 1.0f;
                float f2 = 1.0f;
                float f3 = 2.0f;
                if (!(f1 != f2) && (f1 != f3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat028
        {
            public static int Main_old()
            {
                double d1 = 1.0;
                double d2 = 1.0;
                double d3 = 2.0;
                if (!(d1 != d2) && (d1 != d3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat029
        {
            public static int Main_old()
            {
                float f1 = 1.0f;
                float f2 = 1.0f;
                float f3 = 2.0f;
                if (!(f1 < f2) && (f1 < f3) && !(f3 < f1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat030
        {
            public static int Main_old()
            {
                double d1 = 1.0;
                double d2 = 1.0;
                double d3 = 2.0;
                if (!(d1 < d2) && (d1 < d3) && !(d3 < d1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat031
        {
            public static int Main_old()
            {
                float f1 = 1.0f;
                float f2 = 1.0f;
                float f3 = 2.0f;
                if (!(f1 > f2) && !(f1 > f3) && (f3 > f1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat032
        {
            public static int Main_old()
            {
                double d1 = 1.0;
                double d2 = 1.0;
                double d3 = 2.0;
                if (!(d1 > d2) && !(d1 > d3) && (d3 > d1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat033
        {
            public static int Main_old()
            {
                float f1 = 1.0f;
                float f2 = 1.0f;
                float f3 = 2.0f;
                if ((f1 <= f2) && (f1 <= f3) && !(f3 <= f1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat034
        {
            public static int Main_old()
            {
                double d1 = 1.0;
                double d2 = 1.0;
                double d3 = 2.0;
                if ((d1 <= d2) && (d1 <= d3) && !(d3 <= d1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat035
        {
            public static int Main_old()
            {
                float f1 = 1.0f;
                float f2 = 1.0f;
                float f3 = 2.0f;
                if ((f1 >= f2) && !(f1 >= f3) && (f3 >= f1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat036
        {
            public static int Main_old()
            {
                double d1 = 1.0;
                double d2 = 1.0;
                double d3 = 2.0;
                if ((d1 >= d2) && !(d1 >= d3) && (d3 >= d1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        
        /*
        class Other_TestClass_relat037
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = float.NaN;
                double d1 = 2.0;
                double d2 = double.NaN;
                if (f1 == f2)
                {
                    return 1;
                }
                if (f2 == f2)
                {
                    return 1;
                }
                if (d1 == d2)
                {
                    return 1;
                }
                if (d2 == d2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat038
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = float.NaN;
                double d1 = 2.0;
                double d2 = double.NaN;
                if (!(f1 != f2))
                {
                    return 1;
                }
                if (!(f2 != f2))
                {
                    return 1;
                }
                if (!(d1 != d2))
                {
                    return 1;
                }
                if (!(d2 != d2))
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat039
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = float.NaN;
                double d1 = 2.0;
                double d2 = double.NaN;
                if (f1 < f2)
                {
                    return 1;
                }
                if (f2 < f2)
                {
                    return 1;
                }
                if (d1 < d2)
                {
                    return 1;
                }
                if (d2 < d2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat040
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = float.NaN;
                double d1 = 2.0;
                double d2 = double.NaN;
                if (f1 > f2)
                {
                    return 1;
                }
                if (f2 > f2)
                {
                    return 1;
                }
                if (d1 > d2)
                {
                    return 1;
                }
                if (d2 > d2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat041
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = float.NaN;
                double d1 = 2.0;
                double d2 = double.NaN;
                if (f1 <= f2)
                {
                    return 1;
                }
                if (f2 <= f2)
                {
                    return 1;
                }
                if (d1 <= d2)
                {
                    return 1;
                }
                if (d2 <= d2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat042
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = float.NaN;
                double d1 = 2.0;
                double d2 = double.NaN;
                if (f1 >= f2)
                {
                    return 1;
                }
                if (f2 >= f2)
                {
                    return 1;
                }
                if (d1 >= d2)
                {
                    return 1;
                }
                if (d2 >= d2)
                {
                    return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat043
        {
            public static int Main_old()
            {
                float f1 = +0.0f;
                float f2 = -0.0f;
                double d1 = +0.0;
                double d2 = -0.0;
                if ((f1 == f2) && (f2 == f1) && (d1 == d2) && (d2 == d1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat044
        {
            public static int Main_old()
            {
                float f1 = +0.0f;
                float f2 = -0.0f;
                double d1 = +0.0;
                double d2 = -0.0;
                if ((!(f1 != f2)) && (!(f2 != f1)) && (!(d1 != d2)) && (!(d2 != d1)))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat045
        {
            public static int Main_old()
            {
                float f1 = float.MinValue;
                float f2 = float.NegativeInfinity;
                float f3 = float.NegativeInfinity;
                if ((f1 > f2) && (f1 >= f2) && (f2 < f1) && (f2 <= f1) && (f2 == f3) && (f2 != f1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat046
        {
            public static int Main_old()
            {
                double d1 = double.MinValue;
                double d2 = double.NegativeInfinity;
                double d3 = double.NegativeInfinity;
                if ((d1 > d2) && (d1 >= d2) && (d2 < d1) && (d2 <= d1) && (d2 == d3) && (d2 != d1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat047
        {
            public static int Main_old()
            {
                float f1 = float.MaxValue;
                float f2 = float.PositiveInfinity;
                float f3 = float.PositiveInfinity;
                if ((f1 < f2) && (f1 <= f2) && (f2 > f1) && (f2 >= f1) && (f2 == f3) && (f2 != f1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat048
        {
            public static int Main_old()
            {
                double d1 = double.MaxValue;
                double d2 = double.PositiveInfinity;
                double d3 = double.PositiveInfinity;
                if ((d1 < d2) && (d1 <= d2) && (d2 > d1) && (d2 >= d1) && (d2 == d3) && (d2 != d1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        */
        class Other_TestClass_relat055
        {
            public static int Main_old()
            {
                bool b1 = true;
                bool b2 = true;
                bool b3 = false;
                bool b4 = false;
                if ((b1 == b2) && (b3 == b4) && !(b1 == b3) && !(b4 == b2))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat056
        {
            public static int Main_old()
            {
                bool b1 = true;
                bool b2 = true;
                bool b3 = false;
                bool b4 = false;
                if (!(b1 != b2) && !(b3 != b4) && (b1 != b3) && (b4 != b2))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        enum Others_TestClass_relat057_En { a = 1, b = 1, c = 3 }
        class Other_TestClass_relat057
        {
            public static int Main_old()
            {
                Others_TestClass_relat057_En e1 = Others_TestClass_relat057_En.a;
                Others_TestClass_relat057_En e2 = Others_TestClass_relat057_En.b;
                Others_TestClass_relat057_En e3 = Others_TestClass_relat057_En.c;
                if ((e1 == e2) && !(e1 == e3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        enum Others_TestClass_relat058_En { a = 1, b = 1, c = 3 }
        class Other_TestClass_relat058
        {
            public static int Main_old()
            {
                Others_TestClass_relat058_En e1 = Others_TestClass_relat058_En.a;
                Others_TestClass_relat058_En e2 = Others_TestClass_relat058_En.b;
                Others_TestClass_relat058_En e3 = Others_TestClass_relat058_En.c;
                if (!(e1 != e2) && (e1 != e3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        enum Others_TestClass_relat059_En { a = 1, b = 1, c = 3 }
        class Other_TestClass_relat059
        {
            public static int Main_old()
            {
                Others_TestClass_relat059_En e1 = Others_TestClass_relat059_En.a;
                Others_TestClass_relat059_En e2 = Others_TestClass_relat059_En.b;
                Others_TestClass_relat059_En e3 = Others_TestClass_relat059_En.c;
                if (!(e1 > e2) && !(e1 > e3) && (e3 > e1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        enum Others_TestClass_relat060_En { a = 1, b = 1, c = 3 }
        class Other_TestClass_relat060
        {
            public static int Main_old()
            {
                Others_TestClass_relat060_En e1 = Others_TestClass_relat060_En.a;
                Others_TestClass_relat060_En e2 = Others_TestClass_relat060_En.b;
                Others_TestClass_relat060_En e3 = Others_TestClass_relat060_En.c;
                if (!(e1 < e2) && (e1 < e3) && !(e3 < e1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        enum Others_TestClass_relat061_En { a = 1, b = 1, c = 3 }
        class Other_TestClass_relat061
        {
            public static int Main_old()
            {
                Others_TestClass_relat061_En e1 = Others_TestClass_relat061_En.a;
                Others_TestClass_relat061_En e2 = Others_TestClass_relat061_En.b;
                Others_TestClass_relat061_En e3 = Others_TestClass_relat061_En.c;
                if ((e1 >= e2) && !(e1 >= e3) && (e3 >= e1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        enum Others_TestClass_relat062_En { a = 1, b = 1, c = 3 }
        class Other_TestClass_relat062
        {
            public static int Main_old()
            {
                Others_TestClass_relat062_En e1 = Others_TestClass_relat062_En.a;
                Others_TestClass_relat062_En e2 = Others_TestClass_relat062_En.b;
                Others_TestClass_relat062_En e3 = Others_TestClass_relat062_En.c;
                if ((e1 <= e2) && (e1 <= e3) && !(e3 <= e1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Others_TestClass_relat063 { }
        class Other_TestClass_relat063
        {
            public static int Main_old()
            {
                Others_TestClass_relat063 t1 = new Others_TestClass_relat063();
                Others_TestClass_relat063 t2 = t1;
                Others_TestClass_relat063 t3 = new Others_TestClass_relat063();
                if ((t1 == t2) && !(t1 == t3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Others_TestClass_relat064 { }
        class Other_TestClass_relat064
        {
            public static int Main_old()
            {
                Others_TestClass_relat064 t1 = new Others_TestClass_relat064();
                Others_TestClass_relat064 t2 = t1;
                Others_TestClass_relat064 t3 = new Others_TestClass_relat064();
                if (!(t1 != t2) && (t1 != t3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Others_TestClass_relat065
        {
            public int intI;
            public static bool operator ==(Others_TestClass_relat065 t1, Others_TestClass_relat065 t2)
            {
                if (t1.intI == t2.intI)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            public static bool operator !=(Others_TestClass_relat065 t1, Others_TestClass_relat065 t2)
            {
                if (t1.intI == t2.intI)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class Other_TestClass_relat065
        {
            public static int Main_old()
            {
                Others_TestClass_relat065 t1 = new Others_TestClass_relat065();
                t1.intI = 2;
                Others_TestClass_relat065 t2 = t1;
                Others_TestClass_relat065 t3 = new Others_TestClass_relat065();
                t3.intI = 3;
                if (!(t1 == t2) && (t1 == t3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Others_TestClass_relat066
        {
            public int intI;
            public static bool operator ==(Others_TestClass_relat066 t1, Others_TestClass_relat066 t2)
            {
                if (t1.intI == t2.intI)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            public static bool operator !=(Others_TestClass_relat066 t1, Others_TestClass_relat066 t2)
            {
                if (t1.intI == t2.intI)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class Other_TestClass_relat066
        {
            public static int Main_old()
            {
                Others_TestClass_relat066 t1 = new Others_TestClass_relat066();
                t1.intI = 2;
                Others_TestClass_relat066 t2 = t1;
                Others_TestClass_relat066 t3 = new Others_TestClass_relat066();
                t3.intI = 3;
                if ((t1 != t2) && !(t1 != t3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_relat069_A { }
        public class Other_TestClass_relat069_B : Other_TestClass_relat069_A { }
        class Other_TestClass_relat069
        {
            public static int Main_old()
            {
                Other_TestClass_relat069_A t1 = new Other_TestClass_relat069_A();
                Other_TestClass_relat069_B t2 = new Other_TestClass_relat069_B();
                Other_TestClass_relat069_A t3 = t2;
                if (!(t1 == t2) && (t2 == t3) && (t1 != t2) && !(t2 != t3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        struct Others_TestClass_relat072_Str
        {
            public int intI;
            public static bool operator ==(Others_TestClass_relat072_Str s1, Others_TestClass_relat072_Str s2)
            {
                if (s1.intI == s2.intI)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public static bool operator !=(Others_TestClass_relat072_Str s1, Others_TestClass_relat072_Str s2)
            {
                if (s1.intI != s2.intI)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class Other_TestClass_relat072
        {
            public static int Main_old()
            {
                Others_TestClass_relat072_Str t1 = new Others_TestClass_relat072_Str();
                t1.intI = 2;
                Others_TestClass_relat072_Str t2 = new Others_TestClass_relat072_Str();
                t2.intI = 3;
                if (!(t1 == t2) && (t1 != t2))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Others_TestClass_relat073
        {
            public Others_TestClass_relat073(int i)
            {
                intI = i;
            }
            public int intI;
            public static bool operator ==(Others_TestClass_relat073 t1, Others_TestClass_relat073 t2)
            {
                if (t1.intI == t2.intI)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public static bool operator !=(Others_TestClass_relat073 t1, Others_TestClass_relat073 t2)
            {
                if (t1.intI != t2.intI)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class Other_TestClass_relat073
        {
            public static int Main_old()
            {
                Others_TestClass_relat073 s = new Others_TestClass_relat073(1);
                Others_TestClass_relat073 t = new Others_TestClass_relat073(1);
                if ((s == t) && !((object)s == t) && !(s == (object)t) && !((object)s == (object)t))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat074
        {
            public static int Main_old()
            {
                int i = 123;
                int j = 123;
                if (!((object)i == (object)j))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat075
        {
            public static int Main_old()
            {
                string s1 = "Other_TestClass_relat075 String";
                string s2 = "Other_TestClass_relat075 " + "String".ToString();
                string s3 = "Other_TestClass_relat075 String ";
                if ((s1 == s2) && (s1 != s3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat076
        {
            public static int Main_old()
            {
                string s1 = null;
                string s2 = null;
                string s3 = "Other_TestClass_relat076 String ";
                if ((s1 == s2) && (s1 != s3))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Others_TestClass_relat077_Del();
        class Other_TestClass_relat077
        {
            public int TestMeth1()
            {
                return 1;
            }
            public int TestMeth2()
            {
                return 2;
            }
            public static int Main_old() {
                Other_TestClass_relat077 Other_TestClass_relat077 = new Other_TestClass_relat077();
		Others_TestClass_relat077_Del Del1 = new Others_TestClass_relat077_Del(Other_TestClass_relat077.TestMeth1);
		Others_TestClass_relat077_Del Del2 = Del1;
        Others_TestClass_relat077_Del Del3 = new Others_TestClass_relat077_Del(Other_TestClass_relat077.TestMeth2);
		if ((Del1 == Del2) && !(Del1 == Del3)){
			return 0;
		}
		else {
			return 1;
		}
	}
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        delegate int Others_TestClass_relat078_Del();
        class Other_TestClass_relat078
        {
            public int TestMeth1()
            {
                return 1;
            }
            public int TestMeth2()
            {
                return 2;
            }
            public static int Main_old() {
                Other_TestClass_relat078 Other_TestClass_relat078 = new Other_TestClass_relat078();
		Others_TestClass_relat078_Del Del1 = new Others_TestClass_relat078_Del(Other_TestClass_relat078.TestMeth1);
		Others_TestClass_relat078_Del Del2 = Del1;
        Others_TestClass_relat078_Del Del3 = new Others_TestClass_relat078_Del(Other_TestClass_relat078.TestMeth2);
		if (!(Del1 != Del2) && (Del1 != Del3)){
			return 0;
		}
		else {
			return 1;
		}
	}
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        /*
        public class Other_TestClass_relat079
        {
            const string s1 = null;
            const string s2 = null;
            public static int Main_old()
            {
                if ((s1 == s2) && !(s1 != s2))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
         */
        public class Other_TestClass_relat080
        {
            const float f1 = 2.0f;
            const float f2 = 2.0f;
            const float f3 = 3.0f;
            public static int Main_old()
            {
                if (!(f1 == f2)) return 1;
                if ((f1 == f3)) return 1;
                if ((f1 != f2)) return 1;
                if (!(f1 != f3)) return 1;
                if (f1 > f2) return 1;
                if (!(f3 > f1)) return 1;
                if (f1 > f3) return 1;
                if (f1 < f2) return 1;
                if (!(f1 < f3)) return 1;
                if (f3 < f1) return 1;
                if (!(f1 >= f2)) return 1;
                if (!(f3 >= f1)) return 1;
                if (f1 >= f3) return 1;
                if (!(f1 <= f2)) return 1;
                if (!(f1 <= f3)) return 1;
                if (f3 <= f1) return 1;

                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_relat081
        {
            const double d1 = 2.0;
            const double d2 = 2.0;
            const double d3 = 3.0;
            public static int Main_old()
            {
                if (!(d1 == d2)) return 1;
                if ((d1 == d3)) return 1;
                if ((d1 != d2)) return 1;
                if (!(d1 != d3)) return 1;
                if (d1 > d2) return 1;
                if (!(d3 > d1)) return 1;
                if (d1 > d3) return 1;
                if (d1 < d2) return 1;
                if (!(d1 < d3)) return 1;
                if (d3 < d1) return 1;
                if (!(d1 >= d2)) return 1;
                if (!(d3 >= d1)) return 1;
                if (d1 >= d3) return 1;
                if (!(d1 <= d2)) return 1;
                if (!(d1 <= d3)) return 1;
                if (d3 <= d1) return 1;

                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_relat083
        {
            public static implicit operator string(Other_TestClass_relat083 MC)
            {
                return "Other_TestClass_relat0831";
            }
            public static int Main_old()
            {
                Other_TestClass_relat083 MC = new Other_TestClass_relat083();
                if ((MC == "Other_TestClass_relat0831") && (MC != "Other_TestClass_relat0832"))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_relat084
        {
            public static implicit operator string(Other_TestClass_relat084 MC)
            {
                return "Other_TestClass_relat0841";
            }
            public static int Main_old()
            {
                Other_TestClass_relat084 MC = new Other_TestClass_relat084();
                string TestString1 = "Other_TestClass_relat0841";
                string TestString2 = "Other_TestClass_relat0842";
                if ((MC == TestString1) && (MC != TestString2))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_relat086
        {
            public static bool testMethod()
            {
                float f1 = 1.0f;
                float f2 = -1.0f;
                float f3 = -2.0f;
                if ((f1 < f2) && (f1 < f3) && (f2 < f3) && (f2 > f1) && (f3 > f1) && (f3 > f2))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        class Other_TestClass_relat087
        {
            public static bool testMethod()
            {
                float f1 = 1.0f;
                float f2 = -1.0f;
                float f3 = -2.0f;
                if ((f1 <= f2) && (f1 <= f3) && (f2 <= f3) && (f2 >= f1) && (f3 >= f1) && (f3 >= f2))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        class Other_TestClass_relat088
        {
            public static bool testMethod()
            {
                float f1 = float.MaxValue;
                float f2 = float.MinValue;
                float f3 = 0.0F;
                if ((f1 < f2) && (f1 < f3) && (f3 < f2) && (f2 > f1) && (f3 > f1) && (f2 > f3))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        class Other_TestClass_relat089
        {
            public static bool testMethod()
            {
                float f1 = float.MaxValue;
                float f2 = 0.0f;
                float f3 = float.MinValue;
                if ((f1 <= f2) && (f1 <= f3) && (f2 <= f3) && (f2 >= f1) && (f3 >= f1) && (f3 >= f2))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        class Other_TestClass_operators_logic001
        {
            public static int Main_old()
            {
                int test1 = 5;
                int test2 = 4;
                if ((test1 & test2) == 4)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic002
        {
            public static int Main_old()
            {
                uint test1 = 5;
                uint test2 = 4;
                if ((test1 & test2) == 4)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic003
        {
            public static int Main_old()
            {
                long test1 = 5;
                long test2 = 4;
                if ((test1 & test2) == 4)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic004
        {
            public static int Main_old()
            {
                ulong test1 = 5;
                ulong test2 = 4;
                if ((test1 & test2) == 4)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic005
        {
            public static int Main_old()
            {
                int test1 = 1;
                int test2 = 4;
                if ((test1 | test2) == 5)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic006
        {
            public static int Main_old()
            {
                uint test1 = 1;
                uint test2 = 4;
                if ((test1 | test2) == 5)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic007
        {
            public static int Main_old()
            {
                long test1 = 1;
                long test2 = 4;
                if ((test1 | test2) == 5)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic008
        {
            public static int Main_old()
            {
                ulong test1 = 1;
                ulong test2 = 4;
                if ((test1 | test2) == 5)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic009
        {
            public static int Main_old()
            {
                int test1 = 6;
                int test2 = 5;
                if ((test1 ^ test2) == 3)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic010
        {
            public static int Main_old()
            {
                uint test1 = 6;
                uint test2 = 5;
                if ((test1 ^ test2) == 3)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic011
        {
            public static int Main_old()
            {
                long test1 = 6;
                long test2 = 5;
                if ((test1 ^ test2) == 3)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic012
        {
            public static int Main_old()
            {
                ulong test1 = 6;
                ulong test2 = 5;
                if ((test1 ^ test2) == 3)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic013
        {
            public static int Main_old()
            {
                bool b1 = true;
                bool b2 = true;
                bool b3 = false;
                bool b4 = false;
                if ((b1 & b2) && !(b1 & b3) && !(b4 & b2) && !(b3 & b4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic014
        {
            public static int Main_old()
            {
                bool b1 = true;
                bool b2 = true;
                bool b3 = false;
                bool b4 = false;
                if ((b1 | b2) && (b1 | b3) && (b4 | b2) && !(b3 | b4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic015
        {
            public static int Main_old()
            {
                bool b1 = true;
                bool b2 = true;
                bool b3 = false;
                bool b4 = false;
                if (!(b1 ^ b2) && (b1 ^ b3) && (b4 ^ b2) && !(b3 ^ b4))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic016
        {
            public static int Main_old()
            {
                bool b1 = true;
                bool b2 = true;
                bool b3 = false;
                bool b4 = false;
                if (!(b1 && b2)) return 1;
                if ((b1 && b3)) return 1;
                if ((b4 && b2)) return 1;
                if ((b3 && b4)) return 1;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic017
        {
            public static bool MethCalled = false;
            public static bool Meth()
            {
                MethCalled = true;
                return true;
            }
            public static int Main_old()
            {
                bool b1 = false;
                if (b1 && Meth()) return 1;
                if (MethCalled == true) return 1;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic018
        {
            public static int Main_old()
            {
                bool b1 = true;
                bool b2 = true;
                bool b3 = false;
                bool b4 = false;
                if (!(b1 || b2)) return 1;
                if (!(b1 || b3)) return 1;
                if (!(b4 || b2)) return 1;
                if ((b3 || b4)) return 1;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_operators_logic019
        {
            public static bool MethCalled = false;
            public static bool Meth()
            {
                MethCalled = true;
                return true;
            }
            public static int Main_old()
            {
                bool b1 = true;
                if (!(b1 || Meth())) return 1;
                if (MethCalled == true) return 1;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        /*
        class Other_TestClass_operators_logic022
        {
            public static Other_TestClass_operators_logic022 operator &(Other_TestClass_operators_logic022 t1, Other_TestClass_logic022 t2)
            {
                return new Other_TestClass_operators_logic022();
            }
            public static bool operator true(Other_TestClass_operators_logic022 t1)
            {
                return true;
            }

            public static bool operator false(Other_TestClass_operators_logic022 t1)
            {
                return false;
            }
            public static int Main_old()
            {
                Other_TestClass_operators_logic022 test1 = new Other_TestClass_operators_logic022();
                Other_TestClass_operators_logic022 test2 = new Other_TestClass_operators_logic022();
                if (test1 && test2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        */


        /*
        class Other_TestClass_operators_logic023
        {
            public static Other_TestClass_operators_logic023 operator |(Other_TestClass_operators_logic023 t1, Other_TestClass_logic023 t2)
            {
                return new Other_TestClass_operators_logic023();
            }
            public static bool operator true(Other_TestClass_operators_logic023 t1)
            {
                return true;
            }

            public static bool operator false(Other_TestClass_operators_logic023 t1)
            {
                return false;
            }
            public static int Main_old()
            {
                Other_TestClass_operators_logic023 test1 = new Other_TestClass_operators_logic023();
                Other_TestClass_operators_logic023 test2 = new Other_TestClass_operators_logic023();
                if (test1 || test2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        */

        /*
        class Other_TestClass_operators_logic032
        {
            public static int MethCall = 0;
            public static Other_TestClass_operators_logic032 operator &(Other_TestClass_operators_logic032 t1, Other_TestClass_logic032 t2)
            {
                return new Other_TestClass_operators_logic032();
            }
            public static bool operator true(Other_TestClass_operators_logic032 t1)
            {
                return true;
            }

            public static bool operator false(Other_TestClass_operators_logic032 t1)
            {
                return true;
            }
            public static Other_TestClass_operators_logic032 RetClass()
            {
                MethCall = 1; ;
                return new Other_TestClass_operators_logic032();
            }
            public static int Main_old()
            {
                Other_TestClass_operators_logic032 test1 = new Other_TestClass_operators_logic032();
                if (test1 && RetClass()) { }
                if (MethCall == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        */ 
        class Other_TestClass_operators_logic033
        {
            public static int MethCall = 0;
            public static Other_TestClass_operators_logic033 operator |(Other_TestClass_operators_logic033 t1, Other_TestClass_operators_logic033 t2)
            {
                return new Other_TestClass_operators_logic033();
            }
            public static bool operator true(Other_TestClass_operators_logic033 t1)
            {
                return true;
            }

            public static bool operator false(Other_TestClass_operators_logic033 t1)
            {
                return true;
            }
            public static Other_TestClass_operators_logic033 RetClass()
            {
                MethCall = 1; ;
                return new Other_TestClass_operators_logic033();
            }
            public static int Main_old()
            {
                Other_TestClass_operators_logic033 test1 = new Other_TestClass_operators_logic033();
                if (test1 || RetClass()) { }
                if (MethCall == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_cond001
        {
            public static int Main_old()
            {
                bool b = true;
                int intI = b ? 3 : 4;
                if (intI == 3)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_cond002
        {
            public static int Main_old()
            {
                bool b = false;
                int intI = b ? 3 : 4;
                if (intI == 4)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_cond003
        {
            static bool Pass = true;
            public static int retThree()
            {
                return 3;
            }
            public static int retFour()
            {
                Pass = false;
                return 4;
            }
            public static int Main_old()
            {
                bool b = true;
                int intI = b ? retThree() : retFour();
                if ((intI == 3) && (Pass == true))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_cond004
        {
            static bool Pass = true;
            public static int retThree()
            {
                Pass = false;
                return 3;
            }
            public static int retFour()
            {
                return 4;
            }
            public static int Main_old()
            {
                bool b = false;
                int intI = b ? retThree() : retFour();
                if ((intI == 4) && (Pass == true))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_cond005
        {
            public static int Main_old()
            {
                bool b1 = false;
                bool b2 = false;
                int intI = b1 ? 1 : b2 ? 2 : 3;
                if (intI == 3)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Others_TestClass_cond006
        {
            public static implicit operator bool(Others_TestClass_cond006 t)
            {
                return true;
            }
        }
        class Other_TestClass_cond006
        {
            public static int Main_old()
            {
                Others_TestClass_cond006 t = new Others_TestClass_cond006();
                int intI = t ? 2 : 3;
                if (intI == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Others_TestClass_cond008
        {
            public static bool operator true(Others_TestClass_cond008 t)
            {
                return true;
            }
            public static bool operator false(Others_TestClass_cond008 t)
            {
                return false;
            }
        }
        class Other_TestClass_cond008
        {
            public static int Main_old()
            {
                Others_TestClass_cond008 t = new Others_TestClass_cond008();
                int intI = t ? 2 : 3;
                if (intI == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        interface Others_TestClass_cond010_Inter
        {
            int RetInt();
        }
        class Others_TestClass_cond0101 : Others_TestClass_cond010_Inter
        {
            public static implicit operator Others_TestClass_cond0102(Others_TestClass_cond0101 t)
            {
                return new Others_TestClass_cond0102();
            }
            public int RetInt()
            {
                return 1;
            }
        }
        class Others_TestClass_cond0102 : Others_TestClass_cond010_Inter
        {
            public int RetInt()
            {
                return 2;
            }
        }
        class Other_TestClass_cond010
        {
            public static int Main_old()
            {
                bool b = true;
                Others_TestClass_cond0101 t1 = new Others_TestClass_cond0101();
                Others_TestClass_cond0102 t2 = new Others_TestClass_cond0102();
                Others_TestClass_cond010_Inter test = b ? t1 : t2;
                if (test.RetInt() == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        interface Others_TestClass_cond011_Inter
        {
            int RetInt();
        }
        class Others_TestClass_cond0111 : Others_TestClass_cond011_Inter
        {
            public int RetInt()
            {
                return 1;
            }
        }
        class Others_TestClass_cond0112 : Others_TestClass_cond011_Inter
        {
            public static implicit operator Others_TestClass_cond0111(Others_TestClass_cond0112 t)
            {
                return new Others_TestClass_cond0111();
            }
            public int RetInt()
            {
                return 2;
            }
        }
        class Other_TestClass_cond011
        {
            public static int Main_old()
            {
                bool b = false;
                Others_TestClass_cond0111 t1 = new Others_TestClass_cond0111();
                Others_TestClass_cond0112 t2 = new Others_TestClass_cond0112();
                Others_TestClass_cond011_Inter test = b ? t1 : t2;
                if (test.RetInt() == 1)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Others_TestClass_cond014
        {
            public int ChkVal = 0;
            public static implicit operator bool(Others_TestClass_cond014 MT)
            {
                MT.ChkVal = 1;
                return true;
            }
            public static bool operator true(Others_TestClass_cond014 MT)
            {
                MT.ChkVal = 2;
                return true;
            }
            public static bool operator false(Others_TestClass_cond014 MT)
            {
                MT.ChkVal = 3;
                return false;
            }
        }
        class Other_TestClass_cond014
        {
            public static int Main_old()
            {
                Others_TestClass_cond014 TC = new Others_TestClass_cond014();
                int intI = TC ? 1 : 2;
                if ((intI == 1) && (TC.ChkVal == 1))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Others_TestClass_cond015
        {
            public int ChkVal = 0;
            public static explicit operator bool(Others_TestClass_cond015 MT)
            {
                MT.ChkVal = 1;
                return true;
            }
            public static bool operator true(Others_TestClass_cond015 MT)
            {
                MT.ChkVal = 2;
                return true;
            }
            public static bool operator false(Others_TestClass_cond015 MT)
            {
                MT.ChkVal = 3;
                return false;
            }
        }
        class Other_TestClass_cond015
        {
            public static int Main_old()
            {
                Others_TestClass_cond015 TC = new Others_TestClass_cond015();
                int intI = TC ? 1 : 2;
                if ((intI == 1) && (TC.ChkVal == 2))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_is005
        {
            public static int Main_old()
            {
                int myInt = 3;
                bool b = myInt is int;
                if (b == true)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public struct Others_TestClass_is006_Str
        {
        }
        public class Other_TestClass_is006
        {
            public static int Main_old()
            {
                Others_TestClass_is006_Str ms = new Others_TestClass_is006_Str();
                bool b = ms is Others_TestClass_is006_Str;
                if (b == true)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_is007_T { }
        public class Other_TestClass_is007
        {
            public static int Main_old()
            {
                Other_TestClass_is007_T tt = null;
                object o = tt;
                bool b = o is Other_TestClass_is007_T;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_is008_T { }
        public class Other_TestClass_is008
        {
            public static int Main_old()
            {
                string tt = null;
                object o = tt;
                bool b = o is string;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Others_TestClass_is009_Inter { }
        public class Other_TestClass_is009_T : Others_TestClass_is009_Inter { }
        public class Other_TestClass_is009
        {
            public static int Main_old()
            {
                Others_TestClass_is009_Inter tt = null;
                object o = tt;
                bool b = o is Others_TestClass_is009_Inter;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_is010_T { }
        public class AnotherType { }
        public class Other_TestClass_is010
        {
            public static int Main_old()
            {
                Other_TestClass_is010_T tt = new Other_TestClass_is010_T();
                object o = tt;
                bool b = o is AnotherType;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_is011_A { }
        public class Other_TestClass_is011
        {
            public static int Main_old()
            {
                string s = "foo";
                object o = s;
                bool b = o is Other_TestClass_is011_A;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Others_TestClass_is012_Inter { }
        public interface AnotherInter { }
        public class Other_TestClass_is012_T : Others_TestClass_is012_Inter { }
        public class Other_TestClass_is012
        {
            public static int Main_old()
            {
                Other_TestClass_is012_T tt = new Other_TestClass_is012_T();
                object o = tt;
                bool b = o is AnotherInter;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is013_Base { }
        public class Others_TestClass_is013_Der : Others_TestClass_is013_Base { }
        public class Other_TestClass_is013
        {
            public static int Main_old()
            {
                Others_TestClass_is013_Base mb = new Others_TestClass_is013_Base();
                object o = mb;
                bool b = o is Others_TestClass_is013_Der;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is0141
        {
            public static explicit operator Others_TestClass_is0142(Others_TestClass_is0141 m)
            {
                return new Others_TestClass_is0142();
            }
        }
        public class Others_TestClass_is0142 { }
        public class Other_TestClass_is014
        {
            public static int Main_old()
            {
                Others_TestClass_is0141 mt = new Others_TestClass_is0141();
                object o = mt;
                bool b = o is Others_TestClass_is0142;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is015 { }
        public class Other_TestClass_is015
        {
            public static int Main_old()
            {
                object o = new object();
                bool b = o is Others_TestClass_is015;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_is016_T { }
        public class Other_TestClass_is016
        {
            public static int Main_old()
            {
                Other_TestClass_is016_T tt = new Other_TestClass_is016_T();
                object o = tt;
                bool b = o is Other_TestClass_is016_T;
                if (b == true)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_is017
        {
            public static int Main_old()
            {
                string s = "foo";
                object o = s;
                bool b = o is string;
                if (b == true)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Others_TestClass_is018_Inter { }
        public class Other_TestClass_is018_T : Others_TestClass_is018_Inter { }
        public class Other_TestClass_is018
        {
            public static int Main_old()
            {
                Others_TestClass_is018_Inter mi = new Other_TestClass_is018_T();
                object o = mi;
                bool b = o is Others_TestClass_is018_Inter;
                if (b == true)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Others_TestClass_is019_Inter { }
        public class Other_TestClass_is019_T : Others_TestClass_is019_Inter { }
        public class Other_TestClass_is019
        {
            public static int Main_old()
            {
                Other_TestClass_is019_T mi = new Other_TestClass_is019_T();
                object o = mi;
                bool b = o is Others_TestClass_is019_Inter;
                if (b == true)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is020_Base { }
        public class Others_TestClass_is020_Der : Others_TestClass_is020_Base { }
        public class Other_TestClass_is020
        {
            public static int Main_old()
            {
                Others_TestClass_is020_Der md = new Others_TestClass_is020_Der();
                object o = md;
                bool b = o is Others_TestClass_is020_Base;
                if (b == true)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is0211
        {
            public static implicit operator Others_TestClass_is0212(Others_TestClass_is0211 m)
            {
                return new Others_TestClass_is0212();
            }
        }
        public class Others_TestClass_is0212 { }
        public class Other_TestClass_is021
        {
            public static int Main_old()
            {
                Others_TestClass_is0211 mt = new Others_TestClass_is0211();
                object o = mt;
                bool b = o is Others_TestClass_is0212;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is022 { }
        public class Other_TestClass_is022
        {
            public static int Main_old()
            {
                Others_TestClass_is022 tc = new Others_TestClass_is022();
                object o = tc;
                bool b = o is object;
                if (b == true)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is023 { }
        public class Other_TestClass_is023
        {
            public static int Main_old()
            {
                Others_TestClass_is023 tc = new Others_TestClass_is023();
                bool b = tc is Others_TestClass_is023;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is024 { }
        public class Other_TestClass_is024
        {
            public static int Main_old()
            {
                string tc = "foo";
                bool b = tc is string;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Others_TestClass_is025_Inter { }
        public class Other_TestClass_is025
        {
            public static int Main_old()
            {
                Others_TestClass_is025_Inter mi = null;
                bool b = mi is Others_TestClass_is025_Inter;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Others_TestClass_is026_Inter { }
        public class Others_TestClass_is026 : Others_TestClass_is026_Inter { }
        public class Other_TestClass_is026
        {
            public static int Main_old()
            {
                Others_TestClass_is026 tc = new Others_TestClass_is026(); ;
                bool b = tc is Others_TestClass_is026_Inter;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is027_Base { }
        public class Others_TestClass_is027 : Others_TestClass_is027_Base { }
        public class Other_TestClass_is027
        {
            public static int Main_old()
            {
                Others_TestClass_is027 tc = new Others_TestClass_is027();
                bool b = tc is Others_TestClass_is027_Base;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is0281
        {
            public static implicit operator Others_TestClass_is0282(Others_TestClass_is0281 m)
            {
                return new Others_TestClass_is0282();
            }
        }
        public class Others_TestClass_is0282 { }
        public class Other_TestClass_is028
        {
            public static int Main_old()
            {
                Others_TestClass_is0281 mt = new Others_TestClass_is0281();
                bool b = mt is Others_TestClass_is0282;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is029 { }
        public class AnotherClass { }
        public class Other_TestClass_is029
        {
            public static int Main_old()
            {
                Others_TestClass_is029 tc = new Others_TestClass_is029();
                bool b = tc is AnotherClass;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is030 { }
        public class Other_TestClass_is030
        {
            public static int Main_old()
            {
                Others_TestClass_is030 tc = new Others_TestClass_is030();
                bool b = tc is string;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Others_TestClass_is031_Inter { }
        public interface Others_TestClass_is031_Inter2 { }
        public class Other_TestClass_is031
        {
            public static int Main_old()
            {
                Others_TestClass_is031_Inter mi = null;
                bool b = mi is Others_TestClass_is031_Inter2;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public interface Others_TestClass_is032_Inter { }
        public class Others_TestClass_is032 { }
        public class Other_TestClass_is032
        {
            public static int Main_old()
            {
                Others_TestClass_is032 tc = new Others_TestClass_is032(); ;
                bool b = tc is Others_TestClass_is032_Inter;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is033_Base { }
        public class Others_TestClass_is033 : Others_TestClass_is033_Base { }
        public class Other_TestClass_is033
        {
            public static int Main_old()
            {
                Others_TestClass_is033_Base tc = new Others_TestClass_is033_Base();
                bool b = tc is Others_TestClass_is033;
                if (b == false)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_is0341
        {
            public static explicit operator Others_TestClass_is0342(Others_TestClass_is0341 m)
            {
                return new Others_TestClass_is0342();
            }
        }
        public class Others_TestClass_is0342 { }
        public class Other_TestClass_is034
        {
            public static int Main_old()
            {
                Others_TestClass_is0341 mt = new Others_TestClass_is0341();
                bool b = mt is Others_TestClass_is0342;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_as001
        {
            public static int Main_old()
            {
                string s = "MyString" as string;
                if (s == "MyString")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_as002
        {
            public static int Main_old()
            {
                object o = 5 as object;
                if ((int)o == 5)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_as003
        {
            public static int Main_old()
            {
                string s = "hello";
                object o = s;
                if ((o as System.Array) == null)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_as004
        {
            public static int Main_old()
            {
                string[] s = new string[] { "hello" };
                object o = s;
                if ((o as System.Array) != null)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_as007
        {
            public static int Main_old()
            {
                string[] s = new string[] { "hello" };
                object o = s;
                if ((o as System.Array) as System.Array != null)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_as008
        {
            public static int Main_old()
            {
                string[] s = new string[] { "hello" };
                object o = s;
                if ((o as System.Array) as object != null)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Other_TestClass_as011_A { }
        class Other_TestClass_as011_B : Other_TestClass_as011_A { }
        class Other_TestClass_as011_C : Other_TestClass_as011_B { }
        class D : Other_TestClass_as011_C { }
        class E : Other_TestClass_as011_C { }
        class F : E { }
        class G : F { }
        class Other_TestClass_as011
        {
            static int Main_old()
            {
                Other_TestClass_as011_A a = new F();
                F f = a as F;
                if (f != null)
                    return 0;
                return 1;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        interface IA { }
        class Other_TestClass_as012_A : IA { }
        class Other_TestClass_as012_B : Other_TestClass_as012_A { }
        class Other_TestClass_as012_C : Other_TestClass_as012_A { }
        class Other_TestClass_as012
        {
            public static int Main_old()
            {
                Other_TestClass_as012_A ia = new Other_TestClass_as012_B();
                Other_TestClass_as012_C c = ia as Other_TestClass_as012_C;
                if (c != null)
                    return 1;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_add001
        {
            public static int Main_old()
            {
                string s = "foo" + "bar";
                if (s == "foobar")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_add002
        {
            public static int Main_old()
            {
                string s = "foo" + 1;
                if (s == "foo1")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_add003
        {
            public static int Main_old()
            {
                string s = 1 + "foo";
                if (s == "1foo")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_add004_T { }
        public class Other_TestClass_add004
        {
            public static int Main_old()
            {

                Other_TestClass_add004_T tc = new Other_TestClass_add004_T();
                string s = "foo" + tc;
                if (s == "fooMicrosoft.SPOT.Platform.Tests.OtherTests1+Other_TestClass_add004_T")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_add005_T { }
        public class Other_TestClass_add005
        {
            public static int Main_old()
            {

                Other_TestClass_add005_T tc = new Other_TestClass_add005_T();
                string s = tc + "foo";
                if (s == "Microsoft.SPOT.Platform.Tests.OtherTests1+Other_TestClass_add005_Tfoo")
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Others_TestClass_add006 { }
        public class Other_TestClass_add006
        {
            public static int Main_old()
            {

                return 0;
                string s = "foo" + "bar";
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic001
        {
            public static int Main_old()
            {
                int i1 = 2;
                if ((i1 & 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic002
        {
            public static int Main_old()
            {
                int i1 = 2;
                if ((0 & i1) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic003
        {
            public static int Main_old()
            {
                sbyte sb1 = 2;
                if ((sb1 & 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic004
        {
            public static int Main_old()
            {
                byte b1 = 2;
                if ((b1 & 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic005
        {
            public static int Main_old()
            {
                short s1 = 2;
                if ((s1 & 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic006
        {
            public static int Main_old()
            {
                ushort us1 = 2;
                if ((us1 & 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic007
        {
            public static int Main_old()
            {
                uint ui1 = 2;
                if ((ui1 & 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic008
        {
            public static int Main_old()
            {
                long l1 = 2;
                if ((l1 & 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic009
        {
            public static int Main_old()
            {
                ulong ul1 = 2;
                if ((ul1 & 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic010
        {
            public static int Main_old()
            {
                char c1 = (char)2;
                if ((c1 & 0) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic011
        {
            public static int Main_old()
            {
                int i1 = 2;
                if ((0 | i1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic012
        {
            public static int Main_old()
            {
                sbyte sb1 = 2;
                if ((0 | sb1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic013
        {
            public static int Main_old()
            {
                byte b1 = 2;
                if ((0 | b1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic014
        {
            public static int Main_old()
            {
                short s1 = 2;
                if ((0 | s1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic015
        {
            public static int Main_old()
            {
                ushort us1 = 2;
                if ((0 | us1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic016
        {
            public static int Main_old()
            {
                uint ui1 = 2;
                if ((0 | ui1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic017
        {
            public static int Main_old()
            {
                long l1 = 2;
                if ((0 | l1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic018
        {
            public static int Main_old()
            {
                ulong ul1 = 2;
                if ((0 | ul1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic019
        {
            public static int Main_old()
            {
                char c1 = (char)2;
                if ((0 | c1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic020
        {
            public static int Main_old()
            {
                int i1 = 2;
                if ((0 ^ i1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic021
        {
            public static int Main_old()
            {
                sbyte sb1 = 2;
                if ((0 ^ sb1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic022
        {
            public static int Main_old()
            {
                byte b1 = 2;
                if ((0 ^ b1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic023
        {
            public static int Main_old()
            {
                short s1 = 2;
                if ((0 ^ s1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic024
        {
            public static int Main_old()
            {
                ushort us1 = 2;
                if ((0 ^ us1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic025
        {
            public static int Main_old()
            {
                uint ui1 = 2;
                if ((0 ^ ui1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic026
        {
            public static int Main_old()
            {
                long l1 = 2;
                if ((0 ^ l1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic027
        {
            public static int Main_old()
            {
                ulong ul1 = 2;
                if ((0 ^ ul1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic028
        {
            public static int Main_old()
            {
                char c1 = (char)2;
                if ((0 ^ c1) == 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic029
        {
            public static int Main_old()
            {
                byte a = 0x01;
                byte b = 0x02;
                if ((a ^ b & a | b) != (a | b & a ^ b))
                    return 1;
                if ((a ^ a ^ a | a ^ b) != 0x03)
                    return 1;
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic030
        {
            public static int Main_old()
            {
                {
                    sbyte a = 0x01; sbyte b = 0x02;
                    if ((a ^ b & a | b) != (a | b & a ^ b))
                        return 1;
                    if ((a ^ a ^ a | a ^ b) != 0x03)
                        return 1;
                }
                {
                    short a = 0x01; short b = 0x02;
                    if ((a ^ b & a | b) != (a | b & a ^ b))
                        return 1;
                    if ((a ^ a ^ a | a ^ b) != 0x03)
                        return 1;
                }
                {
                    int a = 0x01; int b = 0x02;
                    if ((a ^ b & a | b) != (a | b & a ^ b))
                        return 1;
                    if ((a ^ a ^ a | a ^ b) != 0x03)
                        return 1;
                }
                {
                    long a = 0x01; long b = 0x02;
                    if ((a ^ b & a | b) != (a | b & a ^ b))
                        return 1;
                    if ((a ^ a ^ a | a ^ b) != 0x03)
                        return 1;
                }
                {
                    byte a = 0x01; byte b = 0x02;
                    if ((a ^ b & a | b) != (a | b & a ^ b))
                        return 1;
                    if ((a ^ a ^ a | a ^ b) != 0x03)
                        return 1;
                }
                {
                    ushort a = 0x01; ushort b = 0x02;
                    if ((a ^ b & a | b) != (a | b & a ^ b))
                        return 1;
                    if ((a ^ a ^ a | a ^ b) != 0x03)
                        return 1;
                }
                {
                    uint a = 0x01; uint b = 0x02;
                    if ((a ^ b & a | b) != (a | b & a ^ b))
                        return 1;
                    if ((a ^ a ^ a | a ^ b) != 0x03)
                        return 1;
                }
                {
                    ulong a = 0x01; ulong b = 0x02;
                    if ((a ^ b & a | b) != (a | b & a ^ b))
                        return 1;
                    if ((a ^ a ^ a | a ^ b) != 0x03)
                        return 1;
                }
                {
                    char a = (char)0x01; char b = (char)0x02;
                    if ((a ^ b & a | b) != (a | b & a ^ b))
                        return 1;
                    if ((a ^ a ^ a | a ^ b) != 0x03)
                        return 1;
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        public class Other_TestClass_logic032
        {
            public static int Main_old()
            {
                checked
                {
                    {
                        sbyte a = 0x0f; sbyte b = 0x01;
                        if ((a ^ b & a | b) != (a | b & a ^ b))
                            return 1;
                        if ((a ^ a ^ a | a ^ b) != 0x0f)
                            return 1;
                    }
                    {
                        short a = 0xff; short b = 0x01;
                        if ((a ^ b & a | b) != (a | b & a ^ b))
                            return 1;
                        if ((a ^ a ^ a | a ^ b) != 0xff)
                            return 1;
                    }
                    {
                        int a = 0xff; int b = 0x01;
                        if ((a ^ b & a | b) != (a | b & a ^ b))
                            return 1;
                        if ((a ^ a ^ a | a ^ b) != 0xff)
                            return 1;
                    }
                    {
                        long a = 0xff; long b = 0x01;
                        if ((a ^ b & a | b) != (a | b & a ^ b))
                            return 1;
                        if ((a ^ a ^ a | a ^ b) != 0xff)
                            return 1;
                    }
                    {
                        byte a = 0xff; byte b = 0x01;
                        if ((a ^ b & a | b) != (a | b & a ^ b))
                            return 1;
                        if ((a ^ a ^ a | a ^ b) != 0xff)
                            return 1;
                    }
                    {
                        ushort a = 0xff; ushort b = 0x01;
                        if ((a ^ b & a | b) != (a | b & a ^ b))
                            return 1;
                        if ((a ^ a ^ a | a ^ b) != 0xff)
                            return 1;
                    }
                    {
                        uint a = 0xff; uint b = 0x01;
                        if ((a ^ b & a | b) != (a | b & a ^ b))
                            return 1;
                        if ((a ^ a ^ a | a ^ b) != 0xff)
                            return 1;
                    }
                    {
                        ulong a = 0xff; ulong b = 0x01;
                        if ((a ^ b & a | b) != (a | b & a ^ b))
                            return 1;
                        if ((a ^ a ^ a | a ^ b) != 0xff)
                            return 1;
                    }
                    {
                        char a = (char)0xff; char b = (char)0x01;
                        if ((a ^ b & a | b) != (a | b & a ^ b))
                            return 1;
                        if ((a ^ a ^ a | a ^ b) != 0xff)
                            return 1;
                    }
                }
                return 0;
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }



    }
}

