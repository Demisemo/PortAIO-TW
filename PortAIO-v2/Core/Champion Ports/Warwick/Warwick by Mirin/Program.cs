using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;
using Color = System.Drawing.Color;

using EloBuddy;
namespace Warwick
{
    internal class Program
    {
        private struct Tuple<TA, TB, TC> : IEquatable<Tuple<TA, TB, TC>>
        {
            private readonly TA item;
            private readonly TB itemType;
            private readonly TC targetingType;

            public Tuple(TA pItem, TB pItemType, TC pTargetingType)
            {
                item = pItem;
                itemType = pItemType;
                targetingType = pTargetingType;
            }

            public TA Item
            {
                get { return item; }
            }

            public TB ItemType
            {
                get { return itemType; }
            }

            public TC TargetingType
            {
                get { return targetingType; }
            }

            public override int GetHashCode()
            {
                return item.GetHashCode() ^ itemType.GetHashCode() ^ targetingType.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                return Equals((Tuple<TA, TB, TC>)obj);
            }

            public bool Equals(Tuple<TA, TB, TC> other)
            {
                return other.item.Equals(item) && other.itemType.Equals(itemType) &&
                       other.targetingType.Equals(targetingType);
            }
        }

        private enum EnumItemType
        {
            Targeted,
            AoE
        }

        private enum EnumItemTargettingType
        {
            Ally,
            EnemyHero,
            EnemyObjects
        }

        public static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        private static string Tab
        {
            get { return "       "; }
        }

        private const string ChampionName = "Warwick";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;

        public static Menu Config;
        public static Menu rMenu;
        public static Menu TargetSelectorMenu;

        public static SpellR SpellR;

        private static Dictionary<string, Tuple<Items.Item, EnumItemType, EnumItemTargettingType>> ItemDb;

        public static void Game_OnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != ChampionName)
                return;

            Q = new Spell(SpellSlot.Q, 350);
            W = new Spell(SpellSlot.W, 4000);
            E = new Spell(SpellSlot.E, 375);
            R = new Spell(SpellSlot.R, 335);
            R.SetSkillshot(0.25f, 90f, 2200f, false, SkillshotType.SkillshotLine);
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            ItemDb = new Dictionary<string, Tuple<Items.Item, EnumItemType, EnumItemTargettingType>>
            {
                {
                    "Tiamat",
                    new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                        new Items.Item(3077, 250f),
                        EnumItemType.AoE,
                        EnumItemTargettingType.EnemyObjects)
                },
                {
                    "Bilge",
                    new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(new Items.Item(3144, 450f),
                        EnumItemType.Targeted, EnumItemTargettingType.EnemyHero)
                },
                {
                    "Blade",
                    new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                        new Items.Item(3153, 450f),
                        EnumItemType.Targeted,
                        EnumItemTargettingType.EnemyHero)
                },
                {
                    "Hydra",
                    new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                        new Items.Item(3074, 250f),
                        EnumItemType.AoE,
                        EnumItemTargettingType.EnemyObjects)
                },
                {
                    "Randiun",
                    new Tuple<Items.Item, EnumItemType, EnumItemTargettingType>(
                        new Items.Item(3143, 490f),
                        EnumItemType.AoE,
                        EnumItemTargettingType.EnemyHero)
                }
            };


            Config = new Menu("Warwick | the Blood Hunter", "Warwick", true);

            TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            Config.AddSubMenu(TargetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            rMenu = new Menu("R", "R");
            {
                SpellR = new SpellR();
                Config.AddSubMenu(rMenu);
            }
            var menuCombo = new Menu("Combo", "Combo");
            {
                Config.AddSubMenu(menuCombo);

                PlayerSpells.Initialize();
            }

            var menuHarass = new Menu("Harass", "Harass");
            {
                menuHarass.AddItem(new MenuItem("Harass.Q.Use", "Use Q").SetValue(true));
                menuHarass.AddItem(new MenuItem("Harass.Q.UseT", Tab + "Toggle").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
                menuHarass.AddItem(new MenuItem("Harass.Q.UseTEnemyUn", Tab + "Don't Use Q Under Turret").SetValue(true));
                menuHarass.AddItem(new MenuItem("Harass.Q.MinMana", Tab + "Min. Mana Per.:").SetValue(new Slider(20, 1)));

                Config.AddSubMenu(menuHarass);
            }

            var menuLane = new Menu("Lane Farm", "Lane Farm");
            {
                menuLane.AddItem(
                    new MenuItem("Lane.Q.Use", "Use Q").SetValue(
                        new StringList(new[] { "Off", "Last Hit", "Only out of AA Range", "Everytime" }, 1)));
                menuLane.AddItem(new MenuItem("Lane.Q.MinMana", Tab + "Min. Mana Per.:").SetValue(new Slider(35, 1)));
                Config.AddSubMenu(menuLane);
            }

            var menuJungle = new Menu("Jungle Farm", "Jungle Farm");
            {
                menuJungle.AddItem(new MenuItem("Jungle.Q.Use", "Use Q").SetValue(true));
                menuJungle.AddItem(new MenuItem("Jungle.Q.MinMana", Tab + "Min. Mana Per.:").SetValue(new Slider(20, 1)));
                Config.AddSubMenu(menuJungle);
            }

            var menuAuto = new Menu("Auto", "Auto");
            {
                menuAuto.AddItem(new MenuItem("Auto.Q.UseQHp", "Keep-Up My Heal with Q").SetValue(true));
                menuAuto.AddItem(new MenuItem("Auto.Q.UseQHpMinHp", Tab + "Min. Heal:").SetValue(new Slider(70, 1)));
                menuAuto.AddItem(new MenuItem("Auto.Q.UseQHpEnemyUn", Tab + "Check Enemy Under Turret Position").SetValue(true));

                menuAuto.AddItem(new MenuItem("Auto.E.Title", "E Settings"));
                {
                    menuAuto.AddItem(new MenuItem("Auto.E.Use", Tab + "Auto E < %HP?").SetValue(true));
                    menuAuto.AddItem(new MenuItem("Auto.E.HP", Tab + "HP% : ").SetValue(new Slider(35, 1)));
                }

                Config.AddSubMenu(menuAuto);
            }

            var menuInterrupt = new Menu("Interruptable Target", "Interruptable Target");
            {
                menuInterrupt.AddItem(new MenuItem("Interrupt.R", "Use R").SetValue(true));

                Config.AddSubMenu(menuInterrupt);
            }

            var menuDraw = new Menu("Draw/Notification", "Draw");
            {
                menuDraw.AddItem(new MenuItem("Draw.Disable", "Disable All Drawings").SetValue(false));

                if (PlayerSpells.SmiteSlot != SpellSlot.Unknown)
                {
                    menuDraw.AddItem(
                        new MenuItem("PermaShowSmiteEnemy", "Show Smite Enemy Permashow Status").SetValue(true))
                        .ValueChanged += (s, ar) =>
                        {
                            if (ar.GetNewValue<bool>())
                            {
                                Config.Item("PermaShowSmiteEnemy").Permashow(true, "Smite to Enemy");
                            }
                            else
                            {
                                Config.Item("PermaShowSmiteEnemy").Permashow(false);
                            }
                        };
                    Config.Item("Spells.Smite.Enemy")
                        .Permashow(menuDraw.Item("PermaShowSmiteEnemy").GetValue<bool>(), "Smite to Enemy");

                    menuDraw.AddItem(
                        new MenuItem("PermaShowSmiteMonster", "Show Smite Monster Permashow Status").SetValue(true))
                        .ValueChanged += (s, ar) =>
                        {
                            if (ar.GetNewValue<bool>())
                            {
                                Config.Item("PermaShowSmiteMonster").Permashow(true, "Smite to Monster");
                            }
                            else
                            {
                                Config.Item("PermaShowSmiteMonster").Permashow(false);
                            }
                        };
                    Config.Item("Spells.Smite.Monster")
                        .Permashow(menuDraw.Item("PermaShowSmiteMonster").GetValue<bool>(), "Smite to Monster");
                }

                menuDraw.AddItem(new MenuItem("Draw.Q", "Draw Q").SetValue(new Circle(true, Color.Bisque)));
                menuDraw.AddItem(new MenuItem("Draw.W", "Draw W").SetValue(new Circle(true, Color.Coral)));
                menuDraw.AddItem(new MenuItem("Draw.E", "Draw E").SetValue(new Circle(true, Color.Aqua)));
                menuDraw.AddItem(new MenuItem("Draw.E.Mini", "Draw E on Mini-Map").SetValue(new Circle(true, Color.Aqua)));
                menuDraw.AddItem(new MenuItem("Draw.R", "Draw R").SetValue(new Circle(true, Color.Chartreuse)));

                Config.AddSubMenu(menuDraw);
            }

            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnEndScene += DrawingOnOnEndScene;
            Chat.Print(ChampionName + " <font color='#ff3232'>the Blood Hunter</font> by Mirin Loaded!");

        }

        private static void DrawingOnOnEndScene(EventArgs args)
        {
            if (E.Level > 0)
            {
                var eMini = Config.Item("Draw.E.Mini").GetValue<Circle>();
                if (eMini.Active)
                {
                    LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, E.Range, eMini.Color, 1, 23, true);
                }
            }

        }

        private static bool GetPriorityAllies(string championName)
        {
            if (
                new string[]
                {
                    "Azir", "Olaf", "Renekton", "Shyvana", "Sion", "Skarner", "Thresh", "Volibear", "MonkeyKing",
                    "Yorick", "Aatrox", "Darius", "Diana", "Ekko", "Evelynn", "Fiora", "Fizz", "Gangplank", "Gragas",
                    "Irelia", "Jax", "Jayce", "Kayle", "Kha'Zix", "Lee Sin", "Nocturne", "Nidalee", "Pantheon", "Poppy",
                    "RekSai", "Rengar", "Riven", "Shaco", "Trundle", "Tryndamere", "Udyr", "Urgot", "Vi", "XinZhao",
                    "Yasuo", "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jinx", "Kalista", "Kennen",
                    "KogMaw", "Lucian", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Talon", "Teemo",
                    "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Zed"
                }.Contains(championName))
            {
                return true;
            }
            return false;
        }

        private static void Game_OnUpdate(EventArgs args)
        {

            R.Range = Player.MoveSpeed * 2.5f;

            if (Config.Item("Auto.E.Use").GetValue<bool>() && E.IsReady() && Player.HealthPercent < Config.Item("Auto.E.HP").GetValue<int>())
                E.Cast();

            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                if (Config.Item("Auto.Q.UseQHp").GetValue<bool>() &&
                    Config.Item("Auto.Q.UseQHpMinHp").GetValue<Slider>().Value > Player.HealthPercent)
                {
                    if (Player.HasBuff("Recall", true))
                        return;

                    List<Obj_AI_Base> enemyObjects =
                        ((from obj in
                            MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All,
                                MinionTeam.NotAlly)
                          select obj)
                            .Union
                            (from obj in HeroManager.Enemies
                             where obj.IsValidTarget(Q.Range) && !obj.IsDead && obj.IsZombie
                             select obj)
                            .Union
                            (from obj in
                                MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,
                                    MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                             select obj))
                            .ToList();
                    if (Q.IsReady() && enemyObjects[0] != null)
                    {
                        if (enemyObjects[0] is AIHeroClient && Config.Item("Auto.Q.UseQHpEnemyUn").GetValue<bool>() &&
                            (enemyObjects[0] as AIHeroClient).UnderTurret())
                            return;
                        Q.Cast(enemyObjects[0]);
                    }
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed ||
                Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit ||
                Config.Item("Harass.Q.UseT").GetValue<KeyBind>().Active)
            {
                Harass();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Laneclear();
                JungleClear();
            }
        }

        private static void Interrupter2_OnInterruptableTarget(AIHeroClient unit,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Config.Item("Interrupt.R").GetValue<bool>())
                return;

            if (R.IsReady() && unit.IsValidTarget(R.Range) && !unit.HasBuff("bansheeveil"))
                R.Cast(unit);
        }

        private static float GetComboDamage(Obj_AI_Base t)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(t, SpellSlot.Q);

            if (R.IsReady())
                fComboDamage += Player.GetSpellDamage(t, SpellSlot.R) + Player.TotalAttackDamage;

            return (float)fComboDamage;
        }

        public static int GetClosesAlliesToEnemy(AIHeroClient t)
        {
            if (!t.IsValidTarget(R.Range))
                return 0;

            return (from ally in
                HeroManager.Allies.Where(
                    a =>
                        !a.IsMe && !a.IsDead && !a.IsZombie && a.Distance(t) < 1200 && a.Health > t.Health / 2 &&
                        a.Health > a.Level * 40)
                    let aMov = ally.MoveSpeed
                    let aPos = ally.Position
                    let tPos = t.Position
                    where aPos.Distance(tPos) < aMov * 1.8
                    select aMov).Any()
                ? 1
                : 0;
        }

        private static void Combo()
        {
            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (!t.IsValidTarget())
                return;

            if (R.IsReady())
            {
                var pred = R.GetPrediction(t);
                if (Q.IsReady() && t.Health < Player.GetSpellDamage(t, SpellSlot.Q))
                    return;

                if (t.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65) &&
                    t.Health < Player.TotalAttackDamage)
                    return;

                if (PlayerSpells.IgniteSlot != SpellSlot.Unknown &&
                    Player.Spellbook.CanUseSpell(PlayerSpells.IgniteSlot) == SpellState.Ready &&
                    t.Health < Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite))
                    return;

                if (t.HasBuff("bansheeveil") || t.IsInvulnerable) // don't use R if enemy's banshee is active!
                    return;

                var useR = Config.Item("R.Use").GetValue<StringList>().SelectedIndex;
                switch (useR)
                {
                    case 1:
                        {
                            if (t.IsValidTarget(R.Range))
                            {
                                R.Cast(pred.CastPosition);
                            }
                            break;
                        }
                    case 2:
                        {
                            if (t.IsValidTarget(R.Range) &&
                                t.Health <
                                GetComboDamage(t) + Player.TotalAttackDamage +
                                (Q.IsReady() ||
                                 Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires < 1.8 &&
                                 Player.Mana > Player.Spellbook.GetSpell(SpellSlot.Q).SData.Mana + Player.Spellbook.GetSpell(SpellSlot.R).SData.Mana
                                    ? Player.GetSpellDamage(t, SpellSlot.Q)
                                    : 0))
                                R.Cast(pred.CastPosition);
                            break;
                        }
                    case 3:
                        {
                            if (t.IsValidTarget(R.Range) &&
                                ((t.Health <
                                  GetComboDamage(t) + Player.TotalAttackDamage +
                                  (Q.IsReady() ||
                                   Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires < 1.8 &&
                                   Player.Mana > Player.Spellbook.GetSpell(SpellSlot.Q).SData.Mana + Player.Spellbook.GetSpell(SpellSlot.R).SData.Mana
                                      ? Player.GetSpellDamage(t, SpellSlot.Q)
                                      : 0)) || t.CountAlliesInRange(800) >= 2 ||
                                 GetClosesAlliesToEnemy(t) > 0))
                                R.Cast(pred.CastPosition);
                            break;
                        }
                }
            }

            if (Q.IsReady() && t.IsValidTarget(Q.Range))
            {
                Q.Cast(t);
            }

            if (E.IsReady() && t.IsValidTarget(E.Range - 25))
            {
                E.Cast();
            }

            CastItems(t);
        }

        private static void Harass()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (!t.IsValidTarget())
                return;

            if (Config.Item("Harass.Q.UseTEnemyUn").GetValue<bool>() && t.UnderTurret())
                return;

            if (Player.ManaPercent < Config.Item("Harass.Q.MinMana").GetValue<Slider>().Value)
                return;

            if (Q.IsReady() && Config.Item("Harass.Q.Use").GetValue<bool>())
            {
                Q.Cast(t);
            }
        }

        private static void Laneclear()
        {
            var useQ = Q.IsReady() && Player.ManaPercent > Config.Item("Lane.Q.MinMana").GetValue<Slider>().Value;

            var qSelectedIndex = Config.Item("Lane.Q.Use").GetValue<StringList>().SelectedIndex;

            var qMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (qMinions.Count == 0)
                return;

            if (useQ)
            {
                switch (qSelectedIndex)
                {
                    case 1:
                        {
                            if (Q.GetDamage(qMinions[0]) > qMinions[0].Health)
                                Q.Cast(qMinions[0]);
                            break;
                        }

                    case 2:
                        {
                            if (Q.GetDamage(qMinions[0]) > qMinions[0].Health &&
                                Player.Distance(qMinions[0]) > Orbwalking.GetRealAutoAttackRange(null) + 65)
                                Q.Cast(qMinions[0]);
                            break;
                        }

                    case 3:
                        {
                            Q.Cast(qMinions[0]);
                            break;
                        }
                }
            }
        }

        private static void JungleClear()
        {
            var useQ = Q.IsReady() && Player.ManaPercent > Config.Item("Jungle.Q.MinMana").GetValue<Slider>().Value &&
                       Config.Item("Jungle.Q.Use").GetValue<bool>();

            var qMobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range + 300, MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (qMobs.Count == 0)
                return;

            if (useQ)
            {
                var jungleMinions = new string[]
                {
                    "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon",
                    "SRU_Baron", "Sru_Crab"
                };

                var xMobs =
                    jungleMinions.FirstOrDefault(
                        name => qMobs[0].Name.Substring(0, qMobs[0].Name.Length - 5).Equals(name));

                if (xMobs != null)
                {
                    if (qMobs[0].IsValidTarget(Q.Range))
                    {
                        Q.Cast(qMobs[0]);
                    }
                }
                else
                {
                    Q.Cast(qMobs[0]);
                }
            }
        }

        private static void CastItems(AIHeroClient t)
        {
            foreach (var item in ItemDb)
            {
                if (item.Value.ItemType == EnumItemType.AoE &&
                    item.Value.TargetingType == EnumItemTargettingType.EnemyHero)
                {
                    if (t.IsValidTarget(item.Value.Item.Range) && item.Value.Item.IsReady())
                        item.Value.Item.Cast();
                }
                if (item.Value.ItemType == EnumItemType.Targeted &&
                    item.Value.TargetingType == EnumItemTargettingType.EnemyHero)
                {
                    if (t.IsValidTarget(item.Value.Item.Range) && item.Value.Item.IsReady())
                        item.Value.Item.Cast(t);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("Draw.Disable").GetValue<bool>())
                return;

            if (Config.Item("Draw.Disable").GetValue<bool>())
                return;

            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item("Draw." + spell.Slot).GetValue<Circle>();
                if (menuItem.Active && spell.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color, 2);
            }
        }
    }
}