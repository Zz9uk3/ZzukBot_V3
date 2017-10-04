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
using ZzukBot.ExtensionMethods;
using ZzukBot.Game.Transport;
using ZzukBot.Objects;
using ObjectManager = ZzukBot.Game.Statics.ObjectManager;

namespace TransportRecorder
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            tbWait1.ReadOnly = true;
            tbRest.ReadOnly = true;
            tbRelative1.ReadOnly = true;
            tbArrived.ReadOnly = true;
            tbEnd1.ReadOnly = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void bSave_Click(object sender, EventArgs e)
        {
            var name = tbFileName.Text;

            var list = "new List<Location> { " + tbRelative1.Text + " }";

            var text = @"var transport = new Transport(" + tbWait1.Text + @", "+ tbRest.Text + @", " + tbArrived.Text + @", " + list + @",
                "+ tbEnd1.Text +");";

            text.Log(tbFileName.Text, false, false);

        }

        private void bWait1_Click(object sender, EventArgs e)
        {
            var pos = ObjectManager.Instance.Player.Position;
            var posStr = $"new Location({pos.X}f, {pos.Y}f, {pos.Z}f)";
            tbWait1.Text = posStr;
        }

        private void bRest_Click(object sender, EventArgs e)
        {
            var transport = ObjectManager.Instance.Player.CurrentTransport;
            if (transport == null) return;
            var pos = transport.Position;
            var posStr = $"new Location({pos.X}f, {pos.Y}f, {pos.Z}f)";
            tbRest.Text = posStr;
        }

        private void bRelative_Click(object sender, EventArgs e)
        {
            var pos = ObjectManager.Instance.Player.Position;
            var posStr = tbRelative1.Text + $"new Location({pos.X}f, {pos.Y}f, {pos.Z}f),";
            posStr.TrimEnd(',');
            tbRelative1.Text = posStr;
            
        }

        private void bArrived_Click(object sender, EventArgs e)
        {
            var transport = ObjectManager.Instance.Player.CurrentTransport;
            if (transport == null) return;
            var pos = transport.Position;
            var posStr = $"new Location({pos.X}f, {pos.Y}f, {pos.Z}f)";
            tbArrived.Text = posStr;
        }

        private void bEnd1_Click(object sender, EventArgs e)
        {
            var pos = ObjectManager.Instance.Player.Position;
            var posStr = $"new Location({pos.X}f, {pos.Y}f, {pos.Z}f)";
            tbEnd1.Text = posStr;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            foreach (var item in Controls)
            {
                var tb = item as TextBox;
                tb?.Clear();
            }
        }
    }
}
