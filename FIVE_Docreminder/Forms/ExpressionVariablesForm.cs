using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections;
using System.Xml.Serialization;

namespace docreminder.Forms
{
    public partial class ExpressionVariablesForm : Form
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

        Hashtable hTvariables = new Hashtable();

        public ExpressionVariablesForm()
        {
            InitializeComponent();
        }

        private void ExpressionVariablesForm_Load(object sender, EventArgs e)
        {

            List<KeyValuePair<string, string>> sqlConnections = new List<KeyValuePair<string, string>>();

            if (Properties.Settings.Default.SQLConnectionString != "")
            {
                try
                {
                    sqlConnections = (List<KeyValuePair<string, string>>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.SQLConnectionString, sqlConnections.GetType()));
                }
                catch
                {
                }
            }
            foreach (KeyValuePair<string, string> kvp in sqlConnections)
            {
                string[] row = { kvp.Key, kvp.Value,"Test" };
                dataGridView1.Rows.Add(row);
            }



            List<KeyValuePair<string, string>> variables = new List<KeyValuePair<string, string>>();
            if(Properties.Settings.Default.ExpressionVariables != "")
                variables = (List<KeyValuePair<string, string>>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.ExpressionVariables, variables.GetType()));

            foreach (KeyValuePair<string, string> kvp in variables)
            {
                string[] row = { kvp.Key, kvp.Value };
                dgwExpressionVariables.Rows.Add(row);
            }
            //variables.Add(new KeyValuePairy<string,string>
            //hTvariables.Add(row.Cells[0].Value, row.Cells[1].Value);
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            //Properties.Settings.Default["KendoxMarkerProperties"] = FileHelper.XmlSerializeToString(markerProperties);
            //Properties.Settings.Default["SQLConnectionString"] = txtSqlConnectionString.Text;

            List<KeyValuePair<string, string>> sqlConnections = new List<KeyValuePair<string, string>>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells.Count > 0 && row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    KeyValuePair<string, string> kvp = new KeyValuePair<string, string>();
                    kvp.Key = row.Cells[0].Value.ToString();
                    kvp.Value = row.Cells[1].Value.ToString();
                    sqlConnections.Add(kvp);
                }
            }
            if (sqlConnections.Count != 0)
                Properties.Settings.Default["SQLConnectionString"] = FileHelper.XmlSerializeToString(sqlConnections);
            else
                Properties.Settings.Default["SQLConnectionString"] = "";


            List<KeyValuePair<string, string>> variables = new List<KeyValuePair<string, string>>();

            foreach(DataGridViewRow row in dgwExpressionVariables.Rows)
            {
                if (row.Cells.Count > 0 && row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    KeyValuePair<string,string> kvp = new KeyValuePair<string,string>();
                    kvp.Key = row.Cells[0].Value.ToString();
                    kvp.Value = row.Cells[1].Value.ToString();
                    variables.Add(kvp);
                }
            }

            Properties.Settings.Default["ExpressionVariables"] = FileHelper.XmlSerializeToString(variables);



            Properties.Settings.Default.Save();
        }

        private void bOK_Click(object sender, EventArgs e)
        {
            bSave_Click(null, null);
            this.Close();
        }

        private void bTestExpression_Click(object sender, EventArgs e)
        {
            ExpressionsEvaluator expVal = new ExpressionsEvaluator();
            try
            {
                string ret = expVal.Evaluate(txtBxExpressionTest.Text).ToString();
                MessageBox.Show(ret);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2 && dataGridView1.Rows[e.RowIndex].Cells[1].Value != null)
            {
                try
                {
                    SqlConnection myConnection = new SqlConnection(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                    myConnection.Open();
                    MessageBox.Show("Connected sucessfully!", "Sucess!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    myConnection.Close();
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Error while connecting to SQL-Server." + Environment.NewLine + exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            //MessageBox.Show("Column-Index:" + e.ColumnIndex);
        }

    }
}
