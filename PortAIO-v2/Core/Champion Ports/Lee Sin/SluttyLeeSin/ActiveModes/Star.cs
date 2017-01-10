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
    class Star : LeeSin
    {
        public static void StarCombo()
        {
            Orbwalking.MoveTo(Game.CursorPos);

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null)
                return;

            var slot = Items.GetWardSlot();
            var qpred = Q.GetPrediction(target);
            if (Q.IsReady() && Player.Spellbook.GetSpell(SpellSlot.Q).Name == "BlindMonkQOne")
            {
                Q.Cast(qpred.CastPosition);
            }

            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "blindmonkqtwo" && target.IsValidTarget(R.Range) && Q.IsReady())
            {
                R.Cast(target);
                Steps = LeeSin.steps.Flash;
            }

            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name.ToLower() == "blindmonkqtwo" && Q.IsReady() && !R.IsReady())
            {
                LeagueSharp.Common.Utility.DelayAction.Add(300, () => Q.Cast());
            }

            if (R.IsReady() && Q.IsReady() && W.IsReady() && slot != null)
            {
                if (target.Distance(Player) > R.Range && target.Distance(Player) < R.Range + 580)
                {
                    var pos = target.ServerPosition.Extend(Player.ServerPosition, 200);
                    if (!Processw && Player.GetSpell(SpellSlot.W).Name == "BlindMonkWOne")
                    {
                        Player.Spellbook.CastSpell(slot.SpellSlot, pos);
                        Lastwarr = Environment.TickCount;
                    }
                    if (Player.GetSpell(SpellSlot.W).Name == "blindmonkwtwo")
                    {
                        Lastwards = Environment.TickCount;
                        //   _lastflashward = Environment.TickCount;
                    }
                }
            }

            if (E.IsReady() && E.Instance.Name == "BlindMonkEOne" && target.IsValidTarget(E.Range))
            {
                E.Cast();
            }

            if (E.IsReady() && E.Instance.Name != "BlindMonkEOne" && !target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                E.Cast();
            }

        }
    }
}
