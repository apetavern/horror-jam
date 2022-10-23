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
	public BasePlayerController? Controller { get; set; } = null;

	/// <summary>
	/// The pawns helmet.
	/// </summary>
	[Net]
	private ModelEntity Helmet { get; set; } = null!;

	/// <summary>
	/// This is a list of stuff we apply the alpha change to
	/// ( This gets stored so that we're not constantly allocating )
	/// </summary>
	private List<ModelEntity> PlayerAndChildren { get; set; } = null!;

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "player" );

		SetModel( "models/player/playermodel.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

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

		Lamp.OuterConeAngle *= 0.4f;
		Lamp.InnerConeAngle *= 0.1f;
		Lamp.Brightness = 0.5f;

		Helmet.SetParent( this, true );

		Animator = new StandardPlayerAnimator();
		Camera = new PawnCamera();
		Controller = new MovementController();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
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
		SimulateInteraction();

		var rotation = Rotation;
		Controller?.Simulate( cl, this, Animator );
		if ( IsInteracting )
			Rotation = rotation;
	}

	/// <inheritdoc/>
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		var rotation = Rotation;
		Controller?.FrameSimulate( cl, this, Animator );
		if ( IsInteracting )
			Rotation = rotation;
	}

	/// <summary>
	/// Sets the alpha on the pawns render color before the game renders.
	/// </summary>
	[Event.Frame]
	public void OnFrame()
	{
		//
		// CACHING THE CHILDREN will definitely become a problem later!!
		// This is a fast but probably not elegant solution
		// If you need this changing or fixing, find Alex
		//
		var alpha = GetAlpha();
		PlayerAndChildren?.ForEach( x => x.RenderColor = x.RenderColor.WithAlpha( alpha ) );
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

	TimeSince timeSinceLastFootstep = 0;

	/// <summary>
	/// A footstep has arrived!
	/// </summary>
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

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	/// <summary>
	/// Allows override of footstep sound volume.
	/// </summary>
	/// <returns>The new footstep volume, where 1 is full volume.</returns>
	public float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f );
	}
}
