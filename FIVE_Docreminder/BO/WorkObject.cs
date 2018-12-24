using docreminder.InfoShareService;
using System;
using System.Threading;

namespace docreminder.BO
{

    class WorkObject
    {
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public DocumentContract document;
        public string objectID { get; private set; }
        public bool ready { get; private set; }
        public bool finished { get; private set; }
        public bool error { get; private set; }
        public string info { get; private set; }
        

        public WorkObject(string InfoShareObjectID)
        { 
            objectID = InfoShareObjectID;
            info = "";

            try
            {
                document = WCFHandler.GetInstance.GetDocument(InfoShareObjectID);
            }
            catch(Exception e)
            {
                string message = string.Format("Couldn't retrieve documentcontract. ObjectID:'{0}', Msg:'{1}'", objectID, e.Message);
                log4.Error(message);
                error = true;
                info = message;
            }

            PrepareForProcessing();
        }

        #region processing
        private void PrepareForProcessing()
        {
            //Start off as not ready.
            ready = false;

            //Check against AdditionalComputedIdentifier if document is valid for processing.
            if (Properties.Settings.Default.AddCpIdisActive)
            {
                try
                {
                    ready = Convert.ToBoolean(NEWExpressionsEvaluator.GetInstance.Evaluate(Properties.Settings.Default.AdditionalComputedIdentifier, document));
                    if (!ready)
                    {
                        ready = false;
                        string message = string.Format("ACI validation returned false", this.objectID);
                        info += message;
                        finished = true;
                    }
                }
                catch (Exception e)
                {
                    string message = string.Format("ACI validation failed! ObjectID:'{0}', Msg:'{1}'", this.objectID, e.Message);
                    log4.Error(message);
                    info = message;
                    error = true;
                }
            }
            else
                ready = true;

            //Check checkoutstate of document
            if (ready && document.CheckOutStateEnum != "NotCheckedOut")
            {
                try { 
                //First try to force-checkin documents.
                WCFHandler.GetInstance.UndoCheckOutDocument(this.objectID);
                }
                catch
                {
                    ready = false;
                    string message = string.Format("Document is checked out! Attempt to undo-checkout failed. Is probably being edited by user.", this.objectID);
                    info += message;
                    finished = true;
                }
            }
        }

        //MOCKUP PROCESSING
        public void Process()
        {
            if(ready && !error)
            {
                try
                {
                    log4.Info(string.Format("Processing Document {0}", objectID));

                    #region EvaluateMarkerProperties
                    //WCFHandler.GetInstance.documentService.UpdateDocumentProperty()
                    DocumentContract newDocContract = this.document;
                    //document.Properties[0].
                    //List<KXWS.SDocumentPropertyUpdate> markerProperties = new List<KXWS.SDocumentPropertyUpdate>();
                    //markerProperties = (List<KXWS.SDocumentPropertyUpdate>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.KendoxMarkerProperties, markerProperties.GetType()));
                    //KXWS.SDocumentPropertyUpdate[] updatePropList;
                    //WCFHandler.GetInstance.documentService.Update
                    #endregion

                    #region Checkout Document
                    //Try to checkout document.
                    try
                        {
                            WCFHandler.GetInstance.CheckOutDocument(this.objectID);
                        }
                    catch(Exception e)
                        {
                            throw new Exception("Document could not be checked out.");
                        }
                    #endregion




                    //Random rand = new Random();
                    //int workingtime = rand.Next(100, 10000);
                    //Thread.Sleep(workingtime);


                    //Random rand2 = new Random();
                    //var err = rand2.Next(0, 10);
                    //if(err == 0)
                    //{
                    //    throw new Exception("RANDOM ERROR");
                    //}

                    string message = string.Format("Document processed sucessfully. ObjectID:'{0}'", this.objectID);
                    log4.Info(message);
                    info = message;
                    finished = true;
                }
                //If an error happens during processing.
                catch(Exception e)
                {
                    string message = string.Format("An error happened during documentprocessing! ObjectID:'{0}', Msg:'{1}'", this.objectID, e.Message);
                    log4.Error(message);
                    info = message;
                    error = true;
                }
                //Documents are always checkedbackin in when finished.
                finally
                {
                    WCFHandler.GetInstance.UndoCheckOutDocument(this.objectID);
                }
            }
        }


        private void Update()
        {

        }

        private void Send()
        {

        }

        #endregion

        #region utility
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

        public string GetPropertyValueFromID(string propertyID)
        {
            return "";
        }

        private void GetDocumentBytes()
        {

        }
        #endregion 

    }
}
