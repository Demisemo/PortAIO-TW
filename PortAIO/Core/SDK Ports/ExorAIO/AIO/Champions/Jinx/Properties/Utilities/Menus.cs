
#pragma warning disable 1587

using EloBuddy; 
using LeagueSharp.SDK; 
namespace ExorAIO.Champions.Jinx
{
    using System.Windows.Forms;

    using ExorAIO.Utilities;

    using LeagueSharp.SDK;
    using LeagueSharp.SDK.Enumerations;
    using LeagueSharp.SDK.UI;

    using Menu = LeagueSharp.SDK.UI.Menu;

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
                        new MenuSeparator(
                            "separator",
                            "The Combo slider only sets the necessary number of hittable enemies"));
                    Vars.QMenu.Add(new MenuSeparator("separator1", "Distance Q will be automatically used."));
                    Vars.QMenu.Add(new MenuSliderButton("combo", "Combo / if Hittable Enemies >= x", 3, 2, 5, true));
                    Vars.QMenu.Add(new MenuSeparator("separator2", " "));
                    Vars.QMenu.Add(new MenuSliderButton("clear", "Clear / if Mana >= x%", 50, 0, 99, true));
                    Vars.QMenu.Add(new MenuSliderButton("harass", "Harass / if Mana >= x%", 50, 0, 99, true));
                    Vars.QMenu.Add(new MenuSliderButton("lasthit", "LastHit / if Mana >= x%", 50, 0, 99, true));
                }
                Vars.SpellsMenu.Add(Vars.QMenu);

                /// <summary>
                ///     Sets the menu for the W.
                /// </summary>
                Vars.WMenu = new Menu("w", "Use W to:");
                {
                    Vars.WMenu.Add(new MenuBool("combo", "Combo", true));
                    Vars.WMenu.Add(new MenuBool("killsteal", "KillSteal", true));
                    Vars.WMenu.Add(new MenuBool("logical", "Logical", true));
                    Vars.WMenu.Add(new MenuSliderButton("harass", "Harass / if Mana >= x%", 50, 0, 99, true));
                }
                Vars.SpellsMenu.Add(Vars.WMenu);

                /// <summary>
                ///     Sets the menu for the E.
                /// </summary>
                Vars.EMenu = new Menu("e", "Use E to:");
                {
                    Vars.EMenu.Add(new MenuBool("gapcloser", "Anti-Gapcloser", true));
                    Vars.EMenu.Add(new MenuBool("logical", "Logical", true));
                    Vars.EMenu.Add(new MenuSliderButton("aoe", "AoE / If can hit >= x enemies", 2, 1, 5, true));
                }
                Vars.SpellsMenu.Add(Vars.EMenu);

                /// <summary>
                ///     Sets the menu for the R.
                /// </summary>
                Vars.RMenu = new Menu("r", "Use R to:");
                {
                    Vars.RMenu.Add(new MenuBool("killsteal", "KillSteal", true));
                    Vars.RMenu.Add(
                        new MenuSeparator(
                            "separator",
                            "The Semi-Automatic R will automatically ult the lowest on health non-invulnerable enemy in range."));
                    Vars.RMenu.Add(new MenuBool("bool", "Semi-Automatic R", true));
                    Vars.RMenu.Add(new MenuKeyBind("key", "Key:", Keys.T, KeyBindType.Press));
                    {
                        /// <summary>
                        ///     Sets the menu for the R Whitelist.
                        /// </summary>
                        Vars.WhiteListMenu = new Menu("whitelist", "Ultimate: Whitelist Menu");
                        {
                            foreach (var target in GameObjects.EnemyHeroes)
                            {
                                Vars.WhiteListMenu.Add(
                                    new MenuBool(
                                        target.ChampionName.ToLower(),
                                        $"Use against: {target.ChampionName}",
                                        true));
                            }
                        }

                        Vars.RMenu.Add(Vars.WhiteListMenu);
                    }
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
            }
            Vars.Menu.Add(Vars.DrawingsMenu);
        }

        #endregion
    }
}