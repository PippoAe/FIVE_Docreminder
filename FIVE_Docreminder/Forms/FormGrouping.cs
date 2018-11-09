using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace docreminder.Forms
{
    public partial class FormGrouping : Form
    {

        MainForm mainform;

        public FormGrouping(MainForm mainform)
        { 
            this.mainform = mainform;
            InitializeComponent();
            Settings_Load();
        }

        private void Settings_Load()
        {
            checkBox1.Checked = Properties.Settings.Default.GroupingAddParent;
            radioButton1.Checked = Properties.Settings.Default.GroupingZipped;
            radioButton2.Checked = !Properties.Settings.Default.GroupingZipped;
            textBox1.Text = Properties.Settings.Default.GroupingZipName;
            checkBox2.Checked = Properties.Settings.Default.GroupingSendWithoutChild;


            //SearchConditions
            List<KXWS.SSearchCondition> searchonlist = new List<KXWS.SSearchCondition>();
            if (Properties.Settings.Default.GroupingSearchProperties != "")
                searchonlist = (List<KXWS.SSearchCondition>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.GroupingSearchProperties, searchonlist.GetType()));

            //Fill SearchProperties
            foreach (KXWS.SSearchCondition searchcon in searchonlist)
            {
                //DataRow test = new DataRow();
                string[] row = { searchcon.propertyTypeName, searchcon.operation, string.Join(";", searchcon.propertyValueArray), searchcon.relation.ToString() };
                //row.HeaderCell.Value = String.Format("{0}", row.Index + 1)
                dgwSearchProperties.Rows.Add(row);
            }

        }

        private void SaveConfig()
        {
            Properties.Settings.Default["GroupingAddParent"] = checkBox1.Checked;
            Properties.Settings.Default["GroupingZipped"] = radioButton1.Checked;
            Properties.Settings.Default["GroupingZipName"] = textBox1.Text;
            Properties.Settings.Default["GroupingSendWithoutChild"] =  checkBox2.Checked;

            //Searchconditions
            List<KXWS.SSearchCondition> searchonlist = new List<KXWS.SSearchCondition>();

            //Searchproperties
            foreach (DataGridViewRow row in dgwSearchProperties.Rows)
            {
                if (row.Cells.Count > 0 && row.Cells[0].Value != null)
                {
                    KXWS.SSearchCondition condition = new KXWS.SSearchCondition();
                    condition.propertyTypeName = row.Cells[0].Value.ToString();

                    if (row.Cells[1].Value != null)
                        condition.operation = row.Cells[1].Value.ToString();
                    else
                        condition.operation = "NONE";

                    if (row.Cells[3].Value != null && row.Cells[3].Value.ToString() == "OR")
                        condition.relation = KXWS.Relations.OR;
                    else
                        condition.relation = KXWS.Relations.AND;


                    //condition.relation = row.Cells[2].Value.ToString();
                    if (row.Cells[2].Value != null)
                    {
                        condition.propertyValueArray = row.Cells[2].Value.ToString().Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        //condition.propertyValueArray = new string[] { row.Cells[2].Value.ToString() };
                    }
                    else
                        condition.propertyValueArray = new string[] { "" };

                    searchonlist.Add(condition);
                }
            }
            Properties.Settings.Default["GroupingSearchProperties"] = FileHelper.XmlSerializeToString(searchonlist);


            Properties.Settings.Default.Save();
        }

        private void bOK_Click(object sender, EventArgs e)
        {
            SaveConfig();
            this.Close();
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        /// <summary>
        /// Add AutoComplete with Documenttypes to DataGridView SearchProperties.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgwSearchProperties_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgwSearchProperties.CurrentCell.ColumnIndex == 0)
            {
                TextBox prodCode = e.Control as TextBox;
                if (prodCode != null)
                {
                    prodCode.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    prodCode.AutoCompleteCustomSource = ClientListDropDown(false, false);
                    prodCode.AutoCompleteSource = AutoCompleteSource.CustomSource;

                }
            }
            else
            {
                TextBox prodCode = e.Control as TextBox;
                if (prodCode != null)
                {
                    prodCode.AutoCompleteMode = AutoCompleteMode.None;
                }
            }
        }


        /// <summary>
        /// Get String Collection for Documenttype-Properties
        /// </summary>
        /// <returns></returns>
        public AutoCompleteStringCollection ClientListDropDown(bool all, bool onlyChangeable)
        {
            AutoCompleteStringCollection asc = new AutoCompleteStringCollection();
            try
            {
                foreach (string sPropName in mainform.webserviceHandler.getAllPropertyTypes(Properties.Settings.Default.Culture, all, onlyChangeable))
                {
                    asc.Add(sPropName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("For AutoComplete please connect to Kendox Server!");
            }
            return asc;
        }

        /// <summary>
        /// Add Indexes to DataGridViewRows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgwSearchProperties_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }
    }
}
