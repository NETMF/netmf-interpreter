////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Microsoft.SPOT.Emulator.Lcd
{
    internal class LcdDriver : HalDriver<ILcdDriver>, ILcdDriver
    {
        LcdDisplay _display;

        private LcdDisplay GetLcdDisplay( bool writeTraceOnError )
        {
            if (_display == null)
            {
                _display = this.Emulator.LcdDisplay ?? new LcdDisplayNull();
            }

            if (writeTraceOnError && (_display is LcdDisplayNull))
            {
                LcdDisplayNull nullDisplay = (LcdDisplayNull)_display;

                if (nullDisplay.DisplayWarning || Emulator.Verbose)
                {
                    Trace.WriteLine( "Warning: System attempts to access the LcdDisplay when none is configured." );
                    nullDisplay.TurnOffWarning();
                }
            }

            return _display;
        }

        #region ILcd Members

        bool ILcdDriver.Initialize()
        {
            return true;
        }

        bool ILcdDriver.Uninitialize()
        {
            return true;
        }

        bool ILcdDriver.RefreshController()
        {
            throw new Exception( "The method or operation is not implemented." );
        }

        void ILcdDriver.Clear()
        {
            GetLcdDisplay(true).DeviceClear();
        }

        IntPtr ILcdDriver.GetFrameBuffer()
        {
            throw new Exception( "The method or operation is not implemented." );
        }

        void ILcdDriver.BitBltEx(int x, int y, int width, int height, IntPtr data)
        {
            GetLcdDisplay(true).DevicePaint(x, y, width, height, data);
        }

        void ILcdDriver.BitBlt(int width, int height, int widthInWords, IntPtr data, bool fUseDelta)
        {
            Debug.Assert(false, "BitBlt not supported");
            throw new NotImplementedException("BitBlt not supported");
        }

        void ILcdDriver.SetContrast( byte Contrast )
        {
            throw new Exception( "The method or operation is not implemented." );
        }

        void ILcdDriver.PowerSave( bool On )
        {
            throw new Exception( "The method or operation is not implemented." );
        }

        void ILcdDriver.SetOscillatorFrequency( byte OscillatorFrequency )
        {
            throw new Exception( "The method or operation is not implemented." );
        }

        void ILcdDriver.WriteChar( byte c, int row, int col )
        {
            throw new Exception( "The method or operation is not implemented." );
        }

        void ILcdDriver.WriteFormattedChar( char c )
        {
            throw new Exception( "The method or operation is not implemented." );
        }

        int ILcdDriver.GetWidth()
        {
            //This occurs only because the debugger uses this.
            //Perhaps we should change the CLR to query LCD_Initialize first, 
            //and let the debugger know we don't support an LCD

            return GetLcdDisplay(false).DeviceWidth; // The system always requires a width for the LCD even if one does not exist
        }

        int ILcdDriver.GetHeight()
        {
            return GetLcdDisplay(false).DeviceHeight; // The system always requires a height for the LCD even if one does not exist
        }

        int ILcdDriver.GetBitsPerPixel()
        {
            return GetLcdDisplay(false).DeviceBitsPerPixel; // The system always requires a bpp value for the LCD even if one does not exist
        }

        #endregion
    }

    public delegate void OnDevicePaintEventHandler(object sender, OnDevicePaintEventArgs args);

    public class OnDevicePaintEventArgs : EventArgs
    {

        public Rectangle Rectangle { get; private set; }
        public Bitmap Bitmap { get; private set; }

        public OnDevicePaintEventArgs(Rectangle rectangle, Bitmap bitmap)
        {
            this.Rectangle = rectangle;
            this.Bitmap = bitmap;
        }
    }

    public class LcdDisplay : EmulatorComponent
    {
        int _width;
        int _height;
        int _bitsPerPixel;
        Bitmap _bitmap;
        OnDevicePaintEventHandler _devicePaintHandler;

        public LcdDisplay() : this(220, 176, 16)
        {
        }

        protected LcdDisplay( int w, int h, int bpp )
        {
            _width = w;
            _height = h;
            _bitsPerPixel = bpp;
        }

        public override void SetupComponent()
        {
            _bitmap = CreateBitmap( _width, _height, _bitsPerPixel, new uint[0] );
        }

        public override bool IsReplaceableBy( EmulatorComponent ec )
        {
            return (ec is LcdDisplay);
        }

        public event OnDevicePaintEventHandler OnDevicePaint
        {
            add { _devicePaintHandler += value; }
            remove { _devicePaintHandler -= value; }
        }

        private System.Drawing.Bitmap CreateBitmap( int width, int height, int bitsPerPixel, uint[] buf )
        {
            System.Drawing.Bitmap bmp;
            System.Drawing.Imaging.PixelFormat pixelFormat;

            System.Drawing.Color[] colors = null;

            switch (bitsPerPixel)
            {
                case 1:
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format1bppIndexed;
                    colors = new System.Drawing.Color[] { System.Drawing.Color.White, System.Drawing.Color.Black };
                    break;
                case 4:
                case 8:
                    //Not tested
                    int cColors = 1 << bitsPerPixel;

                    pixelFormat = (bitsPerPixel == 4) ? System.Drawing.Imaging.PixelFormat.Format4bppIndexed : System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

                    colors = new System.Drawing.Color[cColors];

                    for (int i = 0; i < cColors; i++)
                    {
                        int intensity = 256 / cColors * i;
                        colors[i] = System.Drawing.Color.FromArgb( intensity, intensity, intensity );
                    }

                    break;
                case 16:
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
                    break;
                default:
                    throw new Exception( "The bit depth specified, " + bitsPerPixel + ", is not supported." );
            }

            bmp = new System.Drawing.Bitmap( width, height, pixelFormat );

            if (colors != null)
            {
                System.Drawing.Imaging.ColorPalette palette = bmp.Palette;
                colors.CopyTo( palette.Entries, 0 );
                bmp.Palette = palette;
            }

            return bmp;
        }

        internal virtual void DevicePaint(int x, int y, int width, int height, IntPtr data)
        {
            //Make sure the region being redrawn is completely inside the screen
            Debug.Assert( (x >= 0) && ((x + width) <= _bitmap.Width) );
            Debug.Assert( (y >= 0) && ((y + height) <= _bitmap.Height) );

            Rectangle paintRegion = new Rectangle( x, y, width, height );

            BitmapData lockedRegion = _bitmap.LockBits( paintRegion, System.Drawing.Imaging.ImageLockMode.WriteOnly, _bitmap.PixelFormat );

            try
            {
                switch (_bitsPerPixel)
                {
                    case 16:
                        unsafe
                        {
                            int widthInShorts = lockedRegion.Stride / 2;
                            short* targetAddr = (short*)lockedRegion.Scan0.ToPointer();
                            short* sourceAddr = ((short*)data.ToPointer()) + (y * widthInShorts) + x;

                            // Copy away
                            for (int i = 0; i < height; i++)
                            {
                                for (int j = 0; j < width; j++)
                                {
                                    *(targetAddr + j) = *(sourceAddr + j);
                                }

                                targetAddr += widthInShorts;
                                sourceAddr += widthInShorts;
                            }
                        }
                        break;
                    default:
                        Debug.Assert(false, "DevicePaint() for bitdepth " + _bitsPerPixel + " is not implemented.");
                        Trace.WriteLine("Warning: DevicePaint() for bitdepth " + _bitsPerPixel + " is not supported.");
                        break;
                }
            }
            finally
            {
                _bitmap.UnlockBits( lockedRegion );
            }

            SignalPaintHandler(x, y, width, height);
        }
        
        internal virtual void DeviceClear()
        {
            Rectangle paintRegion = new Rectangle( 0, 0, _bitmap.Width, _bitmap.Height );

            BitmapData lockedRegion = _bitmap.LockBits( paintRegion, System.Drawing.Imaging.ImageLockMode.WriteOnly, _bitmap.PixelFormat );

            try
            {
                switch (_bitsPerPixel)
                {
                    case 16:
                        unsafe
                        {
                            int widthInShorts = lockedRegion.Stride / 2;
                            short* targetAddr = (short*)lockedRegion.Scan0.ToPointer();

                            // Copy away
                            for (int i = 0; i < _height; i++)
                            {
                                for (int j = 0; j < _width; j++)
                                {
                                    *(targetAddr + j) = 0;
                                }

                                targetAddr += widthInShorts;
                            }
                        }
                        break;
                    default:
                        Debug.Assert( false, "DeviceClear() for bitdepth " + _bitsPerPixel + " is not implemented." );
                        Trace.WriteLine( "Warning: DeviceClear() for bitdepth " + _bitsPerPixel + " is not supported." );
                        break;
                }
            }
            finally
            {
                _bitmap.UnlockBits( lockedRegion );
            }

            SignalPaintHandler( 0, 0, _width, _height );
        }

        private void SignalPaintHandler(int x, int y, int width, int height)
        {
            OnDevicePaintEventHandler devicePaintHandler = _devicePaintHandler;            

            if (devicePaintHandler != null && _bitmap != null)
            {
                OnDevicePaintEventArgs args = new OnDevicePaintEventArgs(new Rectangle(x, y, width, height), _bitmap);
                devicePaintHandler(this, args);
            }
        }
        
        public int Width
        {
            get { return _width; }
            set
            {
                ThrowIfNotConfigurable();
                _width = value;
            }
        }

        public int Height
        {
            get { return _height; }
            set
            {
                ThrowIfNotConfigurable();
                _height = value;
            }
        }

        public int BitsPerPixel
        {
            get { return _bitsPerPixel; }
            set
            {
                ThrowIfNotConfigurable();
                _bitsPerPixel = value;
            }
        }

        internal int DeviceWidth
        {
            get { return _width; }
        }

        internal int DeviceHeight
        {
            get { return _height; }
        }

        internal int DeviceBitsPerPixel
        {
            get { return _bitsPerPixel; }
        }
    }

    internal sealed class LcdDisplayNull : LcdDisplay
    {
        public LcdDisplayNull()
            : base( 0, 0, 16 )
        {
        }

        public override void SetupComponent()
        {            
        }

        internal override void DevicePaint( int x, int y, int width, int height, IntPtr data )
        {
        }

        internal override void DeviceClear()
        {
        }

        private bool _displayWarning = true;

        internal bool DisplayWarning
        {
            get { return _displayWarning; }
        }

        internal void TurnOffWarning()
        {
            _displayWarning = false;
        }
    }
}
