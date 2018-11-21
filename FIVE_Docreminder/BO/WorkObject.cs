using docreminder.InfoShareService;
using System;

namespace docreminder.BO
{

    class WorkObject
    {
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public DocumentContract document;
        public WCFHandler _wcfHandler;
        public bool finished { get; private set; }
        public string error { get; private set; }
        public string objectID { get;  private set; }

        public WorkObject(string InfoShareObjectID, WCFHandler wcfHandler)
        {
            
            objectID = InfoShareObjectID;
            _wcfHandler = wcfHandler;

            try
            {
                document = _wcfHandler.documentService.GetDocument(wcfHandler.ConnectionID, InfoShareObjectID);
            }
            catch(Exception e)
            {
                log4.Error(string.Format("Couldn't retrieve documentcontract for doucment '{0}' Message: {1}", objectID,e.Message));
            }
            
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
            Random rndG = new Random();
            int rnd1 = rndG.Next(0, 1000);
            int rnd2 = rndG.Next(0, 1000);
            string evalInput = rnd1.ToString() +"+"+rnd2.ToString();

            log4.Debug(string.Format("Evaluating expression '{0}' for document '{1}'.", evalInput, objectID));
            var evalOutput = NEWExpressionsEvaluator.Evaluate(evalInput);
            log4.Debug(string.Format("Evaluated '{0}' to '{1}'.", evalInput, evalOutput));
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
