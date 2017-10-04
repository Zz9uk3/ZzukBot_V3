using System;
using System.Reflection;

namespace ZzukBot.Authentication.Models
{
    [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "Apply to member * when constructor or method or event: virtualization")]
    internal class PacketModel : EventArgs
    {
        internal uint Opcode { get; set; }
        internal byte[] Content { get; set; }
    }
}