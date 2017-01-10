using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using PortAIO.Properties;
using SharpDX;
using TreeLib.Core;
using TreeLib.Extensions;
using TreeLib.Objects;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace PopBlanc
{
    internal class LeBlanc : TreeLib.Objects.Champion
    {
        private const int ERange = 950;
        private static AIHeroClient KSTarget;
        private static readonly Random Random = new Random(Utils.TickCount);

        public LeBlanc()
        {
            Q = SpellManager.Q;
            W = SpellManager.W;
            E = SpellManager.E;
            R = SpellManager.R;

            Menu = new Menu("PopBlanc", "PopBlanc", true);

            Orbwalker = Menu.AddOrbwalker();

            var spells = Menu.AddMenu("Spells", "Spells");

            var q = spells.AddSpell(
                SpellSlot.Q,
                new List<Orbwalking.OrbwalkingMode>
                {
                    Orbwalking.OrbwalkingMode.Combo,
                    Orbwalking.OrbwalkingMode.Mixed,
                    Orbwalking.OrbwalkingMode.LastHit,
                    Orbwalking.OrbwalkingMode.LaneClear
                });
            q.AddSlider("FarmQMana", "Farm Minimum Mana", 40);

            var w = spells.AddSpell(
                SpellSlot.W,
                new List<Orbwalking.OrbwalkingMode>
                {
                    Orbwalking.OrbwalkingMode.Combo,
                    Orbwalking.OrbwalkingMode.Mixed,
                    Orbwalking.OrbwalkingMode.LaneClear
                });
            w.AddSlider("FarmWMinions", "Farm Minimum Minions", 3, 1, 5);
            w.AddBool("WBackHarass", "Harass W Back");
            w.Item("WBackHarass").SetTooltip("Cast second W after harassing.");
            w.AddBool("WBackFarm", "Farm W Back");
            w.Item("WBackFarm").SetTooltip("Cast second W after farming.");
            w.AddBool("WBackClick", "Left Click W Back", false);
            w.Item("WBackClick").SetTooltip("Cast second W after left clicking.");

            var e = spells.AddSpell(
                SpellSlot.E,
                new List<Orbwalking.OrbwalkingMode> { Orbwalking.OrbwalkingMode.Combo, Orbwalking.OrbwalkingMode.Mixed });

            e.AddSlider("ERangeDecrease", "Decrease Range", 0, 0, 100);
            e.Item("ERangeDecrease").ValueChanged += (sender, args) =>
            {
                E.Range = ERange - args.GetNewValue<Slider>().Value;
                var eDraw = Menu.Item("Draw2");
                if (eDraw == null)
                {
                    return;
                }
                var eCircle = eDraw.GetValue<Circle>();
                eDraw.SetValue(new Circle(eCircle.Active, eCircle.Color, E.Range));
            };
            E.Range = ERange - e.Item("ERangeDecrease").GetValue<Slider>().Value;

            e.AddBool("ComboEFirst", "Combo E First", false);
            e.AddBool("AntiGapcloser", "AntiGapCloser with E");
            e.AddBool("AutoEImmobile", "Auto E Immobile Targets");

            var r = spells.AddSpell(
                SpellSlot.R,
                new List<Orbwalking.OrbwalkingMode> { Orbwalking.OrbwalkingMode.Combo, Orbwalking.OrbwalkingMode.Mixed });
            r.AddBool("LaneClearR", "Use in LaneClear", false);
            r.Item("LaneClearR").SetTooltip("Use R(W) in LaneClear");
            r.AddBool("AntiGapcloserR", "AntiGapCloser with R(E)", false);
            w.AddBool("RBackClick", "Left Click R(W) Back", false);
            w.Item("RBackClick").SetTooltip("Cast second R(W) after left clicking.");
            var combo = Menu.AddMenu("Combo", "Other Combos");

            var twoChainz = combo.AddMenu("2Chainz", "2Chainz");
            twoChainz.AddInfo("2ChainzInfo", " --> Cast E and R(E) on target.", Color.Red);
            twoChainz.AddKeyBind("2Key", "Combo Key", 'H');
            twoChainz.AddBool("2Selected", "Selected Target Only", false);
            twoChainz.AddBool("2W", "Use W if out of range");

            var aoe = combo.AddMenu("AOECombo", "AOE Combo");
            aoe.AddInfo("AOEInfo", " --> Cast W and R(W) on target(s).", Color.Red);
            aoe.AddKeyBind("AOECombo", "Combo Key", 'N');
            aoe.AddBool("AOEW", "Use W");
            aoe.AddBool("GapcloseW", "Use W to Gapclose");
            aoe.Item("GapcloseW").SetTooltip("Gapclose to cast R(W).");
            aoe.AddBool("AOER", "Use R(W)");
            aoe.AddSlider("AOEEnemies", "Minimum Enemies", 2, 1, 5);

            combo.AddBool("ComboOrbwalk", "Orbwalk when Comboing");

            var ks = Menu.AddMenu("Killsteal", "Killsteal");
            ks.AddBool("SmartKS", "Smart Killsteal");
            ks.AddSlider("KSMana", "Minimum Mana", 30);
            ks.AddSlider("KSHealth", "Minimum Health to W", 40);
            ks.AddBool("KSGapclose", "Use W to Gapclose", false);
            ks.Item("KSHealth").SetTooltip("Minimum health to W in to KS.");
            ks.AddSlider("KSEnemies", "Maximum Enemies to W", 3, 1, 4);
            ks.Item("KSEnemies").SetTooltip("Maximum enemies to W in to KS.");

            PassiveManager.Initialize(Menu);

            var flee = Menu.AddMenu("Flee", "Flee");
            flee.AddInfo("FleeInfo", " --> Flees towards cursor position.", Color.Red);
            flee.AddKeyBind("Flee", "Flee", 'T');
            flee.AddBool("FleeW", "Use W");
            flee.AddBool("FleeRW", "Use R(W)");
            flee.AddBool("FleeMove", "Move to Cursor Position");

            var draw = Menu.AddMenu("Drawings", "Drawings");
            draw.AddCircle("Draw0", "Draw Q Range", System.Drawing.Color.Red, Q.Range, false);
            draw.AddCircle("Draw1", "Draw W Range", System.Drawing.Color.Red, W.Range, false);
            draw.AddCircle("Draw2", "Draw E Range", System.Drawing.Color.Purple, E.Range, false);
            draw.AddBool("DrawCD", "Draw on CD");
            draw.AddBool("DrawWBack", "Draw W Back Position");

            var manaCost = new Dictionary<SpellSlot, int[]>
            {
                { SpellSlot.Q, new[] { 0, 50, 60, 70, 80, 90 } },
                { SpellSlot.W, new[] { 0, 80, 85, 90, 95, 100 } },
                { SpellSlot.E, new[] { 0, 80, 80, 80, 80, 80 } },
                { SpellSlot.R, new[] { 0, 50, 60, 70, 80, 90 } }
            };

            var damage = draw.AddMenu("Damage Indicator", "Damage Indicator");
            damage.AddBool("DmgEnabled", "Enabled");
            damage.AddCircle("HPColor", "Health Color", System.Drawing.Color.White);
            damage.AddCircle("FillColor", "Damage Color", System.Drawing.Color.DeepPink);
            damage.AddBool("Killable", "Killable");

            Menu.AddBool("Sounds", "Sounds");

            if (Menu.Item("Sounds").IsActive())
            {
                //new SoundObject(Resources.Load).Play();
            }

            Menu.AddInfo("Info", "By Trees and Lilith!", Color.Red);

            ManaBarIndicator.Initialize(draw, manaCost);
            //DamageIndicator.Initialize(damage, GetComboDamage);
            SpellManager.Initialize(Menu, Orbwalker);
            WBackPosition.Initialize();

            Menu.AddToMainMenu();
        }

        private static int Delay
        {
            get { return (int) (200 + Game.Ping / 2f + Random.Next(100)); }
        }

        private static int WCastTime
        {
            get { return (int) (1000 * W.Delay + Game.Ping / 2f); }
        }

        private static float WRadius
        {
            get { return W.Instance.SData.CastRadius; }
        }

        private static bool EFirst
        {
            get { return Menu.Item("ComboEFirst").IsActive(); }
        }

        private static bool AdditionalComboActive
        {
            get
            {
                return Menu.Item("2Key").IsActive() || Menu.Item("AOECombo").IsActive() || Menu.Item("Flee").IsActive();
            }
        }

        public override void OnUpdate()
        {
            SpellManager.UpdateUltimate();

            if (Player.IsDead)
            {
                return;
            }

            if (Menu.Item("AutoEImmobile").IsActive() && E.IsReady())
            {
                var target = Enemies.FirstOrDefault(e => e.IsValidTarget(E.Range) && e.IsMovementImpaired());
                if (target.IsValidTarget() && E.Cast(target).IsCasted())
                {
                    return;
                }
            }

            if (Menu.Item("AOECombo").IsActive())
            {
                ManualOrbwalk();
                if (AOECombo())
                {
                    return;
                }
            }

            if (Menu.Item("2Key").IsActive())
            {
                ManualOrbwalk();
                if (_2Chainz())
                {
                    return;
                }
            }

            if (Menu.Item("Flee").IsActive() && Flee())
            {
                return;
            }

            if (Menu.Item("SmartKS").IsActive() && Player.ManaPercent >= Menu.Item("KSMana").GetValue<Slider>().Value &&
                AutoKill()) {}
        }

        private static void ManualOrbwalk()
        {
            if (Menu.Item("ComboOrbwalk").IsActive())
            {
                Orbwalking.Orbwalk(Orbwalker.GetTarget(), Game.CursorPos);
            }
        }

        public override void OnCombo(Orbwalking.OrbwalkingMode mode)
        {
            if (AdditionalComboActive)
            {
                return;
            }

            Combo();
        }

        private static void Combo(AIHeroClient targ = null, bool force = false)
        {
            if (Q.LastCastedDelay(Delay) || R.LastCastedDelay(Delay))
            {
                return;
            }

            if (!force && CastSecondW())
            {
                return;
            }

            var target = targ ??
                         TargetSelector.GetTarget(
                             EFirst && E.IsReady() ? E.Range : W.Range + WRadius - 10, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget())
            {
                //Console.WriteLine("BAD TARG");
                return;
            }

            if (CastEFirst(target))
            {
                Console.WriteLine("Combo: Cast E FIRST");
                return;
            }

            if (Q.CanCast(target) && Q.IsActive(force) && Q.CastOnUnit(target))
            {
                Console.WriteLine("Combo: Cast Q");
                return;
            }

            SpellManager.UpdateUltimate();
            if (R.CanCast(target) && R.IsActive(force) && R.GetSpellSlot() == SpellSlot.Q && R.CastOnUnit(target))
            {
                Console.WriteLine("Combo: Cast R(Q)");
                return;
            }

            if (W.IsReady() && target.IsValidTarget(W.Range + WRadius - 10) && W.IsActive(force) && W.IsFirstW())
            {
                if (!force ||
                    (target.CountEnemiesInRange(300) <= Menu.Item("KSEnemies").GetValue<Slider>().Value &&
                     Player.HealthPercent >= Menu.Item("KSHealth").GetValue<Slider>().Value))
                {
                    var pos = Prediction.GetPrediction(target, W.Delay, W.Range + WRadius, W.Speed);
                    if (pos.CastPosition.Distance(target.ServerPosition) < WRadius && W.Cast(pos.CastPosition))
                    {
                        Console.WriteLine("Combo: Cast W");
                        return;
                    }
                }
            }

            if (E.CanCast(target) && E.IsActive(force) && target.DistanceToPlayer() > 50 && E.Cast(target).IsCasted())
            {
                Console.WriteLine("Combo: Cast E");
            }
        }

        private static bool CastSecondW()
        {
            return Menu.Item("WBackHarass").IsActive() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed &&
                   W.IsReady() && !W.IsFirstW() && !Q.IsReady() && W.Cast();
        }

        private static bool CastEFirst(Obj_AI_Base target)
        {
            return EFirst && E.CanCast(target) && E.IsActive() && target.DistanceToPlayer() > 50 &&
                   E.Cast(target).IsCasted();
        }

        private static bool AOECombo()
        {
            if (Player.IsDashing())
            {
                return true;
            }

            var target = TargetSelector.GetTarget(W.Range + WRadius, TargetSelector.DamageType.Magical);

            if (target != null &&
                target.CountEnemiesInRange(WRadius) >= Menu.Item("AOEEnemies").GetValue<Slider>().Value)
            {
                if (W.IsReady() && Menu.Item("AOEW").IsActive() && W.IsFirstW() &&
                    W.Cast(target, false, true).IsCasted())
                {
                    Console.WriteLine("AOE: Cast W");
                    return true;
                }

                if (R.IsReady() && Menu.Item("AOER").IsActive() && R.GetSpellSlot() == SpellSlot.W && R.IsFirstW() &&
                    R.Cast(target, false, true).IsCasted())
                {
                    Console.WriteLine("AOE: Cast R(W)");
                    return true;
                }
            }

            if (!W.IsReady() || !W.IsFirstW() || !R.IsReady(WCastTime) || !Menu.Item("GapcloseW").IsActive())
            {
                return false;
            }

            target = TargetSelector.GetTarget(W.Range * 2, TargetSelector.DamageType.Magical);

            if (target == null || target.CountEnemiesInRange(WRadius) < Menu.Item("AOEEnemies").GetValue<Slider>().Value)
            {
                return false;
            }

            var pos = Player.ServerPosition.Extend(target.ServerPosition, W.Range + 10);

            if (pos.IsValidWPoint() && W.Cast(pos))
            {
                Console.WriteLine("AOE: Cast Gapclose W");
                return true;
            }
            ;

            return false;
        }

        private static bool _2Chainz()
        {
            var chainable = TargetSelector.SelectedTarget ??
                            TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (chainable != null)
            {
                if (E.CanCast(chainable) && E.Cast(chainable).IsCasted())
                {
                    Console.WriteLine("2Chainz: Cast E");
                    return true;
                }

                if (R.CanCast(chainable) && R.IsReady() && R.GetSpellSlot() == SpellSlot.E &&
                    R.Cast(chainable).IsCasted())
                {
                    Console.WriteLine("2Chainz: Cast R(E)");
                    return true;
                }

                return false;
            }

            if (!Menu.Item("2W").IsActive() || !W.IsReady() || !W.IsFirstW() || !E.IsReady())
            {
                return false;
            }

            chainable = TargetSelector.SelectedTarget ??
                        TargetSelector.GetTarget(W.Range + E.Range, TargetSelector.DamageType.Magical);

            if (!chainable.IsValidTarget(W.Range + E.Range) || chainable.HasEBuff())
            {
                return false;
            }

            var pos = Player.ServerPosition.Extend(chainable.ServerPosition, W.Range + 10);
            Console.WriteLine("2Chainz: Cast Gapclose W");
            return W.Cast(pos);
        }

        public override void OnFarm(Orbwalking.OrbwalkingMode mode)
        {
            if (Q.IsReady() && Q.IsActive() && Player.ManaPercent >= Menu.Item("FarmQMana").GetValue<Slider>().Value)
            {
                var killable =
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly)
                        .FirstOrDefault(m => Q.IsKillable(m));

                if (killable.IsValidTarget() && killable.Health > Player.GetAutoAttackDamage(killable, true) &&
                    Q.CastOnUnit(killable))
                {
                    return;
                }
            }

            if (!Q.IsReady() && W.IsReady() && !W.IsFirstW() && Menu.Item("WBackFarm").IsActive() && W.Cast())
            {
                return;
            }

            var wReady = W.IsReady() && W.IsActive() && W.IsFirstW();
            var rReady = R.IsReady() && R.IsActive() && R.GetSpellSlot() == SpellSlot.W && R.IsFirstW();

            if (!wReady && !rReady)
            {
                return;
            }

            var min = Menu.Item("FarmWMinions").GetValue<Slider>().Value;
            var minions = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minions.Count < min)
            {
                return;
            }

            var pos = W.GetCircularFarmLocation(minions);

            if (pos.MinionsHit < min)
            {
                return;
            }

            if (wReady && W.Cast(pos.Position))
            {
                return;
            }

            if (rReady && R.Cast(pos.Position)) {}
        }

        private static bool Flee()
        {
            if (Player.IsDashing())
            {
                return true;
            }

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (Menu.Item("FleeW").IsActive() && W.IsReady() && W.IsFirstW())
            {
                var pos = Player.ServerPosition.Extend(Game.CursorPos, W.Range);
                if (W.Cast(pos))
                {
                    return true;
                }
            }

            SpellManager.UpdateUltimate();
            if (Menu.Item("FleeRW").IsActive() && R.IsReady() && R.GetSpellSlot() == SpellSlot.W && R.IsFirstW())
            {
                var pos = Player.ServerPosition.Extend(Game.CursorPos, W.Range);
                if (R.Cast(pos))
                {
                    return true;
                }
            }

            if (!Menu.Item("FleeMove").IsActive() || Player.Path.ToList().Last().Distance(Game.CursorPos) < 100)
            {
                return true;
            }

            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            return true;
        }

        private static bool AutoKill()
        {
            var wRange = W.IsReady() && W.IsFirstW() && Menu.Item("KSGapclose").IsActive() ? W.Range : 0;
            var enemies =
                HeroManager.Enemies.Where(
                    enemy =>
                        enemy.IsValidTarget(wRange + E.Range) && !enemy.IsZombie &&
                        enemy.Health < GetComboDamage(enemy, SpellSlot.Q, WCastTime)).ToList();

            if (!enemies.Any())
            {
                return false;
            }

            if (!KSTarget.IsValidTarget(wRange + E.Range))
            {
                KSTarget = null;
            }

            KSTarget = enemies.MinOrDefault(e => e.Health);

            if (KSTarget == null || !KSTarget.IsValid)
            {
                return false;
            }

            var pos = Player.ServerPosition.Extend(KSTarget.ServerPosition, W.Range);

            if (!E.IsInRange(KSTarget) && KSTarget.IsValidTarget(E.Range + W.Range) && KSTarget.Health < GetKSDamage(KSTarget, false, pos))
            {

                if (!pos.IsValidWPoint() || !W.IsFirstW() || !W.Cast(pos))
                {
                    return false;
                }

                Console.WriteLine("KS: Gapclose W");
                return true;
            }

            if (KSTarget.Health < GetKSDamage(KSTarget))
            {
                Combo(KSTarget, true);
                return true;
            }

            return false;
        }

        public override void AntiGapcloser_OnEnemyGapcloser(LeagueSharp.Common.ActiveGapcloser gapcloser)
        {
            if (!E.IsReady() || !Menu.Item("AntiGapcloser").IsActive())
            {
                return;
            }

            LeagueSharp.Common.Utility.DelayAction.Add(
                150, () =>
                {
                    if (gapcloser.Sender.IsValidTarget(E.Range) && E.Cast(gapcloser.Sender).IsCasted() &&
                        Menu.Item("AntiGapcloserR").IsActive())
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add(
                            (int) (200 + 1000 * E.Delay + Game.Ping / 2f), () =>
                            {
                                SpellManager.UpdateUltimate();
                                if (R.GetSpellSlot() == SpellSlot.E)
                                {
                                    R.Cast(gapcloser.Sender);
                                }
                            });
                    }
                });
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            foreach (var spell in new[] { "0", "1", "2" })
            {
                var circle = Menu.Item("Draw" + spell).GetValue<Circle>();
                var slot = (SpellSlot) Convert.ToInt32(spell);

                if (circle.Active && Player.Spellbook.GetSpell(slot).IsReady())
                {
                    Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
                }
            }
        }

        public override void Game_OnWndProc(WndEventArgs args)
        {
            if ((!Menu.Item("WBackClick").IsActive() && !Menu.Item("RBackClick").IsActive()) || Player.IsDead ||
                args.Msg != 0x202)
            {
                return;
            }

            var wPos =
                WBackPosition.Positions.Where(p => p.Obj.Position.Distance(Game.CursorPos) < 2000)
                    .OrderBy(p => p.Obj.Position.Distance(Game.CursorPos));
            if (
                wPos.Select(w => w.IsR ? R : W)
                    .Any(
                        spell =>
                            Menu.Item(spell.Slot + "BackClick").IsActive() && spell.IsReady() && !spell.IsFirstW() &&
                            spell.Cast())) {}
        }

        private static float GetKSDamage(Obj_AI_Base enemy, bool includeW = true, Vector3 position = default(Vector3))
        {
            var damage = 0d;

            if (Q.IsReady() && enemy.IsValidTarget(Q.Range, true, position))
            {
                var q = Q.GetDamage(enemy);
                damage += q;

                if (enemy.HasQBuff() || enemy.HasQRBuff())
                {
                    damage += q;
                }
            }

            if (includeW && W.IsReady() && W.IsFirstW() &&
                enemy.IsValidTarget(W.Range, true, position))
            {
                damage += W.GetDamage(enemy);
            }

            if (E.IsReady() && enemy.IsValidTarget(E.Range, true, position))
            {
                SpellManager.EPrediction.UpdateSourcePosition(position, position);
                var pred = SpellManager.EPrediction.GetPrediction(enemy);
                if (pred.Hitchance > HitChance.Medium)
                {
                    damage += E.GetDamage(enemy);
                }
            }

            if (R.IsReady() && enemy.IsValidTarget(R.Range, true, position))
            {
                var d = GetUltimateDamage(enemy, SpellSlot.Unknown);
                if (enemy.HasQBuff() || enemy.HasQRBuff())
                {
                    d += Q.GetDamage(enemy);
                }

                damage += d;
            }

            if (TreeLib.Managers.SpellManager.Ignite != null && TreeLib.Managers.SpellManager.Ignite.IsReady())
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float) damage;
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            return GetComboDamage(enemy, SpellSlot.Unknown);
        }

        private static float GetComboDamage(Obj_AI_Base enemy, SpellSlot slot, int t = 0)
        {
            var damage = 0d;

            if (Q.IsReady(t))
            {
                // 2 for q mark
                var d = Q.GetDamage(enemy);
                if (enemy.HasQBuff() || enemy.HasQRBuff())
                {
                    d += Q.GetDamage(enemy);
                }
                damage += d;
            }

            if (W.IsReady(t) && W.IsFirstW())
            {
                damage += W.GetDamage(enemy);
            }

            if (E.IsReady(t))
            {
                damage += E.GetDamage(enemy);
            }

            if (R.IsReady(t))
            {
                var d = GetUltimateDamage(enemy, slot);
                if (enemy.HasQBuff() || enemy.HasQRBuff())
                {
                    d += Q.GetDamage(enemy);
                }

                damage += d;
            }

            if (ItemManager.FrostQueensClaim.IsValidAndReady())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.FrostQueenClaim);
            }

            if (ItemManager.Botrk.IsValidAndReady())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            }

            if (ItemManager.Cutlass.IsValidAndReady())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
            }

            if (ItemManager.LiandrysTorment.IsValidAndReady())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.LiandrysTorment);
            }

            if (TreeLib.Managers.SpellManager.Ignite != null && TreeLib.Managers.SpellManager.Ignite.IsReady())
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            damage += 2 * Player.GetAutoAttackDamage(enemy, true);

            return (float) damage;
        }

        private static double GetUltimateDamage(Obj_AI_Base enemy, SpellSlot slot)
        {
            var d = 0d;
            var s = Player.Spellbook.GetSpell(SpellSlot.R);
            var level = s.Level;

            if (level < 1 || s.State == SpellState.NotLearned)
            {
                return 0d;
            }

            var maxDamage = new double[] { 200, 400, 600 }[level - 1] + 1.3f * Player.FlatMagicDamageMod;
            var spell = slot.Equals(SpellSlot.Unknown) ? R.GetSpellSlot() : slot;

            switch (spell)
            {
                case SpellSlot.Q:
                    var qDmg = Player.CalcDamage(
                        enemy, Damage.DamageType.Magical,
                        new double[] { 100, 200, 300 }[level - 1] + .65f * Player.FlatMagicDamageMod);
                    d = qDmg > maxDamage ? maxDamage : qDmg;
                    break;
                case SpellSlot.W:
                    d = Player.CalcDamage(
                        enemy, Damage.DamageType.Magical,
                        new double[] { 150, 300, 450 }[level - 1] + .975f * Player.FlatMagicDamageMod);
                    break;
                case SpellSlot.E:
                    var eDmg = Player.CalcDamage(
                        enemy, Damage.DamageType.Magical,
                        new double[] { 100, 200, 300 }[level - 1] + .65f * Player.FlatMagicDamageMod);
                    d = eDmg > maxDamage ? maxDamage : eDmg;
                    break;
            }
            return d;
        }
    }
}