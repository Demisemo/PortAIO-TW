using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Xerath.Drawings.Spells
{
    using System;
    using System.Drawing;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ReformedAIO.Champions.Xerath.Core.Spells;

    using RethoughtLib.FeatureSystem.Abstract_Classes;

    internal sealed class RDrawing : ChildBase
    {
        public override string Name { get; set; } = "R";

        private readonly RSpell spell;

        public RDrawing(RSpell spell)
        {
            this.spell = spell;
        }

        public void OnEndScene(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            Render.Circle.DrawCircle(ObjectManager.Player.Position,
                                     spell.RealRange,
                                     spell.Spell.IsReady()
                                     ? Color.OrangeRed
                                     : Color.DarkSlateGray,
                                     Menu.Item("Xerath.Draw.R.Width").GetValue<Slider>().Value,
                                     Menu.Item("Xerath.Draw.R.Z").GetValue<bool>());

            if (Menu.Item("Xerath.Draw.R.Minimap").GetValue<bool>() && spell.Spell.IsReady())
            {
                LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, spell.RealRange, Color.OrangeRed, 1, 26, true);
            }
        }


        protected override void OnDisable(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnDisable(sender, eventArgs);

            Drawing.OnEndScene -= OnEndScene;
        }

        protected override void OnEnable(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnEnable(sender, eventArgs);


            Drawing.OnEndScene += OnEndScene;
        }

        protected override void OnLoad(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnLoad(sender, eventArgs);

            Menu.AddItem(new MenuItem("Xerath.Draw.R.Minimap", "Minimap").SetValue(true));

            Menu.AddItem(new MenuItem("Xerath.Draw.R.Z", "Draw Z").SetValue(false));

            Menu.AddItem(new MenuItem("Xerath.Draw.R.Width", "Thickness").SetValue(new Slider(3, 1, 5)));
        }
    }
}
