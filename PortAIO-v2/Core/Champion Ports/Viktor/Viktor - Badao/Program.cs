using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using ItemData = LeagueSharp.Common.Data.ItemData;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace ViktorBadao
{
    static class Program
    {
        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell _q, _w, _e, _r;
        private static Menu _menu;
        private static GameObject ViktorR = null;

        public static void Game_OnGameLoad()
        {
            if (Player.ChampionName != "Viktor")
                return;

            _q = new Spell(SpellSlot.Q , 650);
            _w = new Spell(SpellSlot.W,700);
            _e = new Spell(SpellSlot.E,700);
            _r = new Spell(SpellSlot.R,700);
            _r.SetSkillshot(0.25f, 325,float.MaxValue,false,SkillshotType.SkillshotCircle);
            _w.SetSkillshot(0.25f, 325, float.MaxValue, false, SkillshotType.SkillshotCircle);
            _e.SetSkillshot(0.25f, 80, 1050, false, SkillshotType.SkillshotLine);
            _e.MinHitChance = HitChance.Medium;
            //R = new Spells(SpellSlot.R, SkillshotType.SkillshotCircle, 700, 0.25f, 325 / 2, false);

            _menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            Menu orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);
            Menu ts = _menu.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            Menu spellMenu = _menu.AddSubMenu(new Menu("Spells", "Spells"));
            Menu Harass = spellMenu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu Combo = spellMenu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu Focus = spellMenu.AddSubMenu(new Menu("Focus Selected", "Focus Selected"));
            Menu KS = spellMenu.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            Harass.AddItem(new MenuItem("Use Q Harass", "Use Q Harass").SetValue(true));
            Harass.AddItem(new MenuItem("Use E Harass", "Use E Harass").SetValue(true));
            Combo.AddItem(new MenuItem("Use Q Combo", "Use Q Combo").SetValue(true));
            Combo.AddItem(new MenuItem("Use E Combo", "Use E Combo").SetValue(true));
            Combo.AddItem(new MenuItem("Use W Combo", "Use W Combo").SetValue(true));
            Combo.AddItem(new MenuItem("Use R Burst Selected", "Use R Combo").SetValue(true));
            Focus.AddItem(new MenuItem("force focus selected", "force focus selected").SetValue(false));
            Focus.AddItem(new MenuItem("if selected in :", "if selected in :").SetValue(new Slider(1000, 1000, 1500)));
            KS.AddItem(new MenuItem("Use Q KillSteal", "Use Q KillSteal").SetValue(true));
            KS.AddItem(new MenuItem("Use E KillSteal", "Use E KillSteal").SetValue(true));
            KS.AddItem(new MenuItem("Use R KillSteal", "Use R KillSteal").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use R Follow", "Use R Follow").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use W GapCloser", "Use W anti gap").SetValue(true));

            _menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += Create;
            GameObject.OnDelete += Delete;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Chat.Print("Welcome to ViktorWorld");
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_menu.Item("Use W GapCloser").GetValue<bool>() && _w.IsReady() && gapcloser.Sender.IsValidTarget(_w.Range))
            {
                var pos = gapcloser.End;
                if (Player.Distance(pos) <= _w.Range)
                    _w.Cast(pos);
            }
        }
        private static void Create(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Viktor_Base_R_Droid.troy"))
            {
                ViktorR = sender;
            }
        }
        private static void Delete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Viktor_Base_R_Droid.troy"))
            {
                ViktorR = null;
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            //Chat.Print(Player.Position.Distance(Game.CursorPos).ToString());
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Selected()))
            {
                if (_q.IsReady())
                {
                    _orbwalker.SetAttack(false);
                }
                else
                    _orbwalker.SetAttack(true);
            }
            else
            {
                _orbwalker.SetAttack(true);
            }

            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (_menu.Item("Use Q Combo").GetValue<bool>())
                {
                    UseQ();
                }
                if (_menu.Item("Use E Combo").GetValue<bool>())
                {
                    UseE();
                }
                if (_menu.Item("Use W Combo").GetValue<bool>())
                {
                    UseW();
                }
                if (_menu.Item("Use R Burst Selected").GetValue<bool>())
                {
                    UseR();
                }
            }
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (_menu.Item("Use Q Harass").GetValue<bool>())
                    UseQ();
                if (_menu.Item("Use E Harass").GetValue<bool>())
                    UseE();
            }
            ViktorRMove();
            killsteal();
        }

        private static void UseW()
        {
            var target = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
            if ( target.IsValidTarget() && !target.IsZombie && _w.IsReady())
            {
                var pos = Prediction.GetPrediction(target, 0.25f).UnitPosition;
                if (Player.Distance(pos) <= _w.Range)
                {
                    _w.Cast(pos);
                }
            }
        }
        private static void ViktorRMove()
        {
            if (_menu.Item("Use R Follow").GetValue<bool>() && ViktorR != null && _r.IsReady())
            {
                var target = ViktorR.Position.GetEnemiesInRange(2000).Where(t => t.IsValidTarget() && !t.IsZombie).OrderByDescending(t => 1 - t.Distance(ViktorR.Position)).FirstOrDefault();
                if (target.Distance(ViktorR.Position) >= 50)
                {
                    Vector3 x = Prediction.GetPrediction(target,0.5f).UnitPosition;
                    _r.Cast(x);
                }
            }
        }
        private static bool Selected()
        {
            if (!_menu.Item("force focus selected").GetValue<bool>())
            {
                return false;
            }
            else
            {
                var target = TargetSelector.GetSelectedTarget();
                float a = _menu.Item("if selected in :").GetValue<Slider>().Value;
                if (target == null || target.IsDead || target.IsZombie)
                {
                    return false;
                }
                return !(Player.Distance(target.Position) > a);
            }
        }

        private static Obj_AI_Base Gettarget(float range)
        {
            return Selected() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);
        }

        private static void UseQ()
        {
            if (!_q.IsReady())
                return;
            var target = Gettarget(650);
            if (target != null && target.IsValidTarget(650) && !target.IsZombie && _q.IsReady())
                _q.Cast(target);
        }

        private static void UseR()
        {
            if (!_r.IsReady())
                return;
            if (_r.IsReady() && _r.Instance.Name == "ViktorChaosStorm")
            {
                {
                    var target = TargetSelector.GetSelectedTarget();
                    if (target != null && Player.Distance(target.Position) <= 1000 && target.IsValidTarget() && !target.IsZombie && _r.IsReady() && _r.Instance.Name == "ViktorChaosStorm")
                    {
                        CastR(target);
                    }
                }
                {
                    var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
                    if (target != null && target.IsValidTarget() && !target.IsZombie && _r.IsReady() && _r.Instance.Name == "ViktorChaosStorm" )
                    {
                        if (target.Health <= _r.GetDamage(target)*1.7)
                        {
                            CastR(target);
                        }
                    }
                    foreach(var hero in HeroManager.Enemies.Where(x=> x.IsValidTarget(1000) && !x.IsZombie))
                    {

                    }
                }
            }
        }

        private static void CastR(Obj_AI_Base target)
        {
            if (!target.IsValidTarget() || target.IsZombie)
                return;
            var predpos = Prediction.GetPrediction(target, 0.25f).UnitPosition.To2D();
            if (predpos.Distance(Player.Position.To2D()) <= 1000 )
            {
                var castpos = predpos.Distance(Player.Position.To2D()) > 700 ?
                    Player.Position.To2D().Extend(predpos, 700) :
                    predpos;
                _r.Cast(predpos);
            }
        }

        private static void UseE(Obj_AI_Base  ForceTarget = null)
        {
            if (!_e.IsReady())
                return;
            var target = Gettarget(525 + 700);
            if (ForceTarget != null)
                target = ForceTarget;
            if (target != null && target.IsValidTarget(1025) && !target.IsZombie && _e.IsReady())
            {
                AIHeroClient startHeroPos = HeroManager.Enemies.Where(x => x.IsValidTarget(525) && x.NetworkId != target.NetworkId && x.Distance(target) <= 700).MinOrDefault(x => x.Health);
                AIHeroClient startHeroExtend = HeroManager.Enemies.Where(x => x.IsValidTarget() && x.NetworkId != target.NetworkId && x.Distance (target) <= 700
                    && target.Position.To2D().Extend(x.Position.To2D(), 700).Distance(Player.Position) <= 525).MinOrDefault(x => x.Health);
                AIHeroClient endHeroPos = HeroManager.Enemies.Where(x => x.IsValidTarget(525 + 700) && x.NetworkId != target.NetworkId && target.IsValidTarget(525)
                    && x.Distance(target) <= 700).MinOrDefault(x => x.Health);
                AIHeroClient endHeroExtend = HeroManager.Enemies.Where(x => x.IsValidTarget(1025) && x.NetworkId != target.NetworkId
                    && x.Distance(target) <= 700 && x.Position.To2D().Extend(target.Position.To2D(),700).Distance(Player.Position) <= 525).MinOrDefault(x => x.Health);
                Vector3 DefaultPos = Player.Distance(target.Position) >= 525 ? Player.Position.To2D().Extend(target.Position.To2D(), 525).To3D() : target.Position;
                if (startHeroPos != null)
                {
                    _e.SetSkillshot(0.25f, 80, 1050, false, SkillshotType.SkillshotLine, startHeroPos.Position, startHeroPos.Position);
                    CastE(target);
                }
                else if (startHeroExtend != null)
                {
                    //float r = 525;
                    //float d = target.Distance(Player);
                    //float h = Geometry.Distance(Player.Position.To2D(), target.Position.To2D(), startHeroExtend.Position.To2D());
                    //float a = (float)Math.Sqrt(d * d - h * h);
                    //float b = (float)Math.Sqrt(r * r - h * h);
                    //float c = a - b;
                    _e.SetSkillshot(0.25f, 80, 1050, false, SkillshotType.SkillshotLine, target.Position.To2D().Extend(startHeroExtend.Position.To2D(), 700).To3D(), target.Position.To2D().Extend(startHeroExtend.Position.To2D(), 700).To3D());
                    CastE(target);
                }
                else if (endHeroPos != null)
                {
                    _e.SetSkillshot(0.25f, 80, 1050, false, SkillshotType.SkillshotLine, target.Position, target.Position);
                    CastE(endHeroPos);
                }
                else if(endHeroExtend != null)
                {
                    //float r = 525;
                    //float d = endHeroExtend.Distance(Player);
                    //float h = Geometry.Distance(Player.Position.To2D(), target.Position.To2D(), endHeroExtend.Position.To2D());
                    //float a = (float)Math.Sqrt(d * d - h * h);
                    //float b = (float)Math.Sqrt(r * r - h * h);
                    //float c = a - b;
                    _e.SetSkillshot(0.25f, 80, 1050, false, SkillshotType.SkillshotLine, endHeroExtend.Position.To2D().Extend(target.Position.To2D(), 700).To3D(), endHeroExtend.Position.To2D().Extend(target.Position.To2D(), 700).To3D());
                    CastE(endHeroExtend);
                }
                else
                {
                    _e.SetSkillshot(0.25f, 80, 1050, false, SkillshotType.SkillshotLine, DefaultPos, DefaultPos);
                    CastE(target);
                }
            }
        }
        public static void CastE(Obj_AI_Base target)
        {
            if (target == null)
                return;
            var pred = _e.GetPrediction(target);
            if (pred.Hitchance >= HitChance.Medium)
            {
                _e.Cast(_e.RangeCheckFrom, pred.CastPosition);
            }
        }
        public static void killsteal()
        {
            if (_q.IsReady() && _menu.Item("Use Q KillSteal").GetValue<bool>() && !Player.Spellbook.IsAutoAttacking)
            {
                foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsEnemy && hero.IsValidTarget(650)))
                {
                    var dmg = Dame(hero, SpellSlot.Q);
                    if (hero != null && hero.IsValidTarget() && !hero.IsZombie && dmg > hero.Health) { _q.Cast(hero); }
                }
            }
            if (_e.IsReady() && _menu.Item("Use E KillSteal").GetValue<bool>() && !Player.Spellbook.IsAutoAttacking)
            {
                foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsEnemy && hero.IsValidTarget(1025)))
                {
                    var dmg = Dame(hero, SpellSlot.E);
                    if (hero != null && hero.IsValidTarget() && !hero.IsZombie && dmg > hero.Health)
                    {
                        UseE(hero);
                    }
                }
            }

            if (_r.IsReady() && _menu.Item("Use R KillSteal").GetValue<bool>() && !Player.Spellbook.IsAutoAttacking)
            {
                foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsEnemy && hero.IsValidTarget(860)))
                {
                    var dmgR = Dame(hero, SpellSlot.R);
                    var dmgE = Dame(hero, SpellSlot.E);
                    var dmgQ = Dame(hero, SpellSlot.Q);
                    if (hero != null && hero.IsValidTarget() && !hero.IsZombie)
                    {
                        if (dmgE > hero.Health && dmgR > hero.Health)
                        {
                            if (!_e.IsReady())
                                CastR(hero);
                        }
                        else if (dmgQ > hero.Health && dmgR > hero.Health && Player.Distance(hero.Position) <= 600)
                        {
                            if (!_q.IsReady() && !_e.IsReady())
                                CastR(hero);
                        }
                        else if (dmgR > hero.Health) { _r.Cast(hero); }
                    }
                }
            }

        }
        public static double Dame(Obj_AI_Base target, SpellSlot x)
        {
            if (target != null) { return Player.GetSpellDamage(target, x); } else return 0;
        }

    }
}
