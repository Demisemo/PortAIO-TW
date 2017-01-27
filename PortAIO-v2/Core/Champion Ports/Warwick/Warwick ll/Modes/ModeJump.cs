using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WarwickII.Common;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = SharpDX.Color;

using EloBuddy;
using LeagueSharp.Common;
namespace WarwickII.Modes
{
    internal class DangerousSpells
    {
        public string SpellName { get; private set; }
        public string ChampionName { get; private set; }
        public SpellSlot SpellSlot { get; private set; }
        public SkillType Type { get; private set; }

        public enum SkillType
        {
            Target,
            Zone
        }

        public DangerousSpells(string spellName, string championName, SpellSlot spellSlot, SkillType type)
        {
            SpellName = spellName;
            ChampionName = championName;
            SpellSlot = spellSlot;
            Type = type;
        }
    }
}
