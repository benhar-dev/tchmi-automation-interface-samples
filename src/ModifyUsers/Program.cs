namespace ModifyUsers
{
    using System;
    using System.Linq;
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

            var permissions = hmiPrj.GetPermissionInterface();
            ShowUsers(permissions);

            var authDomain = permissions.AvailableDomains.First();

            for (int i = 0; i < 10; ++i)
            {
                var userInstance = permissions.GetUserInterface($"User_{i}", authDomain);
                userInstance.Language = "de-DE";
                userInstance.Password = "1";
                permissions.AddUser(userInstance);
            }

            Utilities.ShowError(permissions);

            Utilities.WaitFor(() =>
            {
                permissions.RefreshData();
                return permissions.Users.Length >= 10;
            }, waitMessage: "Wait for users...");

            ShowUsers(permissions);

            for (int i = 0; i < 10; i += 2)
                permissions.RemoveUser($"User_{i}", authDomain);

            Utilities.WaitFor(() =>
            {
                permissions.RefreshData();
                return permissions.Users.Length < 10;
            }, waitMessage: "Wait for users...");

            ShowUsers(permissions);

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");

            vsHmi.CloseSolution();
            Utilities.CloseVisualStudio(vsHmi);
        }

        private static void ShowUsers(ITcHmiPermissions permissions)
        {
            foreach (var user in permissions.Users)
            {
                if (user == null) continue;
                Console.WriteLine($"Username: {user.Name} ({user.Domain})");
            }
        }
    }
}
