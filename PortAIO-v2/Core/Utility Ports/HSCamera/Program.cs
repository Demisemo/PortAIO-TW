// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="hsCamera">
//      Copyright (c) hsCamera. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using hsCamera.Handlers;
using LeagueSharp;
using LeagueSharp.Common;
using Color = SharpDX.Color;

using EloBuddy; 
using LeagueSharp.Common; 
namespace hsCamera
{
    internal class Program
    {
        public static Menu _config;

        private static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        public static List<AIHeroClient> HeroList => HeroManager.Enemies.ToList();
        // ReSharper disable once UnusedParameter.Local
        public static void Main()
        {
            HsCameraOnLoad(new EventArgs());
        }

        private static void AddChampionMenu()
        {
            var enemycammenu = _config.AddSubMenu(new Menu("敵人拍攝", "enemiescamera"));
            for (var i = 0; i < HeroList.Count; i++)
            {
                enemycammenu.AddItem(new MenuItem("enemies" + HeroList[i].ChampionName,
                        "Show Enemy -> (" + HeroList[i].ChampionName + ")")
                    .SetValue(new KeyBind(Convert.ToUInt32(96 + i), KeyBindType.Press)));

                _config.Item("enemies" + HeroList[i].ChampionName).ValueChanged += (sender, e) =>
                {
                    if (e.GetNewValue<KeyBind>().Active == e.GetOldValue<KeyBind>().Active) return;
                    if (e.GetNewValue<KeyBind>().Active == false) CameraMovement.SemiDynamic(Player.Position);
                };
            }
        }

        /// <summary>
        ///     Amazing OnLoad :jew:
        /// </summary>
        /// <param name="args"></param>
        private static void HsCameraOnLoad(EventArgs args)
        {
            _config = new Menu("HS轉鏡頭 [正式]", "hsCamera", true);
            {
                AddChampionMenu();
                var cameraSpeeds = _config.AddSubMenu(new Menu("拍攝速度[設置]", "cameraspeeds"));
                cameraSpeeds.AddItem(new MenuItem("followcurspeed", "跟隨鼠標(拍攝速度)"))
                    .SetValue(new Slider(23, 1, 50));
                cameraSpeeds.AddItem(new MenuItem("followtfspeed", "跟隨團戰鬥(拍攝速度)"))
                    .SetValue(new Slider(17, 1, 50));
                _config.AddItem(
                    new MenuItem("follow.dynamic", "跟隨英雄拍攝?").SetValue(new KeyBind(17, KeyBindType.Press)));
                _config.AddItem(
                    new MenuItem("dynamicmode", "拍攝模式?").SetValue(
                        new StringList(new[] {"預設", "更隨屬標", "更隨團戰鬥"}, 2)));
                _config.AddItem(
                    new MenuItem("followoffset", "跟隨鼠標 (範圍/偏移)").SetValue(new Slider(400, 0, 700)));
                _config.AddItem(new MenuItem("CLH", "農兵").SetValue(new KeyBind('X', KeyBindType.Press)));
                _config.AddItem(new MenuItem("CLC", "清線").SetValue(new KeyBind('V', KeyBindType.Press)));
                _config.AddItem(new MenuItem("CCombo", "空格鍵 (主要)").SetValue(new KeyBind(32, KeyBindType.Press)))
                    .ValueChanged += (sender, e) =>
                {
                    if (e.GetNewValue<KeyBind>().Active == e.GetOldValue<KeyBind>().Active) return;
                    if (e.GetNewValue<KeyBind>().Active == false) CameraMovement.SemiDynamic(Player.Position);
                };
                _config.AddItem(new MenuItem("credits", "                      .:正式版本 - HS轉鏡頭:."))
                    .SetFontStyle(FontStyle.Bold, Color.DeepPink);
                _config.AddToMainMenu();
            }
            Chat.Print("<font color='#800040'>[6.17] hsCamera</font> <font color='#ff6600'>Loaded!</font>");
            Game.OnUpdate += HsCameraOnUpdate;
        }


        private static void HsCameraOnUpdate(EventArgs args)
        {
            AllModes.AllModes.CameraMode();

            for (var i = 0; i < HeroList.Count; i++)
                if (_config.Item("enemies" + HeroList[i].ChampionName).GetValue<KeyBind>().Active)
                    if (HeroList[i].IsValid && (HeroList[i] != null))
                    {
                        var position = HeroList[i].Position;
                        Camera.ScreenPosition = position.To2D();
                    }
        }
    }
}