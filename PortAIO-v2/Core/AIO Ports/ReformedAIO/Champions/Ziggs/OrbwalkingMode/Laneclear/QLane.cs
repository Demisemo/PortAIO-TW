using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Ziggs.OrbwalkingMode.Laneclear
{
    using System;
    using System.Collections.Generic;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ReformedAIO.Champions.Ziggs.Core.Spells;

    using RethoughtLib.FeatureSystem.Implementations;

    internal sealed class QLane : OrbwalkingChild
    {
        public override string Name { get; set; } = "Q";

        private readonly QSpell spell;

        public QLane(QSpell spell)
        {
            this.spell = spell;
        }

        private IEnumerable<Obj_AI_Base> Minion => MinionManager.GetMinions(ObjectManager.Player.Position, spell.Spell.Range);

        private void OnUpdate(EventArgs args)
        {
            if (!CheckGuardians()
               || Minion == null
               || Menu.Item("Ziggs.Lane.Q.Mana").GetValue<Slider>().Value > ObjectManager.Player.ManaPercent
               || (Menu.Item("Ziggs.Lane.Q.Enemies").GetValue<bool>() && ObjectManager.Player.CountEnemiesInRange(1400) >= 1))
            {
                return;
            }

            var pred = spell.Spell.GetCircularFarmLocation((List<Obj_AI_Base>)Minion);

            if (pred.MinionsHit < Menu.Item("Ziggs.Lane.Q.Count").GetValue<Slider>().Value)
            {
                return;
            }

            foreach (var m in Minion)
            {
                if (m.Health > spell.GetDamage(m) && Menu.Item("Ziggs.Lane.Q.Killable").GetValue<bool>())
                {
                    continue;
                }

                spell.Spell.Cast(pred.Position);
            }
        }

        protected override void OnDisable(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnDisable(sender, eventArgs);

            Game.OnUpdate -= OnUpdate;
        }

        protected override void OnEnable(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnEnable(sender, eventArgs);

            Game.OnUpdate += OnUpdate;
        }

        protected override void OnLoad(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnLoad(sender, eventArgs);

            Menu.AddItem(new MenuItem("Ziggs.Lane.Q.Count", "Min Predicted Hit Count").SetValue(new Slider(3, 1, 6)));

            Menu.AddItem(new MenuItem("Ziggs.Lane.Q.Killable", "Only Killable").SetValue(true));

            Menu.AddItem(new MenuItem("Ziggs.Lane.Q.Enemies", "Return if nearby enemies").SetValue(true));

            Menu.AddItem(new MenuItem("Ziggs.Lane.Q.Mana", "Min Mana %").SetValue(new Slider(70, 0, 100)));
        }
    }
}
