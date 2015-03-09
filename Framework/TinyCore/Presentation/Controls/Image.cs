////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    /// <summary>
    /// Summary description for Image.
    /// </summary>
    public class Image : UIElement
    {
        public Image()
        {
        }

        public Image(Bitmap bmp)
            : this()
        {
            _bitmap = bmp;
        }

        public Bitmap Bitmap
        {
            get
            {
                VerifyAccess();

                return _bitmap;
            }

            set
            {
                VerifyAccess();

                _bitmap = value;
                InvalidateMeasure();
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            desiredWidth = desiredHeight = 0;
            if (_bitmap != null)
            {
                desiredWidth = _bitmap.Width;
                desiredHeight = _bitmap.Height;
            }
        }

        public override void OnRender(DrawingContext dc)
        {
            Bitmap bmp = _bitmap;
            if (bmp != null)
            {
                dc.DrawImage(_bitmap, 0, 0);
            }
        }

        private Bitmap _bitmap;
    }
}


