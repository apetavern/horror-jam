namespace GvarJam;

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
	public Camera Camera
	{
		get => Components.Get<Camera>();
		private set
		{
			Components.RemoveAny<CameraMode>();
			Components.Add( value );
		}
	}

	/// <summary>
	/// The active controller for the pawn.
	/// </summary>
	[Net]
	public BasePlayerController Controller { get; private set; } = null!;

	/// <summary>
	/// The pawns helmet.
	/// </summary>
	[Net]
	private ModelEntity Helmet { get; set; }
	/// <summary>
	/// The pawns lamp.
	/// </summary>
	[Net]
	private SpotLightEntity Lamp { get; set; }

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Helmet = new ModelEntity( "models/cosmetics/spacehelmet.vmdl" )
		{
			EnableHideInFirstPerson = true
		};

		Lamp = new SpotLightEntity
		{
			Parent = Helmet,
			Transform = Helmet.GetAttachment( "light" ).Value
		};
		Lamp.OuterConeAngle *= 0.4f;
		Lamp.InnerConeAngle *= 0.1f;
		Lamp.Brightness = 0.5f;

		Helmet.SetParent( this, true );

		Animator = new StandardPlayerAnimator();
		Camera = new Camera();
		Controller = new MovementController();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	/// <inheritdoc/>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( Camera.ViewMode == ViewModeType.FirstPerson )
				Camera.GoToThirdPerson();
			else
				Camera.GoToFirstPerson();
		}

		Controller?.Simulate( cl, this, Animator );
	}

	/// <inheritdoc/>
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl, this, Animator );
	}

	/// <summary>
	/// Sets the alpha on the pawns render color before the game renders.
	/// </summary>
	[Event.PreRender]
	public void OnPreRender()
	{
		RenderColor = new Color( 1, 1, 1, GetAlpha() );
	}

	/// <summary>
	/// Controls the alpha/transparency of the player. It's mainly used for transitions between the two camera modes.
	/// </summary>
	/// <returns>The transparency to render the player at.</returns>
	private float GetAlpha()
	{
		if ( Camera is Camera camera )
			return camera.GetPlayerAlpha();

		return 1.0f;
	}
}
