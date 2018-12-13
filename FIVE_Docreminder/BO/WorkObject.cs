using docreminder.InfoShareService;
using System;

namespace docreminder.BO
{

    class WorkObject
    {
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public DocumentContract document;
        public string objectID { get; private set; }
        public bool isValid { get; private set; }
        public bool finished { get; private set; }
        public string error { get; private set; }
        

        public WorkObject(string InfoShareObjectID)
        { 
            objectID = InfoShareObjectID;

            try
            {
                document = WCFHandler.GetInstance.GetDocument(InfoShareObjectID);
            }
            catch(Exception e)
            {
                log4.Error(string.Format("Couldn't retrieve documentcontract for document '{0}' Message: {1}", objectID,e.Message));
                error = string.Format("Couldn't retrieve documentcontract for document");
                isValid = false;
                finished = true;
            }

            if (Properties.Settings.Default.AddCpIdisActive)
                PrepareForProcessing();
        }

        public string GetPropertyValueFromName(string propertyTypeName)
        {
            string propId = WCFHandler.GetInstance.GetPropertyTypeID(propertyTypeName);
            foreach(PropertyContract prop in document.Properties)
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


        public void Process()
        {
            log4.Debug(string.Format("Processing Document {0}", objectID));
            Evaluate();
            Update();
            error = "";
            finished = true;
        }

        private void Evaluate()
        {
            //Random rndG = new Random();
            //int rnd1 = rndG.Next(0, 1000);
            //int rnd2 = rndG.Next(0, 1000);
            //string evalInput = rnd1.ToString() +"+"+rnd2.ToString();

            //log4.Debug(string.Format("Evaluating expression '{0}' for document '{1}'.", evalInput, objectID));
            //var evalOutput = NEWExpressionsEvaluator.Evaluate(evalInput);
            //log4.Debug(string.Format("Evaluated '{0}' to '{1}'.", evalInput, evalOutput));
        }

        private void PrepareForProcessing()
        {
            try
            {
                if (Convert.ToBoolean(NEWExpressionsEvaluator.GetInstance.Evaluate(Properties.Settings.Default.AdditionalComputedIdentifier, document)))
                {
                    isValid = true;
                }
                else
                {
                    isValid = false;
                    finished = true;
                }
            }
            catch (Exception e)
            {
                string message = "An Error happened while validating additional computed identifier!" + e.Message;
                error = message;
                log4.Info(message);
                finished = true;
                isValid = false;
            }
        }


        private void Update()
        {

        }

        private void Send()
        {

        }

        private void GetDocumentBytes()
        {

        }

    }
}
