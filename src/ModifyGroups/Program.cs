using System.Linq;
using Beckhoff.TwinCAT.HMI.Automation;
using TcHmiAutomation;

namespace ModifyGroups
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

            var permissions = hmiPrj.GetPermissionInterface();

            ShowGroups(permissions);

            var dummy1 = CreateDummyUser(permissions, "Dummy1");

            permissions.AddUserToGroup(dummy1.Name, dummy1.Domain, "__SystemAdministrators");

            // update data
            permissions.RefreshData();
            ShowGroups(permissions);

            // create new group
            var newGrp0 = permissions.GetGroupInterface();
            newGrp0.Name = "Testgroup A";
            newGrp0.ApplySettings();
            WaitForGroup(newGrp0);
            ShowGroups(permissions);

            // rename group
            newGrp0.Name = "Testgroup B";
            newGrp0.ApplySettings();
            WaitForGroup(newGrp0);
            ShowGroups(permissions);

            // remove group
            var n = permissions.Groups.Length;
            permissions.RemoveGroup(newGrp0);
            Utilities.WaitFor(() => 
            {
                permissions.RefreshData();
                int nn = permissions.Groups.Length;
                return nn == (n - 1);
            }, waitMessage: "Wait for remove...");
            ShowGroups(permissions);

            // finalize the example
            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");
            vsHmi.CloseSolution();
            Utilities.CloseVisualStudio(vsHmi);
        }

        private static void ShowGroups(ITcHmiPermissions permissions)
        {
            var authDomain = permissions.AvailableDomains.First();

            foreach (var grp in permissions.Groups)
            {
                if (grp == null) continue;
                Console.WriteLine($"Group: {grp.Name}");
                var usersOfGroup = grp.GetUsers(authDomain);
                foreach(var usr in usersOfGroup)
                {
                    if(string.IsNullOrEmpty(usr)) continue;
                    Console.WriteLine("   > {0}", usr);
                }
            }
        }

        private static ITcHmiUser CreateDummyUser(ITcHmiPermissions permissions, string name)
        {
            var authDomain = permissions.AvailableDomains.First();
            var userInstance = permissions.GetUserInterface(name, authDomain);
            userInstance.Language = "de-DE";
            userInstance.Password = "1";
            permissions.AddUser(userInstance);

            Utilities.WaitFor2(() =>
            {
                permissions.RefreshData();
                return permissions.Users.Any(x=>x != null && x.Name.Equals(name));
            }
                , waitMessage: $"Wait for user creation on server-side: {name}"
                , walltimeInSecs: 10
                , sleepBetweenCallsInMsecs: 100);

            return userInstance;
        }

        private static void WaitForGroup(ITcHmiGroup grp)
        {
            Utilities.WaitFor(() =>
            {
                grp.Permissions.RefreshData();
                return grp.Permissions.Groups.Any(x => x != null && x.Name.Equals(grp.Name));
            }, waitMessage: $"Wait for group: {grp.Name}");
        }
    }
}
