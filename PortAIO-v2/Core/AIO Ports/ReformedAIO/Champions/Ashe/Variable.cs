using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Ashe
{
    #region Using Directives

    using System.Collections.Generic;

    using LeagueSharp;
    using LeagueSharp.Common;

    #endregion

    internal class Variable
    {
        #region Static Fields

        public static Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell> {
                                                                    {
                                                                        SpellSlot.Q,
                                                                        new Spell(
                                                                        SpellSlot.Q,
                                                                        600f)
                                                                    },
                                                                    {
                                                                        SpellSlot.W,
                                                                        new Spell(
                                                                        SpellSlot.W,
                                                                        1200f)
                                                                    },
                                                                    {
                                                                        SpellSlot.E,
                                                                        new Spell(
                                                                        SpellSlot.E,
                                                                        2000f)
                                                                    },
                                                                    {
                                                                        SpellSlot.R,
                                                                        new Spell(
                                                                        SpellSlot.R,
                                                                        2000f)
                                                                    }
                                                                };

        #endregion

        #region Public Properties

        //public static Orbwalking.Orbwalker Orbwalker { get; internal set; }

        public static AIHeroClient Player => ObjectManager.Player;

        #endregion
    }
}