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
    internal sealed class BotBases
    {
        private static readonly Lazy<BotBases> _instance = new Lazy<BotBases>(() => new BotBases());

        [ImportMany(typeof(IBotBase), AllowRecomposition = true)]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private List<IBotBase> _botBases = null;

        private AggregateCatalog _catalog;
        private CompositionContainer _container;

        private BotBases()
        {
            Refresh();
        }

        internal static BotBases Instance => _instance.Value;
        internal List<IBotBase> Items => _botBases;

        internal void Refresh()
        {
            if (_botBases != null)
                foreach (var x in _botBases)
                {
                    $"BotBases: Disposing botbase {x.Name}".Log(LogFiles.InjectedLog, true);
                    x.Stop();
                    x.Dispose();
                }
            if (_catalog != null)
            {
                $"BotBases: Disposing old catalog".Log(LogFiles.InjectedLog, true);
                _catalog.Catalogs.Clear();
                _catalog?.Dispose();
            }
            _catalog = new AggregateCatalog();
            _botBases?.Clear();
            _container?.Dispose();
            Action<string> load = item =>
            {
                if (!item.EndsWith(".dll")) return;
                $"BotBases: Noting down {item} as possible botbase".Log(LogFiles.InjectedLog, true);
                var dir = Path.GetDirectoryName(item);
                DependencyLoader.SetPluginPath(dir);
                var assembly = Assembly.Load(File.ReadAllBytes(item));
                _catalog.Catalogs.Add(new AssemblyCatalog(assembly));
            };
            foreach (var path in Directory.EnumerateDirectories(Paths.BotBases, "*", SearchOption.TopDirectoryOnly))
            foreach (var item in Directory.GetFiles(path))
                load(item);
            foreach (var item in Directory.GetFiles(Paths.BotBases))
                load(item);
            _container = new CompositionContainer(_catalog);
            $"BotBases: Composing catalogs".Log(LogFiles.InjectedLog, true);
            _container.ComposeParts(this);
            var tmpList = new HashSet<string>();
            foreach (var x in _botBases)
            {
                var name = x.Name.ToLower();
                $"BotBases: Loaded {name} botbase".Log(LogFiles.InjectedLog, true);
                if (tmpList.Contains(name))
                    App.QuitWithMessage(
                        $"Name of Botbases must be unique however there are two or more bases with name {x.Name}");
                tmpList.Add(name);
            }
            Memory.HideAdditionalModules();
        }
    }
}