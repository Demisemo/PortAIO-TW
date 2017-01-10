using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;
using BadaoSeries.CustomOrbwalker;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace BadaoSeries.Plugin
{
    internal class Ahri : AddUI
    {
        public static string ahri1 = "Ahri_Base_Orb_mis.troy";
        public static string ahri2 = "Ahri_Base_Orb_mis_02.troy";
        public static GameObject AhriOrbReturn { get { return ObjectManager.Get<GameObject>().FirstOrDefault(x => x.Name == "Ahri_Base_Orb_mis_02.troy"); } }
        public static GameObject AhriOrb { get { return ObjectManager.Get<GameObject>().FirstOrDefault(x => x.Name == "Ahri_Base_Orb_mis.troy"); } }
        public static List<Vector2> pos = new List<Vector2>();
        public static int Rcount;
        private static bool IsCombo { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo; } }
        private static bool IsHarass { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed; } }
        private static bool IsClear { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear; } }
        private static float Qdamage(Obj_AI_Base target)
        {
            return (float)Player.CalcDamage(target, Damage.DamageType.Magical,
                 new double[] { 40, 65, 90, 115, 140 }[Q.Level-1]
                                    + 0.35 * Player.FlatMagicDamageMod) +
                   (float)Player.CalcDamage(target, Damage.DamageType.True,
                 new double[] { 40, 65, 90, 115, 140 }[Q.Level - 1]
                                    + 0.35 * Player.FlatMagicDamageMod);
        }
        private static float Wdamage(Obj_AI_Base target)
        {
            return (float)Player.CalcDamage(target, Damage.DamageType.Magical,
                 new double[] { 40, 65, 90, 115, 140 }[W.Level-1]
                                    + 0.4 * Player.FlatMagicDamageMod)*1.6f;
        }
        private static float Edamage(Obj_AI_Base target)
        {
            return (float)Player.CalcDamage(target, Damage.DamageType.Magical,
                new double[] { 60, 95, 130, 165, 200 }[E.Level - 1]
                                    + 0.50 * Player.FlatMagicDamageMod);
        }
        private static float Rdamage(Obj_AI_Base target)
        {
            return (float)Player.CalcDamage(target, Damage.DamageType.Magical,
                                    (new double[] { 70, 110, 150 }[R.Level - 1]
                                    + 0.3 * Player.FlatMagicDamageMod)*3);
        }

        private static float AhriDamage(AIHeroClient target)
        {
            float x = 0;
            if (Player.Mana > Q.Instance.SData.Mana)
            {
                if (Q.IsReady()) x += Qdamage(target);
                if (Player.Mana > Q.Instance.SData.Mana + R.Instance.SData.Mana)
                {
                    if (R.IsReady()) x += Rdamage(target) ;
                    if (Player.Mana > Q.Instance.SData.Mana + R.Instance.SData.Mana + E.Instance.SData.Mana)
                    {
                        if (E.IsReady()) x += Edamage(target);
                        if (Player.Mana > Q.Instance.SData.Mana + R.Instance.SData.Mana + E.Instance.SData.Mana + W.Instance.SData.Mana)
                            if (W.IsReady()) x += Wdamage(target);
                    }
                }

            }
            if (LudensEcho.IsReady())
            {
                x = x + (float)Player.CalcDamage(target, Damage.DamageType.Magical, 100 + 0.1 * Player.FlatMagicDamageMod);
            }
            if(Ignite.IsReady())
            {
                x = x + (float)Player.GetSpellDamage(target, Ignite);
            }
            x = x + (float)Player.GetAutoAttackDamage(target, true);
            return x;
        }
        public Ahri()
        {
            Q = new Spell(SpellSlot.Q, 880);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E,975);
            E2 = new Spell(SpellSlot.E, 975);
            R = new Spell(SpellSlot.R,450);//600
            Q.SetSkillshot(0.25f, 100, 1600, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 60, 1550, true, SkillshotType.SkillshotLine);
            E2.SetSkillshot(0.25f, 60, 1550, true, SkillshotType.SkillshotLine);
            Q.DamageType = W.DamageType = E.DamageType = TargetSelector.DamageType.Magical;
            Q.MinHitChance = HitChance.High;
            E.MinHitChance = HitChance.High;

            Menu orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            MainMenu.AddSubMenu(orbwalkerMenu);

            Menu ts = MainMenu.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            Menu Asassin = new Menu("Assassin", "AssasinMode");
            {
                KeyBind(Asassin, "activeAssasin", "Assassin Key", 'T', KeyBindType.Press);
                Separator(Asassin, "1", "Make sure you select a target");
                Separator(Asassin, "2", "before press this key");
                MainMenu.AddSubMenu(Asassin);
            }
            Menu Combo = new Menu("Combo", "Combo");
            {
                Bool(Combo, "Qc", "Q", true);
                Bool(Combo, "Wc", "W", true);
                Bool(Combo, "Ec", "E", true);
                MainMenu.AddSubMenu(Combo);
            }
            Menu Harass = new Menu("Harass", "Harass");
            {
                Bool(Harass, "Qh", "Q", true);
                Bool(Harass, "Wh", "W", true);
                Bool(Harass, "Eh", "E", true);
                Slider(Harass, "manah", "Min mana", 40, 0, 100);
                MainMenu.AddSubMenu(Harass);
            }
            Menu Clear = new Menu("Clear", "Clear");
            {
                Bool(Clear, "Qj", "Q", true);
                Slider(Clear, "Qhitj", "Q if will hit", 2, 1, 3);
                Slider(Clear, "manaj", "Min mana", 40, 0, 100);
                MainMenu.AddSubMenu(Clear);
            }
            Menu Auto = new Menu("Auto", "Auto");
            {
                KeyBind(Auto, "harassa", "Harass Q", 'H',KeyBindType.Toggle);
                Bool(Auto, "interrupta", "E interrupt + gapcloser", true);
                Bool(Auto, "killsteala", "KillSteal", true);
                MainMenu.AddSubMenu(Auto);
            }
            Menu drawMenu = new Menu("Draw", "Draw");
            {
                Bool(drawMenu, "Qd", "Q");
                Bool(drawMenu, "Wd", "W");
                Bool(drawMenu, "Ed", "E");
                Bool(drawMenu, "Rd", "R");
                Bool(drawMenu, "Hpd", "Damage Indicator").ValueChanged += Ahri_ValueChanged;
                MainMenu.AddSubMenu(drawMenu);
            }
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnNewPath += Obj_AI_Base_OnNewPath;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnSpellCast +=Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += Gapcloser_OnGapCloser;
            Interrupter2.OnInterruptableTarget += InterruptableSpell_OnInterruptableTarget;
            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.DamageToUnit = AhriDamage;
            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = drawhp;
            //Custom//DamageIndicator.Initialize(AhriDamage);
            //Custom//DamageIndicator.Enabled = drawhp;
        }

        private void Ahri_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (!Enable) return;
            if (sender != null)
            {
                //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = e.GetNewValue<bool>();
                //Custom//DamageIndicator.Enabled = e.GetNewValue<bool>();
            }
        }
        private static bool comboq { get { return MainMenu.Item("Qc").GetValue<bool>(); } }
        private static bool combow { get { return MainMenu.Item("Wc").GetValue<bool>(); } }
        private static bool comboe { get { return MainMenu.Item("Ec").GetValue<bool>(); } }
        private static bool harassq { get { return MainMenu.Item("Qh").GetValue<bool>(); } }
        private static bool harassw { get { return MainMenu.Item("Wh").GetValue<bool>(); } }
        private static bool harasse { get { return MainMenu.Item("Eh").GetValue<bool>(); } }
        private static int manaharass { get { return MainMenu.Item("manah").GetValue<Slider>().Value; } }
        private static bool clearq { get { return MainMenu.Item("Qj").GetValue<bool>(); } }
        private static int clearqhit { get { return MainMenu.Item("Qhitj").GetValue<Slider>().Value; } }
        private static int manaclear { get { return MainMenu.Item("manaj").GetValue<Slider>().Value; } }
        private static bool autoharassq { get { return MainMenu.Item("harassa").GetValue<KeyBind>().Active; } }
        private static bool autointerrupt { get { return MainMenu.Item("interrupta").GetValue<bool>(); } }
        private static bool autokillsteal { get { return MainMenu.Item("killsteala").GetValue<bool>(); } }
        private static bool drawq { get { return MainMenu.Item("Qd").GetValue<bool>(); } }
        private static bool draww { get { return MainMenu.Item("Wd").GetValue<bool>(); } }
        private static bool drawe { get { return MainMenu.Item("Ed").GetValue<bool>(); } }
        private static bool drawr { get { return MainMenu.Item("Rd").GetValue<bool>(); } }
        private static bool drawhp { get { return MainMenu.Item("Hpd").GetValue<bool>(); } }
        private static bool activeAssasin { get { return MainMenu.Item("activeAssasin").GetValue<KeyBind>().Active; } }

        private void InterruptableSpell_OnInterruptableTarget(AIHeroClient sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Enable)
                return;
            if (sender.IsEnemy && sender.IsValidTarget(E.Range) && E.IsReady() && autointerrupt)
            {
                E.BadaoCast(sender);
            }
        }

        private void Gapcloser_OnGapCloser(ActiveGapcloser gapcloser)
        {
            if (!Enable)
                return;
            if (gapcloser.Sender.IsEnemy && gapcloser.Sender.IsValidTarget(E.Range) && E.IsReady() && autointerrupt)
            {
                E.BadaoCast(gapcloser.Sender);
            }
        }
        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Enable)
                return;
            if (sender.IsMe)
            {
                if (args.SData.Name == R.Instance.Name) Rcount = Utils.GameTimeTickCount;
            }
            if (!activeAssasin && autoharassq && !sender.IsMe && sender.IsEnemy && (sender as AIHeroClient).IsValidTarget(Q.Range) &&
                (args.SData.IsAutoAttack() || !args.SData.CanMoveWhileChanneling) && Player.ManaPercent >= manaharass)
            {
                Q.Cast(sender);
            }
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!Enable)
                return;
            // Q after attack
            if (!E.IsReady() && Q.IsReady() && (IsCombo || IsHarass))
            {
                foreach (var x in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range)))
                    Q.CastIfWillHit(x, 2);
                if ((target as AIHeroClient).IsValidTarget())
                    Q.Cast(target as AIHeroClient);
            }
            // E after attack
            if (E.IsReady() && (IsCombo || IsHarass))
            {
                if ((target as AIHeroClient).IsValidTarget())
                {
                    if (E.BadaoCast(target as AIHeroClient))
                        LeagueSharp.Common.Utility.DelayAction.Add(50, () => Q.Cast(target as AIHeroClient));
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!Enable)
                return;
            if (Player.IsDead)
                return;
            if (drawq)
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Aqua);
            if (draww)
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.Aqua);
            if (drawe)
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.Aqua);
            if (drawr)
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Aqua);
        }
        private void Obj_AI_Base_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (!Enable)
                return;
            if (Player.IsDashing()) return;
            var enemies = HeroManager.Enemies.Select(x => x.NetworkId).ToList();
            if (enemies.Contains(sender.NetworkId) && sender.IsValidTarget())
            {
                if (IsCombo)
                    LeagueSharp.Common.Utility.DelayAction.Add(50, () => comboonnewpath());
                if (IsHarass && Player.ManaPercent >= manaharass)
                    LeagueSharp.Common.Utility.DelayAction.Add(50, () => harassonnewpath());
            }
            if (activeAssasin)
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target.IsValidTarget() && target.NetworkId == sender.NetworkId)
                {
                    AssasinOnNewPath();
                }
            }
        }
        private void Game_OnUpdate(EventArgs args)
        {
            if (!Enable)
            {
                //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = false;
                //Custom//DamageIndicator.Enabled = false;
                return;
            }
            if ((IsCombo || IsHarass) && Orbwalking.CanMove(50)
                && (Q.IsReady() || E.IsReady()) )
            {
                Orbwalker.SetAttack(false);
            }
            else Orbwalker.SetAttack(true);
            if (autokillsteal && !activeAssasin)
                killstealUpdate();
            if (IsCombo)
                comboupdate();
            if (IsHarass && Player.ManaPercent >= manaharass)
                harassupdate();
            if (IsClear && Player.ManaPercent >= manaclear)
                ClearOnUpdate();
            if (activeAssasin)
                AssasinMode();
        }
        private static void killstealUpdate()
        {
            var enemies = HeroManager.Enemies;
            foreach (var x in enemies.Where(x => x.IsValidTarget(Q.Range) && Qdamage(x) > x.Health))
            {
                Q.Cast(x);
            }
            foreach (var x in enemies.Where(x => x.IsValidTarget(W.Range) && Wdamage(x) > x.Health))
            {
                W.Cast(x);
            }
            foreach (var x in enemies.Where(x => x.IsValidTarget(E.Range) && Edamage(x) > x.Health))
            {
                E.Cast(x);
            }
        }
        private static void comboupdate()
        {
            // use W
            if (combow)
            {
                var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                if (W.IsReady() && target.IsValidTarget() && !target.IsZombie)
                {
                    W.Cast();
                }
            }
            //use Q
            if (comboq)
            {
                if (Q.IsReady())
                {
                    foreach (var x in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie))
                    {
                        if (x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression))
                            if (Q.Cast(x) == Spell.CastStates.SuccessfullyCasted)
                                return;
                    }
                }
            }
        }
        private static void comboonnewpath()
        {
            // use Q
            if (comboq)
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (Q.IsReady() && target.IsValidTarget() && !target.IsZombie &&
                    (!E.IsReady() || E.GetBadaoPrediction(target).CollisionObjects.Any()))
                {
                    if (Q.Cast(target) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
                if (Q.IsReady() &&
                    (!E.IsReady() || E.GetBadaoPrediction(target).CollisionObjects.Any()))
                {
                    foreach (var x in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie))
                    {
                        if (x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression))
                            if (Q.Cast(x) == Spell.CastStates.SuccessfullyCasted)
                                return;
                    }
                }
                if (!comboe && Q.IsReady() && target.IsValidTarget() && !target.IsZombie )
                {
                    if (Q.Cast(target) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
            }
            //use E
            if(comboe)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (E.IsReady() && target.IsValidTarget() && !target.IsZombie)
                {
                    if (E.BadaoCast(target) && Q.IsReady() && comboq)
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add(50, () => Q.Cast(target));
                        return;
                    }
                }
                foreach (var x in HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && !x.IsZombie))
                {
                    if (E.BadaoCast(x) && Q.IsReady() && comboq)
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add(50, () => Q.Cast(target));
                        return;
                    }
                }
            }

        }
        private static void harassupdate()
        {
            // use W
            if (harassw)
            {
                var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                if (W.IsReady() && target.IsValidTarget() && !target.IsZombie)
                {
                    W.Cast();
                }
            }
            //use Q
            if (harassq)
            {
                if (Q.IsReady())
                {
                    foreach (var x in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie))
                    {
                        if (x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression))
                            if (Q.Cast(x) == Spell.CastStates.SuccessfullyCasted)
                                return;
                    }
                }
            }
        }
        private static void harassonnewpath()
        {
            // use Q
            if (harassq)
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (Q.IsReady() && target.IsValidTarget() && !target.IsZombie &&
                    (!E.IsReady() || E.GetBadaoPrediction(target).CollisionObjects.Any()))
                {
                    if (Q.Cast(target) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
                if (Q.IsReady() &&
                    (!E.IsReady() || E.GetBadaoPrediction(target).CollisionObjects.Any()))
                {
                    foreach (var x in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie))
                    {
                        if (x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression))
                            if (Q.Cast(x) == Spell.CastStates.SuccessfullyCasted)
                                return;
                    }
                }
                if (!harasse && Q.IsReady() && target.IsValidTarget() && !target.IsZombie)
                {
                    if (Q.Cast(target) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
            }
            //use E
            if (harasse)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (E.IsReady() && target.IsValidTarget() && !target.IsZombie)
                {
                    if (E.BadaoCast(target) && Q.IsReady() && harassq)
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add(50, () => Q.Cast(target));
                        return;
                    }
                }
                foreach (var x in HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && !x.IsZombie))
                {
                    if (E.BadaoCast(x) && Q.IsReady() && harassq)
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add(50, () => Q.Cast(target));
                        return;
                    }
                }
            }

        }
        private static void ClearOnUpdate()
        {
            var farmlocation = Q.GetLineFarmLocation(MinionManager.GetMinions(Q.Range));
            if (clearq && Q.IsReady() && farmlocation.MinionsHit >= clearqhit)
                Q.Cast(farmlocation.Position);
        }
        private static void AssasinMode()
        {
            var target = TargetSelector.GetSelectedTarget();
            if (Orbwalking.CanMove(50))
                EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (target.IsValidTarget() && !target.IsZombie)
            {
                var targetpos = Prediction.GetPrediction(target, 0.25f).UnitPosition.To2D();
                var distance = targetpos.Distance(Player.Position.To2D());
                if (Ignite.IsReady() && target.IsValidTarget(450))
                {
                    Player.Spellbook.CastSpell(Ignite, target);
                }
                if (!R.IsReady(3000) || Player.IsDashing())
                {
                    if (W.IsReady() && Player.Distance(target.Position) <= W.Range)
                    {
                        W.Cast();
                    }
                }
                if (R.IsReady() && AhriOrbReturn == null && AhriOrb == null && Utils.GameTimeTickCount - Rcount >= 500)
                {
                    Vector2 intersec = new Vector2();
                    for (int i = 450; i >= 0; i = i - 50)
                    {
                        for (int j = 50;  j <= 600;  j = j + 50)
                        {
                            var vectors = Geometry.CircleCircleIntersection(Player.Position.To2D(),targetpos, i, j);
                            foreach (var x in vectors)
                            {
                                if (!Collide(x,target) && !x.IsWall())
                                {
                                    intersec = x;
                                    goto ABC;
                                }
                            }
                        }
                    }
                    ABC:
                    if (intersec.IsValid())
                        R.Cast(intersec.To3D());
                }
                else if (R.IsReady() && AhriOrbReturn != null &&
                         Player.Distance(targetpos) < Player.Distance(AhriOrbReturn.Position.To2D()) &&
                         Utils.GameTimeTickCount - Rcount >= 0)
                {
                    var Orb = AhriOrbReturn.Position.To2D();
                    var dis = Orb.Distance(targetpos);
                    Vector2 castpos = new Vector2();
                    for (int i = 450; i >= 200; i = i - 50)
                    {
                        if (Orb.Extend(targetpos, dis + i).Distance(Player.Position.To2D()) <= R.Range &&
                            !Orb.Extend(targetpos, dis + i).IsWall())
                        {
                            castpos = Orb.Extend(targetpos, dis + i);
                            break;
                        }
                    }
                    if (castpos.IsValid())
                        R.Cast(castpos.To3D());
                }
                if (Orbwalking.CanAttack() && Orbwalking.InAutoAttackRange(target))
                {
                    EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    Orbwalking.LastAttackCommandT = Utils.GameTimeTickCount - 4;
                }
            }
        }
        private static void AssasinOnNewPath()
        {
            // use Q
            {
                var target = TargetSelector.GetSelectedTarget();
                if (Q.IsReady() && target.IsValidTarget() && !target.IsZombie &&
                    (!E.IsReady() || E.GetBadaoPrediction(target).CollisionObjects.Any()))
                {
                    if (Q.Cast(target) == Spell.CastStates.SuccessfullyCasted)
                    {
                        Rcount = Utils.GameTimeTickCount;
                        return;
                    }
                }
            }
            //use E
            {
                var target = TargetSelector.GetSelectedTarget();
                if (E.IsReady() && target.IsValidTarget() && !target.IsZombie &&  Utils.GameTimeTickCount >= Rcount + 400)
                {
                    if (E.BadaoCast(target) && Q.IsReady())
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add(50, () => castQ(target));
                        return;
                    }
                }
            }

        }
        private static bool Collide(Vector2 pos, AIHeroClient target)
        {
            E2.UpdateSourcePosition(pos.To3D(),pos.To3D());
            return
                E2.GetBadaoPrediction(target).CollisionObjects.Any();
        }
        private static void castQ(AIHeroClient target)
        {
            if(Q.Cast(target) == Spell.CastStates.SuccessfullyCasted)
                Rcount = Utils.GameTimeTickCount;
        }
    }
}
