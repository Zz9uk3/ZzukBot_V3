using System;
using System.Collections.Generic;
using ZzukBot.Properties;

namespace ZzukBot.Helpers
{
    internal static class Shared
    {
        internal static List<string> DefaultModules =
            new List<string>(Resources.ModulesOfWoW.Split(new[] {Environment.NewLine}, StringSplitOptions.None));
    }
}