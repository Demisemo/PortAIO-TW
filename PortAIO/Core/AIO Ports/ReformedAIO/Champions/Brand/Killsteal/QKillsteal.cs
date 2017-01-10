using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Brand.Killsteal
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ReformedAIO.Champions.Brand.Core.Spells;

    using RethoughtLib.FeatureSystem.Implementations;

    internal sealed class QKillsteal : OrbwalkingChild
    {
        public override string Name { get; set; } = "Q";

        private readonly QSpell spell;

        public QKillsteal(QSpell spell)
        {
            this.spell = spell;
        }

        private AIHeroClient Target => TargetSelector.GetTarget(spell.Spell.Range, TargetSelector.DamageType.Physical);

        private void OnUpdate(EventArgs args)
        {
            if (!CheckGuardians()
                || Target == null 
                || Target.Health > spell.Spell.GetDamage(Target)
                || spell.Prediction(Target).Hitchance < HitChance.High
                || Menu.Item("Brand.Killsteal.Q.Mana").GetValue<Slider>().Value > ObjectManager.Player.ManaPercent)
            {
                return;
            }

            spell.Spell.Cast(spell.Prediction(Target).CastPosition);
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

            Menu.AddItem(new MenuItem("Brand.Killsteal.Q.Mana", "Min Mana %").SetValue(new Slider(0, 0, 100)));
        }
    }
}
