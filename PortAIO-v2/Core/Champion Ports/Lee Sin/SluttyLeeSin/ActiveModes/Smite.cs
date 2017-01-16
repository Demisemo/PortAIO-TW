using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Lee_Sin.ActiveModes
{
    class Smite : LeeSin
    {

        public static readonly string[] Names =
        {
            "SRU_Krug", "SRU_Razorbeak", "SRU_Murkwolf", "SRU_Gromp", "Sru_Crab", "SRU_Blue", "SRU_Red", "SRU_Dragon_Air", "SRU_Dragon_Water", "SRU_Dragon_Fire", "SRU_Dragon_Elder", "SRU_Dragon_Earth", "SRU_Baron", "SRU_RiftHerald"
        };

        public static void AutoSmite()
        {
            if (!GetBool("smiteonkillable", typeof(bool))) return;

            foreach (var mob in
                MinionManager.GetMinions(Player.Position, 550, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth))
            {
                foreach (var name in Names)
                {
                    if (mob.BaseSkinName.Equals(name) && GetBool("usesmiteon" + name, typeof(bool)))
                    {
                        if (!mob.IsValidTarget()) return;

                        if (SmiteDamage(mob) > mob.Health && Smite.IsReady())
                        {
                            Player.Spellbook.CastSpell(Smite, mob);
                        }

                        if (GetBool("qcalcsmite", typeof(bool)))
                        {
                            if ((SmiteDamages(mob) >= mob.Health && Q.IsReady()))
                            {
                                if (Q1())
                                Q.Cast(mob);

                                if (mob.HasBuff("BlindMonkQTwo"))
                                {
                                    Player.Spellbook.CastSpell(Smite, mob);
                                }
                                if (Q2())
                                Q.Cast();
                            }

                            if (GetFuckingSmiteDamage() + Q.GetDamage(mob) + (mob.MaxHealth - mob.Health) * 0.08 >= mob.Health && Q.IsReady() && Player.GetSpell(SpellSlot.Q).Name == "BlindMonkQTwo" && mob.HasBuff("BlindMonkQTwo"))
                            {
                                Q.Cast();
                                Player.Spellbook.CastSpell(Smite, mob);
                            }
                        }
                    }
                }
            }
        }

        public static float SmiteDamage(Obj_AI_Base target)
        {
            float damage = 0;

            if (Smite.IsReady())
            {
                if (target.IsValidTarget(500))
                    damage += GetFuckingSmiteDamage();
            }

            return damage;
        }


        public static float SmiteDamages(Obj_AI_Base target)
        {
            float damage = 0;

            if (Smite.IsReady())
            {
                if (target.IsValidTarget(500))
                    damage += GetFuckingSmiteDamage();
            }

            if (Q.IsReady() && target.IsValidTarget(Q.Range) && GetBool("qcalcsmite", typeof(bool)))
            {
                damage += GetQDamage(target);
            }

            return damage;
        }


        public static float GetFuckingSmiteDamage()
        {
            var level = Player.Level;
            var index = Player.Level / 5;
            float[] dmgs =
            {
                370 + 20*level, 330 + 30*level, 240 + 40*level, 100 + 50*level
            };
            return dmgs[index];
        }
    }
}
