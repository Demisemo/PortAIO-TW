using EloBuddy; 
using LeagueSharp.Common; 
namespace myAurelionSol
{
    using System;
    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Program
    {
        public static void Main()
        {
            Game_OnGameLoad(new EventArgs());
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "AurelionSol")
            {
                return;
            }

            Logic.LoadAssembly();

            Chat.Print("my Series: " + ObjectManager.Player.ChampionName + " Load! Credit By NightMoon!");
        }
    }
}
