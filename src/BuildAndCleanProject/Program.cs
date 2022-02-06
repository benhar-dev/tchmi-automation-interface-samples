namespace BuildAndCleanProject
{
    using System;
    using Beckhoff.TwinCAT.HMI.Automation;

    internal class Program
    {
        private static string Name => typeof(Program).Assembly.GetName().Name;

        private static void Main()
        {
            Utilities.Init(Name,
                out var vsHmi,
                out var hmiPrj,
                waitForDebugger: false,
                version: Defaults.VsVersion.AutoMode,
                isExperimental: false);

            hmiPrj?.Build();
            hmiPrj?.Clean(true);

            Console.WriteLine("Enter any key...");
            Console.ReadKey();

            vsHmi.CloseSolution();
            Utilities.CloseVisualStudio(vsHmi);
        }
    }
}
