using EloBuddy; 
using LeagueSharp.Common; 
namespace Flowers_Yasuo.Manager.Events.Games.Mode
{
    using System.Linq;
    using Spells;
    using LeagueSharp;
    using LeagueSharp.Common;
    using Orbwalking = Orbwalking;
    using static Common.Common;

    internal class Combo : Logic
    {
        internal static void Init()
        {
            var target = TargetSelector.GetSelectedTarget() ??
                         TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Physical);

            if (target == null)
            {
                return;
            }

            if (target.DistanceToPlayer() > R.Range)
            {
                return;
            }

            if (Menu.Item("ComboIgnite", true).GetValue<bool>() && Ignite != SpellSlot.Unknown && Ignite.IsReady()
                && target.IsValidTarget(600f)
                && (target.Health <= Me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite)
                    || target.HealthPercent <= 25))
            {
                Me.Spellbook.CastSpell(Ignite, target);
            }

            if (Menu.Item("ComboItems", true).GetValue<bool>())
            {
                SpellManager.UseItems(target, true);
            }

            if (Menu.Item("ComboR", true).GetValue<KeyBind>().Active && R.IsReady())
            {
                var rEmemies = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range))
                    .Where(x => x.HasBuffOfType(BuffType.Knockup) || x.HasBuffOfType(BuffType.Knockback));

                if (rEmemies.Count() >= Menu.Item("ComboRCount", true).GetValue<Slider>().Value)
                {
                    R.Cast();
                }

                foreach (
                    var rTarget in
                    HeroManager.Enemies.Where(
                            x =>
                                x.IsValidTarget(R.Range) &&
                                Menu.Item("R" + x.ChampionName.ToLower(), true).GetValue<bool>())
                        .Where(x => x.HasBuffOfType(BuffType.Knockup) || x.HasBuffOfType(BuffType.Knockback)))
                {
                    if (rTarget.HealthPercent <= Menu.Item("ComboRHp", true).GetValue<Slider>().Value)
                    {
                        if (CanCastDelayR(rTarget))
                        {
                            R.Cast();
                        }
                    }
                }
            }

            if (Menu.Item("ComboE", true).GetValue<bool>() && E.IsReady())
            {
                var dmg = (float)(SpellManager.GetQDmg(target) * 2 + SpellManager.GetEDmg(target)) +
                          Me.GetAutoAttackDamage(target) * 2 +
                          (R.IsReady() ? R.GetDamage(target) : (float)SpellManager.GetQDmg(target));

                if (target.DistanceToPlayer() >= Orbwalking.GetRealAutoAttackRange(Me) + 65 &&
                    dmg >= target.Health && SpellManager.CanCastE(target) &&
                    (Menu.Item("ComboETurret", true).GetValue<bool>() || !UnderTower(PosAfterE(target))))
                {
                    E.CastOnUnit(target, true);
                }
            }

            if (Menu.Item("ComboEGapcloser", true).GetValue<bool>() && E.IsReady() &&
                target.DistanceToPlayer() >= Menu.Item("ComboEGap", true).GetValue<Slider>().Value)
            {
                if (Menu.Item("ComboEMode", true).GetValue<StringList>().SelectedIndex == 0)
                {
                    SpellManager.EGapTarget(target, Menu.Item("ComboETurret", true).GetValue<bool>(),
                        Menu.Item("ComboEGap", true).GetValue<Slider>().Value, false);
                }
                else
                {
                    SpellManager.EGapMouse(target, Menu.Item("ComboETurret", true).GetValue<bool>(),
                        Menu.Item("ComboEGap", true).GetValue<Slider>().Value, false);
                }
            }

            if (Menu.Item("ComboQ", true).GetValue<bool>() && Me.Spellbook.GetSpell(SpellSlot.Q).IsReady() && !IsDashing)
            {
                if (SpellManager.HaveQ3)
                {
                    if (target.IsValidTarget(Q3.Range))
                    {
                        SpellManager.CastQ3();
                    }
                }
                else
                {
                    if (target.IsValidTarget(Q.Range))
                    {
                        Q.Cast(target, true);
                    }
                }
            }

            if (Me.IsDashing() && Menu.Item("ComboEQFlash", true).GetValue<KeyBind>().Active && Flash != SpellSlot.Unknown &&
                Flash.IsReady() && SpellManager.HaveQ3 && Q3.IsReady() && R.IsReady())
            {
                if (Menu.Item("ComboEQFlashSolo", true).GetValue<bool>() &&
                    !HeroManager.Enemies.Any(x => x.IsValidTarget(1200) && x.NetworkId != target.NetworkId) &&
                    !HeroManager.Allies.Any(x => x.IsValidTarget(1200, false) && x.NetworkId != Me.NetworkId))
                {
                    if (target.Health + target.HPRegenRate * 2 <
                        SpellManager.GetQDmg(target) +
                        (SpellManager.CanCastE(target) ? SpellManager.GetEDmg(target) : 0) +
                        Me.GetAutoAttackDamage(target) * 2 + R.GetDamage(target) &&
                        !HeroManager.Enemies.Any(x => x.IsValidTarget(1200) && x.NetworkId != target.NetworkId) &&
                        !HeroManager.Allies.Any(x => x.IsValidTarget(1200, false) && x.NetworkId != Me.NetworkId))
                    {
                        var bestPos = FlashPoints().FirstOrDefault(x => target.Distance(x) <= 220);

                        if (bestPos.IsValid() && bestPos.CountEnemiesInRange(220) > 0 && Q3.Cast(bestPos, true))
                        {
                            LeagueSharp.Common.Utility.DelayAction.Add(10 + (Game.Ping / 2 - 5),
                                               () => Me.Spellbook.CastSpell(Flash, bestPos));
                        }
                    }
                }

                if (Menu.Item("ComboEQFlashTeam", true).GetValue<bool>() &&
                    HeroManager.Enemies.Count(x => x.IsValidTarget(1200)) >=
                    Menu.Item("ComboEQFlashTeamCount", true).GetValue<Slider>().Value &&
                    HeroManager.Allies.Count(x => x.IsValidTarget(1200, false) && x.NetworkId != Me.NetworkId) >=
                    Menu.Item("ComboEQFlashTeamAlly", true).GetValue<Slider>().Value)
                {
                    var bestPos =
                        FlashPoints()
                            .Where(
                                x =>
                                    HeroManager.Enemies.Count(a => a.IsValidTarget(600f, true, x)) >=
                                    Menu.Item("ComboEQFlashTeamCount", true).GetValue<Slider>().Value)
                            .OrderByDescending(x => HeroManager.Enemies.Count(i => i.Distance(x) <= 220))
                            .FirstOrDefault();

                    if (bestPos.IsValid() &&
                        bestPos.CountEnemiesInRange(220) >=
                        Menu.Item("ComboEQFlashTeamCount", true).GetValue<Slider>().Value && Q3.Cast(bestPos, true))
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add(10 + (Game.Ping / 2 - 5),
                                           () => Me.Spellbook.CastSpell(Flash, bestPos));
                    }
                }
            }

            if (IsDashing)
            {
                if (Menu.Item("ComboEQ", true).GetValue<bool>() && Q.IsReady() && !SpellManager.HaveQ3 &&
                    target.Distance(lastEPos) <= 220)
                {
                    Q.Cast(Me.Position, true);
                }

                if (Menu.Item("ComboEQ3", true).GetValue<bool>() && Q3.IsReady() && SpellManager.HaveQ3 &&
                    target.Distance(lastEPos) <= 220)
                {
                    Q3.Cast(Me.Position, true);
                }

                if (Menu.Item("ComboQStack", true).GetValue<StringList>().SelectedIndex != 3 && Q.IsReady() &&
                    !SpellManager.HaveQ3)
                {
                    switch (Menu.Item("ComboQStack", true).GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            if (MinionManager.GetMinions(lastEPos, 220, MinionTypes.All, MinionTeam.NotAlly).Count > 0 ||
                                HeroManager.Enemies.Count(x => x.IsValidTarget(220, true, lastEPos)) > 0)
                            {
                                Q.Cast(Me.Position, true);
                            }
                            break;
                        case 1:
                            if (HeroManager.Enemies.Count(x => x.IsValidTarget(220, true, lastEPos)) > 0)
                            {
                                Q.Cast(Me.Position, true);
                            }
                            break;
                        case 2:
                            if (MinionManager.GetMinions(lastEPos, 220, MinionTypes.All, MinionTeam.NotAlly).Count > 0)
                            {
                                Q.Cast(Me.Position, true);
                            }
                            break;
                    }
                }
            }
        }
    }
}
