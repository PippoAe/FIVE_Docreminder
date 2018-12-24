using System;
using System.Collections;
using System.Linq;
using docreminder.InfoShareService;

namespace docreminder.BO
{

    /// <summary>
    /// Defines methods for common actions via the InfoShare WCF service.
    /// 
    /// Allows the administrator to create a user, a role, a user group or a 
    /// protection domain. Furthermore, she can assign a user to a group, groups
    /// to a role, roles to a protection domain, and allowed actions to a role.
    /// 
    /// Allows a user to create a property type, a property page template, and an import 
    /// template. It also assigns properties to a property page template, a property 
    /// page template or a protection domain or an info store to an import template. 
    /// </summary>
    public class CommonService
    {
        private CommonClient CommonClient;
        private SecurityStoreContract SecurityStore;
        private SchemaStoreContract SchemaStore;
        private UserStoreContract UserStore;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CommonService()
        {
            this.CommonClient = new CommonClient();
        }

        public CommonService(CommonClient commonClient)
        {
            this.CommonClient = commonClient;
        }

        public void Init(string connAdminUserID)
        {
            this.UserStore = this.CommonClient.GetUserStore(connAdminUserID);
            this.SecurityStore = this.CommonClient.GetSecurityStore(connAdminUserID);
            this.SchemaStore = this.CommonClient.GetSchemaStore(connAdminUserID);
        }

        private void RefreshUserStore(string connAdminUserID)
        {
            this.UserStore = this.CommonClient.GetUserStore(connAdminUserID);
        }
        private void RefreshSchemaStore(string connAdminUserID)
        {
            this.SchemaStore = this.CommonClient.GetSchemaStore(connAdminUserID);
        }

        private void RefreshSecurityStore(string connAdminUserID)
        {
            this.SecurityStore = this.CommonClient.GetSecurityStore(connAdminUserID);
        }

        /// <summary>
        /// Creates a user without a password.
        /// 
        /// The user's name is passed as an argument.
	    /// Calls the overloaded method createUser with a null value for the user password. 
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="user">the user</param>
        /// <param name="displayName">the display name</param>
        /// <param name="userActive">activates or deactivates the user</param>
        /// <returns>the user id of the created user</returns>
        public string CreateUser(string connAdminUserID, string user, string displayName, bool setUserActive)
        {
            return CreateUser(connAdminUserID, user, displayName, setUserActive, null);
        }

        /// <summary>
        /// Creates a user with a password. Password can be null.
        /// 
        /// Creates a user contract and sets mandatory and optional fields on the contract. 
	    /// The user's name is passed as an argument.
	    /// Calls the CreateUser method on an instance of the InfoShareService.CommonClient
	    /// class and passes the user contract and the password as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="user">the user</param>
        /// <param name="displayName">the display name</param>
        /// <param name="userActive">activates or deactivates the user</param>
        /// <param name="userPassword">the user password</param>
        /// <returns>the user id of the created user</returns>
        public string CreateUser(string connAdminUserID, string user, string displayName, bool userActive, string userPassword)
        {
            UserContract userContract = new UserContract
            {

                // Sets mandatory fields
                DisplayName = displayName,
                LoginName = user,
                Active = userActive,

                // Sets optional fields
                LoginWithInternalPassword = true,
                CanChangeLanguage = true,
                CanChangeOptions = true,
                CanExecuteDefaultSearch = true
            };

            string userID = this.CommonClient.CreateUser(connAdminUserID, userContract, userPassword); // optional: userPassword
            this.RefreshUserStore(connAdminUserID);

            return userID;
        }

        /// <summary>
        /// Creates a group contract and sets mandatory and optional fields on the contract.  
	    /// 
	    /// The name of the group contract is passed as an argument.
	    /// Calls the CreateGroup method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the group 
	    /// contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="userGroupName">the user group name</param>
        /// <param name="userGroupDisplayName">the user group display name</param>
        /// <returns>the group id of the created group</returns>
        public string CreateUserGroup(string connAdminUserID, string userGroupName, string userGroupDisplayName)
        {

            GroupContract groupContract = new GroupContract
            {

                // Sets mandatory fields
                Name = userGroupName,
                DisplayName = userGroupDisplayName
            };

            string groupID = this.CommonClient.CreateGroup(connAdminUserID, groupContract);
            this.RefreshUserStore(connAdminUserID);

            return groupID;
        }

        /// <summary>
        /// Gets the group contract for the specified group id and assigns the users ids
        /// passed as argument to the contract.
        /// <p>
        /// Calls the UpdateGroup method on an instance of the InfoShareService.CommonClient
        /// class and passes the connection id of the administrator, and the group 
        /// contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="userIDs">an array of user ids</param>
        /// <param name="groupID">the group id</param>
        public void AssignUserToGroup(string connAdminUserID, string[] userIDs, string groupID)
        {
            GroupContract groupContract = this.GetGroupContract(groupID);
            if (groupContract != null)
            {
                groupContract.UserIds = userIDs;
                this.CommonClient.UpdateGroup(connAdminUserID, groupContract);
            }

            this.RefreshUserStore(connAdminUserID);
        }




        /// <summary>
        /// Gets the group contract for the specified group id.
        /// 
        /// Gets an array of user group contracts from the user store contract 
        /// that is passed as an argument and searches for the group contract 
        /// with the specified group id. Returns the group contract, if it finds 
        /// the contract.
        /// </summary>
        /// <param name="userStore">the user store contract</param>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="groupID">the group id</param>
        /// <returns>the group contract</returns>
        private GroupContract GetGroupContract(string groupID)
        {
            GroupContract groupContract = null;

            // Searches for the group contract with the specified group id
            foreach (GroupContract group in this.UserStore.Groups)
            {
                if (group.Id == groupID)
                {
                    // Contract found
                    groupContract = group;
                    break;
                }
            }

            if (groupContract == null)
            {
                throw new NotFoundException("No group contract found for group ID <" + groupID + ">.");
            }

            return groupContract;
        }

        /// <summary>
        /// Creates a role contract and sets mandatory and optional fields on the contract.
	    ///
	    /// The name of the role contract is passed as an argument.
        /// Calls the cCeateRole method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the role 
	    /// contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="roleName">the role name</param>
        /// <returns>the id of the created role</returns>
        public string CreateRole(string connAdminUserID, string roleName)
        {

            RoleContract roleContract = new RoleContract
            {

                // Sets mandatory field
                Name = roleName
            };

            string roleID = CommonClient.CreateRole(connAdminUserID, roleContract);
            this.RefreshSecurityStore(connAdminUserID);

            return roleID;
        }

        /// <summary>
        /// Gets the role contract for the specified role id and assigns the group ids
	    /// passed as argument to the contract.
	    /// <p>
        /// Calls the UpdateRole method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the role 
	    /// contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="groupIDs">an array of group ids</param>
        /// <param name="roleID">the role id</param>
        public void AssignGroupsToRole(string connAdminUserID, string[] groupIDs, string roleID)
        {
            RoleContract roleContract = this.GetRoleContract(roleID);
            if (roleContract != null)
            {
                roleContract.GroupIds = groupIDs;

                this.CommonClient.UpdateRole(connAdminUserID, roleContract);
            }

            this.RefreshSecurityStore(connAdminUserID);
        }

        /// <summary>
        /// Gets the role contract for the specified role id.
	    /// 
	    /// Gets an array of role contracts from the security store contract that
        /// is passed as an argument and searches for the role contract with
	    /// the specified role id. Returns the role contract, if it finds the
	    /// contract.
        /// </summary>
        /// <param name="roleID">the role id</param>
        /// <returns>the role contract</returns>
        private RoleContract GetRoleContract(string roleID)
        {
            RoleContract roleContract = null;

            // Searches for the role contract with the specified role id
            foreach (RoleContract role in this.SecurityStore.Roles)
            {
                if (role.Id == roleID) // Gets group ID
                { 
                    // Contract found
                    roleContract = role;
                    break;
                }
            }

            if (roleContract == null)
            {
                throw new NotFoundException("No role contract found for role ID <" + roleID + ">.");
            }

            return roleContract;
        }

        /// <summary>
        /// Gets the access operation category id for the specified access operation
        /// category name.
        /// 
        /// Gets an array of access operation category contracts from the security store contract
        /// that is passed as an argument and searches for the access operation category contract 
        /// with the specified access operation category name. Returns the access operation category 
        /// id, if it finds the access operation category contract.
        /// </summary>
        /// <param name="accessOperationCategoryName">the access operation category name</param>
        /// <param name="schemaCulture">the access operation category name</param>
        /// <returns>the access operation category id</returns>
        private string GetAccessOperationCategoryID(string accessOperationCategoryName, string schemaCulture)
        {
            string categoryID = null;

            // The AccessOperationCategoryContract class has a name and an id variable.
            // The name variable is of type StringGlobalContract.
            // Searches for the access operation category contract with the specified access operation category name
            foreach (AccessOperationCategoryContract category in this.SecurityStore.AccessOperationCategories)
            {
                // The stringGlobalContract class has a values variable of type List<stringGlobalEntry>.
                // This is a list of translated text, f.e. "de" "Dokument", "fr" "Dokument", etc.
                //stringGlobalContract strGlobalContract = category.getName().getValue(); // Gets the name variable value

                if (Utility.StringGlobalContains(category.Name, accessOperationCategoryName, schemaCulture))
                {
                    categoryID = category.Id;
                    break;
                }
            }
            if (categoryID == null)
            {
                throw new NotFoundException("No access operation category ID found for access operation category name <" + accessOperationCategoryName + ">.");
            }

            return categoryID;
        }

        /// <summary>
        /// Gets the role contract for the specified role id and assigns
	    /// allowed document actions respectively operations to the contract.
	    ///
        /// Invokes the UpdateRole method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the role 
	    /// contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="roleID">the role id</param>
        /// <param name="accessOperationCategoryName">the access operation category name</param>
        /// <param name="schemaCulture">the schema culture</param>
        public void AssignAllowedDocumentActionsToRole(string connAdminUserID, string roleID, string accessOperationCategoryName,
                string schemaCulture)
        {
            RoleContract roleContract = this.GetRoleContract(roleID);
            if (roleContract != null)
            {
            
                string accessOperationCategoryID = GetAccessOperationCategoryID(accessOperationCategoryName, schemaCulture);
                if (accessOperationCategoryID != null)
                {
                    AccessOperationContract[] array = this.SecurityStore.AccessOperations;
                    ArrayList allowedActionIds = new ArrayList();
                    // Loops through all access operation contracts and, if it belongs to the specified access operation category,
                    // adds it to the arrayList
                    foreach (AccessOperationContract accessOperation in this.SecurityStore.AccessOperations)
                    {
                        if (accessOperation.CategoryId == accessOperationCategoryID)
                        {
                            allowedActionIds.Add(accessOperation.Id);
                        }
                    }

                    roleContract.AllowedActionIds = (string[])allowedActionIds.ToArray(typeof(string));
                    CommonClient.UpdateRole(connAdminUserID, roleContract);
                }
            }

            this.RefreshSecurityStore(connAdminUserID);
        }

        /// <summary>
        /// Creates a protection domain contract and sets a mandatory field on the contract.
	    /// 
	    /// The name of the protection domain contract is passed as an argument.
        /// Calls the CreateProtectionDomain method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the  
	    /// contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="protectionDomainName">the protection domain name</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <returns>the id of the created protection domain</returns>
        public string CreateProtectionDomain(string connAdminUserID, string protectionDomainName, string schemaCulture)
        {            
            StringGlobalContract strGlobalContract = Utility.ConvertStringToStringGlobalContract(protectionDomainName, schemaCulture);

            ProtectionDomainContract protDomainContract = new ProtectionDomainContract
            {
                // Sets mandatory field
                Name = strGlobalContract
            };

            string protectionDomainID = CommonClient.CreateProtectionDomain(connAdminUserID, protDomainContract);
            this.RefreshSecurityStore(connAdminUserID);

            return protectionDomainID;
        }

        /// <summary>
        /// Gets the protection domain id for the specified protection domain name. 
        /// 
        /// Gets an array of protection domain contracts that is passed as an argument
        /// and searches for the protection domain contract with the specified protection 
        /// domain name. Returns the protection domain id, if it finds the protection domain
        /// contract. 
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="protectionDomainName">the protection domain name</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <returns>the protection domain id</returns>
        public string GetProtectionDomainID(string protectionDomainName, string schemaCulture)
        {
            string protectionDomainID = null;

            // Searches for the protection domain contract with the specified protection domain name
            foreach (ProtectionDomainContract protectionDomain in this.SecurityStore.ProtectionDomains)
            {
                if (Utility.StringGlobalContains(protectionDomain.Name, protectionDomainName, schemaCulture))
                {
                    protectionDomainID = protectionDomain.Id;
                    break;
                }
            }
            if (protectionDomainID == null)
            {
                throw new NotFoundException("No protection domain ID found for protection domain name <" + protectionDomainName + ">.");
            }

            return protectionDomainID;
        }

        /// <summary>
        /// Gets the protection domain contract for the specified protection domain
	    /// id and assigns the role ids passed as argument to the contract.
	    /// <p>
        /// Calls the UpdateProtectionDomain method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the  
	    /// protection domain contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="roleIDs">an array of role ids</param>
        /// <param name="protectionDomainID">the protection domain id</param>
        public void AssignRolesToProtectionDomain(string connAdminUserID, string[] roleIDs, string protectionDomainID)
        {
            ProtectionDomainContract protDomainContract = GetProtectionDomainContract(protectionDomainID);
            protDomainContract.RoleIds = roleIDs;		

            CommonClient.UpdateProtectionDomain(connAdminUserID, protDomainContract);
            this.RefreshSecurityStore(connAdminUserID);
        }

        /// <summary>
        /// Gets the protection domain contract for the specified protection domain id.
	    /// 
	    /// Gets an array of protection domain contracts from the security store that
        /// is passed as an argument and searches for the protection domain contract 
        /// with the specified protection domain id. Returns the protection domain contract, 
        /// if it finds the contract.
        /// </summary>
        /// <param name="securityStore">the security store</param>
        /// <param name="protectionDomainID">the protection domain id</param>
        /// <returns>the protection domain contract</returns>
        private ProtectionDomainContract GetProtectionDomainContract(string protectionDomainID)
        {
            ProtectionDomainContract protDomainContract = null;

            // Searches for the protection domain contract with the specified protection domain id
            foreach (ProtectionDomainContract protectionDomain in this.SecurityStore.ProtectionDomains)
            {
                if (protectionDomain.Id == protectionDomainID)
                {
                    // Contract found
                    protDomainContract = protectionDomain;
                    break;
                }
            }
            if (protDomainContract == null)
            {
                throw new NotFoundException("No protection domain contract found for protection domain ID <" + protectionDomainID + ">.");
            }

            return protDomainContract;
        }

        /// <summary>
        /// Creates a property type contract and sets mandatory and optional fields
        /// on the contract.
	    ///
	    /// The name of the property type contract is passed as an argument.
        /// Calls the CreatePropertyType method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the property 
	    /// type contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="propertyName">the property name</param>
        /// <param name="fieldType">the field type</param>
        /// <param name="multiKey">make a multikey property or not. If true, the property can have several values.</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <returns>the property type id of the created property type</returns>
        public string CreatePropertyType(string connAdminUserID, string propertyName, string fieldType, bool multiKey, string schemaCulture) 
        {   
		
		    StringGlobalContract strGlobalContract = Utility.ConvertStringToStringGlobalContract(propertyName, schemaCulture);

            PropertyTypeConfigurationContract propertyTypeConf = new PropertyTypeConfigurationContract
            {
                StringMaximumLength = 100 // mandatory
            };

            PropertyTypeContract propertyTypeContract = new PropertyTypeContract
            {
                // Set mandatory fields
                Name = strGlobalContract,
                PropertyTypePluginTypeEnum = fieldType,
                Configuration = propertyTypeConf,
                Searchable = true, // Makes properties searchable in document search

                // Sets optional fields
                Active = true,
                Multikey = multiKey
            };

            string propertyTypeID = CommonClient.CreatePropertyType(connAdminUserID, propertyTypeContract);
            this.RefreshSchemaStore(connAdminUserID);

            return propertyTypeID;
	    }

        /// <summary>
        /// Creates a property page template contract and sets mandatory fields on
	    /// the contract.
	    /// 
        /// The name of the property page template contract is passed as an argument.
	    /// Calls the CreatePropertyPageTemplate method on an instance of the 
        /// InfoShareService.CommonClient class and passes the connection id of the administrator, 
	    /// and the property page template contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="propertyPageTemplateName">the property page template name</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <returns>the property page template id of the created property page template</returns>
        public string CreatePropertyPageTemplate(string connAdminUserID, string propertyPageTemplateName, string schemaCulture)
        {
		
		    StringGlobalContract strGlobalContract = Utility.ConvertStringToStringGlobalContract(propertyPageTemplateName, schemaCulture);

            PropertyPageTemplateContract propertyPageTemplateContract = new PropertyPageTemplateContract
            {
                Name = strGlobalContract
            };

            string propertyPageTemplateID = CommonClient.CreatePropertyPageTemplate(connAdminUserID, propertyPageTemplateContract);
            this.RefreshSchemaStore(connAdminUserID);

            return propertyPageTemplateID;
	    }

        /// <summary>
        /// Gets the property page template contract for the specified property
	    /// page template id and assigns the properties passed as an argument to
	    /// the contract.
	    ///
	    /// Creates for each property a property template contract and adds them
	    /// to an array. Assigns this array of contracts to the property page
	    /// template contract.
	    /// Calls the UpdatePropertyPageTemplate method on an instance of the 
	    /// InfoShareService.CommonClient class and passes the connection id of the administrator, 
	    /// and the property page template contract as arguments.  
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="propertyPageTemplateID">the property page template id</param>
        /// <param name="propertyPageTemplateName">the property page template name</param>
        /// <param name="arrayOfPropertyTypeIDs">an array of property type ids</param>
        public void AssignPropertiesToPropertyPageTemplate(string connAdminUserID, string propertyPageTemplateID, 
			    string propertyPageTemplateName, string[] arrayOfPropertyTypeIDs)
        {
            PropertyPageTemplateContract propertyPageTemplateContract = this.GetPropertyPageTemplateContract( propertyPageTemplateID);
				
		    // Creates for each array property an instance of class PropertyTemplateContract and adds it to an array
		    // Mandatory fields must also be set either with a default value or must be visible
		    PropertyTemplateContract[] arrayOfPropertyTemplateContract = new PropertyTemplateContract[arrayOfPropertyTypeIDs.Length];
            for(int counter = 0; counter < arrayOfPropertyTypeIDs.Length; counter++)
            {
                PropertyTemplateContract propertyTemplateContract = new PropertyTemplateContract
                {
                    Required = true,
                    Visible = true,
                    CanEditDefaultValue = true,
                    PropertyTypeId = arrayOfPropertyTypeIDs[counter]
                };

                arrayOfPropertyTemplateContract[counter] = propertyTemplateContract; // adds to array
            
            }

		    propertyPageTemplateContract.PropertyTemplates = arrayOfPropertyTemplateContract;

            CommonClient.UpdatePropertyPageTemplate(connAdminUserID, propertyPageTemplateContract);
            this.RefreshSchemaStore(connAdminUserID);
        }

        /// <summary>
        /// Gets the property type contract for the specified property type id.
	    ///
	    /// Gets an array of property type contracts from the schem store contract that
        /// is passed as an argument and searches for the property type contract with 
        /// the specified property type id. Returns the property type contract, if it 
        /// finds the contract.
	    /// </summary>
        /// <param name="propertyTypeID">the property type id</param>
        /// <returns>the property type contract</returns>
        public PropertyTypeContract GetPropertyTypeContract(string propertyTypeID)
        {
            PropertyTypeContract propertyTypeContract = null;
		
		    // Gets an array of property type contracts
		    PropertyTypeContract[] arrayOfPropertyTypeContract = this.SchemaStore.PropertyTypes;
            // Searches for the property type contract with the specified property type id
            foreach (PropertyTypeContract propertyType in arrayOfPropertyTypeContract)
            {
			    if (propertyType.Id == propertyTypeID) {
                    // Contract found
				    propertyTypeContract = propertyType;				
				    break;
			    }
		    }
		    if (propertyTypeContract == null) {
			    throw new NotFoundException("No property type contract found for property type ID <" + propertyTypeID + ">.");
		    }
		
		    return propertyTypeContract;
	    }

        /// <summary>
        /// Creates an import template contract and sets mandatory and optional fields.
	    ///
	    /// The name of the import template contract is passed as an argument.
        /// Calls the CreateImportTemplate method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the import 
	    /// template contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="importTemplateName">the import template name</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <returns>the import template id of the created import template</returns>
        public string CreateImportTemplate(string connAdminUserID, string importTemplateName, string schemaCulture)
        {
            
		    StringGlobalContract strGlobalContract = Utility.ConvertStringToStringGlobalContract(importTemplateName, schemaCulture);

            ImportTemplateContract importTemplateContract = new ImportTemplateContract
            {
                // Sets mandatory field
                Name = strGlobalContract,

                // Sets optional fields
                CanChangeFolders = true,
                CanChangeInfoStore = true,
                CanChangeLifeCycle = true,
                CanChangeLinks = true,
                CanChangeProcessTemplate = true,
                CanChangeSignatureProfile = true,
                CanChangeProtectionDomain = true,
                CanChangeProperties = true
            };

            string importTemplateID = CommonClient.CreateImportTemplate(connAdminUserID, importTemplateContract);
            this.RefreshSchemaStore(connAdminUserID);

            return importTemplateID;

	    }

        /// <summary>
        /// Gets the info store id for the specified info store name.
	    ///
	    /// Gets all info store contracts in an array and searches for the info store
	    /// contract with the specified info store name. Returns the info store id,
	    /// if it finds the info store contract.  
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="infoStoreName">the info store nam</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <returns>the info store id</returns>
        public string GetInfoStoreID(string connAdminUserID, string infoStoreName, string schemaCulture)
        {
            
		    string infoStoreID = null;           

            InfoStoreContract[] arrayOfInfoStore = CommonClient.GetInfoStores(connAdminUserID);

		    foreach (InfoStoreContract infoStore in arrayOfInfoStore)
            {
			    if (Utility.StringGlobalContains(infoStore.Name, infoStoreName, schemaCulture))
                {
                    infoStoreID = infoStore.Id;
                    break;
                }               
		    }

		    if (infoStoreID == null)
            {
			    throw new NotFoundException("No info store ID found for info store name <" + infoStoreName + ">.");
		    }
		
		    return infoStoreID;
	    }

        public InfoStoreContract[] GetAllInfoStores(string connAdminUserID)
        {
            InfoStoreContract[] arrayOfInfoStore = CommonClient.GetInfoStores(connAdminUserID);
            return arrayOfInfoStore;   
        }

        public PropertyTypeContract[] GetAllPropertyTypes()
        {
            return this.SchemaStore.PropertyTypes;
        }

        public LanguageCodeContract[] GetLanguageCodes()
        {
            return this.SchemaStore.LanguageCodes;
        }

        /// <summary>
        /// Gets the import template contract for the specified import template id
	    /// and assigns the info store id to the contract.
	    ///
        /// Calls the UpdateImportTemplate method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the import 
	    /// template contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="importTemplateID">the import template id</param>
        /// <param name="infoStoreID">the info store id</param>
        public void AssignInfoStoreToImportTemplate(string connAdminUserID, string importTemplateID, string infoStoreID)
        {
            ImportTemplateContract importTemplateContract = this.GetImportTemplateContract(importTemplateID);
		    if (importTemplateContract != null)
            {
			    importTemplateContract.InfoStoreId = infoStoreID;

                CommonClient.UpdateImportTemplate(connAdminUserID, importTemplateContract);
		    }

            this.RefreshSchemaStore(connAdminUserID);
        }

        /// <summary>
        /// Gets the import template contract for the specified import template id
	    /// and assigns the protection domain id to the contract.
	    ///
        /// Calls the UpdateImportTemplate method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the import 
	    /// template contract as arguments.
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="importTemplateID">the import template id</param>
        /// <param name="protectionDomainID">the protection domain id</param>
        public void AssignProtectionDomainToImportTemplate(string connAdminUserID, string importTemplateID, 
			    string protectionDomainID)
        {
            ImportTemplateContract importTemplateContract = GetImportTemplateContract(importTemplateID);
		    if (importTemplateContract != null)
            {
			    importTemplateContract.ProtectionDomainId = protectionDomainID;
                this.CommonClient.UpdateImportTemplate(connAdminUserID, importTemplateContract);
		    }

            this.RefreshSchemaStore(connAdminUserID);
        }

        /// <summary>
        /// Gets the import template contract for the specified import template id
	    /// and assigns the property page template id to the contract.
	    ///
	    /// Calls the UpdateImportTemplate method on an instance of the InfoShareService.CommonClient
	    /// class and passes the connection id of the administrator, and the import 
	    /// template contract as arguments.
	    ///
        /// </summary>
        /// <param name="connAdminUserID">the connection id of the administrator</param>
        /// <param name="importTemplateID">the import template id</param>
        /// <param name="propertyPageTemplateID">the property page template id</param>
        public void AssignPropertyPageTemplateToImportTemplate(string connAdminUserID, string importTemplateID, 
			    string propertyPageTemplateID)
        {
            ImportTemplateContract importTemplateContract = this.GetImportTemplateContract(importTemplateID);
		    if (importTemplateContract != null) {
			    importTemplateContract.PropertyPageTemplateId = propertyPageTemplateID;

                this.CommonClient.UpdateImportTemplate(connAdminUserID, importTemplateContract);
		    }

            this.RefreshSchemaStore(connAdminUserID);
        }

        /// <summary>
        /// Gets the property template contract for the specified import template id.
	    ///
        /// Gets an array of import template contracts from the schema store contract
        /// that is passed as an argument and searches for the import template contract 
        /// with the specified import template id. Returns the import template contract, 
        /// if it finds the contract.
        /// </summary>
        /// <param name="connectionID">the connection id</param>
        /// <param name="importTemplateID">the import template id</param>
        /// <returns>the import template contract</returns>
        public ImportTemplateContract GetImportTemplateContract(string importTemplateID) 
        {
            ImportTemplateContract importTemplateContract = null;

            // Searches for the import template contract with the specified import template id
            foreach (ImportTemplateContract importTemplate in this.SchemaStore.ImportTemplates)
            {
			    if (importTemplate.Id == importTemplateID)
                {
                    // Contract found
				    importTemplateContract = importTemplate;				
				    break;
			    }
		    }
		    if (importTemplateContract == null)
            {
			    throw new NotFoundException("No import template contract found for import template ID <" + importTemplateID + ">.");
		    }
		
		    return importTemplateContract;
	    }

        /// <summary>
        /// Gets the property type name for the specified property type id.
	    ///
	    /// Gets an array of property type contracts that is passed as an argument and searches
        /// for the property type contract with the specified property type id. Returns the property
	    /// type name, if it finds the property type contract. 
        /// </summary>
        /// <param name="propertyTypeID">the property type id</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <returns>the property type name</returns>
        public string GetPropertyTypeName(string propertyTypeID, string schemaCulture)
        {
            string propertyTypeName = null;

		    foreach (PropertyTypeContract propertyType in this.SchemaStore.PropertyTypes)
            {
			    if (propertyType.Id == propertyTypeID)
                {
                    propertyTypeName = Utility.GetValue(propertyType.Name, schemaCulture);
                    if(propertyTypeName == null)
                    {
                        var hit = propertyType.Name.Values.Where(x => x.Text != null).First();
                        propertyTypeName = hit.Text;
                        log.Warn(string.Format("Property name for '{0}' not found in culture '{1}' but in culture '{2}'! Configure '{3}' in all cultures!", propertyTypeID, schemaCulture, hit.Culture,propertyTypeName));
                    }
                    break;
                }
		    }
            
            if(propertyTypeName == null)
                log.Error(string.Format("Property name for '{0}' not found in culture '{1}'! Check spelling!", propertyTypeID, schemaCulture));

            return propertyTypeName;
	    }

        /// <summary>
        /// Gets the property type id for the specified property type name.
	    /// <p>
	    /// Gets an array of property type contracts from the schema store contract 
        /// that is passed as an argument and searches for the property type contract 
        /// with the specified property type name. Returns the property type id, if 
        /// it finds the property type contract.
        /// </summary>
        /// <param name="propertyTypeName">the property type name</param>
        /// <param name="schemaCulture">the schema culture</param>
        /// <returns>the property type id</returns>
        public string GetPropertyTypeID(string propertyTypeName, string schemaCulture)
        {
            string propertyTypeID = null;
            foreach (PropertyTypeContract propertyType in this.SchemaStore.PropertyTypes)
            {
                if(Utility.StringGlobalContains(propertyType.Name, propertyTypeName, schemaCulture))
                {
                    propertyTypeID = propertyType.Id;
                    break;
                }   
            }


            if (propertyTypeID == null) {
                foreach (PropertyTypeContract propertyType in this.SchemaStore.PropertyTypes)
                {
                    var hits = propertyType.Name.Values.Where(x => x.Text == propertyTypeName).Count();

                    if (hits > 0)
                    {
                        var hit = propertyType.Name.Values.Where(x => x.Text == propertyTypeName).First();
                        log.Warn(string.Format("PropertyTypeID for '{0}' not found in culture '{1}' but in culture '{2}'! Configure '{0}' in all cultures! ", propertyTypeName, schemaCulture, hit.Culture));
                        return propertyType.Id;
                    }
                }
                log.Error("No property type ID found for property type name '" + propertyTypeName + "' with culture '" + schemaCulture + "'. Check spelling and culture!");
		    }
		
		    return propertyTypeID;
	    }


        /// <summary>
        /// Gets the property page template contract for the specified property page 
	    /// template id.
	    ///
	    /// Gets an array of property page template contracts from the schema store
        /// contract that is passed as an argument and searches for the property
        /// page template contract with the specified property page template id.
        /// Returns the property page template contract, if it finds the contract.
        /// </summary>
        /// <param name="propertyPageTemplateID">the property page template id</param>
        /// <returns>the property page template contract</returns>
        public PropertyPageTemplateContract GetPropertyPageTemplateContract (string propertyPageTemplateID)
        {
            PropertyPageTemplateContract propertyPageTemplateContract = null;
		
            // Searches for the property page template contract with the specified property page template id
		    foreach (PropertyPageTemplateContract template in this.SchemaStore.PropertyPageTemplates)
            {
			    if (template.Id == propertyPageTemplateID)
                {
                    // Contract found
				    propertyPageTemplateContract = template;				
				    break;
			    }
		    }

            if (propertyPageTemplateContract == null)
            {
			    throw new NotFoundException("No property page template contract found for property page template ID <" + propertyPageTemplateID + ">.");
		    }
		
		    return propertyPageTemplateContract;
	    }
    }
}