using EloBuddy;
using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLib1
{
    class SpellManager
    {
        private Dictionary<string, Spell> Spells = new Dictionary<string, Spell>();

        public SpellManager()
        {

        }

        public void Add(string name, Spell spell)
        {
            Spells.Add(name, spell);
        }

        public Spell get(string name)
        {
            return Spells[name];
        }

        public void CastSkillshot(string spell, Obj_AI_Base target, HitChance hitChance = HitChance.VeryHigh, bool packet = true, bool aoe = false)
        {
            if (!Spells[spell].IsReady()) return;

            if (target.IsValidTarget(Spells[spell].Range) && Spells[spell].GetPrediction(target).Hitchance >= hitChance)
                Spells[spell].Cast(target, packet, aoe);
        }
        public void CastSkillshot(string spell, TargetSelector.DamageType damageType, HitChance hitChance = HitChance.VeryHigh, bool packet = true, bool aoe = false)
        {
            if (!Spells[spell].IsReady()) return;

            AIHeroClient target = TargetSelector.GetTarget(Spells[spell].Range, damageType);
            if (target == null) return;

            if (target.IsValidTarget(Spells[spell].Range) && Spells[spell].GetPrediction(target).Hitchance >= hitChance)
                Spells[spell].Cast(target, packet, aoe);
        }
        public void CastOnTarget(string spell, Obj_AI_Base target, bool packet = true)
        {
            if (!Spells[spell].IsReady()) return;

            if (target.IsValidTarget(Spells[spell].Range))
                Spells[spell].CastOnUnit(target, packet);
        }
        public void CastOnTarget(string spell, TargetSelector.DamageType damageType, bool packet = true)
        {
            if (!Spells[spell].IsReady()) return;

            AIHeroClient target = TargetSelector.GetTarget(Spells[spell].Range, damageType);
            if (target == null) return;

            if (target.IsValidTarget(Spells[spell].Range))
                Spells[spell].CastOnUnit(target, packet);
        }
    }
}