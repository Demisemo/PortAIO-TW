﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SCommon;
using SCommon.Database;
using SCommon.PluginBase;
using SPrediction;
using SCommon.Orbwalking;

using SharpDX;
//typedefs
using TargetSelector = SCommon.TS.TargetSelector;
using EloBuddy;

namespace SAutoCarry.Champions
{
    public class Aatrox : SCommon.PluginBase.Champion
    {
        public Aatrox()
            : base("Aatrox", "SAutoCarry - Aatrox")
        {
            OnUpdate += BeforeOrbwalk;
            OnCombo += Combo;
            OnHarass += Harass;
            OnLaneClear += LaneClear;
        }

        public override void CreateConfigMenu()
        {
            Menu combo = new Menu("連招", "SAutoCarry.Aatrox.Combo");
            combo.AddItem(new MenuItem("SAutoCarry.Aatrox.Combo.UseQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("SAutoCarry.Aatrox.Combo.UseW", "使用 W").SetValue(true));
            combo.AddItem(new MenuItem("SAutoCarry.Aatrox.Combo.UseE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("SAutoCarry.Aatrox.Combo.UseR", "使用 R").SetValue(true));
            combo.AddItem(new MenuItem("SAutoCarry.Aatrox.Combo.UseTiamat", "使用海神斧/九頭蛇").SetValue(true));

            Menu harass = new Menu("騷擾", "SAutoCarry.Aatrox.Harass");
            harass.AddItem(new MenuItem("SAutoCarry.Aatrox.Harass.UseQ", "使用 Q").SetValue(true));
            harass.AddItem(new MenuItem("SAutoCarry.Aatrox.Harass.UseW", "使用 W").SetValue(true));
            harass.AddItem(new MenuItem("SAutoCarry.Aatrox.Harass.UseE", "使用 E").SetValue(true));
            harass.AddItem(new MenuItem("SAutoCarry.Aatrox.Harass.MinMana", "血量百分比").SetValue(new Slider(30, 100, 0)));

            Menu laneclear = new Menu("清線/清野", "SAutoCarry.Aatrox.LaneClear");
            laneclear.AddItem(new MenuItem("SAutoCarry.Aatrox.LaneClear.UseQ", "使用 Q").SetValue(true));
            laneclear.AddItem(new MenuItem("SAutoCarry.Aatrox.LaneClear.UseW", "使用 W").SetValue(true));
            laneclear.AddItem(new MenuItem("SAutoCarry.Aatrox.LaneClear.MinMana", "血量百分比").SetValue(new Slider(50, 100, 0)));

            Menu misc = new Menu("雜項", "SAutoCarry.Aatrox.Misc");
            misc.AddItem(new MenuItem("SAutoCarry.Aatrox.Misc.AutoQHP", "自動騷擾血量於%使用Q").SetValue(new Slider(50, 100, 0)));
            misc.AddItem(new MenuItem("SAutoCarry.Aatrox.Misc.RKillSteal", "使用R搶頭").SetValue(true));

            ConfigMenu.AddSubMenu(combo);
            ConfigMenu.AddSubMenu(harass);
            ConfigMenu.AddSubMenu(laneclear);
            ConfigMenu.AddSubMenu(misc);

            ConfigMenu.AddToMainMenu();
        }

        public override void SetSpells()
        {
            Spells[Q] = new Spell(SpellSlot.Q, 425f);

            Spells[W] = new Spell(SpellSlot.W, 200f);

            Spells[E] = new Spell(SpellSlot.E, 550f);
            Spells[E].SetSkillshot(0.20f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);

            Spells[R] = new Spell(SpellSlot.R, 460f);

            // Darius Copy, Aatrox Base
        }

        public void BeforeOrbwalk()
        {
            // Insert code
        }

        public void Combo()
        {
            // Combo Q & W & E & R
        }

        public void Harass()
        {
            // Harass Q & E
        }

        public void LaneClear()
        {
            // LaneClear Q & E
        }

        public void KillSteal()
        {
            // Ks with Q & E
        }

        protected override void OrbwalkingEvents_AfterAttack(SCommon.Orbwalking.AfterAttackArgs args)
        {
            if (Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Mixed)
            {
                if (Spells[W].IsReady() && HarassUseW)
                {
                    Spells[W].Cast();
                    return;
                }
            }

            if (Orbwalker.ActiveMode == SCommon.Orbwalking.Orbwalker.Mode.Combo)
            {
                if (ComboUseTiamat)
                {
                    if (Items.HasItem(3077) && Items.CanUseItem(3077))
                        Items.UseItem(3077);
                    else if (Items.HasItem(3074) && Items.CanUseItem(3074))
                        Items.UseItem(3074);
                    else if (Items.HasItem(3748) && Items.CanUseItem(3748))
                        Items.UseItem(3748);

                    return;
                }
            }
        }

        public bool ComboUseQ
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Combo.UseQ").GetValue<bool>(); }
        }

        public bool ComboUseE
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Combo.UseE").GetValue<bool>(); }
        }

        public bool ComboUseW
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Combo.UseW").GetValue<bool>(); }
        }

        public bool ComboUseR
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Combo.UseR").GetValue<bool>(); }
        }

        public bool ComboUseTiamat
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Combo.UseTiamat").GetValue<bool>(); }
        }

        public bool HarassUseQ
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Harass.UseQ").GetValue<bool>(); }
        }

        public bool HarassUseW
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Harass.UseW").GetValue<bool>(); }
        }

        public int AutoQHP
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Misc.AutoQHP").GetValue<Slider>().Value; }
        }

        public bool HarassUseE
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Harass.UseE").GetValue<bool>(); }
        }

        public bool HarassUseR
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Harass.UseR").GetValue<bool>(); }
        }

        public int HarassRStack
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Harass.RStacks").GetValue<Slider>().Value; }
        }

        public int HarassMinMana
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Harass.MinMana").GetValue<Slider>().Value; }
        }

        public bool LaneClearQ
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.LaneClear.UseQ").GetValue<bool>(); }
        }

        public bool LaneClearW
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.LaneClear.UseW").GetValue<bool>(); }
        }

        public int LaneClearMinMana
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.LaneClear.MinMana").GetValue<Slider>().Value; }
        }

        public bool KillStealR
        {
            get { return ConfigMenu.Item("SAutoCarry.Aatrox.Misc.RKillSteal").GetValue<bool>(); }
        }
    }
}