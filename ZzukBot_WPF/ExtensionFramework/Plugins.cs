using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using ZzukBot.ExtensionFramework.Interfaces;
using ZzukBot.ExtensionMethods;
using ZzukBot.Mem;
using ZzukBot.Settings;

namespace ZzukBot.ExtensionFramework
{
    internal sealed class Plugins
    {
        private static readonly Lazy<Plugins> _instance = new Lazy<Plugins>(() => new Plugins());

        private AggregateCatalog _catalog;
        private CompositionContainer _container;

        [ImportMany(typeof(IPlugin), AllowRecomposition = true)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private List<IPlugin> _plugins = null;

        private Plugins()
        {
            Refresh();
        }

        internal static Plugins Instance => _instance.Value;

        internal List<IPlugin> Items => _plugins;

        internal void Refresh()
        {
            if (_plugins != null)
                foreach (var x in _plugins)
                {
                    $"Plugins: Disposing plugin {x.Name}".Log(LogFiles.InjectedLog, true);
                    x.Unload();
                    x.Dispose();
                }
            if (_catalog != null)
            {
                $"Plugins: Disposing old catalog".Log(LogFiles.InjectedLog, true);
                _catalog.Catalogs.Clear();
                _catalog?.Dispose();
            }
            _catalog = new AggregateCatalog();
            _plugins?.Clear();
            _container?.Dispose();

            Action<string> load = item =>
            {
                if (!item.EndsWith(".dll")) return;
                $"Plugins: Noting down {item} as possible plugin".Log(LogFiles.InjectedLog, true);
                var dir = Path.GetDirectoryName(item);
                DependencyLoader.SetPluginPath(dir);
                var assembly = Assembly.Load(File.ReadAllBytes(item));
                _catalog.Catalogs.Add(new AssemblyCatalog(assembly));
            };
            foreach (var path in Directory.EnumerateDirectories(Paths.Plugins, "*", SearchOption.TopDirectoryOnly))
            foreach (var item in Directory.GetFiles(path))
                load(item);
            foreach (var item in Directory.GetFiles(Paths.Plugins))
                load(item);
            _container = new CompositionContainer(_catalog);
            $"Plugins: Composing catalogs".Log(LogFiles.InjectedLog, true);
            _container.ComposeParts(this);
            var tmpList = new HashSet<string>();
            foreach (var x in _plugins)
            {
                var name = x.Name.ToLower();
                $"Plugins: Loaded {name} plugin".Log(LogFiles.InjectedLog, true);
                if (tmpList.Contains(name))
                    App.QuitWithMessage(
                        $"Name of Plugins must be unique however there are two or more plugins with name {x.Name}");
                tmpList.Add(name);
            }
            Memory.HideAdditionalModules();
        }
    }
}