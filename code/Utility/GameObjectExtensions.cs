namespace GvarJam.Utility;

public static class GameObjectExtensions
{
	public static T GetClosestOrDefault<T>( this IEnumerable<T> components, GameObject target ) where T : Component
	{
		T closest = default;
		var closestDistance = float.MaxValue;

		if ( !target.IsValid() )
			return null;

		foreach ( var comp in components )
		{
			if ( !comp.IsValid() ) continue;
			var distance = target.WorldPosition.DistanceSquared( comp.WorldPosition );
			if ( distance > closestDistance )
				continue;

			closest = comp;
			closestDistance = distance;
		}

		return closest;
	}

	public static async Task DeleteAfterSecondsAndNotVisible( this GameObject go, float seconds )
	{
		await GameTask.DelaySeconds( seconds );
		if ( !go.IsValid() ) return;

		while ( go.IsValid() )
		{
			var numVisible = 0;
			foreach ( var player in go.Scene.GetAllComponents<ColdOpenPlayer>() )
			{
				var tr = go.Scene.Trace.Ray( player.EyePosition, go.WorldPosition )
					.WithoutTags( "trigger" )
					.IgnoreGameObjectHierarchy( go )
					.Run();

				if ( tr.GameObject is not null && tr.GameObject.Root == player.GameObject.Root )
				{
					numVisible++;
					break;
				}
			}

			if ( numVisible == 0 )
				break;

			await GameTask.DelaySeconds( 1f );
		}

		if ( go.IsValid() )
			go.Destroy();
	}
}
