using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ZzukBot.Constants;
using ZzukBot.ExtensionMethods;
using ZzukBot.Game.Frames;
using ZzukBot.Game.Statics;
using ZzukBot.Game.Transport;
using ZzukBot.Mem;
using ZzukBot.Objects;

namespace ZzukBot.Helpers
{
#if DEBUG
    internal static class DebugAssist
    {
        private static bool Applied;

        internal static void Init()
        {
            if (Applied) return;
            Task.Run(() => ConsoleReader());
            Applied = true;
        }

        private static readonly Transport Transport;

        private static Location loc1;

        private static void ConsoleReader()
        {
            while (true)
                try
                {
                    var input = Console.ReadLine();
                    if (input != null)
                    {
                        if (input == "test")
                        {
                            Console.WriteLine(ObjectManager.Instance.Player.HasPet);
                        }
                    }
                    Task.Delay(10).Wait();
                }
                catch (Exception)
                {
                    // ignored
                }
            // ReSharper disable once FunctionNeverReturns
        }

        private static int DoSomething()
        {
            return 10;
        }
    }
#endif
}