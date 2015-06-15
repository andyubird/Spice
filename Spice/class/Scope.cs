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

            if (type == 0)
            {
                if (elm.voltdiff > max) max = elm.voltdiff + 1e-8;
                if (elm.voltdiff < min) min = elm.voltdiff;

                data.Insert(0, elm.voltdiff);
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

            for (int i = 0; i < area.Width - 1; i++)
            {
                screen.DrawLine(new Pen(Color.Aqua),
                    area.X + i,
                    area.Y + area.Height - (int)(area.Height * (data[i] - min) / (max - min)),
                    area.X + i + 1,
                    area.Y + area.Height - (int)(area.Height * (data[i + 1] - min) / (max - min)));
            }

            screen.DrawString(max.ToString(), new Font("Arial", 16), new SolidBrush(Color.Aqua), area.Location);
            screen.DrawString(min.ToString(), new Font("Arial", 16), new SolidBrush(Color.Aqua), area.Location + new Size(0,area.Height));
        }
    }
}
