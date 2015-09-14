/* Stealth UO Client C# API
 * ScriptAPI.cs updated for ScriptDotNet2.dll 22.3.2015 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace ScriptAPI
{
    using ScriptDotNet2;
    using ScriptDotNet2.Data;
    using ScriptDotNet2.Model;


    static class Constants
    {
        static public readonly string[] MapDirection = new string[] { "North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "NorthWest" };
    }

    #region Client
    public static class Client
    {
        public static void Print(string message)
        {
            Stealth.Default.ClientPrint(message);
        }
        public static void Print(uint Obj, ushort Color, ushort Font, string Text)
        {
            Stealth.Default.ClientPrintEx(Obj, Color, Font, Text);
        }
    }
    #endregion

    #region Profile Methods
    public static class Profile
    {
        public static bool IsConnected
        {
            private set { }
            get
            {
                return Stealth.Default.GetConnectedStatus();
            }
        }
    }
    #endregion

    #region Script Methods
    public static class Script
    {
        public static void Wait(int WaitMS)
        {
            Stealth.Default.Wait(WaitMS);
        }
    }
    #endregion

    #region Item Find/Search Methods
    public static class Find
    {
        /// <summary>
        /// Find an Item
        /// </summary>
        /// <param name="Type">Item Type</param>
        /// <param name="Container">[Optional] ID of Container To Search [Default: 0x0 = Ground]</param>
        /// <param name="Recursive">[Optional] Search Sub-Containers Recursively [Default: False]</param>
        /// <param name="Color">[Optional] Color Category To Search[Default: 0xFFFF = All Colors]</param>
        /// <returns>Returns Item if Found or NULL if Not Found</returns>
        public static Item FindItem(ushort Type, uint Container = 0x00000000, bool Recursive = false, ushort Color = 0xFFFF)
        {
            uint finditem = Stealth.Default.FindTypeEx(Type, Color, Container, Recursive);
            if (finditem > 0)
                return new Item(finditem);
            else
                return null;
        }

        /// <summary>
        /// Find all items of a certain type
        /// </summary>
        /// <param name="Type">Item Type</param>
        /// <param name="Container">[Optional] ID of Container To Search [Default: 0x0 = Ground]</param>
        /// <param name="Recursive">[Optional] Search Sub-Containers Recursively [Default: False]</param>
        /// <param name="Color">[Optional] Color Category To Search[Default: 0xFFFF = All Colors]</param>
        /// <returns><Item>List of Items Found</Item></returns>
        public static List<Item> FindItems(ushort Type, uint Container = 0x00000000, bool Recursive = false, ushort Color = 0xFFFF)
        {
            List<Item> AllList = new List<Item>();
            Stealth.Default.FindTypeEx(Type, Color, Container, Recursive);
            if (Stealth.Default.GetFindCount() == 0)
                return AllList;
            List<uint> findlist = Stealth.Default.GetFindList();

            foreach (uint item in findlist)
                AllList.Add(new Item(item));

            return AllList;
        }

        /// <summary>
        /// Searches for items of Types[], in Containers[], and of Colors[]
        /// </summary>
        /// <param name="Types">Array of Item Types to search for</param>
        /// <param name="Containers">Array of Containers to search in [new uint[] = {0x00000000};] for Ground</param>
        /// <param name="Colors">Array of Colors to search for [new ushort[] = { 0xFFFF };] for all colors</param>
        /// <param name="Recursive">[Optional] Search Sub-Containers Recursively [Default: False]</param>
        /// <returns>List of Items Found</returns>
        public static List<Item> FindItems(ushort[] Types, uint[] Containers, ushort[] Colors, bool Recursive = false)
        {
            Stealth.Default.FindTypesArrayEx(Types, Colors, Containers, Recursive);
            List<uint> findlist = Stealth.Default.GetFindList();
            List<Item> AllList = new List<Item>();
            foreach (uint item in findlist)
                AllList.Add(new Item(item));
            return AllList;
        }

        /// <summary>
        /// Set distance (from player) to scan for items
        /// </summary>
        public static uint FindDistance
        {
            get
            {
                return Stealth.Default.GetFindDistance();
            }
            set
            {
                Stealth.Default.SetFindDistance(value);
            }
        }

        /// <summary>
        /// Set Vertical distance (from player) to scan for items
        /// </summary>
        public static uint FindVerticalDistance
        {
            get
            {
                return Stealth.Default.GetFindVertical();
            }
            set
            {
                Stealth.Default.SetFindVertical(value);
            }
        }
    }
    #endregion

    #region Target / Targeting Methods
    public static class Target
    {
        public static Item RequestTileTarget(uint TimeoutMS = 0)
        {
            Stealth.Default.ClientRequestTileTarget();
            Stopwatch timer = new Stopwatch();


            timer.Start();
            while (Stealth.Default.ClientTargetResponsePresent() == false)
            {
                if (TimeoutMS != 0 && timer.ElapsedMilliseconds >= TimeoutMS)
                    return default(Item);
            }

            return new Item(Stealth.Default.ClientTargetResponse().ID);
        }

        public static Item RequestTarget(uint TimeoutMS = 0)
        {
            Stealth.Default.ClientRequestObjectTarget();
            Stopwatch timer = new Stopwatch();


            timer.Start();
            while (Stealth.Default.ClientTargetResponsePresent() == false)
            {
                if (TimeoutMS != 0 && timer.ElapsedMilliseconds >= TimeoutMS)
                    return default(Item);
            }

            return new Item(Stealth.Default.ClientTargetResponse().ID);
        }

        public static TargetInfo TargetSelection
        {
            private set { }
            get
            {
                return Stealth.Default.ClientTargetResponse();
            }
        }

        public static bool IsTargetSet
        {
            private set { }
            get { return Stealth.Default.ClientTargetResponsePresent(); }
        }

        public static void WaitForTarget(int TimeoutMS = 10000)
        {
            Stealth.Default.WaitForTarget(TimeoutMS);
        }

        public static void TargetObject(Item obj)
        {
            Stealth.Default.TargetToObject(obj.ID);
        }

        public static void TargetCoord(ushort X, ushort Y, sbyte Z)
        {
            Stealth.Default.TargetToXYZ(X, Y, Z);
        }

        public static void MakeTargetPointer()
        {
            Stealth.Default.ClientRequestObjectTarget();
        }
    }
    #endregion

    // These aren't very important to most people
    #region BaseType
    /// <summary>
    /// Where it all starts.  Every thing has an Item Type.
    /// </summary>
    public class BaseType
    {
        private readonly ushort _type;
        public ushort Type { private set { } get { return _type; } }

        protected BaseType(uint ID)
        {
            _type = Stealth.Default.GetType(ID);
        }
        public BaseType(ushort Type)
        {
            _type = Type;
        }
    }
    #endregion

    #region BaseThing
    /// <summary>
    /// Most basic class to describe any "thing" in UO
    /// ID, Type, X, Y, Z
    /// </summary>
    public class BaseThing : BaseType, IEquatable<BaseThing>
    {
        protected uint _id;

        public BaseThing(uint ID)
            : base(ID)
        {
            _id = ID;
        }
        public BaseThing(BaseThing thing)
            : base(thing.ID)
        {
            _id = thing.ID;
        }
        public BaseThing(uint ID, ushort TYPE)
            : base(TYPE)
        {
            _id = ID;
        }

        /// <summary>
        /// X Coordinate [Relative to container]
        /// </summary>
        public int X
        {
            private set { }
            get
            {
                return Stealth.Default.GetX(_id);
            }
        }
        /// <summary>
        /// Y Coordinate [Relative to container]
        /// </summary>
        public int Y
        {
            private set { }
            get
            {
                return Stealth.Default.GetY(_id);
            }
        }
        /// <summary>
        /// Z Coordinate [Relative to container]
        /// </summary>
        public sbyte Z
        {
            private set { }
            get
            {
                return Stealth.Default.GetZ(_id);
            }
        }

        /// <summary>
        /// UO Unique Identifier Number
        /// </summary>
        public uint ID { private set { } get { return _id; } }

        public override int GetHashCode()
        {
            return this._id.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            BaseThing other = obj as BaseThing;
            if (other == null)
                return false;
            return (this.ID.Equals(other.ID));
        }

        public bool Equals(BaseThing other)
        {
            if (other == null)
                return false;
            return (this._id.Equals(other.ID));
        }

        public static bool operator ==(BaseThing a, BaseThing b)
        {
            if (object.ReferenceEquals(a, b)) return true;
            if (object.ReferenceEquals(a, null)) return false;
            if (object.ReferenceEquals(b, null)) return false;

            return a.Equals(b);
        }
        public static bool operator !=(BaseThing a, BaseThing b)
        {
            if (object.ReferenceEquals(a, b)) return !true;
            if (object.ReferenceEquals(a, null)) return !false;
            if (object.ReferenceEquals(b, null)) return !false;

            return !a.Equals(b);
        }
    }
    #endregion

    // These are the important ones
    #region Item
    /// <summary>
    /// Basic Item class (all in-game objects derive from this)
    /// </summary>
    public class Item : BaseThing
    {
        public Item Parent
        {
            private set { }
            get
            {
                return new Item(Stealth.Default.GetParent(ID));
            }
        }

        public void Click()
        {
            Stealth.Default.ClickOnObject(_id);
        }

        public void Use()
        {
            Stealth.Default.UseObject(_id);
        }

        /// <summary>
        /// True/False if Item Exists
        /// </summary>
        public bool IsExists
        {
            private set { }
            get
            {
                return Stealth.Default.IsObjectExists(_id);
            }
        }
        public bool IsNPC
        {
            private set { }

            get
            {
                return Stealth.Default.IsNPC(_id);
            }
        }
        public bool IsContainer
        {
            private set { }
            get
            {
                return Stealth.Default.IsContainer(_id);
            }
        }
        public bool IsMovable
        {
            private set { }
            get
            {
                return Stealth.Default.IsMovable(_id);
            }
        }



        /// <summary>
        /// Returns distance from player
        /// [-1 if invalid / non-existant]
        /// </summary>
        public int Distance
        {
            private set { }
            get
            {
                return Stealth.Default.GetDistance(_id);
            }
        }

        /// <summary>
        /// Tooltip describing properties of the item
        /// [Value = NULL if unable to Scan]
        /// </summary>
        public string Tooltip
        {
            private set { }
            get
            {
                return Stealth.Default.GetTooltip(_id);
            }
        }
        public string WaitForTooltip(int WaitMS)
        {
            return Stealth.Default.GetTooltip(ID, WaitMS);
        }

        /// <summary>
        /// UO Color Value
        /// </summary>
        public ushort Color
        {
            private set { }
            get
            {
                return Stealth.Default.GetColor(_id);
            }
        }

        /// <summary>
        /// Quantity of this ID
        /// </summary>
        public int Quantity
        {
            private set { }
            get
            {
                return Stealth.Default.GetQuantity(_id);
            }
        }

        /// <summary>
        /// UO Name Value
        /// </summary>
        public string Name
        {
            private set { }
            get
            {
                return Stealth.Default.GetName(_id);
            }
        }

        public Item(uint ID)
            : base(ID)
        {
        }
        public Item(BaseThing thing)
            : base(thing.ID)
        {
        }
    }
    #endregion

    #region Container
    public class Container : Item
    {
        public Container(uint ID)
            : base(ID)
        {
        }
        public Container(BaseThing thing)
            : base(thing.ID)
        {
        }


        public List<Item> Inventory(ushort FindType = 0xFFFF, bool Recursive = true)
        {
            return Find.FindItems(FindType, _id, Recursive);
        }

        public bool Open()
        {
            base.Use();
            Stealth.Default.Wait(250);
            if (Stealth.Default.GetLastContainer() == _id)
                return true;

            return false;
        }

        public void Close()
        {
            Stealth.Default.CloseClientUIWindow(UIWindowType.Container, _id);
        }
    }
    #endregion

    #region Creature
    /// <summary>
    /// Creature Class
    /// Base type for all living things
    /// Properties Include MaxHP, HP (current), Notoriety
    /// </summary>
    public class Creature : Item
    {
        /// <summary>
        /// Maximum HP of Creature
        /// </summary>
        public int MaxHP
        {
            private set { }
            get
            {
                return Stealth.Default.GetMaxHP(ID);
            }
        }

        /// <summary>
        /// Current HP of Creature
        /// </summary>
        public int HP
        {
            private set { }
            get
            {
                return Stealth.Default.GetHP(ID);
            }
        }

        /// <summary>
        /// Current HP Percentage
        /// </summary>
        public int hpPerc
        {
            private set { }
            get
            {
                int _maxhp = Stealth.Default.GetMaxHP(ID);
                if (_maxhp > 0)
                {
                    int temp = Stealth.Default.GetHP(ID) * 100 / _maxhp;
                    return temp;
                }
                else
                    return 100;
            }
        }

        /// <summary>
        /// Current HP Percentage
        /// </summary>
        public bool IsPoisoned
        {
            private set { }
            get
            {
                bool temp = Stealth.Default.IsPoisoned(ID);
                return temp;
            }
        }



        /// <summary>
        /// Notoriety of Creature
        /// 1: Innocent (Blue)
        /// 2: Guild / Alliance (Green)
        /// 3: Attackable But Not Criminal (Gray)
        /// 4: Criminal (gray)
        /// 5: Enemy (orange)
        /// 6: Murderer (red)
        /// </summary>
        public byte Notoriety
        {
            private set { }
            get
            {
                return Stealth.Default.GetNotoriety(ID);
            }
        }

        public Creature(uint ID)
            : base(ID)
        {
        }
        public Creature(BaseThing thing)
            : base(thing.ID)
        {
        }

    }
    #endregion

    #region NPC
    public class NPC : Creature
    {
        public uint RightHand
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetRhandLayer(), ID);
            }
        }

        public uint LeftHand
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetLhandLayer(), ID);
            }
        }

        public uint Shoes
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetShoesLayer(), ID);
            }
        }

        public uint Pants
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetPantsLayer(), ID);
            }
        }

        public uint Shirt
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetShirtLayer(), ID);
            }
        }

        public uint Hat
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetHatLayer(), ID);
            }
        }

        public uint Gloves
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetGlovesLayer(), ID);
            }
        }

        public uint Ring
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetRingLayer(), ID);
            }
        }

        public uint Talisman
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetTalismanLayer(), ID);
            }
        }

        public uint Neck
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetNeckLayer(), ID);
            }
        }

        public uint Hair
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetHairLayer(), ID);
            }
        }

        public uint Waist
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetWaistLayer(), ID);
            }
        }

        public uint Torso
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetTorsoLayer(), ID);
            }
        }

        public uint Brace
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetBraceLayer(), ID);
            }
        }

        public uint Beard
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetBeardLayer(), ID);
            }
        }

        public uint TorsoH
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetTorsoHLayer(), ID);
            }
        }

        public uint Ear
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetEarLayer(), ID);
            }
        }

        public uint Arms
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetArmsLayer(), ID);
            }
        }

        public uint Cloak
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetCloakLayer(), ID);
            }
        }

        public Container Backpack
        {
            private set { }
            get
            {
                return new Container(Stealth.Default.ObjAtLayerEx(Stealth.GetBpackLayer(), ID));
            }
        }

        public uint Robe
        {
            private set { }
            get
            {
                return Stealth.Default.ObjAtLayerEx(Stealth.GetRobeLayer(), ID);
            }
        }

        public List<Item> Inventory
        {
            private set { }
            get
            {
                return this.Backpack.Inventory();
            }
        }

        public NPC(uint ID)
            : base(ID)
        {
        }
        public NPC(BaseThing thing)
            : base(thing.ID)
        {
        }

    }
    #endregion

    #region Self
    public static class Self
    {
        private static ExtendedInfo extInfo;
        public static uint ID { private set { } get { return Stealth.Default.GetSelfID(); } }

        public static int Mana { private set { } get { return Stealth.Default.GetSelfMana(); } }
        public static int HP { private set { } get { return Stealth.Default.GetSelfLife(); } }
        public static int Stamina { private set { } get { return Stealth.Default.GetSelfStam(); } }
        public static int Weight { private set { } get { return Stealth.Default.GetSelfWeight(); } }

        public static int hpPerc
        {
            private set { }
            get
            {
                int temp = Stealth.Default.GetHP(ID) * 100 / Stealth.Default.GetMaxHP(ID);
                return temp;
            }
        }

        public static int Life { private set { } get { return HP; } }
        public static int Stam { private set { } get { return Stamina; } }

        public static int MaxMana { private set { } get { return Stealth.Default.GetSelfMaxMana(); } }
        public static int MaxHP { private set { } get { return Stealth.Default.GetSelfMaxLife(); } }
        public static int MaxStamina { private set { } get { return Stealth.Default.GetSelfMaxStam(); } }
        public static int MaxWeight { private set { } get { return Stealth.Default.GetSelfMaxWeight(); } }

        public static int MaxLife { private set { } get { return MaxHP; } }
        public static int MaxStam { private set { } get { return MaxStamina; } }


        public static int X { private set { } get { return Stealth.Default.GetX(ID); } }
        public static int Y { private set { } get { return Stealth.Default.GetY(ID); } }
        public static sbyte Z { private set { } get { return Stealth.Default.GetZ(ID); } }

        public static string Direction
        {
            private set { }
            get
            {
                return Constants.MapDirection[Stealth.Default.GetDirection(ID)];
            }
        }
        public static string Facing { private set { } get { return Direction; } }

        public static string DirectionTo(Item Obj)
        {
            return Constants.MapDirection[Stealth.Default.CalcDir(X, Y, Obj.X, Obj.Y)];
        }

        public static string Name
        {
            private set { }
            get
            {
                return Stealth.Default.GetCharName();
            }
        }

        public static bool IsHidden
        {
            private set { }
            get
            {
                return Stealth.Default.GetHiddenStatus();
            }
        }

        public static Container Backpack
        {
            private set { }
            get
            {
                return new Container(Stealth.Default.GetBackpackID());
            }
        }

        public static byte World
        {
            private set { }
            get
            {
                return Stealth.Default.GetWorldNum();
            }
        }

        public static string Shard
        {
            private set { }
            get
            {
                return Stealth.Default.GetShardName();
            }
        }

        public static uint LastContainer
        {
            private set { }
            get
            {
                return Stealth.Default.GetLastContainer();
            }
        }

        public static void Cast(string SpellName, Item Object = null)
        {
            if (Object != null)
                Stealth.Default.CastSpellToObj(SpellName, Object.ID);
            else
                Stealth.Default.CastSpell(SpellName);
        }

        public static bool UseSkill(string Skill)
        {
            return Stealth.Default.UseSkill(Skill);
        }

        public static void UsePrimary()
        {
            Stealth.Default.UsePrimaryAbility();
        }
        public static void UseSecondary()
        {
            Stealth.Default.UseSecondaryAbility();
        }
        public static string ActiveAbility
        {
            private set { }
            get
            {
                return Stealth.Default.GetAbility();
            }
        }

        public static bool WarMode
        {
            set
            {
                Stealth.Default.SetWarMode(value);
            }
            get
            {
                return Stealth.Default.GetWarModeStatus();
            }
        }

        public static Creature Target
        {
            set
            {
                Stealth.Default.Attack(value.ID);
            }
            get
            {
                uint ret = Stealth.Default.GetWarTarget();
                if (ret == 0)
                    return null;
                return new Creature(Stealth.Default.GetWarTarget());
            }
        }

        static Self()
        {
            Properties.Refresh();
        }

        public static class Properties
        {
            /// <summary>
            /// Get updated properties
            /// OSI Shards Only
            /// </summary>
            public static void Refresh()
            {
                extInfo = Stealth.Default.GetExtInfo();
            }

            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort DamageMin { get { return extInfo.DamageMin; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort DamageMax { get { return extInfo.DamageMax; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static uint Tithing_Points { get { return extInfo.TithingPoints; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Hit_Chance_Incr { get { return extInfo.HitChanceIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Swing_Speed_Incr { get { return extInfo.SwingSpeedIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Damage_Chance_Incr { get { return extInfo.DamageChanceIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Lower_Reagent_Cost { get { return extInfo.LowerReagentCost; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort HP_Regen { get { return extInfo.HPRegen; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Stam_Regen { get { return extInfo.StamRegen; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Mana_Regen { get { return extInfo.ManaRegen; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Reflect_Phys_Damage { get { return extInfo.ReflectPhysDamage; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Enhance_Potions { get { return extInfo.EnhancePotions; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Defense_Chance_Incr { get { return extInfo.DefenseChanceIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Spell_Damage_Incr { get { return extInfo.SpellDamageIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Faster_Cast_Recovery { get { return extInfo.FasterCastRecovery; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Faster_Casting { get { return extInfo.FasterCasting; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Lower_Mana_Cost { get { return extInfo.LowerManaCost; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Strength_Incr { get { return extInfo.StrengthIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Dext_Incr { get { return extInfo.DextIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Int_Incr { get { return extInfo.IntIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort HP_Incr { get { return extInfo.HPIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Stam_Incr { get { return extInfo.StamIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Mana_Incr { get { return extInfo.ManaIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Max_HP_Incr { get { return extInfo.MaxHPIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Max_Stam_Incr { get { return extInfo.MaxStamIncr; } private set { } }
            /// <summary>
            /// OSI Shards Only
            /// </summary>
            public static ushort Max_Mana_Increase { get { return extInfo.MaxManaIncrease; } private set { } }

            public static short Luck
            {
                get
                {
                    short luck = Convert.ToInt16(Stealth.Default.GetSelfLuck());
                    if (luck != 0)
                        return luck;
                    return extInfo.Luck;
                }
                private set { }
            }

            public static Byte Sex { private set { } get { return Stealth.Default.GetSelfSex(); } }
            public static String Title { private set { } get { return Stealth.Default.GetCharTitle(); } }
            public static uint Gold { private set { } get { return Stealth.Default.GetSelfGold(); } }
            public static ushort Weight { private set { } get { return Stealth.Default.GetSelfWeight(); } }
            public static ushort MaxWeight { private set { } get { return Stealth.Default.GetSelfMaxWeight(); } }
            public static Byte Race { private set { } get { return Stealth.Default.GetSelfRace(); } }
            public static Byte MaxPets { private set { } get { return Stealth.Default.GetSelfPetsMax(); } }
            public static Byte Pets { private set { } get { return Stealth.Default.GetSelfPetsCurrent(); } }

            public static ushort PhysicalResist { private set { } get { return Stealth.Default.GetSelfArmor(); } }
            public static ushort FireResist { private set { } get { return Stealth.Default.GetSelfFireResist(); } }
            public static ushort ColdResist { private set { } get { return Stealth.Default.GetSelfColdResist(); } }
            public static ushort PoisonResist { private set { } get { return Stealth.Default.GetSelfPoisonResist(); } }
            public static ushort EnergyResist { private set { } get { return Stealth.Default.GetSelfEnergyResist(); } }
        }
    }
    #endregion

    #region GumpClass
    public class GumpClass
    {
        private uint _gumpid, _gumptype;
        private int _gumpidx;
        private GumpInfo _gumpinfo;

        private GumpClass(uint Serial, uint GumpType)
        {
            _gumpid = Serial;
            _gumptype = GumpType;
            _gumpidx = GetIndexbySerial(Serial);
            _gumpinfo = Stealth.Default.GetGumpInfo(Convert.ToUInt16(_gumpidx));
        }

        public uint ID
        {
            private set { }
            get
            {
                return _gumpid;
            }
        }

        public int Index
        {
            private set { }
            get
            {
                return _gumpidx;
            }
        }

        public int Pages
        {
            private set { }
            get
            {
                return _gumpinfo.Pages;
            }
        }

        public bool IsExists()
        {
            for (ushort i = 0; i < GumpCount; i++)
            {
                if (Stealth.Default.GetGumpSerial(i) == _gumpid)
                    return true;
            }
            return false;
        }

        public List<string> Textlines()
        {
            return Stealth.Default.GetGumpTextLines((ushort)_gumpidx);
        }


        public List<ButtonTileArt> ButtonTileArts { get { return _gumpinfo.ButtonTileArts.ToList(); } private set { } }
        public List<CheckBox> CheckBoxes { get { return _gumpinfo.CheckBoxes.ToList(); } private set { } }
        public List<CheckerTrans> CheckerTrans { get { return _gumpinfo.CheckerTrans.ToList(); } private set { } }
        public List<CroppedText> CroppedText { get { return _gumpinfo.CroppedText.ToList(); } private set { } }
        public List<EndGroup> EndGroups { get { return _gumpinfo.EndGroups.ToList(); } private set { } }
        public List<Group> Groups { get { return _gumpinfo.Groups.ToList(); } private set { } }
        public List<GumpButton> GumpButtons { get { return _gumpinfo.GumpButtons.ToList(); } private set { } }
        public List<GumpPic> GumpPics { get { return _gumpinfo.GumpPics.ToList(); } private set { } }
        public List<GumpPicTiled> GumpPicTiled { get { return _gumpinfo.GumpPicTiled.ToList(); } private set { } }
        public List<GumpText> GumpText { get { return _gumpinfo.GumpText.ToList(); } private set { } }
        public List<HtmlGump> HtmlGump { get { return _gumpinfo.HtmlGump.ToList(); } private set { } }
        public List<ItemProperty> ItemProperties { get { return _gumpinfo.ItemProperties.ToList(); } private set { } }
        public List<RadioButton> RadioButtons { get { return _gumpinfo.RadioButtons.ToList(); } private set { } }
        public List<ResizePic> ResizePics { get { return _gumpinfo.ResizePics.ToList(); } private set { } }
        public List<TextEntry> TextEntries { get { return _gumpinfo.TextEntries.ToList(); } private set { } }
        public List<TextEntryLimited> TextEntriesLimited { get { return _gumpinfo.TextEntriesLimited.ToList(); } private set { } }
        public List<TilePicture> TilePicHues { get { return _gumpinfo.TilePicHue.ToList(); } private set { } }
        public List<TilePic> TilePics { get { return _gumpinfo.TilePics.ToList(); } private set { } }
        public List<Tooltip> Tooltips { get { return _gumpinfo.Tooltips.ToList(); } private set { } }
        public List<XmfHTMLGump> XmfHtmlGump { get { return _gumpinfo.XmfHtmlGump.ToList(); } private set { } }
        public List<XmfHTMLGumpColor> XmfHTMLGumpColor { get { return _gumpinfo.XmfHTMLGumpColor.ToList(); } private set { } }
        public List<XmfHTMLTok> XmfHTMLTok { get { return _gumpinfo.XmfHTMLTok.ToList(); } private set { } }

        #region Click Events
        public bool ClickButton(int ButtonID)
        {
            if (IsExists())
                return Stealth.Default.NumGumpButton(Convert.ToUInt16(_gumpidx), ButtonID);

            return false;
        }

        public bool ToggleRadioButton(int RadioID, int Value)
        {
            if (IsExists())
                return Stealth.Default.NumGumpRadiobutton(Convert.ToUInt16(_gumpidx), RadioID, Value);

            return false;
        }

        public bool ToggleCheckBoxButton(int CheckBoxID, int Value)
        {
            if (IsExists())
                return Stealth.Default.NumGumpCheckBox(Convert.ToUInt16(_gumpidx), CheckBoxID, Value);

            return false;
        }

        public bool ChangeTextField(int TextFieldID, String Text)
        {
            if (IsExists())
                return Stealth.Default.NumGumpTextEntry(Convert.ToUInt16(_gumpidx), TextFieldID, Text);

            return false;
        }
        #endregion

        #region Static Members
        public static GumpClass Create(ushort index)
        {
            uint Serial = Stealth.Default.GetGumpSerial(index);
            ushort Type = Convert.ToUInt16(Stealth.Default.GetGumpID(index));

            return new GumpClass(Serial, Type);
        }

        public static GumpClass LastGump()
        {
            uint index = GumpCount - 1;

            if (index >= 0)
            {
                return GumpClass.Create((ushort)index);
            }

            return default(GumpClass);
        }

        public static uint GetGumpType(uint Serial)
        {
            int index = GetIndexbySerial(Serial);
            if (index > -1)
                return Stealth.Default.GetGumpID(Convert.ToUInt16(index));
            return 0;
        }

        public static uint GetSerial(uint GumpType)
        {
            int index = GetIndexbyType(GumpType);
            if (index > -1)
                return Stealth.Default.GetGumpSerial(Convert.ToUInt16(index));
            return 0;
        }

        public static int GetIndexbyType(uint GumpType)
        {
            for (ushort i = 0; i < GumpCount; i++)
            {
                if (Stealth.Default.GetGumpID(i) == GumpType)
                    return i;
            }
            return -1;
        }

        public static int GetIndexbySerial(uint GumpSerial)
        {
            for (ushort i = 0; i < GumpCount; i++)
            {
                if (Stealth.Default.GetGumpSerial(i) == GumpSerial)
                    return i;
            }
            return -1;
        }

        public static uint GumpCount
        {
            private set { }
            get
            {
                return Stealth.Default.GetGumpsCount();
            }
        }
        #endregion
    }
    #endregion

    #region Context Menu
    public class ContextMenu
    {
        Creature _owner;

        public ContextMenu(uint Owner)
        {
            _owner = new Creature(Owner);
        }

        public bool Click(uint ClilocID)
        {
            if ((_owner.IsExists) && (_owner.Distance <= 3))
            {
                Stealth.Default.ClearContextMenu();
                Stealth.Default.RequestContextMenu(_owner.ID);
                String Text = "";
                if ((Text = Stealth.Default.GetContextMenu()).Trim() != "")
                {
                    Text = Text.Replace("", "¶");
                    String[] E = Text.Split(new char[] { '¶' });
                    foreach (String a in E)
                    {
                        String[] b = a.Split(new char[] { '|' });
                        if (Convert.ToUInt32(b[1]) == ClilocID)
                        {
                            Stealth.Default.SetContextMenuHook(_owner.ID, Convert.ToByte(b));
                            Stealth.Default.RequestContextMenu(_owner.ID);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
    #endregion

    #region BaseProperty
    /*
    public class BaseProperty
    {
        uint _ID;
        ClilocRec _clilocrec;

        public List<ClilocItemRec> Items { get { return _clilocrec.Items.ToList(); } }

        public BaseProperty(uint ID)
        {
            _ID = ID;
            Update();
        }

        private void Update()
        {
            _clilocrec = GetObject(_ID);
        }

        public List<string> GetParams(uint ClilocID)
        {
            Update();
            return GetParams(_clilocrec, ClilocID);
        }

        public String StringParameter(uint cliloc, int index)
        {
            Update();
            return StringParameter(_clilocrec, cliloc, index);
        }

        public int IntParameter(uint cliloc, int index)
        {
            Update();
            return IntParameter(_clilocrec, cliloc, index);
        }

        public Double FloatParameter(uint cliloc, int index)
        {
            Update();
            return FloatParameter(_clilocrec, cliloc, index);
        }

        public uint ClilocParameter(uint cliloc, int index)
        {
            Update();
            return ClilocParameter(_clilocrec, cliloc, index);
        }

        public bool ClilocExist(uint cliloc, bool checkparams = false)
        {
            Update();
            return ClilocExist(_clilocrec, cliloc, checkparams);
        }

        #region static Methods

        public static ClilocRec GetObject(uint ID)
        {
            return Stealth.Default.GetClilocRec(ID);
        }

        public static ClilocItemRec GetProperty(ClilocRec rec, uint cliloc)
        {
            foreach (ClilocItemRec item in rec.Items)
                if (item.ClilocID == cliloc)
                    return item;
            return new ClilocItemRec();
        }

        public static List<string> GetParams(ClilocRec rec, uint ClilocID)
        {
            foreach (ClilocItemRec item in rec.Items)
                if (item.ClilocID == ClilocID)
                    return item.Params;
            return new List<string>();
        }

        public static String StringParameter(ClilocRec rec, uint cliloc, int index)
        {
            try
            {
                return GetParams(rec, cliloc)[index];
            }
            catch
            {

            }
            return "";
        }

        public static int IntParameter(ClilocRec rec, uint cliloc, int index)
        {
            try
            {
                return Convert.ToInt32((Regex.Replace(StringParameter(rec, cliloc, index), @"[^0-9]", "").Trim()));
            }
            catch
            {

            }
            return 0;
        }

        public static Double FloatParameter(ClilocRec rec, uint cliloc, int index)
        {
            try
            {
                return Convert.ToDouble((Regex.Replace(StringParameter(rec, cliloc, index), @"^\d+([\.\,]?\d+)?$", "").Trim()));
            }
            catch
            {

            }
            return 0.0;
        }

        public static uint ClilocParameter(ClilocRec rec, uint cliloc, int index)
        {
            try
            {
                return Convert.ToUInt32((Regex.Replace(StringParameter(rec, cliloc, index), @"[^0-9]", "").Trim()));
            }
            catch
            {

            }
            return 0;
        }

        public static bool ClilocExist(ClilocRec rec, uint cliloc, bool checkparams = false)
        {
            foreach (ClilocItemRec item in rec.Items)
            {
                if (item.ClilocID == cliloc)
                    return true;
                if (checkparams)
                {
                    foreach (String P in item.Params)
                    {
                        uint cli = 0;
                        try
                        {
                            cli = Convert.ToUInt32((Regex.Replace(P, @"[^0-9]", "").Trim()));
                        }
                        catch
                        {
                            cli = 0;
                        }
                        if (cli == cliloc)
                            return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
    */
    #endregion
}
