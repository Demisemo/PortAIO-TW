using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LeagueSharp;using DetuksSharp;
using LeagueSharp.Common;

using EloBuddy; namespace ARAMDetFull
{
    public enum ItemCondition
    {
        TAKE_PRIMARY = 0,
        ENEMY_AP = 1,
        ENEMY_MR = 2,
        ENEMY_RANGED = 3,
        ENEMY_LOSING = 4,
    }

    

    public class AutoShopper
    {
        private static readonly Dictionary<int, FullItem> itemList = new Dictionary<int, FullItem>();

        public static AIHeroClient player = ObjectManager.Player;

        public static int testGold = 1375;

        public static Build curBuild;

        private static bool gotStartingItems = false;

        private static bool willGoOverItem = false;

        private static List<InvItem> inv = new List<InvItem>();
        private static List<InvItem> inv2 = new List<InvItem>();

        private static List<int> canBuyOnfull = new List<int>();

        private static bool finished = false;

        private static List<int> failedToFindItemList = new List<int>();

        public static void init()
        {
            string itemJson = "http://ddragon.leagueoflegends.com/cdn/6.24.1/data/en_US/item.json";
            string itemsData = Request(itemJson);
            string itemArray = itemsData.Split(new[] { "data" }, StringSplitOptions.None)[1];
            MatchCollection itemIdArray = Regex.Matches(itemArray, "[\"]\\d*[\"][:][{].*?(?=},\"\\d)");
            foreach (FullItem item in from object iItem in itemIdArray select new FullItem(iItem.ToString()))
                itemList.Add(item.Id, item);
            Console.WriteLine("Finished build init");
            finished = true;
        }

        public static void setBuild(Build build)
        {
            curBuild = build;
        }
        //[SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static string Request(string url)
        {
            WebRequest request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream: dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return responseFromServer;
        }

        public static void buyNext()
        {
            if (!finished)
                return;
            if (!gotStartingItems && player.Level<4)
            {
                //Console.WriteLine("buye starting!");
                foreach (var item in curBuild.startingItems)
                {
                    Shop.BuyItem(item);
                }
                gotStartingItems = true;
            }
            else
            {
                willGoOverItem = false;
                var best = getBestBuyItem();
                if (best == -1)
                    return;
                //if (willGoOverItem || !inventoryFull())
                    Shop.BuyItem((ItemId)best);
              //  else
               // {
              //      Console.WriteLine("wont buy");
               // }
            }
        }

        public static int freeSlots()
	    {
            return ObjectManager.Player.InventoryItems.Count(y => !y.DisplayName.Contains("Poro"))-2;
	    }
	
        public static bool inventoryFull()
	    {
	            return freeSlots() == 6;
        }

        public static int getBestBuyItem()
        {
            foreach (var item in curBuild.coreItems)
            {
                if(item.gotIt())
                    continue;

                List<BuyItem> chain = new List<BuyItem>();
                fillItemsChain(item.getBest().Id, ref chain, true);
                //Console.WriteLine("chain: " + chain.Count);

                var bestItem =
                    chain.Where(sel => sel.price <= player.Gold && (!inventoryFull() || canBuyOnfull.Contains(sel.item.Id))).OrderByDescending(sel2 => sel2.price).FirstOrDefault();
                if (bestItem == null || bestItem.price == 0)
                {
                    if (inventoryFull() && chain.Count == 0)
                    {
                        if (player.Level >= 9 && !player.HasBuff("ElixirOfIron"))
                            return (int)ItemId.Elixir_of_Iron;
                        if (!player.HasBuff("OracleExtractSight"))
                            return (int)ItemId.Oracles_Extract;
                    }
                    return -1;
                }
                //Console.WriteLine("Buy: " + bestItem.item.Name);
                return bestItem.item.Id;
            }
            return -1;
        }

        public static void fillItemsChain(int id, ref List<BuyItem> ids,bool start = false, int masterId = -1)
        {
            if (start)
            {
                canBuyOnfull.Clear();
                inv2.Clear();
                inv2 = player.InventoryItems.Select(iIt => new InvItem
                {
                    id = (int)iIt.Id
                }).ToList();
            }

            foreach (var itNow in inv2.Where(it => !it.used()))
            {
                if (itNow.id == id)
                {
                    canBuyOnfull.Add(masterId);
                    willGoOverItem = true;
                    itNow.setUsed();
                    return;
                }
            }

            var data = getData(id);

            var test = new BuyItem
            {
                item = data,
                price = getItemsPrice(id, true)
            };
            //Console.WriteLine("item: "+test.item.Name+" : price "+test.price);
            ids.Add(test);
            if (data.From != null)
                foreach (var item in data.From)
                {
                    //Console.WriteLine("req Item: "+(int)item);
                    fillItemsChain(item, ref ids, false, id);
                }
        }

        public static int getItemsPrice(int item, bool start = false)
        {
            if (start)
            {
                inv.Clear();
                inv = player.InventoryItems.Select(iIt => new InvItem
                {
                    id = (int)iIt.Id
                }).ToList();
            }

            foreach (var itNow in inv.Where(it => !it.used()))
            {
                if (itNow.id == item)
                {
                    willGoOverItem = true;
                    itNow.setUsed();
                    return 0;
                }
            }

            var FullItem = getData(item);
            return FullItem.Goldbase + ((FullItem.From != null)?FullItem.From.Sum(req => getItemsPrice(req)):0);
        }

        public static FullItem getData(int id, bool throwIfNotFound = true)
        {
            try
            {
                return itemList[id];
            }
            catch (Exception)
            {
                //At this poiunt need to update builds!
                failedToFindItemList.Add(id);
                Console.WriteLine("!!!! Bad itemID: " + id);
                if (throwIfNotFound)
                    throw;
                else
                    return null;
            }
        }

        public static int itemCount(int id, AIHeroClient hero = null)
        {
            return (hero ?? ObjectManager.Player).InventoryItems.Count(slot => (int)slot.Id == id);
        }

        public static bool gotItem(FullItem item, AIHeroClient hero = null)
        {
            //If item is bad we skip it!
            if (failedToFindItemList.Contains(item.Id))
                return true;

            var sameItemList = new List<int>();
            sameItemList.Add(item.Id);
            //Check if transform item!
            if (item.Into != null)
                foreach (var into in item.Into)
                {
                    var intoItem = getData(into, false);
                    if (intoItem == null || intoItem.Ispurchasable)
                        continue;
                    sameItemList.Add(into);
                }
            return (hero ?? ObjectManager.Player).InventoryItems.Any(slot => sameItemList.Contains((int)slot.Id));
        }

    }

    public class Build
    {
        public List<ItemId> startingItems;
        public List<ConditionalItem> coreItems;
    }

    public class BuyItem
    {
        public FullItem item;
        public int price;
    }


    public class InvItem
    {
        private bool got = false;
        public int id;

        public bool used()
        {
            return got;
        }

        public void setUsed()
        {
            got = true;
        }
    }

    public class ConditionalItem
    {

        private FullItem selected;

        private FullItem primary;
        private FullItem secondary;
        private ItemCondition condition;

        public ConditionalItem(int pri, int sec = -1, ItemCondition cond = ItemCondition.TAKE_PRIMARY)
        {
            primary =  AutoShopper.getData(pri);
            secondary = (sec == -1)?null:AutoShopper.getData(sec);
            condition = cond;
        }

        public ConditionalItem(ItemId pri, ItemId sec = ItemId.Unknown, ItemCondition cond = ItemCondition.TAKE_PRIMARY)
        {
            primary = AutoShopper.getData((int)pri);
            secondary = (sec == ItemId.Unknown) ? null : AutoShopper.getData((int)sec);
            condition = cond;
        }

        public bool gotIt()
        {
            return primary == null || AutoShopper.gotItem(primary) || (secondary != null && AutoShopper.gotItem(secondary));
        }

        public FullItem getBest()
        {
            if (selected != null)
                return selected;
            if(condition == ItemCondition.TAKE_PRIMARY)
                selected = primary;
            if (condition == ItemCondition.ENEMY_MR)
            {
                if(EloBuddy.SDK.EntityManager.Heroes.Enemies.Sum(ene => ene.SpellBlock) > EloBuddy.SDK.EntityManager.Heroes.Enemies.Sum(ene => ene.Armor))
                    selected = primary;
                else
                    selected = secondary;
            }

            if (condition == ItemCondition.ENEMY_RANGED)
            {
                if (EloBuddy.SDK.EntityManager.Heroes.Enemies.Count(ene => ene.IsRanged) > EloBuddy.SDK.EntityManager.Heroes.Enemies.Count(ene => ene.IsMelee))
                    selected = primary;
                else
                    selected = secondary;
            }

            if (condition == ItemCondition.ENEMY_AP)
            {
                if (EloBuddy.SDK.EntityManager.Heroes.Enemies.Sum(ene => ene.FlatMagicDamageMod) > EloBuddy.SDK.EntityManager.Heroes.Enemies.Sum(ene => ene.FlatPhysicalDamageMod) * 2.0f)
                    selected = primary;
                else
                    selected = secondary;
            }
            if (condition == ItemCondition.ENEMY_LOSING)
            {
                var allTowers = ObjectManager.Get<Obj_AI_Turret>().ToList();
                var eneTowCount = allTowers.Count(tow => !tow.IsDead && tow.IsEnemy);
                var allyTowCount = allTowers.Count(tow => !tow.IsDead && tow.IsAlly);
                if (allyTowCount == eneTowCount)
                {
                    return HeroManager.Player.Deaths > HeroManager.Player.ChampionsKilled ? secondary : primary;
                }
                if (eneTowCount < allyTowCount)
                    selected = primary;
                else
                    selected = secondary;
            }

            return selected;
        }
    }
}
