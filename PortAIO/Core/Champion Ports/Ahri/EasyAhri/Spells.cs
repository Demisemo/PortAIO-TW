﻿using System;
using LeagueSharp;
using LeagueSharp.Common;
using EloBuddy;

namespace EasyLib2
{
    public static class Spells
    {
        public static bool PacketCast;

        public static void CastSkillshot(Spell spell, TargetSelector.DamageType damageType, HitChance hitChance = HitChance.VeryHigh, bool aoe = false)
        {
            if (!spell.IsReady()) return;

            AIHeroClient target = TargetSelector.GetTarget(spell.Range, damageType);
            if (target == null) return;

            if (target.IsValidTarget(spell.Range) && spell.GetPrediction(target).Hitchance >= hitChance)
                spell.Cast(target, PacketCast, aoe);
        }

        public static void CastSkillshot(Spell spell, Obj_AI_Base target, HitChance hitChance = HitChance.VeryHigh, bool aoe = false)
        {
            if (!spell.IsReady()) return;

            if (target.IsValidTarget(spell.Range) && spell.GetPrediction(target).Hitchance >= hitChance)
                spell.Cast(target, PacketCast, aoe);
        }

        public static void CastTargeted(Spell spell, TargetSelector.DamageType damageType)
        {
            if (!spell.IsReady()) return;

            AIHeroClient target = TargetSelector.GetTarget(spell.Range, damageType);
            if (target == null) return;

            if (target.IsValidTarget(spell.Range))
                spell.CastOnUnit(target, PacketCast);
        }

        public static void CastSelf(Spell spell, TargetSelector.DamageType damageType)
        {
            if (!spell.IsReady()) return;

            AIHeroClient target = TargetSelector.GetTarget(spell.Range, damageType);
            if (target == null) return;

            if (target.IsValidTarget(spell.Range))
                spell.Cast(PacketCast);
        }

        public static void Cast(Spell spell)
        {
            if (!spell.IsReady()) return;

            spell.Cast(PacketCast);
        }
    }
}