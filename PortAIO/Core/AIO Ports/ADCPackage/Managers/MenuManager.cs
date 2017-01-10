using EloBuddy; namespace ADCPackage
{
    class Menu
    {
        public static LeagueSharp.Common.Menu Config = new LeagueSharp.Common.Menu("ADC Package", "adcpackage", true);
        public static LeagueSharp.Common.Orbwalking.Orbwalker Orbwalker;

        public static void Initialize()
        {
            ItemManager.Initialize();

            var orbwalkerMenu = new LeagueSharp.Common.Menu("Custom Orbwalker", "Custom Orbwalker");
            {
                Orbwalker = new LeagueSharp.Common.Orbwalking.Orbwalker(orbwalkerMenu);
                Config.AddSubMenu(orbwalkerMenu);
            }

            var itemMenu = new LeagueSharp.Common.Menu("Item Manager - soon", "Item Manager");
            {
                Config.AddSubMenu(itemMenu);
            }


            Config.AddToMainMenu();
        }

    }
}
