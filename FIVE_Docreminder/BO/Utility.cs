using docreminder.InfoShareService;
using System;

namespace docreminder.BO
{

    /// <summary>
    /// Defines utility methods.
    /// </summary>
    static class Utility
    {
        public  enum SearchComparisonEnum
        {
            Equals = 0,
            NotEqual = 1,
            GreaterThan = 2,
            GreaterOrEqual = 3,
            LessThan = 4,
            LessOrEqual = 5,
            Like = 6,
            Contains = 7,
            Undefined = 8,
            In = 9,
            None = 10,
            Soundex = 11,
            NotIn = 12
        }

        public enum SearchRelationEnum
        {
            AND = 0,
            OR = 1
        }


        /// <summary>
        /// Converts the specified string to a string global contract object.
        /// </summary>
        /// <param name="str">the string to be converted</param>
        /// <param name="culture">the culture</param>
        /// <returns>a string global contract object</returns>
        public static StringGlobalContract ConvertStringToStringGlobalContract(string str, string culture)
        {

            StringGlobalEntry entry = new StringGlobalEntry
            {
                Culture = culture,
                Text = str
            };

            StringGlobalEntry[] arrayOfStringGlobalEntries = new StringGlobalEntry[1];
            arrayOfStringGlobalEntries[0] = entry;

            StringGlobalContract strGlobalContract = new StringGlobalContract
            {
                Values = arrayOfStringGlobalEntries
            };

            return strGlobalContract;
        }

        public static bool StringGlobalContains(StringGlobalContract stringGlobal, String value, String schemaCulture)
        {
            foreach (StringGlobalEntry strGlobalEntry in stringGlobal.Values)
            {
                if (strGlobalEntry.Culture == schemaCulture)
                {
                    if (strGlobalEntry.Text == value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string GetValue(StringGlobalContract stringGlobal, string schemaCulture)
        {
            foreach (StringGlobalEntry strGlobalEntry in stringGlobal.Values)
            {
                if (strGlobalEntry.Culture == schemaCulture)
                {
                    return strGlobalEntry.Text;
                }
            }

            return null;
        }

        public static string FoldeStringArray(string[] arrOfStr)
        {
            string valueStr = "";
      
            if (arrOfStr != null )
            { 
                for (int i = 0; i < arrOfStr.Length; i++)
                {
                    if (!String.IsNullOrEmpty(valueStr))
                        valueStr += ";";
                          
                    valueStr += arrOfStr[i];
                }
            }

            return valueStr;
        }
    }
}