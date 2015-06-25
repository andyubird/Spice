using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Spice
{
    class Scope
    {
        public CircuitElm elm;

        public List<double> data;

        public double min = 0, max = 5;

        public Rectangle area;

        public Scope(CircuitElm celm, Rectangle rect)
        {
            elm = celm;
            data = new List<double>(new double[rect.Width]);
            area = rect;
        }

        public void step(int type)
        {
            if (elm == null) return;
            if (elm.type == 'g') return;

            if (type == 0)
            {
                if (elm.volts[0] - elm.volts[1] > max) max = elm.volts[0] - elm.volts[1] + 1e-8;
                if (elm.volts[0] - elm.volts[1] < min) min = elm.volts[0] - elm.volts[1];

                data.Insert(0, elm.volts[0] - elm.volts[1]);
                data.RemoveAt(area.Width);

                return;
            }
            else
            {
                if (elm.current > max) max = elm.current + 1e-8;
                if (elm.current < min) min = elm.current;

                data.Insert(0, elm.current);
                data.RemoveAt(area.Width);
            }


        }

        public void draw(Graphics screen)
        {
            if (elm == null) return;
            if (elm.type == 'g') return;

            screen.DrawLine(new Pen(Color.Aqua), area.X, area.Y + area.Height, area.X + area.Width, area.Y + area.Height);
            screen.DrawLine(new Pen(Color.Aqua), area.X + area.Width, area.Y, area.X + area.Width, area.Y + area.Height);

            for (int i = 0; i < area.Width - 1; i++)
            {
                screen.DrawLine(new Pen(Color.Yellow),
                    area.X - i + area.Width,
                    area.Y + area.Height - (int)(area.Height * (data[i] - min) / (max - min)),
                    area.X - i - 1 + area.Width,
                    area.Y + area.Height - (int)(area.Height * (data[i + 1] - min) / (max - min)));
            }

            
            screen.DrawString(max.ToString("f4"), new Font("SansSerif", 10), new SolidBrush(Color.Aqua), area.Location + new Size(area.Width, 0));
            screen.DrawString(data[0].ToString("f4"), new Font("SansSerif", 10), new SolidBrush(Color.Aqua), area.Location + new Size(area.Width, area.Height / 2 - 7));
            screen.DrawString(min.ToString("f4"), new Font("SansSerif", 10), new SolidBrush(Color.Aqua), area.Location + new Size(area.Width, area.Height - 15));
        }
    }
}
