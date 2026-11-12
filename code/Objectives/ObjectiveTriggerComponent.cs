namespace GvarJam.Objectives;

public sealed class ObjectiveTriggerComponent : Component, Component.ITriggerListener
{
	public HashSet<ColdOpenPlayer> TouchingPlayers { get; } = new();

	public void OnTriggerEnter( Collider other )
	{
		var player = other.GameObject.GetComponentInParent<ColdOpenPlayer>();
		if ( player is not null )
			TouchingPlayers.Add( player );
	}

	public void OnTriggerExit( Collider other )
	{
		var player = other.GameObject.GetComponentInParent<ColdOpenPlayer>();
		if ( player is not null )
			TouchingPlayers.Remove( player );
	}

	public bool IsTouching( ColdOpenPlayer player )
	{
		if ( TouchingPlayers.Contains( player ) )
			return true;

		var playerBox = new BBox(
			player.WorldPosition + new Vector3( -16f, -16f, 0f ),
			player.WorldPosition + new Vector3( 16f, 16f, 64f )
		);

		foreach ( var col in GetComponents<Collider>() )
		{
			if ( col.GetWorldBounds().Overlaps( playerBox ) )
				return true;

			foreach ( var t in col.Touching )
			{
				if ( t.GameObject.GetComponentInParent<ColdOpenPlayer>() == player )
					return true;
			}
		}

		return false;
	}
}
