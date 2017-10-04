using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZzukBot.ExtensionFramework.Interfaces;
using ZzukBot.Game.Statics;

namespace TransportTester
{
    [Export(typeof(IPlugin))]
    public class TransportTester : IPlugin
    {
        private MainForm _form;

        public TransportTester()
        {
        }

        public void Dispose()
        {
            DisposeForms();
        }

        private void DisposeForms()
        {
            foreach (var mainForm in Application.OpenForms.OfType<MainForm>())
            {
                mainForm.Invoke((Action)(() => { mainForm.Close(); }));
            }
            _form?.Dispose();
            _form = null;
        }

        public string Name { get; } = "TransportTester";
        public string Author { get; } = "Zzuk";
        public Version Version { get; } = new Version(1, 0);
        public bool Load()
        {
            return true;
        }

        public void Unload()
        {
            Dispose();
        }

        public void ShowGui()
        {
            if (_form == null) _form = new MainForm();
            _form.ShowDialog();
            DisposeForms();
        }
    }
}
