global using GvarJam.HammerEntities;
global using GvarJam.Interactions;
global using GvarJam.Inventory;
global using GvarJam.Managers;
global using GvarJam.Monster;
global using GvarJam.Objectives;
global using GvarJam.Player;
global using GvarJam.UI;
global using GvarJam.Utility;
global using GvarJam.Components;
global using Sandbox;
global using Sandbox.Component;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using SandboxEditor;
global using System;
global using System.Collections.Generic;
global using System.Linq;

namespace GvarJam;

/// <summary>
/// The game class.
/// </summary>
public sealed partial class HorrorGame : Game
{
	/// <inheritdoc/>
	public static new HorrorGame Current => (Game.Current as HorrorGame)!;

	/// <summary>
	/// Whether or not the lights in the ship are enabled.
	/// </summary>
	[Net]
	public bool LightsEnabled { get; set; } = true;
	/// <summary>
	/// The instance of the objective system.
	/// </summary>
	private ObjectiveSystem ObjectiveSystem { get; set; } = null!;

	/// <summary>
	/// Whether or not to show debug information in the game.
	/// </summary>
	[ConVar.Replicated( "debug_gvarjam" )]
	public static bool Debug { get; set; } = false;

	/// <summary>
	/// Initializes a default instance of <see cref="HorrorGame"/>.
	/// </summary>
	public HorrorGame()
	{
		if ( !IsServer )
			return;

		_ = new Hud();
		ObjectiveSystem = new();
	}

	/// <inheritdoc/>
	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		var corpses = new List<ModelEntity>();
		var clothes = new ClothingContainer();
		clothes.Clothing.Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shirt/jumpsuit/blue_jumpsuit.clothing" ) );

		foreach ( var prop in All.OfType<Prop>() )
		{
			if ( prop.GetModelName().Contains( "nosplit_blank" ) )
				corpses.Add( prop );
		}

		foreach ( var corpse in corpses )
			clothes.DressEntity( corpse );
	}

	/// <inheritdoc/>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var pawn = new Pawn();
		client.Pawn = pawn;

		var spawnpoints = All.OfType<SpawnPoint>();
		foreach ( PointLightEntity light in All.OfType<PointLightEntity>() )
			light.Components.GetOrCreate<LightCullComponent>();

		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
		if ( randomSpawnPoint is not null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position += Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;
		}
	}

	/// <inheritdoc/>
	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		camSetup.ZNear = 6f;
	}
}
