using System.Collections.Generic;

namespace GvarJam.Utility;

/// <summary>
/// Extension class for <see cref="Entity"/>.
/// </summary>
public static class EntityExtensions
{
	/// <summary>
	/// Gets the closest entity in an enumerator.
	/// </summary>
	/// <param name="entities">The set of entities to search.</param>
	/// <param name="target">The target to get the distance to.</param>
	/// <typeparam name="T">The type contained in the enumerator.</typeparam>
	/// <returns>The closest entity in the set. Null if no elements in the set.</returns>
	public static T? GetClosestOrDefault<T>( this IEnumerable<T> entities, Entity target ) where T : Entity
	{
		T? closest = default;
		var closestDistance = float.MaxValue;

		foreach ( var entity in entities )
		{
			var distance = target.Position.DistanceSquared( entity.Position );
			if ( distance > closestDistance )
				continue;

			closest = entity;
			closestDistance = distance;
		}

		return closest;
	}
}
