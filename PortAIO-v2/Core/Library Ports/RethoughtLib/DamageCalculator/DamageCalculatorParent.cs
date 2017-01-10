//     Copyright (C) 2016 Rethought
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
//     Created: 04.10.2016 1:05 PM
//     Last Edited: 04.10.2016 1:43 PM

using EloBuddy; 
using LeagueSharp.Common; 
namespace RethoughtLib.DamageCalculator
{
    #region Using Directives

    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using RethoughtLib.FeatureSystem.Abstract_Classes;

    #endregion

    public class DamageCalculatorParent : ParentBase, IDamageCalculator
    {
        #region Fields

        private readonly List<IDamageCalculatorModule> damageCalculatorsModules = new List<IDamageCalculatorModule>();

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public override string Name { get; set; } = "Damage Calculator";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Adds the specified damage calculator module.
        /// </summary>
        /// <param name="damageCalculatorModule">The damage calculator module.</param>
        public void Add(IDamageCalculatorModule damageCalculatorModule)
        {
            this.damageCalculatorsModules.Add(damageCalculatorModule);
        }

        /// <summary>
        ///     Gets the damage.
        /// </summary>
        /// <param name="target">The get damage.</param>
        /// <returns></returns>
        public float GetDamage(Obj_AI_Base target)
        {
            var result =
                this.damageCalculatorsModules.Sum(
                    calculatorModule => calculatorModule.GetDamage(target) * calculatorModule.EstimatedAmountInOneCombo);

            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Called when [load].
        /// </summary>
        /// <param name="sender">the sender of the input</param>
        /// <param name="eventArgs">the contextual information</param>
        protected override void OnLoad(object sender, FeatureBaseEventArgs eventArgs)
        {
            base.OnLoad(sender, eventArgs);

            this.Menu.AddItem(new MenuItem("equalsonecombo", "One combo equals: "));

            foreach (var damageCalculator in this.damageCalculatorsModules)
            {
                var slider =
                    this.Menu.AddItem(
                        new MenuItem(this.Path + "." + damageCalculator.Name, damageCalculator.Name).SetValue(
                            new Slider(damageCalculator.EstimatedAmountInOneCombo, 0, 5)));

                slider.ValueChanged +=
                    (o, args) => { damageCalculator.EstimatedAmountInOneCombo = args.GetNewValue<Slider>().Value; };

                damageCalculator.EstimatedAmountInOneCombo = slider.GetValue<Slider>().Value;
            }
        }

        #endregion
    }
}