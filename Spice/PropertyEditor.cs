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
            if (elm.type == 'r')
            {
                TextBox r = new TextBox();
                r.Text = elm.characteristic.ToString();
                Controls.Add(r);

                r.TextChanged += delegate
                { elm.characteristic = Convert.ToInt32(r.Text); };
            }
        }
    }
}
