using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Gnar.Drawings.SpellRange
{
    using System;
    using System.Drawing;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ReformedAIO.Champions.Gnar.Core;

    using RethoughtLib.FeatureSystem.Abstract_Classes;

    internal sealed class ERange : ChildBase
    {
        public override string Name { get; set; }

        public ERange(string name)
        {
            Name = name;
        }

        private GnarState gnarState;

        public void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;

            if (gnarState.Mini)
            {
                Render.Circle.DrawCircle(
                   ObjectManager.Player.Position,
                   Spells.E.Range,
                   Spells.E.IsReady()
                   ? Color.OrangeRed
                   : Color.DarkSlateGray);
            }

            if (gnarState.Mega)
            {
                Render.Circle.DrawCircle(
                   ObjectManager.Player.Position,
                   Spells.E2.Range,
                   Spells.E2.IsReady()
                   ? Color.Red
                   : Color.DarkSlateGray);
            }
        }

        protected override void OnDisable(object sender, FeatureBaseEventArgs eventArgs)
        {
            Drawing.OnDraw -= OnDraw;
        }

        protected override void OnEnable(object sender, FeatureBaseEventArgs eventArgs)
        {
            Drawing.OnDraw += OnDraw;
        }

        protected override void OnLoad(object sender, FeatureBaseEventArgs eventArgs)
        {
            gnarState = new GnarState();
        }
    }
}
