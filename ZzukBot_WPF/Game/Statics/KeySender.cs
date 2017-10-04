using System;
using System.Windows.Forms;
using ZzukBot.Mem;

namespace ZzukBot.Game.Statics
{
    /// <summary>
    ///     Class to send key-presses to WoW
    /// </summary>
    public class KeySender
    {
        /// <summary>
        ///     Key-modifiers
        /// </summary>
        public enum KeyModifier
        {
            NONE = 0,
            WM_SYSKEYDOWN = 260,
            WM_SYSKEYUP = 261,
            WM_CHAR = 258,
            WM_KEYDOWN = 256,
            WM_KEYUP = 257
        }

        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<KeySender> _instance = new Lazy<KeySender>(() => new KeySender());

        private KeySender()
        {
        }

        /// <summary>
        ///     Access to KeySender
        /// </summary>
        public static KeySender Instance => _instance.Value;

        /// <summary>
        ///     Sends a simple keypress (key down and up)
        /// </summary>
        /// <param name="parKey">The key</param>
        public void SendDownUp(Keys parKey)
        {
            MainThread.Instance.SendDownUp(parKey);
        }

        /// <summary>
        ///     Sends the key
        /// </summary>
        /// <param name="parKey">The key</param>
        /// <param name="parModifier">The modifier (up, down etc.)</param>
        public void Send(Keys parKey, KeyModifier parModifier = KeyModifier.NONE)
        {
            MainThread.Instance.Send(parKey, parModifier);
        }

        /// <summary>
        ///     Send a key down
        /// </summary>
        /// <param name="parKey">Key</param>
        public void SendDown(Keys parKey)
        {
            MainThread.Instance.Send(parKey, KeyModifier.WM_KEYDOWN);
        }

        /// <summary>
        ///     Send a key up
        /// </summary>
        /// <param name="parKey">Key</param>
        public void SendUp(Keys parKey)
        {
            MainThread.Instance.Send(parKey, KeyModifier.WM_KEYUP);
        }
    }
}