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

            /// <remarks/>
            NONE,

            /// <remarks/>
            UPDATE,

            /// <remarks/>
            ADD,

            /// <remarks/>
            DELETE,
        }
    }
}
