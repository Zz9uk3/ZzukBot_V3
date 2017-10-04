using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using ZzukBot.Game.Statics;
using ZzukBot.WPF;

#pragma warning disable 1591

namespace ZzukBot.Game
{
    /// <summary>
    ///     A class containing stats (units killed, xp per hour etc.)
    /// </summary>
    public class Statistics : ViewModel, IDisposable
    {
        private readonly List<DuelRequest> _duelRequests = new List<DuelRequest>();
        private readonly List<GuildInvite> _guildInvites = new List<GuildInvite>();
        private readonly List<Loot> _lootList = new List<Loot>();
        private readonly List<PartyInvite> _partyInvites = new List<PartyInvite>();
        private readonly Timer _runningTimer;
        private readonly int _startTime;
        private int _copperGained;
        private bool _dispose;

        private int? _startCopperCount;
        private string _toonName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Statistics" /> class.
        /// </summary>
        public Statistics()
        {
            _startTime = Environment.TickCount;

            WoWEventHandler.Instance.OnXpGain += (sender, args) =>
            {
                XpGained += args.Xp;
                OnPropertyChanged(nameof(XpGained));
                OnPropertyChanged(nameof(XpHour));
            };

            WoWEventHandler.Instance.LevelUp += (sender, args) =>
            {
                LevelUps++;
                OnPropertyChanged(nameof(LevelUps));
            };

            WoWEventHandler.Instance.OnDeath += (sender, args) =>
            {
                Deaths++;
                OnPropertyChanged(nameof(Deaths));
            };

            WoWEventHandler.Instance.OnDisconnect += (sender, args) =>
            {
                Disconnects++;
                OnPropertyChanged(nameof(Disconnects));
            };

            WoWEventHandler.Instance.OnDuelRequest += (sender, args) =>
            {
                var req = new DuelRequest(args.Player);
                _duelRequests.Add(req);
                OnPropertyChanged(nameof(DuelRequests));
                OnPropertyChanged(nameof(DuelRequestsCount));
            };

            WoWEventHandler.Instance.OnGuildInvite += (sender, args) =>
            {
                var invite = new GuildInvite(args.Player, args.Guild);
                _guildInvites.Add(invite);
                OnPropertyChanged(nameof(GuildInvites));
                OnPropertyChanged(nameof(GuildInvitesCount));
            };

            WoWEventHandler.Instance.OnUnitKilled += (sender, args) =>
            {
                UnitsKilled++;
                OnPropertyChanged(nameof(UnitsKilled));
            };

            WoWEventHandler.Instance.OnLoot += (sender, args) =>
            {
                var loot = new Loot(args.ItemName, args.ItemId, args.Count);
                _lootList.Add(loot);
                OnPropertyChanged(nameof(LootList));
                OnPropertyChanged(nameof(LootListCount));
            };

            WoWEventHandler.Instance.OnPartyInvite += (sender, args) =>
            {
                var party = new PartyInvite(args.Player);
                _partyInvites.Add(party);
                OnPropertyChanged(nameof(PartyInvites));
                OnPropertyChanged(nameof(PartyInvitesCount));
            };

            _runningTimer = new Timer(1000 * 5) {AutoReset = true};
            _runningTimer.Elapsed += (sender, args) =>
            {
                OnPropertyChanged(nameof(RunningSince));
                var player = ObjectManager.Instance.Player;
                if (player == null) return;
                ToonName = player.Name;
                var money = player.Money;
                if (_startCopperCount == null)
                    _startCopperCount = money;
                CopperGained = money - _startCopperCount.Value;
            };
            _runningTimer.Start();

            WoWEventHandler.Instance.OnTradeShow += (sender, args) =>
            {
                TradeRequests++;
                OnPropertyChanged(nameof(TradeRequests));
            };
        }

        public int CopperGained
        {
            get { return _copperGained; }
            set
            {
                _copperGained = value;
                OnPropertyChanged();
            }
        }

        public string ToonName
        {
            get { return _toonName; }
            set
            {
                _toonName = value;
                OnPropertyChanged();
            }
        }

        private int _runningSince => Environment.TickCount - _startTime;

        /// <summary>
        ///     Date since data is being collected
        /// </summary>
        /// <value>
        ///     The running since.
        /// </value>
        public int RunningSince => (int) Math.Round(_runningSince / (double) 1000 / 60);

        /// <summary>
        ///     Total XP gained since this instance is collecting data
        /// </summary>
        /// <value>
        ///     The xp gained.
        /// </value>
        public int XpGained { get; private set; }

        /// <summary>
        ///     XP per hour
        /// </summary>
        /// <value>
        ///     The xp hour.
        /// </value>
        public int XpHour => XpGained == 0 ? 0 : (int) Math.Round((double) XpGained / _runningSince * 3600000);

        /// <summary>
        ///     Level ups since this instance started collecting data
        /// </summary>
        /// <value>
        ///     The level ups.
        /// </value>
        public int LevelUps { get; private set; }

        /// <summary>
        ///     Deaths since this instance started collecting data
        /// </summary>
        /// <value>
        ///     The deaths.
        /// </value>
        public int Deaths { get; private set; }

        /// <summary>
        ///     Disconnects since this instance started collecting data
        /// </summary>
        /// <value>
        ///     The disconnects.
        /// </value>
        public int Disconnects { get; private set; }

        /// <summary>
        ///     Units killed since this instance started collecting data
        /// </summary>
        /// <value>
        ///     The units killed.
        /// </value>
        public int UnitsKilled { get; private set; }

        /// <summary>
        ///     Trade requests since this instance started collecting data
        /// </summary>
        /// <value>
        ///     The trade requests.
        /// </value>
        public int TradeRequests { get; private set; }

        /// <summary>
        ///     All guild invites since this instance is collecting data
        /// </summary>
        /// <value>
        ///     The guild invites.
        /// </value>
        public int GuildInvitesCount { get; private set; }

        public IReadOnlyList<GuildInvite> GuildInvites => _guildInvites.ToList();

        /// <summary>
        ///     All duel requests since this instance is collecting data
        /// </summary>
        /// <value>
        ///     The duel requests.
        /// </value>
        public int DuelRequestsCount { get; private set; }

        public IReadOnlyList<DuelRequest> DuelRequests => _duelRequests.ToList();

        /// <summary>
        ///     All looted items since this instance is collecting data
        /// </summary>
        /// <value>
        ///     The loot list.
        /// </value>
        public int LootListCount { get; private set; }

        public IReadOnlyList<Loot> LootList => _lootList.ToList();

        /// <summary>
        ///     All party invites since this instance is collecting data
        /// </summary>
        /// <value>
        ///     The party invites.
        /// </value>
        public int PartyInvitesCount { get; private set; }

        public IReadOnlyList<PartyInvite> PartyInvites => _partyInvites.ToList();

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_dispose)
            {
                if (disposing)
                    _runningTimer?.Dispose();
                _dispose = true;
            }
        }

        ~Statistics()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Struct representing a duel request
        /// </summary>
        public struct DuelRequest
        {
            internal DuelRequest(string parPlayer)
            {
                Player = parPlayer;
                Time = DateTime.Now;
            }

            /// <summary>
            ///     The players name
            /// </summary>
            public readonly string Player;

            /// <summary>
            ///     The time
            /// </summary>
            public readonly DateTime Time;
        }

        /// <summary>
        ///     Struct representing a party invite
        /// </summary>
        public struct PartyInvite
        {
            internal PartyInvite(string parPlayer)
            {
                Player = parPlayer;
                Time = DateTime.Now;
            }

            /// <summary>
            ///     The players name
            /// </summary>
            public readonly string Player;

            /// <summary>
            ///     The time
            /// </summary>
            public readonly DateTime Time;
        }

        /// <summary>
        ///     Struct representing a guild invite
        /// </summary>
        public struct GuildInvite
        {
            internal GuildInvite(string parPlayer, string parGuild)
            {
                Player = parPlayer;
                Guild = parGuild;
                Time = DateTime.Now;
            }

            /// <summary>
            ///     The players name
            /// </summary>
            public readonly string Player;

            /// <summary>
            ///     The guilds name
            /// </summary>
            public readonly string Guild;

            /// <summary>
            ///     The time
            /// </summary>
            public readonly DateTime Time;
        }

        /// <summary>
        ///     Struct representing a looted item
        /// </summary>
        public struct Loot
        {
            internal Loot(string parName, int parId, int parCount)
            {
                Name = parName;
                Id = parId;
                Count = parCount;
                Time = DateTime.Now;
            }

            /// <summary>
            ///     The items name
            /// </summary>
            public readonly string Name;

            /// <summary>
            ///     The items ID
            /// </summary>
            public readonly int Id;

            /// <summary>
            ///     The items count
            /// </summary>
            public readonly int Count;

            /// <summary>
            ///     The time
            /// </summary>
            public readonly DateTime Time;
        }
    }
}