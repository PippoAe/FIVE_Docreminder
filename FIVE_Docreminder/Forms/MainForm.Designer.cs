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
            this.optionenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ScheduleStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sQLVariabelnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bCheckForEBills = new System.Windows.Forms.Button();
            this.bSendEbills = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgwEbills = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rTextBoxLog = new System.Windows.Forms.RichTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusConfig = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusPlatzHalter = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusCountdown = new System.Windows.Forms.ToolStripStatusLabel();
            this.processDocumentsWorker = new System.ComponentModel.BackgroundWorker();
            this.getDocumentsWorker = new System.ComponentModel.BackgroundWorker();
            this.btnSearchMore = new System.Windows.Forms.Button();
            this.timerShutDown = new System.Windows.Forms.Timer(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.menuStrip1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwEbills)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dateiToolStripMenuItem,
            this.sQLVariabelnToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1115, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // dateiToolStripMenuItem
            // 
            this.dateiToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionenToolStripMenuItem,
            this.ScheduleStripMenuItem,
            this.aboutToolStripMenuItem});
            this.dateiToolStripMenuItem.Name = "dateiToolStripMenuItem";
            this.dateiToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.dateiToolStripMenuItem.Text = "Datei";
            // 
            // optionenToolStripMenuItem
            // 
            this.optionenToolStripMenuItem.Name = "optionenToolStripMenuItem";
            this.optionenToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.optionenToolStripMenuItem.Text = "Optionen";
            this.optionenToolStripMenuItem.Click += new System.EventHandler(this.optionenToolStripMenuItem_Click);
            // 
            // ScheduleStripMenuItem
            // 
            this.ScheduleStripMenuItem.Name = "ScheduleStripMenuItem";
            this.ScheduleStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.ScheduleStripMenuItem.Text = "Schedule";
            this.ScheduleStripMenuItem.Click += new System.EventHandler(this.ScheduleStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.aboutToolStripMenuItem.Text = "About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // sQLVariabelnToolStripMenuItem
            // 
            this.sQLVariabelnToolStripMenuItem.Name = "sQLVariabelnToolStripMenuItem";
            this.sQLVariabelnToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
            this.sQLVariabelnToolStripMenuItem.Text = "Evaluator...";
            this.sQLVariabelnToolStripMenuItem.Click += new System.EventHandler(this.sQLVariabelnToolStripMenuItem_Click);
            // 
            // bCheckForEBills
            // 
            this.bCheckForEBills.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bCheckForEBills.Location = new System.Drawing.Point(495, 378);
            this.bCheckForEBills.Name = "bCheckForEBills";
            this.bCheckForEBills.Size = new System.Drawing.Size(194, 23);
            this.bCheckForEBills.TabIndex = 2;
            this.bCheckForEBills.Text = "Dokumente suchen";
            this.bCheckForEBills.UseVisualStyleBackColor = true;
            this.bCheckForEBills.Click += new System.EventHandler(this.bCheckForEBills_Click);
            // 
            // bSendEbills
            // 
            this.bSendEbills.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bSendEbills.Location = new System.Drawing.Point(703, 378);
            this.bSendEbills.Name = "bSendEbills";
            this.bSendEbills.Size = new System.Drawing.Size(209, 23);
            this.bSendEbills.TabIndex = 6;
            this.bSendEbills.Text = "Dokumente verarbeiten";
            this.bSendEbills.UseVisualStyleBackColor = true;
            this.bSendEbills.Click += new System.EventHandler(this.bSendEbill_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.dgwEbills);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(580, 342);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Gefundene Dokumente:";
            // 
            // dgwEbills
            // 
            this.dgwEbills.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgwEbills.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwEbills.Location = new System.Drawing.Point(6, 16);
            this.dgwEbills.Name = "dgwEbills";
            this.dgwEbills.ReadOnly = true;
            this.dgwEbills.Size = new System.Drawing.Size(568, 323);
            this.dgwEbills.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rTextBoxLog);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(519, 339);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Log";
            // 
            // rTextBoxLog
            // 
            this.rTextBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rTextBoxLog.BackColor = System.Drawing.SystemColors.MenuText;
            this.rTextBoxLog.ForeColor = System.Drawing.SystemColors.Menu;
            this.rTextBoxLog.Location = new System.Drawing.Point(6, 16);
            this.rTextBoxLog.Name = "rTextBoxLog";
            this.rTextBoxLog.ReadOnly = true;
            this.rTextBoxLog.Size = new System.Drawing.Size(507, 323);
            this.rTextBoxLog.TabIndex = 4;
            this.rTextBoxLog.Text = "";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusConfig,
            this.toolStripProgressBar1,
            this.toolStripStatusPlatzHalter,
            this.toolStripStatusLabel2,
            this.toolStripStatusCountdown});
            this.statusStrip1.Location = new System.Drawing.Point(0, 410);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1115, 22);
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
            this.toolStripStatusPlatzHalter.Size = new System.Drawing.Size(871, 17);
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
            // processDocumentsWorker
            // 
            this.processDocumentsWorker.WorkerReportsProgress = true;
            this.processDocumentsWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.processDocumentsWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.processDocumentsWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // getDocumentsWorker
            // 
            this.getDocumentsWorker.WorkerReportsProgress = true;
            this.getDocumentsWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
            this.getDocumentsWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker2_ProgressChanged);
            this.getDocumentsWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker2_RunWorkerCompleted);
            // 
            // btnSearchMore
            // 
            this.btnSearchMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSearchMore.Enabled = false;
            this.btnSearchMore.Location = new System.Drawing.Point(918, 378);
            this.btnSearchMore.Name = "btnSearchMore";
            this.btnSearchMore.Size = new System.Drawing.Size(194, 23);
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
            this.progressBar1.Location = new System.Drawing.Point(-1, 407);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1117, 3);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 13;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Size = new System.Drawing.Size(1115, 345);
            this.splitContainer1.SplitterDistance = 586;
            this.splitContainer1.TabIndex = 14;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1115, 432);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnSearchMore);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.bSendEbills);
            this.Controls.Add(this.bCheckForEBills);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Five Informatik AG - Document Reminder";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgwEbills)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem dateiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionenToolStripMenuItem;
        private System.Windows.Forms.Button bCheckForEBills;
        private System.Windows.Forms.Button bSendEbills;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgwEbills;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusConfig;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.ComponentModel.BackgroundWorker processDocumentsWorker;
        private System.ComponentModel.BackgroundWorker getDocumentsWorker;
        private System.Windows.Forms.Button btnSearchMore;
        private System.Windows.Forms.ToolStripMenuItem ScheduleStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusPlatzHalter;
        private System.Windows.Forms.Timer timerShutDown;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusCountdown;
        private System.Windows.Forms.ToolStripMenuItem sQLVariabelnToolStripMenuItem;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RichTextBox rTextBoxLog;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}

