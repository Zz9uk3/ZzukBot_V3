using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZzukBot.Authentication;
using ZzukBot.Authentication.AuthClient;
using ZzukBot.Helpers;
using ZzukBot.Settings;
using ZzukBot.WPF;
using Application = System.Windows.Application;

#pragma warning disable 1591

namespace ZzukBot.UI
{
    internal class LoginViewModel : ViewModel
    {
        private string _email = "";
        private string _password;
        private bool _rememberPassword;
        private bool _waitForAuth;

        public LoginViewModel()
        {
            Options.Values.Load();
            RememberPassword = Options.Values.ZzukSavePassword;
            PropertyChanged += OnPropertyChanged;
            Commands.Register("ProcessAuthenticationCommand", CanAuthenticate, Authenticate);
            Email = Options.Values.ZzukAccount;
            if (Options.Values.ZzukSavePassword)
                Password = Options.Values.ZzukPassword;
        }

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value.ToLower();
                Options.Values.ZzukAccount = Email;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public bool RememberPassword
        {
            get { return _rememberPassword; }
            set
            {
                _rememberPassword = value;
                Options.Values.ZzukSavePassword = RememberPassword;
                OnPropertyChanged();
            }
        }

        private void Authenticate()
        {
            if (RememberPassword)
                Options.Values.ZzukPassword = Password;
            Options.Values.Save();
            IsEnabled = false;
            Task.Run(() =>
            {
                string reason;
                if (AuthProcessor.Instance.Auth(Email, Password, out reason))
                {
                    Application.Current.Dispatcher.Invoke(() => { Result = DialogResult.OK; });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsEnabled = true;
                        var message = $"Authentication failed: {reason}";
                        if (reason.ToLower().Contains("update"))
                        {
                            MessageBox.Show(message + "\r\n" + "Click OK to update the bot");
                            var path = Assembly.GetExecutingAssembly().ExtJumpUp(2);
                            var start = new ProcessStartInfo
                            {
                                FileName = "BotLauncher.exe",
                                WorkingDirectory = path,
                                Arguments = "UPDATE"
                            };
                            Process.Start(start);
                            Environment.Exit(0);
                        }
                        MessageBox.Show(message);
                    });
                }
            });
        }

        public bool CanAuthenticate()
        {
            if (!Email.Contains('@')) return false;
            if (Password == null || Password.Length < 3) return false;
            return true;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
        }
    }
}