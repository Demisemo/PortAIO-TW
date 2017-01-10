using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using HERMES_Kalista.MyUtils;
using ItemData = LeagueSharp.Common.Data.ItemData;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace HERMES_Kalista.MyLogic.Others
{
    public static partial class Events
    {
        public static void OnUpdate(EventArgs args)
        {
            if (LeagueSharp.Common.Utility.Map.GetMap().Type != LeagueSharp.Common.Utility.Map.MapType.SummonersRift) return;
            if (Heroes.Player.HasBuff("rengarralertsound"))
            {
                if (Items.HasItem(ItemData.Oracle_Alteration.Id, Heroes.Player) && Items.CanUseItem(ItemData.Oracle_Alteration.Id))
                {
                    Items.UseItem(ItemData.Oracle_Alteration.Id, Heroes.Player.Position);
                }
                else if (Items.HasItem((int)ItemId.Control_Ward, Heroes.Player))
                {
                    Items.UseItem((int)ItemId.Control_Ward, Heroes.Player.Position.Randomize(0, 125));
                }
            }

            var enemyVayne = Heroes.EnemyHeroes.FirstOrDefault(e => e.CharData.BaseSkinName == "Vayne");
            if (enemyVayne != null && enemyVayne.Distance(Heroes.Player) < 700 && enemyVayne.HasBuff("VayneInquisition"))
            {
                if (Items.HasItem(ItemData.Oracle_Alteration.Id, Heroes.Player) && Items.CanUseItem(ItemData.Oracle_Alteration.Id))
                {
                    Items.UseItem(ItemData.Oracle_Alteration.Id, Heroes.Player.Position);
                }
                else if (Items.HasItem((int)ItemId.Control_Ward, Heroes.Player))
                {
                    Items.UseItem((int)ItemId.Control_Ward, Heroes.Player.Position.Randomize(0, 125));
                }
            }

            if (Heroes.Player.InFountain() && Shop.IsOpen)
            {
                if (Program.ComboMenu.Item("AutoBuy").GetValue<bool>() &&
                    !Items.HasItem(ItemData.Oracle_Alteration.Id, Heroes.Player) && Heroes.Player.Level >= 9 &&
                    HeroManager.Enemies.Any(
                        h =>
                            h.CharData.BaseSkinName == "Rengar" || h.CharData.BaseSkinName == "Talon" ||
                            h.CharData.BaseSkinName == "Vayne"))
                {
                    //Heroes.Shop.BuyItem((ItemId)ItemData.Oracle_Alteration.Id); soon
                }
            }
        }
    }
}
