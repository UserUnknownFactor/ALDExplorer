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
    public partial class ImportFileTypeForm : Form
    {
        public string FileType
        {
            get
            {
                return this.comboBox1.SelectedItem.ToString();
            }
        }

        public ImportFileTypeForm()
        {
            InitializeComponent();
        }

        public ImportFileTypeForm(params string[] fileTypes) : this()
        {
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.AddRange(fileTypes);
        }


        private void ImportFileTypeForm_Load(object sender, EventArgs e)
        {
            this.comboBox1.SelectedIndex = 0;
        }
    }
}
