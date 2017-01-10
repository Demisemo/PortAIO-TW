using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Lee_Sin.Misc;
using Lee_Sin.WardManager;
using SharpDX;
using Color = System.Drawing.Color;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Lee_Sin.Drawings
{
    class OnChamp : LeeSin
    {
        public  static List<Geometry.Polygon.Rectangle> ToList;

        public static List<Geometry.Polygon.Rectangle> NewPoly { get; set; }

        public static void OnSpells(EventArgs args) 
        {
            if (Player.IsDead) return;

            if (UltPoly != null && GetBool("rpolygon", typeof(bool)))
            {
                UltPoly.Draw(Color.Red);
            }

           // DrawRect();
          

            if (RCombo != null && GetBool("rpolygon", typeof(bool))) Render.Circle.DrawCircle((Vector3)RCombo, 100, Color.Red, 5, true);

            //if (GetBool("counthitr", typeof(bool)))
            //{
            //    var getresults = BubbaKush.GetPositions(Player, 1125, (byte)GetValue("enemiescount"), HeroManager.Enemies.Where(x => x.Distance(Player) < 1200).ToList());
            //    if (getresults.Count > 1)
            //    {
            //        var getposition = BubbaKush.SelectBest(getresults, Player);
                 
            //     //   Render.Circle.DrawCircle(getposition, 100, Color.Red, 3, true);
            //    }
            //}


            if (!GetBool("spellsdraw", typeof(bool))) return;
            if (!GetBool("ovdrawings", typeof(bool))) return;
            if (GetBool("qrange", typeof(bool)) && Q.Level > 0)
            {
                var color1 = Q.IsReady() ? Color.DodgerBlue : Color.Red;
                Render.Circle.DrawCircle(Player.Position, Q.Range, color1, 2);
            }

            if (GetBool("wrange", typeof(bool)) && W.Level > 0)
            {
                var colorw = W.IsReady() ? Color.BlueViolet : Color.Red;
                Render.Circle.DrawCircle(Player.Position, W.Range, colorw, 2);
            }

            if (GetBool("erange", typeof(bool)) && E.Level > 0)
            {
                var colore = E.IsReady() ? Color.Plum : Color.Red;
                Render.Circle.DrawCircle(Player.Position, E.Range, colore, 2, true);
            }

            if (GetBool("rrange", typeof(bool)) && R.Level > 0)
            {
                var colorr = R.IsReady() ? Color.LawnGreen : Color.Red;
                Render.Circle.DrawCircle(Player.Position, R.Range, colorr, 2, true);
            }
            var target = HeroManager.Enemies.Where(x => x.Distance(Player) < R.Range && !x.IsDead && x.IsValidTarget(R.Range)).OrderBy(x => x.Distance(Player)).FirstOrDefault();
            if (target == null || Player.IsDead)
            {
                UltPoly = null;
                UltPolyExpectedPos = null;
                return;
            }

            UltPoly = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(target.Position, 1100), target.BoundingRadius + 20);
            if (GetBool("counthitr", typeof(bool)))
            {
                var counts = HeroManager.Enemies.Where(x => x.Distance(Player) < 1200 && x.IsValidTarget(1200)).Count(h => h.NetworkId != target.NetworkId && UltPoly.IsInside(h.ServerPosition));

                Drawing.DrawText(Drawing.WorldToScreen(Player.Position).X - 50, Drawing.WorldToScreen(Player.Position).Y + 30, Color.Magenta, "Ult Will Hit " + counts);
            }
        }
        
    }
}
