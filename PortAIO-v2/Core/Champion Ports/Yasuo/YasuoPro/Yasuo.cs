using System;
using System.Collections.Generic;
using System.Linq;
using EvadeYas;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;


using EloBuddy; 
using LeagueSharp.Common; 
namespace YasuoPro
{

    //Credits to Kortatu/Esk0r for his work on Evade which this assembly relies on heavily!

    internal class Yasuo : Helper
    {
        public AIHeroClient CurrentTarget;
        public bool Fleeing;

        public void OnLoad()
        {
            Yasuo = ObjectManager.Player;

            if (Yasuo.BaseSkinName != "Yasuo")
            {
                return;
            }

            Chat.Print("<font color='#1d87f2'>YasuoPro by Seph Loaded. Good Luck!</font>");
            Chat.Print("<font color='#12FA54'>::::Latest Update: Reworked Waveclear due to complaints & Combo improvements</font>");
            Chat.Print("<font color='#1d87f2'>::::Any Issues/Recommendations - Post On Topic</font>");
            InitItems();
            InitSpells();
            YasuoMenu.Init(this);
            Orbwalker.RegisterCustomMode("YasuoPro.FleeMode", "Flee", YasuoMenu.KeyCode("Z"));
            Program.Init();
            if (GetBool("Misc.Walljump") && Game.MapId == GameMapId.SummonersRift)
            {
                WallJump.Initialize();
            }
            shop = ObjectManager.Get<Obj_Shop>().FirstOrDefault(x => x.IsAlly);
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnGapClose;
            Interrupter2.OnInterruptableTarget += OnInterruptable;
            Obj_AI_Base.OnProcessSpellCast += TargettedDanger.SpellCast;
            CustomEvents.Unit.OnDash += UnitOnOnDash;
        }


        void OnUpdate(EventArgs args)
        {
            if (Yasuo.IsDead || Yasuo.IsRecalling())
            {
                return;
            }

            CastUlt();

            if (GetBool("Evade.WTS"))
            {
                TargettedDanger.OnUpdate();
            }

            var omode = Orbwalker.ActiveMode;

            if (GetBool("Misc.AutoStackQ") && omode != Orbwalking.OrbwalkingMode.Combo && omode != Orbwalking.OrbwalkingMode.CustomMode && !TornadoReady && !CurrentTarget.IsValidEnemy(Spells[Q].Range) && !Yasuo.IsDashing() && !InDash)
            {
                var closest =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.IsValidMinion(Spells[Q].Range) && (MinionManager.IsMinion(x) || x.BaseSkinName.Equals("Sru_Crab")))
                        .MinOrDefault(x => x.Distance(Yasuo));
                if (closest != null)
                {
                    var pred = Spells[Q].GetPrediction(closest);
                    if (pred.Hitchance >= HitChance.Low)
                    {
                        Spells[Q].Cast(pred.CastPosition);
                    }
                }
            }

            if (GetBool("Misc.Walljump") && Game.MapId == GameMapId.SummonersRift)
            {
                WallJump.OnUpdate();
            }


            if (GetKeyBind("Misc.DashMode"))
            {
                MoveToMouse();
                return;
            }

            Fleeing = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.CustomMode;

            if (GetBool("Killsteal.Enabled") && !Fleeing)
            {
                Killsteal();
            }

            if (GetKeyBind("Harass.KB") && !Fleeing)
            {
                Harass();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    Orbwalker.SetAttack(true);
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    Orbwalker.SetAttack(true);
                    Mixed();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    Orbwalker.SetAttack(true);
                    LHSkills();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    Orbwalker.SetAttack(true);
                    NewWaveClear();
                    break;
                case Orbwalking.OrbwalkingMode.CustomMode:
                    Flee();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    break;
            }
        }

        void CastUlt()
        {
            if (!SpellSlot.R.IsReady())
            {
                return;
            }
            if (GetBool("Combo.UseR") && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                CastR(GetSliderInt("Combo.RMinHit"));
            }

            if (GetBool("Misc.AutoR") && !Fleeing)
            {
                CastR(GetSliderInt("Misc.RMinHit"));
            }
        }

        void OnDraw(EventArgs args)
        {
            if (Debug)
            {
                Drawing.DrawCircle(DashPosition.To3D(), Yasuo.BoundingRadius, System.Drawing
                    .Color.Chartreuse);
            }


            if (Yasuo.IsDead || GetBool("Drawing.Disable"))
            {
                return;
            }

            TargettedDanger.OnDraw(args);

            if (GetBool("Misc.Walljump") && Game.MapId == GameMapId.SummonersRift)
            {
                WallJump.OnDraw();
            }

            var pos = Yasuo.Position.WTS();

            Drawing.DrawText(pos.X, pos.Y + 50, isHealthy ? System.Drawing.Color.Green : System.Drawing.Color.Red,
                "Healthy: " + isHealthy);

            var drawq = GetCircle("Drawing.DrawQ");
            var drawe = GetCircle("Drawing.DrawE");
            var drawr = GetCircle("Drawing.DrawR");

            if (drawq.Active)
            {
                Render.Circle.DrawCircle(Yasuo.Position, Qrange, drawq.Color);
            }
            if (drawe.Active)
            {
                Render.Circle.DrawCircle(Yasuo.Position, Spells[E].Range, drawe.Color);
            }
            if (drawr.Active)
            {
                Render.Circle.DrawCircle(Yasuo.Position, Spells[R].Range, drawr.Color);
            }
        }



        void Combo()
        {
            float range = 0;
            if (SpellSlot.R.IsReady())
            {
                range = Spells[R].Range;
            }

            else if (Spells[Q2].IsReady())
            {
                range = Spells[Q2].Range;
            }

            else if (Spells[E].IsReady())
            {
                range = Spells[E].Range;
            }

            CurrentTarget = TargetSelector.GetTarget(range, TargetSelector.DamageType.Physical);

            SmartCombo(CurrentTarget);

            if (GetBool("Combo.UseIgnite"))
            {
                CastIgnite();
            }

            if (GetBool("Items.Enabled"))
            {
                if (GetBool("Items.UseTIA"))
                {
                    Tiamat.Cast(null);
                }
                if (GetBool("Items.UseHDR"))
                {
                    Hydra.Cast(null);
                }
                if (GetBool("Items.UseTitanic"))
                {
                    Titanic.Cast(null);
                }
                if (GetBool("Items.UseBRK") && CurrentTarget != null)
                {
                    Blade.Cast(CurrentTarget);
                }
                if (GetBool("Items.UseBLG") && CurrentTarget != null)
                {
                    Bilgewater.Cast(CurrentTarget);
                }
                if (GetBool("Items.UseYMU"))
                {
                    Youmu.Cast(null);
                }
            }
        }

        float BeyBladeAttemptStick = 0;
        bool InBeyBlade;
        Obj_AI_Base beyBladeTarget = null;
        Obj_AI_Base beyBlademidTarget = null;


        void SmartCombo(AIHeroClient target)
        {
            if (target != null)
            {
                //EQ
                if (TornadoReady && Yasuo.IsDashing())
                {
                    var bestTarget = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidTarget(Qrange) && x.Distance(target) <= QRadius).FirstOrDefault();
                    if (bestTarget != null && GetBool("Combo.UseEQ"))
                    {
                        var pred = Spells[Q].GetPrediction(bestTarget);
                        if (pred.Hitchance >= HitChance.Medium)
                        {
                            Spells[Q].Cast(pred.CastPosition);
                        }
                    }
                }


                if (GetBool("Combo.UseBeyBlade") && TornadoReady && Spells[Q].IsReady())
                {
                    var bbtarget = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
                    if (bbtarget == null)
                    {
                        return;
                    }

                    if (InBeyBlade && TickCount - BeyBladeAttemptStick > 800)
                    {
                        InBeyBlade = false;
                        DontDash = false;
                        return;
                    }

                    if (InBeyBlade && Spells[Flash].IsReady() && Spells[R].IsReady())
                    {
                        if (Yasuo.IsDashing() && InDash)
                        {
                            var dashinfo = Dash.GetDashInfo(Yasuo);
                        
                            if (beyBladeTarget != null && beyBlademidTarget != null && dashinfo.EndPos.Distance(beyBladeTarget) <= FlashRange && (!beyBladeTarget.ServerPosition.PointUnderEnemyTurret() || Helper.GetKeyBind("Misc.TowerDive")))
                            {
                                if (Yasuo.Distance(beyBlademidTarget) <= Spells[Q].Range)
                                {
                                    Spells[Q].Cast(beyBlademidTarget.ServerPosition);
                                    Spells[Flash].Cast(beyBladeTarget.ServerPosition);
                                    InBeyBlade = false;
                                    DontDash = false;
                                }
                            }

                            else
                            {
                                InBeyBlade = false;
                                DontDash = false;
                            }
                        }

                        if (Spells[E].IsReady() && Spells[R].IsReady() && Spells[Flash].IsReady() && !target.ServerPosition.PointUnderEnemyTurret())
                        {
                            var minion = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidTarget(Spells[E].Range) && x.Distance(bbtarget) <= Yasuo.Distance(bbtarget) && GetDashPos(x).Distance(bbtarget) <= Yasuo.Distance(bbtarget) && GetDashPos(x).Distance(bbtarget) <= FlashRange && (SafetyCheck(x, true))).FirstOrDefault();
                            if (minion != null)
                            {
                                Spells[E].Cast(minion);
                                DontDash = true;
                                InBeyBlade = true;
                                beyBlademidTarget = minion;
                                beyBladeTarget = bbtarget;
                                BeyBladeAttemptStick = TickCount;
                            }
                        }
                    }
                }

                if (Spells[Q].IsReady() && !Yasuo.IsDashing())
                {
                    var targ = target.IsInRange(Qrange) ? target : TargetSelector.GetTarget(Qrange, TargetSelector.DamageType.Physical);
                    if (targ != null)
                    {
                        CastQ(target);
                    }
                }

                var eqready = Spells[E].IsReady() && Spells[Q].IsReady() && TornadoReady;

                if (eqready && GetBool("Combo.UseEQ"))
                {
                    var bestTarget = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsDashable() && GetDashPos(x).Distance(target) <= QRadius && (ShouldDive(x) || GetBool("Combo.ETower"))).FirstOrDefault();
                    if (bestTarget != null)
                    {
                        Spells[E].Cast(bestTarget);
                    }


                    else if (GetBool("Combo.UseQ2") && Spells[Q].IsReady() && TornadoReady && target.IsInRange(Qrange) && !InDash && !Yasuo.IsDashing())
                    {
                        CastQ(target);
                    }
                }


                else if (GetBool("Combo.UseQ2") && Spells[Q].IsReady() && TornadoReady && target.IsInRange(Qrange) && !InDash && !Yasuo.IsDashing())
                {
                    CastQ(target);
                }
            }


            if (GetBool("Combo.UseE") && !Helper.DontDash)
            {
                var mode = GetMode();
                if (mode == Modes.Old)
                {
                    CastEOld(CurrentTarget);
                }
                else
                {
                    CastENew(CurrentTarget);
                }
            }

            if (GetBool("Combo.StackQ") && !target.IsValidEnemy(Qrange) && !TornadoReady && !Yasuo.IsDashing() && !InDash)
            {
                var bestmin =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.IsValidMinion(Qrange) && MinionManager.IsMinion(x, false))
                        .MinOrDefault(x => x.Distance(Yasuo));
                if (bestmin != null)
                {
                    var pred = Spells[Q].GetPrediction(bestmin);

                    if (pred.Hitchance >= HitChance.Medium)
                    {
                        Spells[Q].Cast(pred.CastPosition);
                    }
                }
            }

        }

        internal void CastQ(AIHeroClient target)
        {
            if (target != null && !target.IsInRange(Qrange))
            {
                target = TargetSelector.GetTarget(Qrange, TargetSelector.DamageType.Physical);
            }

            if (target != null)
            {
                if (Spells[Q].IsReady() && target.IsValidEnemy(Qrange))
                {
                    UseQ(target, GetHitChance("Hitchance.Q"), GetBool("Combo.UseQ"), GetBool("Combo.UseQ2"));
                    return;
                }

                if (GetBool("Combo.StackQ") && !target.IsValidEnemy(Qrange) && !TornadoReady && !Yasuo.IsDashing() && !InDash)
                {
                    var bestmin =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(x => x.IsValidMinion(Qrange) && MinionManager.IsMinion(x, false))
                            .MinOrDefault(x => x.Distance(Yasuo));
                    if (bestmin != null)
                    {
                        var pred = Spells[Q].GetPrediction(bestmin);

                        if (pred.Hitchance >= HitChance.Medium)
                        {
                            Spells[Q].Cast(bestmin.ServerPosition);
                        }
                    }
                }
            }
        }

        internal void CastEOld(AIHeroClient target, bool force = false)
        {
            if (!SpellSlot.E.IsReady() || Helper.DontDash)
            {
                return;
            }

            var minionsinrange = ObjectManager.Get<Obj_AI_Minion>().Any(x => x.IsDashable());
            if (target == null || !target.IsInRange(minionsinrange ? Spells[E].Range * 2 : Spells[E].Range))
            {
                target = TargetSelector.GetTarget(minionsinrange ? Spells[E].Range * 2 : Spells[E].Range, TargetSelector.DamageType.Physical);
            }

            if (target != null)
            {
                if (isHealthy && target.Distance(Yasuo) >= 0.30 * Yasuo.AttackRange)
                {
                    if (TornadoReady)
                    {
                        if (SafetyCheck(target))
                        {
                            Spells[E].CastOnUnit(target);
                            return;
                        }
                    }

                    if (DashCount >= 1 && GetDashPos(target).IsCloser(target) && target.IsDashable())
                    {
                        if (SafetyCheck(target))
                        {
                            Spells[E].CastOnUnit(target);
                            return;
                        }
                    }

                    if (DashCount == 0)
                    {
                        var bestminion =
                            ObjectManager.Get<Obj_AI_Base>()
                                .Where(
                                    x =>
                                         x.IsDashable()
                                         && GetDashPos(x).IsCloser(target) && (SafetyCheck(x,true)))
                                .OrderBy(x => Vector2.Distance(GetDashPos(x), target.ServerPosition.To2D()))
                                .FirstOrDefault();
                        if (bestminion != null)
                        {
                            Spells[E].CastOnUnit(bestminion);
                        }

                        else if (target.IsDashable() && GetDashPos(target).IsCloser(target) && (SafetyCheck(target, true)))
                        {
                            Spells[E].CastOnUnit(target);
                        }
                    }


                    else
                    {
                        var minion =
                            ObjectManager.Get<Obj_AI_Base>()
                                .Where(x => x.IsDashable() && GetDashPos(x).IsCloser(target) && SafetyCheck(x,true))
                                .OrderBy(x => GetDashPos(x).Distance(target.ServerPosition)).FirstOrDefault();

                        if (minion != null && GetDashPos(minion).IsCloser(target))
                        {
                            Spells[E].CastOnUnit(minion);
                        }
                    }
                }
            }
        }


        internal void CastENew(AIHeroClient target)
        {
            if (!SpellSlot.E.IsReady() || Helper.DontDash || Yasuo.IsDashing() || InDash)
            {
                return;
            }


            var minionsinrange = ObjectManager.Get<Obj_AI_Minion>().Any(x => x.IsDashable());
            if (target == null || !target.IsInRange(minionsinrange ? Spells[E].Range * 2 : Spells[E].Range))
            {
                target = TargetSelector.GetTarget(minionsinrange ? Spells[E].Range * 2 : Spells[E].Range,
                    TargetSelector.DamageType.Physical);
            }

            if (target != null && SafetyCheck(target, true))
            {
                var dist = Yasuo.Distance(target);
                var pctOutOfRange = dist / Yasuo.AttackRange * 100;

                if (pctOutOfRange > 0.8f)
                {
                    if (target.IsDashable())
                    {
                        if (target.ECanKill())
                        {
                            return;
                        }

                        if (TornadoReady && target.IsInRange(Spells[E].Range) && targInKnockupRadius(target))
                        {
                            Spells[E].CastOnUnit(target);
                        }

                        //Stay in range
                        else if (pctOutOfRange > 0.8f)
                        {
                            var bestminion = ObjectManager.Get<Obj_AI_Base>()
                                .Where(x =>
                                    x.IsDashable()
                                    && GetDashPos(x).IsCloser(target) && SafetyCheck(x, true))
                                .MinOrDefault(x => GetDashPos(x).Distance(target));

                            var shouldETarget = bestminion == null || GetDashPos(target).Distance(target) <
                                                GetDashPos(bestminion).Distance(target);
                            if (shouldETarget && GetDashPos(target).IsCloser(target))
                            {
                                Spells[E].CastOnUnit(target);
                            }

                            else if (bestminion != null)
                            {
                                Spells[E].CastOnUnit(bestminion);
                            }
                        }
                    }

                    else
                    {
                        var minion = ObjectManager.Get<Obj_AI_Minion>()
                            .Where(x => x.IsDashable() && x.IsCloser(target) && SafetyCheck(x, true))
                            .MinOrDefault(x => GetDashPos(x).Distance(target));
                        if (minion != null)
                        {
                            Spells[E].CastOnUnit(minion);
                        }
                    }
                }
            }
        }




        private void UnitOnOnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (sender.IsMe && !args.IsBlink)
            {
                DashPosition = args.EndPos;
                LastDashTick = Helper.TickCount;
                var endpos = args.EndPos;

                if (GetBool("Combo.UseEQ"))
                {
                    if (SpellSlot.Q.IsReady())
                    {
                        var mode = Orbwalker.ActiveMode;
                        if (mode == Orbwalking.OrbwalkingMode.Combo || mode == Orbwalking.OrbwalkingMode.Mixed)
                        {
                            if (TornadoReady)
                            {
                                var goodTarget = endpos.To3D().GetEnemiesInRange(QRadius).Any(x => !x.ECanKill());
                                if (goodTarget)
                                {
                                    Spells[Q].Cast(endpos);
                                }


                                else if (GetBool("Combo.UseBeyBlade") && Spells[Flash].IsReady() && Spells[R].IsReady())
                                {
                                    var targ = TargetSelector.GetTarget(Yasuo.Distance(endpos) + Spells[Flash].Range, TargetSelector.DamageType.Physical);
                                    if (targ != null && (SafetyCheck(targ, true)) && endpos.Distance(targ.ServerPosition) <= 0.85f * Spells[Flash].Range)
                                    {
                                        Chat.Print("Beyblade 2");
                                        Spells[Q].Cast(endpos);
                                        Spells[Flash].Cast(targ.ServerPosition);
                                    }
                                }


                                else if (!TornadoReady)
                                {
                                    var targ = endpos.To3D().GetEnemiesInRange(QRadius).Any(x => !x.ECanKill());
                                    if (targ)
                                    {
                                        Spells[Q].Cast(endpos);
                                    }

                                    if (GetBool("Combo.StackQ"))
                                    {
                                        var nonkillableMin = endpos.GetMinionsInRange(QRadius).Any(x => !x.ECanKill());
                                        if (nonkillableMin)
                                        {
                                            Spells[Q].Cast(endpos);
                                            return;
                                        }
                                    }
                                }
                            }

                            else if (mode != Orbwalking.OrbwalkingMode.None && !TornadoReady)
                            {
                                if (endpos.To3D().MinionsInRange(QRadius) > 1 ||
                                    endpos.To3D().CountEnemiesInRange(QRadius) >= 1)
                                {
                                    Spells[Q].Cast(endpos);
                                }
                            }
                        }
                    }
                }
            }
        }


        void CastR(int minhit = 1)
        {
            UltMode ultmode = GetUltMode();

            List<AIHeroClient> ordered = new List<AIHeroClient>();

            if (ultmode == UltMode.Health)
            {
                ordered = KnockedUp.OrderBy(x => x.Health).ThenByDescending(x => TargetSelector.GetPriority(x)).ThenByDescending(x => x.CountEnemiesInRange(350)).ToList();
            }

            if (ultmode == UltMode.Priority)
            {
                ordered = KnockedUp.OrderByDescending(x => TargetSelector.GetPriority(x)).ThenBy(x => x.Health).ThenByDescending(x => x.CountEnemiesInRange(350)).ToList();
            }

            if (ultmode == UltMode.EnemiesHit)
            {
                ordered = KnockedUp.OrderByDescending(x => x.CountEnemiesInRange(350)).ThenByDescending(x => TargetSelector.GetPriority(x)).ThenBy(x => x.Health).ToList();
            }

            if (GetBool("Combo.UltOnlyKillable"))
            {
                var killable = ordered.FirstOrDefault(x => !x.isBlackListed() && x.Health <= Yasuo.GetSpellDamage(x, SpellSlot.R) && x.HealthPercent >= GetSliderInt("Combo.MinHealthUlt") && (GetBool("Combo.UltTower") || GetKeyBind("Misc.TowerDive") || ShouldDive(x)));
                if (killable != null && (!killable.IsInRange(Spells[Q].Range) || !isHealthy))
                {
                    Spells[R].CastOnUnit(killable);
                }
                return;
            }

            if ((GetBool("Combo.OnlyifMin") && ordered.Count() < minhit) || (ordered.Count() == 1 && ordered.FirstOrDefault().HealthPercent < GetSliderInt("Combo.MinHealthUlt")))
            {
                return;
            }

            if (GetBool("Combo.RPriority"))
            {
                var best = ordered.Find(x => !x.isBlackListed() && TargetSelector.GetPriority(x).Equals(2.5f) && (GetBool("Combo.UltTower") || GetKeyBind("Misc.TowerDive") || !x.Position.To2D().PointUnderEnemyTurret()));
                if (best != null && Yasuo.HealthPercent / best.HealthPercent <= 1)
                {
                    Spells[R].CastOnUnit(best);
                    return;
                }
            }

            if (ordered.Count() >= minhit)
            {
                var best2 = ordered.FirstOrDefault(x => !x.isBlackListed() && (GetBool("Combo.UltTower") || GetKeyBind("Misc.TowerDive") || !x.Position.To2D().PointUnderEnemyTurret()));
                if (best2 != null)
                {
                    Spells[R].CastOnUnit(best2);
                }
                return;
            }
        }

        void Flee()
        {
            Orbwalker.SetAttack(false);
            if (GetBool("Flee.UseQ2") && !Yasuo.IsDashing() && SpellSlot.Q.IsReady() && TornadoReady)
            {
                var qtarg = TargetSelector.GetTarget(Spells[Q2].Range, TargetSelector.DamageType.Physical);
                if (qtarg != null)
                {
                    var pred = Spells[Q].GetPrediction(qtarg);
                    if (pred.Hitchance >= HitChance.Medium)
                    {
                        Spells[Q2].Cast(pred.CastPosition);
                    }
                }
            }

            if (FleeMode == FleeType.ToCursor)
            {
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);

                var smart = GetBool("Flee.Smart");

                if (Spells[E].IsReady())
                {
                    if (smart)
                    {
                        Obj_AI_Base dashTarg;

                        if (Yasuo.ServerPosition.PointUnderEnemyTurret())
                        {
                            var closestturret =
                                ObjectManager.Get<Obj_AI_Turret>()
                                    .Where(x => x.IsEnemy)
                                    .MinOrDefault(y => y.Distance(Yasuo));

                            var potential =
                                ObjectManager.Get<Obj_AI_Base>()
                                    .Where(x => x.IsDashable())
                                    .MaxOrDefault(x => GetDashPos(x).Distance(closestturret));

                            if (potential != null)
                            {
                                var gdpos = GetDashPos(potential);
                                if (gdpos.Distance(Game.CursorPos) < Yasuo.Distance(Game.CursorPos) &&
                                    gdpos.Distance(closestturret.Position) - closestturret.BoundingRadius >
                                    Yasuo.Distance(closestturret.Position) - Yasuo.BoundingRadius)
                                {
                                    Spells[E].Cast(potential);
                                    return;
                                }
                            }
                        }

                        dashTarg = ObjectManager.Get<Obj_AI_Base>()
                           .Where(x => x.IsDashable())
                           .MinOrDefault(x => GetDashPos(x).Distance(Game.CursorPos));

                        if (dashTarg != null)
                        {
                            var posafdash = GetDashPos(dashTarg);

                            if (posafdash.Distance(Game.CursorPos) < Yasuo.Distance(Game.CursorPos) &&
                                !posafdash.PointUnderEnemyTurret())
                            {
                                Spells[E].CastOnUnit(dashTarg);
                            }
                        }
                    }

                    else
                    {
                        var dashtarg =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(x => x.IsDashable())
                                .MinOrDefault(x => GetDashPos(x).Distance(Game.CursorPos));

                        if (dashtarg != null)
                        {
                            var posafdash = GetDashPos(dashtarg);
                            if (posafdash.Distance(Game.CursorPos) < Yasuo.Distance(Game.CursorPos) && !posafdash.PointUnderEnemyTurret())
                            {
                                Spells[E].CastOnUnit(dashtarg);
                            }
                        }
                    }
                }

                if (GetBool("Flee.StackQ") && SpellSlot.Q.IsReady() && !TornadoReady && !Yasuo.IsDashing())
                {
                    Obj_AI_Minion qtarg = null;
                    if (!Spells[E].IsReady())
                    {
                        qtarg =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Find(x => x.IsValidTarget(Spells[Q].Range) && MinionManager.IsMinion(x));

                    }
                    else
                    {
                        var etargs =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(
                                    x => x.IsValidTarget(Spells[E].Range) && MinionManager.IsMinion(x) && x.IsDashable());
                        if (!etargs.Any())
                        {
                            qtarg =
                           ObjectManager.Get<Obj_AI_Minion>()
                               .Find(x => x.IsValidTarget(Spells[Q].Range) && MinionManager.IsMinion(x));
                        }
                    }

                    if (qtarg != null)
                    {
                        Spells[Q].Cast(qtarg.ServerPosition);
                    }
                }
            }

            if (FleeMode == FleeType.ToNexus)
            {
                var nexus = shop;
                if (nexus != null)
                {
                    Orbwalker.SetOrbwalkingPoint(nexus.Position);
                    var bestminion = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsDashable()).MinOrDefault(x => GetDashPos(x).Distance(nexus.Position));
                    if (bestminion != null && (!GetBool("Flee.Smart") || GetDashPos(bestminion).Distance(nexus.Position) < Yasuo.Distance(nexus.Position)))
                    {
                        Spells[E].CastOnUnit(bestminion);
                        if (GetBool("Flee.StackQ") && SpellSlot.Q.IsReady() && !TornadoReady)
                        {
                            Spells[Q].Cast(bestminion.ServerPosition);
                        }
                    }
                }
            }

            if (FleeMode == FleeType.ToAllies)
            {
                Obj_AI_Base bestally = HeroManager.Allies.Where(x => !x.IsMe && x.CountEnemiesInRange(300) == 0).MinOrDefault(x => x.Distance(Yasuo));
                if (bestally == null)
                {
                    bestally =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(x => x.IsValidAlly(3000))
                            .MinOrDefault(x => x.Distance(Yasuo));
                }

                if (bestally != null)
                {
                    Orbwalker.SetOrbwalkingPoint(bestally.ServerPosition);
                    if (Spells[E].IsReady())
                    {
                        var besttarget =
                            ObjectManager.Get<Obj_AI_Base>()
                                .Where(x => x.IsDashable())
                                .MinOrDefault(x => GetDashPos(x).Distance(bestally.ServerPosition));
                        if (besttarget != null)
                        {
                            Spells[E].CastOnUnit(besttarget);
                            if (GetBool("Flee.StackQ") && SpellSlot.Q.IsReady() && !TornadoReady)
                            {
                                Spells[Q].Cast(besttarget.ServerPosition);
                            }
                        }
                    }
                }

                else
                {
                    var nexus = shop;
                    if (nexus != null)
                    {
                        Orbwalker.SetOrbwalkingPoint(nexus.Position);
                        var bestminion = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsDashable()).MinOrDefault(x => GetDashPos(x).Distance(nexus.Position));
                        if (bestminion != null && GetDashPos(bestminion).Distance(nexus.Position) < Yasuo.Distance(nexus.Position))
                        {
                            Spells[E].CastOnUnit(bestminion);
                        }
                    }
                }
            }
        }


        void MoveToMouse()
        {
            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (Spells[E].IsReady())
            {
                var bestminion =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(x => x.IsDashable())
                        .MinOrDefault(x => GetDashPos(x).Distance(Game.CursorPos));

                if (bestminion != null)
                {
                    Spells[E].CastOnUnit(bestminion);
                }
            }
        }



        void CastIgnite()
        {
            var target =
                HeroManager.Enemies.Find(
                    x =>
                        x.IsValidEnemy(Spells[Ignite].Range) &&
                        Yasuo.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite) >= x.Health);

            if (Spells[Ignite].IsReady() && target != null)
            {
                Spells[Ignite].Cast(target);
            }
        }


        void Waveclear()
        {
            if (SpellSlot.Q.IsReady() && !Yasuo.IsDashing() && !InDash)
            {
                if (!TornadoReady && GetBool("Waveclear.UseQ") && Yasuo.Spellbook.IsAutoAttacking)
                {
                    var minion =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(x => x.IsValidMinion(Spells[Q].Range) && ((x.IsDashable() && (x.Health - Yasuo.GetSpellDamage(x, SpellSlot.Q) >= GetProperEDamage(x))) || (x.Health - Yasuo.GetSpellDamage(x, SpellSlot.Q) >= 0.15 * x.MaxHealth || x.QCanKill()))).MaxOrDefault(x => x.MaxHealth);
                    if (minion != null)
                    {
                        Spells[Q].Cast(minion.ServerPosition);
                    }
                }

                else if (TornadoReady && GetBool("Waveclear.UseQ2"))
                {
                    var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Distance(Yasuo) > Yasuo.AttackRange && x.IsValidMinion(Spells[Q2].Range) && ((x.IsDashable() && x.Health - Yasuo.GetSpellDamage(x, SpellSlot.Q) >= 0.75 * GetProperEDamage(x)) || (x.Health - Yasuo.GetSpellDamage(x, SpellSlot.Q) >= 0.10 * x.MaxHealth) || x.CanKill(SpellSlot.Q)));
                    var pred =
                        MinionManager.GetBestLineFarmLocation(minions.Select(m => m.ServerPosition.To2D()).ToList(),
                            Spells[Q2].Width, Spells[Q2].Range);
                    if (pred.MinionsHit >= GetSliderInt("Waveclear.Qcount"))
                    {
                        Spells[Q2].Cast(pred.Position);
                        LastTornadoClearTick = Helper.TickCount;
                    }
                }
            }

            if (Helper.TickCount - LastTornadoClearTick < 500)
            {
                return;
            }


            if (SpellSlot.E.IsReady() && GetBool("Waveclear.UseE") && (!GetBool("Waveclear.Smart") || isHealthy) && (Helper.TickCount - WCLastE) >= GetSliderInt("Waveclear.Edelay"))
            {
                var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsDashable() && ((GetBool("Waveclear.UseENK") && (!GetBool("Waveclear.Smart") || x.Health - GetProperEDamage(x) > GetProperEDamage(x) * 3)) || x.ECanKill()) && (GetBool("Waveclear.ETower") || ShouldDive(x)));
                Obj_AI_Minion minion = null;
                minion = minions.OrderBy(x => x.ECanKill()).ThenBy(x => GetDashPos(x).MinionsInRange(200)).FirstOrDefault();
                if (minion != null)
                {
                    Spells[E].Cast(minion);
                    WCLastE = Helper.TickCount;
                }
            }

            if (GetBool("Waveclear.UseItems"))
            {
                if (GetBool("Waveclear.UseTIA"))
                {
                    Tiamat.minioncount = GetSliderInt("Waveclear.MinCountHDR");
                    Tiamat.Cast(null, true);
                }
                if (GetBool("Waveclear.UseTitanic"))
                {
                    Titanic.minioncount = GetSliderInt("Waveclear.MinCountHDR");
                    Titanic.Cast(null, true);
                }
                if (GetBool("Waveclear.UseHDR"))
                {
                    Hydra.minioncount = GetSliderInt("Waveclear.MinCountHDR");
                    Hydra.Cast(null, true);
                }
                if (GetBool("Waveclear.UseYMU"))
                {
                    Youmu.minioncount = GetSliderInt("Waveclear.MinCountYOU");
                    Youmu.Cast(null, true);
                }
            }
        }



        void NewWaveClear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidMinion(Spells[Q2].Range));

            Func<bool> wcQ = delegate
            {
                if (Spells[Q].IsReady() && GetBool("Waveclear.UseQ") && !TornadoReady && !Yasuo.IsDashing())
                {
                    var minion =
                                  ObjectManager.Get<Obj_AI_Minion>()
                                      .Where(x => x.IsValidMinion(Spells[Q].Range) && (x.QCanKill() || x.HealthPercent > 75)).MinOrDefault(x => x.Health);
                    if (minion != null)
                    {
                        var pred = Spells[Q].GetPrediction(minion);
                        if (pred.Hitchance >= HitChance.Medium)
                        {
                            Spells[Q].Cast(pred.CastPosition);
                            return true;
                        }
                    }
                }
                return false;
            };

            Func<bool> wcQDash = delegate
            {
                if (Spells[Q].IsReady() && GetBool("Waveclear.UseQ") && !TornadoReady && Yasuo.IsDashing())
                {
                    var dashinfo = Dash.GetDashInfo(Yasuo);
                    if (dashinfo.EndPos.To3D().MinionsInRange(QRadius) >= GetSliderInt("Waveclear.Qcount"))
                    {
                        Spells[Q].Cast();
                        return true;
                    }
                }
                return false;
            };

            Func<bool> wcQ2 = delegate
            {
                if (Spells[Q].IsReady() && GetBool("Waveclear.UseQ2") && TornadoReady)
                {
                    var minionsQ2 =
                      ObjectManager.Get<Obj_AI_Minion>()
                          .Where(x => x.IsValidMinion(Spells[Q2].Range) && (x.Health > Yasuo.GetAutoAttackDamage(x, false) || !x.IsInRange(Orbwalking.GetRealAutoAttackRange(x))) && (x.QCanKill(true) || x.HealthPercent > 60));

                    var pred = MinionManager.GetBestLineFarmLocation(minionsQ2.Select(m => m.ServerPosition.To2D()).ToList(),
                        Spells[Q2].Width, Spells[Q2].Range);

                    if (pred.MinionsHit >= GetSliderInt("Waveclear.Qcount"))
                    {
                        Spells[Q2].Cast(pred.Position);
                        LastTornadoClearTick = Helper.TickCount;
                        return true;
                    }
                }

                return false;
            };

            Func<bool> wcE = delegate
            {
                if (Spells[E].IsReady() && GetBool("Waveclear.UseE") && isHealthy && TickCount - LastTornadoClearTick > 500 && (Helper.TickCount - WCLastE) >= GetSliderInt("Waveclear.Edelay"))
                {
                    var minion =
                      ObjectManager.Get<Obj_AI_Minion>()
                          .Where(x => x.IsValidMinion(Spells[E].Range) && x.IsDashable() && ((GetBool("Waveclear.UseENK") && x.HealthPercent > 60 || x.ECanKill()) && (GetBool("Waveclear.ETower") || ShouldDive(x)))).MinOrDefault(x => x.Health);

                    if (minion != null)
                    {
                        Spells[E].Cast(minion);
                        WCLastE = Helper.TickCount;
                        return true;
                    }
                }
                return false;
            };

            if (!wcQ() && !wcQDash() && !wcQ2())
            {
                wcE();
            }

            if (GetBool("Waveclear.UseItems"))
            {
                if (GetBool("Waveclear.UseTIA"))
                {
                    Tiamat.minioncount = GetSliderInt("Waveclear.MinCountHDR");
                    Tiamat.Cast(null, true);
                }
                if (GetBool("Waveclear.UseTitanic"))
                {
                    Titanic.minioncount = GetSliderInt("Waveclear.MinCountHDR");
                    Titanic.Cast(null, true);
                }
                if (GetBool("Waveclear.UseHDR"))
                {
                    Hydra.minioncount = GetSliderInt("Waveclear.MinCountHDR");
                    Hydra.Cast(null, true);
                }
                if (GetBool("Waveclear.UseYMU"))
                {
                    Youmu.minioncount = GetSliderInt("Waveclear.MinCountYOU");
                    Youmu.Cast(null, true);
                }
            }
        }


        void Killsteal()
        {
            if (SpellSlot.Q.IsReady() && GetBool("Killsteal.UseQ"))
            {
                var targ = HeroManager.Enemies.Find(x => x.CanKill(SpellSlot.Q) && x.IsInRange(Qrange));
                if (targ != null)
                {
                    UseQ(targ, GetHitChance("Hitchance.Q"));
                    return;
                }
            }

            if (SpellSlot.E.IsReady() && GetBool("Killsteal.UseE"))
            {
                var targ = HeroManager.Enemies.Find(x => x.CanKill(SpellSlot.E) && x.IsInRange(Spells[E].Range));
                if (targ != null)
                {
                    Spells[E].Cast(targ);
                    return;
                }
            }

            if (SpellSlot.R.IsReady() && GetBool("Killsteal.UseR"))
            {
                var targ = KnockedUp.Find(x => x.CanKill(SpellSlot.R) && x.IsValidEnemy(Spells[R].Range) && !x.isBlackListed());
                if (targ != null)
                {
                    Spells[R].Cast(targ);
                    return;
                }
            }

            if (GetBool("Killsteal.UseIgnite"))
            {
                CastIgnite();
                return;
            }

            if (GetBool("Killsteal.UseItems"))
            {
                if (Tiamat.item.IsReady())
                {
                    var targ =
                        HeroManager.Enemies.Find(
                            x =>
                                x.IsValidEnemy(Tiamat.item.Range) &&
                                x.Health <= Yasuo.GetItemDamage(x, Damage.DamageItems.Tiamat));
                    if (targ != null)
                    {
                        Tiamat.Cast(null);
                    }
                }

                if (Titanic.item.IsReady())
                {
                    var targ =
                        HeroManager.Enemies.Find(
                            x =>
                                x.IsValidEnemy(Titanic.item.Range) &&
                                x.Health <= Yasuo.GetItemDamage(x, Damage.DamageItems.Tiamat));
                    if (targ != null)
                    {
                        Titanic.Cast(null);
                    }
                }
                if (Hydra.item.IsReady())
                {
                    var targ =
                      HeroManager.Enemies.Find(
                      x =>
                          x.IsValidEnemy(Hydra.item.Range) &&
                          x.Health <= Yasuo.GetItemDamage(x, Damage.DamageItems.Tiamat));
                    if (targ != null)
                    {
                        Hydra.Cast(null);
                    }
                }
                if (Blade.item.IsReady())
                {
                    var targ = HeroManager.Enemies.Find(
                     x =>
                         x.IsValidEnemy(Blade.item.Range) &&
                         x.Health <= Yasuo.GetItemDamage(x, Damage.DamageItems.Botrk));
                    if (targ != null)
                    {
                        Blade.Cast(targ);
                    }
                }
                if (Bilgewater.item.IsReady())
                {
                    var targ = HeroManager.Enemies.Find(
                                   x =>
                                       x.IsValidEnemy(Bilgewater.item.Range) &&
                                       x.Health <= Yasuo.GetItemDamage(x, Damage.DamageItems.Bilgewater));
                    if (targ != null)
                    {
                        Bilgewater.Cast(targ);
                    }
                }
            }
        }

        void Harass()
        {
            //No harass under enemy turret to avoid aggro
            if (Yasuo.ServerPosition.PointUnderEnemyTurret())
            {
                return;
            }

            var target = TargetSelector.GetTarget(Qrange, TargetSelector.DamageType.Physical);
            if (target != null)
            {
                if (SpellSlot.Q.IsReady() && target.IsInRange(Qrange))
                {
                    UseQ(target, GetHitChance("Hitchance.Q"), GetBool("Harass.UseQ"), GetBool("Harass.UseQ2"));
                }

                if (isHealthy && GetBool("Harass.UseE") && Spells[E].IsReady() &&
                    target.IsInRange(Spells[E].Range * 3) && !target.Position.To2D().PointUnderEnemyTurret())
                {
                    if (target.IsInRange(Spells[E].Range))
                    {
                        Spells[E].CastOnUnit(target);
                        return;
                    }

                    var minion =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(x => x.IsDashable() && !x.ServerPosition.To2D().PointUnderEnemyTurret())
                            .OrderBy(x => GetDashPos(x).Distance(target.ServerPosition))
                            .FirstOrDefault();

                    if (minion != null && GetBool("Harass.UseEMinion") && GetDashPos(minion).IsCloser(target))
                    {
                        Spells[E].Cast(minion);
                    }
                }
            }
        }

        void Mixed()
        {
            if (GetBool("Harass.InMixed"))
            {
                Harass();
            }
            LHSkills();
        }


        void LHSkills()
        {
            if (SpellSlot.Q.IsReady() && !Yasuo.IsDashing())
            {
                if (!TornadoReady && GetBool("Farm.UseQ"))
                {
                    var minion =
                         ObjectManager.Get<Obj_AI_Minion>()
                             .FirstOrDefault(x => x.IsValidMinion(Spells[Q].Range) && x.QCanKill());
                    if (minion != null)
                    {
                        Spells[Q].Cast(minion.ServerPosition);
                    }
                }

                else if (TornadoReady && GetBool("Farm.UseQ2"))
                {
                    var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Distance(Yasuo) > Yasuo.AttackRange && x.IsValidMinion(Spells[Q2].Range) && (x.QCanKill()));
                    var pred =
                        MinionManager.GetBestLineFarmLocation(minions.Select(m => m.ServerPosition.To2D()).ToList(),
                            Spells[Q2].Width, Spells[Q2].Range);
                    if (pred.MinionsHit >= GetSliderInt("Farm.Qcount"))
                    {
                        Spells[Q2].Cast(pred.Position);
                    }
                }
            }

            if (Spells[E].IsReady() && GetBool("Farm.UseE"))
            {
                var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsDashable() && x.ECanKill() && (GetBool("Waveclear.ETower") || ShouldDive(x)));
                if (minion != null)
                {
                    Spells[E].Cast(minion);
                }
            }
        }



        void OnGapClose(ActiveGapcloser args)
        {
            if (Yasuo.ServerPosition.PointUnderEnemyTurret())
            {
                return;
            }
            if (GetBool("Misc.AG") && TornadoReady && Yasuo.Distance(args.End) <= 500)
            {
                var pred = Spells[Q2].GetPrediction(args.Sender);
                if (pred.Hitchance >= GetHitChance("Hitchance.Q"))
                {
                    Spells[Q2].Cast(pred.CastPosition);
                }
            }
        }

        void OnInterruptable(AIHeroClient sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Yasuo.ServerPosition.PointUnderEnemyTurret())
            {
                return;
            }
            if (GetBool("Misc.Interrupter") && TornadoReady && Yasuo.Distance(sender.ServerPosition) <= 500)
            {
                if (args.EndTime >= Spells[Q2].Delay)
                {
                    Spells[Q2].Cast(sender.ServerPosition);
                }
            }
        }
    }

}

