#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

using EloBuddy; namespace HumanizerSharp
{
    public class Program
    {
        public static Menu Menu;
        public static int LastMove;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Dictionary<SpellSlot, int> LastCast = new Dictionary<SpellSlot, int>();
        public static Render.Text BlockedMovement;
        public static Render.Text BlockedSpells;
        public static int BlockedSpellCount;
        public static int BlockedMoveCount;
        public static int NextMovementDelay;
        public static Vector3 LastMovementPosition = Vector3.Zero;

        public static List<SpellSlot> Items = new List<SpellSlot>
        {
            SpellSlot.Item1,
            SpellSlot.Item2,
            SpellSlot.Item3,
            SpellSlot.Item4,
            SpellSlot.Item5,
            SpellSlot.Item6,
            SpellSlot.Trinket
        };

        public static void Game_OnGameLoad()
        {
            Menu = new Menu("Humanizer", "Humanizer", true);

            var spells = Menu.AddSubMenu(new Menu("Spells", "Spells"));

            foreach (var spell in Items)

            {
                var menu = spells.AddSubMenu(new Menu(spell.ToString(), spell.ToString()));
                menu.AddItem(new MenuItem("Enabled" + spell, "Delay " + spell).SetValue(true));
                menu.AddItem(new MenuItem("MinDelay" + spell, "Minimum Delay").SetValue(new Slider(80)));
                menu.AddItem(new MenuItem("MaxDelay" + spell, "Maximum Delay").SetValue(new Slider(200, 100, 400)));
                LastCast.Add(spell, 0);
            }

            spells.AddItem(new MenuItem("DrawSpells", "Draw Blocked Spell Count").SetValue(true));

            var move = Menu.AddSubMenu(new Menu("Movement", "Movement"));
            move.AddItem(new MenuItem("MovementEnabled", "Enabled").SetValue(true));
            move.AddItem(new MenuItem("MovementHumanizeDistance", "Humanize Movement Distance").SetValue(true));
            move.Item("MovementHumanizeDistance")
                .SetTooltip("Stops the orbwalker from moving too closely to last movement");

            move.AddItem(new MenuItem("MovementHumanizeRate", "Humanize Movement Rate").SetValue(true));
            move.Item("MovementHumanizeRate").SetTooltip("Stops the orbwalker from sending too many movement requests.");

            move.AddItem(new MenuItem("MinDelay", "Minimum Delay")).SetValue(new Slider(80));
            move.AddItem(new MenuItem("MaxDelay", "Maximum Delay")).SetValue(new Slider(200, 100, 400));
            move.AddItem(new MenuItem("DrawMove", "Draw Blocked Movement Count").SetValue(true));

            Menu.AddToMainMenu();

            BlockedSpells = new Render.Text(
                "Blocked Spells: ", Drawing.Width - 200, Drawing.Height - 600, 28, Color.Green);
            BlockedSpells.VisibleCondition += sender => Menu.Item("DrawSpells").IsActive();
            BlockedSpells.TextUpdate += () => "Blocked Spells: " + BlockedSpellCount;
            BlockedSpells.Add();

            BlockedMovement = new Render.Text(
                "Blocked Move: ", Drawing.Width - 200, Drawing.Height - 625, 28, Color.Green);
            BlockedMovement.VisibleCondition += sender => Menu.Item("DrawMove").IsActive();
            BlockedMovement.TextUpdate += () => "Blocked Move: " + BlockedMoveCount;
            BlockedMovement.Add();


            EloBuddy.Player.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            var spell = args.Slot;
            var senderValid = sender != null && sender.Owner != null && sender.Owner.IsMe;

            if (!senderValid || !Items.Contains(spell) || !Menu.Item("Enabled" + spell).IsActive())
            {
                return;
            }

            var min = Menu.Item("MinDelay" + spell).GetValue<Slider>().Value;
            var max = Menu.Item("MaxDelay" + spell).GetValue<Slider>().Value;
            var delay = min >= max ? min : WeightedRandom.Next(min, max);

            if (LastCast[spell].TimeSince() < delay)
            {
                BlockedSpellCount++;
                args.Process = false;
                return;
            }

            LastCast[spell] = Utils.TickCount;
        }

        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            var senderValid = sender != null && sender.IsValid && sender.IsMe;

            if (!senderValid || args.Order != GameObjectOrder.MoveTo || !Menu.Item("MovementEnabled").IsActive())
            {
                return;
            }
            if (LastMovementPosition != Vector3.Zero && args.TargetPosition.Distance(LastMovementPosition) < 300)
            {
                if (NextMovementDelay == 0)
                {
                    var min = Menu.Item("MinDelay").GetValue<Slider>().Value;
                    var max = Menu.Item("MaxDelay").GetValue<Slider>().Value;
                    NextMovementDelay = min > max ? min : WeightedRandom.Next(min, max);
                }

                if (Menu.Item("MovementHumanizeRate").IsActive() && LastMove.TimeSince() < NextMovementDelay)
                {
                    NextMovementDelay = 0;
                    BlockedMoveCount++;
                    args.Process = false;
                    return;
                }

                if (Menu.Item("MovementHumanizeDistance").IsActive())
                {
                    var wp = ObjectManager.Player.Path.ToList().To2D();
                   /* if (wp.Count > 1 && wp.Last().Distance(args.TargetPosition) < 20)
                    {
                        //Console.WriteLine("HUMANIZE WAYPOINTS");
                        BlockedMoveCount++;
                        args.Process = false;
                        return;
                    }

                    if (args.TargetPosition.Distance(LastMovementPosition) < 20)
                    {
                        //Console.WriteLine("HUMANIZE LAST POSITION");
                        BlockedMoveCount++;
                        args.Process = false;
                        return;
                    }
                    */
                    if (args.TargetPosition.Distance(Player.ServerPosition) < 50)
                    {
                        // Console.WriteLine("HUMANIZE CURRENT POSITION");
                        BlockedMoveCount++;
                        args.Process = false;
                        return;
                    }
                }
            }

            LastMovementPosition = args.TargetPosition;
            LastMove = Utils.TickCount;
        }
    }
}