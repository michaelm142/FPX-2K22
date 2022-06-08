namespace FPX.Editor
{
    partial class TransformEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.positionXLabel = new System.Windows.Forms.Label();
            this.positionYLabel = new System.Windows.Forms.Label();
            this.positionZLabel = new System.Windows.Forms.Label();
            this.positionGroupBox = new System.Windows.Forms.GroupBox();
            this.positionXBox = new System.Windows.Forms.TextBox();
            this.positionYBox = new System.Windows.Forms.TextBox();
            this.positionZBox = new System.Windows.Forms.TextBox();
            this.positionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // positionXLabel
            // 
            this.positionXLabel.AutoSize = true;
            this.positionXLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.positionXLabel.Location = new System.Drawing.Point(6, 16);
            this.positionXLabel.Name = "positionXLabel";
            this.positionXLabel.Size = new System.Drawing.Size(32, 25);
            this.positionXLabel.TabIndex = 0;
            this.positionXLabel.Text = "X:";
            // 
            // positionYLabel
            // 
            this.positionYLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.positionYLabel.AutoSize = true;
            this.positionYLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.positionYLabel.Location = new System.Drawing.Point(150, 16);
            this.positionYLabel.Name = "positionYLabel";
            this.positionYLabel.Size = new System.Drawing.Size(31, 25);
            this.positionYLabel.TabIndex = 1;
            this.positionYLabel.Text = "Y:";
            // 
            // positionZLabel
            // 
            this.positionZLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.positionZLabel.AutoSize = true;
            this.positionZLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.positionZLabel.Location = new System.Drawing.Point(293, 16);
            this.positionZLabel.Name = "positionZLabel";
            this.positionZLabel.Size = new System.Drawing.Size(30, 25);
            this.positionZLabel.TabIndex = 2;
            this.positionZLabel.Text = "Z:";
            // 
            // positionGroupBox
            // 
            this.positionGroupBox.Controls.Add(this.positionZBox);
            this.positionGroupBox.Controls.Add(this.positionYBox);
            this.positionGroupBox.Controls.Add(this.positionXBox);
            this.positionGroupBox.Controls.Add(this.positionXLabel);
            this.positionGroupBox.Controls.Add(this.positionZLabel);
            this.positionGroupBox.Controls.Add(this.positionYLabel);
            this.positionGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.positionGroupBox.Location = new System.Drawing.Point(3, 4);
            this.positionGroupBox.Name = "positionGroupBox";
            this.positionGroupBox.Size = new System.Drawing.Size(454, 55);
            this.positionGroupBox.TabIndex = 3;
            this.positionGroupBox.TabStop = false;
            this.positionGroupBox.Text = "Position";
            // 
            // positionXBox
            // 
            this.positionXBox.Location = new System.Drawing.Point(44, 19);
            this.positionXBox.Name = "positionXBox";
            this.positionXBox.Size = new System.Drawing.Size(100, 20);
            this.positionXBox.TabIndex = 3;
            // 
            // positionYBox
            // 
            this.positionYBox.Location = new System.Drawing.Point(187, 19);
            this.positionYBox.Name = "positionYBox";
            this.positionYBox.Size = new System.Drawing.Size(100, 20);
            this.positionYBox.TabIndex = 4;
            // 
            // positionZBox
            // 
            this.positionZBox.Location = new System.Drawing.Point(329, 19);
            this.positionZBox.Name = "positionZBox";
            this.positionZBox.Size = new System.Drawing.Size(100, 20);
            this.positionZBox.TabIndex = 5;
            // 
            // TransformEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.positionGroupBox);
            this.Name = "TransformEditor";
            this.Size = new System.Drawing.Size(480, 65);
            this.positionGroupBox.ResumeLayout(false);
            this.positionGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label positionXLabel;
        private System.Windows.Forms.Label positionYLabel;
        private System.Windows.Forms.Label positionZLabel;
        private System.Windows.Forms.GroupBox positionGroupBox;
        private System.Windows.Forms.TextBox positionZBox;
        private System.Windows.Forms.TextBox positionYBox;
        private System.Windows.Forms.TextBox positionXBox;
    }
}
