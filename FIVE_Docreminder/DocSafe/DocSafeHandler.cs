﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Net;
using System.Collections.Specialized;
using System.Globalization;
using RestSharp;
using System.Security.Cryptography.X509Certificates;
using RestSharp.Serializers;
using System.Xml;
using System.Windows.Forms;

namespace docreminder.DocSafe
{
    class DocSafeHandler
    {
        ConsoleWriter log = ConsoleWriter.GetInstance;


        //TODO DocSafe
        public bool SendDocumentToDocsafe(byte[] document, KXWS.SDocument docinfo)
        {
            DocumentEnvelope envelope = PepareDocumentEnvelope(document, docinfo);
            
            
            string filename = docinfo.fileName;

            if(docreminder.Properties.Settings.Default.dsFileName != "")
            {
                ExpressionsEvaluator expVal = new ExpressionsEvaluator();
                filename = expVal.Evaluate(docreminder.Properties.Settings.Default.dsFileName, null, docinfo, false);
            }

            return UploadToDocSafe(envelope, filename);
        }


        private DocumentEnvelope PepareDocumentEnvelope(byte[] document, KXWS.SDocument docinfo)
        {
            //Evaluate Recipients
            ExpressionsEvaluator expVal = new ExpressionsEvaluator();

                DocumentEnvelope docenvelope = new DocumentEnvelope();

                docenvelope.Registration = new Registration();
                docenvelope.Registration.SenderBUID = docreminder.Properties.Settings.Default.dsSenderBUID;
                docenvelope.Registration.SendersObjectID = expVal.Evaluate(docreminder.Properties.Settings.Default.dsSendersObjectID, null, docinfo, false);
                docenvelope.Registration.SendersObjectAlias = expVal.Evaluate(docreminder.Properties.Settings.Default.dsSendersObjectAlias, null, docinfo, false);
                docenvelope.Registration.SendersFlowRef = expVal.Evaluate(docreminder.Properties.Settings.Default.dsSendersFlowRef, null, docinfo, false);
                docenvelope.Registration.SendersFlowAlias = expVal.Evaluate(docreminder.Properties.Settings.Default.dsSendersFlowAlias, null, docinfo, false);

                docenvelope.Registration.SafeIDAlias = new SafeIDAliasType();
                docenvelope.Registration.SafeIDAlias.AliasScope = "DocSafeID";
                docenvelope.Registration.SafeIDAlias.Value = expVal.Evaluate(docreminder.Properties.Settings.Default.dsDocSafeID, null, docinfo, false);


                docenvelope.Properties = new Properties();
                docenvelope.Properties.SendersDocumentID = docinfo.documentID; //<- DOC ID!
                //TODO Configure Title
                docenvelope.Properties.Title = expVal.Evaluate(docreminder.Properties.Settings.Default.dsDocumentTitle, null, docinfo, false);

                docenvelope.Properties.SenderName = docreminder.Properties.Settings.Default.dsSenderName;

                docenvelope.Properties.Annotation = expVal.Evaluate(docreminder.Properties.Settings.Default.dsAnnotation, null, docinfo, false);
                docenvelope.Properties.LinkText = docreminder.Properties.Settings.Default.dsLinktext;
                docenvelope.Properties.LinkURL = docreminder.Properties.Settings.Default.dsLinkURL;
                docenvelope.Properties.CreationTS = System.DateTime.Now;
                docenvelope.Properties.AllowsForward = true;


                docenvelope.XMLMetaData = new XMLMetaData();

                //ADD DOCUMENT AS BYTEARRAY
                docenvelope.DocumentBytes = document;
                docenvelope.MIMEType = "application/pdf";

                return docenvelope;
        }


        private bool UploadToDocSafe(DocumentEnvelope docenvelope, string filename)
        {
            //With Authentification
            var client = new RestClient(docreminder.Properties.Settings.Default.dsRestClientURL);
            var request = new RestRequest(docreminder.Properties.Settings.Default.dsRestRequest, Method.POST);

            //Add Certificate
            X509Certificate2 certificate2 = new X509Certificate2();
            certificate2.Import(docreminder.Properties.Settings.Default.dsCertFileName);
            client.ClientCertificates = new X509CertificateCollection() { certificate2 };

                ////CREATE DOCUMENT ENVELOPE
                //DocumentEnvelope document = new DocumentEnvelope();

                //document.Registration = new Registration();
                //document.Registration.SenderBUID = "SANAKK";
                //document.Registration.SendersObjectID = "KVG-4711";
                //document.Registration.SendersObjectAlias = "Police KVG 4711";
                //document.Registration.SendersFlowRef = "Info";

                //document.Registration.SafeIDAlias = new SafeIDAliasType();
                //document.Registration.SafeIDAlias.AliasScope = "DocSafeID";
                ////document.Registration.SafeIDAlias.Value = "KVAG-QCE6-KY9E-UTNL"; //<- Recipient ID (https://test.docsafe.ch)
                ////document.Registration.SafeIDAlias.Value = "9RL2-XESF-K9WZ-T3T6"; //<- Recipient ID (https://test.docsafe.ch/test2)
                //document.Registration.SafeIDAlias.Value = "XYZ";
                //document.Registration.SendersFlowAlias = "Informationen";

                //document.Properties = new Properties();
                //document.Properties.SendersDocumentID = "2asdasdx0axxxsdasdsadasd2314asd3"; //<- DOC ID!
                //document.Properties.Title = "Löhnabrechnung 01.04.2016-01.05.2016";
                //document.Properties.SenderName = "PhäntasieAG";
                ////document.Properties.EffectiveDate = "";
                ////document.Properties.ExpiryDate = "";
                ////document.Properties.SuggestedDeletionDate = "";
                //document.Properties.Annotation = "Löhnäbrechnung";
                //document.Properties.LinkText = "Info-Center";
                //document.Properties.LinkURL = "http://www.phantasie-ag.ch";
                //document.Properties.CreationTS = System.DateTime.Now;
                //document.Properties.AllowsForward = true;


                //document.XMLMetaData = new XMLMetaData();

                ////ADD DOCUMENT AS BYTEARRAY
                //document.DocumentBytes = File.ReadAllBytes("Lohnabrechnung_JohnDoe.pdf");
                //document.MIMEType = "application/pdf";
               

                //ADD Header
                request.AddHeader("content-type", "multipart/form-data; boundary=---011000010111000001101001");

                //Serialize Document-Onject to XML
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(FileHelper.SerializeObjectUTF8(docenvelope));
                //string filename = docreminder.Properties.Settings.Default.dsRestClientURL
                request.AddParameter("multipart/form-data; boundary=---011000010111000001101001", "-----011000010111000001101001\r\nContent-Disposition: form-data; name=\"documentEnvelope\"; filename=\"" + filename + "\"\r\n\r\n" + xml.InnerXml + "\r\n-----011000010111000001101001--", ParameterType.RequestBody);


                //execute the request
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //log.WriteInfo(String.Format("File uploaded without errors. Server Response:'{0}'", response.Content));
                    var descripton = response.StatusDescription;
                    var content = response.Content; // raw content as string
                    //MessageBox.Show(response.StatusDescription + "\n" + response.Content);
                    log.WriteInfo(response.StatusDescription + "\n" + response.Content);
                    return true;
                }
                else
                {
                    //log.WriteInfo(String.Format("File uploaded with errors. Server Response:'{0}'", response.Content));
                    var descripton = response.StatusDescription;
                    var content = response.Content; // raw content as string
                    //MessageBox.Show(response.StatusDescription + "\n" + response.Content);
                    log.WriteInfo(response.StatusDescription + "\n" + response.Content);
                    //return false;
                    throw new Exception(String.Format("An error happened during File Upload. Server Response: '{0}'",response.Content));
                }
        }

        public string GetVersion(string url,string certpath)
        {


            //*VERSION ABFRAGEN*/
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url+"version");
            //string test = "https://test.docsafe.ch/test2/api/v2/version";
            req.AutomaticDecompression = DecompressionMethods.GZip;
            req.Method = "GET";
            req.ContentLength = 0;
            X509Certificate certificates = new X509Certificate2();
            
            certificates.Import(certpath);
            
            req.ClientCertificates.Add(certificates);
            string html = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            return html;
        }

    }
}
