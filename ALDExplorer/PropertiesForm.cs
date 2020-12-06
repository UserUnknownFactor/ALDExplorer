using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ALDExplorer
{
    public partial class PropertiesForm : Form
    {
        public bool Dirty;

        AldFileEntry _fileEntry;
        public AldFileEntry FileEntry
        {
            get
            {
                return _fileEntry;
            }
            set
            {
                _fileEntry = value;
                ReadFields();
            }
        }

        private int FileLetter
        {
            get
            {
                int fileLetter = 1;
                string fileLetterString = fileLetterTextBox.Text.ToUpperInvariant();

                if (fileLetterString.Length > 0)
                {
                    char l = fileLetterString[0];
                    if (l >= 'A' && l <= 'Z')
                    {
                        fileLetter = l - 'A' + 1;
                    }
                    if (l == '@')
                    {
                        fileLetter = 0;
                    }
                }
                return fileLetter;
            }
            set
            {
                this.fileLetterTextBox.Text = ((value == 0) ? "@" : (((char)(value - 1 + 'A' )).ToString()));
            }
        }

        private void ReadFields()
        {
            if (FileEntry == null)
            {
                this.FileNameTextBox.Text = "";
                this.FileNumberTextBox.Text = "";
                this.fileLetterTextBox.Text = "";
                this.fileSizeLabel.Text = "";
                this.fileAddressLabel.Text = "";

                //this.FileTypeComboBox.Text = "";
            }
            else
            {
                this.FileNameTextBox.Text = FileEntry.FileName;
                this.FileNumberTextBox.Text = FileEntry.FileNumber.ToString();
                this.FileLetter = FileEntry.FileLetter;
                this.fileSizeLabel.Text = FileEntry.FileSize.ToString();
                this.fileAddressLabel.Text = FileEntry.FileAddress.ToString("X");

                var parent = FileEntry.Parent;
                if (parent != null)
                {
                    var fileType = parent.FileType;
                    if (fileType == AldFileType.AFA1File || fileType == AldFileType.AFA2File)
                    {
                        this.lblFileLetter.Visible = false;
                        this.lblFileNumber.Visible = false;
                        this.fileLetterTextBox.Visible = false;
                        this.FileNumberTextBox.Visible = false;
                    }
                }
                //this.FileTypeComboBox.Text = FileEntry.FileType.ToString();
            }
        }

        public PropertiesForm()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            Apply();
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            Apply();
        }

        private void Apply()
        {
            this.Dirty = true;
            this.FileEntry.FileName = FileNameTextBox.Text;
            int fileNumber = 0;
            int.TryParse(FileNumberTextBox.Text, out fileNumber);
            this.FileEntry.FileNumber = fileNumber;
            this.FileEntry.FileLetter = this.FileLetter;

            //FileType fileType = (FileType)0;
            //try
            //{
            //    fileType = (FileType)Enum.Parse(typeof(FileType), this.FileTypeComboBox.Text, true);
            //}
            //catch (ArgumentException)
            //{
            //    int fileTypeInt = 0;
            //    int.TryParse(this.FileTypeComboBox.Text, out fileTypeInt);
            //    fileType = (FileType)fileTypeInt;
            //}
            //this.FileEntry.FileType = fileType;
        }

        private void PropertiesForm_Load(object sender, EventArgs e)
        {
            //var names = Enum.GetNames(typeof(FileType));
            //FileTypeComboBox.BeginUpdate();
            //foreach (var name in names)
            //{
            //    FileTypeComboBox.Items.Add(name);
            //}
            //FileTypeComboBox.EndUpdate();
        }
    }
}
