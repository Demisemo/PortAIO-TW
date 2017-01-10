using EloBuddy; namespace KoreanZed.Common
{
    using System;

    using PortAIO.Properties;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    class CommonForceUltimate
    {
        public delegate void ForceUltimateDelegate(AIHeroClient target = null);

        private readonly AIHeroClient player;

        private int j, k;

        private bool leftButtonDown;

        private bool rightButtonDown;

        private readonly Render.Text Text = new Render.Text(0, 0, "No enemies found", 20, new ColorBGRA(255, 0, 0, 255));

        private readonly ZedMenu zedMenu;

        private readonly ZedSpell r;

        private readonly Orbwalking.Orbwalker zedOrbwalker;

        public CommonForceUltimate(ZedMenu zedMenu, ZedSpells zedSpells, Orbwalking.Orbwalker orbwalker)
        {
            player = ObjectManager.Player;

            this.zedMenu = zedMenu;
            r = zedSpells.R;
            zedOrbwalker = orbwalker;

            mouseImage1 = new Render.Sprite(Resources.mouse1, new Vector2(0, 0));
            mouseImage1.Scale = new Vector2(0.50f, 0.50f);
            mouseImage1.Add();

            mouseImage2 = new Render.Sprite(Resources.mouse2, new Vector2(0, 0));
            mouseImage2.Scale = new Vector2(0.50f, 0.50f);
            mouseImage2.Add();

            denyMouseImage = new Render.Sprite(Resources.denymouse, new Vector2(0, 0));
            denyMouseImage.Scale = new Vector2(0.50f, 0.50f);
            denyMouseImage.Add();
            denyMouseImage.Visible = false;

            Text.Add();
            Text.Visible = false;

            Game.OnWndProc += CheckMouseButtons;
            Game.OnUpdate += ShowAnimation;
        }

        public Render.Sprite mouseImage1 { get; set; }

        public Render.Sprite mouseImage2 { get; set; }

        public Render.Sprite denyMouseImage { get; set; }

        public ForceUltimateDelegate ForceUltimate { get; set; }

        private bool UltimateUp()
        {
            bool b;

            if (zedMenu.GetParamBool("koreanzed.miscmenu.forceultimate")
                && zedOrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                b = r.IsReady() && r.Instance.ToggleState == 0 && ObjectManager.Player.Mana > r.ManaCost;
            }
            else
            {
                b = false;
            }

            return b;
        }

        private void CheckMouseButtons(WndEventArgs args)
        {
            if (UltimateUp())
            {
                switch (args.Msg)
                {
                    case (uint)WindowsMessages.WM_LBUTTONDOWN:
                        leftButtonDown = true;
                        break;

                    case (uint)WindowsMessages.WM_RBUTTONDOWN:
                        rightButtonDown = true;
                        break;

                    case (uint)WindowsMessages.WM_LBUTTONUP:
                        leftButtonDown = false;
                        break;

                    case (uint)WindowsMessages.WM_RBUTTONUP:
                        rightButtonDown = false;
                        break;
                }

                if (leftButtonDown && rightButtonDown && (ForceUltimate != null))
                {
                    if (TargetSelector.GetTarget(r.Range, TargetSelector.DamageType.Physical) == null)
                    {
                        denyMouseImage.Visible = true;
                        Text.Visible = true;
                        Text.OnEndScene();
                        k = 0;
                    }
                    else
                    {
                        ForceUltimate();
                    }
                }
            }
            else
            {
                leftButtonDown = false;
                rightButtonDown = false;
            }
        }

        public void ShowAnimation(EventArgs args)
        {
            if (UltimateUp())
            {
                j++;

                Vector2 pos = Utils.GetCursorPos();
                pos.X -= mouseImage1.Width * 1.2f;

                mouseImage1.Position = pos;

                mouseImage2.Position = pos;

                Vector2 pos2 = Utils.GetCursorPos();
                pos2.X -= denyMouseImage.Width;

                if (denyMouseImage.Visible)
                {
                    k++;
                }

                denyMouseImage.Position = pos2;

                Text.X = (int)(pos2.X - 32);
                Text.Y = (int)(pos2.Y + 50);
                Text.OnEndScene();

                if ((j == 30) && (mouseImage1.Visible))
                {
                    j = 0;
                    mouseImage1.Visible = false;
                    mouseImage2.Visible = true;
                }
                else if ((j == 30) && (!mouseImage1.Visible))
                {
                    j = 0;
                    mouseImage1.Visible = true;
                    mouseImage2.Visible = false;
                }
                if (k == 70)
                {
                    Text.Visible = false;
                    denyMouseImage.Visible = false;
                    k = 0;
                }
            }
            else
            {
                mouseImage1.PositionUpdate = null;
                mouseImage2.PositionUpdate = null;
                denyMouseImage.Visible = false;
                denyMouseImage.PositionUpdate = null;
                mouseImage1.Visible = false;
                mouseImage2.Visible = false;
                Text.Visible = false;
                k = 0;
            }
        }
    }
}