using EloBuddy; 
using LeagueSharp.Common; 
namespace vEvade.SpecialSpells
{
    #region

    using LeagueSharp;
    using LeagueSharp.Common;

    using vEvade.Helpers;
    using vEvade.Spells;

    using SpellData = vEvade.Spells.SpellData;

    #endregion

    public class Riven : IChampionManager
    {
        #region Static Fields

        private static int lastQTick;

        #endregion

        #region Public Methods and Operators

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.MenuName != "RivenQ3")
            {
                return;
            }

            CustomEvents.Unit.OnDash += (sender, args) => OnDash(sender, args, spellData);
            Obj_AI_Base.OnPlayAnimation += (sender, args) => OnPlayAnimation(sender, args, spellData);
        }

        #endregion

        #region Methods

        private static void OnDash(Obj_AI_Base sender, Dash.DashItem args, SpellData data)
        {
            var caster = sender as AIHeroClient;

            if (caster == null || !caster.IsValid || (!caster.IsEnemy && !Configs.Debug)
                || caster.ChampionName != data.ChampName)
            {
                return;
            }

            if (Utils.GameTimeTickCount - lastQTick > 100)
            {
                return;
            }

            var newData = (SpellData)data.Clone();

            if (caster.HasBuff("RivenFengShuiEngine"))
            {
                newData.Radius += 75;
            }

            SpellDetector.AddSpell(caster, args.StartPos, args.EndPos, newData, null, SpellType.None, true, lastQTick);
        }

        private static void OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args, SpellData data)
        {
            var caster = sender as AIHeroClient;

            if (caster == null || !caster.IsValid || (!caster.IsEnemy && !Configs.Debug)
                || caster.ChampionName != data.ChampName)
            {
                return;
            }

            if (args.Animation == "Spell1c")
            {
                lastQTick = Utils.GameTimeTickCount;
            }
        }

        #endregion
    }
}