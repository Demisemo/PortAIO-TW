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
namespace BadaoKingdom.BadaoChampion.BadaoKatarina
{
    using static BadaoKatarinaVariables;
    using static BadaoMainVariables;
    public static class BadaoKatarinaConfig
    {
        public static Menu config;
        public static void BadaoActivate()
        {
            // spells init
            BadaoMainVariables.Q = new Spell(SpellSlot.Q, 625);
            BadaoMainVariables.Q2 = new Spell(SpellSlot.Q);
            BadaoMainVariables.W = new Spell(SpellSlot.W);
            BadaoMainVariables.E = new Spell(SpellSlot.E, 725);
            BadaoMainVariables.R = new Spell(SpellSlot.R, 550);

            // main menu
            config = new Menu("Badao-卡特蓮娜" + ObjectManager.Player.ChampionName, ObjectManager.Player.ChampionName, true);
            config.SetFontStyle(System.Drawing.FontStyle.Bold, SharpDX.Color.YellowGreen);

            // orbwalker menu
            Menu orbwalkerMenu = new Menu("走砍", "Orbwalker");
            BadaoMainVariables.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            config.AddSubMenu(orbwalkerMenu);

            // TS
            Menu ts = config.AddSubMenu(new Menu("目標選擇器", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            // Combo
            Menu Combo = config.AddSubMenu(new Menu("連招", "Combo"));
            ComboCancelRForKS = Combo.AddItem(new MenuItem("ComboCancelRForKS", "不使用R去搶頭")).SetValue(true);
            ComboCancelRNoTarget = Combo.AddItem(new MenuItem("ComboCancelRNoTarget", "如果沒有目標敵人，請取消R")).SetValue(true);

            // Harass
            Menu Harass = config.AddSubMenu(new Menu("騷擾", "Harass"));
            HarassWE = Harass.AddItem(new MenuItem("HarassWE", "W-E 旋轉")).SetValue(true);

            // LaneClear
            Menu LaneClear = config.AddSubMenu(new Menu("清線", "LaneClear"));
            LaneClearQ = LaneClear.AddItem(new MenuItem("LaneClearQ", "使用Q")).SetValue(true);
            LaneClearW = LaneClear.AddItem(new MenuItem("LaneClearW", "使用W")).SetValue(true);

            // LastHit
            Menu LastHit = config.AddSubMenu(new Menu("農兵", "LastHit"));
            LastHitQ = LastHit.AddItem(new MenuItem("LastHitQ", "使用Q")).SetValue(true);

            //// JungleClear
            //Menu JungleClear = config.AddSubMenu(new Menu("JungleClear", "JungleClear"));

            // Auto
            Menu Auto = config.AddSubMenu(new Menu("自動", "Auto"));
            AutoKs = Auto.AddItem(new MenuItem("AutoKs", "搶頭")).SetValue(true);

            //FleeAndWallJump
            Menu FleeJump = config.AddSubMenu(new Menu("逃跑", "Flee And Walljump"));
            FleeKey = FleeJump.AddItem(new MenuItem("FleeKey", "逃跑熱鍵").SetValue(new KeyBind('G', KeyBindType.Press)));
            JumpKey = FleeJump.AddItem(new MenuItem("JumpKey", "過牆熱鍵").SetValue(new KeyBind('H', KeyBindType.Press)));

            // attach to mainmenu
            config.AddToMainMenu();
        }
    }
}
