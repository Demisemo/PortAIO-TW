#region License

/*
 Copyright 2014 - 2015 Nikita Bernthaler
 Extensions.cs is part of SFXTargetSelector.

 SFXTargetSelector is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 SFXTargetSelector is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with SFXTargetSelector. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion License

#region

using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

using EloBuddy; namespace SFXChallenger.SFXTargetSelector
{
    public static class Extensions
    {
        public static AIHeroClient GetTargetNoCollision(this Spell spell,
            bool ignoreShields = true,
            Vector3 from = default(Vector3),
            IEnumerable<AIHeroClient> ignoreChampions = null)
        {
            return TargetSelector.GetTargetNoCollision(spell, ignoreShields, from, ignoreChampions);
        }

        public static AIHeroClient GetTarget(this Spell spell,
            bool ignoreShields = true,
            Vector3 from = default(Vector3),
            IEnumerable<AIHeroClient> ignoreChampions = null)
        {
            return TargetSelector.GetTarget(spell, ignoreShields, from, ignoreChampions);
        }
    }
}