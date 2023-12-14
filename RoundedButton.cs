using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace Trivial
{
    class RoundedButton : Button
    {
        public int CornerRadius { get; set; } = 10;
        public Color BorderColor { get; set; } = Color.Black;
        public int BorderSize { get; set; } = 1;

        public Thread AnimacionFallo { get; set; }
        public Thread AnimacionAcierto { get; set; }

        private Color colorHover;


        public RoundedButton()
        {
            AnimacionFallo = new Thread(AnimarFallo);
            AnimacionAcierto = new Thread(AnimarAcierto);
            colorHover = SystemColors.ButtonHighlight;
        }

      
        
        public void AnimarFallo()
        {
            FormMain.SetAnimation();
            Color colorOriginal = BackColor;
            BackColor = Color.Red;
            Thread.Sleep(800);
            BackColor = colorOriginal;
            AnimacionFallo = new Thread(AnimarFallo);
            Invalidate();
            FormMain.SetAnimation();

        }

        public void AnimarAcierto()
        {
            FormMain.SetAnimation();
            Color colorOriginal = BackColor;
            BackColor = Color.Green;            
            Thread.Sleep(800);
            BackColor = colorOriginal;
            AnimacionAcierto = new Thread(AnimarAcierto);            
            Invalidate();
            FormMain.SetAnimation();
        }
        


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, CornerRadius * 2, CornerRadius * 2, 180, 90);
                path.AddArc(Width - CornerRadius * 2, 0, CornerRadius * 2, CornerRadius * 2, 270, 90);
                path.AddArc(Width - CornerRadius * 2, Height - CornerRadius * 2, CornerRadius * 2, CornerRadius * 2, 0, 90);
                path.AddArc(0, Height - CornerRadius * 2, CornerRadius * 2, CornerRadius * 2, 90, 90);
                
                path.CloseFigure();

                this.Region = new Region(path);

                using (Pen pen = new Pen(BorderColor, BorderSize))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }


    }
}