using EloBuddy; 
using LeagueSharp.Common; 
namespace ElUtilitySuite.Items.DefensiveItems
{
    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Randuins : Item
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public override ItemId Id => ItemId.Randuins_Omen;

        /// <summary>
        ///     Gets or sets the name of the item.
        /// </summary>
        /// <value>
        ///     The name of the item.
        /// </value>
        public override string Name => "蘭頓之兆";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        public override void CreateMenu()
        {
            this.Menu.AddItem(new MenuItem("UseRanduinsCombo", "使用").SetValue(true));
            this.Menu.AddItem(new MenuItem("Mode-randuins", "使用模式: ")).SetValue(new StringList(new[] { "總是使用", "連招時使用" }, 1));
            this.Menu.AddItem(new MenuItem("RanduinsCount", "當X敵人使用").SetValue(new Slider(3, 1, 5)));
        }

        /// <summary>
        ///     Shoulds the use item.
        /// </summary>
        /// <returns></returns>
        public override bool ShouldUseItem()
        {
            return this.Menu.Item("UseRanduinsCombo").IsActive() && this.Player.CountEnemiesInRange(500f) >= this.Menu.Item("RanduinsCount").GetValue<Slider>().Value;
        }

        /// <summary>
        ///     Uses the item.
        /// </summary>
        public override void UseItem()
        {
            if (this.Menu.Item("Mode-randuins").GetValue<StringList>().SelectedIndex == 1 && !this.ComboModeActive)
            {
                return;
            }

            Items.UseItem((int)this.Id);
        }

        #endregion
    }
}