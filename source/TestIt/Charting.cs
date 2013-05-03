using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestIt
{
    public class Charting
    {
        public void ShowActiveRequests()
        {
            Application.EnableVisualStyles();
            var p = new Plot(1000);
            p.MinimizeBox = false;
            Application.Run(p);
        }
    }


    [System.ComponentModel.DesignerCategory("")]
    public class Plot : Form
    {
        Timer timer;
        public Plot(int milliseconds)
        {
            timer = new Timer();
            timer.Interval = milliseconds;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }


        List<Point> previous_points = new List<Point>();
        int counter = 0;
        double global_rescaling_factor = 1;
        int previous_width, previous_height;

        protected override void OnPaint(PaintEventArgs e)
        {
            var dx0 = (int)(e.ClipRectangle.Width / 10.0);
            var dy0 = (int)(e.ClipRectangle.Height / 10.0);


            if (previous_width != e.ClipRectangle.Width || previous_height != e.ClipRectangle.Height)
            {
                previous_points = new List<Point>();
                global_rescaling_factor = 1;
            }

            previous_height = e.ClipRectangle.Height;
            previous_width = e.ClipRectangle.Width;

            // draw border
            e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle);

            // draw X axis
            e.Graphics.DrawLine(new Pen(Color.Black), dx0, e.ClipRectangle.Height - dy0, e.ClipRectangle.Width - dx0, e.ClipRectangle.Height - dy0);

            // draw Y axis
            e.Graphics.DrawLine(new Pen(Color.Black), dx0, dy0, dx0, e.ClipRectangle.Height - dy0);

            int step = 1;

            e.Graphics.DrawString("active requests", new Font("Arial", 10), Brushes.Black, new PointF(dx0, dy0 - 20));
            e.Graphics.DrawString("sec", new Font("Arial", 10), Brushes.Black, new PointF(e.ClipRectangle.Width - dx0 + 5, e.ClipRectangle.Height - dy0));
            e.Graphics.DrawString("0", new Font("Arial", 10), Brushes.Black, new PointF(dx0 - 20, e.ClipRectangle.Height - dy0));
            e.Graphics.DrawString("0", new Font("Arial", 10), Brushes.Black, new PointF(dx0 - 20, e.ClipRectangle.Height - dy0));

            int half_number = ((int)((e.ClipRectangle.Height - 2 * dy0) / (2 * global_rescaling_factor)));
            e.Graphics.DrawString(half_number.ToString(), new Font("Arial", 10), Brushes.Black, new PointF(dx0 - half_number.ToString().Length * 10, ((e.ClipRectangle.Height - 2 * dy0) / 2) + dy0));
            e.Graphics.DrawLine(new Pen(Color.LightGray), dx0, ((e.ClipRectangle.Height - 2 * dy0) / 2) + dy0, e.ClipRectangle.Width - dx0, ((e.ClipRectangle.Height - 2 * dy0) / 2) + dy0);
            int full_number = ((int)((e.ClipRectangle.Height - 2 * dy0) / global_rescaling_factor));
            e.Graphics.DrawString(full_number.ToString(), new Font("Arial", 10), Brushes.Black, new PointF(dx0 - full_number.ToString().Length * 10, dy0));
            e.Graphics.DrawString("0", new Font("Arial", 10), Brushes.Black, new PointF(dx0 - 20, e.ClipRectangle.Height - dy0));

            e.Graphics.DrawString(((e.ClipRectangle.Width - 2 * dx0) / (2 * step)).ToString(), new Font("Arial", 10), Brushes.Black, new PointF(dx0 + ((e.ClipRectangle.Width - 2 * dx0) / 2), e.ClipRectangle.Height - dy0 + 10));

            if (Engine.Done)
            {
                timer.Stop();
            }
            counter = Engine.counter;

            int shift_by = Math.Max(previous_points.Count - (e.ClipRectangle.Width - 2 * dx0) / step, 0);
            int c = shift_by;
            while (c > 0 && previous_points.Count > 0)
            {
                previous_points.Remove(previous_points[0]);
                c--;
            }
            for (int i = 0; i < previous_points.Count; i++)
            {
                previous_points[i] = new Point(previous_points[i].X - shift_by, previous_points[i].Y);
            }

            int height = e.ClipRectangle.Height - dy0;
            Point p = new Point(previous_points.Count > 0 ? previous_points[previous_points.Count - 1].X + step : dx0, TranslateY((int)(counter * global_rescaling_factor), height));
            previous_points.Add(p);

            if (previous_points.Count > 1 && TranslateY(previous_points.Min(f => f.Y), height) > height - dy0 && TranslateY(previous_points.Min(f => f.Y), height) != 0)
            {
                double rescaling_factor = ((height - dy0) * 0.8) / (double)TranslateY(previous_points.Min(f => f.Y), height);
                global_rescaling_factor *= rescaling_factor;
                for (int i = 0; i < previous_points.Count; i++)
                    previous_points[i] = new Point(previous_points[i].X, TranslateY((int)(TranslateY(previous_points[i].Y, height) * rescaling_factor), height));
            }

            if (previous_points.Count > 1 && TranslateY(previous_points.Min(f => f.Y), height) < 0.33 * (height - dy0) && TranslateY(previous_points.Min(f => f.Y), height) != 0)
            {
                double rescaling_factor = ((height - dy0) * 0.8) / (double)TranslateY(previous_points.Min(f => f.Y), height);
                global_rescaling_factor *= rescaling_factor;
                for (int i = 0; i < previous_points.Count; i++)
                    previous_points[i] = new Point(previous_points[i].X, TranslateY((int)(TranslateY(previous_points[i].Y, height) * rescaling_factor), height));
            }

            if (previous_points.Count > 1)
                e.Graphics.DrawLines(new Pen(Color.Red), (previous_points).ToArray());

            base.OnPaint(e);
        }

        public int TranslateY(int Y, int height) //so that (0,0) is where it is on the chart
        {
            return height - Y;
        }

    }
}
