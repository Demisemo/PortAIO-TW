using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SebbyLib;

using EloBuddy; 
using LeagueSharp.Common; 
namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Urgot : Base
    {
        private double OverFarm = 0, lag = 0;
        private int FarmId;
        private int Muramana = 3042, Tear = 3070, Manamune = 3004;

        public Urgot()
        {
            Q = new Spell(SpellSlot.Q, 980);
            Q1 = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 890);
            R = new Spell(SpellSlot.R, 850);

            Q.SetSkillshot(0.25f, 60f, 1600f, true, SkillshotType.SkillshotLine);
            Q1.SetSkillshot(0.25f, 60f, 1600f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 160f, 1500f, false, SkillshotType.SkillshotCircle);
            LoadMenuOKTW();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            //Orbwalking.AfterAttack += afterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnInterruptableSpell;
        }

        private void LoadMenuOKTW()
        {
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw only ready spells", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));

            Config.SubMenu("Items").AddItem(new MenuItem("mura", "Auto Muramana", true).SetValue(true));
            Config.SubMenu("Items").AddItem(new MenuItem("stack", "Stack Tear if full mana", true).SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("autoW", "Auto W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("Waa", "Auto W befor AA", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("AGC", "AntiGapcloserW", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("W Config").AddItem(new MenuItem("Wdmg", "W dmg % hp", true).SetValue(new Slider(10, 100, 0)));

            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("autoE", "Auto E", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("E Config").AddItem(new MenuItem("harassE", "E harass", true).SetValue(true));

            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("autoR", "Auto R under turrent", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("inter", "OnPossibleToInterrupt R", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("Rhp", "dont R if under % hp", true).SetValue(new Slider(50, 100, 0)));
            Config.SubMenu(Player.ChampionName).SubMenu("R Config").AddItem(new MenuItem("useR", "Semi-manual cast R key", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu(Player.ChampionName).SubMenu("R Config").SubMenu("GapCloser R").AddItem(new MenuItem("GapCloser" + enemy.ChampionName, enemy.ChampionName, true).SetValue(false));

            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("HarassMana", "Harass Mana", true).SetValue(new Slider(30, 100, 0)));
            Config.SubMenu(Player.ChampionName).AddItem(new MenuItem("stack", "Stack Tear if full mana", true).SetValue(false));

            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Farm Q", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("LC", "LaneClear", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("LCP", "FAST LaneClear", true).SetValue(true));
        }

        private void OnInterruptableSpell(AIHeroClient unit, InterruptableSpell spell)
        {
            if (Config.Item("inter", true).GetValue<bool>() && R.IsReady() && unit.IsValidTarget(R.Range))
                R.Cast(unit);
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {

            if (R.IsReady())
            {
                var t = gapcloser.Sender;
                if (Config.Item("GapCloser" + t.ChampionName).GetValue<bool>() && t.IsValidTarget(R.Range))
                {
                    R.Cast(t);
                }
            }
            if (Config.Item("AGC", true).GetValue<bool>() && W.IsReady() && Player.Mana > RMANA + WMANA)
            {
                var Target = (AIHeroClient)gapcloser.Sender;
                if (Target.IsValidTarget(E.Range))
                    W.Cast();
            }
        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (FarmId != args.Target.NetworkId)
                FarmId = args.Target.NetworkId;
            if (W.IsReady() && Config.Item("Waa", true).GetValue<bool>() && args.Target.IsValid<AIHeroClient>() && Player.Mana > WMANA + QMANA * 4)
                W.Cast();

            if (Config.Item("mura", true).GetValue<bool>())
            {
                int Mur = Items.HasItem(Muramana) ? 3042 : 3043;
                if (!Player.HasBuff("Muramana") && args.Target.IsEnemy && args.Target.IsValid<AIHeroClient>() && Items.HasItem(Mur) && Items.CanUseItem(Mur) && Player.Mana > RMANA + EMANA + QMANA + WMANA)
                    Items.UseItem(Mur);
                else if (Player.HasBuff("Muramana") && Items.HasItem(Mur) && Items.CanUseItem(Mur))
                    Items.UseItem(Mur);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsRecalling())
                return;
            if (Config.Item("useR", true).GetValue<KeyBind>().Active )
            {
                var tr = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (tr.IsValidTarget())
                    R.Cast(tr);
            }

            if (Program.LagFree(0))
            {
                SetMana();
            }

            if (Program.LagFree(1) && E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();

            if (Program.LagFree(2) && W.IsReady() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();

            if (!ObjectManager.Player.Spellbook.IsAutoAttacking && Q.IsReady())
            {
                LogicQ();
            }

            if (Program.LagFree(4) && !ObjectManager.Player.Spellbook.IsAutoAttacking && R.IsReady())
                LogicR();
        }

        private void LogicW()
        {
            if (Player.Mana > RMANA + WMANA)
            {
                double dmg = OktwCommon.GetIncomingDamage(Player);
                double shieldValue = 20 + W.Level * 40 + 0.08 * Player.MaxMana + 0.8 * Player.FlatMagicDamageMod;
                double HpPercentage = (dmg * 100) / Player.Health;

                if (dmg > shieldValue)
                    W.Cast();
                else if (HpPercentage >= Config.Item("Wdmg", true).GetValue<Slider>().Value)
                    W.Cast();
                else if (Player.Health - dmg <  Player.Level * 10)
                    W.Cast();
            }
        }

        private void LogicQ2()
        {
            if (Program.Farm && Config.Item("farmQ", true).GetValue<bool>())
                farmQ();
            else if (Config.Item("stack", true).GetValue<bool>() && Utils.TickCount - Q.LastCastAttemptT > 4000 && !Player.HasBuff("Recall") && Player.Mana > Player.MaxMana * 0.95 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None && (Items.HasItem(Tear) || Items.HasItem(Manamune)))
                Q.Cast(Player.ServerPosition);
        }

        private void LogicQ()
        {
            if (!Program.None)
            {
                var eTarget = HeroManager.Enemies.OrderBy(x => x.Health).FirstOrDefault(x => x.IsValidTarget(Q1.Range) && x.HasBuff("urgotcorrosivedebuff"));

                if(eTarget != null)
                {
                    Q1.Cast(eTarget.ServerPosition);
                    if (W.IsReady() && (Player.Mana > WMANA + QMANA * 4 || Q.GetDamage(eTarget) * 3 > eTarget.Health) &&  Config.Item("autoW", true).GetValue<bool>())
                    {
                        W.Cast();
                        Program.debug("W");
                    }
                    return;
                }
            }

            if (Program.LagFree(1))
            {
                if (!Orbwalking.CanMove(50))
                    return;
                bool cc = !Program.None && Player.Mana > RMANA + QMANA + EMANA;
                bool harass = Program.Harass && Player.ManaPercent > Config.Item("HarassMana", true).GetValue<Slider>().Value && OktwCommon.CanHarras();

                if (Program.Combo && Player.Mana > RMANA + QMANA)
                {
                    var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

                    if (t.IsValidTarget())
                        Program.CastSpell(Q, t);
                }

                foreach (var t in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range)).OrderBy(t => t.Health))
                {
                    var qDmg = OktwCommon.GetKsDamage(t, Q);

                    if (qDmg * 2 > t.Health)
                    {
                        Program.CastSpell(Q, t);
                        return;
                    }

                    if (cc && !OktwCommon.CanMove(t))
                        Q.Cast(t);

                    if (harass && Config.Item("Harass" + t.ChampionName).GetValue<bool>())
                        Program.CastSpell(Q, t);
                }
            }
            else if (Program.LagFree(2))
            {
                if (Harass && Player.Mana > QMANA)
                {
                    LogicQ2();
                }
                else if (Config.Item("stack", true).GetValue<bool>() && Utils.TickCount - Q.LastCastAttemptT > 4000 && !Player.HasBuff("Recall") && Player.Mana > Player.MaxMana * 0.95 && Program.None && (Items.HasItem(Tear) || Items.HasItem(Manamune)))
                {
                    Q.Cast(Player.Position.Extend(Game.CursorPos, 500));
                }
            }
        }

        private void LogicR()
        {
            R.Range = 400 + 150 * R.Level;
            if (Player.UnderTurret(false) && !ObjectManager.Player.UnderTurret(true) && Player.HealthPercent >= Config.Item("Rhp", true).GetValue<Slider>().Value && Config.Item("autoR", true).GetValue<bool>())
            {
                foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range) && OktwCommon.ValidUlt(target)))
                {
                    if ( target.CountEnemiesInRange(700) < 2 + Player.CountAlliesInRange(700))
                    {
                        R.Cast(target);
                    }
                }
            }
        }

        private void LogicE()
        {
            var qCd = Q.Instance.CooldownExpires - Game.Time;

            var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (t.IsValidTarget())
            {
                var qDmg = Q.GetDamage(t);
                var eDmg = E.GetDamage(t);
                if (eDmg > t.Health)
                    E.Cast(t);
                else if (eDmg + qDmg > t.Health && Player.Mana > EMANA + QMANA)
                    Program.CastSpell(E, t);
                else if (eDmg + 3 * qDmg > t.Health && Player.Mana > EMANA + QMANA * 3)
                    Program.CastSpell(E, t);
                else if (Program.Combo && Player.Mana > EMANA + QMANA * 2 && qCd < 0.5f)
                    Program.CastSpell(E, t);
                else if (Program.Harass && Player.Mana > RMANA + EMANA + QMANA * 5  && Config.Item("harassE", true).GetValue<bool>() && Config.Item("Harass" + t.ChampionName).GetValue<bool>())
                    Program.CastSpell(E, t);
                else if (!Program.None && Player.Mana > RMANA + WMANA + EMANA)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !OktwCommon.CanMove(enemy)))
                        E.Cast(enemy, true, true);
                }
            }
        }

        public void farmQ()
        {
            if (Program.LaneClear)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 800, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    Q.Cast(mob, true);
                    return;
                }
            }

            if (!Config.Item("farmQ", true).GetValue<bool>())
                return;

            var minions = Cache.GetMinions(Player.ServerPosition, Q.Range);

            int orbTarget = 0;
            if (Orbwalker.GetTarget() != null)
                orbTarget = Orbwalker.GetTarget().NetworkId;

            foreach (var minion in minions.Where(minion => orbTarget != minion.NetworkId && !Orbwalker.InAutoAttackRange(minion) && minion.Health < Q.GetDamage(minion)))
            {
                if (Q.Cast(minion) == Spell.CastStates.SuccessfullyCasted)
                    return;
            }

            if (Config.Item("LC", true).GetValue<bool>() && Program.LaneClear && !Orbwalking.CanAttack() && FarmSpells)
            {
                var LCP = Config.Item("LCP", true).GetValue<bool>();

                foreach (var minion in minions.Where(minion => Orbwalker.InAutoAttackRange(minion) && orbTarget != minion.NetworkId))
                {
                    var hpPred = SebbyLib.HealthPrediction.GetHealthPrediction(minion, 300);
                    var dmgMinion = minion.GetAutoAttackDamage(minion);
                    var qDmg = Q.GetDamage(minion);
                    if (hpPred < qDmg)
                    {
                        if (hpPred > dmgMinion)
                        {
                            if (Q.Cast(minion) == Spell.CastStates.SuccessfullyCasted)
                                return;
                        }
                    }
                    else if (LCP)
                    {
                        if (hpPred > dmgMinion + qDmg)
                        {
                            if (Q.Cast(minion) == Spell.CastStates.SuccessfullyCasted)
                                return;
                        }
                    }
                }
            }
        }

        private void SetMana()
        {
            if ((Config.Item("manaDisable", true).GetValue<bool>() && Program.Combo) || Player.HealthPercent < 20)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.SData.Mana;
            WMANA = W.Instance.SData.Mana;
            EMANA = E.Instance.SData.Mana;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.SData.Mana;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (Q.IsReady())
                        LeagueSharp.Common.Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    LeagueSharp.Common.Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (E.IsReady())
                        LeagueSharp.Common.Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
                }
                else
                    LeagueSharp.Common.Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }
            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>())
                {
                    if (R.IsReady())
                        LeagueSharp.Common.Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
                }
                else
                    LeagueSharp.Common.Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }
    }
}
