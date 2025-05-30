using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace GDIVirus.Effects
{
    public class Invert
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
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

        public enum TernaryRasterOperations : uint
        {
            PATINVERT = 0x005A0049
        }

        private uint[] colourMap;
        private int in_x;
        private int in_y;
        private int in_width;
        private int in_height;
        private int speed;

        private bool inverting = false;
        private Thread invertThread;

        public Invert(uint[] colors, int x, int y, int width, int height, int interval_speed)
        {
            colourMap = colors;
            in_x = x;
            in_y = y;
            in_width = width;
            in_height = height;
            speed = interval_speed;
        }

        public void Start()
        {
            inverting = true;

            Random random;

            invertThread = new Thread(() =>
            {
                while (inverting)
                {
                    random = new Random();

                    IntPtr hdc = GetDC(IntPtr.Zero);
                    IntPtr Brush = CreateSolidBrush(colourMap[random.Next(colourMap.Length)]);
                    SelectObject(hdc, Brush);
                    PatBlt(hdc, in_x, in_y, in_width, in_height, TernaryRasterOperations.PATINVERT);
                    DeleteObject(Brush);
                    DeleteDC(hdc);
                    Thread.Sleep(speed);
                }
            });

            invertThread.Start();
        }

        public void Stop()
        {
            inverting = false;
            invertThread.Join();
        }

        public static void PlainInvert()
        {
            Rectangle screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;

            IntPtr hdc = GetDC(IntPtr.Zero);
            IntPtr mdc = CreateCompatibleDC(hdc);
            IntPtr bmp = CreateCompatibleBitmap(hdc, screen.Width, screen.Height);
            IntPtr oldbmp = SelectObject(mdc, bmp);
            BitBlt(mdc, 0, 0, screen.Width, screen.Height, hdc, 0, 0, CopyPixelOperation.SourceCopy);
            BitBlt(mdc, 0, 0, screen.Width, screen.Height, IntPtr.Zero, 0, 0, CopyPixelOperation.DestinationInvert);
            BitBlt(hdc, 0, 0, screen.Width, screen.Height, mdc, 0, 0, CopyPixelOperation.SourceCopy);

            SelectObject(mdc, oldbmp);
            DeleteObject(bmp);
            DeleteDC(mdc);
            ReleaseDC(IntPtr.Zero, hdc);
        }
    }
}
