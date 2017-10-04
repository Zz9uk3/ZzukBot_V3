using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework;
using ZzukBot.ExtensionFramework.Interfaces;
using ZzukBot.ExtensionMethods;
using ZzukBot.Game;
using ZzukBot.Game.Statics;
using ZzukBot.Properties;
using ZzukBot.Settings;
using ZzukBot.WPF;

#pragma warning disable 1591

namespace ZzukBot.UI.MainWindow
{
    internal class MainViewModel : ViewModel
    {
        public class PluginInfo
        {
            public PluginInfo(IPlugin plugin, bool autoLoad)
            {
                Plugin = plugin;
                AutoLoad = autoLoad;
            }
            public IPlugin Plugin { get; set; }
            public bool AutoLoad { get; set; }
        }

        private ObservableCollection<IBotBase> _availableBotBases;
        private ObservableCollection<PluginInfo> _availablePlugins;
        private bool _beepName;
        private bool _beepSay;
        private bool _beepYell;
        private bool _botBaseRunning;
        private ObservableCollection<WoWEventHandler.ChatMessageArgs> _chatMessages;
        private IBotBase _currentBotBase;
        private PluginInfo _currentPlugin;
        private ObservableCollection<string> _invites;
        private ObservableCollection<IPlugin> _loadedPlugins;
        private bool _logChat;
        private bool _logInvites;
        private ObservableCollection<WoWEventHandler.OnLootArgs> _lootedItems;
        private bool _pluginLoaderEnabled;
        private Statistics _stats;
        private string _wowAccount;
        private string _wowPassword;

        public MainViewModel()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingD‌​efault(false);

            LoadedPlugins = new ObservableCollection<IPlugin>();
            Commands.Register<IPlugin>("PluginToggledCommand", plugin => true, LoadPlugin);

            Commands.Register("ShowPluginGuiCommand", CanShowPluginGui, ShowPluginGui);

            AvailablePlugins =
                new ObservableCollection<PluginInfo>(Plugins.Instance.Items.Select(x => new PluginInfo(x, ShouldAutoLoad(x))));
            LoadedPlugins = new ObservableCollection<IPlugin>(AvailablePlugins.Where(x => x.AutoLoad).Select(x => x.Plugin));

            Commands.Register("ReloadPluginsCommand", CanReloadPlugins, ReloadPlugins);

            AvailableBotBases = new ObservableCollection<IBotBase>(BotBases.Instance.Items);
            Commands.Register("ReloadBotBasesCommand", CanReloadBotBases, ReloadBotBases);

            Commands.Register("StartBotBaseCommand", CanStartBotBase,
                StartBotBase);

            Commands.Register("StopBotBaseCommand", CanStopBotBase, StopBotBase);

            Commands.Register("ShowBotBaseGuiCommand", CanShowBotBaseGui, ShowBotBaseGui);

            Options.Values.Load();
            WowAccount = Options.Values.AccountName;
            WowPassword = Options.Values.AccountPassword;
            BeepSay = Options.Values.BeepSay;
            BeepYell = Options.Values.BeepYell;
            BeepName = Options.Values.BeepName;
            LogChat = Options.Values.LogChat;
            LogInvites = Options.Values.LogInvites;


            ChatMessages = new ObservableCollection<WoWEventHandler.ChatMessageArgs>();
            LootedItems = new ObservableCollection<WoWEventHandler.OnLootArgs>();
            Invites = new ObservableCollection<string>();
            PropertyChanged += OnPropertyChanged;
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += UiUpdaterTick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 60);
            dispatcherTimer.Start();
            Stats = new Statistics();
            WoWEventHandler.Instance.OnChatMessage += OnWoWChatMessage;
            WoWEventHandler.Instance.OnLoot +=
                OnWoWLoot;
            Commands.Register("ResetStatsCommand", ResetStats);
            Commands.Register("ResetChatLogCommand", ChatMessages.Clear);
            Commands.Register("ResetLootLogCommand", LootedItems.Clear);

            Commands.Register("ResetInviteLogCommand", Invites.Clear);
            Commands.Register("ResetDebugLogCommand", SharedViewModel.Instance.DebugLog.Clear);
            Commands.Register("SaveSettingsCommand", SaveSettings);

            WoWEventHandler.Instance.OnGuildInvite += OnWoWGuildInvite;
            WoWEventHandler.Instance.OnPartyInvite += OnWoWPartyInvite;
            WoWEventHandler.Instance.OnDuelRequest += OnWoWDuelRequest;

            PluginLoaderEnabled = true;
        }


        public string WowAccount
        {
            get { return _wowAccount; }
            set
            {
                _wowAccount = value;
                OnPropertyChanged();
            }
        }

        public string WowPassword
        {
            get { return _wowPassword; }
            set
            {
                _wowPassword = value;
                OnPropertyChanged();
            }
        }

        public bool BeepSay
        {
            get { return _beepSay; }
            set
            {
                _beepSay = value;
                OnPropertyChanged();
            }
        }

        public bool BeepYell
        {
            get { return _beepYell; }
            set
            {
                _beepYell = value;
                OnPropertyChanged();
            }
        }

        public bool BeepName
        {
            get { return _beepName; }
            set
            {
                _beepName = value;
                OnPropertyChanged();
            }
        }

        public bool LogChat
        {
            get { return _logChat; }
            set
            {
                _logChat = value;
                OnPropertyChanged();
            }
        }

        public bool LogInvites
        {
            get { return _logInvites; }
            set
            {
                _logInvites = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<WoWEventHandler.ChatMessageArgs> ChatMessages
        {
            get { return _chatMessages; }
            set
            {
                _chatMessages = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Invites
        {
            get { return _invites; }
            set
            {
                _invites = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<WoWEventHandler.OnLootArgs> LootedItems
        {
            get { return _lootedItems; }
            set
            {
                _lootedItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IBotBase> AvailableBotBases
        {
            get { return _availableBotBases; }
            set
            {
                _availableBotBases = value;
                OnPropertyChanged();
                if (value.Count == 0) return;
                CurrentBotBase = value[0];
            }
        }

        public ObservableCollection<PluginInfo> AvailablePlugins
        {
            get { return _availablePlugins; }
            set
            {
                _availablePlugins = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IPlugin> LoadedPlugins
        {
            get { return _loadedPlugins; }
            set
            {
                _loadedPlugins = value;
                OnPropertyChanged();
            }
        }

        public PluginInfo CurrentPlugin
        {
            get { return _currentPlugin; }
            set
            {
                _currentPlugin = value;
                OnPropertyChanged();
            }
        }

        public IBotBase CurrentBotBase
        {
            get { return _currentBotBase; }
            set
            {
                _currentBotBase = value;
                OnPropertyChanged();
            }
        }

        public Statistics Stats
        {
            get { return _stats; }
            set
            {
                _stats = value;
                OnPropertyChanged();
            }
        }

        public bool PluginLoaderEnabled
        {
            get { return _pluginLoaderEnabled; }
            set
            {
                _pluginLoaderEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool BotBaseRunning
        {
            get { return _botBaseRunning; }
            set
            {
                _botBaseRunning = value;
                OnPropertyChanged();
            }
        }

        private void ShowBotBaseGui()
        {
            CurrentBotBase.ShowGui();
        }

        private void StopBotBase()
        {
            CurrentBotBase.Stop();
        }

        private void ShowPluginGui()
        {
            CurrentPlugin?.Plugin.ShowGui();
        }

        private void OnWoWDuelRequest(object sender, WoWEventHandler.OnRequestArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                var logItem = "[" + DateTime.Now.ToShortTimeString() + "] [" + args.Player + "] Duel request";
                Invites.Add(logItem);
                if (!LogInvites) return;
                logItem.Log(LogFiles.InviteLog);
            }));
        }

        private void OnWoWPartyInvite(object sender, WoWEventHandler.OnRequestArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                var logItem = "[" + DateTime.Now.ToShortTimeString() + "] [" + args.Player + "] Party invite";
                Invites.Add(logItem);
                if (!LogInvites) return;
                logItem.Log(LogFiles.InviteLog);
            }));
        }

        private void OnWoWGuildInvite(object sender, WoWEventHandler.GuildInviteArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                var logItem = "[" + DateTime.Now.ToShortTimeString() + "] [" + args.Player + "] Guild invite to " +
                              args.Guild;
                Invites.Add(logItem);
                if (!LogInvites) return;
                logItem.Log(LogFiles.InviteLog);
            }));
        }

        private void SaveSettings()
        {
            Options.Values.AccountName = WowAccount;
            Options.Values.AccountPassword = WowPassword;
            Options.Values.BeepSay = BeepSay;
            Options.Values.BeepYell = BeepYell;
            Options.Values.BeepName = BeepName;
            Options.Values.LogChat = LogChat;
            Options.Values.LogInvites = LogInvites;
            Options.Values.Save();
        }

        private void OnWoWLoot(object sender, WoWEventHandler.OnLootArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action) (() => { LootedItems.Add(args); }));
        }

        private void OnWoWChatMessage(object sender, WoWEventHandler.ChatMessageArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                try
                {
                    ChatMessages.Add(args);
                    if (args.ChatChannel == "Say" && BeepSay)
                        PlayBeep();
                    if (args.ChatChannel == "Yell" && BeepYell)
                        PlayBeep();
                    if (args.Message.ToLower().Contains(Stats.ToonName) && BeepName)
                        PlayBeep();

                    if (!LogChat) return;
                    args.ToString().Log(LogFiles.ChatLog);
                }
                catch
                {
                }
            }));
        }

        private void UiUpdaterTick(object sender, EventArgs args)
        {
            var player = ObjectManager.Instance.Player;
            if (player == null) return;
        }

        private void StartBotBase()
        {
            BotBaseRunning = CurrentBotBase.Start(BotBaseCallBack);
        }

        private void ReloadBotBases()
        {
            AvailableBotBases.Clear();
            BotBases.Instance.Refresh();
            foreach (var item in BotBases.Instance.Items)
                AvailableBotBases.Add(item);
            CurrentBotBase = AvailableBotBases.FirstOrDefault();
        }

        private void ReloadPlugins()
        {
            AvailablePlugins.Clear();
            Plugins.Instance.Refresh();
            foreach (var item in Plugins.Instance.Items)
            {
                AvailablePlugins.Add(new PluginInfo(item, ShouldAutoLoad(item)));
            }
            CurrentPlugin = AvailablePlugins.FirstOrDefault();
        }

        private void PlayBeep()
        {
            WinImports.PlaySound(Resources.beep, IntPtr.Zero,
                WinImports.SoundFlags.SND_ASYNC | WinImports.SoundFlags.SND_MEMORY);
        }

        private bool CanStartBotBase()
        {
            if (CurrentBotBase == null) return false;
            return !BotBaseRunning;
        }

        private bool CanStopBotBase()
        {
            if (CurrentBotBase == null) return false;
            return BotBaseRunning;
        }

        private bool CanReloadBotBases()
        {
            return !BotBaseRunning;
        }

        private void BotBaseCallBack()
        {
            new Action(() => { BotBaseRunning = false; }).BeginDispatch(DispatcherPriority.Normal);
        }

        private void LoadPlugin(IPlugin plugin)
        {
            var savePlugin = $"{plugin.Name}_{plugin.Author}";
            if (LoadedPlugins.Contains(plugin))
            {
                LoadedPlugins.Remove(plugin);
                Options.Values.LoadedPlugins.Remove(savePlugin);
                Options.Values.Save();
                plugin.Unload();
                return;
            }
            LoadedPlugins.Add(plugin);
            Options.Values.LoadedPlugins.Add(savePlugin);
            Options.Values.Save();
            plugin.Load();
        }

        private bool ShouldAutoLoad(IPlugin plugin)
        {
            var savePlugin = $"{plugin.Name}_{plugin.Author}";
            return Options.Values.LoadedPlugins.Any(x => x == savePlugin);

        }

        private bool CanShowPluginGui()
        {
            return CurrentPlugin != null && PluginLoaderEnabled;
        }

        private bool CanShowBotBaseGui()
        {
            return CurrentBotBase != null;
        }

        private bool CanReloadPlugins()
        {
            if (!PluginLoaderEnabled) return false;
            return LoadedPlugins.Count == 0;
        }

        private void ResetStats()
        {
            Stats.Dispose();
            Stats = new Statistics();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }
}