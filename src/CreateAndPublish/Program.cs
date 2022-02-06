namespace CreateAndPublish
{
    using System;
    using System.Threading;
    using Beckhoff.TwinCAT.HMI.Automation;
    using TcHmiAutomation;
    using TcHmiAutomation.Publish;

    internal class Program
    {
        private static string Name => typeof(Program).Assembly.GetName().Name;

        private static string RemoteAddress = "https://127.0.0.1:1010/";
        private static string RemoteUsername = "__SystemAdministrator";
        private static string RemotePassword = "1";

        private static void Main()
        {
            Utilities.Init(Name,
                out var vsHmi,
                out var hmiPrj,
                waitForDebugger: false,
                version: Defaults.VsVersion.AutoMode,
                isExperimental: false);

            var profileName = "PublishToVirtualMachine #1";
            var profileToUse = hmiPrj?.GetProfileInstance(profileName);
            if (profileToUse != null)
            {
                profileToUse.TargetUrl = RemoteAddress;
                // if TargetUrl is 'https://' set true (default), otherwise set false
                profileToUse.UseTls = RemoteAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase); 
                profileToUse.ServerUsername = RemoteUsername;
                // profileToUse.ServerPassword = "BhlrUxjYaZqWlus4S9SyTw==";
                profileToUse.ServerPassword = TcHmiAutomationUtilities.GetPublishPasswordHash(RemotePassword);
                profileToUse.LaunchBrowserAfterPublish = true;

                var isValidProfile = hmiPrj.IsValidPublishProfile(profileToUse);
                if (isValidProfile)
                {
                    var waitForFinish = new ManualResetEvent(false);

                    hmiPrj.Publish(profileToUse, true, new PublishCallback(waitForFinish));
                    
                    var res = waitForFinish.WaitOne(3 * 60 * 1000 /* three minutes */);
                    Console.WriteLine("Timeout: {0}", !res);
                }
                else
                {
                    Console.WriteLine("Publish profile: {0}", hmiPrj.LastError);
                }
            }

            Console.WriteLine("Enter any key...");
            Console.ReadLine();
        }

        private class PublishCallback : ITcHmiPublishCallback
        {
            private readonly ManualResetEvent _waitForFinish;

            public PublishCallback(ManualResetEvent ev)
            {
                _waitForFinish = ev;
            }

            public void Progress(ITcHmiPublishProgress data)
            {
                if (data == null) return;

                Console.WriteLine(">> {0}", GetLevel(data.ImportanceLevel));
                Console.WriteLine(">> {0}", GetType(data.MessageType));
                Console.WriteLine(">> {0}", data.Message);
            }

            public void Progress(string data)
            {
                Console.WriteLine($"{data.Replace("\n", string.Empty).Replace("\r", string.Empty)}");
            }

            public void Finished(ITcHmiPublishResult data)
            {
                if (data == null) return;
                
                Console.WriteLine(" (x) Result: {0}", GetResult(data.Result));
                Console.WriteLine(" (x) Completed: {0}", data.IsCompleted);
                Console.WriteLine(" (x) SubmissionID: {0}", data.SubmissionId);

                _waitForFinish?.Set();
            }

            #region Helper

            private string GetLevel(int level)
            {
                switch (level)
                {
                    case 0: return "High";
                    case 1: return "Low";
                    case 2: return "Normal";
                }
                return "None";
            }

            private string GetType(int type)
            {
                switch (type)
                {
                    case 0: return "Message";
                    case 1: return "Warning";
                    case 2: return "Error";
                }
                return "None";
            }

            private string GetResult(int result)
            {
                switch (result)
                {
                    case 0: return "Suspended";
                    case 1: return "Resumed";
                    case 2: return "Failed";
                    case 3: return "Successful";
                }

                return "None";
            }

            #endregion
        }
    }
}
 