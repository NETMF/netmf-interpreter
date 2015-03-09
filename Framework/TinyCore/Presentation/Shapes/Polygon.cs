////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Presentation.Shapes
{
    public class Polygon : Shape
    {

        public Polygon()
        {
        }

        public Polygon(int[] pts)
        {
            Points = pts;
        }

        public int[] Points
        {
            get
            {
                return _pts;
            }

            set
            {
                if(value == null || value.Length == 0)
                {
                    throw new ArgumentException();
                }
                
                _pts = value;

                InvalidateMeasure();
            }
        }

        public override void OnRender(Media.DrawingContext dc)
        {
            if (_pts != null)
            {
                dc.DrawPolygon(Fill, Stroke, _pts);
            }
        }

        private int[] _pts;
    }
}


