namespace RenameProject
{
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

            hmiPrj.Rename($"{Name}_Renamed");

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");

            vsHmi.CloseSolution();
            Utilities.CloseVisualStudio(vsHmi);
        }
    }
}
