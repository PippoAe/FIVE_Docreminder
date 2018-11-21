using System;
using System.IO;
using System.Windows.Forms;

namespace docreminder.Forms
{
    public partial class FormDocSafeSettings : Form
    {
        public FormDocSafeSettings()
        {
            InitializeComponent();
            Settings_Load();
        }

        private void Settings_Load()
        {
            //Rest-Webservice
            txtBxClientURL.Text = Properties.Settings.Default.dsRestClientURL;
            txtBxRequest.Text = Properties.Settings.Default.dsRestRequest;
            txtBxCertificateFilePath.Text = Properties.Settings.Default.dsCertFileName;

            //Sender
            txtBxSenderBUID.Text = Properties.Settings.Default.dsSenderBUID;
            txtBxSenderName.Text = Properties.Settings.Default.dsSenderName;
            txtBxLinkURL.Text = Properties.Settings.Default.dsLinkURL;
            txtBxLinkText.Text = Properties.Settings.Default.dsLinktext;

            //Document
            txtBxObjectID.Text = Properties.Settings.Default.dsSendersObjectID;
            txtBxObjectAlias.Text = Properties.Settings.Default.dsSendersObjectAlias;
            txtBxRecipientID.Text = Properties.Settings.Default.dsDocSafeID;
            txtBxDocumentID.Text = Properties.Settings.Default.dsSendersDocumentID;
            txtBxFileName.Text = Properties.Settings.Default.dsFileName;
            txtBxDocumentTitle.Text = Properties.Settings.Default.dsDocumentTitle;

            //Misc.
            txtBxFlowRef.Text = Properties.Settings.Default.dsSendersFlowRef;
            txtBxFlowAlias.Text = Properties.Settings.Default.dsSendersFlowAlias;
            txtBxAnnotation.Text = Properties.Settings.Default.dsAnnotation;

        }

        private void SaveConfig()
        {
            //Rest-Webservice
            Properties.Settings.Default["dsRestClientURL"] = txtBxClientURL.Text;
            Properties.Settings.Default["dsRestRequest"] = txtBxRequest.Text;
            Properties.Settings.Default["dsCertFileName"] = txtBxCertificateFilePath.Text;

            //Sender
            Properties.Settings.Default["dsSenderBUID"] = txtBxSenderBUID.Text;
            Properties.Settings.Default["dsSenderName"] = txtBxSenderName.Text;
            Properties.Settings.Default["dsLinkURL"] = txtBxLinkURL.Text;
            Properties.Settings.Default["dsLinktext"] = txtBxLinkText.Text;

            //Document
            Properties.Settings.Default["dsSendersObjectID"] = txtBxObjectID.Text;
            Properties.Settings.Default["dsSendersObjectAlias"] = txtBxObjectAlias.Text;
            Properties.Settings.Default["dsDocSafeID"] = txtBxRecipientID.Text;
            Properties.Settings.Default["dsSendersDocumentID"] = txtBxDocumentID.Text;
            Properties.Settings.Default["dsFileName"] = txtBxFileName.Text;
            Properties.Settings.Default["dsDocumentTitle"] = txtBxDocumentTitle.Text;


            //Misc.
            Properties.Settings.Default["dsSendersFlowRef"] = txtBxFlowRef.Text;
            Properties.Settings.Default["dsSendersFlowAlias"] = txtBxFlowAlias.Text;
            Properties.Settings.Default["dsAnnotation"] = txtBxAnnotation.Text;

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

        private void btnTestKendoxConnection_Click(object sender, EventArgs e)
        {
            DocSafe.DocSafeHandler dsHandler = new DocSafe.DocSafeHandler();
            try
            {
                MessageBox.Show(string.Format("Version information was successfully retrieved!\n {0}",dsHandler.GetVersion(txtBxClientURL.Text, txtBxCertificateFilePath.Text)),"Success!",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format("Sommething went wrong while trying to get the version info!\n {0} \n {1}",ex.Message,ex.StackTrace), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileBrowserDialog = new OpenFileDialog();

            if (txtBxCertificateFilePath.Text == "")
            {
                string progPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string templateFilePath = Path.Combine(progPath, "*.p12");
                fileBrowserDialog.InitialDirectory = templateFilePath;
                fileBrowserDialog.RestoreDirectory = true;
                fileBrowserDialog.Filter = "X509Certificate|*.p12;";
            }
            else
            {

                fileBrowserDialog.InitialDirectory = Path.GetDirectoryName(txtBxCertificateFilePath.Text).ToString();
                fileBrowserDialog.RestoreDirectory = true;
                fileBrowserDialog.Filter = "X509Certificate|*.p12;";
            }
            DialogResult result = fileBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
                txtBxCertificateFilePath.Text = fileBrowserDialog.FileName;
        }

    }
}
