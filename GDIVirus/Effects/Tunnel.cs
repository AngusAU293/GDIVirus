using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace GDIVirus.Effects
{
    public class Tunnel
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateSolidBrush(uint crColor);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        static extern bool PatBlt(IntPtr hdc, int x, int y, int width, int height, TernaryRasterOperations rop);

        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height,
                              IntPtr hdcSrc, int xSrc, int ySrc, CopyPixelOperation rop);

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        static extern bool PlgBlt(IntPtr hdcDest, POINT[] lpPoint, IntPtr hdcSrc,
            int nXSrc, int nYSrc, int nWidth, int nHeight, IntPtr hbmMask, int xMask,
            int yMask);

        struct FIXED
        {
            public short fract;
            public short value;
        }

        struct POINTFX
        {
            public FIXED x;
            public FIXED y;
        }

        private int in_x;
        private int in_y;
        private int in_width;
        private int in_height;
        private int in_left;
        private int in_right;
        private int in_top;
        private int in_bottom;

        private bool tunnelling = false;
        private Thread tunnelThread;

        public Tunnel(int x, int y, int width, int height, int left, int right, int top, int bottom)
        {
            in_x = x;
            in_y = y;
            in_width = width;
            in_height = height;
            in_left = left;
            in_right = right;
            in_top = top;
            in_bottom = bottom;
        }

        public void Start()
        {
            Random random;
            POINT[] lppoint = new POINT[3];

            tunnelling = true;

            tunnelThread = new Thread(() =>
            {
                while (tunnelling)
                {
                    random = new Random();

                    IntPtr hdc = GetDC(IntPtr.Zero);
                    IntPtr mhdc = CreateCompatibleDC(hdc);
                    IntPtr hbit = CreateCompatibleBitmap(hdc, in_width, in_height);
                    IntPtr holdbit = SelectObject(mhdc, hbit);
                    lppoint[0].X = (in_left + 50) + 0;
                    lppoint[0].Y = (in_top - 50) + 0;
                    lppoint[1].X = (in_right + 50) + 0;
                    lppoint[1].Y = (in_top + 50) + 0;
                    lppoint[2].X = (in_left - 50) + 0;
                    lppoint[2].Y = (in_bottom - 50) + 0;
                    PlgBlt(hdc, lppoint, hdc, in_left - 20, in_top - 20, (in_right - in_left) + 40, (in_bottom - in_top) + 40, IntPtr.Zero, 0, 0);
                    DeleteDC(hdc);
                    Thread.Sleep(50);
                }
            });

            tunnelThread.Start();
        }

        public void Stop()
        {
            tunnelling = false;
            tunnelThread.Join();
        }
    }
}
