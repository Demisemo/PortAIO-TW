using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace myWorld.Library.SimpleEvade
{
    public static class Evader
    {
        /// <summary>
        /// Returns the posible evade points.
        /// </summary>
        public static List<Vector2> GetEvadePoints(int speed = -1,
            int delay = 0,
            bool isBlink = false,
            bool onlyGood = false)
        {
            speed = speed == -1 ? (int)ObjectManager.Player.MoveSpeed : speed;

            var goodCandidates = new List<Vector2>();
            var badCandidates = new List<Vector2>();

            var polygonList = new List<Geometry.Polygon>();

            var takeClosestPath = false;

            foreach (var skillshot in Initialization.DetectedSkillshots)
            {
                if (skillshot.Evade())
                {
                    if (skillshot.SpellData.TakeClosestPath && skillshot.IsDanger(Initialization.PlayerPosition))
                    {
                        takeClosestPath = true;
                    }

                    polygonList.Add(skillshot.EvadePolygon);
                }
            }

            //Create the danger polygon:
            var dangerPolygons = Geometry.ClipPolygons(polygonList).ToPolygons();
            var myPosition = Initialization.PlayerPosition;

            //Scan the sides of each polygon to find the safe point.
            foreach (var poly in dangerPolygons)
            {
                for (var i = 0; i <= poly.Points.Count - 1; i++)
                {
                    var sideStart = poly.Points[i];
                    var sideEnd = poly.Points[(i == poly.Points.Count - 1) ? 0 : i + 1];

                    var originalCandidate = myPosition.ProjectOn(sideStart, sideEnd).SegmentPoint;
                    var distanceToEvadePoint = Vector2.DistanceSquared(originalCandidate, myPosition);


                    if (distanceToEvadePoint < 600 * 600)
                    {
                        var sideDistance = Vector2.DistanceSquared(sideEnd, sideStart);
                        var direction = (sideEnd - sideStart).Normalized();

                        var s = (distanceToEvadePoint < 200 * 200 && sideDistance > 90 * 90)
                            ? Config.DiagonalEvadePointsCount
                            : 0;
                        for (var j = -s; j <= s; j++)
                        {
                            var candidate = originalCandidate + j * Config.DiagonalEvadePointsStep * direction;
                            var pathToPoint = ObjectManager.Player.GetPath(candidate.To3D()).To2DList();

                            if (!isBlink)
                            {
                                if (Initialization.IsSafePath(pathToPoint, Config.EvadingFirstTimeOffset, speed, delay).IsSafe)
                                {
                                    goodCandidates.Add(candidate);
                                }

                                if (
                                    Initialization.IsSafePath(pathToPoint, Config.EvadingSecondTimeOffset, speed, delay).IsSafe &&
                                    j == 0)
                                {
                                    badCandidates.Add(candidate);
                                }
                            }
                            else
                            {
                                if (Initialization.IsSafeToBlink(
                                    pathToPoint[pathToPoint.Count - 1], Config.EvadingFirstTimeOffset, delay))
                                {
                                    goodCandidates.Add(candidate);
                                }

                                if (Initialization.IsSafeToBlink(
                                    pathToPoint[pathToPoint.Count - 1], Config.EvadingSecondTimeOffset, delay))
                                {
                                    badCandidates.Add(candidate);
                                }
                            }
                        }
                    }
                }
            }

            if (takeClosestPath)
            {
                if (goodCandidates.Count > 0)
                {
                    goodCandidates = new List<Vector2>
                    {
                        goodCandidates.MinOrDefault(vector2 => ObjectManager.Player.Distance(vector2, true))
                    };
                }

                if (badCandidates.Count > 0)
                {
                    badCandidates = new List<Vector2>
                    {
                        badCandidates.MinOrDefault(vector2 => ObjectManager.Player.Distance(vector2, true))
                    };
                }
            }

            return (goodCandidates.Count > 0) ? goodCandidates : (onlyGood ? new List<Vector2>() : badCandidates);
        }

        public static Vector2 GetClosestOutsidePoint(Vector2 from, List<Geometry.Polygon> polygons)
        {
            var result = new List<Vector2>();

            foreach (var poly in polygons)
            {
                for (var i = 0; i <= poly.Points.Count - 1; i++)
                {
                    var sideStart = poly.Points[i];
                    var sideEnd = poly.Points[(i == poly.Points.Count - 1) ? 0 : i + 1];

                    result.Add(from.ProjectOn(sideStart, sideEnd).SegmentPoint);
                }
            }
            return result.MinOrDefault(vector2 => vector2.Distance(from));
        }

        /// <summary>
        /// Returns the safe targets to cast escape spells.
        /// </summary>
        public static List<Obj_AI_Base> GetEvadeTargets(SpellValidTargets[] validTargets,
            int speed,
            int delay,
            float range,
            bool isBlink = false,
            bool onlyGood = false,
            bool DontCheckForSafety = false)
        {
            var badTargets = new List<Obj_AI_Base>();
            var goodTargets = new List<Obj_AI_Base>();
            var allTargets = new List<Obj_AI_Base>();
            foreach (var targetType in validTargets)
            {
                switch (targetType)
                {
                    case SpellValidTargets.AllyChampions:

                        foreach (var ally in ObjectManager.Get<AIHeroClient>())
                        {
                            if (ally.IsValidTarget(range, false) && !ally.IsMe && ally.IsAlly)
                            {
                                allTargets.Add(ally);
                            }
                        }
                        break;


                    case SpellValidTargets.AllyMinions:
                        allTargets.AddRange(
                            MinionManager.GetMinions(
                                ObjectManager.Player.Position, range, MinionTypes.All, MinionTeam.Ally));
                        break;

                    case SpellValidTargets.AllyWards:

                        foreach (var gameObject in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (gameObject.Name.ToLower().Contains("ward") && gameObject.IsValidTarget(range, false) &&
                                gameObject.Team == ObjectManager.Player.Team)
                            {
                                allTargets.Add(gameObject);
                            }
                        }
                        break;

                    case SpellValidTargets.EnemyChampions:
                        foreach (var enemy in ObjectManager.Get<AIHeroClient>())
                        {
                            if (enemy.IsValidTarget(range))
                            {
                                allTargets.Add(enemy);
                            }
                        }

                        break;

                    case SpellValidTargets.EnemyMinions:
                        allTargets.AddRange(
                            MinionManager.GetMinions(
                                ObjectManager.Player.Position, range, MinionTypes.All, MinionTeam.NotAlly));
                        break;

                    case SpellValidTargets.EnemyWards:

                        foreach (var gameObject in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (gameObject.Name.ToLower().Contains("ward") && gameObject.IsValidTarget(range))
                            {
                                allTargets.Add(gameObject);
                            }
                        }
                        break;
                }
            }

            foreach (var target in allTargets)
            {
                if (DontCheckForSafety || Initialization.IsSafe(target.ServerPosition.To2D()).IsSafe)
                {
                    if (isBlink)
                    {
                        if (Utils.TickCount - Initialization.LastWardJumpAttempt < 250 ||
                            Initialization.IsSafeToBlink(target.ServerPosition.To2D(), Config.EvadingFirstTimeOffset, delay))
                        {
                            goodTargets.Add(target);
                        }

                        if (Utils.TickCount - Initialization.LastWardJumpAttempt < 250 ||
                            Initialization.IsSafeToBlink(target.ServerPosition.To2D(), Config.EvadingSecondTimeOffset, delay))
                        {
                            badTargets.Add(target);
                        }
                    }
                    else
                    {
                        var pathToTarget = new List<Vector2>();
                        pathToTarget.Add(Initialization.PlayerPosition);
                        pathToTarget.Add(target.ServerPosition.To2D());

                        if (Utils.TickCount - Initialization.LastWardJumpAttempt < 250 ||
                            Initialization.IsSafePath(pathToTarget, Config.EvadingFirstTimeOffset, speed, delay).IsSafe)
                        {
                            goodTargets.Add(target);
                        }

                        if (Utils.TickCount - Initialization.LastWardJumpAttempt < 250 ||
                            Initialization.IsSafePath(pathToTarget, Config.EvadingSecondTimeOffset, speed, delay).IsSafe)
                        {
                            badTargets.Add(target);
                        }
                    }
                }
            }

            return (goodTargets.Count > 0) ? goodTargets : (onlyGood ? new List<Obj_AI_Base>() : badTargets);
        }
    }

}
