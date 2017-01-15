using EloBuddy; 
using LeagueSharp.Common; 
namespace Flowers_Riven_Reborn.Manager.Menu
{
    using SharpDX;
    using LeagueSharp.Common;
    using FlowersRivenCommon;

    internal class MenuManager : Logic
    {
        private static readonly Color menuColor = new Color(3, 253, 241);

        internal static void Init()
        {
            Menu =
                new Menu("花邊-雷玟", "Flowers' Riven Reborn", true).SetFontStyle(
                    System.Drawing.FontStyle.Regular, menuColor);

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
                comboMenu.AddItem(new MenuItem("ComboQ", "使用 Q 防突進", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboW", "使用 W", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboQW", "使用 Q1 + W", true).SetValue(false));
                comboMenu.AddItem(new MenuItem("ComboEW", "使用 E + W", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboE", "使用 E", true).SetValue(true));
                comboMenu.AddItem(new MenuItem("ComboR", "使用 R", true).SetValue(true));
                comboMenu.AddItem(
                    new MenuItem("R1Combo", "使用 R1", true).SetValue(new KeyBind('G', KeyBindType.Toggle, true))).Permashow();
                comboMenu.AddItem(
                    new MenuItem("R2Mode", "使用 R2 模式: ", true).SetValue(
                        new StringList(new[] { "只擊殺時", "花邊邏輯", "快速邏輯", "關閉" }, 1)));
                comboMenu.AddItem(new MenuItem("ComboIgnite", "使用點燃", true).SetValue(true));
            }

            var burstMenu = Menu.AddSubMenu(new Menu("爆發連招", "Burst"));
            {
                burstMenu.AddItem(new MenuItem("BurstFlash", "使用 閃現", true).SetValue(true));
                burstMenu.AddItem(new MenuItem("BurstIgnite", "使用 點燃", true).SetValue(true));
                burstMenu.AddItem(new MenuItem("Note...", "注意: ", true));
                burstMenu.AddItem(new MenuItem("target...", "左鍵單擊目標", true));
                burstMenu.AddItem(new MenuItem("range...", "和目標在爆發連招範圍", true));
                burstMenu.AddItem(new MenuItem("press...", "然後爆發連招熱鍵", true));
            }

            var harassMenu = Menu.AddSubMenu(new Menu("騷擾", "Harass"));
            {
                harassMenu.AddItem(new MenuItem("HarassQ", "使用 Q", true).SetValue(true));
                harassMenu.AddItem(new MenuItem("HarassW", "使用 W", true).SetValue(true));
                harassMenu.AddItem(new MenuItem("HarassE", "使用 E", true).SetValue(true));
                harassMenu.AddItem(
                    new MenuItem("HarassMode", "騷擾模式", true).SetValue(new StringList(new[] { "智能", "爆發" })));
            }

            var clearMenu = Menu.AddSubMenu(new Menu("清線/清野", "Clear"));
            {
                var laneClearMenu = clearMenu.AddSubMenu(new Menu("清線", "LaneClear"));
                {
                    laneClearMenu.AddItem(new MenuItem("LaneClearQ", "使用 Q", true).SetValue(true));
                    laneClearMenu.AddItem(new MenuItem("LaneClearW", "使用 W", true).SetValue(true));
                }

                var jungleClearMenu = clearMenu.AddSubMenu(new Menu("清野", "JungleClear"));
                {
                    jungleClearMenu.AddItem(new MenuItem("JungleClearQ", "使用 Q", true).SetValue(true));
                    jungleClearMenu.AddItem(new MenuItem("JungleClearW", "使用 W", true).SetValue(true));
                    jungleClearMenu.AddItem(new MenuItem("JungleClearWLogic", "使用 W 智能", true).SetValue(false));
                    jungleClearMenu.AddItem(new MenuItem("JungleClearE", "使用 E", true).SetValue(true));
                }
            }

            var killStealMenu = Menu.AddSubMenu(new Menu("搶頭", "KillSteal"));
            {
                killStealMenu.AddItem(new MenuItem("KillStealW", "使用 W", true).SetValue(true));
                killStealMenu.AddItem(
                    new MenuItem("KillStealE", "使用 E", true).SetValue(true).SetTooltip("E 防突進和 R2 搶頭"));
                killStealMenu.AddItem(new MenuItem("KillStealR", "使用 R", true).SetValue(true));
            }

            var miscMenu = Menu.AddSubMenu(new Menu("雜項", "Misc"));
            {
                var qMenu = miscMenu.AddSubMenu(new Menu("Q 設置", "Q Setting"));
                {
                    var qDelayMenu = qMenu.AddSubMenu(new Menu("延遲設置", "Delay Settings"));
                    {
                        qDelayMenu.AddItem(new MenuItem("Q1Delay", "Q1 延遲: ", true).SetValue(new Slider(280, 200, 350)));
                        qDelayMenu.AddItem(new MenuItem("Q2Delay", "Q2 延遲: ", true).SetValue(new Slider(280, 200, 350)));
                        qDelayMenu.AddItem(new MenuItem("Q3Delay", "Q3 延遲: ", true).SetValue(new Slider(380, 300, 450)));
                        qDelayMenu.AddItem(new MenuItem("AutoSetDelay", "包括Ping?", true).SetValue(false)).ValueChanged +=
                            DelayChanged;
                        qDelayMenu.AddItem(new MenuItem("MinDelay", "設置 QA 延遲?", true).SetValue(false)).ValueChanged +=
                            myDelayChanged;
                    }

                    qMenu.AddItem(new MenuItem("KeepQALive", "保持 Q 靈活", true).SetValue(true));
                    qMenu.AddItem(new MenuItem("Dance", "使用表情動作減少 QA 動畫", true).SetValue(false));
                    qMenu.AddItem(
                        new MenuItem("QMode", "Q 模式 : ", true).SetValue(
                            new StringList(new[] { "目標位置", "鼠標", "最大 Q 選擇目標", "最大 Q 鼠標位置" })));
                }

                var wMenu = miscMenu.AddSubMenu(new Menu("W 設置", "W Setting"));
                {
                    wMenu.AddItem(new MenuItem("AntiGapCloserW", "防突進", true).SetValue(true));
                    wMenu.AddItem(new MenuItem("InterruptTargetW", "中斷技能", true).SetValue(true));
                }

                var eMenu = miscMenu.AddSubMenu(new Menu("E 設置", "E Settings"));
                {
                    foreach (var target in HeroManager.Enemies)
                    {
                        if (target.ChampionName == "Darius")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("Darius", "Darius Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeDariusR", "Shield R", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "Garen")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("Garen", "Garen Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeGarenQ", "Shield Q", true).SetValue(true));
                                spellMenu.AddItem(new MenuItem("EDodgeGarenR", "Shield R", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "Irelia")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("Irelia", "Irelia Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeIreliaE", "Shield E", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "LeeSin")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("LeeSin", "LeeSin Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeLeeSinR", "Shield R", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "Olaf")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("Olaf", "Olaf Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeOlafE", "Shield E", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "Pantheon")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("Pantheon", "Pantheon Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgePantheonW", "Shield W", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "Renekton")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("Renekton", "RenektonW Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeRenektonW", "Shield W", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "Rengar")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("Rengar", "Rengar Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeRengarQ", "Shield Q", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "Veigar")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("Veigar", "Veigar Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeVeigarR", "Shield R", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "Volibear")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("Volibear", "Volibear Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeVolibearW", "Shield W", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "XenZhao")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("XenZhao", "XenZhao Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeXenZhaoQ3", "Shield Q3", true).SetValue(true));
                            }
                        }

                        if (target.ChampionName == "TwistedFate")
                        {
                            var spellMenu = eMenu.AddSubMenu(new Menu("Twisted Fate", "TwistedFate Spell"));
                            {
                                spellMenu.AddItem(new MenuItem("EDodgeTwistedFateW", "Shield W", true).SetValue(true));
                            }
                        }
                    }

                    eMenu.AddItem(new MenuItem("EShielddogde", "Use E Shield Spell", true).SetValue(true));
                }

                var skinMenu = miscMenu.AddSubMenu(new Menu("造型", "SkinChance"));
                {
                    SkinManager.AddToMenu(skinMenu, 7);
                }
            }

            var drawMenu = Menu.AddSubMenu(new Menu("顯示", "Drawings"));
            {
                drawMenu.AddItem(new MenuItem("DrawW", "顯示 W 範圍", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("DrawBurst", "顯示爆發連招範圍", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawRStatus", "顯示 R 狀態", true).SetValue(true));
                //DamageIndicator.AddToMenu(drawMenu, DamageCalculate.GetComboDamage);
            }

            Menu.AddItem(new MenuItem("asdvre1w56", "  "));
            Menu.AddItem(new MenuItem("Credit", "作者 : NightMoon")).SetFontStyle(
                    System.Drawing.FontStyle.Regular, menuColor);
            Menu.AddItem(new MenuItem("Version", "版本 : 2.0.0.0")).SetFontStyle(
                    System.Drawing.FontStyle.Regular, menuColor);

            Menu.AddToMainMenu();

            if (!Menu.Item("AutoSetDelay", true).GetValue<bool>())
            {
                Menu.Item("Q1Delay", true).SetValue(new Slider(280, 200, 350));
                Menu.Item("Q2Delay", true).SetValue(new Slider(280, 200, 350));
                Menu.Item("Q3Delay", true).SetValue(new Slider(380, 300, 450));
            }

            if (Menu.Item("AutoSetDelay", true).GetValue<bool>())
            {
                Menu.Item("MinDelay", true).SetValue(false);
            }
        }

        private static void DelayChanged(object obj, OnValueChangeEventArgs Args)
        {
            if (!Args.GetNewValue<bool>())
            {
                Menu.Item("Q1Delay", true).SetValue(new Slider(280, 200, 350));
                Menu.Item("Q2Delay", true).SetValue(new Slider(280, 200, 350));
                Menu.Item("Q3Delay", true).SetValue(new Slider(380, 300, 450));
            }
        }

        private static void myDelayChanged(object obj, OnValueChangeEventArgs Args)
        {
            if (Args.GetNewValue<bool>())
            {
                Menu.Item("Q1Delay", true).SetValue(new Slider(250, 200, 350));
                Menu.Item("Q2Delay", true).SetValue(new Slider(250, 200, 350));
                Menu.Item("Q3Delay", true).SetValue(new Slider(350, 300, 450));
            }
        }
    }
}
