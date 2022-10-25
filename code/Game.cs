global using GvarJam.HammerEntities;
global using GvarJam.Interactions;
global using GvarJam.Inventory;
global using GvarJam.Objectives;
global using GvarJam.Player;
global using GvarJam.Utility;
global using Sandbox;
global using Sandbox.Component;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using SandboxEditor;
global using System;
global using System.Collections.Generic;
global using System.Linq;
using GvarJam.Interaction;
using GvarJam.UI;

namespace GvarJam;

/// <summary>
/// The game class.
/// </summary>
public sealed partial class MyGame : Sandbox.Game
{
	private ObjectiveSystem ObjectiveSystem { get; set; }

	public MyGame()
	{
		if ( IsServer )
		{
			_ = new Hud();
			ObjectiveSystem = new();
		}
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Pawn();
		client.Pawn = pawn;

		// Get all of the spawnpoints
		var spawnpoints = All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position += Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;
		}
	}

	/// <summary>
	/// Debug command to spawn a monster model.
	/// </summary>
	[ConCmd.Admin( "spawn_monstermodel" )]
	public static void SpawnMonsterModel()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		Pawn pawn = ConsoleSystem.Caller.Pawn as Pawn;

		AnimatedEntity split = new AnimatedEntity( "models/enemy/basic_splitizen.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),

		};

		AnimatedEntity mon = new AnimatedEntity( "models/enemy/monster.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),

		};

		AnimatedEntity mon2 = new AnimatedEntity( "models/enemy/monster.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f - pawn.Rotation.Left * 32f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),

		};

		mon2.SetAnimParameter( "idle", true );

		mon.SetParent( split, true );

		AnimatedEntity cit = new AnimatedEntity( "models/player/playermodel.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),

		};

		AnimatedEntity cit2 = new AnimatedEntity( "models/player/playermodel.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f + pawn.Rotation.Left * 32f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),

		};

		cit.SetMaterialOverride( "models/enemy/materials/citizen/splitizen_skin.vmat" );
		cit.SetBodyGroup( 0, 1 );
		cit.SetBodyGroup( 1, 1 );
		cit.SetBodyGroup( 3, 1 );

		cit2.SetMaterialOverride( Material.Load( "models/enemy/materials/citizen/splitizen_skin_nosplit.vmat" ), "skin" );
		cit2.SetMaterialOverride( Material.Load( "models/enemy/materials/citizen/splitizen_eyes.vmat" ), "eyes" );

		split.SetParent( cit, true );

		new ModelEntity( "models/citizen_clothes/jacket/longsleeve/models/jeans.vmdl" ).SetParent( cit, true );
		new ModelEntity( "models/citizen_clothes/jacket/longsleeve/models/jeans.vmdl" ).SetParent( cit2, true );

		cit2.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		cit2.UseAnimGraph = false;
		cit2.PhysicsBody.Velocity = Vector3.Random * 500f;
		//split.SetAnimParameter( "split", true );
		//mon.SetAnimParameter( "split", true );
	}

	/// <summary>
	/// Debug command to spawn a monster model.
	/// </summary>
	[ConCmd.Admin( "spawn_monstermodelsplitting" )]
	public static void SpawnMonsterModelSplitting()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		Pawn pawn = ConsoleSystem.Caller.Pawn as Pawn;

		AnimatedEntity split = new AnimatedEntity( "models/enemy/basic_splitizen.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),

		};

		AnimatedEntity mon = new AnimatedEntity( "models/enemy/monster.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),

		};

		mon.SetParent( split, true );

		AnimatedEntity cit = new AnimatedEntity( "models/player/playermodel.vmdl" )
		{
			Position = pawn.Position + pawn.Rotation.Forward * 150f,
			Rotation = pawn.Rotation * new Angles( 0, 180, 0 ).ToRotation(),

		};

		cit.SetMaterialOverride( "models/enemy/materials/citizen/splitizen_skin.vmat" );
		cit.SetBodyGroup( 0, 1 );
		cit.SetBodyGroup( 1, 1 );
		cit.SetBodyGroup( 3, 1 );

		split.SetParent( cit, true );

		new ModelEntity( "models/citizen_clothes/jacket/longsleeve/models/jeans.vmdl" ).SetParent( cit, true );

		split.SetAnimParameter( "split", true );
		mon.SetAnimParameter( "split", true );
	}

	/// <summary>
	/// Debug command to spawn a helmet.
	/// </summary>
	[ConCmd.Admin( "spawn_helmet" )]
	public static void SpawnHelmet()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		_ = new Helmet() { Position = ConsoleSystem.Caller.Pawn.Position };
	}

	/// <summary>
	/// Debug command to spawn a Janitor key.
	/// </summary>
	[ConCmd.Admin( "spawn_janitorkey" )]
	public static void SpawnJanitorKey()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		_ = new InventoryItem()
		{
			ItemType = ItemType.JanitorKey,
			Position = ConsoleSystem.Caller.Pawn.Position
		};
	}

	/// <summary>
	/// Debug command to spawn a level 1 keycard.
	/// </summary>
	[ConCmd.Admin( "spawn_keycardlvl1" )]
	public static void SpawnKeycardLevel1()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		_ = new InventoryItem()
		{
			ItemType = ItemType.KeycardLvl1,
			Position = ConsoleSystem.Caller.Pawn.Position
		};
	}

	/// <summary>
	/// Debug command to spawn a level 2 keycard.
	/// </summary>
	[ConCmd.Admin( "spawn_keycardlvl2" )]
	public static void SpawnKeycardLevel2()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		_ = new InventoryItem()
		{
			ItemType = ItemType.KeycardLvl2,
			Position = ConsoleSystem.Caller.Pawn.Position
		};
	}

	/// <summary>
	/// Debug command to spawn a level 3 keycard.
	/// </summary>
	[ConCmd.Admin( "spawn_keycardlvl3" )]
	public static void SpawnKeycardLevel3()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		_ = new InventoryItem()
		{
			ItemType = ItemType.KeycardLvl3,
			Position = ConsoleSystem.Caller.Pawn.Position
		};
	}

	/// <summary>
	/// Debug command to spawn a battery.
	/// </summary>
	[ConCmd.Admin( "spawn_battery" )]
	public static void SpawnBatteryItem()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		_ = new LampBattery() { Position = ConsoleSystem.Caller.Pawn.Position };
	}

	/// <summary>
	/// Debug command to spawn the camera controller.
	/// </summary>
	[ConCmd.Admin( "spawn_cameracontroller" )]
	public static void SpawnCameraController()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		_ = new CameraController() { Position = ConsoleSystem.Caller.Pawn.Position };
	}

	/// <summary>
	/// Debug command to spawn the storage locker.
	/// </summary>
	[ConCmd.Admin( "spawn_storagelocker" )]
	public static void SpawnStorageLocker()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		_ = new StorageLocker() { Position = ConsoleSystem.Caller.Pawn.Position };
	}
}
