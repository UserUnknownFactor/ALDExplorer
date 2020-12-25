namespace ALDExplorer
{
    public class ExListView : System.Windows.Forms.ListView {
      private bool mCreating;
      private bool mReadOnly;
      protected override void OnHandleCreated(System.EventArgs e) {
        mCreating = true;
        base.OnHandleCreated(e);
        mCreating = false;
      }
      public bool ReadOnly {
        get { return mReadOnly; }
        set { mReadOnly = value; }
      }
      protected override void OnItemCheck(System.Windows.Forms.ItemCheckEventArgs e) {
        if (!mCreating && mReadOnly) e.NewValue = e.CurrentValue;
        base.OnItemCheck(e);
      }
    }

    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listView1 = new ALDExplorer.ExListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.searchBox = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsWithPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.newALDFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportSelectedItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.importAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.importNewFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.exportTextFromSCOFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.doNotConvertImageFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysRemapPaletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.includeDirectoriesWhenExportingFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fitImagesToWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmenuPNG2AJP = new System.Windows.Forms.ToolStripMenuItem();
            this.cmenuPNG2QNT = new System.Windows.Forms.ToolStripMenuItem();
            this.cmenuPNG2PNS = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cmenuQNT2PNG = new System.Windows.Forms.ToolStripMenuItem();
            this.cmenuAJP2PNG = new System.Windows.Forms.ToolStripMenuItem();
            this.cmenuPNS2PNG = new System.Windows.Forms.ToolStripMenuItem();
            this.cmenuFLAT2PNG = new System.Windows.Forms.ToolStripMenuItem();
            this.debugCommandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.loadProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStatusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.backgroundOpener = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pictureBox1);
            this.splitContainer1.Size = new System.Drawing.Size(940, 421);
            this.splitContainer1.SplitterDistance = 311;
            this.splitContainer1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listView1);
            this.panel1.Controls.Add(this.searchBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(311, 421);
            this.panel1.TabIndex = 0;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.HideSelection = false;
            this.listView1.LabelEdit = true;
            this.listView1.Location = new System.Drawing.Point(0, 20);
            this.listView1.Name = "listView1";
            this.listView1.OwnerDraw = true;
            this.listView1.ReadOnly = false;
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new System.Drawing.Size(311, 407);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listView1_AfterLabelEdit);
            this.listView1.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listView1_BeforeLabelEdit);
            this.listView1.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView1_DrawItem);
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView1_KeyDown);
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDown);
            this.listView1.Resize += new System.EventHandler(this.listView1_Resize);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportFileToolStripMenuItem,
            this.replaceFileToolStripMenuItem,
            this.deleteFileToolStripMenuItem,
            this.propertiesToolStripMenuItem,
            this.toolStripMenuItem5,
            this.copyToClipboardToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 120);
            // 
            // exportFileToolStripMenuItem
            // 
            this.exportFileToolStripMenuItem.Name = "exportFileToolStripMenuItem";
            this.exportFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exportFileToolStripMenuItem.Text = "&Export files...";
            this.exportFileToolStripMenuItem.Click += new System.EventHandler(this.exportFileToolStripMenuItem_Click);
            // 
            // replaceFileToolStripMenuItem
            // 
            this.replaceFileToolStripMenuItem.Name = "replaceFileToolStripMenuItem";
            this.replaceFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.replaceFileToolStripMenuItem.Text = "&Replace files...";
            this.replaceFileToolStripMenuItem.Click += new System.EventHandler(this.replaceFileToolStripMenuItem_Click);
            // 
            // deleteFileToolStripMenuItem
            // 
            this.deleteFileToolStripMenuItem.Name = "deleteFileToolStripMenuItem";
            this.deleteFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteFileToolStripMenuItem.Text = "&Delete Selected Files";
            this.deleteFileToolStripMenuItem.Click += new System.EventHandler(this.deleteFileToolStripMenuItem_Click);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.propertiesToolStripMenuItem.Text = "&Properties...";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(177, 6);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyToClipboardToolStripMenuItem.Text = "&Copy to Clipboard";
            this.copyToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyToClipboardToolStripMenuItem_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "generic_icon.png");
            this.imageList1.Images.SetKeyName(1, "folder.png");
            this.imageList1.Images.SetKeyName(2, "flash_image_icon.png");
            this.imageList1.Images.SetKeyName(3, "flash_sound_icon.png");
            this.imageList1.Images.SetKeyName(4, "script.png");
            this.imageList1.Images.SetKeyName(5, "music.png");
            this.imageList1.Images.SetKeyName(6, "flash_icon.png");
            this.imageList1.Images.SetKeyName(7, "alicefile2.png");
            // 
            // searchBox
            // 
            this.searchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchBox.ForeColor = System.Drawing.Color.Gray;
            this.searchBox.Location = new System.Drawing.Point(0, 0);
            this.searchBox.MaxLength = 1024;
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(311, 20);
            this.searchBox.TabIndex = 2;
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            this.searchBox.Enter += new System.EventHandler(this.searchBox_Enter);
            this.searchBox.Leave += new System.EventHandler(this.searchBox_Leave);
            // 
            // pictureBox1
            // 
            this.pictureBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(625, 421);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.importExportToolStripMenuItem,
            this.convertToolStripMenuItem,
            this.debugCommandToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(940, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.saveAsWithPatchToolStripMenuItem,
            this.toolStripMenuItem3,
            this.newALDFileToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.fileToolStripMenuItem_DropDownOpening);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.saveAsToolStripMenuItem.Text = "Save &As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // saveAsWithPatchToolStripMenuItem
            // 
            this.saveAsWithPatchToolStripMenuItem.Enabled = false;
            this.saveAsWithPatchToolStripMenuItem.Name = "saveAsWithPatchToolStripMenuItem";
            this.saveAsWithPatchToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.saveAsWithPatchToolStripMenuItem.Text = "Save As &Patch...";
            this.saveAsWithPatchToolStripMenuItem.Click += new System.EventHandler(this.saveAsWithPatchToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(216, 6);
            // 
            // newALDFileToolStripMenuItem
            // 
            this.newALDFileToolStripMenuItem.Name = "newALDFileToolStripMenuItem";
            this.newALDFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newALDFileToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.newALDFileToolStripMenuItem.Text = "&New empty archive";
            this.newALDFileToolStripMenuItem.Click += new System.EventHandler(this.newALDFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(216, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // importExportToolStripMenuItem
            // 
            this.importExportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportAllToolStripMenuItem,
            this.exportSelectedItemsToolStripMenuItem,
            this.toolStripMenuItem6,
            this.importAllToolStripMenuItem,
            this.toolStripMenuItem2,
            this.importNewFilesToolStripMenuItem,
            this.toolStripMenuItem4,
            this.exportTextFromSCOFilesToolStripMenuItem,
            this.toolStripMenuItem1,
            this.doNotConvertImageFilesToolStripMenuItem,
            this.alwaysRemapPaletteToolStripMenuItem,
            this.includeDirectoriesWhenExportingFilesToolStripMenuItem,
            this.fitImagesToWindowToolStripMenuItem});
            this.importExportToolStripMenuItem.Name = "importExportToolStripMenuItem";
            this.importExportToolStripMenuItem.Size = new System.Drawing.Size(93, 20);
            this.importExportToolStripMenuItem.Text = "&Import/Export";
            this.importExportToolStripMenuItem.DropDownOpening += new System.EventHandler(this.importExportToolStripMenuItem_DropDownOpening);
            // 
            // exportAllToolStripMenuItem
            // 
            this.exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            this.exportAllToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.exportAllToolStripMenuItem.Text = "&Export All...";
            this.exportAllToolStripMenuItem.Click += new System.EventHandler(this.exportAllToolStripMenuItem_Click_1);
            // 
            // exportSelectedItemsToolStripMenuItem
            // 
            this.exportSelectedItemsToolStripMenuItem.Name = "exportSelectedItemsToolStripMenuItem";
            this.exportSelectedItemsToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.exportSelectedItemsToolStripMenuItem.Text = "Export Selected Items...";
            this.exportSelectedItemsToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedItemsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(277, 6);
            // 
            // importAllToolStripMenuItem
            // 
            this.importAllToolStripMenuItem.Name = "importAllToolStripMenuItem";
            this.importAllToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.importAllToolStripMenuItem.Text = "&Import All...";
            this.importAllToolStripMenuItem.Click += new System.EventHandler(this.importAllToolStripMenuItem_Click_1);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(277, 6);
            // 
            // importNewFilesToolStripMenuItem
            // 
            this.importNewFilesToolStripMenuItem.Name = "importNewFilesToolStripMenuItem";
            this.importNewFilesToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.importNewFilesToolStripMenuItem.Text = "Import and &add new files...";
            this.importNewFilesToolStripMenuItem.Click += new System.EventHandler(this.importNewFilesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(277, 6);
            // 
            // exportTextFromSCOFilesToolStripMenuItem
            // 
            this.exportTextFromSCOFilesToolStripMenuItem.Name = "exportTextFromSCOFilesToolStripMenuItem";
            this.exportTextFromSCOFilesToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.exportTextFromSCOFilesToolStripMenuItem.Text = "Export &Text from SCO files...";
            this.exportTextFromSCOFilesToolStripMenuItem.Click += new System.EventHandler(this.exportTextFromSCOFilesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(277, 6);
            // 
            // doNotConvertImageFilesToolStripMenuItem
            // 
            this.doNotConvertImageFilesToolStripMenuItem.CheckOnClick = true;
            this.doNotConvertImageFilesToolStripMenuItem.Name = "doNotConvertImageFilesToolStripMenuItem";
            this.doNotConvertImageFilesToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.doNotConvertImageFilesToolStripMenuItem.Text = "&Do not convert image files";
            // 
            // alwaysRemapPaletteToolStripMenuItem
            // 
            this.alwaysRemapPaletteToolStripMenuItem.CheckOnClick = true;
            this.alwaysRemapPaletteToolStripMenuItem.Name = "alwaysRemapPaletteToolStripMenuItem";
            this.alwaysRemapPaletteToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.alwaysRemapPaletteToolStripMenuItem.Text = "Always Remap Palette";
            this.alwaysRemapPaletteToolStripMenuItem.Click += new System.EventHandler(this.alwaysRemapPaletteToolStripMenuItem_Click);
            // 
            // includeDirectoriesWhenExportingFilesToolStripMenuItem
            // 
            this.includeDirectoriesWhenExportingFilesToolStripMenuItem.Checked = true;
            this.includeDirectoriesWhenExportingFilesToolStripMenuItem.CheckOnClick = true;
            this.includeDirectoriesWhenExportingFilesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.includeDirectoriesWhenExportingFilesToolStripMenuItem.Name = "includeDirectoriesWhenExportingFilesToolStripMenuItem";
            this.includeDirectoriesWhenExportingFilesToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.includeDirectoriesWhenExportingFilesToolStripMenuItem.Text = "Include directories when exporting files";
            // 
            // fitImagesToWindowToolStripMenuItem
            // 
            this.fitImagesToWindowToolStripMenuItem.Checked = true;
            this.fitImagesToWindowToolStripMenuItem.CheckOnClick = true;
            this.fitImagesToWindowToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fitImagesToWindowToolStripMenuItem.Name = "fitImagesToWindowToolStripMenuItem";
            this.fitImagesToWindowToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.fitImagesToWindowToolStripMenuItem.Text = "&Fit images to window";
            // 
            // convertToolStripMenuItem
            // 
            this.convertToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmenuPNG2AJP,
            this.cmenuPNG2QNT,
            this.cmenuPNG2PNS,
            this.toolStripSeparator2,
            this.cmenuQNT2PNG,
            this.cmenuAJP2PNG,
            this.cmenuPNS2PNG,
            this.cmenuFLAT2PNG});
            this.convertToolStripMenuItem.Name = "convertToolStripMenuItem";
            this.convertToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.convertToolStripMenuItem.Text = "&Convert";
            // 
            // cmenuPNG2AJP
            // 
            this.cmenuPNG2AJP.Name = "cmenuPNG2AJP";
            this.cmenuPNG2AJP.Size = new System.Drawing.Size(144, 22);
            this.cmenuPNG2AJP.Text = "PNG -> AJP";
            this.cmenuPNG2AJP.Click += new System.EventHandler(this.cmenuPNG2AJP_Click);
            // 
            // cmenuPNG2QNT
            // 
            this.cmenuPNG2QNT.Name = "cmenuPNG2QNT";
            this.cmenuPNG2QNT.Size = new System.Drawing.Size(144, 22);
            this.cmenuPNG2QNT.Text = "PNG -> QNT";
            this.cmenuPNG2QNT.Click += new System.EventHandler(this.cmenuPNG2QNT_Click);
            // 
            // cmenuPNG2PNS
            // 
            this.cmenuPNG2PNS.Name = "cmenuPNG2PNS";
            this.cmenuPNG2PNS.Size = new System.Drawing.Size(144, 22);
            this.cmenuPNG2PNS.Text = "PNG -> PMS";
            this.cmenuPNG2PNS.Click += new System.EventHandler(this.cmenuPNG2PNS_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(141, 6);
            // 
            // cmenuQNT2PNG
            // 
            this.cmenuQNT2PNG.Name = "cmenuQNT2PNG";
            this.cmenuQNT2PNG.Size = new System.Drawing.Size(144, 22);
            this.cmenuQNT2PNG.Text = "QNT -> PNG";
            this.cmenuQNT2PNG.Click += new System.EventHandler(this.cmenuANY2PNG_Click);
            // 
            // cmenuAJP2PNG
            // 
            this.cmenuAJP2PNG.Name = "cmenuAJP2PNG";
            this.cmenuAJP2PNG.Size = new System.Drawing.Size(144, 22);
            this.cmenuAJP2PNG.Text = "AJP -> PNG";
            this.cmenuAJP2PNG.Click += new System.EventHandler(this.cmenuANY2PNG_Click);
            // 
            // cmenuPNS2PNG
            // 
            this.cmenuPNS2PNG.Name = "cmenuPNS2PNG";
            this.cmenuPNS2PNG.Size = new System.Drawing.Size(144, 22);
            this.cmenuPNS2PNG.Text = "PNS -> PNG";
            this.cmenuPNS2PNG.Click += new System.EventHandler(this.cmenuANY2PNG_Click);
            // 
            // cmenuFLAT2PNG
            // 
            this.cmenuFLAT2PNG.Name = "cmenuFLAT2PNG";
            this.cmenuFLAT2PNG.Size = new System.Drawing.Size(144, 22);
            this.cmenuFLAT2PNG.Text = "FLAT -> PNG";
            this.cmenuFLAT2PNG.Click += new System.EventHandler(this.cmenuFLAT2PNG_Click);
            // 
            // debugCommandToolStripMenuItem
            // 
            this.debugCommandToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.debugCommandToolStripMenuItem.Name = "debugCommandToolStripMenuItem";
            this.debugCommandToolStripMenuItem.Size = new System.Drawing.Size(95, 20);
            this.debugCommandToolStripMenuItem.Text = "&Debug Trigger";
            this.debugCommandToolStripMenuItem.Click += new System.EventHandler(this.debugCommandToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadProgress,
            this.toolStatusText});
            this.statusStrip.Location = new System.Drawing.Point(0, 423);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(940, 22);
            this.statusStrip.TabIndex = 2;
            // 
            // loadProgress
            // 
            this.loadProgress.Name = "loadProgress";
            this.loadProgress.Size = new System.Drawing.Size(309, 16);
            this.loadProgress.Visible = false;
            // 
            // toolStatusText
            // 
            this.toolStatusText.Name = "toolStatusText";
            this.toolStatusText.Size = new System.Drawing.Size(48, 17);
            this.toolStatusText.Text = "Ready...";
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(61, 4);
            // 
            // backgroundOpener
            // 
            this.backgroundOpener.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundOpener_DoWork);
            this.backgroundOpener.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundOpener_ProgressChanged);
            this.backgroundOpener.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundOpener_RunWorkerCompleted);
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(940, 445);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "ALD Explorer v3.0a";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SizeChanged += new System.EventHandler(this.Form1_sizeChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.Form1_DragOver);
            this.DragLeave += new System.EventHandler(this.Form1_DragLeave);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem importExportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedItemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem importNewFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem newALDFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem doNotConvertImageFilesToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exportFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportTextFromSCOFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private ExListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem saveAsWithPatchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alwaysRemapPaletteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem includeDirectoriesWhenExportingFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugCommandToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cmenuPNG2AJP;
        private System.Windows.Forms.ToolStripMenuItem cmenuPNG2QNT;
        private System.Windows.Forms.ToolStripMenuItem cmenuQNT2PNG;
        private System.Windows.Forms.ToolStripMenuItem cmenuAJP2PNG;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar loadProgress;
        private System.Windows.Forms.ToolStripStatusLabel toolStatusText;
        private System.Windows.Forms.ToolStripMenuItem cmenuPNG2PNS;
        private System.Windows.Forms.ToolStripMenuItem cmenuPNS2PNG;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem cmenuFLAT2PNG;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripMenuItem fitImagesToWindowToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundOpener;
    }
}

