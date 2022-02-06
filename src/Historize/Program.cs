// ReSharper disable ArgumentsStyleStringLiteral

namespace Historize
{
    using System;
    using TcHmiAutomation;
    using Beckhoff.TwinCAT.HMI.Automation;

    internal class Program
    {
        private static string Name => typeof(Program).Assembly.GetName().Name;

        private static string DefaultPublishConfigName = "default";

        private static void Main()
        {
            Utilities.Init(Name,
                out var vsHmi,
                out var hmiPrj,
                waitForDebugger: false,
                version: Defaults.VsVersion.AutoMode,
                isExperimental: false);

            hmiPrj.AddNuGetPackage("https://api.nuget.org/v3/index.json", "Beckhoff.TwinCAT.HMI.SqliteHistorize", "12.742.5");

            var serverAi = hmiPrj.GetServerInterface();
            var srvExtHistorize = Utilities.WaitForDomain(
                serverAi, 
                "TcHmiSqliteHistorize", 
                waitMessage: "Wait for ENABLED domain...", 
                includeNotPopulated: false);
            Console.WriteLine("Historize: {0}", srvExtHistorize.State);

            var mapRes = serverAi.MapOfflineSymbol("OfflineSymbolOfPort", "ADS.Config::RUNTIMES::PLC1::PORT", "integer");
            if (!mapRes || serverAi.LastError.Length > 0)
                Console.WriteLine("Mapping failed: {0}", serverAi.LastError);

            var mappedSymbols = serverAi.GetMappedSymbols();

            var sym = GetMappedSymbol("OfflineSymbolOfPort", mappedSymbols);
            if(sym != null)
            {
                Console.WriteLine("Original historized symbol:");
                ShowHistorizeData(sym);

                var historizeSettings = sym.HistorizeSettings;

                if (historizeSettings == null)
                    historizeSettings = hmiPrj.GetHistorizeSettingsInstance();

                if (historizeSettings != null)
                {
                    historizeSettings.Interval = "PT7S";
                    historizeSettings.MaxEntries = 1337;
                    historizeSettings.RowLimit = 20000;
                }

                var recSettings = historizeSettings?.RecordingSettings;
                if (recSettings == null || recSettings.Length == 0)
                {
                    // add new recording setting

                    // Publish name: default
                    var recSet = hmiPrj.GetRecordingSettingsInstance();
                    recSet.PublishConfiguration = DefaultPublishConfigName;
                    recSet.Recording = true;

                    // Publish name: remote
                    var recSetRemote = hmiPrj.GetRecordingSettingsInstance();
                    recSetRemote.PublishConfiguration = "remote";
                    recSetRemote.Recording = false;

                    recSettings = new[] { recSet, recSetRemote };

                    if(historizeSettings != null) 
                        historizeSettings.RecordingSettings = recSettings;
                }
                else
                {
                    // change already available recording setting

                    foreach (var recSet in recSettings)
                    {
                        if (recSet?.PublishConfiguration == null) continue;
                        if (!recSet.PublishConfiguration.Equals(DefaultPublishConfigName)) continue;

                        recSet.Recording = true;
                    }
                }

                sym.ApplyHistorizeSettings(historizeSettings);
            }

            mappedSymbols = serverAi.GetMappedSymbols();
            sym = GetMappedSymbol("OfflineSymbolOfPort", mappedSymbols);
            Console.WriteLine("Altered historized symbol:");
            ShowHistorizeData(sym);

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");
            vsHmi.SaveAllFiles();
            Utilities.CloseVisualStudio(vsHmi);
        }

        private static ITcHmiMappedSymbol GetMappedSymbol(string mappedName, ITcHmiMappedSymbol [] symbols)
        {
            if (symbols == null) return null;
            if (string.IsNullOrEmpty(mappedName)) return null;

            foreach(var sym in symbols)
            {
                if (sym == null) continue;
                if (string.IsNullOrEmpty(sym.MappedName)) continue;
                if (sym.MappedName.EndsWith(mappedName, StringComparison.OrdinalIgnoreCase))
                    return sym;
            }

            return null;
        }

        private static void ShowHistorizeData(ITcHmiMappedSymbol sym)
        {
            try
            {
                var historizeSettings = sym?.HistorizeSettings;
                if (historizeSettings == null) return;

                Console.WriteLine("Symbol: {0}", sym.MappedName);
                Console.WriteLine("Interval: {0}", historizeSettings.Interval);
                Console.WriteLine("RowLimit: {0}", historizeSettings.RowLimit);
                Console.WriteLine("MaxEntries: {0}", historizeSettings.MaxEntries);

                var recordingSettings = historizeSettings.RecordingSettings;
                foreach (var recSet in recordingSettings)
                {
                    if (recSet == null) continue;
                    Console.WriteLine("   {0} -> {1}",
                        recSet.PublishConfiguration,
                        recSet.Recording);
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
