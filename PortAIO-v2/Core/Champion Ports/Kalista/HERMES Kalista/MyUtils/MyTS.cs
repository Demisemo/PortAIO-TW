#region LICENSE

/*
 Copyright 2014 - 2014 LeagueSharp
 TargetSelector.cs is part of LeagueSharp.Common.
 
 LeagueSharp.Common is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 LeagueSharp.Common is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with LeagueSharp.Common. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

#region

using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

using EloBuddy; 
using LeagueSharp.Common; 
namespace HERMES_Kalista.MyUtils
{
    public class TargetSelector
    {
        #region Main

        static TargetSelector()
        {
            Game.OnWndProc += GameOnOnWndProc;
            Drawing.OnDraw += DrawingOnOnDraw;
        }

        #endregion

        #region Enum

        public enum TargetingMode
        {
            AutoPriority,
            LowHP,
            Closest,
            LeastAttacks,
            HighestPriority,
        }

        #endregion

        #region Vars

        public static TargetingMode Mode = TargetingMode.AutoPriority;
        private static Menu _configMenu;
        private static AIHeroClient _selectedTargetObjAiHero;

        #endregion

        #region EventArgs

        private static void DrawingOnOnDraw(EventArgs args)
        {
            if (_selectedTargetObjAiHero.IsValidTarget() && _configMenu != null &&
                _configMenu.Item("FocusSelected").GetValue<bool>() &&
                _configMenu.Item("SelTColor").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(
                    _selectedTargetObjAiHero.Position, 150, _configMenu.Item("SelTColor").GetValue<Circle>().Color, 7,
                    true);
            }
        }

        private static void GameOnOnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }
            _selectedTargetObjAiHero =
                HeroManager.Enemies
                    .FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000) // 200 * 200
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();
        }

        #endregion

        #region Functions

        public static AIHeroClient SelectedTarget
        {
            get
            {
                return (_configMenu != null && _configMenu.Item("FocusSelected").GetValue<bool>()
                    ? _selectedTargetObjAiHero
                    : null);
            }
        }

        /// <summary>
        ///     Sets the priority of the hero
        /// </summary>
        public static void SetPriority(AIHeroClient hero, int newPriority)
        {
            if (_configMenu == null || _configMenu.Item("TargetSelector" + hero.ChampionName + "Priority") == null)
            {
                return;
            }
            var p = _configMenu.Item("TargetSelector" + hero.ChampionName + "Priority").GetValue<Slider>();
            p.Value = Math.Max(1, Math.Min(5, newPriority));
            _configMenu.Item("TargetSelector" + hero.ChampionName + "Priority").SetValue(p);
        }

        /// <summary>
        ///     Returns the priority of the hero
        /// </summary>
        public static float GetPriority(AIHeroClient hero)
        {
            var p = 1;
            if (_configMenu != null && _configMenu.Item("TargetSelector" + hero.ChampionName + "Priority") != null)
            {
                p = _configMenu.Item("TargetSelector" + hero.ChampionName + "Priority").GetValue<Slider>().Value;
            }

            switch (p)
            {
                case 2:
                    return 1.5f;
                case 3:
                    return 1.75f;
                case 4:
                    return 2f;
                case 5:
                    return 2.5f;
                default:
                    return 1f;
            }
        }

        private static int GetPriorityFromDb(string championName)
        {
            string[] p1 =
            {
                "Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen", "Gnar",
                "Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus", "Nunu",
                "Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "Sona", 
                "Taric", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick", "Zac", "Zyra"
            };

            string[] p2 =
            {
                "Aatrox", "Darius", "Elise", "Evelynn", "Galio", "Gangplank", "Gragas", "Irelia", "Jax",
                "Lee Sin", "Maokai", "Morgana", "Nocturne", "Pantheon", "Poppy", "Rengar", "Rumble", "Ryze", "Swain",
                "Trundle", "Tryndamere", "Udyr", "Urgot", "Vi", "XinZhao", "RekSai"
            };

            string[] p3 =
            {
                "Akali", "Diana", "Ekko", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce", "Kassadin",
                "Kayle", "Kha'Zix", "Lissandra", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Vladimir", "Yasuo",
                "Zilean"
            };

            string[] p4 =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
                "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Soraka", "Syndra", "Talon",
                "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
                "Zed", "Ziggs"
            };

            if (p1.Contains(championName))
            {
                return 1;
            }
            if (p2.Contains(championName))
            {
                return 2;
            }
            if (p3.Contains(championName))
            {
                return 3;
            }
            return p4.Contains(championName) ? 4 : 1;
        }

        public static void AddToMenu(Menu config)
        {
            _configMenu = config;
            config.AddItem(new MenuItem("FocusSelected", "Focus selected target").SetShared().SetValue(true));
            config.AddItem(
                new MenuItem("ForceFocusSelected", "Only attack selected target").SetShared().SetValue(false));
            config.AddItem(
                new MenuItem("SelTColor", "Selected target color").SetShared().SetValue(new Circle(true, Color.Red)));
            config.AddItem(new MenuItem("Sep", "").SetShared());
            var autoPriorityItem = new MenuItem("AutoPriority", "Auto arrange priorities").SetShared().SetValue(true);
            autoPriorityItem.ValueChanged += autoPriorityItem_ValueChanged;

            foreach (var enemy in HeroManager.Enemies)
            {
                config.AddItem(
                    new MenuItem("TargetSelector" + enemy.ChampionName + "Priority", enemy.ChampionName).SetShared()
                        .SetValue(
                            new Slider(
                                autoPriorityItem.GetValue<bool>() ? GetPriorityFromDb(enemy.ChampionName) : 1, 5, 1)));
                if (autoPriorityItem.GetValue<bool>())
                {
                    config.Item("TargetSelector" + enemy.ChampionName + "Priority")
                        .SetValue(
                            new Slider(
                                autoPriorityItem.GetValue<bool>() ? GetPriorityFromDb(enemy.ChampionName) : 1, 5, 1));
                }
            }
            config.AddItem(autoPriorityItem);
            config.AddItem(
                new MenuItem("TargetingMode", "Target Mode").SetShared()
                    .SetValue(new StringList(Enum.GetNames(typeof(TargetingMode)))));
        }

        private static void autoPriorityItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (!e.GetNewValue<bool>())
            {
                return;
            }
            foreach (var enemy in HeroManager.Enemies)
            {
                _configMenu.Item("TargetSelector" + enemy.ChampionName + "Priority")
                    .SetValue(new Slider(GetPriorityFromDb(enemy.ChampionName), 5, 1));
            }
        }

        public static bool IsInvulnerable(Obj_AI_Base target)
        {
            // Tryndamere's Undying Rage (R)
            if (target.HasBuff("Undying Rage") && target.Health <= 2f)
            {
                return true;
            }

            // Kayle's Intervention (R)
            if (target.HasBuff("JudicatorIntervention"))
            {
                return true;
            }
            return false;
        }


        public static void SetTarget(AIHeroClient hero)
        {
            if (hero.IsValidTarget())
            {
                _selectedTargetObjAiHero = hero;
            }
        }

        public static AIHeroClient GetSelectedTarget()
        {
            return SelectedTarget;
        }

        public static AIHeroClient GetTarget(float range,
            IEnumerable<AIHeroClient> ignoredChamps = null,
            Vector3? rangeCheckFrom = null)
        {
            return GetTarget(ObjectManager.Player, range, ignoredChamps, rangeCheckFrom);
        }

        private static bool IsValidTarget(Obj_AI_Base target,
            float range,
            Vector3? rangeCheckFrom = null)
        {
            return target.IsValidTarget() &&
                   target.Distance(rangeCheckFrom ?? ObjectManager.Player.ServerPosition, true) <
                   Math.Pow(range <= 0 ? Orbwalking.GetRealAutoAttackRange(target) : range, 2) &&
                   !IsInvulnerable(target);
        }

        public static AIHeroClient GetTarget(Obj_AI_Base champion,
            float range,
            IEnumerable<AIHeroClient> ignoredChamps = null,
            Vector3? rangeCheckFrom = null)
        {
            try
            {
                if (ignoredChamps == null)
                {
                    ignoredChamps = new List<AIHeroClient>();
                }

                if (_configMenu != null && IsValidTarget(
                    SelectedTarget, _configMenu.Item("ForceFocusSelected").GetValue<bool>() ? float.MaxValue : range,
                    rangeCheckFrom))
                {
                    return SelectedTarget;
                }

                if (_configMenu != null && _configMenu.Item("TargetingMode") != null &&
                    Mode == TargetingMode.AutoPriority)
                {
                    var menuItem = _configMenu.Item("TargetingMode").GetValue<StringList>();
                    Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out Mode);
                }

                var targets =
                    HeroManager.Enemies
                        .FindAll(
                            hero => !IsInvulnerable(hero) &&
                                    ignoredChamps.All(ignored => ignored.NetworkId != hero.NetworkId) &&
                                    IsValidTarget(hero, range, rangeCheckFrom));

                switch (Mode)
                {
                    case TargetingMode.LowHP:
                        return targets.MinOrDefault(hero => hero.Health);
                    case TargetingMode.Closest:
                        return
                            targets.MinOrDefault(
                                hero =>
                                    (rangeCheckFrom.HasValue ? rangeCheckFrom.Value : champion.ServerPosition).Distance(
                                        hero.ServerPosition, true));
                    case TargetingMode.AutoPriority:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                    champion.CalcDamage(hero, Damage.DamageType.Physical, 100) / (1 + hero.Health) *
                                    GetPriority(hero));
                    case TargetingMode.LeastAttacks:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                    champion.CalcDamage(hero, Damage.DamageType.Physical, 100) / (1 + hero.Health) *
                                    GetPriority(hero));
                    case TargetingMode.HighestPriority:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                    GetPriority(hero));
                    default:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                    champion.CalcDamage(hero, Damage.DamageType.Physical, 100) / (1 + hero.Health) *
                                    GetPriority(hero));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        #endregion
    }
}