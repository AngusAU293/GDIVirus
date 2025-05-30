using GDIVirus.Effects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GDIVirus.Forms
{
    public class BouncingForm : Form
    {
        private Timer bounceTimer;
        private int dx = 10;
        private int dy = 10;

        public BouncingForm()
        {
            this.Text = "GDI Virus";
            this.Size = new Size(400, 200);

            FontFamily arial = new FontFamily("Arial");

            Label text = new Label();
            text.Text = "You have a virus LOL\nNo Robux for you LOL\nYou have a virus LOL\nNo Robux for you LOL";
            text.Font = new Font(arial, 24, FontStyle.Regular);
            text.AutoSize = true;
            text.TextAlign = ContentAlignment.TopLeft;
            text.ForeColor = System.Drawing.Color.Red;
            Controls.Add(text);

            bounceTimer = new Timer();
            bounceTimer.Interval = 20;
            bounceTimer.Tick += BounceTimer_Tick;
            bounceTimer.Start();
        }

        private void BounceTimer_Tick(object sender, EventArgs e)
        {
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

            this.Left += dx;
            this.Top += dy;

            if (this.Right >= screenBounds.Right || this.Left <= screenBounds.Left)
            {
                dx = -dx;
            }

            if (this.Bottom >= screenBounds.Bottom || this.Top <= screenBounds.Top)
            {
                dy = -dy;
            }
        }
    }
}
