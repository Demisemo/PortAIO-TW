using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using PRADA_Vayne_Old.MyUtils;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace PRADA_Vayne_Old.MyLogic.Q
{
    public static partial class Events
    {
        public static void OnGapcloser(ActiveGapcloser gapcloser)
        {
            if (Program.EscapeMenu.SubMenu("antigapcloser")
                .Item("antigc" + gapcloser.Sender.ChampionName)
                .GetValue<bool>())
            {
                if (Heroes.Player.Distance(gapcloser.End) < 425)
                {
                    Tumble.Cast(Heroes.Player.Position.Extend(gapcloser.End, -300));
                }
            }
        }
    }
}
