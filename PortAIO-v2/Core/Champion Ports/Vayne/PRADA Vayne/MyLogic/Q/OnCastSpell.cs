using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using PRADA_Vayne_Old.MyUtils;
using SharpDX;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace PRADA_Vayne_Old.MyLogic.Q
{
    public static partial class Events
    {
        public static void OnCastSpell(Spellbook spellbook, SpellbookCastSpellEventArgs args)
        {
            if (spellbook.Owner.IsMe)
            {
                if (args.Slot == SpellSlot.Q)
                {
                    if (Tumble.TumbleOrderPos != Vector3.Zero)
                    {
                        if (Tumble.TumbleOrderPos.IsDangerousPosition())
                        {
                            Tumble.TumbleOrderPos = Vector3.Zero;
                            args.Process = false;
                        }
                        else
                        {
                            Tumble.TumbleOrderPos = Vector3.Zero;
                        }
                    }
                }
            }
        }
    }
}
