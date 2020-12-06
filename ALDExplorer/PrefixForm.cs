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
    public partial class PrefixForm : Form
    {
        string _path = "";
        public PrefixForm(string path) : this()
        {
            _path = path;
        }

        public PrefixForm()
        {
            InitializeComponent();
        }

        private void PrefixForm_Load(object sender, EventArgs e)
        {
            SetPath(_path);
        }

        private void SetPath(string path)
        {
            _path = path;
            string[] directories = path.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Reverse().ToArray();
            string prefix = "";
            for (int i = 0; i < directories.Length; i++)
            {
                string dir = directories[i];
                prefix = dir + "\\" + prefix;
                lstDirectories.Items.Add(prefix);
            }
        }

        public static string GetPrefix(string path)
        {
            using (var prefixForm = new PrefixForm(Path.GetFullPath(path)))
            {
                if (prefixForm.ShowDialog() == DialogResult.OK)
                {
                    return prefixForm.DirectoryNameTextBox.Text;
                }
                else
                {
                    return "";
                }
            }
        }

        private void lstDirectories_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = lstDirectories.SelectedItem as string;
            if (selectedText != null)
            {
                DirectoryNameTextBox.Text = selectedText;
            }
        }
    }
}
