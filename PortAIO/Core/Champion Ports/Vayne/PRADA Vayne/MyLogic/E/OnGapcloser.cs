using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using PRADA_Vayne_Old.MyUtils;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace PRADA_Vayne_Old.MyLogic.E
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
                    Program.E.Cast(gapcloser.Sender);
                }
            }
        }
        public static void OnGapcloserVHRPlugin(ActiveGapcloser gapcloser)
        {
            if (PRADAHijacker.HijackedMenu.SubMenu("antigapcloser")
                .Item("antigc" + gapcloser.Sender.ChampionName)
                .GetValue<bool>())
            {
                if (Heroes.Player.Distance(gapcloser.End) < 425)
                {
                    Program.E.Cast(gapcloser.Sender);
                }
            }
        }
    }
}
