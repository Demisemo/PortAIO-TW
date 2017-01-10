using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Lucian.OrbwalkingMode.LaneClear
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ReformedAIO.Champions.Lucian.Spells;

    using RethoughtLib.FeatureSystem.Implementations;

    internal sealed class WLaneClear : OrbwalkingChild
    {
        public override string Name { get; set; } = "W";

        private readonly WSpell wSpell;

        public WLaneClear(WSpell wSpell)
        {
            this.wSpell = wSpell;
        }

        private void OnUpdate(EventArgs args)
        {
            if (Menu.Item("Lane.W.EnemiesCheck").GetValue<bool>()
                && ObjectManager.Player.CountEnemiesInRange(1350) >= 1
                || (ObjectManager.Player.ManaPercent <= Menu.Item("Lane.W.Mana").GetValue<Slider>().Value)
                
                || !CheckGuardians())
            {
                return;
            }

            var minion = MinionManager.GetMinions(ObjectManager.Player.Position, wSpell.Spell.Range);

            var pred = wSpell.Spell.GetCircularFarmLocation(minion);

            if (pred.MinionsHit >= Menu.Item("Lane.W.MinHit").GetValue<Slider>().Value)
            {
                wSpell.Spell.Cast(pred.Position);
            }
        }

        protected override void OnLoad(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnLoad(sender, eventArgs);

            Menu.AddItem(new MenuItem("Lane.W.EnemiesCheck", "Check Enemies First").SetValue(true).SetTooltip("Wont W If Nearby Enemies"));
            Menu.AddItem(new MenuItem("Lane.W.MinHit", "Min Hit By W").SetValue(new Slider(3, 0, 6)));
            Menu.AddItem(new MenuItem("Lane.W.Mana", "Min Mana %").SetValue(new Slider(80, 0, 100)));
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
    }
}
