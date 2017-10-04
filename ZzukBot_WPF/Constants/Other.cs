using System.Reflection;

#pragma warning disable 1591

namespace ZzukBot.Constants
{
    internal static class Other
    {
        internal static string BotVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}