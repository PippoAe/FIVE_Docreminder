using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace docreminder
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        /// 
        enum ExitCode : int {
          Success = 0,
          InvalidLogin = 1,
          InvalidFilename = 2,
          UnknownError = 10
        }

        public static bool automode = false;
        public static string customConfigFile = "";

        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if (args.Count() > 0)
            {
                foreach (string arg in args)
                {
                    if (arg == "automode")
                        automode = true;
                    if (arg.Contains(".settings"))
                        customConfigFile = arg;
                }
            }

            Application.Run(new MainForm());

            return (int)ExitCode.Success;
        }


        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Unhandled Thread Exception");
            // here you can log the exception ...
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception).Message, "Unhandled UI Exception");
            // here you can log the exception 
        }
    }
}
