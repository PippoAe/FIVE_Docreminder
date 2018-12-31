using System;
using System.Windows.Forms;

namespace docreminder
{
    public partial class FormInputDialog : Form
    {
        public FormInputDialog(string dialogtitle, string labeltext, string oktext)
        {
            
            InitializeComponent();
            this.Text = dialogtitle;
            lblText.Text = labeltext;
            btnOK.Text = oktext;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void FormInputDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                btnOK_Click(null, null);
            }  
        }

        //public string DialogBox(string recipient)
        //{
        //    this.txtBxRecipient.Text = recipient;
        //    this.txtBxRecipient.Refresh();

        //     Show testDialog as a modal dialog and determine if DialogResult = OK.
        //    if (this.ShowDialog(this) == DialogResult.OK)
        //    {
        //         Read the contents of testDialog's TextBox.
        //        this.Dispose();
        //        return this.txtBxRecipient.Text;
        //    }
        //    else
        //    {
        //        this.Dispose();
        //        return "Cancelled";
                
        //    }
        //}
    }
}
