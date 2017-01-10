using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LCS_Lucian;
using LeagueSharp;
using LeagueSharp.Common;

using EloBuddy; 
using LeagueSharp.Common; 
 namespace LCS_Lucian
{

    class LucianDrawing
    {
        public static void Init()
        {

            if (ObjectManager.Player.IsDead)
            {
                return;
            }
            if (LucianMenu.Config.Item("lucian.q.draw").GetValue<Circle>().Active && LucianSpells.Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.Q.Range, LucianMenu.Config.Item("lucian.q.draw").GetValue<Circle>().Color);
            }
            if (LucianMenu.Config.Item("lucian.q2.draw").GetValue<Circle>().Active && LucianSpells.Q2.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.Q2.Range, LucianMenu.Config.Item("lucian.q2.draw").GetValue<Circle>().Color);
            }
            if (LucianMenu.Config.Item("lucian.w.draw").GetValue<Circle>().Active && LucianSpells.W.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.W.Range, LucianMenu.Config.Item("lucian.w.draw").GetValue<Circle>().Color);
            }
            if (LucianMenu.Config.Item("lucian.e.draw").GetValue<Circle>().Active && LucianSpells.E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.E.Range, LucianMenu.Config.Item("lucian.e.draw").GetValue<Circle>().Color);
            }
            if (LucianMenu.Config.Item("lucian.r.draw").GetValue<Circle>().Active && LucianSpells.R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.R.Range, LucianMenu.Config.Item("lucian.r.draw").GetValue<Circle>().Color);
            }
        }
    }
}