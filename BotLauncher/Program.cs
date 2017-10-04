using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace BotLauncher
{
    [Obfuscation(Feature = "Apply to member * when method or constructor or event: virtualization", Exclude = false)]
    internal class Program
    {
        private static readonly string[] _files =
        {
            "Internal\\ZzukBot.exe",
            "Internal\\Fasm.NET.dll",
            "Internal\\Loader.dll"
        };

        private static bool IsLocked(string parPathAndFile)
        {
            try
            {
                if (File.Exists(parPathAndFile))
                    using (Stream stream = new FileStream(parPathAndFile, FileMode.Open))
                    {
                    }
                return false;
            }
            catch
            {
                Console.WriteLine("File \"" + parPathAndFile + "\" is stil in use.");
                return true;
            }
        }

        [DllImport("kernel32")]
        private static extern bool AllocConsole();

        private static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "UPDATE")
            {
                AllocConsole();
                Update();
            }
            else
            {
                var start = new ProcessStartInfo
                {
                    FileName = "ZzukBot.exe",
                    WorkingDirectory = "Internal\\"
                };
                Process.Start(start);
                Environment.Exit(0);
            }
        }

        private static void Update()
        {
            var canUpdate = false;
            while (!canUpdate)
            {
                Console.Clear();
                canUpdate = true;
                Console.WriteLine(
                    "## ZzukBot's Updater ##\n\nChecking if we can start the update (please leave this window open):");
                foreach (var file in _files)
                    if (IsLocked(file))
                        canUpdate = false;
                if (!canUpdate)
                    Console.WriteLine("Rechecking in a second ...");
                Thread.Sleep(1000);
            }

            Console.WriteLine("Ready to update ...");
            // Set the filename for the zip archive we will download
            var zipTempFilePath = Path.GetTempFileName();
            try
            {
                // Downloading the archive
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile("http://zzukbot.com/Downloads/ZzukBot3.zip", zipTempFilePath);
                }
            }
            catch
            {
                Console.Write("Downloading from URL caused a crash. Aborting ");
                Console.ReadLine();
                Environment.Exit(-1);
            }

            Console.WriteLine("Download completed ...");
            // Get the path the zip file will be extracted to
            var updateFolder = Path.GetTempPath() + "\\ZzukBot";
            // Check if the directory exists and delete it if it does
            if (Directory.Exists(updateFolder))
                Directory.Delete(updateFolder, true);
            // Extract the archive
            ZipFile.ExtractToDirectory(zipTempFilePath, updateFolder);
            // Delete the zip after extracting
            File.Delete(zipTempFilePath);

            // Copying the directory structure of the zip file to the local folder
            foreach (var dirPath in Directory.GetDirectories(updateFolder, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(updateFolder, ".\\"));

            // Copying the files of the zip archive to the local folder
            foreach (var newPath in Directory.GetFiles(updateFolder, "*.*",
                SearchOption.AllDirectories))
            {
                if (newPath.Contains("BotLauncher.exe")) continue;
                var newFile = newPath.Replace(updateFolder, ".\\");
                File.Copy(newPath, newFile, true);
                File.SetLastWriteTime(newFile, DateTime.Now);
            }

            // Delete the extracted zip archive
            Directory.Delete(updateFolder, true);
            Console.Write("Update completed! Press any key to exit");
            Console.ReadLine();
        }
    }
}