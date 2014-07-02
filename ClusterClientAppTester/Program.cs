using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClusterClientAppTester
{
    static class Program
    {
        private static Form1 form;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

                Program.form = new Form1();
                Application.Run(Program.form);
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("log.txt", ex.Message);
                Program.form.ErrorAlert(ex.Message);
            }
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            System.IO.File.WriteAllText("log.txt", e.Message);
            Program.form.Name = e.Message;
            Program.form.ErrorAlert(e.Message);
        }
    }
}
