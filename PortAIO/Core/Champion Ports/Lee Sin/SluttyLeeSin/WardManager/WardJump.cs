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
 namespace Lee_Sin.WardManager
{
    class WardJump : LeeSin
    {
        public static void WardJumped(Vector3 position, bool objectuse, bool use = true)
        {
            var objects =
                ObjectManager.Get<Obj_AI_Base>()
                    .FirstOrDefault(
                        x =>
                            x.IsValid && x.Distance(position) < 200 && x.IsAlly && !x.IsDead &&
                            !x.Name.ToLower().Contains("turret"));

            var ward = WardSorter.Wards();
            if (objectuse)
            {
                if (objects == null)
                {
                    if (W.IsReady() && ward != null && Environment.TickCount - Lastwcasted > 200 && W1() && !use)
                    {
                        Items.UseItem(ward.Id, position);
                    }
                    if (W.IsReady() && ward != null && W1() && use && Environment.TickCount - Lastcastedw > 200)
                    {
                      //  Player.Spellbook.CastSpell(ward., position);
                        Items.UseItem(ward.Id, position);
                    }
                }
            }
            else
            {
                if (W.IsReady() && ward != null && Environment.TickCount - Lastwcasted > 200 && W1() && !use)
                {
                    // Player.Spellbook.CastSpell(ward.SpellSlot, position);
                    Items.UseItem(ward.Id, position);

                }
                if (W.IsReady() && ward != null && W1() && use && Environment.TickCount - Lastcastedw > 200)
                {
                 //   Player.Spellbook.CastSpell(ward.SpellSlot, position);
                    Items.UseItem(ward.Id, position);

                }
            }

            if (!objectuse) return;
            foreach (
                var wards in
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(wards => W.IsReady() && Environment.TickCount - Wardlastcasted > 400 && W1() && !W2() && (objects != null)))
            {
                W.Cast(objects);
                LeeSin.Lastcastedw = Environment.TickCount;
            }
        }
    }
}