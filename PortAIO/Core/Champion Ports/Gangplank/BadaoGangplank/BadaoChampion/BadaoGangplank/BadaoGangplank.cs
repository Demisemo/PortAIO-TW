using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

using EloBuddy; 
using LeagueSharp.Common; 
namespace BadaoKingdom.BadaoChampion.BadaoGangplank
{
    using static BadaoMainVariables;
    using static BadaoGangplankVariables;
    public static class BadaoGangplank
    {
        public static void BadaoActivate()
        {
            BadaoGangplankConfig.BadaoActivate();
            BadaoGangplankBarrels.BadaoActivate();
            BadaoGangplankCombo.BadaoActivate();
            BadaoGangplankHarass.BadaoActivate();
            BadaoGangplankLaneClear.BadaoActivate();
            BadaoGangplankJungleClear.BadaoActivate();
            BadaoGangplankAuto.BadaoActivate();
            //Game.OnUpdate += Game_OnUpdate;
        }
        private static void Game_OnUpdate(EventArgs args)
        {

        }
    }
}
