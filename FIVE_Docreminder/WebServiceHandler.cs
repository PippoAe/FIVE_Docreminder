using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Windows.Forms;
using System.Xml.Schema;


namespace docreminder
{
    public class WebServiceHandler
    {
        KXWS.KXWebService40 WebService = new KXWS.KXWebService40();
        string sSessionGuid = "";
        
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool _hasMore = false;
        private string _resumePoint = null;
        List<KXWS.SUserInfoExt> lUsers;
        List<ProcessTemplateItem> lProcessTemplates;

        public bool hasMore { get { return _hasMore; } set { _hasMore = value; } }
        public string resumePoint { get { return _resumePoint; } set { _resumePoint = value; } }

        public bool Login()
        {
            string ret = "";
            bool errorOccured = false;
            if (sSessionGuid == "")
            {
                try
                {
                    //AEPH 12.02.2016 Decrypt password.
                    string kendoxpassword = "";
                    if(Properties.Settings.Default.isKXPWEncrypted)
                        kendoxpassword = FileHelper.DecryptString(Properties.Settings.Default.KendoxPassword, Properties.Settings.Default.kxpwentropy);
                    else 
                        kendoxpassword = Properties.Settings.Default.KendoxPassword;

                    ret = WebService.Logon(Properties.Settings.Default.KendoxServerAdress, Convert.ToInt32(Properties.Settings.Default.KendoxPort), "", Properties.Settings.Default.KendoxUsername, kendoxpassword, "", Properties.Settings.Default.Culture, Properties.Settings.Default.Culture);
                    
                    kendoxpassword = null;
                }
                catch (Exception e)
                {
                     string encrypted = "";
                    if (Properties.Settings.Default.isKXPWEncrypted)
                    {
                        encrypted = "Password is encrypted, did you maybe change user?";
                    }
                    log4.Error(e.Message + " " +encrypted);
                    ret = e.Message+" "+encrypted;
                    errorOccured = true;
                }
                if (!errorOccured)
                {
                    sSessionGuid = ret;
                    ret = "Sucessfully logged in to '" + Properties.Settings.Default.KendoxServerAdress + ":" + Properties.Settings.Default.KendoxPort + "'. SessionGUID: '" + sSessionGuid + "'";
                    log4.Info(ret);
                }
            }

            else
            {
                if (!WebService.IsLoggedIn(sSessionGuid))
                {
                    sSessionGuid = "";
                    this.Login();
                }
            }

            return !errorOccured;
        }

        //Search and Process

        public KXWS.SDocument[] searchforEbills(DataGridView ebillDataGrid)
        {
            bool errorOccured = false;
            KXWS.SDocument[] documents;

            List<KXWS.SSearchCondition> searchConList = new List<KXWS.SSearchCondition>();
            searchConList = (List<KXWS.SSearchCondition>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.KendoxSearchProperties, searchConList.GetType()));


            //Evaluate Searchconditions
            searchConList = EvaluateSearchConditions(searchConList);


            KXWS.SSearchCondition[] searchConditions = searchConList.ToArray();

            string[] listproperties = new string[searchConList.Count];
            //listproperties = searchConList.Select(x => x.propertyTypeName).ToArray();
            listproperties = getAllPropertyTypes(Properties.Settings.Default.Culture, true, false).ToArray();


            //Prepare List of infostores in Config.
            List<KXWS.SInfoStore> lInfoStores = new List<KXWS.SInfoStore>();
            foreach (string store in Properties.Settings.Default.KendoxInfoStores.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                KXWS.SInfoStore infoStore = new KXWS.SInfoStore();
                infoStore.name = store;
                lInfoStores.Add(infoStore);
            }

            KXWS.SInfoStore[] kxInfoStores = null;
            //If "All" is configured, send null with request, else add all configured items.

            if (!lInfoStores.Exists(x => x.name.ToLower() == "all"))
            {
                kxInfoStores = lInfoStores.ToArray();
            }

            try
            {
                if (_resumePoint == null)
                    log4.Info("Searching for documents...");

                documents = (KXWS.SDocument[])(WebService.Search(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, searchConditions, kxInfoStores, listproperties, null, KXWS.FulltextWordRelations.AND, Properties.Settings.Default.SearchQuantity, false, ref _resumePoint, out _hasMore));
            }
            catch (Exception e)
            {
                log4.Error(e.Message);
                errorOccured = true;
                documents = new KXWS.SDocument[0];
            }
            return documents;
        }

        public List<string> searchForChildDocumentGuids(DataGridViewRow parentDocRow, string parentGuid = null)
        {
            bool errorOccured = false;
            KXWS.SDocument[] documents;
            List<string> childDocuments = new List<string>();

            List<KXWS.SSearchCondition> searchConList = new List<KXWS.SSearchCondition>();
            searchConList = (List<KXWS.SSearchCondition>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.GroupingSearchProperties, searchConList.GetType()));


            //Evaluate Searchconditions
            searchConList = EvaluateSearchConditions(searchConList, parentDocRow);

            KXWS.SSearchCondition[] searchConditions = searchConList.ToArray();


            string[] listproperties = new string[searchConList.Count];
            listproperties = getAllPropertyTypes(Properties.Settings.Default.Culture, true, false).ToArray();

            //string[] listproperties = new string[searchConList.Count];
            //listproperties = getAllPropertyTypes(Properties.Settings.Default.Culture, true, false).ToArray();


            //Prepare List of infostores in Config.
            List<KXWS.SInfoStore> lInfoStores = new List<KXWS.SInfoStore>();
            foreach (string store in Properties.Settings.Default.KendoxInfoStores.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                KXWS.SInfoStore infoStore = new KXWS.SInfoStore();
                infoStore.name = store;
                lInfoStores.Add(infoStore);
            }

            KXWS.SInfoStore[] kxInfoStores = null;
            //If "All" is configured, send null with request, else add all configured items.

            if (!lInfoStores.Exists(x => x.name.ToLower() == "all"))
            {
                kxInfoStores = lInfoStores.ToArray();
            }

            try
            {
                if (_resumePoint == null)
                    log4.Info("Searching for documents...");
                //TODO Maybe make search quantity a setting in grouping.
                string resume = "";
                bool hasmore;
                documents = (KXWS.SDocument[])(WebService.Search(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, searchConditions, kxInfoStores, listproperties, null, KXWS.FulltextWordRelations.AND, 10000, false, ref resume, out hasmore));
            }
            catch (Exception e)
            {
                log4.Error(e.Message);
                errorOccured = true;
                documents = new KXWS.SDocument[0];
            }
            
            
            
            foreach(KXWS.SDocument doc in documents)
            {
                childDocuments.Add(doc.documentID);
            }

            return childDocuments;
        }
        public async Task<bool> ProcessDocument(string docGuid, DataGridViewRow row, List<string> childGuids = null)
        {
            log4.Info(string.Format("TaskID {0} processing DocGuid {1}",Task.CurrentId,docGuid));

            List<KXWS.SDocumentPropertyUpdate> markerProperties = new List<KXWS.SDocumentPropertyUpdate>();
            markerProperties = (List<KXWS.SDocumentPropertyUpdate>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.KendoxMarkerProperties, markerProperties.GetType()));
            KXWS.SDocumentPropertyUpdate[] updatePropList;

            //Check if MarkerProperties can be evaluated correctly.
            //If not, we abort the sending.
            //AEPH 03.02.2016
            try
            {
                CheckPropertyValues(markerProperties, row);
            }
            catch (Exception e)
            {
                log4.Info(string.Format("An Error happened while evaluating the Marker-Properties. Check them! ID[{0}]",Task.CurrentId));
                log4.Info(e.Message);
                row.HeaderCell.Style.BackColor = Color.Red;
                row.HeaderCell.ToolTipText = e.Message;
                return false;
            }

            try
            {
                if (!Properties.Settings.Default.GroupingActive)
                    WebService.UndoCheckout(sSessionGuid, docGuid, null);
                else
                {
                    WebService.UndoCheckout(sSessionGuid, docGuid, null);
                    foreach (string childGuid in childGuids)
                    {
                        WebService.UndoCheckout(sSessionGuid, childGuid, null);
                    }
                }
            }
            catch (Exception e)
            {
                log4.Info(string.Format("Error while trying to undo checkout on document. ID[{0}] {1}", Task.CurrentId, e.Message));
                row.HeaderCell.Style.BackColor = Color.Red;
                row.HeaderCell.ToolTipText = e.Message;
                return false;
            }

            //SendEmail
            bool processed = false;
            KXWS.SDocument docinfo = new KXWS.SDocument();
            string attachmentDirectory = "";
            try
            {
                if (Properties.Settings.Default.SendMailActive || Properties.Settings.Default.StartProcessActive || Properties.Settings.Default.DocSafeActive  || Properties.Settings.Default.CustomWSFunctionsActive)
                {

                    if (Properties.Settings.Default.SendMailActive)
                    {
                        ExpressionsEvaluator expVal = new ExpressionsEvaluator();

                        DateTime now = DateTime.Now;
                        //Only download document if it needs to be sent by mail.
                        byte[] document = null;
                        if (Properties.Settings.Default.AttachDocument)
                        {   
                            //If no grouping is active.
                            if (!Properties.Settings.Default.GroupingActive)
                            {
                                document = WebService.GetDocumentFile(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, docGuid, null, null, KXWS.AccessTypesEnum.ContentExport, out docinfo);
                               
                                string fileName = Properties.Settings.Default.AttachmentRenameProperty;

                                if (fileName != "")
                                {
                                    fileName = expVal.Evaluate(fileName, null, docinfo, false);
                                    if (fileName == "")
                                        fileName = docinfo.fileName;
                                    else
                                    {
                                        string fileExtension = Path.GetExtension(docinfo.fileName);
                                        fileName = fileName + fileExtension;
                                    }
                                }
                                else
                                    fileName = docinfo.fileName;


                                fileName = FileHelper.CleanFileName(fileName);

                                docinfo.fileName = fileName;

                            }

                            //If Grouping is active.
                            else
                            {
                                    //KXWS.SDocument docInfo = new KXWS.SDocument();
                                    string password = null;
                                    if (Properties.Settings.Default.GroupingZipName != "")
                                        password = Properties.Settings.Default.GroupingZipName;

                                    //GetDocumentsZipped worked but takes forever and only adds PDF files to the archive.
                                    //document = WebService.GetDocumentsZipped(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, childGuids.ToArray(), false, false, password, out docinfo);

                                    //List<Helper_Classes.FileModel> files = new List<Helper_Classes.FileModel>();

                                    //Create temp-directory.
                                    string tempDirectory = Path.Combine(Path.GetTempPath(), Convert.ToString(Guid.NewGuid()));
                                    Directory.CreateDirectory(tempDirectory);

                                    DateTime start = DateTime.Now;
                                    
                                    //29.12.2016 AEPH - If ParentGUID is not in ChildGUID hits, there's a problem. So we add it forcefully here.
                                    if (!childGuids.Contains(docGuid))
                                        childGuids.Add(docGuid);


                                    

                                    foreach (string childGuid in childGuids)
                                    {


                                        //If parent isn't added to the group, its ignored, else it gets downloaded like a child-document.
                                        if (!Properties.Settings.Default.GroupingAddParent && childGuid == docGuid)
                                        {
                                        }
                                        else
                                        {
                                            KXWS.SDocument ChildDocinfo;
                                            byte[] childDocument = WebService.GetDocumentFile(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, childGuid, null, null, KXWS.AccessTypesEnum.ContentExport, out ChildDocinfo);
                                            //TODO - SwissRe evaluate fileName from DocumentProperty.
                                            //string renameProp = Properties.Settings.Default.AttachmentRenameProperty;

                                            string fileName = Properties.Settings.Default.AttachmentRenameProperty;
                                            if (fileName != "")
                                            {
                                                fileName = expVal.Evaluate(fileName, null, ChildDocinfo, false);
                                                if (fileName == "")
                                                    fileName = ChildDocinfo.fileName;
                                                else
                                                {
                                                    string fileExtension = Path.GetExtension(ChildDocinfo.fileName);
                                                    fileName = fileName + fileExtension;
                                                }
                                            }
                                            else
                                                fileName = ChildDocinfo.fileName;


                                            fileName = FileHelper.CleanFileName(fileName);


                                            //string path = Path.Combine(tempDirectory, ChildDocinfo.fileName);
                                            
                                            string path = Path.Combine(tempDirectory, fileName); 

                                            path = FileHelper.GetUniqueFilePath(path);
                                            File.WriteAllBytes(path, childDocument);
                                        }
                                    }
                                    DateTime downloading = DateTime.Now;
                                    log4.Info(string.Format("Downloading child documents took {0} seconds. ID[{1}]", (downloading - start).Seconds.ToString(),Task.CurrentId));

                                    if (Properties.Settings.Default.GroupingZipped)
                                    {
                                        string dirName = new DirectoryInfo(@tempDirectory).Name;
                                        string zipFilePath = Path.Combine(Path.GetTempPath(), dirName + ".zip");
                                        ZipFile.CreateFromDirectory(tempDirectory, zipFilePath);
                                        document = File.ReadAllBytes(zipFilePath);
                                        File.Delete(zipFilePath);
                                        Directory.Delete(tempDirectory, true);


                                        docinfo.fileName = Properties.Settings.Default.GroupingZipName + ".zip";

                                        DateTime zipping = DateTime.Now;
                                        log4.Info(string.Format("Zipping took {0} seconds.ID[{1}]", (zipping - downloading).Seconds.ToString(), Task.CurrentId));
                                    }
                                    else
                                    {
                                        attachmentDirectory = tempDirectory;
                                    }
                                
                            }
                        }


                        
                        //11.02.2016 AEPH: Get UserList 
                        if (lUsers == null)
                            lUsers = getAllUsers();

                        using (Task<bool> AsyncTaskSendMail = MailHandler.GetInstance.SendDocumentMail(document, docinfo, row, lUsers, attachmentDirectory))
                        {
                            processed = AsyncTaskSendMail.Result;
                        }

                        if (attachmentDirectory != "")
                            Directory.Delete(attachmentDirectory, true);

                        
                    }

                    if (Properties.Settings.Default.DocSafeActive)
                    {
                        DocSafe.DocSafeHandler dsHandler = new  DocSafe.DocSafeHandler ();
                        byte[] document = null;
                        document = WebService.GetDocumentFile(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, docGuid, null, null, KXWS.AccessTypesEnum.ContentExport, out docinfo);
                        processed = dsHandler.SendDocumentToDocsafe(document, docinfo);
                    }

                    if (Properties.Settings.Default.StartProcessActive)
                    {
                        processed = StartProcess(row);
                    }

                    if (Properties.Settings.Default.CustomWSFunctionsActive)
                    #region #CustomWS Function
                    {
                            List<Forms.ExpressionVariablesForm.KeyValuePair<string, string>> customFunctionsList = new List<Forms.ExpressionVariablesForm.KeyValuePair<string, string>>();
                            if (Properties.Settings.Default.CustomWSFunctions != "")
                                customFunctionsList = (List<Forms.ExpressionVariablesForm.KeyValuePair<string, string>>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.CustomWSFunctions, customFunctionsList.GetType()));

                            //Evaluate them CustomFunctions
                            ExpressionsEvaluator expVal = new ExpressionsEvaluator();

                            if (customFunctionsList.Count <= 0)
                            {
                                throw new Exception("Custom WebService-Functions is Activated, but no functions are defined.");
                            }
                            
                            foreach (Forms.ExpressionVariablesForm.KeyValuePair<string, string> kvp in customFunctionsList)
                            {
                                Forms.ExpressionVariablesForm.KeyValuePair<string, string> kvpEvaluated = kvp;
                                //AEPH: 05.02.2016
                                // kvpEvaluated.Value = string.Format("'{0}';'{1}';'{2}';{3}", sSessionGuid, row.Cells[1].Value, null, kvpEvaluated.Value);
                                //"'string:userGuid;string:documentID;string:storeID;'TEST'"
                                kvpEvaluated.Value = kvpEvaluated.Value.Replace("string:userGuid",string.Format("'{0}'",sSessionGuid));
                                kvpEvaluated.Value = kvpEvaluated.Value.Replace("string:documentID", string.Format("'{0}'",row.Cells[1].Value.ToString()));
                                kvpEvaluated.Value = kvpEvaluated.Value.Replace("string:storeID", string.Format("'{0}'",row.Cells["storeID"].Value.ToString()));


                                string[] sValuesSplitted = kvpEvaluated.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                                int i = 0;
                                foreach (string sValue in sValuesSplitted)
                                {
                                    string sEvaluatedValue = expVal.Evaluate(sValue, row);
                                    sValuesSplitted[i] = sEvaluatedValue;
                                    i++;
                                }

                                //sValuesSplitted.

                                MethodInfo theMethod = WebService.GetType().GetMethod(kvpEvaluated.Key);
                                if (theMethod == null)
                                    throw new Exception(string.Format("Method '{0}' not available on Webservice!", kvpEvaluated.Key));
                                bool answer = false;
                                answer = (bool)theMethod.Invoke(WebService, sValuesSplitted);
                                processed = true;
                            }
                    }
                    #endregion

                }                
                else
                    processed = true;
            }
            catch (Exception e)
            {
                log4.Info(string.Format("Error happened while trying to process document. ID[{0}]",Task.CurrentId) + e.Message + e.InnerException);
                row.HeaderCell.Style.BackColor = Color.Red;
                row.HeaderCell.ToolTipText = e.Message + "Inner Message:" + e.InnerException.Message;
                return false;
            }

            if (processed)
            {
                try
                {
                    //AEPH 03.02.2016
                    updatePropList = EvaluatePropertyValues(markerProperties, row).ToArray();
                }

                catch (Exception e)
                {
                    log4.Info(string.Format("An Error happened while evaluating the Marker-Properties. Check them! ID[{0}]",Task.CurrentId));
                    log4.Info(e.Message);
                    row.HeaderCell.Style.BackColor = Color.Red;
                    row.HeaderCell.ToolTipText = e.Message;
                    return false;
                }

                try
                {
                    if(updatePropList.Count() > 1)
                    { 
                    DateTime starttime = DateTime.Now;
                    if (!Properties.Settings.Default.GroupingActive)
                        WebService.UpdateDocumentProperties(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, docGuid, null, updatePropList, null, null, null, null);
                    else
                    {
                        //ParentDocument is in ChildGuid so all are updated correctly here.
                        foreach(string childGuid in childGuids)
                        {
                            WebService.UpdateDocumentProperties(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, childGuid, null, updatePropList, null, null, null, null);
                        }
                    }
                    log4.Info(string.Format("Updating documentproperties took {0} seconds. ID[{1}]", (DateTime.Now - starttime).Seconds.ToString(), Task.CurrentId));
                    }

                }
                catch (Exception e)
                {
                    log4.Error(string.Format("Something went wrong while updating the documentproperty!! Maybe the property is unchangeable!! ID[{0}] Message: {1}",Task.CurrentId, e.Message));
                    row.HeaderCell.Style.BackColor = Color.Red;
                    row.HeaderCell.ToolTipText = e.Message;
                    return false;
                }

                row.HeaderCell.Style.BackColor = Color.LightGreen;

                return true;
            }
            return false;
        }


        //Outputs

        private bool StartProcess(DataGridViewRow row)
        {
            ExpressionsEvaluator expVal = new ExpressionsEvaluator();

            WebServiceHandler.ProcessTemplateItem processTemplateItem = new WebServiceHandler.ProcessTemplateItem();
            try
            {
                processTemplateItem = (WebServiceHandler.ProcessTemplateItem)FileHelper.XmlDeserializeFromString(Properties.Settings.Default.ProcessName, processTemplateItem.GetType());
            }
            catch
            {
                processTemplateItem.ProcessName = Properties.Settings.Default.ProcessName;
            }
            //if it's empty, it need to be evaluated.
            string processNameEvaluated = processTemplateItem.ProcessName;            

            if (processTemplateItem.ProcessTemplate == null)
            {
                try
                {
                    processNameEvaluated = expVal.Evaluate(processTemplateItem.ProcessName, row);
                }
                catch (Exception e)
                {
                    log4.Error("Could not evaluate process name from expression " + processTemplateItem.ProcessName);
                    throw e;
                }

                List<ProcessTemplateItem> processTemplates = getAllProcessTemplates();
                foreach (ProcessTemplateItem template in processTemplates)
                {
                    if (template.ProcessName == processNameEvaluated)
                        processTemplateItem = template;
                }
            }

            //If the template is still null, throw an exception.
            if (processTemplateItem.ProcessTemplate == null)
                throw new Exception("Couldn't evaluate processtemplate! No process found with name: " + processNameEvaluated);

            
            KXWS.SProcessTemplateExt processTemplate = processTemplateItem.ProcessTemplate;

            
            string[] recipientsInput = Properties.Settings.Default.ProcessRecipient.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < recipientsInput.Length; i++)
            {
                recipientsInput[i] = expVal.Evaluate(recipientsInput[i], row);
            }


            string[] sUserIDs = new string[recipientsInput.Length];


            //Convert USERID TO GUID!
            if (lUsers == null)
                lUsers = getAllUsers();

            for (int i = 0; i < recipientsInput.Length; i++)
            {
                foreach (KXWS.SUserInfoExt uInf in lUsers)
                {
                    if (uInf.displayName == recipientsInput[i] || uInf.logonName == recipientsInput[i] || uInf.emailAddress == recipientsInput[i])
                    {
                        sUserIDs[i] = uInf.userID;
                        break;
                    }
                }
            }



            try
            {
                string procID = WebService.CreateProcess(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, processTemplate, null, -1, null, KXWS.PriorityEnum.NORMAL, sUserIDs, false, null, null);
                string[] docIDs = new string[1];
                docIDs[0] = row.Cells[1].Value.ToString();

                WebService.AddDocumentsToProcess(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, docIDs, procID);
            }
            catch (Exception e)
            {
                log4.Info(string.Format("An error occured while starting the process with the document. ID[{0}]",Task.CurrentId));
                log4.Info(e.Message);
                throw e;
            }
            return true;
        }


        //Get Information from WS

        public List<KXWS.SUserInfoExt> getAllUsers()
        {
            DateTime starttime = DateTime.Now;
            KXWS.SUserInfoExt[] aUsers = WebService.GetAllUsers(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, false);
            List<KXWS.SUserInfoExt> lSUsers = aUsers.ToList<KXWS.SUserInfoExt>();
            log4.Info(string.Format("Getting user took {0} seconds. ID[{1}]", (DateTime.Now - starttime).Seconds.ToString(), Task.CurrentId));
            return lSUsers;
        }

        public List<string> getAllInfoStores()
        {
            KXWS.SInfoStore[] infoStores = WebService.GetAllInfoStores(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, true);
            List<string> lSInfoStores = new List<string>();
            foreach (KXWS.SInfoStore store in infoStores)
            {
                lSInfoStores.Add(store.name);
            }
            return lSInfoStores;
        }

        [Serializable]
        public class ProcessTemplateItem
        {
            public string ProcessName { get; set; }
            public KXWS.SProcessTemplateExt ProcessTemplate { get; set; }

            public override string ToString()
            {
                return ProcessName;
            }
        }
        public List<ProcessTemplateItem> getAllProcessTemplates()
        {
            DateTime now = DateTime.Now;
            int miliseconds;

            if (lProcessTemplates != null)
            {
                return lProcessTemplates;
            }

            KXWS.SProcessTemplateExt[] processTemplates = WebService.GetAllProcessTemplates(sSessionGuid, false, true, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture);

            List<ProcessTemplateItem> lSProcessTemplates = new List<ProcessTemplateItem>();
            foreach (KXWS.SProcessTemplateExt template in processTemplates)
            {
                ProcessTemplateItem pTitem = new ProcessTemplateItem();
                pTitem.ProcessTemplate = template;
                KXWS.SCultureString[] cultureStrings = template.processName.cultureStrings;
                pTitem.ProcessName = cultureStrings[0].text;
                foreach (KXWS.SCultureString sCultStr in cultureStrings)
                {
                    if (sCultStr.description == Properties.Settings.Default.Culture)
                    {
                        pTitem.ProcessName = sCultStr.text;
                        break;
                    }
                }

                lSProcessTemplates.Add(pTitem);
            }
            miliseconds = DateTime.Now.Subtract(now).Milliseconds;
            log4.Info(string.Format("Getting all processtemplates took {0} miliseconds.", miliseconds));
            lProcessTemplates = lSProcessTemplates;
            return lSProcessTemplates;
        }

        public List<string> getAllPropertyTypes(string Language, bool all, bool onlyChangeable)
        {
            KXWS.SPropertyTypeExt[] Properties = WebService.GetPropertyTypes(sSessionGuid, Language, Language, KXWS.PropertyTypeEditEnum.ALL);
            List<string> lSPropertys = new List<string>();

            foreach (KXWS.SPropertyTypeExt prop in Properties)
            {

                if (!prop.isObsolete && prop.isActive)
                {
                    if (all)
                        lSPropertys.Add(prop.name);
                    else
                    {
                        if (onlyChangeable)
                        {
                            if (prop.isFreeEditable)
                                lSPropertys.Add(prop.name);
                        }
                        else
                        {
                            if (prop.isSearchable)
                                lSPropertys.Add(prop.name);
                        }
                    }

                }
            }
            return lSPropertys;
        }

        public List<string> getAllWSFunctions(ProgressBar pBar)
        {
            List<string> lSPropertys = new List<string>();

            lSPropertys = getWSDLInfo(pBar);
            return lSPropertys;
        }

        private List<string> getWSDLInfo(ProgressBar pbar)
        {
            List<string> lSFunctions = new List<string>();
            pbar.Value = 10;
            //Build the URL request string
            UriBuilder uriBuilder = new UriBuilder(Properties.Settings.Default.E_Bill_Uploader_KXWS_KXWebService40);
            uriBuilder.Query = "WSDL";

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Method = "GET";
            webRequest.Accept = "text/xml";

            //Submit a web request to get the web service's WSDL
            ServiceDescription serviceDescription;
            using (WebResponse response = webRequest.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    serviceDescription = ServiceDescription.Read(stream);
                }
            }

            pbar.Value = 30;

            pbar.Refresh();
            foreach (PortType portType in serviceDescription.PortTypes)
            {

                foreach (Operation operation in portType.Operations)
                {
                    Console.Out.WriteLine(operation.Name);

                    foreach (var message in operation.Messages)
                    {
                        if (message is OperationInput)
                            //Console.Out.WriteLine("Input Message: {0}", ((OperationInput)message).Message.Name);
                            if (message is OperationOutput)
                                //Console.Out.WriteLine("Output Message: {0}", ((OperationOutput)message).Message.Name);

                                foreach (System.Web.Services.Description.Message messagePart in serviceDescription.Messages)
                                {
                                    if (messagePart.Name != ((OperationMessage)message).Message.Name) continue;

                                    foreach (MessagePart part in messagePart.Parts)
                                    {
                                        //Console.Out.WriteLine(part.Name);
                                    }
                                }
                    }
                    Console.Out.WriteLine();
                }
            } //End listing of types

            //Drill down into the WSDL's complex types to list out the individual schema elements 
            //and their data types
            Types types = serviceDescription.Types;
            XmlSchema xmlSchema = types.Schemas[0];

            XmlSchemaElement previousElement = null;
            bool add = false;


            string previousFunction = "";

            pbar.Value = 80;
            pbar.Refresh();
            foreach (object item in xmlSchema.Items)
            {
                string infoString = "";
                XmlSchemaElement schemaElement = item as XmlSchemaElement;
                XmlSchemaComplexType complexType = item as XmlSchemaComplexType;

                if (schemaElement != null)
                {
                    //Console.Out.WriteLine("Schema Element: {0}", schemaElement.Name);
                    infoString = string.Format("{0}(", schemaElement.Name);

                    XmlSchemaType schemaType = schemaElement.SchemaType;
                    XmlSchemaComplexType schemaComplexType = schemaType as XmlSchemaComplexType;
                    try
                    {
                        if (schemaComplexType != null)
                        {
                            XmlSchemaParticle particle = schemaComplexType.Particle;
                            XmlSchemaSequence sequence =
                                particle as XmlSchemaSequence;
                            if (sequence != null)
                            {
                                if (previousElement != null && schemaElement.Name.Length >= 8)
                                {
                                    if (previousElement.Name.Contains(schemaElement.Name.Substring(0, schemaElement.Name.Length - 8)))
                                    {
                                        if (((XmlSchemaElement)sequence.Items[0]).SchemaTypeName.Name.Contains("bool"))
                                            add = true;
                                        else
                                            add = false;
                                    }
                                }
                                foreach (XmlSchemaElement childElement in sequence.Items)
                                {
                                    //Console.Out.WriteLine("    Element/Type: {0}:{1}", childElement.Name,
                                    //                      childElement.SchemaTypeName.Name);
                                    infoString += string.Format("{0}:{1},", childElement.SchemaTypeName.Name, childElement.Name);
                                    string test = childElement.ToString();
                                }

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string test = e.Message;
                    }
                }
                else if (complexType != null)
                {
                    Console.Out.WriteLine("Complex Type: {0}", complexType.Name);
                    OutputElements(complexType.Particle);
                }
                try
                {
                    if (infoString != "")
                    {
                        infoString = infoString.Remove(infoString.Length - 1);
                        infoString += ")";
                        if (add)
                        {
                            lSFunctions.Add(previousFunction);
                            add = false;
                        }

                    }
                }
                catch (Exception e)
                {
                    string test = e.Message;
                }
                previousElement = schemaElement;
                previousFunction = infoString;
                //Console.Out.WriteLine();
            }

            //Console.Out.WriteLine();
            //Console.In.ReadLine();
            pbar.Value = 100;
            return lSFunctions;
        }
        private static void OutputElements(XmlSchemaParticle particle)
        {
            XmlSchemaSequence sequence = particle as XmlSchemaSequence;
            XmlSchemaChoice choice = particle as XmlSchemaChoice;
            XmlSchemaAll all = particle as XmlSchemaAll;

            if (sequence != null)
            {
                Console.Out.WriteLine("  Sequence");

                for (int i = 0; i < sequence.Items.Count; i++)
                {
                    XmlSchemaElement childElement = sequence.Items[i] as XmlSchemaElement;
                    XmlSchemaSequence innerSequence = sequence.Items[i] as XmlSchemaSequence;
                    XmlSchemaChoice innerChoice = sequence.Items[i] as XmlSchemaChoice;
                    XmlSchemaAll innerAll = sequence.Items[i] as XmlSchemaAll;

                    if (childElement != null)
                    {
                        Console.Out.WriteLine("    Element/Type: {0}:{1}", childElement.Name,
                                              childElement.SchemaTypeName.Name);
                    }
                    else OutputElements(sequence.Items[i] as XmlSchemaParticle);
                }
            }
            else if (choice != null)
            {
                Console.Out.WriteLine("  Choice");
                for (int i = 0; i < choice.Items.Count; i++)
                {
                    XmlSchemaElement childElement = choice.Items[i] as XmlSchemaElement;
                    XmlSchemaSequence innerSequence = choice.Items[i] as XmlSchemaSequence;
                    XmlSchemaChoice innerChoice = choice.Items[i] as XmlSchemaChoice;
                    XmlSchemaAll innerAll = choice.Items[i] as XmlSchemaAll;

                    if (childElement != null)
                    {
                        Console.Out.WriteLine("    Element/Type: {0}:{1}", childElement.Name,
                                              childElement.SchemaTypeName.Name);
                    }
                    else OutputElements(choice.Items[i] as XmlSchemaParticle);
                }

                Console.Out.WriteLine();
            }
            else if (all != null)
            {
                Console.Out.WriteLine("  All");
                for (int i = 0; i < all.Items.Count; i++)
                {
                    XmlSchemaElement childElement = all.Items[i] as XmlSchemaElement;
                    XmlSchemaSequence innerSequence = all.Items[i] as XmlSchemaSequence;
                    XmlSchemaChoice innerChoice = all.Items[i] as XmlSchemaChoice;
                    XmlSchemaAll innerAll = all.Items[i] as XmlSchemaAll;

                    if (childElement != null)
                    {
                        Console.Out.WriteLine("    Element/Type: {0}:{1}", childElement.Name,
                                              childElement.SchemaTypeName.Name);
                    }
                    else OutputElements(all.Items[i] as XmlSchemaParticle);
                }
                Console.Out.WriteLine();
            }
        }



        //Evaluate Helpers

        public List<KXWS.SSearchCondition> EvaluateSearchConditions(List<KXWS.SSearchCondition> searchConList, DataGridViewRow row = null)
        {
            //Evaluate Searchconditions
            ExpressionsEvaluator expVal = new ExpressionsEvaluator();
            foreach (KXWS.SSearchCondition sCon in searchConList)
            {
                try
                {
                    string[] sNewValues = new string[sCon.propertyValueArray.Length];
                    int i = 0;
                    foreach (string sValue in sCon.propertyValueArray)
                    {

                        //nicht Evaluarieren wenn "".
                        if (sValue != "")
                        {
                            string sEvaluatedValue  = "";
                            if(row == null)
                                sEvaluatedValue = expVal.Evaluate(sValue);
                            else
                                sEvaluatedValue = expVal.Evaluate(sValue,row);
                            sNewValues[i] = sEvaluatedValue;
                        }
                        else
                            sNewValues[i] = sValue;
                        i++;

                    }
                    sCon.propertyValueArray = sNewValues;
                }
                catch (Exception e)
                {
                    log4.Error("An Error happened while evaluating the SearchProperties." + e.Message);
                    throw e;
                }
            }
            return searchConList;
        }

        public List<KXWS.SDocumentPropertyUpdate> EvaluatePropertyValues(List<KXWS.SDocumentPropertyUpdate> markerProperties, DataGridViewRow row)
        {
            ExpressionsEvaluator expVal = new ExpressionsEvaluator();

            //Go trough all markerProperties.
            foreach (KXWS.SDocumentPropertyUpdate prop in markerProperties)
            {
                string[] sNewValues = new string[prop.propertyValues.Count()];
                int i = 0;
                foreach (string sValue in prop.propertyValues)
                {
                    string sEvaluatedValue = expVal.Evaluate(sValue, row);
                    sNewValues[i] = sEvaluatedValue;
                    i++;
                }
                prop.propertyValues = sNewValues;
            }
            return markerProperties;
        }

        public void CheckPropertyValues(List<KXWS.SDocumentPropertyUpdate> markerProperties, DataGridViewRow row)
        {
            ExpressionsEvaluator expVal = new ExpressionsEvaluator();
            foreach (KXWS.SDocumentPropertyUpdate prop in markerProperties)
            {
                string[] sNewValues = new string[prop.propertyValues.Count()];
                int i = 0;
                foreach (string sValue in prop.propertyValues)
                {
                    expVal.Evaluate(sValue, row, testmode: true);
                    i++;
                }
            }
        }
    }
}
