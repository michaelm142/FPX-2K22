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
    public partial class MeshRendererEditor : ComponentEditor
    {
        private string startName;

        public MeshRendererEditor(MeshRenderer Target)
        {
            InitializeComponent();
            this.Target = Target;
            startName = modelNameLabel.Text; 
        }

        public override void UpdateTarget()
        {
            modelNameLabel.Text = string.Format(startName, (Target as MeshRenderer).model.Tag);
        }
    }
}
