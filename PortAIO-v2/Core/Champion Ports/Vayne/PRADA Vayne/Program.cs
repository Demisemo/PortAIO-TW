using System;
using LeagueSharp;
using LeagueSharp.Common;
using PRADA_Vayne_Old.MyUtils;

using EloBuddy;
using LeagueSharp.Common;
namespace PRADA_Vayne_Old
{
    public class Program
    {
        #region Fields and Objects

        public static MActivator Activator;
        public static Orbwalking.Orbwalker Orbwalker;
        public static LeagueSharp.Common.Orbwalking.Orbwalker VHROrbwalker;
        public static EarlyEvade EarlyEvade;

        #region Menu

        public static Menu MainMenu;
        public static Menu ComboMenu;
        public static Menu LaneClearMenu;
        public static Menu EscapeMenu;
        public static Menu ActivatorMenu;
        public static Menu DrawingsMenu;
        public static Menu SkinhackMenu;
        public static Menu OrbwalkerMenu;

        #endregion Menu

        #region Spells

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        #endregion Spells

        #endregion

        public static void Main()
        {
            if (ObjectManager.Player.CharData.BaseSkinName == "Vayne")
            {
                Chat.Print("PRADA Vayne: Please switch to Challenger Series AIO, everything is improved there!");
                Chat.Print("PRADA Vayne: Open Loader > Install new assembly > GitHub > https://github.com/myo/LeagueSharp");
                Chat.Print("PRADA Vayne: Please don't use them together, new one has custom orbwalker.");
                EarlyEvade = new EarlyEvade();
                PRADAHijacker.AttemptHijack();
            }
        }
    }
}
