
using Microsoft.SPOT.Platform.Test;

class NS_TestClass_compunit_03A
{
    public void printClassName()
    {
        NS_TestClass_compunit_03B cB = new NS_TestClass_compunit_03B();
        cB.showName();
    }

    public void showName()
    {
        Log.Comment("Class A");
    }
}