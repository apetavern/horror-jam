global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using System;
global using System.Linq;
using GvarJam.Interactions;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace GvarJam;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class MyGame : Sandbox.Game
{
	public MyGame()
	{
		if ( IsServer )
		{
			_ = new Hud();
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

	[ConCmd.Admin( "spawn_instant_item" )]
	public static void SpawnInstantItem()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		_ = new InstantUseItem() { Position = ConsoleSystem.Caller.Pawn.Position };
	}

	[ConCmd.Admin( "spawn_delayed_item" )]
	public static void SpawnDelayedItem()
	{
		if ( ConsoleSystem.Caller?.Pawn is null )
			return;

		_ = new DelayedUseItem() { Position = ConsoleSystem.Caller.Pawn.Position };
	}
}
