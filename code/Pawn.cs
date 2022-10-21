namespace GvarJam;

partial class Pawn : AnimatedEntity
{
	[Net]
	public PawnAnimator Animator { get; private set; }

	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		private set
		{
			Components.RemoveAny<CameraMode>();
			Components.Add( value );
		}
	}

	[Net]
	public BasePlayerController Controller { get; private set; }

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Animator = new StandardPlayerAnimator();
		Camera = new Camera();
		Controller = new MovementController();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Controller?.Simulate( cl, this, Animator );
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl, this, Animator );
	}

	[Event.PreRender]
	public void OnPreRender()
	{
		float alpha = GetAlpha();
		this.RenderColor = new Color( 1, 1, 1, alpha );
	}

	//
	// This controls the alpha/transparency of the player.
	// It's mainly used for transitions between the two
	// camera modes.
	//
	private float GetAlpha()
	{
		if ( Camera is Camera camera )
			return camera.GetPlayerAlpha();

		return 1.0f;
	}
}
