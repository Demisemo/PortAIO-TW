using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace OKTWPredictioner
{
    public class OKTWPredictioner
    {
        public static Spell[] Spells = { null, null, null, null };
        public static Menu Config;

        public static void Initialize()
        {
            #region Initialize Menu
            Config = new Menu("OKTW Predictioner", "oktwpredictioner", true);
            Config.AddItem(new MenuItem("COMBOKEY", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.AddItem(new MenuItem("HARASSKEY", "Harass").SetValue(new KeyBind('C', KeyBindType.Press)));
            Config.AddItem(new MenuItem("ENABLED", "Enabled").SetValue(true));

            #region Initialize Spells
            Menu skillshots = new Menu("Skillshots", "spredskillshots");
            foreach (var spell in SpellDatabase.Spells)
            {
                if (spell.ChampionName == ObjectManager.Player.CharData.BaseSkinName)
                {
                    Spells[(int)spell.Slot] = new Spell(spell.Slot, spell.Range);
                    Spells[(int)spell.Slot].SetSkillshot(spell.Delay / 1000f, spell.Radius, spell.MissileSpeed, spell.Collisionable, spell.Type);
                    skillshots.AddItem(new MenuItem(String.Format("{0}{1}", spell.ChampionName, spell.Slot), "Convert Spell " + spell.Slot.ToString()).SetValue(true));
                }
            }
            Config.AddSubMenu(skillshots);
            #endregion


            Config.SubMenu("SPREDFORSPREDICTONER").AddItem(new MenuItem("SPREDHITC", "Hit Chance").SetValue(new StringList(ShineCommon.Utility.HitchanceNameArray, 2))).SetTooltip("High is recommended");
            Config.AddToMainMenu();
            #endregion

            #region Initialize Events
            Spellbook.OnCastSpell += EventHandlers.Spellbook_OnCastSpell;
            Obj_AI_Base.OnSpellCast += EventHandlers.AIHeroClient_OnProcessSpellCast;
            #endregion
        }
    }
}
