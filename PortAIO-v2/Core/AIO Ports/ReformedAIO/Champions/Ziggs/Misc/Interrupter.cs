using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Ziggs.Misc
{
    using LeagueSharp;
    using LeagueSharp.Common;

    using ReformedAIO.Champions.Ziggs.Core.Spells;

    using RethoughtLib.FeatureSystem.Implementations;

    internal sealed class ZiggsInterrupter : OrbwalkingChild
    {
        public override string Name { get; set; } = "Interrupter";

        private readonly WSpell spell;

        public ZiggsInterrupter(WSpell spell)
        {
            this.spell = spell;
        }

        private void OnInterruptable(Obj_AI_Base sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!CheckGuardians() || sender == null)
            {
                return;
            }

            if (sender.IsValidTarget(spell.Spell.Range))
            {
                spell.Spell.CastOnUnit(sender);
            }
        }

        protected override void OnLoad(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnLoad(sender, eventArgs);
        }

        protected override void OnDisable(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnDisable(sender, eventArgs);

            Interrupter2.OnInterruptableTarget -= OnInterruptable;
        }

        protected override void OnEnable(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnEnable(sender, eventArgs);

            Interrupter2.OnInterruptableTarget += OnInterruptable;
        }
    }
}
