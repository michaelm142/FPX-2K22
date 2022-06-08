using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Xna.Framework;

using Color = System.Drawing.Color;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace FPX
{
    public class ColorPicker : Panel
    {
        public event EventHandler<EventArgs> ColorChanged;

        public Color SysColor { get; private set; } = Color.White;

        public XnaColor XnaColor
        {
            get { return new XnaColor(SysColor.R, SysColor.G, SysColor.B, SysColor.A); }
            set { SysColor = Color.FromArgb(value.A, value.R, value.G, value.B); ColorChanged(SysColor, new EventArgs()); }
        }

        public ColorPicker()
        {
            Click += ColorPicker_Click;
            BorderStyle = BorderStyle.FixedSingle;
            ColorChanged += ColorPicker_ColorChanged;
        }

        private void ColorPicker_ColorChanged(object sender, EventArgs e)
        {
            BackColor = SysColor;
        }

        private void ColorPicker_Click(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            dialog.Color = SysColor;
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                SysColor = dialog.Color;
                BackColor = SysColor;
                ColorChanged(this, new EventArgs());
            }
        }
    }
}
