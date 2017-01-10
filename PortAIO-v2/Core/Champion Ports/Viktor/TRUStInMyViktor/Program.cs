using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;

using EloBuddy; namespace Viktor
{
    public class Program
    {
        public const string CHAMP_NAME = "Viktor";
        private static readonly AIHeroClient player = ObjectManager.Player;

        // Spells
        private static Spell Q, W, E, R;
        private static readonly int maxRangeE = 1225;
        private static readonly int lengthE = 700;
        private static readonly int speedE = 1050;
        private static readonly int rangeE = 525;
        private static int lasttick = 0;
        private static Vector3 GapCloserPos;
        private static bool AttacksEnabled
        {
            get
            {
                if (keyLinks["comboActive"].Value.Active)
                {
                    return ((!Q.IsReady() || player.Mana < Q.Instance.SData.Mana) && (!E.IsReady() || player.Mana < E.Instance.SData.Mana) && (!boolLinks["qAuto"].Value || player.HasBuff("viktorpowertransferreturn")));
                }
                else if (keyLinks["harassActive"].Value.Active)
                {
                    return ((!Q.IsReady() || player.Mana < Q.Instance.SData.Mana) && (!E.IsReady() || player.Mana < E.Instance.SData.Mana));
                }
                return true;
            }
        }
        // Menu
        public static MenuWrapper menu;

        // Menu links
        public static Dictionary<string, MenuWrapper.BoolLink> boolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
        public static Dictionary<string, MenuWrapper.CircleLink> circleLinks = new Dictionary<string, MenuWrapper.CircleLink>();
        public static Dictionary<string, MenuWrapper.KeyBindLink> keyLinks = new Dictionary<string, MenuWrapper.KeyBindLink>();
        public static Dictionary<string, MenuWrapper.SliderLink> sliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();
        public static Dictionary<string, MenuWrapper.StringListLink> stringLinks = new Dictionary<string, MenuWrapper.StringListLink>();

        private static void OrbwalkingOnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target.Type == GameObjectType.AIHeroClient)
            {
                args.Process = AttacksEnabled;
            }
            else
                args.Process = true;

        }
        public static void Game_OnGameLoad()
        {
            // Champ validation
            if (player.ChampionName != CHAMP_NAME)
                return;




            // Define spells
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, rangeE);
            R = new Spell(SpellSlot.R, 700);

            // Finetune spells
            Q.SetTargetted(0.25f, 2000);
            W.SetSkillshot(0.5f, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0, 80, speedE, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 450f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Create menu
            SetupMenu();

            // Register events
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.OnNonKillableMinion += Orbwalking_OnNonKillableMinion;
            Orbwalking.BeforeAttack += OrbwalkingOnBeforeAttack;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }
        private static void Orbwalking_OnNonKillableMinion(AttackableUnit minion)
        {
            QLastHit((Obj_AI_Base)minion);
        }
        private static void QLastHit(Obj_AI_Base minion)
        {
            bool castQ = ((keyLinks["waveUseQLH"].Value.Active) || boolLinks["waveUseQ"].Value && keyLinks["waveActive"].Value.Active);
            if (castQ)
            {
                var distance = Geometry.Distance(player, minion);
                var t = 250 + (int)distance / 2;
                var predHealth = HealthPrediction.GetHealthPrediction(minion, t, 0);
                // Console.WriteLine(" Distance: " + distance + " timer : " + t + " health: " + predHealth);
                if (predHealth > 0 && Q.IsKillable(minion))
                {
                    Q.Cast(minion);
                }
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Combo
            if (keyLinks["comboActive"].Value.Active)
                OnCombo();
            // Harass�
            if (keyLinks["harassActive"].Value.Active)
                OnHarass();
            // WaveClear
            if (keyLinks["waveActive"].Value.Active)
                OnWaveClear();

            if (keyLinks["jungleActive"].Value.Active)
                OnJungleClear();

            if (keyLinks["FleeActive"].Value.Active)
                Flee();

            if (keyLinks["forceR"].Value.Active)
            {
                if (R.IsReady())
                {
                    List<AIHeroClient> ignoredchamps = new List<AIHeroClient>();

                    foreach (var hero in HeroManager.Enemies)
                    {
                        if (!boolLinks["RU" + hero.ChampionName].Value)
                        {
                            ignoredchamps.Add(hero);
                        }
                    }
                    AIHeroClient RTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical, true, ignoredchamps);
                    R.Cast(RTarget);
                }

            }
            // Ultimate follow
            if (R.Instance.Name != "ViktorChaosStorm" && boolLinks["AutoFollowR"].Value && Environment.TickCount - lasttick > 0)
            {
                var stormT = TargetSelector.GetTarget(player, 1100, TargetSelector.DamageType.Magical);
                if (stormT != null)
                {
                    R.Cast(stormT.ServerPosition);
                    lasttick = Environment.TickCount + 500;
                }
            }
        }

        private static void OnCombo()
        {

            try
            {


                bool useQ = boolLinks["comboUseQ"].Value && Q.IsReady();
                bool useW = boolLinks["comboUseW"].Value && W.IsReady();
                bool useE = boolLinks["comboUseE"].Value && E.IsReady();
                bool useR = boolLinks["comboUseR"].Value && R.IsReady();

                bool killpriority = boolLinks["spPriority"].Value && R.IsReady();
                bool rKillSteal = boolLinks["rLastHit"].Value;
                AIHeroClient Etarget = TargetSelector.GetTarget(maxRangeE, TargetSelector.DamageType.Magical);
                AIHeroClient Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                AIHeroClient RTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                if (killpriority && Qtarget != null & Etarget != null && Etarget != Qtarget && ((Etarget.Health > TotalDmg(Etarget, false, true, false, false)) || (Etarget.Health > TotalDmg(Etarget, false, true, true, false) && Etarget == RTarget)) && Qtarget.Health < TotalDmg(Qtarget, true, true, false, false))
                {
                    Etarget = Qtarget;
                }

                if (RTarget != null && rKillSteal && useR && boolLinks["RU" + RTarget.ChampionName].Value)
                {
                    if (TotalDmg(RTarget, true, true, false, false) < RTarget.Health && TotalDmg(RTarget, true, true, true, true) > RTarget.Health)
                    {
                        R.Cast(RTarget.ServerPosition);
                    }
                }


                if (useE)
                {
                    if (Etarget != null)
                        PredictCastE(Etarget);
                }
                if (useQ)
                {

                    if (Qtarget != null)
                        Q.Cast(Qtarget);
                }
                if (useW)
                {
                    var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

                    if (t != null)
                    {
                        if (t.Path.Count() < 2)
                        {
                            if (t.HasBuffOfType(BuffType.Slow))
                            {
                                if (W.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                                    if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                                        return;
                            }
                            if (t.CountEnemiesInRange(250) > 2)
                            {
                                if (W.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                                    if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                                        return;
                            }
                        }
                    }
                }
                if (useR && R.Instance.Name == "ViktorChaosStorm" && player.CanCast && !player.Spellbook.IsCastingSpell)
                {

                    foreach (var unit in HeroManager.Enemies.Where(h => h.IsValidTarget(R.Range)))
                    {
                        R.CastIfWillHit(unit, stringLinks["HitR"].Value.SelectedIndex + 1);

                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.ToString());
            }
        }

        private static void Flee()
        {
            Orbwalking.MoveTo(Game.CursorPos);
            if (!Q.IsReady() || !(player.HasBuff("viktorqaug") || player.HasBuff("viktorqeaug") || player.HasBuff("viktorqwaug") || player.HasBuff("viktorqweaug")))
            {
                return;
            }
            var closestminion = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly).MinOrDefault(m => player.Distance(m));
            var closesthero = HeroManager.Enemies.MinOrDefault(m => player.Distance(m) < Q.Range);
            if (closestminion.IsValidTarget(Q.Range))
            {
                Q.Cast(closestminion);
            }
            else if (closesthero.IsValidTarget(Q.Range))
            {
                Q.Cast(closesthero);

            }
        }


        private static void OnHarass()
        {
            // Mana check
            if ((player.Mana / player.MaxMana) * 100 < sliderLinks["harassMana"].Value.Value)
                return;
            bool useE = boolLinks["harassUseE"].Value && E.IsReady();
            bool useQ = boolLinks["harassUseQ"].Value && Q.IsReady();
            if (useQ)
            {
                var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (qtarget != null)
                    Q.Cast(qtarget);
            }
            if (useE)
            {
                var harassrange = sliderLinks["eDistance"].Value.Value;
                var target = TargetSelector.GetTarget(harassrange, TargetSelector.DamageType.Magical);

                if (target != null)
                    PredictCastE(target);
            }
        }

        private static void OnWaveClear()
        {
            // Mana check
            if ((player.Mana / player.MaxMana) * 100 < sliderLinks["waveMana"].Value.Value)
                return;

            bool useQ = boolLinks["waveUseQ"].Value && Q.IsReady();
            bool useE = boolLinks["waveUseE"].Value && E.IsReady();

            if (useQ)
            {
                foreach (var minion in MinionManager.GetMinions(player.Position, player.AttackRange))
                {
                    if (Q.IsKillable(minion) && minion.CharData.BaseSkinName.Contains("Siege"))
                    {
                        QLastHit(minion);
                        break;
                    }
                }
            }

            if (useE)
                PredictCastMinionE();
        }

        private static void OnJungleClear()
        {
            // Mana check
            if ((player.Mana / player.MaxMana) * 100 < sliderLinks["waveMana"].Value.Value)
                return;

            bool useQ = boolLinks["waveUseQ"].Value && Q.IsReady();
            bool useE = boolLinks["waveUseE"].Value && E.IsReady();

            if (useQ)
            {
                foreach (var minion in MinionManager.GetMinions(player.Position, player.AttackRange, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth))
                {
                    Q.Cast(minion);
                }
            }

            if (useE)
                PredictCastMinionEJungle();
        }

        public static FarmLocation GetBestLaserFarmLocation(bool jungle)
        {
            var bestendpos = new Vector2();
            var beststartpos = new Vector2();
            var minionCount = 0;
            List<Obj_AI_Base> allminions;
            var minimalhit = sliderLinks["waveNumE"].Value.Value;
            if (!jungle)
            {
                allminions = MinionManager.GetMinions(maxRangeE);

            }
            else
            {
                allminions = MinionManager.GetMinions(maxRangeE, MinionTypes.All, MinionTeam.Neutral);
            }
            var minionslist = (from mnion in allminions select mnion.Position.To2D()).ToList<Vector2>();
            var posiblePositions = new List<Vector2>();
            posiblePositions.AddRange(minionslist);
            var max = posiblePositions.Count;
            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (posiblePositions[j] != posiblePositions[i])
                    {
                        posiblePositions.Add((posiblePositions[j] + posiblePositions[i]) / 2);
                    }
                }
            }

            foreach (var startposminion in allminions.Where(m => player.Distance(m) < rangeE))
            {
                var startPos = startposminion.Position.To2D();

                foreach (var pos in posiblePositions)
                {
                    if (pos.Distance(startPos, true) <= lengthE * lengthE)
                    {
                        var endPos = startPos + lengthE * (pos - startPos).Normalized();

                        var count =
                            minionslist.Count(pos2 => pos2.Distance(startPos, endPos, true, true) <= 140 * 140);

                        if (count >= minionCount)
                        {
                            bestendpos = endPos;
                            minionCount = count;
                            beststartpos = startPos;
                        }

                    }
                }
            }
            if ((!jungle && minimalhit < minionCount) || (jungle && minionCount > 0))
            {
                //Console.WriteLine("MinimalHits: " + minimalhit + "\n Startpos: " + beststartpos + "\n Count : " + minionCount);
                return new FarmLocation(beststartpos, bestendpos, minionCount);
            }
            else
            {
                return new FarmLocation(beststartpos, bestendpos, 0);
            }
        }



        private static bool PredictCastMinionEJungle()
        {
            var farmLocation = GetBestLaserFarmLocation(true);

            if (farmLocation.MinionsHit > 0)
            {
                CastE(farmLocation.Position1, farmLocation.Position2);
                return true;
            }

            return false;
        }

        public struct FarmLocation
        {
            /// <summary>
            /// The minions hit
            /// </summary>
            public int MinionsHit;

            /// <summary>
            /// The start position
            /// </summary>
            public Vector2 Position1;


            /// <summary>
            /// The end position
            /// </summary>
            public Vector2 Position2;

            /// <summary>
            /// Initializes a new instance of the <see cref="FarmLocation"/> struct.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="minionsHit">The minions hit.</param>
            public FarmLocation(Vector2 startpos, Vector2 endpos, int minionsHit)
            {
                Position1 = startpos;
                Position2 = endpos;
                MinionsHit = minionsHit;
            }
        }
        private static bool PredictCastMinionE()
        {
            var farmLoc = GetBestLaserFarmLocation(false);
            if (farmLoc.MinionsHit > 0)
            {
                Console.WriteLine("Minion amount: " + farmLoc.MinionsHit + "\n Startpos: " + farmLoc.Position1 + "\n EndPos: " + farmLoc.Position2);

                CastE(farmLoc.Position1, farmLoc.Position2);
                return true;
            }

            return false;
        }


        private static void PredictCastE(AIHeroClient target)
        {
            // Helpers
            bool inRange = Vector2.DistanceSquared(target.ServerPosition.To2D(), player.Position.To2D()) < E.Range * E.Range;
            PredictionOutput prediction;
            bool spellCasted = false;

            // Positions
            Vector3 pos1, pos2;

            // Champs
            var nearChamps = (from champ in ObjectManager.Get<AIHeroClient>() where champ.IsValidTarget(maxRangeE) && target != champ select champ).ToList();
            var innerChamps = new List<AIHeroClient>();
            var outerChamps = new List<AIHeroClient>();
            foreach (var champ in nearChamps)
            {
                if (Vector2.DistanceSquared(champ.ServerPosition.To2D(), player.Position.To2D()) < E.Range * E.Range)
                    innerChamps.Add(champ);
                else
                    outerChamps.Add(champ);
            }

            // Minions
            var nearMinions = MinionManager.GetMinions(player.Position, maxRangeE);
            var innerMinions = new List<Obj_AI_Base>();
            var outerMinions = new List<Obj_AI_Base>();
            foreach (var minion in nearMinions)
            {
                if (Vector2.DistanceSquared(minion.ServerPosition.To2D(), player.Position.To2D()) < E.Range * E.Range)
                    innerMinions.Add(minion);
                else
                    outerMinions.Add(minion);
            }

            // Main target in close range
            if (inRange)
            {
                // Get prediction reduced speed, adjusted sourcePosition
                E.Speed = speedE * 0.9f;
                E.From = target.ServerPosition + (Vector3.Normalize(player.Position - target.ServerPosition) * (lengthE * 0.1f));
                prediction = E.GetPrediction(target);
                E.From = player.Position;

                // Prediction in range, go on
                if (prediction.CastPosition.Distance(player.Position) < E.Range)
                    pos1 = prediction.CastPosition;
                // Prediction not in range, use exact position
                else
                {
                    pos1 = target.ServerPosition;
                    E.Speed = speedE;
                }

                // Set new sourcePosition
                E.From = pos1;
                E.RangeCheckFrom = pos1;

                // Set new range
                E.Range = lengthE;

                // Get next target
                if (nearChamps.Count > 0)
                {
                    // Get best champion around
                    var closeToPrediction = new List<AIHeroClient>();
                    foreach (var enemy in nearChamps)
                    {
                        // Get prediction
                        prediction = E.GetPrediction(enemy);
                        // Validate target
                        if (prediction.Hitchance >= HitChance.High && Vector2.DistanceSquared(pos1.To2D(), prediction.CastPosition.To2D()) < (E.Range * E.Range) * 0.8)
                            closeToPrediction.Add(enemy);
                    }

                    // Champ found
                    if (closeToPrediction.Count > 0)
                    {
                        // Sort table by health DEC
                        if (closeToPrediction.Count > 1)
                            closeToPrediction.Sort((enemy1, enemy2) => enemy2.Health.CompareTo(enemy1.Health));

                        // Set destination
                        prediction = E.GetPrediction(closeToPrediction[0]);
                        pos2 = prediction.CastPosition;

                        // Cast spell
                        CastE(pos1, pos2);
                        spellCasted = true;
                    }
                }

                // Spell not casted
                if (!spellCasted)
                {
                    CastE(pos1, E.GetPrediction(target).CastPosition);
                }

                // Reset spell
                E.Speed = speedE;
                E.Range = rangeE;
                E.From = player.Position;
                E.RangeCheckFrom = player.Position;
            }

            // Main target in extended range
            else
            {
                // Radius of the start point to search enemies in
                float startPointRadius = 150;

                // Get initial start point at the border of cast radius
                Vector3 startPoint = player.Position + Vector3.Normalize(target.ServerPosition - player.Position) * rangeE;

                // Potential start from postitions
                var targets = (from champ in nearChamps where Vector2.DistanceSquared(champ.ServerPosition.To2D(), startPoint.To2D()) < startPointRadius * startPointRadius && Vector2.DistanceSquared(player.Position.To2D(), champ.ServerPosition.To2D()) < rangeE * rangeE select champ).ToList();
                if (targets.Count > 0)
                {
                    // Sort table by health DEC
                    if (targets.Count > 1)
                        targets.Sort((enemy1, enemy2) => enemy2.Health.CompareTo(enemy1.Health));

                    // Set target
                    pos1 = targets[0].ServerPosition;
                }
                else
                {
                    var minionTargets = (from minion in nearMinions where Vector2.DistanceSquared(minion.ServerPosition.To2D(), startPoint.To2D()) < startPointRadius * startPointRadius && Vector2.DistanceSquared(player.Position.To2D(), minion.ServerPosition.To2D()) < rangeE * rangeE select minion).ToList();
                    if (minionTargets.Count > 0)
                    {
                        // Sort table by health DEC
                        if (minionTargets.Count > 1)
                            minionTargets.Sort((enemy1, enemy2) => enemy2.Health.CompareTo(enemy1.Health));

                        // Set target
                        pos1 = minionTargets[0].ServerPosition;
                    }
                    else
                        // Just the regular, calculated start pos
                        pos1 = startPoint;
                }

                // Predict target position
                E.From = pos1;
                E.Range = lengthE;
                E.RangeCheckFrom = pos1;
                prediction = E.GetPrediction(target);

                // Cast the E
                if (prediction.Hitchance >= HitChance.High)
                    CastE(pos1, prediction.CastPosition);

                // Reset spell
                E.Range = rangeE;
                E.From = player.Position;
                E.RangeCheckFrom = player.Position;
            }

        }



        private static void CastE(Vector3 source, Vector3 destination)
        {
            E.Cast(source, destination);
        }

        private static void CastE(Vector2 source, Vector2 destination)
        {
            E.Cast(source, destination);
        }

        private static void Interrupter2_OnInterruptableTarget(AIHeroClient unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel >= Interrupter2.DangerLevel.High)
            {
                var useW = boolLinks["wInterrupt"].Value;
                var useR = boolLinks["rInterrupt"].Value;

                if (useW && W.IsReady() && unit.IsValidTarget(W.Range) &&
                    (Game.Time + 1.5 + W.Delay) >= args.EndTime)
                {
                    if (W.Cast(unit) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
                else if (useR && unit.IsValidTarget(R.Range) && R.Instance.Name == "ViktorChaosStorm")
                {
                    R.Cast(unit);
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (boolLinks["miscGapcloser"].Value && W.IsInRange(gapcloser.End))
            {
                GapCloserPos = gapcloser.End;
                if (Geometry.Distance(gapcloser.Start, gapcloser.End) > gapcloser.Sender.Spellbook.GetSpell(gapcloser.Slot).SData.CastRangeDisplayOverride && gapcloser.Sender.Spellbook.GetSpell(gapcloser.Slot).SData.CastRangeDisplayOverride > 100)
                {
                    GapCloserPos = Geometry.Extend(gapcloser.Start, gapcloser.End, gapcloser.Sender.Spellbook.GetSpell(gapcloser.Slot).SData.CastRangeDisplayOverride);
                }
                W.Cast(GapCloserPos.To2D(), true);
            }
        }
        private static void AutoW()
        {
            if (!W.IsReady() || !boolLinks["autoW"].Value)
                return;

            var tPanth = HeroManager.Enemies.Find(h => h.IsValidTarget(W.Range) && h.HasBuff("Pantheon_GrandSkyfall_Jump"));
            if (tPanth != null)
            {
                if (W.Cast(tPanth) == Spell.CastStates.SuccessfullyCasted)
                    return;
            }

            foreach (var enemy in HeroManager.Enemies.Where(h => h.IsValidTarget(W.Range)))
            {
                if (enemy.HasBuff("rocketgrab2"))
                {
                    var t = HeroManager.Allies.Find(h => h.ChampionName.ToLower() == "blitzcrank" && h.Distance((AttackableUnit)player) < W.Range);
                    if (t != null)
                    {
                        if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                            return;
                    }
                }
                if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                         enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                         enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Suppression) ||
                         enemy.IsStunned || enemy.IsRecalling())
                {
                    if (W.Cast(enemy) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
                if (W.GetPrediction(enemy).Hitchance == HitChance.Immobile)
                {
                    if (W.Cast(enemy) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            // All circles
            foreach (var circle in circleLinks.Values.Select(link => link.Value))
            {
                if (circle.Active)
                    Render.Circle.DrawCircle(player.Position, circle.Radius, circle.Color);
            }
        }

        private static void ProcessLink(string key, object value)
        {
            if (value is MenuWrapper.BoolLink)
                boolLinks.Add(key, value as MenuWrapper.BoolLink);
            else if (value is MenuWrapper.CircleLink)
                circleLinks.Add(key, value as MenuWrapper.CircleLink);
            else if (value is MenuWrapper.KeyBindLink)
                keyLinks.Add(key, value as MenuWrapper.KeyBindLink);
            else if (value is MenuWrapper.SliderLink)
                sliderLinks.Add(key, value as MenuWrapper.SliderLink);
            else if (value is MenuWrapper.StringListLink)
                stringLinks.Add(key, value as MenuWrapper.StringListLink);
        }
        private static float TotalDmg(Obj_AI_Base enemy, bool useQ, bool useE, bool useR, bool qRange)
        {
            var qaaDmg = new Double[] { 20, 40, 60, 80, 100 };
            var damage = 0d;
            var rTicks = sliderLinks["rTicks"].Value.Value;
            bool inQRange = ((qRange && Orbwalking.InAutoAttackRange(enemy)) || qRange == false);
            //Base Q damage
            if (useQ && Q.IsReady() && inQRange)
            {
                damage += player.GetSpellDamage(enemy, SpellSlot.Q);
                damage += player.CalcDamage(enemy, Damage.DamageType.Magical, qaaDmg[Q.Level - 1] + 0.5 * player.TotalMagicalDamage + player.TotalAttackDamage);
            }

            // Q damage on AA
            if (useQ && !Q.IsReady() && player.HasBuff("viktorpowertransferreturn") && inQRange)
            {
                damage += player.CalcDamage(enemy, Damage.DamageType.Magical, qaaDmg[Q.Level - 1] + 0.5 * player.TotalMagicalDamage + player.TotalAttackDamage);
            }

            //E damage
            if (useE && E.IsReady())
            {
                if (player.HasBuff("viktoreaug") || player.HasBuff("viktorqeaug") || player.HasBuff("viktorqweaug"))
                    damage += player.GetSpellDamage(enemy, SpellSlot.E, 1);
                else
                    damage += player.GetSpellDamage(enemy, SpellSlot.E);
            }

            //R damage + 2 ticks
            if (useR && R.Level > 0 && R.IsReady() && R.Instance.Name == "ViktorChaosStorm")
            {
                damage += Damage.GetSpellDamage(player, enemy, SpellSlot.R, 1) * rTicks;
                damage += Damage.GetSpellDamage(player, enemy, SpellSlot.R);
            }

            // Ludens Echo damage
            if (Items.HasItem(3285))
                damage += player.CalcDamage(enemy, Damage.DamageType.Magical, 100 + player.FlatMagicDamageMod * 0.1);

            //sheen damage
            if (Items.HasItem(3057))
                damage += player.CalcDamage(enemy, Damage.DamageType.Physical, 0.5 * player.BaseAttackDamage);

            //lich bane dmg
            if (Items.HasItem(3100))
                damage += player.CalcDamage(enemy, Damage.DamageType.Magical, 0.5 * player.FlatMagicDamageMod + 0.75 * player.BaseAttackDamage);

            return (float)damage;
        }
        private static float GetComboDamage(Obj_AI_Base enemy)
        {

            return TotalDmg(enemy, true, true, true, false);
        }
        private static void SetupMenu()
        {

            menu = new MenuWrapper("TRUSt in my " + CHAMP_NAME);
            // Combo
            var subMenu = menu.MainMenu.AddSubMenu("Combo");
            ProcessLink("comboUseQ", subMenu.AddLinkedBool("Use Q"));
            ProcessLink("comboUseW", subMenu.AddLinkedBool("Use W"));
            ProcessLink("comboUseE", subMenu.AddLinkedBool("Use E"));
            ProcessLink("comboUseR", subMenu.AddLinkedBool("Use R"));
            ProcessLink("qAuto", subMenu.AddLinkedBool("Dont autoattack without passive"));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("Combo active", 32, KeyBindType.Press));

            subMenu = menu.MainMenu.AddSubMenu("R config");
            ProcessLink("HitR", subMenu.AddLinkedStringList("Auto R if: ", new string[] { "1 target", "2 targets", "3 targets", "4 targets", "5 targets" }, 3));
            ProcessLink("AutoFollowR", subMenu.AddLinkedBool("Auto Follow R"));
            ProcessLink("rTicks", subMenu.AddLinkedSlider("Ultimate ticks to count", 2, 1, 14));


            subMenu = subMenu.AddSubMenu("R one target");
            ProcessLink("forceR", subMenu.AddLinkedKeyBind("Force R on target", 84, KeyBindType.Press));
            ProcessLink("rLastHit", subMenu.AddLinkedBool("1 target ulti"));
            foreach (var hero in HeroManager.Enemies)
            {
                ProcessLink("RU" + hero.ChampionName, subMenu.AddLinkedBool("Use R on: " + hero.ChampionName));
            }


            subMenu = menu.MainMenu.AddSubMenu("Test features");
            ProcessLink("spPriority", subMenu.AddLinkedBool("Prioritize kill over dmg"));


            // Harass
            subMenu = menu.MainMenu.AddSubMenu("Harass");
            ProcessLink("harassUseQ", subMenu.AddLinkedBool("Use Q"));
            ProcessLink("harassUseE", subMenu.AddLinkedBool("Use E"));
            ProcessLink("harassMana", subMenu.AddLinkedSlider("Mana usage in percent (%)", 30));
            ProcessLink("eDistance", subMenu.AddLinkedSlider("Harass range with E", maxRangeE, rangeE, maxRangeE));
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("Harass active", 'C', KeyBindType.Press));

            // WaveClear
            subMenu = menu.MainMenu.AddSubMenu("WaveClear");
            ProcessLink("waveUseQ", subMenu.AddLinkedBool("Use Q"));
            ProcessLink("waveUseE", subMenu.AddLinkedBool("Use E"));
            ProcessLink("waveNumE", subMenu.AddLinkedSlider("Minions to hit with E", 2, 1, 10));
            ProcessLink("waveMana", subMenu.AddLinkedSlider("Mana usage in percent (%)", 30));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("WaveClear active", 'V', KeyBindType.Press));
            ProcessLink("jungleActive", subMenu.AddLinkedKeyBind("JungleClear active", 'G', KeyBindType.Press));

            subMenu = menu.MainMenu.AddSubMenu("LastHit");
            ProcessLink("waveUseQLH", subMenu.AddLinkedKeyBind("Use Q", 'A', KeyBindType.Press));

            // Harass
            subMenu = menu.MainMenu.AddSubMenu("Flee");
            ProcessLink("FleeActive", subMenu.AddLinkedKeyBind("Flee mode", 'Z', KeyBindType.Press));

            // Misc
            subMenu = menu.MainMenu.AddSubMenu("Misc");
            ProcessLink("rInterrupt", subMenu.AddLinkedBool("Use R to interrupt dangerous spells"));
            ProcessLink("wInterrupt", subMenu.AddLinkedBool("Use W to interrupt dangerous spells"));
            ProcessLink("autoW", subMenu.AddLinkedBool("Use W to continue CC"));
            ProcessLink("miscGapcloser", subMenu.AddLinkedBool("Use W against gapclosers"));

            // Drawings
            subMenu = menu.MainMenu.AddSubMenu("Drawings");
            ProcessLink("drawRangeQ", subMenu.AddLinkedCircle("Q range", true, Color.FromArgb(150, Color.IndianRed), Q.Range));
            ProcessLink("drawRangeW", subMenu.AddLinkedCircle("W range", true, Color.FromArgb(150, Color.IndianRed), W.Range));
            ProcessLink("drawRangeE", subMenu.AddLinkedCircle("E range", false, Color.FromArgb(150, Color.DarkRed), E.Range));
            ProcessLink("drawRangeEMax", subMenu.AddLinkedCircle("E max range", true, Color.FromArgb(150, Color.OrangeRed), maxRangeE));
            ProcessLink("drawRangeR", subMenu.AddLinkedCircle("R range", false, Color.FromArgb(150, Color.Red), R.Range));
            ProcessLink("dmgdraw", subMenu.AddLinkedBool("Draw dmg on healthbar"));
            var dmgAfterComboItem = menu.MainMenu.MenuHandle.SubMenu("Dmg Drawing").AddItem(new MenuItem("dmgdraw", "Draw dmg on healthbar").SetValue(true));
            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.DamageToUnit = GetComboDamage;
            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = boolLinks["dmgdraw"].Value;
            dmgAfterComboItem.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                Console.WriteLine("menu changed");
                //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };


        }
    }
}