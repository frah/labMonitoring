using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Windows.Forms;

namespace LabMonitoring
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            /* Set trace listener */
            Trace.Listeners.Remove("Default");
            StreamWriter sw = new StreamWriter("labMonitoring.log") { AutoFlush = true };
            Trace.Listeners.Add(new TextWriterTraceListener(TextWriter.Synchronized(sw), "Log"));
            FileVersionInfo ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Trace.WriteLine(string.Format(">>> {0} {1} <<<", ver.ProductName, ver.ProductVersion));

            /* Set error handler for unhandled exception */
            Application.ThreadException += (x, y) => { 
                Trace.WriteLine(y.Exception.Message + " at " + y.Exception.Source);
                Trace.WriteLine(y.Exception.StackTrace);
            };
            AppDomain.CurrentDomain.UnhandledException += (x, y) =>
            {
                Exception ex = y.ExceptionObject as Exception;
                if (ex != null)
                {
                    Trace.WriteLine(ex.Message + " at " + ex.Source);
                    Trace.WriteLine(ex.StackTrace);
                }
                Trace.WriteLine("UnhandledException is occured at " + x.ToString());
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
