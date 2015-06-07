using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Spice
{
    public partial class PropertyEditor : Form
    {
        public PropertyEditor()
        {
            InitializeComponent();
        }

        public PropertyEditor(CircuitElm elm)
        {
            InitializeComponent();

            if (elm.type != 'w' && elm.type != 'g')
            {
                TextBox r = new TextBox();
                r.SetBounds(20, 15, 110, 50);
                r.Text = elm.characteristic.ToString();
                Controls.Add(r);

                r.TextChanged += delegate
                { if(r.Text!=String.Empty) elm.characteristic = Convert.ToInt32(r.Text); };
            }
        }
    }
}
