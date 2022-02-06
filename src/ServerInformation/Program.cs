using Beckhoff.TwinCAT.HMI.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerInformation
{
    using System;

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

            var srv = hmiPrj.GetServerInterface();
            var srvMetadata = srv.GetMetadata();
            var jsonObj = JObject.Parse(srvMetadata);
            Console.WriteLine("Server Information: {0}", jsonObj.ToString(Formatting.Indented));

            var srvExtensions = srv.GetServerExtensions(true);
            foreach(var it in srvExtensions)
            {
                if(it == null) continue;
                Console.WriteLine("Extension Domain: {0} ({1})", it.DomainName, it.State);
            }

            var memoryUsage = srv.ReadSymbol("Diagnostics::MEMORYUSAGE");
            Console.WriteLine("Memory Usage [MB]: {0}", memoryUsage.Value);

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");

            vsHmi.CloseSolution();
            Utilities.CloseVisualStudio(vsHmi);
        }
    }
}
