using EloBuddy; 
using LeagueSharp.Common; 
namespace Flowers_Talon.Common
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;

    public static class Common
    {
        public static Vector3 GetFirstWallPoint(Vector3 start, Vector3 end)
        {
            if (start.IsValid() && end.IsValid())
            {
                var distance = start.Distance(end);

                for (var i = 0; i < distance; i = i + 1)
                {
                    var newPoint = start.Extend(end, i);

                    if (NavMesh.GetCollisionFlags(newPoint) == CollisionFlags.Wall || newPoint.IsWall())
                    {
                        return newPoint;
                    }
                }
            }

            return Vector3.Zero;
        }

        public static float GetWallWidth(Vector3 start, Vector3 direction)
        {
            var thickness = 0f;

            if (!start.IsValid() || !direction.IsValid())
            {
                return thickness;
            }

            for (var i = 0; i < 1000; i = i + 1)
            {
                if (NavMesh.GetCollisionFlags(start.Extend(direction, i)) == CollisionFlags.Wall ||
                    start.Extend(direction, i).IsWall())
                {
                    thickness += 1;
                }
                else
                {
                    return thickness;
                }
            }

            return thickness;
        }

        public static bool CanWallJump(Vector3 dashEndPos)
        {
            var firstWallPoint = GetFirstWallPoint(ObjectManager.Player.Position, dashEndPos);

            if (firstWallPoint.Equals(Vector3.Zero))
            {
                return false;
            }

            if (dashEndPos.IsWall())
            {
                var wallWidth = GetWallWidth(firstWallPoint, dashEndPos);

                if (wallWidth > 50 && wallWidth - firstWallPoint.Distance(dashEndPos) < wallWidth * 0.4f)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        public static double GetPassiveDamage(Obj_AI_Base target)
        {
            return ObjectManager.Player.CalcDamage
            (target, Damage.DamageType.Physical,
                (float) new double[] {60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160}
                    [ObjectManager.Player.Level <= 11 ? ObjectManager.Player.Level - 1 : 10]
                + 2.0f*ObjectManager.Player.FlatPhysicalDamageMod);
        }

        public static double GetQDamage(Obj_AI_Base target)
        {
            return ObjectManager.Player.CalcDamage
            (target, Damage.DamageType.Physical,
                (float)new double[] { 120, 150, 180, 210, 240 }[ObjectManager.Player.GetSpell(SpellSlot.Q).Level - 1] +
                1.5f *
                ObjectManager.Player.FlatPhysicalDamageMod);
        }

        public static double GetQ1Damage(Obj_AI_Base target)
        {
            return ObjectManager.Player.CalcDamage
            (target, Damage.DamageType.Physical,
                (float)new double[] { 80, 100, 120, 140, 160 }[ObjectManager.Player.GetSpell(SpellSlot.Q).Level - 1] +
                1.0f *
                ObjectManager.Player.FlatPhysicalDamageMod);
        }

        public static double GetWDamage(Obj_AI_Base target)
        {
            return ObjectManager.Player.CalcDamage
            (target, Damage.DamageType.Physical,
                (float)new double[] { 50, 60, 70, 80, 90 }[ObjectManager.Player.GetSpell(SpellSlot.W).Level - 1] +
                0.4f *
                ObjectManager.Player.FlatPhysicalDamageMod);
        }

        public static double GetW1Damage(Obj_AI_Base target)
        {
            return ObjectManager.Player.CalcDamage
            (target, Damage.DamageType.Physical,
                (float)new double[] { 60, 90, 120, 150, 180 }[ObjectManager.Player.GetSpell(SpellSlot.W).Level - 1] +
                0.7f *
                ObjectManager.Player.FlatPhysicalDamageMod);
        }

        public static double GetRDamage(Obj_AI_Base target)
        {
            return ObjectManager.Player.CalcDamage
            (target, Damage.DamageType.Physical,
                (float)new double[] { 80, 135, 150 }[ObjectManager.Player.GetSpell(SpellSlot.R).Level - 1] +
                0.8f *
                ObjectManager.Player.FlatPhysicalDamageMod);
        }

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

        public static bool CheckTargetSureCanKill(Obj_AI_Base target)
        {
            if (target == null)
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

        public static double ComboDamage(AIHeroClient target)
        {
            if (target != null && !target.IsDead && !target.IsZombie)
            {
                if (target.HasBuff("KindredRNoDeathBuff"))
                {
                    return 0;
                }

                if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
                {
                    return 0;
                }

                if (target.HasBuff("JudicatorIntervention"))
                {
                    return 0;
                }

                if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
                {
                    return 0;
                }

                if (target.HasBuff("FioraW"))
                {
                    return 0;
                }

                if (target.HasBuff("ShroudofDarkness"))
                {
                    return 0;
                }

                if (target.HasBuff("SivirShield"))
                {
                    return 0;
                }

                var damage = 0d;

                damage += ObjectManager.Player.GetAutoAttackDamage(target) + GetPassiveDamage(target) + 
                          (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).IsReady()
                              ? (target.DistanceToPlayer() <= 170f + target.BoundingRadius
                                  ? GetQ1Damage(target)
                                  : GetQDamage(target))
                              : 0d) +
                          (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).IsReady()
                              ? GetWDamage(target) + GetW1Damage(target)
                              : 0d) +
                              (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).IsReady()
                              ? GetRDamage(target)
                              : 0d) +
                          ((ObjectManager.Player.GetSpellSlot("SummonerDot") != SpellSlot.Unknown &&
                            ObjectManager.Player.GetSpellSlot("SummonerDot").IsReady())
                              ? 50 + 20 * ObjectManager.Player.Level - (target.HPRegenRate / 5 * 3)
                              : 0d);

                if (target.ChampionName == "Moredkaiser")
                {
                    damage -= target.Mana;
                }

                if (ObjectManager.Player.HasBuff("SummonerExhaust"))
                {
                    damage = damage * 0.6f;
                }

                if (target.HasBuff("GarenW"))
                {
                    damage = damage * 0.7f;
                }

                if (target.HasBuff("ferocioushowl"))
                {
                    damage = damage * 0.7f;
                }

                if (target.HasBuff("BlitzcrankManaBarrierCD") && target.HasBuff("ManaBarrier"))
                {
                    damage -= target.Mana / 2f;
                }

                return damage;
            }

            return 0d;
        }


        public static bool CanMove(this AIHeroClient Target)
        {
            return !(Target.MoveSpeed < 50) && !Target.IsStunned && !Target.HasBuffOfType(BuffType.Stun) &&
                !Target.HasBuffOfType(BuffType.Fear) && !Target.HasBuffOfType(BuffType.Snare) &&
                !Target.HasBuffOfType(BuffType.Knockup) && !Target.HasBuff("Recall") && !Target.HasBuffOfType(BuffType.Knockback)
                && !Target.HasBuffOfType(BuffType.Charm) && !Target.HasBuffOfType(BuffType.Taunt) &&
                !Target.HasBuffOfType(BuffType.Suppression) && (!Target.IsCastingInterruptableSpell()
                || Target.IsMoving);
        }

        public static float DistanceSquared(this Obj_AI_Base source, Vector3 position)
        {
            return source.DistanceSquared(position.To2D());
        }

        public static float DistanceSquared(this Obj_AI_Base source, Vector2 position)
        {
            return source.ServerPosition.DistanceSquared(position);
        }

        public static float DistanceSquared(this Vector3 vector3, Vector2 toVector2)
        {
            return vector3.To2D().DistanceSquared(toVector2);
        }

        public static float DistanceSquared(this Vector2 vector2, Vector2 toVector2)
        {
            return Vector2.DistanceSquared(vector2, toVector2);
        }

        public static float DistanceSquared(this Obj_AI_Base source, Obj_AI_Base target)
        {
            return source.DistanceSquared(target.ServerPosition);
        }

        public static float DistanceSquared(this Vector3 vector3, Vector3 toVector3)
        {
            return vector3.To2D().DistanceSquared(toVector3);
        }

        public static float DistanceSquared(this Vector2 vector2, Vector3 toVector3)
        {
            return Vector2.DistanceSquared(vector2, toVector3.To2D());
        }

        public static float DistanceSquared(this Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, bool onlyIfOnSegment = false)
        {
            var objects = point.ProjectOn(segmentStart, segmentEnd);

            return (objects.IsOnSegment || onlyIfOnSegment == false) ? Vector2.DistanceSquared(objects.SegmentPoint, point) : float.MaxValue;
        }

        public static Vector2[] CircleCircleIntersection(this Vector2 center1, Vector2 center2, float radius1, float radius2)
        {
            var d = center1.Distance(center2);

            if (d > radius1 + radius2 || (d <= Math.Abs(radius1 - radius2)))
            {
                return new Vector2[] { };
            }

            var a = ((radius1 * radius1) - (radius2 * radius2) + (d * d)) / (2 * d);
            var h = (float)Math.Sqrt((radius1 * radius1) - (a * a));
            var direction = (center2 - center1).Normalized();
            var pa = center1 + (a * direction);
            var s1 = pa + h * direction.Perpendicular();
            var s2 = pa - h * direction.Perpendicular();

            return new[] { s1, s2 };
        }

        public static bool Compare(this GameObject gameObject, GameObject @object)
        {
            return gameObject != null && gameObject.IsValid && @object != null && @object.IsValid && gameObject.NetworkId == @object.NetworkId;
        }

        public static MovementCollisionInfo VectorMovementCollision(this Vector2 pointStartA, Vector2 pointEndA, float pointVelocityA, Vector2 pointB, float pointVelocityB, float delay = 0f)
        {
            return new[]
            {
                pointStartA,
                pointEndA }
            .VectorMovementCollision(pointVelocityA, pointB, pointVelocityB, delay);
        }

        public static MovementCollisionInfo VectorMovementCollision(this Vector2[] pointA, float pointVelocityA, Vector2 pointB, float pointVelocityB, float delay = 0f)
        {
            if (pointA.Length < 1)
            {
                return default(MovementCollisionInfo);
            }

            float sP1X = pointA[0].X, sP1Y = pointA[0].Y, eP1X = pointA[1].X, eP1Y = pointA[1].Y, sP2X = pointB.X, sP2Y = pointB.Y;
            float d = eP1X - sP1X, e = eP1Y - sP1Y;
            float dist = (float)Math.Sqrt((d * d) + (e * e)), t1 = float.NaN;
            float s = Math.Abs(dist) > float.Epsilon ? pointVelocityA * d / dist : 0, k = (Math.Abs(dist) > float.Epsilon) ? pointVelocityA * e / dist : 0f;

            float r = sP2X - sP1X, j = sP2Y - sP1Y;
            var c = (r * r) + (j * j);

            if (dist > 0f)
            {
                if (Math.Abs(pointVelocityA - float.MaxValue) < float.Epsilon)
                {
                    var t = dist / pointVelocityA;

                    t1 = pointVelocityB * t >= 0f ? t : float.NaN;
                }
                else if (Math.Abs(pointVelocityB - float.MaxValue) < float.Epsilon)
                {
                    t1 = 0f;
                }
                else
                {
                    float a = (s * s) + (k * k) - (pointVelocityB * pointVelocityB), b = (-r * s) - (j * k);

                    if (Math.Abs(a) < float.Epsilon)
                    {
                        if (Math.Abs(b) < float.Epsilon)
                        {
                            t1 = (Math.Abs(c) < float.Epsilon) ? 0f : float.NaN;
                        }
                        else
                        {
                            var t = -c / (2 * b);

                            t1 = (pointVelocityB * t >= 0f) ? t : float.NaN;
                        }
                    }
                    else
                    {
                        var sqr = (b * b) - (a * c);

                        if (sqr >= 0)
                        {
                            var nom = (float)Math.Sqrt(sqr);
                            var t = (-nom - b) / a;

                            t1 = pointVelocityB * t >= 0f ? t : float.NaN;
                            t = (nom - b) / a;

                            var t2 = (pointVelocityB * t >= 0f) ? t : float.NaN;

                            if (!float.IsNaN(t2) && !float.IsNaN(t1))
                            {
                                if (t1 >= delay && t2 >= delay)
                                {
                                    t1 = Math.Min(t1, t2);
                                }
                                else if (t2 >= delay)
                                {
                                    t1 = t2;
                                }
                            }
                        }
                    }
                }
            }
            else if (Math.Abs(dist) < float.Epsilon)
            {
                t1 = 0f;
            }

            return new MovementCollisionInfo(t1, !float.IsNaN(t1) ? new Vector2(sP1X + (s * t1), sP1Y + (k * t1)) : default(Vector2));
        }

        public struct MovementCollisionInfo
        {
            public Vector2 CollisionPosition;
            public float CollisionTime;

            internal MovementCollisionInfo(float collisionTime, Vector2 collisionPosition)
            {
                CollisionTime = collisionTime;
                CollisionPosition = collisionPosition;
            }

            public object this[int i] => i == 0 ? CollisionTime : (object)CollisionPosition;
        }

        public static float DistanceToPlayer(this Obj_AI_Base source)
        {
            return ObjectManager.Player.Distance(source);
        }

        public static float DistanceToPlayer(this Vector3 position)
        {
            return position.To2D().DistanceToPlayer();
        }

        public static float DistanceToPlayer(this Vector2 position)
        {
            return ObjectManager.Player.Distance(position);
        }

        public static float DistanceToMouse(this Obj_AI_Base source)
        {
            return Game.CursorPos.Distance(source.Position);
        }

        public static float DistanceToMouse(this Vector3 position)
        {
            return position.To2D().DistanceToMouse();
        }

        public static float DistanceToMouse(this Vector2 position)
        {
            return Game.CursorPos.Distance(position.To3D());
        }
    }
}
