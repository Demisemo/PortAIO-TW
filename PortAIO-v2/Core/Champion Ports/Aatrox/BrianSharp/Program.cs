using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BrianSharp.Common;
using LeagueSharp;
using LeagueSharp.Common;
using Orbwalk = BrianSharp.Common.Orbwalker;
using EloBuddy;

namespace BrianSharp.Plugin
{
    internal class Aatrox : Helper
    {
        public Aatrox()
        {
            Q = new Spell(SpellSlot.Q, 650);
            Q2 = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1075, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 550, TargetSelector.DamageType.Magical);
            Q.SetSkillshot(0.6f, 250, 2000, false, SkillshotType.SkillshotCircle);
            Q2.SetSkillshot(0.6f, 150, 2000, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 35, 1250, false, SkillshotType.SkillshotLine);

            var champMenu = new Menu("Plugin", Player.ChampionName + "_Plugin");
            {
                var comboMenu = new Menu("連招", "Combo");
                {
                    AddBool(comboMenu, "Q", "使用 Q");
                    AddBool(comboMenu, "W", "使用 W");
                    AddSlider(comboMenu, "WHpU", "切換到吸血模式血量低於 <", 50);
                    AddBool(comboMenu, "E", "使用 E");
                    AddBool(comboMenu, "R", "使用 R");
                    AddSlider(comboMenu, "RHpU", "-> 當敵人血量低於 <", 60);
                    AddSlider(comboMenu, "RCountA", "-> 敵人 >=", 2, 1, 5);
                    champMenu.AddSubMenu(comboMenu);
                }
                var harassMenu = new Menu("騷擾", "Harass");
                {
                    AddKeybind(harassMenu, "AutoE", "自動 E", "H", KeyBindType.Toggle);
                    AddSlider(harassMenu, "AutoEHpA", "-> 血量低於 >=", 50);
                    AddBool(harassMenu, "Q", "使用 Q");
                    AddSlider(harassMenu, "QHpA", "-> 血量低於 >=", 20);
                    AddBool(harassMenu, "E", "使用 E");
                    champMenu.AddSubMenu(harassMenu);
                }
                var clearMenu = new Menu("清線", "Clear");
                {
                    AddSmiteMob(clearMenu);
                    AddBool(clearMenu, "Q", "使用 Q");
                    AddBool(clearMenu, "W", "使用 W");
                    AddBool(clearMenu, "WPriority", "-> 吸血模式優先");
                    AddSlider(clearMenu, "WHpU", "-> Switch To Heal If Hp <", 50);
                    AddBool(clearMenu, "E", "使用 E");
                    AddBool(clearMenu, "Item", "使用海神斧/九頭蛇");
                    champMenu.AddSubMenu(clearMenu);
                }
                var fleeMenu = new Menu("逃跑", "Flee");
                {
                    AddBool(fleeMenu, "Q", "使用 Q");
                    AddBool(fleeMenu, "E", "使用 E 緩速敵人");
                    champMenu.AddSubMenu(fleeMenu);
                }
                var miscMenu = new Menu("雜項", "Misc");
                {
                    var killStealMenu = new Menu("搶頭", "KillSteal");
                    {
                        AddBool(killStealMenu, "Q", "使用 Q");
                        AddBool(killStealMenu, "E", "使用 E");
                        AddBool(killStealMenu, "R", "使用 R");
                        AddBool(killStealMenu, "Ignite", "使用點燃");
                        AddBool(killStealMenu, "Smite", "使用重擊");
                        miscMenu.AddSubMenu(killStealMenu);
                    }
                    var antiGapMenu = new Menu("防突進", "AntiGap");
                    {
                        AddBool(antiGapMenu, "Q", "使用 Q");
                        foreach (var spell in
                            AntiGapcloser.Spells.Where(
                                i => HeroManager.Enemies.Any(a => i.ChampionName == a.ChampionName)))
                        {
                            AddBool(
                                antiGapMenu, spell.ChampionName + "_" + spell.Slot,
                                "-> Skill " + spell.Slot + " Of " + spell.ChampionName);
                        }
                        miscMenu.AddSubMenu(antiGapMenu);
                    }
                    var interruptMenu = new Menu("中斷技能", "Interrupt");
                    {
                        AddBool(interruptMenu, "Q", "使用 Q");
                        foreach (var spell in
                            Interrupter.Spells.Where(
                                i => HeroManager.Enemies.Any(a => i.ChampionName == a.ChampionName)))
                        {
                            AddBool(
                                interruptMenu, spell.ChampionName + "_" + spell.Slot,
                                "-> Skill " + spell.Slot + " Of " + spell.ChampionName);
                        }
                        miscMenu.AddSubMenu(interruptMenu);
                    }
                    champMenu.AddSubMenu(miscMenu);
                }
                var drawMenu = new Menu("顯示", "Draw");
                {
                    AddBool(drawMenu, "Q", "顯示 Q 範圍", false);
                    AddBool(drawMenu, "E", "顯示 E 範圍", false);
                    AddBool(drawMenu, "R", "顯示 R 範圍", false);
                    champMenu.AddSubMenu(drawMenu);
                }
                MainMenu.AddSubMenu(champMenu);
            }
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
        }

        private static bool HaveWDmg
        {
            get { return Player.HasBuff("AatroxWPower"); }
        }

        private static List<AIHeroClient> GetRTarget
        {
            get
            {
                return
                    HeroManager.Enemies.Where(
                        i =>
                            i.IsValidTarget() &&
                            Player.Distance(Prediction.GetPrediction(i, 0.25f).UnitPosition) < R.Range).ToList();
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling())
            {
                return;
            }
            switch (Orbwalk.CurrentMode)
            {
                case Orbwalker.Mode.Combo:
                    Fight("Combo");
                    break;
                case Orbwalker.Mode.Harass:
                    Fight("Harass");
                    break;
                case Orbwalker.Mode.Clear:
                    Clear();
                    break;
                case Orbwalker.Mode.Flee:
                    Flee();
                    break;
            }
            if (GetValue<bool>("SmiteMob", "Auto") && Orbwalk.CurrentMode != Orbwalker.Mode.Clear)
            {
                SmiteMob();
            }
            AutoE();
            KillSteal();
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (GetValue<bool>("Draw", "Q") && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            }
            if (GetValue<bool>("Draw", "E") && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
            }
            if (GetValue<bool>("Draw", "R") && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Player.IsDead || !GetValue<bool>("AntiGap", "Q") ||
                !GetValue<bool>("AntiGap", gapcloser.Sender.ChampionName + "_" + gapcloser.Slot) || !Q.IsReady())
            {
                return;
            }
            Q2.Cast(gapcloser.Sender, PacketCast);
        }

        private static void OnPossibleToInterrupt(AIHeroClient unit, InterruptableSpell spell)
        {
            if (Player.IsDead || !GetValue<bool>("Interrupt", "Q") ||
                !GetValue<bool>("Interrupt", unit.ChampionName + "_" + spell.Slot) || !Q.IsReady())
            {
                return;
            }
            Q2.Cast(unit, PacketCast);
        }

        private static void Fight(string mode)
        {
            if (GetValue<bool>(mode, "Q") &&
                (mode == "Combo" || Player.HealthPercent >= GetValue<Slider>(mode, "QHpA").Value) &&
                Q2.CastOnBestTarget(Q2.Width / 2, PacketCast, true).IsCasted())
            {
                return;
            }
            if (GetValue<bool>(mode, "E") && E.CastOnBestTarget(0, PacketCast, true).IsCasted())
            {
                return;
            }
            if (mode != "Combo")
            {
                return;
            }
            if (GetValue<bool>(mode, "W") && W.IsReady())
            {
                if (Player.HealthPercent >= GetValue<Slider>(mode, "WHpU").Value)
                {
                    if (!HaveWDmg && W.Cast(PacketCast))
                    {
                        return;
                    }
                }
                else if (HaveWDmg && W.Cast(PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>(mode, "R") && R.IsReady() && !Player.IsDashing())
            {
                var obj = GetRTarget;
                if ((obj.Count > 1 && obj.Any(i => R.IsKillable(i))) ||
                    obj.Any(i => i.HealthPercent < GetValue<Slider>(mode, "RHpU").Value) ||
                    obj.Count >= GetValue<Slider>(mode, "RCountA").Value)
                {
                    R.Cast(PacketCast);
                }
            }
        }

        private static void Clear()
        {
            SmiteMob();
            var minionObj =
                GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth)
                    .Cast<Obj_AI_Base>()
                    .ToList();
            if (!minionObj.Any())
            {
                return;
            }
            if (GetValue<bool>("Clear", "Q") && Q.IsReady())
            {
                var pos = Q.GetCircularFarmLocation(
                    minionObj.Where(i => Q.IsInRange(i, Q.Range + Q.Width / 2)).ToList());
                if (pos.MinionsHit > 1)
                {
                    if (Q.Cast(pos.Position, PacketCast))
                    {
                        return;
                    }
                }
                else
                {
                    var obj = minionObj.FirstOrDefault(i => i.MaxHealth >= 1200);
                    if (obj != null && Q.IsInRange(obj, Q.Range + Q2.Width / 2) && Q.Cast(obj, PacketCast).IsCasted())
                    {
                        return;
                    }
                }
            }
            if (GetValue<bool>("Clear", "E") && E.IsReady())
            {
                var pos = E.GetLineFarmLocation(minionObj);
                if (pos.MinionsHit > 0 && E.Cast(pos.Position, PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>("Clear", "W") && W.IsReady())
            {
                if (Player.HealthPercent >=
                    (GetValue<bool>("Clear", "WPriority") ? 85 : GetValue<Slider>("Clear", "WHpU").Value))
                {
                    if (!HaveWDmg && W.Cast(PacketCast))
                    {
                        return;
                    }
                }
                else if (HaveWDmg && W.Cast(PacketCast))
                {
                    return;
                }
            }
            if (GetValue<bool>("Clear", "Item"))
            {
                var item = Hydra.IsReady() ? Hydra : Tiamat;
                if (item.IsReady() &&
                    (minionObj.Count(i => item.IsInRange(i)) > 2 ||
                     minionObj.Any(i => i.MaxHealth >= 1200 && i.Distance(Player) < item.Range - 80)))
                {
                    item.Cast();
                }
            }
        }

        private static void Flee()
        {
            if (GetValue<bool>("Flee", "Q") && Q.IsReady() && Q.Cast(Game.CursorPos, PacketCast))
            {
                return;
            }
            if (GetValue<bool>("Flee", "E"))
            {
                E.CastOnBestTarget(0, PacketCast, true);
            }
        }

        private static void AutoE()
        {
            if (!GetValue<KeyBind>("Harass", "AutoE").Active ||
                Player.HealthPercent < GetValue<Slider>("Harass", "AutoEHpA").Value)
            {
                return;
            }
            E.CastOnBestTarget(0, PacketCast, true);
        }

        private static void KillSteal()
        {
            if (GetValue<bool>("KillSteal", "Ignite") && Ignite.IsReady())
            {
                var target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
                if (target != null && CastIgnite(target))
                {
                    return;
                }
            }
            if (GetValue<bool>("KillSteal", "Smite") &&
                (CurrentSmiteType == SmiteType.Blue || CurrentSmiteType == SmiteType.Red))
            {
                var target = TargetSelector.GetTarget(760, TargetSelector.DamageType.True);
                if (target != null && CastSmite(target))
                {
                    return;
                }
            }
            if (GetValue<bool>("KillSteal", "Q") && Q.IsReady())
            {
                var target = Q.GetTarget(Q.Width / 2);
                if (target != null && Q.IsKillable(target) && Q2.Cast(target, PacketCast).IsCasted())
                {
                    return;
                }
            }
            if (GetValue<bool>("KillSteal", "E") && E.IsReady())
            {
                var target = E.GetTarget();
                if (target != null && E.IsKillable(target) && E.Cast(target, PacketCast).IsCasted())
                {
                    return;
                }
            }
            if (GetValue<bool>("KillSteal", "R") && R.IsReady())
            {
                var target = GetRTarget.FirstOrDefault(i => R.IsKillable(i));
                if (target != null)
                {
                    R.Cast(PacketCast);
                }
            }
        }
    }
}