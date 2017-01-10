using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace hikiMarksmanRework.Core.Utilitys
{
    public static class VayneHelper
    {
        public static int PushDistance = Helper.VSlider("vayne.e.push.distance");
        public static List<Vector2> Points = new List<Vector2>();
        public static long LastCheck;
        public static void SafeTumble(AIHeroClient enemy)
        {
            var range = Orbwalking.GetRealAutoAttackRange(enemy);
            var path = Geometry.CircleCircleIntersection(ObjectManager.Player.ServerPosition.To2D(),
                Prediction.GetPrediction(enemy, 0.25f).UnitPosition.To2D(), LucianSpells.E.Range, range);

            if (path.Count() > 0)
            {
                var epos = path.MinOrDefault(x => x.Distance(Game.CursorPos));
                if (epos.To3D().UnderTurret(true) || epos.To3D().IsWall())
                {
                    return;
                }

                if (epos.To3D().CountEnemiesInRange(VayneSpells.Q.Range - 100) > 0)
                {
                    return;
                }
                VayneSpells.Q.Cast(epos);
            }
            if (path.Count() == 0)
            {
                var epos = ObjectManager.Player.ServerPosition.Extend(enemy.ServerPosition, -VayneSpells.Q.Range);
                if (epos.UnderTurret(true) || epos.IsWall())
                {
                    return;
                }

                // no intersection or target to close
                VayneSpells.Q.Cast(ObjectManager.Player.ServerPosition.Extend(enemy.ServerPosition, -VayneSpells.Q.Range));
            }
        }
        public static void TumbleCast()
        {
            switch (VayneMenu.Config.Item("vayne.q.type").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    foreach (var enemy in HeroManager.Enemies.Where(o=> o.IsValidTarget(ObjectManager.Player.AttackRange)))
                    {
                        SafeTumble(enemy);
                    }
                    break;
                case 1:
                    VayneSpells.Q.Cast(Game.CursorPos);
                    break;
            }
        }
        public static bool IsCollisionable(this Vector3 pos)
        {
            return NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall) ||
                (NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Building));
        }
        public static bool AsunasAllyFountain(Vector3 position)
        {
            float fountainRange = 750;
            var map = LeagueSharp.Common.Utility.Map.GetMap();
            if (map != null && map.Type == LeagueSharp.Common.Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1050;
            }
            return
                ObjectManager.Get<GameObject>().Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly).Any(spawnPoint => Vector2.Distance(position.To2D(), spawnPoint.Position.To2D()) < fountainRange);
        }

        public static void PradaSmart(AIHeroClient hero)
        {
            var pP = ObjectManager.Player.ServerPosition;
            var p = hero.ServerPosition;
            var pD = PushDistance;
            if ((p.Extend(pP, -pD).IsCollisionable() || p.Extend(pP, -pD / 2f).IsCollisionable() ||
                 p.Extend(pP, -pD / 3f).IsCollisionable()))
            {
                if (!hero.CanMove ||
                    (hero.Spellbook.IsAutoAttacking))
                    VayneSpells.E.Cast(hero);
            }
            var enemiesCount = ObjectManager.Player.CountEnemiesInRange(1200);
            if (enemiesCount > 1 && enemiesCount <= 3)
            {
                var prediction = VayneSpells.E.GetPrediction(hero);
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
                        VayneSpells.E.Cast(hero);
                    }
                }
                
            }
            else
            {
                var hitchance = 50;
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
                        VayneSpells.E.Cast(hero);
                    }
                }
                
            }
        }

        public static void VhrBasic(AIHeroClient hero)
        {
            var ePred = VayneSpells.E.GetPrediction(hero);
            int pushDist = PushDistance;
            var finalPosition = ePred.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -pushDist).To3D();

            for (int i = 1; i < pushDist; i += (int)hero.BoundingRadius)
            {
                Vector3 loc3 = ePred.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -i).To3D();

                if (loc3.IsWall() || AsunasAllyFountain(finalPosition))
                {
                    VayneSpells.E.Cast(hero);
                }  
            }
        }

        public static void Shine(AIHeroClient hero)
        {
            var pushDistance = PushDistance;
            var targetPosition = VayneSpells.E.GetPrediction(hero).UnitPosition;
            var pushDirection = (targetPosition - ObjectManager.Player.ServerPosition).Normalized();
            float checkDistance = pushDistance / 40f;
            for (int i = 0; i < 40; i++)
            {
                Vector3 finalPosition = targetPosition + (pushDirection * checkDistance * i);
                var collFlags = NavMesh.GetCollisionFlags(finalPosition);
                if (collFlags.HasFlag(CollisionFlags.Wall) || collFlags.HasFlag(CollisionFlags.Building)) //not sure about building, I think its turrets, nexus etc
                {
                    VayneSpells.E.Cast(hero);
                }
            }
        }

        public static void MarksmanCondemn (AIHeroClient hero)
        {
            for (var i = 1; i < 8; i++)
            {
                var targetBehind = hero.Position + Vector3.Normalize(hero.ServerPosition - ObjectManager.Player.Position) * i * 50;

                if (targetBehind.IsWall() && hero.IsValidTarget(VayneSpells.E.Range))
                {
                    VayneSpells.E.Cast(hero);
                }
            }
        }

        public static void SharpShooter(AIHeroClient hero)
        {
            var pP = ObjectManager.Player.ServerPosition;
            var p = hero.ServerPosition;
            var pD = PushDistance;
            var prediction = VayneSpells.E.GetPrediction(hero);
            for (var i = 15; i < pD; i += 100)
            {
                var posCf = NavMesh.GetCollisionFlags(prediction.UnitPosition.To2D().Extend(pP.To2D(), -i).To3D());
                if (posCf.HasFlag(CollisionFlags.Wall) || posCf.HasFlag(CollisionFlags.Building))
                {
                    VayneSpells.E.Cast(hero);
                }
            }
        }

        public static void Condemn360(AIHeroClient hero, Vector2 pos = new Vector2())
        {
            if (hero.HasBuffOfType(BuffType.SpellImmunity) || hero.HasBuffOfType(BuffType.SpellShield) ||
                LastCheck + 50 > Environment.TickCount || ObjectManager.Player.IsDashing())
            {
                return;
            } 
            var prediction = VayneSpells.E.GetPrediction(hero);
            var predictionsList = pos.IsValid() ? new List<Vector3>() { pos.To3D() } : new List<Vector3>
                        {
                            hero.ServerPosition,
                            hero.Position,
                            prediction.CastPosition,
                            prediction.UnitPosition
                        };

            var wallsFound = 0;
            Points = new List<Vector2>();
            foreach (var position in predictionsList)
            {
                for (var i = 0; i < PushDistance; i += (int)hero.BoundingRadius) // 420 = push distance
                {
                    var cPos = ObjectManager.Player.Position.Extend(position, ObjectManager.Player.Distance(position) + i).To2D();
                    Points.Add(cPos);
                    if (NavMesh.GetCollisionFlags(cPos.To3D()).HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(cPos.To3D()).HasFlag(CollisionFlags.Building))
                    {
                        wallsFound++;
                        break;
                    }
                }
            }
            if ((wallsFound / predictionsList.Count) >= 33 / 100f)
            {
                VayneSpells.E.Cast(hero);
            }
        }

        public static void CondemnCast()
        {
            switch (VayneMenu.Config.Item("vayne.e.type").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(VayneSpells.E.Range)))
                    {
                        PradaSmart(enemy);
                    }
                    break;
                case 1:
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(VayneSpells.E.Range)))
                    {
                       VhrBasic(enemy);
                    }
                    break;
                case 2:
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(VayneSpells.E.Range)))
                    {
                        Shine(enemy);
                    }
                    break;
                case 3:
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(VayneSpells.E.Range)))
                    {
                        MarksmanCondemn(enemy);
                    }
                    break;
                case 4:
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(VayneSpells.E.Range)))
                    {
                        SharpShooter(enemy);
                    }
                    break;
                case 5:
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(VayneSpells.E.Range)))
                    {
                        Condemn360(enemy);
                    }
                    break;
            }
        }
        public static void VhrBasicJungleCondemn (Obj_AI_Minion hero)
        {
            var ePred = VayneSpells.E.GetPrediction(hero);
            int pushDist = PushDistance;
            var finalPosition = ePred.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -pushDist).To3D();

            for (int i = 1; i < pushDist; i += (int)hero.BoundingRadius)
            {
                Vector3 loc3 = ePred.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -i).To3D();

                if (loc3.IsWall() || AsunasAllyFountain(finalPosition))
                {
                    VayneSpells.E.Cast(hero);
                }
            }
        }
       
    }
}
