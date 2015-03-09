using System;


namespace Microsoft.SPOT.Samples.SaveMyWine
{
    [Serializable]
    public sealed class Range
    {
        public Range(double min, double max)
        {
            Minimum = min;
            Maximum = max;
        }

        public double Minimum;
        public double Maximum;
    }
}
