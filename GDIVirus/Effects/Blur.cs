﻿using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GDIVirus.Effects
{
    public class Blur
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;

            public BLENDFUNCTION(byte op, byte flags, byte alpha, byte format)
            {
                BlendOp = op;
                BlendFlags = flags;
                SourceConstantAlpha = alpha;
                AlphaFormat = format;
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateSolidBrush(uint crColor);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        static extern bool PatBlt(IntPtr hdc, int x, int y, int width, int height, TernaryRasterOperations rop);

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        [DllImport("gdi32.dll", SetLastError = true)]
        static extern bool BitBlt(
            IntPtr hdcDest,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc,
            int nXSrc,
            int nYSrc,
            TernaryRasterOperations dwRop
        );

        [DllImport("gdi32.dll", EntryPoint = "GdiAlphaBlend")]
        public static extern bool AlphaBlend(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
        int nWidthDest, int nHeightDest,
        IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
        BLENDFUNCTION blendFunction);

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

        private int in_x;
        private int in_y;
        private int in_width;
        private int in_height;
        private int speed;

        private bool blurring = false;
        private Thread blurThread;

        public Blur(int x, int y, int width, int height, int interval_speed)
        {
            in_x = x;
            in_y = y;
            in_width = width;
            in_height = height;
            speed = interval_speed;
        }

        public void Start()
        {
            blurring = true;

            Random random;

            blurThread = new Thread(() =>
            {
                while (blurring)
                {
                    random = new Random();

                    IntPtr hdc = GetDC(IntPtr.Zero);
                    IntPtr mhdc = CreateCompatibleDC(hdc);
                    IntPtr hbit = CreateCompatibleBitmap(hdc, in_width, in_height);
                    IntPtr holdbit = SelectObject(mhdc, hbit);
                    BitBlt(mhdc, in_x, in_y, in_width, in_height, hdc, 0, 0, TernaryRasterOperations.SRCCOPY);
                    AlphaBlend(hdc, random.Next(-4, 4), random.Next(-4, 4), in_width, in_height, mhdc, in_x, in_y, in_width, in_height, new BLENDFUNCTION(0, 0, 70, 0));
                    SelectObject(mhdc, holdbit);
                    DeleteObject(holdbit);
                    DeleteObject(hbit);
                    DeleteDC(mhdc);
                    DeleteDC(hdc);
                    Thread.Sleep(speed);
                }
            });

            blurThread.Start();
        }

        public void Stop()
        {
            blurring = false;
            blurThread.Join();
        }
    }
}
