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
    public partial class CameraEditor : ComponentEditor
    {
        public Camera Camera
        {
            get { return Target as Camera; }
        }

        public CameraEditor(Camera target)
        {
            InitializeComponent();
            Target = target;

            fieldOfViewTextBox.Text = Camera.fieldOfView.ToString();
            nearPlaneTextBox.Text = Camera.nearPlaneDistance.ToString();
            farPlaneLabel.Text = Camera.farPlaneDistance.ToString();
            colorPicker1.XnaColor = Camera.ClearColor;

            colorPicker1.ColorChanged += ColorPicker1_ColorChanged;
        }

        private void ColorPicker1_ColorChanged(object sender, EventArgs e)
        {
            Camera.ClearColor = colorPicker1.XnaColor;
        }
    }
}
