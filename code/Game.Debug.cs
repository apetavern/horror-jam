namespace GvarJam;

partial class HorrorGame
{
	/// <summary>
	/// Debug command to force an objective to complete.
	/// </summary>
	/// <param name="objectiveName">The name of the objective to complete.</param>
	[ConCmd.Admin( "objective_force" )]
	public static void ForceObjective( string objectiveName )
	{
		if ( !Debug )
			return;

		var objective = ResourceLibrary.Get<ObjectiveResource>( objectiveName );
		objective.ObjectiveEndEvents.ForEach( x => x.Invoke( Entity.All.OfType<Pawn>().First() ) );

		Log.Trace( $"Forced objective {objective.ObjectiveName}" );
	}

	/// <summary>
	/// Debug command to start testing the cutscene camera.
	/// </summary>
	[ConCmd.Client( "test_cutscene_camera" )]
	public static void TestCutsceneCamera()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is null || ConsoleSystem.Caller.Pawn is not Pawn pawn )
			return;

		var nearestCamera = All.OfType<MountedCamera>().GetClosestOrDefault( pawn );
		if ( nearestCamera is not null )
			pawn.StartCutscene( nearestCamera, "lens_position" );
	}

	/// <summary>
	/// Debug command to stop testing the cutscene camera.
	/// </summary>
	[ConCmd.Client( "test_cutscene_end" )]
	public static void TestCutsceneEnd()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is null || ConsoleSystem.Caller.Pawn is not Pawn pawn )
			return;

		pawn.EndCutscene();
	}

	/// <summary>
	/// Debug command to play the "welcome_f" sound.
	/// </summary>
	[ConCmd.Admin( "welcomef" )]
	public static void PlayWelcomeF()
	{
		if ( !Debug )
			return;

		Sound.FromScreen( "welcome_f" );
	}

	/// <summary>
	/// Debug command to play the "welcome_m" sound.
	/// </summary>
	[ConCmd.Admin( "welcomem" )]
	public static void PlayWelcomeM()
	{
		if ( !Debug )
			return;

		Sound.FromScreen( "welcome_m" );
	}

	/// <summary>
	/// Debug command to spawn a monster model.
	/// </summary>
	[ConCmd.Admin( "spawn_monstermodel" )]
	public static void SpawnMonsterModel()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		AnimatedEntity split = new( "models/enemy/basic_splitizen.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),
		};

		AnimatedEntity mon = new( "models/enemy/monster.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),
		};

		AnimatedEntity mon2 = new( "models/enemy/monster.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f - pawn.Rotation.Left * 32f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),
		};

		mon.SetParent( split, true );
		mon2.SetAnimParameter( "idle", true );

		AnimatedEntity cit = new( "models/player/playermodel.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),
		};
		cit.SetMaterialOverride( "models/enemy/materials/citizen/splitizen_skin.vmat" );
		cit.SetBodyGroup( 0, 1 );
		cit.SetBodyGroup( 1, 1 );
		cit.SetBodyGroup( 3, 1 );

		AnimatedEntity cit2 = new( "models/player/playermodel.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f + pawn.Rotation.Left * 32f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),
		};
		cit2.SetMaterialOverride( Material.Load( "models/enemy/materials/citizen/splitizen_skin_nosplit.vmat" ), "skin" );
		cit2.SetMaterialOverride( Material.Load( "models/enemy/materials/citizen/splitizen_eyes.vmat" ), "eyes" );
		cit2.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		cit2.UseAnimGraph = false;
		cit2.PhysicsBody.Velocity = Vector3.Random * 500f;

		split.SetParent( cit, true );

		new ModelEntity( "models/citizen_clothes/jacket/longsleeve/models/jeans.vmdl" ).SetParent( cit, true );
		new ModelEntity( "models/citizen_clothes/jacket/longsleeve/models/jeans.vmdl" ).SetParent( cit2, true );
	}

	/// <summary>
	/// Debug command to split an existing splitizen
	/// </summary>
	[ConCmd.Admin( "splitmonster" )]
	public static void SpawnMonsterModelSplitting()
	{
		if ( !Debug )
			return;

		All.OfType<Splitizen>().First().DoSplit();
	}

	/// <summary>
	/// Debug command to spawn a helmet.
	/// </summary>
	[ConCmd.Admin( "spawn_helmet" )]
	public static void SpawnHelmet()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		_ = new Helmet() { Position = pawn.Position };
	}

	/// <summary>
	/// Debug command to spawn a Janitor key.
	/// </summary>
	[ConCmd.Admin( "spawn_janitorkey" )]
	public static void SpawnJanitorKey()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		_ = new InventoryItem()
		{
			ItemType = ItemType.JanitorKey,
			Position = pawn.Position
		};
	}

	/// <summary>
	/// Debug command to spawn a level 1 keycard.
	/// </summary>
	[ConCmd.Admin( "spawn_keycardlvl1" )]
	public static void SpawnKeycardLevel1()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		_ = new InventoryItem()
		{
			ItemType = ItemType.KeycardLvl1,
			Position = pawn.Position
		};
	}

	/// <summary>
	/// Debug command to spawn a level 2 keycard.
	/// </summary>
	[ConCmd.Admin( "spawn_keycardlvl2" )]
	public static void SpawnKeycardLevel2()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		_ = new InventoryItem()
		{
			ItemType = ItemType.KeycardLvl2,
			Position = pawn.Position
		};
	}

	/// <summary>
	/// Debug command to spawn a level 3 keycard.
	/// </summary>
	[ConCmd.Admin( "spawn_keycardlvl3" )]
	public static void SpawnKeycardLevel3()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		_ = new InventoryItem()
		{
			ItemType = ItemType.KeycardLvl3,
			Position = pawn.Position
		};
	}

	/// <summary>
	/// Debug command to spawn a battery.
	/// </summary>
	[ConCmd.Admin( "spawn_battery" )]
	public static void SpawnBatteryItem()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		_ = new LampBattery() { Position = pawn.Position };
	}

	/// <summary>
	/// Debug command to spawn the camera controller.
	/// </summary>
	[ConCmd.Admin( "spawn_cameracontroller" )]
	public static void SpawnCameraController()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		_ = new CameraController() { Position = pawn.Position };
	}

	/// <summary>
	/// Debug command to spawn the storage locker.
	/// </summary>
	[ConCmd.Admin( "spawn_storagelocker" )]
	public static void SpawnStorageLocker()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		_ = new StorageLocker() { Position = pawn.Position };
	}

	/// <summary>
	/// Debug command to spawn the monster.
	/// </summary>
	[ConCmd.Admin( "spawn_monster" )]
	public static void SpawnMonster()
	{
		if ( !Debug )
			return;

		var monster = new MonsterEntity();
		Current.MoveToSpawnpoint( monster );
	}

	/// <summary>
	/// Debug command to spawn a note.
	/// </summary>
	[ConCmd.Admin( "spawn_note" )]
	public static void SpawnNote()
	{
		if ( !Debug || ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		_ = new Note() { Position = pawn.Position };
	}
}
