using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Platform.Test
{
    internal class ScreenCapture
    {
        [DllImport("GDI32.dll")]
        public static extern bool BitBlt(int hdest, int nXDest, int nYDest, int nWidth, int nHeight, int hsource, int nXSrc, int nYSrc, int dwRop);
        
        [DllImport("GDI32.dll")]
        public static extern int CreateCompatibleBitmap(int hdc, int nWidth, int nHeight);
        
        [DllImport("GDI32.dll")]
        public static extern int CreateCompatibleDC(int hdc);
        
        [DllImport("GDI32.dll")]
        public static extern bool DeleteDC(int hdc);
        
        [DllImport("GDI32.dll")]
        public static extern bool DeleteObject(int hObject);
        
        [DllImport("GDI32.dll")]
        public static extern int GetDeviceCaps(int hdc, int nIndex);
        
        [DllImport("GDI32.dll")]
        public static extern int SelectObject(int hdc, int hgdiobj);
        
        [DllImport("User32.dll")]
        public static extern int GetDesktopWindow();
        
        [DllImport("User32.dll")]
        public static extern int GetWindowDC(int hWnd);
        
        [DllImport("User32.dll")]
        public static extern int ReleaseDC(int hWnd, int hDC);

        internal void CaptureScreen(string fileName, ImageFormat imageFormat)
        {
            int source = ScreenCapture.GetWindowDC(ScreenCapture.GetDesktopWindow());
            int dest = ScreenCapture.CreateCompatibleDC(source);
            int bitmap = ScreenCapture.CreateCompatibleBitmap(source, ScreenCapture.GetDeviceCaps(source, 8), ScreenCapture.GetDeviceCaps(source, 10)); 
            ScreenCapture.SelectObject(dest, bitmap);
            ScreenCapture.BitBlt(dest, 0, 0, ScreenCapture.GetDeviceCaps(source, 8),
                ScreenCapture.GetDeviceCaps(source, 10), source, 0, 0, 0x00CC0020);
            SaveImageAs(bitmap, fileName, imageFormat);
            Cleanup(bitmap, source, dest);
        }

        private void Cleanup(int bitmap, int source, int dest)
        {
            ScreenCapture.ReleaseDC(ScreenCapture.GetDesktopWindow(), source);
            ScreenCapture.DeleteDC(dest);
            ScreenCapture.DeleteObject(bitmap);
        }

        private void SaveImageAs(int bitmap, string fileName, ImageFormat imageFormat)
        {
            Bitmap image = new Bitmap(Image.FromHbitmap(new IntPtr(bitmap)),
                Image.FromHbitmap(new IntPtr(bitmap)).Width,
                Image.FromHbitmap(new IntPtr(bitmap)).Height);
            image.Save(fileName, imageFormat);
        }
    }
}
