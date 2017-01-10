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
 namespace ezEvade.SpecialSpells
{
    class Sion : ChampionPlugin
    {
        static Sion()
        {
            // todo: fix for multiple sions on same team (e.g one for all)
        }

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "SionR")
            {
                var hero = HeroManager.AllHeroes.FirstOrDefault(x => x.ChampionName == "Sion");
                if (hero != null && hero.CheckTeam())
                {
                    Game.OnUpdate += (args) => Game_OnUpdate(args, hero);
                    SpellDetector.OnProcessSpecialSpell += SpellDetector_OnProcessSpecialSpell;
                }
            }
        }

        private void SpellDetector_OnProcessSpecialSpell(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args, SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "SionR")
            {
                spellData.projectileSpeed = hero.MoveSpeed;
                specialSpellArgs.spellData = spellData;
            }
        }

        private void Game_OnUpdate(EventArgs args, AIHeroClient hero)
        {
            foreach (var spell in SpellDetector.detectedSpells.Where(x => x.Value.heroID == hero.NetworkId && x.Value.info.spellName == "SionR"))
            {
                var facingPos = hero.ServerPosition.To2D() + hero.Direction.To2D().Perpendicular();
                var endPos = hero.ServerPosition.To2D() + (facingPos - hero.ServerPosition.To2D()).Normalized() * 450;

                spell.Value.startPos = hero.ServerPosition.To2D();
                spell.Value.endPos = endPos;

                if (EvadeUtils.TickCount - spell.Value.startTime >= 1000)
                {
                    SpellDetector.CreateSpellData(hero, hero.ServerPosition, endPos.To3D(), spell.Value.info, null, 0, false, SpellType.Line, false);
                    spell.Value.startTime = EvadeUtils.TickCount;
                    break;
                }
            }        
        }
    }
}
