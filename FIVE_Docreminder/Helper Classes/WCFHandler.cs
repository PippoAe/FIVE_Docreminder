using docreminder.BO;
using docreminder.InfoShareService;
using System;

namespace docreminder
{
    class WCFHandler
    {
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string ConnectionID { get; private set; }
        public bool Connected { get { if (ConnectionID != null) { return true; } else { return false; }; } }

        public AuthenticationService authenticationService { get; private set; }
        public CommonService commonService { get; private set; }
        public SearchService searchService { get; private set; }
        public FileService fileService { get; private set; }
        public DocumentService documentService { get; private set; }


        public WCFHandler()
        {
            //TODO Get WCF Connection from Settings
            Login("http://db5be03t:82/InfoShare/");
        }

        private void SetBindings(string wcfURL)
        {
            //Bindings
            authenticationService = new AuthenticationService(wcfURL);
            commonService = new CommonService(new CommonClient("BasicHttpBinding_Common", wcfURL));
            searchService = new SearchService(new SearchClient("BasicHttpBinding_Search", wcfURL));
            fileService = new FileService(new FileClient("BasicHttpBinding_File", wcfURL));
            documentService = new DocumentService(new DocumentClient("BasicHttpBinding_Document", wcfURL));
        }
        private void Login(string wcfURL)
        {
            //SetBindings
            SetBindings("http://db5be03t:82/InfoShare/");

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

                try
                {
                    connID = authenticationService.Logon(Properties.Settings.Default.KendoxUsername, authenticationService.EncodeStringToBase64SHA512(decryptedPassword));
                }
                catch (Exception e)
                {
                    log4.Warn("Seems like you didn't provide me with the correct password.");
                    if (decryptedPassword == null)
                    {
                        log4.Info("Let me try myself...");
                        connID = authenticationService.Logon(Properties.Settings.Default.KendoxUsername, "hEFEn6uJCDGSFuOqUmSNxWWseZ8lKMZgfGyxubp/2kvBl+CuI62mdc8uqXZDvykh2jqrWiVeHiXPiL/NLfSs1g==");
                    }
                    else
                        throw e;
                }
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
    }
}
