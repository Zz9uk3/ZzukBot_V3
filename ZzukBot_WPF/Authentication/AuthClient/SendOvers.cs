using System.Reflection;

namespace ZzukBot.Server.AuthClient
{
    [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "Apply to member * when constructor or method or event: virtualization")]
    internal static class SendOvers
    {
        internal static string[] WardenLoadDetour;

        internal static string[] WardenMemCpyDetour;

        internal static string[] WardenPageScanDetour;

        internal static string[] EventSignal0;

        internal static string[] EventSignal;
    }
}