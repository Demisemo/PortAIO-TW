using EloBuddy; 
using LeagueSharp.Common; 
namespace Flowers_Fiora.Evade
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;

    internal static class Collision
    {
        private static int WallCastT;
        private static Vector2 YasuoWallCastedPos;

        public static void Init()
        {
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsValid && sender.Team == ObjectManager.Player.Team && args.SData.Name == "YasuoWMovingWall")
            {
                WallCastT = Utils.GameTimeTickCount;
                YasuoWallCastedPos = sender.ServerPosition.To2D();
            }
        }

        public static FastPredResult FastPrediction(Vector2 from, Obj_AI_Base unit, int delay, int speed)
        {
            var tDelay = delay / 1000f + from.Distance(unit) / speed;
            var d = tDelay * unit.MoveSpeed;
            var path = unit.Path.ToList().To2D();

            if (path.PathLength() > d)
            {
                return new FastPredResult
                {
                    IsMoving = true,
                    CurrentPos = unit.ServerPosition.To2D(),
                    PredictedPos = path.CutPath((int) d)[0]
                };
            }

            if (path.Count == 0)
            {
                return new FastPredResult
                {
                    IsMoving = false,
                    CurrentPos = unit.ServerPosition.To2D(),
                    PredictedPos = unit.ServerPosition.To2D()
                };
            }

            return new FastPredResult
            {
                IsMoving = false,
                CurrentPos = path[path.Count - 1],
                PredictedPos = path[path.Count - 1]
            };
        }

        public static Vector2 GetCollisionPoint(Skillshot skillshot)
        {
            var collisions = new List<DetectedCollision>();
            var from = skillshot.GetMissilePosition(0);

            skillshot.ForceDisabled = false;

            foreach (var cObject in skillshot.SpellData.CollisionObjects)
            {
                switch (cObject)
                {
                    case CollisionObjectTypes.Minion:
                        foreach (var minion in
                            MinionManager.GetMinions(
                                from.To3D(), 1200, MinionTypes.All,
                                skillshot.Unit.Team == ObjectManager.Player.Team
                                    ? MinionTeam.NotAlly
                                    : MinionTeam.NotAllyForEnemy))
                        {
                            var pred = FastPrediction(
                                from, minion,
                                Math.Max(0, skillshot.SpellData.Delay - (Utils.GameTimeTickCount - skillshot.StartTick)),
                                skillshot.SpellData.MissileSpeed);
                            var pos = pred.PredictedPos;
                            var w = skillshot.SpellData.RawRadius + (!pred.IsMoving ? minion.BoundingRadius - 15 : 0) -
                                    pos.Distance(from, skillshot.End, true);

                            if (w > 0)
                            {
                                collisions.Add(
                                    new DetectedCollision
                                    {
                                        Position =
                                            pos.ProjectOn(skillshot.End, skillshot.Start).LinePoint +
                                            skillshot.Direction * 30,
                                        Unit = minion,
                                        Type = CollisionObjectTypes.Minion,
                                        Distance = pos.Distance(from),
                                        Diff = w,
                                    });
                            }
                        }
                        break;
                    case CollisionObjectTypes.Champions:
                        foreach (
                            var hero in
                            HeroManager.Allies.Where(x => !x.IsMe && x.Distance(ObjectManager.Player) <= 1200))
                        {
                            var pred = FastPrediction(
                                from, hero,
                                Math.Max(0, skillshot.SpellData.Delay - (Utils.GameTimeTickCount - skillshot.StartTick)),
                                skillshot.SpellData.MissileSpeed);
                            var pos = pred.PredictedPos;
                            var w = skillshot.SpellData.RawRadius + 30 - pos.Distance(from, skillshot.End, true);

                            if (w > 0)
                            {
                                collisions.Add(
                                    new DetectedCollision
                                    {
                                        Position =
                                            pos.ProjectOn(skillshot.End, skillshot.Start).LinePoint +
                                            skillshot.Direction * 30,
                                        Unit = hero,
                                        Type = CollisionObjectTypes.Minion,
                                        Distance = pos.Distance(from),
                                        Diff = w,
                                    });
                            }
                        }
                        break;

                    case CollisionObjectTypes.YasuoWall:
                        if (HeroManager.Allies.All(x => x.ChampionName != "Yasuo"))
                        {
                            break;
                        }

                        GameObject wall = null;

                        foreach (var gameObject in ObjectManager.Get<GameObject>())
                        {
                            if (gameObject.IsValid &&
                                System.Text.RegularExpressions.Regex.IsMatch(
                                    gameObject.Name, "_w_windwall.\\.troy",
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                wall = gameObject;
                            }
                        }

                        if (wall == null)
                        {
                            break;
                        }

                        var level = wall.Name.Substring(wall.Name.Length - 6, 1);
                        var wallWidth = 300 + 50 * Convert.ToInt32(level);
                        var wallDirection = (wall.Position.To2D() - YasuoWallCastedPos).Normalized().Perpendicular();
                        var wallStart = wall.Position.To2D() + wallWidth / 2 * wallDirection;
                        var wallEnd = wallStart - wallWidth * wallDirection;
                        var wallPolygon = new Geometry.Polygon.Rectangle(wallStart, wallEnd, 75);
                        var intersection = new Vector2();
                        var intersections = new List<Vector2>();

                        for (var i = 0; i < wallPolygon.Points.Count; i++)
                        {
                            var inter =
                                wallPolygon.Points[i].Intersection(
                                    wallPolygon.Points[i != wallPolygon.Points.Count - 1 ? i + 1 : 0], from,
                                    skillshot.End);
                            if (inter.Intersects)
                            {
                                intersections.Add(inter.Point);
                            }
                        }

                        if (intersections.Count > 0)
                        {
                            intersection = intersections.OrderBy(item => item.Distance(from)).ToList()[0];

                            var collisionT = Utils.GameTimeTickCount +
                                             Math.Max(
                                                 0,
                                                 skillshot.SpellData.Delay -
                                                 (Utils.GameTimeTickCount - skillshot.StartTick)) + 100 +
                                             1000 * intersection.Distance(from) / skillshot.SpellData.MissileSpeed;

                            if (collisionT - WallCastT < 4000)
                            {
                                if (skillshot.SpellData.Type != SkillShotType.SkillshotMissileLine)
                                {
                                    skillshot.ForceDisabled = true;
                                }

                                return intersection;
                            }
                        }
                        break;
                }
            }

            return collisions.Count > 0 ? collisions.OrderBy(c => c.Distance).ToList()[0].Position : new Vector2();
        }
    }
}