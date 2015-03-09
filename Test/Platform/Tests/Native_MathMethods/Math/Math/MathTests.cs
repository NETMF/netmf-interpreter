////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Net;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class MathTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        double PositiveInfinity = System.Double.PositiveInfinity;
        double NegativeInfinity = System.Double.NegativeInfinity;
        double NaN              = System.Double.NaN;

       
        #region ---------Math.cs------------------------------

        [TestMethod]
        public MFTestResults Test_Not_Numbers()//(double val,double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            double nan = (0.0 / 0.0);
            double pos_inf = (3.0 / 0.0);
            double neg_inf = (-3.0 / 0.0);

            if (!Double.IsNaN(nan))
            {
                ///<Test-Err> Err-29 </Test-Err>
                Log.Comment("NaN was not correctly identified");
                result = MFTestResults.Fail;
            }
            if (Double.IsPositiveInfinity(nan))
            {
                ///<Test-Err> Err-29 </Test-Err>
                Log.Comment("NaN was incorrectly identified as Positive Infinity");
                result = MFTestResults.Fail;
            }
            if (Double.IsNegativeInfinity(nan))
            {
                ///<Test-Err> Err-29 </Test-Err>
                Log.Comment("NaN was incorrectly identified as Negative Infinity");
                result = MFTestResults.Fail;
            }

            //--//

            if (!Double.IsPositiveInfinity(pos_inf))
            {
                ///<Test-Err> Err-29 </Test-Err>
                Log.Comment("Positive Infinity was not correctly identified");
                result = MFTestResults.Fail;
            }
            if (Double.IsNaN(pos_inf))
            {
                ///<Test-Err> Err-29 </Test-Err>
                Log.Comment("Positive Infinity  was incorrectly identified as NaN");
                result = MFTestResults.Fail;
            }
            if (Double.IsNegativeInfinity(pos_inf))
            {
                ///<Test-Err> Err-29 </Test-Err>
                Log.Comment("Positive Infinity  was incorrectly identified as Negative Infinity");
                result = MFTestResults.Fail;
            }

            //--//

            if (!Double.IsNegativeInfinity(neg_inf))
            {
                ///<Test-Err> Err-29 </Test-Err>
                Log.Comment("NegativeInfinity was not correctly identified");
                result = MFTestResults.Fail;
            }
            if (Double.IsPositiveInfinity(neg_inf))
            {
                ///<Test-Err> Err-29 </Test-Err>
                Log.Comment("NegativeInfinity Infinity was incorrectly identified as Positive Infinity");
                result = MFTestResults.Fail;
            }
            if (Double.IsNaN(neg_inf))
            {
                ///<Test-Err> Err-29 </Test-Err>
                Log.Comment("NegativeInfinity Infinity was incorrectly identified as NaN");
                result = MFTestResults.Fail;
            }
            
            return result;
        }
        [TestMethod]
        public MFTestResults Test_Abs_double()//(double val,double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            double val = -0.0029832;
            double answer = 0.0029832;
            double res = System.Math.Abs(val);

            if (res != answer)
            {
                ///<Test-Err> Err-28 </Test-Err>
                Log.Comment("Abs(...double val - negative)\t\t -- FAILED AT:\t" + res);
                result = MFTestResults.Fail;
            }
            else
                Log.Comment("Abs(...double val - negative)\t\t -- PASS");

            val = 0.0029832;
            answer = 0.0029832;
            res = System.Math.Abs(val);
            if (res != answer)
            {
                ///<Test-Err> Err-29 </Test-Err>
                Log.Comment("Abs(...double val - positive)\t\t -- FAILED AT:\t" + res);
                result = MFTestResults.Fail;
            }
            else
                Log.Comment("Abs(...double val - positive)\t\t -- PASS");
            return result;
        }
        [TestMethod]
        public MFTestResults Test_Acos()//(double d,double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //double d = 1;            0     30          60 90 120  150         180
            double[] d = new double[] { 1, 0.866025403, 0.5, 0, -0.5, -0.866025403, -1 };
            double[] answer = new double[] { 0, 0.523598776, 1.047197551, 1.570796327, 2.094395102, 2.617993878, 3.141592654 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Acos(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-30 </Test-Err>
                    Log.Comment("Acos(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Acos(..." + d[i] + ")\t\t -- PASS");
            }
            res = System.Math.Acos(-2);
            if (res.ToString("F0") == "NaN")
            {
                Log.Comment("Acos(...-2)\t\t -- PASS");
            }
            else
            {
                ///<Test-Err> Err-30 </Test-Err>
                Log.Comment("Acos(...-2)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            res = System.Math.Acos(2);
            if (res.ToString("F0") == "NaN")
            {
                Log.Comment("Acos(...2)\t\t -- PASS");
            }
            else
            {
                ///<Test-Err> Err-30 </Test-Err>
                Log.Comment("Acos(...2)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }

        [TestMethod]
        public MFTestResults Test_Asin()//(double d, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //double d = 1;            0     30          60 90 120  150         180
            double[] d = new double[] { 0.00000000000000000000, 0.50000000000000000000, 0.86602540378443900000, 1.00000000000000000000/*, -0.86602540378443900000, -0.50000000000000000000, -0.00000000000000012251 */};
            double[] answer = new double[] { 0, 0.523598776, 1.047197551, 1.570796327/*, 2.094395102, 2.617993878, 3.141592654 */};
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Asin(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-31 </Test-Err>
                    Log.Comment("Asin(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Asin(..." + d[i] + ")\t\t -- PASS");
            }
            res = System.Math.Asin(-2);
            if (res.ToString("F0") == "NaN")
            {
                Log.Comment("Asin(...-2)\t\t -- PASS");
            }
            else
            {
                ///<Test-Err> Err-31 </Test-Err>
                Log.Comment("Asin(...-2)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            res = System.Math.Asin(2);
            if (res.ToString("F0") == "NaN")
            {
                Log.Comment("Asin(...2)\t\t -- PASS");
            }
            else
            {
                ///<Test-Err> Err-31 </Test-Err>
                Log.Comment("Asin(...2)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }
        [TestMethod]
        public MFTestResults Test_Atan()//(double d, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            double[] d = new double[] { 0.00000000000000000000, 0.57735026918962600000, 1.73205080756888000000, -1.73205080756888000000, -0.57735026918962600000,/*NaN,*/PositiveInfinity, NegativeInfinity };
            double[] answer = new double[] { 0, 0.523598776, 1.047197551,/* 1.570796327,*/-1.047197551, -0.523598776, 1.5707963267949, -1.5707963267949 };//2.094395102, 2.617993878, 3.141592654 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Atan(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-32 </Test-Err>
                    Log.Comment("Atan(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Atan(..." + d[i] + ")\t\t -- PASS");
            }
            res = System.Math.Atan(NaN);
            if (res.ToString("F0") == "NaN")
            {
                Log.Comment("Atan(...NaN)\t\t -- PASS");
            }
            else
            {
                ///<Test-Err> Err-32 </Test-Err>
                Log.Comment("Atan(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }
        [TestMethod]
        public MFTestResults Test_Atan2()//(double y, double x,double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the angle whose tangent is the quotient of two specified numbers.
            //
            // Parameters:
            //   y:
            //     The y coordinate of a point.
            //
            //   x:
            //     The x coordinate of a point.
            //
            // Returns:
            //     An angle, ¦È, measured in radians, such that -¦Ð¡Ü¦È¡Ü¦Ð, and tan(¦È) = y / x, where
            //     (x, y) is a point in the Cartesian plane. Observe the following: For (x,
            //     y) in quadrant 1, 0 < ¦È < ¦Ð/2.  For (x, y) in quadrant 2, ¦Ð/2 < ¦È¡Ü¦Ð.  For
            //     (x, y) in quadrant 3, -¦Ð < ¦È < -¦Ð/2.  For (x, y) in quadrant 4, -¦Ð/2 < ¦È
            //     < 0.  For points on the boundaries of the quadrants, the return value is
            //     the following: If y is 0 and x is not negative, ¦È = 0.  If y is 0 and x is
            //     negative, ¦È = ¦Ð.  If y is positive and x is 0, ¦È = ¦Ð/2.  If y is negative
            //     and x is 0, ¦È = -¦Ð/2.
            double[] _x = new double[] { 0.00000000000000000000, 0.57735026918962600000, 2.59807621135332000000, -4.33012701892220000000, -1.73205080756888000000, -0.00000000000000042880 };
            double[] _y = new double[] { 0.5, 1, 1.5, 2.5, 3, 3.5 };
            double[] answers = new double[] { 0.00000000000000000000, 0.52359877559829900000, 1.04719755119659000000, -1.04719755119659000000, -0.52359877559829900000, -0.00000000000000012251 };
            double res;
            for (int i = 0; i < _x.Length; i++)
            {
                res = System.Math.Atan2(_x[i], _y[i]); ;
                if ((answers[i] - res) > 0.0001d || (answers[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-33 </Test-Err>
                    Log.Comment("Atan2(..." + _x[i] + ", " + _y[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Atan2(..." + _x[i] + ", " + _y[i] + ")\t\t -- PASS");
            }

            double y = 0;
            double x = 1;
            double answer = 0;
            res = System.Math.Atan2(y, x);

            if (res != answer)
            {
                ///<Test-Err> Err-33 </Test-Err>
                Log.Comment("Atan2(...0, not -ve)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            else
                Log.Comment("Atan2(...0, not -ve)\t\t -- PASS");
            y = 0;
            x = -1;
            answer = 3.141592654;  // ¦Ð
            res = System.Math.Atan2(y, x);

            if ((answer - res) > 0.0001d || (answer - res) < -0.0001d)//if (res != answer)
            {
                ///<Test-Err> Err-33 </Test-Err>
                Log.Comment("Atan2(...0, -ve)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            else
                Log.Comment("Atan2(...0, -ve)\t\t -- PASS");
            y = 2;
            x = 0;
            answer = 1.570796327; // ¦Ð/2 
            res = System.Math.Atan2(y, x);

            if ((answer - res) > 0.0001d || (answer - res) < -0.0001d)//if (res != answer)
            {
                ///<Test-Err> Err-33 </Test-Err>
                Log.Comment("Atan2(...+ve, 0)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            else
                Log.Comment("Atan2(...+ve, 0)\t\t -- PASS");
            y = -2;
            x = 0;
            answer = -1.570796327; //-¦Ð/2 
            res = System.Math.Atan2(y, x);

            if ((answer - res) > 0.0001d || (answer - res) < -0.0001d)//if (res != answer)
            {
                ///<Test-Err> Err-33 </Test-Err>
                Log.Comment("Atan2(...0, -ve)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            else
                Log.Comment("Atan2(...0, -ve)\t\t -- PASS");
           return result;
        }
        [TestMethod]
        public MFTestResults Test_Ceiling()//(double d, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            // Summary:
            //     Returns the smallest integer greater than or equal to the specified double-precision
            //     floating-point number.
            //
            // Parameters:
            //   a:
            //     A double-precision floating-point number.
            //
            // Returns:
            //     The smallest integer greater than or equal to a. If a is equal to System.Double.NaN,
            //     System.Double.NegativeInfinity, or System.Double.PositiveInfinity, that value
            //     is returned.
            double[] d = new double[] { 0.5, 1.1, 9.5, 19.8 };
            double[] answer = new double[] { 1, 2, 10, 20 };

            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Ceiling(d[i]);
                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer)
                {
                    ///<Test-Err> Err-34 </Test-Err>
                    Log.Comment("Ceiling(... " + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Ceiling(... " + d[i] + ")\t\t -- PASS");
                //return 0;
            }
            res = System.Math.Ceiling(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Ceiling(... nan)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-34 </Test-Err>
                Log.Comment("Ceiling(... nan)\t\t -- FAILED AT:\t" + res.ToString());
               result = MFTestResults.Fail;
            }
            //return 0;
            res = System.Math.Ceiling(NegativeInfinity);
            if (res.ToString() == "-Infinity")
                Log.Comment("Ceiling(... NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-34 </Test-Err>
                Log.Comment("Ceiling(... NegativeInfinity)\t\t -- FAILED AT:\t" + res.ToString());
               result = MFTestResults.Fail;
            }
            //return 0;
            res = System.Math.Ceiling(PositiveInfinity);
            if (res.ToString() == "Infinity") //if (res != answer)
                Log.Comment("Ceiling(... PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-34 </Test-Err>
                Log.Comment("Ceiling(... PositiveInfinity)\t\t -- FAILED AT:\t" + res.ToString());
               result = MFTestResults.Fail;
            }
           return result;
        }
        [TestMethod]
        public MFTestResults Test_Cos()//(double a, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            // Summary:
            //     Returns the cosine of the specified angle.
            //
            // Parameters:
            //   d:
            //     An angle, measured in radians.
            //
            // Returns:
            //     The cosine of d.
            double[] d = new double[] { 0.00000000000000000000, 0.52359877559829900000, 1.04719755119660000000, 1.57079632679490000000, 2.09439510239320000000, 2.61799387799149000000, 3.14159265358979000000 };
            double[] answer = new double[] { 1.00000000000000000000, 0.86602540378443900000, 0.50000000000000000000, 0.00000000000000006126, -0.50000000000000000000, -0.86602540378443900000, -1.00000000000000000000 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Cos(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-35 </Test-Err>
                    Log.Comment("Cos(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Cos(..." + d[i] + ")\t\t -- PASS");
            }
            res = System.Math.Cos(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Cos(... NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-35 </Test-Err>
                Log.Comment("Cos(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

           return result;
        }
        [TestMethod]
        public MFTestResults Test_Cosh()//(double a, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the hyperbolic cosine of the specified angle.
            //
            // Parameters:
            //   value:
            //     An angle, measured in radians.
            //
            // Returns:
            //     The hyperbolic cosine of value. If value is equal to System.Double.NegativeInfinity
            //     or System.Double.PositiveInfinity, System.Double.PositiveInfinity is returned.
            //     If value is equal to System.Double.NaN, System.Double.NaN is returned.
            double[] d = new double[] { 0.00000000000000000000, 0.52359877559829900000, 1.04719755119660000000, 1.57079632679490000000, 2.09439510239320000000, 2.61799387799149000000, 3.14159265358979000000 };
            double[] answer = new double[] { 1.00000000000000000000, 1.14023832107643000000, 1.60028685770239000000, 2.50917847865806000000, 4.12183605386995000000, 6.89057236497588000000, 11.59195327552150000000 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Cosh(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-36 </Test-Err>
                    Log.Comment("Cosh(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Cosh(..." + d[i] + ")\t\t -- PASS");
            }
            res = System.Math.Cosh(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Cosh(... NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-36 </Test-Err>
                Log.Comment("Cosh(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            //return 0;
            res = System.Math.Cosh(NegativeInfinity);
            if (res.ToString() == "Infinity")
                Log.Comment("Cosh(... NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-36 </Test-Err>
                Log.Comment("Cosh(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            //return 0;
            res = System.Math.Cosh(PositiveInfinity);
            if (res.ToString() == "Infinity")
                Log.Comment("Cosh(...PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-36 </Test-Err>
                Log.Comment("Cosh(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }
        [TestMethod]
        public MFTestResults Test_IEEERemainder()//(double x, double y, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the remainder resulting from the division of a specified number by
            //     another specified number.
            //
            // Parameters:
            //   x:
            //     A dividend.
            //
            //   y:
            //     A divisor.
            //
            // Returns:
            //     A number equal to x - (y Q), where Q is the quotient of x / y rounded to
            //     the nearest integer (if x / y falls halfway between two integers, the even
            //     integer is returned).  If x - (y Q) is zero, the value +0 is returned if
            //     x is positive, or -0 if x is negative.  If y = 0, System.Double.NaN (Not-A-Number)
            //     is returned.
            double[] x = new double[] { 10, 7, 8, 20 };
            double[] y = new double[] { 3, 2, 5, 12 };
            double[] answer = new double[] { 1, -1, -2, -4 };
            double res;
            int quotient = (int)(7 / 2);
            int r = 7 - quotient * 2;
            for (int i = 0; i < x.Length; i++)
            {
                res = System.Math.IEEERemainder(x[i], y[i]);
                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-37 </Test-Err>
                    Log.Comment("IEEERemainder(..." + x[i] + ", " + y[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("IEEERemainder(..." + x[i] + ", " + y[i] + ")\t\t -- PASS");
            }

            //res = System.Math.IEEERemainder(3, 2); // x/y = 1.5, case of when x/y falls halfway between two integers
            //if ((res -2)>0.0001d)//if (res != answer[i])
            //{
            //    ///<Test-Err> Err-37 </Test-Err>
            //    Log.Comment("IEEERemainder(...3,2)\t\t -- FAILED AT:\t" + res);
            //   result = MFTestResults.Fail;
            //}
            //else
            //    Log.Comment("IEEERemainder(...3,2)\t\t -- PASS");

            res = System.Math.IEEERemainder(4, 2); // x/y = 2, case when x - (y Q) is zero, the value +0 is returned if x is positive
            if (res != 0)//if (res != answer[i])
            {
                ///<Test-Err> Err-37 </Test-Err>
                Log.Comment("IEEERemainder(...4,2)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            else
                Log.Comment("IEEERemainder(...4,2)\t\t -- PASS");

            res = System.Math.IEEERemainder(-4, 2); // x/y = -2, case when x - (y Q) is zero, the value -0 is returned if x is negative
            if (res != 0)//if (res != answer[i])
            {
                ///<Test-Err> Err-37 </Test-Err>
                Log.Comment("IEEERemainder(...-4,2)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            else
                Log.Comment("IEEERemainder(...-4,2)\t\t -- PASS");

            res = System.Math.IEEERemainder(3, 0); // case when If y = 0, System.Double.NaN (Not-A-Number) is returned.
            if (res.ToString() == "NaN")
                Log.Comment("IEEERemainder(...3,0)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-37 </Test-Err>
                Log.Comment("IEEERemainder(...3,0)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

           return result;
        }
        [TestMethod]
        public MFTestResults Test_Exp()//(double d, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns e raised to the specified power.
            //
            // Parameters:
            //   d:
            //     A number specifying a power.
            //
            // Returns:
            //     The number e raised to the power d. If d equals System.Double.NaN or System.Double.PositiveInfinity,
            //     that value is returned. If d equals System.Double.NegativeInfinity, 0 is
            //     returned.
            double[] d = new double[] { 0.00000000000000000000, 0.50000000000000000000, 1.00000000000000000000, 1.50000000000000000000, 2.00000000000000000000, 2.50000000000000000000, 3.00000000000000000000, 3.50000000000000000000, 4.00000000000000000000, 4.50000000000000000000, 5.00000000000000000000, 5.50000000000000000000, 6.00000000000000000000, 6.50000000000000000000, 7.00000000000000000000, 7.50000000000000000000 };
            double[] answer = new double[] { 1, 1.648721271, 2.718281828, 4.48168907, 7.389056099, 12.18249396, 20.08553692, 33.11545196, 54.59815003, 90.0171313, 148.4131591, 244.6919323, 403.4287935, 665.141633, 1096.633158, 1808.042414 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Exp(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-38 </Test-Err>
                    Log.Comment("Exp(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Exp(..." + d[i] + ")\t\t -- PASS");
            }

            res = System.Math.Exp(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Exp(...NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-38 </Test-Err>
                Log.Comment("Exp(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Exp(PositiveInfinity);
            if (res.ToString() == "Infinity")
                Log.Comment("Exp(...PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-38 </Test-Err>
                Log.Comment("Exp(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            res = System.Math.Exp(NegativeInfinity);
            if (res == 0)
                Log.Comment("Exp(...NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-38 </Test-Err>
                Log.Comment("Exp(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }
        [TestMethod]
        public MFTestResults Test_Floor()//(double d, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the largest integer less than or equal to the specified double-precision
            //     floating-point number.
            //
            // Parameters:
            //   d:
            //     A double-precision floating-point number.
            //
            // Returns:
            //     The largest integer less than or equal to d. If d is equal to System.Double.NaN,
            //     System.Double.NegativeInfinity, or System.Double.PositiveInfinity, that value
            //     is returned.
            double[] d = new double[] { -2, -2.5, 0.00000000000000000000, 0.50000000000000000000, 1.00000000000000000000, 1.50000000000000000000, 2.00000000000000000000, 2.50000000000000000000, 3.00000000000000000000, 3.50000000000000000000, 4.00000000000000000000, 4.50000000000000000000, 5.00000000000000000000, 5.50000000000000000000, 6.00000000000000000000, 6.50000000000000000000, 7.00000000000000000000, 7.50000000000000000000 };
            double[] answer = new double[] { -2, -3, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Floor(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-39 </Test-Err>
                    Log.Comment("Floor(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Floor(..." + d[i] + ")\t\t -- PASS");
            }

            res = System.Math.Floor(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Floor(...NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-39 </Test-Err>
                Log.Comment("Floor(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Floor(PositiveInfinity);
            if (res.ToString() == "Infinity")
                Log.Comment("Floor(...PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-39 </Test-Err>
                Log.Comment("Floor(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            res = System.Math.Floor(NegativeInfinity);
            if (res.ToString() == "-Infinity")
                Log.Comment("Floor(...NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-39 </Test-Err>
                Log.Comment("Floor(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

           return result;

        }
        [TestMethod]
        public MFTestResults Test_Log()//(double d, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the natural (base e) logarithm of a specified number.
            //
            // Parameters:
            //   d:
            //     A number whose logarithm is to be found.
            //
            // Returns:
            //     Sign of d Returns Positive The natural logarithm of d; that is, ln d, or
            //     log ed Zero System.Double.NegativeInfinity Negative System.Double.NaN If
            //     d is equal to System.Double.NaN, returns System.Double.NaN. If d is equal
            //     to System.Double.PositiveInfinity, returns System.Double.PositiveInfinity.
            double[] d = new double[] { 0.50000000000000000000, 1.00000000000000000000, 1.50000000000000000000, 2.00000000000000000000, 2.50000000000000000000, 3.00000000000000000000, 3.50000000000000000000, 4.00000000000000000000, 4.50000000000000000000, 5.00000000000000000000, 5.50000000000000000000, 6.00000000000000000000, 6.50000000000000000000, 7.00000000000000000000, 7.50000000000000000000 };
            double[] answer = new double[] { -0.693147181, 0, 0.405465108, 0.693147181, 0.916290732, 1.098612289, 1.252762968, 1.386294361, 1.504077397, 1.609437912, 1.704748092, 1.791759469, 1.871802177, 1.945910149, 2.014903021 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Log(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-40 </Test-Err>
                    Log.Comment("Log(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Log(..." + d[i] + ")\t\t -- PASS");
            }

            res = System.Math.Log(0);
            if (res.ToString() == "-Infinity")
                Log.Comment("Log(...0)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-40 </Test-Err>
                Log.Comment("Log(...0)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Log(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Log(...NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-40 </Test-Err>
                Log.Comment("Log(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Log(PositiveInfinity);
            if (res.ToString() == "Infinity")
                Log.Comment("Log(...PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-40 </Test-Err>
                Log.Comment("Log(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Log(NegativeInfinity);
            if (res.ToString() == "NaN")
                Log.Comment("Log(...NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-40 </Test-Err>
                Log.Comment("Log(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

           return result;
        }
        [TestMethod]
        public MFTestResults Test_Log10()//(double d, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the base 10 logarithm of a specified number.
            //
            // Parameters:
            //   d:
            //     A number whose logarithm is to be found.
            //
            // Returns:
            //     Sign of d Returns Positive The base 10 log of d; that is, log 10d. Zero System.Double.NegativeInfinity
            //     Negative System.Double.NaN If d is equal to System.Double.NaN, this method
            //     returns System.Double.NaN. If d is equal to System.Double.PositiveInfinity,
            //     this method returns System.Double.PositiveInfinity.
            double[] d = new double[] { 0.50000000000000000000, 1.00000000000000000000, 1.50000000000000000000, 2.00000000000000000000, 2.50000000000000000000, 3.00000000000000000000, 3.50000000000000000000, 4.00000000000000000000, 4.50000000000000000000, 5.00000000000000000000, 5.50000000000000000000, 6.00000000000000000000, 6.50000000000000000000, 7.00000000000000000000, 7.50000000000000000000 };
            double[] answer = new double[] { -0.301029996, 0, 0.176091259, 0.301029996, 0.397940009, 0.477121255, 0.544068044, 0.602059991, 0.653212514, 0.698970004, 0.740362689, 0.77815125, 0.812913357, 0.84509804, 0.875061263 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Log10(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-41 </Test-Err>
                    Log.Comment("Log10(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Log10(..." + d[i] + ")\t\t -- PASS");
            }

            res = System.Math.Log10(0);
            if (res.ToString("F0") == "-Infinity")//
                Log.Comment("Log10(...0)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-41 </Test-Err>
                Log.Comment("Log10(...0)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Log10(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Log10(...NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-41 </Test-Err>
                Log.Comment("Log10(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Log10(PositiveInfinity);
            if (res.ToString() == "Infinity")
                Log.Comment("Log10(...PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-41 </Test-Err>
                Log.Comment("Log10(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Log10(NegativeInfinity);
            if (res.ToString() == "NaN")
                Log.Comment("Log10(...NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-41 </Test-Err>
                Log.Comment("Log10(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

           return result;
        }
        [TestMethod]
        public MFTestResults Test_Max_2()//(double x, double y, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the larger of two double-precision floating-point numbers.
            //
            // Parameters:
            //   val1:
            //     The first of two double-precision floating-point numbers to compare.
            //
            //   val2:
            //     The second of two double-precision floating-point numbers to compare.
            //
            // Returns:
            //     Parameter val1 or val2, whichever is larger. If val1 OR both val1
            //     and val2 are equal to System.Double.NaN, System.Double.NaN is returned.
            double[] x = new double[] { 6.00000000000000000000, 6.50000000000000000000, 7.00000000000000000000, 7.50000000000000000000, -0.50000000000000000000, -1.00000000000000000000, -1.50000000000000000000, -2.00000000000000000000 };
            double[] y = new double[] { 1, 1.140238321, 1.600286858, 2.509178479, 4.121836054, 6.890572365, 11.59195328, -0.50000000000000000000 };

            double[] answer = new double[] { 6.00000000000000000000, 6.50000000000000000000, 7.00000000000000000000, 7.50000000000000000000, 4.12183605386995000000, 6.89057236497588000000, 11.59195328, -0.50000000000000000000 };
            double res;
            for (int i = 0; i < x.Length; i++)
            {
                res = System.Math.Max(x[i], y[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//
                {
                    ///<Test-Err> Err-42 </Test-Err>
                    Log.Comment("Max(..." + x[i] + ", " + y[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Max(..." + x[i] + ", " + y[i] + ")\t\t -- PASS");
            }



            res = System.Math.Max(10, NaN);
            if (res == 10)//if ((res.ToString("F0") == "nan") || (res.ToString("F0") == "-nan"))//
                Log.Comment("Max(...10, NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-42 </Test-Err>
                Log.Comment("Max(...10, NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Max(NaN, 10);
            if (res.ToString() == "NaN")
                Log.Comment("Max(...NaN, 10)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-42 </Test-Err>
                Log.Comment("Max(...NaN, 10)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Max(NaN, NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Max(...NaN, NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-42 </Test-Err>
                Log.Comment("Max(...NaN, NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

           return result;
        }
        [TestMethod]
        public MFTestResults Test_Min_2()//(double x, double y, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the smaller of two double-precision floating-point numbers.
            //
            // Parameters:
            //   val1:
            //     The first of two double-precision floating-point numbers to compare.
            //
            //   val2:
            //     The second of two double-precision floating-point numbers to compare.
            //
            // Returns:
            //     Parameter val1 or val2, whichever is smaller. If val1, val2, or both val1
            //     and val2 are equal to System.Double.NaN, System.Double.NaN is returned.
            double[] x = new double[] { 6.00000000000000000000, 6.50000000000000000000, 7.00000000000000000000, 7.50000000000000000000, -0.50000000000000000000, -1.00000000000000000000, -1.50000000000000000000, -2.00000000000000000000 };
            double[] y = new double[] { 1, 1.140238321, 1.600286858, 2.509178479, 4.121836054, 6.890572365, 11.59195328, -0.50000000000000000000 };

            double[] answer = new double[] { 1, 1.140238321, 1.600286858, 2.509178479, -0.50000000000000000000, -1.00000000000000000000, -1.50000000000000000000, -2.00000000000000000000 };
            double res;
            for (int i = 0; i < x.Length; i++)
            {
                res = System.Math.Min(x[i], y[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//
                {
                    ///<Test-Err> Err-43 </Test-Err>
                    Log.Comment("Min(..." + x[i] + ", " + y[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Min(..." + x[i] + ", " + y[i] + ")\t\t -- PASS");
            }



            res = System.Math.Min(10, NaN);
            if (res == 10)//if ((res.ToString("F0") == "nan") || (res.ToString("F0") == "-nan"))//
                Log.Comment("Min(...10, NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-43 </Test-Err>
                Log.Comment("Min(...10, NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Min(NaN, 10);
            if (res.ToString() == "NaN")
                Log.Comment("Min(...NaN, 10)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-43 </Test-Err>
                Log.Comment("Min(...NaN, 10)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Min(NaN, NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Min(...NaN, NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-43 </Test-Err>
                Log.Comment("Min(...NaN, NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

           return result;

        }
        [TestMethod]
        public MFTestResults Test_Pow_2()//(double x, double y, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns a specified number raised to the specified power.
            //
            // Parameters:
            //   x:
            //     A double-precision floating-point number to be raised to a power.
            //
            //   y:
            //     A double-precision floating-point number that specifies a power.
            //
            // Returns:
            //     The number x raised to the power y.
            double[] x = new double[] { 5, 6.00000000000000000000, 6.50000000000000000000, 7.00000000000000000000, 7.50000000000000000000/* -0.50000000000000000000, -1.00000000000000000000, -1.50000000000000000000, -2.00000000000000000000*/ };
            double[] y = new double[] { 0.5, 1, 1.140238321, 1.600286858, 2.509178479 /*4.121836054, 6.890572365, 11.59195328, -0.50000000000000000000*/ };

            double[] answer = new double[] { 2.23606797749979000000, 6.00000000000000000000, 8.45113347506690000000, 22.51123320954050000000, 156.92238129089800000000 /*NaN, NaN, NaN, NaN*/ };
            double res;

            for (int i = 0; i < x.Length; i++)
            {
                res = System.Math.Pow(x[i], y[i]);
                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//
                {
                    ///<Test-Err> Err-44 </Test-Err>
                    Log.Comment("Pow(..." + x[i] + ", " + y[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Pow(..." + x[i] + ", " + y[i] + ")\t\t -- PASS");
            }

            x = new double[] { -0.50000000000000000000, -1.00000000000000000000, -1.50000000000000000000, -2.00000000000000000000 };
            y = new double[] { 4.121836054, 6.890572365, 11.59195328, -0.50000000000000000000 };

            for (int i = 0; i < x.Length; i++)
            {
                res = System.Math.Pow(x[i], y[i]);
                if (res.ToString() == "NaN")
                    Log.Comment("Pow(..." + x[i] + ", " + y[i] + ")\t\t -- PASS");
                else
                {
                    ///<Test-Err> Err-44 </Test-Err>
                    Log.Comment("Pow(..." + x[i] + ", " + y[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
            }

            res = System.Math.Pow(10, NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Pow(...10, NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-44 </Test-Err>
                Log.Comment("Pow(...10, NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Pow(NaN, 10);
            if (res.ToString() == "NaN")
                Log.Comment("Pow(...NaN, 10)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-44 </Test-Err>
                Log.Comment("Pow(...NaN, 10)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Pow(NaN, NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Pow(...NaN, NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-44 </Test-Err>
                Log.Comment("Pow(...NaN, NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

           return result;

        }
        [TestMethod]
        public MFTestResults Test_Round()//(double d, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            // Summary:
            //     Rounds a double-precision floating-point value to the nearest integer.
            //
            // Parameters:
            //   a:
            //     A double-precision floating-point number to be rounded.
            //
            // Returns:
            //     The integer nearest a. If the fractional component of a is halfway between
            //     two integers, one of which is even and the other odd, then the even number
            //     is returned.
            double[] d = new double[] { -0.5, -1.2, 0.5, 1.2, 1.5, 6.00000000000000000000, 6.50000000000000000000, 7.00000000000000000000, 7.50000000000000000000 };

            double[] answer = new double[] { 0, -1, 0, 1, 2, 6, 6, 7, 8 };
            double res;

            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Round(d[i]);
                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//
                {
                    ///<Test-Err> Err-45 </Test-Err>
                    Log.Comment("Round(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Round(..." + d[i] + ")\t\t -- PASS");
            }



            res = System.Math.Round(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Round(...NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-45 </Test-Err>
                Log.Comment("Round(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

           return result;
        }
        [TestMethod]
        public MFTestResults Test_Sign()//(double value, int answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns a value indicating the sign of a double-precision floating-point
            //     number.
            //
            // Parameters:
            //   value:
            //     A signed number.
            //
            // Returns:
            //     A number indicating the sign of value.  Number Description -1 value is less
            //     than zero. 0 value is equal to zero. 1 value is greater than zero.
            //
            // Exceptions:
            //   System.ArithmeticException:
            //     value is equal to System.Double.NaN.
            //     is returned.
            double[] d = new double[] { -0.5, -1.2, 0, 1.2, 1.5, 6, 6.5, 7, 8.5 };

            double[] answer = new double[] { -1, -1, 0, 1, 1, 1, 1, 1, 1 };
            double res;
            double[] res1 = new double[d.Length];
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Sign(d[i]);// the output is wrong, needs to be corrected 
                res1[i] = res;
                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//
                {
                    ///<Test-Err> Err-46 </Test-Err>
                    Log.Comment("Sign(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                    result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Sign(..." + d[i] + ")\t\t -- PASS");
            }


            res = System.Math.Sign(NaN);
            if (res == 0)//((res.ToString("F0") == "nan") || (res.ToString("F0") == "-nan"))//
                Log.Comment("Sign(...NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-46 </Test-Err>
                Log.Comment("Sign(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

           return result;
        }
        [TestMethod]
        public MFTestResults Test_Sin()//(double a, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            // Summary:
            //     Returns the sine of the specified angle.
            //
            // Parameters:
            //   a:
            //     An angle, measured in radians.
            //
            // Returns:
            //     The sine of a. If a is equal to System.Double.NaN, System.Double.NegativeInfinity,
            //     or System.Double.PositiveInfinity, this method returns System.Double.NaN.
            double[] d = new double[] { 0.00000000000000000000, 0.52359877559829900000, 1.04719755119660000000, 1.57079632679490000000, 2.09439510239320000000, 2.61799387799149000000, 3.14159265358979000000 };
            double[] answer = new double[] { 0.00000000000000000000, 0.50000000000000000000, 0.86602540378443900000, 1.00000000000000000000, 0.86602540378443900000, 0.50000000000000000000, 0.00000000000000012251 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Sin(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-47 </Test-Err>
                    Log.Comment("Sin(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Sin(..." + d[i] + ")\t\t -- PASS");
            }

            res = System.Math.Sin(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Sin(...NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-47 </Test-Err>
                Log.Comment("Sin(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            res = System.Math.Sin(PositiveInfinity);
            if (res.ToString() == "NaN")
                Log.Comment("Sin(...PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-47 </Test-Err>
                Log.Comment("Sin(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            res = System.Math.Sin(NegativeInfinity);
            if (res.ToString() == "NaN")
                Log.Comment("Sin(...NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-47 </Test-Err>
                Log.Comment("Sin(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }
        [TestMethod]
        public MFTestResults Test_Sinh()//(double value, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the hyperbolic sine of the specified angle.
            //
            // Parameters:
            //   value:
            //     An angle, measured in radians.
            //
            // Returns:
            //     The hyperbolic sine of value. If value is equal to System.Double.NegativeInfinity,
            //     System.Double.PositiveInfinity, or System.Double.NaN, this method returns
            //     a System.Double equal to value.
            double[] d = new double[] { 0.00000000000000000000, 0.52359877559829900000, 1.04719755119660000000, 1.57079632679490000000, 2.09439510239320000000, 2.61799387799149000000, 3.14159265358979000000 };
            double[] answer = new double[] { 0.00000000000000000000, 0.54785347388804000000, 1.24936705052398000000, 2.30129890230729000000, 3.99869134279982000000, 6.81762330412654000000, 11.54873935725770000000 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Sinh(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-48 </Test-Err>
                    Log.Comment("Sinh(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Sinh(..." + d[i] + ")\t\t -- PASS");
            }

            res = System.Math.Sinh(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Sinh(... NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-48 </Test-Err>
                Log.Comment("Sinh(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Sinh(NegativeInfinity);
            if (res.ToString() == "-Infinity")
                Log.Comment("Sinh(... NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-48 </Test-Err>
                Log.Comment("Sinh(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Sinh(PositiveInfinity);
            if (res.ToString() == "Infinity")
                Log.Comment("Sinh(... PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-48 </Test-Err>
                Log.Comment("Sinh(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }
        [TestMethod]
        public MFTestResults Test_Sqrt()//(double d, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the square root of a specified number.
            //
            // Parameters:
            //   d:
            //     A number.
            //
            // Returns:
            //     Value of d Returns Zero, or positive The positive square root of d. Negative
            //     System.Double.NaN If d is equal to System.Double.NaN or System.Double.PositiveInfinity,
            //     that value is returned.
            double[] d = new double[] { 0.00000000000000000000, 0.52359877559829900000, 1.04719755119660000000, 2.09439510239320000000, 2.61799387799149000000, 3.14159265358979000000 };
            double[] answer = new double[] { 0.00000000000000000000, 0.72360125455826800000, 1.02332670794649000000, 1.44720250911654000000, 1.61802159379642000000, 1.77245385090552000000 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Sqrt(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-49 </Test-Err>
                    Log.Comment("Sqrt(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Sqrt(..." + d[i] + ")\t\t -- PASS");
            }

            res = System.Math.Sqrt(-2);
            if (res.ToString() == "NaN")
                Log.Comment("Sqrt(... -ve)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-49 </Test-Err>
                Log.Comment("Sqrt(...-ve)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Sqrt(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Sqrt(... NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-49 </Test-Err>
                Log.Comment("Sqrt(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Sqrt(NegativeInfinity);
            if (res.ToString() == "NaN")
                Log.Comment("Sqrt(... NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-49 </Test-Err>
                Log.Comment("Sqrt(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Sqrt(PositiveInfinity);
            if (res.ToString() == "Infinity")
                Log.Comment("Sqrt(... PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-49 </Test-Err>
                Log.Comment("Sqrt(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }
        [TestMethod]
        public MFTestResults Test_Tan()//(double a, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the tangent of the specified angle.
            //
            // Parameters:
            //   a:
            //     An angle, measured in radians.
            //
            // Returns:
            //     The tangent of a. If a is equal to System.Double.NaN, System.Double.NegativeInfinity,
            //     or System.Double.PositiveInfinity, this method returns System.Double.NaN.
            double[] d = new double[] { 0.00000000000000000000, 0.52359877559829900000, 1.04719755119660000000, /*1.57079632679490000000, */2.09439510239320000000, 2.61799387799149000000, 3.14159265358979000000 };
            double[] answer = new double[] { 0.00000000000000000000, 0.57735026918962600000, 1.73205080756888000000, -1.73205080756888000000, -0.57735026918962600000, -0.00000000000000012251 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Tan(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-50 </Test-Err>
                    Log.Comment("Tan(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Tan(..." + d[i] + ")\t\t -- PASS");
            }

            //res = System.Math.Tan(1.57079632679490000000); // pi/2 
            //if ((res.ToString("F0") == "nan") || (res.ToString("F0") == "-nan"))//if (res != answer)
            //    Log.Comment("Tan(...PI/2)\t\t -- PASS");
            //else
            //{
            //    ///<Test-Err> Err-50 </Test-Err>
            //    Log.Comment("Tan(...PI/2)\t\t -- FAILED AT:\t" + res);
            //   result = MFTestResults.Fail;
            //}

            res = System.Math.Tan(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Tan(...NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-50 </Test-Err>
                Log.Comment("Tan(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            res = System.Math.Tan(PositiveInfinity);
            if (res.ToString() == "NaN")
                Log.Comment("Tan(...PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-50 </Test-Err>
                Log.Comment("Tan(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
            res = System.Math.Tan(NegativeInfinity);
            if (res.ToString() == "NaN")
                Log.Comment("Tan(...NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-50 </Test-Err>
                Log.Comment("Tan(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }
        [TestMethod]
        public MFTestResults Test_Tanh()//(double value, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Returns the hyperbolic tangent of the specified angle.
            //
            // Parameters:
            //   value:
            //     An angle, measured in radians.
            //
            // Returns:
            //     The hyperbolic tangent of value. If value is equal to System.Double.NegativeInfinity,
            //     this method returns -1. If value is equal to System.Double.PositiveInfinity,
            //     this method returns 1. If value is equal to System.Double.NaN, this method
            //     returns System.Double.NaN.
            double[] d = new double[] { 0.00000000000000000000, 0.52359877559829900000, 1.04719755119660000000, 1.57079632679490000000, 2.09439510239320000000, 2.61799387799149000000, 3.14159265358979000000 };
            double[] answer = new double[] { 0.00000000000000000000, 0.48047277815645200000, 0.78071443535926800000, 0.91715233566727400000, 0.97012382116593100000, 0.98941320735268200000, 0.99627207622075000000 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Tanh(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-51 </Test-Err>
                    Log.Comment("Tanh(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Tanh(..." + d[i] + ")\t\t -- PASS");
            }

            res = System.Math.Tanh(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Tanh(... NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-51 </Test-Err>
                Log.Comment("Tanh(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Tanh(NegativeInfinity);
            if (res == -1)//if (res.ToString("F0") == "-inf")//if (res != answer)
                Log.Comment("Tanh(... NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-51 </Test-Err>
                Log.Comment("Tanh(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Tanh(PositiveInfinity);
            if (res == 1)//if (res.ToString("F0") == "inf") //if (res != answer)
                Log.Comment("Tanh(... PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-51 </Test-Err>
                Log.Comment("Tanh(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }
        [TestMethod]
        public MFTestResults Test_Truncate()//(double d, double answer)
        {
            MFTestResults result = MFTestResults.Pass;
            //
            // Summary:
            //     Calculates the integral part of a specified double-precision floating-point
            //     number.
            //
            // Parameters:
            //   d:
            //     A number to truncate.
            //
            // Returns:
            //     The integral part of d; that is, the number that remains after any fractional
            //     digits have been discarded.
            double[] d = new double[] { 0.00000000000000000000, 0.52359877559829900000, 1.04719755119660000000, 1.57079632679490000000, 2.09439510239320000000, 2.61799387799149000000, 3.14159265358979000000 };
            double[] answer = new double[] { 0, 0, 1, 1, 2, 2, 3 };
            double res;
            for (int i = 0; i < d.Length; i++)
            {
                res = System.Math.Truncate(d[i]);

                if ((answer[i] - res) > 0.0001d || (answer[i] - res) < -0.0001d)//if (res != answer[i])
                {
                    ///<Test-Err> Err-52 </Test-Err>
                    Log.Comment("Truncate(..." + d[i] + ")\t\t -- FAILED AT:\t" + res);
                   result = MFTestResults.Fail;
                }
                else
                    Log.Comment("Truncate(..." + d[i] + ")\t\t -- PASS");
            }

            res = System.Math.Truncate(NaN);
            if (res.ToString() == "NaN")
                Log.Comment("Truncate(... NaN)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-52 </Test-Err>
                Log.Comment("Truncate(...NaN)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Truncate(NegativeInfinity);
            if (res.ToString() == "-Infinity")
                Log.Comment("Truncate(... NegativeInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-52 </Test-Err>
                Log.Comment("Truncate(...NegativeInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }

            res = System.Math.Truncate(PositiveInfinity);
            if (res.ToString() == "Infinity")
                Log.Comment("Truncate(... PositiveInfinity)\t\t -- PASS");
            else
            {
                ///<Test-Err> Err-52 </Test-Err>
                Log.Comment("Truncate(...PositiveInfinity)\t\t -- FAILED AT:\t" + res);
               result = MFTestResults.Fail;
            }
           return result;
        }
        #endregion
 
    }
}
