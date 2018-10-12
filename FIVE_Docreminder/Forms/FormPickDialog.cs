using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace docreminder
{
    public partial class FormPickDialog : Form
    {
        public FormPickDialog(List<string> lChooseableValues,object sender)
        {
            InitializeComponent();
            //this.SetDesktopLocation(Cursor.Position.X-30, Cursor.Position.Y);
            //this.SetDesktopLocation(((Button)sender).
            //Point locationOnForm = ((Button)(sender)).(((Button)sender).FindForm().PointToClient((((Button)sender).Parent.PointToScreen(((Button)sender)).Location));
            
            Control control = (Control)sender;
            Point pnt = control.PointToScreen(Point.Empty);
            //pnt.X = pnt.X - control.Width / 2;
            //pnt.Y = pnt.Y - control.Height / 2;
            this.SetDesktopLocation(pnt.X,pnt.Y);


            comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox1.AutoCompleteSource = AutoCompleteSource.ListItems;
            BindingSource bs = new BindingSource();
            bs.DataSource = lChooseableValues;
            comboBox1.DataSource = bs;
            comboBox1.Focus();
            comboBox1.Refresh();
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void FormPickDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                }

            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }
    }
}
