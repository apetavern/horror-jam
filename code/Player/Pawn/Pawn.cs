using Sandbox.Diagnostics;

namespace GvarJam.Player;

/// <summary>
/// The players pawn.
/// </summary>
public sealed partial class Pawn : CompatEntity
{
	/// <summary>
	/// The active animator of the pawn.
	/// </summary>
	[BindComponent]
	public PawnAnimator Animator { get; }

	/// <summary>
	/// The active camera for the pawn.
	/// </summary>
	public CameraComponent Camera
	{
		get => Components.Get<CameraComponent>();
		set
		{
			Components.RemoveAny<CameraComponent>();
			Components.Add( value );
		}
	}

	/// <summary>
	/// The active controller for the pawn.
	/// </summary>
	[Net]
	public MovementController? Controller { get; set; } = null;

	/// <summary>
	/// The pawns helmet.
	/// </summary>
	[Net]
	public ModelEntity? Helmet { get; private set; } = null;

	/// <summary>
	/// Whether or not movement should be blocked.
	/// </summary>
	[Net, Local]
	public bool BlockMovement { get; set; } = false;

	/// <summary>
	/// Whether or not look rotation should be blocked.
	/// </summary>
	[Net, Local]
	public bool BlockLook { get; set; } = false;

	/// <summary>
	/// This is a list of stuff we apply the alpha change to.
	/// <remarks>This gets stored so that we're not constantly allocating.</remarks>
	/// </summary>
	private List<ModelEntity> PlayerAndChildren { get; set; } = null!;

	/// <summary>
	/// The time since the last footstep the pawn made.
	/// </summary>
	private TimeSince timeSinceLastFootstep = 0;

	/// <summary>
	/// The pawns ragdoll.
	/// </summary>
	private ModelEntity? Ragdoll { get; set; }

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "player" );

		SetModel( "models/player/playermodel.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Components.Create<PawnAnimator>();
		Camera = new PawnCamera();
		Controller = new MovementController() { SprintSpeed = 170.0f, WalkSpeed = 100.0f, DefaultSpeed = 100.0f };

		var clothes = new ClothingContainer();
		clothes.Clothing.Add(
			ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shirt/jumpsuit/blue_jumpsuit.clothing" ) );
		clothes.DressEntity( this );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		foreach ( var itemType in Enum.GetValues<ItemType>() )
			Items.Add( itemType, 0 );

		LifeState = LifeState.Alive;
	}

	/// <inheritdoc/>
	public override void OnKilled()
	{
		BlockLook = true;
		BlockMovement = true;

		(Camera as PawnCamera)?.GoToThirdPerson();

		// Hide the player
		EnableDrawing = false;

		// Hide the players children
		foreach ( var child in Children.OfType<ModelEntity>() )
			child.EnableDrawing = false;

		// Create ragdoll
		Ragdoll = new ModelEntity( "models/player/playermodel.vmdl" ) { Position = Position + Rotation.Forward * 3f };
		Ragdoll.Tags.Add( "trigger" );
		Ragdoll.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		if ( Helmet is not null )
		{
			var helmet = new ModelEntity( "models/cosmetics/spacehelmet.vmdl" );
			helmet.SetParent( Ragdoll, true );
		}

		var clothes = new ModelEntity( "models/citizen_clothes/shirt/jumpsuit/models/blue_jumpsuit.vmdl" );
		clothes.SetParent( Ragdoll, true );

		Controller = null;

		LifeState = LifeState.Dead;
	}

	/// <inheritdoc/>
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		//
		// Set up hideable entities
		//
		PlayerAndChildren = new List<ModelEntity>();
		PlayerAndChildren.Add( this );
		PlayerAndChildren.AddRange( Children.OfType<ModelEntity>() );
	}

	/// <inheritdoc/>
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( LifeState == LifeState.Dead )
		{
			if ( Input.Released( InputButton.Jump ) )
				Respawn();

			return;
		}

		if ( Input.Pressed( InputButton.View ) && Camera is PawnCamera camera )
		{
			if ( camera.ViewMode == ViewModeType.FirstPerson )
				camera.GoToThirdPerson();
			else
				camera.GoToFirstPerson();
		}

		SimulateLamp();
		SimulateInteraction( cl );
		SimulateCutscenes();

		var rotation = Rotation;
		Controller?.Simulate();
		if ( IsInteracting || BlockMovement )
			Rotation = rotation;
		Animator?.Simulate();

		if ( HorrorGame.Debug )
		{
			var i = 0;
			foreach ( var value in Enum.GetValues<ItemType>() )
			{
				DebugOverlay.ScreenText( $"{value}: {Items[value]}", 15 + i );
				i++;
			}
		}
	}

	/// <inheritdoc/>
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		var rotation = Rotation;
		Controller?.FrameSimulate();
		if ( IsInteracting || BlockMovement )
			Rotation = rotation;
		Camera?.Update();
	}

	/// <inheritdoc/>
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !Game.IsClient )
			return;

		if ( timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		var sound = foot == 0 ? tr.Surface.Sounds.FootLeft : tr.Surface.Sounds.FootRight;

		if ( !string.IsNullOrWhiteSpace( sound ) )
		{
			SoundManager.PlayMonsterSound( sound, tr.EndPosition, volume );
		}
	}

	/// <summary>
	/// Respawns the pawn.
	/// </summary>
	public void Respawn()
	{
		BlockLook = false;
		BlockMovement = false;
		LifeState = LifeState.Alive;

		(Camera as PawnCamera)?.GoToFirstPerson();

		// Hide the player
		EnableDrawing = true;

		// Hide the players children
		foreach ( var child in Children.OfType<ModelEntity>() )
			child.EnableDrawing = true;

		// Clear the ragoll
		Ragdoll?.DeleteAsync( 10 );

		Controller = new MovementController() { SprintSpeed = 170.0f, WalkSpeed = 100.0f, DefaultSpeed = 100.0f };

		SoundManager.ShouldPlayChaseSounds( false );
		HorrorGame.Current?.MoveToSpawnpoint( this );
	}

	/// <summary>
	/// Equips the helmet on the player.
	/// </summary>
	public void EquipHelmet()
	{
		Game.AssertServer();
		Assert.True( Helmet is null );

		Helmet = new ModelEntity( "models/cosmetics/spacehelmet.vmdl" ) { EnableHideInFirstPerson = true };

		Lamp = new SpotLightEntity
		{
			Parent = Helmet,
			Transform = Helmet.GetAttachment( "light" )!.Value,
			LightCookie = Texture.Load( FileSystem.Mounted, "materials/effects/lightcookie.vtex" )
		};

		Lamp.Enabled = false;
		Lamp.OuterConeAngle *= 0.6f;
		Lamp.InnerConeAngle *= 0.2f;
		Lamp.Brightness = 1f;

		Helmet.SetParent( this, true );
		AddHelmetChild( To.Single( Client ), Helmet );
	}

	/// <summary>
	/// Allows override of footstep sound volume.
	/// </summary>
	/// <returns>The new footstep volume, where 1 is full volume.</returns>
	public float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f );
	}

	/// <summary>
	/// Controls the alpha/transparency of the player. It's mainly used for transitions between the two camera modes.
	/// </summary>
	/// <returns>The transparency to render the player at.</returns>
	private float GetAlpha()
	{
		if ( Camera is PawnCamera camera )
			return camera.GetPlayerAlpha();

		return 1.0f;
	}

	/// <summary>
	/// Sets the alpha on the pawns render color before the game renders.
	/// </summary>
	[Event.Client.Frame]
	private void OnFrame()
	{
		if ( HidingPlayers )
		{
			foreach ( var pawn in All.OfType<Pawn>() )
			{
				pawn.RenderColor = pawn.RenderColor.WithAlpha( 0 );
				foreach ( var child in pawn.Children.OfType<ModelEntity>() )
					child.RenderColor = child.RenderColor.WithAlpha( 0 );
			}

			return;
		}
		else
		{
			foreach ( var pawn in All.OfType<Pawn>() )
			{
				pawn.RenderColor = pawn.RenderColor.WithAlpha( 1 );
				foreach ( var child in pawn.Children.OfType<ModelEntity>() )
					child.RenderColor = child.RenderColor.WithAlpha( 1 );
			}
		}

		//
		// CACHING THE CHILDREN will definitely become a problem later!!
		// This is a fast but probably not elegant solution
		// If you need this changing or fixing, find Alex
		//
		var alpha = GetAlpha();
		PlayerAndChildren?.ForEach( x => x.RenderColor = x.RenderColor.WithAlpha( alpha ) );
	}

	/// <summary>
	/// Adds the helmet entity to the player and children to hide in first person.
	/// </summary>
	/// <param name="helmet">The helmet entity.</param>
	[ClientRpc]
	private void AddHelmetChild( ModelEntity helmet )
	{
		PlayerAndChildren.Add( helmet );
	}

	/// <summary>
	/// Utility command to kill the pawn if needed.
	/// </summary>
	[ConCmd.Server]
	public static void Kill()
	{
		if ( ConsoleSystem.Caller?.Pawn is not Pawn pawn )
			return;

		pawn.OnKilled();
	}
}
