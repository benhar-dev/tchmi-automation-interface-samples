// ReSharper disable UnusedMember.Global
namespace Beckhoff.TwinCAT.HMI.Automation
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using EnvDTE80;

    public class VsIdeControl
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        private System.Diagnostics.Process _devenv;
        internal bool IsExperimental { get; set; }
        internal DTE2 Instance;
        internal int ProcessId => _devenv?.Id ?? -1;

        internal DTE2 GetDte(int timeoutMax, int processId)
        {
            DTE2 dte;
            var timeoutPerIter = 1000;
            var timeoutTotal = 0;
            do
            {
                System.Threading.Thread.Sleep(timeoutPerIter);
                dte = GetDte(processId, null);
                timeoutTotal += timeoutPerIter;
            }
            while (dte == null && timeoutTotal < (timeoutMax * timeoutPerIter));

            return null;
        }

        internal bool Run(int timeoutMax, string devenvDir, string progIdPrefix = "!VisualStudio.DTE.15.0:")
        {
            var devenvPath = Path.Combine(devenvDir, "devenv.exe");
            if(IsExperimental)
                _devenv = System.Diagnostics.Process.Start(devenvPath, "/rootsuffix exp");
            else
                _devenv = System.Diagnostics.Process.Start(devenvPath, string.Empty);
            DTE2 dte = null;
            var timeoutPerIter = 1000;
            var timeoutTotal = 0;
            do
            {
                System.Threading.Thread.Sleep(timeoutPerIter);
                if (_devenv != null) 
                    dte = GetDte(_devenv.Id, progIdPrefix);
                timeoutTotal += timeoutPerIter;
            }
            while (dte == null && timeoutTotal < (timeoutMax * timeoutPerIter));

            MessageFilter.Register();
            
            //dte?.MainWindow.Activate();

            Instance = dte;

            return dte != null;
        }

        internal void Exit()
        {
            if (Instance != null)
            {
                System.Threading.Thread.Sleep(2000);
                Instance.ExecuteCommand("File.Exit");
                _devenv.WaitForExit(10000);
                Marshal.ReleaseComObject(Instance);
            }
        }

        internal static DTE2 GetDte(int processId, string progIdPrefix = "!VisualStudio.DTE.15.0:")
        {
            string progId = null;
            if (!string.IsNullOrEmpty(progIdPrefix))
                progId = progIdPrefix + processId.ToString();

            object runningObject = null;

            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMonikers);

                var moniker = new IMoniker[1];
                var numberFetched = IntPtr.Zero;
                while (enumMonikers.Next(1, moniker, numberFetched) == 0)
                {
                    var runningObjectMoniker = moniker[0];

                    string name = null;

                    try
                    {
                        runningObjectMoniker?.GetDisplayName(bindCtx, null, out name);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Do nothing, there is something in the ROT that we do not have access to.
                    }

                    if (!string.IsNullOrEmpty(progId))
                    {
                        if (!string.IsNullOrEmpty(name) && string.Equals(name, progId, StringComparison.Ordinal))
                        {
                            Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, out runningObject));
                            break;
                        }
                    }
                    else
                    {
                        //Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, out runningObject));
                        //var localDte = runningObject as DTE2;
                        //if (localDte != null)
                        //{
                        //	if (localDte.)
                        //}
                    }
                }
            }
            finally
            {
                if (enumMonikers != null)
                {
                    Marshal.ReleaseComObject(enumMonikers);
                }

                if (rot != null)
                {
                    Marshal.ReleaseComObject(rot);
                }

                if (bindCtx != null)
                {
                    Marshal.ReleaseComObject(bindCtx);
                }
            }

            var dte = runningObject as DTE2;

            return dte;
        }
    }

    public class MessageFilter : IOleMessageFilter
    {
        //
        // Class containing the IOleMessageFilter
        // thread error-handling functions.

        // Start the filter.
        public static void Register()
        {
            IOleMessageFilter newFilter = new MessageFilter();
            CoRegisterMessageFilter(newFilter, out _);
        }

        // Done with the filter, close it.
        public static void Revoke()
        {
            CoRegisterMessageFilter(null, out _);
        }

        //
        // IOleMessageFilter functions.
        // Handle incoming thread requests.
        int IOleMessageFilter.HandleInComingCall(int dwCallType,
          IntPtr hTaskCaller, int dwTickCount, IntPtr
          lpInterfaceInfo)
        {
            //Return the flag SERVERCALL_ISHANDLED.
            return 0;
        }

        // Thread call was rejected, so try again.
        int IOleMessageFilter.RetryRejectedCall(IntPtr
          hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == 2)
            // flag = SERVERCALL_RETRYLATER.
            {
                // Retry the thread call immediately if return >=0 & 
                // <100.
                return 99;
            }
            // Too busy; cancel call.
            return -1;
        }

        int IOleMessageFilter.MessagePending(IntPtr hTaskCallee,
          int dwTickCount, int dwPendingType)
        {
            //Return the flag PENDINGMSG_WAITDEFPROCESS.
            return 2;
        }

        // Implement the IOleMessageFilter interface.
        [DllImport("Ole32.dll")]
        private static extern int
          CoRegisterMessageFilter(IOleMessageFilter newFilter, out
          IOleMessageFilter oldFilter);
    }

    [ComImport, Guid("00000016-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOleMessageFilter
    {
        [PreserveSig]
        int HandleInComingCall(
            int dwCallType,
            IntPtr hTaskCaller,
            int dwTickCount,
            IntPtr lpInterfaceInfo);

        [PreserveSig]
        int RetryRejectedCall(
            IntPtr hTaskCallee,
            int dwTickCount,
            int dwRejectType);

        [PreserveSig]
        int MessagePending(
            IntPtr hTaskCallee,
            int dwTickCount,
            int dwPendingType);
    }
}
