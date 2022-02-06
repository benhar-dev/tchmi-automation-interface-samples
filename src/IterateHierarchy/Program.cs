namespace IterateHierarchy
{
    using System;
    using TcHmiAutomation;
    using Beckhoff.TwinCAT.HMI.Automation;

    internal class Program
    {
        private static string Name => typeof(Program).Assembly.GetName().Name;

        private static int _depth;

        private static void WriteSpaces()
        {
            for(var i = 0; i < _depth; ++i)
                Console.Write("   ");
        }

        private static void ShowInfo(ITcHmiItem itm)
        {
            WriteSpaces();
            Console.WriteLine("{0} (Parent. {1}, Childs: {2}, Path: {3})", 
                itm.Name, itm.Parent?.Name, itm.ChildCount, itm.PathName);
        }

        private static void Main()
        {
            Utilities.Init(Name,
                out var vsHmi,
                out var hmiPrj,
                waitForDebugger: false,
                version: Defaults.VsVersion.AutoMode,
                isExperimental: false);

            ShowInfo(hmiPrj);
            var rootChilds = hmiPrj.Childs;
            ++_depth;
            foreach (var itm in rootChilds)
            {
                if (itm == null) continue;
                ShowInfo(itm);
                IterateChilds(itm.Childs);
            }
            --_depth;

            var rootKeyboardLayouts = hmiPrj.LookupChild("KeyboardLayouts");
            if(rootKeyboardLayouts != null)
            {
                var deKeyboardLayout = rootKeyboardLayouts.LookupChild("German - compact.keyboard.json");
                Console.WriteLine("deKeyboardLayout: {0}", deKeyboardLayout?.Name);
            }

            var usKeyboardLayout0 = hmiPrj.LookupChild("KeyboardLayouts\\US - compact.keyboard.json");
            var usKeyboardLayout1 = hmiPrj.LookupChild("KeyboardLayouts/US - compact.keyboard.json");
            Console.WriteLine("usKeyboardLayout0: {0}", usKeyboardLayout0?.Name);
            Console.WriteLine("usKeyboardLayout1: {0}", usKeyboardLayout1?.Name);
            
            Console.WriteLine("Enter any key to close project...");
            Console.ReadKey();

            Utilities.CloseVisualStudio(vsHmi);
        }

        private static void IterateChilds(ITcHmiItem [] childs)
        {
            ++_depth;

            if (childs == null) return;
            foreach (var itm in childs)
            {
                if (itm == null) continue;
                ShowInfo(itm);
                IterateChilds(itm.Childs);
            }

            --_depth;
        }
    }
}
