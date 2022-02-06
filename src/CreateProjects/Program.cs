namespace CreateProjects
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

            Console.WriteLine("Project: {0}", hmiPrj?.DteProject?.UniqueName);

            for (var i = 0; i < 2; ++i)
            {
                var localName = $"{Name}{i + 1}";

                Utilities.InitProject(vsHmi, localName, out var localNewHmiPrj);
                Console.WriteLine("Project: {0}", localNewHmiPrj?.DteProject?.UniqueName);
            }
        }
    }
}
