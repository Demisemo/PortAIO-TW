using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Vayne_Old.MyUtils;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace PRADA_Vayne_Old.MyLogic.E
{
    public static class AntiAssasins
    {
        public static void OnCreateGameObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.ToLower().Contains("leapsound.troy"))
            {
                var rengo = Heroes.EnemyHeroes.FirstOrDefault(h => h.CharData.BaseSkinName == "Rengar");
                if (rengo.IsValidTarget(545) && Program.E.IsReady())
                {
                    Program.E.Cast(rengo);
                }
            }
        }
    }
}
