
#pragma warning disable 1587

using EloBuddy; 
using LeagueSharp.SDK; 
namespace ExorAIO.Champions.Darius
{
    using ExorAIO.Utilities;

    using LeagueSharp.SDK.UI;

    /// <summary>
    ///     The menu class.
    /// </summary>
    internal class Menus
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Sets the menu.
        /// </summary>
        public static void Initialize()
        {
            /// <summary>
            ///     Sets the spells menu.
            /// </summary>
            Vars.SpellsMenu = new Menu("spells", "Spells");
            {
                /// <summary>
                ///     Sets the menu for the Q.
                /// </summary>
                Vars.QMenu = new Menu("q", "Use Q to:");
                {
                    Vars.QMenu.Add(
                        new MenuList<string>(
                            "mode",
                            "Use Q in Combo:",
                            new[] { "Always", "Only in Blade Range", "Never" }));
                    Vars.QMenu.Add(new MenuSliderButton("aoe", "AoE / If can hit >= x enemies", 3, 3, 5, true));
                    Vars.QMenu.Add(new MenuSliderButton("harass", "Harass / If Mana >= x%", 50, 0, 99, true));
                    Vars.QMenu.Add(new MenuSliderButton("laneclear", "LaneClear / If Mana >= x%", 50, 0, 99, true));
                    Vars.QMenu.Add(new MenuSliderButton("jungleclear", "JungleClear / If Mana >= x%", 50, 0, 99, true));
                }
                Vars.SpellsMenu.Add(Vars.QMenu);

                /// <summary>
                ///     Sets the menu for the W.
                /// </summary>
                Vars.WMenu = new Menu("w", "Use W to:");
                {
                    Vars.WMenu.Add(new MenuBool("combo", "Combo", true));
                    Vars.WMenu.Add(new MenuSliderButton("buildings", "Buildings / If Mana >= x%", 50, 0, 99, true));
                    Vars.WMenu.Add(new MenuSliderButton("jungleclear", "JungleClear / If Mana >= x%", 50, 0, 99, true));
                }
                Vars.SpellsMenu.Add(Vars.WMenu);

                /// <summary>
                ///     Sets the menu for the E.
                /// </summary>
                Vars.EMenu = new Menu("e", "Use E to:");
                {
                    Vars.EMenu.Add(new MenuBool("combo", "Combo", true));
                    Vars.EMenu.Add(new MenuBool("gapcloser", "Anti-Gapcloser", true));
                    Vars.EMenu.Add(new MenuBool("interrupter", "Interrupt Enemy Channels", true));
                }
                Vars.SpellsMenu.Add(Vars.EMenu);

                /// <summary>
                ///     Sets the drawing menu for the R.
                /// </summary>
                Vars.RMenu = new Menu("r", "Use R to:");
                {
                    Vars.RMenu.Add(new MenuBool("killsteal", "KillSteal", true));
                }
                Vars.SpellsMenu.Add(Vars.RMenu);
            }
            Vars.Menu.Add(Vars.SpellsMenu);

            /// <summary>
            ///     Sets the drawings menu.
            /// </summary>
            Vars.DrawingsMenu = new Menu("drawings", "Drawings");
            {
                Vars.DrawingsMenu.Add(new MenuBool("q", "Q Range"));
                Vars.DrawingsMenu.Add(new MenuBool("w", "W Range"));
                Vars.DrawingsMenu.Add(new MenuBool("e", "E Range"));
                Vars.DrawingsMenu.Add(new MenuBool("r", "R Range"));
            }
            Vars.Menu.Add(Vars.DrawingsMenu);
        }

        #endregion
    }
}