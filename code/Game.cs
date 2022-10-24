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
}
