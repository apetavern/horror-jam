namespace GvarJam.Objectives;

public sealed class ObjectiveSystem : GameObjectSystem<ObjectiveSystem>
{
	public List<Objective> PendingObjectives { get; } = new();
	public List<Objective> ActiveObjectives { get; } = new();

	public event Action<ObjectiveResource> OnObjectiveAdded;
	public event Action<ObjectiveResource> OnObjectiveRemoved;

	private bool _initialized;

	public ObjectiveSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.StartUpdate, 0, OnTick, "ObjectiveSystemTick" );
	}

	public void EnsureInitialized()
	{
		if ( _initialized ) return;
		_initialized = true;

		PendingObjectives.Clear();
		ActiveObjectives.Clear();

		var seen = new HashSet<ObjectiveResource>();
		foreach ( var map in Scene.GetAllComponents<ColdOpenMapInstance>() )
		{
			foreach ( var res in map.Objectives )
			{
				if ( res is not null && seen.Add( res ) )
				{
					PendingObjectives.Add( new Objective { Resource = res } );
					Log.Trace( $"Loaded objective from map manifest: {res.ResourcePath}" );
				}
			}
		}

		foreach ( var file in FileSystem.Mounted.FindFile( "data/objectives", "*.objctv", true ) )
		{
			var path = $"data/objectives/{file}";
			var res = ResourceLibrary.Get<ObjectiveResource>( path );
			if ( res is not null && seen.Add( res ) )
			{
				PendingObjectives.Add( new Objective { Resource = res } );
				Log.Trace( $"Loaded objective: {res.ResourcePath}" );
			}
		}

		foreach ( var res in ResourceLibrary.GetAll<ObjectiveResource>() )
		{
			if ( seen.Add( res ) )
			{
				PendingObjectives.Add( new Objective { Resource = res } );
				Log.Trace( $"Loaded objective: {res.ResourcePath}" );
			}
		}
	}

	public void CheckStartObjectives( ColdOpenPlayer player )
	{
		foreach ( var objective in PendingObjectives.ToList() )
		{
			if ( !objective.CheckStart( player ) ) continue;

			Log.Info( $"Start objective: {objective.Resource.ObjectiveName}" );
			objective.InvokeStartEvents( player );
			Log.Info( $"[Obj] Invoked start events for {objective.Resource.ObjectiveName}" );
			ActiveObjectives.Add( objective );
			PendingObjectives.Remove( objective );
			OnObjectiveAdded?.Invoke( objective.Resource );
			Log.Info( $"[Obj] Added UI for {objective.Resource.ObjectiveName}" );
		}
	}

	public void CheckEndObjectives( ColdOpenPlayer player )
	{
		foreach ( var objective in ActiveObjectives.ToList() )
		{
			if ( !objective.CheckEnd( player ) ) continue;

			Log.Info( $"Complete objective: {objective.Resource.ObjectiveName}" );
			objective.InvokeEndEvents( player );
			ActiveObjectives.Remove( objective );
			OnObjectiveRemoved?.Invoke( objective.Resource );
		}
	}

	private void OnTick()
	{
		if ( Scene.IsEditor ) return;

		EnsureInitialized();

		if ( ColdOpenGame.Debug )
		{
			var line = 5;
			Scene.DebugOverlay.ScreenText( new Vector2( 20, 20 * line++ ), "Objectives:" );
			foreach ( var obj in ActiveObjectives )
			{
				Scene.DebugOverlay.ScreenText( new Vector2( 20, 20 * line++ ), $"Objective: {obj.Resource.ObjectiveName}" );
				Scene.DebugOverlay.ScreenText( new Vector2( 20, 20 * line++ ), obj.Resource.Description );
			}
			Scene.DebugOverlay.ScreenText( new Vector2( 20, 20 * line++ ), $"Pending: {PendingObjectives.Count}, Active: {ActiveObjectives.Count}" );
		}

		foreach ( var player in Scene.GetAllComponents<ColdOpenPlayer>() )
		{
			CheckStartObjectives( player );
			CheckEndObjectives( player );
		}
	}
}
