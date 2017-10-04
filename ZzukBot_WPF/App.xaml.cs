using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Threading;
using Microsoft.Win32;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework;
using ZzukBot.ExtensionMethods;
using ZzukBot.Game.Statics;
using ZzukBot.Helpers;
using ZzukBot.Injector;
using ZzukBot.Mem;
using ZzukBot.Settings;
using ZzukBot.UI;
using ZzukBot.UI.MainWindow;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace ZzukBot
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    internal partial class App
    {
        private const string FasmNetDll = "Fasm.NET.dll";
        private const string LoaderDll = "Loader.dll";
        private const string SettingsSettingsJson = "..\\Settings\\Settings.json";
        private const string Settings = "..\\Settings";
        private const string Botbases = "..\\Botbases";
        private const string Plugins = "..\\Plugins";
        private const string Logs = "..\\Logs";
        private const string CustomClasses = "..\\CustomClasses";
        private static EventWaitHandle _mutexEvent;

        private Thread _thread;
        private static bool AreWeInjected => !Process.GetCurrentProcess().ProcessName.StartsWith("ZzukBot");

        internal static T ShowInvoke<T>() where T : Window
        {
            Current.Dispatcher.Invoke(() =>
            {
                var instance = (T) Activator.CreateInstance(typeof(T), new object[] {});
                instance.ShowDialog();
                return instance;
            });
            return null;
        }


        private static bool IsAlreadyRunning()
        {
            bool created;
            _mutexEvent = new EventWaitHandle(false,
                // ReSharper disable once PossibleNullReferenceException
                EventResetMode.ManualReset, Assembly.GetExecutingAssembly().Location.Replace("\\", "#"), out created);
            return !created;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //DispatcherUnhandledException += (sender, args) =>
            //{
            //    args.Handled = true;
            //    args.Exception.ToString().Log(LogFiles.WpfExceptions);
            //};
            if (!IsAlreadyRunning())
            {
                LaunchBot();
            }
            else
                QuitWithMessage("An instance is already running from this location");
            base.OnStartup(e);
        }

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void LaunchBot()
        {
            SetPaths();
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            if (!AreWeInjected)
            {
                LogFiles.PreInjectLog.ClearLog();
                LogFiles.QuitLog.ClearLog();
                LogFiles.InjectedLog.ClearLog();
                "We are not injected yet".Log(LogFiles.PreInjectLog);
                PrepareAndInject();
                Environment.Exit(0);
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    MessageBox.Show("A exception occured! The details were logged");
                    args.ExceptionObject.ToString().Log(LogFiles.Exceptions, false);
                };
                AppDomain.CurrentDomain.AssemblyResolve += DependencyLoader.CurrentDomain_AssemblyResolve;
#if DEBUG
                Debugger.Launch();
                WinImports.AllocConsole();
                DebugAssist.Init();
#endif
                SetRealmlist();
                try
                {
                    $"Enabling the login block until the user authenticates".Log(LogFiles.InjectedLog, true);
                    LoginBlock.Enable();
                    Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    $"Bringing up the login window".Log(LogFiles.InjectedLog, true);
                    var loginWindow = new LoginWindow();
                    loginWindow.ShowDialog();
                    var loginModel = (LoginViewModel) loginWindow.DataContext;
                    if (loginModel.Result == null || loginModel.Result.Value != DialogResult.OK)
                        Environment.Exit(0);
                    $"Initialising the bot".Log(LogFiles.InjectedLog, true);
                    Memory.Init();
                    $"Disabling the login block".Log(LogFiles.InjectedLog, true);
                    LoginBlock.Disable();
                    $"Showing the bots mainwindow".Log(LogFiles.InjectedLog, true);
                    var mainWindow = new MainWindow();
                    Current.MainWindow = mainWindow;
                    mainWindow.Closed += (sender, args) => { Environment.Exit(0); };
                    mainWindow.Show();
                }
                catch (Exception e)
                {
                    e.ToString().Log(LogFiles.Exceptions);
                }
            }
        }

        private static void PrepareAndInject()
        {
            FileChecks();
            SettingsCheck();
            _mutexEvent.Close();
            Launch.Run();
        }

        private static void SettingsCheck()
        {
            $"Checking settings".Log(LogFiles.PreInjectLog);
            if (File.Exists(SettingsSettingsJson)) return;
            while (true)
            {
                $"Asking user for path to the WoW.exe".Log(LogFiles.PreInjectLog);
                var loc = new OpenFileDialog
                {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = "executable (*.exe)|*.exe",
                    FilterIndex = 1,
                    Title = "Please locate your WoW.exe"
                };
                if (loc.ShowDialog() != DialogResult.OK) return;
                $"User selected {loc.FileName} as 1.12.1 WoW executable".Log(LogFiles.PreInjectLog);
                if (loc.FileName == Assembly.GetExecutingAssembly().Location)
                {
                    MessageBox.Show("Please select the WoW executable!");
                }
                else
                {
                    Options.Values.PathToWoW = loc.FileName;
                    break;
                }
            }
            $"Saving the settings".Log(LogFiles.PreInjectLog);
            Options.Values.Save();
        }

        private static void FileChecks()
        {
            "Checking folder structures of the bot".Log(LogFiles.PreInjectLog);
            Settings.CreateFolderStructure();
            Botbases.CreateFolderStructure();
            Plugins.CreateFolderStructure();
            Logs.CreateFolderStructure();
            CustomClasses.CreateFolderStructure();
            $"Checking FasmNet.dll".Log(LogFiles.PreInjectLog);
            if (!FasmNetDll.FileEqualTo(ZzukBot.Properties.Resources.Fasm_NET))
                QuitWithMessage("Fasm.NET.dll is broken. Please redownload");
            $"Checking mmaps".Log(LogFiles.PreInjectLog);
            if (!Directory.Exists("mmaps"))
                QuitWithMessage("Download the mmaps first please");
            if (Directory.GetFileSystemEntries("mmaps").Length < 1000)
            {
                QuitWithMessage("Download the mmaps first please");
            }
            $"Checking Loader.dll".Log(LogFiles.PreInjectLog);
            LoaderDll.FileCreate(ZzukBot.Properties.Resources.Loader);
        }

        private static void SetPaths()
        {
            Paths.PathToWoW = Directory.GetCurrentDirectory();
            // get all kind of paths the bot need to operate
            var asm = Assembly.GetExecutingAssembly();

            Paths.Settings = asm.ExtJumpUp(1) + "\\" + Settings + "\\Settings.json";
            Paths.Plugins = asm.ExtJumpUp(1) + "\\" + Plugins;
            Paths.BotBases = asm.ExtJumpUp(1) + "\\" + Botbases;
            Paths.CustomClasses = asm.ExtJumpUp(1) + "\\" + CustomClasses;
            Paths.Logs = asm.ExtJumpUp(1) + "\\" + Logs;
            Paths.Root = asm.ExtJumpUp(2);
            Paths.ZzukBot = asm.Location;
            Paths.Internal = asm.ExtJumpUp(1);
        }

        private static void SetRealmlist()
        {
            $"Reading realmlist.wtf to determine which project we are connecting to".Log(LogFiles.InjectedLog, true);
            Options.Values.Load();
            var rlmList = Paths.PathToWoW + "\\realmlist.wtf";
            var project = File.ReadAllLines(rlmList);
            var name = "";
            foreach (var x in project)
                if (x.ToLower().StartsWith("set realmlist "))
                    name = x.ToLower();
            Options.Values.RealmList = name;
            $"We are on project {name.Replace("set realmlist ", "")}".Log(LogFiles.InjectedLog, true);
            Options.Values.Save();
        }


        internal static string GetMd5AsBase64(string parFile)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(parFile))
                {
                    return Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }
        }

        internal static void QuitWithMessage(string parMessage)
        {
            MessageBox.Show(parMessage);
            if (!string.IsNullOrWhiteSpace(Paths.Logs))
                parMessage.Log(LogFiles.QuitLog);
            Environment.Exit(0);
        }

        internal class Singleton : Attribute
        {
            public int Order { get; set; } = int.MaxValue;

            internal static void Initialise()
            {
                // get a list of types which are marked with the InitOnLoad attribute
                var types =
                    AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .Where(t => t.GetCustomAttributes(typeof(Singleton), false).Any())
                        .OrderBy(t => ((Singleton)t.GetCustomAttribute(typeof(Singleton), false)).Order);

                // process each type to force initialise it
                foreach (var type in types)
                {
                    // try to find a static field which is of the same type as the declaring class
                    var field =
                        type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                            .FirstOrDefault(f => f.FieldType == type);
                    // evaluate the static field if found
                    field?.GetValue(null);
                }
                ObjectManager.Unwrap();
            }
        }
    }
}