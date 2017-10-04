using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using ZzukBot;

// ReSharper disable once CheckNamespace

namespace NothingToSeeHere
{
    [Obfuscation(Feature = "renaming", ApplyToMembers = true)]
    internal static class Magikarp
    {
        private static Thread _thread;

        [STAThread]
        [Obfuscation(Feature = "virtualization", Exclude = true)]
        private static int BestPokemon(string args)
        {
            _thread = new Thread(App.Main);
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
            return 1;
        }
    }
}