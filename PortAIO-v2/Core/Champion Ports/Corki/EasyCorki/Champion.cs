using EloBuddy;
using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyLib1
{
    abstract class Champion
    {
        protected AIHeroClient Player;
        protected Menu Menu;
        protected Orbwalking.Orbwalker Orbwalker;
        protected SpellManager Spells;

        private int tick = 1000 / 20;
        private int lastTick = Environment.TickCount;
        private string ChampName;
        private SkinManager SkinManager;

        public Champion(string name)
        {
            ChampName = name;

            Game_OnGameLoad();
        }

        private void Game_OnGameLoad()
        {
            Player = ObjectManager.Player;

            if (ChampName.ToLower() != Player.ChampionName.ToLower())
                return;

            SkinManager = new SkinManager();
            Spells = new SpellManager();

            InitializeSpells(ref Spells);
            InitializeSkins(ref SkinManager);

            Menu = new Menu("Easy" + ChampName, "Easy" + ChampName, true);

            SkinManager.AddToMenu(ref Menu);

            Menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            TargetSelector.AddToMenu(Menu.SubMenu("Target Selector"));

            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            InitializeMenu();

            Menu.AddItem(new MenuItem("Recall_block", "Block spells while recalling").SetValue(true));
            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Chat.Print("Easy" + ChampName + " is loaded!");
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            Draw();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Environment.TickCount < lastTick + tick) return;
            lastTick = Environment.TickCount;

            SkinManager.Update();

            Update();

            if ((Menu.Item("Recall_block").GetValue<bool>() && Player.HasBuff("Recall")) || Player.Spellbook.IsAutoAttacking)
                return;

            bool minionBlock = false;

            foreach (Obj_AI_Minion minion in MinionManager.GetMinions(Player.Position, Player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(Player, minion, false))
                    minionBlock = true;
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (!minionBlock) Harass();
                    break;
                default:
                    if (!minionBlock) Auto();
                    break;
            }
        }

        protected virtual void InitializeSkins(ref SkinManager Skins) { }
        protected virtual void InitializeSpells(ref SpellManager Spells) { }
        protected virtual void InitializeMenu() { }

        protected virtual void Update() { }
        protected virtual void Draw() { }
        protected virtual void Combo() { }
        protected virtual void Harass() { }
        protected virtual void Auto() { }

        protected void DrawCircle(string menuItem, string spell)
        {
            Circle circle = Menu.Item(menuItem).GetValue<Circle>();
            if (circle.Active) LeagueSharp.Common.Utility.DrawCircle(Player.Position, Spells.get(spell).Range, circle.Color);
        }
    }
}