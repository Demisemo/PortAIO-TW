using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace KindredSpirits
{
    internal static class UltimateManager
    {
        private static Menu Menu
        {
            get { return Program.Menu.SubMenu("Spells").SubMenu("R"); }
        }

        private static IEnumerable<AIHeroClient> Allies
        {
            get { return HeroManager.Allies; }
        }

        private static IEnumerable<AIHeroClient> Enemies
        {
            get { return HeroManager.Enemies; }
        }

        private static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        private static float UltimateRadius
        {
            get { return SpellManager.R.Instance.SData.CastRadius; }
        }

        private static int MinAllies
        {
            get { return Menu.Item("SavingAllies").GetValue<Slider>().Value; }
        }

        private static int MaxEnemies
        {
            get { return Menu.Item("SavingEnemies").GetValue<Slider>().Value; }
        }

        public static void Initialize()
        {
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Menu.Item("RCombo").IsActive() || !SpellManager.R.IsReady() ||
                ObjectManager.Player.HealthPercent > Menu.Item("RSelf").GetValue<Slider>().Value)
            {
                return;
            }

            SpellManager.R.CastOnUnit(ObjectManager.Player);
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead || !Menu.Item("RCombo").IsActive() || !SpellManager.R.IsReady())
            {
                return;
            }

            var enemy = sender as AIHeroClient;
            var ally = args.Target as AIHeroClient;

            if (enemy == null || !enemy.IsValid || !enemy.IsEnemy || ally == null || !ally.IsValid || !ally.IsAlly)
            {
                return;
            }

            if (ally.IsMe)
            {
                var predictedHealth = Player.Health - enemy.GetSpellDamage(Player, args.Slot);
                var hpp = predictedHealth / Player.MaxHealth * 100;
                Console.WriteLine("TAKING DAMAGE: {0}", hpp);
                if (predictedHealth < 0 || hpp < Menu.Item("RSelf").GetValue<Slider>().Value)
                {
                    //Console.WriteLine("ULT");
                    SpellManager.R.CastOnUnit(Player);
                }
                return;
            }

            if (!ally.IsValidTarget(SpellManager.R.Range, false))
            {
                return;
            }

            var ultTarget = GetBestUltTarget();
            if (ultTarget == null || ultTarget.Target == null)
            {
                Console.WriteLine("BAD ULT TARGET");
            }
            if (ultTarget != null && ultTarget.Target != null && ultTarget.Target.IsValid)
            {
                Console.WriteLine("ULT UNIT");
                SpellManager.R.CastOnUnit(ultTarget.Target);
            }
        }

        private static UltTarget GetBestUltTarget()
        {
            var possibleAllies = Allies.Where(o => SpellManager.R.IsInRange(o));

            AIHeroClient bestAlly = null;
            var allyCount = 0;
            var enemyCount = 0;
            var allies = new List<AIHeroClient>();
            foreach (var ally in possibleAllies)
            {
                if (!ally.IsMe && !Menu.Item("R" + ally.ChampionName).IsActive())
                {
                    continue;
                }

                var alliesInRange =
                    Allies.Where(
                        o =>
                            !ally.Equals(o) && ally.Distance(o) <= UltimateRadius &&
                            o.HealthPercent <
                            (o.IsMe
                                ? Menu.Item("RSelf").GetValue<Slider>().Value
                                : Menu.Item("RHP" + o.ChampionName).GetValue<Slider>().Value));
                var count = alliesInRange.Count();
                if (count < MinAllies)
                {
                    continue;
                }
                Console.WriteLine("ALLIES IN RANGE: {0}/{1}", count, MinAllies);
                var enemiesInRange = Enemies.Count(o => ally.Distance(o) <= UltimateRadius);

                if (enemiesInRange > MaxEnemies)
                {
                    continue;
                }

                Console.WriteLine("ENEMIES IN RANGE: {0}/{1}", enemiesInRange, MaxEnemies);
                if (count > allyCount || (count == allyCount && enemyCount > enemiesInRange))
                {
                    bestAlly = ally;
                    allyCount = count;
                    allies = alliesInRange.ToList();
                    enemyCount = enemiesInRange;
                }
            }
            return new UltTarget(bestAlly, allies);
        }
    }

    public class UltTarget
    {
        public List<AIHeroClient> AlliesInRange;
        public AIHeroClient Target;

        public UltTarget(AIHeroClient target, List<AIHeroClient> allies)
        {
            Target = target;
            AlliesInRange = allies;
        }
    }
}