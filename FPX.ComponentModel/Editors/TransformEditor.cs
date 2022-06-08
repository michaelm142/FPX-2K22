using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FPX.ComponentModel;

namespace FPX.Editor
{
    public partial class TransformEditor : ComponentEditor
    {
        private Transform transform
        {
            get { return Target as Transform; }
        }

        public TransformEditor(Transform Target)
        {
            InitializeComponent();
            this.Target = Target;
            positionXBox.TextChanged += PositionXBox_TextChanged;
            positionYBox.TextChanged += PositionYBox_TextChanged;
            positionZBox.TextChanged += PositionZBox_TextChanged;
        }

        private void PositionZBox_TextChanged(object sender, EventArgs e)
        {
            float value = 0.0f;
            if (!float.TryParse(positionZBox.Text, out value))
                return;

            transform.localPosition.Z = value;
        }

        private void PositionYBox_TextChanged(object sender, EventArgs e)
        {
            float value = 0.0f;
            if (!float.TryParse(positionYBox.Text, out value))
                return;

            transform.localPosition.Y = value;
        }

        private void PositionXBox_TextChanged(object sender, EventArgs e)
        {
            float value = 0.0f;
            if (!float.TryParse(positionXBox.Text, out value))
                return;

            transform.localPosition.X = value;
        }

        public override void UpdateTarget()
        {
            positionXBox.Text = Target.localPosition.X.ToString();
            positionYBox.Text = Target.localPosition.Y.ToString();
            positionZBox.Text = Target.localPosition.Z.ToString();
        }
    }
}
