using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Beckhoff.TwinCAT.HMI.Automation
{
    public static partial class Utilities
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        private static void VsClose(int processId, int msecTimeout = 5 * 1000)
        {
            try
            {
                if (processId == -1) return;

                var p = Process.GetProcessById(processId);
                p.CloseMainWindow();
                p.WaitForExit(msecTimeout);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Get process failed: {ex.Message}");
            }
        }

        private static void CloseVisualStudioFriendly(int processId, int msecTimeout = 5 * 1000)
        {
            if (processId == -1) return;

            var procs = Process.GetProcessesByName("devenv");
            if (procs.Length == 0) return;

            foreach (var p in procs)
            {
                if (p == null) continue;

                try
                {
                    if (p.HasExited) continue;
                    if (p.Id != processId) continue;
                }
                catch
                {
                    // ignore
                }

                try
                {
                    p.CloseMainWindow();
                }
                catch
                {
                    // ignore
                }

                try
                {
                    p.WaitForExit(msecTimeout);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}
