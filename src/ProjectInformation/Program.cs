namespace ProjectInformation
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

            var prjProps = hmiPrj?.DteProject?.Properties;
            if (prjProps != null)
            {
                #region query all project properties and display them

                foreach (var it in prjProps)
                {
                    var itProp = it as EnvDTE.Property;
                    if (itProp == null) continue;
                    Console.WriteLine($"  {itProp.Name}: {itProp.Value}");
                }

                #endregion

                var oldHmiTitle = prjProps.Item("HmiTitle").Value;
                prjProps.Item("HmiTitle").Value = string.Format("HMI: {0:F}", DateTime.Now);
                vsHmi.SaveAllFiles();
                var newHmiTitle = prjProps.Item("HmiTitle").Value;

                Console.WriteLine($"OLD Title: {oldHmiTitle}");
                Console.WriteLine($"NEW Title: {newHmiTitle}");

                hmiPrj.Build();
            }

            var enumFields = (ConfigFields[])Enum.GetValues(typeof(ConfigFields));
            foreach (var cfgField in enumFields)
                if (hmiPrj != null)
                    Console.WriteLine($"{cfgField}: {hmiPrj.GetConfigValue(cfgField)}");

            // change default login page
            hmiPrj?.ChangeLoginPage("Ries.html");

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");

            vsHmi.CloseSolution();
            Utilities.CloseVisualStudio(vsHmi);
        }
    }
}
