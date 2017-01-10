
#pragma warning disable 1587

using EloBuddy; 
using LeagueSharp.SDK; 
namespace ExorAIO.Champions.Udyr
{
    using System;
    using System.Linq;

    using ExorAIO.Utilities;

    using LeagueSharp;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;

    /// <summary>
    ///     The logics class.
    /// </summary>
    internal partial class Logics
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void BuildingClear(EventArgs args)
        {
            if (!(Variables.Orbwalker.GetTarget() is Obj_HQ) && !(Variables.Orbwalker.GetTarget() is Obj_AI_Turret)
                && !(Variables.Orbwalker.GetTarget() is Obj_BarracksDampener))
            {
                return;
            }

            /// <summary>
            ///     The Q BuildingClear Logic.
            /// </summary>
            if (Vars.Q.IsReady()
                && GameObjects.Player.ManaPercent
                > ManaManager.GetNeededHealth(Vars.Q.Slot, Vars.Menu["spells"]["q"]["buildings"])
                && Vars.Menu["spells"]["q"]["buildings"].GetValue<MenuSliderButton>().BValue)
            {
                Vars.Q.Cast();
            }
        }

        /// <summary>
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Clear(EventArgs args)
        {
            if ((Bools.HasSheenBuff() && Targets.Target.IsValidTarget(GameObjects.Player.GetRealAutoAttackRange()))
                || !(Variables.Orbwalker.GetTarget() as Obj_AI_Minion).IsValidTarget())
            {
                return;
            }

            /// <summary>
            ///     The JungleClear Logic.
            /// </summary>
            if (Targets.JungleMinions.Any())
            {
                /// <summary>
                ///     The E JungleClear Logic.
                /// </summary>
                if (Vars.E.IsReady()
                    && GameObjects.Player.ManaPercent
                    >= ManaManager.GetNeededHealth(Vars.E.Slot, Vars.Menu["spells"]["e"]["jungleclear"])
                    && Vars.Menu["spells"]["e"]["jungleclear"].GetValue<MenuSliderButton>().BValue)
                {
                    var objAiMinion = Variables.Orbwalker.GetTarget() as Obj_AI_Minion;
                    if (objAiMinion != null && objAiMinion.IsValidTarget(Vars.R.Range)
                        && !objAiMinion.HasBuff("udyrbearstuncheck"))
                    {
                        Vars.E.Cast();
                    }
                }

                /// <summary>
                ///     The R JungleClear Logic.
                /// </summary>
                if (Vars.R.IsReady()
                    && GameObjects.Player.ManaPercent
                    >= ManaManager.GetNeededHealth(Vars.R.Slot, Vars.Menu["spells"]["r"]["jungleclear"])
                    && Vars.Menu["spells"]["r"]["jungleclear"].GetValue<MenuSliderButton>().BValue)
                {
                    Vars.R.Cast();
                }

                /// <summary>
                ///     The Q JungleClear Logic.
                /// </summary>
                else if (Vars.Q.IsReady()
                         && GameObjects.Player.ManaPercent
                         >= ManaManager.GetNeededHealth(Vars.Q.Slot, Vars.Menu["spells"]["q"]["jungleclear"])
                         && Vars.Menu["spells"]["q"]["jungleclear"].GetValue<MenuSliderButton>().BValue)
                {
                    Vars.Q.Cast();
                }
            }

            /// <summary>
            ///     The LaneClear R Logic.
            /// </summary>
            else if (Targets.Minions.Any() && Targets.Minions.Count >= 3)
            {
                if (Vars.R.IsReady() && GameObjects.Player.GetBuffCount("UdyrPhoenixStance") != 3
                    && GameObjects.Player.ManaPercent
                    >= ManaManager.GetNeededHealth(Vars.R.Slot, Vars.Menu["spells"]["r"]["laneclear"])
                    && Vars.Menu["spells"]["r"]["laneclear"].GetValue<MenuSliderButton>().BValue)
                {
                    Vars.R.Cast();
                }
            }
        }

        #endregion
    }
}