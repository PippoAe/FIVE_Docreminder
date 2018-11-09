using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace docreminder
{
    public partial class MainForm : Form
    {
        ConsoleWriter log = ConsoleWriter.GetInstance;
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private WebServiceHandler _webServiceHandler;
        public DateTime starttime;
        public TimeSpan scheduledTimeLeft;
        public int sucessfullySent = 0;

        public WebServiceHandler webserviceHandler
        {
            get { return _webServiceHandler; }
            set { _webServiceHandler = value; }
        }

        public MainForm()
        {
            InitializeComponent();
            log4net.Appender.RichTextBoxAppender.SetRichTextBox(rTextBoxLog, "RichTextBoxAppender");

            ColumnHeader header = new ColumnHeader();
            header.Text = "";
            header.Name = "col1";
            lBLog.HeaderStyle = ColumnHeaderStyle.None;
            lBLog.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            lBLog.Columns.Add(header);

            lBLog.Scrollable = true;
            lBLog.View = View.Details;

            //Set Starttime
            starttime = DateTime.Now;
            //CheckSchedule
            CheckSchedule();            

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            log.lWLog = this.lBLog;
        }
        

        //Toolstrip

        private void optionenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings settingsForm = new FormSettings(this);
            settingsForm.Show();
        }

        private void sQLVariabelnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form ExpVarForm = new Forms.ExpressionVariablesForm();
            ExpVarForm.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 aboutForm = new Form2();
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
 

        //Buttons

        private void bCheckForEBills_Click(object sender, EventArgs e)
        {
            log4.Info("TEEEST!");
            //New Search. Remove Resumepoint
            if (_webServiceHandler != null && _webServiceHandler.resumePoint != "")
                _webServiceHandler.resumePoint = "";
            CheckForEBills();   
        }

        private void bSendEbill_Click(object sender, EventArgs e)
        {
            bCheckForEBills.Enabled = false;
            bSendEbills.Enabled = false;
            btnSearchMore.Enabled = false;
            processDocumentsWorker.RunWorkerAsync();

        }

        private void btnSearchMore_Click(object sender, EventArgs e)
        {
            CheckForEBills();
            btnSearchMore.Enabled = false;
        }



        //Santas little helpers.

        public void CheckForEBills()
        {
            bSendEbills.Enabled = false;
            bCheckForEBills.Enabled = false;

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            getDocumentsWorker.RunWorkerAsync();  
            
        }

        public void displayFoundEBills(KXWS.SDocument[] documents)
        {
            string info = String.Format("Found {0} documents matching the searchproperties. HasMore:{1}",
                documents.Count(), _webServiceHandler.hasMore);
            log.WriteInfo(info);

            //dgwEbills.Columns.Clear();
            //dgwEbills.Rows.Clear();
            //dgwEbills.Refresh();
            dgwEbills.Columns.Clear();
            dgwEbills.DataSource = null;

            dgwEbills.DataSource = documents;
            //dgwEbills.

            if (documents.Count() > 0)
            {
                //Add DocumentpropertyColumns to DGW.
                if (documents[0].documentProperties.Count() > 0)
                {
                    foreach (KXWS.SDocumentProperty docprop in documents[0].documentProperties)
                    {
                        dgwEbills.Columns.Add(docprop.name, docprop.name);
                    }
                }

                for (int i = 0; i < dgwEbills.Columns.Count; i++)
                {
                    dgwEbills.Columns[i].Visible = false;
                }
                try
                {
                    List<KXWS.SSearchCondition> searchonlist = new List<KXWS.SSearchCondition>();
                    searchonlist = (List<KXWS.SSearchCondition>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.KendoxSearchProperties, searchonlist.GetType()));

                    dgwEbills.Columns["name"].Visible = true;
                    dgwEbills.Columns["DocumentID"].Visible = true;

                    foreach (KXWS.SSearchCondition searchcon in searchonlist)
                    {
                        dgwEbills.Columns[searchcon.propertyTypeName].Visible = true;
                    }
                }
                catch
                {
                }

                //FillEachDocumentProperty
                foreach (DataGridViewRow row in dgwEbills.Rows)
                {
                    foreach (KXWS.SDocumentProperty docprop in ((KXWS.SDocument)(row.DataBoundItem)).documentProperties)
                    {
                        if (docprop.propertyValues.Count() > 0)
                            row.Cells[docprop.name].Value = docprop.propertyValues[0];
                        else
                            row.Cells[docprop.name].Value = "";
                    }
                }


                //Validate with Additional Computed Identifiers before sending.
                if (Properties.Settings.Default.AddCpIdisActive)
                {
                    log.WriteInfo("Additional computed identifier is active! Validating each document...");


                        int greenlighted = 0;
                        ExpressionsEvaluator expVal = new ExpressionsEvaluator();
                        foreach (DataGridViewRow row in dgwEbills.Rows)
                        {
                            try{
                                    if (Convert.ToBoolean(expVal.Evaluate(Properties.Settings.Default.AdditionalComputedIdentifier, row)))
                                    {
                                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                                        greenlighted++;
                                    }
                                    else
                                    {
                                        row.DefaultCellStyle.BackColor = Color.Orange;
                                    }
                                }
                            catch (Exception e)
                            {
                                log.WriteInfo("An Error happened while validating the documents with the additional computed identifier!" + e.Message);
                                row.DefaultCellStyle.BackColor = Color.Red;
                            }
                        }

                   log.WriteInfo("Found " + greenlighted + " documents matching the additional computed identifier.");
                }
            }
            
        }


        private string GetAllUpdateProperties()
        {
            return "";
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

                toolStripStatusLabel2.Text = String.Format("Scheduled shutdown: {0}, {1}", usedStopDay.dayofweek, usedStopDay.time.ToString("HH:mm"));
            }
            else
            {
                toolStripStatusCountdown.Text = "No shutdown scheduled.";
                toolStripStatusLabel2.Text = "";
            }
        }


        //Workers

        /// <summary>
        /// Process Documents Worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            if (dgwEbills.Rows.Count > 0)
            {
                log.WriteInfo("Document processing initiated. Now trying to process each document.");
                List<KXWS.SSearchCondition> searchConList = new List<KXWS.SSearchCondition>();
                searchConList = (List<KXWS.SSearchCondition>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.KendoxSearchProperties, searchConList.GetType()));

                bool errorHappened = false;

                //Process all documents or Prepare Mailing List!
                foreach (DataGridViewRow row in dgwEbills.Rows)
                {
                    processDocumentsWorker.ReportProgress((100 / dgwEbills.Rows.Count) * (row.Index + 1));
                    //if endtime is reached, turn remaining rows, but dont process.
                    if (scheduledTimeLeft.TotalSeconds < 0)
                    {
                        row.HeaderCell.Style.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        if (row.DefaultCellStyle.BackColor != Color.Red && row.DefaultCellStyle.BackColor != Color.Orange)
                        {
                            string documentId = (string)row.Cells["DocumentID"].Value;
                            if (!Properties.Settings.Default.GroupingActive)
                                Task.Run(() => _webServiceHandler.ProcessDocument(documentId, row));
                            else
                            {
                                List<string> childGuids = webserviceHandler.searchForChildDocumentGuids(row, documentId);
                                
                                //If Count <= 1, check if the child might be the parent.
                                //AEPH 13.01.2017 - Besmer Request "Dont send Parent if no Child is found".
                                if (Properties.Settings.Default.GroupingSendWithoutChild)
                                {
                                    Task.Run(() => _webServiceHandler.ProcessDocument(documentId, row, childGuids));
                                }
                                else
                                {
                                    if (childGuids.Count == 0 || (childGuids.Count == 1 && childGuids.Contains(documentId)))
                                    {
                                        row.HeaderCell.Style.BackColor = Color.LightGreen;
                                        log.WriteInfo("Parentdocument ignored, no child documents available. ObjectID:"+documentId);
                                    }
                                    else
                                    {
                                        Task.Run(() => _webServiceHandler.ProcessDocument(documentId, row, childGuids));
                                    }
                                }
                            }
                        }
                    }
                }



                log.WriteInfo("Waiting for all asynchronus documentprocessing to finish...");
                bool allfinished = false;
                
                processDocumentsWorker.ReportProgress(0);
                while (!allfinished)
                {
                    bool oneraised = false;
                    dgwEbills.Invoke((MethodInvoker)delegate()
                    {
                        dgwEbills.EnableHeadersVisualStyles = false;
                    });

                    int i = 0;
                    foreach (DataGridViewRow row in dgwEbills.Rows)
                    {
                        if (!(row.HeaderCell.Style.BackColor == Color.LightGreen | row.HeaderCell.Style.BackColor == Color.Red | row.DefaultCellStyle.BackColor == Color.Orange))
                        {
                            oneraised = true;
                        }
                        else
                        {
                            i++;
                        }
                    }

                    if (oneraised)
                        allfinished = false;
                    else
                        allfinished = true;

                    processDocumentsWorker.ReportProgress((100 / dgwEbills.Rows.Count) * i);
                }

                foreach (DataGridViewRow row in dgwEbills.Rows)
                {
                    if (row.HeaderCell.Style.BackColor == Color.LightGreen)
                        sucessfullySent++;
                    if(row.HeaderCell.Style.BackColor == Color.Red)
                        errorHappened = true;
                }


                //if there are more documents do nothing.
                if (_webServiceHandler.hasMore && scheduledTimeLeft.TotalSeconds >= 0) {
                    string error = "Documents processed without errors.";
                    if (errorHappened)
                        error = "Documents processed with errors!";
                    log.WriteInfo(error);
                }
                else
                {
                    //Send report if needed
                    if (Properties.Settings.Default.IsReportActive)
                    {
                        log.SendReportMail(sucessfullySent, DateTime.Now - starttime);
                        sucessfullySent = 0;
                    }

                    //Check if Error Happened
                    if (errorHappened)
                        log.WriteError("An Error occured in atleast one of the processed documents.");
                    else
                        log.WriteInfo("All Documents have been processed.");
                }

            }
            else
            {
                log.WriteInfo("There are no documents to be processed.");
            }
        }

        /// <summary>
        /// On the UI Trhead. Update the status.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            this.Text = e.ProgressPercentage.ToString() + "%";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Text = "Five Informatik AG - Document Reminder";
            progressBar1.Value = 0;
            bCheckForEBills.Enabled = true;
            if (_webServiceHandler.hasMore)
                btnSearchMore.Enabled = true;
            bSendEbills.Enabled = false;
            //If automode and NoMore Documents. Close!
            if ((Program.automode && !_webServiceHandler.hasMore) || scheduledTimeLeft.TotalSeconds < 0)
                this.Close();
            //If automode and more Documents searchMore.
            else if (Program.automode && _webServiceHandler.hasMore)
            {
                btnSearchMore_Click(null, null);
            }
        }


        /// <summary>
        /// Get Documents Worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            getDocumentsWorker.ReportProgress(10);
            if (_webServiceHandler == null)
            {
                log.WriteInfo("Logging in into Archive via WebService...");
                getDocumentsWorker.ReportProgress(40);
                _webServiceHandler = new WebServiceHandler();
                
            }

            KXWS.SDocument[] documents;
            if (_webServiceHandler.Login())
            {
                getDocumentsWorker.ReportProgress(60);
                documents = _webServiceHandler.searchforEbills(null);
            }
            else
            {
                getDocumentsWorker.ReportProgress(100);
                documents = new KXWS.SDocument[0];
            }
            getDocumentsWorker.ReportProgress(100);
            e.Result = documents;
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            //this.Text = e.ProgressPercentage.ToString() + "%";
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            displayFoundEBills((KXWS.SDocument[])e.Result);
            progressBar1.Value = 0;
            bSendEbills.Enabled = true;
            bCheckForEBills.Enabled = true;
            if (webserviceHandler.hasMore)
            {
                btnSearchMore.Enabled = true;
                btnSearchMore.Text = String.Format("Nächste {0}", Properties.Settings.Default.SearchQuantity.ToString());
            }
            else
            {
                btnSearchMore.Text = "Nächste";
                btnSearchMore.Enabled = false;
            }

            //IF Automode, send Ebills
            if(Program.automode)
                bSendEbill_Click(this, new EventArgs());
            
        }


        private void timerShutDown_Tick(object sender, EventArgs e)
        {
            scheduledTimeLeft = scheduledTimeLeft.Subtract(new TimeSpan(0, 0, 1));
            
            string TimeSpanText = string.Format(
                scheduledTimeLeft.TotalDays >= 1 ? @"{0:d\.h\:mm\:ss}" : @"{0:hh\:mm\:ss}",
                scheduledTimeLeft); 
             toolStripStatusCountdown.Text = ("("+TimeSpanText+")");
            
            //Scheduled time reached
            if(scheduledTimeLeft.TotalSeconds > 4 & scheduledTimeLeft.TotalSeconds < 5 )
            {
                log.WriteInfo(String.Format("Todays scheduled endtime is almost reached. Shutting down ASAP."));
            }
            if (scheduledTimeLeft.TotalSeconds < 0)
            {
                //No work is being done.
                if (!processDocumentsWorker.IsBusy && !processDocumentsWorker.IsBusy)
                {
                    this.Close();
                }
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            var mess = "";
            if (Program.customConfigFile != "")
                mess = string.Format("Loaded Config: '{0}'", Path.GetFileName(Program.customConfigFile));
            else
                mess = "Loaded Config: 'Default'";
            toolStripStatusConfig.Text = mess;
            log4.Info(mess);

            if (Program.automode)
            {
                log.WriteInfo("Application started in Automode.");
                CheckForEBills();
            }
        }
    }
}

