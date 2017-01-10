using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Lee_Sin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;

    /// <summary>
    /// The static mathematics class for lee sin x on n enemies hit.
    /// </summary>
    public static class Mathematics
    {
        /// <summary>
        /// Gets the best possible destination where the max amount of enemies will be hit.
        /// </summary>
        /// <param name="maxTravelDistance">
        /// The max travel distance of lee sin.
        /// </param>
        /// <param name="player">
        /// The player obj.
        /// </param>
        /// <param name="minHitRequirement">
        /// The min hit requirement, min amount of enemies to be hit.
        /// </param>
        /// <param name="enemies">
        /// The enemies.
        /// </param>
        /// <returns>
        /// The <see cref="Vector3"/>, position that is where lee should be to cast r, eg the destination.
        /// </returns>
        public static Vector3 GetWardFlashPositions(float maxTravelDistance, AIHeroClient player, byte minHitRequirement, List<AIHeroClient> enemies)
        {
            Vector3 destination = SelectBest(GetPositions(player, maxTravelDistance, minHitRequirement, enemies), player);
            return destination;
        }

        // Lazy kappa
        public static Vector3 SelectBest(List<Vector3> getPositionsResults, AIHeroClient player)
        {
            if (getPositionsResults.Count == 0)
            {
                return new Vector3(null);
            }

            return getPositionsResults[0];
        }

        public static List<Vector3> GetPositions(AIHeroClient player, float maxTravelDistance, byte minHitRequirement, List<AIHeroClient> enemies)
        {
            var polygons = GeneratePolygons(enemies);
            var removedDuplicates = RemoveDuplicates(polygons);
            var minHitFiltered = MinHitFilter(removedDuplicates, enemies, minHitRequirement);
            var positions = GeneratePositions(minHitFiltered, player);
            var travelRangeFilter = TravelRangeFilter(positions, maxTravelDistance, player);
            return travelRangeFilter;
        }

        // Best position order is maintained
        private static List<Vector3> TravelRangeFilter(List<Tuple<Geometry.Polygon.Rectangle, Vector3, Vector3>> generatePositionsResult, float maxTravelDistance, AIHeroClient player)
        {
            Vector3 leePos = player.ServerPosition;
            List<Vector3> results = new List<Vector3>();
            foreach (Tuple<Geometry.Polygon.Rectangle, Vector3, Vector3> tuple in generatePositionsResult)
            {
                if (leePos.Distance(tuple.Item2) <= maxTravelDistance)
                {
                    results.Add(tuple.Item2);
                }

                if (leePos.Distance(tuple.Item3) <= maxTravelDistance)
                {
                    results.Add(tuple.Item3);
                }
            }
            return results;
        }

        private static List<Tuple<Geometry.Polygon.Rectangle, Vector3, Vector3>> GeneratePositions(List<Tuple<Geometry.Polygon.Rectangle, byte, List<AIHeroClient>>> minHitFilterResults, AIHeroClient player)
        {
            Vector3 leePos = player.ServerPosition;
            foreach (Tuple<Geometry.Polygon.Rectangle, byte, List<AIHeroClient>> tuple in minHitFilterResults)
            {
                tuple.Item3.OrderBy(e => e.Distance(leePos));
            }

            var results = new List<Tuple<Geometry.Polygon.Rectangle, Vector3, Vector3>>();
            foreach (Tuple<Geometry.Polygon.Rectangle, byte, List<AIHeroClient>> tuple in minHitFilterResults)
            {
                Tuple<Vector3, Vector3> sres = SGeneratePosition(tuple.Item1, tuple.Item3.Last().ServerPosition, tuple.Item3.First().ServerPosition);//polygon, farthest eg last, closest eg first since we ordered them
                results.Add(new Tuple<Geometry.Polygon.Rectangle, Vector3, Vector3>(tuple.Item1, sres.Item1, sres.Item2));
            }
            return results;
        }

        private static Tuple<Vector3, Vector3> SGeneratePosition(Geometry.Polygon.Rectangle polygon, Vector3 fatherst, Vector3 closest)
        {
            Vector3 v0 = MoveVector(fatherst, closest, -187.5F);
            Vector3 v1 = MoveVector(closest, fatherst, -187.5F);
            return new Tuple<Vector3, Vector3>(v0, v1);
        }

        private static List<Tuple<Geometry.Polygon.Rectangle, byte, List<AIHeroClient>>> MinHitFilter(List<Geometry.Polygon.Rectangle> polygons, List<AIHeroClient> enemies, byte minHit)
        {
            List<Tuple<Geometry.Polygon.Rectangle, byte, List<AIHeroClient>>> results = new List<Tuple<Geometry.Polygon.Rectangle, byte, List<AIHeroClient>>>();
            foreach (Geometry.Polygon.Rectangle polygon in polygons)
            {
                byte count = 0x0;
                List<AIHeroClient> inPoly = new List<AIHeroClient>();
                foreach (AIHeroClient tar in enemies)
                {
                    if (polygon.IsInside(tar))
                    {
                        count++;
                        inPoly.Add(tar);
                    }
                }
                if (count >= minHit)
                {
                    results.Add(new Tuple<Geometry.Polygon.Rectangle, byte, List<AIHeroClient>>(polygon, count, inPoly));
                }
            }
            return results.OrderBy(i => i.Item2).ToList();
        }

        private static List<Geometry.Polygon.Rectangle> RemoveDuplicates(List<Geometry.Polygon.Rectangle> input)
        {
            List<Geometry.Polygon.Rectangle> results = new List<Geometry.Polygon.Rectangle>();
            foreach (Geometry.Polygon.Rectangle rectangle in input)
            {
                if (!results.Contains(rectangle))
                {
                    results.Add(rectangle);
                }
            }
            return results;
        }

        private static List<Geometry.Polygon.Rectangle> GeneratePolygons(List<AIHeroClient> enemies)
        {
            List<Geometry.Polygon.Rectangle> results = new List<Geometry.Polygon.Rectangle>();
            foreach (AIHeroClient enemy in enemies)
            {
                AIHeroClient tar = enemy;
                foreach (AIHeroClient end in enemies.Where(e => e != tar))
                {
                    results.Add(
                        SGeneratePolygon(
                            tar.ServerPosition,
                            end.ServerPosition,
                            (tar.BoundingRadius + enemy.BoundingRadius) / 2));
                }
            }
            return results;
        }

        private static Geometry.Polygon.Rectangle SGeneratePolygon(Vector3 start, Vector3 end, float boundingBWidth)
        {
            Vector3 nstart = MoveVector(start, end);
            Vector3 nend = MoveVector(end, start);
            return new Geometry.Polygon.Rectangle(nstart, nend, boundingBWidth);
        }

        // Distance from start to end is t = 1
        public static Vector3 MoveVector(Vector3 start, Vector3 end, float distance = 2250F)
        {
            float t = distance / (start.Distance(end));
            Vector3 direction = new Vector3(end.X - start.X, end.Y - start.Y, end.Z - start.Z);
            float x = start.X + (direction.X * t);
            float y = start.Y + (direction.Y * t);
            float z = start.Z + (direction.Z * t);
            return new Vector3(x, y, z);
        }
    }
}