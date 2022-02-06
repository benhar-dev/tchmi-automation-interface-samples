namespace BuildProject
{
    using Beckhoff.TwinCAT.HMI.Automation;
    using TcHmiAutomation;

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
            
            Utilities.WaitForDebugger();

            if (hmiPrj != null)
                vsHmi.CloseSolution();

            for (var i = 0; i < 2; ++i)
            {
                Utilities.InitProject(vsHmi, Name, out hmiPrj, 30);

                System.Console.WriteLine("Name: {0}", hmiPrj.Name);
                System.Console.WriteLine("Childs: {0}", hmiPrj.ChildCount);
                System.Console.WriteLine("PathName. {0}", hmiPrj.PathName);
                System.Console.WriteLine("Parent: {0}", hmiPrj.Parent == null ? "-" : hmiPrj.Parent.Name);

                hmiPrj.Build("Debug", true);
                var info = hmiPrj.GetProjectInformation();
                System.Console.WriteLine($"ProjectDirectory: {info.ProjectDirectory}");
                System.Threading.Thread.Sleep(2 * 1000);
                if (hmiPrj.LookupChild("Desktop.view") is ITcHmiFile desktopView)
                {
                    desktopView.Open();
                    System.Threading.Thread.Sleep(5 * 1000);
                }

                hmiPrj.Clean(true);

                vsHmi?.CloseSolution();
            }

            Utilities.CloseVisualStudio(vsHmi);
        }
    }
}