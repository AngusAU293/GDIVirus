using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GDIVirus.Effects
{
    public class Text
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart,
            string lpString, int cbString);

        public static void Blit(int x, int y, String text)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            TextOut(hdc, x, y, text, text.Length);
            DeleteDC(hdc);
        }
    }
}
