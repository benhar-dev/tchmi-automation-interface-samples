using System;
using System.IO;
using System.Diagnostics;
using EnvDTE80;
using TcHmiAutomation;

namespace Beckhoff.TwinCAT.HMI.Automation
{
    public static partial class Utilities
    {
        public static string AutomationId = "Beckhoff.TcHmi.1.12";

        public static bool IsDebuggerAttached => Debugger.IsAttached;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathToSolution"></param>
        /// <param name="projectName"></param>
        /// <param name="vsHmi"></param>
        /// <param name="hmiPrj"></param>
        /// <param name="maxSecondsToWait"></param>
        /// <param name="version"></param>
        /// <param name="isExperimental"></param>
        /// <returns></returns>
        public static bool LoadSolution(
            string pathToSolution,
            string projectName,
            out ITcHmiAutomation vsHmi,
            out ITcHmiProject hmiPrj,
            int maxSecondsToWait = 120,
            Defaults.VsVersion version = Defaults.VsVersion.AutoMode,
            bool isExperimental = false
            )
        {
            vsHmi = null;
            hmiPrj = null;
            var dte = StartVisualStudio(version, isExperimental);
            if (dte == null) return false;
            dte.Solution.Open(pathToSolution);
            vsHmi = dte.GetObject(AutomationId) as ITcHmiAutomation;
            if (vsHmi == null) return false;
            var internalHmiPrj = vsHmi.GetHmiProject(projectName);
            WaitFor(() => internalHmiPrj.IsReady(), maxSecondsToWait, "Wait for HMI project...");
            hmiPrj = internalHmiPrj;
            return true;
        }

        public static EnvDTE.Project CreateProject(
            ITcHmiAutomation vsHmi,
            string projectTemplate,
            string projectName,
            string targetDirectory = Defaults.DefaultSolutionDirectory
        )
        {
            return vsHmi.CreateProject(projectTemplate, targetDirectory, projectName);
        }

        /// <summary>
        /// Creates and initializes a HMI project.
        /// </summary>
        /// <param name="vsHmi"></param>
        /// <param name="projectName"></param>
        /// <param name="newHmiPrj"></param>
        /// <param name="maxSecondsToWait"></param>
        /// <param name="targetDirectory"></param>
        public static void InitProject(
            ITcHmiAutomation vsHmi,
            string projectName,
            out ITcHmiProject newHmiPrj,
            int maxSecondsToWait = 120,
            string targetDirectory = Defaults.DefaultSolutionDirectory
            )
        {
            if (vsHmi?.DteProject == null)
            {
                newHmiPrj = vsHmi?.CreateHmiProject(Defaults.DefaultHmiTemplate, targetDirectory, projectName);
            }
            else
            {
                var p = vsHmi.CreateProject(Defaults.DefaultHmiTemplate, targetDirectory, projectName);
                newHmiPrj = vsHmi.GetHmiProject(p);
            }

            if (newHmiPrj != null)
            {
                for (var i = 0; i < 2 * maxSecondsToWait; ++i)
                {
                    try
                    {
                        var isReady = newHmiPrj.IsReady();
                        Console.WriteLine("HMI project is{0}ready", isReady ? " " : " NOT ");
                        if (isReady) return;
                        System.Threading.Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Execution canceled: {0}", ex.Message);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Starts a VisualStudio instance and creates a HMI project.
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="vsHmi"></param>
        /// <param name="hmiPrj"></param>
        /// <param name="maxSecondsToWait"></param>
        /// <param name="version"></param>
        /// <param name="isExperimental"></param>
        /// <param name="hmiTemplate"></param>
        /// <param name="waitForHmiReady"></param>
        /// <param name="waitForDebugger"></param>
        /// <param name="targetDirectory"></param>
        /// <returns></returns>
        public static DTE2 Init(
            string projectName,
            out ITcHmiAutomation vsHmi,
            out ITcHmiProject hmiPrj,
            int maxSecondsToWait = 120,
            Defaults.VsVersion version = Defaults.VsVersion.AutoMode,
            bool isExperimental = true,
            string hmiTemplate = Defaults.DefaultHmiTemplate,
            bool waitForHmiReady = true,
            bool waitForDebugger = true,
            string targetDirectory = Defaults.DefaultSolutionDirectory)
        {
            var dte = StartVisualStudio(version, isExperimental);

            if (dte?.Globals != null)
                dte.Globals["InAutomationMode"] = true;

            vsHmi = GetAutomationInterface(dte, AutomationId);

            if (vsHmi?.DteProject == null)
            {
                hmiPrj = vsHmi?.CreateHmiProject(hmiTemplate, targetDirectory, projectName);
            }
            else
            {
                var p = vsHmi.CreateProject(hmiTemplate, targetDirectory, projectName);
                hmiPrj = vsHmi?.GetHmiProject(p);
            }

            if (hmiPrj != null && waitForHmiReady)
            {
                for (var i = 0; i < 2 * maxSecondsToWait; ++i)
                {
                    try
                    {
                        var isReady = hmiPrj.IsReady();
                        Console.WriteLine("HMI project is{0}ready", isReady ? " " : " NOT ");
                        if (isReady)
                        {
                            if (waitForDebugger)
                            {
                                if (!IsDebuggerAttached)
                                    WaitForDebugger();
                            }

                            return dte;
                        }
                        System.Threading.Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Execution canceled: {0}", ex.Message);
                        return dte;
                    }
                }
            }

            if (waitForDebugger)
            {
                if (!IsDebuggerAttached)
                    WaitForDebugger();
            }

            return dte;
        }

        public static DTE2 StartVisualStudio(
            Defaults.VsVersion version = Defaults.VsVersion.AutoMode,
            bool isExperimental = true)
        {
            switch (version)
            {
                case Defaults.VsVersion.AutoMode:
                    {
                        if (Directory.Exists(Defaults.VsDevEnv19Preview))
                            return StartVisualStudio19Preview(isExperimental);
                        if (Directory.Exists(Defaults.VsDevEnv17))
                            return StartVisualStudio17(isExperimental);
                        if (Directory.Exists(Defaults.VsDevEnv17Community))
                            return StartVisualStudio17Community(isExperimental);
                        if (Directory.Exists(Defaults.VsDevEnv19))
                            return StartVisualStudio19(isExperimental);
                        if (Directory.Exists(Defaults.VsDevEnv19Community))
                            return StartVisualStudio19Community(isExperimental);
                    }
                    break;

                case Defaults.VsVersion.Vs17Community: return StartVisualStudio17Community(isExperimental);
                case Defaults.VsVersion.Vs17Professional: return StartVisualStudio17(isExperimental);
                case Defaults.VsVersion.Vs19Community: return StartVisualStudio19Community(isExperimental);
                case Defaults.VsVersion.Vs19Professional: return StartVisualStudio19(isExperimental);
                case Defaults.VsVersion.Vs19Preview: return StartVisualStudio19Preview(isExperimental);
            }

            return null;
        }

        private static DTE2 StartVisualStudio(string devenvDir, string progIdPrefix, string internalIdentifier, bool isExperimental = true)
        {
            var ideCtrl = new VsIdeControl
            {
                IsExperimental = isExperimental
            };
            var ideInstance = ideCtrl.Run(60, devenvDir, progIdPrefix);
            if (!ideInstance) Console.WriteLine($"{internalIdentifier} start failed");
            return ideCtrl.Instance;
        }

        public static DTE2 StartVisualStudio17(bool isExperimental = true)
        {
            return StartVisualStudio(Defaults.VsDevEnv17, Defaults.VsProgId17, "VisualStudio 2017", isExperimental);
        }

        public static DTE2 StartVisualStudio17Community(bool isExperimental = true)
        {
            return StartVisualStudio(Defaults.VsDevEnv17Community, Defaults.VsProgId17, "VisualStudio 2017", isExperimental);
        }

        public static DTE2 StartVisualStudio19(bool isExperimental = true)
        {
            return StartVisualStudio(Defaults.VsDevEnv19, Defaults.VsProgId19, "VisualStudio 2019", isExperimental);
        }

        public static DTE2 StartVisualStudio19Community(bool isExperimental = true)
        {
            return StartVisualStudio(Defaults.VsDevEnv19Community, Defaults.VsProgId19, "VisualStudio 2019", isExperimental);
        }

        public static DTE2 StartVisualStudio19Preview(bool isExperimental = true)
        {
            return StartVisualStudio(Defaults.VsDevEnv19Preview, Defaults.VsProgId19, "VisualStudio 2019 Preview", isExperimental);
        }

        public static ITcHmiAutomation GetAutomationInterface(DTE2 dte, string projectIdentifier, int walltimeInSeconds = 5)
        {
            if (dte == null) return null;

            for (var i = 0; i < (walltimeInSeconds * 2); ++i)
            {
                try
                {
                    var obj = dte.GetObject(projectIdentifier);
                    if (obj is ITcHmiAutomation vsHmi) return vsHmi;

                    var vsHmiObj = obj as EnvDTE.Project;
                    vsHmi = vsHmiObj?.Object as ITcHmiAutomation;
                    return vsHmi;
                }
                catch
                {
                    //Console.WriteLine("<Exception> {0}", ex.Message);
                    Console.Write(".");
                    System.Threading.Thread.Sleep(500);
                }
            }

            Console.Write("x");

            return null;
        }

        public static void WaitForEnter(string msg = null)
        {
            if (string.IsNullOrEmpty(msg)) Console.WriteLine("Enter any key...");
            else Console.WriteLine(msg);
            Console.ReadKey();
        }

        public static void ShowError(object obj)
        {
            var oo = obj as ITcHmiError;
            if (oo == null) return;

            try
            {
                var v = oo.LastError;
                if (!string.IsNullOrEmpty(v))
                    Console.WriteLine("<Error> {0}", v);
            }
            catch
            {
                // ignore
            }
        }

        public static bool WaitFor(Func<bool> act, int seconds = 10, string waitMessage = null)
        {
            if (act == null) return false;

            for (var i = 0; i < seconds; ++i)
            {
                if (!string.IsNullOrEmpty(waitMessage))
                    Console.WriteLine(waitMessage);
                var res = act.Invoke();
                if (res) return true;

                System.Threading.Thread.Sleep(1000);
            }

            return act.Invoke();
        }

        public static bool WaitFor2(Func<bool> act, int walltimeInSecs = 10, int sleepBetweenCallsInMsecs = 100, string waitMessage = null)
        {
            if (act == null) return false;

            long internalWalltime = walltimeInSecs * 1000 * sleepBetweenCallsInMsecs;

            var sw = new Stopwatch();
            sw.Reset();
            sw.Start();

            for (long i = 0; i < internalWalltime; i += sleepBetweenCallsInMsecs)
            {
                if (!string.IsNullOrEmpty(waitMessage))
                {
                    var swValue = sw.Elapsed;

                    Console.WriteLine("{0}: {1}", swValue, waitMessage);
                }
                var res = act.Invoke();
                if (res) return true;

                System.Threading.Thread.Sleep(sleepBetweenCallsInMsecs);
            }

            return act.Invoke();
        }

        public static ITcHmiServerExtension WaitForDomain(
            ITcHmiServer srv,
            string domainName,
            string waitMessage,
            int walltimeInSecs = 10,
            bool includeNotPopulated = false)
        {
            ITcHmiServerExtension srvExt = null;

            WaitFor(() =>
            {
                var domains = srv.GetServerExtensions(includeNotPopulated);
                foreach (var domain in domains)
                {
                    if (domain == null) continue;
                    var name = domain.DomainName;
                    if (string.IsNullOrEmpty(name)) continue;
                    if (name.Equals(domainName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!includeNotPopulated && !domain.Loaded)
                            return false;

                        srvExt = domain;
                        return true;
                    }
                }
                return false;
            }, seconds: walltimeInSecs,
                waitMessage: waitMessage);

            return srvExt;
        }

        public static void WaitForDebugger()
        {
            WaitForEnter("Attach debugger and enter '<return>'...");
            Console.WriteLine("THANKS");
        }

        public static ITcHmiNuGetSource GetSpecificNuGetSource(ITcHmiNuGetSource[] availableSources, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            foreach (var it in availableSources)
            {
                if (it == null) continue;
                var localName = it.Name;
                //var source = it.Source;
                if (string.IsNullOrEmpty(name)) continue;
                if (localName.Equals(name))
                    return it;
            }

            return null;
        }

        /// <summary>
        /// Closes the current VisualStudio instance.
        /// All documents are saved automatically.
        /// </summary>
        public static void CloseVisualStudio(ITcHmiAutomation vsHmiAutomation)
        {
            if (vsHmiAutomation == null) return;

            var procs = Process.GetProcessesByName("devenv");
            if (procs.Length == 0) return;

            EnvDTE.Project dteProject = null;

            try
            {
                dteProject = vsHmiAutomation.DteProject;
            }
            catch
            {
                // ignore
            }

            try
            {
                dteProject?.DTE?.Documents?.SaveAll();
            }
            catch
            {
                // ignore
            }

            try
            {
                dteProject?.Save(string.Empty);
            }
            catch
            {
                // ignore
            }

            try
            {
                dteProject?.DTE?.Solution?.Close(true);
            }
            catch
            {
                // ignore
            }

            WaitFor(() =>
            {
                for (var i = 0; i < 30; ++i)
                {
                    var sln = dteProject?.DTE?.Solution;
                    if (sln == null) return true;

                    try
                    {
                        if (!sln.IsOpen) return true;
                    }
                    catch
                    {
                        // ignore
                    }
                }

                return false;
            }, 30);

            var vsProcessId = vsHmiAutomation.GetProcessId();

            VsClose(vsProcessId);

            CloseVisualStudioFriendly(vsProcessId);
        }
        
        public static bool IsTestEnvironment()
        {
            if (IsDebuggerAttached) return false;

            var machineName = Environment.MachineName;
            if (!string.IsNullOrEmpty(machineName)
                && machineName.IndexOf("DEVOPSTEST", StringComparison.OrdinalIgnoreCase) > -1)
                return true;

            return false;
        }

        public static void KillAllServer(string filter = "creator")
        {
            if (!IsTestEnvironment()) return;

            var processNamesToKill = new[]
            {
                "tchmisrv",
                "tchmisrv.exe"
            };

            Kill(processNamesToKill, filter: filter);
        }

        public static void KillAllVisualStudios()
        {
            if (!IsTestEnvironment()) return;

            var processNamesToKill = new[]
            {
                "devenv",
                "devenv.exe"
            };

            Kill(processNamesToKill);
        }

        public static void KillAllCefInstances()
        {
            if (!IsTestEnvironment()) return;

            var processNamesToKill = new[]
            {
                "CefSharp.BrowserSubprocess",
                "CefSharp.BrowserSubprocess.exe"
            };

            Kill(processNamesToKill);
        }

        public static void Kill(string[] processNames, int repeats = 5, string filter = null)
        {
            for (var i = 0; i < repeats; ++i)
            {
                try
                {
                    foreach (var name in processNames)
                    {
                        try
                        {
                            var procs = Process.GetProcessesByName(name);
                            if (procs == null || procs.Length == 0) continue;
                            foreach (var p in procs)
                            {
                                var pinfo = p.StartInfo;

                                if (pinfo != null && !string.IsNullOrEmpty(filter))
                                {
                                    var args = pinfo.Arguments;
                                    if (args.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1)
                                        p?.Kill();
                                }
                                else
                                {
                                    p?.Kill();
                                }
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }

            System.Threading.Thread.Sleep(500);
        }
    }
}