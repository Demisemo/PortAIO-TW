using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Slutty_Kindred
{
    internal class Kindred : Helper
    {
        public const string ChampName = "Kindred";
        public static Spell Q, W, E, R;

        internal static void OnLoad()
        {
            //            if (Player.ChampionName != ChampName)
            //                return;

            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 500);
            R = new Spell(SpellSlot.R, 500);

            MenuConfig.OnLoad();

            Printmsg("Majestic AF Kindred Assembly By Hoes Loaded");
            Printmsg1("Current Version: " + typeof (Program).Assembly.GetName().Version);
            Printmsg2("Don't Forget To " + "<font color='#00ff00'>[Upvote]</font> <font color='#FFFFFF'>" +
                      "The Assembly In The Database");

            Game.OnUpdate += OnUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Orbwalking.BeforeAttack += BeforeAttack;
            EloBuddy.Player.OnIssueOrder += OnIssueOrder;
            Drawing.OnDraw += OnDraw;
            CustomEvents.Unit.OnDash += Ondash;
        }

        private static void Ondash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (sender.IsMe || sender.IsAlly) return;
            var endpos = args.EndPos;
            if (endpos.Distance(Player) < 300)
            {
                Q.Cast(endpos.Extend(Player.ServerPosition.To2D(), Q.Range));
            }
        }


        private static void OnDraw(EventArgs args)
        {
            if (!GetBool("draws", typeof (bool))) return;

            if (Q.Level >= 1 && GetBool("drawq", typeof (bool)))
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Red, 4);
            }

            if (W.Level >= 1 && GetBool("draww", typeof (bool)))
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.BlueViolet, 4);
            }

            if (E.Level >= 1 && GetBool("drawe", typeof (bool)))
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.Blue, 4);
            }

            if (R.Level >= 1 && GetBool("drawr", typeof (bool)))
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Black, 4);
            }
        }

        private static void Printmsg(string message)
        {
            Chat.Print(
                "<font color='#6f00ff'>[Majestic AF Kindred]:</font> <font color='#FFFFFF'>" + message + "</font>");
        }

        private static void Printmsg1(string message)
        {
            Chat.Print(
                "<font color='#ff00ff'>[Majestic AF Kindred]:</font> <font color='#FFFFFF'>" + message + "</font>");
        }

        private static void Printmsg2(string message)
        {
            Chat.Print(
                "<font color='#00abff'>[Majestic AF Kindred]:</font> <font color='#FFFFFF'>" + message + "</font>");
        }

        private static void OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
//            if (!sender.IsMe || args.Order != GameObjectOrder.AutoAttack) return;
//
//            var siegeminion =
//    MinionManager.GetMinions(Player.Position, 800).FirstOrDefault(x => x.Name.Contains("siege"));
//
//            if (siegeminion != null && siegeminion.HealthPercent < 20)
//            {
//                foreach (
//                    var minion in
//                        MinionManager.GetMinions(Player.Position, 800))
//                {
//                    args.Process = false;
//                }
//            }
//            else if (siegeminion.HealthPercent < 20)
//            {

//            }
        }


        private static void Wallhops()
        {
            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            var behindPosition = ObjectManager.Player.Position.To2D() + 50;

            var extendedPosition = ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, 300);
            if (behindPosition.IsWall() && Game.CursorPos.IsWall())
            {
                E.Cast(extendedPosition);
            }
        }

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {


            foreach (
                var target in
                    HeroManager.Enemies.Where(
                        x =>
                            x.IsValid && x.Distance(Player) < Player.AttackRange &&
                            x.IsVisible))
            {
                if (!target.HasBuff("kindredcharge")) return;
                if (GetBool("forceetarget", typeof (bool)))
                {
                    if (target.HasBuff("kindredcharge"))
                        Orbwalker.ForceTarget(target);
                }

            }
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None) return;

            var dashPosition = Player.Position.Extend(Game.CursorPos, 350);
            if (!Q.IsReady() || !target.IsValidTarget(Player.AttackRange)) return;

            switch (GetStringValue("qmodes"))
            {
                case 0:
                    Q.Cast(dashPosition);
                    LeagueSharp.Common.Utility.DelayAction.Add(100 + Game.Ping, Orbwalking.ResetAutoAttackTimer);
                    break;
                case 1:
                {
                    Q.Cast(dashPosition);
                    LeagueSharp.Common.Utility.DelayAction.Add(100 + Game.Ping, Orbwalking.ResetAutoAttackTimer);
                }
                    break;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
//            var hero = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
//            if (hero != null)
//            {
//                foreach (var buff in hero.Buffs)
//                {
////                    if (!buff.Name.Contains("kindred")) return;
////                    if (buff.Name.Contains("refresh")) return;
////                    if (buff.Name.Contains("charge")) return;                    
//                    Chat.Print(buff.Name);
//                }
//            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    Jungleclear();
                    break;
            }

//            if (GetBool("wallhops", typeof (KeyBind)))
//            {
//                Wallhops();
//            }
        }

        private static void Jungleclear()
        {
            var minions =
                MinionManager.GetMinions(Player.Position, 800, MinionTypes.All, MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth);
            var dashPosition = Player.Position.Extend(Game.CursorPos, Q.Range);
            if (minions == null) return;
            foreach (var minion in minions)
            {
                if (W.IsReady() && GetBool("usewjungleclear", typeof (bool)))
                    W.Cast();

                if (Q.IsReady() && GetBool("useqjungleclear", typeof (bool)))
                {
                    Q.Cast(dashPosition);
                }

                if (E.IsReady() && GetBool("useejungleclear", typeof (bool)))
                    E.Cast(minion);
            }
        }

        private static void Laneclear()
        {
            if (GetValue("minmana") > Player.ManaPercent) return;

            var minions =
                MinionManager.GetMinions(Player.Position, 800, MinionTypes.All, MinionTeam.Enemy,
                    MinionOrderTypes.MaxHealth);

            if (minions == null) return;

            if (W.IsReady() &&
                GetBool("usewl", typeof (bool)) && minions.Count >= GetValue("wminslider"))
                W.Cast();

            if (Q.IsReady() &&
                GetBool("useql", typeof (bool)) && minions.Count >= GetValue("qminslider"))
                Q.Cast(Game.CursorPos);
        }

        private static void Combo()
        {


            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (target == null)
                return;

            var expires = Player.GetSpell(SpellSlot.Q).CooldownExpires; // hi idk how to use Q.IsReady(x);
            var CD = (int)
                (expires -
                 (Game.Time - 1));



            if (W.IsReady())
            {
                if (Q.IsReady() || CD > 3.5 || target.HealthPercent < 15)
                    W.Cast();
            }

            if (E.IsReady() && target.IsValidTarget(E.Range))
                E.Cast(target);

            var dashPosition = Player.Position.Extend(Game.CursorPos, 320);
            switch (GetStringValue("qmodes"))
            {
                case 0:
                {
                    if (Player.Distance(target) > Player.AttackRange && Player.Distance(target) < Q.Range && Q.IsReady())
                    {
                        Q.Cast(dashPosition);
                        if (target.Distance(Player) < Player.AttackRange)
                        {
                            EloBuddy.Player.IssueOrder(GameObjectOrder.AutoAttack, target);
                        }
                        LeagueSharp.Common.Utility.DelayAction.Add(Game.Ping, Orbwalking.ResetAutoAttackTimer);
                    }
                    break;
                }
                case 1:
                    if (Player.Distance(target) > Player.AttackRange && Player.Distance(target) < Q.Range)
                    {
                        Q.Cast(dashPosition);
                        if (target.Distance(Player) < Player.AttackRange)
                        {
                            EloBuddy.Player.IssueOrder(GameObjectOrder.AutoAttack, target);
                        }
                        LeagueSharp.Common.Utility.DelayAction.Add(Game.Ping, Orbwalking.ResetAutoAttackTimer);
                    }
                    break;
            }
//            

            if (!R.IsReady()) return;
            foreach (var hero in HeroManager.Allies.Where(x => x.IsValid && x.IsVisible && x.Distance(Player) < R.Range
                                                               &&
                                                               x.CountEnemiesInRange(R.Range) <=
                                                               GetValue("minenemies") &&
                                                               x.HealthPercent < GetValue("minhpr"))
                )
            {
                if (!target.IsFacing(hero)) return;
                if (target.Distance(hero) > 550) return;
                if (GetBool("useron" + hero.CharData.BaseSkinName, typeof (bool)))
                    R.Cast(hero);
            }

        }
    }
}
