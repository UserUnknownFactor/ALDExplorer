namespace ALDExplorer
{
    partial class PropertiesForm
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
            this.lblFileName = new System.Windows.Forms.Label();
            this.FileNameTextBox = new System.Windows.Forms.TextBox();
            this.lblFileNumber = new System.Windows.Forms.Label();
            this.FileNumberTextBox = new System.Windows.Forms.TextBox();
            this.ApplyButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.OkButton = new System.Windows.Forms.Button();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.fileSizeLabel = new System.Windows.Forms.Label();
            this.fileAddressLabel = new System.Windows.Forms.Label();
            this.lblFileAddress = new System.Windows.Forms.Label();
            this.lblFileLetter = new System.Windows.Forms.Label();
            this.fileLetterTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(26, 9);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(52, 13);
            this.lblFileName.TabIndex = 0;
            this.lblFileName.Text = "&Filename:";
            // 
            // FileNameTextBox
            // 
            this.FileNameTextBox.Location = new System.Drawing.Point(84, 6);
            this.FileNameTextBox.MaxLength = 256;
            this.FileNameTextBox.Name = "FileNameTextBox";
            this.FileNameTextBox.Size = new System.Drawing.Size(268, 20);
            this.FileNameTextBox.TabIndex = 1;
            // 
            // lblFileNumber
            // 
            this.lblFileNumber.AutoSize = true;
            this.lblFileNumber.Location = new System.Drawing.Point(12, 32);
            this.lblFileNumber.Name = "lblFileNumber";
            this.lblFileNumber.Size = new System.Drawing.Size(66, 13);
            this.lblFileNumber.TabIndex = 2;
            this.lblFileNumber.Text = "File &Number:";
            // 
            // FileNumberTextBox
            // 
            this.FileNumberTextBox.Location = new System.Drawing.Point(84, 29);
            this.FileNumberTextBox.MaxLength = 5;
            this.FileNumberTextBox.Name = "FileNumberTextBox";
            this.FileNumberTextBox.Size = new System.Drawing.Size(66, 20);
            this.FileNumberTextBox.TabIndex = 3;
            // 
            // ApplyButton
            // 
            this.ApplyButton.Location = new System.Drawing.Point(277, 108);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(75, 23);
            this.ApplyButton.TabIndex = 12;
            this.ApplyButton.Text = "&Apply";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(196, 108);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 11;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // OkButton
            // 
            this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkButton.Location = new System.Drawing.Point(115, 108);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 10;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // lblFileSize
            // 
            this.lblFileSize.AutoSize = true;
            this.lblFileSize.Location = new System.Drawing.Point(48, 56);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(30, 13);
            this.lblFileSize.TabIndex = 6;
            this.lblFileSize.Text = "Size:";
            // 
            // fileSizeLabel
            // 
            this.fileSizeLabel.AutoSize = true;
            this.fileSizeLabel.Location = new System.Drawing.Point(81, 56);
            this.fileSizeLabel.Name = "fileSizeLabel";
            this.fileSizeLabel.Size = new System.Drawing.Size(40, 13);
            this.fileSizeLabel.TabIndex = 7;
            this.fileSizeLabel.Text = "fileSize";
            // 
            // fileAddressLabel
            // 
            this.fileAddressLabel.AutoSize = true;
            this.fileAddressLabel.Location = new System.Drawing.Point(81, 77);
            this.fileAddressLabel.Name = "fileAddressLabel";
            this.fileAddressLabel.Size = new System.Drawing.Size(58, 13);
            this.fileAddressLabel.TabIndex = 9;
            this.fileAddressLabel.Text = "fileAddress";
            // 
            // lblFileAddress
            // 
            this.lblFileAddress.AutoSize = true;
            this.lblFileAddress.Location = new System.Drawing.Point(30, 77);
            this.lblFileAddress.Name = "lblFileAddress";
            this.lblFileAddress.Size = new System.Drawing.Size(48, 13);
            this.lblFileAddress.TabIndex = 8;
            this.lblFileAddress.Text = "Address:";
            // 
            // lblFileLetter
            // 
            this.lblFileLetter.AutoSize = true;
            this.lblFileLetter.Location = new System.Drawing.Point(154, 32);
            this.lblFileLetter.Name = "lblFileLetter";
            this.lblFileLetter.Size = new System.Drawing.Size(56, 13);
            this.lblFileLetter.TabIndex = 4;
            this.lblFileLetter.Text = "File &Letter:";
            // 
            // fileLetterTextBox
            // 
            this.fileLetterTextBox.Location = new System.Drawing.Point(213, 29);
            this.fileLetterTextBox.MaxLength = 1;
            this.fileLetterTextBox.Name = "fileLetterTextBox";
            this.fileLetterTextBox.Size = new System.Drawing.Size(37, 20);
            this.fileLetterTextBox.TabIndex = 5;
            // 
            // PropertiesForm
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(359, 139);
            this.Controls.Add(this.fileLetterTextBox);
            this.Controls.Add(this.lblFileLetter);
            this.Controls.Add(this.fileAddressLabel);
            this.Controls.Add(this.lblFileAddress);
            this.Controls.Add(this.fileSizeLabel);
            this.Controls.Add(this.lblFileSize);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.FileNumberTextBox);
            this.Controls.Add(this.lblFileNumber);
            this.Controls.Add(this.FileNameTextBox);
            this.Controls.Add(this.lblFileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PropertiesForm";
            this.Text = "File Properties";
            this.Load += new System.EventHandler(this.PropertiesForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.TextBox FileNameTextBox;
        private System.Windows.Forms.Label lblFileNumber;
        private System.Windows.Forms.TextBox FileNumberTextBox;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Label lblFileSize;
        private System.Windows.Forms.Label fileSizeLabel;
        private System.Windows.Forms.Label fileAddressLabel;
        private System.Windows.Forms.Label lblFileAddress;
        private System.Windows.Forms.Label lblFileLetter;
        private System.Windows.Forms.TextBox fileLetterTextBox;

    }
}