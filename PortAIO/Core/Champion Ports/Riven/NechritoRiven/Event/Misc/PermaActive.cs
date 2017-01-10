using EloBuddy; 
using LeagueSharp.Common; 
namespace NechritoRiven.Event.Misc
{
    #region

    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using NechritoRiven.Core;
    using NechritoRiven.Event.OrbwalkingModes;

    using Orbwalking = Orbwalking;

    #endregion

    internal class PermaActive : Core
    {
        #region Public Methods and Operators

        private static void QMove()
        {
            if (!MenuConfig.QMove || !Spells.Q.IsReady())
            {
                return;
            }

            LeagueSharp.Common.Utility.DelayAction.Add(Game.Ping + 2, () => Spells.Q.Cast(Player.Position - 15));
        }

        public static void Update(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Utils.GameTimeTickCount - LastQ >= 3650 - Game.Ping
                && MenuConfig.KeepQ
                && !Player.InFountain()
                && !Player.HasBuff("Recall")
                && Player.HasBuff("RivenTriCleave"))
            {
                Spells.Q.Cast(Game.CursorPos);
            }

            QMove();

            BackgroundData.ForceSkill();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combos.Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Burst:
                    Combos.Burst();
                    break;
                case Orbwalking.OrbwalkingMode.Flee:
                    FleeMode.Flee();
                    break;
                case Orbwalking.OrbwalkingMode.QuickHarass:
                    FastHarassMode.FastHarass();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Mixed.Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    JungleClearMode.Jungleclear();
                    LaneclearMode.Laneclear();
                    break;
            }
        }
        #endregion
    }
}