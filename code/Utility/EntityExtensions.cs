using System.Threading.Tasks;

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

	/// <summary>
	/// Deletes an entity after an amount of seconds and once no player can see it.
	/// </summary>
	/// <param name="entity">The entity to delete.</param>
	/// <param name="seconds">The number of seconds to wait before attempting to delete the entity.</param>
	/// <returns>The asynchronous task that spawns from this call.</returns>
	public static async Task DeleteAfterSecondsAndNotVisible( this ModelEntity entity, float seconds )
	{
		await Task.Delay( TimeSpan.FromSeconds( seconds ) );

		while ( true )
		{
			var numVisible = 0;
			foreach ( var pawn in Entity.All.OfType<Pawn>() )
			{
				var tr = Trace.Ray( pawn.EyePosition, entity.Position )
					.WithoutTags( "trigger" )
					.Ignore( entity )
					.Run();

				var ent = tr.Entity;
				while ( ent is not null && ent.IsValid && ent != pawn )
					ent = ent.Parent;

				if ( ent != pawn )
					continue;

				numVisible++;
				break;
			}

			if ( numVisible == 0 )
				break;

			await Task.Delay( TimeSpan.FromSeconds( 1 ) );
		}

		entity.Delete();
	}
}
