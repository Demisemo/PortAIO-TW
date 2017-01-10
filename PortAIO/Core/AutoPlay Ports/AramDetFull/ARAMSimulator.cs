using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ARAMDetFull.Champions;
using LeagueSharp;
using DetuksSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using ARAMDetFull.Helpers;
using System.Diagnostics;
using System.ComponentModel;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using DetuksSharp;

using EloBuddy;
namespace ARAMDetFull
{
    class ARAMSimulator
    {
        public enum ARAMPlayState
        {
            Shopping,
            Defending,
            Pushing
        }

        public enum ChampType
        {
            Mage,
            Tank,
            Support,
            Carry,
            Bruiser,
            MageTank,
            MageNoMana,
            TankAS,
            MageAS,
            DpsAS
        }

        internal class ItemToShop
        {
            public List<int> itemIds;
            public List<int> itemsMustHave;
            public List<int> sellItems;
            public bool last = false;

            public int goldReach
            {
                get
                {

                    return (last) ? 99999999 : itemIds.Sum(ids => Items.GetItem(ids).GetPriceToBuy());
                }
            }
        }

        public static string[] mages =
        {
            "Annie", "Ahri", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana",  "FiddleSticks",
             "Gragas", "Heimerdinger", "Karthus",
            "Kassadin",  "Leblanc", "Lissandra", "Lux", "Malzahar",
            "Morgana", "Orianna",   "Swain", "Syndra",  "TwistedFate", "Veigar", "Viktor",
            "Xerath", "Ziggs", "Zyra", "Vel'Koz", "Chogath", "Malphite", "Maokai","Shaco"
        };

        public static string[] supports =
        {
            "Alistar", "Blitzcrank", "Janna", "Karma", "Nami", "Sona", "Soraka", "Taric",
            "Thresh", "Zilean","Lulu",
        };

        public static string[] tanks =
        {
            "Amumu", "DrMundo","Sion", "Galio", "Hecarim", "Rammus", "Sejuani",
            "Shen", "Singed", "Skarner", "Volibear", "Leona",
            "Yorick", "Zac", "Udyr", "Nasus", "Trundle", "Irelia","Braum", "Vi"
        };

        public static string[] ad_carries =
        {
            "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves",  "KogMaw", "MissFortune",
             "Sivir", "Jinx", "Jayce", "Gangplank",
            "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Zed", "Lucian","Yasuo","MasterYi","Quinn","Kalista","Vayne","Kindred", "Jhin"

        };

        public static string[] bruisers =
        {
            "Aatrox", "Darius",  "Fiora", "Garen", "JarvanIV", "Jax", "Khazix", "LeeSin",
            "Nautilus", "Nocturne", "Olaf", "Poppy",
            "Renekton", "Rengar", "Riven", "Shyvana", "Tryndamere", "MonkeyKing", "XinZhao","Pantheon",
            "Rek'Sai","Gnar","Wukong","RekSai", "Kled"
        };

        public static string[] mageNoMana =
        {
            "Akali", "Katarina", "Vladimir", "Rumble","Mordekaiser", "Kennen"
        };

        public static string[] mageTank =
        {
            "Gragas","Galio","Bard","Singed","Nunu","Evelynn","Elise","Ryze","Ekko","Fizz", "Nidalee"
        };

        public static string[] mageAS =
        {
            "Azir","Kayle","Teemo",
        };

        public static string[] tankAS =
        {
            "Warwick",
        };

        public static string[] dpsAS =
        {

        };
        public static AIHeroClient player = ObjectManager.Player;

        public static Obj_HQ fromNex = null;

        public static Obj_HQ toNex = null;


        public static List<Sector> sectors = new List<Sector>();

        public static List<ItemToShop> defBuyThings;


        public static Build champBuild;

        public static ItemToShop nextItem;
        public static int lastBuy = 0;
        public static SummonerSpells sSpells;
        public static int tankBal = -20;

        public static ChampType getType()
        {
            var cName = player.ChampionName;
            if (mages.Contains(cName))
                return ChampType.Mage;

            if (supports.Contains(cName))
                return ChampType.Support;

            if (tanks.Contains(cName))
                return ChampType.Tank;

            if (ad_carries.Contains(cName))
                return ChampType.Carry;

            if (bruisers.Contains(cName))
                return ChampType.Bruiser;

            if (mageNoMana.Contains(cName))
                return ChampType.MageNoMana;

            if (mageTank.Contains(cName))
                return ChampType.MageTank;

            if (mageAS.Contains(cName))
                return ChampType.MageAS;

            if (tankAS.Contains(cName))
                return ChampType.TankAS;

            if (dpsAS.Contains(cName))
                return ChampType.DpsAS;

            return ChampType.Tank;
        }

        public static void setUpItems()
        {
            if (getType() == ChampType.Support || getType() == ChampType.Mage)
            {
                tankBal = 15;

                champBuild = new Build
                {
                    coreItems = new List<ConditionalItem>
                    {
                        new ConditionalItem(ItemId.Athenes_Unholy_Grail),
                        new ConditionalItem(ItemId.Sorcerers_Shoes),
                        new ConditionalItem(ItemId.Rabadons_Deathcap),
                        new ConditionalItem(ItemId.Abyssal_Scepter,ItemId.Zhonyas_Hourglass,ItemCondition.ENEMY_AP),
                        new ConditionalItem(ItemId.Void_Staff,ItemId.Liandrys_Torment, ItemCondition.ENEMY_MR),
                        new ConditionalItem(ItemId.Rylais_Crystal_Scepter),
                    },
                    startingItems = new List<ItemId>
                    {
                        ItemId.Needlessly_Large_Rod
                    }
                };

                #region itemsToBuyList
            }
            else if (getType() == ChampType.TankAS)
            {
                champBuild = new Build
                {
                    coreItems = new List<ConditionalItem>
                    {
                        new ConditionalItem(ItemId.Mercurys_Treads),
                        new ConditionalItem(ItemId.Frozen_Mallet),
                        new ConditionalItem(ItemId.Wits_End),
                        new ConditionalItem(ItemId.Blade_of_the_Ruined_King),
                        new ConditionalItem(ItemId.Banshees_Veil,ItemId.Sunfire_Cape, ItemCondition.ENEMY_MR),
                        new ConditionalItem(ItemId.Trinity_Force),
                    },
                    startingItems = new List<ItemId>
                    {
                        ItemId.Boots_of_Speed,(ItemId)3751
                    }
                };
            }
            else if (getType() == ChampType.DpsAS)
            {
                champBuild = new Build
                {
                    coreItems = new List<ConditionalItem>
                    {
                        new ConditionalItem(ItemId.Boots_of_Speed),
                        new ConditionalItem(ItemId.Statikk_Shiv),
                        new ConditionalItem(ItemId.The_Bloodthirster),
                        new ConditionalItem(ItemId.Phantom_Dancer),
                        new ConditionalItem(ItemId.Infinity_Edge),
                        new ConditionalItem(ItemId.Banshees_Veil,ItemId.Thornmail, ItemCondition.ENEMY_MR),
                    },
                    startingItems = new List<ItemId>
                    {
                        ItemId.Zeal,
                    }
                };
            }
            else if (getType() == ChampType.MageAS)
            {
                champBuild = new Build
                {
                    coreItems = new List<ConditionalItem>
                    {
                        new ConditionalItem(ItemId.Sorcerers_Shoes),
                        new ConditionalItem(ItemId.Nashors_Tooth),
                        new ConditionalItem(ItemId.Rabadons_Deathcap),
                        new ConditionalItem(ItemId.Ludens_Echo),
                        new ConditionalItem(ItemId.Guinsoos_Rageblade),
                        new ConditionalItem(ItemId.Hextech_Gunblade),
                    },
                    startingItems = new List<ItemId>
                    {
                        ItemId.Stinger,
                    }
                };

            }
            else if (getType() == ChampType.MageNoMana)
            {
                tankBal = 0;

                champBuild = new Build
                {
                    coreItems = new List<ConditionalItem>
                    {
                        new ConditionalItem(ItemId.Ludens_Echo),
                        new ConditionalItem(ItemId.Sorcerers_Shoes),
                        new ConditionalItem(ItemId.Abyssal_Scepter,ItemId.Zhonyas_Hourglass,ItemCondition.ENEMY_AP),
                        new ConditionalItem(ItemId.Rabadons_Deathcap),
                        new ConditionalItem(ItemId.Void_Staff,ItemId.Liandrys_Torment, ItemCondition.ENEMY_MR),
                        new ConditionalItem(ItemId.Rylais_Crystal_Scepter),
                    },
                    startingItems = new List<ItemId>
                    {
                        ItemId.Needlessly_Large_Rod
                    }
                };
            }
            else if (getType() == ChampType.MageTank)
            {
                tankBal = -20;
                champBuild = new Build
                {
                    coreItems = new List<ConditionalItem>
                    {
                        new ConditionalItem(ItemId.Rod_of_Ages),
                        new ConditionalItem(ItemId.Sorcerers_Shoes),
                        new ConditionalItem(ItemId.Abyssal_Scepter,ItemId.Zhonyas_Hourglass,ItemCondition.ENEMY_MR),
                        new ConditionalItem(ItemId.Rylais_Crystal_Scepter),
                        new ConditionalItem(ItemId.Rabadons_Deathcap),
                        new ConditionalItem(ItemId.Liandrys_Torment),
                    },
                    startingItems = new List<ItemId>
                    {
                        ItemId.Catalyst_of_Aeons,
                    }
                };
            }
            else if (getType() == ChampType.Tank)
            {
                tankBal = -40;

                champBuild = new Build
                {
                    coreItems = new List<ConditionalItem>
                    {
                        new ConditionalItem(ItemId.Mercurys_Treads),
                        new ConditionalItem(ItemId.Sunfire_Cape),
                        new ConditionalItem(ItemId.Banshees_Veil,ItemId.Thornmail,ItemCondition.ENEMY_AP),
                        new ConditionalItem(ItemId.Frozen_Mallet),
                        new ConditionalItem(ItemId.Maw_of_Malmortius,ItemId.Iceborn_Gauntlet,ItemCondition.ENEMY_AP),
                        new ConditionalItem(ItemId.Warmogs_Armor),
                    },
                    startingItems = new List<ItemId>
                    {
                        ItemId.Giants_Belt,
                    }
                };
            }
            #endregion

            else if (getType() == ChampType.Bruiser) //Bruiser
            {
                tankBal = -25;

                champBuild = new Build
                {
                    coreItems = new List<ConditionalItem>
                    {
                        new ConditionalItem(ItemId.Mercurys_Treads),
                        new ConditionalItem(ItemId.Spirit_Visage,ItemId.Sunfire_Cape,ItemCondition.ENEMY_AP),
                        new ConditionalItem(ItemId.Trinity_Force),
                        new ConditionalItem(ItemId.Banshees_Veil,ItemId.Thornmail,ItemCondition.ENEMY_AP),
                        new ConditionalItem(ItemId.Blade_of_the_Ruined_King),
                        new ConditionalItem(ItemId.Maw_of_Malmortius,ItemId.Randuins_Omen,ItemCondition.ENEMY_AP),
                    },
                    startingItems = new List<ItemId>
                    {
                        ItemId.Boots_of_Speed,ItemId.Ruby_Crystal,ItemId.Long_Sword,
                    }
                };

            }
            else //adc
            {
                tankBal = 15;

                champBuild = new Build
                {
                    coreItems = new List<ConditionalItem>
                    {
                        new ConditionalItem(ItemId.Infinity_Edge),
                        new ConditionalItem(ItemId.Berserkers_Greaves),
                        new ConditionalItem(ItemId.Phantom_Dancer),
                        new ConditionalItem(ItemId.Essence_Reaver),
                        new ConditionalItem(ItemId.Maw_of_Malmortius,ItemId.The_Bloodthirster,ItemCondition.ENEMY_AP),
                        new ConditionalItem((ItemId)ItemId.Blade_of_the_Ruined_King),
                       // new ConditionalItem(ItemId.Banshees_Veil,ItemId.Thornmail,ItemCondition.ENEMY_MR),
                    },
                    startingItems = new List<ItemId>
                    {
                        ItemId.Boots_of_Speed,ItemId.Long_Sword,ItemId.Long_Sword,
                    }
                };

            }
        }

        public static Champions.Champion champ = null;


        public static void setChamp()
        {
            Chat.Print("Champion hero: " + player.ChampionName);
            #region Init Champion
            /*
            switch (player.ChampionName)
            {
                case "Cassiopeia":
                    champ = new Cassiopeia();
                    break;
                case "Pantheon":
                    champ = new PantheonA();
                    break;
                case "Udyr":
                    champ = new Udyr();
                    break;
                case "Quinn":
                    champ = new Quinn();
                    break;
                case "Nami":
                    champ = new Nami();
                    break;
                case "Malphite":
                    champ = new MalphiteA();
                    break;
                case "Urgot":
                    champ = new Urgot();
                    break;
                case "Zyra":
                    champ = new Zyra();
                    break;
                case "Blitzcrank":
                    champ = new BlitzcrankA();
                    break;
                case "Thresh":
                    champ = new Thresh();
                    break;
                case "Karthus":
                    champ = new Karthus();
                    break;
                case "Ezreal":
                    champ = new Ezreal();
                    break;
                case "Vayne":
                    champ = new Vayne();
                    break;
                case "Kassadin":
                    champ = new Kassadin();
                    break;
                case "Zed":
                    champ = new Zed();
                    break;
                case "Maokai":
                    champ = new Maokai();
                    break;
                case "Sejuani":
                    champ = new Sejuani();
                    break;
                case "Darius":
                    champ = new Darius();
                    break;
                case "RekSai":
                    champ = new RekSai2();
                    break;
                case "Varus":
                    champ = new Varus();
                    break;
                case "Draven":
                    champ = new Draven();
                    break;
                case "Katarina":
                    champ = new Katarina();
                    break;
                case "KogMaw":
                    champ = new Kogmaw();
                    break;
                case "Azir":
                    champ = new Azir();
                    break;
                case "Shaco":
                    champ = new Shaco();
                    break;
                case "Singed":
                    champ = new Singed();
                    break;
                case "Twitch":
                    champ = new Twitch();
                    break;
                case "FiddleSticks":
                    champ = new Fiddlestick();
                    break;
                case "Viktor":
                    champ = new ViktorA();
                    break;
                case "Leona":
                    champ = new Leona();
                    break;
                case "Taric":
                    champ = new Taric();
                    break;
                case "Kalista":
                    champ = new Kalista();
                    break;
                case "Brand":
                    champ = new BrandA();
                    break;
                case "Jinx":
                    champ = new Jinx();
                    break;
                case "Nautilus":
                    champ = new Nautilus();
                    break;
                case "Gragas":
                    champ = new Gragas();
                    break;
                case "Aatrox":
                    champ = new Aatrox();
                    break;
                case "Wukong":
                    champ = new WukongA();
                    break;
                case "Syndra"://TODO put back
                    champ = new SyndraA();
                    break;
                case "Jayce":
                    champ = new Jayce();
                    break;
                case "Ziggs":
                    champ = new ZiggsA();
                    break;
                case "Zilean":
                    champ = new Zilean();
                    break;
                case "XinZhao":
                    champ = new XinZhaoA();
                    break;
                case "Talon":
                    champ = new Talon();
                    break;
                case "Xerath":
                    champ = new XerathA();
                    break;
                case "Ahri":
                    champ = new Ahri();
                    break;
                case "Bard":
                    champ = new Brad();
                    break;
                case "Lucian":
                    champ = new Lucian();
                    break;
                case "Vi":
                    champ = new ViA();
                    break;
                case "Hecarim":
                    champ = new Hecarim();
                    break;
                case "Mordekaiser":
                    champ = new MordekaiserA();
                    break;
                case "Garen":
                    champ = new Garen();
                    break;
                case "Anivia":
                    champ = new Anivia();
                    break;
                case "Nunu":
                    champ = new Nunu();
                    break;
                case "Evelynn":
                    champ = new EvelynnA();
                    break;
                case "Malzahar":
                    champ = new Malzahar();
                    break;
                case "Khazix":
                    champ = new Khazix();
                    break;
                case "Veigar":
                    champ = new Veigar();
                    break;
                case "Poppy":
                    champ = new Poppy();
                    break;
                case "Tristana":
                    champ = new Tristana();
                    break;
                case "Lulu":
                    champ = new Lulu();
                    break;
                case "Orianna":
                    champ = new OriannaA();
                    break;
                case "Elise":
                    champ = new Elise();
                    break;
                case "Kayle":
                    champ = new Kayle();
                    break;
                case "Yasuo":
                    champ = new YasuoA();
                    break;
                case "Warwick":
                    champ = new WarwickA();
                    break;
                case "Karma":
                    champ = new KarmaA();
                    break;
                case "Soraka":
                    champ = new Soraka();
                    break;
                case "Ryze":
                    champ = new Ryze();
                    break;
                case "MasterYi":
                    champ = new MasterYi();
                    break;
                case "Annie":
                    champ = new Annie();
                    break;
                case "Ekko":
                    champ = new Ekko();
                    break;
                case "Olaf":
                    champ = new OlafA();
                    break;
                case "Diana":
                    champ = new Diana();
                    break;
                case "TahmKench":
                    champ = new Tahmkench();
                    break;
                case "TwistedFate":
                    champ = new TwistedFateA();
                    break;
                case "Heimerdinger":
                    champ = new Herimerdinger();
                    break;
                case "MissFortune":
                    champ = new MissFortune();
                    break;
                case "Galio":
                    champ = new Galio();
                    break;
                case "Sion":
                    champ = new SionA();
                    break;
                case "Shen":
                    champ = new Shen();
                    break;
                case "Gangplank":
                    champ = new Gangplank();
                    break;
                case "Fizz":
                    champ = new Fizz();
                    break;
                case "Volibear":
                    champ = new Volibear();
                    break;
                case "Graves":
                    champ = new Graves();
                    break;
                case "JarvanIV":
                    champ = new Jarvan();
                    break;
                case "LeeSin":
                    champ = new LeeSinA();
                    break;
                case "Nocturne":
                    champ = new NocturneA();
                    break;
                case "Vladimir":
                    champ = new Vladimir();
                    break;
                case "Taliyah":
                    champ = new TaliyahA();
                    break;
                case "Lissandra":
                    champ = new Lissandra();
                    break;
                case "DrMundo":
                    champ = new DrMundo();
                    break;
                case "Zac":
                    champ = new Zac();
                    break;
                case "Akali":
                    champ = new AkaliA();
                    break;
                case "Riven":
                    champ = new Riven();
                    break;
                case "Teemo":
                    champ = new Teemo();
                    break;
                case "AurelionSol":
                    champ = new AurelionSol();
                    break;
                case "Tryndamere":
                    champ = new Tryndamere();
                    break;
                case "Illaoi":
                    champ = new Illaoi();
                    break;
            }*/
            #endregion
        }

        public static void buyItems()
        {
            if (lastBuy < DeathWalker.now - 2300)
            {
                AutoShopper.buyNext();
                lastBuy = DeathWalker.now;
            }
        }

        public static void setupARMASimulator()
        {
            try
            {
                GameObject.OnCreate += TowerAttackOnCreate;

                if (ObjectManager.Player.Hero != EloBuddy.Champion.Corki)
                    GameObject.OnDelete += onDelete;

                foreach (var tur in ObjectManager.Get<Obj_HQ>())
                {
                    if (tur.Team == GameObjectTeam.Chaos && player.Team == GameObjectTeam.Chaos)
                        fromNex = tur;
                    if (tur.Team == GameObjectTeam.Chaos && player.Team == GameObjectTeam.Order)
                        toNex = tur;

                    if (tur.Team == GameObjectTeam.Order && player.Team == GameObjectTeam.Order)
                        fromNex = tur;

                    if (tur.Team == GameObjectTeam.Order && player.Team == GameObjectTeam.Chaos)
                        toNex = tur;
                }

                if (fromNex == null)
                    return;

                float sep = fromNex.Position.Distance(toNex.Position) / 40;

                Vector2 lastPos = fromNex.Position.To2D();
                for (int i = 0; i < 40; i++)
                {
                    Vector2 end = lastPos.Extend(toNex.Position.To2D(), sep);
                    sectors.Add(new Sector(lastPos, end, 750));
                    lastPos = end;
                }

                MapControl.setupMapControl();
                AutoLevelChamp.setAutoLevel();
                AutoShopper.init();
                setUpItems();
                setChamp();
                AutoShopper.setBuild(champBuild);
                sSpells = new SummonerSpells();
                if (champ != null)
                {
                    champ.setUpSpells();
                }

                DeathWalker.CustomRunCS = (player.IsMelee) ? 250 : 150;
                DeathWalker.CustomMoveDelay = 324;
                DeathWalker.BottingMode = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void onDelete(GameObject sender, EventArgs args)
        {
            if (MapControl.usedRelics.Contains(sender.NetworkId))
                MapControl.usedRelics.Remove(sender.NetworkId);
        }

        private static void TowerAttackOnCreate(GameObject sender, EventArgs args)
        {
            try
            {
                if (sender.IsValid<MissileClient>() && sender.IsValid && (MissileClient)sender != null)
                {
                    var missile = (MissileClient)sender;
                    if (missile.SpellCaster.IsValid<Obj_AI_Turret>() && missile.SpellCaster.IsEnemy && missile.Target != null && missile.Target.IsValid<AIHeroClient>() && missile.Target.IsAlly)
                    {
                        var turret = (Obj_AI_Turret)missile.SpellCaster;
                        if (missile.Target.IsMe)
                        {
                            towerAttackedMe = true;
                            towerAttackedAlly = false;
                        }
                        else if (((AIHeroClient)missile.Target).Distance(turret) < 700)
                        {
                            towerAttackedAlly = true;
                            towerAttackedMe = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void setRambo()
        {
            ramboMode = false;
            if (player.IsZombie && (player.ChampionName == "Sion" || player.ChampionName == "KogMaw") || ARAMTargetSelector.IsInvulnerable(player))
                ramboMode = true;
        }

        public static bool towerAttackedMe = false;
        public static bool towerAttackedAlly = false;

        public static bool haveSeenMinion = false;

        public static bool ramboMode = false;

        public static int balance = 0;
        public static int agrobalance = 0;
        public static Vector2 awayTo;

        public static AIHeroClient deepestAlly = player;

        const UInt32 WM_KEYDOWN = 0x0100;
        const UInt32 WM_KEYUP = 0x0101;
        const int VK_F5 = 0x74;

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        private static bool needRecall = false;
        private static int lastRecall = 0;
        public static bool inDanger = false;
        public static float farmRange = 900;

        public static void updateArmaPlay()
        {
            try
            {
                if (!haveSeenMinion)
                {
                    haveSeenMinion =
                        ObjectManager.Get<Obj_AI_Minion>().Any(min => min.IsTargetable && min.IsHPBarRendered && min.IsAlly && min.Health > 50) ||
                        ARAMDetFull.gameStart + 44 * 1000 < ARAMDetFull.now;
                }

                if (!haveSeenMinion)
                    return;

                try
                {
                    AutoLevelChamp.LevelUpOff();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                if ((player.InShop() || player.IsDead))
                {
                    buyItems();
                }

                if (champ != null)
                {
                    champ.alwaysCheck();
                }

                setRambo();

                if (player.IsDead || player.IsChannelingImportantSpell())
                    return;

                var fightLevel = MapControl.fightLevel();
                MapControl.updateReaches();

                var closestEnemy = EloBuddy.SDK.EntityManager.Heroes.Enemies.Where(ene => !ene.IsDead && ene.IsTargetable && ene.IsHPBarRendered && !ARAMTargetSelector.IsInvulnerable(ene)).OrderBy(ene => player.Position.Distance(ene.Position, true)).FirstOrDefault();
                if (closestEnemy != null && ramboMode)
                {
                    DeathWalker.deathWalk(closestEnemy.Position, true);
                    return;
                }

                if (fightLevel != 0)
                {
                    Aggresivity.addAgresiveMove(new AgresiveMove(40 * fightLevel, 2000, true, true));
                }

                agrobalance = Aggresivity.getAgroBalance();

                balance = (ARAMTargetSelector.IsInvulnerable(player) || player.IsZombie) ? 250 : MapControl.balanceAroundPointAdvanced(player.Position.To2D(), 250 - agrobalance * 5) + agrobalance;
                inDanger = balance < 0;

                if (champ != null)
                {
                    try
                    {
                        if (player.GetEnemiesInRange(ARAMSimulator.farmRange).Count(ene => !ene.IsDead && !ene.IsZombie) != 0)
                            champ.killSteal();
                        else
                            champ.farm();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                if (!Sector.inTowerRange(player.Position.To2D()) || towerAttackedAlly || player.HealthPercent < 25)
                {
                    try
                    {
                        ItemHandler.useItems();
                        sSpells.useSumoners();
                        if (champ != null)
                        {
                            champ.useSpells();
                        }
                        else
                        {
                            MapControl.myControler.useSpells();
                            if (player.MaxMana < 350 || player.ManaPercent > 50)
                                MapControl.myControler.useSpellsOnMinions();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                deepestAlly = EloBuddy.SDK.EntityManager.Heroes.Allies.OrderBy(al => toNex.Position.Distance(al.Position, true)).FirstOrDefault();

                var lookRange = player.AttackRange + ((player.IsMelee) ? 260 : 155);
                var easyKill =
                   EloBuddy.SDK.EntityManager.Heroes.Enemies.FirstOrDefault(ene => ene != null && !ene.IsZombie && !ene.IsDead && ene.Distance(player, true) < lookRange * lookRange &&
                                                             !ARAMTargetSelector.IsInvulnerable(ene) && ene.Health / 1.5 < player.GetAutoAttackDamage(ene) && ene.IsHPBarRendered);

                if (easyKill != null && easyKill.IsValidTarget())
                {
                    Aggresivity.addAgresiveMove(new AgresiveMove(45, 1500, true));
                    DeathWalker.deathWalk(easyKill.Position.To2D().Extend(player.Position.To2D(), player.AttackRange * 0.7f).To3D(), true);
                }


                if (balance < 0)
                    DeathWalker.deathWalk(player.Position.To2D().Extend(fromNex.Position.To2D(), 632).To3D(), true);

                if ((!player.IsMelee || fightLevel < 2) && EloBuddy.SDK.EntityManager.Heroes.Enemies.Any(h => !h.IsDead) && moveToRelicIfForHeal())
                {
                    return;
                }

                if (!player.UnderTurret(true))
                {
                    towerAttackedMe = false;
                    towerAttackedAlly = false;
                }

                if (towerAttackedMe)
                {
                    DeathWalker.CustomOrbwalkMode = false;
                    DeathWalker.deathWalk(player.Position.To2D().Extend(fromNex.Position.To2D(), 600).To3D(), true);
                    return;
                }

                awayTo = eAwayFromTo();
                if (awayTo.IsValid() && awayTo.X != 0)
                {
                    DeathWalker.CustomOrbwalkMode = false;
                    if (champ != null)
                        champ.kiteBack(awayTo);
                    DeathWalker.deathWalk(awayTo.To3D(), true);
                    return;

                }
                else
                {

                    var closestObj =
                        EloBuddy.SDK.EntityManager.Turrets.Enemies.Where(
                            obj => obj.IsValidTarget(700) && !obj.IsDead && !obj.IsInvulnerable && obj.IsTargetable)
                            .OrderBy(obj => obj.Position.Distance(player.Position, true)).FirstOrDefault();

                    if (closestObj != null && (!(closestObj is Obj_AI_Turret) || Sector.towerContainsAlly((Obj_AI_Turret)closestObj)))
                    {
                        DeathWalker.deathWalk(
                            closestObj.Position.Extend(player.Position, player.AttackRange * 0.6f), true);
                        return;
                    }

                    if (player.IsMelee)
                    {
                        var safeMeleeEnem = ARAMTargetSelector.getSafeMeleeTarget();
                        if (safeMeleeEnem != null && safeMeleeEnem.IsValidTarget())
                        {
                            DeathWalker.deathWalk(safeMeleeEnem.Position.Extend(safeMeleeEnem.Direction, player.AttackRange * 0.3f), true);
                            return;
                        }

                    }
                    var fightOn = MapControl.fightIsOn();
                    if (fightOn != null && MapControl.balanceAroundPointAdvanced(fightOn.Position.To2D(), 280, 450) > (-130) && fightOn.Distance(player, true) < 2500 * 2500 && (!player.IsMelee() || !Sector.inTowerRange(fightOn.Position.To2D())))
                    {
                        Aggresivity.addAgresiveMove(new AgresiveMove(40 * MapControl.fightLevel(), 2000, true, true));
                        DeathWalker.deathWalk(fightOn.Position.Extend(player.Position, player.AttackRange * 0.8f), true);
                    }
                    else
                    {
                        if (!inDanger)
                        {
                            Sector orbSector = null;
                            Sector prevSector = null;
                            foreach (var sector in sectors)
                            {
                                sector.update();
                                int sectorCheck = 1150 - MapControl.fearDistance;
                                if (sector.containsEnemyChamp && sector.enemyChampIn.Distance(player, true) < sectorCheck * sectorCheck)
                                {
                                    orbSector = sector;
                                    break;
                                }
                                if (sector.dangerPolig)
                                    break;
                                if (sector.containsEnemy && !sector.containsAlly)
                                    break;
                                orbSector = sector;
                                if (sector.containsEnemy && sector.containsAlly)
                                    break;
                                prevSector = sector;
                            }
                            if (orbSector == null)
                                return;
                            DeathWalker.deathWalk(orbSector.getRandomPointIn().To3D(), false, true);
                        }
                        else
                        {
                            DeathWalker.deathWalk(player.Position.To2D().Extend(fromNex.Position.To2D(), 600).To3D(), false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static bool moveToRelicIfForHeal()
        {
            try
            {
                if (ObjectManager.Player.Hero == EloBuddy.Champion.Corki)
                    return false;
                var relicHeal = MapControl.ClosestRelic();
                if (relicHeal != null)
                {
                    var dist = relicHeal.Distance(player);
                    var bonus = ((50 - dist / 20) > 0) ? (50 - dist / 20) : 0;
                    bool needHeal = player.HealthPercent + (float)agrobalance / 5 - (tankBal / 2.5) < 39 + bonus;
                    if (dist < 100 && !relicHeal.IsMoving)
                    {
                        MapControl.usedRelics.Add(relicHeal.NetworkId);
                    }
                    if ((needHeal && player.HealthPercent > 18 && MapControl.fightIsOn() == null) || (!relicHeal.Name.Contains("Health")) && dist < 2500)
                    {
                        DeathWalker.deathWalk(relicHeal.Position);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool enemIsOnMe(MapControl.ChampControl targetChamp)
        {
            var target = targetChamp.hero;
            try
            {
                if (target.IsAlly || target.IsDead || !target.IsValidTarget() || target == null)
                    return false;
                if (player.HealthPercent > 40 && !target.IsFacing(player))
                    return false;
                if (player.HealthPercent > 50 && targetChamp.lastAttackedUnitId != player.NetworkId)
                    return false;

                float distTo = target.Distance(player, true);
                bool dangerousAround = (balance < -player.HealthPercent);
                float targetReack = (!dangerousAround) ? player.AttackRange + 150 : MapControl.getByObj(target).getReach();
                if (distTo > targetReack * targetReack)
                    return false;

                var per = target.Direction.To2D().Perpendicular();
                var dir = new Vector3(per, 0);
                var enemDir = target.Position + dir * 40;
                if (target.Distance(fromNex.Position, true) < enemDir.Distance(fromNex.Position, true))
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return true;
            }
        }

        public static Vector2 eAwayFromTo()
        {
            if (player.IsMelee() || player.ChampionName == "Kalista")
                return new Vector2(0, 0);

            Vector2 backTo = player.Position.To2D();
            if (!backTo.IsValid())
            {
                return new Vector2(0, 0);
            }
            int count = 0;

            backTo -= (toNex.Position - player.Position).To2D();
            foreach (var enem in MapControl.enemy_champions.Where(enemIsOnMe))
            {
                count++;
                backTo -= (enem.hero.Position - player.Position).To2D();
            }


            if (count > 0)
            {
                var awayTo = player.Position.To2D().Extend(backTo, player.AttackRange * 0.7f);
                if (!Sector.inTowerRange(awayTo))
                    return backTo;
            }
            return new Vector2(0, 0);
        }

    }
}
