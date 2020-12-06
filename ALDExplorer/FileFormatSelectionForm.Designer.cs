namespace ALDExplorer
{
    partial class FileFormatSelectionForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.aldRadioButton = new System.Windows.Forms.RadioButton();
            this.datRadioButton = new System.Windows.Forms.RadioButton();
            this.alkRadioButton = new System.Windows.Forms.RadioButton();
            this.afa1RadioButton = new System.Windows.Forms.RadioButton();
            this.afa2RadioButton = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // aldRadioButton
            // 
            this.aldRadioButton.AutoSize = true;
            this.aldRadioButton.Checked = true;
            this.aldRadioButton.Location = new System.Drawing.Point(12, 12);
            this.aldRadioButton.Name = "aldRadioButton";
            this.aldRadioButton.Size = new System.Drawing.Size(65, 17);
            this.aldRadioButton.TabIndex = 0;
            this.aldRadioButton.TabStop = true;
            this.aldRadioButton.Text = "ALD File";
            this.aldRadioButton.UseVisualStyleBackColor = true;
            // 
            // datRadioButton
            // 
            this.datRadioButton.AutoSize = true;
            this.datRadioButton.Location = new System.Drawing.Point(12, 35);
            this.datRadioButton.Name = "datRadioButton";
            this.datRadioButton.Size = new System.Drawing.Size(118, 17);
            this.datRadioButton.TabIndex = 1;
            this.datRadioButton.Text = "System 3.0 DAT file";
            this.datRadioButton.UseVisualStyleBackColor = true;
            // 
            // alkRadioButton
            // 
            this.alkRadioButton.AutoSize = true;
            this.alkRadioButton.Location = new System.Drawing.Point(12, 58);
            this.alkRadioButton.Name = "alkRadioButton";
            this.alkRadioButton.Size = new System.Drawing.Size(64, 17);
            this.alkRadioButton.TabIndex = 2;
            this.alkRadioButton.Text = "ALK File";
            this.alkRadioButton.UseVisualStyleBackColor = true;
            // 
            // afa1RadioButton
            // 
            this.afa1RadioButton.AutoSize = true;
            this.afa1RadioButton.Location = new System.Drawing.Point(12, 81);
            this.afa1RadioButton.Name = "afa1RadioButton";
            this.afa1RadioButton.Size = new System.Drawing.Size(116, 17);
            this.afa1RadioButton.TabIndex = 3;
            this.afa1RadioButton.Text = "AFA File (version 1)";
            this.afa1RadioButton.UseVisualStyleBackColor = true;
            // 
            // afa2RadioButton
            // 
            this.afa2RadioButton.AutoSize = true;
            this.afa2RadioButton.Location = new System.Drawing.Point(12, 104);
            this.afa2RadioButton.Name = "afa2RadioButton";
            this.afa2RadioButton.Size = new System.Drawing.Size(116, 17);
            this.afa2RadioButton.TabIndex = 4;
            this.afa2RadioButton.Text = "AFA File (version 2)";
            this.afa2RadioButton.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(12, 136);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(93, 136);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // FileFormatSelectionForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(180, 169);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.afa2RadioButton);
            this.Controls.Add(this.afa1RadioButton);
            this.Controls.Add(this.alkRadioButton);
            this.Controls.Add(this.datRadioButton);
            this.Controls.Add(this.aldRadioButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FileFormatSelectionForm";
            this.Text = "Select File Format:";
            this.Load += new System.EventHandler(this.FileFormatSelectionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton aldRadioButton;
        private System.Windows.Forms.RadioButton datRadioButton;
        private System.Windows.Forms.RadioButton alkRadioButton;
        private System.Windows.Forms.RadioButton afa1RadioButton;
        private System.Windows.Forms.RadioButton afa2RadioButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}