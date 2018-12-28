using System;
using System.Collections.Generic;
using System.Drawing;
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
            checkBox3.Checked = Properties.Settings.Default.GroupingInheritMarkerProperties;


            //SearchConditions
            List<InfoShareService.SearchConditionContract> searchonlist = new List<InfoShareService.SearchConditionContract>();
            if (Properties.Settings.Default.GroupingSearchProperties != "")
                searchonlist = (List<InfoShareService.SearchConditionContract>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.GroupingSearchProperties, searchonlist.GetType()));

            //Fill SearchProperties
            foreach (InfoShareService.SearchConditionContract searchcon in searchonlist)
            {
                string comparisonEnum = Enum.GetName(typeof(BO.Utility.SearchComparisonEnum), Convert.ToInt16(searchcon.ComparisonEnum));
                string relationEnum = Enum.GetName(typeof(BO.Utility.SearchRelationEnum), Convert.ToInt16(searchcon.RelationEnum));

                string[] row = { WCFHandler.GetInstance.GetPropertyTypeName(searchcon.PropertyTypeId), comparisonEnum, string.Join(";", searchcon.Values), relationEnum };
                dgwSearchProperties.Rows.Add(row);
            }
        }

        private void SaveConfig()
        {
            Properties.Settings.Default["GroupingAddParent"] = checkBox1.Checked;
            Properties.Settings.Default["GroupingZipped"] = radioButton1.Checked;
            Properties.Settings.Default["GroupingZipName"] = textBox1.Text;
            Properties.Settings.Default["GroupingSendWithoutChild"] =  checkBox2.Checked;
            Properties.Settings.Default["GroupingInheritMarkerProperties"] = checkBox3.Checked;

            //New SearchProperties
            List<InfoShareService.SearchConditionContract> searchConditions = new List<InfoShareService.SearchConditionContract>();
            foreach (DataGridViewRow row in dgwSearchProperties.Rows)
            {
                if (row.Cells.Count > 0 && row.Cells[0].Value != null)
                {
                    InfoShareService.SearchConditionContract condition = new InfoShareService.SearchConditionContract
                    {
                        //Translate propertyname to ID
                        PropertyTypeId = WCFHandler.GetInstance.GetPropertyTypeID(row.Cells[0].Value.ToString())
                    };

                    if (row.Cells[1].Value != null)
                    {
                        //BO.Utility.SearchComparisonEnum sce;
                        Enum.TryParse(row.Cells[1].Value.ToString(), out BO.Utility.SearchComparisonEnum sce);
                        condition.ComparisonEnum = ((int)sce).ToString();
                    }
                    else
                        condition.ComparisonEnum = ((int)BO.Utility.SearchComparisonEnum.None).ToString();

                    if (row.Cells[3].Value != null && row.Cells[3].Value.ToString() == "OR")
                        condition.RelationEnum = ((int)BO.Utility.SearchRelationEnum.OR).ToString();
                    else
                        condition.RelationEnum = ((int)BO.Utility.SearchRelationEnum.AND).ToString();

                    if (row.Cells[2].Value != null)
                    {
                        condition.Values = row.Cells[2].Value.ToString().Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                        condition.Values = new string[] { "" };

                    searchConditions.Add(condition);
                }
            }
            Properties.Settings.Default["GroupingSearchProperties"] = FileHelper.XmlSerializeToString(searchConditions);

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
        /// Get String Collection for Documenttype-Properties
        /// </summary>
        /// <returns></returns>
        public AutoCompleteStringCollection PropertyListDropDown(bool onlyEditable = false)
        {
            AutoCompleteStringCollection asc = new AutoCompleteStringCollection();
            try
            {
                //foreach (string sPropName in mainform.webserviceHandler.getAllPropertyTypes(Properties.Settings.Default.Culture, all, onlyChangeable))
                foreach (string sPropName in WCFHandler.GetInstance.GetAllPropertyTypes(onlyEditable))
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
                    prodCode.AutoCompleteCustomSource = PropertyListDropDown();
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
    }
}
