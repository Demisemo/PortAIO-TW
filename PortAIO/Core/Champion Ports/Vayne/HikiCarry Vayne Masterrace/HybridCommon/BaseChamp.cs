using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using HybridCommon;
using SharpDX;
using SharpDX.Direct3D9;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace HybridCommon
{
    public abstract class BaseChamp
    {
        
        public const int Q = 0, W = 1, E = 2, R = 3;

        public Menu Config, combo, ult, harass, laneclear, jungle,CondemnMenu,condemnwhitelist, customizableinterrupter, misc, drawing, evade, activator, qss, botrk, youmuu, qssMenu;
        public Orbwalking.Orbwalker Orbwalker;
        public Spell[] Spells = new Spell[4];
        public static long LastCheck;
        public static List<Vector2> Points = new List<Vector2>();
        public Evader m_evader;
        public Font Text;

        public delegate void dVoidDelegate();
        public dVoidDelegate BeforeOrbWalking, BeforeDrawing;
        public dVoidDelegate[] OrbwalkingFunctions = new dVoidDelegate[4];
        public static Vector3 BlueTurretBot1 = new Vector3(10504, 1029, 41); // 100 Range
        public static Vector3 BlueTurretBot2 = new Vector3(6919, 1483, 43); // 100 Range
        public static Vector3 BlueTurretBot3 = new Vector3(4281, 1253, 86); // 100 Range
        public static Vector3 BlueTurretBotInhibitor = new Vector3(3459, 1236, 89); // 200 Range
        public static Vector3 BlueTurretRock1 = new Vector3(4000, 2370, 120); // 180 Range

        public BaseChamp(string szChampName)
        {
            Text = new Font(Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Malgun Gothic",
                    Height = 15,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural
                });

            Config = new Menu(String.Format("HikiCarry Vayne Masterrace", szChampName), szChampName, true);
            
            TargetSelector.AddToMenu(Config.SubMenu("Target Selector Settings"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking Settings"));
            
            SpellDatabase.InitalizeSpellDatabase();
        }
        
        public virtual void CreateConfigMenu()
        {
            //
        }

        public virtual void SetSpells()
        {
            //
        }

        public virtual void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling() || args == null)
                return;

            if (BeforeOrbWalking != null) BeforeOrbWalking();

            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None && OrbwalkingFunctions[(int)Orbwalker.ActiveMode] != null)
                OrbwalkingFunctions[(int)Orbwalker.ActiveMode]();
        }

        public virtual void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            //
        }


        public virtual void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            //
        }

        public virtual void Interrupter_OnPossibleToInterrupt(AIHeroClient sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            //
        }

        public virtual void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            //
        }

        public virtual void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //
        }

        public void CastSkillshot(AIHeroClient t, Spell s, HitChance hc = HitChance.High)
        {
            
        }

        public bool ComboReady()
        {
            return Spells[Q].IsReady() && Spells[W].IsReady() && Spells[E].IsReady() && Spells[R].IsReady();
        }

        #region Damage Calculation Funcitons
        public double CalculateComboDamage(AIHeroClient target, int aacount = 2)
        {
            return CalculateSpellDamage(target) + CalculateSummonersDamage(target) + CalculateItemsDamage(target) + CalculateAADamage(target, aacount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageQ(AIHeroClient target)
        {
            if (Spells[Q].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);

            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageW(AIHeroClient target)
        {
            if (Spells[W].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);

            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageE(AIHeroClient target)
        {
            if (Spells[E].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);

            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageR(AIHeroClient target)
        {
            if (Spells[R].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);

            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CalculateSpellDamage(AIHeroClient target)
        {
            return CalculateDamageQ(target) + CalculateDamageW(target) + CalculateDamageE(target) + CalculateDamageR(target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculateSummonersDamage(AIHeroClient target)
        {
            var ignite = ObjectManager.Player.GetSpellSlot("summonerdot");
            if (ignite != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(ignite) == SpellState.Ready && ObjectManager.Player.Distance(target, false) < 550)
                return ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite); //ignite

            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateItemsDamage(AIHeroClient target)
        {
            double dmg = 0.0;

            if (Items.CanUseItem(3144) && ObjectManager.Player.Distance(target, false) < 550)
                dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //bilgewater cutlass

            if (Items.CanUseItem(3153) && ObjectManager.Player.Distance(target, false) < 550)
                dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Botrk); //botrk

            if(Items.HasItem(3057))
                dmg += ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, ObjectManager.Player.BaseAttackDamage); //sheen

            if (Items.HasItem(3100))
                dmg += ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, (0.75 * ObjectManager.Player.BaseAttackDamage) + (0.50 * ObjectManager.Player.FlatMagicDamageMod)); //lich bane
            
            if(Items.HasItem(3285))
                dmg += ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, 100 + (0.1 * ObjectManager.Player.FlatMagicDamageMod)); //luden

            return dmg;
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateAADamage(AIHeroClient target, int aacount = 2)
        {
            double dmg = ObjectManager.Player.GetAutoAttackDamage(target) * aacount;

            if (Items.HasItem(3115))
                dmg += ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, 15 + (0.15 * ObjectManager.Player.FlatMagicDamageMod)); //nashor

            return dmg;
        }
        #endregion
    }
}
