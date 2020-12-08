using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FreeImageAPI;
using System.Diagnostics;
using WMPLib;
//using DDW.Swf;
using ALDExplorer.Formats;

namespace ALDExplorer
{
    public partial class Form1 : Form
    {
        AldFileCollection loadedAldFile = null;
        //AldFileCollection loadedAldFiles = null;
        public Form1()
        {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Debug.AutoFlush = true;
            InitializeComponent();
        }

        bool DoNotConvertImageFiles
        {
            get
            {
                return this.doNotConvertImageFilesToolStripMenuItem.Checked;
            }
            set
            {
                this.doNotConvertImageFilesToolStripMenuItem.Checked = value;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void ConvertFile(string fileName, string to)
        {
            try
            {
                using (var bitmap = GetImage(fileName))
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var outName = Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fileName) + to;
                    switch (to)
                    {
                        case ".png":
                            bitmap.Save(outName, FREE_IMAGE_FORMAT.FIF_PNG);
                            Debug.Print("written to: " + outName);
                            bitmap.Dispose();
                            break;
                        case ".ajp":
                            using (var ms = new FileStream(outName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                                ImageConverter.SaveAjp(ms, bitmap);
                            Debug.Print("written to: " + outName);
                            bitmap.Dispose();
                            break;
                        case ".vsp":
                            using (var ms = new FileStream(outName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                                ImageConverter.SaveVsp(ms, bitmap);
                            Debug.Print("written to: " + outName);
                            bitmap.Dispose();
                            break;
                        case ".pms":
                            using (var ms = new FileStream(outName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                                ImageConverter.SavePms(ms, bitmap);
                            Debug.Print("written to: " + outName);
                            bitmap.Dispose();
                            break;
                        case ".qnt":
                            using (var ms = new FileStream(outName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                                ImageConverter.SaveQnt(ms, bitmap);
                            Debug.Print("written to: " + outName);
                            bitmap.Dispose();
                            break;
                    }
                }
            }
            catch (InvalidDataException)
            {
                MessageBox.Show(this, "The loaded file is not a valid " + 
                    Path.GetExtension(fileName).Substring(1).ToUpper() + 
                    " file", "ALDExplorer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

            LoadTreeView(false);
        }

        private void OpenFile()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "AliceSoft Archive Files (*.ALD;*.AFA;*.ALK;*.DAT)|*.ald;*.afa;*.alk;*.dat|All Files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    OpenFile(openFileDialog.FileName);
                }
            }
        }

        private void OpenFile(string fileName)
        {
            loadedAldFile = new AldFileCollection();
            try
            {
                loadedAldFile.ReadFile(fileName);
            }
            catch (InvalidDataException)
            {
                MessageBox.Show(this, "The loaded file is not a valid AliceSoft .ALD, .AFA, .ALK, or .DAT file.", "ALDExplorer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

            LoadTreeView(false);
        }

        void LoadTreeView()
        {
            LoadTreeView(false);
        }

        private void LoadTreeView(bool preserveItems)
        {
            if (loadedAldFile == null) return;

            try
            {
                listView1.BeginUpdate();

                HideFlashPlayer();
                HideMediaPlayer();
                int firstVisibleIndex = 0;
                int focusedIndex = -1;

                HashSet<AldFileEntry> selectedEntries = new HashSet<AldFileEntry>();
                HashSet<AldFileEntry> expandedEntries = new HashSet<AldFileEntry>();

                if (preserveItems)
                {
                    var firstVisibleItem = listView1.TopItem;
                    if (firstVisibleItem != null)
                    {
                        firstVisibleIndex = firstVisibleItem.Index;
                    }
                    //note focused index
                    var focusedItem = listView1.FocusedItem;
                    if (focusedItem != null)
                    {
                        focusedIndex = focusedItem.Index;
                    }

                    //remember expanded items and selected items
                    expandedEntries.AddRange(GetExpandedItems());
                    selectedEntries.AddRange(GetSelectedEntries());
                }


                var itemCollection = listView1.Items;
                listView1.Items.Clear();
                //treeView1.BeginUpdate();
                //treeView1.Nodes.Clear();
                int lastFileLetter = -1;

                if (loadedAldFile.FileEntries.Count == 0)
                {
                    var aldFile = loadedAldFile.AldFiles.FirstOrDefault();
                    //add new node
                    var newNode2 = new ListViewItem();
                    if (aldFile != null)
                    {
                        newNode2.Text = Path.GetFileName(aldFile.AldFileName);
                    }
                    else
                    {
                        newNode2.Text = "Invalid ALD File";
                    }
                    newNode2.Tag = aldFile;
                    newNode2.ImageIndex = 1;
                    itemCollection.Add(newNode2);
                }

                foreach (var entry in loadedAldFile.FileEntries)
                {
                    if (entry.FileLetter != lastFileLetter)
                    {
                        lastFileLetter = entry.FileLetter;
                        var aldFile = loadedAldFile.GetAldFileByLetter(lastFileLetter);

                        //add new node
                        var newNode2 = new ListViewItem();
                        if (aldFile != null)
                        {
                            newNode2.Text = Path.GetFileName(aldFile.AldFileName);
                        }
                        else
                        {
                            newNode2.Text = "Invalid ALD File";
                        }
                        newNode2.Tag = aldFile;
                        newNode2.ImageIndex = 1;
                        itemCollection.Add(newNode2);
                    }
                    //var newNode = new TreeNode();
                    var newNode = new ListViewItem();
                    if (entry.FileName == null)
                    {
                        entry.FileName = "CORRUPT FILE " + entry.FileNumber.ToString();
                    }
                    newNode.Text = entry.FileName;
                    newNode.Tag = entry;
                    newNode.SetFileIndentCount();

                    string ext = Path.GetExtension(entry.FileName).ToLowerInvariant();
                    int imageIndex = GetImageIndex(ext);
                    newNode.ImageIndex = imageIndex;
                    //if (Debugger.IsAttached)
                    {
                        newNode.ToolTipText = "File Number: " + (entry.FileNumber) + /*", File Type: " + entry.FileType + */ ", File Size: " + entry.FileSize + ", File Address: " + entry.FileAddress.ToString("X");
                    }
                    itemCollection.Add(newNode);
                }
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                if (preserveItems)
                {
                    if (firstVisibleIndex >= 0 && firstVisibleIndex < listView1.Items.Count)
                    {
                        var lastItem = listView1.Items[listView1.Items.Count - 1];
                        lastItem.EnsureVisible();
                        var newTopItem = listView1.Items[firstVisibleIndex];
                        newTopItem.EnsureVisible();
                    }
                    if (focusedIndex >= 0 && focusedIndex < listView1.Items.Count)
                    {
                        var listItem = listView1.Items[focusedIndex];
                        listItem.Focused = true;
                    }
                    ExpandItems(expandedEntries);
                    listView1.SelectedIndices.Clear();
                    SelectItems(selectedEntries);
                }
            }
            finally
            {
                listView1.EndUpdate();
            }
        }

        private void SelectItems(IEnumerable<AldFileEntry> selectedEntriesSequence)
        {
            HashSet<AldFileEntry> selectedEntries = new HashSet<AldFileEntry>(selectedEntriesSequence);
            foreach (ListViewItem item in listView1.Items)
            {
                var entry = item.Tag as AldFileEntry;
                if (entry != null && selectedEntries.Contains(entry))
                {
                    item.Selected = true;
                }
            }
        }

        private void ExpandItems(IEnumerable<AldFileEntry> expandedEntriesSequence)
        {
            HashSet<AldFileEntry> expandedEntries = new HashSet<AldFileEntry>(expandedEntriesSequence);
            foreach (ListViewItem item in listView1.Items)
            {
                var entry = item.Tag as AldFileEntry;
                if (entry != null && expandedEntries.Contains(entry))
                {
                    TryExpandItem(item.Index);
                }
            }
        }

        private int[] GetExpandedIndices()
        {
            return listView1.Items.OfType<ListViewItem>().Where(i => i.IsFileNode() && i.NextNodeIsSubfileNode()).Select(i => i.Index).ToArray();
        }

        private AldFileEntry[] GetExpandedItems()
        {
            return GetExpandedIndices().Select(i => listView1.Items[i].Tag as AldFileEntry).ToArray();
        }

        private static int GetImageIndex(string ext)
        {
            int imageIndex = 0;
            if (ext == ".vsp" || ext == ".pms" || ext == ".jpg" || ext == ".ajp" || ext == ".bmp" || ext == ".qnt" || ext == ".png")
            {
                imageIndex = 2;
            }
            else if (ext == ".mp3" || ext == ".wav" || ext == ".ogg" || ext == ".mid")
            {
                imageIndex = 3;
            }
            else if (ext == ".swf" || ext == ".aff")
            {
                imageIndex = 6;
            }
            else if (ext == ".dcf" || ext == ".pcf")
            {
                imageIndex = 2;
            }
            else if (ext == ".sco")
            {
                imageIndex = 4;
            }
            else if (ext == ".flat")
            {
                imageIndex = 7;
            }
            else
            {
                imageIndex = 0;
            }
            return imageIndex;
        }

        AxWMPLib.AxWindowsMediaPlayer axMediaPlayer = null;
        AxShockwaveFlashObjects.AxShockwaveFlash axFlashPlayer = null;
        bool mediaPlayerCreated = false;
        bool flashPlayerCreated = false;

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var entry = e.Node.Tag as AldFileEntry;
            DisplayEntry(entry);
        }

        private void DisplayEntry(AldFileEntry entry)
        {
            if (entry != null)
            {
                FreeImageBitmap bitmap = null;
                string extension = Path.GetExtension(entry.FileName).ToLowerInvariant();
                using (bitmap = GetBitmapFromFile(entry))
                {
                    if (false)
                    {
                        if (Debugger.IsAttached && bitmap != null)
                        {
                            //long originalSize = entry.FileSize;
                            var ms = new MemoryStream();
                            switch (extension)
                            {
                                case ".ajp":
                                    ImageConverter.SaveAjp(ms, bitmap);
                                    bitmap.Dispose();
                                    bitmap = ImageConverter.LoadAjp(ms.ToArray());
                                    break;
                                case ".vsp":
                                    ImageConverter.SaveVsp(ms, bitmap);
                                    bitmap.Dispose();
                                    bitmap = ImageConverter.LoadVsp(ms.ToArray());
                                    break;
                                case ".pms":
                                    ImageConverter.SavePms(ms, bitmap);
                                    bitmap.Dispose();
                                    bitmap = ImageConverter.LoadPms(ms.ToArray());
                                    break;
                                case ".qnt":
                                    ImageConverter.SaveQnt(ms, bitmap);
                                    bitmap.Dispose();
                                    bitmap = ImageConverter.LoadQnt(ms.ToArray());
                                    break;
                            }
                        }
                    }

                    if (extension == ".mid" || extension == ".mp3" || extension == ".ogg" || extension == ".wav")
                    {
                        CreateMediaPlayer(entry);
                    }

                    if (extension == ".swf" || extension == ".aff")
                    {
                        CreateFlashPlayer(entry);
                    }

                    if (bitmap != null)
                    {
                        HideMediaPlayer();
                        HideFlashPlayer();
                        if (pictureBox1.Image != null)
                        {
                            pictureBox1.Image.Dispose();
                            pictureBox1.Image = null;
                        }
                        pictureBox1.Image = bitmap.ToBitmap();
                        pictureBox1.Visible = true;
                    }
                }
            }
        }

        private void CreateFlashPlayer(AldFileEntry entry)
        {
            if (axFlashPlayer == null)
            {
                if (!flashPlayerCreated)
                {
                    flashPlayerCreated = true;
                    try
                    {
                        axFlashPlayer = new AxShockwaveFlashObjects.AxShockwaveFlash();
                    }
                    catch
                    {

                    }
                    if (axFlashPlayer != null)
                    {
                        axFlashPlayer.SuspendLayout();
                        axFlashPlayer.Parent = splitContainer1.Panel2;
                        axFlashPlayer.Visible = true;
                        axFlashPlayer.Dock = DockStyle.Fill;
                        axFlashPlayer.PerformLayout();
                    }

                }
            }

            if (axFlashPlayer != null)
            {

                axFlashPlayer.LoadMovie(0, "");
                string tempFileName = GetTempFileFromEntry(entry);
                HideMediaPlayer();
                pictureBox1.Visible = false;
                axFlashPlayer.Visible = true;
                axFlashPlayer.Dock = DockStyle.None;
                axFlashPlayer.Width = axFlashPlayer.Width - 1;
                axFlashPlayer.Height = axFlashPlayer.Height;
                axFlashPlayer.Dock = DockStyle.Fill;
                axFlashPlayer.ScaleMode = 3;
                axFlashPlayer.LoadMovie(0, tempFileName);
            }
        }

        private void CreateMediaPlayer(AldFileEntry entry)
        {
            if (axMediaPlayer == null)
            {
                if (!mediaPlayerCreated)
                {
                    mediaPlayerCreated = true;
                    try
                    {
                        axMediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
                    }
                    catch
                    {

                    }
                    if (axMediaPlayer != null)
                    {
                        axMediaPlayer.SuspendLayout();
                        axMediaPlayer.Parent = splitContainer1.Panel2;
                        axMediaPlayer.Visible = true;
                        axMediaPlayer.Dock = DockStyle.Fill;
                        axMediaPlayer.PerformLayout();
                    }
                }
            }

            if (axMediaPlayer != null)
            {
                axMediaPlayer.URL = "";
                string tempFileName = GetTempFileFromEntry(entry);
                pictureBox1.Visible = false;
                HideFlashPlayer();
                axMediaPlayer.Visible = true;
                axMediaPlayer.Dock = DockStyle.None;
                axMediaPlayer.Width = axMediaPlayer.Width - 1;
                axMediaPlayer.Height = axMediaPlayer.Height;
                axMediaPlayer.Dock = DockStyle.Fill;
                axMediaPlayer.URL = tempFileName;
            }
        }

        private static string GetTempFileFromEntry(AldFileEntry entry)
        {
            TempFileManager.DefaultInstance.DeleteMyTempFiles();
            string fileName = entry.FileName;
            string ext = Path.GetExtension(entry.FileName).ToLowerInvariant();
            var fileBytes = entry.GetFileData();
            if ((ext == ".aff" || ext == ".swf") && IsAffFile(fileBytes))
            {
                fileName = Path.ChangeExtension(fileName, ".swf");
                fileBytes = SwfToAffConverter.ConvertAffToSwf(fileBytes);
            }
            string tempFileName = TempFileManager.DefaultInstance.CreateTempFile(fileName);

            File.WriteAllBytes(tempFileName, fileBytes);
            return tempFileName;
        }

        private void HideMediaPlayer()
        {
            if (axMediaPlayer != null)
            {
                axMediaPlayer.Visible = false;
                if (axMediaPlayer.URL != "")
                {
                    axMediaPlayer.URL = "";
                    TempFileManager.DefaultInstance.DeleteMyTempFiles();
                }
            }
        }

        private void HideFlashPlayer()
        {
            if (axFlashPlayer != null)
            {
                if (axFlashPlayer.Visible == true)
                {
                    axFlashPlayer.Visible = false;
                    axFlashPlayer.Stop();
                    axFlashPlayer.LoadMovie(0, "about:blank");
                    TempFileManager.DefaultInstance.DeleteMyTempFiles();
                }
            }
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            //if (Debugger.IsAttached)
            //{
            //    this.saveAsWithPatchToolStripMenuItem.Visible = true;
            //}
            //else
            //{
            //    var owner = this.saveAsWithPatchToolStripMenuItem.OwnerItem as ToolStripMenuItem;
            //    if (owner != null)
            //    {
            //        owner.DropDownItems.Remove(this.saveAsWithPatchToolStripMenuItem);
            //    }
            //}

            if (this.loadedAldFile == null)
            {
                this.saveAsToolStripMenuItem.Enabled = false;
                this.saveAsWithPatchToolStripMenuItem.Enabled = false;
            }
            else
            {
                this.saveAsWithPatchToolStripMenuItem.Enabled = true;
                this.saveAsToolStripMenuItem.Enabled = true;
            }
        }

        private void makePatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakePatch();
        }

        private void MakePatch()
        {
            //if (loadedAldFile == null)
            //{
            //    return;
            //}
            //string baseName = Path.GetFileNameWithoutExtension(loadedAldFile.AldFileName);
            //baseName = baseName.Substring(0, baseName.Length - 1) + "Y.ALD";
            //string newFileName = Path.Combine(Path.GetDirectoryName(loadedAldFile.AldFileName), baseName);

            //string patchDirectory;

            //using (var openFileDialog = new OpenFileDialog())
            //{
            //    openFileDialog.CheckFileExists = false;
            //    openFileDialog.CheckPathExists = true;
            //    openFileDialog.Filter = "All Files (*.*)|*.*";
            //    openFileDialog.FileName = "SELECT A DIRECTORY";
            //    if (openFileDialog.ShowDialog() == DialogResult.OK)
            //    {
            //        string fileName = openFileDialog.FileName;
            //        if (!Directory.Exists(fileName))
            //        {
            //            fileName = Path.GetDirectoryName(fileName);
            //        }
            //        patchDirectory = fileName;
            //        MakePatch(newFileName, patchDirectory);

            //    }

            //}

        }

        private void MakePatch(string newFileName, string patchDirectory)
        {
            //if (loadedAldFile == null)
            //{
            //    return;
            //}
            //var newAldFile = new AldFile();
            //if (loadedAldFile.Footer != null)
            //{
            //    newAldFile.Footer = (byte[])loadedAldFile.Footer.Clone();
            //}

            //foreach (var entry in loadedAldFile.FileEntries)
            //{
            //    string fileName = entry.FileName;
            //    string ext = Path.GetExtension(fileName).ToLowerInvariant();

            //    var entry2 = entry.Clone();
            //    string externalFileName = Path.Combine(patchDirectory, fileName);
            //    if (File.Exists(externalFileName))
            //    {
            //        entry2.ReplacementFileName = externalFileName;
            //    }
            //    externalFileName = Path.Combine(patchDirectory, Path.ChangeExtension(fileName, ".png"));
            //    if (File.Exists(externalFileName))
            //    {
            //        entry2.ReplacementFileName = externalFileName;
            //    }
            //    if (!string.IsNullOrEmpty(entry2.ReplacementFileName))
            //    {
            //        newAldFile.FileEntries.Add(entry2);
            //    }
            //}

            //newAldFile.SaveFileAndCommit(newFileName);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void Save()
        {
            if (loadedAldFile == null)
            {
                return;
            }
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = loadedAldFile.AldFileName;
            saveFileDialog.Filter = "AliceSoft Archive Files (*.ALD;*.AFA;*.ALK;*.DAT)|*.ald;*.afa;*.alk;*.dat|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                loadedAldFile.SaveFile(saveFileDialog.FileName);
            }
        }

        private void importAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportAll();
        }

        private void ImportAll()
        {
            if (loadedAldFile == null)
            {
                return;
            }

            string patchDirectory;

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.CheckFileExists = false;
                openFileDialog.CheckPathExists = true;
                openFileDialog.Filter = "All Files (*.*)|*.*";
                openFileDialog.FileName = "SELECT A DIRECTORY";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = openFileDialog.FileName;
                    if (!Directory.Exists(fileName))
                    {
                        fileName = Path.GetDirectoryName(fileName);
                    }
                    patchDirectory = fileName;
                    int fileCount = ImportAll(patchDirectory);
                    if (fileCount == 0)
                    {
                        MessageBox.Show(this, "No matching files were found!", "ALDExplorer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else if (fileCount >= 1)
                    {
                        MessageBox.Show(this, fileCount.ToString() + " file(s) will be imported the next time this ALD file is saved.", "ALDExplorer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            LoadTreeView(true);
        }

        private int ImportAll(string patchDirectory)
        {
            if (loadedAldFile == null)
            {
                return -1;
            }

            var entries = loadedAldFile.FileEntries;
            return ImportAll2(patchDirectory, entries);
        }

        private int ImportAll2(string patchDirectory, IList<AldFileEntry> entries)
        {
            int fileCount = 0;

            foreach (var entry in entries)
            {
                string fileName = entry.FileName;
                string fileNameThisDirectory = Path.GetFileName(fileName);

                //try current directory and other path
                bool imported = TryImportFile(patchDirectory, entry, fileName);
                if (!imported && fileNameThisDirectory != fileName)
                {
                    imported = TryImportFile(patchDirectory, entry, fileNameThisDirectory);
                }

                if (entry.HasSubImages())
                {
                    var subImages = entry.GetSubImages();
                    string newDirectory1 = Path.Combine(patchDirectory, fileName + "_files");
                    string newDirectory2 = Path.Combine(patchDirectory, fileNameThisDirectory + "_files");
                    if (Directory.Exists(newDirectory1))
                    {
                        fileCount += ImportAll2(newDirectory1, subImages);
                    }
                    else if (Directory.Exists(newDirectory2))
                    {
                        fileCount += ImportAll2(newDirectory2, subImages);
                    }
                }

                if (imported)
                {
                    fileCount++;
                }
            }
            return fileCount;
        }

        private bool TryImportFile(string patchDirectory, AldFileEntry entry, string fileName)
        {
            return TryImportFile(patchDirectory, entry, fileName, null);
        }

        private bool TryImportFile(string patchDirectory, AldFileEntry entry, string fileName, string newExt)
        {
            bool imported = false;
            if (!String.IsNullOrEmpty(newExt))
            {
                fileName = Path.ChangeExtension(fileName, newExt);
            }

            //string fileExt = Path.GetExtension(fileName).ToLowerInvariant();
            //string entryExt = Path.GetExtension(entry.FileName).ToLowerInvariant();

            string externalFileName = Path.Combine(patchDirectory, fileName);
            if (File.Exists(externalFileName))
            {
                entry.ReplacementFileName = externalFileName;
                imported = true;
            }
            else if (!this.DoNotConvertImageFiles && String.IsNullOrEmpty(newExt))
            {
                if (TryImportFile(patchDirectory, entry, fileName, ".png")) { return true; }
                if (TryImportFile(patchDirectory, entry, fileName, ".jpg")) { return true; }
                if (TryImportFile(patchDirectory, entry, fileName, ".swf")) { return true; }
                if (TryImportFile(patchDirectory, entry, fileName, ".mp3")) { return true; }
                if (TryImportFile(patchDirectory, entry, fileName, ".ogg")) { return true; }
                if (TryImportFile(patchDirectory, entry, fileName, ".wav")) { return true; }
            }
            //    string desiredExt = ".png";



            //    externalFileName = Path.Combine(patchDirectory, Path.ChangeExtension(fileName, ".png"));
            //    if (File.Exists(externalFileName))
            //    {
            //        entry.ReplacementFileName = externalFileName;
            //        imported = true;
            //    }
            return imported;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //axWindowsMediaPlayer1.Visible = false;
            //axWindowsMediaPlayer1.Dock = DockStyle.Fill;
            pictureBox1.Visible = true;
            pictureBox1.Dock = DockStyle.Fill;

            this.debugCommandToolStripMenuItem.Visible = Debugger.IsAttached;

            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                OpenFile(args[1]);
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null)
                {
                    OpenFile(files[0]);
                }
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragLeave(object sender, EventArgs e)
        {

        }

        private void Form1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void exportAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportAll();
        }

        private void ExportAll()
        {
            if (loadedAldFile == null)
            {
                return;
            }
            ExportFiles(loadedAldFile.FileEntries);
        }

        private void ExportSelectedFiles()
        {
            if (loadedAldFile == null)
            {
                return;
            }

            AldFileEntry[] entries = GetSelectedEntries();

            if (entries.Length > 0)
            {
                ExportFiles(entries);
            }

        }

        private AldFileEntry[] GetSelectedEntries()
        {
            List<AldFileEntry> entries = new List<AldFileEntry>();

            //foreach (TreeNode node in treeView1.Nodes)
            foreach (ListViewItem node in listView1.SelectedItems)
            {
                //if (node.IsSelected)
                {
                    var entry = node.Tag as AldFileEntry;
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }
            return entries.ToArray();
        }

        private void ExportCheckedFiles()
        {
            if (loadedAldFile == null)
            {
                return;
            }

            List<AldFileEntry> entries = GetCheckedEntries();

            if (entries.Count > 0)
            {
                ExportFiles(entries);
            }
        }

        private List<AldFileEntry> GetCheckedEntries()
        {
            List<AldFileEntry> entries = new List<AldFileEntry>();

            //foreach (TreeNode node in treeView1.Nodes)
            foreach (ListViewItem node in listView1.CheckedItems)
            {
                if (node.Checked)
                {
                    var entry = node.Tag as AldFileEntry;
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }
            return entries;
        }

        private void ExportFiles(IEnumerable<AldFileEntry> entries)
        {
            int count = entries.Count();
            if (count == 0)
            {
                return;
            }
            if (count == 1 && !entries.FirstOrDefault().HasSubImages())
            {
                ExportFile(entries.FirstOrDefault());
                return;
            }
            string outputPath = GetOutputPath();
            if (outputPath == null)
            {
                return;
            }
            ExportFiles(entries, outputPath);
        }

        private void ExportFiles(IEnumerable<AldFileEntry> entries, string outputPath)
        {
            foreach (var entry in entries)
            {
                ExportFile(entry, outputPath);
            }
        }

        private static string GetOutputPath()
        {
            return GetOutputPath(true);
        }

        private static string GetOutputPath(bool saving)
        {
            string outputPath = null;
            FileDialog fileDialog = null;
            try
            {
                if (saving)
                {
                    var saveFileDialog = new SaveFileDialog();
                    saveFileDialog.OverwritePrompt = false;
                    fileDialog = saveFileDialog;
                    fileDialog.FileName = "SELECT DIRECTORY TO EXPORT FILES TO";
                }
                else
                {
                    fileDialog = new OpenFileDialog();
                    fileDialog.FileName = "SELECT DIRECTORY TO IMPORT FILES FROM";
                }
                fileDialog.CheckFileExists = false;
                fileDialog.CheckPathExists = true;
                fileDialog.Filter = "All Files (*.*)|*.*";
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (Directory.Exists(fileDialog.FileName))
                    {
                        outputPath = fileDialog.FileName;
                    }
                    else
                    {
                        outputPath = Path.GetDirectoryName(fileDialog.FileName);
                    }
                }
                else
                {
                    outputPath = null;
                }
            }
            finally
            {
                if (fileDialog != null)
                {
                    fileDialog.Dispose();
                }
            }
            return outputPath;
        }

        private void ExportFile(AldFileEntry entry)
        {
            ExportFile(entry, null);
            //return;

            //FreeImageBitmap bitmap = null;
            //if (!this.DoNotConvertImageFiles)
            //{
            //    bitmap = GetBitmapFromFile(entry);
            //}
            //try
            //{
            //    string inputFileName = entry.FileName;
            //    inputFileName = Path.GetFileName(inputFileName);
            //    string extension = Path.GetExtension(inputFileName).ToLowerInvariant();

            //    if (extension == ".aff")
            //    {
            //        inputFileName = Path.ChangeExtension(inputFileName, ".swf");
            //    }

            //    string outputFileName = PromptForOutputFileName(inputFileName, extension, bitmap);
            //    if (outputFileName == null)
            //    {
            //        return;
            //    }

            //    ExportFileWithName(entry, outputFileName, bitmap);
            //}
            //finally
            //{
            //    if (bitmap != null)
            //    {
            //        bitmap.Dispose();
            //    }
            //}
        }

        private string PromptForOutputFileName(string inputFileName, string extension, FreeImageBitmap bitmap)
        {
            inputFileName = inputFileName.Replace('/', '\\');
            string outputFileName;
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.OverwritePrompt = true;
                saveFileDialog.CheckFileExists = false;
                saveFileDialog.CheckPathExists = true;

                if (bitmap != null)
                {
                    saveFileDialog.Filter = "PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|GIF Files (*.gif)|*.gif|JPG Files (*.jpg)|*.jpg|TGA Files (*.tga)|*.tga|All Files (*.*)|*.*";
                }
                else
                {
                    saveFileDialog.Filter = "All Files (*.*)|*.*";
                }
                string desiredExt = GetDesiredExtension(extension, bitmap);

                saveFileDialog.DefaultExt = desiredExt;
                saveFileDialog.FileName = Path.ChangeExtension(inputFileName, desiredExt);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    outputFileName = saveFileDialog.FileName;
                }
                else
                {
                    outputFileName = null;
                }
            }
            return outputFileName;
        }

        private string GetDesiredExtension(string extension, FreeImageBitmap bitmap)
        {
            string desiredExt = extension;
            if (bitmap != null)
            {
                desiredExt = ".png";
                if (extension == ".ajp" && bitmap.ColorDepth == 24)
                {
                    desiredExt = ".jpg";
                }
                if (extension == ".jpg")
                {
                    desiredExt = ".jpg";
                }
            }
            if (extension == ".aff" && !this.DoNotConvertImageFiles)
            {
                desiredExt = ".swf";
            }
            return desiredExt;
        }

        private void ExportFile(AldFileEntry entry, string outputPath)
        {
            string fileName = entry.FileName;
            if (!this.includeDirectoriesWhenExportingFilesToolStripMenuItem.Checked)
            {
                fileName = Path.GetFileName(fileName);
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (entry.HasSubImages() && outputPath != null)
            {
                string outputPathNew = Path.Combine(outputPath, fileName + "_files");
                var subImages = entry.GetSubImages();
                foreach (var subImageEntry in subImages)
                {
                    ExportFile(subImageEntry, outputPathNew);
                }
            }

            FreeImageBitmap bitmap = null;
            if (!this.DoNotConvertImageFiles)
            {
                bitmap = GetBitmapFromFile(entry);
            }

            try
            {
                string outputFileName;
                string desiredExt = GetDesiredExtension(extension, bitmap);

                if (outputPath == null)
                {
                    outputFileName = PromptForOutputFileName(fileName, extension, bitmap);
                    if (outputFileName == null)
                    {
                        return;
                    }
                }
                else
                {
                    outputFileName = Path.Combine(outputPath, Path.ChangeExtension(fileName, desiredExt));
                }

                ExportFileWithName(entry, outputFileName, bitmap);

                //if (bitmap != null)
                //{
                //    string outputExt = Path.GetExtension(outputFileName).ToLowerInvariant();

                //    if (outputExt == ".jpg" && extension == ".ajp" && bitmap.ColorDepth == 24)
                //    {
                //        byte[] jpegFile;
                //        FreeImageBitmap alphaImage;
                //        AjpHeader ajpHeader;
                //        ImageConverter.LoadAjp(entry.GetFileData(), out jpegFile, out alphaImage, out ajpHeader);
                //        File.WriteAllBytes(outputFileName, jpegFile);
                //    }
                //    else if (outputExt == ".jpg" && extension == ".jpg")
                //    {
                //        File.WriteAllBytes(outputFileName, entry.GetFileData());
                //    }
                //    else
                //    {
                //        ExportFileWithName(entry, outputFileName, bitmap);
                //        bitmap.Save(outputFileName);
                //    }
                //}
                //else
                //{
                //    ExportFileWithName(entry, outputFileName, bitmap);
                //    //string outputFileName = Path.Combine(outputPath, fileName);
                //    //using (var fs = File.OpenWrite(outputFileName))
                //    //{
                //    //    entry.WriteDataToStream(fs);
                //    //}
                //}
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
            }
        }

        private void ExportFileWithName(AldFileEntry entry, string outputFileName, FreeImageBitmap bitmap)
        {
            string outputDirectory = Path.GetDirectoryName(outputFileName);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string outputExt = Path.GetExtension(outputFileName).ToLowerInvariant();
            string inputExt = Path.GetExtension(entry.FileName).ToLowerInvariant();

            if (bitmap != null)
            {
                if (outputExt == ".jpg" && inputExt == ".ajp" && bitmap.ColorDepth == 24)
                {
                    byte[] jpegFile;
                    FreeImageBitmap alphaImage;
                    AjpHeader ajpHeader;
                    ImageConverter.LoadAjp(entry.GetFileData(), out jpegFile, out alphaImage, out ajpHeader);
                    File.WriteAllBytes(outputFileName, jpegFile);
                }
                else if (outputExt == ".jpg" && inputExt == ".jpg")
                {
                    File.WriteAllBytes(outputFileName, entry.GetFileData());
                }
                else
                {
                    bitmap.Save(outputFileName);
                }
            }
            else
            {
                if (!this.DoNotConvertImageFiles && (inputExt == ".aff" || inputExt == ".swf") && outputExt == ".swf")
                {
                    var fileBytes = entry.GetFileData();
                    if (IsAffFile(fileBytes))
                    {
                        fileBytes = SwfToAffConverter.ConvertAffToSwf(fileBytes);
                    }
                    File.WriteAllBytes(outputFileName, fileBytes);
                }
                else if (!this.DoNotConvertImageFiles && (inputExt == ".aff" || inputExt == ".swf") && outputExt == ".aff")
                {
                    var fileBytes = entry.GetFileData();
                    if (!IsAffFile(fileBytes))
                    {
                        fileBytes = SwfToAffConverter.ConvertSwfToAff(fileBytes);
                    }
                    File.WriteAllBytes(outputFileName, fileBytes);
                }
                else
                {
                    using (var fs = File.OpenWrite(outputFileName))
                    {
                        entry.WriteDataToStream(fs);
                    }
                }
            }
        }

        private static bool IsAffFile(byte[] fileBytes)
        {
            bool isAffFile = false;
            string sig = ASCIIEncoding.ASCII.GetString(fileBytes, 0, 3);
            if (sig == "FWS" || sig == "CWS")
            {
                isAffFile = false;
            }
            else if (sig == "AFF")
            {
                isAffFile = true;
            }
            return isAffFile;
        }

        private FreeImageBitmap GetBitmapFromFile(AldFileEntry entry)
        {
            string fileName = entry.FileName;
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            //bool fileExtensionHandled = false;
            bool fileExtensionSupported =
                (extension == ".vsp") ||
                (extension == ".dcf") ||
                (extension == ".pcf") ||
                (extension == ".pms") ||
                (extension == ".qnt") ||
                (extension == ".ajp") ||
                (extension == ".bmp") ||
                (extension == ".gif") ||
                (extension == ".jpg") ||
                (extension == ".png");

            FreeImageBitmap bitmap = null;
            if (fileExtensionSupported)
            {
                var fileBytes = entry.GetFileData(true);
                bitmap = GetImage(fileBytes, extension);

                //for JPG files, try a PMS file at +10000 for the alpha channel
                if (bitmap != null && extension == ".jpg")
                {
                    var alphaChannel = bitmap.GetChannel(FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);
                    if (alphaChannel == null)
                    {
                        int otherFileNumber = entry.FileNumber + 10000;
                        var otherEntry = this.loadedAldFile.FileEntriesByNumber.GetOrNull(otherFileNumber);
                        if (otherEntry != null)
                        {
                            if (otherEntry.FileName.EndsWith(".pms", StringComparison.OrdinalIgnoreCase))
                            {
                                var alphaBitmap = GetImage(otherEntry.GetFileData(true), ".pms");
                                if (alphaBitmap != null && alphaBitmap.Width == bitmap.Width && alphaBitmap.Height == bitmap.Height)
                                {
                                    bitmap.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);
                                    bitmap.SetChannel(alphaBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);
                                }
                            }
                        }
                    }
                }
            }

            //if (extension == ".vsp")
            //{
            //    bitmap = ImageConverter.LoadVsp(entry.GetFileData());
            //    fileExtensionHandled = true;
            //}
            //else if (extension == ".pms")
            //{
            //    bitmap = ImageConverter.LoadPms(entry.GetFileData());
            //    fileExtensionHandled = true;
            //}
            //else if (extension == ".qnt")
            //{
            //    bitmap = ImageConverter.LoadQnt(entry.GetFileData());
            //    fileExtensionHandled = true;
            //}
            //else if (extension == ".ajp")
            //{
            //    bitmap = ImageConverter.LoadAjp(entry.GetFileData());
            //    fileExtensionHandled = true;
            //}
            //else if (extension == ".bmp")
            //{
            //    var ms = new MemoryStream();
            //    entry.WriteDataToStream(ms, false);
            //    bitmap = new FreeImageBitmap(ms, FREE_IMAGE_FORMAT.FIF_BMP);
            //    fileExtensionHandled = true;
            //}
            //else if (extension == ".jpg")
            //{
            //    var ms = new MemoryStream();
            //    entry.WriteDataToStream(ms, false);
            //    bitmap = new FreeImageBitmap(ms, FREE_IMAGE_FORMAT.FIF_JPEG);
            //    fileExtensionHandled = true;

            //    int otherFileNumber = entry.FileNumber + 10000;

            //    if (this.loadedAldFile.FileEntriesByNumber.ContainsKey(otherFileNumber))
            //    {
            //        var otherEntry = this.loadedAldFile.FileEntriesByNumber[otherFileNumber];
            //        if (otherEntry.FileName.EndsWith(".pms", StringComparison.OrdinalIgnoreCase))
            //        {
            //            using (var alphaBitmap = ImageConverter.LoadPms(otherEntry.GetFileData()))
            //            {
            //                if (alphaBitmap != null)
            //                {
            //                    bitmap.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);
            //                    bitmap.SetChannel(alphaBitmap, FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);
            //                }
            //            }
            //        }
            //    }
            //}
            //else if (extension == ".png")
            //{
            //    var ms = new MemoryStream();
            //    entry.WriteDataToStream(ms, false);
            //    try
            //    {
            //        bitmap = new FreeImageBitmap(ms, FREE_IMAGE_FORMAT.FIF_PNG);
            //    }
            //    catch
            //    {

            //    }
            //    fileExtensionHandled = true;
            //}
            ////else if (extension == ".ogg" || extension == ".aff" || extension == ".swf" || extension == ".mp3" || extension == ".wav")
            ////{
            ////    handled = true;
            ////}

            //if (fileExtensionHandled && bitmap == null)
            //{
            //    var fileData = entry.GetFileData(false);
            //    if (bitmap == null) { try { bitmap = ImageConverter.LoadVsp(fileData); } catch { } }
            //    if (bitmap == null) { try { bitmap = ImageConverter.LoadPms(fileData); } catch { } }
            //    if (bitmap == null) { try { bitmap = ImageConverter.LoadQnt(fileData); } catch { } }
            //    if (bitmap == null) { try { bitmap = ImageConverter.LoadAjp(fileData); } catch { } }
            //    if (bitmap == null)
            //    {
            //        var ms = new MemoryStream(fileData);
            //        if (bitmap == null) { try { bitmap = new FreeImageBitmap(ms, FREE_IMAGE_FORMAT.FIF_BMP); } catch { } }
            //        if (bitmap == null) { try { bitmap = new FreeImageBitmap(ms, FREE_IMAGE_FORMAT.FIF_JPEG); } catch { } }
            //        if (bitmap == null) { try { bitmap = new FreeImageBitmap(ms, FREE_IMAGE_FORMAT.FIF_PNG); } catch (Exception ex) { } }
            //        if (bitmap == null) { try { bitmap = new FreeImageBitmap(ms); } catch { } }
            //    }
            //}
            return bitmap;
        }

        public static FreeImageBitmap GetImage(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                long fsLength = fs.Length;
                var br = new BinaryReader(fs);
                var fileBytes = br.ReadBytes((int)fsLength);
                return GetImage(fileBytes, Path.GetExtension(fileName));
            }
        }

        public static FreeImageBitmap GetImage(byte[] fileBytes)
        {
            return GetImage(fileBytes, "");
        }

        public static FreeImageBitmap GetImage(byte[] fileBytes, string extension)
        {
            FreeImageBitmap bitmap = null;
            var fileStream = new MemoryStream(fileBytes);
            //&& extension == ".vsp"

            //first try just the loader for the matching file extension
            if (bitmap == null && extension == ".qnt") { try { bitmap = ImageConverter.LoadQnt(fileBytes); } catch { } }
            if (bitmap == null && extension == ".dcf") { bitmap = ImageConverter.LoadXcf(fileBytes);  }
            if (bitmap == null && extension == ".pcf") { bitmap = ImageConverter.LoadXcf(fileBytes); }
            if (bitmap == null && extension == ".png") { try { bitmap = new FreeImageBitmap(fileStream, FREE_IMAGE_FORMAT.FIF_PNG); } catch { } }
            if (bitmap == null && extension == ".pms") { try { bitmap = ImageConverter.LoadPms(fileBytes); } catch { } }
            if (bitmap == null && extension == ".ajp") { try { bitmap = ImageConverter.LoadAjp(fileBytes); } catch { } }
            if (bitmap == null && (extension == ".jpg" || extension == ".jpeg")) { try { bitmap = new FreeImageBitmap(fileStream, FREE_IMAGE_FORMAT.FIF_JPEG); } catch { } }
            if (bitmap == null && extension == ".vsp") { try { bitmap = ImageConverter.LoadVsp(fileBytes); } catch { } }
            if (bitmap == null && extension == ".bmp") { try { bitmap = new FreeImageBitmap(fileStream, FREE_IMAGE_FORMAT.FIF_BMP); } catch { } }

            //if it fails, try them all
            if (bitmap == null) { try { bitmap = ImageConverter.LoadQnt(fileBytes); } catch { } }
            if (bitmap == null) { try { bitmap = new FreeImageBitmap(fileStream, FREE_IMAGE_FORMAT.FIF_PNG); } catch { } }
            if (bitmap == null) { try { bitmap = ImageConverter.LoadPms(fileBytes); } catch { } }
            if (bitmap == null) { try { bitmap = ImageConverter.LoadAjp(fileBytes); } catch { } }
            if (bitmap == null) { try { bitmap = new FreeImageBitmap(fileStream, FREE_IMAGE_FORMAT.FIF_JPEG); } catch { } }
            if (bitmap == null) { try { bitmap = ImageConverter.LoadVsp(fileBytes); } catch { } }
            if (bitmap == null) { try { bitmap = new FreeImageBitmap(fileStream, FREE_IMAGE_FORMAT.FIF_BMP); } catch { } }
            if (bitmap == null) { try { bitmap = new FreeImageBitmap(fileStream); } catch { } }
            //if (bitmap == null) { try { bitmap = LoadSwfBitmap(fileBytes); } catch (Exception ex) { } }

            if (bitmap == null && Debugger.IsAttached)
            {
                Debugger.Break();
            }
            return bitmap;
        }

        /*private static FreeImageBitmap LoadSwfBitmap(byte[] fileBytes)
        {
            var swfTagWrapper = new TagWrapper(fileBytes);
            var tag = swfTagWrapper.Tag as DefineBitsLosslessTag;
            //if (tag.BitmapFormat == BitmapFormat.RGB24Bit && tag.HasAlpha)
            //{
            //    for (int y = 0; y < tag.Height; y++)
            //    {
            //        for (int x = 0; x < tag.Width; x++)
            //        {
            //            int i = (int)(y * tag.Width + x);
            //            var pixel = tag.BitmapData[i];
            //            int a = pixel.A;
            //            if (a > 0 && a != 255)
            //            {
            //                float amult = 255.0f / (float)a;
            //                pixel.R = (byte)((float)pixel.R * amult + 0.5f);
            //                pixel.G = (byte)((float)pixel.G * amult + 0.5f);
            //                pixel.B = (byte)((float)pixel.B * amult + 0.5f);
            //                tag.BitmapData[i] = pixel;
            //            }
            //        }
            //    }
            //}
            //else
            //{

            //}
            return new FreeImageBitmap(tag.GetBitmap());

            //var bitmap = tag.GetBitmap();
            //var bits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //unsafe
            //{
            //    byte* scan0 = (byte*)bits.Scan0;
            //    //int i = 0;
            //    for (int y = 0; y < bitmap.Height; y++)
            //    {
            //        for (int x = 0; x < bitmap.Width; x++)
            //        {
            //            int b = scan0[x * 4 + 0];
            //            int g = scan0[x * 4 + 1];
            //            int r = scan0[x * 4 + 2];
            //            int a = scan0[x * 4 + 3];
            //            if (a > 0)
            //            {

            //            }
            //        }
            //        scan0 += bits.Stride;
            //    }



            //}



            //var result = new FreeImageBitmap(bitmap.Width, bitmap.Height, bits.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, bits.Scan0);
            //bitmap.UnlockBits(bits);
            //return result;
        }*/

        private void exportAllToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ExportAll();
        }

        private void exportSelectedItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelectedFiles();
        }

        private void importAllToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ImportAll();
        }

        private void importExportToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripMenuItem;
            if (toolStripItem == null)
            {
                return;
            }
            bool enabled = (loadedAldFile != null);
            foreach (ToolStripItem item in toolStripItem.DropDownItems)
            {
                item.Enabled = enabled;
            }
            doNotConvertImageFilesToolStripMenuItem.Enabled = true;
            alwaysRemapPaletteToolStripMenuItem.Enabled = true;
            includeDirectoriesWhenExportingFilesToolStripMenuItem.Enabled = true;
        }

        private void newALDFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFile();
        }

        private void NewFile()
        {
            var desiredFileType = FileFormatSelectionForm.SelectFileType();
            if (desiredFileType == AldFileType.Invalid)
            {
                return;
            }
            loadedAldFile = new AldFileCollection();
            var aFile = loadedAldFile.GetAldFileByLetter(1);
            aFile.FileType = desiredFileType;
            aFile.AldFileName = "new";
            switch (aFile.FileType)
            {
                case AldFileType.AldFile:
                    aFile.AldFileName += "GA.ald";
                    break;
                case AldFileType.AlkFile:
                    aFile.AldFileName += ".alk";
                    break;
                case AldFileType.DatFile:
                    aFile.AldFileName = "ACG.DAT";
                    break;
                case AldFileType.AFA1File:
                case AldFileType.AFA2File:
                    aFile.AldFileName += ".afa";
                    break;
            }

            //loadedAldFile.AldFiles[0].FileType = AldFileType.AFA2File;
            LoadTreeView(false);
        }

        //private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        treeView1.SelectedNode = e.Node;
        //    }
        //}

        private void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelectedEntries();
        }

        private void ExportSelectedEntries()
        {
            ExportFiles(GetSelectedEntries());
        }

        private AldFileEntry GetSelectedEntry()
        {
            AldFileEntry selectedEntry = null;

            var selectedNode = GetSelectedNode();

            if (selectedNode != null)
            {
                selectedEntry = selectedNode.Tag as AldFileEntry;
                if (selectedEntry != null)
                {
                    return selectedEntry;
                }
                //var node = selectedNode.Tag as SubImages.Node;
                //if (node != null)
                //{
                //    var dummyEntry = GetDummyEntry(node);
                //    return dummyEntry;
                //}
            }
            return null;
        }

        //private static AldFileEntry GetDummyEntry(SubImages.Node node)
        //{
        //    var dummyEntry = new AldFileEntry();
        //    dummyEntry.Index = -1;
        //    dummyEntry.Parent = node.Parent.Parent;
        //    dummyEntry.FileAddress = node.Parent.FileAddress + node.Offset;
        //    dummyEntry.FileSize = node.Bytes.Length;
        //    dummyEntry.FileName = node.FileName;
        //    return dummyEntry;
        //}

        private ListViewItem GetSelectedNode()
        {
            var selectedNode = listView1.FocusedItem;
            if (selectedNode == null || selectedNode.Selected != true)
            {
                selectedNode = listView1.SelectedItems.OfType<ListViewItem>().FirstOrDefault();
            }
            return selectedNode;
        }

        private void replaceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReplaceFiles(GetSelectedEntries());
        }

        private void ReplaceFiles(IEnumerable<AldFileEntry> selectedEntries)
        {
            int count = selectedEntries.Count();
            if (count == 0)
            {
                return;
            }
            if (count == 1)
            {
                ReplaceFile(selectedEntries.FirstOrDefault());
                return;
            }

            //todo: better UI for replacing files
            foreach (var f in selectedEntries)
            {
                if (!ReplaceFile(f))
                {
                    break;
                }
            }
        }

        private bool ReplaceFile(AldFileEntry selectedEntry)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                string extension = Path.GetExtension(selectedEntry.FileName).ToLowerInvariant();
                string initialFileName = selectedEntry.FileName;
                if (extension == ".vsp" || extension == ".pms" || extension == ".qnt" || extension == ".bmp" || extension == ".ajp" || extension == ".dcf" || extension == ".pcf")
                {
                    openFileDialog.Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*";
                    initialFileName = Path.ChangeExtension(initialFileName, ".png");
                }
                else if (extension == ".jpg")
                {
                    openFileDialog.Filter = "JPG Files (*.jpg)|*.png|All Files (*.*)|*.*";
                }
                else
                {
                    openFileDialog.Filter = "All Files (*.*)|*.*";
                }
                openFileDialog.FileName = initialFileName;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedEntry.ReplacementFileName = openFileDialog.FileName;
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.loadedAldFile == null)
            {
                return;
            }
            var selectedNodes = GetSelectedNodeAndSubnodes();
            var selectedEntries = GetSelectedEntries();
            DeleteEntries(selectedEntries, selectedNodes);
        }

        private ListViewItem[] GetSelectedNodeAndSubnodes()
        {
            List<ListViewItem> selectedNodes = new List<ListViewItem>();
            var selectedItems = listView1.GetSelectedItems();

            foreach (ListViewItem node in selectedItems)
            {
                if (node.IsFileNode())
                {
                    selectedNodes.Add(node);
                    ListViewItem nextNode = node.GetNextNode();
                    while (nextNode != null && nextNode.IsSubfileNode())
                    {
                        selectedNodes.Add(nextNode);
                        nextNode = nextNode.GetNextNode();
                    }
                }
            }
            return selectedNodes.OrderBy(n => n.Index).ToArray();
        }

        private void DeleteEntries(IEnumerable<AldFileEntry> entriesToDelete, IEnumerable<ListViewItem> nodesToDelete)
        {
            if (entriesToDelete != null && entriesToDelete.Count() > 0)
            {
                if (MessageBox.Show(this, "Really delete files from ALD?", "ALDExplorer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    var entryPairsToDelete = GetEntryPairsToDelete(entriesToDelete);
                    foreach (var pair in entryPairsToDelete)
                    {
                        int index = pair.Key;
                        var aldFile = pair.Value;
                        if (index >= 0 && index < aldFile.FileEntries.Count)
                        {
                            aldFile.FileEntries.RemoveAt(index);
                        }
                    }

                    //foreach (var entry in entriesToDelete)
                    //{
                    //    var aldFile = loadedAldFile.GetAldFileByLetter(entry.FileLetter);
                    //    if (aldFile != null)
                    //    {
                    //        aldFile.FileEntries.Remove(entry);
                    //    }
                    //    loadedAldFile.FileEntries.Remove(entry);
                    //}
                    if (nodesToDelete != null)
                    {
                        listView1.BeginUpdate();
                        var nodesIndexesToDelete = GetNodeIndexesToDelete(nodesToDelete);
                        foreach (int i in nodesIndexesToDelete)
                        {
                            var node = listView1.Items[i];
                            var nodeEntry = node.Tag as AldFileEntry;
                            if (nodeEntry != null)
                            {
                                listView1.Items.RemoveAt(i);
                            }
                        }

                        //foreach (var node in nodesToDelete)
                        //{
                        //    var nodeEntry = node.Tag as AldFileEntry;
                        //    if (nodeEntry != null)
                        //    {
                        //        node.Remove();
                        //    }
                        //}
                        listView1.EndUpdate();
                    }
                    loadedAldFile.Refresh();
                }
            }
        }

        private KeyValuePair<int, AldFile>[] GetEntryPairsToDelete(IEnumerable<AldFileEntry> entriesToDelete)
        {
            this.loadedAldFile.UpdateIndexes();
            var entryPairsToDelete = entriesToDelete.Select(e => new KeyValuePair<int, AldFile>(e.Index, loadedAldFile.GetAldFileByLetter(e.FileLetter))).OrderByDescending(pair => pair.Key).ToArray();
            return entryPairsToDelete;
        }

        private static int[] GetNodeIndexesToDelete(IEnumerable<ListViewItem> nodesToDelete)
        {
            var nodesIndexesToDelete = nodesToDelete.Select(node => node.Index).OrderByDescending(i => i).ToArray();
            return nodesIndexesToDelete;
        }

        private void deleteCheckedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loadedAldFile == null)
            {
                return;
            }

            List<AldFileEntry> entries = GetCheckedEntries();
            var checkedNodes = listView1.CheckedItems.OfType<ListViewItem>().ToArray();

            DeleteEntries(entries, checkedNodes);
        }

        private void importNewFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportNewFiles();
        }

        private void ImportNewFiles()
        {
            if (loadedAldFile == null)
            {
                return;
            }

            var selectedEntry = GetSelectedEntry();
            int newFileLetter = 1;
            if (selectedEntry != null)
            {
                newFileLetter = selectedEntry.FileLetter;
            }
            else
            {
                var selectedNode = GetSelectedNode();
                if (selectedNode == null)
                {
                    selectedNode = listView1.Items.OfType<ListViewItem>().FirstOrDefault();
                }
                if (selectedNode != null)
                {
                    var selectedAldFile = selectedNode.Tag as AldFile;
                    if (selectedAldFile != null)
                    {
                        newFileLetter = selectedAldFile.FileLetter;
                    }
                }
            }
            var aldFile = loadedAldFile.GetAldFileByLetter(newFileLetter);

            string[] fileNames = null;
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "All Files (*.*)|*.*";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fileNames = openFileDialog.FileNames;
                }
            }
            if (fileNames == null || fileNames.Length == 0)
            {
                return;
            }

            bool anyPngFiles = AnyPngFiles(fileNames);
            bool anySwfFiles = AnySwfFiles(fileNames);
            string newExtension = "";
            string newSwfExtension = ".swf";
            if (anyPngFiles)
            {
                using (var fileTypesForm = new ImportFileTypeForm(".qnt", ".ajp", ".pms", ".vsp", ".png", ".jpg", ".bmp"))
                {
                    fileTypesForm.ShowDialog();
                    newExtension = fileTypesForm.FileType;
                }
            }
            if (anySwfFiles)
            {
                using (var fileTypesForm = new ImportFileTypeForm(".aff", ".swf"))
                {
                    fileTypesForm.ShowDialog();
                    newSwfExtension = fileTypesForm.FileType;
                }
            }

            string prefix = "";
            var fileType = this.loadedAldFile.FileType;
            if (fileType == AldFileType.AFA1File || fileType == AldFileType.AFA2File)
            {
                prefix = PrefixForm.GetPrefix(Path.GetDirectoryName(fileNames[0]));
            }

            foreach (var fileName in fileNames)
            {
                var entry = new AldFileEntry();
                entry.FileAddress = 0;
                entry.FileHeader = null;
                entry.FileName = prefix + Path.GetFileName(fileName);
                if (entry.FileName.ToLowerInvariant().EndsWith(".png"))
                {
                    entry.FileName = Path.ChangeExtension(entry.FileName, newExtension);
                }

                entry.FileNumber = GetFileNumber(entry.FileName);
                entry.FileLetter = newFileLetter;
                var fileInfo = new FileInfo(fileName);
                entry.FileSize = (int)fileInfo.Length;
                entry.HeaderAddress = 0;
                entry.Index = -1;
                entry.Parent = null;
                entry.ReplacementFileName = fileName;
                aldFile.FileEntries.Add(entry);
                //this.loadedAldFile.FileEntries.Add(entry);
            }
            loadedAldFile.Refresh();
            LoadTreeView(true);
        }

        private static bool AnyPngFiles(string[] fileNames)
        {
            bool anyPngFiles = (fileNames.Any(f => f.ToLowerInvariant().EndsWith(".png")));
            return anyPngFiles;
        }

        private static bool AnySwfFiles(string[] fileNames)
        {
            bool anySwfFiles = (fileNames.Any(f => f.ToLowerInvariant().EndsWith(".swf")));
            return anySwfFiles;
        }

        private static int GetFileNumber(string fileName)
        {
            int maxIndex = -1;
            int maxLength = 0;

            int currentIndex = -1;
            int currentLength = 0;
            //find the longest number in the filename
            for (int i = 0; i < fileName.Length; i++)
            {
                char c = fileName[i];
                if (char.IsNumber(c))
                {
                    if (currentLength == 0)
                    {
                        currentIndex = i;
                        currentLength = 1;
                    }
                    else
                    {
                        currentLength++;
                    }
                    if (currentLength > maxLength)
                    {
                        maxIndex = currentIndex;
                        maxLength = currentLength;
                    }
                }
                else
                {
                    currentIndex = -1;
                    currentLength = 0;
                }
            }

            int number = 0;
            if (maxIndex >= 0)
            {
                string substr = fileName.Substring(maxIndex, maxLength);
                if (int.TryParse(substr, out number))
                {

                }
                else
                {
                    number = 0;
                }
            }
            return number;
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var entry = GetSelectedEntry();
            if (entry != null)
            {
                using (var bitmap = GetBitmapFromFile(entry))
                {
                    if (bitmap != null)
                    {
                        using (var windowsBitmap = bitmap.ToBitmap())
                        {
                            Clipboard.SetImage(windowsBitmap);
                        }
                    }
                }
            }
        }

        private void exportTextFromSCOFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportText();
        }

        private void ExportText()
        {
            if (this.loadedAldFile == null || (this.loadedAldFile.AldFileName ?? "") == "")
            {
                return;
            }
            string ainFileName = Path.Combine(Path.GetDirectoryName(this.loadedAldFile.AldFileName), "System39.ain");
            if (!File.Exists(ainFileName))
            {
                return;
            }

            string[] ainStrings = GetAinStrings(ainFileName);
            if (ainStrings == null)
            {
                return;
            }

            foreach (var fileEntry in loadedAldFile.FileEntries)
            {
                string extension = Path.GetExtension(fileEntry.FileName).ToLowerInvariant();
                if (extension == ".sco")
                {
                    ExportText(fileEntry, ainStrings);
                }
            }
        }

        static Encoding shiftJis = Encoding.GetEncoding("shift-jis");

        private void ExportText(AldFileEntry fileEntry, string[] ainStrings)
        {
            string textDirectory = Path.Combine(Path.GetDirectoryName(this.loadedAldFile.AldFileName), "text");
            string textBaseFileName = Path.ChangeExtension(fileEntry.FileName, ".txt");
            string fileName = Path.Combine(textDirectory, textBaseFileName);

            string[] strings = GetFileStrings(fileEntry, ainStrings);
            if (strings.Length > 0)
            {
                if (!Directory.Exists(textDirectory))
                {
                    Directory.CreateDirectory(textDirectory);
                }
                File.WriteAllLines(fileName, strings, shiftJis);
            }
        }

        private string[] GetFileStrings(AldFileEntry fileEntry, string[] ainStrings)
        {
            var bytes = fileEntry.GetFileData();
            var ms = new MemoryStream(bytes);
            var br = new BinaryReader(ms);
            List<string> list = new List<string>();
            int position = -1;
            while (true)
            {
                position = bytes.IndexOf("/", position + 1);
                if (position == -1)
                {
                    break;
                }
                if (position + 5 < bytes.Length)
                {
                    if (bytes[position + 1] >= 0x7C && bytes[position + 1] <= 0x7F)
                    {
                        ms.Position = position + 2;
                        int stringIndex = br.ReadInt32();
                        if (stringIndex >= 0 && stringIndex < ainStrings.Length)
                        {
                            string str = ainStrings[stringIndex];
                            list.Add(str);
                            position = (int)ms.Position - 1;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            return list.ToArray();
        }

        private string[] GetAinStrings(string ainFileName)
        {
            var bytes = File.ReadAllBytes(ainFileName);
            for (int i = 4; i < bytes.Length; i++)
            {
                int b = bytes[i];
                bytes[i] = (byte)((b >> 6) | b << 2);
            }
            var ms = new MemoryStream(bytes);
            var br = new BinaryReader(ms);
            ms.Position = 4;
            int number4 = br.ReadInt32();
            string hel0 = br.ReadStringFixedSize(4);
            if (hel0 != "HEL0")
            {
                return null;
            }

            int addressOfFunc = bytes.IndexOf("FUNC\0\0\0\0", (int)ms.Position);
            if (addressOfFunc == -1) return null;
            int addressOfVari = bytes.IndexOf("VARI\0\0\0\0", addressOfFunc + 8);
            if (addressOfVari == -1) return null;
            int addressOfMsgi = bytes.IndexOf("MSGI\0\0\0\0", addressOfVari + 8);
            if (addressOfMsgi == -1) return null;

            ms.Position = addressOfMsgi + 8;
            int numberOfStrings = br.ReadInt32();
            List<string> list = new List<string>(numberOfStrings);
            for (int i = 0; i < numberOfStrings; i++)
            {
                string str = br.ReadStringNullTerminated();
                list.Add(str);
            }

            return list.ToArray();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (axMediaPlayer != null && !axMediaPlayer.IsDisposed)
            {
                this.Hide();
                axMediaPlayer.URL = "";
                axMediaPlayer.Dispose();
            }
            if (TempFileManager.DefaultInstanceCreated)
            {
                TempFileManager.DefaultInstance.DeleteMyTempFiles();
                TempFileManager.DefaultInstance.Destroy();
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenProperties();
        }

        private void OpenProperties()
        {
            var selectedEntry = GetSelectedEntry();
            if (selectedEntry != null)
            {
                using (var propertiesForm = new PropertiesForm())
                {
                    propertiesForm.FileEntry = selectedEntry;
                    propertiesForm.ShowDialog();
                    if (propertiesForm.Dirty)
                    {
                        this.loadedAldFile.Refresh();
                        LoadTreeView(true);
                    }
                }
            }
        }

        void WhenIdle(Action action)
        {
            EventHandler appIdleHandler = null;
            appIdleHandler = (sender, e) =>
                {
                    Application.Idle -= appIdleHandler;
                    action();
                };

            Application.Idle += new EventHandler(appIdleHandler);
        }

        bool ready = true;
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ready == true)
            {
                ready = false;
                WhenIdle(() =>
                    {
                        if (ready == false)
                        {
                            DisplayEntry(GetSelectedEntry());
                            ready = true;
                        }
                    }
                        );
            }
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            //listView1.Columns[0].Width = listView1.ClientRectangle.Width - 20;
        }

        private void saveAsWithPatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.loadedAldFile == null)
            {
                return;
            }

            var fileType = this.loadedAldFile.FileType;
            if (fileType == AldFileType.AldFile || fileType == AldFileType.DatFile)
            {
                int mFileLetter = 13;
                int zFileLetter = 26;

                var aFile = loadedAldFile.GetAldFileByLetter(1, false);
                var mFile = loadedAldFile.GetAldFileByLetter(mFileLetter, false);

                string aFileName = null;

                if (aFile != null && mFile == null)
                {
                    aFileName = aFile.AldFileName;
                    string mFileName = loadedAldFile.GetAldFileName(aFileName, mFileLetter);
                    if (!File.Exists(mFileName))
                    {
                        string aBaseName = Path.GetFileName(aFileName);
                        string mBaseName = Path.GetFileName(mFileName);
                        string zBaseName = Path.GetFileName(loadedAldFile.GetAldFileName(aFileName, zFileLetter));
                        string prompt = "The file " + aBaseName + " will be renamed to " + mBaseName + "." + "\r\n" +
                            "A new stub file named " + aBaseName + " will be created to tell the game where all the files are." + "\r\n" +
                            "New and modified files will be added to " + zBaseName + "." + "\r\n" +
                            "A user installing this patch must rename " + aBaseName + " to " + mBaseName + ", then copy over files " + aBaseName + " and " + zBaseName;
                        var dialogResult = MessageBox.Show(this, prompt, "ALDExplorer", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        if (dialogResult == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                }

                loadedAldFile.CreatePatch(mFileLetter, zFileLetter);
                LoadTreeView(true);
            }
            else if (fileType == AldFileType.AFA1File || fileType == AldFileType.AFA2File || fileType == AldFileType.AlkFile)
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = loadedAldFile.AldFileName;
                saveFileDialog.Filter = "AliceSoft Archive Files (*.ALD;*.AFA;*.ALK;*.DAT)|*.ald;*.afa;*.alk;*.dat|All Files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (saveFileDialog.FileName == loadedAldFile.AldFileName)
                    {
                        MessageBox.Show("Cannot replace original file with a patch.", "ALD Explorer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                    loadedAldFile.CreatePatch2(saveFileDialog.FileName);
                    //LoadTreeView(true);
                }
            }
        }

        private void Save2()
        {
            if (loadedAldFile == null)
            {
                return;
            }
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = loadedAldFile.AldFileName;
            saveFileDialog.Filter = "AliceSoft Archive Files (*.ALD;*.AFA;*.ALK;*.DAT)|*.ald;*.afa;*.alk;*.dat|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                loadedAldFile.SaveFile(saveFileDialog.FileName, true);
            }
        }

        private void alwaysRemapPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AldFile.AlwaysRemapImages = alwaysRemapPaletteToolStripMenuItem.Checked;
        }

        private void listView1_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            AldFileEntry entry = GetEntry(e.Item);
            if (entry != null)
            {
                e.CancelEdit = false;
            }
            else
            {
                e.CancelEdit = true;
            }
        }

        private AldFileEntry GetEntry(int i)
        {
            AldFileEntry entry = null;
            if (i >= 0 && i < this.listView1.Items.Count)
            {
                var item = this.listView1.Items[i];
                entry = item.Tag as AldFileEntry;
            }
            return entry;
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            AldFileEntry entry = GetEntry(e.Item);
            if (entry != null && !e.CancelEdit && !String.IsNullOrEmpty(e.Label))
            {
                //error checking maybe?
                entry.FileName = e.Label.Trim();
                var node = listView1.Items[e.Item];
                node.Text = entry.FileName;
            }
        }

        bool TryExpandItem(int i)
        {
            if (i < 0 || i >= listView1.Items.Count)
            {
                return false;
            }
            var selectedItem = listView1.Items[i];
            var selectedEntry = selectedItem.Tag as AldFileEntry;
            if (selectedEntry != null)
            {
                return AddSubItems(i);
            }
            return false;
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            var selectedItem = listView1.SelectedItems.OfType<ListViewItem>().FirstOrDefault();
            int selectedIndex = listView1.SelectedIndices.OfType<int>().FirstOrDefault();
            if (selectedItem != null)
            {
                if (e.KeyData == Keys.F2)
                {
                    selectedItem.BeginEdit();
                    e.Handled = true;
                }
                if (e.KeyData == Keys.Right)
                {
                    if (TryExpandItem(selectedIndex))
                    {
                        e.Handled = true;
                    }
                }
                if (e.KeyData == Keys.Left)
                {
                    if (TryRemoveSubItems(selectedIndex))
                    {
                        e.Handled = true;
                    }
                    if (selectedItem.IsSubfileNode())
                    {
                        int i = selectedIndex;
                        while (listView1.Items[i].IsSubfileNode() && i >= 0)
                        {
                            i--;
                        }
                        if (i >= 0)
                        {
                            var item = listView1.Items[i];
                            item.EnsureVisible();
                            selectedItem.Selected = false;
                            item.Selected = true;
                            item.Focused = true;
                        }
                        e.Handled = true;
                    }
                }
            }
        }

        private bool TryRemoveSubItems(int i)
        {
            if (i < 0 || i >= listView1.Items.Count)
            {
                return false;
            }
            var selectedItem = listView1.Items[i];

            if (selectedItem.IsFileNode())
            {
                if (i + 1 < listView1.Items.Count)
                {
                    var nextItem = listView1.Items[i + 1];
                    if (nextItem.IsSubfileNode())
                    {
                        return RemoveSubItems(i);
                    }
                }
            }
            return false;
        }

        private void debugCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedEntry = GetSelectedEntry();
            //if (selectedEntry != null)
            //{
            //    var subImages = new SubImages(selectedEntry);
            //}
        }

        bool AddSubItems(int i)
        {
            if (i < 0 || i >= listView1.Items.Count)
            {
                return false;
            }
            var listViewItem = listView1.Items[i];
            if (i + 1 < listView1.Items.Count)
            {
                var nextListViewItem = listView1.Items[i + 1];
                if (listViewItem.IndentCount < nextListViewItem.IndentCount)
                {
                    return false;
                }
            }


            var entry = listViewItem.Tag as AldFileEntry;
            if (entry == null)
            {
                return false;
            }
            var subImages = entry.GetSubImages();
            if (subImages == null || subImages.Length == 0)
            {
                return false;
            }

            //var subImages = new SubImages(entry);
            //var nodes = subImages.GetSubimages();
            //if (nodes == null || nodes.Length == 0)
            //{
            //    return false;
            //}

            listView1.BeginUpdate();
            foreach (var subImage in subImages)
            {
                var newItem = new ListViewItem();
                newItem.Text = subImage.FileName;
                newItem.SetSubfileIndentCount();
                newItem.ImageIndex = GetImageIndex(Path.GetExtension(newItem.Text).ToLowerInvariant());
                newItem.Tag = subImage;
                i++;
                listView1.Items.Insert(i, newItem);
            }
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.EndUpdate();
            return true;
        }

        private bool RemoveSubItems(int i)
        {
            if (i < 0 || i >= listView1.Items.Count)
            {
                return false;
            }
            i++;
            listView1.BeginUpdate();
            while (i < listView1.Items.Count && listView1.Items[i].IsSubfileNode())
            {
                listView1.Items.RemoveAt(i);
            }
            listView1.EndUpdate();
            return true;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = listView1.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                if (!TryExpandItem(item.Index))
                {
                    TryRemoveSubItems(item.Index);
                }
            }
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
            var item = e.Item;
            if (item != null)
            {
                var entry = item.Tag as AldFileEntry;
                if (item.IsFileNode() && entry != null && entry.HasSubImages())
                {
                    int i2 = e.ItemIndex + 1;
                    bool isPlus = true;
                    if (i2 >= 0 && i2 < listView1.Items.Count)
                    {
                        var item2 = listView1.Items[i2];
                        if (item2.IsSubfileNode())
                        {
                            isPlus = false;
                        }
                    }
                    int locationOfPlusIcon = e.Bounds.Left;

                    int indentSize = 0;
                    if (listView1.SmallImageList != null)
                    {
                        indentSize = listView1.SmallImageList.ImageSize.Width;
                    }
                    if (indentSize == 0)
                    {
                        return;
                    }

                    int drawX = e.Bounds.Left + (item.IndentCount - 1) * indentSize;
                    int drawY = e.Bounds.Top;

                    if (isPlus)
                    {
                        e.Graphics.DrawImageUnscaled(Properties.Resources.plusicon, drawX, drawY);
                    }
                    else
                    {
                        e.Graphics.DrawImageUnscaled(Properties.Resources.minusicon, drawX, drawY);
                    }
                }
            }
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var item0 = listView1.GetItemAt(e.X, e.Y);
                var item = listView1.GetItemAt(e.X + 16, e.Y);
                if (item != null && item0 == null)
                {
                    if (!TryExpandItem(item.Index))
                    {
                        TryRemoveSubItems(item.Index);
                    }
                }
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void cmenuPNG2_internal(string extension)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PNG Image (*.PNG)|*.png|All Files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ConvertFile(openFileDialog.FileName, extension);
                }
            }
        }

        private void cmenuPNG2AJP_Click(object sender, EventArgs e)
        {
            cmenuPNG2_internal(".ajp");
        }

        private void cmenuPNG2QNT_Click(object sender, EventArgs e)
        {
            cmenuPNG2_internal(".qnt");
        }

        private void cmenuPNG2PNS_Click(object sender, EventArgs e)
        {
            cmenuPNG2_internal(".pns");
        }

        private void cmenuANY2PNG_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "AliceSoft Image (*.PNS,*.QNT,*.AJP)|*.ajp;*.qnt;*.pns;*.jpeg|All Files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var ext = Path.GetExtension(openFileDialog.FileName);
                    var extBIG = ext.Substring(1).ToUpper();
                    if (extBIG != "AJP" && extBIG != "PNS" && extBIG != "QNT" && extBIG != "JPG" && extBIG != "JPEG") return;
                    ConvertFile(openFileDialog.FileName, ".png");
                }
            }
        }
    }

    public static class ListViewItemExtensions
    {
        public static void SetRootIndentCount(this ListViewItem item)
        {
            item.IndentCount = 0;
        }
        public static void SetFileIndentCount(this ListViewItem item)
        {
            item.IndentCount = 1;
        }
        public static void SetSubfileIndentCount(this ListViewItem item)
        {
            item.IndentCount = 2;
        }
        public static bool IsRootNode(this ListViewItem item)
        {
            return item.IndentCount == 0;
        }
        public static bool IsFileNode(this ListViewItem item)
        {
            return item.IndentCount == 1;
        }
        public static bool IsSubfileNode(this ListViewItem item)
        {
            return item.IndentCount == 2;
        }
        //public static int FindListViewItem(this ListView.ListViewItemCollection items, string text)
        //{

        //}

        public static ListViewItem GetNextNode(this ListViewItem item)
        {
            if (item == null) return null;
            int index = item.Index;
            var parent = item.ListView;

            //if (index < 0 || index >= parent.Items.Count)
            //{
            //    return null;
            //}

            //var item2 = parent.Items[index];
            //if (item2 != item)
            //{
            //    //if index was incorrect, find the real index
            //    int i;
            //    for (i = 0; i < parent.Items.Count; i++)
            //    {
            //        var itemInList = parent.Items[i];
            //        if (itemInList.Text == item.Text)
            //        {
            //            index = i;
            //            item = itemInList;
            //            break;
            //        }
            //    }
            //    if (i == parent.Items.Count)
            //    {
            //        item = null;
            //        return null;
            //    }
            //}

            int nextIndex = index + 1;
            if (nextIndex < 0 || nextIndex >= parent.Items.Count)
            {
                return null;
            }
            return parent.Items[nextIndex];
        }


        public static bool NextNodeIsSubfileNode(this ListViewItem item)
        {
            var nextNode = item.GetNextNode();
            if (nextNode == null)
            {
                return false;
            }
            return nextNode.IsSubfileNode();
        }

        public static ListViewItem[] GetSelectedItems(this ListView listView)
        {
            int[] selectedIndices = listView.SelectedIndices.OfType<int>().OrderBy(i => i).ToArray();
            return selectedIndices.Select(i => listView.Items[i]).ToArray();
        }
    }
}
