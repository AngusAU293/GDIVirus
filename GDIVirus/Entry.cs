using GDIVirus.Audio;
using GDIVirus.Effects;
using GDIVirus.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static GDIVirus.Audio.PCM_Audio;

namespace GDIVirus
{
    internal class Entry
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
        static extern IntPtr CreateSolidBrush(uint crColor);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        static extern bool PatBlt(IntPtr hdc, int x, int y, int width, int height, TernaryRasterOperations rop);

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("user32.dll")]
        static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        enum TernaryRasterOperations : uint
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

        private static int width = Screen.PrimaryScreen.Bounds.Width, height = Screen.PrimaryScreen.Bounds.Height;
        private static int left = Screen.PrimaryScreen.Bounds.Left, right = Screen.PrimaryScreen.Bounds.Right, top = Screen.PrimaryScreen.Bounds.Top, bottom = Screen.PrimaryScreen.Bounds.Bottom;

        private class BouncingFormInstance
        {
            public BouncingForm form;
            public Thread thread;
        }

        private static List<BouncingFormInstance> bouncingFormsLaunched = new List<BouncingFormInstance>();

        private static void LaunchBouncingForm()
        {
            BouncingFormInstance instance = new BouncingFormInstance();
            Thread thread = new Thread(() =>
            {
                Random random = new Random();
                int screen_width = Screen.PrimaryScreen.Bounds.Width, screen_height = Screen.PrimaryScreen.Bounds.Height;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                BouncingForm form = new BouncingForm();
                form.TopMost = true;
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(random.Next(0, screen_width - 200), random.Next(0, screen_height - 150));
                instance.form = form;
                Application.Run(form);
            });

            instance.thread = thread;

            bouncingFormsLaunched.Add(instance);
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private static void CloseBouncingForms()
        {
            foreach (BouncingFormInstance instance in bouncingFormsLaunched)
            {
                if (instance.form.IsHandleCreated)
                {
                    instance.form.Invoke((MethodInvoker)(() => instance.form.Close()));
                }

                instance.thread.Join();
            }

            bouncingFormsLaunched.Clear();
        }

        private static void CreateBouncingWindows()
        {
            for (int i = 0; i < 10; i++)
            {
                LaunchBouncingForm();
                Thread.Sleep(50);
            }
        }

        private static Bitmap CaptureScreen()
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;
            Bitmap bmp = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            }

            return bmp;
        }


        private static void RestoreScreen(Bitmap screenshot)
        {
            IntPtr screenDC = GetDC(IntPtr.Zero);
            IntPtr memDC = CreateCompatibleDC(screenDC);
            IntPtr hBitmap = screenshot.GetHbitmap();
            IntPtr oldBitmap = SelectObject(memDC, hBitmap);

            BitBlt(screenDC, 0, 0, screenshot.Width, screenshot.Height, memDC, 0, 0, TernaryRasterOperations.SRCCOPY);

            SelectObject(memDC, oldBitmap);
            DeleteObject(hBitmap);
            DeleteDC(memDC);
        }

        private static Thread textThread;

        private static void SpawnRandomText(int amount, int interval, String text, bool async)
        {
            void SpawnText()
            {
                Random random = new Random();

                for (int i = 0; i <= amount; i++)
                {
                    Text.Blit(random.Next(width), random.Next(height), text);
                    Thread.Sleep(interval);
                }
            }

            if (async)
            {
                textThread = new Thread(SpawnText);
                textThread.Start();
            }
            else
            {
                SpawnText();
            }
        }

        public static void Main()
        {
            DialogResult pre_warning = MessageBox.Show("Although this program isn't harmful to your computer, it can still trigger medical conditions such as epilepsy due to flashing GDI effects. If you don't wish to execute this program, simpily click \"Cancel\" to exit.", "Free Robux Legit.exe", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (pre_warning == DialogResult.Cancel)
                Environment.Exit(0);

            Thread.Sleep(300);

            uint[] randcolor = { 0xFF0000, 0x00FF00, 0x0000FF, 0x00FFCD, 0xD5FF00, 0xFF9100, 0xD500FF };

            Invert invert = new Invert(randcolor, 0, 0, width, height, 100);
            Blur blur = new Blur(0, 0, width, height, 350);
            Tunnel tunnel = new Tunnel(0, 0, width, height, left, right, top, bottom);
            Melt melt = new Melt(width, height);
            Slide slide = new Slide(0, 0, width, height);
            HatchBrush hatchBrush = new HatchBrush(randcolor, 0, 0, width, height, 500);

            PCM_Audio c_pcm = new PCM_Audio();
            float[] freqs = { 50f, 90f, 80f, 56f, 102f, 150f, 130f, 80f };
            float[] freqs2 = { 200f, 150f, 200f, 150f };
            float[] freqs3 = { 50f, 150f, 70f, 150f, 10f, 200f };
            float[] freqs4 = { 200f, 50f, 70f, 150f, 190f, 220f };

            void PCM1()
            {
                c_pcm.PCM(Waves.Square, 16000, 1, freqs3, 10);
            }
            void PCM2()
            {
                c_pcm.PCM(Waves.Sine, 10000, 5, freqs4, 14);
            }

            Thread launchWindowsThread = new Thread(CreateBouncingWindows);

            Bitmap original_screen = CaptureScreen();
            launchWindowsThread.Start();
            Thread.Sleep(3500);
            CloseBouncingForms();
            launchWindowsThread.Join();
            Thread.Sleep(500);
            c_pcm.PCM(Waves.Triangle, 16000, 1, freqs, 10);
            SpawnRandomText(300, 20, "YOU HAVE A VIRUS!", true);
            c_pcm.PCM(Waves.Triangle, 16000, 1, freqs, 10);
            Thread.Sleep(1000);
            c_pcm.PCM(Waves.Triangle, 16000, 5, freqs, 10);
            melt.Start();
            c_pcm.PCM(Waves.Triangle, 16000, 6, freqs, 10);
            Thread.Sleep(5000);
            c_pcm.PCM(Waves.Triangle, 16000, 3, freqs, 10);
            blur.Start();
            Thread.Sleep(3000);
            melt.Stop();
            blur.Stop();
            tunnel.Start();
            c_pcm.PCM(Waves.Sawtooth, 16000, 10, freqs2, 10);
            Thread.Sleep(5000);
            tunnel.Stop();
            RestoreScreen(original_screen);
            original_screen = CaptureScreen();
            Thread pcm1 = new Thread(new ThreadStart(PCM1));
            pcm1.Start();
            Thread pcm2 = new Thread(new ThreadStart(PCM2));
            pcm2.Start();
            slide.Start(5, 5, 10);
            SpawnRandomText(400, 1, "LOL LOL LOL", true);
            invert.Start();
            Thread.Sleep(6000);
            invert.Stop();
            slide.Stop();
            hatchBrush.Start();
            pcm1.Join();
            pcm2.Join();
            c_pcm.PCM(Waves.Square, 18000, 5, freqs3, 16);
            slide.Start(-5, -5, 80);
            Thread.Sleep(8000);
            hatchBrush.Stop();
            slide.Stop();
            RestoreScreen(original_screen);
            original_screen = CaptureScreen();
            c_pcm.PCM(Waves.Whitenoise, 16000, 1, freqs4, 10);
            invert.Start();
            blur.Start();
            Thread.Sleep(8000);
            invert.Stop();
            blur.Stop();
            RestoreScreen(original_screen);
        }
    }
}
