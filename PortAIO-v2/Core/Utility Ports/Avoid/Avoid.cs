using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;
using SharpDX;

using EloBuddy; 
using LeagueSharp.Common; 
namespace Avoid
{
    public class Avoid
    {
        private static readonly Dictionary<GameObject, AvoidObject> _avoidableObjects = new Dictionary<GameObject, AvoidObject>();
        public static Dictionary<GameObject, AvoidObject> AvoidableObjects
        {
            get { return new Dictionary<GameObject, AvoidObject>(_avoidableObjects); }
        }

        private static void OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (sender.IsMe)
            {
                if (!Config.Enabled)
                {
                    return;
                }

                switch (args.Order)
                {
                    case GameObjectOrder.AttackTo:
                    case GameObjectOrder.MoveTo:

                        // Skip everything if we are stuck
                        if (_avoidableObjects.Any(
                                o =>
                                    o.Value.MenuState.Value &&
                                    o.Value.ShouldBeAvoided(o.Key) &&
                                    Geometry.CircleCircleIntersection(
                                        ObjectManager.Player.ServerPosition.To2D(),
                                        o.Key.Position.To2D(),
                                        ObjectManager.Player.BoundingRadius,
                                        o.Value.BoundingRadius).Length > 1))
                        {
                            return;
                        }

                        var path = ObjectManager.Player.GetPath(args.TargetPosition);
                        for (int i = 1; i < path.Length; i++)
                        {
                            var start = path[i - 1].To2D();
                            var end = path[i].To2D();     
                            
                            // Minimalize the amount of avoidable objects to loop through
                            var distanceSqr = start.Distance(end, true);
                            var entries = _avoidableObjects.Where(
                                o =>
                                    o.Value.MenuState.Value &&
                                    o.Value.ShouldBeAvoided(o.Key) &&
                                    start.Distance(o.Key.Position, true) < distanceSqr &&
                                    end.Distance(o.Key.Position, true) < distanceSqr)
                                        .OrderBy(
                                            o =>
                                                ObjectManager.Player.Distance(o.Key.Position.To2D(), true));

                            foreach (var entry in entries)
                            {
                                var avoidPosition = entry.Key.Position.To2D();
                                var length = start.Distance(end) + ObjectManager.Player.BoundingRadius;
                                for (int j = 25; j < length; j += 25)
                                {
                                    // Get the next check point
                                    var checkPoint = start.Extend(end, j);

                                    // Calculate intersection points
                                    var intersections = Geometry.CircleCircleIntersection(
                                        checkPoint,
                                        avoidPosition,
                                        ObjectManager.Player.BoundingRadius,
                                        entry.Value.BoundingRadius);

                                    if (intersections.Length > 1)
                                    {
                                        // Update NavCells
                                        var cells = new Dictionary<NavMeshCell, CollisionFlags>();
                                        var step = 2 * Math.PI / 8;
                                        for (var theta = 0d; theta < 2 * Math.PI + step; theta += step)
                                        {
                                            var pos = NavMesh.WorldToGrid((float)(avoidPosition.X + entry.Value.BoundingRadius * Math.Cos(theta)),
                                                                          (float)(avoidPosition.Y - entry.Value.BoundingRadius * Math.Sin(theta)));
                                            
                                            var cell = pos.ToCell();
                                            if (!cells.Keys.Any(o => o.GridX == cell.GridX && o.GridY == cell.GridY))
                                            {
                                                cells.Add(cell, cell.CollFlags);
                                                cell.CollFlags = CollisionFlags.Wall;
                                            }
                                        }

                                        // Get new path
                                        var newPath = ObjectManager.Player.GetPath(args.TargetPosition);

                                        // Revert old flags
                                        foreach (var cell in cells)
                                        {
                                            cell.Key.CollFlags = cell.Value;
                                        }

                                        // Get new end
                                        for (var k = 0; k < newPath.Length; k++)
                                        {
                                            if (newPath[k].To2D().Distance(start, true) < 10 * 10 &&
                                                k + 1 < newPath.Length)
                                            {
                                                // Move to new end and cancel the current order
                                                EloBuddy.Player.IssueOrder(args.Order, newPath[k + 1], false);
                                                args.Process = false;
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        break;
                }
            }
        }

        public static void OnGameStart()
        {


            // Validate that there are avoidable objects in the current matchup
            if (ObjectDatabase.AvoidObjects.Count == 0)
            {
                return;
            }

            // Listen to events
            ObjectDetector.OnAvoidObjectAdded += OnAvoidObjectAdded;
            GameObject.OnDelete += OnDelete;
            GameObject.OnFloatPropertyChange += OnPropertyChange;
            Drawing.OnDraw += OnDraw;
            EloBuddy.Player.OnIssueOrder += OnIssueOrder;
        }

        private static void OnPropertyChange(GameObject sender, GameObjectFloatPropertyChangeEventArgs args)
        {
            var key = _avoidableObjects.Find(e => e.Key.NetworkId == sender.NetworkId).Key;
            if (key != null)
            {
                // Nidalee W
                if (sender.Name == "Noxious Trap" &&
                    args.Property == "mPercentBubbleRadiusMod" &&
                    args.Value == -1 &&
                    args.Value == 0)
                {
                    var baseObject = sender as Obj_AI_Base;
                    // Yes, it is named nidalee spear... Rito please...
                    if (baseObject != null && baseObject.BaseSkinName == "Nidalee_Spear")
                    {
                        _avoidableObjects.Remove(key);
                    }
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Config.DrawRanges)
            {
                return;
            }

            foreach (var obj in ObjectManager.Get<Obj_AI_Base>())
            {
                if (!obj.IsMe && ObjectManager.Player.Distance(obj.Position, true) < 400 * 400)
                {
                    //Render.Circle.DrawCircle(obj.Position, obj.BoundingRadius, Color.Red);
                    //var pos = Drawing.WorldToScreen(obj.Position);
                    //Drawing.DrawText(pos.X, pos.Y, Color.White, obj.Name);
                    //Chat.Print("{0}: {1}", obj.Name, obj.BoundingRadius);
                    //Chat.Print("{0} ({1}): {2}", obj.BaseSkinName, obj.BoundingRadius, string.Join(" | ", obj.Buffs.Select(b => b.DisplayName)));
                }
            }

            foreach (var entry in _avoidableObjects)
            {
                if (entry.Value.ShouldBeAvoided(entry.Key))
                {
                    // Draw a circle around the avoid object
                    Render.Circle.DrawCircle(entry.Key.Position, entry.Value.BoundingRadius, (Config.Enabled && entry.Value.MenuState.Value) ? Color.White : Color.Red);
                }
            }
        }

        private static void OnAvoidObjectAdded(GameObject sender, AvoidObject avoidObject)
        {
            _avoidableObjects[sender] = avoidObject;
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.IsValid)
            {
                var removeKeys = new List<GameObject>();
                foreach (var entry in _avoidableObjects)
                {
                    if (entry.Key.NetworkId == sender.NetworkId)
                    {
                        removeKeys.Add(entry.Key);
                        break;
                    }
                }

                removeKeys.ForEach(
                    key =>
                    {
                        _avoidableObjects.Remove(key);
                    });
            }
        }
    }
}
