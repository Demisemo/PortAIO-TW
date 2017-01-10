using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace BadaoYasuo
{
    static class Program
    {
        public static readonly List<string> SupportedChampion = new List<string>()
        {
           "Yasuo"
        };
        public static void Main()
        {
            Game_OnGameLoad();
        }

        static void Game_OnGameLoad()
        {
            if (!SupportedChampion.Contains(ObjectManager.Player.ChampionName))
            {
                return;
            }
            Chat.Print("<font color=\"#24ff24\">Badao </font>" + "<font color=\"#ff8d1a\">" +
                ObjectManager.Player.ChampionName + "</font>" + "<font color=\"#24ff24\"> loaded!</font>");
            BadaoKingdom.BadaoChampion.BadaoYasuo.BadaoYasuo.BadaoActivate();

            // summoner spells

            BadaoMainVariables.Ignite = ObjectManager.Player.GetSpellSlot("SummonerDot");
            BadaoMainVariables.Flash  = ObjectManager.Player.GetSpellSlot("SummonerFlash");
            foreach (var spells in ObjectManager.Player.Spellbook.Spells.Where(
                x =>
                (x.Slot == SpellSlot.Summoner1 || x.Slot == SpellSlot.Summoner2)
                && x.Name.ToLower().Contains("smite")))
            {
                BadaoMainVariables.Smite = spells.Slot;
                break;
            }


        }
    }
}
