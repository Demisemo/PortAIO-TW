using EloBuddy; 
using LeagueSharp.Common; 
namespace Flowers_Yasuo.Manager.Menu
{
    using Evade;
    using SharpDX;
    using LeagueSharp.Common;
    using Orbwalking = Orbwalking;

    internal class MenuManager : Logic
    {
        internal static void Init()
        {
            Menu = new Menu("花邊-犽宿", "Flowers' Yasuo", true);

            var orbMenu = Menu.AddSubMenu(new Menu("走砍", "Orbwalker"));
            {
                Orbwalker = new Orbwalking.Orbwalker(orbMenu);
            }

            var comboMenu = Menu.AddSubMenu(new Menu("連招", "Combo"));
            {
                comboMenu.AddItem(new MenuItem("ComboQ", "使用 Q", true).SetValue(true));
                comboMenu.AddItem(
                    new MenuItem("ComboQStack", "使用 Q| 是否疊 Q(突進時)", true).SetValue(
                        new StringList(new[] {"Both", "只在英雄", "只在小兵", "關"}, 3)));
                comboMenu.AddItem(new MenuItem("ComboE", "使用 E", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboETurret", "使用 E 塔下", true).SetValue(false));
                comboMenu.AddItem(new MenuItem("ComboEGapcloser", "使用 E 防突進", true).SetValue(true));
                comboMenu.AddItem(
                    new MenuItem("ComboEMode", "Use E 防突進模式: ", true).SetValue(
                        new StringList(new[] {"目標", "鼠標"})));
                comboMenu.AddItem(
                    new MenuItem("ComboEGap", "Use E 防突進| 目標距離玩家 >=x", true).SetValue(
                        new Slider(250, 0, 1300)));
                comboMenu.AddItem(new MenuItem("ComboEQ", "使用 EQ", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboEQ3", "使用 EQ3", true).SetValue(true));
                comboMenu.AddItem(
                    new MenuItem("ComboR", "使用 R", true).SetValue(new KeyBind('R', KeyBindType.Toggle, true)));
                comboMenu.AddItem(
                    new MenuItem("ComboRHp", "使用 R|當敵方血量低於百分比 <= x%", true).SetValue(new Slider(50)));
                comboMenu.AddItem(
                    new MenuItem("ComboRCount", "使用 R|當敵方擊飛x目標 >= x", true).SetValue(
                        new Slider(2, 1, 5)));
                comboMenu.AddItem(
                    new MenuItem("ComboEQFlash", "使用 EQ 閃現?", true).SetValue(new KeyBind('E', KeyBindType.Toggle)));
                comboMenu.AddItem(
                    new MenuItem("ComboEQFlashSolo", "使用 EQ 閃現|單挑模式", true).SetValue(true));
                comboMenu.AddItem(
                    new MenuItem("ComboEQFlashTeam", "使用 EQ 閃現|團戰模式", true).SetValue(true));
                comboMenu.AddItem(
                    new MenuItem("ComboEQFlashTeamCount", "使用 EQ 閃現|命中計算 >= x", true).SetValue(
                        new Slider(3, 1, 5)));
                comboMenu.AddItem(
                    new MenuItem("ComboEQFlashTeamAlly", "使用 EQ 閃現|隊友擊飛計算 >= x", true).SetValue(
                        new Slider(2, 0, 5)));
                comboMenu.AddItem(new MenuItem("ComboIgnite", "使用 點燃", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboItems", "使用 物品", true).SetValue(true));
            }

            var harassMenu = Menu.AddSubMenu(new Menu("騷擾", "Harass"));
            {
                harassMenu.AddItem(new MenuItem("HarassQ", "使用 Q", true).SetValue(true));
                harassMenu.AddItem(new MenuItem("HarassQ3", "使用 Q3", true).SetValue(true));
                harassMenu.AddItem(new MenuItem("HarassE", "使用 E", true).SetValue(false));
                harassMenu.AddItem(new MenuItem("HarassTower", "使用 E 塔下", true).SetValue(true));
            }

            var laneClearMenu = Menu.AddSubMenu(new Menu("清線", "LaneClear"));
            {
                laneClearMenu.AddItem(new MenuItem("LaneClearQ", "使用 Q", true).SetValue(true));
                laneClearMenu.AddItem(new MenuItem("LaneClearQ3", "使用 Q3", true).SetValue(true));
                laneClearMenu.AddItem(
                    new MenuItem("LaneClearQ3count", "使用 Q3| 小兵數量 >= x", true).SetValue(new Slider(3, 1, 5)));
                laneClearMenu.AddItem(new MenuItem("LaneClearE", "使用 E", true).SetValue(true));
                laneClearMenu.AddItem(new MenuItem("LaneClearETurret", "使用 E 塔下", true).SetValue(false));
                laneClearMenu.AddItem(new MenuItem("LaneClearItems", "使用物品", true).SetValue(true));
            }

            var jungleClearMenu = Menu.AddSubMenu(new Menu("清野", "JungleClear"));
            {
                jungleClearMenu.AddItem(new MenuItem("JungleClearQ", "使用 Q", true).SetValue(true));
                jungleClearMenu.AddItem(new MenuItem("JungleClearQ3", "使用 Q3", true).SetValue(true));
                jungleClearMenu.AddItem(new MenuItem("JungleClearE", "使用 E", true).SetValue(true));
            }

            var lastHitMenu = Menu.AddSubMenu(new Menu("農兵", "LastHit"));
            {
                lastHitMenu.AddItem(new MenuItem("LastHitQ", "使用 Q", true).SetValue(true));
                lastHitMenu.AddItem(new MenuItem("LastHitQ3", "使用 Q3", true).SetValue(true));
                lastHitMenu.AddItem(new MenuItem("LastHitE", "使用 E", true).SetValue(true));
                lastHitMenu.AddItem(new MenuItem("LastHitETurret", "使用 E 塔下", true).SetValue(false));
            }

            var fleeMenu = Menu.AddSubMenu(new Menu("逃跑", "Flee"));
            {
                fleeMenu.AddItem(new MenuItem("FleeQ", "使用 Q", true).SetValue(true));
                fleeMenu.AddItem(new MenuItem("FleeQ3", "使用 Q3", true).SetValue(true));
                fleeMenu.AddItem(new MenuItem("FleeE", "使用 E", true).SetValue(true));
                fleeMenu.AddItem(new MenuItem("FleeWallJump", "使用 E 過牆", true).SetValue(true));
            }

            var miscMenu = Menu.AddSubMenu(new Menu("雜項", "Misc"));
            {
                var qMenu = miscMenu.AddSubMenu(new Menu("Q 設置", "Q Settings"));
                {
                    qMenu.AddItem(new MenuItem("KillStealQ", "使用 Q 搶頭", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("KillStealQ3", "使用 Q3 搶頭", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("Q3Int", "使用 Q3 Interrupter", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("Q3Anti", "使用 Q3 AntiGapcloser", true).SetValue(true));
                    qMenu.AddItem(
                        new MenuItem("StackQ", "疊 Q", true).SetValue(new KeyBind('T', KeyBindType.Toggle, true)));
                    qMenu.AddItem(
                        new MenuItem("AutoQ", "自動 Q 騷擾敵人", true).SetValue(new KeyBind('N', KeyBindType.Toggle, true)));
                    qMenu.AddItem(
                        new MenuItem("AutoQ3", "自動 Q3 騷擾敵人", true).SetValue(false));
                }

                var eMenu = miscMenu.AddSubMenu(new Menu("E 設置", "E Settings"));
                {
                    eMenu.AddItem(new MenuItem("KillStealE", "使用 E 搶頭", true).SetValue(true));
                }

                var rMenu = miscMenu.AddSubMenu(new Menu("R 設置", "R Settings"));
                {
                    var rWhitelist = rMenu.AddSubMenu(new Menu("R 白名單", "R Whitelist"));
                    {
                        foreach (var hero in HeroManager.Enemies)
                        {
                            rWhitelist.AddItem(
                                new MenuItem("R" + hero.ChampionName.ToLower(), hero.ChampionName, true).SetValue(true));
                        }
                    }

                    var autoR = rMenu.AddSubMenu(new Menu("自動 R", "Auto R"));
                    {
                        autoR.AddItem(new MenuItem("AutoR", "自動 R", true)).SetValue(true);
                        autoR.AddItem(
                            new MenuItem("AutoRCount", "自動 R|當敵方擊飛x目標 >= x", true).SetValue(
                                new Slider(3, 1, 5)));
                        autoR.AddItem(
                            new MenuItem("AutoRRangeCount", "自動 R|當隊友擊飛x目標 >= x", true).SetValue(
                                new Slider(2, 1, 5)));
                        autoR.AddItem(
                            new MenuItem("AutoRMyHp", "自動 R|當敵方血量低於百分比 >= x%", true).SetValue(
                                new Slider(50)));
                    }
                }

                var evadeSettings = miscMenu.AddSubMenu(new Menu("躲避設置", "Evade Settings"));
                {
                    var evadespellSettings = evadeSettings.AddSubMenu(new Menu("躲避技能", "Dodge Spells"));
                    {
                        EvadeManager.Init(evadespellSettings);
                    }

                    var evadeMenu = evadeSettings.AddSubMenu(new Menu("躲避目標", "EvadeTarget"));
                    {
                        EvadeTargetManager.Init(evadeMenu);
                    }
                }

                var skinMenu = miscMenu.AddSubMenu(new Menu("造型變換", "SkinChange"));
                {
                    skinMenu.AddItem(new MenuItem("EnableSkin", "啟用", true).SetValue(false)).ValueChanged += EnbaleSkin;
                    skinMenu.AddItem(
                        new MenuItem("SelectSkin", "選擇造型: ", true).SetValue(
                            new StringList(new[]
                            {
                                "Classic", "High Noon", "Project: Yasuo", "Blood Moon", "Others", "Others1", "Others2",
                                "Others3", "Others4"
                            })));
                }

                var autoWardMenu = miscMenu.AddSubMenu(new Menu("自動插眼", "Auto Ward"));
                {
                    autoWardMenu.AddItem(new MenuItem("AutoWardEnable", "啟用", true).SetValue(true));
                    autoWardMenu.AddItem(new MenuItem("OnlyCombo", "只在連招模式有效", true).SetValue(true));
                }

                miscMenu.AddItem(new MenuItem("EQFlash", "EQ閃現按鍵", true).SetValue(new KeyBind('A', KeyBindType.Press)));

            }

            var drawMenu = Menu.AddSubMenu(new Menu("顯示", "Draw"));
            {
                drawMenu.AddItem(new MenuItem("DrawQ", "顯示 Q 範圍", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawQ3", "顯示 Q3 範圍", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawW", "顯示 W 範圍", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawE", "顯示 E 範圍", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawR", "顯示 R 範圍", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawSpots", "顯示過牆點", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawStackQ", "顯示疊Q狀態", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawAutoQ", "顯示自動Q狀態", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawComboEQStatus", "顯示EQ閃現狀態", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawRStatus", "顯示連招時R狀態", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawStackQPerma", "顯示疊 Q PermaShow", true).SetValue(true))
                    .ValueChanged += StackQChanged;
                drawMenu.AddItem(new MenuItem("DrawAutoQPerma", "顯示自動 Q PermaShow", true).SetValue(true))
                    .ValueChanged += AutoQChanged;
                drawMenu.AddItem(new MenuItem("DrawComboEQPerma", "顯示連招 EQ 閃現 PermaShow", true).SetValue(true))
                    .ValueChanged += ComboEQFlashChanged;
                drawMenu.AddItem(new MenuItem("DrawRStatusPerma", "顯示連招 R PermaShow", true).SetValue(true))
                    .ValueChanged += ComboRChanged;
            }

            Menu.AddItem(new MenuItem("asd ad asd ", " ", true));
            Menu.AddItem(new MenuItem("Credit", "作者: NightMoon", true));

            Menu.AddToMainMenu();

            if (Menu.Item("DrawStackQPerma", true).GetValue<bool>())
            {
                Menu.Item("StackQ", true).Permashow(true, "Stack Q Active", Color.MediumSlateBlue);
            }

            if (Menu.Item("DrawAutoQPerma", true).GetValue<bool>())
            {
                Menu.Item("AutoQ", true).Permashow(true, "Auto Q Active", Color.Orange);
            }

            if (Menu.Item("DrawComboEQPerma", true).GetValue<bool>())
            {
                Menu.Item("ComboEQFlash", true).Permashow(true, "Combo EQFlash Active", Color.Pink);
            }

            if (Menu.Item("DrawRStatusPerma", true).GetValue<bool>())
            {
                Menu.Item("ComboR", true).Permashow(true, "Combo R Active", Color.PowderBlue);
            }
        }

        private static void ComboEQFlashChanged(object obj, OnValueChangeEventArgs Args)
        {
            Menu.Item("ComboEQFlash", true).Permashow(Args.GetNewValue<bool>(), "Combo EQFlash Active", Color.Pink);
        }

        private static void ComboRChanged(object obj, OnValueChangeEventArgs Args)
        {
            Menu.Item("ComboR", true).Permashow(Args.GetNewValue<bool>(), "Combo R Active", Color.PowderBlue);
        }

        private static void AutoQChanged(object obj, OnValueChangeEventArgs Args)
        {
            Menu.Item("AutoQ", true).Permashow(Args.GetNewValue<bool>(), "Auto Q Active", Color.Orange);
        }

        private static void StackQChanged(object obj, OnValueChangeEventArgs Args)
        {
            Menu.Item("StackQ", true).Permashow(Args.GetNewValue<bool>(), "Stack Q Active", Color.MediumSlateBlue);
        }
    }
}