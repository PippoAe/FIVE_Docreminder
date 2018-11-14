using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace docreminder.Forms
{
    public partial class FormSchedule : Form
    {
        private ConsoleWriter log;
        private static readonly log4net.ILog log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FormSchedule()
        {
            log = ConsoleWriter.GetInstance;
            InitializeComponent();
        }

        private void FormSchedule_Load(object sender, EventArgs e)
        {
            List<StopDay> stopDays = new List<StopDay>();
            if (Properties.Settings.Default.StopSchedule != "")
            {
                try
                {
                    stopDays = (List<StopDay>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.StopSchedule, stopDays.GetType()));
                }
                catch
                {
                    log4.Info("Error while loading Schedule from settings, loading blank instead.");
                }
            }
            
            foreach (StopDay dayElem in stopDays)
            {
                dataGridView1.Rows.Add(dayElem.dayofweek.ToString(), dayElem.time.ToString("HH:mm:ss"));
            }
        }

        private void bOK_Click(object sender, EventArgs e)
        {
            bSave_Click(null, null);
            this.DialogResult = DialogResult.OK;
            //this.Close();
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            List<StopDay> stopDays = new List<StopDay>();
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                        {
                            StopDay dayelem = new StopDay();
                            DayOfWeek myDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), (string)row.Cells[0].Value.ToString(), true);
                            dayelem.dayofweek = myDay;
                            //dayelem.time = TimeSpan.Parse(row.Cells[1].Value.ToString());
                            dayelem.time = Convert.ToDateTime(row.Cells[1].Value.ToString());
                            stopDays.Add(dayelem);
                        }
                    }

                Properties.Settings.Default["StopSchedule"] = FileHelper.XmlSerializeToString(stopDays);
                Properties.Settings.Default.Save();
            }

            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<StopDay> stopDays = new List<StopDay>();
            stopDays = (List<StopDay>)(FileHelper.XmlDeserializeFromString(Properties.Settings.Default.StopSchedule, stopDays.GetType()));

            DayOfWeek dayNow = DateTime.Today.DayOfWeek;
            TimeSpan timeNow = DateTime.Now.TimeOfDay;

            foreach (StopDay sd in stopDays)
            {
                if (sd.dayofweek == dayNow)
                {
                    TimeSpan stopTime = sd.time.TimeOfDay;
                    if (stopTime <= timeNow)
                    {
                        MessageBox.Show(String.Format("Wir schalten jetzt aus! stopTime:{0}, timeNow:{1}", stopTime.ToString(), timeNow.ToString()));
                    }
                    else
                    {
                        TimeSpan ts = (stopTime - timeNow);
                        string wann = ts.ToString(@"hh\:mm\:ss");
                        MessageBox.Show(String.Format("Wir schalten in {0} aus! stopTime:{1}, timeNow:{2}",wann, stopTime.ToString(), timeNow.ToString()));
                    }
                }
            }
        }
    }

    [Serializable]
    [XmlType(TypeName = "StopDays")]
    public class StopDay
    {
        private DayOfWeek _DayOfWeek;
        //private TimeSpan _Time;
        private DateTime _Time;
        public DayOfWeek dayofweek { get { return _DayOfWeek; } set { _DayOfWeek = value; } }
        //public TimeSpan time { get { return _Time; } set { _Time = value; } }
        public DateTime time { get { return _Time; } set { _Time = value; } }
    }
}
