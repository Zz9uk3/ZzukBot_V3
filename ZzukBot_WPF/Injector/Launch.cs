using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using ZzukBot.Constants;
using ZzukBot.ExtensionMethods;
using ZzukBot.Helpers.GreyMagic;
using ZzukBot.Settings;

namespace ZzukBot.Injector
{
    internal static class Launch
    {
        internal static void Run()
        {
            try
            {
                $"Loading path of WoW.exe from the settings".Log(LogFiles.PreInjectLog);
                Options.Values.Load();
                $"Starting up the WoW process".Log(LogFiles.PreInjectLog);
                var si = new WinImports.STARTUPINFO();
                WinImports.PROCESS_INFORMATION pi;
                WinImports.CreateProcess(Options.Values.PathToWoW, null,
                    IntPtr.Zero, IntPtr.Zero, false,
                    WinImports.ProcessCreationFlags.CREATE_DEFAULT_ERROR_MODE,
                    IntPtr.Zero, null, ref si, out pi);

                var proc = Process.GetProcessById((int) pi.dwProcessId);
                if (proc.Id == 0)
                {
                    MessageBox.Show(
                        "Couldnt get the WoW process. Is the path in Settings.xml right? If no delete it and rerun ZzukBot");
                    return;
                }
                $"Waiting for WoW process to initialise".Log(LogFiles.PreInjectLog);

                while (!proc.WaitForInputIdle(1000))
                {
                    $"WaitForInputIdle returned false. Trying again".Log(LogFiles.PreInjectLog);
                    proc.Refresh();
                }
                while (string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                {
                    Thread.Sleep(200);
                    proc.Refresh();
                }
                Thread.Sleep(2000);
                $"Initialising new ProcessReader".Log(LogFiles.PreInjectLog);
                using (var reader = new ExternalProcessReader(proc))
                {
                    $"Retrieving function addresses for injection".Log(LogFiles.PreInjectLog);
                    var loadDllPtr = WinImports.GetProcAddress(WinImports.GetModuleHandle("kernel32.dll"),
                        "LoadLibraryW");
                    if (loadDllPtr == IntPtr.Zero)
                    {
                        MessageBox.Show("Couldnt get address of LoadLibraryW");
                        return;
                    }

                    $"Allocating memory for injection".Log(LogFiles.PreInjectLog);
                    var LoaderStrPtr = reader.AllocateMemory(500);
                    if (LoaderStrPtr == IntPtr.Zero)
                    {
                        MessageBox.Show("Couldnt allocate memory 2");
                        return;
                    }

                    var LoaderStr =
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                        + "\\Loader.dll";
                    $"Preparing Loader.dll for injection".Log(LogFiles.PreInjectLog);
                    var res = reader.WriteString(LoaderStrPtr, LoaderStr, Encoding.Unicode);
                    if (!res)
                    {
                        MessageBox.Show("Couldnt write dll path to WoW's memory");
                        return;
                    }
                    Thread.Sleep(1000);
                    $"Starting injection".Log(LogFiles.PreInjectLog);
                    if (
                        WinImports.CreateRemoteThread(proc.Handle, (IntPtr) null, (IntPtr) 0, loadDllPtr, LoaderStrPtr,
                            0,
                            (IntPtr) null) == (IntPtr) 0)
                        MessageBox.Show("Couldnt inject the dll");
                    Thread.Sleep(1);
                    $"Freeing allocated memory for injection".Log(LogFiles.PreInjectLog);
                    reader.FreeMemory(LoaderStrPtr);
                }
            }
            catch (Exception e)
            {
                $"Exception occured while injecting: {e.Message}".Log(LogFiles.PreInjectLog);
                MessageBox.Show(e.Message);
            }
        }
    }
}