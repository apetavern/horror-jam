namespace GvarJam.Player;

/// <summary>
/// The players pawn.
/// </summary>
public sealed partial class Pawn : AnimatedEntity
{
	/// <summary>
	/// The active animator of the pawn.
	/// </summary>
	[Net]
	public PawnAnimator Animator { get; private set; } = null!;

	/// <summary>
	/// The active camera for the pawn.
	/// </summary>
	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		set
		{
			Components.RemoveAny<CameraMode>();
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
	/// This is a list of stuff we apply the alpha change to
	/// ( This gets stored so that we're not constantly allocating )
	/// </summary>
	private List<ModelEntity> PlayerAndChildren { get; set; } = null!;

	/// <summary>
	/// This will block movement vector, crouching, and jumping in
	/// <see cref="BuildInput(InputBuilder)" />.
	/// </summary>
	[Net, Local]
	public bool BlockMovement { get; set; } = false;

	/// <summary>
	/// This will block the player from being able to look around.
	/// </summary>
	[Net, Local]
	public bool BlockLook { get; set; } = false;

	/// <summary>
	/// The time since the last footstep the pawn made.
	/// </summary>
	private TimeSince timeSinceLastFootstep = 0;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "player" );

		SetModel( "models/player/playermodel.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Animator = new StandardPlayerAnimator();
		Camera = new PawnCamera();
		Controller = new MovementController()
		{
			SprintSpeed = 170.0f,
			WalkSpeed = 100.0f,
			DefaultSpeed = 100.0f
		};

		ClothingContainer clothes = new ClothingContainer();

		clothes.Clothing.Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shirt/jumpsuit/blue_jumpsuit.clothing" ) );

		clothes.DressEntity( this );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		foreach ( var itemType in Enum.GetValues<ItemType>() )
			Items.Add( itemType, 0 );
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
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Input.Pressed( InputButton.View ) && Camera is PawnCamera camera )
		{
			if ( camera.ViewMode == ViewModeType.FirstPerson )
				camera.GoToThirdPerson();
			else
				camera.GoToFirstPerson();
		}

		SimulateLamp();
		SimulateInteraction( cl );
		SimulateSanity();
		SimulateCutscenes();

		var rotation = Rotation;
		Controller?.Simulate( cl, this, Animator );
		if ( IsInteracting || BlockMovement )
			Rotation = rotation;

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
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		var rotation = Rotation;
		Controller?.FrameSimulate( cl, this, Animator );
		if ( IsInteracting || BlockMovement )
			Rotation = rotation;
	}

	/// <inheritdoc/>
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !IsClient )
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
	/// Equips the helmet on the player.
	/// </summary>
	public void EquipHelmet()
	{
		Host.AssertServer();
		Assert.True( Helmet is null );

		Helmet = new ModelEntity( "models/cosmetics/spacehelmet.vmdl" )
		{
			EnableHideInFirstPerson = true
		};

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
	[Event.Frame]
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
}
