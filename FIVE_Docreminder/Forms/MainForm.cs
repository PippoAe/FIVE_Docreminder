using docreminder.InfoShareService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace docreminder
{
    public partial class MainForm : Form
    {
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string defaultText = "Five Informatik AG - Document Reminder";

        public DateTime starttime;
        public TimeSpan scheduledTimeLeft;
        public int sucessfullySent = 0;
        private List<BO.WorkObject> currentWorkObjects;
        
        private int nOofErrors;

        public MainForm()
        {
            InitializeComponent();

            ColumnHeader header = new ColumnHeader
            {
                Text = "",
                Name = "col1"
            };

            //Set Starttime
            starttime = DateTime.Now;
            //CheckSchedule
            CheckSchedule();

            //UI Refresh Timer - mainly used to see update status of workobjects.
            Timer timer = new Timer();
            timer.Interval = (500); // 10 secs
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        #region Events&Presentation

        #region Visuals
        private void timer_Tick(object sender, EventArgs e)
        {
            dgwDocuments.SuspendLayout();
            dgwDocuments.Refresh();
            dgwDocuments.ResumeLayout();         
        }

        private void dgwDocuments_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            dgwDocuments.SuspendLayout();
            switch (e.ColumnIndex)
            {
                //ObjectID
                case 0:
                    break;

                //RDY
                case 1:
                    if (Convert.ToBoolean(e.Value))
                        e.CellStyle.BackColor = Color.LightGreen;
                    break;

                //Finished
                case 2:
                    if (Convert.ToBoolean(e.Value))
                        e.CellStyle.BackColor = Color.LightGreen;
                    break;

                //Error
                case 3:
                    if (Convert.ToBoolean(e.Value))
                        e.CellStyle.BackColor = Color.PaleVioletRed;
                    break;
            }
            dgwDocuments.ResumeLayout();
        }

        private void dgwDocuments_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgwDocuments.SuspendLayout();
            dgwDocuments.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgwDocuments.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgwDocuments.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgwDocuments.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            //childs
            if (Properties.Settings.Default.GroupingActive)
            {
                dgwDocuments.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgwDocuments.Columns[4].Visible = true;
            }
            else
                dgwDocuments.Columns[4].Visible = false;

            //info
            dgwDocuments.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dgwDocuments.ClearSelection();
            dgwDocuments.ResumeLayout();
        }
        #endregion

        #region Events
        private void MainForm_Shown(object sender, EventArgs e)
        {
            cBShowConsole.Checked = Properties.Settings.Default.ConsoleEnabled;
            cBShowConsole_CheckedChanged(cBShowConsole, null);

            var mess = "";
            if (Program.customConfigFile != "")
                mess = string.Format("Loaded Config: '{0}'", Path.GetFileName(Program.customConfigFile));
            else
                mess = "Loaded Config: 'Default'";
            toolStripStatusConfig.Text = mess;
            log4.Info(mess);

            if (Program.automode)
            {
                log4.Info("Application started in Automode. Hiding Mainform.");
                this.Hide();
                CheckForDocuments();
            }
        }

        private void timerShutDown_Tick(object sender, EventArgs e)
        {
            scheduledTimeLeft = scheduledTimeLeft.Subtract(new TimeSpan(0, 0, 1));

            string TimeSpanText = string.Format(
                scheduledTimeLeft.TotalDays >= 1 ? @"{0:d\.h\:mm\:ss}" : @"{0:hh\:mm\:ss}",
                scheduledTimeLeft);
            toolStripStatusCountdown.Text = ("(" + TimeSpanText + ")");

            //Scheduled time reached
            if (scheduledTimeLeft.TotalSeconds > 4 & scheduledTimeLeft.TotalSeconds < 5)
            {
                log4.Info(string.Format("Todays scheduled endtime is almost reached. Shutting down ASAP."));
            }
            if (scheduledTimeLeft.TotalSeconds < 0)
            {
                //No work is being done.
                if (!ProcessDocumentsWorker.IsBusy && !ProcessDocumentsWorker.IsBusy)
                {
                    Exit();
                }
            }
        }

        private void cBShowConsole_CheckedChanged(object sender, EventArgs e)
        {
            bool showConsole = ((CheckBox)sender).Checked;
            Properties.Settings.Default["ConsoleEnabled"] = showConsole;
            Properties.Settings.Default.Save();

            if (showConsole)
                AllocConsole();
            else
                FreeConsole();


        }
        #endregion

        #region Clicks
        private void bCheckForDocuments_Click(object sender, EventArgs e)
        {
            //New Search. Delete resumepoint if its not the first search.
            if(currentWorkObjects != null)
                WCFHandler.GetInstance.resumePoint = null;

            CheckForDocuments();
        }
        private void btnSearchMore_Click(object sender, EventArgs e)
        {
            CheckForDocuments();
            btnSearchMore.Enabled = false;
        }

        private void bProcessDocuments_Click(object sender, EventArgs e)
        {
            ProcessDocuments();
        }


        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings settingsForm = new FormSettings(this);
            settingsForm.Show();
        }

        private void evaluatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form ExpVarForm = new Forms.ExpressionVariablesForm();
            ExpVarForm.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout aboutForm = new FormAbout();
            aboutForm.Show();
        }

        private void ScheduleStripMenuItem_Click(object sender, EventArgs e)
        {
            Forms.FormSchedule ScheduleForm = new Forms.FormSchedule();
            DialogResult result = ScheduleForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                CheckSchedule();
            }
        }

        #endregion

        #endregion

        #region Helpers
        public void CheckForDocuments()
        {
            bProcessDocuments.Enabled = false;
            bGetDocumentsDocuments.Enabled = false;

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            GetDocumentsWorker.RunWorkerAsync();
        }

        public void ProcessDocuments()
        {
            bGetDocumentsDocuments.Enabled = false;
            bProcessDocuments.Enabled = false;
            btnSearchMore.Enabled = false;

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            ProcessDocumentsWorker.RunWorkerAsync();
        }

        private void CheckSchedule()
        {
            List<Forms.StopDay> stopDays = new List<Forms.StopDay>();
            if (Properties.Settings.Default.StopSchedule != "")
                stopDays = (List<Forms.StopDay>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.StopSchedule, stopDays.GetType()));

            DayOfWeek dayNow = DateTime.Today.DayOfWeek;
            TimeSpan timeNow = DateTime.Now.TimeOfDay;

            if (stopDays.Count() != 0)
            {
                Forms.StopDay usedStopDay = new Forms.StopDay();
                DateTime now = DateTime.Now;
                TimeSpan shortest = new TimeSpan(7, 0, 0, 0);
                foreach (Forms.StopDay sd in stopDays)
                {
                    int daysUntil = ((int)sd.dayofweek - (int)now.DayOfWeek + 7) % 7;
                    //if its today but time has passed its in a week.
                    if (daysUntil == 0 && sd.time.TimeOfDay < now.TimeOfDay)
                    {
                        daysUntil = daysUntil + 7;
                    }
                    TimeSpan timetill = now.AddDays(daysUntil) - now;
                    if (timetill <= shortest)
                    {
                        //if its not today or the time has not passed yet
                        if (timetill.Days != 0 | sd.time.TimeOfDay > now.TimeOfDay)
                        {
                            shortest = (sd.time.TimeOfDay - now.TimeOfDay).Add(timetill);
                            usedStopDay = sd;
                        }
                    }
                }
                scheduledTimeLeft = shortest;
                timerShutDown.Start();

                toolStripStatusLabel2.Text = string.Format("Scheduled shutdown: {0}, {1}", usedStopDay.dayofweek, usedStopDay.time.ToString("HH:mm"));
            }
            else
            {
                toolStripStatusCountdown.Text = "No shutdown scheduled.";
                toolStripStatusLabel2.Text = "";
            }
        }

        private void Exit()
        {
            if (Properties.Settings.Default.SendErrorMail && nOofErrors > 0)
            {
                log4.Info("Sending error-mail...");
                MailHandler.GetInstance.SendErrorMail();
            }

            log4.Info("Application shutting down.");
            this.Refresh();

        #if DEBUG
            log4.Info("We are running in debug. Waiting for input before shutdown.");
            Console.ReadLine();
        #endif
            this.Close();
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        #endregion

        #region Workers

        #region GetDocumentsWorker
        private void GetDocumentsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            GetDocumentsWorker.ReportProgress(10);
            try
            {
                List<BO.WorkObject> workObjects = new List<BO.WorkObject>();
                DocumentSimpleContract[] documents;
                if (WCFHandler.GetInstance.isConnected())
                {
                    GetDocumentsWorker.ReportProgress(10);
                    documents = WCFHandler.GetInstance.SearchForDocuments();
                    GetDocumentsWorker.ReportProgress(20);

                    //int done = 0;
                    GetDocumentsWorker.ReportProgress(60);
                    
                    foreach (DocumentSimpleContract siCo in documents)
                    {
                        workObjects.Add(new BO.WorkObject(siCo.Id));
                        //ReportProgress
                        //done++;
                        //var total = workObjects.Count();
                        //double progress = ((double)done / total) * 100;
                        //GetDocumentsWorker.ReportProgress(Convert.ToInt32(progress));
                    }
                    GetDocumentsWorker.ReportProgress(80);

                    //Check for cross-referenced child documents if child documents inherit parent document markerproperties.
                    if (Properties.Settings.Default.GroupingActive && Properties.Settings.Default.GroupingInheritMarkerProperties)
                    {
                        DateTime start = DateTime.Now;
                        List<DocumentContract> allAffectedDocs = new List<DocumentContract>();
                        foreach (BO.WorkObject wo in workObjects)
                        {
                            allAffectedDocs.AddRange(wo.allAffectedDocuments);
                        }
                        var duplicates = allAffectedDocs.GroupBy(x => x.Id).Where(g => g.Count() > 1).Select(y => y.Key);
                        foreach (BO.WorkObject wo in workObjects)
                        {
                            var numberOfdupesForDoc = wo.allAffectedDocuments.Where(x => duplicates.Contains(x.Id)).Count();
                            if(numberOfdupesForDoc > 0)
                                wo.AbortProcessing(string.Format("Some child-documents of this document ({0}) are referenced by other parent-documents. This is not allowed if markerproperties are inherited to child-documents.", numberOfdupesForDoc));
                        }
                        DateTime end = DateTime.Now;
                        log4.Debug(string.Format("Checking cross-references for documents took {0}ms.", (end - start).TotalMilliseconds.ToString()));
                    }
                    GetDocumentsWorker.ReportProgress(100);                  
                }
                e.Result = workObjects;
            }
            catch (Exception ex)
            {
                log4.Error(string.Format("An error happened while searching for documents. Message: {0}", ex.Message));
            }
        }

        private void GetDocumentsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            this.Text = e.ProgressPercentage.ToString() + "%";
        }

        private void GetDocumentsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Text = defaultText;
            progressBar1.Value = 0;
            //Set current WorkObjects.
            currentWorkObjects = (List<BO.WorkObject>)e.Result;
            //Update DGW
            dgwDocuments.SuspendLayout();
            dgwDocuments.DataSource = currentWorkObjects;
            dgwDocuments.ResumeLayout();

            //Set Info to Groupbox-Text.
            int found;
            int ready;
            found = currentWorkObjects.Count();
            ready = currentWorkObjects.Where(x => x.ready == true).Count();
            groupBoxDocuments.Text = string.Format("Gefundene Dokumente: ({0}/{1})", ready, found);
            //Log found and ready document amounts.
            log4.Info(string.Format("{0} of {1} documents are ready for processing.", ready, found));



            progressBar1.Value = 0;
            bProcessDocuments.Enabled = true;
            bGetDocumentsDocuments.Enabled = true;

            if (WCFHandler.GetInstance.hasMore)
            {
                btnSearchMore.Enabled = true;
                btnSearchMore.Text = string.Format("Nächste {0}", Properties.Settings.Default.SearchQuantity.ToString());
            }
            else
            {
                btnSearchMore.Text = "Nächste";
                btnSearchMore.Enabled = false;
            }

            //IF Automode, Process-Documents.
            if (Program.automode)
                bProcessDocuments_Click(this, new EventArgs());
        }
        #endregion

        #region ProcessDocumentsWorker
        private void ProcessDocumentsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var noOfItems = currentWorkObjects.Where(x => x.ready).Count();
            if (noOfItems > 0)
            {
                //Start processing of all workobjects.
                log4.Info("Document processing initiated. Now trying to process each document.");
                foreach (BO.WorkObject wo in currentWorkObjects)
                {
                    Task.Run(() => wo.Process());
                }


                //Wait for all documents to finish.
                log4.Info("Waiting for all documentprocessing to finish...");
                bool finished = false;
                while (!finished)
                {
                    var doneProcessing = currentWorkObjects.Where(x => x.finished || x.error).Count();
                    var total = currentWorkObjects.Count();
                    if (doneProcessing == total)
                        finished = true;

                    double progress = ((double)doneProcessing / total) * 100;
                    ProcessDocumentsWorker.ReportProgress(Convert.ToInt32(progress));
                    Thread.Sleep(100);
                }
            }

            else
            {
                log4.Info("There are no documents to be processed.");
            }
        }

        private void ProcessDocumentsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            this.Text = e.ProgressPercentage.ToString() + "%";
        }

        private void ProcessDocumentsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            log4.Info("Done!");

            this.Text = defaultText;
            progressBar1.Value = 0;
            progressBar1.Style = ProgressBarStyle.Blocks;
            this.Refresh();
            bGetDocumentsDocuments.Enabled = true;
            if (WCFHandler.GetInstance.hasMore)
                btnSearchMore.Enabled = true;
            bProcessDocuments.Enabled = false;

            //Add total number of errors to counter.
            nOofErrors += currentWorkObjects.Where(x => x.error).Count();





            //If automode and NoMore Documents. Close!
            if ((Program.automode && !WCFHandler.GetInstance.hasMore) || scheduledTimeLeft.TotalSeconds < 0)
            {
                log4.Info(string.Format("No more documents left or scheduled shutdown-time has been reached. Shutting down."));
                Exit();
            }
            //If automode and more Documents searchMore.
            else if (Program.automode && WCFHandler.GetInstance.hasMore)
            {
                btnSearchMore_Click(null, null);
            }
        }
        #endregion

        #endregion 
    }
}

