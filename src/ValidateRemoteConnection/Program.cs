namespace ValidateRemoteConnection
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    internal class Program
    {
        #region Remote Server Configuration

        private const string RemoteHostname = "127.0.0.1";
        private const int RemotePort = 1020;
        private const string Username = "__SystemAdministrator";
        private const string Password = "1";

        #endregion

        private static void Main()
        {
            var info = new AuthInfo();

            info.Validate(RemoteHostname, RemotePort, Username, Password, authInfo =>
            {
                Console.WriteLine("Result: {0}", authInfo.Result);
                Console.WriteLine("ErrorValue: {0}", authInfo.ErrorValue);
                Console.WriteLine("ErrorMessage: {0}", authInfo.ErrorMessage.Length > 0 ? authInfo.ErrorMessage : "-");
            });

            Console.WriteLine("Wait for result...");
            Console.ReadKey();
        }
    }

    public class AuthInfo
    {
        // TODO FOR CUSTOMER: environment settings should be set from outsite if differ on runtime system

        #region Environment Settings

        private const string AppPath = @"C:\TwinCAT\Functions\TE2000-HMI-Engineering\MSBuild";
        private const string AppName = "TcHmiAutomationUtility.exe";

        #endregion

        private Action<AuthInfo> _callback;

        public async void Validate(string remoteHostname, int remotePort, string username, string password, Action<AuthInfo> callback)
        {
            _callback = callback;

            await RunProcessAsync(this, remoteHostname, remotePort, username, password);
        }

        private static Task<int> RunProcessAsync(AuthInfo ctx, string remoteHostname, int remotePort, string username, string password)
        {
            var tcs = new TaskCompletionSource<int>();

            var fullAppPath = Path.Combine(AppPath, AppName);

            var p = new Process
            {
                StartInfo =
                {
                    FileName = fullAppPath,
                    Arguments = $"{remoteHostname} {remotePort} {username} {password}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            p.Exited += (sender, args) =>
            {
                tcs.SetResult(p.ExitCode);

                var allLines = p.StandardOutput.ReadToEnd();
                ctx.Parse(allLines);

                p.Dispose();

                ctx._callback?.Invoke(ctx);
            };

            p.Start();

            return tcs.Task;
        }

        public bool Result { get; private set; }
        public string ErrorValue { get; private set; } = string.Empty;
        public string ErrorMessage { get; private set; } = string.Empty;

        public void Parse(string plainData)
        {
            if (string.IsNullOrEmpty(plainData)) return;
            try
            {
                var obj = JObject.Parse(plainData);

                if(obj["result"] != null)
                {
                    if (bool.TryParse(obj["result"].ToString(), out var v))
                        Result = v;
                }

                if(obj["errorValue"] != null) 
                    ErrorValue = obj["errorValue"].ToString();

                if(obj["errorMessage"] != null)
                    ErrorMessage = obj["errorMessage"].ToString();
            }
            catch
            {
                // ignore
            }
        }
    }
}
