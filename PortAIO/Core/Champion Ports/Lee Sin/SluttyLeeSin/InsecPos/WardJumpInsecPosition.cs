using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Lee_Sin.InsecPos
{
    class WardJumpInsecPosition : LeeSin
    {
        public static IEnumerable<AIHeroClient> GetAllyHeroes(AIHeroClient unit, int range)
        {
            return
                ObjectManager.Get<AIHeroClient>()
                    .Where(hero => hero.IsAlly && !hero.IsMe && !hero.IsDead && hero.Distance(unit) < range).OrderBy(x => x.Distance(Player))
                    .ToList();
        }

        public static Vector2 InsecPos(AIHeroClient target, int extendvalue, bool flashcasting)
        {


            if (SelectedAllyAiMinion != null)
            {
                return
                    SelectedAllyAiMinion.ServerPosition.Extend(target.ServerPosition,
                        SelectedAllyAiMinion.Distance(target) + extendvalue).To2D();

            }
            else
            {
                var objAiHero = GetAllyHeroes(target, 2300).FirstOrDefault();
                if (GetBool("useobjectsallies", typeof(bool)) && objAiHero != null)
                {
                    return
                        objAiHero.ServerPosition.Extend(target.ServerPosition,
                            objAiHero.Distance(target) + extendvalue).To2D();
                }

                if (!GetBool("useobjectsallies", typeof(bool)) || objAiHero == null)
                {
                    return Player.ServerPosition.Extend(target.ServerPosition,
                        Player.Distance(target) + extendvalue).To2D();
                }
            }

            return new Vector2();
        }
    }
}
