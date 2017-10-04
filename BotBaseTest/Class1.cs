using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ZzukBot.ExtensionFramework.Interfaces;

namespace BotBaseTest
{
    [Export(typeof(IBotBase))]
    public class Class1 : IBotBase
    {
        private Action _stopCallback;
        public bool Start(Action OnStopCallback)
        {
            var newInstace = new BotBaseDependency.Class1();
            newInstace.RunThis();
            MessageBox.Show("STARTED");
            _stopCallback = OnStopCallback;
            return true;
        }

        public void Stop()
        {
            MessageBox.Show("STOPPED");
            _stopCallback();
        }

        public void ShowGui()
        {
        }

        public string Name { get; } = "MyTestBotBase";
        public string Author { get; } = "Zzuk";
        public int Version { get; } = 10;

        public void Dispose()
        {
        }

        public void PauseBotbase(Action onPauseCallback)
        {
            throw new NotImplementedException();
        }

        public bool ResumeBotbase()
        {
            throw new NotImplementedException();
        }
    }
}
