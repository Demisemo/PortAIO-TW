using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace zedisback
{
    internal class AssassinManager
    {
        public AssassinManager()
        {
            Load();
        }

        private static void Load()
        {
            Program.TargetSelectorMenu.AddSubMenu(new Menu("Assassin Manager", "MenuAssassin"));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinActive", "Active").SetValue(true));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("Ax", ""));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinSelectOption", "Set: ").SetValue(new StringList(new[] {"Single Select", "Multi Select"})));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("Ax", ""));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinSetClick", "Add/Remove with Right-Click").SetValue(true));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinReset", "Reset List").SetValue(new KeyBind("L".ToCharArray()[0],KeyBindType.Press)));

            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddSubMenu(new Menu("Draw:", "Draw"));

            Program.TargetSelectorMenu.SubMenu("MenuAssassin").SubMenu("Draw").AddItem(new MenuItem("DrawSearch", "Search Range").SetValue(new Circle(true,Color.Gray)));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").SubMenu("Draw").AddItem(new MenuItem("DrawActive", "Active Enemy").SetValue(new Circle(true,Color.GreenYellow)));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").SubMenu("Draw").AddItem(new MenuItem("DrawNearest", "Nearest Enemy").SetValue(new Circle(true,Color.DarkSeaGreen)));
            

            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddSubMenu(new Menu("Assassin List:", "AssassinMode"));
            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                Program.TargetSelectorMenu.SubMenu("MenuAssassin")
                    .SubMenu("AssassinMode")
                    .AddItem(
                        new MenuItem("Assassin" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(
                            TargetSelector.GetPriority(enemy) > 3));
            }
            Program.TargetSelectorMenu.SubMenu("MenuAssassin")
                .AddItem(new MenuItem("AssassinSearchRange", "Search Range")).SetValue(new Slider(1000, 2000));

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        static void ClearAssassinList()
        {
            foreach (
                var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.IsEnemy)) 
            {
                
                Program.TargetSelectorMenu.Item("Assassin" + enemy.BaseSkinName).SetValue(false);
                
            }
        }
        private static void OnUpdate(EventArgs args)
        {
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            
            if (Program.TargetSelectorMenu.Item("AssassinReset").GetValue<KeyBind>().Active && args.Msg == 257)
            {
                ClearAssassinList();
                Chat.Print(
                    "<font color='#FFFFFF'>Reset Assassin List is Complete! Click on the enemy for Add/Remove.</font>");
            }

            if (args.Msg != 0x201)
            {
                return;
            }

            if (Program.TargetSelectorMenu.Item("AssassinSetClick").GetValue<bool>())
            {
                foreach (var objAiHero in from hero in ObjectManager.Get<AIHeroClient>()
                                          where hero.IsValidTarget()
                                          select hero
                                              into h
                                              orderby h.Distance(Game.CursorPos) descending
                                              select h
                                                  into enemy
                                                  where enemy.Distance(Game.CursorPos) < 150f
                                                  select enemy)
                {
                    if (objAiHero != null && objAiHero.IsVisible && !objAiHero.IsDead)
                    {
                        var xSelect =
                            Program.TargetSelectorMenu.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex;

                        switch (xSelect)
                        {
                            case 0:
                                ClearAssassinList();
                                Program.TargetSelectorMenu.Item("Assassin" + objAiHero.BaseSkinName).SetValue(true);
                                Chat.Print(
                                    string.Format(
                                        "<font color='FFFFFF'>Added to Assassin List</font> <font color='#09F000'>{0} ({1})</font>",
                                        objAiHero.Name, objAiHero.BaseSkinName));
                                break;
                            case 1:
                                var menuStatus = Program.TargetSelectorMenu.Item("Assassin" + objAiHero.BaseSkinName).GetValue<bool>();
                                Program.TargetSelectorMenu.Item("Assassin" + objAiHero.BaseSkinName).SetValue(!menuStatus);
                                Chat.Print(
                                    string.Format("<font color='{0}'>{1}</font> <font color='#09F000'>{2} ({3})</font>",
                                        !menuStatus ? "#FFFFFF" : "#FF8877",
                                        !menuStatus ? "Added to Assassin List:" : "Removed from Assassin List:",
                                        objAiHero.Name, objAiHero.BaseSkinName));
                                break;
                        }
                    }
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Program.TargetSelectorMenu.Item("AssassinActive").GetValue<bool>())
                return;

            var drawSearch = Program.TargetSelectorMenu.Item("DrawSearch").GetValue<Circle>();
            var drawActive = Program.TargetSelectorMenu.Item("DrawActive").GetValue<Circle>();
            var drawNearest = Program.TargetSelectorMenu.Item("DrawNearest").GetValue<Circle>();

            var drawSearchRange = Program.TargetSelectorMenu.Item("AssassinSearchRange").GetValue<Slider>().Value;
            if (drawSearch.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, drawSearchRange, drawSearch.Color);
            }

            foreach (
                var enemy in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(enemy => enemy.Team != ObjectManager.Player.Team)
                        .Where(
                            enemy =>
                                enemy.IsVisible &&
                                Program.TargetSelectorMenu.Item("Assassin" + enemy.BaseSkinName) != null &&
                                !enemy.IsDead)
                        .Where(
                            enemy => Program.TargetSelectorMenu.Item("Assassin" + enemy.BaseSkinName).GetValue<bool>()))
            {
                if (ObjectManager.Player.Distance(enemy.ServerPosition) < drawSearchRange)
                {
                    if (drawActive.Active)
                        Render.Circle.DrawCircle(enemy.Position, 85f, drawActive.Color);
                }
                else if (ObjectManager.Player.Distance(enemy.ServerPosition) > drawSearchRange &&
                         ObjectManager.Player.Distance(enemy.ServerPosition) < drawSearchRange + 400) 
                {
                    if (drawNearest.Active)
                        Render.Circle.DrawCircle(enemy.Position, 85f, drawNearest.Color);
                }
            }
        }
    }
}
