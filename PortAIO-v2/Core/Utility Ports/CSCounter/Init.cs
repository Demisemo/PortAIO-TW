using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

using EloBuddy; 
using LeagueSharp.Common; 
namespace CS_Counter
{
    internal class Init
    {

        public static Menu Menu2;
        public static MenuItem Menuenable2;
        public static MenuItem Menuenable3;
        public static MenuItem Menuenable4;
        public static MenuItem Advanced;
        public static MenuItem Advanced_box;

        public static MenuItem XPos;
        public static MenuItem YPos;

        public static void PrepareMenu()
        {
            Notifications.AddNotification("CS Counter loaded.", 100);

            Menu2 = new Menu("計算補兵數/吃兵數", "menu2", true);

            CsCounter.Line = new Line(Drawing.Direct3DDevice);

            CsCounter.Textx = new Font(
                    Drawing.Direct3DDevice,
                    new FontDescription
                    {
                        FaceName = "Calibri",
                        Height = 13,
                        OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Default
                    });

            Menuenable2 = new MenuItem("menu.drawings.enable2", "使用計算吃兵數").SetValue(true);
            Menu2.AddItem(Menuenable2);
            Menuenable3 = new MenuItem("menu.drawings.enable3", "自身吃小兵數").SetValue(true);
            Menu2.AddItem(Menuenable3);
            Menuenable4 = new MenuItem("menu.drawings.enable4", "隊友吃小兵數").SetValue(true);
            Menu2.AddItem(Menuenable4);
            Advanced = new MenuItem("menu.drawings.advanced", "高級農兵").SetValue(false);
            Menu2.AddItem(Advanced);
            Advanced_box = new MenuItem("menu.drawings.advanced_box", "關閉盒子").SetValue(false);
            Menu2.AddItem(Advanced_box);
            XPos = new MenuItem("menu.Calc.calc5", "X - 位置").SetValue(new Slider(0, -100));
            Menu2.AddItem(XPos);
            YPos = new MenuItem("menu.Calc.calc6", "Y - 位置").SetValue(new Slider(0, -100));
            Menu2.AddItem(YPos);


            Menu2.AddToMainMenu();
        }
    }
}
