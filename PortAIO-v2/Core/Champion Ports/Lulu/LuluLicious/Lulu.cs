﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Core;
using TreeLib.Core.Extensions;
using TreeLib.Extensions;
using TreeLib.Managers;
using TreeLib.Objects;
using ActiveGapcloser = TreeLib.Core.ActiveGapcloser;
using Color = SharpDX.Color;
using EloBuddy;
using PortAIO.Properties;

namespace LuluLicious
{
    internal class Lulu : TreeLib.Objects.Champion
    {
        private const int RRadius = 350;
        private static int LastWCast;
        private static Obj_AI_Base QAfterETarget;

        private static readonly Dictionary<SpellSlot, int[]> ManaCostDictionary = new Dictionary<SpellSlot, int[]>
        {
            { SpellSlot.Q, new[] { 0, 60, 65, 70, 75, 80 } },
            { SpellSlot.W, new[] { 0, 65, 65, 65, 65, 65 } },
            { SpellSlot.E, new[] { 0, 60, 70, 80, 90, 100 } },
            { SpellSlot.R, new[] { 0, 0, 0, 0, 0, 0 } }
        };

        public Lulu()
        {
            Q = SpellManager.Q;
            W = SpellManager.W;
            E = SpellManager.E;
            R = SpellManager.R;

            Menu = new Menu("Lulu", "Lulu", true);
            Menu.SetFontStyle(FontStyle.Regular, Color.MediumPurple);
            Orbwalker = Menu.AddOrbwalker();

            var combo = Menu.AddMenu("Spells", "Spells");
            combo.SetFontStyle(FontStyle.Regular, System.Drawing.Color.DeepSkyBlue.ToSharpDXColor());

            var pix = combo.AddMenu("Pix", "Pix");
            pix.AddInfo("PixQ", "-- Pix Q --", Color.Purple);
            pix.Item("PixQ").SetTooltip("Use Pix to Cast Q");
            pix.AddBool("QPixCombo", "Use in Combo", false);
            pix.AddBool("QPixHarass", "Use in Harass", false);

            pix.AddInfo("PixEQ", "-- Pix E->Q --", Color.Purple);
            pix.Item("PixEQ").SetTooltip("Use E into Pix Q");
            pix.AddBool("EQPixCombo", "Use in Combo");
            pix.AddBool("EQPixHarass", "Use in Harass");

            var q = combo.AddMenu("Q", "Q");
            q.AddBool("QCombo", "Use in Combo");
            q.AddBool("QHarass", "Use in Harass");

            q.AddInfo("QMisc2", "-- Misc --", Color.DeepSkyBlue);
            q.AddBool("QGapcloser", "Use Q on Gapcloser");
            q.AddBool("QImpaired", "Auto Q Movement Impaired", false);
            q.AddInfo("QMisc1", "-- Farm --", Color.Red);
            q.AddKeyBind("QFarm", "Use Q to Farm", 'K', KeyBindType.Toggle, true);
            q.AddBool("QLC", "Use in LaneClear");
            q.AddBool("QLH", "Use in LastHit", false);

            var w = combo.AddMenu("W", "W");

            var wEnemies = w.AddMenu("WEnemies", "Enemy Priority");
            foreach (var enemy in Enemies)
            {
                wEnemies.AddSlider(enemy.ChampionName + "WPriority", enemy.ChampionName, 1, 0, 5);
            }

            wEnemies.AddInfo("WEnemiesInfo", "0 means don't cast, 5 is highest priority", Color.DeepSkyBlue);
            wEnemies.AddBool("WPriority", "Priority Enabled", false);

            w.AddBool("WCombo", "Use on Enemy in Combo");
            w.AddBool("WHarass", "Use on Enemy in Harass");
            w.AddBool("WGapcloser", "Use W on Gapcloser");
            w.AddBool("WInterrupter", "Use W to Interrupt");

            var e = combo.AddMenu("E", "E");

            var eAllies = e.AddMenu("EAllies", "Ally Shielding");
            foreach (var ally in Allies)
            {
                eAllies.AddSlider(ally.ChampionName + "EPriority", ally.ChampionName + " Min Health", 20);
            }

            eAllies.AddInfo("EAlliesInfo", "Set to 0 to never shield ally.", Color.DeepSkyBlue);
            eAllies.AddBool("EAuto", "Use E on Allies");

            e.AddBool("ECombo", "Use on Enemy in Combo");
            e.AddBool("EHarass", "Use on Enemy in Harass");

            var r = combo.AddMenu("R", "R");

            var saver = r.AddMenu("Saver", "Saver");
            foreach (var ally in Allies)
            {
                saver.AddSlider(ally.ChampionName + "RPriority", ally.ChampionName + " Min Health", 15);
            }

            saver.AddInfo("RAlliesInfo", "Set to 0 to never ult ally.", Color.DeepSkyBlue);
            saver.AddBool("RAuto", "Use R on Allies");

            r.AddKeyBind("RForce", "Force Ult Ally", 'K');
            r.Item("RForce").SetTooltip("Casts R on the lowest HP ally in R range");
            r.AddBool("RInterrupter", "Use R on Interrupt");

            r.AddBool("RKnockup", "Auto R to Knockup");
            r.AddSlider("RKnockupEnemies", "Min Enemes to Knockup", 2, 1, 5);

            var ks = Menu.AddMenu("Killsteal", "Killsteal");
            ks.SetFontStyle(FontStyle.Regular, Color.Red);
            ks.AddBool("KSEnabled", "Enabled");
            ks.AddBool("KSQ", "Use Q");
            ks.AddBool("KSE", "Use E");
            ks.AddBool("KSEQ", "Use E->Q");

            ManaManager.Initialize(Menu);
            Q.SetManaCondition(ManaManager.ManaMode.Combo, 5);
            Q.SetManaCondition(ManaManager.ManaMode.Harass, 5);
            Q.SetManaCondition(ManaManager.ManaMode.Farm, 30);
            W.SetManaCondition(ManaManager.ManaMode.Combo, 15);
            W.SetManaCondition(ManaManager.ManaMode.Harass, 15);
            E.SetManaCondition(ManaManager.ManaMode.Combo, 10);
            E.SetManaCondition(ManaManager.ManaMode.Harass, 10);

            var flee = Menu.AddMenu("Flee", "Flee");
            flee.SetFontStyle(FontStyle.Regular, Color.Yellow);
            flee.AddInfo("FleeInfo", " --> Flees towards cursor position.", Color.Yellow);
            flee.AddKeyBind("Flee", "Flee", 'T');
            flee.AddBool("FleeW", "Use W");
            flee.AddBool("FleeMove", "Move to Cursor Position");

            var draw = Menu.AddMenu("Drawings", "Drawings");
            draw.SetFontStyle(FontStyle.Regular, Color.DeepPink);

            draw.AddCircle("DrawQ", "Draw Q", System.Drawing.Color.Purple, Q.Range);
            draw.AddCircle("DrawW", "Draw W/E", System.Drawing.Color.Purple, W.Range);
            draw.AddCircle("DrawR", "Draw R", System.Drawing.Color.Purple, R.Range);
            draw.AddBool("DrawPix", "Draw Pix");
            draw.AddBool("FarmPermashow", "Permashow Farm Enabled");

            if (draw.Item("FarmPermashow").IsActive())
            {
                q.Item("QFarm").Permashow();
            }

            draw.Item("FarmPermashow").ValueChanged +=
                (sender, eventArgs) => { q.Item("QFarm").Permashow(eventArgs.GetNewValue<bool>()); };

            var misc = Menu.AddMenu("Misc", "Misc");
            misc.SetFontStyle(FontStyle.Regular, Color.MediumPurple);

            CustomAntiGapcloser.Initialize(misc);
            CustomInterrupter.Initialize(misc);

            var superman = misc.AddMenu("Superman", "Speedy Up!");
            superman.SetFontStyle(FontStyle.Regular, Color.Red);
            superman.AddInfo("SupermanInfo", " --> Casts W+E on prioritized ally.", Color.Red);
            foreach (var ally in Allies.Where(a => !a.IsMe))
            {
                superman.AddSlider(ally.ChampionName + "WEPriority", ally.ChampionName + " Priority", 1, 0, 5);
            }

            superman.AddInfo("SupermanInfo2", "Set to 0 to never speedy up ally.", Color.Red);
            superman.AddKeyBind("Superman", "Use Speedy Up!", 'A');

            misc.AddBool("Support", "Support Mode", false);

            Menu.AddInfo("Info", "By Trees and Lilith!", Color.MediumPurple);
            Menu.AddToMainMenu();

            var dmg = draw.AddMenu("DamageIndicator", "Damage Indicator");
            dmg.AddBool("DmgEnabled", "Draw Damage Indicator");
            dmg.AddCircle("HPColor", "Predicted Health Color", System.Drawing.Color.White);
            dmg.AddCircle("FillColor", "Damage Color", System.Drawing.Color.MediumPurple);
            dmg.AddBool("Killable", "Killable Text");
            //DamageIndicator.Initialize(dmg, Utility.GetComboDamage);

            ManaBarIndicator.Initialize(draw, ManaCostDictionary);
            Pix.Initialize(Menu.Item("DrawPix"));
            SpellManager.Initialize(combo, Orbwalker);

            CustomAntiGapcloser.OnEnemyGapcloser += CustomAntiGapcloser_OnEnemyGapcloser;
            CustomInterrupter.OnInterruptableTarget += CustomInterrupter_OnInterruptableTarget;
        }

        public override void OnCombo(Orbwalking.OrbwalkingMode mode)
        {
            if (W.IsActive() && !W.HasManaCondition() && W.IsReady() && Menu.Item("WPriority").IsActive())
            {
                var wTarg = Utility.GetBestWTarget();
                if (wTarg != null && W.CanCast(wTarg) && W.CastOnUnit(wTarg))
                {
                    Console.WriteLine("[AUTO] Cast W");
                    return;
                }
            }

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical) ?? Pix.GetTarget();

            if (!target.IsValidTarget() || !SpellManager.Q.IsInRange(target))
            {
                PixCombo();
                return;
            }

            if (PixCombo())
            {
                return;
            }

            if (E.IsActive() && !E.HasManaCondition() && E.CanCast(target) && E.CastOnUnit(target))
            {
                Console.WriteLine("[Combo] Cast E");
                return;
            }

            if (Q.IsReady() && W.IsActive() && !W.HasManaCondition() && W.CanCast(target) && W.CastOnUnit(target))
            {
                Console.WriteLine("[Combo] Cast W");
                return;
            }

            if (!Q.IsActive() || !Q.IsReady() || Q.HasManaCondition())
            {
                return;
            }

            if (Q.Cast(target).IsCasted())
            {
                Console.WriteLine("[Combo] Cast Q");
            }
        }

        private static bool PixCombo(AIHeroClient target, bool useQ, bool useE, bool killSteal = false)
        {
            if (!target.IsValidTarget() || !Pix.IsValid())
            {
                return false;
            }

            useQ &= Q.IsReady() && (killSteal || !Q.HasManaCondition());
            useE &= useQ && E.IsReady() && (killSteal || !E.HasManaCondition()) &&
                    Player.Mana > ManaCostDictionary[Q.Slot][Q.Level] + ManaCostDictionary[E.Slot][E.Level];

            if (useQ && SpellManager.PixQ.IsInRange(target) && SpellManager.PixQ.Cast(target).IsCasted())
            {
                Console.WriteLine("[Pix] Cast Q");
                return true;
            }

            if (!useE)
            {
                return false;
            }

            var eqTarget = Pix.GetETarget(target);
            if (eqTarget == null || !E.CastOnUnit(eqTarget))
            {
                return false;
            }

            Console.WriteLine("[Pix] Cast E");
            return true;
        }

        private static bool PixCombo()
        {
            var mode = Orbwalker.ActiveMode.GetModeString();
            var target = Pix.GetTarget(Q.Range + E.Range);
            return PixCombo(target, Menu.Item("QPix" + mode).IsActive(), Menu.Item("EQPix" + mode).IsActive());
        }

        public override void OnFarm(Orbwalking.OrbwalkingMode mode)
        {
            if (!Menu.Item("QFarm").IsActive() || !Q.IsReady() || Q.HasManaCondition())
            {
                return;
            }

            var condition = mode == Orbwalking.OrbwalkingMode.LaneClear ? Menu.Item("QLC") : Menu.Item("QLH");

            if (condition == null || !condition.IsActive())
            {
                return;
            }

            var qMinions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var killable = qMinions.FirstOrDefault(o => o.Health < Q.GetDamage(o));

            if (killable != null && !killable.CanAAKill() && Q.Cast(killable).IsCasted())
            {
                return;
            }

            var pixMinions = Pix.GetMinions();
            killable = pixMinions.FirstOrDefault(o => o.Health < Q.GetDamage(o));

            if (Pix.IsValid() && killable != null && !killable.CanAAKill() &&
                SpellManager.PixQ.Cast(killable).IsCasted())
            {
                return;
            }

            if (mode == Orbwalking.OrbwalkingMode.LastHit)
            {
                return;
            }

            var pos = Q.GetLineFarmLocation(qMinions);
            var spell = Q;

            var pixPos = Pix.GetFarmLocation();

            if (Pix.IsValid() && pixPos.MinionsHit > pos.MinionsHit)
            {
                pos = pixPos;
                spell = SpellManager.PixQ;
            }

            if (pos.MinionsHit > 2 && spell.Cast(pos.Position)) {}
        }

        public override void OnUpdate()
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Player.IsRecalling())
            {
                return;
            }

            if (Saver())
            {
                return;
            }

            if (Flee())
            {
                return;
            }

            if (AutoQ())
            {
                return;
            }

            if (Superman())
            {
                return;
            }

            if (AutoR())
            {
                return;
            }

            if (Killsteal()) {}
        }

        private static bool Killsteal()
        {
            if (!Menu.Item("KSEnabled").IsActive())
            {
                return false;
            }

            var mana = Player.Mana;
            var useQ = Menu.Item("KSQ").IsActive() && Q.IsReady();
            var useE = Menu.Item("KSE").IsActive() && E.IsReady();
            var useEQ = Menu.Item("KSEQ").IsActive() && Player.Mana > Q.ManaCost + E.ManaCost;

            if (!useQ && !useE)
            {
                return false;
            }

            foreach (var enemy in
                Enemies.Where(e => e.IsValidTarget(E.Range + Q.Range) && !e.IsZombie).OrderBy(e => e.Health))
            {
                var qDmg = Q.GetDamage(enemy);
                var eDmg = E.GetDamage(enemy);

                if (useE && E.IsInRange(enemy))
                {
                    if (eDmg > enemy.Health && E.CastOnUnit(enemy))
                    {
                        return true;
                    }

                    if (useQ && qDmg + eDmg > enemy.Health && useEQ && E.CastOnUnit(enemy))
                    {
                        QAfterETarget = enemy;
                        return true;
                    }
                }


                if (useQ && qDmg > enemy.Health && Q.IsInRange(enemy) && Q.Cast(enemy).IsCasted())
                {
                    return true;
                }

                if (useQ && useE && useEQ && qDmg > enemy.Health && PixCombo(enemy, true, true, true))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Saver()
        {
            if (Player.InFountain())
            {
                return false;
            }

            var useE = Menu.Item("EAuto").IsActive() && E.IsReady();
            var useR = Menu.Item("RAuto").IsActive() && R.IsReady();

            if (!useE && !useR)
            {
                return false;
            }

            foreach (var ally in Allies.Where(h => h.IsValidTarget(R.Range, false) && h.CountEnemiesInRange(300) > 0))
            {
                var hp = ally.GetPredictedHealthPercent();

                if (useE && E.IsInRange(ally) &&
                    hp <= Menu.Item(ally.ChampionName + "EPriority").GetValue<Slider>().Value)
                {
                    E.CastOnUnit(ally);
                }

                if (useR && hp <= Menu.Item(ally.ChampionName + "RPriority").GetValue<Slider>().Value &&
                    R.CastOnUnit(ally))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AutoQ()
        {
            return Menu.Item("QImpaired").IsActive() && Q.IsReady() &&
                   Enemies.Any(e => e.IsValidTarget(Q.Range) && e.IsMovementImpaired() && Q.Cast(e).IsCasted());
        }

        private static bool Superman()
        {
            if (!Menu.Item("Superman").IsActive() || !(W.IsReady() || E.IsReady()))
            {
                return false;
            }

            var target = Utility.GetBestWETarget();

            if (target == null)
            {
                Console.WriteLine("TARG");
                return false;
            }

            if (W.IsReady() && W.IsInRange(target) && W.CastOnUnit(target)) {}

            return E.IsReady() && E.IsInRange(target) && E.CastOnUnit(target);
        }

        private static bool AutoR()
        {
            if (!R.IsReady() || Player.InFountain())
            {
                return false;
            }

            if (Menu.Item("RForce").IsActive() &&
                Allies.Where(h => h.IsValidTarget(R.Range, false)).OrderBy(o => o.Health).Any(o => R.CastOnUnit(o)))
            {
                return true;
            }


            if (!Menu.Item("RKnockup").IsActive())
            {
                return false;
            }

            var count = 0;
            var bestAlly = Player;
            foreach (var ally in Allies.Where(a => a.IsValidTarget(R.Range, false)))
            {
                var c = ally.CountEnemiesInRange(RRadius);

                if (c <= count)
                {
                    continue;
                }

                count = c;
                bestAlly = ally;
            }

            return count >= Menu.Item("RKnockupEnemies").GetValue<Slider>().Value && R.CastOnUnit(bestAlly);
        }

        private static bool Flee()
        {
            if (!Menu.Item("Flee").IsActive())
            {
                return false;
            }

            if (Player.IsDashing())
            {
                return true;
            }

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (Menu.Item("FleeW").IsActive() && W.IsReady() && W.CastOnUnit(Player))
            {
                return true;
            }

            if (!Menu.Item("FleeMove").IsActive() || Player.Path.ToList().Last().Distance(Game.CursorPos) < 100)
            {
                return true;
            }

            return EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }

        public override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Slot == SpellSlot.W)
                {
                    LastWCast = Utils.TickCount;
                }

                if (args.Slot == SpellSlot.E && QAfterETarget != null)
                {
                    LeagueSharp.Common.Utility.DelayAction.Add(
                        100, () =>
                        {
                            SpellManager.PixQ.Cast(QAfterETarget);
                            QAfterETarget = null;
                        });
                }

                return;
            }

            if (!(Menu.Item("EAuto").IsActive() && E.IsReady()) || !(Menu.Item("RAuto").IsActive() && R.IsReady()))
            {
                return;
            }

            if (sender.IsMinion || ((Obj_AI_Base)args.Target).IsMinion)
            {
                return;
            }

            var caster = sender as AIHeroClient;
            var target = args.Target as AIHeroClient;

            if ((!caster.IsEnemy) || (!target.IsAlly))
            {
                Console.WriteLine("Someone isn't valid");
                return;
            }

            var damage = 0d;
            try
            {
                damage = caster.GetSpellDamage(target, args.SData.Name);
                Console.WriteLine("DMG : " + damage);
            }
            catch {}

            var hp = (target.Health - damage) / target.MaxHealth * 100;
            Console.WriteLine("HP : " + hp);

            if (E.CanCast(target) && hp <= Menu.Item(target.ChampionName + "EPriority").GetValue<Slider>().Value)
            {
                E.Cast(target);
            }

            if (R.CanCast(target) && hp <= Menu.Item(target.ChampionName + "RPriority").GetValue<Slider>().Value)
            {
                R.Cast(target);
            }
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            foreach (var spell in new[] { "Q", "W", "R" })
            {
                var circle = Menu.Item("Draw" + spell).GetValue<Circle>();
                if (circle.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
                }
            }
        }

        public override void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!Menu.Item("Support").GetValue<bool>() ||
                !HeroManager.Allies.Any(x => x.IsValidTarget(1000, false) && !x.IsMe))
            {
                return;
            }

            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed &&
                Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit)
            {
                return;
            }

            var minion = args.Target as Obj_AI_Base;
            if (minion != null && minion.IsMinion && minion.IsValidTarget())
            {
                args.Process = false;
            }
        }

        public void CustomInterrupter_OnInterruptableTarget(AIHeroClient sender,
            CustomInterrupter.InterruptableTargetEventArgs args)
        {
            if (sender == null || !sender.IsValidTarget())
            {
                return;
            }

            if (Utils.TickCount - LastWCast < 2000)
            {
                return;
            }

            if (Menu.Item("WInterrupter").IsActive() && W.CanCast(sender) && W.CastOnUnit(sender))
            {
                return;
            }

            if (!Menu.Item("RInterrupter").IsActive() || !R.IsReady())
            {
                return;
            }

            if (
                Allies.OrderBy(h => h.Distance(sender))
                    .Any(h => h.IsValidTarget(R.Range, false) && h.Distance(sender) < RRadius && R.CastOnUnit(h))) {}
        }

        private static void CustomAntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget())
            {
                return;
            }

            if (Menu.Item("QGapcloser").IsActive() && Q.CanCast(gapcloser.Sender))
            {
                Q.Cast(gapcloser.Sender);
            }

            if (Menu.Item("WGapcloser").IsActive() && W.CanCast(gapcloser.Sender) && W.CastOnUnit(gapcloser.Sender)) {}
        }
    }
}