using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.ScottyDoesKnow.DsBackup.Helpers
{
    internal static class FileHelper
    {
        // https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            Directory.CreateDirectory(destinationDir);

            foreach (var file in dir.GetFiles())
                file.CopyTo(Path.Combine(destinationDir, file.Name));

            if (!recursive)
                return;

            var subDirs = dir.GetDirectories();
            foreach (var subDir in subDirs)
            {
                var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
