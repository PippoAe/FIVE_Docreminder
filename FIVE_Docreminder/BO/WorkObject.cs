using docreminder.InfoShareService;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace docreminder.BO
{

    class WorkObject
    {
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public DocumentContract document;
        public List<DocumentContract> childDocuments = new List<DocumentContract>();
        public List<DocumentContract> allAffectedDocuments = new List<DocumentContract>();
        private List<MarkerProperty> markerProperties = new List<MarkerProperty>();

        public string objectID { get; private set; }
        public bool ready { get; private set; }
        public bool finished { get; private set; }
        public bool error { get; private set; }
        public int childs { get; private set; }
        public string info { get; private set; }

        private bool prepared = false;



        public WorkObject(string InfoShareObjectID)
        {
            objectID = InfoShareObjectID;
            info = "";

            PrepareForProcessing();
        }

        public bool preparationFinished()
        {
            return prepared;
        }

        private void PrepareForProcessing()
        {
            //Start off as not ready.
            ready = false;
            try
            {
                #region Get the Document
                //Get Document
                try { document = WCFHandler.GetInstance.GetDocument(this.objectID); }
                catch (Exception e) { throw new Exception(string.Format("Couldn't retrieve documentcontract. Msg:'{0}'", e.Message)); }
                #endregion

                #region Check against Additional Computed Identifier
                //Check against AdditionalComputedIdentifier if document is valid for processing.
                if (Properties.Settings.Default.AddCpIdisActive)
                {
                    try
                    {
                        ready = Convert.ToBoolean(ExpressionsEvaluator.GetInstance.Evaluate(Properties.Settings.Default.AdditionalComputedIdentifier, document));
                        if (!ready)
                        {
                            ready = false;
                            string message = string.Format("ACI validation returned false");
                            info += message;
                            finished = true;
                        }
                    }
                    catch (Exception e) { throw new Exception(string.Format("ACI validation failed! Msg:'{0}'", e.Message)); }
                }
                else
                    ready = true;
                #endregion

                #region Get the Child-documents
                if (ready && Properties.Settings.Default.GroupingActive)
                {
                    try
                    {
                        var childDocs = WCFHandler.GetInstance.SearchForChildDocuments(document);
                        
                        foreach (DocumentSimpleContract childDoc in childDocs)
                        {
                            //only add to childs if child!=parent
                            if (childDoc.Id != this.objectID)
                                childDocuments.Add(WCFHandler.GetInstance.GetDocument(childDoc.Id));
                        }

                        if (childDocuments.Count == 0 && !Properties.Settings.Default.GroupingSendWithoutChild)
                        {
                            ready = false;
                            string message = string.Format("Parent document has no child-documents. It will be ignored.");
                            info += message;
                            finished = true;
                        }
                        childs = childDocuments.Count;
                    }
                    catch (Exception e) { throw new Exception(string.Format("Couldn't retrieve child-documents. Msg:'{0}'", e.Message)); }
                }
                #endregion

                #region Check CheckoutState of documents
                //Check checkoutstate of document
                if (ready && document.CheckOutStateEnum != "NotCheckedOut")
                {
                    try
                    {
                        //First try to force-checkin documents.
                        WCFHandler.GetInstance.UndoCheckOutDocument(this.objectID);
                    }
                    catch (Exception e) { throw new Exception(string.Format("Document is checked out! Attempt to undo-checkout failed. Is probably being edited by user. Msg:'{0}'", e.Message)); }
                }

                //Check checkoutstate of childdocuments if Markerproperties get inherited on childdocuments.
                if(ready && Properties.Settings.Default.GroupingInheritMarkerProperties)
                {
                    try
                    {
                        foreach (DocumentContract childDoc in childDocuments)
                        {
                            if(childDoc.CheckOutStateEnum != "NotCheckedOut")
                                try {  WCFHandler.GetInstance.UndoCheckOutDocument(childDoc.Id); }
                                catch(Exception e) { throw new Exception(string.Format("Child-ObjectID:'{0}'. Msg:'{1}'",childDoc.Id, e.Message)); }
                        }
                    }
                    catch (Exception e) { throw new Exception(string.Format("A child-document is checked out! Attempt to undo-checkout failed. Is probably being edited by user. Msg:'{0}'", e.Message)); }
                }
                #endregion

                #region Prepare list of all documents touched by this WorkObject.
                allAffectedDocuments = new List<DocumentContract>();
                allAffectedDocuments.Add(this.document);
                //Only add ChildDocuments if Markerproperties get inherited on childdocuments.
                if (Properties.Settings.Default.GroupingInheritMarkerProperties)
                    allAffectedDocuments.AddRange(childDocuments);
                #endregion

                #region Markerproperties and new DocumentContracts
                if (ready)
                {
                    try
                    {
                        //Get Markerproperties from settings.
                        markerProperties = (List<BO.MarkerProperty>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.NEWMarkerProperties, markerProperties.GetType()));

                        //Evaluate MarkerProperty-Values
                        foreach (MarkerProperty mProp in markerProperties)
                        {
                            //Evaluate markerproperty
                            for (int i = 0; i < mProp.values.Length; i++)
                            {
                                mProp.values[i] = ExpressionsEvaluator.GetInstance.Evaluate(mProp.values[i], this.document,false,true);
                            }
                        }

                        //Merge propertyupdates into all affected documents
                        foreach (DocumentContract doc in allAffectedDocuments)
                        {
                            MergeMarkerPropertiesToDocument(doc);
                        }
                    }
                    catch (Exception e) { throw new Exception(string.Format("Markerproperties could not be prepared! Msg: {0}", e.Message)); }
                }
                #endregion
            }

            catch (Exception e)
            {
                string message = string.Format("There was a problem preparing the document for processing. ObjectID:'{0}', Msg:'{1}'", objectID, e.Message);
                log4.Error(message);
                ready = false;
                error = true;
                info = message;
            }
            finally
            {
                prepared = true;
            }
        }

        public void Process()
        {
            if (ready && !error)
            {
                try
                {
                    log4.Info(string.Format("Processing Document. ObjectID:'{0}'", objectID));
                    string mailRecipient = null;
                    string docSafeRecipient = null;
                    WCFHandler wCFHandler = WCFHandler.GetInstance;



                    #region Checkout all documents needed during processing.
                    //Try to checkout document.
                    try
                    {
                        wCFHandler.CheckOutDocument(this.objectID);
                    }
                    catch (Exception e) { throw new Exception(string.Format("Document could not be checked out. Msg:'{0}'", e.Message)); }

                    if (Properties.Settings.Default.GroupingInheritMarkerProperties)
                    {
                        try
                        {
                            foreach (DocumentContract childDoc in childDocuments)
                            {
                                try { wCFHandler.CheckOutDocument(childDoc.Id); }
                                catch (Exception e) { throw new Exception(string.Format("Child-ObjectID:'{0}'. Msg:'{1}'", childDoc.Id, e.Message)); }
                            }
                        }
                        catch (Exception e) { throw new Exception(string.Format("Child-Document could not be checked out. Msg:'{0}'", e.Message)); }
                    }
                    #endregion

                    #region Send E-Mail
                    if(Properties.Settings.Default.SendMailActive)
                    {
                        string attachmentDirectory = "";
                        try
                        {
                            DateTime startOfDownload = DateTime.Now;


                            if (Properties.Settings.Default.AttachDocument)
                            {
                                //Create temp-directory.
                                string tempDirectory = Path.Combine(Path.GetTempPath(), Convert.ToString(Guid.NewGuid()));
                                Directory.CreateDirectory(tempDirectory);

                                //Prepare list of all documents to send.
                                List<DocumentContract> documentsToSend = new List<DocumentContract>();
                                if(!Properties.Settings.Default.GroupingActive)
                                    documentsToSend.Add(this.document);
                                else
                                {
                                    documentsToSend.AddRange(childDocuments);
                                    if (Properties.Settings.Default.GroupingAddParent)
                                        documentsToSend.Add(this.document);
                                }

                                foreach (DocumentContract doc in documentsToSend)
                                {
                                    byte[] docBytes = wCFHandler.GetDocumentFile(this.objectID);

                                    string fileName = Properties.Settings.Default.AttachmentRenameProperty;
                                    if (fileName != "")
                                    {
                                        fileName = ExpressionsEvaluator.GetInstance.Evaluate(fileName, doc);
                                        if (fileName == "")
                                            fileName = doc.Name;
                                        else
                                        {
                                            string fileExtension = Path.GetExtension(doc.Name);
                                            fileName = fileName + fileExtension;
                                        }
                                    }
                                    else
                                        fileName = doc.Name;


                                    fileName = FileHelper.CleanUpFileName(fileName);
                                    string path = Path.Combine(tempDirectory, fileName);
                                    path = FileHelper.GetUniqueFilePath(path);
                                    System.IO.File.WriteAllBytes(path, docBytes);

                                }
                                DateTime endOfDownload = DateTime.Now;
                                log4.Debug(string.Format("Downloading documents took {0}ms. ObjectID:'{1}'", (endOfDownload - startOfDownload).TotalMilliseconds.ToString(), this.objectID));

                                if (Properties.Settings.Default.GroupingZipped)
                                {
                                    //Create directory withing tempDirectory.
                                    string dirName = new DirectoryInfo(@tempDirectory).Name;

                                    //Create name of ZipFile
                                    string zipName = "";
                                    if (Properties.Settings.Default.GroupingZipName != "")
                                        zipName = Properties.Settings.Default.GroupingZipName + ".zip";
                                    else
                                        zipName = "Documents.zip";

                                    //Create ZipFilePath
                                    string zipTempDirectory = Path.Combine(Path.GetTempPath(), Convert.ToString(Guid.NewGuid()));
                                    Directory.CreateDirectory(zipTempDirectory);
                                    string zipFilePath = Path.Combine(zipTempDirectory, zipName);

                                    //Create Zip-File.
                                    ZipFile.CreateFromDirectory(tempDirectory, zipFilePath);

                                    //Delete old Tempdirectory
                                    Directory.Delete(tempDirectory,true);

                                    //Set new tempdirectory where ZIP file is.
                                    tempDirectory = zipTempDirectory;

                                    DateTime endOfZipping = DateTime.Now;
                                    log4.Debug(string.Format("Zipping took {0}ms. ObjectID:'{1}'", (endOfZipping - endOfDownload).TotalMilliseconds.ToString(), this.objectID));
                                }
                                attachmentDirectory = tempDirectory;
                            }

                            try { 
                                using (Task<string> AsyncTaskSendMail = MailHandler.GetInstance.SendDocumentMail(this.document, attachmentDirectory))
                                {
                                    mailRecipient = AsyncTaskSendMail.Result;
                                }
                            }
                            catch(Exception e)
                            {
                                throw new Exception(string.Format(e.InnerException.Message));
                            }


                            if (attachmentDirectory != "")
                                Directory.Delete(attachmentDirectory, true);

                            #region OldStuff
                            ////If no grouping is active.
                            //if (!Properties.Settings.Default.GroupingActive)
                            //{
                            //    document = WCFHandler.GetInstance.GetDocumentFile(this.objectID);
                            //    string fileName = Properties.Settings.Default.AttachmentRenameProperty;

                            //    if (fileName != "")
                            //    {
                            //        fileName = NEWExpressionsEvaluator.GetInstance.Evaluate(fileName, this.document);
                            //        if (fileName == "")
                            //            fileName = this.document.Name;
                            //        else
                            //        {
                            //            string fileExtension = Path.GetExtension(this.document.Name);
                            //            fileName = fileName + fileExtension;
                            //        }
                            //    }
                            //    else
                            //        fileName = this.document.Name;


                            //    fileName = FileHelper.CleanUpFileName(fileName);

                            //    //docinfo.fileName = fileName;
                            //}

                            ////If Grouping is active.
                            //else
                            //{
                            //}


                            //11.02.2016 AEPH: Get UserList 
                            //if (lUsers == null)
                            //    lUsers = getAllUsers();

                            //using (Task<bool> AsyncTaskSendMail = MailHandler.GetInstance.SendDocumentMail(document, docinfo, row, lUsers, attachmentDirectory))
                            //{
                            //    processed = AsyncTaskSendMail.Result;
                            //}

                            //    using (Task<bool> AsyncTaskSendMail = MailHandler.GetInstance.SendDocumentMail(this.document, attachmentDirectory))
                            //        {
                            //            bool processed = AsyncTaskSendMail.Result;
                            //            if (!processed)
                            //                throw new Exception(string.Format("Document E-Mail could not be sent."));
                            //        }


                            //        //MailHandler.GetInstance.SendDocumentMail(this.document, attachmentDirectory);

                            //        if (attachmentDirectory != "")
                            //            Directory.Delete(attachmentDirectory, true);
                            //    }
                            #endregion
                        }
                        catch (Exception e) { throw new Exception(string.Format("Document E-Mail could not be sent.  Msg:'{0}'",e.Message)); }
                    }
                    #endregion

                    #region Start Process
                    if (Properties.Settings.Default.StartProcessActive)
                    { 
                        try
                        {

                        }
                        catch (Exception e) { throw new Exception("Process could not be started."); }
                    }
                    #endregion

                    #region Send to DocSafe 
                    if (Properties.Settings.Default.DocSafeActive)
                    {
                        try
                        {
                            DocSafe.DocSafeHandler dsHandler = new DocSafe.DocSafeHandler();
                            byte[] document = null;
                            //document = WebService.GetDocumentFile(sSessionGuid, Properties.Settings.Default.Culture, Properties.Settings.Default.Culture, docGuid, null, null, KXWS.AccessTypesEnum.ContentExport, out docinfo);
                            document = wCFHandler.GetDocumentFile(this.objectID);
                            docSafeRecipient = dsHandler.SendDocumentToDocsafe(document, this.document);
                        }
                        catch (Exception e) { throw new Exception("Document E-Mail could not be sent."); }
                    }
                    #endregion

                    #region Set Markerproperties
                    try
                    { 
                        foreach (DocumentContract doc in allAffectedDocuments)
                        {
                            WCFHandler.GetInstance.UpdateDocument(doc);
                        }
                    }
                    catch (Exception e) { throw new Exception(string.Format("Markerproperties could not be set! Msg:'{0}'", e.Message)); }

                    #endregion

                    string message = string.Format("Document processed sucessfully. ObjectID:'{0}' {1} {2}", this.objectID, mailRecipient != null ? "E-Mail Recipient: '" + mailRecipient+"'" : "", docSafeRecipient != null ? "DocSafe Recipient: '" + docSafeRecipient + "'" : "");
                    log4.Info(message);
                    info = message;
                    finished = true;
                }
                //If an error happens during processing.
                catch (Exception e)
                {
                    string message = string.Format("An error happened during documentprocessing! ObjectID:'{0}', Msg:'{1}'", this.objectID, e.Message);
                    log4.Error(message);
                    info = message;
                    error = true;
                }
                //Documents are always checked back in in when finished.
                finally
                {
                    try
                    {
                        foreach (DocumentContract doc in allAffectedDocuments)
                        {
                            WCFHandler.GetInstance.UndoCheckOutDocument(doc.Id);
                        }
                    }
                    catch { }
                }
            }
        }

        public void AbortProcessing(string reasonForAbortion)
        {
            string message = string.Format("Document processing aborted. ObjectID:'{0}', Msg:'{1}'", objectID, reasonForAbortion);
            log4.Error(message);
            ready = false;
            error = true;
            info = message;
        }

        #region Utility
        public string GetPropertyValueFromName(string propertyTypeName)
        {
            string propId = WCFHandler.GetInstance.GetPropertyTypeID(propertyTypeName);
            foreach (PropertyContract prop in document.Properties)
            {
                if (prop.PropertyTypeId == propId)
                    return prop.Values[0];
            }
            return "";
        }

        public void MergeMarkerPropertiesToDocument(DocumentContract doc)
        {
            //Merge new properties to current document properties.
            foreach(MarkerProperty mProp in markerProperties)
            { 
            var prop = doc.Properties.Where(x => x.PropertyTypeId == mProp.propertyTypeID).FirstOrDefault();
            if (mProp.updateAction == MarkerProperty.UpdateAction.ADD)
            {
                //If prop exists. Add at the end.
                if (prop != null)
                {
                    List<string> vals = prop.Values.ToList();
                    vals.AddRange(mProp.values);
                    prop.Values = vals.ToArray();
                }
                //Create new
                else
                {
                    PropertyContract newProp = new PropertyContract()
                    {
                        PropertyTypeId = mProp.propertyTypeID,
                        Values = mProp.values
                    };
                    var props = this.document.Properties.ToList();
                    props.Add(newProp);
                    this.document.Properties = props.ToArray();
                }
            }
            if (mProp.updateAction == MarkerProperty.UpdateAction.UPDATE)
            {
                //If prop exists. Replace values.
                if (prop != null)
                {
                    prop.Values = mProp.values.ToArray();
                }
                //Create new
                else
                {
                    PropertyContract newProp = new PropertyContract()
                    {
                        PropertyTypeId = mProp.propertyTypeID,
                        Values = mProp.values
                    };
                    var props = this.document.Properties.ToList();
                    props.Add(newProp);
                    this.document.Properties = props.ToArray();
                }
            }
            if (mProp.updateAction == MarkerProperty.UpdateAction.DELETE)
            {
                //Remove property if it exists.
                if (prop != null)
                {
                    var props = this.document.Properties.ToList();
                    props.Remove(prop);
                    this.document.Properties = props.ToArray();
                }
            }
            if (mProp.updateAction == MarkerProperty.UpdateAction.NONE)
            {
                //Do Nothing
            }
            }
        }
        #endregion 

    }
}
