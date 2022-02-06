namespace CreateServerExtension
{
    using EnvDTE;
    using System;
    using System.Linq;
    using Beckhoff.TwinCAT.HMI.Automation;

    internal class Program
    {
        private static string Name => typeof(Program).Assembly.GetName().Name;

        private static void Main()
        {
            //const string hmiTemplate = "ServerExtensionCSharp";
            const string hmiTemplate = "ServerExtensionCSharpDotNetCore";
            //const string hmiTemplate = "ServerExtensionCSharpReflection";

            var dte2 = Utilities.Init(Name,
                out var vsHmi,
                out var hmiPrj,
                waitForHmiReady: false,
                waitForDebugger: false,
                version: Defaults.VsVersion.AutoMode,
                isExperimental: false);

            for (var i = 0; i < 10; ++i)
                System.Threading.Thread.Sleep(500);

            var slnBuild = dte2.Solution.SolutionBuild;
            slnBuild.Build(true);
            var lastBuildInfo = slnBuild.LastBuildInfo;
            var buildState = slnBuild.BuildState;

            var panes = dte2.ToolWindows.OutputWindow.OutputWindowPanes;

            var isSucceeded = false;

            foreach (OutputWindowPane pane in panes)
            {
                if (!pane.Name.Contains("Build")) continue;

                //pane.OutputString(buildMessage + "\n");
                pane.Activate();
                var txtdoc = pane.TextDocument;
                txtdoc.Selection.SelectAll();
                var selectedText = txtdoc.Selection.Text;

                var lines = selectedText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var resultLine = lines.Last()?.Trim();

                // Example result:
                // ========== Build: 1 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========
                if (!string.IsNullOrWhiteSpace(resultLine))
                    isSucceeded = resultLine.IndexOf("Build: 1 succeeded", StringComparison.OrdinalIgnoreCase) != -1;

                break;
            }

            Console.WriteLine("LastBuildInfo: {0}", lastBuildInfo);
            Console.WriteLine("BuildState: {0}", buildState);
            Console.WriteLine("IsSucceeded: {0}", isSucceeded);
            
            Console.ReadKey();
        }
    }
}
