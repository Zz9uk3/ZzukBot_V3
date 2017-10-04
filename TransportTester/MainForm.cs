using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZzukBot.Game.Statics;
using ZzukBot.Game.Transport;
using ZzukBot.Mem;
using ZzukBot.Objects;
using ObjectManager = ZzukBot.Game.Statics.ObjectManager;

namespace TransportTester
{
    public partial class MainForm : Form
    {
        private readonly MainThread.Updater _updater;

        private void DoTransport()
        {
            var player = ObjectManager.Instance.Player;
            var curPoint = _path[_currentIndex];
            var distance = player.Position.GetDistanceTo2D(curPoint);
            if (distance <= 1.6)
            {
                if (_currentIndex <= _path.Length - 2)
                {
                    _currentIndex++;
                    curPoint = _path[_currentIndex];
                }
                else
                {
                    _updater.Stop();
                    Hacks.Instance.AntiCtmStutter = false;
                }
            }
            player.CtmTo(curPoint);
        }

        private Location _start;
        private Location _end;
        private Location[] _path;
        private int _currentIndex;


        public MainForm()
        {
            _updater = new MainThread.Updater(DoTransport, 20);
            InitializeComponent();
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            _end = ObjectManager.Instance.Player.Position;
        }

        private void bStop_Click(object sender, EventArgs e)
        {
            _start = ObjectManager.Instance.Player.Position;
            _currentIndex = 0;
            var path = ZzukBot.Game.Statics.Navigation.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, _start, _end,
                true);
            _path = path;
            Hacks.Instance.AntiCtmStutter = true;
            _updater.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            _updater.Stop();
            base.OnClosed(e);
        }
    }
}
