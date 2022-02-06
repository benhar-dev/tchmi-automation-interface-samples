namespace Beckhoff.TwinCAT.HMI.Automation
{
    public static class Defaults
    {
        public enum VsVersion
        {
            AutoMode,
            Vs17Professional,
            Vs17Community,
            Vs19Professional,
            Vs19Community,
            Vs19Preview
        }

        public const string VsDevEnv17 = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE";
        public const string VsDevEnv17Community = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE";
        public const string VsDevEnv19 = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE";
        public const string VsDevEnv19Community = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE";
        public const string VsDevEnv19Preview = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Preview\Common7\IDE";

        public const string VsProgId17 = "!VisualStudio.DTE.15.0:";
        public const string VsProgId19 = "!VisualStudio.DTE.16.0:";

        public const string DefaultSolutionDirectory = @"C:\temp\Automation";
        public const string DefaultHmiTemplate = "TwinCAT HMI Project";
        public const string DefaultHmiCategory = "TwinCAT HMI";
    }
}
