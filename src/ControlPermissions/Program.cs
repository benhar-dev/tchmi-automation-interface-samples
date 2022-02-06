namespace ControlPermissions
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

            var desktopView = hmiPrj.LookupChild("Desktop.view") as ITcHmiFile;
            var btn = desktopView?.AddControl("Desktop", "BtnPermissions", "TcHmi.Controls.Beckhoff.TcHmiButton");

            btn?.SetPermission(AccessRight.Observe, "Testgroup A", Permission.Allow);
            btn?.SetPermission(AccessRight.Operate, "Testgroup B", Permission.Inherit);
            var currentPermissions = btn?.GetPermissions();
            ShowPermissions(currentPermissions);
            Utilities.WaitForEnter("(1) Check source - enter any key...");

            btn?.SetPermission(AccessRight.Observe, "Testgroup A", Permission.Inherit);
            btn?.RemovePermission(AccessRight.Operate, "Testgroup B");
            currentPermissions = btn?.GetPermissions();
            ShowPermissions(currentPermissions);
            Utilities.WaitForEnter("(2) Check source - enter any key...");

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");
            vsHmi.CloseSolution();
            Utilities.CloseVisualStudio(vsHmi);
        }

        private static void ShowPermissions(ITcHmiControlAccessRight [] permissions)
        {
            if(permissions == null) return;
            if(permissions.Length == 0) return;

            Console.WriteLine(" # Permissions");
            Console.WriteLine("-----------------------------------");
            foreach(var it in permissions)
            {
                if(it == null) continue;
                Console.WriteLine(" > {0}, {1}, {2}", it.Access, it.GroupName, it.GroupPermission);
            }
            Console.WriteLine("-----------------------------------");
        }
    }
}
