namespace GvarJam;

public sealed class ColdOpenGame : GameObjectSystem<ColdOpenGame>
{
	[ConVar( "debug_gvarjam", ConVarFlags.Replicated )]
	public static bool Debug { get; set; } = false;

	private bool _spawnedInitialPlayer;
	private sealed class BatterySpawnEntry
	{
		public Transform Transform { get; init; }
		public float Power { get; init; }
		public LampBatteryComponent Current { get; set; }
	}

	private readonly List<BatterySpawnEntry> _batterySpawns = new();

	public ColdOpenGame( Scene scene ) : base( scene )
	{
		Listen( Stage.StartUpdate, 0, EnsurePlayerSpawned, "ColdOpenGameTick" );
	}

	public void OnMapReady()
	{
		EnsurePlayerSpawned();
	}

	private void EnsurePlayerSpawned()
	{
		if ( Scene.IsEditor || _spawnedInitialPlayer )
			return;

		var map = Scene.GetAllComponents<MapInstance>().FirstOrDefault();
		if ( map is not null && !map.IsLoaded )
			return;

		if ( Scene.GetAllComponents<ColdOpenPlayer>().Any() )
		{
			_spawnedInitialPlayer = true;
			return;
		}

		_spawnedInitialPlayer = true;

		// Ensure HUD GameObject exists
		if ( !Scene.GetAllComponents<HudPanelComponent>().Any() )
		{
			var hudGo = new GameObject( true, "HUD" );
			hudGo.AddComponent<ScreenPanel>();
			hudGo.AddComponent<HudPanelComponent>();
		}

		// Disable minimal.scene's default daytime Sun & 2D Skybox if map provides its own lighting/skybox
		if ( map is not null )
		{
			var sun = Scene.Directory.FindByName( "Sun" ).FirstOrDefault();
			if ( sun is not null && Scene.GetAllComponents<SkyBox2D>().Count() > 1 )
				sun.Enabled = false;

			var defaultSky = Scene.Directory.FindByName( "2D Skybox" ).FirstOrDefault();
			if ( defaultSky is not null && Scene.GetAllComponents<SkyBox2D>().Count() > 1 )
				defaultSky.Enabled = false;

			var defaultCam = Scene.Directory.FindByName( "Camera" ).FirstOrDefault();
			if ( defaultCam is not null )
				defaultCam.Enabled = false;
		}

		SpawnPlayer();
	}

	public ColdOpenPlayer SpawnPlayer()
	{
		var go = new GameObject( true, "Player" );
		go.Tags.Add( "player" );
		var player = go.AddComponent<ColdOpenPlayer>();
		MoveToSpawnpoint( go );
		return player;
	}

	public void MoveToSpawnpoint( GameObject go )
	{
		var spawnpoints = Scene.GetAllComponents<SpawnPoint>().ToList();
		var sp = spawnpoints.OrderBy( _ => Guid.NewGuid() ).FirstOrDefault();
		if ( sp is not null )
		{
			go.WorldPosition = sp.WorldPosition + Vector3.Up * 50.0f;
			go.WorldRotation = sp.WorldRotation;
		}
	}

	public void RegisterBatterySpawn( LampBatteryComponent bat )
	{
		if ( bat is null || !bat.IsValid() ) return;
		if ( _batterySpawns.Any( x => x.Current == bat || x.Transform.Position.Distance( bat.WorldPosition ) < 4f ) )
			return;

		_batterySpawns.Add( new BatterySpawnEntry
		{
			Transform = bat.WorldTransform,
			Power = bat.BatteryPower,
			Current = bat
		} );
	}

	public void RespawnWorldBatteries()
	{
		foreach ( var dropped in Scene.GetAllObjects( true ).Where( g => g.Name == "DroppedBattery" ).ToList() )
		{
			if ( dropped.IsValid() )
				dropped.Destroy();
		}

		foreach ( var entry in _batterySpawns )
		{
			if ( entry.Current.IsValid() && !entry.Current.GameObject.IsDestroyed )
				continue;

			var go = new GameObject( true, "RespawnedBattery" );
			go.WorldTransform = entry.Transform;
			var bat = go.AddComponent<LampBatteryComponent>();
			bat.BatteryPower = entry.Power;
			entry.Current = bat;
		}
	}

	[ConCmd( "objective_force" )]
	public static void ForceObjective( string objectiveName )
	{
		if ( !Debug ) return;
		var scene = Game.ActiveScene;
		var player = scene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		var obj = ResourceLibrary.Get<ObjectiveResource>( objectiveName );
		if ( player is not null && obj is not null )
		{
			obj.ObjectiveEndEvents?.ForEach( x => x.Invoke( player ) );
			Log.Info( $"Forced objective {obj.ObjectiveName}" );
		}
	}

	[ConCmd( "splitmonster" )]
	public static void SplitMonsterCmd()
	{
		if ( !Debug ) return;
		Game.ActiveScene?.GetAllComponents<SplitizenComponent>().FirstOrDefault()?.DoSplit();
	}

	[ConCmd( "kill_player" )]
	public static void KillPlayerCmd()
	{
		Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault()?.OnKilled();
	}
}
