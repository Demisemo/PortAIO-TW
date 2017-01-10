using EloBuddy; 
using LeagueSharp.Common; 
namespace ElUtilitySuite.Summoners
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using ElUtilitySuite.Vendor.SFX;

    using LeagueSharp;
    using LeagueSharp.Common;

    public class Heal : IPlugin
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the heal spell.
        /// </summary>
        /// <value>
        ///     The heal spell.
        /// </value>
        public Spell HealSpell { get; set; }

        /// <summary>
        /// The Menu
        /// </summary>
        public Menu Menu { get; set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        private AIHeroClient Player => ObjectManager.Player;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        /// <param name="rootMenu">The root menu.</param>
        /// <returns></returns>
        public void CreateMenu(Menu rootMenu)
        {
            if (this.Player.GetSpellSlot("summonerheal") == SpellSlot.Unknown)
            {
                return;
            }

            var predicate = new Func<Menu, bool>(x => x.Name == "SummonersMenu");
            var menu = !rootMenu.Children.Any(predicate)
                           ? rootMenu.AddSubMenu(new Menu("Summoners", "SummonersMenu"))
                           : rootMenu.Children.First(predicate);

            var healMenu = menu.AddSubMenu(new Menu("治癒", "Heal"));
            {
                healMenu.AddItem(new MenuItem("Heal.Activated", "使用治癒").SetValue(true));
                healMenu.AddItem(new MenuItem("PauseHealHotkey", "不使用治癒熱鍵").SetValue(new KeyBind('L', KeyBindType.Press)));
                healMenu.AddItem(new MenuItem("min-health", "血量低於%使用").SetValue(new Slider(20, 1)));
                healMenu.AddItem(new MenuItem("min-damage", "受到傷害%使用").SetValue(new Slider(20, 1)));
                healMenu.AddItem(new MenuItem("seperator21", ""));
                foreach (var x in HeroManager.Allies)
                {
                    healMenu.AddItem(new MenuItem($"healon{x.ChampionName}", "使用於: " + x.ChampionName))
                        .SetValue(true);
                }
            }

            this.Menu = healMenu;
        }

        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public void Load()
        {
            try
            {
                var healSlot = this.Player.GetSpellSlot("summonerheal");

                if (healSlot == SpellSlot.Unknown)
                {
                    return;
                }

                IncomingDamageManager.RemoveDelay = 500;
                IncomingDamageManager.Skillshots = true;
                this.HealSpell = new Spell(healSlot, 850);
                Game.OnUpdate += this.OnUpdate;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error occurred: '{0}'", e);
            }
        }

        /// <summary>
        ///     Fired when the game is updated.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void OnUpdate(EventArgs args)
        {
            try
            {
                if (this.Player.IsDead || !this.HealSpell.IsReady() || this.Player.InFountain() || this.Player.IsRecalling() || !this.Menu.Item("Heal.Activated").IsActive() || this.Menu.Item("PauseHealHotkey").GetValue<KeyBind>().Active)
                {
                    return;
                }

                foreach (var ally in HeroManager.Allies)
                {
                    if (!this.Menu.Item($"healon{ally.ChampionName}").IsActive())
                    {
                        return;
                    }

                    if (ally.IsRecalling() || ally.IsInvulnerable || ally.HasBuff("ChronoShift"))
                    {
                        return;
                    }

                    var enemies = ally.CountEnemiesInRange(750f);
                    var totalDamage = IncomingDamageManager.GetDamage(ally) * 1.1f;

                    if (ally.HealthPercent <= this.Menu.Item("min-health").GetValue<Slider>().Value && 
                        this.HealSpell.IsInRange(ally) && !ally.IsDead && enemies >= 1)
                    {
                        if ((int)(totalDamage / ally.Health) > this.Menu.Item("min-damage").GetValue<Slider>().Value
                            || ally.HealthPercent < this.Menu.Item("min-health").GetValue<Slider>().Value)
                        {
                            this.Player.Spellbook.CastSpell(this.HealSpell.Slot);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error occurred: '{0}'", e);
            }
        }

        #endregion
    }
}
