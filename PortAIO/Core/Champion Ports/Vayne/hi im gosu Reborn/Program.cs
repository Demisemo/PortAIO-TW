#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SebbyLib;


#endregion

using EloBuddy; 
using LeagueSharp.Common; 
namespace hi_im_gosu_Reborn
{
    public class Vayne
    {
        public static Spell E;
        public static Spell E2;
        public static Spell Q;
        public static Spell W;
        public static Spell R;




        public static Vector3 TumblePosition = Vector3.Zero;


        public static Orbwalking.Orbwalker orbwalker;

        private static string News = "Added New Custom Orbwalker for better cs'ing and movement";

        public static Menu menu;

        public static Dictionary<string, SpellSlot> spellData;
        public static Items.Item zzrot = new Items.Item(3512, 400);

        public static AIHeroClient tar;
        public const string ChampName = "Vayne";
        public static AIHeroClient Player;
        public static BuffType[] buffs;
        public static Spell cleanse;
        public static Menu Itemsmenu;
        public static Menu qmenu;
        public static Menu emenu;
        public static Menu gmenu;
        public static Menu imenu;
        public static Menu rmenu;
        public static Menu botrk;
        public static Menu qss;

        //private static Obj_AI_Base target;
        /* Asuna VayneHunter Copypasta */
        public static readonly Vector2 MidPos = new Vector2(6707.485f, 8802.744f);

        public static readonly Vector2 DragPos = new Vector2(11514, 4462);

        public static float LastMoveC;
        //private static Obj_AI_Base target;
        public static bool CheckTarget(Obj_AI_Base target, float range = float.MaxValue)
        {
            if (target == null)
            {
                return false;
            }

            if (target.DistanceToPlayer() > range)
            {
                return false;
            }

            if (target.HasBuff("KindredRNoDeathBuff"))
            {
                return false;
            }

            if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
            {
                return false;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
            {
                return false;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return false;
            }

            if (target.HasBuff("SivirShield"))
            {
                return false;
            }

            return !target.HasBuff("FioraW");
        }
       

        public static void MoveToLimited(Vector3 where)
        {
            if (Environment.TickCount - LastMoveC < 80)
            {
                return;
            }

            LastMoveC = Environment.TickCount;
            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, where);
        }

        /* End Asuna VayneHunter Copypasta */

        public static void Main()
        {
            try
            {
                Game_OnGameLoad(new EventArgs());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }
        /*public static void DrawingOnOnDraw(EventArgs args)
        {
            // if vayne got Q ready
            if (Q.IsReady())
            {
                var s = ObjectManager.Player.Position;
                var e = s.Extend(Game.CursorPos, Q.Range);
                DrawPointer(s, e, Q.Range);
            }
        }*/

        /* public static void DrawPointer(Vector3 start, Vector3 end, float len)
         {
             var line = new Geometry.Polygon.Line(start, end, len);
             var endNext = end.Extend(new Vector3(1, 0, 0), 100).To2D()
                 .RotateAroundPoint(start.To2D(), 90 * (float)Math.PI / 180);
             var endNext2 = end.Extend(new Vector3(1, 0, 0), 100).To2D()
                 .RotateAroundPoint(start.To2D(), -90 * (float)Math.PI / 180);
             var line2 = new Geometry.Polygon.Line(end.To2D(), endNext, 50);
             var line3 = new Geometry.Polygon.Line(end.To2D(), endNext2, 50);
             line2.Draw(System.Drawing.Color.Crimson);
             line3.Draw(System.Drawing.Color.Crimson);
             line.Draw(System.Drawing.Color.Crimson);
         }*/
        public static void Game_OnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q, 300f);
            W = new Spell(SpellSlot.W);
            E2 = new Spell(
               SpellSlot.E,(uint)(650 + ObjectManager.Player.BoundingRadius));
            E = new Spell(SpellSlot.E, 550f);
            E.SetTargetted(0.25f, 1600f);
            R = new Spell(SpellSlot.R);

            Player = ObjectManager.Player;
            //Utils.PrintMessage("Vayne loaded");
            if (Player.ChampionName != ChampName) return;
            spellData = new Dictionary<string, SpellSlot>();
            //Chat.Print("Riven");
            menu = new Menu("Gosu", "Gosu", true);
            //Orbwalker
            menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("Orbwalker"));
            //TS
            var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            // SimpleTs.AddToMenu(TargetSelectorMenu);
            menu.AddSubMenu(TargetSelectorMenu);

            menu.AddItem(
                new MenuItem("aaqaa", "Auto -> Q -> AA").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            qmenu = menu.AddSubMenu(new Menu("Tumble", "Tumble"));
            qmenu.AddItem(new MenuItem("UseQC", "Use Q Combo").SetValue(true));
            qmenu.AddItem(new MenuItem("hq", "Use Q Harass").SetValue(true));
            qmenu.AddItem(new MenuItem("restrictq", "Restrict Q usage?").SetValue(true));
            qmenu.AddItem(new MenuItem("UseQJ", "Use Q Farm").SetValue(true));
            qmenu.AddItem(new MenuItem("Junglemana", "Minimum Mana to Use Q Farm").SetValue(new Slider(60, 1, 100)));
            qmenu.AddItem(new MenuItem("AntiMQ", "Use Anti - Melee [Q]").SetValue(true));
           // qmenu.AddItem(new MenuItem("FastQ", "Fast Q").SetValue(true).SetValue(new KeyBind("Q".ToCharArray()[0], KeyBindType.Press)));
            qmenu.AddItem(new MenuItem("FocusTwoW", "Focus 2 W Stacks").SetValue(true));
            //qmenu.AddItem(new MenuItem("DrawQ", "Draw Q Arrow").SetValue(true));


            emenu = menu.AddSubMenu(new Menu("Condemn", "Condemn"));
            emenu.AddItem(new MenuItem("UseEC", "Use E Combo").SetValue(true));
            emenu.AddItem(new MenuItem("he", "Use E Harass").SetValue(true));
            emenu.AddItem(new MenuItem("UseET", "Use E (Toggle)").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            emenu.AddItem(new MenuItem("zzrot", "[Beta] ZZrot Condemn").SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle))).Permashow(true, "Vayne | ZZRot Toggle", Color.Aqua);
           // emenu.AddItem(new MenuItem("FlashE", "Flash E").SetValue(true).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));


            //emenu.AddItem(new MenuItem("Gap_E", "Use E To Gabcloser").SetValue(true));
            // emenu.AddItem(new MenuItem("GapD", "Anti GapCloser Delay").SetValue(new Slider(0, 0, 1000)).SetTooltip("Sets a delay before the Condemn for Antigapcloser is casted."));
            emenu.AddItem(new MenuItem("EMode", "Use E Mode:", true).SetValue(new StringList(new[] { "Lord's", "Gosu", "Flowers", "VHR", "Marksman", "Sharpshooter", "OKTW", "Shine", "PRADASMART", "PRADAPERFECT", "OLDPRADA", "PRADALEGACY" })));
            emenu.AddItem(new MenuItem("PushDistance", "E Push Distance").SetValue(new Slider(415, 475, 300)));
            emenu.AddItem(new MenuItem("EHitchance", "E Hitchance").SetValue(new Slider(50, 1, 100)).SetTooltip("Only use this for Prada Condemn Methods"));
            emenu.AddItem(new MenuItem("UseEaa", "Use E after auto").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Press)));



            rmenu = menu.AddSubMenu(new Menu("Ult", "Ult"));
            rmenu.AddItem(new MenuItem("visibleR", "Smart Invisible R").SetValue(true).SetTooltip("Wether you want to set a delay to stay in R before you Q"));
            rmenu.AddItem(new MenuItem("Qtime", "Duration to wait").SetValue(new Slider(700, 0, 1000)));

            imenu = menu.AddSubMenu(new Menu("Interrupt Settings", "Interrupt Settings"));
            imenu.AddItem(new MenuItem("Int_E", "Use E To Interrupt").SetValue(true));
            imenu.AddItem(new MenuItem("Interrupt", "Interrupt Danger Spells", true).SetValue(true));
            imenu.AddItem(new MenuItem("AntiAlistar", "Interrupt Alistar W", true).SetValue(true));
            imenu.AddItem(new MenuItem("AntiRengar", "Interrupt Rengar Jump", true).SetValue(true));
            imenu.AddItem(new MenuItem("AntiKhazix", "Interrupt Khazix R", true).SetValue(true));


            gmenu = menu.AddSubMenu(new Menu("Gap Closer", "Gap Closer"));
            gmenu.AddItem(new MenuItem("Gapcloser", "Anti Gapcloser", true).SetValue(false));
            foreach (var target in HeroManager.Enemies)
            {
                gmenu.AddItem(
                    new MenuItem("AntiGapcloser" + target.ChampionName.ToLower(), target.ChampionName, true)
                        .SetValue(false));
            }


            menu.AddItem(new MenuItem("walltumble", "Wall Tumble"))
                .SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Press));
            menu.AddItem(new MenuItem("useR", "Use R Combo").SetValue(true));
            menu.AddItem(new MenuItem("enemys", "If Enemys Around >=").SetValue(new Slider(2, 1, 5)));
            Itemsmenu = menu.AddSubMenu(new Menu("Items", "Items"));
            Itemsmenu.AddSubMenu(new Menu("Potions", "Potions"));
            Itemsmenu.SubMenu("Potions")
                .AddItem(new MenuItem("usehppotions", "Use Health potion/Refillable/Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            Itemsmenu.SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "If Health % <").SetValue(new Slider(35, 1, 100)));
            Itemsmenu.SubMenu("Potions")
                .AddItem(new MenuItem("usemppotions", "Use Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            Itemsmenu.SubMenu("Potions")
                .AddItem(new MenuItem("usepotionmp", "If Mana % <").SetValue(new Slider(35, 1, 100)));
            Itemsmenu.AddItem(new MenuItem("BOTRK", "Use BOTRK").SetValue(true));
            botrk = Itemsmenu.AddSubMenu(new Menu("BOTRK Settings", "usebotrk"));
            botrk.AddItem(new MenuItem("myhp", "Use if my HP < %")).SetValue(new Slider(20, 0, 100));
            botrk.AddItem(new MenuItem("theirhp", "Use if enemy HP < %")).SetValue(new Slider(20, 0, 100));
            Itemsmenu.AddItem(new MenuItem("Ghostblade", "Use Ghostblade").SetValue(true));
            Itemsmenu.AddItem(new MenuItem("QSS", "Use QSS/Merc Scimitar/Cleanse").SetValue(true));
            qss = menu.SubMenu("Items").AddSubMenu(new Menu("QSS/Merc/Cleanse Settings", "useqss"));

            buffs = new[]
                        {
                            BuffType.Blind, BuffType.Charm, BuffType.CombatDehancer, BuffType.Fear, BuffType.Flee,
                            BuffType.Knockback, BuffType.Knockup, BuffType.Polymorph, BuffType.Silence,
                            BuffType.Snare, BuffType.Stun, BuffType.Suppression, BuffType.Taunt
                        };

            for (int i = 0; i < buffs.Length; i++)
            {
                qss.AddItem(new MenuItem(buffs[i].ToString(), buffs[i].ToString()).SetValue(true));
            }

            Q = new Spell(SpellSlot.Q, 0f);
            R = new Spell(SpellSlot.R, float.MaxValue);
            E = new Spell(SpellSlot.E, 650f);
            E.SetTargetted(0.25f, 1600f);

            var cde = ObjectManager.Player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlot("SummonerBoost"));
            if (cde != null)
            {
                if (cde.Slot != SpellSlot.Unknown) //trees
                {
                    cleanse = new Spell(cde.Slot);
                }
            }

            E.SetTargetted(0.25f, 2200f);
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Obj_AI_Base.OnPlayAnimation += OnPlay;
            GameObject.OnCreate += OnCreate;
            //  Drawing.OnDraw += DrawingOnOnDraw;


            //Chat.Print("<font color='#881df2'>Blm95 Vayne Reborn by LordZEDith</font> Loaded.");
            Chat.Print("<font size='30'>hi_im_gosu Reborn</font> <font color='#b756c5'>by LordZEDith</font>");
            Chat.Print("<font color='#b756c5'>NEWS: </font>" + News);
            //Chat.Print(
            // "<font color='#f2f21d'>Do you like it???  </font> <font color='#ff1900'>Drop 1 Upvote in Database </font>");
            //  Chat.Print(
            //  "<font color='#f2f21d'>Buy me cigars </font> <font color='#ff1900'>ssssssssssmith@hotmail.com</font> (10) S");
            menu.AddToMainMenu();
        }



        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady())
            {
                if (imenu.Item("AntiAlistar", true).GetValue<bool>() && gapcloser.Sender.ChampionName == "Alistar" &&
                    gapcloser.SkillType == GapcloserType.Targeted)
                {
                    E.Cast(gapcloser.Sender, true);

                    if (gmenu.Item("Gapcloser", true).GetValue<bool>() &&
                        gmenu.Item("AntiGapcloser" + gapcloser.Sender.ChampionName.ToLower(), true).GetValue<bool>())
                    {
                        if (gapcloser.Sender.DistanceToPlayer() <= 200 && gapcloser.Sender.IsValid)
                        {
                            E.CastOnUnit(gapcloser.Sender, true);
                        }
                    }
                }
            }
        }
        public static void OnCreate(GameObject sender, EventArgs Args)
        {
            var Rengar = HeroManager.Enemies.Find(heros => heros.ChampionName.Equals("Rengar"));
            var Khazix = HeroManager.Enemies.Find(heros => heros.ChampionName.Equals("Khazix"));

            if (Rengar != null && imenu.Item("AntiRengar", true).GetValue<bool>())
            {
                if (sender.Name == "Rengar_LeapSound.troy" && sender.Position.Distance(ObjectManager.Player.Position) < E.Range)
                {
                    E.CastOnUnit(Rengar);
                }
            }

            if (Khazix != null && imenu.Item("AntiKhazix", true).GetValue<bool>())
            {
                if (sender.Name == "Khazix_Base_E_Tar.troy" && sender.Position.Distance(ObjectManager.Player.Position) <= 300)
                {
                    E.CastOnUnit(Khazix);
                }
            }
        }

        public static void Interrupter2_OnInterruptableTarget(AIHeroClient unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (imenu.Item("Int_E", true).GetValue<bool>() && E.IsReady() && unit.IsEnemy &&
                unit.IsValidTarget(E.Range))
            {
                if (args.DangerLevel >= Interrupter2.DangerLevel.High)
                {
                    E.CastOnUnit(unit, true);
                }
            }
        }

        public static void Game_ProcessSpell(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name.ToLower() == "zedult" && args.Target.IsMe)
            {
                if (Items.CanUseItem(3140))
                {
                    LeagueSharp.Common.Utility.DelayAction.Add(1000, () => Items.UseItem(3140));
                }
                else if (Items.CanUseItem(3139))
                {
                    LeagueSharp.Common.Utility.DelayAction.Add(1000, () => Items.UseItem(3139));
                }
                else if (cleanse != null && cleanse.IsReady())
                {
                    LeagueSharp.Common.Utility.DelayAction.Add(1000, () => cleanse.Cast());
                }
            }
            if (hero != null)
                if (args.Target != null)
                    if (args.Target.IsMe)
                        if (hero.Type == GameObjectType.AIHeroClient)
                            if (hero.IsEnemy)
                                if (hero.IsMelee)
                                    if (args.SData.IsAutoAttack())
                                        if (qmenu.Item("AntiMQ").GetValue<bool>())
                                            if (Q.IsReady())
                                                Q.Cast(ObjectManager.Player.Position.Extend(hero.Position, -Q.Range));

        }

        public static void Usepotion()
        {
            var iusehppotion = menu.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = Player.Health
                               <= (Player.MaxHealth * (menu.Item("usepotionhp").GetValue<Slider>().Value) / 100);
            var iusemppotion = menu.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = Player.Mana
                               <= (Player.MaxMana * (menu.Item("usepotionmp").GetValue<Slider>().Value) / 100);
            if (Player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (LeagueSharp.Common.Utility.CountEnemiesInRange(800) > 0)
            {
                if (iusepotionhp && iusehppotion
                    && !(ObjectManager.Player.HasBuff("RegenerationPotion")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")))
                {
                    if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }

                    if (Items.HasItem(2003) && Items.CanUseItem(2003))
                    {
                        Items.UseItem(2003);
                    }

                    if (Items.HasItem(2031) && Items.CanUseItem(2031))
                    {
                        Items.UseItem(2031);
                    }

                    if (Items.HasItem(2032) && Items.CanUseItem(2032))
                    {
                        Items.UseItem(2032);
                    }

                    if (Items.HasItem(2033) && Items.CanUseItem(2033))
                    {
                        Items.UseItem(2033);
                    }
                }

                if (iusepotionmp && iusemppotion
                    && !(ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }

                    if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }

                    if (Items.HasItem(2032) && Items.CanUseItem(2032))
                    {
                        Items.UseItem(2032);
                    }

                    if (Items.HasItem(2033) && Items.CanUseItem(2033))
                    {
                        Items.UseItem(2033);
                    }
                }
            }
        }

        /* public static void Farm()
         {
             var mob =
                 MinionManager.GetMinions(
                     Player.ServerPosition,
                     E.Range,
                     MinionTypes.All,
                     MinionTeam.Neutral,
                     MinionOrderTypes.MaxHealth).FirstOrDefault();
             var Minions = MinionManager.GetMinions(Player.Position.Extend(Game.CursorPos, Q.Range), Player.AttackRange, MinionTypes.All);
             var useQ = qmenu.Item("UseQJ").GetValue<bool>();
             int countMinions = 0;
             foreach (var minions in Minions.Where(minion => minion.Health < Player.GetAutoAttackDamage(minion) || minion.Health < Q.GetDamage(minion)))
             {
                 countMinions++;
             }
             if (countMinions >= 2 && useQ && Q.IsReady() && Minions != null)
                 Q.Cast(Player.Position.Extend(Game.CursorPos, Q.Range/2));
             if (useQ && Q.IsReady() && Orbwalking.InAutoAttackRange(mob) && mob != null)
             {
                 Q.Cast(Game.CursorPos);
             }
         }*/
        public static void OnPlay(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe)
            {
                switch (args.Animation)
                {
                    case "Spell1a":
                   EloBuddy.Player.DoEmote(Emote.Dance);
                        break;
                }
            }
        }

        public static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;

            if (orbwalker.ActiveMode.ToString() == "LaneClear"
                && 100 * (Player.Mana / Player.MaxMana) > qmenu.Item("Junglemana").GetValue<Slider>().Value)
            {
                var mob =
                    MinionManager.GetMinions(
                        Player.ServerPosition,
                        E.Range,
                        MinionTypes.All,
                        MinionTeam.Neutral,
                        MinionOrderTypes.MaxHealth).FirstOrDefault();
                var Minions = MinionManager.GetMinions(
                    Player.Position.Extend(Game.CursorPos, Q.Range),
                    Player.AttackRange,
                    MinionTypes.All);
                var useQ = qmenu.Item("UseQJ").GetValue<bool>();
                int countMinions = 0;
                foreach (var minions in
                    Minions.Where(
                        minion =>
                        minion.Health < Player.GetAutoAttackDamage(minion)
                        || minion.Health < Q.GetDamage(minion) + Player.GetAutoAttackDamage(minion) || minion.Health < Q.GetDamage(minion)))
                {
                    countMinions++;
                }

                if (countMinions >= 2 && useQ && Q.IsReady() && Minions != null) Q.Cast(Player.Position.Extend(Game.CursorPos, Q.Range / 2));

                if (useQ && Q.IsReady() && Orbwalking.InAutoAttackRange(mob) && mob != null)
                {
                    Q.Cast(Game.CursorPos);
                    EloBuddy.Player.DoEmote(Emote.Dance);

                }
            }


            if (!(target is AIHeroClient)) return;

            tar = (AIHeroClient)target;

            if (menu.Item("aaqaa").GetValue<KeyBind>().Active)
            {
                if (Q.IsReady())
                {

                    Q.Cast(Game.CursorPos);
                    EloBuddy.Player.DoEmote(Emote.Dance);

                }


               Orbwalking.Orbwalk(TargetSelector.GetTarget(625, TargetSelector.DamageType.Physical), Game.CursorPos);
            }

           // Condemn.FlashE();

            if (menu.Item("zzrot").GetValue<KeyBind>().Active)
            {
                Condemn.RotE();
            }


            if (orbwalker.ActiveMode.ToString() == "Combo")
            {
                if (Itemsmenu.Item("BOTRK").GetValue<bool>()
                    && (tar.Health <= tar.MaxHealth * botrk.Item("theirhp").GetValue<Slider>().Value / 100)
                    || (Player.Health <= Player.MaxHealth * botrk.Item("myhp").GetValue<Slider>().Value / 100))
                {
                    //Chat.Print("in");
                    if (Items.CanUseItem(3153))
                    {
                        Items.UseItem(3153, tar);
                    }
                    else if (Items.CanUseItem(3144))
                    {
                        {
                            Items.UseItem(3144, tar);
                        }
                    }
                }

                if (Itemsmenu.Item("Ghostblade").GetValue<bool>() && tar.IsValidTarget(800))
                {
                    if (Items.CanUseItem(3142))
                    {
                        Items.UseItem(3142);
                    }
                }
            }

            if (emenu.Item("UseEaa").GetValue<KeyBind>().Active)
            {
                E.Cast((Obj_AI_Base)target);
                emenu.Item("UseEaa").SetValue<KeyBind>(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle));
            }

            if (Q.IsReady()
                && ((orbwalker.ActiveMode.ToString() == "Combo" && qmenu.Item("UseQC").GetValue<bool>())
                    || (orbwalker.ActiveMode.ToString() == "Mixed" && qmenu.Item("hq").GetValue<bool>())))
            {
                if (qmenu.Item("restrictq").GetValue<bool>())
                {
                    var after = ObjectManager.Player.Position
                                + Normalize(Game.CursorPos - ObjectManager.Player.Position) * 300;
                    //Chat.Print("After: {0}", after);
                    var disafter = Vector3.DistanceSquared(after, tar.Position);
                    //Chat.Print("DisAfter: {0}", disafter);
                    //Chat.Print("first calc: {0}", (disafter) - (630*630));
                    if ((disafter < 630 * 630) && disafter > 150 * 150)
                    {
                        Q.Cast(Game.CursorPos);
                        EloBuddy.Player.DoEmote(Emote.Dance);

                    }

                    if (Vector3.DistanceSquared(tar.Position, ObjectManager.Player.Position) > 630 * 630
                        && disafter < 630 * 630)
                    {
                        Q.Cast(Game.CursorPos);
                        EloBuddy.Player.DoEmote(Emote.Dance);

                    }
                }
                else
                {
                    Q.Cast(Game.CursorPos);
                    EloBuddy.Player.DoEmote(Emote.Dance);


                }
                //Q.Cast(Game.CursorPos);
            }
        }
        public static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                var buff = ObjectManager.Player.GetBuff("vaynetumblefade");
                if (buff != null)
                    if (buff.IsValidBuff())
                        if (rmenu.Item("visibleR").GetValue<bool>() && Player.CountEnemiesInRange(800) >= 1)
                        {
                            if (buff.EndTime - Game.Time >
                            buff.EndTime - buff.StartTime -
                            (rmenu.Item("Qtime").GetValue<Slider>().Value / 1000))
                                if (!ObjectManager.Player.Position.UnderTurret(true))
                                    args.Process = false;
                        }
                        else
                        {
                            args.Process = true;
                        }
            }
        }

        public static Vector3 Normalize(Vector3 A)
        {
            double distance = Math.Sqrt(A.X * A.X + A.Y * A.Y);
            return new Vector3(new Vector2((float)(A.X / distance)), (float)(A.Y / distance));
        }


        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (menu.Item("useR").GetValue<bool>() && R.IsReady()
                && ObjectManager.Player.CountEnemiesInRange(1000) >= menu.Item("enemys").GetValue<Slider>().Value
                && orbwalker.ActiveMode.ToString() == "Combo")
            {
                R.Cast();
            }

            Usepotion(); 

            if (menu.Item("aaqaa").GetValue<KeyBind>().Active)
            {
               Orbwalking.Orbwalk(TargetSelector.GetTarget(625, TargetSelector.DamageType.Physical), Game.CursorPos);
              
            }

            if (Itemsmenu.Item("QSS").GetValue<bool>())
            {
                for (int i = 0; i < buffs.Length; i++)
                {
                    if (ObjectManager.Player.HasBuffOfType(buffs[i]) && qss.Item(buffs[i].ToString()).GetValue<bool>())
                    {
                        if (Items.CanUseItem(3140))
                        {
                            Items.UseItem(3140);
                        }
                        else if (Items.CanUseItem(3139))
                        {
                            Items.UseItem(3140);
                        }
                        else if (cleanse != null && cleanse.IsReady())
                        {
                            cleanse.Cast();
                        }
                    }
                }
            }

            //||
            //(orbwalker.ActiveMode.ToString() != "Combo" || !menu.Item("UseEC").GetValue<bool>()) &&
            //!menu.Item("UseET").GetValue<KeyBind>().Active)) return;
            if ((orbwalker.ActiveMode.ToString() == "Combo" && emenu.Item("UseEC").GetValue<bool>()) || (orbwalker.ActiveMode.ToString() == "Mixed" && emenu.Item("he").GetValue<bool>()) || emenu.Item("UseET").GetValue<KeyBind>().Active)
            {
                switch (emenu.Item("EMode", true).GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        {
                            Condemn.Run();
                        }
                        break;
                    case 1:
                        {
                            foreach (var hero in from hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsValidTarget(550f))
                                                 let prediction = E.GetPrediction(hero)
                                                 where NavMesh.GetCollisionFlags(
                                                     prediction.UnitPosition.To2D()
                                                         .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                                             -emenu.Item("PushDistance").GetValue<Slider>().Value)
                                                         .To3D())
                                                     .HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(
                                                         prediction.UnitPosition.To2D()
                                                             .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                                                 -(emenu.Item("PushDistance").GetValue<Slider>().Value / 2))
                                                             .To3D())
                                                         .HasFlag(CollisionFlags.Wall)
                                                 select hero)
                            {
                                E.CastOnUnit(hero);
                            }
                        }
                        break;
                    case 2:
                        {
                            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                            var EPred = E.GetPrediction(target);
                            var PD = emenu.Item("PushDistance").GetValue<Slider>().Value;
                            var PP = EPred.UnitPosition.Extend(ObjectManager.Player.Position, -PD);

                            for (int i = 1; i < PD; i += (int)target.BoundingRadius)
                            {
                                var VL = EPred.UnitPosition.Extend(ObjectManager.Player.Position, -i);
                                var J4 = ObjectManager.Get<Obj_AI_Base>()
                                    .Any(f => f.Distance(PP) <= target.BoundingRadius && f.Name.ToLower() == "beacon");
                                var CF = NavMesh.GetCollisionFlags(VL);

                                if (CF.HasFlag(CollisionFlags.Wall) || CF.HasFlag(CollisionFlags.Building) || J4)
                                {
                                    if (CheckTarget(target, E.Range))
                                    {
                                        E.CastOnUnit(target);
                                        return;
                                    }
                                }
                            }
                        }
                        break;
                    case 3:
                        {
                            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                            var pushDistance = emenu.Item("PushDistance").GetValue<Slider>().Value;
                            var Prediction = E.GetPrediction(target);
                            var endPosition = Prediction.UnitPosition.Extend
                                (ObjectManager.Player.ServerPosition, -pushDistance);

                            if (Prediction.Hitchance >= HitChance.VeryHigh)
                            {
                                if (endPosition.IsWall())
                                {
                                    var condemnRectangle = new LeagueSharp.Common.Geometry.Polygon.Rectangle(target.ServerPosition.To2D(),
                                        endPosition.To2D(), target.BoundingRadius);

                                    if (
                                        condemnRectangle.Points.Count(
                                            point =>
                                                NavMesh.GetCollisionFlags(point.X, point.Y)
                                                    .HasFlag(CollisionFlags.Wall)) >=
                                        condemnRectangle.Points.Count * (20 / 100f))
                                    {
                                        if (CheckTarget(target, E.Range))
                                        {
                                            E.CastOnUnit(target);
                                        }
                                    }
                                }
                                else
                                {
                                    var step = pushDistance / 5f;
                                    for (float i = 0; i < pushDistance; i += step)
                                    {
                                        var endPositionEx = Prediction.UnitPosition.Extend(ObjectManager.Player.ServerPosition, -i);
                                        if (endPositionEx.IsWall())
                                        {
                                            var condemnRectangle =
                                                new LeagueSharp.Common.Geometry.Polygon.Rectangle(target.ServerPosition.To2D(),
                                                    endPosition.To2D(), target.BoundingRadius);

                                            if (
                                                condemnRectangle.Points.Count(
                                                    point =>
                                                        NavMesh.GetCollisionFlags(point.X, point.Y)
                                                            .HasFlag(CollisionFlags.Wall)) >=
                                                condemnRectangle.Points.Count * (20 / 100f))
                                            {
                                                if (CheckTarget(target, E.Range))
                                                {
                                                    E.CastOnUnit(target);
                                                }
                                            }
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case 4:
                        {
                            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                            for (var i = 1; i < 8; i++)
                            {
                                var targetBehind = target.Position +
                                                   Vector3.Normalize(target.ServerPosition - ObjectManager.Player.Position) * i * 50;

                                if (targetBehind.IsWall() && target.IsValidTarget(E.Range))
                                {
                                    if (CheckTarget(target, E.Range))
                                    {
                                        E.CastOnUnit(target);
                                        return;
                                    }
                                }
                            }
                        }
                        break;
                    case 5:
                        {
                            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                            var prediction = E.GetPrediction(target);

                            if (prediction.Hitchance >= HitChance.High)
                            {
                                var finalPosition = prediction.UnitPosition.Extend(ObjectManager.Player.Position, -400);

                                if (finalPosition.IsWall())
                                {
                                    E.CastOnUnit(target);
                                    return;
                                }

                                for (var i = 1; i < 400; i += 50)
                                {
                                    var loc3 = prediction.UnitPosition.Extend(ObjectManager.Player.Position, -i);

                                    if (loc3.IsWall())
                                    {
                                        if (CheckTarget(target, E.Range))
                                        {
                                            E.CastOnUnit(target);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case 6:
                        {
                            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                            var prepos = E.GetPrediction(target);
                            float pushDistance = 470;
                            var radius = 250;
                            var start2 = target.ServerPosition;
                            var end2 = prepos.CastPosition.Extend(ObjectManager.Player.ServerPosition, -pushDistance);
                            var start = start2.To2D();
                            var end = end2.To2D();
                            var dir = (end - start).Normalized();
                            var pDir = dir.Perpendicular();
                            var rightEndPos = end + pDir * radius;
                            var leftEndPos = end - pDir * radius;
                            var rEndPos = new Vector3(rightEndPos.X, rightEndPos.Y, ObjectManager.Player.Position.Z);
                            var lEndPos = new Vector3(leftEndPos.X, leftEndPos.Y, ObjectManager.Player.Position.Z);
                            var step = start2.Distance(rEndPos) / 10;

                            for (var i = 0; i < 10; i++)
                            {
                                var pr = start2.Extend(rEndPos, step * i);
                                var pl = start2.Extend(lEndPos, step * i);

                                if (pr.IsWall() && pl.IsWall())
                                {
                                    if (CheckTarget(target, E.Range))
                                    {
                                        E.CastOnUnit(target);
                                        return;
                                    }
                                }
                            }
                        }
                        break;
                    case 7:
                        foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(E.Range)))
                        {

                            var pushDistance = emenu.Item("PushDistance").GetValue<Slider>().Value;
                            var targetPosition = E.GetPrediction(target).UnitPosition;
                            var pushDirection = (targetPosition - ObjectManager.Player.ServerPosition).Normalized();
                            float checkDistance = pushDistance / 40f;
                            for (int i = 0; i < 40; i++)
                            {
                                Vector3 finalPosition = targetPosition + (pushDirection * checkDistance * i);
                                var collFlags = NavMesh.GetCollisionFlags(finalPosition);
                                if (collFlags.HasFlag(CollisionFlags.Wall) || collFlags.HasFlag(CollisionFlags.Building)) //not sure about building, I think its turrets, nexus etc
                                {
                                    E.CastOnUnit(target);
                                }
                            }


                        }
                        break;
                    case 8:
                        {
                            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                            var pP = Player.ServerPosition;
                            var p = target.ServerPosition;
                            var pD = Vayne.emenu.Item("PushDistance").GetValue<Slider>().Value;
                           // var mode = Vayne.emenu.Item("EMode", true).GetValue<StringList>().SelectedIndex;
                            if ((p.Extend(pP, -pD).IsCollisionable() || p.Extend(pP, -pD / 2f).IsCollisionable() ||
                 p.Extend(pP, -pD / 3f).IsCollisionable()))
                            {
                                if (!target.CanMove ||
                                    (target.Spellbook.IsAutoAttacking))
                                {
                                    E.CastOnUnit(target);
                                }

                                var enemiesCount = ObjectManager.Player.CountEnemiesInRange(1200);
                                if (enemiesCount > 1 && enemiesCount <= 3)
                                {
                                    var prediction = Vayne.E.GetPrediction(target);
                                    for (var i = 15; i < pD; i += 75)
                                    {
                                        var posFlags = NavMesh.GetCollisionFlags(
                                            prediction.UnitPosition.To2D()
                                                .Extend(
                                                    pP.To2D(),
                                                    -i)
                                                .To3D());
                                        if (posFlags.HasFlag(CollisionFlags.Wall) || posFlags.HasFlag(CollisionFlags.Building))
                                        {
                                            E.CastOnUnit(target);
                                        }

                                        else
                                        {
                                            var hitchance = emenu.Item("EHitchance").GetValue<Slider>().Value;
                                            var angle = 0.20 * hitchance;
                                            const float travelDistance = 0.5f;
                                            var alpha = new Vector2((float)(p.X + travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                                (float)(p.X + travelDistance * Math.Sin(Math.PI / 180 * angle)));
                                            var beta = new Vector2((float)(p.X - travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                                (float)(p.X - travelDistance * Math.Sin(Math.PI / 180 * angle)));

                                            for (var j = 15; j < pD; j += 100)
                                            {
                                                if (pP.To2D().Extend(alpha,
                                                        j)
                                                    .To3D().IsCollisionable() && pP.To2D().Extend(beta, j).To3D().IsCollisionable())
                                                { E.CastOnUnit(target); }
                                            }

                                        }
                                    }
                                }




                            }
                            break;
                        }
                    case 9:
                        {
                            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                            var pP = Player.ServerPosition;
                            var p = target.ServerPosition;
                            var pD = Vayne.emenu.Item("PushDistance").GetValue<Slider>().Value;
                          //  var mode = Vayne.emenu.Item("EMode", true).GetValue<StringList>().SelectedIndex;
                            if (
                (p.Extend(pP, -pD).IsCollisionable() || p.Extend(pP, -pD / 2f).IsCollisionable() ||
                 p.Extend(pP, -pD / 3f).IsCollisionable()))
                            {
                                if (!target.CanMove ||
                                    (target.Spellbook.IsAutoAttacking))
                                {
                                    E.CastOnUnit(target);
                                }
                                var hitchance = emenu.Item("EHitchance").GetValue<Slider>().Value;
                                var angle = 0.20 * hitchance;
                                const float travelDistance = 0.5f;
                                var alpha = new Vector2((float)(p.X + travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                    (float)(p.X + travelDistance * Math.Sin(Math.PI / 180 * angle)));
                                var beta = new Vector2((float)(p.X - travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                    (float)(p.X - travelDistance * Math.Sin(Math.PI / 180 * angle)));

                                for (var i = 15; i < pD; i += 100)
                                {
                                    if (pP.To2D().Extend(alpha,
                                            i)
                                        .To3D().IsCollisionable() && pP.To2D().Extend(beta, i).To3D().IsCollisionable())
                                    {
                                        E.CastOnUnit(target);
                                    }
                                }
                                
                            }
                        }
                        break;
                    case 10:
                        {
                            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                            var pP = Player.ServerPosition;
                            var p = target.ServerPosition;
                            var pD = Vayne.emenu.Item("PushDistance").GetValue<Slider>().Value;
                           // var mode = Vayne.emenu.Item("EMode", true).GetValue<StringList>().SelectedIndex;
                            if (!target.CanMove ||
                                    (target.Spellbook.IsAutoAttacking))
                            {
                                E.CastOnUnit(target);
                            }

                            var hitchance = emenu.Item("EHitchance").GetValue<Slider>().Value;
                            var angle = 0.20 * hitchance;
                                const float travelDistance = 0.5f;
                                var alpha = new Vector2((float)(p.X + travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                    (float)(p.X + travelDistance * Math.Sin(Math.PI / 180 * angle)));
                                var beta = new Vector2((float)(p.X - travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                    (float)(p.X - travelDistance * Math.Sin(Math.PI / 180 * angle)));

                                for (var i = 15; i < pD; i += 100)
                                {
                                    if (pP.To2D().Extend(alpha,
                                            i)
                                        .To3D().IsCollisionable() || pP.To2D().Extend(beta, i).To3D().IsCollisionable())
                                {
                                    E.CastOnUnit(target);
                                }
                            }
                                
                            
                        }
                        break;
                    case 11:
                        {
                            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                            var pP = Player.ServerPosition;
                            var p = target.ServerPosition;
                            var pD = Vayne.emenu.Item("PushDistance").GetValue<Slider>().Value;
                           // var mode = Vayne.emenu.Item("EMode", true).GetValue<StringList>().SelectedIndex;
                            var prediction = Vayne.E.GetPrediction(target);
                            for (var i = 15; i < pD; i += 75)
                            {
                                var posCF = NavMesh.GetCollisionFlags(
                                    prediction.UnitPosition.To2D()
                                        .Extend(
                                            pP.To2D(),
                                            -i)
                                        .To3D());
                                if (posCF.HasFlag(CollisionFlags.Wall) || posCF.HasFlag(CollisionFlags.Building))
                                {
                                    
                                        E.CastOnUnit(target);
                                    
                                }
                            }
                            
                        }
                        break;
                }
            }
        }
    }
}


                
           
               
