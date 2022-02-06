namespace TmcHandling
{
    using System;
    using System.IO;
    using Beckhoff.TwinCAT.HMI.Automation;

    internal class Program
    {
        private static string Name => typeof(Program).Assembly.GetName().Name;

        private static string PathDir => Path.GetDirectoryName(typeof(Program).Assembly.FullName);

        private static void Main()
        {
            Utilities.Init(Name,
                out var vsHmi,
                out var hmiPrj,
                waitForDebugger: false,
                version: Defaults.VsVersion.AutoMode,
                isExperimental: false);

            var srv = hmiPrj.GetServerInterface();

            var res1 = srv.AttachTmcFile("TMC1", @"Testdata\AnalyticsProject.tmc");
            Console.WriteLine($"Result #1: {res1}");
            
            var res2 = srv.AttachTmcFile("TMC2", Path.Combine(PathDir, @"Testdata\AnalyticsProject.tmc"));
            Console.WriteLine($"Result #2: {res2}");

            //srv.DetachTmcFile("TMC2");

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");

            vsHmi.SaveAllFiles();
            Utilities.CloseVisualStudio(vsHmi);
        }
    }
}
