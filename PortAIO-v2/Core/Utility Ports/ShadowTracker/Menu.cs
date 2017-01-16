using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


using EloBuddy; namespace ShadowTracker
{
    class MainMenu
    {
        public static Menu _MainMenu;        
        public static void Menu()
        {
            try // try start
            {
                _MainMenu = new Menu("敵人閃現進迷霧中的位置", "ShadowTracker", true);
                _MainMenu.AddToMainMenu();

                var Draw = new Menu("顯示", "Draw");
                {
                    Draw.AddItem(new MenuItem("Skill", "技能").SetValue(true));
                    Draw.AddItem(new MenuItem("Spell", "咒語").SetValue(true));
                    Draw.AddItem(new MenuItem("Item", "項目").SetValue(true));
                }
                _MainMenu.AddSubMenu(Draw);                
            } // try end     
            catch (Exception e)
            {
                Console.Write(e);
                Chat.Print("當腳本不能使用、任何BUG，聯絡作者: by KorFresh (Code 1)");
            }           
            
        }
    }
}
