using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Slutty_ryze
{
    class DrawManager
    {
        #region Variable Declaration
        private const int XOffset = 10;
        private const int YOffset = 20;
        private const int Width = 103;
        private const int Height = 8;
        private static readonly Render.Text Text = new Render.Text(0, 0, "", 14, Color.Red, "monospace");
        private static readonly System.Drawing.Color _color = System.Drawing.Color.Lime;
        private static readonly System.Drawing.Color _fillColor = System.Drawing.Color.Goldenrod;
        private static readonly System.Drawing.Color _colorblind = System.Drawing.Color.LightBlue;
        private static readonly System.Drawing.Color _fillColorblind = System.Drawing.Color.Teal;
        #endregion
        #region Private Fuctions
        private static System.Drawing.Color GetColor(bool b)
        {
            return b ? System.Drawing.Color.DarkGreen : System.Drawing.Color.Red;
        }

        private static string BoolToString(bool b)
        {
            return b ? "ON" : "OFF";
        }

        private static System.Drawing.Color GetColorblind(bool c)
        {
            return c ? System.Drawing.Color.Teal : System.Drawing.Color.Magenta;
        }

        private static string BoolToStringblind(bool c)
        {
            return c ? "ON" : "OFF";    
        }

        private static string KeyToString(KeyBind key)
        {
            var sKey = key.Key.ToString();
            var iKey = int.Parse(sKey);
            return iKey > 90 ? sKey : ((char)iKey).ToString();
        }

        private static void DrawKeys(Vector2 pos)
        {
            switch (GlobalManager.Config.Item("drawoptions").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    Drawing.DrawLine(new Vector2(pos.X - 25, pos.Y + 20), new Vector2(pos.X + 150, pos.Y + 20), 2,
                        System.Drawing.Color.SteelBlue);
                  //  if (LaneOptions.newPoly != null)
                   // LaneOptions.newPoly.Draw(System.Drawing.Color.Blue);
                    var col = 0;
                    Drawing.DrawText(pos.X, pos.Y, System.Drawing.Color.SteelBlue, "Key Table");

                    Drawing.DrawText(pos.X, ++col * 25 + pos.Y, System.Drawing.Color.SteelBlue, "Stack Tear Key:{0}" + GlobalManager.Config.Item("tearS").GetValue<KeyBind>());
                        //KeyToString(GlobalManager.Config.Item("tearS").GetValue<KeyBind>()));


                    Drawing.DrawText(pos.X, ++col * 25 + pos.Y, System.Drawing.Color.SteelBlue, "Disable Lane Clear Key: " + GlobalManager.Config.Item("disablelane").GetValue<KeyBind>());
                        //KeyToString(GlobalManager.Config.Item("disablelane").GetValue<KeyBind>()));

                    Drawing.DrawLine(new Vector2(pos.X - 25, ++col * 25 + pos.Y), new Vector2(pos.X + 150, col * 25 + pos.Y),
                        2, System.Drawing.Color.SteelBlue);
                    break;
                case 1:
                    Drawing.DrawLine(new Vector2(pos.X - 25, pos.Y + 20), new Vector2(pos.X + 150, pos.Y + 20), 2,
                        System.Drawing.Color.LightBlue);

                    var col1 = 0;
                    Drawing.DrawText(pos.X, pos.Y, System.Drawing.Color.LightBlue, "Key Table");

                    Drawing.DrawText(pos.X, ++col1 * 25 + pos.Y, System.Drawing.Color.LightBlue, "Stack Tear Key:{0}" + GlobalManager.Config.Item("tearS").GetValue<KeyBind>());
                        //KeyToString(GlobalManager.Config.Item("tearS").GetValue<KeyBind>()));

                    Drawing.DrawText(pos.X, ++col1 * 25 + pos.Y, System.Drawing.Color.LightBlue, "Disable Lane Clear Key: " + GlobalManager.Config.Item("disablelane").GetValue<KeyBind>());
                        //KeyToString(GlobalManager.Config.Item("disablelane").GetValue<KeyBind>()));

                    Drawing.DrawLine(new Vector2(pos.X - 25, ++col1 * 25 + pos.Y), new Vector2(pos.X + 150, col1 * 25 + pos.Y),
                        2, System.Drawing.Color.LightBlue);
                    break;
            }
        }


        #endregion
        #region Public Functions
        public static void Drawing_OnDraw(EventArgs args)
        {
            GlobalManager.EnableDrawingDamage = GlobalManager.Config.Item("RushDrawEDamage").GetValue<bool>();
            GlobalManager.EnableFillDamage = GlobalManager.Config.Item("RushDrawWDamageFill").GetValue<Circle>().Active;
            GlobalManager.DamageFillColor = GlobalManager.Config.Item("RushDrawWDamageFill").GetValue<Circle>().Color;
            if (GlobalManager.GetHero.IsDead)
                return;
            if (!GlobalManager.Config.Item("Draw").GetValue<bool>())
                return;

            if (GlobalManager.Config.Item("keyBindDisplay").GetValue<bool>())
                DrawKeys(new Vector2(Drawing.Width - 250, (float)Drawing.Height / 2));

           // if (!GlobalManager.GetHero.Position.IsOnScreen())
             //   return;

            var tears = GlobalManager.Config.Item("tearS").GetValue<KeyBind>().Active;

            var laneclear = GlobalManager.Config.Item("disablelane").GetValue<KeyBind>().Active;

            var heroPosition = Drawing.WorldToScreen(GlobalManager.GetHero.Position);
            var textDimension = Drawing.GetTextEntent(("Stunnable!"), 15);

            switch (GlobalManager.Config.Item("drawoptions").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    if (GlobalManager.Config.Item("qDraw").GetValue<bool>() && Champion.Q.Level > 0)
                        Render.Circle.DrawCircle(GlobalManager.GetHero.Position, Champion.Q.Range, System.Drawing.Color.Green, 3);
                    if (GlobalManager.Config.Item("eDraw").GetValue<bool>() && Champion.E.Level > 0)
                        Render.Circle.DrawCircle(GlobalManager.GetHero.Position, Champion.E.Range, System.Drawing.Color.Gold, 3);
                    if (GlobalManager.Config.Item("wDraw").GetValue<bool>() && Champion.W.Level > 0)
                        Render.Circle.DrawCircle(GlobalManager.GetHero.Position, Champion.W.Range, System.Drawing.Color.Blue, 3);

                    if (!GlobalManager.Config.Item("notdraw").GetValue<bool>()) return;

                    Drawing.DrawText(heroPosition.X - textDimension.Width, heroPosition.Y - textDimension.Height,
                        GetColor(tears),
                        "Tear Stack: " + BoolToString(tears));

                    Drawing.DrawText(heroPosition.X + 20, heroPosition.Y - 30, GetColor(laneclear),
                        "Lane Clear: " + BoolToString(laneclear));
                    break;
                case 1:
                    if (GlobalManager.Config.Item("qDraw").GetValue<bool>() && Champion.Q.Level > 0)
                        Render.Circle.DrawCircle(GlobalManager.GetHero.Position, Champion.Q.Range, System.Drawing.Color.Teal, 3);
                    if (GlobalManager.Config.Item("eDraw").GetValue<bool>() && Champion.E.Level > 0)
                        Render.Circle.DrawCircle(GlobalManager.GetHero.Position, Champion.E.Range, System.Drawing.Color.Magenta, 3);
                    if (GlobalManager.Config.Item("wDraw").GetValue<bool>() && Champion.W.Level > 0)
                        Render.Circle.DrawCircle(GlobalManager.GetHero.Position, Champion.W.Range, System.Drawing.Color.Black, 3);

                    if (!GlobalManager.Config.Item("notdraw").GetValue<bool>()) return;

                    Drawing.DrawText(heroPosition.X - textDimension.Width, heroPosition.Y - textDimension.Height,
                        GetColorblind(tears),
                        "Tear Stack: " + BoolToStringblind(tears));
                    Drawing.DrawText(heroPosition.X + 20, heroPosition.Y - 30, GetColorblind(laneclear),
                        "Lane Clear: " + BoolToStringblind(laneclear));
                    break;
            }
        }

        public static void Drawing_OnDrawChamp(EventArgs args)
        {
            if (!GlobalManager.EnableDrawingDamage || GlobalManager.DamageToUnit == null)
                return;


            foreach (var unit in HeroManager.Enemies.Where(h => h.IsValid && h.IsHPBarRendered))
            {
                var barPos = unit.HPBarPosition;
                var damage = GlobalManager.DamageToUnit(unit);
                var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                var yPos = barPos.Y + YOffset;
                var xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                var xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                if (damage > unit.Health)
                {
                    Text.X = (int)barPos.X + XOffset;
                    Text.Y = (int)barPos.Y + YOffset - 13;
                    Text.text = "Killable With Combo Rotation " + (unit.Health - damage);
                    Text.OnEndScene();
                }
                switch (GlobalManager.Config.Item("drawoptions").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + Height, 1, _color);

                        if (!GlobalManager.EnableFillDamage) continue;
                        var differenceInHp = xPosCurrentHp - xPosDamage;
                        var pos1 = barPos.X + 9 + (107 * percentHealthAfterDamage);
                        for (var i = 0; i < differenceInHp; i++)
                        {
                            Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, _fillColor);
                        }
                        break;
                    case 1:
                        Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + Height, 1, _colorblind);

                        if (!GlobalManager.EnableFillDamage) continue;
                        var differenceInHp1 = xPosCurrentHp - xPosDamage;
                        var pos11 = barPos.X + 9 + (107 * percentHealthAfterDamage);
                        for (var i = 0; i < differenceInHp1; i++)
                        {
                            Drawing.DrawLine(pos11 + i, yPos, pos11 + i, yPos + Height, 1, _fillColorblind);
                        }
                        break;
                }
            }
        }
        #endregion
    }
}
