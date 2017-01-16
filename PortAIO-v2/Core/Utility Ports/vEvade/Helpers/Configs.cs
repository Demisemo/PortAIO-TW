using EloBuddy; 
using LeagueSharp.Common; 
namespace vEvade.Helpers
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using LeagueSharp;
    using LeagueSharp.Common;

    using vEvade.Core;
    using vEvade.EvadeSpells;
    using vEvade.SpecialSpells;
    using vEvade.Spells;

    using SpellData = vEvade.Spells.SpellData;

    #endregion

    internal static class Configs
    {
        #region Constants

        public const int CrossingTime = 250;

        public const int EvadePointChangeTime = 300;

        public const int EvadingFirstTime = 250;

        public const int EvadingRouteChangeTime = 250;

        public const int EvadingSecondTime = 80;

        public const int ExtraSpellRadius = 9;

        public const int ExtraSpellRange = 20;

        public const int GridSize = 10;

        public const int PathFindingInnerDistance = 35;

        public const int PathFindingOuterDistance = 60;

        #endregion

        #region Static Fields

        public static bool Debug;

        public static Menu Menu;

        private static readonly Dictionary<string, IChampionManager> ChampionManagers =
            new Dictionary<string, IChampionManager>();

        #endregion

        #region Public Properties

        public static int CheckBlock => Menu.Item("CheckBlock").GetValue<StringList>().SelectedIndex;

        public static bool CheckCollision => Menu.Item("CheckCollision").GetValue<bool>();

        public static bool CheckHp => Menu.Item("CheckHp").GetValue<bool>();

        public static bool DodgeCircle => Menu.Item("DodgeCircle").GetValue<bool>();

        public static bool DodgeCone => Menu.Item("DodgeCone").GetValue<bool>();

        public static bool DodgeDangerous => Menu.Item("DodgeDangerous").GetValue<KeyBind>().Active;

        public static int DodgeFoW => Menu.Item("DodgeFoW").GetValue<StringList>().SelectedIndex;

        public static bool DodgeLine => Menu.Item("DodgeLine").GetValue<bool>();

        public static bool DodgeTrap => Menu.Item("DodgeTrap").GetValue<bool>();

        public static bool DrawSpells => Menu.Item("DrawSpells").GetValue<bool>();

        public static bool DrawStatus => Menu.Item("DrawStatus").GetValue<bool>();

        public static bool Enabled => Menu.Item("Enabled").GetValue<KeyBind>().Active;

        #endregion

        #region Public Methods and Operators

        public static void CreateMenu()
        {
            Menu = new Menu("v躲避", "vEvade", true);
            Menu.AddToMainMenu();
            LoadSpecialSpellPlugins();

            var spells = new Menu("技能", "Spells");

            foreach (var hero in HeroManager.AllHeroes.Where(i => i.IsEnemy || Debug))
            {
                foreach (var spell in
                    SpellDatabase.Spells.Where(
                        i =>
                        !Evade.OnProcessSpells.ContainsKey(i.SpellName)
                        && (i.IsSummoner || i.ChampName == hero.ChampionName)))
                {
                    if (spell.IsSummoner && hero.GetSpellSlot(spell.SpellName) != SpellSlot.Summoner1
                        && hero.GetSpellSlot(spell.SpellName) != SpellSlot.Summoner2)
                    {
                        continue;
                    }

                    Evade.OnProcessSpells.Add(spell.SpellName, spell);

                    foreach (var name in spell.ExtraSpellNames)
                    {
                        Evade.OnProcessSpells.Add(name, spell);
                    }

                    if (!string.IsNullOrEmpty(spell.MissileName))
                    {
                        Evade.OnMissileSpells.Add(spell.MissileName, spell);
                    }

                    foreach (var name in spell.ExtraMissileNames)
                    {
                        Evade.OnMissileSpells.Add(name, spell);
                    }

                    if (!string.IsNullOrEmpty(spell.TrapName))
                    {
                        Evade.OnTrapSpells.Add(spell.TrapName, spell);
                    }

                    LoadSpecialSpell(spell);

                    var txt = "S_" + spell.MenuName;
                    var subMenu =
                        new Menu(spell.IsSummoner ? spell.SpellName : spell.ChampName + " (" + spell.Slot + ")", txt);
                    subMenu.AddItem(
                        new MenuItem(txt + "_DangerLvl", "危險級別").SetValue(new Slider(spell.DangerValue, 1, 5)));
                    subMenu.AddItem(new MenuItem(txt + "_IsDangerous", "是危險的").SetValue(spell.IsDangerous));
                    subMenu.AddItem(new MenuItem(txt + "_IgnoreHp", "如果忽視血量"))
                        .SetValue(new Slider(!spell.IsDangerous ? 65 : 80, 1));
                    subMenu.AddItem(new MenuItem(txt + "_Draw", "顯示").SetValue(true));
                    subMenu.AddItem(new MenuItem(txt + "_Enabled", "啟用").SetValue(!spell.DisabledByDefault))
                        .SetTooltip(spell.MenuName);
                    spells.AddSubMenu(subMenu);
                }
            }

            Menu.AddSubMenu(spells);

            var evadeSpells = new Menu("躲避技能", "EvadeSpells");

            foreach (var spell in EvadeSpellDatabase.Spells)
            {
                var txt = "ES_" + spell.MenuName;
                var subMenu = new Menu(spell.MenuName, txt);
                subMenu.AddItem(
                    new MenuItem(txt + "_DangerLvl", "危險級別").SetValue(new Slider(spell.DangerLevel, 1, 5)));

                if (spell.IsTargetted && spell.ValidTargets.Contains(SpellValidTargets.AllyWards))
                {
                    subMenu.AddItem(new MenuItem(txt + "_WardJump", "過牆").SetValue(false));
                }

                subMenu.AddItem(new MenuItem(txt + "_Enabled", "啟用").SetValue(true));
                evadeSpells.AddSubMenu(subMenu);
            }

            Menu.AddSubMenu(evadeSpells);

            var shieldAlly = new Menu("隊友保護", "ShieldAlly");

            foreach (var ally in HeroManager.Allies.Where(i => !i.IsMe))
            {
                shieldAlly.AddItem(new MenuItem("SA_" + ally.ChampionName, ally.ChampionName).SetValue(false));
            }

            Menu.AddSubMenu(shieldAlly);

            var misc = new Menu("雜項", "Misc");
            misc.AddItem(new MenuItem("CheckCollision", "檢查碰撞").SetValue(false));
            misc.AddItem(new MenuItem("CheckHp", "檢查玩家血量").SetValue(false));
            misc.AddItem(
                new MenuItem("CheckBlock", "Block Cast While Dodge").SetValue(
                    new StringList(new[] { "關閉", "只有危險", "總是" }, 1)));
            misc.AddItem(
                new MenuItem("DodgeFoW", "戰爭迷霧躲避技能").SetValue(
                    new StringList(new[] { "關閉", "跟踪", "躲避" }, 2)));
            misc.AddItem(new MenuItem("DodgeLine", "躲避直線技能").SetValue(true));
            misc.AddItem(new MenuItem("DodgeCircle", "躲避圓圈技能").SetValue(true));
            misc.AddItem(new MenuItem("DodgeCone", "躲避錐體技能").SetValue(true));
            misc.AddItem(new MenuItem("DodgeTrap", "躲避陷阱").SetValue(true));
            Menu.AddSubMenu(misc);

            var draw = new Menu("顯示", "Draw");
            draw.AddItem(new MenuItem("DrawSpells", "顯示技能").SetValue(true));
            draw.AddItem(new MenuItem("DrawStatus", "顯示狀態").SetValue(true));
            Menu.AddSubMenu(draw);

            Menu.AddItem(new MenuItem("Enabled", "啟用").SetValue(new KeyBind('K', KeyBindType.Toggle, true)))
                .Permashow();
            Menu.AddItem(
                new MenuItem("DodgeDangerous", "只有躲避危險").SetValue(new KeyBind(32, KeyBindType.Press)))
                .Permashow();
        }

        #endregion

        #region Methods

        private static void LoadSpecialSpell(SpellData spell)
        {
            if (ChampionManagers.ContainsKey(spell.ChampName))
            {
                ChampionManagers[spell.ChampName].LoadSpecialSpell(spell);
            }

            ChampionManagers["AllChampions"].LoadSpecialSpell(spell);
        }

        private static void LoadSpecialSpellPlugins()
        {
            ChampionManagers.Add("AllChampions", new AllChampions());

            foreach (var hero in HeroManager.AllHeroes.Where(i => i.IsEnemy || Debug))
            {
                var plugin =
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .FirstOrDefault(
                            i => i.IsClass && i.Namespace == "vEvade.SpecialSpells" && i.Name == hero.ChampionName);

                if (plugin != null && !ChampionManagers.ContainsKey(hero.ChampionName))
                {
                    ChampionManagers.Add(hero.ChampionName, (IChampionManager)NewInstance(plugin));
                }
            }
        }

        private static object NewInstance(Type type)
        {
            var target = type.GetConstructor(Type.EmptyTypes);
            var dynamic = new DynamicMethod(string.Empty, type, new Type[0], target.DeclaringType);
            var il = dynamic.GetILGenerator();
            il.DeclareLocal(target.DeclaringType);
            il.Emit(OpCodes.Newobj, target);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            var method = (Func<object>)dynamic.CreateDelegate(typeof(Func<object>));

            return method();
        }

        #endregion
    }
}