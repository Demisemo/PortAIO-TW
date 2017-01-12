using EloBuddy; 
using LeagueSharp.Common; 
namespace MoonRiven
{
    using System.Drawing;
    using LeagueSharp.Common;

    internal class MenuInit : Logic
    {
        internal static void Init()
        {
            Menu = new Menu("Moon-雷玟", "MoonRiven", true).SetFontStyle(FontStyle.Regular, menuColor);

            var targetMenu = Menu.AddSubMenu(new Menu("目標選擇器", "Target Selector"));
            {
                TargetSelector.AddToMenu(targetMenu);
            }

            var orbMenu = Menu.AddSubMenu(new Menu("走砍", "Orbwalking"));
            {
                Orbwalker = new Orbwalking.Orbwalker(orbMenu);
            }

            var comboMenu = Menu.AddSubMenu(new Menu("連招", "Combo"));
            {
                comboMenu.AddItem(new MenuItem("ComboQGap", "使用 Q 防突進", true).SetValue(false));
                comboMenu.AddItem(new MenuItem("ComboW", "使用 W", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboWLogic", "使用 W 邏輯", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboE", "使用 E", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboEGap", "使用 E 防突進", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboR1", "使用 R1", true).SetValue(new KeyBind('G', KeyBindType.Toggle, true)));
                comboMenu.AddItem(
                    new MenuItem("ComboR2", "使用 R2 模式: ", true).SetValue(
                        new StringList(new[] {"my Logic", "當可擊殺時", "快速", "關閉"})));
                comboMenu.AddItem(new MenuItem("ComboItem", "使用物品", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboYoumuu", "使用妖夢鬼刀", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboDot", "使用點燃", true).SetValue(true));
            }

            var burstMenu = Menu.AddSubMenu(new Menu("爆發連招", "Burst"));
            {
                burstMenu.AddItem(new MenuItem("BurstFlash", "使用閃現", true).SetValue(true));
                burstMenu.AddItem(new MenuItem("BurstDot", "使用點燃", true).SetValue(true));
                burstMenu.AddItem(
                    new MenuItem("BurstMode", "爆發連招模式: ", true).SetValue(
                        new StringList(new[] {"Shy 模式", "EQ 閃現模式"})));
                burstMenu.AddItem(
                    new MenuItem("BurstSwitch", "切換爆發連招熱鍵", true).SetValue(
                        new KeyBind('H',KeyBindType.Press))).ValueChanged += SwitchBurstMode;
            }

            var harassMenu = Menu.AddSubMenu(new Menu("騷擾", "Harass"));
            {
                harassMenu.AddItem(new MenuItem("HarassQ", "使用 Q", true).SetValue(true));
                harassMenu.AddItem(new MenuItem("HarassW", "使用 W", true).SetValue(true));
                harassMenu.AddItem(new MenuItem("HarassE", "使用 E", true).SetValue(true));
                harassMenu.AddItem(
                    new MenuItem("HarassMode", "騷擾模式: ", true).SetValue(new StringList(new[] { "智能", "預設" })));
            }

            var clearMenu = Menu.AddSubMenu(new Menu("清線/清除", "Clear"));
            {
                var laneClearMenu = clearMenu.AddSubMenu(new Menu("清線", "LaneClear"));
                {
                    laneClearMenu.AddItem(new MenuItem("LaneClearQ", "使用 Q", true).SetValue(true));
                    laneClearMenu.AddItem(new MenuItem("LaneClearQSmart", "使用 Q 智能農兵", true).SetValue(true)); 
                    laneClearMenu.AddItem(new MenuItem("LaneClearQT", "使用 Q 重置普擊防禦塔", true).SetValue(true));
                    laneClearMenu.AddItem(new MenuItem("LaneClearW", "使用 W", true).SetValue(true));
                    laneClearMenu.AddItem(
                        new MenuItem("LaneClearWCount", "使用 W 小兵命中數量 >= x", true).SetValue(new Slider(3, 1, 5)));
                    laneClearMenu.AddItem(new MenuItem("LaneClearItem", "使用物品", true).SetValue(true));
                }

                var jungleClearMenu = clearMenu.AddSubMenu(new Menu("清野", "JungleClear"));
                {
                    jungleClearMenu.AddItem(new MenuItem("JungleClearQ", "使用 Q", true).SetValue(true));
                    jungleClearMenu.AddItem(new MenuItem("JungleClearW", "使用 W", true).SetValue(true));
                    jungleClearMenu.AddItem(new MenuItem("JungleClearE", "使用 E", true).SetValue(true));
                    jungleClearMenu.AddItem(new MenuItem("JungleClearItem", "使用物品", true).SetValue(true));
                }
            }

            var fleeMenu = Menu.AddSubMenu(new Menu("逃跑", "Flee"));
            {
                fleeMenu.AddItem(new MenuItem("FleeQ", "使用 Q", true).SetValue(true));
                fleeMenu.AddItem(new MenuItem("FleeW", "使用 W", true).SetValue(true));
                fleeMenu.AddItem(new MenuItem("FleeE", "使用 E", true).SetValue(true));
            }

            var miscMenu = Menu.AddSubMenu(new Menu("雜項", "Misc"));
            {
                var qMenu = miscMenu.AddSubMenu(new Menu("Q 設置", "Q Settings"));
                {
                    qMenu.AddItem(new MenuItem("KeepQ", "保持 Q 靈活", true).SetValue(true));
                    qMenu.AddItem(
                        new MenuItem("QMode", "Q Mode: ", true).SetValue(new StringList(new[] {"至目標", "至鼠標"})));
                }

                var wMenu = miscMenu.AddSubMenu(new Menu("W 設置", "W Settings"));
                {
                    wMenu.AddItem(new MenuItem("AntiGapcloserW", "防突進", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("InterruptW", "中斷技能", true).SetValue(true));
                }

                var eMenu = miscMenu.AddSubMenu(new Menu("E 設置", "E Settings"));
                {
                    eMenu.AddItem(new MenuItem("DodgeE", "躲避", true).SetValue(true));
                }
            }

            var drawMenu = Menu.AddSubMenu(new Menu("顯示", "Drawings"));
            {
                drawMenu.AddItem(new MenuItem("DrawW", "顯示 W 範圍", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("DrawE", "顯示 E 範圍", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("DrawRStatus", "顯示 R 狀態", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawBurst", "顯示爆發連招", true).SetValue(true));
                DamageIndicator.AddToMenu(drawMenu);
            }


            Menu.AddItem(new MenuItem("asdqwe123asd", " ", true));
            Menu.AddItem(new MenuItem("Credits", "作者: NightMoon", true).SetFontStyle(FontStyle.Regular, menuColor));
            Menu.AddToMainMenu();
        }

        private static void SwitchBurstMode(object obj, OnValueChangeEventArgs Args)
        {
            if (Args.GetNewValue<KeyBind>().Active)
            {
                switch (Menu.Item("BurstMode", true).GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        Menu.Item("BurstMode", true).SetValue(new StringList(new[] { "Shy Mode", "EQ Flash Mode" }, 1));
                        break;
                    case 1:
                        Menu.Item("BurstMode", true).SetValue(new StringList(new[] { "Shy Mode", "EQ Flash Mode" }));
                        break;
                }
            }
        }

        internal static bool ComboQGap => Menu.Item("ComboQGap", true).GetValue<bool>();
        internal static bool ComboW => Menu.Item("ComboW", true).GetValue<bool>();
        internal static bool ComboWLogic => Menu.Item("ComboWLogic", true).GetValue<bool>();
        internal static bool ComboE => Menu.Item("ComboE", true).GetValue<bool>();
        internal static bool ComboEGap => Menu.Item("ComboEGap", true).GetValue<bool>();
        internal static bool ComboR => Menu.Item("ComboR1", true).GetValue<KeyBind>().Active;
        internal static int ComboR2 => Menu.Item("ComboR2", true).GetValue<StringList>().SelectedIndex;
        internal static bool ComboItem => Menu.Item("ComboItem", true).GetValue<bool>();
        internal static bool ComboYoumuu => Menu.Item("ComboYoumuu", true).GetValue<bool>();
        internal static bool ComboDot => Menu.Item("ComboDot", true).GetValue<bool>();
        internal static bool BurstFlash => Menu.Item("BurstFlash", true).GetValue<bool>();
        internal static bool BurstDot => Menu.Item("BurstDot", true).GetValue<bool>();
        internal static int BurstMode => Menu.Item("BurstMode", true).GetValue<StringList>().SelectedIndex;
        internal static bool HarassQ => Menu.Item("HarassQ", true).GetValue<bool>();
        internal static bool HarassW => Menu.Item("HarassW", true).GetValue<bool>();
        internal static bool HarassE => Menu.Item("HarassE", true).GetValue<bool>();
        internal static int HarassMode => Menu.Item("HarassMode", true).GetValue<StringList>().SelectedIndex;
        internal static bool LaneClearQ => Menu.Item("LaneClearQ", true).GetValue<bool>();
        internal static bool LaneClearQSmart => Menu.Item("LaneClearQSmart", true).GetValue<bool>();
        internal static bool LaneClearQT => Menu.Item("LaneClearQT", true).GetValue<bool>();
        internal static bool LaneClearItem => Menu.Item("LaneClearItem", true).GetValue<bool>();
        internal static int LaneClearWCount => Menu.Item("LaneClearWCount", true).GetValue<Slider>().Value;
        internal static bool LaneClearW => Menu.Item("LaneClearW", true).GetValue<bool>();
        internal static bool JungleClearQ => Menu.Item("JungleClearQ", true).GetValue<bool>();
        internal static bool JungleClearW => Menu.Item("JungleClearW", true).GetValue<bool>();
        internal static bool JungleClearE => Menu.Item("JungleClearE", true).GetValue<bool>();
        internal static bool JungleClearItem => Menu.Item("JungleClearItem", true).GetValue<bool>();
        internal static bool FleeQ => Menu.Item("FleeQ", true).GetValue<bool>();
        internal static bool FleeW => Menu.Item("FleeW", true).GetValue<bool>();
        internal static bool FleeE => Menu.Item("FleeE", true).GetValue<bool>();
        internal static bool KeepQ => Menu.Item("KeepQ", true).GetValue<bool>();
        internal static int QMode => Menu.Item("QMode", true).GetValue<StringList>().SelectedIndex;
        internal static bool AntiGapcloserW => Menu.Item("AntiGapcloserW", true).GetValue<bool>();
        internal static bool InterruptW => Menu.Item("InterruptW", true).GetValue<bool>();
        internal static bool DodgeE => Menu.Item("DodgeE", true).GetValue<bool>();
        internal static bool DrawW => Menu.Item("DrawW", true).GetValue<bool>();
        internal static bool DrawE => Menu.Item("DrawE", true).GetValue<bool>();
        internal static bool DrawRStatus => Menu.Item("DrawRStatus", true).GetValue<bool>();
        internal static bool DrawBurst => Menu.Item("DrawBurst", true).GetValue<bool>();
    }
}