// Copyright 2014 - 2014 Esk0r
// Config.cs is part of Evade.
// 
// Evade is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Evade is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Evade. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

using EloBuddy; namespace Evade
{
    internal static class Config
    {
        public const bool PrintSpellData = false;
        public const bool TestOnAllies = false;
        public const int SkillShotsExtraRadius = 9;
        public const int SkillShotsExtraRange = 20;
        public const int GridSize = 10;
        public const int ExtraEvadeDistance = 15;
        public const int PathFindingDistance = 60;
        public const int PathFindingDistance2 = 35;

        public const int DiagonalEvadePointsCount = 7;
        public const int DiagonalEvadePointsStep = 20;

        public const int CrossingTimeOffset = 250;

        public const int EvadingFirstTimeOffset = 250;
        public const int EvadingSecondTimeOffset = 80;

        public const int EvadingRouteChangeTimeOffset = 250;

        public const int EvadePointChangeInterval = 300;
        public static int LastEvadePointChangeT = 0;

        public static Menu Menu;

        public static void CreateMenu()
        {
            Menu = new Menu("躲避#", "Evade", true);

            //Create the evade spells submenus.
            var evadeSpells = new Menu("躲避技能", "evadeSpells");
            foreach (var spell in EvadeSpellDatabase.Spells)
            {
                var subMenu = new Menu(spell.Name, spell.Name);

                subMenu.AddItem(
                    new MenuItem("DangerLevel" + spell.Name, "危險級別").SetValue(
                        new Slider(spell.DangerLevel, 5, 1)));

                if (spell.IsTargetted && spell.ValidTargets.Contains(SpellValidTargets.AllyWards))
                {
                    subMenu.AddItem(new MenuItem("WardJump" + spell.Name, "過牆").SetValue(true));
                }

                subMenu.AddItem(new MenuItem("Enabled" + spell.Name, "啟用").SetValue(true));

                evadeSpells.AddSubMenu(subMenu);
            }
            Menu.AddSubMenu(evadeSpells);

            //Create the skillshots submenus.
            var skillShots = new Menu("技能", "Skillshots");

            foreach (var hero in ObjectManager.Get<AIHeroClient>())
            {
                if (hero.Team != ObjectManager.Player.Team || Config.TestOnAllies)
                {
                    foreach (var spell in SpellDatabase.Spells)
                    {
                        if (String.Equals(spell.ChampionName, hero.ChampionName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);

                            subMenu.AddItem(
                                new MenuItem("DangerLevel" + spell.MenuItemName, "危險級別").SetValue(
                                    new Slider(spell.DangerValue, 5, 1)));

                            subMenu.AddItem(
                                new MenuItem("IsDangerous" + spell.MenuItemName, "是危險的").SetValue(
                                    spell.IsDangerous));

                            subMenu.AddItem(new MenuItem("Draw" + spell.MenuItemName, "顯示").SetValue(true));
                            subMenu.AddItem(new MenuItem("Enabled" + spell.MenuItemName, "啟用").SetValue(!spell.DisabledByDefault));

                            skillShots.AddSubMenu(subMenu);
                        }
                    }
                }
            }

            Menu.AddSubMenu(skillShots);

            var shielding = new Menu("隊友保護", "Shielding");

            foreach (var ally in ObjectManager.Get<AIHeroClient>())
            {
                if (ally.IsAlly && !ally.IsMe)
                {
                    shielding.AddItem(
                        new MenuItem("shield" + ally.ChampionName, "Shield " + ally.ChampionName).SetValue(true));
                }
            }
            Menu.AddSubMenu(shielding);

            var collision = new Menu("碰撞", "Collision");
            collision.AddItem(new MenuItem("MinionCollision", "小兵碰撞").SetValue(false));
            collision.AddItem(new MenuItem("HeroCollision", "英雄碰撞").SetValue(false));
            collision.AddItem(new MenuItem("YasuoCollision", "犽宿風牆停止攻擊").SetValue(true));
            collision.AddItem(new MenuItem("EnableCollision", "啟用").SetValue(false));
            //TODO add mode.
            Menu.AddSubMenu(collision);

            var drawings = new Menu("顯示", "Drawings");
            drawings.AddItem(new MenuItem("EnabledColor", "啟用技能顏色").SetValue(Color.White));
            drawings.AddItem(new MenuItem("DisabledColor", "關閉技能顏色").SetValue(Color.Red));
            drawings.AddItem(new MenuItem("MissileColor", "彈道顏色").SetValue(Color.LimeGreen));
            drawings.AddItem(new MenuItem("Border", "邊框寬度").SetValue(new Slider(2, 5, 1)));

            drawings.AddItem(new MenuItem("EnableDrawings", "啟用").SetValue(true));
            Menu.AddSubMenu(drawings);

            var misc = new Menu("雜項", "Misc");
            misc.AddItem(new MenuItem("BlockSpells", "打斷技能，同時躲避").SetValue(new StringList(new []{"不要", "只有危險", "總是"}, 1)));
            misc.AddItem(new MenuItem("DisableFow", "關閉戰爭迷霧中躲避").SetValue(false));
            misc.AddItem(new MenuItem("ShowEvadeStatus", "顯示躲避狀態").SetValue(false));
            if (ObjectManager.Player.CharData.BaseSkinName == "Olaf")
            {
                misc.AddItem(
                    new MenuItem("DisableEvadeForOlafR", "Automatic disable Evade when Olaf's ulti is active!")
                        .SetValue(true));
            }


            Menu.AddSubMenu(misc);

            Menu.AddItem(
                new MenuItem("Enabled", "啟用").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Toggle, true))).Permashow(true, "Evade");

            Menu.AddItem(
                new MenuItem("OnlyDangerous", "躲避危險技能的熱鍵").SetValue(new KeyBind(32, KeyBindType.Press))).Permashow();

            Menu.AddToMainMenu();
        }
    }
}
