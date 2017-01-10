using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;

using EloBuddy;
using LeagueSharp.Common;
namespace SPredictioner
{
    public class EventHandlers
    {
        private static bool[] handleEvent = { true, true, true, true };

        public static void Game_OnGameLoad()
        {
            SPredictioner.Initialize();
        }

        public static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                SpellSlot slot = ObjectManager.Player.GetSpellSlot(args.SData.Name);
                if (!Utility.IsValidSlot(slot))
                    return;

                if (!handleEvent[(int)slot])
                {
                    if (SPredictioner.Spells[(int)slot] != null)
                        handleEvent[(int)slot] = true;
                }
            }
        }

        public static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                if (SPredictioner.Config.Item("ENABLED").GetValue<bool>() && (SPredictioner.Config.Item("COMBOKEY").GetValue<KeyBind>().Active || SPredictioner.Config.Item("HARASSKEY").GetValue<KeyBind>().Active))
                {
                    if (!Utility.IsValidSlot(args.Slot))
                        return;

                    if (SPredictioner.Spells[(int)args.Slot] == null)
                        return;

                    if (!SPredictioner.Config.Item(String.Format("{0}{1}", ObjectManager.Player.ChampionName, args.Slot)).GetValue<bool>())
                        return;

                    if (handleEvent[(int)args.Slot])
                    {
                        args.Process = false;
                        var enemy = TargetSelector.GetTarget(SPredictioner.Spells[(int)args.Slot].Range, TargetSelector.DamageType.Physical);


                        if (enemy != null)
                        {
                            if (ObjectManager.Player.ChampionName == "Viktor" && args.Slot == SpellSlot.E)
                            {
                                handleEvent[(int)args.Slot] = false;
                                SPredictioner.Spells[(int)args.Slot].SPredictionCastVector(enemy, 500, ShineCommon.Utility.HitchanceArray[SPredictioner.Config.Item("SPREDHITC").GetValue<StringList>().SelectedIndex]);
                            }
                            else if (ObjectManager.Player.ChampionName == "Diana" && args.Slot == SpellSlot.Q)
                            {
                                handleEvent[(int)args.Slot] = false;
                                SPredictioner.Spells[(int)args.Slot].SPredictionCastArc(enemy, ShineCommon.Utility.HitchanceArray[SPredictioner.Config.Item("SPREDHITC").GetValue<StringList>().SelectedIndex]);
                            }
                            else if (ObjectManager.Player.ChampionName == "Veigar" && args.Slot == SpellSlot.E)
                            {
                                handleEvent[(int)args.Slot] = false;
                                SPredictioner.Spells[(int)args.Slot].SPredictionCastRing(enemy, 80, ShineCommon.Utility.HitchanceArray[SPredictioner.Config.Item("SPREDHITC").GetValue<StringList>().SelectedIndex]);
                            }
                            else
                            {
                                SPredictioner.Spells[(int)args.Slot].SPredictionCast(enemy, ShineCommon.Utility.HitchanceArray[SPredictioner.Config.Item("SPREDHITC").GetValue<StringList>().SelectedIndex]);
                                handleEvent[(int)args.Slot] = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
