using EloBuddy; 
using LeagueSharp.SDK; 
namespace ExorAIO.Champions.Jinx
{
    using LeagueSharp;
    using LeagueSharp.SDK;

    /// <summary>
    ///     The methods class.
    /// </summary>
    internal class Methods
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The methods.
        /// </summary>
        public static void Initialize()
        {
            Game.OnUpdate += Jinx.OnUpdate;
            Events.OnGapCloser += Jinx.OnGapCloser;
            Variables.Orbwalker.OnAction += Jinx.OnAction;
        }

        #endregion
    }
}