using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace EasyPeasyRivenSqueezy
{
    /// <summary>
    ///     Manages Riven's stuff, such as Passive, Q, Ult, etc., and loads stuff :D
    /// </summary>
    internal class Riven
    {
        //TODO: organize this shit
        public static int QCount
        {
            get
            {
                return Player.HasBuff("RivenTriCleave", true)
                    ? Player.Buffs.FirstOrDefault(x => x.Name == "RivenTriCleave").Count
                    : 0;
            }
        }

        public static Menu Menu { get; set; }
        public static Orbwalking.Orbwalker Orbwalker { get; set; }

        public static float EWRange
        {
            get { return E.Range + W.Range/2 + Player.BoundingRadius; }
        }

        public static Spell Q { get; internal set; }
        public static Spell W { get; internal set; }
        public static Spell E { get; internal set; }
        public static Spell R { get; internal set; }
        public static Spell Ignite { get; internal set; }
        public static int LastQ { get; internal set; }

        public static bool RActivated
        {
            get { return Player.HasBuff("RivenFengShuiEngine", true); }
        }

        public static bool CanWindSlash
        {
            get { return Player.HasBuff("rivenwindslashready", true); }
        }

        public static float PassiveDmg
        {
            get
            {
                double[] dmgMult = {0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5};
                var dmg = (Player.FlatPhysicalDamageMod + Player.BaseAttackDamage)*dmgMult[Player.Level/3];
                return (float) dmg;
            }
        }

        public static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        public static int QDelay { get; set; }
        public static Obj_AI_Base LastTarget { get; set; }
        public static bool CanQ { get; set; }

        public static void OnGameLoad()
        {
            if (Player.ChampionName != "Riven")
            {
                return; // This guy is missing out..
            }

            CreateMenu();

            Q = new Spell(SpellSlot.Q, 260) {Delay = 0.5f};
            W = new Spell(SpellSlot.W, 260);
            E = new Spell(SpellSlot.E, 250) {Delay = 0.3f, Speed = 1450};
            R = new Spell(SpellSlot.R, 1100);
            Ignite = new Spell(Player.GetSpellSlot("summonerdot"), 600);

            Q.SetSkillshot(0.5f, 100, 1400, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.25f, 150, 2200, false, SkillshotType.SkillshotCone);

            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Game.OnUpdate += RivenCombo.OnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2OnOnInterruptableTarget;

            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = true;
            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.DamageToUnit = RivenCombo.GetDamage;

            NotificationHandler.ShowWelcome();

            Chat.Print("<font color=\"#7CFC00\"><b>EasyPeasyRivenSqueezy:</b></font> Loaded");
        }

        private static void Interrupter2OnOnInterruptableTarget(AIHeroClient sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsValidTarget(EWRange) || !GetBool("InterruptEW"))
            {
                NotificationHandler.ShowInterrupterAlert(false);
                return;
            }

            E.Cast(sender.Position);
            W.Cast();

            NotificationHandler.ShowInterrupterAlert(true);
        }

        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget() || !GetBool("HandleGapclosers"))
            {
                return;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && RivenCombo.CanHardEngage(gapcloser.Sender))
            {
                R.Cast();
                RivenCombo.CastCircleThing();
                W.Cast();
            }
            else if (gapcloser.Sender.IsValidTarget(Q.Range + Player.BoundingRadius) && QCount == 2)
            {
                Q.Cast(gapcloser.Sender.ServerPosition);
            }
            else if (gapcloser.Sender.IsValidTarget(W.Range))
            {
                W.Cast();
            }

            NotificationHandler.ShowGapcloserAlert();
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawEngage = Menu.Item("DrawEngageRange").GetValue<bool>();
            var drawQ = Menu.Item("DrawQ").GetValue<bool>();
            var drawW = Menu.Item("DrawW").GetValue<bool>();
            var drawE = Menu.Item("DrawE").GetValue<bool>();
            var drawR = Menu.Item("DrawR").GetValue<bool>();
            var p = Player.Position;

            if (drawEngage)
            {
                Render.Circle.DrawCircle(p, EWRange, E.IsReady() && W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(p, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.SData.Name == "RivenFengShuiEngine" && GetBool("KeepRAlive"))
            {
                LeagueSharp.Common.Utility.DelayAction.Add(
                    (int) (15000 - Game.Ping/2 - R.Delay*1000), delegate
                    {
                        if (CanWindSlash)
                        {
                            var bestTarget =
                                ObjectManager.Get<AIHeroClient>()
                                    .Where(x => x.IsValidTarget(R.Range))
                                    .OrderBy(x => x.Health)
                                    .FirstOrDefault();

                            if (bestTarget != null)
                            {
                                R.Cast(bestTarget);
                            }
                        }
                    });
            }
        }

        public static bool GetBool(string item)
        {
            return Menu.Item(item).GetValue<bool>();
        }

        private static void CreateMenu()
        {
            (Menu = new Menu("EasyPeasy雷玟", "cmEZPZ_Riven", true)).AddToMainMenu();

            var orbwalkMenu = new Menu("走砍", "orbwalk");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            var tsMenu = new Menu("目標選擇器", "ts");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            var comboMenu = new Menu("連招", "combo_kek");
            var comboSpellsMenu = new Menu("技能", "combo_spells");
            comboSpellsMenu.AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            comboSpellsMenu.AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            comboSpellsMenu.AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            comboSpellsMenu.AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
            comboMenu.AddSubMenu(comboSpellsMenu);

            comboMenu.AddItem(
                new MenuItem("UseROption", "和時使用R").SetValue(
                    new StringList(new[] {"Hard", "Easy", "Probably"})));
            comboMenu.AddItem(
                new MenuItem("UseRPercent", "當敵人血量低於%時不使用R").SetValue(new Slider(1, 1)));
            comboMenu.AddItem(
                new MenuItem("UseRIfCantCancel", "仍然使用R如果無法取消動畫").SetValue(false));
            comboMenu.AddItem(new MenuItem("QExtraDelay", "額外 Q 延遲").SetValue(new Slider(0, 0, 1000)));
            comboMenu.AddItem(new MenuItem("DontEIntoWall", "不要有E著牆壁").SetValue(true));
            comboMenu.AddItem(new MenuItem("DontEInAARange", "當敵人目標在您的普攻範圍內，不使用E").SetValue(true));
            comboMenu.AddItem(new MenuItem("GapcloseQ", "Q 防突進").SetValue(true));
            comboMenu.AddItem(new MenuItem("GapcloseE", "E 防突進").SetValue(true));
            comboMenu.AddItem(new MenuItem("FollowTarget", "跟隨目標").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("騷擾", "harass");
            harassMenu.AddItem(new MenuItem("UseQMixed", "使用 Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseWMixed", "使用 W").SetValue(true));
            harassMenu.AddItem(new MenuItem("UseEMixed", "使用 E").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            var farmMenu = new Menu("清線", "cmWC");
            var farmSpellsMenu = new Menu("技能", "farm_spells");
            farmSpellsMenu.AddItem(new MenuItem("UseQWaveClear", "使用 Q").SetValue(true));
            farmSpellsMenu.AddItem(new MenuItem("UseWWaveClear", "使用 W").SetValue(true));
            farmSpellsMenu.AddItem(new MenuItem("UseEWaveClear", "使用 E").SetValue(true));
            farmMenu.AddSubMenu(farmSpellsMenu);

            farmMenu.AddItem(new MenuItem("UseFastQ", "使用快速 Q").SetValue(true));
            farmMenu.AddItem(new MenuItem("UseItems", "使用物品").SetValue(false));
            Menu.AddSubMenu(farmMenu);

            var miscMenu = new Menu("雜項", "cmMisc");
            miscMenu.AddItem(new MenuItem("KeepQAlive", "保持 Q 靈活").SetValue(true));
            miscMenu.AddItem(new MenuItem("KeepRAlive", "保持 R 靈活").SetValue(true));
            miscMenu.AddItem(new MenuItem("HandleGapclosers", "應付防突進").SetValue(true));
            miscMenu.AddItem(new MenuItem("InterruptEW", "中斷技能使用EW").SetValue(true));
            miscMenu.AddItem(new MenuItem("IgniteKillable", "當方可擊殺使用點燃").SetValue(true));
            miscMenu.AddItem(new MenuItem("IgniteKS", "點燃搶頭").SetValue(true));
            miscMenu.AddItem(new MenuItem("Notifications", "使用通知").SetValue(true));
            miscMenu.AddItem(new MenuItem("CancelQAnimation", "阻擋 Q 動畫").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            var fleeMenu = new Menu("逃跑", "cmFlee");
            fleeMenu.AddItem(new MenuItem("UseQFlee", "使用 Q").SetValue(true));
            fleeMenu.AddItem(new MenuItem("UseEFlee", "使用 E").SetValue(true));
            fleeMenu.AddItem(new MenuItem("UseGattaGoFast", "使用妖夢鬼刀").SetValue(true));
            fleeMenu.AddItem(new MenuItem("FleeActive", "逃跑!").SetValue(new KeyBind(84, KeyBindType.Press)));
            Menu.AddSubMenu(fleeMenu);

            var drawMenu = new Menu("顯示", "cmDraw");
            drawMenu.AddItem(new MenuItem("DrawEngageRange", "顯示參與範圍").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawQ", "顯示 Q 範圍").SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawW", "顯示 W 範圍").SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawE", "顯示 E 範圍").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawR", "顯示 R 範圍").SetValue(true));
            Menu.AddSubMenu(drawMenu);
        }

        private static void Obj_AI_Base_OnPlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Animation.Contains("Spell1"))
            {
                LastQ = Environment.TickCount;

                if (GetBool("CancelQAnimation"))
                {
                    Chat.Say("/l");
                }

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo ||
                    Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear ||
                    Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    // TODO: implement this
                    var movePos =
                        (ObjectManager.Player.Position.To2D() -
                         (Player.BoundingRadius + 10)*ObjectManager.Player.Direction.To2D().Perpendicular()).To3D();

                    LeagueSharp.Common.Utility.DelayAction.Add(100, () => EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos));
                    LeagueSharp.Common.Utility.DelayAction.Add(
                        (int) (QDelay + 100 + Player.AttackDelay*100), Orbwalking.ResetAutoAttackTimer);
                }
            }

            if (args.Animation.Contains("Attack") &&
                (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo ||
                 Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear ||
                 Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
            {
                var aaDelay = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                    ? GetBool("UseFastQ") ? Player.AttackDelay*100 + Game.Ping/2f : Player.AttackCastDelay*1000
                    : Player.AttackDelay*100 + Game.Ping / 2f;

                LeagueSharp.Common.Utility.DelayAction.Add(
                    (int) (QDelay + aaDelay), () =>
                    {
                        if ((GetBool("UseQCombo") && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) ||
                            (GetBool("UseQMixed") && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) ||
                            (GetBool("UseQWaveClear") && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear))
                        {
                            Q.Cast(LastTarget.Position);
                        }
                    });
            }
        }
    }
}