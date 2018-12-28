using docreminder.InfoShareService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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



        public WorkObject(string InfoShareObjectID)
        {
            objectID = InfoShareObjectID;
            info = "";

            PrepareForProcessing();
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
                        ready = Convert.ToBoolean(NEWExpressionsEvaluator.GetInstance.Evaluate(Properties.Settings.Default.AdditionalComputedIdentifier, document));
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
                                mProp.values[i] = NEWExpressionsEvaluator.GetInstance.Evaluate(mProp.values[i], this.document);
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
        }

        public void Process()
        {
            if (ready && !error)
            {
                try
                {
                    log4.Info(string.Format("Processing Document. ObjectID:'{0}'", objectID));

                    #region Checkout all documents needed during processing.
                    //Try to checkout document.
                    try
                    {
                        WCFHandler.GetInstance.CheckOutDocument(this.objectID);
                    }
                    catch (Exception e) { throw new Exception("Document could not be checked out."); }

                    if (Properties.Settings.Default.GroupingInheritMarkerProperties)
                    {
                        try
                        {
                            foreach (DocumentContract childDoc in childDocuments)
                            {
                                try { WCFHandler.GetInstance.CheckOutDocument(childDoc.Id); }
                                catch (Exception e) { throw new Exception(string.Format("Child-ObjectID:'{0}'. Msg:'{1}'", childDoc.Id, e.Message)); }
                            }
                        }
                        catch (Exception e) { throw new Exception(string.Format("Child-Document could not be checked out. Msg:'{0}'", e.Message)); }
                    }
                    #endregion

                    #region Set Markerproperty
                    foreach(DocumentContract doc in allAffectedDocuments)
                    {
                        WCFHandler.GetInstance.UpdateDocument(doc);
                    }

                    #endregion 
                    string message = string.Format("Document processed sucessfully. ObjectID:'{0}'", this.objectID);
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
                    //WCFHandler.GetInstance.UndoCheckOutDocument(this.objectID);
                    //List<DocumentContract> allDocs = new List<DocumentContract>();
                    //allDocs.Add(this.document);
                    ////Only check ChildDocuments if Markerproperties get inherited on childdocuments.
                    //if (Properties.Settings.Default.GroupingInheritMarkerProperties)
                    //    allDocs.AddRange(childDocuments);
                    //foreach (DocumentContract doc in allDocs)
                    //{
                    //    WCFHandler.GetInstance.UndoCheckOutDocument(doc.Id);
                    //}
                }
            }
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
