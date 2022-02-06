namespace UserControlParameters
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
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

            var itmUserCtrl =  hmiPrj.AddItem("UserCtrl", HmiTemplates.UserControl);
            var jsonUserCtrl = itmUserCtrl.Childs.First() as ITcHmiFile;
            if(jsonUserCtrl != null)
                Console.WriteLine("Source: {0}", jsonUserCtrl.GetSource());

            if (jsonUserCtrl != null)
            {
                var o = JObject.Parse(jsonUserCtrl.GetSource());
                if (o["parameters"] != null)
                {
                    var ar = o["parameters"] as JArray;

                    ShowAttributes(ar);

                    ar?.Add(new JObject
                    {
                        ["name"] = "data-tchmi-BrandNewAttribute",
                        ["displayName"] = "Just A Test"
                    });

                    jsonUserCtrl.SetSource(o.ToString(Formatting.Indented));

                    Utilities.WaitForEnter("(1) Check source - enter any key...");
                }
            }

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");
            vsHmi.SaveAllFiles();
            Utilities.CloseVisualStudio(vsHmi);
        }

        private static void ShowAttributes(JArray attrArray)
        {
            foreach (var oo in attrArray)
            {
                var itObj = oo as JObject;
                if (itObj == null) continue;
                Console.WriteLine(" {0} -> {1} ", itObj["name"], itObj["displayName"]);
            }
        }
    }
}
