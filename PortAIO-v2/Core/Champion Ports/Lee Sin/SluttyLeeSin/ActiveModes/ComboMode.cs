using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Lee_Sin.WardManager;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Lee_Sin.ActiveModes
{
    class ComboMode : LeeSin
    {
        public static void Combo()
        {
            #region R combos

            var unit =
                HeroManager.Enemies.Where(
                    x => x.Distance(Player) < 500 && !x.IsDead && x.IsValidTarget(500) && x.Health < R.GetDamage(x) + 50)
                    .OrderBy(x => x.Distance(Player))
                    .FirstOrDefault();
            if (unit != null)
            {
                foreach (var targets in
                    HeroManager.Enemies.Where(
                        x =>
                            !x.IsDead && x.IsValidTarget() && x.IsVisible && x.Distance(unit) < 1000 &&
                            x.Distance(unit) > 300 && x.NetworkId != unit.NetworkId && x.Health < R.GetDamage(x)))
                {
                    var prediction = Prediction.GetPrediction(targets, 0.1f);

                    var pos = prediction.UnitPosition.Extend(unit.ServerPosition,
                        prediction.UnitPosition.Distance(unit.ServerPosition) + 250);

                    RCombo = pos;

                    var slot = WardSorter.Wards();
                    if (unit.Distance(Player) > 500)
                    {
                        RCombo = null;
                    }

                    if (W.IsReady() && R.IsReady() && Player.ServerPosition.Distance(unit.ServerPosition) < 500 &&
                        slot != null)
                    {
                        WardJump.WardJumped(pos, false, true);
                    }
                }

                if (Player.IsDead)
                {
                    UltPoly = null;
                    UltPolyExpectedPos = null;
                    return;
                }

                UltPoly = new Geometry.Polygon.Rectangle(Player.ServerPosition,
                    Player.ServerPosition.Extend(unit.Position, 1100), unit.BoundingRadius + 30);

                var counts =
                    HeroManager.Enemies.Where(
                        x => x.Distance(Player) < 1100 && x.IsValidTarget(1100) && x.Health < R.GetDamage(x))
                        .Count(h => h.NetworkId != unit.NetworkId && UltPoly.IsInside(h.ServerPosition));

                if (counts >= 1 && R.IsReady() && Created && R.IsReady())
                {
                    R.Cast(unit);
                }
            }

            #endregion

            #region Regular combo

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (!target.IsValidTarget())
                return;

            var useq = GetBool("useq", typeof (bool));
            var usee = GetBool("usee", typeof (bool));
            var user = GetBool("user", typeof (bool));
            var usew = GetBool("wardjumpcombo", typeof (bool));
            var smite = GetBool("usessmite", typeof (bool));
            if (GetStringValue("hydrati") == 0 || GetStringValue("hydrati") == 2)
            {
                if (target.IsValidTarget(400) && (ItemReady(Tiamat) || ItemReady(Hydra)) &&
                    (HasItem(Tiamat) || HasItem(Hydra)))
                {
                    SelfCast(HasItem(Hydra) ? Hydra : Tiamat);
                }
            }

            if (GetBool("youm", typeof (bool)) && HasItem(Youm) && ItemReady(Youm) &&
                target.Distance(Player) < Q.Range - 300)
            {
                SelfCast(Youm);
            }

            if (GetBool("omen", typeof (bool)) && HasItem(Omen) && ItemReady(Omen) &&
                Player.CountAlliesInRange(400) >= GetValue("minrand"))
            {
                SelfCast(Omen);
            }
            if (usew)
            {
                if (Environment.TickCount - Lastqc > 300 && Environment.TickCount - Laste > 300 &&
                    Environment.TickCount - Lastwcombo > 300)
                {
                    if (W.IsReady() && target.Distance(Player) <= Player.AttackRange && W1())
                    {
                        W.Cast(Player);
                        Lastwcombo = Environment.TickCount;
                    }

                    if (W.IsReady() && target.Distance(Player) <= Player.AttackRange && W2() && !HasPassive())
                    {
                        W.Cast();
                    }
                }
            }

            if (useq)
            {
                if (Environment.TickCount - Lastqc > 300 && Environment.TickCount - Laste > 300 &&
                    Environment.TickCount - Lastwcombo > 300)
                {
                    var qpred = Q.GetPrediction(target);
                    if (Q.IsReady() && Q1())
                    {
                        OnUpdate.CastSpell(Q, target);
                        Lastqc = Environment.TickCount;
                    }

                    if (Q2() && Q.IsReady() && GetBool("useq2", typeof (bool)))
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add(GetValue("secondqdelay"), () => Q.Cast());
                        Lastqc = Environment.TickCount;
                    }
                }
            }

            if (usee)
            {
                    if (target.Distance(Player) <= E.Range && E1())
                    {
                        E.Cast();
                        Laste = Environment.TickCount;
                    }
                    if ((Player.Distance(target) >
                         Player.AttackRange + Player.BoundingRadius + target.BoundingRadius + 100 ||
                         Environment.TickCount - Laste > 2300) && E2())
                    {
                        E.Cast();
                        Laste = Environment.TickCount;
                    }
                
            }

            if (user && target.IsValidTarget(R.Range) && R.IsReady())
            {
                if (Q.GetDamage(target) + 70 < target.Health)
                {
                   // Chat.Print("firstcheck");
                    if (target.Health > Player.GetAutoAttackDamage(target) + 30)
                    {
                     //   Chat.Print("secondCheck");
                        if (Q.IsReady() &&
                            target.Health <=
                            R.GetDamage(target) + GetQDamage(target) + Player.GetAutoAttackDamage(target) &&
                            Q.IsReady() && target.Health > GetQDamage(target))
                        {
                            R.Cast(target);
                        }

                        if (target.Health <= R.GetDamage(target) + Q.GetDamage(target) && Q.IsReady() &&
                            Player.Mana > 30)
                        {
                            R.Cast(target);
                        }
                    }
                }
            }


            if (Smite.IsReady() && target.Distance(Player) < 500 && smite && target.Health < ActiveModes.Smite.GetFuckingSmiteDamage())
            {
                Player.Spellbook.CastSpell(Smite, target);
            }

            var poss = Player.ServerPosition.Extend(target.ServerPosition, 600);
            if (!GetBool("wardjumpcombo1", typeof(bool))) return;

            if (!E.IsReady() || !W.IsReady() || !(target.Distance(Player) > E.Range) || !LastQ(target)) return;
            if (!Q.IsReady() && Environment.TickCount - Q.LastCastAttemptT > 1000)
            {
                WardManager.WardJump.WardJumped(poss, true, true);
            }

            #endregion
        }
    }
}
