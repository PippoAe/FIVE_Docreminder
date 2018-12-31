using docreminder.InfoShareService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace docreminder.BO
{

    /// <summary>
    /// Defines a method for searching for a document.
    /// </summary>
    public class SearchService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        SearchClient SearchClient;

        public SearchService()
        {
            this.SearchClient = new SearchClient();
        }

        public SearchService(SearchClient searchClient)
        {
            this.SearchClient = searchClient;
        }

        /// <summary>
        /// Searches for a document by passing the property type name, the comparison
        /// operator, and the property type name value as search criterias. 
        ///
        /// The array of result properties is a list of properties which are  
        /// to be returned from the document search.
        /// Calls the search method on an instance of the InfoShareService.SearchClient class
        /// and passes the connection id, the search definiton contract, and the
        /// result properties as arguments. This method returns the search result 
        /// contract which contains all documents found. 
        /// Returns null, if no property type id is found for the specified property type name.
        /// </summary>
        /// <param name="searchClient">the search client of the info share service</param>
        /// <param name="commonClient">the common client of the info share service</param>
        /// <param name="connectionID">the connection id</param>
        /// <param name="propertyTypeName">the property type name</param>
        /// <param name="propertyValue">the property type name value</param>
        /// <param name="comparisonOperator">the comparison operator</param>
        /// <param name="arrayOfResultPropertyIDs">the array of result properties</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <returns>the search result contract</returns>

        public SearchResultContract SearchDocument(CommonService commonService, string connectionID,
            string propertyTypeID, string propertyTypeIDValue, string comparisonOperator, string[] arrayOfResultPropertyIDs, string schemaCulture)
        {
            
            SearchResultContract searchResultContract = null;

            if (propertyTypeID != null)
            {
                // Sets values on SearchConditionContract
                SearchConditionContract searchConditionContract = new SearchConditionContract
                {
                    PropertyTypeId = propertyTypeID,
                    Values = new String[] { propertyTypeIDValue },
                    ComparisonEnum = comparisonOperator
                };

                // Adds SearchConditionContract object to an array
                SearchConditionContract[] arrayOfSearchConditionContract = new SearchConditionContract[1];
                arrayOfSearchConditionContract[0] = searchConditionContract;

                // Sets values on SearchDefinitionContract
                SearchDefinitionContract searchDefinitionContract = new SearchDefinitionContract
                {
                    Conditions = arrayOfSearchConditionContract,
                    PageSize = 100
                };

                // Searches for document
                searchResultContract = this.SearchClient.Search(connectionID, searchDefinitionContract, arrayOfResultPropertyIDs /* resultProperties */,
                        null /* resume point */, customSecurityToken: null);

            }

            return searchResultContract;
        }


        public SearchResultContract SearchDocument(CommonService commonService, string connectionID,
    string[] searchProperties, string[] ResultProperties, string schemaCulture)
        {
            SearchResultContract searchResultContract = null;

            List<SearchConditionContract> lSearchConditionContract = new List<SearchConditionContract>();

            log.Info("Preparing search- and resultproperties...");

            log.Info("Searchproperties: ");
            foreach (string prop in searchProperties)
            {
                //SearchProperties = [index|value(s)|operator|relation]
                string[] split = prop.Split('|');

                //Pepare comparison and relation enumerators
                Utility.SearchComparisonEnum comparisonEnum = 0;
                Utility.SearchRelationEnum relationEnum = 0;
                if (2 < split.Length)
                    comparisonEnum = (Utility.SearchComparisonEnum)Convert.ToInt16(split[2]);
                if (3 < split.Length)
                    relationEnum = (Utility.SearchRelationEnum)Convert.ToInt16(split[3]); 

                
                string propertyTypeID = commonService.GetPropertyTypeID(split[0], schemaCulture);
                if (propertyTypeID != null)
                {
                    // Sets values on SearchConditionContract
                    SearchConditionContract searchConditionContract = new SearchConditionContract
                    {
                        PropertyTypeId = propertyTypeID
                    };
                    string[] values = split[1].Split(';'); //Split multikey-search values by ";"-symbol.

                    //Special variables
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (Regex.Matches(values[i], "%").Count == 2)
                        {
                            string val = values[i].Trim('%'); //Remove leading and ending %
                            
                            if(val.StartsWith("currentdate"))
                            {
                                DateTime now = DateTime.Now;

                                val = val.Replace("currentdate", "");
                                if (val.Length > 2)
                                {
                                    //string op = val.Substring(0, 1);
                                    string dmy = val.Substring(val.Length - 1, 1);
                                    double op = Convert.ToDouble(val.Replace(dmy, ""));

                                    if (dmy == "d")
                                        now = now.AddDays(op);
                                    if (dmy == "m")
                                        now = now.AddMonths(Convert.ToInt16(op));
                                    if (dmy == "y")
                                        now = now.AddYears(Convert.ToInt16(op));
                                }
                                
                                values[i] = now.ToString("yyyy-MM-dd");
                            }

                            if (val.StartsWith("endlastyear"))
                            {
                                DateTime now = DateTime.Now;
                            }
                        }
                    }

                    searchConditionContract.Values = values;
                    if (values.Length > 1) //If it's multiple values, comparisonEnum must be "In".
                        comparisonEnum = Utility.SearchComparisonEnum.In;
                    searchConditionContract.Values = values;
                    searchConditionContract.ComparisonEnum = ((int)comparisonEnum).ToString();
                    searchConditionContract.RelationEnum = ((int)relationEnum).ToString();
                    lSearchConditionContract.Add(searchConditionContract);
                    //Write Property to Console
                    log.Info(String.Format("[{0} {2} {1}] {3} ", split[0], String.Join(";", values), comparisonEnum.ToString(), relationEnum.ToString()));
                }
            }

            // Sets values on SearchDefinitionContract
            SearchDefinitionContract searchDefinitionContract = new SearchDefinitionContract
            {
                Conditions = lSearchConditionContract.ToArray(),
                PageSize = 100000
            };

            log.Info("Resultproperties:");
            // Assembles an array of property type IDs to be returned from the document search
            //String[] resultPropertyIds = new String[ResultProperties.Length];
            List<String> resultPropertyIds = new List<String>();
            log.Info("[" + String.Join(",", ResultProperties) + "]");
            
            //Get the propertyID's
            for (int i = 0; i < ResultProperties.Length; i++)
            {
                String propertyTypeId = commonService.GetPropertyTypeID(ResultProperties[i], schemaCulture);
                if (propertyTypeId != null)
                    resultPropertyIds.Add(propertyTypeId);
            }
            


            log.Info("Searching for documents...");
            // Searches for document
            searchResultContract = this.SearchClient.Search(connectionID, searchDefinitionContract, resultPropertyIds.ToArray() /* resultProperties */,
                    null /* resume point */, customSecurityToken: null);
                
            return searchResultContract;
        }

        /// <summary>
        /// Searches for documents based on a SearchConditionContract-Array
        /// </summary>
        /// <param name="commonService"></param>
        /// <param name="connectionID">Connection ID to we</param>
        /// <param name="searchConditions">Array of searchconditions to search for.</param>
        /// <returns></returns>
        public SearchResultContract SearchDocument(CommonService commonService, string connectionID, SearchDefinitionContract searchDefinitionContract,string resumePoint)
        {
            SearchResultContract firstSearchResultContract = new SearchResultContract();
            try
            {
                //First search
                firstSearchResultContract = this.SearchClient.Search(connectionID, searchDefinitionContract,null,
                                        resumePoint, customSecurityToken: null);

                return firstSearchResultContract;
            }
            catch (Exception e)
            {
                log.Error(string.Format("An error happened while trying to search for documents. Message: {0}", e.Message));
                throw e;
            }
        }
    }
}
