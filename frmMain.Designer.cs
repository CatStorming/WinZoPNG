namespace WinZoPNG
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
      StatusStrip1 = new StatusStrip();
      stLabel = new ToolStripStatusLabel();
      OpenFileDlg = new OpenFileDialog();
      FolderBrowserDlg = new FolderBrowserDialog();
      TimProgress = new System.Windows.Forms.Timer(components);
      contextMenuStrip1 = new ContextMenuStrip(components);
      TmSelectAll = new ToolStripMenuItem();
      TmInvertSelection = new ToolStripMenuItem();
      toolStripMenuItem1 = new ToolStripSeparator();
      TmCopyFilename = new ToolStripMenuItem();
      TmCopyFull = new ToolStripMenuItem();
      toolStripMenuItem2 = new ToolStripSeparator();
      TmRemove = new ToolStripMenuItem();
      LvTarget = new BufferedListView();
      ChFilename = new ColumnHeader();
      ChSizeBefore = new ColumnHeader();
      ChSizeAfter = new ColumnHeader();
      ChReduction = new ColumnHeader();
      ChProgress = new ColumnHeader();
      panel1 = new Panel();
      ChkKeepTimestamp = new CheckBox();
      CmbOptLv = new ComboBox();
      LblOptLevel = new Label();
      CmbPriority = new ComboBox();
      LblPriority = new Label();
      NumThreads = new NumericUpDown();
      LblThread = new Label();
      BtnQuit = new Button();
      BtnAbout = new Button();
      BtnRm = new Button();
      BtnAddDir = new Button();
      BtnAddFiles = new Button();
      BtnCancel = new Button();
      BtnExec = new Button();
      StatusStrip1.SuspendLayout();
      contextMenuStrip1.SuspendLayout();
      panel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)NumThreads).BeginInit();
      SuspendLayout();
      // 
      // StatusStrip1
      // 
      resources.ApplyResources(StatusStrip1, "StatusStrip1");
      StatusStrip1.ImageScalingSize = new Size(32, 32);
      StatusStrip1.Items.AddRange(new ToolStripItem[] { stLabel });
      StatusStrip1.Name = "StatusStrip1";
      StatusStrip1.ShowItemToolTips = true;
      StatusStrip1.DoubleClick += StLabel_DoubleClick;
      // 
      // stLabel
      // 
      stLabel.AutoToolTip = true;
      stLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
      stLabel.DoubleClickEnabled = true;
      resources.ApplyResources(stLabel, "stLabel");
      stLabel.Name = "stLabel";
      stLabel.Overflow = ToolStripItemOverflow.Never;
      stLabel.DoubleClick += StLabel_DoubleClick;
      // 
      // OpenFileDlg
      // 
      OpenFileDlg.DefaultExt = "*.png";
      OpenFileDlg.FileName = "*.png";
      resources.ApplyResources(OpenFileDlg, "OpenFileDlg");
      OpenFileDlg.Multiselect = true;
      OpenFileDlg.RestoreDirectory = true;
      // 
      // FolderBrowserDlg
      // 
      resources.ApplyResources(FolderBrowserDlg, "FolderBrowserDlg");
      FolderBrowserDlg.ShowNewFolderButton = false;
      // 
      // TimProgress
      // 
      TimProgress.Interval = 1000;
      TimProgress.Tick += TimProgress_Tick;
      // 
      // contextMenuStrip1
      // 
      contextMenuStrip1.ImageScalingSize = new Size(32, 32);
      contextMenuStrip1.Items.AddRange(new ToolStripItem[] { TmSelectAll, TmInvertSelection, toolStripMenuItem1, TmCopyFilename, TmCopyFull, toolStripMenuItem2, TmRemove });
      contextMenuStrip1.Name = "contextMenuStrip1";
      resources.ApplyResources(contextMenuStrip1, "contextMenuStrip1");
      // 
      // TmSelectAll
      // 
      TmSelectAll.Name = "TmSelectAll";
      resources.ApplyResources(TmSelectAll, "TmSelectAll");
      TmSelectAll.Click += TmSelectAll_Click;
      // 
      // TmInvertSelection
      // 
      TmInvertSelection.Name = "TmInvertSelection";
      resources.ApplyResources(TmInvertSelection, "TmInvertSelection");
      TmInvertSelection.Click += TmInvertSelection_Click;
      // 
      // toolStripMenuItem1
      // 
      toolStripMenuItem1.Name = "toolStripMenuItem1";
      resources.ApplyResources(toolStripMenuItem1, "toolStripMenuItem1");
      // 
      // TmCopyFilename
      // 
      TmCopyFilename.Name = "TmCopyFilename";
      resources.ApplyResources(TmCopyFilename, "TmCopyFilename");
      TmCopyFilename.Click += TmCopyFilename_Click;
      // 
      // TmCopyFull
      // 
      TmCopyFull.Name = "TmCopyFull";
      resources.ApplyResources(TmCopyFull, "TmCopyFull");
      TmCopyFull.Click += TmCopyFull_Click;
      // 
      // toolStripMenuItem2
      // 
      toolStripMenuItem2.Name = "toolStripMenuItem2";
      resources.ApplyResources(toolStripMenuItem2, "toolStripMenuItem2");
      // 
      // TmRemove
      // 
      TmRemove.Name = "TmRemove";
      resources.ApplyResources(TmRemove, "TmRemove");
      TmRemove.Click += BtnRm_Click;
      // 
      // LvTarget
      // 
      resources.ApplyResources(LvTarget, "LvTarget");
      LvTarget.Columns.AddRange(new ColumnHeader[] { ChFilename, ChSizeBefore, ChSizeAfter, ChReduction, ChProgress });
      LvTarget.FullRowSelect = true;
      LvTarget.Name = "LvTarget";
      LvTarget.UseCompatibleStateImageBehavior = false;
      LvTarget.View = View.Details;
      LvTarget.VirtualMode = true;
      LvTarget.ColumnClick += LvTarget_ColumnClick;
      LvTarget.RetrieveVirtualItem += LvTarget_RetrieveVirtualItem;
      LvTarget.DragDrop += FrmMain_DragDrop;
      LvTarget.DragEnter += FrmMain_DragEnter;
      LvTarget.KeyDown += LvTarget_KeyDown;
      LvTarget.MouseUp += LvTarget_MouseUp;
      // 
      // ChFilename
      // 
      resources.ApplyResources(ChFilename, "ChFilename");
      // 
      // ChSizeBefore
      // 
      resources.ApplyResources(ChSizeBefore, "ChSizeBefore");
      // 
      // ChSizeAfter
      // 
      resources.ApplyResources(ChSizeAfter, "ChSizeAfter");
      // 
      // ChReduction
      // 
      resources.ApplyResources(ChReduction, "ChReduction");
      // 
      // ChProgress
      // 
      resources.ApplyResources(ChProgress, "ChProgress");
      // 
      // panel1
      // 
      resources.ApplyResources(panel1, "panel1");
      panel1.Controls.Add(ChkKeepTimestamp);
      panel1.Controls.Add(CmbOptLv);
      panel1.Controls.Add(LblOptLevel);
      panel1.Controls.Add(CmbPriority);
      panel1.Controls.Add(LblPriority);
      panel1.Controls.Add(NumThreads);
      panel1.Controls.Add(LblThread);
      panel1.Controls.Add(BtnQuit);
      panel1.Controls.Add(BtnAbout);
      panel1.Controls.Add(BtnRm);
      panel1.Controls.Add(BtnAddDir);
      panel1.Controls.Add(BtnAddFiles);
      panel1.Controls.Add(BtnCancel);
      panel1.Controls.Add(BtnExec);
      panel1.Name = "panel1";
      // 
      // ChkKeepTimestamp
      // 
      resources.ApplyResources(ChkKeepTimestamp, "ChkKeepTimestamp");
      ChkKeepTimestamp.Name = "ChkKeepTimestamp";
      ChkKeepTimestamp.UseVisualStyleBackColor = true;
      // 
      // CmbOptLv
      // 
      CmbOptLv.DropDownStyle = ComboBoxStyle.DropDownList;
      CmbOptLv.DropDownWidth = 300;
      CmbOptLv.Items.AddRange(new object[] { resources.GetString("CmbOptLv.Items"), resources.GetString("CmbOptLv.Items1"), resources.GetString("CmbOptLv.Items2"), resources.GetString("CmbOptLv.Items3") });
      resources.ApplyResources(CmbOptLv, "CmbOptLv");
      CmbOptLv.Name = "CmbOptLv";
      // 
      // LblOptLevel
      // 
      resources.ApplyResources(LblOptLevel, "LblOptLevel");
      LblOptLevel.Name = "LblOptLevel";
      // 
      // CmbPriority
      // 
      CmbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
      CmbPriority.DropDownWidth = 300;
      CmbPriority.FormattingEnabled = true;
      CmbPriority.Items.AddRange(new object[] { resources.GetString("CmbPriority.Items"), resources.GetString("CmbPriority.Items1"), resources.GetString("CmbPriority.Items2"), resources.GetString("CmbPriority.Items3"), resources.GetString("CmbPriority.Items4") });
      resources.ApplyResources(CmbPriority, "CmbPriority");
      CmbPriority.Name = "CmbPriority";
      // 
      // LblPriority
      // 
      resources.ApplyResources(LblPriority, "LblPriority");
      LblPriority.Name = "LblPriority";
      // 
      // NumThreads
      // 
      resources.ApplyResources(NumThreads, "NumThreads");
      NumThreads.Maximum = new decimal(new int[] { 4, 0, 0, 0 });
      NumThreads.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
      NumThreads.Name = "NumThreads";
      NumThreads.Value = new decimal(new int[] { 1, 0, 0, 0 });
      NumThreads.ValueChanged += NumThreads_ValueChanged;
      // 
      // LblThread
      // 
      resources.ApplyResources(LblThread, "LblThread");
      LblThread.Name = "LblThread";
      // 
      // BtnQuit
      // 
      resources.ApplyResources(BtnQuit, "BtnQuit");
      BtnQuit.Name = "BtnQuit";
      BtnQuit.UseVisualStyleBackColor = true;
      BtnQuit.Click += BtnQuit_Click;
      // 
      // BtnAbout
      // 
      resources.ApplyResources(BtnAbout, "BtnAbout");
      BtnAbout.Name = "BtnAbout";
      BtnAbout.UseVisualStyleBackColor = true;
      BtnAbout.Click += BtnAbout_Click;
      // 
      // BtnRm
      // 
      resources.ApplyResources(BtnRm, "BtnRm");
      BtnRm.Name = "BtnRm";
      BtnRm.UseVisualStyleBackColor = true;
      BtnRm.Click += BtnRm_Click;
      // 
      // BtnAddDir
      // 
      resources.ApplyResources(BtnAddDir, "BtnAddDir");
      BtnAddDir.Name = "BtnAddDir";
      BtnAddDir.UseVisualStyleBackColor = true;
      BtnAddDir.Click += BtnAddDir_Click;
      // 
      // BtnAddFiles
      // 
      resources.ApplyResources(BtnAddFiles, "BtnAddFiles");
      BtnAddFiles.Name = "BtnAddFiles";
      BtnAddFiles.UseVisualStyleBackColor = true;
      BtnAddFiles.Click += BtnAddFiles_Click;
      // 
      // BtnCancel
      // 
      resources.ApplyResources(BtnCancel, "BtnCancel");
      BtnCancel.Name = "BtnCancel";
      BtnCancel.UseVisualStyleBackColor = true;
      BtnCancel.Click += BtnCancel_Click;
      // 
      // BtnExec
      // 
      resources.ApplyResources(BtnExec, "BtnExec");
      BtnExec.Name = "BtnExec";
      BtnExec.UseVisualStyleBackColor = true;
      BtnExec.Click += BtnExec_Click;
      // 
      // FrmMain
      // 
      AllowDrop = true;
      resources.ApplyResources(this, "$this");
      AutoScaleMode = AutoScaleMode.Font;
      Controls.Add(panel1);
      Controls.Add(LvTarget);
      Controls.Add(StatusStrip1);
      DoubleBuffered = true;
      Name = "FrmMain";
      FormClosing += FrmMain_FormClosing;
      FormClosed += FrmMain_FormClosed;
      DragDrop += FrmMain_DragDrop;
      DragEnter += FrmMain_DragEnter;
      StatusStrip1.ResumeLayout(false);
      StatusStrip1.PerformLayout();
      contextMenuStrip1.ResumeLayout(false);
      panel1.ResumeLayout(false);
      panel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)NumThreads).EndInit();
      ResumeLayout(false);
    }

    #endregion

    private StatusStrip StatusStrip1;
        private OpenFileDialog OpenFileDlg;
        private FolderBrowserDialog FolderBrowserDlg;
    private System.Windows.Forms.Timer TimProgress;
    private ContextMenuStrip contextMenuStrip1;
    private ToolStripMenuItem TmCopyFilename;
    private ToolStripMenuItem TmCopyFull;
    private ToolStripMenuItem TmSelectAll;
    private ToolStripMenuItem TmInvertSelection;
    private ToolStripMenuItem TmRemove;
    private ToolStripSeparator toolStripMenuItem1;
    private ToolStripSeparator toolStripMenuItem2;
        private BufferedListView LvTarget;
        private ColumnHeader ChFilename;
        private ColumnHeader ChSizeBefore;
        private ColumnHeader ChSizeAfter;
        private ColumnHeader ChReduction;
        private ColumnHeader ChProgress;
        private Panel panel1;
        private Button BtnExec;
        private ComboBox CmbOptLv;
        private Label LblOptLevel;
        private ComboBox CmbPriority;
        private Label LblPriority;
        private NumericUpDown NumThreads;
        private Label LblThread;
        private Button BtnQuit;
        private Button BtnAbout;
        private Button BtnRm;
        private Button BtnAddDir;
        private Button BtnAddFiles;
        private Button BtnCancel;
        private ToolStripStatusLabel stLabel;
    private CheckBox ChkKeepTimestamp;
  }
}