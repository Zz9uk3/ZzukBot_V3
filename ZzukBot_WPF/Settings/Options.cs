using System.Collections.Generic;

#pragma warning disable 1591

namespace ZzukBot.Settings
{
    public class Options
    {
        private Options()
        {
        }

        public string AccountName { get; set; } = "";
        public string AccountPassword { get; set; } = "";
        public bool BeepInvites { get; set; } = false;
        public bool BeepName { get; set; } = false;
        public bool BeepSay { get; set; } = false;
        public bool BeepWhisper { get; set; } = false;
        public bool BeepYell { get; set; } = false;
        public string IrcChannel { get; set; } = "";
        public string IrcNick { get; set; } = "";
        public bool LogChat { get; set; } = false;
        public bool LogInvites { get; set; } = false;
        public string PathToWoW { get; set; } = "";
        public HashSet<string> LoadedPlugins { get; set; } = new HashSet<string>();

        public string RealmList { get; set; } = "";
        public bool UseIrc { get; set; } = false;
        public string ZzukAccount { get; set; } = "";
        public string ZzukPassword { get; set; } = "";
        public bool ZzukSavePassword { get; set; } = false;

        internal static Options Values { get; set; } = new Options();

        internal void Load()
        {
            Values = OptionManager.Get("Settings").LoadFromJson<Options>();
        }

        internal void Save()
        {
            OptionManager.Get("Settings").SaveToJson(this);
        }
    }
}