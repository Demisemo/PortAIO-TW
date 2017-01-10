using EloBuddy; 
using LeagueSharp.Common; 
namespace NechritoRiven
{
    #region

    using System.Drawing;

    using LeagueSharp.Common;

    using Color = SharpDX.Color;

    #endregion

    internal class MenuConfig : Core.Core
    {
        #region Constants

        private const string MenuName = "Nechrito 雷玟";

        #endregion

        #region Static Fields

        private static Menu config;

        #endregion

        #region Public Properties

        public static bool AlwaysF => config.Item("AlwaysF").GetValue<KeyBind>().Active;

        public static bool UseR1 => config.Item("UseR1").GetValue<KeyBind>().Active;

        public static bool LaneQFast => config.Item("laneQFast").GetValue<bool>();

        public static bool AnimDance => config.Item("animDance").GetValue<bool>();

        public static bool AnimLaugh => config.Item("animLaugh").GetValue<bool>();

        public static bool AnimTalk => config.Item("animTalk").GetValue<bool>();

        public static bool AnimTaunt => config.Item("animTaunt").GetValue<bool>();

        public static bool CancelPing => config.Item("CancelPing").GetValue<bool>();

        public static bool Dind => config.Item("Dind").GetValue<bool>();

        public static bool DrawAlwaysR => config.Item("DrawAlwaysR").GetValue<bool>();

        public static bool DrawBt => config.Item("DrawBT").GetValue<bool>();

        public static bool DrawCb => config.Item("DrawCB").GetValue<bool>();

        public static bool DrawFh => config.Item("DrawFH").GetValue<bool>();

        public static bool DrawHs => config.Item("DrawHS").GetValue<bool>();

        public static StringList EmoteList => config.Item("EmoteList").GetValue<StringList>();

        public static bool FleeSpot => config.Item("FleeSpot").GetValue<bool>();

        public static bool FleeYomuu => config.Item("FleeYoumuu").GetValue<bool>();

        public static bool ForceFlash => config.Item("DrawForceFlash").GetValue<bool>();

        public static bool GapcloserMenu => config.Item("GapcloserMenu").GetValue<bool>();

        public static bool Ignite => config.Item("ignite").GetValue<bool>();

        public static bool KsW => config.Item("ksW").GetValue<bool>();

        public static bool KsR2 => config.Item("ksR2").GetValue<bool>();

        public static bool InterruptMenu => config.Item("InterruptMenu").GetValue<bool>();

        public static bool IreliaLogic => config.Item("IreliaLogic").GetValue<bool>();

        public static bool JnglE => config.Item("JungleE").GetValue<bool>();

        public static bool JnglQ => config.Item("JungleQ").GetValue<bool>();

        public static bool JnglW => config.Item("JungleW").GetValue<bool>();

        public static bool KeepQ => config.Item("KeepQ").GetValue<bool>();

        public static bool LaneE => config.Item("LaneE").GetValue<bool>();

        public static bool LaneEnemy => config.Item("LaneEnemy").GetValue<bool>();

        public static bool SafeR1 => config.Item("SafeR1").GetValue<bool>();

        public static bool LaneQ => config.Item("LaneQ").GetValue<bool>();

        public static bool LaneW => config.Item("LaneW").GetValue<bool>();

        public static bool Doublecast => config.Item("Doublecast").GetValue<bool>();

        public static bool Flash => config.Item("FlashOften").GetValue<bool>();

        public static bool OverKillCheck => config.Item("OverKillCheck").GetValue<bool>();

        public static int Q2D => config.Item("Q2D").GetValue<Slider>().Value;

        public static int Qd => config.Item("QD").GetValue<Slider>().Value;

        public static int Qld => config.Item("Q3D").GetValue<Slider>().Value;

        public static bool QMove => config.Item("QMove").GetValue<KeyBind>().Active;

        public static bool QReset => config.Item("qReset").GetValue<bool>();

        public static bool R2Draw => config.Item("R2Draw").GetValue<bool>();

        public static StringList SkinList => config.Item("SkinList").GetValue<StringList>();

        public static bool UseSkin => config.Item("UseSkin").GetValue<bool>();

        public static bool WallFlee => config.Item("WallFlee").GetValue<bool>();

        public static bool Q3Wall => config.Item("Q3Wall").GetValue<bool>();

        public static bool UltHarass => config.Item("UltHarass").GetValue<bool>();

        public static int WallWidth => config.Item("WallWidth").GetValue<Slider>().Value;

        #endregion

        #region Public Methods and Operators

        public static void LoadMenu()
        {
            config = new Menu(MenuName, MenuName, true).SetFontStyle(FontStyle.Bold, Color.Cyan);

            var orbwalker = new Menu("走砍", "rorb");
            Orbwalker = new Orbwalking.Orbwalker(orbwalker);
            config.AddSubMenu(orbwalker);

            var animation = new Menu("減少QA動畫", "Animation");
            animation.AddItem(new MenuItem("QD", "Q1 Ping").SetValue(new Slider(205, 205, 340)));
            animation.AddItem(new MenuItem("Q2D", "Q2 Ping").SetValue(new Slider(205, 205, 340)));
            animation.AddItem(new MenuItem("Q3D", "Q3 Ping").SetValue(new Slider(340, 340, 380)));
            animation.AddItem(new MenuItem("CancelPing", "包括Ping").SetValue(true));
            animation.AddItem(new MenuItem("EmoteList", "表情").SetValue(new StringList(new[] { "說笑話", "嘲諷", "跳舞", "大笑", "切換" }, 3)));
            config.AddSubMenu(animation);

            var combo = new Menu("連招", "Combo");
            combo.AddItem(new MenuItem("Q3Wall", "過牆").SetValue(true));
            combo.AddItem(new MenuItem("FlashOften", "閃現順發模式").SetValue(false).SetTooltip("Will flash if killable, always."));
            combo.AddItem(new MenuItem("OverKillCheck", "R2 最大傷害").SetValue(true));
            combo.AddItem(new MenuItem("Doublecast", "Doublecast").SetValue(true)).SetTooltip("快速連招時每秒傷害");
            combo.AddItem(new MenuItem("UltHarass", "在騷擾中使用大招 (只在擊殺時)").SetValue(false));
            combo.AddItem(new MenuItem("UseR1", "使用 R").SetValue(new KeyBind('G', KeyBindType.Toggle)));
            combo.AddItem(new MenuItem("AlwaysF", "使用 閃現").SetValue(new KeyBind('L', KeyBindType.Toggle)));
            config.AddSubMenu(combo);

            var lane = new Menu("農兵", "Lane");
            lane.AddItem(new MenuItem("LaneEnemy", "停止如果附近有敵人").SetValue(true));
            lane.AddItem(new MenuItem("laneQFast", "快速清線").SetValue(true));
            lane.AddItem(new MenuItem("LaneQ", "使用 Q").SetValue(true));
            lane.AddItem(new MenuItem("LaneW", "使用 W").SetValue(true));
            lane.AddItem(new MenuItem("LaneE", "使用 E").SetValue(true));
            config.AddSubMenu(lane);

            var jngl = new Menu("清野", "Jungle");
            jngl.AddItem(new MenuItem("JungleQ", "使用 Q").SetValue(true));
            jngl.AddItem(new MenuItem("JungleW", "使用 W").SetValue(true));
            jngl.AddItem(new MenuItem("JungleE", "使用 E").SetValue(true));
            config.AddSubMenu(jngl);

            var killsteal = new Menu("搶頭", "Killsteal");
            killsteal.AddItem(new MenuItem("ignite", "使用 點燃").SetValue(true));
            killsteal.AddItem(new MenuItem("ksW", "使用 W").SetValue(true));
            killsteal.AddItem(new MenuItem("ksR2", "使用 R2").SetValue(true));
            config.AddSubMenu(killsteal);

            var misc = new Menu("雜項", "Misc");
            misc.AddItem(new MenuItem("GapcloserMenu", "防突進").SetValue(true));
            misc.AddItem(new MenuItem("InterruptMenu", "中斷技能").SetValue(true));
            misc.AddItem(new MenuItem("KeepQ", "保持 Q 靈活").SetValue(true));
            misc.AddItem(new MenuItem("QMove", "Q 移動").SetValue(new KeyBind('K', KeyBindType.Press))).SetTooltip("將Q的移動到鼠標");
            config.AddSubMenu(misc);

            var draw = new Menu("顯示", "Draw");
            draw.AddItem(new MenuItem("DrawForceFlash", "閃現 狀態").SetValue(true));
            draw.AddItem(new MenuItem("DrawAlwaysR", "R 狀態").SetValue(true));
            draw.AddItem(new MenuItem("R2Draw", "R2 傷害").SetValue(false));
            draw.AddItem(new MenuItem("Dind", "傷害指示器").SetValue(true));
            draw.AddItem(new MenuItem("FleeSpot", "顯示逃跑點").SetValue(true));
            draw.AddItem(new MenuItem("DrawCB", "連招").SetValue(true));
            draw.AddItem(new MenuItem("DrawBT", "順發").SetValue(false));
            draw.AddItem(new MenuItem("DrawFH", "快速騷擾").SetValue(false));
            draw.AddItem(new MenuItem("DrawHS", "騷擾").SetValue(false));
            config.AddSubMenu(draw);

            var flee = new Menu("逃跑", "Flee");
            flee.AddItem(new MenuItem("WallFlee", "逃跑時過牆").SetValue(true).SetTooltip("在逃跑模式時跳過牆"));
            flee.AddItem(new MenuItem("FleeYoumuu", "使用妖夢鬼刀").SetValue(true).SetTooltip("逃跑時後使用妖夢鬼刀"));
            config.AddSubMenu(flee);

            var skin = new Menu("造型更換", "SkinChanger");
            skin.AddItem(new MenuItem("UseSkin", "使用造型更換").SetValue(false)).SetTooltip("可各種切換造型");
            skin.AddItem(new MenuItem("SkinList", "造型").SetValue(new StringList(new[]
                            {
                "Default",
                "Redeemed",
                "Crimson Elite",
                "Battle Bunny",
                "Championship",
                "Dragonblade",
                "Arcade",
                "Championship 2016",
                "Chroma 1",
                "Chroma 2",
                "Chroma 3",
                "Chroma 4",
                "Chroma 5",
                "Chroma 6",
                "Chroma 7",
                "Chroma 8"
                            })));

            config.AddSubMenu(skin);

            config.AddItem(new MenuItem("version", "版本: 6.24.5").SetFontStyle(FontStyle.Bold, Color.Cyan));

            config.AddItem(new MenuItem("paypal", "贊助Paypal: nechrito@live.se").SetFontStyle(FontStyle.Regular, Color.Cyan));

            config.AddToMainMenu();
        }

        #endregion
    }
}