namespace SymbolInformation
{
    using Beckhoff.TwinCAT.HMI.Automation;
    using TcHmiAutomation;
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

            #region internal symbols

            var symbolA = hmiPrj.GetInternalSymbolInstance("Surname", "Mueller", "String");
            hmiPrj.AddInternalSymbol(symbolA);

            #endregion

            var serverAi = hmiPrj.GetServerInterface();
            var mappedSymbols = serverAi.GetMappedSymbols();
            ShowMappings(mappedSymbols);
            Console.WriteLine("Number of mapped Symbols: {0}", mappedSymbols.Length);

            // create offline symbols
            // "ADS.Config::RUNTIMES::PLC1::PORT"
            var mapRes = serverAi.MapOfflineSymbol("OfflineSymbolOfPort", "ADS.Config::RUNTIMES::PLC1::PORT", "integer", online: false);
            if (!mapRes || serverAi.LastError.Length > 0)
                Console.WriteLine("Mapping failed: {0}", serverAi.LastError);

            ShowMappings(mappedSymbols);
            Console.WriteLine("Number of mapped Symbols: {0}", mappedSymbols.Length);

            var memoryUsage = serverAi.ReadSymbol("Diagnostics::MEMORYUSAGE");
            Console.WriteLine("Memory Usage [MB]: {0}", memoryUsage.Value);

            var r1 = serverAi.MapSymbol("MemoryUsage", "Diagnostics::MEMORYUSAGE", "TcHmiSrv");
            if (!r1 || serverAi.LastError.Length > 0)
                Console.WriteLine("Mapping failed: {0}", serverAi.LastError);

            serverAi.RefreshSymbols();
            System.Threading.Thread.Sleep(5);

            mappedSymbols = serverAi.GetMappedSymbols();
            Console.WriteLine("Number of mapped Symbols: {0}", mappedSymbols.Length);

            memoryUsage = serverAi.ReadSymbol("MemoryUsage");
            Console.WriteLine("Memory Usage [MB]: {0}", memoryUsage.Value);

            var r2 = serverAi.UnMapSymbol("MemoryUsage", "TcHmiSrv");
            if (!r2 || serverAi.LastError.Length > 0)
                Console.WriteLine("Mapping failed: {0}", serverAi.LastError);

            mappedSymbols = serverAi.GetMappedSymbols();
            Console.WriteLine("Number of mapped Symbols: {0}", mappedSymbols.Length);

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");

            vsHmi.CloseSolution();
            Utilities.CloseVisualStudio(vsHmi);
        }

        private static void ShowMappings(ITcHmiMappedSymbol[] mappedSymbols)
        {
            foreach (var sym in mappedSymbols)
            {
                if (sym == null) continue;
                Console.WriteLine("Symbol: {0}", sym.MappedName);
            }
        }
    }
}
