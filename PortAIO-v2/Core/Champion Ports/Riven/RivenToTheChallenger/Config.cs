using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace RivenToTheChallenger
{
    class Config
    {
        //Flame about singleton inc
        private Menu menu;

        private static string scriptPrefix => "andre.rttc.";

        public Orbwalking.Orbwalker Orbwalker;

        public MenuItem this[string itemName] => menu.Item(scriptPrefix + itemName);

        public Config()
        {
            menu = new Menu("挑戰者-雷玟", "andre.rttc", true);
            
            #region SubMenus declaration
            var orbwalkerMenu = new Menu("走砍", "andre.rttc.orbwalker");
            var comboMenu =  new Menu("連招設置", "andre.rttc.combo");
            var utilityMenu =  new Menu("大招設置", "andre.rttc.utility");
            var fleeMenu =  new Menu("逃跑設置", "andre.rttc.flee");
            var jungleMenu =  new Menu("清線設置", "andre.rttc.jungle");
            var eMenu =  new Menu("E 設置", "andre.rttc.eee");
            var drawMenu =  new Menu("顯示設置", "andre.rttc.draw");
            var advancedMenu =  new Menu("高級設置", "andre.rttc.advanced");
            #endregion 

            #region SubMenus
            
            #region Orbwalker
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Orbwalker.RegisterCustomMode(orbwalkerMenu.Name + ".escape", "Escape", 'Y');
            menu.AddSubMenu(orbwalkerMenu);
            #endregion
            
            #region Combo Settings
            #region SpellUsage
            var comboSpellUsage = new Menu("技能使用", comboMenu.Name + ".spells"); //combo.spells
            comboSpellUsage.AddItem(new MenuItem(comboSpellUsage.Name + ".useq", "使用 Q").SetValue(true));
            comboSpellUsage.AddItem(new MenuItem(comboSpellUsage.Name + ".usew", "使用 W").SetValue(true));
            comboSpellUsage.AddItem(new MenuItem(comboSpellUsage.Name + ".usehydra", "使用九頭蛇").SetValue(true));
            comboSpellUsage.AddItem(new MenuItem(comboSpellUsage.Name + ".useygb", "使用妖夢鬼刀").SetValue(true));
            comboMenu.AddSubMenu(comboSpellUsage);
            #endregion
            
            #region Initiator
            var initiatorMenu = new Menu("中斷技能", comboMenu.Name + ".ew");//combo.ew
            initiatorMenu.AddItem(
                new MenuItem(initiatorMenu.Name + ".w", "使用 E -> W").SetValue(
                    new StringList(new[] { "Force Q", "Force AA", "Automatic" }, 2)));
            comboMenu.AddSubMenu(initiatorMenu);
            #endregion

            #region GapClose
            var comboGapClose = new Menu("防突進", comboMenu.Name + ".gap"); //combo.gap
            comboGapClose.AddItem(
                new MenuItem(comboGapClose.Name + ".q", "防突進 [使用 - Q]").SetValue(
                    new StringList(new[] { "Disabled", "In Combo", "在連招+目標中左鍵單擊" })));
            comboGapClose.AddItem(
                new MenuItem(comboGapClose.Name + ".w", "防突進 [使用 - W]").SetValue(
                    new StringList(new[] { "Disabled", "In Combo", "在連招+目標中左鍵單擊" })));
            comboMenu.AddSubMenu(comboGapClose);
            #endregion

            #region R Settings
            var comboRMenu = new Menu("R設置", comboMenu.Name + ".r"); //combo.r
            comboRMenu.AddItem(new MenuItem(comboRMenu.Name + ".r1", "使用 R1").SetValue(true));
            comboRMenu.AddItem(new MenuItem(comboRMenu.Name + ".r2", "使用 R2").SetValue(true));
            //comboRMenu.AddItem(new MenuItem("andre.rttc.combo.r.r2bindfr1", "Bind R2 Cast to Force R1").SetValue(true));
            comboRMenu.AddItem(
                new MenuItem("andre.rttc.combo.r.combomode", "在下一個動畫後強制R1").SetValue(new KeyBind('N',
                    KeyBindType.Toggle, false)));
            comboMenu.AddSubMenu(comboRMenu);
            #endregion

            menu.AddSubMenu(comboMenu);
            #endregion
            
            #region Utility Menu
            #region KillSteal
            var killStealMenu = new Menu("搶頭", utilityMenu.Name + ".ks"); //utility.ks
            killStealMenu.AddItem(new MenuItem(killStealMenu.Name + ".useq", "使用 Q").SetValue(true));
            killStealMenu.AddItem(new MenuItem(killStealMenu.Name + ".usew", "使用 W").SetValue(true));
            killStealMenu.AddItem(new MenuItem(killStealMenu.Name + ".user", "使用 R2").SetValue(true));
            killStealMenu.AddItem(new MenuItem(killStealMenu.Name + ".usei", "使用 點燃").SetValue(true));

            utilityMenu.AddSubMenu(killStealMenu);
            #endregion

            #region Defensive
            var defensiveMenu = new Menu("防禦性", utilityMenu.Name + ".defensive"); //utility.defensive
            defensiveMenu.AddItem(new MenuItem(defensiveMenu.Name + ".w", "使用 W").SetValue(true));
            utilityMenu.AddSubMenu(defensiveMenu);
            #endregion

            menu.AddSubMenu(utilityMenu);

            #endregion

            #region Flee Menu
            //utility
            fleeMenu.AddItem(new MenuItem(fleeMenu.Name + ".useq", "使用 Q").SetValue(true));
            fleeMenu.AddItem(new MenuItem(fleeMenu.Name + ".usee", "使用 E").SetValue(true));
            fleeMenu.AddItem(new MenuItem(fleeMenu.Name + ".key", "可以在走砍設置"));
            menu.AddSubMenu(fleeMenu);
            #endregion

            #region Jungle

            #endregion

            #region Harass

            #endregion

            #region Draw

            #endregion

            #region Advanced

            #endregion
            #endregion

            menu.AddToMainMenu();


        }
    }
}
