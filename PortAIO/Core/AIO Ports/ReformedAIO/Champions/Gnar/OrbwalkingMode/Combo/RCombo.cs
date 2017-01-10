using EloBuddy; 
using LeagueSharp.Common; 
namespace ReformedAIO.Champions.Gnar.OrbwalkingMode.Combo
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ReformedAIO.Champions.Gnar.Core;
    using ReformedAIO.Champions.Gnar.Logic;
    using ReformedAIO.Library.Get_Information.HeroInfo;

    using RethoughtLib.FeatureSystem.Implementations;

    using SharpDX;

    internal sealed class RCombo : OrbwalkingChild
    {
        private readonly GnarWallDetection gnarWallDetection = new GnarWallDetection();

        private readonly GnarState gnarState = new GnarState();

        private readonly HeroInfo heroInfo = new HeroInfo();

        public override string Name { get; set; } = "R";

        private static AIHeroClient Target => TargetSelector.GetTarget(Spells.R2.Range, TargetSelector.DamageType.Physical);

        private void GameOnUpdate(EventArgs args)
        {
            if (Target == null 
                || heroInfo.HasSpellShield(Target) 
                || gnarState.Mini
                || !CheckGuardians())
            {
                return;
            }

            var wall = gnarWallDetection.Wall(Target.Position, Menu.Item("Range").GetValue<Slider>().Value);

            ObjectManager.Player.GetPath(wall);

            if (wall != Vector3.Zero)
            {
                Spells.R2.Cast(wall);
            }
        }

        protected override void OnLoad(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnLoad(sender, eventArgs);

            Menu.AddItem(new MenuItem("Range", "Range").SetValue(new Slider(590, 0, 590)));
        }

        protected override void OnDisable(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnDisable(sender, eventArgs);

            Game.OnUpdate -= GameOnUpdate;
        }

        protected override void OnEnable(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnEnable(sender, eventArgs);

            Game.OnUpdate += GameOnUpdate;
        }
    }
}
