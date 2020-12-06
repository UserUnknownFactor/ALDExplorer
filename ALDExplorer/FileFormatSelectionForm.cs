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
    public partial class FileFormatSelectionForm : Form
    {
        public FileFormatSelectionForm()
        {
            InitializeComponent();
        }

        public static AldFileType SelectFileType()
        {
            using (var form = new FileFormatSelectionForm())
            {
                var dialogResult = form.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    if (form.aldRadioButton.Checked)
                    {
                        return AldFileType.AldFile;
                    }
                    if (form.datRadioButton.Checked)
                    {
                        return AldFileType.DatFile;
                    }
                    if (form.alkRadioButton.Checked)
                    {
                        return AldFileType.AlkFile;
                    }
                    if (form.afa1RadioButton.Checked)
                    {
                        return AldFileType.AFA1File;
                    }
                    if (form.afa2RadioButton.Checked)
                    {
                        return AldFileType.AFA2File;
                    }
                }
            }
            return AldFileType.Invalid;
        }

        private void FileFormatSelectionForm_Load(object sender, EventArgs e)
        {

        }
    }

    
}
