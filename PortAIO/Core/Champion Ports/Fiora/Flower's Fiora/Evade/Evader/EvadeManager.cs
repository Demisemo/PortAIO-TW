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
    using static Common.Common;
    using Geometry = GE;

    public static class EvadeManager
    {
        public const int SkillShotsExtraRadius = 9;
        public const int SkillShotsExtraRange = 20;
        public const int GridSize = 10;
        public const int EvadingFirstTimeOffset = 250;
        public const int EvadingSecondTimeOffset = 80;

        public static Menu Menu = Logic.Menu;

        public static List<Skillshot> DetectedSkillshots = new List<Skillshot>();

        public static void Init()
        {
            Collision.Init();

            Game.OnUpdate += OnUpdate;
            SkillshotDetector.OnDeleteMissile += OnDeleteMissile;
            SkillshotDetector.OnDetectSkillshot += OnDetectSkillshot;
        }

        private static void OnUpdate(EventArgs args)
        {
            DetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());

            foreach (var skillshot in DetectedSkillshots)
            {
                skillshot.OnUpdate();
            }

            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if (ObjectManager.Player.HasBuffOfType(BuffType.SpellImmunity) || ObjectManager.Player.HasBuffOfType(BuffType.SpellShield))
            {
                return;
            }

            var currentPath = ObjectManager.Player.Path.ToList().To2D();
            var safeResult = IsSafe(ObjectManager.Player.ServerPosition.To2D());
            var safePath = IsSafePath(currentPath, 100);

            if (!safePath.IsSafe)
            {
                if (!safeResult.IsSafe)
                {
                    TryToEvade(safeResult.SkillshotList, Game.CursorPos.To2D());
                }
            }
        }

        private static void TryToEvade(List<Skillshot> HitBy, Vector2 Pos)
        {
            var dangerLevel = 0;

            foreach (var skillshot in HitBy)
            {
                dangerLevel = Math.Max(dangerLevel, skillshot.GetValue<Slider>("DangerLevel").Value);
            }

            foreach (var evadeSpell in EvadeSpellDatabase.Spells)
            {
                if (evadeSpell.Enabled && evadeSpell.DangerLevel <= dangerLevel)
                {
                    if (evadeSpell.IsReady())
                    {
                        switch (evadeSpell.Slot)
                        {
                            case SpellSlot.Q:
                                var points = GetEvadePoints(evadeSpell.Speed, evadeSpell.Delay);

                                points.RemoveAll(
                                    item => item.Distance(ObjectManager.Player.ServerPosition) > evadeSpell.Range);

                                for (var i = 0; i < points.Count; i++)
                                {
                                    points[i] = ObjectManager.Player.ServerPosition.To2D().Extend(points[i], evadeSpell.Range);
                                }

                                for (var i = points.Count - 1; i > 0; i--)
                                {
                                    if (!IsSafe(points[i]).IsSafe)
                                    {
                                        points.RemoveAt(i);
                                    }
                                }

                                if (points.Count > 0)
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, Pos.Closest(points).To3D());
                                    return;
                                }
                                break;
                            case SpellSlot.W:
                                var skillShotList =
                                    HitBy.Where(
                                        x => x.SpellData.CollisionObjects.Contains(CollisionObjectTypes.YasuoWall));

                                if (skillShotList.Any())
                                {
                                    var willHitList =
                                        skillShotList.Where(
                                            x =>
                                                x.IsAboutToHit(
                                                    150 + evadeSpell.Delay,
                                                    ObjectManager.Player));

                                    if (willHitList.Any())
                                    {
                                        if (
                                            willHitList.OrderByDescending(
                                                    x => dangerLevel)
                                                .Any(
                                                    x =>
                                                        Logic.W.Cast(
                                                            ObjectManager.Player.ServerPosition.Extend(x.Start.To3D(),
                                                                300))))
                                        {
                                            return;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }


        private static void OnDeleteMissile(Skillshot skillshot, MissileClient missile)
        {
            if (skillshot.SpellData.SpellName == "VelkozQ")
            {
                var spellData = SpellDatabase.GetByName("VelkozQSplit");
                var direction = skillshot.Direction.Perpendicular();

                if (DetectedSkillshots.Count(s => s.SpellData.SpellName == "VelkozQSplit") == 0)
                {
                    for (var i = -1; i <= 1; i = i + 2)
                    {
                        var skillshotToAdd = new Skillshot(
                            DetectionType.ProcessSpell, spellData, Utils.GameTimeTickCount, missile.Position.To2D(),
                            missile.Position.To2D() + i * direction * spellData.Range, skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                    }
                }
            }
        }

        private static void OnDetectSkillshot(Skillshot skillshot)
        {
            var alreadyAdded = false;

            foreach (var item in DetectedSkillshots)
            {
                if (item.SpellData.SpellName == skillshot.SpellData.SpellName &&
                    item.Unit.NetworkId == skillshot.Unit.NetworkId &&
                    skillshot.Direction.AngleBetween(item.Direction) < 5 &&
                    (skillshot.Start.Distance(item.Start) < 100 || skillshot.SpellData.FromObjects.Length == 0))
                {
                    alreadyAdded = true;
                }
            }

            if (skillshot.Unit.Team == ObjectManager.Player.Team)
            {
                return;
            }

            if (skillshot.Start.Distance(ObjectManager.Player.ServerPosition.To2D()) >
                (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000) * 1.5)
            {
                return;
            }

            if (!alreadyAdded || skillshot.SpellData.DontCheckForDuplicates)
            {
                if (skillshot.DetectionType == DetectionType.ProcessSpell)
                {
                    if (skillshot.SpellData.MultipleNumber != -1)
                    {
                        var originalDirection = skillshot.Direction;

                        for (var i = -(skillshot.SpellData.MultipleNumber - 1) / 2;
                            i <= (skillshot.SpellData.MultipleNumber - 1) / 2;
                            i++)
                        {
                            var end = skillshot.Start +
                                      skillshot.SpellData.Range *
                                      originalDirection.Rotated(skillshot.SpellData.MultipleAngle * i);
                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                                skillshot.Unit);

                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "UFSlash")
                    {
                        skillshot.SpellData.MissileSpeed = 1600 + (int)skillshot.Unit.MoveSpeed;
                    }

                    if (skillshot.SpellData.SpellName == "SionR")
                    {
                        skillshot.SpellData.MissileSpeed = (int)skillshot.Unit.MoveSpeed;
                    }

                    if (skillshot.SpellData.Invert)
                    {
                        var newDirection = -(skillshot.End - skillshot.Start).Normalized();
                        var end = skillshot.Start + newDirection * skillshot.Start.Distance(skillshot.End);
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.Centered)
                    {
                        var start = skillshot.Start - skillshot.Direction * skillshot.SpellData.Range;
                        var end = skillshot.Start + skillshot.Direction * skillshot.SpellData.Range;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    var unit = skillshot.Unit as AIHeroClient;

                    if (unit != null && skillshot.SpellData.SpellName == "TaricE" && unit.ChampionName == "Taric")
                    {
                        var target =
                            HeroManager.AllHeroes.FirstOrDefault(
                                h => h.Team == skillshot.Unit.Team && h.IsVisible && h.HasBuff("taricwleashactive"));

                        if (target != null)
                        {
                            var start = target.ServerPosition.To2D();
                            var direction = (skillshot.OriginalEnd - start).Normalized();
                            var end = start + direction * skillshot.SpellData.Range;
                            var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick,
                                    start, end, target)
                            {
                                OriginalEnd = skillshot.OriginalEnd
                            };
                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                    }

                    if (skillshot.SpellData.SpellName == "SyndraE" || skillshot.SpellData.SpellName == "syndrae5")
                    {
                        var angle = 60;
                        var edge1 =
                            (skillshot.End - skillshot.Unit.ServerPosition.To2D()).Rotated(
                                -angle / 2 * (float)Math.PI / 180);
                        var edge2 = edge1.Rotated(angle * (float)Math.PI / 180);

                        var positions = new List<Vector2>();

                        var explodingQ = DetectedSkillshots.FirstOrDefault(s => s.SpellData.SpellName == "SyndraQ");

                        if (explodingQ != null)
                        {
                            positions.Add(explodingQ.End);
                        }

                        foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (minion.Name == "Seed" && !minion.IsDead && minion.Team != ObjectManager.Player.Team)
                            {
                                positions.Add(minion.ServerPosition.To2D());
                            }
                        }

                        foreach (var position in positions)
                        {
                            var v = position - skillshot.Unit.ServerPosition.To2D();
                            if (edge1.CrossProduct(v) > 0 && v.CrossProduct(edge2) > 0 &&
                                position.Distance(skillshot.Unit) < 800)
                            {
                                var start = position;
                                var end = skillshot.Unit.ServerPosition.To2D()
                                    .Extend(
                                        position,
                                        skillshot.Unit.Distance(position) > 200 ? 1300 : 1000);
                                var startTime = skillshot.StartTick;

                                startTime += (int)(150 + skillshot.Unit.Distance(position) / 2.5f);
                                var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, startTime, start, end,
                                    skillshot.Unit);
                                DetectedSkillshots.Add(skillshotToAdd);
                            }
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "MalzaharQ")
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 400;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 400;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "ZyraQ")
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 450;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 450;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "DianaArc")
                    {
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, SpellDatabase.GetByName("DianaArcArc"), skillshot.StartTick,
                            skillshot.Start, skillshot.End,
                            skillshot.Unit);

                        DetectedSkillshots.Add(skillshotToAdd);
                    }

                    if (skillshot.SpellData.SpellName == "ZiggsQ")
                    {
                        var d1 = skillshot.Start.Distance(skillshot.End);
                        var d2 = d1 * 0.4f;
                        var d3 = d2 * 0.69f;
                        var bounce1SpellData = SpellDatabase.GetByName("ZiggsQBounce1");
                        var bounce2SpellData = SpellDatabase.GetByName("ZiggsQBounce2");
                        var bounce1Pos = skillshot.End + skillshot.Direction * d2;
                        var bounce2Pos = bounce1Pos + skillshot.Direction * d3;

                        bounce1SpellData.Delay =
                            (int)(skillshot.SpellData.Delay + d1 * 1000f / skillshot.SpellData.MissileSpeed + 500);
                        bounce2SpellData.Delay =
                            (int)(bounce1SpellData.Delay + d2 * 1000f / bounce1SpellData.MissileSpeed + 500);

                        var bounce1 = new Skillshot(
                            skillshot.DetectionType, bounce1SpellData, skillshot.StartTick, skillshot.End, bounce1Pos,
                            skillshot.Unit);
                        var bounce2 = new Skillshot(
                            skillshot.DetectionType, bounce2SpellData, skillshot.StartTick, bounce1Pos, bounce2Pos,
                            skillshot.Unit);

                        DetectedSkillshots.Add(bounce1);
                        DetectedSkillshots.Add(bounce2);
                    }

                    if (skillshot.SpellData.SpellName == "ZiggsR")
                    {
                        skillshot.SpellData.Delay =
                            (int)(1500 + 1500 * skillshot.End.Distance(skillshot.Start) / skillshot.SpellData.Range);
                    }

                    if (skillshot.SpellData.SpellName == "JarvanIVDragonStrike")
                    {
                        var endPos = new Vector2();

                        foreach (var s in DetectedSkillshots)
                        {
                            if (s.Unit.NetworkId == skillshot.Unit.NetworkId && s.SpellData.Slot == SpellSlot.E)
                            {
                                var extendedE = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start,
                                    skillshot.End + skillshot.Direction * 100, skillshot.Unit);

                                if (!extendedE.IsSafe(s.End))
                                {
                                    endPos = s.End;
                                }
                                break;
                            }
                        }

                        foreach (var m in ObjectManager.Get<Obj_AI_Minion>())
                        {
                            if (m.BaseSkinName == "jarvanivstandard" && m.Team == skillshot.Unit.Team)
                            {
                                var extendedE = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start,
                                    skillshot.End + skillshot.Direction * 100, skillshot.Unit);

                                if (!extendedE.IsSafe(m.Position.To2D()))
                                {
                                    endPos = m.Position.To2D();
                                }
                                break;
                            }
                        }

                        if (endPos.IsValid())
                        {
                            skillshot = new Skillshot(DetectionType.ProcessSpell, SpellDatabase.GetByName("JarvanIVEQ"),
                                Utils.GameTimeTickCount, skillshot.Start, endPos, skillshot.Unit);
                            skillshot.End = endPos + 200 * (endPos - skillshot.Start).Normalized();
                            skillshot.Direction = (skillshot.End - skillshot.Start).Normalized();
                        }
                    }
                }

                if (skillshot.SpellData.SpellName == "OriannasQ")
                {
                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, SpellDatabase.GetByName("OriannaQend"), skillshot.StartTick,
                        skillshot.Start, skillshot.End,
                        skillshot.Unit);

                    DetectedSkillshots.Add(skillshotToAdd);
                }

                if (skillshot.SpellData.DisableFowDetection && skillshot.DetectionType == DetectionType.RecvPacket)
                {
                    return;
                }

                DetectedSkillshots.Add(skillshot);
            }
        }

        public static IsSafeResult IsSafe(Vector2 point)
        {
            var result = new IsSafeResult {SkillshotList = new List<Skillshot>()};

            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.Evade() && skillshot.IsDanger(point))
                {
                    result.SkillshotList.Add(skillshot);
                }
            }

            result.IsSafe = result.SkillshotList.Count == 0;

            return result;
        }

        public static SafePathResult IsSafePath(List<Vector2> path, int timeOffset, int speed = -1, int delay = 0, Obj_AI_Base unit = null)
        {
            var IsSafe = true;
            var intersections = new List<FoundIntersection>();
            var intersection = new FoundIntersection();

            foreach (var skillshot in DetectedSkillshots.Where(x => x.Evade()))
            {
                var sResult = skillshot.IsSafePath(path, timeOffset, speed, delay, unit);
                IsSafe = IsSafe && sResult.IsSafe;

                if (sResult.Intersection.Valid)
                {
                    intersections.Add(sResult.Intersection);
                }
            }

            if (!IsSafe)
            {
                var intersetion = intersections.MinOrDefault(o => o.Distance);
                return new SafePathResult(false, intersetion.Valid ? intersetion : intersection);
            }

            return new SafePathResult(true, intersection);
        }

        public static List<Vector2> GetEvadePoints(int speed = -1, int delay = 0, bool onlyGood = false)
        {
            speed = speed == -1 ? (int)ObjectManager.Player.MoveSpeed : speed;

            var goodCandidates = new List<Vector2>();
            var badCandidates = new List<Vector2>();
            var polygonList = new List<Geometry.Polygon>();
            var takeClosestPath = false;

            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.Evade())
                {
                    if (skillshot.SpellData.TakeClosestPath && skillshot.IsDanger(ObjectManager.Player.ServerPosition.To2D()))
                    {
                        takeClosestPath = true;
                    }

                    polygonList.Add(skillshot.EvadePolygon);
                }
            }

            var dangerPolygons = Geometry.ClipPolygons(polygonList).ToPolygons();
            var myPosition = ObjectManager.Player.ServerPosition.To2D();

            foreach (var poly in dangerPolygons)
            {
                for (var i = 0; i <= poly.Points.Count - 1; i++)
                {
                    var sideStart = poly.Points[i];
                    var sideEnd = poly.Points[i == poly.Points.Count - 1 ? 0 : i + 1];
                    var originalCandidate = myPosition.ProjectOn(sideStart, sideEnd).SegmentPoint;
                    var distanceToEvadePoint = Vector2.DistanceSquared(originalCandidate, myPosition);

                    if (distanceToEvadePoint < 600 * 600)
                    {
                        var sideDistance = Vector2.DistanceSquared(sideEnd, sideStart);
                        var direction = (sideEnd - sideStart).Normalized();
                        var s = distanceToEvadePoint < 200 * 200 && sideDistance > 90 * 90 ? 7 : 0;

                        for (var j = -s; j <= s; j++)
                        {
                            var candidate = originalCandidate + j*20*direction;
                            var pathToPoint = ObjectManager.Player.GetPath(candidate.To3D()).To2DList();

                            if (IsSafePath(pathToPoint, 250, speed, delay).IsSafe)
                            {
                                goodCandidates.Add(candidate);
                            }

                            if (IsSafePath(pathToPoint, 80, speed, delay).IsSafe && j == 0)
                            {
                                badCandidates.Add(candidate);
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

            return goodCandidates.Count > 0 ? goodCandidates : (onlyGood ? new List<Vector2>() : badCandidates);
        }

        public static List<Obj_AI_Base> GetEvadeTargets(EvadeSpellData spell, bool onlyGood = false, bool DontCheckForSafety = false)
        {
            var badTargets = new List<Obj_AI_Base>();
            var goodTargets = new List<Obj_AI_Base>();
            var allTargets = new List<Obj_AI_Base>();

            allTargets.AddRange(
                HeroManager.Enemies.Where(x => x.Distance(ObjectManager.Player) <= spell.Range));
            allTargets.AddRange(
                MinionManager.GetMinions(
                    ObjectManager.Player.Position, spell.Range, MinionTypes.All, MinionTeam.NotAlly));

            foreach (var target in allTargets.Where(x => DontCheckForSafety || IsSafe(x.ServerPosition.To2D()).IsSafe))
            {
                var pathToTarget = new List<Vector2> {ObjectManager.Player.ServerPosition.To2D(), target.ServerPosition.To2D()};

                if (IsSafePath(pathToTarget, EvadingFirstTimeOffset, spell.Speed, spell.Delay).IsSafe)
                {
                    goodTargets.Add(target);
                }

                if (IsSafePath(pathToTarget, EvadingSecondTimeOffset, spell.Speed, spell.Delay).IsSafe)
                {
                    badTargets.Add(target);
                }
            }

            return goodTargets.Count > 0 ? goodTargets : (onlyGood ? new List<Obj_AI_Base>() : badTargets);
        }
    }
}