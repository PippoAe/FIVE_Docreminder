namespace docreminder
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.dateiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ScheduleStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EvaluatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bGetDocumentsDocuments = new System.Windows.Forms.Button();
            this.bProcessDocuments = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusConfig = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusPlatzHalter = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusCountdown = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnSearchMore = new System.Windows.Forms.Button();
            this.timerShutDown = new System.Windows.Forms.Timer(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBoxDocuments = new System.Windows.Forms.GroupBox();
            this.dgwDocuments = new System.Windows.Forms.DataGridView();
            this.GetDocumentsWorker = new System.ComponentModel.BackgroundWorker();
            this.ProcessDocumentsWorker = new System.ComponentModel.BackgroundWorker();
            this.cBShowConsole = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBoxDocuments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwDocuments)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dateiToolStripMenuItem,
            this.EvaluatorToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(664, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // dateiToolStripMenuItem
            // 
            this.dateiToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.ScheduleStripMenuItem,
            this.aboutToolStripMenuItem});
            this.dateiToolStripMenuItem.Name = "dateiToolStripMenuItem";
            this.dateiToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.dateiToolStripMenuItem.Text = "Datei";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // ScheduleStripMenuItem
            // 
            this.ScheduleStripMenuItem.Name = "ScheduleStripMenuItem";
            this.ScheduleStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.ScheduleStripMenuItem.Text = "Schedule";
            this.ScheduleStripMenuItem.Click += new System.EventHandler(this.ScheduleStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.aboutToolStripMenuItem.Text = "About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // EvaluatorToolStripMenuItem
            // 
            this.EvaluatorToolStripMenuItem.Name = "EvaluatorToolStripMenuItem";
            this.EvaluatorToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
            this.EvaluatorToolStripMenuItem.Text = "Evaluator...";
            this.EvaluatorToolStripMenuItem.Click += new System.EventHandler(this.evaluatorToolStripMenuItem_Click);
            // 
            // bGetDocumentsDocuments
            // 
            this.bGetDocumentsDocuments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bGetDocumentsDocuments.Location = new System.Drawing.Point(3, 348);
            this.bGetDocumentsDocuments.Name = "bGetDocumentsDocuments";
            this.bGetDocumentsDocuments.Size = new System.Drawing.Size(124, 23);
            this.bGetDocumentsDocuments.TabIndex = 2;
            this.bGetDocumentsDocuments.Text = "Dokumente suchen";
            this.bGetDocumentsDocuments.UseVisualStyleBackColor = true;
            this.bGetDocumentsDocuments.Click += new System.EventHandler(this.bCheckForDocuments_Click);
            // 
            // bProcessDocuments
            // 
            this.bProcessDocuments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bProcessDocuments.Location = new System.Drawing.Point(185, 348);
            this.bProcessDocuments.Name = "bProcessDocuments";
            this.bProcessDocuments.Size = new System.Drawing.Size(186, 23);
            this.bProcessDocuments.TabIndex = 6;
            this.bProcessDocuments.Text = "Dokumente verarbeiten";
            this.bProcessDocuments.UseVisualStyleBackColor = true;
            this.bProcessDocuments.Click += new System.EventHandler(this.bProcessDocuments_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusConfig,
            this.toolStripProgressBar1,
            this.toolStripStatusPlatzHalter,
            this.toolStripStatusLabel2,
            this.toolStripStatusCountdown});
            this.statusStrip1.Location = new System.Drawing.Point(0, 380);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(664, 22);
            this.statusStrip1.TabIndex = 11;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusConfig
            // 
            this.toolStripStatusConfig.Name = "toolStripStatusConfig";
            this.toolStripStatusConfig.Size = new System.Drawing.Size(120, 17);
            this.toolStripStatusConfig.Text = "toolStripStatusConfig";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            this.toolStripProgressBar1.Visible = false;
            // 
            // toolStripStatusPlatzHalter
            // 
            this.toolStripStatusPlatzHalter.Name = "toolStripStatusPlatzHalter";
            this.toolStripStatusPlatzHalter.Size = new System.Drawing.Size(420, 17);
            this.toolStripStatusPlatzHalter.Spring = true;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(109, 17);
            this.toolStripStatusLabel2.Text = "No stop scheduled.";
            // 
            // toolStripStatusCountdown
            // 
            this.toolStripStatusCountdown.Name = "toolStripStatusCountdown";
            this.toolStripStatusCountdown.Size = new System.Drawing.Size(0, 17);
            // 
            // btnSearchMore
            // 
            this.btnSearchMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSearchMore.Enabled = false;
            this.btnSearchMore.Location = new System.Drawing.Point(125, 348);
            this.btnSearchMore.Name = "btnSearchMore";
            this.btnSearchMore.Size = new System.Drawing.Size(56, 23);
            this.btnSearchMore.TabIndex = 12;
            this.btnSearchMore.Text = "Nächste";
            this.btnSearchMore.UseVisualStyleBackColor = true;
            this.btnSearchMore.Click += new System.EventHandler(this.btnSearchMore_Click);
            // 
            // timerShutDown
            // 
            this.timerShutDown.Interval = 1000;
            this.timerShutDown.Tick += new System.EventHandler(this.timerShutDown_Tick);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(-1, 377);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(666, 3);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 13;
            // 
            // groupBoxDocuments
            // 
            this.groupBoxDocuments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDocuments.Controls.Add(this.dgwDocuments);
            this.groupBoxDocuments.Location = new System.Drawing.Point(3, 27);
            this.groupBoxDocuments.Name = "groupBoxDocuments";
            this.groupBoxDocuments.Size = new System.Drawing.Size(661, 315);
            this.groupBoxDocuments.TabIndex = 10;
            this.groupBoxDocuments.TabStop = false;
            this.groupBoxDocuments.Text = "Gefundene Dokumente:";
            // 
            // dgwDocuments
            // 
            this.dgwDocuments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgwDocuments.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgwDocuments.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwDocuments.Location = new System.Drawing.Point(6, 16);
            this.dgwDocuments.Name = "dgwDocuments";
            this.dgwDocuments.ReadOnly = true;
            this.dgwDocuments.RowHeadersVisible = false;
            this.dgwDocuments.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgwDocuments.Size = new System.Drawing.Size(649, 293);
            this.dgwDocuments.TabIndex = 5;
            this.dgwDocuments.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgwDocuments_CellFormatting);
            this.dgwDocuments.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgwDocuments_DataBindingComplete);
            // 
            // GetDocumentsWorker
            // 
            this.GetDocumentsWorker.WorkerReportsProgress = true;
            this.GetDocumentsWorker.WorkerSupportsCancellation = true;
            this.GetDocumentsWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.GetDocumentsWorker_DoWork);
            this.GetDocumentsWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.GetDocumentsWorker_ProgressChanged);
            this.GetDocumentsWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.GetDocumentsWorker_RunWorkerCompleted);
            // 
            // ProcessDocumentsWorker
            // 
            this.ProcessDocumentsWorker.WorkerReportsProgress = true;
            this.ProcessDocumentsWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ProcessDocumentsWorker_DoWork);
            this.ProcessDocumentsWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.ProcessDocumentsWorker_ProgressChanged);
            this.ProcessDocumentsWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ProcessDocumentsWorker_RunWorkerCompleted);
            // 
            // cBShowConsole
            // 
            this.cBShowConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cBShowConsole.AutoSize = true;
            this.cBShowConsole.Location = new System.Drawing.Point(594, 360);
            this.cBShowConsole.Name = "cBShowConsole";
            this.cBShowConsole.Size = new System.Drawing.Size(64, 17);
            this.cBShowConsole.TabIndex = 14;
            this.cBShowConsole.Text = "Console";
            this.cBShowConsole.UseVisualStyleBackColor = true;
            this.cBShowConsole.CheckedChanged += new System.EventHandler(this.cBShowConsole_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 402);
            this.Controls.Add(this.cBShowConsole);
            this.Controls.Add(this.bGetDocumentsDocuments);
            this.Controls.Add(this.groupBoxDocuments);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnSearchMore);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.bProcessDocuments);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Five Informatik AG - Document Reminder";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBoxDocuments.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgwDocuments)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem dateiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.Button bGetDocumentsDocuments;
        private System.Windows.Forms.Button bProcessDocuments;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusConfig;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Button btnSearchMore;
        private System.Windows.Forms.ToolStripMenuItem ScheduleStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusPlatzHalter;
        private System.Windows.Forms.Timer timerShutDown;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusCountdown;
        private System.Windows.Forms.ToolStripMenuItem EvaluatorToolStripMenuItem;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.GroupBox groupBoxDocuments;
        private System.Windows.Forms.DataGridView dgwDocuments;
        private System.ComponentModel.BackgroundWorker GetDocumentsWorker;
        private System.ComponentModel.BackgroundWorker ProcessDocumentsWorker;
        private System.Windows.Forms.CheckBox cBShowConsole;
    }
}

