using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using HERMES_Kalista.MyUtils;
using HERMES_Kalista.Utils;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace HERMES_Kalista.MyLogic.Others
{
    public static partial class Events
    {
        public static void OnProcessSpellcast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            #region Anti-Stealth

            if (args.SData.Name.ToLower().Contains("talonshadow")) //#TODO get the actual buff name
            {
                if (Items.HasItem((int) ItemId.Oracle_Alteration) &&
                    Items.CanUseItem((int) ItemId.Oracle_Alteration))
                {
                    Items.UseItem((int) ItemId.Oracle_Alteration, Heroes.Player.Position);
                }
                else if (Items.HasItem((int) ItemId.Control_Ward, Heroes.Player))
                {
                    Items.UseItem((int) ItemId.Control_Ward, Heroes.Player.Position.Randomize(0, 125));
                }
            }

            #endregion

            if (Program.ComboMenu.Item("RComboSelf").GetValue<bool>() && Program.R.IsReady() && sender.IsEnemy &&
                args.Target.NetworkId == ObjectManager.Player.NetworkId)
            {
                var cctype = Utils.SpellDb.GetByName(args.SData.Name).CcType;
                if (ObjectManager.Player.CountEnemiesInRange(600) > 1 && cctype == CcType.Suppression ||
                    (cctype == CcType.Knockup &&
                     HeroManager.Enemies.Any(e => e.ChampionName == "Yasuo" && e.Distance(ObjectManager.Player) < 1100)) ||
                    cctype == CcType.Pull)
                {
                    Program.R.Cast();
                }
            }
        }
    }
}
