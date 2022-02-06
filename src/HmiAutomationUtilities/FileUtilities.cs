namespace Beckhoff.TwinCAT.HMI.Automation
{
    using System.IO;

    public static class FileUtilities
    {
        public static void DeleteDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!Directory.Exists(path)) return;
            try
            {
                Directory.Delete(path, true);
            }
            catch
            {
                // ignore
            }
        }

        public static void DeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!File.Exists(path)) return;
            try
            {
                File.Delete(path);
            }
            catch
            {
                // ignore
            }
        }

        public static void Delete(string path)
        {
            DeleteDirectory(path);
            DeleteFile(path);
        }

        public static void CreateDirectory(string path = Defaults.DefaultSolutionDirectory)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch
            {
                // ignore
            }
        }

        public static void FindAndDeleteAll(string rootdir, string pattern)
        {
            if (string.IsNullOrEmpty(rootdir)) return;
            if (string.IsNullOrEmpty(pattern)) return;

            var dirEntries = Directory.GetDirectories(rootdir, pattern, SearchOption.AllDirectories);
            foreach (var dname in dirEntries)
                DeleteDirectory(dname);

            var fileEntries = Directory.GetFiles(rootdir, pattern, SearchOption.AllDirectories);
            foreach (var fname in fileEntries)
                DeleteFile(fname);
        }
    }
}
