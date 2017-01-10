using LeagueSharp;
using LeagueSharp.Common;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace SyndraL33T
{
    public static class Utils
    {
        public static double GetComboDamage(this AIHeroClient target, bool useQ, bool useW, bool useE, bool useR)
        {
            if (!target.IsValidTarget())
            {
                return 0d;
            }

            var damage = 0d;
            var comboMana = 0d;
            var mana = EntryPoint.Player.Mana;
            var nonIgnoredR = !EntryPoint.Menu.Item("l33t.stds.ups.ignore." + target.ChampionName).GetValue<bool>();

            if (Mechanics.Spells[SpellSlot.R].IsReady() && useR && nonIgnoredR)
            {
                var manaCost = Mechanics.Spells[SpellSlot.R].Instance.Instance.SData.Mana;
                if (comboMana + manaCost <= mana)
                {
                    comboMana += manaCost;
                    damage += Mechanics.Spells[SpellSlot.R].Damage(target);
                }
            }

            if (Mechanics.Spells[SpellSlot.Q].IsReady() && useQ)
            {
                var manaCost = Mechanics.Spells[SpellSlot.Q].Instance.Instance.SData.Mana;
                if (comboMana + manaCost <= mana)
                {
                    comboMana += manaCost;
                    damage += Mechanics.Spells[SpellSlot.Q].Damage(target);
                }
            }
            if (Mechanics.Spells[SpellSlot.W].IsReady() && useQ)
            {
                var manaCost = Mechanics.Spells[SpellSlot.W].Instance.Instance.SData.Mana;
                if (comboMana + manaCost <= mana)
                {
                    comboMana += manaCost;
                    damage += Mechanics.Spells[SpellSlot.W].Damage(target);
                }
            }
            if (Mechanics.Spells[SpellSlot.E].IsReady() && useQ)
            {
                var manaCost = Mechanics.Spells[SpellSlot.E].Instance.Instance.SData.Mana;
                if (comboMana + manaCost <= mana)
                {
                    damage += Mechanics.Spells[SpellSlot.E].Damage(target);
                }
            }

            return damage;
        }

        public static bool BuffCheck(this Obj_AI_Base enemy)
        {
            return enemy.HasBuff("UndyingRage") &&
                   EntryPoint.Menu.Item("l33t.stds.ups.disablebuff.undying").GetValue<bool>() ||
                   enemy.HasBuff("JudicatorIntervention") &&
                   EntryPoint.Menu.Item("l33t.stds.ups.disablebuff.judicator").GetValue<bool>() ||
                   enemy.HasBuff("Ferocious Howl") &&
                   EntryPoint.Menu.Item("l33t.stds.ups.disablebuff.alistar").GetValue<bool>() ||
                   enemy.HasBuff("Chrono Shift") &&
                   EntryPoint.Menu.Item("l33t.stds.ups.disablebuff.zilean").GetValue<bool>() ||
                   enemy.HasBuff("ZacRebirthReady") &&
                   EntryPoint.Menu.Item("l33t.stds.ups.disablebuff.zac").GetValue<bool>() ||
                   enemy.HasBuff("AttroxPassiveReady") &&
                   EntryPoint.Menu.Item("l33t.stds.ups.disablebuff.aatrox").GetValue<bool>() ||
                   enemy.HasBuff("Spell Shield") &&
                   EntryPoint.Menu.Item("l33t.stds.ups.disablebuff.sivir").GetValue<bool>() ||
                   enemy.HasBuff("Black Shield") &&
                   EntryPoint.Menu.Item("l33t.stds.ups.disablebuff.morgana").GetValue<bool>();
        }
    }
}