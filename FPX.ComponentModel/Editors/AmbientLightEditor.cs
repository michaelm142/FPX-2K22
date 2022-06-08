using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FPX.Visual;

namespace FPX.Editor
{
    public partial class LightEditor : ComponentEditor
    {
        public LightEditor(Light Target)
        {
            InitializeComponent();
            this.Target = Target;
            colorPicker1.XnaColor = Target.DiffuseColor;
            colorPicker1.ColorChanged += ColorPicker1_ColorChanged;
        }

        private void ColorPicker1_ColorChanged(object sender, EventArgs e)
        {
            (Target as Light).DiffuseColor = colorPicker1.XnaColor;
        }
    }
}
