using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;

using EloBuddy;
using LeagueSharp.Common;
namespace badaoTalon
{
    class Program
    {
        private static AIHeroClient Player { get { return ObjectManager.Player; } }

        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W,/* E,*/ R;

        private static Menu Menu;

        private static float l;

        public static void Main()
        {
            Game_OnGameLoad();
        }

        static void Game_OnGameLoad()
        {
            if (Player.ChampionName != "Talon")
                return;

            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 900);
            //E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 500);
            W.SetSkillshot(0.25f, 75, 2300, false, SkillshotType.SkillshotLine);


            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            Menu orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);
            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            Menu spellMenu = Menu.AddSubMenu(new Menu("Spells", "Spells"));
            spellMenu.AddItem(new MenuItem("Use W Harass", "Use W Harass").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use W Combo", "Use W Combo").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use R Combo", "Use R Combo").SetValue(true));
            spellMenu.AddItem(new MenuItem("force focus selected", "force focus selected").SetValue(false));
            spellMenu.AddItem(new MenuItem("if selected in :", "if selected in :").SetValue(new Slider(1000, 1000, 1500)));
            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += oncast;
            Chat.Print("Welcome to TalonWorld");
        }
        public static void oncast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
                return;
            if (spell.Name.Contains("ItemTiamatCleave"))
            {
                Chat.Say("/l");
                if (Q.IsReady())
                {
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    {
                        l = 1;
                    }
                }
            }
            if (spell.Name.Contains("TalonNoxianDiplomacy"))
            {
                l = 0;
                LeagueSharp.Common.Utility.DelayAction.Add(30, () => Orbwalking.ResetAutoAttackTimer());
            }
        }
        public static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (HasItem())
                {
                    CastItem();
                }
                else
                {
                    if (target.IsValidTarget(165))
                        Q.Cast(target as Obj_AI_Base);
                }
            }
        }
        public static void Qitem()
        {
            if (l == 1)
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                    Q.Cast(target);
            }
        }
        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            Qitem();
            if (Selected() == true && !Orbwalker.InAutoAttackRange(TargetSelector.GetSelectedTarget()) && !Player.Spellbook.IsAutoAttacking)
            {
                Orbwalker.SetAttack(false);
            }
            else
            {
                Orbwalker.SetAttack(true);
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (Menu.Item("Use W Combo").GetValue<bool>())
                {
                    useW();
                }
                if (Menu.Item("Use R Combo").GetValue<bool>())
                {
                    useR();
                }

            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (Menu.Item("Use W Harass").GetValue<bool>())
                {
                    useW();
                }
            }
        }

        public static bool Selected()
        {
            if (!Menu.Item("force focus selected").GetValue<bool>())
            {
                return false;
            }
            else
            {
                var target = TargetSelector.GetSelectedTarget();
                float a = Menu.Item("if selected in :").GetValue<Slider>().Value;
                if (target == null || target.IsDead || target.IsZombie)
                {
                    return false;
                }
                else
                {
                    if (Player.Distance(target.Position) > a)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }

        public static Obj_AI_Base gettarget(float range)
        {
            if (Selected())
            {
                return TargetSelector.GetSelectedTarget();
            }
            else
            {
                return TargetSelector.GetTarget(range, TargetSelector.DamageType.Physical);
            }
        }

        public static void useW()
        {
            var target = gettarget(W.Range);
            if (Orbwalking.InAutoAttackRange(target) && Orbwalking.CanAttack())
                return;
            if (Orbwalking.InAutoAttackRange(target) && !Orbwalking.CanAttack() && Q.IsReady())
                return;
            if (Orbwalking.InAutoAttackRange(target) && !Orbwalking.CanAttack() && Player.HasBuff("talonnoxiandiplomacybuff"))
                return;
            if (target != null && target.IsValidTarget() && !target.IsZombie && W.IsReady())
            {
                var t = Prediction.GetPrediction(target, 200).CastPosition;
                float y = target.MoveSpeed * 250 / 1000;
                var m = Player.Distance(target.Position);
                float n = target.MoveSpeed * (250 / 1000 + m / 2300);
                // moving target
                bool moving;
                if (target.Distance(t) < y)
                {
                    moving = false;
                }
                else
                {
                    moving = true;
                }
                // position after moving
                Vector3 x;
                if (moving == false)
                {
                    x = target.Position;
                }
                else
                {
                    x = target.Position.Extend(t, n - 50);
                }
                // check range to cast 
                if (Player.Distance(x) <= 600)
                {
                    W.Cast(x);
                }
            }
        }
        public static void useR()
        {
            var target = gettarget(R.Range);
            if (target != null && target.IsValidTarget() && !target.IsZombie && R.IsReady() && R.Instance.Name == "TalonR")
            {
                if (Orbwalking.InAutoAttackRange(target) && !Orbwalking.CanAttack() && !Q.IsReady())
                {
                    var x = Player.Position;
                    R.Cast(x);
                    LeagueSharp.Common.Utility.DelayAction.Add(40, () => R.Cast(x));
                }
                if (!Orbwalking.InAutoAttackRange(target) && Player.Distance(target.Position) <= 450)
                {
                    var x = Player.Position;
                    R.Cast(x);
                    LeagueSharp.Common.Utility.DelayAction.Add(40, () => R.Cast(x));
                }
            }
        }
        public static bool HasItem()
        {
            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady() || ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady()
                || ItemData.Youmuus_Ghostblade.GetItem().IsReady())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void CastItem()
        {
            if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                ItemData.Youmuus_Ghostblade.GetItem().Cast();
            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                ItemData.Tiamat_Melee_Only.GetItem().Cast();
            if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
        }
    }
}
