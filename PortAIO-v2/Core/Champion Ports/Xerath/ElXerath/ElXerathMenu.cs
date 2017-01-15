using EloBuddy; 
using LeagueSharp.Common; 
namespace ElXerath
{
    using System;

    using LeagueSharp.Common;

    public class ElXerathMenu
    {
        #region Static Fields

        public static Menu Menu;

        #endregion

        #region Public Methods and Operators

        public static void Initialize()
        {
            Menu = new Menu("El-齊勒斯", "menu", true);

            //ElXerath.Orbwalker
            var orbwalkerMenu = new Menu("走砍", "orbwalker");
            Xerath.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            //ElXerath.TargetSelector
            var targetSelector = new Menu("目標選擇器", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            Menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("連招", "Combo");
            cMenu.AddItem(new MenuItem("ElXerath.Combo.Q", "使用 Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElXerath.Combo.W", "使用 W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElXerath.Combo.E", "使用 E").SetValue(true));
            cMenu.AddItem(new MenuItem("ComboActive", "連招!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu.AddSubMenu(cMenu);

            var rMenu = new Menu("大招", "Ult");
            rMenu.AddItem(new MenuItem("ElXerath.R.AutoUseR", "自動使用").SetValue(true));
            rMenu.AddItem(
                new MenuItem("ElXerath.R.Mode", "模式: ").SetValue(
                    new StringList(new[] { "預設", "自動延遲", "OnTap", "自訂命中", "鼠標位置" })));
            rMenu.AddItem(
                new MenuItem("ElXerath.R.OnTap", "大招修改").SetValue(
                    new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            rMenu.AddItem(new MenuItem("ElXerath.R.Block", "阻止移動").SetValue(true));

            rMenu.SubMenu("CustomDelay").AddItem(new MenuItem("ElXerath.R.Delay", "自定義延遲").SetValue(true));
            for (var i = 1; i <= 5; i++)
            {
                rMenu.SubMenu("CustomDelay")
                    .SubMenu("Custom delay")
                    .AddItem(new MenuItem("Delay" + i, "延遲" + i).SetValue(new Slider(0, 1500, 0)));
            }

            rMenu.AddItem(new MenuItem("ElXerath.R.Radius", "目標半徑").SetValue(new Slider(700, 1500, 300)));

            Menu.AddSubMenu(rMenu);

            var hMenu = new Menu("騷擾", "Harass");
            hMenu.AddItem(new MenuItem("ElXerath.Harass.Q", "使用 Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElXerath.Harass.W", "使用 W").SetValue(true));
            hMenu.SubMenu("AutoHarass")
                .AddItem(
                    new MenuItem("ElXerath.AutoHarass", "[開/關] 自動騷擾", false).SetValue(
                        new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle)));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElXerath.UseQAutoHarass", "使用 Q").SetValue(true));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElXerath.UseWAutoHarass", "使用 W").SetValue(true));
            hMenu.SubMenu("AutoHarass")
                .AddItem(new MenuItem("ElXerath.harass.mana", "自動騷擾魔量管理"))
                .SetValue(new Slider(55));

            Menu.AddSubMenu(hMenu);

            var lMenu = new Menu("清線/清野", "LaneClear");
            lMenu.AddItem(new MenuItem("ElXerath.clear.Q", "清線使用 Q").SetValue(true));
            lMenu.AddItem(new MenuItem("ElXerath.clear.W", "清線使用 W").SetValue(true));
            lMenu.AddItem(new MenuItem("fasfsafsafsasfasfa", ""));
            lMenu.AddItem(new MenuItem("ElXerath.jclear.Q", "清野使用 Q").SetValue(true));
            lMenu.AddItem(new MenuItem("ElXerath.jclear.W", "清野使用 W").SetValue(true));
            lMenu.AddItem(new MenuItem("ElXerath.jclear.E", "清野使用 E").SetValue(true));
            lMenu.AddItem(new MenuItem("fasfsafsafsadsasasfasfa", ""));
            lMenu.AddItem(new MenuItem("minmanaclear", "魔量管理")).SetValue(new Slider(55));

            Menu.AddSubMenu(lMenu);

            //ElXerath.Misc
            var miscMenu = new Menu("雜項", "Misc");
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.off", "關閉顯示").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.Q", "顯示 Q 範圍").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.W", "顯示 W 範圍").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.E", "顯示 E 範圍").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.R", "顯示 R 範圍").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.Text", "顯示文字").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElXerath.Draw.RON", "顯示 R 目標半徑").SetValue(true));
            miscMenu.AddItem(new MenuItem("useEFafsdsgdrmddsddsasfsasdsdsaadsd", ""));
            miscMenu.AddItem(new MenuItem("ElXerath.Ignite", "使用點燃").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElXerath.misc.ks", "搶頭模式").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElXerath.misc.Antigapcloser", "反突進").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElXerath.misc.Notifications", "使用通知").SetValue(true));
            miscMenu.AddItem(new MenuItem("useEdaadaDFafsdsgdrmddsddsasfsasdsdsaadsd", ""));
            miscMenu.AddItem(
                new MenuItem("ElXerath.Misc.E", "使用 E 熱鍵").SetValue(
                    new KeyBind("H".ToCharArray()[0], KeyBindType.Press)));
            miscMenu.AddItem(new MenuItem("useEdaadaDFafsddssdsgdrmddsddsasfsasdsdsaadsd", ""));
            miscMenu.AddItem(
                new MenuItem("ElXerath.hitChance", "Hitchance Q").SetValue(
                    new StringList(new[] { "低", "中", "高", "非常高" }, 3)));

            Menu.AddSubMenu(miscMenu);

            Menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            Menu.AddItem(new MenuItem("422442fsaafsf", "版本: 1.0.0.6"));
            Menu.AddItem(new MenuItem("fsasfafsfsafsa", "作者: By jQuery"));

            Menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }

        #endregion
    }
}