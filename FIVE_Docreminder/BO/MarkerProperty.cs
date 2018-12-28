using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace docreminder.BO
{
    public class MarkerProperty
    {
        public string propertyTypeID;
        public string[] values;
        public UpdateAction updateAction;


        public MarkerProperty()
        {
        }

        public MarkerProperty(string PropertyTypeID, string[] Values, UpdateAction UpdateAction)
        {
            propertyTypeID = PropertyTypeID;
            values = Values;
            updateAction = UpdateAction;
        }

        public enum UpdateAction
        {

            /// <summary>
            /// NONE: Does not change a document property.
            /// </summary>
            NONE,

            /// <summary>
            /// UPDATE: Overwrites the existing document property values. If the document property does not exist the new document property is added.
            /// </summary>
            UPDATE,
            
            /// <summary>
            /// ADD: Add new document property. If the document property does not exist the document property is added. 
            /// If the document property already exists the values are added to the existing ones. 
            /// Attention: To add multiple values to a document property the poperty type of the document property must be a multikey. 
            /// If values are added to an existing document property the document property must be a multikey.
            /// </summary>
            ADD,

            /// <summary>
            /// DELETE: Delete a document property (idempotend: if the document property does not exist - do nothing).
            /// </summary>
            DELETE,
        }
    }
}
