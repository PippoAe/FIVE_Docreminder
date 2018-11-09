using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.IO;
using System.Security.Cryptography;

namespace docreminder
{
    public partial class FormSettings : Form
    {

        MainForm mainform;

        ConsoleWriter log = ConsoleWriter.GetInstance;

        List<KeyValuePair<string, bool>> lInfoStores = new List<KeyValuePair<string, bool>>();
        List<string> lWsFunctions = null;


        public FormSettings(MainForm mainform)
        {
            this.mainform = mainform;
            InitializeComponent();

        }

        private void Settings_Load(object sender, EventArgs e)
        {
            string sProgramPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);


            // Create the ToolTip and associate with the Form container.
            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 20000;
            toolTip1.InitialDelay = 500;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(this.btnInsertIndexMailSubject, "Add a documentproperty");
            toolTip1.SetToolTip(this.btnBrowseEmailTemplate, "Browse e-mail template.");

            toolTip1.SetToolTip(this.txtBxAdditionalComputedIdentifier, "Result must be a boolean.\r\n" +
                                                                        "Custom Functions: TodayDate, TodayTime,\r\n" +
                                                                        "IDX('Prop'),DateDiff(date,date), AddDays(date,days).\r\n" +
                                                                        "Hard-coded dates in # like '#01.01.2015#'");
            toolTip1.SetToolTip(this.btnInsertMultiLanguageProperty, "If selected, the template is selected using the value of this property.\r\n"+
                                                                     "For Example: template.html is selected -> template.html.de, template.html.fr,template.html.en is used.");

            toolTip1.SetToolTip(this.btnInsertAttachmentRenameProperty, "If this is configured, the configured value will be used as the filename of the attachment (also works with grouping).\r\n" +
                                                                        "This field uses the NCALC-Syntax. The Filetype is automatically added depending on the original file.");


            toolTip1.SetToolTip(this.cbEncodePW, "Save password encrypted.");


            //Load All Configuration from Config
            txtBxSMTPServer.Text = Properties.Settings.Default.SMTPServerAdress;
            txtBxSMTPPort.Text = Properties.Settings.Default.SMTPPort;
            txtBXSMTPUsername.Text = Properties.Settings.Default.SMTPUsername;
            txtBxSMTPPassword.Text = Properties.Settings.Default.SMPTPassword;
            cBUseTSL.Checked = Properties.Settings.Default.SMTPUseSSL;

            cBSendMailActive.Checked = Properties.Settings.Default.SendMailActive;
            txtBxSMTPSender.Text = Properties.Settings.Default.SMTPSender;
            txtBxSMTPSendTo.Text = Properties.Settings.Default.EBillSendTo;
            txtBxSMTPSubject.Text = Properties.Settings.Default.EBillSubject;
            //EmailTemplate
            txtBxEmailTemplatePath.Text = Properties.Settings.Default.EmailTemplatePath;
            txtBxMultiLanguageProperty.Text = Properties.Settings.Default.MultiLanguageTemplateProperty;
            txtBxAttachmentRenameProperty.Text = Properties.Settings.Default.AttachmentRenameProperty;

            if ((string)Properties.Settings.Default.EmailTemplatePath == "")
            {
                string templateFilePath = Path.Combine(sProgramPath, "standard_email_template.html");
                txtBxEmailTemplatePath.Text = templateFilePath;
            }
            else
                txtBxEmailTemplatePath.Text = Properties.Settings.Default.EmailTemplatePath;



            cBDocSafeActive.Checked = Properties.Settings.Default.DocSafeActive;

            txtBxKendoxWebserviceURL.Text = Properties.Settings.Default.E_Bill_Uploader_KXWS_KXWebService40;
            txtBxKendoxServer.Text = Properties.Settings.Default.KendoxServerAdress;
            txtBxKendoxPort.Text = Properties.Settings.Default.KendoxPort;
            txtBxKendoxUsername.Text = Properties.Settings.Default.KendoxUsername;
            cbEncodePW.Checked = Properties.Settings.Default.isKXPWEncrypted;
            txtBxKendoxPassword.Text = Properties.Settings.Default.KendoxPassword;
            cBCulture.SelectedText = Properties.Settings.Default.Culture;
            cBIsLogActive.Checked = Properties.Settings.Default.IsLogActive;
            txtBxLogPath.Text = Properties.Settings.Default.LogPath;

            cBSendErrorMail.Checked = Properties.Settings.Default.SendErrorMail;
            tbErrorMailSendTo.Text = Properties.Settings.Default.ErrorMailSendTo;
            tBErrorMailSubject.Text = Properties.Settings.Default.ErrorMailSubject;
            cBErrorMailIncludeLog.Checked = Properties.Settings.Default.ErrorMailIncludeLog;

            cBStartProcessActive.Checked = Properties.Settings.Default.StartProcessActive;
            txtBxProcessRecipient.Text = Properties.Settings.Default.ProcessRecipient;

            WebServiceHandler.ProcessTemplateItem processTemplateItem = new WebServiceHandler.ProcessTemplateItem();
     
            //If ProcessTemplate is a string, it has to be evaluated. Else its fixed.
            processTemplateItem.ProcessName = "Template auswählen...";
            if (Properties.Settings.Default.ProcessName != "")
            {
                try
                {
                    processTemplateItem = (WebServiceHandler.ProcessTemplateItem)FileHelper.XmlDeserializeFromString(Properties.Settings.Default.ProcessName, processTemplateItem.GetType());
                }
                catch
                {
                    //ProcessTemplateItem seems to be a string and needs to be evaluated on runtime.
                    processTemplateItem.ProcessName = Properties.Settings.Default.ProcessName;
                }
            } 

            cBSelectedProcess.Items.Add(processTemplateItem);
            cBSelectedProcess.SelectedIndex = 0;



            //linkfile
            cBAttFile.Checked = Properties.Settings.Default.AttachDocument;
            if (!cBAttFile.Checked)
            {
                Properties.Settings.Default.GroupingActive = false;
                cBGroupingActive.Enabled = false;
            }

            cBKxLinkFile.Checked = Properties.Settings.Default.AttachLnkFile;
            if ((string)Properties.Settings.Default.LnkFilePath == "")
            {
                string templateFilePath = Path.Combine(sProgramPath, "standard_linkfile_template.dlk");
                txtBxLinkFilePath.Text = templateFilePath;
            }
            else
                txtBxLinkFilePath.Text = Properties.Settings.Default.LnkFilePath;

            //Grouping
            cBGroupingActive.Checked = Properties.Settings.Default.GroupingActive;

            //Report
            cbIsReportActive.Checked = Properties.Settings.Default.IsReportActive;
            txtBxReportRecipient.Text = Properties.Settings.Default.ReportRecipient;
            txtBxReportSubject.Text = Properties.Settings.Default.ReportSubject;
            if ((string)Properties.Settings.Default.ReportTemplatePath == "")
            {
                string templateFilePath = Path.Combine(sProgramPath, "standard_report_template.html");
                txtBxReportTemplatePath.Text = templateFilePath;
            }
            else
                txtBxReportTemplatePath.Text = Properties.Settings.Default.ReportTemplatePath;

            //AdditionalIdentifier
            txtBxAdditionalComputedIdentifier.Text = Properties.Settings.Default.AdditionalComputedIdentifier;
            cBAddCpIdisActive.Checked = Properties.Settings.Default.AddCpIdisActive;


            foreach (string store in Properties.Settings.Default.KendoxInfoStores.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                lInfoStores.Add(new KeyValuePair<string, bool>(store, true));
            }
            UpdateInfoStoreList(lInfoStores);

            //SearchQuantity
            nUdSearchQuantity.Value = Properties.Settings.Default.SearchQuantity;

            //SearchConditions
            List<KXWS.SSearchCondition> searchonlist = new List<KXWS.SSearchCondition>();
            if (Properties.Settings.Default.KendoxSearchProperties != "")
                searchonlist = (List<KXWS.SSearchCondition>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.KendoxSearchProperties, searchonlist.GetType()));

            //Fill SearchProperties
            foreach (KXWS.SSearchCondition searchcon in searchonlist)
            {
                //DataRow test = new DataRow();
                string[] row = { searchcon.propertyTypeName, searchcon.operation, string.Join(";", searchcon.propertyValueArray), searchcon.relation.ToString() };
                //row.HeaderCell.Value = String.Format("{0}", row.Index + 1)
                dgwSearchProperties.Rows.Add(row);
            }


            //KendoxMarkerProperties
            List<KXWS.SDocumentPropertyUpdate> markerProperties = new List<KXWS.SDocumentPropertyUpdate>();
            if (Properties.Settings.Default.KendoxMarkerProperties != "")
                markerProperties = (List<KXWS.SDocumentPropertyUpdate>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.KendoxMarkerProperties, markerProperties.GetType()));


            //Fill MarkerProperties
            foreach (KXWS.SDocumentPropertyUpdate markerProperty in markerProperties)
            {
                string updateAction = "UPDATE";
                switch (markerProperty.updateAction)
                {
                    case KXWS.UpdateActions.UPDATE:
                        updateAction = "UPDATE";
                        break;
                    case KXWS.UpdateActions.NONE:
                        updateAction = "NONE";
                        break;
                    case KXWS.UpdateActions.DELETE:
                        updateAction = "DELETE";
                        break;
                    case KXWS.UpdateActions.ADD:
                        updateAction = "ADD";
                        break;
                }

                string[] row = { markerProperty.propertyTypeName, string.Join(";", markerProperty.propertyValues), updateAction };
                dGwMarkerProperties.Rows.Add(row);
            }


            //Custom WS Functions
            cBCustomWSFunction.Checked = Properties.Settings.Default.CustomWSFunctionsActive;
            List<Forms.ExpressionVariablesForm.KeyValuePair<string, string>> customFunctionsList = new List<Forms.ExpressionVariablesForm.KeyValuePair<string, string>>();
            if (Properties.Settings.Default.CustomWSFunctions != "")
                customFunctionsList = (List<Forms.ExpressionVariablesForm.KeyValuePair<string, string>>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.CustomWSFunctions, customFunctionsList.GetType()));

            foreach (Forms.ExpressionVariablesForm.KeyValuePair<string, string> kvp in customFunctionsList)
            {
                string[] row = { kvp.Key, kvp.Value };
                dgwCustomWSFunction.Rows.Add(row);
            }



            //Preload with Serverdata if connection has been made.
            if (mainform.webserviceHandler != null)
            {
                if (mainform.webserviceHandler.Login())
                {
                    preLoadSettingsData();
                }
            }

        }


        /// <summary>
        /// Update InfoStores to ComboboxList Control.
        /// </summary>
        /// <param name="lInfoStores"></param>
        private void UpdateInfoStoreList(List<KeyValuePair<string, bool>> lInfoStores)
        {

            //If All is not in list add it in first position.
            if (!lInfoStores.Exists(x => x.Key == "All"))
            {
                lInfoStores.Insert(0, new KeyValuePair<string, bool>("All", false));
            }


            cLbInfoStores.Items.Clear();

            foreach (KeyValuePair<string, bool> kvp in lInfoStores)
            {
                cLbInfoStores.Items.Add(kvp.Key, kvp.Value);
            }
            cLbInfoStores.Refresh();
        }

        private void bSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (txtBxLogPath.Text == "")
            {
                string standardLogPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Five_DocReminder\\Log";
                System.IO.Directory.CreateDirectory(standardLogPath);
                folderBrowserDialog1.SelectedPath = standardLogPath;
            }
            else
                folderBrowserDialog1.SelectedPath = @txtBxLogPath.Text;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
                txtBxLogPath.Text = folderBrowserDialog1.SelectedPath;
        }


        private void bSave_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }


        private void bOK_Click(object sender, EventArgs e)
        {
            SaveConfig();
            this.Close();
        }

        private void SaveConfig()
        {
            Properties.Settings.Default["SMTPServerAdress"] = txtBxSMTPServer.Text;
            Properties.Settings.Default["SMTPPort"] = txtBxSMTPPort.Text;
            Properties.Settings.Default["SMTPUsername"] = txtBXSMTPUsername.Text;
            Properties.Settings.Default["SMPTPassword"] = txtBxSMTPPassword.Text;
            Properties.Settings.Default["SMTPUseSSL"] = cBUseTSL.Checked;

            //Mail
            Properties.Settings.Default["SendMailActive"] = cBSendMailActive.Checked;
            Properties.Settings.Default["SMTPSender"] = txtBxSMTPSender.Text;
            Properties.Settings.Default["EBillSendTo"] = txtBxSMTPSendTo.Text;
            Properties.Settings.Default["EBillSubject"] = txtBxSMTPSubject.Text;
            //Mail - Template 
            Properties.Settings.Default["EmailTemplatePath"] = txtBxEmailTemplatePath.Text;
            Properties.Settings.Default["MultiLanguageTemplateProperty"] = txtBxMultiLanguageProperty.Text;

            //Mail - Attachments
            Properties.Settings.Default["AttachmentRenameProperty"] = txtBxAttachmentRenameProperty.Text;
            Properties.Settings.Default["AttachDocument"] = cBAttFile.Checked;
            Properties.Settings.Default["GroupingActive"] = cBGroupingActive.Checked;
            Properties.Settings.Default["AttachLnkFile"] = cBKxLinkFile.Checked;
            Properties.Settings.Default["LnkFilePath"] = txtBxLinkFilePath.Text;

            //DocSafe
            Properties.Settings.Default["DocSafeActive"] = cBDocSafeActive.Checked;

            //Process
            Properties.Settings.Default["StartProcessActive"] = cBStartProcessActive.Checked;
            //if item = null its a string that needs to be evaluated on runtime.
            if (cBSelectedProcess.SelectedItem != null)
                Properties.Settings.Default["ProcessName"] = FileHelper.XmlSerializeToString(cBSelectedProcess.SelectedItem);
            else
                Properties.Settings.Default["ProcessName"] = cBSelectedProcess.Text;

            Properties.Settings.Default["ProcessRecipient"] = txtBxProcessRecipient.Text;

            //Kendox Server
            Properties.Settings.Default["E_Bill_Uploader_KXWS_KXWebService40"] = txtBxKendoxWebserviceURL.Text;
            Properties.Settings.Default["KendoxServerAdress"] = txtBxKendoxServer.Text;
            Properties.Settings.Default["KendoxPort"] = txtBxKendoxPort.Text;
            Properties.Settings.Default["KendoxUsername"] = txtBxKendoxUsername.Text;
            Properties.Settings.Default["isKXPWEncrypted"] = cbEncodePW.Checked;
            //AEPH 12.02.2016 Encrypt Password
            //Password not yet encrypted.
            if (txtBxKendoxPassword.Text.Length < 25 && cbEncodePW.Checked)
            {
                string[] encrypted = FileHelper.EncryptString(txtBxKendoxPassword.Text);
                Properties.Settings.Default["KendoxPassword"] = encrypted[0];
                Properties.Settings.Default["kxpwentropy"] = encrypted[1];
            }
            else
            {
                Properties.Settings.Default["KendoxPassword"] = txtBxKendoxPassword.Text;
            }

            Properties.Settings.Default["Culture"] = cBCulture.Text;

            if ((string)Properties.Settings.Default["LogPath"] != txtBxLogPath.Text)
                MessageBox.Show("Einige Änderungen (insbesondere am Logpfad) setzten einen Neustart der Applikation voraus.", "Achtung!",
    MessageBoxButtons.OK, MessageBoxIcon.Information);

            Properties.Settings.Default["IsLogActive"] = cBIsLogActive.Checked;
            Properties.Settings.Default["LogPath"] = txtBxLogPath.Text;


            //ErrorMail
            Properties.Settings.Default["SendErrorMail"] = cBSendErrorMail.Checked;
            Properties.Settings.Default["ErrorMailSendTo"] = tbErrorMailSendTo.Text;
            Properties.Settings.Default["ErrorMailSubject"] = tBErrorMailSubject.Text;
            Properties.Settings.Default["ErrorMailIncludeLog"] = cBErrorMailIncludeLog.Checked;


            //Report
            Properties.Settings.Default["IsReportActive"] = cbIsReportActive.Checked;
            Properties.Settings.Default["ReportRecipient"] = txtBxReportRecipient.Text;
            Properties.Settings.Default["ReportSubject"] = txtBxReportSubject.Text;
            Properties.Settings.Default["ReportTemplatePath"] = txtBxReportTemplatePath.Text;

            //AdditionalComputedIdentifier
            Properties.Settings.Default["AdditionalComputedIdentifier"] = txtBxAdditionalComputedIdentifier.Text;
            Properties.Settings.Default["AddCpIdisActive"] = cBAddCpIdisActive.Checked;


            //Searchconditions
            List<KXWS.SSearchCondition> searchonlist = new List<KXWS.SSearchCondition>();

            //MarkerProperties
            List<KXWS.SDocumentPropertyUpdate> markerProperties = new List<KXWS.SDocumentPropertyUpdate>();

            // DataGridViewRowCollection rows = dgwSearchProperties.Rows;

            //InfoStores
            string selectedStores = "";
            foreach (Object checkedInfoStore in cLbInfoStores.CheckedItems)
            {
                selectedStores += checkedInfoStore + ";";
            }
            Properties.Settings.Default["KendoxInfoStores"] = selectedStores;


            //SearchQuantity
            Properties.Settings.Default["SearchQuantity"] = Convert.ToInt32(nUdSearchQuantity.Value);

            //Searchproperties
            foreach (DataGridViewRow row in dgwSearchProperties.Rows)
            {
                if (row.Cells.Count > 0 && row.Cells[0].Value != null)
                {
                    KXWS.SSearchCondition condition = new KXWS.SSearchCondition();
                    condition.propertyTypeName = row.Cells[0].Value.ToString();

                    if (row.Cells[1].Value != null)
                        condition.operation = row.Cells[1].Value.ToString();
                    else
                        condition.operation = "NONE";

                    if (row.Cells[3].Value != null && row.Cells[3].Value.ToString() == "OR")
                        condition.relation = KXWS.Relations.OR;
                    else
                        condition.relation = KXWS.Relations.AND;


                    //condition.relation = row.Cells[2].Value.ToString();
                    if (row.Cells[2].Value != null)
                    {
                        condition.propertyValueArray = row.Cells[2].Value.ToString().Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        //condition.propertyValueArray = new string[] { row.Cells[2].Value.ToString() };
                    }
                    else
                        condition.propertyValueArray = new string[] { "" };

                    searchonlist.Add(condition);
                }
            }
            Properties.Settings.Default["KendoxSearchProperties"] = FileHelper.XmlSerializeToString(searchonlist);


            //KendoxMarkerProperties
            foreach (DataGridViewRow row in dGwMarkerProperties.Rows)
            {
                if (row.Cells.Count > 0 && row.Cells[0].Value != null)
                {

                    KXWS.SDocumentPropertyUpdate markerProp = new KXWS.SDocumentPropertyUpdate();
                    markerProp.propertyTypeName = row.Cells[0].Value.ToString();

                    if (row.Cells[1].Value != null)
                        markerProp.propertyValues = row.Cells[1].Value.ToString().Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    else
                        markerProp.propertyValues = new string[1] { "" };

                    markerProp.updateAction = KXWS.UpdateActions.UPDATE;
                    if (row.Cells[2].Value != null)
                    {
                        switch (row.Cells[2].Value.ToString().ToLower())
                        {
                            case "none":
                                markerProp.updateAction = KXWS.UpdateActions.NONE;
                                break;
                            case "update":
                                markerProp.updateAction = KXWS.UpdateActions.UPDATE;
                                break;
                            case "add":
                                markerProp.updateAction = KXWS.UpdateActions.ADD;
                                break;
                            case "delete":
                                markerProp.updateAction = KXWS.UpdateActions.DELETE;
                                break;
                        }
                    };

                    markerProperties.Add(markerProp);
                }
            }

            Properties.Settings.Default["KendoxMarkerProperties"] = FileHelper.XmlSerializeToString(markerProperties);

            Properties.Settings.Default["CustomWSFunctionsActive"] = cBCustomWSFunction.Checked;
            //Custom WS Functions
            List<Forms.ExpressionVariablesForm.KeyValuePair<string, string>> variables = new List<Forms.ExpressionVariablesForm.KeyValuePair<string, string>>();

            foreach (DataGridViewRow row in dgwCustomWSFunction.Rows)
            {
                if (row.Cells.Count > 0 && row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    Forms.ExpressionVariablesForm.KeyValuePair<string, string> kvp = new Forms.ExpressionVariablesForm.KeyValuePair<string, string>();
                    kvp.Key = row.Cells[0].Value.ToString();
                    kvp.Value = row.Cells[1].Value.ToString();
                    variables.Add(kvp);
                }
            }

            Properties.Settings.Default["CustomWSFunctions"] = FileHelper.XmlSerializeToString(variables);
            if (variables.Count <= 0)
                Properties.Settings.Default["CustomWSFunctionsActive"] = false;

            

            Properties.Settings.Default.Save();
        }

        private void SearchPropertiesSerialization()
        {
            KXWS.SSearchCondition[] sSearchConditions = new KXWS.SSearchCondition[1];
            KXWS.SSearchCondition sSearchCondition = new KXWS.SSearchCondition();
            sSearchCondition.propertyTypeName = "ebillmail";
            sSearchCondition.operation = "GT";
            sSearchCondition.propertyValueArray = new string[] { "1" };
            sSearchConditions[0] = sSearchCondition;

            Properties.Settings.Default.KendoxSearchProperties = FileHelper.XmlSerializeToString(sSearchConditions);

            KXWS.SSearchCondition[] sSearchConditionsTEST = new KXWS.SSearchCondition[1];

            sSearchConditionsTEST = (KXWS.SSearchCondition[])FileHelper.XmlDeserializeFromString(Properties.Settings.Default.KendoxSearchProperties, sSearchConditionsTEST.GetType());
        }

        private void btnTestSMTP_Click(object sender, EventArgs e)
        {
            SaveConfig();
            //FormInputDialog testDialog = new FormInputDialog(txtBxSMTPSendTo.Text);
            string recipient = txtBxSMTPSendTo.Text;
            FormInputDialog inputDialog = new FormInputDialog("SMTP-Test", "Recipient of Test-Email:", "Send!");
            inputDialog.txtBxInput.Text = recipient;
            if (inputDialog.ShowDialog(this) == DialogResult.OK)
            {
                recipient = inputDialog.txtBxInput.Text;
                log.SendTestMail(recipient);
                inputDialog.Dispose();
            }
        }

        private void btnTestKendoxConnection_Click(object sender, EventArgs e)
        {
            SaveConfig();
            preLoadSettingsData();
        }

        private void preLoadSettingsData()
        {
            bool loggedin = false;

            //Connect Button Action & Label
            lblKXConTest.Text = "Trying to log in...";
            this.Refresh();

            if (mainform.webserviceHandler == null)
            {
                mainform.webserviceHandler = new WebServiceHandler();
            }

            if (mainform.webserviceHandler.Login())
            {
                lblKXConTest.Text = "Logged in sucessfully!";
                btnTestKendoxConnection.Enabled = false;
                loggedin = true;

            }
            else
            {
                lblKXConTest.Text = "Login failed! See log...";
            }

            if (loggedin)
            {
                //Populate InfoStore Checkboxes.
                foreach (string storeName in mainform.webserviceHandler.getAllInfoStores())
                {
                    if (lInfoStores.Exists(x => x.Key == storeName))
                    {
                        lInfoStores.Remove(lInfoStores.Find(x => x.Key == storeName));
                        lInfoStores.Add(new KeyValuePair<string, bool>(storeName, true));
                    }
                    else
                    {
                        lInfoStores.Add(new KeyValuePair<string, bool>(storeName, false));
                    }
                }
                UpdateInfoStoreList(lInfoStores);

                //Populate ProcessTemplateBox
                foreach (WebServiceHandler.ProcessTemplateItem pti in mainform.webserviceHandler.getAllProcessTemplates())
                {
                    if (pti.ProcessName != cBSelectedProcess.Text)
                        cBSelectedProcess.Items.Add(pti);
                }

                //Activate Index Buttons
                btnInsertIndexMailRecipient.Enabled = true;
                btnInsertIndexMailSubject.Enabled = true;
                btnInsertIndexProcessRecipients.Enabled = true;
                btnInsertMultiLanguageProperty.Enabled = true;
                btnInsertAttachmentRenameProperty.Enabled = true;

            }

        }

        //InfoStore Item has been checked.
        private void cLbInfoStores_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string checkedItemName = cLbInfoStores.Items[e.Index].ToString();

            if (tcSettings.SelectedTab.Name == "tbProcessing" && cLbInfoStores.Items[e.Index].ToString() == "All")
            {
                if (e.NewValue == CheckState.Checked)
                {
                    for (int i = 1; i < cLbInfoStores.Items.Count; i++)
                    {
                        cLbInfoStores.SetItemChecked(i, true);
                    }
                }
                else
                {
                    for (int i = 1; i < cLbInfoStores.Items.Count; i++)
                    {
                        cLbInfoStores.SetItemChecked(i, false);
                    }
                }

            }
        }

        /// <summary>
        /// Add AutoComplete with Documenttypes to DataGridView SearchProperties.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgwSearchProperties_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgwSearchProperties.CurrentCell.ColumnIndex == 0)
            {
                TextBox prodCode = e.Control as TextBox;
                if (prodCode != null)
                {
                    prodCode.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    prodCode.AutoCompleteCustomSource = ClientListDropDown(false, false);
                    prodCode.AutoCompleteSource = AutoCompleteSource.CustomSource;

                }
            }
            else
            {
                TextBox prodCode = e.Control as TextBox;
                if (prodCode != null)
                {
                    prodCode.AutoCompleteMode = AutoCompleteMode.None;
                }
            }
        }


        /// <summary>
        /// Add AutoComplete with Documenttypes to DataGridView MarkerProperties.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dGwMarkerProperties_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dGwMarkerProperties.CurrentCell.ColumnIndex == 0)
            {
                TextBox prodCode = e.Control as TextBox;
                if (prodCode != null)
                {
                    prodCode.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    prodCode.AutoCompleteCustomSource = ClientListDropDown(false, true);
                    prodCode.AutoCompleteSource = AutoCompleteSource.CustomSource;

                }
            }
            else if (dGwMarkerProperties.CurrentCell.ColumnIndex == 1)
            {
                TextBox prodCode = e.Control as TextBox;
                if (prodCode != null)
                {
                    prodCode.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    var source = new AutoCompleteStringCollection();
                    source.AddRange(new string[]
                                     {
                                         " TodayDate",
                                         " TodayTime",
                                         " DateDiff()",
                                         " AddDays()",
                                         " IDX('Property')"
                                     });
                    prodCode.AutoCompleteCustomSource = source;
                    prodCode.AutoCompleteSource = AutoCompleteSource.CustomSource;

                }
            }
            else
            {
                TextBox prodCode = e.Control as TextBox;
                if (prodCode != null)
                {
                    prodCode.AutoCompleteMode = AutoCompleteMode.None;
                }
            }
        }

        private void dgwCustomWSFunction_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            
            if (dgwCustomWSFunction.CurrentCell.ColumnIndex == 0)
            {
                TextBox prodCode = e.Control as TextBox;
                if (prodCode != null)
                {
                    prodCode.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    prodCode.AutoCompleteCustomSource = getCustomFunctionList(true, false,null);
                    prodCode.AutoCompleteSource = AutoCompleteSource.CustomSource;

                }
            }
            else
            {
                TextBox prodCode = e.Control as TextBox;
                if (prodCode != null)
                {
                    prodCode.AutoCompleteMode = AutoCompleteMode.None;
                }
            }
        }

        private void dgwCustomWSFunction_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                //AEPH 05.02.2016 Check if null first.
                if (dgwCustomWSFunction[0, e.RowIndex].Value != null)
                {
                    AutoCompleteStringCollection asc = getCustomFunctionList(false, true, dgwCustomWSFunction[0, e.RowIndex].Value.ToString());
                    if (asc.Count != 0)
                    {
                        //AEPH - 05.02.2016
                        //asc[0] = asc[0].Replace("string:userGuid,string:documentID,string:storeID,", "");
                        //asc[0] = asc[0].Replace("string:userGuid", "");
                        asc[0] = asc[0].Replace(",", ";");

                        dgwCustomWSFunction[1, e.RowIndex].Value = asc[0];
                    }
                }
            }
        }

        /// <summary>
        /// Get String Collection for Documenttype-Properties
        /// </summary>
        /// <returns></returns>
        public AutoCompleteStringCollection ClientListDropDown(bool all, bool onlyChangeable)
        {
            AutoCompleteStringCollection asc = new AutoCompleteStringCollection();
            try
            {
                foreach (string sPropName in mainform.webserviceHandler.getAllPropertyTypes(Properties.Settings.Default.Culture, all, onlyChangeable))
                {
                    asc.Add(sPropName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("For AutoComplete please connect to Kendox Server!");
            }
            return asc;
        }

        /// <summary>
        /// Get String Collection for Documenttype-Properties
        /// </summary>
        /// <returns></returns>
        public AutoCompleteStringCollection getCustomFunctionList(bool functionOnly, bool parametersonly, string specificfunction)
        {
            if (lWsFunctions == null)
            {
                try
                {
                   
                    lWsFunctions = mainform.webserviceHandler.getAllWSFunctions(pBarCustomWSFunctions);
                }
                catch (Exception e)
                {
                }
            }

            AutoCompleteStringCollection asc = new AutoCompleteStringCollection();
            try
            {
                foreach (string sFunctionName in lWsFunctions)
                {
                    int l = sFunctionName.IndexOf("(");
                    string functionName = sFunctionName.Substring(0, l);
                    string parameters = FileHelper.GetTextBetween(sFunctionName, "(", ")");
                    if (specificfunction != null && functionName.ToLower() != specificfunction.ToLower())
                    {

                    }
                    else
                    {

                        if (!functionOnly && !parametersonly)
                        {
                            asc.Add(sFunctionName);
                        }
                        else
                        {
                            if (functionOnly)
                                asc.Add(functionName);

                            if (parametersonly)
                                asc.Add(parameters);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("For AutoComplete please connect to Kendox Server!");
            }
            return asc;
        }


        /// <summary>
        /// Add Indexes to DataGridViewRows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgwSearchProperties_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }


        private void btnInsertIndexMailRecipient_Click(object sender, EventArgs e)
        {
            //FormInputDialog testDialog = new FormInputDialog(txtBxSMTPSendTo.Text);
            string recipient = txtBxSMTPSendTo.Text;

            FormPickDialog pickDialog = new FormPickDialog(mainform.webserviceHandler.getAllPropertyTypes(Properties.Settings.Default.Culture, true, false), sender);
            if (pickDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtBxSMTPSendTo.Text = txtBxSMTPSendTo.Text.Insert(txtBxSMTPSendTo.SelectionStart, "IDX('" + pickDialog.comboBox1.Text + "')");
                //Prevent Textbox from selecting all on Focus.
                txtBxSMTPSendTo.GotFocus += delegate { txtBxSMTPSendTo.Select(txtBxSMTPSubject.TextLength, 0); };
                txtBxSMTPSendTo.Focus();
                pickDialog.Dispose();
            }
            else
            {
                pickDialog.Dispose();
                txtBxSMTPSendTo.Focus();
            }
        }

        private void btnInsertMultiLanguageProperty_Click(object sender, EventArgs e)
        {
            string property = txtBxMultiLanguageProperty.Text;

            FormPickDialog pickDialog = new FormPickDialog(mainform.webserviceHandler.getAllPropertyTypes(Properties.Settings.Default.Culture, true, false), sender);
            if (pickDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtBxMultiLanguageProperty.Text = txtBxMultiLanguageProperty.Text.Insert(txtBxMultiLanguageProperty.SelectionStart, "IDX('" + pickDialog.comboBox1.Text + "')");
                //Prevent Textbox from selecting all on Focus.
                txtBxMultiLanguageProperty.GotFocus += delegate { txtBxMultiLanguageProperty.Select(txtBxMultiLanguageProperty.TextLength, 0); };
                txtBxMultiLanguageProperty.Focus();
                pickDialog.Dispose();
            }
            else
            {
                pickDialog.Dispose();
                txtBxMultiLanguageProperty.Focus();
            }
        }

        private void btnInsertAttachmentRenameProperty_Click(object sender, EventArgs e)
        {
            string property = txtBxAttachmentRenameProperty.Text;

            FormPickDialog pickDialog = new FormPickDialog(mainform.webserviceHandler.getAllPropertyTypes(Properties.Settings.Default.Culture, true, false), sender);
            if (pickDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtBxAttachmentRenameProperty.Text = txtBxAttachmentRenameProperty.Text.Insert(txtBxAttachmentRenameProperty.SelectionStart, "IDX('" + pickDialog.comboBox1.Text + "')");
                //Prevent Textbox from selecting all on Focus.
                txtBxAttachmentRenameProperty.GotFocus += delegate { txtBxAttachmentRenameProperty.Select(txtBxAttachmentRenameProperty.TextLength, 0); };
                txtBxAttachmentRenameProperty.Focus();
                pickDialog.Dispose();
            }
            else
            {
                pickDialog.Dispose();
                txtBxAttachmentRenameProperty.Focus();
            }
        }




        /// <summary>
        /// Insert Indexfield on Subject Field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInsertIndexMailSubject_Click(object sender, EventArgs e)
        {
            FormPickDialog pickDialog = new FormPickDialog(mainform.webserviceHandler.getAllPropertyTypes(Properties.Settings.Default.Culture, true, false), sender);
            if (pickDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtBxSMTPSubject.Text = txtBxSMTPSubject.Text.Insert(txtBxSMTPSubject.SelectionStart, "IDX('" + pickDialog.comboBox1.Text + "')");
                //Prevent Textbox from selecting all on Focus.
                txtBxSMTPSubject.GotFocus += delegate { txtBxSMTPSubject.Select(txtBxSMTPSubject.TextLength, 0); };
                txtBxSMTPSubject.Focus();
                pickDialog.Dispose();
            }
            else
            {
                pickDialog.Dispose();
                txtBxSMTPSubject.Focus();
            }
        }

        //private void btnInsertIndexMailBody_Click(object sender, EventArgs e)
        //{
        //    FormPickDialog pickDialog = new FormPickDialog(mainform.webserviceHandler.getAllPropertyTypes("DE"), sender);
        //    if (pickDialog.ShowDialog(this) == DialogResult.OK)
        //    {
        //        rtxtBxHTMLBody.Text = rtxtBxHTMLBody.Text.Insert(rtxtBxHTMLBody.SelectionStart, "*IDX[" + pickDialog.comboBox1.Text + "]");
        //        //Prevent Textbox from selecting all on Focus.
        //        rtxtBxHTMLBody.GotFocus += delegate { rtxtBxHTMLBody.Select(rtxtBxHTMLBody.TextLength, 0); };
        //        rtxtBxHTMLBody.Focus();
        //        pickDialog.Dispose();
        //    }
        //    else
        //    {
        //        pickDialog.Dispose();
        //        rtxtBxHTMLBody.Focus();
        //    }
        //}

        private void btnTest_Click(object sender, EventArgs e)
        {
            ExpressionsEvaluator expVal = new ExpressionsEvaluator();
            try
            {
                string ret = expVal.Evaluate(txtBxAdditionalComputedIdentifier.Text).ToString();
                bool value;
                bool isBool = Boolean.TryParse(ret, out value);
                if (isBool)
                {
                    MessageBox.Show(String.Format("Sucessful!\nResult is: '{0}'. ", ret));
                }
                else
                {
                    MessageBox.Show(String.Format("Result is not a boolean!\nResult is: '{0}'. ", ret));
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }


        private void btnSelectTemplate_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileBrowserDialog = new OpenFileDialog();

            if (txtBxLinkFilePath.Text == "")
            {
                string progPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string templateFilePath = Path.Combine(progPath, "linkfile.dlk");
                fileBrowserDialog.InitialDirectory = templateFilePath;
                fileBrowserDialog.RestoreDirectory = true;
                fileBrowserDialog.Filter = "Kendox Linkfile|*.dlk;";
            }
            else
            {
                fileBrowserDialog.InitialDirectory = @txtBxLinkFilePath.Text;
                fileBrowserDialog.RestoreDirectory = true;
                fileBrowserDialog.Filter = "Kendox Linkfile|*.dlk;";
            }
            DialogResult result = fileBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
                txtBxLinkFilePath.Text = fileBrowserDialog.FileName;
        }

        private void btnBrowseReportTemplate_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileBrowserDialog = new OpenFileDialog();

            if (txtBxLinkFilePath.Text == "")
            {
                string progPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string templateFilePath = Path.Combine(progPath, "standard_report_template.html");
                fileBrowserDialog.InitialDirectory = templateFilePath;
                fileBrowserDialog.RestoreDirectory = true;
                fileBrowserDialog.Filter = "Template|*.html;";
            }
            else
            {

                fileBrowserDialog.InitialDirectory = Path.GetDirectoryName(txtBxReportTemplatePath.Text).ToString();
                fileBrowserDialog.RestoreDirectory = true;
                fileBrowserDialog.Filter = "Template|*.html;";
            }
            DialogResult result = fileBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
                txtBxReportTemplatePath.Text = fileBrowserDialog.FileName;
        }

        private void btnBrowseEmailTemplate_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileBrowserDialog = new OpenFileDialog();

            if (txtBxEmailTemplatePath.Text == "")
            {
                string progPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string templateFilePath = Path.Combine(progPath, "standard_email_template.html");
                fileBrowserDialog.InitialDirectory = templateFilePath;
                fileBrowserDialog.RestoreDirectory = true;
                fileBrowserDialog.Filter = "Template|*.html;";
            }
            else
            {

                fileBrowserDialog.InitialDirectory = Path.GetDirectoryName(txtBxEmailTemplatePath.Text).ToString();
                fileBrowserDialog.RestoreDirectory = true;
                fileBrowserDialog.Filter = "Template|*.html;";
            }
            DialogResult result = fileBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
                txtBxEmailTemplatePath.Text = fileBrowserDialog.FileName;
        }

        private void btnVariables_Click(object sender, EventArgs e)
        {
            Form ExpVarForm = new Forms.ExpressionVariablesForm();
            ExpVarForm.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //FormInputDialog testDialog = new FormInputDialog(txtBxSMTPSendTo.Text);
            string recipient = txtBxProcessRecipient.Text;

            FormPickDialog pickDialog = new FormPickDialog(mainform.webserviceHandler.getAllPropertyTypes(Properties.Settings.Default.Culture, true, false), sender);
            if (pickDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtBxProcessRecipient.Text = txtBxProcessRecipient.Text.Insert(txtBxProcessRecipient.SelectionStart, "IDX('" + pickDialog.comboBox1.Text + "')");
                //Prevent Textbox from selecting all on Focus.
                txtBxProcessRecipient.GotFocus += delegate { txtBxProcessRecipient.Select(txtBxSMTPSubject.TextLength, 0); };
                txtBxProcessRecipient.Focus();
                pickDialog.Dispose();
            }
            else
            {
                pickDialog.Dispose();
                txtBxProcessRecipient.Focus();
            }
        }

        private void btnConfigureDocSafe_Click(object sender, EventArgs e)
        {
            Forms.FormDocSafeSettings docSafeSettings = new Forms.FormDocSafeSettings();
            docSafeSettings.Show();
        }

        private void btnConfigureGrouping_Click_1(object sender, EventArgs e)
        {
            Forms.FormGrouping formGrouping = new Forms.FormGrouping(this.mainform);
            formGrouping.Show();
        }

        private void cBAttFile_CheckedChanged(object sender, EventArgs e)
        {
            if (!cBAttFile.Checked)
            {
                cBGroupingActive.Enabled = false;
                cBGroupingActive.Checked = false;
            }
            else
            {
                cBGroupingActive.Enabled = true;
            }
        }

    }
}
