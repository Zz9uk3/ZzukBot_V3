using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace TransportRecorder
{
    [Export(typeof(ZzukBot.ExtensionFramework.Interfaces.IPlugin))]
    public class TransportRecorder : ZzukBot.ExtensionFramework.Interfaces.IPlugin
    {
        private MainForm _form;

        public TransportRecorder()
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

        public string Name { get; } = "TransportRecorder";
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
