using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GDIVirus.Effects
{
    public class HatchBrush
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateHatchBrush(int iHatch, uint Color);

        [DllImport("gdi32.dll")]
        static extern uint SetBkColor(IntPtr hdc, uint crColor);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern bool PatBlt(IntPtr hdc, int nXLeft, int nYLeft, int nWidth, int nHeight, TernaryRasterOperations dwRop);

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

        private uint[] colourMap;
        private int in_x;
        private int in_y;
        private int in_width;
        private int in_height;
        private int speed;

        private bool brushing;
        private Thread brushThread;

        public HatchBrush(uint[] colours, int x, int y, int width, int height, int interval_speed)
        {
            colourMap = colours;
            in_x = x;
            in_y = y;
            in_width = width;
            in_height = height;
            speed = interval_speed;
        }

        public void Start()
        {
            brushing = true;

            Random random;

            brushThread = new Thread(() =>
            {
                while (brushing)
                {
                    random = new Random();

                    IntPtr hdc = GetDC(IntPtr.Zero);
                    IntPtr brush = CreateHatchBrush(random.Next(4), colourMap[random.Next(colourMap.Length)]);
                    SetBkColor(hdc, colourMap[random.Next(colourMap.Length)]);
                    SelectObject(hdc, brush);
                    PatBlt(hdc, in_x, in_y, in_width, in_height, TernaryRasterOperations.PATINVERT);
                    DeleteObject(brush);
                    DeleteDC(hdc);
                    Thread.Sleep(speed);
                }
            });

            brushThread.Start();
        }

        public void Stop()
        {
            brushing = false;
            brushThread.Join();
        }
    }
}
