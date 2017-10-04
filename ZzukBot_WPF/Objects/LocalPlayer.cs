using System;
using System.Linq;
using ZzukBot.Constants;
using ZzukBot.ExtensionMethods;
using ZzukBot.Game.Statics;
using ZzukBot.Helpers;
using ZzukBot.Mem;

namespace ZzukBot.Objects
{
    /// <summary>
    ///     Class for the local player
    /// </summary>
    public class LocalPlayer : WoWUnit
    {
        /// <summary>
        ///     facing with coordinates instead of a passed unit
        /// </summary>
        private const float facingComparer = 0.2f;

        /// <summary>
        /// Let the toon jump
        /// </summary>
        public void Jump()
        {
            Lua.Instance.Execute("Jump()");
        }

        /// <summary>
        /// Retrieves the transport we are currently on (ship, ceppelin, elevator etc.)
        /// </summary>
        //public WoWGameObject CurrentTransport
        //{
        //    get
        //    {
        //        if (!ObjectManager.Instance.IsIngame) return null;
        //        var transportGuid = 0x00C7B608.ReadAs<UInt64>();
        //        if (transportGuid == 0) return null;
        //        var transport = ObjectManager.Instance.GameObjects.FirstOrDefault(x => x.Guid == transportGuid);
        //        return transport;
        //    }
        //}

        /// <summary>
        ///     Constructor
        /// </summary>
        internal LocalPlayer(ulong parGuid, IntPtr parPointer, Enums.WoWObjectTypes parType)
            : base(parGuid, parPointer, parType)
        {
        }

        /// <summary>
        ///     Position of the characters corpse
        /// </summary>
        public Location CorpsePosition => new Location(
            Memory.Reader.Read<float>(Offsets.Player.CorpsePositionX),
            Memory.Reader.Read<float>(Offsets.Player.CorpsePositionY),
            Memory.Reader.Read<float>(Offsets.Player.CorpsePositionZ));

        internal float CtmX => Memory.Reader.Read<float>(Offsets.Player.CtmX);

        /// <summary>
        ///     Always friendly for local player
        /// </summary>
        public override Enums.UnitReaction Reaction => Enums.UnitReaction.Friendly;

        internal float CtmY => Memory.Reader.Read<float>(Offsets.Player.CtmY);

        internal float CtmZ => Memory.Reader.Read<float>(Offsets.Player.CtmZ);

        /// <summary>
        ///     The current spell by ID we are casting (0 or spell ID)
        /// </summary>
        /// <value>
        ///     The casting.
        /// </value>
        public override int Casting
        {
            get
            {
                var tmpId = base.Casting;
                var tmpName = Spell.Instance.GetName(tmpId);
                if (tmpName == "Heroic Strike" || tmpName == "Maul")
                    return 0;
                return tmpId;
            }
        }

        /// <summary>
        ///     Current spell casted by name
        /// </summary>
        /// <value>
        ///     The name of the casting as.
        /// </value>
        public string CastingAsName => Spell.Instance.GetName(Casting);


        /// <summary>
        ///     true if no click to move action is taking place
        /// </summary>
        /// <value>
        /// </value>
        public bool IsCtmIdle => CtmState == (int) PrivateEnums.CtmType.None ||
                                 CtmState == 12;

        /// <summary>
        ///     Characters money in copper
        /// </summary>
        /// <value>
        ///     The money.
        /// </value>
        public int Money => ReadRelative<int>(0x2FD0);

        /// <summary>
        ///     Get or set the current ctm state
        /// </summary>
        private int CtmState
        {
            get
            {
                return
                    Memory.Reader.Read<int>(Offsets.Player.CtmState);
            }

            set { Memory.Reader.Write(Offsets.Player.CtmState, value); }
        }

        /// <summary>
        ///     Characters movement state
        /// </summary>
        /// <value>
        ///     The state of the movement.
        /// </value>
        public new Enums.MovementFlags MovementState => ReadRelative<Enums.MovementFlags>(Offsets.Descriptors.MovementFlags);

        /// <summary>
        ///     Determine if the character is inside a campfire
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is in campfire; otherwise, <c>false</c>.
        /// </value>
        public bool IsInCampfire
        {
            get
            {
                var playerPos = ObjectManager.Instance.Player.Position;
                var tmp = ObjectManager.Instance.GameObjects
                    .FirstOrDefault(i => i.Name == "Campfire" && i.Position.GetDistanceTo(playerPos) <= 2.9f);
                return tmp != null;
            }
        }

        /// <summary>
        ///     the ID of the map we are on
        /// </summary>
        public uint MapId => Memory.Reader.Read<uint>(
            IntPtr.Add(
                Memory.Reader.Read<IntPtr>(Offsets.ObjectManager.ManagerBase), 0xCC));

        ///// <summary>
        ///// Are we in LoS with object?
        ///// </summary>
        //internal bool InLoSWith(WoWObject parObject)
        //{
        //    // return 1 if something is inbetween the two coordinates
        //    return Functions.Intersect(Position, parObject.Position) == 0;
        //}

        /// <summary>
        ///     Is our character in CC?
        /// </summary>
        /// <value>
        /// </value>
        public bool IsInCC => 0 == Memory.Reader.Read<int>(Offsets.Player.IsInCC);

        private ulong ComboPointGuid { get; set; }

        /// <summary>
        ///     Get combopoints for current mob
        /// </summary>
        public byte ComboPoints
        {
            get
            {
                var ptr1 = ReadRelative<IntPtr>(Offsets.Player.ComboPoints1);
                var ptr2 = IntPtr.Add(ptr1, Offsets.Player.ComboPoints2);
                if (ComboPointGuid == 0)
                    Memory.Reader.Write(ptr2, 0);
                var points = Memory.Reader.Read<byte>(ptr2);
                if (points == 0)
                {
                    ComboPointGuid = TargetGuid;
                    return points;
                }
                if (ComboPointGuid != TargetGuid)
                {
                    Memory.Reader.Write<byte>(ptr2, 0);
                    return 0;
                }
                return Memory.Reader.Read<byte>(ptr2);
            }
        }

        /// <summary>
        /// Will retrieve the corpse of the player when in range
        /// </summary>
        public void RetrieveCorpse()
        {
            Functions.RetrieveCorpse();
        }

        /// <summary>
        /// Will release the spirit if the player is dead
        /// </summary>
        public void RepopMe()
        {
            Functions.RepopMe();
        }

        /// <summary>
        ///     Tells if our character can overpower
        /// </summary>
        public bool CanOverpower => ComboPoints > 0;


        /// <summary>
        ///     Tells if the character got a pet
        /// </summary>
        public bool HasPet => ObjectManager.Instance.Pet != null;

        /// <summary>
        ///     Tells if the character is eating
        /// </summary>
        public bool IsEating => GotAura("Food");

        /// <summary>
        ///     Tells if the character is drinking
        /// </summary>
        public bool IsDrinking => GotAura("Drink");

        /// <summary>
        ///     The characters class
        /// </summary>
        public Enums.ClassId Class => (Enums.ClassId) Memory.Reader.Read<byte>(Offsets.Player.Class);

        /// <summary>
        ///     Tells if the character is stealthed
        /// </summary>
        public bool IsStealth
        {
            get
            {
                switch (Class)
                {
                    case Enums.ClassId.Rogue:
                    case Enums.ClassId.Druid:
                        return (PlayerBytes & 0x02000000) == 0x02000000;
                }
                return false;
            }
        }

        /// <summary>
        ///     The player bytes
        /// </summary>
        public uint PlayerBytes => GetDescriptor<uint>(0x228);

        /// <summary>
        ///     Characters race
        /// </summary>
        public string Race
        {
            get
            {
                const string getUnitRace = "{0} = UnitRace('player')";
                var result = Lua.Instance.ExecuteWithResult(getUnitRace);
                return result[0];
            }
        }

        /// <summary>
        ///     Are we in ghost form
        /// </summary>
        public bool InGhostForm => Memory.Reader.Read<byte>(Offsets.Player.IsGhost) == 1;

        internal float ZAxis
        {
            set { Memory.Reader.Write(IntPtr.Add(Pointer, Offsets.Unit.PosZ), value); }
            get { return ReadRelative<float>((int) IntPtr.Add(Pointer, Offsets.Unit.PosZ)); }
        }

        /// <summary>
        ///     Time until we can accept a resurrect
        /// </summary>
        public int TimeUntilResurrect
        {
            get
            {
                var result = Lua.Instance.ExecuteWithResult("{0} = GetCorpseRecoveryDelay()");
                return Convert.ToInt32(result[0]);
            }
        }

        /// <summary>
        ///     XP gained into current level
        /// </summary>
        public int CurrentXp => GetDescriptor<int>(Offsets.Descriptors.CurrentXp);

        /// <summary>
        ///     XP needed for the whole level
        /// </summary>
        public int NextLevelXp => GetDescriptor<int>(Offsets.Descriptors.NextLevelXp);

        /// <summary>
        ///     Zone text
        /// </summary>
        public string RealZoneText => 0xB4B404.PointsTo().ReadString();

        /// <summary>
        ///     Continent text
        /// </summary>
        public string ContinentText => Offsets.Player.ContinentText.ReadString();

        /// <summary>
        ///     Minimap text
        /// </summary>
        public string MinimapZoneText
            => Offsets.Player.MinimapZoneText.ReadString();

        /// <summary>
        ///     Guid of the unit the Merchant Frame belongs to (can be 0)
        /// </summary>
        public ulong VendorGuid => 0x00BDDFA0.ReadAs<ulong>();

        /// <summary>
        ///     Guid of the unit the Quest Frame belongs to (can be 0)
        /// </summary>
        public ulong QuestNpcGuid => 0x00BE0810.ReadAs<ulong>();

        /// <summary>
        ///     Guid of the unit the Gossip Frame belongs to (can be 0)
        /// </summary>
        public ulong GossipNpcGuid => 0x00BC3F58.ReadAs<ulong>();

        /// <summary>
        ///     Guid of the unit our character is looting right now
        /// </summary>
        public ulong CurrentLootGuid => (Pointer + 0x1D28).ReadAs<ulong>();


        internal void TurnOnSelfCast()
        {
            const string turnOnSelfCast = "SetCVar('autoSelfCast',1)";
            Lua.Instance.Execute(turnOnSelfCast);
        }


        /// <summary>
        ///     Starts a movement
        /// </summary>
        /// <param name="parBits">The movement bits</param>
        public void StartMovement(Enums.ControlBits parBits)
        {
            MainThread.Instance.Invoke(() =>
            {
                if (parBits != Enums.ControlBits.Nothing)
                {
                    var movementState = MovementState;
                    if (parBits.HasFlag(Enums.ControlBits.Front) && movementState.HasFlag(Enums.MovementFlags.Back))
                        StopMovement(Enums.ControlBits.Back);

                    if (parBits.HasFlag(Enums.ControlBits.Back) && movementState.HasFlag(Enums.MovementFlags.Front))
                        StopMovement(Enums.ControlBits.Front);

                    if (parBits.HasFlag(Enums.ControlBits.Left) && movementState.HasFlag(Enums.MovementFlags.Right))
                        StopMovement(Enums.ControlBits.Right);

                    if (parBits.HasFlag(Enums.ControlBits.Right) && movementState.HasFlag(Enums.MovementFlags.Left))
                        StopMovement(Enums.ControlBits.Left);

                    if (parBits.HasFlag(Enums.ControlBits.StrafeLeft) && movementState.HasFlag(Enums.MovementFlags.StrafeRight))
                        StopMovement(Enums.ControlBits.StrafeRight);

                    if (parBits.HasFlag(Enums.ControlBits.StrafeRight) && movementState.HasFlag(Enums.MovementFlags.StrafeLeft))
                        StopMovement(Enums.ControlBits.StrafeLeft);

                }
                Console.WriteLine("Starting movement");
                Functions.SetControlBit((int)parBits, 1, Environment.TickCount);
            });
        }

        /// <summary>
        ///     Stops movement
        /// </summary>
        /// <param name="parBits">The movement bits</param>
        public void StopMovement(Enums.ControlBits parBits)
        {
            Functions.SetControlBit((int) parBits, 0, Environment.TickCount);
        }

        /// <summary>
        ///     Start a ctm movement
        /// </summary>
        /// <param name="parPosition">The position.</param>
        public void CtmTo(Location parPosition)
        {
            //float disX = Math.Abs(this.CtmX - parPosition.X);
            //float disY = Math.Abs(this.CtmY - parPosition.Y);
            //if (disX < 0.2f && disY < 0.2f) return;
            Functions.Ctm(Pointer, PrivateEnums.CtmType.Move, parPosition, 0);
            //SendMovementUpdate((int)Enums.MovementOpCodes.setFacing);
            WoWEventHandler.Instance.TriggerCtmEvent(new WoWEventHandler.OnCtmArgs(parPosition,
                (int) PrivateEnums.CtmType.Move));
        }

        /// <summary>
        ///     Stop the current ctm movement
        /// </summary>
        public void CtmStopMovement()
        {
            if (CtmState != (int) PrivateEnums.CtmType.None &&
                CtmState != 12) //&& CtmState != (int)Enums.CtmType.Face)
            {
                var pos = ObjectManager.Instance.Player.Position;
                Functions.Ctm(Pointer, PrivateEnums.CtmType.None, pos, 0);
                WoWEventHandler.Instance.TriggerCtmEvent(new WoWEventHandler.OnCtmArgs(pos,
                    (int) PrivateEnums.CtmType.None));
            }
            else if ((CtmState == 12 || CtmState == (int) PrivateEnums.CtmType.None) &&
                     ObjectManager.Instance.Player.MovementState != 0)
            {
                var tmp =
                    Enum.GetValues(typeof(Enums.ControlBits))
                        .Cast<Enums.ControlBits>()
                        .Aggregate(Enums.ControlBits.Nothing, (current, bits) => current | bits);
                ObjectManager.Instance.Player.StopMovement(tmp);
            }
        }

        /// <summary>
        ///     Set CTM to idle (wont stop movement however)
        /// </summary>
        public void CtmSetToIdle()
        {
            if (CtmState != 12)
                CtmState = 12;
        }

        /// <summary>
        ///     Enables CTM.
        /// </summary>
        public void EnableCtm()
        {
            const string ctmOn = "ConsoleExec('Autointeract 1')";
            Lua.Instance.Execute(ctmOn);
        }

        /// <summary>
        ///     Disables CTM.
        /// </summary>
        public void DisableCtm()
        {
            const string ctmOff = "ConsoleExec('Autointeract 0')";
            Lua.Instance.Execute(ctmOff);
        }

        /// <summary>
        ///     Gets the latency.
        /// </summary>
        /// <returns></returns>
        public int GetLatency()
        {
            const string getLatency = "_, _, {0} = GetNetStats()";
            var result = Lua.Instance.ExecuteWithResult(getLatency);
            return Convert.ToInt32(result[0]);
        }

        /// <summary>
        ///     Simulate rightclick on a unit
        /// </summary>
        /// <param name="parUnit">The unit.</param>
        public void RightClick(WoWUnit parUnit)
        {
            Functions.OnRightClickUnit(parUnit.Pointer, 1);
        }

        /// <summary>
        ///     Simulate rightclick on a unit
        /// </summary>
        /// <param name="parUnit">The unit.</param>
        /// <param name="parAuto">Send shift</param>
        internal void RightClick(WoWUnit parUnit, bool parAuto)
        {
            var type = 0;
            if (parAuto) type = 1;
            Functions.OnRightClickUnit(parUnit.Pointer, type);
        }


        /// <summary>
        ///     CTM face an WoWObject
        /// </summary>
        /// <param name="parObject">The Object.</param>
        public void CtmFace(WoWObject parObject)
        {
            var tmp = parObject.Position;
            Functions.Ctm(Pointer, PrivateEnums.CtmType.FaceTarget,
                tmp, parObject.Guid);
        }

        /// <summary>
        ///     Check if we are in line of sight with an object
        /// </summary>
        /// <param name="parObject">The object.</param>
        /// <returns></returns>
        public bool InLosWith(WoWObject parObject)
        {
            return InLosWith(parObject.Position);
        }

        /// <summary>
        ///     Check if we are in line of sight with an object
        /// </summary>
        /// <param name="parPosition">The position.</param>
        /// <returns></returns>
        public bool InLosWith(Location parPosition)
        {
            var i = Functions.Intersect(Position, parPosition);
            return i.R == 0 && i.X == 0 && i.Y == 0 && i.Z == 0;
        }

        /// <summary>
        ///     Set Facing towards passed object
        /// </summary>
        /// <param name="parObject">The object.</param>
        public void Face(WoWObject parObject)
        {
            //Location xyz = new Location(parObject.Position.X, parObject.Position.Y, parObject.Position.Z);
            //Functions.Ctm(this.Pointer, Enums.CtmType.FaceTarget, xyz, parObject.Guid);
            Face(parObject.Position);
        }

        /// <summary>
        ///     Set facing towards a position
        /// </summary>
        /// <param name="parPosition">The position.</param>
        public void Face(Location parPosition)
        {
            if (IsFacing(parPosition)) return;
            Functions.SetFacing(IntPtr.Add(Pointer, Offsets.Player.MovementStruct), RequiredFacing(parPosition));
            SendMovementUpdate((int) PrivateEnums.MovementOpCodes.setFacing);
        }

        /// <summary>
        /// Set facing to value
        /// </summary>
        /// <param name="facing"></param>
        public void Face(float facing)
        {
            Functions.SetFacing(IntPtr.Add(Pointer, Offsets.Player.MovementStruct), facing);
            SendMovementUpdate((int)PrivateEnums.MovementOpCodes.setFacing);
        }

        /// <summary>
        ///     Determines if we are facing a position
        /// </summary>
        /// <param name="parCoordinates">The coordinates.</param>
        /// <returns></returns>
        public bool IsFacing(Location parCoordinates)
        {
            return FacingRelativeTo(parCoordinates) < facingComparer;
        }

        internal void SendMovementUpdate(int parOpCode)
        {
            Functions.SendMovementUpdate(Pointer, Environment.TickCount, parOpCode);
        }

        /// <summary>
        ///     Sets the last hardware action to the current tickcount
        /// </summary>
        public void AntiAfk()
        {
            Memory.Reader.Write(Offsets.Functions.LastHardwareAction, Environment.TickCount);
        }

        /// <summary>
        ///     Sets the target
        /// </summary>
        /// <param name="parObject">The object.</param>
        public void SetTarget(WoWObject parObject)
        {
            if (parObject == null)
            {
                SetTarget(0);
                return;
            }
            SetTarget(parObject.Guid);
        }

        /// <summary>
        ///     Sets the target.
        /// </summary>
        /// <param name="parGuid">The targets guid</param>
        public void SetTarget(ulong parGuid)
        {
            Functions.SetTarget(parGuid);
            TargetGuid = parGuid;
        }

        internal void RefreshSpells()
        {
            Spell.UpdateSpellbook();
        }

        #region Added Custom Class Functions

        /// <summary>
        ///     Tells if we are to close to utilise ranged physical attacks (Auto Shot etc).
        /// </summary>
        /// <value>
        ///     <c>true</c> if we are too close.
        /// </value>
        public bool ToCloseForRanged => ObjectManager.Instance.Target.DistanceToPlayer < 5;

        internal IntPtr SkillField => Pointer.Add(8).ReadAs<IntPtr>().Add(0xB38);

        /// <summary>
        ///     Determines whether the mainhand weapon is temp. chanted (poisons etc)
        /// </summary>
        /// <returns>
        ///     Returns <c>true</c> if the mainhand is enchanted
        /// </returns>
        public bool IsMainhandEnchanted()
        {
            try
            {
                const string isMainhandEnchanted = "{0} = GetWeaponEnchantInfo()";
                var result = Lua.Instance.ExecuteWithResult(isMainhandEnchanted);
                return result[0] == "1";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Determines whether the offhand weapon is temp. chanted (poisons etc)
        /// </summary>
        /// <returns>
        ///     Returns <c>true</c> if the offhand is enchanted
        /// </returns>
        public bool IsOffhandEnchanted()
        {
            try
            {
                const string isOffhandEnchanted = "_, _, _, {0} = GetWeaponEnchantInfo()";
                var result = Lua.Instance.ExecuteWithResult(isOffhandEnchanted);
                return result[0] == "1";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Enchants the mainhand
        /// </summary>
        /// <param name="parItemName">Name of the item to use on the mainhand weapon</param>
        public void EnchantMainhandItem(string parItemName)
        {
            const string enchantMainhand = "PickupInventoryItem(16)";
            Inventory.Instance.GetItem(parItemName).Use();
            Lua.Instance.Execute(enchantMainhand);
        }

        /// <summary>
        ///     Enchants the offhand
        /// </summary>
        /// <param name="parItemName">Name of the item to apply</param>
        public void EnchantOffhandItem(string parItemName)
        {
            const string enchantOffhand = "PickupInventoryItem(17)";
            Inventory.Instance.GetItem(parItemName).Use();
            Lua.Instance.Execute(enchantOffhand);
        }

        /// <summary>
        ///     Determines whether a wand is equipped
        /// </summary>
        /// <returns>
        ///     Return <c>true</c> if a wand is equipped
        /// </returns>
        public bool IsWandEquipped()
        {
            const string checkWand = "{0} = HasWandEquipped()";
            var result = Lua.Instance.ExecuteWithResult(checkWand);
            return result[0].Contains("1");
        }

        /// <summary>
        ///     Tells if using aoe will engage the character with other units that arent fighting right now
        /// </summary>
        /// <param name="parRange">The radius around the character</param>
        /// <returns>
        ///     Returns <c>true</c> if we can use AoE without engaging other unpulled units
        /// </returns>
        public bool IsAoeSafe(int parRange)
        {
            var mobs = ObjectManager.Instance.Npcs.
                FindAll(i => (i.Reaction == Enums.UnitReaction.Hostile || i.Reaction == Enums.UnitReaction.Neutral) &&
                             i.DistanceToPlayer < parRange).ToList();

            foreach (var mob in mobs)
                if (mob.TargetGuid != Guid)
                    return false;
            return true;
        }


        /// <summary>
        ///     Tells if a totem is spawned
        /// </summary>
        /// <param name="parName">Name of the totem</param>
        /// <returns>
        ///     Returns the distance from the player to the totem or -1 if the totem isnt summoned
        /// </returns>
        public float IsTotemSpawned(string parName)
        {
            var totem = ObjectManager.Instance.Npcs.FirstOrDefault(i => i.IsTotem && i.Name.ToLower().Contains(parName.ToLower())
                                                                            &&
                                                                            i.SummonedBy ==
                                                                            ObjectManager.Instance.Player.Guid);
                if (totem != null)
                    return totem.DistanceToPlayer;
                return -1;
        }


        /// <summary>
        ///     Eat food specified in settings if we arent already eating
        ///     <param name="parFoodName">Name of the food</param>
        /// </summary>
        public void Eat(string parFoodName)
        {
            if (IsEating) return;
            if (Inventory.Instance.GetItemCount(parFoodName) == 0) return;
            if (Wait.For("EatTimeout", 100))
                Inventory.Instance.GetItem(parFoodName);
        }


        /// <summary>
        ///     Drinks drink specified in settings if we arent already drinking
        ///     <param name="parDrinkName">Name of the food</param>
        /// </summary>
        public void Drink(string parDrinkName)
        {
            if (IsDrinking) return;
            if (Inventory.Instance.GetItemCount(parDrinkName) == 0) return;
            if (Wait.For("DrinkTimeout", 100))
                Inventory.Instance.GetItem(parDrinkName);
        }

        #endregion
    }
}