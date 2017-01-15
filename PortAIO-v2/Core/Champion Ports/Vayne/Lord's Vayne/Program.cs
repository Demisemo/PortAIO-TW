
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


using EloBuddy; 
using LeagueSharp.Common; 
namespace Lord_s_Vayne
{
    class Program
    {
        public static Spell E;
        public static Spell E2;
        public static Spell Q;
        public static Spell W;
        public static Spell R;




        public static Vector3 TumblePosition = Vector3.Zero;


        public static Orbwalking.Orbwalker orbwalker;

        private static string News = "Added Flash Condemn and Flash Condemn Percent Hp as well as Q animation Cancel on manual cast, Fixed Focus 2 W stacks"; 

        public static Menu menu;

        public static Dictionary<string, SpellSlot> spellData;
        public static Items.Item zzrot = new Items.Item(3512, 400);

        public static AIHeroClient tar;
        public const string ChampName = "Vayne";
        public static AIHeroClient Player;

        //public static Menu Itemsmenu;
        public static Menu qmenu;
        public static Menu emenu;
        public static Menu gmenu;
        public static Menu imenu;
        public static Menu rmenu;
       
    


        public static float LastMoveC;

        public static void MoveToLimited(Vector3 where)
        {
            if (Environment.TickCount - LastMoveC < 80)
            {
                return;
            }

            LastMoveC = Environment.TickCount;
            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, where);
        }

        public static void Main()
        {
            try
            {
                Game_OnGameLoad(new EventArgs());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }
        #region GameOnLoad
       
            public static void Game_OnGameLoad(EventArgs args)
            {
                Program.Q = new Spell(SpellSlot.Q, 300f);
                Program.W = new Spell(SpellSlot.W);
                Program.E2 = new Spell(
                   SpellSlot.E, (uint)(650 + ObjectManager.Player.BoundingRadius));
                Program.E = new Spell(SpellSlot.E, 550f);
                Program.E.SetTargetted(0.25f, 1600f);
                Program.R = new Spell(SpellSlot.R);

                Program.Player = ObjectManager.Player;
     
                if (Program.Player.ChampionName != ChampName) return;
                Program.spellData = new Dictionary<string, SpellSlot>();
 
                Program.menu = new Menu("Lord's-汎", "Lord's Vayne", true);
            
                Program.menu.AddSubMenu(new Menu("走砍", "Orbwalker"));
                Program.orbwalker = new Orbwalking.Orbwalker(Program.menu.SubMenu("Orbwalker"));
             
                var TargetSelectorMenu = new Menu("目標選擇器", "Target Selector");
                TargetSelector.AddToMenu(TargetSelectorMenu);
               
                Program.menu.AddSubMenu(TargetSelectorMenu);

                Program.menu.AddItem(
                    new MenuItem("aaqaa", "自動 -> Q -> AA").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

                Program.qmenu = Program.menu.AddSubMenu(new Menu("翻滾", "Tumble"));
                Program.qmenu.AddItem(new MenuItem("FastQ", "快速 Q").SetValue(true).SetValue(true).SetTooltip("Q Animation Cancelation"));
                Program.qmenu.AddItem(new MenuItem("FastQs", "手動取消 Q 單擊").SetValue(true).SetValue(true).SetTooltip("Cancel Your Q when you manually cast it(Need to have \"Fast Q\" on too)")).Permashow(true, "Vayne | Vayne Manul Cancel", Color.Aqua); 
                Program.qmenu.AddItem(new MenuItem("UseQC", "使用 Q 連招").SetValue(true));
                Program.qmenu.AddItem(new MenuItem("QMode", "使用 Q 模式:", true).SetValue(new StringList(new[] { "Gosu", "Side", "Cursor", "SmartQ", "SafeQ", "AggroQ", "Burst", "Hiki"})));
            /*if (Program.qmenu.Item("QMode", true).GetValue<StringList>().SelectedIndex == 8)
            {
                Program.qmenu.AddItem(new MenuItem("QOrderBy", "Q to position").SetValue(new StringList(new[] { "CLOSETOMOUSE", "CLOSETOTARGET" })));
            }*/
            if (Program.qmenu.Item("QMode", true).GetValue<StringList>().SelectedIndex == 8)
            {
                Program.qmenu.AddItem(new MenuItem("smartq", "使用智能 Q").SetValue(true));
                Program.qmenu.AddItem(new MenuItem("qspam", "Ignore Checks AKA Spam Q").SetValue(true));
                
            }
                Program.qmenu.AddItem(new MenuItem("hq", "使用 Q 騷擾").SetValue(true));
                Program.qmenu.AddItem(new MenuItem("restrictq", "限制使用Q次數?").SetValue(true));
                Program.qmenu.AddItem(new MenuItem("UseQJ", "使用 Q 農兵").SetValue(true));                           
                Program.qmenu.AddItem(new MenuItem("Junglemana", "多少x小兵數量使用Q").SetValue(new Slider(60, 1, 100)));
                Program.qmenu.AddItem(new MenuItem("AntiMQ", "使用防突進 [Q]").SetValue(true));             
                Program.qmenu.AddItem(new MenuItem("FocusTwoW", "疊2次數 W ").SetValue(true));
                //qmenu.AddItem(new MenuItem("DrawQ", "Draw Q Arrow").SetValue(true));


                Program.emenu = Program.menu.AddSubMenu(new Menu("E閃設置", "Condemn"));
                Program.emenu.AddItem(new MenuItem("UseEC", "使用 E 連招").SetValue(true));
                Program.emenu.AddItem(new MenuItem("he", "使用 E 騷擾").SetValue(true));
                Program.emenu.AddItem(new MenuItem("UseCF", "使用 E閃現").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Press)));
                Program.emenu.AddItem(new MenuItem("UseCFA", "使用 E閃現 (開/關)").SetValue(new KeyBind("O".ToCharArray()[0], KeyBindType.Toggle)).SetTooltip("Auto Flash Condemn when you have % HP")).Permashow(true, "Vayne | Auto Flash Condemn", Color.Aqua);
                Program.emenu.AddItem(new MenuItem("UseCFHP", "使用 E閃現血量低於%").SetValue(new Slider(25))); 
                Program.emenu.AddItem(new MenuItem("UseET", "使用 E (開/關)").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
                Program.emenu.AddItem(new MenuItem("zzrot", "[測試] ZZrotE閃現").SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle))).Permashow(true, "Vayne | ZZRot Toggle", Color.Aqua);
                // emenu.AddItem(new MenuItem("FlashE", "Flash E").SetValue(true).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));


                //emenu.AddItem(new MenuItem("Gap_E", "Use E To Gabcloser").SetValue(true));
                // emenu.AddItem(new MenuItem("GapD", "Anti GapCloser Delay").SetValue(new Slider(0, 0, 1000)).SetTooltip("Sets a delay before the Condemn for Antigapcloser is casted."));
                Program.emenu.AddItem(new MenuItem("EMode", "使用 E 模式:", true).SetValue(new StringList(new[] { "Lord's", "Gosu", "Flowers", "VHR", "Marksman", "Sharpshooter", "OKTW", "Shine", "PRADASMART", "PRADAPERFECT", "OLDPRADA", "PRADALEGACY" })));
                Program.emenu.AddItem(new MenuItem("PushDistance", "E 推的距離").SetValue(new Slider(415, 475, 300)));
                Program.emenu.AddItem(new MenuItem("EHitchance", "E 命中率").SetValue(new Slider(50, 1, 100)).SetTooltip("只用於Prada Condemn方法"));
                Program.emenu.AddItem(new MenuItem("UseEaa", "自動使用 E ").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Press)));


                Program.rmenu = Program.menu.AddSubMenu(new Menu("大招", "Ult"));
                Program.rmenu.AddItem(new MenuItem("visibleR", "智能使用 R").SetValue(true).SetTooltip("Wether you want to set a delay to stay in R before you Q"));
                Program.rmenu.AddItem(new MenuItem("Qtime", "等待時間").SetValue(new Slider(700, 0, 1000)));

                Program.imenu = Program.menu.AddSubMenu(new Menu("中斷技能設置", "Interrupt Settings"));
                Program.imenu.AddItem(new MenuItem("Int_E", "使用E中斷技能").SetValue(true));
                Program.imenu.AddItem(new MenuItem("Interrupt", "中斷技能危險技能", true).SetValue(true));
                Program.imenu.AddItem(new MenuItem("AntiAlistar", "中斷技能亞歷斯塔 W", true).SetValue(true));
                Program.imenu.AddItem(new MenuItem("AntiRengar", "中斷技能雷葛爾 跳", true).SetValue(true));
                Program.imenu.AddItem(new MenuItem("AntiKhazix", "中斷技能卡力斯 R", true).SetValue(true));
                Program.imenu.AddItem(new MenuItem("AntiKhazix", "中斷技能卡力斯 R", true).SetValue(true));


                Program.gmenu = Program.menu.AddSubMenu(new Menu("防突進", "Gap Closer"));
                Program.gmenu.AddItem(new MenuItem("Gapcloser", "Anti Gapcloser", true).SetValue(false));
                foreach (var target in HeroManager.Enemies)
                {
                    Program.gmenu.AddItem(
                        new MenuItem("AntiGapcloser" + target.ChampionName.ToLower(), target.ChampionName, true)
                            .SetValue(false));
                }


                
                Program.menu.AddItem(new MenuItem("useR", "連招時使用R").SetValue(false));
                Program.menu.AddItem(new MenuItem("enemys", "當敵人數量 >=").SetValue(new Slider(2, 1, 5)));



                Q = new Spell(SpellSlot.Q, 0f);
                R = new Spell(SpellSlot.R, float.MaxValue);
                E = new Spell(SpellSlot.E, 650f);
                E.SetTargetted(0.25f, 1600f);

               

                E.SetTargetted(0.25f, 2200f);
                Obj_AI_Base.OnProcessSpellCast += Events.Game_SpellProcess.Game_ProcessSpell;
                Obj_AI_Base.OnProcessSpellCast += Events.OnSpellProcess.Obj_AI_Base_OnProcessSpellCast;
                Game.OnUpdate += Events.GameUpdate.Game_OnGameUpdate;
                Orbwalking.AfterAttack += Events.AfterAttack.Orbwalking_AfterAttack;
                Orbwalking.BeforeAttack += Events.BeforeAttack.Orbwalking_BeforeAttack;
                AntiGapcloser.OnEnemyGapcloser += Events.AntiGapCloser.AntiGapcloser_OnEnemyGapcloser;
                Interrupter2.OnInterruptableTarget += Events.Interrupter.Interrupter2_OnInterruptableTarget;               
                GameObject.OnCreate += Events.OnCreates.OnCreate;
                //  Drawing.OnDraw += DrawingOnOnDraw;


                //Chat.Print("<font color='#881df2'>Blm95 Vayne Reborn by LordZEDith</font> Loaded.");
                Chat.Print("<font size='30'>Lord's Vayne</font> <font color='#b756c5'>by LordZEDith</font>");
                Chat.Print("<font color='#b756c5'>NEWS: </font>" + Program.News);
                //Chat.Print(
                // "<font color='#f2f21d'>Do you like it???  </font> <font color='#ff1900'>Drop 1 Upvote in Database </font>");
                //  Chat.Print(
                //  "<font color='#f2f21d'>Buy me cigars </font> <font color='#ff1900'>ssssssssssmith@hotmail.com</font> (10) S");
                Program.menu.AddToMainMenu();
            }

        
    }
        #endregion
    }


