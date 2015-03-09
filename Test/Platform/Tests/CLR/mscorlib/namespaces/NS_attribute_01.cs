
using System;

using Microsoft.SPOT.Platform.Test;

[assembly:CLSCompliant(true)]

class NS_TestClass_attribute_01
{
	public void printClassName()
	{
		Log.Comment("Class A");
	}

	static void Main_old() {}

    public static bool testMethod()
    {
        Main_old();
        return true;
    }
}