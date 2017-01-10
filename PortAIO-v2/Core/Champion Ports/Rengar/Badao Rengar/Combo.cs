using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace BadaoRengar
{
    public static class Combo
    {
        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        public static void BadaoActivate()
        {
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            CustomEvents.Unit.OnDash += Unit_OnDash;
        }

        public static void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (!sender.IsMe)
                return;
            if (Variables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;

            var mode = Variables.ComboMode.GetValue<StringList>().SelectedValue;
            if (Helper.HasItem())
            {
                if (args.Duration - 100 - Game.Ping / 2 > 0)
                {
                    LeagueSharp.Common.Utility.DelayAction.Add((args.EndTick - Environment.TickCount - Game.Ping - 150),() => Helper.CastItem());
                }
                else
                {
                    Helper.CastItem();
                }
            }

            if (Player.Mana < 4)
            {
                var targetE = TargetSelector.GetTarget(Variables.E.Range, TargetSelector.DamageType.Physical);
                if (Variables.E.IsReady() && targetE.IsValidTarget() && !targetE.IsZombie)
                {
                    Helper.CastE(targetE);
                }
            }
            if (mode == "E")
            {
                if (Player.Mana == 4)
                {
                    var targetE = TargetSelector.GetTarget(Variables.E.Range, TargetSelector.DamageType.Physical);
                    if (Variables.E.IsReady() && targetE.IsValidTarget() && !targetE.IsZombie)
                    {
                        Helper.CastE(targetE);
                    }
                }
            }
            
            //Chat.Say("dash");
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Variables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;

            if (Variables.ComboMode.GetValue<StringList>().SelectedValue != "Q" && Player.Mana == 4)
            {
                if (Helper.HasItem())
                    Helper.CastItem();
            }
            else if (Variables.Q.IsReady())
            {
                Variables.Q.Cast(target as Obj_AI_Base);
            }
            else if (Helper.HasItem())
            {
                Helper.CastItem();
            }
            else if (Variables.E.IsReady())
            {
                var targetE = TargetSelector.GetTarget(Variables.E.Range, TargetSelector.DamageType.Physical);
                if (Variables.E.IsReady() && targetE.IsValidTarget() && !targetE.IsZombie)
                {
                    Helper.CastE(targetE);
                }
            }
        }

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Variables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;

            if (args.Unit.IsMe && Variables.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
            }

        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Variables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;

            if (Helper.HasSmite && Variables.ComboSmite.GetValue<bool>())
            {
                if (Helper.hasSmiteRed || Helper.hasSmiteBlue)
                {
                    var target = TargetSelector.GetTarget(650, TargetSelector.DamageType.Physical);
                    if (target.IsValidTarget() && !target.IsZombie && Player.Distance(target.Position) <= Player.BoundingRadius + 500 + target.BoundingRadius)
                    {
                        Player.Spellbook.CastSpell(Variables.Smite, target);
                    }
                }
            }

            if (!Player.HasBuff("RengarR"))
            {
                if (Player.Mana < 4)
                {
                    var targetQ = TargetSelector.GetTarget(Variables.Q.Range, TargetSelector.DamageType.Physical);
                    if (Variables.Q.IsReady() && targetQ.IsValidTarget() && !targetQ.IsZombie)
                    {
                        if (!Player.IsDashing() && Orbwalking.CanMove(90)
                           && !(Orbwalking.CanAttack() && HeroManager.Enemies.Any(x => x.IsValidTarget() && Orbwalking.InAutoAttackRange(x))))
                        {
                            Variables.Q.Cast(targetQ);
                        }
                    }
                    var targetW = TargetSelector.GetTarget(500, TargetSelector.DamageType.Magical);
                    if (Variables.W.IsReady() && targetW.IsValidTarget() && !targetW.IsZombie)
                    {
                        Variables.W.Cast(targetW);
                    }
                    if (Player.IsDashing() || Orbwalking.CanMove(90)
                        && !(Orbwalking.CanAttack() && HeroManager.Enemies.Any(x => x.IsValidTarget() && Orbwalking.InAutoAttackRange(x))))
                    {
                        var targetE = TargetSelector.GetTarget(Variables.E.Range, TargetSelector.DamageType.Physical);
                        if (Variables.E.IsReady() && targetE.IsValidTarget() && !targetE.IsZombie)
                        {
                            Helper.CastE(targetE);
                        }
                    }

                }
                if (Player.Mana == 4)
                {
                    if (Variables.ComboMode.GetValue<StringList>().SelectedValue == "E")
                    {
                        if (Player.IsDashing() || Orbwalking.CanMove(90)
                         && !(Orbwalking.CanAttack() && HeroManager.Enemies.Any(x => x.IsValidTarget() && Orbwalking.InAutoAttackRange(x))))
                        {
                            var targetE = TargetSelector.GetTarget(Variables.E.Range, TargetSelector.DamageType.Physical);
                            if (Variables.E.IsReady() && targetE.IsValidTarget() && !targetE.IsZombie)
                            {
                                Helper.CastE(targetE);
                            }
                        }
                    }
                    if (Variables.ComboMode.GetValue<StringList>().SelectedValue == "W")
                    {
                        var targetW = TargetSelector.GetTarget(500, TargetSelector.DamageType.Magical);
                        if (Variables.W.IsReady() && targetW.IsValidTarget() && !targetW.IsZombie)
                        {
                            Variables.W.Cast(targetW);
                        }
                    }
                    if (Variables.ComboMode.GetValue<StringList>().SelectedValue == "Q")
                    {
                        var targetQ = TargetSelector.GetTarget(Variables.Q.Range, TargetSelector.DamageType.Physical);
                        if (Variables.Q.IsReady() && targetQ.IsValidTarget() && !targetQ.IsZombie)
                        {
                            if (!Player.IsDashing() && Orbwalking.CanMove(90)
                               && !(Orbwalking.CanAttack() && HeroManager.Enemies.Any(x => x.IsValidTarget() && Orbwalking.InAutoAttackRange(x))))
                            {
                                Variables.Q.Cast(targetQ);
                            }
                        }
                    }
                }
            }
        
        }
    }
}
