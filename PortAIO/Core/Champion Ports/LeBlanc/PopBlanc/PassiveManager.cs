using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TreeLib.Extensions;

using EloBuddy;
using LeagueSharp.Common;
using System.Linq;

namespace PopBlanc
{
    internal class PassiveManager
    {
        private static Menu _menu;
        private static Obj_AI_Base _clone;

        private static bool _override;

        private static bool Enabled
        {
            get { return !Player.IsDead && _menu.Item("CloneEnabled").GetValue<bool>(); }
        }

        private static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Initialize(Menu menu)
        {
            _menu = menu.AddMenu("Passive", "Passive");
            _menu.AddInfo("CloneInfo", " --> Automatic movement of clone.", Color.Red);
            _menu.AddBool("CloneEnabled", "Control Clone");
            _menu.AddList("CloneMode", "Mode", new[] { "To Player", "To Target", "Away from Player" });
            _menu.AddBool("CloneOverride", "Manual Override");
            _menu.Item("CloneOverride").SetTooltip("If clone is manually moved stop automatically moving.");

            EloBuddy.Player.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (_clone != null && !_override && _menu.Item("CloneOverride").IsActive() &&
                sender.NetworkId == _clone.NetworkId)
            {
                _override = true;
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(Player.Name))
            {
                return;
            }

            _clone = sender as Obj_AI_Base;
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (_clone != null && sender.NetworkId.Equals(_clone.NetworkId))
            {
                _clone = null;
                _override = false;
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            try
            {
                if (!Enabled)
                {
                    return;
                }

                var pet = _clone; //Player.Pet as Obj_AI_Base;
                var mode = _menu.Item("CloneMode").GetValue<StringList>().SelectedIndex;
                var aaRange = Orbwalking.GetRealAutoAttackRange(pet);

                if (_override || pet == null || !pet.IsValid || pet.IsDead || pet.Health < 1)
                {
                    return;
                }

                var target = TargetSelector.GetTarget(
                    aaRange, TargetSelector.DamageType.Physical, true, null, pet.ServerPosition);

                if (mode == 1 &&
                    !(pet.CanAttack && !pet.Spellbook.IsAutoAttacking && !pet.Spellbook.IsAutoAttacking && target.IsValidTarget()))
                {
                    mode = 0;
                }

                switch (mode)
                {
                    case 0: // toward player
                        var pos = Player.Path.ToList().Count > 1
                            ? Player.Path.ToList()[1]
                            : Player.ServerPosition;
                        LeagueSharp.Common.Utility.DelayAction.Add(
                            200, () => { EloBuddy.Player.IssueOrder(GameObjectOrder.MovePet, pos, false); });
                        break;
                    case 1: //toward target
                        EloBuddy.Player.IssueOrder(GameObjectOrder.AutoAttackPet, target, false);
                        break;
                    case 2: //away from player
                        LeagueSharp.Common.Utility.DelayAction.Add(
                            100,
                            () =>
                            {
                                EloBuddy.Player.IssueOrder(
                                    GameObjectOrder.MovePet,
                                    pet.Position + 500 * (pet.Position - Player.Position).Normalized(), false);
                            });
                        break;
                }
            }
            catch {}
        }
    }
}