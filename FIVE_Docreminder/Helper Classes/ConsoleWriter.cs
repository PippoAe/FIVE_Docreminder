using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Xml;
using System.Reflection;
using System.Web;

namespace docreminder
{
    class ConsoleWriter
    {
        private static ConsoleWriter instance;

        public ListView lWLog { get; set; }

        string logfile = "";

        //SmtpClient client;

        private ConsoleWriter()
        {
            string today = DateTime.Today.Day.ToString();
            //AEPH 15.02.2017
            //Check if Logpath Exists, else we're on a new machine.
            if(Properties.Settings.Default.IsLogActive && Directory.Exists(Properties.Settings.Default.LogPath))
            {
                 logfile = Properties.Settings.Default.LogPath + ("\\" + today + ".txt");

                 if (File.Exists(logfile))
                 {
                     FileInfo fiInfo = new FileInfo(logfile);
                     string test = DateTime.Today.Subtract(System.TimeSpan.FromDays(2)).Day.ToString();

                     if (fiInfo.LastWriteTime.Day == DateTime.Today.Subtract(System.TimeSpan.FromDays(2)).Day)
                     {
                         File.Delete(logfile);
                     }
                 }

                 if (!File.Exists(logfile))
                 {
                     using (System.IO.FileStream fs = System.IO.File.Create(logfile));
                 }

                //25.01.2017 - LogFile get's deleted if too big.
                // FileInfo f = new FileInfo(logfile);
                //if (f.Length > 1000 * 500)
                //   File.Delete(logfile);
            }
        }




        public static ConsoleWriter GetInstance
        {
            get
            {
                if (instance == null)
                    instance = new ConsoleWriter();
                return instance;
            }
        }

        public void WriteClean(string msg)
        {
            WriteToLog(msg);
            Console.WriteLine(msg);
        }

        public void WriteInfo(string msg)
        {
            string sInfo = "*Info* ";
            SendToConsole(sInfo + msg, false);
        }

        public void WriteError(string msg)
        {
            string sError = "*Error* ";
            SendToConsole(sError + msg, true, true);
        }


        public void WriteEnd(string msg)
        {
            string sInfo = "*Info* ";
            SendToConsole(sInfo + msg, true);
        }

        public void WriteToLog(string line)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@logfile, true))

                    file.WriteLine(line);
            }
            catch (Exception e)
            {
            }

        }

        private void SendToListView(string line)
        {
            lWLog.Invoke((MethodInvoker)delegate() 
            {
                ListViewItem lWitem = new ListViewItem(line);
                lWitem.ToolTipText = line;
                lWLog.Items.Add(lWitem);
                lWLog.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                lWLog.Items[lWLog.Items.Count - 1].EnsureVisible();
            });
        }

        private void SendToListView(ListViewItem listviewitem)
        {
            lWLog.Invoke((MethodInvoker)delegate()
            {
                lWLog.Items.Add(listviewitem);
                lWLog.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                lWLog.Items[lWLog.Items.Count - 1].EnsureVisible();
            });
        }



        private void SendToConsole(string msg, bool stop)
        {
            string sTimeStamp = DateTime.Now.ToString("HH:mm:ss.ff");
            //string sAppName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string sAppName = "";
            string line = sAppName + "[" + sTimeStamp + "]: " + msg;
            WriteToLog(line);
            
            //lWLog.Invoke((MethodInvoker)delegate() { lWLog.Items.Add(line);} );
            SendToListView(line);

            //lWLog.Items.Add(line);

            if (stop)
            {
                string eline = sAppName + "[" + sTimeStamp + "]: " + "Job stopped. Check config and try again.";
                WriteToLog(eline);
                //lWLog.Items.Add(eline);
                SendToListView(eline);
                //Environment.Exit(0);
            }
            //lWLog.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //lWLog.Items[lWLog.Items.Count - 1].EnsureVisible();
        }

        private void SendToConsole(string msg, bool stop, bool error)
        {
            string sTimeStamp = DateTime.Now.ToString("HH:mm:ss.ff");
            //string sAppName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string sAppName = "";
            string line = sAppName + "[" + sTimeStamp + "]: " + msg;
            WriteToLog(line);
            
            ListViewItem li = new ListViewItem();
            if (error)
                li.ForeColor = Color.Red;
            li.Text = line;

            SendToListView(li);
            

            if (stop)
            {
                string eline = sAppName + "[" + sTimeStamp + "]: " + "Job stopped. Check config and try again.";
                WriteToLog(eline);
                ListViewItem li2 = new ListViewItem();
                li2.ForeColor = Color.Red;
                li2.Text = eline;
                
                SendToListView(li2);

                if (error)
                {
                    if (Properties.Settings.Default.SendErrorMail)
                    {
                        SendErrorMail();
                    }
                    else
                    {
                        if (Program.automode)
                        {
                            Application.Exit();
                        }
                    }
                }
                if (Program.automode)
                {
                    Application.Exit();
                }
            }
        }



        private void SendErrorMail()
        {

            MailMessage mail = new MailMessage();
            mail.Sender = new MailAddress(Properties.Settings.Default.SMTPSender);
            mail.From = new MailAddress(Properties.Settings.Default.SMTPSender);

            string mailRecipients = Properties.Settings.Default.ErrorMailSendTo;
            foreach (var address in mailRecipients.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                mail.To.Add(address);
            }

            //client = new SmtpClient();

            //if (Properties.Settings.Default.SMTPUsername != "")
            //{
            //    client.UseDefaultCredentials = false;
            //    System.Net.NetworkCredential creds = new System.Net.NetworkCredential(Properties.Settings.Default.SMTPUsername, Properties.Settings.Default.SMPTPassword);
            //    client.Credentials = creds;
            //}
            //else
            //{
            //    client.UseDefaultCredentials = true;
            //}

            //if (Properties.Settings.Default.SMTPPort == "")
            //    client.Port = 25;
            //else
            //    client.Port = Convert.ToInt32(Properties.Settings.Default.SMTPPort);

            //client.DeliveryMethod = SmtpDeliveryMethod.Network;


            //client.Host = Properties.Settings.Default.SMTPServerAdress;

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
            catch(Exception e)
            {
                WriteInfo("There was a problem while gathering system-information."+e.Message);
            }


            if (Properties.Settings.Default.ErrorMailIncludeLog)
            {
                mail.Attachments.Add(new Attachment(logfile));
            }

            try
            {
                SendEmail(mail);
                //client.Send(mail);
                WriteInfo("Error-Mail was sent successfully.\n Job stopped. Check config and try again.");
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

                WriteInfo("Problem with sending mail: " + e.Message +"\r\n" + innerMessage);
                if (Program.automode)
                {
                    Application.Exit();
                }
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



        public bool SendTestMail(string recipient)
        {
            MailMessage mail = new MailMessage(Properties.Settings.Default.SMTPSender, recipient);
            //mail.From = new MailAddress(Properties.Settings.Default.SMTPSender, "FIVE Informatik AG");

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
                WriteInfo("There was a problem while gathering system-information." + e.Message);
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

            try
            {
                //client.Send(mail);
                SendEmail(mail);
                WriteInfo("SMTP Test-Mail was sent successfully.");
                return true;
            }
            catch (Exception e)
            {
                String innerMessage = (e.InnerException != null)
                    ? e.InnerException.Message
                    : "";
                WriteInfo("Problem with sending SMTP-Mail: " + e.Message + "\r\n" + innerMessage);
                return false;
            }
        }

        public async Task<bool> SendDocumentMail(byte[] document, KXWS.SDocument docinfo, DataGridViewRow row, List<KXWS.SUserInfoExt> lUsers = null, string tempDirectory = "")
        {
            ExpressionsEvaluator expVal = new ExpressionsEvaluator();

            //PrepareMail
            MailMessage mail = new MailMessage();
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
                    recipientsInput[i] = expVal.Evaluate(recipientsInput[i], row);
                }

                

                for (int i = 0; i < recipientsInput.Length; i++)
                {
                    if (FileHelper.IsValidMail(recipientsInput[i]))
                        validRecipients[i] = recipientsInput[i];
                    else
                    {
                        string evaluatedRecipient = recipientsInput[i].ToLower();
                        foreach (KXWS.SUserInfoExt uInf in lUsers)
                        {
                            if (uInf.displayName.ToLower() == evaluatedRecipient || uInf.logonName.ToLower() == evaluatedRecipient)
                            {
                                if (uInf.emailAddress != null)
                                {
                                    evaluatedRecipient = uInf.emailAddress.ToString();
                                    if (FileHelper.IsValidMail(evaluatedRecipient.ToString()))
                                    {
                                        validRecipients[i] = evaluatedRecipient;
                                        break;
                                    }
                                }
                                else
                                    WriteInfo(String.Format("No valid E-Mail could be resolved for recipient: '{0}'.", evaluatedRecipient));
                            }
                        }
                    }

                }
                 
            }
            catch(Exception e)
            {
                throw new Exception("An Error happened during preparation of the e-mail recipients."+e.Message);
            }


            //Add Recipients to mail.
            foreach (string address in validRecipients)
            {
                if(address != null)
                    mail.To.Add(address);
            }

            if (mail.To.Count == 0)
                throw new Exception("No valid E-mail could be extracted from configured recipients:"+String.Join(";",recipientsInput));

            //Evaluate  MailSubject
            string mailSubject = Properties.Settings.Default.EBillSubject;
            try
            {
                mailSubject = expVal.Evaluate(mailSubject, row);
            }
            catch (Exception e)
            {
                WriteInfo("Ignoring expression in Subject-line. Error: " + e.Message);
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

                //TODO Maybe read as HTML here to maintain format of all expressions.
                mailBody = File.ReadAllText(Properties.Settings.Default.EmailTemplatePath, enc);
            }
            else
            {
                string usedTemplatePath = Properties.Settings.Default.EmailTemplatePath;
                string sMultiLangPropVal = expVal.Evaluate(Properties.Settings.Default.MultiLanguageTemplateProperty, row);
                string sMLPath = Properties.Settings.Default.EmailTemplatePath+"."+sMultiLangPropVal;

                if (sMultiLangPropVal == "" || !File.Exists(sMLPath))
                    WriteInfo("MultiLanguage prop not on document, or template not found! Using standard template instead.");
                else
                    usedTemplatePath = sMLPath;

                Encoding enc;
                using (StreamReader reader = new StreamReader(usedTemplatePath, Encoding.GetEncoding(1252), true))
                {
                    reader.Peek(); // you need this!
                    enc = reader.CurrentEncoding;
                }
                mailBody = File.ReadAllText(usedTemplatePath, enc);
            }

            //AEPH 10.02.2016 Multiline Support for Regex Match.
            //But replace new-line char.
            //string[] IndexHits = Regex.Matches(mailBody, @"(?<=\$\[)(.)*?(?=\]\$)").Cast<Match>().Select(m => m.Value).ToArray();
            string[] IndexHits = Regex.Matches(mailBody, @"(?<=\$\[)(.|\s)*?(?=\]\$)").Cast<Match>().Select(m => m.Value).ToArray();

            foreach (string expression in IndexHits)
            {
                string expressionNewLineEscaped = expression.Replace(System.Environment.NewLine, "");

                string rep = "$[" + expression + "]$";
                string with = "$[" + expression + "]$";
                try
                {
                    with = expVal.Evaluate(expressionNewLineEscaped, row);
                }
                catch(Exception e)
                {
                    WriteInfo("Ignoring expression in Template. Expression:'"+rep+"'. Error: "+e.Message);
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



            //Attachement
            if (Properties.Settings.Default.AttachDocument)
            {

                //Add all documents from tempdirectory if original files will be sent.
                if (Properties.Settings.Default.GroupingActive && tempDirectory != "")
                {
                    //DONE - SwissRe Sorting functionality.
                    //AEPH 23.03.2017 - Getting Files in Alphabetical Order.
                    //string[] files = Directory.GetFiles(tempDirectory,"*",SearchOption.AllDirectories);
                    var files = Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).OrderBy(f => f);
                    foreach (string file in files)
                    {
                       mail.Attachments.Add(new Attachment(file));
                    }
                    //Dont delete Directory yet or attachements will not be sent.
                    //Directory.Delete(tempDirectory,true);
                }
                else
                {
                    MemoryStream ms = new MemoryStream(document);

                    //create the attachment from a stream. Be sure to name the data 
                    //with a file and 
                    //media type that is respective of the data
                    mail.Attachments.Add(new Attachment(ms, docinfo.fileName));
                }

            }


            if (Properties.Settings.Default.AttachLnkFile && !Properties.Settings.Default.GroupingActive)
            {
                //Get some binary data
                string sFileContent = "";
                string templateFilePath = "";
                string docID = "";
                string storeID = "";
                string docname = "";
                
                //AEPH 09.02.2016
                if (docinfo.documentID != null)
                {
                    docID = docinfo.documentID.ToString();
                    storeID = docinfo.storeID.ToString();
                    docname = docinfo.name;
                }
                else
                {
                    docname = row.Cells[0].Value.ToString();
                    docID = row.Cells[1].Value.ToString();
                    storeID = row.Cells["storeID"].Value.ToString();
                }
                

                if (Properties.Settings.Default.LnkFilePath != "")
                {
                    templateFilePath = Properties.Settings.Default.LnkFilePath;
                }
                else
                {
                    string progPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    templateFilePath = Path.Combine(progPath, "standard_linkfile_template.dlk");
                }
                

                if (File.Exists(templateFilePath))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(templateFilePath);
                        XmlNode nodeDocID = doc.SelectSingleNode("/InfoShareLink/InfoShareFiles/InfoShareFile/InfoObjectID");
                        XmlNode nodeStoreID = doc.SelectSingleNode("/InfoShareLink/InfoShareFiles/InfoShareFile/InfoStoreID");

                        nodeDocID.InnerText = docID;
                        nodeStoreID.InnerText = storeID;

                        sFileContent = doc.InnerXml;
                    }
                    catch(Exception e)
                    {
                        WriteInfo("Error happened during preparation of the linkfile. Ignoring template." + e.Message);
                        //AEPH 09.02.2016
                        sFileContent = "<InfoShareLink><InfoShareFiles><InfoShareFile><InfoObjectID>" + docID + "</InfoObjectID><InfoStoreID>" + storeID + "</InfoStoreID></InfoShareFile></InfoShareFiles><InfoShareFolderConfiguration><GroupedListViewColumns /></InfoShareFolderConfiguration></InfoShareLink>";
                    }
                }
                else
                {
                    //AEPH 09.02.2016
                    WriteInfo("No template path for linkfile defined, using standard link-file template instead.");
                    sFileContent = "<InfoShareLink><InfoShareFiles><InfoShareFile><InfoObjectID>" + docID + "</InfoObjectID><InfoStoreID>" + storeID + "</InfoStoreID></InfoShareFile></InfoShareFiles><InfoShareFolderConfiguration><GroupedListViewColumns /></InfoShareFolderConfiguration></InfoShareLink>";
                }


                byte[] data = new byte[sFileContent.Length * sizeof(char)];
                System.Buffer.BlockCopy(sFileContent.ToCharArray(), 0, data, 0, data.Length);

                //save the data to a memory stream
                MemoryStream ms = new MemoryStream(data);

                //create the attachment from a stream. Be sure to name the data 
                //with a file and 
                //media type that is respective of the data
                mail.Attachments.Add(new Attachment(ms, docname+".dlk", "text/plain"));
            }


            //AEPH 25.01.2017
            bool result = false;
            WriteInfo(String.Format("Trying to send Mail. Recipient:'{0}'",mail.To.ToString()));
            try
            {
                Task<bool> AsyncTaskSendMail = SendEmailAsync(mail, row);
                await AsyncTaskSendMail;
                result = AsyncTaskSendMail.Result;
                WriteInfo(String.Format("Positive answer from mail server. Recipient:'{0}'", mail.To.ToString()));
            }
            catch(Exception exp)
            {
                throw new Exception(String.Format("Error happened while sending mail! Recipient:'{0}', Message:{1}",mail.To.ToString(),exp.ToString()));
                    //throw new Exception("Error happened while sending mail! Message:" + exp.ToString());   
            }

            mail.Dispose();

            return result;
        }

        public bool SendReportMail(int iSentDocuments,TimeSpan timeused)
        {

            //Prepare Recipient.
            MailMessage mail = new MailMessage();
            mail.Sender = new MailAddress(Properties.Settings.Default.SMTPSender);
            mail.From = new MailAddress(Properties.Settings.Default.SMTPSender);

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

            string mailBody = File.ReadAllText(Properties.Settings.Default.ReportTemplatePath,enc);
            try
            {
                strClientIPAddress = WebUtility.HtmlEncode(GetLocalIPv4(NetworkInterfaceType.Ethernet));
                strClientMachineName = WebUtility.HtmlEncode(Environment.MachineName.ToString().Trim());
                strClientUserName = WebUtility.HtmlEncode(Environment.UserName.ToString().Trim());
                strClientDomainName = WebUtility.HtmlEncode(Environment.UserDomainName.ToString().Trim());
                strClientOSVersion = WebUtility.HtmlEncode(Environment.OSVersion.ToString().Trim());
            }
            catch(Exception e)
            {
                WriteInfo("There happened an error while extracting the system-variables used for the report." + e.Message);
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

                WriteInfo("Report(s) sucessfully sent to:'"+mailRecipients+"'");
                return true;
            }
            catch (Exception e)
            {
                String innerMessage = (e.InnerException != null)
                  ? e.InnerException.Message
                  : "";
                WriteInfo("Problem occured while sending report: " + e.Message + "\r\n"+ innerMessage);
                //WriteInfo(e.InnerException.ToString());
                return false;
            }
        }

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

        public static bool TestConnection(string smtpServerAddress, int port)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(smtpServerAddress);
            IPEndPoint endPoint = new IPEndPoint(hostEntry.AddressList[0], port);
            using (Socket tcpSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                //try to connect and test the rsponse for code 220 = success
                tcpSocket.Connect(endPoint);
                if (!CheckResponse(tcpSocket, 220))
                {
                    return false;
                }

                // send HELO and test the response for code 250 = proper response
                SendData(tcpSocket, string.Format("HELO {0}\r\n", Dns.GetHostName()));
                if (!CheckResponse(tcpSocket, 250))
                {
                    return false;
                }

                // if we got here it's that we can connect to the smtp server
                return true;
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


        public async Task<bool> SendEmailAsync(MailMessage message,DataGridViewRow row)
        {
            using (var smtpClient = PrepareSMTPClient())
            {
                    //09.02.2016 AEPH (AsyncSending)
                    //25.01.2017 AEPH TODO: Possible positive answer if mail not sent/CHECK
                    await smtpClient.SendMailAsync(message);
 
                    return true;
            }
        }

        //Decapped.
        public bool SendEmail(MailMessage message)
        {
            DateTime now = DateTime.Now;

            using (var smtpClient = PrepareSMTPClient())
            {
                //try
               // {
                    smtpClient.Send(message);
                    WriteInfo(String.Format("We waited {0} for the SMTP Server.", (DateTime.Now - now).TotalSeconds));
                    return true;
                //}
                //catch(Exception e)
                //{
                //    throw (Exception e);
                //    return false;
                //}
            }
        }

    }
}
