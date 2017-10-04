using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using ZzukBot.Annotations;
using ZzukBot.Constants;
using ZzukBot.WPF.Commands;

#pragma warning disable 1591

namespace ZzukBot.WPF
{
    public class ViewModel : INotifyPropertyChanged
    {
        internal readonly CommandHandler Commands;
        private bool _isEnabled = true;
        private DialogResult? _result;

        internal ViewModel()
        {
            Commands = new CommandHandler(this);
        }

        public DialogResult? Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public string WindowTitle => "ZzukBot - " + Other.BotVersion;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}