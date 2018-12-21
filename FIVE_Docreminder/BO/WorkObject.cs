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
            //Start off as ready
            ready = true;

            //Check against AdditionalComputedIdentifier if document is valid for processing.
            if (Properties.Settings.Default.AddCpIdisActive)
            {
                try
                {
                    var isValid = Convert.ToBoolean(NEWExpressionsEvaluator.GetInstance.Evaluate(Properties.Settings.Default.AdditionalComputedIdentifier, document));
                    if (!isValid)
                    {
                        ready = false;
                        string message = string.Format("ACI = false ", this.objectID);
                        info += message;
                        finished = true;
                    }
                }
                catch (Exception e)
                {
                    ready = false;
                    string message = string.Format("ACI validation failed! ObjectID:'{0}', Msg:'{1}'", this.objectID, e.Message);
                    log4.Error(message);
                    info = message;
                    error = true;
                }
            }

            //Check checkoutstate of document
            if (document.CheckOutStateEnum != "NotCheckedOut")
            {
                ready = false;
                string message = string.Format("Document is checked out! ", this.objectID);
                info += message;
                finished = true;
            }
        }

        //MOCKUP PROCESSING
        public void Process()
        {
            if(ready && !error)
            { 
                log4.Info(string.Format("Processing Document {0}", objectID));
                Random rand = new Random();
                int workingtime = rand.Next(100, 10000);
                Thread.Sleep(workingtime);


                Random rand2 = new Random();
                var err = rand2.Next(0, 10);
                if(err == 0)
                {
                    string message = string.Format("An error happened during documentprocessing! ObjectID:'{0}', Msg:'{1}'", this.objectID, "ERROROROROROROR");
                    log4.Error(message);
                    info = message;
                    error = true;
                }
                else
                {
                    string message = string.Format("Document processed sucessfully. ObjectID:'{0}'", this.objectID);
                    log4.Info(message);
                    info = message;
                    finished = true;
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
