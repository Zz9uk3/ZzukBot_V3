using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ZzukBot.Authentication.Models;

namespace ZzukBot.Authentication.AuthClient
{
    [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "Apply to member * when constructor or method or event: virtualization")]
    internal class AuthClientHandler
    {
        private static readonly object _lock = new object();
        private static readonly Lazy<AuthClientHandler> _instance = new Lazy<AuthClientHandler>(() => new AuthClientHandler());
        internal static AuthClientHandler Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance.Value;
                }
            }
        }
        private AuthClientHandler()
        {
        }

        internal bool CheckConnection()
        {
            try
            {
                if (_connected)
                {
                    CloseConnection();
                }
                _client = new TcpClient(_server, _port);
                _netStream = _client.GetStream();
                _ssl = new SslStream(_netStream, false, ValidateCert)
                {
                    WriteTimeout = 60000,
                    ReadTimeout = 60000
                };
                _ssl.AuthenticateAsClient("ZzukBotVanilla");
                _reader = new BinaryReader(_ssl, Encoding.UTF8);
                _writer = new BinaryWriter(_ssl, Encoding.UTF8);
                _connected = true;
                return true;
            }
            catch
            {
                CloseConnection();
                return false;
            }
        }

        private readonly Random _random = new Random();
        internal void Write(uint opcode, byte[] content = null)
        {
            if (!_connected) return;
            try
            {
                if (content == null)
                    content = new byte[] { 0 };
                try
                {
                    _writer.Write(opcode);
                    _writer.Write(content.Length);
                    _writer.Write(content);
                    _writer.Flush();
                }
                catch
                {
                    CloseConnection();
                }
            }
            catch (Exception e)
            {
            }
        }

        private void CloseConnection()
        {
            lock (_lock)
            {
                if (!_connected) return;
                _connected = false;
                _reader.Close();
                _writer.Close();
                _ssl.Close();
                _netStream.Close();
                _client.Close();
                OnDisconnect?.Invoke(this, null);
                return;
            }
        }
        internal PacketModel GetNextPacket()
        {
            try
            {
                if (!_connected) return null;
                var opcode = _reader.ReadUInt32();
                var length = _reader.ReadInt32();
                var content = _reader.ReadBytes(length);
                return new PacketModel
                {
                    Opcode = opcode,
                    Content = content,
                };
            }
            catch
            {
                CloseConnection();
                return null;
            }
        }

        internal void WriteRandomly(int packetCount, params PacketModel[] packets)
        {
            if (packets.Length > packetCount) throw new Exception();
            var sendOn = new List<int>();
            while (sendOn.Count < packets.Length)
            {
                var nextRan = _random.Next(0, packetCount);
                if (!sendOn.Contains(nextRan))
                    sendOn.Add(nextRan);
            }
            for (var i = 0; i < packetCount; i++)
            {
                var index = sendOn.IndexOf(i);
                if (index == -1)
                {
                    Write(GetUnusedOpcode());
                    continue;
                }
                Write(packets[index].Opcode, packets[index].Content);
            }
        }

        private uint GetUnusedOpcode()
        {
            return (uint)_random.Next(18, int.MaxValue);
        }

        internal PacketModel[] GetRandomly(int packetCount, params uint[] opcodes)
        {
            var opcodeList = opcodes.ToList();
            var ret = new PacketModel[opcodeList.Count];
            for (var i = 0; i < packetCount; i++)
            {
                var pack = GetNextPacket();
                var index = opcodeList.IndexOf(pack.Opcode);
                if (index != -1)
                {
                    ret[index] = pack;
                }
            }
            return ret;
        }


        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private readonly string _server = "auth.zzukbot.com";

        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private readonly int _port = 6200;

        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private volatile bool _connected;

        private TcpClient _client;
        private NetworkStream _netStream;
        private SslStream _ssl;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        internal event EventHandler<PacketModel> OnDisconnect;

        internal static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}