using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Vayne.Core.Spells
{
    using LeagueSharp;
    using LeagueSharp.Common;

    using RethoughtLib.FeatureSystem.Implementations;
    using RethoughtLib.FeatureSystem.Switches;

    internal class RSpell : SpellChild
    {
        public override string Name { get; set; } = "Final Hour";

        public override Spell Spell { get; set; }

        public float GetDamage(Obj_AI_Base target)
        {
            return Spell.GetDamage(target);
        }

        protected override void OnLoad(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnLoad(sender, eventArgs);

            Spell = new Spell(SpellSlot.R);
        }

        protected override void SetSwitch()
        {
            Switch = new UnreversibleSwitch(Menu);
        }
    }
}
