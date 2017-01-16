﻿using EloBuddy;

namespace LeagueSharp.Common
{
    /// <summary>
    ///     Adds hacks to the menu.
    /// </summary>
    internal class Hacks
    {
        #region Constants

        private const int WM_KEYDOWN = 0x100;

        private const int WM_KEYUP = 0x101;

        #endregion

        #region Static Fields

        private static Menu menu;

        private static MenuItem MenuAntiAfk;

        private static MenuItem MenuDisableDrawings;

        private static MenuItem MenuDisableSay;

        private static MenuItem MenuTowerRange;

        #endregion

        #region Public Methods and Operators

        public static void Shutdown()
        {
            Menu.Remove(menu);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        internal static void Initialize()
        {
            menu = new Menu("Hacks", "Hacks");

            MenuAntiAfk = menu.AddItem(new MenuItem("AfkHack", "關閉掛網警告").SetValue(true));
            MenuAntiAfk.ValueChanged += (sender, args) => EloBuddy.Hacks.AntiAFK = args.GetNewValue<bool>();

            MenuDisableDrawings = menu.AddItem(new MenuItem("DrawingHack", "關閉所有顯示範圍").SetValue(false));
            MenuDisableDrawings.ValueChanged +=
                (sender, args) => EloBuddy.Hacks.DisableDrawings = args.GetNewValue<bool>();
            MenuDisableDrawings.SetValue(EloBuddy.Hacks.DisableDrawings);

            MenuDisableSay =menu.AddItem(new MenuItem("SayHack", "禁用L#發送聊天框").SetValue(false).SetTooltip("Block Game.Say from Assemblies"));
            MenuDisableSay.ValueChanged +=(sender, args) => EloBuddy.Hacks.IngameChat = args.GetNewValue<bool>();

            MenuTowerRange = menu.AddItem(new MenuItem("TowerHack", "顯示防禦塔範圍").SetValue(true));
            MenuTowerRange.ValueChanged +=
                (sender, args) => EloBuddy.Hacks.TowerRanges = args.GetNewValue<bool>();

            EloBuddy.Hacks.AntiAFK = MenuAntiAfk.GetValue<bool>();
            EloBuddy.Hacks.DisableDrawings = MenuDisableDrawings.GetValue<bool>();
            EloBuddy.Hacks.IngameChat = !MenuDisableSay.GetValue<bool>();
            EloBuddy.Hacks.TowerRanges = MenuTowerRange.GetValue<bool>();

            CommonMenu.Instance.AddSubMenu(menu);

            Game.OnWndProc += args =>
                {
                    if (!MenuDisableDrawings.GetValue<bool>())
                    {
                        return;
                    }

                    if ((int)args.WParam != Config.ShowMenuPressKey)
                    {
                        return;
                    }

                    if (args.Msg == WM_KEYDOWN)
                    {
                        EloBuddy.Hacks.DisableDrawings = false;
                    }

                    if (args.Msg == WM_KEYUP)
                    {
                        EloBuddy.Hacks.DisableDrawings = true;
                    }
                };
        }

        #endregion
    }
}