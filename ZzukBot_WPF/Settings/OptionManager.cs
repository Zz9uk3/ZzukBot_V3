using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using ZzukBot.Helpers;

namespace ZzukBot.Settings
{
    /// <summary>
    ///     Class to save and load settings from the Settiongs folder
    /// </summary>
    public sealed class OptionManager
    {
        private static readonly List<OptionManager> _Managers = new List<OptionManager>();
        private static string _SettingsPath;

        private OptionManager(string parName)
        {
            Name = parName;
        }

        internal string Name { get; }

        internal bool FileExists => File.Exists(SettingsPath);

        private string SettingsPath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_SettingsPath)) return _SettingsPath + $"{Name}.json";
                var tmp1 = Assembly.GetExecutingAssembly().ExtJumpUp(2);
                _SettingsPath = tmp1 + "\\Settings\\";
                return _SettingsPath + $"{Name}.json";
            }
        }


        /// <summary>
        ///     Retrieves an instance managing the passed settings file
        /// </summary>
        /// <remarks>
        ///     <code>
        /// var manager = Get("Settings");
        /// </code>
        ///     Will return a manager dealing with Settings\Settings.json
        /// </remarks>
        /// <param name="parParent">The name of the settings file</param>
        /// <returns>OptionManager instance</returns>
        public static OptionManager Get(string parParent)
        {
            var tmpManager = _Managers.FirstOrDefault(i => i.Name == parParent);
            if (tmpManager != null) return tmpManager;
            tmpManager = new OptionManager(parParent);
            _Managers.Add(tmpManager);
            return tmpManager;
        }

        /// <summary>
        ///     Will save all public fields of the passed object to the file the manager is dealing with
        /// </summary>
        /// <param name="parObject">The object to save</param>
        public void SaveToJson(object parObject)
        {
            var txt = JsonConvert.SerializeObject(parObject, Formatting.Indented).Replace("\r\n", "\n");
            File.WriteAllText(SettingsPath, txt);
        }

        /// <summary>
        ///     Loads the content of the file the manager deals with into an object
        /// </summary>
        /// <remarks>
        ///     If the file doesnt exist yet a default object of type T will be returned<br />
        ///     Make sure that your managers class container has all fields set to a default value not null
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns>The object</returns>
        public T LoadFromJson<T>()
        {
            if (!File.Exists(SettingsPath)) return default(T);
            var txt = File.ReadAllText(SettingsPath);
            return JsonConvert.DeserializeObject<T>(txt);
        }
    }
}