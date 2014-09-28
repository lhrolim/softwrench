namespace softWrench.sW4.Controls.WebForms
{
    partial class RichTextBox
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
            this.rTxtBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rTxtBox
            // 
            this.rTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rTxtBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rTxtBox.Location = new System.Drawing.Point(0, 0);
            this.rTxtBox.Name = "rTxtBox";
            this.rTxtBox.Size = new System.Drawing.Size(800, 450);
            this.rTxtBox.TabIndex = 0;
            this.rTxtBox.Text = "";
            this.rTxtBox.WordWrap = false;
            this.rTxtBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.rTxtBox_MouseDown);
            // 
            // RichTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.rTxtBox);
            this.Name = "RichTextBox";
            this.Size = new System.Drawing.Size(800, 450);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rTxtBox;
    }
}
