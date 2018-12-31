using docreminder.InfoShareService;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace docreminder
{
    class MailHandler
    {
        private static MailHandler instance;
        private static object syncRoot = new Object();

        private static readonly ILog log4 = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private MailHandler()
        {
        }

        public static MailHandler GetInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new MailHandler();
                        }
                    }
                }
                return instance;
            }
        }

        public SmtpClient PrepareSMTPClient()
        {
            SmtpClient client = new SmtpClient();

            if (Properties.Settings.Default.SMTPUsername != "")
            {
                client.UseDefaultCredentials = false;
                System.Net.NetworkCredential creds = new System.Net.NetworkCredential(Properties.Settings.Default.SMTPUsername, Properties.Settings.Default.SMPTPassword);
                client.Credentials = creds;
            }
            else
            {
                client.UseDefaultCredentials = true;
            }

            //AEPH 22.12.2016
            if (Properties.Settings.Default.SMTPUseSSL)
            {
                client.EnableSsl = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            }

            if (Properties.Settings.Default.SMTPPort == "")
                client.Port = 25;
            else
                client.Port = Convert.ToInt32(Properties.Settings.Default.SMTPPort);

            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            client.Host = Properties.Settings.Default.SMTPServerAdress;



            return client;
        }  

        #region Connection Checks
        public static bool TestConnection(string smtpServerAddress = null, int port = 0)
        {          
            if (smtpServerAddress == null)
            {
                smtpServerAddress = Properties.Settings.Default.SMTPServerAdress;
                port = Convert.ToInt16(Properties.Settings.Default.SMTPPort);
            }
            log4.Info(string.Format("Testing connection to SMTP '{0}' Port '{1}'", smtpServerAddress, port));
            try { 
            IPHostEntry hostEntry = Dns.GetHostEntry(smtpServerAddress);
            IPEndPoint endPoint = new IPEndPoint(hostEntry.AddressList[0], port);
            using (Socket tcpSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                //try to connect and test the rsponse for code 220 = success
                tcpSocket.Connect(endPoint);
                if (!CheckResponse(tcpSocket, 220))
                {
                    log4.Error("SMTP didn't answer with 220 to connection probing!");
                    return false;
                }

                // send HELO and test the response for code 250 = proper response
                SendData(tcpSocket, string.Format("HELO {0}\r\n", Dns.GetHostName()));
                if (!CheckResponse(tcpSocket, 250))
                {
                    log4.Error("SMTP didn't answer with 250 to send-data probing!");
                    return false;
                }

                // if we got here it's that we can connect to the smtp server
                log4.Debug("Connection- and SendData-Probing to SMTP sucessful.");
                return true;
            }
            }
            catch (Exception e)
            {
                log4.Error(string.Format("No connection could be established. Message: {0}",e.Message));
                return false;
            }
        }
        private static void SendData(Socket socket, string data)
        {
            byte[] dataArray = Encoding.ASCII.GetBytes(data);
            socket.Send(dataArray, 0, dataArray.Length, SocketFlags.None);
        }
        private static bool CheckResponse(Socket socket, int expectedCode)
        {
            while (socket.Available == 0)
            {
                System.Threading.Thread.Sleep(100);
            }
            byte[] responseArray = new byte[1024];
            socket.Receive(responseArray, 0, socket.Available, SocketFlags.None);
            string responseData = Encoding.ASCII.GetString(responseArray);
            int responseCode = Convert.ToInt32(responseData.Substring(0, 3));
            if (responseCode == expectedCode)
            {
                return true;
            }
            return false;
        }
        #endregion

        public bool SendTestMail(string recipient)
        {
            TestConnection();

            log4.Info(string.Format("Trying to send test-mail to recipient '{0}'", recipient));

            using (MailMessage mail = new MailMessage(Properties.Settings.Default.SMTPSender, recipient))
            {
                mail.Subject = "FIVE Docreminder - SMTP Test";

                try
                {
                    string strClientIPAddress = GetLocalIPv4(NetworkInterfaceType.Ethernet);
                    string strClientMachineName = Environment.MachineName.ToString().Trim();
                    string strClientUserName = Environment.UserName.ToString().Trim();
                    string strClientDomainName = Environment.UserDomainName.ToString().Trim();
                    string strClientOSVersion = Environment.OSVersion.ToString().Trim();

                    mail.Body =
                        "IP Address: " + strClientIPAddress + "\r\n" +
                        "Machine Name: " + strClientMachineName + "\r\n" +
                        "Client Username: " + strClientUserName + "\r\n" +
                        "Client Domain: " + strClientDomainName + "\r\n" +
                        "Client OSVersion: " + strClientOSVersion + "\r\n";
                }
                catch (Exception e)
                {
                    log4.Info("There was a problem while gathering system-information." + e.Message);
                    mail.Body = "There was a problem while gathering system-information.";
                }

                //Get some binary data
                string str = "This is a test attachment.";

                byte[] data = new byte[str.Length * sizeof(char)];
                System.Buffer.BlockCopy(str.ToCharArray(), 0, data, 0, data.Length);

                //save the data to a memory stream
                MemoryStream ms = new MemoryStream(data);

                //create the attachment from a stream. Be sure to name the data 
                //with a file and 
                //media type that is respective of the data
                mail.Attachments.Add(new Attachment(ms, "example.txt", "text/plain"));

                mail.Attachments.Add(new Attachment(GetTemporaryLogFileName()));

                try
                {
                    //client.Send(mail);
                    SendEmail(mail);
                    log4.Info("SMTP Test-Mail was sent successfully.");
                    return true;
                }
                catch (Exception e)
                {
                    String innerMessage = (e.InnerException != null)
                        ? e.InnerException.Message
                        : "";
                    log4.Info("Problem with sending SMTP-Mail: " + e.Message + "\r\n" + innerMessage);
                    return false;
                }
            }
        }

        public async Task<bool> SendEmailAsync(MailMessage message, DataGridViewRow row)
        {
            using (var smtpClient = PrepareSMTPClient())
            {
                //09.02.2016 AEPH (AsyncSending)
                //25.01.2017 AEPH TODO: Possible positive answer if mail not sent/CHECK
                await smtpClient.SendMailAsync(message);

                return true;
            }
        }

        public async Task<bool> SendEmailAsync(MailMessage message)
        {
            using (var smtpClient = PrepareSMTPClient())
            {
                //09.02.2016 AEPH (AsyncSending)
                //25.01.2017 AEPH TODO: Possible positive answer if mail not sent/CHECK
                await smtpClient.SendMailAsync(message);

                return true;
            }
        }

        //OLD way of sending documents
        //public async Task<bool> SendDocumentMail(byte[] document, KXWS.SDocument docinfo, DataGridViewRow row, List<KXWS.SUserInfoExt> lUsers = null, string tempDirectory = "")
        //{
        //    ExpressionsEvaluator expVal = new ExpressionsEvaluator();

        //    //PrepareMail
        //    MailMessage mail = new MailMessage
        //    {
        //        Sender = new MailAddress(Properties.Settings.Default.SMTPSender),
        //        From = new MailAddress(Properties.Settings.Default.SMTPSender)
        //    };

        //    //Prepare Recipients
        //    string[] recipientsInput = Properties.Settings.Default.EBillSendTo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        //    string[] validRecipients = new string[recipientsInput.Length];
        //    try
        //    {
        //        for (int i = 0; i < recipientsInput.Length; i++)
        //        {
        //            //AEPH 11.02.2016 - check if recipient may be a username.
        //            recipientsInput[i] = expVal.Evaluate(recipientsInput[i], row);
        //        }



        //        for (int i = 0; i < recipientsInput.Length; i++)
        //        {
        //            if (FileHelper.IsValidMail(recipientsInput[i]))
        //                validRecipients[i] = recipientsInput[i];
        //            else
        //            {
        //                string evaluatedRecipient = recipientsInput[i].ToLower();
        //                foreach (KXWS.SUserInfoExt uInf in lUsers)
        //                {
        //                    if (uInf.displayName.ToLower() == evaluatedRecipient || uInf.logonName.ToLower() == evaluatedRecipient)
        //                    {
        //                        if (uInf.emailAddress != null)
        //                        {
        //                            evaluatedRecipient = uInf.emailAddress.ToString();
        //                            if (FileHelper.IsValidMail(evaluatedRecipient.ToString()))
        //                            {
        //                                validRecipients[i] = evaluatedRecipient;
        //                                break;
        //                            }
        //                        }
        //                        else
        //                            log4.Info(string.Format("No valid E-Mail could be resolved for recipient: '{0}'.", evaluatedRecipient));
        //                    }
        //                }
        //            }

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("An Error happened during preparation of the e-mail recipients." + e.Message);
        //    }


        //    //Add Recipients to mail.
        //    foreach (string address in validRecipients)
        //    {
        //        if (address != null)
        //            mail.To.Add(address);
        //    }

        //    if (mail.To.Count == 0)
        //        throw new Exception("No valid E-mail could be extracted from configured recipients:" + string.Join(";", recipientsInput));

        //    //Evaluate  MailSubject
        //    string mailSubject = Properties.Settings.Default.EBillSubject;
        //    try
        //    {
        //        mailSubject = expVal.Evaluate(mailSubject, row);
        //    }
        //    catch (Exception e)
        //    {
        //        log4.Info("Ignoring expression in Subject-line. Error: " + e.Message);
        //    }
        //    mail.Subject = mailSubject;

        //    string mailBody = "";

        //    //Check if MulitlanguageTemplateSupport is Active
        //    if (Properties.Settings.Default.MultiLanguageTemplateProperty == "")
        //    {
        //        Encoding enc;
        //        using (StreamReader reader = new StreamReader(Properties.Settings.Default.EmailTemplatePath, Encoding.GetEncoding(1252), true))
        //        {
        //            reader.Peek(); // you need this!
        //            enc = reader.CurrentEncoding;
        //        }

        //        //TODO Maybe read as HTML here to maintain format of all expressions.
        //        mailBody = File.ReadAllText(Properties.Settings.Default.EmailTemplatePath, enc);
        //    }
        //    else
        //    {
        //        string usedTemplatePath = Properties.Settings.Default.EmailTemplatePath;
        //        string sMultiLangPropVal = expVal.Evaluate(Properties.Settings.Default.MultiLanguageTemplateProperty, row);
        //        string sMLPath = Properties.Settings.Default.EmailTemplatePath + "." + sMultiLangPropVal;

        //        if (sMultiLangPropVal == "" || !File.Exists(sMLPath))
        //            log4.Info("MultiLanguage prop not on document, or template not found! Using standard template instead.");
        //        else
        //            usedTemplatePath = sMLPath;

        //        Encoding enc;
        //        using (StreamReader reader = new StreamReader(usedTemplatePath, Encoding.GetEncoding(1252), true))
        //        {
        //            reader.Peek(); // you need this!
        //            enc = reader.CurrentEncoding;
        //        }
        //        mailBody = File.ReadAllText(usedTemplatePath, enc);
        //    }

        //    //AEPH 10.02.2016 Multiline Support for Regex Match.
        //    //But replace new-line char.
        //    string[] IndexHits = Regex.Matches(mailBody, @"(?<=\$\[)(.|\s)*?(?=\]\$)").Cast<Match>().Select(m => m.Value).ToArray();

        //    foreach (string expression in IndexHits)
        //    {
        //        string expressionNewLineEscaped = expression.Replace(System.Environment.NewLine, "");

        //        string rep = "$[" + expression + "]$";
        //        string with = "$[" + expression + "]$";
        //        try
        //        {
        //            with = expVal.Evaluate(expressionNewLineEscaped, row);
        //        }
        //        catch (Exception e)
        //        {
        //            log4.Info("Ignoring expression in Template. Expression:'" + rep + "'. Error: " + e.Message);
        //        }

        //        with = WebUtility.HtmlEncode(with);
        //        mailBody = mailBody.Replace(rep, with);
        //    }

        //    //Check for HTML
        //    Regex tagRegex = new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");
        //    if (tagRegex.IsMatch(mailBody))
        //    {
        //        mail.IsBodyHtml = true;
        //    }

        //    mail.Body = mailBody;



        //    //Attachement
        //    if (Properties.Settings.Default.AttachDocument)
        //    {

        //        //Add all documents from tempdirectory if original files will be sent.
        //        if (Properties.Settings.Default.GroupingActive && tempDirectory != "")
        //        {
        //            //DONE - SwissRe Sorting functionality.
        //            //AEPH 23.03.2017 - Getting Files in Alphabetical Order.
        //            //string[] files = Directory.GetFiles(tempDirectory,"*",SearchOption.AllDirectories);
        //            var files = Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).OrderBy(f => f);
        //            foreach (string file in files)
        //            {
        //                mail.Attachments.Add(new Attachment(file));
        //            }
        //            //Dont delete Directory yet or attachements will not be sent.
        //            //Directory.Delete(tempDirectory,true);
        //        }
        //        else
        //        {
        //            MemoryStream ms = new MemoryStream(document);

        //            //create the attachment from a stream. Be sure to name the data 
        //            //with a file and 
        //            //media type that is respective of the data
        //            mail.Attachments.Add(new Attachment(ms, docinfo.fileName));
        //        }

        //    }


        //    if (Properties.Settings.Default.AttachLnkFile && !Properties.Settings.Default.GroupingActive)
        //    {
        //        //Get some binary data
        //        string sFileContent = "";
        //        string templateFilePath = "";
        //        string docID = "";
        //        string storeID = "";
        //        string docname = "";

        //        //AEPH 09.02.2016
        //        if (docinfo.documentID != null)
        //        {
        //            docID = docinfo.documentID.ToString();
        //            storeID = docinfo.storeID.ToString();
        //            docname = docinfo.name;
        //        }
        //        else
        //        {
        //            docname = row.Cells[0].Value.ToString();
        //            docID = row.Cells[1].Value.ToString();
        //            storeID = row.Cells["storeID"].Value.ToString();
        //        }


        //        if (Properties.Settings.Default.LnkFilePath != "")
        //        {
        //            templateFilePath = Properties.Settings.Default.LnkFilePath;
        //        }
        //        else
        //        {
        //            string progPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //            templateFilePath = Path.Combine(progPath, "standard_linkfile_template.dlk");
        //        }


        //        if (File.Exists(templateFilePath))
        //        {
        //            try
        //            {
        //                XmlDocument doc = new XmlDocument();
        //                doc.Load(templateFilePath);
        //                XmlNode nodeDocID = doc.SelectSingleNode("/InfoShareLink/InfoShareFiles/InfoShareFile/InfoObjectID");
        //                XmlNode nodeStoreID = doc.SelectSingleNode("/InfoShareLink/InfoShareFiles/InfoShareFile/InfoStoreID");

        //                nodeDocID.InnerText = docID;
        //                nodeStoreID.InnerText = storeID;

        //                sFileContent = doc.InnerXml;
        //            }
        //            catch (Exception e)
        //            {
        //                log4.Info("Error happened during preparation of the linkfile. Ignoring template." + e.Message);
        //                //AEPH 09.02.2016
        //                sFileContent = "<InfoShareLink><InfoShareFiles><InfoShareFile><InfoObjectID>" + docID + "</InfoObjectID><InfoStoreID>" + storeID + "</InfoStoreID></InfoShareFile></InfoShareFiles><InfoShareFolderConfiguration><GroupedListViewColumns /></InfoShareFolderConfiguration></InfoShareLink>";
        //            }
        //        }
        //        else
        //        {
        //            //AEPH 09.02.2016
        //            log4.Info("No template path for linkfile defined, using standard link-file template instead.");
        //            sFileContent = "<InfoShareLink><InfoShareFiles><InfoShareFile><InfoObjectID>" + docID + "</InfoObjectID><InfoStoreID>" + storeID + "</InfoStoreID></InfoShareFile></InfoShareFiles><InfoShareFolderConfiguration><GroupedListViewColumns /></InfoShareFolderConfiguration></InfoShareLink>";
        //        }


        //        byte[] data = new byte[sFileContent.Length * sizeof(char)];
        //        System.Buffer.BlockCopy(sFileContent.ToCharArray(), 0, data, 0, data.Length);

        //        //save the data to a memory stream
        //        MemoryStream ms = new MemoryStream(data);

        //        //create the attachment from a stream. Be sure to name the data 
        //        //with a file and 
        //        //media type that is respective of the data
        //        mail.Attachments.Add(new Attachment(ms, docname + ".dlk", "text/plain"));
        //    }


        //    //AEPH 25.01.2017
        //    bool result = false;
        //    log4.Info(string.Format("Trying to send Mail. Recipient:'{0}'", mail.To.ToString()));
        //    try
        //    {
        //        Task<bool> AsyncTaskSendMail = SendEmailAsync(mail, row);
        //        await AsyncTaskSendMail;
        //        result = AsyncTaskSendMail.Result;
        //        log4.Info(string.Format("Positive answer from mail server. Recipient:'{0}'", mail.To.ToString()));
        //    }
        //    catch (Exception exp)
        //    {
        //        throw new Exception(string.Format("Error happened while sending mail! Recipient:'{0}', Message:{1}", mail.To.ToString(), exp.ToString()));
        //    }

        //    mail.Dispose();

        //    return result;
        //}

        public async Task<string> SendDocumentMail(DocumentContract doc, string tempDirectory)
        {
            //PrepareMail
            using (MailMessage mail = new MailMessage())
            {
                mail.Sender = new MailAddress(Properties.Settings.Default.SMTPSender);
                mail.From = new MailAddress(Properties.Settings.Default.SMTPSender);

                //Prepare Recipients
                string[] recipientsInput = Properties.Settings.Default.EBillSendTo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                string[] validRecipients = new string[recipientsInput.Length];
                try
                {
                    for (int i = 0; i < recipientsInput.Length; i++)
                    {
                        //AEPH 11.02.2016 - check if recipient may be a username.
                        recipientsInput[i] = ExpressionsEvaluator.GetInstance.Evaluate(recipientsInput[i], doc);
                    }

                    for (int i = 0; i < recipientsInput.Length; i++)
                    {
                        if (FileHelper.IsValidMail(recipientsInput[i]))
                            validRecipients[i] = recipientsInput[i];
                        else
                        {
                            string evaluatedRecipient = recipientsInput[i].ToLower();
                            foreach (UserContract uInf in WCFHandler.GetInstance.GetAllUsers())
                            {
                                if (uInf.DisplayName.ToLower() == evaluatedRecipient || uInf.LoginName.ToLower() == evaluatedRecipient || uInf.Id == evaluatedRecipient)
                                {
                                    if (uInf.Email != null && uInf.Email != "")
                                    {
                                        evaluatedRecipient = uInf.Email;
                                        if (FileHelper.IsValidMail(evaluatedRecipient.ToString()))
                                        {
                                            validRecipients[i] = evaluatedRecipient;
                                            break;
                                        }
                                    }
                                    else
                                        log4.Warn(string.Format("No valid E-Mail could be resolved for recipient: '{0}'. Object ID: '{1}'", uInf.DisplayName, doc.Id));
                                }
                            }
                        }

                    }

                }
                catch (Exception e){ throw new Exception(string.Format("An Error happened during preparation of the e-mail recipients. Msg:'{0}'",e.Message)); }

                //Add Recipients to mail.
                foreach (string address in validRecipients)
                {
                    if (address != null)
                        mail.To.Add(address);
                }

                if (mail.To.Count == 0)
                    throw new Exception("No valid E-mail could be extracted from configured recipients:" + string.Join(";", recipientsInput));

                //Evaluate  MailSubject
                string mailSubject = Properties.Settings.Default.EBillSubject;
                try
                {
                    mailSubject = ExpressionsEvaluator.GetInstance.Evaluate(mailSubject, doc);
                }
                catch (Exception e)
                {
                    log4.Warn(string.Format("An Error happened during preparation of the e-mail subject. Msg:'{0}'", e.Message));
                }


                mail.Subject = mailSubject;

                string mailBody = "";

                //Check if MulitlanguageTemplateSupport is Active
                if (Properties.Settings.Default.MultiLanguageTemplateProperty == "")
                {
                    Encoding enc;
                    using (StreamReader reader = new StreamReader(Properties.Settings.Default.EmailTemplatePath, Encoding.GetEncoding(1252), true))
                    {
                        reader.Peek(); // you need this!
                        enc = reader.CurrentEncoding;
                    }

                    //Note: Maybe read as HTML here to maintain format of all expressions.
                    mailBody = System.IO.File.ReadAllText(Properties.Settings.Default.EmailTemplatePath, enc);
                }
                else
                {
                    string usedTemplatePath = Properties.Settings.Default.EmailTemplatePath;
                    string sMultiLangPropVal = ExpressionsEvaluator.GetInstance.Evaluate(Properties.Settings.Default.MultiLanguageTemplateProperty, doc);
                    string sMLPath = Properties.Settings.Default.EmailTemplatePath + "." + sMultiLangPropVal;

                    if (sMultiLangPropVal == "" || !System.IO.File.Exists(sMLPath))      
                        log4.Warn(string.Format("MultiLanguage prop not on document, or template not found! Using standard template instead. Object ID: '{0}'", doc.Id));
                    else
                        usedTemplatePath = sMLPath;

                    Encoding enc;
                    using (StreamReader reader = new StreamReader(usedTemplatePath, Encoding.GetEncoding(1252), true))
                    {
                        reader.Peek(); // you need this!
                        enc = reader.CurrentEncoding;
                    }
                    mailBody = System.IO.File.ReadAllText(usedTemplatePath, enc);
                }

                //AEPH 10.02.2016 Multiline Support for Regex Match.
                //But replace new-line char.
                string[] IndexHits = Regex.Matches(mailBody, @"(?<=\$\[)(.|\s)*?(?=\]\$)").Cast<Match>().Select(m => m.Value).ToArray();

                foreach (string expression in IndexHits)
                {
                    string expressionNewLineEscaped = expression.Replace(System.Environment.NewLine, "");

                    string rep = "$[" + expression + "]$";
                    string with = "$[" + expression + "]$";
                    try
                    {
                        with = ExpressionsEvaluator.GetInstance.Evaluate(expressionNewLineEscaped, doc);
                    }
                    catch (Exception e)
                    {
                        log4.Warn(string.Format("Ignoring expression in E-Mail Template. Expression: '{0}' Object ID: '{1}' Msg:'{2}'", rep, doc.Id, e.Message));
                    }

                    with = WebUtility.HtmlEncode(with);
                    mailBody = mailBody.Replace(rep, with);
                }

                //Check for HTML
                Regex tagRegex = new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");
                if (tagRegex.IsMatch(mailBody))
                {
                    mail.IsBodyHtml = true;
                }

                mail.Body = mailBody;


                //Attachements
                if (Properties.Settings.Default.AttachDocument)
                {

                    //DONE - SwissRe Sorting functionality.
                    //AEPH 23.03.2017 - Getting Files in Alphabetical Order.
                    var files = Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).OrderBy(f => f);
                    foreach (string file in files)
                    {
                        mail.Attachments.Add(new Attachment(file));
                    }
                    //Dont delete Directory yet or attachements will not be sent.
                    //Directory.Delete(tempDirectory,true);
                }


                if (Properties.Settings.Default.AttachLnkFile)
                {
                    //Get some binary data
                    string sFileContent = "";
                    string templateFilePath = "";
                    string docID = "";
                    string storeID = "";
                    string docname = "";

                    docID = doc.Id;
                    storeID = doc.InfoStoreId;
                    docname = doc.Name;


                    if (Properties.Settings.Default.LnkFilePath != "")
                    {
                        templateFilePath = Properties.Settings.Default.LnkFilePath;
                    }
                    else
                    {
                        string progPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        templateFilePath = Path.Combine(progPath, "DefaultTemplates/standard_linkfile_template.dlk");
                    }

                    if (System.IO.File.Exists(templateFilePath))
                    {
                        try
                        {
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.Load(templateFilePath);
                            XmlNode nodeDocID = xDoc.SelectSingleNode("/InfoShareLink/InfoShareFiles/InfoShareFile/InfoObjectID");
                            XmlNode nodeStoreID = xDoc.SelectSingleNode("/InfoShareLink/InfoShareFiles/InfoShareFile/InfoStoreID");

                            nodeDocID.InnerText = docID;
                            nodeStoreID.InnerText = storeID;

                            sFileContent = xDoc.InnerXml;
                        }
                        catch (Exception e)
                        {
                            log4.Warn(string.Format("Error happened during preparation of the linkfile. Ignoring template. Object ID: '{0}' Msg:'{1}'",  doc.Id, e.Message));
                            sFileContent = "<InfoShareLink><InfoShareFiles><InfoShareFile><InfoObjectID>" + docID + "</InfoObjectID><InfoStoreID>" + storeID + "</InfoStoreID></InfoShareFile></InfoShareFiles><InfoShareFolderConfiguration><GroupedListViewColumns /></InfoShareFolderConfiguration></InfoShareLink>";
                        }
                    }
                    else
                    {
                        //AEPH 09.02.2016
                        log4.Warn(string.Format("No template path for linkfile defined, using standard link-file template instead. Object ID: '{0}'", doc.Id));
                        sFileContent = "<InfoShareLink><InfoShareFiles><InfoShareFile><InfoObjectID>" + docID + "</InfoObjectID><InfoStoreID>" + storeID + "</InfoStoreID></InfoShareFile></InfoShareFiles><InfoShareFolderConfiguration><GroupedListViewColumns /></InfoShareFolderConfiguration></InfoShareLink>";
                    }


                    byte[] data = new byte[sFileContent.Length * sizeof(char)];
                    System.Buffer.BlockCopy(sFileContent.ToCharArray(), 0, data, 0, data.Length);

                    //save the data to a memory stream
                    MemoryStream ms = new MemoryStream(data);

                    //create the attachment from a stream. Be sure to name the data 
                    //with a file and 
                    //media type that is respective of the data
                    mail.Attachments.Add(new Attachment(ms, docname + ".dlk", "text/plain"));
                }


                //AEPH 25.01.2017
                string result = null;
                log4.Info(string.Format("Trying to send Mail. Recipient:'{0}' Object ID: '{1}'", mail.To.ToString(),doc.Id));
                try
                {
                    Task<bool> AsyncTaskSendMail = SendEmailAsync(mail);
                    await AsyncTaskSendMail;
                    //result = AsyncTaskSendMail.Result;
                    result = mail.To.ToString();
                    log4.Info(string.Format("Positive answer from mail server. Recipient:'{0}' Object ID: '{1}'", mail.To.ToString(), doc.Id));
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Error happened while sending mail! Recipient:'{0}', Object ID: '{1}', Msg:'{2}'", mail.To.ToString(),doc.Id, e.Message));
                }
                return result;
            }
        }

        public bool SendEmail(MailMessage message)
        {
            DateTime now = DateTime.Now;
            using (var smtpClient = PrepareSMTPClient())
            {
                smtpClient.Send(message);
                log4.Debug(string.Format("We waited {0} for the SMTP Server.", (DateTime.Now - now).TotalSeconds));
                return true;
            }
        }

        private void SendErrorMail()
        {

            MailMessage mail = new MailMessage
            {
                Sender = new MailAddress(Properties.Settings.Default.SMTPSender),
                From = new MailAddress(Properties.Settings.Default.SMTPSender)
            };

            string mailRecipients = Properties.Settings.Default.ErrorMailSendTo;
            foreach (var address in mailRecipients.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                mail.To.Add(address);
            }

            mail.Subject = Properties.Settings.Default.ErrorMailSubject;

            try
            {
                string strClientIPAddress = GetLocalIPv4(NetworkInterfaceType.Ethernet);
                string strClientMachineName = Environment.MachineName.ToString().Trim();
                string strClientUserName = Environment.UserName.ToString().Trim();
                string strClientDomainName = Environment.UserDomainName.ToString().Trim();
                string strClientOSVersion = Environment.OSVersion.ToString().Trim();

                mail.Body =
                    "IP Address: " + strClientIPAddress + "\r\n" +
                    "Machine Name: " + strClientMachineName + "\r\n" +
                    "Client Username: " + strClientUserName + "\r\n" +
                    "Client Domain: " + strClientDomainName + "\r\n" +
                    "Client OSVersion: " + strClientOSVersion + "\r\n";
            }
            catch (Exception e)
            {
                log4.Info("There was a problem while gathering system-information." + e.Message);
            }


            if (Properties.Settings.Default.ErrorMailIncludeLog)
            {
                mail.Attachments.Add(new Attachment(GetTemporaryLogFileName()));
            }

            try
            {
                SendEmail(mail);
                //client.Send(mail);
                log4.Info("Error-Mail was sent successfully.\n Job stopped. Check config and try again.");
                if (Program.automode)
                {
                    Application.Exit();
                }
            }
            catch (Exception e)
            {
                String innerMessage = (e.InnerException != null)
                  ? e.InnerException.Message
                  : "";

                log4.Info("Problem with sending mail: " + e.Message + "\r\n" + innerMessage);
                if (Program.automode)
                {
                    Application.Exit();
                }
            }
        }

        public bool SendReportMail(int iSentDocuments, TimeSpan timeused)
        {

            //Prepare Recipient.
            MailMessage mail = new MailMessage
            {
                Sender = new MailAddress(Properties.Settings.Default.SMTPSender),
                From = new MailAddress(Properties.Settings.Default.SMTPSender)
            };

            string mailSubject = Properties.Settings.Default.ReportSubject;


            mail.Subject = mailSubject;


            Encoding enc;
            using (StreamReader reader = new StreamReader(Properties.Settings.Default.ReportTemplatePath, Encoding.GetEncoding(1252), true))
            {
                reader.Peek(); // you need this!
                enc = reader.CurrentEncoding;
            }

            string strClientIPAddress = "";
            string strClientMachineName = "";
            string strClientUserName = "";
            string strClientDomainName = "";
            string strClientOSVersion = "";

            string mailBody = System.IO.File.ReadAllText(Properties.Settings.Default.ReportTemplatePath, enc);
            try
            {
                strClientIPAddress = WebUtility.HtmlEncode(GetLocalIPv4(NetworkInterfaceType.Ethernet));
                strClientMachineName = WebUtility.HtmlEncode(Environment.MachineName.ToString().Trim());
                strClientUserName = WebUtility.HtmlEncode(Environment.UserName.ToString().Trim());
                strClientDomainName = WebUtility.HtmlEncode(Environment.UserDomainName.ToString().Trim());
                strClientOSVersion = WebUtility.HtmlEncode(Environment.OSVersion.ToString().Trim());
            }
            catch (Exception e)
            {
                log4.Info("There happened an error while extracting the system-variables used for the report." + e.Message);
            }

            mailBody = mailBody.Replace("*datetoday*", DateTime.Now.Date.ToString("D"));
            mailBody = mailBody.Replace("*computer*", strClientMachineName);
            mailBody = mailBody.Replace("*ip*", strClientIPAddress);
            mailBody = mailBody.Replace("*user*", strClientUserName);
            mailBody = mailBody.Replace("*domain*", strClientDomainName);
            mailBody = mailBody.Replace("*os*", strClientOSVersion);
            mailBody = mailBody.Replace("*endtime*", DateTime.Now.ToString("t"));
            string test = timeused.ToString("mm'm'ss's'");
            mailBody = mailBody.Replace("*timeused*", test);
            mailBody = mailBody.Replace("*nofdocuments*", iSentDocuments.ToString());
            //ReplaceValues




            //Check for HTML
            Regex tagRegex = new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");
            if (tagRegex.IsMatch(mailBody))
            {
                mail.IsBodyHtml = true;
            }

            mail.Body = mailBody;


            try
            {
                string mailRecipients = Properties.Settings.Default.ReportRecipient;
                foreach (var address in mailRecipients.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.To.Clear();
                    mail.To.Add(address);
                    SendEmail(mail);
                }

                log4.Info("Report(s) sucessfully sent to:'" + mailRecipients + "'");
                return true;
            }
            catch (Exception e)
            {
                String innerMessage = (e.InnerException != null)
                  ? e.InnerException.Message
                  : "";
                log4.Info("Problem occured while sending report: " + e.Message + "\r\n" + innerMessage);
                return false;
            }
        }

        #region Utility
        public string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }
        public static string GetTemporaryLogFileName()
        {
            FileAppender rootAppender = ((Hierarchy)LogManager.GetRepository())
                                             .Root.Appenders.OfType<FileAppender>()
                                             .FirstOrDefault();

            string filename = rootAppender != null ? rootAppender.File : string.Empty;

            var tempLogFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(filename));

            using (var stream = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //mail.Attachments.Add(new Attachment(stream, Path.GetFileName(GetLogFileName())));
                using (FileStream fs = System.IO.File.OpenWrite(tempLogFile))
                {
                    stream.CopyTo(fs);
                }
            }
            return tempLogFile;
        }
        #endregion
    }
}
