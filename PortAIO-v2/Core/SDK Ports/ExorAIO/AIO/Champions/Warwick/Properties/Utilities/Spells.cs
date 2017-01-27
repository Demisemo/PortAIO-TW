using EloBuddy; 
using LeagueSharp.SDK; 
namespace ExorAIO.Champions.Warwick
{
    using ExorAIO.Utilities;

    using LeagueSharp;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.Enumerations;

    /// <summary>
    ///     The spells class.
    /// </summary>
    internal class Spells
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Sets the spells.
        /// </summary>
        public static void Initialize()
        {
            Vars.Q = new Spell(SpellSlot.Q, 350);
            Vars.W = new Spell(SpellSlot.W, 4000);
            Vars.E = new Spell(SpellSlot.E, 375);
            Vars.R = new Spell(SpellSlot.R, 335);
            Vars.R.SetSkillshot(0.25f, 90f, 2200f, false, SkillshotType.SkillshotLine);
        }

        #endregion
    }
}