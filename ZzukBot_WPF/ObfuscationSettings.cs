using System.Reflection;

[assembly: Obfuscation(Feature = "encrypt resources [compress]", Exclude = false)]
[assembly: Obfuscation(Feature = "string encryption", Exclude = false)]
[assembly: Obfuscation(Feature = "code control flow obfuscation", Exclude = false)]
[assembly: ObfuscateAssembly(false)]
[assembly: Obfuscation(Feature = "rename serializable symbols", Exclude = false)]