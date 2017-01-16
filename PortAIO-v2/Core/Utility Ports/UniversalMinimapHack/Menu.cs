using System.Drawing;
using LeagueSharp.Common;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace UniversalMinimapHack
{
    public class Menu : LeagueSharp.Common.Menu
    {
        private readonly MenuItem _iconOpacity;
        private readonly MenuItem _slider;
        private readonly MenuItem _ssCircle;
        private readonly MenuItem _ssCircleColor;
        private readonly MenuItem _ssCircleSize;
        private readonly MenuItem _ssFallbackPing;
        private readonly MenuItem _ssTimerEnabler;
        private readonly MenuItem _ssTimerMin;
        private readonly MenuItem _ssTimerMinPing;
        private readonly MenuItem _ssTimerOffset;
        private readonly MenuItem _ssTimerSize;

        public Menu() : base("顯示敵人最後位置", "UniversalMinimapHack", true)
        {
            _slider = new MenuItem("scale", "圖標比例 % (按下 F5 重新加載)").SetValue(new Slider(20));
            _iconOpacity = new MenuItem("opacity", "圖標不透明度 % (按下 F5 重新加載)").SetValue(new Slider(70));
            _ssTimerEnabler = new MenuItem("enableSS", "啟用").SetValue(true);
            _ssTimerSize = new MenuItem("sizeSS", "SS 文字大小 (按下 F5 重新加載)").SetValue(new Slider(15));
            _ssTimerOffset = new MenuItem("offsetSS", "SS 文字高度").SetValue(new Slider(15, -50, +50));
            _ssTimerMin = new MenuItem("minSS", "顯示後 X 秒").SetValue(new Slider(30, 1, 180));
            _ssTimerMinPing = new MenuItem("minPingSS", "Ping 之後 X 秒").SetValue(new Slider(30, 5, 180));
            _ssFallbackPing = new MenuItem("fallbackSS", "退卻 Ping (本地)").SetValue(false);
            AddItem(new MenuItem("", "[定制]"));
            AddItem(_slider);
            AddItem(_iconOpacity);
            var ssMenu = new LeagueSharp.Common.Menu("SS 時間", "ssTimer");
            ssMenu.AddItem(_ssTimerEnabler);
            ssMenu.AddItem(new MenuItem("1", "--- [額外] ---"));
            ssMenu.AddItem(_ssTimerMin);
            ssMenu.AddItem(_ssFallbackPing);
            ssMenu.AddItem(_ssTimerMinPing);
            ssMenu.AddItem(new MenuItem("2", "--- [定制] ---"));
            ssMenu.AddItem(_ssTimerSize);
            ssMenu.AddItem(_ssTimerOffset);
            var ssCircleMenu = new LeagueSharp.Common.Menu("SS 圓圈", "ccCircles");
            _ssCircle = new MenuItem("ssCircle", "啟用").SetValue(true);
            _ssCircleSize = new MenuItem("ssCircleSize", "最大圓圈大小").SetValue(new Slider(7000, 500, 15000));
            _ssCircleColor = new MenuItem("ssCircleColor", "圓圈顏色").SetValue(System.Drawing.Color.Green);
            ssCircleMenu.AddItem(_ssCircle);
            ssCircleMenu.AddItem(_ssCircleSize);
            ssCircleMenu.AddItem(_ssCircleColor);
            AddSubMenu(ssMenu);
            AddSubMenu(ssCircleMenu);
            AddToMainMenu();
        }

        public float IconScale
        {
            get { return _slider.GetValue<Slider>().Value / 100f; }
        }

        public float IconOpacity
        {
            get { return _iconOpacity.GetValue<Slider>().Value / 100f; }
        }

        // ReSharper disable once InconsistentNaming
        public bool SSTimer
        {
            get { return _ssTimerEnabler.GetValue<bool>(); }
        }

        // ReSharper disable once InconsistentNaming
        public int SSTimerSize
        {
            get { return _ssTimerSize.GetValue<Slider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public int SSTimerOffset
        {
            get { return _ssTimerOffset.GetValue<Slider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public int SSTimerStart
        {
            get { return _ssTimerMin.GetValue<Slider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public bool Ping
        {
            get { return _ssFallbackPing.GetValue<bool>(); }
        }

        // ReSharper disable once InconsistentNaming
        public int MinPing
        {
            get { return _ssTimerMinPing.GetValue<Slider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public bool SSCircle
        {
            get { return _ssCircle.GetValue<bool>(); }
        }

        // ReSharper disable once InconsistentNaming
        public int SSCircleSize
        {
            get { return _ssCircleSize.GetValue<Slider>().Value; }
        }

        // ReSharper disable once InconsistentNaming
        public Color SSCircleColor
        {
            get { return _ssCircleColor.GetValue<Color>(); }
        }
    }
}