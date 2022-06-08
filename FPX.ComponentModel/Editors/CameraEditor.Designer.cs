namespace FPX.Editor
{
    partial class CameraEditor
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
            this.label1 = new System.Windows.Forms.Label();
            this.fieldOfViewTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nearPlaneTextBox = new System.Windows.Forms.TextBox();
            this.farPlaneLabel = new System.Windows.Forms.Label();
            this.farPlaneTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.colorPicker1 = new FPX.ColorPicker();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Field of View:";
            // 
            // fieldOfViewTextBox
            // 
            this.fieldOfViewTextBox.Location = new System.Drawing.Point(140, 17);
            this.fieldOfViewTextBox.Name = "fieldOfViewTextBox";
            this.fieldOfViewTextBox.Size = new System.Drawing.Size(100, 20);
            this.fieldOfViewTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(5, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Near Plane:";
            // 
            // nearPlaneTextBox
            // 
            this.nearPlaneTextBox.Location = new System.Drawing.Point(140, 43);
            this.nearPlaneTextBox.Name = "nearPlaneTextBox";
            this.nearPlaneTextBox.Size = new System.Drawing.Size(100, 20);
            this.nearPlaneTextBox.TabIndex = 3;
            // 
            // farPlaneLabel
            // 
            this.farPlaneLabel.AutoSize = true;
            this.farPlaneLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.farPlaneLabel.Location = new System.Drawing.Point(5, 63);
            this.farPlaneLabel.Name = "farPlaneLabel";
            this.farPlaneLabel.Size = new System.Drawing.Size(102, 25);
            this.farPlaneLabel.TabIndex = 4;
            this.farPlaneLabel.Text = "Far Plane:";
            // 
            // farPlaneTextBox
            // 
            this.farPlaneTextBox.Location = new System.Drawing.Point(140, 69);
            this.farPlaneTextBox.Name = "farPlaneTextBox";
            this.farPlaneTextBox.Size = new System.Drawing.Size(100, 20);
            this.farPlaneTextBox.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(5, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 25);
            this.label3.TabIndex = 6;
            this.label3.Text = "Clear Color:";
            // 
            // colorPicker1
            // 
            this.colorPicker1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.colorPicker1.Location = new System.Drawing.Point(140, 95);
            this.colorPicker1.Name = "colorPicker1";
            this.colorPicker1.Size = new System.Drawing.Size(100, 18);
            this.colorPicker1.TabIndex = 7;
            this.colorPicker1.XnaColor = new Microsoft.Xna.Framework.Color(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            // 
            // CameraEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.colorPicker1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.farPlaneTextBox);
            this.Controls.Add(this.farPlaneLabel);
            this.Controls.Add(this.nearPlaneTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.fieldOfViewTextBox);
            this.Controls.Add(this.label1);
            this.Name = "CameraEditor";
            this.Size = new System.Drawing.Size(498, 150);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox fieldOfViewTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox nearPlaneTextBox;
        private System.Windows.Forms.Label farPlaneLabel;
        private System.Windows.Forms.TextBox farPlaneTextBox;
        private System.Windows.Forms.Label label3;
        private ColorPicker colorPicker1;
    }
}
