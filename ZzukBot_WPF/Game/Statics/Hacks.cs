using System;
using ZzukBot.Mem.AntiWarden;

namespace ZzukBot.Game.Statics
{
    /// <summary>
    ///     Class to deal with little hacks (All warden-proof)
    /// </summary>
    public sealed class Hacks
    {
        private static readonly Lazy<Hacks> _instance = new Lazy<Hacks>(() => new Hacks());

        private Hacks()
        {
        }

        /// <summary>
        ///     Access to the instance
        /// </summary>
        public static Hacks Instance => _instance.Value;

        internal bool AntiCtmStutter
        {
            get { return IsHackActive("Ctm"); }
            set { ToggleHack("Ctm", value); }
        }

        /// <summary>
        ///     Deactive collision with non interactable objects (fences, trees, etc.)
        /// </summary>
        /// <value>
        ///     true will disable collision. false will enable it again
        /// </value>
        public bool Collision1
        {
            get { return IsHackActive("Collision"); }
            set { ToggleHack("Collision", value); }
        }

        /// <summary>
        ///     Deactivate collision with objects that show a tooltip while hovering over (mailboxes, gates, ores, herbs etc.)
        /// </summary>
        /// <value>
        ///     true will disable collision. false will enable it again
        /// </value>
        public bool Collision2
        {
            get { return IsHackActive("Collision3"); }
            set { ToggleHack("Collision3", value); }
        }

        private static bool IsHackActive(string hack)
        {
            var hackInstance = HookWardenMemScan.GetHack(hack);
            return hackInstance != null && hackInstance.IsActivated;
        }

        private static void ToggleHack(string hack, bool state)
        {
            var hackInstance = HookWardenMemScan.GetHack(hack);
            if (hackInstance == null) return;
            if (state)
                hackInstance.Apply();
            else
                hackInstance.Remove();
        }
    }
}