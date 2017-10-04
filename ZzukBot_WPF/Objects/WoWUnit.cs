using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZzukBot.Constants;
using ZzukBot.ExtensionMethods;
using ZzukBot.Game.Statics;
using ZzukBot.Mem;

namespace ZzukBot.Objects
{
    /// <summary>
    ///     Represents a unit (NPC or player)
    /// </summary>
    public class WoWUnit : WoWObject
    {
        /// <summary>
        ///     Constructor taking guid aswell Ptr to object
        /// </summary>
        internal WoWUnit(ulong parGuid, IntPtr parPointer, Enums.WoWObjectTypes parType)
            : base(parGuid, parPointer, parType)
        {
        }

        /// <summary>
        /// The quest state of the unit
        /// </summary>
        public Enums.NpcQuestOfferState QuestState => ReadRelative<Enums.NpcQuestOfferState>(0xCB8);

        /// <summary>
        /// UnitFlags of the NPC (Gossip, Flightmaster, Auctionator etc.)
        /// </summary>
        public Enums.NpcFlags NpcFlags => GetDescriptor<Enums.NpcFlags>(0x24C);

        /// <summary>
        /// Determines if the unit is on taxi (gryphon)
        /// </summary>
        public bool IsOnTaxi => (Pointer.Add(0x110).ReadAs<IntPtr>().Add(0xA0).ReadAs<int>() >> 0x14) != 0;

        /// <summary>
        ///     Position (will be relative to the transport the unit is on)
        /// </summary>
        public override Location Position
        {
            get
            {
                try
                {
                    var X = ReadRelative<float>(Offsets.Unit.PosX);
                    var Y = ReadRelative<float>(Offsets.Unit.PosY);
                    var Z = ReadRelative<float>(Offsets.Unit.PosZ);
                    return new Location(X, Y, Z);
                }
                catch
                {
                    return new Location(0, 0, 0);
                }
                
            }
        }

        /// <summary>
        /// Check if the unit is dead (health == 0)
        /// </summary>
        public virtual bool IsDead => Health == 0;

        /// <summary>
        /// The required skinning level calculated from the units level. Attention: This number doesnt mean you can actually skin the unit
        /// </summary>
        public int RequiredSkinningLevel
        {
            get
            {
                var level = Level;
                level = level > 60 ? 60 : level;
                if (level > 10)
                {
                    if (level > 20)
                    {
                        return 5 * level;
                    }
                    return 2 * (5 * level - 50);
                }
                return 1;
            }
        }

        /// <summary>
        /// Check if the unit is silenced
        /// </summary>
        public bool IsSilenced => (Flags & Enums.UnitFlags.UNIT_FLAG_SILENCED) == Enums.UnitFlags.UNIT_FLAG_SILENCED;

        /// <summary>
        /// Transport Guid the unit is on
        /// </summary>
        public ulong TransportGuid => Pointer.Add(0x118).ReadAs<IntPtr>().Add(0x38).ReadAs<ulong>();

        /// <summary>
        /// Transport the unit is on
        /// </summary>
        public WoWGameObject CurrentTransport
        {
            get
            {
                var guid = TransportGuid;
                return guid == 0 ? null : ObjectManager.Instance.GameObjects.FirstOrDefault(x => x.Guid == guid);
            }
        }

        /// <summary>
        ///     Distance to our character
        /// </summary>
        public float DistanceToPlayer => Position.GetDistanceTo(ObjectManager.Instance.Player.Position);

        /// <summary>
        ///     All auras on unit by ID
        /// </summary>
        public List<int> Auras
        {
            get
            {
                var tmpAuras = new List<int>();
                var auraBase = Offsets.Unit.AuraBase;
                var curCount = 0;
                while (true)
                {
                    var auraId = GetDescriptor<int>(auraBase);
                    if (curCount == 10) break;
                    if (auraId != 0)
                        tmpAuras.Add(auraId);
                    auraBase += 4;
                    curCount++;
                }
                return tmpAuras;
            }
        }

        /// <summary>
        ///     All debuffs on unit by ID
        /// </summary>
        public List<int> Debuffs
        {
            get
            {
                var tmpAuras = new List<int>();
                var auraBase = 0x13C;
                var curCount = 0;
                while (true)
                {
                    var auraId = GetDescriptor<int>(auraBase);
                    if (curCount == 16) break;
                    if (auraId != 0)
                        tmpAuras.Add(auraId);
                    auraBase += 4;
                    curCount++;
                }
                return tmpAuras;
            }
        }

        /// <summary>
        ///     Name of NPC / Player
        /// </summary>
        public override string Name
        {
            get
            {
                try
                {
                    switch (WoWType)
                    {
                        case Enums.WoWObjectTypes.OT_UNIT:
                            return UnitName;

                        case Enums.WoWObjectTypes.OT_PLAYER:
                            return PlayerName;
                    }
                }
                catch
                {
                    // ignored
                }
                return "";
            }
        }

        private string UnitName
        {
            get
            {
                var ptr1 = ReadRelative<IntPtr>(Offsets.Unit.NameBase);
                if (ptr1 == IntPtr.Zero) return "";
                var ptr2 = Memory.Reader.Read<IntPtr>(ptr1);
                if (ptr2 == IntPtr.Zero) return "";
                return ptr2.ReadString();
            }
        }

        private string PlayerName
        {
            get
            {
                var nameBasePtr = Memory.Reader.Read<IntPtr>(Offsets.PlayerObject.NameBase);
                while (true)
                {
                    var nextGuid =
                        Memory.Reader.Read<ulong>(IntPtr.Add(nameBasePtr, Offsets.PlayerObject.NameBaseNextGuid));
                    if (nextGuid == 0)
                        return "";
                    if (nextGuid != Guid)
                        nameBasePtr = Memory.Reader.Read<IntPtr>(nameBasePtr);
                    else
                        break;
                }
                return nameBasePtr.Add(0x14).ReadString();
            }
        }

        /// <summary>
        ///     0 or the GUID of the object that summoned the creature
        /// </summary>
        public ulong SummonedBy => GetDescriptor<ulong>(Offsets.Descriptors.SummonedByGuid);

        /// <summary>
        ///     NPC ID
        /// </summary>
        public int Id => ReadRelative<int>(Offsets.Descriptors.NpcId);

        /// <summary>
        ///     The faction Id
        /// </summary>
        public int FactionId => GetDescriptor<int>(Offsets.Descriptors.FactionId);

        //internal int FactionID => GetDescriptor<int>(Offsets.Descriptors.FactionId);

        /// <summary>
        ///     The movement state of the Unit
        /// </summary>
        public virtual Enums.MovementFlags MovementState => ReadRelative<Enums.MovementFlags>(Offsets.Descriptors.MovementFlags);

        /// <summary>
        ///     Dynamic Flags of the Unit (Is lootable / Is tapped?)
        /// </summary>
        public int DynamicFlags => GetDescriptor<int>(Offsets.Descriptors.DynamicFlags);

        /// <summary>
        ///     Units flags
        /// </summary>
        public Enums.UnitFlags Flags => GetDescriptor<Enums.UnitFlags>(Offsets.Descriptors.Flags);

        /// <summary>
        ///     Gets a value indicating whether this unit is fleeing
        /// </summary>
        public bool IsFleeing
        {
            get
            {
                var flag = Enums.UnitFlags.UNIT_FLAG_FLEEING;
                return (Flags & flag) ==
                       flag;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this unit is confused (Scatter Shot etc.)
        /// </summary>
        public bool IsConfused
        {
            get
            {
                var flag = Enums.UnitFlags.UNIT_FLAG_CONFUSED;
                return (Flags & flag) ==
                       flag;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this unit is in combat.
        /// </summary>
        public bool IsInCombat
        {
            get
            {
                var flag = Enums.UnitFlags.UNIT_FLAG_IN_COMBAT;
                return (Flags & flag) ==
                       flag;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this unit is skinable.
        /// </summary>
        public bool IsSkinable
        {
            get
            {
                var flag = Enums.UnitFlags.UNIT_FLAG_SKINNABLE;
                return (Flags & flag) ==
                       flag;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this unit is stunned.
        /// </summary>
        public bool IsStunned
        {
            get
            {
                var flag = Enums.UnitFlags.UNIT_FLAG_STUNNED;
                return (Flags & flag) ==
                       flag;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this unit got its movement disabled.
        /// </summary>
        public bool IsMovementDisabled
        {
            get
            {
                var flag = Enums.UnitFlags.UNIT_FLAG_DISABLE_MOVE;
                return (Flags & flag) ==
                       flag;
            }
        }

        internal bool IsCrowdControlled => IsStunned | IsFleeing | IsConfused;

        /// <summary>
        ///     Tells if the unit is tapped by me (gives XP)
        /// </summary>
        public bool TappedByMe => DynamicFlags >= PrivateEnums.DynamicFlags.TappedByMe &&
                                  DynamicFlags <= PrivateEnums.DynamicFlags.TappedByMe + 0x2;

        /// <summary>
        ///     Tells if the unit is tapped by someone else (grey portrait)
        /// </summary>
        public bool TappedByOther => DynamicFlags >= PrivateEnums.DynamicFlags.TappedByOther &&
                                     DynamicFlags <= PrivateEnums.DynamicFlags.TappedByOther + 2;

        /// <summary>
        ///     Tells if the unit is tapped by no one
        /// </summary>
        public bool IsUntouched => DynamicFlags == PrivateEnums.DynamicFlags.Untouched;

        /// <summary>
        ///     Tells if the unit is hunter marked
        /// </summary>
        public bool IsMarked => DynamicFlags == PrivateEnums.DynamicFlags.IsMarked;

        /// <summary>
        ///     Tells if the unit can be looted
        /// </summary>
        public bool CanBeLooted
        {
            get
            {
                if (Health == 0)
                    return (DynamicFlags & 1) != 0;
                return false;
            }
        }

        /// <summary>
        ///     Health
        /// </summary>
        public int Health
        {
            get
            {
                var ret = int.MaxValue;
                try
                {
                    ret = GetDescriptor<int>(Offsets.Descriptors.Health);
                }
                catch
                {
                    // ignored
                }
                return ret;
            }
        } 

        /// <summary>
        ///     Max health
        /// </summary>
        public int MaxHealth => GetDescriptor<int>(Offsets.Descriptors.MaxHealth);

        /// <summary>
        ///     health percent.
        /// </summary>
        public int HealthPercent => (int) (Health / (float) MaxHealth * 100);

        /// <summary>
        ///     Mana
        /// </summary>
        public int Mana => GetDescriptor<int>(Offsets.Descriptors.Mana);

        /// <summary>
        ///     maximum mana.
        /// </summary>
        public int MaxMana => GetDescriptor<int>(Offsets.Descriptors.MaxMana);

        /// <summary>
        ///     mana percent.
        /// </summary>
        public int ManaPercent => (int) (Mana / (float) MaxMana * 100);

        /// <summary>
        ///     Rage
        /// </summary>
        public int Rage => GetDescriptor<int>(Offsets.Descriptors.Rage) / 10;

        /// <summary>
        ///     Energy
        /// </summary>
        public int Energy => GetDescriptor<int>(Offsets.Descriptors.Energy);

        /// <summary>
        ///     Guid of the units target
        /// </summary>
        public ulong TargetGuid
        {
            get { return GetDescriptor<ulong>(Offsets.Descriptors.TargetGuid); }
            set { SetDescriptor(Offsets.Descriptors.TargetGuid, value); }
        }

        /// <summary>
        ///     Id of the spell the unit is casting currently
        /// </summary>
        public virtual int Casting => ReadRelative<int>(0xC8C);

        /// <summary>
        ///     Id of the spell the unit is channeling currently
        /// </summary>
        public int Channeling => GetDescriptor<int>(Offsets.Descriptors.IsChanneling);

        /// <summary>
        ///     Units reaction to the player
        /// </summary>
        public virtual Enums.UnitReaction Reaction
            => Functions.UnitReaction(Pointer, ObjectManager.Instance.Player.Pointer);

        /// <summary>
        ///     Is player?
        /// </summary>
        public bool IsPlayer => WoWType == Enums.WoWObjectTypes.OT_PLAYER;

        /// <summary>
        ///     Is Npc?
        /// </summary>
        public bool IsMob => WoWType == Enums.WoWObjectTypes.OT_UNIT;


        /// <summary>
        ///     Facing of the unit
        /// </summary>
        public new float Facing
        {
            get
            {
                var pi2 = (float) (2 * Math.PI);
                var facing = ReadRelative<float>(0x9C4);
                //if (facing >= pi2)
                //    facing -= pi2;
                //else if (facing < 0)
                //{
                //    facing = facing + pi2;
                //}
                return facing;
            }
        }

        /// <summary>
        ///     Is critter?
        /// </summary>
        public bool IsCritter => Enums.CreatureType.Critter == CreatureType;

        /// <summary>
        ///     Is unit a totem?
        /// </summary>
        public bool IsTotem => Enums.CreatureType.Totem == CreatureType;


        /// <summary>
        ///     Unit rank (rare, elite, normal etc.)
        /// </summary>
        public Enums.CreatureRankTypes CreatureRank => (Enums.CreatureRankTypes) Functions.GetCreatureRank(Pointer);

        /// <summary>
        ///     The type of the unit
        /// </summary>
        public Enums.CreatureType CreatureType => Functions.GetCreatureType(Pointer);

        /// <summary>
        ///     Units level
        /// </summary>
        public int Level => GetDescriptor<int>(Offsets.Descriptors.Level);

        /// <summary>
        ///     Is the unit mounted?
        /// </summary>
        public bool IsMounted => GetDescriptor<int>(Offsets.Descriptors.MountDisplayId) != 0;

        /// <summary>
        ///     Is the unit a pet?
        /// </summary>
        public bool IsPet => SummonedBy != 0;

        /// <summary>
        ///     Is the unit pet of a player?
        /// </summary>
        public bool IsPlayerPet
        {
            get
            {
                var tmpGuid = SummonedBy;
                if (tmpGuid == 0) return false;
                var obj = ObjectManager.Instance.Players.FirstOrDefault(i => i.Guid == tmpGuid);
                return obj != null;
            }
        }

        /// <summary>
        ///     Is the unit swimming?
        /// </summary>
        public bool IsSwimming
            =>
                (MovementState & Enums.MovementFlags.Swimming) ==
                Enums.MovementFlags.Swimming;


        /// <summary>
        ///     Interacts with the unit
        /// </summary>
        /// <param name="parAutoLoot">Shift</param>
        public void Interact(bool parAutoLoot)
        {
            Functions.OnRightClickUnit(Pointer, Convert.ToInt32(parAutoLoot));
        }

        /// <summary>
        ///     Facing relative to another object
        /// </summary>
        /// <param name="parObject">The object</param>
        /// <returns></returns>
        public float FacingRelativeTo(WoWObject parObject)
        {
            return FacingRelativeTo(parObject.Position);
        }

        /// <summary>
        ///     Facing relative to another object
        /// </summary>
        /// <param name="parPosition">The position.</param>
        /// <returns></returns>
        public float FacingRelativeTo(Location parPosition)
        {
            return (float) Math.Round(Math.Abs(RequiredFacing(parPosition) - Facing), 2);
        }

        /// <summary>
        ///     Required facing to look at the unit
        /// </summary>
        /// <param name="parObject">The object.</param>
        /// <returns></returns>
        public float RequiredFacing(WoWObject parObject)
        {
            return RequiredFacing(parObject.Position);
        }

        /// <summary>
        ///     Required facing to look at the position
        /// </summary>
        /// <param name="parPosition">The position.</param>
        /// <returns></returns>
        public float RequiredFacing(Location parPosition)
        {
            var f = (float) Math.Atan2(parPosition.Y - Position.Y, parPosition.X - Position.X);
            if (f < 0.0f)
            {
                f = f + (float) Math.PI * 2.0f;
            }
            else
            {
                if (f > (float) Math.PI * 2)
                    f = f - (float) Math.PI * 2.0f;
            }
            return f;
        }


        /// <summary>
        ///     Check whether the unit got an aura or not
        /// </summary>
        /// <param name="parName">Name</param>
        /// <returns></returns>
        public bool GotAura(string parName)
        {
            var tmpAuras = Auras;
            return
                tmpAuras.Select(
                    i =>
                        string.Equals(Spell.Instance.GetName(i), parName,
                            StringComparison.OrdinalIgnoreCase)).Any(tmpBool => tmpBool);
        }

        /// <summary>
        ///     Check whether the unit got an debuff or not
        /// </summary>
        /// <param name="parName">Name</param>
        /// <returns></returns>
        public bool GotDebuff(string parName)
        {
            var tmpAuras = Debuffs;
            return
                tmpAuras.Select(
                    i =>
                        string.Equals(Spell.Instance.GetName(i), parName,
                            StringComparison.OrdinalIgnoreCase)).Any(tmpBool => tmpBool);
        }

        /// <summary>
        ///     Tells if using aoe will engage the character with other units that arent fighting right now
        /// </summary>
        /// <param name="parRange">The radius around the unit</param>
        /// <returns>
        ///     Returns <c>true</c> if we can use AoE without engaging other unpulled units
        /// </returns>
        public bool IsAoeSafe(int parRange)
        {
            var mobs = ObjectManager.Instance.Npcs.
                FindAll(i => (i.Reaction == Enums.UnitReaction.Hostile || i.Reaction == Enums.UnitReaction.Neutral) &&
                             i.DistanceToPlayer < parRange).ToList();

            foreach (var mob in mobs)
                if (mob.TargetGuid != ObjectManager.Instance.Player.Guid)
                    return false;
            return true;
        }

        /// <summary>
        /// Checks to see if the unit is in the specified range.
        /// 
        /// If target is passed, checks to see if the unit is in range to the given target.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool InRange(float range = 5, WoWUnit target = null)
        {
            return (target == null ? DistanceToPlayer : Position.GetDistanceTo(target.Position)) < range;
        }
    }
}