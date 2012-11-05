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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message + "at" + ex.Source);
                Trace.WriteLine(ex.StackTrace);
            }
        }
    }
}
