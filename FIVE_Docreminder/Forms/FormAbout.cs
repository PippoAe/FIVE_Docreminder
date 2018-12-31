using System.Windows.Forms;
using System.Reflection;

namespace docreminder
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
            label1.Text = Application.ProductName +" Version: " + Application.ProductVersion;
            label2.Text = Application.CompanyName;
            label3.Text = GetCopyright();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.fiveinfo.ch");
        }

        private string GetCopyright()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            object[] obj = asm.GetCustomAttributes(false);
            foreach (object o in obj)
            {
                if (o.GetType() ==
                    typeof(System.Reflection.AssemblyCopyrightAttribute))
                {
                    AssemblyCopyrightAttribute aca =
            (AssemblyCopyrightAttribute)o;
                    return aca.Copyright;
                }
            }
            return string.Empty;
        }
    }
}
