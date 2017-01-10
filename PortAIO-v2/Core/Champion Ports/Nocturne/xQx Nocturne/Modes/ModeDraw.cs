using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp.Common;
using Nocturne.Champion;
using Nocturne.Common;
using PortAIO.Properties;
using SharpDX;
using Color = System.Drawing.Color;
using CommonGeometry = Nocturne.Common.CommonGeometry;

using EloBuddy; 
using LeagueSharp.Common; 
namespace Nocturne.Modes
{
    using System.Linq;
    using LeagueSharp;
    internal class NocturneQ
    {
        public GameObject Object { get; set; }
        public float NetworkId { get; set; }
        public Vector3 QPos { get; set; }
        public double ExpireTime { get; set; }
    }

    internal class NocturneUnspeakableHorror
    {
        public static float StartTime { get; set; }
        public static float EndTime { get; set; }

    }
    internal class NocturneParanoia
    {
        public static float StartTime { get; set; }
        public static float EndTime { get; set; }
    }

    internal class BlueBuff
    {
        public static float StartTime { get; set; }
        public static float EndTime { get; set; }
    }

    internal class RedBuff
    {
        public static float StartTime { get; set; }
        public static float EndTime { get; set; }
    }

    enum PcMode
    {
        NewComputer,
        NormalComputer,
        OldComputer
    }

    internal class ModeDraw
    {
        public static Menu MenuLocal { get; private set; }
        public static Menu SubMenuSpells { get; private set; }
        public static Menu SubMenuBuffs { get; private set; }
        public static Menu SubMenuTimers { get; private set; }
        public static Menu SubMenuManaBarIndicator { get; private set; }
        private static Spell Q => PlayerSpells.Q;
        private static Spell W => PlayerSpells.W;
        private static Spell E => PlayerSpells.E;
        private static Spell R => PlayerSpells.R;
        public static PcMode PcMode { get; set; }

        private static readonly List<MenuItem> MenuLocalSubMenuItems = new List<MenuItem>();
        private static readonly string[] pcMode = new[] { "newpc.", "oldpc." };
 

        private static readonly List<NocturneQ> NocturneQ = new List<NocturneQ>();
        public static void Init(Menu ParentMenu)
        {
            MenuLocal = new Menu("Drawings", "Drawings");
            {

                MenuLocal.AddItem(
                 new MenuItem("DrawPc.Mode", "Adjust settings to your own computer:").SetValue(new StringList(new[] { "New Computer", "Old Computer" }, 0)).SetFontStyle(FontStyle.Regular, SharpDX.Color.Coral)).ValueChanged +=
                 (sender, args) =>
                 {
                     InitRefreshMenuItems();
                 };

                SubMenuManaBarIndicator = new Menu("Mana Bar Combo Indicator", "ManaBarIndicator");
                {
                    for (int i = 0; i < 2; i++)
                    {
                        SubMenuManaBarIndicator.AddItem(new MenuItem(pcMode[i] + "DrawManaBar.Q", "Q:").SetValue(true).SetFontStyle(FontStyle.Regular, Q.MenuColor()));
                        SubMenuManaBarIndicator.AddItem(new MenuItem(pcMode[i] + "DrawManaBar.W", "W:").SetValue(true).SetFontStyle(FontStyle.Regular, W.MenuColor()));
                        SubMenuManaBarIndicator.AddItem(new MenuItem(pcMode[i] + "DrawManaBar.E", "E:").SetValue(true).SetFontStyle(FontStyle.Regular, E.MenuColor()));
                        SubMenuManaBarIndicator.AddItem(new MenuItem(pcMode[i] + "DrawManaBar.R", "R:").SetValue(true).SetFontStyle(FontStyle.Regular, R.MenuColor()));
                    }
                    MenuLocal.AddSubMenu(SubMenuManaBarIndicator);
                }


                SubMenuSpells = new Menu("Spells", "DrawSpells");
                {
                    for (int i = 0; i < 2; i++)
                    {
                        SubMenuSpells.AddItem(new MenuItem(pcMode[i] + "DrawQRange", "Q:").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))).SetFontStyle(FontStyle.Regular, Q.MenuColor()));
                        SubMenuSpells.AddItem(new MenuItem(pcMode[i] + "DrawERange", "E:").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))).SetFontStyle(FontStyle.Regular, E.MenuColor()));
                        SubMenuSpells.AddItem(new MenuItem(pcMode[i] + "DrawRRange", "R:").SetValue(new StringList(new[] { "Off", "Map", "Mini Map", "Both" }, 3)).SetFontStyle(FontStyle.Regular, R.MenuColor()));
                    }
                    MenuLocal.AddSubMenu(SubMenuSpells);
                }

                SubMenuBuffs = new Menu("Buffs", "DrawBuffs");
                {
                    for (int i = 0; i < 2; i++)
                    {
                        SubMenuBuffs.AddItem(new MenuItem(pcMode[i] + "DrawQBuff", "Q: Buff Status").SetValue(new StringList(new[] {"Off", "On"}, 1)).SetFontStyle(FontStyle.Regular, Q.MenuColor()));
                        SubMenuBuffs.AddItem(new MenuItem(pcMode[i] + "DrawBuffs", "Show Jungle Buff Time Circle").SetValue(new StringList(new[] {"Off", "Blue Buff", "Red Buff", "Both"}, 3)));
                    }
                    MenuLocal.AddSubMenu(SubMenuBuffs);
                }

                SubMenuTimers = new Menu("Timers", "DrawTimers");
                {
                    for (int i = 0; i < 2; i++)
                    {
                        SubMenuTimers.AddItem(new MenuItem(pcMode[i] + "DrawEStatus", "E: Show Target Horror Time Circle").SetValue(new StringList(new[] { "Off", "On" }, 1)).SetFontStyle(FontStyle.Regular, E.MenuColor()));
                        SubMenuTimers.AddItem(new MenuItem(pcMode[i] + "DrawRStatus", "R: Show Ultimate Time Circle").SetValue(new StringList(new[] { "Off", "On" }, 1)).SetFontStyle(FontStyle.Regular, R.MenuColor()));
                    }
                    MenuLocal.AddSubMenu(SubMenuTimers);

                }

                for (int i = 0; i < 2; i++)
                {
                    MenuLocal.AddItem(new MenuItem(pcMode[i] + "DrawKillableEnemy", "Killable Enemy Notification").SetValue(true));
                    MenuLocal.AddItem(new MenuItem(pcMode[i] + "DrawKillableEnemyMini", "Killable Enemy [Mini Map]").SetValue(new Circle(true, Color.GreenYellow)));
                }

                for (int i = 0; i < 2; i++)
                {
                    MenuLocal.AddItem(new MenuItem(pcMode[i] + "DrawMinionLastHist", "Draw Minion Last Hit").SetValue(new Circle(true, Color.GreenYellow)));
                }


                for (int i = 0; i < 2; i++)
                {
                    var dmgAfterComboItem = new MenuItem(pcMode[i] + "DrawDamageAfterCombo", "Combo Damage").SetValue(true);
                    {
                        MenuLocal.AddItem(dmgAfterComboItem);

                        //LeagueSharp.Common.Utility.HpBar//DamageIndicator.DamageToUnit = Common.CommonMath.GetComboDamage;
                        //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
                        dmgAfterComboItem.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
                        {
                            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                        };
                    }
                }
            }

            CommonManaBar.Init(MenuLocal);

            ParentMenu.AddSubMenu(MenuLocal);
            InitRefreshMenuItems();

            //Sprite.Init();

            Game.OnUpdate += GameOnOnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;

            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += DrawingOnOnEndScene;
            

            //Obj_AI_Base.OnBuffGain += (sender, args) =>
            //{
            //    if (sender.IsMe)
            //    Chat.Print(args.Buff.DisplayName.ToString());
            //    //if (args.Buff.DisplayName == "BlessingoftheLizardElder") //red

            //    if (args.Buff.DisplayName == "CrestoftheAncientGolem")
            //    {

            //        if (BlueBuff.EndTime < Game.Time)
            //        {
            //            BlueBuff.StartTime = args.Buff.StartTime;
            //            BlueBuff.EndTime = args.Buff.EndTime;
            //        }
            //    }
            //};
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            // NocturneParanoia
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() &&
                MenuLocal.Item(GetPcModeStringValue + "DrawEStatus").GetValue<StringList>().SelectedIndex == 1 & t.HasNocturneUnspeakableHorror())
            {
                BuffInstance b = t.Buffs.Find(buff => buff.DisplayName == "NocturneUnspeakableHorror");
                if (NocturneUnspeakableHorror.EndTime < Game.Time || b.EndTime > NocturneUnspeakableHorror.EndTime)
                {
                    NocturneUnspeakableHorror.StartTime = b.StartTime;
                    NocturneUnspeakableHorror.EndTime = b.EndTime - 1;
                }
            }

            //foreach (var b in ObjectManager.Player.Buffs.Where(b => b.DisplayName.Contains("NocturneParanoiaTarget")))
            //{

            //    Console.WriteLine(b.DisplayName + " : " + b.StartTime + " : " + b.EndTime + " : " + Game.Time);
            //}



            if (MenuLocal.Item(GetPcModeStringValue + "DrawRStatus").GetValue<StringList>().SelectedIndex == 1 && ObjectManager.Player.HasNocturneParanoia())
            {
                BuffInstance b = ObjectManager.Player.Buffs.Find(buff => buff.DisplayName == "NocturneParanoiaTarget");
                if (NocturneParanoia.EndTime < Game.Time || b.EndTime > NocturneParanoia.EndTime)
                {
                    NocturneParanoia.StartTime = b.StartTime;
                    NocturneParanoia.EndTime = b.EndTime;
                }
            }


            var drawBuffs = MenuLocal.Item(GetPcModeStringValue + "DrawBuffs").GetValue<StringList>().SelectedIndex;
            if ((drawBuffs == 1 | drawBuffs == 3) && ObjectManager.Player.HasBlueBuff())
            {
                BuffInstance b = ObjectManager.Player.Buffs.Find(buff => buff.DisplayName == "CrestoftheAncientGolem");
                if (BlueBuff.EndTime < Game.Time || b.EndTime > BlueBuff.EndTime)
                {
                    BlueBuff.StartTime = b.StartTime;
                    BlueBuff.EndTime = b.EndTime;
                }
            }

            if ((drawBuffs == 2 | drawBuffs == 3) && ObjectManager.Player.HasRedBuff())
            {
                BuffInstance b = ObjectManager.Player.Buffs.Find(buff => buff.DisplayName == "BlessingoftheLizardElder");
                if (RedBuff.EndTime < Game.Time || b.EndTime > RedBuff.EndTime)
                {
                    RedBuff.StartTime = b.StartTime;
                    RedBuff.EndTime = b.EndTime;
                }
            }
        }

        private static MenuItem GetMenuItems(Menu menu)
        {
            foreach (var j in menu.Children.Cast<Menu>().SelectMany(GetMenu).SelectMany(i => i.Items))
            {
                MenuLocalSubMenuItems.Add(j);
            }

            foreach (var j in menu.Items)
            {
                MenuLocalSubMenuItems.Add(j);
            }
            return null;
        }
        private static IEnumerable<Menu> GetMenu(Menu menu)
        {
            yield return menu;

            foreach (var childChild in menu.Children.SelectMany(GetMenu))
                yield return childChild;
        }
        public static PcMode GetPcModeEnum
        {
            get
            {
                if (MenuLocal.Item("DrawPc.Mode").GetValue<StringList>().SelectedIndex == 0)
                {
                    return PcMode.NewComputer;
                }

                if (MenuLocal.Item("DrawPc.Mode").GetValue<StringList>().SelectedIndex == 1)
                {
                    return PcMode.NormalComputer;
                }

                if (MenuLocal.Item("DrawPc.Mode").GetValue<StringList>().SelectedIndex == 2)
                {
                    return PcMode.OldComputer;
                }

                return PcMode.NormalComputer;
            }
        }

        public static string GetPcModeStringValue => pcMode[MenuLocal.Item("DrawPc.Mode").GetValue<StringList>().SelectedIndex];

        private static void InitRefreshMenuItems()
        {
            int argsValue = MenuLocal.Item("DrawPc.Mode").GetValue<StringList>().SelectedIndex;
            MenuLocalSubMenuItems.Clear();
            GetMenuItems(MenuLocal);

            foreach (var item in MenuLocalSubMenuItems)
            {
                item.Show(true);
                switch (argsValue)
                {
                    case 0:
                        if (!item.Name.StartsWith("newpc.") && !item.Name.StartsWith("DrawPc.Mode"))
                        {
                            item.Show(false);
                        }
                        break;
                    case 1:
                        if (!item.Name.StartsWith("oldpc.") && !item.Name.StartsWith("DrawPc.Mode"))
                        {
                            item.Show(false);
                        }
                        break;
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            DrawSpells();
            DrawMinionLastHit();
            KillableEnemy();
            DrawQBuffStatus();
            DrawBuffs();
            DrawRStatus();
            DrawHorror();
        }

        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.Name.Contains("NocturneDuskbringer_path_green.troy"))
            {
                NocturneQ nocX = new NocturneQ
                {
                    QPos = obj.Position,
                    ExpireTime = Game.Time + 8,
                    NetworkId = obj.NetworkId,
                    Object = obj
                };
                NocturneQ.Add(nocX);
            }
        }

        private static void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.Name == "NocturneDuskbringer_path_green.troy")
            {
                NocturneQ.Clear();
            }
        }

        private static void DrawQBuffStatus()
        {
            if (Modes.ModeConfig.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
            {
                return;
            }

            if (MenuLocal.Item(GetPcModeStringValue + "DrawQBuff").GetValue<StringList>().SelectedIndex == 1)
            {
                foreach (var o in NocturneQ)
                {
                    Render.Circle.DrawCircle(o.QPos, 135f,
                        ObjectManager.Player.HasBuffInst("NocturneDuskbringer") ? Color.White : Color.DarkSlateGray, 1);
                }
            }
        }

        private static void DrawHorror()
        {

            //foreach (var obj in ObjectManager.Get<Obj_AI_Base>().Where(o => !o.IsDead))
            //{
            //    if (obj.IsValidTarget(E.Range) && MenuLocal.Item(GetPcModeStringValue + "DrawEStatus").GetValue<StringList>().SelectedIndex == 1 & obj.HasNocturneUnspeakableHorror())
            //    {
            //        if (NocturneUnspeakableHorror.EndTime >= Game.Time)
            //        {
            //            var circle =
            //                new CommonGeometry.Circle2(obj.Position.To2D(), 180f,
            //                    Game.Time*100 - NocturneUnspeakableHorror.StartTime*100,
            //                    NocturneUnspeakableHorror.EndTime*100 - NocturneUnspeakableHorror.StartTime*100)
            //                    .ToPolygon();
            //            circle.Draw(Color.Red, 5);
            //            // Drawing.DrawCircle(ObjectManager.Player.Position, 180f, System.Drawing.Color.LightCoral);
            //        }
            //    }

            //}
            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget() && MenuLocal.Item(GetPcModeStringValue + "DrawEStatus").GetValue<StringList>().SelectedIndex == 1 & t.HasNocturneUnspeakableHorror())
            {
                if (NocturneUnspeakableHorror.EndTime >= Game.Time)
                {
                    var circle = new CommonGeometry.Circle2(t.Position.To2D(), 180f, Game.Time * 100 - NocturneUnspeakableHorror.StartTime * 100, NocturneUnspeakableHorror.EndTime * 100 - NocturneUnspeakableHorror.StartTime * 100).ToPolygon();
                    circle.Draw(Color.Red, 3);
                    // Drawing.DrawCircle(ObjectManager.Player.Position, 180f, System.Drawing.Color.LightCoral);
                }
            }

        }
        private static void DrawRStatus()
        {
            if (MenuLocal.Item(GetPcModeStringValue + "DrawRStatus").GetValue<StringList>().SelectedIndex == 1 && ObjectManager.Player.HasNocturneParanoia())
            {
                if (NocturneParanoia.EndTime >= Game.Time)
                {
                    var circle1 = new CommonGeometry.Circle2(new Vector2(ObjectManager.Player.Position.X + 3, ObjectManager.Player.Position.Y - 3), 220f, Game.Time * 100 - NocturneParanoia.StartTime * 100, NocturneParanoia.EndTime * 100 - NocturneParanoia.StartTime * 100).ToPolygon();
                    circle1.Draw(Color.Black, 4);

                    var circle = new CommonGeometry.Circle2(ObjectManager.Player.Position.To2D(), 220f, Game.Time * 100 - NocturneParanoia.StartTime * 100, NocturneParanoia.EndTime * 100 - NocturneParanoia.StartTime * 100).ToPolygon();
                    circle.Draw(Color.DarkOrange, 4);
                    // Drawing.DrawCircle(ObjectManager.Player.Position, 180f, System.Drawing.Color.LightCoral);
                }
            }
        }

        private static void DrawBuffs()
        {
            var drawBuffs = MenuLocal.Item(GetPcModeStringValue + "DrawBuffs").GetValue<StringList>().SelectedIndex;

            if ((drawBuffs == 1 | drawBuffs == 3) && ObjectManager.Player.HasBlueBuff() )
            {
                if (BlueBuff.EndTime >= Game.Time)
                {
                    var circle1 = new CommonGeometry.Circle2(new Vector2(ObjectManager.Player.Position.X + 3, ObjectManager.Player.Position.Y - 3), 190f, Game.Time - BlueBuff.StartTime, BlueBuff.EndTime - BlueBuff.StartTime).ToPolygon();
                    circle1.Draw(Color.Black, 4);

                    var circle = new CommonGeometry.Circle2(ObjectManager.Player.Position.To2D(), 190f, Game.Time - BlueBuff.StartTime, BlueBuff.EndTime - BlueBuff.StartTime ).ToPolygon();
                    circle.Draw(Color.DarkBlue, 4);
                 //   Drawing.DrawCircle(ObjectManager.Player.Position, 150f, System.Drawing.Color.DarkBlue);
                }
            }

            if ((drawBuffs == 2 || drawBuffs == 3) && ObjectManager.Player.HasRedBuff())
            {
                if (RedBuff.EndTime >= Game.Time)
                {
                    var circle1 = new CommonGeometry.Circle2(new Vector2(ObjectManager.Player.Position.X + 3, ObjectManager.Player.Position.Y - 3), 160f, Game.Time - RedBuff.StartTime, RedBuff.EndTime - RedBuff.StartTime).ToPolygon();
                    circle1.Draw(Color.Black, 4);

                    var circle = new CommonGeometry.Circle2(ObjectManager.Player.Position.To2D(), 160f, Game.Time - RedBuff.StartTime, RedBuff.EndTime - RedBuff.StartTime).ToPolygon();
                    circle.Draw(Color.DarkRed, 4);
                    //Drawing.DrawCircle(ObjectManager.Player.Position, 130f, System.Drawing.Color.IndianRed);
                }
            }
        }

        private static void DrawingOnOnEndScene(EventArgs args)
        {
            if (MenuLocal.Item(GetPcModeStringValue + "DrawRRange").GetValue<StringList>().SelectedIndex == 2 || MenuLocal.Item(GetPcModeStringValue + "DrawRRange").GetValue<StringList>().SelectedIndex == 3)
            {
            #pragma warning disable 618
                LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.GreenYellow: Color.DimGray, thickness: 1, quality: 23, onMinimap: true);
            #pragma warning restore 618
            }

            var drawKillableEnemyMini = MenuLocal.Item(GetPcModeStringValue + "DrawKillableEnemyMini").GetValue<Circle>();
            if (drawKillableEnemyMini.Active)
            {
                foreach (
                    var e in
                        HeroManager.Enemies.Where(
                            e => e.IsVisible && !e.IsDead && !e.IsZombie && e.Health < Common.CommonMath.GetComboDamage(e)))
                {
                    if ((int) Game.Time%2 == 1)
                    {
                        #pragma warning disable 618
                        LeagueSharp.Common.Utility.DrawCircle(e.Position, 850, drawKillableEnemyMini.Color, 2, 30, true);
                        #pragma warning restore 618
                    }
                }
            }
        }

        private static void DrawSpells()
        {
            var drawQ = MenuLocal.Item(GetPcModeStringValue + "DrawQRange").GetValue<Circle>();
            if (drawQ.Active && Q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? drawQ.Color : Color.LightGray, Q.IsReady() ? 5: 1);
            }

            var drawE = MenuLocal.Item(GetPcModeStringValue + "DrawERange").GetValue<Circle>();
            if (drawE.Active && E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? drawE.Color: Color.LightGray, E.IsReady() ? 5 : 1);
            }

            var drawR = MenuLocal.Item(GetPcModeStringValue + "DrawRRange").GetValue<StringList>().SelectedIndex;
            if ((drawR == 1 || drawR == 3) && R.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? drawE.Color : Color.LightGray, R.IsReady() ? 5 : 1);
            }
        }

        public static AIHeroClient GetKillableEnemy
        {
            get
            {
                if (MenuLocal.Item(GetPcModeStringValue + "DrawKillableEnemy").GetValue<bool>())
                {
                    return HeroManager.Enemies.FirstOrDefault(e => e.IsVisible && !e.IsDead && !e.IsZombie && e.Health < Common.CommonMath.GetComboDamage(e));
                }
                return null;
            }
        }

        private static void KillableEnemy()
        {
            if (MenuLocal.Item(GetPcModeStringValue + "DrawKillableEnemy").GetValue<bool>())
            {
                var t = KillableEnemyAa;
                if (t.Item1 != null && t.Item1.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 800) && t.Item2 > 0)
                {
                    //CommonHelper.DrawText(CommonHelper.Text, $"{t.Item1.ChampionName}: {t.Item2} x AA Damage = Kill", (int)t.Item1.HPBarPosition.X + 65, (int)t.Item1.HPBarPosition.Y + 5, SharpDX.Color.White);
                    CommonHelper.DrawText(CommonHelper.Text, $"{t.Item1.ChampionName}: {t.Item2} Combo = Kill", (int)t.Item1.HPBarPosition.X + 85, (int)t.Item1.HPBarPosition.Y + 5, SharpDX.Color.GreenYellow);
                }
            }
        }
        private static void DrawMinionLastHit()
        {
            var drawMinionLastHit = MenuLocal.Item(GetPcModeStringValue + "DrawMinionLastHist").GetValue<Circle>();
            if (drawMinionLastHit.Active)
            {
                foreach (
                    var xMinion in
                        MinionManager.GetMinions(
                            ObjectManager.Player.Position,
                            ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + 300,
                            MinionTypes.All,
                            MinionTeam.Enemy,
                            MinionOrderTypes.MaxHealth)
                            .Where(xMinion => ObjectManager.Player.GetAutoAttackDamage(xMinion, true) >= xMinion.Health))
                {
                    Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius, drawMinionLastHit.Color);
                }
            }
        }
        private static Tuple<AIHeroClient, int> KillableEnemyAa
        {
            get
            {
                var x = 0;
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                {
                    if (t.IsValidTarget())
                    {
                        //if (t.Health < ObjectManager.Player.TotalAttackDamage * (1 / ObjectManager.Player.AttackCastDelay > 1500 ? 12 : 8))
                            if (t.Health < Common.CommonMath.GetComboDamage(t))
                            {
                            x = (int)Math.Ceiling(t.Health / ObjectManager.Player.TotalAttackDamage);
                        }
                        return new Tuple<AIHeroClient, int>(t, x);
                    }

                }
                return new Tuple<AIHeroClient, int>(t, x);
            }
        }
    }

    internal class Sprite
    {
        private static Spell Q => PlayerSpells.Q;

        private static Vector2 DrawPosition
        {
            get
            {
                var drawStatus = CommonTargetSelector.MenuLocal.Item("Draw.Status").GetValue<StringList>().SelectedIndex;
                if (KillableEnemy == null || (drawStatus != 2 && drawStatus != 3))
                    return new Vector2(0f, 0f);

                return new Vector2(KillableEnemy.HPBarPosition.X + KillableEnemy.BoundingRadius / 2f,
                    KillableEnemy.HPBarPosition.Y - 70);
            }
        }

        private static bool DrawSprite => true;

        private static AIHeroClient KillableEnemy
        {
            get
            {
                var t = ModeDraw.GetKillableEnemy;

                if (t.IsValidTarget())
                    return t;

                return null;
            }
        }

        internal static void Init()
        {
            new Render.Sprite(Resources.killableenemy, new Vector2())
            {
                PositionUpdate = () => DrawPosition,
                Scale = new Vector2(1f, 1f),
                VisibleCondition = sender => DrawSprite
            }.Add();
        }
    }
}
