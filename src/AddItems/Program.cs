// ReSharper disable UnusedVariable

namespace AddItems
{
    using System;
    using System.Collections.Generic;
    using TcHmiAutomation;
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

            hmiPrj.ToggleSubscriptionMode(false);

            // TODO please provide any file to add with AddExistingItem(..)
            var userControlWithJson = hmiPrj.AddExistingItem(
                "RiesCtrl.usercontrol",
                @"Testfiles\UserControl1.usercontrol",
                new[] {
                    @"Testfiles\UserControl1.usercontrol.json"
                });

            var viewItem = hmiPrj.AddView("View_HelloWorld");
            var contentItem = hmiPrj.AddContent("Content_HelloWorld");

            var folder0 = hmiPrj.AddFolder(@"UserControls\");
            if (folder0 != null)
                Console.WriteLine("Folder: {0} (Parent: {1}, Childs: {2}, Path: {3})",
                    folder0.Name, folder0.Parent?.Name, folder0.ChildCount, folder0.PathName);

            var folder1 = folder0?.AddFolder(@"Ctrl0");
            if (folder1 != null)
                Console.WriteLine("Folder: {0} (Parent: {1}, Childs: {2}, Path: {3})",
                    folder1.Name, folder1.Parent?.Name, folder1.ChildCount, folder1.PathName);

            var folder2 = folder0?.AddFolder(@"Ctrl1");
            if (folder2 != null)
                Console.WriteLine("Folder: {0} (Parent: {1}, Childs: {2}, Path: {3})",
                    folder2.Name, folder2.Parent?.Name, folder2.ChildCount, folder2.PathName);

            var items = new List<ITcHmiItem>
            {
                folder0?.AddItem("View0.view", "View"),
                folder0?.AddItem("Content0.content", "Content"),
                folder0?.AddItem("UserControl0.usercontrol", "UserControl")
            };
            foreach (var it in items)
                Console.WriteLine("Folder: {0} (Parent: {1}, Childs: {2}, Path: {3})",
                    it.Name, it.Parent?.Name, it.ChildCount, it.PathName);

            var existingItem0 = hmiPrj.AddExistingItem("MyCoolView.view", @"C:\tfs\temp\Automation\Testfiles\HelloView.view");
            var existingItem1 = hmiPrj.AddExistingItem("MyWorstView.view", @"C:\tfs\temp\Automation\Testfiles\RiesView.view");
            var existingItem2 = folder2?.AddExistingItem("Blubb.view", @"C:\tfs\temp\Automation\Testfiles\HelloView.view");

            var existingItem3 = hmiPrj.AddExistingItem("MyCoolViewNoExt", @"C:\tfs\temp\Automation\Testfiles\HelloView.view");
            var existingItem4 = hmiPrj.AddExistingItem("MyWorstViewNoExt", @"C:\tfs\temp\Automation\Testfiles\RiesView.view");
            var existingItem5 = folder2?.AddExistingItem("BlubbNoExt", @"C:\tfs\temp\Automation\Testfiles\HelloView.view");

            for (var i = 0; i < 10; ++i)
            {
                var userControlRootId = $"UserControl{i}";
                var userControlItem = hmiPrj.AddUserControl($@"UserControls\{userControlRootId}");
                var ctrl = userControlItem.AddControl(userControlRootId, $"SecondControl{i}", "TcHmi.Controls.Beckhoff.TcHmiButton");

                ctrl.ChangeAttributes(new[]
                {
                    hmiPrj.GetControlAttributeInstance("data-tchmi-left", $"{i*25}"),
                    hmiPrj.GetControlAttributeInstance("data-tchmi-top", $"{i*25}")
                });

                var ids = userControlItem.GetAllIdentifiers();

                Console.WriteLine("Identifiers: {0}", string.Join(", ", ids));
            }

            hmiPrj.ToggleSubscriptionMode(true);

            //var ctrlRootId = $"Ctrl";
            //var ctrlItem = hmiPrj.AddUserControl(ctrlRootId);
            //var ctrlWithAttrs = ctrlItem.AddControl(ctrlRootId, $"AttrCtrl", "TcHmi.Controls.Beckhoff.TcHmiButton", new ITcHmiControlAttribute[]
            //{
            //    hmiPrj.GetControlAttributeInstance("data-tchmi-left", "3"),
            //    hmiPrj.GetControlAttributeInstance("data-tchmi-top", "3")
            //});

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");

            vsHmi.SaveAllFiles();

            Utilities.CloseVisualStudio(vsHmi);
        }
    }
}
