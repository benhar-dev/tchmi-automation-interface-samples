namespace LocalizationHandling
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

            hmiPrj.AddLocalizationEntries(new []
            {
                hmiPrj.GetLocalizationEntryInstance("Key0", "Hello"),
                hmiPrj.GetLocalizationEntryInstance("Key1", "World!")
            });

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");

            vsHmi.SaveAllFiles();

            Utilities.CloseVisualStudio(vsHmi);
        }
    }
}
