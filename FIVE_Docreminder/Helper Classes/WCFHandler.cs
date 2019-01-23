using docreminder.BO;
using docreminder.InfoShareService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace docreminder
{
    class WCFHandler
    {
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static WCFHandler instance;
        private static object syncRoot = new Object();
    
        public string ConnectionID { get; private set; }
        private DateTime lastActionTime = DateTime.Now;
        private static TimeSpan connectionTimeOut;

        public AuthenticationService authenticationService { get; private set; }
        public CommonService commonService { get; private set; }
        public SearchService searchService { get; private set; }
        public FileService fileService { get; private set; }
        public DocumentService documentService { get; private set; }
        public ProcessService processService { get; private set; }


        private bool _hasMore = false;
        private string _resumePoint = null;

        public bool hasMore { get { return _hasMore; } set { _hasMore = value; } }
        public string resumePoint { get { return _resumePoint; } set { _resumePoint = value; } }


        public static WCFHandler GetInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new WCFHandler();
                        }
                    }
                }

                return instance;
            }
        }

        private WCFHandler()
        {
            Login();
        }


        #region Connection
        private void SetBindings(string wcfURL)
        {
            //Bindings
            authenticationService = new AuthenticationService(wcfURL);
            commonService = new CommonService(new CommonClient("BasicHttpBinding_Common", wcfURL));
            searchService = new SearchService(new SearchClient("BasicHttpBinding_Search", wcfURL));
            fileService = new FileService(new FileClient("BasicHttpBinding_File", wcfURL));
            documentService = new DocumentService(new DocumentClient("BasicHttpBinding_Document", wcfURL));
            processService = new ProcessService(new ProcessClient("BasicHttpBinding_Process", wcfURL));
        }
        public void Login()
        {
            //SetBindings
            SetBindings(Properties.Settings.Default.KendoxWCFURL);

            log4.Info("Trying to logon with user '" + Properties.Settings.Default.KendoxUsername + "' ...");
            string connID = "";
            try
            {
                //Decrypt Password
                string decryptedPassword = "";
                if (Properties.Settings.Default.isKXPWEncrypted)
                    decryptedPassword = FileHelper.DecryptString(Properties.Settings.Default.KendoxPassword, Properties.Settings.Default.kxpwentropy);
                else
                    decryptedPassword = Properties.Settings.Default.KendoxPassword;


                    LogonResultContract res;
                    res = authenticationService.Logon(Properties.Settings.Default.KendoxUsername, authenticationService.EncodeStringToBase64SHA512(decryptedPassword));
                    connID = res.ConnectionId;
                    connectionTimeOut = TimeSpan.FromSeconds(res.ConnectionTimeoutSeconds);

            }

            catch (Exception e)
            {
                string encrypted = "";
                if (Properties.Settings.Default.isKXPWEncrypted)
                {
                    encrypted = "Password is encrypted, did you maybe change user?";
                }
                log4.Error(e.Message + " " + encrypted);
            }

            if(connID != "")
            { 
                commonService.Init(connID); // load schemaStore, userStore, securityStore only once !!!
                log4.Info(string.Format("Successfully logged on with '{0}' connID '{1}'", Properties.Settings.Default.KendoxUsername, connID));
                ConnectionID = connID;
            }          
        }
        public bool isConnected()
        {
            //We have been connected. But timeout since last action is reached.
            if (ConnectionID != null && (lastActionTime + connectionTimeOut) < DateTime.Now)
            {
                //Refresh connection
                log4.Info("Session-Timeout reached. Refreshing session.");
                Login();
                lastActionTime = DateTime.Now;
                return true;
            }
            if (ConnectionID != null)
            {
                lastActionTime = DateTime.Now;
                return true;
            }
            //We have never been connected
            return false;
        }
        #endregion

        #region Utility
        internal void UndoCheckOutDocument(string infoShareObjectID)
        {
            documentService.UndoCheckOutDocument(ConnectionID, infoShareObjectID);
        }

        internal void CheckOutDocument(string infoShareObjectID)
        {
            documentService.CheckOutDocument(ConnectionID, infoShareObjectID);
        }

        internal void UpdateDocument(DocumentContract documentConctract)
        {
            documentService.UpdateDocument(ConnectionID, documentConctract);
        }

        public string GetPropertyTypeName(string id)
        {
            if (isConnected())
                return commonService.GetPropertyTypeName(id, Properties.Settings.Default.Culture);
            else
                return id;
        }

        public string GetProcessTemplateName(string id)
        {
            if (isConnected())
                return commonService.GetProcessTemplateName(id, Properties.Settings.Default.Culture);
            else
                return id;
        }


        public string GetPropertyTypeID(string name)
        {
            if (isConnected())
                return commonService.GetPropertyTypeID(name, Properties.Settings.Default.Culture);
            else
                return name;
        }

        internal IEnumerable<string> GetAllPropertyTypes(bool editable = false)
        {
            List<string> lSpropertyNames = new List<string>();

            List<PropertyTypeContract> filteredProps;

            PropertyTypeContract[] props = commonService.GetAllPropertyTypes();
            if (!editable)
                filteredProps = props.Where(x => x.Searchable == true).ToList();

            else
                filteredProps = props.Where(x => x.FreeEditable == true).ToList();


            foreach (PropertyTypeContract ptc in filteredProps)
            {
                string name = Utility.GetValue(ptc.Name, Properties.Settings.Default.Culture);
                if (name != null)
                    lSpropertyNames.Add(name);
                else
                {
                    name = ptc.Name.Values.Where(x => x.Text != null).Select(x => x.Text).First();
                    lSpropertyNames.Add(name);
                }
            }
            return lSpropertyNames;
        }

        internal ProcessTemplateContract GetProcessTemplateByName(string processName)
        {
            if (isConnected())
                return commonService.GetProcessTemplateByName(processName, Properties.Settings.Default.Culture);
            else
                return null;
        }

        internal IEnumerable<string> GetAllInfoStores()
        {
            List<string> lSInfoStores = new List<string>();

            InfoStoreContract[] infoStores = commonService.GetAllInfoStores(ConnectionID);

            foreach (InfoStoreContract isc in infoStores)
            {
                string name = Utility.GetValue(isc.Name, Properties.Settings.Default.Culture);
                if (name != null)
                    lSInfoStores.Add(name);
                else
                {
                    name = isc.Name.Values.Where(x => x.Text != null).Select(x => x.Text).First();
                    lSInfoStores.Add(name);
                }
            }
            return lSInfoStores;
        }

        internal IEnumerable<ProcessTemplateContract> GetAllProcessTemplates()
        {
            List<ProcessTemplateContract> lSProcessTemplates = new List<ProcessTemplateContract>();
            return commonService.GetAllProcessTemplates();

            //foreach (ProcessTemplateContract pti in processTemplates)
            //{
            //    string name = Utility.GetValue(pti.ProcessName, Properties.Settings.Default.Culture);
            //    if (name != null)
            //        lSProcessTemplates.Add(name);
            //    else
            //    {
            //        name = pti.Name.Values.Where(x => x.Text != null).Select(x => x.Text).First();
            //        lSProcessTemplates.Add(name);
            //    }
            //}
            //return lSProcessTemplates;
        }

        internal DocumentContract GetDocument(string infoShareObjectID)
        {
            return documentService.GetDocument(ConnectionID, infoShareObjectID);
        }

        internal UserContract[] GetAllUsers()
        {
            UserStoreContract usc = commonService.GetAllUsers(ConnectionID);
            return usc.Users;
        }
        #endregion

        internal DocumentSimpleContract[] SearchForDocuments()
        {
            isConnected();

            log4.Info("Searching for documents...");
            List<DocumentContract> documents = new List<DocumentContract>();

            List<SearchConditionContract> searchConContractList = new List<SearchConditionContract>();
            searchConContractList = (List<SearchConditionContract>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.SearchProperties, searchConContractList.GetType()));  
            searchConContractList = EvaluateSearchConditions(searchConContractList);
            SearchDefinitionContract sDefContract = new SearchDefinitionContract
            {
                Conditions = searchConContractList.ToArray()
            };

            //Set InfoStores to search for.
            //If "All" is selected, "null" is sent.
            List<string> infoStores = new List<string>();
            var infoStoresArray = Properties.Settings.Default.KendoxInfoStores.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if(!infoStoresArray.Contains("All"))
            { 
                foreach (string infoStore in infoStoresArray)
                {
                    infoStores.Add(commonService.GetInfoStoreID(ConnectionID, infoStore, Properties.Settings.Default.Culture));
                }
                sDefContract.SearchStores = infoStores.ToArray();
            }

            //Set PageSize
            sDefContract.PageSize = Properties.Settings.Default.SearchQuantity;


            var resultContract = searchService.SearchDocument(commonService, ConnectionID, sDefContract,_resumePoint);
            _hasMore = resultContract.HasMore;

            if (resultContract.HasMore)
                _resumePoint = resultContract.ResumePoint;
            else
                _resumePoint = null;

            log4.Info(string.Format("Found {0} documents matching the searchproperties.", resultContract.Documents.Count()));

            return resultContract.Documents;
        }

        internal DocumentSimpleContract[] SearchForChildDocuments(DocumentContract parentDoc)
        {
            isConnected();

            //Get Grouping-Search Properties and evaluate them.
            List<SearchConditionContract> searchConContractList = new List<SearchConditionContract>();
            searchConContractList = (List<SearchConditionContract>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.GroupingSearchProperties, searchConContractList.GetType()));
            searchConContractList = EvaluateSearchConditions(searchConContractList, parentDoc);
            SearchDefinitionContract sDefContract = new SearchDefinitionContract
            {
                Conditions = searchConContractList.ToArray()
            };


            //***ADDITIONAL CHECKS FOR SAFETY***            
            //Check against no grouping search-properties.
            if(searchConContractList.Count() < 1)
                throw new Exception("No grouping searchproperties defined! This could result in an accidental sending of an entire archive. This file will thus be ignored!");

            //If grouping searchproperty of parent evaluated to empty (val = "") we could end up sending an entire archive by accident.
            foreach (SearchConditionContract scc in searchConContractList)
            {
                foreach(string val in scc.Values)
                {
                    if (val == "")
                        throw new Exception("Searchproperties for child-document-search were empty. This could result in an accidental sending of an entire archive. This file will thus be ignored!");
                }
            }




            //Set InfoStores to search for.
            //If "All" is selected, "null" is sent.
            List<string> infoStores = new List<string>();
            var infoStoresArray = Properties.Settings.Default.KendoxInfoStores.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (!infoStoresArray.Contains("All"))
            {
                foreach (string infoStore in infoStoresArray)
                {
                    infoStores.Add(commonService.GetInfoStoreID(ConnectionID, infoStore, Properties.Settings.Default.Culture));
                }
                sDefContract.SearchStores = infoStores.ToArray();
            }

            //Set PageSize
            //Maximum of 50 childdocuments would get send.
            sDefContract.PageSize = 50;


            var resultContract = searchService.SearchDocument(commonService, ConnectionID, sDefContract, null);

            return resultContract.Documents;
        }

        internal void StartProcess(ProcessTemplateContract procTemplate, string objectID, string[] userIds)
        {
            ProcessContract process = new ProcessContract(); //Create new emtpy ProcessContract
            process.ProcessTemplateId = procTemplate.Id; //Set processTemplateID
            process.DocumentIds = new string[1] { objectID };
            process = processService.CreateProcess(ConnectionID, process); //Create Process

            //If there are custom userids, assign them to the current task.
            if (userIds.Count() > 0)
            {
                //Start the process with assignUsers = false.
                StartProcessResultContract startResult = processService.StartProcess(ConnectionID, process, false);
                process = startResult.Process;
                //Get CurrentTask
                if (process.CurrentTask != null)
                {
                    //Assign new Users to task.
                    process.CurrentTask.AssignedUserIds = userIds;
                    //Update the process with assigned users, this will start the task automatically.
                    processService.UpdateProcess(ConnectionID, process);
                }
                else
                    throw new Exception(string.Format("The process does not contain a task and could not be started!"));
            }
            //If there are no special recipients just start the task with assignUsers = true 
            //and the server will start the task with standard users defined in the ContractTemplate.
            else
                processService.StartProcess(ConnectionID, process, true);
        }

        internal byte[] GetDocumentFile(string infoShareObjectID)
        {
            return fileService.DownloadFileBytesOnly(ConnectionID, infoShareObjectID);
        }

        public List<SearchConditionContract> EvaluateSearchConditions(List<SearchConditionContract> searchConList, DocumentContract doc = null)
        {
            //Evaluate Searchconditions
            foreach (SearchConditionContract sCon in searchConList)
            {
                try
                {
                    string[] sNewValues = new string[sCon.Values.Length];
                    int i = 0;
                    foreach (string sValue in sCon.Values)
                    {

                        //don't evaluate if ""
                        if (sValue != "")
                        {
                            string sEvaluatedValue = "";
                            sEvaluatedValue = ExpressionsEvaluator.GetInstance.Evaluate(sValue, doc,false,true);
                            sNewValues[i] = sEvaluatedValue;
                        }
                        else
                            sNewValues[i] = sValue;
                        i++;

                    }
                    sCon.Values = sNewValues;
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("An Error happened while evaluating the SearchProperties. Msg: {0}", e.Message));
                }
            }
            return searchConList;
        }


    }
}
