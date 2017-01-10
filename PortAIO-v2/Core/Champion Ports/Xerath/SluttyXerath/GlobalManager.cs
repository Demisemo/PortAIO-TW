using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using EloBuddy; 
 using LeagueSharp.Common; 
 namespace The_Slutty_Xerath
{
    class GlobalManager : Xerath
    {
        public static int RRange
        {
            get
            {
                if (Player.GetSpell(SpellSlot.R).Level == 1)
                    return 3200;

                if (Player.GetSpell(SpellSlot.R).Level == 2)
                    return 4400;

                if (Player.GetSpell(SpellSlot.R).Level == 3)
                    return 5600;

                return 0;
            }

        }


        public static bool RCasted()
        {
            return Environment.TickCount - lastrr < 250 || ObjectManager.Player.HasBuff("XerathLocusOfPower2");
        }

        public static float GetComboDamage(AIHeroClient enemy)
        {
            var damage = 0d;
            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R)*3;

            if (Ignite.IsReady())
                damage += IgniteDamage(enemy);


            return (float)damage;
        }

        public static int RCount
        {
            get
            {
                var rcount = Player.Buffs.FirstOrDefault(b => b.DisplayName == "rcountchecklater");
                return rcount != null ? rcount.Count : 0;
            }
        }

        private static float IgniteDamage(AIHeroClient target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }


        private static DamageToUnitDelegate _damageToUnit;
        public static bool EnableDrawingDamage { get; set; }
        public static Color DamageFillColor { get; set; }

        public delegate float DamageToUnitDelegate(AIHeroClient hero);



        public static DamageToUnitDelegate DamageToUnit
        {
            get { return _damageToUnit; }

            set
            {
                if (_damageToUnit == null)
                {
                    Drawing.OnDraw += Drawing_OnDrawChamp;
                }
                _damageToUnit = value;
            }
        }
    }
}
