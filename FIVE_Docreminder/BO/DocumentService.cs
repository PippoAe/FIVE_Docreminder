using docreminder.InfoShareService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace docreminder.BO
{

    /// <summary>
    /// Defines methods for the handling of documents.
    ///
    /// Allows a user to create or delete or update a document, and print the
    /// document's properties.
    /// </summary>
    public class DocumentService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DocumentClient DocumentClient;

        public DocumentService()
        {
            this.DocumentClient = new DocumentClient();
        }

        public DocumentService(DocumentClient documentClient)
        {
            this.DocumentClient = documentClient;
        }

        /// <summary>
        /// Creates a document for the specified file id.
        ///
        /// Searches for the import template contract with the specified import 
        /// template id and assigns the template to the document contract. It also 
        /// takes over the protection domain and the info store from the import 
        /// template and assigns it to the document contract. It reads out the 
        /// properties passed in the hash map and sets the properties' keys and 
        /// values for the document contract. Furthermore, it sets the name of 
        /// the document. Calls the createDocument method on an instance of the
        /// InfoShareService class and passes the connection id, the document 
        /// contract, and the file id as arguments. Returns the document contract,
        /// if the contract is successfully created.
        /// 
        /// Before creating a document you must call either the uploadFileBytes or
        /// uploadFileBytesInChunks method on an instance of the File class. These
        /// methods return the file id you need to create a document.
        /// 
        /// </summary>
        /// <param name="documentClient">the document client of the info share WCF service</param>
        /// <param name="commonClient">the common client of the info share WCF service</param>
        /// <param name="connectionID">the connection id</param>
        /// <param name="fileID">the file name</param>
        /// <param name="documentName">the document name</param>
        /// <param name="importTemplateID">the import template id</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <param name="dictionaryProperties">the dictionary that contains the properties as key/value pairs</param>
        /// <returns>the document contract</returns>
        public DocumentContract CreateDocument(CommonService commonService, string connectionID, string fileID, 
            string documentName, string importTemplateID, string schemaCulture, Dictionary<string, string[]> dictionaryProperties) 
        {    
            
		    DocumentContract documentContract = null;
		
		    // Gets the import template contract for the specified import template id
            ImportTemplateContract importTemplateContract = commonService.GetImportTemplateContract(importTemplateID);

		    string propertyPageTemplateID = importTemplateContract.PropertyPageTemplateId;
		    // Gets the property page template contract assigned to the import template contract
            PropertyPageTemplateContract propertyPageTemplateContract = commonService.GetPropertyPageTemplateContract(propertyPageTemplateID);
		    if (propertyPageTemplateContract != null)
            {
			    // Gets an array of all property template contracts assigned to the property page template
                PropertyTemplateContract[] arrayOfPropertyTemplateContract = propertyPageTemplateContract.PropertyTemplates;

                // Array of property contracts to be set on the document contract further down (see A)
		        IList<PropertyContract> listOfPropertyContract = new List<PropertyContract>(); 

                foreach (PropertyTemplateContract template in arrayOfPropertyTemplateContract)
                {
				    string propertyTypeID = template.PropertyTypeId;
                    PropertyTypeContract propertyTypeContract = commonService.GetPropertyTypeContract(propertyTypeID);

                    string propertyName = Utility.GetValue(propertyTypeContract.Name, schemaCulture);
                    if (!String.IsNullOrEmpty(propertyName))
                    {
                        string[] values;
                        dictionaryProperties.TryGetValue(propertyName, out values); // Gets value for key from dictionary

                        PropertyContract propertyContract = new PropertyContract
                        {
                            PropertyTypeId = propertyTypeID,
                            Values = values
                        };

                        listOfPropertyContract.Add(propertyContract); // Adds property contract
                    }				
			    }

                // Sets mandatory fields
                documentContract = new DocumentContract
                {
                    ImportTemplateId = importTemplateID,
                    ProtectionDomainId = importTemplateContract.ProtectionDomainId,
                    InfoStoreId = importTemplateContract.InfoStoreId,
                    Properties = listOfPropertyContract.ToArray(), // Sets all property contracts (A)
                    Name = documentName
                };

                //documentContract = this.DocumentClient.CreateDocument(connectionID, documentContract, fileID, options: null);

            }
		
		    return documentContract;				
	    }
		
	    /// <summary>
	    /// Gets a list of document simple contracts from the search result contract argument
	    /// and returns the first document.
	    /// </summary>
        /// <param name="searchResultContract">the search result contract</param>
	    /// <returns>the first document</returns>
	    public DocumentSimpleContract GetFirstDocument(SearchResultContract searchResultContract)
        {
            DocumentSimpleContract documentSimpleContract = null;
		
		    if (searchResultContract.Documents.Length > 0)
            { 
                // Checks if at least one document is found
			    documentSimpleContract = searchResultContract.Documents[0]; // Gets the first document
		    }
				
		    return documentSimpleContract;
	    }

        /// <summary>
        /// Prints the result properties and its values.
        ///
        /// Gets an array of property contracts from the document simple contract
        /// that is passed as an argument. For each loop it prints the property and 
        /// its values to the console.
        /// </summary>
        /// <param name="commonClient">the common client of the info share WCF service</param>
        /// <param name="connectionID">the connection id</param>
        /// <param name="documentSimpleContract">the document simple contract</param>
        /// <param name="schemaCulture">the schema culture</param>
        public void PrintResultProperties(CommonService commonService, string connectionID, DocumentSimpleContract documentSimpleContract, string schemaCulture) 
        {    
		
		    Console.WriteLine("Trying to print result properties of document <" + documentSimpleContract.Name + ">, document ID <" +
				    documentSimpleContract.Id + "> ...");

            // Gets an array of all property contracts to be printed
		    foreach (PropertyContract propertyContract in documentSimpleContract.Properties)
            {
			    string propertyTypeId = propertyContract.PropertyTypeId;
                string propertyTypeName = commonService.GetPropertyTypeName(propertyTypeId, schemaCulture);

                string valueStr = Utility.FoldeStringArray(propertyContract.Values);

                Console.Write(String.Format("   {0,-20} {1}", propertyTypeName, valueStr));
			    Console.WriteLine();
			
		    }
		    Console.WriteLine("Result properties of document <" + documentSimpleContract.Name + ">, document ID <" +
				    documentSimpleContract.Id + "> successfully printed.");
	    }
	
	    /// <summary>
        /// Gets the document id for the specified document simple contract.
	    /// </summary>
        /// <param name="documentSimpleContract">the document simple contract</param>
	    /// <returns>the document id</returns>
	    public string GetDocumentID (DocumentSimpleContract documentSimpleContract)
        {
		    return documentSimpleContract.Id;
	    }

        /// <summary>
        /// Gets the document name for the specified document simple contract.
        /// </summary>
        /// <param name="documentSimpleContract">the document simple contract</param>
        /// <returns>the document name</returns>
	    public string GetDocumentName (DocumentSimpleContract documentSimpleContract)
        {
		    return documentSimpleContract.Name;
	    }

        /// <summary>
        /// Updates the value of a document property with the new property value.
        /// 
        /// Gets the property type id for the specified property type name.
	    /// Gets the document contract for the specified document id. The contract 
	    /// contains an array of property contracts. Gets a list of property contracts.
	    /// The method searches for the property contract with the property type id.
	    /// If it finds the contract, it updates the property contract with the new 
	    /// value. Then it updates the document contract's properties field.
	    /// Afterwards it calls the updateDocument method on an instance of the
	    /// InfoShareService class and passes the connection id, the document contract,
	    /// and the check in parameter contract as arguments.
        /// Releases the reservation of the document in order that other users can edit the document
	    /// Returns a document contract, if the document is successfully updated.
        ///
        /// </summary>
        /// <param name="documentClient">the document client of the info share WCF service</param>
        /// <param name="commonClient">the common client of the info share WCF service</param>
        /// <param name="connectionID">the connection id</param>
        /// <param name="documentID">the document id</param>
        /// <param name="propertyTypeName">the property type name</param>
        /// <param name="newPropertyValue">the new property value</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <returns>the document contract</returns>
        public DocumentContract UpdateDocumentProperty(CommonService commonService, string connectionID, 
            string documentID, string propertyTypeName, string newPropertyValue, string schemaCulture) 
        {    
     
		    string propertyTypeID = commonService.GetPropertyTypeID(propertyTypeName, schemaCulture);

            DocumentContract documentContract = this.DocumentClient.GetDocument(connectionID, documentID);

            // Loops through all properties of property contract
            foreach (PropertyContract propertyContract in documentContract.Properties)
            { 
			    if (propertyContract.PropertyTypeId == propertyTypeID)
                {
				    // If property contract found, set new value
				    propertyContract.Values = new string[] { newPropertyValue }; 					
				    break;
			    }			
		    }

            CheckInParameterContract checkInParameterContract = new CheckInParameterContract
            {
                // Releases the reservation of the document in order that other users can edit the document
                ReleaseReservation = true
            };

            documentContract = this.DocumentClient.UpdateDocument(connectionID, documentContract, null, checkInParameterContract);
            return documentContract;
	    }

        /// <summary>
        /// Deletes a document.
        /// </summary>
        /// <param name="connectionID">the connection id</param>
        /// <param name="documentID">the id of the document to be deleted</param>
        /// <param name="ignoreRecycleBin">if true the recycle bin settings are ignored</param>
        public void DeleteDocument(string connectionID, string documentID, bool ignoreRecycleBin) 
        {            
            //this.DocumentClient.DeleteDocument(connectionID, documentID, ignoreRecycleBin);	
	    }
        
        /// <summary>
        /// Gets the document contract for the specified document id.
        /// </summary>
        /// <param name="connectionID">the connection id</param>
        /// <param name="documentID">the document id</param>
        /// <returns>the document contract</returns>
        public DocumentContract GetDocument(string connectionID, string documentID)
        {
            return this.DocumentClient.GetDocument(connectionID, documentID);
        }

        public void UndoCheckOutDocument(string connectionID, string documentID)
        {
            this.DocumentClient.UndoCheckOutDocument(connectionID,  documentID);
        }

        public void CheckOutDocument(string connectionID, string documentID)
        {
            this.DocumentClient.CheckOutDocument(connectionID, documentID);
        }

        /// <summary>
        /// Updates the document with a new file version.
        /// </summary>
        /// <param name="connectionID">the connection id</param>
        /// <param name="documentContract">the document contract to be updated</param>
        /// <param name="fileId">the file id of the new file version</param>
        /// <returns>the updated document contract</returns>
        public DocumentContract UpdateDocument(string connectionID, DocumentContract documentContract, string fileId = null)
        {
            CheckInParameterContract checkInParameterContract = new CheckInParameterContract
            {

                // Releases the reservation of the document in order that other users can edit the document
                ReleaseReservation = true,
                Comment = "ImmobBelegID, Kontierungsmaske & Nachindex für ExterneBelege"
            };


            DocumentContract updatedDocumentContract = new DocumentContract();
            if (documentContract.CheckOutStateEnum == "NotCheckedOut")
            {
                updatedDocumentContract = this.DocumentClient.UpdateDocument(connectionID, documentContract, fileId, checkInParameterContract);
            }
            else
            {
                log.Warn(String.Format("Document checked out already. Current-State:'{0}'", documentContract.CheckOutStateEnum));
                log.Warn("Trying to force-checkin document...");
                this.DocumentClient.UndoCheckOutDocument(connectionID, documentContract.Id);
                updatedDocumentContract = this.DocumentClient.UpdateDocument(connectionID, documentContract, fileId, checkInParameterContract);
                log.Info("Document sucesfully updated.");
            } 
            return updatedDocumentContract;
        }
	
    }

}
