using NCalc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace docreminder
{
    public static class NEWExpressionsEvaluator
    {
        [Serializable]
        [XmlType(TypeName = "CustomKeyValuePair")]
        public struct KeyValuePair<K, V>
        {
            public K Key
            { get; set; }

            public V Value
            { get; set; }
        }

        [Serializable]
        [XmlType(TypeName = "SqlConnecton")]
        public struct sqlItem<K, V>
        {
            public K name
            { get; set; }

            public V connection
            { get; set; }
        }

        private static List<KeyValuePair<string, string>> variables;
        private static List<KeyValuePair<string, string>> sqlConnectionsList;
        private static List<sqlItem<string, SqlConnection>> sqlConnections = new List<sqlItem<string, SqlConnection>>();


        static NEWExpressionsEvaluator()
        {
            variables = new List<KeyValuePair<string, string>>();
            if(Properties.Settings.Default.ExpressionVariables != "")
                variables = (List<KeyValuePair<string, string>>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.ExpressionVariables, variables.GetType()));


            if (Properties.Settings.Default.SQLConnectionString != "")
            {
                sqlConnectionsList = new List<KeyValuePair<string, string>>();
                sqlConnectionsList = (List<KeyValuePair<string, string>>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.SQLConnectionString, sqlConnectionsList.GetType()));
                try
                {
                    foreach (KeyValuePair<string, string> kvp in sqlConnectionsList)
                    {
                        sqlItem<string, SqlConnection> con = new sqlItem<string, SqlConnection>();

                        SqlConnection sqlConnection = new SqlConnection(kvp.Value);
                        con.connection = sqlConnection;
                        con.name = kvp.Key;
                        sqlConnections.Add(con);

                    }
                    //sqlCon = new SqlConnection(Properties.Settings.Default.SQLConnectionString);
                }
                catch (Exception exp)
                {

                }
            }

        }

        public static string Evaluate(string input, DataGridViewRow row = null, KXWS.SDocument docinfo = null,bool testmode = false)
        {
            Hashtable hTenteredIDX = new Hashtable();

            string returnvalue = "";
            Expression e = new Expression(input);
            e.Parameters["TodayDate"] = DateTime.Now.Date;
            e.Parameters["TodayTime"] = DateTime.Now;
            int year = DateTime.Now.Year;
            e.Parameters["EndYear"] = new DateTime(year, 12, 31);

            foreach (KeyValuePair<string, string> kvp in variables)
            {
                e.Parameters[kvp.Key] = new Expression(kvp.Value);
            }
            

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
            {
                if (name == "AddDays")
                {
                    var date1 = args.Parameters[0].Evaluate();
                    var days = args.Parameters[1].Evaluate();

                    //Check if days are a TimeSpan
                    if (days.GetType() == typeof(TimeSpan))
                    {
                        args.Result = ((DateTime)date1).Add((TimeSpan)days);
                    }
                    //Check if are a string.
                    else if (days.GetType() == typeof(string))
                    {
                        args.Result = ((DateTime)date1).AddDays(0);
                    }

                    //Else they are a double. (Use AddDays)
                    else
                    {
                        args.Result = ((DateTime)date1).AddDays(Convert.ToDouble(days));
                    }

                }

                if (name == "DateDiff")
                {
                    var date1 = args.Parameters[0].Evaluate();
                    var date2 = args.Parameters[1].Evaluate();

                    //args.Result = ((DateTime)args.Parameters[0].Evaluate() - (DateTime)args.Parameters[1].Evaluate()).TotalDays;

                    args.Result = (DateTime)date2 - (DateTime)date1;
                }

                if (name == "IDX")
                {
                    string index = args.Parameters[0].Evaluate().ToString();
                    string idxvalue = "";

                    //Get IDX Value from Docinfo if available.
                    if (docinfo != null)
                    {
                        foreach (KXWS.SDocumentProperty prop in docinfo.documentProperties)
                        {
                            if (prop.name == index){
                                if(prop.propertyValues.Count() > 0)
                                    idxvalue = prop.propertyValues[0];
                            }
                        }
                    }

                    //Else get it from the row.
                    else
                    {
                        if(row != null)
                            idxvalue = row.Cells[index].Value.ToString();
                        //If nothing helps, let user pick it.
                        else
                        {
                            if (hTenteredIDX.ContainsKey(index))
                            {
                                idxvalue = hTenteredIDX[index].ToString();
                            }
                            else
                            {

                                FormInputDialog inputDialog = new FormInputDialog("IDX Value", "Set Value for '" + index.ToString() + "':", "OK");
                                if (inputDialog.ShowDialog(null) == DialogResult.OK)
                                {
                                    //recipient = inputDialog.txtBxInput.Text;
                                    idxvalue = inputDialog.txtBxInput.Text;
                                    hTenteredIDX.Add(index.ToString(), idxvalue);
                                    inputDialog.Dispose();
                                }
                            }
                        }
                    }

                    //Check if its a Date
                    DateTime idxValueDate;
                    string[] formats = { "dd.MM.yyyy", "dd.MM.yyyy HH:mm", "dd.MM.yyyy HH:mm:ss" };
                    if (DateTime.TryParseExact(idxvalue, formats, new CultureInfo(Properties.Settings.Default.Culture), System.Globalization.DateTimeStyles.None, out idxValueDate))
                    {
                        args.Result = idxValueDate;
                        return;
                    }

                    //Check if its a number
                    int idxValueInt;
                    if (int.TryParse(idxvalue, out idxValueInt) && idxValueInt != 0)
                    {
                        args.Result = idxValueInt;
                        return;
                    }

                    //Check if its a Decimal
                    decimal idxValueDecimal;
                    if (decimal.TryParse(idxvalue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out idxValueDecimal) && idxValueDecimal != 0)
                    {
                        args.Result = idxValueDecimal;
                        return;
                    }

                    //Else, forward it as a string.
                    args.Result = idxvalue;

                }

                if (name == "SQL")
                {
                    string connection = ((NCalc.Domain.ValueExpression)(args.Parameters[0].ParsedExpression)).Value.ToString();
                    var selectstatement = args.Parameters[1].Evaluate();

                    SqlConnection sqlCon = (sqlConnections.Find(item => item.name == connection.ToString())).connection;
                    if (sqlCon == null)
                        throw new System.ArgumentException(string.Format("SQL-Connection '{0}' not defined.", connection));


                    if (sqlCon != null && sqlCon.State == ConnectionState.Closed)
                        sqlCon.Open();
                    SqlCommand command = new SqlCommand(selectstatement.ToString());
                    command.Connection = sqlCon;


                    //AEPH 03.02.2016 
                    //If testmode is on, command is only tested on server, not executed. (noexec mode)
                    //This is used to validate the markerproperties prior to output processing.
                    if (testmode)
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = "SET NOEXEC ON;";
                        //command.Connection.Open();
                        //command.ExecuteNonQuery();
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            args.Result = (string)reader[0].ToString();
                        }
                    }
                    if (args.Result == null)
                        args.Result = "";
                }

                if (name == "RegEx")
                {
                    var value = args.Parameters[0].Evaluate();
                    string expression = ((NCalc.Domain.ValueExpression)(args.Parameters[1].ParsedExpression)).Value.ToString();

                    Regex regex = new Regex(@expression);
                    Match match = regex.Match(value.ToString());

                    args.Result = match.Success;
                }

                if (name == "RegExReplace")
                {
                    var sInput = args.Parameters[0].Evaluate();
                    var sReplacement = args.Parameters[1].Evaluate();
                    string expression = ((NCalc.Domain.ValueExpression)(args.Parameters[2].ParsedExpression)).Value.ToString();

                    Regex regex = new Regex(@expression);
                    args.Result = regex.Replace(sInput.ToString(), sReplacement.ToString());
                }

                if (name == "SS")
                {
                    var sInput = args.Parameters[0].Evaluate();
                    string arg1 = args.Parameters[1].Evaluate().ToString();
                    string arg2 = args.Parameters[2].Evaluate().ToString();

                    int iStartIndex;
                    int.TryParse(arg1, out iStartIndex);
                    
                    int iEndIndex;
                    int.TryParse(arg2, out iEndIndex);
                    string output = sInput.ToString();
                    args.Result = output.Substring(iStartIndex, iEndIndex);
                }
            };

            if (e.HasErrors())
            {
                //MessageBox.Show(e.Error);
                throw new Exception(e.Error);
            }

            else
            {
                try
                {
                    var output = e.Evaluate();
                    //AEPH 04.02.2016
                    //decimal dec;
                    //Decimal.TryParse(output.ToString(),out dec);
                    //returnvalue = FileHelper.ToInvariantString(output);
                    //AEPH 05.02.2016
                    //Decimal sign is used based on system settings.
                    returnvalue = output.ToString();
                }

                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            return returnvalue;
        }

    }
}
